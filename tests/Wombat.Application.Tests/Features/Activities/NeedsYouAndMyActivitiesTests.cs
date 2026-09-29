using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Activities.Queries.ListNeedsYou;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T342 (B6, B7, E7, E9): Needs you, My activities' rows and the activity page's status card, over Sipho Ndlovu's work as
/// the runbook leaves it in Act 3. His reflection, returned by Dr Botha (Step 3.16), and his unsent Mini-CEX draft need
/// him; his request to Dr Zulu, which he may still cancel (Step 3.12), and his closed and finished work do not. The
/// assessors' inboxes are what they were (Steps 3.24, 3.26).
/// </summary>
public sealed class NeedsYouAndMyActivitiesTests
{
    private const string Ndlovu = "sipho";
    private const string Botha = "sarah";
    private const string Naidoo = "david";
    private const string Zulu = "thandi";
    private const string Khumalo = "fatima";
    private const string Mahlangu = "nomsa";

    private const int MiniCex = 1;
    private const int Reflective = 2;

    private const int ReturnedReflection = 1;
    private const int DraftMiniCex = 2;
    private const int RequestToZulu = 3;
    private const int Declined = 4;
    private const int Completed = 5;
    private const int MahlangusRequest = 6;

    private static readonly DateTime Clock = new(2026, 9, 29, 7, 0, 0, DateTimeKind.Utc);

    private static readonly FakeUserDirectory People = new(
        (Ndlovu, "Sipho Ndlovu"), (Botha, "Sarah Botha"), (Naidoo, "David Naidoo"), (Zulu, "Thandi Zulu"),
        (Khumalo, "Fatima Khumalo"), (Mahlangu, "Nomsa Mahlangu"));

    [Fact]
    public async Task NeedsYou_IsHisReturnedReflectionAndHisDraft_NotTheWorkWithOthers()
    {
        await using var db = await SeededAsync();

        var rows = await NeedsYouAsync(db, Ndlovu);

        rows.Select(row => row.Id).Should().Equal(
            [ReturnedReflection, DraftMiniCex],
            "most recently updated first; the request he may still cancel is with Dr Zulu (Step 3.12), and closed and finished work waits on no one");
    }

    [Fact]
    public async Task AReturnedRow_SaysWhoReturnedItWhenAndWithWhatNote()
    {
        await using var db = await SeededAsync();

        var reflection = (await NeedsYouAsync(db, Ndlovu)).Single(row => row.Id == ReturnedReflection);

        reflection.IsReturned.Should().BeTrue();
        reflection.Returned.Should().Be(new ActivityReturnDto(Botha, "Sarah Botha", Clock.AddHours(-1), "Say what you would change."));
        reflection.Holder.Should().Be(new ActivityHolderDto(ActivityHolderKind.Author, Ndlovu, "Sipho Ndlovu", true, Clock.AddHours(-1)));
        reflection.NomineeName.Should().Be("Sarah Botha", "the link's second line: with Sarah Botha");
        reflection.DisplayName.Should().Be("Reflective Exercise (Paediatrics) · PAED-001 · 2026-09-09");
        reflection.CurrentStateLabel.Should().Be("Draft");
    }

    [Fact]
    public async Task AnUnsentDraft_IsNotReturned_AndNamesTheAssessorItIsFor()
    {
        await using var db = await SeededAsync();

        var draft = (await NeedsYouAsync(db, Ndlovu)).Single(row => row.Id == DraftMiniCex);

        draft.IsReturned.Should().BeFalse();
        draft.NomineeName.Should().Be("David Naidoo", "to David Naidoo");
        draft.Holder!.Kind.Should().Be(ActivityHolderKind.Author);
        draft.DisplayName.Should().Be("Mini-CEX (Paediatrics) · PAED-003 · 2026-09-25");
    }

    /// <summary>
    /// T342 (lane D): each row carries its type's shape, from its pinned form, so the page words the nominee "with Sarah
    /// Botha" for the reflection and "to David Naidoo" for the Mini-CEX, in both lists.
    /// </summary>
    [Fact]
    public async Task EachRow_CarriesItsTypesShape_ForTheLinksToOrWith()
    {
        await using var db = await SeededAsync();

        var needsYou = await NeedsYouAsync(db, Ndlovu);
        var page = await MineAsync(db);

        foreach (var rows in new[] { needsYou, page.Items })
        {
            rows.Single(row => row.Id == ReturnedReflection).Shape.Should().Be(ActivityTypeShape.DiscussedOrReviewed);
            rows.Single(row => row.Id == DraftMiniCex).Shape.Should().Be(ActivityTypeShape.Rated);
        }
    }

