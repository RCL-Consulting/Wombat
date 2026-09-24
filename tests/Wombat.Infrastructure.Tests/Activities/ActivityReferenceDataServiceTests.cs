using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

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

        var service = new ActivityReferenceDataService(db);
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

        var service = new ActivityReferenceDataService(db);
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

        var service = new ActivityReferenceDataService(db);
        // InstitutionalAdmin with no speciality/sub-speciality claims.
        var instAdmin = Principal(institutionId: 1, roles: [WombatRoles.InstitutionalAdmin]);

        var options = await service.GetEpaOptionsAsync(instAdmin);

        options.Select(o => o.Value).Should().BeEquivalentTo(["1", "2", "3"]);
    }

    [Fact]
    public async Task GetNomineeOptions_ListsTheActivityInstitutionsEligiblePeople_NotTheCallers()
    {
        // T102: the picker is the nominee directory, keyed on the activity (its stamped institution and subject), with
        // no caller in the question. Everyone who fails one condition is a decoy.
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "u1", 1, WombatRoles.Assessor).FirstName = "Thandi";
        db.Users.Local.Single(user => user.Id == "u1").LastName = "Naidoo";
        db.Users.Local.Single(user => user.Id == "u1").Email = "naidoo@kgk";
        NomineeSeed.AddUser(db, "other-institution", 2, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, "not-an-assessor", 1, WombatRoles.Coordinator);
        NomineeSeed.AddUser(db, "deactivated", 1, DateTimeOffset.MaxValue, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, "briefly-locked", 1, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Assessor);
        NomineeSeed.AddUser(db, "subject", 1, WombatRoles.Assessor, WombatRoles.Trainee);
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        var options = await service.GetNomineeOptionsAsync(
            new NomineeOptionScope("subject", [WombatRoles.Assessor], ForExistingActivity: true, ActivityInstitutionId: 1, StoredValue: null));

        options.Select(option => option.Value).Should().BeEquivalentTo(["u1", "briefly-locked"],
            "a brute-force lockout lifts by itself; only a deactivation removes someone");
        options.Single(option => option.Value == "u1").Label.Should().Be("Thandi Naidoo (naidoo@kgk)");
    }

    [Fact]
    public async Task GetNomineeOptions_KeepsAStoredNomineeWhoHasFallenOffTheList_LabelledNeutrally()
    {
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "moved-away", 2, WombatRoles.Assessor);
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        var options = await service.GetNomineeOptionsAsync(
            new NomineeOptionScope("subject", [WombatRoles.Assessor], ForExistingActivity: true, ActivityInstitutionId: 1, StoredValue: "moved-away"));

        options.Should().ContainSingle();
        options[0].Value.Should().Be("moved-away");
        options[0].Label.Should().EndWith("(not on the current list)");
    }

    [Fact]
    public async Task GetNomineeOptions_OnAnExistingActivityWithNoStampedInstitution_ListsNobody()
    {
        // The subject is resolvable to institution 1, where u1 is eligible, so an empty list can only mean the null
        // stamp was used as the null stamp, not quietly replaced by the subject's institution.
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "subject", 1, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, "u1", 1, WombatRoles.Assessor);
        db.Set<TraineeProfile>().Add(ActiveProfile(id: 1, userId: "subject", institutionId: 1));
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        var options = await service.GetNomineeOptionsAsync(
            new NomineeOptionScope("subject", [WombatRoles.Assessor], ForExistingActivity: true, ActivityInstitutionId: null, StoredValue: null));

        options.Should().BeEmpty("the write path accepts nobody against a null stamp");

        // Sanity: the same subject on the create page does resolve to institution 1 and finds u1, so the fixture could
        // have produced a non-empty list.
        var onCreate = await service.GetNomineeOptionsAsync(
            new NomineeOptionScope("subject", [WombatRoles.Assessor], ForExistingActivity: false, ActivityInstitutionId: null, StoredValue: null));

        onCreate.Select(option => option.Value).Should().Equal("u1");
    }

    [Fact]
    public async Task GetNomineeOptions_OnAnExistingActivity_UsesTheStamp_NotWhereTheSubjectTrainsNow()
    {
        // The activity was stamped at institution 1; the trainee has since moved to 2. The write path judges an
        // existing activity's nominee against its stamp, so the picker must offer institution 1's people, not 2's.
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "subject", 2, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, "at-stamped-institution", 1, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, "at-current-institution", 2, WombatRoles.Assessor);
        db.Set<TraineeProfile>().Add(ActiveProfile(id: 1, userId: "subject", institutionId: 2));
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        var options = await service.GetNomineeOptionsAsync(
            new NomineeOptionScope("subject", [WombatRoles.Assessor], ForExistingActivity: true, ActivityInstitutionId: 1, StoredValue: null));

        options.Select(option => option.Value).Should().Equal("at-stamped-institution");
    }

    [Fact]
    public async Task GetNomineeOptions_OnTheCreatePage_ResolvesTheSubjectsInstitutionAsTheCreateWillStampIt()
    {
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "subject", 9, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, "at-profile-institution", 1, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, "at-identity-institution", 9, WombatRoles.Assessor);
        // The active profile wins over the Identity row, exactly as ActivityService stamps it.
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "subject",
            InstitutionId = 1,
            CurriculumId = 77,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        var options = await service.GetNomineeOptionsAsync(
            new NomineeOptionScope("subject", [WombatRoles.Assessor], ForExistingActivity: false, ActivityInstitutionId: null, StoredValue: null));

        options.Select(option => option.Value).Should().Equal("at-profile-institution");
    }

    [Fact]
    public async Task GetNomineeOptions_RequiresEveryRole()
    {
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "both", 1, WombatRoles.Assessor, WombatRoles.CommitteeMember);
        NomineeSeed.AddUser(db, "assessor-only", 1, WombatRoles.Assessor);
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        var options = await service.GetNomineeOptionsAsync(new NomineeOptionScope(
            "subject", [WombatRoles.Assessor, WombatRoles.CommitteeMember], ForExistingActivity: true, ActivityInstitutionId: 1, StoredValue: null));

        options.Select(option => option.Value).Should().Equal("both");
    }

    [Fact]
    public async Task GetUserOption_ReturnsOnePersonsLabel_OrNullForAnUnknownId()
    {
        await using var db = CreateDb();
        NomineeSeed.AddUser(db, "u1", 1, WombatRoles.Assessor);
        await db.SaveChangesAsync();

        var service = new ActivityReferenceDataService(db);

        (await service.GetUserOptionAsync("u1"))!.Value.Should().Be("u1");
        (await service.GetUserOptionAsync("nobody")).Should().BeNull();
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

        var service = new ActivityReferenceDataService(db);

        var byId = await service.GetEntrustmentScaleLevelOptionsAsync("2");
        byId.Select(o => o.Value).Should().Equal("1", "2");
        // T100: the Value stays the Order (the stored, compared datum); the Label is the rung alone.
        byId[0].Label.Should().Be("Observation only");

        var byName = await service.GetEntrustmentScaleLevelOptionsAsync("Paed General Entrustment Scale");
        byName.Should().HaveCount(2);

        (await service.GetEntrustmentScaleLevelOptionsAsync(null)).Should().BeEmpty();
        (await service.GetEntrustmentScaleLevelOptionsAsync("nonexistent")).Should().BeEmpty();
    }

    private static TraineeProfile ActiveProfile(int id, string userId, int institutionId)
        => new()
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = 77,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        };

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
}
