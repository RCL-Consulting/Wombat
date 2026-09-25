using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Trainees;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// The trainees the caller may start a multi-source feedback campaign about, each with their active profile: the campaign
/// form's trainee picker. Exactly the trainees <see cref="CreateMsfCampaignCommand" /> accepts, by the same rule
/// (<see cref="MsfCampaignRules.CampaignSubjectsAsync" />, <see cref="MsfCampaignRules.MayStartCampaignAboutAsync" />).
/// (T238)
/// </summary>
/// <remarks>
/// Until T238 the form listed every trainee profile at the caller's institution through the admin trainees list
/// (<see cref="ListTraineesForSpecialityQuery" />), which shows completed and withdrawn profiles by design. So it offered
/// graduates, trainees who had withdrawn and profiles that had outlived their user's Trainee role, and it offered a
/// trainee with two profiles there twice. A current trainee has one active profile, so each is offered once.
/// </remarks>
public sealed record ListMsfCampaignSubjectsQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<TraineeProfileDto>>;

public sealed class ListMsfCampaignSubjectsQueryHandler
    : IRequestHandler<ListMsfCampaignSubjectsQuery, IReadOnlyList<TraineeProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListMsfCampaignSubjectsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<IReadOnlyList<TraineeProfileDto>> Handle(
        ListMsfCampaignSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjectUserIds = await MsfCampaignRules.CampaignSubjectsAsync(
            _dbContext, _users, request.Principal, cancellationToken);

        if (subjectUserIds.Count == 0)
        {
            return [];
        }

        var profiles = await TraineeScopeResolver.ActiveProfiles(_dbContext)
            .AsNoTracking()
            .Where(profile => subjectUserIds.Contains(profile.UserId))
            .Include(profile => profile.Curriculum)
                .ThenInclude(curriculum => curriculum.SubSpeciality)
                    .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .ToListAsync(cancellationToken);

        // One read of exactly these accounts' names and emails, not one account and its roles per trainee (T248 review).
        var contacts = await _users.GetContactsAsync(subjectUserIds, cancellationToken);

        var offered = new List<TraineeProfileDto>(profiles.Count);
        foreach (var profile in profiles)
        {
            // Every current trainee has an account; one removed since the rule was read is left out, not shown by id.
            if (!contacts.TryGetValue(profile.UserId, out var user))
            {
                continue;
            }

            offered.Add(new TraineeProfileDto(
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
                profile.DeactivatedOn));
        }

        return offered
            .OrderBy(trainee => trainee.LastName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(trainee => trainee.FirstName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(trainee => trainee.UserId, StringComparer.Ordinal)
            .ToArray();
    }
}
