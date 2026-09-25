using System.Security.Claims;
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
/// T206 on a real PostgreSQL server: an invitee added from another tab while an open is running is either mailed or
/// refused, never stored on an open campaign that did not mail them.
/// </summary>
/// <remarks>
/// <para>
/// Opening reads the invitations, mails each one, then stores the new token hashes and the open in one save under the
/// campaign's <c>xmin</c> token (T184). Adding an invitee used to insert its row and nothing else, so an add that
/// committed while an open was mailing left the campaign open with an invitee nobody had written to, and there is no
/// resend. Adding an invitee now writes the campaign row too, so the two meet at the token, whichever saves first:
/// </para>
/// <list type="bullet">
/// <item>The add commits during the open: the open is refused, the campaign stays a draft with the new invitee, and
/// opening it again mails everyone, the new invitee included.</item>
/// <item>The open commits during the add: the add is refused and nothing of it is stored.</item>
/// </list>
/// <para>
/// EF InMemory has no <c>xmin</c>, so only Postgres can show this. Each command runs inside the real
/// <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real <see cref="AuditWriter" /> on its own
/// context, as a request would, so a refused command's pending changes meet the audit trap too. The schema helpers
/// follow <c>MsfOpenCampaignRacePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class MsfInviteDuringOpenRacePostgresTests : IAsyncLifetime
{
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string LateInvitee = "peer-late@example.test";

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "nurse-2@example.test",
        "consultant-1@example.test"
    ];

    private readonly TestSchemas _schemas = new();
    private readonly InvitationTokenService _tokens = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task AnInviteeAddedWhileAnOpenMails_RefusesTheOpen_AndOpeningAgainMailsThemToo()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The open has read the three invitations; at its first send, the other tab's add runs from start to save.
            var firstOpen = new RecordingSender(
                beforeFirstSend: () => AddThroughTheAuditPipelineAsync(schema, campaignId, LateInvitee));

            var open = () => OpenThroughTheAuditPipelineAsync(schema, campaignId, firstOpen);
            (await open.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(OpenMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the add changed the campaign's xmin under the open");

            firstOpen.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents, "the open had read three");

            await using (var read = NewContext(schema))
            {
                var campaign = await ReadCampaignAsync(read, campaignId);
                campaign.State.Should().Be(MsfCampaignState.Draft, "the refused open stored nothing");
                campaign.OpenedOn.Should().BeNull();
                campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                    .Should().BeEquivalentTo([.. Respondents, LateInvitee], "the add committed");

                foreach (var message in firstOpen.Sent)
                {
                    var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                    _tokens.VerifyToken(TokenIn(message), stored.TokenHash)
                        .Should().BeFalse($"the refused open must not have stored {message.To}'s link");
                }

                (await read.AuditEntries.AsNoTracking()
                        .Where(entry => entry.Action == nameof(OpenMsfCampaignCommand))
                        .Select(entry => entry.Success)
                        .ToListAsync())
                    .Should().Equal([false], "the refused open leaves its failure row, written alone (T201)");
            }

            // What the refusal asks the coordinator to do.
            var secondOpen = new RecordingSender(beforeFirstSend: null);
            await OpenThroughTheAuditPipelineAsync(schema, campaignId, secondOpen);

            secondOpen.Sent.Select(message => message.To).Should().BeEquivalentTo([.. Respondents, LateInvitee]);

            await using (var read = NewContext(schema))
            {
                var campaign = await ReadCampaignAsync(read, campaignId);
                campaign.State.Should().Be(MsfCampaignState.Open);

                foreach (var message in secondOpen.Sent)
                {
                    var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                    _tokens.VerifyToken(TokenIn(message), stored.TokenHash)
                        .Should().BeTrue($"{message.To}'s link from the open that saved must work");
                }
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AnOpenThatCommitsWhileAnInviteeIsBeingAdded_RefusesTheAdd_AndStoresNothingOfIt()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The add has read the draft and passed its checks; just before its save, the other tab's open runs from
            // start to save.
            var open = new RecordingSender(beforeFirstSend: null);
            var raceBeforeTheAddsSave = new RaceBeforeFirstSave();
            raceBeforeTheAddsSave.Arm(() => OpenThroughTheAuditPipelineAsync(schema, campaignId, open));

            var add = () => AddThroughTheAuditPipelineAsync(schema, campaignId, LateInvitee, raceBeforeTheAddsSave);
            (await add.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(AddMsfInvitationCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the open changed the campaign's xmin under the add");

            raceBeforeTheAddsSave.Ran.Should().BeTrue("guard: the open ran between the add's read and its save");
            open.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents);

            await using var read = NewContext(schema);
            var campaign = await ReadCampaignAsync(read, campaignId);

            campaign.State.Should().Be(MsfCampaignState.Open);
            campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                .Should().BeEquivalentTo(Respondents, "the refused add stored no invitation, and none nobody has mailed");

            foreach (var message in open.Sent)
            {
                var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                _tokens.VerifyToken(TokenIn(message), stored.TokenHash).Should().BeTrue();
            }

            var rows = await read.AuditEntries.AsNoTracking()
                .Where(entry => entry.Action == nameof(AddMsfInvitationCommand))
                .ToListAsync();
            rows.Should().ContainSingle().Which.Success.Should().BeFalse("the refused add leaves its failure row, written alone (T201)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private async Task OpenThroughTheAuditPipelineAsync(string schema, int campaignId, IEmailSender sender)
    {
        await using var db = NewContext(schema);
        var command = new OpenMsfCampaignCommand(campaignId, Administrator());
        var handler = new OpenMsfCampaignCommandHandler(
            db,
            sender,
            new InvitationTokenService(),
            new FakeUserDirectory(("trainee-1", "Thandi Nkosi")).WithTrainees("trainee-1"),
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

    private async Task AddThroughTheAuditPipelineAsync(
        string schema, int campaignId, string email, IInterceptor? interceptor = null)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.PeerDoctor, Administrator());
        var handler = new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees("trainee-1"));

        await new AuditPipelineBehavior<AddMsfInvitationCommand, int>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    private static Task<MsfCampaign> ReadCampaignAsync(ApplicationDbContext read, int campaignId)
        => read.MsfCampaigns
            .AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

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
        await CurrentTraineeSeed.AdmitAsync(db, "trainee-1");
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            State = MsfCampaignState.Draft,
            Template = new MsfTemplate { Name = "T206 MSF" },
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

    /// <summary>Records every send. The first send can run something first: here, the other tab's add.</summary>
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
