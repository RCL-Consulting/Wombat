namespace Wombat.Web.Security;

/// <summary>
/// Where My account's forms post, where the answer goes, and the words the page shows for it (T335, flow 01; the review
/// of the t335 branch; T339, flow 02).
/// </summary>
/// <remarks>
/// <para>
/// The name is saved by <see cref="SubmitPath" />, an endpoint in <c>Program.cs</c> that the page's form posts to, as
/// change password's does (T265). The shell names the person from the sign-in cookie's <c>display_name</c> claim, so it
/// reads no database; a name saved in the page's circuit left that claim stale in the circuit and in the cookie, and the
/// account row went on showing the old name. The endpoint saves the name and issues the cookie again, in one request, and
/// sends the browser back to <see cref="PagePath" />, a full page load whose circuit starts from the new cookie. An
/// institutional sign-in is removed the same way, by <see cref="SignInMethodEndpoints.RemovePath" /> (T339).
/// </para>
/// <para>
/// A refusal travels as a code, never as words, and the page chooses the words (T285), so a crafted link cannot put words
/// of its choosing on the page. An institution is named by the provider's key in the address, and the page names it from
/// its own configuration, or not at all.
/// </para>
/// </remarks>
public static class ProfileOutcome
{
    /// <summary>My account.</summary>
    public const string PagePath = "/account/profile";

    /// <summary>The endpoint the page's name form posts to.</summary>
    public const string SubmitPath = "/account/profile/submit";

    /// <summary>The endpoints' log category: a fault in a save is logged, and the user is told in words of the page's.</summary>
    public const string LogCategory = "Wombat.Web.Account.Profile";

    // ---- statuses ----

    /// <summary>The page's <c>status</c> once the name is saved.</summary>
    public const string Saved = "saved";

    /// <summary>The page's <c>status</c> once change password has changed the password: its redirect's code (T339).</summary>
    public const string PasswordUpdated = ChangePasswordOutcome.Updated;

    /// <summary>The page's <c>status</c> once an institutional sign-in is removed; <c>provider</c> names it (T339).</summary>
    public const string SignInRemoved = "sign-in-removed";

    // ---- the name's refusals ----

    /// <summary>The first name was left blank; the last name was not.</summary>
    public const string FirstNameMissing = "FirstNameMissing";

    /// <summary>The last name was left blank; the first name was not.</summary>
    public const string LastNameMissing = "LastNameMissing";

    /// <summary>
    /// Both names were left blank: a crafted post, or two fields of spaces, since the browser's own check stops an empty
    /// one. Both fields are marked (T339).
    /// </summary>
    public const string NameMissing = "NameMissing";

    /// <summary>A name is longer than <see cref="MaxNameLength" /> characters.</summary>
    public const string NameTooLong = "NameTooLong";

    /// <summary>A fault, or a code the page does not know.</summary>
    public const string Failed = "Failed";

    // ---- the removal's refusals (T339) ----

    /// <summary>The password did not match. Shown inside the dialog, which the page opens again.</summary>
    public const string RemoveWrongPassword = "RemoveWrongPassword";

    /// <summary>The sign-in is the account's only way in: a crafted post, or a second tab's.</summary>
    public const string RemoveLastSignIn = "RemoveLastSignIn";

    /// <summary>A fault, a sign-in the account does not have (any more), or a second tab's removal in between.</summary>
    public const string RemoveFailed = "RemoveFailed";

    /// <summary>
    /// The sign-in throttle refused the password check, as it refuses change password's (T156). Not on the boards; the
    /// endpoint checks a password, so the throttle applies, and this says so inside the dialog.
    /// </summary>
    public const string RemoveTooManyAttempts = "RemoveTooManyAttempts";

    /// <summary>
    /// The account was locked already when Remove was posted, by someone else's guesses at the sign-in page, say: no
    /// password is checked, nothing changes, and the session goes on. Shown inside the dialog, with the wait
    /// (<see cref="ChangePasswordOutcome.MinutesParameter" />), as change password's <see cref="ChangePasswordOutcome.AccountLocked" />
    /// (the review of the t339 branch). Only a lock the Remove's own check trips signs the session out.
    /// </summary>
    public const string RemoveAccountLocked = "RemoveAccountLocked";

    /// <summary>The longest a first or last name may be, as the command's validator holds it.</summary>
    public const int MaxNameLength = 100;

    // ---- the words ----

    /// <summary>What the page says once the name is saved, changed or not (E10).</summary>
    public const string SavedMessage = "Name saved.";

    /// <summary>What the page says, under its header, when change password sends the browser back.</summary>
    public const string PasswordUpdatedMessage = "Password updated.";

    /// <summary>The first half of every refusal of the name.</summary>
    public const string NameNotSaved = "Your name was not saved.";

    /// <summary>What the page says for <see cref="Failed" /> and for any code it does not know.</summary>
    public const string GeneralRefusal = "Your name could not be saved. Try again.";

    /// <summary>What the first name's field says when it was left blank.</summary>
    public const string FirstNameMissingMessage = "Enter your first name.";

    /// <summary>What the last name's field says when it was left blank.</summary>
    public const string LastNameMissingMessage = "Enter your last name.";

