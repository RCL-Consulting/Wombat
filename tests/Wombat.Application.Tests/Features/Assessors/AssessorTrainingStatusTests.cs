using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Assessors;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Assessors;

public sealed class AssessorTrainingStatusTests
{
    [Fact]
    public async Task CreateOrUpdate_PersistsTrainingCompletedOn()
    {
        await using var db = CreateDb();
        Seed(db);

        var users = new StubUserAdministrationService(new UserIdentityDetails(
            UserId: "assessor-1",
            Email: "a@example.org",
            FirstName: "Al",
            LastName: "Assessor",
            InstitutionId: 1,
            SpecialityIds: Array.Empty<int>(),
            SubSpecialityIds: Array.Empty<int>(),
            Roles: new[] { WombatRoles.Assessor }));

        var handler = new CreateOrUpdateAssessorProfileCommandHandler(db, users);
        var completedOn = new DateOnly(2026, 2, 14);

        var dto = await handler.Handle(
            new CreateOrUpdateAssessorProfileCommand(
                "assessor-1",
                TrainingStatus: AssessorTrainingStatus.Trained,
                TrainingCompletedOn: completedOn,
                Qualifications: "MBChB, FCP(SA)",
                Principal: TestPrincipals.Administrator()),
            CancellationToken.None);

        dto.TrainingStatus.Should().Be(AssessorTrainingStatus.Trained);
        dto.TrainingCompletedOn.Should().Be(completedOn);
        var stored = await db.Set<AssessorProfile>().SingleAsync(p => p.UserId == "assessor-1");
        stored.TrainingStatus.Should().Be(AssessorTrainingStatus.Trained);
        stored.TrainingCompletedOn.Should().Be(completedOn);
    }

    [Fact]
    public async Task CreateOrUpdate_InTraining_ClearsCompletedDate()
    {
        await using var db = CreateDb();
        Seed(db);

        var users = new StubUserAdministrationService(new UserIdentityDetails(
            UserId: "assessor-1",
            Email: "a@example.org",
            FirstName: "Al",
            LastName: "Assessor",
            InstitutionId: 1,
            SpecialityIds: Array.Empty<int>(),
            SubSpecialityIds: Array.Empty<int>(),
            Roles: new[] { WombatRoles.Assessor }));

        var handler = new CreateOrUpdateAssessorProfileCommandHandler(db, users);

        // A date supplied alongside a non-completed status is dropped by the handler.
        var dto = await handler.Handle(
            new CreateOrUpdateAssessorProfileCommand(
                "assessor-1",
                TrainingStatus: AssessorTrainingStatus.InTraining,
                TrainingCompletedOn: new DateOnly(2026, 2, 14),
                Qualifications: "MBChB",
                Principal: TestPrincipals.Administrator()),
            CancellationToken.None);

        dto.TrainingStatus.Should().Be(AssessorTrainingStatus.InTraining);
        dto.TrainingCompletedOn.Should().BeNull();
    }

    [Fact]
    public async Task TheTrainingRead_ExposesTrainingCompletedOn()
    {
        await using var db = CreateDb();
        Seed(db);
        db.Set<AssessorProfile>().Add(new AssessorProfile
        {
            Id = 5,
            UserId = "assessor-2",
            InstitutionId = 1,
            Qualifications = "MBChB",
            TrainingCompletedOn = new DateOnly(2025, 9, 1)
        });
        await db.SaveChangesAsync();

        var users = new StubUserAdministrationService(new UserIdentityDetails(
            "assessor-2", "b@example.org", "Bea", "Assessor", 1,
            Array.Empty<int>(), Array.Empty<int>(), new[] { WombatRoles.Assessor }));

        // T376 (lane T): the user page's training section reads the record by the person, not by the profile's id.
        var handler = new GetAssessorTrainingQueryHandler(db, users);
        var dto = await handler.Handle(new GetAssessorTrainingQuery("assessor-2", TestPrincipals.Administrator()), CancellationToken.None);

        dto!.TrainingCompletedOn.Should().Be(new DateOnly(2025, 9, 1));
    }

    [Fact]
    public async Task List_ReturnsTrainingStatus_ForMixedPopulation()
    {
        await using var db = CreateDb();
        Seed(db);
        db.Set<AssessorProfile>().AddRange(
            new AssessorProfile { Id = 1, UserId = "a1", InstitutionId = 1, Qualifications = "MBChB", TrainingCompletedOn = new DateOnly(2025, 5, 5) },
            new AssessorProfile { Id = 2, UserId = "a2", InstitutionId = 1, Qualifications = "MBChB", TrainingCompletedOn = null });
        await db.SaveChangesAsync();

        var users = new StubUserAdministrationService(
            new UserIdentityDetails("a1", "a1@x", "Alpha", "One", 1, Array.Empty<int>(), Array.Empty<int>(), new[] { WombatRoles.Assessor }),
            new UserIdentityDetails("a2", "a2@x", "Beta", "Two", 1, Array.Empty<int>(), Array.Empty<int>(), new[] { WombatRoles.Assessor }));

        // T376 (lane T, Q2): the list is of every Assessor holder, each joined to their record.
        var handler = new ListAssessorsQueryHandler(db, users);
        var dtos = (await handler.Handle(new ListAssessorsQuery(TestPrincipals.Administrator()), CancellationToken.None)).Rows;

        dtos.Should().HaveCount(2);
        dtos.Single(d => d.UserId == "a1").TrainingCompletedOn.Should().Be(new DateOnly(2025, 5, 5));
        dtos.Single(d => d.UserId == "a2").TrainingCompletedOn.Should().BeNull();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void Seed(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "Test Hospital" });
        db.SaveChanges();
    }

    private sealed class StubUserAdministrationService : IUserAdministrationService
    {
        private readonly Dictionary<string, UserIdentityDetails> _users;

        public StubUserAdministrationService(params UserIdentityDetails[] users)
        {
            _users = users.ToDictionary(user => user.UserId, StringComparer.Ordinal);
        }

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.Where(user => user.Roles.Contains(role)).ToArray());

        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

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
