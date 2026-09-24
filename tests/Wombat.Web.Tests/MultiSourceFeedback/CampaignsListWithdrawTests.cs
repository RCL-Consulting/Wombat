using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// A Coordinator withdraws a draft or an open campaign from the campaign list, after a confirmation. (T206)
/// </summary>
/// <remarks>
/// Until T206 <see cref="WithdrawMsfCampaignCommand" /> had no caller in the product, so T202's anonymise-on-withdraw
/// ran only in tests. Withdrawing kills every link a respondent holds and removes their addresses for good, so the row's
/// button asks first, as DESIGN.md asks of every destructive action, and the red button is only in the dialog.
/// </remarks>
public sealed class CampaignsListWithdrawTests : TestContext
{
    private static readonly DateOnly ClosesOn = new(2029, 3, 21);

    public CampaignsListWithdrawTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    [Fact]
    public void Withdraw_IsOfferedOnADraftOrAnOpenCampaign_AndOnNoOther()
    {
        var cut = RenderPage(new FakeSender(
            Campaign(1, "Thandi Nkosi", MsfCampaignState.Draft),
            Campaign(2, "Sipho Dlamini", MsfCampaignState.Open),
            Campaign(3, "Anna Botha", MsfCampaignState.UnderReview),
            Campaign(4, "Lerato Mokoena", MsfCampaignState.Released),
            Campaign(5, "Johan Smit", MsfCampaignState.Withdrawn)));

        RowsWithAWithdrawButton(cut).Should().Equal("Thandi Nkosi", "Sipho Dlamini");

        var button = WithdrawButton(cut, "Sipho Dlamini");
        button.ClassList.Should().Contain(["btn", "btn-sm", "btn-outline"])
            .And.NotContain("btn-danger", "the red button belongs only in the dialog's footer");
        button.GetAttribute("aria-label")
            .Should().Be("Withdraw the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)");
    }

    [Fact]
    public void Withdraw_OpensADialogNamingTheCampaign_AndSendsNothingUntilConfirmed()
    {
        var sender = new FakeSender(Campaign(2, "Sipho Dlamini", MsfCampaignState.Open));
        var cut = RenderPage(sender);

        WithdrawButton(cut, "Sipho Dlamini").Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("opening the dialog must not withdraw the campaign");

        var dialog = Text(cut.Find("dialog"));
        dialog.Should().Contain("Withdraw this campaign?")
            .And.Contain("Withdraw the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)?")
            .And.Contain("any link already sent stops working, and every respondent's email address is removed")
            .And.Contain("never released to the trainee, and withdrawing cannot be undone");
        cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign")
            .ClassList.Should().Contain("btn-danger");
    }

