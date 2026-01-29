using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class SiteYearScheduleConfiguration : IEntityTypeConfiguration<SiteYearSchedule>
{
    public void Configure(EntityTypeBuilder<SiteYearSchedule> builder)
    {
        builder.ToTable("site_year_schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(s => s.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(s => s.OpeningTime)
            .HasColumnName("opening_time")
            .IsRequired();

        builder.Property(s => s.ClosingTime)
            .HasColumnName("closing_time")
            .IsRequired();

        // One schedule per site per year
        builder.HasIndex(s => new { s.SiteId, s.Year })
            .IsUnique();
    }
}
