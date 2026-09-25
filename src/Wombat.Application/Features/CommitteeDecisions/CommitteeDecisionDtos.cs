using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record DecisionPanelMemberInput(string UserId, DecisionPanelMemberRole Role);

public sealed record DecisionPanelMemberDto(
    int Id,
    string UserId,
    DecisionPanelMemberRole Role);

/// <param name="DecisionBodyKey">The College committee the panel sits as (T131), or null for a general panel.</param>
/// <param name="DecisionBodyName">That committee's name, or null for a general panel.</param>
public sealed record DecisionPanelSummaryDto(
    int Id,
    string Name,
    DecisionPanelScope Scope,
    int? InstitutionId,
    int? SpecialityId,
    int MemberCount,
    string? DecisionBodyKey = null,
    string? DecisionBodyName = null);

/// <param name="DecisionBodyKey">The College committee the panel sits as (T131), or null for a general panel.</param>
/// <param name="DecisionBodyName">That committee's name, or null for a general panel.</param>
public sealed record DecisionPanelDetailDto(
    int Id,
    string Name,
    DecisionPanelScope Scope,
    int? InstitutionId,
    int? SpecialityId,
    IReadOnlyList<DecisionPanelMemberDto> Members,
    string? DecisionBodyKey = null,
    string? DecisionBodyName = null);

public sealed record CommitteeReviewListItemDto(
    int Id,
    string TraineeUserId,
    int PanelId,
    string PanelName,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    CommitteeReviewState State,
    CommitteeDecisionCategory? CurrentDecisionCategory,
    DateTime? RatifiedOn,
    bool IsFormative = false,
    CommitteeReviewType ReviewType = CommitteeReviewType.AnnualProgression)
{
    /// <summary>The academic year of the period the review sits for (T131, Decision 4).</summary>
    public required int AcademicYear { get; init; }

    /// <summary>The semester, 1 or 2, of the period the review sits for.</summary>
    public required int Semester { get; init; }

    /// <summary>The period the review sits for: "2026 S1".</summary>
    public string PeriodLabel => $"{AcademicYear} S{Semester}";

    /// <summary>
    /// Whether the review's decision records a progression category: false for an entrustment-only review (T131 slice 5),
    /// whose <see cref="CurrentDecisionCategory" /> is null once decided as well as before.
    /// </summary>
    public bool DecidesProgression => CommitteeReviewTypes.DecidesProgression(ReviewType);

    /// <summary>
    /// Whether the committee has recorded its decision: a summative review that is decided, ratified, under appeal or
    /// final. Said apart from <see cref="CurrentDecisionCategory" />, which is null on an entrustment-only review's
    /// decision too (T131 slice 5).
    /// </summary>
    public bool HasDecision => !IsFormative && State is CommitteeReviewState.Decided or CommitteeReviewState.Ratified
        or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final;

    /// <summary>
    /// Whose review it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// schedule's Trainee column (T142). Filled by <c>ListReviewsForPanelQuery</c> in one lookup for the page. Null from
    /// the trainee's own list and the chair's, which do not show it, and from scheduling.
    /// </summary>
    public string? TraineeName { get; init; }
}

/// <param name="Category">
/// The progression outcome, or null on an entrustment-only review's decision, whose decision is the STARs staged at it
/// (T131 slice 5).
/// </param>
public sealed record CommitteeDecisionDto(
    int Id,
    CommitteeDecisionCategory? Category,
    string Rationale,
    string? Conditions,
    DateTime DecidedOn,
    string DecidedByChairUserId,
    int? SupersedesDecisionId)
{
    /// <summary>
    /// Who was recorded as present when this decision was taken, chair first, with the role each held then (T165).
    /// Each decision keeps its own sitting: a decision an appeal remitted names whoever sat for the appeal.
    /// </summary>
    public IReadOnlyList<CommitteePersonDto> Attendees { get; init; } = [];
}

