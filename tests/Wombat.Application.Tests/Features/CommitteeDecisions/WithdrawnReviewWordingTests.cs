using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T258: a withdrawn review's outcome, as a review list says it. Nothing was ratified at it, so it is neither "Pending"
/// (nothing more is decided at it) nor the category of a decision it recorded and never ratified.
/// </summary>
public sealed class WithdrawnReviewWordingTests
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(false, CommitteeDecisionCategory.SatisfactoryProgress)]
    [InlineData(true, null)]
    public void AWithdrawnReview_ListsItsOutcomeAsNone(bool formative, CommitteeDecisionCategory? recorded)
    {
        var listed = Listed(CommitteeReviewState.Withdrawn, recorded, formative);

        CommitteeDecisionWording.OutcomeLabel(listed).Should().Be(CommitteeDecisionWording.WithdrawnOutcome);
        listed.HasDecision.Should().BeFalse();
    }

    [Fact]
    public void AnOpenReview_IsStillPending()
    {
        CommitteeDecisionWording.OutcomeLabel(Listed(CommitteeReviewState.Scheduled, null, formative: false))
            .Should().Be(CommitteeDecisionWording.PendingOutcome);
    }

    /// <summary>
    /// T258 review: a withdrawn entrustment-only review is not told that its decision is "the entrustment decisions staged
    /// below", nor that it issued any: nothing is staged at it, and it issued nothing.
    /// </summary>
    [Fact]
    public void AWithdrawnEntrustmentOnlyReview_IsToldItIssuedNothing()
    {
        CommitteeDecisionWording.EntrustmentOnlyPanelNote(CommitteeReviewState.Withdrawn)
            .Should().Be(CommitteeDecisionWording.EntrustmentOnlyWithdrawnForThePanel)
            .And.Contain("issued no entrustment decision");
        CommitteeDecisionWording.EntrustmentOnlyPanelNote(CommitteeReviewState.InProgress)
            .Should().Be(CommitteeDecisionWording.EntrustmentOnlyForThePanel);
        CommitteeDecisionWording.EntrustmentOnlyPanelNote(CommitteeReviewState.Ratified)
            .Should().Be(CommitteeDecisionWording.EntrustmentOnlyIssuedForThePanel);
    }

    /// <summary>
    /// T258 review: the review page says the day a review was withdrawn on the South African calendar, the one the erasure
    /// ends the trainee's profile on. At 22:30 UTC it is already the next day there.
    /// </summary>
    [Fact]
    public async Task TheReviewSaysTheDayItWasWithdrawn_OnTheSouthAfricanCalendar()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = 1,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }]
        });
        var review = new CommitteeReview
        {
            Id = 30,
            AcademicYear = 2026,
            Semester = 1,
            PanelId = 20,
            TraineeUserId = "deleted_user_1a2b3c4d",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 2)
        };
        review.Withdraw(CommitteeReview.WithdrawnTraineeErased, new DateTime(2026, 9, 24, 22, 30, 0, DateTimeKind.Utc));
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        var coordinator = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coord-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, 1.ToString(CultureInfo.InvariantCulture))
            ],
            "test"));

        var shown = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(30, coordinator),
            CancellationToken.None);

        shown.WithdrawnOn.Should().Be(new DateOnly(2026, 9, 25));
        shown.WithdrawalReason.Should().Be(CommitteeReview.WithdrawnTraineeErased);
    }

    private static CommitteeReviewListItemDto Listed(CommitteeReviewState state, CommitteeDecisionCategory? category, bool formative)
        => new(
            30, "deleted_user_1a2b3c4d", 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2), state, category, RatifiedOn: null, IsFormative: formative)
        {
            AcademicYear = 2026,
            Semester = 1
        };
}
