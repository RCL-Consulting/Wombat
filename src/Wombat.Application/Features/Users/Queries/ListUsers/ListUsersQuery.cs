using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Users.Queries.ListUsers;

public sealed record ListUsersQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<UserSummaryDto>>;

public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IApplicationDbContext _dbContext;

    public ListUsersQueryHandler(IUserAdministrationService userAdministrationService, IApplicationDbContext dbContext)
    {
        _userAdministrationService = userAdministrationService;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator() && !request.Principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException("You do not have permission to list users.");
        }

        var users = await _userAdministrationService.ListAllUsersAsync(cancellationToken);
        var query = users.AsEnumerable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<UserSummaryDto>();
            }
            // InstitutionalAdmin sees own-institution users plus unscoped (no-institution) users
            // such as global Administrators — they're visible but cannot be mutated from this surface.
            query = query.Where(user => !user.InstitutionId.HasValue || user.InstitutionId.Value == scopedInstitutionId.Value);
        }

        var filtered = query.ToArray();
        var institutionIds = filtered
            .Where(user => user.InstitutionId.HasValue)
            .Select(user => user.InstitutionId!.Value)
            .Distinct()
            .ToArray();

        var institutionsById = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(entity => institutionIds.Contains(entity.Id))
            .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

        return filtered
            .Select(user => new UserSummaryDto(
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.InstitutionId,
                user.InstitutionId.HasValue && institutionsById.TryGetValue(user.InstitutionId.Value, out var name) ? name : null,
                user.Roles.ToArray(),
                user.IsLockedOut))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToArray();
    }
}
