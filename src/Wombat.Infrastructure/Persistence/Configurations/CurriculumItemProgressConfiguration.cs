using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemProgressConfiguration : IEntityTypeConfiguration<CurriculumItemProgress>
{
    public void Configure(EntityTypeBuilder<CurriculumItemProgress> builder)
    {
        builder.ToTable("CurriculumItemProgresses");
        builder.Property(entity => entity.TraineeUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.LastUpdated).HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.CreditedActivityKeysJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(entity => new { entity.CurriculumItemId, entity.TraineeUserId }).IsUnique();
        builder.HasIndex(entity => entity.TraineeUserId);

        builder.HasOne(entity => entity.CurriculumItem)
            .WithMany()
            .HasForeignKey(entity => entity.CurriculumItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
