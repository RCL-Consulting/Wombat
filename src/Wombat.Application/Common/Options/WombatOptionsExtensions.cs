namespace Wombat.Application.Common.Options;

public static class WombatOptionsExtensions
{
    /// <summary>
    /// The path of the MSF respondent page on the web app (<c>MsfRespond.razor</c>, T205). A test holds the page's route to
    /// it.
    /// </summary>
    public const string MsfRespondPath = "/msf/respond";

    /// <summary>
    /// The absolute base URL an MSF respondent is sent to, with any trailing slash removed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One implementation, because two callers build a respondent link and they must not drift:
    /// <c>OpenMsfCampaignCommandHandler</c> when the campaign opens, and
    /// <c>MsfInvitationExpiryReminderJob</c> when it re-issues a token before expiry (T132).
    /// A relative URL is not clickable in a mail client, so "absolute" is a correctness
    /// requirement rather than a preference — the reminder job shipped with a hardcoded
    /// <c>/msf/respond</c> and nobody could follow it.
    /// </para>
    /// <para>
    /// Unset, it is the web app's own page, <c>{Wombat:BaseUrl}/msf/respond</c> (T205). Set, it must be on BaseUrl's scheme,
    /// host and port, when BaseUrl is set: the page is the web app's, so a link anywhere else is a mistake, and one that
    /// would open a campaign without complaint and mail every respondent a link nobody can use. The Api host's address
    /// (port 5090, where dev pointed until T205), <c>http</c> for an <c>https</c> site and another host are all refused.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Neither the option nor BaseUrl is set, the option is not an absolute http(s) URL, or it is not on BaseUrl's
    /// origin. Thrown rather than returning null: every caller is about to email a link, and a link nobody can follow is
    /// worse than a refusal that says why.
    /// </exception>
    public static string RequireMsfRespondUrl(this WombatOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var baseUri = TryParseWebUrl(options.BaseUrl);

        if (string.IsNullOrWhiteSpace(options.MsfRespondUrl))
        {
            return baseUri is not null
                ? baseUri.ToString().TrimEnd('/') + MsfRespondPath
                : throw new InvalidOperationException(
                    "Wombat:MsfRespondUrl, or Wombat:BaseUrl that it defaults from, must be configured before an MSF " +
                    "respondent link can be sent.");
        }

        var uri = TryParseWebUrl(options.MsfRespondUrl)
            ?? throw new InvalidOperationException("Wombat:MsfRespondUrl must be an absolute http or https URL.");

        if (baseUri is not null &&
            Uri.Compare(uri, baseUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) != 0)
        {
            throw new InvalidOperationException(
                $"Wombat:MsfRespondUrl ({uri.GetLeftPart(UriPartial.Authority)}) must be on Wombat:BaseUrl's scheme, host " +
                $"and port ({baseUri.GetLeftPart(UriPartial.Authority)}): the MSF respondent page is the web app's own " +
                $"(\"{MsfRespondPath}\"). Leave it unset to use {baseUri.ToString().TrimEnd('/')}{MsfRespondPath}.");
        }

        return uri.ToString().TrimEnd('/');
    }

    /// <summary>
    /// An absolute http or https URL, or null. Not merely absolute: on Linux, where the server runs, a path such as
    /// <c>/msf/respond</c> parses as an absolute <c>file://</c> URL.
    /// </summary>
    private static Uri? TryParseWebUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            ? uri
            : null;
}
