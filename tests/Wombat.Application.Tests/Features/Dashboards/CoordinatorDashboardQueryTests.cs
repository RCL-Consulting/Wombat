using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Scheduling;
using Wombat.Application.Tests.Scheduling;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The Coordinator's "Stalled requests": the activities awaiting a reviewer, read from each activity's PINNED workflow,
/// that nobody has moved for <see cref="DashboardThresholds.CoordinatorStallDays" /> and that the caller may read (T101),
/// oldest first (T297).
/// </summary>
/// <remarks>
/// Until T297 the card read the literal state <c>submitted</c>, which T074 chose for the old draft-to-submitted shape.
/// Every rated CPSA instrument waits for its assessor in <c>requested</c>, so no stalled Mini-CEX ever reached the card,
/// though the assessor nudge mailed about it the same morning (Step 3.30).
/// </remarks>
public sealed class CoordinatorDashboardQueryTests
{
    private const int InstitutionId = 1;
    private const int MiniCexTypeId = 1;
    private const int PortfolioReviewTypeId = 2;

    [Fact]
    public async Task EmptyDatabase_ReturnsEmptyLists()
    {
        await using var db = CreateDb();

        var result = await Handle(db);

        result.StalledRequests.Should().BeEmpty();
        result.ExpiringInvitations.Should().BeEmpty();
    }

    /// <summary>
    /// The Verification's case: a Mini-CEX in <c>requested</c> untouched for eight days is listed; a draft and a declined
    /// request as old are not, since a draft is its author's to move and a declined request has no move left.
    /// </summary>
    [Fact]
    public async Task ARequestedCpsaMiniCex_UntouchedForEightDays_IsListed_AndADraftAndADeclinedRequestAreNot()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddActivity(db, 1, MiniCexTypeId, "requested", "trainee-1", daysAgo: 8);
        AddActivity(db, 2, MiniCexTypeId, "draft", "trainee-2", daysAgo: 20);
        AddActivity(db, 3, MiniCexTypeId, "declined", "trainee-3", daysAgo: 20);
        AddActivity(db, 4, MiniCexTypeId, "completed", "trainee-4", daysAgo: 20);
        await db.SaveChangesAsync();

        var result = await Handle(db, users: new StubUserAdmin(User("trainee-1", "Nomsa", "Mahlangu")));