    /// <summary>Q6: a registrar with no other role finds the Activity inbox empty; his drafts and returns are Needs you.</summary>
    [Fact]
    public async Task ARegistrarsInbox_IsEmpty()
    {
        await using var db = await SeededAsync();

        (await InboxAsync(db, Principal(Ndlovu, WombatRoles.Trainee))).Should().BeEmpty();
    }

    /// <summary>Steps 3.24, 3.26: the assessors' inboxes hold the requests naming them, as before T342.</summary>
    [Fact]
    public async Task TheAssessorsInboxes_HoldTheRequestsNamingThem()
    {
        await using var db = await SeededAsync();

        (await InboxAsync(db, Principal(Zulu, WombatRoles.Assessor))).Select(row => row.Id).Should().Equal(RequestToZulu);
        (await InboxAsync(db, Principal(Naidoo, WombatRoles.Assessor))).Select(row => row.Id).Should().Equal(
            [MahlangusRequest], "Ndlovu's unsent draft naming him is in nobody's inbox until it is submitted");
        (await NeedsYouAsync(db, Naidoo)).Should().BeEmpty("none of it is his own");
    }

    [Fact]
    public async Task MyActivities_SaysWhoHasEachRowNow()
    {
        await using var db = await SeededAsync();

        var page = await MineAsync(db);

        var holders = page.Items.ToDictionary(row => row.Id, row => (row.Holder!.Kind, row.Holder.Name));
        holders[ReturnedReflection].Should().Be((ActivityHolderKind.Author, "Sipho Ndlovu"));
        holders[DraftMiniCex].Should().Be((ActivityHolderKind.Author, "Sipho Ndlovu"));
        holders[RequestToZulu].Should().Be((ActivityHolderKind.Person, "Thandi Zulu"));
        holders[Declined].Should().Be((ActivityHolderKind.Closed, (string?)null));
        holders[Completed].Should().Be((ActivityHolderKind.Done, (string?)null));
        page.Items.Single(row => row.Id == ReturnedReflection).IsReturned.Should().BeTrue();
        page.Items.Where(row => row.Id != ReturnedReflection).Should().OnlyContain(row => !row.IsReturned);
    }

    /// <summary>
    /// E7: the declined request and its re-filing share type, EPA and date, so each name ends with its nominee; no other
    /// row's does.
    /// </summary>
    [Fact]
    public async Task TwoActivitiesAlike_AreToldApartByTheirNominee()
    {
        await using var db = await SeededAsync();

        var names = (await MineAsync(db)).Items.ToDictionary(row => row.Id, row => (row.DisplayName, row.DisplayNameHasNominee));

        names[Declined].Should().Be(("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09 · Fatima Khumalo", true));
        names[Completed].Should().Be(("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09 · Sarah Botha", true));
        names[RequestToZulu].Should().Be(("Mini-CEX (Paediatrics) · PAED-004 · 2026-09-04", false));
    }

    /// <summary>E9: a draft saved with no EPA and no date says so.</summary>
    [Fact]
    public async Task ADraftWithNoEpaAndNoDate_IsNamedSo()
    {
        await using var db = await SeededAsync();
        db.Activities.Add(Filed(7, MiniCex, "draft", "{}", epaId: null, observedOn: null, updated: Clock.AddDays(-9)));
        db.Set<ActivityTransition>().Add(Move(7, "draft", "draft", "create", Ndlovu, Clock.AddDays(-9)));
        await db.SaveChangesAsync();

        var draft = (await MineAsync(db)).Items.Single(row => row.Id == 7);

        draft.DisplayName.Should().Be("Mini-CEX (Paediatrics) · no EPA yet · no date yet");
        draft.NomineeName.Should().BeNull("no assessor is named yet");
    }

