using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T379 (flow 13, S11, F7; boards C01–C07): Trainees' Completed and closed, read on its own and paged at 20, the latest end
/// first. Until T379 it was Current trainees' read filtered for the inactive profiles, with an Edit named "Edit ‹name›" and
/// a caption that repeated its heading (T209 review's TraineesListOutcomeTests held its Outcome words, kept here).
/// </summary>
public sealed class TraineesClosedListTests : TestContext
{
    private readonly TestAuthorizationContext _auth;

    public TraineesClosedListTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("mbatha@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TwoEnded_AreListedTheLatestEndFirst_WithTheirProgrammeOutcomeAndView()
    {
        // C02: the rows in the order the query answers, the latest end first.
        var cut = Render(new FakeSender(Page(Molefe(), DuPlessis())));

        var section = Closed(cut);
        Text(section.QuerySelector("h2")!).Should().Be("Completed and closed 2 ended");
        var table = section.QuerySelector("table")!;
        Text(table.QuerySelector("caption")!).Should().Be("Programmes that have ended, the latest end first. An ended record is read-only.");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal(
            "Name", "Curriculum and sub-speciality", "Programme", "Outcome", "Actions");

        var rows = table.QuerySelectorAll("tbody tr");
        rows.Select(row => Text(row.QuerySelector("th[scope=row]")!)).Should().Equal("Lerato Molefe", "Pieter du Plessis");
        rows.Select(row => Text(row.QuerySelector("td[data-label='Programme']")!)).Should().Equal(
            "2023-01-15 to 2026-10-26 Training year 4 when it ended",
            "2025-01-15 to 2026-10-24 Training year 2 when it ended");
        rows.Select(row => Text(row.QuerySelector("td[data-label='Outcome']")!)).Should().Equal(
            "Completed 2026-10-26", "Withdrawn 2026-10-24");
        rows[0].QuerySelector(".outcome-cell .badge")!.ClassList.Should().Contain("badge-completed");
        rows[1].QuerySelector(".outcome-cell .badge")!.ClassList.Should().Contain("badge-withdrawn");
        rows[1].QuerySelector("td[data-label='Programme']")!.ClassList.Should().Contain("col-wrap");
        rows[0].QuerySelector("th[scope=row]")!.ClassList.Should().NotContain("col-wrap");

        var view = rows[0].QuerySelector("a")!;
        Text(view).Should().Be("View");
        view.GetAttribute("aria-label").Should().Be("View Lerato Molefe's record");
        view.GetAttribute("href").Should().Be("/admin/trainees/edit?id=2");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Edit" && link.GetAttribute("href") == "/admin/trainees/edit?id=2");
        section.QuerySelector(".closed-list .clinic-table--stack-wide").Should().NotBeNull("StackWide");
        section.QuerySelectorAll(".pager").Should().BeEmpty("one page");
    }

    [Fact]
    public void NoneEnded_SaysSo()
    {
        var cut = Render(new FakeSender(Page()));

        var section = Closed(cut);
        Text(section.QuerySelector("h2")!).Should().Be("Completed and closed");
        Text(section.QuerySelector(".state-panel-title")!).Should().Be("No programme has ended yet.");
        Text(section.QuerySelector(".state-panel-copy")!).Should().Be(
            "A trainee is listed here once their programme is marked complete or a withdrawal is recorded, on their programme's page.");
        section.QuerySelector("table").Should().BeNull();
    }

    [Fact]
    public void PastTwenty_ThePagerTurns_AndTheFocusGoesToTheHeading()
    {
        var first = Enumerable.Range(1, 20).Select(index => Row(100 + index, $"Person {index}")).ToArray();
        var second = Enumerable.Range(21, 20).Select(index => Row(100 + index, $"Person {index}")).ToArray();
        var sender = new FakeSender(new EndedProfilesPageDto(first, 40, 1, 20), new EndedProfilesPageDto(second, 40, 2, 20));
        var cut = Render(sender);

        var pager = Closed(cut).QuerySelector("nav.pager")!;
        pager.GetAttribute("aria-label").Should().Be("Completed and closed, pages");
        Text(pager.QuerySelector("label")!).Should().Be("Ended programmes per page");
        Text(Closed(cut).QuerySelector("h2")!).Should().Be("Completed and closed 40 ended");

        pager.QuerySelectorAll("button").Single(button => Text(button) == "Next").Click();

        sender.Asked.OfType<ListEndedProfilesQuery>().Select(query => (query.Page, query.PageSize))
            .Should().Equal((1, 20), (2, 20));
        cut.WaitForAssertion(() => Text(Closed(cut).QuerySelector("tbody tr th")!).Should().Be("Person 21"));
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke());
    }

