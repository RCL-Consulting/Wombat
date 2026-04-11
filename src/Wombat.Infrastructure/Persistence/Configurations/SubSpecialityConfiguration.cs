using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class SubSpecialityConfiguration : IEntityTypeConfiguration<SubSpeciality>
{
    public void Configure(EntityTypeBuilder<SubSpeciality> builder)
    {
        builder.ToTable("SubSpecialities");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2000);
        builder.HasIndex(entity => new { entity.SpecialityId, entity.Name }).IsUnique();
    }
}
