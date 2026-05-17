using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Loan;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class LoanService : ILoanService
{
    private readonly IRepository<Loan> _repository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public LoanService(
        IRepository<Loan> repository,
        IMapper mapper,
        IDataSourceContext ds,
        IConfiguration configuration)
    {
        _repository = repository;
        _mapper = mapper;
        _ds = ds;
        _configuration = configuration;
    }

    private bool IsGlobalSource()
    {
        return _ds.Source.ToString().Equals("GLOBAL", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PaginatedResponse<LoanResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (IsGlobalSource())
        {
            return await GetAllGlobalAsync(request);
        }

        IQueryable<Loan> query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();

            query = query.Where(l =>
                (l.Conditions != null && l.Conditions.ToLower().Contains(searchTerm)) ||
                l.ArtworkId.ToString().Contains(searchTerm) ||
                l.ExhibitorId.ToString().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "startdate" => request.IsDescending
                ? query.OrderByDescending(l => l.StartDate)
                : query.OrderBy(l => l.StartDate),

            "enddate" => request.IsDescending
                ? query.OrderByDescending(l => l.EndDate)
                : query.OrderBy(l => l.EndDate),

            "artwork" => request.IsDescending
                ? query.OrderByDescending(l => l.ArtworkId)
                : query.OrderBy(l => l.ArtworkId),

            "exhibitor" => request.IsDescending
                ? query.OrderByDescending(l => l.ExhibitorId)
                : query.OrderBy(l => l.ExhibitorId),

            _ => query.OrderByDescending(l => l.StartDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<LoanResponseDto>>(items);

        return PaginatedResponse<LoanResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<LoanResponseDto>> GetAllGlobalAsync(PagedRequest request)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return PaginatedResponse<LoanResponseDto>.Create(
                new List<LoanResponseDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        var loans = new List<Loan>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT
                l.LOAN_ID,
                l.ARTWORK_ID,
                l.EXHIBITOR_ID,
                l.START_DATE,
                l.END_DATE,
                l.CONDITIONS,
                l.SOURCE_REGION,

                c.TITLE AS ARTWORK_TITLE,
                c.ARTIST_ID,
                c.YEAR_CREATED,
                c.MEDIUM,

                a.NAME AS ARTIST_NAME,

                ex.NAME AS EXHIBITOR_NAME,
                ex.CITY AS EXHIBITOR_CITY
            FROM (
                SELECT
                    LOAN_ID,
                    ARTWORK_ID,
                    EXHIBITOR_ID,
                    START_DATE,
                    END_DATE,
                    CONDITIONS,
                    'AM' AS SOURCE_REGION
                FROM LOAN_AM@link_am

                UNION ALL

                SELECT
                    LOAN_ID,
                    ARTWORK_ID,
                    EXHIBITOR_ID,
                    START_DATE,
                    END_DATE,
                    CONDITIONS,
                    'EU' AS SOURCE_REGION
                FROM LOAN_EU@link_eu
            ) l
            LEFT JOIN ARTWORK_CORE@link_eu c
              ON c.ARTWORK_ID = l.ARTWORK_ID
            LEFT JOIN ARTIST_EU@link_eu a
              ON a.ARTIST_ID = c.ARTIST_ID
            LEFT JOIN (
                SELECT
                    EXHIBITOR_ID,
                    NAME,
                    CITY,
                    'AM' AS SOURCE_REGION
                FROM EXHIBITOR_AM@link_am

                UNION ALL

                SELECT
                    EXHIBITOR_ID,
                    NAME,
                    CITY,
                    'EU' AS SOURCE_REGION
                FROM EXHIBITOR_EU@link_eu
            ) ex
              ON ex.EXHIBITOR_ID = l.EXHIBITOR_ID
             AND ex.SOURCE_REGION = l.SOURCE_REGION
            ORDER BY l.START_DATE DESC, l.LOAN_ID";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var loanId = Convert.ToInt32(reader["LOAN_ID"]);

            var artworkId = reader["ARTWORK_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTWORK_ID"]);

            var exhibitorId = reader["EXHIBITOR_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["EXHIBITOR_ID"]);

            var artistId = reader["ARTIST_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTIST_ID"]);

            var loan = new Loan
            {
                Id = loanId,

                ArtworkId = artworkId,

                ExhibitorId = exhibitorId,

                StartDate = reader["START_DATE"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(reader["START_DATE"]),

                EndDate = reader["END_DATE"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["END_DATE"]),

                Conditions = reader["CONDITIONS"] == DBNull.Value
                    ? null
                    : reader["CONDITIONS"]?.ToString(),

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

                        Artist = artistId == 0
                            ? null
                            : new Artist
                            {
                                Id = artistId,
                                Name = reader["ARTIST_NAME"] == DBNull.Value
                                    ? $"Artist #{artistId}"
                                    : reader["ARTIST_NAME"]?.ToString() ?? $"Artist #{artistId}"
                            }
                    },

                Exhibitor = exhibitorId == 0
                    ? null
                    : new Exhibitor
                    {
                        Id = exhibitorId,
                        Name = reader["EXHIBITOR_NAME"] == DBNull.Value
                            ? $"Exhibitor #{exhibitorId}"
                            : reader["EXHIBITOR_NAME"]?.ToString() ?? $"Exhibitor #{exhibitorId}",
                        City = reader["EXHIBITOR_CITY"] == DBNull.Value
                            ? null
                            : reader["EXHIBITOR_CITY"]?.ToString()
                    }
            };

            loans.Add(loan);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLowerInvariant();

            loans = loans
                .Where(l =>
                    (l.Conditions != null &&
                     l.Conditions.ToLowerInvariant().Contains(searchTerm)) ||
                    (l.Artwork != null &&
                     l.Artwork.Title.ToLowerInvariant().Contains(searchTerm)) ||
                    (l.Artwork?.Artist != null &&
                     l.Artwork.Artist.Name.ToLowerInvariant().Contains(searchTerm)) ||
                    (l.Exhibitor != null &&
                     l.Exhibitor.Name.ToLowerInvariant().Contains(searchTerm)))
                .ToList();
        }

        loans = request.SortBy?.ToLowerInvariant() switch
        {
            "startdate" => request.IsDescending
                ? loans.OrderByDescending(l => l.StartDate).ToList()
                : loans.OrderBy(l => l.StartDate).ToList(),

            "enddate" => request.IsDescending
                ? loans.OrderByDescending(l => l.EndDate).ToList()
                : loans.OrderBy(l => l.EndDate).ToList(),

            "artwork" => request.IsDescending
                ? loans.OrderByDescending(l => l.Artwork?.Title).ToList()
                : loans.OrderBy(l => l.Artwork?.Title).ToList(),

            "exhibitor" => request.IsDescending
                ? loans.OrderByDescending(l => l.Exhibitor?.Name).ToList()
                : loans.OrderBy(l => l.Exhibitor?.Name).ToList(),

            _ => loans.OrderByDescending(l => l.StartDate).ToList()
        };

        var totalCount = loans.Count;

        var pagedItems = loans
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<LoanResponseDto>>(pagedItems);

        return PaginatedResponse<LoanResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<LoanResponseDto?> GetByIdAsync(int id)
    {
        if (IsGlobalSource())
        {
            var all = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items.FirstOrDefault(l => l.Id == id);
        }

        var loan = await _repository.Query()
            .FirstOrDefaultAsync(l => l.Id == id);

        return loan == null ? null : _mapper.Map<LoanResponseDto>(loan);
    }

    public async Task<LoanResponseDto> CreateAsync(CreateLoanDto dto)
    {
        var loan = _mapper.Map<Loan>(dto);

        await _repository.AddAsync(loan);
        await _repository.SaveChangesAsync();

        return _mapper.Map<LoanResponseDto>(loan);
    }

    public async Task<LoanResponseDto> UpdateAsync(int id, UpdateLoanDto dto)
    {
        var loan = await _repository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException(nameof(Loan), id);

        if (dto.ArtworkId.HasValue) loan.ArtworkId = dto.ArtworkId.Value;
        if (dto.ExhibitorId.HasValue) loan.ExhibitorId = dto.ExhibitorId.Value;
        if (dto.StartDate.HasValue) loan.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) loan.EndDate = dto.EndDate;
        if (dto.Conditions != null) loan.Conditions = dto.Conditions;

        _repository.Update(loan);
        await _repository.SaveChangesAsync();

        return _mapper.Map<LoanResponseDto>(loan);
    }

    public async Task DeleteAsync(int id)
    {
        var loan = await _repository.Query()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException(nameof(Loan), id);

        _repository.Delete(loan);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<LoanResponseDto>> GetActiveAsync()
    {
        if (IsGlobalSource())
        {
            var all = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items
                .Where(l => l.EndDate == null || l.EndDate >= DateTime.UtcNow.Date)
                .ToList();
        }

        var today = DateTime.UtcNow.Date;

        var loans = await _repository.Query()
            .Where(l => l.EndDate == null || l.EndDate >= today)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
    }

    public async Task<IEnumerable<LoanResponseDto>> GetOverdueAsync()
    {
        if (IsGlobalSource())
        {
            var all = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items
                .Where(l => l.EndDate != null && l.EndDate < DateTime.UtcNow.Date)
                .ToList();
        }

        var today = DateTime.UtcNow.Date;

        var loans = await _repository.Query()
            .Where(l => l.EndDate != null && l.EndDate < today)
            .OrderByDescending(l => l.EndDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
    }

    public async Task<LoanStatisticsDto> GetStatisticsAsync()
    {
        if (IsGlobalSource())
        {
            var all = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            var loans = all.Items.ToList();
            var today = DateTime.UtcNow.Date;

            return new LoanStatisticsDto
            {
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.EndDate == null || l.EndDate >= today),
                ByExhibitor = loans
                    .Where(l => !string.IsNullOrWhiteSpace(l.ExhibitorName))
                    .GroupBy(l => l.ExhibitorName!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        var localLoans = await _repository.Query().ToListAsync();
        var localToday = DateTime.UtcNow.Date;

        return new LoanStatisticsDto
        {
            TotalLoans = localLoans.Count,
            ActiveLoans = localLoans.Count(l => l.EndDate == null || l.EndDate >= localToday),
            ByExhibitor = localLoans
                .GroupBy(l => $"Exhibitor #{l.ExhibitorId}")
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}