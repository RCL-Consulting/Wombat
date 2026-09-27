using System.Diagnostics;
using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Domain.Identity;
using Wombat.Web.Components;
using Wombat.Web.Components.Layout;
using Wombat.Web.Components.Pages;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using ErrorPage = Wombat.Web.Components.Pages.Error;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// T335, flow 01 (R2-Denied-*, R2-NotFound-*, R2-Error-*; the round-2 review, S1 to S6 and D6) and T321: Access denied,
/// Page not found and the error page, signed in and signed out, in the words round 3 gives them, drawn once in the one
/// shell.
/// </summary>
/// <remarks>
/// Until T335 Access denied read "Your current role does not allow access to this area", as if another role might; in-app
/// navigation drew it inside a second copy of the layout (T321); Page not found offered "Back to home"; and the error page
/// showed a 55-character request id its circuit then lost.
/// </remarks>
public sealed class SystemPagesTests : TestContext
{
    private static readonly DateTimeOffset AFailure = new(2026, 9, 26, 13, 14, 0, TimeSpan.Zero);

    // ---- Access denied, signed in: why, from the claims alone ----

    [Theory]
    [InlineData(new[] { WombatRoles.Trainee }, "Your role (Trainee) does not open this page.")]
    [InlineData(new[] { WombatRoles.Assessor, WombatRoles.CommitteeMember }, "None of your roles (Committee member, Assessor) opens this page.")]
    [InlineData(new string[0], "Your account holds no role that opens this page.")]
    public void AccessDenied_SignedIn_SaysWhyFromTheRolesHeld(string[] roles, string refusal)
    {
        var cut = RenderSignedIn<AccessDenied>(roles, "/access-denied?ReturnUrl=%2Fadmin%2Faudit");

        cut.Find("h1").TextContent.Should().Be("You cannot open this page");
        TabTitle.Of(this, cut).Should().Be("You cannot open this page · Wombat", "the tab says what the heading says");
        var lines = cut.FindAll(".system-panel p").Select(Text).ToList();
        lines.Should().Equal(refusal, "If you need it for your work, ask your institution's Wombat administrator.");
        Links(cut).Should().Equal([("Go to Home", "/")], "no switch is offered: access is the union of the roles held (S1)");
        cut.Markup.Should().NotContain("audit", "the page it refused is never named (D6)");
    }

    // S1: "ask your institution's administrator" sent an Institutional admin to herself.
    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.CollegeAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void AccessDenied_ForAnAdministratorOfAnything_AsksThePlatformAdministrator(string role)
    {
        var cut = RenderSignedIn<AccessDenied>([role, WombatRoles.Assessor], "/access-denied");

        Text(cut.FindAll(".system-panel p").ElementAt(1)).Should().Be("If you need it for your work, ask the platform administrator.");
    }

    [Fact]
    public void AccessDenied_NamesNeitherThePageNorTheRolesThatWouldOpenIt()
    {
        var cut = RenderSignedIn<AccessDenied>([WombatRoles.Trainee], "/access-denied?ReturnUrl=%2Fadmin%2Fusers");

        cut.Markup.Should().NotContainAny(["users", "Users", "Administrator", "Institutional admin"]);
    }

    // ---- Access denied, signed out: sign in, and come back to the page asked for ----

    [Theory]
    [InlineData("%2Fadmin%2Faudit", "/account/login?ReturnUrl=%2Fadmin%2Faudit")]
    [InlineData("%2Factivities%2Finbox%3Fpage%3D2", "/account/login?ReturnUrl=%2Factivities%2Finbox%3Fpage%3D2")]
    public void AccessDenied_SignedOut_WithAPageAskedFor_SignsInAndComesBack(string returnUrl, string signIn)
    {
        var cut = RenderSignedOut<AccessDenied>($"/access-denied?ReturnUrl={returnUrl}");

        cut.Find("h1").TextContent.Should().Be("Sign in to open this page");
        TabTitle.Of(this, cut).Should().Be("Sign in to open this page · Wombat");
        Text(cut.Find(".system-card p")).Should().Be("After you sign in, Wombat brings you back to the page you asked for.");
        Links(cut).Should().Equal([("Sign in", signIn)]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?ReturnUrl=")]
    [InlineData("?ReturnUrl=%2F%2Fevil.example")]
    [InlineData("?ReturnUrl=https%3A%2F%2Fevil.example%2F")]
    [InlineData("?ReturnUrl=%2F%5Cevil.example")]
    [InlineData("?ReturnUrl=%2Faccess-denied")]
    public void AccessDenied_SignedOut_WithNoPageToComeBackTo_SaysSignInToCarryOn(string query)
    {
        var cut = RenderSignedOut<AccessDenied>($"/access-denied{query}");

        Text(cut.Find(".system-card p")).Should().Be("Sign in to carry on.");
        Links(cut).Should().Equal([("Sign in", "/account/login")]);
        cut.Markup.Should().NotContain("evil");
    }

    // ---- Access denied, reached in the app: in place, drawn once ----

    // T321, D6: in-app navigation to a page the role may not open renders Access denied at that address. Routes wrapped it
    // in a LayoutView inside AuthorizeRouteView's NotAuthorized, which AuthorizeRouteView already draws in its default
    // layout: two sidebars, two top rows, two Sign out buttons.
    [Fact]
    public void AccessDenied_ReachedInTheApp_IsDrawnOnce_InTheOneShell()
    {
        Services.AddSingleton(new IdentityErrorDescriber());
        Services.AddSingleton(Options.Create(new IdentityOptions()));
        Services.AddWombatCircuitServices();
        Services.AddSingleton<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>(
            new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider());
        Services.AddActingRoleSwitch();
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("patel@kgk.test");
        auth.SetRoles(WombatRoles.Assessor);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "patel"));
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/admin/audit");

        var cut = RenderComponent<Routes>(parameters => parameters
            .Add(routes => routes.Acting, ActingRoleResolver.Resolve(stored: null, [WombatRoles.Assessor])));

        cut.WaitForAssertion(() => cut.FindAll("h1").Should().ContainSingle().Which.TextContent.Should().Be("You cannot open this page"));
        cut.FindComponents<MainLayout>().Should().ContainSingle("the one shell, once");
        cut.FindComponents<NavMenu>().Should().ContainSingle("one sidebar, so one set of landmarks and one Sign out");
        Text(cut.Find(".system-panel p")).Should().Be("Your role (Assessor) does not open this page.");
    }

