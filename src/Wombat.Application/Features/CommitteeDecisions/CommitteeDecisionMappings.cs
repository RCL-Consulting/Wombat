using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

internal static class CommitteeDecisionMappings
{
    public static CommitteeReviewDetailDto ToDetailDto(this CommitteeReview review)
        => new(
            review.Id,
            review.TraineeUserId,
            review.PanelId,
            review.Panel.Name,
            review.ReviewPeriodFrom,
            review.ReviewPeriodTo,
            review.ScheduledOn,
            review.State,
            review.StartedOn,
            review.StartedByUserId,
            review.RatifiedOn,
            review.RatifiedByUserId,
            review.FinalizedOn,
            review.Decisions
                .OrderByDescending(decision => decision.DecidedOn)
                .ThenByDescending(decision => decision.Id)
                .Select(decision => new CommitteeDecisionDto(
                    decision.Id,
                    decision.Category,
                    decision.Rationale,
                    decision.Conditions,
                    decision.DecidedOn,
                    decision.DecidedByChairUserId,
                    decision.SupersedesDecisionId)
                {
                    // T165. Who sat for this decision, ids and roles only: the query that serves the page names them.
                    Attendees = decision.Attendees
                        .OrderBy(attendee => attendee.Role)
                        .ThenBy(attendee => attendee.UserId, StringComparer.Ordinal)
                        .Select(attendee => new CommitteePersonDto(attendee.UserId, attendee.Role))
                        .ToArray()
                })
                .ToArray(),
            review.Appeals
                .OrderByDescending(appeal => appeal.LodgedOn)
                .ThenByDescending(appeal => appeal.Id)
                .Select(appeal => new CommitteeAppealDto(
                    appeal.Id,
                    appeal.LodgedOn,
                    appeal.LodgedByUserId,
                    appeal.Reason,
                    appeal.ResolvedOn,
                    appeal.ResolvedByUserId,
                    appeal.Outcome))
                .ToArray(),
            review.EvidenceItems
                .OrderBy(item => item.SourceType)
                .ThenBy(item => item.SourceRecordedOn)
                .ThenBy(item => item.Id)
                .Select(item => new CommitteeEvidenceDto(
                    item.Id,
                    item.SourceType,
                    item.ActivityId,
                    item.MsfCampaignId,
                    item.SupervisorReportId,
                    item.SourceLabel,
                    item.Summary,
                    item.SourceRecordedOn,
                    item.EpaId,
                    item.EpaCode,
                    item.EpaTitle,
                    item.InstrumentKey,
                    item.InstrumentName,
                    item.IsRatedInstrument,
                    item.RatingOrder,
                    item.RatingLabel,
                    item.ObservedOn,
                    item.ObservedOnSource is null ? null : item.ObservedOnSource == ObservationDateSource.Declared,
                    item.SourceState,
                    item.SourceFinished,
                    item.AssessorUserId,
                    // T220: the frozen label, else the key of a line frozen before labels were.
                    item.SourceStateLabel ?? item.SourceState))
                .ToArray(),
            review.IsFormative,
            review.ReviewType)
        {
            // T165. Who sits on the panel. Ids and roles only: the query that serves the page names them (T142) and says
            // who may sit (PanelSeat).
            PanelMembers = review.Panel.Members
                .OrderBy(member => member.Role)
                .ThenBy(member => member.UserId, StringComparer.Ordinal)
                .Select(member => new CommitteePersonDto(member.UserId, member.Role))
                .ToArray(),
            QuorumShortfall = review is { IsFormative: false, State: CommitteeReviewState.Decided }
                ? review.QuorumShortfall()
                : null,
            AcademicYear = review.AcademicYear,
            Semester = review.Semester,
            // The day, on the South African calendar the erasure ends the trainee's profile on (T258 review): on the UTC
            // calendar the two disagree for an erasure between 22:00 and midnight.
            WithdrawnOn = review.WithdrawnOn is { } withdrawnOn ? ProgrammeCalendar.DateOf(withdrawnOn) : null,
            WithdrawalReason = review.WithdrawalReason
        };
}
