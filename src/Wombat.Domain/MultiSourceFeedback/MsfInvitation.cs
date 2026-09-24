using System.Security.Cryptography;
using System.Text;

namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfInvitation
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string? RespondentEmail { get; set; }
    public string? RespondentEmailHash { get; set; }
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
    /// answer from each learner would count spellings. It is kept when the campaign closes and the address is hashed:
    /// it names a teaching session, not a person, and no report breaks answers down by it.
    /// </para>
    /// </remarks>
    public string? TeachingContext { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// When the link <see cref="TokenHash" /> verifies was issued: when the invitation was added, then when the campaign
    /// opened and mailed it, then when a reminder replaced it (<see cref="IssueLink" />, T206).
    /// </summary>
    /// <remarks>
    /// The reminder is sent once per link, and this is what says so (<see cref="IsReminderDue" />). A link issued on or
    /// after the first reminder day is not replaced by a reminder: its email already named the same last day, and a
    /// reminder would only retire it.
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
    /// A reminder retires the link it replaces, and a respondent part-way through the questionnaire on that link loses
    /// what they typed when they submit (the page cannot tell a replaced link from a mistyped one). That is likeliest
    /// just after the invitation arrives, so a link mailed less than a day before the job runs is left alone: a campaign
    /// opened the evening before the first reminder day is reminded a day later, not the next morning. A day is the job's
    /// own cadence, so the reminder comes at most one run later than it otherwise would.
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
    /// Stores the hash of a newly issued link, which retires the one before it, and when it was issued. Opening the
    /// campaign issues the first link a respondent is sent, and a reminder replaces it. (T206)
    /// </summary>
    public void IssueLink(string tokenHash, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        TokenHash = tokenHash;
        IssuedOn = utcNow;
    }

    /// <summary>
    /// Replaces the respondent's address with a one-way hash of it. An invitation already anonymised is left as it
    /// was, so its first <see cref="AnonymizedOn" /> stands.
    /// </summary>
    /// <remarks>
    /// The one place a respondent is anonymised. <see cref="MsfCampaign.Close" /> calls it for every invitation, and
    /// both ways a campaign closes (the coordinator's close command and the hourly auto-close job) go through that, so
    /// the two cannot drift apart. Until T184 the job carried its own copy of this routine. <see cref="MsfCampaign.Withdraw" />
    /// calls it too (T202).
    /// </remarks>
    public void Anonymize(DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(RespondentEmail))
        {
            return;
        }

        RespondentEmailHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(RespondentEmail.Trim().ToUpperInvariant())));
        RespondentEmail = null;
        AnonymizedOn = utcNow;
    }
}
