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

    internal const string AdministratorMessage =
        "An administrator account signs in with its password, not through institutional sign-in.";

    internal const string WrongInstitutionMessage =
        "This account is not registered at the institution this sign-in belongs to. Contact your administrator.";

    public sealed class ExternalLoginResult
    {
        public bool Succeeded { get; init; }
        public bool RequiresLinking { get; init; }
        public string? UserId { get; init; }
        public string? Email { get; init; }
        public string? ErrorMessage { get; init; }
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
            return new ExternalLoginResult { ErrorMessage = "Unknown SSO provider." };
        }

        // Extract group claims
        var groupsClaim = providerConfig.GroupsClaim;
        var groupIds = loginInfo.Principal.FindAll(groupsClaim)
            .Select(c => c.Value)
            .ToList();

        // 1. Try to find user by existing external login link
        var user = await _userManager.FindByLoginAsync(providerKey, externalSubjectId);

        if (user is not null)
        {
            return await SignInExistingUserAsync(user, providerConfig, groupIds, name, email, ipAddress, userAgent, cancellationToken);
        }

        // 2. Try to find user by email within the same institution
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailUser = await _userManager.FindByEmailAsync(email);
            if (emailUser is not null && emailUser.InstitutionId == providerConfig.InstitutionId)
            {
                // A deactivated account is not offered a link: the password page would end in a lockout message that
                // is wrong for an administrator's lock, and nothing it could do would let the person in. (T149)
                if (UserDeactivation.IsDeactivated(emailUser.LockoutEnd))
                {
                    return new ExternalLoginResult { ErrorMessage = LockedOutMessage };
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
            providerKey, externalSubjectId, providerConfig, email, name, groupIds, ipAddress, userAgent, cancellationToken);
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
            return new ExternalLoginResult { ErrorMessage = "Unknown SSO provider." };
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return new ExternalLoginResult { ErrorMessage = "The identity provider did not supply an email address." };
        }

        // Already linked: the callback signs that account in directly, so a link here can only be a replay.
        if (await _userManager.FindByLoginAsync(providerKey, externalSubjectId) is not null)
        {
            return new ExternalLoginResult { ErrorMessage = "This institutional sign-in is already linked to an account." };
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || user.InstitutionId != providerConfig.InstitutionId)
        {
            // Unstamped, like the local login's failure: stamping would say which institution the address belongs to.
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                "No account in the provider's institution for the asserted email.");
            return new ExternalLoginResult { ErrorMessage = LinkRefusedMessage };
        }

        // The local login refuses an SSO-only account before checking any password, and so does this: a password check
        // here would count failures against an account whose owner never had one, and let anyone with an IdP identity
        // that asserts its email lock it out. The message is the same as a wrong password's.
        if (!user.AllowLocalPassword)
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                "The account the provider's email names has no local password to prove ownership with.");
            return new ExternalLoginResult { ErrorMessage = LinkRefusedMessage };
        }

        if (await _userManager.IsInRoleAsync(user, WombatRoles.Administrator))
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user, ipAddress, userAgent,
                "An Administrator account cannot be linked to institutional sign-in.");
            return new ExternalLoginResult { ErrorMessage = AdministratorMessage };
        }

        var check = await _signInManager.CheckPasswordSignInAsync(user, password ?? string.Empty, lockoutOnFailure: true);
        if (check.IsLockedOut)
        {
            await WriteLinkAuditAsync("SsoAccountLinkLockedOut", success: false, user, ipAddress, userAgent,
                "Account locked; the link was refused.");
            return new ExternalLoginResult { ErrorMessage = LinkLockedOutMessage };
        }

        if (!check.Succeeded)
        {
            await WriteLinkAuditAsync("SsoAccountLinkFailed", success: false, user: null, ipAddress, userAgent,
                "Wrong password for the account the provider's email names.");
            return new ExternalLoginResult { ErrorMessage = LinkRefusedMessage };
        }

        var addLoginResult = await _userManager.AddLoginAsync(user,
            new UserLoginInfo(providerKey, externalSubjectId, providerConfig.DisplayName));

        if (!addLoginResult.Succeeded)
        {
            return new ExternalLoginResult
            {
                ErrorMessage = string.Join("; ", addLoginResult.Errors.Select(e => e.Description))
            };
        }

        // Stamped like SsoLogin and SsoFirstLogin below. An unstamped row is Administrator-only since T101, and this is
        // the row an account-takeover investigation starts from: the moment an external identity was bound to an
        // existing local account. The institution's own admin is the one who would notice it. (T101)
        await WriteLinkAuditAsync("SsoAccountLinked", success: true, user, ipAddress, userAgent, errorMessage: null);

        var groupIds = loginInfo.Principal.FindAll(providerConfig.GroupsClaim).Select(c => c.Value).ToList();
        return await SignInExistingUserAsync(user, providerConfig, groupIds, name, email, ipAddress, userAgent, cancellationToken);
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
    /// </remarks>
    private async Task<ExternalLoginResult> SignInExistingUserAsync(
        WombatIdentityUser user,
        SsoProviderOptions providerConfig,
        List<string> groupIds,
        string? name,
        string? email,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var providerKey = providerConfig.Key;

        if (UserDeactivation.IsDeactivated(user.LockoutEnd))
        {
            await WriteLinkAuditAsync("SsoLoginRefused", success: false, user, ipAddress, userAgent, "Account is deactivated.");
            return new ExternalLoginResult { ErrorMessage = LockedOutMessage };
        }

        if (await _userManager.IsInRoleAsync(user, WombatRoles.Administrator))
        {
            await WriteLinkAuditAsync("SsoLoginRefused", success: false, user, ipAddress, userAgent,
                "Administrator accounts cannot sign in through SSO.");
            return new ExternalLoginResult { ErrorMessage = AdministratorMessage };
        }

        if (user.InstitutionId != providerConfig.InstitutionId)
        {
            await WriteLinkAuditAsync("SsoLoginRefused", success: false, user, ipAddress, userAgent,
                $"Account belongs to another institution than provider '{providerKey}'.");
            return new ExternalLoginResult { ErrorMessage = WrongInstitutionMessage };
        }

        // Update profile from claims if changed
        var changed = false;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var parts = name.Split(' ', 2);
            var firstName = parts[0];
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;
            if (user.FirstName != firstName || user.LastName != lastName)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                changed = true;
            }
        }
        if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            user.Email = email;
            user.UserName = email;
            changed = true;
        }
        if (changed)
        {
            await _userManager.UpdateAsync(user);
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

    private async Task<ExternalLoginResult> ProvisionNewUserAsync(
        string providerKey,
        string externalSubjectId,
        SsoProviderOptions providerConfig,
        string? email,
        string? name,
        List<string> groupIds,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new ExternalLoginResult { ErrorMessage = "The identity provider did not supply an email address." };
        }

        var parts = (name ?? email).Split(' ', 2);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : string.Empty;

        var user = new WombatIdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // The provider asserted it
            FirstName = firstName,
            LastName = lastName,
            InstitutionId = providerConfig.InstitutionId,
            AllowLocalPassword = false
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return new ExternalLoginResult
            {
                ErrorMessage = string.Join("; ", createResult.Errors.Select(e => e.Description))
            };
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
