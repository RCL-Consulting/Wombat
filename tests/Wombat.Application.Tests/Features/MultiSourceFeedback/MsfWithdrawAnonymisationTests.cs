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
                .Handle(new WithdrawMsfCampaignCommand(campaignId, TestPrincipals.Administrator()), CancellationToken.None);
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
                new WithdrawMsfCampaignCommand(campaignId, TestPrincipals.Coordinator(CoordinatorInstitution)),
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
    /// A withdraw sent from a list loaded before the campaign closed is refused, and the campaign is left as it is:
    /// withdrawing one under review would be a decision never to release it, which the list does not offer. (T206 review)
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.UnderReview, "has closed and is under review.")]
    [InlineData(MsfCampaignState.Released, "has been released to the trainee.")]
    public async Task AWithdrawOfACampaignNoLongerDraftOrOpen_IsRefused_AndChangesNothing(
        MsfCampaignState state, string saysWhy)
    {
        var campaignId = await SeedCampaignAsync(state, traineeInstitutionId: CoordinatorInstitution);

        await using (var db = CreateDb())
        {
            var withdraw = () => new WithdrawMsfCampaignCommandHandler(db).Handle(
                new WithdrawMsfCampaignCommand(campaignId, TestPrincipals.Coordinator(CoordinatorInstitution)),
                CancellationToken.None);

            (await withdraw.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage("Only a draft or open campaign can be withdrawn, and this one " + saysWhy);
        }

        await using var read = CreateDb();
        var campaign = await read.MsfCampaigns.AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        campaign.State.Should().Be(state);
        campaign.WithdrawnOn.Should().BeNull();
        campaign.Invitations.Select(invitation => invitation.RespondentEmail).Should().BeEquivalentTo(Respondents);
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
                .Handle(new WithdrawMsfCampaignCommand(campaignId, coordinator), CancellationToken.None);
        }

        DateTime? firstWithdrawnOn;
        await using (var read = CreateDb())
        {
            firstWithdrawnOn = (await read.MsfCampaigns.AsNoTracking().SingleAsync(entity => entity.Id == campaignId)).WithdrawnOn;
        }

        await using (var db = CreateDb())
        {
            var again = () => new WithdrawMsfCampaignCommandHandler(db)
                .Handle(new WithdrawMsfCampaignCommand(campaignId, coordinator), CancellationToken.None);

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

    private async Task<int> SeedCampaignAsync(MsfCampaignState state, int? traineeInstitutionId = null)
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
            Template = new MsfTemplate { Name = "Annual MSF", IsActive = true },
            Invitations = Respondents
                .Select(email => new MsfInvitation
                {
                    RespondentEmail = email,
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
