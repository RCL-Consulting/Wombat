namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfResponse
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int InvitationId { get; set; }
    public DateTime SubmittedOn { get; set; }

    public MsfCampaign Campaign { get; set; } = null!;
    public MsfInvitation Invitation { get; set; } = null!;
    public ICollection<MsfResponseAnswer> Answers { get; set; } = [];
}
