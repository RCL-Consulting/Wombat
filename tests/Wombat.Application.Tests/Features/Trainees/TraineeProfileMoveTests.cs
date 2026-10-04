using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T304: a trainee profile save is not an admission. An unchanged curriculum keeps the adoption it is pinned to, superseded
/// or not; only a move is judged, and only into the institution's active adoption for the trainee's discipline. A move, or
/// a change of programme start, replays the trainee's credit in the same save, under the trainee's credit hold.
/// </summary>
/// <remarks>
/// The institution adopted 11.1 (adoption 1), then re-adopted 11.2 (adoption 2), as KGK does at runbook Step 6.32. The
/// trainee is pinned to 11.1 and has one completion, PAED-001 observed on 10 August 2026, credited on 11.1's item.
/// </remarks>
public sealed class TraineeProfileMoveTests
{
    private const string TraineeId = "trainee-1";
    private const int InstitutionId = 10;
    private const int ProfileId = 1;
    private const int WbaTypeId = 100;
    private const int ActivityId = 200;

    private const int V111 = 3000;
    private const int V112 = 3001;
    private const int OtherDiscipline = 3002;
    private const int Superseded = 1;
    private const int Active = 2;

    private const int EpaId = 5000;
    private const int ItemOn111 = 4000;
    private const int ItemOn112 = 4001;

    private static readonly DateOnly Start = new(2025, 1, 1);
    private static readonly DateOnly Expected = new(2028, 1, 14);
    private static readonly DateOnly Observed = new(2026, 8, 10);

    // ---- an unchanged curriculum keeps its pin ---------------------------------------------------------------------

