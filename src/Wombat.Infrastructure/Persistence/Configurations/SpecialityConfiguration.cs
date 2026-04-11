using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class SpecialityConfiguration : IEntityTypeConfiguration<Speciality>
{
    public void Configure(EntityTypeBuilder<Speciality> builder)
    {
        builder.ToTable("Specialities");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2000);
        builder.HasIndex(entity => new { entity.InstitutionId, entity.Name }).IsUnique();

        builder.HasMany(entity => entity.SubSpecialities)
            .WithOne(entity => entity.Speciality)
            .HasForeignKey(entity => entity.SpecialityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
