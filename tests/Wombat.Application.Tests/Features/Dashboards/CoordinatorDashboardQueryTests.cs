using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Domain.Activities;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

public sealed class CoordinatorDashboardQueryTests
{
    [Fact]
    public async Task EmptyDatabase_ReturnsEmptyLists()
    {
        await using var db = CreateDb();
        var handler = new GetCoordinatorDashboardSummaryQueryHandler(
            db, Options.Create(new DashboardThresholds()), new StubUserAdmin());
        var principal = CreatePrincipal("coord-1", institutionId: 1);

        var result = await handler.Handle(
            new GetCoordinatorDashboardSummaryQuery(principal), CancellationToken.None);

        result.StalledRequests.Should().BeEmpty();
        result.ExpiringInvitations.Should().BeEmpty();
    }

    [Fact]
    public async Task WithStalledActivities_ReturnsThem()
    {
        await using var db = CreateDb();
        SeedStalledData(db);
        var handler = new GetCoordinatorDashboardSummaryQueryHandler(
            db, Options.Create(new DashboardThresholds { CoordinatorStallDays = 7 }),
            new StubUserAdmin(new UserIdentityDetails(
                "trainee-1", "trainee1@example.com", "Test", "Trainee", 1,
                System.Array.Empty<int>(), System.Array.Empty<int>(), System.Array.Empty<string>())));
        var principal = CreatePrincipal("coord-1", institutionId: 1);

        var result = await handler.Handle(
            new GetCoordinatorDashboardSummaryQuery(principal), CancellationToken.None);

        result.StalledRequests.Should().HaveCount(1);
        result.StalledRequests[0].ActivityTypeName.Should().Be("Mini-CEX");
        // T094-followup: the panel shows the trainee's name, not the raw UserId GUID.
        result.StalledRequests[0].SubjectName.Should().Be("Test Trainee");
    }

    [Fact]
    public async Task RecentlySubmittedOrNonSubmitted_AreNotStalled()
    {
        await using var db = CreateDb();
        var activityType = new ActivityType
        {
            Id = 1, Key = "mini_cex", Name = "Mini-CEX", Scope = ActivityScope.Global
        };
        db.ActivityTypes.Add(activityType);

        // Submitted but recent (UpdatedOn just now) — not stalled.
        db.Activities.Add(new Activity
        {
            Id = 1, ActivityTypeId = 1, SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1", CurrentState = "submitted",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-20), UpdatedOn = DateTime.UtcNow
        });
        // Old but still a draft (never submitted) — not stalled.
        db.Activities.Add(new Activity
        {
            Id = 2, ActivityTypeId = 1, SubjectUserId = "trainee-2",
            CreatedByUserId = "trainee-2", CurrentState = "draft",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-20), UpdatedOn = DateTime.UtcNow.AddDays(-20)
        });
        // Old and completed (terminal) — not stalled.
        db.Activities.Add(new Activity
        {
            Id = 3, ActivityTypeId = 1, SubjectUserId = "trainee-3",
            CreatedByUserId = "trainee-3", CurrentState = "completed",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-20), UpdatedOn = DateTime.UtcNow.AddDays(-20)
        });
        db.SaveChanges();

        var handler = new GetCoordinatorDashboardSummaryQueryHandler(
            db, Options.Create(new DashboardThresholds { CoordinatorStallDays = 7 }), new StubUserAdmin());
        var principal = CreatePrincipal("coord-1", institutionId: 1);

        var result = await handler.Handle(
            new GetCoordinatorDashboardSummaryQuery(principal), CancellationToken.None);

        result.StalledRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task WithExpiringInvitation_ReturnsIt()
    {
        await using var db = CreateDb();
        SeedExpiringInvitation(db);
        var handler = new GetCoordinatorDashboardSummaryQueryHandler(
            db, Options.Create(new DashboardThresholds()), new StubUserAdmin());
        var principal = CreatePrincipal("coord-1", institutionId: 1);

        var result = await handler.Handle(
            new GetCoordinatorDashboardSummaryQuery(principal), CancellationToken.None);

        result.ExpiringInvitations.Should().HaveCount(1);
        result.ExpiringInvitations[0].Email.Should().Be("test@example.com");
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedStalledData(ApplicationDbContext db)
    {
        var activityType = new ActivityType
        {
            Id = 1, Key = "mini_cex", Name = "Mini-CEX",
            Scope = ActivityScope.Global
        };
        db.ActivityTypes.Add(activityType);

        db.Activities.Add(new Activity
        {
            Id = 1, ActivityTypeId = 1, SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1", CurrentState = "submitted",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-12), UpdatedOn = DateTime.UtcNow.AddDays(-10)
        });

        db.SaveChanges();
    }

    private static void SeedExpiringInvitation(ApplicationDbContext db)
    {
        db.Invitations.Add(new Invitation
        {
            Id = 1, Email = "test@example.com", TokenHash = "hash123",
            TargetRole = "Trainee", InstitutionId = 1,
            IssuedByUserId = "admin-1", IssuedOn = DateTime.UtcNow.AddDays(-7),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        });
        db.SaveChanges();
    }

    private static ClaimsPrincipal CreatePrincipal(
        string userId, int? institutionId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, "Coordinator")
        };
        if (institutionId.HasValue)
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private sealed class StubUserAdmin : IUserAdministrationService
    {
        private readonly Dictionary<string, UserIdentityDetails> _users;

        public StubUserAdmin(params UserIdentityDetails[] users)
            => _users = users.ToDictionary(user => user.UserId, StringComparer.Ordinal);

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
