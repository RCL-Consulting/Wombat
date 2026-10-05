using System.Text.RegularExpressions;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Web.Components.Shared.Programme;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// Send a reminder's words (T358, flow 06; C4, E1, D7; round 3 items 23, 29; R2-Waiting w2–w9, R2-Registrar r2d, r2r,
/// r2x), each as the boards give it for the cast, and none with a third-person pronoun (round 3 check 1).
/// </summary>
public sealed partial class ReminderWordsTests
{
    private static readonly DateTime UpdatedOn = new(2026, 9, 26, 6, 10, 0, DateTimeKind.Utc);

    [Fact]
    public void TheButton_AndItsName()
    {
        ReminderWords.Button.Should().Be("Send a reminder");
        ReminderWords.ButtonName(Row(), null).Should().Be(
            "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu");
        ReminderWords.ButtonName(Row(), "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu, Requested (1 of 2)")
            .Should().Be("Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu, Requested (1 of 2)");
    }

    [Fact]
    public void TheDialog_NamesTheAssessor_TheMail_AndWhatItDoesNotDo()
    {
        ReminderWords.DialogTitle(Row()).Should().Be("Send Thandi Zulu a reminder?");
        ReminderWords.DialogMail(Row()).Should().Be(
            "Thandi Zulu gets one email, \"Activities awaiting your assessment\", listing this request: Mini-CEX (Paediatrics) " +
            "from Nomsa Mahlangu — waiting 8 days.");
        ReminderWords.DialogMovesNothing(Row()).Should().Be(
            "It moves nothing: the request stays Requested, its wait is not restarted, and Nomsa Mahlangu is not told.");
        ReminderWords.DialogBody(Row()).Should().Be($"{ReminderWords.DialogMail(Row())} {ReminderWords.DialogMovesNothing(Row())}");
        (ReminderWords.Cancel, ReminderWords.Confirm, ReminderWords.Sending)
            .Should().Be(("Don't send", "Send the reminder", "Sending…"));
    }

    /// <summary>Review 3: the dialog says the wait as the mail will, never "0 days".</summary>
    [Theory]
    [InlineData(0, "waiting less than a day")]
    [InlineData(1, "waiting 1 day")]
    public void TheDialogsWait_IsTheMailsWords(int days, string phrase)
        => ReminderWords.DialogMail(Row() with { WaitedDays = days }).Should().EndWith($"— {phrase}.");

    [Fact]
    public void TheResult_SaysWhatWasSent_AndThatNothingMoved()
    {
        var sent = Result(ReminderOutcome.Sent);

        ReminderWords.IsRefusal(sent).Should().BeFalse();
        ReminderWords.Lead(sent).Should().Be("Reminder sent to Thandi Zulu.");
        ReminderWords.Result(sent).Should().Be(
            "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu, " +
            "waiting 8 days. The request is still Requested; its wait is unchanged.");
    }

    [Fact]
    public void EachRefusal_IsNotSent_AndWhy()
    {
        ReminderWords.Result(Result(ReminderOutcome.Deactivated, assessor: "Fatima Khumalo")).Should().Be(
            "Not sent. Fatima Khumalo's account is deactivated, so Wombat sends Fatima Khumalo no email. The request still waits.");
        ReminderWords.Result(Result(ReminderOutcome.NoEmail)).Should().Be(
            "Not sent. Thandi Zulu has no email address in Wombat. Ask your institutional admin to add one.");
        ReminderWords.Result(Result(ReminderOutcome.NoAccount)).Should().Be(
            "Not sent. Wombat has no account for the person this request names; it was erased or deleted, so there is nobody " +
            "to email. The request still waits.");
        ReminderWords.Result(Result(ReminderOutcome.RemindedToday) with { Reminder = Reminder() }).Should().Be(
            "Not sent. Pieter Smit reminded Thandi Zulu today already.");
        ReminderWords.Result(SendActivityReminderResult.NotFound).Should().Be(
            "Not sent. This request is no longer on your list.", "D7: never the moved words, which would tell its state");

        foreach (var outcome in Enum.GetValues<ReminderOutcome>().Where(outcome => outcome != ReminderOutcome.Sent))
        {
            ReminderWords.IsRefusal(Result(outcome)).Should().BeTrue();
            ReminderWords.Lead(Result(outcome)).Should().Be("Not sent.");
        }
    }

