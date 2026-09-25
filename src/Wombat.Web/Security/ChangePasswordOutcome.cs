using Microsoft.AspNetCore.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// Where the change-password form posts, where the answer goes, and the words the page shows for it (T265).
/// </summary>
/// <remarks>
/// <para>
/// The password is changed by <see cref="SubmitPath" />, an endpoint in <c>Program.cs</c> that the page's form posts to,
/// as sign-in, register and link-account post to theirs. Not in the page's circuit: changing a password changes the
/// account's security stamp, which the sign-in cookie carries, so the cookie is issued again in the same request, and a
/// circuit's response started long before. Until T265 the page called <c>RefreshSignInAsync</c> in its circuit, which
/// threw "Headers are read-only" after the password had changed, and the page crashed.
/// </para>
/// <para>
/// The endpoint answers with a redirect back to <see cref="PagePath" />, carrying what happened in the address. A refusal
/// travels as codes, never as words: Identity's error codes and two of the endpoint's own. The page shows each code it
/// knows in the server's own words, and one general sentence for any other, so a crafted link cannot put words of its
/// choosing on the page.
/// </para>
/// </remarks>
public static class ChangePasswordOutcome
{
    /// <summary>The change-password page.</summary>
    public const string PagePath = "/account/change-password";

    /// <summary>The endpoint the page's form posts to.</summary>
    public const string SubmitPath = "/account/change-password/submit";

    /// <summary>The endpoint's log category: a fault in a change is logged, and the user is told in words of the page's.</summary>
    public const string LogCategory = "Wombat.Web.Account.ChangePassword";

    /// <summary>The page's <c>status</c> once the password has changed.</summary>
    public const string Updated = "updated";

    /// <summary>A field of the form was left blank.</summary>
    public const string FieldsMissing = "FieldsMissing";

    /// <summary>The new password and its confirmation differ.</summary>
    public const string ConfirmationMismatch = "ConfirmationMismatch";

    /// <summary>A refusal that is not the user's to put right, or a code the page does not know.</summary>
    public const string Failed = "Failed";

    /// <summary>
    /// The account is locked, by Identity's lockout after too many wrong passwords (this form's or the sign-in page's).
    /// The current password is not checked while it lasts. The session goes on: a lockout is not a deactivation, which
    /// changes the security stamp and so ends the session instead.
    /// </summary>
    public const string LockedOut = "LockedOut";

    /// <summary>The address has used up the sign-in throttle, which this form's password check shares.</summary>
    public const string TooManyAttempts = "TooManyAttempts";

    /// <summary>The account signs in through its institution (SSO), so it has no password of its own to change here.</summary>
    public const string InstitutionalSignIn = "InstitutionalSignIn";

    /// <summary>What the page says for <see cref="Failed" /> and for any code it does not know.</summary>
    public const string GeneralRefusal = "The password could not be changed. Please try again.";

    /// <summary>What the page says for <see cref="LockedOut" />.</summary>
    public const string LockedOutMessage =
        "Too many incorrect passwords, so the account is locked for a few minutes. Please try again later.";

    /// <summary>What the page says for <see cref="TooManyAttempts" />.</summary>
    public const string TooManyAttemptsMessage = "Too many attempts from this network. Please wait a few minutes and try again.";

    /// <summary>What the page says for <see cref="InstitutionalSignIn" />.</summary>
    public const string InstitutionalSignInMessage =
        "This account signs in through your institution, so it has no password to change here.";

    /// <summary>
    /// What the sign-in page says for <see cref="SignInOutcome.SessionEnded" />: a session that had already ended when it
    /// asked for a change. Its cookie carried a security stamp the account no longer has (a lock, a change made in another
    /// browser, a change of roles). No change is made, and the cookie is taken away rather than issued again.
    /// </summary>
    public const string SessionEndedMessage = "Your session has ended. Please sign in again.";

    /// <summary>
    /// What the sign-in page says for <see cref="SignInOutcome.PasswordChanged" />: the password was changed but the cookie
    /// could not be issued again. The old cookie's stamp is stale, so it is taken away, and the user signs in with the new
    /// password.
    /// </summary>
    public const string ChangedSignInAgainMessage = "Your password was changed. Please sign in with your new password.";

    /// <summary>The page, saying the password was changed.</summary>
    public static string UpdatedUrl { get; } = $"{PagePath}?status={Updated}";

    /// <summary>The page, refusing with <paramref name="codes" />; <see cref="Failed" /> when there are none.</summary>
    public static string RefusedUrl(IEnumerable<string> codes)
    {
        var distinct = codes.Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.Ordinal).ToList();
        if (distinct.Count == 0)
        {
            distinct.Add(Failed);
        }

        return $"{PagePath}?{string.Join("&", distinct.Select(code => $"error={Uri.EscapeDataString(code)}"))}";
    }

    /// <summary>
    /// The sentences the page shows for the codes it was sent back with, each once, in the order sent: Identity's own
    /// description for each password rule, from the rules this app sets, and <see cref="GeneralRefusal" /> for any code
    /// this list does not name.
    /// </summary>
    public static IReadOnlyList<string> Describe(
        IEnumerable<string>? codes,
        IdentityErrorDescriber describer,
        PasswordOptions rules)
    {
        var sentences = new List<string>();
        foreach (var code in codes ?? [])
        {
            var sentence = code switch
            {
                FieldsMissing => "Enter your current password, a new password, and the new password again to confirm it.",
                ConfirmationMismatch => "The password confirmation does not match.",
                LockedOut => LockedOutMessage,
                TooManyAttempts => TooManyAttemptsMessage,
                InstitutionalSignIn => InstitutionalSignInMessage,
                nameof(IdentityErrorDescriber.PasswordMismatch) => describer.PasswordMismatch().Description,
                _ => PasswordRuleMessages.Describe(code, describer, rules) ?? GeneralRefusal
            };

            if (!sentences.Contains(sentence, StringComparer.Ordinal))
            {
                sentences.Add(sentence);
            }
        }

        return sentences;
    }
}
