using System.Data.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Scheduling;

/// <summary>
/// T117 on a real PostgreSQL server: the weekly coordinator digest, run end to end, mails each coordinator only their own
/// institution's trainees, campaigns and reviews; and (T240) no coordinator whose account is deactivated or who opted out
/// of digest emails.
/// </summary>
/// <remarks>
/// <para>
/// <c>WeeklyCoordinatorDigestJobTests</c> holds the rules on EF InMemory, which evaluates every query in memory. Here the
/// same job runs on Npgsql, with Identity and the app's claims factory wired as the hosts wire them, so every per-recipient
/// query must translate: the trainee resolver's preferred-profile ladder, the campaign rule joined to the roster, and the
/// scheduled reviews with their panels and members. A browser run needs an Administrator to start the job, so this is the
/// end-to-end evidence an agent can produce.
/// </para>
/// <para>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class WeeklyCoordinatorDigestPostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheDigest_OnPostgres_MailsEachCoordinatorOnlyTheirInstitutionsRows_AndReadsNoActivityContent()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            var emailSender = new RecordingEmailSender();
            var commands = new CommandLog();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
                .AddInterceptors(commands));
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IEmailSender>(_ => emailSender);
            services.AddIdentityCore<WombatIdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>();
            services.AddScoped<IUserAdministrationService, UserAdministrationService>();

            await using var root = services.BuildServiceProvider();

            await using (var migrate = root.CreateAsyncScope())
            {
                var db = migrate.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
                await new DataSeeder(db).SeedAsync();
            }

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            int campaignA, campaignB;

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var elsewhere = new Institution { Name = "Elsewhere General", ShortCode = "T117-ELSE" };
                db.Institutions.Add(elsewhere);
                await db.SaveChangesAsync();

                var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
                var type = await db.ActivityTypes.OrderBy(entity => entity.Id).Select(entity => new { entity.Id, entity.Version }).FirstAsync();

                NomineeSeed.AddUser(db, "coord-a", host, WombatRoles.Coordinator);
                NomineeSeed.AddUser(db, "coord-b", elsewhere.Id, WombatRoles.Coordinator);
                NomineeSeed.AddUser(db, "admin-coord", host, WombatRoles.Administrator, WombatRoles.Coordinator);
                var unaddressed = NomineeSeed.AddUser(db, "coord-unaddressed", host, WombatRoles.Coordinator);
                unaddressed.Email = null;
                unaddressed.NormalizedEmail = null;

                // T240: the shared reminder policy, on the server. An administrator's lock is written as
                // DateTimeOffset.MaxValue and read back through Npgsql; a brute-force lockout is not a deactivation.
                NomineeSeed.AddUser(db, "coord-locked", host, DateTimeOffset.MaxValue, WombatRoles.Coordinator);
                NomineeSeed.AddUser(db, "coord-locked-out", host, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Coordinator);
                NomineeSeed.AddUser(db, "coord-opted-out", host, WombatRoles.Coordinator).OptOutOfDigestEmails = true;

                AddTrainee(db, "a-idle", "Aisha", "Idle", host, curriculumId);
                AddTrainee(db, "a-busy", "Andile", "Busy", host, curriculumId);
                AddTrainee(db, "b-idle", "Bongani", "Idle", elsewhere.Id, curriculumId);

                // T284 on the server: idle, on A's roster, and not current, so neither is listed as inactive. The lock is
                // what an administrator's writes, read back through Npgsql; the other withdrew and kept the role.
                AddTrainee(db, "a-locked", "Lerato", "Locked", host, curriculumId).LockoutEnd = DateTimeOffset.MaxValue;
                AddTrainee(db, "a-withdrew", "Wandile", "Withdrew", host, curriculumId, isActive: false);

                var filed = now.AddDays(-3);
                db.Activities.Add(new Activity
                {
                    ActivityTypeId = type.Id,
                    SchemaVersion = type.Version,
                    InstitutionId = host,
                    SubjectUserId = "a-busy",
                    CreatedByUserId = "a-busy",
                    CurrentState = "draft",
                    DataJson = "{}",
                    CreatedOn = filed,
                    UpdatedOn = filed,
                    ObservedOn = DateOnly.FromDateTime(filed)
                });

                var template = new MsfTemplate { Name = "T117 MSF" };
                db.MsfTemplates.Add(template);
                var underReviewA = Campaign(template, "a-busy", today);
                var underReviewB = Campaign(template, "b-idle", today);
                db.MsfCampaigns.AddRange(underReviewA, underReviewB);

                db.CommitteeReviews.AddRange(
                    Review(Panel("T117 A panel", host, now), "a-idle", today.AddDays(2)),
                    Review(Panel("T117 B panel", elsewhere.Id, now), "b-idle", today.AddDays(3)));

                await db.SaveChangesAsync();
                campaignA = underReviewA.Id;
                campaignB = underReviewB.Id;
            }

            var logger = new CapturingLogger();
            commands.Texts.Clear();
            await new WeeklyCoordinatorDigestJob(root.GetRequiredService<IServiceScopeFactory>())
                .ExecuteAsync(new ScheduledJobContext(now, logger), CancellationToken.None);

            emailSender.Sent.Select(message => message.To).Should().BeEquivalentTo(
                ["coord-a@test.local", "coord-b@test.local", "admin-coord@test.local", "coord-locked-out@test.local"]);
            logger.Messages.Should().ContainSingle().Which.Should().Be(
                "WeeklyCoordinatorDigestJob: digests sent 4; coordinators skipped: deactivated 1, opted out of digest emails 1, " +
                "no email address 1, holds Trainee 0, no institution 0.");

            var expectedA = new[]
            {
                "Aisha Idle",
                $"T117 MSF (campaign #{campaignA})",
                $"Aisha Idle on {today.AddDays(2):yyyy-MM-dd}"
            };
            var expectedB = new[]
            {
                "Bongani Idle",
                $"T117 MSF (campaign #{campaignB})",
                $"Bongani Idle on {today.AddDays(3):yyyy-MM-dd}"
            };

            // The Administrator who also coordinates is held to their institution, on the server as in memory.
            foreach (var hostRecipient in new[] { "coord-a", "admin-coord", "coord-locked-out" })
            {
                ListedItems(emailSender.To(hostRecipient)).Should().Equal(expectedA);
            }

            ListedItems(emailSender.To("coord-b")).Should().Equal(expectedB);

            // The activity table is read for who filed, and for nothing a mail could carry (the class remarks).
            var activityReads = commands.Texts.Where(text => text.Contains("FROM \"Activities\"", StringComparison.Ordinal)).ToList();
            activityReads.Should().ContainSingle();
            activityReads[0].Should().NotContain("\"DataJson\"").And.Contain("DISTINCT");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static WombatIdentityUser AddTrainee(
        ApplicationDbContext db, string userId, string firstName, string lastName, int institutionId, int curriculumId, bool isActive = true)
    {
        var trainee = NomineeSeed.AddUser(db, userId, institutionId, WombatRoles.Trainee);
        trainee.FirstName = firstName;
        trainee.LastName = lastName;
        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = isActive
        });
        return trainee;
    }

    private static MsfCampaign Campaign(MsfTemplate template, string subjectUserId, DateOnly today)
        => new()
        {
            Template = template,
            SubjectUserId = subjectUserId,
            CreatedByUserId = "someone",
            CreatedOn = DateTime.UtcNow.AddDays(-40),
            OpensOn = today.AddDays(-40),
            ClosesOn = today.AddDays(-5),
            State = MsfCampaignState.UnderReview,
            OpenedOn = DateTime.UtcNow.AddDays(-40),
            ClosedOn = DateTime.UtcNow.AddDays(-5)
        };

    private static DecisionPanel Panel(string name, int institutionId, DateTime now)
        => new()
        {
            Name = name,
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institutionId,
            CreatedOn = now,
            Members = [new DecisionPanelMember { UserId = $"chair-{institutionId}", Role = DecisionPanelMemberRole.Chair }]
        };

    private static CommitteeReview Review(DecisionPanel panel, string traineeUserId, DateOnly scheduledOn)
        => new()
        {
            AcademicYear = 2026,
            Semester = 2,
            Panel = panel,
            TraineeUserId = traineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 7, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = scheduledOn
        };

    /// <summary>Every "  - " item of a digest's text body, in order, whatever heading it is under.</summary>
    private static IReadOnlyList<string> ListedItems(EmailMessage message)
        => message.TextBody.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Where(line => line.StartsWith("  - ", StringComparison.Ordinal))
            .Select(line => line[4..])
            .ToList();

    private sealed class RecordingEmailSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public EmailMessage To(string userId) => Sent.Should().ContainSingle(message => message.To == $"{userId}@test.local").Which;

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>Keeps each rendered log message.</summary>
    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }

    /// <summary>Every query EF sends, so what the job reads is asserted on the wire.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Texts.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
