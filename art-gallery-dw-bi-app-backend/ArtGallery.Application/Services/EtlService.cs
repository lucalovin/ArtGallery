using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Etl;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;
using System.Diagnostics;

namespace ArtGallery.Application.Services;

public class EtlService : IEtlService
{
    private readonly IRepository<EtlSync> _repository;
    private readonly IEntityCountService _entityCountService;
    private readonly ICodeBasedEtlService _codeBasedEtlService;
    private readonly IMapper _mapper;

    public EtlService(
        IRepository<EtlSync> repository, 
        IEntityCountService entityCountService,
        ICodeBasedEtlService codeBasedEtlService,
        IMapper mapper)
    {
        _repository = repository;
        _entityCountService = entityCountService;
        _codeBasedEtlService = codeBasedEtlService;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<EtlSyncResponseDto>> GetSyncsAsync(PagedRequest request)
    {
        var query = _repository.Query();

        query = request.SortBy?.ToLower() switch
        {
            "syncdate" => request.IsDescending ? query.OrderByDescending(e => e.SyncDate) : query.OrderBy(e => e.SyncDate),
            "status" => request.IsDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            _ => query.OrderByDescending(e => e.SyncDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<EtlSyncResponseDto>>(items);

        return PaginatedResponse<EtlSyncResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<EtlSyncResponseDto?> GetSyncByIdAsync(int id)
    {
        var sync = await _repository.GetByIdAsync(id);
        return sync == null ? null : _mapper.Map<EtlSyncResponseDto>(sync);
    }

    public async Task<EtlSyncResponseDto> TriggerSyncAsync(TriggerSyncDto dto)
    {
        var stopwatch = Stopwatch.StartNew();

        var sync = new EtlSync
        {
            SyncDate = DateTime.UtcNow,
            Status = "Running",
            SyncType = dto.SyncType,
            SourceSystem = dto.SourceSystem,
            TargetSystem = dto.TargetSystem
        };

        await _repository.AddAsync(sync);
        await _repository.SaveChangesAsync();

        try
        {
            // Actually propagate data from OLTP to DW
            var fullLoad = dto.SyncType?.ToLower() == "full";
            var result = await _codeBasedEtlService.PropagateAllAsync(fullLoad);

            stopwatch.Stop();
            sync.Status = result.Success ? "Completed" : "Failed";
            sync.RecordsProcessed = result.TotalRecordsProcessed;
            sync.RecordsFailed = result.Success ? 0 : 1;
            sync.Duration = stopwatch.ElapsedMilliseconds;
            sync.Details = result.Success 
                ? $"Successfully synced {result.TotalRecordsProcessed} records (Artists: {result.ArtistsProcessed}, Artworks: {result.ArtworksProcessed}, Exhibitions: {result.ExhibitionsProcessed}, Visitors: {result.VisitorsProcessed}, Staff: {result.StaffProcessed}, Facts: {result.FactRecordsProcessed})"
                : result.ErrorMessage;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            sync.Status = "Failed";
            sync.Duration = stopwatch.ElapsedMilliseconds;
            sync.ErrorMessage = ex.Message;
        }

        _repository.Update(sync);
        await _repository.SaveChangesAsync();

        return _mapper.Map<EtlSyncResponseDto>(sync);
    }

    public async Task<EtlStatusDto> GetStatusAsync()
    {
        var syncs = await _repository.Query().ToListAsync();
        var lastSync = syncs.OrderByDescending(s => s.SyncDate).FirstOrDefault();
        var isRunning = syncs.Any(s => s.Status == "Running");

        // Get record counts from actual database tables
        var dataSources = new List<EtlDataSourceDto>
        {
            new EtlDataSourceDto 
            { 
                Id = "artworks", 
                Name = "Artworks", 
                Status = "connected",
                RecordCount = await _entityCountService.GetArtworkCountAsync()
            },
            new EtlDataSourceDto 
            { 
                Id = "exhibitions", 
                Name = "Exhibitions", 
                Status = "connected",
                RecordCount = await _entityCountService.GetExhibitionCountAsync()
            },
            new EtlDataSourceDto 
            { 
                Id = "visitors", 
                Name = "Visitors", 
                Status = "connected",
                RecordCount = await _entityCountService.GetVisitorCountAsync()
            },
            new EtlDataSourceDto 
            { 
                Id = "staff", 
                Name = "Staff", 
                Status = "connected",
                RecordCount = await _entityCountService.GetStaffCountAsync()
            },
            new EtlDataSourceDto 
            { 
                Id = "loans", 
                Name = "Loans", 
                Status = "connected",
                RecordCount = await _entityCountService.GetLoanCountAsync()
            }
        };

        var totalRecords = dataSources.Sum(ds => ds.RecordCount);
        var successfulSyncs = syncs.Count(s => s.Status == "Completed");
        var successRate = syncs.Count > 0 ? (double)successfulSyncs / syncs.Count * 100 : 100;

        return new EtlStatusDto
        {
            IsRunning = isRunning,
            Status = isRunning ? "running" : "idle",
            LastSync = lastSync != null ? _mapper.Map<EtlSyncResponseDto>(lastSync) : null,
            TotalSyncs = syncs.Count,
            SuccessfulSyncs = successfulSyncs,
            FailedSyncs = syncs.Count(s => s.Status == "Failed"),
            SuccessRate = successRate,
            DataSources = dataSources,
            Stats = new EtlStatsDto
            {
                TotalRecords = totalRecords,
                Duration = lastSync != null ? $"{lastSync.Duration}ms" : "N/A",
                SuccessRate = successRate,
                FailedRecords = syncs.Sum(s => s.RecordsFailed)
            }
        };
    }

    public Task<IEnumerable<EtlMappingDto>> GetMappingsAsync()
    {
        // Return default mappings for the DW schema
        var mappings = new List<EtlMappingDto>
        {
            new EtlMappingDto
            {
                SourceTable = "Artworks",
                TargetTable = "DimArtwork",
                FieldMappings = new List<FieldMappingDto>
                {
                    new FieldMappingDto { SourceField = "Id", TargetField = "ArtworkKey" },
                    new FieldMappingDto { SourceField = "Title", TargetField = "Title" },
                    new FieldMappingDto { SourceField = "Artist", TargetField = "Artist" },
                    new FieldMappingDto { SourceField = "Year", TargetField = "CreationYear" },
                    new FieldMappingDto { SourceField = "Collection", TargetField = "CollectionType" }
                }
            },
            new EtlMappingDto
            {
                SourceTable = "Visitors",
                TargetTable = "DimVisitor",
                FieldMappings = new List<FieldMappingDto>
                {
                    new FieldMappingDto { SourceField = "Id", TargetField = "VisitorKey" },
                    new FieldMappingDto { SourceField = "FirstName + LastName", TargetField = "FullName", Transformation = "CONCAT" },
                    new FieldMappingDto { SourceField = "MembershipType", TargetField = "MembershipLevel" }
                }
            }
        };

        return Task.FromResult<IEnumerable<EtlMappingDto>>(mappings);
    }

    public Task UpdateMappingsAsync(IEnumerable<EtlMappingDto> mappings)
    {
        // In a real implementation, this would persist the mappings
        return Task.CompletedTask;
    }

    public async Task<EtlStatisticsDto> GetStatisticsAsync()
    {
        var syncs = await _repository.Query().ToListAsync();

        return new EtlStatisticsDto
        {
            TotalSyncs = syncs.Count,
            SuccessfulSyncs = syncs.Count(s => s.Status == "Completed"),
            FailedSyncs = syncs.Count(s => s.Status == "Failed"),
            SuccessRate = syncs.Count > 0 ? (double)syncs.Count(s => s.Status == "Completed") / syncs.Count * 100 : 0,
            AverageDuration = syncs.Count > 0 ? (long)syncs.Average(s => s.Duration) : 0,
            TotalRecordsProcessed = syncs.Sum(s => s.RecordsProcessed),
            TotalRecordsFailed = syncs.Sum(s => s.RecordsFailed),
            BySyncType = syncs.GroupBy(s => s.SyncType)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public Task<bool> ValidateDataConsistencyAsync()
    {
        // In a real implementation, this would compare OLTP and DW data
        return Task.FromResult(true);
    }
}
