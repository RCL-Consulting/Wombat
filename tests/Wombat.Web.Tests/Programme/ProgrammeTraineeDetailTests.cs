using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Programme;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Programme;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.Progress.ProgressFixtures;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// The registrar page (T358, flow 06, lane C; R2-Registrar r1–r8; C2, C8, C9; D8; review 6, 7, 8, 30; round-3-check 2, 4):
/// every state the board draws. Typical at Step 3.56's figures; no rating yet with Send a reminder; the reminder's dialog,
/// result and refusal in the section; with STARs and a review; each section's own error and Try again; loading with the
/// header; the page error; ended; not found. The standing's names move to the in-page charts, and no word names a person
/// by a pronoun.
/// </summary>
/// <remarks>
/// The day is 2026-10-04 (D). Nomsa Mahlangu is profile 12, user "mahlangu", training year 1. The trail, Home › Programme
/// trainees › the name, comes with lane B's owner row (NavOwners); it is PageHeader's, so not held here.
/// </remarks>
public sealed class ProgrammeTraineeDetailTests : WombatTestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";
    private const string Secret = "Npgsql: the connection string was rejected.";
    private const int Profile = 12;
    private static readonly DateOnly Today = new(2026, 10, 4);
    private static readonly DateTime UpdatedOn = new(2026, 9, 26, 6, 10, 0, DateTimeKind.Utc);

    private readonly FakeSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public ProgrammeTraineeDetailTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("zulu@kgk.test");
        _auth.SetRoles(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "zulu"));
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 10, 4, 8, 0, 0, TimeSpan.Zero)));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── r1: typical (Step 3.52b, 3.56) ──────────────────────────────────────

    [Fact]
    public void Typical_NamesTheRegistrar_AndDrawsTheSixSectionsInOrder()
    {
        Answer(trajectories: [Paed004Trajectory()]);

        var cut = Render();

        Text(cut.Find("h1")).Should().Be("Nomsa Mahlangu");
        TabTitle.Of(this, cut).Should().Be("Nomsa Mahlangu · Wombat");
        Text(cut.Find(".page-subtitle")).Should().Be("Training year 1 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics");

        var stack = cut.Find("div.registrar-stack");
        stack.Children.Select(section => section.LocalName).Should().AllBe("section");
        stack.Children.Select(section => section.QuerySelector("h2")!.Id).Should().Equal(
            "period-h", "epas-h", "standing-h", "trajectories-h", "waiting-h", "reviews-h");
        stack.Children.Select(section => section.GetAttribute("aria-labelledby")).Should().Equal(
            "period-h", "epas-h", "standing-h", "trajectories-h", "waiting-h", "reviews-h");
        stack.QuerySelectorAll("h2").Should().OnlyContain(heading => heading.ClassList.Contains("epa-section-title"));
        stack.QuerySelectorAll("h2").Select(Text).Should().Equal(
            "This period", "EPAs", "Entrustment against Annexure A", "Rating trajectories", "Waiting for assessors", "Committee reviews");
    }

    [Fact]
    public void ThisPeriod_IsFlow05sFigures_AndTheEndsLine_InStaffWords()
    {
        Answer();

        var cut = Render();

        var period = cut.Find("section.detail-card.period-card");
        Metrics(period).Should().Equal(("0 of 2", "EPAs met this semester"), ("0 of 1", "EPAs met in 2026"));
        Text(period.QuerySelector("p.period-ends")!).Should().Be(
            "Semester 2, 2026 ends on 2026-11-30. Training year 1 sets the minimum level each encounter is judged against.");
    }

    [Fact]
    public void TheEpas_AreTheIndex_WithItsNamesAsText_AndItsStarColumn()
    {
        // Review 7: the EPA pages are the registrar's own; on this page every name is text.
        Answer();

        var cut = Render();

        var epas = cut.Find("section.index-section");
        epas.QuerySelectorAll("table.clinic-table--index").Should().HaveCount(2);
        epas.QuerySelectorAll("a").Should().BeEmpty();
        Text(epas.QuerySelector("tbody th")!).Should().Be("PAED-001 — Providing paediatric emergency care to children");
        epas.QuerySelector("thead th:last-child")!.TextContent.Should().Be("STAR against training year 1");
    }

    [Fact]
    public void TheStanding_IsThePanelWhole_ForStaff_AndAChartedEpasNameMovesToItsChart()
    {
        // D8, r1: PAED-004 has a chart on this page, so its name links there; PAED-001 has none, so it is text.
        Answer(trajectories: [Paed004Trajectory(), Trajectory(1, "PAED-001")]);

        var cut = Render();

        var standing = cut.Find("section.standing-panel");
        Text(standing.QuerySelector("p")!).Should().StartWith("In training year 1 on 2026-10-04.");
        var charted = standing.QuerySelectorAll("tbody th a.epa-link").Should().ContainSingle().Which;
        charted.GetAttribute("href").Should().Be("#trajectory-4");
        Text(charted).Should().StartWith("PAED-004");
        cut.Find("section#trajectory-4");

        charted.Click();

        JSInterop.Invocations.Should().Contain(invocation =>
            invocation.Identifier == PageFocus.FocusByIdIdentifier && (string)invocation.Arguments[0]! == "trajectory-4");
    }

    [Fact]
    public void TheTrajectories_AreAChartPerRatedEpa_EachAnH3_OverTheAcademicYear_NamingTheRegistrar()
    {
        Answer(trajectories: [Paed004Trajectory(), Trajectory(1, "PAED-001")]);

        var cut = Render();

        var section = cut.Find("section.list-section");
        var card = section.QuerySelector("section.trajectory-card#trajectory-4")!;
        Text(card.QuerySelector("h3")!).Should().Be("PAED-004 — Managing common neonatal conditions");
        Text(card.QuerySelector(".trajectory-head p")!).Should().Be(
            "Nomsa Mahlangu · 1 rating in the 2026 academic year, from Thandi Zulu. At the minimum.");
        section.QuerySelectorAll("section.trajectory-card").Should().ContainSingle("an EPA with no rating has no chart");
        card.QuerySelector(".trajectory-chart-today").Should().BeNull("no Today rule (D8)");

        var query = _sender.Received.OfType<GetEpaTrajectoryForTraineeQuery>().Single();
        (query.TraineeUserId, query.From, query.To, query.EpaId).Should().Be(("mahlangu", (DateOnly?)new DateOnly(2026, 1, 1), (DateOnly?)new DateOnly(2026, 12, 31), (int?)null));
    }

    [Fact]
    public void NothingWaiting_AndNoReview_SayTheRegistrarsName()
    {
        Answer();

        var cut = Render();

        Text(Section(cut, "waiting-h").QuerySelector("p.card-empty")!).Should().Be("Nothing of Nomsa Mahlangu's waits for an assessor.");
        Text(Section(cut, "reviews-h").QuerySelector("p.card-empty")!).Should().Be("No review is scheduled for Nomsa Mahlangu.");
    }

    [Fact]
    public void EveryRead_IsTheRegistrarsAsTheRoleReadAs()
    {
        // C2, D2, E4: the registrar first, as the role read as (a Committee member acting as Assessor reads as Committee
        // member), then flow 05's reads by the user id it returned.
        Answer();

        Render(acting: WombatRoles.Assessor);

        var registrar = _sender.Received.OfType<GetProgrammeTraineeQuery>().Single();
        (registrar.ActingRole, registrar.ProfileId).Should().Be((WombatRoles.CommitteeMember, Profile));
        _sender.Received.OfType<GetCurriculumProgressForTraineeQuery>().Single().TraineeUserId.Should().Be("mahlangu");
        _sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Single().AsOf.Should().BeNull();
        var waiting = _sender.Received.OfType<ListWaitingForAssessorsQuery>().Single();
        (waiting.ActingRole, waiting.SubjectUserId).Should().Be((WombatRoles.CommitteeMember, "mahlangu"));
        _sender.Received.OfType<ListReviewsForProgrammeTraineeQuery>().Single().TraineeUserId.Should().Be("mahlangu");
        _sender.Received.OfType<GetMsfCoverageForTraineeQuery>().Should().BeEmpty("only an ended record reads each period's MSF");
    }

    // ─── r2: no rating yet, and Send a reminder (Step 3.30) ──────────────────

    [Fact]
    public void AtStep330_NoChartYet_AndTheWaitingRequestOffersSendAReminder()
    {
        SignInAs(WombatRoles.Coordinator);
        Answer(waiting: Waiting(mayRemind: true, Request()));

        var cut = Render(acting: WombatRoles.Coordinator);

        Text(Section(cut, "trajectories-h").QuerySelector("p.card-empty")!).Should().Be("No ratings yet in the 2026 academic year.");
        cut.FindAll("section.standing-panel a.epa-link").Should().BeEmpty("nothing is charted, so every name is text");

        var waiting = Section(cut, "waiting-h");
        waiting.ClassList.Should().Contain(["detail-card", "detail-card--warning"]);
        Text(waiting.QuerySelector("h2 .badge")!).Should().Be("1 waiting, 1 overdue");
        Text(waiting.QuerySelector("p.needs-you-rule")!).Should().Be("Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5.");
        var row = waiting.QuerySelector("li.needs-you-row.needs-you-row--overdue")!;
        row.QuerySelectorAll("p.needs-you-why").Select(Text).Should().Equal("With Thandi Zulu", "Waiting 8 days");
        var button = row.QuerySelector("button.reminder-action")!;
        Text(button).Should().Be("Send a reminder");
        button.GetAttribute("aria-label").Should().Be(
            "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu");
    }

    [Fact]
    public void ACommitteeMember_ReadsTheWaitingRequest_WithNoReminder()
    {
        Answer(waiting: Waiting(mayRemind: false, Request()));

        var cut = Render();

        Section(cut, "waiting-h").QuerySelector("li.needs-you-row").Should().NotBeNull();
        cut.FindAll("button.reminder-action").Should().BeEmpty();
    }

    // ─── r2d, r2r: the dialog and the result ─────────────────────────────────

    [Fact]
    public void TheReminder_OpensItsDialog_AndItsResultIsShownInTheSection_TakingTheFocus_TheRowReadAgain()
    {
        SignInAs(WombatRoles.Coordinator);
        var reads = 0;
        _sender.On<ListWaitingForAssessorsQuery>(_ => ++reads == 1
            ? Waiting(mayRemind: true, Request())
            : Waiting(mayRemind: true, Request() with { RemindedToday = true, LastReminder = new ActivityReminderDto(UpdatedOn, Today, "Pieter Smit") }));
        _sender.On<SendActivityReminderCommand>(_ => Sent());
        Answer(waiting: null);

        var cut = Render(acting: WombatRoles.Coordinator);
        cut.Find("button.reminder-action").Click();

        Text(cut.Find("dialog h2")).Should().Be("Send Thandi Zulu a reminder?");
        cut.Find("dialog button.btn-primary").Click();

        var command = _sender.Received.OfType<SendActivityReminderCommand>().Single();
        (command.ActingRole, command.ActivityId, command.ExpectedState, command.ExpectedUpdatedOn).Should().Be(
            (WombatRoles.Coordinator, 42, "requested", UpdatedOn));
        var waiting = Section(cut, "waiting-h");
        var alert = waiting.QuerySelector(".action-result .alert.alert-success")!;
        alert.GetAttribute("role").Should().Be("status");
        Text(alert).Should().Be(
            "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu, " +
            "waiting 8 days. The request is still Requested; its wait is unchanged.");
        reads.Should().Be(2, "the rows are read again after the answer");
        waiting.QuerySelector("button.reminder-action").Should().BeNull("the button gives way to the Reminded line");
        Text(waiting.QuerySelector(".needs-you-row .progress-row-meta")!).Should().Be("Reminded 2026-10-04 by Pieter Smit");
        cut.WaitForAssertion(() => FocusedReference().Should().Be(cut.Instance.WaitingResultElement.Id));
    }

    // ─── r2x: a refusal ──────────────────────────────────────────────────────

    [Fact]
    public void ARefusal_IsSaidInTheSection_AsAnAlert_AndTheRowStands()
    {
        SignInAs(WombatRoles.Coordinator);
        _sender.On<SendActivityReminderCommand>(_ => new SendActivityReminderResult(
            ReminderOutcome.NoEmail, "Thandi Zulu", "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8, "Requested", null, null));
        Answer(waiting: Waiting(mayRemind: true, Request()));

        var cut = Render(acting: WombatRoles.Coordinator);
        cut.Find("button.reminder-action").Click();
        cut.Find("dialog button.btn-primary").Click();

        var alert = Section(cut, "waiting-h").QuerySelector(".action-result .alert.alert-danger")!;
        alert.GetAttribute("role").Should().Be("alert");
        Text(alert).Should().Be("Not sent. Thandi Zulu has no email address in Wombat. Ask your institutional admin to add one.");
        cut.Find("button.reminder-action");
    }

    [Fact]
    public void ASendThatFails_IsTheFixedWords_InTheSection()
    {
        SignInAs(WombatRoles.Coordinator);
        _sender.On<SendActivityReminderCommand>(_ => throw new InvalidOperationException(Secret));
        Answer(waiting: Waiting(mayRemind: true, Request()));

        var cut = Render(acting: WombatRoles.Coordinator);
        cut.Find("button.reminder-action").Click();
        cut.Find("dialog button.btn-primary").Click();

        Text(Section(cut, "waiting-h").QuerySelector(".action-result .alert.alert-danger")!).Should().Be(ReminderWords.Failed);
        cut.Markup.Should().NotContain(Secret);
    }

    // ─── r3: with STARs, and a review ────────────────────────────────────────

    [Fact]
    public void AReview_IsALinkToTheReviewItOpens_WithItsPeriodAndState()
    {
        var review = Review(31);
        Answer(reviews: [review]);

        var cut = Render();

        var item = Section(cut, "reviews-h").QuerySelector("ul.list-unstyled li.progress-row.progress-row--link")!;
        var link = item.QuerySelector("a.progress-row-link")!;
        link.GetAttribute("href").Should().Be("/committee/reviews/31");
        Text(link).Should().Be(RegistrarWords.ReviewLink(review));
        Text(item.QuerySelector("p.progress-row-meta")!).Should().Be("Semester 2, 2026 · Scheduled");
    }

    // ─── r4: each section's own load error ───────────────────────────────────

    [Fact]
    public void EachSection_FailsApart_InItsOwnSentence_WithItsOwnTryAgain_AndTheHeaderStands()
    {
        _sender
            .On<GetProgrammeTraineeQuery>(_ => Mahlangu())
            .On<GetCurriculumProgressForTraineeQuery>(_ => throw new InvalidOperationException(Secret))
            .On<GetEntrustmentStandingForTraineeQuery>(_ => throw new InvalidOperationException(Secret))
            .On<GetEpaTrajectoryForTraineeQuery>(_ => throw new InvalidOperationException(Secret))
            .On<ListWaitingForAssessorsQuery>(_ => throw new InvalidOperationException(Secret))
            .On<ListReviewsForProgrammeTraineeQuery>(_ => throw new InvalidOperationException(Secret));

        var cut = Render();

        Text(cut.Find("h1")).Should().Be("Nomsa Mahlangu");
        var errors = cut.FindAll("div.registrar-stack section .alert.alert-danger.section-error").ToList();
        errors.Select(alert => Text(alert.QuerySelector(".alert-row-text strong")!)).Should().Equal(
            "Could not load this period.", "Could not load the EPAs.", "Could not load the standing.",
            "Could not load the rating trajectories.", "Could not load what waits for an assessor.", "Could not load the committee reviews.");
        errors.Should().OnlyContain(alert => Text(alert.QuerySelector(".alert-row-text")!).EndsWith(
            "Nothing has changed. Try again, or come back in a few minutes.", StringComparison.Ordinal));
        errors.Should().OnlyContain(alert => alert.GetAttribute("role") == "alert" && Text(alert.QuerySelector("button")!) == "Try again");
        cut.Markup.Should().NotContain(Secret);
    }

    [Fact]
    public void ASectionsTryAgain_ReadsThatSectionAlone_AndItsHeadingTakesTheFocus()
    {
        var fail = true;
        Answer(reviews: []);
        _sender.On<ListReviewsForProgrammeTraineeQuery>(_ => fail ? throw new InvalidOperationException(Secret) : new[] { Review(31) });

        var cut = Render();
        var heading = cut.Find("h2#reviews-h").GetAttribute("blazor:elementreference");

        fail = false;
        Section(cut, "reviews-h").QuerySelector(".section-error button")!.Click();

        cut.WaitForAssertion(() => Section(cut, "reviews-h").QuerySelector("a.progress-row-link").Should().NotBeNull());
        _sender.Received.OfType<ListReviewsForProgrammeTraineeQuery>().Should().HaveCount(2);
        _sender.Received.OfType<GetCurriculumProgressForTraineeQuery>().Should().ContainSingle("only that section is read again");
        FocusedReference().Should().Be(heading);
    }

    // ─── r8: loading ─────────────────────────────────────────────────────────

    [Fact]
    public void WhileItLoads_TheHeaderStands_EachSectionsTitleOverSkeletons_AndTheStatusSaysSo()
    {
        var pending = new TaskCompletionSource<object?>();
        Answer();
        _sender.OnAsync<GetProgrammeTraineeQuery>(_ => pending.Task);

        var cut = Render(wait: false);

        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("Loading this registrar's progress.");
        Text(cut.Find("h1")).Should().Be("Programme trainee");
        var stack = cut.Find("div.registrar-stack[aria-busy=true]");
        stack.QuerySelectorAll("h2.epa-section-title").Select(Text).Should().Equal(
            "This period", "EPAs", "Entrustment against Annexure A");
        stack.QuerySelectorAll(".dashboard-card-skeleton .skeleton").Should().NotBeEmpty();
        stack.QuerySelectorAll("[style]").Should().BeEmpty("no inline sizes (round-3-check 5)");

        pending.SetResult(Mahlangu());
        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Nomsa Mahlangu"), AsyncWorkTimeout);
        Text(cut.Find("p.visually-hidden[role=status]")).Should().BeEmpty();
    }

    // ─── r5: the page's load error ───────────────────────────────────────────

    [Fact]
    public void ThePagesLoadError_IsProgrammeTrainee_TheFixedWords_AndTryAgain_IntoTheHeading()
    {
        var fail = true;
        Answer();
        _sender.On<GetProgrammeTraineeQuery>(_ => fail ? throw new InvalidOperationException(Secret) : Mahlangu());

        var cut = Render(waitFor: "Could not load this registrar");

        Text(cut.Find("h1")).Should().Be("Programme trainee");
        TabTitle.Of(this, cut).Should().Be("Programme trainee · Wombat");
        Text(cut.Find(".action-result .alert.alert-danger .alert-row-text")).Should().Be(
            "Could not load this registrar's progress. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain(Secret);

        fail = false;
        cut.Find(".action-result .alert-row button").Click();

        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Nomsa Mahlangu"));
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == PageFocus.FocusHeadingIdentifier);
    }

    // ─── r6: programme ended (read-only) ─────────────────────────────────────

    [Fact]
    public void AnEndedRegistrar_ReadsTheEndedRecord_InStaffWords_AsOnTheLastDay()
    {
        var ended = new ProgrammeEndDto(Completed: false, EndedOn: new DateOnly(2026, 10, 2), Today: Today);
        SignInAs(WombatRoles.SpecialityAdmin);
        Answer(
            registrar: DuPlessis(ended),
            summary: Summary(new DateOnly(2026, 10, 2), new DateOnly(2025, 1, 15), 2,
            [
                Item("PAED-001", QuotaPeriod.Semester, 3, Waived(Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3),
                    periods: [Waived(Semester2Of2026, QuotaWindowStatus.ExemptProgrammeEnded, 1, 3), Counting(Semester1Of2026, 3, 3, minimumReached: 2)])
                    with { EpaId = 1 }
            ]) with { Ended = ended },
            waiting: Waiting(mayRemind: true, Request() with { SubjectName = "Pieter du Plessis" }));

        var cut = Render(acting: WombatRoles.SpecialityAdmin);

        Text(cut.Find(".page-subtitle")).Should().Be("Training year 2 · Programme ended 2026-10-02 · Kgosi Kgari Teaching Hospital, Paediatrics");
        Text(cut.Find("div.registrar-stack > .alert.alert-info")).Should().Be(
            "Pieter du Plessis's programme ended on 2026-10-02, part-way through Semester 2, 2026. No target applies after " +
            "that; what follows is the record as it stood then, read-only.");
        cut.FindAll("section.period-card").Should().BeEmpty("the record stands in place of This period and EPAs");
        var record = cut.Find("section[aria-labelledby=ended-h]");
        Text(record.QuerySelector("h2#ended-h.epa-section-title")!).Should().Be("Curriculum targets");
        record.QuerySelectorAll("a").Should().BeEmpty("the EPA pages are the registrar's own");
        Text(record.QuerySelector("h3")!).Should().Be("The programme");
        Text(record).Should().Contain("2 when the programme ended").And.Contain("Minimum when the programme ended: 5.");
        Text(record.QuerySelector(".dashboard-grid dd")!).Should().Be("no target (the programme ended part-way through) · 1 recorded");
        cut.FindAll(".progress-bar").Should().BeEmpty("nothing is still to do");

        Text(Section(cut, "trajectories-h").QuerySelector("p.card-empty")!).Should().Be("No ratings in the 2026 academic year.");
        cut.Find("button.reminder-action");
        _sender.Received.OfType<GetEntrustmentStandingForTraineeQuery>().Single().AsOf.Should().Be(new DateOnly(2026, 10, 2));
        _sender.Received.OfType<GetMsfCoverageForTraineeQuery>().Single().To.Should().Be(new DateOnly(2026, 10, 2));
        WordsAboutThePeople(cut).Should().Match(text => !ProgrammeWordsFixtures.NamesAPersonByPronoun(text));
    }

    // ─── r7: not found ───────────────────────────────────────────────────────

    [Fact]
    public void AnIdThisRoleMayNotRead_IsFlow01sPageNotFound_AndReadsNothingElse()
    {
        Answer(notFound: true);

        var cut = Render(waitFor: "Page not found");

        Text(cut.Find("h1")).Should().Be("Page not found");
        TabTitle.Of(this, cut).Should().Be("Page not found · Wombat");
        cut.Find(".system-panel").QuerySelectorAll("p").Select(Text).Should().Equal(
            "There is no page at this address.", "Check the address, or start again from Home.");
        Text(cut.Find(".system-panel a.btn.btn-primary")).Should().Be("Go to Home");
        cut.FindAll(".registrar-stack").Should().BeEmpty();
        _sender.Received.Should().ContainSingle().Which.Should().BeOfType<GetProgrammeTraineeQuery>();
    }

    // ─── Words ───────────────────────────────────────────────────────────────

    [Fact]
    public void NoPronoun_NamesAPerson_OnTheTypicalPage_OrWithARequestWaiting()
    {
        SignInAs(WombatRoles.Coordinator);
        Answer(trajectories: [Paed004Trajectory()], waiting: Waiting(mayRemind: true, Request()), reviews: [Review(31)]);

        var cut = Render(acting: WombatRoles.Coordinator);

        WordsAboutThePeople(cut).Should().Match(text => !ProgrammeWordsFixtures.NamesAPersonByPronoun(text));
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static readonly ProgrammeScopeDto Kgk = new(
        WombatRoles.CommitteeMember, ProgrammeScopeKind.Institution, "Kgosi Kgari Teaching Hospital", 10, [], []);

    private static ProgrammeTraineeDto Mahlangu()
        => new(Profile, "mahlangu", "Nomsa Mahlangu", 1, "Kgosi Kgari Teaching Hospital", "Paediatrics", "Semester 2, 2026", null, Kgk);

    private static ProgrammeTraineeDto DuPlessis(ProgrammeEndDto ended)
        => new(Profile, "duplessis", "Pieter du Plessis", 2, "Kgosi Kgari Teaching Hospital", "Paediatrics", "Semester 2, 2026", ended, Kgk);

    /// <summary>Nomsa Mahlangu at 3.56: PAED-001 none, PAED-004 one of three, PAED-008 none of one.</summary>
    private static TraineeCurriculumProgressSummaryDto MahlanguSummary()
        => Summary(Today, new DateOnly(2026, 1, 15), 1,
        [
            Item("PAED-001", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 0, 3)) with { EpaId = 1, DecisionCadence = QuotaPeriod.Semester },
            Item("PAED-004", QuotaPeriod.Semester, 3, Counting(Semester2Of2026, 1, 3), title: "Managing common neonatal conditions") with { EpaId = 4, DecisionCadence = QuotaPeriod.Semester },
            Item("PAED-008", QuotaPeriod.AcademicYear, 1, Counting(Year2026, 0, 1), title: "Evaluating") with { EpaId = 8, DecisionCadence = QuotaPeriod.AcademicYear }
        ]);

    private static EntrustmentStandingDto Standing()
        => new(Today, new DateOnly(2026, 1, 15), 1, false,
            [StandingEpa(1, "PAED-001", "Providing paediatric emergency care to children"), StandingEpa(4, "PAED-004", "Managing common neonatal conditions")],
            new ExitRuleReadinessDto(2, 0, [new ExitLevelGroupDto(6, "5", 2, 0)], ["PAED-001", "PAED-004"]));

    private static EpaStandingDto StandingEpa(int epaId, string code, string title)
        => new(100 + epaId, epaId, code, title, "CPSA Paediatric Entrustment Scale v11.1", IsLocal: false, 3, "3a", false, 6, "5",
            null, EntrustmentStandingStatus.NoDecision, EntrustmentStandingStatus.NoDecision, null);

    private static EpaTrajectoryDto Paed004Trajectory()
        => Trajectory(4, "PAED-004", "Managing common neonatal conditions",
            new TrajectoryPointDto(77, new DateOnly(2026, 10, 1), true, 3, "3a", "Mini-CEX", "zulu")
            {
                AssessorName = "Thandi Zulu",
                ActivityName = "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01",
                TrainingYear = 1,
                MinimumLabel = "3a",
                AgainstMinimum = TrajectoryAgainstMinimum.AtOrAbove
            });

    private static EpaTrajectoryDto Trajectory(int epaId, string code, string title = "Providing paediatric emergency care to children", params TrajectoryPointDto[] points)
        => new(epaId, code, title, true, 42, "CPSA Paediatric Entrustment Scale v11.1", [], points)
        {
            ExitLevelOrder = 6,
            ExitLevelLabel = "5",
            MinimumSteps = [new TrajectoryMinimumStepDto(new DateOnly(2026, 1, 15), 1, 3, "3a")],
            WindowFrom = new DateOnly(2026, 1, 1),
            WindowTo = new DateOnly(2026, 12, 31)
        };

    private static ActivitySummaryDto Request()
        => ActivityRows.Waiting(
                42, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true, since: UpdatedOn, epaCode: "PAED-004",
                observedOn: new DateOnly(2026, 10, 1)) with
            {
                Holder = new ActivityHolderDto(ActivityHolderKind.Person, "zulu", "Thandi Zulu", false, UpdatedOn),
                NomineeName = "Thandi Zulu"
            };

    private static WaitingForAssessorsDto Waiting(bool mayRemind, params ActivitySummaryDto[] items)
        => new(Kgk, items, items.Length, items.Count(item => item.IsOverdue), items.Length, items.Count(item => item.IsOverdue),
            [new NomineeOptionDto("zulu", "Thandi Zulu")], 7, 5, 1, ProgrammeTraineeDetail.WaitingRows, mayRemind);

    private static CommitteeReviewListItemDto Review(int id)
        => new(id, "mahlangu", 3, "Paediatrics panel", new DateOnly(2026, 7, 1), new DateOnly(2026, 11, 30), new DateOnly(2026, 11, 20),
            CommitteeReviewState.Scheduled, null, null)
        {
            AcademicYear = 2026,
            Semester = 2
        };

    private static SendActivityReminderResult Sent()
        => new(ReminderOutcome.Sent, "Thandi Zulu", "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8,
            "Requested", null, new ActivityReminderDto(UpdatedOn, Today, "Pieter Smit"));

    /// <summary>Answers every read the page sends; a read already told (On) keeps its own answer.</summary>
    private void Answer(
        ProgrammeTraineeDto? registrar = null,
        TraineeCurriculumProgressSummaryDto? summary = null,
        IReadOnlyList<EpaTrajectoryDto>? trajectories = null,
        WaitingForAssessorsDto? waiting = null,
        IReadOnlyList<CommitteeReviewListItemDto>? reviews = null,
        bool notFound = false)
    {
        _sender
            .OnUnlessSet<GetProgrammeTraineeQuery>(_ => notFound ? null : registrar ?? Mahlangu())
            .OnUnlessSet<GetCurriculumProgressForTraineeQuery>(_ => summary ?? MahlanguSummary())
            .OnUnlessSet<GetEntrustmentStandingForTraineeQuery>(_ => Standing())
            .OnUnlessSet<GetEpaTrajectoryForTraineeQuery>(_ => trajectories ?? [])
            .OnUnlessSet<ListWaitingForAssessorsQuery>(_ => waiting ?? Waiting(mayRemind: false))
            .OnUnlessSet<ListReviewsForProgrammeTraineeQuery>(_ => reviews ?? [])
            .OnUnlessSet<GetMsfCoverageForTraineeQuery>(_ => null);
    }

    private void SignInAs(string role)
    {
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "smit"));
    }

    private IRenderedComponent<ProgrammeTraineeDetail> Render(string? acting = WombatRoles.CommitteeMember, bool wait = true, string? waitFor = null)
    {
        var held = acting switch
        {
            WombatRoles.CommitteeMember or WombatRoles.Assessor => new[] { WombatRoles.CommitteeMember, WombatRoles.Assessor },
            _ => [acting!]
        };
        var cut = RenderComponent<ProgrammeTraineeDetail>(parameters => parameters
            .Add(page => page.ProfileId, Profile)
            .AddCascadingValue(ActingRoleResolver.Resolve(acting, held)));
        if (wait)
        {
            cut.WaitForState(() => waitFor is null ? !cut.Markup.Contains(RegistrarWords.Loading) : cut.Markup.Contains(waitFor));
        }

        return cut;
    }

    private string? FocusedReference()
        => JSInterop.Invocations.Last(invocation => invocation.Identifier == FocusIdentifier)
            .Arguments[0].Should().BeOfType<ElementReference>().Which.Id;

    private static IElement Section(IRenderedFragment cut, string headingId)
        => cut.Find($"section[aria-labelledby={headingId}]");

    private static IReadOnlyList<(string Value, string Label)> Metrics(IElement card)
        => card.QuerySelectorAll(".dashboard-metric")
            .Select(metric => (Text(metric.QuerySelector(".dashboard-metric-value")!), Text(metric.QuerySelector(".dashboard-metric-label")!)))
            .ToList();

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string PageText(IRenderedFragment cut)
        => Regex.Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), @"\s+", " ").Trim();

    /// <summary>
    /// The page's words as staff read them about the registrar: the dialog quotes the mail's own subject, "Activities
    /// awaiting your assessment", which is the assessor's mail speaking to the assessor (lane A1), so it is set aside.
    /// </summary>
    private static string WordsAboutThePeople(IRenderedFragment cut)
        => PageText(cut).Replace("\"Activities awaiting your assessment\"", "\"…\"", StringComparison.Ordinal);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
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

        public FakeSender OnUnlessSet<TRequest>(Func<TRequest, object?> answer)
            => _answers.ContainsKey(typeof(TRequest)) ? this : On(answer);

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
