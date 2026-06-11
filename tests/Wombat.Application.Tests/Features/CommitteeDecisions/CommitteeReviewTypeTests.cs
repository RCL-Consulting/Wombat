using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

public sealed class CommitteeReviewTypeTests
{
    [Fact]
    public async Task Schedule_WithPreGraduationType_PersistsAndReturnsType()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var handler = new ScheduleCommitteeReviewCommandHandler(dbContext);
        var dto = await handler.Handle(
            new ScheduleCommitteeReviewCommand(
                "trainee-1", 20,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), new DateOnly(2026, 4, 1),
                CreatePrincipal("coord-1", [WombatRoles.Coordinator]),
                ReviewType: CommitteeReviewType.PreGraduation),
            CancellationToken.None);

        dto.ReviewType.Should().Be(CommitteeReviewType.PreGraduation);
        var stored = await dbContext.Set<CommitteeReview>().SingleAsync(review => review.Id == dto.Id);
        stored.ReviewType.Should().Be(CommitteeReviewType.PreGraduation);
    }

    [Fact]
    public async Task Schedule_WithoutType_DefaultsToAnnualProgression()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);

        var handler = new ScheduleCommitteeReviewCommandHandler(dbContext);
        var dto = await handler.Handle(
            new ScheduleCommitteeReviewCommand(
                "trainee-1", 20,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), new DateOnly(2026, 4, 1),
                CreatePrincipal("coord-1", [WombatRoles.Coordinator])),
            CancellationToken.None);

        dto.ReviewType.Should().Be(CommitteeReviewType.AnnualProgression);
        var stored = await dbContext.Set<CommitteeReview>().SingleAsync(review => review.Id == dto.Id);
        stored.ReviewType.Should().Be(CommitteeReviewType.AnnualProgression);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        dbContext.Institutions.Add(new Institution { Id = 1, Name = "Test Hospital", IsActive = true, CreatedOn = DateTime.UtcNow });
        dbContext.Specialities.Add(new Speciality { Id = 5, CollegeId = 1, Name = "General Medicine", IsActive = true });
        dbContext.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "ARCP panel",
            Scope = DecisionPanelScope.Speciality,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }
            ]
        });
        await dbContext.SaveChangesAsync();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(WombatClaimTypes.SpecialityId, "5"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
