using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.SetUserLockout;
using Wombat.Application.Features.Users.Queries.GetRequestsWaitingForUser;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T284, then T376 (flow 12, Q5, Q17, F6; boards P02, P14–P18, P31, P32): Lockout says what a lock does beyond sign-in,
/// before the administrator presses Lock out…, naming only the effects that apply to this person; Lock out… asks in a
/// dialog, with the requests waiting for them read as it opens; Reactivate is one press; each result shows in Lockout.
/// </summary>
/// <remarks>
/// Until T284 it said only that locking "prevents sign-in immediately". Since T165, T240, T268 and T284 a lock also takes
/// the user off every committee panel, stops them being a current trainee, and stops their reminders; since T102 nobody
/// can name them as an assessor. Until T376 every person's list named all four, and the lock was one red press.
/// </remarks>
public sealed class UserDetailLockCardTests : UserDetailPageTestBase
{
    private const string Panel = "They sit on no committee panel.";

    private const string Trainee =
        "They are not a current trainee. No committee review or feedback campaign can be started about them, a draft " +
        "feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the " +
        "speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions " +
        "due, and the weekly digest's inactive trainees.";

    private const string Reminders = "They are sent no activity reminders or weekly digests.";
    private const string Assessor = "They cannot be named as an activity's assessor.";

    private const string Records =
        "Their records stay as they are, and a review or feedback campaign already under way can still be finished. " +
        "Reactivating clears the lockout.";

    [Fact]
    public void BeforeALock_ThePartNamesWhatALockDoes_BesideTheTrigger()
    {
        var part = Part(Open(Zulu(WombatRoles.Trainee, WombatRoles.CommitteeMember, WombatRoles.Assessor)), "Lockout");

        Text(part.QuerySelector("p")!).Should().Be("Locking out the user prevents sign-in immediately. Until they are reactivated:");
        part.QuerySelectorAll("#lockout-effects li").Select(Text).Should().Equal(Panel, Trainee, Reminders, Assessor);
        Text(part).Should().Contain(Records);

        // The effects come before the trigger, so they are read before it is reached.
        var trigger = part.QuerySelectorAll("button").Single(candidate => Text(candidate) == "Lock out…");
        part.QuerySelector("#lockout-effects")!.CompareDocumentPosition(trigger).Should().HaveFlag(DocumentPositions.Following);
    }

    [Theory]
    [InlineData(new[] { WombatRoles.Assessor }, new[] { Reminders, Assessor })]                     // P14 Patel
    [InlineData(new[] { WombatRoles.Trainee }, new[] { Trainee, Reminders })]                       // P18 Dlamini
    [InlineData(new[] { WombatRoles.CommitteeMember }, new[] { Panel, Reminders })]                 // P02 Zulu before 2.13
    [InlineData(new[] { WombatRoles.CommitteeMember, WombatRoles.Assessor }, new[] { Panel, Reminders, Assessor })]
    [InlineData(new string[0], new[] { Reminders })]                                                // P09 no role
    public void Q17_TheListNamesOnlyTheEffectsThatApplyToThisPerson(string[] roles, string[] effects)
    {
        var part = Part(Open(Zulu(roles) with { Roles = roles }), "Lockout");

        part.QuerySelectorAll("#lockout-effects li").Select(Text).Should().Equal(effects);
    }

    [Fact]
    public void P15_LockOut_OpensItsDialog_KeepAccessFirst_WithTheEffectsAndTheWaitingRequest()
    {
        Sender.Waiting = new WaitingRequestsDto(1, new DateTimeOffset(2026, 10, 12, 10, 35, 0, TimeSpan.Zero),
            [new WaitingRow("Pieter du Plessis", "Portfolio and Logbook Review (Paediatrics)", "PAED-015",
                new DateTimeOffset(2026, 10, 12, 10, 35, 0, TimeSpan.Zero), true)]);
        var cut = Open(Patel());
        Sender.Received.OfType<GetRequestsWaitingForUserQuery>().Should().BeEmpty("F6: never on a page view");

        Named(cut, "Lock out Mohammed Patel").Click();

        Sender.Received.OfType<GetRequestsWaitingForUserQuery>().Should().ContainSingle("read as the dialog opens");
        var dialog = Dialog(cut, "Lock out Mohammed Patel?");
        Text(dialog.QuerySelector(".dialog-body > p")!).Should().Be(
            "Mohammed Patel can no longer sign in, and any open session ends within a minute. Until they are reactivated:");
        dialog.QuerySelectorAll(".dialog-body > ul.dialog-list li").Select(Text).Should().Equal(Reminders, Assessor);
        var waiting = dialog.QuerySelector(".alert-warning")!;
        Text(waiting.QuerySelector("p")!).Should().Be(
            "1 request is waiting for Mohammed Patel, since 2026-10-12 12:35 SAST. It stays with them; it does not move to " +
            "anyone else, and no reminder is sent about it.");
        Text(waiting.QuerySelector("strong")!).Should().Be("1 request is waiting for Mohammed Patel");
        Text(dialog.QuerySelectorAll(".dialog-body > p").Last()).Should().Be(Records);
        var buttons = dialog.QuerySelectorAll(".form-actions button");
        buttons.Select(Text).Should().Equal("Keep access", "Lock out Mohammed Patel");
        buttons[1].ClassList.Should().Contain("btn-danger");
        Sender.Commands.Should().BeEmpty("nothing is locked until the dialog's button is pressed");
    }

