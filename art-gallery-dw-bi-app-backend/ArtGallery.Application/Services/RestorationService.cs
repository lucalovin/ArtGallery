using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Restoration;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class RestorationService : IRestorationService
{
    private readonly IRepository<Restoration> _repository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public RestorationService(
        IRepository<Restoration> repository,
        IMapper mapper,
        IDataSourceContext ds,
        IConfiguration configuration)
    {
        _repository = repository;
        _mapper = mapper;
        _ds = ds;
        _configuration = configuration;
    }

    private bool ShouldUseGlobalConnection()
    {
        var source = _ds.Source.ToString().ToUpperInvariant();
        return source is "AM" or "EU" or "GLOBAL";
    }

    public async Task<PaginatedResponse<RestorationResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (ShouldUseGlobalConnection())
        {
            return await GetAllFromGlobalAsync(request);
        }

        IQueryable<Restoration> query = _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff);

        query = request.SortBy?.ToLower() switch
        {
            "startdate" => request.IsDescending
                ? query.OrderByDescending(r => r.StartDate)
                : query.OrderBy(r => r.StartDate),

            "staff" => request.IsDescending
                ? query.OrderByDescending(r => r.Staff!.Name)
                : query.OrderBy(r => r.Staff!.Name),

            _ => query.OrderByDescending(r => r.StartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<RestorationResponseDto>>(items);

        return PaginatedResponse<RestorationResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<RestorationResponseDto>> GetAllFromGlobalAsync(PagedRequest request)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return PaginatedResponse<RestorationResponseDto>.Create(
                new List<RestorationResponseDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        var items = new List<Restoration>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                r.RESTORATION_ID,
                r.ARTWORK_ID,
                r.STAFF_ID,
                r.START_DATE,
                r.END_DATE,
                r.DESCRIPTION,
                s.NAME AS STAFF_NAME,
                s.ROLE AS STAFF_ROLE
            FROM RESTORATION r
            LEFT JOIN STAFF s
                ON s.STAFF_ID = r.STAFF_ID
            ORDER BY r.START_DATE DESC";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var staffId = reader["STAFF_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["STAFF_ID"]);

            var restoration = new Restoration
            {
                Id = Convert.ToInt32(reader["RESTORATION_ID"]),
                ArtworkId = Convert.ToInt32(reader["ARTWORK_ID"]),
                StaffId = staffId,
                StartDate = reader["START_DATE"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(reader["START_DATE"]),
                EndDate = reader["END_DATE"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["END_DATE"]),
                Description = reader["DESCRIPTION"]?.ToString(),
                Staff = staffId == 0
                    ? null
                    : new Staff
                    {
                        Id = staffId,
                        Name = reader["STAFF_NAME"]?.ToString() ?? "Unknown",
                        Role = reader["STAFF_ROLE"]?.ToString()
                    }
            };

            items.Add(restoration);
        }

        items = request.SortBy?.ToLower() switch
        {
            "startdate" => request.IsDescending
                ? items.OrderByDescending(r => r.StartDate).ToList()
                : items.OrderBy(r => r.StartDate).ToList(),

            "staff" => request.IsDescending
                ? items.OrderByDescending(r => r.Staff?.Name).ToList()
                : items.OrderBy(r => r.Staff?.Name).ToList(),

            _ => items.OrderByDescending(r => r.StartDate).ToList()
        };

        var totalCount = items.Count;

        var pagedItems = items
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<RestorationResponseDto>>(pagedItems);

        return PaginatedResponse<RestorationResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<RestorationResponseDto?> GetByIdAsync(int id)
    {
        if (ShouldUseGlobalConnection())
        {
            var result = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return result.Items.FirstOrDefault(r => r.Id == id);
        }

        var restoration = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .FirstOrDefaultAsync(r => r.Id == id);

        return restoration == null ? null : _mapper.Map<RestorationResponseDto>(restoration);
    }

    public async Task<RestorationResponseDto> CreateAsync(CreateRestorationDto dto)
    {
        var restoration = _mapper.Map<Restoration>(dto);

        await _repository.AddAsync(restoration);
        await _repository.SaveChangesAsync();

        var created = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .FirstAsync(r => r.Id == restoration.Id);

        return _mapper.Map<RestorationResponseDto>(created);
    }

    public async Task<RestorationResponseDto> UpdateAsync(int id, UpdateRestorationDto dto)
    {
        var restoration = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(Restoration), id);

        if (dto.ArtworkId.HasValue) restoration.ArtworkId = dto.ArtworkId.Value;
        if (dto.StaffId.HasValue) restoration.StaffId = dto.StaffId.Value;
        if (dto.StartDate.HasValue) restoration.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) restoration.EndDate = dto.EndDate;
        if (dto.Description != null) restoration.Description = dto.Description;

        _repository.Update(restoration);
        await _repository.SaveChangesAsync();

        return _mapper.Map<RestorationResponseDto>(restoration);
    }

    public async Task DeleteAsync(int id)
    {
        var restoration = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Restoration), id);

        _repository.Delete(restoration);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<RestorationResponseDto>> GetInProgressAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items
                .Where(r => r.EndDate == null)
                .ToList();
        }

        var restorations = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .Where(r => r.EndDate == null)
            .ToListAsync();

        return _mapper.Map<IEnumerable<RestorationResponseDto>>(restorations);
    }

    public async Task<IEnumerable<RestorationResponseDto>> GetCompletedAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items
                .Where(r => r.EndDate != null)
                .OrderByDescending(r => r.EndDate)
                .ToList();
        }

        var restorations = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .Where(r => r.EndDate != null)
            .OrderByDescending(r => r.EndDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<RestorationResponseDto>>(restorations);
    }

    public async Task<RestorationStatisticsDto> GetStatisticsAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return new RestorationStatisticsDto
            {
                TotalRestorations = all.Items.Count,
                InProgressRestorations = all.Items.Count(r => r.EndDate == null),
                CompletedRestorations = all.Items.Count(r => r.EndDate != null),
                ByStaff = all.Items
                    .Where(r => !string.IsNullOrWhiteSpace(r.StaffName))
                    .GroupBy(r => r.StaffName!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        var restorations = await _repository.Query()
            .Include(r => r.Staff)
            .ToListAsync();

        return new RestorationStatisticsDto
        {
            TotalRestorations = restorations.Count,
            InProgressRestorations = restorations.Count(r => r.EndDate == null),
            CompletedRestorations = restorations.Count(r => r.EndDate != null),
            ByStaff = restorations
                .Where(r => r.Staff != null)
                .GroupBy(r => r.Staff!.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}