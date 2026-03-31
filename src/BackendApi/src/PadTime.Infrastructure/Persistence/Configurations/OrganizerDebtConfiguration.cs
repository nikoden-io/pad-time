// -----------------------------------------------------------------------
// Copyright (c) Nikoden.IO. All rights reserved.
// -----------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Billing;

namespace PadTime.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the Entity Framework mapping for the <see cref="OrganizerDebt"/> entity.
/// Maps to the "organizer_debts" table with a unique constraint enforcing one debt record per member.
/// </summary>
public sealed class OrganizerDebtConfiguration : IEntityTypeConfiguration<OrganizerDebt>
{
    /// <inheritdoc />
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

        builder.Property(m => m.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .ValueGeneratedNever();

        // One debt record per member
        builder.HasIndex(d => d.MemberId)
            .IsUnique();
    }
}