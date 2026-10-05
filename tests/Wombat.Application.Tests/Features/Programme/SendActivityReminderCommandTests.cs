using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Tests.Scheduling;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using static Wombat.Application.Tests.Features.Programme.ProgrammeWaitingFixture;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// Send a reminder (T358, flow 06; Q4, C4, E1, D6, D7; review 1–3): one email to the assessor a waiting request names,
/// and a record of who sent it when, one per request per South African day. It moves nothing, and it refuses by
/// answering, with nothing staged.
/// </summary>
public sealed class SendActivityReminderCommandTests
{
    [Fact]
    public async Task AReminder_IsRecorded_AndOneMailIsHandedOver_InTheNudgesWords()
    {
        await using var db = Seeded();
        var request = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        await db.SaveChangesAsync();
        var mail = new RecordingEmailSender();

        var result = await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, request);

        result.Outcome.Should().Be(ReminderOutcome.Sent);
        result.AssessorName.Should().Be("Thandi Zulu");
        result.SubjectName.Should().Be("Nomsa Mahlangu");
        result.ActivityName.Should().StartWith("Mini-CEX");
        result.WaitedDays.Should().Be(8);
        result.CurrentStateLabel.Should().Be("Requested");
        result.Reminder.Should().Be(new Wombat.Application.Features.Programme.Waiting.ActivityReminderDto(
            Now, new DateOnly(2026, 10, 4), "Pieter Smit"));

        var row = await db.ActivityReminders.AsNoTracking().SingleAsync();
        (row.ActivityId, row.SentByUserId, row.AssessorUserId, row.SentOn, row.SentOnDay)
            .Should().Be((1, Smit, Zulu, Now, new DateOnly(2026, 10, 4)));

