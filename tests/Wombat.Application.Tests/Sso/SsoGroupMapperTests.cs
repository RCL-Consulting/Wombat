using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Sso;

public sealed class SsoGroupMapperTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Mock<UserManager<WombatIdentityUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<WombatIdentityUser>>();
        var userManager = new Mock<UserManager<WombatIdentityUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Track roles added/removed so GetRolesAsync reflects the final state
        var userRoles = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        userManager.Setup(m => m.GetRolesAsync(It.IsAny<WombatIdentityUser>()))
            .ReturnsAsync((WombatIdentityUser u) =>
                userRoles.TryGetValue(u.Id, out var roles) ? roles.ToList() : new List<string>());

        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<WombatIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync((WombatIdentityUser u, string role) =>
            {
                if (!userRoles.ContainsKey(u.Id))
                    userRoles[u.Id] = new HashSet<string>(StringComparer.Ordinal);
                userRoles[u.Id].Add(role);
                return IdentityResult.Success;
            });

        userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<WombatIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync((WombatIdentityUser u, string role) =>
            {
                if (userRoles.TryGetValue(u.Id, out var roles))
                    roles.Remove(role);
                return IdentityResult.Success;
            });

        return userManager;
    }

    [Fact]
    public async Task Apply_MatchingGroup_AssignsRole()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(new SsoGroupRoleMapping
        {
            ProviderKey = "uct",
            ExternalGroupId = "group-1",
            ExternalGroupDisplayName = "Surgery",
            WombatRole = WombatRoles.Trainee,
            InstitutionId = 1
        });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-1", InstitutionId = 1 };

        var roles = await mapper.ApplyAsync(user, "uct", ["group-1"]);

        roles.Should().Contain(WombatRoles.Trainee);
        userManager.Verify(m => m.AddToRoleAsync(user, WombatRoles.Trainee), Times.Once);
        db.UserRoleAssignments.Should().ContainSingle(a =>
            a.UserId == "user-1" && a.Role == WombatRoles.Trainee && a.Source == RoleAssignmentSource.Sso);
    }

    [Fact]
    public async Task Apply_OverlappingGroups_AssignsMultipleRoles()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.AddRange(
            new SsoGroupRoleMapping
            {
                ProviderKey = "uct",
                ExternalGroupId = "group-1",
                ExternalGroupDisplayName = "Surgery",
                WombatRole = WombatRoles.Trainee,
                InstitutionId = 1
            },
            new SsoGroupRoleMapping
            {
                ProviderKey = "uct",
                ExternalGroupId = "group-2",
                ExternalGroupDisplayName = "Assessors",
                WombatRole = WombatRoles.Assessor,
                InstitutionId = 1
            });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-2", InstitutionId = 1 };

        var roles = await mapper.ApplyAsync(user, "uct", ["group-1", "group-2"]);

        userManager.Verify(m => m.AddToRoleAsync(user, WombatRoles.Trainee), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(user, WombatRoles.Assessor), Times.Once);
    }

    [Fact]
    public async Task Apply_NoMatchingGroups_AssignsNoRoles()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(new SsoGroupRoleMapping
        {
            ProviderKey = "uct",
            ExternalGroupId = "group-1",
            ExternalGroupDisplayName = "Surgery",
            WombatRole = WombatRoles.Trainee,
            InstitutionId = 1
        });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-3", InstitutionId = 1 };

        var roles = await mapper.ApplyAsync(user, "uct", ["unrelated-group"]);

        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<WombatIdentityUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Apply_AdministratorMapping_IsSkipped()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(new SsoGroupRoleMapping
        {
            ProviderKey = "uct",
            ExternalGroupId = "admin-group",
            ExternalGroupDisplayName = "IT Admins",
            WombatRole = WombatRoles.Administrator,
            InstitutionId = 1
        });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-4", InstitutionId = 1 };

        var roles = await mapper.ApplyAsync(user, "uct", ["admin-group"]);

        userManager.Verify(m => m.AddToRoleAsync(It.IsAny<WombatIdentityUser>(), WombatRoles.Administrator), Times.Never);
    }

    [Fact]
    public async Task Apply_RemovedFromGroup_RemovesSsoAssignedRole()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(new SsoGroupRoleMapping
        {
            ProviderKey = "uct",
            ExternalGroupId = "group-1",
            ExternalGroupDisplayName = "Surgery",
            WombatRole = WombatRoles.Trainee,
            InstitutionId = 1
        });
        // User previously had the role via SSO
        db.UserRoleAssignments.Add(new UserRoleAssignment
        {
            UserId = "user-5",
            Role = WombatRoles.Trainee,
            Source = RoleAssignmentSource.Sso,
            ProviderKey = "uct",
            AssignedOn = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-5", InstitutionId = 1 };

        // User is no longer in group-1
        await mapper.ApplyAsync(user, "uct", []);

        userManager.Verify(m => m.RemoveFromRoleAsync(user, WombatRoles.Trainee), Times.Once);
        db.UserRoleAssignments.Should().NotContain(a =>
            a.UserId == "user-5" && a.Role == WombatRoles.Trainee && a.Source == RoleAssignmentSource.Sso);
    }

    [Fact]
    public async Task Apply_ManuallyAssignedRole_PreservedWhenGroupRemoved()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(new SsoGroupRoleMapping
        {
            ProviderKey = "uct",
            ExternalGroupId = "group-1",
            ExternalGroupDisplayName = "Surgery",
            WombatRole = WombatRoles.Coordinator,
            InstitutionId = 1
        });
        // User has the role from both SSO and manual assignment
        db.UserRoleAssignments.AddRange(
            new UserRoleAssignment
            {
                UserId = "user-6",
                Role = WombatRoles.Coordinator,
                Source = RoleAssignmentSource.Sso,
                ProviderKey = "uct",
                AssignedOn = DateTime.UtcNow.AddDays(-1)
            },
            new UserRoleAssignment
            {
                UserId = "user-6",
                Role = WombatRoles.Coordinator,
                Source = RoleAssignmentSource.Manual,
                AssignedOn = DateTime.UtcNow.AddDays(-2)
            });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-6", InstitutionId = 1 };

        // User is no longer in group-1, but has a manual assignment
        await mapper.ApplyAsync(user, "uct", []);

        // Should NOT remove from Identity role because manual assignment exists
        userManager.Verify(m => m.RemoveFromRoleAsync(user, WombatRoles.Coordinator), Times.Never);
        // But the SSO tracking record IS removed
        db.UserRoleAssignments.Should().NotContain(a =>
            a.UserId == "user-6" && a.Role == WombatRoles.Coordinator && a.Source == RoleAssignmentSource.Sso);
        // Manual record is still there
        db.UserRoleAssignments.Should().ContainSingle(a =>
            a.UserId == "user-6" && a.Role == WombatRoles.Coordinator && a.Source == RoleAssignmentSource.Manual);
    }

    [Fact]
    public async Task Apply_DifferentProvider_DoesNotInterfere()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.AddRange(
            new SsoGroupRoleMapping
            {
                ProviderKey = "uct",
                ExternalGroupId = "group-1",
                ExternalGroupDisplayName = "UCT Group",
                WombatRole = WombatRoles.Trainee,
                InstitutionId = 1
            },
            new SsoGroupRoleMapping
            {
                ProviderKey = "wits",
                ExternalGroupId = "group-1",
                ExternalGroupDisplayName = "Wits Group",
                WombatRole = WombatRoles.Assessor,
                InstitutionId = 2
            });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);
        var user = new WombatIdentityUser { Id = "user-7", InstitutionId = 1 };

        var roles = await mapper.ApplyAsync(user, "uct", ["group-1"]);

        // Only UCT mapping should apply, not Wits
        userManager.Verify(m => m.AddToRoleAsync(user, WombatRoles.Trainee), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(user, WombatRoles.Assessor), Times.Never);
    }
}
