using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>A curriculum version the trainee profile page's picker offers (T304).</summary>
public sealed record CurriculumChoiceDto(int Id, string Name, string Version, int SpecialityId, int SubSpecialityId);

/// <summary>
/// The curricula a running trainee profile may be saved with (T304): its pinned version, which an unchanged save keeps
/// whether or not its adoption has been superseded, and the institution's active adoption for the trainee's discipline,
/// the one version a move is accepted into (<see cref="UpdateTraineeProfileCommand" />). Before T304 the picker listed
/// every curriculum the caller could open, so a registrar on 11.2 was offered 11.1, and the move back was refused.
/// </summary>
public sealed record GetTraineeCurriculumChoicesQuery(int TraineeProfileId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<CurriculumChoiceDto>>;

/// <summary>
/// The curricula a pending trainee may be admitted into (T304): their institution's active adoptions, one per discipline,
/// the versions <see cref="AdmitTraineeCommand" /> accepts. For an Administrator too: before T304 the admit form offered
/// them every curriculum in the catalogue, and after a re-adoption it offered everyone the superseded version.
/// </summary>
public sealed record GetAdmissionCurriculumChoicesQuery(string UserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<CurriculumChoiceDto>>;

public sealed class GetTraineeCurriculumChoicesQueryHandler
    : IRequestHandler<GetTraineeCurriculumChoicesQuery, IReadOnlyList<CurriculumChoiceDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTraineeCurriculumChoicesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CurriculumChoiceDto>> Handle(GetTraineeCurriculumChoicesQuery request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.Id == request.TraineeProfileId)
            .Select(entity => new { entity.InstitutionId, entity.CurriculumId, entity.Curriculum.SubSpecialityId })
            .SingleOrDefaultAsync(cancellationToken);

        // Out of scope reads as not found (T056), as the profile read does.
        if (profile is null || !request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new InvalidOperationException("The trainee profile could not be found.");
        }

        var active = TraineeAdoptionResolver.ActiveAdoptions(_dbContext, profile.InstitutionId)
            .Where(adoption => adoption.SubSpecialityId == profile.SubSpecialityId)
            .Select(adoption => adoption.CurriculumId);

        return await CurriculumChoices.ListAsync(
            _dbContext.Set<Curriculum>().Where(curriculum => curriculum.Id == profile.CurriculumId || active.Contains(curriculum.Id)),
            cancellationToken);
    }
}

public sealed class GetAdmissionCurriculumChoicesQueryHandler
    : IRequestHandler<GetAdmissionCurriculumChoicesQuery, IReadOnlyList<CurriculumChoiceDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public GetAdmissionCurriculumChoicesQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<CurriculumChoiceDto>> Handle(GetAdmissionCurriculumChoicesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken);

        // The admission's own scope rule (AdmitTraineeCommand): a registrar of another institution is not the caller's.
        if (user is null || (user.InstitutionId is { } own && !request.Principal.CanAccessInstitution(own)))
        {
            throw new InvalidOperationException("The pending trainee could not be found.");
        }

        // The institution the admission pins the trainee to, as AdmitTraineeCommand resolves it. None, none offered.
        if ((user.InstitutionId ?? request.Principal.GetInstitutionId()) is not int institutionId)
        {
            return [];
        }

        var active = TraineeAdoptionResolver.ActiveAdoptions(_dbContext, institutionId).Select(adoption => adoption.CurriculumId);
        return await CurriculumChoices.ListAsync(
            _dbContext.Set<Curriculum>().Where(curriculum => active.Contains(curriculum.Id)),
            cancellationToken);
    }
}

internal static class CurriculumChoices
{
    public static async Task<IReadOnlyList<CurriculumChoiceDto>> ListAsync(
        IQueryable<Curriculum> curricula,
        CancellationToken cancellationToken)
        => await curricula
            .AsNoTracking()
            .OrderBy(curriculum => curriculum.Name)
            .ThenBy(curriculum => curriculum.Version)
            .Select(curriculum => new CurriculumChoiceDto(
                curriculum.Id,
                curriculum.Name,
                curriculum.Version,
                curriculum.SubSpeciality.SpecialityId,
                curriculum.SubSpecialityId))
            .ToListAsync(cancellationToken);
}
