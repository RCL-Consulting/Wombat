using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

public sealed record ListAssessorsForSpecialityQuery(int? SpecialityId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<AssessorProfileDto>>
{
    public ListAssessorsForSpecialityQuery(ClaimsPrincipal principal) : this(null, principal) { }
}

public sealed class ListAssessorsForSpecialityQueryHandler : IRequestHandler<ListAssessorsForSpecialityQuery, IReadOnlyList<AssessorProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ListAssessorsForSpecialityQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<AssessorProfileDto>> Handle(ListAssessorsForSpecialityQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<AssessorProfile>()
            .AsNoTracking()
            .Include(entity => entity.Institution)
            .Include(entity => entity.Speciality)
            .Include(entity => entity.SubSpeciality)
            .AsQueryable();

        if (request.SpecialityId.HasValue)
        {
            query = query.Where(entity => entity.SpecialityId == request.SpecialityId.Value);
        }

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<AssessorProfileDto>();
            }
            query = query.Where(entity => entity.InstitutionId == scopedInstitutionId.Value);
        }

        var profiles = await query
            .OrderBy(entity => entity.Speciality != null ? entity.Speciality.Name : string.Empty)
            .ThenBy(entity => entity.SubSpeciality != null ? entity.SubSpeciality.Name : string.Empty)
            .ThenBy(entity => entity.Id)
            .ToListAsync(cancellationToken);

        var usersById = new Dictionary<string, UserIdentityDetails>();
        foreach (var profile in profiles)
        {
            if (usersById.ContainsKey(profile.UserId))
            {
                continue;
            }

            var user = await _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken);
            if (user is not null)
            {
                usersById[user.UserId] = user;
            }
        }

        return profiles
            .Where(profile => usersById.ContainsKey(profile.UserId))
            .Select(profile =>
            {
                var user = usersById[profile.UserId];
                return new AssessorProfileDto(
                    profile.Id,
                    user.UserId,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    profile.Qualifications,
                    profile.InstitutionId,
                    profile.Institution.Name,
                    profile.SpecialityId,
                    profile.Speciality?.Name,
                    profile.SubSpecialityId,
                    profile.SubSpeciality?.Name,
                    profile.TrainingStatus,
                    profile.TrainingCompletedOn);
            })
            .OrderBy(profile => profile.LastName)
            .ThenBy(profile => profile.FirstName)
            .ToArray();
    }
}
