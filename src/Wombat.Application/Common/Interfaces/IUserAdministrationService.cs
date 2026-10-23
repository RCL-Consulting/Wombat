namespace Wombat.Application.Common.Interfaces;

public interface IUserAdministrationService
{
    Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The account whose email is <paramref name="email" />, compared trimmed and whatever its case (T373, flow 11, Q12,
    /// S4); null when none holds it. An invitation is refused at issue for an address an account already holds (T286's issue
    /// half). The default implementation reads every user; the real service overrides it with one query on Identity's
    /// normalised email.
    /// </summary>
    async Task<UserIdentityDetails?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);
        var wanted = email.Trim();
        return (await ListAllUsersAsync(cancellationToken))
            .FirstOrDefault(user => string.Equals(user.Email.Trim(), wanted, StringComparison.OrdinalIgnoreCase));
    }

    Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// "First Last" for exactly the users asked about, whatever roles they hold. A user who does not exist is
    /// left out. The default implementation reads every user; the real service overrides it with one query by id.
    /// </summary>
    async Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListAllUsersAsync(cancellationToken))
            .Where(user => wanted.Contains(user.UserId))
            .ToDictionary(user => user.UserId, user => $"{user.FirstName} {user.LastName}".Trim(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Which of exactly the users asked about hold <paramref name="role" />. A user who does not exist, or does not hold
    /// it, is left out. The default implementation reads the role's whole listing; the real service overrides it with one
    /// query by id, so asking about a panel's few candidates does not load every holder of the role. (T237)
    /// </summary>
    async Task<IReadOnlySet<string>> WhichHoldRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListUsersInRoleAsync(role, cancellationToken))
            .Select(user => user.UserId)
            .Where(wanted.Contains)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Which of exactly the users asked about hold <paramref name="role" /> on an account that is not deactivated
    /// (<see cref="UserIdentityDetails.IsDeactivated" />: an administrator's lock or an erasure, never a brute-force
    /// lockout that lifts itself after minutes). A user who does not exist, does not hold the role, or is deactivated is
    /// left out. The account half of a current trainee (<c>TraineeScopeResolver</c>, T238, T268). The default
    /// implementation reads the role's whole listing; the real service overrides it with one query by id.
    /// </summary>
    async Task<IReadOnlySet<string>> WhichActivelyHoldRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListUsersInRoleAsync(role, cancellationToken))
            .Where(user => !user.IsDeactivated)
            .Select(user => user.UserId)
            .Where(wanted.Contains)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// The name and email of exactly the users asked about, whatever roles they hold: what a picker labels a person by.
    /// A user who does not exist is left out. The default implementation reads every user; the real service overrides it
    /// with one query by id, so a picker of an institution's trainees reads only them. (T248)
    /// </summary>
    async Task<IReadOnlyDictionary<string, UserContact>> GetContactsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListAllUsersAsync(cancellationToken))
            .Where(user => wanted.Contains(user.UserId))
            .ToDictionary(
                user => user.UserId,
                user => new UserContact(user.UserId, user.FirstName, user.LastName, user.Email),
                StringComparer.Ordinal);
    }

    /// <summary>
    /// How the account signs in: whether it has a Wombat password that signs it in, and its institutional sign-ins (T339,
    /// flow 02; T286's list). Null for an account that does not exist. The default implementation throws: only the real
    /// service reads the logins, and a test directory that never shows My account need not.
    /// </summary>
    Task<AccountSignInMethods?> GetSignInMethodsAsync(string userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This user directory does not read sign-in methods.");

    /// <summary>
    /// Removes the account's institutional sign-in through <paramref name="provider" /> (T339, flow 02; T286's removal),
    /// unless it is the account's last way in: no password that signs it in, and no other institutional sign-in. The check
    /// and the removal are one: the account is read before its logins, and the removal is saved against the account's
    /// concurrency stamp, so a second tab that removed another sign-in in between makes this one a
    /// <see cref="InstitutionalSignInRemoval.Conflict" />, never a second removal past the guard. Nothing is left staged
    /// when it answers anything but <see cref="InstitutionalSignInRemoval.Removed" /> (the audit pipeline saves what a
    /// failed handler left staged). The default implementation throws, as <see cref="GetSignInMethodsAsync" />'s does.
    /// </summary>
    Task<InstitutionalSignInRemoval> RemoveInstitutionalSignInAsync(
        string userId,
        string provider,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This user directory does not remove sign-in methods.");

    Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task UpdateScopeAsync(
        string userId,
        int institutionId,
        IReadOnlyCollection<int> specialityIds,
        IReadOnlyCollection<int> subSpecialityIds,
        CancellationToken cancellationToken = default);
    Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="work" /> in one database transaction on the context the user store saves through, committed
    /// only when it returns (T373 fix pass, B2). The admission saves the profile, the scope and the role swap inside it, so
    /// a fault in the swap leaves the registrar pending with nothing saved, and a retry admits; until the fix the remove of
    /// PendingTrainee committed before the add of Trainee could fail. On a fault what the work left tracked is discarded,
    /// as the accept's (<see cref="IInvitedUserProvisioner.InOneTransactionAsync{T}" />). The default implementation runs
    /// the work as it is: a test directory has no database to hold it in.
    /// </summary>
    Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        return work(cancellationToken);
    }

    Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
    Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of Users (T376, flow 12, S6, S7, K26; build-contracts § 1): the accounts in <paramref name="filter" />'s
    /// institution (every account, those with none included, when it names none), matched, ordered and paged by the
    /// filter, each with its roles; the count in that scope and the count that matched. The real service answers it in SQL,
    /// the roles of the page's rows in one query, so no row's roles are read on their own (K26: until T376 Users read every
    /// account, then each one's roles in turn). The default implementation reads every user and applies the same rules in
    /// memory, so a test directory need not override it.
    /// </summary>
    async Task<UserDirectoryPage> ListUsersPageAsync(UserDirectoryFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var scoped = (await ListAllUsersAsync(cancellationToken))
            .Where(user => filter.InstitutionId is not int institutionId || user.InstitutionId == institutionId)
            .ToArray();
        var matched = scoped
            .Where(filter.Matches)
            .OrderBy(user => user.LastName.ToLowerInvariant(), StringComparer.Ordinal)
            .ThenBy(user => user.FirstName.ToLowerInvariant(), StringComparer.Ordinal)
            .ThenBy(user => user.UserId, StringComparer.Ordinal)
            .ToArray();

        var page = filter.PageWithin(matched.Length);
        return new UserDirectoryPage(
            matched.Skip((page - 1) * filter.PageSize).Take(filter.PageSize).ToArray(),
            scoped.Length,
            matched.Length,
            page);
    }
}

