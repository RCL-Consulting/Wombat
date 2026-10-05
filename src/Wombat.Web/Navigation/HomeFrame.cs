using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Web.Navigation;

/// <summary>
/// Home's frame, as round 3 draws it (T335, flow 01; R2-Landing-*, R2-Shell-*, R2-Phone-Folded; review S20, S22(c)): the
/// heading, the subtitle and the one header action, per acting role. The cards inside belong to later flows.
/// </summary>
/// <remarks>
/// <para>
/// Until T335 Home was headed "Welcome, {email}" with "Viewing as {role key}" under it. The shell now names the person
/// (the top bar) and the role (the sidebar's head), so Home is headed by its nav label, and the subtitle says which frame
/// this is and which period the cards count in.
/// </para>
/// <para>
/// The semester is the national academic year's (<see cref="AcademicPeriod" />, D13), on the South African calendar
/// (<see cref="QuotaCalendar" />), so the subtitle names the same "Semester 2, 2026" as the progress pages and the
/// committee's targets card do on the same day.
/// </para>
/// </remarks>
public static class HomeFrame
{
    /// <summary>Home's heading, and its tab's stem: its nav label (D10).</summary>
    public const string Heading = "Home";

    /// <summary>
    /// "Assessor · Semester 2, 2026": the acting role's label and the semester holding <paramref name="today" />. Null for
    /// someone who holds no role, who has no acting role to name (D8).
    /// </summary>
    public static string? Subtitle(string? actingRole, DateOnly today)
        => actingRole is null
            ? null
            : $"{WombatRoleLabels.For(actingRole)} · {QuotaText.SemesterName(AcademicPeriod.Containing(today))}";

    /// <summary>
    /// The one action Home's header offers the acting role, or null (round 1's A-Spec § e): the main job that starts from
    /// Home rather than from a card's row.
    /// </summary>
    /// <remarks>
    /// Three roles have one. The Trainee's main job is filing an observation, the Institutional admin's is inviting a
    /// person (the invitation form is on the invitations page), and the Coordinator's is starting an MSF campaign, MSF
    /// campaigns' new-campaign form (T358, flow 06, Q6; review 26: until then a "Quick action" card on the Coordinator's
    /// Home). Every other role's main job is a row of a card on Home, or a nav link, and a pending trainee can file nothing
    /// yet. The link opens a page that admits the role, as every link on Home does (DashboardLinkAuthorizationTests).
    /// </remarks>
    public static HomeAction? ActionFor(string? actingRole) => actingRole switch
    {
        WombatRoles.Trainee => new HomeAction("Log an activity", "/activities/new", "circle-plus"),
        WombatRoles.InstitutionalAdmin => new HomeAction("Invite a person", "/admin/invitations", "user-plus"),
        WombatRoles.Coordinator => new HomeAction("Start an MSF campaign", "/msf/campaigns/new", "plus"),
        _ => null
    };
}

/// <summary>A header action on Home: its words, where it goes, and its Lucide icon.</summary>
public sealed record HomeAction(string Label, string Href, string Icon);
