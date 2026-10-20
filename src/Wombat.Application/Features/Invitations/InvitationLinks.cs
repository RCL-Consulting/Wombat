using Wombat.Application.Common.Options;

namespace Wombat.Application.Features.Invitations;

/// <summary>The link an account invitation's mail carries. (T283)</summary>
public static class InvitationLinks
{
    /// <summary>
    /// The registration link with <paramref name="token" />, under the configured base URL. It opens the start path, which
    /// keeps the token in the register cookie and answers 303 to the page with no query, so the token leaves the address
    /// bar by a redirect, not by script (T373, flow 11, S2; T315). The Web's <c>RegisterOutcome.StartPath</c> is held equal
    /// to it by a test.
    /// </summary>
    public static string RegistrationUrl(WombatOptions options, string token)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var baseUrl = (options.BaseUrl ?? string.Empty).TrimEnd('/');
        return $"{baseUrl}/account/register/start?token={Uri.EscapeDataString(token)}";
    }
}
