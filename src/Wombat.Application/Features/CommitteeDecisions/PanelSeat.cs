using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Who may sit on a decision panel, and so be counted towards a decision's quorum: an active holder of the
/// CommitteeMember role at the panel's institution. The one rule the panel picker lists, panel create and update enforce,
/// the review page offers as present, and recording a decision enforces. (T165, D46)
/// </summary>
/// <remarks>
/// <para>
/// A quorum is only as good as the people it counts. Before this rule a panel could hold any string as a member: a
/// forged id, an erased member's pseudonym, a deactivated account, someone who had left the institution. Each could be
/// ticked "present" to make the chair's second. Only the page's picker filtered, and a picker is not a gate (T102).
/// </para>
/// <para>
/// Every role, External included, must be at the panel's institution. The panel page has always said that
/// cross-institution externals are not supported, and the picker has only ever offered a non-Administrator their own
/// institution's committee members; allowing an external from elsewhere is a later decision, not a side effect of this
/// one.
/// </para>
/// <para>
/// Read from the user store, never from claims: a caller's claims are frozen for the life of a Blazor circuit, and
/// another member's are not available at all. "Active" is not deactivated (<see cref="UserIdentityDetails.IsDeactivated" />);
/// a brute-force lockout that lifts itself after minutes does not unseat anyone.
/// </para>
/// <para>
/// The trainee under review is refused separately, where a review is known (<see cref="TraineeUnderReview" />): a
/// trainee may hold CommitteeMember and sit on a panel, but never at their own review.
/// </para>
/// </remarks>
public static class PanelSeat
{
    /// <summary>The refusal of a panel that names someone who may not sit on it.</summary>
    public const string NotEligible =
        "Only an active committee member at the panel's institution can sit on it.";

    /// <summary>The refusal of an attendance that names someone who may not sit.</summary>
    public const string NotEligibleToBePresent =
        "Only an active committee member at the panel's institution can be recorded as present.";

    /// <summary>The refusal of an attendance that names the trainee whose review it is.</summary>
    public const string TraineeUnderReview =
        "The trainee under review cannot be recorded as present at their own review.";

    /// <summary>Whether this user may sit on a panel run at <paramref name="panelInstitutionId" />.</summary>
    public static bool Admits(UserIdentityDetails user, int panelInstitutionId)
    {
        ArgumentNullException.ThrowIfNull(user);

        return !user.IsDeactivated &&
               user.InstitutionId == panelInstitutionId &&
               user.Roles.Contains(WombatRoles.CommitteeMember, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Everyone who may sit on a panel run at <paramref name="panelInstitutionId" />, by user id.</summary>
    public static async Task<IReadOnlyDictionary<string, UserIdentityDetails>> EligibleAsync(
        IUserAdministrationService users,
        int panelInstitutionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(users);

        return (await users.ListUsersInRoleAsync(WombatRoles.CommitteeMember, cancellationToken))
            .Where(user => Admits(user, panelInstitutionId))
            .GroupBy(user => user.UserId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Refuses a panel's member list naming anyone who may not sit on a panel at this institution. Reads only: panel
    /// create and update call it before they change anything.
    /// </summary>
    public static async Task DemandMembersAsync(
        IUserAdministrationService users,
        int panelInstitutionId,
        IEnumerable<DecisionPanelMemberInput> members,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(members);

        var eligible = await EligibleAsync(users, panelInstitutionId, cancellationToken);
        if (members.Any(member => !eligible.ContainsKey(member.UserId.Trim())))
        {
            throw new InvalidOperationException(NotEligible);
        }
    }

    /// <summary>
    /// The review's panel members named as present, each held to the rule: on this panel, eligible to sit now, and not
    /// the trainee under review. Reads only: the handlers that record a decision call it before they change anything.
    /// </summary>
    /// <exception cref="InvalidOperationException">Someone named may not be recorded as present.</exception>
    public static async Task<IReadOnlyCollection<DecisionPanelMember>> DemandPresentAsync(
        IUserAdministrationService users,
        CommitteeReview review,
        IReadOnlyList<string>? presentUserIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(review);

        if (presentUserIds is null)
        {
            throw new InvalidOperationException(CommitteeReview.QuorumRule);
        }

        var present = new List<DecisionPanelMember>(presentUserIds.Count);
        foreach (var userId in presentUserIds)
        {
            var member = review.Panel.Members.FirstOrDefault(candidate =>
                string.Equals(candidate.UserId, userId?.Trim(), StringComparison.Ordinal));

            present.Add(member ?? throw new InvalidOperationException(
                "Only members of this review's panel can be recorded as present."));
        }

        if (present.Any(member => string.Equals(member.UserId, review.TraineeUserId, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(TraineeUnderReview);
        }

        if (present.Count > 0)
        {
            var eligible = await EligibleAsync(users, review.Panel.InstitutionId, cancellationToken);
            if (present.Any(member => !eligible.ContainsKey(member.UserId)))
            {
                throw new InvalidOperationException(NotEligibleToBePresent);
            }
        }

        return present;
    }
}
