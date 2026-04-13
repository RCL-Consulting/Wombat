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
    public MsfCampaignState State { get; set; } = MsfCampaignState.Draft;
    public string? CoordinatorNarrative { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTime? OpenedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public DateTime? ReleasedOn { get; set; }
    public DateTime? WithdrawnOn { get; set; }

    public MsfTemplate Template { get; set; } = null!;
    public ICollection<MsfInvitation> Invitations { get; set; } = [];
    public ICollection<MsfResponse> Responses { get; set; } = [];

    public void Open(DateTime utcNow)
    {
        if (State != MsfCampaignState.Draft)
        {
            throw new InvalidOperationException("Only draft campaigns can be opened.");
        }

        State = MsfCampaignState.Open;
        OpenedOn = utcNow;
    }

    public void Close(DateTime utcNow)
    {
        if (State is not (MsfCampaignState.Open or MsfCampaignState.UnderReview))
        {
            throw new InvalidOperationException("Only open or under-review campaigns can be closed.");
        }

        State = MsfCampaignState.UnderReview;
        ClosedOn = utcNow;
    }

    public void Release(string reviewerUserId, string? narrative, DateTime utcNow)
    {
        if (State != MsfCampaignState.UnderReview)
        {
            throw new InvalidOperationException("Only under-review campaigns can be released.");
        }

        State = MsfCampaignState.Released;
        ReviewedByUserId = reviewerUserId.Trim();
        CoordinatorNarrative = string.IsNullOrWhiteSpace(narrative) ? null : narrative.Trim();
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
