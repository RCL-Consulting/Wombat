namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// What a decision panel must hold: each member once, exactly one chair, and at least
/// <see cref="CommitteeReview.Quorum" /> members, so that the chair is never the whole committee. (T165, D46)
/// </summary>
/// <remarks>
/// Before T165 a panel needed only one entry, of any role. A one-member panel's chair records and ratifies alone, which
/// is the single-person decision the College rules out. A panel smaller than the quorum could never ratify under T165's
/// attendance rule either, so it is refused when it is made, not when it first tries to decide.
/// </remarks>
public static class DecisionPanelComposition
{
    public const string MemberListedTwice = "A panel member is listed more than once.";

    public const string ExactlyOneChair = "A panel needs exactly one chair.";

    public const string TooFewMembers =
        "A panel needs at least two members: the chair and at least one other, so that no decision is one person's.";

    /// <summary>Whether no user is named twice. Ids are compared as stored: trimmed, ordinal.</summary>
    public static bool NamesEachMemberOnce(IEnumerable<string> userIds)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var trimmed = userIds.Select(Normalise).ToArray();
        return trimmed.Distinct(StringComparer.Ordinal).Count() == trimmed.Length;
    }

    /// <summary>Whether exactly one member holds the Chair role.</summary>
    public static bool HasOneChair(IEnumerable<DecisionPanelMemberRole> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        return roles.Count(role => role == DecisionPanelMemberRole.Chair) == 1;
    }

    /// <summary>Whether the panel holds at least <see cref="CommitteeReview.Quorum" /> distinct, named members.</summary>
    public static bool HoldsAQuorum(IEnumerable<string> userIds)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        return userIds
            .Select(Normalise)
            .Where(userId => userId.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Count() >= CommitteeReview.Quorum;
    }

    private static string Normalise(string? userId) => userId?.Trim() ?? string.Empty;
}
