using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Layout;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Navigation;
using Wombat.Web.Tests.Design;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// The shell (T335, flow 01; DESIGN.md § Layout grid; R2-Shell-*, R2-Phone-*, R2-Rules § 3): the brand cell, the acting
/// role, one account row holding the name and the one Sign out form, the CSS-only phone toggle, and a signed-out shell
/// with no navigation. It reads no database: the name is a claim.
/// </summary>
public sealed partial class MainLayoutTests : TestContext
{
    private const string Page = "<h1>The page</h1>";

    public MainLayoutTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- the account row ----

    [Fact]
    public void SignedIn_TheShellHasOneSignOutForm_InTheAccountRow()
    {
        var cut = RenderSignedIn([WombatRoles.Assessor], new Claim(WombatClaimTypes.DisplayName, "Mohammed Patel"));

        var form = cut.FindAll("form").Should().ContainSingle("one Sign out, placed by CSS at both widths (D9)").Which;
        form.GetAttribute("action").Should().Be("/account/logout/submit",
            "the page is /account/logout; its post moved beside it, as every account form's (T339, flow 02, B1)");
        form.GetAttribute("method").Should().Be("post");
        form.ParentElement!.ClassList.Should().Contain("account-row");
        form.QuerySelector("button[type=submit]")!.TextContent.Trim().Should().Be("Sign out");
        cut.FindAll("button").Where(button => button.TextContent.Contains("Sign out", StringComparison.Ordinal)).Should().ContainSingle();
    }

    // The review of the t335 branch: the error page is drawn in the failed request's rerun, whose form token would be the
    // failed pass's, its cookie cleared with that pass's response. Sign out there is a link to the sign-out page.
    [Fact]
    public void OnTheErrorPage_SignOutIsALinkToTheSignOutPage_AndThereIsNoForm()
    {
        var cut = RenderSignedIn([WombatRoles.Assessor], route: typeof(Wombat.Web.Components.Pages.Error));

        cut.FindAll("form").Should().BeEmpty("no form carries a token the rerun cannot stand behind");
        var signOut = cut.FindAll(".account-row a").Should().ContainSingle(link => link.ClassList.Contains("sign-out-button")).Which;
        signOut.GetAttribute("href").Should().Be("/account/logout-confirm", "the sign-out page draws its own form and token");
        signOut.TextContent.Trim().Should().Be("Sign out");
        signOut.ClassList.Should().Contain(["btn", "btn-outline", "btn-sm"], "it looks as the button does everywhere else");
    }

    [Fact]
    public void TheName_IsTheDisplayNameClaim_WholeInTheLink_AndItsTitle()
    {
        var cut = RenderSignedIn([WombatRoles.Assessor], new Claim(WombatClaimTypes.DisplayName, "Mohammed Patel"));

        var link = cut.Find(".account-row a.account-link");
        link.GetAttribute("href").Should().Be("/account/profile");
        link.TextContent.Trim().Should().Be("Mohammed Patel", "its whole text is its accessible name");
        link.GetAttribute("title").Should().Be("Mohammed Patel");
        link.HasAttribute("aria-label").Should().BeFalse("the text already names it");
    }

    [Fact]
    public void WithNoNameClaim_TheNameIsTheSignInName_TheEmail_WholeInTheDom()
    {
        // The bootstrap administrator has no stored name (S7): the email, cut by the bar's CSS at 28 characters.
        const string email = "s.mokoena.registrar@kgk.wombat.local";
        var cut = RenderSignedIn([WombatRoles.Administrator], name: email);

        var link = cut.Find("a.account-link");
        link.TextContent.Trim().Should().Be(email);
        link.GetAttribute("title").Should().Be(email);
    }

    [Fact]
    public void TheBar_CutsTheNameAt28Characters_WithAnEllipsis_AndThePhoneMenuWrapsItWhole()
    {
        var css = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css")));

        var bar = css.Should().ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selector == ".account-name").Which;
        bar.Value("max-width").Should().Be("28ch");
        bar.Value("text-overflow").Should().Be("ellipsis");
        bar.Value("white-space").Should().Be("nowrap");

