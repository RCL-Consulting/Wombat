using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T281: an encounter observed after the programme's last day (<c>TraineeProfile.EndedOn</c>: completion or withdrawal)
/// credits nothing on that profile. One rule, in <c>CreditApplier.PlanAsync</c>, so the live completion and every replay
/// agree; and an end recorded after such an encounter was credited takes the credit back in the same save.
/// </summary>
/// <remarks>
/// The last day is 20 August 2026 throughout, recorded on 25 September (<see cref="Now" />), as in the lifecycle check
/// that found the defect (T252). An encounter on 10 August is inside the programme; one on 10 September is after it. Both
/// are semester 2, so the defect was one tally counting both. Every refusal and failure is checked against the audit trap:
/// nothing dirty is left, the audit pipeline's save of the same context writes nothing, and a fresh read finds nothing
/// changed.
/// </remarks>
public sealed class CreditAfterProgrammeEndTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;
    private const int CurriculumId = 3000;
    private const int EpaId = 5000;
    private const int ItemId = 4000;
    private const int ProfileId = 1;
    private const int WbaTypeId = 100;
    private const int MiniCexTypeId = 101;

    private const int InsideId = 200;
    private const int AfterId = 201;

    /// <summary>08:00 UTC on 25 September 2026, the same day in South Africa.</summary>
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 9, 25);
    private static readonly DateOnly LastDay = new(2026, 8, 20);
    private static readonly DateOnly Inside = new(2026, 8, 10);
    private static readonly DateOnly After = new(2026, 9, 10);

    // ---- the rule, in the plan -------------------------------------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AnEncounterAfterTheLastDay_PlansNoCredit_AndOneOnTheLastDay_Does(bool completed)
    {
        var options = await SeededAsync();
        await EndDirectlyAsync(options, LastDay, completed);

        await using var db = new ApplicationDbContext(options);
        var applier = new CreditApplier(db);

        (await PlanAsync(applier, LastDay.AddDays(1))).Credits.Should().BeEmpty(
            "an encounter after the programme's last day credits nothing on that profile (T281)");
        (await PlanAsync(applier, LastDay)).Credits.Should().ContainSingle(
            "the last day itself is inside the programme");
        (await PlanAsync(applier, Inside)).Credits.Should().ContainSingle();
    }

    [Fact]
    public async Task WhileTheProgrammeRuns_ALaterEncounterStillCredits()
    {
        var options = await SeededAsync();

        await using var db = new ApplicationDbContext(options);
        (await PlanAsync(new CreditApplier(db), After)).Credits.Should().ContainSingle("no end is recorded, so nothing is after it");
    }

    [Fact]
    public async Task AnEndBeingRecorded_IsJudgedAsRecorded_OnlyByAPlanThatIsHandedIt_ForTheProfileCreditAccruesAgainst()
    {
        // The replay a withdrawal runs judges against the end it has not yet saved (CreditSubject.PendingEnd), and only when
        // it names the profile credit accrues against. Every other plan reads the stored end.
        var options = await SeededAsync();

        await using var db = new ApplicationDbContext(options);
        var applier = new CreditApplier(db);

        (await PlanAsync(applier, After, new PendingProgrammeEnd(ProfileId, LastDay))).Credits.Should().BeEmpty(
            "the pending end counts as recorded");
        (await PlanAsync(applier, LastDay, new PendingProgrammeEnd(ProfileId, LastDay))).Credits.Should().ContainSingle(
            "the last day itself is inside the programme");
        (await PlanAsync(applier, After, new PendingProgrammeEnd(ProfileId + 1, LastDay))).Credits.Should().ContainSingle(
            "an end recorded on another profile says nothing about this one");
    }

    [Fact]
    public async Task AnUnsavedChange_ToAProfileTheRequestTracks_DoesNotChangeWhatCreditPlansAgainst()
    {
        // T281 review: credit reads the profile as stored. A request that has ended a profile without saving it, and asks
        // for a plan without handing the end over, plans as if the programme still ran: no plan reads another's unsaved
        // edit, and nothing scans the change tracker to find one.
        var options = await SeededAsync();

        await using var db = new ApplicationDbContext(options);
        (await db.Set<TraineeProfile>().SingleAsync()).Deactivate(LastDay, Today);

        (await PlanAsync(new CreditApplier(db), After)).Credits.Should().ContainSingle(
            "the end is not stored, and this plan was not handed it");
    }

    // ---- the live path ---------------------------------------------------------------------------------------------

    [Fact]
    public async Task AMiniCexObservedAfterTheLastDay_IsFiledAndCompleted_ButCreditsNothing_AndItsCompletionSaysSo()
    {
        // Not refused at filing, as a date before the programme start is (T160): the record is kept, and its completion is
        // stamped zero, which is the activity page's "counted towards no curriculum requirement" warning (T108).
        var options = await SeededAsync();
        await EndDirectlyAsync(options, LastDay, completed: false);

        var draft = await CreateAsync(options, MiniCexRequest(After));
        (await TransitionAsync(options, draft.Id, "submit", TraineeId)).CurrentState.Should().Be("requested");
        var done = await TransitionAsync(options, draft.Id, "complete", AssessorId, CompletionPatch);

        done.CurrentState.Should().Be("completed");
        done.Transitions.Single(transition => transition.TransitionKey == "complete").CreditedItemCount
            .Should().Be(0, "credit was evaluated and nothing counted");

        await using var verify = new ApplicationDbContext(options);
        (await verify.CurriculumItemProgresses.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AMiniCexObservedOnTheLastDay_Credits()
    {
        var options = await SeededAsync();
        await EndDirectlyAsync(options, LastDay, completed: false);

        var draft = await CreateAsync(options, MiniCexRequest(LastDay));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var done = await TransitionAsync(options, draft.Id, "complete", AssessorId, CompletionPatch);

        done.Transitions.Single(transition => transition.TransitionKey == "complete").CreditedItemCount.Should().Be(1);

        await using var verify = new ApplicationDbContext(options);
        (await verify.CurriculumItemProgresses.SingleAsync()).LastObservedOn.Should().Be(LastDay);
    }

    // ---- the rebuild -----------------------------------------------------------------------------------------------

    [Fact]
    public async Task ARebuild_TakesBackCreditForEncountersAfterAnEndAlreadyRecorded()
    {
        // An end saved before T281, with credit that accrued after it still in the tally. The rebuild is the repair tool,
        // and it applies the same rule as the live path.
        var options = await SeededAsync();
        await CreditBothAsync(options);
        await EndDirectlyAsync(options, LastDay, completed: false);

        await using (var db = new ApplicationDbContext(options))
        {
            (await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
                .Handle(new RebuildCurriculumProgressCommand(TestPrincipals.Administrator()), CancellationToken.None))
                .CreditApplications.Should().Be(1);
        }

        await AssertOnlyTheEncounterInsideCountsAsync(options);
    }

    // ---- recording the end takes the credit back -------------------------------------------------------------------

    [Fact]
    public async Task RecordingALastDay_BeforeACreditedEncounter_TakesItsCreditBack_InTheSameSave()
    {
        var options = await SeededAsync();
        await CreditBothAsync(options);
        await AssertBothCountAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            await DeactivateHandler(db).Handle(
                new DeactivateTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var profile = await verify.Set<TraineeProfile>().SingleAsync();
            profile.DeactivatedOn.Should().Be(LastDay);
            profile.IsActive.Should().BeFalse();
        }

        await AssertOnlyTheEncounterInsideCountsAsync(options);

        // The ended progress page reads the tally the replay rewrote: semester 2, which the programme ended in part-way,
        // shows only the encounter up to the last day.
        await using var read = new ApplicationDbContext(options);
        var summary = await TraineeQuotaProgressReader.ReadAsync(read, TraineeId, Today, CancellationToken.None);
        summary!.Ended!.EndedOn.Should().Be(LastDay);
        var current = summary.Items.Single().Current;
        current.Name.Should().Be("Semester 2, 2026");
        current.EndedPartWay.Should().BeTrue();
        current.Count.Should().Be(1);
        current.LastObservedOn.Should().Be(Inside);
    }

    [Fact]
    public async Task ACompletionDay_BeforeACreditedEncounter_TakesItsCreditBack_AndStillRemovesTheRoleAndEmails()
    {
        var options = await SeededAsync();
        await CreditBothAsync(options);

        var users = new Mock<IUserAdministrationService>();
        users.Setup(service => service.GetByIdAsync(TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails(TraineeId, "trainee@test.local", "Lerato", "Molefe", InstitutionId, [], [], [WombatRoles.Trainee]));
        var email = new Mock<IEmailSender>();

        await using (var db = new ApplicationDbContext(options))
        {
            await new CompleteTraineeProfileCommandHandler(db, users.Object, email.Object, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(Now))
                .Handle(new CompleteTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.Set<TraineeProfile>().SingleAsync()).CompletedOn.Should().Be(LastDay);
        }

        await AssertOnlyTheEncounterInsideCountsAsync(options);
        users.Verify(service => service.RemoveRoleAsync(TraineeId, WombatRoles.Trainee, It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(service => service.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnEndThatTakesNothingBack_IsSavedAlone_WithoutAReplay()
    {
        // Both encounters are before 20 September, so recording it changes nothing a replay would change because of it.
        // The applier throws if it is asked anything: the end must be saved without it.
        var options = await SeededAsync();
        await CreditBothAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            await new DeactivateTraineeProfileCommandHandler(db, new ThrowingCreditApplier(), new TraineeCreditLock(db), new FixedClock(Now)).Handle(
                new DeactivateTraineeProfileCommand(ProfileId, new DateOnly(2026, 9, 20), TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Set<TraineeProfile>().SingleAsync()).DeactivatedOn.Should().Be(new DateOnly(2026, 9, 20));
        await AssertBothCountAsync(options);
    }

    [Fact]
    public async Task WhenTheReplayFails_TheEndIsPutBackToo_AndTheAuditSaveCommitsNothing()
    {
        var options = await SeededAsync();
        await CreditBothAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            var act = () => new DeactivateTraineeProfileCommandHandler(db, new ThrowingCreditApplier(), new TraineeCreditLock(db), new FixedClock(Now)).Handle(
                new DeactivateTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");
            await AssertNothingToCommitAsync(db);
        }

        await AssertStillRunningAsync(options);
        await AssertBothCountAsync(options);
    }

    [Fact]
    public async Task WhenTheSaveFails_TheEndIsPutBackToo_AndTheAuditSaveCommitsNothing()
    {
        // The save is where change detection has already flagged the end Modified: putting the value back alone would not
        // be enough, and the audit pipeline's second save would commit it.
        var failingSave = new FailArmedSave();
        var options = await SeededAsync(failingSave);
        await CreditBothAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            failingSave.Arm();
            var act = () => DeactivateHandler(db).Handle(
                new DeactivateTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);

            await act.Should().ThrowAsync<DbUpdateException>();
            await AssertNothingToCommitAsync(db);
        }

        await AssertStillRunningAsync(options);
        await AssertBothCountAsync(options);
    }

    // ---- the end's hold (T281 review) -----------------------------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RecordingAnEnd_HoldsTheProfileBeforeReadingIt_AndCommitsTheHoldAfterItsSave(bool completing)
    {
        // The order is the guarantee: the hold keeps every completion of the trainee's out from before the end reads
        // whether it takes credit back until its save has committed. ProgrammeEndCreditRacePostgresTests drives the races.
        var options = await SeededAsync();
        await CreditBothAsync(options);

        var events = new List<string>();
        await using (var db = new ApplicationDbContext(options))
        {
            var hold = new RecordingEndLock(db, events);
            if (completing)
            {
                var users = new Mock<IUserAdministrationService>();
                users.Setup(service => service.RemoveRoleAsync(TraineeId, WombatRoles.Trainee, It.IsAny<CancellationToken>()))
                    .Callback(() => events.Add("remove role"))
                    .Returns(Task.CompletedTask);
                await new CompleteTraineeProfileCommandHandler(db, users.Object, Mock.Of<IEmailSender>(), new CreditApplier(db), hold, new FixedClock(Now))
                    .Handle(new CompleteTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
            }
            else
            {
                await new DeactivateTraineeProfileCommandHandler(db, new CreditApplier(db), hold, new FixedClock(Now))
                    .Handle(new DeactivateTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
            }
        }

        events.Should().Equal(completing
            ? [$"hold end {ProfileId} (anything read: False)", "commit (saved: True)", "remove role"]
            : [$"hold end {ProfileId} (anything read: False)", "commit (saved: True)"]);
        await AssertOnlyTheEncounterInsideCountsAsync(options);
    }

    [Fact]
    public async Task WhenTheEndsHoldCannotBeHad_NothingIsReadOrChanged()
    {
        // A hold that gives up waiting (EndBusy on PostgreSQL) comes before the first read, so the audit pipeline's save has
        // nothing to commit and the programme still runs.
        var options = await SeededAsync();
        await CreditBothAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            var act = () => new DeactivateTraineeProfileCommandHandler(db, new CreditApplier(db), new FailingEndLock(), new FixedClock(Now))
                .Handle(new DeactivateTraineeProfileCommand(ProfileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");
            db.ChangeTracker.Entries().Should().BeEmpty("the hold is taken before the profile is read");
            await AssertNothingToCommitAsync(db);
        }

        await AssertStillRunningAsync(options);
        await AssertBothCountAsync(options);
    }

    // ---- harness ---------------------------------------------------------------------------------------------------

    private static Task<CreditPlan> PlanAsync(CreditApplier applier, DateOnly observedOn, PendingProgrammeEnd? pendingEnd = null)
        => applier.PlanAsync(
            new CreditSubject(TraineeId, observedOn, ObservedOnDeclared: true, $$"""{ "epa_id": {{EpaId}}, "score": 4 }""",
                new DateTime(2026, 9, 24, 9, 0, 0, DateTimeKind.Utc)) { PendingEnd = pendingEnd },
            new ActivityType { CreditRulesJson = CreditsTheEpa, SchemaJson = SchemaJson });

    /// <summary>Writes down when the end's hold is asked for and committed, and what the request had read or saved by then.</summary>
    private sealed class RecordingEndLock(ApplicationDbContext db, List<string> events) : ITraineeCreditLock
    {
        public Task<ICreditHold> HoldForMoveAsync(int traineeProfileId, CancellationToken cancellationToken)
            => throw new NotSupportedException("No profile save is made here (T304).");

        public Task<ICreditHold> HoldForEndAsync(int traineeProfileId, CancellationToken cancellationToken)
        {
            events.Add($"hold end {traineeProfileId} (anything read: {db.ChangeTracker.Entries().Any()})");
            return Task.FromResult<ICreditHold>(new Hold(db, events));
        }

        public Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<string> traineeUserIds, CancellationToken cancellationToken)
            => throw new NotSupportedException("Recording an end never holds for credit.");

        private sealed class Hold(ApplicationDbContext db, List<string> events) : ICreditHold
        {
            public Task CommitAsync(CancellationToken cancellationToken)
            {
                events.Add($"commit (saved: {!db.ChangeTracker.HasChanges()})");
                return Task.CompletedTask;
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    /// <summary>An end's hold that cannot be had.</summary>
    private sealed class FailingEndLock : ITraineeCreditLock
    {
        public Task<ICreditHold> HoldForMoveAsync(int traineeProfileId, CancellationToken cancellationToken)
            => throw new NotSupportedException("No profile save is made here (T304).");

        public async Task<ICreditHold> HoldForEndAsync(int traineeProfileId, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new InvalidOperationException("induced failure while taking the end's hold");
        }

        public Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<string> traineeUserIds, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private static DeactivateTraineeProfileCommandHandler DeactivateHandler(ApplicationDbContext db)
        => new(db, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(Now));

    /// <summary>What the audit pipeline's catch does: save the request's context. Nothing may be dirty, or it is committed.</summary>
    private static async Task AssertNothingToCommitAsync(ApplicationDbContext db)
    {
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await db.SaveChangesAsync()).Should().Be(0);
        db.ChangeTracker.Clear();
    }

    private static async Task AssertStillRunningAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var verify = new ApplicationDbContext(options);
        var profile = await verify.Set<TraineeProfile>().SingleAsync();
        profile.IsActive.Should().BeTrue();
        profile.DeactivatedOn.Should().BeNull();
        profile.EndedOn.Should().BeNull();
    }

    private static async Task AssertBothCountAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var verify = new ApplicationDbContext(options);
        var row = await verify.CurriculumItemProgresses.SingleAsync();
        (row.AcademicYear, row.Semester, row.CountsSoFar).Should().Be((2026, 2, 2));
        Keys(row).Should().Equal($"{InsideId}:complete", $"{AfterId}:complete");
        (await StampAsync(verify, InsideId)).Should().Be(1);
        (await StampAsync(verify, AfterId)).Should().Be(1);
    }

    private static async Task AssertOnlyTheEncounterInsideCountsAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var verify = new ApplicationDbContext(options);
        var row = await verify.CurriculumItemProgresses.SingleAsync();
        (row.AcademicYear, row.Semester, row.CountsSoFar, row.MinimumLevelReachedCount).Should().Be((2026, 2, 1, 1));
        Keys(row).Should().Equal($"{InsideId}:complete");
        row.LastObservedOn.Should().Be(Inside);
        row.LastActivityId.Should().Be(InsideId);

        (await StampAsync(verify, InsideId)).Should().Be(1);
        (await StampAsync(verify, AfterId)).Should().Be(0, "its completion now says it counted towards nothing");
        (await verify.Activities.CountAsync()).Should().Be(2, "the record is kept");
    }

    private static string[] Keys(CurriculumItemProgress row)
        => JsonSerializer.Deserialize<string[]>(row.CreditedActivityKeysJson)!.Order(StringComparer.Ordinal).ToArray();

    private static async Task<int?> StampAsync(ApplicationDbContext db, int activityId)
        => (await db.ActivityTransitions.SingleAsync(transition =>
            transition.ActivityId == activityId && transition.TransitionKey == "complete")).CreditedItemCount;

    /// <summary>
    /// Two completions, one inside the programme and one after its future last day, credited as a running programme
    /// credits them (by the rebuild, which also stamps their completions).
    /// </summary>
    private static async Task CreditBothAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using (var seed = new ApplicationDbContext(options))
        {
            AddCompletedActivity(seed, InsideId, Inside);
            AddCompletedActivity(seed, AfterId, After);
            await seed.SaveChangesAsync();
        }

        await using var db = new ApplicationDbContext(options);
        (await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
            .Handle(new RebuildCurriculumProgressCommand(TestPrincipals.Administrator()), CancellationToken.None))
            .CreditApplications.Should().Be(2, "guard: while the programme runs, both credit");
    }

    /// <summary>An end recorded without the handlers: as on a database whose end was saved before T281.</summary>
    private static async Task EndDirectlyAsync(DbContextOptions<ApplicationDbContext> options, DateOnly lastDay, bool completed)
    {
        await using var db = new ApplicationDbContext(options);
        var profile = await db.Set<TraineeProfile>().SingleAsync();
        if (completed)
        {
            profile.Complete(lastDay, Today);
        }
        else
        {
            profile.Deactivate(lastDay, Today);
        }

        await db.SaveChangesAsync();
    }

    private static void AddCompletedActivity(ApplicationDbContext db, int activityId, DateOnly observedOn)
    {
        var filedOn = observedOn.AddDays(2).ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        var activity = new Activity
        {
            Id = activityId,
            ActivityTypeId = WbaTypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{EpaId}}, "score": 4 }""",
            InstitutionId = InstitutionId,
            CreatedOn = filedOn.AddHours(-1),
            UpdatedOn = filedOn,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared
        };

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "draft",
            ToState = "submitted",
            TransitionKey = "submit",
            ActorUserId = TraineeId,
            OccurredOn = filedOn.AddMinutes(-5)
        });
        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "submitted",
            ToState = "completed",
            TransitionKey = "complete",
            ActorUserId = TraineeId,
            OccurredOn = filedOn
        });

        db.Activities.Add(activity);
    }

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, string dataJson)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(new CreateActivityInput(MiniCexTypeId, TraineeId, TraineeId, dataJson, Principal(TraineeId)));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options, int activityId, string transitionKey, string actorId, string? patch = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(
            new TransitionActivityInput(activityId, transitionKey, actorId, Principal(actorId), patch, Note: null));
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(
            db,
            new SchemaValidator(),
            new WorkflowEvaluator(),
            new CreditApplier(db),
            new FieldPermissionEvaluator(),
            new FixedClock(Now));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private static string MiniCexRequest(DateOnly observedOn) => $$"""
        {
          "epa_id": {{EpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "{{observedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}}",
          "setting": "ward",
          "presenting_problem": "Bronchiolitis",
          "complexity": "moderate"
        }
        """;

    private const string CompletionPatch = """
        { "overall_level": 3, "strengths": "Clear.", "improvements": "Earlier escalation.", "plan": "Repeat." }
        """;

    /// <param name="interceptors">Registered on every context of the test, as the failing save needs.</param>
    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptors)
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        // The handlers load the profile with its curriculum, sub-speciality and speciality, which are required.
        db.Institutions.Add(new Institution { Id = InstitutionId, Name = "Host", ShortCode = "HOST", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = InstitutionId, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = CurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1" });

        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = ItemId,
            CurriculumId = CurriculumId,
            EpaId = EpaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = ProfileId,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        db.ActivityTypes.Add(PublishedType(WbaTypeId, "wba_under_test", SchemaJson, WorkflowJson, CreditsTheEpa));
        db.ActivityTypes.Add(PublishedType(
            MiniCexTypeId, "mini_cex_cpsa", ReadSeed("mini_cex_cpsa", "schema.json"), ReadSeed("mini_cex_cpsa", "workflow.json"),
            ReadSeed("mini_cex_cpsa", "credit.json")));

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityType PublishedType(int id, string key, string schemaJson, string workflowJson, string creditJson)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        return type;
    }

    private static string ReadSeed(string folder, string file)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", folder, file));

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

    /// <summary>Refuses every question, so a test proves nothing was asked of credit, or that a failure mid-replay is undone.</summary>
    private sealed class ThrowingCreditApplier : ICreditApplier
    {
        public Task<CreditPlan> PlanAsync(CreditSubject subject, ActivityType activityType, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("induced failure: credit was asked");

        public CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity)
            => throw new InvalidOperationException("induced failure: credit was applied");

        public Task<CreditApplicationResult> ApplyAsync(Activity completedActivity, ActivityType activityType, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("induced failure: credit was replayed");
    }

    /// <summary>Makes the next SaveChanges fail once armed, as a deadlock or a dropped connection would.</summary>
    private sealed class FailArmedSave : SaveChangesInterceptor
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

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
