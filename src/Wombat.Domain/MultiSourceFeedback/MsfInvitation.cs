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
