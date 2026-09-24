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
        {
            table.HasCheckConstraint("CK_CurriculumItems_QuotaPeriod", "\"QuotaPeriod\" IN (0, 1)");

            // The decision cadence reuses QuotaPeriod's integers (T131). Null is a meaningful "no published cadence";
            // any other value outside the enum would read as something nobody chose.
            table.HasCheckConstraint(
                "CK_CurriculumItems_DecisionCadence",
                "\"DecisionCadence\" IS NULL OR \"DecisionCadence\" IN (0, 1)");
        });
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

        // jsonb, like every other stored document here and like CreditedActivityKeysJson, the house precedent for a
        // JSON array (T122). The cost is Postgres's re-rendering: what is read back is never byte-equal to what was
        // written, so compare it only through CurriculumItem.ParsePermittedTools. No foreign key can reach inside
        // it; both writers validate the keys against WbaTools instead.
        builder.Property(entity => entity.PermittedToolsJson).HasColumnType("jsonb");

        // T131. Restrict, like the scale pin: deleting a body must not silently send an EPA back to the general panel.
        builder.Property(entity => entity.DecisionBodyKey).HasMaxLength(DecisionBody.KeyMaxLength);
        builder.HasOne(entity => entity.DecisionBody)
            .WithMany()
            .HasForeignKey(entity => entity.DecisionBodyKey)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
