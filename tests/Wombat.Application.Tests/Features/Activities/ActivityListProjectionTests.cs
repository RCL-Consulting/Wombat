using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Tests.Shared;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T137 and T106 item 14: an activity list row carries what tells it apart from its siblings (the EPA and the encounter
/// date) and whether it counted (the latest credit outcome), in both lists that return <see cref="ActivitySummaryDto" />.
/// </summary>
public sealed class ActivityListProjectionTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";

    private const int HistoryEpaId = 5000;
    private const int WardRoundEpaId = 5001;

    /// <summary>A stamp whose EPA has since gone: the row must still list, with no EPA.</summary>
    private const int VanishedEpaId = 7777;

    private static readonly DateTime Clock = new(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc);

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    [Fact]
    public async Task SubjectList_CarriesTheStampedEpasCodeAndTitle()
    {
        await using var db = await SeededAsync();

        var rows = await SubjectListAsync(db);

        Row(rows, 1).Should().Match<ActivitySummaryDto>(row =>
            row.EpaId == HistoryEpaId && row.EpaCode == "PAED-001" && row.EpaTitle == "Take a history");
        Row(rows, 2).Should().Match<ActivitySummaryDto>(row =>
            row.EpaId == WardRoundEpaId && row.EpaCode == "PAED-002" && row.EpaTitle == "Lead a ward round");
    }

    /// <summary>
    /// The join is LEFT: an activity about no EPA, or one whose EPA no longer exists, is still the trainee's activity.
    /// </summary>
    [Fact]
    public async Task SubjectList_ListsRowsWithNoEpa_AndWithAnEpaThatNoLongerExists()
    {
        await using var db = await SeededAsync();

        var rows = await SubjectListAsync(db);

        rows.Should().HaveCount(8);
        Row(rows, 3).EpaCode.Should().BeNull();
        Row(rows, 3).EpaTitle.Should().BeNull();
        Row(rows, 5).EpaId.Should().Be(VanishedEpaId);
        Row(rows, 5).EpaCode.Should().BeNull();
    }

    /// <summary>
    /// T231. Each row says whether its EPA is in force now, by the flag the activity's own picker labels by
    /// (<c>EpaOptionLabel</c>): false for a deactivated EPA, true for one in force, and null where there is no EPA to
    /// mark, whether the activity is about none or its EPA no longer exists.
    /// </summary>
    [Fact]
    public async Task SubjectList_SaysWhetherEachRowsEpaIsInForceNow()
    {
        await using var db = await SeededAsync();
        await DeactivateAsync(db, WardRoundEpaId);

        var rows = await SubjectListAsync(db);

        rows.Where(row => row.EpaId == WardRoundEpaId).Select(row => row.EpaInForce).Should().Equal(false, false, false);
        rows.Where(row => row.EpaId == HistoryEpaId).Select(row => row.EpaInForce).Should().Equal(true, true, true);
        Row(rows, 3).EpaInForce.Should().BeNull("the activity is about no EPA");
        Row(rows, 5).EpaInForce.Should().BeNull("its EPA no longer exists, so there is nothing to mark");
    }

    [Fact]
    public async Task SubjectList_CarriesTheEncounterDate_AndWhetherAnyoneStatedIt()
    {
        await using var db = await SeededAsync();

        var rows = await SubjectListAsync(db);

        Row(rows, 1).ObservedOn.Should().Be(new DateOnly(2026, 3, 10));
        Row(rows, 1).ObservedOnDeclared.Should().BeTrue();
        Row(rows, 3).ObservedOn.Should().Be(new DateOnly(2026, 3, 12));
        Row(rows, 3).ObservedOnDeclared.Should().BeFalse("its date is only the filing day");
    }

    /// <summary>
    /// T108's three values, each kept apart: counted (2), evaluated and counted towards nothing (0), never evaluated
    /// (null, an activity still in flight).
    /// </summary>
    [Fact]
    public async Task SubjectList_CarriesTheCreditOutcome_ThreeValued()
    {
        await using var db = await SeededAsync();

        var rows = await SubjectListAsync(db);

        Row(rows, 1).CreditedItemCount.Should().Be(2);
        Row(rows, 2).CreditedItemCount.Should().Be(0);
        Row(rows, 3).CreditedItemCount.Should().BeNull();
    }

    /// <summary>
    /// The LATEST transition that evaluated credit decides, and a later transition that evaluated nothing does not
    /// erase it: a rebuild's re-stamp of an old zero to three is what the row must show.
    /// </summary>
    [Fact]
    public async Task SubjectList_TakesTheLatestEvaluatedOutcome_IgnoringLaterMovesThatEvaluatedNothing()
    {
        await using var db = await SeededAsync();

        var rows = await SubjectListAsync(db);

        Row(rows, 4).CreditedItemCount.Should().Be(3);
    }

    /// <summary>
    /// Two moves that evaluated credit at the same instant: the later-inserted (higher id) decides, so the answer does
    /// not depend on the order the server happens to return ties in.
    /// </summary>
    [Fact]
    public async Task SubjectList_BreaksAnEvaluationTieOnTheLaterMove()
    {
        await using var db = await SeededAsync();

        var rows = await SubjectListAsync(db);

        Row(rows, 7).CreditedItemCount.Should().Be(4);
    }

    /// <summary>
    /// The list shows the encounter date and not the audit clock, so it is ordered by the encounter date: an encounter
    /// filed late sits at its own date, not at the top. Rows of one date fall back to the most recently updated, then
    /// the highest id, so the order is total.
    /// </summary>
    [Fact]
    public async Task SubjectList_IsOrderedByEncounterDate_ThenLastUpdated_ThenId()
    {
        await using var db = NewContext();
        AddProbeType(db);

        var fifteenth = Activity(11, "completed", null, new DateOnly(2026, 3, 15), declared: true);
        fifteenth.UpdatedOn = Clock.AddHours(1);
        var backDated = Activity(12, "completed", null, new DateOnly(2026, 3, 1), declared: true);
        backDated.UpdatedOn = Clock.AddHours(9);
        var tenthEarlier = Activity(13, "completed", null, new DateOnly(2026, 3, 10), declared: true);
        tenthEarlier.UpdatedOn = Clock.AddHours(2);
        var tenthLater = Activity(14, "completed", null, new DateOnly(2026, 3, 10), declared: true);
        tenthLater.UpdatedOn = Clock.AddHours(5);
        var tenthSameInstantLowerId = Activity(15, "completed", null, new DateOnly(2026, 3, 10), declared: true);
        tenthSameInstantLowerId.UpdatedOn = Clock.AddHours(2);
        var tenthSameInstantHigherId = Activity(16, "completed", null, new DateOnly(2026, 3, 10), declared: true);
        tenthSameInstantHigherId.UpdatedOn = Clock.AddHours(2);

        db.Activities.AddRange(fifteenth, backDated, tenthEarlier, tenthLater, tenthSameInstantLowerId, tenthSameInstantHigherId);
        await db.SaveChangesAsync();

        var rows = await SubjectListAsync(db);

        rows.Select(row => row.Id).Should().Equal(
            [11, 14, 16, 15, 13, 12],
            "the back-dated encounter (12), filed last, belongs at 1 March, not at the top");
    }

    [Fact]
    public async Task Inbox_CarriesTheEpaAndTheEncounterDate()
    {
        await using var db = await SeededAsync();

        var rows = await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty)
            .Handle(new ListActivitiesByActorInboxQuery(Principal(AssessorId)), CancellationToken.None);

        rows.Select(row => row.Id).Should().BeEquivalentTo([3, 6, 8], "only the requested activities naming this assessor are actionable");

        var withEpa = rows.Single(row => row.Id == 6);
        withEpa.EpaCode.Should().Be("PAED-002");
        withEpa.EpaTitle.Should().Be("Lead a ward round");
        withEpa.ObservedOn.Should().Be(new DateOnly(2026, 3, 14));
        withEpa.ObservedOnDeclared.Should().BeTrue();
        withEpa.CreditedItemCount.Should().BeNull();

        var withoutEpa = rows.Single(row => row.Id == 3);
        withoutEpa.EpaCode.Should().BeNull();
        withoutEpa.ObservedOnDeclared.Should().BeFalse();
    }

    /// <summary>T231. The inbox says it too, by the same flag.</summary>
    [Fact]
    public async Task Inbox_SaysWhetherEachRowsEpaIsInForceNow()
    {
        await using var db = await SeededAsync();
        await DeactivateAsync(db, WardRoundEpaId);

        var rows = await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty)
            .Handle(new ListActivitiesByActorInboxQuery(Principal(AssessorId)), CancellationToken.None);

        rows.Single(row => row.Id == 6).EpaInForce.Should().BeFalse("PAED-002 is deactivated");
        rows.Single(row => row.Id == 8).EpaInForce.Should().BeTrue("PAED-001 is in force");
        rows.Single(row => row.Id == 3).EpaInForce.Should().BeNull("the activity is about no EPA");
    }

    /// <summary>
    /// The inbox carries the same credit outcome as the trainee's list, by the same rule: an activity back in an
    /// actionable state after a completion that credited still says what the latest evaluation credited, ties going to
    /// the later move.
    /// </summary>
    [Fact]
    public async Task Inbox_CarriesTheLatestEvaluatedCreditOutcome()
    {
        await using var db = await SeededAsync();

        var rows = await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty)
            .Handle(new ListActivitiesByActorInboxQuery(Principal(AssessorId)), CancellationToken.None);

        rows.Single(row => row.Id == 8).CreditedItemCount.Should().Be(2);
        rows.Single(row => row.Id == 3).CreditedItemCount.Should().BeNull("its only move evaluated nothing");
    }

    /// <summary>
    /// T142. The Subject column used to print the trainee's user id. It names them now, in one lookup for the page, and
    /// only for the rows the page lists: the subject of an activity this assessor cannot act on is nobody's business here.
    /// </summary>
    [Fact]
    public async Task Inbox_NamesEachSubject_InOneLookupForTheRowsItLists()
    {
        await using var db = await SeededAsync();
        db.Activities.Single(activity => activity.Id == 6).SubjectUserId = "departed-trainee";
        db.Activities.Single(activity => activity.Id == 1).SubjectUserId = "someone-else";
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory((TraineeId, "Thandi Nkosi"), ("someone-else", "Sipho Mahlangu"));

        var rows = await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), users)
            .Handle(new ListActivitiesByActorInboxQuery(Principal(AssessorId)), CancellationToken.None);

        rows.Single(row => row.Id == 3).SubjectName.Should().Be("Thandi Nkosi");
        rows.Single(row => row.Id == 8).SubjectName.Should().Be("Thandi Nkosi");
        rows.Single(row => row.Id == 6).SubjectName.Should().Be("departed-trainee", "no user by that id exists any more");

        users.Lookups.Should().ContainSingle()
            .Which.Should().BeEquivalentTo([TraineeId, "departed-trainee"], "activity 1 is complete, so not in this inbox");
    }

    // ---- fixtures ---------------------------------------------------------------------------------------------------

    private static async Task<IReadOnlyList<ActivitySummaryDto>> SubjectListAsync(ApplicationDbContext db)
        => (await new ListActivitiesBySubjectQueryHandler(db, FakeUserDirectory.Empty)
            .Handle(new ListActivitiesBySubjectQuery(TraineeId, Principal(TraineeId)), CancellationToken.None)).Items;

    private static ActivitySummaryDto Row(IReadOnlyList<ActivitySummaryDto> rows, int id) => rows.Single(row => row.Id == id);

    private static async Task DeactivateAsync(ApplicationDbContext db, int epaId)
    {
        (await db.Epas.SingleAsync(epa => epa.Id == epaId)).Deactivate(Clock).Should().BeTrue();
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Eight activities of one type about one trainee:
    /// 1 credited two items (PAED-001); 2 credited nothing (PAED-002); 3 is in flight, about no EPA, undated;
    /// 4 was re-stamped from 0 to 3 and then moved on without evaluating; 5 carries an EPA that no longer exists;
    /// 6 is in flight for the assessor (PAED-002); 7 has two evaluations at one instant (1, then 4);
    /// 8 credited one item, was rebuilt to two at the same instant, and was sent back, so it is in the assessor's inbox
    /// again.
    /// </summary>
    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = NewContext();
        AddProbeType(db);

        db.Activities.AddRange(
            Activity(1, "completed", HistoryEpaId, new DateOnly(2026, 3, 10), declared: true,
                Move("create", Clock, null), Move("complete", Clock.AddHours(1), 2)),
            Activity(2, "completed", WardRoundEpaId, new DateOnly(2026, 3, 11), declared: true,
                Move("create", Clock, null), Move("complete", Clock.AddHours(1), 0)),
            Activity(3, "requested", null, new DateOnly(2026, 3, 12), declared: false,
                Move("create", Clock, null)),
            Activity(4, "completed", HistoryEpaId, new DateOnly(2026, 3, 13), declared: true,
                // Inserted out of time order, so the later-evaluated move has the LOWER id: time decides, not insertion.
                Move("amend", Clock.AddHours(2), 3), Move("complete", Clock.AddHours(1), 0), Move("sign_off", Clock.AddHours(3), null)),
            Activity(5, "completed", VanishedEpaId, new DateOnly(2026, 3, 13), declared: true,
                Move("complete", Clock.AddHours(1), 1)),
            Activity(6, "requested", WardRoundEpaId, new DateOnly(2026, 3, 14), declared: true,
                Move("create", Clock, null)),
            Activity(7, "completed", WardRoundEpaId, new DateOnly(2026, 3, 15), declared: true,
                // One instant, inserted in this order, so the second has the higher id and must win.
                Move("complete", Clock.AddHours(1), 1), Move("rebuild", Clock.AddHours(1), 4)),
            Activity(8, "requested", HistoryEpaId, new DateOnly(2026, 3, 16), declared: true,
                // A tie again (1, then 2 at one instant), so the inbox's tie-breaker is pinned as well as the list's.
                Move("complete", Clock.AddHours(1), 1), Move("rebuild", Clock.AddHours(1), 2), Move("send_back", Clock.AddHours(2), null)));

        await db.SaveChangesAsync();
        return db;
    }

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static void AddProbeType(ApplicationDbContext db)
    {
        db.Epas.AddRange(
            new Epa { Id = HistoryEpaId, Code = "PAED-001", Title = "Take a history" },
            new Epa { Id = WardRoundEpaId, Code = "PAED-002", Title = "Lead a ward round" });

        var type = new ActivityType
        {
            Id = 1,
            Key = "probe",
            Name = "Probe",
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = WorkflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = Clock
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 1,
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = WorkflowJson,
            CreditRulesJson = """{ "counts_for": [] }""",
            PublishedByUserId = "system",
            PublishedOn = Clock
        });
        db.ActivityTypes.Add(type);
    }

    private static Activity Activity(
        int id,
        string state,
        int? epaId,
        DateOnly observedOn,
        bool declared,
        params ActivityTransition[] transitions)
    {
        var activity = new Activity
        {
            Id = id,
            ActivityTypeId = 1,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = state,
            DataJson = $$"""{ "assessor_user_id": "{{AssessorId}}" }""",
            EpaId = epaId,
            ObservedOn = observedOn,
            ObservedOnSource = declared ? ObservationDateSource.Declared : ObservationDateSource.CreatedOn,
            CreatedOn = Clock,
            UpdatedOn = Clock.AddMinutes(id)
        };

        foreach (var transition in transitions)
        {
            activity.Transitions.Add(transition);
        }

        return activity;
    }

    private static ActivityTransition Move(string key, DateTime occurredOn, int? creditedItemCount) => new()
    {
        FromState = "requested",
        ToState = "completed",
        TransitionKey = key,
        ActorUserId = AssessorId,
        OccurredOn = occurredOn,
        SnapshotJson = "{}",
        CreditedItemCount = creditedItemCount
    };

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
