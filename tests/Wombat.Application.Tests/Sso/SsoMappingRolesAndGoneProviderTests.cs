using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Sso;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Sso;

/// <summary>
/// T376 (flow 12): Q19, a mapping to Institutional admin is the Administrator's alone to make (O06: a posted role the
/// page's select does not offer her, refused beside Wombat role); S4, only a staff role is mapped; S5, a mapping whose
/// provider is no longer set up for its institution is listed as gone, and only the Administrator deletes it.
/// </summary>
public sealed class SsoMappingRolesAndGoneProviderTests
{
    private const int Kgk = 2;
    private const int Paediatrics = 10;
    private const int Surgery = 11;
    private const int GeneralPaediatrics = 20;
    private const int GeneralSurgery = 21;
    private const int Neonatology = 22;

    /// <summary>KGK's provider, "kgk", and "old-kgk" no longer configured; "moved" now serves another institution.</summary>
    private static IOptions<SsoOptions> Providers() => Options.Create(new SsoOptions
    {
        Providers =
        [
            new SsoProviderOptions { Key = "kgk", DisplayName = "KGK staff sign-in", InstitutionId = Kgk },
            new SsoProviderOptions { Key = "moved", DisplayName = "Moved sign-in", InstitutionId = 9 }
        ]
    });

    // ─── Q19 and S4: the roles a mapping gives ──────────────────────────────

