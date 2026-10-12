using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Domain.CommitteeDecisions;

/// <summary>Why an EPA is on a review's agenda. (T131 slice 4)</summary>
public enum CommitteeAgendaLineOrigin
{
    /// <summary>The EPA's decision cadence made it due at this sitting: planned when the review was scheduled or started.</summary>
    Cadence = 1,

    /// <summary>The chair staged a decision on an EPA the agenda did not hold, and routing gives it to this panel.</summary>
    Chair = 2
}

/// <summary>
/// Where an agenda line stands. "Staged" is not a state: it is read from the review's staged decision on the EPA, which
/// is unique per EPA. "Missed" is not one either: it is computed, never stored. (T131, Decision 6)
/// </summary>
/// <remarks>
/// There is no deferral (T361, D53): the value 2, Deferred, is gone, and the others keep their numbers, which a check
/// constraint names (<c>CK_CommitteeAgendaLines_DecidedNamesItsStar</c> uses 3).
/// </remarks>
public enum CommitteeAgendaLineState
{
    /// <summary>Still to be decided at this sitting.</summary>
    Due = 1,

    /// <summary>Ratifying issued a STAR on it (<see cref="CommitteeAgendaLine.EntrustmentDecisionId" />).</summary>
    Decided = 3,

    /// <summary>
    /// Not decided at this sitting: the review was ratified with nothing staged on the line, closing or optional alike
    /// (T361, D53, E1). The decision's one sentence about such lines is <see cref="CommitteeDecision.UndecidedNote" />. A
    /// later sitting in the window plans the line again.
    /// </summary>
    NotDecided = 4,

    /// <summary>
    /// Another sitting had decided the EPA in this window when the review settled its agenda, so it was not decided here:
    /// written when the committee's decision is recorded, or at ratify, for a line still due with nothing staged on it.
    /// Until then it is read live, and names no STAR of its own. (T235)
    /// </summary>
    DecidedElsewhere = 5
}

/// <summary>
/// One EPA a committee review is there to decide: the line of its agenda that makes a missed decision visible rather
/// than merely absent. (T131 slice 4, Decisions 4-6 and 9)
/// </summary>
/// <remarks>
/// <para>
/// <b>What puts a line here.</b> The trainee's in-force curriculum items that have a decision cadence
/// (<see cref="CurriculumItem.DecisionCadence" />) and route to the review's panel. The window is the item's cadence
/// window (<see cref="QuotaWindow" />) that holds the review's period: the review's own semester for a semester-cadence
/// EPA, both semesters of the year for an annual one. There is no calendar here but <see cref="AcademicPeriod" />'s.
/// </para>
/// <para>
/// <b>Closing</b> (<see cref="IsClosing" />): a line due at this sitting, the window's last chance. It no longer holds
/// anything back (T361, D53): left unstaged, it is written Not decided when the review is ratified, as an optional line
/// is, and the decisions-due page reads it so. A line is closing when the sitting is in the window's last semester: every semester-cadence
/// line, and an annual line at the semester-2 sitting (Decision 5), except a line the College has decided as opportunity
/// allows (<see cref="CurriculumItem.DecisionIsOpportunistic" />, O7) and a line in a window the trainee joined part-way
/// through (<see cref="IsPartialPeriod" />, Decision 9, D14/D42). An annual EPA reaches every binding sitting of its
/// year until it is decided, and until the year's last sitting it is optional ("Due by year end").
/// </para>
/// <para>
/// <b>Frozen.</b> The EPA's code and title are copied when the line is written, as the evidence snapshot's lines are
/// (T167): the agenda says what the sitting was asked, whatever happens to the catalogue after. The curriculum item and
/// the EPA are ids with no foreign key for the same reason. Lines are planned at schedule and at Start and then only
/// change state; routing or curriculum changes after Start reach the next review.
/// </para>
/// <para>
/// <b>Decided elsewhere</b> (T235). A line is planned because its window was undecided, and another sitting can decide it
/// while this review is open: a late semester-1 review ratifying an annual EPA that the semester-2 review holds as closing.
/// Such a line is optional: whether another sitting has decided
/// the window is read, by the one predicate the planner and the decisions-due page share, whenever the line is judged,
/// and written as <see cref="CommitteeAgendaLineState.DecidedElsewhere" /> once the review settles its agenda.
/// </para>
/// <para>
/// <b>No user id</b>, so erasure has nothing here to pseudonymise.
/// </para>
/// </remarks>
public sealed class CommitteeAgendaLine
{
    public const int EpaCodeMaxLength = 64;
    public const int EpaTitleMaxLength = 200;

