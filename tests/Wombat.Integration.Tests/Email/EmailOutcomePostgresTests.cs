using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Email;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Email;

/// <summary>
/// T400 (flow 18, Q18, K3; build-contracts § 0) on a real PostgreSQL server: the <c>EmailOutcomes</c> table the
/// <c>T400_EmailOutcomes</c> migration adds, reached through <c>MigrateAsync</c>, so a migration EF does not discover (a
/// missing Designer file) fails here. A Sent and a Dropped row are written and read back, a 2,000-character refusal is kept
/// whole, both indexes exist, and a read of the last 24 hours by moment returns only those rows.
/// </summary>
/// <remarks>
/// Isolated the way <c>ActivityTypeAdminScopePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>),
/// registered before it is created and dropped in a finally and again on dispose.
/// </remarks>
public sealed class EmailOutcomePostgresTests : IAsyncLifetime
{
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 41, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheMigratedTable_KeepsEachOutcome_ItsRefusalWhole_AndAnswersTheLast24Hours()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(id => id.EndsWith("_T400_EmailOutcomes", StringComparison.Ordinal));
            }

            var longRefusal = new string('r', EmailOutcome.RefusalMaxLength - 4) + " end";
            await using (var db = NewContext(schema))
            {
                db.Set<EmailOutcome>().AddRange(
                    EmailOutcome.Sent(Now.AddHours(-2)),
                    EmailOutcome.Dropped(Now.AddHours(-1), "550 5.1.1 Mailbox unavailable"),
                    EmailOutcome.Dropped(Now.AddMinutes(-5), longRefusal),
                    EmailOutcome.Dropped(Now.AddMinutes(-1), null),
                    // Outside the window: a day and a second ago, and two days ago.
                    EmailOutcome.Sent(Now.AddDays(-1).AddSeconds(-1)),
                    EmailOutcome.Dropped(Now.AddDays(-2), "421 Try again later"));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var all = await db.Set<EmailOutcome>().AsNoTracking().OrderBy(outcome => outcome.OccurredAt).ToListAsync();
                all.Should().HaveCount(6);
                all.Select(outcome => outcome.Id).Should().OnlyHaveUniqueItems().And.OnlyContain(id => id > 0);

                var sent = all.Single(outcome => outcome.OccurredAt == Now.AddHours(-2));
                sent.Kind.Should().Be(EmailOutcomeKind.Sent);
                sent.Refusal.Should().BeNull();
                sent.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);

                var dropped = all.Single(outcome => outcome.OccurredAt == Now.AddHours(-1));
                dropped.Kind.Should().Be(EmailOutcomeKind.Dropped);
                dropped.Refusal.Should().Be("550 5.1.1 Mailbox unavailable");

                all.Single(outcome => outcome.OccurredAt == Now.AddMinutes(-5)).Refusal
                    .Should().Be(longRefusal).And.HaveLength(EmailOutcome.RefusalMaxLength);
                all.Single(outcome => outcome.OccurredAt == Now.AddMinutes(-1)).Refusal.Should().BeNull();

                var since = Now.AddDays(-1);
                var lastDay = await db.Set<EmailOutcome>().AsNoTracking()
                    .Where(outcome => outcome.OccurredAt >= since && outcome.OccurredAt <= Now)
                    .OrderBy(outcome => outcome.OccurredAt)
                    .ToListAsync();
                lastDay.Select(outcome => outcome.OccurredAt).Should().Equal(
                    Now.AddHours(-2), Now.AddHours(-1), Now.AddMinutes(-5), Now.AddMinutes(-1));

                var droppedInTheWindow = await db.Set<EmailOutcome>()
                    .CountAsync(outcome => outcome.Kind == EmailOutcomeKind.Dropped && outcome.OccurredAt >= since);
                droppedInTheWindow.Should().Be(3);

                var indexes = await db.Database
                    .SqlQueryRaw<string>(
                        """SELECT indexname AS "Value" FROM pg_indexes WHERE schemaname = current_schema() AND tablename = 'EmailOutcomes'""")
                    .ToListAsync();
                indexes.Should().BeEquivalentTo("PK_EmailOutcomes", "IX_EmailOutcomes_OccurredAt", "IX_EmailOutcomes_Kind_OccurredAt");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
