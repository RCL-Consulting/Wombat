using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T303: a user's page offers the Trainee role under neither Add role nor Remove. Admission grants it and Mark complete
/// takes it away, so a held Trainee reads "System-managed", as a held PendingTrainee does, and the Add role field says
/// where a registrar is made a trainee.
/// </summary>
/// <remarks>
/// Until T303 Dr Molefe's page (runbook Step 2.28), a pending registrar's, offered Trainee under Add role beside
/// InstitutionalAdmin to Assessor, and after Mark complete (Step 5.17) it offered Trainee again. A held Trainee had a
/// Remove, which took the role from a running programme.
/// </remarks>
public sealed class UserDetailTraineeRoleTests : TestContext
{
    private const int InstitutionA = 1;

    private const string TraineeRoleNotOffered =
        "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit to curriculum'.";

    /// <summary>Every role Add role may offer, in its order: InstitutionalAdmin to Assessor.</summary>
    private static readonly string[] ManagedHere =
    [
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor
    ];

    private TestAuthorizationContext? _auth;

    public static TheoryData<string> UserAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin
    };

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void APendingRegistrarsPage_OffersInstitutionalAdminToAssessor_NotTrainee_AndMarksPendingTraineeSystemManaged(string role)
    {
        // Step 2.28: Dr Molefe, invited and awaiting admission.
        var cut = Render(role, User([WombatRoles.PendingTrainee]));

        AddRoleOptions(cut).Should().Equal(ManagedHere, role);
        RoleRows(cut).Should().Equal([(WombatRoles.PendingTrainee, "System-managed")], role);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void AGraduatesPage_WhichHoldsNoRole_OffersNoTrainee(string role)
    {
        // Step 5.17: Mark complete took the Trainee role away, and the page read "This user has no roles.".
        var cut = Render(role, User([]));

        cut.Markup.Should().Contain("This user has no roles.");
        AddRoleOptions(cut).Should().Equal(ManagedHere, role);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void ATraineesPage_MarksTheTraineeRoleSystemManaged_WithNoRemove_AndKeepsRemoveForARoleManagedHere(string role)
    {
        var cut = Render(role, User([WombatRoles.Trainee, WombatRoles.Coordinator]));

        RoleRows(cut).Should().Equal(
            [(WombatRoles.Trainee, "System-managed"), (WombatRoles.Coordinator, "Remove")], role);
        cut.FindAll("button").Select(button => button.GetAttribute("aria-label"))
            .Should().NotContain("Remove the Trainee role", role)
            .And.Contain("Remove the Coordinator role", "the control: a role the surface manages keeps its Remove");
        AddRoleOptions(cut).Should().NotContain(WombatRoles.Trainee, role)
            .And.NotContain(WombatRoles.Coordinator, "a held role is not offered again");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void TheAddRoleField_SaysWhereARegistrarIsMadeATrainee_AndTheSelectNamesIt(string role)
    {
        var cut = Render(role, User([WombatRoles.PendingTrainee]));

        var help = cut.Find("#add-role-select-help");
        Text(help).Should().Be(TraineeRoleNotOffered);
        cut.Find("#add-role-select").GetAttribute("aria-describedby").Should().Be("add-role-select-help");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private IRenderedComponent<UserDetail> Render(string callerRole, UserDetailDto user)
    {
        _auth ??= this.AddTestAuthorization();
        _auth.SetAuthorized("admin@test");
        _auth.SetRoles(callerRole);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "admin-1") };
        if (callerRole != WombatRoles.Administrator)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString()));
        }

        _auth.SetClaims(claims.ToArray());

        // The reset-password field's show/hide toggle calls into the browser; it is not what these tests are about.
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddSingleton<IScopedSender>(new FakeSender(user));
        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, user.UserId));
        cut.WaitForState(() => cut.FindAll("#add-role-select").Count == 1);
        return cut;
    }

    /// <summary>The roles Add role offers, by value, leaving out the empty "Select role".</summary>
    private static IReadOnlyList<string> AddRoleOptions(IRenderedComponent<UserDetail> cut)
        => cut.FindAll("#add-role-select option")
            .Select(option => option.GetAttribute("value") ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToArray();

    /// <summary>Each held role, and what its row offers: "Remove", or the muted note that stands in its place.</summary>
    private static IReadOnlyList<(string Role, string Offer)> RoleRows(IRenderedComponent<UserDetail> cut)
        => cut.FindAll(".stack-list li")
            .Select(row => (Text(row.QuerySelector("span")!), Text(row.Children[^1])))
            .ToArray();

    private static UserDetailDto User(IReadOnlyCollection<string> roles) => new(
        "molefe", "molefe@a.test", "Lerato", "Molefe", InstitutionA, "Institution A", [], [], roles, false, []);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(UserDetailDto user) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetUserByIdQuery
                ? Task.FromResult((TResponse)(object)user)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"No command is sent in these tests: {request.GetType().Name}");
    }
}
