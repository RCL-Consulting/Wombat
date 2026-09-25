namespace Wombat.Domain.Curricula;

/// <summary>
/// Whether a trainee is held to a target in a given window (T130, D14; T209, D49).
/// </summary>
public enum QuotaWindowStatus
{
    /// <summary>The trainee was in the programme in time for this window, so the target applies.</summary>
    Counting = 0,

    /// <summary>
    /// The trainee started part-way through the window. D14 exempts the partial period, and counting starts at
    /// the next boundary. Encounters in the window are still credited and still shown. Only the target is
    /// waived, and they do not carry into the next window.
    /// </summary>
    ExemptPartialPeriod = 1,

    /// <summary>The whole window lies before the trainee's programme start, or the day asked about does.</summary>
    NotStarted = 2,

    /// <summary>
    /// The programme ended (completion or deactivation) inside the window, before the window's last month. D49 exempts
    /// the window, as D14 exempts a late start's first one. Encounters in it are still credited and still shown. Only the
    /// target is waived.
    /// </summary>
    ExemptProgrammeEnded = 3,

    /// <summary>
    /// The whole window lies after the programme ended. It is outside the programme, so it holds no target and is never
    /// short (D49). Anything credited in it is still shown.
    /// </summary>
    AfterProgrammeEnd = 4
}

