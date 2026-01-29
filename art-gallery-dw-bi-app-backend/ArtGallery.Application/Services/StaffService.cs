using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

    public StaffService(IRepository<Staff> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<StaffResponseDto>> GetAllAsync(PagedRequest request)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchTerm) ||
                                     s.Role.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
            "role" => request.IsDescending ? query.OrderByDescending(s => s.Role) : query.OrderBy(s => s.Role),
            "hiredate" => request.IsDescending ? query.OrderByDescending(s => s.HireDate) : query.OrderBy(s => s.HireDate),
            _ => query.OrderBy(s => s.Name)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<StaffResponseDto>>(items);

        return PaginatedResponse<StaffResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<StaffResponseDto?> GetByIdAsync(int id)
    {
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
        // Note: Department no longer exists, using Role instead
        var staff = await _repository.FindAsync(s => 
            s.Role.ToLower() == department.ToLower());
        return _mapper.Map<IEnumerable<StaffResponseDto>>(staff);
    }

    public async Task<StaffStatisticsDto> GetStatisticsAsync()
    {
        var staff = await _repository.Query().ToListAsync();

        return new StaffStatisticsDto
        {
            TotalStaff = staff.Count,
            ByRole = staff.GroupBy(s => s.Role)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByCertificationLevel = staff
                .Where(s => s.CertificationLevel != null)
                .GroupBy(s => s.CertificationLevel!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
