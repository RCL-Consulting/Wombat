namespace Wombat.Application.Common.Options;

public static class WombatOptionsExtensions
{
    /// <summary>
    /// The absolute base URL an MSF respondent is sent to, with any trailing slash removed.
    /// </summary>
    /// <remarks>
    /// One implementation, because two callers build a respondent link and they must not drift:
    /// <c>OpenMsfCampaignCommandHandler</c> when the campaign opens, and
    /// <c>MsfInvitationExpiryReminderJob</c> when it re-issues a token before expiry (T132).
    /// A relative URL is not clickable in a mail client, so "absolute" is a correctness
    /// requirement rather than a preference — the reminder job shipped with a hardcoded
    /// <c>/msf/respond</c> and nobody could follow it.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// The option is missing, blank, or not an absolute URL. Thrown rather than returning null:
    /// every caller is about to email a link, and a link nobody can follow is worse than a
    /// refusal that says why.
    /// </exception>
    public static string RequireMsfRespondUrl(this WombatOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.MsfRespondUrl))
        {
            throw new InvalidOperationException(
                "Wombat:MsfRespondUrl must be configured before an MSF respondent link can be sent.");
        }

        if (!Uri.TryCreate(options.MsfRespondUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("Wombat:MsfRespondUrl must be an absolute URL.");
        }

        return uri.ToString().TrimEnd('/');
    }
}
