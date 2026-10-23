using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

/// <summary>
/// One holder of the Assessor role on the Assessors list (T376, flow 12, Q2; boards T02, T08, T14): the person, their one
/// scope in words, and the training record if one is kept. <see cref="ProfileId" /> null means no record (the list's
/// "Not recorded", the row's Record training); a record with Not started is a record (the nit).
/// </summary>
public sealed record AssessorRowDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string? InstitutionName,
    string Scope,
    int? ProfileId,
    AssessorTrainingStatus? TrainingStatus,
    DateOnly? TrainingCompletedOn)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>A page of the Assessors list, with the count of every holder in the caller's scope (F9: the pager's).</summary>
public sealed record AssessorListDto(IReadOnlyList<AssessorRowDto> Rows, int TotalCount, int Page, int PageSize);

/// <summary>
/// Every holder of the Assessor role in the caller's scope, joined to their training record if any, by last name, a page at
/// a time (T376, flow 12, Q2, Q18, S14, F9). Until T376 the list was of assessor profiles, so a holder with no profile was
/// missing and a removed Assessor's profile was still listed, and its user lookups ran one per row.
/// </summary>
/// <remarks>
/// <para>
/// An Institutional admin lists the holders at her own institution; the Administrator every holder, an account with no
/// institution included (T14). A Trainee-holding admin is refused before anything is read (T278, T290; the page shows its
/// note instead).
/// </para>
/// <para>
/// Q18: a person whose Assessor role was removed is not listed, though their profile is kept, and is listed again, with
/// it, when the role is given back. The order is Users' (S7): last name, then first name, ignoring case, then the id.
/// </para>
/// </remarks>
public sealed record ListAssessorsQuery(ClaimsPrincipal Principal, int Page = 1, int PageSize = ListAssessorsQuery.DefaultPageSize)
    : IRequest<AssessorListDto>
{
    /// <summary>The pager's default page (Q22: a page of 20).</summary>
    public const int DefaultPageSize = 20;

    /// <summary>The pager's sizes (<c>PagerControls</c>); any other asked is the default.</summary>
    public static readonly IReadOnlyList<int> PageSizes = [10, 20, 50, 100];
}

public sealed class ListAssessorsQueryHandler : IRequestHandler<ListAssessorsQuery, AssessorListDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ListAssessorsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<AssessorListDto> Handle(ListAssessorsQuery request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var pageSize = ListAssessorsQuery.PageSizes.Contains(request.PageSize) ? request.PageSize : ListAssessorsQuery.DefaultPageSize;

        var holders = (await _userAdministrationService.ListUsersInRoleAsync(WombatRoles.Assessor, cancellationToken))
            .AsEnumerable();

        if (!request.Principal.IsAdministrator())
        {
            if (request.Principal.GetInstitutionId() is not int institutionId)
            {
                return new AssessorListDto([], 0, 1, pageSize);
            }

            holders = holders.Where(user => user.InstitutionId == institutionId);
        }

        var ordered = holders
            .OrderBy(user => user.LastName.ToLowerInvariant(), StringComparer.Ordinal)
            .ThenBy(user => user.FirstName.ToLowerInvariant(), StringComparer.Ordinal)
            .ThenBy(user => user.UserId, StringComparer.Ordinal)
            .ToArray();

        var lastPage = Math.Max(1, (int)Math.Ceiling(ordered.Length / (double)pageSize));
        var page = Math.Clamp(request.Page, 1, lastPage);
        var rows = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        // One read of the page's records and one of its names, whatever the page's size (K26: no lookup per row).
        var userIds = rows.Select(user => user.UserId).ToArray();
        var profiles = userIds.Length == 0
            ? new Dictionary<string, AssessorProfile>(StringComparer.Ordinal)
            : await _dbContext.Set<AssessorProfile>()
                .AsNoTracking()
                .Where(profile => userIds.Contains(profile.UserId))
                .ToDictionaryAsync(profile => profile.UserId, StringComparer.Ordinal, cancellationToken);
        var names = await PersonScopeNames.LoadAsync(_dbContext, rows, cancellationToken);

        return new AssessorListDto(
            rows.Select(user =>
            {
                var profile = profiles.GetValueOrDefault(user.UserId);
                return new AssessorRowDto(
                    user.UserId,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    names.InstitutionName(user),
                    names.Scope(user),
                    profile?.Id,
                    profile?.TrainingStatus,
                    profile?.TrainingCompletedOn);
            }).ToArray(),
            ordered.Length,
            page,
            pageSize);
    }
}
