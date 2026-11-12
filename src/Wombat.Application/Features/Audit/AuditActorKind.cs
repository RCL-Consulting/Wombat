namespace Wombat.Application.Features.Audit;

/// <summary>
/// Who an audit row says acted (T400, flow 18, K8, T20): a person by their account, the MSF respondent who answers by link
/// with no account, or nobody signed in. The row carries no flag for it: the query reads it from the row's user id and its
/// action.
/// </summary>
public enum AuditActorKind
{
    /// <summary>A row with a user id: the person is named live (Q15).</summary>
    Person,

    /// <summary>
    /// <c>SubmitMsfResponseCommand</c> with no user id: the respondent, whom the pipeline never names
    /// (<c>IAnonymousAuditedCommand</c>, T205).
    /// </summary>
    MsfRespondent,

    /// <summary>Every other row with no user id, a failed sign-in included.</summary>
    None,
}
