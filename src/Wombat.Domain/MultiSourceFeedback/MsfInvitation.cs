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
    /// the two cannot drift apart. Until T184 the job carried its own copy of this routine.
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
