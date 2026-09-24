using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T168 on a real PostgreSQL server: per EPA and semester, whether a released MSF campaign covering the EPA closed in it
/// (<see cref="GetMsfCoverageForTraineeQuery" />).
/// </summary>
/// <remarks>
/// The unit suite runs on EF InMemory, which evaluates the state rule, the recorded-EPA rule and the close-day bounds in
/// memory. Here they must translate: <c>ClosedOn</c> is a <c>timestamptz</c>, so the semester edges are compared on the
/// server as UTC instants, the last instant of 30 June in semester 1 and the first of 1 July in semester 2, and at the
/// span's far end the December fold (D40): a campaign closed in December, as late as its last instant, is semester 2's,
/// and one closed at the first instant of the next year is outside. The schema helpers follow
/// <c>MsfCampaignsOutsideSnapshotPostgresTests</c>.
/// </remarks>
public sealed class MsfCoveragePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string Trainee = "trainee-t168";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task Coverage_OnPostgres_IsReleasedAndRecordedOnly_BucketedByTheUtcCloseDay_InOneServerSideQuery()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int host, first, second, closedLastInstantOfJune, closedFirstInstantOfJuly, closedMidDecember, closedLastInstantOfDecember;

            await using (var db = NewContext(schema))
            {
                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculum = await db.Curricula.Include(entity => entity.Items).SingleAsync(entity => entity.Name == "IM Core Curriculum");
                first = curriculum.Items.Single().EpaId;

                // A second EPA on the seeded curriculum, so one campaign can cover one and not the other.
                var epa = new Epa
                {
                    SubSpecialityId = curriculum.SubSpecialityId,
                    Code = "EPA-T168",
                    Title = "A second EPA for T168",
                    Description = "Integration test.",
                    RequiredKnowledgeSkills = "None.",
                    CreatedOn = DateTime.UtcNow
                };
                db.Epas.Add(epa);
                await db.SaveChangesAsync();
                second = epa.Id;
                curriculum.Items.Add(new CurriculumItem { EpaId = second, RequiredCount = 1, MinimumLevelOrder = 4, WindowMonths = 12 });

                db.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = Trainee,
                    InstitutionId = host,
                    CurriculumId = curriculum.Id,
                    ProgrammeStartDate = new DateOnly(2025, 1, 1),
                    ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                    IsActive = true
                });

                var template = new MsfTemplate { Name = "T168 MSF" };
                db.MsfTemplates.Add(template);
                await db.SaveChangesAsync();

                // Semester 1's last instant, covering both but recording only the first: the second was dropped.
                closedLastInstantOfJune = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-06-30T23:59:30Z", (first, true), (second, false));
                // Semester 2's first instant.
                closedFirstInstantOfJuly = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-07-01T00:00:00Z", (second, true));
                // After the College's 30 November, and the bucket's last instant: semester 2 to the end of December.
                closedMidDecember = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-12-15T12:00:00Z", (first, true));
                closedLastInstantOfDecember = await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-12-31T23:59:30Z", (second, true));
                // Withdrawn, before the year, and after it: none covers anything in 2026.
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Withdrawn, "2026-03-01T12:00:00Z", (second, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2025-12-31T23:59:59Z", (first, true));
                await AddCampaignAsync(db, template.Id, MsfCampaignState.Released, "2027-01-01T00:00:00Z", (first, true), (second, true));
                // Another trainee's.
                await AddCampaignAsync(
                    db, template.Id, MsfCampaignState.Released, "2026-08-01T12:00:00Z", "another-trainee", (first, true));
            }

            var commands = new CommandLog();
            MsfCoverageDto coverage;
            await using (var db = NewContext(schema, commands))
            {
                coverage = (await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
                    new GetMsfCoverageForTraineeQuery(Trainee, CommitteeMember(host), new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)),
                    CancellationToken.None))!;
            }

            coverage.Periods.Select(period => period.Name).Should().Equal("Semester 1, 2026", "Semester 2, 2026");
            Covered(coverage, first).Should().Equal((2026, 1, closedLastInstantOfJune), (2026, 2, closedMidDecember));
            Covered(coverage, second).Should().Equal((2026, 2, closedLastInstantOfDecember));
            coverage.Epas.Single(epa => epa.EpaId == first).For(2026, 1)!.Latest!.ClosedOn.Should().Be(new DateOnly(2026, 6, 30));
            coverage.Epas.Single(epa => epa.EpaId == second).For(2026, 2)!.Campaigns
                .Select(campaign => (campaign.CampaignId, campaign.ClosedOn))
                .Should().Equal(
                    [(closedLastInstantOfDecember, new DateOnly(2026, 12, 31)), (closedFirstInstantOfJuly, new DateOnly(2026, 7, 1))],
                    "semester 2 runs from July's first instant to December's last, newest first");

            var read = commands.Texts.Where(text => text.Contains("\"MsfCampaignEpas\"", StringComparison.Ordinal))
                .Should().ContainSingle("one round trip: nothing is fetched and filtered on the client").Subject;
            read.Should().Contain("\"MsfCampaigns\"", "the state and close-day rules are the campaign's, joined in SQL");
            read.Should().Contain("\"RecordedOn\"");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private static IEnumerable<(int Year, int Semester, int CampaignId)> Covered(MsfCoverageDto coverage, int epaId)
        => coverage.Epas.Single(epa => epa.EpaId == epaId).Periods
            .Where(period => period.IsCovered)
            .Select(period => (period.Year, period.Semester, period.Latest!.CampaignId))
            .ToArray();

    private static Task<int> AddCampaignAsync(
        ApplicationDbContext db,
        int templateId,
        MsfCampaignState state,
        string closedOn,
        params (int EpaId, bool Recorded)[] epas)
        => AddCampaignAsync(db, templateId, state, closedOn, Trainee, epas);

    private static async Task<int> AddCampaignAsync(
        ApplicationDbContext db,
        int templateId,
        MsfCampaignState state,
        string closedOn,
        string subjectUserId,
        params (int EpaId, bool Recorded)[] epas)
    {
        var closed = DateTime.Parse(closedOn, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        var released = closed.AddDays(2);
        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            TemplateId = templateId,
            CreatedByUserId = "coordinator-t168",
            CreatedOn = new DateTime(2025, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2025, 11, 1),
            ClosesOn = DateOnly.FromDateTime(closed),
            State = state,
            OpenedOn = new DateTime(2025, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = closed,
            ReleasedOn = state == MsfCampaignState.Released ? released : null,
            WithdrawnOn = state == MsfCampaignState.Withdrawn ? released : null
        };

        foreach (var (epaId, recorded) in epas)
        {
            campaign.CoveredEpas.Add(new MsfCampaignEpa { EpaId = epaId, RecordedOn = recorded ? released : null });
        }

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    /// <summary>A committee member at the trainee's institution: overseen, so the scope lookup runs on the server too.</summary>
    private static ClaimsPrincipal CommitteeMember(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "committee-t168"),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>A migrated schema with the demo institution and its curriculum seeded (<see cref="DataSeeder" />).</summary>
    private async Task<string> MigratedSchemaAsync()
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

    /// <summary>Drops every schema this test created. Called from the test's finally and again from DisposeAsync.</summary>
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

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>MsfRespondEndpointFlowTests</c>.</summary>
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
