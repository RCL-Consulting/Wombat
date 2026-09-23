using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemConfiguration : IEntityTypeConfiguration<CurriculumItem>
{
    public void Configure(EntityTypeBuilder<CurriculumItem> builder)
    {
        // QuotaPeriod is stored as its integer value (T130). A value outside the enum would read as an academic
        // year (QuotaWindow treats anything but Semester that way), but a write must never produce one.
        builder.ToTable("CurriculumItems", table =>
            table.HasCheckConstraint("CK_CurriculumItems_QuotaPeriod", "\"QuotaPeriod\" IN (0, 1)"));
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

        // Nullable on purpose: an unpinned item compares ordinals exactly as it did before T109. Restrict
        // rather than SetNull, because silently unpinning a curriculum when a scale is deleted is how the
        // defect comes back.
        builder.HasOne(entity => entity.Scale)
            .WithMany()
            .HasForeignKey(entity => entity.ScaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => entity.ScaleId);
    }
}
