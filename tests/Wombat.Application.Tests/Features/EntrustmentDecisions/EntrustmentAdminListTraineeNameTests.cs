using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

/// <summary>
/// The entrustment-decision admin list names each decision's trainee. It used to print their user id. (T142)
/// </summary>
public sealed class EntrustmentAdminListTraineeNameTests
{
    [Fact]
    public async Task EachDecisionNamesItsTrainee_InOneLookupForTheRowsListed()
    {
        await using var db = SeededDb();
        db.Set<EntrustmentDecision>().AddRange(
            Decision("amara", epaId: 7),
            Decision("amara", epaId: 8),
            Decision("departed-trainee", epaId: 7));
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory(("amara", "Amara Okafor"), ("gugu", "Gugu Zulu"));

        var decisions = await new ListEntrustmentDecisionsForAdminQueryHandler(db, users).Handle(
            new ListEntrustmentDecisionsForAdminQuery(null, null, TestPrincipals.Administrator()), CancellationToken.None);

        decisions.Select(decision => decision.TraineeName).Should().BeEquivalentTo(
            ["Amara Okafor", "Amara Okafor", "departed-trainee"],
            "a trainee who no longer exists is shown by id");
        users.Lookups.Should().ContainSingle().Which.Should().BeEquivalentTo(["amara", "departed-trainee"]);
    }

    /// <summary>
    /// The page shows a name and no id, so the trainee filter takes what the page shows: part of a name, in any case.
    /// It used to match only an exact user id, which after T142 nothing on the page gives.
    /// </summary>
    [Theory]
    [InlineData("zulu")]
    [InlineData("  Gugu Z ")]
    public async Task TheTraineeFilter_MatchesPartOfTheNameTheListShows_IgnoringCase(string filter)
    {
        await using var db = SeededDb();
        db.Set<EntrustmentDecision>().AddRange(
            Decision("amara", epaId: 7), Decision("gugu", epaId: 7), Decision("gugu", epaId: 8));
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory(("amara", "Amara Okafor"), ("gugu", "Gugu Zulu"));

        var decisions = await new ListEntrustmentDecisionsForAdminQueryHandler(db, users).Handle(
            new ListEntrustmentDecisionsForAdminQuery(filter, null, TestPrincipals.Administrator()), CancellationToken.None);

        decisions.Select(decision => decision.TraineeName).Should().Equal("Gugu Zulu", "Gugu Zulu");
        users.Lookups.Should().ContainSingle("the names are looked up once, before the filter narrows them");
    }

    [Fact]
    public async Task TheTraineeFilter_StillMatchesAnExactUserId_AndNotPartOfOne()
    {
        await using var db = SeededDb();
        db.Set<EntrustmentDecision>().AddRange(Decision("trainee-12", epaId: 7), Decision("trainee-123", epaId: 7));
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory(("trainee-12", "Amara Okafor"), ("trainee-123", "Gugu Zulu"));
        var handler = new ListEntrustmentDecisionsForAdminQueryHandler(db, users);

        var exact = await handler.Handle(
            new ListEntrustmentDecisionsForAdminQuery("trainee-12", null, TestPrincipals.Administrator()), CancellationToken.None);
        var partial = await handler.Handle(
            new ListEntrustmentDecisionsForAdminQuery("trainee-1", null, TestPrincipals.Administrator()), CancellationToken.None);

        exact.Should().ContainSingle().Which.TraineeName.Should().Be("Amara Okafor");
        partial.Should().BeEmpty("an id is matched whole: part of one names nobody");
    }

    private static EntrustmentDecision Decision(string traineeUserId, int epaId)
        => EntrustmentDecision.Issue(
            traineeUserId,
            epaId,
            authorisedLevelId: 3,
            issuedOn: new DateOnly(2026, 4, 1),
            expiresOn: null,
            committeeReviewId: 30,
            chairUserId: "chair-1",
            rationale: "Ratified at the annual review.",
            evidenceLinks: []);

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "Standard 5-point" });
        db.EntrustmentLevels.Add(new EntrustmentLevel { Id = 3, ScaleId = 1, Order = 3, Label = "Level 3" });
        db.Epas.AddRange(
            new Epa { Id = 7, Code = "EPA-07", Title = "Emergency triage", IsActive = true },
            new Epa { Id = 8, Code = "EPA-08", Title = "Admission", IsActive = true });
        db.SaveChanges();
        return db;
    }
}
