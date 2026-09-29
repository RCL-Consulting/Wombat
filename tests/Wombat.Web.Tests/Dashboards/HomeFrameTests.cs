using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.CommitteeMember;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.InstitutionalAdmin;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.Navigation;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T335, flow 01 (R2-Landing-*, R2-Shell-*, R2-Phone-Folded; the round-2 review, S20 and S22(c)) and T329's Home half:
/// Home's frame. It is headed "Home", with the acting role and the semester under it and the role's one header action;
/// the frame and the cards' titles draw at once, a skeleton in each card while the read runs; and a failed read shows one
/// alert with Try again, and no cards.
/// </summary>
/// <remarks>
/// Until T335 Home was headed "Welcome, {email}", "Viewing as {role key}" under it, with a "You also act as … Switch view"
/// line; while a dashboard read, the page showed a column of bare skeletons, and a failed read printed the exception's
/// message.
/// </remarks>
public sealed class HomeFrameTests : WombatTestContext
{
    /// <summary>22:30 UTC on 30 June 2026 is already 1 July in South Africa: semester 2.</summary>
    private static readonly DateTimeOffset LateOnTheLastDayOfSemesterOne = new(2026, 6, 30, 22, 30, 0, TimeSpan.Zero);

    private static readonly DateTime When = new(2026, 9, 18, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>What a failed read says: the log's words, never the page's.</summary>
    private const string FailureText = "Npgsql could not reach the database (a fake failure).";

    public static TheoryData<string> EveryRoleThatReads()
        => new(WombatRoles.All.Where(role => role != WombatRoles.CollegeAdmin));

    // ---- the subtitle's semester: a pure function of the South African date ----

    [Theory]
    [InlineData(WombatRoles.Assessor, "2026-09-26", "Assessor · Semester 2, 2026")]
    [InlineData(WombatRoles.CommitteeMember, "2026-06-30", "Committee member · Semester 1, 2026")]
    [InlineData(WombatRoles.Trainee, "2026-07-01", "Trainee · Semester 2, 2026")]
    [InlineData(WombatRoles.SubSpecialityAdmin, "2026-12-31", "Sub-speciality admin · Semester 2, 2026")]
    [InlineData(WombatRoles.PendingTrainee, "2027-01-01", "Pending trainee · Semester 1, 2027")]
    public void TheSubtitle_IsTheActingRolesLabel_AndTheSemesterOfTheDay(string role, string today, string expected)
        => HomeFrame.Subtitle(role, DateOnly.Parse(today, System.Globalization.CultureInfo.InvariantCulture)).Should().Be(expected);

    [Fact]
    public void WithNoRole_ThereIsNoSubtitle()
        => HomeFrame.Subtitle(null, new DateOnly(2026, 9, 26)).Should().BeNull("D8: no role, nothing to name");

    // ---- the header ----

    [Fact]
    public void Home_IsHeadedHome_AndTitledSo_WithTheSubtitleOfTheSouthAfricanDay()
    {
        var cut = RenderHome(Reads.Answer, WombatRoles.CommitteeMember, WombatRoles.Assessor);

        cut.FindAll("h1").Should().ContainSingle().Which.TextContent.Should().Be("Home");
        TabTitle.Of(this, cut).Should().Be("Home · Wombat");
        cut.Find(".page-subtitle").TextContent.Should().Be("Committee member · Semester 2, 2026",
            "22:30 UTC on 30 June is 1 July on the South African calendar");
        cut.Markup.Should().NotContainAny(["Welcome", "Viewing as", "You also act as", "Switch view"],
            "the top bar names the person, the sidebar's head the role, and the sidebar offers the switch");
    }

    [Theory]
    [InlineData(WombatRoles.Trainee, "Log an activity", "/activities/new")]
    [InlineData(WombatRoles.InstitutionalAdmin, "Invite a person", "/admin/invitations")]
    public void TheHeader_OffersTheRolesOneAction(string role, string label, string href)
    {
        var cut = RenderHome(Reads.Answer, role);

        var action = cut.Find(".header-container .actions-cell").QuerySelectorAll("a, button").Should().ContainSingle().Which;
        action.TextContent.Trim().Should().Be(label);
        action.GetAttribute("href").Should().Be(href);
        action.ClassList.Should().Contain(["btn", "btn-primary"]);
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.CollegeAdmin)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.PendingTrainee)]
    public void EveryOtherRole_HasNoHeaderAction(string role)
    {
        var cut = RenderHome(Reads.Answer, role);

        cut.FindAll(".header-container .actions-cell").Should().BeEmpty("A-Spec § e: the role's main job is a card's row or a nav link");
    }

    // The header action is a link Home offers, so it opens a page that admits the role (T261's rule, as for the cards).
    [Theory]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public async Task TheHeaderAction_OpensAPageThatAdmitsTheRole(string role)
    {
        var href = HomeFrame.ActionFor(role)!.Href;

        var page = PageAccess.PageFor(href);
        page.Should().NotBeNull();
        (await PageAccess.RefusalOf(page!, role)).Should().BeNull();
    }

    // ---- loading: the frame at once, a skeleton in each card, nothing to act on ----

    [Fact]
    public void WhileTheReadRuns_TheFrameAndTheCardTitlesAreDrawn_EachCardASkeleton_AndNothingIsOffered()
    {
        var cut = RenderHome(Reads.Hang, WombatRoles.CommitteeMember);

        cut.Find("h1").TextContent.Should().Be("Home");
        cut.Find(".page-subtitle").TextContent.Should().StartWith("Committee member · Semester");
        var grid = cut.Find(".dashboard-grid");
        grid.GetAttribute("aria-busy").Should().Be("true");
        CardTitles(cut).Should().Equal("Targets this period", "Targets met by EPA");
        cut.FindAll(".dashboard-grid .detail-card").Should().OnlyContain(card => card.QuerySelectorAll(".skeleton").Length > 0);
        grid.QuerySelectorAll("a, button").Should().BeEmpty("no action is offered before the read returns");
    }

    [Fact]
    public void WhileTheReadRuns_ACardThatLinks_IsNoLinkYet()
    {
        var cut = RenderHome(Reads.Hang, WombatRoles.Trainee);

        CardTitles(cut).Should().StartWith(["Curriculum targets", "Needs you"]);
        cut.FindAll(".dashboard-grid a").Should().BeEmpty();
        cut.FindAll(".detail-card--interactive").Should().BeEmpty();
    }

    // CoordinatorDashboard's stripe and the admins' coverage title read the summary as the card was drawn, safe only while
    // no card rendered before the read returned (the review, S22(c)). Every dashboard now draws its cards first.
    [Theory]
    [MemberData(nameof(EveryRoleThatReads))]
    public void EveryDashboard_DrawsItsFrame_BeforeItsReadReturns(string role)
    {
        var cut = RenderHome(Reads.Hang, role);

        cut.Find(".dashboard-grid").GetAttribute("aria-busy").Should().Be("true");
        CardTitles(cut).Should().NotBeEmpty();
        cut.FindAll(".dashboard-grid a[href], .dashboard-grid button").Should().BeEmpty();
    }

    // ---- a failed read: one alert, Try again, no cards (T329) ----

    [Theory]
    [MemberData(nameof(EveryRoleThatReads))]
    public void AFailedRead_ShowsOneAlert_WithTryAgain_AndNoCards(string role)
    {
        var cut = RenderHome(Reads.Throw, role);

        var alert = cut.FindAll(".alert").Should().ContainSingle().Which;
        alert.ClassList.Should().Contain("alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.");
        alert.QuerySelector("button")!.TextContent.Trim().Should().Be("Try again");
        cut.FindAll(".dashboard-grid, .detail-card").Should().BeEmpty("a card drawn empty would say there is nothing");
        cut.Markup.Should().NotContain(FailureText, "the failure's own words are the log's, not the page's");
        cut.Find("h1").TextContent.Should().Be("Home", "the frame stays");
    }

    [Fact]
    public void TryAgain_ReadsAgain_AndItsAnswerTakesTheFocus()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var sender = new Sender(Reads.Throw);
        var cut = RenderHome(sender, WombatRoles.Assessor);
        cut.Find(".alert-danger");

        sender.Reads = Reads.Answer;
        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => CardTitles(cut).Should().Equal("Waiting for your rating", "Recent decisions"));
        cut.FindAll(".alert-danger").Should().BeEmpty();
        sender.Count.Should().Be(2);
        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.FindComponent<DashboardFrame>().FindComponent<ActionResult>().Instance.Element.Id,
                "the button went with the alert, so the answer takes the focus (T234)");
    }

    // ---- no role ----

    [Fact]
    public void WithNoRole_NoSubtitle_AndNoRoleAssigned_WithNoLink()
    {
        var cut = RenderHome(Reads.Answer);

        cut.Find("h1").TextContent.Should().Be("Home");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        CardTitles(cut).Should().Equal("No role assigned");
        cut.FindAll("article a, .dashboard-grid a").Should().BeEmpty();
    }

    // A-Spec § e: a graduate holds no role for the programme they finished (T252), and Home points to the record they keep.
    [Fact]
    public async Task WithNoRole_AndATrainingRecord_HomePointsToMyProgress_WhichAdmitsThem()
    {
        var cut = RenderHome(Reads.Answer, [], new Claim(WombatClaimTypes.TraineeRecord, "true"));

        cut.FindAll(".page-subtitle").Should().BeEmpty();
        CardTitles(cut).Should().Equal("Your training record");
        var link = cut.Find(".dashboard-grid a");
        link.GetAttribute("href").Should().Be("/portfolio/progress");

        var graduate = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "graduate-1"), new Claim(WombatClaimTypes.TraineeRecord, "true")], "Test"));
        (await PageAccess.RefusalOf(PageAccess.PageFor("/portfolio/progress")!, graduate)).Should().BeNull();
    }

    // ---- rendering ----

    private IRenderedComponent<Home> RenderHome(Reads reads, params string[] roles)
        => RenderHome(new Sender(reads), roles, []);

    private IRenderedComponent<Home> RenderHome(Reads reads, string[] roles, params Claim[] claims)
        => RenderHome(new Sender(reads), roles, claims);

    private IRenderedComponent<Home> RenderHome(Sender sender, params string[] roles)
        => RenderHome(sender, roles, []);

    private IRenderedComponent<Home> RenderHome(Sender sender, string[] roles, Claim[] claims)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("zulu@kgk.test");
        auth.SetRoles(roles);
        auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, "zulu"), .. claims]);
        Services.AddSingleton<IScopedSender>(sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(LateOnTheLastDayOfSemesterOne));

        return RenderComponent<Home>(parameters => parameters
            .AddCascadingValue(ActingRoleResolver.Resolve(stored: null, roles)));
    }

    private static List<string> CardTitles(IRenderedFragment cut)
        => cut.FindAll(".detail-card .dashboard-card-title > span:not(.badge)").Select(title => title.TextContent.Trim()).ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public enum Reads
    {
        Answer,
        Hang,
        Throw
    }

    /// <summary>Answers every dashboard's query, or never answers, or fails, as the test sets it; counts the reads.</summary>
    private sealed class Sender(Reads reads) : IScopedSender
    {
        public Reads Reads { get; set; } = reads;

        public int Count { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Count++;
            return Reads switch
            {
                Reads.Hang => new TaskCompletionSource<TResponse>().Task,
                Reads.Throw => Task.FromException<TResponse>(new InvalidOperationException(FailureText)),
                _ => Task.FromResult((TResponse)Answer(request))
            };
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static object Answer(object request) => request switch
        {
            GetAdministratorDashboardSummaryQuery => new AdministratorDashboardSummaryDto(DatabaseHealthy: true, TotalUserCount: 12),
            GetInstitutionalAdminDashboardSummaryQuery => new InstitutionalAdminDashboardSummaryDto([], SpecialityCount: 1, SubSpecialityCount: 1),
            GetSpecialityAdminDashboardSummaryQuery => new SpecialityAdminDashboardSummaryDto(1, 3, 1, Coverage()),
            GetSubSpecialityAdminDashboardSummaryQuery => new SubSpecialityAdminDashboardSummaryDto(1, 3, 1, Coverage()),
            GetCommitteeMemberDashboardSummaryQuery => new CommitteeMemberDashboardSummaryDto("Semester 2, 2026", "July to November", [], [], 0),
            GetCoordinatorDashboardSummaryQuery => new CoordinatorDashboardSummaryDto([], []),
            GetAssessorDashboardSummaryQuery => new AssessorDashboardSummaryDto(
                1,
                [new AwaitingReviewItem(52, "Mini-CEX", "Nomsa Mahlangu", "requested", "Requested", When, IsOverdue: false)],
                []),
            GetTraineeDashboardSummaryQuery query => new TraineeDashboardSummaryDto(
                null, [], [], [],
                IsPendingTrainee: query.Principal.IsInRole(WombatRoles.PendingTrainee) && !query.Principal.IsInRole(WombatRoles.Trainee)),
            _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
        };

        private static CurriculumCoverage Coverage()
            => new(new DateOnly(2026, 9, 23), "Semester 2, 2026", "July to November", [], [], 0);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
