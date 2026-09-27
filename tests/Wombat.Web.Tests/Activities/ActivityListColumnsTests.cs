using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Epas;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T137 and T106 item 14: /activities/mine and /activities/inbox show the EPA and the encounter date, and the trainee's
/// list shows what each completion credited.
/// </summary>
/// <remarks>
/// The symptom T137 was filed on, verbatim: a released MSF campaign covering three EPAs listed as three rows reading
/// "Multi-Source Feedback (Paediatrics) · recorded · 2026-09-21 08:44", and the only way to tell them apart was to open
/// each one. The first test is that page, rebuilt from its own rows.
/// </remarks>
public sealed class ActivityListColumnsTests : TestContext
{
    private const string MsfName = "Multi-Source Feedback (Paediatrics)";
    private static readonly DateOnly CampaignClosed = new(2026, 9, 19);

    public ActivityListColumnsTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
    }

    [Fact]
    public void MyActivities_TellsACampaignsPerEpaRowsApart()
    {
        var cut = RenderMine(
            Msf(1, 5001, "PAED-001", "Assess and manage an acutely unwell child"),
            Msf(2, 5002, "PAED-002", "Lead a ward round"),
            Msf(3, 5003, "PAED-003", "Communicate with families"));

        var rows = BodyRows(cut);
        rows.Should().HaveCount(3);

        Column(cut, rows, "EPA").Should().Equal(
            "PAED-001 — Assess and manage an acutely unwell child",
            "PAED-002 — Lead a ward round",
            "PAED-003 — Communicate with families");
        Column(cut, rows, "Encounter date").Should().OnlyContain(date => date == "2026-09-19");
    }

    /// <summary>
    /// The three T108 values, worded as the activity's own history table words them, and never alike.
    /// </summary>
    // T335, flow 01 (the titles lane's note; D10): the header action and the page it opens use the same words.
    [Fact]
    public void MyActivities_OffersLogAnActivity_InTheWordsOfThePageItOpens()
    {
        var cut = RenderMine(Row(1, "draft", creditedItemCount: null));

        var action = cut.Find(".header-container .actions-cell a[href='/activities/new']");
        action.TextContent.Trim().Should().Be("Log an activity");
    }

    [Fact]
    public void MyActivities_ShowsWhatEachCompletionCredited()
    {
        var cut = RenderMine(
            Row(1, "completed", creditedItemCount: 2),
            Row(2, "completed", creditedItemCount: 1),
            Row(3, "completed", creditedItemCount: 0),
            Row(4, "requested", creditedItemCount: null));

        Column(cut, BodyRows(cut), "Credited").Should().Equal("2 items", "1 item", "None", "—");
    }

    [Fact]
    public void MyActivities_ShowsTheEncounterDate_NotTheAuditClock_AndMarksAnUndatedOne()
    {
        var cut = RenderMine(
            Row(1, "completed", creditedItemCount: null) with { ObservedOn = new DateOnly(2026, 3, 10), ObservedOnDeclared = true },
            Row(2, "draft", creditedItemCount: null) with { ObservedOn = new DateOnly(2026, 3, 20), ObservedOnDeclared = false });

        var dates = Column(cut, BodyRows(cut), "Encounter date");
        dates[0].Should().Be("2026-03-10");
        dates[1].Should().Be("not recorded (created 2026-03-20)", "nobody stated when it happened, so it must not read as a clinical date");

        cut.FindAll("th").Select(header => header.TextContent.Trim()).Should().NotContain("Updated");
        cut.Markup.Should().NotContain(":44", "the audit clock (06:44 UTC) is no longer a column, in any time zone");
    }

    [Fact]
    public void MyActivities_ShowsADashForAnActivityAboutNoEpa()
    {
        var cut = RenderMine(Row(1, "draft", creditedItemCount: null) with { EpaId = null, EpaCode = null, EpaTitle = null, EpaInForce = null });

        Column(cut, BodyRows(cut), "EPA").Should().Equal("—");
    }

    [Fact]
    public void Inbox_ShowsTheEpaAndTheEncounterDate()
    {
        var cut = RenderInbox(
            Row(1, "requested", creditedItemCount: null) with { EpaCode = "PAED-004", EpaTitle = "Resuscitate a child", ObservedOn = new DateOnly(2026, 3, 11) },
            Row(2, "requested", creditedItemCount: null) with { EpaId = null, EpaCode = null, EpaTitle = null, EpaInForce = null, ObservedOnDeclared = false, ObservedOn = new DateOnly(2026, 3, 12) });

        var rows = BodyRows(cut);
        Column(cut, rows, "EPA").Should().Equal("PAED-004 — Resuscitate a child", "—");
        Column(cut, rows, "Encounter date").Should().Equal("2026-03-11", "not recorded (created 2026-03-12)");

        // The inbox keeps its subject and its waiting clock.
        cut.FindAll("th").Select(header => header.TextContent.Trim()).Should().Contain(["Subject", "Updated"]);
    }

    /// <summary>
    /// T142. The Subject column printed the trainee's user id. It shows the name the query resolved, and nothing here
    /// looks one up.
    /// </summary>
    [Fact]
    public void Inbox_NamesTheSubject_NotTheirUserId()
    {
        var cut = RenderInbox(
            Row(1, "requested", creditedItemCount: null) with { SubjectName = "Thandi Nkosi" },
            Row(2, "requested", creditedItemCount: null) with { SubjectUserId = "departed-trainee", SubjectName = "departed-trainee" });

        Column(cut, BodyRows(cut), "Subject").Should().Equal("Thandi Nkosi", "departed-trainee");
        cut.Markup.Should().NotContain("trainee-1", "the first row's user id is not on the page");
    }

    /// <summary>
    /// T231. An EPA that is not in force now reads "(no longer in use)", the words and the flag of the activity's own
    /// picker (<c>EpaOptionLabel</c>, D48), in a muted span. An EPA in force, and an activity about no EPA, read as before.
    /// The marked cell is compared with the picker's own text (<c>EpaOptionLabel.For</c>), not a copy of it, so a change
    /// to the picker's wording that the cell does not follow fails here.
    /// </summary>
    [Fact]
    public void MyActivities_MarksAnEpaThatIsNoLongerInForce()
    {
        var cut = RenderMine(
            Row(1, "completed", creditedItemCount: 1),
            Row(2, "completed", creditedItemCount: 0) with { EpaId = 5006, EpaCode = "PAED-006", EpaTitle = "Manage a sick neonate", EpaInForce = false },
            Row(3, "draft", creditedItemCount: null) with { EpaId = null, EpaCode = null, EpaTitle = null, EpaInForce = null });

        Column(cut, BodyRows(cut), "EPA").Should().Equal(
            "PAED-001 — Take a history",
            EpaOptionLabel.For("PAED-006", "Manage a sick neonate", inForce: false),
            "—");
        MarkersIn(cut).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    /// <summary>T231. The inbox marks it too: an assessor asked to act on an activity whose EPA was deactivated.</summary>
    [Fact]
    public void Inbox_MarksAnEpaThatIsNoLongerInForce()
    {
        var cut = RenderInbox(
            Row(1, "requested", creditedItemCount: null) with { EpaId = 5006, EpaCode = "PAED-006", EpaTitle = "Manage a sick neonate", EpaInForce = false },
            Row(2, "requested", creditedItemCount: null),
            Row(3, "requested", creditedItemCount: null) with { EpaId = null, EpaCode = null, EpaTitle = null, EpaInForce = null });

        Column(cut, BodyRows(cut), "EPA").Should().Equal(
            EpaOptionLabel.For("PAED-006", "Manage a sick neonate", inForce: false),
            "PAED-001 — Take a history",
            "—");
        MarkersIn(cut).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    /// <summary>
    /// T239. Each row's View is named by its row, so the campaign's three rows are three different links to a screen
    /// reader too, and the column has a header.
    /// </summary>
    [Fact]
    public void MyActivities_NamesEachRowsView_ByItsTypeEpaAndEncounterDate()
    {
        var cut = RenderMine(
            Msf(1, 5001, "PAED-001", "Assess and manage an acutely unwell child"),
            Msf(2, 5002, "PAED-002", "Lead a ward round"),
            Msf(3, 5003, "PAED-003", "Communicate with families"));

        LinkNames(cut, "View").Should().Equal(
            $"View {MsfName}, PAED-001, encounter date 2026-09-19",
            $"View {MsfName}, PAED-002, encounter date 2026-09-19",
            $"View {MsfName}, PAED-003, encounter date 2026-09-19");
        cut.FindAll("thead th").Last().TextContent.Trim().Should().Be("Actions");
    }

    /// <summary>T239. The inbox's Open names whose activity it is too; two that read the same add when each was updated.</summary>
    [Fact]
    public void Inbox_NamesEachRowsOpen_ByItsTypeSubjectEpaAndEncounterDate()
    {
        var cut = RenderInbox(
            Row(1, "requested", creditedItemCount: null) with { SubjectName = "Thandi Nkosi" },
            Row(2, "requested", creditedItemCount: null) with { SubjectName = "Sam Smit", EpaId = null, EpaCode = null, EpaTitle = null, EpaInForce = null },
            Row(3, "requested", creditedItemCount: null) with { SubjectName = "Thandi Nkosi", UpdatedOn = new DateTime(2026, 3, 21, 6, 44, 0, DateTimeKind.Utc) });

        var names = LinkNames(cut, "Open");
        names[1].Should().Be("Open Mini-CEX (CPSA) for Sam Smit, encounter date 2026-03-10");
        names[0].Should().StartWith("Open Mini-CEX (CPSA) for Thandi Nkosi, PAED-001, encounter date 2026-03-10, updated ");
        names.Should().OnlyHaveUniqueItems();
        cut.FindAll("thead th").Last().TextContent.Trim().Should().Be("Actions");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------------

    /// <summary>The accessible name of each row's link whose visible text is <paramref name="label" />.</summary>
    private static IReadOnlyList<string> LinkNames<T>(IRenderedComponent<T> cut, string label) where T : Microsoft.AspNetCore.Components.IComponent
        => cut.FindAll("tbody td .actions-cell a")
            .Where(link => link.TextContent.Trim() == label)
            .Select(link => Accessibility.AccessibleNames.NameOf(cut, link))
            .ToList();

    private static ActivitySummaryDto Msf(int id, int epaId, string code, string title) => new(
        id,
        7,
        "msf_cpsa",
        MsfName,
        "trainee-1",
        "recorded",
        "Recorded",
        new DateTime(2026, 9, 21, 6, 44, 0, DateTimeKind.Utc),
        new DateTime(2026, 9, 21, 6, 44, 0, DateTimeKind.Utc),
        epaId,
        code,
        title,
        true,
        CampaignClosed,
        true,
        null);

    private static ActivitySummaryDto Row(int id, string state, int? creditedItemCount) => new(
        id,
        2,
        "mini_cex_cpsa",
        "Mini-CEX (CPSA)",
        "trainee-1",
        state,
        LabelOf(state),
        new DateTime(2026, 3, 20, 6, 44, 0, DateTimeKind.Utc),
        new DateTime(2026, 3, 20, 6, 44, 0, DateTimeKind.Utc),
        5000,
        "PAED-001",
        "Take a history",
        true,
        new DateOnly(2026, 3, 10),
        true,
        creditedItemCount);

    /// <summary>What the list query hands the page for a <c>mini_cex_cpsa</c> state: its label in the seed.</summary>
    private static string LabelOf(string state) => state switch
    {
        "requested" => "Requested",
        "completed" => "Completed",
        "draft" => "Draft",
        _ => state
    };

    private IRenderedComponent<MyActivities> RenderMine(params ActivitySummaryDto[] rows)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(rows));
        var cut = RenderComponent<MyActivities>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == rows.Length);
        return cut;
    }

    private IRenderedComponent<ActivityInbox> RenderInbox(params ActivitySummaryDto[] rows)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(rows));
        var cut = RenderComponent<ActivityInbox>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == rows.Length);
        return cut;
    }

    /// <summary>The text of every muted span in the EPA column's cells (an undated encounter is muted too).</summary>
    private static IReadOnlyList<string> MarkersIn<T>(IRenderedComponent<T> cut) where T : Microsoft.AspNetCore.Components.IComponent
    {
        var index = cut.FindAll("thead th").Select(cell => cell.TextContent.Trim()).ToList().IndexOf("EPA");
        return BodyRows(cut)
            .SelectMany(row => row.QuerySelectorAll("td")[index].QuerySelectorAll("span.muted"))
            .Select(span => span.TextContent.Trim())
            .ToList();
    }

    private static IReadOnlyList<IElement> BodyRows<T>(IRenderedComponent<T> cut) where T : Microsoft.AspNetCore.Components.IComponent
        => cut.FindAll("tbody tr");

    /// <summary>The text of one column, found by its header, so a reordered table cannot pass by accident.</summary>
    private static IReadOnlyList<string> Column<T>(IRenderedComponent<T> cut, IReadOnlyList<IElement> rows, string header)
        where T : Microsoft.AspNetCore.Components.IComponent
    {
        var index = cut.FindAll("thead th").Select(cell => cell.TextContent.Trim()).ToList().IndexOf(header);
        index.Should().BeGreaterThanOrEqualTo(0, "the table must have a '{0}' column", header);

        return rows.Select(row => row.QuerySelectorAll("td")[index].TextContent.Trim()).ToList();
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly IReadOnlyList<ActivitySummaryDto> _rows;

        public FakeSender(IReadOnlyList<ActivitySummaryDto> rows) => _rows = rows;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is ListActivitiesBySubjectQuery or ListActivitiesByActorInboxQuery
                ? Task.FromResult((TResponse)(object)_rows)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
