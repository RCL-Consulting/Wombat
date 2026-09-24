using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// The committee's review schedule and review page name the trainee. Both used to print the trainee's user id. (T142)
/// </summary>
public sealed class CommitteeReviewTraineeNameTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;

    [Fact]
    public async Task TheSchedule_NamesEachTrainee_InOneLookupForTheReviewsItLists()
    {
        await using var db = SeededDb();
        var panelA = await AddPanelAsync(db, InstitutionA);
        var panelB = await AddPanelAsync(db, InstitutionB);
        await AddReviewAsync(db, panelA, "amara");
        await AddReviewAsync(db, panelA, "amara");
        await AddReviewAsync(db, panelA, "departed-trainee");
        await AddReviewAsync(db, panelB, "gugu");
        var users = new FakeUserDirectory(("amara", "Amara Okafor"), ("gugu", "Gugu Zulu"));

        var reviews = await new ListReviewsForPanelQueryHandler(db, users).Handle(
            new ListReviewsForPanelQuery(TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        reviews.Select(review => review.TraineeName).Should().BeEquivalentTo(
            ["Amara Okafor", "Amara Okafor", "departed-trainee"],
            "a trainee who no longer exists is shown by id; one listed twice is looked up once");
        users.Lookups.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(["amara", "departed-trainee"], "institution B's review is not on this page");
    }

    [Fact]
    public async Task TheReviewPage_NamesTheTrainee()
    {
        await using var db = SeededDb();
        var reviewId = await AddReviewAsync(db, await AddPanelAsync(db, InstitutionA), "amara");
        var users = new FakeUserDirectory(("amara", "Amara Okafor"));

        var review = await new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        review.TraineeName.Should().Be("Amara Okafor");
        review.TraineeUserId.Should().Be("amara", "the id still drives the page's trajectory query");
    }

    [Fact]
    public async Task TheReviewPage_LooksUpNobody_ForACallerTheLadderRefuses()
    {
        await using var db = SeededDb();
        var reviewId = await AddReviewAsync(db, await AddPanelAsync(db, InstitutionB), "gugu");
        var users = new FakeUserDirectory(("gugu", "Gugu Zulu"));

        var act = () => new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        users.Lookups.Should().BeEmpty();
    }

    private static async Task<int> AddPanelAsync(ApplicationDbContext db, int institutionId)
    {
        var panel = new DecisionPanel
        {
            Name = $"Panel-{institutionId}",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institutionId,
            CreatedOn = DateTime.UtcNow
        };
        db.Set<DecisionPanel>().Add(panel);
        await db.SaveChangesAsync();
        return panel.Id;
    }

    private static async Task<int> AddReviewAsync(ApplicationDbContext db, int panelId, string traineeUserId)
    {
        var review = new CommitteeReview
        {
            TraineeUserId = traineeUserId,
            PanelId = panelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };
        db.Set<CommitteeReview>().Add(review);
        await db.SaveChangesAsync();
        return review.Id;
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Set<Institution>().AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.SaveChanges();
        return db;
    }
}
