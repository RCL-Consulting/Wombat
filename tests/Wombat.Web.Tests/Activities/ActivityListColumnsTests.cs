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
using Wombat.Application.Features.Epas;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T137 and T106 item 14: /activities/mine and /activities/inbox show the EPA and the encounter date, and the trainee's
/// list shows what each completion credited. Since T342 (flow 03) My activities shows the EPA and the date in each row's
/// link, its name (MyActivitiesTests holds the rest of the new page). Since T350 (flow 04) the inbox's columns are
/// ActivityInboxTests'.
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

        // T342 (flow 03): the EPA and the encounter date are in each row's link, its name "Type · EPA · date" (E7, E9),
        // where T137 had them as columns.
        Column(cut, rows, "Activity").Should().Equal(
            $"{MsfName} · PAED-001 · 2026-09-19",
            $"{MsfName} · PAED-002 · 2026-09-19",
            $"{MsfName} · PAED-003 · 2026-09-19");
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

        Column(cut, BodyRows(cut), "Credit").Should().Equal("2 items", "1 item", "None", "—");
    }

    /// <summary>
    /// T342 (flow 03, R3-C-Mine): four columns, Activity, Who has it now, State and Credit. The encounter date is in the
    /// name, and the audit clock is on no column (T137).
    /// </summary>
    [Fact]
    public void MyActivities_HasTheBoardsFourColumns_AndNoAuditClock()
    {
        var cut = RenderMine(Row(1, "completed", creditedItemCount: null));

        cut.FindAll("thead th").Select(header => header.TextContent.Trim())
            .Should().Equal("Activity", "Who has it now", "State", "Credit");
        cut.Markup.Should().NotContain(":44", "the audit clock (06:44 UTC) is no column, in any time zone");
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

        // T342: the name carries the code only, so the mark is under the link, in the picker's own words (D48).
        var marks = cut.FindAll("tbody td .muted").Select(mark => mark.TextContent.Trim()).ToList();
        marks.Should().Equal($"PAED-006 {EpaOptionLabel.NoLongerInUse}");
        BodyRows(cut).ToList()[1].QuerySelector(".muted").Should().NotBeNull("only the EPA that is not in force is marked");
    }

    /// <summary>
    /// T239, restated by T342 (T280): a row is its activity's link, and the link's words are its name. No View button, and
    /// no Actions column.
    /// </summary>
    [Fact]
    public void MyActivities_EachRowIsItsActivitysLink_NamedByItsOwnWords()
    {
        var cut = RenderMine(
            Msf(1, 5001, "PAED-001", "Assess and manage an acutely unwell child"),
            Msf(2, 5002, "PAED-002", "Lead a ward round"));

        var links = cut.FindAll("tbody a.activity-link");
        links.Select(link => link.GetAttribute("href")).Should().Equal("/activities/1", "/activities/2");
        links.Should().OnlyContain(link => !link.HasAttribute("aria-label"), "each link's words are its name");
        cut.FindAll("tbody .actions-cell").Should().BeEmpty();
        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().NotContain("Actions");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------------

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
        null)
    {
        DisplayName = $"{MsfName} · {code} · 2026-09-19",
        Holder = new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, null)
    };

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
        creditedItemCount)
    {
        DisplayName = "Mini-CEX (CPSA) · PAED-001 · 2026-03-10"
    };

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
            => request switch
            {
                // T342 (B7): My activities' list comes a page at a time.
                ListActivitiesBySubjectQuery => Task.FromResult((TResponse)(object)new ActivityListPageDto(_rows, 1, 20, _rows.Count)),
                ListNeedsYouQuery => Task.FromResult((TResponse)(object)Array.Empty<ActivitySummaryDto>()),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
