using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListDecidedByYou;
using Wombat.Application.Features.Activities.Queries.ListWaitingForYou;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T350, flow 04 (R3-C-Inbox, every R3-I-* state; DESIGN.md § List page, R2; round 1, Q1): the Activity inbox is two
/// headed, counted sections. "Waiting for you" is every row of the waiting read Home and the way on share, oldest first,
/// each its link "from &lt;registrar&gt;", its EPA, its state with Overdue beside it and how long it has waited, in South
/// African time; "Decided by you" is the decided read, newest first, 20 a page, with a pager whose ends are aria-disabled
/// and whose page turn puts the focus on the section's heading (C10 f).
/// </summary>
/// <remarks>
/// Until T350 the inbox was one table (Type, Subject, EPA, Encounter date, State, Updated, an Open button), newest first,
/// its Updated column in the server's zone, under "Work waiting for you to rate, review or record.". The registrar's empty
/// state (T342) moved here from MyActivitiesTests, and the EPA marker (T231) from ActivityListColumnsTests.
/// </remarks>
public sealed class ActivityInboxTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    /// <summary>A Mini-CEX waiting since 08:12 SAST on 30 September.</summary>
    private static readonly DateTime EightTwelve = new(2026, 9, 30, 6, 12, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    public ActivityInboxTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("patel@test");
        _auth.SetRoles(WombatRoles.Assessor);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "patel"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- the frame ----

    [Fact]
    public void TheInbox_SaysWhatItIsFor()
    {
        var cut = Render(new Sender());

        cut.Find("h1").TextContent.Should().Be("Activity inbox");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("What waits for you to rate, review or discuss.");
        cut.Markup.Should().NotContain("Work waiting for you to rate, review or record.");
    }

    // R3-I-clear (Dr Patel, Step 3.4): both sections, each saying it is empty, with no "0" badge.
    [Fact]
    public void Clear_BothSectionsSaySo_WithNoBadge()
    {
        var cut = Render(new Sender());

        Headings(cut).Should().Equal("Waiting for you", "Decided by you");
        cut.FindAll("h2 .badge").Should().BeEmpty("a count of nothing is no badge");
        var waiting = cut.Find("section[aria-labelledby='waiting-h']");
        waiting.QuerySelector(".state-panel-title")!.TextContent.Should().Be("Inbox clear");
        waiting.QuerySelector(".state-panel-copy")!.TextContent.Should().Be("Nothing is waiting for you.");
        cut.Find("section[aria-labelledby='decided-h'] .state-panel-title").TextContent.Should().Be("No decisions yet.");
        cut.FindAll("table, .pager").Should().BeEmpty();
    }

    // R3-I-clear-with-decisions (Dr Khumalo): nothing waits, one decision.
    [Fact]
    public void ClearWithDecisions_SaysNothingWaits_AndListsTheDecision()
    {
        var cut = Render(new Sender(decided: [ActivityRows.Decided(
            7, subjectName: "Sipho Ndlovu", state: "declined", stateLabel: "Declined", isFinished: false,
            decidedOn: new DateTime(2026, 9, 30, 6, 1, 0, DateTimeKind.Utc), creditedItemCount: null)]));

        cut.Find("section[aria-labelledby='waiting-h'] .state-panel-title").TextContent.Should().Be("Inbox clear");
        var decided = cut.Find("section[aria-labelledby='decided-h']");
        Text(decided.QuerySelector("h2")!).Should().Be("Decided by you 1 decision");
        decided.QuerySelector("h2 .badge")!.ClassList.Should().Contain("badge-draft");
        decided.QuerySelector(".needs-you-rule")!.TextContent.Should().Be(
            "Newest first. Everything you completed, declined, discussed or signed off.");
        Headers(decided).Should().Equal("Activity", "Decision", "Decided", "Credit");
        var cells = decided.QuerySelectorAll("tbody td").ToList();
        cells[0].QuerySelector("a.activity-link")!.TextContent.Should().Be("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Sipho Ndlovu");
        cells[1].QuerySelector(".badge")!.ClassList.Should().Contain("badge-declined");
        Text(cells[1]).Should().Be("Declined");
        Text(cells[2]).Should().Be("2026-09-30 08:01 SAST", "the decision's moment is South African, with its zone (note 8)");
        Text(cells[3]).Should().Be("—");
        cells.Select(cell => cell.GetAttribute("data-label")).Should().Equal(null, null, "Decided", "Credit");
    }

    // R3-I-two (Dr Patel, Step 3.11): two rows, oldest first as the read gives them, each its link from its registrar.
    [Fact]
    public void Two_AreListedWithTheirRegistrarsEpaStateAndWait()
    {
        var cut = Render(new Sender(waiting:
        [
            ActivityRows.Waiting(10, typeName: "Portfolio and Logbook Review (Paediatrics)", subjectName: "Pieter du Plessis",
                state: "submitted", stateLabel: "Awaiting review", since: new DateTime(2026, 9, 30, 6, 6, 0, DateTimeKind.Utc),
                epaCode: "PAED-015", observedOn: new DateOnly(2026, 9, 29)),
            ActivityRows.Waiting(11, typeName: "DOPS (Paediatrics)", subjectName: "Nomsa Mahlangu", since: EightTwelve,
                epaCode: "PAED-002", observedOn: new DateOnly(2026, 9, 22))
        ]));

        var waiting = cut.Find("section[aria-labelledby='waiting-h']");
        var heading = waiting.QuerySelector("h2#waiting-h")!;
        heading.GetAttribute("tabindex").Should().Be("-1");
        Text(heading).Should().Be("Waiting for you 2 waiting");
        heading.QuerySelector(".badge")!.ClassList.Should().Contain("badge-submitted");
        waiting.QuerySelector(".needs-you-rule")!.TextContent.Should().Be("Oldest first. Overdue once it has waited 7 days.");

        var table = waiting.QuerySelector("table")!;
        table.ClassList.Should().Contain("clinic-table--stack");
        Headers(waiting).Should().Equal("Activity", "EPA", "State", "Waiting");
        var rows = table.QuerySelectorAll("tbody tr").ToList();
        rows.Select(row => row.QuerySelector("a")!.GetAttribute("href")).Should().Equal("/activities/10", "/activities/11");

        var first = rows[0].QuerySelectorAll("td").ToList();
        first[0].QuerySelector("a.activity-link")!.TextContent.Should().Be(
            "Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-09-29, from Pieter du Plessis");
        first[1].ClassList.Should().Contain("epa-cell");
        Text(first[1]).Should().Be("PAED-015 — Take a history");
        first[2].QuerySelector(".needs-you-badges .badge")!.TextContent.Should().Be("Awaiting review");
        first[3].ClassList.Should().Contain("waited-cell");
        first[3].QuerySelector("b")!.TextContent.Should().Be("Less than a day");
        first[3].QuerySelector("span")!.TextContent.Should().Be("since 2026-09-30 08:06 SAST");
        first.Select(cell => cell.GetAttribute("data-label")).Should().Equal(null, "EPA", null, "Waiting");

        cut.FindAll("th").Should().OnlyContain(header => header.GetAttribute("role") == "columnheader");
        cut.FindAll("tbody .actions-cell, tbody button").Should().BeEmpty("the row is its link: no Open button");
        cut.Markup.Should().NotContain("Updated").And.NotContain("Encounter date");
    }

    // R3-I-overdue: Overdue in words beside the state, the count saying so, the wait in whole days.
    [Fact]
    public void Overdue_IsABadgeBesideTheState_AndTheCountSaysSo()
    {
        var cut = Render(new Sender(waiting:
        [
            ActivityRows.Waiting(10, subjectName: "Pieter du Plessis", state: "submitted", stateLabel: "Awaiting review",
                waitedDays: 8, overdue: true, since: new DateTime(2026, 9, 22, 6, 6, 0, DateTimeKind.Utc))
        ]));

        Text(cut.Find("#waiting-h")).Should().Be("Waiting for you 1 waiting, 1 overdue");
        var cells = cut.FindAll("section[aria-labelledby='waiting-h'] tbody td").ToList();
        cells[2].QuerySelectorAll(".badge").Select(badge => (badge.ClassName, badge.TextContent)).Should().Equal(
            ("badge badge-submitted", "Awaiting review"), ("badge badge-overdue", "Overdue"));
        cells[3].QuerySelector("b")!.TextContent.Should().Be("8 days");
        cells[3].QuerySelector("span")!.TextContent.Should().Be("since 2026-09-22 08:06 SAST");
    }

    // Note 8: a row updated at 22:30 UTC was updated the next day in South Africa, and says so.
    [Fact]
    public void TheWait_IsSinceTheSouthAfricanMoment_NeverTheServersClock()
    {
        var cut = Render(new Sender(waiting: [ActivityRows.Waiting(10, since: new DateTime(2026, 9, 29, 22, 30, 0, DateTimeKind.Utc))]));

        cut.Find(".waited-cell span").TextContent.Should().Be("since 2026-09-30 00:30 SAST");
    }

    // R3-I-paused-epa: an EPA not in force now is marked in the picker's own words (T231, D48).
    [Fact]
    public void APausedEpa_IsMarkedNoLongerInUse()
    {
        var paused = ActivityRows.Waiting(12, epaCode: "PAED-012") with { EpaTitle = "Communicate with families", EpaInForce = false };
        var cut = Render(new Sender(waiting: [ActivityRows.Waiting(10), paused]));

        var epas = cut.FindAll(".epa-cell").Select(Text).ToList();
        epas.Should().Equal("PAED-001 — Take a history", EpaOptionLabel.For("PAED-012", "Communicate with families", inForce: false));
        cut.FindAll(".epa-cell .muted").Select(mark => mark.TextContent.Trim()).Should().Equal(EpaOptionLabel.NoLongerInUse);
    }

    // R3-I-alike (note 9; C11): two requests from one registrar, on one EPA and date, are told apart by their accessible
    // names on the link: the state they share says nothing, so each adds when it has waited since.
    [Fact]
    public void Alike_AreToldApartByTheirLinksNames()
    {
        var cut = Render(new Sender(waiting:
        [
            ActivityRows.Waiting(20, since: EightTwelve),
            ActivityRows.Waiting(21, since: EightTwelve.AddMinutes(3)),
            ActivityRows.Waiting(22, typeName: "DOPS (Paediatrics)", since: EightTwelve)
        ]));

        var links = cut.FindAll("section[aria-labelledby='waiting-h'] a.activity-link").ToList();
        links[0].GetAttribute("aria-label").Should().Be(
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, waiting since 2026-09-30 08:12 SAST");
        links[1].GetAttribute("aria-label").Should().Be(
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, waiting since 2026-09-30 08:15 SAST");
        links[2].HasAttribute("aria-label").Should().BeFalse("a link no other reads as is named by its own words");
        links.Select(link => Accessibility.AccessibleNames.NameOf(cut, link)).Should().OnlyHaveUniqueItems();
    }

    // A decided link that reads as a waiting one is told apart too: one naming over both sections.
    [Fact]
    public void AWaitingAndADecidedLinkThatReadTheSame_AreToldApart()
    {
        var cut = Render(new Sender(
            waiting: [ActivityRows.Waiting(30, since: EightTwelve)],
            decided: [ActivityRows.Decided(31, decidedOn: EightTwelve)]));

        cut.FindAll("a.activity-link").Select(link => link.GetAttribute("aria-label")).Should().Equal(
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Requested",
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Completed");
    }

    // R3-I-twenty-five: the inbox holds them all, where Home shows five.
    [Fact]
    public void TwentyFive_AreAllListed()
    {
        var rows = Enumerable.Range(1, 25).Select(id => ActivityRows.Waiting(id, waitedDays: 33 - id, overdue: true,
            observedOn: new DateOnly(2026, 8, 1).AddDays(id))).ToList();
        var cut = Render(new Sender(waiting: rows));

        cut.FindAll("section[aria-labelledby='waiting-h'] tbody tr").Should().HaveCount(25);
        Text(cut.Find("#waiting-h")).Should().Be("Waiting for you 25 waiting, 25 overdue");
        cut.FindAll(".waiting-more").Should().BeEmpty("the overflow line is Home's");
    }

    // R3-I-long: a 31-character name and a 108-character EPA title are shown whole, in their cells.
    [Fact]
    public void Long_NamesAndTitlesAreShownWhole()
    {
        const string title = "Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research";
        var row = ActivityRows.Waiting(40, subjectName: "Nomsa Thandeka Mahlangu-Dlamini", epaCode: "PAED-015") with { EpaTitle = title };
        var cut = Render(new Sender(waiting: [row]));

        cut.Find(".activity-link-to").TextContent.Should().Be("from Nomsa Thandeka Mahlangu-Dlamini");
        Text(cut.Find(".epa-cell")).Should().Be($"PAED-015 — {title}");
    }

    // ---- Decided by you, a page at a time ----

    // R3-I-decided-45 (C5, C10 f): the pager as built, its Previous aria-disabled on page 1, not disabled; a page turn reads
    // that page and puts the focus on the section's heading.
    [Fact]
    public void Decided45_ComeTwentyAPage_ThePagersEndsAriaDisabled_AndAPageTurnFocusesTheHeading()
    {
        var sender = new Sender(decidedTotal: 45);
        var cut = Render(sender);

        Text(cut.Find("#decided-h")).Should().Be("Decided by you 45 decisions");
        cut.Find("#decided-h").GetAttribute("tabindex").Should().Be("-1");
        cut.Find(".pager-info").TextContent.Should().Be("Showing 1–20 of 45");
        sender.DecidedPages.Should().Equal(1);
        sender.DecidedSizes.Should().Equal([20], "the section opens at 20 rows");
        cut.Find("nav.pager").GetAttribute("aria-label").Should().Be("Decided by you, pages", "the board's landmark (D2)");
        var previous = PagerButton(cut, "Previous");
        previous.GetAttribute("aria-disabled").Should().Be("true");
        previous.HasAttribute("disabled").Should().BeFalse("a disabled button drops the focus");
        PagerButton(cut, "Next").HasAttribute("aria-disabled").Should().BeFalse();

        previous.Click();
        sender.DecidedPages.Should().Equal([1], "Previous on page 1 sends nothing");

        PagerButton(cut, "Next").Click();
        cut.WaitForAssertion(() => sender.DecidedPages.Should().Equal(1, 2));
        cut.Find(".pager-info").TextContent.Should().Be("Showing 21–40 of 45");
        cut.WaitForAssertion(() => FocusedIds().Should().ContainSingle()
            .Which.Should().Be(cut.Instance.DecidedHeading.Id, "the heading says which list turned"));
        sender.WaitingReads.Should().Be(1, "a page turn reads the decisions only");

        PagerButton(cut, "Next").Click();
        cut.WaitForAssertion(() => sender.DecidedPages.Should().Equal(1, 2, 3));
        var next = PagerButton(cut, "Next");
        next.GetAttribute("aria-disabled").Should().Be("true");
        next.Click();
        sender.DecidedPages.Should().Equal([1, 2, 3], "Next on the last page sends nothing");
    }

    // The build review's A1: a change of page size reads the first page at that size and leaves the focus on the select,
    // since Chrome and Edge fire change on every arrow of a closed select. Only a page turn moves it to the heading.
    [Fact]
    public void APageSizeChange_ReadsTheFirstPageAtThatSize_AndLeavesTheFocusOnTheSelect()
    {
        var sender = new Sender(decidedTotal: 45);
        var cut = Render(sender);

        cut.Find(".pager-page-size-select").Change("50");

        cut.WaitForAssertion(() => sender.DecidedSizes.Should().Equal(20, 50));
        sender.DecidedPages.Should().Equal(1, 1);
        cut.Find(".pager-info").TextContent.Should().Be("Showing 1–45 of 45");
        FocusedIds().Should().BeEmpty("the select keeps the focus");
    }

    // The build review's A3: a page turn whose read fails puts the focus on the failure, which says so, where it fell to
    // the page body with the pressed button gone.
    [Fact]
    public void APageTurnThatFails_FocusesTheFailure()
    {
        var sender = new Sender(decidedTotal: 45);
        var cut = Render(sender);

        sender.Fail = new InvalidOperationException("Npgsql: gone");
        PagerButton(cut, "Next").Click();

        cut.WaitForAssertion(() => cut.Find(".alert").TextContent.Should().Contain(ActivityInbox.LoadFailed));
        cut.Markup.Should().NotContain("Npgsql");
        cut.WaitForAssertion(() => FocusedIds().Should().ContainSingle()
            .Which.Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    // The build review's A2: Home's "All your decisions" (/activities/inbox#decided-h) lands on the section once it has
    // loaded; the fragment's target does not exist while the page reads, so the browser's own jump finds nothing.
    [Fact]
    public void ArrivingAtDecidedH_FocusesDecidedByYou_OnceLoaded()
    {
        var sender = new Sender(decidedTotal: 3);
        Services.AddSingleton<IScopedSender>(sender);
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/activities/inbox#decided-h");
        var cut = RenderComponent<ActivityInbox>();

        cut.WaitForAssertion(() => FocusedIds().Should().ContainSingle().Which.Should().Be(cut.Instance.DecidedHeading.Id));
    }

    [Fact]
    public void ArrivingWithNoFragment_MovesNoFocus()
    {
        var cut = Render(new Sender(decidedTotal: 3));

        FocusedIds().Should().BeEmpty("FocusOnNavigate has the h1");
    }

    // ---- the registrar, loading, a failure ----

    // T342, restated: a caller holding no role but Trainee has nothing here, and is told where their work is.
    [Theory]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.PendingTrainee)]
    public void RegistrarOnly_IsSentToNeedsYou(string role)
    {
        _auth.SetRoles(role);
        var cut = Render(new Sender());

        cut.Find(".state-panel-title").TextContent.Should().Be("Nothing here is yours to act on.");
        cut.Find(".state-panel-copy").TextContent.Should().Be(
            "This inbox holds work that assessors rate, discuss or review. Your drafts and work returned to you are under My activities, in Needs you.");
        cut.Find(".detail-card--empty a[href='/activities/mine']").TextContent.Trim().Should().Be("Open My activities");
        cut.FindAll("h2").Should().BeEmpty("the two sections are an assessor's");
    }

    [Theory]
    [InlineData(WombatRoles.Assessor)]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Trainee, WombatRoles.Assessor)]
    public void AnyoneElse_GetsTheTwoSections(params string[] roles)
    {
        _auth.SetRoles(roles);
        var cut = Render(new Sender());

        Headings(cut).Should().Equal("Waiting for you", "Decided by you");
        cut.FindAll(".detail-card--empty a").Should().BeEmpty();
    }

    // R3-I-loading (C11): the status is on the page from the first render and says so while the page reads; the sections'
    // headings stand over skeletons.
    [Fact]
    public void Loading_SaysSoInAStatus_UnderTheSectionsHeadings()
    {
        var sender = new Sender { Hang = true };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityInbox>();

        var status = cut.Find("p[role='status']");
        status.ClassList.Should().Contain("visually-hidden");
        status.TextContent.Should().Be("Loading the Activity inbox.");
        Headings(cut).Should().Equal("Waiting for you", "Decided by you");
        cut.FindAll("section[aria-busy='true']").Should().HaveCount(2);
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("a.activity-link, table").Should().BeEmpty();
    }

    [Fact]
    public void Loaded_TheStatusStaysAndIsEmpty()
    {
        var cut = Render(new Sender());

        cut.Find("p[role='status']").TextContent.Should().BeEmpty();
    }

    // R3-I-load-error: the fixed words, never the exception's text, and Try again; its answer focuses the first section.
    [Fact]
    public void ALoadFailure_SaysSoInItsOwnWords_AndTryAgainFocusesWaitingForYou()
    {
        var sender = new Sender { Fail = new InvalidOperationException("Npgsql: connection refused") };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityInbox>();

        cut.WaitForAssertion(() => cut.Find(".alert").TextContent.Should().Contain(
            "Could not load the Activity inbox. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll("h2").Should().BeEmpty();

        sender.Fail = null;
        cut.Find(".alert button").Click();

        cut.WaitForAssertion(() => Headings(cut).Should().Equal("Waiting for you", "Decided by you"));
        cut.WaitForAssertion(() => FocusedIds().Should().ContainSingle().Which.Should().Be(cut.Instance.WaitingHeading.Id));
    }

    // The build review's D4: a registrar's Try again whose answer is the empty state focuses that state, the region that
    // replaces the error, where it focused nothing and the focus fell to the body.
    [Fact]
    public void ARegistrarsTryAgain_FocusesTheEmptyState()
    {
        _auth.SetRoles(WombatRoles.Trainee);
        var sender = new Sender { Fail = new InvalidOperationException("Npgsql: connection refused") };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityInbox>();
        cut.WaitForAssertion(() => cut.Find(".alert button"));

        sender.Fail = null;
        cut.Find(".alert button").Click();

        cut.WaitForAssertion(() => cut.Find(".state-panel-title").TextContent.Should().Be(ActivityInbox.RegistrarEmptyTitle));
        var empty = cut.Find(".detail-card--empty");
        empty.GetAttribute("tabindex").Should().Be("-1");
        cut.WaitForAssertion(() => FocusedIds().Should().ContainSingle()
            .Which.Should().Be(cut.FindComponent<StatePanel>().Instance.EmptyElement.Id));
    }

    // ---- helpers ----

    private IRenderedComponent<ActivityInbox> Render(Sender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityInbox>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static List<string> Headings(IRenderedFragment cut)
        => cut.FindAll("h2").Select(heading => heading.ChildNodes.First().TextContent.Trim()).ToList();

    private static List<string> Headers(IElement section)
        => section.QuerySelectorAll("thead th").Select(header => header.TextContent.Trim()).ToList();

    private static IElement PagerButton(IRenderedFragment cut, string label)
        => cut.FindAll(".pager-actions button").Single(button => button.TextContent.Trim() == label);

    // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so a focus call is matched to the
    // page's own reference.
    private List<string> FocusedIds()
        => JSInterop.Invocations[FocusIdentifier].Select(call => ((ElementReference)call.Arguments[0]!).Id).ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Answers the page's two reads; counts them; fails or never answers when told to.</summary>
    private sealed class Sender(
        IReadOnlyList<ActivitySummaryDto>? waiting = null,
        IReadOnlyList<ActivitySummaryDto>? decided = null,
        int? decidedTotal = null) : IScopedSender
    {
        public Exception? Fail { get; set; }

        public bool Hang { get; init; }

        public int WaitingReads { get; private set; }

        public List<int> DecidedPages { get; } = [];

        public List<int> DecidedSizes { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (Hang)
            {
                return new TaskCompletionSource<TResponse>().Task;
            }

            if (Fail is not null)
            {
                throw Fail;
            }

            object answer = request switch
            {
                ListWaitingForYouQuery => Waiting(),
                ListDecidedByYouQuery query => Decided(query),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
            return Task.FromResult((TResponse)answer);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private WaitingForYouDto Waiting()
        {
            WaitingReads++;
            var rows = waiting ?? [];
            return new WaitingForYouDto(rows, rows.Count(row => row.IsOverdue), 7);
        }

        private ActivityListPageDto Decided(ListDecidedByYouQuery query)
        {
            DecidedPages.Add(query.Page);
            DecidedSizes.Add(query.PageSize);
            var total = decidedTotal ?? (decided ?? []).Count;
            var rows = decided ?? Enumerable.Range(0, Math.Min(query.PageSize, Math.Max(0, total - ((query.Page - 1) * query.PageSize))))
                .Select(index => ActivityRows.Decided(1000 + ((query.Page - 1) * query.PageSize) + index,
                    decidedOn: ActivityRows.When.AddMinutes(-index - (query.Page * 100))))
                .ToList();
            return new ActivityListPageDto(rows, query.Page, query.PageSize, total);
        }
    }
}
