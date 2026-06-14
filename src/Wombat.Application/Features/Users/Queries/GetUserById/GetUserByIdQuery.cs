using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(string UserId, ClaimsPrincipal Principal) : IRequest<UserDetailDto?>;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IApplicationDbContext _dbContext;

    public GetUserByIdQueryHandler(IUserAdministrationService userAdministrationService, IApplicationDbContext dbContext)
    {
        _userAdministrationService = userAdministrationService;
        _dbContext = dbContext;
    }

    public async Task<UserDetailDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator() && !request.Principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException("You do not have permission to view users.");
        }

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        // Scope guard: out-of-scope users are reported as 404, not 403, to avoid leaking id existence.
        // Unscoped (global) accounts such as Administrators are visible only to a global Administrator,
        // never to a tenant-level InstitutionalAdmin.
        if (!request.Principal.IsAdministrator()
            && (!user.InstitutionId.HasValue || !request.Principal.CanAccessInstitution(user.InstitutionId.Value)))
        {
            return null;
        }

        string? institutionName = null;
        if (user.InstitutionId.HasValue)
        {
            institutionName = await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(entity => entity.Id == user.InstitutionId.Value)
                .Select(entity => (string?)entity.Name)
                .SingleOrDefaultAsync(cancellationToken);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var invitations = await _dbContext.Set<Invitation>()
            .AsNoTracking()
            .Where(entity =>
                entity.Email == user.Email &&
                !entity.UsedOn.HasValue &&
                !entity.RevokedOn.HasValue &&
                entity.ExpiresOn >= today)
            .OrderByDescending(entity => entity.IssuedOn)
            .ToListAsync(cancellationToken);

        var pendingInvitationDtos = new List<UserPendingInvitationDto>(invitations.Count);
        if (invitations.Count > 0)
        {
            var inviteInstitutionIds = invitations
                .Where(entity => entity.InstitutionId.HasValue)
                .Select(entity => entity.InstitutionId!.Value)
                .Distinct()
                .ToArray();
            var inviteCollegeIds = invitations
                .Where(entity => entity.CollegeId.HasValue)
                .Select(entity => entity.CollegeId!.Value)
                .Distinct()
                .ToArray();
            var inviteSpecialityIds = invitations
                .Where(entity => entity.SpecialityId.HasValue)
                .Select(entity => entity.SpecialityId!.Value)
                .Distinct()
                .ToArray();

            var institutionLookup = await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(entity => inviteInstitutionIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

            var collegeLookup = await _dbContext.Set<College>()
                .AsNoTracking()
                .Where(entity => inviteCollegeIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

            var specialityLookup = await _dbContext.Set<Speciality>()
                .AsNoTracking()
                .Where(entity => inviteSpecialityIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

            foreach (var invitation in invitations)
            {
                // A CollegeAdmin invitation has no institution; show its College name in the scope column. (T093)
                var scopeName = invitation.InstitutionId.HasValue
                    ? (institutionLookup.TryGetValue(invitation.InstitutionId.Value, out var instName) ? instName : "Unknown")
                    : invitation.CollegeId.HasValue
                        ? (collegeLookup.TryGetValue(invitation.CollegeId.Value, out var collName) ? collName : "Unknown")
                        : "Unknown";

                pendingInvitationDtos.Add(new UserPendingInvitationDto(
                    invitation.Id,
                    invitation.TargetRole,
                    invitation.InstitutionId,
                    scopeName,
                    invitation.SpecialityId,
                    invitation.SpecialityId.HasValue && specialityLookup.TryGetValue(invitation.SpecialityId.Value, out var specName) ? specName : null,
                    invitation.IssuedOn,
                    invitation.ExpiresOn));
            }
        }

        return new UserDetailDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.InstitutionId,
            institutionName,
            user.SpecialityIds.ToArray(),
            user.SubSpecialityIds.ToArray(),
            user.Roles.ToArray(),
            user.IsLockedOut,
            pendingInvitationDtos);
    }
}
