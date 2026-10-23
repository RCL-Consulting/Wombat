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
/// The assessor-profile save records training only (T376, flow 12, M1, F4): it writes no scope, so it cannot move anyone or
/// strip a held speciality, and the profile's institution is the person's own. A registrar who also administers may not
/// make it (a trainee first, T289), nor anyone for their own account, and an id outside the caller's institution reads as
/// one that names nobody. Every refusal leaves the profile table and the user store untouched.
/// </summary>
public sealed class AssessorProfileRefusalTests
{
    private const int HomeInstitution = 1;
    private const int OtherInstitution = 2;
    private const string Assessor = "assessor-1";
    private const string OtherAssessor = "assessor-2";

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public async Task ATraineeWhoAdministers_SavesNoProfile_AndIsRefusedBeforeTheLookup(string role)
    {
        await using var db = CreateDb();
        var users = Users();
        var principal = TestPrincipals.InRoles([role, WombatRoles.Trainee], "registrar-admin", HomeInstitution);

        var act = () => Handler(db, users).Handle(Command(Assessor, principal), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        users.Lookups.Should().BeEmpty("the refusal is asked of the caller, before the user is looked up");
        users.ScopeUpdates.Should().BeEmpty();
        (await db.Set<AssessorProfile>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CannotRecordAnotherInstitutionsAssessor_AndIsToldNothingAboutTheId()
    {
        await using var db = CreateDb();
        var users = Users();
        var principal = TestPrincipals.InstitutionalAdmin(HomeInstitution);

        var other = () => Handler(db, users).Handle(Command(OtherAssessor, principal), CancellationToken.None);
        var nobody = () => Handler(db, users).Handle(Command("no-such-user", principal), CancellationToken.None);

        var otherMessage = (await other.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message;
        var nobodyMessage = (await nobody.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message;
        otherMessage.Should().Be(nobodyMessage, "an id at another institution reads as one that names nobody");
        users.ScopeUpdates.Should().BeEmpty();
        (await db.Set<AssessorProfile>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task NobodyRecordsTheirOwnTraining()
    {
        await using var db = CreateDb();
        var users = Users();
        var principal = TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin], Assessor, HomeInstitution);

        var act = () => Handler(db, users).Handle(Command(Assessor, principal), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be(CreateOrUpdateAssessorProfileCommandHandler.OwnTrainingRefused);
        (await db.Set<AssessorProfile>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task TheProfileIsRecordedAtThePersonsOwnInstitution_ForTheAdministratorToo()
    {
        await using var db = CreateDb();
        var users = Users();

        var dto = await Handler(db, users)
            .Handle(Command(OtherAssessor, TestPrincipals.Administrator()), CancellationToken.None);

        dto.InstitutionId.Should().Be(OtherInstitution);
        (await db.Set<AssessorProfile>().SingleAsync()).InstitutionId.Should().Be(OtherInstitution);
        users.ScopeUpdates.Should().BeEmpty("the save records training and moves nobody (M1)");
    }

    [Fact]
    public async Task AnExistingProfile_FollowsThePersonsInstitution_AtTheNextSave()
    {
        await using var db = CreateDb();
        db.Set<AssessorProfile>().Add(new AssessorProfile { Id = 7, UserId = OtherAssessor, InstitutionId = HomeInstitution, Qualifications = "MBChB" });
        await db.SaveChangesAsync();
        var users = Users();

        await Handler(db, users).Handle(Command(OtherAssessor, TestPrincipals.Administrator()), CancellationToken.None);

        (await db.Set<AssessorProfile>().SingleAsync()).InstitutionId.Should().Be(OtherInstitution, "F4: written from the person at save");
        users.ScopeUpdates.Should().BeEmpty();
    }

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public async Task TheControl_AnAdminWithoutTrainee_SavesTraining_AndWritesNoScope(string role)
    {
        await using var db = CreateDb();
        var users = Users();
        var principal = role == WombatRoles.Administrator
            ? TestPrincipals.Administrator()
            : TestPrincipals.InstitutionalAdmin(HomeInstitution);

        var dto = await Handler(db, users).Handle(Command(Assessor, principal), CancellationToken.None);

        dto.InstitutionId.Should().Be(HomeInstitution);
        dto.TrainingStatus.Should().Be(AssessorTrainingStatus.Trained);
        users.ScopeUpdates.Should().BeEmpty("M1: no UpdateScopeAsync, so the scope and the security stamp stay as they are");
    }

    private static CreateOrUpdateAssessorProfileCommandHandler Handler(ApplicationDbContext db, RecordingUsers users)
        => new(db, users);

    private static CreateOrUpdateAssessorProfileCommand Command(
        string userId, System.Security.Claims.ClaimsPrincipal principal)
        => new(userId, AssessorTrainingStatus.Trained, new DateOnly(2026, 2, 14), "MBChB", principal);

    private static RecordingUsers Users() => new(
        new UserIdentityDetails(Assessor, "a1@kgk.example", "Al", "One", HomeInstitution,
            Array.Empty<int>(), Array.Empty<int>(), [WombatRoles.Assessor]),
        new UserIdentityDetails(OtherAssessor, "a2@other.example", "Bea", "Two", OtherInstitution,
            Array.Empty<int>(), Array.Empty<int>(), [WombatRoles.Assessor]));

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        db.Institutions.AddRange(
            new Institution { Id = HomeInstitution, Name = "Home Hospital" },
            new Institution { Id = OtherInstitution, Name = "Other Hospital" });
        db.SaveChanges();
        return db;
    }

    private sealed class RecordingUsers : IUserAdministrationService
    {
        private readonly Dictionary<string, UserIdentityDetails> _users;

        public RecordingUsers(params UserIdentityDetails[] users)
        {
            _users = users.ToDictionary(user => user.UserId, StringComparer.Ordinal);
        }

        public List<string> Lookups { get; } = [];

        public List<(string UserId, int InstitutionId)> ScopeUpdates { get; } = [];

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            Lookups.Add(userId);
            return Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);
        }

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.Where(user => user.Roles.Contains(role)).ToArray());

        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
        {
            ScopeUpdates.Add((userId, institutionId));
            return Task.CompletedTask;
        }

        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.ToArray());

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