        var phone = css.Should().ContainSingle(rule => rule.AtRule == "@media (max-width: 640.98px)" && rule.Selector == ".account-name").Which;
        phone.Value("max-width").Should().Be("none");
        phone.Value("white-space").Should().Be("normal");
        phone.Value("overflow-wrap").Should().Be("anywhere");
    }

    [Fact]
    public void OnMyAccount_TheNameIsTheCurrentPage_AndNothingElse()
    {
        var cut = RenderSignedIn([WombatRoles.Trainee], route: typeof(Profile));

        cut.Find("a.account-link").GetAttribute("aria-current").Should().Be("page");
        cut.FindAll("[aria-current]").Should().ContainSingle("no nav item is lit on My account");
    }

    [Fact]
    public void ElsewhereTheName_IsNotCurrent()
        => RenderSignedIn([WombatRoles.Trainee], route: typeof(ChangePassword)).Find("a.account-link").HasAttribute("aria-current")
            .Should().BeFalse();

    [Fact]
    public void TheAccountLink_IsATarget_OfAtLeast32Px()
    {
        // T328: it was 21px tall.
        var css = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css")));

        css.Should().ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selector == ".account-link")
            .Which.Value("min-height").Should().Be("32px");
        css.Should().ContainSingle(rule => rule.AtRule == "@media (max-width: 640.98px)" && rule.Selector == ".account-link")
            .Which.Value("min-height").Should().Be("44px");
    }

    // ---- the brand and the role ----

    [Fact]
    public void TheBrand_IsTheMark_AndTheWordmark_LinkingHome()
    {
        var brand = RenderSignedIn([WombatRoles.Assessor]).Find(".sidebar > a.brand");

        brand.GetAttribute("href").Should().Be("/");
        var mark = brand.QuerySelector("img")!;
        mark.GetAttribute("src").Should().Be("/brand/wombat-mark.svg");
        mark.GetAttribute("alt").Should().BeEmpty("the wordmark names the link");
        mark.GetAttribute("width").Should().Be("32");
        brand.TextContent.Trim().Should().Be("Wombat");
    }

    [Fact]
    public void ThePhoneBar_NamesTheActingRole_AndNoRoleNamesNone()
    {
        RenderSignedIn([WombatRoles.SubSpecialityAdmin]).Find(".phone-role").TextContent.Trim()
            .Should().Be("Acting as Sub-speciality admin");

        using var noRole = new TestContext();
        noRole.JSInterop.Mode = JSRuntimeMode.Loose;
        RenderSignedIn(noRole, [], []).FindAll(".phone-role").Should().BeEmpty();
    }

    // ---- the phone toggle (S11) ----

    [Fact]
    public void ThePhoneToggle_IsAHiddenCheckbox_FirstInThePage_ItsLabelInTheHeader_WithNoAriaExpanded()
    {
        var cut = RenderSignedIn([WombatRoles.Trainee]);

        var page = cut.Find(".page");
        var toggle = page.FirstElementChild!;
        toggle.LocalName.Should().Be("input");
        toggle.GetAttribute("type").Should().Be("checkbox");
        toggle.Id.Should().Be("nav-toggle");
        toggle.ClassList.Should().Contain("nav-toggle");
        toggle.NextElementSibling!.LocalName.Should().Be("header", "every part of the shell is the checkbox's later sibling, or in one");

        var label = cut.FindAll("label[for='nav-toggle']").Should().ContainSingle().Which;
        label.ParentElement!.LocalName.Should().Be("header", "the toggle is the phone bar's, in the banner");
        label.QuerySelector(".nav-toggle-closed")!.TextContent.Trim().Should().Be("Menu");
        label.QuerySelector(".nav-toggle-open")!.TextContent.Trim().Should().Be("Close");
        cut.FindAll("[aria-expanded]").Should().BeEmpty("the checkbox's own state is what a screen reader hears");
    }

    // The review of the t335 branch. Keyed by the path, the checkbox survived a navigation to the page shown (the lit item
    // tapped, the brand tapped on Home), so the menu stayed open over it. Keyed by a count of the circuit's navigations,
    // every navigation replaces it, unchecked. bUnit's markup cannot show that an element was replaced, so the render
    // tree's own key is read: a new key is a new element to the diff.
    [Fact]
    public void EveryNavigation_ReplacesThePhoneToggle_ToThePageShownToo_SoTheMenuCloses()
    {
        var cut = RenderSignedIn([WombatRoles.Trainee], route: typeof(ActivityInbox));
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        var first = ToggleKey(cut);

        navigation.NavigateTo("/activities/inbox");
        cut.WaitForAssertion(() => ToggleKey(cut).Should().NotBe(first, "the same page again: the lit item tapped"));
        var second = ToggleKey(cut);

        navigation.NavigateTo("/");
        cut.WaitForAssertion(() => ToggleKey(cut).Should().NotBe(second, "another page"));
        var third = ToggleKey(cut);

        navigation.NavigateTo("/");
        cut.WaitForAssertion(() => ToggleKey(cut).Should().NotBe(third, "the brand tapped on Home"));
        cut.FindAll("#nav-toggle").Should().ContainSingle();
    }

    [Fact]
    public void TheToggle_TakesTheNavRing_OnItsLabel_AndIsNoTabStopFrom641Px()
    {
        var css = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css")));

        css.Should().ContainSingle(rule => rule.Selector == ".nav-toggle:focus-visible ~ .sidebar .nav-toggle-label")
            .Which.Value("outline").Should().Be("2px solid var(--nav-focus-ring)");
        css.Should().Contain(rule => rule.AtRule == "@media (min-width: 641px)" && rule.Selectors.Contains(".nav-toggle")
            && rule.Value("display") == "none");
        css.Should().ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selector == ".nav-toggle")
            .Which.Value("position").Should().Be("absolute", "visually hidden, still focusable, below 641px");
    }

    [Fact]
    public void EveryControlOnDarkChrome_TakesTheNavRing()
    {
        // S13: the page's ring is 2.90:1 on the gradient's start.
        var layout = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css")));
        var nav = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "NavMenu.razor.css")));
        var item = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "NavItemLink.razor.css")));

        string[] onLayout = [".brand:focus-visible", ".sign-in:focus-visible", ".account-link:focus-visible", ".sign-out-button:focus-visible"];
        foreach (var selector in onLayout)
        {
            layout.Should().Contain(rule => rule.Selectors.Contains(selector) && rule.Value("outline") == "2px solid var(--nav-focus-ring)", selector);
        }

        nav.Should().Contain(rule => rule.Selectors.Contains(".role-switch:focus-visible") && rule.Selectors.Contains(".role-option:focus-visible")
            && rule.Value("outline") == "2px solid var(--nav-focus-ring)");
        item.Should().Contain(rule => rule.Selector == ".nav-link:focus-visible" && rule.Value("outline") == "2px solid var(--nav-focus-ring)");
    }

    // ---- signed out ----

    [Fact]
    public void SignedOut_TheShellIsTheGradientBar_WithTheBrandAndSignIn_AndNoNavigation()
    {
        this.AddTestAuthorization();
        var cut = RenderLayout(this, ActingRole.None, route: null);

        cut.FindAll("nav, .sidebar, .account-row, #nav-toggle, form").Should().BeEmpty("a visitor has no navigation and nothing to sign out of");
        var bar = cut.Find(".signed-out-bar");
        bar.QuerySelector("a.brand")!.GetAttribute("href").Should().Be("/");
        var signIn = bar.QuerySelector("a.sign-in")!;
        signIn.GetAttribute("href").Should().Be("/account/login");
        signIn.TextContent.Trim().Should().Be("Sign in");
        cut.Find("article").InnerHtml.Should().Contain("The page");
    }

    // ---- the shell reads no data (S7) ----

    // The review of the t335 branch: the scan read "@inject" lines alone, in three files. An [Inject] property in @code,
    // or a component the shell renders, slipped past it. Every component of the rendered shell, signed in with a switch to
    // offer and a lit item, is read for what it has injected: "@inject" and [Inject] both compile to an [Inject] property.
    [Fact]
    public void TheShell_AndEveryComponentItRenders_InjectNothingThatReadsData()
    {
        var cut = RenderSignedIn([WombatRoles.Assessor, WombatRoles.CommitteeMember, WombatRoles.Trainee],
            [new Claim(WombatClaimTypes.DisplayName, "Thandi Zulu")], route: typeof(ActivityInbox));

        var shell = cut.FindComponents<IComponent>().Select(component => component.Instance.GetType())
            .Prepend(cut.Instance.GetType())
            .Where(type => type.Assembly == typeof(MainLayout).Assembly)
            .Distinct()
            .ToList();
        shell.Should().Contain([typeof(MainLayout), typeof(NavMenu), typeof(NavItemLink), typeof(Wombat.Web.Components.Shared.Icon)],
            "guard: the render reaches the shell's own components");

        var injected = shell
            .SelectMany(type => InjectedInto(type).Select(service => $"{type.Name}: {service.Name}"))
            .ToList();

        injected.Select(entry => entry[(entry.IndexOf(": ", StringComparison.Ordinal) + 2)..]).Distinct()
            .Should().BeSubsetOf(["NavigationManager", "IJSRuntime"],
                $"the name, the roles and the acting role are claims and cascades; an outage must not take the error page's shell ({string.Join(", ", injected)})");
    }

    [Fact]
    public void TheInjectionScan_SeesAnInjectPropertyInCode()
        => InjectedInto(typeof(InjectsInCode)).Should().ContainSingle().Which.Should().Be(typeof(IServiceProvider),
            "guard: a component's [Inject] property in @code is what the shell's scan reads");

    // ---- the banner (the review of the t335 branch) ----

    [Fact]
    public void TheBrandTheNavigationAndTheAccountRow_AreInOneBanner_ThePageInMain()
    {
        var cut = RenderSignedIn([WombatRoles.Assessor], route: typeof(ActivityInbox));

        var banner = cut.FindAll("header").Should().ContainSingle("one banner: a <header> outside main is the page's banner").Which;
        banner.Closest("main, article, aside, nav, section").Should().BeNull("a header inside one of these is no banner");
        banner.QuerySelector(":scope > a.brand").Should().NotBeNull("the brand");
        banner.QuerySelectorAll("nav[aria-label='Main']").Should().ContainSingle("the navigation");
        banner.QuerySelector(".account-row a.account-link").Should().NotBeNull("the account row, outside every landmark until the review");
        banner.QuerySelector(".account-row .sign-out-button").Should().NotBeNull();
        cut.Find("main").Closest("header").Should().BeNull("the page is its own landmark, beside the banner");
    }

    // ---- a Windows contrast theme (the review of the t335 branch) ----

    [Fact]
    public void InForcedColours_TheLitItemsBarIsABorder_AndMyAccountsUnderlineATextUnderline()
    {
        // A contrast theme paints no box-shadow, and both cues were box-shadows.
        const string forced = "@media (forced-colors: active)";
        var item = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "NavItemLink.razor.css")));
        var layout = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css")));

        item.Should().ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selector == ".nav-link.active")
            .Which.Value("box-shadow").Should().Be("inset 3px 0 0 var(--nav-text-strong)", "guard: the bar is a shadow");
        var bar = item.Should().ContainSingle(rule => rule.AtRule == forced && rule.Selector == ".nav-link.active").Which;
        bar.Value("border-left").Should().Be("3px solid CanvasText");
        bar.Value("padding-left").Should().Be("calc(0.625rem - 3px)", "the border takes its width from the padding, so the label stays");
        item.Should().ContainSingle(rule => rule.AtRule == "@media (forced-colors: active) and (max-width: 640.98px)"
                && rule.Selector == ".nav-link.active")
            .Which.Value("padding-left").Should().Be("calc(0.75rem - 3px)", "the phone row's padding, less the border");

        var underline = layout.Should().ContainSingle(rule => rule.AtRule == forced && rule.Selector == ".account-link[aria-current=\"page\"]").Which;
        underline.Value("text-decoration").Should().Be("underline 3px CanvasText");
        underline.Value("box-shadow").Should().Be("none");
        layout.Last(rule => rule.Selector == ".account-link[aria-current=\"page\"]").AtRule.Should().Be(forced,
            "after the phone's own underline, so it wins there too");
    }

    [Fact]
    public void ThePage_IsInTheArticle_UnderTheMainLandmark()
    {
        var cut = RenderSignedIn([WombatRoles.Assessor], route: typeof(ActivityInbox));

        cut.Find("main > article h1").TextContent.Should().Be("The page");
        cut.FindAll("nav[aria-label='Main']").Should().ContainSingle();
    }

    // ---- helpers ----

    /// <summary>The service types a component has injected, "@inject" and [Inject] alike, its base types' included.</summary>
    private static IEnumerable<Type> InjectedInto(Type component)
        => component.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Concat(component.BaseType is { } parent && parent != typeof(object)
                ? parent.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                : [])
            .Where(property => property.GetCustomAttribute<InjectAttribute>() is not null)
            .Select(property => property.PropertyType)
            .Distinct();

    /// <summary>A component that injects in its code, not by "@inject": what the scan must see.</summary>
    private sealed class InjectsInCode : ComponentBase
    {
        [Inject] private IServiceProvider Services { get; set; } = default!;
    }

    /// <summary>The <c>@key</c> the phone toggle's checkbox has in the render tree, where the diff reads it.</summary>
    /// <remarks>
    /// The render tree's types are the framework's own (BL0006), read here on purpose: what a key does is decided by the
    /// diff, from the tree, and bUnit's markup is parsed afresh at each render, so no element in it is ever the same one.
    /// </remarks>
