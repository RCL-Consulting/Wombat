using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class InstitutionCurriculumAdoptionConfiguration : IEntityTypeConfiguration<InstitutionCurriculumAdoption>
{
    public void Configure(EntityTypeBuilder<InstitutionCurriculumAdoption> builder)
    {
        builder.ToTable("InstitutionCurriculumAdoptions");
        builder.Property(entity => entity.AdoptedOn).HasColumnType("date");

        // At most one active adoption per (institution, discipline). Re-adoption deactivates the old
        // record and inserts a new active one, so the constraint is partial (active rows only).
        builder.HasIndex(entity => new { entity.InstitutionId, entity.SubSpecialityId })
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE");

        builder.HasIndex(entity => entity.CurriculumId);

        builder.HasOne(entity => entity.Institution)
            .WithMany()
            .HasForeignKey(entity => entity.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Curriculum)
            .WithMany()
            .HasForeignKey(entity => entity.CurriculumId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SubSpeciality>()
            .WithMany()
            .HasForeignKey(entity => entity.SubSpecialityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
