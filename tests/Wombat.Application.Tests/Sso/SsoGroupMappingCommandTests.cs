using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Sso;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Sso;

public sealed class SsoGroupMappingCommandTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Create_ValidMapping_Succeeds()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db);

        var id = await handler.Handle(new CreateSsoGroupMappingCommand(
            "uct", "group-1", "Surgery", WombatRoles.Trainee, 1, null, null, TestPrincipals.Administrator()), CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var mapping = await db.SsoGroupRoleMappings.FindAsync(id);
        mapping.Should().NotBeNull();
        mapping!.ProviderKey.Should().Be("uct");
        mapping.WombatRole.Should().Be(WombatRoles.Trainee);
    }

    [Fact]
    public async Task Create_AdministratorRole_Throws()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db);

        var act = () => handler.Handle(new CreateSsoGroupMappingCommand(
            "uct", "group-1", "Admins", WombatRoles.Administrator, 1, null, null, TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Administrator*cannot*SSO*");
    }

    [Fact]
    public async Task Delete_ExistingMapping_Succeeds()
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

        var handler = new DeleteSsoGroupMappingCommandHandler(db);
        await handler.Handle(new DeleteSsoGroupMappingCommand(1, TestPrincipals.Administrator()), CancellationToken.None);

        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NonExistentMapping_Throws()
    {
        await using var db = CreateDb();
        var handler = new DeleteSsoGroupMappingCommandHandler(db);

        var act = () => handler.Handle(new DeleteSsoGroupMappingCommand(999, TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Create_InstitutionalAdmin_RejectsOtherInstitution()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db);

        var act = () => handler.Handle(new CreateSsoGroupMappingCommand(
            "uct", "group-x", "Other", WombatRoles.Trainee, InstitutionId: 99, null, null, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
