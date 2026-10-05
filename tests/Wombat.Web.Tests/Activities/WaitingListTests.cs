using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T350, flow 04 (C6, R1; R3-C-Home, R3-C-Inbox, the way on): WaitingList is NeedsYouList's row with an overdue edge.
/// Each row is its link ("Type · EPA · date", then "from &lt;registrar&gt;"), its state's badge with Overdue in words
/// beside it, and how long it has waited; two links that read the same are told apart by their accessible names (note 9,
/// C11).
/// </summary>
public sealed class WaitingListTests : TestContext
{
    [Fact]
    public void EachRow_IsItsLinkFromItsRegistrar_ItsStatesBadge_AndItsWait()
    {
        var cut = Render(
            ActivityRows.Waiting(
                10, typeName: "Portfolio and Logbook Review (Paediatrics)", subjectName: "Pieter du Plessis",
                state: "submitted", stateLabel: "Awaiting review", epaCode: "PAED-015", observedOn: new DateOnly(2026, 9, 29)),
            ActivityRows.Waiting(11, typeName: "DOPS (Paediatrics)", subjectName: "Nomsa Mahlangu", waitedDays: 1,
                epaCode: "PAED-002", observedOn: new DateOnly(2026, 9, 22)));

        cut.Find("ul").ClassList.Should().Contain(["needs-you", "stack-list"]);
        var rows = cut.FindAll("li").ToList();
        rows.Should().HaveCount(2);
        rows.Should().AllSatisfy(row => row.ClassList.Should().Equal("needs-you-row"));

        var link = rows[0].QuerySelector("a.activity-link")!;
        link.GetAttribute("href").Should().Be("/activities/10");
        link.TextContent.Should().Be("Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-09-29, from Pieter du Plessis");
        link.QuerySelector(".activity-link-to")!.TextContent.Should().Be("from Pieter du Plessis");
        link.HasAttribute("aria-label").Should().BeFalse("its own words are its name");

        Badges(rows[0]).Should().Equal(("badge badge-submitted", "Awaiting review"));
        rows[0].QuerySelector(".needs-you-why")!.TextContent.Should().Be("Waiting less than a day");
        rows[1].QuerySelector(".needs-you-why")!.TextContent.Should().Be("Waiting 1 day");
    }

    /// <summary>Note 14: Overdue is a badge in words beside the state's, never in its place, and the row carries the edge.</summary>
    [Fact]
    public void AnOverdueRow_HasTheWarningEdge_AndOverdueBesideItsState()
    {
        var cut = Render(ActivityRows.Waiting(20, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true));

        var row = cut.Find("li");
        row.ClassList.Should().Contain(["needs-you-row", "needs-you-row--overdue"]);
        Badges(row).Should().Equal(("badge badge-submitted", "Requested"), ("badge badge-overdue", "Overdue"));
        row.QuerySelector(".needs-you-badges")!.Should().NotBeNull();
        row.QuerySelector(".needs-you-why")!.TextContent.Should().Be("Waiting 8 days");
    }

    [Fact]
    public void WithSince_TheRowSaysSinceWhen()
    {
        var cut = RenderComponent<WaitingList>(parameters => parameters
            .Add(list => list.Items, [ActivityRows.Waiting(
                30, waitedDays: 8, overdue: true, since: new DateTime(2026, 9, 22, 6, 6, 0, DateTimeKind.Utc))])
            .Add(list => list.WithSince, true));

        cut.Find(".needs-you-why").TextContent.Should().Be("Waiting 8 days, since 2026-09-22 08:06 SAST.");
    }