    [Fact]
    public async Task AnInstitutionalAdmin_CannotMapAGroupToInstitutionalAdmin_BesideWombatRole_AndNothingIsStored()
    {
        await using var db = CreateDb();

        var act = () => Create(db, WombatRoles.InstitutionalAdmin, TestPrincipals.InstitutionalAdmin(Kgk));

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.InstitutionalAdminNotMappable);
        refusal.Field.Should().Be(SsoMappingField.Role);
        refusal.Message.Should().Be("Only a Wombat administrator can map a group to Institutional admin.");
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task TheControl_TheAdministrator_MapsAGroupToInstitutionalAdmin()
    {
        await using var db = CreateDb();

        await Create(db, WombatRoles.InstitutionalAdmin, TestPrincipals.Administrator());

        (await db.SsoGroupRoleMappings.SingleAsync()).WombatRole.Should().Be(WombatRoles.InstitutionalAdmin);
    }

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Assessor)]
    public async Task AnInstitutionalAdmin_MapsEveryStaffRoleTheSelectOffersHer(string role)
    {
        await using var db = CreateDb();

        await Create(db, role, TestPrincipals.InstitutionalAdmin(Kgk));

        (await db.SsoGroupRoleMappings.SingleAsync()).WombatRole.Should().Be(role);
    }

    [Theory]
    [InlineData(WombatRoles.CollegeAdmin)]
    [InlineData("NotARole")]
    public async Task ARoleNoInstitutionsGroupCanGive_IsRefused_EvenToTheAdministrator(string role)
    {
        await using var db = CreateDb();

        var act = () => Create(db, role, TestPrincipals.Administrator());

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.RoleNotMappable);
        refusal.Field.Should().Be(SsoMappingField.Role);
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task TheAdministratorRefusal_AndTheAdmissionRefusal_AreTypedOnWombatRole()
    {
        await using var db = CreateDb();

        var administrator = (await ((Func<Task>)(() => Create(db, WombatRoles.Administrator, TestPrincipals.Administrator())))
            .Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        var trainee = (await ((Func<Task>)(() => Create(db, WombatRoles.Trainee, TestPrincipals.InstitutionalAdmin(Kgk))))
            .Should().ThrowAsync<SsoMappingRefusedException>()).Which;

        administrator.Reason.Should().Be(SsoMappingRefusal.AdministratorNotMappable);
        administrator.Field.Should().Be(SsoMappingField.Role);
        trainee.Reason.Should().Be(SsoMappingRefusal.AdmissionRoleNotMappable);
        trainee.Message.Should().Be(CreateSsoGroupMappingCommandHandler.AdmissionRoleNotMappable);
    }

    [Fact]
    public async Task AProviderNotTheInstitutions_IsTypedOnProvider()
    {
        await using var db = CreateDb();

        var act = () => new CreateSsoGroupMappingCommandHandler(db, Providers()).Handle(
            new CreateSsoGroupMappingCommand("old-kgk", "g", "Group", WombatRoles.Assessor, Kgk, null, null,
                TestPrincipals.InstitutionalAdmin(Kgk)),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.ProviderNotTheInstitutions);
        refusal.Field.Should().Be(SsoMappingField.Provider);
    }

    // ─── S5: a gone provider's mappings ─────────────────────────────────────

    [Fact]
    public async Task TheList_SaysWhichMappingsProviderIsGone_AndNamesEachProvider()
    {
        await using var db = CreateDb();
        db.SsoGroupRoleMappings.AddRange(Mapping("kgk", "live"), Mapping("old-kgk", "removed"), Mapping("moved", "elsewhere"));
        await db.SaveChangesAsync();

        var rows = await new ListSsoGroupMappingsQueryHandler(db, Providers())
            .Handle(new ListSsoGroupMappingsQuery(TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        rows.Select(row => (row.ExternalGroupId, row.ProviderGone, row.ProviderName)).Should().BeEquivalentTo(new[]
        {
            ("live", false, "KGK staff sign-in"),
            ("removed", true, "old-kgk"),
            ("elsewhere", true, "Moved sign-in")
        }, "a provider that now serves another institution gives KGK's people nothing either");
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CannotDeleteAGoneProvidersMapping_AndItStays()
    {
        await using var db = CreateDb();
        var mapping = Mapping("old-kgk", "removed");
        db.SsoGroupRoleMappings.Add(mapping);
        await db.SaveChangesAsync();

        var act = () => Delete(db, mapping.Id, TestPrincipals.InstitutionalAdmin(Kgk));

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.ProviderGone);
        refusal.Message.Should().Be(DeleteSsoGroupMappingCommandHandler.ProviderGoneAdministratorOnly);
        db.SsoGroupRoleMappings.Should().ContainSingle();
    }

    [Fact]
    public async Task TheAdministrator_DeletesAGoneProvidersMapping()
    {
        await using var db = CreateDb();
        var mapping = Mapping("old-kgk", "removed");
        db.SsoGroupRoleMappings.Add(mapping);
        await db.SaveChangesAsync();

        await Delete(db, mapping.Id, TestPrincipals.Administrator());

        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task TheControl_AnInstitutionalAdmin_DeletesHerLiveProvidersMapping()
    {
        await using var db = CreateDb();
        var mapping = Mapping("kgk", "live");
        db.SsoGroupRoleMappings.Add(mapping);
        await db.SaveChangesAsync();

        await Delete(db, mapping.Id, TestPrincipals.InstitutionalAdmin(Kgk));

        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task AnUnknownId_ReadsAsNotFound_ForEveryone()
    {
        await using var db = CreateDb();

        var act = () => Delete(db, 404, TestPrincipals.Administrator());

        (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which.Reason.Should().Be(SsoMappingRefusal.NotFound);
    }

    // ─── B3: the scope a mapping gives ──────────────────────────────────────

    [Theory]
    [InlineData(WombatRoles.Assessor, null, null, SsoMappingField.Speciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.Assessor, Paediatrics, null, SsoMappingField.SubSpeciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.SubSpecialityAdmin, null, null, SsoMappingField.Speciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.SpecialityAdmin, null, null, SsoMappingField.Speciality, "Speciality administrators must be scoped to a speciality.")]
    public async Task B3_ARoleWhoseNeedTheMappingDoesNotMeet_IsRefused_UnderItsField(
        string role, int? specialityId, int? subSpecialityId, SsoMappingField field, string words)
    {
        await using var db = CreateDb();

        var act = () => Create(db, role, TestPrincipals.InstitutionalAdmin(Kgk), specialityId, subSpecialityId);

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.ScopeNeeded);
        refusal.Field.Should().Be(field);
        refusal.Message.Should().Be(words);
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Surgery, GeneralSurgery, SsoMappingField.Speciality)]
    [InlineData(Paediatrics, Neonatology, SsoMappingField.SubSpeciality)]
    [InlineData(Paediatrics, GeneralSurgery, SsoMappingField.SubSpeciality)]
    public async Task B3_AScopeTheInstitutionHasNotAdopted_OrASubSpecialityOfAnotherSpeciality_IsRefused(
        int specialityId, int subSpecialityId, SsoMappingField field)
    {
        // A crafted request (the page offers only valid pairs): Surgery / General Surgery, which KGK has not adopted;
        // Neonatology, not adopted; General Surgery under Paediatrics, a pair of two specialities.
        await using var db = CreateDb();

        var act = () => Create(db, WombatRoles.Assessor, TestPrincipals.InstitutionalAdmin(Kgk), specialityId, subSpecialityId);

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.NotAdopted);
        refusal.Field.Should().Be(field);
        db.SsoGroupRoleMappings.Should().BeEmpty();
    }

    [Fact]
    public async Task B3_ASubSpecialityOnARoleKeptToASpeciality_IsRefused()
    {
        await using var db = CreateDb();

        var act = () => Create(db, WombatRoles.Coordinator, TestPrincipals.InstitutionalAdmin(Kgk), Paediatrics, GeneralPaediatrics);

        var refusal = (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which;
        refusal.Reason.Should().Be(SsoMappingRefusal.NotScopable);
        refusal.Field.Should().Be(SsoMappingField.SubSpeciality);
    }

    [Fact]
    public async Task B3_AScopeOnAnInstitutionalAdminMapping_IsRefused_EvenToTheAdministrator()
    {
        await using var db = CreateDb();

        var act = () => Create(db, WombatRoles.InstitutionalAdmin, TestPrincipals.Administrator(), Paediatrics, null);

        (await act.Should().ThrowAsync<SsoMappingRefusedException>()).Which.Reason.Should().Be(SsoMappingRefusal.NotScopable);
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator, null)]
    [InlineData(WombatRoles.Coordinator, Paediatrics)]
    [InlineData(WombatRoles.CommitteeMember, Paediatrics)]
    public async Task B3_TheControl_ARoleKeptToAnAdoptedSpecialityOrToNone_IsMapped(string role, int? specialityId)
    {
        await using var db = CreateDb();

        await Create(db, role, TestPrincipals.InstitutionalAdmin(Kgk), specialityId, null);

        (await db.SsoGroupRoleMappings.SingleAsync()).SpecialityId.Should().Be(specialityId);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    /// <summary>A mapping with the scope its role needs (T376 build review, B3): Paediatrics, and Paediatrics / Paediatrics where a sub-speciality is needed.</summary>
    private static Task<int> Create(ApplicationDbContext db, string role, ClaimsPrincipal principal)
        => Create(
            db,
            role,
            principal,
            UserAdministrationRules.RoleNeedsSpeciality(role) ? Paediatrics : null,
            UserAdministrationRules.RoleTakesSubSpeciality(role) ? GeneralPaediatrics : null);

    private static Task<int> Create(ApplicationDbContext db, string role, ClaimsPrincipal principal, int? specialityId, int? subSpecialityId)
        => new CreateSsoGroupMappingCommandHandler(db, Providers()).Handle(
            new CreateSsoGroupMappingCommand("kgk", "kgk-group", "KGK group", role, Kgk, specialityId, subSpecialityId, principal),
            CancellationToken.None);

    private static Task Delete(ApplicationDbContext db, int id, ClaimsPrincipal principal)
        => new DeleteSsoGroupMappingCommandHandler(db, Providers()).Handle(
            new DeleteSsoGroupMappingCommand(id, principal), CancellationToken.None);

    private static SsoGroupRoleMapping Mapping(string providerKey, string groupId) => new()
    {
        ProviderKey = providerKey,
        ExternalGroupId = groupId,
        ExternalGroupDisplayName = groupId,
        WombatRole = WombatRoles.CommitteeMember,
        InstitutionId = Kgk
    };

    private static ApplicationDbContext CreateDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Institutions.Add(new Wombat.Domain.Institutions.Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Set<College>().Add(new College { Id = 1, Name = "College", ShortCode = "C" });
        db.Set<Speciality>().AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery" });
        db.Set<SubSpeciality>().AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "Paediatrics" },
            new SubSpeciality { Id = Neonatology, SpecialityId = Paediatrics, Name = "Neonatology" },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery" });
        // KGK has adopted Paediatrics / Paediatrics only; Neonatology and Surgery are not KGK's.
        db.Set<InstitutionCurriculumAdoption>().Add(
            new InstitutionCurriculumAdoption { Id = 1, InstitutionId = Kgk, CurriculumId = 1, SubSpecialityId = GeneralPaediatrics, AdoptedOn = new DateOnly(2026, 1, 1) });
        db.SaveChanges();
        return db;
    }
}
