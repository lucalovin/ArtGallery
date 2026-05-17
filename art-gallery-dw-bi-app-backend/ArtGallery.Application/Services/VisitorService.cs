using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Visitor;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class VisitorService : IVisitorService
{
    private readonly IRepository<Visitor> _repository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public VisitorService(
        IRepository<Visitor> repository,
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

    public async Task<PaginatedResponse<VisitorResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (ShouldUseGlobalConnection())
        {
            return await GetAllFromGlobalAsync(request);
        }

        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(v => v.Name.ToLower().Contains(searchTerm) ||
                                     (v.Email != null && v.Email.ToLower().Contains(searchTerm)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
            "email" => request.IsDescending ? query.OrderByDescending(v => v.Email) : query.OrderBy(v => v.Email),
            "membershiptype" => request.IsDescending ? query.OrderByDescending(v => v.MembershipType) : query.OrderBy(v => v.MembershipType),
            _ => query.OrderBy(v => v.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<VisitorResponseDto>>(items);

        return PaginatedResponse<VisitorResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<VisitorResponseDto>> GetAllFromGlobalAsync(PagedRequest request)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return PaginatedResponse<VisitorResponseDto>.Create(
                new List<VisitorResponseDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        var visitors = new List<Visitor>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                VISITOR_ID,
                NAME,
                EMAIL,
                PHONE,
                MEMBERSHIP_TYPE,
                JOIN_DATE
            FROM VISITOR";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            visitors.Add(new Visitor
            {
                Id = Convert.ToInt32(reader["VISITOR_ID"]),
                Name = reader["NAME"]?.ToString() ?? "Unknown",
                Email = reader["EMAIL"] == DBNull.Value ? null : reader["EMAIL"]?.ToString(),
                Phone = reader["PHONE"] == DBNull.Value ? null : reader["PHONE"]?.ToString(),
                MembershipType = reader["MEMBERSHIP_TYPE"] == DBNull.Value ? null : reader["MEMBERSHIP_TYPE"]?.ToString(),
                JoinDate = reader["JOIN_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["JOIN_DATE"])
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();

            visitors = visitors
                .Where(v => v.Name.ToLower().Contains(searchTerm) ||
                            (!string.IsNullOrWhiteSpace(v.Email) &&
                             v.Email.ToLower().Contains(searchTerm)))
                .ToList();
        }

        visitors = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending
                ? visitors.OrderByDescending(v => v.Name).ToList()
                : visitors.OrderBy(v => v.Name).ToList(),

            "email" => request.IsDescending
                ? visitors.OrderByDescending(v => v.Email).ToList()
                : visitors.OrderBy(v => v.Email).ToList(),

            "membershiptype" => request.IsDescending
                ? visitors.OrderByDescending(v => v.MembershipType).ToList()
                : visitors.OrderBy(v => v.MembershipType).ToList(),

            _ => visitors.OrderBy(v => v.Name).ToList()
        };

        var totalCount = visitors.Count;

        var pagedItems = visitors
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<VisitorResponseDto>>(pagedItems);

        return PaginatedResponse<VisitorResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<VisitorResponseDto?> GetByIdAsync(int id)
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items.FirstOrDefault(v => v.Id == id);
        }

        var visitor = await _repository.GetByIdAsync(id);
        return visitor == null ? null : _mapper.Map<VisitorResponseDto>(visitor);
    }

    public async Task<VisitorResponseDto> CreateAsync(CreateVisitorDto dto)
    {
        var visitor = _mapper.Map<Visitor>(dto);

        await _repository.AddAsync(visitor);
        await _repository.SaveChangesAsync();

        return _mapper.Map<VisitorResponseDto>(visitor);
    }

    public async Task<VisitorResponseDto> UpdateAsync(int id, UpdateVisitorDto dto)
    {
        var visitor = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Visitor), id);

        if (dto.Name != null) visitor.Name = dto.Name;
        if (dto.Email != null) visitor.Email = dto.Email;
        if (dto.Phone != null) visitor.Phone = dto.Phone;
        if (dto.MembershipType != null) visitor.MembershipType = dto.MembershipType;
        if (dto.JoinDate.HasValue) visitor.JoinDate = dto.JoinDate;

        _repository.Update(visitor);
        await _repository.SaveChangesAsync();

        return _mapper.Map<VisitorResponseDto>(visitor);
    }

    public async Task DeleteAsync(int id)
    {
        var visitor = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Visitor), id);

        _repository.Delete(visitor);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<VisitorResponseDto>> GetMembersAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items
                .Where(v => !string.IsNullOrWhiteSpace(v.MembershipType) &&
                            v.MembershipType != "None")
                .ToList();
        }

        var visitors = await _repository.FindAsync(v =>
            v.MembershipType != null &&
            v.MembershipType != "None");

        return _mapper.Map<IEnumerable<VisitorResponseDto>>(visitors);
    }

    public async Task<VisitorStatisticsDto> GetStatisticsAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            var visitors = all.Items.ToList();

            return new VisitorStatisticsDto
            {
                TotalVisitors = visitors.Count,
                TotalMembers = visitors.Count(v =>
                    !string.IsNullOrWhiteSpace(v.MembershipType) &&
                    v.MembershipType != "None"),
                ByMembershipType = visitors
                    .Where(v => !string.IsNullOrWhiteSpace(v.MembershipType))
                    .GroupBy(v => v.MembershipType!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        var localVisitors = await _repository.Query().ToListAsync();

        return new VisitorStatisticsDto
        {
            TotalVisitors = localVisitors.Count,
            TotalMembers = localVisitors.Count(v =>
                v.MembershipType != null &&
                v.MembershipType != "None"),
            ByMembershipType = localVisitors
                .Where(v => v.MembershipType != null)
                .GroupBy(v => v.MembershipType!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<VisitorResponseDto>> SearchAsync(string query)
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue,
                Search = query
            });

            return all.Items.ToList();
        }

        var searchTerm = query.ToLower();

        var visitors = await _repository.FindAsync(v =>
            v.Name.ToLower().Contains(searchTerm) ||
            (v.Email != null && v.Email.ToLower().Contains(searchTerm)));

        return _mapper.Map<IEnumerable<VisitorResponseDto>>(visitors);
    }
}