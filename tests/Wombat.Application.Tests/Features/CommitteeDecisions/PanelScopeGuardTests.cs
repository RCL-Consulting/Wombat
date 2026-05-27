using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T063 scope guards on the Decision Panel surface. InstitutionalAdmin can only
/// administer panels in their institution (resolved via the panel's Institution
/// directly for Institution-scoped panels, or via the Speciality's InstitutionId
/// for Speciality-scoped panels). Coordinator still cannot administer panels at all.
/// </summary>
public sealed class PanelScopeGuardTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int SpecialityInB = 5;

    [Fact]
    public async Task Create_InstitutionalAdmin_CanCreatePanelInOwnInstitution()
    {
        await using var db = SeededDb();
        var handler = new CreateDecisionPanelCommandHandler(db);

        var result = await handler.Handle(
            new CreateDecisionPanelCommand(
                Name: "KGK ARCP",
                Scope: DecisionPanelScope.Institution,
                InstitutionId: InstitutionA,
                SpecialityId: null,
                Members: new[] { new DecisionPanelMemberInput("u-chair", DecisionPanelMemberRole.Chair) },
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.Name.Should().Be("KGK ARCP");
        result.InstitutionId.Should().Be(InstitutionA);
    }

    [Fact]
    public async Task Create_InstitutionalAdmin_CannotCreatePanelInOtherInstitution()
    {
        await using var db = SeededDb();
        var handler = new CreateDecisionPanelCommandHandler(db);

        var act = () => handler.Handle(
            new CreateDecisionPanelCommand(
                Name: "Cross-Institution",
                Scope: DecisionPanelScope.Institution,
                InstitutionId: InstitutionB,
                SpecialityId: null,
                Members: new[] { new DecisionPanelMemberInput("u-chair", DecisionPanelMemberRole.Chair) },
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*your institution*");
    }

    [Fact]
    public async Task Create_InstitutionalAdmin_CannotCreateSpecialityPanelOutsideInstitution()
    {
        await using var db = SeededDb();
        var handler = new CreateDecisionPanelCommandHandler(db);

        // SpecialityInB belongs to InstitutionB. An InstitutionA admin cannot create
        // a speciality-scoped panel on it.
        var act = () => handler.Handle(
            new CreateDecisionPanelCommand(
                Name: "Outside",
                Scope: DecisionPanelScope.Speciality,
                InstitutionId: null,
                SpecialityId: SpecialityInB,
                Members: new[] { new DecisionPanelMemberInput("u-chair", DecisionPanelMemberRole.Chair) },
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Update_InstitutionalAdmin_CannotUpdatePanelInOtherInstitution()
    {
        await using var db = SeededDb();
        var panel = new DecisionPanel
        {
            Name = "B-panel",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionB,
            CreatedOn = DateTime.UtcNow,
            Members = new[] { new DecisionPanelMember { UserId = "chair-b", Role = DecisionPanelMemberRole.Chair } }
        };
        db.Set<DecisionPanel>().Add(panel);
        await db.SaveChangesAsync();

        var handler = new UpdateDecisionPanelCommandHandler(db);
        var act = () => handler.Handle(
            new UpdateDecisionPanelCommand(
                PanelId: panel.Id,
                Members: new[] { new DecisionPanelMemberInput("chair-b", DecisionPanelMemberRole.Chair) },
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetById_InstitutionalAdmin_ReturnsNullForOutOfScopePanel()
    {
        await using var db = SeededDb();
        var panel = new DecisionPanel
        {
            Name = "B-panel",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionB,
            CreatedOn = DateTime.UtcNow
        };
        db.Set<DecisionPanel>().Add(panel);
        await db.SaveChangesAsync();

        var handler = new GetDecisionPanelByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetDecisionPanelByIdQuery(panel.Id, TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task List_InstitutionalAdmin_OnlySeesOwnInstitutionPanels()
    {
        await using var db = SeededDb();
        db.Set<DecisionPanel>().AddRange(
            new DecisionPanel { Name = "A-panel", Scope = DecisionPanelScope.Institution, InstitutionId = InstitutionA, CreatedOn = DateTime.UtcNow },
            new DecisionPanel { Name = "B-panel", Scope = DecisionPanelScope.Institution, InstitutionId = InstitutionB, CreatedOn = DateTime.UtcNow },
            new DecisionPanel { Name = "Spec-B-panel", Scope = DecisionPanelScope.Speciality, SpecialityId = SpecialityInB, CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var handler = new ListDecisionPanelsQueryHandler(db);
        var result = await handler.Handle(
            new ListDecisionPanelsQuery(TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.Select(panel => panel.Name).Should().BeEquivalentTo("A-panel");
    }

    [Fact]
    public async Task Create_Coordinator_IsStillRejectedByPolicy()
    {
        await using var db = SeededDb();
        var handler = new CreateDecisionPanelCommandHandler(db);

        var identity = new System.Security.Claims.ClaimsIdentity("test");
        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "coord-1"));
        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, WombatRoles.Coordinator));
        var coordinator = new System.Security.Claims.ClaimsPrincipal(identity);

        var act = () => handler.Handle(
            new CreateDecisionPanelCommand(
                Name: "Should reject",
                Scope: DecisionPanelScope.Institution,
                InstitutionId: InstitutionA,
                SpecialityId: null,
                Members: new[] { new DecisionPanelMemberInput("u-chair", DecisionPanelMemberRole.Chair) },
                Principal: coordinator),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*manage committee panels*");
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Set<Institution>().AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = SpecialityInB, InstitutionId = InstitutionB, Name = "SpecB", IsActive = true });
        db.SaveChanges();
        return db;
    }
}
