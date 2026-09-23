namespace Wombat.Domain.Curricula;

/// <summary>
/// The window a curriculum item's <see cref="CurriculumItem.RequiredCount" /> is a target for (T130, D18, D39).
/// </summary>
/// <remarks>
/// <para>
/// Progress is always <em>stored</em> per semester (<see cref="AcademicPeriod" />). This is what a reader
/// adds up and compares the target against: one semester's bucket, or both semesters of an academic year.
/// Keeping the choice out of the storage key means an administrator who changes it re-reads the same stored
/// buckets differently. Nothing needs re-bucketing and nothing is stranded.
/// </para>
/// <para>
/// For the CPSA paediatric catalogue the value comes from Annexure B's per-semester column, NOT from the
/// catalogue's <c>currency</c> string. That string is the College's entrustment-decision cadence, which
/// Annexure B states is independent of the observation cadence (EPA 3: observed three per semester, decided
/// annually). See D39.
/// </para>
/// </remarks>
public enum QuotaPeriod
{
    /// <summary>
    /// The target is per academic year: both semesters added together. This is the zero value on purpose,
    /// because it is the only reading that makes sense for a curriculum that publishes a bare yearly count,
    /// and it is what every item created without an explicit choice gets.
    /// </summary>
    AcademicYear = 0,

    /// <summary>The target is per semester, and it resets at each semester boundary.</summary>
    Semester = 1
}
