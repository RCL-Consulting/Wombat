using System.Globalization;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The onboarding pages' addresses (T373, flow 11, build-contracts § 0), so no page writes one by hand: the invitations
/// list and Invite a person (lane I), Trainees and Admit (lane A), and the two pages both link to (a user's page, Curriculum
/// adoptions). The menu's items (<c>NavItems.Invitations</c>, <c>NavItems.Trainees</c>) keep their own literals, which a test
/// holds equal to these. Register's addresses are <c>RegisterOutcome</c>'s.
/// </summary>
public static class OnboardingLinks
{
    /// <summary>The invitations list: <c>/admin/invitations</c>.</summary>
    public const string Invitations = "/admin/invitations";

    /// <summary>Invite a person, the issue form's page of its own (Q1): <c>/admin/invitations/new</c>.</summary>
    public const string InviteAPerson = "/admin/invitations/new";

    /// <summary>Trainees, the Waiting and Current lists: <c>/admin/trainees</c>.</summary>
    public const string Trainees = "/admin/trainees";

    /// <summary>Curriculum adoptions, where an institution adopts what its registrars are admitted to (N26, M04).</summary>
    public const string CurriculumAdoptions = "/admin/adoptions";

    /// <summary>
    /// Invite a person, filled from an invitation in the caller's scope (N19: a revoked one's role and scope, the address
    /// left empty; N20: an expired one's address, role and scope; Q9): <c>/admin/invitations/new?from=12</c>. The page reads
    /// the invitation again in scope; the address is never in the URL.
    /// </summary>
    public static string InviteFrom(int invitationId) => Invariant($"{InviteAPerson}?from={invitationId}");

    /// <summary>Admit ‹name› (Q5, Q17): <c>/admin/trainees/admit?userId=…</c>.</summary>
    public static string Admit(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return $"/admin/trainees/admit?userId={Uri.EscapeDataString(userId)}";
    }

    /// <summary>
    /// Trainees after an admission (S8): <c>/admin/trainees?admitted=7</c>. The page re-reads that profile in the caller's
    /// scope and writes the sentence; the URL carries the id only, never a name, a curriculum or a date.
    /// </summary>
    public static string Admitted(int traineeProfileId) => Invariant($"{Trainees}?admitted={traineeProfileId}");

    /// <summary>
    /// Trainees after Cancel on Admit (T373, lane A; T10): <c>/admin/trainees?back=…</c>, so the registrar's own Admit…
    /// takes the focus again. The id alone, as <see cref="Admit" /> carries it.
    /// </summary>
    public static string BackFromAdmit(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return $"{Trainees}?back={Uri.EscapeDataString(userId)}";
    }

    /// <summary>A user's page (N15's "open Pieter Smit under Users", M13's "Reactivate it under Users"): <c>/admin/users/…</c>.</summary>
    public static string User(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return $"/admin/users/{Uri.EscapeDataString(userId)}";
    }

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
