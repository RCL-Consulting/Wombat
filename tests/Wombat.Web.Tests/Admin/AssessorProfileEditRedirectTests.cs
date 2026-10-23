using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Assessors;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Assessors;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T376 (flow 12, lane T, S10): the old <c>/admin/assessors/edit?id=‹profile id›</c> follows the profile, in the caller's
/// scope, to the person's page with Assessor training open, replacing itself in the history; an unknown, out-of-scope,
/// missing or malformed id reads as the user page's "User unavailable", under Users.
/// </summary>
public sealed class AssessorProfileEditRedirectTests : TestContext
{
    private readonly FakeSender _sender = new();

    public AssessorProfileEditRedirectTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("mbatha@kgk.test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"), new Claim(WombatClaimTypes.InstitutionId, "1"));
    }

    [Fact]
    public void AProfileId_LeadsToItsPersonsTrainingSection()
    {
        _sender.Owners[6] = "zulu";

        RenderAt("/admin/assessors/edit?id=6");

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.Uri.Should().EndWith("/admin/users/zulu?section=training");
        _sender.Asked.Should().Equal(6);
    }

    [Theory]
    [InlineData("/admin/assessors/edit?id=99", true)]
    [InlineData("/admin/assessors/edit", false)]
    [InlineData("/admin/assessors/edit?id=abc", false)]
    [InlineData("/admin/assessors/edit?id=-3", false)]
    public void AnUnknownOutOfScopeMissingOrMalformedId_IsUserUnavailable(string address, bool asked)
    {
        var cut = RenderAt(address);

        cut.WaitForState(() => cut.FindAll(".detail-card--empty").Count > 0);
        cut.Find("h1").TextContent.Should().Be(UserPageWords.Heading);
        cut.Find(".state-panel-title").TextContent.Should().Be(UserPageWords.UnavailableTitle);
        cut.Find(".state-panel-copy").TextContent.Should().Be(UserPageWords.UnavailableBody);
        var goToUsers = cut.Find(".detail-card--empty a.btn");
        goToUsers.TextContent.Trim().Should().Be(UserPageWords.GoToUsers);
        goToUsers.GetAttribute("href").Should().Be("/admin/users");
        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith(address);
        (_sender.Asked.Count > 0).Should().Be(asked);
    }

    [Fact]
    public void TheRoute_SitsUnderUsers_AsThePersonsPageDoes()
    {
        var role = WombatRoles.InstitutionalAdmin;
        var menu = NavItems.For(
            ActingRoleResolver.Resolve(role, [role]),
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test")));

        NavOwners.Lit(typeof(AssessorProfileEdit), role, menu).Item.Should().Be(NavItems.Users);
    }

    private IRenderedComponent<AssessorProfileEdit> RenderAt(string address)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(address);
        return RenderComponent<AssessorProfileEdit>();
    }

    private sealed class FakeSender : IScopedSender
    {
        public Dictionary<int, string> Owners { get; } = [];
        public List<int> Asked { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetAssessorProfileOwnerQuery owner)
            {
                Asked.Add(owner.ProfileId);
                return Task.FromResult((TResponse)(object?)Owners.GetValueOrDefault(owner.ProfileId)!);
            }

            throw new NotSupportedException(request.GetType().Name);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
