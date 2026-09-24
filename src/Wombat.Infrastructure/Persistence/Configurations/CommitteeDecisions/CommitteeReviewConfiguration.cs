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

        // Optimistic concurrency on the review, using Postgres's own xmin (no column, no write path), as MsfCampaign does.
        // Load-bearing since T131: staging a decision checks the review's state in memory, and ratifying deletes only the
        // staged rows it read. Without a token a stage that read the review before a ratify committed would add a staged
        // row to a ratified review, and a stage committed after a ratify read the review would be left behind by it: a
        // staged row nothing can remove or issue. So staging marks the review modified, and whichever of the two saves
        // second is refused whole (StagePendingEntrustmentDecision, CommitteeReviewRacePostgresTests).
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

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
