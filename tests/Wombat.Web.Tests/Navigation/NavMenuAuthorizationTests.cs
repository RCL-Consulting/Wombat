using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Components.Layout;
using Wombat.Web.Components.Pages.Placeholder;
using static Wombat.Web.Tests.Navigation.PageAccess;

namespace Wombat.Web.Tests.Navigation;

public sealed class NavMenuAuthorizationTests : TestContext
{
    private const string SignedOutRow = "Signed out";
    private const string EveryoneRow = "Everyone signed in";

    // A link is judged by PageAccess: PageFor finds the page the router would open, RefusalOf judges it by the app's own
    // policies. The role dashboards are held to the same rule by DashboardLinkAuthorizationTests (T261).

    public static TheoryData<string> EveryRole() => new(WombatRoles.All);

    public static TheoryData<string[]> EveryRoleAndAllOfThem()
    {
        var data = new TheoryData<string[]>();
        foreach (var role in WombatRoles.All)
        {
            data.Add([role]);
        }

        data.Add(WombatRoles.All.ToArray());
        return data;
    }

    public static TheoryData<string[]> EveryPairOfRolesAndAllOfThem()
    {
        var data = new TheoryData<string[]>();
        for (var first = 0; first < WombatRoles.All.Count; first++)
        {
            for (var second = first + 1; second < WombatRoles.All.Count; second++)
            {
                data.Add([WombatRoles.All[first], WombatRoles.All[second]]);
            }
        }

        data.Add(WombatRoles.All.ToArray());
        return data;
    }

