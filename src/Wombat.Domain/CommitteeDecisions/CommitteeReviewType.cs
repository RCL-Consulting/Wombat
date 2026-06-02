namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// Descriptive classification of a committee review's purpose. Independent of
/// <see cref="CommitteeReview.IsFormative"/> (which controls whether the review issues a binding
/// decision); the type records why the review is being held. No engine logic branches on it — it is
/// recorded so reports and the committee can distinguish a routine annual progression review from a
/// final pre-graduation review. (F-4B-1 d)
/// </summary>
public enum CommitteeReviewType
{
    AnnualProgression = 1,
    PreGraduation = 2
}
