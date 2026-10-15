using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.MultiSourceFeedback.MsfReportFixtures;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The MSF report in each of round 3's states (T369, flow 10, boards R1, R3, R4, R7, R7b, R8a, R8b, R9, R10, R11, R12, R13,
/// R14, R15; F1, F5, F7, F9, F10, M2, M5, M6, S15).
/// </summary>
public sealed class CampaignReportPageTests : WombatTestContext
{
    private readonly CampaignReportActionsTests.RatedLevelsReferenceData _referenceData = new();

    public CampaignReportPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
    }

    // ─── The header (F9) ─────────────────────────────────────────────────────

    [Fact]
    public void TheHeader_NamesTheTrainee_TheWindow_AndLinksTheCampaignPageOnItsOwnLine()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview));

        Text(cut.Find("h1")).Should().Be("MSF report for Lerato Molefe");
        Text(cut.Find(".page-subtitle span")).Should().Be("Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28");
        var link = cut.Find(".page-subtitle a.review-progress-link");
        Text(link).Should().Be("Open its campaign page");
        link.GetAttribute("href").Should().Be(MsfLinks.Campaign(CampaignId));
    }

    // ─── R1: open (F1) ───────────────────────────────────────────────────────

    [Fact]
    public void AnOpenCampaign_ShowsTheStatusCardAndTheChecklistOnly_NoGroup_AndCloseWithItsHelp()
    {
        var cut = Render(Report(MsfCampaignState.Open, totalResponses: 3, surviving: 0));

        var card = cut.Find("section.status-card");
        card.ClassList.Should().Contain("activity-status--others", "someone else's move while open (F7)");
        Text(card.QuerySelector(".badge")!).Should().Be("Open");
        Text(card.QuerySelector("h2.status-line")!).Should().Be("Still taking responses, until 2026-10-28.");
        Text(card.QuerySelector("p.field-help")!).Should().Be(
            "The responses are read once the campaign has closed and every address has been removed.");
        Checklist(cut).Should().Equal(
            ("is-short", "3 responses, of the 5 needed."),
            ("is-short", "0 respondent groups reporting, of the 2 needed."),
            ("is-rule", "A respondent group reports once 3 of it have responded; a group with fewer is suppressed."));
        cut.FindAll(".epa-lines li").Select(Text).Should().Equal(
            "PAED-010 — Leading and operating within a clinical team",
            "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams");
        Text(cut.Find(".epa-lines + p")).Should().Be(
            "One evidence record per EPA is written to the trainee's portfolio when the report is released.");
        Text(cut.Find(".status-actions p")).Should().Be(
            "Release is offered once the campaign has closed. It closes by itself once 2026-10-28 has passed; close it sooner here.");

        cut.FindAll(".group-card, .suppressed-line, .release-section").Should().BeEmpty("F1: nothing by group while open");
        cut.Markup.Should().NotContain("By respondent group");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ─── R3: under review, ready ─────────────────────────────────────────────

    [Fact]
    public void ACampaignUnderReview_ShowsTheGroupsInTheEnumsOrder_TheSuppressedLine_AndTheReleaseSectionLast()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview));

        var card = cut.Find("section.status-card");
        card.ClassList.Should().Contain("detail-card--emphasis", "the viewer's move (F7)");
        Text(card.QuerySelector("h2.status-line")!).Should().Be("Ready to release: every threshold is met.");
        Checklist(cut).Take(2).Should().Equal(
            ("is-met", "8 responses, of the 5 needed."), ("is-met", "2 respondent groups reporting, of the 2 needed."));
        card.QuerySelector(".status-note").Should().BeNull("a coordinator closed it, not the job");

        Text(cut.Find("#msf-groups-heading")).Should().Be("By respondent group");
        Text(cut.Find(".groups-intro")).Should().Be(
            "As Lerato Molefe will read it once released. Ratings are averaged from 1, Well below expectations, to 5, " +
            "Well above expectations.");
        cut.FindAll(".group-card h3").Select(Text).Should().Equal(
            "Consultant · 4.33 average rating, from 3 responses", "Nurse · 4.67 average rating, from 3 responses");
        Text(cut.Find(".suppressed-line")).Should().Be(
            "Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and " +
            "comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered.");

        var release = cut.Find("section.release-section");
        Text(release.QuerySelector("h2")!).Should().Be("Release to Lerato Molefe");
        release.QuerySelectorAll(".submit-check li").Select(Text).Should().Equal(
            "Lerato Molefe can read your narrative and the groups shown above on MSF reports.",
            "2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.",
            "Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports.",
            "It cannot be undone.");
        Text(cut.Find("#msf-narrative-count")).Should().Be("0 of 4,000 characters");
        cut.Find("#msf-narrative").GetAttribute("aria-describedby").Should().Be("msf-narrative-count");
        cut.FindAll("#msf-level option").Select(Text).Should().Equal("Not stated", "1", "2", "3a", "3b", "4", "5");
        var releaseButton = cut.Find("#msf-release-campaign");
        Text(releaseButton).Should().Be("Release to trainee…");
        releaseButton.HasAttribute("aria-disabled").Should().BeFalse();
        Text(cut.Find(".release-actions p")).Should().Be("If it should never be released, withdraw it on its campaign page.");
        cut.Find(".release-actions a.msf-inline-link").GetAttribute("href").Should().Be(MsfLinks.Campaign(CampaignId));

        // Status first, release last.
        var order = cut.FindAll("section").Select(section => section.ClassName ?? string.Empty).ToList();
        order.FindIndex(name => name.Contains("status-card")).Should().BeLessThan(order.FindIndex(name => name.Contains("msf-stack-card")));
        order.FindIndex(name => name.Contains("msf-stack-card")).Should().BeLessThan(order.FindIndex(name => name.Contains("release-section")));
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheNarrativeCounter_CountsWhatIsTyped_AndTheBrowsersScriptIsAskedToCountToo()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview));

        cut.Find("#msf-narrative").Change(new string('a', 106));

        Text(cut.Find("#msf-narrative-count")).Should().Be("106 of 4,000 characters");
        cut.Find("#msf-narrative").HasAttribute("maxlength").Should().BeFalse("a maxlength cuts a paste silently (DESIGN.md)");
        JSInterop.Invocations.Should().Contain(invocation =>
            invocation.Identifier == "import" && Equals(invocation.Arguments[0], "./js/review-appeal.js"));
    }

    [Fact]
    public void ANarrativeOverTheLimit_IsRefusedUnderTheField_AndOpensNoDialog()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview));
        cut.Find("#msf-narrative").Change(new string('a', 4001));

        cut.Find("#msf-release-campaign").Click();

        Text(cut.Find("#msf-narrative-count")).Should().Be("4,001 of 4,000 characters");
        Text(cut.Find("#msf-narrative-message")).Should().Be("Write the narrative in 4,000 characters or fewer.");
        var narrative = cut.Find("#msf-narrative");
        narrative.GetAttribute("aria-invalid").Should().Be("true");
        narrative.GetAttribute("aria-describedby").Should().Be("msf-narrative-count msf-narrative-message");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
    }

    // ─── R4: closed by the job (F5) ──────────────────────────────────────────

    [Fact]
    public void ACampaignClosedByTheJob_SaysItClosedByItself()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview) with { ClosedByUserId = null });

        Text(cut.Find(".status-card .status-note")).Should().Be("Closed by itself on 2026-10-14, once 2026-10-28 had passed.");
    }

    // ─── R7, R7b: released ───────────────────────────────────────────────────

    [Fact]
    public void AReleasedReport_ShowsItsOneLiner_WhatWasRecorded_TheGroupsAsSheReadsThem_AndWhatWasReleased()
    {
        var report = Report(MsfCampaignState.Released) with
        {
            EvidenceRecordedOn = ReleasedOn,
            CoordinatorNarrative = "Colleagues describe a careful registrar.",
            CoveredEpas = [Paed010 with { Recorded = true }, Paed012 with { Recorded = false, WillBeRecorded = false }]
        };
        var cut = Render(report);

        var card = cut.Find("section.status-card");
        card.ClassList.Should().Contain("activity-status--done");
        Text(card.QuerySelector(".status-line")!).Should().Be("The report has been released to the trainee.");
        card.QuerySelectorAll(".report-line > span:not(.sep)").Select(Text).Should().Equal(
            "2026-10-14 16:34 SAST", "8 responses, with 5 needed", "2 respondent groups reporting, with 2 needed");
        card.QuerySelectorAll(".report-line .sep").Should().OnlyContain(sep => sep.GetAttribute("aria-hidden") == "true");
        cut.FindAll(".threshold-list").Should().BeEmpty();
        cut.FindAll(".epa-lines li").Select(Text).Should().Equal(
            "PAED-010 — Leading and operating within a clinical team",
            "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams: not recorded");
        Text(cut.Find(".epa-lines + p")).Should().Be("Recorded on the portfolio: 2026-10-14 16:34 SAST");

        Text(cut.Find(".groups-intro")).Should().StartWith("As Lerato Molefe reads it.");
        Text(cut.Find(".suppressed-line")).Should().Be(
            "Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so their ratings and " +
            "comments are not shown here.");
        cut.FindAll(".stored dt, .stored dd").Select(Text).Should().Equal(
            "Narrative", "Colleagues describe a careful registrar.", "Supervision level", "Not stated");
        cut.FindAll(".release-section, #msf-close-campaign").Should().BeEmpty();
    }

    // ─── R8a, R8b, R9: blocked; no EPA ───────────────────────────────────────

    /// <summary>R8a, F10: Anele Dlamini's campaign, one Consultant who answered; the reason ends with the link (S15).</summary>
    [Fact]
    public void AReleaseShortOfResponses_IsAriaDisabled_WithItsReasonBesideIt_EndingWithTheWithdrawLink()
    {
        var report = Report(MsfCampaignState.UnderReview, totalResponses: 1, surviving: 0,
            groups: [Suppressed(MsfRespondentCategory.Consultant, 1)], epas: [Paed012]) with { SubjectName = "Anele Dlamini" };
        var cut = Render(report);

        Text(cut.Find("h1")).Should().Be("MSF report for Anele Dlamini");
        Text(cut.Find(".status-line")).Should().Be("Not ready to release.");
        Checklist(cut).Take(2).Should().Equal(
            ("is-short", "1 response, of the 5 needed."), ("is-short", "0 respondent groups reporting, of the 2 needed."));
        cut.FindAll(".group-card").Should().BeEmpty();
        Text(cut.Find(".suppressed-line")).Should().Be(
            "Suppressed: Consultant. Fewer than 3 responses, so its rating and comment are not shown here. Anele Dlamini " +
            "sees nothing of it, not even that it answered.");

        var release = cut.Find("#msf-release-campaign");
        release.GetAttribute("aria-disabled").Should().Be("true");
        release.GetAttribute("aria-describedby").Should().Be("msf-release-reason");
        Text(cut.Find("#msf-release-reason")).Should().Be(
            "Release to trainee: Release needs 5 responses; 1 has come back. If it should never be released, withdraw it " +
            "on its campaign page.");
        cut.Find("#msf-release-reason a.msf-inline-link").GetAttribute("href").Should().Be(MsfLinks.Campaign(CampaignId));
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    /// <summary>R8b and check item 2: the groups' reason keeps the link too.</summary>
    [Fact]
    public void AReleaseShortOfGroups_SaysSo_AndItsReasonKeepsTheWithdrawLink()
    {
        var report = Report(MsfCampaignState.UnderReview, totalResponses: 6, surviving: 1,
            groups: [Shown(MsfRespondentCategory.Nurse, 4.0, "a", "b", "c"), Suppressed(MsfRespondentCategory.Consultant, 2)]);
        var cut = Render(report);

        Checklist(cut).Take(2).Should().Equal(
            ("is-met", "6 responses, of the 5 needed."), ("is-short", "1 respondent group reporting, of the 2 needed."));
        Text(cut.Find("#msf-release-reason")).Should().Be(
            "Release to trainee: Release needs 2 respondent groups to report; 1 cleared the 3-response threshold. " +
            "Feedback that survives from too few groups is not multi-source feedback. If it should never be released, " +
            "withdraw it on its campaign page.");
        cut.Find("#msf-release-reason a.msf-inline-link");
    }

    /// <summary>R9, R6c: a campaign naming no EPA; the records line drops, the dialog says none is recorded.</summary>
    [Fact]
    public void ACampaignNamingNoEpa_SaysSo_AndItsReleaseRecordsNone()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview, epas: []));

        Text(cut.Find(".status-card div > p")).Should().Be(
            "This campaign names no EPA, so releasing it will record no evidence on the trainee's portfolio.");
        cut.FindAll(".release-section .submit-check li").Select(Text).Should().NotContain(line => line.Contains("evidence record"));

        cut.Find("#msf-release-campaign").Click();
        cut.FindAll("dialog .dialog-body li").Select(Text).Should().Contain("No evidence is recorded.");
    }

    /// <summary>R6b, M6: an EPA that has left the trainee's curriculum is named as not to be recorded, before release; D3: by name.</summary>
    [Fact]
    public void AnEpaThatWillNotBeRecorded_IsNamed_InTheChecklistTheReleaseSectionAndTheDialog()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview, epas: [Paed010, Paed012 with { WillBeRecorded = false }]));

        Text(cut.Find(".epa-lines + p")).Should().Be(
            "PAED-012 is no longer on Lerato Molefe's curriculum and will not be recorded. One evidence record per EPA is " +
            "written to " +
            "the trainee's portfolio when the report is released.");
        cut.FindAll(".release-section .submit-check li").Select(Text).Should().Contain(
            "1 evidence record is written to Lerato Molefe's portfolio, for PAED-010; PAED-012 is no longer on Lerato " +
            "Molefe's curriculum and is not recorded.");

        cut.Find("#msf-release-campaign").Click();
        cut.FindAll("dialog .dialog-body li").Select(Text).Should().Contain(
            "1 evidence record is written to Lerato Molefe's portfolio: PAED-010. PAED-012: not recorded, no longer on " +
            "Lerato Molefe's curriculum.");
    }

    // ─── R10, R6d: her programme has ended (M5, F4) ──────────────────────────

    [Fact]
    public void AReportWhoseTraineesProgrammeEnded_WarnsKeepsTheRelease_AndSendsNoMail()
    {
        var ended = new DateOnly(2026, 10, 10);
        var cut = Render(Report(MsfCampaignState.UnderReview) with { SubjectIsCurrent = false, SubjectEndedOn = ended });

        var warning = cut.Find(".status-card .alert-warning");
        Text(warning).Should().Be(MsfCampaignText.ProgrammeEnded(TraineeName, ended));
        warning.HasAttribute("role").Should().BeFalse("standing content");
        Text(cut.Find(".groups-intro")).Should().StartWith("As Lerato Molefe would have read it.");
        cut.FindAll(".release-section .submit-check li").Select(Text).Should().Equal(
            "Lerato Molefe can no longer open MSF reports; the report is kept with the evidence on Lerato Molefe's record.",
            "2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012.",
            "No email is sent: Lerato Molefe can no longer open MSF reports.",
            "It cannot be undone.");
        cut.Find("#msf-release-campaign").GetAttribute("aria-disabled").Should().BeNull("Q10: warn, and keep the release");

        cut.Find("#msf-release-campaign").Click();
        var dialog = cut.FindAll("dialog").Single(element => element.TextContent.Contains("Release this report"));
        Text(dialog.QuerySelector(".dialog-body > p")!).Should().Be(MsfCampaignText.ProgrammeEnded(TraineeName, ended));
        dialog.QuerySelectorAll(".dialog-body li").Select(Text).Should().Equal(
            "Lerato Molefe can no longer open MSF reports; the report is kept with the evidence on Lerato Molefe's record.",
            "2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012.",
            "Supervision level: Not stated.",
            "No email is sent: Lerato Molefe can no longer open MSF reports.");
    }

    // ─── R11: a dozen comments ───────────────────────────────────────────────

    [Fact]
    public void AGroupWithADozenComments_ShowsEveryOneVerbatim_TheLongestWhole()
    {
        var comments = Enumerable.Range(1, 11).Select(index => $"Comment {index}").Append(new string('x', 4000)).ToArray();
        var cut = Render(Report(MsfCampaignState.UnderReview, totalResponses: 17, groups:
        [
            Shown(MsfRespondentCategory.Consultant, 4.0, "a", "b", "c"),
            Shown(MsfRespondentCategory.Nurse, 4.5, comments)
        ]));

        var nurse = cut.FindAll(".group-card").ElementAt(1);
        Text(nurse.QuerySelector("h3")!).Should().Be("Nurse · 4.50 average rating, from 12 responses");
        nurse.QuerySelectorAll(".group-comments li").Should().HaveCount(12);
        nurse.QuerySelectorAll(".group-comments li").Last().TextContent.Should().HaveLength(4000);
    }

    [Fact]
    public void AGroupOfOne_ReadsFromOneResponse()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview) with
        {
            MinimumCategoryResponses = 1,
            Categories = [Shown(MsfRespondentCategory.Consultant, 3.0, "Only one.")]
        });

        Text(cut.Find(".group-card h3")).Should().Be("Consultant · 3.00 average rating, from 1 response");
        Checklist(cut).Last().Should().Be(
            ("is-rule", "A respondent group reports once 1 of it has responded; a group with fewer is suppressed."));
    }

    // ─── R15: fifteen EPAs ───────────────────────────────────────────────────

    [Fact]
    public void FifteenEpas_AreEachListedWhole()
    {
        var epas = Enumerable.Range(1, 15).Select(index => Epa(100 + index, $"PAED-{index:000}", $"Title {index}")).ToList();
        var cut = Render(Report(MsfCampaignState.UnderReview, epas: epas));

        cut.FindAll(".epa-lines li").Should().HaveCount(15);
        cut.FindAll(".release-section .submit-check li").Select(Text).Should().Contain(line =>
            line.StartsWith("15 evidence records are written", StringComparison.Ordinal) && line.EndsWith("PAED-014 and PAED-015.", StringComparison.Ordinal));
    }

    // ─── Learner feedback (T164, M4) ─────────────────────────────────────────

    [Fact]
    public void ALearnerFeedbackReport_CountsAndNamesItsContexts_AndOffersNoLevel()
    {
        var cut = Render(Report(MsfCampaignState.UnderReview) with
        {
            Kind = MsfTemplateKind.LearnerFeedback,
            TeachingContextCount = 2,
            TeachingContextsResponded = ["Student tutorial", "Ward round"]
        });

        cut.FindAll(".status-card .status-note").Select(Text).Should().Contain(
            "Teaching contexts that responded: 2 (Student tutorial, Ward round)");
        Text(cut.Find(".page-subtitle span")).Should().Contain("Learner feedback");
        cut.FindAll("#msf-level").Should().BeEmpty("learner_feedback_cpsa rates nothing");
        _referenceData.AskedFor.Should().Equal("learner_feedback_cpsa");

        cut.Find("#msf-release-campaign").Click();
        cut.FindAll("dialog .dialog-body li").Select(Text).Should().NotContain(line => line.StartsWith("Supervision level", StringComparison.Ordinal));
        cut.FindAll("dialog .dialog-body > p").Select(Text).Should().Contain(
            "Releasing cannot be undone: the narrative cannot be changed afterwards.");
    }

    // ─── R12, R13, R14 ───────────────────────────────────────────────────────

    [Fact]
    public void AnUnknownOrOutOfScopeId_IsReportUnavailable_WithAWayBackToTheCampaigns()
    {
        var sender = new NullReportSender();
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Report unavailable"));
        Text(cut.Find(".state-panel-copy")).Should().Be("The selected report could not be loaded.");
        var back = cut.Find(".detail-card--empty a.btn");
        Text(back).Should().Be("Go to MSF campaigns");
        back.GetAttribute("href").Should().Be(MsfLinks.Campaigns);
    }

    [Fact]
    public void WhileLoading_TheHeaderReadsMsfReport_AndAStatusSaysSo()
    {
        var sender = new HeldReportSender();
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        Text(cut.Find("h1")).Should().Be("MSF report");
        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("Loading the report.");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
    }

    [Fact]
    public void AFailedRead_IsAFixedSentenceWithTryAgain_AndTryAgainReadsAgain()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview))
        {
            FirstReadFailure = new InvalidOperationException("Npgsql: connection refused.")
        };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(MsfReportText.LoadFailed));
        Text(cut.Find("h1")).Should().Be("MSF report");
        cut.Markup.Should().NotContain("Npgsql");

        cut.Find(".alert-row button").Click();

        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("MSF report for Lerato Molefe"));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<CampaignReport> Render(MsfCampaignAggregateReportDto report)
    {
        Services.AddSingleton<IScopedSender>(new MsfReportSender(report));
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll(".status-card").Count > 0);
        return cut;
    }

    private static IReadOnlyList<(string Kind, string Text)> Checklist(IRenderedComponent<CampaignReport> cut)
        => cut.FindAll(".threshold-list li").Select(item => (item.ClassName ?? string.Empty, Text(item))).ToList();

    private sealed class NullReportSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetCampaignAggregateReportQuery
                ? Task.FromResult(default(TResponse)!)
                : throw new NotSupportedException();

        public Task Send(MediatR.IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class HeldReportSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => new TaskCompletionSource<TResponse>().Task;

        public Task Send(MediatR.IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
