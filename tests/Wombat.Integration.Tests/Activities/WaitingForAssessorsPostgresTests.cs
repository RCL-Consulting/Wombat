using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T358 (flow 06, lane A1) on a real PostgreSQL server: Waiting for assessors' read, and Send a reminder's lost race.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which evaluates each query in memory and enforces no unique index. Here the read
/// must translate: the waiting narrowing (<c>= ANY</c>), the programme scope's stamps beneath the read rule, the reminders
/// by activity, and the recipients' projection over the accounts table. Beside it, the same-day block as Postgres
/// enforces it: two staff who both pass the handler's check meet the unique index, and the second is answered "already
/// reminded today", with nothing left staged for the audit write (D6).
/// </para>
/// <para>
/// Isolated as <c>DashboardWaitingPostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), dropped in
/// a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class WaitingForAssessorsPostgresTests : IAsyncLifetime
{
    private const string Registrar = "t358-mahlangu";
    private const string Zulu = "t358-zulu";
    private const string Patel = "t358-patel";
    private const string Khumalo = "t358-khumalo";
    private const string Smit = "t358-smit";
    private const string Mokoena = "t358-mokoena";

    /// <summary>D, 10:00 in South Africa.</summary>
    private static readonly DateTime Now = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheRead_OnPostgres_ListsWhatWaitsForANamedAssessor_WithItsRemindersAndWhetherItsNomineeCanBeReminded()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            var programme = await ArrangeAsync(schema);

            int miniCex, review, toKhumalo;
            await using (var db = NewContext(schema))
            {
                var miniCexType = AddType(db, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
                var portfolioType = AddType(db, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
                var teachingType = AddType(db, "teaching_session", "Teaching session");

                var requested = Add(db, programme, miniCexType, "requested", Zulu, Now.AddDays(-8));
                var submitted = Add(db, programme, portfolioType, "submitted", Patel, Now.AddDays(-8).AddMinutes(-30));
                var recent = Add(db, programme, miniCexType, "requested", Khumalo, Now.AddHours(-2));
                Add(db, programme, teachingType, "submitted", null, Now.AddDays(-3));
                Add(db, programme, miniCexType, "draft", Zulu, Now.AddDays(-1));
                await db.SaveChangesAsync();
                (miniCex, review, toKhumalo) = (requested.Id, submitted.Id, recent.Id);

                db.ActivityReminders.AddRange(
                    ActivityReminder.Record(miniCex, Mokoena, Zulu, new DateTime(2026, 10, 2, 7, 0, 0, DateTimeKind.Utc)),
                    ActivityReminder.Record(miniCex, Smit, Zulu, new DateTime(2026, 10, 3, 22, 30, 0, DateTimeKind.Utc)),
                    ActivityReminder.Record(review, Smit, Patel, new DateTime(2026, 10, 3, 21, 30, 0, DateTimeKind.Utc)));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var result = (await Handler(db).Handle(
                    new ListWaitingForAssessorsQuery(Principal(Smit, WombatRoles.Coordinator, programme.InstitutionId), WombatRoles.Coordinator),
                    CancellationToken.None))!;

                result.Items.Select(item => (item.Id, item.CurrentStateLabel, item.IsOverdue, item.WaitedDays, item.Holder!.Name)).Should().Equal(
                    [
                        (review, "Awaiting review", true, 8, "Mohammed Patel"),
                        (miniCex, "Requested", true, 8, "Thandi Zulu"),
                        (toKhumalo, "Requested", false, 0, "Fatima Khumalo")
                    ],
                    "what waits for a named assessor, oldest first; not the role-held teaching session, not the draft");
                result.Items.Select(item => item.SubjectName).Should().AllBe("Nomsa Mahlangu");
                var rows = result.Items.ToDictionary(item => item.Id);
                rows[miniCex].LastReminder.Should().Be(new ActivityReminderDto(
                    new DateTime(2026, 10, 3, 22, 30, 0, DateTimeKind.Utc), new DateOnly(2026, 10, 4), "Pieter Smit"));
                rows[miniCex].RemindedToday.Should().BeTrue();
                rows[review].RemindedToday.Should().BeFalse("23:30 on the 3rd in South Africa");
                rows[toKhumalo].CannotRemind.Should().Be(ReminderOutcome.Deactivated);
                rows[miniCex].CannotRemind.Should().BeNull();
                result.Nominees.Select(nominee => nominee.Name).Should().Equal("Fatima Khumalo", "Mohammed Patel", "Thandi Zulu");
                (result.MatchCount, result.MatchOverdueCount).Should().Be((3, 2));

                var asAdmin = (await Handler(db).Handle(
                    new ListWaitingForAssessorsQuery(
                        Principal(Mokoena, WombatRoles.SpecialityAdmin, programme.InstitutionId, specialityId: programme.SpecialityId),
                        WombatRoles.SpecialityAdmin, OverdueOnly: true, WithUserId: Zulu),
                    CancellationToken.None))!;
                asAdmin.Items.Select(item => item.Id).Should().Equal(miniCex);
                (asAdmin.MatchCount, asAdmin.TotalCount, asAdmin.TotalOverdueCount).Should().Be((1, 3, 2));
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// D6: two staff send the same reminder at once. Both pass the check; the other's row lands first, so this one meets
    /// the unique index, is told who reminded the assessor today, and leaves nothing staged: the audit write that follows a
    /// command saves the same context, and must not send the refused row again.
    /// </summary>
    [Fact]
    public async Task TheLostRace_OnPostgres_IsAnsweredRemindedToday_AndLeavesNothingStaged()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            var programme = await ArrangeAsync(schema);

            int activityId;
            DateTime updatedOn;
            await using (var db = NewContext(schema))
            {
                var activity = Add(db, programme, AddType(db, "mini_cex_cpsa", "Mini-CEX (Paediatrics)"), "requested", Zulu, Now.AddDays(-8));
                await db.SaveChangesAsync();
                (activityId, updatedOn) = (activity.Id, activity.UpdatedOn);
            }

            await using (var db = NewContext(schema))
            {
                var mail = new CountingEmailSender();
                // The other sender's row lands between this handler's check and its save: the recipient port is the last
                // read before the row is staged, so it lets the other sender in there.
                var racing = new LandsAnotherReminderFirst(new ReminderRecipients(db), schema, activityId);
                var handler = new SendActivityReminderCommandHandler(db, Names(), racing, mail, new FixedClock(Now));

                var result = await handler.Handle(
                    new SendActivityReminderCommand(
                        Principal(Smit, WombatRoles.Coordinator, programme.InstitutionId), WombatRoles.Coordinator,
                        activityId, "requested", updatedOn),
                    CancellationToken.None);

                result.Outcome.Should().Be(ReminderOutcome.RemindedToday);
                result.Reminder!.SentByName.Should().Be("Refilwe Mokoena", "the reminder that landed is the other sender's");
                mail.Count.Should().Be(0, "the mail is handed over only after the save");
                db.ChangeTracker.Entries<ActivityReminder>().Should().BeEmpty("the refused row is detached (D6)");
                await db.SaveChangesAsync(); // the audit write's save: nothing to send again
            }

            await using (var db = NewContext(schema))
            {
                (await db.ActivityReminders.AsNoTracking().Select(row => row.SentByUserId).ToListAsync())
                    .Should().Equal(Mokoena);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the cast ----

    private sealed record Programme(int InstitutionId, int SpecialityId, int SubSpecialityId);

    private static async Task<Programme> ArrangeAsync(string schema)
    {
        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using var context = NewContext(schema);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var subSpeciality = new SubSpeciality
        {
            Name = "Paediatrics",
            Speciality = new Speciality
            {
                Name = "Paediatrics",
                College = new College { Name = $"T358 College {suffix}", ShortCode = $"T358C-{suffix}" }
            }
        };
        var institution = new Institution { Name = "Kgosi Kgari Teaching Hospital", ShortCode = $"KGK-{suffix}" };
        context.SubSpecialities.Add(subSpeciality);
        context.Institutions.Add(institution);
        await context.SaveChangesAsync();

        foreach (var (id, first, last) in new[]
                 {
                     (Registrar, "Nomsa", "Mahlangu"), (Zulu, "Thandi", "Zulu"), (Patel, "Mohammed", "Patel"),
                     (Khumalo, "Fatima", "Khumalo"), (Smit, "Pieter", "Smit"), (Mokoena, "Refilwe", "Mokoena")
                 })
        {
            var user = NomineeSeed.AddUser(
                context, id, institution.Id, id == Khumalo ? UserDeactivation.IndefiniteLockoutEnd : null);
            user.FirstName = first;
            user.LastName = last;
        }

        await context.SaveChangesAsync();
        return new Programme(institution.Id, subSpeciality.SpecialityId, subSpeciality.Id);
    }

    private static FakeUserDirectory Names() => new(
        (Registrar, "Nomsa Mahlangu"), (Zulu, "Thandi Zulu"), (Patel, "Mohammed Patel"), (Khumalo, "Fatima Khumalo"),
        (Smit, "Pieter Smit"), (Mokoena, "Refilwe Mokoena"));

    private static ListWaitingForAssessorsQueryHandler Handler(ApplicationDbContext db)
        => new(db, Names(), new ReminderRecipients(db), Options.Create(new DashboardThresholds()), new FixedClock(Now));

    private static int AddType(ApplicationDbContext db, string key, string name)
    {
        var workflowJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, "workflow.json"));
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var type = new ActivityType
        {
            Key = $"t358_{key}",
            Name = name,
            Scope = ActivityScope.Global,
            Version = 1,
            IsActive = true,
            SchemaJson = "{}",
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = publishedOn
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            PublishedByUserId = "system",
            PublishedOn = publishedOn
        });
        db.ActivityTypes.Add(type);
        db.SaveChanges();
        return type.Id;
    }

    private static Activity Add(ApplicationDbContext db, Programme programme, int typeId, string state, string? assessor, DateTime updatedOn)
    {
        var activity = new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = Registrar,
            CreatedByUserId = Registrar,
            CurrentState = state,
            DataJson = assessor is null ? "{}" : $$"""{ "assessor_user_id": "{{assessor}}" }""",
            InstitutionId = programme.InstitutionId,
            SpecialityId = programme.SpecialityId,
            SubSpecialityId = programme.SubSpecialityId,
            CreatedOn = updatedOn.AddDays(-1),
            UpdatedOn = updatedOn,
            ObservedOn = DateOnly.FromDateTime(updatedOn)
        };
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "draft", ToState = state, TransitionKey = "submit", ActorUserId = Registrar, OccurredOn = updatedOn,
            SnapshotJson = "{}"
        });
        db.Activities.Add(activity);
        return activity;
    }

    private static ClaimsPrincipal Principal(string userId, string role, int institutionId, int? specialityId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
        };
        if (specialityId is int speciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, speciality.ToString(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);

    /// <summary>The recipient port, which first lets another sender's reminder land, through a context of its own.</summary>
    private sealed class LandsAnotherReminderFirst(IReminderRecipients inner, string schema, int activityId) : IReminderRecipients
    {
        public async Task<IReadOnlyDictionary<string, ReminderRecipientDto>> LoadAsync(
            IEnumerable<string> userIds, CancellationToken cancellationToken)
        {
            await using (var other = NewContext(schema))
            {
                other.ActivityReminders.Add(ActivityReminder.Record(activityId, Mokoena, Zulu, Now.AddMinutes(-1)));
                await other.SaveChangesAsync(cancellationToken);
            }

            return await inner.LoadAsync(userIds, cancellationToken);
        }
    }

    private sealed class CountingEmailSender : IEmailSender
    {
        public int Count { get; private set; }

        public Task SendAsync(Application.Common.Email.EmailMessage message, CancellationToken cancellationToken = default)
        {
            Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now);
    }
}
