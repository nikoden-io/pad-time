using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class OrganizerDebtConfiguration : IEntityTypeConfiguration<OrganizerDebt>
{
    public void Configure(EntityTypeBuilder<OrganizerDebt> builder)
    {
        builder.ToTable("organizer_debts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(d => d.AmountCents)
            .HasColumnName("amount_cents")
            .IsRequired();

        builder.Property(d => d.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(d => d.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.Property(d => d.Version)
            .HasColumnName("version")
            .IsRowVersion();

        // One debt record per member
        builder.HasIndex(d => d.MemberId)
            .IsUnique();
    }
}
