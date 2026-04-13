using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
            db, Options.Create(new DashboardThresholds()));
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
            db, Options.Create(new DashboardThresholds { CoordinatorStallDays = 7 }));
        var principal = CreatePrincipal("coord-1", institutionId: 1);

        var result = await handler.Handle(
            new GetCoordinatorDashboardSummaryQuery(principal), CancellationToken.None);

        result.StalledRequests.Should().HaveCount(1);
        result.StalledRequests[0].ActivityTypeName.Should().Be("Mini-CEX");
    }

    [Fact]
    public async Task WithExpiringInvitation_ReturnsIt()
    {
        await using var db = CreateDb();
        SeedExpiringInvitation(db);
        var handler = new GetCoordinatorDashboardSummaryQueryHandler(
            db, Options.Create(new DashboardThresholds()));
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
            CreatedByUserId = "trainee-1", CurrentState = "requested",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-10), UpdatedOn = DateTime.UtcNow
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
}
