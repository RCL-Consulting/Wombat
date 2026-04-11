using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumConfiguration : IEntityTypeConfiguration<Curriculum>
{
    public void Configure(EntityTypeBuilder<Curriculum> builder)
    {
        builder.ToTable("Curricula");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Version).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.EffectiveFrom).HasColumnType("date");
        builder.Property(entity => entity.EffectiveTo).HasColumnType("date");
        builder.HasIndex(entity => new { entity.SubSpecialityId, entity.Name, entity.Version }).IsUnique();

        builder.HasOne(entity => entity.SubSpeciality)
            .WithMany(entity => entity.Curricula)
            .HasForeignKey(entity => entity.SubSpecialityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Items)
            .WithOne(entity => entity.Curriculum)
            .HasForeignKey(entity => entity.CurriculumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
