namespace Wombat.Domain.DataRights;

/// <summary>
/// Where a data-rights request stands. Stored as its number, so each member keeps the value it was stored under.
/// </summary>
/// <remarks>
/// 1 was <c>UnderReview</c>, which nothing ever set (<c>DataRightsRequest.Review()</c> had no caller), so the queue's
/// Status filter offered a state no request reached. It is gone (T387, Q8), and the T387 migration moves any row stored
/// under 1 back to <see cref="Submitted" />.
/// </remarks>
public enum DataRightsRequestStatus
{
    Submitted = 0,
    Approved = 2,
    Rejected = 3,
    Completed = 4,
    Withdrawn = 5
}
