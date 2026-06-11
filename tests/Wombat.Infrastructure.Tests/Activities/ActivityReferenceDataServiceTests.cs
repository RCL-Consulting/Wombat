using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

public sealed class ActivityReferenceDataServiceTests
{
    [Fact]
    public async Task GetEpaOptions_ScopesToCallerSubSpeciality()
    {
        await using var db = CreateDb();
        // Two sub-specialities under one speciality; caller only scoped to sub-spec 1.
        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-002", Title = "Resus", IsActive = true });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-001", Title = "Admission", IsActive = true });
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 2, Code = "OTHER-001", Title = "Out of scope", IsActive = true });
        db.Epas.Add(new Epa { Id = 4, SubSpecialityId = 1, Code = "PAED-099", Title = "Retired", IsActive = false });
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db, new StubUserAdministrationService([]));
        var principal = Principal(subSpecialityIds: [1]);

        var options = await service.GetEpaOptionsAsync(principal);

        // Only active EPAs in sub-spec 1, ordered by code, labelled "Code — Title".
        options.Select(o => o.Value).Should().Equal("2", "1");
        options[0].Label.Should().Be("PAED-001 — Admission");
    }

    [Fact]
    public async Task GetEpaOptions_AdministratorSeesAllActive()
    {
        await using var db = CreateDb();
        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "A-1", Title = "One", IsActive = true });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 2, Code = "A-2", Title = "Two", IsActive = true });
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db, new StubUserAdministrationService([]));
        var admin = Principal(roles: [WombatRoles.Administrator]);

        var options = await service.GetEpaOptionsAsync(admin);

        options.Select(o => o.Value).Should().BeEquivalentTo(["1", "2"]);
    }

    [Fact]
    public async Task GetEpaOptions_InstitutionalAdmin_SeesWholeNationalCatalogue()
    {
        await using var db = CreateDb();
        // EPAs are a national (College-owned) catalogue now (T091), so an InstitutionalAdmin building a
        // form sees every EPA regardless of College. (Adoption-based narrowing arrives in phase 4.)
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paeds" });
        db.Set<Speciality>().Add(new Speciality { Id = 2, CollegeId = 1, Name = "Surgery" });
        db.Set<Speciality>().Add(new Speciality { Id = 3, CollegeId = 2, Name = "Other college" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 10, SpecialityId = 1, Name = "Gen Paeds" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 11, SpecialityId = 2, Name = "Gen Surg" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 12, SpecialityId = 3, Name = "Other sub" });
        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 10, Code = "P-1", Title = "Paeds EPA", IsActive = true });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 11, Code = "S-1", Title = "Surg EPA", IsActive = true });
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 12, Code = "O-1", Title = "Other-college EPA", IsActive = true });
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db, new StubUserAdministrationService([]));
        // InstitutionalAdmin with no speciality/sub-speciality claims.
        var instAdmin = Principal(institutionId: 1, roles: [WombatRoles.InstitutionalAdmin]);

        var options = await service.GetEpaOptionsAsync(instAdmin);

        options.Select(o => o.Value).Should().BeEquivalentTo(["1", "2", "3"]);
    }

    [Fact]
    public async Task GetAssessorOptions_ScopesToCallerInstitution()
    {
        await using var db = CreateDb();
        var users = new List<UserIdentityDetails>
        {
            new("u1", "naidoo@kgk", "Thandi", "Naidoo", InstitutionId: 1, [], [], [WombatRoles.Assessor]),
            new("u2", "other@demo", "Out", "Scope", InstitutionId: 2, [], [], [WombatRoles.Assessor]),
        };
        var service = new ActivityReferenceDataService(db, new StubUserAdministrationService(users));
        var principal = Principal(institutionId: 1);

        var options = await service.GetAssessorOptionsAsync(principal);

        options.Should().ContainSingle();
        options[0].Value.Should().Be("u1");
        options[0].Label.Should().Be("Thandi Naidoo (naidoo@kgk)");
    }

    [Fact]
    public async Task GetScaleLevelOptions_ResolvesByIdAndOrdersByLevel()
    {
        await using var db = CreateDb();
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 2, Name = "Paed General Entrustment Scale" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 10, ScaleId = 2, Order = 2, Label = "Direct supervision" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 11, ScaleId = 2, Order = 1, Label = "Observation only" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 12, ScaleId = 99, Order = 1, Label = "Other scale" });
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db, new StubUserAdministrationService([]));

        var byId = await service.GetEntrustmentScaleLevelOptionsAsync("2");
        byId.Select(o => o.Value).Should().Equal("1", "2");
        byId[0].Label.Should().Be("1. Observation only");

        var byName = await service.GetEntrustmentScaleLevelOptionsAsync("Paed General Entrustment Scale");
        byName.Should().HaveCount(2);

        (await service.GetEntrustmentScaleLevelOptionsAsync(null)).Should().BeEmpty();
        (await service.GetEntrustmentScaleLevelOptionsAsync("nonexistent")).Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClaimsPrincipal Principal(
        int? institutionId = null,
        IReadOnlyCollection<int>? subSpecialityIds = null,
        IReadOnlyCollection<string>? roles = null)
    {
        var claims = new List<Claim>();
        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }
        foreach (var subId in subSpecialityIds ?? [])
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subId.ToString()));
        }
        foreach (var role in roles ?? [])
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private sealed class StubUserAdministrationService : IUserAdministrationService
    {
        private readonly IReadOnlyList<UserIdentityDetails> _users;
        public StubUserAdministrationService(IReadOnlyList<UserIdentityDetails> users) => _users = users;

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult(_users);

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<UserIdentityDetails?>(null);
        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_users);
        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
