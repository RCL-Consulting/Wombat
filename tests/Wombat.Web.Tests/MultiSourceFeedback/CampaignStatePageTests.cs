using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The campaign page shows the campaign as it is now, with its invitees counted by respondent group, and offers only what
/// its state allows. (T217)
/// </summary>
/// <remarks>
/// Until T217 the page showed the draft's invitee form and its Open button in every state and was never read again, so a
/// campaign just opened still read "Add invitees, then open the campaign", offered Open, and showed nobody invited.
/// </remarks>
public sealed class CampaignStatePageTests : TestContext
{
    private const int CampaignId = 7;

    public CampaignStatePageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    // ─── What each state offers ──────────────────────────────────────────────

    [Theory]
    [InlineData(MsfCampaignState.Draft, "Draft", true, true, null)]
    [InlineData(MsfCampaignState.Open, "Open", false, true, "View report")]
    [InlineData(MsfCampaignState.Closed, "Closed", false, false, "Review and release")]
    [InlineData(MsfCampaignState.UnderReview, "Under review", false, false, "Review and release")]
    [InlineData(MsfCampaignState.Released, "Released", false, false, "View report")]
    [InlineData(MsfCampaignState.Withdrawn, "Withdrawn", false, false, null)]
    public void EachState_ShowsItself_AndOffersOnlyWhatItAllows(
        MsfCampaignState state, string label, bool offersOpenAndInvite, bool offersWithdraw, string? reportLink)
    {
        var cut = Render(new CampaignSender(Setup(state, Group(MsfRespondentCategory.PeerDoctor, 3, 1))));

        Text(cut.Find("#msf-campaign-state")).Should().Be(label);

        cut.FindAll("#msf-open-campaign").Should().HaveCount(offersOpenAndInvite ? 1 : 0);
        cut.FindAll("#msf-respondent-email").Should().HaveCount(offersOpenAndInvite ? 1 : 0, "invitees are added only to a draft");
        cut.FindAll("#msf-withdraw-campaign").Should().HaveCount(offersWithdraw ? 1 : 0, "Withdraw is offered where the list offers it (T206)");

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
        if (state == MsfCampaignState.Withdrawn)
        {
            CampaignCard(cut).QuerySelectorAll(".form-actions").Should().BeEmpty();
        }

        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnOpenCampaign_NamesTheTrainee_AndSaysWhenItCloses_AndWhereToCloseItSooner()
    {
        var cut = Render(new CampaignSender(Setup(MsfCampaignState.Open, Group(MsfRespondentCategory.Nurse, 2, 0))));

        cut.FindAll(".details-list > div")
            .Select(row => $"{Text(row.QuerySelector("dt")!)}: {Text(row.QuerySelector("dd")!)}")
            .Should().Equal(
                "Trainee: Sipho Dlamini",
                "Template: Annual MSF (Multi-source feedback)",
                "State: Open",
                "Response window: 2029-03-01 to 2029-03-21");

        Text(cut.Find("#msf-campaign-state-note")).Should().Be(
            "Open. Each respondent has been emailed a link, which gives 2029-03-21 as their last day to respond. The " +
            "campaign closes by itself once that day has passed; to close it sooner, use Close campaign on its report.");
        cut.Markup.Should().NotContain("Add invitees, then open the campaign");
    }

    // ─── Invitees, counted ───────────────────────────────────────────────────

    [Fact]
    public void AnOpenCampaign_CountsItsInviteesAndResponsesByGroup_WithATotal()
    {
        var cut = Render(new CampaignSender(Setup(MsfCampaignState.Open,
            Group(MsfRespondentCategory.PeerDoctor, 4, 3),
            Group(MsfRespondentCategory.Ahp, 2, 1),
            Group(MsfRespondentCategory.Patient, 1, 0))));

        var table = cut.Find("table");
        table.GetAttribute("aria-labelledby").Should().Be("msf-invitees-heading");
        Text(cut.Find("#msf-invitees-heading")).Should().Be("Invitees");
        HeaderCells(table).Should().Equal("Respondent group", "Invited", "Responded");
        BodyRows(table).Should().Equal(
            "Peer doctor | 4 | 3",
            "Allied health professional | 2 | 1",
            "Patient | 1 | 0");
        FooterRow(table).Should().Be("All groups | 7 | 4");

        // It says what the page does and promises nothing more: a campaign whose category threshold is one shows a
        // one-person group's answers on its report, whatever this page shows (T217 review).
        Text(cut.Find("#msf-invitees-note")).Should().Be(
            "Counted by respondent group. This page never lists who was invited or which of them responded.");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ADraft_CountsItsInvitees_WithNoRespondedColumn()
    {
        var cut = Render(new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.Consultant, 2, 0))));

