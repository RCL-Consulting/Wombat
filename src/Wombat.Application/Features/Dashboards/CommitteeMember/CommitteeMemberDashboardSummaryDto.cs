using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Application.Features.Dashboards.CommitteeMember;

/// <summary>
/// A committee member's view of the trainees in their sub-specialities: each trainee's targets for the current
/// period, and each EPA's (T130).
/// </summary>
/// <remarks>
/// Before T130 this card listed trainees "approaching completion": at least 80% of a lifetime total nobody
/// published. Once targets are per period there is no lifetime total, and a percentage of a period's target
/// reads as behind in every period's first weeks. So the card shows counts, fewest met first, and the threshold
/// is gone.
/// </remarks>
public sealed record CommitteeMemberDashboardSummaryDto(
    string CurrentSemesterName,
    string CurrentSemesterMonths,
    IReadOnlyList<TraineeTargetsItem> TraineeTargets,
    IReadOnlyList<EpaTargetCoverage> EpaTargets,
    int ExemptTraineeCount);

public sealed record TraineeTargetsItem(
    string TraineeUserId,
    string TraineeName,
    int SemesterTargetsMet,
    int SemesterTargetsApplying,
    int YearTargetsMet,
    int YearTargetsApplying);
