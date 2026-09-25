using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T119: the rebuild is the only thing in the product that can move an already-stored tally, so wiring
/// the encounter date is what finally makes it necessary to run — and it was not safe to run.
/// </summary>
/// <remarks>
/// <para>
/// The defect these tests exist for: the handler deleted every <c>CurriculumItemProgress</c> row and
/// SAVED, then replayed and saved separately. Anything going wrong in between — a failure, a
/// cancellation, a deadlock — left every trainee in the system on zero progress with no way back. The
/// repair is to zero the rows in place, replay into them, and commit once.
/// </para>
/// <para>
/// The counters are also the second half of T106 item 12: a rebuild that re-credits an activity must
/// refresh the transition's <c>CreditedItemCount</c>, or T108's "this credited nothing" banner survives
/// the exact remediation it recommends.
/// </para>
/// </remarks>
public sealed class RebuildCurriculumProgressTests
{
    private const int CreditingTypeId = 100;
    private const int NonCreditingTypeId = 101;
    private const int CreditedEpaId = 5000;
    private const int UncreditedEpaId = 6000;
    private const int NationalItemId = 4000;
    private const int OrphanItemId = 4001;
    private const int CurriculumId = 3000;

    /// <summary>
    /// A fixed "now". The dates below used to be relative to DateTime.UtcNow, which T130 made a hazard: two
    /// activities 100 and 50 days ago land in one semester on some run dates and in two on others, so the
    /// same test asserted a single row on some days and threw on the rest.
    /// </summary>
    private static readonly DateTime Now = new(2026, 9, 23, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Rebuild_ThatThrowsPartWay_LeavesThePriorRowsIntact()
    {
        // The whole reason this handler was unsafe. trainee-2 already has a stored tally; trainee-1 has
        // none yet, so the replay must ADD a row for them before it reaches the failure. Both halves of
        // the rollback are therefore exercised: the zeroed row has to come back, and the added row has
        // to disappear.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-2", score: 4, daysAgo: 50);

            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 900,
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-2",
                // Activity 201's semester: filed and observed 50 days before Now, 2026-08-04.
                AcademicYear = 2026,
                Semester = 2,
                CountsSoFar = 7,
                MinimumLevelReachedCount = 5,
                LastActivityId = 201,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                CreditedActivityKeysJson = """["201:complete"]"""
            });

            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            // Activity 201 (trainee-2) is replayed second, because the replay runs in filing order.
            var handler = new RebuildCurriculumProgressCommandHandler(
                db, new ThrowsOnCall(new CreditApplier(db), failOnCall: 2));

