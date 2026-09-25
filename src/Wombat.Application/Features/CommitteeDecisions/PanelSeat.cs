using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Who may sit on a decision panel, and so be counted towards a decision's quorum: an active holder of the
/// CommitteeMember role at the panel's institution who does not hold Trainee. The one rule the panel picker lists, panel
/// create and update enforce, the review page offers as present, recording a decision enforces, and the appeal body is
/// named by and must meet to resolve an appeal (<see cref="AppealBodyAt" />). (T165, D46, T237)
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
/// <b>Never someone who holds Trainee</b> (<see cref="TraineeScopeResolver.HoldersAsync" />, T237), whatever seat: the
/// rung every seat predicate asks first (<c>CommitteeDecisionAuthorization.HoldsSeat</c> and <c>WorksOnPanel</c>, the
/// T194 review, after T185). Until T237 a trainee who held CommitteeMember could be seated as the trainees'
/// representative, and so as the Chair or an External member, where since T194 they cannot act: a panel chaired by one
/// had no working chair, and a quorum could count a member who cannot act. The picker now leaves them out and a save
/// naming them is refused, by this one rule; a member given Trainee after being seated is taken off at the panel's next
/// save, is not offered or accepted as present, and the review page says why the panel cannot decide
/// (<c>CommitteeReviewDetailDto.PanelShortfall</c>).
/// </para>
/// <para>
/// The trainee under review is refused separately, where a review is known (<see cref="TraineeUnderReview" />), so a
/// trainee who has since lost the Trainee role still never sits at their own review.
/// </para>
/// </remarks>
public static class PanelSeat
{
    /// <summary>The refusal of a panel that names someone who may not sit on it.</summary>
    public const string NotEligible =
        "Only an active committee member at the panel's institution who is not a trainee can sit on it.";

    /// <summary>The refusal of an attendance that names someone who may not sit.</summary>
    public const string NotEligibleToBePresent =
        "Only an active committee member at the panel's institution who is not a trainee can be recorded as present.";

    /// <summary>
    /// The refusal of someone on the appeal body who may not sit at the review now (<see cref="DemandSitsOnAppealBody" />).
    /// They hold a seat on its panel, so may read the review, and the refusal tells them nothing they cannot see.
    /// </summary>
    public const string MayNotResolveFromSeat =
        "You sit on this panel's appeal body but cannot resolve its appeals now: only an active committee member at the " +
        "panel's institution who is not a trainee can, and never the trainee under review.";

    /// <summary>The refusal of an attendance that names the trainee whose review it is.</summary>
    public const string TraineeUnderReview =
        "The trainee under review cannot be recorded as present at their own review.";