    [Fact]
    public async Task AfterAReAdoption_SavingAProfilePinnedToTheSupersededVersion_WithOnlyTheExpectedCompletionChanged_Succeeds()
    {
        // Step 6.34: Dr Dlamini stays on 11.1 after KGK re-adopted 11.2. The save was refused as an admission into a
        // version the institution no longer admits into. The credit applier throws if asked: nothing is replayed.
        var options = await SeededAsync();
        await CreditAsync(options);

        UpdateTraineeProfileResult result;
        await using (var db = new ApplicationDbContext(options))
        {
            result = await Handler(db, new ThrowingCreditApplier()).Handle(
                new UpdateTraineeProfileCommand(ProfileId, V111, Start, new DateOnly(2028, 7, 14), TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);
        }

        result.Message.Should().Be("Trainee profile saved.");
        result.Recount.Should().BeNull();

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Set<TraineeProfile>().SingleAsync();
        stored.CurriculumId.Should().Be(V111);
        stored.AdoptionId.Should().Be(Superseded, "an unchanged curriculum keeps the adoption it is pinned to");
        stored.ExpectedCompletionDate.Should().Be(new DateOnly(2028, 7, 14));
        await AssertCountsOnAsync(verify, ItemOn111);
    }

    // ---- a move is judged, and only a move ------------------------------------------------------------------------

    [Fact]
    public async Task AMoveBackToTheSupersededVersion_IsRefusedBeforeAnyWrite()
    {
        var options = await SeededAsync(pinnedTo: V112, adoption: Active);

        await using (var db = new ApplicationDbContext(options))
        {
            var act = () => Handler(db).Handle(
                new UpdateTraineeProfileCommand(ProfileId, V111, Start, Expected, TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(UpdateTraineeProfileCommandHandler.MoveNotAdopted);
            await AssertNothingToCommitAsync(db);
        }

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Set<TraineeProfile>().SingleAsync();
        (stored.CurriculumId, stored.AdoptionId, stored.ExpectedCompletionDate).Should().Be((V112, Active, Expected));
    }

    [Fact]
    public async Task AMoveIntoAnotherDisciplinesAdoptedVersion_IsRefusedBeforeAnyWrite()
    {
        // The move is into the institution's active adoption for the trainee's own discipline, which is what the picker
        // offers. Another discipline's adoption is an admission into another programme, not a move.
        var options = await SeededAsync();

        await using (var db = new ApplicationDbContext(options))
        {
            var act = () => Handler(db).Handle(
                new UpdateTraineeProfileCommand(ProfileId, OtherDiscipline, Start, Expected, TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(UpdateTraineeProfileCommandHandler.MoveNotAdopted);
            await AssertNothingToCommitAsync(db);
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Set<TraineeProfile>().SingleAsync()).CurriculumId.Should().Be(V111);
    }

    [Fact]
    public async Task AMoveTo112_ReCreditsTheTraineesCompletionsOn112sItems_AndRemoves111sTallies_InOneSave()
    {
        // Steps 6.36-6.38: Dr Ndlovu read zero on 11.2 until a global rebuild. Now the move replays him in its own save.
        var saves = new CountingSaves();
        var options = await SeededAsync(interceptors: saves);
        await CreditAsync(options);

        UpdateTraineeProfileResult result;
        await using (var db = new ApplicationDbContext(options))
        {
            saves.Reset();
            result = await Handler(db).Handle(
                new UpdateTraineeProfileCommand(ProfileId, V112, Start, Expected, TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);
        }

        saves.Count.Should().Be(1, "the move and its replay are one save");
        result.Recount.Should().Be(new TraineeCreditRecount(CurriculumMoved: true, CompletionsCounted: 1));
        result.Message.Should().Be("Trainee profile saved. 1 completion was counted again against 11.2.");
        result.Profile.CurriculumVersion.Should().Be("11.2");

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Set<TraineeProfile>().SingleAsync();
        (stored.CurriculumId, stored.AdoptionId).Should().Be((V112, Active));
        await AssertCountsOnAsync(verify, ItemOn112);
    }

    [Fact]
    public async Task WhenTheMovesReplayFails_TheProfileStaysOn111_AndTheAuditSaveCommitsNothing()
    {
        var options = await SeededAsync();
        await CreditAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            var act = () => Handler(db, new ThrowingCreditApplier()).Handle(
                new UpdateTraineeProfileCommand(ProfileId, V112, Start, new DateOnly(2028, 7, 14), TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");
            await AssertNothingToCommitAsync(db);
        }

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Set<TraineeProfile>().SingleAsync();
        (stored.CurriculumId, stored.AdoptionId, stored.ExpectedCompletionDate).Should().Be((V111, Superseded, Expected));
        await AssertCountsOnAsync(verify, ItemOn111);
    }

    [Fact]
    public async Task WhenTheMovesSaveFails_TheProfileIsPutBackToo_AndTheAuditSaveCommitsNothing()
    {
        // The save is where change detection has flagged the profile Modified: putting the values back alone would not do.
        var failingSave = new FailArmedSave();
        var options = await SeededAsync(interceptors: failingSave);
        await CreditAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            failingSave.Arm();
            var act = () => Handler(db).Handle(
                new UpdateTraineeProfileCommand(ProfileId, V112, Start, Expected, TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<DbUpdateException>();
            await AssertNothingToCommitAsync(db);
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Set<TraineeProfile>().SingleAsync()).CurriculumId.Should().Be(V111);
        await AssertCountsOnAsync(verify, ItemOn111);
    }

    // ---- a change of programme start ------------------------------------------------------------------------------

    [Fact]
    public async Task AProgrammeStartChange_Replays_AndJudgesACompletionsMinimumAtTheNewTrainingYear()
    {
        // 11.1's item asks for level 4 in year 1 and level 3 after it. Started on 1 January 2025, the 10 August 2026
        // encounter is in year 2 and its level 3 meets the minimum; started on 1 January 2026 it is in year 1, and does not.
        var options = await SeededAsync();
        await CreditAsync(options);
        await using (var check = new ApplicationDbContext(options))
        {
            (await check.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(1, "guard: year 2");
        }

        UpdateTraineeProfileResult result;
        await using (var db = new ApplicationDbContext(options))
        {
            result = await Handler(db).Handle(
                new UpdateTraineeProfileCommand(ProfileId, V111, new DateOnly(2026, 1, 1), Expected, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        result.Recount.Should().Be(new TraineeCreditRecount(CurriculumMoved: false, CompletionsCounted: 1));
        result.Message.Should().Be("Trainee profile saved. 1 completion was counted again from the new programme start date.");

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Set<TraineeProfile>().SingleAsync();
        (stored.ProgrammeStartDate, stored.AdoptionId).Should().Be((new DateOnly(2026, 1, 1), Superseded));
        var row = await verify.CurriculumItemProgresses.SingleAsync();
        row.CurriculumItemId.Should().Be(ItemOn111);
        (row.CountsSoFar, row.MinimumLevelReachedCount).Should().Be((1, 0), "the encounter is now in year 1, whose minimum is 4");
    }

    [Theory]
    [InlineData(0, "Trainee profile saved. They have no completions to count against 11.2.")]
    [InlineData(2, "Trainee profile saved. 2 completions were counted again against 11.2.")]
    public void TheMessage_CountsTheCompletionsInPlainWords(int counted, string expected)
    {
        var profile = new TraineeProfileDto(
            ProfileId, TraineeId, "t@test", "Thabo", "Ndlovu", V112, "Paediatrics", "11.2", 1, "Paediatrics", 1, "General",
            Start, Expected, IsActive: true);

        new UpdateTraineeProfileResult(profile, new TraineeCreditRecount(true, counted)).Message.Should().Be(expected);
        new UpdateTraineeProfileResult(profile, new TraineeCreditRecount(false, 0)).Message
            .Should().Be("Trainee profile saved. They have no completions to count again.");
    }

    // ---- the hold -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ASave_HoldsTheProfileBeforeReadingIt_AndCommitsTheHoldAfterItsSave_BeforeTheScopeIsRewritten()
    {
        // The order is the guarantee: no completion of the trainee's credits against the old pin while the move saves.
        // ProgrammeEndCreditRacePostgresTests drives the race on a real server.
        var options = await SeededAsync();
        await CreditAsync(options);

        var events = new List<string>();
        await using (var db = new ApplicationDbContext(options))
        {
            var users = Users();
            users.Setup(service => service.UpdateScopeAsync(
                    TraineeId, InstitutionId, It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                .Callback(() => events.Add("scope"))
                .Returns(Task.CompletedTask);

            await new UpdateTraineeProfileCommandHandler(db, users.Object, new CreditApplier(db), new RecordingMoveLock(db, events))
                .Handle(new UpdateTraineeProfileCommand(ProfileId, V112, Start, Expected, TestPrincipals.Administrator()), CancellationToken.None);
        }

        events.Should().Equal($"hold move {ProfileId} (anything read: False)", "commit (saved: True)", "scope");
    }

    [Fact]
    public async Task WhenTheMovesHoldCannotBeHad_NothingIsReadOrChanged()
    {
        var options = await SeededAsync();
        await CreditAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            var act = () => new UpdateTraineeProfileCommandHandler(db, Users().Object, new CreditApplier(db), new FailingMoveLock())
                .Handle(new UpdateTraineeProfileCommand(ProfileId, V112, Start, Expected, TestPrincipals.Administrator()), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");
            db.ChangeTracker.Entries().Should().BeEmpty("the hold is taken before the profile is read");
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Set<TraineeProfile>().SingleAsync()).CurriculumId.Should().Be(V111);
    }

    // ---- harness ---------------------------------------------------------------------------------------------------

    private static UpdateTraineeProfileCommandHandler Handler(ApplicationDbContext db, ICreditApplier? creditApplier = null)
        => new(db, Users().Object, creditApplier ?? new CreditApplier(db), new TraineeCreditLock(db));

    private static Mock<IUserAdministrationService> Users()
    {
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.GetByIdAsync(TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails(TraineeId, "ndlovu@kgk", "Thabo", "Ndlovu", InstitutionId, [1], [1], [WombatRoles.Trainee]));
        return users;
    }

    /// <summary>The single progress row the one completion makes, on <paramref name="itemId" />, and its stamp.</summary>
    private static async Task AssertCountsOnAsync(ApplicationDbContext verify, int itemId)
    {
        var row = await verify.CurriculumItemProgresses.SingleAsync();
        row.CurriculumItemId.Should().Be(itemId);
        (row.AcademicYear, row.Semester, row.CountsSoFar, row.MinimumLevelReachedCount).Should().Be((2026, 2, 1, 1));
        JsonSerializer.Deserialize<string[]>(row.CreditedActivityKeysJson).Should().Equal($"{ActivityId}:complete");
        (await verify.ActivityTransitions.SingleAsync(transition => transition.TransitionKey == "complete"))
            .CreditedItemCount.Should().Be(1);
    }

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

    /// <summary>The completion, credited as a running programme credits it (by the rebuild, which also stamps it).</summary>
    private static async Task CreditAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        (await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
            .Handle(new RebuildCurriculumProgressCommand(TestPrincipals.Administrator()), CancellationToken.None))
            .CreditApplications.Should().Be(1, "guard: the completion credits the pinned version's item");
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(
        int pinnedTo = V111, int adoption = Superseded, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptors)
            .Options;

        await using var db = new ApplicationDbContext(options);

        db.Institutions.Add(new Institution { Id = InstitutionId, Name = "KGK", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = InstitutionId, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 2, SpecialityId = 1, Name = "Neonatology", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = V111, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1" });
        db.Curricula.Add(new Curriculum { Id = V112, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.2" });
        db.Curricula.Add(new Curriculum { Id = OtherDiscipline, SubSpecialityId = 2, Name = "CPSA Neonatology", Version = "1.0" });

        db.Set<InstitutionCurriculumAdoption>().AddRange(
            new InstitutionCurriculumAdoption { Id = Superseded, InstitutionId = InstitutionId, CurriculumId = V111, SubSpecialityId = 1, AdoptedOn = new DateOnly(2025, 1, 1), IsActive = false },
            new InstitutionCurriculumAdoption { Id = Active, InstitutionId = InstitutionId, CurriculumId = V112, SubSpecialityId = 1, AdoptedOn = new DateOnly(2026, 9, 1), IsActive = true },
            new InstitutionCurriculumAdoption { Id = 3, InstitutionId = InstitutionId, CurriculumId = OtherDiscipline, SubSpecialityId = 2, AdoptedOn = new DateOnly(2025, 1, 1), IsActive = true });

        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });

        // Level 4 in year 1, level 3 after it (T073).
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = ItemOn111, CurriculumId = V111, EpaId = EpaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, MinimumLevelByStageJson = """{ "1": 4 }""", WindowMonths = 12
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = ItemOn112, CurriculumId = V112, EpaId = EpaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, WindowMonths = 12
        });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = ProfileId,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = pinnedTo,
            AdoptionId = adoption,
            ProgrammeStartDate = Start,
            ExpectedCompletionDate = Expected,
            IsActive = true
        });

        db.ActivityTypes.Add(PublishedType());
        AddCompletedActivity(db);

        await db.SaveChangesAsync();
        return options;
    }

    private static void AddCompletedActivity(ApplicationDbContext db)
    {
        var filedOn = Observed.AddDays(2).ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        var activity = new Activity
        {
            Id = ActivityId,
            ActivityTypeId = WbaTypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{EpaId}}, "score": 3 }""",
            InstitutionId = InstitutionId,
            CreatedOn = filedOn.AddHours(-1),
            UpdatedOn = filedOn,
            ObservedOn = Observed,
            ObservedOnSource = ObservationDateSource.Declared
        };

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = ActivityId, FromState = "draft", ToState = "submitted", TransitionKey = "submit",
            ActorUserId = TraineeId, OccurredOn = filedOn.AddMinutes(-5)
        });
        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = ActivityId, FromState = "submitted", ToState = "completed", TransitionKey = "complete",
            ActorUserId = TraineeId, OccurredOn = filedOn
        });

        db.Activities.Add(activity);
    }

    private static ActivityType PublishedType()
    {
        var type = new ActivityType
        {
            Id = WbaTypeId, Key = "wba_under_test", Name = "WBA under test", Scope = ActivityScope.Global, Version = 1,
            SchemaJson = SchemaJson, WorkflowJson = WorkflowJson, CreditRulesJson = CreditsTheEpa, DisplayFieldsJson = "[]",
            OwnerUserId = "system", CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = WbaTypeId, Version = 1, SchemaJson = SchemaJson, WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa, DisplayFieldsJson = "[]", PublishedByUserId = "system",
            PublishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        return type;
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

    /// <summary>Writes down when the move's hold is asked for and committed, and what the request had read or saved by then.</summary>
    private sealed class RecordingMoveLock(ApplicationDbContext db, List<string> events) : ITraineeCreditLock
    {
        public Task<ICreditHold> HoldForMoveAsync(int traineeProfileId, CancellationToken cancellationToken)
        {
            events.Add($"hold move {traineeProfileId} (anything read: {db.ChangeTracker.Entries().Any()})");
            return Task.FromResult<ICreditHold>(new Hold(db, events));
        }

        public Task<ICreditHold> HoldForEndAsync(int traineeProfileId, CancellationToken cancellationToken)
            => throw new NotSupportedException("A profile save never holds for an end.");

        public Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<string> traineeUserIds, CancellationToken cancellationToken)
            => throw new NotSupportedException("A profile save never holds for credit.");

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

    /// <summary>A move's hold that cannot be had.</summary>
    private sealed class FailingMoveLock : ITraineeCreditLock
    {
        public async Task<ICreditHold> HoldForMoveAsync(int traineeProfileId, CancellationToken cancellationToken)
        {
            await Task.Yield();
            throw new InvalidOperationException("induced failure while taking the move's hold");
        }

        public Task<ICreditHold> HoldForEndAsync(int traineeProfileId, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<string> traineeUserIds, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    /// <summary>Counts the saves that reach the database.</summary>
    private sealed class CountingSaves : SaveChangesInterceptor
    {
        public int Count { get; private set; }

        public void Reset() => Count = 0;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Count++;
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
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
}
