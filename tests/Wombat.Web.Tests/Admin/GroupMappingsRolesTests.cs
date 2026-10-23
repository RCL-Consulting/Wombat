using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Sso;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The SSO mappings page offers no role a mapping may not give: not Administrator, and not Trainee or PendingTrainee,
/// which only admission gives (T333). It offers only the caller's institution's providers, and someone who holds Trainee
/// beside an admin role is shown T278's note in place of everything (T288).
/// </summary>
public sealed class GroupMappingsRolesTests : TestContext
{
    private readonly TestAuthorizationContext _auth;

    public GroupMappingsRolesTests()
    {
        _auth = this.AddTestAuthorization();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(Options.Create(new SsoOptions
        {
            Providers =
            [
                new SsoProviderOptions { Key = "kgk", DisplayName = "KGK sign-in", InstitutionId = 4 },
                new SsoProviderOptions { Key = "wits", DisplayName = "Wits sign-in", InstitutionId = 5 }
            ]
        }));
    }

    [Fact]
    public void TheRoleChoices_LeaveOutAdministrator_Trainee_AndPendingTrainee()
    {
        SignIn([WombatRoles.InstitutionalAdmin]);
        var sender = new EmptySender();

        var cut = Render(sender);

        var roles = cut.FindAll("#mapping-role option").Select(option => option.GetAttribute("value")).ToList();
        roles.Should().Contain(WombatRoles.Assessor);
        roles.Should().NotContain([WombatRoles.Trainee, WombatRoles.PendingTrainee, WombatRoles.Administrator]);
    }

    [Fact]
    public void TheProviders_AreTheCallersInstitutionsOnly()
    {
        SignIn([WombatRoles.InstitutionalAdmin]);

        var cut = Render(new EmptySender());

        cut.FindAll("#mapping-provider option").Select(option => option.TextContent).Should().Equal("KGK sign-in");
    }

    [Fact]
    public void ATraineeWhoAdministers_IsShownTheNote_AndNothingIsRead()
    {
        SignIn([WombatRoles.InstitutionalAdmin, WombatRoles.Trainee]);
        var sender = new EmptySender();

        var cut = Render(sender);

        cut.Find("#sso-mappings-trainee-note").TextContent.Should().Contain(UserAdministrationRules.TraineeAdministersNoUser);
        cut.FindAll("form").Should().BeEmpty();
        sender.Requests.Should().BeEmpty("the note is decided before anything is read");
    }

    private IRenderedComponent<GroupMappings> Render(EmptySender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<GroupMappings>();
    }

    private void SignIn(string[] roles)
    {
        _auth.SetAuthorized("caller@test");
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(WombatClaimTypes.InstitutionId, "4"));
    }

    /// <summary>Answers every list query with an empty list, and records what was asked.</summary>
    private sealed class EmptySender : IScopedSender
    {
        public List<object> Requests { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var element = typeof(TResponse).GetGenericArguments().Single();
            return Task.FromResult((TResponse)(object)Array.CreateInstance(element, 0));
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.CompletedTask;
        }
    }
}
