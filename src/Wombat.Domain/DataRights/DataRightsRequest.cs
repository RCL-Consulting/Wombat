namespace Wombat.Domain.DataRights;

public sealed class DataRightsRequest
{
    private DataRightsRequest() { }

    public Guid Id { get; set; }
    public string RequesterUserId { get; set; } = string.Empty;
    public string RequesterDisplayName { get; set; } = string.Empty;
    public DateTime RequestedOn { get; set; }
    public DataRightsRequestType Type { get; set; }
    public DataRightsRequestStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DecisionNote { get; set; }
    public string? DecidedByUserId { get; set; }
    public DateTime? DecidedOn { get; set; }
    public DateTime? CompletedOn { get; set; }

    public ICollection<DataRightsRectification> Rectifications { get; set; } = [];

    public static DataRightsRequest Create(
        string requesterUserId,
        string requesterDisplayName,
        DataRightsRequestType type,
        string reason,
        DateTime utcNow)
    {
        return new DataRightsRequest
        {
            Id = Guid.CreateVersion7(utcNow),
            RequesterUserId = requesterUserId.Trim(),
            RequesterDisplayName = requesterDisplayName.Trim(),
            RequestedOn = utcNow,
            Type = type,
            Status = DataRightsRequestStatus.Submitted,
            Reason = reason.Trim()
        };
    }

    public void Review()
    {
        if (Status != DataRightsRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be placed under review.");

        Status = DataRightsRequestStatus.UnderReview;
    }

    public void Approve(string decidedByUserId, string decisionNote, DateTime utcNow)
    {
        if (Status is not (DataRightsRequestStatus.Submitted or DataRightsRequestStatus.UnderReview))
            throw new InvalidOperationException("Only submitted or under-review requests can be approved.");

        DecidedByUserId = decidedByUserId.Trim();
        DecisionNote = decisionNote.Trim();
        DecidedOn = utcNow;
        Status = DataRightsRequestStatus.Approved;
    }

    public void Reject(string decidedByUserId, string decisionNote, DateTime utcNow)
    {
        if (Status is not (DataRightsRequestStatus.Submitted or DataRightsRequestStatus.UnderReview))
            throw new InvalidOperationException("Only submitted or under-review requests can be rejected.");

        DecidedByUserId = decidedByUserId.Trim();
        DecisionNote = decisionNote.Trim();
        DecidedOn = utcNow;
        Status = DataRightsRequestStatus.Rejected;
    }

    public void Complete(DateTime utcNow)
    {
        if (Status != DataRightsRequestStatus.Approved)
            throw new InvalidOperationException("Only approved requests can be completed.");

        CompletedOn = utcNow;
        Status = DataRightsRequestStatus.Completed;
    }

    public void Withdraw()
    {
        if (Status is DataRightsRequestStatus.Completed or DataRightsRequestStatus.Withdrawn)
            throw new InvalidOperationException("Completed or already-withdrawn requests cannot be withdrawn.");

        Status = DataRightsRequestStatus.Withdrawn;
    }
}
