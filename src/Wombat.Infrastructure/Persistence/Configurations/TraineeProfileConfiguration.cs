using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class TraineeProfileConfiguration : IEntityTypeConfiguration<TraineeProfile>
{
    public void Configure(EntityTypeBuilder<TraineeProfile> builder)
    {
        builder.ToTable("TraineeProfiles");
        builder.Property(profile => profile.UserId).HasMaxLength(450);
        builder.HasIndex(profile => profile.UserId)
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE");

        builder.HasOne(profile => profile.Curriculum)
            .WithMany()
            .HasForeignKey(profile => profile.CurriculumId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Institution>()
            .WithMany()
            .HasForeignKey(profile => profile.InstitutionId)
            .OnDelete(DeleteBehavior.Restrict);

        // The list the programme ended with (T312): jsonb, like every other stored document here, and read only through
        // EndedCurriculumItem.Parse, since Postgres re-renders what it stores. A snapshot the profile owns whole, written
        // once with the end in the profile's own row, so the end and its list commit, and are put back, together.
        builder.Property(profile => profile.EndedCurriculumJson).HasColumnType("jsonb");

        // Pins the trainee to the national curriculum version their institution adopted (T091 phase 4).
        builder.HasOne<InstitutionCurriculumAdoption>()
            .WithMany()
            .HasForeignKey(profile => profile.AdoptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
