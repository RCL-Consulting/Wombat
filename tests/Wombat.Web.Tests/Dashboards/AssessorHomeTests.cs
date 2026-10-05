using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListWaitingForYou;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.CommitteeMember;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T350, flow 04 (R3-C-Home, every R3-H-* state; DESIGN.md § Dashboard page, R1): the Assessor's Home is two cards, each
/// the Activity inbox's own read. "Waiting for you" lists the first five rows in the inbox's words, its count in words as
/// its badge, the rule line, the overflow past five and "Open Activity inbox"; "Recent decisions" lists five, dated, and
/// "All your decisions". And the other-role line (note 7, E4, C12): someone who holds Assessor, acting in another role,
/// is told on Home what waits for them in the inbox, with no switch.
/// </summary>
public sealed class AssessorHomeTests : TestContext
{
    /// <summary>The replay's day, 30 September 2026, 10:00 SAST.</summary>
    private static readonly DateTimeOffset Today = new(2026, 9, 30, 8, 0, 0, TimeSpan.Zero);

    private static readonly DateTime EightFive = new(2026, 9, 30, 6, 5, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    public AssessorHomeTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("patel@test");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "patel"));
        Services.AddSingleton<TimeProvider>(new FixedClock(Today));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- the Assessor's cards ----

    // R3-H-empty (Dr Patel, Step 2.36): no badge, "Nothing is waiting for you.", "No decisions yet.", and no "All your
    // decisions" with none to show.
    [Fact]
    public void Empty_SaysNothingWaits_AndNoDecisionsYet()
    {
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome([], [])), WombatRoles.Assessor);

        Titles(cut).Should().Equal("Waiting for you", "Recent decisions");
        var waiting = Card(cut, "card-waiting");
        waiting.QuerySelector("h2 .badge").Should().BeNull("no \"0\" badge (R1)");
        waiting.QuerySelector(".card-empty")!.TextContent.Should().Be("Nothing is waiting for you.");
        waiting.QuerySelectorAll("li").Should().BeEmpty();
        Footer(waiting).Should().Equal(("Open Activity inbox", "/activities/inbox"));
        waiting.ClassList.Should().Contain("detail-card--emphasis").And.NotContain("detail-card--warning");

        var decisions = Card(cut, "card-decisions");
        decisions.QuerySelector(".card-empty")!.TextContent.Should().Be("No decisions yet.");
        decisions.QuerySelectorAll(".dashboard-card-footer a").Should().BeEmpty();
        cut.Markup.Should().NotContain("Waiting for your rating").And.NotContain("Open inbox");
    }

    // R3-H-one-waiting (Dr Khumalo): the row as the inbox's, the count in words, the rule line.
    [Fact]
    public void OneWaiting_IsTheInboxsRow_CountedInWords()
    {
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome(
            [ActivityRows.Waiting(30, typeName: "Case-Based Discussion (Paediatrics)", subjectName: "Pieter du Plessis",
                since: EightFive, epaCode: "PAED-002", observedOn: new DateOnly(2026, 9, 25))],
            [ActivityRows.Decided(31, subjectName: "Sipho Ndlovu", state: "declined", stateLabel: "Declined", isFinished: false,
                decidedOn: new DateTime(2026, 9, 30, 6, 1, 0, DateTimeKind.Utc), creditedItemCount: null)])), WombatRoles.Assessor);

        var waiting = Card(cut, "card-waiting");
        waiting.TagName.Should().Be("SECTION");
        waiting.GetAttribute("aria-labelledby").Should().Be("card-waiting");
        waiting.ClassList.Should().Contain("dashboard-span-2");
        var badge = waiting.QuerySelector("h2 .badge")!;
        badge.TextContent.Should().Be("1 waiting");
        badge.HasAttribute("aria-hidden").Should().BeFalse("the words are the badge, to the eye and to a screen reader");
        waiting.QuerySelector(".needs-you-rule")!.TextContent.Should().Be("Oldest first. Overdue once it has waited 7 days.");
        var row = waiting.QuerySelector("li.needs-you-row")!;
        row.QuerySelector("a.activity-link")!.TextContent.Should().Be(
            "Case-Based Discussion (Paediatrics) · PAED-002 · 2026-09-25, from Pieter du Plessis");
        row.QuerySelector(".needs-you-why")!.TextContent.Should().Be("Waiting less than a day");
        waiting.QuerySelector(".waiting-more").Should().BeNull();

        var decision = Card(cut, "card-decisions").QuerySelector("li.decided-row")!;
        decision.QuerySelector("a.activity-link")!.GetAttribute("href").Should().Be("/activities/31");
        decision.QuerySelector(".decided-meta .badge")!.ClassList.Should().Contain("badge-declined");
        decision.QuerySelector(".decided-meta .badge")!.TextContent.Should().Be("Declined");
        decision.QuerySelector(".decided-date")!.TextContent.Should().Be("2026-09-30");
        Footer(Card(cut, "card-decisions")).Should().Equal(("All your decisions", "/activities/inbox#decided-h"));
    }

    // R3-H-several-overdue (Dr Patel): the warning stripe in place of the emphasis one (nit T15), Overdue beside the state.
    [Fact]
    public void SeveralWithOneOverdue_TheCardTakesTheWarningStripe_AndTheCountSaysSo()
    {
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome(
            [
                ActivityRows.Waiting(10, subjectName: "Pieter du Plessis", state: "submitted", stateLabel: "Awaiting review",
                    waitedDays: 8, overdue: true, since: new DateTime(2026, 9, 22, 6, 6, 0, DateTimeKind.Utc)),
                ActivityRows.Waiting(11, since: EightFive)
            ],
            [])), WombatRoles.Assessor);

        var waiting = Card(cut, "card-waiting");
        waiting.QuerySelector("h2 .badge")!.TextContent.Should().Be("2 waiting, 1 overdue");
        waiting.ClassList.Should().Contain("detail-card--warning").And.NotContain("detail-card--emphasis");
        var rows = waiting.QuerySelectorAll("li").ToList();
        rows[0].ClassList.Should().Contain("needs-you-row--overdue");
        rows[0].QuerySelectorAll(".badge").Select(badge => badge.TextContent).Should().Equal("Awaiting review", "Overdue");
        rows[0].QuerySelector(".needs-you-why")!.TextContent.Should().Be("Waiting 8 days");
        rows[1].ClassList.Should().NotContain("needs-you-row--overdue");
    }

    // R3-H-overdue-only.
    [Fact]
    public void OverdueOnly_OneRowOneOverdue()
    {
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome(
            [ActivityRows.Waiting(10, waitedDays: 8, overdue: true)], [])), WombatRoles.Assessor);

        Card(cut, "card-waiting").QuerySelector("h2 .badge")!.TextContent.Should().Be("1 waiting, 1 overdue");
    }

    // R3-H-twenty-five (E5): five rows, the inbox's first five in its order, and the rest said in words above the foot.
    [Fact]
    public void TwentyFive_FiveRows_AndTheOverflowLine()
    {
        var rows = Enumerable.Range(1, 25).Select(id => ActivityRows.Waiting(id, waitedDays: 33 - id, overdue: true,
            observedOn: new DateOnly(2026, 8, 1).AddDays(id))).ToList();
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome(rows, [])), WombatRoles.Assessor);

        var waiting = Card(cut, "card-waiting");
        waiting.QuerySelectorAll("li a").Select(link => link.GetAttribute("href")).Should().Equal(
            "/activities/1", "/activities/2", "/activities/3", "/activities/4", "/activities/5");
        waiting.QuerySelector("h2 .badge")!.TextContent.Should().Be("25 waiting, 25 overdue");
        var more = waiting.QuerySelector(".waiting-more")!;
        more.TextContent.Should().Be("20 more wait in the Activity inbox.");
        more.NextElementSibling!.ClassList.Should().Contain("dashboard-card-footer", "the overflow line stands above the foot");
    }

    [Fact]
    public void Six_OneMoreWaits()
    {
        var rows = Enumerable.Range(1, 6).Select(id => ActivityRows.Waiting(id, observedOn: new DateOnly(2026, 9, id))).ToList();
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome(rows, [])), WombatRoles.Assessor);

        cut.Find(".waiting-more").TextContent.Should().Be("1 more waits in the Activity inbox.");
    }

    // R3-H-loading (C11): the status says so, present before its words; the cards are their titles and skeletons.
    [Fact]
    public void Loading_TheStatusSaysSo_AndTheCardsAreTheirTitles()
    {
        var cut = RenderHome(new Sender(null) { Hang = true }, WombatRoles.Assessor, wait: false);

        var status = cut.Find("p[role='status']");
        status.ClassList.Should().Contain("visually-hidden");
        status.TextContent.Should().Be("Loading your Home.");
        Titles(cut).Should().Equal("Waiting for you", "Recent decisions");
        cut.FindAll(".dashboard-grid a, .dashboard-grid .badge").Should().BeEmpty();
    }

    [Fact]
    public void Loaded_TheStatusStays_Empty()
    {
        var cut = RenderHome(new Sender(ActivityRows.AssessorHome([], [])), WombatRoles.Assessor);

        cut.Find("p[role='status']").TextContent.Should().BeEmpty();
    }

    // R3-H-load-error: the frame's words, Try again, no cards.
    [Fact]
    public void ALoadError_IsTheFramesAlert_WithNoCards()
    {
        var cut = RenderHome(new Sender(null) { Fail = true }, WombatRoles.Assessor, wait: false);

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Contain("Could not load your Home."));
        cut.FindAll(".detail-card").Should().BeEmpty();
        cut.Markup.Should().NotContain("fake failure");
    }

    // R3-H-switched: Dr Zulu, acting as Assessor, has her cards and no other-role line.
    [Fact]
    public void AnAssessorActingAsAssessor_HasTheCards_AndNoLine()
    {
        var sender = new Sender(ActivityRows.AssessorHome([ActivityRows.Waiting(10, waitedDays: 8, overdue: true)], []));
        var cut = RenderHome(sender, WombatRoles.Assessor, WombatRoles.CommitteeMember, WombatRoles.Assessor);

        Titles(cut).Should().Equal("Waiting for you", "Recent decisions");
        cut.FindAll(".other-role-line").Should().BeEmpty("her Home already holds the inbox's card");
        sender.WaitingReads.Should().Be(0, "the line does not read what the card already has");
    }

    // ---- the other-role line (note 7; E4; C12) ----

    // R3-H-committee-line: one, overdue; the warning tint, "Open it", and the link opens the activity with no switch.
    [Fact]
    public void TheLine_OneOverdue()
    {
        var zulu = ActivityRows.Waiting(60, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true,
            epaCode: "PAED-004", observedOn: new DateOnly(2026, 9, 27));
        var cut = RenderCommitteeHome([zulu]);

        var line = cut.Find("section.other-role-line");
        line.ClassList.Should().Contain(["alert", "alert-warning"]);
        line.GetAttribute("aria-label").Should().Be("Waiting for you in the Activity inbox");
        line.HasAttribute("role").Should().BeFalse("it arrives with the page: not a live region");
        line.QuerySelector(".alert-row-text")!.TextContent.Should().Be(
            "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics) · PAED-004 · 2026-09-27, from Nomsa Mahlangu, waiting 8 days.");
        var open = line.QuerySelector("a")!;
        open.TextContent.Should().Be("Open it");
        open.GetAttribute("href").Should().Be("/activities/60");
        open.GetAttribute("aria-label").Should().Be("Open it: Mini-CEX (Paediatrics) · PAED-004 · 2026-09-27, from Nomsa Mahlangu");
        line.QuerySelectorAll("a[href^='/dashboard/switch']").Should().BeEmpty("the line holds no switch");
    }

    // R3-H-committee-line-several: the oldest named, how many are overdue.
    [Fact]
    public void TheLine_Several()
    {
        var cut = RenderCommitteeHome(
        [
            ActivityRows.Waiting(60, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true, epaCode: "PAED-004",
                observedOn: new DateOnly(2026, 9, 27)),
            ActivityRows.Waiting(61),
            ActivityRows.Waiting(62)
        ]);

        var line = cut.Find("section.other-role-line");
        line.ClassList.Should().Contain("alert-warning");
        line.QuerySelector(".alert-row-text")!.TextContent.Should().Be(
            "3 activities wait for you in the Activity inbox; 1 is overdue. The oldest: Mini-CEX (Paediatrics) · PAED-004 · 2026-09-27, from Nomsa Mahlangu, waiting 8 days.");
        line.QuerySelector("a")!.TextContent.Should().Be("Open the oldest");
        line.QuerySelector("a")!.GetAttribute("href").Should().Be("/activities/60");
    }

    // R3-H-committee-line-none-overdue: the info tint.
    [Fact]
    public void TheLine_NoneOverdue_IsInfo()
    {
        var cut = RenderCommitteeHome([ActivityRows.Waiting(63, observedOn: new DateOnly(2026, 9, 30), epaCode: "PAED-002")]);

        var line = cut.Find("section.other-role-line");
        line.ClassList.Should().Contain("alert-info").And.NotContain("alert-warning");
        line.QuerySelector(".alert-row-text")!.TextContent.Should().Be(
            "1 activity waits for you in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-30, from Anele Dlamini, waiting less than a day.");
    }

    [Fact]
    public void TheLine_NothingWaiting_IsNoLine()
    {
        var cut = RenderCommitteeHome([]);

        cut.FindAll(".other-role-line").Should().BeEmpty();
    }

    // C12: a failed read shows nothing and is logged; it never becomes a Home error, which the committee's frame owns.
    [Fact]
    public void TheLine_AFailedRead_IsNoLine_AndIsLogged_NeverAHomeError()
    {
        var logger = new CapturingLogger<OtherRoleLine>();
        Services.AddSingleton<ILogger<OtherRoleLine>>(logger);
        var sender = new Sender(null) { FailWaiting = true };

        var cut = RenderHome(sender, WombatRoles.CommitteeMember, WombatRoles.CommitteeMember, WombatRoles.Assessor);

        cut.FindAll(".other-role-line").Should().BeEmpty();
        cut.FindAll(".alert-danger").Should().BeEmpty("the committee's Home read, and the line's failure is not its");
        logger.Entries.Should().ContainSingle().Which.Level.Should().Be(LogLevel.Error);
        cut.Markup.Should().NotContain("fake failure");
    }

    // The line is flow 04's words in the slot Home gives it: above the dashboard, below the header.
    [Fact]
    public void TheLine_StandsBetweenTheHeaderAndTheDashboard()
    {
        var cut = RenderCommitteeHome([ActivityRows.Waiting(63)]);

        var markup = cut.Markup;
        var line = markup.IndexOf("other-role-line", StringComparison.Ordinal);
        line.Should().BeGreaterThan(markup.IndexOf("header-container", StringComparison.Ordinal));
        line.Should().BeLessThan(markup.IndexOf("dashboard-grid", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Trainee)]
    public void NoLine_ForSomeoneWhoDoesNotHoldAssessor(string role)
    {
        var sender = new Sender(null);
        // The dashboard's own read is not this test's: it fails, and the line decides before any read of its own.
        var cut = RenderHome(sender, role, wait: false, role);

        cut.FindAll(".other-role-line").Should().BeEmpty();
        sender.WaitingReads.Should().Be(0);
    }

    // ---- rendering ----

    private IRenderedComponent<Home> RenderCommitteeHome(IReadOnlyList<ActivitySummaryDto> waiting)
        => RenderHome(new Sender(null, waiting), WombatRoles.CommitteeMember, WombatRoles.CommitteeMember, WombatRoles.Assessor);

    private IRenderedComponent<Home> RenderHome(Sender sender, string acting, params string[] roles)
        => RenderHome(sender, acting, wait: true, roles.Length == 0 ? [acting] : roles);

    private IRenderedComponent<Home> RenderHome(Sender sender, string acting, bool wait, params string[] roles)
    {
        if (roles.Length == 0)
        {
            roles = [acting];
        }

        _auth.SetRoles(roles);
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<Home>(parameters => parameters.AddCascadingValue(ActingRoleResolver.Resolve(acting, roles)));
        if (wait)
        {
            cut.WaitForState(() => cut.FindAll(".dashboard-grid").Count > 0 && cut.FindAll(".dashboard-grid[aria-busy]").Count == 0);
        }

        return cut;
    }

    private static IElement Card(IRenderedFragment cut, string headingId)
        => cut.Find($"#{headingId}").ParentElement!;

    private static List<string> Titles(IRenderedFragment cut)
        => cut.FindAll(".detail-card .dashboard-card-title > span:not(.badge)").Select(title => title.TextContent.Trim()).ToList();

    private static List<(string Label, string? Href)> Footer(IElement card)
        => card.QuerySelectorAll(".dashboard-card-footer a").Select(link => (link.TextContent.Trim(), link.GetAttribute("href"))).ToList();

    private sealed class Sender(AssessorDashboardSummaryDto? assessor, IReadOnlyList<ActivitySummaryDto>? waiting = null) : IScopedSender
    {
        public bool Hang { get; init; }

        public bool Fail { get; init; }

        public bool FailWaiting { get; init; }

        public int WaitingReads { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (Hang)
            {
                return new TaskCompletionSource<TResponse>().Task;
            }

            if (Fail)
            {
                return Task.FromException<TResponse>(new InvalidOperationException("a fake failure"));
            }

            object answer = request switch
            {
                GetAssessorDashboardSummaryQuery => assessor ?? throw new NotSupportedException("no assessor summary"),
                GetCommitteeMemberDashboardSummaryQuery => OversightHomeFixtures.Committee(),
                ListWaitingForYouQuery => Waiting(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
            return Task.FromResult((TResponse)answer);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private WaitingForYouDto Waiting()
        {
            WaitingReads++;
            if (FailWaiting)
            {
                throw new InvalidOperationException("a fake failure");
            }

            var rows = waiting ?? [];
            return new WaitingForYouDto(rows, rows.Count(row => row.IsOverdue), 7);
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, exception, formatter(state, exception)));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
