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
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Opening a campaign mails every respondent before it touches the campaign, so a send that fails leaves a draft that
/// can be opened again, not a half-opened campaign. (T184)
/// </summary>
/// <remarks>
/// <para>
/// The audit pipeline saves the request's DbContext from its catch, so whatever a handler had pending when it threw is
/// committed. Until T184 the handler opened the campaign first and rotated each token just before its own mail: a send
/// that threw committed an open campaign in which every respondent after the failure held no link, and since Open()
/// refuses a campaign that is not a draft, nothing could put that right.
/// </para>
/// <para>
/// Each command here runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the
/// real <see cref="AuditWriter" /> on the handler's own context, which is the path that commits. The test then saves
/// once more, clears the tracker and reads the campaign back, so nothing tracked can mask what was stored.
/// </para>
/// </remarks>
public sealed class MsfOpenCampaignSendFailureTests
{
    private const string RespondUrl = "https://wombat.example/msf/respond";

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "nurse-2@example.test",
        "consultant-1@example.test"
    ];

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly InvitationTokenService _tokens = new();

    [Fact]
    public async Task AFailedSend_LeavesTheCampaignUnopened_WithNoTokenRotated()
    {
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Draft);
        var before = await ReadAsync(campaignId);
        var sender = new ScriptedEmailSender(failOnSend: 2);

        await using var db = CreateDb();
        var refusal = await Record.ExceptionAsync(() => OpenThroughTheAuditPipelineAsync(db, campaignId, sender));

        // The audit pipeline has already saved from its catch; save again and clear, as a later request would see it.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var after = await ReadAsync(campaignId);
        after.State.Should().Be(MsfCampaignState.Draft);
        after.OpenedOn.Should().BeNull();
        after.TokenHashes.Should().Equal(before.TokenHashes, "no token may be rotated by an open that did not happen");

        sender.Attempted.Should().Be(2, "the second send is the one that failed");
        refusal.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be(OpenMsfCampaignCommandHandler.InvitationsNotSent);
    }

    [Fact]
    public async Task AFailedSend_IsRecordedWithoutTheRespondentsAddress()
    {
        // The sender's own message names the recipient, as an SMTP refusal might. The audit row records the handler's
        // refusal instead, and the address is kept out of the log it was redacted from at invitation. (T184)
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Draft);
        var sender = new ScriptedEmailSender(failOnSend: 2);

        await using (var db = CreateDb())
        {
            var open = () => OpenThroughTheAuditPipelineAsync(db, campaignId, sender);
            await open.Should().ThrowAsync<InvalidOperationException>();
        }

        await using var read = CreateDb();
        var row = await read.AuditEntries.AsNoTracking().SingleAsync();
        row.Action.Should().Be(nameof(OpenMsfCampaignCommand));
        row.Success.Should().BeFalse();
        foreach (var respondent in Respondents)
        {
            row.ErrorMessage.Should().NotContain(respondent);
            row.SummaryJson.Should().NotContain(respondent);
        }

        row.ErrorMessage.Should().Be(OpenMsfCampaignCommandHandler.InvitationsNotSent);
    }

    [Fact]
    public async Task AfterAFailedSend_OpeningAgain_MailsEveryRespondentALinkThatWorks()
    {
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Draft);
        var before = await ReadAsync(campaignId);

        await using (var failing = CreateDb())
        {
            var open = () => OpenThroughTheAuditPipelineAsync(failing, campaignId, new ScriptedEmailSender(failOnSend: 2));
            await open.Should().ThrowAsync<InvalidOperationException>();
        }

        // A new request, as the coordinator's second click would be.
        var sender = new ScriptedEmailSender(failOnSend: null);
        await using (var retry = CreateDb())
        {
            await OpenThroughTheAuditPipelineAsync(retry, campaignId, sender);
        }

        var after = await ReadAsync(campaignId);
        after.State.Should().Be(MsfCampaignState.Open);
        after.OpenedOn.Should().NotBeNull();

        sender.Sent.Select(message => message.To).Should().BeEquivalentTo(Respondents);
        foreach (var message in sender.Sent)
        {
            // The link mailed is the one stored: the token was minted before the send and its hash assigned after.
            var stored = after.Invitations.Single(invitation => invitation.Email == message.To);
            _tokens.VerifyToken(TokenIn(message), stored.TokenHash).Should().BeTrue($"{message.To}'s link must work");
            stored.TokenHash.Should().NotBe(before.Invitations.Single(i => i.Email == message.To).TokenHash);
        }
    }

    [Fact]
    public async Task OpeningACampaignThatIsNotADraft_MailsNobody_AndChangesNothing()
    {
        // The state refusal now comes before the first mail. Were it left to Open(), after the sends, an open campaign
        // would mail every respondent a second link that its stored tokens do not match.
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Open);
        var before = await ReadAsync(campaignId);
        var sender = new ScriptedEmailSender(failOnSend: null);

        await using var db = CreateDb();
        var open = () => OpenThroughTheAuditPipelineAsync(db, campaignId, sender);

        (await open.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("Only draft campaigns can be opened.");

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        sender.Attempted.Should().Be(0);
        (await ReadAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static async Task OpenThroughTheAuditPipelineAsync(ApplicationDbContext db, int campaignId, IEmailSender sender)
    {
        var command = new OpenMsfCampaignCommand(campaignId, TestPrincipals.Administrator());
        var handler = new OpenMsfCampaignCommandHandler(
            db,
            sender,
            new InvitationTokenService(),
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

    private async Task<int> SeedCampaignAsync(MsfCampaignState state)
    {
        await using var db = CreateDb();
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            State = state,
            OpenedOn = state == MsfCampaignState.Draft ? null : DateTime.UtcNow.AddDays(-1),
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
            },
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

    /// <summary>What an open could change, read through a fresh context so nothing tracked can mask it.</summary>
    private async Task<CampaignSnapshot> ReadAsync(int campaignId)
    {
        await using var db = CreateDb();
        var campaign = await db.MsfCampaigns
            .AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        var invitations = campaign.Invitations
            .OrderBy(invitation => invitation.Id)
            .Select(invitation => new InvitationSnapshot(invitation.RespondentEmail, invitation.TokenHash))
            .ToArray();

        return new CampaignSnapshot(campaign.State, campaign.OpenedOn, invitations);
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed record InvitationSnapshot(string? Email, string TokenHash);

    private sealed record CampaignSnapshot(MsfCampaignState State, DateTime? OpenedOn, InvitationSnapshot[] Invitations)
    {
        public string[] TokenHashes => Invitations.Select(invitation => invitation.TokenHash).ToArray();
    }

    /// <summary>Records every send; the <c>failOnSend</c>-th throws, naming the recipient as an SMTP refusal would.</summary>
    private sealed class ScriptedEmailSender(int? failOnSend) : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];
        public int Attempted { get; private set; }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Attempted++;
            if (Attempted == failOnSend)
            {
                throw new InvalidOperationException($"The mail server refused {message.To}.");
            }

            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "admin-user";
        public string? UserDisplay => "Admin";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }
}
