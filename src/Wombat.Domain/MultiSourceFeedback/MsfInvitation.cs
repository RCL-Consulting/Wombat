namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfInvitation
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string? RespondentEmail { get; set; }
    public MsfRespondentCategory RespondentCategory { get; set; }

    /// <summary>
    /// Where a <see cref="MsfRespondentCategory.Learner" /> was taught by the trainee, as the coordinator typed it; null
    /// for every other category. (T164)
    /// </summary>
    /// <remarks>
    /// <para>
    /// EPA 15 asks for "feedback from at least two assessors across at least two teaching contexts" before entrustment.
    /// A response carries its invitation's context, so a campaign can say how many distinct contexts answered
    /// (<see cref="MsfCampaign.RespondedTeachingContexts" />). Nothing is gated on it: how the College counts "two
    /// teaching contexts" is its open question 9 (§ 3F), so it is reported and never enforced.
    /// </para>
    /// <para>
    /// On the invitation rather than asked of the respondent. The coordinator knows where the teaching happened and
    /// types it the same way for each learner of one group, which is what makes the count mean something; a free-text
    /// answer from each learner would count spellings. It is kept when the campaign closes and the address is erased:
    /// it names a teaching session, not a person, and no report breaks answers down by it.
    /// </para>
    /// </remarks>
    public string? TeachingContext { get; set; }

    /// <summary>
    /// The start of the link's token, stored in the clear under a unique index, so a link names its one row: the respondent
    /// page reads that row and checks the whole token against its <see cref="TokenHash" /> in constant time. Null until a
    /// link is issued (<see cref="IssueLink" />), so no link finds a draft's invitee. (T163)
    /// </summary>
    /// <remarks>
    /// Not a secret, and not derived from the respondent: random, and the token's secret half follows it. Until T163 the
    /// hash was the only key, so every response loaded every invitation there was and hashed the token against each.
    /// </remarks>
    public string? TokenSelector { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// The selector of the link a reminder replaced, which still takes the respondent's one response until their last day
    /// to respond (<see cref="LastDayToRespond" />); null when no reminder has replaced a link, and once the link is
    /// retired. Under a unique index of its own, as <see cref="TokenSelector" /> is. (T214)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Until T214 a reminder retired the link it replaced outright, and the page cannot tell a replaced link from a
    /// mistyped one. So a respondent who opened the questionnaire from their invitation and was still typing when the
    /// reminder job ran lost every answer on submit, told only that the link was "not recognised" (T206 review).
    /// </para>
    /// <para>
    /// Retired (<see cref="RetirePreviousLink" />) when the respondent answers through either link
    /// (<see cref="RecordResponse" />) and when the invitation is anonymised (<see cref="Anonymize" />), which closing and
    /// withdrawing the campaign both do. Nothing revokes an MSF invitation today; a revoked one is refused through
    /// either link, since the refusal is the invitation's, not the link's.
    /// </para>
    /// <para>
    /// The reminder job stores a previous link seconds after it read the invitation, once the mail is sent, and an answer
    /// or a close can commit in between. The database keeps it off an answered row
    /// (<c>CK_MsfInvitations_PreviousLinkUnanswered</c>), and the job's store is checked against the campaign's xmin
    /// token, which closing and withdrawing move (T214 review).
    /// </para>
    /// </remarks>
    public string? PreviousTokenSelector { get; set; }

    /// <summary>
    /// The hash of the link <see cref="PreviousTokenSelector" /> names, checked as <see cref="TokenHash" /> is; null
    /// exactly when that is. (T214)
    /// </summary>
    public string? PreviousTokenHash { get; set; }

    /// <summary>
    /// When the link <see cref="TokenHash" /> verifies was issued: when the invitation was added, then when the campaign
    /// opened and mailed it, then when a reminder replaced it (<see cref="IssueLink" />, <see cref="ReplaceLink" />, T206).
    /// </summary>
    /// <remarks>
    /// The reminder is sent once per link, and this is what says so (<see cref="IsReminderDue" />). A link issued on or
    /// after the first reminder day is not replaced by a reminder: its email already named the same last day, and a
    /// reminder would only repeat it.
    /// </remarks>
    public DateTime IssuedOn { get; set; }

    public DateOnly ExpiresOn { get; set; }
    public DateTime? RespondedOn { get; set; }
    public DateTime? RevokedOn { get; set; }
    public DateTime? AnonymizedOn { get; set; }

    public MsfCampaign Campaign { get; set; } = null!;
    public ICollection<MsfResponse> Responses { get; set; } = [];

    public bool IsTokenUsable(DateOnly today)
        => RevokedOn is null && RespondedOn is null && ExpiresOn >= today;

    /// <summary>
    /// The last day a respondent's link takes a response: the earlier of the day the feedback window closes and the
    /// invitation's own expiry. (T202, T205)
    /// </summary>
    /// <remarks>
    /// The one deadline a respondent is told, in the invitation email and on the page the link opens. A link is refused
    /// once its expiry has passed, and once its campaign has closed, which the auto-close job does the day after
    /// <paramref name="campaignClosesOn" />. The expiry is written a week after the window closes
    /// (<c>AddMsfInvitationCommandHandler</c>), so printing it would tell a respondent the link works for a week it does
    /// not (T202 review).
    /// </remarks>
    public static DateOnly LastDayToRespond(DateOnly campaignClosesOn, DateOnly expiresOn)
        => expiresOn < campaignClosesOn ? expiresOn : campaignClosesOn;

    /// <summary>
    /// How many days before <see cref="LastDayToRespond" /> a respondent who has not answered is first due a reminder.
    /// (T206)
    /// </summary>
    public const int ReminderDaysBeforeLastDay = 2;

    /// <summary>
    /// How long a link must have been out before a reminder may replace it. (T206 review)
    /// </summary>
    /// <remarks>
    /// A campaign opened the evening before the first reminder day is reminded a day later, not the next morning: a
    /// reminder hours after the invitation it repeats is noise. A day is the job's own cadence, so the reminder comes at
    /// most one run later than it otherwise would. Until T214 it also shielded a respondent answering through a link that
    /// had only just arrived, because a reminder retired the link it replaced and what was typed into it was lost on
    /// submit. Since T214 the replaced link keeps working until the last day to respond (<see cref="PreviousTokenSelector" />).
    /// </remarks>
    public static readonly TimeSpan ReminderMinimumLinkAge = TimeSpan.FromDays(1);

    /// <summary>
    /// Whether this respondent is due the reminder: their campaign is open and its window has begun, they have not
    /// answered, their link has not been revoked, and today falls between the first reminder day and their last day to
    /// respond, inclusive. Only for a link issued before the first reminder day, so each link is replaced by a reminder
    /// at most once, and at least <see cref="ReminderMinimumLinkAge" /> ago. (T206)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keyed on <see cref="LastDayToRespond" />, the one deadline the invitation and the respondent page give, and so in
    /// practice on the day the campaign's window closes. Until T206 the reminder job keyed on <see cref="ExpiresOn" />,
    /// which the product writes a week after the window closes, so it could fire only on a campaign the auto-close job
    /// had closed days before, and never did.
    /// </para>
    /// <para>
    /// A window, not a single day: the job runs daily, and a run missed on the first reminder day (the host was down)
    /// is made up on the next. A second run on one day, the scheduler's catch-up or an administrator's "Run now", finds
    /// the link it just issued and sends nothing. The scheduler never runs the job twice at once
    /// (<c>ScheduledJobLocks</c>), which is what makes that hold.
    /// </para>
    /// <para>
    /// The campaign is asked here, not only by the job's query, so that the rule is whole in one place: a draft's
    /// invitees were never sent a link to be reminded of, and a closed or withdrawn campaign takes no response. A
    /// campaign opened early is not chased before its window opens (T206 review).
    /// </para>
    /// </remarks>
    public bool IsReminderDue(MsfCampaign campaign, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        if (campaign.State != MsfCampaignState.Open ||
            RespondedOn is not null ||
            RevokedOn is not null ||
            string.IsNullOrWhiteSpace(RespondentEmail))
        {
            return false;
        }

        var today = DateOnly.FromDateTime(utcNow);
        var lastDay = LastDayToRespond(campaign.ClosesOn, ExpiresOn);
        var firstReminderDay = lastDay.AddDays(-ReminderDaysBeforeLastDay);

        return today >= campaign.OpensOn &&
               today >= firstReminderDay &&
               today <= lastDay &&
               IssuedOn < firstReminderDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) &&
               utcNow - IssuedOn >= ReminderMinimumLinkAge;
    }

    /// <summary>
    /// Stores the selector and hash of the first link a respondent is sent, and when it was issued: opening the campaign
    /// issues it. Whatever links the invitation held before are dropped, the previous link too (T214 review): a draft's
    /// placeholder hash was never mailed, and the link just issued is the only one this invitation now answers to.
    /// (T206, T163)
    /// </summary>
    public void IssueLink(string tokenSelector, string tokenHash, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenSelector);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        TokenSelector = tokenSelector;
        TokenHash = tokenHash;
        IssuedOn = utcNow;
        RetirePreviousLink();
    }

    /// <summary>
    /// Stores a reminder's new link in place of the one the respondent was sent, and keeps that one as the previous link
    /// (<see cref="PreviousTokenSelector" />), which still takes their response until their last day to respond. (T214)
    /// </summary>
    /// <remarks>
    /// Only a link a token can find is kept: one with a selector. An invitation holding none, as every invitation stored
    /// before T163 does, is found by no link, and keeping its hash would revive nothing. A reminder replaces
    /// a link at most once (<see cref="IsReminderDue" />), so one previous link is all there ever is; were a second
    /// replacement to come, the older of the two would be retired.
    /// </remarks>
    public void ReplaceLink(string tokenSelector, string tokenHash, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenSelector);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        var replacedSelector = TokenSelector;
        var replacedHash = TokenSelector is null ? null : TokenHash;
        IssueLink(tokenSelector, tokenHash, utcNow);
        PreviousTokenSelector = replacedSelector;
        PreviousTokenHash = replacedHash;
    }

    /// <summary>
    /// Records the respondent's one response, through either link, and retires the previous link: nothing is left for it
    /// to take. (T214)
    /// </summary>
    public void RecordResponse(DateTime submittedOn)
    {
        RespondedOn = submittedOn;
        RetirePreviousLink();
    }

    /// <summary>Retires the link a reminder replaced, so no link but the current one names this invitation. (T214)</summary>
    public void RetirePreviousLink()
    {
        PreviousTokenSelector = null;
        PreviousTokenHash = null;
    }

    /// <summary>
    /// Erases the respondent's address, keeping nothing derived from it. An invitation already anonymised is left as
    /// it was, so its first <see cref="AnonymizedOn" /> stands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The one place a respondent is anonymised. <see cref="MsfCampaign.Close" /> calls it for every invitation, and
    /// both ways a campaign closes (the coordinator's close command and the hourly auto-close job) go through that, so
    /// the two cannot drift apart. Until T184 the job carried its own copy of this routine. <see cref="MsfCampaign.Withdraw" />
    /// calls it too (T202).
    /// </para>
    /// <para>
    /// Until T207 it kept an unsalted SHA-256 of the upper-cased address in its place. Nothing ever read it, and anyone
    /// holding the list of invited addresses could hash each one and match it to its row, and so to the answers given
    /// on that invitation: a pseudonym, not anonymity. Nothing needs to recognise a respondent after the campaign
    /// closes, so the address goes and nothing stands in for it. A respondent pseudonym, should one ever be needed,
    /// is an HMAC-SHA256 keyed with <c>Wombat__PseudonymSalt</c>, never a bare hash of the address.
    /// </para>
    /// </remarks>
    public void Anonymize(DateTime utcNow)
    {
        // The campaign takes no more responses, so the link a reminder replaced has nothing left to take (T214). Retired
        // whether or not the address is still here to erase.
        RetirePreviousLink();

        if (string.IsNullOrWhiteSpace(RespondentEmail))
        {
            return;
        }

        RespondentEmail = null;
        AnonymizedOn = utcNow;
    }
}
