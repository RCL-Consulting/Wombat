using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemConfiguration : IEntityTypeConfiguration<CurriculumItem>
{
    /// <summary>One national item per EPA per curriculum (T223): unique on (curriculum, EPA) where there is no owner.</summary>
    public const string NationalEpaIndexName = "UX_CurriculumItems_National_Curriculum_Epa";

    /// <summary>One item of each institution's own per EPA per curriculum (T223): unique on (curriculum, EPA, owner).</summary>
    public const string LocalEpaIndexName = "UX_CurriculumItems_Local_Curriculum_Epa_Owner";

    /// <summary>
    /// No national item and institution's own item on one EPA of one curriculum (T223). Not in the EF model, which cannot
    /// state an exclusion constraint: the T223 migration writes it.
    /// </summary>
    public const string EpaOncePerInstitutionConstraintName = "EX_CurriculumItems_EpaOncePerInstitution";

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
        // An EPA is on a curriculum once for each institution's trainees (T223). A national item holds its EPA for every
        // institution that adopts the curriculum, and an institution's own item holds it for that institution alone. So
        // two national items never share an EPA, nor two items of one institution's own, and these two partial indexes say
        // so; but institution A's own item and institution B's may, since no trainee is measured against both.
        //
        // Until T223 one index held one item per EPA per curriculum whoever owned it (T091 phase 3), so one institution's
        // own item on a national EPA kept every other institution from an item on it.
        builder.HasIndex(entity => new { entity.CurriculumId, entity.EpaId })
            .IsUnique()
            .HasFilter("\"OwningInstitutionId\" IS NULL")
            .HasDatabaseName(NationalEpaIndexName);

        // Unfiltered: PostgreSQL treats nulls as distinct, so it constrains only an institution's own items, and it covers
        // every row, so it is also the index a lookup by curriculum uses (the filtered one above serves no such lookup).
        builder.HasIndex(entity => new { entity.CurriculumId, entity.EpaId, entity.OwningInstitutionId })
            .IsUnique()
            .HasDatabaseName(LocalEpaIndexName);

        // The third pair, a national item and an institution's own item on one EPA, would measure that institution's
        // trainees against the EPA twice: credit matches an EPA to every item a trainee reads, and the committee plans a
        // line per item. No unique index can refuse it, so the T223 migration adds an exclusion constraint
        // (EpaOncePerInstitutionConstraintName), which the model does not carry. The Add and Update commands refuse it
        // first (CurriculumAdminScope.HoldsItsEpaAgainst); the constraint is for the write that races them, whose refusal
        // the commands put in the same words (CurriculumAdminScope.EpaHeldRefusalAsync).

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