/// <param name="IsLockedOut">Locked out right now, for any reason: an administrator's lock, an erasure, or a brute-force
/// lockout that lifts itself after minutes.</param>
/// <param name="IsDeactivated">
/// Locked indefinitely, as an administrator's lock and an erasure leave an account, and not merely locked out for a few
/// minutes by failed passwords (<c>UserDeactivation</c>, T102). A deactivated account can sit on no committee panel
/// (T165).
/// </param>
/// <param name="CollegeId">
/// The College a College admin administers (T093), which a College admin's scope reads as (T376, flow 12, S1,
/// <c>PersonScopeWords</c>); null for everyone else. Last and defaulted, so the directories tests build by hand need not
/// name it.
/// </param>
public sealed record UserIdentityDetails(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut = false,
    bool IsDeactivated = false,
    int? CollegeId = null);

/// <summary>What a picker labels a person by: their name and email, and nothing about their roles or scope. (T248)</summary>
public sealed record UserContact(string UserId, string FirstName, string LastName, string Email);

/// <summary>How an account signs in (T339, flow 02).</summary>
/// <param name="HasLocalPassword">
/// A Wombat password signs the account in: it may use one (<c>AllowLocalPassword</c>, which the sign-in and change password
/// endpoints hold it to) and it has one. An account provisioned by its institution's sign-in has neither.
/// </param>
/// <param name="InstitutionalSignIns">Its institutional sign-ins, each once, by the provider's key.</param>
/// <param name="AllowsLocalPassword">
/// The account may use a Wombat password at all (<c>AllowLocalPassword</c>), whether or not it has one. False for an account
/// its institution's sign-in provisioned: a reset would set a password no sign-in accepts, so the user page offers none and
/// says the person signs in only through the provider (T376, flow 12, F3). Last and defaulted, so flow 02's readers and the
/// tests that build one by hand are unchanged.
/// </param>
public sealed record AccountSignInMethods(
    bool HasLocalPassword,
    IReadOnlyList<InstitutionalSignIn> InstitutionalSignIns,
    bool AllowsLocalPassword = true);