    [Fact]
    public void P31_TwoWaiting_GiveTheCountAndTheOldest()
    {
        var oldest = new DateTimeOffset(2026, 10, 12, 10, 35, 0, TimeSpan.Zero);
        Sender.Waiting = new WaitingRequestsDto(2, oldest,
        [
            new WaitingRow("Pieter du Plessis", "Portfolio and Logbook Review (Paediatrics)", "PAED-015", oldest, true),
            new WaitingRow("Anele Dlamini", "Mini-CEX", null, oldest.AddDays(5), false)
        ]);
        var cut = Open(Patel());

        Named(cut, "Lock out Mohammed Patel").Click();

        var waiting = Dialog(cut, "Lock out Mohammed Patel?").QuerySelector(".alert-warning")!;
        Text(waiting.QuerySelector("p")!).Should().Be(
            "2 requests are waiting for Mohammed Patel, the oldest since 2026-10-12 12:35 SAST. They stay with them; they do " +
            "not move to anyone else, and no reminder is sent about them.");
        waiting.QuerySelectorAll("li").Select(Text).Should().Equal(
            "Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue",
            "Anele Dlamini · Mini-CEX · waiting since 2026-10-17 12:35 SAST");
    }

    [Fact]
    public void P32_MoreThanFive_ListFiveAndSayHowManyMore()
    {
        var oldest = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);
        Sender.Waiting = new WaitingRequestsDto(8, oldest,
            Enumerable.Range(0, 5).Select(day => new WaitingRow("Pieter du Plessis", "Mini-CEX", null, oldest.AddDays(day), true)).ToList());
        var cut = Open(Patel());

        Named(cut, "Lock out Mohammed Patel").Click();

        var rows = Dialog(cut, "Lock out Mohammed Patel?").QuerySelectorAll(".alert-warning li").Select(Text).ToList();
        rows.Should().HaveCount(6);
        rows[^1].Should().Be("and 3 more.");
    }

    [Fact]
    public void P16_TheLock_IsConfirmed_ItsResultInLockout_Focused_AndStatusReadsLockedOut()
    {
        Sender.After = _ => Patel() with { IsLockedOut = true };
        var cut = Open(Patel());
        Named(cut, "Lock out Mohammed Patel").Click();

        DialogButton(cut, "Lock out Mohammed Patel").Click();

        Sender.Commands.OfType<SetUserLockoutCommand>().Single().Locked.Should().BeTrue();
        var part = Part(cut, "Lockout");
        var alert = part.QuerySelector(".action-result .alert-success")!;
        Text(alert).Should().Be("Mohammed Patel is locked out. Any open session ends within a minute.");
        Text(alert.QuerySelector("strong")!).Should().Be("Mohammed Patel is locked out.");
        Text(part).Should().Contain("This user is currently locked out and cannot sign in.");
        part.QuerySelectorAll("button").Select(Text).Should().Equal("Reactivate user");
        part.QuerySelector("#lockout-effects").Should().BeNull();
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1));
    }

    [Fact]
    public void P17_Reactivate_IsOnePress_ItsResultInLockout()
    {
        Sender.After = _ => Patel();
        var cut = Open(Patel() with { IsLockedOut = true });

        Button(cut, "Reactivate user").Click();

        Sender.Commands.OfType<SetUserLockoutCommand>().Single().Locked.Should().BeFalse();
        Text(Part(cut, "Lockout").QuerySelector(".action-result .alert-success")!)
            .Should().Be("Mohammed Patel is reactivated and can sign in again.");
        Part(cut, "Lockout").QuerySelectorAll("button").Select(Text).Should().Equal("Lock out…");
    }

    [Fact]
    public void ForAnAdministrator_TheAdministratorsAccountOffersNoLock_AndListsNoEffects()
    {
        SignIn(WombatRoles.Administrator);
        var part = Part(Open(Zulu(WombatRoles.Administrator)), "Lockout");

        Text(part).Should().Contain("Global administrators cannot be locked out from this surface.");
        part.QuerySelector("#lockout-effects").Should().BeNull();
    }

    private static UserDetailDto Patel() => new(
        "patel", "patel@kgk.wombat.local", "Mohammed", "Patel", Kgk, KgkName, [10], [20], [WombatRoles.Assessor], false)
    {
        Scope = "Paediatrics / Paediatrics", SpecialityNames = ["Paediatrics"], SubSpecialityNames = ["Paediatrics"]
    };
}
