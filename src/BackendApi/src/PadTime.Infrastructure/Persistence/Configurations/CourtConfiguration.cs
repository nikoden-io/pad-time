using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Booking;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("courts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(c => c.Label)
            .HasColumnName("label")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        // Business Rule Constraints
        // Requirement 2.5: Prevent duplicate court numbers within the same site
        builder.HasIndex(c => new { c.SiteId, c.Label })
            .IsUnique()
            .HasDatabaseName("IX_courts_site_id_label_unique");

        // Performance Indexes for Court Management
        // Support efficient court lookups by site
        builder.HasIndex(c => c.SiteId)
            .HasDatabaseName("IX_courts_site_id");

        // Support filtering by active status
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_courts_is_active");

        // Audit field indexes for temporal queries
        builder.HasIndex(c => c.CreatedAtUtc)
            .HasDatabaseName("IX_courts_created_at_utc");

        // Composite index for common query patterns (active courts by site)
        builder.HasIndex(c => new { c.SiteId, c.IsActive })
            .HasDatabaseName("IX_courts_site_active");

        // Composite index for court management operations (site + label + active status)
        builder.HasIndex(c => new { c.SiteId, c.Label, c.IsActive })
            .HasDatabaseName("IX_courts_site_label_active");
    }
}
