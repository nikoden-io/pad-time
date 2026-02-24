using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Site;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class SiteScheduleConfiguration : IEntityTypeConfiguration<SiteSchedule>
{
    public void Configure(EntityTypeBuilder<SiteSchedule> builder)
    {
        builder.ToTable("site_schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ValidFrom)
            .HasColumnName("valid_from")
            .IsRequired();

        builder.Property(s => s.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(s => s.OpeningTime)
            .HasColumnName("opening_time")
            .IsRequired();

        builder.Property(s => s.ClosingTime)
            .HasColumnName("closing_time")
            .IsRequired();

        builder.Property(s => s.ApplicableDays)
            .HasColumnName("applicable_days")
            .HasColumnType("integer[]");

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Existing composite index for schedule resolution
        builder.HasIndex(s => new { s.SiteId, s.ValidFrom, s.Priority })
            .HasDatabaseName("IX_site_schedules_site_valid_priority");

        // Additional Performance Indexes for Schedule Management
        // Support efficient schedule lookups by site
        builder.HasIndex(s => s.SiteId)
            .HasDatabaseName("IX_site_schedules_site_id");

        // Support filtering by active status
        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_site_schedules_is_active");

        // Support temporal queries for schedule validity
        builder.HasIndex(s => new { s.ValidFrom, s.ValidUntil })
            .HasDatabaseName("IX_site_schedules_validity_period");

        // Support priority-based schedule resolution
        builder.HasIndex(s => new { s.SiteId, s.Priority, s.IsActive })
            .HasDatabaseName("IX_site_schedules_site_priority_active");

        // Audit field indexes
        builder.HasIndex(s => s.CreatedAtUtc)
            .HasDatabaseName("IX_site_schedules_created_at_utc");

        builder.HasIndex(s => s.UpdatedAtUtc)
            .HasDatabaseName("IX_site_schedules_updated_at_utc");

        // Support schedule overlap detection queries
        builder.HasIndex(s => new { s.SiteId, s.ValidFrom, s.ValidUntil, s.IsActive })
            .HasDatabaseName("IX_site_schedules_overlap_detection");
    }
}
