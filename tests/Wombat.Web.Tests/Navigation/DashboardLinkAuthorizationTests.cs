using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.CommitteeMember;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.InstitutionalAdmin;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;
using static Wombat.Web.Tests.Navigation.PageAccess;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// T261: the first page each role sees had dead ends. The pending trainee's "Complete your profile" and the Coordinator's
/// "Issue invitation" went to routes no page answers, the InstitutionalAdmin's Institutions and Specialities to pages
/// that refuse one, and a CollegeAdmin, which Home had no case for, got the trainee's dashboard and its trainee-only
/// links. T178 holds every nav link to "opens a page that admits the role"; this holds every dashboard link to the same
/// rule, judged by the same <see cref="PageAccess" />. Home is rendered for a holder of each role, as the role sees it.
/// </summary>
public sealed class DashboardLinkAuthorizationTests
{
    private static readonly DateTime When = new(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc);

    public static TheoryData<string> EveryRole() => new(WombatRoles.All);

    [Theory]
    [MemberData(nameof(EveryRole))]
    public async Task EveryLinkOnTheRolesDashboard_OpensAPageTheRoleIsAdmittedTo(string role)
    {
        foreach (var href in RenderHomeFor(role).Hrefs)
        {
            var page = PageFor(href);
            page.Should().NotBeNull($"the {role}'s dashboard links to {href}, which should route to a page");
            (await RefusalOf(page!, role)).Should().BeNull($"the {role}'s dashboard offers {href}");
        }
    }

    // Home picks a dashboard per role. The one it picks admits the role, so no role falls through to another's
    // dashboard and the links written for that one (T261: the CollegeAdmin got the trainee's).
    [Theory]
    [MemberData(nameof(EveryRole))]
    public async Task Home_ShowsEachRoleADashboardThatAdmitsIt(string role)
    {
        var dashboard = RenderHomeFor(role).Dashboard;

        (await RefusalOf(dashboard, role)).Should().BeNull($"Home shows a {role} the {dashboard.Name}");
    }

    // The judge above reads each dashboard's [Authorize], which Blazor enforces on a routed page only: on a dashboard it
    // is the statement of the roles it is for. One with none would be judged by the fallback policy, which admits every
    // signed-in user, so Home could show it to any role and pass. Every dashboard names its roles or a policy.
    [Fact]
    public void EveryDashboard_NamesTheRolesItIsFor()
    {
        var dashboards = DashboardTypes();

        dashboards.Should().NotBeEmpty();
        foreach (var dashboard in dashboards)
        {
            var declared = dashboard.GetCustomAttributes(inherit: true).OfType<IAuthorizeData>().ToList();
            declared.Should().NotBeEmpty($"the {dashboard.Name} should say which roles it is for");
            declared.Should().OnlyContain(
                data => !string.IsNullOrWhiteSpace(data.Roles) || !string.IsNullOrWhiteSpace(data.Policy),
                $"a bare [Authorize] on the {dashboard.Name} admits every signed-in user");
        }
    }

    // The theories above judge what the dashboards render, so a link the renders never draw would go unjudged. Every
    // dashboard is shown to some role, and every link it can draw is drawn by a render of it: each link its own source
    // declares, and each declared by a component it names, and by the components those name (the shared
    // EpaTargetCoverageList, say). A new link, or one in a branch these renders do not reach, fails here until
    // DashboardSender draws it. Per dashboard, so a link another dashboard happens to draw does not stand in for it.
    [Fact]
    public void TheRenders_DrawEveryLinkEachDashboardCanDraw()
    {
        var drawn = WombatRoles.All
            .Select(RenderHomeFor)
            .GroupBy(home => home.Dashboard.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.SelectMany(home => home.Hrefs).ToList(), StringComparer.Ordinal);
        var sources = Directory.GetFiles(DashboardsFolder(), "*.razor");

        sources.Should().NotBeEmpty();
        foreach (var source in sources)
        {
            var dashboard = Path.GetFileNameWithoutExtension(source);
            drawn.Should().ContainKey(dashboard, $"Home shows the {dashboard} to some role");

            foreach (var (declaredIn, declared) in DeclaredLinks(source))
            {
                var pattern = PatternFor(declared);
                drawn[dashboard].Should().Contain(
                    href => pattern.IsMatch(href),
                    $"{declaredIn} declares the link {declared}, and a render of the {dashboard} should draw it to be judged");
            }
        }
    }