    // ---- Page not found ----

    [Fact]
    public void NotFound_SignedIn_SaysSo_AndOffersHome_EchoingNoAddress()
    {
        var cut = RenderSignedIn<NotFound>([WombatRoles.Assessor], "/activities/no-such-page?said=%3Cb%3Ehello%3C%2Fb%3E");

        cut.Find("h1").TextContent.Should().Be("Page not found");
        TabTitle.Of(this, cut).Should().Be("Page not found · Wombat");
        cut.FindAll(".system-panel p").Select(Text).Should().Equal(
            "There is no page at this address.", "Check the address, or start again from Home.");
        Links(cut).Should().Equal([("Go to Home", "/")]);
        cut.Markup.Should().NotContainAny(["no-such-page", "hello", "institution"],
            "no echoed address (a crafted link's words) and no line about another institution's records (S4)");
    }

    [Fact]
    public void NotFound_SignedOut_SaysOnlyThatThereIsNoPage_AndOffersHome()
    {
        var cut = RenderSignedOut<NotFound>("/not-found");

        cut.Find("h1").TextContent.Should().Be("Page not found");
        cut.FindAll(".system-card p").Select(Text).Should().Equal("There is no page at this address.");
        Links(cut).Should().Equal([("Go to Home", "/")]);
        cut.Markup.Should().NotContain("sign in first", "the only way here signed out is typing /not-found (S3)");
    }

    // ---- The error page ----

    [Fact]
    public void TheErrorPage_AfterAFailure_SaysSo_WithTheReferenceAndTheTime_AndOffersTheFailedAddressAgain()
    {
        using var activity = new Activity("request").SetIdFormat(ActivityIdFormat.W3C).Start();
        var context = Failed("/activities/21", "?tab=history", activity, SignedIn([WombatRoles.Trainee]));

        var cut = RenderError(context);

        cut.Find("h1").TextContent.Should().Be("Something went wrong");
        TabTitle.Of(this, cut).Should().Be("Something went wrong · Wombat");
        Text(cut.Find(".system-panel p")).Should().Be(
            "Wombat could not finish this request. Try again. If it keeps happening, send this reference to your " +
            "institution's Wombat administrator.");
        var reference = cut.Find(".reference-block");
        reference.QuerySelectorAll("dt").Select(Text).Should().Equal("Reference", "Time");
        var id = reference.QuerySelector("dd code")!.TextContent;
        id.Should().Be(activity.TraceId.ToHexString()).And.HaveLength(32, "the trace id, not the 55-character traceparent (S6)");
        Text(reference.QuerySelectorAll("dd").ElementAt(1)).Should().Be("2026-09-26 15:14 SAST");

        var tryAgain = cut.Find("a[href='/activities/21?tab=history']");
        Text(tryAgain).Should().Be("Try again");
        tryAgain.GetAttribute("data-enhance-nav").Should().Be("false", "the failed request is made again, as it was");
        tryAgain.ClassList.Should().Contain("btn-primary");
        cut.Find("a[href='/']").ClassList.Should().Contain("btn-outline");
    }

