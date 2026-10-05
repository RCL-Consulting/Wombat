using System.Security.Claims;
using Bunit;
using FluentAssertions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Layout;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// The role head (T335, flow 01; R2-Rules § 1 and § 4; R2-Phone-Bars; D8): "Acting as" and the role's label; one "Switch to
/// …" link beside a second role held; a native "Change role" disclosure beside two or more; no head without a role. The
/// menu under it is the acting role's, so it changes with the switch.
/// </summary>
public sealed class RoleSwitchTests : TestContext
{
    public RoleSwitchTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheMenu_ChangesWithTheActingRole()
    {
        // Dr Zulu: Committee member and Assessor (R2-Phone-Zulu-Open, R2-Detail-Inbox).
        string[] held = [WombatRoles.CommitteeMember, WombatRoles.Assessor];

        Labels(Render(held, WombatRoles.CommitteeMember)).Should().Equal(
            "Home", "Programme trainees", "Committee reviews", "Decision panels", "My data rights");
        Labels(Render(held, WombatRoles.Assessor)).Should().Equal("Home", "Activity inbox", "My data rights");
    }

    [Fact]
    public void TwoRoles_OfferOneSwitchLink_ToTheOtherRolesHome_AsAFullPageLoad()
    {
        var cut = Render([WombatRoles.CommitteeMember, WombatRoles.Assessor], WombatRoles.CommitteeMember);

        var head = cut.Find(".role-head");
        head.QuerySelector(".role-now")!.TextContent.Trim().Should().Be("Acting as Committee member");
        head.QuerySelector("details").Should().BeNull("two roles need no disclosure");

        var link = head.QuerySelectorAll("a").Should().ContainSingle().Which;
        link.TextContent.Trim().Should().Be("Switch to Assessor");
        link.GetAttribute("href").Should().Be("/dashboard/switch/Assessor", "no return address: it lands on the new role's Home");
        link.GetAttribute("data-enhance-nav").Should().Be("false", "a switch is a full page load, from a static page too");
    }

    [Fact]
    public void ThreeOrMoreRoles_OfferANativeChangeRoleDisclosure_ListingEachOtherRole_InPrecedenceOrder()
    {
        var cut = Render([WombatRoles.Assessor, WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], WombatRoles.SpecialityAdmin);

        var details = cut.Find(".role-head details");
        details.HasAttribute("open").Should().BeFalse("closed until asked");
        details.QuerySelector("summary")!.TextContent.Trim().Should().Be("Change role");

        var options = details.QuerySelectorAll("a");
        options.Select(a => a.TextContent.Trim()).Should().Equal("Switch to Committee member", "Switch to Assessor");
        options.Select(a => a.GetAttribute("href")).Should().Equal("/dashboard/switch/CommitteeMember", "/dashboard/switch/Assessor");
        options.Should().OnlyContain(a => a.GetAttribute("data-enhance-nav") == "false");
        cut.FindAll(".role-head > a").Should().BeEmpty("the options are inside the disclosure");
    }

    [Theory]
    [MemberData(nameof(NavMenuAuthorizationTests.EveryRole), MemberType = typeof(NavMenuAuthorizationTests))]
    public void OnlyHeldRoles_AreOffered_NeverTheActingOne(string acting)
    {
        // Every role held beside Assessor and Trainee: the switch names the others it holds and nothing it does not.
        var held = new[] { acting, WombatRoles.Assessor, WombatRoles.Trainee }.Distinct().ToArray();
        var cut = Render(held, acting);

        var offered = cut.FindAll(".role-head a").Select(a => a.GetAttribute("href")!["/dashboard/switch/".Length..]).ToList();
        offered.Should().BeEquivalentTo(held.Where(role => role != acting));
        offered.Should().NotContain(acting);
        cut.FindAll(".role-head a").Should().OnlyContain(a => !a.GetAttribute("href")!.Contains('?'));
    }

    [Fact]
    public void OneRole_HasAHead_AndNoSwitch()
    {
        var cut = Render([WombatRoles.Trainee], WombatRoles.Trainee);

        cut.Find(".role-now").TextContent.Trim().Should().Be("Acting as Trainee");
        cut.FindAll(".role-head a, .role-head details").Should().BeEmpty();
    }

    [Fact]
    public void APendingTrainee_IsActingAsPendingTrainee_LikeAnyRole()
        => Render([WombatRoles.PendingTrainee], WombatRoles.PendingTrainee).Find(".role-now").TextContent.Trim()
            .Should().Be("Acting as Pending trainee");

    [Fact]
    public void NoRole_HasNoHead_AndTheListStartsUnderTheBrand()
    {
        var cut = Render([], null);

        cut.FindAll(".role-head").Should().BeEmpty("D8: no \"Acting as\" without a role");
        cut.Markup.Should().NotContain("Acting as");
        cut.Find(".nav-panel").FirstElementChild!.ClassList.Should().Contain("nav-list");
    }

    [Fact]
    public void TheRolesName_WrapsAndIsNeverCut()
    {
        // R2-Phone-Bars 2: "Sub-speciality admin" takes two lines; nothing truncates a role.
        var css = Wombat.Web.Tests.Design.Stylesheet.Parse(File.ReadAllText(Wombat.Web.Tests.Design.Stylesheet.WebFile("Components", "Layout", "NavMenu.razor.css")));

        css.Where(rule => rule.Selectors.Any(selector => selector.StartsWith(".role-now", StringComparison.Ordinal)))
            .SelectMany(rule => rule.Declarations)
            .Should().NotContain(declaration => declaration.Property == "text-overflow" || declaration.Value == "nowrap");
        css.Should().Contain(rule => rule.Selector == ".role-now" && rule.Value("flex-wrap") == "wrap");
    }

    private IRenderedComponent<NavMenu> Render(string[] held, string? acting)
    {
        var role = ActingRoleResolver.Resolve(acting, held);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            held.Select(r => new Claim(ClaimTypes.Role, r)).Prepend(new Claim(ClaimTypes.NameIdentifier, "zulu")), "Test"));

        return RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(role)
            .AddCascadingValue(NavItems.For(role, user)));
    }

    private static List<string> Labels(IRenderedComponent<NavMenu> cut)
        => cut.FindAll(".nav-link").Select(a => a.TextContent.Trim()).ToList();
}