    // Exact link text, not a substring of the markup: "Activities" must be its own link, not the tail of "My Activities".
    [Theory]
    [InlineData(WombatRoles.Trainee, new[] { "Activities", "My Activities", "MSF Reports", "My Committee Reviews", "My Progress", "Export Portfolio", "My Account", "Logout" })]
    [InlineData(WombatRoles.PendingTrainee, new[] { "Activities", "My Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Assessor, new[] { "Activity Inbox", "Recent Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Coordinator, new[] { "Data Rights Requests", "MSF Campaigns", "Stalled Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Administrator, new[] { "Institutions", "Invitations", "Users", "Activity Types", "System", "My Account", "Logout" })]
    public void NavMenu_ShowsExpectedLinksForRole(string role, string[] expectedLinks)
    {
        var cut = RenderFor(role);

        Labels(cut).Should().Contain(expectedLinks);
    }

    // T141: a trainee's progress page is one nav click away, not behind a placeholder.
    [Fact]
    public void Trainee_MyProgressLinksToTheProgressPage()
    {
        var cut = RenderFor(WombatRoles.Trainee);

        var link = cut.FindAll("a.nav-link").SingleOrDefault(a => a.TextContent.Trim() == "My Progress");

        link.Should().NotBeNull("a Trainee is offered My Progress");
        link!.GetAttribute("href").Should().Be("/portfolio/progress");
    }

    // T154: the portfolio review tells the trainee to export the period and hand the PDF over, so the export is one nav
    // click away rather than a URL typed from help text.
    [Fact]
    public void Trainee_ExportPortfolioLinksToTheExportPage()
    {
        var cut = RenderFor(WombatRoles.Trainee);

        var link = cut.FindAll("a.nav-link").SingleOrDefault(a => a.TextContent.Trim() == "Export Portfolio");

        link.Should().NotBeNull("a Trainee is offered Export Portfolio");
        link!.GetAttribute("href").Should().Be("/portfolio/export");
    }

    // T141: these pages are [Authorize(Roles = ...)] without PendingTrainee, so the nav must not offer them to one.
    [Theory]
    [InlineData("/portfolio/progress", "My Progress")]
    [InlineData("/msf/my-reports", "MSF Reports")]
    [InlineData("/committee/my-reviews", "My Committee Reviews")]
    public void PendingTrainee_IsNotOfferedATraineeOnlyPage(string href, string label)
    {
        var cut = RenderFor(WombatRoles.PendingTrainee);
        var links = cut.FindAll("a.nav-link");

        // The PendingTrainee still gets the shared trainee block, so the absence below is not an unauthorized render.
        links.Should().Contain(a => a.GetAttribute("href") == "/activities/mine");

        links.Should().NotContain(a => a.GetAttribute("href") == href);
        links.Should().NotContain(a => a.TextContent.Trim() == label);
    }

    // T141's defect as a rule, for every role (T178): every link the nav offers opens a page that admits the role. A page
    // is judged by the app's own policies, so a Policy = "..." page is judged as the router judges it, not refused
    // outright. A link added to a section whose page refuses one of the section's roles fails here.
    [Theory]
    [MemberData(nameof(EveryRole))]
    public async Task EveryLinkOffered_OpensAPageTheRoleIsAdmittedTo(string role)
    {
        var cut = RenderFor(role);
        var hrefs = cut.FindAll("a.nav-link").Select(a => a.GetAttribute("href") ?? string.Empty).ToList();

        hrefs.Should().NotBeEmpty();
        foreach (var href in hrefs)
        {
            var page = PageFor(href);
            page.Should().NotBeNull($"the nav link {href} should route to a page");
            (await RefusalOf(page!, role)).Should().BeNull($"a {role} is offered {href}");
        }
    }

    // The judge is not vacuous: the pages T178 took out of the nav refuse the roles that used to be offered them.
    [Theory]
    [InlineData("/admin/curricula", WombatRoles.SpecialityAdmin)]
    [InlineData("/admin/curricula", WombatRoles.SubSpecialityAdmin)]
    [InlineData("/admin/invitations", WombatRoles.Coordinator)]
    public async Task APageTheNavNoLongerOffersARole_RefusesThatRole(string href, string role)
    {
        (await RefusalOf(PageFor(href)!, role)).Should().NotBeNull($"{href} does not admit a {role}");
        RenderFor(role).FindAll("a.nav-link").Should().NotContain(a => a.GetAttribute("href") == href);
    }

    // T178: the Coordinator was offered "Data Rights" twice, their own and the queue of everyone's. No role, and no mix of
    // roles, sees a page twice or two links with one name: sections are merged page by page, and labels are unique.
    [Theory]
    [MemberData(nameof(EveryRoleAndAllOfThem))]
    public void NoLink_IsOfferedTwice(string[] roles)
    {
        var links = RenderFor(roles).FindAll(".nav-link").ToList();

        links.Select(a => a.TextContent.Trim()).Should().OnlyHaveUniqueItems();
        links.Select(a => a.GetAttribute("href")).Where(href => href is not null).Should().OnlyHaveUniqueItems();
    }

    // T252: completing a programme removes the Trainee role, but a graduate still holds a trainee profile, and sign-in
    // gives them the trainee-record claim. It offers My Progress alone, and the page's policy admits them on it.
    [Fact]
    public async Task AGraduateHoldingNoRole_IsOfferedMyProgress_ByAPageThatAdmitsThem()
    {
        var cut = RenderFor([], TraineeRecord());

        var links = cut.FindAll("a.nav-link").ToList();
        links.Select(a => a.GetAttribute("href")).Where(href => href is not null && href != string.Empty)
            .Should().Equal(["/account/profile", "/account/data-rights", "/portfolio/progress"],
                "the record is theirs; the role's work, filing and exporting included, is not offered");
        (await RefusalOf(PageFor("/portfolio/progress")!, Holder(roles: [], TraineeRecord()))).Should().BeNull();
    }

    [Fact]
    public void AGraduateWhoIsNowAnAssessor_SeesMyProgressOnce_BeforeTheirAssessorLinks()
    {
        LabelsFor([WombatRoles.Assessor], TraineeRecord()).Should().Equal(
            "Home", "My Account", "Data Rights", "My Progress", "Activity Inbox", "Recent Activities", "Logout");
    }

    [Fact]
    public void ATraineeWhoHoldsTheClaimToo_SeesTheirOwnNav_UnchangedByIt()
    {
        LabelsFor([WombatRoles.Trainee], TraineeRecord()).Should().Equal(LabelsFor(WombatRoles.Trainee));
    }

    [Theory]
    [InlineData(WombatRoles.PendingTrainee)]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.CommitteeMember)]
    public async Task WithoutTheTraineeRoleOrTheClaim_MyProgressIsNotOffered_AndItsPageRefuses(string role)
    {
        RenderFor(role).FindAll("a.nav-link").Should().NotContain(a => a.GetAttribute("href") == "/portfolio/progress");
        (await RefusalOf(PageFor("/portfolio/progress")!, role)).Should().NotBeNull();
    }

    // T178: DESIGN.md's table had gone stale for most roles. It is now read and compared, row by row, with the nav as
    // rendered, so the two change together.
    [Fact]
    public void DesignMdNavTable_MatchesTheRenderedNav_ForEveryRole()
    {
        var rows = DesignNavTable().ToDictionary(row => row.Name, row => row.Items);

        rows.Keys.Should().BeEquivalentTo(
            WombatRoles.All.Append(SignedOutRow).Append(EveryoneRow),
            "DESIGN.md's nav table has a row for every role in WombatRoles.All, and no other");

        foreach (var role in WombatRoles.All)
        {
            LabelsFor(role).Should().Equal(SignedInNav(rows[EveryoneRow], rows[role]), $"DESIGN.md's {role} row is the {role}'s nav");
        }

        LabelsFor().Should().Equal(rows[SignedOutRow], "DESIGN.md's signed-out row is the nav");
    }

    // T178: DESIGN.md promises a user holding several roles each of their roles' rows, in the table's order, with a page an
    // earlier row already offered left out. Every pair of roles and all of them at once, so a reordered or regrouped
    // Sections that changes what such a user sees fails here even when every single-role nav is unchanged.
    [Theory]
    [MemberData(nameof(EveryPairOfRolesAndAllOfThem))]
    public void AUserWithSeveralRoles_SeesEachRolesRow_InTheTablesOrder_EachPageOnce(string[] roles)
    {
        var table = DesignNavTable();
        var everyone = table.Single(row => row.Name == EveryoneRow).Items;

        var offered = new HashSet<string>(StringComparer.Ordinal);
        var own = table
            .Where(row => roles.Contains(row.Name))
            .SelectMany(row => row.Items)
            .Where(offered.Add)
            .ToList();

        LabelsFor(roles).Should().Equal(
            SignedInNav(everyone, own),
            $"a holder of {string.Join(" and ", roles)} sees their rows of DESIGN.md's table in its order, each page once");
    }

    // T178: the placeholder names only the features the nav still links to it, each under the link's own name. An entry
    // no link reaches is a stub a real page has replaced; a link with no entry lands on "Coming soon".
    [Fact]
    public void ThePlaceholderPage_KnowsExactlyTheFeaturesTheNavLinksToIt()
    {
        const string prefix = "/placeholder/";
        var placeholderLinks = RenderFor(WombatRoles.All.ToArray()).FindAll("a.nav-link")
            .Where(a => (a.GetAttribute("href") ?? string.Empty).StartsWith(prefix, StringComparison.Ordinal))
            .ToDictionary(a => a.GetAttribute("href")![prefix.Length..], a => a.TextContent.Trim());

        placeholderLinks.Should().NotBeEmpty();
        PlaceholderPage.Headings.Should().BeEquivalentTo(placeholderLinks);
    }

    // T131 slice 6: the decisions-due page is for the roles that schedule reviews, one nav click away, and its page
    // admits each of them. A CommitteeMember is not offered it: the page has no CommitteeMember arm.
    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public async Task ASchedulingRole_IsOfferedDecisionsDue_ByAPageThatAdmitsIt(string role)
    {
        var cut = RenderFor(role);

        var link = cut.FindAll("a.nav-link").SingleOrDefault(a => a.TextContent.Trim() == "Decisions Due");

        link.Should().NotBeNull($"a {role} schedules committee reviews");
        link!.GetAttribute("href").Should().Be("/committee/decisions-due");
        (await RefusalOf(PageFor("/committee/decisions-due")!, role)).Should().BeNull();
    }

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.Assessor)]
    public async Task ARoleThatSchedulesNoReview_IsNotOfferedDecisionsDue_AndItsPageRefusesIt(string role)
    {
        var cut = RenderFor(role);

        cut.FindAll("a.nav-link").Should().NotContain(a => a.GetAttribute("href") == "/committee/decisions-due");
        (await RefusalOf(PageFor("/committee/decisions-due")!, role)).Should().NotBeNull();
    }

