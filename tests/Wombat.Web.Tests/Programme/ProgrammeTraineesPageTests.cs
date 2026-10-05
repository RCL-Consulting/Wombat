using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Programme;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.Programme.ProgrammeWordsFixtures;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// Programme trainees (T358, flow 06, lane C; R2-Trainees t1–t10; Q1, Q2; C6, C7; D2, D3, D9; review 24, 31, 33): every
/// state the board draws. Typical at Step 3.52, Short on PAED-002 with the focus on its heading, Nothing filed, no match,
/// empty with no form, the Sub-speciality admin's subtitle, heavy with the pager, exempt, loading and load error; nothing
/// changes until Show; the address carries the filters and reading it sets them; no pronoun anywhere.
/// </summary>
/// <remarks>The figures and words are <see cref="ProgrammeWordsTests" />'; what only a render shows is where each sits.</remarks>
public sealed class ProgrammeTraineesPageTests : WombatTestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";
    private const string Secret = "Npgsql: the connection string was rejected.";

    private readonly FakeSender _sender = new();

    public ProgrammeTraineesPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("zulu@kgk.test");
        auth.SetRoles(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "zulu"));
        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── t1: typical (Step 3.52) ─────────────────────────────────────────────

    [Fact]
    public void Typical_IsTheHeaderTheFormTheCountedHeadingTheRuleAndTheRoster()
    {
        _sender.Answer(Typical());

        var cut = Render();

        Text(cut.Find("h1")).Should().Be("Programme trainees");
        TabTitle.Of(this, cut).Should().Be("Programme trainees · Wombat");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "Current registrars at Kgosi Kgari Teaching Hospital, read as Committee member · Semester 2, 2026");

        var form = cut.Find("form.search-container");
        form.GetAttribute("aria-label").Should().Be("Filter Programme trainees");
        Text(cut.Find("label[for=f-epa]")).Should().Be("Short on");
        Text(cut.Find("label[for=f-year]")).Should().Be("Training year");
        Text(cut.Find("label[for=f-filed]")).Should().Be("Nothing filed in 30 days");
        cut.FindAll("label").Count(label => Text(label) == "Nothing filed in 30 days").Should().Be(1, "the box's label once (review 31)");
        cut.Find("#f-epa option").TextContent.Should().Be("Any EPA");
        cut.Find("#f-year option").TextContent.Should().Be("Any");
        var actions = cut.Find("div.search-field.filter-actions");
        Text(actions.QuerySelector("button[type=submit].btn-primary")!).Should().Be("Show");
        actions.QuerySelector("a").Should().BeNull("Clear filters only once a filter is set");

        var section = cut.Find("section.list-section");
        section.GetAttribute("aria-labelledby").Should().Be("trainees-h");
        var heading = cut.Find("h2#trainees-h.list-section-title");
        Text(heading).Should().Be("5 current registrars");
        heading.GetAttribute("tabindex").Should().Be("-1");
        Text(cut.Find("p.needs-you-rule")).Should().Be("Fewest met first, then by surname. Semester 2, 2026 ends on 2026-11-30.");

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--index"]);
        Text(table.QuerySelector("caption")!).Should().Be("Current registrars, fewest EPAs met first");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal(
            "Registrar", "Training year", "This semester", "In 2026", "Furthest short", "Last filed");
        Names(cut).Should().Equal("Pieter du Plessis", "Nomsa Mahlangu", "Sipho Ndlovu", "Anele Dlamini", "Lerato Molefe");

        var mahlangu = RowOf(cut, "Nomsa Mahlangu");
        var link = mahlangu.QuerySelector("th[scope=row] a.progress-row-link")!;
        link.GetAttribute("href").Should().Be("/programme/trainees/12");
        mahlangu.QuerySelectorAll("td").Select(cell => cell.GetAttribute("data-label")).Should().Equal(
            "Training year", "This semester", "In 2026", "Furthest short", "Last filed");
        Text(mahlangu.QuerySelector("td[data-label='Training year']")!).Should().Be("1");
        Figure(mahlangu, "This semester").Should().Be(("0 of 10", "EPAs met this semester"));
        Figure(mahlangu, "In 2026").Should().Be(("0 of 5", "EPAs met in 2026"));
        Text(mahlangu.QuerySelector("td[data-label='Last filed']")!).Should().Be("2026-10-01");
        Text(mahlangu.QuerySelector("td[data-label='Furthest short'] .count-figure")!).Should().Be("PAED-001, PAED-003, PAED-005");
        Text(mahlangu.QuerySelector("td[data-label='Furthest short'] .count-meta")!).Should().Be("0 of 3 each this semester");
    }

    [Fact]
    public void ThePage_ReadsAsTheActingRole_ElseTheFirstAdmittedRoleHeld()
    {
        // D2, E4: a Committee member acting as Assessor who types the address reads it as Committee member.
        _sender.Answer(Typical());

        Render(acting: WombatRoles.Assessor);

        _sender.Received.OfType<ListProgrammeTraineesQuery>().Single().ActingRole.Should().Be(WombatRoles.CommitteeMember);
    }

    // ─── t2: Short on PAED-002 (from Home's EPA row) ─────────────────────────

    [Fact]
    public void ShortOn_ReadFromTheAddress_SetsTheFilter_AndItsColumns_AndTheHeadingTakesTheFocus()
    {
        _sender.Answer(ShortOnPaed002());

        var cut = Render("/programme/trainees?short=2");

        var query = _sender.Received.OfType<ListProgrammeTraineesQuery>().Single();
        (query.ShortOnEpaId, query.TrainingYear, query.NothingFiled, query.Page).Should().Be(((int?)2, (int?)null, false, 1));
        cut.Find("#f-epa").GetAttribute("value").Should().Be("2");
        Text(cut.Find("#f-epa option[value='2']")).Should().Be("PAED-002 — Managing common paediatric presentations");

        Text(cut.Find("h2#trainees-h")).Should().Be("5 of 5 current registrars are short on PAED-002");
        Text(cut.Find("p.needs-you-rule")).Should().Be(
            "PAED-002 — Managing common paediatric presentations, 3 per semester. Furthest from its target first, then " +
            "fewest EPAs met, then by surname.");
        Text(cut.Find("caption")).Should().Be("Current registrars short on PAED-002, furthest from its target first");
        cut.FindAll("thead th").Select(Text).Should().Equal(
            "Registrar", "Training year", "PAED-002", "This semester", "In 2026", "Last filed");
        Names(cut).Should().Equal("Pieter du Plessis", "Anele Dlamini", "Lerato Molefe", "Nomsa Mahlangu", "Sipho Ndlovu");
        Figure(RowOf(cut, "Nomsa Mahlangu"), "PAED-002").Should().Be(("1 of 3 this semester", "2 more by 2026-11-30"));

        var clear = cut.Find("div.filter-actions a.btn.btn-outline");
        Text(clear).Should().Be("Clear filters");
        clear.GetAttribute("href").Should().Be("/programme/trainees");

        FocusedReference().Should().Be(cut.Find("h2#trainees-h").GetAttribute("blazor:elementreference"));
    }

    // ─── Show: nothing changes until it is pressed (D3) ──────────────────────

    [Fact]
    public void ChoosingAFilter_ChangesNothing_UntilShow_WhichCarriesTheFiltersInTheAddress()
    {
        _sender.Answer(Typical());
        var cut = Render();

        cut.Find("#f-epa").Change("2");
        cut.Find("#f-year").Change("4");
        cut.Find("#f-filed").Change(true);

        _sender.Received.OfType<ListProgrammeTraineesQuery>().Should().ContainSingle("no field acts on change");
        Text(cut.Find("h2#trainees-h")).Should().Be("5 current registrars");

        cut.Find("form").Submit();

        Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/programme/trainees?short=2&year=4&filed=true");
    }

    [Fact]
    public void ShowWithTheFiltersAlreadyShown_ReadsAgainInPlace_AndTheHeadingTakesTheFocus()
    {
        _sender.Answer(Typical());
        var cut = Render();

        cut.Find("form").Submit();

        _sender.Received.OfType<ListProgrammeTraineesQuery>().Should().HaveCount(2);
        Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/programme/trainees");
        FocusedReference().Should().Be(cut.Instance.ListHeadingElement.Id);
    }

    // ─── t3: Nothing filed in 30 days ────────────────────────────────────────

    [Fact]
    public void NothingFiled_LeadsWithLastFiled_AndSaysWhatCounts()
    {
        var filter = new ProgrammeTraineesFilter(NothingFiled: true);
        _sender.Answer(Dto(Read(filter: filter) with { Rows = [Row("Sipho Ndlovu", 12, lastFiled: new DateOnly(2026, 9, 1))] }, matches: 1));

        var cut = Render("/programme/trainees?filed=true");

        cut.Find("#f-filed").HasAttribute("checked").Should().BeTrue();
        Text(cut.Find("h2#trainees-h")).Should().Be("1 of 5 current registrars has filed nothing in 30 days");
        Text(cut.Find("p.needs-you-rule")).Should().Be(
            "Nothing filed (a draft is not filed) in the last 30 days; a recorded MSF counts. A registrar admitted less than 30 " +
            "days ago is not listed. Longest without first.");
        cut.FindAll("thead th").Select(Text).Should().Equal("Registrar", "Training year", "Last filed", "This semester", "In 2026");
        Text(RowOf(cut, "Sipho Ndlovu").QuerySelector("td[data-label='Last filed']")!).Should().Be("2026-09-01");
    }

    // ─── t4: no match, not empty ─────────────────────────────────────────────

    [Fact]
    public void NoMatch_SaysSo_WithWhatWasAsked_AndClearFilters_UnderTheCountedHeading()
    {
        var filter = new ProgrammeTraineesFilter(1, 4, true);
        _sender.Answer(Dto(Read(shortOn: Paed001, filter: filter) with { Epas = [Paed001, Paed002], TrainingYears = [1, 2, 3, 4] }, matches: 0));

        var cut = Render("/programme/trainees?short=1&year=4&filed=true", WombatRoles.SpecialityAdmin, WombatRoles.SpecialityAdmin);

        Text(cut.Find("h2#trainees-h")).Should().Be("0 of 5 current registrars");
        var card = cut.Find("section.list-section .detail-card--empty");
        Text(card.QuerySelector(".state-panel-title")!).Should().Be("No registrar matches these filters.");
        Text(card.QuerySelector(".state-panel-copy")!).Should().Be("Short on PAED-001, training year 4, nothing filed in 30 days.");
        Text(card.QuerySelector(".form-actions a.btn.btn-outline")!).Should().Be("Clear filters");
        cut.FindAll("table").Should().BeEmpty();
        cut.Find("form.search-container");
        cut.Find("#f-year").GetAttribute("value").Should().Be("4");
    }

    // ─── t5: empty, no form ──────────────────────────────────────────────────

    [Fact]
    public void Empty_IsNoCurrentRegistrars_AndDrawsNoForm()
    {
        _sender.Answer(Dto(Read(current: 0), matches: 0));

        var cut = Render(roles: WombatRoles.SpecialityAdmin, acting: WombatRoles.SpecialityAdmin);

        cut.FindAll("form").Should().BeEmpty();
        Text(cut.Find("h2#trainees-h")).Should().Be("Current registrars");
        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No current registrars");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be("They appear here once they are admitted to the programme.");
    }

    // ─── t10: the Sub-speciality admin's ─────────────────────────────────────

    [Fact]
    public void TheSubSpecialityAdmin_ReadsTheSubSpeciality_AndTheSubtitleSaysSo()
    {
        _sender.Answer(Dto(Read() with { Scope = Paediatrics, Rows = CastAt352() }, matches: 5));

        var cut = Render(roles: WombatRoles.SubSpecialityAdmin, acting: WombatRoles.SubSpecialityAdmin);

        Text(cut.Find(".page-subtitle")).Should().Be(
            "Current registrars in Paediatrics, read as Sub-speciality admin · Semester 2, 2026");
        _sender.Received.OfType<ListProgrammeTraineesQuery>().Single().ActingRole.Should().Be(WombatRoles.SubSpecialityAdmin);
    }

    // ─── t6: heavy, the pager ────────────────────────────────────────────────

    [Fact]
    public void Heavy_IsTwentyAPage_UnderALabelledPager_AndAPageTurnFocusesTheHeading()
    {
        var rows = Enumerable.Range(1, 23).Select(i => Row($"Registrar {i:00}", 100 + i)).ToList();
        _sender.Answer(query => new ProgrammeTraineesDto(Read(current: 23) with { Rows = rows },
            rows.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList(), 23, query.Page, query.PageSize));

        var cut = Render();

        cut.FindAll("tbody tr").Should().HaveCount(20);
        var pager = cut.Find("nav.pager");
        pager.GetAttribute("aria-label").Should().Be("Current registrars, pages");
        _sender.Received.OfType<ListProgrammeTraineesQuery>().Single().PageSize.Should().Be(20);

        cut.FindAll("nav.pager .pager-actions button").Last().Click();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));
        _sender.Received.OfType<ListProgrammeTraineesQuery>().Last().Page.Should().Be(2);
        FocusedReference().Should().Be(cut.Instance.ListHeadingElement.Id);
    }

    // ─── t7: one registrar exempt this period ────────────────────────────────

    [Fact]
    public void AnExemptRegistrar_IsListedLast_ItsFigureCellsOne_SayingWhyInWords()
    {
        var exempt = Row("Sipho Ndlovu", 13, semesterApplying: 0, yearApplying: 0, exemption: ProgrammeExemption.StartedPartWay,
            lastFiled: new DateOnly(2026, 9, 30));
        _sender.Answer(Dto(Read(exempt: 1) with { Rows = [.. CastAt352().Where(row => row.Name != "Sipho Ndlovu"), exempt] }, matches: 5));

        var cut = Render();

        Text(cut.Find("p.needs-you-rule")).Should().Be(
            "Fewest met first, then by surname. 1 registrar exempt this period, not counted, listed last.");
        Text(cut.Find("caption")).Should().Be("Current registrars, fewest EPAs met first, one exempt this period");
        Names(cut).Last().Should().Be("Sipho Ndlovu");
        var cells = RowOf(cut, "Sipho Ndlovu").QuerySelectorAll("td").ToList();
        cells.Select(cell => cell.GetAttribute("data-label")).Should().Equal("Training year", "This period", "Last filed");
        cells[1].GetAttribute("colspan").Should().Be("3");
        Text(cells[1].QuerySelector(".badge")!).Should().Be("Exempt this period");
        Text(cells[1].QuerySelector(".count-meta")!).Should().Be("Started part-way through the period");
    }

    // ─── t8: loading ─────────────────────────────────────────────────────────

    [Fact]
    public void WhileItLoads_TheFiltersStand_SkeletonRowsUnderTheHeading_AndTheStatusSaysSo()
    {
        var pending = new TaskCompletionSource<object?>();
        _sender.AnswerAsync(_ => pending.Task);

        var cut = Render(wait: false);

        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("Loading Programme trainees.");
        cut.Find("form.search-container");
        var section = cut.Find("section.list-section[aria-busy=true]");
        Text(section.QuerySelector("h2")!).Should().Be("Current registrars");
        section.QuerySelectorAll(".dashboard-card-skeleton .skeleton").Should().HaveCount(4);
        section.QuerySelectorAll("[style]").Should().BeEmpty("no inline sizes (round-3-check 5)");

        pending.SetResult(Typical());
        cut.WaitForAssertion(() => cut.Find("table"), AsyncWorkTimeout);
        Text(cut.Find("p.visually-hidden[role=status]")).Should().BeEmpty();
    }

    // ─── t9: load error ──────────────────────────────────────────────────────

    [Fact]
    public void AFailedRead_IsTheFixedWords_WithTryAgain_WhoseAnswerTakesTheFocus()
    {
        var fail = true;
        _sender.Answer(_ => fail ? throw new InvalidOperationException(Secret) : Typical());

        var cut = Render(waitFor: "Could not load Programme trainees.");

        var alert = cut.Find(".action-result .alert.alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load Programme trainees. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain(Secret);
        cut.Find("form.search-container");

        fail = false;
        alert.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => cut.Find("h2#trainees-h"));
        FocusedReference().Should().Be(cut.Instance.ListHeadingElement.Id);
    }

    [Fact]
    public void ARoleWithNoProgramme_IsPageNotFound()
    {
        _sender.Answer(_ => null);

        var cut = Render(waitFor: "Page not found");

        Text(cut.Find("h1")).Should().Be("Page not found");
        Text(cut.Find(".system-panel p")).Should().Be("There is no page at this address.");
        cut.FindAll("form").Should().BeEmpty();
    }

    // ─── Words ───────────────────────────────────────────────────────────────

    [Fact]
    public void NoPronoun_NamesAPerson_InAnyStateDrawn()
    {
        _sender.Answer(Typical());
        var typical = Render();
        PageText(typical).Should().Match(text => !NamesAPersonByPronoun(text));
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    /// <summary>The cast at Step 3.52, as the reader orders it: fewest met first, then by surname (R2-Trainees t1).</summary>
    private static IReadOnlyList<ProgrammeTraineeRowDto> CastAt352() =>
    [
        Row("Pieter du Plessis", 11, 2, furthest: [Short("PAED-001", 0, 3), Short("PAED-002", 0, 3), Short("PAED-003", 0, 3)], lastFiled: new(2026, 9, 29)),
        Row("Nomsa Mahlangu", 12, 1, furthest: [Short("PAED-001", 0, 3), Short("PAED-003", 0, 3), Short("PAED-005", 0, 3)], lastFiled: new(2026, 10, 1)),
        Row("Sipho Ndlovu", 13, 1, furthest: [Short("PAED-001", 0, 3), Short("PAED-003", 0, 3), Short("PAED-004", 0, 3)], lastFiled: new(2026, 9, 30)),
        Row("Anele Dlamini", 14, 3, semesterMet: 1, furthest: [Short("PAED-002", 0, 3), Short("PAED-003", 0, 3), Short("PAED-005", 0, 3)], lastFiled: new(2026, 9, 24)),
        Row("Lerato Molefe", 15, 4, semesterMet: 1, furthest: [Short("PAED-002", 0, 3), Short("PAED-003", 0, 3), Short("PAED-004", 0, 3)], lastFiled: new(2026, 9, 30))
    ];

    private static ProgrammeTraineesDto Typical()
        => Dto(Read() with { Rows = CastAt352(), Epas = [Paed001, Paed002], TrainingYears = [1, 2, 3, 4] }, matches: 5);

    private static ProgrammeTraineesDto ShortOnPaed002()
    {
        var shortOn = new Dictionary<string, int> { ["Pieter du Plessis"] = 0, ["Anele Dlamini"] = 0, ["Lerato Molefe"] = 0, ["Nomsa Mahlangu"] = 1, ["Sipho Ndlovu"] = 1 };
        var rows = shortOn.Select(pair => CastAt352().Single(row => row.Name == pair.Key) with { ShortOn = Short("PAED-002", pair.Value, 3) }).ToList();
        return Dto(Read(shortOn: Paed002, filter: new ProgrammeTraineesFilter(2)) with { Rows = rows, Epas = [Paed001, Paed002], TrainingYears = [1, 2, 3, 4] }, matches: 5);
    }

    private static ProgrammeTraineeRowDto Row(
        string name, int profileId, int? trainingYear = 1, int semesterMet = 0, int semesterApplying = 10, int yearApplying = 5,
        ProgrammeExemption? exemption = null, IReadOnlyList<EpaShortfallDto>? furthest = null, DateOnly? lastFiled = null)
        => ProgrammeWordsFixtures.Row(name, trainingYear, semesterMet, semesterApplying, 0, yearApplying, exemption, furthest, null, lastFiled)
            with { ProfileId = profileId };

    private static ProgrammeTraineesDto Dto(ProgrammeRosterRead read, int matches)
        => new(read, read.Rows.Take(20).ToList(), matches, 1, 20);

    private IRenderedComponent<ProgrammeTrainees> Render(
        string address = "/programme/trainees", string? acting = WombatRoles.CommitteeMember, string? roles = null, bool wait = true, string? waitFor = null)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        var held = roles is null ? [WombatRoles.CommitteeMember, WombatRoles.Assessor] : new[] { roles };
        var cut = RenderComponent<ProgrammeTrainees>(parameters => parameters.AddCascadingValue(ActingRoleResolver.Resolve(acting, held)));
        if (wait)
        {
            cut.WaitForState(() => waitFor is null ? !cut.Markup.Contains("Loading Programme trainees.") : cut.Markup.Contains(waitFor));
        }

        return cut;
    }

    private string? FocusedReference()
        => JSInterop.Invocations.Last(invocation => invocation.Identifier == FocusIdentifier)
            .Arguments[0].Should().BeOfType<ElementReference>().Which.Id;

    private static (string Value, string Meta) Figure(IElement row, string column)
    {
        var cell = row.QuerySelector($"td[data-label='{column}']")!;
        return (Text(cell.QuerySelector(".count-figure")!), Text(cell.QuerySelector(".count-meta")!));
    }

    private static IReadOnlyList<string> Names(IRenderedFragment cut) => cut.FindAll("tbody th[scope=row]").Select(Text).ToList();

    private static IElement RowOf(IRenderedFragment cut, string name)
        => cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("th")!) == name);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string PageText(IRenderedFragment cut)
        => Regex.Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), @"\s+", " ").Trim();

    /// <summary>Answers Programme trainees' read and records every request.</summary>
    private sealed class FakeSender : IScopedSender
    {
        private Func<ListProgrammeTraineesQuery, Task<object?>> _answer = _ => Task.FromResult<object?>(null);

        public List<object> Received { get; } = [];

        public void Answer(ProgrammeTraineesDto dto) => _answer = _ => Task.FromResult<object?>(dto);

        public void Answer(Func<ListProgrammeTraineesQuery, ProgrammeTraineesDto?> answer)
            => _answer = query => Task.FromResult<object?>(answer(query));

        public void AnswerAsync(Func<ListProgrammeTraineesQuery, Task<object?>> answer) => _answer = answer;

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            return request is ListProgrammeTraineesQuery query
                ? (TResponse)(await _answer(query))!
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
