using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// The sign-in page's address with a refusal or a notice in it, and the words the page shows for each (T285; T339, flow
/// 02).
/// </summary>
/// <remarks>
/// <para>
/// Sign-in, the institutional sign-in's callback, the link page's lockout, change password and sign-out all send the
/// browser back to the sign-in page when they refuse or end a session, with what happened in <c>?error=</c>. Until T285
/// that was the sentence itself, which the page printed as it arrived: a crafted link could put any words on Wombat's own
/// sign-in page (a phone number to call, say). HTML-encoded, so text and not script, but the page's words all the same.
/// </para>
/// <para>
/// Now the address carries a code, and the page chooses the sentence (<see cref="Describe" />), as the change-password
/// page has since T265 (<see cref="ChangePasswordOutcome" />). A code the page does not know gets
/// <see cref="GeneralRefusal" />, whatever it says.
/// </para>
/// <para>
/// Four codes are notices, not refusals (<see cref="IsNotice" />): the session ended, the password changed elsewhere, the
/// person signed out, and a lockout signed them out. Nothing the person did on this page was refused, so the page shows
/// them as information, not as an error (T339, flow 02).
/// </para>
/// </remarks>
public static class SignInOutcome
{
    /// <summary>The sign-in page.</summary>
    public const string PagePath = "/account/login";

    /// <summary>The email or the password was left blank.</summary>
    public const string FieldsMissing = "FieldsMissing";

    /// <summary>
    /// A wrong password, an address no account has, an account that signs in only through its institution, or a locked
    /// account: all one refusal, so the page says nothing about which addresses have accounts or which are locked (T156;
    /// T287 in T339, flow 02). Its words depend on whether the page offers institutional sign-in
    /// (<see cref="SignInMessages.Refused" />), which the page knows for itself.
    /// </summary>
    public const string Refused = "Refused";

    /// <summary>The client has used up the sign-in throttle (<see cref="SignInThrottle" />).</summary>
    public const string TooManyAttempts = "TooManyAttempts";

    /// <summary>
    /// Identity's lockout refused the account. No endpoint sends it since T339: a lockout is sent as
    /// <see cref="Refused" />, so its address is the same as a wrong password's (T287, the round 2 review's A1). A link that
    /// still carries it reads as <see cref="Refused" /> does.
    /// </summary>
    public const string LockedOut = "LockedOut";

    /// <summary>
    /// The session had ended: a change of password was asked for by a session that already had (T265 review), or a tab's
    /// circuit found its sign-in ended and the browser's session was signed out (<see cref="SessionEnd" />, the T279
    /// review). A notice.
    /// </summary>
    public const string SessionEnded = "SessionEnded";

    /// <summary>The password was changed, but the sign-in cookie could not be issued again (T265). A notice.</summary>
    public const string PasswordChanged = "PasswordChanged";

    /// <summary>The person signed out (T339, flow 02, B1). A notice.</summary>
    public const string SignedOut = "SignedOut";

    /// <summary>
    /// A lockout signed the session out: the fifth wrong current password on change password, or in the Remove dialog on My
    /// account (<see cref="ChangePasswordOutcome.LockedSignedOut" />, T339, flow 02, E2). A notice.
    /// </summary>
    public const string LockedSignedOut = ChangePasswordOutcome.LockedSignedOut;

    /// <summary>The institutional sign-in's callback found no external login to read.</summary>
    public const string ExternalLoginUnavailable = "ExternalLoginUnavailable";

    /// <summary>The link page was posted after the institutional sign-in in progress had ended.</summary>
    public const string ExternalSessionExpired = "ExternalSessionExpired";

    /// <summary>The institutional sign-in failed with no reason given.</summary>
    public const string SsoFailed = "SsoFailed";

    /// <summary>What the page says for any code it does not know.</summary>
    public const string GeneralRefusal = "Sign-in could not be completed. Try again.";

    /// <summary>What the page says for <see cref="FieldsMissing" />.</summary>
    public const string FieldsMissingMessage = "Enter your email and your password.";

