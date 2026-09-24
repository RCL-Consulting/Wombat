using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Activities;
using Wombat.Domain.Audit;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T131 review, on a real PostgreSQL server: staging, removing and ratifying at one review each check the review in
/// memory and then save, so two of them that overlap are settled by the save. Whichever saves second is refused whole,
/// with a sentence the chair can act on, and no staged decision is ever left on a ratified review.
/// </summary>
/// <remarks>
/// <para>
/// The review carries an <c>xmin</c> token (CommitteeReviewConfiguration), and staging marks the review modified, so a
/// stage and a ratify that both read the review before either saved cannot both commit. Without it a stage that read the
/// review as decided while a ratify committed added its row to the ratified review, and a stage that committed after a
/// ratify read the review was left behind by it: a staged row that Remove refuses (the review is no longer open) and
/// nothing will ever issue. EF InMemory has no <c>xmin</c>, so only Postgres can show this.
/// </para>
/// <para>
/// The overlap is made exact with a <see cref="SaveChangesInterceptor" /> on the first request's context: the second
/// request runs, from start to commit, just before the first one's save. Each runs inside the real
/// <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real <see cref="AuditWriter" /> on its
/// own context, as a request would, so the refused one's failure row is stored and nothing it tried to write is (T201).
/// The schema helpers follow <c>CommitteeEvidenceSnapshotPostgresTests</c>.
/// </para>
/// <para>
/// Since T165 the staged decisions are fixed when the committee's decision is recorded: staging and removing act only on a
/// review in progress (<see cref="StagedStars" />). So a stage or a remove that races a ratify is one that read the review
/// while it was still in progress, with the decision's recording and the ratify both committing before its save; and a
/// stage that reaches a decided review is refused by its state, before anything races.
/// </para>
/// </remarks>
public sealed class CommitteeReviewRacePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string TraineeUserId = "trainee-t131-race";
    private const string ChairUserId = "chair-t131-race";
    private const string MemberUserId = "member-t131-race";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task AStageThatReadTheReviewBeforeARatifyCommitted_IsRefusedWhole_AndLeavesNothingOnTheRatifiedReview()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync();

            // The stage reads the review while it is in progress; the decision is recorded and the review ratified, each
            // from start to commit; then the stage saves (T165: a decided review is never staged on).
            var stage = () => StageAsync(review, review.Paed002, review.Rung3a, [review.LineB],
                beforeSave: async () =>
                {
                    await RecordAsync(review);
                    await RatifyAsync(review);
                });

            var refusal = await stage.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(StagePendingEntrustmentDecisionCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await read.PendingEntrustmentDecisions.CountAsync())
                .Should().Be(0, "the refused stage's row must not be left on the ratified review, where nothing can remove it");
            (await read.EntrustmentDecisions.Select(star => star.EpaId).ToListAsync())
                .Should().Equal(new[] { review.Paed001 }, "the ratify issued what was staged when it read the review");
            (await AuditRowsAsync(review.Schema)).Should().BeEquivalentTo(
                "RatifyCommitteeDecisionCommand: succeeded",
                $"StagePendingEntrustmentDecisionCommand: {StagePendingEntrustmentDecisionCommandHandler.ReviewChanged}");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    public static TheoryData<string> StagesAttemptedMeanwhile => new() { "a new decision", "an edit to the staged one" };

    /// <summary>
    /// Before T165 a stage could commit on a decided review between a ratify's read and its save, and the ratify was then
    /// refused whole (ReviewChanged). Since T165 the staged decisions are fixed when the decision is recorded, so that
    /// stage is refused by the review's state and writes nothing, and the ratify issues exactly what was staged at the
    /// recording: not the new decision, and the staged one as it stood, not as the refused edit would have left it.
    /// </summary>
    [Theory]
    [MemberData(nameof(StagesAttemptedMeanwhile))]
    public async Task AStageAttemptedWhileARatifyIsInFlight_IsRefusedAsFixed_AndTheRatifyIssuesWhatWasStagedAtTheRecording(string stagedMeanwhile)
    {
        try
        {
            var review = await DecidedReviewWithPaed001StagedAsync();

            // The ratify reads the review and its staged decisions; the stage then runs from start to its refusal; then the
            // ratify saves.
            Func<Task> attempt = stagedMeanwhile == "a new decision"
                ? () => StageAsync(review, review.Paed002, review.Rung3a, [review.LineB])
                : () => StageAsync(review, review.Paed001, review.Rung3b, [review.LineA, review.LineB], review.PendingId);
            var ratify = () => RatifyAsync(review, beforeSave: async () =>
                (await attempt.Should().ThrowExactlyAsync<InvalidOperationException>())
                    .Which.Message.Should().Be(StagedStars.FixedWhenDecided));

            await ratify.Should().NotThrowAsync();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await read.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
            var star = await read.EntrustmentDecisions.Include(decision => decision.EvidenceLinks).SingleAsync();
            star.EpaId.Should().Be(review.Paed001);
            star.AuthorisedLevelId.Should().Be(review.Rung3a, "the refused edit changed nothing");
            star.EvidenceLinks.Select(link => link.CommitteeEvidenceId).Should().Equal(review.LineA);

            (await AuditRowsAsync(review.Schema)).Should().BeEquivalentTo(
                $"StagePendingEntrustmentDecisionCommand: {StagedStars.FixedWhenDecided}",
                "RatifyCommitteeDecisionCommand: succeeded");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task ARemoveThatLosesToARatify_SaysSo_AndTheRatifyStands()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync();

            // The remove reads the review while it is in progress; the decision is recorded and the review ratified, each
            // from start to commit; then the remove saves (T165: a decided review's staged decisions are fixed).
            var remove = () => RemoveAsync(review, review.PendingId, beforeSave: async () =>
            {
                await RecordAsync(review);
                await RatifyAsync(review);
            });

            var refusal = await remove.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(RemovePendingEntrustmentDecisionCommandHandler.AlreadyGone);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await read.EntrustmentDecisions.Select(star => star.EpaId).ToListAsync()).Should().Equal(review.Paed001);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// Check 7's race: a second decision on the EPA reaches the table after the stage's check read it. The unique index
    /// on (review, EPA) refuses the insert, and the chair is told which EPA, not EF's "see the inner exception".
    /// </summary>
    [Fact]
    public async Task ADecisionOnTheSameEpa_StoredBetweenTheCheckAndTheSave_IsRefusedByName()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync();

            // Written straight to the table, so the review itself is untouched and only the index stands in the way.
            var stage = () => StageAsync(review, review.Paed002, review.Rung3a, [review.LineB], beforeSave: async () =>
            {
                await using var other = NewContext(review.Schema);
                other.PendingEntrustmentDecisions.Add(PendingEntrustmentDecision.Stage(
                    review.ReviewId, review.Paed002, review.Rung3b, new DateOnly(2026, 7, 2), null, "The other chair's.",
                    [review.LineB], ChairUserId, DateTime.UtcNow));
                await other.SaveChangesAsync();
            });

            var refusal = await stage.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(StagePendingEntrustmentDecisionCommandHandler.AlreadyStaged("PAED-002"));
            refusal.Which.InnerException.Should().BeAssignableTo<DbUpdateException>()
                .Which.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);

            await using var read = NewContext(review.Schema);
            (await read.PendingEntrustmentDecisions.SingleAsync(pending => pending.EpaId == review.Paed002)).Rationale
                .Should().Be("The other chair's.");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// The token's one other consequence. Erasure pseudonymises a review's starter and ratifier with raw SQL and its
    /// trainee through the tracker; were the raw UPDATEs run after the review was loaded, they would move its xmin under
    /// the tracker and the erasure's save would be refused whole. The person erased here is all three.
    /// </summary>
    [Fact]
    public async Task ErasingAReviewsTraineeWhoAlsoStartedAndRatifiedIt_Completes()
    {
        var schema = await CreateSchemaAsync();

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(SchemaConnectionString(schema)));
            services.AddIdentity<WombatIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<ErasureExecutor>();

            await using var root = services.BuildServiceProvider();

            await using (var migrate = root.CreateAsyncScope())
            {
                await migrate.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
            }

            string userId;
            int reviewId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var users = arrange.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                var user = new WombatIdentityUser { UserName = "self@host.test", Email = "self@host.test", FirstName = "Sam", LastName = "Self" };
                (await users.CreateAsync(user)).Succeeded.Should().BeTrue();
                userId = user.Id;

                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "Host", ShortCode = "HST" };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();
                var review = new CommitteeReview
                {
                    Panel = new DecisionPanel
                    {
                        Name = "Erasure CCC",
                        Scope = DecisionPanelScope.Institution,
                        InstitutionId = institution.Id,
                        CreatedOn = DateTime.UtcNow
                    },
                    TraineeUserId = userId,
                    ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                    ReviewPeriodTo = new DateOnly(2026, 6, 30),
                    ScheduledOn = new DateOnly(2026, 7, 2)
                };
                review.Start([], userId, DateTime.UtcNow);
                // A quorate sitting, the chair and one other (T165); the chair is the person erased.
                review.RecordDecision(
                    CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, userId, DateTime.UtcNow,
                    [
                        new DecisionPanelMember { UserId = userId, Role = DecisionPanelMemberRole.Chair },
                        new DecisionPanelMember { UserId = "member-erasure", Role = DecisionPanelMemberRole.Member }
                    ]);
                review.Ratify(userId, DateTime.UtcNow);
                db.CommitteeReviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.Id;
            }

            await using (var act = root.CreateAsyncScope())
            {
                var request = DataRightsRequest.Create(userId, "Sam Self", DataRightsRequestType.Erasure, "Leaving.", DateTime.UtcNow);
                var db = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Set<DataRightsRequest>().Add(request);
                await db.SaveChangesAsync();

                await act.ServiceProvider.GetRequiredService<ErasureExecutor>().ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
            }

            await using (var assert = root.CreateAsyncScope())
            {
                var db = assert.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var erased = await db.CommitteeReviews.AsNoTracking().SingleAsync(entity => entity.Id == reviewId);
                erased.TraineeUserId.Should().StartWith("deleted_user_");
                erased.StartedByUserId.Should().Be(erased.TraineeUserId);
                erased.RatifiedByUserId.Should().Be(erased.TraineeUserId);
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ---- The requests, as a request runs them -------------------------------------------------------------------------

    private async Task StageAsync(
        SeededReview review, int epaId, int levelId, int[] evidenceItemIds, int? pendingId = null, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new StagePendingEntrustmentDecisionCommand(
            review.ReviewId, pendingId, epaId, levelId, new DateOnly(2026, 7, 2), null, "Consistent across the window.",
            evidenceItemIds, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new StagePendingEntrustmentDecisionCommandHandler(db).Handle(command, CancellationToken.None));
    }

    private async Task RatifyAsync(SeededReview review, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new RatifyCommitteeDecisionCommand(review.ReviewId, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new RatifyCommitteeDecisionCommandHandler(db).Handle(command, CancellationToken.None));
    }

    /// <summary>Records the committee's decision with a quorate sitting, the chair and the other member (T165).</summary>
    private async Task RecordAsync(SeededReview review, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new RecordCommitteeDecisionCommand(
            review.ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, [ChairUserId, MemberUserId], Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new RecordCommitteeDecisionCommandHandler(db, Committee(review)).Handle(command, CancellationToken.None));
    }

    /// <summary>The panel's two members, each an active committee member at its institution (T165, PanelSeat).</summary>
    private static FakeUserDirectory Committee(SeededReview review)
        => FakeUserDirectory.CommitteeMembersAt(review.HostId, ChairUserId, MemberUserId);

    private async Task RemoveAsync(SeededReview review, int pendingId, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new RemovePendingEntrustmentDecisionCommand(review.ReviewId, pendingId, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new RemovePendingEntrustmentDecisionCommandHandler(db).Handle(command, CancellationToken.None));
    }

    private static Task<TResponse> ThroughTheAuditPipelineAsync<TRequest, TResponse>(
        ApplicationDbContext db, TRequest command, Func<Task<TResponse>> handle)
        where TRequest : IRequest<TResponse>
        => new AuditPipelineBehavior<TRequest, TResponse>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handle(), CancellationToken.None);

    /// <summary>The raced requests' audit rows, as "action: outcome". The seeding ran its handlers without the pipeline.</summary>
    private async Task<IReadOnlyList<string>> AuditRowsAsync(string schema)
    {
        await using var read = NewContext(schema);
        var rows = await read.AuditEntries
            .AsNoTracking()
            .Where(entry => entry.Category == AuditCategory.Command
                && (entry.Action == "StagePendingEntrustmentDecisionCommand" || entry.Action == "RatifyCommitteeDecisionCommand"))
            .Select(entry => new { entry.Action, entry.Success, entry.ErrorMessage })
            .ToListAsync();

        return rows.Select(row => $"{row.Action}: {(row.Success ? "succeeded" : row.ErrorMessage)}").ToArray();
    }

    private static ClaimsPrincipal Chair()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ChairUserId),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ---- The review ---------------------------------------------------------------------------------------------------

    private sealed record SeededReview(
        string Schema, int ReviewId, int PendingId, int Paed001, int Paed002, int Rung3a, int Rung3b, int LineA, int LineB, int HostId);

    /// <summary>
    /// <see cref="InProgressReviewWithPaed001StagedAsync" />, with the committee's decision then recorded by a quorum,
    /// which fixes the staged decision (T165).
    /// </summary>
    private async Task<SeededReview> DecidedReviewWithPaed001StagedAsync()
    {
        var review = await InProgressReviewWithPaed001StagedAsync();

        await using (var db = NewContext(review.Schema))
        {
            await new RecordCommitteeDecisionCommandHandler(db, Committee(review)).Handle(
                new RecordCommitteeDecisionCommand(
                    review.ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, [ChairUserId, MemberUserId], Chair()),
                CancellationToken.None);
        }

        return review;
    }

    /// <summary>
    /// A review in progress on the v11.1 catalogue, its snapshot holding one completed CCA on PAED-001 (line A) and one
    /// on PAED-002 (line B), with a decision on PAED-001 staged on line A. Its panel is the chair and one member.
    /// </summary>
    private async Task<SeededReview> InProgressReviewWithPaed001StagedAsync()
    {
        var schema = await SeededSchemaAsync();
        int reviewId, paed001, paed002, rung3a, rung3b, activityA, activityB, hostId;

        await using (var db = NewContext(schema))
        {
            var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
            hostId = host;
            var curriculumId = await db.Curricula
                .Where(entity => entity.Name == "Paediatric EPA Curriculum" && entity.Version == "11.1")
                .Select(entity => entity.Id)
                .SingleAsync();
            paed001 = await NationalEpaAsync(db, "PAED-001");
            paed002 = await NationalEpaAsync(db, "PAED-002");
            var ladderId = await db.EntrustmentScales
                .Where(scale => scale.Name == "CPSA Paediatric Entrustment Scale v11.1")
                .Select(scale => scale.Id)
                .SingleAsync();
            rung3a = await db.EntrustmentLevels.Where(level => level.ScaleId == ladderId && level.Label == "3a").Select(level => level.Id).SingleAsync();
            rung3b = await db.EntrustmentLevels.Where(level => level.ScaleId == ladderId && level.Label == "3b").Select(level => level.Id).SingleAsync();
            var cca = await db.ActivityTypes.Where(type => type.Key == "cca_cpsa").Select(type => new { type.Id, type.Version }).SingleAsync();

            db.TraineeProfiles.Add(new TraineeProfile
            {
                UserId = TraineeUserId,
                InstitutionId = host,
                CurriculumId = curriculumId,
                ProgrammeStartDate = new DateOnly(2025, 1, 15),
                ExpectedCompletionDate = new DateOnly(2029, 1, 14)
            });

            var review = new CommitteeReview
            {
                Panel = new DecisionPanel
                {
                    Name = "T131 race CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = host,
                    CreatedOn = DateTime.UtcNow,
                    // The chair and one other: a committee decision needs a quorum (T165, D46).
                    Members =
                    [
                        new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
                        new DecisionPanelMember { UserId = MemberUserId, Role = DecisionPanelMemberRole.Member }
                    ]
                },
                TraineeUserId = TraineeUserId,
                ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                ReviewPeriodTo = new DateOnly(2026, 6, 30),
                ScheduledOn = new DateOnly(2026, 7, 2)
            };
            db.CommitteeReviews.Add(review);

            var a = CompletedCca(cca.Id, cca.Version, host, paed001, new DateOnly(2026, 2, 10));
            var b = CompletedCca(cca.Id, cca.Version, host, paed002, new DateOnly(2026, 3, 10));
            db.Activities.AddRange(a, b);
            await db.SaveChangesAsync();
            reviewId = review.Id;
            activityA = a.Id;
            activityB = b.Id;
        }

        await using (var db = NewContext(schema))
        {
            await new StartCommitteeReviewCommandHandler(db).Handle(new StartCommitteeReviewCommand(reviewId, Chair()), CancellationToken.None);
        }

        int lineA, lineB, pendingId;
        await using (var db = NewContext(schema))
        {
            lineA = await db.Set<CommitteeEvidence>().Where(line => line.ActivityId == activityA).Select(line => line.Id).SingleAsync();
            lineB = await db.Set<CommitteeEvidence>().Where(line => line.ActivityId == activityB).Select(line => line.Id).SingleAsync();

            var staged = await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
                new StagePendingEntrustmentDecisionCommand(
                    reviewId, null, paed001, rung3a, new DateOnly(2026, 7, 2), null, "Target met.", [lineA], Chair()),
                CancellationToken.None);
            pendingId = staged.Id;
        }

        return new SeededReview(schema, reviewId, pendingId, paed001, paed002, rung3a, rung3b, lineA, lineB, hostId);
    }

    private static Task<int> NationalEpaAsync(ApplicationDbContext db, string code)
        => db.Epas.Where(epa => epa.Code == code && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();

    private static Activity CompletedCca(int typeId, int version, int institutionId, int epaId, DateOnly observedOn)
    {
        var filed = observedOn.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        return new Activity
        {
            ActivityTypeId = typeId,
            SchemaVersion = version,
            InstitutionId = institutionId,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = TraineeUserId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{epaId.ToString(CultureInfo.InvariantCulture)}}, "assessor_user_id": "assessor-1", "observed_on": "{{observedOn:yyyy-MM-dd}}", "overall_level": 3 }""",
            EpaId = epaId,
            CreatedOn = filed,
            UpdatedOn = filed,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared
        };
    }

    /// <summary>Runs the other request once, just before this context's first save goes to the database.</summary>
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
        public string? UserId => ChairUserId;
        public string? UserDisplay => "Chair";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }

    // ---- Schema helpers (as CommitteeEvidenceSnapshotPostgresTests) ----------------------------------------------------

    private async Task<string> SeededSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

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

    /// <summary>A new, empty schema, registered for dropping before it exists so that no failure can leak it.</summary>
    private async Task<string> CreateSchemaAsync()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{schema}\"";
        await command.ExecuteNonQueryAsync();

        return schema;
    }

    /// <summary>Drops every schema this test created. Called from the test's finally and again from DisposeAsync.</summary>
    private async Task DropSchemasAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas.ToList())
        {
            // Belt and braces: this class only ever drops a schema it named itself.
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (beforeFirstSave is not null)
        {
            options.AddInterceptors(new BeforeFirstSave(beforeFirstSave));
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>MsfRespondEndpointFlowTests</c>.</summary>
    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }
}
