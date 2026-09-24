using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// Withdrawing a campaign anonymises its respondents, as closing one does. (T202)
/// </summary>
/// <remarks>
/// A withdrawn campaign takes no more responses and is never released, so nothing needs a respondent's address after
/// it. Until T202 only <see cref="MsfCampaign.Close" /> anonymised, and a withdrawn campaign kept every address for good.
/// </remarks>
public sealed class MsfCampaignWithdrawTests
{
    private static readonly DateTime Earlier = new(2029, 5, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime WithdrawnAt = new(2029, 6, 3, 14, 0, 0, DateTimeKind.Utc);

    /// <summary>Every state a campaign can be withdrawn from. A draft has respondents too: they are added before the open.</summary>
    public static TheoryData<MsfCampaignState> WithdrawableStates => new()
    {
        MsfCampaignState.Draft,
        MsfCampaignState.Open,
        MsfCampaignState.UnderReview
    };

    [Theory]
    [MemberData(nameof(WithdrawableStates))]
    public void Withdraw_AnonymisesEveryInvitation_AtTheWithdrawalTime(MsfCampaignState state)
    {
        var campaign = Campaign(state, Invitation("nurse-1@example.test"), Invitation("consultant-1@example.test"));

        campaign.Withdraw(WithdrawnAt);

        Assert.Equal(MsfCampaignState.Withdrawn, campaign.State);
        Assert.Equal(WithdrawnAt, campaign.WithdrawnOn);
        Assert.All(campaign.Invitations, invitation =>
        {
            Assert.Null(invitation.RespondentEmail);
            Assert.Equal(64, invitation.RespondentEmailHash?.Length);
            Assert.Equal(WithdrawnAt, invitation.AnonymizedOn);
        });
        Assert.Equal(2, campaign.Invitations.Select(invitation => invitation.RespondentEmailHash).Distinct().Count());
    }

    [Fact]
    public void Withdraw_HashesAnAddressAsCloseDoes()
    {
        // One routine: the hash a withdrawal leaves is the hash a close would have left for the same address.
        var withdrawn = Campaign(MsfCampaignState.Open, Invitation("Nurse-1@Example.test"));
        var closed = Campaign(MsfCampaignState.Open, Invitation("  nurse-1@example.TEST "));

        withdrawn.Withdraw(WithdrawnAt);
        closed.Close(WithdrawnAt);

        Assert.NotNull(withdrawn.Invitations.Single().RespondentEmailHash);
        Assert.Equal(closed.Invitations.Single().RespondentEmailHash, withdrawn.Invitations.Single().RespondentEmailHash);
    }

    [Fact]
    public void Withdraw_LeavesAnInvitationTheCloseAlreadyAnonymisedAsItWas()
    {
        // Withdraw is legal from UnderReview, which a close reaches having anonymised everyone already.
        var anonymised = Invitation(email: null);
        anonymised.RespondentEmailHash = "EARLIER";
        anonymised.AnonymizedOn = Earlier;
        var campaign = Campaign(MsfCampaignState.UnderReview, anonymised);

        campaign.Withdraw(WithdrawnAt);

        Assert.Equal("EARLIER", anonymised.RespondentEmailHash);
        Assert.Equal(Earlier, anonymised.AnonymizedOn);
    }

    [Fact]
    public void Withdraw_RefusesAReleasedCampaign_AndAnonymisesNothing()
    {
        var invitation = Invitation("nurse-1@example.test");
        var campaign = Campaign(MsfCampaignState.Released, invitation);

        var refusal = Assert.Throws<InvalidOperationException>(() => campaign.Withdraw(WithdrawnAt));

        Assert.Equal("Released campaigns cannot be withdrawn.", refusal.Message);
        Assert.Equal("nurse-1@example.test", invitation.RespondentEmail);
        Assert.Null(invitation.AnonymizedOn);
    }

    /// <summary>
    /// A second withdrawal, from a list another tab left stale, is refused: the first one's time stands beside the
    /// anonymising it did. (T206 review)
    /// </summary>
    [Fact]
    public void Withdraw_RefusesACampaignAlreadyWithdrawn_AndTheFirstWithdrawalStands()
    {
        var campaign = Campaign(MsfCampaignState.Open, Invitation("nurse-1@example.test"));
        campaign.Withdraw(Earlier);

        var refusal = Assert.Throws<InvalidOperationException>(() => campaign.Withdraw(WithdrawnAt));

        Assert.Equal(MsfCampaign.AlreadyWithdrawn, refusal.Message);
        Assert.Equal(Earlier, campaign.WithdrawnOn);
        Assert.Equal(Earlier, campaign.Invitations.Single().AnonymizedOn);
    }

    private static MsfCampaign Campaign(MsfCampaignState state, params MsfInvitation[] invitations)
        => new() { State = state, Invitations = invitations.ToList() };

    private static MsfInvitation Invitation(string? email)
        => new()
        {
            RespondentEmail = email,
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenHash = "token-hash",
            IssuedOn = Earlier,
            ExpiresOn = new DateOnly(2029, 7, 7)
        };
}
