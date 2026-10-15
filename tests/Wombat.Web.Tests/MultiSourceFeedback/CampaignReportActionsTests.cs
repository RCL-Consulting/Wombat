using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.MultiSourceFeedback.MsfReportFixtures;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// Close campaign… and Release to trainee… on the report (T369, flow 10, boards R2, R3, R6a–R6d, R7, R16, R17; round 3
/// F8, M10): each confirms through a ConfirmDialog first; once it answers, the report is read again (T225), a result takes
/// the focus at the head of the page, and a refusal shows beside the button that sent it with the focus back on that
/// button, or heads the page once the button is gone.
/// </summary>
public sealed class CampaignReportActionsTests : WombatTestContext
{
    private const string TypedNarrative =
        "Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions";

    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    public CampaignReportActionsTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IActivityReferenceDataService>(new RatedLevelsReferenceData());
    }

    // ─── Close ───────────────────────────────────────────────────────────────

    [Fact]
    public void CloseCampaign_OpensADangerDialog_ThatCountsTheUnusedLinks_AndSendsNothingUntilConfirmed()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open));
        var cut = Render(sender);

        var close = cut.Find("#msf-close-campaign");
        Text(close).Should().Be("Close campaign…");
        close.ClassList.Should().Contain("btn-outline", "the red button belongs only in the dialog (F8)");
        close.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty();

        var dialog = CloseDialog(cut);
        Text(dialog.QuerySelector("h2")!).Should().Be("Close this campaign now?");
        dialog.QuerySelectorAll(".dialog-body p, .dialog-body li").Select(Text).Should().Equal(
            "Close the campaign for Lerato Molefe (Default MSF, closing 2026-10-28) before its last day?",
            "It takes no more responses: the 1 link not yet used stops working.",
            "Every respondent's email address is removed, so no link can be resent.",
            "The report keeps the 8 responses it has, for you to review and release.",
            "Closing cannot be undone.");
        dialog.QuerySelectorAll("button").Select(Text).Should().Equal("Keep it open", "Close campaign");
        dialog.QuerySelector("button.btn-danger").Should().NotBeNull("Close destroys: the links stop and the addresses go (F8)");
        dialog.Closest(".msf-page").Should().NotBeNull("inside the page's column, so the phone rules reach it (M10)");
    }

    [Fact]
    public void KeepItOpen_SendsNothing()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open));
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Keep it open").Click();

        sender.Commands.Should().BeEmpty();
        JSInterop.VerifyInvoke("wombatDialog.close");
    }

    [Fact]
    public void ClosingTheCampaign_ReadsItAgain_ShowsItUnderReviewWithItsGroups_AndTheResultTakesTheFocus()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open, totalResponses: 8));
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(
            "Campaign closed and anonymised for review. Every respondent's email address has been removed."));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<CloseMsfCampaignCommand>();
        sender.Reads.Should().Be(2, "the report is read again after the close");
        Text(cut.Find(".status-card .badge")).Should().Be("Under review");
        cut.FindAll("#msf-close-campaign").Should().BeEmpty();
        cut.FindAll(".group-card h3").Select(Text).Should().Equal(
            "Consultant · 4.33 average rating, from 3 responses", "Nurse · 4.67 average rating, from 3 responses");
        FocusWentTo(cut, cut.Instance.ResultRegion!.Value);
    }

    /// <summary>R16: refused on a campaign still open, the refusal sits beside Close campaign… and the focus goes back to it.</summary>
    [Fact]
    public void ACloseRefusedOnACampaignStillOpen_ShowsTheRefusalBesideTheButton_AndGivesItTheFocusBack()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new MsfRefusedException(CloseMsfCampaignCommandHandler.CampaignChanged)
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".status-actions .alert-danger")).Should().Be(
            "Not closed. " + CloseMsfCampaignCommandHandler.CampaignChanged));
        cut.Find(".status-actions .alert-danger").GetAttribute("role").Should().Be("alert");
        cut.FindAll(".action-result").Should().BeEmpty("the refusal is beside the button it answers");
        sender.Reads.Should().Be(2);
        cut.Find("#msf-close-campaign").HasAttribute("disabled").Should().BeFalse("for the retry");
        FocusWentTo(cut, cut.Instance.CloseButton);
    }

    /// <summary>T246: a close refused because the campaign closed elsewhere; Close is gone, so the refusal heads the page.</summary>
    [Fact]
    public void ACloseRefusedBecauseItClosedElsewhere_HeadsThePage_AndTakesTheFocus()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new MsfRefusedException(MsfCampaign.OnlyOpenCanBeClosed),
            StateAfterRefusal = MsfCampaignState.UnderReview
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Not closed. Only open campaigns can be closed."));
        Text(cut.Find(".status-card .badge")).Should().Be("Under review");
        cut.FindAll("#msf-close-campaign").Should().BeEmpty();
        FocusWentTo(cut, cut.Instance.ResultRegion!.Value);
    }

    [Fact]
    public void AnUnexpectedCloseFailure_IsAFixedSentence_NeverTheExceptionsText()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new TimeoutException("Npgsql: the connection pool is exhausted.")
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".status-actions .alert-danger")).Should().Be(
            "Not closed. " + MsfReportText.ActionFailed));
        cut.Markup.Should().NotContain("Npgsql");
    }

    /// <summary>
    /// T369, build review R1: the framework throws <see cref="InvalidOperationException" /> too (a sequence with no element,
    /// EF Core's own complaints), so only the campaign's own refusal type shows its words; this one is a failure, logged.
    /// </summary>
    [Fact]
    public void ACloseThatFailsWithAFrameworkInvalidOperation_IsAFixedSentence_NeverTheExceptionsText()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException("Sequence contains no elements.")
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".status-actions .alert-danger")).Should().Be(
            "Not closed. " + MsfCampaignText.ActionFailed));
        cut.Markup.Should().NotContain("Sequence contains");
    }

    [Fact]
    public void WhileACloseRuns_ASecondConfirmSendsNothing()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open)) { Hold = true };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        sender.Commands.Should().ContainSingle();
        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-success").Should().ContainSingle(), AsyncWorkTimeout);
        sender.Commands.Should().ContainSingle();
    }

    [Fact]
    public void ACloseTaken_ButTheReportCannotThenBeRead_KeepsTheResult_AndTheFailureIsThePanels()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.Open))
        {
            ReadFailureAfterCommand = new InvalidOperationException("The database could not be reached.")
        };
        var cut = Render(sender);

        cut.Find("#msf-close-campaign").Click();
        DialogButton(CloseDialog(cut), "Close campaign").Click();

        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-success").Should().ContainSingle());
        cut.Markup.Should().Contain(MsfReportText.LoadFailed).And.NotContain("could not be reached");
        cut.Find(".alert-row button").TextContent.Should().Contain("Try again");
    }

    // ─── Release ─────────────────────────────────────────────────────────────

    [Fact]
    public void ReleaseToTrainee_OpensAPrimaryDialog_ThatSaysWhatTheReleaseDoes_AndSendsNothingUntilConfirmed()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview));
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("#msf-release-campaign").Click();

        sender.Commands.Should().BeEmpty();
        var dialog = ReleaseDialog(cut);
        Text(dialog.QuerySelector("h2")!).Should().Be("Release this report to Lerato Molefe?");
        dialog.QuerySelectorAll(".dialog-body li").Select(Text).Should().Equal(
            "Lerato Molefe can read it on MSF reports: your narrative, and the ratings and comments of the Consultant and " +
            "Nurse groups. Nothing of the other groups is shown, not even that they answered.",
            "2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012.",
            "Supervision level: Not stated.",
            "Lerato Molefe is sent an email saying a report has been released, with a link to MSF reports. The email " +
            "holds none of the feedback.");
        Text(dialog.QuerySelector(".dialog-body > p")!).Should().Be(
            "Releasing cannot be undone: the narrative and the level cannot be changed afterwards.");
        dialog.QuerySelectorAll("button").Select(Text).Should().Equal("Not yet", "Release to Lerato Molefe");
        dialog.QuerySelector("button.btn-primary").Should().NotBeNull("Release destroys nothing (F8)");
    }

    [Fact]
    public void TheReleaseDialog_NamesTheLevelChosen_ByItsRung()
    {
        var cut = Render(new MsfReportSender(Report(MsfCampaignState.UnderReview)));
        cut.Find("#msf-level").Change("5");

        cut.Find("#msf-release-campaign").Click();

        ReleaseDialog(cut).QuerySelectorAll(".dialog-body li").Select(Text).Should().Contain("Supervision level: 4.");
    }

    [Fact]
    public void NotYet_SendsNothing_AndKeepsWhatWasTyped()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview));
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("#msf-release-campaign").Click();
        DialogButton(ReleaseDialog(cut), "Not yet").Click();

        sender.Commands.Should().BeEmpty();
        cut.Find("#msf-narrative").GetAttribute("value").Should().Be(TypedNarrative);
    }

    [Fact]
    public void ReleasingTheReport_SendsTheNarrativeAndLevel_ShowsItAsReleased_AndTheResultTakesTheFocus()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview));
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);
        cut.Find("#msf-level").Change("5");

        cut.Find("#msf-release-campaign").Click();
        DialogButton(ReleaseDialog(cut), "Release to Lerato Molefe").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be("Report released to Lerato Molefe."));
        var release = sender.Commands.Should().ContainSingle().Which.Should().BeOfType<ReleaseMsfCampaignCommand>().Subject;
        release.Narrative.Should().Be(TypedNarrative);
        release.EntrustmentLevel.Should().Be(5);
        release.ReviewerUserId.Should().Be(CoordinatorUserId);

        Text(cut.Find(".status-card .badge")).Should().Be("Released");
        Text(cut.Find(".status-line")).Should().Be("The report has been released to the trainee.");
        cut.FindAll("#msf-release-campaign, #msf-narrative, #msf-level").Should().BeEmpty("nothing can be sent again");
        cut.FindAll(".stored dt, .stored dd").Select(Text).Should().Equal(
            "Narrative", TypedNarrative, "Supervision level", "4");
        FocusWentTo(cut, cut.Instance.ResultRegion!.Value);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    /// <summary>R17: refused on a campaign still under review, beside Release with the focus back on it.</summary>
    [Fact]
    public void AReleaseRefusedOnACampaignStillUnderReview_ShowsTheRefusalBesideRelease_AndGivesItTheFocusBack()
    {
        const string refusal = "The campaign changed while it was being released.";
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview))
        {
            Refusal = new MsfRefusedException(refusal)
        };
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("#msf-release-campaign").Click();
        DialogButton(ReleaseDialog(cut), "Release to Lerato Molefe").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".release-actions .alert-danger")).Should().Be(
            $"Not released. {refusal} It is still Under review."));
        cut.FindAll(".action-result").Should().BeEmpty();
        sender.Reads.Should().Be(2, "the report is read again after the refusal");
        cut.Find("#msf-narrative").GetAttribute("value").Should().Be(TypedNarrative, "what was typed is kept");
        FocusWentTo(cut, cut.Instance.ReleaseButton);
    }

    [Fact]
    public void AReleaseRefusedBecauseItWasWithdrawnElsewhere_HeadsThePage_InTheHandlersWords()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview))
        {
            Refusal = new MsfRefusedException(ReleaseMsfCampaignCommandHandler.CampaignChanged),
            StateAfterRefusal = MsfCampaignState.Withdrawn
        };
        var cut = Render(sender);
        cut.Find("#msf-narrative").Change(TypedNarrative);

        cut.Find("#msf-release-campaign").Click();
        DialogButton(ReleaseDialog(cut), "Release to Lerato Molefe").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Not released. " + ReleaseMsfCampaignCommandHandler.CampaignChanged));
        Text(cut.Find(".status-card .badge")).Should().Be("Withdrawn");
        cut.FindAll("#msf-release-campaign, #msf-narrative").Should().BeEmpty();
        cut.Markup.Should().NotContain(TypedNarrative, "this tab's text was never given to the trainee");
        FocusWentTo(cut, cut.Instance.ResultRegion!.Value);
    }

    [Fact]
    public void ARefusalByAValidator_ShowsEachFailuresMessage_NotItsLogLine()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview))
        {
            Refusal = new ValidationException(
            [
                new ValidationFailure("Narrative", "The narrative can be at most 4000 characters."),
                new ValidationFailure("EntrustmentLevel", "That level is not on this campaign's scale.")
            ])
        };
        var cut = Render(sender);

        cut.Find("#msf-release-campaign").Click();
        DialogButton(ReleaseDialog(cut), "Release to Lerato Molefe").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".release-actions .alert-danger")).Should().Be(
            "Not released. The narrative can be at most 4000 characters. That level is not on this campaign's scale. " +
            "It is still Under review."));
        cut.Markup.Should().NotContain("Validation failed");
    }

    /// <summary>A release not ready is aria-disabled (DESIGN.md): pressed, it opens nothing and sends nothing.</summary>
    [Fact]
    public void ReleaseNotReady_IsAriaDisabled_AndAPressOpensNoDialog()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview, totalResponses: 1, surviving: 0));
        var cut = Render(sender);

        var release = cut.Find("#msf-release-campaign");
        release.HasAttribute("disabled").Should().BeFalse("it stays in the tab order with its reason");
        release.GetAttribute("aria-disabled").Should().Be("true");
        release.ClassList.Should().Contain("is-unavailable");
        release.Click();

        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void WhileAReleaseRuns_ASecondConfirmSendsNothing()
    {
        var sender = new MsfReportSender(Report(MsfCampaignState.UnderReview)) { Hold = true };
        var cut = Render(sender);

        cut.Find("#msf-release-campaign").Click();
        DialogButton(ReleaseDialog(cut), "Release to Lerato Molefe").Click();
        DialogButton(ReleaseDialog(cut), "Release to Lerato Molefe").Click();

        sender.Commands.Should().ContainSingle();
        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-success").Should().ContainSingle(), AsyncWorkTimeout);
        sender.Commands.Should().ContainSingle();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<CampaignReport> Render(MsfReportSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll(".status-card").Count > 0);
        return cut;
    }

    private static AngleSharp.Dom.IElement CloseDialog(IRenderedComponent<CampaignReport> cut)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == "Close this campaign now?");

    private static AngleSharp.Dom.IElement ReleaseDialog(IRenderedComponent<CampaignReport> cut)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!).StartsWith("Release this report", StringComparison.Ordinal));

    private static AngleSharp.Dom.IElement DialogButton(AngleSharp.Dom.IElement dialog, string label)
        => dialog.QuerySelectorAll("button").Single(button => Text(button) == label);

    private void FocusWentTo(IRenderedComponent<CampaignReport> cut, ElementReference target)
        => cut.WaitForAssertion(() => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == FocusIdentifier)
            .Select(invocation => invocation.Arguments[0])
            .OfType<ElementReference>()
            .Select(reference => reference.Id)
            .Should().Contain(target.Id));

    /// <summary>The CPSA ladder's rungs by their order: order 5 is rung "4" (T100).</summary>
    internal sealed class RatedLevelsReferenceData : StubActivityReferenceDataService
    {
        public List<string> AskedFor { get; } = [];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
            string activityTypeKey, CancellationToken cancellationToken = default)
        {
            AskedFor.Add(activityTypeKey);
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(activityTypeKey == "learner_feedback_cpsa"
                ? []
                :
                [
                    new("1", "1"), new("2", "2"), new("3", "3a"), new("4", "3b"), new("5", "4"), new("6", "5")
                ]);
        }
    }
}
