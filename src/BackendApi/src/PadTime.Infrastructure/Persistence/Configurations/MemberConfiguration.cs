using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadTime.Domain.Members;

namespace PadTime.Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.Subject)
            .HasColumnName("subject")
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(m => m.Matricule, matricule =>
        {
            matricule.Property(x => x.Value)
                .HasColumnName("matricule")
                .HasMaxLength(10)
                .IsRequired();

            matricule.Ignore(x => x.Category);

            matricule.HasIndex(x => x.Value)
                .IsUnique();
        });

        builder.Property(m => m.SiteId)
            .HasColumnName("site_id");

        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(m => m.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(m => m.Version)
            .HasColumnName("version")
            .IsRowVersion();

        builder.Ignore(m => m.Category);

        builder.HasIndex(m => m.Subject)
            .IsUnique();
    }
}
