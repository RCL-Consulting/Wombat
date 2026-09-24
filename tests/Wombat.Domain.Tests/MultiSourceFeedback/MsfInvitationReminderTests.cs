using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// When a respondent who has not answered is due the reminder, and that each link is replaced by one at most once.
/// (T206)
/// </summary>
/// <remarks>
/// The invitation's expiry is written as the product writes it, a week after the window closes
/// (<c>AddMsfInvitationCommandHandler</c>). Until T206 the reminder keyed on that expiry, so it was due only once the
/// auto-close job had already closed the campaign.
/// </remarks>
public sealed class MsfInvitationReminderTests
{
    private static readonly DateOnly ClosesOn = new(2029, 3, 21);

    /// <summary>The link mailed when the campaign opened, a fortnight before the window closes.</summary>
    private static readonly DateTime OpenedAt = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);

    /// <summary>An open campaign whose window runs from the day it opened to <see cref="ClosesOn" />.</summary>
    private static readonly MsfCampaign Open = Campaign(MsfCampaignState.Open);

    [Theory]
    [InlineData(-3, false)]
    [InlineData(-2, true)]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public void TheReminderIsDue_FromTwoDaysBeforeTheWindowCloses_UntilItsLastDay(int daysFromClose, bool due)
    {
        var invitation = OpenedInvitation();

        Assert.Equal(due, invitation.IsReminderDue(Open, At(ClosesOn.AddDays(daysFromClose))));
    }

    [Fact]
    public void AnInvitationThatExpiresBeforeTheWindowCloses_IsDueTwoDaysBeforeItsExpiry()
    {
        var invitation = OpenedInvitation();
        invitation.ExpiresOn = ClosesOn.AddDays(-4);

        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-7))));
        Assert.True(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-6))));
        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-2))));
    }

    [Fact]
    public void ALinkAReminderReplaced_IsNotReplacedAgain()
    {
        var invitation = OpenedInvitation();
        var remindedAt = At(ClosesOn.AddDays(-2));

        invitation.IssueLink("reminder-selector", "reminder-hash", remindedAt);

        Assert.Equal("reminder-selector", invitation.TokenSelector);
        Assert.Equal("reminder-hash", invitation.TokenHash);
        Assert.Equal(remindedAt, invitation.IssuedOn);
        Assert.False(invitation.IsReminderDue(Open, remindedAt.AddHours(3)), "a second run the same day");
        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-1))));
        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn)));
    }

    [Fact]
    public void ALinkMailedInsideTheWindow_IsNotReplaced()
    {
        // A campaign opened the day before its window closes: the invitation just sent names the same last day.
        var invitation = OpenedInvitation();
        invitation.IssueLink("opening-selector", "opening-hash", At(ClosesOn.AddDays(-1)).AddHours(-1));

        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-1))));
        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn)));
    }

    /// <summary>
    /// A link mailed the evening before the first reminder day is not replaced the next morning, while its respondent
    /// may be answering through it, but a day later (T206 review).
    /// </summary>
    [Fact]
    public void ALinkMailedTheEveningBeforeTheFirstReminderDay_IsReplacedADayLater_NotTheNextMorning()
    {
        var invitation = OpenedInvitation();
        invitation.IssueLink("opening-selector", "opening-hash", ClosesOn.AddDays(-3).ToDateTime(new TimeOnly(21, 30), DateTimeKind.Utc));

        Assert.False(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-2))), "mailed ten and a half hours before");
        Assert.True(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-1))));
    }

    [Fact]
    public void ALinkMailedADayBeforeTheFirstReminderRun_IsReplacedOnTheFirstReminderDay()
    {
        var mailedAt = At(ClosesOn.AddDays(-2)) - MsfInvitation.ReminderMinimumLinkAge;

        var aDayOld = OpenedInvitation();
        aDayOld.IssueLink("opening-selector", "opening-hash", mailedAt);
        var notQuite = OpenedInvitation();
        notQuite.IssueLink("opening-selector", "opening-hash", mailedAt.AddTicks(1));

        Assert.True(aDayOld.IsReminderDue(Open, At(ClosesOn.AddDays(-2))));
        Assert.False(notQuite.IsReminderDue(Open, At(ClosesOn.AddDays(-2))));
    }

    /// <summary>
    /// A draft's invitees were never sent a link, and a closed or withdrawn campaign takes no response: none is reminded,
    /// whoever asks (T206 review).
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Released)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public void NoReminderIsDue_OnACampaignThatIsNotOpen(MsfCampaignState state)
    {
        var invitation = OpenedInvitation();

        Assert.True(invitation.IsReminderDue(Open, At(ClosesOn.AddDays(-2))), "guard: due on the open campaign");
        Assert.False(invitation.IsReminderDue(Campaign(state), At(ClosesOn.AddDays(-2))));
    }

    /// <summary>
    /// A campaign opened, and so mailed, before its window opens is not chased for feedback before the window opens
    /// (T206 review).
    /// </summary>
    [Fact]
    public void NoReminderIsDue_BeforeTheWindowOpens()
    {
        var oneDayWindow = Campaign(MsfCampaignState.Open, opensOn: ClosesOn.AddDays(-1));
        var invitation = OpenedInvitation();

        Assert.False(invitation.IsReminderDue(oneDayWindow, At(ClosesOn.AddDays(-2))));
        Assert.True(invitation.IsReminderDue(oneDayWindow, At(ClosesOn.AddDays(-1))));
    }

    [Fact]
    public void ARespondentWhoAnswered_WhoseLinkWasRevoked_OrWhoWasAnonymised_IsNeverDue()
    {
        var reminderDay = At(ClosesOn.AddDays(-2));

        var answered = OpenedInvitation();
        answered.RespondedOn = OpenedAt.AddDays(1);

        var revoked = OpenedInvitation();
        revoked.RevokedOn = OpenedAt.AddDays(1);

        var anonymised = OpenedInvitation();
        anonymised.Anonymize(OpenedAt.AddDays(1));

        Assert.False(answered.IsReminderDue(Open, reminderDay));
        Assert.False(revoked.IsReminderDue(Open, reminderDay));
        Assert.False(anonymised.IsReminderDue(Open, reminderDay));
    }

    private static MsfInvitation OpenedInvitation()
    {
        var invitation = new MsfInvitation
        {
            RespondentEmail = "nurse-1@example.test",
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenHash = "added-hash",
            IssuedOn = OpenedAt.AddDays(-3),
            ExpiresOn = ClosesOn.AddDays(7)
        };

        invitation.IssueLink("opening-selector", "opening-hash", OpenedAt);
        return invitation;
    }

    private static MsfCampaign Campaign(MsfCampaignState state, DateOnly? opensOn = null)
        => new()
        {
            State = state,
            OpensOn = opensOn ?? DateOnly.FromDateTime(OpenedAt),
            ClosesOn = ClosesOn
        };

    /// <summary>08:00 UTC, when the reminder job runs.</summary>
    private static DateTime At(DateOnly day) => day.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc);
}
