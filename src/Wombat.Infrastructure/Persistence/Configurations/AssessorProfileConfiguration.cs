using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class AssessorProfileConfiguration : IEntityTypeConfiguration<AssessorProfile>
{
    public void Configure(EntityTypeBuilder<AssessorProfile> builder)
    {
        builder.ToTable("AssessorProfiles");
        builder.Property(profile => profile.UserId).HasMaxLength(450);
        builder.Property(profile => profile.Qualifications).HasMaxLength(4000);
        builder.Property(profile => profile.TrainingStatus).HasConversion<int>();
        builder.HasIndex(profile => profile.UserId).IsUnique();

        builder.HasOne(profile => profile.Institution)
            .WithMany()
            .HasForeignKey(profile => profile.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(profile => profile.Speciality)
            .WithMany()
            .HasForeignKey(profile => profile.SpecialityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(profile => profile.SubSpeciality)
            .WithMany()
            .HasForeignKey(profile => profile.SubSpecialityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
