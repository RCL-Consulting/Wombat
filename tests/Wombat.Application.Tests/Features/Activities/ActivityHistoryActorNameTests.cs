using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// The activity page's History table names who made each move. It used to print their user ids. (T142)
/// </summary>
/// <remarks>
/// The names are resolved by the query that shows the history, not by <c>ActivityService.Map</c>, which create and
/// transition share. So the service is stubbed here: what is under test is the query's one lookup.
/// </remarks>
public sealed class ActivityHistoryActorNameTests
{
    private static readonly ClaimsPrincipal Caller =
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "assessor-1")], "test"));

    [Fact]
    public async Task EachMoveIsNamed_InOneLookupForEveryActor()
    {
        var detail = Detail(
            Move(1, "trainee-1"),
            Move(2, "assessor-1"),
            Move(3, "trainee-1"),
            Move(4, "departed-user"));
        var users = new FakeUserDirectory(("trainee-1", "Thandi Nkosi"), ("assessor-1", "Dr Ruth Mokoena"));

        var result = await Handler(detail, users).Handle(new GetActivityByIdQuery(7, Caller), CancellationToken.None);

        result!.Activity.Transitions.Select(transition => transition.ActorName).Should().Equal(
            "Thandi Nkosi", "Dr Ruth Mokoena", "Thandi Nkosi", "departed-user");
        users.Lookups.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(["trainee-1", "assessor-1", "departed-user"]);
    }

    [Fact]
    public async Task NothingIsLookedUp_ForAnActivityTheCallerMayNotRead()
    {
        var users = new FakeUserDirectory(("trainee-1", "Thandi Nkosi"));

        var result = await Handler(null, users).Handle(new GetActivityByIdQuery(7, Caller), CancellationToken.None);

        result.Should().BeNull();
        users.Lookups.Should().BeEmpty();
    }

    [Fact]
    public async Task TheRestOfTheDetailIsPassedThroughUnchanged()
    {
        var detail = Detail(Move(1, "trainee-1"));

        var result = await Handler(detail, new FakeUserDirectory()).Handle(new GetActivityByIdQuery(7, Caller), CancellationToken.None);

        result!.EditableFieldKeys.Should().Equal(detail.EditableFieldKeys);
        result.AvailableActions.Should().Equal(detail.AvailableActions);
        result.Activity.Should().BeEquivalentTo(detail.Activity, options => options.Excluding(activity => activity.Transitions));
        result.Activity.Transitions.Single().Should().BeEquivalentTo(
            detail.Activity.Transitions.Single(), options => options.Excluding(transition => transition.ActorName));
    }

    [Fact]
    public async Task TheSubjectIsNamed_InTheSameLookup_EvenWhenTheyMadeNoMove()
    {
        // T342, flow 03: the page's subtitle, About's Registrar and the status card name the registrar.
        var detail = Detail(Move(1, "assessor-1"));
        var users = new FakeUserDirectory(("trainee-1", "Thandi Nkosi"), ("assessor-1", "Dr Ruth Mokoena"));

        var result = await Handler(detail, users).Handle(new GetActivityByIdQuery(7, Caller), CancellationToken.None);

        result!.SubjectName.Should().Be("Thandi Nkosi");
        users.Lookups.Should().ContainSingle().Which.Should().BeEquivalentTo(["assessor-1", "trainee-1"]);
    }

    [Fact]
    public async Task TheStampedEpa_RidesWithItsTitle_AndWhetherItIsInForce()
    {
        // T342, C13: About names the EPA in full, and warns when a paused EPA holds the credit (D48).
        var detail = Detail(Move(1, "trainee-1"));
        detail = detail with { Activity = detail.Activity with { EpaId = 12 } };
        var db = Database();
        db.Set<Wombat.Domain.Epas.Epa>().Add(new Wombat.Domain.Epas.Epa
        {
            Id = 12, Code = "PAED-002", Title = "Managing common paediatric presentations", IsActive = false
        });
        await db.SaveChangesAsync();

        var result = await Handler(detail, new FakeUserDirectory(), db).Handle(new GetActivityByIdQuery(7, Caller), CancellationToken.None);

        (result!.EpaCode, result.EpaTitle, result.EpaInForce).Should().Be(("PAED-002", "Managing common paediatric presentations", false));
    }

    [Fact]
    public async Task AnActivityAboutNoEpa_CarriesNone()
    {
        var result = await Handler(Detail(Move(1, "trainee-1")), new FakeUserDirectory())
            .Handle(new GetActivityByIdQuery(7, Caller), CancellationToken.None);

        (result!.EpaCode, result.EpaTitle, result.EpaInForce).Should().Be(((string?)null, (string?)null, (bool?)null));
    }

    private static ApplicationDbContext Database()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static GetActivityByIdQueryHandler Handler(ActivityDetailDto? detail, FakeUserDirectory users, ApplicationDbContext db)
    {
        var service = new Mock<IActivityService>(MockBehavior.Strict);
        service
            .Setup(activities => activities.GetDetailAsync(7, Caller, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);
        return new GetActivityByIdQueryHandler(service.Object, users, db);
    }

    private static GetActivityByIdQueryHandler Handler(ActivityDetailDto? detail, FakeUserDirectory users)
    {
        var service = new Mock<IActivityService>(MockBehavior.Strict);
        service
            .Setup(activities => activities.GetDetailAsync(7, Caller, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // The status card's sibling read (T342, E7) goes to a database; this activity has no siblings in an empty one.
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        return new GetActivityByIdQueryHandler(service.Object, users, db);
    }

    private static ActivityTransitionDto Move(int id, string actorUserId) => new(
        id,
        "requested",
        "requested",
        "note",
        "Requested",
        "Requested",
        "Note",
        actorUserId,
        new DateTime(2026, 9, 16, 8, id, 0, DateTimeKind.Utc),
        null,
        "{}",
        null,
        null,
        null);

    private static ActivityDetailDto Detail(params ActivityTransitionDto[] transitions)
    {
        var activity = new ActivityDto(
            7,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            "mini_cex",
            1,
            "{}",
            "{}",
            "[]",
            "{}",
            "trainee-1",
            1,
            "trainee-1",
            "requested",
            "Requested",
            "{}",
            null,
            null,
            new DateOnly(2026, 9, 16),
            true,
            new DateTime(2026, 9, 16, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 16, 8, 0, 0, DateTimeKind.Utc),
            transitions);

        return new ActivityDetailDto(activity, ["overall_level"], [new ActivityActionDto("complete", false)]);
    }
}
