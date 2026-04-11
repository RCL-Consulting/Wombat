using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Forms;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class AssessmentFormConfiguration : IEntityTypeConfiguration<AssessmentForm>
{
    public void Configure(EntityTypeBuilder<AssessmentForm> builder)
    {
        builder.ToTable("AssessmentForms");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => new { entity.SubSpecialityId, entity.Name });

        builder.HasOne(entity => entity.Institution)
            .WithMany()
            .HasForeignKey(entity => entity.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Speciality)
            .WithMany()
            .HasForeignKey(entity => entity.SpecialityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.SubSpeciality)
            .WithMany(entity => entity.AssessmentForms)
            .HasForeignKey(entity => entity.SubSpecialityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Scale)
            .WithMany(entity => entity.Forms)
            .HasForeignKey(entity => entity.ScaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Criteria)
            .WithOne(entity => entity.Form)
            .HasForeignKey(entity => entity.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.EpaLinks)
            .WithOne(entity => entity.Form)
            .HasForeignKey(entity => entity.FormId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
