using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Configurations.MultiSourceFeedback;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T228 on a real PostgreSQL server: a campaign invites an address once, in any capitals, whoever writes it and however
/// two adds race, and an erased address blocks nobody.
/// </summary>
/// <remarks>
/// <para>
/// The add command refuses an address its campaign already invites before it writes anything
/// (<c>MsfInvitationAddressOnceTests</c>). Two adds that both pass that check before either saves are what only a server
/// can show. Every add moves the campaign's <c>xmin</c> token (T206), but it is the unique index on the campaign and the
/// lower-cased address that refuses the second, and it holds for any writer, the token's or not. Each add runs inside the real
/// <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real <see cref="AuditWriter" /> on its own
/// context, as a request would, so a refused add's pending insert meets the audit trap too (T201).
/// </para>
/// <para>
/// Isolated as <c>MsfInviteDuringOpenRacePostgresTests</c> is: a schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class MsfInvitationAddressOncePostgresTests : IAsyncLifetime
{
    private const string T228MigrationSuffix = "_T228_MsfInvitationAddressOnce";
    private const string UniqueViolation = "23505";
    private const string Invited = "peer-9@example.test";

    private readonly TestSchemas _schemas = new();
    private readonly InvitationTokenService _tokens = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TwoAddsOfOneAddress_RacingToTheSave_LeaveOneInvitation()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The first add has read the draft and found the address not yet invited; just before its save, the other
            // tab's add of the same address, in other capitals, runs from start to save.
            var race = new RaceBeforeFirstSave();
            race.Arm(() => AddThroughTheAuditPipelineAsync(schema, campaignId, "PEER-9@Example.Test"));

            // EF sends the campaign's update and the invitation's insert as one batch, the update first. The update finds
            // the token moved and changes nothing, but the save raises the server's refusal of the insert, so the add
            // that lost is told what it lost to: the address, not a changed campaign.
            var first = () => AddThroughTheAuditPipelineAsync(schema, campaignId, Invited, race);
            (await first.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(AddMsfInvitationCommandHandler.AlreadyInvited)
                .WithInnerException<DbUpdateException>()
                .Which.InnerException.Should().BeOfType<PostgresException>()
                .Which.ConstraintName.Should().Be(MsfInvitationConfiguration.RespondentEmailKeyIndex);

            race.Ran.Should().BeTrue("guard: the second add ran between the first's check and its save");

            await using var read = NewContext(schema);
            (await read.MsfInvitations.AsNoTracking().Where(invitation => invitation.CampaignId == campaignId)
                    .Select(invitation => invitation.RespondentEmail).ToListAsync())
                .Should().Equal(["PEER-9@Example.Test"], "one invitation, the add that saved first");

            (await AddAuditOutcomesAsync(read)).Should().BeEquivalentTo(
                [true, false], "the refused add leaves its failure row, written alone, and its insert is not sent again (T201)");

            // Tried again, the add is refused by the check, before anything is sent.
            var again = () => AddThroughTheAuditPipelineAsync(schema, campaignId, Invited);
            (await again.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(AddMsfInvitationCommandHandler.AlreadyInvited)
                .Which.InnerException.Should().BeNull();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AnAddressStoredBetweenTheCheckAndTheSave_ByAWriterThatLeavesTheCampaignAlone_IsRefusedByTheIndex()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // A writer the xmin token does not see: it stores the address and never touches the campaign's row. Only the
            // index can refuse the add that follows it.
            var race = new RaceBeforeFirstSave();
            race.Arm(async () =>
            {
                await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
                await InsertInvitationAsync(connection, campaignId, "Peer-9@EXAMPLE.test");
            });

            var add = () => AddThroughTheAuditPipelineAsync(schema, campaignId, Invited, race);
            (await add.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(AddMsfInvitationCommandHandler.AlreadyInvited)
                .WithInnerException<DbUpdateException>()
                .Which.InnerException.Should().BeOfType<PostgresException>()
                .Which.ConstraintName.Should().Be(MsfInvitationConfiguration.RespondentEmailKeyIndex);

            race.Ran.Should().BeTrue("guard: the other writer ran between the add's check and its save");

            await using var read = NewContext(schema);
            (await read.MsfInvitations.AsNoTracking().Where(invitation => invitation.CampaignId == campaignId)
                    .Select(invitation => invitation.RespondentEmail).ToListAsync())
                .Should().Equal(["Peer-9@EXAMPLE.test"], "the refused add stored nothing");

            (await AddAuditOutcomesAsync(read)).Should().Equal(
                [false], "the failure row is stored, and the refused insert was not sent again with it (T201)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheIndex_HoldsOneAddressPerCampaign_InAnyCapitals_AndLetsAnErasedAddressGo()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);
            var otherCampaignId = await SeedDraftCampaignAsync(schema);

            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
            var firstId = await InsertInvitationAsync(connection, campaignId, Invited);

            (await KeyOfAsync(schema, firstId)).Should().Be(Invited, "the key is the address, lower-cased by the database");

            var sameAddress = () => InsertInvitationAsync(connection, campaignId, "PEER-9@example.test");
            (await sameAddress.Should().ThrowAsync<PostgresException>())
                .Which.Should().Match<PostgresException>(refusal =>
                    refusal.SqlState == UniqueViolation &&
                    refusal.ConstraintName == MsfInvitationConfiguration.RespondentEmailKeyIndex);

            await InsertInvitationAsync(connection, otherCampaignId, Invited);
            await InsertInvitationAsync(connection, campaignId, null);
            await InsertInvitationAsync(connection, campaignId, null);

            // Anonymised as closing and withdrawing do it (MsfInvitation.Anonymize, T207): the address goes, and the key
            // goes with it in the same update, so nothing derived from the address is left and it blocks nobody.
            await using (var db = NewContext(schema))
            {
                var invitation = await db.MsfInvitations.SingleAsync(candidate => candidate.Id == firstId);
                invitation.Anonymize(DateTime.UtcNow);
                await db.SaveChangesAsync();
            }

            (await KeyOfAsync(schema, firstId)).Should().BeNull("the key is erased with the address");
            await InsertInvitationAsync(connection, campaignId, Invited);

            (await ScalarAsync(schema, """SELECT COUNT(*) FROM "MsfInvitations" """)).Should().Be(5L);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheMigration_KeepsOneInvitationPerAddress_TheOneThatResponded_ElseTheFirst_AndDownDropsTheKey()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            string predecessor;
            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t228 = migrations.FindIndex(migration => migration.EndsWith(T228MigrationSuffix, StringComparison.Ordinal));
                t228.Should().BePositive("guard: the T228 migration is in the assembly, after at least one other");
                predecessor = migrations[t228 - 1];
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            // As the product stored them before T228: dev campaign 11 held two addresses invited twice each.
            int campaign, other, unansweredCopy, answeredCopy, firstOfTwo, secondOfTwo, solo, erasedOne, erasedTwo, elsewhere, response;
            int firstAnswered, secondAnswered, firstAnsweredResponse, secondAnsweredResponse;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                campaign = await InsertCampaignAsync(connection);
                other = await InsertCampaignAsync(connection);

                unansweredCopy = await InsertInvitationAsync(connection, campaign, "dup@example.test");
                answeredCopy = await InsertInvitationAsync(connection, campaign, "DUP@example.test", respondedOn: DateTime.UtcNow);
                response = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfResponses" ("CampaignId", "InvitationId", "SubmittedOn") VALUES ($1, $2, now()) RETURNING "Id"
                    """,
                    campaign, answeredCopy);
                firstOfTwo = await InsertInvitationAsync(connection, campaign, "twice@example.test");
                secondOfTwo = await InsertInvitationAsync(connection, campaign, "Twice@Example.test");
                solo = await InsertInvitationAsync(connection, campaign, "solo@example.test");
                erasedOne = await InsertInvitationAsync(connection, campaign, null);
                erasedTwo = await InsertInvitationAsync(connection, campaign, null);
                elsewhere = await InsertInvitationAsync(connection, other, "dup@example.test");

                // One person who answered through both links: the first answer is kept, and the second goes with its copy.
                firstAnswered = await InsertInvitationAsync(connection, campaign, "both@example.test", respondedOn: DateTime.UtcNow);
                secondAnswered = await InsertInvitationAsync(connection, campaign, "Both@Example.test", respondedOn: DateTime.UtcNow);
                firstAnsweredResponse = await InsertResponseAsync(connection, campaign, firstAnswered);
                secondAnsweredResponse = await InsertResponseAsync(connection, campaign, secondAnswered);
            }

            await MigrateToLatestAsync(schema);

            await using (var read = NewContext(schema))
            {
                var kept = await read.MsfInvitations.AsNoTracking().Select(invitation => invitation.Id).ToListAsync();
                kept.Should().BeEquivalentTo(
                    [answeredCopy, firstOfTwo, solo, erasedOne, erasedTwo, elsewhere, firstAnswered],
                    "the copy that responded is kept over the first; of two unanswered, or two answered, the first; erased and other campaigns' rows are no one's duplicates");
                kept.Should().NotContain([unansweredCopy, secondOfTwo, secondAnswered]);
                (await read.MsfResponses.AsNoTracking().Select(row => row.Id).ToListAsync())
                    .Should().BeEquivalentTo(
                        [response, firstAnsweredResponse],
                        "a kept copy's response stays, and a deleted copy's second response goes with it");
                secondAnsweredResponse.Should().NotBe(firstAnsweredResponse, "guard: two responses were stored");
            }

            (await Catalog.IndexIsUniqueAsync(schema, MsfInvitationConfiguration.RespondentEmailKeyIndex)).Should().BeTrue();

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            (await ScalarAsync(schema,
                    """
                    SELECT COUNT(*) FROM information_schema.columns
                    WHERE table_schema = current_schema() AND table_name = 'MsfInvitations' AND column_name = $1
                    """,
                    MsfInvitationConfiguration.RespondentEmailKey))
                .Should().Be(0L, "Down drops the key, and its index with it");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The command, as a request runs it ───────────────────────────────────

    private async Task AddThroughTheAuditPipelineAsync(
        string schema, int campaignId, string email, IInterceptor? interceptor = null)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.PeerDoctor, Administrator());
        var handler = new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees("trainee-1"));

        await new AuditPipelineBehavior<AddMsfInvitationCommand, int>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    private static Task<List<bool>> AddAuditOutcomesAsync(ApplicationDbContext read)
        => read.AuditEntries.AsNoTracking()
            .Where(entry => entry.Action == nameof(AddMsfInvitationCommand))
            .Select(entry => entry.Success)
            .ToListAsync();

    private async Task<int> SeedDraftCampaignAsync(string schema)
    {
        await using var db = NewContext(schema);
        await CurrentTraineeSeed.AdmitAsync(db, "trainee-1");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today,
            ClosesOn = today.AddDays(14),
            State = MsfCampaignState.Draft,
            Template = new MsfTemplate { Name = "T228 MSF" }
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    /// <summary>A campaign written as SQL, so that it can be stored on a schema older than the model.</summary>
    private static async Task<int> InsertCampaignAsync(NpgsqlConnection connection)
    {
        var template = await InsertAsync(connection,
            """
            INSERT INTO "MsfTemplates" ("Name", "IsActive", "AllowPatientResponses", "Kind")
            VALUES ('T228 MSF', TRUE, FALSE, 0) RETURNING "Id"
            """);
        return await InsertAsync(connection,
            """
            INSERT INTO "MsfCampaigns"
                ("SubjectUserId", "TemplateId", "CreatedByUserId", "CreatedOn", "OpensOn", "ClosesOn", "State",
                 "MinimumResponses", "MinimumRespondentCategories", "MinimumCategoryResponses")
            VALUES ('trainee-1', $1, 'coordinator-1', now(), CURRENT_DATE - 1, CURRENT_DATE + 3, $2, 0, 0, 0)
            RETURNING "Id"
            """,
            template, (int)MsfCampaignState.Open);
    }

    /// <summary>A response written as SQL, on a schema older than the model.</summary>
    private static Task<int> InsertResponseAsync(NpgsqlConnection connection, int campaignId, int invitationId)
        => InsertAsync(connection,
            """
            INSERT INTO "MsfResponses" ("CampaignId", "InvitationId", "SubmittedOn") VALUES ($1, $2, now()) RETURNING "Id"
            """,
            campaignId, invitationId);

    /// <summary>An invitation written as SQL: a writer that never touches its campaign's row.</summary>
    private Task<int> InsertInvitationAsync(
        NpgsqlConnection connection, int campaignId, string? email, DateTime? respondedOn = null)
        => InsertAsync(connection,
            """
            INSERT INTO "MsfInvitations"
                ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenHash", "IssuedOn", "ExpiresOn", "RespondedOn",
                 "AnonymizedOn")
            VALUES ($1, $2, $3, $4, now(), CURRENT_DATE + 10, $5, $6)
            RETURNING "Id"
            """,
            campaignId,
            (object?)email ?? DBNull.Value,
            (int)MsfRespondentCategory.Nurse,
            _tokens.HashToken(_tokens.GenerateToken()),
            (object?)respondedOn ?? DBNull.Value,
            email is null ? DateTime.UtcNow : DBNull.Value);

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>Runs the competing writer once, just before the first save on the context it is attached to.</summary>
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

    // ─── SQL helpers ─────────────────────────────────────────────────────────

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private async Task<object?> ScalarAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        var result = await command.ExecuteScalarAsync();
        return result is DBNull ? null : result;
    }

    /// <summary>The database's key for one invitation: its address, lower-cased, or null once the address is erased.</summary>
    private async Task<string?> KeyOfAsync(string schema, int invitationId)
        => (string?)await ScalarAsync(
            schema,
            $"""SELECT "{MsfInvitationConfiguration.RespondentEmailKey}" FROM "MsfInvitations" WHERE "Id" = $1""",
            invitationId);

    // ─── Schema helpers (as MsfInviteDuringOpenRacePostgresTests) ────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();
        await MigrateToLatestAsync(schema);
        return schema;
    }

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    private ApplicationDbContext NewContext(string schema, IInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
        }

        return new ApplicationDbContext(options.Options);
    }
}