#pragma warning disable BL0006
    private object? ToggleKey(IRenderedComponent<MainLayout> cut)
    {
        var holder = cut.FindComponent<CascadingValue<NavMenuModel>>().ComponentId;
        var frames = (ArrayRange<RenderTreeFrame>)typeof(Renderer)
            .GetMethod("GetCurrentRenderTreeFrames", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(Renderer, [holder])!;

        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType == RenderTreeFrameType.Element && frame.ElementName == "input"
                && Enumerable.Range(index + 1, frame.ElementSubtreeLength - 1)
                    .Select(attribute => frames.Array[attribute])
                    .Any(attribute => attribute.FrameType == RenderTreeFrameType.Attribute
                        && attribute.AttributeName == "id" && Equals(attribute.AttributeValue, "nav-toggle")))
            {
                return frame.ElementKey;
            }
        }

        throw new InvalidOperationException("The phone toggle's checkbox is not in the render tree.");
    }
#pragma warning restore BL0006

    private IRenderedComponent<MainLayout> RenderSignedIn(string[] roles, params Claim[] claims)
        => RenderSignedIn(this, roles, claims);

    private IRenderedComponent<MainLayout> RenderSignedIn(string[] roles, string name)
        => RenderSignedIn(this, roles, [], name);

    private IRenderedComponent<MainLayout> RenderSignedIn(string[] roles, Type route)
        => RenderSignedIn(this, roles, [], route: route);

    private IRenderedComponent<MainLayout> RenderSignedIn(string[] roles, Claim[] claims, Type route)
        => RenderSignedIn(this, roles, claims, route: route);

    private static IRenderedComponent<MainLayout> RenderSignedIn(
        TestContext context, string[] roles, Claim[] claims, string name = "user@kgk.wombat.local", Type? route = null)
    {
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized(name);
        auth.SetRoles(roles);
        auth.SetClaims(claims);

        return RenderLayout(context, ActingRoleResolver.Resolve(null, roles), route);
    }

    private static IRenderedComponent<MainLayout> RenderLayout(TestContext context, ActingRole acting, Type? route)
    {
        context.Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/activities/inbox");
        return context.RenderComponent<MainLayout>(parameters =>
        {
            parameters
                .Add(layout => layout.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, Page)))
                .AddCascadingValue(acting);
            if (route is not null)
            {
                parameters.AddCascadingValue(new RouteData(route, new Dictionary<string, object?>()));
            }
        });
    }
}
