using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// Closing a campaign anonymises its respondents, and opening one can be refused before anything is sent. (T184) Only an
/// open campaign closes, so a second close cannot move the first one's time. (T246)
/// </summary>
public sealed class MsfCampaignOpenCloseTests
{
    private static readonly DateTime Earlier = new(2029, 5, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FirstClosedAt = new(2029, 6, 28, 9, 0, 0, DateTimeKind.Utc);
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
        // An invitation whose address is already gone keeps its first anonymising. Until T246 this was how a second
        // close, legal then from UnderReview, left the first close's stamps; a second close is now refused outright
        // (Close_ASecondClose_IsRefused_AndTheFirstCloseStands), and this is the invitation's own guarantee.
        var anonymised = Invitation(email: null);
        anonymised.AnonymizedOn = Earlier;
        var campaign = OpenCampaign(anonymised);

        campaign.Close(ClosedAt);

        Assert.Null(anonymised.RespondentEmail);
        Assert.Equal(Earlier, anonymised.AnonymizedOn);
    }

    /// <summary>
    /// A close from a report tab loaded while the campaign was open, pressed after another tab closed it, is refused, and
    /// the first close's time stands: <see cref="MsfCampaign.ClosedOn" /> decides the campaign's semester, its committee
    /// window and its evidence date. Until T246 the second close succeeded and moved it to that day. (T246)
    /// </summary>
    [Fact]
    public void Close_ASecondClose_IsRefused_AndTheFirstCloseStands()
    {
        var campaign = OpenCampaign(Invitation("nurse-1@example.test"));
        campaign.Close(FirstClosedAt);

        var refusal = Assert.Throws<InvalidOperationException>(() => campaign.Close(ClosedAt));

        Assert.Equal(MsfCampaign.OnlyOpenCanBeClosed, refusal.Message);
        Assert.Equal("Only open campaigns can be closed.", refusal.Message);
        Assert.Equal(MsfCampaignState.UnderReview, campaign.State);
        Assert.Equal(FirstClosedAt, campaign.ClosedOn);
        Assert.Equal(FirstClosedAt, campaign.Invitations.Single().AnonymizedOn);
    }

    /// <summary>
    /// Only an open campaign closes. Anything else is refused before any change: its state and its close time stay, and
    /// no invitation is touched, not even the link a reminder replaced, which anonymising retires (T214). A draft has not
    /// opened; one under review, released or withdrawn has finished taking responses. (T246)
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Released)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public void Close_RefusesAnythingButAnOpenCampaign_AndChangesNothing(MsfCampaignState state)
    {
        DateTime? closedOn = state is MsfCampaignState.Closed or MsfCampaignState.UnderReview or MsfCampaignState.Released
            ? FirstClosedAt
            : null;
        var invitation = Invitation("nurse-1@example.test");
        invitation.PreviousTokenSelector = "previous-selector";
        invitation.PreviousTokenHash = "previous-hash";
        var campaign = new MsfCampaign { State = state, ClosedOn = closedOn, Invitations = [invitation] };

        var refusal = Assert.Throws<InvalidOperationException>(() => campaign.Close(ClosedAt));

        Assert.Equal(MsfCampaign.OnlyOpenCanBeClosed, refusal.Message);
        Assert.Equal(state, campaign.State);
        Assert.Equal(closedOn, campaign.ClosedOn);
        Assert.Equal("nurse-1@example.test", invitation.RespondentEmail);
        Assert.Null(invitation.AnonymizedOn);
        Assert.Equal("previous-selector", invitation.PreviousTokenSelector);
        Assert.Equal("previous-hash", invitation.PreviousTokenHash);
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
