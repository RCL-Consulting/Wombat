using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Activities;
using Wombat.Domain.Audit;
using Wombat.Domain.Institutions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T281 (review) on a real PostgreSQL server: a completion racing the recording of its trainee's programme end ends with
/// the credit a rebuild would write, so an encounter after the last day counts towards nothing however the two interleave.
/// </summary>
/// <remarks>
/// <para>
/// Each race is driven from two contexts on two connections, as <c>EpaCreditRacePostgresTests</c> drives T230's. One request
/// is stopped just before its first save, holding what it holds; the other is started, and the test waits until the server
/// reports it waiting for a lock, or until it has finished, which is what it did before the review. Then the gate opens and
/// both run to the end. The completion is the live path itself (<see cref="ActivityService.TransitionAsync" />); the end is
/// the withdrawal's handler (<see cref="DeactivateTraineeProfileCommandHandler" />), and the reactivation the EPA edit's.
/// </para>
/// <para>
/// The last day is 20 August 2026, recorded on 25 September. The schema names no encounter-date field, so an activity's
/// encounter is the South African day it was created on: 10 September, after the last day, unless a test says otherwise.
/// The outcome is judged the way T230's is: an Administrator rebuild afterwards changes nothing, neither a progress row nor
/// a completion's stamp.
/// </para>
/// <para>
/// Each test works on a schema of its own and names its connections, so the wait it looks for can only be its own.
/// </para>
/// </remarks>
public sealed class ProgrammeEndCreditRacePostgresTests : IAsyncLifetime
{
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";
    private const string TraineeUserId = "trainee-t281";

