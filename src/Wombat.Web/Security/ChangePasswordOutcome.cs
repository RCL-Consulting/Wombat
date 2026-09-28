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
/// The endpoint answers with a redirect back to <see cref="PagePath" /> when it refuses, carrying what happened in the
/// address. A refusal travels as codes, never as words: Identity's error codes and the endpoint's own. The page shows each
/// code it knows in the server's own words, and one general sentence for any other, so a crafted link cannot put words of
/// its choosing on the page. A change goes to My account, which says the password was updated (<see cref="UpdatedUrl" />),
/// and the fifth wrong current password to the sign-in page (<see cref="LockedSignedOut" />) (T339, flow 02).
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

    /// <summary>
    /// My account's <c>status</c> once the password has changed: "Password updated." there (T339, flow 02). Until T339 the
    /// endpoint came back to this page with <c>?status=updated</c>.
    /// </summary>
    public const string Updated = "password-updated";

    /// <summary>A field of the form was left blank.</summary>
    public const string FieldsMissing = "FieldsMissing";

    /// <summary>The new password and its confirmation differ.</summary>
    public const string ConfirmationMismatch = "ConfirmationMismatch";

    /// <summary>A refusal that is not the user's to put right, or a code the page does not know.</summary>
    public const string Failed = "Failed";

    /// <summary>
    /// The sign-in page's notice once a lockout has signed the session out (T339, flow 02, E2): the fifth wrong current
    /// password locks the account, as the sign-in page's fifth wrong password does, and the session that guessed is ended
    /// with it, so a stolen cookie cannot wait the lock out and guess again. Until T339 the lockout came back to this page
    /// as a refusal, and the session went on. The Remove dialog's password check on My account ends the same way. Only a
    /// lock the post's own check trips: a post made while the account is locked already is <see cref="AccountLocked" />.
    /// </summary>
    public const string LockedSignedOut = "LockedSignedOut";

    /// <summary>
    /// The account was locked already when the form was posted: by someone else's guesses at the sign-in page, say. No
    /// password is checked, nothing changes, and the session goes on; the page says how long to wait
    /// (<see cref="AccountLockedMessage" />). Until the review of the t339 branch such a post was taken for the fifth wrong
    /// guess: Identity answers a locked account's check with a lockout without checking the password, so the owner's own
    /// change ended every session of the account and told them they had typed their password wrong.
    /// </summary>
    public const string AccountLocked = "AccountLocked";

    /// <summary>
    /// The address's parameter carrying <see cref="AccountLocked" />'s wait, in whole minutes. The page holds it to the
    /// lockout Identity is configured with (<see cref="LockedFor" />), so a crafted link can shorten the wait it says but
    /// never lengthen it.
    /// </summary>
    public const string MinutesParameter = "minutes";

    /// <summary>The address has used up the sign-in throttle, which this form's password check shares.</summary>
    public const string TooManyAttempts = "TooManyAttempts";

    /// <summary>The account signs in through its institution (SSO), so it has no password of its own to change here.</summary>
    public const string InstitutionalSignIn = "InstitutionalSignIn";

    /// <summary>What the page says for <see cref="Failed" /> and for any code it does not know.</summary>
    public const string GeneralRefusal = "The password could not be changed. Try again.";

    /// <summary>
    /// What a refusal the person can put right on the page begins with: a wrong current password, a blank field, a
    /// confirmation that differs, or a rule broken (T339, flow 02).
    /// </summary>
    public const string NotChanged = "Your password was not changed.";

    /// <summary>What the page says for <see cref="ConfirmationMismatch" />, in the refusal and under Confirm new password.</summary>
    public const string ConfirmationMismatchMessage = "The password confirmation does not match.";

    /// <summary>What the page says for <see cref="TooManyAttempts" />.</summary>
    public const string TooManyAttemptsMessage = "Too many attempts from this network. Wait a few minutes and try again.";

    /// <summary>What the page says for <see cref="InstitutionalSignIn" />.</summary>
    public const string InstitutionalSignInMessage =
        "This account signs in through your institution, so it has no password to change here.";

    /// <summary>
    /// What the sign-in page says for <see cref="SignInOutcome.SessionEnded" />: a session that had already ended when it
    /// asked for a change. Its cookie carried a security stamp the account no longer has (a lock, a change made in another
    /// browser, a change of roles). No change is made, and the cookie is taken away rather than issued again. The same
    /// words follow a tab whose circuit found its sign-in ended (<see cref="SessionEnd" />, the T279 review).
    /// </summary>
    public const string SessionEndedMessage = "Your session has ended. Sign in again.";

    /// <summary>
    /// What the sign-in page says for <see cref="SignInOutcome.PasswordChanged" />: the password was changed but the cookie
    /// could not be issued again. The old cookie's stamp is stale, so it is taken away, and the user signs in with the new
    /// password.
    /// </summary>
    public const string ChangedSignInAgainMessage = "Your password was changed. Sign in with your new password.";

    /// <summary>
    /// What the sign-in page says for <see cref="LockedSignedOut" />, with the lockout's length as Identity is configured
    /// (<see cref="LockoutOptions.DefaultLockoutTimeSpan" />), never a number written here (T339, flow 02, E2 and the round 3
    /// check). It names no account, so a crafted link says nothing about anyone.
    /// </summary>
    public static string LockedSignedOutMessage(TimeSpan lockout)
    {
        var span = LockoutWords(lockout);
        return "Your current password was entered incorrectly too many times, so your account is locked for " + span +
               " and you have been signed out. Wait " + span + ", then sign in again.";
    }

    /// <summary>
    /// What the page says for <see cref="AccountLocked" />, and the Remove dialog for its own
    /// (<see cref="ProfileOutcome.RemoveAccountLocked" />): the wait in whole minutes, rounded up. It names no account.
    /// </summary>
    public static string AccountLockedMessage(TimeSpan lockedFor)
        => $"Your account is locked. Wait {LockoutWords(lockedFor)}, then try again.";

    /// <summary>The page, refusing because the account is locked already, for <paramref name="lockedFor" /> more.</summary>
    public static string LockedUrl(TimeSpan lockedFor)
        => $"{PagePath}?error={AccountLocked}&{MinutesParameter}={Minutes(lockedFor)}";

    /// <summary>
    /// The wait an address says (<see cref="MinutesParameter" />), held to <paramref name="lockout" />, the lockout Identity
    /// is configured with: that lockout when the address says none, or a number that is not a wait.
    /// </summary>
    public static TimeSpan LockedFor(string? minutes, TimeSpan lockout)
        => int.TryParse(minutes, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var sent)
           && sent >= 1 && sent <= Minutes(lockout)
            ? TimeSpan.FromMinutes(sent)
            : lockout;

    /// <summary>My account, saying the password was updated.</summary>
    public static string UpdatedUrl { get; } = $"{ProfileOutcome.PagePath}?status={Updated}";

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
    /// The sentences the page shows for the codes it was sent back with, in reading order (<see cref="PasswordRefusal.Flat" />
    /// of <see cref="Refusal" />).
    /// </summary>
    public static IReadOnlyList<string> Describe(
        IEnumerable<string>? codes,
        IdentityErrorDescriber describer,
        PasswordOptions rules)
        => Refusal(codes, describer, rules).Flat;

    /// <summary>
    /// What the page shows for the codes it was sent back with (T339, flow 02): <see cref="NotChanged" /> first when the
    /// person can put the refusal right here; each other code's sentence once, in the order sent, and
    /// <see cref="GeneralRefusal" /> for any code this list does not name; and the password rules broken, in the one order
    /// the page lists them in (<see cref="PasswordRuleMessages" />), whatever order Identity sent them in.
    /// </summary>
    /// <param name="codes">The codes the page was sent back with.</param>
    /// <param name="describer">Identity's error words (Wombat's).</param>
    /// <param name="rules">The password rules Identity is configured with.</param>
    /// <param name="lockedFor">
    /// <see cref="AccountLocked" />'s wait, as the page reads it (<see cref="LockedFor" />); Identity's own default lockout
    /// for a caller that does not say.
    /// </param>
    public static PasswordRefusal Refusal(
        IEnumerable<string>? codes,
        IdentityErrorDescriber describer,
        PasswordOptions rules,
        TimeSpan? lockedFor = null)
    {
        ArgumentNullException.ThrowIfNull(describer);
        ArgumentNullException.ThrowIfNull(rules);

        var sent = (codes ?? []).Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
        if (sent.Count == 0)
        {
            return PasswordRefusal.None;
        }

        var sentences = new List<string>();
        var puttable = false;
        foreach (var code in sent.Where(code => !PasswordRuleMessages.IsRule(code)))
        {
            var sentence = code switch
            {
                FieldsMissing => "Enter your current password, a new password, and the new password again to confirm it.",
                ConfirmationMismatch => ConfirmationMismatchMessage,
                TooManyAttempts => TooManyAttemptsMessage,
                InstitutionalSignIn => InstitutionalSignInMessage,
                AccountLocked => AccountLockedMessage(lockedFor ?? new LockoutOptions().DefaultLockoutTimeSpan),
                nameof(IdentityErrorDescriber.PasswordMismatch) => describer.PasswordMismatch().Description,
                _ => GeneralRefusal
            };

            puttable |= code is FieldsMissing or ConfirmationMismatch or nameof(IdentityErrorDescriber.PasswordMismatch);
            if (!sentences.Contains(sentence, StringComparer.Ordinal))
            {
                sentences.Add(sentence);
            }
        }

        var broken = PasswordRuleMessages.Broken(sent, rules);
        return new PasswordRefusal(puttable || broken.Count > 0 ? NotChanged : null, sentences, broken);
    }

    /// <summary>"15 minutes": a lockout's length in words, in whole minutes, rounded up, and never under one.</summary>
    internal static string LockoutWords(TimeSpan lockout)
    {
        var minutes = Minutes(lockout);
        return minutes == 1 ? "1 minute" : $"{minutes} minutes";
    }

    /// <summary>A lockout's length in whole minutes, rounded up, and never under one.</summary>
    internal static int Minutes(TimeSpan lockout) => Math.Max(1, (int)Math.Ceiling(lockout.TotalMinutes));
}
