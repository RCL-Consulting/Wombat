using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.GetRequestsWaitingForUser;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Pages.Admin.Users;

/// <summary>
/// The user page's words (T376, flow 12; boards P01–P36; Spec 3's messages): its subtitle, the no-role sentences (S2), each
/// role's Remove dialog (S3, P36), the waiting requests (F6), the lockout list per person (Q17), and every result (A5). In
/// one place, so a test reads each sentence from here, never the page's markup. The page says "they": no pronoun is stored
/// (round 2's PROPOSED, settled), so F2's "for every role he holds" is "for every role they hold".
/// </summary>
public static class UserPageText
{
    // ─── The header ─────────────────────────────────────────────────────────

    /// <summary>The subtitle for another person (P02), naming the person's institution when the caller is scoped to one.</summary>
    public static string Subtitle(string? institutionName)
        => string.IsNullOrWhiteSpace(institutionName)
            ? "Roles, sign-in and lockout for one person, with assessor training for an Assessor."
            : $"Roles, sign-in and lockout for one person at {institutionName}, with assessor training for an Assessor.";

    /// <summary>The subtitle on the caller's own account (P01).</summary>
    public const string OwnSubtitle = "Your own account: your roles and how you sign in.";

    /// <summary>P24's loading line.</summary>
    public const string Loading = "Loading this person.";

    /// <summary>P25: a failed read, in fixed words (T329); the failure goes to the log.</summary>
    public const string LoadFailed = "Could not load this person. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>An action that failed for a reason no handler gave, in fixed words (T329).</summary>
    public const string ActionFailed = "That did not go through. Nothing has changed. Try again, or come back in a few minutes.";

    // ─── Roles and scope ────────────────────────────────────────────────────

    /// <summary>S2: the no-role sentence's first part, which the page bolds, for an ended programme or none.</summary>
    public static string NoRoleLead(string name, EndedProgrammeDto? ended) => ended switch
    {
        null => $"{name} holds no role.",
        { Completed: true } => $"{name} holds no role: they completed the programme on {ended.EndedOn:yyyy-MM-dd}.",
        _ => $"{name} holds no role: they left the programme on {ended.EndedOn:yyyy-MM-dd}."
    };

    /// <summary>S2: what is still open to a person with no role.</summary>
    public static string NoRoleRest(EndedProgrammeDto? ended)
        => ended is null
            ? "They can sign in to My account and My data rights."
            : "They can still sign in to read their training record (My progress) and their data rights.";

    /// <summary>Why a role has no Remove… (P13, P18): the two system-managed roles, and those this surface does not change.</summary>
    public static string RoleRowNote(string role) => role switch
    {
        WombatRoles.Trainee => "System-managed",
        WombatRoles.PendingTrainee => "System-managed: waiting to be admitted.",
        _ => "Not changed here."
    };

    /// <summary>A person with no institution (U14's College admin): Add a role… offers nothing, and this says why.</summary>
    public static string NoInstitutionForRoles(string name)
        => $"{name} has no institution, so no role can be added here: every role offered here works at an institution.";

    /// <summary>The Role fieldset's refusal when none is chosen.</summary>
    public const string ChooseARole = "Choose a role.";

    /// <summary>A required Speciality left at its placeholder (P08), in the built words.</summary>
    public const string ChooseSpeciality = "Speciality administrators must be scoped to a speciality.";

    /// <summary>A required Speciality or Sub-speciality left unchosen for a role that needs both, in the built words.</summary>
    public const string ChooseBoth = "The selected role requires speciality and sub-speciality scope.";

    /// <summary>P04: a read-out of what the person already holds (NEW; Spec 3 does not list it).</summary>
    public static string AlreadyHeld(string name) => $"Already held by {name}.";

    /// <summary>P29: the only speciality the person's institution has adopted (NEW; Spec 3 does not list it).</summary>
    public static string OnlySpeciality(string institutionName) => $"The only speciality {institutionName} has adopted.";

    /// <summary>P04: the only sub-speciality the person's institution has adopted.</summary>
    public static string OnlySubSpeciality(string institutionName) => $"The only sub-speciality {institutionName} has adopted.";

    /// <summary>The only sub-speciality of the person's speciality that the institution has adopted, beside others.</summary>
    public static string OnlySubSpecialityOf(string specialityName, string institutionName)
        => $"The only sub-speciality of {specialityName} {institutionName} has adopted.";

