using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Options;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Handles the OIDC callback: looks up or provisions a Wombat user,
/// applies group-to-role mappings, and signs in.
/// </summary>
public sealed class ExternalLoginHandler
{
    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly SignInManager<WombatIdentityUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly SsoGroupMapper _groupMapper;
    private readonly IAuditWriter _auditWriter;
    private readonly IOptions<SsoOptions> _ssoOptions;
    private readonly ILogger<ExternalLoginHandler> _logger;

    public ExternalLoginHandler(
        UserManager<WombatIdentityUser> userManager,
        SignInManager<WombatIdentityUser> signInManager,
        ApplicationDbContext dbContext,
        SsoGroupMapper groupMapper,
        IAuditWriter auditWriter,
        IOptions<SsoOptions> ssoOptions,
        ILogger<ExternalLoginHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _groupMapper = groupMapper;
        _auditWriter = auditWriter;
        _ssoOptions = ssoOptions;
        _logger = logger;
    }

    /// <summary>
    /// The one message a failed link shows, whatever failed: a wrong password, an account outside the provider's
    /// institution, or no account. Anything more specific would tell a guesser which it was. (T149)
    /// </summary>
    internal const string LinkRefusedMessage = "The account could not be linked. Check your password and try again.";

    internal const string LinkLockedOutMessage =
        "Too many failed attempts. Please try again later or reset your password.";

    internal const string LockedOutMessage = "This account is locked. Contact your administrator.";

    internal const string UnknownProviderMessage = "Unknown SSO provider.";

    internal const string NoEmailMessage = "The identity provider did not supply an email address.";

    internal const string AlreadyLinkedMessage = "This institutional sign-in is already linked to an account.";

    internal const string LinkFailedMessage = "Linking failed.";

    internal const string AdministratorMessage =
        "An administrator account signs in with its password, not through institutional sign-in.";

    internal const string WrongInstitutionMessage =
        "This account is not registered at the institution this sign-in belongs to. Contact your administrator.";

    /// <summary>
    /// What a person is told when the provider did not verify the email: at the callback, whether or not an account holds
    /// the address, and at the link. It reads the same in every case, so it says nothing about which addresses have
    /// accounts. (T155)
    /// </summary>
    internal const string EmailNotVerifiedMessage =
        "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account " +
        "or create one. Sign in with your password if you have one, or ask your administrator for an invitation.";

    /// <summary>
    /// What a first sign-in is told when Identity refuses the new account for anything but a taken address: an address
    /// its user-name rules do not allow, say. Never Identity's own description, which quotes the address. (T156)
    /// </summary>
    internal const string AccountNotCreatedMessage =
        "Wombat could not create an account from your institution's sign-in. Contact your administrator.";

    /// <summary>The audit reason when Identity refused the new account. The row names the codes, never the address. (T156)</summary>
    internal const string AccountRefusedByValidationReason = "Identity refused the new account; none was created.";

    /// <summary>The audit reason when a first sign-in or a link is refused because the email is unverified. (T155)</summary>
    internal const string EmailNotVerifiedReason = "The provider did not assert the email as verified.";

    /// <summary>The audit reason when a first sign-in is refused because another account holds the email. (T155)</summary>
    internal const string EmailInUseReason = "Another account holds the provider's verified email.";

    /// <summary><c>unique_violation</c>: the new account's user name was inserted by another request first.</summary>
    private const string UniqueViolation = "23505";

    internal const string EmailInUseMessage =
        "A Wombat account already uses this email address, so a new one cannot be created. Contact your administrator.";

    /// <summary>The audit reason when the verified address is another account's. Neither account is changed. (T155)</summary>
    internal const string EmailHeldByAnotherAccountReason =
        "The provider's verified email is another account's address; neither account was changed.";

    /// <summary>The audit reason when Identity refused the verified address. The account is left as it was. (T155)</summary>
    internal const string EmailRefusedByValidationReason =
        "Identity refused the provider's verified email; the account was left unchanged.";

