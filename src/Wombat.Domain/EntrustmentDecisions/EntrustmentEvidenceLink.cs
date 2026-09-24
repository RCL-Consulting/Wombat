using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.EntrustmentDecisions;

/// <summary>
/// One item of evidence a STAR rests on: a line of the issuing review's frozen evidence snapshot, copied onto the
/// decision when the review is ratified. (D38, T131)
/// </summary>
/// <remarks>
/// <para>
/// Built only by <see cref="FromSnapshot" />, from the snapshot row itself, so what a certificate lists is what the
/// committee saw: the row's label, summary and recorded time, its source, and <see cref="CommitteeEvidenceId" />, the
/// row it came from. Until T131 a link was whatever the caller typed, and every STAR staged through the page had none.
/// </para>
/// <para>
/// A copy, not a reference, as the snapshot row is. <see cref="CommitteeEvidenceId" /> has no foreign key, for T167's
/// reason: a snapshot row and a STAR must each outlive, and never block, a change to the other.
/// </para>
/// </remarks>
public sealed class EntrustmentEvidenceLink
{
    private EntrustmentEvidenceLink()
    {
    }

    public int Id { get; private set; }
    public int DecisionId { get; private set; }
    public EntrustmentEvidenceSourceType SourceType { get; private set; }
    public int? ActivityId { get; private set; }
    public int? MsfCampaignId { get; private set; }
    public int? CommitteeReviewId { get; private set; }

    /// <summary>
    /// The <see cref="CommitteeEvidence" /> row of the issuing review's snapshot this link was built from. Null only on a
    /// link written before T131.
    /// </summary>
    public int? CommitteeEvidenceId { get; private set; }

    public string SourceLabel { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public DateTime? SourceRecordedOn { get; private set; }

    public EntrustmentDecision Decision { get; private set; } = null!;

    /// <summary>
    /// A link that says what this line of a review's frozen snapshot says: an activity, or a released MSF campaign.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The line is not stored yet (it has no id to point back at), it is a supervisor report (a link holds an activity or
    /// a campaign), or it names no source.
    /// </exception>
    public static EntrustmentEvidenceLink FromSnapshot(CommitteeEvidence line)
    {
        ArgumentNullException.ThrowIfNull(line);

        if (line.Id <= 0)
        {
            throw new InvalidOperationException("An evidence link must name a stored line of a review's evidence snapshot.");
        }

        var link = line.SourceType switch
        {
            CommitteeEvidenceSourceType.Activity => Create(
                EntrustmentEvidenceSourceType.Activity,
                line.ActivityId,
                msfCampaignId: null,
                line.SourceLabel,
                line.Summary,
                line.SourceRecordedOn),
            CommitteeEvidenceSourceType.MsfCampaign => Create(
                EntrustmentEvidenceSourceType.MsfCampaign,
                activityId: null,
                line.MsfCampaignId,
                line.SourceLabel,
                line.Summary,
                line.SourceRecordedOn),
            _ => throw new InvalidOperationException(
                "A supervisor report line cannot ground an entrustment decision: an evidence link holds an activity or " +
                "an MSF campaign.")
        };

        link.CommitteeEvidenceId = line.Id;
        return link;
    }

    private static EntrustmentEvidenceLink Create(
        EntrustmentEvidenceSourceType sourceType,
        int? activityId,
        int? msfCampaignId,
        string sourceLabel,
        string summary,
        DateTime? sourceRecordedOn)
    {
        if (string.IsNullOrWhiteSpace(sourceLabel))
        {
            throw new InvalidOperationException("An evidence source label is required.");
        }

        var expectedId = sourceType == EntrustmentEvidenceSourceType.Activity ? activityId : msfCampaignId;
        if (expectedId is null)
        {
            throw new InvalidOperationException("The evidence line names no source record.");
        }

        return new EntrustmentEvidenceLink
        {
            SourceType = sourceType,
            ActivityId = activityId,
            MsfCampaignId = msfCampaignId,
            SourceLabel = sourceLabel.Trim(),
            Summary = summary?.Trim() ?? string.Empty,
            SourceRecordedOn = sourceRecordedOn
        };
    }
}
