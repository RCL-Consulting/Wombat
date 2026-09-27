namespace Wombat.Web.Security;

/// <summary>
/// Where My account's form posts, where the answer goes, and the words the page shows for it (T335, flow 01; the review
/// of the t335 branch).
/// </summary>
/// <remarks>
/// <para>
/// The name is saved by <see cref="SubmitPath" />, an endpoint in <c>Program.cs</c> that the page's form posts to, as
/// change password's does (T265). The shell names the person from the sign-in cookie's <c>display_name</c> claim, so it
/// reads no database; a name saved in the page's circuit left that claim stale in the circuit and in the cookie, and the
/// account row went on showing the old name. The endpoint saves the name and issues the cookie again, in one request, and
/// sends the browser back to <see cref="PagePath" />, a full page load whose circuit starts from the new cookie.
/// </para>
/// <para>
/// A refusal travels as a code, never as words, and the page chooses the words (T285), so a crafted link cannot put words
/// of its choosing on the page.
/// </para>
/// </remarks>
public static class ProfileOutcome
{
    /// <summary>My account.</summary>
    public const string PagePath = "/account/profile";

    /// <summary>The endpoint the page's form posts to.</summary>
    public const string SubmitPath = "/account/profile/submit";

    /// <summary>The endpoint's log category: a fault in a save is logged, and the user is told in words of the page's.</summary>
    public const string LogCategory = "Wombat.Web.Account.Profile";

    /// <summary>The page's <c>status</c> once the name is saved.</summary>
    public const string Saved = "saved";

    /// <summary>A name was left blank.</summary>
    public const string NameMissing = "NameMissing";

    /// <summary>A name is longer than <see cref="MaxNameLength" /> characters.</summary>
    public const string NameTooLong = "NameTooLong";

    /// <summary>A fault, or a code the page does not know.</summary>
    public const string Failed = "Failed";

    /// <summary>The longest a first or last name may be, as the command's validator holds it.</summary>
    public const int MaxNameLength = 100;

    /// <summary>What the page says for <see cref="Failed" /> and for any code it does not know.</summary>
    public const string GeneralRefusal = "Your name could not be saved. Please try again.";

    /// <summary>The page, saying the name was saved.</summary>
    public static string SavedUrl { get; } = $"{PagePath}?status={Saved}";

    /// <summary>The page, refusing with <paramref name="code" />.</summary>
    public static string RefusedUrl(string code) => $"{PagePath}?error={Uri.EscapeDataString(code)}";

    /// <summary>The sentence the page shows for <paramref name="code" />: its own for a code it knows, the general one otherwise.</summary>
    public static string Describe(string code) => code switch
    {
        NameMissing => "Enter your first name and your last name.",
        NameTooLong => $"A first name or a last name can be at most {MaxNameLength} characters.",
        _ => GeneralRefusal
    };
}
