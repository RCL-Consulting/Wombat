namespace Wombat.Application.Common.Options;

public sealed class WombatOptions
{
    public const string SectionName = "Wombat";

    public bool AllowSelfRegistration { get; set; }
    public string? BaseUrl { get; set; }
    public string? MsfRespondUrl { get; set; }
    public string? SeedAdminEmail { get; set; }
    public string? SeedAdminPassword { get; set; }
    public string? PseudonymSalt { get; set; }

    /// <summary>
    /// Whether startup carries edits to the activity-type seed folders into types that already
    /// exist, by publishing a new version (T103). On by default — without it a seed edit reaches a
    /// fresh database only.
    /// </summary>
    /// <remarks>
    /// The kill switch is <c>Wombat__RefreshSeededActivityTypes=false</c>. When it is off the
    /// refresher still runs read-only and logs what it would have republished, so a stale seed is a
    /// visible no-op rather than a silent one. Turning it off is worth doing around a rollback: a
    /// binary rollback does not undo a version bump, so the older binary's older seed files would
    /// diff again and publish a third version.
    /// </remarks>
    public bool RefreshSeededActivityTypes { get; set; } = true;

    /// <summary>
    /// Whether startup gives already-stored activities the encounter date their clinician typed, once
    /// the schema version they are pinned to declares an <c>observation_date_field</c> (T119).
    /// </summary>
    /// <remarks>
    /// The kill switch is <c>Wombat__RestampActivityObservationDates=false</c>. Off, the pass still runs
    /// read-only and logs how many rows it would have moved, so a corpus still dated by the audit clock
    /// is visible rather than silent. It only ever touches rows whose date is still the creation
    /// timestamp, so it cannot overwrite a corrected date and is safe to leave on.
    /// </remarks>
    public bool RestampActivityObservationDates { get; set; } = true;
}
