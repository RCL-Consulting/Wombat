using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Queries.GetProgrammeStartForTrainee;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T192: the programme start the activity form hints at, read as every other read about a trainee is
/// (<c>TraineeScopeResolver.MayReadAsync</c>). That it is the start the encounter-date bound refuses by is shown against
/// the gate itself, in <c>EncounterDateBoundsTests.TheStartTheFormIsGiven_IsTheOneThisBoundRefusesBy</c>.
/// </summary>
public sealed class GetProgrammeStartForTraineeTests
{
    private const string TraineeId = "trainee-1";
    private const string OtherTraineeId = "trainee-2";

    private static readonly DateOnly TraineeStart = new(2025, 1, 1);

    [Fact]
    public async Task ATrainee_ReadsTheirOwnStart()
    {
        await using var db = await SeededAsync();

        (await AskAsync(db, TraineeId, Caller(TraineeId, WombatRoles.Trainee))).Should().Be(TraineeStart);
    }

    [Fact]
    public async Task ATrainee_ReadsNoOtherTraineesStart_AndTheRefusalLooksLikeNoProfile()
    {
        await using var db = await SeededAsync();

        (await AskAsync(db, TraineeId, Caller(OtherTraineeId, WombatRoles.Trainee))).Should().BeNull();
        (await AskAsync(db, "nobody", Caller(TraineeId, WombatRoles.Trainee))).Should().BeNull();
    }

    [Fact]
    public async Task ACallerWithNoProfile_ReadsNothing()
    {
        // A PendingTrainee has no profile until admission, and the gate then bounds nothing but the future.
        await using var db = await SeededAsync();

        (await AskAsync(db, "pending-1", Caller("pending-1", WombatRoles.PendingTrainee))).Should().BeNull();
    }

    [Fact]
    public async Task AGlobalAdministrator_ReadsAnyTraineesStart()
    {
        await using var db = await SeededAsync();

        (await AskAsync(db, TraineeId, Caller("admin-1", WombatRoles.Administrator))).Should().Be(TraineeStart);
    }

    private static async Task<DateOnly?> AskAsync(ApplicationDbContext db, string traineeUserId, ClaimsPrincipal caller)
        => await new GetProgrammeStartForTraineeQueryHandler(db).Handle(
            new GetProgrammeStartForTraineeQuery(traineeUserId, caller), CancellationToken.None);

    private static ClaimsPrincipal Caller(string userId, string role)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role)],
            "test"));

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<TraineeProfile>().AddRange(
            Profile(1, TraineeId, TraineeStart),
            Profile(2, OtherTraineeId, new DateOnly(2024, 1, 1)));
        await db.SaveChangesAsync();

        return db;
    }

    private static TraineeProfile Profile(int id, string userId, DateOnly start) => new()
    {
        Id = id,
        UserId = userId,
        InstitutionId = 10,
        CurriculumId = 3000,
        ProgrammeStartDate = start,
        ExpectedCompletionDate = start.AddYears(4),
        IsActive = true
    };
}