    // S6: signed out it may be an MSF respondent, with no account and no institution here.
    [Fact]
    public void TheErrorPage_SignedOut_SendsTheReferenceToWhoeverSentTheLink()
    {
        var cut = RenderError(Failed("/msf/respond", "?token=abc", activity: null, new ClaimsPrincipal(new ClaimsIdentity())));

        Text(cut.Find(".system-card p")).Should().Be(
            "Wombat could not finish this request. Try again. If it keeps happening, send this reference to whoever sent " +
            "you the link, or to your Wombat administrator.");
        cut.Find(".reference-block dd code").TextContent.Should().Be("0HN-TEST:00000001", "with no trace, the request's own id");
        Links(cut).Should().Equal([("Try again", "/msf/respond?token=abc"), ("Go to Home", "/")]);
        cut.FindAll(".actions-cell .btn svg").Should().BeEmpty(
            "signed out, the buttons carry no icons, as the signed-out Page not found and Access denied (R2-Error-Out)");
    }

    [Fact]
    public void TheErrorPage_SignedIn_ItsButtonsCarryTheirIcons()
    {
        var cut = RenderError(Failed("/activities/21", string.Empty, activity: null, SignedIn([WombatRoles.Trainee])));

        cut.FindAll(".actions-cell .btn").Select(button => button.QuerySelector("svg use")?.GetAttribute("href"))
            .Should().Equal("/icons/refresh-cw.svg#i", "/icons/home.svg#i");
    }

    [Fact]
    public void TheErrorPage_OffersNoTryAgain_ForAnAddressThatIsNotOnThisSite()
    {
        var cut = RenderError(Failed("//evil.example", string.Empty, activity: null, SignedIn([WombatRoles.Trainee])));

        Links(cut).Should().Equal([("Go to Home", "/")]);
        cut.Find("a[href='/']").ClassList.Should().Contain("btn-primary");
    }

    // A.5.8 (R2-Error-Typed): /Error typed as an address. No request failed, so there is no reference to give.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TheErrorPage_Typed_SaysNothingWentWrong_WithNoReferenceAndNoTryAgain(bool signedIn)
    {
        var context = new DefaultHttpContext
        {
            User = signedIn ? SignedIn([WombatRoles.Trainee]) : new ClaimsPrincipal(new ClaimsIdentity())
        };

        var cut = RenderError(context);

        cut.Find("h1").TextContent.Should().Be("Nothing went wrong");
        TabTitle.Of(this, cut).Should().Be("Nothing went wrong · Wombat");
        Text(cut.Find("p")).Should().Be("This is Wombat's error page, opened directly. No request failed, so there is nothing to report.");
        cut.FindAll(".reference-block").Should().BeEmpty();
        Links(cut).Should().Equal([("Go to Home", "/")]);
        cut.FindAll(".actions-cell .btn svg").Should().HaveCount(signedIn ? 1 : 0, "signed out, no icon (R2-Error-Out)");
    }

    [Fact]
    public void TheErrorPage_IsStaticAndOpenToEveryone()
    {
        // Static, it renders once inside the failed request, so its reference is that request's (T321); anonymous, a visitor
        // who has not signed in sees it rather than a sign-in form.
        typeof(ErrorPage).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.ExcludeFromInteractiveRoutingAttribute), inherit: true)
            .Should().ContainSingle();
        typeof(ErrorPage).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), inherit: true)
            .Should().ContainSingle();
    }

    // ---- rendering ----

    private IRenderedComponent<TPage> RenderSignedIn<TPage>(string[] roles, string address)
        where TPage : IComponent
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("someone@kgk.test");
        auth.SetRoles(roles);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "someone"));
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<TPage>();
    }

    private IRenderedComponent<TPage> RenderSignedOut<TPage>(string address)
        where TPage : IComponent
    {
        this.AddTestAuthorization();
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<TPage>();
    }

    private IRenderedComponent<ErrorPage> RenderError(HttpContext context)
    {
        Services.AddSingleton<TimeProvider>(new FixedClock(AFailure));
        return RenderComponent<ErrorPage>(parameters => parameters.AddCascadingValue<HttpContext>(context));
    }

    /// <summary>The request as the exception handler reruns it: its failed path and query, and its trace.</summary>
    private static DefaultHttpContext Failed(string path, string query, Activity? activity, ClaimsPrincipal user)
    {
        var context = new DefaultHttpContext { User = user, TraceIdentifier = "0HN-TEST:00000001" };
        context.Request.Path = ErrorPages.Path;
        context.Request.QueryString = new QueryString(query.Length == 0 ? null : query);
        var failure = new ExceptionHandlerFeature { Error = new InvalidOperationException("boom"), Path = path };
        context.Features.Set<IExceptionHandlerFeature>(failure);
        context.Features.Set<IExceptionHandlerPathFeature>(failure);
        if (activity is not null)
        {
            context.Features.Set<IHttpActivityFeature>(new ActivityFeature(activity));
        }

        return context;
    }

    private static ClaimsPrincipal SignedIn(string[] roles)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "someone"), .. roles.Select(role => new Claim(ClaimTypes.Role, role))],
            "Test"));

    private static List<(string Text, string? Href)> Links(IRenderedFragment cut)
        => cut.FindAll("a").Select(link => (Text(link), link.GetAttribute("href"))).ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class ActivityFeature(Activity activity) : IHttpActivityFeature
    {
        public Activity Activity { get; set; } = activity;
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
