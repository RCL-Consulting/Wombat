namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Where one trainee stands on every EPA of their curriculum: the committee's STAR decision against Annexure A's target
/// for their training year, and against the programme exit rule. Read-only; nothing is gated on it. (T166)
/// </summary>
/// <param name="AsOf">The day the training year is read for.</param>
/// <param name="ProgrammeStartDate">The start of the programme on the trainee's preferred profile.</param>
/// <param name="TargetYear">
/// The training year whose targets are shown: <c>TraineeProfile.GetStage</c> on <paramref name="AsOf" /> (D17), or 1
/// when the programme has not started, which is the year the trainee will first be held to.
/// </param>
/// <param name="ProgrammeNotStarted">The programme starts after <paramref name="AsOf" />.</param>
/// <param name="Epas">One row per curriculum item in force, by EPA code.</param>
/// <param name="Exit">The exit rule, counted over the national core rows only.</param>
public sealed record EntrustmentStandingDto(
    DateOnly AsOf,
    DateOnly ProgrammeStartDate,
    int TargetYear,
    bool ProgrammeNotStarted,
    IReadOnlyList<EpaStandingDto> Epas,
    ExitRuleReadinessDto Exit)
{
    /// <summary>How many EPAs have a STAR decision at or above this year's target.</summary>
    public int AtYearTarget => Count(EntrustmentStandingStatus.AtOrAbove);

    /// <summary>How many EPAs have a STAR decision below this year's target.</summary>
    public int BelowYearTarget => Count(EntrustmentStandingStatus.Below);

    /// <summary>How many EPAs have no active STAR decision.</summary>
    public int WithoutDecision => Count(EntrustmentStandingStatus.NoDecision);

    /// <summary>How many EPAs have a decision that cannot be read against the item's ladder.</summary>
    public int NotComparable => Count(EntrustmentStandingStatus.NotComparable);

    /// <summary>How many EPAs have no level for this training year, so their exit level stands in.</summary>
    public int YearTargetsFromExitLevel => Epas.Count(epa => epa.YearTargetIsExitLevel);

    /// <summary>How many EPAs are the institution's own additions, outside the College's exit rule.</summary>
    public int LocalEpas => Epas.Count(epa => epa.IsLocal);

    private int Count(EntrustmentStandingStatus status) => Epas.Count(epa => epa.YearStatus == status);
}

/// <summary>One EPA of the trainee's curriculum. (T166)</summary>
/// <param name="ScaleName">The ladder the item's minima are pinned to (T109), or null when it is unpinned.</param>
/// <param name="IsLocal">
/// An institution's own addition to the national curriculum (<c>OwningInstitutionId</c> set): shown with its own exit
/// level, but not counted in the College's exit rule.
/// </param>
/// <param name="YearTargetOrder">
/// Annexure A's level for <see cref="EntrustmentStandingDto.TargetYear" />:
/// <c>CurriculumItem.GetMinimumLevelForStage</c>, the per-stage map the credit engine also reads. A year the map does
/// not name reads the exit level, as it does there, and <paramref name="YearTargetIsExitLevel" /> says so.
/// </param>
/// <param name="YearTargetLabel">That level as the rung a clinician reads ("3b", T100).</param>
/// <param name="YearTargetIsExitLevel">
/// The item's per-stage map names no level for this training year (it has no map, or the trainee is past the years it
/// names), so the year target is the exit level standing in, not a level anyone set for the year.
/// </param>
/// <param name="ExitLevelOrder">The level required to finish: <c>CurriculumItem.MinimumLevelOrder</c>.</param>
/// <param name="ExitLevelLabel">That level as a rung.</param>
/// <param name="Decision">The active STAR decision on this EPA, or null.</param>
/// <param name="YearStatus">The decision against the year target.</param>
/// <param name="ExitStatus">The decision against the exit level.</param>
/// <param name="LatestRating">The latest rating a named assessor gave on this EPA that the caller may read, or null.</param>
public sealed record EpaStandingDto(
    int CurriculumItemId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    string? ScaleName,
    bool IsLocal,
    int YearTargetOrder,
    string YearTargetLabel,
    bool YearTargetIsExitLevel,
    int ExitLevelOrder,
    string ExitLevelLabel,
    StandingDecisionDto? Decision,
    EntrustmentStandingStatus YearStatus,
    EntrustmentStandingStatus ExitStatus,
    StandingRatingDto? LatestRating);

/// <summary>An active entrustment decision (STAR), as the standing shows it. (T166)</summary>
/// <param name="LevelLabel">The authorised level's own label, on its own ladder.</param>
/// <param name="OtherLadderName">
/// The decision's ladder when it is known to differ from the item's pin, so the page can say why the two are not
/// compared. Null when it is on the item's ladder or the item is unpinned.
/// </param>
public sealed record StandingDecisionDto(
    int DecisionId,
    int LevelOrder,
    string LevelLabel,
    string? OtherLadderName,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn);

/// <summary>
/// The latest rating a named assessor gave on an EPA, read by <c>AttributedRatings</c>, the reading the trajectory chart
/// plots. (T135, T166)
/// </summary>
/// <param name="RatingLabel">
/// The rating as a rung: on its own ladder when that is known to differ from the item's, otherwise on the item's, as
/// the trajectory chart labels it.
/// </param>
/// <param name="OtherLadderName">The rating's ladder when it is known to differ from the item's pin, otherwise null.</param>
/// <param name="AgainstYearTarget">
/// The rating against this year's target, by the same rule as the decision: one assessor's view of one encounter, not
/// an entrustment decision, shown beside it.
/// </param>
public sealed record StandingRatingDto(
    int ActivityId,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    int Rating,
    string RatingLabel,
    string? OtherLadderName,
    string Source,
    EntrustmentStandingStatus AgainstYearTarget);

/// <summary>
/// The programme exit rule, "Level 5 in 9 EPAs and Level 4 in the remaining 6" for the CPSA v11.1 catalogue, read as
/// "every EPA's STAR decision at or above its own exit level". The two are the same statement because the catalogue
/// pins which nine and which six (<c>minimumLevelOrder</c> per EPA). The rule is the College's, so it is counted over
/// the national core items only; an institution's local items are not in it. Informational: recording a Graduate
/// decision or completing a programme does not consult it. (T166)
/// </summary>
/// <param name="EpaCount">How many EPAs the rule is counted over: the national core items in force.</param>
/// <param name="AtExitLevel">How many of them have a STAR decision at or above their exit level.</param>
/// <param name="Groups">The same count per exit level, highest first: "Level 5: 2 of 9".</param>
/// <param name="NotYetEpaCodes">The EPAs not yet at their exit level, by code.</param>
public sealed record ExitRuleReadinessDto(
    int EpaCount,
    int AtExitLevel,
    IReadOnlyList<ExitLevelGroupDto> Groups,
    IReadOnlyList<string> NotYetEpaCodes)
{
    /// <summary>Every EPA the rule counts is at its exit level. False when it counts none.</summary>
    public bool Met => EpaCount > 0 && AtExitLevel == EpaCount;
}

/// <summary>The EPAs that share one exit level, and how many of them the trainee holds it on. (T166)</summary>
public sealed record ExitLevelGroupDto(int LevelOrder, string LevelLabel, int EpaCount, int AtLevel);
