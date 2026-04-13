namespace Wombat.Domain.CommitteeDecisions;

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

    public CommitteeReview Review { get; set; } = null!;
}
