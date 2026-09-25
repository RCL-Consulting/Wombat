using System.Web;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// A campaign opened while mail is down: the page counts the links not delivered, and Resend sends each of those
/// respondents a new link that works once mail is back. (T251)
/// </summary>
/// <remarks>
/// <para>
/// Until T251 the open reported that every respondent had been emailed. A send is a hand-off to an in-process queue that
/// never fails for want of a mail server (<see cref="QueuedEmailSender" />); the worker delivered later, retried three
/// times and dropped the mail, and nothing on any page said so. A dropped link is a respondent who can never answer.
/// </para>
/// <para>
/// Each campaign here is opened and resent through the real handlers, handing their mail to a real
/// <see cref="EmailQueue" />, and each queue is drained by a real <see cref="EmailWorker" /> whose mail server fails or
/// works as the test says. The outcome is recorded by a stand-in for <c>MsfLinkDeliveryRecorder</c> that writes what its
/// statement writes through tracked entities, since the in-memory provider runs no <c>ExecuteUpdate</c>; the recorder's
/// own statement runs on PostgreSQL in <c>MsfLinkDeliveryPostgresTests</c>. Commands run inside the real audit pipeline,
/// and every read is through a fresh context after a save and a cleared tracker, so nothing tracked masks what was stored.
/// </para>
/// </remarks>
public sealed class MsfLinkDeliveryTests
{
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string TraineeId = "trainee-1";

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "nurse-2@example.test",
        "consultant-1@example.test"
    ];

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly InvitationTokenService _tokens = new();

    // ─── The whole path, mail down then back ─────────────────────────────────

    [Fact]
    public async Task WithMailDown_EveryLinkIsCountedNotDelivered_AndResendDeliversEachANewLinkThatWorks_OnceMailIsBack()
    {
        var campaignId = await SeedDraftAsync();

        // Opened: every link is handed over, and nothing has been heard of any yet.
        var opening = new EmailQueue();
        await OpenAsync(campaignId, opening);
        (await CountsAsync(campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 3));

        // The worker finds the mail server down, retries each mail, and gives up.
        var down = new ScriptedSmtp(refuses: _ => true);
        await DrainAsync(opening, down);
        down.Attempts.Should().Be(Respondents.Length * EmailWorker.MaxRetries);
        (await CountsAsync(campaignId)).Should().Be(new Counts(NotDelivered: 3, BeingSent: 0));

        // Resend: a new link for each, handed over; the count moves to being sent.
        var resending = new EmailQueue();
        var resent = await ResendAsync(campaignId, resending);
        resent.Should().Be(3);
        (await CountsAsync(campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 3));

        // Mail is back.
        var up = new ScriptedSmtp(refuses: _ => false);
        await DrainAsync(resending, up);
        (await CountsAsync(campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 0));

        up.Delivered.Select(message => message.To).Should().BeEquivalentTo(Respondents);
        foreach (var message in up.Delivered)
        {
            (await OpensTheQuestionnaireAsync(TokenIn(message))).Should().BeTrue($"{message.To}'s new link must work");
        }

        // The links first mailed reached nobody, and are not kept as a previous link: nothing answers to them.
        foreach (var message in down.Refused.DistinctBy(message => message.To))
        {
            (await OpensTheQuestionnaireAsync(TokenIn(message))).Should().BeFalse($"{message.To}'s dropped link opens nothing");
        }

        (await ReadAsync(campaignId)).Invitations.Should().OnlyContain(invitation => invitation.PreviousTokenSelector == null);
    }

    [Fact]
    public async Task OnlyTheLinksNotDelivered_AreSentAgain_AndTheOthersAreLeftAsTheyWere()
    {
        var campaignId = await SeedDraftAsync();
        var opening = new EmailQueue();
        await OpenAsync(campaignId, opening);
        await DrainAsync(opening, new ScriptedSmtp(refuses: message => message.To == "nurse-2@example.test"));
        (await CountsAsync(campaignId)).Should().Be(new Counts(NotDelivered: 1, BeingSent: 0));
        var before = await ReadAsync(campaignId);

        var resending = new EmailQueue();
        (await ResendAsync(campaignId, resending)).Should().Be(1);

        var after = await ReadAsync(campaignId);
        Queued(resending).Select(message => message.To).Should().Equal("nurse-2@example.test");
        foreach (var address in new[] { "nurse-1@example.test", "consultant-1@example.test" })
        {
            after.Of(address).Should().BeEquivalentTo(before.Of(address), "a delivered link is not touched");
        }

        after.Of("nurse-2@example.test").TokenHash.Should().NotBe(before.Of("nurse-2@example.test").TokenHash);
    }

    /// <summary>
    /// A host that crashed reports nothing, so its links would read as being sent for ever. Past the deadline they count
    /// as not delivered; the link replaced may have arrived after all, so it is kept as the previous link and still works.
    /// </summary>
    [Fact]
    public async Task ALinkNothingWasHeardOf_IsBeingSentUntilTheDeadline_ThenResent_AndTheOldLinkStillWorks()
    {
        var campaignId = await SeedDraftAsync();
        var capturing = new CapturingSender();
        await OpenAsync(campaignId, capturing);
        var openedAt = (await ReadAsync(campaignId)).Invitations.Max(invitation => invitation.IssuedOn);

        var early = new FixedClock(openedAt.AddMinutes(10));
        (await CountsAsync(campaignId, early)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 3));
        var tooEarly = await RefusalAsync(() => ResendAsync(campaignId, new CapturingSender(), early));
        tooEarly.Message.Should().Be(ResendMsfLinksCommandHandler.NothingToResend);

        var late = new FixedClock(openedAt + MsfInvitation.DeliveryReportDeadline + TimeSpan.FromMinutes(1));
        (await CountsAsync(campaignId, late)).Should().Be(new Counts(NotDelivered: 3, BeingSent: 0));
        var resent = new CapturingSender();
        (await ResendAsync(campaignId, resent, late)).Should().Be(3);

        foreach (var message in capturing.Sent.Concat(resent.Sent))
        {
            (await OpensTheQuestionnaireAsync(TokenIn(message))).Should().BeTrue($"both of {message.To}'s links work");
        }
    }

    // ─── What the mail carries ───────────────────────────────────────────────

    [Fact]
    public async Task EachLinksMail_CarriesAKeyNamingItsInvitationAndTheLinkStored_InNoTagAndNowhereInTheMail()
    {
        var campaignId = await SeedDraftAsync();
        var sender = new CapturingSender();

        await OpenAsync(campaignId, sender);

        var stored = await ReadAsync(campaignId);
        sender.Sent.Should().HaveCount(Respondents.Length);
        foreach (var message in sender.Sent)
        {
            MsfInvitation.TryReadDeliveryKey(message.DeliveryKey, out var invitationId, out var selector).Should().BeTrue();
            var invitation = stored.Invitations.Single(candidate => candidate.Id == invitationId);
            invitation.Email.Should().Be(message.To);
            selector.Should().Be(invitation.TokenSelector, "the key names the link the mail carries, which is the one stored");

            message.Tags.Should().Equal("msf-invite", $"campaign:{campaignId}");
            message.TextBody.Should().NotContain(message.DeliveryKey);
            message.HtmlBody.Should().NotContain(message.DeliveryKey);
            message.Subject.Should().NotContain(message.DeliveryKey);
        }
    }

    // ─── Refusals ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AHandOffThatFails_ChangesNoLink_AndItsAuditRowNamesNoAddress()
    {
        var campaignId = await SeedDraftAsync();
        var opening = new EmailQueue();
        await OpenAsync(campaignId, opening);
        await DrainAsync(opening, new ScriptedSmtp(refuses: _ => true));
        var before = await ReadAsync(campaignId);
        var sender = new CapturingSender(failOnSend: 2);

        await using (var db = CreateDb())
        {
            var refusal = await RefusalAsync(() => ResendThroughTheAuditPipelineAsync(db, campaignId, sender));
            refusal.Message.Should().Be(ResendMsfLinksCommandHandler.LinksNotSent);

            // The audit pipeline has already saved from its catch; save again and clear, as a later request would see it.
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }

        (await ReadAsync(campaignId)).Should().BeEquivalentTo(before, "no link may change on a resend that did not happen");
        (await CountsAsync(campaignId)).Should().Be(new Counts(NotDelivered: 3, BeingSent: 0), "resending again sends them all");

        await using var read = CreateDb();
        var row = await read.AuditEntries.AsNoTracking()
            .SingleAsync(entry => entry.Action == nameof(ResendMsfLinksCommand));
        row.Success.Should().BeFalse();
        row.ErrorMessage.Should().Be(ResendMsfLinksCommandHandler.LinksNotSent);
        foreach (var respondent in Respondents)
        {
            row.SummaryJson.Should().NotContain(respondent);
        }
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public async Task ACampaignThatIsNotOpen_IsRefused_AndNothingIsSentOrChanged(MsfCampaignState state)
    {
        var campaignId = await SeedDraftAsync();
        await using (var db = CreateDb())
        {
            var campaign = await db.MsfCampaigns.SingleAsync(entity => entity.Id == campaignId);
            campaign.State = state;
            await db.SaveChangesAsync();
        }

        var before = await ReadAsync(campaignId);
        var sender = new CapturingSender();

        var refusal = await RefusalAsync(() => ResendAsync(campaignId, sender));

        refusal.Message.Should().Be(ResendMsfLinksCommandHandler.OnlyOpenCampaigns);
        sender.Sent.Should().BeEmpty();
        (await ReadAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task ACampaignWhoseLinksWereAllDelivered_IsRefused_AndNothingIsSent()
    {
        var campaignId = await SeedDraftAsync();
        var opening = new EmailQueue();
        await OpenAsync(campaignId, opening);
        await DrainAsync(opening, new ScriptedSmtp(refuses: _ => false));
        var before = await ReadAsync(campaignId);
        var sender = new CapturingSender();

        var refusal = await RefusalAsync(() => ResendAsync(campaignId, sender));

        refusal.Message.Should().Be(ResendMsfLinksCommandHandler.NothingToResend);
        sender.Sent.Should().BeEmpty();
        (await ReadAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task ATraineeWithNoNameOnRecord_IsRefused_BeforeAnythingIsSent()
    {
        var campaignId = await SeedDraftAsync();
        var opening = new EmailQueue();
        await OpenAsync(campaignId, opening);
        await DrainAsync(opening, new ScriptedSmtp(refuses: _ => true));
        var before = await ReadAsync(campaignId);
        var sender = new CapturingSender();

        var refusal = await RefusalAsync(() => ResendAsync(campaignId, sender, users: FakeUserDirectory.Empty));

        refusal.Message.Should().Be(ResendMsfLinksCommandHandler.TraineeHasNoName);
        sender.Sent.Should().BeEmpty();
        (await ReadAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    // ─── The steps ───────────────────────────────────────────────────────────

    private async Task OpenAsync(int campaignId, EmailQueue queue)
        => await OpenAsync(campaignId, new QueuedEmailSender(queue, NullLogger<QueuedEmailSender>.Instance));

    private async Task OpenAsync(int campaignId, IEmailSender sender)
    {
        await using var db = CreateDb();
        var command = new OpenMsfCampaignCommand(campaignId, TestPrincipals.Administrator());
        var handler = new OpenMsfCampaignCommandHandler(
            db,
            sender,
            new InvitationTokenService(),
            new FakeUserDirectory((TraineeId, "Thandi Nkosi")),
            Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }));

        await ThroughTheAuditPipelineAsync(db, command, () => handler.Handle(command, CancellationToken.None));
    }

    private Task<int> ResendAsync(int campaignId, EmailQueue queue)
        => ResendAsync(campaignId, new QueuedEmailSender(queue, NullLogger<QueuedEmailSender>.Instance));

    private async Task<int> ResendAsync(
        int campaignId, IEmailSender sender, TimeProvider? clock = null, IUserAdministrationService? users = null)
    {
        await using var db = CreateDb();
        return await ResendThroughTheAuditPipelineAsync(db, campaignId, sender, clock, users);
    }

    private static async Task<int> ResendThroughTheAuditPipelineAsync(
        ApplicationDbContext db,
        int campaignId,
        IEmailSender sender,
        TimeProvider? clock = null,
        IUserAdministrationService? users = null)
    {
        var command = new ResendMsfLinksCommand(campaignId, TestPrincipals.Administrator());
        var handler = new ResendMsfLinksCommandHandler(
            db,
            sender,
            new InvitationTokenService(),
            users ?? new FakeUserDirectory((TraineeId, "Thandi Nkosi")),
            Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }),
            clock);

        var resent = 0;
        await ThroughTheAuditPipelineAsync(db, command, async () => resent = await handler.Handle(command, CancellationToken.None));
        return resent;
    }

    private static async Task ThroughTheAuditPipelineAsync<TCommand>(ApplicationDbContext db, TCommand command, Func<Task> handle)
        where TCommand : notnull
        => await new AuditPipelineBehavior<TCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await handle();
                    return Unit.Value;
                },
                CancellationToken.None);

    /// <summary>Runs a worker over everything queued, with the mail server <paramref name="smtp" /> stands for.</summary>
    private async Task DrainAsync(EmailQueue queue, ISmtpSender smtp)
    {
        queue.Writer.Complete();

        var services = new ServiceCollection();
        services.AddSingleton(smtp);
        services.AddSingleton<IEmailDeliveryObserver>(new TrackedDeliveryRecorder(CreateDb));
        await using var provider = services.BuildServiceProvider();

        var worker = new EmailWorker(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailWorker>.Instance,
            retryDelay: _ => TimeSpan.Zero);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await worker.ExecutePublicAsync(timeout.Token);
    }

    private static List<EmailMessage> Queued(EmailQueue queue)
    {
        var messages = new List<EmailMessage>();
        while (queue.Reader.TryRead(out var message))
        {
            messages.Add(message);
        }

        return messages;
    }

    private async Task<Counts> CountsAsync(int campaignId, TimeProvider? clock = null)
    {
        await using var db = CreateDb();
        var setup = await new GetMsfCampaignSetupQueryHandler(db, new FakeUserDirectory((TraineeId, "Thandi Nkosi")), clock)
            .Handle(new GetMsfCampaignSetupQuery(campaignId, TestPrincipals.Administrator()), CancellationToken.None);

        setup.Should().NotBeNull();
        return new Counts(setup!.LinksNotDelivered, setup.LinksBeingSent);
    }

    /// <summary>Whether the link opens the questionnaire, by the rule the respondent page and the submit both run.</summary>
    private async Task<bool> OpensTheQuestionnaireAsync(string token)
    {
        await using var db = CreateDb();
        try
        {
            await MsfCampaignRules.GetActiveInvitationByTokenAsync(db, token, _tokens, CancellationToken.None);
            return true;
        }
        catch (MsfResponseRefusedException)
        {
            return false;
        }
    }

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        var thrown = await Record.ExceptionAsync(act);
        thrown.Should().BeOfType<InvalidOperationException>("the command must refuse");
        return thrown!;
    }

    private static string TokenIn(EmailMessage message)
    {
        var url = message.TextBody
            .Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl, StringComparison.Ordinal));
        return HttpUtility.ParseQueryString(new Uri(url).Query)["token"]!;
    }

    // ─── Data ────────────────────────────────────────────────────────────────

    private async Task<int> SeedDraftAsync()
    {
        await using var db = CreateDb();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var campaign = new MsfCampaign
        {
            SubjectUserId = TraineeId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today,
            ClosesOn = today.AddDays(14),
            State = MsfCampaignState.Draft,
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
                    ExpiresOn = today.AddDays(21)
                })
                .ToList()
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    /// <summary>What a resend could change, read through a fresh context so nothing tracked can mask it.</summary>
    private async Task<CampaignSnapshot> ReadAsync(int campaignId)
    {
        await using var db = CreateDb();
        var campaign = await db.MsfCampaigns
            .AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        return new CampaignSnapshot(
            campaign.State,
            campaign.Invitations
                .OrderBy(invitation => invitation.Id)
                .Select(invitation => new InvitationSnapshot(
                    invitation.Id,
                    invitation.RespondentEmail,
                    invitation.TokenSelector,
                    invitation.TokenHash,
                    invitation.PreviousTokenSelector,
                    invitation.IssuedOn,
                    invitation.DeliveryLinkSelector,
                    invitation.SentOn,
                    invitation.DeliveryFailedOn))
                .ToArray());
    }

    /// <summary>What the campaign page is told: how many links were not delivered, and how many are being sent.</summary>
    private sealed record Counts(int NotDelivered, int BeingSent);

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed record InvitationSnapshot(
        int Id,
        string? Email,
        string? TokenSelector,
        string TokenHash,
        string? PreviousTokenSelector,
        DateTime IssuedOn,
        string? DeliveryLinkSelector,
        DateTime? SentOn,
        DateTime? DeliveryFailedOn);

    private sealed record CampaignSnapshot(MsfCampaignState State, InvitationSnapshot[] Invitations)
    {
        public InvitationSnapshot Of(string email) => Invitations.Single(invitation => invitation.Email == email);
    }

    // ─── Doubles ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Writes what <c>MsfLinkDeliveryRecorder</c>'s statement writes, under the same conditions, through tracked entities:
    /// the in-memory provider runs no <c>ExecuteUpdate</c>.
    /// </summary>
    private sealed class TrackedDeliveryRecorder(Func<ApplicationDbContext> createDb) : IEmailDeliveryObserver
    {
        public async Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken)
        {
            if (!MsfInvitation.TryReadDeliveryKey(message.DeliveryKey, out var invitationId, out var selector))
            {
                return;
            }

            await using var db = createDb();
            var invitation = await db.MsfInvitations.SingleOrDefaultAsync(
                candidate => candidate.Id == invitationId &&
                             candidate.RespondentEmail != null &&
                             candidate.AnonymizedOn == null,
                cancellationToken);

            if (invitation is null)
            {
                return;
            }

            invitation.DeliveryLinkSelector = selector;
            invitation.SentOn = outcome.Sent ? outcome.At : null;
            invitation.DeliveryFailedOn = outcome.Sent ? null : outcome.At;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>A mail server that refuses whatever <c>refuses</c> says, on every attempt.</summary>
    private sealed class ScriptedSmtp(Func<EmailMessage, bool> refuses) : ISmtpSender
    {
        public int Attempts { get; private set; }

        public List<EmailMessage> Delivered { get; } = [];

        public List<EmailMessage> Refused { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            Attempts++;
            if (refuses(message))
            {
                Refused.Add(message);
                throw new InvalidOperationException($"The mail server could not be reached for {message.To}.");
            }

            Delivered.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>Records every hand-off; the <c>failOnSend</c>-th throws, naming the recipient as a mail server might.</summary>
    private sealed class CapturingSender(int? failOnSend = null) : IEmailSender
    {
        private int _attempted;

        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            _attempted++;
            if (_attempted == failOnSend)
            {
                throw new InvalidOperationException($"The mail queue refused {message.To}.");
            }

            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
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
