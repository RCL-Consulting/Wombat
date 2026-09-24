using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// Closing a campaign anonymises its respondents, and opening one can be refused before anything is sent. (T184)
/// </summary>
public sealed class MsfCampaignOpenCloseTests
{
    private static readonly DateTime Earlier = new(2029, 5, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ClosedAt = new(2029, 6, 30, 23, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Close_AnonymisesEveryInvitation_AtTheCloseTime()
    {
        var campaign = OpenCampaign(
            Invitation("nurse-1@example.test"),
            Invitation("consultant-1@example.test"));

        campaign.Close(ClosedAt);

        Assert.Equal(MsfCampaignState.UnderReview, campaign.State);
        Assert.All(campaign.Invitations, invitation =>
        {
            Assert.Null(invitation.RespondentEmail);
            Assert.Equal(ClosedAt, invitation.AnonymizedOn);
        });
    }

    /// <summary>
    /// Closing keeps nothing anyone could compute from a respondent's address: not the address, and not an unsalted
    /// hash of it, whatever its case and spacing. Until T207 it kept a SHA-256 of the upper-cased address. (T207)
    /// </summary>
    [Theory]
    [InlineData("nurse-1@example.test")]
    [InlineData("Nurse-1@Example.test")]
    [InlineData("  nurse-1@example.TEST ")]
    public void Close_KeepsNoUnsaltedHashOfTheAddress(string address)
    {
        var campaign = OpenCampaign(Invitation(address));

        campaign.Close(ClosedAt);

        var invitation = campaign.Invitations.Single();
        Assert.Equal(ClosedAt, invitation.AnonymizedOn);
        RespondentAddressTraces.AssertNoneKept(invitation, address);
    }

    [Fact]
    public void Close_LeavesAnAlreadyAnonymisedInvitationAsItWas()
    {
        // Close is legal from UnderReview too, so a second close must not re-stamp the first anonymising.
        var anonymised = Invitation(email: null);
        anonymised.AnonymizedOn = Earlier;
        var campaign = OpenCampaign(anonymised);

        campaign.Close(ClosedAt);

        Assert.Null(anonymised.RespondentEmail);
        Assert.Equal(Earlier, anonymised.AnonymizedOn);
    }

    [Theory]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Released)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public void EnsureCanOpen_RefusesAnythingButADraft_AndChangesNothing(MsfCampaignState state)
    {
        var campaign = new MsfCampaign { State = state };

        var refusal = Assert.Throws<InvalidOperationException>(campaign.EnsureCanOpen);

        Assert.Equal("Only draft campaigns can be opened.", refusal.Message);
        Assert.Equal(state, campaign.State);
        Assert.Null(campaign.OpenedOn);
    }

    [Fact]
    public void EnsureCanOpen_AcceptsADraft_AndOpenOpensIt()
    {
        var campaign = new MsfCampaign { State = MsfCampaignState.Draft };

        campaign.EnsureCanOpen();
        Assert.Equal(MsfCampaignState.Draft, campaign.State);

        campaign.Open(Earlier);
        Assert.Equal(MsfCampaignState.Open, campaign.State);
        Assert.Equal(Earlier, campaign.OpenedOn);
    }

    private static MsfCampaign OpenCampaign(params MsfInvitation[] invitations)
        => new() { State = MsfCampaignState.Open, OpenedOn = Earlier, Invitations = invitations.ToList() };

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
