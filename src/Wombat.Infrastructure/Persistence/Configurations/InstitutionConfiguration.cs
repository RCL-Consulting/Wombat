using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("Institutions");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.ShortCode).HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.ContactEmail).HasMaxLength(320);
        builder.HasIndex(entity => entity.Name).IsUnique();
        builder.HasIndex(entity => entity.ShortCode).IsUnique();

        builder.HasMany(entity => entity.Specialities)
            .WithOne(entity => entity.Institution)
            .HasForeignKey(entity => entity.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
