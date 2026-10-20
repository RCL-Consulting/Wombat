using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Trainees;

public sealed record ListTraineesForSpecialityQuery(int? SpecialityId, ClaimsPrincipal Principal) : IRequest<IReadOnlyList<TraineeProfileDto>>
{
    public ListTraineesForSpecialityQuery(ClaimsPrincipal principal) : this(null, principal) { }
}

public sealed class ListTraineesForSpecialityQueryHandler : IRequestHandler<ListTraineesForSpecialityQuery, IReadOnlyList<TraineeProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly TimeProvider _clock;

    /// <param name="clock">"Today" on the South African calendar, which each profile's training year is counted to (T373); the system clock unless a test pins it.</param>
    public ListTraineesForSpecialityQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userAdministrationService,
        TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<IReadOnlyList<TraineeProfileDto>> Handle(ListTraineesForSpecialityQuery request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

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
            query = query.Where(entity => entity.InstitutionId == scopedInstitutionId.Value);
        }

        var profiles = await query
            .OrderBy(entity => entity.Curriculum.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.Curriculum.SubSpeciality.Name)
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

        // T373 (T306's admission half): Current trainees shows each programme's training year today, the same 365-day
        // blocks the Trainee's Home and the credit read (TraineeProfile.StageOn).
        var today = QuotaCalendar.Today(_clock);
        var institutionIds = profiles.Select(profile => profile.InstitutionId).Distinct().ToArray();
        var institutionNames = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institutionIds.Contains(institution.Id))
            .ToDictionaryAsync(institution => institution.Id, institution => institution.Name, cancellationToken);
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
                    profile.IsActive,
                    profile.CompletedOn,
                    profile.DeactivatedOn)
                {
                    TrainingYear = TraineeProfile.StageOn(profile.ProgrammeStartDate, today),
                    InstitutionName = institutionNames.GetValueOrDefault(profile.InstitutionId)
                };
            })
            .OrderBy(profile => profile.LastName)
            .ThenBy(profile => profile.FirstName)
            .ToArray();
    }
}
