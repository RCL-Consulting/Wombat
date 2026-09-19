namespace Wombat.Domain.DataRights;

public sealed class DataRightsRequest
{
    private DataRightsRequest() { }

    public Guid Id { get; set; }
    public string RequesterUserId { get; set; } = string.Empty;
    public string RequesterDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The requester's institution, snapshotted when the request was submitted. (T112)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A subject access report is everything the product holds about one person — the richest object
    /// in the system. Until T112 every handler in this feature gated on role alone, so any
    /// <c>Coordinator</c> in any institution could list, read, download, approve and <b>erase</b> any
    /// other institution's requests. Scoping needs to know where a request belongs, and nothing here
    /// recorded it.
    /// </para>
    /// <para>
    /// Stamped rather than derived, for the same reasons as <c>Activity.InstitutionId</c> (T101): it
    /// is one column comparison rather than a join, and a person who later moves institutions does not
    /// drag the visibility of an old request with them. Taken from the submitting principal's own
    /// claim, because the submitter IS the data subject.
    /// </para>
    /// <para>
    /// Null when the requester carried no institution claim — a global Administrator, or a user whose
    /// record has none. Null matches no scoped reviewer, so such a request is Administrator-only.
    /// That is the safe direction for a feature that can irreversibly erase a person's record.
    /// </para>
    /// <para>
    /// NOTE the deliberate difference from <c>Activity.InstitutionId</c>, which T101 resolves through
    /// the subject's <c>TraineeProfile</c>. A data-rights request is about a PERSON, and most
    /// requesters — assessors, coordinators, administrative staff — have no trainee profile at all, so
    /// the identity record is the only source that answers for everyone. It is also the source the
    /// <c>institution_id</c> claim is issued from, which is why the runtime stamp and the migration's
    /// backfill agree exactly. The consequence to keep in mind: for a trainee whose profile and
    /// identity record disagree, the request and their activities can land in different institutions.
    /// </para>
    /// </remarks>
    public int? InstitutionId { get; set; }
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
        DateTime utcNow,
        int? institutionId = null)
    {
        return new DataRightsRequest
        {
            Id = Guid.CreateVersion7(utcNow),
            RequesterUserId = requesterUserId.Trim(),
            RequesterDisplayName = requesterDisplayName.Trim(),
            InstitutionId = institutionId,
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
