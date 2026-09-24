using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
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
                "trainee-1", 20, 2026, 1,
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
                "trainee-1", 20, 2026, 1,
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
        // The trainee's programme is in the speciality the panel covers: a Speciality-scoped panel reviews only its own
        // speciality's trainees (T131, T194 item 2).
        dbContext.SubSpecialities.Add(new SubSpeciality { Id = 9, SpecialityId = 5, Name = "Acute Care", IsActive = true });
        dbContext.Curricula.Add(new Curriculum { Id = 1, SubSpecialityId = 9, Name = "Acute Care", Version = "1" });
        dbContext.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "ARCP panel",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = 1,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }
            ]
        });
        // The trainee trains at the panel's institution: a panel acts only on its own institution's trainees. (T182)
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = 1, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1)
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
        claims.Add(new Claim(WombatClaimTypes.InstitutionId, "1"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
