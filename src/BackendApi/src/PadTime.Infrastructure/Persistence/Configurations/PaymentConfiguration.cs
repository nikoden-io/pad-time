using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.MatchId)
            .HasColumnName("match_id")
            .IsRequired();

        builder.Property(p => p.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(p => p.ParticipantId)
            .HasColumnName("participant_id")
            .IsRequired();

        builder.Property(p => p.AmountCents)
            .HasColumnName("amount_cents")
            .IsRequired();

        builder.Property(p => p.Purpose)
            .HasColumnName("purpose")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.State)
            .HasColumnName("state")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(p => p.ProcessedAtUtc)
            .HasColumnName("processed_at_utc");

        builder.Property(p => p.Version)
            .HasColumnName("version")
            .IsRowVersion();

        // Critical: Idempotency constraint
        builder.HasIndex(p => p.IdempotencyKey)
            .IsUnique();

        builder.HasIndex(p => p.MemberId);
        builder.HasIndex(p => p.MatchId);
    }
}
