using System.Data;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Distributed Database (BDD) controller for Module 3 / MODBD.
/// Aligned with sql/modbd/*.sql (XEPDB1, schemas ARTGALLERY_AM / ARTGALLERY_EU / ARTGALLERY_GLOBAL).
///
/// Cerinta 1: local CRUD on AM/EU stations.
/// Cerinta 2: read-only unified view on GLOBAL (uses GLOBAL_ARTWORK / GLOBAL_EXHIBITOR + UNION ALL via @link_*).
/// Cerinta 3: a local LMD on AM or EU is observable through GLOBAL.
/// Cerinta 4: a "global" LMD is routed by the controller to the correct fragment(s),
///            because the new schema does NOT define INSTEAD OF triggers.
/// </summary>
[ApiController]
[Route("api/bdd")]
[Produces("application/json")]
public class BddController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BddController> _logger;

    public BddController(IConfiguration configuration, ILogger<BddController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // =========================================================================
    // Connection helpers
    // =========================================================================

    private string GetConnString(string station)
    {
        var key = station.ToUpperInvariant() switch
        {
            "AM"     => "BddAmConnection",
            "EU"     => "BddEuConnection",
            "GLOBAL" => "BddGlobalConnection",
            _        => throw new ArgumentException($"Unknown station '{station}'. Valid: AM, EU, GLOBAL.")
        };
        var cs = _configuration.GetConnectionString(key);
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException($"Connection string '{key}' is not configured.");
        return cs;
    }

    private OracleConnection OpenConnection(string station)
    {
        var conn = new OracleConnection(GetConnString(station));
        conn.Open();
        return conn;
    }

    // -------------------------------------------------------------------------
    // Entity sets per station (must match sql/modbd/*.sql)
    // -------------------------------------------------------------------------

    // AM-only entities (horizontally fragmented + AM-side replicated dim + vertical AM half)
    private static readonly Dictionary<string, string> AmEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        { "EXHIBITOR",          "EXHIBITOR_AM" },
        { "EXHIBITION",         "EXHIBITION_AM" },
        { "LOAN",               "LOAN_AM" },
        { "GALLERY_REVIEW",     "GALLERY_REVIEW_AM" },
        { "ARTWORK_EXHIBITION", "ARTWORK_EXHIBITION_AM" },
        { "ARTIST",             "ARTIST_AM" },
        { "COLLECTION",         "COLLECTION_AM" },
        { "ARTWORK_DETAILS",    "ARTWORK_DETAILS" }, // vertical AM-half
    };

    private static readonly Dictionary<string, string> EuEntities = new(StringComparer.OrdinalIgnoreCase)
    {
        { "EXHIBITOR",          "EXHIBITOR_EU" },
        { "EXHIBITION",         "EXHIBITION_EU" },
        { "LOAN",               "LOAN_EU" },
        { "GALLERY_REVIEW",     "GALLERY_REVIEW_EU" },
        { "ARTWORK_EXHIBITION", "ARTWORK_EXHIBITION_EU" },
        { "ARTIST",             "ARTIST_EU" },
        { "COLLECTION",         "COLLECTION_EU" },
        { "ARTWORK_CORE",       "ARTWORK_CORE" },    // vertical EU-half
    };

    // GLOBAL-only base tables (live physically in ARTGALLERY_GLOBAL)
    private static readonly HashSet<string> GlobalOnlyTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "LOCATION", "VISITOR", "STAFF", "INSURANCE_POLICY",
        "INSURANCE", "RESTORATION", "ACQUISITION"
    };

    private static string ResolveLocalTable(string station, string entity)
    {
        var dict = station.ToUpperInvariant() switch
        {
            "AM" => AmEntities,
            "EU" => EuEntities,
            _    => throw new ArgumentException($"Local CRUD requires station AM or EU, got '{station}'.")
        };
        if (!dict.TryGetValue(entity, out var table))
            throw new ArgumentException($"Entity '{entity}' is not defined on station '{station}'. " +
                                        $"Valid: {string.Join(", ", dict.Keys)}.");
        return table;
    }

    /// <summary>
    /// Returns a SELECT statement that exposes the entity at "global" scope.
    /// Uses the two transparency views (GLOBAL_ARTWORK, GLOBAL_EXHIBITOR) where defined,
    /// otherwise builds a UNION ALL or queries the GLOBAL-only table directly.
    /// </summary>
    private static string ResolveGlobalSelect(string entity)
    {
        var e = entity.ToUpperInvariant();
        return e switch
        {
            "ARTWORK"            => "SELECT * FROM GLOBAL_ARTWORK",
            "EXHIBITOR"          => "SELECT * FROM GLOBAL_EXHIBITOR",
            "EXHIBITION"         => "SELECT * FROM EXHIBITION_AM@link_am UNION ALL SELECT * FROM EXHIBITION_EU@link_eu",
            "LOAN"               => "SELECT * FROM LOAN_AM@link_am UNION ALL SELECT * FROM LOAN_EU@link_eu",
            "GALLERY_REVIEW"     => "SELECT * FROM GALLERY_REVIEW_AM@link_am UNION ALL SELECT * FROM GALLERY_REVIEW_EU@link_eu",
            "ARTWORK_EXHIBITION" => "SELECT * FROM ARTWORK_EXHIBITION_AM@link_am UNION ALL SELECT * FROM ARTWORK_EXHIBITION_EU@link_eu",
            "ARTIST"             => "SELECT * FROM ARTIST_AM@link_am UNION ALL SELECT * FROM ARTIST_EU@link_eu",
            "COLLECTION"         => "SELECT * FROM COLLECTION_AM@link_am UNION ALL SELECT * FROM COLLECTION_EU@link_eu",
            _ when GlobalOnlyTables.Contains(e) => $"SELECT * FROM {e}",
            _ => throw new ArgumentException($"Unknown global entity '{entity}'.")
        };
    }

    private static List<Dictionary<string, object?>> ReadAll(OracleDataReader reader)
    {
        var rows = new List<Dictionary<string, object?>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    private static Dictionary<string, object?> Pick(Dictionary<string, object?> src, params string[] keys)
    {
        var dst = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var k in keys)
            if (src.TryGetValue(k, out var v)) dst[k] = v;
        return dst;
    }

    // =========================================================================
    // Cerinta 1 - Local CRUD (AM / EU)
    // =========================================================================

    [HttpGet("local/{station}/{entity}")]
    public async Task<ActionResult<ApiResponse<List<Dictionary<string, object?>>>>> GetLocal(
        string station, string entity, [FromQuery] int limit = 200)
    {
        try
        {
            var table = ResolveLocalTable(station, entity);
            await using var conn = OpenConnection(station);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {table} FETCH FIRST :lim ROWS ONLY";
            cmd.Parameters.Add(new OracleParameter("lim", limit));
            await using var reader = await cmd.ExecuteReaderAsync();
            var rows = ReadAll((OracleDataReader)reader);
            return Ok(ApiResponse<List<Dictionary<string, object?>>>.SuccessResponse(rows,
                $"{rows.Count} rows from {table}@{station.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD GetLocal failed: {Station}/{Entity}", station, entity);
            return BadRequest(ApiResponse<List<Dictionary<string, object?>>>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("local/{station}/{entity}")]
    public async Task<ActionResult<ApiResponse<int>>> InsertLocal(
        string station, string entity, [FromBody] Dictionary<string, object?> values)
    {
        if (values == null || values.Count == 0)
            return BadRequest(ApiResponse<int>.FailureResponse("Body must contain at least one column."));
        try
        {
            var n = await ExecuteLocalAction(station, entity, "insert", values, null, null);
            var table = ResolveLocalTable(station, entity);
            return Ok(ApiResponse<int>.SuccessResponse(n,
                $"Inserted {n} row(s) into {table}@{station.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD InsertLocal failed: {Station}/{Entity}", station, entity);
            return BadRequest(ApiResponse<int>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("local/{station}/{entity}")]
    public async Task<ActionResult<ApiResponse<int>>> UpdateLocal(
        string station, string entity,
        [FromQuery] string keyColumn, [FromQuery] string keyValue,
        [FromBody] Dictionary<string, object?> values)
    {
        if (string.IsNullOrWhiteSpace(keyColumn))
            return BadRequest(ApiResponse<int>.FailureResponse("'keyColumn' query param is required."));
        if (values == null || values.Count == 0)
            return BadRequest(ApiResponse<int>.FailureResponse("Body must contain at least one column."));
        try
        {
            var n = await ExecuteLocalAction(station, entity, "update", values, keyColumn, keyValue);
            var table = ResolveLocalTable(station, entity);
            return Ok(ApiResponse<int>.SuccessResponse(n,
                $"Updated {n} row(s) in {table}@{station.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD UpdateLocal failed: {Station}/{Entity}", station, entity);
            return BadRequest(ApiResponse<int>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("local/{station}/{entity}")]
    public async Task<ActionResult<ApiResponse<int>>> DeleteLocal(
        string station, string entity,
        [FromQuery] string keyColumn, [FromQuery] string keyValue)
    {
        if (string.IsNullOrWhiteSpace(keyColumn))
            return BadRequest(ApiResponse<int>.FailureResponse("'keyColumn' query param is required."));
        try
        {
            var n = await ExecuteLocalAction(station, entity, "delete", null, keyColumn, keyValue);
            var table = ResolveLocalTable(station, entity);
            return Ok(ApiResponse<int>.SuccessResponse(n,
                $"Deleted {n} row(s) from {table}@{station.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD DeleteLocal failed: {Station}/{Entity}", station, entity);
            return BadRequest(ApiResponse<int>.FailureResponse(ex.Message));
        }
    }

    // =========================================================================
    // Cerinta 2 - Global read (transparent)
    // =========================================================================

    [HttpGet("global/{entity}")]
    public async Task<ActionResult<ApiResponse<List<Dictionary<string, object?>>>>> GetGlobal(
        string entity, [FromQuery] int limit = 200)
    {
        try
        {
            var inner = ResolveGlobalSelect(entity);
            await using var conn = OpenConnection("GLOBAL");
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM ({inner}) FETCH FIRST :lim ROWS ONLY";
            cmd.Parameters.Add(new OracleParameter("lim", limit));
            await using var reader = await cmd.ExecuteReaderAsync();
            var rows = ReadAll((OracleDataReader)reader);
            return Ok(ApiResponse<List<Dictionary<string, object?>>>.SuccessResponse(rows,
                $"{rows.Count} rows from GLOBAL view of {entity.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD GetGlobal failed: {Entity}", entity);
            return BadRequest(ApiResponse<List<Dictionary<string, object?>>>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Cerinta 4 (write at "global" scope). Because there are NO INSTEAD OF triggers in the
    /// new schema, the controller routes the write to the correct fragment(s).
    ///
    /// Routing rules:
    ///   GLOBAL-only tables          -> GLOBAL connection.
    ///   ARTIST / COLLECTION         -> dual-write to AM and EU (replicated dimensions).
    ///   EXHIBITOR                   -> route by 'city' field: New York=>AM, Paris/London/Madrid=>EU.
    ///   ARTWORK                     -> split: CORE columns to ARTWORK_CORE@EU, DETAILS to ARTWORK_DETAILS@AM.
    ///   EXHIBITION / LOAN /
    ///   GALLERY_REVIEW /
    ///   ARTWORK_EXHIBITION          -> require explicit ?station=AM|EU.
    /// </summary>
    [HttpPost("global/{entity}")]
    public async Task<ActionResult<ApiResponse<object>>> InsertGlobal(
        string entity, [FromBody] Dictionary<string, object?> values,
        [FromQuery] string? station = null)
    {
        if (values == null || values.Count == 0)
            return BadRequest(ApiResponse<object>.FailureResponse("Body must contain at least one column."));
        try
        {
            var result = await RouteGlobalWrite(entity, "insert", values, null, null, station);
            return Ok(ApiResponse<object>.SuccessResponse(result,
                $"Global INSERT routed for {entity.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD InsertGlobal failed: {Entity}", entity);
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPut("global/{entity}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateGlobal(
        string entity,
        [FromQuery] string keyColumn, [FromQuery] string keyValue,
        [FromBody] Dictionary<string, object?> values,
        [FromQuery] string? station = null)
    {
        if (string.IsNullOrWhiteSpace(keyColumn))
            return BadRequest(ApiResponse<object>.FailureResponse("'keyColumn' query param is required."));
        if (values == null || values.Count == 0)
            return BadRequest(ApiResponse<object>.FailureResponse("Body must contain at least one column."));
        try
        {
            var result = await RouteGlobalWrite(entity, "update", values, keyColumn, keyValue, station);
            return Ok(ApiResponse<object>.SuccessResponse(result,
                $"Global UPDATE routed for {entity.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD UpdateGlobal failed: {Entity}", entity);
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpDelete("global/{entity}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteGlobal(
        string entity,
        [FromQuery] string keyColumn, [FromQuery] string keyValue,
        [FromQuery] string? station = null)
    {
        if (string.IsNullOrWhiteSpace(keyColumn))
            return BadRequest(ApiResponse<object>.FailureResponse("'keyColumn' query param is required."));
        try
        {
            var result = await RouteGlobalWrite(entity, "delete", null, keyColumn, keyValue, station);
            return Ok(ApiResponse<object>.SuccessResponse(result,
                $"Global DELETE routed for {entity.ToUpperInvariant()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD DeleteGlobal failed: {Entity}", entity);
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    // =========================================================================
    // Diagnostics
    // =========================================================================

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<object>>> Status()
    {
        var report = new Dictionary<string, object>();
        foreach (var st in new[] { "AM", "EU", "GLOBAL" })
        {
            try
            {
                await using var conn = OpenConnection(st);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT SYS_CONTEXT('USERENV','CURRENT_SCHEMA'), SYSDATE FROM DUAL";
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                    report[st] = new { ok = true, schema = reader.GetString(0), serverTime = reader.GetDateTime(1) };
            }
            catch (Exception ex)
            {
                report[st] = new { ok = false, error = ex.Message };
            }
        }
        return Ok(ApiResponse<object>.SuccessResponse(report, "BDD station status"));
    }

    // =========================================================================
    // Cerinta 3 - Local LMD -> visible globally (demo)
    // =========================================================================

    public class DemoScenarioRequest
    {
        public string? Scenario { get; set; }
        public string? Action   { get; set; } // insert | update | delete
        public Dictionary<string, object?>? Values { get; set; }
        public string? KeyColumn { get; set; }
        public string? KeyValue  { get; set; }
        public string? GlobalEntity { get; set; }
        public string? Station { get; set; } // optional override for routed scenarios
    }

    [HttpPost("demo/local-to-global")]
    public async Task<ActionResult<ApiResponse<object>>> LocalToGlobal([FromBody] DemoScenarioRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Scenario) || string.IsNullOrWhiteSpace(req.Action))
            return BadRequest(ApiResponse<object>.FailureResponse("'scenario' and 'action' are required."));
        try
        {
            var (station, localEntity, defaultGlobal) = req.Scenario.ToLowerInvariant() switch
            {
                "exhibitor_am"             => ("AM", "EXHIBITOR",          "EXHIBITOR"),
                "exhibitor_eu"             => ("EU", "EXHIBITOR",          "EXHIBITOR"),
                "exhibition_am"            => ("AM", "EXHIBITION",         "EXHIBITION"),
                "exhibition_eu"            => ("EU", "EXHIBITION",         "EXHIBITION"),
                "loan_am"                  => ("AM", "LOAN",               "LOAN"),
                "loan_eu"                  => ("EU", "LOAN",               "LOAN"),
                "gallery_review_am"        => ("AM", "GALLERY_REVIEW",     "GALLERY_REVIEW"),
                "gallery_review_eu"        => ("EU", "GALLERY_REVIEW",     "GALLERY_REVIEW"),
                "artwork_exhibition_am"    => ("AM", "ARTWORK_EXHIBITION", "ARTWORK_EXHIBITION"),
                "artwork_exhibition_eu"    => ("EU", "ARTWORK_EXHIBITION", "ARTWORK_EXHIBITION"),
                "artwork_core"             => ("EU", "ARTWORK_CORE",       "ARTWORK"),
                "artwork_details"          => ("AM", "ARTWORK_DETAILS",    "ARTWORK"),
                "artist_am"                => ("AM", "ARTIST",             "ARTIST"),
                "artist_eu"                => ("EU", "ARTIST",             "ARTIST"),
                "collection_am"            => ("AM", "COLLECTION",         "COLLECTION"),
                "collection_eu"            => ("EU", "COLLECTION",         "COLLECTION"),
                _ => throw new ArgumentException($"Unknown scenario '{req.Scenario}'.")
            };

            var globalEntity = req.GlobalEntity ?? defaultGlobal;

            var before = await SnapshotGlobal(globalEntity);
            var affected = await ExecuteLocalAction(station, localEntity, req.Action!, req.Values, req.KeyColumn, req.KeyValue);
            var after = await SnapshotGlobal(globalEntity);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                scenario = req.Scenario,
                action = req.Action,
                station,
                localEntity,
                globalEntity,
                affectedRows = affected,
                before,
                after
            }, "Cerinta 3 - local LMD visible at global scope"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD LocalToGlobal demo failed");
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    /// <summary>
    /// Cerinta 4: a "global" LMD is routed by the controller to the correct fragment(s).
    /// </summary>
    [HttpPost("demo/global-to-local")]
    public async Task<ActionResult<ApiResponse<object>>> GlobalToLocal([FromBody] DemoScenarioRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Scenario) || string.IsNullOrWhiteSpace(req.Action))
            return BadRequest(ApiResponse<object>.FailureResponse("'scenario' and 'action' are required."));
        try
        {
            // (globalEntity, list of (station, localTable) snapshots to capture before/after)
            var (globalEntity, fragmentTargets) = req.Scenario.ToLowerInvariant() switch
            {
                "horizontal_exhibitor"      => ("EXHIBITOR",          new[] { ("AM","EXHIBITOR_AM"),          ("EU","EXHIBITOR_EU") }),
                "horizontal_exhibition"     => ("EXHIBITION",         new[] { ("AM","EXHIBITION_AM"),         ("EU","EXHIBITION_EU") }),
                "horizontal_loan"           => ("LOAN",               new[] { ("AM","LOAN_AM"),               ("EU","LOAN_EU") }),
                "horizontal_review"         => ("GALLERY_REVIEW",     new[] { ("AM","GALLERY_REVIEW_AM"),     ("EU","GALLERY_REVIEW_EU") }),
                "horizontal_artwork_exh"    => ("ARTWORK_EXHIBITION", new[] { ("AM","ARTWORK_EXHIBITION_AM"), ("EU","ARTWORK_EXHIBITION_EU") }),
                "vertical_artwork"          => ("ARTWORK",            new[] { ("EU","ARTWORK_CORE"),          ("AM","ARTWORK_DETAILS") }),
                "replicated_artist"         => ("ARTIST",             new[] { ("AM","ARTIST_AM"),             ("EU","ARTIST_EU") }),
                "replicated_collection"     => ("COLLECTION",         new[] { ("AM","COLLECTION_AM"),         ("EU","COLLECTION_EU") }),
                "global_location"           => ("LOCATION",           new[] { ("GLOBAL","LOCATION") }),
                "global_visitor"            => ("VISITOR",            new[] { ("GLOBAL","VISITOR") }),
                "global_staff"              => ("STAFF",              new[] { ("GLOBAL","STAFF") }),
                "global_insurance_policy"   => ("INSURANCE_POLICY",   new[] { ("GLOBAL","INSURANCE_POLICY") }),
                "global_insurance"          => ("INSURANCE",          new[] { ("GLOBAL","INSURANCE") }),
                "global_restoration"        => ("RESTORATION",        new[] { ("GLOBAL","RESTORATION") }),
                "global_acquisition"        => ("ACQUISITION",        new[] { ("GLOBAL","ACQUISITION") }),
                _ => throw new ArgumentException($"Unknown scenario '{req.Scenario}'.")
            };

            var before = new Dictionary<string, object?>();
            foreach (var (st, table) in fragmentTargets)
                before[$"{st}.{table}"] = await SnapshotLocalRaw(st, table);

            var routing = await RouteGlobalWrite(globalEntity, req.Action!, req.Values, req.KeyColumn, req.KeyValue, req.Station);

            var after = new Dictionary<string, object?>();
            foreach (var (st, table) in fragmentTargets)
                after[$"{st}.{table}"] = await SnapshotLocalRaw(st, table);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                scenario = req.Scenario,
                action = req.Action,
                globalEntity,
                routing,
                before,
                after
            }, "Cerinta 4 - global LMD routed to fragment(s)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BDD GlobalToLocal demo failed");
            return BadRequest(ApiResponse<object>.FailureResponse(ex.Message));
        }
    }

    // =========================================================================
    // Internal helpers
    // =========================================================================

    private async Task<List<Dictionary<string, object?>>> SnapshotGlobal(string entity, int limit = 25)
    {
        var inner = ResolveGlobalSelect(entity);
        await using var conn = OpenConnection("GLOBAL");
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM ({inner}) FETCH FIRST :lim ROWS ONLY";
        cmd.Parameters.Add(new OracleParameter("lim", limit));
        await using var reader = await cmd.ExecuteReaderAsync();
        return ReadAll((OracleDataReader)reader);
    }

    private async Task<List<Dictionary<string, object?>>> SnapshotLocalRaw(string station, string table, int limit = 25)
    {
        await using var conn = OpenConnection(station);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM {table} FETCH FIRST :lim ROWS ONLY";
        cmd.Parameters.Add(new OracleParameter("lim", limit));
        await using var reader = await cmd.ExecuteReaderAsync();
        return ReadAll((OracleDataReader)reader);
    }

    private async Task<int> ExecuteLocalAction(string station, string entity, string action,
        Dictionary<string, object?>? values, string? keyColumn, string? keyValue)
    {
        var table = ResolveLocalTable(station, entity);
        await using var conn = OpenConnection(station);
        await using var cmd = conn.CreateCommand();
        cmd.BindByName = true;
        BuildDml(cmd, table, action, values, keyColumn, keyValue);
        return await cmd.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecuteOnTable(string station, string table, string action,
        Dictionary<string, object?>? values, string? keyColumn, string? keyValue)
    {
        await using var conn = OpenConnection(station);
        await using var cmd = conn.CreateCommand();
        cmd.BindByName = true;
        BuildDml(cmd, table, action, values, keyColumn, keyValue);
        return await cmd.ExecuteNonQueryAsync();
    }

    private static void BuildDml(OracleCommand cmd, string table, string action,
        Dictionary<string, object?>? values, string? keyColumn, string? keyValue)
    {
        switch (action.ToLowerInvariant())
        {
            case "insert":
                if (values == null || values.Count == 0) throw new ArgumentException("INSERT requires 'values'.");
                cmd.CommandText = $"INSERT INTO {table} ({string.Join(", ", values.Keys)}) " +
                                  $"VALUES ({string.Join(", ", values.Keys.Select(k => ":" + k))})";
                foreach (var kv in values) cmd.Parameters.Add(new OracleParameter(kv.Key, kv.Value ?? DBNull.Value));
                break;
            case "update":
                if (values == null || values.Count == 0) throw new ArgumentException("UPDATE requires 'values'.");
                if (string.IsNullOrWhiteSpace(keyColumn) || string.IsNullOrWhiteSpace(keyValue))
                    throw new ArgumentException("UPDATE requires 'keyColumn' and 'keyValue'.");
                cmd.CommandText = $"UPDATE {table} SET {string.Join(", ", values.Keys.Select(k => $"{k} = :{k}"))} " +
                                  $"WHERE {keyColumn} = :__pk";
                foreach (var kv in values) cmd.Parameters.Add(new OracleParameter(kv.Key, kv.Value ?? DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("__pk", keyValue));
                break;
            case "delete":
                if (string.IsNullOrWhiteSpace(keyColumn) || string.IsNullOrWhiteSpace(keyValue))
                    throw new ArgumentException("DELETE requires 'keyColumn' and 'keyValue'.");
                cmd.CommandText = $"DELETE FROM {table} WHERE {keyColumn} = :__pk";
                cmd.Parameters.Add(new OracleParameter("__pk", keyValue));
                break;
            default:
                throw new ArgumentException($"Unsupported action '{action}'.");
        }
    }

    /// <summary>
    /// Decides which physical table(s) on which station(s) receive the write,
    /// then performs them and returns a per-target affected-rows report.
    /// </summary>
    private async Task<object> RouteGlobalWrite(string entity, string action,
        Dictionary<string, object?>? values, string? keyColumn, string? keyValue, string? stationOverride)
    {
        var e = entity.ToUpperInvariant();
        var report = new List<object>();

        // ---- GLOBAL-only base tables ----
        if (GlobalOnlyTables.Contains(e))
        {
            var n = await ExecuteOnTable("GLOBAL", e, action, values, keyColumn, keyValue);
            report.Add(new { station = "GLOBAL", table = e, affected = n });
            return new { strategy = "global-only", targets = report };
        }

        // ---- ARTIST / COLLECTION: replicated dimension, dual write ----
        if (e == "ARTIST" || e == "COLLECTION")
        {
            var amTable = AmEntities[e];
            var euTable = EuEntities[e];
            var nAm = await ExecuteOnTable("AM", amTable, action, values, keyColumn, keyValue);
            var nEu = await ExecuteOnTable("EU", euTable, action, values, keyColumn, keyValue);
            report.Add(new { station = "AM", table = amTable, affected = nAm });
            report.Add(new { station = "EU", table = euTable, affected = nEu });
            return new { strategy = "replicated-dual-write", targets = report };
        }

        // ---- EXHIBITOR: route by city (or use stationOverride) ----
        if (e == "EXHIBITOR")
        {
            string? targetStation = stationOverride?.ToUpperInvariant();
            if (string.IsNullOrEmpty(targetStation))
            {
                if (action.Equals("insert", StringComparison.OrdinalIgnoreCase))
                {
                    if (values == null || !values.TryGetValue("city", out var cityObj) || cityObj == null)
                        throw new ArgumentException("EXHIBITOR INSERT requires either ?station=AM|EU or a 'city' field.");
                    var city = cityObj.ToString()!.Trim();
                    targetStation = city.Equals("New York", StringComparison.OrdinalIgnoreCase)
                        ? "AM"
                        : (city.Equals("Paris", StringComparison.OrdinalIgnoreCase) ||
                           city.Equals("London", StringComparison.OrdinalIgnoreCase) ||
                           city.Equals("Madrid", StringComparison.OrdinalIgnoreCase))
                            ? "EU"
                            : throw new ArgumentException(
                                $"Cannot infer fragment from city '{city}'. Provide ?station=AM|EU.");
                }
                else
                {
                    throw new ArgumentException("EXHIBITOR UPDATE/DELETE requires ?station=AM|EU.");
                }
            }
            var table = targetStation == "AM" ? "EXHIBITOR_AM" : "EXHIBITOR_EU";
            var n = await ExecuteOnTable(targetStation, table, action, values, keyColumn, keyValue);
            report.Add(new { station = targetStation, table, affected = n });
            return new { strategy = "horizontal-by-city", targets = report };
        }

        // ---- ARTWORK: vertical split (CORE -> EU, DETAILS -> AM) ----
        if (e == "ARTWORK")
        {
            if (action.Equals("insert", StringComparison.OrdinalIgnoreCase))
            {
                if (values == null) throw new ArgumentException("ARTWORK INSERT requires values.");
                var core = Pick(values, "artwork_id", "title", "artist_id", "year_created", "medium", "collection_id");
                var details = Pick(values, "artwork_id", "location_id", "estimated_value");
                if (!core.ContainsKey("artwork_id") || !details.ContainsKey("artwork_id"))
                    throw new ArgumentException("ARTWORK INSERT requires 'artwork_id'.");
                var nEu = await ExecuteOnTable("EU", "ARTWORK_CORE",    "insert", core,    null, null);
                var nAm = await ExecuteOnTable("AM", "ARTWORK_DETAILS", "insert", details, null, null);
                report.Add(new { station = "EU", table = "ARTWORK_CORE",    affected = nEu });
                report.Add(new { station = "AM", table = "ARTWORK_DETAILS", affected = nAm });
                return new { strategy = "vertical-split", targets = report };
            }
            if (action.Equals("update", StringComparison.OrdinalIgnoreCase))
            {
                if (values == null) throw new ArgumentException("ARTWORK UPDATE requires values.");
                var core = Pick(values, "title", "artist_id", "year_created", "medium", "collection_id");
                var details = Pick(values, "location_id", "estimated_value");
                if (core.Count > 0)
                {
                    var nEu = await ExecuteOnTable("EU", "ARTWORK_CORE", "update", core, keyColumn, keyValue);
                    report.Add(new { station = "EU", table = "ARTWORK_CORE", affected = nEu });
                }
                if (details.Count > 0)
                {
                    var nAm = await ExecuteOnTable("AM", "ARTWORK_DETAILS", "update", details, keyColumn, keyValue);
                    report.Add(new { station = "AM", table = "ARTWORK_DETAILS", affected = nAm });
                }
                if (report.Count == 0) throw new ArgumentException("ARTWORK UPDATE: no CORE/DETAILS columns provided.");
                return new { strategy = "vertical-split", targets = report };
            }
            if (action.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                // Trigger TRG_VFRAG_ARTWORK_DELETE on ARTWORK_CORE cascades into ARTWORK_DETAILS@link_am.
                var nEu = await ExecuteOnTable("EU", "ARTWORK_CORE", "delete", null, keyColumn, keyValue);
                report.Add(new { station = "EU", table = "ARTWORK_CORE", affected = nEu, note = "ARTWORK_DETAILS cascaded by TRG_VFRAG_ARTWORK_DELETE" });
                return new { strategy = "vertical-cascade-via-trigger", targets = report };
            }
        }

        // ---- Horizontal-by-explicit-station entities ----
        if (e == "EXHIBITION" || e == "LOAN" || e == "GALLERY_REVIEW" || e == "ARTWORK_EXHIBITION")
        {
            var st = stationOverride?.ToUpperInvariant();
            if (string.IsNullOrEmpty(st) || (st != "AM" && st != "EU"))
                throw new ArgumentException($"{e} global write requires ?station=AM|EU.");
            var dict = st == "AM" ? AmEntities : EuEntities;
            var table = dict[e];
            var n = await ExecuteOnTable(st, table, action, values, keyColumn, keyValue);
            report.Add(new { station = st, table, affected = n });
            return new { strategy = "horizontal-by-station", targets = report };
        }

        throw new ArgumentException($"Unsupported global entity '{entity}'.");
    }
}
