using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// The sign-in page's address with a refusal in it, and the words the page shows for each refusal (T285).
/// </summary>
/// <remarks>
/// <para>
/// Sign-in, the institutional sign-in's callback, the link page's lockout and change password all send the browser back
/// to the sign-in page when they refuse or end a session, with what happened in <c>?error=</c>. Until T285 that was the
/// sentence itself, which the page printed as it arrived: a crafted link could put any words on Wombat's own sign-in page
/// (a phone number to call, say). HTML-encoded, so text and not script, but the page's words all the same.
/// </para>
/// <para>
/// Now the address carries a code, and the page chooses the sentence (<see cref="Describe" />), as the change-password
/// page has since T265 (<see cref="ChangePasswordOutcome" />). A code the page does not know gets
/// <see cref="GeneralRefusal" />, whatever it says.
/// </para>
/// </remarks>
public static class SignInOutcome
{
    /// <summary>The sign-in page.</summary>
    public const string PagePath = "/account/login";

    /// <summary>The email or the password was left blank.</summary>
    public const string FieldsMissing = "FieldsMissing";

    /// <summary>
    /// A wrong password, an address no account has, or an account that signs in only through its institution: all one
    /// refusal, so the page says nothing about which addresses have accounts (T156). Its words depend on whether the page
    /// offers institutional sign-in (<see cref="SignInMessages.Refused" />), which the page knows for itself.
    /// </summary>
    public const string Refused = "Refused";

    /// <summary>The client has used up the sign-in throttle (<see cref="SignInThrottle" />).</summary>
    public const string TooManyAttempts = "TooManyAttempts";

    /// <summary>Identity's lockout refused the account.</summary>
    public const string LockedOut = "LockedOut";

    /// <summary>
    /// The session had ended: a change of password was asked for by a session that already had (T265 review), or a tab's
    /// circuit found its sign-in ended and the browser's session was signed out (<see cref="SessionEnd" />, the T279
    /// review).
    /// </summary>
    public const string SessionEnded = "SessionEnded";

    /// <summary>The password was changed, but the sign-in cookie could not be issued again (T265).</summary>
    public const string PasswordChanged = "PasswordChanged";

    /// <summary>The institutional sign-in's callback found no external login to read.</summary>
    public const string ExternalLoginUnavailable = "ExternalLoginUnavailable";

    /// <summary>The link page was posted after the institutional sign-in in progress had ended.</summary>
    public const string ExternalSessionExpired = "ExternalSessionExpired";

    /// <summary>The institutional sign-in failed with no reason given.</summary>
    public const string SsoFailed = "SsoFailed";

    /// <summary>What the page says for any code it does not know.</summary>
    public const string GeneralRefusal = "Sign-in could not be completed. Please try again.";

    /// <summary>What the page says for <see cref="FieldsMissing" />.</summary>
    public const string FieldsMissingMessage = "Email and password are required.";

    /// <summary>What the page says for <see cref="LockedOut" />.</summary>
    public const string LockedOutMessage =
        "Too many failed sign-in attempts. Please try again later or reset your password.";

    /// <summary>What the page says for <see cref="ExternalLoginUnavailable" />.</summary>
    public const string ExternalLoginUnavailableMessage = "External login information was not available.";

    /// <summary>What the page says for <see cref="ExternalSessionExpired" />.</summary>
    public const string ExternalSessionExpiredMessage = "External login session expired. Please try again.";

    /// <summary>What the page says for <see cref="SsoFailed" />.</summary>
    public const string SsoFailedMessage = "SSO login failed.";

    /// <summary>The sign-in page, refusing with <paramref name="code" />, and coming back to <paramref name="returnUrl" />.</summary>
    public static string Url(string code, string? returnUrl = null)
    {
        var query = $"error={Uri.EscapeDataString(code)}";

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            query += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        return $"{PagePath}?{query}";
    }

    /// <summary>
    /// The sentence the page shows for <paramref name="code" />: its own for each code above, the institutional sign-in's
    /// for each <see cref="ExternalLoginRefusal" /> code, and <see cref="GeneralRefusal" /> for anything else. Null when
    /// there is no code, so the page shows no refusal.
    /// </summary>
    public static string? Describe(string? code, bool institutionalSignInOffered)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return code switch
        {
            FieldsMissing => FieldsMissingMessage,
            Refused => SignInMessages.Refused(institutionalSignInOffered),
            TooManyAttempts => SignInMessages.TooManyFailedAttempts,
            LockedOut => LockedOutMessage,
            SessionEnded => ChangePasswordOutcome.SessionEndedMessage,
            PasswordChanged => ChangePasswordOutcome.ChangedSignInAgainMessage,
            ExternalLoginUnavailable => ExternalLoginUnavailableMessage,
            ExternalSessionExpired => ExternalSessionExpiredMessage,
            SsoFailed => SsoFailedMessage,
            _ => ExternalLoginRefusal.Describe(code) ?? GeneralRefusal
        };
    }
}
