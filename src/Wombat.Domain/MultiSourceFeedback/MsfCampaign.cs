namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfCampaign
{
    public int Id { get; set; }
    public string SubjectUserId { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateOnly OpensOn { get; set; }
    public DateOnly ClosesOn { get; set; }
    public int MinimumResponses { get; set; } = 8;
    public int MinimumCategoryResponses { get; set; } = 3;

    /// <summary>
    /// How many respondent categories must survive suppression before the report may be released. (T121)
    /// </summary>
    /// <remarks>
    /// College decision D11, 2026-09-20: <b>at least two</b>. <see cref="MinimumCategoryResponses" /> only
    /// ever suppressed a thin category's data from the report; nothing enforced that any category
    /// survived. A campaign answered by eight peer doctors therefore passed
    /// <see cref="MinimumResponses" />, showed one category and five suppressed ones, and was released as
    /// "multi-source feedback" on a single source. Sits beside the other two thresholds, and is edited on
    /// the same form, because an operator who can raise the response floor should be able to see this one.
    /// </remarks>
    public int MinimumRespondentCategories { get; set; } = 2;

    public MsfCampaignState State { get; set; } = MsfCampaignState.Draft;
    public string? CoordinatorNarrative { get; set; }

    /// <summary>
    /// The one entrustment ordinal the releasing reviewer states, or null when they state none. (T121)
    /// </summary>
    /// <remarks>
    /// <para>
    /// College decision D10, 2026-09-20: the level an MSF asserts is stated by <b>the releasing
    /// reviewer</b>, a named clinician, informed by the aggregate — never derived from respondent scores.
    /// The aggregate cannot supply one: <c>MsfAggregationService</c> produces per-(category, question)
    /// arithmetic means as <c>double</c>, and on the CPSA ladder — rungs 1, 2, 3a, 3b, 4, 5 at ordinals
    /// 1–6 — a mean of 3.4 is not a rung, and the rungs it sits between are not evenly spaced.
    /// </para>
    /// <para>
    /// Optional, and null is the honest default: MSF then records evidence and asserts no level. It is
    /// copied verbatim into <c>overall_level</c> on each evidence activity the release creates, which is
    /// where it is bound to a ladder by the pinned schema's <c>scale_key</c> (T109).
    /// </para>
    /// <para>
    /// Stored as the scale's <b>order</b>, which is what every other rating in the product stores and
    /// compares — not the printed rung label. On the CPSA ladder order 5 is rung "4" (T100).
    /// </para>
    /// </remarks>
    public int? ReviewerEntrustmentLevel { get; set; }

    public string? ReviewedByUserId { get; set; }
    public DateTime? OpenedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public DateTime? ReleasedOn { get; set; }
    public DateTime? WithdrawnOn { get; set; }

    /// <summary>
    /// When this campaign's per-EPA evidence activities were created, or null if they never were. (T121)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not a duplicate of <see cref="ReleasedOn" />, and the difference is the whole point: a release
    /// whose fan-out produced nothing — every covered EPA had left the subject's curriculum, or the
    /// coverage set was empty — leaves this null while <see cref="ReleasedOn" /> is set. That pair is
    /// exactly the repairable state, and it is queryable.
    /// </para>
    /// <para>
    /// It is also the retry guard. <see cref="Release" /> already refuses a second release, so a replay
    /// cannot get this far by the front door; this stops a repair path creating a second set of
    /// activities by the back one.
    /// </para>
    /// </remarks>
    public DateTime? EvidenceRecordedOn { get; set; }

    public MsfTemplate Template { get; set; } = null!;
    public ICollection<MsfInvitation> Invitations { get; set; } = [];
    public ICollection<MsfResponse> Responses { get; set; } = [];

    /// <inheritdoc cref="MsfCampaignEpa" />
    public ICollection<MsfCampaignEpa> CoveredEpas { get; set; } = [];

    public void Open(DateTime utcNow)
    {
        EnsureCanOpen();

        State = MsfCampaignState.Open;
        OpenedOn = utcNow;
    }

    /// <summary>
    /// Refuses, as <see cref="Open" /> would, a campaign that cannot be opened, and changes nothing. (T184)
    /// </summary>
    /// <remarks>
    /// Opening mails every respondent before the campaign is touched, so the refusal has to come before the first
    /// mail: a campaign that is already open must not send a second round of links that its stored tokens do not
    /// match.
    /// </remarks>
    public void EnsureCanOpen()
    {
        if (State != MsfCampaignState.Draft)
        {
            throw new InvalidOperationException("Only draft campaigns can be opened.");
        }
    }

    /// <summary>
    /// Closes the campaign to responses and anonymises every respondent (<see cref="MsfInvitation.Anonymize" />).
    /// </summary>
    /// <remarks>
    /// The anonymising is part of closing, not a separate step a caller must remember. A campaign closes in two
    /// places, the coordinator's close command and the hourly auto-close job, and until T184 each carried its own copy
    /// of the anonymise routine. The caller must have loaded <see cref="Invitations" />: an unloaded collection is
    /// empty, and nothing would be anonymised.
    /// </remarks>
    public void Close(DateTime utcNow)
    {
        if (State is not (MsfCampaignState.Open or MsfCampaignState.UnderReview))
        {
            throw new InvalidOperationException("Only open or under-review campaigns can be closed.");
        }

        State = MsfCampaignState.UnderReview;
        ClosedOn = utcNow;

        foreach (var invitation in Invitations)
        {
            invitation.Anonymize(utcNow);
        }
    }

    /// <summary>
    /// Releases the aggregate report to the trainee, recording who released it and what, if anything,
    /// they judged about the trainee's supervision needs.
    /// </summary>
    /// <remarks>
    /// This is the point of no return, and that is why T121 hangs the evidence fan-out off it rather than
    /// off <see cref="Close" />: <see cref="Close" /> moves to <see cref="MsfCampaignState.UnderReview" />
    /// and <see cref="Withdraw" /> is legal from there, so evidence created at close could stand behind a
    /// withdrawn campaign. <see cref="Withdraw" /> refuses a released campaign and this method refuses
    /// anything but <see cref="MsfCampaignState.UnderReview" />, so the fan-out is once-only by
    /// construction.
    /// </remarks>
    public void Release(string reviewerUserId, string? narrative, int? entrustmentLevel, DateTime utcNow)
    {
        if (State != MsfCampaignState.UnderReview)
        {
            throw new InvalidOperationException("Only under-review campaigns can be released.");
        }

        State = MsfCampaignState.Released;
        ReviewedByUserId = reviewerUserId.Trim();
        CoordinatorNarrative = string.IsNullOrWhiteSpace(narrative) ? null : narrative.Trim();
        ReviewerEntrustmentLevel = entrustmentLevel;
        ReleasedOn = utcNow;
    }

    public void Withdraw(DateTime utcNow)
    {
        if (State == MsfCampaignState.Released)
        {
            throw new InvalidOperationException("Released campaigns cannot be withdrawn.");
        }

        State = MsfCampaignState.Withdrawn;
        WithdrawnOn = utcNow;
    }
}
