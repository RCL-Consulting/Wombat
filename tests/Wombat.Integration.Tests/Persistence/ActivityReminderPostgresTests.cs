using System.Globalization;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Persistence;

/// <summary>
/// T358 (flow 06, lane A0) on a real PostgreSQL server: the build's one migration, <c>T358_ActivityRemindersAndAdmission</c>.
/// It sets every stored profile's admission day to its programme start (D1), and its unique index refuses a second
/// reminder about one request on one South African day, whoever sends it, while the next day's is accepted (round 3's
/// settled same-day rule, D6).
/// </summary>
/// <remarks>
/// The unit suites run on EF InMemory, which runs no migration and enforces no unique index. The backfill is rehearsed on
/// a schema stopped at the migration before it, whose profiles table has no <c>AdmittedOn</c>, so its profile is written by
/// raw SQL; the rows it points at are written through the domain, their tables unchanged by T358. The schema helpers follow
/// <c>AppealOutcomeMigrationPostgresTests</c>.
/// </remarks>
public sealed class ActivityReminderPostgresTests : IAsyncLifetime
{
    private const string LastMigrationBeforeT358 = "20260927084116_T335_AccountActingRole";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_AddsTheReminderTable_AndSetsEveryStoredProfilesAdmissionDay_ToItsProgrammeStart()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT358);
                (await db.Database.GetPendingMigrationsAsync())
                    .Should().ContainSingle(migration => migration.EndsWith("_T358_ActivityRemindersAndAdmission", StringComparison.Ordinal));
            }

            (await Catalog.ColumnExistsAsync(schema, "TraineeProfiles", "AdmittedOn")).Should().BeFalse("guard: before T358");
            (await Catalog.TableExistsAsync(schema, "ActivityReminders")).Should().BeFalse("guard: before T358");

            int institutionId, curriculumId;
            await using (var db = NewContext(schema))
            {
                (institutionId, curriculumId) = await AddProgrammeAsync(db);
            }

            await ExecuteAsync(
                schema,
                """
                INSERT INTO "TraineeProfiles" ("UserId", "InstitutionId", "CurriculumId", "ProgrammeStartDate", "ExpectedCompletionDate", "IsActive")
                VALUES ('registrar-1', $1, $2, DATE '2026-01-15', DATE '2030-01-15', TRUE),
                       ('registrar-2', $1, $2, DATE '2024-07-01', DATE '2028-07-01', FALSE)
                """,
                institutionId,
                curriculumId);

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            }

            (await Catalog.ColumnTypeAsync(schema, "TraineeProfiles", "AdmittedOn")).Should().Be("date");
            (await Catalog.TableExistsAsync(schema, "ActivityReminders")).Should().BeTrue();
            (await Catalog.IndexNamesAsync(schema, "ActivityReminders")).Should().Contain(
                ["IX_ActivityReminders_ActivityId_SentOn", "IX_ActivityReminders_ActivityId_SentOnDay"]);

            await using (var db = NewContext(schema))
            {
                var admitted = await db.TraineeProfiles.AsNoTracking()
                    .OrderBy(profile => profile.UserId)
                    .Select(profile => new { profile.UserId, profile.ProgrammeStartDate, profile.AdmittedOn })
                    .ToListAsync();
                admitted.Should().HaveCount(2);
                admitted.Should().OnlyContain(profile => profile.AdmittedOn == profile.ProgrammeStartDate,
                    "no stored profile recorded its admission; its programme start is the nearest day it holds");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TwoRemindersAboutOneRequest_OnOneSouthAfricanDay_AreRefusedByTheIndex_OnTwoDaysAccepted()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            int activityId, otherActivityId;
            await using (var db = NewContext(schema))
            {
                var (institutionId, _) = await AddProgrammeAsync(db);
                var typeId = AddType(db);
                var activity = AddActivity(db, typeId, institutionId);
                var other = AddActivity(db, typeId, institutionId);
                await db.SaveChangesAsync();
                (activityId, otherActivityId) = (activity.Id, other.Id);
            }

            // 06:00 UTC on 4 October is 08:00 in South Africa; 21:59 UTC is 23:59 there, still the 4th.
            await using (var db = NewContext(schema))
            {
                db.ActivityReminders.Add(ActivityReminder.Record(
                    activityId, "smit", "zulu", new DateTime(2026, 10, 4, 6, 0, 0, DateTimeKind.Utc)));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                db.ActivityReminders.Add(ActivityReminder.Record(
                    activityId, "mokoena", "zulu", new DateTime(2026, 10, 4, 21, 59, 0, DateTimeKind.Utc)));

                var refused = await FluentActions.Awaiting(() => db.SaveChangesAsync())
                    .Should().ThrowAsync<DbUpdateException>("a second sender, the same South African day");
                var postgres = refused.Which.InnerException.Should().BeOfType<PostgresException>().Subject;
                postgres.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
                postgres.ConstraintName.Should().Be("IX_ActivityReminders_ActivityId_SentOnDay");
            }

            // 22:01 UTC on 4 October is 00:01 on the 5th in South Africa: the next day, accepted. Another request the
            // same day is its own.
            await using (var db = NewContext(schema))
            {
                db.ActivityReminders.AddRange(
                    ActivityReminder.Record(activityId, "mokoena", "zulu", new DateTime(2026, 10, 4, 22, 1, 0, DateTimeKind.Utc)),
                    ActivityReminder.Record(otherActivityId, "smit", "patel", new DateTime(2026, 10, 4, 6, 30, 0, DateTimeKind.Utc)));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var days = await db.ActivityReminders.AsNoTracking()
                    .OrderBy(reminder => reminder.ActivityId).ThenBy(reminder => reminder.SentOn)
                    .Select(reminder => new { reminder.ActivityId, reminder.SentOnDay, reminder.SentByUserId })
                    .ToListAsync();
                days.Should().Equal(
                    new { ActivityId = activityId, SentOnDay = new DateOnly(2026, 10, 4), SentByUserId = "smit" },
                    new { ActivityId = activityId, SentOnDay = new DateOnly(2026, 10, 5), SentByUserId = "mokoena" },
                    new { ActivityId = otherActivityId, SentOnDay = new DateOnly(2026, 10, 4), SentByUserId = "smit" });

                // The reminders go with their activity (the key cascades).
                await db.Activities.Where(activity => activity.Id == activityId).ExecuteDeleteAsync();
                (await db.ActivityReminders.CountAsync(reminder => reminder.ActivityId == activityId)).Should().Be(0);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T358, review 1: a reminder names two people, who sent it and the assessor it was sent to, and an erasure moves
    /// both to the pseudonym, as it moves <see cref="ActivityTransition.ActorUserId" />. Someone else's id on the same row
    /// is left as it was.
    /// </summary>
    [Fact]
    public async Task AnErasure_PseudonymisesTheSenderAndTheAssessor_OfEveryReminder()
    {
        var schema = await _schemas.CreateAsync();
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
            services.AddIdentity<WombatIdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddScoped<ErasureExecutor>();
            await using var root = services.BuildServiceProvider();

            string erasedId;
            Guid requestId;
            int activityId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();

                var (institutionId, _) = await AddProgrammeAsync(db);
                var person = new WombatIdentityUser
                {
                    UserName = "zulu@kgk.test", Email = "zulu@kgk.test", FirstName = "Thandi", LastName = "Zulu", InstitutionId = institutionId
                };
                (await arrange.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>()
                    .CreateAsync(person, "Correct-Horse-Battery-9!")).Succeeded.Should().BeTrue();
                erasedId = person.Id;

                var activity = AddActivity(db, AddType(db), institutionId);
                await db.SaveChangesAsync();
                activityId = activity.Id;

                db.ActivityReminders.AddRange(
                    ActivityReminder.Record(activityId, "smit", erasedId, new DateTime(2026, 10, 3, 6, 0, 0, DateTimeKind.Utc)),
                    ActivityReminder.Record(activityId, erasedId, "patel", new DateTime(2026, 10, 4, 6, 0, 0, DateTimeKind.Utc)));
                var request = DataRightsRequest.Create(
                    erasedId, "Thandi Zulu", DataRightsRequestType.Erasure, "Leaving the programme.", DateTime.UtcNow);
                db.DataRightsRequests.Add(request);
                await db.SaveChangesAsync();
                requestId = request.Id;
            }

            await using (var act = root.CreateAsyncScope())
            {
                var db = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var request = await db.DataRightsRequests.SingleAsync(entity => entity.Id == requestId);
                await act.ServiceProvider.GetRequiredService<ErasureExecutor>().ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
            }

            await using var read = root.CreateAsyncScope();
            var reminders = await read.ServiceProvider.GetRequiredService<ApplicationDbContext>().ActivityReminders.AsNoTracking()
                .OrderBy(reminder => reminder.SentOn)
                .ToListAsync();
            reminders.Should().HaveCount(2);
            reminders[0].SentByUserId.Should().Be("smit");
            reminders[0].AssessorUserId.Should().StartWith("deleted_user_");
            reminders[1].SentByUserId.Should().Be(reminders[0].AssessorUserId, "one person, one pseudonym");
            reminders[1].AssessorUserId.Should().Be("patel");
            reminders.Should().NotContain(reminder => reminder.SentByUserId == erasedId || reminder.AssessorUserId == erasedId);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static async Task<(int InstitutionId, int CurriculumId)> AddProgrammeAsync(ApplicationDbContext db)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var curriculum = new Curriculum
        {
            Name = "T358 programme",
            Version = "1",
            EffectiveFrom = new DateOnly(2025, 1, 1),
            SubSpeciality = new SubSpeciality
            {
                Name = "General Paediatrics",
                Speciality = new Speciality
                {
                    Name = "Paediatrics",
                    College = new College { Name = $"T358 College {suffix}", ShortCode = $"T358C-{suffix}" }
                }
            }
        };
        var institution = new Institution { Name = $"T358 Hospital {suffix}", ShortCode = $"T358H-{suffix}" };
        db.Curricula.Add(curriculum);
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();
        return (institution.Id, curriculum.Id);
    }

    private static int AddType(ApplicationDbContext db)
    {
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var type = new ActivityType
        {
            Key = $"t358_{Guid.NewGuid():N}",
            Name = "Mini-CEX (Paediatrics)",
            Scope = ActivityScope.Global,
            Version = 1,
            IsActive = true,
            SchemaJson = "{}",
            WorkflowJson = "{}",
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = publishedOn
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = "{}",
            CreditRulesJson = """{ "counts_for": [] }""",
            PublishedByUserId = "system",
            PublishedOn = publishedOn
        });
        db.ActivityTypes.Add(type);
        db.SaveChanges();
        return type.Id;
    }

    private static Activity AddActivity(ApplicationDbContext db, int typeId, int institutionId)
    {
        var at = new DateTime(2026, 9, 26, 8, 0, 0, DateTimeKind.Utc);
        var activity = new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = "registrar-1",
            CreatedByUserId = "registrar-1",
            CurrentState = "requested",
            DataJson = "{}",
            InstitutionId = institutionId,
            CreatedOn = at,
            UpdatedOn = at,
            ObservedOn = new DateOnly(2026, 9, 26)
        };
        db.Activities.Add(activity);
        return activity;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return Convert.ToInt32(await command.ExecuteNonQueryAsync(), CultureInfo.InvariantCulture);
    }
}