    [Fact]
    public async Task MyActivities_ComesAPageAtATime_WithTheTotal()
    {
        await using var db = await SeededAsync();

        var first = await MineAsync(db, page: 1, pageSize: 2);
        var last = await MineAsync(db, page: 99, pageSize: 2);
        var everything = await MineAsync(db);

        everything.Should().Match<ActivityListPageDto>(page => page.Page == 1 && page.PageSize == 20 && page.TotalCount == 5);
        everything.Items.Select(row => row.Id).Should().Equal(
            [DraftMiniCex, ReturnedReflection, Declined, Completed, RequestToZulu], "newest encounter first, then the most recently updated");
        first.Items.Select(row => row.Id).Should().Equal(everything.Items.Take(2).Select(row => row.Id));
        (first.TotalCount, first.PageCount).Should().Be((5, 3));
        last.Page.Should().Be(3, "a page past the end is the last page");
        last.Items.Select(row => row.Id).Should().Equal(RequestToZulu);
    }

    [Fact]
    public async Task TheActivitysPage_CarriesItsStatusCard()
    {
        await using var db = await SeededAsync();

        var reflection = await DetailAsync(db, ReturnedReflection, Principal(Ndlovu, WombatRoles.Trainee));
        var completed = await DetailAsync(db, Completed, Principal(Ndlovu, WombatRoles.Trainee));
        var request = await DetailAsync(db, RequestToZulu, Principal(Zulu, WombatRoles.Assessor));

        reflection!.Returned.Should().Be(new ActivityReturnDto(Botha, "Sarah Botha", Clock.AddHours(-1), "Say what you would change."));
        reflection.Holder!.Should().Match<ActivityHolderDto>(holder => holder.Kind == ActivityHolderKind.Author && holder.IsViewer);
        reflection.NomineeName.Should().Be("Sarah Botha");
        reflection.DisplayName.Should().Be("Reflective Exercise (Paediatrics) · PAED-001 · 2026-09-09");

        completed!.Holder!.Kind.Should().Be(ActivityHolderKind.Done);
        completed.Returned.Should().BeNull();
        completed.DisplayName.Should().Be("Mini-CEX (Paediatrics) · PAED-002 · 2026-09-09 · Sarah Botha", "E7, as on My activities");

        request!.Holder.Should().Be(new ActivityHolderDto(ActivityHolderKind.Person, Zulu, "Thandi Zulu", true, Clock.AddDays(-2)));
        request.DisplayName.Should().Be("Mini-CEX (Paediatrics) · PAED-004 · 2026-09-04");
    }

    /// <summary>T101: the page still answers null for an activity the caller may not read, and looks nothing up for it.</summary>
    [Fact]
    public async Task TheActivitysPage_IsStillNullForAStranger()
    {
        await using var db = await SeededAsync();
        var users = new FakeUserDirectory();

        var detail = await new GetActivityByIdQueryHandler(Service(db), users, db)
            .Handle(new GetActivityByIdQuery(ReturnedReflection, Principal(Mahlangu, WombatRoles.Trainee)), CancellationToken.None);

        detail.Should().BeNull();
        users.Lookups.Should().BeEmpty();
    }

    // ---- fixtures ---------------------------------------------------------------------------------------------------

    private static async Task<IReadOnlyList<ActivitySummaryDto>> NeedsYouAsync(ApplicationDbContext db, string userId)
        => await new ListNeedsYouQueryHandler(db, new WorkflowEvaluator(), People)
            .Handle(new ListNeedsYouQuery(Principal(userId, WombatRoles.Trainee)), CancellationToken.None);

