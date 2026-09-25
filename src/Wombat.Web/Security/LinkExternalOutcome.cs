using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// The link-your-account page's address, with a refusal in it when there is one, and the words the page shows for each
/// refusal (T285). The link endpoint sends a code, never a sentence, as the sign-in endpoint does
/// (<see cref="SignInOutcome" />): until T285 the page printed whatever text its <c>?error=</c> carried.
/// </summary>
public static class LinkExternalOutcome
{
    /// <summary>The link-your-account page.</summary>
    public const string PagePath = "/account/link-external";

    /// <summary>The password was left blank.</summary>
    public const string PasswordRequired = "PasswordRequired";

    /// <summary>What the page says for <see cref="PasswordRequired" />.</summary>
    public const string PasswordRequiredMessage = "Your password is required.";

    /// <summary>What the page says for any code it does not know.</summary>
    public const string GeneralRefusal = "The account could not be linked. Please try again.";

    /// <summary>
    /// The page, coming back to <paramref name="returnUrl" /> once linked, and refusing with <paramref name="code" /> when
    /// there is one. No email: the page reads it from the sign-in in progress, so a crafted link cannot show one address
    /// while another is linked, and the address stays out of the proxy's access log (T149).
    /// </summary>
    public static string Url(string? returnUrl, string? code)
        => $"{PagePath}?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}" +
           (string.IsNullOrWhiteSpace(code) ? string.Empty : $"&error={Uri.EscapeDataString(code)}");

    /// <summary>
    /// The sentence the page shows for <paramref name="code" />: its own for <see cref="PasswordRequired" />, the
    /// institutional sign-in's for each <see cref="ExternalLoginRefusal" /> code, and <see cref="GeneralRefusal" /> for
    /// anything else. Null when there is no code, so the page shows no refusal.
    /// </summary>
    public static string? Describe(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return code == PasswordRequired
            ? PasswordRequiredMessage
            : ExternalLoginRefusal.Describe(code) ?? GeneralRefusal;
    }
}