    [Fact]
    public void MovedMeanwhile_SaysWhatItIsNow_AndWhetherItStillWaits()
    {
        var gone = Result(ReminderOutcome.MovedMeanwhile, assessor: null) with
        {
            CurrentStateLabel = "Completed", MovedOn = new DateTime(2026, 10, 4, 6, 12, 0, DateTimeKind.Utc)
        };
        ReminderWords.Detail(gone).Should().Be(
            "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01 moved at 2026-10-04 08:12 SAST: it is now Completed and waits for nobody.");

        var handedOn = Result(ReminderOutcome.MovedMeanwhile, assessor: "Mohammed Patel") with
        {
            MovedOn = new DateTime(2026, 10, 4, 6, 12, 0, DateTimeKind.Utc), StillWaiting = true
        };
        ReminderWords.Detail(handedOn).Should().Be(
            "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01 moved at 2026-10-04 08:12 SAST: it is now Requested, with Mohammed Patel.");
    }

    [Fact]
    public void TheRecord_AndTheNoReminderLines()
    {
        ReminderWords.Reminded(Reminder()).Should().Be("Reminded 2026-10-04 by Pieter Smit");

        ReminderWords.CannotRemind(Row()).Should().BeNull();
        ReminderWords.CannotRemind(Row("Fatima Khumalo") with { CannotRemind = ReminderOutcome.Deactivated })
            .Should().Be("No reminder: Fatima Khumalo's account is deactivated.");
        ReminderWords.CannotRemind(Row() with { CannotRemind = ReminderOutcome.NoEmail })
            .Should().Be("No reminder: Thandi Zulu has no email address in Wombat.");
        ReminderWords.CannotRemind(Row() with { CannotRemind = ReminderOutcome.NoAccount })
            .Should().Be("No reminder: the person this request names has no account.");
    }

    /// <summary>Round 3 check 1: the name, "the registrar" or "the assessor", never she, her, he, him or his.</summary>
    [Fact]
    public void NoWord_UsesAThirdPersonPronoun()
    {
        var words = new List<string>
        {
            ReminderWords.Button, ReminderWords.Cancel, ReminderWords.Confirm, ReminderWords.Sending, ReminderWords.Failed,
            ReminderWords.ButtonName(Row(), null), ReminderWords.DialogTitle(Row()), ReminderWords.DialogBody(Row()),
            ReminderWords.Reminded(Reminder())
        };
        words.AddRange(Enum.GetValues<ReminderOutcome>().Select(outcome => ReminderWords.Result(Result(outcome) with { Reminder = Reminder() })));
        words.AddRange(new[] { ReminderOutcome.Deactivated, ReminderOutcome.NoEmail, ReminderOutcome.NoAccount }
            .Select(reason => ReminderWords.CannotRemind(Row() with { CannotRemind = reason })!));

        words.Should().AllSatisfy(word => Pronoun().IsMatch(word).Should().BeFalse(word));
    }

    [GeneratedRegex(@"\b(she|her|hers|he|him|his|they|them|their)\b", RegexOptions.IgnoreCase)]
    private static partial Regex Pronoun();

    private static ActivitySummaryDto Row(string assessor = "Thandi Zulu")
        => ActivityRows.Waiting(
                42, subjectName: "Nomsa Mahlangu", waitedDays: 8, overdue: true, since: UpdatedOn, epaCode: "PAED-004",
                observedOn: new DateOnly(2026, 10, 1)) with
            {
                Holder = new ActivityHolderDto(ActivityHolderKind.Person, "assessor", assessor, false, UpdatedOn),
                NomineeName = assessor
            };

    private static ActivityReminderDto Reminder()
        => new(new DateTime(2026, 10, 4, 6, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 10, 4), "Pieter Smit");

    private static SendActivityReminderResult Result(ReminderOutcome outcome, string? assessor = "Thandi Zulu")
        => new(outcome, assessor, "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8, "Requested",
            new DateTime(2026, 10, 4, 6, 12, 0, DateTimeKind.Utc), null);
}
