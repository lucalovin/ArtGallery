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
        CreateMap<Artwork, ArtworkResponseDto>()
            .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : null))
            .ForMember(dest => dest.CollectionName, opt => opt.MapFrom(src => src.Collection != null ? src.Collection.Name : null))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));
        CreateMap<Artwork, ArtworkListDto>()
            .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : null))
            .ForMember(dest => dest.CollectionName, opt => opt.MapFrom(src => src.Collection != null ? src.Collection.Name : null))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));
        CreateMap<CreateArtworkDto, Artwork>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artist, opt => opt.Ignore())
            .ForMember(dest => dest.Collection, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore())
            .ForMember(dest => dest.ExhibitionArtworks, opt => opt.Ignore())
            .ForMember(dest => dest.Loans, opt => opt.Ignore())
            .ForMember(dest => dest.Insurances, opt => opt.Ignore())
            .ForMember(dest => dest.Restorations, opt => opt.Ignore());
        CreateMap<UpdateArtworkDto, Artwork>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Exhibition mappings
        CreateMap<Exhibition, ExhibitionResponseDto>()
            .ForMember(dest => dest.ArtworkCount, opt => opt.MapFrom(src => src.ExhibitionArtworks.Count))
            .ForMember(dest => dest.ExhibitorName, opt => opt.MapFrom(src => src.Exhibitor != null ? src.Exhibitor.Name : null));
        CreateMap<Exhibition, ExhibitionDetailDto>()
            .ForMember(dest => dest.ArtworkCount, opt => opt.MapFrom(src => src.ExhibitionArtworks.Count))
            .ForMember(dest => dest.ExhibitorName, opt => opt.MapFrom(src => src.Exhibitor != null ? src.Exhibitor.Name : null))
            .ForMember(dest => dest.Artworks, opt => opt.MapFrom(src => src.ExhibitionArtworks));
        CreateMap<ExhibitionArtwork, ExhibitionArtworkDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork != null ? src.Artwork.Title : null))
            .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artwork != null && src.Artwork.Artist != null ? src.Artwork.Artist.Name : null));
        CreateMap<CreateExhibitionDto, Exhibition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Exhibitor, opt => opt.Ignore())
            .ForMember(dest => dest.ExhibitionArtworks, opt => opt.Ignore());
        CreateMap<UpdateExhibitionDto, Exhibition>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Visitor mappings
        CreateMap<Visitor, VisitorResponseDto>();
        CreateMap<CreateVisitorDto, Visitor>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<UpdateVisitorDto, Visitor>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Staff mappings
        CreateMap<Staff, StaffResponseDto>();
        CreateMap<CreateStaffDto, Staff>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Restorations, opt => opt.Ignore());
        CreateMap<UpdateStaffDto, Staff>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Loan mappings
        CreateMap<Loan, LoanResponseDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork != null ? src.Artwork.Title : null))
            .ForMember(dest => dest.ExhibitorName, opt => opt.MapFrom(src => src.Exhibitor != null ? src.Exhibitor.Name : null));
        CreateMap<CreateLoanDto, Loan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore())
            .ForMember(dest => dest.Exhibitor, opt => opt.Ignore());
        CreateMap<UpdateLoanDto, Loan>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Insurance mappings
        CreateMap<Insurance, InsuranceResponseDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork != null ? src.Artwork.Title : null))
            .ForMember(dest => dest.PolicyProvider, opt => opt.MapFrom(src => src.Policy != null ? src.Policy.Provider : null));
        CreateMap<InsurancePolicy, InsurancePolicyResponseDto>();
        CreateMap<CreateInsuranceDto, Insurance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore())
            .ForMember(dest => dest.Policy, opt => opt.Ignore());
        CreateMap<UpdateInsuranceDto, Insurance>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Restoration mappings
        CreateMap<Restoration, RestorationResponseDto>()
            .ForMember(dest => dest.ArtworkTitle, opt => opt.MapFrom(src => src.Artwork != null ? src.Artwork.Title : null))
            .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.Staff != null ? src.Staff.Name : null));
        CreateMap<CreateRestorationDto, Restoration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Artwork, opt => opt.Ignore())
            .ForMember(dest => dest.Staff, opt => opt.Ignore());
        CreateMap<UpdateRestorationDto, Restoration>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // ETL mappings
        CreateMap<EtlSync, EtlSyncResponseDto>();
    }
}
