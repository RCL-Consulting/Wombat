using System.Security.Claims;
using System.Text.Json;
using System.Web;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Accounts;
using Wombat.Application.Features.Institutions.Commands.CreateInstitution;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Audit;

/// <summary>
/// T201 on a real PostgreSQL server: when a command's own save is refused, its failure audit row is stored and the
/// handler's own exception reaches the caller.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AuditWriter" /> saves through the request's DbContext, the one the handler used. After a refused save EF
/// keeps the refused changes tracked, so until T201 the audit write in the pipeline's catch sent them again. Refused a
/// second time, it lost the audit row, and EF's second exception replaced the handler's: the concurrency error a racing
/// request should see, or the "already exists" a handler had translated a unique-index violation into.
/// </para>
/// <para>
/// Only a real server refuses a save: EF InMemory has neither <c>xmin</c> nor unique indexes. Each command runs inside
/// the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real <see cref="AuditWriter" />
/// on its own context, as a request would. The race is <c>MsfOpenCampaignRacePostgresTests</c>' (T184), and so are the
/// schema helpers: a schema of its own, dropped in a <c>finally</c> and again in <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class AuditOnRefusedSavePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string RespondUrl = "https://wombat.example/msf/respond";

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
    public async Task AConcurrencyConflictInTheHandlersSave_LeavesAFailureAuditRow_AndSurfacesTheHandlersOwnError()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The second open reads the draft, and at its first send the first open runs from start to save. The
            // second's save then meets the campaign's changed xmin.
            var first = new RecordingSender(beforeFirstSend: null);
            var firstOutcome = new HandlerOutcome();
            var second = new RecordingSender(
                beforeFirstSend: () => OpenThroughTheAuditPipelineAsync(schema, campaignId, first, firstOutcome));
            var secondOutcome = new HandlerOutcome();

            var secondOpen = () => OpenThroughTheAuditPipelineAsync(schema, campaignId, second, secondOutcome);

            // Since T206 the handler names the refusal for the coordinator, and carries the concurrency error inside it.
            var thrown = (await secondOpen.Should().ThrowAsync<InvalidOperationException>()).Which;
            secondOutcome.Thrown.Should().BeOfType<InvalidOperationException>()
                .Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>("the handler's own save was refused");
            thrown.Should().BeSameAs(
                secondOutcome.Thrown,
                "the caller must see the handler's error, not one the audit write raised re-saving the refused changes");
            firstOutcome.Thrown.Should().BeNull();

            await using var read = NewContext(schema);
            var rows = await read.AuditEntries
                .AsNoTracking()
                .Where(entry => entry.Action == nameof(OpenMsfCampaignCommand))
                .ToListAsync();

            rows.Should().HaveCount(2, "the open that saved and the open that was refused each leave a row");
            rows.Should().ContainSingle(entry => entry.Success);
            var failure = rows.Single(entry => !entry.Success);
            failure.ErrorMessage.Should().Be(secondOutcome.Thrown!.Message);
            failure.ActorUserId.Should().Be("admin-1");

            // The refused changes were discarded, not saved alongside the row: the stored links are the first open's.
            var campaign = await read.MsfCampaigns
                .AsNoTracking()
                .Include(entity => entity.Invitations)
                .SingleAsync(entity => entity.Id == campaignId);

            campaign.State.Should().Be(MsfCampaignState.Open);
            foreach (var message in first.Sent)
            {
                var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                _tokens.VerifyToken(TokenIn(message), stored.TokenHash).Should().BeTrue();
            }

            foreach (var message in second.Sent)
            {
                var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                _tokens.VerifyToken(TokenIn(message), stored.TokenHash).Should().BeFalse();
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// A dozen Create/Update handlers catch the <see cref="DbUpdateException" /> of a unique-index violation and throw
    /// an <see cref="InvalidOperationException" /> that says what clashed. The refused insert is still tracked, so until
    /// T201 the audit write re-sent it and the admin saw EF's message instead of that one.
    /// </summary>
    [Fact]
    public async Task AUniqueViolationTheHandlerTranslates_LeavesAFailureAuditRow_AndSurfacesTheTranslation()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var shortCode = $"T201-{suffix}";

            var created = new HandlerOutcome();
            await CreateInstitutionThroughTheAuditPipelineAsync(schema, $"T201 first {suffix}", shortCode, created);
            created.Thrown.Should().BeNull();

            var duplicate = new HandlerOutcome();
            var createDuplicate = () =>
                CreateInstitutionThroughTheAuditPipelineAsync(schema, $"T201 second {suffix}", shortCode, duplicate);

            var thrown = (await createDuplicate.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(duplicate.Thrown);
            thrown.Message.Should().Be("An institution with the same name or short code already exists.");
            thrown.InnerException.Should().BeAssignableTo<DbUpdateException>("the handler translated a refused save");

            await using var read = NewContext(schema);
            var rows = await read.AuditEntries
                .AsNoTracking()
                .Where(entry => entry.Action == nameof(CreateInstitutionCommand))
                .OrderBy(entry => entry.OccurredAt)
                .ToListAsync();

            rows.Select(entry => entry.Success).Should().Equal(true, false);
            rows[1].ErrorMessage.Should().Be("An institution with the same name or short code already exists.");

            (await read.Institutions.CountAsync(institution => institution.ShortCode == shortCode)).Should().Be(1);
            (await read.Institutions.AnyAsync(institution => institution.Name == $"T201 second {suffix}"))
                .Should().BeFalse("the refused insert was discarded, not saved with the audit row");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// Why a save the handler reports refused is discarded outright, and not left to the ordinary write, which now falls
    /// back when the database refuses it: a refusal need not repeat. Here the clashing institution is deleted between the
    /// handler's refused insert and the audit write. Re-sent, the insert would now succeed, and the row recording its
    /// failure would commit it.
    /// </summary>
    [Fact]
    public async Task ARefusedInsertThatWouldNowSucceed_IsNotCommittedByItsFailureRow()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var shortCode = $"T201-{suffix}";

            var created = new HandlerOutcome();
            await CreateInstitutionThroughTheAuditPipelineAsync(schema, $"T201 first {suffix}", shortCode, created);
            created.Thrown.Should().BeNull();

            // Armed once the handler has thrown, so it runs inside the audit write's save.
            var race = new RaceBeforeFirstSave();
            var duplicate = new HandlerOutcome
            {
                OnThrown = () => race.Arm(async () =>
                {
                    await using var competing = NewContext(schema);
                    await competing.Institutions.Where(institution => institution.ShortCode == shortCode).ExecuteDeleteAsync();
                })
            };
            var createDuplicate = () => CreateInstitutionThroughTheAuditPipelineAsync(
                schema, $"T201 second {suffix}", shortCode, duplicate, race);

            (await createDuplicate.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(duplicate.Thrown);
            race.Ran.Should().BeTrue("the clashing institution must be gone before the audit write reaches the server");

            await using var read = NewContext(schema);
            var rows = await read.AuditEntries
                .AsNoTracking()
                .Where(entry => entry.Action == nameof(CreateInstitutionCommand))
                .OrderBy(entry => entry.OccurredAt)
                .ToListAsync();

            rows.Select(entry => entry.Success).Should().Equal(true, false);
            (await read.Institutions.AnyAsync(institution => institution.ShortCode == shortCode))
                .Should().BeFalse("the insert the handler reported refused must not be committed under its failure row");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// A save ASP.NET Identity refuses never reaches the pipeline as a <see cref="DbUpdateException" />: its user store
    /// catches the concurrency conflict and returns a failed result, which <see cref="UserAdministrationService" />
    /// throws as a plain <see cref="InvalidOperationException" /> with nothing inside it. The refused user update is
    /// still tracked all the same, so the ordinary write re-sent it, was refused again, and lost the row, and EF's second
    /// exception replaced Identity's message. Every command that saves through <c>UserManager</c> took this path: a
    /// profile edit, an admission, a role grant or removal, a password reset, a lock.
    /// </summary>
    [Fact]
    public async Task AConcurrencyConflictIdentitySwallows_LeavesAFailureAuditRow_AndSurfacesIdentitysError()
    {
        const string userId = "trainee-t201";

        try
        {
            var race = new RaceBeforeFirstSave();
            var schema = await CreateSchemaAsync();
            await using var root = await MigratedIdentityServicesAsync(schema, race);

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                NomineeSeed.AddUser(db, userId, institutionId: null, WombatRoles.Trainee);
                await db.SaveChangesAsync();
            }

            // An admin's edit of the same user commits between this request's read of the user and its save.
            race.Arm(async () =>
            {
                await using var competing = root.CreateAsyncScope();
                var users = competing.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                var user = await users.FindByIdAsync(userId);
                user!.FirstName = "Competing";
                (await users.UpdateAsync(user)).Succeeded.Should().BeTrue();
            });

            await using var request = root.CreateAsyncScope();
            var requestDb = request.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var command = new UpdateCurrentUserProfileCommand(
                new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test")),
                "Refused",
                "Edit");
            var handler = new UpdateCurrentUserProfileCommandHandler(new UserAdministrationService(
                request.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
                requestDb));
            var outcome = new HandlerOutcome();

            var update = () => new AuditPipelineBehavior<UpdateCurrentUserProfileCommand, UserProfileDto>(
                    new AuditWriter(requestDb),
                    new FixedAuditContext())
                .Handle(
                    command,
                    async () =>
                    {
                        UserProfileDto? profile = null;
                        await outcome.RecordAsync(async () => profile = await handler.Handle(command, CancellationToken.None));
                        return profile!;
                    },
                    CancellationToken.None);

            var thrown = (await update.Should().ThrowAsync<InvalidOperationException>()).Which;
            race.Ran.Should().BeTrue("the competing edit must have committed inside the request's save");
            thrown.Should().BeSameAs(outcome.Thrown, "the caller must see Identity's error, not the audit write's");
            thrown.Message.Should().Be("Optimistic concurrency failure, object has been modified.");
            thrown.InnerException.Should().BeNull("Identity swallowed the DbUpdateConcurrencyException: nothing says a save was refused");

            await using var read = NewContext(schema);
            var row = (await read.AuditEntries
                    .AsNoTracking()
                    .Where(entry => entry.Action == nameof(UpdateCurrentUserProfileCommand))
                    .ToListAsync())
                .Should().ContainSingle("the refused edit leaves its row").Subject;
            row.Success.Should().BeFalse();
            row.ErrorMessage.Should().Be(thrown.Message);

            var stored = await read.Users.AsNoTracking().SingleAsync(user => user.Id == userId);
            stored.FirstName.Should().Be("Competing", "the refused edit was discarded, not saved with the row");
            stored.LastName.Should().Be(userId);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private async Task OpenThroughTheAuditPipelineAsync(
        string schema, int campaignId, IEmailSender sender, HandlerOutcome outcome)
    {
        await using var db = NewContext(schema);
        var command = new OpenMsfCampaignCommand(campaignId, Administrator());
        var handler = new OpenMsfCampaignCommandHandler(
            db,
            sender,
            new InvitationTokenService(),
            new FakeUserDirectory(("trainee-1", "Thandi Nkosi")),
            Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }));

        await new AuditPipelineBehavior<OpenMsfCampaignCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await outcome.RecordAsync(() => handler.Handle(command, CancellationToken.None));
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    private async Task CreateInstitutionThroughTheAuditPipelineAsync(
        string schema, string name, string shortCode, HandlerOutcome outcome, IInterceptor? interceptor = null)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new CreateInstitutionCommand(name, shortCode, ContactEmail: null, Administrator());
        var handler = new CreateInstitutionCommandHandler(db);

        await new AuditPipelineBehavior<CreateInstitutionCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await outcome.RecordAsync(() => handler.Handle(command, CancellationToken.None));
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    /// <summary>The exception the handler itself threw, captured inside the pipeline, before the audit write runs.</summary>
    private sealed class HandlerOutcome
    {
        public Exception? Thrown { get; private set; }

        /// <summary>Runs once the handler has thrown, before the pipeline writes the failure row.</summary>
        public Action? OnThrown { get; init; }

        public async Task RecordAsync(Func<Task> handle)
        {
            try
            {
                await handle();
            }
            catch (Exception exception)
            {
                Thrown = exception;
                OnThrown?.Invoke();
                throw;
            }
        }
    }

    private static string TokenIn(EmailMessage message)
    {
        var url = message.TextBody
            .Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl, StringComparison.Ordinal));
        return HttpUtility.ParseQueryString(new Uri(url).Query)["token"]!;
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
            Template = new MsfTemplate { Name = "T201 MSF" },
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

    /// <summary>Records every send. The first send can run something first: here, the other open.</summary>
    private sealed class RecordingSender(Func<Task>? beforeFirstSend) : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (Sent.Count == 0 && beforeFirstSend is not null)
            {
                await beforeFirstSend();
            }

            Sent.Add(message);
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

    // ─── Schema helpers (as MsfOpenCampaignRacePostgresTests) ────────────────

    /// <summary>
    /// Runs something once, inside the first save made after it is armed, before that save's commands reach the server:
    /// a competing request's committed change, landing between a request's read and its write, or between a handler's
    /// refused save and the audit write.
    /// </summary>
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
            // Disarmed before it runs, so the competing request's own save passes straight through.
            var competing = Interlocked.Exchange(ref _armed, null);
            if (competing is not null)
            {
                await competing();
                Ran = true;
            }

            return result;
        }
    }

    /// <summary>A migrated schema behind the Identity stack a request resolves: UserManager on the scope's context.</summary>
    private async Task<ServiceProvider> MigratedIdentityServicesAsync(string schema, IInterceptor interceptor)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(SchemaConnectionString(schema))
            .AddInterceptors(interceptor));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var root = services.BuildServiceProvider();

        await using var migrationScope = root.CreateAsyncScope();
        await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

        return root;
    }

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
