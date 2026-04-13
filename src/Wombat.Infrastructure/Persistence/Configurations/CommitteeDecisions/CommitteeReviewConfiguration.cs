using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeReviewConfiguration : IEntityTypeConfiguration<CommitteeReview>
{
    public void Configure(EntityTypeBuilder<CommitteeReview> builder)
    {
        builder.ToTable("CommitteeReviews");
        builder.Property(entity => entity.TraineeUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.StartedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.RatifiedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.ReviewPeriodFrom).HasColumnType("date");
        builder.Property(entity => entity.ReviewPeriodTo).HasColumnType("date");
        builder.Property(entity => entity.ScheduledOn).HasColumnType("date");

        builder.HasIndex(entity => new { entity.TraineeUserId, entity.State });
        builder.HasIndex(entity => new { entity.PanelId, entity.ScheduledOn });

        builder.HasMany(entity => entity.Decisions)
            .WithOne(entity => entity.Review)
            .HasForeignKey(entity => entity.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.Appeals)
            .WithOne(entity => entity.Review)
            .HasForeignKey(entity => entity.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(entity => entity.EvidenceItems)
            .WithOne(entity => entity.Review)
            .HasForeignKey(entity => entity.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
