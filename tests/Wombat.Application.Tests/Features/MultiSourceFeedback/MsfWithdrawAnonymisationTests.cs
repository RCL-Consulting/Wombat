using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The withdraw command stores every respondent anonymised. (T202)
/// </summary>
/// <remarks>
/// <see cref="MsfCampaign.Withdraw" /> anonymises only the invitations it has been given, so this is the half the domain
/// test cannot see: that the command loads them, and that what it stores is what the aggregate did. Read back through a
/// second context, so nothing tracked can stand in for what was saved.
/// </remarks>
public sealed class MsfWithdrawAnonymisationTests
{
    private static readonly string[] Respondents = ["nurse-1@example.test", "consultant-1@example.test"];

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    public async Task TheWithdrawCommand_StoresEveryRespondentAnonymised(MsfCampaignState state)
    {
        var campaignId = await SeedCampaignAsync(state);

        await using (var db = CreateDb())
        {
            await new WithdrawMsfCampaignCommandHandler(db)
                .Handle(new WithdrawMsfCampaignCommand(campaignId, state, TestPrincipals.Administrator()), CancellationToken.None);
        }

        await using var read = CreateDb();
        var campaign = await read.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        campaign.State.Should().Be(MsfCampaignState.Withdrawn);
        campaign.Invitations.Should().HaveCount(Respondents.Length);
        campaign.Invitations.Should().OnlyContain(invitation =>
            invitation.RespondentEmail == null &&
            invitation.AnonymizedOn == campaign.WithdrawnOn);
    }

    /// <summary>
    /// The page's caller: the campaign list offers Withdraw to a Coordinator, who runs campaigns for their own
    /// institution's trainees (T206, T113).
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    public async Task ACoordinatorOfTheTraineesInstitution_WithdrawsTheCampaign_AndEveryRespondentIsStoredAnonymised(
        MsfCampaignState state)
    {
        var campaignId = await SeedCampaignAsync(state, traineeInstitutionId: CoordinatorInstitution);

        await using (var db = CreateDb())
        {
            await new WithdrawMsfCampaignCommandHandler(db).Handle(
                new WithdrawMsfCampaignCommand(campaignId, state, TestPrincipals.Coordinator(CoordinatorInstitution)),
                CancellationToken.None);
        }

        await using var read = CreateDb();
        var campaign = await read.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        campaign.State.Should().Be(MsfCampaignState.Withdrawn);
        campaign.WithdrawnOn.Should().NotBeNull();
        campaign.Invitations.Should().HaveCount(Respondents.Length).And.OnlyContain(invitation =>
            invitation.RespondentEmail == null &&
            invitation.AnonymizedOn == campaign.WithdrawnOn);
    }

