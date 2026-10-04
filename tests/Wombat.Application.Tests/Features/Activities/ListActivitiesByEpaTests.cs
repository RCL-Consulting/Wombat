using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.TestHelpers.AssessorReads;
using Counts = Wombat.Application.Tests.TestHelpers.TraineeCounts;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// My activities' read with an EPA filter (T355, C10): the EPA page's "Activities on this EPA". Only that EPA's finished
/// activities, by each pinned workflow, so a Declined or cancelled request is never listed and a recorded MSF row is;
/// ordered, paged and named as without the filter, each name by E7 over the subject's whole list.
/// </summary>
public sealed class ListActivitiesByEpaTests
{
    private const int Paed001 = 5000;
    private const int Paed002 = 5001;
    private const int MsfTypeId = 31;

    [Fact]
    public async Task OnlyThatEpasFinishedActivities_AreListed_MsfIncluded_NewestEncounterFirst()
    {
        await using var db = Seeded();
        Add(db, 1, CpsaMiniCexTypeId, "completed", Paed001, new DateOnly(2026, 9, 21));
        Add(db, 2, CpsaMiniCexTypeId, "declined", Paed001, new DateOnly(2026, 9, 22));
        Add(db, 3, CpsaMiniCexTypeId, "cancelled", Paed001, new DateOnly(2026, 9, 23));
        Add(db, 4, CpsaMiniCexTypeId, "requested", Paed001, new DateOnly(2026, 9, 24));
        Add(db, 5, MsfTypeId, "recorded", Paed001, new DateOnly(2026, 10, 3));
        Add(db, 6, CpsaReflectiveTypeId, "discussed", Paed001, new DateOnly(2026, 9, 25));
        Add(db, 7, CpsaMiniCexTypeId, "completed", Paed002, new DateOnly(2026, 9, 26));
        Add(db, 8, CpsaMiniCexTypeId, "completed", Paed001, new DateOnly(2026, 9, 20), subject: "trainee-2");
        await db.SaveChangesAsync();

        var page = await ListAsync(db, epaId: Paed001);

        page.Items.Select(item => item.Id).Should().Equal([5, 6, 1], "finished, on PAED-001, hers, newest encounter first");
        page.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task TheFilteredList_IsPaged_AndNamedOverTheWholeList()
    {
        // E7: the completed Mini-CEX shares its type, EPA and date with a request still in hand, which the filter leaves
        // out; its name still carries its assessor, as My activities names it.
        await using var db = Seeded();
        var day = new DateOnly(2026, 9, 13);
        Add(db, 1, CpsaMiniCexTypeId, "completed", Paed001, day);
        Add(db, 2, CpsaMiniCexTypeId, "requested", Paed001, day, dataJson: """{ "assessor_user_id": "assessor-2" }""");
        for (var id = 10; id < 14; id++)
        {
            Add(db, id, CpsaMiniCexTypeId, "completed", Paed001, new DateOnly(2026, 8, id));
        }

        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory((AssessorId, "Sarah Botha"), ("assessor-2", "Fatima Khumalo"));

        var first = await ListAsync(db, epaId: Paed001, pageSize: 2, directory: directory);
        first.Items.Select(item => item.Id).Should().Equal(1, 13);
        (first.TotalCount, first.PageCount).Should().Be((5, 3));
        first.Items[0].DisplayNameHasNominee.Should().BeTrue();
        first.Items[0].DisplayName.Should().EndWith(" · Sarah Botha");

        var last = await ListAsync(db, epaId: Paed001, page: 9, pageSize: 2, directory: directory);
        (last.Page, last.Items.Count).Should().Be((3, 1));
    }

    [Fact]
    public async Task WithoutTheFilter_TheListIsAsToday()
    {
        await using var db = Seeded();
        Add(db, 1, CpsaMiniCexTypeId, "completed", Paed001, new DateOnly(2026, 9, 21));
        Add(db, 2, CpsaMiniCexTypeId, "declined", Paed001, new DateOnly(2026, 9, 22));
        Add(db, 3, CpsaMiniCexTypeId, "requested", Paed002, new DateOnly(2026, 9, 23));
        Add(db, 4, CpsaMiniCexTypeId, "draft", null, new DateOnly(2026, 9, 24));
        await db.SaveChangesAsync();

        var page = await ListAsync(db, epaId: null);

        page.Items.Select(item => item.Id).Should().Equal(4, 3, 2, 1);
        page.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task EachRow_SaysWhetherItsEpasPageOpensForHer()
    {
        // T355, build review G4: an item of her preferred profile's curriculum, national or her institution's, in force or
        // paused, has a page; another institution's local EPA, or one on no item (a version move, a removed item), has none.
        await using var db = Seeded();
        Counts.SeedCurriculum(db);
        Counts.SeedTrainee(db, TraineeId, new DateOnly(2023, 1, 14), profileId: 900);
        Add(db, 1, CpsaMiniCexTypeId, "completed", Counts.Paed001, new DateOnly(2026, 9, 21));
        Add(db, 2, CpsaMiniCexTypeId, "completed", Counts.Paed012, new DateOnly(2026, 9, 20));
        Add(db, 3, CpsaMiniCexTypeId, "completed", Counts.Kgk001, new DateOnly(2026, 9, 19));
        Add(db, 4, CpsaMiniCexTypeId, "completed", Counts.Oth001, new DateOnly(2026, 9, 18));
        Add(db, 5, CpsaMiniCexTypeId, "completed", Counts.OffCurriculum, new DateOnly(2026, 9, 17));
        Add(db, 6, CpsaMiniCexTypeId, "draft", null, new DateOnly(2026, 9, 16));
        await db.SaveChangesAsync();

        var page = await ListAsync(db, epaId: null);

        page.Items.Select(item => (item.Id, item.EpaPageOpens)).Should().Equal(
            (1, true), (2, true), (3, true), (4, false), (5, false), (6, false));
    }

    private static ApplicationDbContext Seeded()
    {
        var db = CreateDb();
        SeedCpsaTypes(db);
        ShippedSeeds.AddType(db, MsfTypeId, "msf_cpsa", "Multi-Source Feedback (Paediatrics)").SystemManaged = true;
        db.SaveChanges();
        return db;
    }

    private static void Add(
        ApplicationDbContext db, int id, int typeId, string state, int? epaId, DateOnly observedOn,
        string subject = TraineeId, string? dataJson = null)
    {
        var at = observedOn.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1, SubjectUserId = subject, CreatedByUserId = subject,
            CurrentState = state, DataJson = dataJson ?? NamesTheAssessor, EpaId = epaId, ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared, CreatedOn = at, UpdatedOn = at
        });
    }

    private static Task<ActivityListPageDto> ListAsync(
        ApplicationDbContext db, int? epaId, int page = 1, int pageSize = 20, FakeUserDirectory? directory = null)
        => new ListActivitiesBySubjectQueryHandler(db, directory ?? FakeUserDirectory.Empty)
            .Handle(
                new ListActivitiesBySubjectQuery(TraineeId, CreatePrincipal(TraineeId, "Trainee"), page, pageSize) { EpaId = epaId },
                CancellationToken.None);
}
