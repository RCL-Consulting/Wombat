namespace Wombat.Application.Features.Dashboards.Assessor;

/// <param name="PendingRequestCount">
/// How many activities the caller can act on now, less their own portfolio: the rows of their Activity Inbox
/// (<c>ActivityWaiting.LoadActionableAsync</c>) whose subject is someone else (T297). The card and the inbox it links to
/// count the same rows.
/// </param>
/// <param name="AwaitingReview">
/// Those activities, oldest first, at most <c>GetAssessorDashboardSummaryQueryHandler.AwaitingReviewListed</c> of them.
/// </param>
/// <param name="RecentDecisions">
/// The activities the caller moved last that are finished or have no move left, newest move first (T297).
/// </param>
public sealed record AssessorDashboardSummaryDto(
    int PendingRequestCount,
    IReadOnlyList<AwaitingReviewItem> AwaitingReview,
    IReadOnlyList<RecentDecisionItem> RecentDecisions);

/// <summary>An activity waiting on the caller: a row of their inbox that is not their own (T297).</summary>
/// <param name="SubjectName">
/// Whose activity it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />), the id only
/// when that user has no name on record (T250).
/// </param>
/// <param name="CurrentState">The stored state key: the badge's colour.</param>
/// <param name="CurrentStateLabel">
/// The state as the activity's pinned workflow labels it, the key only when that workflow does not declare it: the
/// badge's text when the activity is not overdue (T220).
/// </param>
/// <param name="WaitingSince">When the activity last moved, which is when it came to the caller.</param>
/// <param name="IsOverdue">Whether it has waited longer than <c>DashboardThresholds.AssessorDueDays</c>.</param>
public sealed record AwaitingReviewItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    string CurrentState,
    string CurrentStateLabel,
    DateTime WaitingSince,
    bool IsOverdue);

/// <param name="SubjectName">
/// Whose activity it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />), the id only
/// when that user has no name on record (T250).
/// </param>
/// <param name="FinalState">The stored state key: with <paramref name="IsFinished" />, the badge's colour.</param>
/// <param name="FinalStateLabel">
/// The state as the activity's pinned workflow labels it, the key only when that workflow does not declare it: the
/// badge's text (T220).
/// </param>
/// <param name="IsFinished">
/// Whether the state is a terminal state of the activity's pinned workflow (<c>ActivityCompletion</c>, D44); false for
/// a dead end, such as a declined or cancelled request. The badge is green when it is, whatever the key is called, so a
/// finished teaching session, which ends in <c>accepted</c>, is not badged as work in hand (T266 review).
/// </param>
/// <param name="DecidedOn">When the caller made the move that left it there.</param>
public sealed record RecentDecisionItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    string FinalState,
    string FinalStateLabel,
    bool IsFinished,
    DateTime DecidedOn);
