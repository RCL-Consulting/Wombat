using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Scheduling;

/// <summary>
/// T240 on a real PostgreSQL server: the draft reminder, run end to end, reminds a trainee of their stale drafts and
/// writes to no deactivated account and to no one who opted out of digest emails.
/// </summary>
/// <remarks>
/// <para>
/// <c>ActivityDraftNudgeJobTests</c> holds the rules on EF InMemory. Here the job runs on Npgsql, so the stale-draft query
/// and the reminder policy's one account query must translate, and an administrator's lock, written as
/// <see cref="DateTimeOffset.MaxValue" />, must still read back as a deactivation. The assessor nudge loads its
/// recipients through the same query. A browser run needs an Administrator to start the job, so this is the end-to-end
/// evidence an agent can produce.
/// </para>
/// <para>
/// Isolated the way <c>WeeklyCoordinatorDigestPostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>),
/// registered before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class ActivityDraftNudgePostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheDraftReminder_OnPostgres_SkipsDeactivatedAndOptedOutTrainees_AndSaysSo()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            var emailSender = new RecordingEmailSender();
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IEmailSender>(_ => emailSender);

            await using var root = services.BuildServiceProvider();

            await using (var migrate = root.CreateAsyncScope())
            {
                var db = migrate.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
                await new DataSeeder(db).SeedAsync();
            }

            var now = DateTime.UtcNow;
            string typeName;

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var type = await db.ActivityTypes.OrderBy(entity => entity.Id).Select(entity => new { entity.Id, entity.Version, entity.Name }).FirstAsync();
                typeName = type.Name;

                NomineeSeed.AddUser(db, "t240-active", host, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, "t240-locked-out", host, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Trainee);
                NomineeSeed.AddUser(db, "t240-deactivated", host, DateTimeOffset.MaxValue, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, "t240-opted-out", host, WombatRoles.Trainee).OptOutOfDigestEmails = true;

                foreach (var subject in new[] { "t240-active", "t240-locked-out", "t240-deactivated", "t240-opted-out" })
                {
                    db.Activities.Add(Draft(type.Id, type.Version, host, subject, now.AddDays(-20)));
                }

                // Touched within 14 days: not stale, so not in the reminder and not counted anywhere.
                db.Activities.Add(Draft(type.Id, type.Version, host, "t240-active", now.AddDays(-2)));

                await db.SaveChangesAsync();
            }

            var logger = new CapturingLogger();
            await new ActivityDraftNudgeJob(root.GetRequiredService<IServiceScopeFactory>())
                .ExecuteAsync(new ScheduledJobContext(now, logger), CancellationToken.None);

            emailSender.Sent.Select(message => message.To).Should().BeEquivalentTo(
                ["t240-active@test.local", "t240-locked-out@test.local"]);
            emailSender.Sent.Should().AllSatisfy(message => message.TextBody.Should().Contain(typeName).And.Contain("20 days"));
            logger.Messages.Should().ContainSingle().Which.Should().Be(
                "ActivityDraftNudgeJob: trainees reminded 2 (drafts 2); trainees skipped: no such account 0, deactivated 1, " +
                "opted out of digest emails 1, no email address 0.");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Activity Draft(int typeId, int version, int institutionId, string subjectUserId, DateTime updatedOn)
        => new()
        {
            ActivityTypeId = typeId,
            SchemaVersion = version,
            InstitutionId = institutionId,
            SubjectUserId = subjectUserId,
            CreatedByUserId = subjectUserId,
            CurrentState = "draft",
            DataJson = "{}",
            CreatedOn = updatedOn,
            UpdatedOn = updatedOn,
            ObservedOn = DateOnly.FromDateTime(updatedOn)
        };

    private sealed class RecordingEmailSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

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
}
