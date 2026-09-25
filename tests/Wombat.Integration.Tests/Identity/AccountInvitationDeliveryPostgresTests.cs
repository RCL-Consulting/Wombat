using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Web;
using FluentAssertions;
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
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.Invitations;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T283 on a real PostgreSQL server: the mail worker's report of each account invitation's mail lands on the invitation,
/// the invitations list says which were not delivered, and Resend emails a link that works once mail is back, while the
/// link it replaced is refused.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AccountInvitationDeliveryRecorder" /> and the resend's store are each one conditioned <c>UPDATE</c>, which
/// the in-memory provider cannot run, and their point is how they meet the other writers of the row: a second resend, an
/// accept, a revoke, and a report that the mail arrived after all. So those meetings are staged here, with the real
/// handlers, the real worker and the real recorder, each on its own context as a request and the worker's scope would be.
/// </para>
/// <para>
/// Isolated as every Postgres class is since T241: a schema of its own, created through <see cref="TestSchemas" /> and
/// dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class AccountInvitationDeliveryPostgresTests : IAsyncLifetime
{
    private const string BaseUrl = "https://wombat.example";
    private const string Invitee = "registrar@hospital.test";
    private const string T283MigrationSuffix = "_T283_AccountInvitationDeliveryOutcome";
    private const string DeliveryOutcomeCheck = "CK_Invitations_DeliveryOutcome";

    private static readonly string[] OutcomeColumns = ["DeliveryFailedOn", "DeliveryFailures", "SentOn"];

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
            await IssueAsync(schema, new CapturingSender());

            (await OutcomeColumnsAsync(schema)).Should().Equal(OutcomeColumns);
            (await ScalarAsync<long>(schema,
                    """SELECT COUNT(*) FROM "Invitations" WHERE "SentOn" IS NULL AND "DeliveryFailedOn" IS NULL AND "DeliveryFailures" = 0"""))
                .Should().Be(1, "nothing has been reported yet, and nothing is guessed");

            var both = () => ExecuteAsync(schema, """UPDATE "Invitations" SET "SentOn" = now(), "DeliveryFailedOn" = now()""");
            (await both.Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be(DeliveryOutcomeCheck);

            var negative = () => ExecuteAsync(schema, """UPDATE "Invitations" SET "DeliveryFailures" = -1""");
            (await negative.Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be("CK_Invitations_DeliveryFailures");

            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t283 = migrations.FindIndex(migration => migration.EndsWith(T283MigrationSuffix, StringComparison.Ordinal));
                t283.Should().BePositive("guard: the T283 migration is in the assembly, after at least one other");
                await db.GetService<IMigrator>().MigrateAsync(migrations[t283 - 1]);
            }

            (await OutcomeColumnsAsync(schema)).Should().BeEmpty();
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Invitations" """)).Should().Be(1);

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

    // ─── End to end ──────────────────────────────────────────────────────────

    /// <summary>
    /// The task's case. Issued while the mail server refuses everything, the invitation is listed as not delivered at once.
    /// Resent once mail is back, it is listed as sent, its new link opens the register page, and the link first mailed is
    /// refused: a resend retires it.
    /// </summary>
    [Fact]
    public async Task AnInvitationIssuedWhileMailIsDown_IsListedNotDelivered_AndResendDeliversALinkThatWorks()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            var down = new EmailQueue();
            var issued = await IssueAsync(schema, Queued(down));
            await DrainAsync(schema, down, new ScriptedSmtp(works: false));

            var listed = (await ListAsync(schema)).Should().ContainSingle().Which;
            (listed.Delivery, listed.DeliveryFailures, listed.SuggestCheckingAddress)
                .Should().Be((InvitationDelivery.NotDelivered, 1, false));

            var up = new EmailQueue();
            var resent = await ResendAsync(schema, issued.InvitationId, Queued(up));
            (await ListAsync(schema)).Single().Delivery.Should().Be(InvitationDelivery.BeingSent);

            var smtp = new ScriptedSmtp(works: true);
            await DrainAsync(schema, up, smtp);

            var mail = smtp.Delivered.Should().ContainSingle().Which;
            mail.To.Should().Be(Invitee);
            TokenIn(mail).Should().Be(resent.Token);

            listed = (await ListAsync(schema)).Should().ContainSingle().Which;
            (listed.Delivery, listed.DeliveryFailures, listed.SuggestCheckingAddress)
                .Should().Be((InvitationDelivery.Sent, 1, false));

            (await PreviewAsync(schema, resent.Token)).Email.Should().Be(Invitee);
            var old = () => PreviewAsync(schema, issued.Token);
            (await old.Should().ThrowAsync<InvitationRefusedException>()).Which.Reason.Should().Be(InvitationRefusal.Invalid);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// An address the mail server always refuses stays not delivered however often it is resent. From the second failure
    /// the list says to check the address; Resend is still offered, since a mail server down twice looks the same.
    /// </summary>
    [Fact]
    public async Task ASecondFailedMail_SaysToCheckTheAddress()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            var first = new EmailQueue();
            var issued = await IssueAsync(schema, Queued(first));
            await DrainAsync(schema, first, new ScriptedSmtp(works: false));
            (await ListAsync(schema)).Single().SuggestCheckingAddress.Should().BeFalse("one failure is as likely the server's");

            var second = new EmailQueue();
            await ResendAsync(schema, issued.InvitationId, Queued(second));
            await DrainAsync(schema, second, new ScriptedSmtp(works: false));

            var listed = (await ListAsync(schema)).Single();
            (listed.Delivery, listed.DeliveryFailures, listed.SuggestCheckingAddress)
                .Should().Be((InvitationDelivery.NotDelivered, 2, true));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Who may resend is who may issue (T283 review): a resend hands its caller a new registration link. An Administrator
    /// resends a CollegeAdmin's invitation, and an InstitutionalAdmin their own institution's. That no one else resends a
    /// CollegeAdmin's, that College's own CollegeAdmin included, is <c>InvitationDeliveryTests</c>.
    /// </summary>
    [Theory]
    [InlineData("an Administrator, a CollegeAdmin's invitation")]
    [InlineData("an InstitutionalAdmin, their institution's invitation")]
    public async Task WhoeverMayIssueAnInvitation_MayResendIt(string resend)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var ofACollegeAdmin = resend.StartsWith("an Administrator", StringComparison.Ordinal);
            IssuedInvitationResult issued;
            ClaimsPrincipal caller;
            if (ofACollegeAdmin)
            {
                int collegeId;
                await using (var seed = NewContext(schema))
                {
                    var college = new College { Name = "T283 College", ShortCode = "T283" };
                    seed.Colleges.Add(college);
                    await seed.SaveChangesAsync();
                    collegeId = college.Id;
                }

                await using var db = NewContext(schema);
                issued = await new IssueInvitationCommandHandler(db, _tokens, new CapturingSender(), Options())
                    .Handle(
                        new IssueInvitationCommand(
                            Invitee, WombatRoles.CollegeAdmin, null, collegeId, null, null, "admin-1", Administrator()),
                        CancellationToken.None);
                caller = Administrator();
            }
            else
            {
                issued = await IssueAsync(schema, new CapturingSender());
                await using var db = NewContext(schema);
                caller = InstitutionalAdmin(await db.Institutions.Select(institution => institution.Id).SingleAsync());
            }

            await AgeTheLinkAsync(schema);
            var sender = new CapturingSender();
            var resent = await ResendAsync(schema, issued.InvitationId, sender, caller);

            TokenIn(sender.Sent.Should().ContainSingle().Which).Should().Be(resent.Token);
            (await PreviewAsync(schema, resent.Token)).Email.Should().Be(Invitee);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A mail still queued as the app stops is never offered to the mail server. Its link reads as not delivered at once,
    /// so it can be resent, but it is not counted towards the address check: nothing says the address was at fault. So two
    /// restarts with the mail still queued never say the mail server may be refusing the address. (T283 review)
    /// </summary>
    [Fact]
    public async Task AMailStillQueuedAsTheAppStops_IsNotDelivered_ButIsNotCountedAgainstTheAddress()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var smtp = new ScriptedSmtp(works: true);

            var first = new EmailQueue();
            var issued = await IssueAsync(schema, Queued(first));
            await DrainAsync(schema, first, smtp, appStopping: true);

            var listed = (await ListAsync(schema)).Should().ContainSingle().Which;
            (listed.Delivery, listed.DeliveryFailures, listed.SuggestCheckingAddress)
                .Should().Be((InvitationDelivery.NotDelivered, 0, false));

            var second = new EmailQueue();
            await ResendAsync(schema, issued.InvitationId, Queued(second));
            await DrainAsync(schema, second, smtp, appStopping: true);

            smtp.Delivered.Should().BeEmpty("guard: neither mail was offered to the mail server");
            listed = (await ListAsync(schema)).Should().ContainSingle().Which;
            (listed.Delivery, listed.DeliveryFailures, listed.SuggestCheckingAddress)
                .Should().Be((InvitationDelivery.NotDelivered, 0, false));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The report ──────────────────────────────────────────────────────────

    /// <summary>
    /// A mail nothing was heard of for an hour reads as not delivered, and a resend replaces its link. If the first mail's
    /// report lands after all, it is about a link the invitation no longer holds, and changes nothing: the list speaks of
    /// the new link.
    /// </summary>
    [Fact]
    public async Task AReportAboutAReplacedLink_ChangesNothing_AndTheNewLinksReportCounts()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var firstSender = new CapturingSender();
            var issued = await IssueAsync(schema, firstSender);
            await AgeTheLinkAsync(schema);
            (await ListAsync(schema)).Single().Delivery.Should().Be(InvitationDelivery.NotDelivered);

            var secondSender = new CapturingSender();
            await ResendAsync(schema, issued.InvitationId, secondSender);

            await ReportAsync(schema, firstSender.Sent.Single(), sent: true);
            (await ListAsync(schema)).Single().Delivery.Should().Be(InvitationDelivery.BeingSent,
                "the report is about the link the resend retired");

            await ReportAsync(schema, secondSender.Sent.Single(), sent: false);
            var listed = (await ListAsync(schema)).Single();
            (listed.Delivery, listed.DeliveryFailures).Should().Be((InvitationDelivery.NotDelivered, 1));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ALinkIsReportedOnOnce_SoAFailureIsCountedOnce()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var sender = new CapturingSender();
            await IssueAsync(schema, sender);

            await ReportAsync(schema, sender.Sent.Single(), sent: false);
            await ReportAsync(schema, sender.Sent.Single(), sent: false);
            await ReportAsync(schema, sender.Sent.Single(), sent: true);

            var listed = (await ListAsync(schema)).Single();
            (listed.Delivery, listed.DeliveryFailures).Should().Be((InvitationDelivery.NotDelivered, 1));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The resend and the other writers of the row ─────────────────────────

    /// <summary>
    /// Another writer commits between the resend's read and its store: a second resend, an accept, a revoke, or the
    /// report that the first mail arrived after all. The store is conditioned on what the read found, so it stores nothing,
    /// no mail is handed over, and the refusal says the invitation changed. The resend runs under the audit pipeline, which
    /// saves the handler's context to write its row: nothing the resend did is committed with it.
    /// </summary>
    [Theory]
    [InlineData("resent elsewhere")]
    [InlineData("accepted")]
    [InlineData("revoked")]
    [InlineData("delivered after all")]
    public async Task AResendThatMeetsAnotherWriter_StoresNothing_AndSendsNothing(string writer)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var firstSender = new CapturingSender();
            var issued = await IssueAsync(schema, firstSender);
            await AgeTheLinkAsync(schema);
            var readHash = _tokens.HashToken(issued.Token);
            var otherHash = _tokens.HashToken("another-resend");

            Func<Task> competitor = writer switch
            {
                "resent elsewhere" => () => ExecuteAsync(schema,
                    $"""UPDATE "Invitations" SET "TokenHash" = '{otherHash}', "IssuedOn" = now()"""),
                "accepted" => () => ExecuteAsync(schema, """UPDATE "Invitations" SET "UsedOn" = now()"""),
                "revoked" => () => ExecuteAsync(schema, """UPDATE "Invitations" SET "RevokedOn" = now()"""),
                _ => () => ReportAsync(schema, firstSender.Sent.Single(), sent: true)
            };

            var sender = new CapturingSender();
            var act = () => ResendAsync(schema, issued.InvitationId, sender, new BeforeTheStore(competitor));

            (await act.Should().ThrowAsync<InvalidOperationException>())
                .Which.Message.Should().Be(ResendInvitationCommandHandler.InvitationChanged);
            sender.Sent.Should().BeEmpty();

            await using var db = NewContext(schema);
            var row = await db.Set<Invitation>().AsNoTracking().SingleAsync();
            row.TokenHash.Should().Be(writer == "resent elsewhere" ? otherHash : readHash, "the resend stored no link of its own");
            row.DeliveryFailedOn.Should().BeNull();
            (await db.Set<AuditEntry>().AsNoTracking().SingleAsync(entry => entry.Action == nameof(ResendInvitationCommand)))
                .Success.Should().BeFalse("the refusal is on the audit log");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The steps ───────────────────────────────────────────────────────────

    private async Task<IssuedInvitationResult> IssueAsync(string schema, IEmailSender sender)
    {
        await using (var seed = NewContext(schema))
        {
            if (!await seed.Institutions.AnyAsync())
            {
                seed.Institutions.Add(new Institution { Name = "Groote Schuur Hospital", ShortCode = "GSH", IsActive = true });
                await seed.SaveChangesAsync();
            }
        }

        await using var db = NewContext(schema);
        var institutionId = await db.Institutions.Select(institution => institution.Id).SingleAsync();
        return await new IssueInvitationCommandHandler(db, _tokens, sender, Options())
            .Handle(
                new IssueInvitationCommand(
                    Invitee, WombatRoles.InstitutionalAdmin, institutionId, null, null, null, "admin-1", Administrator()),
                CancellationToken.None);
    }

    private Task<IssuedInvitationResult> ResendAsync(
        string schema, int invitationId, IEmailSender sender, params IInterceptor[] interceptors)
        => ResendAsync(schema, invitationId, sender, Administrator(), interceptors);

    private async Task<IssuedInvitationResult> ResendAsync(
        string schema, int invitationId, IEmailSender sender, ClaimsPrincipal caller, params IInterceptor[] interceptors)
    {
        await using var db = NewContext(schema, interceptors);
        var command = new ResendInvitationCommand(invitationId, caller);
        var handler = new ResendInvitationCommandHandler(db, _tokens, sender, Options());

        return await new AuditPipelineBehavior<ResendInvitationCommand, IssuedInvitationResult>(
                new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    private static async Task<IReadOnlyList<ActiveInvitationDto>> ListAsync(string schema)
    {
        await using var db = NewContext(schema);
        return await new ListActiveInvitationsQueryHandler(db)
            .Handle(new ListActiveInvitationsQuery(Administrator()), CancellationToken.None);
    }

    private async Task<InvitationPreviewDto> PreviewAsync(string schema, string token)
    {
        await using var db = NewContext(schema);
        return await new GetInvitationPreviewQueryHandler(db, _tokens, new NoAccounts())
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);
    }

    /// <summary>The real recorder, on a context of its own, as the worker's scope runs it.</summary>
    private static async Task ReportAsync(string schema, EmailMessage message, bool sent)
    {
        await using var db = NewContext(schema);
        var outcome = sent
            ? EmailDeliveryOutcome.Delivered(1, DateTime.UtcNow)
            : EmailDeliveryOutcome.Dropped(3, DateTime.UtcNow);
        await new AccountInvitationDeliveryRecorder(db).RecordAsync(message, outcome, CancellationToken.None);
    }

    /// <summary>
    /// A real worker over everything queued, reporting through the real recorder; or, when <paramref name="appStopping" />,
    /// a worker whose host is already stopping, which offers nothing to the mail server and abandons what is queued.
    /// </summary>
    private static async Task DrainAsync(string schema, EmailQueue queue, ISmtpSender smtp, bool appStopping = false)
    {
        queue.Writer.Complete();

        var services = new ServiceCollection();
        services.AddSingleton(smtp);
        services.AddScoped<IApplicationDbContext>(_ => NewContext(schema));
        services.AddScoped<IEmailDeliveryObserver, AccountInvitationDeliveryRecorder>();
        await using var provider = services.BuildServiceProvider();

        var worker = new EmailWorker(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailWorker>.Instance,
            retryDelay: _ => TimeSpan.Zero);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        if (appStopping)
        {
            await timeout.CancelAsync();
        }

        await worker.ExecutePublicAsync(timeout.Token);
    }

    /// <summary>Nothing was heard of the link's mail, and an hour has passed: it reads as not delivered.</summary>
    private static Task AgeTheLinkAsync(string schema)
        => ExecuteAsync(schema, """UPDATE "Invitations" SET "IssuedOn" = "IssuedOn" - interval '2 hours'""");

    private static QueuedEmailSender Queued(EmailQueue queue) => new(queue, NullLogger<QueuedEmailSender>.Instance);

    private static string TokenIn(EmailMessage message)
    {
        var url = message.TextBody.Split('\n').Select(line => line.Trim()).Single(line => line.StartsWith(BaseUrl, StringComparison.Ordinal));
        return HttpUtility.ParseQueryString(new Uri(url).Query)["token"]!;
    }

    private static IOptions<WombatOptions> Options() => Microsoft.Extensions.Options.Options.Create(new WombatOptions { BaseUrl = BaseUrl });

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "inst-admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ─── Doubles ─────────────────────────────────────────────────────────────

    private sealed class CapturingSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>A mail server that takes every mail, or none.</summary>
    private sealed class ScriptedSmtp(bool works) : ISmtpSender
    {
        public List<EmailMessage> Delivered { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            if (!works)
            {
                throw new InvalidOperationException("The mail server could not be reached.");
            }

            Delivered.Add(message);
            return Task.CompletedTask;
        }
    }

    /// <summary>Runs another writer, once, just before this context's first <c>UPDATE</c> of an invitation goes out.</summary>
    private sealed class BeforeTheStore(Func<Task> competitor) : DbCommandInterceptor
    {
        private Func<Task>? _competitor = competitor;

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("UPDATE \"Invitations\"", StringComparison.Ordinal) &&
                Interlocked.Exchange(ref _competitor, null) is { } run)
            {
                await run();
            }

            return result;
        }
    }

    /// <summary>No account holds any address: the register page's preview asks only about the invitation.</summary>
    private sealed class NoAccounts : IInvitedUserProvisioner
    {
        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(InvitedAddressStatus.Available);

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email, string password, string firstName, string lastName, string targetRole, int? institutionId,
            int? collegeId, int? specialityId, int? subSpecialityId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
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

    // ─── Schema helpers ──────────────────────────────────────────────────────

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

    private static async Task<List<string>> OutcomeColumnsAsync(string schema)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(
            """
            SELECT column_name FROM information_schema.columns
            WHERE table_schema = current_schema() AND table_name = 'Invitations'
              AND column_name IN ('SentOn', 'DeliveryFailedOn', 'DeliveryFailures')
            ORDER BY column_name
            """,
            connection);
        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private static async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync())!;
    }

    private static async Task ExecuteAsync(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
