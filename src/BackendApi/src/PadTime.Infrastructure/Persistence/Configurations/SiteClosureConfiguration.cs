// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Entity Framework mapping for the <see cref="SiteClosure"/> entity.
/// Maps to the "site_closures" table with composite indexes for period queries,
/// GIN index for court-specific closures, and type/reason filtering indexes.
/// </summary>
public sealed class SiteClosureConfiguration : IEntityTypeConfiguration<SiteClosure>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SiteClosure> builder)
    {
        builder.ToTable("site_closures");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Reason)
            .HasColumnName("reason")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(c => c.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(c => c.ModifiedOpeningTime)
            .HasColumnName("modified_opening_time");

        builder.Property(c => c.ModifiedClosingTime)
            .HasColumnName("modified_closing_time");

        builder.Property(c => c.AffectedCourtIds)
            .HasColumnName("affected_court_ids")
            .HasColumnType("uuid[]");

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(c => c.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Existing composite index for closure period queries
        builder.HasIndex(c => new { c.SiteId, c.StartDate, c.EndDate })
            .HasDatabaseName("IX_site_closures_site_period");

        // Additional Performance Indexes for Closure Management
        // Support efficient closure lookups by site
        builder.HasIndex(c => c.SiteId)
            .HasDatabaseName("IX_site_closures_site_id");

        // Support filtering by closure type
        builder.HasIndex(c => c.Type)
            .HasDatabaseName("IX_site_closures_type");

        // Support filtering by closure reason
        builder.HasIndex(c => c.Reason)
            .HasDatabaseName("IX_site_closures_reason");

        // Support date range queries for availability checking
        builder.HasIndex(c => c.StartDate)
            .HasDatabaseName("IX_site_closures_start_date");

        builder.HasIndex(c => c.EndDate)
            .HasDatabaseName("IX_site_closures_end_date");

        // Audit field indexes
        builder.HasIndex(c => c.CreatedAtUtc)
            .HasDatabaseName("IX_site_closures_created_at_utc");

        builder.HasIndex(c => c.UpdatedAtUtc)
            .HasDatabaseName("IX_site_closures_updated_at_utc");

        // Support complex availability queries (site + date range + type)
        builder.HasIndex(c => new { c.SiteId, c.Type, c.StartDate, c.EndDate })
            .HasDatabaseName("IX_site_closures_site_type_period");

        // Support court-specific closure queries
        // Note: AffectedCourtIds is an array, so we use GIN index for array operations
        builder.HasIndex(c => c.AffectedCourtIds)
            .HasDatabaseName("IX_site_closures_affected_courts")
            .HasMethod("gin");
    }
}