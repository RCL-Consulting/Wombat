using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T165 on real PostgreSQL: the attendance each decision records is stored by the migration's table and read back with
/// its roles, one row per member per decision, so the chair can sit for both the review's decision and an appeal's
/// replacement; and erasure pseudonymises a member's attendance and the evidence lines naming them as the assessor. The
/// erasure executor runs raw SQL, so the in-memory provider cannot run it.
/// </summary>
/// <remarks>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose.
/// </remarks>
public sealed class CommitteeAttendancePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task EachDecisionsAttendance_IsStoredWithItsRoles_AndErasurePseudonymisesAMembersPresenceAndAssessments()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        try
        {
            await using (var connection = new NpgsqlConnection(_baseConnectionString))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = $"CREATE SCHEMA \"{schema}\"";
                await command.ExecuteNonQueryAsync();
            }

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(SchemaConnectionString(schema)));
            services.AddIdentity<WombatIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<ErasureExecutor>();

            await using var root = services.BuildServiceProvider();

            await using (var migrationScope = root.CreateAsyncScope())
            {
                await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
            }

            string memberId;
            int reviewId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var users = arrange.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                var member = new WombatIdentityUser
                {
                    UserName = "naidoo@kgk.test",
                    Email = "naidoo@kgk.test",
                    FirstName = "Priya",
                    LastName = "Naidoo"
                };
                (await users.CreateAsync(member)).Succeeded.Should().BeTrue();
                memberId = member.Id;

                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "Kgosi Kgari", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();

                var members = new[]
                {
                    new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = memberId, Role = DecisionPanelMemberRole.Member },
                    new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
                };
                var panel = new DecisionPanel
                {
                    Name = "Paediatrics CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = institution.Id,
                    CreatedOn = DateTime.UtcNow,
                    Members = members
                };

                var sitting = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc);
                var review = new CommitteeReview
                {
                    AcademicYear = 2026,
                    Semester = 1,
                    Panel = panel,
                    TraineeUserId = "trainee-1",
                    ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                    ReviewPeriodTo = new DateOnly(2026, 6, 30),
                    ScheduledOn = new DateOnly(2026, 7, 2)
                };
                review.Start(
                    [
                        new CommitteeEvidence
                        {
                            SourceType = CommitteeEvidenceSourceType.Activity,
                            ActivityId = 100,
                            SourceLabel = "CCA #100",
                            Summary = "State: completed.",
                            RatingOrder = 3,
                            RatingLabel = "3a",
                            AssessorUserId = memberId
                        }
                    ],
                    "chair-1",
                    sitting);
                review.RecordDecision(
                    CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", sitting,
                    [members[0], members[1]]);
                review.Ratify("chair-1", sitting);

                // An appeal remits the decision; the appeal's own sitting takes the replacement. The chair sits for both,
                // which a unique index on (review, member) would refuse.
                var appealSitting = sitting.AddDays(30);
                review.LodgeAppeal("The conditions are disproportionate.", "trainee-1", appealSitting);
                review.ResolveAppeal(
                    CommitteeAppealOutcome.Remitted, "external-1", appealSitting, CommitteeDecisionCategory.SatisfactoryProgress,
                    "Conditions lifted on appeal.", null, [members[0], members[2]]);

                db.DecisionPanels.Add(panel);
                db.CommitteeReviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.Id;
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stored = await db.CommitteeReviews.AsNoTracking()
                    .Include(review => review.Decisions)
                        .ThenInclude(decision => decision.Attendees)
                    .SingleAsync(review => review.Id == reviewId);

                var replacement = stored.GetCurrentDecision()!;
                var original = stored.Decisions.Single(decision => decision.Id != replacement.Id);
                original.Attendees.Select(attendee => $"{attendee.UserId}:{attendee.Role}")
                    .Should().BeEquivalentTo("chair-1:Chair", $"{memberId}:Member");
                replacement.Attendees.Select(attendee => $"{attendee.UserId}:{attendee.Role}")
                    .Should().BeEquivalentTo("chair-1:Chair", "external-1:External");
                stored.QuorumShortfall().Should().BeNull();
            }

            await using (var act = root.CreateAsyncScope())
            {
                var request = DataRightsRequest.Create(
                    memberId, "Priya Naidoo", DataRightsRequestType.Erasure, "Leaving the programme.", DateTime.UtcNow);
                var db = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Set<DataRightsRequest>().Add(request);
                await db.SaveChangesAsync();

                await act.ServiceProvider.GetRequiredService<ErasureExecutor>().ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
            }

            await using (var assert = root.CreateAsyncScope())
            {
                var db = assert.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var attendees = await db.CommitteeDecisionAttendees.AsNoTracking()
                    .Where(attendee => attendee.Decision.ReviewId == reviewId)
                    .ToListAsync();
                attendees.Should().HaveCount(4, "the record of who sat is kept, under a pseudonym");
                attendees.Should().NotContain(attendee => attendee.UserId == memberId);
                attendees.Should().ContainSingle(attendee => attendee.Role == DecisionPanelMemberRole.Member)
                    .Which.UserId.Should().NotBe(memberId);

                var line = await db.CommitteeEvidenceItems.AsNoTracking().SingleAsync(item => item.ReviewId == reviewId);
                line.AssessorUserId.Should().NotBeNull().And.NotBe(memberId);
                line.AssessorUserId.Should().Be(attendees.Single(attendee => attendee.Role == DecisionPanelMemberRole.Member).UserId,
                    "one person is one pseudonym wherever they appear");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString) { SearchPath = schema, Pooling = false }.ConnectionString;

    private async Task DropSchemasAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas.ToList())
        {
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    /// <summary>The same resolution order as the other PostgreSQL tests.</summary>
    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }
}