    // T300: the guard names the College as the author of its disciplines' activity types (T091), so the builder admits a
    // CollegeAdmin and the nav offers it to him. Until T300 both pages refused him and his nav had no Activity Types.
    [Theory]
    [InlineData("/admin/activity-types")]
    [InlineData("/admin/activity-types/new")]
    [InlineData("/admin/activity-types/11")]
    public async Task ACollegeAdmin_IsAdmittedToTheActivityTypeBuilder(string href)
    {
        (await RefusalOf(PageFor(href)!, WombatRoles.CollegeAdmin)).Should().BeNull($"a CollegeAdmin writes his College's types at {href}");
    }

    [Fact]
    public void ACollegeAdmin_IsOfferedActivityTypes()
    {
        var link = RenderFor(WombatRoles.CollegeAdmin).FindAll("a.nav-link").SingleOrDefault(a => a.TextContent.Trim() == "Activity Types");

        link.Should().NotBeNull("the College keeps its instruments in the builder");
        link!.GetAttribute("href").Should().Be("/admin/activity-types");
    }

    // The builder's policy still refuses the roles that write no activity type.
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

    // T145: the legacy assessment-forms screen let an administrator link a form to EPAs and said "EPA linked", but
    // nothing read the link. Which instrument may assess an EPA is the curriculum item's permitted-tools list (T122), so
    // the screen is retired: no role is offered it, and no page answers its routes.
    [Theory]
    [MemberData(nameof(EveryRole))]
    public void NoRole_IsOfferedTheRetiredAssessmentFormsScreen(string role)
    {
        var cut = RenderFor(role);

        cut.FindAll("a.nav-link").Should().NotContain(
            a => (a.GetAttribute("href") ?? string.Empty).StartsWith("/admin/forms", StringComparison.OrdinalIgnoreCase));
        cut.FindAll("a.nav-link").Should().NotContain(a => a.TextContent.Trim() == "Forms");
    }

