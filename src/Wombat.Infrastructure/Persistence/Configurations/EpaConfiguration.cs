using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class EpaConfiguration : IEntityTypeConfiguration<Epa>
{
    public void Configure(EntityTypeBuilder<Epa> builder)
    {
        // T196: an EPA is inactive exactly when it records when its pause began. Credit is judged against that moment
        // (Epa.InForceAt), so an inactive EPA with no moment, or an active one with a stale one, would credit wrongly.
        builder.ToTable("Epas", table => table.HasCheckConstraint(
            "CK_Epas_DeactivatedOn",
            "(\"IsActive\" AND \"DeactivatedOn\" IS NULL) OR (NOT \"IsActive\" AND \"DeactivatedOn\" IS NOT NULL)"));
        builder.Property(entity => entity.Code).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Title).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Domain).HasMaxLength(200);
        builder.Property(entity => entity.Description).HasMaxLength(4000);
        builder.Property(entity => entity.RequiredKnowledgeSkills).HasMaxLength(8000);

        // National EPA codes are unique per sub-speciality; institution-local extras (T091 phase 3) are
        // unique per (sub-speciality, institution). Partial indexes keep the two namespaces separate —
        // Postgres treats NULLs as distinct, so a single composite index would not enforce national uniqueness.
        builder.HasIndex(entity => new { entity.SubSpecialityId, entity.Code })
            .IsUnique()
            .HasFilter("\"OwningInstitutionId\" IS NULL");
        builder.HasIndex(entity => new { entity.SubSpecialityId, entity.OwningInstitutionId, entity.Code })
            .IsUnique()
            .HasFilter("\"OwningInstitutionId\" IS NOT NULL");
        builder.HasSeedKey();

        builder.HasOne(entity => entity.SubSpeciality)
            .WithMany(entity => entity.Epas)
            .HasForeignKey(entity => entity.SubSpecialityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Institution>()
            .WithMany()
            .HasForeignKey(entity => entity.OwningInstitutionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
