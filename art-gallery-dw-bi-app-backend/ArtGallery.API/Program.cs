using System.Data;
using System.Data.Common;
using Serilog;
using Azure.Identity;
using AspNetCoreRateLimit;
using ArtGallery.API.Extensions;
using ArtGallery.API.Middleware;
using ArtGallery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Oracle.ManagedDataAccess.Client;

// Configure ODP.NET to not use wallet (fix for ORA-28365)
OracleConfiguration.WalletLocation = "";
OracleConfiguration.TnsAdmin = "";

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault (if enabled)
var keyVaultEnabled = builder.Configuration.GetValue<bool>("KeyVault:Enabled");
if (keyVaultEnabled)
{
    var keyVaultUrl = builder.Configuration["KeyVault:VaultUrl"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
        
        Log.Information("Azure Key Vault configured: {VaultUrl}", keyVaultUrl);
    }
}

// Configure Serilog (sinks configured in appsettings.json)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add custom services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapperConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddSwaggerConfiguration();

// Add rate limiting
builder.Services.AddRateLimitingConfiguration(builder.Configuration);

// Add response caching
builder.Services.AddCachingConfiguration(builder.Configuration);
builder.Services.AddResponseCaching();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Art Gallery API v1");
        c.RoutePrefix = "swagger";
    });
}

// CORS (must be before exception handling to ensure error responses include CORS headers)
app.UseCors("AllowVueApp");

// Global exception handling
app.UseExceptionHandling();

// Rate limiting (before other middleware)
app.UseIpRateLimiting();

// Request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Response caching
app.UseResponseCaching();

app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Database initialization (disabled for Oracle - use separate migration scripts)
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate", false))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // For Oracle, we typically don't use EF migrations
        // Instead, verify the connection is working
        await context.Database.CanConnectAsync();
        Log.Information("Successfully connected to OLTP Oracle database");
        
        var dwContext = services.GetRequiredService<DwDbContext>();
        await dwContext.Database.CanConnectAsync();
        Log.Information("Successfully connected to DW Oracle database");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while connecting to the database");
    }
}

if (builder.Configuration.GetValue<bool>("Database:ValidateSchema", false))
{
    await ValidateOltpSchemaAsync(app.Services, builder.Configuration);
}

Log.Information("Art Gallery API is starting...");
Log.Information("Oracle OLTP Connection: {Connection}", 
    MaskConnectionString(builder.Configuration.GetConnectionString("OltpConnection") ?? ""));
Log.Information("Oracle DW Connection: {Connection}", 
    MaskConnectionString(builder.Configuration.GetConnectionString("DwConnection") ?? ""));

app.Run();

// Helper method to mask sensitive parts of connection string
static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "[Not Configured]";
    
    // Mask password
    var masked = System.Text.RegularExpressions.Regex.Replace(
        connectionString, 
        @"Password=([^;]+)", 
        "Password=****",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    
    return masked;
}

static async Task ValidateOltpSchemaAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("SchemaValidation");
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var connection = context.Database.GetDbConnection();
    var defaultSchema = ResolveDefaultSchema(context, connection);

    if (string.IsNullOrWhiteSpace(defaultSchema))
    {
        logger.LogWarning("Skipped schema validation because the default schema could not be determined.");
        return;
    }

    var missingTables = new List<string>();

    try
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (string.IsNullOrWhiteSpace(tableName))
            {
                continue;
            }

            var tableSchema = (entityType.GetSchema() ?? defaultSchema).ToUpperInvariant();
            
            // Skip DW tables - only validate OLTP tables
            if (tableSchema == "ART_GALLERY_DW" || tableName.StartsWith("DIM_") || tableName.StartsWith("FACT_"))
            {
                continue;
            }
            
            var tableExists = await TableExistsAsync(connection, tableSchema, tableName);

            if (!tableExists)
            {
                missingTables.Add($"{tableSchema}.{tableName.ToUpperInvariant()}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to validate Oracle schema.");
        return;
    }
    finally
    {
        if (connection.State == ConnectionState.Open)
        {
            await connection.CloseAsync();
        }
    }

    if (missingTables.Count == 0)
    {
        logger.LogInformation("Oracle OLTP schema validation succeeded. All mapped tables are present under {Schema}.", defaultSchema);
    }
    else
    {
        var message = $"Oracle OLTP schema validation failed. Missing or inaccessible tables: {string.Join(", ", missingTables)}";
        logger.LogError(message);
        throw new InvalidOperationException(message);
    }
}

static async Task<bool> TableExistsAsync(DbConnection connection, string schema, string tableName)
{
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT COUNT(*) FROM ALL_TABLES WHERE OWNER = :p_owner AND TABLE_NAME = :p_table";

    var ownerParam = command.CreateParameter();
    ownerParam.ParameterName = "p_owner";
    ownerParam.Value = schema.ToUpperInvariant();
    command.Parameters.Add(ownerParam);

    var tableParam = command.CreateParameter();
    tableParam.ParameterName = "p_table";
    tableParam.Value = tableName.ToUpperInvariant();
    command.Parameters.Add(tableParam);

    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result) > 0;
}

static string? ResolveDefaultSchema(DbContext context, DbConnection connection)
{
    var modelSchema = context.Model.GetDefaultSchema();
    if (!string.IsNullOrWhiteSpace(modelSchema))
    {
        return modelSchema.ToUpperInvariant();
    }

    try
    {
        var builder = new OracleConnectionStringBuilder(connection.ConnectionString);
        if (!string.IsNullOrWhiteSpace(builder.UserID))
        {
            return builder.UserID.ToUpperInvariant();
        }
    }
    catch
    {
        // ignore parsing errors and continue to null
    }

    return null;
}
