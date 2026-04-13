using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class InstitutionBrandConfiguration : IEntityTypeConfiguration<InstitutionBrand>
{
    public void Configure(EntityTypeBuilder<InstitutionBrand> builder)
    {
        builder.ToTable("InstitutionBrands");
        builder.Property(entity => entity.PrimaryColorHex).HasMaxLength(7);
        builder.Property(entity => entity.SecondaryColorHex).HasMaxLength(7);

        builder.HasIndex(entity => entity.InstitutionId).IsUnique();

        builder.HasOne(entity => entity.Institution)
            .WithOne()
            .HasForeignKey<InstitutionBrand>(entity => entity.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