    private CommitteeAgendaLine()
    {
    }

    public int Id { get; private set; }
    public int ReviewId { get; private set; }

    /// <summary>The curriculum item the line was planned from. No foreign key: the line is a frozen record.</summary>
    public int CurriculumItemId { get; private set; }

    /// <summary>The EPA. Unique per review. No foreign key, as on the evidence snapshot.</summary>
    public int EpaId { get; private set; }

    public string EpaCode { get; private set; } = string.Empty;
    public string EpaTitle { get; private set; } = string.Empty;
    public CommitteeAgendaLineOrigin Origin { get; private set; }

    /// <summary>The academic year of the decision window.</summary>
    public int WindowYear { get; private set; }

    /// <summary>The semester of the decision window, or null for a whole-year window (an annual EPA).</summary>
    public int? WindowSemester { get; private set; }

    /// <summary>
    /// Whether the line is due at this sitting, the window's last semester (Decision 5). Since D53 it labels the line and
    /// tells the decisions-due page a not-decided window apart; it holds nothing back (T361).
    /// </summary>
    public bool IsClosing { get; private set; }

    /// <summary>
    /// Whether the trainee started part-way through the window (D14, D42): the line stays, labelled, optional, and never
    /// counts as missed (Decision 9).
    /// </summary>
    public bool IsPartialPeriod { get; private set; }

    public CommitteeAgendaLineState State { get; private set; } = CommitteeAgendaLineState.Due;

    /// <summary>The STAR ratifying issued on the line. Set exactly when <see cref="State" /> is Decided.</summary>
    public int? EntrustmentDecisionId { get; private set; }

    public CommitteeReview Review { get; private set; } = null!;
    public EntrustmentDecision? EntrustmentDecision { get; private set; }

    /// <summary>Whether the window is a whole academic year.</summary>
    public bool IsYearWindow => WindowSemester is null;

    /// <summary>The window's semesters, in order.</summary>
    public IReadOnlyList<AcademicPeriod> WindowSemesters => WindowSemester is int semester
        ? [new AcademicPeriod(WindowYear, semester)]
        : AcademicPeriod.SemestersOf(WindowYear);

    /// <summary>The window's last counted day (31 December for a window ending in semester 2).</summary>
    public DateOnly WindowEnd => WindowSemesters[^1].End;

    /// <summary>"2026 S1" for a semester window, "2026" for a year.</summary>
    public string WindowLabel => WindowLabelOf(WindowYear, WindowSemester);

    /// <summary>The one wording of a decision window.</summary>
    public static string WindowLabelOf(int year, int? semester)
        => semester is int s ? new AcademicPeriod(year, s).ToString() : year.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Whether an item decided on <paramref name="window" /> is due at a sitting for <paramref name="sitting" />: the
    /// window holds the sitting, and the trainee had started the programme by the window's end.
    /// </summary>
    public static bool IsDueAt(QuotaWindow window, AcademicPeriod sitting)
    {
        ArgumentNullException.ThrowIfNull(window);
        return window.Covers(sitting.Year, sitting.Semester) && window.Status != QuotaWindowStatus.NotStarted;
    }

    /// <summary>
    /// Whether a line on <paramref name="window" /> is closing at a sitting for <paramref name="sitting" />: the sitting
    /// is in the window's last semester, the item is not decided as opportunity allows, and the trainee did not join the
    /// window part-way through. The one statement of Decisions 5 and 9 and of O7.
    /// </summary>
    public static bool ClosesAt(QuotaWindow window, AcademicPeriod sitting, bool isOpportunistic)
    {
        ArgumentNullException.ThrowIfNull(window);
        return !isOpportunistic &&
               window.Status == QuotaWindowStatus.Counting &&
               window.Semesters[^1] == sitting;
    }