            var rebuild = async () => await handler.Handle(
                new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

            await rebuild.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");

            // The context must also be left clean: it is scoped to the request, and a half-finished
            // rebuild that left zeroed rows sitting in the change tracker would simply move the defect
            // to whoever calls SaveChanges next.
            await db.SaveChangesAsync();
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var rows = await verify.CurriculumItemProgresses.ToListAsync();

            rows.Should().ContainSingle("the failed rebuild must not have added trainee-1's row");

            var trainee2 = rows.Single();
            trainee2.TraineeUserId.Should().Be("trainee-2");
            trainee2.CountsSoFar.Should().Be(7);
            trainee2.MinimumLevelReachedCount.Should().Be(5);
            trainee2.CreditedActivityKeysJson.Should().Be("""["201:complete"]""");
        }
    }

    [Fact]
    public async Task Rebuild_StampsCreditedItemCountOnTheTransitionThatCreditedIt()
    {
        // T106 item 12: the activity was completed while the curriculum was wrong, so T108 stamped a
        // zero and ActivityView has been telling the reader to fix the curriculum and rebuild ever
        // since. The rebuild is that remediation; if it does not refresh the stamp, the banner stays on
        // for ever.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            var activity = AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            Completion(activity).CreditedItemCount = 0;
            Completion(activity).CreditScaleMismatchCount = 0;
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);

            result.TransitionsStamped.Should().Be(1);
            result.ProgressRowsWritten.Should().Be(1);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var transitions = await verify.ActivityTransitions.Where(t => t.ActivityId == 200).ToListAsync();

            transitions.Single(t => t.TransitionKey == "complete").CreditedItemCount.Should().Be(1);
            transitions.Single(t => t.TransitionKey == "complete").CreditScaleMismatchCount.Should().Be(0);

            transitions.Single(t => t.TransitionKey == "submit").CreditedItemCount.Should().BeNull(
                "a non-terminal move never had credit evaluated for it");
        }
    }

    [Fact]
    public async Task Rebuild_LeavesTypesThatCreditNothingByDesignUnstamped()
    {
        // The counts_for gate is checked before the applier is called, exactly as the live path checks
        // it. Reading a zero out of the result instead would flag every reflective note, journal club,
        // procedure log, QI project, research output and teaching session as having credited nothing.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 210, subjectUserId: "trainee-1", score: 4, daysAgo: 100,
                activityTypeId: NonCreditingTypeId);
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ActivitiesReplayed.Should().Be(0);
            result.TransitionsStamped.Should().Be(0);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.ActivityTransitions.SingleAsync(t => t.ActivityId == 210 && t.TransitionKey == "complete"))
                .CreditedItemCount.Should().BeNull();
        }
    }

    [Fact]
    public async Task Rebuild_ReproducesTheSemesterTalliesTheIncrementalPathProduced()
    {
        // T130's verification item: "a rebuild reproduces the same bucket counts as incremental crediting".
        // Same activities, same pinned version, same applier. A rebuild is a replay, so it must land on the same
        // numbers the live completions did, bucket by bucket.
        //
        // Three things make this more than the applier compared with itself:
        //  - an ABSOLUTE oracle as well as the equality check, so a bucket written as (0, 0) on both sides fails;
        //  - a fresh DbContext per live completion, as production has, so the applier's database half (not its
        //    Local half) finds the row the previous completion wrote;
        //  - two completions in one semester filed in the opposite order to their encounters, so LastActivityId
        //    proves the replay runs in filing order.
        var options = NewDatabase();
        IReadOnlyList<Snapshot> incremental;

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            // Filed 2026-07-20 about an encounter on 2026-03-10 (semester 1).
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 2, daysAgo: 65, observedOn: new DateOnly(2026, 3, 10));
            // Filed 2026-06-15 about an encounter on 2026-04-01 (semester 1): filed first, observed later.
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-1", score: 4, daysAgo: 100, observedOn: new DateOnly(2026, 4, 1));
            // Filed and observed 2026-08-04 (semester 2).
            AddCompletedActivity(seed, activityId: 202, subjectUserId: "trainee-1", score: 4, daysAgo: 50);
            await seed.SaveChangesAsync();
        }

        // The live path, in filing order: 201, then 200, then 202. Each completion gets its own context and its
        // own save, exactly as ActivityService.TransitionAsync does at the end of a terminal move.
        foreach (var activityId in new[] { 201, 200, 202 })
        {
            await using var db = new ApplicationDbContext(options);
            var activity = await db.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == activityId);
            await new CreditApplier(db).ApplyAsync(activity, PinnedType(CreditsTheEpa), CancellationToken.None);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            incremental = await SnapshotsAsync(db);
        }

        incremental.Should().Equal(
            [
                new Snapshot(NationalItemId, "trainee-1", 2026, 1, CountsSoFar: 2, MinimumLevelReachedCount: 1,
                    ScaleMismatchCount: 0, UnverifiedLevelCount: 2, LastActivityId: 200,
                    LastObservedOn: new DateOnly(2026, 4, 1), LastObservedOnDeclared: true,
                    CreditedActivityKeysJson: """["200:complete","201:complete"]"""),
                new Snapshot(NationalItemId, "trainee-1", 2026, 2, CountsSoFar: 1, MinimumLevelReachedCount: 1,
                    ScaleMismatchCount: 0, UnverifiedLevelCount: 1, LastActivityId: 202,
                    LastObservedOn: new DateOnly(2026, 8, 4), LastObservedOnDeclared: true,
                    CreditedActivityKeysJson: """["202:complete"]""")
            ],
            "guard: the baseline itself must be two semester buckets, each credited by the encounters observed in it");

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ProgressRowsWritten.Should().Be(2);
            result.ProgressRowsRemoved.Should().Be(0);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await SnapshotsAsync(verify)).Should().Equal(incremental);
        }
    }

    [Fact]
    public async Task Rebuild_WritesWhetherEachLastEncounterWasStated_FromTheActivities_NotFromTheStoredRow()
    {
        // T219. A progress row keeps whether its last encounter's date was stated, because it keeps no link to the
        // completion that date came from. Semester 1's latest encounter (5 May) is undated, so its date is only the day
        // its form was created. Semester 2's latest date (4 August) has two encounters, one stated and one not, so it
        // is stated; the undated one is filed later, so a replay that let the last credit overwrite a tie would say
        // otherwise. The stored rows carry the opposite flags. A rebuild writes both from the activities.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 150, observedOn: new DateOnly(2026, 3, 10));
            // Created 2026-05-05 with no encounter date: ObservedOn is that day, and says so.
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-1", score: 4, daysAgo: 141, source: ObservationDateSource.CreatedOn);
            AddCompletedActivity(seed, activityId: 202, subjectUserId: "trainee-1", score: 4, daysAgo: 50, observedOn: new DateOnly(2026, 8, 4));
            // Created 2026-08-04, the day of 202's stated encounter, and replayed after it (filed at the same moment,
            // and the replay breaks the tie by id).
            AddCompletedActivity(seed, activityId: 203, subjectUserId: "trainee-1", score: 4, daysAgo: 50, source: ObservationDateSource.CreatedOn);

            seed.CurriculumItemProgresses.AddRange(
                StaleRow(id: 910, semester: 1, new DateOnly(2026, 5, 5), declared: true, """["200:complete","201:complete"]"""),
                StaleRow(id: 911, semester: 2, new DateOnly(2026, 8, 4), declared: false, """["202:complete","203:complete"]"""));
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            (result.ProgressRowsWritten, result.ProgressRowsRemoved).Should().Be((2, 0));
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await SnapshotsAsync(verify)).Select(row => (row.Semester, row.LastObservedOn, row.LastObservedOnDeclared))
                .Should().Equal(
                    (1, (DateOnly?)new DateOnly(2026, 5, 5), false),
                    (2, (DateOnly?)new DateOnly(2026, 8, 4), true));
        }

        static CurriculumItemProgress StaleRow(int id, int semester, DateOnly lastObservedOn, bool declared, string keys) => new()
        {
            Id = id,
            CurriculumItemId = NationalItemId,
            TraineeUserId = "trainee-1",
            AcademicYear = 2026,
            Semester = semester,
            CountsSoFar = 2,
            MinimumLevelReachedCount = 2,
            LastObservedOn = lastObservedOn,
            LastObservedOnDeclared = declared,
            LastUpdated = Now.AddDays(-30),
            CreditedActivityKeysJson = keys
        };
    }

    [Fact]
    public async Task Rebuild_MovesACreditIntoTheSemesterItsEncounterNowFallsIn()
    {
        // The re-bucketing tool (D40). Activity 200 was observed on 30 June, which is semester 1 today. Its credit
        // is stored in semester 2, as it would be if the boundary had been read as "June is semester 2" when it
        // was credited. A rebuild recomputes the bucket from ObservedOn, writes semester 1 and removes the
        // semester-2 row it no longer reproduces.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 20, observedOn: new DateOnly(2026, 6, 30));
            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 902,
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-1",
                AcademicYear = 2026,
                Semester = 2,
                CountsSoFar = 1,
                MinimumLevelReachedCount = 1,
                LastActivityId = 200,
                LastUpdated = Now.AddDays(-20),
                CreditedActivityKeysJson = """["200:complete"]"""
            });
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ProgressRowsRemoved.Should().Be(1);
            result.ProgressRowsWritten.Should().Be(1);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = await verify.CurriculumItemProgresses.SingleAsync();
            (row.AcademicYear, row.Semester).Should().Be((2026, 1));
            row.CountsSoFar.Should().Be(1);
            row.LastObservedOn.Should().Be(new DateOnly(2026, 6, 30));
        }
    }

    [Fact]
    public async Task Rebuild_ThatThrowsAfterAddingASecondSemester_LeavesNothingBehind()
    {
        // The rollback's hardest case under T130. trainee-1 already holds a semester-1 row; the replay adds a
        // semester-2 row for the SAME item and trainee, then fails. That added row must not survive, and the
        // audit pipeline's catch, which saves this same DbContext, must find nothing to write. Before T130 the
        // rollback matched rows on (item, trainee) alone, so it would have taken the new semester-2 row for the
        // pre-existing one, left it Added, and let the audit save commit it.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-1", score: 4, daysAgo: 50);
            AddCompletedActivity(seed, activityId: 202, subjectUserId: "trainee-1", score: 4, daysAgo: 10);
            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 903,
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-1",
                AcademicYear = 2026,
                Semester = 1,
                CountsSoFar = 5,
                MinimumLevelReachedCount = 5,
                LastActivityId = 200,
                LastObservedOn = new DateOnly(2026, 6, 15),
                // Not what the replay of activity 200 writes (its date is stated), so the rollback has something to undo.
                LastObservedOnDeclared = false,
                LastUpdated = Now.AddDays(-100),
                CreditedActivityKeysJson = """["200:complete"]"""
            });
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            // Calls 1 and 2 credit semester 1 and ADD semester 2; call 3 throws.
            var handler = new RebuildCurriculumProgressCommandHandler(db, new ThrowsOnCall(new CreditApplier(db), failOnCall: 3));
            var rebuild = async () => await handler.Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);
            await rebuild.Should().ThrowAsync<InvalidOperationException>();

            db.ChangeTracker.Entries<CurriculumItemProgress>()
                .Where(entry => entry.State != EntityState.Unchanged)
                .Should().BeEmpty("the audit pipeline's catch saves this context, so nothing may be left dirty");

            // The tracked row holds what was loaded, the stated flag included (T219): the replay set it, and nothing the
            // replay wrote may outlive the rollback.
            var tracked = db.CurriculumItemProgresses.Local.Should().ContainSingle().Subject;
            (tracked.CountsSoFar, tracked.LastObservedOn, tracked.LastObservedOnDeclared)
                .Should().Be((5, (DateOnly?)new DateOnly(2026, 6, 15), false));

            await db.SaveChangesAsync();
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = (await verify.CurriculumItemProgresses.ToListAsync()).Should().ContainSingle().Subject;
            row.Id.Should().Be(903);
            row.CountsSoFar.Should().Be(5);
            row.LastObservedOn.Should().Be(new DateOnly(2026, 6, 15));
        }
    }

    [Fact]
    public async Task Rebuild_ThatMeetsACompletionCreditedMidRebuild_RefusesRatherThanStampingItCreditedNothing()
    {
        // Found by the review of T130. A live completion that opens a new semester row after the rebuild has read
        // the rows is invisible to its zeroing and its removal. The replay then finds that completion's key already
        // in the new row, skips it, and would re-stamp the transition "credited nothing" over a credit that stands,
        // raising T108's banner on a counted encounter. The rebuild refuses instead, and leaves nothing behind.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            var activity = AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 50);
            Completion(activity).CreditedItemCount = 1;
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            // The live completion lands in another context, between the rebuild's read of the rows and its replay.
            var concurrently = new CreditsElsewhereFirst(new CreditApplier(db), options, new CurriculumItemProgress
            {
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-1",
                AcademicYear = 2026,
                Semester = 2,
                CountsSoFar = 1,
                MinimumLevelReachedCount = 1,
                LastActivityId = 200,
                LastUpdated = Now,
                CreditedActivityKeysJson = """["200:complete"]"""
            });

            var handler = new RebuildCurriculumProgressCommandHandler(db, concurrently);
            var rebuild = async () => await handler.Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

            await rebuild.Should().ThrowAsync<InvalidOperationException>().WithMessage("*changed while the rebuild was running*");
            db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged).Should().BeEmpty();
            await db.SaveChangesAsync();
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.ActivityTransitions.SingleAsync(t => t.ActivityId == 200 && t.TransitionKey == "complete"))
                .CreditedItemCount.Should().Be(1, "the stamp describes a credit that stands, and must not be overwritten with 0");
            (await verify.CurriculumItemProgresses.SingleAsync()).CountsSoFar.Should().Be(1);
        }
    }

    [Fact]
    public async Task Rebuild_WhoseSaveFails_LeavesNothingForTheAuditSaveToCommit()
    {
        // The save is guarded as well as the replay. If it fails (a deadlock, a dropped connection, the xmin token
        // catching a live completion mid-rebuild), the audit pipeline saves this same DbContext again. Left dirty,
        // that second save would commit the whole rebuild while the operator was told it failed.
        var failFirstSave = new FailFirstSave();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(failFirstSave)
            .Options;

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            var activity = AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            Completion(activity).CreditedItemCount = 0;
            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 904,
                CurriculumItemId = OrphanItemId,
                TraineeUserId = "trainee-1",
                AcademicYear = 2026,
                Semester = 1,
                CountsSoFar = 3,
                LastUpdated = Now.AddDays(-1),
                CreditedActivityKeysJson = """["999:complete"]"""
            });
            await seed.SaveChangesAsync();
        }

        failFirstSave.Arm();

        await using (var db = new ApplicationDbContext(options))
        {
            var rebuild = async () => await Rebuild(db);
            await rebuild.Should().ThrowAsync<DbUpdateException>();

            db.ChangeTracker.Entries()
                .Where(entry => entry.State != EntityState.Unchanged)
                .Should().BeEmpty("the stamps, the new row, the zeroing and the removal must all be undone");

            await db.SaveChangesAsync();
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = (await verify.CurriculumItemProgresses.ToListAsync()).Should().ContainSingle().Subject;
            row.Id.Should().Be(904, "the orphan the rebuild would have removed is still there");
            row.CountsSoFar.Should().Be(3);
            (await verify.ActivityTransitions.SingleAsync(t => t.ActivityId == 200 && t.TransitionKey == "complete"))
                .CreditedItemCount.Should().Be(0, "the re-stamp was rolled back with everything else");
        }
    }

    [Fact]
    public async Task Rebuild_ScopedToOneTrainee_LeavesEveryOtherTraineeUntouched()
    {
        // T119 wants a rebuild confined to the rows whose dates actually moved. trainee-2's row is
        // deliberately wrong; a scoped rebuild must not notice, and a global one must fix it.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-2", score: 4, daysAgo: 50);

            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 900,
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-2",
                // Activity 201's semester, so the global rebuild fixes this row in place rather than
                // removing it and writing another.
                AcademicYear = 2026,
                Semester = 2,
                CountsSoFar = 99,
                MinimumLevelReachedCount = 99,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                CreditedActivityKeysJson = """["201:complete"]"""
            });

            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db, traineeUserId: "trainee-1");
            result.ActivitiesReplayed.Should().Be(1, "trainee-2's activity must not even be read");
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.CurriculumItemProgresses.SingleAsync(row => row.TraineeUserId == "trainee-1"))
                .CountsSoFar.Should().Be(1);
            (await verify.CurriculumItemProgresses.SingleAsync(row => row.TraineeUserId == "trainee-2"))
                .CountsSoFar.Should().Be(99, "a scoped rebuild must leave every other trainee alone");
        }

        await using (var db = new ApplicationDbContext(options))
        {
            await Rebuild(db);
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ProgressRowsRemoved.Should().Be(0, "row 900 is in the right semester, so it is fixed in place, not replaced");
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = await verify.CurriculumItemProgresses.SingleAsync(entity => entity.TraineeUserId == "trainee-2");
            row.CountsSoFar.Should().Be(1, "a global rebuild re-scores everybody from the activities");
            row.Id.Should().Be(900);
        }
    }

    [Fact]
    public async Task Rebuild_RemovesAProgressRowNothingReproduces()
    {
        // A rebuild is a rebuild, not a top-up: a row no completion accounts for any more — the activity
        // was cancelled, or re-dated out of the trainee's programme — must go, or the tally is additive
        // for ever.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);

            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 901,
                CurriculumItemId = OrphanItemId,
                TraineeUserId = "trainee-1",
                AcademicYear = 2026,
                Semester = 1,
                CountsSoFar = 3,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                CreditedActivityKeysJson = """["999:complete"]"""
            });

            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ProgressRowsRemoved.Should().Be(1);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var rows = await verify.CurriculumItemProgresses.ToListAsync();
            rows.Should().ContainSingle();
            rows.Single().CurriculumItemId.Should().Be(NationalItemId);
        }
    }

    /// <summary>
    /// T196, D48. A rebuild judges each completion at its own moment, the transition credit is recorded against, so while
    /// an EPA is deactivated it keeps the credit earned before the deactivation and credits nothing completed since. Before
    /// T196 it judged every completion against the catalogue as it stood that day, and dropped the earned credit with the
    /// paused. Once the EPA is back in force every moment is, and the paused completion is credited too.
    /// </summary>
    [Fact]
    public async Task Rebuild_WhileTheEpaIsDeactivated_KeepsTheCreditEarnedBeforeIt_AndCreditsNothingCompletedSince()
    {
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-1", score: 4, daysAgo: 50);
            await seed.SaveChangesAsync();

            // Between the two completions.
            (await seed.Epas.SingleAsync(epa => epa.Id == CreditedEpaId)).Deactivate(Now.AddDays(-60));
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);

            result.ActivitiesReplayed.Should().Be(2, "credit was evaluated for both");
            result.CreditApplications.Should().Be(1);
            result.ProgressRowsWritten.Should().Be(1);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.CurriculumItemProgresses.SingleAsync()).CreditedActivityKeysJson.Should().Be(
                """["200:complete"]""", "the credit earned while the EPA was in force survives a rebuild while it is inactive");
            (await CompletionStampAsync(verify, 200)).Should().Be(1);
            (await CompletionStampAsync(verify, 201)).Should().Be(0, "completed during the pause, so its credit waits");
        }

        await using (var reactivate = new ApplicationDbContext(options))
        {
            (await reactivate.Epas.SingleAsync(epa => epa.Id == CreditedEpaId)).Reactivate();
            await reactivate.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            (await Rebuild(db)).CreditApplications.Should().Be(2);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.CurriculumItemProgresses.Select(row => row.CreditedActivityKeysJson).ToListAsync())
                .Should().BeEquivalentTo("""["200:complete"]""", """["201:complete"]""");
            (await CompletionStampAsync(verify, 201)).Should().Be(1);
        }
    }

    /// <summary>
    /// T196. The moment is the completion's, not the encounter's: the live path judges the EPA when the activity completes,
    /// so an encounter observed before the deactivation but completed after it was paused live, and a rebuild that judged
    /// the encounter date would credit what the live path did not.
    /// </summary>
    [Fact]
    public async Task Rebuild_JudgesTheCompletionsMoment_NotTheEncounterDate()
    {
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(
                seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 50,
                observedOn: DateOnly.FromDateTime(Now.AddDays(-70)));
            await seed.SaveChangesAsync();

            (await seed.Epas.SingleAsync(epa => epa.Id == CreditedEpaId)).Deactivate(Now.AddDays(-60));
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            (await Rebuild(db)).CreditApplications.Should().Be(0);
        }

        await using var verify = new ApplicationDbContext(options);
        verify.CurriculumItemProgresses.Should().BeEmpty();
        (await CompletionStampAsync(verify, 200)).Should().Be(0);
    }

    /// <summary>
    /// T196: why one timestamp is enough. Deactivated, reactivated, deactivated again: the rebuild credits everything
    /// completed before the current pause, including what was completed during the first one, because the reactivation
    /// that closed the first pause credited it. Only the current pause holds anything back.
    /// </summary>
    [Fact]
    public async Task Rebuild_AfterASecondDeactivation_HoldsBackOnlyTheCurrentPause()
    {
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-1", score: 4, daysAgo: 80);
            AddCompletedActivity(seed, activityId: 202, subjectUserId: "trainee-1", score: 4, daysAgo: 40);
            await seed.SaveChangesAsync();

            var epa = await seed.Epas.SingleAsync(entity => entity.Id == CreditedEpaId);
            epa.Deactivate(Now.AddDays(-90));
            epa.Reactivate();
            epa.Deactivate(Now.AddDays(-60));
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            (await Rebuild(db)).CreditApplications.Should().Be(2);
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.CurriculumItemProgresses.Select(row => row.CreditedActivityKeysJson).ToListAsync())
            .SelectMany(json => System.Text.Json.JsonSerializer.Deserialize<string[]>(json)!)
            .Should().BeEquivalentTo("200:complete", "201:complete");
        (await CompletionStampAsync(verify, 202)).Should().Be(0);
    }

    private static async Task<int?> CompletionStampAsync(ApplicationDbContext db, int activityId)
        => (await db.ActivityTransitions.SingleAsync(transition =>
            transition.ActivityId == activityId && transition.TransitionKey == "complete")).CreditedItemCount;

    [Fact]
    public async Task Rebuild_RefusesACallerWhoIsNotAnAdministrator()
    {
        // The command has no entry point yet. When it gets one, the gate is already here rather than
        // only on the page — a rebuild re-scores every trainee in every institution.
        var options = NewDatabase();

        await using var db = new ApplicationDbContext(options);
        Seed(db);
        AddCompletedActivity(db, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
        await db.SaveChangesAsync();

        var handler = new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db));

        var rebuild = async () => await handler.Handle(
            new RebuildCurriculumProgressCommand(Principal(WombatRoles.InstitutionalAdmin)), CancellationToken.None);

        await rebuild.Should().ThrowAsync<UnauthorizedAccessException>();
        db.CurriculumItemProgresses.Should().BeEmpty();
    }

    // ─── Harness ─────────────────────────────────────────────────────────────

    private static async Task<RebuildCurriculumProgressResult> Rebuild(
        ApplicationDbContext db, string? traineeUserId = null)
    {
        var handler = new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db));
        return await handler.Handle(
            new RebuildCurriculumProgressCommand(Administrator(), traineeUserId), CancellationToken.None);
    }

    /// <summary>
    /// Everything a replay is supposed to reproduce, compared as one value. LastUpdated is deliberately absent:
    /// it is an audit clock and a replay rewrites it. LastObservedOn and LastObservedOnDeclared are present, because
    /// they are what a reader shows and they must survive a rebuild exactly (T219).
    /// </summary>
    private sealed record Snapshot(
        int CurriculumItemId,
        string TraineeUserId,
        int AcademicYear,
        int Semester,
        int CountsSoFar,
        int MinimumLevelReachedCount,
        int ScaleMismatchCount,
        int UnverifiedLevelCount,
        int? LastActivityId,
        DateOnly? LastObservedOn,
        bool LastObservedOnDeclared,
        string CreditedActivityKeysJson)
    {
        public static Snapshot Of(CurriculumItemProgress row)
            => new(
                row.CurriculumItemId,
                row.TraineeUserId,
                row.AcademicYear,
                row.Semester,
                row.CountsSoFar,
                row.MinimumLevelReachedCount,
                row.ScaleMismatchCount,
                row.UnverifiedLevelCount,
                row.LastActivityId,
                row.LastObservedOn,
                row.LastObservedOnDeclared,
                row.CreditedActivityKeysJson);
    }

    /// <summary>Every row, one per key: the in-memory provider enforces no unique index, so the test checks it.</summary>
    private static async Task<IReadOnlyList<Snapshot>> SnapshotsAsync(ApplicationDbContext db)
    {
        var rows = await db.CurriculumItemProgresses.AsNoTracking().ToListAsync();
        rows.GroupBy(CurriculumItemProgressKey.Of).Should().OnlyContain(group => group.Count() == 1, "one row per (item, trainee, semester)");

        return rows
            .OrderBy(row => row.CurriculumItemId)
            .ThenBy(row => row.TraineeUserId, StringComparer.Ordinal)
            .ThenBy(row => row.AcademicYear)
            .ThenBy(row => row.Semester)
            .Select(Snapshot.Of)
            .ToList();
    }

    /// <summary>
    /// Before the first plan, writes a progress row through a separate context: a live completion committing while the
    /// rebuild runs.
    /// </summary>
    private sealed class CreditsElsewhereFirst(ICreditApplier inner, DbContextOptions<ApplicationDbContext> options, CurriculumItemProgress row) : ICreditApplier
    {
        private bool _done;

        public async Task<CreditPlan> PlanAsync(CreditSubject subject, ActivityType activityType, CancellationToken cancellationToken = default)
        {
            if (!_done)
            {
                _done = true;
                await using var elsewhere = new ApplicationDbContext(options);
                elsewhere.CurriculumItemProgresses.Add(row);
                await elsewhere.SaveChangesAsync(cancellationToken);
            }

            return await inner.PlanAsync(subject, activityType, cancellationToken);
        }

        public CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity) => inner.Apply(plan, completedActivity);

        public async Task<CreditApplicationResult> ApplyAsync(Activity completedActivity, ActivityType activityType, CancellationToken cancellationToken = default)
            => Apply(await PlanAsync(CreditSubject.Of(completedActivity), activityType, cancellationToken), completedActivity);
    }

    /// <summary>Makes the next SaveChanges fail once it is armed, as a deadlock or a dropped connection would.</summary>
    private sealed class FailFirstSave : SaveChangesInterceptor
    {
        private bool _armed;

        public void Arm() => _armed = true;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (_armed)
            {
                _armed = false;
                throw new DbUpdateException("induced failure at save");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    /// <summary>Fails the Nth credit application, leaving the earlier ones already written in memory.</summary>
    private sealed class ThrowsOnCall : ICreditApplier
    {
        private readonly ICreditApplier _inner;
        private readonly int _failOnCall;
        private int _calls;

        public ThrowsOnCall(ICreditApplier inner, int failOnCall)
        {
            _inner = inner;
            _failOnCall = failOnCall;
        }

        public async Task<CreditApplicationResult> ApplyAsync(
            Activity completedActivity, ActivityType activityType, CancellationToken cancellationToken = default)
        {
            _calls++;
            if (_calls == _failOnCall)
            {
                throw new InvalidOperationException("induced failure mid-replay");
            }

            return await _inner.ApplyAsync(completedActivity, activityType, cancellationToken);
        }

        public Task<CreditPlan> PlanAsync(CreditSubject subject, ActivityType activityType, CancellationToken cancellationToken = default)
            => _inner.PlanAsync(subject, activityType, cancellationToken);

        public CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity)
            => _inner.Apply(plan, completedActivity);
    }

    private static ActivityTransition Completion(Activity activity)
        => activity.Transitions.Single(transition => transition.TransitionKey == "complete");

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Administrator() => Principal(WombatRoles.Administrator);

    private static ClaimsPrincipal Principal(string role)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"));

    private static ActivityType PinnedType(string creditRulesJson)
        => new() { CreditRulesJson = creditRulesJson, SchemaJson = SchemaJson };

    private static Activity AddCompletedActivity(
        ApplicationDbContext db,
        int activityId,
        string subjectUserId,
        int score,
        int daysAgo,
        int activityTypeId = CreditingTypeId,
        DateOnly? observedOn = null,
        ObservationDateSource source = ObservationDateSource.Declared)
    {
        var filedOn = Now.AddDays(-daysAgo);

        var activity = new Activity
        {
            Id = activityId,
            ActivityTypeId = activityTypeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = subjectUserId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{CreditedEpaId}}, "score": {{score}} }""",
            CreatedOn = filedOn,
            UpdatedOn = filedOn,
            // T119: the encounter date, which is what the stage minimum is selected by. Set alongside
            // CreatedOn rather than left at default, because default(DateOnly) is 0001-01-01 and falls
            // before every programme start.
            ObservedOn = observedOn ?? DateOnly.FromDateTime(filedOn),
            ObservedOnSource = source
        };

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "draft",
            ToState = "submitted",
            TransitionKey = "submit",
            ActorUserId = subjectUserId,
            OccurredOn = filedOn.AddMinutes(-5)
        });

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "submitted",
            ToState = "completed",
            TransitionKey = "complete",
            ActorUserId = subjectUserId,
            OccurredOn = filedOn
        });

        db.Activities.Add(activity);
        return activity;
    }

    private static void Seed(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = CreditedEpaId, SubSpecialityId = 1, Code = "EPA-1", Title = "Take a history" });
        db.Epas.Add(new Epa { Id = UncreditedEpaId, SubSpecialityId = 1, Code = "EPA-X", Title = "Nothing files against this" });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = NationalItemId,
            CurriculumId = CurriculumId,
            EpaId = CreditedEpaId,
            RequiredCount = 3,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        // Exists so an orphaned progress row has something to point at; no activity ever credits it.
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = OrphanItemId,
            CurriculumId = CurriculumId,
            EpaId = UncreditedEpaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        foreach (var (id, userId) in new[] { (1, "trainee-1"), (2, "trainee-2") })
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                Id = id,
                UserId = userId,
                InstitutionId = 10,
                CurriculumId = CurriculumId,
                ProgrammeStartDate = new DateOnly(2025, 1, 1),
                ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        db.ActivityTypes.Add(ActivityTypeWith(CreditingTypeId, "wba_under_test", CreditsTheEpa));
        db.ActivityTypes.Add(ActivityTypeWith(NonCreditingTypeId, "reflective_note_under_test", CreditsNothingByDesign));

        db.SaveChanges();
    }

    private static ActivityType ActivityTypeWith(int id, string key, string creditRulesJson)
    {
        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = 10,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        return activityType;
    }

    private const string CreditsTheEpa = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

    private const string CreditsNothingByDesign = """{ "counts_for": [] }""";

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "score", "type": "number", "label": "Score", "required": true }
              ]
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "complete", "from": "submitted", "to": "completed", "actor": "subject" }
          ]
        }
        """;
}
