using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T230 on a real PostgreSQL server: a completion racing its EPA's deactivation or reactivation ends with the credit a
/// rebuild would write.
/// </summary>
/// <remarks>
/// <para>
/// Each race is driven from two contexts on two connections. One request is stopped at a gate while it holds what it
/// holds; the other is started, and the test waits until the server reports it waiting for a lock, or until it has
/// finished, which is what it did before T230. Then the gate opens and both run to the end. The completion is the live
/// path itself (<see cref="ActivityService.TransitionAsync" />); the EPA's side is the command handlers.
/// </para>
/// <para>
/// The outcome is judged the way the task states it: an Administrator rebuild afterwards changes nothing, neither a
/// progress row nor a completion's stamp. The clock is shared between the two requests and moves on at every read, so
/// the order they read it in is the order of their moments, as on one server.
/// </para>
/// <para>
/// Each test works on a schema of its own (<c>SearchPath = it_&lt;guid&gt;</c>), as <c>EpaActivePeriodPostgresTests</c>
/// does, and names its connections, so the wait it looks for can only be its own.
/// </para>
/// <para>
/// The last tests pin the hold itself: a change's lock lets a foreign-key check on the EPA through, and a hold that waits
/// past the command timeout (one second here, Npgsql's thirty in the product) is refused as busy, leaving the context
/// fit for the audit pipeline's save and nothing of the request in the database.
/// </para>
/// </remarks>
public sealed class EpaCreditRacePostgresTests : IAsyncLifetime
{
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";
    private const string TraineeUserId = "trainee-t230";

    private static readonly DateTime DeactivatedAt = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(20);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    // ─── A reactivation racing a completion that read the EPA as inactive ────

