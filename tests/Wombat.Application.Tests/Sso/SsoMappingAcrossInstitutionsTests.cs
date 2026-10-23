using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Sso;
using Wombat.Application.Features.Users;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Sso;

/// <summary>
/// A provider's mappings grant roles to the users who sign in through it, all of them at the provider's institution. A
/// mapping is therefore made only for the provider's own institution, which must be the caller's too, and the mapper
/// applies no other. A registrar who also administers manages no mapping (a trainee first). (T288)
/// </summary>
public sealed class SsoMappingAcrossInstitutionsTests
{
    private const int Kgk = 1;
    private const int Wits = 2;

    private static IOptions<SsoOptions> Providers() => Options.Create(new SsoOptions
    {
        Providers =
        [
            new SsoProviderOptions { Key = "kgk", InstitutionId = Kgk },
            new SsoProviderOptions { Key = "wits", InstitutionId = Wits }
        ]
    });

    [Fact]
    public async Task AnInstitutionalAdmin_CannotMapAnotherInstitutionsProvider_AndNothingIsStored()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

        var act = () => handler.Handle(Command("wits", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be(CreateSsoGroupMappingCommandHandler.ProviderNotTheInstitutions);
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task TheAdministrator_CannotMapAProviderForAnInstitutionItDoesNotServe()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

        var act = () => handler.Handle(Command("kgk", Wits, TestPrincipals.Administrator()), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be(CreateSsoGroupMappingCommandHandler.ProviderNotTheInstitutions);
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task AProviderThatIsNotConfigured_IsRefused()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

        var act = () => handler.Handle(Command("nowhere", Kgk, TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task TheControl_AnInstitutionalAdmin_MapsHerOwnProvider()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

        var id = await handler.Handle(Command("kgk", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await db.SsoGroupRoleMappings.SingleAsync()).Id.Should().Be(id);
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public async Task ATraineeWhoAdministers_ListsCreatesAndDeletesNoMapping(string role)
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(Mapping("kgk", Kgk));
        await db.SaveChangesAsync();
        var mappingId = (await db.SsoGroupRoleMappings.SingleAsync()).Id;
        var principal = TestPrincipals.InRoles([role, WombatRoles.Trainee], "registrar-admin", Kgk);

        var list = () => new ListSsoGroupMappingsQueryHandler(db, Providers()).Handle(new ListSsoGroupMappingsQuery(principal), CancellationToken.None);
        var create = () => new CreateSsoGroupMappingCommandHandler(db, Providers()).Handle(Command("kgk", Kgk, principal), CancellationToken.None);
        var delete = () => new DeleteSsoGroupMappingCommandHandler(db, Providers()).Handle(new DeleteSsoGroupMappingCommand(mappingId, principal), CancellationToken.None);

        (await list.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        (await create.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        (await delete.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        (await db.SsoGroupRoleMappings.CountAsync()).Should().Be(1, "nothing was added or deleted");
    }

    [Fact]
    public async Task TheMapper_IgnoresAMappingMadeAtAnotherInstitutionForTheSameProvider()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.Add(Mapping("kgk", Wits));
        await db.SaveChangesAsync();
        var roles = new List<string>();
        var userManager = RecordingUserManager(roles);
        var mapper = new SsoGroupMapper(userManager.Object, db, NullLogger<SsoGroupMapper>.Instance);

        await mapper.ApplyAsync(new WombatIdentityUser { Id = "kgk-user", InstitutionId = Kgk }, "kgk", Kgk, ["consultants"]);

        roles.Should().BeEmpty("the mapping was made at another institution, so it grants nothing here");
        db.UserRoleAssignments.Should().BeEmpty();
    }

    // Committee member at Any speciality: these tests are about the institution, and a role that needs no scope keeps the
    // mapping's scope checks (T376 build review, B3) out of them.
    private static CreateSsoGroupMappingCommand Command(string providerKey, int institutionId, System.Security.Claims.ClaimsPrincipal principal)
        => new(providerKey, "consultants", "Consultants", WombatRoles.CommitteeMember, institutionId, null, null, principal);

    private static SsoGroupRoleMapping Mapping(string providerKey, int institutionId) => new()
    {
        ProviderKey = providerKey,
        ExternalGroupId = "consultants",
        ExternalGroupDisplayName = "Consultants",
        WombatRole = WombatRoles.Assessor,
        InstitutionId = institutionId
    };

    private static Mock<UserManager<WombatIdentityUser>> RecordingUserManager(List<string> added)
    {
        var userManager = new Mock<UserManager<WombatIdentityUser>>(
            new Mock<IUserStore<WombatIdentityUser>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);
        userManager.Setup(m => m.GetRolesAsync(It.IsAny<WombatIdentityUser>())).ReturnsAsync(() => added.ToList());
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<WombatIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync((WombatIdentityUser _, string role) => { added.Add(role); return IdentityResult.Success; });
        userManager.Setup(m => m.UpdateSecurityStampAsync(It.IsAny<WombatIdentityUser>())).ReturnsAsync(IdentityResult.Success);
        return userManager;
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
