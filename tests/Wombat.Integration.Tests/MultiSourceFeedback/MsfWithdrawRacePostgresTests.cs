using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// A withdraw that another change to the campaign races is refused with a message a coordinator can act on, and stores
/// nothing. (T206 review)
/// </summary>
/// <remarks>
/// <para>
/// Withdrawing saves under the campaign's <c>xmin</c> token (<c>MsfCampaignConfiguration</c>), and since T206 adding an
/// invitee writes the campaign row too, as do opening and closing it. So a withdraw whose save comes after one of those
/// is refused by the server. Until the T206 review its refusal reached the campaign list as EF's own message ("The
/// database operation was expected to affect 1 row(s)…"), a link to Microsoft's documentation included.
/// </para>
/// <para>
/// EF InMemory has no <c>xmin</c>, so only Postgres can show this. The withdraw runs inside the real
/// <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real <see cref="AuditWriter" /> on its own
/// context, as a request would, so the refused changes meet the audit trap too (T201). The schema helpers follow
/// <c>MsfInviteDuringOpenRacePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class MsfWithdrawRacePostgresTests : IAsyncLifetime
{
    private const string LateInvitee = "peer-late@example.test";

    private static readonly string[] Respondents =
    [
        "nurse-1@example.test",
        "consultant-1@example.test"
    ];

    private readonly TestSchemas _schemas = new();
    private readonly InvitationTokenService _tokens = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task AWithdrawThatAnAddedInviteeRaced_IsRefusedSayingWhat_AndStoresNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema);

            // The withdraw has read the draft and anonymised it in memory; just before its save, the other tab's add runs
            // from start to save.
            var raceBeforeTheWithdrawsSave = new RaceBeforeFirstSave();
            raceBeforeTheWithdrawsSave.Arm(() => AddAsync(schema, campaignId, LateInvitee));

            var withdraw = () => WithdrawThroughTheAuditPipelineAsync(
                schema, campaignId, MsfCampaignState.Draft, raceBeforeTheWithdrawsSave);
            (await withdraw.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(WithdrawMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the add changed the campaign's xmin under the withdraw");

            raceBeforeTheWithdrawsSave.Ran.Should().BeTrue("guard: the add ran between the withdraw's read and its save");

            await using var read = NewContext(schema);
            var campaign = await read.MsfCampaigns
                .AsNoTracking()
                .Include(entity => entity.Invitations)
                .SingleAsync(entity => entity.Id == campaignId);

            campaign.State.Should().Be(MsfCampaignState.Draft, "the refused withdraw stored nothing");
            campaign.WithdrawnOn.Should().BeNull();
            campaign.Invitations.Select(invitation => invitation.RespondentEmail)
                .Should().BeEquivalentTo([.. Respondents, LateInvitee], "nobody was anonymised, and the add committed");
            campaign.Invitations.Should().OnlyContain(invitation => invitation.AnonymizedOn == null);

            (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(WithdrawMsfCampaignCommand))
                    .Select(entry => entry.Success)
                    .ToListAsync())
                .Should().Equal([false], "the refused withdraw leaves its failure row, written alone (T201)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T199: a campaign under review can be withdrawn, and so can be withdrawn and released from two tabs at once. The
    /// release saves first; the withdraw is refused in the coordinator's words, and the release stands: a report the
    /// trainee may already be reading is never taken back.
    /// </summary>
    [Fact]
    public async Task AWithdrawOfACampaignUnderReview_ThatAReleaseRaced_IsRefused_AndTheReleaseStands()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var campaignId = await SeedDraftCampaignAsync(schema, MsfCampaignState.UnderReview);

            var raceBeforeTheWithdrawsSave = new RaceBeforeFirstSave();
            raceBeforeTheWithdrawsSave.Arm(() => ReleaseAsync(schema, campaignId));

            var withdraw = () => WithdrawThroughTheAuditPipelineAsync(
                schema, campaignId, MsfCampaignState.UnderReview, raceBeforeTheWithdrawsSave);
            (await withdraw.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(WithdrawMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the release changed the campaign's xmin under the withdraw");

            raceBeforeTheWithdrawsSave.Ran.Should().BeTrue("guard: the release ran between the withdraw's read and its save");

            await using var read = NewContext(schema);
            var campaign = await read.MsfCampaigns.AsNoTracking().SingleAsync(entity => entity.Id == campaignId);

            campaign.State.Should().Be(MsfCampaignState.Released, "the refused withdraw stored nothing");
            campaign.ReleasedOn.Should().NotBeNull();
            campaign.WithdrawnOn.Should().BeNull();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T199 review: the other way round. A release whose save comes after a withdraw from another tab is refused in the
    /// coordinator's words, not EF's ("The database operation was expected to affect 1 row(s)…"), and the evidence it had
    /// staged goes with it: the audit pipeline finds the refused save inside the refusal and writes its own row alone
    /// (T201). The withdraw stands, and the trainee's record holds nothing from a campaign that was never released.
    /// </summary>
    [Fact]
    public async Task AReleaseThatAWithdrawRaced_IsRefusedSayingWhat_RecordsNoEvidence_AndTheWithdrawStands()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var (campaignId, coordinator) = await SeedReleasableCampaignAsync(schema);

            // The release has read the campaign and staged one evidence record per covered EPA; just before its save, the
            // other tab's withdraw runs from start to save.
            var raceBeforeTheReleasesSave = new RaceBeforeFirstSave();
            raceBeforeTheReleasesSave.Arm(() => WithdrawAsync(schema, campaignId, coordinator));

            var release = () => ReleaseThroughTheAuditPipelineAsync(schema, campaignId, coordinator, raceBeforeTheReleasesSave);
            (await release.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(ReleaseMsfCampaignCommandHandler.CampaignChanged)
                .WithInnerException<DbUpdateConcurrencyException>("the withdraw changed the campaign's xmin under the release");

            raceBeforeTheReleasesSave.Ran.Should().BeTrue("guard: the withdraw ran between the release's read and its save");
            raceBeforeTheReleasesSave.ActivitiesStaged.Should().Be(1, "guard: the release had staged its evidence record");

            await using var read = NewContext(schema);
            var campaign = await read.MsfCampaigns
                .AsNoTracking()
                .Include(entity => entity.CoveredEpas)
                .SingleAsync(entity => entity.Id == campaignId);

            campaign.State.Should().Be(MsfCampaignState.Withdrawn, "the withdraw committed, and the refused release stored nothing");
            campaign.ReleasedOn.Should().BeNull();
            campaign.EvidenceRecordedOn.Should().BeNull();
            campaign.CoveredEpas.Should().ContainSingle().Which.RecordedOn.Should().BeNull();
            (await read.Activities.AsNoTracking().CountAsync(activity => activity.SubjectUserId == ReleasedTraineeUserId))
                .Should().Be(0, "the staged evidence was discarded with the refused save");

            (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(ReleaseMsfCampaignCommand))
                    .Select(entry => entry.Success)
                    .ToListAsync())
                .Should().Equal([false], "the refused release leaves its failure row, written alone (T201)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ─── The commands, as a request runs them ────────────────────────────────

    private async Task WithdrawThroughTheAuditPipelineAsync(
        string schema, int campaignId, MsfCampaignState confirmed, IInterceptor interceptor)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new WithdrawMsfCampaignCommand(campaignId, confirmed, Administrator());
        var handler = new WithdrawMsfCampaignCommandHandler(db);

        await new AuditPipelineBehavior<WithdrawMsfCampaignCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await handler.Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    /// <summary>The release as the report page sends it, inside the real audit pipeline, on a context of its own.</summary>
    private async Task ReleaseThroughTheAuditPipelineAsync(
        string schema, int campaignId, ClaimsPrincipal coordinator, IInterceptor interceptor)
    {
        await using var db = NewContext(schema, interceptor);
        var command = new ReleaseMsfCampaignCommand(
            campaignId, ReleasingCoordinatorId, "Colleagues describe a careful registrar.", EntrustmentLevel: null, coordinator);
        var handler = new ReleaseMsfCampaignCommandHandler(
            db,
            new MsfAggregationService(),
            new ActivityService(
                db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(),
                TimeProvider.System, new EpaCreditLock(db)),
            new ActivityReferenceDataService(db),
            NullLogger<ReleaseMsfCampaignCommandHandler>.Instance);

        await new AuditPipelineBehavior<ReleaseMsfCampaignCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await handler.Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    /// <summary>The other tab's withdraw, of the campaign its page showed under review.</summary>
    private async Task WithdrawAsync(string schema, int campaignId, ClaimsPrincipal coordinator)
    {
        await using var db = NewContext(schema);
        await new WithdrawMsfCampaignCommandHandler(db).Handle(
            new WithdrawMsfCampaignCommand(campaignId, MsfCampaignState.UnderReview, coordinator),
            CancellationToken.None);
    }

    private async Task AddAsync(string schema, int campaignId, string email)
    {
        await using var db = NewContext(schema);
        await new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees("trainee-1")).Handle(
            new AddMsfInvitationCommand(campaignId, email, MsfRespondentCategory.PeerDoctor, Administrator()),
            CancellationToken.None);
    }

    /// <summary>
    /// The row a release writes: the state, who released it and when, as <see cref="MsfCampaign.Release" /> sets them.
    /// The release's evidence rows are its own business and have no bearing on the campaign row's token.
    /// </summary>
    private async Task ReleaseAsync(string schema, int campaignId)
    {
        await using var db = NewContext(schema);
        var campaign = await db.MsfCampaigns.SingleAsync(entity => entity.Id == campaignId);
        campaign.Release("coordinator-1", "Released from another tab.", entrustmentLevel: null, DateTime.UtcNow);
        await db.SaveChangesAsync();
    }

    /// <param name="state">
    /// Draft by default. A campaign under review has closed, which removed every address (MsfCampaign.Close).
    /// </param>
    private async Task<int> SeedDraftCampaignAsync(string schema, MsfCampaignState state = MsfCampaignState.Draft)
    {
        await using var db = NewContext(schema);
        await CurrentTraineeSeed.AdmitAsync(db, "trainee-1");
        var closed = state == MsfCampaignState.UnderReview;
        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            State = state,
            OpenedOn = state == MsfCampaignState.Draft ? null : DateTime.UtcNow.AddDays(-14),
            ClosedOn = closed ? DateTime.UtcNow.AddDays(-1) : null,
            Template = new MsfTemplate { Name = "T206 review MSF" },
            Invitations = Respondents
                .Select(email => new MsfInvitation
                {
                    RespondentEmail = closed ? null : email,
                    AnonymizedOn = closed ? DateTime.UtcNow.AddDays(-1) : null,
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

    private const string ReleasedTraineeUserId = "trainee-release";

    private const string ReleasingCoordinatorId = "coordinator-1";

    /// <summary>The College's v11.1 curriculum, as <c>PaediatricCatalogueSeeder</c> names it.</summary>
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    /// <summary>
    /// A campaign a release would record evidence from: its trainee admitted on the College's paediatric curriculum at
    /// the institution <c>DataSeeder</c> creates, one EPA that multi-source feedback may cover, and four responses from two
    /// respondent groups, which clears the release gates. Returned with the coordinator who runs it: the evidence type's
    /// <c>record</c> transition is <c>role:Coordinator</c>'s.
    /// </summary>
    private async Task<(int CampaignId, ClaimsPrincipal Coordinator)> SeedReleasableCampaignAsync(string schema)
    {
        await using var db = NewContext(schema);

        var institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        var curriculumId = await db.Curricula.Where(entity => entity.Name == PaediatricCurriculumName).Select(entity => entity.Id).SingleAsync();

        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = ReleasedTraineeUserId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3)),
            IsActive = true
        });
        await db.SaveChangesAsync();

        // An EPA the release will not drop: the options it narrows the covered EPAs by itself.
        var coverable = await new ActivityReferenceDataService(db).GetSubjectCurriculumEpaOptionsAsync(
            ReleasedTraineeUserId, MsfEvidenceKinds.CoverageToolKeyFor(MsfTemplateKind.Msf), CancellationToken.None);
        coverable.Should().NotBeEmpty("guard: multi-source feedback may cover an EPA on this curriculum");
        var epaId = int.Parse(coverable[0].Value, CultureInfo.InvariantCulture);

        var question = new MsfQuestion { Order = 1, Prompt = "Professional performance", Type = MsfQuestionType.Scale, Required = true };
        var campaign = new MsfCampaign
        {
            SubjectUserId = ReleasedTraineeUserId,
            CreatedByUserId = ReleasingCoordinatorId,
            CreatedOn = DateTime.UtcNow.AddDays(-30),
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2),
            MinimumResponses = 4,
            MinimumCategoryResponses = 2,
            MinimumRespondentCategories = 2,
            State = MsfCampaignState.UnderReview,
            OpenedOn = DateTime.UtcNow.AddDays(-30),
            ClosedOn = DateTime.UtcNow.AddDays(-2),
            Template = new MsfTemplate { Name = "T199 review MSF", Questions = [question] },
            CoveredEpas = [new MsfCampaignEpa { EpaId = epaId }]
        };

        MsfRespondentCategory[] categories =
        [
            MsfRespondentCategory.Consultant, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse, MsfRespondentCategory.Nurse
        ];
        foreach (var category in categories)
        {
            var invitation = new MsfInvitation
            {
                RespondentCategory = category,
                TokenHash = _tokens.HashToken(_tokens.GenerateToken()),
                IssuedOn = DateTime.UtcNow.AddDays(-30),
                ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2),
                RespondedOn = DateTime.UtcNow.AddDays(-5),
                AnonymizedOn = DateTime.UtcNow.AddDays(-2)
            };
            var response = new MsfResponse
            {
                SubmittedOn = DateTime.UtcNow.AddDays(-5),
                Campaign = campaign,
                Answers = [new MsfResponseAnswer { Question = question, ScaleValue = 4 }]
            };
            invitation.Responses.Add(response);
            campaign.Invitations.Add(invitation);
            campaign.Responses.Add(response);
        }

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();

        var coordinator = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ReleasingCoordinatorId),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

        return (campaign.Id, coordinator);
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

    /// <summary>Runs the competing request once, just before the first save on the context it is attached to.</summary>
    private sealed class RaceBeforeFirstSave : SaveChangesInterceptor
    {
        private Func<Task>? _armed;

        public bool Ran { get; private set; }

        /// <summary>How many activities the intercepted save was about to insert: a release's staged evidence.</summary>
        public int ActivitiesStaged { get; private set; }

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
                ActivitiesStaged = eventData.Context?.ChangeTracker.Entries<Activity>()
                    .Count(entry => entry.State == EntityState.Added) ?? 0;
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

    // ─── Schema helpers (as MsfInviteDuringOpenRacePostgresTests) ────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    /// <summary>A migrated schema holding what the Web host seeds at startup: the demo institution and the v11.1 catalogue.</summary>
    private async Task<string> SeededSchemaAsync()
    {
        var schema = await MigratedSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

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
