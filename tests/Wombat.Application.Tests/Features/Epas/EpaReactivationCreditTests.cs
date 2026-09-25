using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T196, D48: deactivating an EPA pauses its credit, and reactivating it credits what was filed meanwhile, for the admin
/// who can reactivate it, without an Administrator rebuild.
/// </summary>
/// <remarks>
/// <para>
/// Before T196 a completion while an EPA was inactive was stamped zero for good: reactivating put the EPA back in force for
/// the next completion only, and the one thing that credited the paused ones was a rebuild, which only an Administrator
/// can run. A CollegeAdmin or InstitutionalAdmin who reactivated an EPA left every paused completion uncredited.
/// </para>
/// <para>
/// "Live" credit here is <see cref="CreditApplier.ApplyAsync" /> over an activity already carrying its completion, which
/// judges the EPA at the completion's own time and stamps it, as <c>ActivityService</c> does. Deactivating and
/// reactivating go through the handlers, with a pinned clock.
/// </para>
/// </remarks>
public sealed class EpaReactivationCreditTests
{
    private const int CollegeId = 1;
    private const int SubSpecialityId = 1;
    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 11;
    private const int CurriculumId = 3000;
    private const int CreditingTypeId = 100;
    private const int TwoEpaTypeId = 101;
    private const int NonCreditingTypeId = 102;

    private const int NationalEpaId = 5000;
    private const int OtherNationalEpaId = 6000;
    private const int LocalEpaId = 7000;
    private const int NationalItemId = 4000;
    private const int OtherNationalItemId = 4001;
    private const int LocalItemId = 4002;

