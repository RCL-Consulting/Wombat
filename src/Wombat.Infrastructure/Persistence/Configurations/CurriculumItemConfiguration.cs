using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemConfiguration : IEntityTypeConfiguration<CurriculumItem>
{
    public void Configure(EntityTypeBuilder<CurriculumItem> builder)
    {
        builder.ToTable("CurriculumItems");
        builder.HasIndex(entity => new { entity.CurriculumId, entity.EpaId }).IsUnique();

        builder.HasOne(entity => entity.Epa)
            .WithMany(entity => entity.CurriculumItems)
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