/// <summary>
/// One institutional sign-in linked to an account: the provider's key, which a removal names, and the name a person reads.
/// Wombat stores no date for a link (<c>AspNetUserLogins</c> has none), so none is shown (T339, C1).
/// </summary>
public sealed record InstitutionalSignIn(string Provider, string DisplayName);

/// <summary>What a removal of an institutional sign-in did (T339, flow 02).</summary>
public enum InstitutionalSignInRemoval
{
    /// <summary>The sign-in is gone, and the account's security stamp has changed with it.</summary>
    Removed,

    /// <summary>The account has no sign-in through that provider: never linked, or removed already (a second tab).</summary>
    NotLinked,

    /// <summary>It is the account's last way in: no password that signs it in, and no other institutional sign-in.</summary>
    LastWayIn,

    /// <summary>The account changed after it was read (a second tab removed another sign-in); nothing was removed.</summary>
    Conflict
}

/// <summary>Users' Status filter (T376, flow 12, Q6): the administrative lock only (K25), never a brute-force lockout.</summary>
public enum UserDirectoryStatus
{
    /// <summary>Not deactivated: the account can sign in, a brute-force lockout that lifts itself aside.</summary>
    Active,

    /// <summary>Deactivated (<see cref="UserIdentityDetails.IsDeactivated" />): an administrator's lock, or an erasure.</summary>
    LockedOut
}

/// <summary>
/// What one page of Users asks the directory for (T376, flow 12, S6; <see cref="IUserAdministrationService.ListUsersPageAsync" />).
/// </summary>
/// <param name="InstitutionId">
/// The institution whose accounts are listed; null lists every account, those with no institution included: the
/// Administrator's list alone (an Institutional admin's names her own institution, so an unscoped account never reaches it).
/// </param>
/// <param name="Words">
/// The words typed, each already trimmed and lower-cased: an account matches when every word is part of its first name,
/// its last name or its email (S6: "thandi zulu", "zulu thandi", "zul" and the address all find Thandi Zulu). None matches
/// every account.
/// </param>
/// <param name="Role">A role's key: only the accounts that hold it. Null for any role.</param>
/// <param name="NoRole">Only the accounts that hold no role at all (Q6, S2). <paramref name="Role" /> is then null.</param>
/// <param name="Status">Active or locked out (K25); null for either.</param>
/// <param name="Page">The page asked for, from 1; past the last page the last is answered.</param>
/// <param name="PageSize">The rows on a page.</param>
public sealed record UserDirectoryFilter(
    int? InstitutionId,
    IReadOnlyList<string> Words,
    string? Role,
    bool NoRole,
    UserDirectoryStatus? Status,
    int Page,
    int PageSize)
{
    /// <summary>Whether <paramref name="user" /> answers the filter's words, role and status (its institution aside).</summary>
    public bool Matches(UserIdentityDetails user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var wordsMatch = Words.All(word =>
            user.FirstName.Contains(word, StringComparison.OrdinalIgnoreCase) ||
            user.LastName.Contains(word, StringComparison.OrdinalIgnoreCase) ||
            user.Email.Contains(word, StringComparison.OrdinalIgnoreCase));
        var roleMatches = NoRole
            ? user.Roles.Count == 0
            : Role is null || user.Roles.Contains(Role, StringComparer.Ordinal);
        var statusMatches = Status switch
        {
            UserDirectoryStatus.Active => !user.IsDeactivated,
            UserDirectoryStatus.LockedOut => user.IsDeactivated,
            _ => true
        };
        return wordsMatch && roleMatches && statusMatches;
    }

    /// <summary>The page answered for <paramref name="matchCount" /> matching accounts: <see cref="Page" />, or the last.</summary>
    public int PageWithin(int matchCount)
    {
        var size = Math.Max(1, PageSize);
        var lastPage = Math.Max(1, (int)Math.Ceiling(matchCount / (double)size));
        return Math.Clamp(Page, 1, lastPage);
    }
}

/// <summary>One page of Users, as the directory answers it (T376, flow 12, S6).</summary>
/// <param name="Rows">The page's accounts, in S7's order (last name, first name, id, each ignoring case), with their roles.</param>
/// <param name="TotalCount">Every account in the filter's institution (all of them for the Administrator), unfiltered.</param>
/// <param name="MatchCount">The accounts that matched, across every page.</param>
/// <param name="Page">The page answered (the last when one past it was asked for).</param>
public sealed record UserDirectoryPage(
    IReadOnlyList<UserIdentityDetails> Rows,
    int TotalCount,
    int MatchCount,
    int Page);
