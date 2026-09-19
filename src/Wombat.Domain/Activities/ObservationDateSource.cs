namespace Wombat.Domain.Activities;

/// <summary>
/// Where an <see cref="Activity.ObservedOn" /> value came from. (T119)
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Activity.ObservedOn" /> is non-null, so this carries the information a nullable column
/// would have carried — without pushing <c>COALESCE</c> into four SQL sites and every phase-3
/// <c>GROUP BY</c>. Same device as <c>CurriculumItemProgress.UnverifiedLevelCount</c> (T109): a value
/// that says how much a stored number should be trusted, rather than a null that says nothing.
/// </para>
/// <para>
/// <see cref="CreatedOn" /> means <b>nobody stated when this happened</b> — the date is the audit clock.
/// A surface may mark such a point as undated evidence rather than presenting the filing date as a
/// clinical fact, and an operator can count how much of the corpus is still guessing.
/// </para>
/// </remarks>
public enum ObservationDateSource
{
    /// <summary>A clinician stated it, in the field the pinned schema's pointer names.</summary>
    Declared = 0,

    /// <summary>Nobody stated it; the date is <see cref="Activity.CreatedOn" />.</summary>
    CreatedOn = 1
}