    // The walk above reaches what a dashboard names: the coverage list the three staff dashboards share, the card, and
    // what those name. The card's href only passes on its Href parameter, so the value is taken from the caller that sets
    // it, and Icon's SVG <use href> is no link.
    [Fact]
    public void TheWalk_ReadsTheComponentsADashboardNames()
    {
        Components(Path.Combine(DashboardsFolder(), "CommitteeMemberDashboard.razor"))
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .Should().Contain(["CommitteeMemberDashboard", "StatePanel", "Alert", "DashboardCard", "Icon", "EpaTargetCoverageList"]);

        var traineeLinks = DeclaredLinks(Path.Combine(DashboardsFolder(), "TraineeDashboard.razor")).ToList();
        traineeLinks.Should().Contain(("TraineeDashboard", "/portfolio/progress"));
        traineeLinks.Should().OnlyContain(link => link.DeclaredIn == "TraineeDashboard");
    }

    // The judge is not vacuous: each link T261 took off a dashboard answers no page, and none is offered now.
    [Theory]
    [InlineData(WombatRoles.PendingTrainee, "/account/manage")]
    [InlineData(WombatRoles.Coordinator, "/admin/invitations/new")]
    public void ALinkTakenOffADashboard_AnswersNoPage(string role, string href)
    {
        PageFor(href).Should().BeNull($"no page answers {href}");
        RenderHomeFor(role).Hrefs.Should().NotContain(href);
    }

    // ... or its page refuses the role that was offered it. The Coordinator is not sent to the invitations list either:
    // it admits only an Administrator or an InstitutionalAdmin (T178).
    [Theory]
    [InlineData(WombatRoles.Coordinator, "/admin/invitations")]
    [InlineData(WombatRoles.InstitutionalAdmin, "/admin/institutions")]
    [InlineData(WombatRoles.InstitutionalAdmin, "/admin/specialities")]
    [InlineData(WombatRoles.CollegeAdmin, "/portfolio/progress")]
    [InlineData(WombatRoles.CollegeAdmin, "/portfolio/authorisations")]
    public async Task ALinkTakenOffADashboard_OpensAPageThatRefusesTheRole(string role, string href)
    {
        (await RefusalOf(PageFor(href)!, role)).Should().NotBeNull($"{href} does not admit a {role}");
        RenderHomeFor(role).Hrefs.Should().NotContain(href);
    }

    // T261: a CollegeAdmin's dashboard is the national catalogue it authors, the CollegeAdmin's nav row, and nothing of
    // the trainee's.
    [Fact]
    public void ACollegeAdmin_SeesTheCatalogueItAuthors()
    {
        var home = RenderHomeFor(WombatRoles.CollegeAdmin);

        home.Dashboard.Should().Be<CollegeAdminDashboard>();
        home.Hrefs.Should().Equal("/admin/specialities", "/admin/epas", "/admin/curricula");
    }

    // A pending trainee is sent to the account page, and to nothing a Trainee alone may open.
    [Fact]
    public void APendingTrainee_IsSentToTheAccountPage()
    {
        RenderHomeFor(WombatRoles.PendingTrainee).Hrefs.Should().Equal("/account/profile");
    }

    // The T261 review: a user who holds no role (an administrator removed the last one, which RemoveRoleFromUser allows)
    // was shown the trainee's dashboard, "Viewing as Trainee", and its links to pages that refuse them. Home says they
    // hold none, and offers no link.
    [Fact]
    public void AUserWithNoRole_IsShownNoDashboardAndNoLink()
    {
        var home = RenderHome();

        home.Dashboards.Should().BeEmpty();
        home.Hrefs.Should().BeEmpty();
        home.Markup.Should().Contain("No role assigned").And.NotContain("Viewing as");
    }

    /// <summary>
    /// Home, rendered for a signed-in holder of the one role: which dashboard it shows, and every link on the page in
    /// document order.
    /// </summary>
    private static (Type Dashboard, List<string> Hrefs) RenderHomeFor(string role)
    {
        var home = RenderHome(role);
        home.Dashboards.Should().ContainSingle($"Home shows a {role} one dashboard");

        return (home.Dashboards[0], home.Hrefs);
    }

