using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// A reminder keeps the link it replaces as the previous link, and the previous link is retired when the respondent
/// answers and when the invitation is anonymised, which closing and withdrawing the campaign both do. (T214)
/// </summary>
/// <remarks>
/// Until T214 a reminder retired the link it replaced outright, so a respondent part-way through the questionnaire on
/// it lost their answers on submit. That the lookup accepts the previous link until the last day to respond is
/// <c>MsfPreviousLinkTests</c> and <c>MsfLinkSelectorPostgresTests</c>.
/// </remarks>
public sealed class MsfInvitationPreviousLinkTests
{
    private static readonly DateOnly ClosesOn = new(2029, 3, 21);
    private static readonly DateTime OpenedAt = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime RemindedAt = new(2029, 3, 19, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AReminder_KeepsTheLinkItReplaces_AsThePreviousLink()
    {
        var invitation = OpenedInvitation();

        invitation.ReplaceLink("reminder-selector", "reminder-hash", RemindedAt);

        Assert.Equal("reminder-selector", invitation.TokenSelector);
        Assert.Equal("reminder-hash", invitation.TokenHash);
        Assert.Equal(RemindedAt, invitation.IssuedOn);
        Assert.Equal("opening-selector", invitation.PreviousTokenSelector);
        Assert.Equal("opening-hash", invitation.PreviousTokenHash);
    }

    /// <summary>
    /// Opening mails the first link; what the invitation held before, a draft's placeholder hash, was never mailed and is
    /// not kept.
    /// </summary>
    [Fact]
    public void OpeningTheCampaign_KeepsNoPreviousLink()
    {
        var invitation = DraftInvitation();

        invitation.IssueLink("opening-selector", "opening-hash", OpenedAt);

        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);
    }

    /// <summary>
    /// Issuing a link makes it the only one the invitation answers to: a previous link it held is dropped with the rest
    /// (T214 review). Opening is the only caller today, and a draft holds none, so this is what keeps it so.
    /// </summary>
    [Fact]
    public void IssuingALink_DropsAPreviousLink()
    {
        var invitation = RemindedInvitation();

        invitation.IssueLink("issued-selector", "issued-hash", RemindedAt.AddHours(1));

        Assert.Equal("issued-selector", invitation.TokenSelector);
        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);
    }

    /// <summary>
    /// An invitation holding no selector, as every invitation stored before T163 does, is found by no link; a reminder
    /// that replaces its hash keeps nothing, so no link is revived.
    /// </summary>
    [Fact]
    public void AReminderReplacingAHashNoLinkNames_KeepsNoPreviousLink()
    {
        var invitation = DraftInvitation();

        invitation.ReplaceLink("reminder-selector", "reminder-hash", RemindedAt);

        Assert.Equal("reminder-selector", invitation.TokenSelector);
        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);
    }

    [Fact]
    public void AReminderThatRefusesItsArguments_ChangesNothing()
    {
        var invitation = OpenedInvitation();

        Assert.Throws<ArgumentException>(() => invitation.ReplaceLink(" ", "reminder-hash", RemindedAt));
        Assert.Throws<ArgumentException>(() => invitation.ReplaceLink("reminder-selector", "", RemindedAt));

        Assert.Equal("opening-selector", invitation.TokenSelector);
        Assert.Equal("opening-hash", invitation.TokenHash);
        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);
    }

    [Fact]
    public void AnAnswer_IsRecorded_AndRetiresThePreviousLink()
    {
        var invitation = RemindedInvitation();
        var answeredAt = RemindedAt.AddHours(2);

        invitation.RecordResponse(answeredAt);

        Assert.Equal(answeredAt, invitation.RespondedOn);
        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);
        Assert.Equal("reminder-selector", invitation.TokenSelector);
    }

    [Fact]
    public void Anonymising_RetiresThePreviousLink_EvenOfAnInvitationWhoseAddressIsAlreadyGone()
    {
        var invitation = RemindedInvitation();
        invitation.Anonymize(RemindedAt.AddDays(1));

        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);

        var addressGone = RemindedInvitation();
        addressGone.RespondentEmail = null;
        addressGone.Anonymize(RemindedAt.AddDays(1));

        Assert.Null(addressGone.PreviousTokenSelector);
        Assert.Null(addressGone.PreviousTokenHash);
        Assert.Null(addressGone.AnonymizedOn);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosingOrWithdrawingTheCampaign_RetiresEveryPreviousLink(bool withdraw)
    {
        var campaign = new MsfCampaign
        {
            State = MsfCampaignState.Open,
            OpensOn = DateOnly.FromDateTime(OpenedAt),
            ClosesOn = ClosesOn,
            Invitations = [RemindedInvitation(), RemindedInvitation()]
        };

        if (withdraw)
        {
            campaign.Withdraw(RemindedAt.AddDays(1));
        }
        else
        {
            campaign.Close(RemindedAt.AddDays(3));
        }

        Assert.All(campaign.Invitations, invitation =>
        {
            Assert.Null(invitation.PreviousTokenSelector);
            Assert.Null(invitation.PreviousTokenHash);
        });
    }

    private static MsfInvitation DraftInvitation()
        => new()
        {
            RespondentEmail = "nurse-1@example.test",
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenHash = "added-hash",
            IssuedOn = OpenedAt.AddDays(-3),
            ExpiresOn = ClosesOn.AddDays(7)
        };

    private static MsfInvitation OpenedInvitation()
    {
        var invitation = DraftInvitation();
        invitation.IssueLink("opening-selector", "opening-hash", OpenedAt);
        return invitation;
    }

    private static MsfInvitation RemindedInvitation()
    {
        var invitation = OpenedInvitation();
        invitation.ReplaceLink("reminder-selector", "reminder-hash", RemindedAt);
        Assert.NotNull(invitation.PreviousTokenSelector);
        return invitation;
    }
}
