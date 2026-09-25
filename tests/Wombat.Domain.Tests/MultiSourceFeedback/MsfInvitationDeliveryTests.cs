using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// What became of a link's mail, as the invitation holds it: which links count as not delivered or still being sent, the
/// key a link's mail carries, what a replacement does with a dropped link, and that anonymising clears the outcome. (T251)
/// </summary>
/// <remarks>
/// The rule is an expression, so the page's count and the resend are one statement each; here it is compiled and asked
/// of invitations in memory. That EF translates it, on the in-memory provider and on PostgreSQL, is
/// <c>ResendMsfLinksCommandHandlerTests</c> and <c>MsfLinkDeliveryPostgresTests</c>.
/// </remarks>
public sealed class MsfInvitationDeliveryTests
{
    private static readonly DateOnly ClosesOn = new(2029, 3, 21);
    private static readonly DateTime OpenedAt = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Soon = OpenedAt.AddMinutes(10);
    private static readonly DateTime Late = OpenedAt + MsfInvitation.DeliveryReportDeadline + TimeSpan.FromMinutes(1);

    // ─── The key a link's mail carries ───────────────────────────────────────

    [Fact]
    public void TheDeliveryKey_NamesTheInvitationAndTheLink_AndReadsBack()
    {
        var key = MsfInvitation.DeliveryKey(42, "AbC-_12xyzAbC-_1");

        Assert.True(MsfInvitation.TryReadDeliveryKey(key, out var invitationId, out var selector));
        Assert.Equal(42, invitationId);
        Assert.Equal("AbC-_12xyzAbC-_1", selector);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("msf-link:")]
    [InlineData("msf-link:42")]
    [InlineData("msf-link:42:")]
    [InlineData("msf-link:0:selector")]
    [InlineData("msf-link:-3:selector")]
    [InlineData("msf-link:x:selector")]
    [InlineData("msf-link:42:one:two")]
    [InlineData("invitation:42:selector")]
    public void AnyOtherKey_NamesNoInvitation(string? key)
    {
        Assert.False(MsfInvitation.TryReadDeliveryKey(key, out var invitationId, out var selector));
        Assert.Equal(0, invitationId);
        Assert.Equal(string.Empty, selector);
    }

