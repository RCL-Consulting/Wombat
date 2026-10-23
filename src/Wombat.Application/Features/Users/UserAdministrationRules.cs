using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users;

public static class UserAdministrationRules
{
    /// <summary>
    /// Roles that may be added or removed via the admin Users surface, in the order the user page offers them. Excludes:
    /// - Administrator: must remain DB-direct (per CLAUDE.md, also cannot be assigned via SSO).
    /// - PendingTrainee: system-managed by the invitation acceptance pipeline.
    /// - Trainee: system-managed by admission (T303). <c>AdmitTrainee</c> grants it, with the profile and the adoption pin,
    ///   and <c>CompleteTraineeProfile</c> takes it away (<see cref="TraineeRoleByAdmissionOnly" />).
    /// </summary>
    public static readonly IReadOnlyCollection<string> AssignableRoles =
    [
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor
    ];

    public static bool IsAssignableRole(string role) => AssignableRoles.Contains(role);

    /// <summary>
    /// The refusal to add or remove the Trainee role on the Users surface, and where a registrar is made a trainee
    /// instead. (T303)
    /// </summary>
    /// <remarks>
    /// Until T303 Add role offered Trainee. For a pending registrar it gave the role without a profile or an adoption pin,
    /// beside PendingTrainee, and a later admission saved the profile and then failed on the role it found already held;
    /// for a graduate it gave the role back against a completed profile. Remove role took it from a running programme,
    /// whose trainee could then open none of their own pages. Admission grants it and Mark complete takes it away, so the
    /// role always travels with a profile. Asked of the role alone, before the user is looked up, so nothing is written
    /// before it.
    /// </remarks>
    public const string TraineeRoleByAdmissionOnly =
        "The Trainee role cannot be added or removed here. A registrar becomes a trainee when admitted to a curriculum: " +
        "open Trainees and choose 'Admit…'. Marking their programme complete takes the role away.";

    /// <summary>
    /// What the user page's Add role field says: that Trainee is not offered, and where a registrar is made one. (T303)
    /// Since T373 the button is Waiting to be admitted's "Admit…" (fix pass, C5/R1): until then it was "Admit to curriculum".
    /// </summary>
    public const string TraineeRoleNotOffered =
        "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.";

