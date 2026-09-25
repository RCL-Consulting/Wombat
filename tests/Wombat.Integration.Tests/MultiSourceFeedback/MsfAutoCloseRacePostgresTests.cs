using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Scheduling;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T267 on a real PostgreSQL server: a campaign whose row is written while the hourly auto-close job is closing it holds
/// back no other expired campaign. Each is closed in the same run.
/// </summary>
/// <remarks>
/// <para>
/// The job's save is checked against the campaign's <c>xmin</c> token (<c>MsfCampaignConfiguration</c>). EF InMemory has
/// no <c>xmin</c>, so only Postgres can show the refusal. Until T267 the job closed every expired campaign and stored them
/// in one save, so one refusal left every campaign open until the next run, an hour later.
/// </para>
/// <para>
/// The race is forced by an interceptor on the job's contexts. It runs the competing write just before the job's save of
/// the middle of three expired campaigns, so there is a campaign before it and one after it. The competing write is the
/// coordinator's real close command, or a write that leaves the campaign open: a resend of one respondent's link, which
/// replaces the link and keeps the one replaced as the previous link (T214), and stores the campaign row unchanged, as
/// <c>ResendMsfLinks</c> and a reminder's store (<c>MsfInvitationExpiryReminderJob</c>) do. Each runs on its own context,
/// as a request would. Isolated as every Postgres class is since T241: a schema of its own,
/// dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class MsfAutoCloseRacePostgresTests : IAsyncLifetime
{
    /// <summary>The job's clock: just after midnight UTC on the day after the three windows end.</summary>
    private static readonly DateTime JobNow = new(2026, 3, 2, 0, 0, 5, DateTimeKind.Utc);

    private static readonly DateOnly WindowEnded = new(2026, 3, 1);

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "consultant-1@example.test"
    ];

    private readonly TestSchemas _schemas = new();
    private readonly InvitationTokenService _tokens = new();

    /// <summary>Each link a competing resend stored, in the order it stored them.</summary>
    private readonly List<(int InvitationId, string Selector)> _resent = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task ACoordinatorsCloseOfOneExpiredCampaign_LeavesEveryOtherExpiredCampaignClosedInTheSameRun()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var (first, raced, last, running) = await SeedAsync(schema);
            var race = new RaceBeforeTheJobSaves(raced, times: 1, () => CoordinatorClosesAsync(schema, raced));
            var logger = new CapturingLogger();

            await RunTheJobAsync(schema, race, logger);

            race.Ran.Should().Be(1, "guard: the coordinator's close ran between the job's read of the campaign and its save");

            foreach (var id in new[] { first, last })
            {
                var campaign = await CampaignAsync(schema, id);
                campaign.State.Should().Be(MsfCampaignState.UnderReview, $"campaign {id} expired and was not raced");
                campaign.ClosedOn.Should().Be(JobNow, "the job closed it, in the same run");
                campaign.Invitations.Should().OnlyContain(invitation => invitation.RespondentEmail == null && invitation.AnonymizedOn == JobNow);
            }

            var closedElsewhere = await CampaignAsync(schema, raced);
            closedElsewhere.State.Should().Be(MsfCampaignState.UnderReview);
            closedElsewhere.ClosedOn.Should().NotBe(JobNow, "the coordinator's close stands, and the job did not close it again (T246)");
            closedElsewhere.ClosedOn.Should().BeAfter(JobNow);
            closedElsewhere.Invitations.Should().OnlyContain(invitation => invitation.RespondentEmail == null);

            (await CampaignAsync(schema, running)).State.Should().Be(MsfCampaignState.Open, "its window has not ended");

            logger.Messages.Should().Contain(
                (LogLevel.Information,
                 $"MsfCampaignAutoCloseJob: campaign {raced} was closed or withdrawn elsewhere while this run was closing it (it is now UnderReview), so it was skipped."));
            logger.Messages.Should().Contain(
                (LogLevel.Information, "MsfCampaignAutoCloseJob: auto-closed 2 of 3 expired campaigns; skipped 1, left open 0, failed 0."));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <remarks>
    /// The resend replaced one respondent's link and kept the replaced one as the previous link, which still takes an
    /// answer while the campaign is open (T214). The close after the second read must retire it, with the mail's outcome
    /// (T251): a close retried on the context of the refused save, its invitations as first read, would see nothing to
    /// retire and leave a live link on an anonymised invitation of a closed campaign.
    /// </remarks>
    [Fact]
    public async Task AWriteThatLeavesTheCampaignOpen_IsReadAgain_AndEveryExpiredCampaignClosesInTheSameRun()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var (first, raced, last, running) = await SeedAsync(schema);
            var race = new RaceBeforeTheJobSaves(raced, times: 1, () => ResendALinkAsync(schema, raced));
            var logger = new CapturingLogger();

            await RunTheJobAsync(schema, race, logger);

            race.Ran.Should().Be(1, "guard: the write ran between the job's read of the campaign and its save");

            foreach (var id in new[] { first, raced, last })
            {
                var campaign = await CampaignAsync(schema, id);
                campaign.State.Should().Be(MsfCampaignState.UnderReview, $"campaign {id} expired");
                campaign.ClosedOn.Should().Be(JobNow, "the job closed it, in the same run");
                campaign.Invitations.Should().OnlyContain(invitation => invitation.RespondentEmail == null && invitation.AnonymizedOn == JobNow);
                campaign.Invitations.Should().OnlyContain(
                    invitation => invitation.PreviousTokenSelector == null && invitation.PreviousTokenHash == null,
                    "closing retires every previous link (T214)");
                campaign.Invitations.Should().OnlyContain(
                    invitation => invitation.SentOn == null && invitation.DeliveryFailedOn == null && invitation.DeliveryLinkSelector == null,
                    "closing clears what became of every link's mail (T251)");
            }

            var (resentInvitation, resentSelector) = _resent.Should().ContainSingle().Subject;
            (await CampaignAsync(schema, raced)).Invitations.Single(invitation => invitation.Id == resentInvitation)
                .TokenSelector.Should().Be(resentSelector, "guard: the resend's write stands, and the job closed over it");

            (await CampaignAsync(schema, running)).State.Should().Be(MsfCampaignState.Open);

            logger.Messages.Should().Contain(
                (LogLevel.Information, "MsfCampaignAutoCloseJob: auto-closed 3 of 3 expired campaigns; skipped 0, left open 0, failed 0."));
            logger.Messages.Should().NotContain(message => message.Level == LogLevel.Warning);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ACampaignWrittenUnderBothReads_IsLeftOpenForTheNextRun_AndTheOthersClose()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var (first, raced, last, _) = await SeedAsync(schema);
            var race = new RaceBeforeTheJobSaves(raced, times: 2, () => ResendALinkAsync(schema, raced));
            var logger = new CapturingLogger();

            await RunTheJobAsync(schema, race, logger);

            race.Ran.Should().Be(2, "guard: the write ran before each of the job's two saves of the campaign");

            foreach (var id in new[] { first, last })
            {
                var campaign = await CampaignAsync(schema, id);
                campaign.State.Should().Be(MsfCampaignState.UnderReview, $"campaign {id} expired and was not raced");
                campaign.ClosedOn.Should().Be(JobNow);
            }

            var leftOpen = await CampaignAsync(schema, raced);
            leftOpen.State.Should().Be(MsfCampaignState.Open, "both of the job's saves were refused, and nothing of either was stored");
            leftOpen.ClosedOn.Should().BeNull();
            leftOpen.Invitations.Select(invitation => invitation.RespondentEmail).Should().BeEquivalentTo(Respondents);
            leftOpen.Invitations.Should().ContainSingle(invitation => invitation.PreviousTokenSelector != null)
                .Which.TokenSelector.Should().Be(_resent[^1].Selector, "the second resend's write stands, and nothing of the job's");

            logger.Messages.Should().Contain(
                (LogLevel.Warning,
                 $"MsfCampaignAutoCloseJob: campaign {raced} changed twice while this run was closing it, so it was left open for the next run."));
            logger.Messages.Should().Contain(
                (LogLevel.Information, "MsfCampaignAutoCloseJob: auto-closed 2 of 3 expired campaigns; skipped 0, left open 1, failed 0."));

            // The next run closes it, and retires the link the resends kept.
            await RunTheJobAsync(schema, interceptor: null, new CapturingLogger());
            var closedNextRun = await CampaignAsync(schema, raced);
            closedNextRun.State.Should().Be(MsfCampaignState.UnderReview);
            closedNextRun.Invitations.Should().OnlyContain(
                invitation => invitation.RespondentEmail == null &&
                              invitation.PreviousTokenSelector == null &&
                              invitation.PreviousTokenHash == null &&
                              invitation.SentOn == null &&
                              invitation.DeliveryLinkSelector == null);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The job and the competing writes ────────────────────────────────────

    /// <summary>Runs the job as the scheduler does: one scope per context it asks for, each carrying the interceptor.</summary>
    private static async Task RunTheJobAsync(string schema, IInterceptor? interceptor, ILogger logger)
    {
        var services = new ServiceCollection();
        services.AddScoped<IApplicationDbContext>(_ => NewContext(schema, interceptor));
        await using var provider = services.BuildServiceProvider();

        await new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(JobNow, logger), CancellationToken.None);
    }

    /// <summary>The coordinator's close command, as a request runs it, on its own context.</summary>
    private static async Task CoordinatorClosesAsync(string schema, int campaignId)
    {
        await using var db = NewContext(schema);
        await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
            .Handle(new CloseMsfCampaignCommand(campaignId, Administrator()), CancellationToken.None);
    }

    /// <summary>
    /// A resend of the first respondent's link, as <c>ResendMsfLinks</c> stores it: a new link in place of theirs, which
    /// is kept as the previous link (<see cref="MsfInvitation.ReplaceLink" />, T214), and the campaign row stored
    /// unchanged, so its <c>xmin</c> moves and it stays open. The mail worker's report that the new link was sent is
    /// stored with it (T251).
    /// </summary>
    private async Task ResendALinkAsync(string schema, int campaignId)
    {
        await using var db = NewContext(schema);
        var campaign = await db.MsfCampaigns
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);
        var invitation = campaign.Invitations.OrderBy(entity => entity.Id).First();
        var link = _tokens.GenerateSelectorToken();

        invitation.ReplaceLink(link.Selector, link.Hash, JobNow);
        invitation.SentOn = JobNow;
        invitation.DeliveryLinkSelector = link.Selector;
        db.Entry(campaign).Property(entity => entity.State).IsModified = true;
        await db.SaveChangesAsync();

        _resent.Add((invitation.Id, link.Selector));
    }

    /// <summary>
    /// Runs the competing write just before the job saves <c>campaignId</c> closed, up to <c>times</c> times; every other
    /// save passes straight through.
    /// </summary>
    private sealed class RaceBeforeTheJobSaves(int campaignId, int times, Func<Task> competing) : SaveChangesInterceptor
    {
        private int _remaining = times;

        public int Ran { get; private set; }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var closesTheRacedCampaign = eventData.Context!.ChangeTracker.Entries<MsfCampaign>()
                .Any(entry => entry.State == EntityState.Modified && entry.Entity.Id == campaignId);

            if (closesTheRacedCampaign && _remaining > 0)
            {
                _remaining--;
                await competing();
                Ran++;
            }

            return result;
        }
    }

    // ─── Seed and read ───────────────────────────────────────────────────────

    /// <summary>
    /// Three open campaigns whose windows ended yesterday, by the job's clock, and one whose window runs on. Returned in
    /// id order, which is the order the job closes them in.
    /// </summary>
    private async Task<(int First, int Raced, int Last, int Running)> SeedAsync(string schema)
    {
        await using var db = NewContext(schema);
        var template = new MsfTemplate { Name = "T267 MSF" };
        var campaigns = new[] { WindowEnded, WindowEnded, WindowEnded, WindowEnded.AddDays(7) }
            .Select((closesOn, index) => OpenCampaign(template, $"trainee-{index + 1}", closesOn))
            .ToList();

        // One save per campaign, so the ids run in the order above.
        foreach (var campaign in campaigns)
        {
            db.MsfCampaigns.Add(campaign);
            await db.SaveChangesAsync();
        }

        return (campaigns[0].Id, campaigns[1].Id, campaigns[2].Id, campaigns[3].Id);
    }

    private MsfCampaign OpenCampaign(MsfTemplate template, string subjectUserId, DateOnly closesOn)
    {
        var opensOn = closesOn.AddDays(-14);
        var openedOn = opensOn.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);

        return new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            CreatedByUserId = "coordinator-1",
            CreatedOn = openedOn.AddDays(-1),
            OpensOn = opensOn,
            ClosesOn = closesOn,
            State = MsfCampaignState.Open,
            OpenedOn = openedOn,
            Template = template,
            Invitations = Respondents
                .Select(email =>
                {
                    var link = _tokens.GenerateSelectorToken();
                    return new MsfInvitation
                    {
                        RespondentEmail = email,
                        RespondentCategory = email.StartsWith("nurse", StringComparison.Ordinal)
                            ? MsfRespondentCategory.Nurse
                            : MsfRespondentCategory.Consultant,
                        TokenSelector = link.Selector,
                        TokenHash = link.Hash,
                        IssuedOn = openedOn,
                        ExpiresOn = closesOn.AddDays(7)
                    };
                })
                .ToList()
        };
    }

    private static async Task<MsfCampaign> CampaignAsync(string schema, int campaignId)
    {
        await using var db = NewContext(schema);
        return await db.MsfCampaigns
            .AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);
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

    /// <summary>Keeps each log entry's level and rendered message.</summary>
    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add((logLevel, formatter(state, exception)));
    }

    // ─── Schema helpers (as MsfWithdrawRacePostgresTests) ────────────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema, IInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
        }

        return new ApplicationDbContext(options.Options);
    }
}
