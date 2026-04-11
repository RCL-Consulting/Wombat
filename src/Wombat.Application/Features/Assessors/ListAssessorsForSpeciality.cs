using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

public sealed record ListAssessorsForSpecialityQuery(int? SpecialityId = null) : IRequest<IReadOnlyList<AssessorProfileDto>>;

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
        var profiles = await _dbContext.Set<AssessorProfile>()
            .AsNoTracking()
            .Include(entity => entity.Institution)
            .Include(entity => entity.Speciality)
            .Include(entity => entity.SubSpeciality)
            .Where(entity => !request.SpecialityId.HasValue || entity.SpecialityId == request.SpecialityId.Value)
            .OrderBy(entity => entity.Speciality != null ? entity.Speciality.Name : string.Empty)
            .ThenBy(entity => entity.SubSpeciality != null ? entity.SubSpeciality.Name : string.Empty)
            .ThenBy(entity => entity.Id)
            .ToListAsync(cancellationToken);

        var users = await Task.WhenAll(profiles.Select(profile => _userAdministrationService.GetByIdAsync(profile.UserId, cancellationToken)));
        var usersById = users
            .Where(user => user is not null)
            .ToDictionary(user => user!.UserId, user => user!);

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
                    profile.SubSpeciality?.Name);
            })
            .OrderBy(profile => profile.LastName)
            .ThenBy(profile => profile.FirstName)
            .ToArray();
    }
}
