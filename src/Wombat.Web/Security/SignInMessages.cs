namespace Wombat.Web.Security;

/// <summary>What the sign-in page is sent back with when a sign-in is refused.</summary>
public static class SignInMessages
{
    /// <summary>
    /// A wrong password, an address no account has, an account that signs in only through its institution, and a locked
    /// account all read the same, so the answer says nothing about which addresses have accounts, how they sign in, or
    /// whether they are locked (T156; T287 in T339, flow 02). Until T156 an institutional account was told "This account
    /// uses institutional sign-in", which an unknown address was not; until T339 a locked account was told it had had too
    /// many failed attempts.
    /// </summary>
    public const string InvalidCredentials = "Invalid email or password.";

    /// <summary>
    /// <see cref="InvalidCredentials" /> where the page offers institutional sign-in: the same whichever of those three
    /// refusals it was, and it points someone who signs in through their institution at its button.
    /// </summary>
    public const string InvalidCredentialsOrInstitutional =
        "Invalid email or password. If your institution signs you in, use its sign-in button below.";

    /// <summary>The client has used up the sign-in throttle's failures (<see cref="SignInThrottle" />).</summary>
    public const string TooManyFailedAttempts =
        "Too many failed sign-in attempts from this network. Wait a few minutes and try again.";

    /// <summary>What a refused sign-in says, whichever of <see cref="InvalidCredentials" />' causes it was.</summary>
    public static string Refused(bool institutionalSignInOffered)
        => institutionalSignInOffered ? InvalidCredentialsOrInstitutional : InvalidCredentials;
}
