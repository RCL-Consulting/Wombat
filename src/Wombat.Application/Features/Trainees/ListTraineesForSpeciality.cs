using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record ListTraineesForSpecialityQuery(int? SpecialityId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<TraineeProfileDto>>
{
    public ListTraineesForSpecialityQuery(ClaimsPrincipal principal) : this(null, principal) { }
}

public sealed class ListTraineesForSpecialityQueryHandler : IRequestHandler<ListTraineesForSpecialityQuery, IReadOnlyList<TraineeProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ListTraineesForSpecialityQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<TraineeProfileDto>> Handle(ListTraineesForSpecialityQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .AsQueryable();

        if (request.SpecialityId.HasValue)
        {
            query = query.Where(entity => entity.Curriculum.SubSpeciality.SpecialityId == request.SpecialityId.Value);
        }

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<TraineeProfileDto>();
            }
            query = query.Where(entity => entity.Curriculum.SubSpeciality.Speciality.InstitutionId == scopedInstitutionId.Value);
        }

        var profiles = await query
            .OrderBy(entity => entity.Curriculum.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.Curriculum.SubSpeciality.Name)
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
                return new TraineeProfileDto(
                    profile.Id,
                    user.UserId,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    profile.CurriculumId,
                    profile.Curriculum.Name,
                    profile.Curriculum.Version,
                    profile.Curriculum.SubSpeciality.SpecialityId,
                    profile.Curriculum.SubSpeciality.Speciality.Name,
                    profile.Curriculum.SubSpecialityId,
                    profile.Curriculum.SubSpeciality.Name,
                    profile.ProgrammeStartDate,
                    profile.ExpectedCompletionDate,
                    profile.IsActive);
            })
            .OrderBy(profile => profile.LastName)
            .ThenBy(profile => profile.FirstName)
            .ToArray();
    }
}