        var sent = mail.To(Zulu);
        sent.Subject.Should().Be("Activities awaiting your assessment");
        sent.Tags.Should().Equal("reminder", "assessor-reminder");
        sent.TextBody.Should().Contain("Hi Thandi,").And.Contain("Mini-CEX from Nomsa Mahlangu — waiting 8 days");
    }

    /// <summary>C4c: it moves nothing. No state, no transition, no UpdatedOn, so the wait is not restarted.</summary>
    [Fact]
    public async Task AReminder_LeavesTheRequestsStateTransitionsAndLastMoveUntouched()
    {
        await using var db = Seeded();
        var request = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var before = await db.Activities.AsNoTracking().Include(a => a.Transitions).SingleAsync(a => a.Id == 1);

        (await SendAsync(db, new RecordingEmailSender(), Coordinator(), WombatRoles.Coordinator, request)).Outcome
            .Should().Be(ReminderOutcome.Sent);

        var after = await db.Activities.AsNoTracking().Include(a => a.Transitions).SingleAsync(a => a.Id == 1);
        (after.CurrentState, after.UpdatedOn, after.Transitions.Count).Should().Be((before.CurrentState, before.UpdatedOn, before.Transitions.Count));
        db.ChangeTracker.Entries<Activity>().Should().BeEmpty("the activity is never read tracked");
    }

    /// <summary>E1: a reminder about one request is "email about one particular thing", which an opt-out does not stop.</summary>
    [Fact]
    public async Task AnAssessorWhoOptedOutOfDigestEmails_IsStillSentAReminder()
    {
        await using var db = Seeded();
        db.Users.Single(user => user.Id == Zulu).OptOutOfDigestEmails = true;
        var request = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        await db.SaveChangesAsync();
        var mail = new RecordingEmailSender();

        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, request)).Outcome.Should().Be(ReminderOutcome.Sent);
        mail.Recipients.Should().Equal(RecordingEmailSender.EmailOf(Zulu));
    }

    /// <summary>
    /// The settled same-day rule: once anyone has reminded the assessor today, on the South African calendar, everyone is
    /// told so; the next South African day it is sent again.
    /// </summary>
    [Fact]
    public async Task ASecondSenderTheSameSouthAfricanDay_IsRefused_AndTheNextDayItIsSent()
    {
        await using var db = Seeded();
        var request = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        await db.SaveChangesAsync();
        var mail = new RecordingEmailSender();
        var evening = new DateTime(2026, 10, 4, 21, 30, 0, DateTimeKind.Utc); // 23:30 SAST, still the 4th
        var midnight = new DateTime(2026, 10, 4, 22, 5, 0, DateTimeKind.Utc); // 00:05 SAST on the 5th

        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, request)).Outcome.Should().Be(ReminderOutcome.Sent);

        var second = await SendAsync(db, mail, SpecialityAdmin(), WombatRoles.SpecialityAdmin, request, now: evening);
        second.Outcome.Should().Be(ReminderOutcome.RemindedToday);
        second.Reminder!.SentByName.Should().Be("Pieter Smit");
        second.AssessorName.Should().Be("Thandi Zulu");

        (await SendAsync(db, mail, SpecialityAdmin(), WombatRoles.SpecialityAdmin, request, now: midnight)).Outcome
            .Should().Be(ReminderOutcome.Sent);

        mail.Sent.Should().HaveCount(2);
        (await db.ActivityReminders.AsNoTracking().Select(row => row.SentOnDay).ToListAsync())
            .Should().BeEquivalentTo([new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 5)]);
    }

    [Fact]
    public async Task EachRecipientRefusal_IsAnswered_WithNothingStagedAndNoMail()
    {
        await using var db = Seeded();
        db.Users.Single(user => user.Id == Khumalo).LockoutEnd = UserDeactivation.IndefiniteLockoutEnd;
        db.Users.Single(user => user.Id == Patel).Email = "  ";
        var deactivated = AddRequest(db, 1, Mahlangu, Khumalo, Now.AddDays(-4));
        var noEmail = AddRequest(db, 2, Mahlangu, Patel, Now.AddDays(-3));
        var noAccount = AddRequest(db, 3, Mahlangu, "erased-assessor", Now.AddDays(-2));
        await db.SaveChangesAsync();
        var mail = new RecordingEmailSender();

        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, deactivated)).Outcome.Should().Be(ReminderOutcome.Deactivated);
        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, noEmail)).Outcome.Should().Be(ReminderOutcome.NoEmail);
        var erased = await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, noAccount);
        erased.Outcome.Should().Be(ReminderOutcome.NoAccount);

        mail.Sent.Should().BeEmpty();
        ShouldHaveStagedNothing(db);
    }

    /// <summary>A lockout from failed passwords lifts itself: not a deactivation, so the reminder goes.</summary>
    [Fact]
    public async Task ABruteForceLockout_IsNoRefusal()
    {
        await using var db = Seeded();
        db.Users.Single(user => user.Id == Zulu).LockoutEnd = Now.AddMinutes(15);
        var request = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        await db.SaveChangesAsync();

        (await SendAsync(db, new RecordingEmailSender(), Coordinator(), WombatRoles.Coordinator, request)).Outcome
            .Should().Be(ReminderOutcome.Sent);
    }

    /// <summary>
    /// Moved meanwhile: another state, a save since (UpdatedOn), or no longer waiting for a named person. The answer says
    /// what it is now and when it moved, and stages nothing.
    /// </summary>
    [Fact]
    public async Task ARequestThatMovedSinceTheListWasRead_IsAnsweredMovedMeanwhile()
    {
        await using var db = Seeded();
        var completed = AddRequest(db, 1, Mahlangu, Zulu, Now.AddHours(-2), state: "completed");
        var saved = AddRequest(db, 2, Mahlangu, Zulu, Now.AddHours(-1));
        await db.SaveChangesAsync();
        var mail = new RecordingEmailSender();

        var gone = await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, completed, expectedState: "requested");
        gone.Outcome.Should().Be(ReminderOutcome.MovedMeanwhile);
        gone.CurrentStateLabel.Should().Be("Completed");
        gone.MovedOn.Should().Be(Now.AddHours(-2));
        gone.StillWaiting.Should().BeFalse();
        gone.AssessorName.Should().BeNull("it waits for nobody now");

        var resaved = await SendAsync(
            db, mail, Coordinator(), WombatRoles.Coordinator, saved, expectedUpdatedOn: Now.AddDays(-3));
        resaved.Outcome.Should().Be(ReminderOutcome.MovedMeanwhile);
        resaved.StillWaiting.Should().BeTrue("it still waits for Thandi Zulu, saved since");
        resaved.MovedOn.Should().Be(Now.AddHours(-1));

        mail.Sent.Should().BeEmpty();
        ShouldHaveStagedNothing(db);
    }

    /// <summary>
    /// D7: an id the reader cannot reach (no such id, another hospital's, outside the role's scope, the reader's own) is
    /// not found, never the moved-meanwhile words; and a role that may not remind is answered the same way.
    /// </summary>
    [Fact]
    public async Task ARequestOutOfReach_OrARoleThatMayNotRemind_IsNotFound()
    {
        await using var db = Seeded();
        var otherHospital = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8), institutionId: OtherHospital);
        var surgery = AddRequest(db, 2, DuPlessis, Patel, Now.AddDays(-8), specialityId: Surgery, subSpecialityId: SurgerySub);
        var own = AddRequest(db, 3, Smit, Zulu, Now.AddDays(-8));
        var fine = AddRequest(db, 4, Mahlangu, Zulu, Now.AddDays(-8));
        await db.SaveChangesAsync();
        var mail = new RecordingEmailSender();

        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, otherHospital)).Should().Be(SendActivityReminderResult.NotFound);
        (await SendAsync(db, mail, SpecialityAdmin(), WombatRoles.SpecialityAdmin, surgery)).Outcome.Should().Be(ReminderOutcome.NotFound);
        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, own)).Outcome.Should().Be(ReminderOutcome.NotFound);
        (await SendAsync(db, mail, CommitteeMember(), WombatRoles.CommitteeMember, fine)).Outcome.Should().Be(ReminderOutcome.NotFound);
        (await SendAsync(db, mail, Coordinator(), WombatRoles.SpecialityAdmin, fine)).Outcome.Should().Be(ReminderOutcome.NotFound, "not held");
        (await SendAsync(db, mail, Coordinator(), WombatRoles.Coordinator, new Activity { Id = 999, CurrentState = "requested" }))
            .Outcome.Should().Be(ReminderOutcome.NotFound);

        mail.Sent.Should().BeEmpty();
        ShouldHaveStagedNothing(db);
    }

    [Fact]
    public async Task ARoleHeldReview_IsNotARequestToRemindAbout()
    {
        await using var db = Seeded();
        var teaching = AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8), typeId: AssessorReads.TeachingTypeId, state: "submitted");
        await db.SaveChangesAsync();

        (await SendAsync(db, new RecordingEmailSender(), Coordinator(), WombatRoles.Coordinator, teaching)).Outcome
            .Should().Be(ReminderOutcome.MovedMeanwhile, "it waits for a role, not a named assessor (E3), so it is not on the list");
    }

    /// <summary>The audit pipeline commits whatever a handler left staged: a refusal must leave nothing.</summary>
    private static void ShouldHaveStagedNothing(ApplicationDbContext db)
    {
        db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged && entry.State != EntityState.Detached)
            .Should().BeEmpty();
        db.ActivityReminders.AsNoTracking().Should().BeEmpty();
    }
}
