using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.CurriculumProgress;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Progress;

/// <summary>
/// T130: the surfaces the annual quota is read on, rendered from hand-built read models.
/// </summary>
/// <remarks>
/// <para>
/// The figures themselves are computed, and tested, in the Application layer (<c>QuotaProgressCalculator</c>,
/// <c>TraineeQuotaProgressReader</c>, <c>CurriculumCoverageReader</c>). What only a render can show is the copy:
/// that a semester item reads "this semester" and an annual one "in 2026", that an exempt item loses its bar
/// rather than showing a target it cannot meet (D14), that the closed window's result survives the boundary,
/// and that the rebuild runs only once the operator has confirmed it.
/// </para>
/// <para>
/// Every date is fixed. 23 September 2026 is in semester 2 of the 2026 academic year, whose College end is
/// 30 November. The read models are what the reader would build for the stated programme start on that day,
/// so a fixture that could not occur (a semester item exempt beside a counting one, say) is never rendered.
/// </para>
/// </remarks>
public sealed class QuotaProgressRenderingTests : WombatTestContext
{
    private static readonly DateOnly AsOf = new(2026, 9, 23);

    private static readonly WindowShape Semester1Of2025 = new("Semester 1, 2025", "January to June", new(2025, 1, 1), new(2025, 6, 30));
    private static readonly WindowShape Semester2Of2025 = new("Semester 2, 2025", "July to November", new(2025, 7, 1), new(2025, 11, 30));
    private static readonly WindowShape Semester1Of2026 = new("Semester 1, 2026", "January to June", new(2026, 1, 1), new(2026, 6, 30));
    private static readonly WindowShape Semester2Of2026 = new("Semester 2, 2026", "July to November", new(2026, 7, 1), new(2026, 11, 30));
    private static readonly WindowShape Year2026 = new("2026 academic year", "January to November", new(2026, 1, 1), new(2026, 11, 30));
    private static readonly WindowShape Year2025 = new("2025 academic year", "January to November", new(2025, 1, 1), new(2025, 11, 30));

    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly TestAuthorizationContext _auth;

