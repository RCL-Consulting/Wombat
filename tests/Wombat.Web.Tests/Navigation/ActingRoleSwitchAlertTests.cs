using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Web.Components;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// T335, flow 01 (R2-Rules § 1, R2-Detail-Email, R2-Landing-Assessor): after a switch, the page it lands on says "You are
/// now acting as …" in an info alert under its header, takes the focus there, and, away from Home, offers to switch back
/// to the same page. Once: a word for another role, or one whose page has been left, says nothing.
/// </summary>
public sealed class ActingRoleSwitchAlertTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    /// <summary>What <c>FocusOnNavigate</c> calls.</summary>
    private const string FocusBySelectorIdentifier = "Blazor._internal.domWrapper.focusBySelector";

    private const string Title = "Mini-CEX for Anele Dlamini";

    private static readonly ActingRole ZuluAsAssessor =
        ActingRoleResolver.Resolve(WombatRoles.Assessor, [WombatRoles.CommitteeMember, WombatRoles.Assessor]);

    public ActingRoleSwitchAlertTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // R2-Detail-Email: she opened the rating request from her email as Committee member; the link switched her.
    [Fact]
    public void AfterASwitch_TheLandingPageSaysSo_AndOffersToSwitchBackToTheSamePage()
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/activities/21?tab=history#rating");

        var cut = RenderHeader(ZuluAsAssessor, new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor));

        var region = cut.Find(".header-container + .action-result");
        var alert = region.QuerySelector(".alert.alert-info")!;
        alert.GetAttribute("role").Should().Be("status");
        alert.QuerySelector("span")!.TextContent.Should().Be("You are now acting as Assessor.");

        var back = alert.QuerySelector("a")!;
        back.TextContent.Trim().Should().Be("Switch back to Committee member");
        back.ClassList.Should().Contain(["btn", "btn-outline", "btn-sm"]);
        back.GetAttribute("data-enhance-nav").Should().Be("false", "a switch is a full page load, from a static page too");
        back.GetAttribute("href").Should().Be(
            "/dashboard/switch/CommitteeMember?returnUrl=%2Factivities%2F21%3Ftab%3Dhistory",
            "the same page, with its query and without its fragment");
    }

    // R2-Landing-Assessor: a switch that names no return address lands on Home, where the sidebar's switch is the way back.
    [Fact]
    public void OnHome_ThereIsNoSwitchBack()
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/");

        var cut = RenderHeader(ZuluAsAssessor, new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor));

        cut.Find(".alert").TextContent.Trim().Should().Be("You are now acting as Assessor.");
        cut.FindAll(".alert a").Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.Coordinator)]
    public void NoSwitchBack_WhenThereWasNoOtherRoleHeldToGoBackTo(string? from)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/activities/21");

        var cut = RenderHeader(ZuluAsAssessor, new ActingRoleSwitchResult(from, WombatRoles.Assessor));

        cut.Find(".alert span").TextContent.Should().Be("You are now acting as Assessor.");
        cut.FindAll(".alert a").Should().BeEmpty();
    }

    // Only a word whose new role is the acting role is said: one left over from an earlier switch, or naming a role the
    // frame is not showing, says nothing.
    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Administrator)]
    public void AWordForAnotherRoleThanTheActingOne_SaysNothing(string to)
    {
        var cut = RenderHeader(ZuluAsAssessor, new ActingRoleSwitchResult(WombatRoles.Assessor, to));

        cut.FindAll(".action-result").Should().BeEmpty();
        cut.Markup.Should().NotContain("acting as");
    }

    [Fact]
    public void NoWord_SaysNothing_AndThePageHeaderIsAsItWas()
    {
        var withNone = RenderHeader(ZuluAsAssessor, result: null);
        var uncascaded = RenderComponent<PageHeader>(parameters => parameters.Add(header => header.Title, Title));

        withNone.FindAll(".action-result").Should().BeEmpty();
        uncascaded.FindAll(".action-result").Should().BeEmpty();
        uncascaded.Markup.Should().Be(withNone.Markup);
    }

    // Once: the circuit's first navigation leaves the page it landed on (Routes marks it), and the next page's header says
    // nothing. The landing page's own header keeps what it showed.
    [Fact]
    public void OnceThePageIsLeft_TheNextPageSaysNothing()
    {
        var notice = new ActingRoleSwitchNotice(new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor));
        var landing = RenderHeader(ZuluAsAssessor, notice);

        notice.LeavePage();
        var next = RenderHeader(ZuluAsAssessor, notice);

        next.FindAll(".action-result").Should().BeEmpty();
        landing.Render();
        landing.FindAll(".action-result .alert").Should().ContainSingle();
    }

    [Fact]
    public void TheAlert_TakesTheFocus_OnceForThePage()
    {
        var notice = new ActingRoleSwitchNotice(new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor));

        var first = RenderHeader(ZuluAsAssessor, notice);

        // Prerendered or static, the region carries autofocus; interactive, it is focused after its first render.
        var region = first.Find(".action-result");
        region.HasAttribute("autofocus").Should().BeTrue();
        region.GetAttribute("tabindex").Should().Be("-1");
        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(first.FindComponent<ActionResult>().Instance.Element.Id);

        // A header drawn again on the same page (a page that reloads what it shows) does not pull the focus back.
        var again = RenderHeader(ZuluAsAssessor, notice);
        again.Find(".action-result").HasAttribute("autofocus").Should().BeFalse();
        JSInterop.Invocations.Count(invocation => invocation.Identifier == FocusIdentifier).Should().Be(1);
    }

    // The focus must win over FocusOnNavigate, which focuses the h1 after the same first render. The real Routes, on a
    // real page (Page not found, whose header needs nothing loaded), as the circuit renders it: FocusOnNavigate sits
    // beside AuthorizeRouteView, so it renders before any component of the page, its call goes first, and the alert's
    // focus comes after it. Changed deliberately by T339 (flow 02): the page was change password, which is static now, and
    // so never in a circuit.
    [Fact]
    public void InTheRealRoutes_TheAlertIsFocusedAfterFocusOnNavigateHasFocusedTheHeading()
    {
        AddRoutesServices();
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("t.zulu@kgk.test");
        auth.SetRoles(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "zulu"), new Claim(WombatClaimTypes.ActingRole, WombatRoles.Assessor));
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/not-found");

        var cut = RenderComponent<Routes>(parameters => parameters
            .Add(routes => routes.Acting, ZuluAsAssessor)
            .Add(routes => routes.SwitchResult, new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor)));

        cut.WaitForAssertion(() => cut.Find(".action-result .alert span").TextContent.Should().Be("You are now acting as Assessor."));
        cut.Find("h1").TextContent.Should().Be("Page not found", "guard: the page is the one asked for");

        var focusCalls = JSInterop.Invocations
            .Where(invocation => invocation.Identifier is FocusIdentifier or FocusBySelectorIdentifier)
            .Select(invocation => invocation.Identifier)
            .ToList();
        focusCalls.Should().Equal([FocusBySelectorIdentifier, FocusIdentifier], "the heading first, then the alert, which wins");
        JSInterop.Invocations.Single(invocation => invocation.Identifier == FocusBySelectorIdentifier).Arguments[0].Should().Be("h1");
    }

    // A navigation inside the circuit leaves the landing page: Routes marks the word spent, so the next page is silent.
    [Fact]
    public void InTheRealRoutes_ANavigationLeavesTheWordBehind()
    {
        AddRoutesServices();
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("t.zulu@kgk.test");
        auth.SetRoles(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "zulu"));
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo("/not-found");

        var cut = RenderComponent<Routes>(parameters => parameters
            .Add(routes => routes.Acting, ZuluAsAssessor)
            .Add(routes => routes.SwitchResult, new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor)));
        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert").Should().ContainSingle());

        navigation.NavigateTo("/access-denied");

        cut.WaitForAssertion(() => cut.Find("h1").TextContent.Should().NotBe("Page not found", "guard: another page is shown"));
        cut.FindAll(".page-header, .header-container").Should().NotBeEmpty("guard: the next page has a header that could say it");
        cut.Markup.Should().NotContain("You are now acting as");
    }

    // The review of the t335 branch: Blazor.resumeCircuit() starts a new circuit from the page's own descriptor, Routes'
    // parameters and the switch's result included, so a resumed circuit said the result again. The first circuit to render
    // a result says it and the server records its nonce (ActingRoleSwitchResults); a later circuit given the same result
    // says nothing. The prerender, which is no circuit, says it as it always did.
    [Fact]
    public void AResumedCircuit_DoesNotSayTheResultAgain_WhichThePrerenderAndTheFirstCircuitSaid()
    {
        AddRoutesServices();
        SignInZulu();
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/not-found");
        var result = new ActingRoleSwitchResult(WombatRoles.CommitteeMember, WombatRoles.Assessor, Nonce: "3F2A9C01B7D84E6F9A0B1C2D3E4F5061");

        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        Said(RenderRoutes(result)).Should().Be("You are now acting as Assessor.", "the prerender says it, and records nothing");

        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Said(RenderRoutes(result)).Should().Be("You are now acting as Assessor.", "the page's circuit says it once more, as it replaces the prerender");

        var resumed = RenderRoutes(result);
        resumed.WaitForAssertion(() => resumed.Find("h1").TextContent.Should().Be("Page not found", "guard: the page is drawn"));
        resumed.Markup.Should().NotContain("You are now acting as", "a circuit resumed from the page's descriptor says nothing again");

        var another = new ActingRoleSwitchResult(WombatRoles.Assessor, WombatRoles.CommitteeMember, Nonce: "00112233445566778899AABBCCDDEEFF");
        Said(RenderRoutes(another, ActingRoleResolver.Resolve(WombatRoles.CommitteeMember, [WombatRoles.CommitteeMember, WombatRoles.Assessor])))
            .Should().Be("You are now acting as Committee member.", "another switch is another result, said in its own circuit");
    }

    /// <summary>What the real Routes needs registered beside the page it shows (Page not found's header needs nothing loaded).</summary>
    private void AddRoutesServices()
    {
        Services.AddSingleton(new IdentityErrorDescriber());
        Services.AddSingleton(Options.Create(new IdentityOptions()));
        Services.AddWombatCircuitServices();
        Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        Services.AddActingRoleSwitch();
    }

    private void SignInZulu()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("t.zulu@kgk.test");
        auth.SetRoles(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "zulu"));
    }

    private IRenderedComponent<Routes> RenderRoutes(ActingRoleSwitchResult result, ActingRole? acting = null)
        => RenderComponent<Routes>(parameters => parameters
            .Add(routes => routes.Acting, acting ?? ZuluAsAssessor)
            .Add(routes => routes.SwitchResult, result));

    /// <summary>What the page's switch alert says, once the page has rendered; null when it says nothing.</summary>
    private static string? Said(IRenderedComponent<Routes> cut)
    {
        cut.WaitForAssertion(() => cut.FindAll("h1").Should().NotBeEmpty("guard: the page is drawn"));
        return cut.FindAll(".action-result .alert span").FirstOrDefault()?.TextContent.Trim();
    }

    private IRenderedComponent<PageHeader> RenderHeader(ActingRole acting, ActingRoleSwitchResult? result)
        => RenderHeader(acting, new ActingRoleSwitchNotice(result));

    private IRenderedComponent<PageHeader> RenderHeader(ActingRole acting, ActingRoleSwitchNotice notice)
        => RenderComponent<PageHeader>(parameters => parameters
            .Add(header => header.Title, Title)
            .AddCascadingValue(acting)
            .AddCascadingValue(notice));
}