    /// <summary>Nothing adopted to choose from: the role cannot be added until a curriculum is adopted.</summary>
    public static string NothingAdopted(string institutionName)
        => $"{institutionName} has adopted nothing this role can be scoped to yet, so it cannot be added. Adopt a curriculum first.";

    /// <summary>The optional Speciality's placeholder (Coordinator, Committee member at Any speciality).</summary>
    public const string AnySpecialityOption = "Any speciality";

    /// <summary>F2: the narrowing, said before the save (P29).</summary>
    public static string Narrowing(string name, string scope) => $"{name}'s scope becomes {scope}, for every role they hold.";

    /// <summary>Add role's result (P05, P28, P30), its first sentence bolded by the page.</summary>
    public static string RoleAddedLead(string role, string name) => $"{WombatRoleLabels.For(role)} added to {name}.";

    /// <summary>
    /// What became of the scope (M2, F1, F2): it stays; it becomes narrower than Any speciality, for every role; or it is
    /// now another.
    /// </summary>
    public static string RoleAddedScope(string name, string before, string after)
        => string.Equals(before, after, StringComparison.Ordinal)
            ? $"{name}'s scope stays {after}. Any open session ends within a minute."
            : string.Equals(before, PersonScopeWords.AnySpeciality, StringComparison.Ordinal)
                ? $"{name}'s scope becomes {after}, for every role they hold. Any open session ends within a minute."
                : $"{name}'s scope is now {after}. Any open session ends within a minute.";

    // ─── Remove… (S3, P06, P36) ─────────────────────────────────────────────

    public static string RemoveTitle(string role, string name) => $"Remove {WombatRoleLabels.For(role)} from {name}?";

    public static string RemoveConfirm(string role) => $"Remove {WombatRoleLabels.For(role)}";

    public const string KeepTheRole = "Keep the role";

    /// <summary>The trigger's name (P02): "Remove Committee member from Thandi Zulu".</summary>
    public static string RemoveTriggerName(string role, string name) => $"Remove {WombatRoleLabels.For(role)} from {name}";

    /// <summary>What a role's removal does, one sentence each (P36), then that open sessions end.</summary>
    public static string RemoveEffect(string role, UserDetailDto person) => role switch
    {
        WombatRoles.CommitteeMember =>
            $"{person.Name} leaves every committee panel and can no longer sit reviews. If {person.Name} chairs a review, its " +
            "ratification waits until the panel has a chair again. Any open session ends within a minute.",
        WombatRoles.SpecialityAdmin => person.SpecialityNames.Count > 0
            ? $"{person.Name} no longer runs the {string.Join(", ", person.SpecialityNames)} committee's work: panels, reviews and decisions due. Any open session ends within a minute."
            : $"{person.Name} no longer runs a speciality's committee work: panels, reviews and decisions due. Any open session ends within a minute.",
        WombatRoles.SubSpecialityAdmin => PersonScopeWords.Disciplines(person.SpecialityNames, person.SubSpecialityNames) is { } disciplines
            ? $"{person.Name} no longer runs the {disciplines} committee's work. Any open session ends within a minute."
            : $"{person.Name} no longer runs a sub-speciality's committee work. Any open session ends within a minute.",
        WombatRoles.Coordinator =>
            $"{person.Name} can no longer run MSF campaigns or chase requests waiting for assessors; a campaign already under " +
            "way can still be finished. Any open session ends within a minute.",
        WombatRoles.InstitutionalAdmin => string.IsNullOrWhiteSpace(person.InstitutionName)
            ? $"{person.Name} can no longer invite, admit or manage people, or adopt curricula. Any open session ends within a minute."
            : $"{person.Name} can no longer invite, admit or manage people, or adopt curricula, at {person.InstitutionName}. Any open session ends within a minute.",
        WombatRoles.Assessor =>
            $"{person.Name} can no longer be named as an activity's assessor, and leaves the Assessors list. Any open session ends within a minute.",
        _ => $"{person.Name} no longer holds {WombatRoleLabels.For(role)}. Any open session ends within a minute."
    };

