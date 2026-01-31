using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for GalleryReview entity.
/// </summary>
public class GalleryReviewConfiguration : IEntityTypeConfiguration<GalleryReview>
{
    public void Configure(EntityTypeBuilder<GalleryReview> builder)
    {
        builder.ToTable("GALLERY_REVIEW");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("REVIEW_ID");

        builder.Property(r => r.VisitorId)
            .IsRequired()
            .HasColumnName("VISITOR_ID");

        builder.Property(r => r.ArtworkId)
            .HasColumnName("ARTWORK_ID");

        builder.Property(r => r.ExhibitionId)
            .HasColumnName("EXHIBITION_ID");

        builder.Property(r => r.Rating)
            .IsRequired()
            .HasColumnName("RATING");

        builder.Property(r => r.ReviewText)
            .HasMaxLength(256)
            .HasColumnName("REVIEW_TEXT");

        builder.Property(r => r.ReviewDate)
            .IsRequired()
            .HasColumnName("REVIEW_DATE");

        // Relationships
        builder.HasOne(r => r.Visitor)
            .WithMany()
            .HasForeignKey(r => r.VisitorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Artwork)
            .WithMany()
            .HasForeignKey(r => r.ArtworkId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Exhibition)
            .WithMany()
            .HasForeignKey(r => r.ExhibitionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(r => r.VisitorId);
        builder.HasIndex(r => r.ArtworkId);
        builder.HasIndex(r => r.ExhibitionId);
        builder.HasIndex(r => r.ReviewDate);
    }
}