    private static readonly DateOnly LastDay = new(2026, 8, 20);
    private static readonly DateTime RecordedAt = new(2026, 9, 25, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FiledAfterTheEnd = new(2026, 9, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(20);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    // ─── An end racing a completion ──────────────────────────────────────────

    [Fact]
    public async Task AnEnd_WaitsForACompletionInFlight_AndTakesBackTheCreditItSaved()
    {
        // Before the review: the end read that nothing after its last day had credited while the completion was still in
        // flight, and saved alone; the completion then saved its credit, which stayed in the ended programme's tally.
        var fixture = await ArrangeAsync();
        var filed = await AddSubmittedActivityAsync(fixture, FiledAfterTheEnd);

        var completionGate = new Gate();
        var completion = Task.Run(() => CompleteAsync(fixture, filed, "completion", completionGate));
        await completionGate.ReachedAsync(completion);

        var end = Task.Run(() => RecordTheEndAsync(fixture, "end"));
        var endWaited = await WaitsForALockAsync(fixture, "end", end);

        completionGate.Open();
        await completion;
        await end;

        endWaited.Should().BeTrue("the completion holds its trainee until its save commits");
        (await EndedOnAsync(fixture)).Should().Be(LastDay);
        (await StampAsync(fixture, filed)).Should().Be(0, "the end read the credit once it had committed, and took it back");
        (await ProgressRowCountAsync(fixture)).Should().Be(0);
        await ARebuildChangesNothingAsync(fixture);
    }

    /// <param name="endReplays">
    /// Whether the end has credit to take back (an earlier PAED-001 encounter after the last day, already credited), so the
    /// save it is stopped at is its replay's. The completion, on PAED-002, then opens a progress row the replay never read:
    /// the hole neither the rows' xmin tokens nor the replay's check for a row that arrived mid-rebuild can see.
    /// </param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ACompletion_WaitsForAnEndInFlight_AndCreditsNothingAfterItsLastDay(bool endReplays)
    {
        // Before the review: the completion read the profile while the end was in flight, found no end, credited, and saved;
        // the end, which had decided what to take back before that credit existed, then saved too.
        var fixture = await ArrangeAsync();
        int? earlier = null;
        if (endReplays)
        {
            // Observed on 25 August, after the last day, and credited while the programme ran.
            earlier = await AddSubmittedActivityAsync(fixture, new DateTime(2026, 8, 25, 9, 0, 0, DateTimeKind.Utc));
            await CompleteAsync(fixture, earlier.Value, "earlier");
            (await StampAsync(fixture, earlier.Value)).Should().Be(1, "guard: credited while the programme ran");
        }

        var filed = await AddSubmittedActivityAsync(fixture, FiledAfterTheEnd, fixture.SecondEpaId);

        var endGate = new Gate();
        var end = Task.Run(() => RecordTheEndAsync(fixture, "end", endGate));
        await endGate.ReachedAsync(end);

        var completion = Task.Run(() => CompleteAsync(fixture, filed, "completion"));
        var completionWaited = await WaitsForALockAsync(fixture, "completion", completion);

        endGate.Open();
        await end;
        await completion;

        completionWaited.Should().BeTrue("the end holds the profile until its save commits");
        (await StampAsync(fixture, filed)).Should().Be(0, "the completion read the end once it had committed");
        if (earlier is { } taken)
        {
            (await StampAsync(fixture, taken)).Should().Be(0, "the end's replay took it back");
        }

        (await ProgressRowCountAsync(fixture)).Should().Be(0);
        await ARebuildChangesNothingAsync(fixture);
    }

    [Fact]
    public async Task AReactivation_WaitsForAnEndInFlight_AndCreditsNoPausedCompletionAfterItsLastDay()
    {
        // A completion during the EPA's pause credits nothing and is stamped zero, so an end has nothing to take back from
        // it. Before the review the reactivation read the profile while the end was in flight, credited the paused
        // completion (after the last day) and saved; the end then saved alone.
        var fixture = await ArrangeAsync();
        await DeactivateTheEpaAsync(fixture);
        var filed = await AddSubmittedActivityAsync(fixture, FiledAfterTheEnd);
        await CompleteAsync(fixture, filed, "paused");
        (await StampAsync(fixture, filed)).Should().Be(0, "guard: completed during the pause");

        var endGate = new Gate();
        var end = Task.Run(() => RecordTheEndAsync(fixture, "end", endGate));
        await endGate.ReachedAsync(end);

        var reactivation = Task.Run(() => ReactivateTheEpaAsync(fixture, "reactivation"));
        var reactivationWaited = await WaitsForALockAsync(fixture, "reactivation", reactivation);

        endGate.Open();
        await end;
        var reactivated = await reactivation;

        reactivationWaited.Should().BeTrue("the end holds the profile until its save commits");
        reactivated.CompletionsCredited.Should().Be(0, "the reactivation read the end once it had committed");
        (await StampAsync(fixture, filed)).Should().Be(0);
        await ARebuildChangesNothingAsync(fixture);
    }

    // ─── A move racing a completion (T304) ───────────────────────────────────

    [Fact]
    public async Task ACompletion_WaitsForAMoveInFlight_AndCreditsAgainstTheMovedProfile()
    {
        // A profile save that moves the trainee to the institution's newly adopted version holds the profile, as an end
        // does, until its save commits. Without the hold the completion read the profile as it stood, credited the old
        // version's item, and saved after the move's replay had read the trainee's rows: credit left on the old version.
        var fixture = await ArrangeAsync();
        var move = await ArrangeReAdoptionAsync(fixture);
        var earlier = await AddSubmittedActivityAsync(fixture, new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc));
        await CompleteAsync(fixture, earlier, "earlier");
        var filed = await AddSubmittedActivityAsync(fixture, FiledAfterTheEnd, fixture.SecondEpaId);

        var moveGate = new Gate();
        var moving = Task.Run(() => MoveAsync(fixture, move, "move", moveGate));
        await moveGate.ReachedAsync(moving);

        var completion = Task.Run(() => CompleteAsync(fixture, filed, "completion"));
        var completionWaited = await WaitsForALockAsync(fixture, "completion", completion);

        moveGate.Open();
        var saved = await moving;
        await completion;

        completionWaited.Should().BeTrue("the move holds the profile until its save commits");
        saved.Message.Should().Be("Trainee profile saved. 1 completion was checked against T304.2, and 1 counts towards it.");
        (await CreditedCurriculaAsync(fixture)).Should().Equal(
            [move.NewCurriculumId], "both completions credit the version the trainee was moved to, and nothing is left on the old one");
        (await StampAsync(fixture, filed)).Should().Be(1);
        await ARebuildChangesNothingAsync(fixture);
    }

    [Fact]
    public async Task AMove_WaitsForACompletionInFlight_AndReplaysTheCreditItSaved()
    {
        var fixture = await ArrangeAsync();
        var move = await ArrangeReAdoptionAsync(fixture);
        var filed = await AddSubmittedActivityAsync(fixture, FiledAfterTheEnd);

        var completionGate = new Gate();
        var completion = Task.Run(() => CompleteAsync(fixture, filed, "completion", completionGate));
        await completionGate.ReachedAsync(completion);

        var moving = Task.Run(() => MoveAsync(fixture, move, "move"));
        var moveWaited = await WaitsForALockAsync(fixture, "move", moving);

        completionGate.Open();
        await completion;
        await moving;

        moveWaited.Should().BeTrue("the completion holds its trainee until its save commits");
        (await CreditedCurriculaAsync(fixture)).Should().Equal(
            [move.NewCurriculumId], "the move read the credit once it had committed, and replayed it onto the new version");
        await ARebuildChangesNothingAsync(fixture);
    }

    // ─── What the holds hold off, and how a hold gives up ────────────────────

    [Fact]
    public async Task ACompletionsHold_DoesNotHoldOffAnotherCompletion_ButHoldsOffAnEnd()
    {
        var fixture = await ArrangeAsync();

        await using var db = NewContext(fixture, "completion");
        await using var hold = await new TraineeCreditLock(db).HoldForCreditAsync([TraineeUserId], CancellationToken.None);

        await using var other = await OpenAsync(fixture, fixture.ApplicationName("other"));
        await ExecuteAsync(other, "SET lock_timeout = '3s'");

        await ExecuteAsync(other, "BEGIN");
        var anotherCompletion = () => ExecuteAsync(
            other, $"""SELECT 1 FROM "TraineeProfiles" WHERE "UserId" = '{TraineeUserId}' FOR SHARE""");
        await anotherCompletion.Should().NotThrowAsync("completions of one trainee never wait for one another");
        await ExecuteAsync(other, "ROLLBACK");

        var anEnd = () => ExecuteAsync(
            other, $"""SELECT 1 FROM "TraineeProfiles" WHERE "Id" = {fixture.ProfileId} FOR NO KEY UPDATE""");
        (await anEnd.Should().ThrowAsync<PostgresException>())
            .Which.SqlState.Should().Be(PostgresErrorCodes.LockNotAvailable, "an end waits for a completion's hold");
    }

    [Fact]
    public async Task AnEndThatWaitsPastTheCommandTimeout_IsRefusedAsBusy_AndLeavesNothingForTheAuditSaveToCommit()
    {
        var fixture = await ArrangeAsync();
        await using var completion = await HoldTheProfileAsync(fixture, $"""WHERE "UserId" = '{TraineeUserId}' FOR SHARE""");

        await using var db = NewContext(fixture, "end", commandTimeoutSeconds: 1);
        var record = () => new DeactivateTraineeProfileCommandHandler(db, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(RecordedAt))
            .Handle(new DeactivateTraineeProfileCommand(fixture.ProfileId, LastDay, Administrator()), CancellationToken.None);
        var refusal = (await record.Should().ThrowAsync<InvalidOperationException>()).Which;

        refusal.Message.Should().Be(TraineeCreditLock.EndBusy, "the page shows the message, not Npgsql's");
        await AuditSaveCommitsOnlyItsRowAsync(fixture, db, refusal);
        await completion.DisposeAsync();

        (await EndedOnAsync(fixture)).Should().BeNull("the end was refused before its first read");
    }

    [Fact]
    public async Task ACompletionThatWaitsPastTheCommandTimeout_IsRefusedAsBusy_AndLeavesNothingForTheAuditSaveToCommit()
    {
        // The completion holds its EPA first, in the transaction the trainee's hold then waits in: giving up must roll both
        // back and leave no transaction for the audit save.
        var fixture = await ArrangeAsync();
        var filed = await AddSubmittedActivityAsync(fixture, FiledAfterTheEnd);
        await using var end = await HoldTheProfileAsync(fixture, $"""WHERE "Id" = {fixture.ProfileId} FOR NO KEY UPDATE""");

        await using var db = NewContext(fixture, "completion", commandTimeoutSeconds: 1);
        var service = Service(db, new FixedClock(RecordedAt));

        var complete = () => service.TransitionAsync(new TransitionActivityInput(
            filed, "complete", TraineeUserId, Trainee(), DataPatchJson: null, Note: null));
        var refusal = (await complete.Should().ThrowAsync<InvalidOperationException>()).Which;

        refusal.Message.Should().Be(TraineeCreditLock.CreditBusy, "the page shows the message, not Npgsql's");
        await AuditSaveCommitsOnlyItsRowAsync(fixture, db, refusal);
        await end.DisposeAsync();

        await using var verify = NewContext(fixture);
        var stored = await verify.Activities.AsNoTracking()
            .Include(entity => entity.Transitions)
            .SingleAsync(entity => entity.Id == filed);
        stored.CurrentState.Should().Be("submitted");
        stored.Transitions.Should().ContainSingle("only the submission; the completion was refused before its first change");
        (await ProgressRowCountAsync(fixture)).Should().Be(0);
    }

    // ─── The requests ────────────────────────────────────────────────────────

    /// <summary>The trainee completes the activity through the live path, on a connection of its own.</summary>
    private async Task CompleteAsync(Fixture fixture, int activityId, string connection, Gate? beforeSave = null)
    {
        await using var db = NewContext(fixture, connection, beforeSave);
        await Service(db, new FixedClock(RecordedAt)).TransitionAsync(new TransitionActivityInput(
            activityId, "complete", TraineeUserId, Trainee(), DataPatchJson: null, Note: null));
    }

    /// <summary>An administrator records 20 August as the trainee's last day (Deactivate), on 25 September.</summary>
    private async Task RecordTheEndAsync(Fixture fixture, string connection, Gate? beforeSave = null)
    {
        await using var db = NewContext(fixture, connection, beforeSave);
        await new DeactivateTraineeProfileCommandHandler(db, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(RecordedAt))
            .Handle(new DeactivateTraineeProfileCommand(fixture.ProfileId, LastDay, Administrator()), CancellationToken.None);
    }

    /// <summary>An administrator moves the trainee to the version the institution has just adopted, on 25 September.</summary>
    private async Task<UpdateTraineeProfileResult> MoveAsync(Fixture fixture, ReAdoption move, string connection, Gate? beforeSave = null)
    {
        await using var db = NewContext(fixture, connection, beforeSave);
        return await new UpdateTraineeProfileCommandHandler(db, Users(), new CreditApplier(db), new TraineeCreditLock(db))
            .Handle(
                new UpdateTraineeProfileCommand(
                    fixture.ProfileId, move.NewCurriculumId, new DateOnly(2026, 1, 1), new DateOnly(2029, 12, 31), Administrator()),
                CancellationToken.None);
    }

    private sealed record ReAdoption(int OldCurriculumId, int NewCurriculumId);

    /// <summary>
    /// The trainee's institution adopted the paediatric curriculum, which the trainee is pinned to, and has now re-adopted
    /// a clone of it, "T304.2", as KGK re-adopts 11.2 at runbook Step 6.32.
    /// </summary>
    private async Task<ReAdoption> ArrangeReAdoptionAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);
        var profile = await db.TraineeProfiles.SingleAsync(entity => entity.Id == fixture.ProfileId);
        var old = await db.Curricula.Include(entity => entity.Items).SingleAsync(entity => entity.Id == profile.CurriculumId);