        result.StalledRequests.Select(item => (item.ActivityId, item.ActivityTypeName, item.SubjectName))
            .Should().Equal((1, "Mini-CEX (Paediatrics)", "Nomsa Mahlangu"));
    }

    [Fact]
    public async Task EveryStalledRequest_IsListedOldestFirst_WhateverStateItWaitsIn()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddActivity(db, 1, MiniCexTypeId, "requested", "trainee-1", daysAgo: 8);
        AddActivity(db, 2, PortfolioReviewTypeId, "submitted", "trainee-2", daysAgo: 12);
        await db.SaveChangesAsync();

        var result = await Handle(db, users: new StubUserAdmin(
            User("trainee-1", "Nomsa", "Mahlangu"), User("trainee-2", "Pieter", "du Plessis")));

        result.StalledRequests.Select(item => (item.ActivityId, item.SubjectName))
            .Should().Equal((2, "Pieter du Plessis"), (1, "Nomsa Mahlangu"));
        result.StalledRequests[0].LastMovedOn.Should().BeCloseTo(DateTime.UtcNow.AddDays(-12).AddHours(-1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ARequestMovedWithinTheStallDays_IsNotStalled()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddActivity(db, 1, MiniCexTypeId, "requested", "trainee-1", daysAgo: 6);
        AddActivity(db, 2, PortfolioReviewTypeId, "submitted", "trainee-2", daysAgo: 0);
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.StalledRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task AnotherInstitutionsStalledRequest_IsNotListed()
    {
        // The stall panel used to be unscoped while the invitation panel beside it was scoped, so a coordinator saw every
        // institution's backlog, ids and subject names included (T101).
        await using var db = CreateDb();
        SeedTypes(db);
        AddActivity(db, 1, PortfolioReviewTypeId, "submitted", "trainee-1", daysAgo: 10);
        AddActivity(db, 2, PortfolioReviewTypeId, "submitted", "trainee-9", daysAgo: 11, institutionId: 2);
        await db.SaveChangesAsync();

        var result = await Handle(db, users: new StubUserAdmin(User("trainee-1", "Test", "Trainee")));

        result.StalledRequests.Should().ContainSingle();
        // T094-followup: the panel shows the trainee's name, not the raw UserId GUID.
        result.StalledRequests[0].SubjectName.Should().Be("Test Trainee");
    }

    /// <summary>
    /// The shared predicate (T297): a request the assessor nudge writes about is on the card once it has waited the stall
    /// days. Both read "awaiting a reviewer" through <c>ActivityWaiting</c>; the nudge adds only its need for a named
    /// nominee to mail.
    /// </summary>
    [Fact]
    public async Task AnActivityTheAssessorNudgeMailsAbout_IsOnTheCard()
    {
        var emailSender = new RecordingEmailSender();
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IEmailSender>(_ => emailSender);
        services.AddSingleton<ScheduledJobMailTally>();
        await using var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SeedTypes(db);
            AddActivity(db, 21, MiniCexTypeId, "requested", "trainee-1", daysAgo: 8, assessorId: "assessor-zulu");
            AddActivity(db, 10, PortfolioReviewTypeId, "submitted", "trainee-2", daysAgo: 8, assessorId: "assessor-patel");
            NomineeSeed.AddUser(db, "trainee-1", InstitutionId, WombatRoles.Trainee);
            NomineeSeed.AddUser(db, "trainee-2", InstitutionId, WombatRoles.Trainee);
            NomineeSeed.AddUser(db, "assessor-zulu", InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, "assessor-patel", InstitutionId, WombatRoles.Assessor);
            await db.SaveChangesAsync();
        }

        await new AssessorPendingNudgeJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(DateTime.UtcNow, new CapturingLogger()), CancellationToken.None);

        emailSender.Recipients.Should().BeEquivalentTo(["assessor-zulu@test.local", "assessor-patel@test.local"]);
        emailSender.Sent.Single(message => message.To == "assessor-zulu@test.local").TextBody.Should().Contain("Mini-CEX (Paediatrics)");

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var result = await Handle(db, users: new StubUserAdmin());

            result.StalledRequests.Select(item => item.ActivityId).Should().BeEquivalentTo([21, 10]);
        }
    }

    [Fact]
    public async Task WithExpiringInvitation_ReturnsIt()
    {
        await using var db = CreateDb();
        db.Invitations.Add(new Invitation
        {
            Id = 1, Email = "test@example.com", TokenHash = "hash123",
            TargetRole = "Trainee", InstitutionId = InstitutionId,
            IssuedByUserId = "admin-1", IssuedOn = DateTime.UtcNow.AddDays(-7),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
        });
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.ExpiringInvitations.Should().HaveCount(1);
        result.ExpiringInvitations[0].Email.Should().Be("test@example.com");
    }

    private static async Task<CoordinatorDashboardSummaryDto> Handle(ApplicationDbContext db, StubUserAdmin? users = null)
        => await new GetCoordinatorDashboardSummaryQueryHandler(
                db, Options.Create(new DashboardThresholds { CoordinatorStallDays = 7 }), users ?? new StubUserAdmin())
            .Handle(new GetCoordinatorDashboardSummaryQuery(CreatePrincipal("coord-1", InstitutionId)), CancellationToken.None);

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>The shipped Mini-CEX, waiting in <c>requested</c>, and portfolio review, waiting in <c>submitted</c>.</summary>
    private static void SeedTypes(ApplicationDbContext db)
    {
        ShippedSeeds.AddType(db, MiniCexTypeId, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        ShippedSeeds.AddType(db, PortfolioReviewTypeId, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
    }

    private static void AddActivity(
        ApplicationDbContext db,
        int id,
        int typeId,
        string state,
        string subject,
        int daysAgo,
        int institutionId = InstitutionId,
        string assessorId = "assessor-1")
    {
        var updatedOn = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-1);
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
            SubjectUserId = subject, CreatedByUserId = subject, CurrentState = state,
            DataJson = $$"""{ "assessor_user_id": "{{assessorId}}" }""",
            InstitutionId = institutionId,
            CreatedOn = updatedOn.AddDays(-1), UpdatedOn = updatedOn
        });
    }

    private static UserIdentityDetails User(string id, string firstName, string lastName)
        => new(id, $"{id}@example.com", firstName, lastName, InstitutionId, [], [], []);

    private static ClaimsPrincipal CreatePrincipal(string userId, int? institutionId = null)
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
