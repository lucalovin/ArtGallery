using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Insurance;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class InsuranceService : IInsuranceService
{
    private readonly IRepository<Insurance> _repository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public InsuranceService(
        IRepository<Insurance> repository,
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

    public async Task<PaginatedResponse<InsuranceResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (ShouldUseGlobalConnection())
        {
            return await GetAllFromGlobalAsync(request);
        }

        IQueryable<Insurance> query = _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy);

        query = request.SortBy?.ToLower() switch
        {
            "provider" => request.IsDescending
                ? query.OrderByDescending(i => i.Policy!.Provider)
                : query.OrderBy(i => i.Policy!.Provider),

            "insuredamount" => request.IsDescending
                ? query.OrderByDescending(i => i.InsuredAmount)
                : query.OrderBy(i => i.InsuredAmount),

            _ => query.OrderBy(i => i.Id)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<InsuranceResponseDto>>(items);

        return PaginatedResponse<InsuranceResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<InsuranceResponseDto>> GetAllFromGlobalAsync(PagedRequest request)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return PaginatedResponse<InsuranceResponseDto>.Create(
                new List<InsuranceResponseDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        var items = new List<Insurance>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                i.INSURANCE_ID,
                i.ARTWORK_ID,
                i.POLICY_ID,
                i.INSURED_AMOUNT,

                p.PROVIDER,
                p.START_DATE,
                p.END_DATE,
                p.TOTAL_COVERAGE_AMOUNT,

                c.TITLE AS ARTWORK_TITLE,
                c.ARTIST_ID,
                c.YEAR_CREATED,
                c.MEDIUM,
                c.COLLECTION_ID
            FROM INSURANCE i
            LEFT JOIN INSURANCE_POLICY p
                ON p.POLICY_ID = i.POLICY_ID
            LEFT JOIN ARTWORK_CORE@link_eu c
                ON c.ARTWORK_ID = i.ARTWORK_ID
            ORDER BY i.INSURANCE_ID";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var insuranceId = Convert.ToInt32(reader["INSURANCE_ID"]);

            var artworkId = reader["ARTWORK_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTWORK_ID"]);

            var policyId = reader["POLICY_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["POLICY_ID"]);

            var artistId = reader["ARTIST_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTIST_ID"]);

            int? collectionId = reader["COLLECTION_ID"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["COLLECTION_ID"]);

            var insurance = new Insurance
            {
                Id = insuranceId,

                ArtworkId = artworkId,

                PolicyId = policyId,

                InsuredAmount = reader["INSURED_AMOUNT"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(reader["INSURED_AMOUNT"]),

                Artwork = artworkId == 0
                    ? null
                    : new Artwork
                    {
                        Id = artworkId,

                        Title = reader["ARTWORK_TITLE"] == DBNull.Value
                            ? $"Artwork #{artworkId}"
                            : reader["ARTWORK_TITLE"]?.ToString() ?? $"Artwork #{artworkId}",

                        ArtistId = artistId,

                        YearCreated = reader["YEAR_CREATED"] == DBNull.Value
                            ? null
                            : Convert.ToInt32(reader["YEAR_CREATED"]),

                        Medium = reader["MEDIUM"] == DBNull.Value
                            ? null
                            : reader["MEDIUM"]?.ToString(),

                        CollectionId = collectionId
                    },

                Policy = policyId == 0
                    ? null
                    : new InsurancePolicy
                    {
                        Id = policyId,

                        Provider = reader["PROVIDER"] == DBNull.Value
                            ? "Unknown"
                            : reader["PROVIDER"]?.ToString() ?? "Unknown",

                        StartDate = reader["START_DATE"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(reader["START_DATE"]),

                        EndDate = reader["END_DATE"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(reader["END_DATE"]),

                        TotalCoverageAmount = reader["TOTAL_COVERAGE_AMOUNT"] == DBNull.Value
                            ? 0
                            : Convert.ToDecimal(reader["TOTAL_COVERAGE_AMOUNT"])
                    }
            };

            items.Add(insurance);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLowerInvariant();

            items = items
                .Where(i =>
                    (i.Artwork != null &&
                     !string.IsNullOrWhiteSpace(i.Artwork.Title) &&
                     i.Artwork.Title.ToLowerInvariant().Contains(searchTerm)) ||
                    (i.Policy != null &&
                     !string.IsNullOrWhiteSpace(i.Policy.Provider) &&
                     i.Policy.Provider.ToLowerInvariant().Contains(searchTerm)))
                .ToList();
        }

        items = request.SortBy?.ToLowerInvariant() switch
        {
            "provider" => request.IsDescending
                ? items.OrderByDescending(i => i.Policy?.Provider).ToList()
                : items.OrderBy(i => i.Policy?.Provider).ToList(),

            "insuredamount" => request.IsDescending
                ? items.OrderByDescending(i => i.InsuredAmount).ToList()
                : items.OrderBy(i => i.InsuredAmount).ToList(),

            "artwork" => request.IsDescending
                ? items.OrderByDescending(i => i.Artwork?.Title).ToList()
                : items.OrderBy(i => i.Artwork?.Title).ToList(),

            _ => items.OrderBy(i => i.Id).ToList()
        };

        var totalCount = items.Count;

        var pagedItems = items
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<InsuranceResponseDto>>(pagedItems);

        return PaginatedResponse<InsuranceResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<InsuranceResponseDto?> GetByIdAsync(int id)
    {
        if (ShouldUseGlobalConnection())
        {
            var result = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return result.Items.FirstOrDefault(i => i.Id == id);
        }

        var insurance = await _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy)
            .FirstOrDefaultAsync(i => i.Id == id);

        return insurance == null ? null : _mapper.Map<InsuranceResponseDto>(insurance);
    }

    public async Task<InsuranceResponseDto> CreateAsync(CreateInsuranceDto dto)
    {
        var insurance = _mapper.Map<Insurance>(dto);

        await _repository.AddAsync(insurance);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InsuranceResponseDto>(insurance);
    }

    public async Task<InsuranceResponseDto> UpdateAsync(int id, UpdateInsuranceDto dto)
    {
        var insurance = await _repository.Query()
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException(nameof(Insurance), id);

        if (dto.ArtworkId.HasValue) insurance.ArtworkId = dto.ArtworkId.Value;
        if (dto.PolicyId.HasValue) insurance.PolicyId = dto.PolicyId.Value;
        if (dto.InsuredAmount.HasValue) insurance.InsuredAmount = dto.InsuredAmount.Value;

        _repository.Update(insurance);
        await _repository.SaveChangesAsync();

        return _mapper.Map<InsuranceResponseDto>(insurance);
    }

    public async Task DeleteAsync(int id)
    {
        var insurance = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Insurance), id);

        _repository.Delete(insurance);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<InsuranceResponseDto>> GetActiveAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items.ToList();
        }

        var today = DateTime.UtcNow.Date;

        var insurances = await _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy)
            .Where(i => i.Policy != null && i.Policy.EndDate >= today)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InsuranceResponseDto>>(insurances);
    }

    public async Task<IEnumerable<InsuranceResponseDto>> GetExpiringAsync(int daysThreshold = 30)
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items.ToList();
        }

        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold).Date;
        var today = DateTime.UtcNow.Date;

        var insurances = await _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy)
            .Where(i => i.Policy != null &&
                        i.Policy.EndDate >= today &&
                        i.Policy.EndDate <= thresholdDate)
            .OrderBy(i => i.Policy!.EndDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<InsuranceResponseDto>>(insurances);
    }

    public async Task<InsuranceStatisticsDto> GetStatisticsAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            var items = all.Items.ToList();

            return new InsuranceStatisticsDto
            {
                TotalInsurances = items.Count,
                TotalPolicies = items
                    .Select(i => i.PolicyId)
                    .Distinct()
                    .Count(),
                TotalInsuredAmount = items.Sum(i => i.InsuredAmount),
                ByProvider = new Dictionary<string, int>()
            };
        }

        var insurances = await _repository.Query()
            .Include(i => i.Policy)
            .ToListAsync();

        return new InsuranceStatisticsDto
        {
            TotalInsurances = insurances.Count,
            TotalPolicies = insurances.Select(i => i.PolicyId).Distinct().Count(),
            TotalInsuredAmount = insurances.Sum(i => i.InsuredAmount),
            ByProvider = insurances
                .Where(i => i.Policy != null)
                .GroupBy(i => i.Policy!.Provider)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}