    [Fact]
    public void ADeliveryKey_IsRefusedForNoInvitation_OrASelectorItCouldNotReadBack()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MsfInvitation.DeliveryKey(0, "selector"));
        Assert.Throws<ArgumentException>(() => MsfInvitation.DeliveryKey(42, " "));
        Assert.Throws<ArgumentException>(() => MsfInvitation.DeliveryKey(42, "sel:ector"));
    }

    // ─── Not delivered, being sent ───────────────────────────────────────────

    [Fact]
    public void ALinkWhoseMailWasDropped_IsNotDelivered_AtOnce()
    {
        var invitation = Opened(outcome: Dropped("opening-selector"));

        Assert.True(NotDelivered(invitation, Soon));
        Assert.False(BeingSent(invitation, Soon));
        Assert.True(invitation.LinkDeliveryFailed);
    }

    [Fact]
    public void ALinkWhoseMailWasSent_IsNeitherNotDeliveredNorBeingSent_HoweverLongAgo()
    {
        var invitation = Opened(outcome: Sent("opening-selector"));

        Assert.False(NotDelivered(invitation, Late));
        Assert.False(BeingSent(invitation, Late));
        Assert.False(invitation.LinkDeliveryFailed);
    }

    [Fact]
    public void ALinkNothingWasReportedOf_IsBeingSent_UntilTheDeadline_ThenNotDelivered()
    {
        // The host crashed with the mail queued: nothing will ever be reported.
        var invitation = Opened(outcome: null);

        Assert.True(BeingSent(invitation, Soon));
        Assert.False(NotDelivered(invitation, Soon));

        Assert.False(BeingSent(invitation, Late));
        Assert.True(NotDelivered(invitation, Late));
        Assert.False(invitation.LinkDeliveryFailed, "nothing says it was dropped: it may have arrived");
    }

    /// <summary>
    /// An outcome about a link the invitation no longer holds, or one it never stored (an open refused at its save), says
    /// nothing about the current link: a reminder's link whose mail is not yet reported is being sent, whatever became of
    /// the link before it.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AnOutcomeAboutAnotherLink_SaysNothingAboutTheCurrentOne(bool otherWasSent)
    {
        var outcome = otherWasSent ? Sent("another-selector") : Dropped("another-selector");
        var invitation = Opened(outcome);

        Assert.True(BeingSent(invitation, Soon));
        Assert.False(NotDelivered(invitation, Soon));
        Assert.True(NotDelivered(invitation, Late));
        Assert.False(invitation.LinkDeliveryFailed);
    }

    /// <summary>
    /// Only a respondent who can still answer through a new link is counted: nobody is sent a link to a campaign that
    /// takes no answer, or for an answer already given.
    /// </summary>
    [Theory]
    [InlineData("answered")]
    [InlineData("revoked")]
    [InlineData("anonymised")]
    [InlineData("no link yet")]
    [InlineData("draft")]
    [InlineData("under review")]
    [InlineData("withdrawn")]
    [InlineData("window closed")]
    [InlineData("expired")]
    public void ARespondentWhoCannotAnswerThroughANewLink_IsNeverCounted(string why)
    {
        var dropped = Opened(outcome: Dropped("opening-selector"));
        var unreported = Opened(outcome: null);

        foreach (var invitation in new[] { dropped, unreported })
        {
            switch (why)
            {
                case "answered": invitation.RecordResponse(Soon); break;
                case "revoked": invitation.RevokedOn = Soon; break;
                case "anonymised": invitation.Anonymize(Soon); break;
                case "no link yet": invitation.TokenSelector = null; break;
                case "draft": invitation.Campaign.State = MsfCampaignState.Draft; break;
                case "under review": invitation.Campaign.State = MsfCampaignState.UnderReview; break;
                case "withdrawn": invitation.Campaign.State = MsfCampaignState.Withdrawn; break;
                case "window closed": invitation.Campaign.ClosesOn = DateOnly.FromDateTime(Late).AddDays(-1); break;
                case "expired": invitation.ExpiresOn = DateOnly.FromDateTime(Late).AddDays(-1); break;
            }

            Assert.False(NotDelivered(invitation, Late), why);
            Assert.False(BeingSent(invitation, Soon), why);
        }
    }

    [Fact]
    public void OnTheLastDayToRespond_ALinkNotDelivered_IsStillCounted()
    {
        var invitation = Opened(outcome: Dropped("opening-selector"));
        var lastDay = ClosesOn.ToDateTime(new TimeOnly(23, 0), DateTimeKind.Utc);

        Assert.True(NotDelivered(invitation, lastDay));
    }

    // ─── Replacing a link ────────────────────────────────────────────────────

    /// <summary>
    /// A dropped link reached nobody, so a new link takes its place without being kept: the previous link, which a reminder
    /// kept and the respondent may be answering through, stays as it was.
    /// </summary>
    [Fact]
    public void ReplacingALinkWhoseMailWasDropped_LetsItGo_AndKeepsThePreviousLink()
    {
        var invitation = Opened(outcome: null);
        invitation.ReplaceLink("reminder-selector", "reminder-hash", Soon);
        Record(invitation, Dropped("reminder-selector"));
        Assert.Equal("opening-selector", invitation.PreviousTokenSelector);

        invitation.ReplaceLink("resent-selector", "resent-hash", Late);

        Assert.Equal("resent-selector", invitation.TokenSelector);
        Assert.Equal("resent-hash", invitation.TokenHash);
        Assert.Equal(Late, invitation.IssuedOn);
        Assert.Equal("opening-selector", invitation.PreviousTokenSelector);
        Assert.Equal("opening-hash", invitation.PreviousTokenHash);
    }

    [Fact]
    public void ReplacingTheFirstLinkWhoseMailWasDropped_KeepsNoPreviousLink()
    {
        var invitation = Opened(outcome: Dropped("opening-selector"));

        invitation.ReplaceLink("resent-selector", "resent-hash", Soon);

        Assert.Equal("resent-selector", invitation.TokenSelector);
        Assert.Null(invitation.PreviousTokenSelector);
        Assert.Null(invitation.PreviousTokenHash);
    }

    /// <summary>
    /// A link nothing was heard of may have arrived: it is kept as the previous link, as a reminder keeps any link it
    /// replaces (T214).
    /// </summary>
    [Fact]
    public void ReplacingALinkNothingWasReportedOf_KeepsItAsThePreviousLink()
    {
        var invitation = Opened(outcome: null);

        invitation.ReplaceLink("resent-selector", "resent-hash", Late);

        Assert.Equal("resent-selector", invitation.TokenSelector);
        Assert.Equal("opening-selector", invitation.PreviousTokenSelector);
        Assert.Equal("opening-hash", invitation.PreviousTokenHash);
    }

    /// <summary>
    /// The review's case (T251 review, finding 3): the first link was reported sent, a reminder replaced it, and nothing
    /// was heard of the reminder's mail (the host crashed with it queued). The page counts that link not delivered, and a
    /// resend keeps the first link, the one the respondent is known to hold and may be answering through, not the
    /// reminder's, which most likely never left.
    /// </summary>
    [Fact]
    public void ReplacingALinkNothingWasReportedOf_KeepsThePreviousLinkWhoseMailWasSent()
    {
        var invitation = Opened(outcome: Sent("opening-selector"));
        invitation.ReplaceLink("reminder-selector", "reminder-hash", Soon);
        Assert.Equal("opening-selector", invitation.PreviousTokenSelector);
        var resentAt = Soon + MsfInvitation.DeliveryReportDeadline + TimeSpan.FromMinutes(1);
        Assert.True(NotDelivered(invitation, resentAt), "guard: the reminder's link counts as not delivered");

        invitation.ReplaceLink("resent-selector", "resent-hash", resentAt);

        Assert.Equal("resent-selector", invitation.TokenSelector);
        Assert.Equal("resent-hash", invitation.TokenHash);
        Assert.Equal(resentAt, invitation.IssuedOn);
        Assert.Equal("opening-selector", invitation.PreviousTokenSelector);
        Assert.Equal("opening-hash", invitation.PreviousTokenHash);
    }

    /// <summary>
    /// Once the reminder's own mail is reported sent, it is the link the respondent is known to hold, and a replacement
    /// keeps it as the previous link, as a reminder keeps any other (T214).
    /// </summary>
    [Fact]
    public void ReplacingALinkWhoseMailWasSent_KeepsIt_OverTheOneBeforeIt()
    {
        var invitation = Opened(outcome: Sent("opening-selector"));
        invitation.ReplaceLink("reminder-selector", "reminder-hash", Soon);
        Record(invitation, Sent("reminder-selector"));

        invitation.ReplaceLink("second-reminder-selector", "second-reminder-hash", Late);

        Assert.Equal("second-reminder-selector", invitation.TokenSelector);
        Assert.Equal("reminder-selector", invitation.PreviousTokenSelector);
        Assert.Equal("reminder-hash", invitation.PreviousTokenHash);
    }

    [Fact]
    public void ANewLinkIsBeingSent_UntilItsOwnMailIsReported()
    {
        var invitation = Opened(outcome: Dropped("opening-selector"));

        invitation.ReplaceLink("resent-selector", "resent-hash", Late);

        Assert.True(BeingSent(invitation, Late.AddMinutes(1)));
        Assert.False(NotDelivered(invitation, Late.AddMinutes(1)));

        Record(invitation, Sent("resent-selector"));

        Assert.False(BeingSent(invitation, Late.AddMinutes(1)));
        Assert.False(NotDelivered(invitation, Late.AddHours(5)));
    }

    // ─── Anonymising ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosingOrWithdrawingTheCampaign_ClearsWhatBecameOfEveryLink(bool withdraw)
    {
        var sent = Opened(outcome: Sent("opening-selector"));
        var dropped = Opened(outcome: Dropped("opening-selector"));
        var campaign = sent.Campaign;
        campaign.Invitations = [sent, dropped];
        dropped.Campaign = campaign;

        if (withdraw)
        {
            campaign.Withdraw(Late);
        }
        else
        {
            campaign.Close(Late);
        }

        Assert.All(campaign.Invitations, invitation =>
        {
            Assert.Null(invitation.SentOn);
            Assert.Null(invitation.DeliveryFailedOn);
            Assert.Null(invitation.DeliveryLinkSelector);
        });
    }

    [Fact]
    public void Anonymising_ClearsTheOutcome_EvenOfAnInvitationWhoseAddressIsAlreadyGone()
    {
        var invitation = Opened(outcome: Dropped("opening-selector"));
        invitation.RespondentEmail = null;

        invitation.Anonymize(Late);

        Assert.Null(invitation.DeliveryFailedOn);
        Assert.Null(invitation.DeliveryLinkSelector);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static bool NotDelivered(MsfInvitation invitation, DateTime utcNow)
        => MsfInvitation.LinkNotDelivered(utcNow).Compile()(invitation);

    private static bool BeingSent(MsfInvitation invitation, DateTime utcNow)
        => MsfInvitation.LinkBeingSent(utcNow).Compile()(invitation);

    private sealed record Outcome(string Selector, bool Sent);

    private static Outcome Sent(string selector) => new(selector, true);

    private static Outcome Dropped(string selector) => new(selector, false);

    /// <summary>As <c>MsfLinkDeliveryRecorder</c> writes it: the link, and exactly one of the two times.</summary>
    private static void Record(MsfInvitation invitation, Outcome outcome)
    {
        invitation.DeliveryLinkSelector = outcome.Selector;
        invitation.SentOn = outcome.Sent ? OpenedAt.AddMinutes(1) : null;
        invitation.DeliveryFailedOn = outcome.Sent ? null : OpenedAt.AddMinutes(1);
    }

    private static MsfInvitation Opened(Outcome? outcome)
    {
        var campaign = new MsfCampaign
        {
            State = MsfCampaignState.Open,
            OpensOn = DateOnly.FromDateTime(OpenedAt),
            ClosesOn = ClosesOn
        };

        var invitation = new MsfInvitation
        {
            Id = 7,
            Campaign = campaign,
            RespondentEmail = "nurse-1@example.test",
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenHash = "added-hash",
            IssuedOn = OpenedAt.AddDays(-3),
            ExpiresOn = ClosesOn.AddDays(7)
        };

        campaign.Invitations = [invitation];
        invitation.IssueLink("opening-selector", "opening-hash", OpenedAt);
        if (outcome is not null)
        {
            Record(invitation, outcome);
        }

        return invitation;
    }
}
