using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users;

public static class UserAdministrationRules
{
    /// <summary>
    /// Roles that may be added or removed via the admin Users surface. Excludes:
    /// - Administrator: must remain DB-direct (per CLAUDE.md, also cannot be assigned via SSO).
    /// - PendingTrainee: system-managed by the invitation acceptance pipeline.
    /// </summary>
    public static readonly IReadOnlyCollection<string> AssignableRoles =
    [
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor,
        WombatRoles.Trainee
    ];

    public static bool IsAssignableRole(string role) => AssignableRoles.Contains(role);

    // ─── Who administers users (T278) ───────────────────────────────────────

    /// <summary>The refusal to administer users for a caller who holds no role that administers them.</summary>
    public const string MayNotAdministerUsers = "You do not have permission to administer users.";

    /// <summary>
    /// The refusal of every user-administration read and command to a caller who holds the Trainee role beside a role that
    /// administers users, and what the Users pages say to them in place of what those roles would offer. It is given
    /// before any user is looked up, so it says nothing about an id. (T278)
    /// </summary>
    public const string TraineeAdministersNoUser =
        "You hold the Trainee role, so you cannot view or change user accounts, including your own.";

    /// <summary>The refusal to add or remove a role on the caller's own account. (T278)</summary>
    public const string OwnRolesNotChangeable =
        "You cannot change your own roles. Another administrator must change them.";

    /// <summary>The refusal to lock out or reactivate the caller's own account. (T278)</summary>
    public const string OwnLockoutNotChangeable = "You cannot lock out or reactivate your own account.";

    /// <summary>
    /// The refusal to reset the caller's own password from the Users page, which asks for no current password. (T278)
    /// </summary>
    public const string OwnPasswordNotResettable =
        "You cannot reset your own password here. Change it on the Change password page, which asks for your current password.";

    /// <summary>
    /// What the user page says on the caller's own account, where it offers no role, lockout or password change. (T278)
    /// </summary>
    public const string OwnAccountNote =
        "This is your own account, so you cannot change its roles, lockout or password here. Another administrator can " +
        "change your roles or lockout.";

    /// <summary>
    /// Whether this caller may administer users at all: a role that administers them, and not the Trainee role. Which
    /// users is the handlers' institution scope (T056), asked after this. (T278)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185), asked before every arm, the
    /// Administrator's included. A registrar who also administers their institution, or the system, lists no user, opens
    /// none, and changes nobody's roles, lockout, password or invitations. Not a peer's: a user's roles decide what they
    /// may read and act on, and since T256 every chair action on a committee review reads the user store, so locking out
    /// a panel's chair, or taking CommitteeMember from them, stops that panel ratifying the reviews of the trainees it
    /// sits for. Not their own either (<see cref="IsCaller" />, which refuses it to everyone): until T278 a Trainee who
    /// also held InstitutionalAdmin could remove their own Trainee role, sign in again, and administer the panel that
    /// reviews them, which undid T256.
    /// </para>
    /// <para>
    /// Only a caller whose Trainee role is what stands in the way, one who also holds a role that administers users, is
    /// told that it is (<see cref="TraineeAdministersNoUser" />), as the panel pages tell them (T256). Anyone else holds
    /// no role that administers users, and is told so.
    /// </para>
    /// </remarks>
    public static bool MayAdministerUsers(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return !TraineeScopeResolver.ActsAsTrainee(principal) && HoldsUserAdministrationRole(principal);
    }

    /// <summary>
    /// Refuses, before anything is looked up, a caller who may not administer users at all
    /// (<see cref="MayAdministerUsers" />). Every Users read and command asks it first. (T278)
    /// </summary>
    public static void DemandUserAdministration(ClaimsPrincipal principal)
    {
        if (MayAdministerUsers(principal))
        {
            return;
        }

        throw new UnauthorizedAccessException(
            HoldsUserAdministrationRole(principal) ? TraineeAdministersNoUser : MayNotAdministerUsers);
    }

    /// <summary>
    /// What the Users pages say to someone who holds Trainee beside a role that administers users: why they list and
    /// offer nobody (<see cref="TraineeAdministersNoUser" />); null for anyone else. The pages ask it before they read
    /// anything, as the handlers refuse before they look anything up. (T278)
    /// </summary>
    /// <remarks>
    /// Read off <see cref="MayAdministerUsers" /> itself, as <see cref="DemandUserAdministration" /> is, so the pages and
    /// the handlers cannot drift: a caller the handlers would admit is never shown the note.
    /// </remarks>
    public static string? TraineeNoteOnUserPages(ClaimsPrincipal principal)
        => !MayAdministerUsers(principal) && HoldsUserAdministrationRole(principal) ? TraineeAdministersNoUser : null;

    /// <summary>
    /// Whether <paramref name="userId" /> is the caller's own account. Nobody changes their own roles, lockout or
    /// password from the Users surface, the Administrator included: the handlers refuse it (<see cref="DemandNotCaller" />)
    /// and the user page offers none of them on the caller's own account. (T278)
    /// </summary>
    /// <remarks>
    /// Revoking the pending invitations addressed to the caller's own email is not refused: an invitation creates an
    /// account, and one addressed to an account that exists cannot be accepted, so revoking it changes nothing about
    /// the account.
    /// </remarks>
    public static bool IsCaller(ClaimsPrincipal principal, string userId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(callerUserId) && string.Equals(callerUserId, userId, StringComparison.Ordinal);
    }

    /// <summary>
    /// Refuses, before the user is looked up, a change to the caller's own account (<see cref="IsCaller" />) with
    /// <paramref name="refusal" />, the words for what was asked. (T278)
    /// </summary>
    public static void DemandNotCaller(ClaimsPrincipal principal, string userId, string refusal)
    {
        if (IsCaller(principal, userId))
        {
            throw new UnauthorizedAccessException(refusal);
        }
    }

    /// <summary>
    /// Whether this caller holds a role that administers users, whatever else they hold: the roles the Users pages'
    /// policy admits (<c>AdministratorOrInstitutionalAdmin</c>). Not the right to administer them, which is
    /// <see cref="MayAdministerUsers" />: this only says which refusal a caller who may not is given.
    /// </summary>
    private static bool HoldsUserAdministrationRole(ClaimsPrincipal principal)
        => principal.IsAdministrator() || principal.IsInstitutionalAdmin();
}