public sealed record CommitteeAppealDto(
    int Id,
    DateTime LodgedOn,
    string LodgedByUserId,
    string Reason,
    DateTime? ResolvedOn,
    string? ResolvedByUserId,
    CommitteeAppealOutcome? Outcome);

/// <summary>
/// One frozen line of a review's evidence snapshot. The parameters from <paramref name="EpaId" /> on are T167's and are
/// null on a line frozen before it, and on an MSF campaign's line, which reports across EPAs.
/// </summary>
/// <param name="EpaId">The EPA the activity is evidence for (its stamped <c>Activity.EpaId</c>), or null.</param>
/// <param name="EpaCode">That EPA's code when the review started.</param>
/// <param name="EpaTitle">That EPA's title when the review started.</param>
/// <param name="InstrumentKey">The type's <c>WbaToolKey</c>, or null when it declares none.</param>
/// <param name="InstrumentName">The instrument by name, else the activity type's name.</param>
/// <param name="IsRatedInstrument">
/// Whether the pinned version rates the trainee at all. False is an unrated instrument; true with no
/// <paramref name="RatingLabel" /> is a rating nobody recorded.
/// </param>
/// <param name="RatingOrder">The rung as the stored ordinal.</param>
/// <param name="RatingLabel">The rung as a clinician reads it ("3a"), else the bare ordinal.</param>
/// <param name="ObservedOn">When the encounter happened.</param>
/// <param name="ObservedOnDeclared">
/// False when nobody stated an encounter date and <paramref name="ObservedOn" /> is only the day the activity was
/// created (T161, T197).
/// </param>
/// <param name="SourceState">The activity's workflow state, or the campaign's state, when the review started.</param>
/// <param name="SourceFinished">
/// Whether that was finished work: a terminal state of the activity's pinned workflow (D44), or a released campaign. Null
/// on a line frozen before T131. The page says when every item a staged decision names was unfinished; nothing refuses it.
/// </param>
/// <param name="AssessorUserId">
/// Who the pinned version names as the assessor, as frozen at Start (T165); null when it names nobody. An id, used to
/// compare with the panel's chair, never shown.
/// </param>
public sealed record CommitteeEvidenceDto(
    int Id,
    CommitteeEvidenceSourceType SourceType,
    int? ActivityId,
    int? MsfCampaignId,
    int? SupervisorReportId,
    string SourceLabel,
    string Summary,
    DateTime? SourceRecordedOn,
    int? EpaId = null,
    string? EpaCode = null,
    string? EpaTitle = null,
    string? InstrumentKey = null,
    string? InstrumentName = null,
    bool? IsRatedInstrument = null,
    int? RatingOrder = null,
    string? RatingLabel = null,
    DateOnly? ObservedOn = null,
    bool? ObservedOnDeclared = null,
    string? SourceState = null,
    bool? SourceFinished = null,
    string? AssessorUserId = null)
{
    /// <summary>
    /// An activity line frozen before T167, which recorded none of the columns above: every activity line frozen since
    /// carries its encounter date. The page lists these as they were written rather than under an EPA they never named.
    /// </summary>
    public bool FrozenBeforeLinesNamedTheirEpa => SourceType == CommitteeEvidenceSourceType.Activity && ObservedOn is null;

    /// <summary>
    /// Whether a staged entrustment decision may name this line as evidence it rests on: the predicate the staging
    /// handler enforces, so the page's picker offers exactly those lines (D38, T131).
    /// </summary>
    public bool CanGroundADecision => Wombat.Application.Features.EntrustmentDecisions.StagedEvidence.CanGround(SourceType);
}

