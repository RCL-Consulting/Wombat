using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

/// <summary>A review's agenda: the EPAs it is there to decide (T131 slice 4).</summary>
public sealed class CommitteeAgendaLineConfiguration : IEntityTypeConfiguration<CommitteeAgendaLine>
{
    public void Configure(EntityTypeBuilder<CommitteeAgendaLine> builder)
    {
        // Numbers, not names, because a check constraint is SQL: 2 is Deferred and 3 is Decided
        // (CommitteeAgendaLineState). CommitteeAgendaLinePostgresTests holds each of them.
        builder.ToTable("CommitteeAgendaLines", table =>
        {
            // A decision window is one semester of an academic year, or the whole year (null).
            table.HasCheckConstraint(
                "CK_CommitteeAgendaLines_WindowSemester",
                "\"WindowSemester\" IS NULL OR \"WindowSemester\" IN (1, 2)");

            // A line is Decided exactly when it names the STAR ratifying issued on it.
            table.HasCheckConstraint(
                "CK_CommitteeAgendaLines_DecidedNamesItsStar",
                "(\"State\" = 3) = (\"EntrustmentDecisionId\" IS NOT NULL)");

            // A line is Deferred exactly when it carries the committee's reason.
            table.HasCheckConstraint(
                "CK_CommitteeAgendaLines_DeferredHasAReason",
                "(\"State\" = 2) = (\"DeferralReason\" IS NOT NULL)");
        });

        // Frozen at creation, like the evidence snapshot's lines (T167): no foreign key on the curriculum item or the EPA,
        // so the agenda outlives, and never blocks, a change to the catalogue or the curriculum.
        builder.Property(entity => entity.EpaCode).HasMaxLength(CommitteeAgendaLine.EpaCodeMaxLength).IsRequired();
        builder.Property(entity => entity.EpaTitle).HasMaxLength(CommitteeAgendaLine.EpaTitleMaxLength).IsRequired();
        builder.Property(entity => entity.DeferralReason).HasMaxLength(CommitteeAgendaLine.DeferralReasonMaxLength);

        builder.Ignore(entity => entity.IsYearWindow);
        builder.Ignore(entity => entity.WindowSemesters);
        builder.Ignore(entity => entity.WindowEnd);
        builder.Ignore(entity => entity.WindowLabel);

        // Optimistic concurrency on each line, using Postgres's own xmin (no column, no write path), as the review has.
        // Ratify closes an optional line NotDecided while a deferral of the same line may commit under it: without a
        // token the loser's UPDATE lands on a row the winner changed, and the table's checks, not a concurrency refusal,
        // are what stop it. With one, whichever saves second is refused as the review having changed
        // (CommitteeReviewRacePostgresTests). Deferring and reinstating still mark the review modified: ratify never
        // writes a deferred line, so only the review's token holds a reinstatement against a ratify.
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        // One line per EPA at a review: "staged" is read from the review's staged decision on the EPA, which is unique on
        // the same pair (PendingEntrustmentDecisionConfiguration).
        builder.HasIndex(entity => new { entity.ReviewId, entity.EpaId }).IsUnique();

        builder.HasOne(entity => entity.Review)
            .WithMany(review => review.AgendaLines)
            .HasForeignKey(entity => entity.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        // The STAR a Decided line names. STARs are revoked, never deleted, and the check constraint above would refuse a
        // Decided line losing it, so the key restricts.
        builder.HasOne(entity => entity.EntrustmentDecision)
            .WithMany()
            .HasForeignKey(entity => entity.EntrustmentDecisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
