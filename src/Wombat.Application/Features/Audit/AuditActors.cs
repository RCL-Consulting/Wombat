using Wombat.Application.Features.MultiSourceFeedback;

namespace Wombat.Application.Features.Audit;

/// <summary>
/// How the audit reads name who acted (T400, flow 18, K8, Q15, M3): the one rule the log and an entry share.
/// </summary>
internal static class AuditActors
{
    /// <summary>
    /// The kind: a person by a user id; the MSF respondent for <c>SubmitMsfResponseCommand</c> with none (the pipeline names
    /// nobody for it, T205); nobody signed in for every other row with none.
    /// </summary>
    public static AuditActorKind KindOf(string? actorUserId, string action)
        => actorUserId is not null
            ? AuditActorKind.Person
            : string.Equals(action, nameof(SubmitMsfResponseCommand), StringComparison.Ordinal)
                ? AuditActorKind.MsfRespondent
                : AuditActorKind.None;

    /// <summary>
    /// The name a page prints: a person's as <c>DataRightsPeople.NamesOfAsync</c> read it now (never the stored display,
    /// which keeps a real name past an erasure); the stored display only on a row with no user id; none for the respondent.
    /// </summary>
    public static string? NameOf(
        AuditActorKind kind,
        string? actorUserId,
        string? storedDisplay,
        IReadOnlyDictionary<string, string> names)
        => kind switch
        {
            AuditActorKind.Person => names[actorUserId!],
            AuditActorKind.None => string.IsNullOrWhiteSpace(storedDisplay) ? null : storedDisplay,
            _ => null,
        };
}
