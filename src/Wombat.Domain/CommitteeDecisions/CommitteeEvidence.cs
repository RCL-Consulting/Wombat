using Wombat.Domain.Activities;

namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// One line of the evidence a committee review froze when it started: what the panel saw, kept as it was then.
/// </summary>
/// <remarks>
/// <para>
/// A snapshot, not a reference. Every column is copied out of its source at Start and never re-read, so the record of
/// what the committee weighed survives the activity being edited, re-rated or moved on afterwards. None of the ids is a
/// foreign key, for the same reason: a later change to an activity, a campaign or the EPA catalogue must neither
/// cascade into this row nor be blocked by it.
/// </para>
/// <para>
/// The columns from <see cref="EpaId" /> on were added by T167, so that a line says which EPA it is about, by which
/// instrument, at which rung and on which day, without the panel opening each activity. They are null on every row
/// frozen before T167 (an activity row with no <see cref="ObservedOn" /> is one of those), and on an MSF campaign row,
/// which is a report across several EPAs rather than evidence for one.
/// </para>
/// </remarks>
public sealed class CommitteeEvidence
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public CommitteeEvidenceSourceType SourceType { get; set; }
    public int? ActivityId { get; set; }
    public int? MsfCampaignId { get; set; }
    public int? SupervisorReportId { get; set; }
    public string SourceLabel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime? SourceRecordedOn { get; set; }

    /// <summary>
    /// The EPA the activity is evidence for, from its stamped <c>Activity.EpaId</c> (T137). Null when it is about no EPA.
    /// </summary>
    public int? EpaId { get; set; }

    /// <summary>That EPA's code when the review started.</summary>
    public string? EpaCode { get; set; }

    /// <summary>That EPA's title when the review started.</summary>
    public string? EpaTitle { get; set; }

    /// <summary>
    /// The instrument the activity's type declares (<c>ActivityType.WbaToolKey</c>, T122), or null when it declares none.
    /// </summary>
    public string? InstrumentKey { get; set; }

    /// <summary>
    /// The instrument by name: the College's name for <see cref="InstrumentKey" />, else the activity type's own name.
    /// </summary>
    public string? InstrumentName { get; set; }

    /// <summary>
    /// Whether the activity's pinned version rates the trainee at all (it declares a <c>rated_level_field</c>, T126).
    /// False is an unrated instrument; true with no <see cref="RatingOrder" /> is a rating nobody recorded.
    /// </summary>
    public bool? IsRatedInstrument { get; set; }

    /// <summary>The rung the pinned version's rated field held, as the stored ordinal.</summary>
    public int? RatingOrder { get; set; }

    /// <summary>
    /// <see cref="RatingOrder" /> as the rung a clinician reads ("3a") on the ladder the rated field names, or the bare
    /// ordinal when that ladder cannot be resolved.
    /// </summary>
    public string? RatingLabel { get; set; }

    /// <summary>When the encounter happened (<c>Activity.ObservedOn</c>, T119).</summary>
    public DateOnly? ObservedOn { get; set; }

    /// <summary>
    /// Whether a clinician stated <see cref="ObservedOn" />, or it is only the day the form was filed (T161).
    /// </summary>
    public ObservationDateSource? ObservedOnSource { get; set; }

    /// <summary>
    /// The source's state when the review started: an activity's workflow state, a campaign's lifecycle state. Every
    /// state is kept and labelled (T135): a run of declines is evidence too.
    /// </summary>
    public string? SourceState { get; set; }

    public CommitteeReview Review { get; set; } = null!;
}
