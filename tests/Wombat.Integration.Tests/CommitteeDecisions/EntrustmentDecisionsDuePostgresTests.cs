using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 6 on a real PostgreSQL server, migrated and seeded with the v11.1 catalogue: the decisions-due query reads
/// the trainee's cadence, the STARs and the agenda lines through Npgsql, and says where each decision stands.
/// </summary>
/// <remarks>
/// The handler tests run on EF InMemory, which translates nothing: not the preferred-profile predicate
/// (<see cref="TraineeScopeResolver" />), the join from a STAR to the sitting that issued it, nor a line's projection
/// through its review, panel and STAR. The schema helpers follow <c>CommitteeEvidenceSnapshotPostgresTests</c>: a schema
/// of the test's own, registered before it is created and dropped in a <c>finally</c>, with <see cref="DisposeAsync" />
/// as a backstop.
/// </remarks>
public sealed class EntrustmentDecisionsDuePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string TraineeUserId = "trainee-t131-6";
    private const string ChairUserId = "chair-t131-6";
    private const string MemberUserId = "member-t131-6";

    private static readonly DateOnly Today = new(2026, 9, 24);

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task TheDecisionsDue_ReadTheCadenceTheStarsAndTheAgenda_OnPostgres()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int hostId, panelId, paed001, firstReviewId;

            await using (var db = NewContext(schema))
            {
                hostId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula
                    .Where(entity => entity.Name == "Paediatric EPA Curriculum" && entity.Version == "11.1")
                    .Select(entity => entity.Id)
                    .SingleAsync();
                paed001 = await db.Epas.Where(epa => epa.Code == "PAED-001" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();

                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = TraineeUserId,
                    InstitutionId = hostId,
                    CurriculumId = curriculumId,
                    ProgrammeStartDate = new DateOnly(2025, 1, 15),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 14)
                });

                var panel = new DecisionPanel
                {
                    Name = "T131 slice 6 CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = hostId,
                    CreatedOn = DateTime.UtcNow,
                    Members =
                    [
                        new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
                        new DecisionPanelMember { UserId = MemberUserId, Role = DecisionPanelMemberRole.Member }
                    ]
                };
                db.DecisionPanels.Add(panel);
                await db.SaveChangesAsync();
                panelId = panel.Id;
            }

            // Semester 1: PAED-001 staged and ratified, every other closing line deferred.
            firstReviewId = await ScheduleAsync(schema, panelId, hostId, 2026, 1);
            await using (var db = NewContext(schema))
            {
                await new StartCommitteeReviewCommandHandler(db).Handle(
                    new StartCommitteeReviewCommand(firstReviewId, Chair()), CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                var evidence = new CommitteeEvidence
                {
                    ReviewId = firstReviewId,
                    SourceType = CommitteeEvidenceSourceType.Activity,
                    ActivityId = 9001,
                    EpaId = paed001,
                    EpaCode = "PAED-001",
                    SourceLabel = "Mini-CEX #9001",
                    Summary = "State: completed.",
                    ObservedOn = new DateOnly(2026, 2, 10)
                };
                db.CommitteeEvidenceItems.Add(evidence);
                await db.SaveChangesAsync();

                var rung = await db.EntrustmentLevels
                    .Where(level => level.Scale.Name == "CPSA Paediatric Entrustment Scale v11.1" && level.Label == "3a")
                    .Select(level => level.Id)
                    .SingleAsync();
                await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
                    new StagePendingEntrustmentDecisionCommand(
                        firstReviewId, null, paed001, rung, new DateOnly(2026, 7, 2), null, "Target met.", [evidence.Id], Chair()),
                    CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                var closing = await db.CommitteeAgendaLines
                    .Where(line => line.ReviewId == firstReviewId && line.IsClosing && line.EpaId != paed001)
                    .Select(line => line.Id)
                    .ToListAsync();
                foreach (var lineId in closing)
                {
                    await new DeferAgendaLineCommandHandler(db).Handle(
                        new DeferAgendaLineCommand(firstReviewId, lineId, "Not at a decision point.", Chair()), CancellationToken.None);
                }
            }

            await using (var db = NewContext(schema))
            {
                await new RecordCommitteeDecisionCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(hostId, ChairUserId, MemberUserId))
                    .Handle(
                        new RecordCommitteeDecisionCommand(
                            firstReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null,
                            [ChairUserId, MemberUserId], Chair()),
                        CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                await new RatifyCommitteeDecisionCommandHandler(db).Handle(
                    new RatifyCommitteeDecisionCommand(firstReviewId, Chair()), CancellationToken.None);
            }

            var semester1 = await DueAsync(schema, hostId, 2026, 1);

            // Semester 2: a review scheduled, not yet started. Its agenda holds the year's annual EPAs too, so from here
            // they read Scheduled in semester 1's view as well: their window is the year.
            var secondReviewId = await ScheduleAsync(schema, panelId, hostId, 2026, 2);
            var semester2 = await DueAsync(schema, hostId, 2026, 2);

            int starId;
            await using (var db = NewContext(schema))
            {
                starId = await db.EntrustmentDecisions.Where(star => star.TraineeUserId == TraineeUserId).Select(star => star.Id).SingleAsync();
            }

            var mine1 = semester1.Items.Where(item => item.TraineeUserId == TraineeUserId).ToArray();
            mine1.Should().HaveCount(15, "every v11.1 EPA has a cadence, and a semester-1 sitting's period holds each window");
            var decided = mine1.Single(item => item.EpaCode == "PAED-001");
            (decided.Status, decided.ReviewId, decided.EntrustmentDecisionId, decided.MayOpenReview)
                .Should().Be((EntrustmentDecisionDueStatus.Decided, firstReviewId, starId, true));
            mine1.Where(item => item.EpaCode is "PAED-002" or "PAED-004" or "PAED-005" or "PAED-010" or "PAED-012")
                .Should().OnlyContain(item => item.Status == EntrustmentDecisionDueStatus.Deferred && item.ReviewId == firstReviewId);
            mine1.Where(item => item.EpaCode is "PAED-003" or "PAED-006")
                .Should().OnlyContain(item => item.Status == EntrustmentDecisionDueStatus.DueByYearEnd);
            mine1.Where(item => item.EpaCode is "PAED-008" or "PAED-009" or "PAED-013")
                .Should().OnlyContain(item => item.Status == EntrustmentDecisionDueStatus.AsOpportunityAllows);
            decided.SchedulePanelId.Should().Be(panelId, "no neonatal panel: the general panel decides every EPA");

            var mine2 = semester2.Items.Where(item => item.TraineeUserId == TraineeUserId).ToArray();
            mine2.Should().HaveCount(15);
            var scheduled = mine2.Single(item => item.EpaCode == "PAED-001");
            (scheduled.Status, scheduled.ReviewId).Should().Be((EntrustmentDecisionDueStatus.Scheduled, secondReviewId));
            mine2.Where(item => item.EpaCode is "PAED-003" or "PAED-008")
                .Should().OnlyContain(item => item.Status == EntrustmentDecisionDueStatus.Scheduled && item.ReviewId == secondReviewId);
            var annualInSemester1 = (await DueAsync(schema, hostId, 2026, 1)).Items
                .Single(item => item.TraineeUserId == TraineeUserId && item.EpaCode == "PAED-003");
            annualInSemester1.Status.Should().Be(
                EntrustmentDecisionDueStatus.Scheduled, "an annual EPA's window is the year, which the new review decides");

            // T131 slice 6 review: the seat the open semester-2 review holds is read on Postgres by the scheduling
            // handler's predicate, and an annual EPA schedules the year's last sitting.
            (annualInSemester1.SchedulePeriodKey, annualInSemester1.HoldingReviewId, annualInSemester1.MayOpenHoldingReview)
                .Should().Be(("2026-2", secondReviewId, true));
            mine2.Single(item => item.EpaCode == "PAED-002").HoldingReviewId.Should().Be(secondReviewId);

            // The STAR revoked: its window is to be decided again.
            await using (var db = NewContext(schema))
            {
                var star = await db.EntrustmentDecisions.SingleAsync(entity => entity.Id == starId);
                star.Revoke("Issued in error.", "admin-user", DateTime.UtcNow);
                await db.SaveChangesAsync();
            }

            var revoked = (await DueAsync(schema, hostId, 2026, 1)).Items
                .Single(item => item.TraineeUserId == TraineeUserId && item.EpaCode == "PAED-001");
            (revoked.Status, revoked.EntrustmentDecisionId).Should().Be((EntrustmentDecisionDueStatus.Revoked, starId));
            (revoked.SchedulePeriodKey, revoked.HoldingReviewId).Should().Be(
                ("2026-1", (int?)null), "the semester-1 review is ratified, so nothing holds that seat");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private async Task<int> ScheduleAsync(string schema, int panelId, int hostId, int year, int semester)
    {
        await using var db = NewContext(schema);
        var review = await new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new ScheduleCommitteeReviewCommand(
                TraineeUserId, panelId, year, semester, new DateOnly(year, 1, 1), new DateOnly(year, 12, 31),
                new DateOnly(2026, 7, 2), Coordinator(hostId)),
            CancellationToken.None);
        return review.Id;
    }

    private async Task<EntrustmentDecisionsDueDto> DueAsync(string schema, int hostId, int year, int semester)
    {
        await using var db = NewContext(schema);
        var due = await new GetEntrustmentDecisionsDueQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new GetEntrustmentDecisionsDueQuery(year, semester, null, Coordinator(hostId), Today),
            CancellationToken.None);
        return due ?? throw new InvalidOperationException("The query returned nothing.");
    }

    private static ClaimsPrincipal Chair()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ChairUserId),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-t131-6"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ---- schemas --------------------------------------------------------------------------------------------------------

    private async Task<string> SeededSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

        return schema;
    }

    /// <summary>A new, empty schema, registered for dropping before it exists so that no failure can leak it.</summary>
    private async Task<string> CreateSchemaAsync()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{schema}\"";
        await command.ExecuteNonQueryAsync();

        return schema;
    }

    /// <summary>Drops every schema this test created. Called from each test's finally and again from DisposeAsync.</summary>
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
            // Belt and braces: this class only ever drops a schema it named itself.
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema)).Options);

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>WbaToolAllowListPostgresTests</c>.</summary>
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
