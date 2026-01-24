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
            query = query.Where(s => s.FirstName.ToLower().Contains(searchTerm) ||
                                     s.LastName.ToLower().Contains(searchTerm) ||
                                     s.Email.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(s => s.LastName) : query.OrderBy(s => s.LastName),
            "department" => request.IsDescending ? query.OrderByDescending(s => s.Department) : query.OrderBy(s => s.Department),
            "hiredate" => request.IsDescending ? query.OrderByDescending(s => s.HireDate) : query.OrderBy(s => s.HireDate),
            _ => query.OrderBy(s => s.LastName)
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

        if (dto.FirstName != null) staff.FirstName = dto.FirstName;
        if (dto.LastName != null) staff.LastName = dto.LastName;
        if (dto.Email != null) staff.Email = dto.Email;
        if (dto.Phone != null) staff.Phone = dto.Phone;
        if (dto.Department != null) staff.Department = dto.Department;
        if (dto.Position != null) staff.Position = dto.Position;
        if (dto.Salary.HasValue) staff.Salary = dto.Salary;
        if (dto.Status != null) staff.Status = dto.Status;
        if (dto.ImageUrl != null) staff.ImageUrl = dto.ImageUrl;
        if (dto.Bio != null) staff.Bio = dto.Bio;

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
        var staff = await _repository.FindAsync(s => 
            s.Department.ToLower() == department.ToLower());
        return _mapper.Map<IEnumerable<StaffResponseDto>>(staff);
    }

    public async Task<StaffStatisticsDto> GetStatisticsAsync()
    {
        var staff = await _repository.Query().ToListAsync();

        return new StaffStatisticsDto
        {
            TotalStaff = staff.Count,
            ActiveStaff = staff.Count(s => s.Status == "Active"),
            ByDepartment = staff.GroupBy(s => s.Department)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByStatus = staff.GroupBy(s => s.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