    /// <summary>Q18: removing Assessor keeps the training record.</summary>
    public static string TrainingKept(string name)
        => $"Any assessor training recorded for {name} is kept, and shows again if Assessor is given back.";

    /// <summary>P06: the scope is the person's, so it stays for the roles still held.</summary>
    public static string ScopeStaysFor(string name, string scope, IReadOnlyList<string> rolesLeft)
        => rolesLeft.Count == 1
            ? $"{name}'s scope stays {scope}, for {WombatRoleLabels.For(rolesLeft[0])}, the role still held."
            : $"{name}'s scope stays {scope}, for {string.Join(", ", rolesLeft.Select(WombatRoleLabels.For))}, the roles still held.";

    /// <summary>S3, S2: the last role's removal leaves no role.</summary>
    public static string WillHoldNoRole(string name) => $"{name} will hold no role. They can sign in to My account and My data rights.";

    /// <summary>Remove's result (P07), its first sentence bolded.</summary>
    public static string RoleRemovedLead(string role, string name) => $"{WombatRoleLabels.For(role)} removed from {name}.";

    public const string SessionsEnd = "Any open session ends within a minute.";

    // ─── The waiting requests (F6) ──────────────────────────────────────────

    /// <summary>
    /// The count and the oldest, which the page bolds up to the comma: "1 request is waiting for Mohammed Patel", then
    /// <see cref="WaitingSince" />.
    /// </summary>
    public static string WaitingLead(int count, string name)
        => count == 1 ? $"1 request is waiting for {name}" : $"{count} requests are waiting for {name}";

    /// <summary>"since 2026-10-12 12:35 SAST" for one, "the oldest since …" for more.</summary>
    public static string WaitingSince(WaitingRequestsDto waiting)
        => waiting.Count == 1
            ? $", since {ActivityMoments.When(waiting.OldestWaitingSince!.Value.UtcDateTime)}."
            : $", the oldest since {ActivityMoments.When(waiting.OldestWaitingSince!.Value.UtcDateTime)}.";

    /// <summary>
    /// The lock's: they stay, nobody else gets them, and a locked account is sent no reminder (the shared reminder policy
    /// skips a deactivated account, T240).
    /// </summary>
    public static string WaitingStayLocked(int count)
        => count == 1
            ? "It stays with them; it does not move to anyone else, and no reminder is sent about it."
            : "They stay with them; they do not move to anyone else, and no reminder is sent about them.";

    /// <summary>
    /// The Remove on Assessor's: they stay, and nobody else gets them. Not "no reminder is sent" (the lane's text): the
    /// nightly nudge still writes to a nominee who lost Assessor (<c>AssessorPendingNudgeJob</c>'s remarks), and the code wins.
    /// </summary>
    public static string WaitingStayRemoved(int count)
        => count == 1
            ? "It stays with them; it does not move to anyone else."
            : "They stay with them; they do not move to anyone else.";

    /// <summary>One row: "Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since … · overdue".</summary>
    public static string WaitingRowText(WaitingRow row)
    {
        var parts = new List<string> { row.TraineeName, row.ActivityTypeName };
        if (!string.IsNullOrWhiteSpace(row.Reference))
        {
            parts.Add(row.Reference);
        }

        parts.Add($"waiting since {ActivityMoments.When(row.WaitingSince.UtcDateTime)}");
        if (row.IsOverdue)
        {
            parts.Add("overdue");
        }

        return string.Join(" · ", parts);
    }

    /// <summary>P32: the rows not listed.</summary>
    public static string AndMore(int more) => $"and {more} more.";

    /// <summary>The waiting read failed when the dialog opened: said, so the dialog never implies there are none.</summary>
    public static string WaitingUnread(string name)
        => $"Could not check for requests waiting for {name}. Any that wait stay with them.";

    // ─── Sign-in and access ─────────────────────────────────────────────────

    /// <summary>A.4.5's result in Password, its first sentence bolded.</summary>
    public static string PasswordResetLead(string name) => $"Password reset for {name}.";

    public static string PasswordResetRest(string name) => $"Share the new password with {name} out-of-band.";

    /// <summary>F3: an account that may not use a Wombat password (P22).</summary>
    public static string SignsInOnlyThrough(string name, IReadOnlyList<string> providers)
        => providers.Count == 0
            ? $"{name} signs in only through their institution. A password cannot be set for them."
            : $"{name} signs in only through {string.Join(" or ", providers)}. A password cannot be set for them.";

