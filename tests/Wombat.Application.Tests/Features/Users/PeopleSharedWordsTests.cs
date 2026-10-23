using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Institutions.Queries.GetMyInstitutionName;
using Wombat.Application.Features.Users;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// T376 (flow 12, build-contracts § 0): what the people pages' lanes share, written before them. A person's one scope in
/// words (S1: Users' Scope column, the who line, Assessors), the latest ended programme of a person with no role (S2), and
/// the caller's own institution's name, read now (S9).
/// </summary>
public sealed class PeopleSharedWordsTests
{
    private static readonly string[] None = [];

    // ---- S1: the scope cell's rule ----

    [Fact]
    public void AnInstitutionalAdmin_ReadsAsTheirInstitution()
        => PersonScopeWords.For([WombatRoles.InstitutionalAdmin], "Kgosi Kgari Teaching Hospital", null, None, None)
            .Should().Be("Kgosi Kgari Teaching Hospital");

    [Fact]
    public void ACollegeAdmin_ReadsAsTheirCollege()
        => PersonScopeWords.For([WombatRoles.CollegeAdmin], null, "College of Paediatricians of South Africa", None, None)
            .Should().Be("College of Paediatricians of South Africa");

    [Fact]
    public void SomeoneWithNoInstitution_ReadsADash()
        => PersonScopeWords.For([WombatRoles.Administrator], null, null, None, None).Should().Be(PersonScopeWords.NoScope);

    [Theory]
    [InlineData(new[] { "Paediatrics" }, new[] { "Paediatrics" }, "Paediatrics / Paediatrics")]
    [InlineData(new[] { "Paediatrics" }, new string[0], "Paediatrics")]
    [InlineData(new[] { "Surgery", "paediatrics" }, new string[0], "paediatrics, Surgery")]
    [InlineData(new string[0], new string[0], "Any speciality")]
    public void Otherwise_TheDisciplines_OrAnySpeciality(string[] specialities, string[] subSpecialities, string expected)
        => PersonScopeWords.For([WombatRoles.CommitteeMember], "Kgosi Kgari Teaching Hospital", null, specialities, subSpecialities)
            .Should().Be(expected);

    [Fact]
    public void APersonWithNoRole_ReadsByTheSameRule()
        => PersonScopeWords.For(None, "Kgosi Kgari Teaching Hospital", null, ["Paediatrics"], ["Paediatrics"])
            .Should().Be("Paediatrics / Paediatrics", "du Plessis's scope stays after his programme ends (U07)");

    [Fact]
    public async Task TheNames_AreReadOnceForEveryPerson_AndGiveEachTheirScope()
    {
        await using var db = CreateDb();
        var zulu = Person("zulu", [WombatRoles.CommitteeMember, WombatRoles.Assessor], institutionId: 1, specialities: [10], subSpecialities: [20]);
        var mbatha = Person("mbatha", [WombatRoles.InstitutionalAdmin], institutionId: 1);
        var kruger = Person("kruger", [WombatRoles.CollegeAdmin], institutionId: null, collegeId: 5);
        var smit = Person("smit", [WombatRoles.Coordinator], institutionId: 1);

        var names = await PersonScopeNames.LoadAsync(db, [zulu, mbatha, kruger, smit], CancellationToken.None);

        names.Scope(zulu).Should().Be("Paediatrics / Paediatrics");
        names.Scope(mbatha).Should().Be("Kgosi Kgari Teaching Hospital");
        names.Scope(kruger).Should().Be("College of Paediatricians of South Africa");
        names.Scope(smit).Should().Be("Any speciality");
        names.InstitutionName(kruger).Should().BeNull("the page writes Global (no institution)");
        names.InstitutionName(zulu).Should().Be("Kgosi Kgari Teaching Hospital");
    }

    // ---- S2: the latest ended programme ----

    [Fact]
    public async Task TheLatestEndedProgramme_SaysWhenAndHow_AndAPersonWithNoneIsLeftOut()
    {
        await using var db = CreateDb();
        var left = Profile(1, "duplessis");
        left.Deactivate(new DateOnly(2026, 10, 18), new DateOnly(2026, 10, 18));
        var completed = Profile(2, "molefe");
        completed.Complete(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 20));
        var earlier = Profile(3, "molefe");
        earlier.Deactivate(new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 1));
        db.Set<TraineeProfile>().AddRange(left, completed, earlier, Profile(4, "running"));
        await db.SaveChangesAsync();

        var ended = await EndedProgrammes.LatestForAsync(db, ["duplessis", "molefe", "running", "nobody"], CancellationToken.None);

        ended.Should().HaveCount(2);
        ended["duplessis"].Should().Be(new EndedProgrammeDto(new DateOnly(2026, 10, 18), Completed: false));
        ended["molefe"].Should().Be(new EndedProgrammeDto(new DateOnly(2026, 10, 20), Completed: true));
    }

    // ---- S9: her institution's name ----

    [Fact]
    public async Task HerInstitutionsName_IsReadNow_AndTheAdministratorHasNone()
    {
        await using var db = CreateDb();
        var handler = new GetMyInstitutionNameQueryHandler(db);

        (await handler.Handle(new GetMyInstitutionNameQuery(TestPrincipals.InstitutionalAdmin(1)), CancellationToken.None))
            .Should().Be("Kgosi Kgari Teaching Hospital");
        (await handler.Handle(new GetMyInstitutionNameQuery(TestPrincipals.Administrator()), CancellationToken.None))
            .Should().BeNull();
        (await handler.Handle(new GetMyInstitutionNameQuery(TestPrincipals.InstitutionalAdmin(99)), CancellationToken.None))
            .Should().BeNull("an institution that is gone has no name to show");
    }

    private static UserIdentityDetails Person(
        string id,
        IReadOnlyCollection<string> roles,
        int? institutionId,
        int[]? specialities = null,
        int[]? subSpecialities = null,
        int? collegeId = null)
        => new(id, $"{id}@kgk.example", id, id, institutionId, specialities ?? [], subSpecialities ?? [], roles,
            CollegeId: collegeId);

    private static TraineeProfile Profile(int id, string userId) => new()
    {
        Id = id,
        UserId = userId,
        InstitutionId = 1,
        CurriculumId = 1,
        ProgrammeStartDate = new DateOnly(2024, 1, 15),
        AdmittedOn = new DateOnly(2024, 1, 15),
        ExpectedCompletionDate = new DateOnly(2028, 1, 14)
    };

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        db.Institutions.Add(new Institution { Id = 1, Name = "Kgosi Kgari Teaching Hospital" });
        db.Set<College>().Add(new College { Id = 5, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        db.Set<Speciality>().Add(new Speciality { Id = 10, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 20, SpecialityId = 10, Name = "Paediatrics" });
        db.SaveChanges();
        return db;
    }
}
