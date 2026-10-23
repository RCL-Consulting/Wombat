using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Assessors;
using Wombat.Application.Features.Users;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Assessors;

/// <summary>
/// T376 (flow 12, lane T): the Assessors list is of every holder of the Assessor role in the caller's scope, joined to a
/// training record if one is kept (Q2), by last name, a page at a time (F9); a removed Assessor's record is kept and not
/// listed, and listed again with the role (Q18). The user page's section reads one person's record, and the old edit route
/// follows a profile id to its person (S10), each reading an out-of-scope person as none. The save's words are the
/// section's (T05, Spec 3).
/// </summary>
public sealed class AssessorListAndTrainingTests
{
    private const int Kgk = 1;
    private const int Other = 2;
    private const int Paediatrics = 5;
    private const int PaediatricsSub = 9;

    // ─── the list ────────────────────────────────────────────────────────────

    [Fact]
    public async Task EveryHolderAtHerInstitution_IsListed_WithOrWithoutARecord_ByLastNameIgnoringCase()
    {
        await using var db = CreateDb();
        db.Set<AssessorProfile>().Add(Profile(3, "zulu", AssessorTrainingStatus.Trained, new DateOnly(2018, 10, 20)));
        await db.SaveChangesAsync();
        var users = new Users(
            Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.Assessor, WombatRoles.CommitteeMember),
            Person("plessis", "Pieter", "du Plessis", Kgk, WombatRoles.Assessor),
            Person("botha", "Sarah", "Botha", Kgk, WombatRoles.Assessor),
            Person("elsewhere", "Anna", "Abel", Other, WombatRoles.Assessor),
            Person("rensburg", "John", "van Rensburg", Kgk, WombatRoles.CommitteeMember));

        var list = await List(db, users, TestPrincipals.InstitutionalAdmin(Kgk));