    /// <summary>What the dialog's password field says after <see cref="RemoveWrongPassword" />.</summary>
    public const string IncorrectPasswordMessage = "Incorrect password.";

    /// <summary>What the page says when it cannot read the account.</summary>
    public const string LoadErrorMessage = "Could not load your account. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The page, saying the name was saved.</summary>
    public static string SavedUrl { get; } = $"{PagePath}?status={Saved}";

    /// <summary>The page, saying the password was changed.</summary>
    public static string PasswordUpdatedUrl { get; } = $"{PagePath}?status={PasswordUpdated}";

    /// <summary>The page, refusing the name with <paramref name="code" />.</summary>
    public static string RefusedUrl(string code) => $"{PagePath}?error={Uri.EscapeDataString(code)}";

    /// <summary>The page, saying the sign-in through <paramref name="provider" /> was removed.</summary>
    public static string SignInRemovedUrl(string provider)
        => $"{PagePath}?status={SignInRemoved}&provider={Uri.EscapeDataString(provider)}";

    /// <summary>The page, refusing the removal of the sign-in through <paramref name="provider" /> with <paramref name="code" />.</summary>
    public static string RemoveRefusedUrl(string code, string provider)
        => $"{PagePath}?error={Uri.EscapeDataString(code)}&provider={Uri.EscapeDataString(provider)}";

    /// <summary>
    /// The page, refusing the removal of the sign-in through <paramref name="provider" /> because the account is locked
    /// already, for <paramref name="lockedFor" /> more (<see cref="RemoveAccountLocked" />).
    /// </summary>
    public static string RemoveLockedUrl(string provider, TimeSpan lockedFor)
        => $"{RemoveRefusedUrl(RemoveAccountLocked, provider)}&{ChangePasswordOutcome.MinutesParameter}={ChangePasswordOutcome.Minutes(lockedFor)}";

    /// <summary>Whether <paramref name="code" /> is a refusal of a removal, which the How you sign in card shows.</summary>
    public static bool IsRemoveRefusal(string? code)
        => code is RemoveWrongPassword or RemoveLastSignIn or RemoveFailed or RemoveTooManyAttempts or RemoveAccountLocked;

    /// <summary>Whether the removal's refusal is shown inside the dialog, which the page opens again (R3-MA-RemoveStates a).</summary>
    public static bool IsShownInTheDialog(string? code) => code is RemoveWrongPassword or RemoveTooManyAttempts or RemoveAccountLocked;

    /// <summary>
    /// The sentence the Your name card shows for <paramref name="code" />: its own for a code it knows, the general one
    /// otherwise.
    /// </summary>
    public static string Describe(string code) => code switch
    {
        FirstNameMissing => $"{NameNotSaved} {FirstNameMissingMessage}",
        LastNameMissing => $"{NameNotSaved} {LastNameMissingMessage}",
        NameMissing => $"{NameNotSaved} Enter your first name and your last name.",
        NameTooLong => $"A first name or a last name can be at most {MaxNameLength} characters.",
        _ => GeneralRefusal
    };

    /// <summary>What the first name's field says after <paramref name="code" />; null when the field is not refused.</summary>
    public static string? FirstNameRefusal(string? code)
        => code is FirstNameMissing or NameMissing ? FirstNameMissingMessage : null;

    /// <summary>What the last name's field says after <paramref name="code" />; null when the field is not refused.</summary>
    public static string? LastNameRefusal(string? code)
        => code is LastNameMissing or NameMissing ? LastNameMissingMessage : null;

    /// <summary>
    /// What the page says once the sign-in through an institution is removed, naming it by <paramref name="institution" />,
    /// the provider's configured name, or not at all when the page does not know the provider.
    /// </summary>
    public static string SignInRemovedMessage(string? institution)
        => institution is null ? "Institutional sign-in removed." : $"{institution} sign-in removed.";

    /// <summary>
    /// The sentence for a refusal of a removal (<see cref="IsRemoveRefusal" />), naming the institution as
    /// <see cref="SignInRemovedMessage" /> does. <paramref name="lockedFor" /> is <see cref="RemoveAccountLocked" />'s wait
    /// as the page reads it (<see cref="ChangePasswordOutcome.LockedFor" />); Identity's own default lockout for a caller
    /// that does not say.
    /// </summary>
    public static string DescribeRemoval(string code, string? institution, TimeSpan? lockedFor = null)
    {
        var yours = institution is null ? "Your institutional sign-in" : $"Your {institution} sign-in";
        return code switch
        {
            RemoveWrongPassword => $"Your sign-in was not removed. {IncorrectPasswordMessage}",
            RemoveTooManyAttempts => "Your sign-in was not removed. Too many attempts from this network. Wait a few minutes and try again.",
            RemoveAccountLocked => "Your sign-in was not removed. " + ChangePasswordOutcome.AccountLockedMessage(
                lockedFor ?? new Microsoft.AspNetCore.Identity.LockoutOptions().DefaultLockoutTimeSpan),
            RemoveLastSignIn => $"{yours} was not removed. It is the only way you sign in to Wombat.",
            _ => $"{yours} was not removed. Try again."
        };
    }
}
