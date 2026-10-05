using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Programme;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// Send a reminder (T358, flow 06; C4; round 3 items 22, 23, 29): the Waiting cell's tail in every state the boards draw
/// (R2-Waiting w1–w9, R2-Registrar r2, r2d, r2r, r2x). The last reminder's record, then the button, the "No reminder: …"
/// line, or nothing; the dialog on its safe button; the confirm in flight; and the answer handed to the host once the
/// dialog has closed, whose result region then takes the focus.
/// </summary>
public sealed class ReminderActionTests : WombatTestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private static readonly DateTime UpdatedOn = new(2026, 9, 26, 6, 10, 0, DateTimeKind.Utc);

    private readonly HeldSender _sender = new();

    public ReminderActionTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("smit@kgk.test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "smit"));
        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- w1, r2: the button ----

    [Fact]
    public void ARemindableRow_OffersTheButton_NamedForTheAssessorAndTheActivity()
    {
        var cut = Render(Row());

        var button = cut.Find("button.reminder-action");
        button.ClassList.Should().Equal("btn", "btn-outline", "btn-sm", "reminder-action");
        button.TextContent.Should().Be("Send a reminder");
        button.GetAttribute("aria-label").Should().Be(
            "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu");
        cut.FindAll(".waited-cell .progress-row-meta").Should().BeEmpty("no reminder has been sent");
    }

    /// <summary>Round 3 item 29: two rows that read the same; the button says "(1 of 2)" as the link does.</summary>
    [Fact]
    public void TwoRowsThatReadTheSame_NameTheButtonByTheLinksOwnName()
    {
        var cut = Render(Row(), linkName: "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu, Requested (1 of 2)");

        cut.Find("button.reminder-action").GetAttribute("aria-label").Should().Be(
            "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu, Requested (1 of 2)");
    }

    // ---- w2, r2d: the dialog ----

    [Fact]
    public void TheButton_OpensTheDialog_OnItsSafeButton_NamingTheMailAndWhatItDoesNotDo()
    {
        var cut = Render(Row());

        cut.Find("button.reminder-action").Click();

        JSInterop.Invocations.Select(invocation => invocation.Identifier).Should().Contain("wombatDialog.showModal");
        var dialog = cut.Find("dialog");
        dialog.QuerySelector("h2")!.TextContent.Should().Be("Send Thandi Zulu a reminder?");
        dialog.QuerySelector("p")!.TextContent.Should().Be(
            "Thandi Zulu gets one email, \"Activities awaiting your assessment\", listing this request: Mini-CEX (Paediatrics) " +
            "from Nomsa Mahlangu — waiting 8 days. It moves nothing: the request stays Requested, its wait is not restarted, " +
            "and Nomsa Mahlangu is not told.");
        var buttons = dialog.QuerySelectorAll("button").ToList();
        buttons.Select(button => button.TextContent).Should().Equal("Don't send", "Send the reminder");
        buttons[0].ClassList.Should().Contain("btn-outline", "the safe button comes first, so the modal opens on it");
        _sender.Received.Should().BeEmpty("opening the dialog sends nothing");
    }

    // ---- w2b: in flight ----

    [Fact]
    public void WhileTheReminderIsOnItsWay_TheConfirmSaysSending_AndIsAriaDisabled_AndDontSendIsDisabled()
    {
        var cut = Render(Row());
        cut.Find("button.reminder-action").Click();

        Confirm(cut).Click();
        cut.WaitForAssertion(() => _sender.Received.Should().ContainSingle());

        var confirm = Confirm(cut);
        confirm.TextContent.Should().Be("Sending…");
        confirm.GetAttribute("aria-disabled").Should().Be("true");
        confirm.HasAttribute("disabled").Should().BeFalse("it keeps the focus");
        cut.Find("dialog button.btn-outline").HasAttribute("disabled").Should().BeTrue();

        Confirm(cut).Click();
        _sender.Received.Should().ContainSingle("a second press sends nothing");

        _sender.Release(Sent());
        cut.WaitForAssertion(() => cut.Instance.Answers.Should().ContainSingle(), AsyncWorkTimeout);
    }

    /// <summary>The command carries the role read as, and the state and last move the row was read with.</summary>
    [Fact]
    public void TheCommand_IsTheRowAsRead_SentAsTheActingRole()
    {
        var cut = Render(Row(), actingRole: WombatRoles.SpecialityAdmin);
        cut.Find("button.reminder-action").Click();
        Confirm(cut).Click();

        cut.WaitForAssertion(() => _sender.Received.Should().ContainSingle());
        var command = _sender.Received.Single().Should().BeOfType<SendActivityReminderCommand>().Subject;
        (command.ActingRole, command.ActivityId, command.ExpectedState, command.ExpectedUpdatedOn)
            .Should().Be((WombatRoles.SpecialityAdmin, 42, "requested", UpdatedOn));
        command.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("smit");
        _sender.Release(Sent());
    }

    // ---- w3, r2r: the result, after the dialog has closed, taking the focus ----

    [Fact]
    public void TheAnswer_IsHandedToTheHost_OnceTheDialogHasClosed_AndItsResultTakesTheFocus()
    {
        var cut = Render(Row());
        cut.Find("button.reminder-action").Click();
        Confirm(cut).Click();
        cut.WaitForAssertion(() => _sender.Received.Should().ContainSingle());
        FocusCalls().Should().BeEmpty("nothing has answered yet");

        _sender.Release(Sent());

        var region = cut.FindComponent<ActionResult>();
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(region.Instance.Element.Id), AsyncWorkTimeout);
        var identifiers = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        identifiers.IndexOf("wombatDialog.close").Should().BeLessThan(identifiers.IndexOf(FocusIdentifier),
            "while a modal dialog is open nothing outside it can take the focus");
        region.Find(".alert-success").TextContent.Should().Be(
            "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu, " +
            "waiting 8 days. The request is still Requested; its wait is unchanged.");
        region.Find(".alert-success").GetAttribute("role").Should().Be("status");
    }

    // ---- w5–w8, r2x: refusals ----

    public static TheoryData<ReminderOutcome, string> Refusals() => new()
    {
        { ReminderOutcome.Deactivated, "Not sent. Thandi Zulu's account is deactivated, so Wombat sends Thandi Zulu no email. The request still waits." },
        { ReminderOutcome.NoEmail, "Not sent. Thandi Zulu has no email address in Wombat. Ask your institutional admin to add one." },
        { ReminderOutcome.NoAccount, "Not sent. Wombat has no account for the person this request names; it was erased or deleted, so there is nobody to email. The request still waits." },
        { ReminderOutcome.RemindedToday, "Not sent. Pieter Smit reminded Thandi Zulu today already." },
        { ReminderOutcome.MovedMeanwhile, "Not sent. Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01 moved at 2026-10-04 08:12 SAST: it is now Completed and waits for nobody." },
        { ReminderOutcome.NotFound, "Not sent. This request is no longer on your list." }
    };

    [Theory]
    [MemberData(nameof(Refusals))]
    public void ARefusal_IsHandedToTheHost_AsAnAlert_ThatTakesTheFocus(ReminderOutcome outcome, string words)
    {
        var cut = Render(Row());
        cut.Find("button.reminder-action").Click();
        Confirm(cut).Click();
        cut.WaitForAssertion(() => _sender.Received.Should().ContainSingle());

        _sender.Release(Refused(outcome));

        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle(), AsyncWorkTimeout);
        var alert = cut.Find(".action-result .alert-danger");
        alert.TextContent.Should().Be(words);
        alert.GetAttribute("role").Should().Be("alert");
    }

    /// <summary>A send that fails is not a refusal: fixed words, never the exception's text (T329).</summary>
    [Fact]
    public void ASendThatFails_IsHandedToTheHostAsAFailure_InFixedWords()
    {
        var cut = Render(Row());
        cut.Find("button.reminder-action").Click();
        Confirm(cut).Click();
        cut.WaitForAssertion(() => _sender.Received.Should().ContainSingle());

        _sender.Fail(new InvalidOperationException("a secret connection string"));

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger").TextContent.Should().Be(
            "Not sent. Something went wrong, and no reminder was sent. Try again, or come back in a few minutes."), AsyncWorkTimeout);
        cut.Markup.Should().NotContain("secret");
        cut.Instance.Answers.Should().BeEmpty();
    }

    // ---- w4, w4b, w9: the record and the line ----

    /// <summary>w4: a same-day reminder blocks every staff member, so the record shows and no button.</summary>
    [Fact]
    public void RemindedToday_ShowsTheRecord_AndNoButton()
    {
        var cut = Render(Row() with { LastReminder = Reminder(new DateOnly(2026, 10, 4)), RemindedToday = true });

        cut.FindAll("button.reminder-action").Should().BeEmpty();
        Lines(cut).Should().Equal("Reminded 2026-10-04 by Pieter Smit");
    }

    /// <summary>w4b: the next day the last reminder's record stays and the button is back.</summary>
    [Fact]
    public void TheNextDay_TheRecordStays_AndTheButtonIsBack()
    {
        var cut = Render(Row() with { LastReminder = Reminder(new DateOnly(2026, 10, 4)), RemindedToday = false });

        Lines(cut).Should().Equal("Reminded 2026-10-04 by Pieter Smit");
        cut.FindAll("button.reminder-action").Should().ContainSingle();
        var cell = cut.Find(".waited-cell");
        cell.Children.Select(child => child.TagName.ToLowerInvariant()).Should().StartWith(["b", "span", "span", "button"],
            "the wait, its since, the record, then the button");
    }

    public static TheoryData<ReminderOutcome, string> CannotRemind() => new()
    {
        { ReminderOutcome.Deactivated, "No reminder: Thandi Zulu's account is deactivated." },
        { ReminderOutcome.NoEmail, "No reminder: Thandi Zulu has no email address in Wombat." },
        { ReminderOutcome.NoAccount, "No reminder: the person this request names has no account." }
    };

    /// <summary>E3's w9: a nominee who cannot be reminded is said so in the row, before anyone presses.</summary>
    [Theory]
    [MemberData(nameof(CannotRemind))]
    public void ANomineeWhoCannotBeReminded_IsSaidSo_AndThereIsNoButton(ReminderOutcome reason, string words)
    {
        var cut = Render(Row() with { CannotRemind = reason });

        cut.FindAll("button.reminder-action").Should().BeEmpty();
        Lines(cut).Should().Equal(words);
    }

    // ---- helpers ----

    private IRenderedComponent<ReminderHost> Render(ActivitySummaryDto item, string? linkName = null, string actingRole = WombatRoles.Coordinator)
        => RenderComponent<ReminderHost>(parameters => parameters
            .Add(host => host.Item, item)
            .Add(host => host.LinkName, linkName)
            .Add(host => host.ActingRole, actingRole));

    private static AngleSharp.Dom.IElement Confirm(IRenderedComponent<ReminderHost> cut) => cut.Find("dialog button.btn-primary");

    private static List<string> Lines(IRenderedComponent<ReminderHost> cut)
        => cut.FindAll(".waited-cell .progress-row-meta").Select(line => line.TextContent).ToList();

    private IEnumerable<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier);

    /// <summary>R2-Waiting's Mini-CEX: PAED-004, observed 2026-10-01, from Nomsa Mahlangu to Thandi Zulu, 8 days.</summary>
    private static ActivitySummaryDto Row()
        => ActivityRows.Waiting(
                42, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true, since: UpdatedOn, epaCode: "PAED-004",
                observedOn: new DateOnly(2026, 10, 1)) with
            {
                Holder = new ActivityHolderDto(ActivityHolderKind.Person, "zulu", "Thandi Zulu", false, UpdatedOn),
                NomineeName = "Thandi Zulu"
            };

    private static ActivityReminderDto Reminder(DateOnly day)
        => new(day.ToDateTime(new TimeOnly(6, 0), DateTimeKind.Utc), day, "Pieter Smit");

    private static SendActivityReminderResult Sent()
        => new(ReminderOutcome.Sent, "Thandi Zulu", "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8,
            "Requested", null, Reminder(new DateOnly(2026, 10, 4)));

    private static SendActivityReminderResult Refused(ReminderOutcome outcome)
        => outcome == ReminderOutcome.NotFound
            ? SendActivityReminderResult.NotFound
            : new(outcome, outcome == ReminderOutcome.MovedMeanwhile ? null : "Thandi Zulu",
                "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8,
                outcome == ReminderOutcome.MovedMeanwhile ? "Completed" : "Requested",
                outcome == ReminderOutcome.MovedMeanwhile ? new DateTime(2026, 10, 4, 6, 12, 0, DateTimeKind.Utc) : null,
                outcome == ReminderOutcome.RemindedToday ? Reminder(new DateOnly(2026, 10, 4)) : null);

    /// <summary>Holds the reminder's command open until the test answers it.</summary>
    private sealed class HeldSender : IScopedSender
    {
        private TaskCompletionSource<SendActivityReminderResult> _answer = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public List<object> Received { get; } = [];

        public void Release(SendActivityReminderResult result) => _answer.TrySetResult(result);

        public void Fail(Exception exception) => _answer.TrySetException(exception);

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            return (TResponse)(object)await _answer.Task;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
