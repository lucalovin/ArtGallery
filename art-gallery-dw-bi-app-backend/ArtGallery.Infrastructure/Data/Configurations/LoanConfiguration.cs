using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Loan entity.
/// </summary>
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.BorrowerName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.BorrowerType)
            .HasMaxLength(100);

        builder.Property(l => l.BorrowerContact)
            .HasMaxLength(255);

        builder.Property(l => l.BorrowerAddress)
            .HasMaxLength(500);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(l => l.InsuranceValue)
            .HasPrecision(18, 2);

        builder.Property(l => l.InsuranceProvider)
            .HasMaxLength(255);

        builder.Property(l => l.InsurancePolicyNumber)
            .HasMaxLength(100);

        builder.Property(l => l.LoanFee)
            .HasPrecision(18, 2);

        builder.Property(l => l.Purpose)
            .HasMaxLength(500);

        builder.Property(l => l.ConditionOnLoan)
            .HasMaxLength(500);

        builder.Property(l => l.ConditionOnReturn)
            .HasMaxLength(500);

        builder.Property(l => l.Notes)
            .HasMaxLength(1000);

        builder.HasOne(l => l.Artwork)
            .WithMany(a => a.Loans)
            .HasForeignKey(l => l.ArtworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.LoanEndDate);
    }
}
