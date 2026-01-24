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
            query = query.Where(v => v.FirstName.ToLower().Contains(searchTerm) ||
                                     v.LastName.ToLower().Contains(searchTerm) ||
                                     v.Email.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsDescending ? query.OrderByDescending(v => v.LastName) : query.OrderBy(v => v.LastName),
            "email" => request.IsDescending ? query.OrderByDescending(v => v.Email) : query.OrderBy(v => v.Email),
            "membershiptype" => request.IsDescending ? query.OrderByDescending(v => v.MembershipType) : query.OrderBy(v => v.MembershipType),
            _ => query.OrderBy(v => v.LastName)
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

        if (dto.FirstName != null) visitor.FirstName = dto.FirstName;
        if (dto.LastName != null) visitor.LastName = dto.LastName;
        if (dto.Email != null) visitor.Email = dto.Email;
        if (dto.Phone != null) visitor.Phone = dto.Phone;
        if (dto.MembershipType != null) visitor.MembershipType = dto.MembershipType;
        if (dto.MembershipExpiry.HasValue) visitor.MembershipExpiry = dto.MembershipExpiry;
        if (dto.Address != null) visitor.Address = dto.Address;
        if (dto.City != null) visitor.City = dto.City;
        if (dto.Country != null) visitor.Country = dto.Country;
        if (dto.Notes != null) visitor.Notes = dto.Notes;

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
        var visitors = await _repository.FindAsync(v => v.MembershipType != "None");
        return _mapper.Map<IEnumerable<VisitorResponseDto>>(visitors);
    }

    public async Task<VisitorStatisticsDto> GetStatisticsAsync()
    {
        var visitors = await _repository.Query().ToListAsync();
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        return new VisitorStatisticsDto
        {
            TotalVisitors = visitors.Count,
            TotalMembers = visitors.Count(v => v.MembershipType != "None"),
            NewVisitorsThisMonth = visitors.Count(v => v.CreatedAt >= thisMonth),
            ByMembershipType = visitors.GroupBy(v => v.MembershipType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByCountry = visitors.Where(v => v.Country != null)
                .GroupBy(v => v.Country!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<VisitorResponseDto>> SearchAsync(string query)
    {
        var searchTerm = query.ToLower();
        var visitors = await _repository.FindAsync(v =>
            v.FirstName.ToLower().Contains(searchTerm) ||
            v.LastName.ToLower().Contains(searchTerm) ||
            v.Email.ToLower().Contains(searchTerm));
        return _mapper.Map<IEnumerable<VisitorResponseDto>>(visitors);
    }
}