        list.Rows.Select(row => row.UserId).Should().Equal(["botha", "plessis", "zulu"],
            "every holder at her institution, by last name ignoring case: du Plessis under d; van Rensburg, never an " +
            "Assessor, and another institution's holder are not listed");
        list.TotalCount.Should().Be(3);
        var botha = list.Rows[0];
        botha.ProfileId.Should().BeNull("no record: the list's Not recorded and Record training");
        botha.TrainingStatus.Should().BeNull();
        var zulu = list.Rows[2];
        zulu.ProfileId.Should().Be(3);
        zulu.TrainingStatus.Should().Be(AssessorTrainingStatus.Trained);
        zulu.TrainingCompletedOn.Should().Be(new DateOnly(2018, 10, 20));
    }

    [Fact]
    public async Task EachRow_CarriesThePersonsOneScope_InWords()
    {
        await using var db = CreateDb();
        var users = new Users(
            Person("zulu", "Thandi", "Zulu", Kgk, [Paediatrics], [PaediatricsSub], WombatRoles.Assessor),
            Person("smit", "Pieter", "Smit", Kgk, [], [], WombatRoles.Assessor));

        var rows = (await List(db, users, TestPrincipals.InstitutionalAdmin(Kgk))).Rows;

        rows.Single(row => row.UserId == "zulu").Scope.Should().Be("Paediatrics / Paediatrics");
        rows.Single(row => row.UserId == "smit").Scope.Should().Be(PersonScopeWords.AnySpeciality);
        rows.Should().OnlyContain(row => row.InstitutionName == "Kgosi Kgari Teaching Hospital");
    }

    [Fact]
    public async Task ARemovedAssessorsRecord_IsKept_AndNotListed_ThenListedAgainWithTheRole()
    {
        await using var db = CreateDb();
        db.Set<AssessorProfile>().Add(Profile(4, "zulu", AssessorTrainingStatus.Trained, null));
        await db.SaveChangesAsync();

        var removed = new Users(Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.CommitteeMember));
        (await List(db, removed, TestPrincipals.InstitutionalAdmin(Kgk))).Rows.Should().BeEmpty("Q18: hidden with the role");
        (await db.Set<AssessorProfile>().CountAsync()).Should().Be(1, "Q18: the record is kept, not deleted");

        var givenBack = new Users(Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.CommitteeMember, WombatRoles.Assessor));
        (await List(db, givenBack, TestPrincipals.InstitutionalAdmin(Kgk))).Rows
            .Should().ContainSingle().Which.ProfileId.Should().Be(4, "shown again, with its record, when the role is given back");
    }

    [Fact]
    public async Task TheAdministrator_ListsEveryHolder_AtEveryInstitution_AndWithNone()
    {
        await using var db = CreateDb();
        var users = new Users(
            Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.Assessor),
            Person("elsewhere", "Anna", "Abel", Other, WombatRoles.Assessor),
            Person("nowhere", "Ned", "Nobody", null, WombatRoles.Assessor));

        var rows = (await List(db, users, TestPrincipals.Administrator())).Rows;

        rows.Select(row => row.UserId).Should().Equal("elsewhere", "nowhere", "zulu");
        rows.Single(row => row.UserId == "nowhere").InstitutionName.Should().BeNull("the page writes Global (no institution)");
    }

    [Fact]
    public async Task ThirtyHolders_AreTwentyAPage_TheTotalCountingAll()
    {
        await using var db = CreateDb();
        var users = new Users(Enumerable.Range(1, 30)
            .Select(i => Person($"a{i:00}", "Assessor", $"Name{i:00}", Kgk, WombatRoles.Assessor)).ToArray());
        var principal = TestPrincipals.InstitutionalAdmin(Kgk);

        var first = await List(db, users, principal);
        var second = await new ListAssessorsQueryHandler(db, users)
            .Handle(new ListAssessorsQuery(principal, Page: 2), CancellationToken.None);
        var tooFar = await new ListAssessorsQueryHandler(db, users)
            .Handle(new ListAssessorsQuery(principal, Page: 9, PageSize: 50), CancellationToken.None);

        first.Rows.Should().HaveCount(20);
        first.TotalCount.Should().Be(30);
        first.PageSize.Should().Be(20);
        second.Rows.Select(row => row.UserId).Should().Equal(Enumerable.Range(21, 10).Select(i => $"a{i:00}"));
        tooFar.Page.Should().Be(1, "a page past the last is the last");
        tooFar.Rows.Should().HaveCount(30);
    }

    [Fact]
    public async Task ATraineeWhoAdministers_IsRefused_BeforeAnythingIsRead()
    {
        await using var db = CreateDb();
        var users = new Users(Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.Assessor));
        var principal = TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin, WombatRoles.Trainee], "registrar-admin", Kgk);

        var act = () => List(db, users, principal);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        users.Reads.Should().Be(0);
    }

    // ─── one person's record, and the old route ──────────────────────────────

    [Fact]
    public async Task TheTrainingRead_IsThePersonsRecord_AndNoneOutsideHerScope()
    {
        await using var db = CreateDb();
        db.Set<AssessorProfile>().AddRange(
            Profile(3, "zulu", AssessorTrainingStatus.Provisional, new DateOnly(2026, 9, 20)),
            Profile(8, "elsewhere", AssessorTrainingStatus.Trained, null));
        await db.SaveChangesAsync();
        var users = new Users(
            Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.Assessor),
            Person("elsewhere", "Anna", "Abel", Other, WombatRoles.Assessor),
            Person("botha", "Sarah", "Botha", Kgk, WombatRoles.Assessor));
        var handler = new GetAssessorTrainingQueryHandler(db, users);
        var her = TestPrincipals.InstitutionalAdmin(Kgk);

        (await handler.Handle(new GetAssessorTrainingQuery("zulu", her), CancellationToken.None))
            .Should().Be(new AssessorTrainingDto(3, AssessorTrainingStatus.Provisional, new DateOnly(2026, 9, 20), "MBChB"));
        (await handler.Handle(new GetAssessorTrainingQuery("botha", her), CancellationToken.None)).Should().BeNull("no record");
        (await handler.Handle(new GetAssessorTrainingQuery("elsewhere", her), CancellationToken.None))
            .Should().BeNull("another institution's person reads as one with no record");
        (await handler.Handle(new GetAssessorTrainingQuery("elsewhere", TestPrincipals.Administrator()), CancellationToken.None))
            .Should().NotBeNull();
    }

    [Fact]
    public async Task TheOldRoute_FollowsAProfileIdToItsPerson_AndNoWhereElse()
    {
        await using var db = CreateDb();
        db.Set<AssessorProfile>().AddRange(
            Profile(3, "zulu", AssessorTrainingStatus.Trained, null),
            Profile(8, "elsewhere", AssessorTrainingStatus.Trained, null));
        await db.SaveChangesAsync();
        var users = new Users(
            Person("zulu", "Thandi", "Zulu", Kgk, WombatRoles.Assessor),
            Person("elsewhere", "Anna", "Abel", Other, WombatRoles.Assessor));
        var handler = new GetAssessorProfileOwnerQueryHandler(db, users);
        var her = TestPrincipals.InstitutionalAdmin(Kgk);

        (await handler.Handle(new GetAssessorProfileOwnerQuery(3, her), CancellationToken.None)).Should().Be("zulu");
        (await handler.Handle(new GetAssessorProfileOwnerQuery(99, her), CancellationToken.None)).Should().BeNull("unknown");
        (await handler.Handle(new GetAssessorProfileOwnerQuery(8, her), CancellationToken.None))
            .Should().BeNull("out of scope reads as unknown");
        (await handler.Handle(new GetAssessorProfileOwnerQuery(8, TestPrincipals.Administrator()), CancellationToken.None))
            .Should().Be("elsewhere");
    }

    // ─── the save's words and refusals ───────────────────────────────────────

    [Theory]
    [InlineData(AssessorTrainingStatus.Provisional, "2026-09-20", "Training saved for Fatma Khumalo: Provisional (awaiting sign-off), completed 2026-09-20.")]
    [InlineData(AssessorTrainingStatus.Trained, "2018-10-20", "Training saved for Fatma Khumalo: Trained, completed 2018-10-20.")]
    [InlineData(AssessorTrainingStatus.Trained, null, "Training saved for Fatma Khumalo: Trained, no date.")]
    [InlineData(AssessorTrainingStatus.Provisional, null, "Training saved for Fatma Khumalo: Provisional (awaiting sign-off), no date.")]
    [InlineData(AssessorTrainingStatus.InTraining, null, "Training saved for Fatma Khumalo: In training.")]
    [InlineData(AssessorTrainingStatus.NotStarted, null, "Training saved for Fatma Khumalo: Not started.")]
    public void TheSavedWords_SayTheStatus_AndTheDateOnlyForAFinishedOne(AssessorTrainingStatus status, string? date, string expected)
        => AssessorTrainingWords.Saved("Fatma Khumalo", status, date is null ? null : DateOnly.Parse(date))
            .Should().Be(expected);

    [Fact]
    public void TheValidator_SaysTheSectionsWords_NeverAPropertysName()
    {
        var result = new CreateOrUpdateAssessorProfileCommandValidator().Validate(
            new CreateOrUpdateAssessorProfileCommand("zulu", (AssessorTrainingStatus)99, null, "  ", TestPrincipals.Administrator()));

        result.Errors.Select(error => error.ErrorMessage).Should().BeEquivalentTo(
            AssessorTrainingWords.ChooseStatus, AssessorTrainingWords.TypeQualifications);
    }

    [Fact]
    public async Task ASaveForSomeoneWithoutAssessor_OrWithNoInstitution_IsRefusedByReason_AndSavesNothing()
    {
        await using var db = CreateDb();
        var users = new Users(
            Person("patel", "Mohammed", "Patel", Kgk, WombatRoles.CommitteeMember),
            Person("nowhere", "Ned", "Nobody", null, WombatRoles.Assessor));
        var handler = new CreateOrUpdateAssessorProfileCommandHandler(db, users);

        var notOne = () => handler.Handle(Save("patel"), CancellationToken.None);
        var noInstitution = () => handler.Handle(Save("nowhere"), CancellationToken.None);

        (await notOne.Should().ThrowAsync<AssessorTrainingRefusedException>()).Which.Reason.Should().Be(AssessorTrainingRefusal.NotAnAssessor);
        (await noInstitution.Should().ThrowAsync<AssessorTrainingRefusedException>()).Which.Reason.Should().Be(AssessorTrainingRefusal.NoInstitution);
        (await db.Set<AssessorProfile>().CountAsync()).Should().Be(0);
    }

    // ─── fixtures ────────────────────────────────────────────────────────────

    private static CreateOrUpdateAssessorProfileCommand Save(string userId)
        => new(userId, AssessorTrainingStatus.Trained, null, "MBChB", TestPrincipals.Administrator());

    private static Task<AssessorListDto> List(ApplicationDbContext db, Users users, ClaimsPrincipal principal)
        => new ListAssessorsQueryHandler(db, users).Handle(new ListAssessorsQuery(principal), CancellationToken.None);

    private static AssessorProfile Profile(int id, string userId, AssessorTrainingStatus status, DateOnly? completedOn) => new()
    {
        Id = id,
        UserId = userId,
        InstitutionId = Kgk,
        Qualifications = "MBChB",
        TrainingStatus = status,
        TrainingCompletedOn = completedOn
    };

    private static UserIdentityDetails Person(string id, string first, string last, int? institutionId, params string[] roles)
        => Person(id, first, last, institutionId, [], [], roles);

    private static UserIdentityDetails Person(
        string id, string first, string last, int? institutionId, int[] specialityIds, int[] subSpecialityIds, params string[] roles)
        => new(id, $"{id}@kgk.example", first, last, institutionId, specialityIds, subSpecialityIds, roles);

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital" },
            new Institution { Id = Other, Name = "Other Hospital" });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = PaediatricsSub, SpecialityId = Paediatrics, Name = "Paediatrics" });
        db.SaveChanges();
        return db;
    }

    /// <summary>A directory of the given people, counting its reads (a Trainee-holding admin's refusal comes first).</summary>
    private sealed class Users : IUserAdministrationService
    {
        private readonly Dictionary<string, UserIdentityDetails> _users;

        public Users(params UserIdentityDetails[] users)
        {
            _users = users.ToDictionary(user => user.UserId, StringComparer.Ordinal);
        }

        public int Reads { get; private set; }

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            Reads++;
            return Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);
        }

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            Reads++;
            return Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.Where(user => user.Roles.Contains(role)).ToArray());
        }

        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
        {
            Reads++;
            return Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.ToArray());
        }

        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("M1: no training read or save writes a scope");

        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