    [Fact]
    public void CancellingTheDialog_SendsNothing()
    {
        var sender = new FakeSender(Campaign(2, "Sipho Dlamini", MsfCampaignState.Open));
        var cut = RenderPage(sender);

        WithdrawButton(cut, "Sipho Dlamini").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Cancel").Click();

        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void ConfirmingTheDialog_WithdrawsThatCampaignAsTheCaller_AndTheListShowsItWithdrawn()
    {
        var sender = new FakeSender(
            Campaign(1, "Thandi Nkosi", MsfCampaignState.Draft),
            Campaign(2, "Sipho Dlamini", MsfCampaignState.Open));
        var cut = RenderPage(sender);

        WithdrawButton(cut, "Sipho Dlamini").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign").Click();

        var command = sender.Commands.Should().ContainSingle().Which.Should().BeOfType<WithdrawMsfCampaignCommand>().Subject;
        command.CampaignId.Should().Be(2);
        command.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("coordinator-1");

        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-success")).Should().Be(
            "The campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21) has been withdrawn. Its respondents' links " +
            "no longer work, and their email addresses have been removed."));
        StateOf(cut, "Sipho Dlamini").Should().Be("Withdrawn", "the list is read again after the withdraw");
        RowsWithAWithdrawButton(cut).Should().Equal("Thandi Nkosi");
    }

    [Fact]
    public void ARefusedWithdraw_SaysWhy_AndKeepsTheList()
    {
        var sender = new FakeSender(Campaign(2, "Sipho Dlamini", MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException("Released campaigns cannot be withdrawn.")
        };
        var cut = RenderPage(sender);

        WithdrawButton(cut, "Sipho Dlamini").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-danger")).Should().Be("Released campaigns cannot be withdrawn."));
        cut.FindAll(".alert.alert-success").Should().BeEmpty();
        StateOf(cut, "Sipho Dlamini").Should().Be("Open");
    }

    [Fact]
    public void ASecondConfirmWhileTheFirstWithdrawRuns_SendsNothing()
    {
        // As the campaign editor's open (T202): the second confirm can reach the circuit before the render that closes
        // the dialog does.
        var sender = new FakeSender(Campaign(2, "Sipho Dlamini", MsfCampaignState.Open)) { Hold = true };
        var cut = RenderPage(sender);

        WithdrawButton(cut, "Sipho Dlamini").Click();
        var confirm = cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign");
        confirm.Click();
        WithdrawButton(cut, "Sipho Dlamini").HasAttribute("disabled").Should().BeTrue("a withdraw is in flight");
        cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign").Click();

        sender.Commands.Should().ContainSingle();

        sender.Release();
        cut.WaitForAssertion(() => cut.FindAll(".alert.alert-success").Should().ContainSingle());
        sender.Commands.Should().ContainSingle();
    }

    /// <summary>
    /// The result is announced, and takes the focus once the dialog has closed: the row's Withdraw button, which had the
    /// focus before the dialog, is gone. (T206 review)
    /// </summary>
    [Fact]
    public void AWithdrawsResult_IsAnnounced_AndTakesTheFocusOnceTheDialogHasClosed()
    {
        var cut = RenderPage(new FakeSender(Campaign(2, "Sipho Dlamini", MsfCampaignState.Open)));

        WithdrawButton(cut, "Sipho Dlamini").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign").Click();

        cut.WaitForAssertion(() => cut.Find(".alert.alert-success").GetAttribute("role").Should().Be("status"));
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void ARefusedWithdraw_IsAnnouncedAsAnAlert_AndTakesTheFocusOnceTheDialogHasClosed()
    {
        var cut = RenderPage(new FakeSender(Campaign(2, "Sipho Dlamini", MsfCampaignState.Open))
        {
            Refusal = new InvalidOperationException("This campaign has already been withdrawn.")
        });

        WithdrawButton(cut, "Sipho Dlamini").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Withdraw campaign").Click();

        cut.WaitForAssertion(() => cut.Find(".alert.alert-danger").GetAttribute("role").Should().Be("alert"));
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    private void ResultTookTheFocusAfterTheDialogClosed(IRenderedComponent<CampaignsList> cut)
    {
        // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so the focused reference is
        // matched to the page's own result region, the one element that captures it.
        cut.Find(".action-result").GetAttribute("tabindex").Should().Be("-1");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));

        // While the modal is open, nothing outside it can take the focus.
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "the dialog closes before the result takes the focus");
    }

    /// <summary>What <see cref="Microsoft.AspNetCore.Components.ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private IRenderedComponent<CampaignsList> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CampaignsList>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);
        return cut;
    }

    private static MsfCampaignSummaryDto Campaign(int id, string subjectName, MsfCampaignState state)
        => new(id, $"trainee-{id}", "Annual MSF", ClosesOn.AddDays(-20), ClosesOn, 8, 3, state, 3, 0, null)
        {
            SubjectName = subjectName
        };

    private static IReadOnlyList<string> RowsWithAWithdrawButton(IRenderedFragment cut)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelectorAll("button").Any(button => Text(button) == "Withdraw"))
            .Select(row => Text(row.QuerySelector("td")!))
            .ToList();

    private static IElement WithdrawButton(IRenderedFragment cut, string subjectName)
        => cut.FindAll("tbody tr")
            .Single(row => Text(row.QuerySelector("td")!) == subjectName)
            .QuerySelectorAll("button")
            .Single(button => Text(button) == "Withdraw");

    private static string StateOf(IRenderedFragment cut, string subjectName)
        => Text(cut.FindAll("tbody tr")
            .Single(row => Text(row.QuerySelector("td")!) == subjectName)
            .QuerySelectorAll("td")[2]);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>Answers the list, and withdraws as the handler would: the campaign reads back as withdrawn.</summary>
    private sealed class FakeSender(params MsfCampaignSummaryDto[] campaigns) : IScopedSender
    {
        private readonly List<MsfCampaignSummaryDto> _campaigns = [.. campaigns];

        public List<IRequest> Commands { get; } = [];

        public Exception? Refusal { get; init; }

        /// <summary>Holds each withdraw until <see cref="Release" />.</summary>
        public bool Hold { get; init; }

        private TaskCompletionSource? _held;

        public void Release() => (_held ?? throw new InvalidOperationException("No withdraw was sent.")).SetResult();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListMsfCampaignsForCoordinatorQuery => (IReadOnlyList<MsfCampaignSummaryDto>)_campaigns.ToList(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);

            if (Refusal is not null)
            {
                return Task.FromException(Refusal);
            }

            if (request is WithdrawMsfCampaignCommand withdraw)
            {
                var index = _campaigns.FindIndex(campaign => campaign.Id == withdraw.CampaignId);
                _campaigns[index] = _campaigns[index] with { State = MsfCampaignState.Withdrawn };
            }

            if (Hold)
            {
                _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                return _held.Task;
            }

            return Task.CompletedTask;
        }
    }
}
