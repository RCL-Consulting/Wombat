using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The schedule page's states, as round 3's boards R2-4 draw them (T364, flow 08): empty from Committee reviews (C4), the
/// type preselected by <c>CommitteeReviewTypes.DefaultFor</c> (F4), the seat already held before Create (Q3; T309 item 1),
/// a formative check-in, NothingToDecide (S1), the preview's failure (C6), each refusal on Create in a summary and beside
/// its field with the focus on the summary (C7, T324), busy (S6), created, and the load error (C11, T338).
/// </summary>
public sealed partial class ScheduleReviewPageTests : TestContext
{
    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;
    private const string Molefe = "molefe";

    /// <summary>2026-10-13 10:00 in South Africa: 2026 S2.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 13, 8, 0, 0, TimeSpan.Zero);

    private readonly RecordingSender _sender = new();

    public ScheduleReviewPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-a"));
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(Now));
        _sender
            .On<GetCommitteeReviewsAccessQuery>(_ => new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null))
            .On<ListDecisionPanelsQuery>(_ => new[]
            {
                new DecisionPanelSummaryDto(GeneralPanel, "Paed Annual Review Panel", DecisionPanelScope.Institution, 1, null, 4),
                new DecisionPanelSummaryDto(
                    NeonatalPanel, "Neonatal CCC", DecisionPanelScope.Institution, 1, null, 2, "neonatal",
                    "Neonatal team Clinical Competency Committee")
            })
            .On<ListSchedulableTraineesQuery>(_ => new[] { new SchedulableTraineeDto(Molefe, "Lerato Molefe", 4) })
            .On<GetSchedulingSeatQuery>(_ => null)
            .On<PreviewCommitteeAgendaQuery>(query => Preview(query.AcademicYear, query.Semester))
            .On<ScheduleCommitteeReviewCommand>(command => Scheduled(77, command));
    }

    // ---- Empty, from Committee reviews (C4) -----------------------------------------------------------------------------

    [Fact]
    public void FromCommitteeReviews_TheFormIsEmpty_TheTraineeWaitsForAPanel_AndTheDefaultTypeIsPreselected()
    {
        var cut = RenderPage();

        cut.Find("h1").TextContent.Should().Be("Schedule a review");
        cut.Find(".page-subtitle").TextContent.Should().Be(ScheduleWords.Subtitle);
        cut.Find(".committee-calendar").Should().NotBeNull("the five pages wrap their content in it (§ 0)");
        cut.Find("#review-panel").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("#review-trainee").HasAttribute("disabled").Should().BeTrue();
        cut.Find("#review-trainee option").TextContent.Should().Be("Choose a panel first");
        cut.Find("#review-trainee-help").TextContent.Should().Be(ScheduleWords.TraineeWaitsForPanel);
        cut.Find("#review-period").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("#review-scheduled").GetAttribute("value").Should().Be("2026-10-13", "Scheduled on starts on today");
        cut.Find("#review-scheduled").GetAttribute("type").Should().Be("date", "F3: the browser's date input");

        // F4: preselected by CommitteeReviewTypes.DefaultFor, Annual progression for a general panel; Formative check-in
        // folded into Review type (T309 item 3), with no Formative only tick.
        TypeValues(cut).Should().Equal("AnnualProgression", "PreGraduation", ScheduleReview.FormativeKey);
        Checked(cut).Should().Be(CommitteeReviewTypes.DefaultFor(false, 2).ToString());
        cut.FindAll("#review-formative").Should().BeEmpty();
        // D7 (T364 build review): each radio is named by its type alone, and its help is the radio's description, shown
        // under the label.
        Text(cut.Find("label[for='review-type-PreGraduation']")).Should().Be("Pre-graduation review");
        Text(cut.Find("#review-type-PreGraduation-help")).Should().Be(
            "The final review before the programme ends: it decides the trainee's progression, and whether they may graduate.");
        cut.Find("#review-type-PreGraduation").GetAttribute("aria-describedby").Should().Be("review-type-PreGraduation-help");
        Text(cut.Find("label[for='review-type-Formative']")).Should().Be("Formative check-in");
        Text(cut.Find("#review-type-Formative-help")).Should().Be(
            "Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. " +
            "It takes no seat for the period.");
        cut.Find("#review-type-Formative").GetAttribute("aria-describedby").Should().Be("review-type-Formative-help");

        Text(cut.Find("#agenda-preview-summary")).Should().Be(ScheduleWords.ChooseToPreview);
        CancelLink(cut).GetAttribute("href").Should().Be("/committee/reviews");
        cut.Find("button[type=submit]").TextContent.Trim().Should().Be("Create review");
    }

    [Fact]
    public void InSemester1_TheDefaultIsStillTheFirstAllowedType_AndEntrustmentOnlyIsOffered()
    {
        // round-3-check.md: the S1 boards' Entrustment-only default is wrong; the code's default is the first allowed type.
        var cut = RenderPage();
        cut.Find("#review-panel").Change(GeneralPanel.ToString());
        cut.Find("#review-period").Change("2026-1");

        TypeValues(cut).Should().Equal("AnnualProgression", "PreGraduation", "EntrustmentOnly", ScheduleReview.FormativeKey);
        Checked(cut).Should().Be(CommitteeReviewTypes.DefaultFor(false, 1).ToString()).And.Be("AnnualProgression");
        Text(cut.Find("#review-type-AnnualProgression-help")).Should().EndWith("Decides the trainee's progression for the year.");
    }

    [Fact]
    public void AnUnchosenType_FollowsTheDefault_WhenThePanelChanges()
    {
        // D4 (T364 build review; F4): before the neonatal CCC in S1 the default is Entrustment-only; on a general panel it
        // is Annual progression, though Entrustment-only is still allowed there.
        var cut = RenderPage();
        cut.Find("#review-period").Change("2026-1");
        cut.Find("#review-panel").Change(NeonatalPanel.ToString());
        Checked(cut).Should().Be("EntrustmentOnly", "guard: the neonatal CCC's default");

        cut.Find("#review-panel").Change(GeneralPanel.ToString());

        Checked(cut).Should().Be(CommitteeReviewTypes.DefaultFor(false, 1).ToString()).And.Be("AnnualProgression");
    }

    [Fact]
    public void ATypeChosenByHand_IsKept_WhilePanelAndPeriodChangesStillAllowIt()
    {
        var cut = RenderPage();
        cut.Find("#review-period").Change("2026-1");
        cut.Find("#review-panel").Change(GeneralPanel.ToString());
        cut.Find("#review-type-EntrustmentOnly").Change(true);

        cut.Find("#review-panel").Change(NeonatalPanel.ToString());
        cut.Find("#review-panel").Change(GeneralPanel.ToString());
        cut.Find("#review-period").Change("2025-1");

        Checked(cut).Should().Be("EntrustmentOnly", "the registrar's own choice, still allowed");

        cut.Find("#review-period").Change("2026-2");
        Checked(cut).Should().Be("AnnualProgression", "not allowed in S2, so the default");
    }

    // ---- The seat already held (Q3, T309 item 1) ------------------------------------------------------------------------

    [Fact]
    public void ASeatAlreadyHeld_IsSaidBeforeCreate_AndCreateIsNotOffered_ButTheReviewInTheWayIs()
    {
        _sender.On<GetSchedulingSeatQuery>(_ => Seat());
        var cut = RenderPage();

        Choose(cut, "2026-2");
        cut.WaitForState(() => Text(cut.Find("#agenda-preview-summary")).Contains("Already held."));

        Text(cut.Find("#agenda-preview-summary")).Should().Be(
            "Already held. " + Seat().Sentence + " " + ScheduleWords.CheckInTakesNoSeat);
        cut.FindAll("button[type=submit]").Should().BeEmpty("Create is not offered while the seat is held");
        Text(cut.Find(".form-actions .move-reasons")).Should().Be(
            "Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.");
        var open = cut.Find(".form-actions a.btn-primary");
        open.TextContent.Should().Be("Open review #1");
        open.GetAttribute("href").Should().Be("/committee/reviews/1");
        open.GetAttribute("aria-label").Should().Be("Open review #1 of Lerato Molefe", "the visible words first (S7)");

        // A check-in takes no seat: choosing it offers Create again, and reads neither seat nor agenda.
        cut.Find("#review-type-Formative").Change(true);
        cut.Find("button[type=submit]").TextContent.Trim().Should().Be("Create review");
        Text(cut.Find("#agenda-preview-summary")).Should().Be(ScheduleWords.FormativeNoAgenda);
    }

    [Fact]
    public void AFormativeCheckIn_IsScheduledAsFormative_WithItsDefaultStoredType()
    {
        var cut = RenderPage();
        Choose(cut, "2026-2");
        cut.Find("#review-type-Formative").Change(true);

        cut.Find("form").Submit();

        var command = _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().ContainSingle().Subject;
        command.IsFormative.Should().BeTrue();
        command.ReviewType.Should().Be(CommitteeReviewType.AnnualProgression);
    }

    [Fact]
    public void WhileTheAgendaIsRead_TheAsideSaysSo_NeverToChooseAgain()
    {
        // D2 (T364 build review): between the choice and the reads returning, the live region says it is reading.
        var answer = new TaskCompletionSource<object?>();
        _sender.OnPending<PreviewCommitteeAgendaQuery>(_ => answer.Task);
        var cut = RenderPage();

        Choose(cut, "2026-2");

        Text(cut.Find("#agenda-preview-summary")).Should().Be(ScheduleWords.ReadingAgenda).And.Be("Reading the agenda.");

        answer.SetResult(Preview(2026, 2));
        cut.WaitForState(() => !Text(cut.Find("#agenda-preview-summary")).StartsWith("Reading", StringComparison.Ordinal));
        Text(cut.Find("#agenda-preview-summary")).Should().NotBe(ScheduleWords.ChooseToPreview);
    }

    // ---- NothingToDecide (S1) -------------------------------------------------------------------------------------------

    [Fact]
    public void APanelThatDecidesNothing_IsSaidFirst_InTheHandlersSentence_AndCreateRefusesBesidePanel()
    {
        const string Nothing =
            "Neonatal CCC decides no EPA on this trainee's curriculum, so an entrustment-only review before it would have nothing to decide.";
        _sender
            .On<PreviewCommitteeAgendaQuery>(query => Preview(query.AcademicYear, query.Semester) with
            {
                Lines = [],
                NothingToDecide = Nothing,
                PanelDecidesAnything = false
            })
            .On<ScheduleCommitteeReviewCommand>(_ => throw new CommitteeRefusedException(Nothing));
        var cut = RenderPage();
        Choose(cut, "2026-2", NeonatalPanel);
        cut.WaitForState(() => Text(cut.Find("#agenda-preview-summary")).Contains("No EPA will be on the agenda."));

        Text(cut.Find("#agenda-preview-summary")).Should().Be($"No EPA will be on the agenda. {Nothing}");
        Checked(cut).Should().Be("EntrustmentOnly", "the one binding type before the neonatal CCC (F4)");

        cut.Find("form").Submit();

        var summary = cut.Find("#schedule-refusal");
        Text(summary).Should().Be($"The review was not created. Everything you chose is kept below. Panel: {Nothing}");
        summary.QuerySelector("a")!.GetAttribute("href").Should().Be("#review-panel");
        cut.Find("#review-panel-msg").TextContent.Should().Be(Nothing);
    }

    // ---- The preview's failure (C6, T338, T272) ------------------------------------------------------------------------

    [Fact]
    public void APreviewThatFails_SaysSoInFixedWords_WithTryAgain_AndCreateStaysOffered()
    {
        _sender.On<PreviewCommitteeAgendaQuery>(_ => throw new TimeoutException("Npgsql: the pool is exhausted"));
        var cut = RenderPage();

        Choose(cut, "2026-2");
        cut.WaitForState(() => Text(cut.Find("#agenda-preview-summary")).Contains(ScheduleWords.PreviewFailed));

        Text(cut.Find("#agenda-preview-summary")).Should().Be(
            $"{ScheduleWords.PreviewFailed} Try again {ScheduleWords.PreviewFailedNote}");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("pool");
        cut.Find("#agenda-preview-summary").QuerySelectorAll("[role]").Should().BeEmpty("no role=alert in the live region");
        cut.Find("button[type=submit]").TextContent.Trim().Should().Be("Create review", "the preview is advice");

        _sender.On<PreviewCommitteeAgendaQuery>(query => Preview(query.AcademicYear, query.Semester));
        cut.FindAll("#agenda-preview button").Single(button => button.TextContent.Trim() == "Try again").Click();
        cut.WaitForState(() => Text(cut.Find("#agenda-preview-summary")).Contains("will be on the agenda"));
    }

    // ---- Refusals on Create (C7, T324) ----------------------------------------------------------------------------------

    [Fact]
    public void MissingFields_AreRefusedInASummaryAboveTheActions_AndBesideEachField_WithTheFocusOnTheSummary()
    {
        var cut = RenderPage();

        cut.Find("form").Submit();

        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().BeEmpty();
        var summary = cut.Find("#schedule-refusal");
        summary.GetAttribute("tabindex").Should().Be("-1");
        Text(summary.QuerySelector("strong")!).Should().Be("The review was not created.");
        summary.QuerySelectorAll("li a").Select(link => (Text(link), link.GetAttribute("href"))).Should().Equal(
            ("Panel: Choose a panel.", "#review-panel"),
            ("Period: Choose the period the review sits for.", "#review-period"),
            ("Evidence window, From: Choose the day the window starts.", "#review-from"),
            ("Evidence window, To: Choose the day the window ends.", "#review-to"));
        summary.TextContent.Should().NotContain("Trainee", "no line points at the disabled Trainee (S6)");
        summary.TextContent.Should().NotContain("review type", "F4: the type is preselected, never refused");

        var panel = cut.Find("#review-panel");
        panel.GetAttribute("aria-invalid").Should().Be("true");
        panel.ClassList.Should().Contain("invalid", "Blazor's own mark, which app.css styles (DESIGN.md § Form system)");
        panel.GetAttribute("aria-describedby").Should().Be("review-panel-help review-panel-msg schedule-refusal");
        cut.Find("#review-panel-msg").TextContent.Should().Be("Choose a panel.");

        // The summary sits above the actions, and takes the focus once drawn.
        summary.NextElementSibling!.ClassList.Should().Contain("form-actions");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.SummaryElement.Id));

        // A changed value drops its marks at once; the summary stays until the next press.
        cut.Find("#review-panel").Change(GeneralPanel.ToString());
        cut.FindAll("#review-panel-msg").Should().BeEmpty();
        cut.Find("#review-panel").HasAttribute("aria-invalid").Should().BeFalse();
        cut.FindAll("#schedule-refusal").Should().ContainSingle();
    }

    [Fact]
    public void ASummaryLine_MovesTheFocusToItsField()
    {
        var cut = RenderPage();
        cut.Find("form").Submit();

        cut.Find("#schedule-refusal li a").Click();

        JSInterop.Invocations.Where(call => call.Identifier == PageFocus.FocusByIdIdentifier)
            .Select(call => call.Arguments[0]).Should().Contain("review-panel");
    }

    [Fact]
    public void AWindowThatEndsBeforeItStarts_IsRefusedBesideTo_InWombatsWords()
    {
        var cut = RenderPage();
        Choose(cut, "2026-2");
        cut.Find("#review-to").Change("2025-12-31");

        cut.Find("form").Submit();

        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().BeEmpty();
        cut.Find("#review-to-msg").TextContent.Should().Be("The window ends before it starts. Choose a day on or after 2026-01-01.");
        Text(cut.Find("#schedule-refusal li")).Should().Be(
            "Evidence window, To: The window ends before it starts. Choose a day on or after 2026-01-01.");
    }

    [Fact]
    public void TheSeatTakenMeanwhile_IsRefusedInTheHandlersWords_AndTheAsideTurnsToAlreadyHeld()
    {
        // The scratch state (C7, F5): a second tab took the seat after the page read it free.
        var held = Seat();
        var taken = false;
        _sender
            .On<GetSchedulingSeatQuery>(_ => taken ? held : null)
            .On<ScheduleCommitteeReviewCommand>(_ =>
            {
                taken = true;
                throw new CommitteeRefusedException(held.Sentence);
            });
        var cut = RenderPage();
        Choose(cut, "2026-2");

        cut.Find("form").Submit();

        var summary = cut.Find("#schedule-refusal");
        Text(summary).Should().StartWith($"The review was not created. Everything you chose is kept below. {held.Sentence}");
        var link = summary.QuerySelector("p a")!;
        link.GetAttribute("href").Should().Be("/committee/reviews/1");
        link.TextContent.Should().Be("Open review #1", "the summary links a short clause (S4)");
        Text(cut.Find("#agenda-preview-summary")).Should().StartWith("Already held.");
        cut.FindAll("button[type=submit]").Should().BeEmpty();
    }

    [Fact]
    public void AnUnexpectedFailure_IsAFixedSentence_NeverTheExceptionsText()
    {
        _sender.On<ScheduleCommitteeReviewCommand>(_ => throw new NullReferenceException("Object reference not set"));
        var cut = RenderPage();
        Choose(cut, "2026-2");

        cut.Find("form").Submit();

        Text(cut.Find("#schedule-refusal li")).Should().Be(ScheduleWords.CreateFailed);
        cut.Markup.Should().NotContain("Object reference");
    }

    [Fact]
    public void AFrameworksInvalidOperation_IsAFixedSentence_NeverItsMessage()
    {
        // T364, build review R4 (T338): the framework throws InvalidOperationException too; only the handler's own refusal
        // (CommitteeRefusedException) is shown in its words.
        _sender.On<ScheduleCommitteeReviewCommand>(_ => throw new InvalidOperationException("Sequence contains more than one element"));
        var cut = RenderPage();
        Choose(cut, "2026-2");

        cut.Find("form").Submit();

        Text(cut.Find("#schedule-refusal li")).Should().Be(ScheduleWords.CreateFailed);
        cut.Markup.Should().NotContain("Sequence contains");
    }

    [Fact]
    public void AScopeRefusal_ShowsInTheHandlersOwnWords()
    {
        // S4: "never the exception's text" is for unexpected failures; a handler's sentence still shows.
        const string NotSchedulable = "A review can only be scheduled on a panel of your institution that covers the trainee's programme.";
        _sender.On<ScheduleCommitteeReviewCommand>(_ => throw new UnauthorizedAccessException(NotSchedulable));
        var cut = RenderPage();
        Choose(cut, "2026-2");

        cut.Find("form").Submit();

        Text(cut.Find("#schedule-refusal li")).Should().Be(NotSchedulable);
    }

    // ---- Busy and created (S6) ------------------------------------------------------------------------------------------

    [Fact]
    public void WhileCreateRuns_ItReadsCreating_AndCancelIsADisabledButton_ThenTheReviewOpens()
    {
        var answer = new TaskCompletionSource<CommitteeReviewListItemDto>();
        _sender.OnAsync<ScheduleCommitteeReviewCommand>(_ => answer.Task);
        var cut = RenderPage();
        Choose(cut, "2026-2");

        cut.Find("form").Submit();

        var create = cut.Find("button[type=submit]");
        create.TextContent.Trim().Should().Be("Creating…");
        create.ClassList.Should().Contain("is-running");
        create.GetAttribute("aria-disabled").Should().Be("true");
        create.HasAttribute("disabled").Should().BeFalse("it keeps the focus (T234)");
        var cancel = cut.FindAll(".form-actions button").Single(button => button.TextContent.Trim() == "Cancel");
        cancel.HasAttribute("disabled").Should().BeTrue();
        cut.FindAll(".form-actions a").Should().BeEmpty("Cancel while busy is not a followable link");

        // A second press sends nothing.
        cut.Find("form").Submit();
        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().ContainSingle();

        answer.SetResult(Scheduled(77, _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Single()));
        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/committee/reviews/77"));
    }

    // ---- Loading and the load error (C11, T338) -------------------------------------------------------------------------

    [Fact]
    public void ALoadThatFails_SaysSoInFixedWords_WithTryAgain()
    {
        _sender.On<ListDecisionPanelsQuery>(_ => throw new TimeoutException("the database went away"));
        var cut = RenderComponent<ScheduleReview>();

        cut.WaitForState(() => cut.Markup.Contains(ScheduleWords.LoadFailure));
        cut.Find("h1").TextContent.Should().Be("Schedule a review");
        cut.Markup.Should().NotContain("went away");
        cut.FindAll("button").Should().Contain(button => button.TextContent.Trim() == "Try again");
        cut.FindAll("#review-panel").Should().BeEmpty();
    }

    // ---- Fixture --------------------------------------------------------------------------------------------------------

    private IRenderedComponent<ScheduleReview> RenderPage()
    {
        var cut = RenderComponent<ScheduleReview>();
        cut.WaitForState(() => cut.FindAll("#review-panel").Count == 1);
        return cut;
    }

    private static void Choose(IRenderedComponent<ScheduleReview> cut, string period, int panel = GeneralPanel)
    {
        cut.Find("#review-period").Change(period);
        cut.Find("#review-panel").Change(panel.ToString());
        cut.WaitForState(() => cut.FindAll($"select#review-trainee option[value='{Molefe}']").Count == 1);
        cut.Find("#review-trainee").Change(Molefe);
    }

    private static IElement CancelLink(IRenderedComponent<ScheduleReview> cut)
        => cut.FindAll(".form-actions a").Single(link => link.TextContent.Trim() == "Cancel");

    private static IReadOnlyList<string> TypeValues(IRenderedComponent<ScheduleReview> cut)
        => cut.FindAll("input[name='review-type']").Select(radio => radio.GetAttribute("value") ?? string.Empty).ToArray();

    private static string? Checked(IRenderedComponent<ScheduleReview> cut)
        => cut.FindAll("input[name='review-type']").SingleOrDefault(radio => radio.HasAttribute("checked"))?.GetAttribute("value");

    private static SchedulingSeatDto Seat()
        => new(
            1, "Paed Annual Review Panel", "2026 S2", new DateOnly(2026, 10, 12), "Lerato Molefe",
            "Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). " +
            "A trainee has one binding review for each period before the panels that decide the same EPAs, until it is " +
            "ratified: open that review, or ratify it before scheduling another.");

    private static CommitteeReviewListItemDto Scheduled(int id, ScheduleCommitteeReviewCommand command)
        => new(
            id, command.TraineeUserId, command.PanelId, "Paed Annual Review Panel",
            command.ReviewPeriodFrom, command.ReviewPeriodTo, command.ScheduledOn, CommitteeReviewState.Scheduled, null, null)
        {
            AcademicYear = command.AcademicYear,
            Semester = command.Semester
        };

    private static CommitteeAgendaPreviewDto Preview(int year, int semester)
    {
        var label = $"{year} S{semester}";
        return new CommitteeAgendaPreviewDto(
            year, semester, label, true,
            [
                new CommitteeAgendaLineDto(0, 1, "PAED-001", "Providing paediatric emergency care to children",
                    CommitteeAgendaLineOrigin.Cadence, year, semester, label, true, false, CommitteeAgendaLineState.Due,
                    CommitteeAgendaLineStatus.Due, false, false, null, 0)
            ],
            [],
            []);
    }

    private static string Text(IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));
            return this;
        }

        public RecordingSender OnAsync<TRequest>(Func<TRequest, Task<CommitteeReviewListItemDto>> answer)
        {
            _answers[typeof(TRequest)] = async request => await answer((TRequest)request);
            return this;
        }

        /// <summary>An answer the test settles itself, so a state while the request is in flight can be seen.</summary>
        public RecordingSender OnPending<TRequest>(Func<TRequest, Task<object?>> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => (TResponse)(await Answer(request))!;

        public async Task Send(IRequest request, CancellationToken cancellationToken = default) => await Answer(request);

        private Task<object?> Answer(object request)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