    [Fact]
    public void TheAdministratorsList_PutsTheInstitutionUnderEachName()
    {
        _auth.SetRoles(WombatRoles.Administrator);
        var cut = Render(new FakeSender(Page(Molefe() with { InstitutionName = "Kgosi Kgari Teaching Hospital" })));

        var table = Closed(cut).QuerySelector("table")!;
        Text(table.QuerySelector("thead th")!).Should().Be("Name and institution");
        var header = table.QuerySelector("tbody th[scope=row] .person-cell")!;
        Text(header).Should().Be("Lerato Molefe Kgosi Kgari Teaching Hospital");
        Text(header.QuerySelector("span")!).Should().Be("Kgosi Kgari Teaching Hospital");
    }

    [Fact]
    public void TwoPeopleWithOneName_AreToldApartByTheirAddresses()
    {
        // C05: under the name and in View's name.
        var cut = Render(new FakeSender(Page(
            Row(7, "Thandi Nkosi", "nkosi1@kgk.wombat.local"),
            Row(8, "Thandi Nkosi", "nkosi2@kgk.wombat.local"))));

        var rows = Closed(cut).QuerySelectorAll("tbody tr");
        rows[0].QuerySelector("th .person-cell span")!.InnerHtml.Should().Be("nkosi1@<wbr>kgk.wombat.local");
        rows.Select(row => row.QuerySelector("a")!.GetAttribute("aria-label")).Should().Equal(
            "View Thandi Nkosi's record, nkosi1@kgk.wombat.local",
            "View Thandi Nkosi's record, nkosi2@kgk.wombat.local");
    }

    [Fact]
    public void AnErasedPerson_IsListedUnderThePseudonym_AsWithdrawn()
    {
        // C07, F7.
        var erased = new EndedProfileRowDto(
            9, "deleted_user_1a2b3c4d", "deleted_user_1a2b3c4d", string.Empty, "Paediatric EPA Curriculum", "11.1", "Paediatrics",
            new DateOnly(2024, 1, 15), new EndedProgrammeDto(new DateOnly(2026, 10, 27), false) { TrainingYearAtEnd = 3 }, null, true);
        var cut = Render(new FakeSender(Page(erased, Molefe())));

        var row = Closed(cut).QuerySelectorAll("tbody tr")[0];
        Text(row.QuerySelector("th")!).Should().Be("deleted_user_1a2b3c4d");
        Text(row.QuerySelector(".outcome-cell")!).Should().Be("Withdrawn 2026-10-27");
        row.QuerySelector("a")!.GetAttribute("aria-label").Should().Be("View deleted_user_1a2b3c4d's record");
    }

    [Fact]
    public void AnEndWithNoDayRecorded_SaysWithdrawn_WithNoDay()
    {
        // T209 review: a profile deactivated before T209 recorded no day.
        var cut = Render(new FakeSender(Page(Row(5, "Anna Molefe") with { Ended = null })));

        var row = Closed(cut).QuerySelector("tbody tr")!;
        Text(row.QuerySelector(".outcome-cell")!).Should().Be("Withdrawn");
        Text(row.QuerySelector("td[data-label='Programme']")!).Should().Be("2023-01-15");
    }

    private static EndedProfilesPageDto Page(params EndedProfileRowDto[] rows) => new(rows, rows.Length, 1, 20);

    private static EndedProfileRowDto Molefe() => new(
        2, "molefe", "Lerato Molefe", "molefe@kgk.wombat.local", "Paediatric EPA Curriculum", "11.1", "Paediatrics",
        new DateOnly(2023, 1, 15), new EndedProgrammeDto(new DateOnly(2026, 10, 26), true) { TrainingYearAtEnd = 4 }, null, false);

    private static EndedProfileRowDto DuPlessis() => new(
        5, "duplessis", "Pieter du Plessis", "duplessis@kgk.wombat.local", "Paediatric EPA Curriculum", "11.1", "Paediatrics",
        new DateOnly(2025, 1, 15), new EndedProgrammeDto(new DateOnly(2026, 10, 24), false) { TrainingYearAtEnd = 2 }, null, false);

    private static EndedProfileRowDto Row(int id, string name, string? email = null) => new(
        id, $"user-{id}", name, email ?? $"user{id}@kgk.wombat.local", "Paediatric EPA Curriculum", "11.1", "Paediatrics",
        new DateOnly(2023, 1, 15), new EndedProgrammeDto(new DateOnly(2026, 6, 30), false) { TrainingYearAtEnd = 4 }, null, false);

    private IRenderedComponent<PendingTraineesList> Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<PendingTraineesList>();
        cut.WaitForState(() => cut.FindAll("#trainees-closed-heading").Count > 0);
        return cut;
    }

    private static IElement Closed(IRenderedFragment cut) => cut.Find("section[aria-labelledby=trainees-closed-heading]");

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(params EndedProfilesPageDto[] pages) : IScopedSender
    {
        public List<object> Asked { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Asked.Add(request);
            object response = request switch
            {
                ListPendingTraineesQuery => Array.Empty<PendingTraineeDto>(),
                ListTraineesForSpecialityQuery => Array.Empty<TraineeProfileDto>(),
                ListEndedProfilesQuery query => pages.FirstOrDefault(page => page.Page == query.Page) ?? pages[0],
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
