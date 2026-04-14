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

    public sealed class ExternalLoginResult
    {
        public bool Succeeded { get; init; }
        public bool RequiresLinking { get; init; }
        public string? UserId { get; init; }
        public string? Email { get; init; }
        public string? ProviderKey { get; init; }
        public string? ExternalSubjectId { get; init; }
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
            return await SignInExistingUserAsync(user, providerKey, groupIds, name, email, ipAddress, userAgent, cancellationToken);
        }

        // 2. Try to find user by email within the same institution
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailUser = await _userManager.FindByEmailAsync(email);
            if (emailUser is not null && emailUser.InstitutionId == providerConfig.InstitutionId)
            {
                // Offer to link — do not auto-link across unverified email match
                return new ExternalLoginResult
                {
                    RequiresLinking = true,
                    Email = email,
                    ProviderKey = providerKey,
                    ExternalSubjectId = externalSubjectId
                };
            }
        }

        // 3. Provision a new user
        return await ProvisionNewUserAsync(
            providerKey, externalSubjectId, providerConfig, email, name, groupIds, ipAddress, userAgent, cancellationToken);
    }

    /// <summary>
    /// Link an external login to an existing user after they confirm ownership
    /// by entering their local password.
    /// </summary>
    public async Task<ExternalLoginResult> LinkAndSignInAsync(
        string email,
        string password,
        string providerKey,
        string externalSubjectId,
        ExternalLoginInfo loginInfo,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new ExternalLoginResult { ErrorMessage = "User not found." };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
        {
            return new ExternalLoginResult { ErrorMessage = "Invalid password." };
        }

        var addLoginResult = await _userManager.AddLoginAsync(user,
            new UserLoginInfo(providerKey, externalSubjectId, providerKey));

        if (!addLoginResult.Succeeded)
        {
            return new ExternalLoginResult
            {
                ErrorMessage = string.Join("; ", addLoginResult.Errors.Select(e => e.Description))
            };
        }

        var providerConfig = _ssoOptions.Value.Providers.FirstOrDefault(
            p => string.Equals(p.Key, providerKey, StringComparison.Ordinal));

        var groupsClaim = providerConfig?.GroupsClaim ?? "groups";
        var groupIds = loginInfo.Principal.FindAll(groupsClaim).Select(c => c.Value).ToList();

        await _auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Authentication,
            action: "SsoAccountLinked",
            success: true,
            actorUserId: user.Id,
            actorDisplay: $"{user.FirstName} {user.LastName}".Trim(),
            actorIpAddress: ipAddress,
            actorUserAgent: userAgent));

        return await SignInExistingUserAsync(user, providerKey, groupIds, null, null, ipAddress, userAgent, cancellationToken);
    }

    private async Task<ExternalLoginResult> SignInExistingUserAsync(
        WombatIdentityUser user,
        string providerKey,
        List<string> groupIds,
        string? name,
        string? email,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
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
