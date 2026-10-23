using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
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

    /// <summary>One provider, "uct", serving institution 1.</summary>
    internal static IOptions<SsoOptions> Providers()
        => Options.Create(new SsoOptions { Providers = [new SsoProviderOptions { Key = "uct", InstitutionId = 1 }] });

    [Fact]
    public async Task Create_ValidMapping_Succeeds()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

        var id = await handler.Handle(new CreateSsoGroupMappingCommand(
            "uct", "group-1", "Surgery", WombatRoles.Coordinator, 1, null, null, TestPrincipals.Administrator()), CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var mapping = await db.SsoGroupRoleMappings.FindAsync(id);
        mapping.Should().NotBeNull();
        mapping!.ProviderKey.Should().Be("uct");
        mapping.WombatRole.Should().Be(WombatRoles.Coordinator, "a role kept to no speciality needs no scope (an Assessor's is T376 B3's test)");
    }

    [Fact]
    public async Task Create_AdministratorRole_Throws()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

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

        var handler = new DeleteSsoGroupMappingCommandHandler(db, Providers());
        await handler.Handle(new DeleteSsoGroupMappingCommand(1, TestPrincipals.Administrator()), CancellationToken.None);

        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NonExistentMapping_Throws()
    {
        await using var db = CreateDb();
        var handler = new DeleteSsoGroupMappingCommandHandler(db, Providers());

        var act = () => handler.Handle(new DeleteSsoGroupMappingCommand(999, TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Create_InstitutionalAdmin_RejectsOtherInstitution()
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());

        var act = () => handler.Handle(new CreateSsoGroupMappingCommand(
            "uct", "group-x", "Other", WombatRoles.Assessor, InstitutionId: 99, null, null, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Admission's roles are never mapped (T333): a mapping to Trainee would grant it with no profile, give a graduate it
    /// back, and take it from a running trainee whenever their group changed. Refused to the Administrator too.
    /// </summary>
    [Theory]
    [InlineData(WombatRoles.Trainee, WombatRoles.Administrator)]
    [InlineData(WombatRoles.PendingTrainee, WombatRoles.Administrator)]
    [InlineData(WombatRoles.Trainee, WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.PendingTrainee, WombatRoles.InstitutionalAdmin)]
    public async Task Create_AdmissionsRoles_AreRefused_AndNothingIsStored(string role, string caller)
    {
        await using var db = CreateDb();
        var handler = new CreateSsoGroupMappingCommandHandler(db, Providers());
        var principal = caller == WombatRoles.Administrator
            ? TestPrincipals.Administrator()
            : TestPrincipals.InstitutionalAdmin(institutionId: 1);

        var act = () => handler.Handle(new CreateSsoGroupMappingCommand(
            "uct", "registrars", "Registrars", role, 1, null, null, principal), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be(CreateSsoGroupMappingCommandHandler.AdmissionRoleNotMappable);
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }
}
