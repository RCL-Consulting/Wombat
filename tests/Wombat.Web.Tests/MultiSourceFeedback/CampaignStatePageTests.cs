using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// A campaign's page (T369, flow 10, K1–K22): the status card on its stripe, the invitees counted by respondent group, a
/// draft's addresses as list items, the Links part while open, About this campaign, and each action's result and refusal
/// where the boards put them. It shows the campaign as it is now and offers only what its state allows (T217).
/// </summary>
public sealed class CampaignStatePageTests : WombatTestContext
{
    private const int CampaignId = 7;

    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private static readonly DateOnly OpensOn = new(2026, 10, 14);
    private static readonly DateOnly ClosesOn = new(2026, 10, 28);

    public CampaignStatePageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(
            new Claim(ClaimTypes.NameIdentifier, "coordinator-1"),
            new Claim(WombatClaimTypes.InstitutionId, "1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    // ─── What each state offers (F7) ─────────────────────────────────────────

    [Theory]
    [InlineData(MsfCampaignState.Draft, "Draft", "detail-card--emphasis", true, true, null)]
    [InlineData(MsfCampaignState.Open, "Open", "activity-status--others", false, true, "View report")]
    [InlineData(MsfCampaignState.Closed, "Closed", "detail-card--emphasis", false, true, "Review and release")]
    [InlineData(MsfCampaignState.UnderReview, "Under review", "detail-card--emphasis", false, true, "Review and release")]
    [InlineData(MsfCampaignState.Released, "Released", "activity-status--done", false, false, "View report")]
    [InlineData(MsfCampaignState.Withdrawn, "Withdrawn", "activity-status--declined", false, false, null)]
    public void EachState_ShowsItselfOnItsStripe_AndOffersOnlyWhatItAllows(
        MsfCampaignState state, string label, string stripe, bool offersOpenAndInvite, bool offersWithdraw, string? reportLink)
    {
        var cut = Render(new CampaignSender(Campaign(state, Group(MsfRespondentCategory.PeerDoctor, 3, 1)) with { HasOpened = state != MsfCampaignState.Draft }));

        var card = StatusCard(cut);
        card.ClassList.Should().Contain(["detail-card", "status-card", stripe], "F7: the stripe by class only (M8)");
        Text(cut.Find("#msf-campaign-state")).Should().Be(label);
        cut.Find("#msf-campaign-state").ClassList.Should().Contain(BadgeFor.MsfCampaign(state));

        cut.FindAll("#msf-open-campaign").Should().HaveCount(offersOpenAndInvite ? 1 : 0);
        cut.FindAll("#msf-respondent-email").Should().HaveCount(offersOpenAndInvite ? 1 : 0, "invitees are added only to a draft");
        cut.FindAll("#msf-withdraw-campaign").Should().HaveCount(offersWithdraw ? 1 : 0, "Withdraw is offered where the list offers it (T206)");
        if (offersWithdraw)
        {
            Text(cut.Find("#msf-withdraw-campaign")).Should().Be("Withdraw campaign…", "it opens a dialog");
        }

        var links = cut.FindAll("#msf-campaign-report");
        if (reportLink is null)
        {
            links.Should().BeEmpty();
        }
        else
        {
            var link = links.Should().ContainSingle().Subject;
            Text(link).Should().Be(reportLink);
            link.GetAttribute("href").Should().Be($"/msf/reports/{CampaignId}");
        }

        // A withdrawn campaign offers nothing at all: no empty actions row under it.
        card.QuerySelectorAll(".status-actions").Should().HaveCount(state == MsfCampaignState.Withdrawn ? 0 : 1);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheHeading_AndTheSubtitle_NameTheCampaign()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Draft)));

