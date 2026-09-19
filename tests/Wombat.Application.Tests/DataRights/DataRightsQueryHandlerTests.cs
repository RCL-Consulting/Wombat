using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.DataRights;
using Wombat.Infrastructure.Persistence;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;

namespace Wombat.Application.Tests.DataRights;

public sealed class DataRightsQueryHandlerTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ClaimsPrincipal UserPrincipal(string userId = "user-1", string name = "Test User", params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, name)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public async Task GetMyRequests_ReturnsOnlyOwnRequests()
    {
        await using var db = CreateDb();

        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("user-1", "User One", DataRightsRequestType.Access, "My data", DateTime.UtcNow));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("user-2", "User Two", DataRightsRequestType.Export, "My data too", DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new GetMyDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new GetMyDataRightsRequestsQuery(UserPrincipal("user-1")),
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].RequesterDisplayName.Should().Be("User One");
    }

    [Fact]
    public async Task ListRequests_FilterByType_ReturnsMatchingOnly()
    {
        await using var db = CreateDb();

        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("user-1", "User One", DataRightsRequestType.Access, "Access", DateTime.UtcNow));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("user-2", "User Two", DataRightsRequestType.Erasure, "Erase", DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new ListDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new ListDataRightsRequestsQuery(
                Type: DataRightsRequestType.Erasure,
                Status: null,
                RequesterUserId: null,
                Principal: AdministratorPrincipal()),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Type.Should().Be(DataRightsRequestType.Erasure);
    }

    [Fact]
    public async Task ListRequests_Pagination_ReturnsCorrectPage()
    {
        await using var db = CreateDb();

        for (var i = 0; i < 10; i++)
        {
            db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
                $"user-{i}", $"User {i}", DataRightsRequestType.Access, $"Reason {i}",
                DateTime.UtcNow.AddMinutes(-i)));
        }
        await db.SaveChangesAsync();

        var handler = new ListDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new ListDataRightsRequestsQuery(
                Type: null,
                Status: null,
                RequesterUserId: null,
                Principal: AdministratorPrincipal(),
                Page: 2,
                PageSize: 3),
            CancellationToken.None);

        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(2);
    }

    [Fact]
    public async Task GetById_AsOwner_ReturnsRequest()
    {
        await using var db = CreateDb();

        var entity = DataRightsRequest.Create("user-1", "User One", DataRightsRequestType.Access, "My data", DateTime.UtcNow);
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();

        var handler = new GetDataRightsRequestByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, UserPrincipal("user-1")),
            CancellationToken.None);

        result.RequesterUserId.Should().Be("user-1");
        result.Type.Should().Be(DataRightsRequestType.Access);
    }

    [Fact]
    public async Task GetById_AsOtherNonAdmin_ThrowsUnauthorized()
    {
        await using var db = CreateDb();

        var entity = DataRightsRequest.Create("user-1", "User One", DataRightsRequestType.Access, "My data", DateTime.UtcNow);
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();

        var handler = new GetDataRightsRequestByIdQueryHandler(db);
        var act = () => handler.Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, UserPrincipal("user-2", "Other User")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetById_AsAdministrator_ReturnsAnyRequest()
    {
        await using var db = CreateDb();

        var entity = DataRightsRequest.Create("user-1", "User One", DataRightsRequestType.Access, "My data", DateTime.UtcNow);
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();

        var handler = new GetDataRightsRequestByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, UserPrincipal("admin-1", "Admin", "Administrator")),
            CancellationToken.None);

        result.RequesterUserId.Should().Be("user-1");
    }

    // ─── T112: institution scoping ───────────────────────────────────────────

    /// <summary>
    /// Until T112 every handler here gated on role alone, so a Coordinator at one hospital listed,
    /// opened, downloaded and could <b>erase</b> another hospital's requests. A subject access report
    /// is everything the product holds about one person, so this was the widest disclosure in the
    /// product.
    /// </summary>
    [Fact]
    public async Task ListRequests_ACoordinatorSeesOnlyTheirOwnInstitution()
    {
        await using var db = CreateDb();

        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
            "user-here", "Here", DataRightsRequestType.Access, "Mine", DateTime.UtcNow, institutionId: 1));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
            "user-there", "There", DataRightsRequestType.Access, "Theirs", DateTime.UtcNow, institutionId: 2));
        await db.SaveChangesAsync();

        var handler = new ListDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new ListDataRightsRequestsQuery(null, null, null, CoordinatorPrincipal(institutionId: 1)),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].RequesterDisplayName.Should().Be("Here");
        result.TotalCount.Should().Be(1, "the count must be scoped too, or it leaks the other institution's volume");
    }

    [Fact]
    public async Task ListRequests_AnAdministratorSeesEveryInstitution()
    {
        await using var db = CreateDb();

        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
            "user-here", "Here", DataRightsRequestType.Access, "Mine", DateTime.UtcNow, institutionId: 1));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
            "user-there", "There", DataRightsRequestType.Access, "Theirs", DateTime.UtcNow, institutionId: 2));
        await db.SaveChangesAsync();

        var handler = new ListDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new ListDataRightsRequestsQuery(null, null, null, AdministratorPrincipal()),
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// An unstamped request — a requester carrying no institution claim — is Administrator-only.
    /// Erasure is irreversible, so an unplaceable request withholds the power rather than spreading
    /// it to every Coordinator.
    /// </summary>
    [Fact]
    public async Task ListRequests_AnUnstampedRequestIsInvisibleToAScopedCoordinator()
    {
        await using var db = CreateDb();

        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
            "user-nowhere", "Nowhere", DataRightsRequestType.Erasure, "No institution", DateTime.UtcNow));
        await db.SaveChangesAsync();

        var handler = new ListDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new ListDataRightsRequestsQuery(null, null, null, CoordinatorPrincipal(institutionId: 1)),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    /// <summary>
    /// A Coordinator with no institution claim must see nothing, not everything. The scoping filter
    /// has to fail closed when the claim is absent.
    /// </summary>
    [Fact]
    public async Task ListRequests_ACoordinatorWithNoInstitutionClaimSeesNothing()
    {
        await using var db = CreateDb();

        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create(
            "user-here", "Here", DataRightsRequestType.Access, "Mine", DateTime.UtcNow, institutionId: 1));
        await db.SaveChangesAsync();

        var handler = new ListDataRightsRequestsQueryHandler(db);
        var result = await handler.Handle(
            new ListDataRightsRequestsQuery(null, null, null, CoordinatorPrincipal(institutionId: null)),
            CancellationToken.None);

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ListRequests_ACallerWithNoReviewRoleIsRefused()
    {
        await using var db = CreateDb();
        var handler = new ListDataRightsRequestsQueryHandler(db);

        var act = () => handler.Handle(
            new ListDataRightsRequestsQuery(null, null, null, TraineePrincipal()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static ClaimsPrincipal AdministratorPrincipal()
        => Build("admin-1", roles: [WombatRoles.Administrator]);

    private static ClaimsPrincipal CoordinatorPrincipal(int? institutionId)
        => Build("coord-1", institutionId, [WombatRoles.Coordinator]);

    private static ClaimsPrincipal TraineePrincipal()
        => Build("trainee-1", 1, [WombatRoles.Trainee]);

    private static ClaimsPrincipal Build(string userId, int? institutionId = null, string[]? roles = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        foreach (var role in roles ?? [])
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

}
