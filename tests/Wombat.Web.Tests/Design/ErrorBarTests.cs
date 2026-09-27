using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Layout;
using Wombat.Web.Navigation;
using Wombat.Web.Tests.Hosting;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T335 flow 01 (review S19, D7): the error bar blazor.web.js shows when a page's circuit fails says what to do, lays its
/// row out where the runtime's inline style cannot undo it, and keeps the classes the runtime wires its buttons by
/// (DESIGN.md § The reconnect dialog and the error bar).
/// </summary>
/// <remarks>
/// Until T335 it was the template's, copied from ClinicAssist: "An unhandled error has occurred." with a Reload link and a
/// 🗙, on <c>lightyellow</c>, with no layout for a phone. The runtime shows it by setting an inline <c>display: block</c> on
/// <c>#blazor-error-ui</c>, which beats any stylesheet's flex row there, so the row is a wrapper inside it. The markup is
/// <c>MainLayout</c>'s as it renders, signed in and signed out: the bar sits outside the layout's <c>AuthorizeView</c>, so
/// every page has it. (Until the review of the t335 branch it was read from the source, on a claim that the layout could
/// not be rendered here.)
/// </remarks>
public sealed partial class ErrorBarTests : TestContext
{
    private const string Sentence = "This page no longer responds; copy anything you need, then reload.";

    public ErrorBarTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TheBar_IsOnEveryPage_OutsideTheSignedInAndSignedOutFrames(bool signedIn)
    {
        var bar = ErrorBar(signedIn);

        bar.Closest(".page").Should().BeNull("the bar is outside the AuthorizeView that draws either frame");
        bar.PreviousElementSibling!.ClassList.Should().Contain("page", "it is the layout's own, beside the frame, at its foot");
    }

    [Fact]
    public void TheBar_HoldsTheSentence_ThenReloadAndDismiss_OnAWrapperTheRuntimesStyleCannotReach()
    {
        var bar = ErrorBar(signedIn: true);

        bar.HasAttribute("data-nosnippet").Should().BeTrue();
        bar.GetAttribute("role").Should().Be("alert", "it is announced as the runtime shows it");

        var row = bar.Children.Should().ContainSingle("the runtime's display: block lands on the outer element alone").Which;
        row.LocalName.Should().Be("div");
        row.ClassList.Should().Equal("error-bar-row");

        row.Children.Select(child => child.LocalName).Should().Equal(["svg", "p", "button", "button"],
            "the icon, the sentence, then Reload and Dismiss, in the order a phone stacks them");
        var icon = row.Children[0];
        IconName(icon).Should().Be("triangle-alert");
        icon.ClassList.Should().Contain("error-bar-icon");
        Text(row.Children[1]).Should().Be(Sentence);

        // R2-ErrorBar: small buttons, each with its icon (refresh-cw, x), which the phone's rule hides.
        var reload = row.Children[2];
        Text(reload).Should().Be("Reload");
        reload.GetAttribute("type").Should().Be("button");
        reload.ClassList.Should().Equal(["btn", "btn-sm", "btn-primary", "reload"], "blazor.web.js wires .reload to reload the page");
        IconName(reload.QuerySelector("svg.error-bar-button-icon")!).Should().Be("refresh-cw");

        var dismiss = row.Children[3];
        Text(dismiss).Should().Be("Dismiss");
        dismiss.GetAttribute("type").Should().Be("button");
        dismiss.ClassList.Should().Equal(["btn", "btn-sm", "btn-outline", "dismiss"], "and .dismiss to hide the bar (D7: it is kept)");
        IconName(dismiss.QuerySelector("svg.error-bar-button-icon")!).Should().Be("x");

        bar.QuerySelectorAll("a").Should().BeEmpty("Reload is a button: a link to \".\" left the page when the runtime had not wired it");
        Text(bar).Should().NotContainAny(["unhandled", "error has occurred", "Reference", "Request ID", "🗙"],
            "the words are the design's, with no reference (D7)");
    }

    [Fact]
    public void TheBar_IsHiddenUntilTheRuntimeShowsIt_OnTheWarningTokens_WithTheRowInside()
    {
        var rules = LayoutCss();

        var bar = TopLevel(rules, "#blazor-error-ui");
        bar.Value("display").Should().Be("none", "the runtime shows it");
        bar.Value("position").Should().Be("fixed");
        bar.Value("bottom").Should().Be("0");
        bar.Value("background-color").Should().Be("var(--warning-bg)");
        bar.Value("border-top").Should().Be("3px solid var(--warning-color)");
        bar.Value("box-shadow").Should().Be("var(--shadow-bar)");
        bar.Value("color").Should().Be("var(--text-color)");

        TopLevel(rules, ".error-bar-row").Value("display").Should().Be("flex", "the row is on the wrapper, which no inline style reaches");
        TopLevel(rules, ".error-bar-row ::deep .error-bar-icon").Value("color").Should().Be("var(--warning-color)");

        var css = Stylesheet.WithoutComments(File.ReadAllText(CssPath()));
        var barRules = css[css.IndexOf("#blazor-error-ui", StringComparison.Ordinal)..];
        RawColour().Matches(barRules).Select(match => match.Value).Should().BeEmpty("a colour is a token from :root");
    }

