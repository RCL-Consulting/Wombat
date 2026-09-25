using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeReviewConfiguration : IEntityTypeConfiguration<CommitteeReview>
{
    public void Configure(EntityTypeBuilder<CommitteeReview> builder)
    {
        // The period a review sits for is a semester of an academic year (T131, Decision 4). The year's bounds are
        // ReviewPeriodRules' (2 to 9998), the years whose semesters and their neighbours AcademicPeriod can represent: a
        // stored year outside them would make CommitteeReview.Period throw and take the review's page down with it.
        builder.ToTable("CommitteeReviews", table =>
        {
            table.HasCheckConstraint("CK_CommitteeReviews_Semester", "\"Semester\" IN (1, 2)");
            table.HasCheckConstraint("CK_CommitteeReviews_AcademicYear", "\"AcademicYear\" BETWEEN 2 AND 9998");

            // A withdrawn review (7) says when and why, and no other review claims either (T258).
            table.HasCheckConstraint(
                "CK_CommitteeReviews_Withdrawn",
                "(\"State\" = 7 AND \"WithdrawnOn\" IS NOT NULL AND \"WithdrawalReason\" IS NOT NULL) OR " +
                "(\"State\" <> 7 AND \"WithdrawnOn\" IS NULL AND \"WithdrawalReason\" IS NULL)");
        });
        builder.Property(entity => entity.TraineeUserId).HasMaxLength(450).IsRequired();
        builder.Ignore(entity => entity.Period);
        builder.Ignore(entity => entity.DecidesProgression);
        builder.Property(entity => entity.StartedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.RatifiedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.WithdrawalReason).HasMaxLength(CommitteeReview.WithdrawalReasonMaxLength);
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

        // One open binding review per trainee, panel and period (T131 slice 4): the agenda is planned per period, and two
        // open sittings would each carry the same closing lines. Only while the review is Scheduled (1), InProgress (2)
        // or Decided (3), so a remediation or post-appeal sitting can follow a ratified one; formative reviews carry no
        // agenda and are left out. The schedule handler checks more widely first (one per seat: every general panel at
        // the institution, or every panel sitting as the same College committee) and names the review in the way; this
        // holds the same panel when two are scheduled at once.
        builder.HasIndex(entity => new { entity.TraineeUserId, entity.PanelId, entity.AcademicYear, entity.Semester })
            .IsUnique()
            .HasFilter("\"IsFormative\" = FALSE AND \"State\" IN (1, 2, 3)")
            .HasDatabaseName("IX_CommitteeReviews_OneOpenBindingReviewPerPeriod");

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
