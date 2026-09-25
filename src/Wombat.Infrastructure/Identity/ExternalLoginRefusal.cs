namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Why an institutional sign-in or a link was refused (<see cref="ExternalLoginHandler.ExternalLoginResult.ErrorCode" />),
/// and the one sentence each code is shown as (T285).
/// </summary>
/// <remarks>
/// The endpoints put the code in the address they redirect to, never the sentence, and the sign-in and link pages show
/// <see cref="Describe" />'s sentence for it: until T285 they printed whatever text the address carried, so a crafted link
/// could put words of its choosing on Wombat's own sign-in page. A code this list does not name has no sentence here, and
/// the page shows its own general one.
/// </remarks>
public static class ExternalLoginRefusal
{
    /// <summary>The provider named is not one Wombat is configured with.</summary>
    public const string UnknownProvider = "SsoUnknownProvider";

    /// <summary>The provider sent no email address.</summary>
    public const string NoEmail = "SsoNoEmail";

    /// <summary>The provider did not assert its email as verified (T155).</summary>
    public const string EmailNotVerified = "SsoEmailNotVerified";

    /// <summary>The account is deactivated: an administrator's lock or an erasure (T149).</summary>
    public const string AccountLocked = "SsoAccountLocked";

    /// <summary>An Administrator account signs in with its password only (T149).</summary>
    public const string Administrator = "SsoAdministrator";

    /// <summary>The account is another institution's than the provider's (T149).</summary>
    public const string WrongInstitution = "SsoWrongInstitution";

    /// <summary>Another account holds the provider's verified email, so no account is created (T155).</summary>
    public const string EmailInUse = "SsoEmailInUse";

    /// <summary>Identity refused the new account for anything but a taken address (T156).</summary>
    public const string AccountNotCreated = "SsoAccountNotCreated";

    /// <summary>The institutional sign-in is linked to an account already.</summary>
    public const string AlreadyLinked = "SsoAlreadyLinked";

    /// <summary>Every refused link but a lockout: a wrong password, no account, another institution's (T149).</summary>
    public const string LinkRefused = "SsoLinkRefused";

    /// <summary>The account's lockout refused the link's password check.</summary>
    public const string LinkLockedOut = "SsoLinkLockedOut";

    /// <summary>The link could not be saved, for a reason that is not the person's to put right.</summary>
    public const string LinkFailed = "SsoLinkFailed";

    /// <summary>The sentence for <paramref name="code" />; null for a code this list does not name.</summary>
    public static string? Describe(string? code) => code switch
    {
        UnknownProvider => ExternalLoginHandler.UnknownProviderMessage,
        NoEmail => ExternalLoginHandler.NoEmailMessage,
        EmailNotVerified => ExternalLoginHandler.EmailNotVerifiedMessage,
        AccountLocked => ExternalLoginHandler.LockedOutMessage,
        Administrator => ExternalLoginHandler.AdministratorMessage,
        WrongInstitution => ExternalLoginHandler.WrongInstitutionMessage,
        EmailInUse => ExternalLoginHandler.EmailInUseMessage,
        AccountNotCreated => ExternalLoginHandler.AccountNotCreatedMessage,
        AlreadyLinked => ExternalLoginHandler.AlreadyLinkedMessage,
        LinkRefused => ExternalLoginHandler.LinkRefusedMessage,
        LinkLockedOut => ExternalLoginHandler.LinkLockedOutMessage,
        LinkFailed => ExternalLoginHandler.LinkFailedMessage,
        _ => null
    };

    /// <summary>Every code, for a test that asks each to have a sentence.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        UnknownProvider, NoEmail, EmailNotVerified, AccountLocked, Administrator, WrongInstitution, EmailInUse,
        AccountNotCreated, AlreadyLinked, LinkRefused, LinkLockedOut, LinkFailed
    ];
}