    [Fact]
    public void AtPhoneWidth_TheSentenceComesFirst_ThenReloadAndDismissSideBySide_At44Px()
    {
        const string narrow = "@media (max-width: 640.98px)";
        var rules = LayoutCss();

        var row = rules.Should().ContainSingle(rule => rule.AtRule == narrow && rule.Selector == ".error-bar-row").Which;
        row.Value("flex-wrap").Should().Be("wrap");
        rules.Should().ContainSingle(rule => rule.AtRule == narrow && rule.Selector == ".error-bar-text")
            .Which.Value("flex-basis").Should().Be("calc(100% - 20px - var(--space-sm))",
                "the sentence fills the first line beside its 20px icon, so the buttons wrap below it");

        var buttons = rules.Should().ContainSingle(rule => rule.AtRule == narrow && rule.Selector == ".error-bar-row .btn").Which;
        buttons.Value("flex").Should().Be("1 1 0", "side by side, each half the row");
        buttons.Value("min-height").Should().Be("44px");
        buttons.Value("font-size").Should().Be("0.95rem", "a full button's words at 44px, not .btn-sm's (R2-ErrorBar-Narrow)");
        rules.Should().ContainSingle(rule => rule.AtRule == narrow && rule.Selector == ".error-bar-row ::deep .error-bar-button-icon")
            .Which.Value("display").Should().Be("none", "the phone's buttons carry no icons (R2-ErrorBar-Narrow)");
        rules.Where(rule => rule.AtRule.Length == 0 && rule.Selectors.Any(selector => selector.Contains("error-bar-button-icon", StringComparison.Ordinal)))
            .Should().BeEmpty("from 641px the icons show");
    }

    [Fact]
    public void NoRuleStylesReloadOrDismiss_SoTheyAreTheRuntimesClassesAlone()
    {
        // DESIGN.md § Non-negotiables lets .reload and .dismiss go undefined because blazor.web.js reads them and nothing
        // styles them (DefinedClassTests' framework allowance). A rule that styled them would make them styling classes.
        var css = Stylesheet.WithoutComments(File.ReadAllText(CssPath()));
        var app = Stylesheet.WithoutComments(File.ReadAllText(Stylesheet.WebFile("wwwroot", "app.css")));

        Regex.IsMatch(css + app, @"\.(reload|dismiss)(?![\w-])").Should().BeFalse();
    }

    [Fact]
    public async Task TheRuntimeTheAppServes_ShowsTheBarByItsId_AndWiresReloadAndDismissByTheirClasses()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);
        var src = document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")!)
            .Should().ContainSingle(url => url.Contains("blazor.web", StringComparison.Ordinal)).Which;
        using var response = await host.Client.GetAsync(src);
        var runtime = await response.Content.ReadAsStringAsync();

        // A property assignment (onclick = …) from the runtime's own script, so the CSP's script-src has nothing to refuse,
        // and it works on a <button> as on the template's <a>.
        runtime.Should().ContainAll(["\"#blazor-error-ui\"", "\"#blazor-error-ui .reload\"", "\"#blazor-error-ui .dismiss\""]);
        Regex.IsMatch(runtime, @"\.style\.display\s*=\s*""block""").Should().BeTrue("the runtime shows the bar with an inline display: block");
    }

    // ---- helpers ----

    /// <summary><c>#blazor-error-ui</c> as MainLayout renders it, for someone signed in or a visitor who has not.</summary>
    private IElement ErrorBar(bool signedIn)
    {
        var auth = this.AddTestAuthorization();
        if (signedIn)
        {
            auth.SetAuthorized("n.mahlangu@kgk.wombat.local");
            auth.SetRoles(WombatRoles.Trainee);
            auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mahlangu"));
        }

        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/activities/mine");
        var cut = RenderComponent<MainLayout>(parameters => parameters
            .Add(layout => layout.Body, (RenderFragment)(builder => builder.AddMarkupContent(0, "<h1>The page</h1>")))
            .AddCascadingValue(signedIn ? ActingRoleResolver.Resolve(null, [WombatRoles.Trainee]) : ActingRole.None));

        cut.FindAll(signedIn ? ".page:not(.page--signed-out)" : ".page--signed-out").Should().ContainSingle("guard: the frame drawn");
        return cut.FindAll("#blazor-error-ui").Should().ContainSingle().Which;
    }

    /// <summary>The Lucide glyph an <c>Icon</c> draws: its sprite file's name.</summary>
    private static string IconName(IElement svg)
        => Regex.Match(svg.QuerySelector("use")!.GetAttribute("href")!, @"^/icons/(?<name>[\w-]+)\.svg#i$").Groups["name"].Value;

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string CssPath() => Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css");

    private static IReadOnlyList<CssRule> LayoutCss() => Stylesheet.Parse(File.ReadAllText(CssPath()));

    private static CssRule TopLevel(IReadOnlyList<CssRule> rules, string selector)
        => rules.Should().ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selector == selector,
            $"MainLayout.razor.css styles {selector} once at the top level").Which;

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|rgba?\(|hsla?\(|\blightyellow\b")]
    private static partial Regex RawColour();
}