    /// <summary>
    /// The inbox's "alike" state: two requests from one registrar, on one EPA and date, filed 08:12 and 08:15. They read
    /// the same, so each link names when it has waited since; their state says the same of both, so it tells them
    /// nothing and is not added (RowNames, T239). Only those two carry an aria-label.
    /// </summary>
    [Fact]
    public void TwoLinksThatReadTheSame_AreToldApartByWhenEachHasWaitedSince_AndOnlyTheyCarryANameOfTheirOwn()
    {
        var cut = Render(
            ActivityRows.Waiting(1, since: new DateTime(2026, 9, 30, 6, 12, 0, DateTimeKind.Utc)),
            ActivityRows.Waiting(2, since: new DateTime(2026, 9, 30, 6, 15, 0, DateTimeKind.Utc)),
            ActivityRows.Waiting(3, subjectName: "Pieter du Plessis"));

        var links = cut.FindAll("a.activity-link").ToList();
        links[0].GetAttribute("aria-label").Should().Be(
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, waiting since 2026-09-30 08:12 SAST");
        links[1].GetAttribute("aria-label").Should().Be(
            "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, waiting since 2026-09-30 08:15 SAST");
        links[2].HasAttribute("aria-label").Should().BeFalse();
    }

    [Fact]
    public void TwoAlikeInEverythingButState_AreToldApartByTheirState()
    {
        var names = ActivityRowNames.Waiting(
        [
            ActivityRows.Waiting(1),
            ActivityRows.Waiting(2, state: "accepted", stateLabel: "Accepted"),
            ActivityRows.Waiting(3),
        ]);

        names[1].Should().Be("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Requested (1 of 2)");
        names[2].Should().Be("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Accepted");
        names[3].Should().Be("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini, Requested (2 of 2)");
    }

    /// <summary>A page that names its rows among more than it passes (Home's first five) passes its names.</summary>
    [Fact]
    public void ThePagesNames_AreUsed_WhenGiven()
    {
        var row = ActivityRows.Waiting(1);
        var cut = RenderComponent<WaitingList>(parameters => parameters
            .Add(list => list.Items, [row])
            .Add(list => list.Names, new Dictionary<int, string> { [1] = "Named by the page" }));

        cut.Find("a.activity-link").GetAttribute("aria-label").Should().Be("Named by the page");
    }

    // ---- T358 (flow 06; E6): the staff reading and the row's action ------------------------------------------------

    /// <summary>E6: "With Mohammed Patel" on a line of its own, above flow 04's "Waiting 8 days", which is unchanged.</summary>
    [Fact]
    public void TheStaffReading_SaysWhomEachRowWaitsWith_AboveItsWait()
    {
        var row = ActivityRows.Waiting(10, subjectName: "Pieter du Plessis", waitedDays: 8, overdue: true) with
        {
            Holder = new Wombat.Application.Features.Activities.Dtos.ActivityHolderDto(
                Wombat.Application.Features.Activities.Dtos.ActivityHolderKind.Person, "patel", "Mohammed Patel", false, null)
        };

        var cut = RenderComponent<WaitingList>(parameters => parameters
            .Add(list => list.Items, [row])
            .Add(list => list.WithNominee, true));

        cut.FindAll(".needs-you-why").Select(line => line.TextContent).Should().Equal("With Mohammed Patel", "Waiting 8 days");
        cut.Find("li").ClassList.Should().Contain(["needs-you-row", "needs-you-row--overdue"]);
    }

    /// <summary>The registrar page's Send a reminder: drawn after the why-lines, inside the row.</summary>
    [Fact]
    public void ARowAction_IsDrawnAfterTheWhyLines_InsideTheRow()
    {
        var cut = RenderComponent<WaitingList>(parameters => parameters
            .Add(list => list.Items, [ActivityRows.Waiting(1), ActivityRows.Waiting(2, subjectName: "Nomsa Mahlangu")])
            .Add(list => list.RowAction, item => builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "row-action");
                builder.AddContent(2, $"Act on {item.Id}");
                builder.CloseElement();
            }));

        var rows = cut.FindAll("li").ToList();
        rows.Select(row => row.QuerySelector("button.row-action")!.TextContent).Should().Equal("Act on 1", "Act on 2");
        rows[0].Children.Last().TagName.Should().Be("BUTTON", "after the why-lines");
    }

    /// <summary>The fence: without the staff reading or an action, every row is flow 04's.</summary>
    [Fact]
    public void WithNeither_TheRowsAreFlow04s()
    {
        var plain = Render(ActivityRows.Waiting(1, waitedDays: 8, overdue: true));
        var off = RenderComponent<WaitingList>(parameters => parameters
            .Add(list => list.Items, [ActivityRows.Waiting(1, waitedDays: 8, overdue: true)])
            .Add(list => list.WithNominee, false)
            .Add(list => list.RowAction, (RenderFragment<Wombat.Application.Features.Activities.Dtos.ActivitySummaryDto>?)null));

        off.MarkupMatches(plain.Markup);
        plain.FindAll(".needs-you-why").Select(line => line.TextContent).Should().Equal("Waiting 8 days");
    }

    private IRenderedComponent<WaitingList> Render(params Wombat.Application.Features.Activities.Dtos.ActivitySummaryDto[] rows)
        => RenderComponent<WaitingList>(parameters => parameters.Add(list => list.Items, rows));

    private static IEnumerable<(string Class, string Text)> Badges(IElement row)
        => row.QuerySelectorAll(".needs-you-badges .badge").Select(badge => (badge.ClassName!, badge.TextContent));
}
