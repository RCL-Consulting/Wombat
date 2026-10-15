using FluentAssertions;
using Wombat.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// T369 (flow 10, build-contracts § 0): <see cref="MsfCampaignRules.SubjectStandingAsync" />, what the campaign page, the
/// report and the release read of the registrar's standing. Current is the rule create, invite and open apply; the end
/// date is the most recent programme's end, and only once no programme is running, so a locked registrar is not current
/// and has no end (K13), and one whose programme has ended reads its day (K14, R10, F4).
/// </summary>
public sealed class MsfSubjectStandingTests
{
    private const string SubjectUserId = "trainee-1";

    public enum Standing
    {
        Current,
        Locked,
        Ended
    }

    [Theory]
    [InlineData(Standing.Current, true, null)]
    [InlineData(Standing.Locked, false, null)]
    [InlineData(Standing.Ended, false, "2026-09-30")]
    public async Task TheSubjectsStanding(Standing standing, bool isCurrent, string? endedOn)
    {
        await using var db = CreateDb();
        // An earlier programme withdrawn from, so the end read is the most recent one, not the first.
        db.Set<TraineeProfile>().Add(Ended(new DateOnly(2020, 1, 1), new DateOnly(2021, 6, 30)));
        db.Set<TraineeProfile>().Add(standing == Standing.Ended
            ? Ended(new DateOnly(2023, 1, 1), new DateOnly(2026, 9, 30))
            : Profile(new DateOnly(2023, 1, 1)));
        await db.SaveChangesAsync();

        var users = new FakeUserDirectory((SubjectUserId, "Lerato Molefe")).With(new UserIdentityDetails(
            SubjectUserId, "lerato@example.test", "Lerato", "Molefe", 1, [], [], [WombatRoles.Trainee],
            IsLockedOut: standing == Standing.Locked, IsDeactivated: standing == Standing.Locked));

        var result = await MsfCampaignRules.SubjectStandingAsync(db, users, SubjectUserId, CancellationToken.None);

        result.Should().Be(new MsfSubjectStanding(isCurrent, endedOn is null ? null : DateOnly.Parse(endedOn)));
    }

    private static TraineeProfile Profile(DateOnly start) => new()
    {
        UserId = SubjectUserId, InstitutionId = 1, CurriculumId = 1,
        ProgrammeStartDate = start, ExpectedCompletionDate = start.AddYears(4)
    };

    private static TraineeProfile Ended(DateOnly start, DateOnly endedOn)
    {
        var profile = Profile(start);
        profile.Deactivate(endedOn, new DateOnly(2026, 10, 15));
        return profile;
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
