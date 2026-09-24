using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// A withdraw that another change to the campaign races is refused with a message a coordinator can act on, and stores
/// nothing. (T206 review)
/// </summary>
/// <remarks>
/// <para>
/// Withdrawing saves under the campaign's <c>xmin</c> token (<c>MsfCampaignConfiguration</c>), and since T206 adding an
/// invitee writes the campaign row too, as do opening and closing it. So a withdraw whose save comes after one of those
/// is refused by the server. Until the T206 review its refusal reached the campaign list as EF's own message ("The
/// database operation was expected to affect 1 row(s)…"), a link to Microsoft's documentation included.
/// </para>
/// <para>
/// EF InMemory has no <c>xmin</c>, so only Postgres can show this. The withdraw runs inside the real
/// <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real <see cref="AuditWriter" /> on its own
/// context, as a request would, so the refused changes meet the audit trap too (T201). The schema helpers follow
/// <c>MsfInviteDuringOpenRacePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class MsfWithdrawRacePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string LateInvitee = "peer-late@example.test";

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "consultant-1@example.test"
    ];

    private readonly List<string> _schemas = [];
    private readonly InvitationTokenService _tokens = new();
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task AWithdrawThatAnAddedInviteeRaced_IsRefusedSayingWhat_AndStoresNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The withdraw has read the draft and anonymised it in memory; just before its save, the other tab's add runs
            // from start to save.
            var raceBeforeTheWithdrawsSave = new RaceBeforeFirstSave();
            raceBeforeTheWithdrawsSave.Arm(() => AddAsync(schema, campaignId, LateInvitee));

            var withdraw = () => WithdrawThroughTheAuditPipelineAsync(schema, campaignId, raceBeforeTheWithdrawsSave);
            (await withdraw.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(WithdrawMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the add changed the campaign's xmin under the withdraw");

            raceBeforeTheWithdrawsSave.Ran.Should().BeTrue("guard: the add ran between the withdraw's read and its save");

            await using var read = NewContext(schema);
            var campaign = await read.MsfCampaigns
                .AsNoTracking()
                .Include(entity => entity.Invitations)
                .SingleAsync(entity => entity.Id == campaignId);

            campaign.State.Should().Be(MsfCampaignState.Draft, "the refused withdraw stored nothing");
            campaign.WithdrawnOn.Should().BeNull();
            campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                .Should().BeEquivalentTo([.. Respondents, LateInvitee], "nobody was anonymised, and the add committed");
            campaign.Invitations.Should().OnlyContain(invitation => invitation.AnonymizedOn == null);

            (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(WithdrawMsfCampaignCommand))
                    .Select(entry => entry.Success)
                    .ToListAsync())
                .Should().Equal([false], "the refused withdraw leaves its failure row, written alone (T201)");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private async Task WithdrawThroughTheAuditPipelineAsync(string schema, int campaignId, IInterceptor interceptor)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new WithdrawMsfCampaignCommand(campaignId, Administrator());
        var handler = new WithdrawMsfCampaignCommandHandler(db);

        await new AuditPipelineBehavior<WithdrawMsfCampaignCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await handler.Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    private async Task AddAsync(string schema, int campaignId, string email)
    {
        await using var db = NewContext(schema);
        await new AddMsfInvitationCommandHandler(db, new InvitationTokenService()).Handle(
            new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.PeerDoctor, Administrator()),
            CancellationToken.None);
    }

    private async Task<int> SeedDraftCampaignAsync(string schema)
    {
        await using var db = NewContext(schema);
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            State = MsfCampaignState.Draft,
            Template = new MsfTemplate { Name = "T206 review MSF" },
            Invitations = Respondents
                .Select(email => new MsfInvitation
                {
                    RespondentEmail = email,
                    RespondentCategory = email.StartsWith("nurse", StringComparison.Ordinal)
                        ? MsfRespondentCategory.Nurse
                        : MsfRespondentCategory.Consultant,
                    TokenHash = _tokens.HashToken(_tokens.GenerateToken()),
                    IssuedOn = DateTime.UtcNow,
                    ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(21)
                })
                .ToList()
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>Runs the competing request once, just before the first save on the context it is attached to.</summary>
    private sealed class RaceBeforeFirstSave : SaveChangesInterceptor
    {
        private Func<Task>? _armed;

        public bool Ran { get; private set; }

        public void Arm(Func<Task> competing) => _armed = competing;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            // Disarmed before it runs, so the audit row's own save after the refusal passes straight through.
            var competing = Interlocked.Exchange(ref _armed, null);
            if (competing is not null)
            {
                await competing();
                Ran = true;
            }

            return result;
        }
    }

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "admin-1";
        public string? UserDisplay => "Admin";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }

    // ─── Schema helpers (as MsfInviteDuringOpenRacePostgresTests) ────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

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

    private ApplicationDbContext NewContext(string schema, IInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
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
}