    /// <summary>What the page says for <see cref="SignedOut" />.</summary>
    public const string SignedOutMessage = "You have signed out.";

    /// <summary>What the page says for <see cref="ExternalLoginUnavailable" /> when it offers no institution's button.</summary>
    public const string ExternalLoginUnavailableMessage =
        "Your institution's sign-in did not complete. Sign in with your email and password.";

    /// <summary>What the page says for <see cref="ExternalLoginUnavailable" /> beside its institutions' buttons.</summary>
    public const string ExternalLoginUnavailableWithButtonsMessage =
        "Your institution's sign-in did not complete. Try your institution's button again, or sign in with your email and " +
        "password.";

    /// <summary>What the page says for <see cref="ExternalSessionExpired" /> when it offers no institution's button.</summary>
    public const string ExternalSessionExpiredMessage =
        "Your institution's sign-in took too long and has expired. Sign in with your email and password.";

    /// <summary>What the page says for <see cref="ExternalSessionExpired" /> beside its institutions' buttons.</summary>
    public const string ExternalSessionExpiredWithButtonsMessage =
        "Your institution's sign-in took too long and has expired. Use your institution's button to start again.";

    /// <summary>What the page says for <see cref="SsoFailed" />.</summary>
    public const string SsoFailedMessage =
        "Wombat could not sign you in through your institution this time. Try again in a few minutes, or sign in with " +
        "your email and password.";

    /// <summary>The sign-in page, with <paramref name="code" />, and coming back to <paramref name="returnUrl" />.</summary>
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
    /// Whether <paramref name="code" /> is a notice (the page's info kind, <c>role="status"</c>) rather than a refusal (its
    /// danger kind, <c>role="alert"</c>): <see cref="SessionEnded" />, <see cref="PasswordChanged" />,
    /// <see cref="SignedOut" /> and <see cref="LockedSignedOut" />. Every other code, one the page does not know included,
    /// is a refusal (T339, flow 02).
    /// </summary>
    public static bool IsNotice(string? code)
        => code is SessionEnded or PasswordChanged or SignedOut or LockedSignedOut;

    /// <summary>
    /// The sentence the page shows for <paramref name="code" />: its own for each code above, the institutional sign-in's
    /// for each <see cref="ExternalLoginRefusal" /> code, and <see cref="GeneralRefusal" /> for anything else. Null when
    /// there is no code, so the page shows nothing.
    /// </summary>
    /// <param name="code">The code in the address.</param>
    /// <param name="institutionalSignInOffered">Whether the page offers an institution's button.</param>
    /// <param name="lockout">
    /// How long Identity's lockout lasts (<c>IdentityOptions.Lockout.DefaultLockoutTimeSpan</c>), which
    /// <see cref="LockedSignedOut" /> says.
    /// </param>
    public static string? Describe(string? code, bool institutionalSignInOffered, TimeSpan lockout)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return code switch
        {
            FieldsMissing => FieldsMissingMessage,
            Refused or LockedOut => SignInMessages.Refused(institutionalSignInOffered),
            TooManyAttempts => SignInMessages.TooManyFailedAttempts,
            SessionEnded => ChangePasswordOutcome.SessionEndedMessage,
            PasswordChanged => ChangePasswordOutcome.ChangedSignInAgainMessage,
            SignedOut => SignedOutMessage,
            LockedSignedOut => ChangePasswordOutcome.LockedSignedOutMessage(lockout),
            ExternalLoginUnavailable => institutionalSignInOffered
                ? ExternalLoginUnavailableWithButtonsMessage
                : ExternalLoginUnavailableMessage,
            ExternalSessionExpired => institutionalSignInOffered
                ? ExternalSessionExpiredWithButtonsMessage
                : ExternalSessionExpiredMessage,
            SsoFailed => SsoFailedMessage,
            _ => ExternalLoginRefusal.Describe(code) ?? GeneralRefusal
        };
    }
}
