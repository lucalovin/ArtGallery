using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Staff;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class StaffService : IStaffService
{
    private readonly IRepository<Staff> _repository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public StaffService(
        IRepository<Staff> repository,
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

    public async Task<PaginatedResponse<StaffResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (ShouldUseGlobalConnection())
        {
            return await GetAllFromGlobalAsync(request);
        }

        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();

            query = query.Where(s =>
                s.Name.ToLower().Contains(searchTerm) ||
                s.Role.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),

            "role" => request.IsDescending
                ? query.OrderByDescending(s => s.Role)
                : query.OrderBy(s => s.Role),

            "hiredate" => request.IsDescending
                ? query.OrderByDescending(s => s.HireDate)
                : query.OrderBy(s => s.HireDate),

            _ => query.OrderBy(s => s.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<StaffResponseDto>>(items);

        return PaginatedResponse<StaffResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<StaffResponseDto>> GetAllFromGlobalAsync(PagedRequest request)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return PaginatedResponse<StaffResponseDto>.Create(
                new List<StaffResponseDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        var staff = new List<Staff>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                STAFF_ID,
                NAME,
                ROLE,
                HIRE_DATE,
                CERTIFICATION_LEVEL
            FROM STAFF";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            staff.Add(new Staff
            {
                Id = Convert.ToInt32(reader["STAFF_ID"]),
                Name = reader["NAME"]?.ToString() ?? "Unknown",
                Role = reader["ROLE"]?.ToString() ?? "Unknown",
                HireDate = reader["HIRE_DATE"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(reader["HIRE_DATE"]),
                CertificationLevel = reader["CERTIFICATION_LEVEL"] == DBNull.Value
                    ? null
                    : reader["CERTIFICATION_LEVEL"]?.ToString()
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();

            staff = staff
                .Where(s =>
                    s.Name.ToLower().Contains(searchTerm) ||
                    s.Role.ToLower().Contains(searchTerm))
                .ToList();
        }

        staff = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending
                ? staff.OrderByDescending(s => s.Name).ToList()
                : staff.OrderBy(s => s.Name).ToList(),

            "role" => request.IsDescending
                ? staff.OrderByDescending(s => s.Role).ToList()
                : staff.OrderBy(s => s.Role).ToList(),

            "hiredate" => request.IsDescending
                ? staff.OrderByDescending(s => s.HireDate).ToList()
                : staff.OrderBy(s => s.HireDate).ToList(),

            _ => staff.OrderBy(s => s.Name).ToList()
        };

        var totalCount = staff.Count;

        var pagedItems = staff
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<StaffResponseDto>>(pagedItems);

        return PaginatedResponse<StaffResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<StaffResponseDto?> GetByIdAsync(int id)
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items.FirstOrDefault(s => s.Id == id);
        }

        var staff = await _repository.GetByIdAsync(id);
        return staff == null ? null : _mapper.Map<StaffResponseDto>(staff);
    }

    public async Task<StaffResponseDto> CreateAsync(CreateStaffDto dto)
    {
        var staff = _mapper.Map<Staff>(dto);

        await _repository.AddAsync(staff);
        await _repository.SaveChangesAsync();

        return _mapper.Map<StaffResponseDto>(staff);
    }

    public async Task<StaffResponseDto> UpdateAsync(int id, UpdateStaffDto dto)
    {
        var staff = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Staff), id);

        if (dto.Name != null) staff.Name = dto.Name;
        if (dto.Role != null) staff.Role = dto.Role;
        if (dto.HireDate.HasValue) staff.HireDate = dto.HireDate.Value;
        if (dto.CertificationLevel != null) staff.CertificationLevel = dto.CertificationLevel;

        _repository.Update(staff);
        await _repository.SaveChangesAsync();

        return _mapper.Map<StaffResponseDto>(staff);
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Staff), id);

        _repository.Delete(staff);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<StaffResponseDto>> GetByDepartmentAsync(string department)
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return all.Items
                .Where(s => s.Role.Equals(department, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var staff = await _repository.FindAsync(s =>
            s.Role.ToLower() == department.ToLower());

        return _mapper.Map<IEnumerable<StaffResponseDto>>(staff);
    }

    public async Task<StaffStatisticsDto> GetStatisticsAsync()
    {
        if (ShouldUseGlobalConnection())
        {
            var all = await GetAllFromGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            var staff = all.Items.ToList();

            return new StaffStatisticsDto
            {
                TotalStaff = staff.Count,
                ByRole = staff
                    .GroupBy(s => s.Role)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByCertificationLevel = staff
                    .Where(s => !string.IsNullOrWhiteSpace(s.CertificationLevel))
                    .GroupBy(s => s.CertificationLevel!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        var localStaff = await _repository.Query().ToListAsync();

        return new StaffStatisticsDto
        {
            TotalStaff = localStaff.Count,
            ByRole = localStaff
                .GroupBy(s => s.Role)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByCertificationLevel = localStaff
                .Where(s => s.CertificationLevel != null)
                .GroupBy(s => s.CertificationLevel!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}