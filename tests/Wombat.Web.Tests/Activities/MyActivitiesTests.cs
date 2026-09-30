using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Activities.Queries.ListNeedsYou;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T342, flow 03 (R3-C-Mine, R3-Spec § 1): My activities opens with Needs you when there is any, then All activities a
/// page at a time, each row its activity's link, who has it now, its state and its credit.
/// </summary>
public sealed class MyActivitiesTests : TestContext
{
    private readonly TestAuthorizationContext _auth;

    public MyActivitiesTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("trainee@test");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
        // A page turn moves the focus (ElementReference.FocusAsync, a JS call).
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>What <see cref="Microsoft.AspNetCore.Components.ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    [Fact]
    public void TheSubtitle_SaysTheListsOrder()
    {
        var cut = RenderMine([], [ActivityRows.Row(1)]);

        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("Everything you have filed or logged, newest encounter first.");
    }

    [Fact]
    public void NeedsYou_ComesFirst_WithItsCountInWords_ItsRule_AndWhyEachRowIsThere()
    {
        var returned = ActivityRows.Returned(2);
        var draft = ActivityRows.Row(1);
        var cut = RenderMine([returned, draft], [draft, returned, ActivityRows.Row(3, "completed", "Completed")]);

        var sections = cut.FindAll("section").ToList();
        sections.Should().HaveCount(2);
        var needsYou = sections[0];
        needsYou.QuerySelector("h2")!.Id.Should().Be("needs-you-heading");
        Text(needsYou.QuerySelector("h2")!).Should().Be("Needs you 2, 2 items");
        needsYou.QuerySelector("h2 .badge")!.GetAttribute("aria-hidden").Should().Be("true", "the figure is read as words (A16)");
        needsYou.QuerySelector(".needs-you-rule")!.TextContent.Should().Be(
            "Your drafts, and work returned to you. A request you can still cancel is with its assessor, so it is not here: it is under All activities.");

        needsYou.QuerySelectorAll(".needs-you-why").Select(why => why.TextContent).Should().Equal(
            "Returned to you by Sarah Botha on 2026-09-29. Change it and submit again.",
            "Not submitted yet. It is in nobody's inbox until you submit it.");
        needsYou.QuerySelectorAll(".needs-you-row .badge").Select(badge => badge.TextContent).Should().Equal("Draft", "Draft");

        Text(sections[1].QuerySelector("h2")!).Should().Be("All activities (3)");
    }

    [Fact]
    public void NeedsYou_IsNotDrawn_WhenNothingNeedsYou()
    {
        var cut = RenderMine([], [ActivityRows.Row(1, "requested", "Requested")]);

        cut.FindAll("#needs-you-heading").Should().BeEmpty();
        cut.Markup.Should().NotContain("needs-you-rule");
    }

    /// <summary>
    /// The link's second line: "to" the one a request goes to, "with" the one a reflection or a review is discussed with
    /// (the type's shape), none without a nominee, and none where the name already ends with the nominee (E7). A visually
    /// hidden ", " parts the two lines (A16).
    /// </summary>
    [Fact]
    public void EachLink_NamesItsNominee_ToOrWith_BehindAHiddenComma()
    {
        var cut = RenderMine([], [
            ActivityRows.Row(1),
            ActivityRows.Row(2, typeName: "Reflective Exercise (Paediatrics)", nominee: "Sarah Botha", shape: ActivityTypeShape.DiscussedOrReviewed, epaCode: "PAED-001"),
            ActivityRows.Row(3, typeName: "KGK Teaching Session Log", nominee: null, shape: ActivityTypeShape.LoggedByYou, epaCode: "PAED-015"),
            ActivityRows.Row(4, epaCode: "PAED-002") with { DisplayName = "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-25 · Sarah Botha", DisplayNameHasNominee = true, NomineeName = "Sarah Botha" }
        ]);

        var links = cut.FindAll("tbody a.activity-link").ToList();
        links.Select(link => link.TextContent).Should().Equal(
            "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, to David Naidoo",
            "Reflective Exercise (Paediatrics) · PAED-001 · 2026-09-25, with Sarah Botha",
            "KGK Teaching Session Log · PAED-015 · 2026-09-25",
            "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-25 · Sarah Botha");
        links[0].QuerySelector(".visually-hidden")!.TextContent.Should().Be(", ");
        links[0].QuerySelector(".activity-link-to")!.TextContent.Should().Be("to David Naidoo");
        links.Should().OnlyContain(link => !link.HasAttribute("aria-label"), "no two links read the same here");
    }

    /// <summary>T280: two links that would read the same are told apart by their state, in a name that starts with their words.</summary>
    [Fact]
    public void TwoLinksThatReadTheSame_AreToldApartByTheirState()
    {
        var cut = RenderMine([], [
            ActivityRows.Row(1, nominee: null),
            ActivityRows.Row(2, "cancelled", "Cancelled", nominee: null, holder: new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, null))
        ]);

