using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Application.Tests.Features.Users;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.UsersList;

/// <summary>
/// T376, flow 12, lane U: Users as one page (S6, S7, F9, Q22), through the directory's default implementation of
/// <see cref="IUserAdministrationService.ListUsersPageAsync" />, which applies the real service's rules in memory. The SQL
/// half (the database's order, the one query for the page's roles) is <c>UsersPagePostgresTests</c>'.
/// </summary>
public sealed class ListUsersQueryTests
{
    private const int Kgk = 1;
    private const int Other = 2;
    private const int Paediatrics = 10;
    private const int PaediatricsSub = 20;
    private const int College = 5;

    [Theory]
    [InlineData("zulu")]
    [InlineData("thandi zulu")]
    [InlineData("zulu thandi")]
    [InlineData("  THANDI   Zul ")]
    [InlineData("zulu@kgk.wombat.local")]
    [InlineData("andi")]
    public async Task Search_FindsThandiZulu_ByAnyWordOfHerName_InEitherOrder_OrHerAddress(string search)
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(Mbatha(), Search: search));

        list.Rows.Select(row => row.UserId).Should().Equal("zulu");
        list.MatchCount.Should().Be(1);
        list.TotalCount.Should().Be(12, "the count of the scope, which the heading reads as \"1 of 12\"");
    }

    [Fact]
    public async Task Search_EveryWordMustMatch_SoTwoPeoplesNamesFindNobody()
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(Mbatha(), Search: "thandi patel"));

        list.Rows.Should().BeEmpty();
        list.MatchCount.Should().Be(0);
    }

    [Fact]
    public async Task TheOrder_IsLastName_ThenFirstName_IgnoringCase_SoTheParticlesSortByTheirLetters()
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(Mbatha()));

        list.Rows.Select(row => row.LastName).Should().Equal(
            "Botha", "du Plessis", "Khumalo", "Mbatha", "Mokoena", "Molefe", "Naidoo", "Patel", "Sithole", "Smit",
            "van Rensburg", "Zulu");
    }

    [Theory]
    [InlineData(WombatRoles.Assessor, new[] { "khumalo", "patel", "zulu" })]
    [InlineData(WombatRoles.CommitteeMember, new[] { "botha", "naidoo", "vanrensburg", "zulu" })]
    [InlineData(UsersFilterValues.NoRole, new[] { "duplessis", "molefe" })]
    public async Task Role_ListsItsHolders_AndNoRole_ThoseWhoHoldNone(string role, string[] expected)
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(Mbatha(), Role: role));

        list.Rows.Select(row => row.UserId).Should().Equal(expected);
    }

    [Fact]
    public async Task Status_LockedOut_IsTheAdministrativeLock_NotABruteForceLockout()
    {
        var (db, users) = Cast();

        var locked = await Handle(db, users, new ListUsersQuery(Mbatha(), Status: UsersFilterValues.Locked));
        var active = await Handle(db, users, new ListUsersQuery(Mbatha(), Status: UsersFilterValues.Active));

        locked.Rows.Select(row => row.UserId).Should().Equal(new[] { "patel" }, "Smit's failed passwords lift themselves (K25)");
        locked.Rows.Single().IsDeactivated.Should().BeTrue();
        active.Rows.Select(row => row.UserId).Should().Contain("smit").And.NotContain("patel");
        active.MatchCount.Should().Be(11);
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.CollegeAdmin)]
    public async Task AnInstitutionalAdmin_IsRefused_TheAdministratorsTwoRoles(string role)
    {
        var (db, users) = Cast();

        var list = () => Handle(db, users, new ListUsersQuery(Mbatha(), Role: role));

        (await list.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().NotContain("@");
        users.Lookups.Should().BeEmpty("refused before anything is read");
    }

    [Fact]
    public async Task TheAdministrator_MayFilterByHisTwoRoles()
    {
        var (db, users) = Cast();

        var administrators = await Handle(db, users, new ListUsersQuery(TestPrincipals.Administrator(), Role: WombatRoles.Administrator));
        var collegeAdmins = await Handle(db, users, new ListUsersQuery(TestPrincipals.Administrator(), Role: WombatRoles.CollegeAdmin));

        administrators.Rows.Select(row => row.UserId).Should().Equal("devadmin");
        collegeAdmins.Rows.Select(row => row.UserId).Should().Equal("collegeadmin");
    }

    [Theory]
    [InlineData("Wizard", null)]
    [InlineData(null, "asleep")]
    public async Task ARoleOrStatusThePageNeverOffers_IsRefused(string? role, string? status)
    {
        var (db, users) = Cast();

        var list = () => Handle(db, users, new ListUsersQuery(Mbatha(), Role: role, Status: status));

        await list.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AnInstitutionalAdmin_ListsHerInstitutionOnly_NeverAnotherNorAnAccountWithNone()
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(Mbatha(), PageSize: 100));

        list.Rows.Should().OnlyContain(row => row.InstitutionId == Kgk);
        list.Rows.Select(row => row.UserId).Should().NotContain(["elsewhere", "devadmin", "collegeadmin"]);
        (await Handle(db, users, new ListUsersQuery(Mbatha(), Search: "elsewhere"))).MatchCount.Should().Be(0);
    }

    [Fact]
    public async Task TheAdministrator_ListsEveryAccount_ThoseWithNoInstitutionIncluded()
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(TestPrincipals.Administrator(), PageSize: 100));

        list.TotalCount.Should().Be(15);
        list.Rows.Select(row => row.UserId).Should().Contain(["elsewhere", "devadmin", "collegeadmin"]);
        list.Rows.Single(row => row.UserId == "devadmin").InstitutionName.Should().BeNull();
        list.Rows.Single(row => row.UserId == "devadmin").Scope.Should().Be(PersonScopeWords.NoScope);
        list.Rows.Single(row => row.UserId == "collegeadmin").Scope.Should().Be("College of Paediatricians of South Africa");
    }

    [Fact]
    public async Task ATraineeWhoAdministersUsers_IsRefused_BeforeAnythingIsRead()
    {
        var (db, users) = Cast();
        var caller = TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin, WombatRoles.Trainee], "mbatha", institutionId: Kgk);

        var list = () => Handle(db, users, new ListUsersQuery(caller, Search: "zulu"));

        await list.Should().ThrowAsync<UnauthorizedAccessException>();
        users.Lookups.Should().BeEmpty();
    }

    [Fact]
    public async Task APage_HoldsItsSize_TheMatchCountAllOfThem_AndAPagePastTheLastAnswersTheLast()
    {
        var (db, users) = Cast();

        var first = await Handle(db, users, new ListUsersQuery(Mbatha(), PageSize: 10));
        var second = await Handle(db, users, new ListUsersQuery(Mbatha(), Page: 2, PageSize: 10));
        var beyond = await Handle(db, users, new ListUsersQuery(Mbatha(), Page: 9, PageSize: 10));

        first.Rows.Should().HaveCount(10);
        first.MatchCount.Should().Be(12);
        first.PageSize.Should().Be(10);
        second.Rows.Select(row => row.LastName).Should().Equal("van Rensburg", "Zulu");
        second.Page.Should().Be(2);
        beyond.Page.Should().Be(2);
        beyond.Rows.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(1000)]
    public async Task ASizeThePagerDoesNotOffer_ReadsAsTwenty(int size)
    {
        var (db, users) = Cast();

        var list = await Handle(db, users, new ListUsersQuery(Mbatha(), PageSize: size));

        list.PageSize.Should().Be(UsersFilterValues.DefaultPageSize);
    }

    [Fact]
    public async Task ARow_HasItsRolesInTheOneOrder_ItsScopeInWords_AndAnEndedProgramme_OnlyWithNoRole()
    {
        var (db, users) = Cast();

        var rows = (await Handle(db, users, new ListUsersQuery(Mbatha(), PageSize: 100))).Rows.ToDictionary(row => row.UserId);

        rows["zulu"].Roles.Should().Equal(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        rows["zulu"].Scope.Should().Be("Paediatrics / Paediatrics");
        rows["mbatha"].Scope.Should().Be("Kgosi Kgari Teaching Hospital");
        rows["smit"].Scope.Should().Be(PersonScopeWords.AnySpeciality);
        rows["duplessis"].Roles.Should().BeEmpty();
        rows["duplessis"].HasEndedProgramme.Should().BeTrue();
        rows["duplessis"].Scope.Should().Be("Paediatrics / Paediatrics", "a person with no role reads by the same rule (U07)");
        rows["molefe"].HasEndedProgramme.Should().BeTrue("a completed programme is ended too");
        rows["zulu"].HasEndedProgramme.Should().BeFalse();
        rows["vanrensburg"].HasEndedProgramme.Should().BeFalse("only an ended profile counts, never a running one");
    }

    [Fact]
    public async Task AnInstitutionalAdminWithNoInstitution_ListsNobody()
    {
        var (db, users) = Cast();
        var caller = TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin], "nowhere", institutionId: null);

        var list = await Handle(db, users, new ListUsersQuery(caller));

        list.Rows.Should().BeEmpty();
        list.TotalCount.Should().Be(0);
    }

    [Fact]
    public void TheWordsOfASearch_AreTrimmed_LowerCased_EachOnce_AndAtMostEight()
    {
        UsersFilterValues.WordsOf("  Thandi  ZULU thandi ").Should().Equal("thandi", "zulu");
        UsersFilterValues.WordsOf(null).Should().BeEmpty();
        UsersFilterValues.WordsOf(string.Join(' ', Enumerable.Range(1, 20).Select(n => $"w{n}"))).Should().HaveCount(UsersFilterValues.MaxWords);
    }

    private static System.Security.Claims.ClaimsPrincipal Mbatha()
        => TestPrincipals.InstitutionalAdmin(Kgk, userId: "mbatha");

    private static Task<UserListDto> Handle(ApplicationDbContext db, RecordingUserAdministrationService users, ListUsersQuery query)
        => new ListUsersQueryHandler(users, db).Handle(query, CancellationToken.None);

    /// <summary>2.12's ten at Kgosi Kgari, du Plessis (left) and Molefe (completed) with no role, and three elsewhere.</summary>
    private static (ApplicationDbContext Db, RecordingUserAdministrationService Users) Cast()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.Add(new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Institutions.Add(new Institution { Id = Other, Name = "Elsewhere Hospital", ShortCode = "ELS" });
        db.Colleges.Add(new College { Id = College, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = College, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = PaediatricsSub, SpecialityId = Paediatrics, Name = "Paediatrics" });
        var left = new TraineeProfile { Id = 1, UserId = "duplessis", InstitutionId = Kgk, IsActive = true };
        left.Deactivate(new DateOnly(2026, 10, 18), new DateOnly(2026, 10, 18));
        var completed = new TraineeProfile { Id = 2, UserId = "molefe", InstitutionId = Kgk, IsActive = true };
        completed.Complete(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 20));
        db.TraineeProfiles.AddRange(left, completed, new TraineeProfile { Id = 3, UserId = "vanrensburg", InstitutionId = Kgk, IsActive = true });
        db.SaveChanges();

        var users = new RecordingUserAdministrationService();
        int[] paeds = [Paediatrics];
        int[] paedsSub = [PaediatricsSub];
        users.Add(Person("zulu", "Thandi", "Zulu", Kgk, paeds, paedsSub, WombatRoles.Assessor, WombatRoles.CommitteeMember));
        users.Add(Person("botha", "Sarah", "Botha", Kgk, paeds, [], WombatRoles.CommitteeMember));
        users.Add(Person("khumalo", "Fatma", "Khumalo", Kgk, paeds, paedsSub, WombatRoles.Assessor));
        users.Add(Person("mbatha", "Nolwazi", "Mbatha", Kgk, [], [], WombatRoles.InstitutionalAdmin));
        users.Add(Person("mokoena", "Refilwe", "Mokoena", Kgk, paeds, [], WombatRoles.SpecialityAdmin));
        users.Add(Person("naidoo", "David", "Naidoo", Kgk, paeds, [], WombatRoles.CommitteeMember));
        users.Add(Person("patel", "Mohammed", "Patel", Kgk, paeds, paedsSub, WombatRoles.Assessor) with { IsLockedOut = true, IsDeactivated = true });
        users.Add(Person("sithole", "Kabelo", "Sithole", Kgk, paeds, paedsSub, WombatRoles.SubSpecialityAdmin));
        users.Add(Person("smit", "Pieter", "Smit", Kgk, [], [], WombatRoles.Coordinator) with { IsLockedOut = true });
        users.Add(Person("vanrensburg", "John", "van Rensburg", Kgk, [], [], WombatRoles.CommitteeMember));
        users.Add(Person("duplessis", "Pieter", "du Plessis", Kgk, paeds, paedsSub));
        users.Add(Person("molefe", "Lerato", "Molefe", Kgk, paeds, paedsSub));
        users.Add(Person("elsewhere", "Else", "Where", Other, [], [], WombatRoles.Coordinator));
        users.Add(Person("devadmin", "Demo", "Administrator", null, [], [], WombatRoles.Administrator));
        users.Add(Person("collegeadmin", "College", "Admin", null, [], [], WombatRoles.CollegeAdmin) with { CollegeId = College });
        return (db, users);
    }

    private static UserIdentityDetails Person(
        string id, string first, string last, int? institutionId, int[] specialities, int[] subSpecialities, params string[] roles)
        => new(id, $"{id}@kgk.wombat.local", first, last, institutionId, specialities, subSpecialities, roles);
}
