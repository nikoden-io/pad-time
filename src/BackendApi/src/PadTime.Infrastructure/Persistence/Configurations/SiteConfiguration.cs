// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Entity Framework mapping for the <see cref="Site"/> entity.
/// Maps to the "sites" table with cascading relationships to courts, schedules, and closures,
/// unique name+city constraint, and indexes for search, filtering, and pagination.
/// </summary>
public sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("sites");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.StreetNumber)
            .HasColumnName("street_number")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(s => s.Street)
            .HasColumnName("street")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Postcode)
            .HasColumnName("postcode")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.City)
            .HasColumnName("city")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Country)
            .HasColumnName("country")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(s => s.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        // Relationships
        builder.HasMany(s => s.Courts)
            .WithOne()
            .HasForeignKey(c => c.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Schedules)
            .WithOne()
            .HasForeignKey(sc => sc.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Closures)
            .WithOne()
            .HasForeignKey(c => c.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Courts).AutoInclude();
        builder.Navigation(s => s.Schedules).AutoInclude();
        builder.Navigation(s => s.Closures).AutoInclude();

        // Business Rule Constraints
        // Requirement 1.5: Prevent creation of sites with duplicate names within the same geographic area
        builder.HasIndex(s => new { s.Name, s.City })
            .IsUnique()
            .HasDatabaseName("IX_sites_name_city_unique");

        // Performance Indexes for Search and Filtering
        // Requirement 4.1, 4.2: Support site listings with search and filtering
        builder.HasIndex(s => s.Name)
            .HasDatabaseName("IX_sites_name_search");

        builder.HasIndex(s => s.City)
            .HasDatabaseName("IX_sites_city_search");

        builder.HasIndex(s => s.Country)
            .HasDatabaseName("IX_sites_country_search");

        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_sites_is_active_filter");

        // Audit and Temporal Indexes
        // Requirement 5.5: Maintain audit logs and support temporal queries
        builder.HasIndex(s => s.CreatedAtUtc)
            .HasDatabaseName("IX_sites_created_at_utc");

        builder.HasIndex(s => s.UpdatedAtUtc)
            .HasDatabaseName("IX_sites_updated_at_utc");

        // Composite index for common query patterns (active sites by location)
        builder.HasIndex(s => new { s.IsActive, s.City, s.Country })
            .HasDatabaseName("IX_sites_active_location");

        // Composite index for pagination and sorting
        builder.HasIndex(s => new { s.CreatedAtUtc, s.Id })
            .HasDatabaseName("IX_sites_created_id_pagination");
    }
}