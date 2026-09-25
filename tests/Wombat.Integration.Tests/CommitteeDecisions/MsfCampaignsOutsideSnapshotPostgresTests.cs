using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T173 on a real PostgreSQL server: the count of a review window's MSF campaigns that its evidence snapshot does not
/// hold (<see cref="CountMsfCampaignsOutsideSnapshotQuery" />), and its agreement with the snapshot itself.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which evaluates the window, the state rule, the group-by and the "no evidence
/// item of this review names it" subquery in memory. On Npgsql the subquery must translate to a correlated EXISTS and
/// the grouping to GROUP BY, or EF throws, or the campaigns are counted on the client. <c>ClosedOn</c> is a
/// <c>timestamptz</c>, so the window's UTC bounds are compared on the server too. Both are asserted on what reached
/// the server.
/// </para>
/// <para>
/// The schema helpers follow <c>MsfCampaignScopePostgresTests</c>: the test builds its context on a schema of its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>), registered before it is created and dropped in a <c>finally</c>, with
/// <see cref="DisposeAsync" /> as a backstop. Nothing that can fail runs in <see cref="InitializeAsync" />.
/// </para>
/// </remarks>
public sealed class MsfCampaignsOutsideSnapshotPostgresTests : IAsyncLifetime
{
    private const string Trainee = "trainee-t173";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheCount_OnPostgres_FollowsTheSnapshotThroughStartAndRelease_InOneServerSideQuery()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int host, reviewId, closedOnLastDay, releasedBeforeStart;

            await using (var db = NewContext(schema))
            {
                // The panel is at the seeded demo institution, and so is the trainee: a panel acts only on its own
                // institution's trainees, and Start checks it (T182).
                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = Trainee,
                    InstitutionId = host,
                    CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2025, 1, 1),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                    IsActive = true
                });

                var panel = new DecisionPanel
                {
                    Name = "T173 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = host,
                    CreatedOn = DateTime.UtcNow,
                    Members = [new DecisionPanelMember { UserId = "chair-t173", Role = DecisionPanelMemberRole.Chair }]
                };
                db.DecisionPanels.Add(panel);
                var template = new MsfTemplate { Name = "T173 MSF" };
                db.MsfTemplates.Add(template);
                await db.SaveChangesAsync();

                var review = new CommitteeReview
                {
                    AcademicYear = 2026,
                    Semester = 1,
                    PanelId = panel.Id,
                    TraineeUserId = Trainee,
                    ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                    ReviewPeriodTo = new DateOnly(2026, 3, 31),
                    ScheduledOn = new DateOnly(2026, 4, 2)
                };
                db.CommitteeReviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.Id;

                // The window's first and last instants, in UTC, are in; the instants either side are out.
                closedOnLastDay = await AddCampaignAsync(db, template.Id, MsfCampaignState.UnderReview, "2026-03-31T23:59:30Z");
                await AddCampaignAsync(db, template.Id, MsfCampaignState.UnderReview, "2026-01-01T00:00:00Z");
                await AddCampaignAsync(db, template.Id, MsfCampaignState.UnderReview, "2026-04-01T00:00:00Z");
                await AddCampaignAsync(db, template.Id, MsfCampaignState.UnderReview, "2025-12-31T23:59:59Z");
                releasedBeforeStart = await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2026-02-10T12:00:00Z");
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Withdrawn, "2026-02-11T12:00:00Z");
                await AddCampaignAsync(db, template.Id, MsfCampaignState.UnderReview, "2026-02-12T12:00:00Z", "another-trainee");
            }

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                (await CountAsync(db, reviewId, host)).Should().Be(new MsfCampaignsOutsideSnapshotDto(2, 0));
            }

            var count = commands.Texts.Where(text => text.Contains("\"MsfCampaigns\"", StringComparison.Ordinal))
                .Should().ContainSingle("one round trip: nothing is fetched and counted on the client").Subject;
            count.Should().Contain("GROUP BY");
            count.Should().Contain("EXISTS", "the snapshot-membership test runs in SQL");
            count.Should().Contain("\"CommitteeEvidenceItems\"");

            await using (var db = NewContext(schema))
            {
                var started = await new StartCommitteeReviewCommandHandler(db).Handle(
                    new StartCommitteeReviewCommand(reviewId, Chair(host)),
                    CancellationToken.None);
                started.EvidenceItems.Where(item => item.MsfCampaignId is not null).Select(item => item.MsfCampaignId)
                    .Should().Equal([releasedBeforeStart], "the setup: Start takes the one released campaign");
            }

            await using (var db = NewContext(schema))
            {
                // Still two awaiting release, now measured against a snapshot that holds a campaign.
                (await CountAsync(db, reviewId, host)).Should().Be(new MsfCampaignsOutsideSnapshotDto(2, 0));

                var campaign = await db.MsfCampaigns.SingleAsync(entity => entity.Id == closedOnLastDay);
                campaign.State = MsfCampaignState.Released;
                campaign.ReleasedOn = new DateTime(2026, 4, 5, 9, 0, 0, DateTimeKind.Utc);
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                (await CountAsync(db, reviewId, host)).Should().Be(
                    new MsfCampaignsOutsideSnapshotDto(1, 1),
                    "a campaign released after Start is still in no snapshot, and the one released before it is in this one");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<MsfCampaignsOutsideSnapshotDto> CountAsync(ApplicationDbContext db, int reviewId, int institutionId)
        => new CountMsfCampaignsOutsideSnapshotQueryHandler(db).Handle(
            new CountMsfCampaignsOutsideSnapshotQuery(reviewId, Chair(institutionId)),
            CancellationToken.None);

    private static async Task<int> AddCampaignAsync(
        ApplicationDbContext db,
        int templateId,
        MsfCampaignState state,
        string closedOn,
        string subjectUserId = Trainee)
    {
        var closed = DateTime.Parse(closedOn, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            TemplateId = templateId,
            CreatedByUserId = "coordinator-t173",
            CreatedOn = new DateTime(2025, 12, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2025, 12, 1),
            ClosesOn = DateOnly.FromDateTime(closed),
            State = state,
            OpenedOn = new DateTime(2025, 12, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = closed,
            ReleasedOn = state == MsfCampaignState.Released ? closed.AddDays(1) : null,
            WithdrawnOn = state == MsfCampaignState.Withdrawn ? closed.AddDays(1) : null
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private static ClaimsPrincipal Chair(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "chair-t173"),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>
    /// A migrated schema with the demo institution and its curricula seeded (<see cref="DataSeeder" />): a panel must
    /// belong to an institution that exists, and its trainee must hold a profile there (T182).
    /// </summary>
    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        return schema;
    }

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>Every query EF sends through one context, so "one round trip, all in SQL" is asserted on the wire.</summary>
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
