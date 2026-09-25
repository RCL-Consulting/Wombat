using System.Globalization;
using System.Linq.Expressions;

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

    /// <summary>
    /// When the mail carrying the link <see cref="DeliveryLinkSelector" /> names was accepted by the mail server; null
    /// when none has been reported sent. (T251)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mail leaves the product after the request that sends it: a send is a hand-off to an in-process queue, and the mail
    /// worker delivers it later, retrying three times before it gives up. Until T251 nothing recorded what became of an
    /// MSF link, so a campaign opened while the mail server was down was reported as sent to every respondent, and none of
    /// them could ever answer.
    /// </para>
    /// <para>
    /// Written only by the worker's report (<c>MsfLinkDeliveryRecorder</c>), with <see cref="DeliveryLinkSelector" /> and
    /// <see cref="DeliveryFailedOn" /> in one statement, and never by issuing a link. The report can arrive before the
    /// link it is about has been stored: the open and the reminder hand the mail over first and store the link after
    /// (T184, T206), and a fast mail server answers in between. So an outcome says which link it is about, and it counts
    /// only while that is still the current link (<see cref="LinkDeliveryFailed" />, <see cref="LinkNotDelivered" />).
    /// </para>
    /// <para>
    /// Carries no address, but it is not nothing about the respondent: the worker logs the address it sent to at the same
    /// instant, so a time one mail apart from the next ties a row, and the answers given on it, to an address. It serves
    /// only the Resend of an open campaign, so anonymising the invitation clears it (<see cref="Anonymize" />), and every
    /// save of an anonymised invitation writes it cleared, whatever that save read (<c>ApplicationDbContext</c>): a report
    /// that lands between a close's read and its save would otherwise survive on the erased row (T251 review).
    /// </para>
    /// </remarks>
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// When the mail carrying the link <see cref="DeliveryLinkSelector" /> names was given up on: the worker's last retry
    /// failed, or the host stopped with it still queued. Null otherwise; never set with <see cref="SentOn" />. (T251)
    /// </summary>
    public DateTime? DeliveryFailedOn { get; set; }

    /// <summary>
    /// The selector of the link whose mail <see cref="SentOn" /> or <see cref="DeliveryFailedOn" /> reports on; null
    /// until the worker has reported on one. The link's own selector, which the invitation already holds in the clear
    /// (<see cref="TokenSelector" />): not a secret, and nothing about the respondent. (T251)
    /// </summary>
    public string? DeliveryLinkSelector { get; set; }

    public MsfCampaign Campaign { get; set; } = null!;
    public ICollection<MsfResponse> Responses { get; set; } = [];

    public bool IsTokenUsable(DateOnly today)
        => RevokedOn is null && RespondedOn is null && ExpiresOn >= today;

    /// <summary>
    /// Whether the invitation holds no address: anonymising erased it (<see cref="Anonymize" />). Such an invitation holds
    /// no delivery outcome either, and every save of one writes that it holds none (<c>ApplicationDbContext</c>). (T251)
    /// </summary>
    public bool HoldsNoAddress => string.IsNullOrWhiteSpace(RespondentEmail);

    /// <summary>
    /// An address as two invitations to one campaign are compared: trimmed and lower-cased. A campaign invites an address
    /// once, so that no one is mailed two links, responds twice and counts twice towards the minimums. (T228)
    /// </summary>
    /// <remarks>
    /// An address is stored trimmed (<c>AddMsfInvitationCommandHandler</c>), and the database holds the rule too, in a
    /// unique index on the campaign and the lower-cased address while it is held (<c>MsfInvitationConfiguration</c>).
    /// Anonymising an invitation (<see cref="Anonymize" />) nulls its address, and the key with it.
    /// </remarks>
    public static string AddressKey(string address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return address.Trim().ToLowerInvariant();
    }

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
    /// Stores a new link in place of the one the respondent was sent, a reminder's (T214) or a resend's (T251), and keeps
    /// that one as the previous link (<see cref="PreviousTokenSelector" />), which still takes their response until their
    /// last day to respond. A link whose own mail was reported dropped (<see cref="LinkDeliveryFailed" />) is not kept: it
    /// reached nobody, and the previous link stays as it was. Nor is a link nothing was heard of kept in place of a
    /// previous link whose mail was reported sent (<see cref="PreviousLinkDeliveredOverUnreported" />). (T251)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only a link a token can find is kept: one with a selector. An invitation holding none, as every invitation stored
    /// before T163 does, is found by no link, and keeping its hash would revive nothing. One previous link is all there
    /// ever is: were a link the respondent may hold to be replaced twice, the older of the two would be retired.
    /// </para>
    /// <para>
    /// A dropped link is let go rather than kept for T251's sake. A reminder whose own mail was dropped, resent, would
    /// otherwise push out the link the respondent was first mailed, which still works and which they may be answering
    /// through, for a link nobody holds. A link that nothing was ever heard of (the host stopped without reporting it) may
    /// have arrived, so it is kept as a reminder keeps any other, unless the one slot holds a link known to have
    /// arrived: that link is what the respondent surely holds, so it stays, and the link whose fate is unknown goes
    /// (T251 review). Were it the other way, a resend after a crash that lost a reminder's mail would retire the link the
    /// respondent was first mailed, and may be answering through, for a link that most likely never left.
    /// </para>
    /// </remarks>
    public void ReplaceLink(string tokenSelector, string tokenHash, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenSelector);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (LinkDeliveryFailed || PreviousLinkDeliveredOverUnreported)
        {
            TokenSelector = tokenSelector;
            TokenHash = tokenHash;
            IssuedOn = utcNow;
            return;
        }

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

        // What became of the last link's mail serves only a resend, which a campaign that takes no responses has no use
        // for (T251). A dropped mail is a fact about the address, so it goes with it. Cleared as the previous link is,
        // whether or not the address is still here.
        SentOn = null;
        DeliveryFailedOn = null;
        DeliveryLinkSelector = null;

        if (string.IsNullOrWhiteSpace(RespondentEmail))
        {
            return;
        }

        RespondentEmail = null;
        AnonymizedOn = utcNow;
    }

    // ─── What became of a link's mail (T251) ────────────────────────────────

    /// <summary>What an MSF link's mail carries so that the worker can report its outcome. (T251)</summary>
    private const string DeliveryKeyPrefix = "msf-link:";

    /// <summary>
    /// How long a link's mail may go unreported before the link counts as not delivered: the host stopped without
    /// reporting it, so it is not coming. (T251)
    /// </summary>
    /// <remarks>
    /// The queue lives in the web process. A host that stops in order reports what it still holds as dropped
    /// (<c>EmailWorker</c>), but not the mail it was sending at that moment, which may have arrived; one that crashes
    /// reports nothing. Their links would otherwise read as being sent for ever. An hour is well past what the worker
    /// takes over one mail (three attempts, each bounded by the mail server's timeout) and past a long queue of them. A
    /// link counted too soon, still queued, is only sent a second time: the first is kept as the previous link
    /// (<see cref="ReplaceLink" />), so whichever arrives works, unless the link before it is known to have arrived, which
    /// is then kept instead and works beside the new one.
    /// </remarks>
    public static readonly TimeSpan DeliveryReportDeadline = TimeSpan.FromHours(1);

    /// <summary>
    /// The key an MSF link's mail carries (<c>EmailMessage.DeliveryKey</c>), which the worker hands back with the mail's
    /// outcome: this invitation, and the link the mail carries. (T251)
    /// </summary>
    /// <remarks>
    /// Not a tag, and never logged: every log line about a mail names its recipient, and a log that paired an address with
    /// the invitation's row would name the respondent behind that row's answers long after anonymising erased the
    /// address, which is what T207 took out of the database.
    /// </remarks>
    public static string DeliveryKey(int invitationId, string linkSelector)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(invitationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(linkSelector);
        if (linkSelector.Contains(':', StringComparison.Ordinal))
        {
            throw new ArgumentException("A link selector holds no colon.", nameof(linkSelector));
        }

        return string.Create(CultureInfo.InvariantCulture, $"{DeliveryKeyPrefix}{invitationId}:{linkSelector}");
    }

    /// <summary>The invitation and link a <see cref="DeliveryKey" /> names; false for any other key, or none. (T251)</summary>
    public static bool TryReadDeliveryKey(string? key, out int invitationId, out string linkSelector)
    {
        invitationId = 0;
        linkSelector = string.Empty;

        if (key is null || !key.StartsWith(DeliveryKeyPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = key[DeliveryKeyPrefix.Length..].Split(':');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var id) ||
            id <= 0 ||
            string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        invitationId = id;
        linkSelector = parts[1];
        return true;
    }

    /// <summary>
    /// Whether the mail carrying the current link was reported dropped: nobody holds that link. (T251)
    /// </summary>
    public bool LinkDeliveryFailed
        => TokenSelector is not null && DeliveryLinkSelector == TokenSelector && DeliveryFailedOn is not null;

    /// <summary>
    /// Whether the outcome on record is about the previous link, and says it was sent, while nothing has been heard of the
    /// current link's mail: the previous link is the one the respondent is known to hold. (T251 review)
    /// </summary>
    /// <remarks>
    /// The last report wins (<c>MsfLinkDeliveryRecorder</c>), so the outcome still naming the previous link means that no
    /// report about the current one has landed.
    /// </remarks>
    private bool PreviousLinkDeliveredOverUnreported
        => PreviousTokenSelector is not null &&
           DeliveryLinkSelector == PreviousTokenSelector &&
           DeliveryLinkSelector != TokenSelector &&
           SentOn is not null;

    /// <summary>
    /// The invitations whose current link did not reach their respondent, and who can still answer through a new one: the
    /// ones the campaign page counts as not delivered and its Resend sends again (<c>ResendMsfLinksCommand</c>). (T251)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A link is not delivered when the worker reported its mail dropped, or when nothing was reported of it within
    /// <see cref="DeliveryReportDeadline" /> of its issue. An outcome about another link, one this invitation no longer
    /// holds or does not hold yet, says nothing about the current one.
    /// </para>
    /// <para>
    /// Only while a new link could still be answered: the campaign is open, the respondent has not answered, has not been
    /// revoked and has not been anonymised, and their last day to respond (<see cref="LastDayToRespond" />) has not
    /// passed. A draft's invitees hold no link yet.
    /// </para>
    /// <para>
    /// An expression, so the page's count and the resend are one statement each and read no row per invitee (T217):
    /// the page is sent a number, never who. It never names the address, not even to ask whether one is held, so the
    /// page's count sends no statement that mentions an address (<c>MsfCampaignScopePostgresTests</c>); an open campaign's
    /// invitations all hold one until anonymising erases it.
    /// </para>
    /// </remarks>
    public static Expression<Func<MsfInvitation, bool>> LinkNotDelivered(DateTime utcNow)
    {
        var reportDue = utcNow - DeliveryReportDeadline;
        return AwaitingAnswerAnd(utcNow, invitation =>
            invitation.DeliveryLinkSelector == invitation.TokenSelector
                ? invitation.DeliveryFailedOn != null
                : invitation.IssuedOn < reportDue);
    }

    /// <summary>
    /// The invitations whose current link's mail has not been reported on yet, and is not overdue: still being sent.
    /// Asked of the same invitations as <see cref="LinkNotDelivered" />. (T251)
    /// </summary>
    public static Expression<Func<MsfInvitation, bool>> LinkBeingSent(DateTime utcNow)
    {
        var reportDue = utcNow - DeliveryReportDeadline;
        return AwaitingAnswerAnd(utcNow, invitation =>
            invitation.DeliveryLinkSelector != invitation.TokenSelector && invitation.IssuedOn >= reportDue);
    }

    /// <summary>
    /// <paramref name="delivery" />, asked only of an invitation that still awaits an answer through the link it holds:
    /// the rule <see cref="LinkNotDelivered" /> and <see cref="LinkBeingSent" /> share. (T251)
    /// </summary>
    private static Expression<Func<MsfInvitation, bool>> AwaitingAnswerAnd(
        DateTime utcNow, Expression<Func<MsfInvitation, bool>> delivery)
    {
        var today = DateOnly.FromDateTime(utcNow);
        Expression<Func<MsfInvitation, bool>> awaiting = invitation =>
            invitation.Campaign.State == MsfCampaignState.Open &&
            invitation.Campaign.ClosesOn >= today &&
            invitation.ExpiresOn >= today &&
            invitation.RespondedOn == null &&
            invitation.RevokedOn == null &&
            invitation.AnonymizedOn == null &&
            invitation.TokenSelector != null;

        var parameter = awaiting.Parameters[0];
        var deliveryBody = new ParameterSwap(delivery.Parameters[0], parameter).Visit(delivery.Body);
        return Expression.Lambda<Func<MsfInvitation, bool>>(Expression.AndAlso(awaiting.Body, deliveryBody), parameter);
    }

    /// <summary>Puts one lambda's parameter in place of another's, so two bodies can be joined into one lambda.</summary>
    private sealed class ParameterSwap(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