    [Fact]
    public async Task AReactivation_WaitsForACompletionThatReadTheEpaAsInactive_AndCreditsIt()
    {
        // Before T230: the reactivation read its candidates while the completion was still in flight, missed it, and the
        // completion then saved its zero stamp. It stayed uncredited until an Administrator rebuild.
        var fixture = await ArrangeAsync();
        await DeactivateAsync(fixture, new FixedClock(DeactivatedAt));
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 8, 3, 9, 0, 0, DateTimeKind.Utc));
        var clock = new SharedClock(new DateTime(2026, 8, 5, 8, 0, 0, DateTimeKind.Utc));

        var completionGate = new Gate();
        var completion = Task.Run(() => CompleteAsync(fixture, filed, clock, "completion", completionGate));
        await completionGate.ReachedAsync(completion);

        var reactivation = Task.Run(() => ReactivateAsync(fixture, "reactivation"));
        var reactivationWaited = await WaitsForALockAsync(fixture, "reactivation", reactivation);

        completionGate.Open();
        await completion;
        var reactivated = await reactivation;

        await ARebuildChangesNothingAsync(fixture);
        reactivationWaited.Should().BeTrue("the completion holds the EPA until its save commits");
        reactivated.CompletionsCredited.Should().Be(1, "the reactivation read its candidates after the completion committed");
        (await StampAsync(fixture, filed)).Should().Be(1);
    }

    [Fact]
    public async Task ACompletion_WaitsForAReactivationInFlight_AndReadsTheEpaAsActive()
    {
        // Before T230: the completion read the EPA as inactive while the reactivation was in flight, and saved its zero
        // stamp; the reactivation had read its candidates before the completion existed. Uncredited, as above.
        var fixture = await ArrangeAsync();
        await DeactivateAsync(fixture, new FixedClock(DeactivatedAt));
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 8, 3, 9, 0, 0, DateTimeKind.Utc));
        var clock = new SharedClock(new DateTime(2026, 8, 5, 8, 0, 0, DateTimeKind.Utc));

        var reactivationGate = new Gate();
        var reactivation = Task.Run(() => ReactivateAsync(fixture, "reactivation", reactivationGate));
        await reactivationGate.ReachedAsync(reactivation);

        var completion = Task.Run(() => CompleteAsync(fixture, filed, clock, "completion"));
        var completionWaited = await WaitsForALockAsync(fixture, "completion", completion);

        reactivationGate.Open();
        var reactivated = await reactivation;
        await completion;

        await ARebuildChangesNothingAsync(fixture);
        completionWaited.Should().BeTrue("the reactivation holds the EPA until its save commits");
        reactivated.CompletionsCredited.Should().Be(0);
        (await StampAsync(fixture, filed)).Should().Be(1, "the completion read the EPA after the reactivation committed");
    }

    [Fact]
    public async Task TwoReactivationsOfOneEpa_TheSecondWaits_AndTheCreditIsWrittenOnce()
    {
        // The T196 review's race between two reactivations, which the second used to lose with "Save again": it now waits
        // for the first, finds the EPA active, and has nothing to credit.
        var fixture = await ArrangeAsync();
        await DeactivateAsync(fixture, new FixedClock(DeactivatedAt));
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 8, 3, 9, 0, 0, DateTimeKind.Utc));
        await CompleteAsync(fixture, filed, new SharedClock(new DateTime(2026, 8, 5, 8, 0, 0, DateTimeKind.Utc)), "completion");
        (await StampAsync(fixture, filed)).Should().Be(0, "completed during the pause");

        var firstGate = new Gate();
        var first = Task.Run(() => ReactivateAsync(fixture, "first", firstGate));
        await firstGate.ReachedAsync(first);

        var second = Task.Run(() => ReactivateAsync(fixture, "second"));
        var secondWaited = await WaitsForALockAsync(fixture, "second", second);

        firstGate.Open();
        var firstResult = await first;
        var secondResult = await second;

        secondWaited.Should().BeTrue();
        (firstResult.CompletionsCredited, secondResult.CompletionsCredited).Should().Be((1, 0));
        (await StampAsync(fixture, filed)).Should().Be(1);
        await ARebuildChangesNothingAsync(fixture);
    }

    // ─── A deactivation racing a completion that read the EPA as active ──────

    /// <param name="bySave">
    /// Deactivated by unticking Active and saving (<see cref="UpdateEpaCommandHandler" />) rather than by the Deactivate
    /// button (<see cref="DeactivateEpaCommandHandler" />).
    /// </param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ACompletionAfterADeactivationTookItsMoment_WaitsForIt_AndIsPaused(bool bySave)
    {
        // Before T230: the deactivation took its moment, the completion started after it and read the EPA as still active,
        // and both saved. The completion kept live credit for a moment inside the pause, which a rebuild while the EPA is
        // inactive took away. The deactivation is stopped as it reads the clock, so a deactivation that took its moment
        // before it held the EPA is caught too.
        var fixture = await ArrangeAsync();
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 4, 28, 9, 0, 0, DateTimeKind.Utc));
        var clock = new SharedClock(DeactivatedAt);

        var deactivationGate = new Gate();
        var deactivating = new GatedClock(clock, deactivationGate);
        var deactivation = Task.Run(() => bySave
            ? SaveInactiveAsync(fixture, deactivating, "deactivation")
            : DeactivateAsync(fixture, deactivating, "deactivation"));
        await deactivationGate.ReachedAsync(deactivation);

        var completion = Task.Run(() => CompleteAsync(fixture, filed, clock, "completion"));
        var completionWaited = await WaitsForALockAsync(fixture, "completion", completion);

        deactivationGate.Open();
        await deactivation;
        await completion;

        await ARebuildChangesNothingAsync(fixture);
        completionWaited.Should().BeTrue("the deactivation holds the EPA from before it takes its moment until it commits");
        (await StampAsync(fixture, filed)).Should().Be(0, "the completion's moment falls inside the pause");
        (await CompletedAtAsync(fixture, filed)).Should().BeAfter((await DeactivatedOnAsync(fixture))!.Value);
    }

    /// <param name="bySave">As above.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ADeactivation_WaitsForACompletionThatReadTheEpaAsActive_AndItsPauseBeginsAfterIt(bool bySave)
    {
        // The other order. The completion's credit stands, and a rebuild while the EPA is inactive keeps it, because the
        // deactivation takes its moment only once the completion has committed.
        var fixture = await ArrangeAsync();
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 4, 28, 9, 0, 0, DateTimeKind.Utc));
        var clock = new SharedClock(DeactivatedAt);

        var completionGate = new Gate();
        var completion = Task.Run(() => CompleteAsync(fixture, filed, clock, "completion", completionGate));
        await completionGate.ReachedAsync(completion);

        var deactivation = Task.Run(() => bySave
            ? SaveInactiveAsync(fixture, clock, "deactivation")
            : DeactivateAsync(fixture, clock, "deactivation"));
        var deactivationWaited = await WaitsForALockAsync(fixture, "deactivation", deactivation);

        completionGate.Open();
        await completion;
        await deactivation;

        await ARebuildChangesNothingAsync(fixture);
        deactivationWaited.Should().BeTrue("the completion holds the EPA until its save commits");
        (await StampAsync(fixture, filed)).Should().Be(1);
        (await DeactivatedOnAsync(fixture)).Should().BeAfter(await CompletedAtAsync(fixture, filed));
    }

    [Fact]
    public async Task ACompletionWhoseMomentPrecedesADeactivation_ButWhichHoldsTheEpaAfterItCommits_IsCredited()
    {
        // The completion reads the clock, then a deactivation takes the EPA, reads a later moment and commits, and only then
        // does the completion hold the EPA. It reads the pause, and judges the moment it stamps its transition with against
        // it, as a rebuild does: that moment falls before the pause began, so it credits. A completion that judged a later
        // reading of the clock than the one it stamps would credit nothing here, and the rebuild would disagree.
        var fixture = await ArrangeAsync();
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 4, 28, 9, 0, 0, DateTimeKind.Utc));
        var clock = new SharedClock(DeactivatedAt);

        var completionGate = new Gate();
        var completion = Task.Run(() => CompleteAsync(fixture, filed, new GatedClock(clock, completionGate), "completion"));
        await completionGate.ReachedAsync(completion);

        await DeactivateAsync(fixture, clock, "deactivation");

        completionGate.Open();
        await completion;

        await ARebuildChangesNothingAsync(fixture);
        (await StampAsync(fixture, filed)).Should().Be(1, "the completion's moment falls before the pause began");
        (await CompletedAtAsync(fixture, filed)).Should().BeBefore((await DeactivatedOnAsync(fixture))!.Value);
    }

    // ─── What a change's hold does not hold off, and how a hold gives up ─────

    [Fact]
    public async Task AnEpaChange_HoldsOffACompletion_ButNotARowThatNamesTheEpa()
    {
        // A change's hold conflicts with a completion's, and not with the lock a foreign-key check takes on the EPA row.
        // Under FOR UPDATE every insert naming the EPA (a curriculum item, an entrustment decision, a pending one, an MSF
        // campaign's EPA) would wait for the change to commit, and a reactivation plans every completion of its pause inside
        // its hold. The probe table stands in for those four: its foreign key is checked the same way theirs are.
        var fixture = await ArrangeAsync();
        await using (var setup = await OpenAsync(fixture, applicationName: null))
        {
            await ExecuteAsync(setup, """CREATE TABLE "T230Probe" ("EpaId" integer NOT NULL REFERENCES "Epas" ("Id"))""");
        }

        await using var db = NewContext(fixture, "change");
        await using var hold = await new EpaCreditLock(db).HoldForChangeAsync(fixture.EpaId, CancellationToken.None);

        await using var other = await OpenAsync(fixture, fixture.ApplicationName("other"));
        await ExecuteAsync(other, "SET lock_timeout = '3s'");

        var insert = () => ExecuteAsync(other, $"""INSERT INTO "T230Probe" ("EpaId") VALUES ({fixture.EpaId})""");
        await insert.Should().NotThrowAsync("a foreign-key check on the EPA does not wait for a change to it");

        var completionsHold = () => ExecuteAsync(other, $"""SELECT 1 FROM "Epas" WHERE "Id" = {fixture.EpaId} FOR SHARE""");
        (await completionsHold.Should().ThrowAsync<PostgresException>())
            .Which.SqlState.Should().Be(PostgresErrorCodes.LockNotAvailable, "a completion's hold still waits for the change");
    }

    [Fact]
    public async Task ACompletionThatWaitsPastTheCommandTimeout_IsRefusedAsBusy_AndLeavesNothingForTheAuditSaveToCommit()
    {
        var fixture = await ArrangeAsync();
        var filed = await AddSubmittedActivityAsync(fixture, createdOn: new DateTime(2026, 4, 28, 9, 0, 0, DateTimeKind.Utc));
        await using var change = await HoldTheEpaAsync(fixture, "FOR NO KEY UPDATE");

        await using var db = NewContext(fixture, "completion", commandTimeoutSeconds: 1);
        var service = new ActivityService(
            db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(),
            new SharedClock(DeactivatedAt), new EpaCreditLock(db));

        var complete = () => service.TransitionAsync(new TransitionActivityInput(
            filed, "complete", TraineeUserId, Trainee(), DataPatchJson: null, Note: null));
        var refusal = (await complete.Should().ThrowAsync<InvalidOperationException>()).Which;

        refusal.Message.Should().Be(EpaCreditLock.CreditBusy, "the page shows the message, not Npgsql's");
        await AuditSaveCommitsOnlyItsRowAsync(fixture, db, refusal);
        await change.DisposeAsync();

        await using var verify = NewContext(fixture);
        var stored = await verify.Activities.AsNoTracking()
            .Include(entity => entity.Transitions)
            .SingleAsync(entity => entity.Id == filed);
        stored.CurrentState.Should().Be("submitted");
        stored.Transitions.Should().ContainSingle("only the submission; the completion was refused before its first change");
        (await verify.CurriculumItemProgresses.CountAsync()).Should().Be(0);
    }

    /// <param name="bySave">As above.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AnEpaChangeThatWaitsPastTheCommandTimeout_IsRefusedAsBusy_AndLeavesNothingForTheAuditSaveToCommit(bool bySave)
    {
        var fixture = await ArrangeAsync();
        var command = await EditCommandAsync(fixture, isActive: false);
        await using var completion = await HoldTheEpaAsync(fixture, "FOR SHARE");

        await using var db = NewContext(fixture, "deactivation", commandTimeoutSeconds: 1);
        Func<Task> deactivate = bySave
            ? () => new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), new FixedClock(DeactivatedAt))
                .Handle(command, CancellationToken.None)
            : () => new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), new FixedClock(DeactivatedAt))
                .Handle(new DeactivateEpaCommand(fixture.EpaId, Administrator()), CancellationToken.None);
        var refusal = (await deactivate.Should().ThrowAsync<InvalidOperationException>()).Which;

        refusal.Message.Should().Be(EpaCreditLock.ChangeBusy, "the page shows the message, not Npgsql's");
        await AuditSaveCommitsOnlyItsRowAsync(fixture, db, refusal);
        await completion.DisposeAsync();

        (await DeactivatedOnAsync(fixture)).Should().BeNull("the deactivation was refused before its first change");
    }

    // ─── The requests ────────────────────────────────────────────────────────

    /// <summary>The trainee completes the activity through the live path, on a connection of its own.</summary>
    private async Task CompleteAsync(Fixture fixture, int activityId, TimeProvider clock, string connection, Gate? beforeSave = null)
    {
        await using var db = NewContext(fixture, connection, beforeSave);
        var service = new ActivityService(
            db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(), clock, new EpaCreditLock(db));

        await service.TransitionAsync(new TransitionActivityInput(
            activityId,
            "complete",
            TraineeUserId,
            Trainee(),
            DataPatchJson: null,
            Note: null));
    }

    /// <summary>
    /// Another request holding the EPA as a completion (<c>FOR SHARE</c>) or a change (<c>FOR NO KEY UPDATE</c>) would,
    /// in a transaction left open until the returned connection is disposed.
    /// </summary>
    private async Task<NpgsqlConnection> HoldTheEpaAsync(Fixture fixture, string lockClause)
    {
        var connection = await OpenAsync(fixture, fixture.ApplicationName("holder"));
        await ExecuteAsync(connection, "BEGIN");
        await ExecuteAsync(connection, $"""SELECT 1 FROM "Epas" WHERE "Id" = {fixture.EpaId} {lockClause}""");
        return connection;
    }

    /// <summary>
    /// What the audit pipeline's catch does after a refusal that is not a refused save: add its row to the request's own
    /// context and save it (<c>AuditWriter.WriteAsync</c>). The context must still work after the timeout, and the save
    /// must carry that row and nothing else.
    /// </summary>
    private async Task AuditSaveCommitsOnlyItsRowAsync(Fixture fixture, ApplicationDbContext db, Exception refusal)
    {
        db.Database.CurrentTransaction.Should().BeNull("a refused hold leaves no transaction for the audit save to write into");
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        await new AuditWriter(db).WriteAsync(AuditEntry.Create(
            DeactivatedAt, AuditCategory.Command, "T230Refused", success: false, errorMessage: refusal.Message));
        db.ChangeTracker.Clear();

        await using var verify = NewContext(fixture);
        (await verify.Set<AuditEntry>().AsNoTracking().CountAsync(entry => entry.Action == "T230Refused")).Should().Be(1);
    }

    private async Task<UpdateEpaResult> ReactivateAsync(Fixture fixture, string connection, Gate? beforeSave = null)
        => await SaveAsync(fixture, isActive: true, new FixedClock(DeactivatedAt.AddMonths(4)), connection, beforeSave);

    private Task SaveInactiveAsync(Fixture fixture, TimeProvider clock, string connection)
        => SaveAsync(fixture, isActive: false, clock, connection, beforeSave: null);

    private async Task<UpdateEpaResult> SaveAsync(Fixture fixture, bool isActive, TimeProvider clock, string connection, Gate? beforeSave)
    {
        var command = await EditCommandAsync(fixture, isActive);

        await using var db = NewContext(fixture, connection, beforeSave);
        return await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), clock)
            .Handle(command, CancellationToken.None);
    }

    /// <summary>The EPA's edit form as it stands, saved with Active ticked or not.</summary>
    private async Task<UpdateEpaCommand> EditCommandAsync(Fixture fixture, bool isActive)
    {
        await using var read = NewContext(fixture);
        var epa = await read.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId);
        return new UpdateEpaCommand(
            epa.Id, epa.SubSpecialityId, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, isActive, Administrator());
    }

    private async Task DeactivateAsync(Fixture fixture, TimeProvider clock, string? connection = null)
    {
        await using var db = NewContext(fixture, connection);
        await new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), clock)
            .Handle(new DeactivateEpaCommand(fixture.EpaId, Administrator()), CancellationToken.None);
    }

    /// <summary>
    /// Waits until the server reports the named connection waiting for a lock (true), or the request has finished without
    /// waiting (false), which is what every request did before T230.
    /// </summary>
    private async Task<bool> WaitsForALockAsync(Fixture fixture, string connection, Task request)
    {
        await using var monitor = await TestDatabase.OpenAdminConnectionAsync();

        var deadline = DateTime.UtcNow + Patience;
        while (DateTime.UtcNow < deadline)
        {
            if (request.IsCompleted)
            {
                await request;
                return false;
            }

            await using var command = new NpgsqlCommand(
                """SELECT count(*) FROM pg_stat_activity WHERE application_name = $1 AND wait_event_type = 'Lock'""", monitor);
            command.Parameters.Add(new NpgsqlParameter { Value = fixture.ApplicationName(connection) });
            if (Convert.ToInt64(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture) > 0)
            {
                return true;
            }

            await Task.Delay(25);
        }

        throw new TimeoutException($"'{connection}' neither waited for a lock nor finished within {Patience}.");
    }

    // ─── The outcome ─────────────────────────────────────────────────────────

    /// <summary>What the race left is what a rebuild writes: the rebuild moves no progress row and no stamp.</summary>
    private async Task ARebuildChangesNothingAsync(Fixture fixture)
    {
        var live = await SnapshotAsync(fixture);

        await using (var db = NewContext(fixture))
        {
            await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
                .Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);
        }

        (await SnapshotAsync(fixture)).Should().BeEquivalentTo(live, options => options.WithStrictOrdering(),
            "the race must end with the credit a rebuild would write");
    }

    private async Task<int?> StampAsync(Fixture fixture, int activityId)
    {
        await using var db = NewContext(fixture);
        return await db.ActivityTransitions.AsNoTracking()
            .Where(transition => transition.ActivityId == activityId && transition.TransitionKey == "complete")
            .Select(transition => transition.CreditedItemCount)
            .SingleAsync();
    }

    private async Task<DateTime> CompletedAtAsync(Fixture fixture, int activityId)
    {
        await using var db = NewContext(fixture);
        return await db.ActivityTransitions.AsNoTracking()
            .Where(transition => transition.ActivityId == activityId && transition.TransitionKey == "complete")
            .Select(transition => transition.OccurredOn)
            .SingleAsync();
    }

    private async Task<DateTime?> DeactivatedOnAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);
        return await db.Epas.AsNoTracking().Where(epa => epa.Id == fixture.EpaId).Select(epa => epa.DeactivatedOn).SingleAsync();
    }

    private sealed record Row(
        int CurriculumItemId, string TraineeUserId, int AcademicYear, int Semester, int CountsSoFar,
        int MinimumLevelReachedCount, int UnverifiedLevelCount, int? LastActivityId, DateOnly? LastObservedOn, string Keys);

    private sealed record Stamp(int ActivityId, int? CreditedItemCount, int? CreditScaleMismatchCount);

    private sealed record Snapshot(IReadOnlyList<Row> Rows, IReadOnlyList<Stamp> Stamps);

    /// <summary>
    /// Every progress row and every completion's stamp. The key set is parsed and re-serialised, because Postgres discards
    /// the submitted jsonb text and renders its own. Rows are compared by their natural key, not their id, because a
    /// rebuild would open a row the race failed to open with a new id.
    /// </summary>
    private async Task<Snapshot> SnapshotAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);

        var rows = (await db.CurriculumItemProgresses.AsNoTracking().ToListAsync())
            .Select(row => new Row(
                row.CurriculumItemId, row.TraineeUserId, row.AcademicYear, row.Semester, row.CountsSoFar,
                row.MinimumLevelReachedCount, row.UnverifiedLevelCount, row.LastActivityId, row.LastObservedOn,
                JsonSerializer.Serialize(JsonSerializer.Deserialize<string[]>(row.CreditedActivityKeysJson)!.Order(StringComparer.Ordinal))))
            .OrderBy(row => row.CurriculumItemId)
            .ThenBy(row => row.AcademicYear)
            .ThenBy(row => row.Semester)
            .ToList();

        var stamps = await db.ActivityTransitions.AsNoTracking()
            .Where(transition => transition.TransitionKey == "complete")
            .OrderBy(transition => transition.ActivityId)
            .Select(transition => new Stamp(transition.ActivityId, transition.CreditedItemCount, transition.CreditScaleMismatchCount))
            .ToListAsync();

        return new Snapshot(rows, stamps);
    }

    // ─── Gates and clocks ────────────────────────────────────────────────────

    /// <summary>Where one request stops: it says it has arrived, and waits to be let on.</summary>
    private sealed class Gate
    {
        private readonly TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _open = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Arrive() => _reached.TrySetResult();

        public Task WaitToBeOpenedAsync() => _open.Task.WaitAsync(Patience);

        public void Open() => _open.TrySetResult();

        /// <summary>Returns once the request is at the gate, and surfaces its failure if it failed before getting there.</summary>
        public async Task ReachedAsync(Task request)
        {
            var first = await Task.WhenAny(_reached.Task, request).WaitAsync(Patience);
            if (first == request)
            {
                await request;
                throw new InvalidOperationException("The request finished without reaching its gate.");
            }
        }
    }

    /// <summary>Stops the request just before its first save goes to the database, holding whatever it holds.</summary>
    private sealed class GateBeforeFirstSave(Gate gate) : SaveChangesInterceptor
    {
        private Gate? _gate = gate;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref _gate, null) is { } stop)
            {
                stop.Arrive();
                await stop.WaitToBeOpenedAsync();
            }

            return result;
        }
    }

    /// <summary>One server's clock for both requests: every read is a minute after the one before, whoever makes it.</summary>
    private sealed class SharedClock(DateTime start) : TimeProvider
    {
        private long _ticks = start.Ticks;

        public override DateTimeOffset GetUtcNow()
            => new(new DateTime(Interlocked.Add(ref _ticks, TimeSpan.TicksPerMinute), DateTimeKind.Utc), TimeSpan.Zero);
    }

    /// <summary>Stops the request at its first reading of the clock, after the reading is taken.</summary>
    private sealed class GatedClock(TimeProvider inner, Gate gate) : TimeProvider
    {
        private Gate? _gate = gate;

        public override DateTimeOffset GetUtcNow()
        {
            var now = inner.GetUtcNow();
            if (Interlocked.Exchange(ref _gate, null) is { } stop)
            {
                stop.Arrive();

                // The handler reads the clock synchronously, so it is held here on its own thread-pool thread.
                stop.WaitToBeOpenedAsync().GetAwaiter().GetResult();
            }

            return now;
        }
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private sealed record Fixture(string Schema, int InstitutionId, int ActivityTypeId, int EpaId)
    {
        /// <summary>What the named connection reports itself as to <c>pg_stat_activity</c>: unique to this test's schema.</summary>
        public string ApplicationName(string connection) => $"{Schema[..20]}-{connection}";
    }

    /// <summary>A seeded schema, a PAED-001 trainee from 2026, and a type crediting the EPA its data names.</summary>
    private async Task<Fixture> ArrangeAsync()
    {
        var schema = await SeededSchemaAsync();
        await using var db = NewContext(schema, applicationName: null, beforeSave: null);

        var institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        var curriculumId = await db.Curricula.Where(entity => entity.Name == PaediatricCurriculumName).Select(entity => entity.Id).SingleAsync();
        var epaId = await db.CurriculumItems
            .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null && item.Epa.Code == "PAED-001")
            .Select(item => item.EpaId)
            .SingleAsync();

        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = TraineeUserId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        });

        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var activityType = new ActivityType
        {
            Key = "wba_t230_postgres",
            Name = "WBA under test (T230)",
            Scope = ActivityScope.Institution,
            ScopeId = institutionId,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn,
            IsActive = true
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        db.ActivityTypes.Add(activityType);
        await db.SaveChangesAsync();

        return new Fixture(schema, institutionId, activityType.Id, epaId);
    }

    /// <summary>
    /// An activity the trainee has submitted and may now complete. The schema names no encounter-date field, so its
    /// encounter is the South African day it was created on.
    /// </summary>
    private async Task<int> AddSubmittedActivityAsync(Fixture fixture, DateTime createdOn)
    {
        await using var db = NewContext(fixture);

        var activity = new Activity
        {
            ActivityTypeId = fixture.ActivityTypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = TraineeUserId,
            CurrentState = "submitted",
            DataJson = $$"""{ "epa_id": {{fixture.EpaId}}, "score": 4 }""",
            CreatedOn = createdOn,
            UpdatedOn = createdOn.AddMinutes(5),
            ObservedOn = DateOnly.FromDateTime(createdOn),
            ObservedOnSource = ObservationDateSource.CreatedOn,
            InstitutionId = fixture.InstitutionId
        };

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "draft",
            ToState = "submitted",
            TransitionKey = "submit",
            ActorUserId = TraineeUserId,
            OccurredOn = createdOn.AddMinutes(5)
        });

        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        return activity.Id;
    }

    private static ClaimsPrincipal Trainee()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, TraineeUserId)], "IntegrationTest"));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "IntegrationTest"));

    // ─── Postgres plumbing, as in EpaActivePeriodPostgresTests ────────────────

    /// <summary>A fresh schema, migrated and seeded the way startup does it: DataSeeder, then the paediatric catalogue.</summary>
    private async Task<string> SeededSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema, applicationName: null, beforeSave: null))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema, applicationName: null, beforeSave: null))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema, applicationName: null, beforeSave: null))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

        return schema;
    }

    private ApplicationDbContext NewContext(
        Fixture fixture, string? connection = null, Gate? beforeSave = null, int? commandTimeoutSeconds = null)
        => NewContext(fixture.Schema, connection is null ? null : fixture.ApplicationName(connection), beforeSave, commandTimeoutSeconds);

    /// <param name="commandTimeoutSeconds">
    /// Shorter than Npgsql's 30 seconds, for a test that waits past it; null keeps the default, as the product does.
    /// </param>
    private ApplicationDbContext NewContext(string schema, string? applicationName, Gate? beforeSave, int? commandTimeoutSeconds = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
            ConnectionString(schema, applicationName),
            npgsql => npgsql.CommandTimeout(commandTimeoutSeconds));
        if (beforeSave is not null)
        {
            options.AddInterceptors(new GateBeforeFirstSave(beforeSave));
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>The schema's connection string, named so that <see cref="WaitsForALockAsync" /> can find it.</summary>
    private static string ConnectionString(string schema, string? applicationName)
        => TestDatabase.SchemaConnectionString(schema, applicationName);

    private async Task<NpgsqlConnection> OpenAsync(Fixture fixture, string? applicationName)
    {
        var connection = new NpgsqlConnection(ConnectionString(fixture.Schema, applicationName));
        await connection.OpenAsync();
        return connection;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private const string CreditsTheEpa = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

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