        foreach (var adoption in await db.InstitutionCurriculumAdoptions
                     .Where(entity => entity.InstitutionId == fixture.InstitutionId && entity.SubSpecialityId == old.SubSpecialityId)
                     .ToListAsync())
        {
            adoption.IsActive = false;
        }

        var superseded = new InstitutionCurriculumAdoption
        {
            InstitutionId = fixture.InstitutionId, CurriculumId = old.Id, SubSpecialityId = old.SubSpecialityId,
            AdoptedOn = new DateOnly(2026, 1, 1), IsActive = false
        };
        db.InstitutionCurriculumAdoptions.Add(superseded);
        await db.SaveChangesAsync();

        var clone = old.CloneAsNewVersion("T304.2", new DateOnly(2026, 9, 1), effectiveTo: null);
        db.Curricula.Add(clone);
        await db.SaveChangesAsync();

        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption
        {
            InstitutionId = fixture.InstitutionId, CurriculumId = clone.Id, SubSpecialityId = clone.SubSpecialityId,
            AdoptedOn = new DateOnly(2026, 9, 1), IsActive = true
        });
        profile.AdoptionId = superseded.Id;
        await db.SaveChangesAsync();

        return new ReAdoption(old.Id, clone.Id);
    }

    /// <summary>The curricula whose items hold any of the trainee's progress rows.</summary>
    private async Task<List<int>> CreditedCurriculaAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);
        var itemIds = await db.CurriculumItemProgresses.AsNoTracking()
            .Where(row => row.TraineeUserId == TraineeUserId)
            .Select(row => row.CurriculumItemId)
            .ToListAsync();
        return await db.CurriculumItems.AsNoTracking()
            .Where(item => itemIds.Contains(item.Id))
            .Select(item => item.CurriculumId)
            .Distinct()
            .ToListAsync();
    }

    /// <summary>The trainee as the user directory answers for them; the scope write it is asked for does nothing.</summary>
    private static IUserAdministrationService Users() => DispatchProxy.Create<IUserAdministrationService, UsersStub>();

    public class UsersStub : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            => targetMethod?.Name switch
            {
                nameof(IUserAdministrationService.GetByIdAsync) => Task.FromResult<UserIdentityDetails?>(
                    new UserIdentityDetails(TraineeUserId, "trainee@test", "Thabo", "Ndlovu", null, [], [], [WombatRoles.Trainee])),
                nameof(IUserAdministrationService.UpdateScopeAsync) => Task.CompletedTask,
                _ => throw new NotSupportedException(targetMethod?.Name)
            };
    }

    private async Task DeactivateTheEpaAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);
        await new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), new FixedClock(new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc)))
            .Handle(new DeactivateEpaCommand(fixture.EpaId, Administrator()), CancellationToken.None);
    }

    private async Task<UpdateEpaResult> ReactivateTheEpaAsync(Fixture fixture, string connection)
    {
        UpdateEpaCommand command;
        await using (var read = NewContext(fixture))
        {
            var epa = await read.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId);
            command = new UpdateEpaCommand(
                epa.Id, epa.SubSpecialityId, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category,
                IsActive: true, Administrator());
        }

        await using var db = NewContext(fixture, connection);
        return await new UpdateEpaCommandHandler(
                db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), new FixedClock(RecordedAt))
            .Handle(command, CancellationToken.None);
    }

    private static ActivityService Service(ApplicationDbContext db, TimeProvider clock)
        => new(
            db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(), clock,
            new EpaCreditLock(db), new TraineeCreditLock(db));

    /// <summary>
    /// Another request holding the trainee's profile as a completion (<c>FOR SHARE</c>) or an end (<c>FOR NO KEY UPDATE</c>)
    /// would, in a transaction left open until the returned connection is disposed.
    /// </summary>
    private async Task<NpgsqlConnection> HoldTheProfileAsync(Fixture fixture, string whereAndLock)
    {
        var connection = await OpenAsync(fixture, fixture.ApplicationName("holder"));
        await ExecuteAsync(connection, "BEGIN");
        await ExecuteAsync(connection, $"""SELECT 1 FROM "TraineeProfiles" {whereAndLock}""");
        return connection;
    }

    /// <summary>
    /// What the audit pipeline's catch does after a refusal that is not a refused save: add its row to the request's own
    /// context and save it. The context must still work after the timeout, and the save must carry that row and nothing else.
    /// </summary>
    private async Task AuditSaveCommitsOnlyItsRowAsync(Fixture fixture, ApplicationDbContext db, Exception refusal)
    {
        db.Database.CurrentTransaction.Should().BeNull("a refused hold leaves no transaction for the audit save to write into");
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        await new AuditWriter(db).WriteAsync(AuditEntry.Create(
            RecordedAt, AuditCategory.Command, "T281Refused", success: false, errorMessage: refusal.Message));
        db.ChangeTracker.Clear();

        await using var verify = NewContext(fixture);
        (await verify.Set<AuditEntry>().AsNoTracking().CountAsync(entry => entry.Action == "T281Refused")).Should().Be(1);
    }

    /// <summary>
    /// Waits until the server reports the named connection waiting for a lock (true), or the request has finished without
    /// waiting (false), which is what every request did before the review.
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

    private async Task<int> ProgressRowCountAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);
        return await db.CurriculumItemProgresses.AsNoTracking().CountAsync(row => row.TraineeUserId == TraineeUserId);
    }

    private async Task<DateOnly?> EndedOnAsync(Fixture fixture)
    {
        await using var db = NewContext(fixture);
        return await db.TraineeProfiles.AsNoTracking()
            .Where(profile => profile.Id == fixture.ProfileId)
            .Select(profile => profile.CompletedOn ?? profile.DeactivatedOn)
            .SingleAsync();
    }

    private sealed record Row(
        int CurriculumItemId, string TraineeUserId, int AcademicYear, int Semester, int CountsSoFar,
        int MinimumLevelReachedCount, int UnverifiedLevelCount, int? LastActivityId, DateOnly? LastObservedOn, string Keys);

    private sealed record Stamp(int ActivityId, int? CreditedItemCount, int? CreditScaleMismatchCount);

    private sealed record Snapshot(IReadOnlyList<Row> Rows, IReadOnlyList<Stamp> Stamps);

    /// <summary>
    /// Every progress row and every completion's stamp. The key set is parsed and re-serialised, because Postgres discards
    /// the submitted jsonb text and renders its own.
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

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private sealed record Fixture(string Schema, int InstitutionId, int ActivityTypeId, int EpaId, int SecondEpaId, int ProfileId)
    {
        /// <summary>What the named connection reports itself as to <c>pg_stat_activity</c>: unique to this test's schema.</summary>
        public string ApplicationName(string connection) => $"{Schema[..20]}-{connection}";
    }

    /// <summary>A seeded schema, a paediatric trainee from 2026, and a type crediting the EPA its data names (PAED-001 unless a test names PAED-002).</summary>
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
        var secondEpaId = await db.CurriculumItems
            .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null && item.Epa.Code == "PAED-002")
            .Select(item => item.EpaId)
            .SingleAsync();

        var profile = new TraineeProfile
        {
            UserId = TraineeUserId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        };
        db.TraineeProfiles.Add(profile);

        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var activityType = new ActivityType
        {
            Key = "wba_t281_postgres",
            Name = "WBA under test (T281)",
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

        return new Fixture(schema, institutionId, activityType.Id, epaId, secondEpaId, profile.Id);
    }

    /// <summary>An activity the trainee has submitted and may now complete; its encounter is the day it was created on.</summary>
    private async Task<int> AddSubmittedActivityAsync(Fixture fixture, DateTime createdOn, int? epaId = null)
    {
        await using var db = NewContext(fixture);

        var activity = new Activity
        {
            ActivityTypeId = fixture.ActivityTypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = TraineeUserId,
            CurrentState = "submitted",
            DataJson = $$"""{ "epa_id": {{epaId ?? fixture.EpaId}}, "score": 4 }""",
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

    // ─── Postgres plumbing, as in EpaCreditRacePostgresTests ─────────────────

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
            TestDatabase.SchemaConnectionString(schema, applicationName),
            npgsql => npgsql.CommandTimeout(commandTimeoutSeconds));
        if (beforeSave is not null)
        {
            options.AddInterceptors(new GateBeforeFirstSave(beforeSave));
        }

        return new ApplicationDbContext(options.Options);
    }

    private async Task<NpgsqlConnection> OpenAsync(Fixture fixture, string? applicationName)
    {
        var connection = new NpgsqlConnection(TestDatabase.SchemaConnectionString(fixture.Schema, applicationName));
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
