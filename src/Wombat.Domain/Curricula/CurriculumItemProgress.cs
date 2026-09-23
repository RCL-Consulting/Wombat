namespace Wombat.Domain.Curricula;

/// <summary>
/// One trainee's credited tally on one curriculum item in one semester (T130).
/// </summary>
/// <remarks>
/// <para>
/// Before T130 this was one lifetime row per (item, trainee). It is now one row per (item, trainee,
/// <see cref="AcademicPeriod" />), and the semester is always the one containing <c>Activity.ObservedOn</c>.
/// The row key depends on the encounter date and on nothing else: not on the item's
/// <see cref="CurriculumItem.QuotaPeriod" />, not on its target, not on the trainee's programme start. That
/// is what keeps a rebuild deterministic and stops rows being stranded when an administrator edits either of
/// those. A per-year figure is the sum of two rows. The D14 exemption is applied by readers.
/// </para>
/// <para>
/// <see cref="AcademicYear" /> and <see cref="Semester" /> are <c>required</c> so that no writer and no test
/// fixture can build a row without deciding which bucket it is in. Postgres would refuse the (0, 0) a default
/// would leave, but the in-memory provider the unit tests run on would not.
/// </para>
/// </remarks>
public sealed class CurriculumItemProgress
{
    public int Id { get; set; }
    public int CurriculumItemId { get; set; }
    public string TraineeUserId { get; set; } = string.Empty;

    /// <summary>The academic year of the semester this row counts, which is the calendar year it runs in.</summary>
    public required int AcademicYear { get; set; }

    /// <summary>1 or 2 (<see cref="AcademicPeriod" />). Enforced in the database by a CHECK constraint.</summary>
    public required int Semester { get; set; }

    public int CountsSoFar { get; set; }
    public int MinimumLevelReachedCount { get; set; }

    /// <summary>
    /// The entrustment scale the comparisons behind <see cref="MinimumLevelReachedCount" /> were actually
    /// made on, or null when no scale-verified comparison has ever contributed to it (T109).
    /// </summary>
    /// <remarks>
    /// Deliberately not redundant with <c>CurriculumItem.ScaleId</c>. If an administrator later re-pins the
    /// item, this row still records what the stored tally was computed against — which is the only thing
    /// that makes a now-stale tally detectable rather than merely wrong.
    /// </remarks>
    public int? MinimumLevelScaleId { get; set; }

    /// <summary>
    /// Completions that counted toward <see cref="CountsSoFar" /> but were refused the minimum because the
    /// assessment's ladder and the curriculum item's ladder were both known and different (T109).
    /// </summary>
    public int ScaleMismatchCount { get; set; }

    /// <summary>
    /// Completions whose level comparison was made without scale proof on at least one side — the
    /// pre-T109 behaviour, preserved. The exposure meter: a non-zero value says this tally rests on
    /// ordinals nobody has verified are on the same ladder.
    /// </summary>
    public int UnverifiedLevelCount { get; set; }

    public int? LastActivityId { get; set; }

    /// <summary>
    /// The latest encounter date (<c>Activity.ObservedOn</c>) among the completions this row counts, or null for
    /// a row nothing has credited yet.
    /// </summary>
    /// <remarks>
    /// This is what a reader shows as "last encounter". <see cref="LastUpdated" /> is an audit clock: a rebuild
    /// rewrites it on every row it touches, so after a rebuild every item would read "last credited" on the day
    /// of the rebuild. This value is a function of the credited activities alone, so a rebuild reproduces it
    /// exactly.
    /// </remarks>
    public DateOnly? LastObservedOn { get; set; }

    public DateTime LastUpdated { get; set; }
    public string CreditedActivityKeysJson { get; set; } = "[]";

    public CurriculumItem CurriculumItem { get; set; } = null!;

    /// <summary>True when this row is the bucket for <paramref name="period" />.</summary>
    public bool IsIn(AcademicPeriod period) => AcademicYear == period.Year && Semester == period.Semester;
}

/// <summary>
/// The natural key of a <see cref="CurriculumItemProgress" /> row, unique in the database. It is shared by
/// <c>CreditApplier</c> (Infrastructure), which looks rows up by it, and by the rebuild (Application), which
/// zeroes, restores and removes rows by it. The rebuild's rollback identifies rows it added by their key, so
/// the two layers must agree exactly. Defining the key once is what guarantees that.
/// </summary>
public readonly record struct CurriculumItemProgressKey(
    int CurriculumItemId,
    string TraineeUserId,
    int AcademicYear,
    int Semester)
{
    public static CurriculumItemProgressKey Of(CurriculumItemProgress row)
        => new(row.CurriculumItemId, row.TraineeUserId, row.AcademicYear, row.Semester);

    public static CurriculumItemProgressKey For(int curriculumItemId, string traineeUserId, AcademicPeriod period)
        => new(curriculumItemId, traineeUserId, period.Year, period.Semester);
}
