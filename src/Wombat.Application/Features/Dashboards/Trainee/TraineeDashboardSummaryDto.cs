using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Application.Features.Dashboards.Trainee;

/// <param name="CurriculumTargets">
/// The trainee's curriculum progress for the current period: the same read model the progress page uses (T130). For a
/// trainee whose programme has ended, the one they ended on, marked as ended (T252). Null for a pending trainee or one
/// with no profile at all.
/// </param>
/// <param name="NeedsYou">
/// Every row of the caller's Needs you, most recently updated first: their drafts and the work returned to them, read by
/// the code <c>ListNeedsYouQuery</c> reads them with (<c>NeedsYou.ReadAsync</c>; T297's rule, restated by T342). Home's
/// card lists the first <c>NeedsYouListed</c> and counts them all.
/// </param>
public sealed record TraineeDashboardSummaryDto(
    TraineeCurriculumProgressSummaryDto? CurriculumTargets,
    IReadOnlyList<ActivitySummaryDto> NeedsYou,
    IReadOnlyList<RecentActivityItem> RecentActivities,
    IReadOnlyList<UpcomingDeadlineItem> UpcomingDeadlines,
    bool IsPendingTrainee);

/// <param name="CurrentState">The stored state key: with <paramref name="IsFinished" />, the badge's colour.</param>
/// <param name="CurrentStateLabel">
/// The state as the activity's pinned workflow labels it, the key only when that workflow does not declare it: the
/// badge's text (T220).
/// </param>
/// <param name="IsFinished">
/// Whether the state is a terminal state of the activity's pinned workflow (<c>ActivityCompletion</c>, D44): the badge is
/// green when it is, whatever the key is called, so a finished teaching session, which ends in <c>accepted</c>, is not
/// badged as work in hand (T266 review).
/// </param>
public sealed record RecentActivityItem(
    int ActivityId,
    string ActivityTypeName,
    string CurrentState,
    string CurrentStateLabel,
    bool IsFinished,
    DateTime CreatedOn);

public sealed record UpcomingDeadlineItem(
    int ActivityId,
    string ActivityTypeName,
    string FieldLabel,
    DateOnly DueDate);
