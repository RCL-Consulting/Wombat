using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Application.Features.Dashboards.Trainee;

/// <summary>
/// The Trainee's Home, in one read behind <c>DashboardFrame</c>, so a failure is Home's one load error (T355, R1; note 3):
/// Your targets, Needs you, Recent decisions and My authorisations. Recent activities and Upcoming deadlines are retired
/// (Q3; T298): a date is shown on the row it belongs to.
/// </summary>
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
/// <param name="RecentDecisions">
/// What someone else decided on the caller's own requests, newest decision first, the first
/// <c>RecentDecisionsListed</c> (<c>DecidedOnYours</c>, T355, B1; E6), each with its count line (E5).
/// </param>
/// <param name="Standing">
/// The standing against Annexure A in summary mode (<c>EntrustmentStandingReader</c>, note 3): no latest rating is read.
/// As on the day the targets are read for (the programme's last day once it has ended). Null for a pending trainee or one
/// with no profile.
/// </param>
public sealed record TraineeDashboardSummaryDto(
    TraineeCurriculumProgressSummaryDto? CurriculumTargets,
    IReadOnlyList<ActivitySummaryDto> NeedsYou,
    IReadOnlyList<ActivitySummaryDto> RecentDecisions,
    EntrustmentStandingDto? Standing,
    bool IsPendingTrainee);
