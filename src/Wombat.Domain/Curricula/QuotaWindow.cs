namespace Wombat.Domain.Curricula;

/// <summary>
/// Whether a trainee is held to a target in a given window (T130, D14).
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
    NotStarted = 2
}

/// <summary>
/// The window a curriculum item's target is measured over on a given day, and whether the target applies to
/// this trainee. This is the one implementation of the College's D14 rule.
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
    /// started the programme on <paramref name="programmeStart" />. Total: it never throws for any pair of dates.
    /// </summary>
    public static QuotaWindow For(QuotaPeriod kind, DateOnly day, DateOnly programmeStart)
    {
        var semesters = SemestersOfWindowContaining(kind, day);
        var firstCounted = FirstCountedPeriodFor(kind, programmeStart);

        if (day < programmeStart)
        {
            return new QuotaWindow(kind, semesters, QuotaWindowStatus.NotStarted, firstCounted);
        }

        if (programmeStart > LatestOnTimeStart(kind, semesters))
        {
            return new QuotaWindow(kind, semesters, QuotaWindowStatus.ExemptPartialPeriod, firstCounted);
        }

        return new QuotaWindow(kind, semesters, QuotaWindowStatus.Counting, firstCounted);
    }

    /// <summary>The window of the same kind immediately before this one, judged for the same trainee, or null at the start of time.</summary>
    public QuotaWindow? Preceding(DateOnly programmeStart)
    {
        var previous = Semesters[0].Previous();
        return previous is null ? null : For(Kind, previous.Value.End, programmeStart);
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
    /// The first semester of the first window of <paramref name="kind" /> whose target applies to a trainee who
    /// started on <paramref name="programmeStart" />. Null only when no later window is representable.
    /// </summary>
    public static AcademicPeriod? FirstCountedPeriodFor(QuotaPeriod kind, DateOnly programmeStart)
    {
        var containing = SemestersOfWindowContaining(kind, programmeStart);
        if (programmeStart <= LatestOnTimeStart(kind, containing))
        {
            return containing[0];
        }

        return containing[^1].Next();
    }

    private static IReadOnlyList<AcademicPeriod> SemestersOfWindowContaining(QuotaPeriod kind, DateOnly day)
        => kind == QuotaPeriod.Semester
            ? [AcademicPeriod.Containing(day)]
            : AcademicPeriod.SemestersOf(day.Year);
}
