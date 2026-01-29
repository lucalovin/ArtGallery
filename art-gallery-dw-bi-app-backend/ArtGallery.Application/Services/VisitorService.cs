using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

    public VisitorService(IRepository<Visitor> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<VisitorResponseDto>> GetAllAsync(PagedRequest request)
    {
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
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<VisitorResponseDto>>(items);

        return PaginatedResponse<VisitorResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<VisitorResponseDto?> GetByIdAsync(int id)
    {
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
        var visitors = await _repository.FindAsync(v => v.MembershipType != null && v.MembershipType != "None");
        return _mapper.Map<IEnumerable<VisitorResponseDto>>(visitors);
    }

    public async Task<VisitorStatisticsDto> GetStatisticsAsync()
    {
        var visitors = await _repository.Query().ToListAsync();

        return new VisitorStatisticsDto
        {
            TotalVisitors = visitors.Count,
            TotalMembers = visitors.Count(v => v.MembershipType != null && v.MembershipType != "None"),
            ByMembershipType = visitors
                .Where(v => v.MembershipType != null)
                .GroupBy(v => v.MembershipType!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<VisitorResponseDto>> SearchAsync(string query)
    {
        var searchTerm = query.ToLower();
        var visitors = await _repository.FindAsync(v =>
            v.Name.ToLower().Contains(searchTerm) ||
            (v.Email != null && v.Email.ToLower().Contains(searchTerm)));
        return _mapper.Map<IEnumerable<VisitorResponseDto>>(visitors);
    }
}