/// <summary>
/// The window a curriculum item's target is measured over on a given day, and whether the target applies to
/// this trainee. This is the one implementation of the College's D14 rule, and of D49, its mirror at the programme's end.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why D14 is applied when progress is read, not when credit is written.</b> Credit always lands in the
/// semester that contains the encounter, whoever the trainee is. Applying the exemption at write time would
/// make the stored buckets depend on <c>TraineeProfile.ProgrammeStartDate</c>, which an administrator can
/// change in place with no rebuild. A suppressed credit would also stamp <c>CreditedItemCount = 0</c>, which
/// raises the "this credited nothing" banner and blames the curriculum for a perfectly good encounter.
/// Evidence is never discarded. The target is waived.
/// </para>
/// <para>
/// <b>What "part-way through" means (D42, provisional, awaiting the College).</b> D14 was asked about "a
/// registrar who starts mid-year" and does not say how late a start has to be. Read literally, a start one
/// day after a boundary exempts the whole period. That would take a year's annual targets away from a
/// registrar who started on the first working day of January, because 1 January is a public holiday.
/// So the rule is:
/// </para>
/// <list type="bullet">
/// <item><b>Semester targets.</b> A start within the semester's first calendar month counts as starting on the
/// boundary. A later start exempts the semester. The month is an invented tolerance: it absorbs the gap
/// between a post's contractual start on the 1st and the day the registrar is actually recorded as starting,
/// and nothing more.</item>
/// <item><b>Academic-year targets.</b> Only a start in semester 2 exempts the year. That is the College's own
/// meaning of "mid-year" and its own boundary, so no number is invented. A registrar who starts in April has
/// eight months for a one-per-annum EPA.</item>
/// </list>
/// <para>
/// Both rules live in <see cref="LatestOnTimeStart" />. Changing them needs no rebuild.
/// </para>
/// <para>
/// <b>The end of the programme (D49, provisional, T209).</b> A programme that completion or deactivation ends inside a
/// window, before the window's last month, is exempt for it, as a late start is for its first window: the College
/// published no figure for part of a period, and pro-rating would invent one. An end in the window's last month (June
/// for semester 1, November for semester 2 and for the academic year, on the College's calendar) or later holds the full
/// target. A window that starts after the end is outside the programme: never short. The end is the day the programme
/// actually ended (<c>TraineeProfile.EndedOn</c>), never the expected completion date, which is only a plan. The rule
/// lives in <see cref="EarliestFullPeriodEnd" />. Unlike D42 it is the same for both kinds, as D49 words it: an
/// academic-year target is waived for an end in October, although D42 holds a start in April to one.
/// </para>
/// </remarks>
public sealed record QuotaWindow(
    QuotaPeriod Kind,
    IReadOnlyList<AcademicPeriod> Semesters,
    QuotaWindowStatus Status,
    AcademicPeriod? FirstCountedPeriod)
{
    /// <summary>The window's first day.</summary>
    public DateOnly Start => Semesters[0].Start;

    /// <summary>The window's last counted day, inclusive (31 December for a window ending in semester 2).</summary>
    public DateOnly End => Semesters[^1].End;

    /// <summary>The window's last day on the College's calendar (30 November for a window ending in semester 2).</summary>
    public DateOnly NominalEnd => Semesters[^1].NominalEnd;

    /// <summary>The academic year the window belongs to.</summary>
    public int AcademicYear => Semesters[0].Year;

    /// <summary>True when the stored semester bucket (<paramref name="year" />, <paramref name="semester" />) lies inside this window.</summary>
    public bool Covers(int year, int semester)
        => Semesters.Any(period => period.Year == year && period.Semester == semester);

    /// <summary>
    /// The window of <paramref name="kind" /> that contains <paramref name="day" />, judged for a trainee who
    /// started the programme on <paramref name="programmeStart" /> and, when it has ended, left it on
    /// <paramref name="programmeEnd" />. Total: it never throws for any dates, in any order.
    /// </summary>
    /// <param name="programmeEnd">
    /// The day the programme actually ended (<c>TraineeProfile.EndedOn</c>), or null while it is running. Null reproduces
    /// the rule before D49 exactly, which is what a caller that reads only running programmes, or a committee's decision
    /// cadence rather than a target, passes.
    /// </param>
    public static QuotaWindow For(QuotaPeriod kind, DateOnly day, DateOnly programmeStart, DateOnly? programmeEnd = null)
    {
        var semesters = SemestersOfWindowContaining(kind, day);
        var firstCounted = FirstCountedPeriodFor(kind, programmeStart, programmeEnd);

        if (programmeEnd is { } lastDay && semesters[0].Start > lastDay)
        {
            return new QuotaWindow(kind, semesters, QuotaWindowStatus.AfterProgrammeEnd, firstCounted);
        }

        if (day < programmeStart)
        {
            return new QuotaWindow(kind, semesters, QuotaWindowStatus.NotStarted, firstCounted);
        }

        if (programmeStart > LatestOnTimeStart(kind, semesters))
        {
            return new QuotaWindow(kind, semesters, QuotaWindowStatus.ExemptPartialPeriod, firstCounted);
        }

        if (programmeEnd is { } end && end < EarliestFullPeriodEnd(semesters))
        {
            return new QuotaWindow(kind, semesters, QuotaWindowStatus.ExemptProgrammeEnded, firstCounted);
        }

        return new QuotaWindow(kind, semesters, QuotaWindowStatus.Counting, firstCounted);
    }

    /// <summary>
    /// The window of the same kind immediately before this one, judged for the same trainee, or null at the start of time.
    /// Pass the <paramref name="programmeEnd" /> this window was judged with.
    /// </summary>
    public QuotaWindow? Preceding(DateOnly programmeStart, DateOnly? programmeEnd = null)
    {
        var previous = Semesters[0].Previous();
        return previous is null ? null : For(Kind, previous.Value.End, programmeStart, programmeEnd);
    }

    /// <summary>
    /// The latest programme start that still counts as starting on time for the window: the one place D42's
    /// tolerance lives.
    /// </summary>
    public static DateOnly LatestOnTimeStart(QuotaPeriod kind, IReadOnlyList<AcademicPeriod> window)
        => kind == QuotaPeriod.Semester
            ? window[0].Start.AddMonths(1).AddDays(-1)
            : AcademicPeriod.SecondSemesterStart(window[0].Year).AddDays(-1);

    /// <summary>
    /// The earliest programme end that still holds a window to its full target: the first day of the window's last month
    /// on the College's calendar (1 June for semester 1; 1 November for semester 2 and for the academic year, whose
    /// December is folded in, D40). An earlier end inside the window exempts it. The one place D49's rule lives, beside
    /// <see cref="LatestOnTimeStart" />.
    /// </summary>
    public static DateOnly EarliestFullPeriodEnd(IReadOnlyList<AcademicPeriod> window)
    {
        var lastDay = window[^1].NominalEnd;
        return new DateOnly(lastDay.Year, lastDay.Month, 1);
    }

    /// <summary>
    /// The first semester of the first window of <paramref name="kind" /> whose target applies to a trainee who
    /// started on <paramref name="programmeStart" />. Null when no later window is representable, and when the programme
    /// ended (<paramref name="programmeEnd" />) before that window's last month: no window of this kind ever held the
    /// trainee to a target, so there is no "targets start with" to say.
    /// </summary>
    public static AcademicPeriod? FirstCountedPeriodFor(QuotaPeriod kind, DateOnly programmeStart, DateOnly? programmeEnd = null)
    {
        var containing = SemestersOfWindowContaining(kind, programmeStart);
        var first = programmeStart <= LatestOnTimeStart(kind, containing)
            ? containing[0]
            : containing[^1].Next();

        if (first is { } period && programmeEnd is { } end &&
            end < EarliestFullPeriodEnd(SemestersOfWindowContaining(kind, period.Start)))
        {
            return null;
        }

        return first;
    }

    private static IReadOnlyList<AcademicPeriod> SemestersOfWindowContaining(QuotaPeriod kind, DateOnly day)
        => kind == QuotaPeriod.Semester
            ? [AcademicPeriod.Containing(day)]
            : AcademicPeriod.SemestersOf(day.Year);
}
