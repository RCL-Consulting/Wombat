using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Components.Layout;
using Wombat.Web.Navigation;
using static Wombat.Web.Tests.Navigation.PageAccess;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// The navigation's content, per acting role (T335, flow 01; T178, T141, T252; DESIGN.md § The NavMenu). Every link opens a
/// page that admits the acting role offering it, no link is offered twice or under a name its page does not carry, and the
/// navigation is the acting role's alone, never the union of the roles held.
/// </summary>
/// <remarks>
/// A link is judged by <see cref="PageAccess" />: <c>PageFor</c> finds the page the router would open, <c>RefusalOf</c>
/// judges it by the app's own policies. The role dashboards are held to the same rule by DashboardLinkAuthorizationTests
/// (T261). What the navigation offers is <see cref="NavItems.For" />, a pure function of the acting role and the claims,
/// so most of this reads it; the rest renders <see cref="NavMenu" /> to show the markup carries it.
/// </remarks>
public sealed partial class NavMenuAuthorizationTests : TestContext
{
    private const string NoRoleRow = "No role";

    public NavMenuAuthorizationTests()
    {
        // NavMenu scrolls its lit item into view after it renders; none is lit here, and the call is not the subject.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static TheoryData<string> EveryRole() => new(WombatRoles.All);

    // ---- per acting role ----

    [Theory]
    [MemberData(nameof(EveryRole))]
    public async Task EveryLinkOffered_OpensAPageTheActingRoleIsAdmittedTo(string role)
    {
        // T141's defect as a rule, for every role (T178), now per acting role: every link opens a page that admits the role
        // shown. A page is judged by the app's own policies, so a Policy = "..." page is judged as the router judges it.
        var hrefs = Hrefs(Render(Holder([role]), role));

        hrefs.Should().NotBeEmpty();
        foreach (var href in hrefs)
        {
            var page = PageFor(href);
            page.Should().NotBeNull($"the nav link {href} should route to a page");
            (await RefusalOf(page!, Holder([role]))).Should().BeNull($"a {role} is offered {href}");
        }
    }

    [Theory]
    [MemberData(nameof(EveryRole))]
    public void TheMenu_IsTheActingRolesAlone_NotTheUnionOfTheRolesHeld(string role)
    {
        // Dr Zulu holds Committee member and Assessor: acting as either, she sees that role's links and no other's. Every
        // role is held beside every other here, so a link that leaked from a held role into another's menu fails. The
        // personal links are the person's, so holding Trainee adds My progress whatever the acting role (D8).
        var alone = Written(NavItems.For(ActingAs(role, role), Holder([role])));

        foreach (var other in WombatRoles.All.Where(other => other != role))
        {
            Written(NavItems.For(ActingAs(role, role, other), Holder([role, other])))
                .Should().Be(alone, $"acting as {role}, holding {other} too, the menu is still the {role}'s");
        }
    }

    [Theory]
    [MemberData(nameof(EveryRole))]
    public void NoLink_IsOfferedTwice_OrUnderAnotherLinksName(string role)
    {
        // T178: the Coordinator was offered "Data Rights" twice. Personal links included, a graduate's claim too.
        var menu = NavItems.For(ActingAs(role, role), Holder([role], TraineeRecord()));

        menu.Items.Select(item => item.Label).Should().OnlyHaveUniqueItems();
        menu.Items.Select(item => item.Href).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [MemberData(nameof(EveryRole))]
    public void AMenuOfMoreThanEightLinks_IsGrouped_AndOneOfEightOrFewer_IsFlat(string role)
    {
        var menu = NavItems.For(ActingAs(role, role), Holder([role]));
        var count = menu.Items.Count();

        if (count > NavItems.FlatLimit)
        {
            menu.Groups[0].Heading.Should().BeNull();
            menu.Groups[0].Items.Should().Equal([NavItems.Home], "Home stands alone above the groups");
            menu.Groups.Skip(1).Should().OnlyContain(group => group.Heading != null && group.Items.Count > 0);
        }
        else
        {
            menu.Groups.Should().ContainSingle().Which.Heading.Should().BeNull($"{count} links are one flat list");
        }
    }

    [Fact]
    public void OnlyTheAdministrators_AndTheInstitutionalAdmins_AreGrouped()
    {
        WombatRoles.All.Where(role => NavItems.For(ActingAs(role, role), Holder([role])).Groups.Count > 1)
            .Should().BeEquivalentTo([WombatRoles.Administrator, WombatRoles.InstitutionalAdmin], "17 and 16 links, A-Spec");
    }

    [Theory]
    [MemberData(nameof(EveryRole))]
    public void HomeComesFirst_AndMyDataRightsLast(string role)
    {
        var items = NavItems.For(ActingAs(role, role), Holder([role])).Items.ToList();

        items[0].Should().Be(NavItems.Home);
        items[^1].Should().Be(NavItems.MyDataRights);
    }

    // ---- what the menu never holds ----

    [Theory]
    [MemberData(nameof(EveryRole))]
    public void NoLink_TargetsAPlaceholder_OrSignsOut_OrOpensMyAccount(string role)
    {
        // The nav never links to an unbuilt page (the placeholder page went with its five stubs, S22e), and the account row
        // carries Sign out and My account.
        var cut = Render(Holder([role], TraineeRecord()), role);

        Hrefs(cut).Should().NotContain(href => href.StartsWith("/placeholder", StringComparison.OrdinalIgnoreCase));
        Hrefs(cut).Should().NotContain(["/account/profile", "/account/logout"]);
        cut.FindAll("form, button").Should().BeEmpty("Sign out is the account row's");
        cut.FindAll("a").Select(a => a.TextContent.Trim()).Should().NotContain(["Logout", "Sign out", "My account"]);
    }

    [Fact]
    public void NoPlaceholderPage_Exists()
    {
        Pages.Should().NotContain(page => page.Template.StartsWith("/placeholder", StringComparison.OrdinalIgnoreCase),
            "the five stubs were dropped or moved to later flows, and their page with them");
    }

    [Theory]
    [MemberData(nameof(EveryRole))]
    public void NoRole_IsOfferedTheRetiredAssessmentFormsScreen(string role)
    {
        // T145: which instrument may assess an EPA is the curriculum item's permitted-tools list (T122); the legacy screen
        // is retired, no role is offered it, and no page answers its routes.
        Hrefs(Render(Holder([role]), role)).Should().NotContain(href => href.StartsWith("/admin/forms", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("/admin/forms")]
    [InlineData("/admin/forms/new")]
    [InlineData("/admin/forms/1")]
    public void NoPage_AnswersARetiredAssessmentFormsRoute(string href)
        => PageFor(href).Should().BeNull($"{href} was the retired assessment-forms screen, so it answers 404");

    // ---- the personal links (D8, T252) ----

    [Fact]
    public async Task AGraduateHoldingNoRole_IsOfferedHomeMyProgressAndMyDataRights_ByPagesThatAdmitThem()
    {
        var graduate = Holder([], TraineeRecord());
        var menu = NavItems.For(ActingRole.None, graduate);

        Labels(menu).Should().Equal("Home", "My progress", "My data rights");
        menu.Personal.Should().Equal(NavItems.MyProgress, NavItems.MyDataRights);
        foreach (var item in menu.Items)
        {
            (await RefusalOf(PageFor(item.Href)!, graduate)).Should().BeNull($"a graduate is offered {item.Href}");
        }
    }

    [Fact]
    public void AGraduateActingAsAssessor_SeesTheInbox_ThenMyProgressAndMyDataRights_UnderTheRule()
    {
        var cut = Render(Holder([WombatRoles.Assessor], TraineeRecord()), WombatRoles.Assessor);

        cut.FindAll(".nav-items:not(.nav-personal) a").Select(a => a.TextContent.Trim()).Should().Equal("Home", "Activity inbox");
        cut.FindAll(".nav-personal a").Select(a => a.TextContent.Trim()).Should().Equal("My progress", "My data rights");
    }

    [Theory]
    [InlineData(new[] { WombatRoles.Trainee }, true)]
    [InlineData(new[] { WombatRoles.Trainee, WombatRoles.Assessor }, false)]
    public void ATrainee_SeesMyProgressOnce_AsAPersonalLink_WhateverTheActingRole(string[] held, bool withClaim)
    {
        var user = withClaim ? Holder(held, TraineeRecord()) : Holder(held);
        foreach (var acting in held)
        {
            var menu = NavItems.For(ActingAs(acting, held), user);
            menu.Items.Count(item => item == NavItems.MyProgress).Should().Be(1, $"acting as {acting}");
            menu.Personal.Should().Contain(NavItems.MyProgress);
        }
    }

    [Theory]
    [InlineData(WombatRoles.PendingTrainee)]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.CommitteeMember)]
    public async Task WithoutTheTraineeRoleOrTheClaim_MyProgressIsNotOffered_AndItsPageRefuses(string role)
    {
        NavItems.For(ActingAs(role, role), Holder([role])).Items.Should().NotContain(NavItems.MyProgress);
        (await RefusalOf(PageFor("/portfolio/progress")!, role)).Should().NotBeNull();
    }

    // ---- particular pages ----

    // T141: these pages are [Authorize(Roles = ...)] without PendingTrainee, so the nav must not offer them to one.
    [Theory]
    [InlineData("/portfolio/progress")]
    [InlineData("/msf/my-reports")]
    [InlineData("/committee/my-reviews")]
    [InlineData("/portfolio/export")]
    public void APendingTrainee_IsNotOfferedATraineeOnlyPage(string href)
    {
        var menu = NavItems.For(ActingAs(WombatRoles.PendingTrainee, WombatRoles.PendingTrainee), Holder([WombatRoles.PendingTrainee]));

        menu.Items.Should().Contain(NavItems.MyActivities, "guard: the pending trainee's own links are there");
        menu.Items.Select(item => item.Href).Should().NotContain(href);
    }

    // T131 slice 6: the decisions-due page is for the roles that schedule reviews, and its page admits each of them.
    [Theory]
    [InlineData(WombatRoles.Coordinator, true)]
    [InlineData(WombatRoles.InstitutionalAdmin, true)]
    [InlineData(WombatRoles.SpecialityAdmin, true)]
    [InlineData(WombatRoles.SubSpecialityAdmin, true)]
    [InlineData(WombatRoles.Administrator, true)]
    [InlineData(WombatRoles.CommitteeMember, false)]
    [InlineData(WombatRoles.Trainee, false)]
    [InlineData(WombatRoles.Assessor, false)]
    public async Task DecisionsDue_IsOfferedToTheRolesThatScheduleReviews_AndItsPageAdmitsExactlyThem(string role, bool schedules)
    {
        NavItems.For(ActingAs(role, role), Holder([role])).Offers(NavItems.DecisionsDue).Should().Be(schedules);
        ((await RefusalOf(PageFor("/committee/decisions-due")!, role)) is null).Should().Be(schedules);
    }

    // T300: the College writes its disciplines' activity types (T091), so the builder admits a CollegeAdmin and the nav
    // offers it. The builder's policy still refuses the roles that write none.
    [Theory]
    [InlineData("/admin/activity-types")]
    [InlineData("/admin/activity-types/new")]
    [InlineData("/admin/activity-types/11")]
    public async Task ACollegeAdmin_IsAdmittedToTheActivityTypeBuilder_AndOfferedIt(string href)
    {
        (await RefusalOf(PageFor(href)!, WombatRoles.CollegeAdmin)).Should().BeNull();
        NavItems.For(ActingAs(WombatRoles.CollegeAdmin, WombatRoles.CollegeAdmin), Holder([WombatRoles.CollegeAdmin]))
            .Offers(NavItems.ActivityTypes).Should().BeTrue();
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.Trainee)]
    public async Task ARoleThatWritesNoActivityType_IsNotAdmittedToTheBuilder(string role)
    {
        (await RefusalOf(PageFor("/admin/activity-types")!, role)).Should().NotBeNull();
        (await RefusalOf(PageFor("/admin/activity-types/11")!, role)).Should().NotBeNull();
    }

    // The judge is not vacuous: pages the nav does not offer a role refuse it.
    [Theory]
    [InlineData("/admin/curricula", WombatRoles.SpecialityAdmin)]
    [InlineData("/admin/curricula", WombatRoles.SubSpecialityAdmin)]
    [InlineData("/admin/invitations", WombatRoles.Coordinator)]
    public async Task APageTheNavDoesNotOfferARole_RefusesThatRole(string href, string role)
    {
        (await RefusalOf(PageFor(href)!, role)).Should().NotBeNull($"{href} does not admit a {role}");
        NavItems.For(ActingAs(role, role), Holder([role])).Items.Select(item => item.Href).Should().NotContain(href);
    }

    // T269: the coordinator's MSF report page sends a campaign's subject to their own copy, whatever role brought them.
    [Theory]
    [MemberData(nameof(EveryRole))]
    public async Task EveryRoleTheCoordinatorsReportPageAdmits_IsAdmittedToTheTraineesCopy(string role)
    {
        if (await RefusalOf(PageFor("/msf/reports/1")!, role) is not null)
        {
            return;
        }

        (await RefusalOf(PageFor("/msf/my-reports/1")!, role)).Should().BeNull(
            $"a {role} who is a campaign's subject is sent from /msf/reports/1 to /msf/my-reports/1");
    }

    // ---- each item names its page ----

    [Fact]
    public void EveryItem_NamesThePageItsAddressOpens()
    {
        foreach (var item in AllItems())
        {
            PageFor(item.Href).Should().Be(item.Page, $"{item.Label}'s address, {item.Href}, opens that page");
        }
    }

    [Fact]
    public void EveryItemsLabel_IsItsPagesTabStem()
    {
        // "A nav label, its page's h1, its breadcrumb and its tab title stem are the same words" (R2-Tokens; D10). The tab's
        // stem is held to the h1 by PageTitleTests; this holds the label to the tab.
        foreach (var item in AllItems())
        {
            var source = File.ReadAllText(SourceOf(item.Page));
            var title = PageTitle().Match(source);
            title.Success.Should().BeTrue($"{item.Page.Name} has a literal <PageTitle>");
            var text = Regex.Replace(title.Groups["text"].Value, @"\s+", " ").Trim();

            text.Should().Be($"{item.Label} · Wombat", $"the {item.Label} link names its page as its tab does");
        }
    }

    // ---- DESIGN.md's table ----

    // T178: DESIGN.md's table had gone stale for most roles. It is read and compared, row by row, with the menu, so the
    // two change together.
    [Fact]
    public void DesignMdNavTable_MatchesTheMenu_ForEveryActingRole()
    {
        var (roles, personal) = DesignNavTables();

        roles.Keys.Should().BeEquivalentTo(WombatRoles.All.Append(NoRoleRow),
            "DESIGN.md's nav table has a row for every role in WombatRoles.All, one for no role, and no other");

        foreach (var role in WombatRoles.All)
        {
            Written(NavItems.For(ActingAs(role, role), Holder([role]))).Should().Be(roles[role], $"DESIGN.md's {role} row is the {role}'s menu");
        }

        Written(NavItems.For(ActingRole.None, Holder([]))).Should().Be(roles[NoRoleRow]);
        personal.Should().Equal(
            NavItems.For(ActingRole.None, Holder([], TraineeRecord())).Personal.Select(item => item.Label),
            "DESIGN.md's personal links, in order");
    }

    // ---- the markup ----

    [Fact]
    public void AGroupsHeading_IsNotALink_NorAHeading_AndNamesItsList()
    {
        var cut = Render(Holder([WombatRoles.Administrator]), WombatRoles.Administrator);

        var headings = cut.FindAll(".nav-group-heading");
        headings.Select(heading => heading.TextContent.Trim()).Should().Equal("Platform", "Organisations", "People", "Catalogue", "Reviews");
        headings.Should().OnlyContain(heading => heading.LocalName == "p", "the page's h1 is the first heading on it");
        foreach (var heading in headings)
        {
            cut.Find($"ul[aria-labelledby='{heading.Id}']").Should().NotBeNull($"the {heading.TextContent.Trim()} list is named by its heading");
        }

        cut.Find("nav").GetAttribute("aria-label").Should().Be("Main");
    }

    [Fact]
    public void EveryLink_CarriesItsIcon_AndItsLabel()
    {
        var cut = Render(Holder([WombatRoles.Trainee], TraineeRecord()), WombatRoles.Trainee);

        foreach (var link in cut.FindAll(".nav-link"))
        {
            link.QuerySelector("svg.icon use")!.GetAttribute("href").Should().StartWith("/icons/");
            link.QuerySelector(".nav-label")!.TextContent.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void EveryIcon_TheMenuNames_IsAFileTheAppServes()
    {
        var icons = Path.Combine(SolutionRoot(), "src", "Wombat.Web", "wwwroot", "icons");

        AllItems().Select(item => item.Icon).Distinct()
            .Where(icon => !File.Exists(Path.Combine(icons, icon + ".svg")))
            .Should().BeEmpty("Icon.razor uses /icons/{name}.svg#i");
    }

    // ---- helpers ----

    private IRenderedComponent<NavMenu> Render(ClaimsPrincipal user, string? acting)
    {
        var role = ActingRoleResolver.Resolve(acting, user.FindAll(ClaimTypes.Role).Select(claim => claim.Value));
        return RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(role)
            .AddCascadingValue(NavItems.For(role, user)));
    }

    private static List<string> Hrefs(IRenderedComponent<NavMenu> cut)
        => cut.FindAll("a.nav-link").Select(a => a.GetAttribute("href") ?? string.Empty).ToList();

    private static List<string> Labels(NavMenuModel menu) => menu.Items.Select(item => item.Label).ToList();

    /// <summary>A role's menu as DESIGN.md writes it: "Home, A, B", or grouped "Home; Heading: A, B; Heading: C".</summary>
    private static string Written(NavMenuModel menu)
        => string.Join("; ", menu.Groups.Select(group =>
            (group.Heading is null ? string.Empty : group.Heading + ": ") + string.Join(", ", group.Items.Select(item => item.Label))));

    /// <summary>Every item the menu can offer, in any role, with any claim.</summary>
    private static List<NavItem> AllItems()
        => WombatRoles.All
            .SelectMany(role => NavItems.For(ActingAs(role, role), Holder([role], TraineeRecord())).Items)
            .Distinct()
            .ToList();

    /// <summary>An acting role: <paramref name="role" />, among the roles held.</summary>
    private static ActingRole ActingAs(string role, params string[] held) => ActingRoleResolver.Resolve(role, held);

    /// <summary>A signed-in holder of these roles, carrying each role's scope claims, and these claims.</summary>
    private static ClaimsPrincipal Holder(string[] roles, params Claim[] claims)
    {
        var all = roles.SelectMany(role => PrincipalFor(role).Claims)
            .Where(claim => claim.Type != ClaimTypes.NameIdentifier)
            .DistinctBy(claim => (claim.Type, claim.Value))
            .Prepend(new Claim(ClaimTypes.NameIdentifier, "user"))
            .Concat(claims);
        return new ClaimsPrincipal(new ClaimsIdentity(all, authenticationType: "Test"));
    }

    /// <summary>The claim sign-in issues to anyone holding a trainee profile, current or ended (T252).</summary>
    private static Claim TraineeRecord() => new(WombatClaims.TraineeRecord, "true");

    private static string SourceOf(Type page)
        => Path.Combine(
            [SolutionRoot(), "src", "Wombat.Web", .. page.FullName!["Wombat.Web.".Length..].Split('.')[..^1], page.Name + ".razor"]);

    /// <summary>
    /// DESIGN.md § The NavMenu's two tables: "| Acting role | Links |", a row per role and one for no role, and
    /// "| Personal link | Offered to |", in order.
    /// </summary>
    private static (Dictionary<string, string> Roles, List<string> Personal) DesignNavTables()
    {
        var design = File.ReadAllLines(Path.Combine(SolutionRoot(), "execution", "architecture", "DESIGN.md"));
        var section = design
            .SkipWhile(line => line != "## The NavMenu")
            .Skip(1)
            .TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal))
            .ToList();
        section.Should().NotBeEmpty("DESIGN.md has a § The NavMenu");

        var rows = new Dictionary<string, List<string[]>>(StringComparer.Ordinal);
        string? table = null;
        foreach (var line in section)
        {
            if (!line.StartsWith('|'))
            {
                table = null;
                continue;
            }

            var cells = line.Split('|', StringSplitOptions.TrimEntries)[1..^1];
            if (table is null)
            {
                table = cells[0];
                rows[table] = [];
            }
            else if (!cells[0].StartsWith("--", StringComparison.Ordinal))
            {
                rows[table].Add(cells);
            }
        }

        rows.Keys.Should().Contain(["Acting role", "Personal link"], "DESIGN.md § The NavMenu has both tables");
        return (
            rows["Acting role"].ToDictionary(cells => cells[0], cells => cells[1], StringComparer.Ordinal),
            rows["Personal link"].Select(cells => cells[0]).ToList());
    }

    [GeneratedRegex(@"<PageTitle>(?<text>[^<@]*)</PageTitle>")]
    private static partial Regex PageTitle();
}
