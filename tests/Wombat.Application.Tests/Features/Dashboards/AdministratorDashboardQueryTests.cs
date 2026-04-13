using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

public sealed class AdministratorDashboardQueryTests
{
    [Fact]
    public async Task EmptyDatabase_ReturnsHealthyWithZeroUsers()
    {
        await using var db = CreateDb();
        db.Institutions.Add(new Institution { Id = 1, Name = "Test" });
        await db.SaveChangesAsync();

        var userService = new Mock<IUserAdministrationService>();
        foreach (var role in WombatRoles.All)
        {
            userService
                .Setup(s => s.ListUsersInRoleAsync(role, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserIdentityDetails>());
        }

        var handler = new GetAdministratorDashboardSummaryQueryHandler(db, userService.Object);
        var principal = CreatePrincipal("admin-1");

        var result = await handler.Handle(
            new GetAdministratorDashboardSummaryQuery(principal), CancellationToken.None);

        result.DatabaseHealthy.Should().BeTrue();
        result.TotalUserCount.Should().Be(0);
    }

    [Fact]
    public async Task WithUsers_CountsDistinctAcrossRoles()
    {
        await using var db = CreateDb();
        db.Institutions.Add(new Institution { Id = 1, Name = "Test" });
        await db.SaveChangesAsync();

        var userService = new Mock<IUserAdministrationService>();
        var sharedUser = new UserIdentityDetails("u1", "u1@ex.com", "A", "B", 1, [], [], ["Trainee", "Assessor"]);

        userService
            .Setup(s => s.ListUsersInRoleAsync("Trainee", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserIdentityDetails> { sharedUser });
        userService
            .Setup(s => s.ListUsersInRoleAsync("Assessor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserIdentityDetails> { sharedUser });

        foreach (var role in WombatRoles.All.Where(r => r != "Trainee" && r != "Assessor"))
        {
            userService
                .Setup(s => s.ListUsersInRoleAsync(role, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserIdentityDetails>());
        }

        var handler = new GetAdministratorDashboardSummaryQueryHandler(db, userService.Object);
        var principal = CreatePrincipal("admin-1");

        var result = await handler.Handle(
            new GetAdministratorDashboardSummaryQuery(principal), CancellationToken.None);

        result.TotalUserCount.Should().Be(1, "same user in two roles should count once");
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal(string userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, "Administrator")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
