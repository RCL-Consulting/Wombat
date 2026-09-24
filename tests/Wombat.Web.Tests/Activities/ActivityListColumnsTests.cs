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
        dates[1].Should().Be("2026-03-20 (filed; no encounter date)", "nobody stated when it happened, so it must not read as a clinical date");

        cut.FindAll("th").Select(header => header.TextContent.Trim()).Should().NotContain("Updated");
        cut.Markup.Should().NotContain(":44", "the audit clock (06:44 UTC) is no longer a column, in any time zone");
    }

    [Fact]
    public void MyActivities_ShowsADashForAnActivityAboutNoEpa()
    {
        var cut = RenderMine(Row(1, "draft", creditedItemCount: null) with { EpaId = null, EpaCode = null, EpaTitle = null });

        Column(cut, BodyRows(cut), "EPA").Should().Equal("—");
    }

    [Fact]
    public void Inbox_ShowsTheEpaAndTheEncounterDate()
    {
        var cut = RenderInbox(
            Row(1, "requested", creditedItemCount: null) with { EpaCode = "PAED-004", EpaTitle = "Resuscitate a child", ObservedOn = new DateOnly(2026, 3, 11) },
            Row(2, "requested", creditedItemCount: null) with { EpaId = null, EpaCode = null, EpaTitle = null, ObservedOnDeclared = false, ObservedOn = new DateOnly(2026, 3, 12) });

        var rows = BodyRows(cut);
        Column(cut, rows, "EPA").Should().Equal("PAED-004 — Resuscitate a child", "—");
        Column(cut, rows, "Encounter date").Should().Equal("2026-03-11", "2026-03-12 (filed; no encounter date)");

        // The raw subject id stays, for T142 to replace, and the inbox keeps its waiting clock.
        cut.FindAll("th").Select(header => header.TextContent.Trim()).Should().Contain(["Subject", "Updated"]);
    }

    // ---- helpers ----------------------------------------------------------------------------------------------------

    private static ActivitySummaryDto Msf(int id, int epaId, string code, string title) => new(
        id,
        7,
        "msf_cpsa",
        MsfName,
        "trainee-1",
        "recorded",
        new DateTime(2026, 9, 21, 6, 44, 0, DateTimeKind.Utc),
        new DateTime(2026, 9, 21, 6, 44, 0, DateTimeKind.Utc),
        epaId,
        code,
        title,
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
        new DateTime(2026, 3, 20, 6, 44, 0, DateTimeKind.Utc),
        new DateTime(2026, 3, 20, 6, 44, 0, DateTimeKind.Utc),
        5000,
        "PAED-001",
        "Take a history",
        new DateOnly(2026, 3, 10),
        true,
        creditedItemCount);

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