    /// <summary>
    /// Everyone who may sit on a panel run at <paramref name="panelInstitutionId" />, by user id: the one rule, which
    /// every caller reads through here.
    /// </summary>
    /// <remarks>
    /// Two reads, because a user record from a role listing carries only the role it was listed by: a committee member's
    /// record never says whether they also hold Trainee, so the store's role links are asked that, of the active committee
    /// members at the institution alone (<see cref="TraineeScopeResolver.HoldersAsync" />, T237).
    /// </remarks>
    public static async Task<IReadOnlyDictionary<string, UserIdentityDetails>> EligibleAsync(
        IUserAdministrationService users,
        int panelInstitutionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(users);

        var committee = (await users.ListUsersInRoleAsync(WombatRoles.CommitteeMember, cancellationToken))
            .Where(user => IsActiveCommitteeMemberAt(user, panelInstitutionId))
            .GroupBy(user => user.UserId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        if (committee.Count == 0)
        {
            return committee;
        }

        var trainees = await TraineeScopeResolver.HoldersAsync(users, committee.Keys.ToArray(), cancellationToken);
        return committee
            .Where(pair => !trainees.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// The members of this review's panel who may sit at it now: eligible to sit on the panel (<see cref="EligibleAsync" />),
    /// and never the trainee whose review it is. Who the review page offers as present and, among its Chair and External
    /// members, the appeal body that can act (<see cref="AppealBodyAt" />).
    /// </summary>
    public static IEnumerable<DecisionPanelMember> SittingAt(
        CommitteeReview review,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
    {
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(eligible);

        return review.Panel.Members.Where(member =>
            eligible.ContainsKey(member.UserId) &&
            !string.Equals(member.UserId, review.TraineeUserId, StringComparison.Ordinal));
    }

    /// <summary>
    /// The appeal body that can act at this review now, chair first: its panel's Chair and External members among those
    /// who may sit at it (<see cref="SittingAt" />). The one list the review page's appeal-body note names and resolving an
    /// appeal demands the caller is on (<see cref="DemandSitsOnAppealBody" />), so the note never leaves out someone who
    /// can resolve the appeal, nor names someone who cannot. (T237)
    /// </summary>
    public static IReadOnlyList<DecisionPanelMember> AppealBodyAt(
        CommitteeReview review,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
        => SittingAt(review, eligible)
            .Where(member => member.Role is DecisionPanelMemberRole.Chair or DecisionPanelMemberRole.External)
            .OrderBy(member => member.Role)
            .ThenBy(member => member.UserId, StringComparer.Ordinal)
            .ToArray();

    /// <summary>
    /// Refuses a caller who holds a seat on the appeal body but may not sit at this review now (<see cref="AppealBodyAt" />):
    /// deactivated, moved to another institution, no longer a committee member, given Trainee since signing in, or the
    /// trainee under review. Reads only: the resolve handler calls it before it changes anything. (T237)
    /// </summary>
    /// <remarks>
    /// The seat predicate alone (<c>CommitteeDecisionAuthorization.ResolvesAppeals</c>) reads the caller's claims, and a
    /// claim cannot say whether a person is still a committee member at the panel's institution. Until T237 a chair who
    /// had lost the CommitteeMember role, or moved away, could still uphold or dismiss an appeal, while the note that tells
    /// every other reader who resolves it, filled by this rule, left them out; and a former trainee seated on the panel
    /// that reviewed them could uphold their own appeal.
    /// </remarks>
    /// <exception cref="UnauthorizedAccessException">The caller may not resolve the appeal from their seat now.</exception>
    public static void DemandSitsOnAppealBody(
        CommitteeReview review,
        string userId,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
    {
        if (!AppealBodyAt(review, eligible).Any(member => string.Equals(member.UserId, userId, StringComparison.Ordinal)))
        {
            throw new UnauthorizedAccessException(MayNotResolveFromSeat);
        }
    }

    /// <summary>
    /// The half of the rule a committee member's own record answers: not deactivated, at the panel's institution, and
    /// holding CommitteeMember. Private, so nothing asks it without the Trainee half (<see cref="EligibleAsync" />).
    /// </summary>
    private static bool IsActiveCommitteeMemberAt(UserIdentityDetails user, int panelInstitutionId)
        => !user.IsDeactivated &&
           user.InstitutionId == panelInstitutionId &&
           user.Roles.Contains(WombatRoles.CommitteeMember, StringComparer.OrdinalIgnoreCase);

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

        return DemandPresent(
            review,
            presentUserIds,
            await EligibleAsync(users, review.Panel.InstitutionId, cancellationToken));
    }

    /// <summary>
    /// <see cref="DemandPresentAsync" /> with who may sit already read, for a handler that reads it for another check too
    /// (resolving an appeal, <see cref="DemandSitsOnAppealBody" />).
    /// </summary>
    /// <exception cref="InvalidOperationException">Someone named may not be recorded as present.</exception>
    public static IReadOnlyCollection<DecisionPanelMember> DemandPresent(
        CommitteeReview review,
        IReadOnlyList<string>? presentUserIds,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
    {
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(eligible);

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

        if (present.Any(member => !eligible.ContainsKey(member.UserId)))
        {
            throw new InvalidOperationException(NotEligibleToBePresent);
        }

        return present;
    }
}