    private static async Task<IReadOnlyList<ActivitySummaryDto>> InboxAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), People)
            .Handle(new ListActivitiesByActorInboxQuery(principal), CancellationToken.None);

    private static async Task<ActivityListPageDto> MineAsync(ApplicationDbContext db, int page = 1, int pageSize = 20)
        => await new ListActivitiesBySubjectQueryHandler(db, People)
            .Handle(new ListActivitiesBySubjectQuery(Ndlovu, Principal(Ndlovu, WombatRoles.Trainee), page, pageSize), CancellationToken.None);

    private static async Task<ActivityDetailDto?> DetailAsync(ApplicationDbContext db, int id, ClaimsPrincipal principal)
        => await new GetActivityByIdQueryHandler(Service(db), People, db)
            .Handle(new GetActivityByIdQuery(id, principal), CancellationToken.None);

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        AddSeededType(db, MiniCex, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        AddSeededType(db, Reflective, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");
        db.Epas.AddRange(
            new Epa { Id = 1, Code = "PAED-001", Title = "One" },
            new Epa { Id = 2, Code = "PAED-002", Title = "Managing common paediatric presentations" },
            new Epa { Id = 3, Code = "PAED-003", Title = "Three" },
            new Epa { Id = 4, Code = "PAED-004", Title = "Four" });

        db.Activities.AddRange(
            Filed(ReturnedReflection, Reflective, "draft", Naming(Botha), 1, new DateOnly(2026, 9, 9), Clock.AddHours(-1)),
            Filed(DraftMiniCex, MiniCex, "draft", Naming(Naidoo), 3, new DateOnly(2026, 9, 25), Clock.AddHours(-2)),
            Filed(RequestToZulu, MiniCex, "requested", Naming(Zulu), 4, new DateOnly(2026, 9, 4), Clock.AddDays(-2)),
            Filed(Declined, MiniCex, "declined", Naming(Khumalo), 2, new DateOnly(2026, 9, 9), Clock.AddDays(-3)),
            Filed(Completed, MiniCex, "completed", Naming(Botha), 2, new DateOnly(2026, 9, 9), Clock.AddDays(-4)),
            Filed(MahlangusRequest, MiniCex, "requested", Naming(Naidoo), 1, new DateOnly(2026, 9, 20), Clock.AddDays(-1), subject: Mahlangu));

        db.Set<ActivityTransition>().AddRange(
            Move(ReturnedReflection, "draft", "draft", "create", Ndlovu, Clock.AddDays(-5)),
            Move(ReturnedReflection, "draft", "submitted", "submit", Ndlovu, Clock.AddDays(-5).AddMinutes(1)),
            Move(ReturnedReflection, "submitted", "draft", "return", Botha, Clock.AddHours(-1), "Say what you would change."),
            Move(DraftMiniCex, "draft", "draft", "create", Ndlovu, Clock.AddHours(-2)),
            Move(RequestToZulu, "draft", "draft", "create", Ndlovu, Clock.AddDays(-2).AddMinutes(-1)),
            Move(RequestToZulu, "draft", "requested", "submit", Ndlovu, Clock.AddDays(-2)),
            Move(Declined, "draft", "requested", "submit", Ndlovu, Clock.AddDays(-6)),
            Move(Declined, "requested", "declined", "decline", Khumalo, Clock.AddDays(-3), "I was not there."),
            Move(Completed, "draft", "requested", "submit", Ndlovu, Clock.AddDays(-5)),
            Move(Completed, "requested", "completed", "complete", Botha, Clock.AddDays(-4)),
            Move(MahlangusRequest, "draft", "requested", "submit", Mahlangu, Clock.AddDays(-1)));

        await db.SaveChangesAsync();
        return db;
    }

    /// <summary>A shipped type with its seed's form and workflow on version 1, as a publish leaves it.</summary>
    private static void AddSeededType(ApplicationDbContext db, int id, string key, string name)
    {
        var type = ShippedSeeds.AddType(db, id, key, name);
        var schemaJson = File.ReadAllText(Path.Combine(ShippedSeeds.Folder, key, "schema.json"));
        type.SchemaJson = schemaJson;
        type.Versions.Single().SchemaJson = schemaJson;
    }

    private static string Naming(string userId) => $$"""{ "assessor_user_id": "{{userId}}" }""";

    private static Activity Filed(
        int id, int typeId, string state, string dataJson, int? epaId, DateOnly? observedOn, DateTime updated, string subject = Ndlovu) => new()
    {
        Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
        SubjectUserId = subject, CreatedByUserId = subject, CurrentState = state, DataJson = dataJson,
        EpaId = epaId,
        ObservedOn = observedOn ?? DateOnly.FromDateTime(updated),
        ObservedOnSource = observedOn is null ? ObservationDateSource.CreatedOn : ObservationDateSource.Declared,
        InstitutionId = 10, SpecialityId = 5, SubSpecialityId = 6,
        CreatedOn = updated.AddDays(-1), UpdatedOn = updated
    };

    private static int _moveId;

    private static ActivityTransition Move(
        int activityId, string from, string to, string key, string actor, DateTime on, string? note = null) => new()
    {
        Id = Interlocked.Increment(ref _moveId),
        ActivityId = activityId, FromState = from, ToState = to, TransitionKey = key, ActorUserId = actor, OccurredOn = on, Note = note
    };

    private static ClaimsPrincipal Principal(string userId, string role)
        => TestPrincipals.InRoles([role], userId, 10);
}