    /// <summary>
    /// T199: a campaign that has closed, whose report nobody will release, is withdrawn by whoever may release it. It is
    /// then never released, and a committee review stops calling it awaiting release. Closing already removed every
    /// address, and the time it did so stands: withdrawing removes nothing more.
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Closed)]
    public async Task ACoordinatorOfTheTraineesInstitution_WithdrawsACampaignThatHasClosed_AndItsCloseStands(
        MsfCampaignState state)
    {
        var closedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var campaignId = await SeedCampaignAsync(state, traineeInstitutionId: CoordinatorInstitution, closedOn: closedOn);

        await using (var db = CreateDb())
        {
            await new WithdrawMsfCampaignCommandHandler(db).Handle(
                new WithdrawMsfCampaignCommand(campaignId, state, TestPrincipals.Coordinator(CoordinatorInstitution)),
                CancellationToken.None);
        }

        await using var read = CreateDb();
        var campaign = await read.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        campaign.State.Should().Be(MsfCampaignState.Withdrawn);
        campaign.WithdrawnOn.Should().NotBeNull();
        campaign.ReleasedOn.Should().BeNull();
        campaign.Invitations.Should().HaveCount(Respondents.Length).And.OnlyContain(invitation =>
            invitation.RespondentEmail == null && invitation.AnonymizedOn == closedOn);
    }

    /// <summary>
    /// A withdraw of a campaign released meanwhile, from a page loaded before another tab released it, is refused, and
    /// the campaign is left as it is: its report is the trainee's. (T206 review; since T199 the only state besides
    /// Withdrawn that is refused)
    /// </summary>
    [Fact]
    public async Task AWithdrawOfAReleasedCampaign_IsRefused_AndChangesNothing()
    {
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Released, traineeInstitutionId: CoordinatorInstitution);

        await using (var db = CreateDb())
        {
            var withdraw = () => new WithdrawMsfCampaignCommandHandler(db).Handle(
                new WithdrawMsfCampaignCommand(campaignId, MsfCampaignState.UnderReview, TestPrincipals.Coordinator(CoordinatorInstitution)),
                CancellationToken.None);

            (await withdraw.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(WithdrawMsfCampaignCommandHandler.ReleasedNotWithdrawable);
        }

        await using var read = CreateDb();
        var campaign = await read.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        campaign.State.Should().Be(MsfCampaignState.Released);
        campaign.WithdrawnOn.Should().BeNull();
        campaign.Invitations.Select(invitation => invitation.RespondentEmail).Should().BeEquivalentTo(Respondents);
    }

    /// <summary>
    /// T199 review: a page loaded while the campaign was open asks in an open campaign's words (links stop, addresses go).
    /// If the auto-close job or another tab closes it before the coordinator confirms, withdrawing it would be the
    /// decision never to release its report, which nobody was asked. It is refused, and nothing changes: the save the
    /// audit pipeline makes after a refusal, run here on the handler's own context, finds nothing to commit.
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Open, MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Open, MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.Draft, MsfCampaignState.UnderReview)]
    public async Task AWithdrawConfirmedBeforeTheCampaignClosed_IsRefused_AndChangesNothing(
        MsfCampaignState confirmed, MsfCampaignState now)
    {
        var closedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var campaignId = await SeedCampaignAsync(now, traineeInstitutionId: CoordinatorInstitution, closedOn: closedOn);

        await using var db = CreateDb();
        var withdraw = () => new WithdrawMsfCampaignCommandHandler(db).Handle(
            new WithdrawMsfCampaignCommand(campaignId, confirmed, TestPrincipals.Coordinator(CoordinatorInstitution)),
            CancellationToken.None);

        (await withdraw.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage(WithdrawMsfCampaignCommandHandler.ClosedSinceShown);

        // The audit trap: AuditPipelineBehavior saves this context from its catch.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var campaign = await db.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        campaign.State.Should().Be(now, "the refused withdraw stored nothing");
        campaign.WithdrawnOn.Should().BeNull();
        campaign.Invitations.Should().HaveCount(Respondents.Length).And.OnlyContain(invitation =>
            invitation.RespondentEmail == null && invitation.AnonymizedOn == closedOn);
    }

    /// <summary>
    /// A withdraw confirmed in words that still hold is taken: a draft opened since stops its links and removes its
    /// addresses as the draft's dialog said, and a campaign closed and then put under review is still never released.
    /// The reverse of the refusal above is not asked (a campaign does not reopen, and whoever confirmed a closed campaign's
    /// withdraw may withdraw an open one by the same rule), so a caller who says they saw it closed withdraws an open one.
    /// (T199 review)
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Draft, MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.Closed, MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.UnderReview, MsfCampaignState.Open)]
    public async Task AWithdrawConfirmedInWordsThatStillHold_IsTaken(MsfCampaignState confirmed, MsfCampaignState now)
    {
        var closedOn = MsfCampaignRules.WithdrawingForgoesRelease(now) ? new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc) : (DateTime?)null;
        var campaignId = await SeedCampaignAsync(now, traineeInstitutionId: CoordinatorInstitution, closedOn: closedOn);

        await using (var db = CreateDb())
        {
            await new WithdrawMsfCampaignCommandHandler(db).Handle(
                new WithdrawMsfCampaignCommand(campaignId, confirmed, TestPrincipals.Coordinator(CoordinatorInstitution)),
                CancellationToken.None);
        }

        await using var read = CreateDb();
        (await read.MsfCampaigns.AsNoTracking().SingleAsync(entity => entity.Id == campaignId))
            .State.Should().Be(MsfCampaignState.Withdrawn);
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft, false)]
    [InlineData(MsfCampaignState.Open, false)]
    [InlineData(MsfCampaignState.Closed, true)]
    [InlineData(MsfCampaignState.UnderReview, true)]
    public void WithdrawingForgoesRelease_OnlyOnceTheCampaignHasClosed(MsfCampaignState state, bool forgoesRelease)
    {
        // The one predicate the command's refusal and both pages' withdraw wording ask (T199 review).
        MsfCampaignRules.WithdrawingForgoesRelease(state).Should().Be(forgoesRelease);
    }

    [Theory]
    [InlineData(MsfCampaignState.Draft, true)]
    [InlineData(MsfCampaignState.Open, true)]
    [InlineData(MsfCampaignState.Closed, true)]
    [InlineData(MsfCampaignState.UnderReview, true)]
    [InlineData(MsfCampaignState.Released, false)]
    [InlineData(MsfCampaignState.Withdrawn, false)]
    public void WhatCanBeWithdrawn_IsEveryCampaignNotYetReleased(MsfCampaignState state, bool withdrawable)
    {
        // The one predicate the command, the campaign list and the campaign page ask (T199).
        MsfCampaignRules.IsWithdrawable(state).Should().Be(withdrawable);
    }

    /// <summary>
    /// A second withdraw of the same campaign, from another tab, is refused, and the first one's time stands beside the
    /// anonymising it did. (T206 review)
    /// </summary>
    [Fact]
    public async Task ASecondWithdraw_IsRefused_AndTheFirstWithdrawalStands()
    {
        var campaignId = await SeedCampaignAsync(MsfCampaignState.Open, traineeInstitutionId: CoordinatorInstitution);
        var coordinator = TestPrincipals.Coordinator(CoordinatorInstitution);

        await using (var db = CreateDb())
        {
            await new WithdrawMsfCampaignCommandHandler(db)
                .Handle(new WithdrawMsfCampaignCommand(campaignId, MsfCampaignState.Open, coordinator), CancellationToken.None);
        }

        DateTime? firstWithdrawnOn;
        await using (var read = CreateDb())
        {
            firstWithdrawnOn = (await read.MsfCampaigns.AsNoTracking().SingleAsync(entity => entity.Id == campaignId)).WithdrawnOn;
        }

        await using (var db = CreateDb())
        {
            var again = () => new WithdrawMsfCampaignCommandHandler(db)
                .Handle(new WithdrawMsfCampaignCommand(campaignId, MsfCampaignState.Open, coordinator), CancellationToken.None);

            (await again.Should().ThrowAsync<InvalidOperationException>()).WithMessage(MsfCampaign.AlreadyWithdrawn);
        }

        await using var after = CreateDb();
        var campaign = await after.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        firstWithdrawnOn.Should().NotBeNull();
        campaign.WithdrawnOn.Should().Be(firstWithdrawnOn);
        campaign.Invitations.Should().OnlyContain(invitation => invitation.AnonymizedOn == firstWithdrawnOn);
    }

    private const int CoordinatorInstitution = 1;

    /// <param name="closedOn">
    /// When the campaign closed, which anonymised every invitation then (MsfCampaign.Close); null for one never closed,
    /// whose invitations still hold their addresses.
    /// </param>
    private async Task<int> SeedCampaignAsync(MsfCampaignState state, int? traineeInstitutionId = null, DateTime? closedOn = null)
    {
        await using var db = CreateDb();

        if (traineeInstitutionId is int institutionId)
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = "trainee-1", InstitutionId = institutionId, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        var campaign = new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow.AddDays(-7),
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            State = state,
            OpenedOn = state == MsfCampaignState.Draft ? null : DateTime.UtcNow.AddDays(-1),
            ClosedOn = closedOn,
            Template = new MsfTemplate { Name = "Annual MSF", IsActive = true },
            Invitations = Respondents
                .Select(email => new MsfInvitation
                {
                    RespondentEmail = closedOn is null ? email : null,
                    AnonymizedOn = closedOn,
                    RespondentCategory = MsfRespondentCategory.Nurse,
                    TokenHash = "hash",
                    IssuedOn = DateTime.UtcNow.AddDays(-1),
                    ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(21)
                })
                .ToList()
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
