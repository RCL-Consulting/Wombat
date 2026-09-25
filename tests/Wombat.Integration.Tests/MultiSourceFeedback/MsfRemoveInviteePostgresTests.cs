using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using System.Web;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T247 on a real PostgreSQL server: a draft's invitee is removed by a real delete, which frees their address for the
/// campaign again; a remove and an open that race meet at the campaign's <c>xmin</c> token, so an invitee who has been
/// mailed a link is never removed, and an open never stores a link for an invitee removed under it; and the campaign page
/// lists no address for a campaign that opened after the page first read it as a draft.
/// </summary>
/// <remarks>
/// <para>
/// Removing an invitee writes the campaign row as well as deleting the invitation, as adding one does (T206), so the save
/// is checked against the token. An invitation's responses go with it (<c>MsfResponseConfiguration</c>), which is why the
/// draft rule has to hold at the save and not only when the campaign was read. EF InMemory has no <c>xmin</c>, so only
/// Postgres can show this.
/// </para>
/// <para>
/// Each command runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real
/// <see cref="AuditWriter" /> on its own context, as a request would, so a refused command's pending changes meet the
/// audit trap too. The schema helpers follow <c>MsfInviteDuringOpenRacePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class MsfRemoveInviteePostgresTests : IAsyncLifetime
{
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string Mistake = "nurse-1@example.test";

    private static readonly string[] Respondents =
    [
        Mistake,
        "nurse-2@example.test",
        "consultant-1@example.test"
    ];

    private readonly TestSchemas _schemas = new();
    private readonly InvitationTokenService _tokens = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task ADraftsInvitee_IsDeleted_AndTheirAddressCanBeInvitedAgain()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);
            var mistakeId = await InvitationIdAsync(schema, campaignId, Mistake);

            await RemoveThroughTheAuditPipelineAsync(schema, campaignId, mistakeId);

            await using (var read = NewContext(schema))
            {
                var campaign = await ReadCampaignAsync(read, campaignId);
                campaign.State.Should().Be(MsfCampaignState.Draft);
                campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                    .Should().BeEquivalentTo(Respondents.Except([Mistake]));

                var row = await read.AuditEntries.AsNoTracking()
                    .SingleAsync(entry => entry.Action == nameof(RemoveMsfInvitationCommand));
                row.Success.Should().BeTrue();
                row.SummaryJson.Should().NotContain("@", "the row names no respondent (T205)");
            }

            // The unique index on the campaign and the address (T228) no longer holds it.
            await AddThroughTheAuditPipelineAsync(schema, campaignId, Mistake.ToUpperInvariant());

            await using (var read = NewContext(schema))
            {
                (await ReadCampaignAsync(read, campaignId)).Invitations.Should().HaveCount(Respondents.Length);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AnOpenThatCommitsWhileAnInviteeIsBeingRemoved_RefusesTheRemove_AndTheInviteeKeepsTheirLink()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);
            var mistakeId = await InvitationIdAsync(schema, campaignId, Mistake);

            // The remove has read the draft and found the invitee; just before its save, the other tab's open runs from
            // start to save, and mails every invitee, the one being removed included.
            var open = new RecordingSender(beforeFirstSend: null);
            var raceBeforeTheRemovesSave = new RaceBeforeFirstSave();
            raceBeforeTheRemovesSave.Arm(() => OpenThroughTheAuditPipelineAsync(schema, campaignId, open));

            var remove = () => RemoveThroughTheAuditPipelineAsync(schema, campaignId, mistakeId, raceBeforeTheRemovesSave);
            (await remove.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(RemoveMsfInvitationCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the open changed the campaign's xmin under the remove");

            raceBeforeTheRemovesSave.Ran.Should().BeTrue("guard: the open ran between the remove's read and its save");
            open.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents);

            await using var read = NewContext(schema);
            var campaign = await ReadCampaignAsync(read, campaignId);
            campaign.State.Should().Be(MsfCampaignState.Open);
            campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                .Should().BeEquivalentTo(Respondents, "the refused remove deleted nobody who has been mailed a link");

            var mailed = open.Sent.Single(message => message.To == Mistake);
            _tokens.VerifyToken(TokenIn(mailed), campaign.Invitations.Single(invitation => invitation.Id == mistakeId).TokenHash)
                .Should().BeTrue("the link they were mailed still names their invitation");

            (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(RemoveMsfInvitationCommand))
                    .Select(entry => entry.Success)
                    .ToListAsync())
                .Should().Equal([false], "the refused remove leaves its failure row, written alone (T201)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ARemoveThatCommitsWhileAnOpenMails_RefusesTheOpen_AndOpeningAgainMailsOnlyThoseLeft()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);
            var mistakeId = await InvitationIdAsync(schema, campaignId, Mistake);

            // The open has read the three invitations; at its first send, the other tab's remove runs from start to save.
            var firstOpen = new RecordingSender(
                beforeFirstSend: () => RemoveThroughTheAuditPipelineAsync(schema, campaignId, mistakeId));

            var open = () => OpenThroughTheAuditPipelineAsync(schema, campaignId, firstOpen);
            (await open.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(OpenMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the remove changed the campaign's xmin under the open");

            await using (var read = NewContext(schema))
            {
                var campaign = await ReadCampaignAsync(read, campaignId);
                campaign.State.Should().Be(MsfCampaignState.Draft, "the refused open stored nothing");
                campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                    .Should().BeEquivalentTo(Respondents.Except([Mistake]), "the remove committed");
                campaign.Invitations.Should().OnlyContain(invitation => invitation.TokenSelector == null,
                    "no link the refused open mailed was stored, so none of them, the removed invitee's included, works");
            }

            var secondOpen = new RecordingSender(beforeFirstSend: null);
            await OpenThroughTheAuditPipelineAsync(schema, campaignId, secondOpen);

            secondOpen.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents.Except([Mistake]));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ACampaignOpenedAfterThePageReadItAsADraft_ListsNoAddress()
    {
        // The setup query reads the campaign, then counts its invitations, then, for a campaign it read as a draft, reads
        // the addresses in a statement that asks the campaign row again. An open that commits after the first read leaves
        // a page that shows the campaign as the draft it read, with its counts and without a single address.
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            var openBeforeTheInvitationRead = new BeforeTheInvitationRead(async () =>
            {
                await using var other = NewContext(schema);
                var campaign = await other.MsfCampaigns.SingleAsync(entity => entity.Id == campaignId);
                campaign.State = MsfCampaignState.Open;
                await other.SaveChangesAsync();
            });

            MsfCampaignSetupDto? setup;
            await using (var db = NewContext(schema, openBeforeTheInvitationRead))
            {
                setup = await new GetMsfCampaignSetupQueryHandler(db, FakeUserDirectory.Empty)
                    .Handle(new GetMsfCampaignSetupQuery(campaignId, Administrator()), CancellationToken.None);
            }

            openBeforeTheInvitationRead.Ran.Should().BeTrue("guard: the open ran between the two reads");
            setup!.State.Should().Be(MsfCampaignState.Draft, "the first read found a draft");
            setup.Invitees.Sum(group => group.Invited).Should().Be(Respondents.Length);
            setup.DraftInvitees.Should().BeEmpty();
            JsonSerializer.Serialize(setup).Should().NotContain("@example.test");

            // Guard: without the race, the same draft lists every address.
            await using (var db = NewContext(schema))
            {
                var campaign = await db.MsfCampaigns.SingleAsync(entity => entity.Id == campaignId);
                campaign.State = MsfCampaignState.Draft;
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var draft = await new GetMsfCampaignSetupQueryHandler(db, FakeUserDirectory.Empty)
                    .Handle(new GetMsfCampaignSetupQuery(campaignId, Administrator()), CancellationToken.None);
                draft!.DraftInvitees.Select(invitee => invitee.Email).Should().Equal(Respondents);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private async Task RemoveThroughTheAuditPipelineAsync(
        string schema, int campaignId, int invitationId, IInterceptor? interceptor = null)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new RemoveMsfInvitationCommand(campaignId, invitationId, Administrator());
        var handler = new RemoveMsfInvitationCommandHandler(db);

        await new AuditPipelineBehavior<RemoveMsfInvitationCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await handler.Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    private async Task AddThroughTheAuditPipelineAsync(string schema, int campaignId, string email)
    {
        await using var db = NewContext(schema);
        var command = new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.Nurse, Administrator());
        var handler = new AddMsfInvitationCommandHandler(db, new InvitationTokenService());

        await new AuditPipelineBehavior<AddMsfInvitationCommand, int>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    private async Task OpenThroughTheAuditPipelineAsync(string schema, int campaignId, IEmailSender sender)
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
                    await handler.Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    private static Task<MsfCampaign> ReadCampaignAsync(ApplicationDbContext read, int campaignId)
        => read.MsfCampaigns
            .AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

    private async Task<int> InvitationIdAsync(string schema, int campaignId, string email)
    {
        await using var read = NewContext(schema);
        return await read.MsfInvitations.AsNoTracking()
            .Where(invitation => invitation.CampaignId == campaignId && invitation.RespondentEmail == email)
            .Select(invitation => invitation.Id)
            .SingleAsync();
    }

    private static string TokenIn(EmailMessage message)
    {
        var url = message.TextBody
            .Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl, StringComparison.Ordinal));
        return HttpUtility.ParseQueryString(new Uri(url).Query)["token"]!;
    }

    /// <summary>A draft inviting <see cref="Respondents" />, in that order.</summary>
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
            Template = new MsfTemplate { Name = "T247 MSF" }
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();

        // One at a time, so the ids follow the list's order, as the page lists them.
        foreach (var email in Respondents)
        {
            db.MsfInvitations.Add(new MsfInvitation
            {
                CampaignId = campaign.Id,
                RespondentEmail = email,
                RespondentCategory = email.StartsWith("nurse", StringComparison.Ordinal)
                    ? MsfRespondentCategory.Nurse
                    : MsfRespondentCategory.Consultant,
                TokenHash = _tokens.HashToken(_tokens.GenerateToken()),
                IssuedOn = DateTime.UtcNow,
                ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(21)
            });
            await db.SaveChangesAsync();
        }

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

    /// <summary>Records every send. The first send can run something first: here, the other tab's remove.</summary>
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

    /// <summary>Runs the competing write once, just before the first statement that reads the invitations.</summary>
    private sealed class BeforeTheInvitationRead(Func<Task> competing) : DbCommandInterceptor
    {
        private Func<Task>? _armed = competing;

        public bool Ran { get; private set; }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("\"MsfInvitations\"", StringComparison.Ordinal) &&
                Interlocked.Exchange(ref _armed, null) is { } write)
            {
                await write();
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
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
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
