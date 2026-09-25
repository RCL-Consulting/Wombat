using System.Reflection;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Layout;

namespace Wombat.Web.Tests.Navigation;

public sealed class NavMenuAuthorizationTests : TestContext
{
    // Exact link text, not a substring of the markup: "Activities" must be its own link, not the tail of "My Activities".
    [Theory]
    [InlineData(WombatRoles.Trainee, new[] { "Activities", "My Activities", "MSF Reports", "Committee Reviews", "My Progress", "Export Portfolio", "My Account", "Logout" })]
    [InlineData(WombatRoles.PendingTrainee, new[] { "Activities", "My Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Assessor, new[] { "Activity Inbox", "Recent Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Coordinator, new[] { "Invitations", "Stalled Activities", "My Account", "Logout" })]
    [InlineData(WombatRoles.Administrator, new[] { "Institutions", "Invitations", "Users", "Activity Types", "System", "My Account", "Logout" })]
    public void NavMenu_ShowsExpectedLinksForRole(string role, string[] expectedLinks)
    {
        var cut = RenderFor(role);

        var labels = cut.FindAll(".nav-link").Select(e => e.TextContent.Trim()).ToList();

        labels.Should().Contain(expectedLinks);
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
    [InlineData("/committee/my-reviews", "Committee Reviews")]
    public void PendingTrainee_IsNotOfferedATraineeOnlyPage(string href, string label)
    {
        var cut = RenderFor(WombatRoles.PendingTrainee);
        var links = cut.FindAll("a.nav-link");

        // The PendingTrainee still gets the shared trainee block, so the absence below is not an unauthorized render.
        links.Should().Contain(a => a.GetAttribute("href") == "/activities/mine");

        links.Should().NotContain(a => a.GetAttribute("href") == href);
        links.Should().NotContain(a => a.TextContent.Trim() == label);
    }

    // T141's defect as a rule: every link the nav offers a trainee, pending or admitted, opens a page that admits them.
    // A new link added to the block shared with PendingTrainee fails here if its page is Trainee-only.
    [Theory]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.PendingTrainee)]
    public void EveryLinkOffered_OpensAPageTheRoleIsAdmittedTo(string role)
    {
        var cut = RenderFor(role);
        var hrefs = cut.FindAll("a.nav-link").Select(a => a.GetAttribute("href") ?? string.Empty).ToList();

        hrefs.Should().NotBeEmpty();
        foreach (var href in hrefs)
        {
            var page = PageFor(href);
            page.Should().NotBeNull($"the nav link {href} should route to a page");
            RefusalOf(page!, role).Should().BeNull($"a {role} is offered {href}");
        }
    }

    // T131 slice 6: the decisions-due page is for the roles that schedule reviews, one nav click away, and its page
    // admits each of them. A CommitteeMember is not offered it: the page has no CommitteeMember arm.
    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void ASchedulingRole_IsOfferedDecisionsDue_ByAPageThatAdmitsIt(string role)
    {
        var cut = RenderFor(role);

        var link = cut.FindAll("a.nav-link").SingleOrDefault(a => a.TextContent.Trim() == "Decisions Due");

        link.Should().NotBeNull($"a {role} schedules committee reviews");
        link!.GetAttribute("href").Should().Be("/committee/decisions-due");
        RefusalOf(PageFor("/committee/decisions-due")!, role).Should().BeNull();
    }

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.Assessor)]
    public void ARoleThatSchedulesNoReview_IsNotOfferedDecisionsDue_AndItsPageRefusesIt(string role)
    {
        var cut = RenderFor(role);

        cut.FindAll("a.nav-link").Should().NotContain(a => a.GetAttribute("href") == "/committee/decisions-due");
        RefusalOf(PageFor("/committee/decisions-due")!, role).Should().NotBeNull();
    }

    private IRenderedComponent<NavMenu> RenderFor(string role)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("user@example.com");
        auth.SetRoles(role);

        return RenderComponent<NavMenu>();
    }

    // A literal route wins over a parameterised one, as in the router: /activities/new is not /activities/{Id}.
    private static Type? PageFor(string href)
    {
        var path = "/" + href.TrimStart('/');
        var pages = typeof(NavMenu).Assembly.GetTypes()
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(r => (Page: t, r.Template)))
            .ToList();

        return pages.FirstOrDefault(p => string.Equals(p.Template, path, StringComparison.OrdinalIgnoreCase)).Page
            ?? pages.FirstOrDefault(p => Matches(p.Template, path)).Page;
    }

    private static bool Matches(string template, string path)
    {
        var want = template.Trim('/').Split('/');
        var got = path.Trim('/').Split('/');

        return want.Length == got.Length
            && want.Zip(got).All(s => s.First.StartsWith('{') || string.Equals(s.First, s.Second, StringComparison.OrdinalIgnoreCase));
    }

    // Null when the role gets in; otherwise why not. A policy cannot be judged here, so it counts as a refusal.
    private static string? RefusalOf(Type page, string role)
    {
        if (page.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any())
        {
            return null;
        }

        foreach (var authorize in page.GetCustomAttributes<AuthorizeAttribute>(inherit: true))
        {
            if (!string.IsNullOrEmpty(authorize.Policy))
            {
                return $"{page.Name} requires policy {authorize.Policy}";
            }

            if (!string.IsNullOrEmpty(authorize.Roles)
                && !authorize.Roles.Split(',', StringSplitOptions.TrimEntries).Contains(role))
            {
                return $"{page.Name} admits only {authorize.Roles}";
            }
        }

        return null;
    }
}
