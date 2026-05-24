using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Sso;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Sso;

/// <summary>
/// T056.e scope-guard tests: InstitutionalAdmin sees and manages SSO mappings only for
/// their own institution.
/// </summary>
public sealed class SsoScopeGuardTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task List_InstitutionalAdmin_SeesOnlyOwn()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.AddRange(
            new SsoGroupRoleMapping { ProviderKey = "p", ExternalGroupId = "g1", ExternalGroupDisplayName = "A", WombatRole = WombatRoles.Trainee, InstitutionId = 1 },
            new SsoGroupRoleMapping { ProviderKey = "p", ExternalGroupId = "g2", ExternalGroupDisplayName = "B", WombatRole = WombatRoles.Trainee, InstitutionId = 2 });
        await db.SaveChangesAsync();

        var handler = new ListSsoGroupMappingsQueryHandler(db);
        var result = await handler.Handle(
            new ListSsoGroupMappingsQuery(TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Select(m => m.InstitutionId).Should().AllBeEquivalentTo(1);
    }

    [Fact]
    public async Task Delete_InstitutionalAdmin_OtherInstitution_Rejected()
    {
        await using var db = CreateDb();
        var mapping = new SsoGroupRoleMapping { ProviderKey = "p", ExternalGroupId = "g", ExternalGroupDisplayName = "X", WombatRole = WombatRoles.Trainee, InstitutionId = 2 };
        db.SsoGroupRoleMappings.Add(mapping);
        await db.SaveChangesAsync();

        var handler = new DeleteSsoGroupMappingCommandHandler(db);
        var act = () => handler.Handle(
            new DeleteSsoGroupMappingCommand(mapping.Id, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
