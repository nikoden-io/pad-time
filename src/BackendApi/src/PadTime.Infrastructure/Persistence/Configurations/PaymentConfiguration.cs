// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Entity Framework mapping for the <see cref="Payment"/> entity.
/// Maps to the "payments" table with a unique idempotency key constraint to prevent duplicate payments.
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    /// <inheritdoc />
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

        builder.Property(m => m.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        // Critical: Idempotency constraint
        builder.HasIndex(p => p.IdempotencyKey)
            .IsUnique();

        builder.HasIndex(p => p.MemberId);
        builder.HasIndex(p => p.MatchId);
    }
}