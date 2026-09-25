using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T113 on a real PostgreSQL server: the coordinator's campaign list, confined through
/// <see cref="TraineeScopeResolver.PreferredProfiles" /> in SQL.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which evaluates the preferred-profile predicate - "no profile of the same trainee
/// ranks above this one" - in memory. On Npgsql it has to translate to a correlated NOT EXISTS inside the campaign
/// query's EXISTS, or EF throws, or the list is filtered on the client after every campaign in the country has been
/// read. Both are asserted on what reached the server. <c>MsfRespondEndpointFlowTests</c> would have covered the list on
/// Postgres, but it cannot set itself up on a fresh schema ([T140]).
/// </para>
/// <para>
/// The schema helpers follow <c>NomineeDirectoryPostgresTests</c>: each test builds its context on a schema of its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>), registered before it is created and dropped in a <c>finally</c>, with
/// <see cref="DisposeAsync" /> as a backstop. Nothing that can fail runs in <see cref="InitializeAsync" />.
/// </para>
/// </remarks>
public sealed class MsfCampaignScopePostgresTests : IAsyncLifetime
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
    public async Task TheCoordinatorList_OnPostgres_IsConfinedToTheirInstitution_InOneServerSideQuery()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int host, elsewhere, single, other, twoProfiles, unadmitted;

            await using (var db = NewContext(schema))
            {
                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var institution = new Institution { Name = "Elsewhere General", ShortCode = "T113-ELSE" };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();
                elsewhere = institution.Id;

                var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();

                // trainee-host trains at DEMO; trainee-else at the other institution. trainee-two has no current
                // profile and two past ones (the unique index allows one ACTIVE profile per trainee, so this is the
                // only way two can tie): DEMO first, with the LATER programme start, then the other institution. The
                // shared tie-break (IsActive, then Id) says the second; the retired MSF copy (programme start) said the
                // first.
                AddProfile(db, "trainee-host", host, curriculumId, new DateOnly(2025, 1, 1));
                AddProfile(db, "trainee-else", elsewhere, curriculumId, new DateOnly(2025, 1, 1));
                AddProfile(db, "trainee-two", host, curriculumId, new DateOnly(2025, 6, 1), isActive: false);
                await db.SaveChangesAsync();
                AddProfile(db, "trainee-two", elsewhere, curriculumId, new DateOnly(2024, 1, 1), isActive: false);
                await db.SaveChangesAsync();

                var template = new MsfTemplate { Name = "T113 MSF" };
                db.MsfTemplates.Add(template);
                await db.SaveChangesAsync();

                single = await AddCampaignAsync(db, template.Id, "trainee-host");
                other = await AddCampaignAsync(db, template.Id, "trainee-else");
                twoProfiles = await AddCampaignAsync(db, template.Id, "trainee-two");
                unadmitted = await AddCampaignAsync(db, template.Id, "no-profile-anywhere");
            }

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                (await ListAsync(db, Coordinator(host))).Should().BeEquivalentTo([single]);
            }

            var listing = commands.Texts.Should().ContainSingle("one round trip: nothing is fetched and filtered on the client").Subject;
            listing.Should().Contain("\"TraineeProfiles\"");
            listing.Should().Contain("NOT EXISTS", "the preferred-profile predicate runs in SQL");

            await using (var db = NewContext(schema))
            {
                (await ListAsync(db, Coordinator(elsewhere))).Should().BeEquivalentTo([other, twoProfiles]);
                (await ListAsync(db, Administrator())).Should().BeEquivalentTo([single, other, twoProfiles, unadmitted]);

                // The single-campaign check reads the same definition, so it agrees with the list row for row.
                (await TraineeScopeResolver.ResolveAsync(db, "trainee-two", CancellationToken.None))!
                    .InstitutionId.Should().Be(elsewhere);
                (await new GetCampaignAggregateReportQueryHandler(db, new MsfAggregationService())
                        .Handle(new GetCampaignAggregateReportQuery(twoProfiles, Coordinator(host)), CancellationToken.None))
                    .Should().BeNull();
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// T217 on a real server: the campaign page's invitee counts are grouped by respondent group and count the responses
    /// as the report does, and nothing sent to the server reads a respondent's address.
    /// </summary>
    [Fact]
    public async Task TheCampaignPagesInviteeCounts_OnPostgres_AreByGroup_AndNeverReadAnAddress()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int host, campaignId;

            await using (var db = NewContext(schema))
            {
                host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
                AddProfile(db, "trainee-host", host, curriculumId, new DateOnly(2025, 1, 1));

                var template = new MsfTemplate { Name = "T217 MSF" };
                db.MsfTemplates.Add(template);
                await db.SaveChangesAsync();

                campaignId = await AddCampaignAsync(db, template.Id, "trainee-host");
                var campaign = await db.MsfCampaigns.SingleAsync(entity => entity.Id == campaignId);
                campaign.State = MsfCampaignState.Open;
                campaign.OpenedOn = DateTime.UtcNow;

                var invitees = new[]
                {
                    (MsfRespondentCategory.Nurse, false),
                    (MsfRespondentCategory.PeerDoctor, true),
                    (MsfRespondentCategory.PeerDoctor, false)
                };
                var number = 0;
                foreach (var (category, responded) in invitees)
                {
                    var invitation = new MsfInvitation
                    {
                        CampaignId = campaignId,
                        RespondentEmail = $"t217-respondent-{++number}@example.test",
                        RespondentCategory = category,
                        TokenHash = Guid.NewGuid().ToString("N"),
                        IssuedOn = DateTime.UtcNow,
                        ExpiresOn = campaign.ClosesOn.AddDays(7)
                    };

                    if (responded)
                    {
                        invitation.RecordResponse(DateTime.UtcNow);
                        invitation.Responses.Add(new MsfResponse { CampaignId = campaignId, SubmittedOn = DateTime.UtcNow });
                    }

                    db.MsfInvitations.Add(invitation);
                }

                await db.SaveChangesAsync();
            }

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                var setup = await new GetMsfCampaignSetupQueryHandler(db, FakeUserDirectory.Empty)
                    .Handle(new GetMsfCampaignSetupQuery(campaignId, Coordinator(host)), CancellationToken.None);

                setup.Should().NotBeNull();
                setup!.State.Should().Be(MsfCampaignState.Open);
                setup.Invitees.Should().Equal(
                    new MsfInviteeCountDto(MsfRespondentCategory.PeerDoctor, 2, 1),
                    new MsfInviteeCountDto(MsfRespondentCategory.Nurse, 1, 0));
            }

            commands.Texts.Should().Contain(text => text.Contains("\"MsfInvitations\""), "the invitees are read on the server");
            commands.Texts.Should().NotContain(text => text.Contains("RespondentEmail"),
                "the page counts invitees and never reads who they are");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private static async Task<int[]> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => (await new ListMsfCampaignsForCoordinatorQueryHandler(db, FakeUserDirectory.Empty)
                .Handle(new ListMsfCampaignsForCoordinatorQuery(principal), CancellationToken.None))
            .Select(summary => summary.Id)
            .ToArray();

    private static void AddProfile(
        ApplicationDbContext db,
        string userId,
        int institutionId,
        int curriculumId,
        DateOnly start,
        bool isActive = true)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = start,
            ExpectedCompletionDate = start.AddYears(4),
            IsActive = isActive
        });

    private static async Task<int> AddCampaignAsync(ApplicationDbContext db, int templateId, string subjectUserId)
    {
        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            TemplateId = templateId,
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2026, 9, 1),
            ClosesOn = new DateOnly(2026, 9, 30),
            State = MsfCampaignState.Draft
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, $"coordinator-{institutionId}"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(System.Globalization.CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

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