public sealed record CommitteeReviewDetailDto(
    int Id,
    string TraineeUserId,
    int PanelId,
    string PanelName,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    CommitteeReviewState State,
    DateTime? StartedOn,
    string? StartedByUserId,
    DateTime? RatifiedOn,
    string? RatifiedByUserId,
    DateTime? FinalizedOn,
    IReadOnlyList<CommitteeDecisionDto> Decisions,
    IReadOnlyList<CommitteeAppealDto> Appeals,
    IReadOnlyList<CommitteeEvidenceDto> EvidenceItems,
    bool IsFormative = false,
    CommitteeReviewType ReviewType = CommitteeReviewType.AnnualProgression)
{
    /// <summary>The academic year of the period the review sits for (T131, Decision 4).</summary>
    public required int AcademicYear { get; init; }

    /// <summary>The semester, 1 or 2, of the period the review sits for.</summary>
    public required int Semester { get; init; }

    /// <summary>The period the review sits for: "2026 S1".</summary>
    public string PeriodLabel => $"{AcademicYear} S{Semester}";

    /// <summary>
    /// Whether the review's decision records a progression category (T131 slice 5): false for an entrustment-only review,
    /// whose decision is the STARs staged at it. The page's decision and remit forms ask for a category only when true,
    /// which is when the handlers require one.
    /// </summary>
    public bool DecidesProgression => CommitteeReviewTypes.DecidesProgression(ReviewType);

    /// <summary>
    /// The review's agenda (T131 slice 4). Filled by <c>GetCommitteeReviewByIdQuery</c>, through the review's read ladder.
    /// Null from the commands that answer with the review: the mapper they share reads nothing, so the review page reads
    /// the agenda again (<c>GetCommitteeAgendaQuery</c>) after each action.
    /// </summary>
    public CommitteeAgendaDto? Agenda { get; init; }

    /// <summary>
    /// Whose review it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// review page's Trainee (T142). Filled by <c>GetCommitteeReviewByIdQuery</c>. Null from the commands that answer
    /// with the review: <c>CommitteeDecisionMappings.ToDetailDto</c> is shared with six commands and does no lookup, so
    /// the review page reads the review back through the query after each action.
    /// </summary>
    public string? TraineeName { get; init; }

    /// <summary>The rule a decision's attendance is held to, as the review page states it (T165).</summary>
    public static string QuorumRule => CommitteeReview.QuorumRule;

    /// <summary>
    /// The review's panel as it stands, chair first (T165). The record-decision and remit forms offer as present only
    /// those who may sit (<see cref="CommitteePersonDto.MaySit" />).
    /// </summary>
    public IReadOnlyList<CommitteePersonDto> PanelMembers { get; init; } = [];

    /// <summary>
    /// Why this panel cannot take a decision now, or null when it can: its chair may not sit, or fewer than
    /// <see cref="CommitteeReview.Quorum" /> of its members may (T165, <c>PanelSeat</c>). The page disables Record, and
    /// the remit form's Resolve, and shows this as the reason, rather than let the server refuse after the chair has
    /// written the decision.
    /// </summary>
    public string? PanelShortfall
    {
        get
        {
            var chair = PanelMembers.FirstOrDefault(member => member.Role == DecisionPanelMemberRole.Chair);
            if (chair is not { MaySit: true })
            {
                return "The panel's chair cannot be recorded as present: only an active committee member at the " +
                       "panel's institution can, and never the trainee under review. A panel administrator must name " +
                       "another chair.";
            }

            return PanelMembers.Count(member => member.MaySit) < CommitteeReview.Quorum
                ? "Only the chair can be recorded as present: a decision needs the chair and at least one other, each an " +
                  "active committee member at the panel's institution and none the trainee under review. A panel " +
                  "administrator must add a member."
                : null;
        }
    }

    /// <summary>
    /// Why the decision cannot be ratified for want of a quorum, or null when it can (<c>CommitteeReview.QuorumShortfall</c>,
    /// T165). Set only on a decided summative review: the page disables Ratify and shows this as its reason.
    /// </summary>
    public string? QuorumShortfall { get; init; }

    /// <summary>
    /// The panel's chair, when every rated line of the evidence snapshot names them as its assessor; otherwise null.
    /// </summary>
    /// <remarks>
    /// The College's summative decision is "never by a single assessor", and a committee whose chair is also the only
    /// assessor of the evidence is close to exactly that. T165 flags it on the review page rather than refusing it (D46):
    /// in a small programme it can be unavoidable, and the panel should weigh it, not be blocked by it. Read from the
    /// frozen snapshot (<see cref="CommitteeEvidenceDto.AssessorUserId" />), so it says what the panel weighed. A rated
    /// line that names nobody, an MSF's for one, is not the chair's, so it clears the flag.
    /// </remarks>
    public CommitteePersonDto? ChairRatedEveryLine
    {
        get
        {
            var chair = PanelMembers.FirstOrDefault(member => member.Role == DecisionPanelMemberRole.Chair);
            if (chair is null)
            {
                return null;
            }

            var rated = EvidenceItems.Where(item => item.RatingOrder is not null).ToArray();
            return rated.Length > 0 && rated.All(item => string.Equals(item.AssessorUserId, chair.UserId, StringComparison.Ordinal))
                ? chair
                : null;
        }
    }

    /// <summary>
    /// This review, named from an earlier copy of it: the trainee's name, the names of the panel and of those present at
    /// each decision, and which panel members may sit.
    /// </summary>
    /// <remarks>
    /// A command answers with the review from the lookup-free mapper, which names nobody and seats nobody (T142, T165).
    /// The review page and the trainee's list keep what they loaded with instead of reading the review back. Everyone
    /// recorded as present at a decision the page has just taken was a panel member when it loaded, so their names are
    /// among the panel's.
    /// </remarks>
    public CommitteeReviewDetailDto WithNamesFrom(CommitteeReviewDetailDto? earlier)
    {
        if (earlier is null)
        {
            return this;
        }

        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var person in earlier.PanelMembers.Concat(earlier.Decisions.SelectMany(decision => decision.Attendees)))
        {
            if (person.Name is not null)
            {
                names.TryAdd(person.UserId, person.Name);
            }
        }

        var seated = earlier.PanelMembers
            .Where(member => member.MaySit)
            .Select(member => member.UserId)
            .ToHashSet(StringComparer.Ordinal);

        CommitteePersonDto Named(CommitteePersonDto person)
            => person.Name is null && names.TryGetValue(person.UserId, out var name) ? person with { Name = name } : person;

        return this with
        {
            TraineeName = earlier.TraineeName,
            PanelMembers = PanelMembers
                .Select(Named)
                .Select(member => member with { MaySit = member.MaySit || seated.Contains(member.UserId) })
                .ToArray(),
            Decisions = Decisions
                .Select(decision => decision with { Attendees = decision.Attendees.Select(Named).ToArray() })
                .ToArray()
        };
    }
}

/// <summary>
/// A person on a review's panel, or recorded as present at its decision, with the role they held (T165).
/// </summary>
public sealed record CommitteePersonDto(string UserId, DecisionPanelMemberRole Role)
{
    /// <summary>
    /// Their name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />), filled by
    /// <c>GetCommitteeReviewByIdQuery</c>; null from the commands, whose mapper names nobody.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// A panel member only: whether they may be recorded as present at this review now, an active committee member at
    /// the panel's institution who is not the trainee under review (<c>PanelSeat</c>, T165). Filled by
    /// <c>GetCommitteeReviewByIdQuery</c> while the review can still take a decision; false everywhere else, so nobody is
    /// offered as present on the strength of a copy that never asked.
    /// </summary>
    public bool MaySit { get; init; }

    /// <summary>The name, or the id only where no name could be found (T142).</summary>
    public string DisplayName => Name ?? UserId;

    /// <summary>The name with the role a reader needs beside it: "Thandi Zulu (chair)", "Ann Lee (external)".</summary>
    public string Label => Role switch
    {
        DecisionPanelMemberRole.Chair => $"{DisplayName} (chair)",
        DecisionPanelMemberRole.External => $"{DisplayName} (external)",
        _ => DisplayName
    };
}
