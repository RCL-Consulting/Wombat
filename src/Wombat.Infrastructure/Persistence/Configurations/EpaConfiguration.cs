using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Epas;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class EpaConfiguration : IEntityTypeConfiguration<Epa>
{
    public void Configure(EntityTypeBuilder<Epa> builder)
    {
        builder.ToTable("Epas");
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(4000);
        builder.Property(entity => entity.RequiredKnowledgeSkills).HasMaxLength(8000);
        builder.HasIndex(entity => new { entity.SubSpecialityId, entity.Code }).IsUnique();

        builder.HasOne(entity => entity.SubSpeciality)
            .WithMany(entity => entity.Epas)
            .HasForeignKey(entity => entity.SubSpecialityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
