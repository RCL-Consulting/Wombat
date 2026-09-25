using System.Globalization;
using System.Security.Claims;
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
    private const string TraineeUserId = "trainee-t131-race";
    private const string ChairUserId = "chair-t131-race";
    private const string MemberUserId = "member-t131-race";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

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
            await _schemas.DropAllAsync();
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
            await _schemas.DropAllAsync();
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
            refusal.Which.Message.Should().Be(RemovePendingEntrustmentDecisionCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await read.EntrustmentDecisions.Select(star => star.EpaId).ToListAsync()).Should().Equal(review.Paed001);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T213 item 1. The staged STARs are fixed when the committee's decision is recorded (T165, D46), and a remove checks
    /// the review's state only when it reads it. Before T213 it did not mark the review modified, so one that read the
    /// review in progress deleted the staged decision after the decision was recorded, and ratifying then issued less than
    /// the panel recorded. Now the remove's save checks the review's xmin and is refused whole.
    /// </summary>
    [Fact]
    public async Task ARemoveThatReadTheReviewBeforeARecordCommitted_IsRefusedWhole_AndTheRecordedStarStaysStaged()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync();

            // The remove reads the review while it is in progress; the decision is recorded from start to commit; then the
            // remove saves.
            var remove = () => RemoveAsync(review, review.PendingId, beforeSave: () => RecordAsync(review));

            var refusal = await remove.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(RemovePendingEntrustmentDecisionCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using (var read = NewContext(review.Schema))
            {
                (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                    .Should().Be(CommitteeReviewState.Decided);
                (await read.PendingEntrustmentDecisions.Select(pending => pending.Id).ToListAsync())
                    .Should().Equal(new[] { review.PendingId }, "the decision recorded with it is the committee's, and fixed");
            }

            (await AuditRowsOfAsync(review.Schema, nameof(RecordCommitteeDecisionCommand), nameof(RemovePendingEntrustmentDecisionCommand)))
                .Should().BeEquivalentTo(
                    "RecordCommitteeDecisionCommand: succeeded",
                    $"RemovePendingEntrustmentDecisionCommand: {RemovePendingEntrustmentDecisionCommandHandler.ReviewChanged}");

            // And the ratify issues what was recorded.
            await RatifyAsync(review);
            await using var after = NewContext(review.Schema);
            (await after.EntrustmentDecisions.Select(star => star.EpaId).ToListAsync()).Should().Equal(review.Paed001);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T213 item 1, the other way round: a record that read the review, and the decision staged on it, is refused whole
    /// when a remove commits before its save. Otherwise the decision would fix a staged set other than the one the chair
    /// saw when recording it.
    /// </summary>
    [Fact]
    public async Task ARecordThatReadTheReviewBeforeARemoveCommitted_IsRefusedWhole_WithAReadableRefusal()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync();

            var record = () => RecordAsync(review, beforeSave: () => RemoveAsync(review, review.PendingId));

            var refusal = await record.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(RecordCommitteeDecisionCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.InProgress);
            (await read.CommitteeDecisions.CountAsync()).Should().Be(0, "nothing of the refused record is stored");
            (await read.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
            (await AuditRowsOfAsync(review.Schema, nameof(RecordCommitteeDecisionCommand), nameof(RemovePendingEntrustmentDecisionCommand)))
                .Should().BeEquivalentTo(
                    "RemovePendingEntrustmentDecisionCommand: succeeded",
                    $"RecordCommitteeDecisionCommand: {RecordCommitteeDecisionCommandHandler.ReviewChanged}");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T213 item 2. A decision staged between a record's read and its save moves the review's xmin (staging marks the
    /// review modified), so the record is refused whole. Before T213 the chair was shown EF's own concurrency message; now
    /// the refusal says what happened and what to do, as staging's and ratifying's do.
    /// </summary>
    [Fact]
    public async Task ARecordThatReadTheReviewBeforeAStageCommitted_IsRefusedWhole_WithAReadableRefusal()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync();

            var record = () => RecordAsync(review, beforeSave: () => StageAsync(review, review.Paed002, review.Rung3a, [review.LineB]));

            var refusal = await record.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(RecordCommitteeDecisionCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using (var read = NewContext(review.Schema))
            {
                (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                    .Should().Be(CommitteeReviewState.InProgress, "the review is still open, so the chair can record again");
                (await read.CommitteeDecisions.CountAsync()).Should().Be(0);
                (await read.CommitteeDecisionAttendees.CountAsync()).Should().Be(0);
                (await read.PendingEntrustmentDecisions.Select(pending => pending.EpaId).ToListAsync())
                    .Should().BeEquivalentTo(new[] { review.Paed001, review.Paed002 });
            }

            (await AuditRowsOfAsync(review.Schema, nameof(RecordCommitteeDecisionCommand), nameof(StagePendingEntrustmentDecisionCommand)))
                .Should().BeEquivalentTo(
                    "StagePendingEntrustmentDecisionCommand: succeeded",
                    $"RecordCommitteeDecisionCommand: {RecordCommitteeDecisionCommandHandler.ReviewChanged}");

            // The chair records again, and the decision now fixes both staged decisions.
            await RecordAsync(review);
            await using var after = NewContext(review.Schema);
            (await after.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Decided);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T131 slice 4: a deferral is how a closing line is let through ratify, and like a staged decision it is part of the
    /// decision the panel records, so it is fixed then (T165). Deferring and reinstating act only on a review in progress,
    /// and mark the review modified, so its xmin is checked with the line: one that read the review while it was in
    /// progress is refused whole if the decision was recorded, and the review ratified, before it saved. Without that, a
    /// ratify would commit around a reinstatement, leaving a ratified review with a closing line still due, or a line of a
    /// ratified review would be changed. And one attempted once the decision is recorded is refused by the review's state
    /// before anything races, so the ratify closes the agenda as it was recorded.
    /// </summary>
    [Fact]
    public async Task AReinstatementAttemptedWhileARatifyIsInFlight_IsRefusedAsFixed_AndTheRatifyClosesTheAgendaAsRecorded()
    {
        try
        {
            var review = await DecidedReviewWithPaed001StagedAsync(everyClosingLineDeferred: true);

            var ratify = () => RatifyAsync(review, beforeSave: async () =>
                (await ReinstatingPaed002(review).Should().ThrowExactlyAsync<InvalidOperationException>())
                    .Which.Message.Should().Be(DeferAgendaLineCommandHandler.FixedWhenDecided));

            await ratify.Should().NotThrowAsync();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await read.EntrustmentDecisions.Select(star => star.EpaId).ToListAsync()).Should().Equal(review.Paed001);
            (await LineStateAsync(read, review.Paed002Line))
                .Should().Be((CommitteeAgendaLineState.Deferred, Deferral), "the refused reinstatement changed nothing");
            (await AuditRowsAsync(review.Schema)).Should().BeEquivalentTo(
                $"ReinstateAgendaLineCommand: {DeferAgendaLineCommandHandler.FixedWhenDecided}",
                "RatifyCommitteeDecisionCommand: succeeded");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AReinstateThatReadTheReviewBeforeARatifyCommitted_IsRefusedWhole()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync(everyClosingLineDeferred: true);

            // The reinstatement reads the review while it is in progress; the decision is recorded and the review
            // ratified, each from start to commit; then the reinstatement saves.
            var reinstate = () => ReinstateAsync(review, review.Paed002Line, beforeSave: async () =>
            {
                await RecordAsync(review);
                await RatifyAsync(review);
            });

            var refusal = await reinstate.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(DeferAgendaLineCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await LineStateAsync(read, review.Paed002Line))
                .Should().Be((CommitteeAgendaLineState.Deferred, Deferral), "a line of a ratified review is not reinstated");
            (await AuditRowsAsync(review.Schema)).Should().BeEquivalentTo(
                "RatifyCommitteeDecisionCommand: succeeded",
                $"ReinstateAgendaLineCommand: {DeferAgendaLineCommandHandler.ReviewChanged}");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ADeferralAttemptedWhileARatifyIsInFlight_IsRefusedAsFixed_AndTheRatifyClosesTheLine()
    {
        try
        {
            var review = await DecidedReviewWithPaed001StagedAsync(everyClosingLineDeferred: true);

            // PAED-003 is annual, so optional at this semester-1 sitting: the ratify closes it Not decided.
            var ratify = () => RatifyAsync(review, beforeSave: async () =>
                (await DeferringPaed003(review).Should().ThrowExactlyAsync<InvalidOperationException>())
                    .Which.Message.Should().Be(DeferAgendaLineCommandHandler.FixedWhenDecided));

            await ratify.Should().NotThrowAsync();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await read.EntrustmentDecisions.Select(star => star.EpaId).ToListAsync()).Should().Equal(review.Paed001);
            (await LineStateAsync(read, review.Paed003Line))
                .Should().Be((CommitteeAgendaLineState.NotDecided, null), "the refused deferral changed nothing");
            (await AuditRowsAsync(review.Schema)).Should().BeEquivalentTo(
                $"DeferAgendaLineCommand: {DeferAgendaLineCommandHandler.FixedWhenDecided}",
                "RatifyCommitteeDecisionCommand: succeeded");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ADeferralThatReadTheReviewBeforeARatifyCommitted_IsRefusedWhole()
    {
        try
        {
            var review = await InProgressReviewWithPaed001StagedAsync(everyClosingLineDeferred: true);

            // The deferral reads the review while it is in progress; the decision is recorded and the review ratified,
            // each from start to commit; then the deferral saves.
            var defer = () => DeferAsync(review, review.Paed003Line, beforeSave: async () =>
            {
                await RecordAsync(review);
                await RatifyAsync(review);
            });

            var refusal = await defer.Should().ThrowExactlyAsync<InvalidOperationException>();
            refusal.Which.Message.Should().Be(DeferAgendaLineCommandHandler.ReviewChanged);
            refusal.Which.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();

            await using var read = NewContext(review.Schema);
            (await read.CommitteeReviews.SingleAsync(entity => entity.Id == review.ReviewId)).State
                .Should().Be(CommitteeReviewState.Ratified);
            (await LineStateAsync(read, review.Paed003Line))
                .Should().Be((CommitteeAgendaLineState.NotDecided, null), "the ratify closed it, and nothing defers it after");
            (await AuditRowsAsync(review.Schema)).Should().BeEquivalentTo(
                "RatifyCommitteeDecisionCommand: succeeded",
                $"DeferAgendaLineCommand: {DeferAgendaLineCommandHandler.ReviewChanged}");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private Func<Task> ReinstatingPaed002(SeededReview review) => () => ReinstateAsync(review, review.Paed002Line);

    private Func<Task> DeferringPaed003(SeededReview review) => () => DeferAsync(review, review.Paed003Line);

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
            await _schemas.DropAllAsync();
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
        var schema = await _schemas.CreateAsync();

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
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
                    AcademicYear = 2026,
                    Semester = 1,
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
                    ], [], []);
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
            await _schemas.DropAllAsync();
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
        await ThroughTheAuditPipelineAsync(db, command, () => new StagePendingEntrustmentDecisionCommandHandler(db, Committee(review)).Handle(command, CancellationToken.None));
    }

    private async Task RatifyAsync(SeededReview review, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new RatifyCommitteeDecisionCommand(review.ReviewId, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new RatifyCommitteeDecisionCommandHandler(db, Committee(review)).Handle(command, CancellationToken.None));
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

    private async Task DeferAsync(SeededReview review, int lineId, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new DeferAgendaLineCommand(review.ReviewId, lineId, Deferral, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new DeferAgendaLineCommandHandler(db, Committee(review)).Handle(command, CancellationToken.None));
    }

    private async Task ReinstateAsync(SeededReview review, int lineId, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new ReinstateAgendaLineCommand(review.ReviewId, lineId, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new ReinstateAgendaLineCommandHandler(db, Committee(review)).Handle(command, CancellationToken.None));
    }

    private static async Task<(CommitteeAgendaLineState State, string? Reason)> LineStateAsync(ApplicationDbContext db, int lineId)
    {
        var line = await db.CommitteeAgendaLines.AsNoTracking().SingleAsync(entity => entity.Id == lineId);
        return (line.State, line.DeferralReason);
    }

    private async Task RemoveAsync(SeededReview review, int pendingId, Func<Task>? beforeSave = null)
    {
        await using var db = NewContext(review.Schema, beforeSave);
        var command = new RemovePendingEntrustmentDecisionCommand(review.ReviewId, pendingId, Chair());
        await ThroughTheAuditPipelineAsync(db, command, () => new RemovePendingEntrustmentDecisionCommandHandler(db, Committee(review)).Handle(command, CancellationToken.None));
    }

    private static Task<TResponse> ThroughTheAuditPipelineAsync<TRequest, TResponse>(
        ApplicationDbContext db, TRequest command, Func<Task<TResponse>> handle)
        where TRequest : IRequest<TResponse>
        => new AuditPipelineBehavior<TRequest, TResponse>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => handle(), CancellationToken.None);

    /// <summary>The raced requests' audit rows, as "action: outcome". The seeding ran its handlers without the pipeline.</summary>
    private Task<IReadOnlyList<string>> AuditRowsAsync(string schema)
        => AuditRowsOfAsync(
            schema,
            nameof(StagePendingEntrustmentDecisionCommand), nameof(RatifyCommitteeDecisionCommand),
            nameof(DeferAgendaLineCommand), nameof(ReinstateAgendaLineCommand));

    /// <summary>The audit rows of these commands, as "action: outcome".</summary>
    private async Task<IReadOnlyList<string>> AuditRowsOfAsync(string schema, params string[] actions)
    {
        await using var read = NewContext(schema);
        var rows = await read.AuditEntries
            .AsNoTracking()
            .Where(entry => entry.Category == AuditCategory.Command && actions.Contains(entry.Action))
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

    /// <summary>The reason every deferral here gives.</summary>
    private const string Deferral = "Not observed this semester.";

    /// <param name="Paed002Line">The agenda line on PAED-002: closing, and deferred when every closing line is.</param>
    /// <param name="Paed003Line">The agenda line on PAED-003: annual, so optional at this semester-1 sitting, and due.</param>
    private sealed record SeededReview(
        string Schema, int ReviewId, int PendingId, int Paed001, int Paed002, int Rung3a, int Rung3b, int LineA, int LineB,
        int HostId, int Paed002Line, int Paed003Line);

    /// <summary>
    /// <see cref="InProgressReviewWithPaed001StagedAsync" />, with the committee's decision then recorded by a quorum,
    /// which fixes the staged decision (T165).
    /// </summary>
    /// <param name="everyClosingLineDeferred">
    /// The trainee started on time, so the semester EPAs' lines are closing, and every one but PAED-001's is deferred
    /// before the decision is recorded, as recording demands (T131 slice 4): the review can be ratified.
    /// </param>
    private async Task<SeededReview> DecidedReviewWithPaed001StagedAsync(bool everyClosingLineDeferred = false)
    {
        var review = await InProgressReviewWithPaed001StagedAsync(everyClosingLineDeferred);

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
    /// <param name="everyClosingLineDeferred">
    /// The trainee started on time, so the semester EPAs' lines are closing, and every one but PAED-001's is deferred
    /// while the review is in progress: the decision can be recorded, and the review ratified (T131 slice 4). Otherwise
    /// the trainee joined part-way through the semester, and no line is closing.
    /// </param>
    private async Task<SeededReview> InProgressReviewWithPaed001StagedAsync(bool everyClosingLineDeferred = false)
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

            // Joined on 1 February, after semester 1's first month, so the review's semester lines are a partial period
            // (T131 slice 4, Decision 9): on the agenda, optional, and never in the way of a ratify. These tests race
            // staging against ratifying; the closing-line rule has tests of its own. The deferral races need closing
            // lines, so their trainee started the year before.
            db.TraineeProfiles.Add(new TraineeProfile
            {
                UserId = TraineeUserId,
                InstitutionId = host,
                CurriculumId = curriculumId,
                ProgrammeStartDate = everyClosingLineDeferred ? new DateOnly(2025, 1, 15) : new DateOnly(2026, 2, 1),
                ExpectedCompletionDate = new DateOnly(2029, 1, 14)
            });

            var review = new CommitteeReview
            {
                AcademicYear = 2026,
                Semester = 1,
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
            await new StartCommitteeReviewCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(new StartCommitteeReviewCommand(reviewId, Chair()), CancellationToken.None);
        }

        int lineA, lineB, pendingId;
        await using (var db = NewContext(schema))
        {
            lineA = await db.Set<CommitteeEvidence>().Where(line => line.ActivityId == activityA).Select(line => line.Id).SingleAsync();
            lineB = await db.Set<CommitteeEvidence>().Where(line => line.ActivityId == activityB).Select(line => line.Id).SingleAsync();

            var staged = await new StagePendingEntrustmentDecisionCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                new StagePendingEntrustmentDecisionCommand(
                    reviewId, null, paed001, rung3a, new DateOnly(2026, 7, 2), null, "Target met.", [lineA], Chair()),
                CancellationToken.None);
            pendingId = staged.Id;
        }

        int paed002Line, paed003Line;
        await using (var db = NewContext(schema))
        {
            paed002Line = await AgendaLineOfAsync(db, reviewId, paed002);
            paed003Line = await AgendaLineOfAsync(db, reviewId, await NationalEpaAsync(db, "PAED-003"));

            // While the review is in progress: a deferral is part of the decision the panel records, and fixed with it.
            if (everyClosingLineDeferred)
            {
                var closing = await db.CommitteeAgendaLines
                    .Where(line => line.ReviewId == reviewId && line.IsClosing && line.EpaId != paed001)
                    .Select(line => line.Id)
                    .ToListAsync();
                closing.Should().Contain(paed002Line);

                foreach (var lineId in closing)
                {
                    await new DeferAgendaLineCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
                        new DeferAgendaLineCommand(reviewId, lineId, Deferral, Chair()), CancellationToken.None);
                    db.ChangeTracker.Clear();
                }
            }
        }

        return new SeededReview(
            schema, reviewId, pendingId, paed001, paed002, rung3a, rung3b, lineA, lineB, hostId, paed002Line, paed003Line);
    }

    private static Task<int> AgendaLineOfAsync(ApplicationDbContext db, int reviewId, int epaId)
        => db.CommitteeAgendaLines.Where(line => line.ReviewId == reviewId && line.EpaId == epaId).Select(line => line.Id).SingleAsync();

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
        var schema = await _schemas.CreateAsync();

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

    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (beforeFirstSave is not null)
        {
            options.AddInterceptors(new BeforeFirstSave(beforeFirstSave));
        }

        return new ApplicationDbContext(options.Options);
    }
}
