using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Booking;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class ClosureConfiguration : IEntityTypeConfiguration<Closure>
{
    public void Configure(EntityTypeBuilder<Closure> builder)
    {
        builder.ToTable("closures");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.SiteId)
            .HasColumnName("site_id");

        builder.Property(c => c.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(c => c.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500)
            .IsRequired();

        // Unique closure per site per date (including global)
        builder.HasIndex(c => new { c.SiteId, c.Date })
            .IsUnique();
    }
}
