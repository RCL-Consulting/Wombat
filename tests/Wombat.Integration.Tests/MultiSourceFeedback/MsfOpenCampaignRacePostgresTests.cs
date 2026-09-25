using System.Security.Claims;
using System.Web;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
/// T184 on a real PostgreSQL server: when two opens of one draft campaign race, the first to save wins, and the links
/// it mailed are the ones stored.
/// </summary>
/// <remarks>
/// <para>
/// Opening mails every respondent before the campaign is touched, then assigns the new token hashes and opens the
/// campaign in one save. Two opens that race (a double-click on the editor's button can) both read a draft, both pass
/// <see cref="MsfCampaign.EnsureCanOpen" />, and both mail. What keeps the first round of links alive is that the hashes
/// travel in the same save as the campaign, under its <c>xmin</c> token: the second save is refused whole. Were the
/// hashes saved on their own, the loser would overwrite the winner's, and every link the open that "succeeded" sent
/// would be dead. EF InMemory has no <c>xmin</c>, so only Postgres can show this.
/// </para>
/// <para>
/// Each open runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real
/// <see cref="AuditWriter" /> on its own context, as a request would. The schema helpers follow
/// <c>MsfCampaignScopePostgresTests</c>: a schema of its own, dropped in a <c>finally</c> and again in
/// <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class MsfOpenCampaignRacePostgresTests : IAsyncLifetime
{
    private const string RespondUrl = "https://wombat.example/msf/respond";

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
    public async Task TwoOpensThatRace_TheFirstToSaveWins_AndOnlyTheLinksItMailedWork()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The second open reads the draft, and at its first send the first open runs from start to save.
            var first = new RecordingSender(beforeFirstSend: null);
            var second = new RecordingSender(beforeFirstSend: () => OpenThroughTheAuditPipelineAsync(schema, campaignId, first));

            var secondOpen = () => OpenThroughTheAuditPipelineAsync(schema, campaignId, second);
            (await secondOpen.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(OpenMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>(
                    "the campaign's xmin changed under the second open when the first one saved (T206 names the refusal)");

            // Both mailed everyone: the race is not prevented, only made safe for the links already stored.
            first.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents);
            second.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents);

            await using var read = NewContext(schema);
            var campaign = await read.MsfCampaigns
                .AsNoTracking()
                .Include(entity => entity.Invitations)
                .SingleAsync(entity => entity.Id == campaignId);

            campaign.State.Should().Be(MsfCampaignState.Open);
            campaign.OpenedOn.Should().NotBeNull();

            foreach (var message in first.Sent)
            {
                var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                _tokens.VerifyToken(TokenIn(message), stored.TokenHash)
                    .Should().BeTrue($"{message.To}'s link from the open that saved must work");
            }

            foreach (var message in second.Sent)
            {
                var stored = campaign.Invitations.Single(invitation => invitation.RespondentEmail == message.To);
                _tokens.VerifyToken(TokenIn(message), stored.TokenHash)
                    .Should().BeFalse($"the refused open must not have replaced {message.To}'s stored link");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The open, as a request runs it ──────────────────────────────────────

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
            Template = new MsfTemplate { Name = "T184 MSF" },
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

    // ─── Schema helpers (as MsfCampaignScopePostgresTests) ───────────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
