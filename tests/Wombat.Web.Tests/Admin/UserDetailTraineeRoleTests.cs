using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T303: a user's page offers the Trainee role under neither Add a role… nor Remove…. Admission grants it and Mark complete
/// takes it away, so a held Trainee reads "System-managed", a held Pending trainee "System-managed: waiting to be admitted.",
/// and Add a role… says where a registrar is made a trainee. Since T376 (flow 12) the roles are radios in a note panel, by
/// their labels (Q14).
/// </summary>
/// <remarks>
/// Until T303 Dr Molefe's page (runbook Step 2.28), a pending registrar's, offered Trainee under Add role beside
/// InstitutionalAdmin to Assessor, and after Mark complete (Step 5.17) it offered Trainee again. A held Trainee had a
/// Remove, which took the role from a running programme.
/// </remarks>
public sealed class UserDetailTraineeRoleTests : UserDetailPageTestBase
{
    private const string TraineeRoleNotOffered =
        "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.";

    /// <summary>Every role Add a role… may offer, in its order: Institutional admin to Assessor.</summary>
    private static readonly string[] ManagedHere =
    [
        "Institutional admin", "Speciality admin", "Sub-speciality admin", "Coordinator", "Committee member", "Assessor"
    ];

    public static TheoryData<string> UserAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin
    };

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void APendingRegistrarsPage_OffersInstitutionalAdminToAssessor_NotTrainee_AndMarksPendingTraineeSystemManaged(string role)
    {
        SignIn(role);
        var cut = OpenPanel(Molefe([WombatRoles.PendingTrainee]));

        Offered(cut).Should().Equal(ManagedHere, role);
        RoleRows(cut).Should().Equal([("Pending trainee", "System-managed: waiting to be admitted. Open Trainees")], role);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void AGraduatesPage_WhichHoldsNoRole_OffersNoTrainee(string role)
    {
        // Step 5.17: Mark complete took the Trainee role away; since T376 the page says so (S2), not "This user has no roles.".
        SignIn(role);
        var cut = OpenPanel(Molefe([]));

        cut.Markup.Should().NotContain("This user has no roles.");
        cut.FindAll(".no-role").Should().ContainSingle();
        Offered(cut).Should().Equal(ManagedHere, role);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void ATraineesPage_MarksTheTraineeRoleSystemManaged_WithNoRemove_AndKeepsRemoveForARoleManagedHere(string role)
    {
        SignIn(role);
        var cut = OpenPanel(Molefe([WombatRoles.Trainee, WombatRoles.Coordinator]));

        RoleRows(cut).Should().Equal([("Coordinator", "Remove…"), ("Trainee", "System-managed")], role);
        cut.FindAll("button").Select(button => button.GetAttribute("aria-label"))
            .Should().NotContain("Remove Trainee from Lerato Molefe", role)
            .And.Contain("Remove Coordinator from Lerato Molefe", "the control: a role the surface manages keeps its Remove…");
        Offered(cut).Should().NotContain("Trainee", role).And.NotContain("Coordinator", "a held role is not offered again");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void AddARole_SaysWhereARegistrarIsMadeATrainee_ClosedAndOpen(string role)
    {
        SignIn(role);
        var cut = Open(Molefe([WombatRoles.PendingTrainee]), userId: "molefe");

        Text(Section(cut, "Roles and scope").QuerySelector("p.field-help")!).Should().Be(TraineeRoleNotOffered);

        cut.Find("#add-role-toggle").Click();
        Text(cut.Find("#add-role-panel p.field-help")).Should().Be(TraineeRoleNotOffered);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private IRenderedComponent<UserDetail> OpenPanel(UserDetailDto user)
    {
        var cut = Open(user, userId: user.UserId);
        cut.Find("#add-role-toggle").Click();
        return cut;
    }

    /// <summary>The roles Add a role… offers, by their labels.</summary>
    private static IReadOnlyList<string> Offered(IRenderedComponent<UserDetail> cut)
        => cut.FindAll("#add-role-panel fieldset .form-check label").Select(Text).ToArray();

    /// <summary>Each held role, and what its row offers: Remove…, or the note that stands in its place.</summary>
    private static IReadOnlyList<(string Role, string Offer)> RoleRows(IRenderedComponent<UserDetail> cut)
        => cut.FindAll(".role-row")
            .Select(row => (Text(row.QuerySelector(".role-row-label")!), Text(row.Children[^1])))
            .ToArray();

    private static UserDetailDto Molefe(IReadOnlyCollection<string> roles) => new(
        "molefe", "molefe@kgk.wombat.local", "Lerato", "Molefe", Kgk, KgkName, [], [], roles, false);
}
