namespace Wombat.Domain.EntrustmentDecisions;

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
    public string SourceLabel { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public DateTime? SourceRecordedOn { get; private set; }

    public EntrustmentDecision Decision { get; private set; } = null!;

    public static EntrustmentEvidenceLink Create(
        EntrustmentEvidenceSourceType sourceType,
        int? activityId,
        int? msfCampaignId,
        int? committeeReviewId,
        string sourceLabel,
        string summary,
        DateTime? sourceRecordedOn)
    {
        if (string.IsNullOrWhiteSpace(sourceLabel))
        {
            throw new InvalidOperationException("An evidence source label is required.");
        }

        var populatedIds = (activityId.HasValue ? 1 : 0)
            + (msfCampaignId.HasValue ? 1 : 0)
            + (committeeReviewId.HasValue ? 1 : 0);
        if (populatedIds != 1)
        {
            throw new InvalidOperationException("An evidence link must reference exactly one source id.");
        }

        var expectedSourceType = activityId.HasValue
            ? EntrustmentEvidenceSourceType.Activity
            : msfCampaignId.HasValue
                ? EntrustmentEvidenceSourceType.MsfCampaign
                : EntrustmentEvidenceSourceType.CommitteeReview;
        if (sourceType != expectedSourceType)
        {
            throw new InvalidOperationException("The evidence source type does not match the populated source id.");
        }

        return new EntrustmentEvidenceLink
        {
            SourceType = sourceType,
            ActivityId = activityId,
            MsfCampaignId = msfCampaignId,
            CommitteeReviewId = committeeReviewId,
            SourceLabel = sourceLabel.Trim(),
            Summary = summary?.Trim() ?? string.Empty,
            SourceRecordedOn = sourceRecordedOn
        };
    }
}
