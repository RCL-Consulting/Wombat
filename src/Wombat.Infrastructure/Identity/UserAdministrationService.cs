using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Identity;

public sealed class UserAdministrationService : IUserAdministrationService
{
    /// <summary>The first words of a refused reset, before what was wrong with the password (T339, flow 02, E11).</summary>
    internal const string ResetRefusedMessage = "The password was not reset.";

    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IOptions<SsoOptions>? _ssoOptions;

    /// <param name="ssoOptions">
    /// The configured providers, whose names an institutional sign-in is shown by (T339). Optional, so the tests that make
    /// the service by hand need not pass it: without it a sign-in is named as it was stored when it was linked.
    /// </param>
    public UserAdministrationService(
        UserManager<WombatIdentityUser> userManager,
        ApplicationDbContext dbContext,
        IOptions<SsoOptions>? ssoOptions = null)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _ssoOptions = ssoOptions;
    }

    public async Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await LoadUsersQuery()
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Map(user, roles.ToArray());
    }

    public async Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        var userIds = usersInRole.Select(user => user.Id).ToArray();

        if (userIds.Length == 0)
        {
            return [];
        }

        var users = await LoadUsersQuery()
            .Where(entity => userIds.Contains(entity.Id))
            .OrderBy(entity => entity.LastName)
            .ThenBy(entity => entity.FirstName)
            .ToListAsync(cancellationToken);

        var usersById = users.ToDictionary(entity => entity.Id, entity => entity);

        return usersInRole
            .Where(user => usersById.ContainsKey(user.Id))
            .Select(user => Map(usersById[user.Id], [role]))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToArray();
    }

    public async Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var ids = userIds.Distinct(StringComparer.Ordinal).ToArray();
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(entity => ids.Contains(entity.Id))
            .Select(entity => new { entity.Id, entity.FirstName, entity.LastName })
            .ToListAsync(cancellationToken);

        return users.ToDictionary(user => user.Id, user => $"{user.FirstName} {user.LastName}".Trim(), StringComparer.Ordinal);
    }

    /// <summary>
    /// One query over exactly the users asked about, reading only their names and emails: the campaign form's trainee
    /// picker labels an institution's current trainees by it, and would otherwise read each one's account and roles in
    /// turn (T248).
    /// </summary>
    public async Task<IReadOnlyDictionary<string, UserContact>> GetContactsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<string, UserContact>(StringComparer.Ordinal);
        }

        var ids = userIds.Distinct(StringComparer.Ordinal).ToArray();
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(entity => ids.Contains(entity.Id))
            .Select(entity => new { entity.Id, entity.FirstName, entity.LastName, entity.Email })
            .ToListAsync(cancellationToken);

        return users.ToDictionary(
            user => user.Id,
            user => new UserContact(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty),
            StringComparer.Ordinal);
    }

    /// <summary>
    /// One query over the role links of exactly the users asked about, never the role's whole listing: who may sit on a
    /// decision panel asks it of the committee members at one institution, and would otherwise load every trainee in the
    /// country with their scopes on each review page (T237).
    /// </summary>
    public Task<IReadOnlySet<string>> WhichHoldRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        CancellationToken cancellationToken = default)
        => HoldersOfRoleAsync(userIds, role, activeOnly: false, cancellationToken);

    /// <summary>
    /// <see cref="WhichHoldRoleAsync" />, less every account an administrator has locked or an erasure has closed
    /// (<see cref="UserDeactivation" />), in the same one query: a current trainee is asked of it (T268). A brute-force
    /// lockout, which lifts itself after minutes, leaves an account in the answer.
    /// </summary>
    public Task<IReadOnlySet<string>> WhichActivelyHoldRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        CancellationToken cancellationToken = default)
        => HoldersOfRoleAsync(userIds, role, activeOnly: true, cancellationToken);

    private async Task<IReadOnlySet<string>> HoldersOfRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var ids = userIds.Distinct(StringComparer.Ordinal).ToArray();
        var normalizedRole = _userManager.NormalizeName(role);
        var holders = _dbContext.UserRoles
            .AsNoTracking()
            .Where(link => ids.Contains(link.UserId))
            .Join(
                _dbContext.Roles.Where(entity => entity.NormalizedName == normalizedRole),
                link => link.RoleId,
                entity => entity.Id,
                (link, _) => link.UserId);

        if (activeOnly)
        {
            // The same threshold NomineeDirectory translates (UserDeactivation): a constant, so the SQL never changes.
            holders = holders.Join(
                _dbContext.Users.Where(entity =>
                    entity.LockoutEnd == null || entity.LockoutEnd < UserDeactivation.Threshold),
                userId => userId,
                entity => entity.Id,
                (userId, _) => userId);
        }

        return (await holders.Distinct().ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await LoadUsersQuery()
            .OrderBy(entity => entity.LastName)
            .ThenBy(entity => entity.FirstName)
            .ToListAsync(cancellationToken);

        var results = new List<UserIdentityDetails>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            results.Add(Map(user, roles.ToArray()));
        }
        return results;
    }

    public async Task<AccountSignInMethods?> GetSignInMethodsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Users
            .AsNoTracking()
            .Where(entity => entity.Id == userId)
            .Select(entity => new { entity.AllowLocalPassword, HasPassword = entity.PasswordHash != null })
            .SingleOrDefaultAsync(cancellationToken);
        if (account is null)
        {
            return null;
        }

        var logins = await _dbContext.UserLogins
            .AsNoTracking()
            .Where(login => login.UserId == userId)
            .ToListAsync(cancellationToken);

        // Each provider once: an account holds at most one subject a provider asserts (ExternalLoginHandler refuses a
        // second link), and a removal names the provider, not the subject.
        var signIns = logins
            .GroupBy(login => login.LoginProvider, StringComparer.Ordinal)
            .Select(group => new InstitutionalSignIn(group.Key, DisplayNameOf(group.First())))
            .OrderBy(signIn => signIn.DisplayName, StringComparer.CurrentCulture)
            .ThenBy(signIn => signIn.Provider, StringComparer.Ordinal)
            .ToList();

        return new AccountSignInMethods(account.AllowLocalPassword && account.HasPassword, signIns);
    }

    /// <summary>
    /// The guard and the removal, in that order (T339, flow 02; the round 2 review's B-D6). The account is read before its
    /// logins, and <see cref="UserManager{TUser}.RemoveLoginAsync" /> saves the removal with the account's new security
    /// stamp against the concurrency stamp read here. So a removal another tab saved before the logins were read is seen by
    /// the count, and one saved after the account was read fails the save: two tabs cannot each leave the other's sign-in
    /// as the last way in and then remove it too.
    /// </summary>
    public async Task<InstitutionalSignInRemoval> RemoveInstitutionalSignInAsync(
        string userId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        // Tracked: in a request that has already read the account (the endpoint's session check), this is that instance,
        // so the concurrency stamp is the one read first.
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        var logins = await _dbContext.UserLogins
            .AsNoTracking()
            .Where(login => login.UserId == userId)
            .ToListAsync(cancellationToken);

        var target = logins.FirstOrDefault(login => string.Equals(login.LoginProvider, provider, StringComparison.Ordinal));
        if (target is null)
        {
            return InstitutionalSignInRemoval.NotLinked;
        }

        // The last way in: a password that cannot sign the account in (AllowLocalPassword false, as the sign-in endpoint
        // refuses it) is no way in, whatever the row holds.
        var hasPassword = user.AllowLocalPassword && user.PasswordHash is not null;
        if (!hasPassword && logins.Count(login => !ReferenceEquals(login, target)) == 0)
        {
            return InstitutionalSignInRemoval.LastWayIn;
        }

        var result = await _userManager.RemoveLoginAsync(user, target.LoginProvider, target.ProviderKey);
        if (result.Succeeded)
        {
            return InstitutionalSignInRemoval.Removed;
        }

        // Identity's store catches the concurrency conflict and answers a failed result, leaving the login's deletion and
        // the account's new stamps staged. The handler throws on this answer, and the audit pipeline's row would send them
        // again (T201): take them back, and read the account as it now stands.
        foreach (var entry in _dbContext.ChangeTracker.Entries<IdentityUserLogin<string>>()
                     .Where(entry => entry.Entity.UserId == userId && entry.State == EntityState.Deleted)
                     .ToList())
        {
            entry.State = EntityState.Detached;
        }

        await _dbContext.Entry(user).ReloadAsync(cancellationToken);
        return InstitutionalSignInRemoval.Conflict;
    }

    /// <summary>
    /// The provider's name as configured now, else as it was stored when the sign-in was linked, else its key: a provider
    /// renamed in configuration is shown by its new name.
    /// </summary>
    private string DisplayNameOf(IdentityUserLogin<string> login)
    {
        var configured = _ssoOptions?.Value.Providers
            .FirstOrDefault(provider => string.Equals(provider.Key, login.LoginProvider, StringComparison.Ordinal))?
            .DisplayName;

        return !string.IsNullOrWhiteSpace(configured) ? configured
            : !string.IsNullOrWhiteSpace(login.ProviderDisplayName) ? login.ProviderDisplayName
            : login.LoginProvider;
    }

    public async Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    public async Task UpdateScopeAsync(
        string userId,
        int institutionId,
        IReadOnlyCollection<int> specialityIds,
        IReadOnlyCollection<int> subSpecialityIds,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(entity => entity.SpecialityScopes)
            .Include(entity => entity.SubSpecialityScopes)
            .SingleOrDefaultAsync(entity => entity.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("The user could not be found.");

        // An external login belongs to an institution's identity provider, and SSO refuses an account that is not in
        // the provider's institution (T149). So a move drops the old links, in this one save: otherwise the stale link
        // is found first at every sign-in and the moved user is refused for ever, and an SSO-only user could not link
        // at the new institution either.
        if (user.InstitutionId != institutionId)
        {
            var logins = await _dbContext.UserLogins.Where(login => login.UserId == userId).ToListAsync(cancellationToken);
            _dbContext.UserLogins.RemoveRange(logins);
        }

        var desiredSpecialityIds = specialityIds.Distinct().OrderBy(id => id).ToArray();
        var desiredSubSpecialityIds = subSpecialityIds.Distinct().OrderBy(id => id).ToArray();

        // The institution and the scopes are claims every sign-in carries (WombatUserClaimsPrincipalFactory), and gates
        // read them from the claims. So a change to them changes the security stamp, in this one save, as a change of roles
        // does: every session already signed in to the account is signed out within SessionRevalidation.Interval, rather
        // than working on at the old institution for as long as its cookie or circuit lives (T279). A save that changes
        // neither leaves the stamp, and so the account's sessions, alone.
        var scopeChanged = user.InstitutionId != institutionId ||
                           !user.SpecialityScopes.Select(scope => scope.SpecialityId).Distinct().Order()
                               .SequenceEqual(desiredSpecialityIds) ||
                           !user.SubSpecialityScopes.Select(scope => scope.SubSpecialityId).Distinct().Order()
                               .SequenceEqual(desiredSubSpecialityIds);
        if (scopeChanged)
        {
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
        }

        user.InstitutionId = institutionId;

        SyncScopes(
            user.SpecialityScopes,
            desiredSpecialityIds,
            existing => existing.SpecialityId,
            value => new WombatIdentityUserSpecialityScope
            {
                UserId = userId,
                SpecialityId = value
            });

        SyncScopes(
            user.SubSpecialityScopes,
            desiredSubSpecialityIds,
            existing => existing.SubSpecialityId,
            value => new WombatIdentityUserSubSpecialityScope
            {
                UserId = userId,
                SubSpecialityId = value
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (!await _userManager.IsInRoleAsync(user, WombatRoles.PendingTrainee))
        {
            throw new InvalidOperationException("Only users in the PendingTrainee role can be admitted.");
        }

        var removeResult = await _userManager.RemoveFromRoleAsync(user, WombatRoles.PendingTrainee);
        if (!removeResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", removeResult.Errors.Select(error => error.Description)));
        }

        var addResult = await _userManager.AddToRoleAsync(user, WombatRoles.Trainee);
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", addResult.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(ResetRefusal(result.Errors));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The user could not be found.");

        // Lockout uses DateTimeOffset.MaxValue as the "indefinite" sentinel. Clearing the lockout
        // requires setting LockoutEnd to null (UserManager.SetLockoutEndDateAsync only writes the
        // column; we also need lockout enabled to be true to honour it).
        if (locked)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
        }

        var lockoutEnd = locked ? UserDeactivation.IndefiniteLockoutEnd : (DateTimeOffset?)null;
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await _userManager.UpdateSecurityStampAsync(user);
    }

    /// <summary>
    /// What the administrator's reset card says when Identity refuses the new password: that it was not reset, then each
    /// rule broken, once, in the one order the rules are always listed in, under their one heading (T339, flow 02, E11).
    /// Until T339 it was Identity's descriptions joined with "; ", in the order Identity found them. The words are
    /// <see cref="WombatIdentityErrorDescriber" />'s, which Identity's own errors carry; any other refusal reads as its
    /// description.
    /// </summary>
    internal static string ResetRefusal(IEnumerable<IdentityError> errors)
    {
        var all = errors.ToList();
        var others = all
            .Where(error => !WombatIdentityErrorDescriber.IsRule(error.Code))
            .Select(error => error.Description)
            .Distinct(StringComparer.Ordinal);
        var rules = WombatIdentityErrorDescriber.RuleOrder
            .Select(code => all.FirstOrDefault(error => string.Equals(error.Code, code, StringComparison.Ordinal)))
            .OfType<IdentityError>()
            .Select(error => error.Description)
            .ToList();

        var words = new List<string> { ResetRefusedMessage };
        words.AddRange(others);
        if (rules.Count > 0)
        {
            words.Add(WombatIdentityErrorDescriber.Heading);
            words.AddRange(rules);
        }

        return string.Join(" ", words);
    }

    private IQueryable<WombatIdentityUser> LoadUsersQuery()
        => _dbContext.Users
            .AsNoTracking()
            .Include(entity => entity.SpecialityScopes)
            .Include(entity => entity.SubSpecialityScopes);

    private static UserIdentityDetails Map(WombatIdentityUser user, IReadOnlyCollection<string> roles)
        => new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.InstitutionId,
            user.SpecialityScopes.Select(scope => scope.SpecialityId).Distinct().ToArray(),
            user.SubSpecialityScopes.Select(scope => scope.SubSpecialityId).Distinct().ToArray(),
            roles,
            IsLockedOut: user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            IsDeactivated: UserDeactivation.IsDeactivated(user.LockoutEnd));

    private static void SyncScopes<TScope>(
        ICollection<TScope> currentScopes,
        IReadOnlyCollection<int> desiredValues,
        Func<TScope, int> getValue,
        Func<int, TScope> createScope)
        where TScope : class
    {
        var toRemove = currentScopes
            .Where(existing => !desiredValues.Contains(getValue(existing)))
            .ToArray();

        foreach (var scope in toRemove)
        {
            currentScopes.Remove(scope);
        }

        var existingValues = currentScopes.Select(getValue).ToHashSet();
        foreach (var value in desiredValues)
        {
            if (!existingValues.Contains(value))
            {
                currentScopes.Add(createScope(value));
            }
        }
    }
}