    /// <summary>
    /// Refuses the Trainee role (<see cref="TraineeRoleByAdmissionOnly" />), then any other role the surface does not
    /// manage (<see cref="AssignableRoles" />) in the words of <paramref name="refusedWith" />, which names what was asked.
    /// Asked of the role alone, so Add role and Remove role ask it before the user is looked up. (T303)
    /// </summary>
    public static void DemandAssignableRole(string role, Func<string, string> refusedWith)
    {
        ArgumentNullException.ThrowIfNull(refusedWith);

        if (string.Equals(role, WombatRoles.Trainee, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(TraineeRoleByAdmissionOnly);
        }

        if (!IsAssignableRole(role))
        {
            throw new InvalidOperationException(refusedWith(role));
        }
    }

    // ─── Add a role… (T376, flow 12) ────────────────────────────────────────

    /// <summary>The roles whose need includes a sub-speciality: Sub-speciality admin and Assessor (Spec 2, F1).</summary>
    public static bool RoleTakesSubSpeciality(string role)
        => role is WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor;

    /// <summary>The roles that need a speciality: Speciality admin, and those that need a sub-speciality too.</summary>
    public static bool RoleNeedsSpeciality(string role)
        => role is WombatRoles.SpecialityAdmin || RoleTakesSubSpeciality(role);

    /// <summary>
    /// The roles a speciality may keep to one, or leave at Any speciality (Coordinator, Committee member): Add a role… offers
    /// a speciality to a person at Any speciality, with no need to choose one.
    /// </summary>
    public static bool RoleMayKeepToSpeciality(string role)
        => role is WombatRoles.Coordinator or WombatRoles.CommitteeMember;

    /// <summary>The refusal of a role the person already holds; the page never offers one.</summary>
    public static string RoleAlreadyHeld(string role) => $"They already hold {WombatRoleLabels.For(role)}.";

    /// <summary>The refusal for a person with no institution (U14's College admin): every role offered here works at one.</summary>
    public const string RoleNeedsAnInstitution =
        "This person has no institution, so no role can be added here: every role offered here works at an institution.";

    /// <summary>The refusal of a speciality the person's institution has not adopted (S13).</summary>
    public const string SpecialityNotAdopted = "Choose a speciality this person's institution has adopted.";

    /// <summary>The refusal of a sub-speciality the person's institution has not adopted (S13).</summary>
    public const string SubSpecialityNotAdopted = "Choose a sub-speciality this person's institution has adopted.";

    /// <summary>The refusal of a sub-speciality outside the specialities the person will hold.</summary>
    public const string SubSpecialityNotUnderScope = "Choose a sub-speciality of the speciality this person is scoped to.";

    /// <summary>
    /// What each role's radio says under it in Add a role… (P03, P04, P08, P28; the nit). For a person scoped to a speciality
    /// the roles that may keep to one say the scope they will work in, and Sub-speciality admin asks nothing more; for a
    /// person at Any speciality they say what to choose, as Invite a person's do.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="personName">"Thandi Zulu".</param>
    /// <param name="scope">The person's scope in words (<see cref="PersonScopeWords" />).</param>
    /// <param name="holdsSpeciality">Whether the person's scope names a speciality.</param>
    public static string RoleHelp(string role, string personName, string scope, bool holdsSpeciality) => role switch
    {
        WombatRoles.InstitutionalAdmin => "Runs an institution in Wombat: invites and admits people, adopts curricula.",
        WombatRoles.SpecialityAdmin => "Runs one speciality's committee work: panels, reviews and decisions due.",
        WombatRoles.SubSpecialityAdmin => holdsSpeciality
            ? "Runs one sub-speciality's committee work."
            : "Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.",
        WombatRoles.Coordinator => holdsSpeciality
            ? $"Works across the institution, kept to {personName}'s scope: {scope}."
            : "Works across the institution. Choose a speciality only to keep them to it.",
        WombatRoles.CommitteeMember => holdsSpeciality
            ? $"Sits committee reviews, in {personName}'s scope: {scope}."
            : "Sits committee reviews. Choose a speciality only to keep them to it.",
        WombatRoles.Assessor => "Assesses the registrars who ask them, at this institution.",
        _ => WombatRoleLabels.For(role)
    };

    // ─── Sign-in and access (T376, flow 12, F3, F7) ─────────────────────────

    /// <summary>
    /// The reset's refusal for an account that may not use a Wombat password (F3: <c>AllowLocalPassword</c> false): the
    /// page offers no field for one, and the command refuses it, so a reset never sets a password no sign-in accepts.
    /// </summary>
    public const string PasswordNotSettable =
        "The password was not reset. This account signs in only through its institution, so a password cannot be set for it.";

    /// <summary>F7: the refusal of an institutional sign-in the account no longer has (another tab removed it).</summary>
    public const string SignInNotLinked = "That institutional sign-in is no longer linked to this account. Nothing was removed.";

    /// <summary>F7: the refusal when the account changed while the sign-in was being removed.</summary>
    public const string SignInChanged =
        "This account's sign-ins changed while the sign-in was being removed. Nothing was removed. Try again.";

    /// <summary>The refusal to remove the caller's own institutional sign-in here (flow 02's My account does it).</summary>
    public const string OwnSignInNotRemovable =
        "You cannot remove your own institutional sign-in here. Remove it from My account, which asks for your password.";

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
    /// The user, if this caller may change them; otherwise one refusal, <paramref name="refusal" />, whether the id names
    /// nobody or someone outside the caller's institution. (T290, T194's rule)
    /// </summary>
    /// <remarks>
    /// Asked straight after the lookup, before any check of what the user holds. Until T290 Add role, Remove role and
    /// password reset said "The user could not be found." where scope would refuse, and Add and Remove role and
    /// <c>SetUserLockout</c> gave the global-administrator refusal before the scope check, so an InstitutionalAdmin could
    /// tell which ids existed elsewhere and which belonged to an administrator. A global Administrator sees every user, so
    /// is told plainly that an id names nobody.
    /// </remarks>
    public static UserIdentityDetails DemandInScope(ClaimsPrincipal principal, UserIdentityDetails? user, string refusal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return user ?? throw new InvalidOperationException("The user could not be found.");
        }

        if (user?.InstitutionId is not int institutionId || !principal.CanAccessInstitution(institutionId))
        {
            throw new UnauthorizedAccessException(refusal);
        }

        return user;
    }

    /// <summary>
    /// Whether this caller holds a role that administers users, whatever else they hold: the roles the Users pages'
    /// policy admits (<c>AdministratorOrInstitutionalAdmin</c>). Not the right to administer them, which is
    /// <see cref="MayAdministerUsers" />: this only says which refusal a caller who may not is given.
    /// </summary>
    private static bool HoldsUserAdministrationRole(ClaimsPrincipal principal)
        => principal.IsAdministrator() || principal.IsInstitutionalAdmin();
}