    [Theory]
    [InlineData("/admin/forms")]
    [InlineData("/admin/forms/new")]
    [InlineData("/admin/forms/1")]
    public void NoPage_AnswersARetiredAssessmentFormsRoute(string href)
    {
        PageFor(href).Should().BeNull($"{href} was the retired assessment-forms screen, so it answers 404");
    }

    // T269: the coordinator's MSF report page sends a campaign's subject to their own copy, whatever role brought them
    // there, so every role that page admits is admitted to the trainee's page, or the subject would be sent to a
    // refusal. Not a nav link: the nav still offers MSF Reports to Trainee alone.
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

    private IRenderedComponent<NavMenu> RenderFor(params string[] roles) => RenderFor(roles, []);

    private IRenderedComponent<NavMenu> RenderFor(string[] roles, params Claim[] claims)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("user@example.com");
        auth.SetRoles(roles);
        auth.SetClaims(claims);

        return RenderComponent<NavMenu>();
    }

    /// <summary>The claim sign-in issues to anyone holding a trainee profile, current or ended (T252).</summary>
    private static Claim TraineeRecord() => new(WombatClaims.TraineeRecord, "true");

    // Every item's name, the links and the Logout button alike, in menu order.
    private static List<string> Labels(IRenderedComponent<NavMenu> cut)
        => cut.FindAll(".nav-link").Select(e => e.TextContent.Trim()).ToList();

    // The nav's labels for a user holding these roles, or signed out when there are none, each in a context of its own:
    // a context's authorization cannot be replaced once it has rendered.
    private static List<string> LabelsFor(params string[] roles) => LabelsFor(roles, []);

    private static List<string> LabelsFor(string[] roles, params Claim[] claims)
    {
        using var context = new TestContext();
        var auth = context.AddTestAuthorization();
        if (roles.Length > 0 || claims.Length > 0)
        {
            auth.SetAuthorized("user@example.com");
            auth.SetRoles(roles);
            auth.SetClaims(claims);
        }

        return Labels(context.RenderComponent<NavMenu>());
    }

    // A signed-in user's nav: the Everyone row, its … replaced by the role links.
    private static List<string> SignedInNav(List<string> everyone, IEnumerable<string> roleLinks)
    {
        var roleSlot = everyone.IndexOf("…");
        roleSlot.Should().BeGreaterThan(0, "DESIGN.md's Everyone row marks where the role's own links go with …");

        return everyone.Take(roleSlot).Concat(roleLinks).Concat(everyone.Skip(roleSlot + 1)).ToList();
    }

    // DESIGN.md § The NavMenu: "| Role | Items |" rows in the table's order, the items comma-separated.
    private static List<(string Name, List<string> Items)> DesignNavTable()
    {
        var design = File.ReadAllLines(Path.Combine(SolutionRoot(), "execution", "architecture", "DESIGN.md"));
        var section = design
            .SkipWhile(line => line != "## The NavMenu")
            .Skip(1)
            .TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal))
            .ToList();
        section.Should().NotBeEmpty("DESIGN.md has a § The NavMenu");

        return section
            .Where(line => line.StartsWith('|') && !line.StartsWith("|--", StringComparison.Ordinal))
            .Select(line => line.Split('|', StringSplitOptions.TrimEntries))
            .Where(cells => cells[1] != "Role")
            .Select(cells => (
                cells[1],
                cells[2].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ToList();
    }

    /// <summary>A signed-in user holding these roles, with no scope claim, and these claims.</summary>
    private static ClaimsPrincipal Holder(string[] roles, params Claim[] claims)
        => new(new ClaimsIdentity(
            roles.Select(role => new Claim(ClaimTypes.Role, role))
                .Prepend(new Claim(ClaimTypes.NameIdentifier, "user"))
                .Concat(claims),
            authenticationType: "Test"));
}