    /// <summary>
    /// Home, rendered for a signed-in user holding these roles (none for a user with no role): the dashboards it shows,
    /// every link on the page in document order, and its markup. Each render has a context of its own: a context's
    /// authorization cannot be replaced once it has rendered.
    /// </summary>
    private static (List<Type> Dashboards, List<string> Hrefs, string Markup) RenderHome(params string[] roles)
    {
        using var context = new TestContext();
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized(roles.Length == 0 ? "no.role@wombat.local" : $"{roles[0].ToLowerInvariant()}@wombat.local");
        auth.SetRoles(roles);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "user-1"));

        // No request, so no preferred-dashboard cookie: Home picks by DashboardPriority, as for a first visit.
        context.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());
        context.Services.AddSingleton<IScopedSender>(new DashboardSender());

        var cut = context.RenderComponent<Home>();
        cut.WaitForState(() => cut.FindAll(".dashboard-grid").Count > 0);

        var dashboards = DashboardTypes().Where(type => HasComponent(cut, type)).ToList();
        var hrefs = cut.FindAll("a[href]").Select(a => a.GetAttribute("href")!).ToList();
        return (dashboards, hrefs, cut.Markup);
    }

    private static List<Type> DashboardTypes()
        => typeof(TraineeDashboard).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(TraineeDashboard).Namespace && typeof(IComponent).IsAssignableFrom(type))
            .ToList();

    private static string DashboardsFolder()
        => Path.Combine(SolutionRoot(), "src", "Wombat.Web", "Components", "Pages", "Dashboards");

    private static bool HasComponent(IRenderedFragment cut, Type component)
        => (bool)typeof(RenderedFragmentExtensions)
            .GetMethod(nameof(RenderedFragmentExtensions.HasComponent))!
            .MakeGenericMethod(component)
            .Invoke(null, [cut])!;

    /// <summary>
    /// Every link a dashboard can draw, with the component whose source declares it: those in the dashboard's own source,
    /// and those in the source of every component it names, and of every component those name, down the tree.
    /// </summary>
    private static IEnumerable<(string DeclaredIn, string Declared)> DeclaredLinks(string dashboardSource)
        => Components(dashboardSource).SelectMany(path =>
            DeclaredHrefs(File.ReadAllText(path)).Select(href => (Path.GetFileNameWithoutExtension(path), href)));

    /// <summary>The source, and the source of every Wombat.Web component it names and those name, each once.</summary>
    private static List<string> Components(string source)
    {
        var byName = Directory
            .GetFiles(Path.Combine(SolutionRoot(), "src", "Wombat.Web", "Components"), "*.razor", SearchOption.AllDirectories)
            .ToLookup(path => Path.GetFileNameWithoutExtension(path), StringComparer.Ordinal);
        var found = new List<string> { source };
        for (var next = 0; next < found.Count; next++)
        {
            // A component's tag: <DashboardCard, or <Wombat.Web.Components.Pages.Dashboards.TraineeDashboard.
            foreach (Match tag in Regex.Matches(File.ReadAllText(found[next]), @"<((?:[A-Za-z_][A-Za-z0-9_]*\.)*[A-Z][A-Za-z0-9_]*)\b"))
            {
                var name = tag.Groups[1].Value.Split('.')[^1];
                found.AddRange(byName[name].Where(path => !found.Contains(path, StringComparer.OrdinalIgnoreCase)));
            }
        }

        return found;
    }

    /// <summary>
    /// Every href a Razor source declares, the &lt;a href&gt; and a DashboardCard's Href alike, but not an SVG
    /// &lt;use&gt;'s (Icon's), which is no link. One that only passes on the component's own parameter (DashboardCard's
    /// <c>href="@Href"</c>) is left out: the caller that sets the parameter declares its value, and its source is read too.
    /// </summary>
    private static IEnumerable<string> DeclaredHrefs(string razor)
        => Regex.Matches(razor, @"(?<!<(?:use|link|base)\b[^>]*)\bhref\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase)
            .Select(match => match.Groups[1].Value)
            .Where(href => !PassesOnAParameter(razor, href));

    private static bool PassesOnAParameter(string razor, string href)
    {
        var passed = Regex.Match(href, @"^@([A-Za-z_][A-Za-z0-9_]*)$");
        return passed.Success
            && Regex.IsMatch(razor, @"\[Parameter[^\]]*\]\s*public\s+\S+\s+" + passed.Groups[1].Value + @"\s*\{");
    }

    // "@item.ActivityId" or "@(expression)" is a value the render fills in: one path segment, whatever it holds.
    private static Regex PatternFor(string declared)
    {
        var literals = Regex.Split(declared, "@\\([^)]*\\)|@[A-Za-z_][A-Za-z0-9_.]*");
        return new Regex("^" + string.Join("[^/?#]+", literals.Select(Regex.Escape)) + "$");
    }

    /// <summary>
    /// Answers each dashboard's query with every list it shows filled, and the trainee's targets with one unmet, so every
    /// branch that shows a row is drawn and a link added to one is judged. The trainee's is pending as its handler decides
    /// (PendingTrainee without Trainee), and a pending trainee has no targets, as from the handler.
    /// </summary>
    private sealed class DashboardSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)Answer(request));

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        private static object Answer(object request) => request switch
        {
            GetAdministratorDashboardSummaryQuery => new AdministratorDashboardSummaryDto(DatabaseHealthy: true, TotalUserCount: 12),
            GetInstitutionalAdminDashboardSummaryQuery => new InstitutionalAdminDashboardSummaryDto(
                [new RoleCountItem(WombatRoles.Trainee, 3)], SpecialityCount: 1, SubSpecialityCount: 1),
            GetSpecialityAdminDashboardSummaryQuery => new SpecialityAdminDashboardSummaryDto(1, 3, 1, Coverage()),
            GetSubSpecialityAdminDashboardSummaryQuery => new SubSpecialityAdminDashboardSummaryDto(1, 3, 1, Coverage()),
            GetCommitteeMemberDashboardSummaryQuery => new CommitteeMemberDashboardSummaryDto(
                "Semester 1, 2026", "January to June", [new TraineeTargetsItem("trainee-1", "Thandi Nkosi", 1, 2, 0, 1)], [Epa()], 0),
            GetCoordinatorDashboardSummaryQuery => new CoordinatorDashboardSummaryDto(
                [new StalledRequestItem(51, "Mini-CEX", "Thandi Nkosi", When)],
                [new ExpiringInvitationItem(1, "new.trainee@wombat.local", WombatRoles.Trainee, new DateOnly(2026, 3, 22))]),
            GetAssessorDashboardSummaryQuery => new AssessorDashboardSummaryDto(
                2,
                [new AcceptedActivityItem(52, "Mini-CEX", "Thandi Nkosi", "Accepted", When, IsOverdue: false)],
                [new RecentDecisionItem(53, "Mini-CEX", "Thandi Nkosi", "completed", "Completed", When)]),
            GetTraineeDashboardSummaryQuery query => Trainee(
                pending: query.Principal.IsInRole(WombatRoles.PendingTrainee) && !query.Principal.IsInRole(WombatRoles.Trainee)),
            _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
        };

        private static TraineeDashboardSummaryDto Trainee(bool pending)
            => new(
                pending ? null : Targets(),
                [new ActivityInboxItem(54, "Mini-CEX", "draft", "Draft", When)],
                [new RecentActivityItem(55, "Mini-CEX", "submitted", "Submitted", When)],
                [new UpcomingDeadlineItem(56, "Mini-CEX", "Due", new DateOnly(2026, 3, 27))],
                IsPendingTrainee: pending);

        // One semester item that applies and is not met: a row of the trainee's targets card.
        private static TraineeCurriculumProgressSummaryDto Targets()
        {
            var current = new QuotaWindowDto(
                "Semester 1, 2026", "January to June", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), QuotaWindowStatus.Counting,
                Count: 1, Target: 2, IsMet: false, Shortfall: 1, PercentOfTarget: 50, MinimumLevelReachedCount: 1,
                LastObservedOn: new DateOnly(2026, 3, 10), LastObservedOnDeclared: true, FirstCountedName: null, FirstCountedOn: null);

            return new TraineeCurriculumProgressSummaryDto(
                AsOf: new DateOnly(2026, 3, 20), ProgrammeStartDate: new DateOnly(2025, 1, 1), TraineeStage: 2,
                CurrentSemesterName: "Semester 1, 2026", CurrentSemesterMonths: "January to June",
                CurrentSemesterNominalEnd: new DateOnly(2026, 6, 30), IsAfterTeachingYear: false,
                SemesterTargetsMet: 0, SemesterTargetsApplying: 1, YearTargetsMet: 0, YearTargetsApplying: 0,
                HasSemesterItems: true, HasYearItems: false, ProgrammeNotStarted: false,
                SemesterTargetsStart: null, YearTargetsStart: null,
                Items: [new TraineeCurriculumProgressDto(1, 1, "EPA 1", "Resuscitate a newborn", QuotaPeriod.Semester, 2, current, null, 3, "3a", null)]);
        }

        private static CurriculumCoverage Coverage()
            => new(
                new DateOnly(2026, 3, 20), "Semester 1, 2026", "January to June",
                [new TraineeTargetCoverage("trainee-1", 0, 1, 0, 0)], [Epa()], 0);

        // One EPA's target that applies to a trainee who has not met it: a row, with its bar.
        private static EpaTargetCoverage Epa()
            => new(1, "EPA 1", "Resuscitate a newborn", QuotaPeriod.Semester, 2, TraineesMet: 0, TraineesApplying: 1, TraineesExempt: 0);
    }
}
