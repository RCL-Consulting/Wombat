using System.Globalization;
using System.Security.Claims;
using System.Web;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Scheduling;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T251 on a real PostgreSQL server: the mail worker's report of each MSF link's mail lands on its invitation, in any
/// order with the request that sent it; the campaign page counts the links not delivered; and Resend delivers each of
/// those respondents a link that works once mail is back.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MsfLinkDeliveryRecorder" /> writes with one conditioned <c>UPDATE</c>, which the in-memory provider cannot
/// run, and its whole point is how it meets the other writers of the row: the open and the reminder, which store the link
/// after handing its mail over, and a close, which anonymises. So those meetings are staged here, with the real handlers,
/// the real worker and the real recorder, each on its own context as a request and the worker's scope would be.
/// </para>
/// <para>
/// Isolated as every Postgres class is since T241: a schema of its own, created through <see cref="TestSchemas" /> and
/// dropped in a finally and again on dispose, with every connection from <see cref="TestDatabase" />.
/// </para>
/// </remarks>
public sealed class MsfLinkDeliveryPostgresTests : IAsyncLifetime
{
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string T251MigrationSuffix = "_T251_MsfInvitationDeliveryOutcome";
    private const string DeliveryOutcomeCheck = "CK_MsfInvitations_DeliveryOutcome";

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "nurse-2@example.test",
        "consultant-1@example.test"
    ];

    private static readonly string[] OutcomeColumns = ["DeliveryFailedOn", "DeliveryLinkSelector", "SentOn"];

    private readonly TestSchemas _schemas = new();
    private readonly InvitationTokenService _tokens = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    // ─── The migration ───────────────────────────────────────────────────────

    [Fact]
    public async Task TheMigration_AddsTheOutcomeEmpty_RefusesSentAndDroppedTogether_AndDownTakesItAway()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            await OpenAsync(schema, campaignId, new CapturingSender());

            (await OutcomeColumnsAsync(schema)).Should().Equal(OutcomeColumns);
            (await ScalarAsync<long>(schema,
                    """SELECT COUNT(*) FROM "MsfInvitations" WHERE "SentOn" IS NOT NULL OR "DeliveryFailedOn" IS NOT NULL OR "DeliveryLinkSelector" IS NOT NULL"""))
                .Should().Be(0, "nothing has been reported yet, and nothing is guessed");

            var both = () => ExecuteAsync(schema,
                """UPDATE "MsfInvitations" SET "DeliveryLinkSelector" = "TokenSelector", "SentOn" = now(), "DeliveryFailedOn" = now()""");
            (await both.Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be(DeliveryOutcomeCheck);

            string predecessor;
            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t251 = migrations.FindIndex(migration => migration.EndsWith(T251MigrationSuffix, StringComparison.Ordinal));
                t251.Should().BePositive("guard: the T251 migration is in the assembly, after at least one other");
                predecessor = migrations[t251 - 1];
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            (await OutcomeColumnsAsync(schema)).Should().BeEmpty();
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" """)).Should().Be(Respondents.Length);

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            (await OutcomeColumnsAsync(schema)).Should().Equal(OutcomeColumns);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The report and the other writers of the row ─────────────────────────

    /// <summary>
    /// A fast mail server answers before the open has stored the links it handed over. The report is written by the
    /// invitation's id and names its link, so it lands on the row before the link does, and counts once the open's save
    /// stores that link; the report writes no campaign row, so that save, checked against the campaign's xmin, is not
    /// refused.
    /// </summary>
    [Fact]
    public async Task AReportThatArrivesBeforeTheOpenHasStoredItsLinks_IsKept_AndTheOpenIsNotRefused()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            var sender = new CapturingSender(onSend: message => ReportAsync(schema, message, sent: true));

            await OpenAsync(schema, campaignId, sender);

            var invitations = await ReadInvitationsAsync(schema);
            invitations.Should().HaveCount(Respondents.Length).And.OnlyContain(invitation =>
                invitation.TokenSelector != null &&
                invitation.DeliveryLinkSelector == invitation.TokenSelector &&
                invitation.SentOn != null &&
                invitation.DeliveryFailedOn == null);
            (await CampaignAsync(schema, campaignId)).State.Should().Be(MsfCampaignState.Open);
            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 0));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The same race for a resend, as a local mail server runs it: each new link is delivered, and reported, before the
    /// resend has stored it, while the invitation still holds the dropped link and the report about that one. The last
    /// report wins, so the resent links read as delivered once stored, not as unreported and, an hour on, as not
    /// delivered again.
    /// </summary>
    [Fact]
    public async Task AReportThatArrivesBeforeAResendHasStoredItsLinks_IsKept_OverTheReportAboutTheDroppedLink()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            var opening = new CapturingSender();
            await OpenAsync(schema, campaignId, opening);
            foreach (var message in opening.Sent)
            {
                await ReportAsync(schema, message, sent: false);
            }

            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 3, BeingSent: 0));

            var resending = new CapturingSender(onSend: message => ReportAsync(schema, message, sent: true));
            (await ResendAsync(schema, campaignId, resending)).Should().Be(3);

            (await ReadInvitationsAsync(schema)).Should().OnlyContain(invitation =>
                invitation.DeliveryLinkSelector == invitation.TokenSelector &&
                invitation.SentOn != null &&
                invitation.DeliveryFailedOn == null);
            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 0));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A campaign closed while its mail was still queued: the report finds every address erased and writes nothing, so the
    /// anonymised rows hold no outcome, and nothing computable from an address is left anywhere (T207).
    /// </summary>
    [Fact]
    public async Task AReportAfterTheCampaignClosed_WritesNothing_AndNoTraceOfAnyAddressRemains()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            var sender = new CapturingSender();
            await OpenAsync(schema, campaignId, sender);
            await ReportAsync(schema, sender.Sent[0], sent: false);

            await using (var db = NewContext(schema))
            {
                await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
                    .Handle(new CloseMsfCampaignCommand(campaignId, Administrator()), CancellationToken.None);
            }

            foreach (var message in sender.Sent)
            {
                await ReportAsync(schema, message, sent: false);
            }

            (await ReadInvitationsAsync(schema)).Should().OnlyContain(invitation =>
                invitation.RespondentEmail == null &&
                invitation.SentOn == null &&
                invitation.DeliveryFailedOn == null &&
                invitation.DeliveryLinkSelector == null);

            foreach (var respondent in Respondents)
            {
                (await RespondentTraceSearch.TracesAsync(TestDatabase.SchemaConnectionString(schema), respondent))
                    .Should().BeEmpty($"nothing computable from {respondent} may outlive the close");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The race the review found (T251 review, finding 1): the worker's report lands after an anonymising request has read
    /// the invitations and before it saves. EF writes only the columns it saw change, and a column read as null and
    /// cleared to null has not changed, so the report's time and selector would survive on the erased row: a time one mail
    /// apart from the next, beside a log line that names the address mailed at that instant, on the row the respondent's
    /// answers name. Every save of an anonymised invitation therefore writes the three columns
    /// (<see cref="ApplicationDbContext" />), whichever of the three writers anonymises.
    /// </summary>
    [Theory]
    [InlineData("close")]
    [InlineData("withdraw")]
    [InlineData("auto-close")]
    public async Task AReportThatLandsBetweenAnAnonymisingReadAndItsSave_DoesNotSurviveOnTheErasedRows(string anonymiser)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            var sender = new CapturingSender();
            await OpenAsync(schema, campaignId, sender);

            var landedBeforeTheSave = -1L;
            var reportsLand = new BeforeFirstSave(async () =>
            {
                foreach (var message in sender.Sent)
                {
                    await ReportAsync(schema, message, sent: true);
                }

                landedBeforeTheSave = await ScalarAsync<long>(schema,
                    """SELECT COUNT(*) FROM "MsfInvitations" WHERE "SentOn" IS NOT NULL AND "RespondentEmail" IS NOT NULL""");
            });

            await AnonymiseAsync(schema, campaignId, anonymiser, reportsLand);

            landedBeforeTheSave.Should().Be(
                Respondents.Length, "guard: every report was written after the invitations were read and before the save");
            (await ReadInvitationsAsync(schema)).Should().HaveCount(Respondents.Length).And.OnlyContain(invitation =>
                invitation.RespondentEmail == null &&
                invitation.AnonymizedOn != null &&
                invitation.SentOn == null &&
                invitation.DeliveryFailedOn == null &&
                invitation.DeliveryLinkSelector == null);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The other order: the report arrives while the close's save holds the rows, uncommitted. It waits on the row's lock,
    /// and once the close commits, PostgreSQL checks the row as the close left it against the report's condition, finds the
    /// address erased, and the report writes nothing.
    /// </summary>
    [Fact]
    public async Task AReportThatWaitsOnAnUncommittedClose_FindsTheAddressErased_AndWritesNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            var sender = new CapturingSender();
            await OpenAsync(schema, campaignId, sender);

            await using (var closing = NewContext(schema))
            {
                await using var transaction = await closing.Database.BeginTransactionAsync();
                await new CloseMsfCampaignCommandHandler(closing, new MsfAggregationService())
                    .Handle(new CloseMsfCampaignCommand(campaignId, Administrator()), CancellationToken.None);

                var reporter = $"t251-report-{Guid.NewGuid():N}";
                var report = ReportAsync(schema, sender.Sent[0], sent: true, applicationName: reporter);
                await WaitUntilWaitingForALockAsync(reporter, report);

                await transaction.CommitAsync();
                await report.WaitAsync(TimeSpan.FromSeconds(30));
            }

            (await ReadInvitationsAsync(schema)).Should().HaveCount(Respondents.Length).And.OnlyContain(invitation =>
                invitation.RespondentEmail == null &&
                invitation.SentOn == null &&
                invitation.DeliveryFailedOn == null &&
                invitation.DeliveryLinkSelector == null);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── Mail down, then back ────────────────────────────────────────────────

    [Fact]
    public async Task WithMailDown_ThePageCountsEveryLinkNotDelivered_AndResendDeliversEachALinkThatWorks_OnceMailIsBack()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);

            var opening = new EmailQueue();
            await OpenAsync(schema, campaignId, Queued(opening));
            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 3));

            var down = new ScriptedSmtp(works: false);
            await DrainAsync(schema, opening, down);
            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 3, BeingSent: 0));
            (await ReadInvitationsAsync(schema)).Should().OnlyContain(invitation =>
                invitation.DeliveryLinkSelector == invitation.TokenSelector && invitation.DeliveryFailedOn != null);

            var resending = new EmailQueue();
            (await ResendAsync(schema, campaignId, Queued(resending))).Should().Be(3);
            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 3));

            var up = new ScriptedSmtp(works: true);
            await DrainAsync(schema, resending, up);
            (await CountsAsync(schema, campaignId)).Should().Be(new Counts(NotDelivered: 0, BeingSent: 0));

            up.Delivered.Select(message => message.To).Should().BeEquivalentTo(Respondents);
            foreach (var message in up.Delivered)
            {
                (await OpensTheQuestionnaireAsync(schema, TokenIn(message))).Should().BeTrue($"{message.To}'s new link must work");
            }

            foreach (var message in down.Refused.DistinctBy(message => message.To))
            {
                (await OpensTheQuestionnaireAsync(schema, TokenIn(message)))
                    .Should().BeFalse($"{message.To}'s dropped link reached nobody and is not kept");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A close that commits while a resend is handing its links over: the resend's save is checked against the campaign's
    /// xmin and refused whole, so no link lands on an anonymised row, and the links it sent open nothing.
    /// </summary>
    [Fact]
    public async Task AResendThatACloseOvertakes_IsRefusedWhole_AndItsLinksOpenNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftAsync(schema);
            var opening = new CapturingSender();
            await OpenAsync(schema, campaignId, opening);
            foreach (var message in opening.Sent)
            {
                await ReportAsync(schema, message, sent: false);
            }

            var resending = new CapturingSender(beforeFirstSend: async () =>
            {
                await using var db = NewContext(schema);
                await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
                    .Handle(new CloseMsfCampaignCommand(campaignId, Administrator()), CancellationToken.None);
            });

            var resend = () => ResendAsync(schema, campaignId, resending);
            (await resend.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(ResendMsfLinksCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>();

            (await CampaignAsync(schema, campaignId)).State.Should().Be(MsfCampaignState.UnderReview);
            (await ReadInvitationsAsync(schema)).Should().OnlyContain(invitation =>
                invitation.RespondentEmail == null && invitation.DeliveryLinkSelector == null);
            foreach (var message in resending.Sent)
            {
                (await OpensTheQuestionnaireAsync(schema, TokenIn(message))).Should().BeFalse();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The steps ───────────────────────────────────────────────────────────

    private async Task OpenAsync(string schema, int campaignId, IEmailSender sender)
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

    private async Task<int> ResendAsync(string schema, int campaignId, IEmailSender sender)
    {
        await using var db = NewContext(schema);
        var command = new ResendMsfLinksCommand(campaignId, Administrator());
        var handler = new ResendMsfLinksCommandHandler(
            db,
            sender,
            new InvitationTokenService(),
            new FakeUserDirectory(("trainee-1", "Thandi Nkosi")),
            Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }));

        return await new AuditPipelineBehavior<ResendMsfLinksCommand, int>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    /// <summary>The real recorder, on a context of its own, as the worker's scope runs it.</summary>
    private static async Task ReportAsync(string schema, EmailMessage message, bool sent, string? applicationName = null)
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema, applicationName))
            .Options);
        var outcome = sent
            ? EmailDeliveryOutcome.Delivered(1, DateTime.UtcNow)
            : EmailDeliveryOutcome.Dropped(3, DateTime.UtcNow);
        await new MsfLinkDeliveryRecorder(db).RecordAsync(message, outcome, CancellationToken.None);
    }

    /// <summary>A real worker over everything queued, reporting through the real recorder.</summary>
    private async Task DrainAsync(string schema, EmailQueue queue, ISmtpSender smtp)
    {
        queue.Writer.Complete();

        var services = new ServiceCollection();
        services.AddSingleton(smtp);
        services.AddScoped<IApplicationDbContext>(_ => NewContext(schema));
        services.AddScoped<IEmailDeliveryObserver, MsfLinkDeliveryRecorder>();
        await using var provider = services.BuildServiceProvider();

        var worker = new EmailWorker(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailWorker>.Instance,
            retryDelay: _ => TimeSpan.Zero);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await worker.ExecutePublicAsync(timeout.Token);
    }

    /// <summary>
    /// Anonymises the campaign's invitations through one of the three writers that do, on a context carrying
    /// <paramref name="interceptor" />: the coordinator's close, the withdrawal, or the hourly auto-close job, run a day
    /// after the window closed.
    /// </summary>
    private async Task AnonymiseAsync(string schema, int campaignId, string anonymiser, IInterceptor interceptor)
    {
        switch (anonymiser)
        {
            case "close":
                await using (var db = NewContext(schema, interceptor))
                {
                    await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
                        .Handle(new CloseMsfCampaignCommand(campaignId, Administrator()), CancellationToken.None);
                }

                break;

            case "withdraw":
                await using (var db = NewContext(schema, interceptor))
                {
                    await new WithdrawMsfCampaignCommandHandler(db)
                        .Handle(new WithdrawMsfCampaignCommand(campaignId, MsfCampaignState.Open, Administrator()), CancellationToken.None);
                }

                break;

            case "auto-close":
                var services = new ServiceCollection();
                services.AddScoped<IApplicationDbContext>(_ => NewContext(schema, interceptor));
                await using (var provider = services.BuildServiceProvider())
                {
                    var closesOn = (await CampaignAsync(schema, campaignId)).ClosesOn;
                    var dayAfter = closesOn.AddDays(1).ToDateTime(new TimeOnly(1, 0), DateTimeKind.Utc);
                    await new MsfCampaignAutoCloseJob(provider.GetRequiredService<IServiceScopeFactory>())
                        .ExecuteAsync(new ScheduledJobContext(dayAfter, NullLogger.Instance), CancellationToken.None);
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(anonymiser), anonymiser, null);
        }

        (await CampaignAsync(schema, campaignId)).State.Should().NotBe(
            MsfCampaignState.Open, $"guard: the {anonymiser} committed");
    }

    /// <summary>Waits until the server reports the named connection waiting for a lock; fails if the request finished first.</summary>
    private static async Task WaitUntilWaitingForALockAsync(string applicationName, Task request)
    {
        await using var monitor = await TestDatabase.OpenAdminConnectionAsync();

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            request.IsCompleted.Should().BeFalse("guard: the report must wait on the close's uncommitted rows");

            await using var command = new NpgsqlCommand(
                """SELECT count(*) FROM pg_stat_activity WHERE application_name = $1 AND wait_event_type = 'Lock'""", monitor);
            command.Parameters.Add(new NpgsqlParameter { Value = applicationName });
            if (Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture) > 0)
            {
                return;
            }

            await Task.Delay(25);
        }

        throw new TimeoutException($"The report '{applicationName}' did not wait for a lock within 30 seconds.");
    }

    private static QueuedEmailSender Queued(EmailQueue queue) => new(queue, NullLogger<QueuedEmailSender>.Instance);

    private async Task<Counts> CountsAsync(string schema, int campaignId)
    {
        await using var db = NewContext(schema);
        var setup = await new GetMsfCampaignSetupQueryHandler(db, new FakeUserDirectory(("trainee-1", "Thandi Nkosi")))
            .Handle(new GetMsfCampaignSetupQuery(campaignId, Administrator()), CancellationToken.None);

        setup.Should().NotBeNull();
        return new Counts(setup!.LinksNotDelivered, setup.LinksBeingSent);
    }

    private async Task<bool> OpensTheQuestionnaireAsync(string schema, string token)
    {
        await using var db = NewContext(schema);
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

    private static string TokenIn(EmailMessage message)
    {
        var url = message.TextBody
            .Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl, StringComparison.Ordinal));
        return HttpUtility.ParseQueryString(new Uri(url).Query)["token"]!;
    }

    // ─── The data ────────────────────────────────────────────────────────────

    private async Task<int> SeedDraftAsync(string schema)
    {
        await using var db = NewContext(schema);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today,
            ClosesOn = today.AddDays(14),
            State = MsfCampaignState.Draft,
            Template = new MsfTemplate
            {
                Name = "T251 MSF",
                IsActive = true,
                Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.LongText, Required = true }]
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

    private async Task<List<MsfInvitation>> ReadInvitationsAsync(string schema)
    {
        await using var db = NewContext(schema);
        return await db.MsfInvitations.AsNoTracking().OrderBy(invitation => invitation.Id).ToListAsync();
    }

    private async Task<MsfCampaign> CampaignAsync(string schema, int campaignId)
    {
        await using var db = NewContext(schema);
        return await db.MsfCampaigns.AsNoTracking().SingleAsync(campaign => campaign.Id == campaignId);
    }

    private async Task<List<string>> OutcomeColumnsAsync(string schema)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(
            """
            SELECT column_name FROM information_schema.columns
            WHERE table_schema = current_schema() AND table_name = 'MsfInvitations'
              AND column_name IN ('SentOn', 'DeliveryFailedOn', 'DeliveryLinkSelector')
            ORDER BY column_name
            """, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var columns = new List<string>();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync())!;
    }

    private async Task ExecuteAsync(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
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

    /// <summary>What the campaign page is told: how many links were not delivered, and how many are being sent.</summary>
    private sealed record Counts(int NotDelivered, int BeingSent);

    // ─── Doubles ─────────────────────────────────────────────────────────────

    /// <summary>Records every hand-off; the first can run something first, and each can run something after.</summary>
    private sealed class CapturingSender(Func<Task>? beforeFirstSend = null, Func<EmailMessage, Task>? onSend = null) : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (Sent.Count == 0 && beforeFirstSend is not null)
            {
                await beforeFirstSend();
            }

            Sent.Add(message);
            if (onSend is not null)
            {
                await onSend(message);
            }
        }
    }

    /// <summary>A mail server that takes every mail, or none.</summary>
    private sealed class ScriptedSmtp(bool works) : ISmtpSender
    {
        public List<EmailMessage> Delivered { get; } = [];

        public List<EmailMessage> Refused { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            if (!works)
            {
                Refused.Add(message);
                throw new InvalidOperationException("The mail server could not be reached.");
            }

            Delivered.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>Runs something once, just before this context's first save goes to the database.</summary>
    private sealed class BeforeFirstSave(Func<Task> action) : SaveChangesInterceptor
    {
        private Func<Task>? _action = action;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref _action, null) is { } run)
            {
                await run();
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

    // ─── Schema helpers (as MsfOpenCampaignRacePostgresTests) ───────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .AddInterceptors(interceptors)
            .Options);
}