    /// <summary>A line the item's cadence makes due at this sitting.</summary>
    /// <exception cref="ArgumentException">The item is not due at this sitting (<see cref="IsDueAt" />).</exception>
    public static CommitteeAgendaLine ForCadence(
        int curriculumItemId,
        int epaId,
        string epaCode,
        string epaTitle,
        bool isOpportunistic,
        QuotaWindow window,
        AcademicPeriod sitting)
    {
        if (!IsDueAt(window, sitting))
        {
            throw new ArgumentException(
                $"{epaCode} is not due at a sitting for {sitting}: its window does not hold the sitting, or the trainee had not started.",
                nameof(window));
        }

        return Create(
            CommitteeAgendaLineOrigin.Cadence,
            curriculumItemId,
            epaId,
            epaCode,
            epaTitle,
            window,
            ClosesAt(window, sitting, isOpportunistic));
    }

    /// <summary>
    /// A line the chair adds by staging a decision on an EPA the agenda does not hold. Never closing: the chair chose to
    /// decide it here, and a staged line is decided at ratify.
    /// </summary>
    public static CommitteeAgendaLine ForChair(
        int curriculumItemId,
        int epaId,
        string epaCode,
        string epaTitle,
        QuotaWindow window)
        => Create(CommitteeAgendaLineOrigin.Chair, curriculumItemId, epaId, epaCode, epaTitle, window, isClosing: false);

    /// <summary>Ratifying issued <paramref name="decision" /> on the line. Through the navigation: the STAR has no id until saved.</summary>
    internal void Decide(EntrustmentDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (State != CommitteeAgendaLineState.Due)
        {
            throw new InvalidOperationException($"{EpaCode} is not due at this review, so no decision can be recorded on it.");
        }

        State = CommitteeAgendaLineState.Decided;
        EntrustmentDecision = decision;
        if (decision.Id > 0)
        {
            EntrustmentDecisionId = decision.Id;
        }
    }

    /// <summary>
    /// Another sitting decided the line's window, so the review settles it without deciding it here (T235): when the
    /// committee's decision is recorded, or at ratify. Only a line still due.
    /// </summary>
    internal void SettleDecidedElsewhere()
    {
        if (State != CommitteeAgendaLineState.Due)
        {
            throw new InvalidOperationException($"{EpaCode} is not due at this review, so it cannot be settled as decided elsewhere.");
        }

        State = CommitteeAgendaLineState.DecidedElsewhere;
    }

    /// <summary>The review was ratified with nothing staged on this line, closing or optional alike (T361, D53).</summary>
    internal void CloseUndecided()
    {
        if (State != CommitteeAgendaLineState.Due)
        {
            throw new InvalidOperationException($"{EpaCode} is not due at this review, so it cannot be closed as not decided.");
        }

        State = CommitteeAgendaLineState.NotDecided;
    }

    private static CommitteeAgendaLine Create(
        CommitteeAgendaLineOrigin origin,
        int curriculumItemId,
        int epaId,
        string epaCode,
        string epaTitle,
        QuotaWindow window,
        bool isClosing)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (curriculumItemId <= 0 || epaId <= 0)
        {
            throw new ArgumentException("An agenda line names a curriculum item and an EPA.");
        }

        if (string.IsNullOrWhiteSpace(epaCode))
        {
            throw new ArgumentException("An agenda line names its EPA's code.", nameof(epaCode));
        }

        return new CommitteeAgendaLine
        {
            Origin = origin,
            CurriculumItemId = curriculumItemId,
            EpaId = epaId,
            EpaCode = Clip(epaCode.Trim(), EpaCodeMaxLength),
            EpaTitle = Clip((epaTitle ?? string.Empty).Trim(), EpaTitleMaxLength),
            WindowYear = window.AcademicYear,
            WindowSemester = window.Kind == QuotaPeriod.Semester ? window.Semesters[0].Semester : null,
            IsClosing = isClosing,
            IsPartialPeriod = window.Status == QuotaWindowStatus.ExemptPartialPeriod,
            State = CommitteeAgendaLineState.Due
        };
    }

    private static string Clip(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}