    public QuotaProgressRenderingTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("trainee@test");
        _auth.SetRoles(WombatRoles.Trainee);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));
    }

    // My progress itself is MyProgressTests and EpaProgressTableTests (T355, flow 05).

    // ---------------------------------------------------------------------------------------------
    // EpaTargetCoverageList: Targets by EPA (T358, flow 06; Q5; R2-Home c1, c6, c8, c11)
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Coverage_IsACountOfRegistrarsWhoMetTheTarget_NotAMean_InItsOwnColumnOverItsCaption()
    {
        // "4 of 9" tells a coordinator five have not; "44%" could mean everyone is halfway. Since T358 the figure stands in
        // its own column over its caption, the bar under the words and hidden, since the words say it (item 31).
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 2,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 2)));

        var row = cut.Find("li.coverage-row");
        var link = row.QuerySelector("a.progress-row-link")!;
        link.GetAttribute("href").Should().Be("/programme/trainees?short=1", "the EPA's name opens the registrars short on it");
        Text(link).Should().Be(
            "PAED-001 — Providing paediatric emergency care to children: 4 of 9 registrars met this semester. Show the registrars short on it.");
        Text(link.QuerySelector(".visually-hidden")!).Should().Be(": 4 of 9 registrars met this semester. Show the registrars short on it.");
        Text(row.QuerySelector(".progress-row-meta")!).Should().Be("3 per semester");
        Text(row.QuerySelector(".dashboard-metric .count-figure")!).Should().Be("4 of 9");
        Text(row.QuerySelector(".dashboard-metric .dashboard-metric-label")!).Should().Be("registrars met this semester");

        var bar = row.QuerySelector(".progress-bar")!;
        bar.GetAttribute("aria-hidden").Should().Be("true");
        bar.HasAttribute("role").Should().BeFalse("a bar under words that state it is decoration (§ Dashboard layout grid)");
        bar.QuerySelector(".progress-bar-fill")!.GetAttribute("style").Should().Be("width:44%");
        bar.QuerySelector(".progress-bar-fill")!.ClassList.Should().NotContain("is-complete");
        cut.Markup.Should().NotContain("44%<", "never a percentage in words");
    }

    [Fact]
    public void AYearlyEpa_CountsTheRegistrarsWhoMetItInTheAcademicYear()
    {
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(8, "PAED-008", "Evaluating and managing neurodevelopmental and behavioural presentations in children", QuotaPeriod.AcademicYear, 1, 0, 5, 0)));

        Text(cut.Find(".dashboard-metric-label")).Should().Be("registrars met in 2026");
        Text(cut.Find(".progress-row-meta")).Should().Be("1 per academic year");
    }

    [Fact]
    public void AnEpaEveryRegistrarIsExemptFrom_ReadsAllExempt_WithNoBar()
    {
        // Nobody applying is not 0%: a bar would read as a coverage failure, and a percentage would divide by zero.
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(11, "PAED-011", "Managing population health challenges", QuotaPeriod.AcademicYear, 1, 0, 0, 3)));

        var row = cut.Find("li.coverage-row");
        Text(row.QuerySelector(".count-figure")!).Should().Be("All exempt");
        Text(row.QuerySelector(".dashboard-metric-label")!).Should().Be("this period");
        row.QuerySelectorAll(".progress-bar").Should().BeEmpty();
        Text(row).Should().NotContain("of 0");
    }

    [Fact]
    public void AnEpaEveryRegistrarMet_KeepsItsNameAsText_AndFillsTheBar()
    {
        // Round 3 item 33 (c11): a list of the registrars short on it would be empty, so the name is not a link.
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(6, "PAED-006", "Managing long-term health conditions (LTHCs)", QuotaPeriod.Semester, 2, 5, 5, 0)));

        var row = cut.Find("li.coverage-row");
        row.QuerySelectorAll("a").Should().BeEmpty();
        Text(row.QuerySelector("div > span")!).Should().Be("PAED-006 — Managing long-term health conditions (LTHCs)");
        Text(row.QuerySelector(".progress-row-meta")!).Should().Be("2 per semester · every registrar has met it");
        Text(row.QuerySelector(".count-figure")!).Should().Be("5 of 5");
        var fill = cut.Find(".progress-bar-fill");
        fill.GetAttribute("style").Should().Be("width:100%");
        fill.ClassList.Should().Contain("is-complete");
    }

    [Fact]
    public void ALocalExtra_NamesItsOwnerInItsCadence()
    {
        var cut = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(21, "KGK-001", "Running a paediatric outreach clinic at a district hospital", QuotaPeriod.AcademicYear, 1, 0, 2, 0)
            {
                OwningInstitutionName = "Kgosi Kgari Teaching Hospital"
            }));

        Text(cut.Find(".progress-row-meta")).Should().Be("1 per academic year · Kgosi Kgari Teaching Hospital's own");
    }

    [Fact]
    public void TheRuleLine_SaysTheOrderAndWhenTheSemesterEnds_AfterTheOneExemptionWording()
    {
        var plural = RenderCoverage(Coverage(
            exemptTrainees: 2,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 2)));
        Text(plural.Find("p.needs-you-rule")).Should().Be(
            "2 registrars exempt this period, not counted. Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.");

        var singular = RenderCoverage(Coverage(
            exemptTrainees: 1,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 1)));
        Text(singular.Find("p.needs-you-rule")).Should().Be(
            "1 registrar exempt this period, not counted. Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.");

        var none = RenderCoverage(Coverage(
            exemptTrainees: 0,
            new EpaTargetCoverage(1, "PAED-001", "Providing paediatric emergency care to children", QuotaPeriod.Semester, 3, 4, 9, 0)));
        Text(none.Find("p.needs-you-rule")).Should().Be("Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.");
        Text(none.Find("ul")).Should().NotContain("trainee", "the programme's figures count registrars (Q10)");
    }

    [Fact]
    public void CoverageWithNoEpas_SaysThereIsNoCurrentRegistrar()
    {
        var cut = RenderCoverage(Coverage(exemptTrainees: 0));

        Text(cut.Find("p.card-empty")).Should().Be("No targets this period: there is no current registrar.");
        cut.FindAll(".coverage-row, .needs-you-rule").Should().BeEmpty();
    }

    // ---------------------------------------------------------------------------------------------
    // CurriculumProgressRebuild
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void TheRebuildPage_OffersTheButton_AndSendsNothingUntilConfirmed()
    {
        // The rebuild re-scores every trainee in every institution. Opening the dialog must not start it.
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(_ => ARebuildResult());
        var cut = RenderRebuildPage(sender);

        var button = PageRebuildButton(cut);
        button.HasAttribute("disabled").Should().BeFalse();

        button.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Received.Should().BeEmpty("only the dialog's confirm button runs the rebuild");
        cut.FindAll("dl.details-list").Should().BeEmpty();
    }

    [Fact]
    public void TheRebuildPage_SaysAnInactiveEpaKeepsItsProgress_AndNoLongerWarnsItIsLost()
    {
        // T196, D48. Until T196 a rebuild while an EPA was inactive removed the progress it had earned, and this page
        // warned of it. The rebuild now judges each completion at its own moment, so it loses nothing; the page says when
        // a paused completion counts instead, and the dialog no longer threatens the loss.
        var cut = RenderRebuildPage(new FakeSender());

        cut.FindAll(".alert.alert-warning").Should().BeEmpty("a rebuild no longer removes an inactive EPA's progress");
        cut.Markup.Should()
            .Contain("An inactive EPA keeps the progress it earned while it was active.")
            .And.Contain("reactivating it credits those activities without a rebuild");
        Text(cut.Find("dialog")).Should().Contain("An inactive EPA keeps the progress it earned while it was active.")
            .And.NotContain("keeps none of its progress");
    }

    [Fact]
    public void TheRebuildPage_SaysItCreditsAgainstTodaysCurriculum_AndJudgesOnlyAnEpasPauseAsOfTheCompletion()
    {
        // T196 review. A rebuild reads today's items, targets and scale pins; that is what makes it the repair after a
        // curriculum edit. Only whether an EPA was active is judged as of the completion. The page said the opposite:
        // "against the curriculum as it stood when the activity was completed".
        var cut = RenderRebuildPage(new FakeSender());

        var text = Text(cut.Find(".form-container"));
        text.Should().Contain("A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels.")
            .And.Contain("Only whether an EPA was active is judged as of when the activity was completed.")
            .And.NotContain("as it stood when the activity was completed");
    }

    [Fact]
    public void ConfirmingTheRebuild_SendsExactlyOneGlobalRebuild_AndListsWhatItMoved()
    {
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(_ => ARebuildResult());
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();

        var command = sender.Received.Should().ContainSingle().Which.Should().BeOfType<RebuildCurriculumProgressCommand>().Which;
        command.TraineeUserId.Should().BeNull("the page rebuilds everybody");
        command.Principal.IsInRole(WombatRoles.Administrator).Should().BeTrue("the handler authorizes the signed-in caller");

        Text(cut.Find(".alert.alert-success")).Should().Be("Curriculum progress was rebuilt.");
        Details(cut.Find("dl.details-list")).Should().Equal(new Dictionary<string, string>
        {
            ["Activities re-read"] = "6",
            ["Curriculum items credited"] = "7",
            ["Semester tallies written"] = "4",
            ["Stale tallies removed"] = "2",
            ["Completions re-stamped"] = "5"
        });
        cut.FindAll(".alert-danger").Should().BeEmpty();

        // The page closes the dialog before it starts the rebuild, so the modal's button cannot fire a second run
        // while the first is going; ConfirmDialog closes it again afterwards, which is harmless.
        JSInterop.VerifyInvoke("wombatDialog.close", calledTimes: 2);
    }

    [Fact]
    public void AFailedRebuild_SaysNothingWasChanged_AndLeavesTheButtonUsable()
    {
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(
            _ => throw new InvalidOperationException("Only a global Administrator may rebuild curriculum progress."));
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();

        sender.Received.OfType<RebuildCurriculumProgressCommand>().Should().ContainSingle();
        Text(cut.Find(".alert.alert-danger")).Should().Be(
            "The rebuild failed and nothing was changed: Only a global Administrator may rebuild curriculum progress.");
        cut.FindAll(".alert-success").Should().BeEmpty();
        cut.FindAll("dl.details-list").Should().BeEmpty();

        var button = PageRebuildButton(cut);
        button.HasAttribute("disabled").Should().BeFalse();
        Text(button).Should().Be("Rebuild progress");
    }

    [Fact]
    public void ASuccessfulRetry_ClearsThePreviousFailure()
    {
        var calls = 0;
        var sender = new FakeSender().On<RebuildCurriculumProgressCommand>(_ =>
            ++calls == 1 ? throw new InvalidOperationException("deadlock detected") : ARebuildResult());
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();
        cut.FindAll(".alert-danger").Should().ContainSingle();

        PageRebuildButton(cut).Click();
        ConfirmButton(cut).Click();

        sender.Received.Should().HaveCount(2);
        cut.FindAll(".alert-danger").Should().BeEmpty("a stale failure beside a success would contradict it");
        Text(cut.Find(".alert.alert-success")).Should().Be("Curriculum progress was rebuilt.");
    }

    [Fact]
    public async Task WhileTheRebuildRuns_TheButtonSaysSo_AndAPressAsksNothing()
    {
        // A second press while the first rebuild is still replaying would queue a second global rebuild. The button is
        // not disabled (T234): the dialog, closed as the rebuild starts, hands the focus back to it, and a browser drops
        // the focus of a button it disables, to the page. A press while it runs opens no dialog.
        var pending = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new FakeSender().OnAsync<RebuildCurriculumProgressCommand>(_ => pending.Task);
        var cut = RenderRebuildPage(sender);

        PageRebuildButton(cut).Click();
        var confirming = ConfirmButton(cut).ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => Text(PageRebuildButton(cut)).Should().Be("Rebuilding…"));
        PageRebuildButton(cut).HasAttribute("disabled").Should().BeFalse("it has the focus (T234)");

        PageRebuildButton(cut).Click();
        JSInterop.VerifyInvoke("wombatDialog.showModal", calledTimes: 1);

        pending.SetResult(ARebuildResult());
        await confirming;

        cut.WaitForAssertion(() => Text(PageRebuildButton(cut)).Should().Be("Rebuild progress"), AsyncWorkTimeout);
        sender.Received.Should().ContainSingle();
    }

    // ---------------------------------------------------------------------------------------------
    // Fixtures: what the reader builds on 23 September 2026 for each kind of starter
    // ---------------------------------------------------------------------------------------------

    private static TraineeCurriculumProgressDto Item(
        int id,
        string code,
        string title,
        QuotaPeriod period,
        int target,
        QuotaWindowDto current,
        QuotaWindowDto? previous = null,
        IReadOnlyList<QuotaWindowDto>? periods = null)
        => new(id, EpaId: id, code, title, period, target, current, previous, EffectiveMinimumLevelOrder: 3, EffectiveMinimumLevelLabel: "3a", TrainingYearChangedOn: null, periods);

    private static QuotaWindowDto Counting(
        WindowShape window, int count, int target, int minimumReached = 0, DateOnly? lastObservedOn = null, bool lastObservedOnDeclared = true)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            QuotaWindowStatus.Counting,
            count,
            target,
            IsMet: count >= target,
            Shortfall: Math.Max(0, target - count),
            PercentOfTarget: Math.Min(100, count * 100 / target),
            minimumReached,
            lastObservedOn,
            LastObservedOnDeclared: lastObservedOn is not null && lastObservedOnDeclared,
            FirstCountedName: null,
            FirstCountedOn: null);

    private static QuotaWindowDto Exempt(WindowShape window, int count, int target, string firstCountedName, DateOnly firstCountedOn)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            QuotaWindowStatus.ExemptPartialPeriod,
            count,
            target,
            IsMet: false,
            Shortfall: 0,
            PercentOfTarget: 0,
            MinimumLevelReachedCount: count,
            LastObservedOn: count > 0 ? window.Start.AddDays(10) : null,
            LastObservedOnDeclared: count > 0,
            firstCountedName,
            firstCountedOn);

    /// <summary>A window D49 holds to no target: the programme ended in it before its last month, or before it began.</summary>
    private static QuotaWindowDto NoTargetAfterTheEnd(WindowShape window, QuotaWindowStatus status, int count, int target)
        => new(
            window.Name,
            window.Months,
            window.Start,
            window.NominalEnd,
            status,
            count,
            target,
            IsMet: false,
            Shortfall: 0,
            PercentOfTarget: 0,
            MinimumLevelReachedCount: count,
            LastObservedOn: count > 0 ? window.Start.AddDays(10) : null,
            LastObservedOnDeclared: count > 0,
            FirstCountedName: null,
            FirstCountedOn: null);

    /// <summary>The page-level figures, derived from the items the way the reader derives them.</summary>
    private static TraineeCurriculumProgressSummaryDto Summary(
        DateOnly asOf,
        DateOnly programmeStart,
        int? stage,
        IReadOnlyList<TraineeCurriculumProgressDto> items,
        QuotaStartDto? semesterStart = null,
        QuotaStartDto? yearStart = null)
    {
        var semesterItems = items.Where(item => item.IsPerSemester).ToList();
        var yearItems = items.Where(item => !item.IsPerSemester).ToList();
        var semester2NominalEnd = new DateOnly(asOf.Year, 11, 30);

        return new TraineeCurriculumProgressSummaryDto(
            asOf,
            programmeStart,
            stage,
            CurrentSemesterName: "Semester 2, 2026",
            CurrentSemesterMonths: "July to November",
            CurrentSemesterNominalEnd: semester2NominalEnd,
            IsAfterTeachingYear: asOf > semester2NominalEnd,
            SemesterTargetsMet: semesterItems.Count(item => item.Current.IsMet),
            SemesterTargetsApplying: semesterItems.Count(item => item.Current.Applies),
            YearTargetsMet: yearItems.Count(item => item.Current.IsMet),
            YearTargetsApplying: yearItems.Count(item => item.Current.Applies),
            HasSemesterItems: semesterItems.Count > 0,
            HasYearItems: yearItems.Count > 0,
            ProgrammeNotStarted: asOf < programmeStart,
            SemesterTargetsStart: semesterItems.Count > 0 ? semesterStart : null,
            YearTargetsStart: yearItems.Count > 0 ? yearStart : null,
            Items: items);
    }

    private static CurriculumCoverage Coverage(int exemptTrainees, params EpaTargetCoverage[] epas)
        => new(AsOf, "Semester 2, 2026", "July to November", [], epas, exemptTrainees);

    private static RebuildCurriculumProgressResult ARebuildResult()
        => new(ActivitiesReplayed: 6, CreditApplications: 7, ProgressRowsWritten: 4, ProgressRowsRemoved: 2, TransitionsStamped: 5);

    private sealed record WindowShape(string Name, string Months, DateOnly Start, DateOnly NominalEnd);

    // ---------------------------------------------------------------------------------------------
    // Rendering helpers
    // ---------------------------------------------------------------------------------------------

    private IRenderedComponent<EpaTargetCoverageList> RenderCoverage(CurriculumCoverage coverage)
    {
        PinCulture();
        return RenderComponent<EpaTargetCoverageList>(parameters => parameters.Add(list => list.Coverage, coverage));
    }

    private IRenderedComponent<CurriculumProgressRebuild> RenderRebuildPage(FakeSender sender)
    {
        PinCulture();

        // The rebuild page is Administrator-only; the handler checks the caller it is handed.
        _auth.SetAuthorized("admin@test");
        _auth.SetRoles(WombatRoles.Administrator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        // ConfirmDialog opens and closes a native <dialog> through JS. Set both calls up explicitly (bUnit's
        // strict mode would throw on anything else) so the tests can also verify they were made.
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();

        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumProgressRebuild>();
        cut.WaitForState(() => cut.Markup.Contains("Curriculum progress"));

        return cut;
    }

    private static IElement PageRebuildButton(IRenderedFragment cut)
        => cut.FindAll(".form-container button").Single(button => button.Closest("dialog") is null);

    private static IElement ConfirmButton(IRenderedFragment cut)
        => cut.FindAll("dialog button").Single(button => Text(button) == "Rebuild progress");

    private static Dictionary<string, string> Details(IElement detailsList)
        => detailsList.Children
            .Where(child => child.LocalName == "div")
            .ToDictionary(
                row => Text(row.QuerySelector("dt")!),
                row => Text(row.QuerySelector("dd")!));

    /// <summary>An element's text with Razor's source indentation collapsed to single spaces.</summary>
    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string PageText(IRenderedFragment cut)
        => Regex.Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), @"\s+", " ").Trim();

    /// <summary>
    /// The page formats dates with the current culture. Pin one whose month names are English, so "30 November
    /// 2026" does not depend on the machine the suite runs on.
    /// </summary>
    private static void PinCulture()
    {
        var southAfrican = CultureInfo.GetCultureInfo("en-ZA");
        CultureInfo.CurrentCulture = southAfrican;
        CultureInfo.CurrentUICulture = southAfrican;
    }

    protected override void Dispose(bool disposing)
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
        base.Dispose(disposing);
    }

    /// <summary>Answers each request type it is told about and records every request it receives.</summary>
    private sealed class FakeSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Received { get; } = [];

        public FakeSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));
            return this;
        }

        public FakeSender OnAsync<TRequest>(Func<TRequest, Task<object?>> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            if (!_answers.TryGetValue(request.GetType(), out var answer))
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return (TResponse)(await answer(request))!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