        Text(cut.Find("h1")).Should().Be("MSF campaign for Lerato Molefe");
        Text(cut.Find(".page-subtitle")).Should().Be("Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28");
        cut.Find(".msf-header").Should().NotBeNull("its actions stack at 390");
    }

    // ─── A draft (K1–K5c) ────────────────────────────────────────────────────

    [Fact]
    public void ADraftThatInvitesNobody_SaysSo_AndOffersOpenUnavailable_WithItsReason()
    {
        var sender = new CampaignSender(Campaign(MsfCampaignState.Draft));
        var cut = Render(sender);

        Text(cut.Find("#msf-status-line")).Should().Be("Add invitees, then open the campaign to send anonymous response links.");
        Text(cut.Find("#msf-invitees-none")).Should().Be("Nobody has been invited yet.");
        cut.FindAll("#msf-invitee-counts").Should().BeEmpty();
        StatusCard(cut).TextContent.Should().Contain("Once the campaign opens, this page never lists who was invited or which of them responded.");

        // K1: unavailable, not disabled, its reason beside it; pressing it sends nothing.
        var open = cut.Find("#msf-open-campaign");
        open.HasAttribute("disabled").Should().BeFalse();
        open.GetAttribute("aria-disabled").Should().Be("true");
        open.GetAttribute("aria-describedby").Should().Be("msf-open-reason");
        Text(cut.Find("#msf-open-reason")).Should().Be(
            "Open campaign: add at least one invitee first. Opening the campaign emails each invitee a link to respond.");
        cut.Find("#msf-open-reason").Closest("ul")!.ClassList.Should().Contain("move-reasons");
        open.HasAttribute("blazor:onclick").Should().BeFalse("pressing it could only be refused, so it has no handler");
        sender.Commands.Should().BeEmpty();

        Text(cut.Find("#msf-address-count")).Should().Be("0 addresses");
        cut.Find("#msf-addresses").TextContent.Should().Contain("No address yet.");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ADraftInvitingNine_CountsThemByGroup_WithNoRespondedColumn_AndSaysWhomOpeningEmails()
    {
        var cut = Render(new CampaignSender(DraftInviting(TheNine)));

        var table = cut.Find("#msf-invitee-counts");
        table.ClassList.Should().Contain(["clinic-table", "count-table"]);
        table.Closest(".count-box").Should().NotBeNull("a draft's counts sit in the card without a heading (K2)");
        table.ParentElement!.ClassList.Should().NotContain("shadow", "no .shadow inside a card (S13)");
        Text(table.QuerySelector("caption")!).Should().Be(
            "Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded.");
        HeaderCells(table).Should().Equal("Respondent group", "Invited");
        BodyRows(table).Should().Equal(
            "Peer doctor | 2", "Consultant | 3", "Nurse | 3", "Allied health professional | 1");
        FooterRow(table).Should().Be("All groups | 9");

        Text(cut.Find("#msf-opening-emails")).Should().Be(
            "Opening emails each of the 9 invitees a link to respond, with 2026-10-28 as their last day.");
        cut.Find("#msf-open-campaign").HasAttribute("aria-disabled").Should().BeFalse();
        cut.FindAll("#msf-open-reason").Should().BeEmpty();
    }

    [Fact]
    public void ADraft_ListsEachAddressTyped_OverItsGroup_EachWithARemoveNamingIt()
    {
        var cut = Render(new CampaignSender(DraftInviting(TheNine)));

        Text(cut.Find("#msf-address-count")).Should().Be("9 addresses");
        var rows = cut.FindAll("#msf-draft-invitees > li.invitee-row").ToList();
        rows.Should().HaveCount(9);
        Text(rows[0].QuerySelector(".invitee-address")!).Should().StartWith("grace.mokwena@kgk.wombat.local");
        Text(rows[0].QuerySelector(".invitee-group")!).Should().Be("Nurse");
        var remove = rows[0].QuerySelector("button")!;
        Text(remove).Should().Be("Remove…");
        remove.GetAttribute("aria-label").Should().Be("Remove grace.mokwena@kgk.wombat.local (Nurse)");
        cut.Markup.Should().NotContain("<table id=\"msf-draft-invitees\"", "a list, not a table to scroll at 390 (T270)");
    }

    [Fact]
    public void TheAddForm_MarksTheEmailRequired_AndOffersTheGroupsInTheEnumsOrder_PeerDoctorChosen()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Draft)));

        var form = cut.Find("#msf-respondent-email").Closest("form")!;
        form.HasAttribute("novalidate").Should().BeTrue("S4: the page refuses in its words, not the browser");
        form.TextContent.Should().Contain("* marks a field you must fill in.");
        var email = cut.Find("#msf-respondent-email");
        email.GetAttribute("type").Should().Be("email");
        email.GetAttribute("aria-required").Should().Be("true");
        Text(cut.Find("label[for=msf-respondent-email]")).Should().Be("Respondent email *");
        cut.Find("label[for=msf-respondent-email] .required-mark").GetAttribute("aria-hidden").Should().Be("true");

        Text(cut.Find("label[for=msf-respondent-category]")).Should().Be("Respondent group", "no mark: it always holds a value");
        cut.FindAll("#msf-respondent-category option").Select(Text)
            .Should().Equal("Peer doctor", "Consultant", "Nurse", "Allied health professional", "Other");
        cut.Find("#msf-respondent-category").GetAttribute("value").Should().Be(nameof(MsfRespondentCategory.PeerDoctor));
    }

    [Fact]
    public void AddingAnInvitee_CountsThemAtOnce_SaysInviteeAddedAlone_AndMovesTheFocusToTheClearedEmail()
    {
        var sender = new CampaignSender(DraftInviting(("grace.mokwena@kgk.wombat.local", MsfRespondentCategory.Nurse)));
        var cut = Render(sender);

        cut.Find("#msf-respondent-category").Change(nameof(MsfRespondentCategory.Consultant));
        SubmitInvitee(cut, "khumalo@kgk.wombat.local");

        var add = sender.Commands.OfType<AddMsfInvitationCommand>().Should().ContainSingle().Subject;
        add.RespondentEmail.Should().Be("khumalo@kgk.wombat.local");
        add.RespondentCategory.Should().Be(MsfRespondentCategory.Consultant);
        add.CampaignId.Should().Be(CampaignId);

        // F11: "Invitee added." alone; the address is in the list below.
        cut.WaitForAssertion(() => Text(cut.Find("#msf-add-result")).Should().Be("Invitee added."));
        cut.Find("#msf-add-result").GetAttribute("role").Should().Be("status");
        cut.Find("#msf-respondent-email").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("#msf-respondent-email").GetAttribute("aria-describedby").Should().Be("msf-add-result");
        cut.Find("#msf-respondent-category").GetAttribute("value").Should().Be(nameof(MsfRespondentCategory.Consultant),
            "the next invitee is usually from the same group");
        FooterRow(cut.Find("#msf-invitee-counts")).Should().Be("All groups | 2");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.EmailInput.Id));
    }

    [Theory]
    [InlineData("", "Enter the respondent's email address.")]
    [InlineData("naledi.sebego@kgk", "Enter a valid email address, for example name@example.org.")]
    public void ABlankOrMalformedAddress_IsRefusedUnderTheField_InThePagesWords_AndNothingIsSent(string typed, string refusal)
    {
        var sender = new CampaignSender(DraftInviting(TheNine));
        var cut = Render(sender);

        SubmitInvitee(cut, typed);

        sender.Commands.Should().BeEmpty();
        var email = cut.Find("#msf-respondent-email");
        Text(cut.Find("#msf-respondent-email-error")).Should().Be(refusal);
        cut.Find("#msf-respondent-email-error").ClassList.Should().Contain("validation-message");
        email.GetAttribute("aria-invalid").Should().Be("true");
        email.GetAttribute("aria-describedby").Should().Be("msf-respondent-email-error");
        email.GetAttribute("value").Should().Be(typed, "what was typed is kept to correct");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.EmailInput.Id));
    }

    [Fact]
    public void AnAddressAlreadyInvited_IsRefusedUnderTheField_TheAddressKept_AndTheFocusInIt()
    {
        var sender = new CampaignSender(DraftInviting(TheNine)) { Refusal = new MsfRefusedException(AddMsfInvitationCommandHandler.AlreadyInvited) };
        var cut = Render(sender);

        cut.Find("#msf-respondent-category").Change(nameof(MsfRespondentCategory.Nurse));
        SubmitInvitee(cut, "Grace.Mokwena@kgk.wombat.local");

        cut.WaitForAssertion(() => Text(cut.Find("#msf-respondent-email-error")).Should().Be(AddMsfInvitationCommandHandler.AlreadyInvited));
        cut.Find("#msf-respondent-email").GetAttribute("value").Should().Be("Grace.Mokwena@kgk.wombat.local");
        cut.FindAll("#msf-add-result").Should().BeEmpty();
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.EmailInput.Id));
    }

    [Fact]
    public void AnAddRefusedBecauseTheCampaignWasOpenedElsewhere_ShowsItOpen_AndTheRefusalAtTheTopTakesTheFocus()
    {
        var sender = new CampaignSender(DraftInviting(TheNine))
        {
            Refusal = new MsfRefusedException("Invitations can only be added while a campaign is in draft."),
            StateAfterRefusal = MsfCampaignState.Open
        };
        var cut = Render(sender);

        SubmitInvitee(cut, "late@kgk.wombat.local");

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Open"));
        Text(cut.Find("#msf-campaign-result .alert-danger")).Should().Be(
            "Not added. Invitations can only be added while a campaign is in draft.");
        cut.FindAll("#msf-respondent-email").Should().BeEmpty("an open campaign has no invitee form");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void WhileAnAddRuns_ASecondPressSendsNothing_AndTheFirstAnswersAlone()
    {
        var sender = new CampaignSender(DraftInviting(TheNine)) { Hold = true };
        var cut = Render(sender);

        SubmitInvitee(cut, "one@kgk.wombat.local");
        cut.Find("#msf-add-invitee").GetAttribute("aria-disabled").Should().Be("true");
        cut.Find("#msf-add-invitee").HasAttribute("disabled").Should().BeFalse("it may hold the focus (T234)");
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();
        cut.Find("#msf-open-campaign").Click();

        sender.Commands.Should().ContainSingle();
        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll("#msf-add-result").Should().ContainSingle(), AsyncWorkTimeout);
        sender.Commands.Should().ContainSingle();
    }

    // ─── Open (K6, K20) ──────────────────────────────────────────────────────

    [Fact]
    public void OpeningACampaign_ShowsItOpen_TheResultAtTheTopTakesTheFocus_AndTheLinksPartSaysTheyAreBeingSent()
    {
        var sender = new CampaignSender(DraftInviting(TheNine));
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Open"));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<OpenMsfCampaignCommand>();
        Text(cut.Find("#msf-campaign-result .alert-success")).Should().Be("Campaign opened; links are being sent.");
        Text(cut.Find("#msf-status-line")).Should().Be("0 of 9 invitees have responded.");
        Text(cut.Find("#msf-links-being-sent")).Should().Be("9 links are still being sent. Reload this page to see whether they were delivered.");
        cut.Find("#msf-links-being-sent").ClassList.Should().Contain("alert-info");
        cut.FindAll("#msf-open-campaign, #msf-respondent-email, #msf-addresses").Should().BeEmpty();
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void ARefusedOpenOnACampaignStillADraft_IsBesideTheButton_AndLeavesTheFocusOnItForTheRetry()
    {
        var sender = new CampaignSender(DraftInviting(TheNine))
        {
            Refusal = new MsfRefusedException(OpenMsfCampaignCommandHandler.InvitationsNotSent),
            RefuseOnce = true
        };
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => cut.FindAll(".status-actions .alert-danger").Should().ContainSingle());
        var refusal = cut.Find(".status-actions .alert-danger");
        Text(refusal).Should().Be($"Not opened. {OpenMsfCampaignCommandHandler.InvitationsNotSent}");
        refusal.GetAttribute("role").Should().Be("alert");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier,
            "the Open button that had the focus is still there, for the retry (K20)");

        cut.Find("#msf-open-campaign").Click();
        sender.Commands.Should().HaveCount(2, "the retry is sent");
    }

    [Fact]
    public void AnOpenThatFailsForNoReasonAHandlerGave_SaysAFixedSentence_NeverTheExceptionsText()
    {
        var sender = new CampaignSender(DraftInviting(TheNine)) { Refusal = new TimeoutException("Npgsql: the pool is exhausted") };
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".status-actions .alert-danger")).Should().Be(
            "Not opened. Something went wrong. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
    }

    /// <summary>T369, build review R1: an <see cref="InvalidOperationException" /> that is not the campaign's own refusal.</summary>
    [Fact]
    public void AnOpenThatFailsWithAFrameworkInvalidOperation_SaysTheFixedSentence_NeverTheExceptionsText()
    {
        var sender = new CampaignSender(DraftInviting(TheNine))
        {
            Refusal = new InvalidOperationException("A second operation was started on this context instance.")
        };
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".status-actions .alert-danger")).Should().Be(
            "Not opened. " + MsfCampaignText.ActionFailed));
        cut.Markup.Should().NotContain("second operation");
    }

    // ─── Withdraw (K10, K12, K21) ────────────────────────────────────────────

    [Fact]
    public void Withdraw_AsksFirst_NamingTheCampaign_KeepTheCampaignFirst_AndSendsNothingUntilConfirmed()
    {
        var sender = new CampaignSender(DraftInviting(TheNine));
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty();
        var dialog = WithdrawDialog(cut);
        dialog.Closest(".msf-page").Should().NotBeNull("inside the page column, so its buttons stack at 390 (M10)");
        Text(dialog.QuerySelector("h2")!).Should().Be("Withdraw this campaign?");
        dialog.QuerySelectorAll(".form-actions button").Select(Text).Should().Equal("Keep the campaign", "Withdraw campaign");

        dialog.QuerySelectorAll("button").Single(button => Text(button) == "Keep the campaign").Click();
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void WithdrawsDialogUnderReview_IsWithdrawBodysUnderReviewForm_WordForWord()
    {
        // Check item 1: K10's body is the built sentence, not the board's draft tail.
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.UnderReview, Group(MsfRespondentCategory.Nurse, 3, 3)) with { HasOpened = true }));

        cut.Find("#msf-withdraw-campaign").Click();

        Text(WithdrawDialog(cut).QuerySelector("p")!).Should().Be(MsfCampaignText.WithdrawBody(
            "Lerato Molefe (Default MSF, closing 2026-10-28)", MsfCampaignState.UnderReview));
    }

    [Fact]
    public void ConfirmingWithdraw_WithdrawsAsTheCaller_AndTheResultAtTheTopTakesTheFocusAfterTheDialogCloses()
    {
        var sender = new CampaignSender(DraftInviting(TheNine));
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();
        ConfirmWithdrawButton(cut).Click();

        var command = sender.Commands.Should().ContainSingle().Which.Should().BeOfType<WithdrawMsfCampaignCommand>().Subject;
        command.ConfirmedState.Should().Be(MsfCampaignState.Draft);
        command.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("coordinator-1");
        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Withdrawn"));
        Text(cut.Find("#msf-campaign-result .alert-success")).Should().Be(
            "The campaign for Lerato Molefe (Default MSF, closing 2026-10-28) has been withdrawn. Its respondents' links no " +
            "longer work, and their email addresses have been removed.");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeLessThan(calls.IndexOf(FocusIdentifier));
    }

    [Fact]
    public void ARefusedWithdraw_IsBesideTheButton_AndTheFocusGoesBackToIt()
    {
        var sender = new CampaignSender(Campaign(MsfCampaignState.UnderReview, Group(MsfRespondentCategory.Nurse, 3, 3)) with { HasOpened = true })
        {
            Refusal = new MsfRefusedException("Released campaigns cannot be withdrawn."),
            RefuseOnce = true
        };
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();
        ConfirmWithdrawButton(cut).Click();

        cut.WaitForAssertion(() => Text(cut.Find(".status-actions .alert-danger")).Should().Be(
            "Not withdrawn. Released campaigns cannot be withdrawn."));
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.WithdrawButton.Id));
    }

    [Fact]
    public void AWithdrawOfACampaignClosedSinceThePageWasRead_IsRefused_AndTheNextDialogAsksInItsClosedWords()
    {
        var sender = new CampaignSender(Campaign(MsfCampaignState.Open, Group(MsfRespondentCategory.PeerDoctor, 3, 1)) with { HasOpened = true })
        {
            Refusal = new MsfRefusedException(WithdrawMsfCampaignCommandHandler.ClosedSinceShown),
            RefuseOnce = true,
            StateAfterRefusal = MsfCampaignState.UnderReview
        };
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();
        ConfirmWithdrawButton(cut).Click();

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Under review"));
        Text(cut.Find(".status-actions .alert-danger")).Should().Be($"Not withdrawn. {WithdrawMsfCampaignCommandHandler.ClosedSinceShown}");

        cut.Find("#msf-withdraw-campaign").Click();
        Text(WithdrawDialog(cut)).Should().Contain("It has closed, and its report has not been released.");
        ConfirmWithdrawButton(cut).Click();

        sender.Commands.OfType<WithdrawMsfCampaignCommand>().Last().ConfirmedState.Should().Be(MsfCampaignState.UnderReview);
        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Withdrawn"));
    }

    [Fact]
    public void ADraftWithdrawn_SaysSo_SaysNobodyWasInvited_AndOffersNothing()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Withdrawn)));

        Text(cut.Find("#msf-status-line")).Should().Be(
            "Withdrawn. It takes no responses, any link sent no longer works, and every respondent's email address has been " +
            "removed. It will never be released to the trainee.");
        Text(cut.Find("#msf-invitees-none")).Should().Be("Nobody was invited.");
        cut.FindAll(".status-actions").Should().BeEmpty();
    }

    [Fact]
    public void ACampaignThatNeverOpened_HasNoRespondedColumn_ThoughItIsWithdrawn()
    {
        // T270: a draft withdrawn has no responses to count.
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Withdrawn, Group(MsfRespondentCategory.Nurse, 2, 0))));

        HeaderCells(cut.Find("#msf-invitee-counts")).Should().Equal("Respondent group", "Invited");
    }

    // ─── Remove (K3, K4) ─────────────────────────────────────────────────────

    [Fact]
    public void Remove_AsksFirst_NamingTheInvitee_KeepTheInviteeFirst_AndSendsNothingUntilConfirmed()
    {
        var sender = new CampaignSender(DraftInviting(TheNine));
        var cut = Render(sender);

        RemoveButton(cut, "palesa.tau@kgk.wombat.local").Click();

        sender.Commands.Should().BeEmpty();
        var dialog = RemoveDialog(cut);
        Text(dialog.QuerySelector("p")!).Should().Be(
            "Remove palesa.tau@kgk.wombat.local (Nurse) from this campaign? They hold no working link, and will not be sent " +
            "one when the campaign opens. While the campaign is a draft, they can be added again.");
        dialog.QuerySelectorAll(".form-actions button").Select(Text).Should().Equal("Keep the invitee", "Remove invitee");
    }

    [Fact]
    public void ConfirmingRemove_RemovesThatInvitee_RecountsAtOnce_AndTheResultAboveTheRowsTakesTheFocus()
    {
        var sender = new CampaignSender(DraftInviting(TheNine));
        var cut = Render(sender);

        RemoveButton(cut, "palesa.tau@kgk.wombat.local").Click();
        ConfirmRemoveButton(cut).Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<RemoveMsfInvitationCommand>().Which.InvitationId.Should().Be(9);
        cut.WaitForAssertion(() => Text(cut.Find("#msf-addresses-result .alert-success")).Should().Be(
            "palesa.tau@kgk.wombat.local (Nurse) has been removed from this campaign, and will not be emailed a link when it opens."));
        Text(cut.Find("#msf-address-count")).Should().Be("8 addresses");
        FooterRow(cut.Find("#msf-invitee-counts")).Should().Be("All groups | 8");
        Text(cut.Find("#msf-opening-emails")).Should().StartWith("Opening emails each of the 8 invitees", "M3");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.RowsResultRegion.Id));
    }

    [Fact]
    public void ARemoveRefusedBecauseTheCampaignWasOpenedElsewhere_ShowsItOpen_ListsNoAddress_AndTheRefusalTakesTheFocus()
    {
        var sender = new CampaignSender(DraftInviting(TheNine))
        {
            Refusal = new MsfRefusedException("Invitees can only be removed while a campaign is in draft."),
            StateAfterRefusal = MsfCampaignState.Open
        };
        var cut = Render(sender);

        RemoveButton(cut, "palesa.tau@kgk.wombat.local").Click();
        ConfirmRemoveButton(cut).Click();

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Open"));
        Text(cut.Find("#msf-campaign-result .alert-danger")).Should().Be(
            "Not removed. Invitees can only be removed while a campaign is in draft.");
        cut.Markup.Should().NotContain("@kgk.wombat.local", "an open campaign's page lists no address");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    // ─── Not current, ended, learner feedback (K13–K15) ──────────────────────

    [Fact]
    public void ADraftWhoseTraineeIsNotCurrent_OffersOpenAndAddUnavailable_WithReasons_TheFieldsDisabled_AndWithdrawWorks()
    {
        var sender = new CampaignSender(Campaign(MsfCampaignState.Draft) with { SubjectName = "Pieter du Plessis", SubjectIsCurrent = false });
        var cut = Render(sender);

        var open = cut.Find("#msf-open-campaign");
        open.GetAttribute("aria-disabled").Should().Be("true");
        Text(cut.Find("#msf-open-reason")).Should().Be(
            "Open campaign: Pieter du Plessis is no longer in a programme. Withdraw this campaign.");

        cut.Find("#msf-respondent-email").HasAttribute("disabled").Should().BeTrue("nothing can be sent (K13)");
        cut.Find("#msf-respondent-category").HasAttribute("disabled").Should().BeTrue();
        var add = cut.Find("#msf-add-invitee");
        add.HasAttribute("disabled").Should().BeFalse();
        add.GetAttribute("aria-disabled").Should().Be("true");
        add.GetAttribute("type").Should().Be("button", "it is no submit");
        Text(cut.Find("#msf-add-reason")).Should().Be(
            "Add invitee: Pieter du Plessis is no longer in a programme, so nobody can be invited.");
        add.HasAttribute("blazor:onclick").Should().BeFalse();
        open.HasAttribute("blazor:onclick").Should().BeFalse();
        sender.Commands.Should().BeEmpty();

        Text(AboutValue(cut, "Trainee")).Should().Be("Pieter du Plessis", "no training year for a trainee not current");
        cut.Find("#msf-withdraw-campaign").HasAttribute("aria-disabled").Should().BeFalse();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ACampaignUnderReviewWhoseTraineesProgrammeEnded_SaysARelease_StillRecordsTheEvidence()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.UnderReview, Group(MsfRespondentCategory.Nurse, 3, 3))
            with { HasOpened = true, SubjectIsCurrent = false, SubjectEndedOn = new DateOnly(2026, 10, 20) }));

        var ended = cut.Find("#msf-programme-ended");
        Text(ended).Should().Be(MsfCampaignText.ProgrammeEnded("Lerato Molefe", new DateOnly(2026, 10, 20)));
        ended.ClassList.Should().Contain("alert-warning");
        ended.HasAttribute("role").Should().BeFalse("standing content");
        ended.Closest(".status-card").Should().NotBeNull();
    }

    [Fact]
    public void ALearnerFeedbackDraft_AsksATeachingContext_HasOneGroup_AndSaysItsGroupsMinimumIsFixed()
    {
        var setup = Campaign(MsfCampaignState.Draft) with
        {
            Kind = MsfTemplateKind.LearnerFeedback,
            TemplateName = "Teaching feedback",
            AcceptedCategories = [MsfRespondentCategory.Learner],
            MinimumRespondentCategories = 1,
            Invitees = [Group(MsfRespondentCategory.Learner, 1, 0)],
            DraftInvitees = [new MsfDraftInviteeDto(1, "student-1@example.test", MsfRespondentCategory.Learner, "Ward round")]
        };
        var cut = Render(new CampaignSender(setup));

        cut.FindAll("#msf-respondent-category").Should().BeEmpty();
        Text(cut.Find("label[for=msf-teaching-context]")).Should().Be("Teaching context *");
        cut.Find("#msf-addresses").TextContent.Should().Contain("Every invitee is in one respondent group, Learner.");
        Text(cut.Find("#msf-draft-invitees .invitee-group")).Should().Be("Learner · Teaching context: Ward round");
        cut.Find("#msf-draft-invitees button").GetAttribute("aria-label").Should().Be("Remove student-1@example.test (Learner)");
        BodyRows(cut.Find("#msf-invitee-counts")).Should().Equal("Learner | 1");
        FooterRow(cut.Find("#msf-invitee-counts")).Should().Be("All groups | 1", "K15 keeps the total under one group");
        Text(AboutValue(cut, "Thresholds")).Should().Be(
            "5 responses; 3 in a respondent group; 1 respondent group reporting Learner feedback has one respondent group, " +
            "so its groups-reporting minimum is fixed at 1.");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ALearnerWithNoTeachingContext_IsRefusedUnderTheField_AndNothingIsSent()
    {
        var setup = Campaign(MsfCampaignState.Draft) with
        {
            Kind = MsfTemplateKind.LearnerFeedback,
            AcceptedCategories = [MsfRespondentCategory.Learner]
        };
        var sender = new CampaignSender(setup);
        var cut = Render(sender);

        SubmitInvitee(cut, "student-1@example.test");

        sender.Commands.Should().BeEmpty();
        Text(cut.Find("#msf-teaching-context-error")).Should().Be("Say where this learner was taught.");
        cut.Find("#msf-teaching-context").GetAttribute("aria-invalid").Should().Be("true");
        // Build review D6: the field refused takes the focus, not Add invitee.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ContextInput.Id));
    }

    // ─── Open, under review, released (K6–K11) ───────────────────────────────

    [Fact]
    public void AnOpenCampaign_SaysHowManyResponded_WhenItCloses_AndCountsByGroupUnderItsHeading()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Open,
            Group(MsfRespondentCategory.PeerDoctor, 2, 1), Group(MsfRespondentCategory.Consultant, 3, 3),
            Group(MsfRespondentCategory.Nurse, 3, 3), Group(MsfRespondentCategory.Ahp, 1, 1)) with { HasOpened = true }));

        Text(cut.Find("#msf-status-line")).Should().Be("8 of 9 invitees have responded.");
        Text(cut.Find(".status-note")).Should().Be(
            "Each respondent is emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by " +
            "itself once that day has passed; to close it sooner, use Close campaign on its report.");
        var table = cut.Find("#msf-invitee-counts");
        table.Closest(".status-split").Should().NotBeNull();
        Text(table.Closest("div")!.ParentElement!.QuerySelector("h3")!).Should().Be("Invitees by respondent group");
        HeaderCells(table).Should().Equal("Respondent group", "Invited", "Responded");
        FooterRow(table).Should().Be("All groups | 9 | 8");
        cut.FindAll("th.num, td.num").Should().NotBeEmpty("counts are right-aligned in tabular figures");
        cut.Markup.Should().NotContain("@kgk.wombat.local");
    }

    [Theory]
    [InlineData(1, "1 of 1 invitee has responded.")]
    [InlineData(2, "1 of 2 invitees have responded.")]
    public void TheOpenLine_IsSingularWhereItCountsOne(int invited, string line)
    {
        // T270: every count that counts has its singular.
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Open, Group(MsfRespondentCategory.Nurse, invited, 1)) with { HasOpened = true }));

        Text(cut.Find("#msf-status-line")).Should().Be(line);
    }

    [Fact]
    public void ACampaignUnderReview_SaysWhereItIsReleased_AndAboutSaysWhenItClosed_OnTheSouthAfricanCalendar()
    {
        // 22:30 UTC on the 14th is already the 15th in South Africa.
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.UnderReview, Group(MsfRespondentCategory.Nurse, 3, 3))
            with { HasOpened = true, ClosedOn = new DateTime(2026, 10, 14, 22, 30, 0, DateTimeKind.Utc) }));

        Text(cut.Find("#msf-status-line")).Should().Be("Closed to responses, and every respondent's email address has been removed.");
        Text(cut.Find(".status-note")).Should().Be(
            "Review the responses on its report, and release it to the trainee there. If it should never be released, withdraw it here.");
        Text(AboutValue(cut, "Closed to responses")).Should().Be("on 2026-10-15");
    }

    [Fact]
    public void AReleasedCampaign_SaysSo_AndOffersOnlyItsReport()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Released, Group(MsfRespondentCategory.Nurse, 3, 3)) with { HasOpened = true }));

        Text(cut.Find("#msf-status-line")).Should().Be("The report has been released to the trainee.");
        cut.FindAll(".status-actions a, .status-actions button").Select(Text).Should().Equal("View report");
    }

    // ─── About this campaign ─────────────────────────────────────────────────

    [Fact]
    public void About_NamesTheTraineeWithTheirYear_TheTemplate_TheThresholds_AndEachEpa()
    {
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Draft)));

        var about = cut.Find("#msf-about");
        Text(about.QuerySelector("h2")!).Should().Be("About this campaign");
        about.QuerySelector("dl")!.ClassList.Should().Contain("msf-facts");
        Text(AboutValue(cut, "Trainee")).Should().Be("Lerato Molefe, training year 4");
        Text(AboutValue(cut, "Template")).Should().Be("Default MSF (Multi-source feedback)");
        Text(AboutValue(cut, "Thresholds")).Should().Be("5 responses; 3 in a respondent group; 2 respondent groups reporting");
        Text(AboutValue(cut, "Evidence")).Should().StartWith("Evidence for PAED-010 and PAED-012");
        about.QuerySelectorAll(".epa-lines li").Select(Text).Should().Equal(
            "PAED-010 — Leading and operating within a clinical team",
            "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams");
        cut.FindAll("#msf-about dt").Select(Text).Should().NotContain("Closed to responses", "a draft has not closed");
    }

    [Fact]
    public void About_CountsFifteenEpas_AndListsEach()
    {
        var epas = Enumerable.Range(1, 15)
            .Select(number => new MsfCoveredEpaDto(100 + number, $"PAED-{number:000}", $"EPA {number}", false))
            .ToList();
        var cut = Render(new CampaignSender(Campaign(MsfCampaignState.Draft) with { CoveredEpas = epas }));

        Text(AboutValue(cut, "Evidence")).Should().StartWith("Evidence for 15 EPAs");
        cut.FindAll("#msf-about .epa-lines li").Should().HaveCount(15);
    }

    // ─── Unavailable, loading, load error (K16–K18) ──────────────────────────

    [Fact]
    public void ACampaignThisCallerDoesNotRun_IsUnavailable_WithAWayBackToTheList()
    {
        var cut = Render(new CampaignSender(null!), waitForState: false);

        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Campaign unavailable"));
        Text(cut.Find("#msf-campaign-unavailable .state-panel-copy")).Should().Be("The selected campaign could not be loaded.");
        var back = cut.Find("#msf-campaign-unavailable a");
        Text(back).Should().Be("Go to MSF campaigns");
        back.GetAttribute("href").Should().Be("/msf/campaigns");
    }

    [Fact]
    public void WhileTheCampaignIsRead_TheHeadingShowsAtOnce_AndNoActionIsOffered()
    {
        var sender = new CampaignSender(DraftInviting(TheNine)) { HoldReads = true };
        var cut = Render(sender, waitForState: false);

        Text(cut.Find("h1")).Should().Be("MSF campaign");
        Text(cut.Find("p[role=status]")).Should().Be("Loading the campaign.");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("button").Where(button => button.Closest("dialog") is null).Should().BeEmpty();

        sender.ReleaseRead();
        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("MSF campaign for Lerato Molefe"), AsyncWorkTimeout);
    }

    [Fact]
    public void AFailedRead_IsAFixedSentenceWithTryAgain_NeverTheExceptionsText_AndTryAgainReadsAgain()
    {
        var sender = new CampaignSender(DraftInviting(TheNine)) { ReadFailures = 1 };
        var cut = Render(sender, waitForState: false);

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load this campaign. Nothing has changed. Try again, or come back in a few minutes."));
        Text(cut.Find("h1")).Should().Be("MSF campaign");
        cut.Markup.Should().NotContain("connection reset");

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();
        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("MSF campaign for Lerato Molefe"));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static readonly (string Email, MsfRespondentCategory Category)[] TheNine =
    [
        ("grace.mokwena@kgk.wombat.local", MsfRespondentCategory.Nurse),
        ("kagiso.motsepe@kgk.wombat.local", MsfRespondentCategory.PeerDoctor),
        ("lindiwe.khoza@kgk.wombat.local", MsfRespondentCategory.PeerDoctor),
        ("khumalo@kgk.wombat.local", MsfRespondentCategory.Consultant),
        ("botha@kgk.wombat.local", MsfRespondentCategory.Consultant),
        ("naledi.sebego@kgk.wombat.local", MsfRespondentCategory.Ahp),
        ("ndlovu@kgk.wombat.local", MsfRespondentCategory.Consultant),
        ("zulu@kgk.wombat.local", MsfRespondentCategory.Nurse),
        ("palesa.tau@kgk.wombat.local", MsfRespondentCategory.Nurse)
    ];

    private IRenderedComponent<CampaignEdit> Render(CampaignSender sender, bool waitForState = true)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        if (waitForState)
        {
            cut.WaitForState(() => cut.FindAll("#msf-campaign-state").Count == 1);
        }

        return cut;
    }

    private static IElement StatusCard(IRenderedFragment cut) => cut.Find("#msf-campaign-status");

    private static IElement WithdrawDialog(IRenderedFragment cut)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == "Withdraw this campaign?");

    private static IElement ConfirmWithdrawButton(IRenderedFragment cut)
        => WithdrawDialog(cut).QuerySelectorAll("button").Single(candidate => Text(candidate) == "Withdraw campaign");

    private static IElement RemoveButton(IRenderedFragment cut, string email)
        => cut.FindAll("#msf-draft-invitees > li")
            .Single(row => row.QuerySelector("button")!.GetAttribute("aria-label")!.StartsWith($"Remove {email} ", StringComparison.Ordinal))
            .QuerySelector("button")!;

    private static IElement RemoveDialog(IRenderedFragment cut)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == "Remove this invitee?");

    private static IElement ConfirmRemoveButton(IRenderedFragment cut)
        => RemoveDialog(cut).QuerySelectorAll("button").Single(candidate => Text(candidate) == "Remove invitee");

    private static void SubmitInvitee(IRenderedFragment cut, string email)
    {
        cut.Find("#msf-respondent-email").Change(email);
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();
    }

    /// <summary>The value About gives beside a term.</summary>
    private static IElement AboutValue(IRenderedFragment cut, string term)
        => cut.FindAll("#msf-about dt").Single(dt => Text(dt) == term).NextElementSibling!;

    /// <summary>Lerato Molefe's campaign as the setup query reads it, current, in her fourth year, 5 / 3 / 2.</summary>
    private static MsfCampaignSetupDto Campaign(MsfCampaignState state, params MsfInviteeCountDto[] invitees)
        => new(CampaignId, "Default MSF", MsfTemplateKind.Msf, state,
            [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse,
             MsfRespondentCategory.Ahp, MsfRespondentCategory.Other])
        {
            SubjectName = "Lerato Molefe",
            OpensOn = OpensOn,
            ClosesOn = ClosesOn,
            Invitees = invitees,
            MinimumResponses = 5,
            MinimumCategoryResponses = 3,
            MinimumRespondentCategories = 2,
            CoveredEpas =
            [
                new MsfCoveredEpaDto(110, "PAED-010", "Leading and operating within a clinical team", false),
                new MsfCoveredEpaDto(112, "PAED-012", "Communicating with and counselling patients, caregivers and healthcare teams", false)
            ],
            TrainingYear = 4,
            SubjectIsCurrent = true
        };

    private static MsfInviteeCountDto Group(MsfRespondentCategory category, int invited, int responded) => new(category, invited, responded);

    /// <summary>A draft inviting these addresses, counted by group and listed in order, invitation ids from 1. (T247)</summary>
    private static MsfCampaignSetupDto DraftInviting(params (string Email, MsfRespondentCategory Category)[] invitees)
        => Campaign(MsfCampaignState.Draft) with
        {
            Invitees = Counted(invitees.Select(invitee => invitee.Category)),
            DraftInvitees = invitees
                .Select((invitee, index) => new MsfDraftInviteeDto(index + 1, invitee.Email, invitee.Category, null))
                .ToList()
        };

    private static List<MsfInviteeCountDto> Counted(IEnumerable<MsfRespondentCategory> categories)
        => categories.GroupBy(category => category).OrderBy(group => group.Key)
            .Select(group => Group(group.Key, group.Count(), 0)).ToList();

    private static IReadOnlyList<string> HeaderCells(IElement table) => table.QuerySelectorAll("thead th").Select(Text).ToList();

    private static IReadOnlyList<string> BodyRows(IElement table) => table.QuerySelectorAll("tbody tr").Select(RowText).ToList();

    private static string? FooterRow(IElement table) => table.QuerySelector("tfoot tr") is { } row ? RowText(row) : null;

    private static string RowText(IElement row) => string.Join(" | ", row.Children.Select(Text));

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>
    /// Answers the page's reads with the campaign as it stands, and adds to, removes from, opens and withdraws it as the
    /// handlers would. A refusal leaves it in <see cref="StateAfterRefusal" />, as a campaign changed elsewhere.
    /// </summary>
    private sealed class CampaignSender(MsfCampaignSetupDto? setup) : IScopedSender
    {
        private MsfCampaignSetupDto? _setup = setup;
        private int _nextInvitationId = 100;
        private TaskCompletionSource? _held;
        private TaskCompletionSource? _heldRead;
        private bool _refused;
        private int _readFailures;

        /// <summary>Every add, remove, open and withdraw sent, in order.</summary>
        public List<object> Commands { get; } = [];

        public Exception? Refusal { get; init; }

        /// <summary>Refuses only the first command, as a refusal for a campaign changed elsewhere does.</summary>
        public bool RefuseOnce { get; init; }

        public MsfCampaignState? StateAfterRefusal { get; init; }

        /// <summary>Holds each command until <see cref="Release" />.</summary>
        public bool Hold { get; init; }

        /// <summary>Holds the first read until <see cref="ReleaseRead" />.</summary>
        public bool HoldReads { get; init; }

        /// <summary>How many reads fail before one answers.</summary>
        public int ReadFailures { init => _readFailures = value; }

        public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

        public void ReleaseRead() => (_heldRead ?? throw new InvalidOperationException("No read was held.")).SetResult();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetMsfCampaignSetupQuery)
            {
                if (HoldReads && _heldRead is null)
                {
                    _heldRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    await _heldRead.Task;
                }

                if (_readFailures > 0)
                {
                    _readFailures--;
                    throw new TimeoutException("connection reset");
                }

                return (TResponse)(object)_setup!;
            }

            if (request is AddMsfInvitationCommand add)
            {
                Commands.Add(add);
                if (Refuses())
                {
                    Refuse();
                    throw Refusal!;
                }

                var setup = _setup!;
                _setup = setup with
                {
                    Invitees = Counted(setup.DraftInvitees.Select(invitee => invitee.Category).Append(add.RespondentCategory)),
                    DraftInvitees =
                    [
                        .. setup.DraftInvitees,
                        new MsfDraftInviteeDto(_nextInvitationId++, add.RespondentEmail, add.RespondentCategory, add.TeachingContext)
                    ]
                };
                await HeldAsync();
                return (TResponse)(object)_nextInvitationId;
            }

            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public async Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);

            if (Refuses())
            {
                Refuse();
                throw Refusal!;
            }

            var setup = _setup!;
            _setup = request switch
            {
                OpenMsfCampaignCommand => setup with
                {
                    State = MsfCampaignState.Open,
                    DraftInvitees = [],
                    HasOpened = true,
                    LinksBeingSent = setup.Invitees.Sum(group => group.Invited)
                },
                WithdrawMsfCampaignCommand => setup with { State = MsfCampaignState.Withdrawn, DraftInvitees = [] },
                RemoveMsfInvitationCommand remove => Removed(setup, remove.InvitationId),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            await HeldAsync();
        }

        private bool Refuses() => Refusal is not null && !(RefuseOnce && _refused);

        private async Task HeldAsync()
        {
            if (Hold)
            {
                _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                await _held.Task;
            }
        }

        private void Refuse()
        {
            _refused = true;
            if (StateAfterRefusal is { } state)
            {
                // As the query answers: no state but a draft lists an address (T247).
                _setup = _setup! with
                {
                    State = state,
                    HasOpened = state != MsfCampaignState.Draft,
                    DraftInvitees = state == MsfCampaignState.Draft ? _setup.DraftInvitees : []
                };
            }
        }

        private static MsfCampaignSetupDto Removed(MsfCampaignSetupDto setup, int invitationId)
        {
            var left = setup.DraftInvitees.Where(candidate => candidate.InvitationId != invitationId).ToList();
            return setup with { DraftInvitees = left, Invitees = Counted(left.Select(invitee => invitee.Category)) };
        }
    }
}
