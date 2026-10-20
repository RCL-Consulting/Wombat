namespace Wombat.Web.Security;

/// <summary>
/// The sign-out page's two addresses and the endpoint its form posts to (T339, flow 02, the round 2 review's B1 and A12).
/// </summary>
/// <remarks>
/// <para>
/// Until T339 the post went to <c>/account/logout</c> itself and the page lived at <c>/account/logout-confirm</c> only. A
/// Razor component's address answers both GET and POST, so the page could not take <c>/account/logout</c> while the post
/// was mapped there: the post moved to <see cref="SubmitPath" />, as every other account form's did, and both addresses
/// draw the page. Neither GET signs anyone out.
/// </para>
/// <para>
/// A signed-in post requires the antiforgery token (E6), which the page's form and the shell's Sign out form carry, and
/// lands on the sign-in page saying "You have signed out." (<see cref="SignInOutcome.SignedOut" />). The token alone does
/// not hold E6: another site's form reaches the endpoint without the sign-in cookie (SameSite=Lax), so it arrives not
/// signed in, and is only redirected there. It signs nothing out and deletes no cookie (the review of the t339 branch:
/// until then that branch signed out, and the browser applied the deletions).
/// </para>
/// </remarks>
public static class SignOutOutcome
{
    /// <summary>The sign-out page.</summary>
    public const string PagePath = "/account/logout";

    /// <summary>
    /// The same page, at the address the error page's Sign out links to: a form on the error page would carry a token its
    /// own failed request invalidated, so it links here, and this page draws a form and token of its own (T321).
    /// </summary>
    public const string ConfirmPath = "/account/logout-confirm";

    /// <summary>The endpoint the sign-out forms post to.</summary>
    public const string SubmitPath = "/account/logout/submit";

    /// <summary>Where signing out lands: the sign-in page, saying so.</summary>
    public static string SignedOutUrl { get; } = SignInOutcome.Url(SignInOutcome.SignedOut);

    /// <summary>
    /// The form field Register's "Sign out and continue" sends (T373, flow 11, F5; G18): the page to land on instead of
    /// <see cref="SignedOutUrl" />. Flow 02's page and the shell's Sign out send none.
    /// </summary>
    public const string ReturnToField = "returnTo";

    /// <summary>
    /// Where a sign-out lands: <see cref="RegisterOutcome.PagePath" /> when <paramref name="returnTo" /> is exactly that
    /// path (ordinal), so the person goes on to the invitation the register cookie still holds; <see cref="SignedOutUrl" />
    /// for anything else, so the field is never an open redirect (F5).
    /// </summary>
    public static string LandingFor(string? returnTo)
        => string.Equals(returnTo, RegisterOutcome.PagePath, StringComparison.Ordinal) ? RegisterOutcome.PagePath : SignedOutUrl;
}
