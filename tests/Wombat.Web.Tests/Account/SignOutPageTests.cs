using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Account;

/// <summary>
/// The sign-out page's words (T339, flow 02, R2-SO and R3-SO-Notes): who is signed in, by name and email as stored, and
/// what signing out does. <c>Hosting/SignOutPageHostingTests</c> holds the page as a browser receives it.
/// </summary>
public sealed class SignOutPageTests : TestContext
{
    [Fact]
    public void ThePage_NamesWhoIsSignedIn_ByNameAndEmail()
    {
        var cut = Render(
            new Claim(WombatClaimTypes.DisplayName, "Mohammed Patel"),
            new Claim(ClaimTypes.Email, "patel@kgk.wombat.local"));

        cut.Find("h1").TextContent.Trim().Should().Be("Sign out");
        cut.Find("p").TextContent.Trim().Should().Be(
            "You are signed in as Mohammed Patel (patel@kgk.wombat.local). Signing out ends your session in this browser.");
        cut.Find("form").GetAttribute("action").Should().Be(SignOutOutcome.SubmitPath);
        cut.FindComponents<Microsoft.AspNetCore.Components.Forms.AntiforgeryToken>().Should().ContainSingle(
            "the post requires its token (E6; bUnit renders it empty)");
    }

    [Fact]
    public void AnAccountWithNoName_IsNamedByItsEmailOnce()
    {
        // The display name of an account with no name is its email (WombatUserClaimsPrincipalFactory.DisplayNameOf).
        var cut = Render(
            new Claim(WombatClaimTypes.DisplayName, "patel@kgk.wombat.local"),
            new Claim(ClaimTypes.Email, "patel@kgk.wombat.local"));

        cut.Find("p").TextContent.Trim().Should().Be(
            "You are signed in as patel@kgk.wombat.local. Signing out ends your session in this browser.");
    }

    [Fact]
    public void AVisitorWhoIsNotSignedIn_IsSentToSignIn_WithNoReturnAddress()
    {
        this.AddTestAuthorization();
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo(SignOutOutcome.PagePath);

        RenderComponent<Logout>();

        navigation.Uri.Should().Be(navigation.BaseUri + "account/login");
    }

    private IRenderedComponent<Logout> Render(params Claim[] claims)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("patel@kgk.wombat.local");
        auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, "patel"), .. claims]);
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(SignOutOutcome.PagePath);
        return RenderComponent<Logout>();
    }
}