    private static readonly DateTime DeactivatedAt = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ReactivatedAt = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ReactivatingANationalEpa_AsItsCollegeAdmin_CreditsWhatWasCompletedWhileItWasInactive()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        // In force: credited live.
        await CompleteLiveAsync(options, activityId: 200, "trainee-1", NationalEpaId, completedAt: new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));

        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));

        // Paused: credits nothing, stamped zero.
        await CompleteLiveAsync(options, activityId: 201, "trainee-1", NationalEpaId, completedAt: new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc));
        (await StampAsync(options, 201)).Should().Be(0);
        (await KeysAsync(options)).Should().BeEquivalentTo("200:complete");

        var result = await ReactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));

        result.CompletionsCredited.Should().Be(1);
        result.Epa.IsActive.Should().BeTrue();
        (await KeysAsync(options)).Should().BeEquivalentTo("200:complete", "201:complete");
        (await StampAsync(options, 201)).Should().Be(1, "the T108 warning goes once the paused credit has landed");
        (await StampAsync(options, 200)).Should().Be(1, "a completion credited while in force is not touched");

        await using var verify = new ApplicationDbContext(options);
        var epa = await verify.Epas.SingleAsync(entity => entity.Id == NationalEpaId);
        (epa.IsActive, epa.DeactivatedOn).Should().Be((true, (DateTime?)null));
    }

    [Fact]
    public async Task Reactivation_WritesProgressExactlyAsARebuildWould()
    {
        // Two paused completions in one semester for one trainee (one row, opened by the first and incremented by the
        // second), one for another trainee, one before the pause, and one on another EPA during the pause (credited
        // live, and untouched). An Administrator rebuild afterwards must find nothing to change.
        var options = NewDatabase();
        await SeedAsync(options);

        await CompleteLiveAsync(options, 200, "trainee-1", NationalEpaId, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));
        await CompleteLiveAsync(options, 201, "trainee-1", NationalEpaId, new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc));
        await CompleteLiveAsync(options, 202, "trainee-1", NationalEpaId, new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc));
        await CompleteLiveAsync(options, 203, "trainee-2", NationalEpaId, new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc));
        await CompleteLiveAsync(options, 204, "trainee-1", OtherNationalEpaId, new DateTime(2026, 8, 6, 9, 0, 0, DateTimeKind.Utc));

        (await ReactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId))).CompletionsCredited.Should().Be(3);

        var afterReactivation = await SnapshotAsync(options);
        afterReactivation.Rows.Should().HaveCount(4, "trainee-1 S1, trainee-1 S2 (two completions), trainee-2 S2, and the other EPA's row");
        afterReactivation.Rows.Single(row => row.TraineeUserId == "trainee-1" && row.CurriculumItemId == NationalItemId && row.Semester == 2)
            .CountsSoFar.Should().Be(2, "the second paused completion found the row the first one opened");

        await using (var db = new ApplicationDbContext(options))
        {
            var rebuild = await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
                .Handle(new RebuildCurriculumProgressCommand(TestPrincipals.Administrator()), CancellationToken.None);
            rebuild.ProgressRowsRemoved.Should().Be(0);
        }

        (await SnapshotAsync(options)).Should().BeEquivalentTo(afterReactivation);
    }

    [Fact]
    public async Task ReactivatingALocalEpa_AsItsOwningInstitutionalAdmin_CreditsWhatWasPaused_AndAnotherInstitutionsAdminIsRefused()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        await DeactivateAsync(options, LocalEpaId, TestPrincipals.InstitutionalAdmin(InstitutionId));
        await CompleteLiveAsync(options, 200, "trainee-1", LocalEpaId, new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc));
        (await KeysAsync(options)).Should().BeEmpty();

        var refused = () => ReactivateAsync(options, LocalEpaId, TestPrincipals.InstitutionalAdmin(OtherInstitutionId));
        await refused.Should().ThrowAsync<UnauthorizedAccessException>();
        (await KeysAsync(options)).Should().BeEmpty("a refused reactivation credits nothing");

        (await ReactivateAsync(options, LocalEpaId, TestPrincipals.InstitutionalAdmin(InstitutionId))).CompletionsCredited.Should().Be(1);
        (await KeysAsync(options)).Should().BeEquivalentTo("200:complete");
    }

    [Fact]
    public async Task ReactivatingOneEpa_LeavesAnotherThatIsStillInactivePaused()
    {
        // A reactivation credits its own EPA's items and no others: the other EPA's paused completion is a candidate (it
        // was completed during this pause too), but its credit waits for its own reactivation.
        var options = NewDatabase();
        await SeedAsync(options);
        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));
        await DeactivateAsync(options, OtherNationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));
        await CompleteLiveAsync(options, 200, "trainee-1", NationalEpaId, new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc));
        await CompleteLiveAsync(options, 201, "trainee-1", OtherNationalEpaId, new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc));

        (await ReactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId))).CompletionsCredited.Should().Be(1);

        (await KeysAsync(options)).Should().BeEquivalentTo("200:complete");
        (await StampAsync(options, 201)).Should().Be(0);

        (await ReactivateAsync(options, OtherNationalEpaId, TestPrincipals.CollegeAdmin(CollegeId))).CompletionsCredited.Should().Be(1);
        (await KeysAsync(options)).Should().BeEquivalentTo("200:complete", "201:complete");
    }

    [Fact]
    public async Task Reactivation_WhoseCreditCannotBePlanned_ChangesNothing()
    {
        // The audit pipeline's catch saves the request's DbContext, so a failure after the reactivation would commit it
        // without the credit it owes. Every read happens first: a failure while planning leaves the context clean.
        var options = NewDatabase();
        await SeedAsync(options);
        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));
        await CompleteLiveAsync(options, 200, "trainee-1", NationalEpaId, new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc));

        await using (var db = new ApplicationDbContext(options))
        {
            var handler = new UpdateEpaCommandHandler(db, new PlanningFails(), new EpaCreditLock(db), new FixedClock(ReactivatedAt));
            var reactivate = () => handler.Handle(Update(NationalEpaId, isActive: true, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);

            await reactivate.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");

            db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged).Should().BeEmpty();
            (await db.SaveChangesAsync()).Should().Be(0, "the audit pipeline's save must find nothing to commit");
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Epas.SingleAsync(entity => entity.Id == NationalEpaId)).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivating_StampsWhenThePauseBegan_AndDeactivatingAgainKeepsIt()
    {
        // One timestamp is the whole history the credit rule needs only if a second deactivation cannot move it later:
        // that would put completions the live path paused before the pause began, and a rebuild would credit them.
        var options = NewDatabase();
        await SeedAsync(options);

        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId), at: DeactivatedAt);
        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId), at: DeactivatedAt.AddDays(10));

        await using (var db = new ApplicationDbContext(options))
        {
            await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new FixedClock(DeactivatedAt.AddDays(20)))
                .Handle(Update(NationalEpaId, isActive: false, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);
        }

        await using var verify = new ApplicationDbContext(options);
        var epa = await verify.Epas.SingleAsync(entity => entity.Id == NationalEpaId);
        (epa.IsActive, epa.DeactivatedOn).Should().Be((false, (DateTime?)DeactivatedAt));
    }

    [Fact]
    public async Task SavingWithActiveUnticked_DeactivatesFromNow()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new FixedClock(DeactivatedAt))
                .Handle(Update(NationalEpaId, isActive: false, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);

            result.Epa.IsActive.Should().BeFalse();
            result.CompletionsCredited.Should().Be(0);
        }

        await using var verify = new ApplicationDbContext(options);
        (await verify.Epas.SingleAsync(entity => entity.Id == NationalEpaId)).DeactivatedOn.Should().Be(DeactivatedAt);
    }

    [Fact]
    public async Task Reactivation_AddsToAStampThatAlreadyCountsAnotherEpa_AndMatchesARebuild()
    {
        // T196 review. A completion whose type credits two EPAs, one of them paused: live, it credits the other EPA and is
        // stamped 1. Reactivating adds the paused EPA's item to that stamp rather than replacing it, which is what a
        // rebuild stamps. Every other fixture credits one EPA, so replacing the stamp passed every suite.
        var options = NewDatabase();
        await SeedAsync(options);
        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));

        await CompleteLiveAsync(
            options, 300, "trainee-1", NationalEpaId, new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc),
            typeId: TwoEpaTypeId,
            dataJson: $$"""{ "epa_id": {{NationalEpaId}}, "second_epa_id": {{OtherNationalEpaId}}, "score": 4 }""");
        (await StampAsync(options, 300)).Should().Be(1, "only the EPA in force credited when it completed");

        (await ReactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId))).CompletionsCredited.Should().Be(1);

        (await StampAsync(options, 300)).Should().Be(2, "the stamp counts both items the completion has now credited");
        (await KeysAsync(options)).Should().BeEquivalentTo("300:complete", "300:complete");

        var afterReactivation = await SnapshotAsync(options);
        await using (var db = new ApplicationDbContext(options))
        {
            await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
                .Handle(new RebuildCurriculumProgressCommand(TestPrincipals.Administrator()), CancellationToken.None);
        }

        (await SnapshotAsync(options)).Should().BeEquivalentTo(afterReactivation, "a rebuild stamps the same sum");
    }

    [Fact]
    public async Task Reactivation_ReadsOnlyCompletionsThatCouldCredit()
    {
        // T196 review. The candidates were every activity with any transition since the pause began, drafts and types
        // that credit nothing included, each loaded with its type's versions and its transitions into an interactive save.
        // For an EPA inactive since before T196 (its pause dated from its creation), that was the whole table.
        var options = NewDatabase();
        await SeedAsync(options);
        await DeactivateAsync(options, NationalEpaId, TestPrincipals.CollegeAdmin(CollegeId));

        var during = new DateTime(2026, 8, 4, 9, 0, 0, DateTimeKind.Utc);
        await CompleteLiveAsync(options, 200, "trainee-1", NationalEpaId, during);

        // A draft of a crediting type, still with its assessor; and a completion of a type that credits nothing.
        await AddUncreditedActivityAsync(options, 201, CreditingTypeId, "submitted", during.AddHours(1));
        await AddUncreditedActivityAsync(options, 202, NonCreditingTypeId, "completed", during.AddHours(2));

        await using var db = new ApplicationDbContext(options);
        var result = await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new FixedClock(ReactivatedAt))
            .Handle(Update(NationalEpaId, isActive: true, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);

        result.CompletionsCredited.Should().Be(1);
        db.ChangeTracker.Entries<Activity>().Select(entry => entry.Entity.Id).Should().BeEquivalentTo([200],
            "only a terminal completion pinned to rules that credit can hold paused credit");
    }

    // ─── Harness ─────────────────────────────────────────────────────────────

    private static async Task DeactivateAsync(
        DbContextOptions<ApplicationDbContext> options, int epaId, ClaimsPrincipal principal, DateTime? at = null)
    {
        await using var db = new ApplicationDbContext(options);
        await new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), new FixedClock(at ?? DeactivatedAt))
            .Handle(new DeactivateEpaCommand(epaId, principal), CancellationToken.None);
    }

    private static async Task<UpdateEpaResult> ReactivateAsync(DbContextOptions<ApplicationDbContext> options, int epaId, ClaimsPrincipal principal)
    {
        await using var db = new ApplicationDbContext(options);
        return await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new FixedClock(ReactivatedAt))
            .Handle(Update(epaId, isActive: true, principal), CancellationToken.None);
    }

    private static UpdateEpaCommand Update(int epaId, bool isActive, ClaimsPrincipal principal)
        => new(epaId, SubSpecialityId, $"EPA-{epaId}", $"EPA {epaId}", null, null, EpaCategory.Core, isActive, principal);

    /// <summary>
    /// A completion as the live path credits it: in its own context, at the completion's own moment, stamped with what it
    /// credited.
    /// </summary>
    private static async Task CompleteLiveAsync(
        DbContextOptions<ApplicationDbContext> options, int activityId, string subjectUserId, int epaId, DateTime completedAt,
        int typeId = CreditingTypeId, string? dataJson = null)
    {
        await using (var seed = new ApplicationDbContext(options))
        {
            var activity = new Activity
            {
                Id = activityId,
                ActivityTypeId = typeId,
                SchemaVersion = 1,
                SubjectUserId = subjectUserId,
                CreatedByUserId = subjectUserId,
                CurrentState = "completed",
                DataJson = dataJson ?? $$"""{ "epa_id": {{epaId}}, "score": 4 }""",
                CreatedOn = completedAt.AddHours(-1),
                UpdatedOn = completedAt,
                ObservedOn = DateOnly.FromDateTime(completedAt),
                InstitutionId = InstitutionId
            };

            activity.Transitions.Add(new ActivityTransition
            {
                ActivityId = activityId,
                FromState = "draft",
                ToState = "submitted",
                TransitionKey = "submit",
                ActorUserId = subjectUserId,
                OccurredOn = completedAt.AddMinutes(-5)
            });

            activity.Transitions.Add(new ActivityTransition
            {
                ActivityId = activityId,
                FromState = "submitted",
                ToState = "completed",
                TransitionKey = "complete",
                ActorUserId = subjectUserId,
                OccurredOn = completedAt
            });

            seed.Activities.Add(activity);
            await seed.SaveChangesAsync();
        }

        await using var db = new ApplicationDbContext(options);
        var stored = await db.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == activityId);
        var (creditRulesJson, schemaJson) = typeId == TwoEpaTypeId ? (CreditsTwoEpas, TwoEpaSchemaJson) : (CreditsTheEpa, SchemaJson);
        var credited = await new CreditApplier(db).ApplyAsync(stored, new ActivityType { CreditRulesJson = creditRulesJson, SchemaJson = schemaJson });

        var completion = stored.Transitions.Single(transition => transition.TransitionKey == "complete");
        completion.CreditedItemCount = credited.UpdatedRows.Count;
        completion.CreditScaleMismatchCount = credited.ScaleMismatchCount;
        await db.SaveChangesAsync();
    }

    /// <summary>An activity nothing credited: its one transition happened during the pause.</summary>
    private static async Task AddUncreditedActivityAsync(
        DbContextOptions<ApplicationDbContext> options, int activityId, int typeId, string state, DateTime movedAt)
    {
        await using var db = new ApplicationDbContext(options);

        var activity = new Activity
        {
            Id = activityId,
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = state,
            DataJson = $$"""{ "epa_id": {{NationalEpaId}}, "score": 4 }""",
            CreatedOn = movedAt.AddHours(-1),
            UpdatedOn = movedAt,
            ObservedOn = DateOnly.FromDateTime(movedAt),
            InstitutionId = InstitutionId
        };

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "draft",
            ToState = state,
            TransitionKey = state == "completed" ? "complete" : "submit",
            ActorUserId = "trainee-1",
            OccurredOn = movedAt
        });

        db.Activities.Add(activity);
        await db.SaveChangesAsync();
    }

    private static async Task<int?> StampAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return (await db.ActivityTransitions.SingleAsync(transition =>
            transition.ActivityId == activityId && transition.TransitionKey == "complete")).CreditedItemCount;
    }

    private static async Task<IReadOnlyList<string>> KeysAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        return (await db.CurriculumItemProgresses.Select(row => row.CreditedActivityKeysJson).ToListAsync())
            .SelectMany(json => JsonSerializer.Deserialize<string[]>(json)!)
            .ToList();
    }

    private sealed record Row(
        int CurriculumItemId, string TraineeUserId, int AcademicYear, int Semester, int CountsSoFar,
        int MinimumLevelReachedCount, int UnverifiedLevelCount, int? LastActivityId, DateOnly? LastObservedOn, string Keys);

    private sealed record Snapshot(IReadOnlyList<Row> Rows, IReadOnlyList<(int ActivityId, int? CreditedItemCount)> Stamps);

    private static async Task<Snapshot> SnapshotAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        var rows = (await db.CurriculumItemProgresses.AsNoTracking().ToListAsync())
            .Select(row => new Row(
                row.CurriculumItemId, row.TraineeUserId, row.AcademicYear, row.Semester, row.CountsSoFar,
                row.MinimumLevelReachedCount, row.UnverifiedLevelCount, row.LastActivityId, row.LastObservedOn,
                JsonSerializer.Serialize(JsonSerializer.Deserialize<string[]>(row.CreditedActivityKeysJson)!.Order(StringComparer.Ordinal))))
            .OrderBy(row => row.CurriculumItemId).ThenBy(row => row.TraineeUserId, StringComparer.Ordinal)
            .ThenBy(row => row.AcademicYear).ThenBy(row => row.Semester)
            .ToList();

        var stamps = (await db.ActivityTransitions.AsNoTracking()
                .Where(transition => transition.TransitionKey == "complete")
                .Select(transition => new { transition.ActivityId, transition.CreditedItemCount })
                .ToListAsync())
            .OrderBy(stamp => stamp.ActivityId)
            .Select(stamp => (stamp.ActivityId, stamp.CreditedItemCount))
            .ToList();

        return new Snapshot(rows, stamps);
    }

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);

        db.Colleges.Add(new College { Id = CollegeId, Name = "CMSA", ShortCode = "CMSA", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = CollegeId, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = SubSpecialityId, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Institutions.AddRange(
            new Institution { Id = InstitutionId, Name = "Inst A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = OtherInstitutionId, Name = "Inst B", ShortCode = "IB", IsActive = true, CreatedOn = DateTime.UtcNow });

        db.Epas.AddRange(
            new Epa { Id = NationalEpaId, SubSpecialityId = SubSpecialityId, Code = $"EPA-{NationalEpaId}", Title = $"EPA {NationalEpaId}" },
            new Epa { Id = OtherNationalEpaId, SubSpecialityId = SubSpecialityId, Code = $"EPA-{OtherNationalEpaId}", Title = $"EPA {OtherNationalEpaId}" },
            new Epa { Id = LocalEpaId, SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionId, Code = $"EPA-{LocalEpaId}", Title = $"EPA {LocalEpaId}" });

        db.Curricula.Add(new Curriculum { Id = CurriculumId, SubSpecialityId = SubSpecialityId, Name = "Paediatrics", Version = "11.1", EffectiveFrom = new DateOnly(2025, 1, 1) });
        db.CurriculumItems.AddRange(
            Item(NationalItemId, NationalEpaId, owningInstitutionId: null),
            Item(OtherNationalItemId, OtherNationalEpaId, owningInstitutionId: null),
            Item(LocalItemId, LocalEpaId, owningInstitutionId: InstitutionId));

        foreach (var (id, userId) in new[] { (1, "trainee-1"), (2, "trainee-2") })
        {
            db.TraineeProfiles.Add(new TraineeProfile
            {
                Id = id,
                UserId = userId,
                InstitutionId = InstitutionId,
                CurriculumId = CurriculumId,
                ProgrammeStartDate = new DateOnly(2025, 1, 1),
                ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        var activityType = new ActivityType
        {
            Id = CreditingTypeId,
            Key = "wba_under_test",
            Name = "WBA under test",
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = CreditingTypeId,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        db.ActivityTypes.Add(activityType);
        db.ActivityTypes.Add(Type(TwoEpaTypeId, "wba_two_epas_under_test", TwoEpaSchemaJson, CreditsTwoEpas));
        db.ActivityTypes.Add(Type(NonCreditingTypeId, "reflection_under_test", SchemaJson, CreditsNothing));
        await db.SaveChangesAsync();
    }

    private static ActivityType Type(int id, string key, string schemaJson, string creditRulesJson)
    {
        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = schemaJson,
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
            SchemaJson = schemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        return activityType;
    }

    private static CurriculumItem Item(int id, int epaId, int? owningInstitutionId)
        => new()
        {
            Id = id,
            CurriculumId = CurriculumId,
            EpaId = epaId,
            OwningInstitutionId = owningInstitutionId,
            RequiredCount = 3,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        };

    /// <summary>A planner that fails, as a dropped connection would while the reactivation is reading.</summary>
    private sealed class PlanningFails : ICreditApplier
    {
        public Task<CreditPlan> PlanAsync(CreditSubject subject, ActivityType activityType, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("induced failure while planning");

        public CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity)
            => throw new InvalidOperationException("Apply must not run when planning failed");

        public Task<CreditApplicationResult> ApplyAsync(Activity completedActivity, ActivityType activityType, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("induced failure while planning");
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private const string CreditsTheEpa = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

    private const string CreditsTwoEpas = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" },
            { "curriculum_item_match": { "epa_field": "second_epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

    private const string CreditsNothing = """{ "counts_for": [] }""";

    private const string TwoEpaSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "second_epa_id", "type": "epa", "label": "Second EPA", "required": true },
                { "key": "score", "type": "number", "label": "Score", "required": true }
              ]
            }
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
