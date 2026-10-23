using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users.Queries.ListUsers;

/// <summary>
/// One page of Users (T376, flow 12, S6, S7, F9, Q22): the people in the caller's scope matching what was asked, by last
/// name, with their roles in order, their one scope and whether they can sign in.
/// </summary>
/// <param name="Search">The name or address typed: each word must be part of the first name, the last name or the email.</param>
/// <param name="Role">A role's key, or <see cref="UsersFilterValues.NoRole" />; null or empty for any role.</param>
/// <param name="Status"><see cref="UsersFilterValues.Active" /> or <see cref="UsersFilterValues.Locked" />; null or empty for either.</param>
/// <param name="Page">The page, from 1; past the last, the last.</param>
/// <param name="PageSize">10, 20, 50 or 100; anything else reads as <see cref="UsersFilterValues.DefaultPageSize" />.</param>
public sealed record ListUsersQuery(
    ClaimsPrincipal Principal,
    string? Search = null,
    string? Role = null,
    string? Status = null,
    int Page = 1,
    int PageSize = UsersFilterValues.DefaultPageSize) : IRequest<UserListDto>;

/// <summary>The values Users' filter is asked with, which its address carries (T376, flow 12, S6).</summary>
public static class UsersFilterValues
{
    /// <summary>The Role filter's "No role" (Q6, S2); the Web's <c>PeopleLinks.NoRole</c> equals it.</summary>
    public const string NoRole = "none";

    /// <summary>The Status filter's Active: not locked out by an administrator (K25).</summary>
    public const string Active = "active";

    /// <summary>The Status filter's Locked out: the administrative lock (K25).</summary>
    public const string Locked = "locked";

    /// <summary>Users' page of 20 (Q22).</summary>
    public const int DefaultPageSize = 20;

    /// <summary>The sizes the pager offers (PagerControls' 10, 20, 50 and 100).</summary>
    public static readonly IReadOnlyList<int> PageSizes = [10, 20, 50, 100];

    /// <summary>
    /// The roles only the Administrator may filter by (S6 (b), U14): nobody at an institution holds either, and an
    /// Institutional admin is not shown them.
    /// </summary>
    public static readonly IReadOnlyList<string> AdministratorOnlyRoles = [WombatRoles.Administrator, WombatRoles.CollegeAdmin];

    /// <summary>The most words a search is matched by; the rest are not read (a pasted paragraph is not a name).</summary>
    public const int MaxWords = 8;

    /// <summary>The words of <paramref name="search" />: trimmed, lower-cased, each once, at most <see cref="MaxWords" />.</summary>
    public static IReadOnlyList<string> WordsOf(string? search)
        => string.IsNullOrWhiteSpace(search)
            ? []
            : search
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(word => word.ToLowerInvariant())
                .Distinct(StringComparer.Ordinal)
                .Take(MaxWords)
                .ToArray();
}

public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, UserListDto>
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IApplicationDbContext _dbContext;

    public ListUsersQueryHandler(IUserAdministrationService userAdministrationService, IApplicationDbContext dbContext)
    {
        _userAdministrationService = userAdministrationService;
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <para>
    /// Someone who holds Trainee lists nobody, whatever other role they hold, and is refused before anything is read
    /// (T278, <see cref="UserAdministrationRules.DemandUserAdministration" />). The Users page asks the same rule first and
    /// sends nothing (<see cref="UserAdministrationRules.TraineeNoteOnUserPages" />).
    /// </para>
    /// <para>
    /// An Institutional admin lists her own institution's accounts only: an account with no institution (an Administrator,
    /// a College admin) is managed from the Administrator's list alone. She may not ask for Administrator or College admin
    /// (S6 (b)); the page never offers them to her, so only a crafted request is refused. A role that names no role, or a
    /// status that is neither value, is refused too: the page sends only what it offers.
    /// </para>
    /// <para>
    /// The names a row needs (its institution, College, specialities and sub-specialities) are read once for the page
    /// (<see cref="PersonScopeNames" />), and the ended programmes of only its people with no role
    /// (<see cref="EndedProgrammes" />): a constant number of queries whatever the page's size (K26).
    /// </para>
    /// </remarks>
    public async Task<UserListDto> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var isAdministrator = request.Principal.IsAdministrator();
        var (role, noRole) = RoleOf(request.Role, isAdministrator);
        var status = StatusOf(request.Status);
        var pageSize = UsersFilterValues.PageSizes.Contains(request.PageSize) ? request.PageSize : UsersFilterValues.DefaultPageSize;

        int? institutionId = null;
        if (!isAdministrator)
        {
            if (request.Principal.GetInstitutionId() is not int scoped)
            {
                return new UserListDto([], 0, 0, 1, pageSize);
            }

            institutionId = scoped;
        }

        var answer = await _userAdministrationService.ListUsersPageAsync(
            new UserDirectoryFilter(
                institutionId,
                UsersFilterValues.WordsOf(request.Search),
                role,
                noRole,
                status,
                Math.Max(1, request.Page),
                pageSize),
            cancellationToken);

        var names = await PersonScopeNames.LoadAsync(_dbContext, answer.Rows, cancellationToken);
        var withNoRole = answer.Rows.Where(user => user.Roles.Count == 0).Select(user => user.UserId).ToArray();
        var ended = await EndedProgrammes.LatestForAsync(_dbContext, withNoRole, cancellationToken);

        var rows = answer.Rows
            .Select(user => new UserSummaryDto(
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.InstitutionId,
                names.InstitutionName(user),
                WombatRoleLabels.InOrder(user.Roles),
                names.Scope(user),
                user.IsDeactivated,
                user.Roles.Count == 0 && ended.ContainsKey(user.UserId)))
            .ToArray();

        return new UserListDto(rows, answer.TotalCount, answer.MatchCount, answer.Page, pageSize);
    }

    private static (string? Role, bool NoRole) RoleOf(string? requested, bool isAdministrator)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return (null, false);
        }

        var role = requested.Trim();
        if (string.Equals(role, UsersFilterValues.NoRole, StringComparison.Ordinal))
        {
            return (null, true);
        }

        if (!WombatRoles.All.Contains(role, StringComparer.Ordinal))
        {
            throw new ArgumentException("Users cannot be filtered by that role.", nameof(requested));
        }

        if (!isAdministrator && UsersFilterValues.AdministratorOnlyRoles.Contains(role, StringComparer.Ordinal))
        {
            throw new UnauthorizedAccessException("Only a Wombat administrator can list the holders of that role.");
        }

        return (role, false);
    }

    private static UserDirectoryStatus? StatusOf(string? requested)
        => requested?.Trim() switch
        {
            null or "" => null,
            UsersFilterValues.Active => UserDirectoryStatus.Active,
            UsersFilterValues.Locked => UserDirectoryStatus.LockedOut,
            _ => throw new ArgumentException("Users cannot be filtered by that status.", nameof(requested))
        };
}
