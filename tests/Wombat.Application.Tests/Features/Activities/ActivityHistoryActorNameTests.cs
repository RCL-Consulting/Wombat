using System.Security.Claims;
using FluentAssertions;
using Moq;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
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

    private static GetActivityByIdQueryHandler Handler(ActivityDetailDto? detail, FakeUserDirectory users)
    {
        var service = new Mock<IActivityService>(MockBehavior.Strict);
        service
            .Setup(activities => activities.GetDetailAsync(7, Caller, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        return new GetActivityByIdQueryHandler(service.Object, users);
    }

    private static ActivityTransitionDto Move(int id, string actorUserId) => new(
        id,
        "requested",
        "requested",
        "note",
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
