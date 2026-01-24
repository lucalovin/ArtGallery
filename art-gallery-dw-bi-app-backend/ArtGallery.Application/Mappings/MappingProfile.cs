using AutoMapper;
using ArtGallery.Domain.Entities;
using ArtGallery.Application.DTOs.Artwork;
using ArtGallery.Application.DTOs.Exhibition;
using ArtGallery.Application.DTOs.Visitor;
using ArtGallery.Application.DTOs.Staff;
using ArtGallery.Application.DTOs.Loan;
using ArtGallery.Application.DTOs.Insurance;
using ArtGallery.Application.DTOs.Restoration;
using ArtGallery.Application.DTOs.Etl;

namespace ArtGallery.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Artwork mappings
        CreateMap<Domain.Entities.Artwork, ArtworkResponseDto>();
        CreateMap<Domain.Entities.Artwork, ArtworkListDto>();
        CreateMap<CreateArtworkDto, Domain.Entities.Artwork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ExhibitionArtworks, opt => opt.Ignore())
            .ForMember(dest => dest.Loans, opt => opt.Ignore())
            .ForMember(dest => dest.Insurances, opt => opt.Ignore())
            .ForMember(dest => dest.Restorations, opt => opt.Ignore());
        CreateMap<UpdateArtworkDto, Domain.Entities.Artwork>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Exhibition mappings
        CreateMap<Exhibition, ExhibitionResponseDto>()
            .ForMember(dest => dest.ArtworkCount, opt => opt.MapFrom(src => src.ExhibitionArtworks.Count));
        CreateMap<Exhibition, ExhibitionDetailDto>()
            .ForMember(dest => dest.ArtworkCount, opt => opt.MapFrom(src => src.ExhibitionArtworks.Count))
            .ForMember(dest => dest.Artworks, opt => opt.MapFrom(src => src.ExhibitionArtworks));
        CreateMap<ExhibitionArtwork, ExhibitionArtworkDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Artwork.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Artwork.Title))
            .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artwork.Artist))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Artwork.ImageUrl));
        CreateMap<CreateExhibitionDto, Exhibition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Planning"))
            .ForMember(dest => dest.ActualVisitors, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ExhibitionArtworks, opt => opt.Ignore());
        CreateMap<UpdateExhibitionDto, Exhibition>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Visitor mappings
        CreateMap<Domain.Entities.Visitor, VisitorResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        CreateMap<CreateVisitorDto, Domain.Entities.Visitor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalVisits, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.LastVisit, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateVisitorDto, Domain.Entities.Visitor>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Staff mappings
        CreateMap<Domain.Entities.Staff, StaffResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        CreateMap<CreateStaffDto, Domain.Entities.Staff>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active"))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateStaffDto, Domain.Entities.Staff>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Loan mappings
        CreateMap<Domain.Entities.Loan, LoanResponseDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork.Title))
            .ForMember(dest => dest.ArtworkArtist, opt => opt.MapFrom(src => src.Artwork.Artist))
            .ForMember(dest => dest.ArtworkImageUrl, opt => opt.MapFrom(src => src.Artwork.ImageUrl));
        CreateMap<CreateLoanDto, Domain.Entities.Loan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pending"))
            .ForMember(dest => dest.ConditionOnReturn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateLoanDto, Domain.Entities.Loan>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Insurance mappings
        CreateMap<Domain.Entities.Insurance, InsuranceResponseDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork.Title))
            .ForMember(dest => dest.ArtworkArtist, opt => opt.MapFrom(src => src.Artwork.Artist));
        CreateMap<CreateInsuranceDto, Domain.Entities.Insurance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Active"))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateInsuranceDto, Domain.Entities.Insurance>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Restoration mappings
        CreateMap<Domain.Entities.Restoration, RestorationResponseDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork.Title))
            .ForMember(dest => dest.ArtworkArtist, opt => opt.MapFrom(src => src.Artwork.Artist))
            .ForMember(dest => dest.ArtworkImageUrl, opt => opt.MapFrom(src => src.Artwork.ImageUrl));
        CreateMap<CreateRestorationDto, Domain.Entities.Restoration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Scheduled"))
            .ForMember(dest => dest.ActualCost, opt => opt.Ignore())
            .ForMember(dest => dest.ConditionAfter, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateRestorationDto, Domain.Entities.Restoration>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ETL mappings
        CreateMap<EtlSync, EtlSyncResponseDto>();
    }
}