    /// <summary>
    /// Whether the provider asserts that its email claim is verified: every <paramref name="claimType" /> claim it sent
    /// reads as true, and there is at least one. Absent, false, or anything else is unverified.
    /// </summary>
    /// <remarks>
    /// ID tokens carry the value as <c>true</c>, userinfo mapped through ClaimActions as <c>True</c>, and some providers
    /// send the string <c>"true"</c>; <see cref="bool.TryParse(string, out bool)" /> reads all three. Entra ID's
    /// <c>xms_edov</c> is documented as a boolean but has been seen as the string <c>"1"</c> (supabase/auth handles both
    /// encodings), and a JSON number 1 becomes the claim value <c>1</c> too, so <c>1</c> also reads as true. Nothing
    /// else does: an assertion the code cannot read fails closed. (T155)
    /// </remarks>
    internal static bool EmailIsVerified(ClaimsPrincipal principal, string? claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return false;
        }

        var values = principal.FindAll(claimType).Select(claim => claim.Value).ToList();
        return values.Count > 0 && values.All(ReadsAsTrue);

        static bool ReadsAsTrue(string value)
            => (bool.TryParse(value, out var verified) && verified) || string.Equals(value, "1", StringComparison.Ordinal);
    }

    public sealed class ExternalLoginResult
    {
        public bool Succeeded { get; init; }
        public bool RequiresLinking { get; init; }
        public string? UserId { get; init; }
        public string? Email { get; init; }

        /// <summary>
        /// Why it was refused, as an <see cref="ExternalLoginRefusal" /> code: what the endpoints put in the address they
        /// redirect to, and the page describes (T285).
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>The sentence the page shows for <see cref="ErrorCode" />.</summary>
        public string? ErrorMessage => ExternalLoginRefusal.Describe(ErrorCode);
    }

    /// <summary>
    /// Process the external login callback. Returns a result describing
    /// whether the user was signed in, needs linking, or an error occurred.
    /// </summary>
    public async Task<ExternalLoginResult> HandleCallbackAsync(
        ExternalLoginInfo loginInfo,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var providerKey = loginInfo.LoginProvider;
        var externalSubjectId = loginInfo.ProviderKey;
        var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)
                    ?? loginInfo.Principal.FindFirstValue("email");
        var name = loginInfo.Principal.FindFirstValue(ClaimTypes.Name)
                   ?? loginInfo.Principal.FindFirstValue("name");

        var providerConfig = _ssoOptions.Value.Providers.FirstOrDefault(
            p => string.Equals(p.Key, providerKey, StringComparison.Ordinal));

        if (providerConfig is null)
        {
            _logger.LogError("SSO callback for unknown provider '{ProviderKey}'", providerKey);
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.UnknownProvider };
        }

        // Extract group claims
        var groupsClaim = providerConfig.GroupsClaim;
        var groupIds = loginInfo.Principal.FindAll(groupsClaim)
            .Select(c => c.Value)
            .ToList();

        var emailVerified = EmailIsVerified(loginInfo.Principal, providerConfig.EmailVerifiedClaim);

        // 1. Try to find user by existing external login link
        var user = await _userManager.FindByLoginAsync(providerKey, externalSubjectId);

        if (user is not null)
        {
            return await SignInExistingUserAsync(
                user, providerConfig, groupIds, name, email, emailVerified, ipAddress, userAgent, cancellationToken);
        }

        // 2. Try to find user by email within the same institution, but only by an email the provider verified. A link is
        // a lasting credential: once made, it signs the account in through the provider after its password has changed.
        // Matching an unverified email would let anyone who can set their own address at the provider, and has learnt
        // the account's password once, keep a way in that no password change ends. It would also say, by offering the
        // link page, which addresses have accounts here. An unverified email goes on to step 3, which refuses it with
        // the same message whether or not an account holds the address. (T155)
        if (emailVerified && !string.IsNullOrWhiteSpace(email))
        {
            var emailUser = await _userManager.FindByEmailAsync(email);
            if (emailUser is not null && emailUser.InstitutionId == providerConfig.InstitutionId)
            {
                // A deactivated account is not offered a link: the password page would end in a lockout message that
                // is wrong for an administrator's lock, and nothing it could do would let the person in. (T149)
                if (UserDeactivation.IsDeactivated(emailUser.LockoutEnd))
                {
                    return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.AccountLocked };
                }

                // Offer to link — do not auto-link across unverified email match. What gets linked is read back from
                // the external cookie at submit, never from the link page (T149).
                return new ExternalLoginResult
                {
                    RequiresLinking = true,
                    Email = email
                };
            }
        }

        // 3. Provision a new user
        return await ProvisionNewUserAsync(
            providerKey, externalSubjectId, providerConfig, email, emailVerified, name, groupIds, ipAddress, userAgent,
            cancellationToken);
    }

    /// <summary>
    /// Link the external login in the caller's external cookie to the local account the callback matched, after the
    /// caller proves they own it with its password. (T149)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Everything that decides WHICH login is linked to WHICH account comes from <paramref name="loginInfo" />, the
    /// external login the provider actually authenticated, never from the form. Before T149 the provider, the subject
    /// id and the email were all posted fields: anyone holding any external session could name any account, guess its
    /// password with no lockout and no throttle, and on a hit bind an external identity of their choosing to it.
    /// </para>
    /// <para>
    /// The account is the one the callback offered to link: the provider's asserted email, inside the provider's
    /// institution. The password is checked with <c>lockoutOnFailure</c>, so guesses count toward the same lockout as
    /// the local login, and a locked account cannot link. Every failure but the lockout reads the same.
    /// </para>
    /// </remarks>
    public async Task<ExternalLoginResult> LinkAndSignInAsync(
        ExternalLoginInfo loginInfo,
        string password,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loginInfo);

        var providerKey = loginInfo.LoginProvider;
        var externalSubjectId = loginInfo.ProviderKey;
        var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)
                    ?? loginInfo.Principal.FindFirstValue("email");
        var name = loginInfo.Principal.FindFirstValue(ClaimTypes.Name)
                   ?? loginInfo.Principal.FindFirstValue("name");

        var providerConfig = _ssoOptions.Value.Providers.FirstOrDefault(
            p => string.Equals(p.Key, providerKey, StringComparison.Ordinal));
        if (providerConfig is null)
        {
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.UnknownProvider };
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.NoEmail };
        }

        // The callback offers a link only for a verified email, but this endpoint is reachable with any external cookie
        // the provider's handler has set, without passing through the callback. So it judges the email itself, before it
        // looks up any account or checks any password: an unverified email names no account and costs none a failed
        // attempt. (T155)
        var emailVerified = EmailIsVerified(loginInfo.Principal, providerConfig.EmailVerifiedClaim);
        if (!emailVerified)
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                EmailNotVerifiedReason);
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.EmailNotVerified };
        }

        // Already linked: the callback signs that account in directly, so a link here can only be a replay.
        if (await _userManager.FindByLoginAsync(providerKey, externalSubjectId) is not null)
        {
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.AlreadyLinked };
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || user.InstitutionId != providerConfig.InstitutionId)
        {
            // Unstamped, like the local login's failure: stamping would say which institution the address belongs to.
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                "No account in the provider's institution for the asserted email.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.LinkRefused };
        }

        // The local login refuses an SSO-only account before checking any password, and so does this: a password check
        // here would count failures against an account whose owner never had one, and let anyone with an IdP identity
        // that asserts its email lock it out. The message is the same as a wrong password's.
        if (!user.AllowLocalPassword)
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                "The account the provider's email names has no local password to prove ownership with.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.LinkRefused };
        }

        if (await _userManager.IsInRoleAsync(user, WombatRoles.Administrator))
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user, ipAddress, userAgent,
                "An Administrator account cannot be linked to institutional sign-in.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.Administrator };
        }

        var check = await _signInManager.CheckPasswordSignInAsync(user, password ?? string.Empty, lockoutOnFailure: true);
        if (check.IsLockedOut)
        {
            await WriteLinkAuditAsync("SsoAccountLinkLockedOut", success: false, user, ipAddress, userAgent,
                "Account locked; the link was refused.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.LinkLockedOut };
        }

        if (!check.Succeeded)
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                "Wrong password for the account the provider's email names.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.LinkRefused };
        }

        var addLoginResult = await _userManager.AddLoginAsync(user,
            new UserLoginInfo(providerKey, externalSubjectId, providerConfig.DisplayName));

        // Refused as a code, never Identity's description. LoginAlreadyAssociated is the one a person can meet: another
        // request linked this institutional sign-in after the check above (during the password check, say) and before
        // Identity's own look-up, which refuses before it adds anything; it reads as that check's refusal. Anything else is
        // logged, and the page says linking failed (T285). Two requests that pass Identity's look-up together are not
        // refused here: the second one's write breaks the logins table's key, and the user store throws rather than
        // returning a result.
        if (!addLoginResult.Succeeded)
        {
            // A refusal of the account's write (a user validator's) comes after Identity's store has added the login to the
            // context, and returns without taking it back: the next save in this request, an audit row's, would commit the
            // link that was just refused (the audit trap).
            DiscardAddedLogins();

            var codes = addLoginResult.Errors.Select(error => error.Code).ToList();
            if (codes.Contains(nameof(IdentityErrorDescriber.LoginAlreadyAssociated)))
            {
                return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.AlreadyLinked };
            }

            _logger.LogWarning(
                "Linking user {UserId} to an institutional sign-in through '{ProviderKey}' was refused by Identity ({Codes}).",
                user.Id, providerKey, string.Join(", ", codes));
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.LinkFailed };
        }

        // Stamped like SsoLogin and SsoFirstLogin below. An unstamped row is Administrator-only since T101, and this is
        // the row an account-takeover investigation starts from: the moment an external identity was bound to an
        // existing local account. The institution's own admin is the one who would notice it. (T101)
        await WriteLinkAuditAsync("SsoAccountLinked", success: true, user, ipAddress, userAgent, errorMessage: null);

        // The account was found by this email, so there is nothing for the sign-in to sync; it is still judged the same.
        var groupIds = loginInfo.Principal.FindAll(providerConfig.GroupsClaim).Select(c => c.Value).ToList();
        return await SignInExistingUserAsync(
            user, providerConfig, groupIds, name, email, emailVerified, ipAddress, userAgent, cancellationToken);
    }

    /// <summary>
    /// A refused first sign-in has no account to name. It is stamped with the provider's institution, whose
    /// administrators run the provider, and carries no address. (T155)
    /// </summary>
    private Task WriteProvisioningRefusedAuditAsync(int institutionId, string? ipAddress, string? userAgent, string reason)
        => _auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "SsoProvisioningRefused",
            success: false,
            actorIpAddress: ipAddress,
            actorUserAgent: userAgent,
            institutionId: institutionId,
            errorMessage: reason));

    /// <summary>
    /// A first sign-in whose new account Identity refused. Until T156 the person was shown Identity's descriptions, so an
    /// address held as another account's user name read "Username '…' is already taken." A taken address, which
    /// <see cref="AddressHeldByAnotherAccountAsync" /> catches first unless another sign-in took it in between, is told
    /// what that check tells it; anything else gets <see cref="AccountNotCreatedMessage" />. The codes go to the log and
    /// the audit row, never to the person. (T156) An address taken so late that Identity's own check missed it too is
    /// refused by the database's unique index instead, and told the same, where the account is created.
    /// </summary>
    private async Task<ExternalLoginResult> RefuseCreatedAccountAsync(
        IdentityResult createResult,
        string providerKey,
        int institutionId,
        string? ipAddress,
        string? userAgent)
    {
        var codes = createResult.Errors.Select(error => error.Code).ToList();
        var taken = codes.Any(code => code is nameof(IdentityErrorDescriber.DuplicateUserName)
            or nameof(IdentityErrorDescriber.DuplicateEmail));

        _logger.LogWarning(
            "SSO first sign-in through '{ProviderKey}' was refused by Identity ({Codes}); no account was created.",
            providerKey, string.Join(", ", codes));

        if (taken)
        {
            await WriteProvisioningRefusedAuditAsync(institutionId, ipAddress, userAgent, EmailInUseReason);
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.EmailInUse };
        }

        await WriteProvisioningRefusedAuditAsync(institutionId, ipAddress, userAgent,
            $"{AccountRefusedByValidationReason} ({string.Join(", ", codes)})");
        return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.AccountNotCreated };
    }

    private Task WriteLinkAuditAsync(
        string action,
        bool success,
        WombatIdentityUser? user,
        string? ipAddress,
        string? userAgent,
        string? errorMessage)
        => _auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: action,
            success: success,
            actorUserId: user?.Id,
            actorDisplay: user is null ? null : $"{user.FirstName} {user.LastName}".Trim(),
            actorIpAddress: ipAddress,
            actorUserAgent: userAgent,
            institutionId: user?.InstitutionId,
            errorMessage: errorMessage));

    /// <remarks>
    /// Refuses, before touching the account (T149):
    /// <list type="bullet">
    ///   <item>a DEACTIVATED account — an administrator's lock or an erasure (<see cref="UserDeactivation" />). Not a
    ///   running brute-force lockout: that protects the password, which this path never checks, and honouring it here
    ///   would let anyone keep an SSO user out by typing five wrong passwords at the local login every fifteen
    ///   minutes;</item>
    ///   <item>an Administrator account — the role cannot be granted through SSO, so it is not reachable through it
    ///   either, or the institution's identity-provider admins could sign in as a global Administrator;</item>
    ///   <item>an account of another institution — the provider's group mappings would hand it this institution's
    ///   roles. An admin moving a user to another institution removes their external logins
    ///   (<c>UserAdministrationService.UpdateScopeAsync</c>), so this refusal does not strand a moved user.</item>
    /// </list>
    /// Then it syncs the email (<see cref="SyncEmailAsync" />) before anything else is saved, then the name, then the roles.
    /// </remarks>
    private async Task<ExternalLoginResult> SignInExistingUserAsync(
        WombatIdentityUser user,
        SsoProviderOptions providerConfig,
        List<string> groupIds,
        string? name,
        string? email,
        bool emailVerified,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var providerKey = providerConfig.Key;

        if (UserDeactivation.IsDeactivated(user.LockoutEnd))
        {
            await WriteLinkAuditAsync("SsoLoginRefused", success: false, user, ipAddress, userAgent, "Account is deactivated.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.AccountLocked };
        }

        if (await _userManager.IsInRoleAsync(user, WombatRoles.Administrator))
        {
            await WriteLinkAuditAsync("SsoLoginRefused", success: false, user, ipAddress, userAgent,
                "Administrator accounts cannot sign in through SSO.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.Administrator };
        }

        if (user.InstitutionId != providerConfig.InstitutionId)
        {
            await WriteLinkAuditAsync("SsoLoginRefused", success: false, user, ipAddress, userAgent,
                $"Account belongs to another institution than provider '{providerKey}'.");
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.WrongInstitution };
        }

        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            if (emailVerified)
            {
                await SyncEmailAsync(user, email, providerKey, ipAddress, userAgent, cancellationToken);
            }
            else
            {
                _logger.LogInformation(
                    "SSO sign-in of user {UserId} through '{ProviderKey}' carried an email the provider does not assert as " +
                    "verified; the account's email was left as it was.",
                    user.Id, providerKey);
            }
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            var parts = name.Split(' ', 2);
            var firstName = parts[0];
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;
            if (user.FirstName != firstName || user.LastName != lastName)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                var renamed = await _userManager.UpdateAsync(user);
                if (!renamed.Succeeded)
                {
                    // A refused update stays tracked, and the group mapper's save below would commit it. (T155)
                    await DiscardChangesAsync(user, cancellationToken);
                    _logger.LogWarning("SSO sign-in could not update the name of user {UserId}: {Errors}",
                        user.Id, string.Join("; ", renamed.Errors.Select(e => e.Code)));
                }
            }
        }

        // Sync roles from groups
        await _groupMapper.ApplyAsync(user, providerKey, groupIds, cancellationToken);

        await _signInManager.SignInAsync(user, isPersistent: false);

        await _auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "SsoLogin",
            success: true,
            actorUserId: user.Id,
            actorDisplay: $"{user.FirstName} {user.LastName}".Trim(),
            actorIpAddress: ipAddress,
            actorUserAgent: userAgent,
            institutionId: user.InstitutionId));

        return new ExternalLoginResult { Succeeded = true, UserId = user.Id };
    }

    /// <summary>
    /// Writes the provider's verified email to the account as its email and user name, or leaves the account as it was.
    /// Called only for an email the provider asserts as verified, and before anything else in the sign-in is saved. (T155)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before T155 every SSO sign-in copied the email claim, verified or not, and ignored the result of the update. A claim
    /// naming another account's address was refused by Identity as a duplicate user name, but the refused values stayed
    /// on the tracked entity, and the group mapper's save committed them: the account took the other person's address,
    /// with its normalised columns still the old ones.
    /// </para>
    /// <para>
    /// An address another account holds, as its email or its user name, is refused before anything is changed. Identity
    /// would catch only the user name: it is not configured to require unique emails, and the sign-in page's
    /// <c>FindByEmailAsync</c> throws once two accounts share one.
    /// </para>
    /// <para>
    /// The change is one validated write, not <c>SetUserNameAsync</c> then <c>SetEmailAsync</c>. Those are two saves, so a
    /// refusal of the second would leave the user name on the new address and the email on the old one. The write does
    /// what <c>SetEmailAsync</c> does, except that it marks the address confirmed, because the provider verified it:
    /// <c>UpdateSecurityStampAsync</c> rotates the stamp, runs Identity's validators, sets both normalised columns and
    /// saves. If it is refused, the account is reloaded before anything else can save it.
    /// </para>
    /// </remarks>
    private async Task SyncEmailAsync(
        WombatIdentityUser user,
        string email,
        string providerKey,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (await AddressHeldByAnotherAccountAsync(email, user.Id))
        {
            _logger.LogWarning(
                "SSO sign-in of user {UserId} through '{ProviderKey}' asserted a verified email that another account holds. " +
                "The email was not synced, and neither account was changed.",
                user.Id, providerKey);
            await WriteLinkAuditAsync("SsoEmailSyncRefused", success: false, user, ipAddress, userAgent,
                EmailHeldByAnotherAccountReason);
            return;
        }

        user.Email = email;
        user.UserName = email;
        user.EmailConfirmed = true;
        var updated = await _userManager.UpdateSecurityStampAsync(user);
        if (!updated.Succeeded)
        {
            await DiscardChangesAsync(user, cancellationToken);
            _logger.LogWarning(
                "SSO sign-in of user {UserId} through '{ProviderKey}' asserted a verified email that Identity refused: " +
                "{Errors}. The account was left unchanged.",
                user.Id, providerKey, string.Join("; ", updated.Errors.Select(e => e.Code)));
            await WriteLinkAuditAsync("SsoEmailSyncRefused", success: false, user, ipAddress, userAgent,
                EmailRefusedByValidationReason);
            return;
        }

        // No address in the row: the audit log outlives an erasure, which pseudonymises the account but not free text.
        await WriteLinkAuditAsync("SsoEmailChanged", success: true, user, ipAddress, userAgent, errorMessage: null);
    }

    /// <summary>Whether an account other than <paramref name="userId" /> holds the address as its email or user name.</summary>
    private async Task<bool> AddressHeldByAnotherAccountAsync(string address, string? userId)
    {
        var byEmail = await _userManager.FindByEmailAsync(address);
        if (byEmail is not null && byEmail.Id != userId)
        {
            return true;
        }

        var byUserName = await _userManager.FindByNameAsync(address);
        return byUserName is not null && byUserName.Id != userId;
    }

    /// <summary>
    /// Puts back what a refused Identity update left on the tracked account. <c>UserManager</c> sets the values before it
    /// validates them, and a refusal returns without saving but without undoing them, so the next save in this scope (the
    /// group mapper's, or the audit writer's) would commit them. Reloading also resets the object itself, which the
    /// sign-in goes on to build its cookie from. (T155)
    /// </summary>
    private Task DiscardChangesAsync(WombatIdentityUser user, CancellationToken cancellationToken)
        => _dbContext.Entry(user).ReloadAsync(cancellationToken);

    /// <summary>Takes out of the context every external login added to it and not yet saved. (T285)</summary>
    private void DiscardAddedLogins()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries<IdentityUserLogin<string>>()
                     .Where(entry => entry.State == EntityState.Added)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    private async Task<ExternalLoginResult> ProvisionNewUserAsync(
        string providerKey,
        string externalSubjectId,
        SsoProviderOptions providerConfig,
        string? email,
        bool emailVerified,
        string? name,
        List<string> groupIds,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.NoEmail };
        }

        // An account is created from the email the provider asserts, and the address becomes its user name, its sign-in
        // match and where its mail goes. So it must be one the provider verified: otherwise anyone who can set their own
        // email at the provider could take an address before its owner is invited, and InvitedUserProvisioner refuses an
        // address that is taken. A provider that verifies nothing still signs in the accounts it is linked to. (T155)
        //
        // This comes before any lookup. An unverified email reaches this point whether or not an account holds it (the
        // callback matches only verified ones), so asking who holds it first would answer that question for anyone who
        // can type an address at their provider: "in use" for a held address, "not verified" for any other.
        if (!emailVerified)
        {
            _logger.LogWarning(
                "SSO first sign-in through '{ProviderKey}' carried an email the provider does not assert as verified; " +
                "no account was created.",
                providerKey);
            await WriteProvisioningRefusedAuditAsync(providerConfig.InstitutionId, ipAddress, userAgent,
                EmailNotVerifiedReason);
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.EmailNotVerified };
        }

        // Reached with a verified email only when no account of this institution has it, so a holder is another
        // institution's account, or one that holds the address only as its user name. Identity would refuse a duplicate
        // user name but not a duplicate email, and two accounts sharing an email break the sign-in page's lookup for
        // both. (T155)
        if (await AddressHeldByAnotherAccountAsync(email, userId: null))
        {
            _logger.LogWarning(
                "SSO first sign-in through '{ProviderKey}' asserted an email another account holds; no account was created.",
                providerKey);
            await WriteProvisioningRefusedAuditAsync(providerConfig.InstitutionId, ipAddress, userAgent,
                EmailInUseReason);
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.EmailInUse };
        }

        var parts = (name ?? email).Split(' ', 2);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : string.Empty;

        var user = new WombatIdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // The provider verified it: nothing unverified reaches this point.
            FirstName = firstName,
            LastName = lastName,
            InstitutionId = providerConfig.InstitutionId,
            AllowLocalPassword = false
        };

        IdentityResult createResult;
        try
        {
            createResult = await _userManager.CreateAsync(user);
        }
        catch (DbUpdateException exception) when (exception.InnerException is DbException { SqlState: UniqueViolation })
        {
            // Another first sign-in with the same address created its account after both had passed Identity's user-name
            // check, so the unique index on the normalised user name refuses this one, and the user store throws that
            // rather than returning a result. It is refused as the in-use check refuses it, not left to the error page.
            // The account is taken out of the context first: it is still tracked as added, and the refusal's audit row
            // saves this context, which would send it again. (T156 review)
            _dbContext.Entry(user).State = EntityState.Detached;
            _logger.LogWarning(
                "SSO first sign-in through '{ProviderKey}' was refused by the database: another account took the address " +
                "at the same moment; no account was created.",
                providerKey);
            await WriteProvisioningRefusedAuditAsync(providerConfig.InstitutionId, ipAddress, userAgent, EmailInUseReason);
            return new ExternalLoginResult { ErrorCode = ExternalLoginRefusal.EmailInUse };
        }

        if (!createResult.Succeeded)
        {
            return await RefuseCreatedAccountAsync(createResult, providerKey, providerConfig.InstitutionId, ipAddress, userAgent);
        }

        var addLoginResult = await _userManager.AddLoginAsync(user,
            new UserLoginInfo(providerKey, externalSubjectId, providerConfig.DisplayName));

        if (!addLoginResult.Succeeded)
        {
            _logger.LogError("Failed to add external login for new user {UserId}: {Errors}",
                user.Id, string.Join("; ", addLoginResult.Errors.Select(e => e.Description)));
        }

        // Apply group-to-role mappings
        var roles = await _groupMapper.ApplyAsync(user, providerKey, groupIds, cancellationToken);

        // If no roles were assigned, default to PendingTrainee
        if (roles.Count == 0)
        {
            await _userManager.AddToRoleAsync(user, WombatRoles.PendingTrainee);
            _dbContext.UserRoleAssignments.Add(new UserRoleAssignment
            {
                UserId = user.Id,
                Role = WombatRoles.PendingTrainee,
                Source = RoleAssignmentSource.Sso,
                ProviderKey = providerKey,
                AssignedOn = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SSO-provisioned user {UserId} ({Email}) has no matching group mappings — assigned PendingTrainee.",
                user.Id, email);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        await _auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "SsoFirstLogin",
            success: true,
            actorUserId: user.Id,
            actorDisplay: $"{firstName} {lastName}".Trim(),
            actorIpAddress: ipAddress,
            actorUserAgent: userAgent,
            institutionId: providerConfig.InstitutionId));

        return new ExternalLoginResult { Succeeded = true, UserId = user.Id };
    }
}
