using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
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
            !string.IsNullOrWhiteSpace(invitation.RespondentEmailHash) &&
            invitation.AnonymizedOn == campaign.WithdrawnOn);
    }

    private async Task<int> SeedCampaignAsync(MsfCampaignState state)
    {
        await using var db = CreateDb();
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
