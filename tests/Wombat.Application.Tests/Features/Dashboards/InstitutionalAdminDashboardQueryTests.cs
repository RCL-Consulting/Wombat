using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.InstitutionalAdmin;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

public sealed class InstitutionalAdminDashboardQueryTests
{
    [Fact]
    public async Task WithSeededData_ReturnsCorrectCounts()
    {
        await using var db = CreateDb();
        SeedInstitutionData(db);

        var userService = new Mock<IUserAdministrationService>();
        userService
            .Setup(s => s.ListUsersInRoleAsync("Trainee", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserIdentityDetails>
            {
                new("t1", "t1@ex.com", "A", "B", 1, [], [], ["Trainee"]),
                new("t2", "t2@ex.com", "C", "D", 1, [], [], ["Trainee"])
            });
        userService
            .Setup(s => s.ListUsersInRoleAsync("Assessor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserIdentityDetails>
            {
                new("a1", "a1@ex.com", "E", "F", 1, [], [], ["Assessor"])
            });

        // Return empty for all other roles
        foreach (var role in WombatRoles.All.Where(r => r != "Trainee" && r != "Assessor"))
        {
            userService
                .Setup(s => s.ListUsersInRoleAsync(role, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserIdentityDetails>());
        }

        var handler = new GetInstitutionalAdminDashboardSummaryQueryHandler(db, userService.Object);
        var principal = CreatePrincipal("admin-1", institutionId: 1);

        var result = await handler.Handle(
            new GetInstitutionalAdminDashboardSummaryQuery(principal), CancellationToken.None);

        result.SpecialityCount.Should().Be(1);
        result.SubSpecialityCount.Should().Be(1);
        result.UsersByRole.Should().Contain(r => r.Role == "Trainee" && r.Count == 2);
        result.UsersByRole.Should().Contain(r => r.Role == "Assessor" && r.Count == 1);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedInstitutionData(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "Test" });
        db.Specialities.Add(new Speciality { Id = 1, InstitutionId = 1, Name = "Spec" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "SubSpec" });
        db.SaveChanges();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, int? institutionId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, "InstitutionalAdmin")
        };
        if (institutionId.HasValue)
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