        cut.FindAll("tbody a.activity-link").Select(link => link.GetAttribute("aria-label")).Should().Equal(
            "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, Draft",
            "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25, Cancelled");
    }

    [Fact]
    public void WhoHasItNow_IsWordedForEachKindOfHolder()
    {
        var cut = RenderMine([], [
            ActivityRows.Row(1),
            ActivityRows.Row(2, "requested", "Requested", holder: new ActivityHolderDto(ActivityHolderKind.Person, "thandi", "Thandi Zulu", false, ActivityRows.When)),
            ActivityRows.Row(3, "awaiting_review", "Awaiting review", holder: new ActivityHolderDto(ActivityHolderKind.Waiting, null, null, false, ActivityRows.When)),
            ActivityRows.Row(4, "completed", "Completed", holder: new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, ActivityRows.When), creditedItemCount: 1),
            ActivityRows.Row(5, "declined", "Declined", holder: new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, ActivityRows.When)),
            ActivityRows.Row(6, "requested", "Requested", holder: new ActivityHolderDto(ActivityHolderKind.Person, "trainee-1", "Sipho Ndlovu", true, ActivityRows.When))
        ]);

        Cells(cut, 1).Should().Equal("You", "Thandi Zulu", "Waiting for Awaiting review.", "Done", "Closed", "You");
        Cells(cut, 3).Should().Equal("—", "—", "—", "1 item", "—", "—");
        // Done is green whatever the state is called (D44); a refusal is red.
        cut.FindAll("tbody tr").ToList()[3].QuerySelector(".badge")!.ClassList.Should().Contain("badge-completed");
        cut.FindAll("tbody tr").ToList()[4].QuerySelector(".badge")!.ClassList.Should().Contain("badge-declined");
    }

    /// <summary>A8: the table stacks below 641px with its header row still in it, and says so in roles, not only in tags.</summary>
    [Fact]
    public void TheTable_StacksWithAHiddenHeaderAndExplicitRoles()
    {
        var cut = RenderMine([], [ActivityRows.Row(1)]);

        var table = cut.Find("table");
        table.ClassList.Should().Contain("clinic-table").And.Contain("clinic-table--stack");
        table.GetAttribute("role").Should().Be("table");
        cut.Find("thead").GetAttribute("role").Should().Be("rowgroup");
        cut.Find("tbody").GetAttribute("role").Should().Be("rowgroup");
        cut.FindAll("tr").Should().OnlyContain(row => row.GetAttribute("role") == "row");
        cut.FindAll("th").Should().OnlyContain(header => header.GetAttribute("role") == "columnheader" && header.GetAttribute("scope") == "col");
        cut.FindAll("td").Select(cell => cell.GetAttribute("data-label")).Should().Equal(null, "Who has it now", null, "Credit");
    }

    [Fact]
    public void AllActivities_ComeAPageAtATime()
    {
        var sender = new Sender([], Enumerable.Range(1, 20).Select(id => ActivityRows.Row(id)).ToList(), totalCount: 45);
        var cut = Render(sender);

        Text(cut.Find("#all-activities-heading")).Should().Be("All activities (45)");
        cut.Find(".pager-info").TextContent.Should().Be("Showing 1–20 of 45");
        sender.Pages.Should().Equal(1);

        cut.FindAll(".pager-actions button").Single(button => button.TextContent == "Next").Click();

        cut.WaitForAssertion(() => sender.Pages.Should().Equal(1, 2));
        cut.Find(".pager-info").TextContent.Should().Be("Showing 21–40 of 45");
        // The button pressed may now be disabled: the list's heading takes the focus.
        cut.WaitForAssertion(() => JSInterop.Invocations[FocusIdentifier].Should().ContainSingle());
    }

    [Fact]
    public void Empty_SaysNoActivitiesYet_AndOffersLogAnActivity()
    {
        var cut = RenderMine([], []);

        cut.Find(".state-panel-title").TextContent.Should().Be("No activities yet");
        cut.Find(".state-panel-copy").TextContent.Should().Be("Log an activity to ask an assessor to rate an encounter, or to log a teaching session.");
        cut.Find(".detail-card--empty a[href='/activities/new']").TextContent.Trim().Should().Be("Log an activity");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void ALoadFailure_SaysSoInItsOwnWords_NeverTheExceptions_AndOffersTryAgain()
    {
        var cut = Render(new Sender([], [], fail: new InvalidOperationException("Npgsql: connection refused")));

        cut.WaitForAssertion(() => cut.Find(".alert").TextContent.Should().Contain(MyActivities.LoadFailed));
        cut.Markup.Should().NotContain("Npgsql");
        cut.Find(".alert button").TextContent.Trim().Should().Be("Try again");
    }

    // The Activity inbox's tests, the registrar's empty state among them, moved to ActivityInboxTests when T350 (flow 04)
    // redrew the page in two sections.

    // ---- helpers -----------------------------------------------------------------------------------------------------

    private IRenderedComponent<MyActivities> RenderMine(IReadOnlyList<ActivitySummaryDto> needsYou, IReadOnlyList<ActivitySummaryDto> rows)
        => Render(new Sender(needsYou, rows));

    private IRenderedComponent<MyActivities> Render(Sender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<MyActivities>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static IReadOnlyList<string> Cells(IRenderedFragment cut, int column)
        => cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll("td")[column].TextContent.Trim()).ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class Sender(
        IReadOnlyList<ActivitySummaryDto> needsYou,
        IReadOnlyList<ActivitySummaryDto> rows,
        int? totalCount = null,
        Exception? fail = null) : IScopedSender
    {
        public List<int> Pages { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (fail is not null)
            {
                throw fail;
            }

            object answer = request switch
            {
                ListNeedsYouQuery => needsYou,
                ListActivitiesBySubjectQuery query => Page(query),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
            return Task.FromResult((TResponse)answer);
        }

        private ActivityListPageDto Page(ListActivitiesBySubjectQuery query)
        {
            Pages.Add(query.Page);
            query.PageSize.Should().Be(20, "the page opens at 20 rows");
            query.SubjectUserId.Should().Be("trainee-1");
            return new ActivityListPageDto(rows, query.Page, query.PageSize, totalCount ?? rows.Count);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
