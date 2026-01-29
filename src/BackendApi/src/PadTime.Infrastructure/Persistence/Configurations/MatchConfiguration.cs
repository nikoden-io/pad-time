using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(m => m.CourtId)
            .HasColumnName("court_id")
            .IsRequired();

        builder.Property(m => m.OrganizerId)
            .HasColumnName("organizer_id")
            .IsRequired();

        builder.Property(m => m.StartAtUtc)
            .HasColumnName("start_at_utc")
            .IsRequired();

        builder.Property(m => m.EndAtUtc)
            .HasColumnName("end_at_utc")
            .IsRequired();

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(m => m.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(m => m.Version)
            .HasColumnName("version")
            .IsRowVersion();

        builder.HasMany(m => m.Participants)
            .WithOne()
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Critical: Anti double-booking constraint
        // Only one match per court per slot (excluding cancelled)
        builder.HasIndex(m => new { m.CourtId, m.StartAtUtc })
            .IsUnique()
            .HasFilter("status <> 'Cancelled'");

        builder.HasIndex(m => m.SiteId);
        builder.HasIndex(m => m.OrganizerId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.StartAtUtc);
    }
}
