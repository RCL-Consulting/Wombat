using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Identity;

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
    }
}
