using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemConfiguration : IEntityTypeConfiguration<CurriculumItem>
{
    public void Configure(EntityTypeBuilder<CurriculumItem> builder)
    {
        builder.ToTable("CurriculumItems");
        // One item per EPA per curriculum, whether it is a national core item or an institution-local
        // addition (T091 phase 3) — an institution can't re-add an EPA already in the national core.
        builder.HasIndex(entity => new { entity.CurriculumId, entity.EpaId }).IsUnique();

        builder.HasOne(entity => entity.Epa)
            .WithMany(entity => entity.CurriculumItems)
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Institution>()
            .WithMany()
            .HasForeignKey(entity => entity.OwningInstitutionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
