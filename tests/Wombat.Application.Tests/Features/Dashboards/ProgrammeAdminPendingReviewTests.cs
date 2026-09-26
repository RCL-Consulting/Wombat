using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The SpecialityAdmin's and SubSpecialityAdmin's "Pending reviews": the programme's backlog awaiting a reviewer, read from
/// each activity's PINNED workflow (T297), on the stamps T185 conjoins (speciality or sub-speciality, and the institution).
/// </summary>
/// <remarks>
/// Until T297 the tile counted the literal states <c>submitted</c> and <c>in_review</c>. No seed has <c>in_review</c>, and
/// every rated CPSA instrument waits in <c>requested</c>, so Dr du Plessis's CBD was never pending (Steps 3.53, 3.54).
/// </remarks>
public sealed class ProgrammeAdminPendingReviewTests
{
    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;
    private const int SpecialityId = 5;
    private const int SubSpecialityId = 6;

    private const int CbdTypeId = 1;
    private const int PortfolioReviewTypeId = 2;

    private static readonly DateOnly AsOf = new(2026, 9, 23);

    [Fact]
    public async Task TheSpecialityAdmin_CountsARequestedCpsaWbaInHerProgramme_ButNotOneAtAnotherInstitution()
    {
        await using var db = await SeededAsync();

        var result = await new GetSpecialityAdminDashboardSummaryQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetSpecialityAdminDashboardSummaryQuery(
                TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "mokoena", InstitutionId, specialityId: SpecialityId), AsOf),
            CancellationToken.None);

        result.PendingReviewCount.Should().Be(2, "the requested CBD and the portfolio review awaiting review, at her institution");
    }

    [Fact]
    public async Task TheSubSpecialityAdmin_CountsARequestedCpsaWbaInHisProgramme_ButNotOneAtAnotherInstitution()
    {
        await using var db = await SeededAsync();

        var result = await new GetSubSpecialityAdminDashboardSummaryQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetSubSpecialityAdminDashboardSummaryQuery(
                TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sithole", InstitutionId, subSpecialityId: SubSpecialityId), AsOf),
            CancellationToken.None);

        result.PendingReviewCount.Should().Be(2);
    }

    [Fact]
    public async Task ASingleRequestedCpsaWba_CountsOne()
    {
        await using var db = NewContext();
        ShippedSeeds.AddType(db, CbdTypeId, "cbd_cpsa", "Case-Based Discussion (Paediatrics)");
        Add(db, 1, CbdTypeId, "requested", InstitutionId);
        await db.SaveChangesAsync();

        var result = await new GetSpecialityAdminDashboardSummaryQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetSpecialityAdminDashboardSummaryQuery(
                TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "mokoena", InstitutionId, specialityId: SpecialityId), AsOf),
            CancellationToken.None);

        result.PendingReviewCount.Should().Be(1);
    }

    /// <summary>
    /// In the programme and at the institution: a requested CBD (1) and a portfolio review awaiting review (2), counted; a
    /// draft (3), which only its author can move, a declined request (4), with no move left, and a completed one (5), not.
    /// At another institution: a requested CBD (6), not counted.
    /// </summary>
    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = NewContext();
        ShippedSeeds.AddType(db, CbdTypeId, "cbd_cpsa", "Case-Based Discussion (Paediatrics)");
        ShippedSeeds.AddType(db, PortfolioReviewTypeId, "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)");
        Add(db, 1, CbdTypeId, "requested", InstitutionId);
        Add(db, 2, PortfolioReviewTypeId, "submitted", InstitutionId);
        Add(db, 3, CbdTypeId, "draft", InstitutionId);
        Add(db, 4, CbdTypeId, "declined", InstitutionId);
        Add(db, 5, CbdTypeId, "completed", InstitutionId);
        Add(db, 6, CbdTypeId, "requested", OtherInstitutionId);
        await db.SaveChangesAsync();
        return db;
    }

    private static void Add(ApplicationDbContext db, int id, int typeId, string state, int institutionId)
        => db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
            SubjectUserId = $"trainee-{id}", CreatedByUserId = $"trainee-{id}", CurrentState = state,
            DataJson = """{ "assessor_user_id": "assessor-1" }""",
            InstitutionId = institutionId, SpecialityId = SpecialityId, SubSpecialityId = SubSpecialityId,
            CreatedOn = DateTime.UtcNow.AddDays(-2), UpdatedOn = DateTime.UtcNow.AddDays(-1)
        });

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
