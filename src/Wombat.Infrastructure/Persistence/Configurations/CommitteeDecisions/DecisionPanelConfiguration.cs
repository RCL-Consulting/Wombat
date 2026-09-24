using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class DecisionPanelConfiguration : IEntityTypeConfiguration<DecisionPanel>
{
    public void Configure(EntityTypeBuilder<DecisionPanel> builder)
    {
        // A panel's scope and its speciality say the same thing (T131 slice 3): institution-wide (1) has no speciality,
        // Speciality (2) has one. Routing reads the scope (DecisionRouting.IsEligible) while the body index below and the
        // handlers' check read the speciality, so a row where they disagreed (institution-wide with a speciality) would
        // take a speciality's body slot while routing treated it as the institution's, and a second institution-wide
        // neonatal panel would be allowed beside it. The handlers never write one; this keeps any other writer out.
        builder.ToTable("DecisionPanels", table => table.HasCheckConstraint(
            "CK_DecisionPanels_ScopeMatchesSpeciality",
            "(\"Scope\" = 1 AND \"SpecialityId\" IS NULL) OR (\"Scope\" = 2 AND \"SpecialityId\" IS NOT NULL)"));

        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => entity.Name);

        // Every panel sits at one institution (T182): the committee rules compare it with the trainee's, and a panel
        // without one matched nobody, so it could review no one.
        builder.HasOne<Institution>()
            .WithMany()
            .HasForeignKey(entity => entity.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        // The College-named committee the panel sits as (T131, Decision 2). A restricting foreign key to the national
        // vocabulary, as on CurriculumItems: a body a panel sits as cannot be deleted from under it.
        builder.Property(entity => entity.DecisionBodyKey).HasMaxLength(DecisionBody.KeyMaxLength);
        builder.HasOne(entity => entity.DecisionBody)
            .WithMany()
            .HasForeignKey(entity => entity.DecisionBodyKey)
            .OnDelete(DeleteBehavior.Restrict);

        // At most one panel per (institution, speciality) sits as each body, and a null speciality is one value, so an
        // institution has at most one institution-wide neonatal panel. An item tagged with a body then routes to exactly
        // one panel wherever one carries the tag (DecisionRouting). General panels are not constrained: the filter leaves
        // them out, so an institution keeps as many as it likes. The handlers check it first and name the panel already
        // sitting as the body; the index holds against a race.
        builder.HasIndex(entity => new { entity.InstitutionId, entity.SpecialityId, entity.DecisionBodyKey })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasFilter("\"DecisionBodyKey\" IS NOT NULL")
            .HasDatabaseName("IX_DecisionPanels_InstitutionId_SpecialityId_DecisionBodyKey");

        // Declared, not left to convention: EF counts the index above as covering the institution foreign key and would
        // drop the one it made for it, but a partial index serves only the panels that carry a body. Every panel list
        // filters on the institution, and deleting an institution checks its panels.
        builder.HasIndex(entity => entity.InstitutionId);

        builder.HasMany(entity => entity.Members)
            .WithOne(entity => entity.Panel)
            .HasForeignKey(entity => entity.PanelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Reviews)
            .WithOne(entity => entity.Panel)
            .HasForeignKey(entity => entity.PanelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