        var table = cut.Find("table");
        HeaderCells(table).Should().Equal("Respondent group", "Invited");
        BodyRows(table).Should().Equal("Consultant | 2");
        table.QuerySelector("tfoot").Should().BeNull("one group needs no total");
    }

    [Fact]
    public void ADraftWithNobodyInvited_SaysSo_AndHasNoTable()
    {
        var cut = Render(new CampaignSender(Setup(MsfCampaignState.Draft)));

        Text(cut.Find("#msf-invitees-none")).Should().Be("Nobody has been invited yet.");
        cut.FindAll("table").Should().BeEmpty();
        Text(cut.Find("#msf-campaign-state-note")).Should().Be("Add invitees, then open the campaign to send anonymous response links.");
    }

    [Fact]
    public void AddingAnInvitee_CountsThemAtOnce()
    {
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 1, 0)));
        var cut = Render(sender);

        cut.Find("#msf-respondent-email").Change("nurse-1@example.test");
        cut.Find("#msf-respondent-category").Change(nameof(MsfRespondentCategory.Nurse));
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-success")).Should().Be("Invitee added."));
        BodyRows(cut.Find("table")).Should().Equal("Peer doctor | 1", "Nurse | 1");
        FooterRow(cut.Find("table")).Should().Be("All groups | 2");
        cut.Markup.Should().NotContain("nurse-1@example.test", "the page lists no address, the one just added included");
    }

    [Fact]
    public void AddingAnInvitee_ClearsTheEmail_AndMovesTheFocusToItForTheNext()
    {
        // The browser check found the focus on the page body after an add: a new model made EditForm rebuild the form.
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 1, 0)));
        var cut = Render(sender);

        cut.Find("#msf-respondent-email").Change("nurse-1@example.test");
        cut.Find("#msf-respondent-category").Change(nameof(MsfRespondentCategory.Nurse));
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-success")).Should().Be("Invitee added."));
        cut.Find("#msf-respondent-email").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("#msf-respondent-category").GetAttribute("value").Should().Be(nameof(MsfRespondentCategory.Nurse),
            "the next invitee is usually from the same group");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.EmailInput!.Value.Id));
    }

    [Fact]
    public void AnAddRefusedBecauseTheCampaignWasOpenedElsewhere_ShowsItOpen_AndMovesTheFocusToTheRefusal()
    {
        // Opened in another tab: the refusal's re-read shows the campaign open, and the invitee form, with the submit
        // button that had the focus, is gone. (T217 review)
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)))
        {
            Refusal = new InvalidOperationException("Invitations can only be added while a campaign is in draft."),
            StateAfterRefusal = MsfCampaignState.Open
        };
        var cut = Render(sender);

        SubmitInvitee(cut, "peer-4@example.test");

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger")).Should().Be("Invitations can only be added while a campaign is in draft."));
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<AddMsfInvitationCommand>();
        Text(cut.Find("#msf-campaign-state")).Should().Be("Open");
        cut.FindAll("#msf-respondent-email").Should().BeEmpty();

        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void AnAddRefusedOnACampaignStillADraft_LeavesTheFormAndTheFocusForTheRetry()
    {
        // The refusal asks for the invitee to be added again, so the form stays, and the focus with it.
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)))
        {
            Refusal = new InvalidOperationException(AddMsfInvitationCommandHandler.CampaignChanged)
        };
        var cut = Render(sender);

        SubmitInvitee(cut, "peer-4@example.test");

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger")).Should().Be(AddMsfInvitationCommandHandler.CampaignChanged));
        Text(cut.Find("#msf-campaign-state")).Should().Be("Draft");
        cut.Find("#msf-respondent-email");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier);
    }

    [Fact]
    public void TheInviteePicker_NamesEachGroupByItsLabel()
    {
        var cut = Render(new CampaignSender(Setup(MsfCampaignState.Draft)));

        var options = cut.FindAll("#msf-respondent-category option");
        options.Select(Text).Should().Equal("Peer doctor", "Consultant", "Nurse", "Allied health professional");
        options.Select(option => option.GetAttribute("value")).Should().Equal("PeerDoctor", "Consultant", "Nurse", "Ahp");
    }

    // ─── Opening ─────────────────────────────────────────────────────────────

    [Fact]
    public void OpeningACampaign_ShowsItOpen_WithNoOpenAndNoInviteeForm_AndMovesTheFocusToTheResult()
    {
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)));
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Open"));
        Text(cut.Find(".alert-success")).Should().Be("Campaign opened, and each respondent has been emailed a link to respond.");
        cut.FindAll("#msf-open-campaign").Should().BeEmpty("the campaign is open");
        cut.FindAll("#msf-respondent-email").Should().BeEmpty("invitees are added only to a draft");
        HeaderCells(cut.Find("table")).Should().Equal("Respondent group", "Invited", "Responded");
        cut.Find("#msf-withdraw-campaign");
        Text(cut.Find("#msf-campaign-report")).Should().Be("View report");

        // The Open button that had the focus is gone.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void AnOpenRefusedBecauseTheCampaignWasOpenedElsewhere_ShowsTheRefusal_AndTheCampaignAsItIsNow()
    {
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)))
        {
            Refusal = new InvalidOperationException("Only draft campaigns can be opened."),
            StateAfterRefusal = MsfCampaignState.Open
        };
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger")).Should().Be("Only draft campaigns can be opened."));
        Text(cut.Find("#msf-campaign-state")).Should().Be("Open", "the page reads the campaign again after a refusal");
        cut.FindAll("#msf-open-campaign").Should().BeEmpty();

        // The Open button that had the focus is gone here too.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void ARefusedOpenOnACampaignStillADraft_LeavesTheOpenButtonAndTheFocusForTheRetry()
    {
        // A failed send asks the coordinator to open the campaign again (T184): the button stays, and so does the focus.
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)))
        {
            Refusal = new InvalidOperationException(OpenMsfCampaignCommandHandler.InvitationsNotSent)
        };
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger")).Should().Be(OpenMsfCampaignCommandHandler.InvitationsNotSent));
        Text(cut.Find("#msf-campaign-state")).Should().Be("Draft");
        cut.Find("#msf-open-campaign").HasAttribute("disabled").Should().BeFalse();
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier);
    }

    [Fact]
    public void ARefusal_IsKept_WhenTheCampaignCannotThenBeReadAgain()
    {
        // The refusal says why the open was not taken; the card says the campaign could not be loaded. (T217 review)
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)))
        {
            Refusal = new InvalidOperationException(OpenMsfCampaignCommandHandler.InvitationsNotSent),
            ReadFailureAfterRefusal = new InvalidOperationException("The database could not be reached.")
        };
        var cut = Render(sender);

        cut.Find("#msf-open-campaign").Click();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Campaign unavailable"));
        cut.FindAll(".alert-danger").Select(Text).Should().Equal(OpenMsfCampaignCommandHandler.InvitationsNotSent);
    }

    // ─── Withdrawing ─────────────────────────────────────────────────────────

    [Fact]
    public void Withdraw_AsksFirst_NamingTheCampaign_AndSendsNothingUntilConfirmed()
    {
        var sender = new CampaignSender(Setup(MsfCampaignState.Open, Group(MsfRespondentCategory.PeerDoctor, 3, 1)));
        var cut = Render(sender);

        var button = cut.Find("#msf-withdraw-campaign");
        button.ClassList.Should().Contain(["btn", "btn-outline"]).And.NotContain("btn-danger", "the red button belongs only in the dialog");
        button.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty();
        Text(cut.Find("dialog")).Should().Contain("Withdraw this campaign?")
            .And.Contain("Withdraw the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)?")
            .And.Contain("withdrawing cannot be undone");

        cut.FindAll("dialog button").Single(candidate => Text(candidate) == "Cancel").Click();
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void ConfirmingWithdraw_WithdrawsAsTheCaller_ShowsItWithdrawn_AndTheResultTakesTheFocusAfterTheDialogCloses()
    {
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)));
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();
        cut.FindAll("dialog button").Single(candidate => Text(candidate) == "Withdraw campaign").Click();

        var command = sender.Commands.Should().ContainSingle().Which.Should().BeOfType<WithdrawMsfCampaignCommand>().Subject;
        command.CampaignId.Should().Be(CampaignId);
        command.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("coordinator-1");

        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Withdrawn"));
        var status = cut.Find(".alert-success");
        status.GetAttribute("role").Should().Be("status");
        Text(status).Should().Be(
            "The campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21) has been withdrawn. Its respondents' links no " +
            "longer work, and their email addresses have been removed.");
        cut.FindAll("#msf-withdraw-campaign, #msf-open-campaign, #msf-respondent-email").Should().BeEmpty();
        BodyRows(cut.Find("table")).Should().Equal("Peer doctor | 3 | 0");

        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "the dialog closes before the result takes the focus");
    }

    [Fact]
    public void WhileAWithdrawRuns_NothingElseIsSent_AndASecondConfirmSendsNothing()
    {
        // As the campaign list's withdraw (T206) and this page's open (T202): a second click can reach the circuit before
        // the render that disables its button. A second withdraw would be refused as already withdrawn, and its refusal
        // would replace the first one's result. (T217 review)
        var sender = new CampaignSender(Setup(MsfCampaignState.Draft, Group(MsfRespondentCategory.PeerDoctor, 3, 0)))
        {
            Hold = true
        };
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();
        ConfirmWithdrawButton(cut).Click();
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<WithdrawMsfCampaignCommand>();

        cut.Find("#msf-withdraw-campaign").HasAttribute("disabled").Should().BeTrue("a withdraw is in flight");
        cut.Find("#msf-open-campaign").HasAttribute("disabled").Should().BeTrue("a withdraw is in flight");
        AddInviteeButton(cut).HasAttribute("disabled").Should().BeTrue("a withdraw is in flight");

        ConfirmWithdrawButton(cut).Click();
        cut.Find("#msf-withdraw-campaign").Click();
        cut.Find("#msf-open-campaign").Click();
        SubmitInvitee(cut, "peer-4@example.test");

        sender.Commands.Should().ContainSingle("one withdraw is in flight, and nothing may be sent beside it");
        JSInterop.Invocations.Count(invocation => invocation.Identifier == "wombatDialog.showModal")
            .Should().Be(1, "the dialog is not asked for again while the withdraw runs");

        sender.Release();
        cut.WaitForAssertion(() => Text(cut.Find("#msf-campaign-state")).Should().Be("Withdrawn"));
        sender.Commands.Should().ContainSingle();
        cut.FindAll(".alert-danger").Should().BeEmpty();
        cut.FindAll(".alert-success").Should().ContainSingle();
    }

    [Fact]
    public void ARefusedWithdraw_SaysWhy_AndShowsTheCampaignAsItIsNow()
    {
        var sender = new CampaignSender(Setup(MsfCampaignState.Open, Group(MsfRespondentCategory.PeerDoctor, 3, 3)))
        {
            Refusal = new InvalidOperationException("Only a draft or open campaign can be withdrawn, and this one has closed and is under review."),
            StateAfterRefusal = MsfCampaignState.UnderReview
        };
        var cut = Render(sender);

        cut.Find("#msf-withdraw-campaign").Click();
        cut.FindAll("dialog button").Single(candidate => Text(candidate) == "Withdraw campaign").Click();

        cut.WaitForAssertion(() => cut.Find(".alert-danger").GetAttribute("role").Should().Be("alert"));
        Text(cut.Find(".alert-danger")).Should().Contain("has closed and is under review");
        Text(cut.Find("#msf-campaign-state")).Should().Be("Under review");
        cut.FindAll("#msf-withdraw-campaign").Should().BeEmpty();
        Text(cut.Find("#msf-campaign-report")).Should().Be("Review and release");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private IRenderedComponent<CampaignEdit> Render(CampaignSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll("#msf-campaign-state").Count == 1);
        return cut;
    }

    private static IElement ConfirmWithdrawButton(IRenderedFragment cut)
        => cut.FindAll("dialog button").Single(candidate => Text(candidate) == "Withdraw campaign");

    private static IElement AddInviteeButton(IRenderedFragment cut)
        => cut.FindAll("button").Single(candidate => Text(candidate) == "Add invitee");

    private static void SubmitInvitee(IRenderedFragment cut, string email)
    {
        cut.Find("#msf-respondent-email").Change(email);
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();
    }

    private static MsfCampaignSetupDto Setup(MsfCampaignState state, params MsfInviteeCountDto[] invitees)
        => new(CampaignId, "Annual MSF", MsfTemplateKind.Msf, state,
            [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse, MsfRespondentCategory.Ahp])
        {
            SubjectName = "Sipho Dlamini",
            OpensOn = new DateOnly(2029, 3, 1),
            ClosesOn = new DateOnly(2029, 3, 21),
            Invitees = invitees
        };

    private static MsfInviteeCountDto Group(MsfRespondentCategory category, int invited, int responded)
        => new(category, invited, responded);

    /// <summary>The campaign's card: the section holding its state.</summary>
    private static IElement CampaignCard(IRenderedFragment cut)
        => cut.Find("#msf-campaign-state").Closest("section")!;

    private static IReadOnlyList<string> HeaderCells(IElement table)
        => table.QuerySelectorAll("thead th").Select(Text).ToList();

    private static IReadOnlyList<string> BodyRows(IElement table)
        => table.QuerySelectorAll("tbody tr").Select(RowText).ToList();

    private static string? FooterRow(IElement table)
        => table.QuerySelector("tfoot tr") is { } row ? RowText(row) : null;

    private static string RowText(IElement row)
        => string.Join(" | ", row.Children.Select(Text));

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>
    /// Answers the page's reads with the campaign as it stands, and adds to, opens and withdraws it as the handlers would:
    /// it reads back counted, open or withdrawn. A refusal of any of the three leaves it in
    /// <see cref="StateAfterRefusal" />, as a campaign changed elsewhere.
    /// </summary>
    private sealed class CampaignSender(MsfCampaignSetupDto setup) : IScopedSender
    {
        private MsfCampaignSetupDto _setup = setup;
        private TaskCompletionSource? _held;
        private bool _refused;

        /// <summary>Every add, open and withdraw sent, in order.</summary>
        public List<object> Commands { get; } = [];

        public Exception? Refusal { get; init; }

        public MsfCampaignState? StateAfterRefusal { get; init; }

        /// <summary>What every read of the campaign throws once a command has been refused.</summary>
        public Exception? ReadFailureAfterRefusal { get; init; }

        /// <summary>Holds each open and withdraw until <see cref="Release" />.</summary>
        public bool Hold { get; init; }

        public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is GetMsfCampaignSetupQuery && _refused && ReadFailureAfterRefusal is not null)
            {
                return Task.FromException<TResponse>(ReadFailureAfterRefusal);
            }

            if (request is AddMsfInvitationCommand add)
            {
                Commands.Add(add);
                if (Refusal is not null)
                {
                    Refuse();
                    return Task.FromException<TResponse>(Refusal);
                }

                // Counted as the query counts it: one more invited in the invitee's group, in the category's order.
                var groups = _setup.Invitees.ToDictionary(group => group.Category);
                groups[add.RespondentCategory] = groups.TryGetValue(add.RespondentCategory, out var group)
                    ? group with { Invited = group.Invited + 1 }
                    : new MsfInviteeCountDto(add.RespondentCategory, 1, 0);
                _setup = _setup with { Invitees = groups.Values.OrderBy(entry => entry.Category).ToList() };
                return Task.FromResult((TResponse)(object)groups.Values.Sum(entry => entry.Invited));
            }

            object? response = request switch
            {
                ListMsfTemplatesQuery => (IReadOnlyList<MsfTemplateDto>)[new MsfTemplateDto(1, "Annual MSF", null, false, true, [])],
                ListTraineesForSpecialityQuery => (IReadOnlyList<TraineeProfileDto>)[],
                GetMsfCampaignSetupQuery query when query.CampaignId == CampaignId => _setup,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);

            if (Refusal is not null)
            {
                Refuse();
                return Task.FromException(Refusal);
            }

            _setup = request switch
            {
                OpenMsfCampaignCommand => _setup with { State = MsfCampaignState.Open },
                WithdrawMsfCampaignCommand => _setup with { State = MsfCampaignState.Withdrawn },
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            if (Hold)
            {
                _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                return _held.Task;
            }

            return Task.CompletedTask;
        }

        private void Refuse()
        {
            _refused = true;
            if (StateAfterRefusal is { } state)
            {
                _setup = _setup with { State = state };
            }
        }
    }
}
