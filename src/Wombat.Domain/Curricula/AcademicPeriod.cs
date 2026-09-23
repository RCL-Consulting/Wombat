namespace Wombat.Domain.Curricula;

/// <summary>
/// One semester of the national academic year: the bucket an encounter's credit is counted in (T130).
/// </summary>
/// <remarks>
/// <para>
/// <b>The calendar (D13, D40).</b> The College runs one national academic year, January to November, with
/// the semester boundary in June. Semester 1 is 1 January to 30 June; semester 2 is 1 July to 31 December.
/// Two readings in that are this repository's, not the College's. Each lives in exactly one place:
/// </para>
/// <list type="bullet">
/// <item><b>June is semester 1.</b> The College said "boundary in June" without saying which side June falls
/// on, or on which day. <see cref="SecondSemesterStartMonth" /> and <see cref="SecondSemesterStartDay" /> are
/// the only place that reading lives, and a mid-June answer is expressible. If the boundary moves, a
/// rebuild re-buckets every encounter: the bucket is recomputed from <c>Activity.ObservedOn</c>, and rows the
/// replay does not reproduce are removed.</item>
/// <item><b>December folds into semester 2.</b> December is outside the January–November teaching year, but
/// nothing stops an encounter happening in it, and an encounter is still evidence. <see cref="Containing" />
/// must be total over <see cref="DateOnly" /> because it feeds credit. The bucket runs to 31 December, and
/// <see cref="NominalEnd" /> keeps the College's own end date for what a reader is told.</item>
/// </list>
/// <para>
/// <b>Not a training year.</b> <c>TraineeProfile.GetStage</c> counts 365-day blocks from each trainee's own
/// programme start, and it selects the per-stage minimum level. A period is a fixed calendar window shared by
/// every registrar in the country, and it selects the bucket. The two disagree by construction (D17): one
/// answers which target applies to this trainee, the other which bucket this encounter falls in. Never derive
/// either one from the other.
/// </para>
/// <para>
/// Not a committee review's <c>ReviewPeriodFrom</c>/<c>ReviewPeriodTo</c> either, which is an arbitrary window
/// chosen per review. That is why this type is not called "Period".
/// </para>
/// </remarks>
public readonly record struct AcademicPeriod
{
    /// <summary>
    /// The month semester 2 begins in. With <see cref="SecondSemesterStartDay" /> this is the register's reading
    /// of D13's "boundary in June": June belongs to semester 1. The College has not yet confirmed it.
    /// </summary>
    public const int SecondSemesterStartMonth = 7;

    /// <summary>The day of <see cref="SecondSemesterStartMonth" /> semester 2 begins on.</summary>
    public const int SecondSemesterStartDay = 1;

    /// <summary>The last month of the College's academic year (D13: January to November).</summary>
    public const int LastTeachingMonth = 11;

    public AcademicPeriod(int year, int semester)
    {
        if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "The year must be a representable calendar year.");
        }

        if (semester is not (1 or 2))
        {
            throw new ArgumentOutOfRangeException(nameof(semester), semester, "An academic year has two semesters: 1 and 2.");
        }

        Year = year;
        Semester = semester;
    }

    /// <summary>The academic year, which is the calendar year it runs in.</summary>
    public int Year { get; }

    /// <summary>1 or 2.</summary>
    public int Semester { get; }

    /// <summary>The first day of the semester.</summary>
    public DateOnly Start => Semester == 1 ? new DateOnly(Year, 1, 1) : SecondSemesterStart(Year);

    /// <summary>The last day of the bucket, inclusive. For semester 2 this is 31 December (the December fold).</summary>
    public DateOnly End => Semester == 1 ? SecondSemesterStart(Year).AddDays(-1) : new DateOnly(Year, 12, 31);

    /// <summary>
    /// The last day of the semester on the College's calendar. That is 30 November for semester 2, because D13
    /// ends the academic year in November; it equals <see cref="End" /> for semester 1. This is what a reader
    /// is told. Counting still runs to <see cref="End" />.
    /// </summary>
    public DateOnly NominalEnd => Semester == 1
        ? End
        : new DateOnly(Year, LastTeachingMonth, 1).AddMonths(1).AddDays(-1);

    /// <summary>The first day of semester 2 in <paramref name="year" />.</summary>
    public static DateOnly SecondSemesterStart(int year)
        => new(year, SecondSemesterStartMonth, SecondSemesterStartDay);

    /// <summary>
    /// The semester a date falls in. Total over every <see cref="DateOnly" />: it never throws, and every date
    /// belongs to exactly one semester. It runs inside credit, after the terminal transition has been decided,
    /// so a throw here would be a throw in the middle of a completion.
    /// </summary>
    public static AcademicPeriod Containing(DateOnly date)
        => new(date.Year, date < SecondSemesterStart(date.Year) ? 1 : 2);

    /// <summary>Both semesters of one academic year, in order.</summary>
    public static IReadOnlyList<AcademicPeriod> SemestersOf(int year) => [new(year, 1), new(year, 2)];

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    /// <summary>
    /// The following semester, or null after semester 2 of the last representable year. It returns null rather
    /// than throwing, because a reader asking "when does counting start" must not crash a page.
    /// </summary>
    public AcademicPeriod? Next()
    {
        if (Semester == 1)
        {
            return new AcademicPeriod(Year, 2);
        }

        return Year == DateOnly.MaxValue.Year ? null : new AcademicPeriod(Year + 1, 1);
    }

    /// <summary>The preceding semester, or null before semester 1 of year 1.</summary>
    public AcademicPeriod? Previous()
    {
        if (Semester == 2)
        {
            return new AcademicPeriod(Year, 1);
        }

        return Year == DateOnly.MinValue.Year ? null : new AcademicPeriod(Year - 1, 2);
    }

    public override string ToString() => $"{Year} S{Semester}";
}
