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
            Assert.Equal(64, invitation.RespondentEmailHash?.Length);
            Assert.Equal(ClosedAt, invitation.AnonymizedOn);
        });
        Assert.Equal(2, campaign.Invitations.Select(invitation => invitation.RespondentEmailHash).Distinct().Count());
    }

    [Fact]
    public void Close_HashesAnAddress_WhateverItsCaseAndSpacing()
    {
        var first = OpenCampaign(Invitation("Nurse-1@Example.test"));
        var second = OpenCampaign(Invitation("  nurse-1@example.TEST "));

        first.Close(ClosedAt);
        second.Close(ClosedAt);

        Assert.NotNull(first.Invitations.Single().RespondentEmailHash);
        Assert.Equal(first.Invitations.Single().RespondentEmailHash, second.Invitations.Single().RespondentEmailHash);
    }

    [Fact]
    public void Close_LeavesAnAlreadyAnonymisedInvitationAsItWas()
    {
        // Close is legal from UnderReview too, so a second close must not re-stamp the first anonymising.
        var anonymised = Invitation(email: null);
        anonymised.RespondentEmailHash = "EARLIER";
        anonymised.AnonymizedOn = Earlier;
        var campaign = OpenCampaign(anonymised);

        campaign.Close(ClosedAt);

        Assert.Equal("EARLIER", anonymised.RespondentEmailHash);
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