    /// <summary>Q10: no institutional sign-in linked.</summary>
    public static string NoSignInLinked(string name, bool hasPassword)
        => hasPassword
            ? $"No institutional sign-in is linked. {name} signs in with an email and a password."
            : "No institutional sign-in is linked.";

    public static string SignInRemoveTriggerName(string provider, string name) => $"Remove the {provider} sign-in from {name}";

    public static string SignInRemoveTitle(string provider, string name) => $"Remove the {provider} sign-in from {name}?";

    /// <summary>
    /// What the removal leaves, by the account (T376 build review, P4): a password only where there is one; otherwise the
    /// other institutional sign-ins, as <see cref="SignInRemovedRest" /> says after it.
    /// </summary>
    public static string SignInRemoveBody(string provider, string name, bool hasPassword, int othersLeft)
        => hasPassword
            ? $"{name} can no longer sign in through {provider}. Signing in with the email and a password still works."
            : othersLeft == 1
                ? $"{name} can no longer sign in through {provider}. {name} still signs in through their other institutional sign-in."
                : $"{name} can no longer sign in through {provider}. {name} still signs in through their other institutional sign-ins.";

    public const string KeepTheSignIn = "Keep the sign-in";

    public const string RemoveSignIn = "Remove sign-in";

    /// <summary>P33's result, its first sentence bolded.</summary>
    public static string SignInRemovedLead(string provider, string name) => $"The {provider} sign-in was removed from {name}.";

    public static string SignInRemovedRest(string name, bool hasPassword, int othersLeft)
        => hasPassword
            ? $"{name} signs in with an email and a password."
            : othersLeft == 1 ? $"{name} signs in through their other institutional sign-in." : $"{name} signs in through their other institutional sign-ins.";

    /// <summary>The built last-way-in words (flow 02), beside a Remove… that would leave no way in.</summary>
    public const string LastWayIn = "The institutional sign-in is the account's only way in.";

    // ─── Lockout (Q5, Q17) ──────────────────────────────────────────────────

    public const string LockoutIntro = "Locking out the user prevents sign-in immediately. Until they are reactivated:";

    public const string LockoutPanel = "They sit on no committee panel.";

    public const string LockoutTrainee =
        "They are not a current trainee. No committee review or feedback campaign can be started about them, a draft " +
        "feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the " +
        "speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions due, " +
        "and the weekly digest's inactive trainees.";

    public const string LockoutReminders = "They are sent no activity reminders or weekly digests.";

    public const string LockoutAssessor = "They cannot be named as an activity's assessor.";

    public const string LockoutRecords =
        "Their records stay as they are, and a review or feedback campaign already under way can still be finished. " +
        "Reactivating clears the lockout.";

    public const string LockedNow = "This user is currently locked out and cannot sign in.";

    public const string AdministratorNotLockable = "Global administrators cannot be locked out from this surface.";

    /// <summary>
    /// The effects that apply to this person (Q17, the nit): the panel line for a Committee member, the trainee line for a
    /// Trainee, the reminders line for everyone, the assessor line for an Assessor.
    /// </summary>
    public static IReadOnlyList<string> LockoutEffects(IReadOnlyCollection<string> roles)
    {
        var effects = new List<string>();
        if (roles.Contains(WombatRoles.CommitteeMember, StringComparer.Ordinal))
        {
            effects.Add(LockoutPanel);
        }

        if (roles.Contains(WombatRoles.Trainee, StringComparer.Ordinal))
        {
            effects.Add(LockoutTrainee);
        }

        effects.Add(LockoutReminders);
        if (roles.Contains(WombatRoles.Assessor, StringComparer.Ordinal))
        {
            effects.Add(LockoutAssessor);
        }

        return effects;
    }

    public static string LockTriggerName(string name) => $"Lock out {name}";

    public static string LockTitle(string name) => $"Lock out {name}?";

    public static string LockBody(string name)
        => $"{name} can no longer sign in, and any open session ends within a minute. Until they are reactivated:";

    public const string KeepAccess = "Keep access";

    public static string LockedLead(string name) => $"{name} is locked out.";

    public static string Reactivated(string name) => $"{name} is reactivated and can sign in again.";
}
