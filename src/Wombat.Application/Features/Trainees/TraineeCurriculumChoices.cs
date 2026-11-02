using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Trainees;

/// <summary>A curriculum version the trainee profile page's picker offers (T304).</summary>
public sealed record CurriculumChoiceDto(int Id, string Name, string Version, int SpecialityId, int SubSpecialityId)
{
    /// <summary>
    /// Whether this version is the institution's active adoption for the trainee's discipline: the one a move goes into
    /// (T392, flow 15, D05: "The version your institution has adopted for ‹discipline›."). Filled by
    /// <see cref="GetTraineeCurriculumChoicesQuery" /> only (N-4): a choice from anywhere else carries false whatever its
    /// adoption, so do not read it there.
    /// </summary>
    public bool IsActiveAdoption { get; init; }

    /// <summary>
    /// The day the institution superseded this version, for a pinned version that is no longer its active adoption (T392,
    /// D01: "… which your institution superseded on ‹date›."): the <c>AdoptedOn</c> of the adoption that followed the
    /// version's latest adoption in the discipline, both ordered by (<c>AdoptedOn</c>, <c>Id</c>), since going back adds a
    /// second adoption on the same day (S1). Null for the active adoption, and for a version the institution has no later
    /// adoption after. Filled by <see cref="GetTraineeCurriculumChoicesQuery" /> only.
    /// </summary>
    public DateOnly? SupersededOn { get; init; }
}

/// <summary>
/// The curricula a running trainee profile may be saved with (T304): its pinned version, which an unchanged save keeps
/// whether or not its adoption has been superseded, and the institution's active adoption for the trainee's discipline,
/// the one version a move is accepted into (<see cref="UpdateTraineeProfileCommand" />). Before T304 the picker listed
/// every curriculum the caller could open, so a registrar on 11.2 was offered 11.1, and the move back was refused.
/// </summary>
public sealed record GetTraineeCurriculumChoicesQuery(int TraineeProfileId, ClaimsPrincipal Principal)
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
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

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

        var choices = await CurriculumChoices.ListAsync(
            _dbContext.Set<Curriculum>().Where(curriculum => curriculum.Id == profile.CurriculumId || active.Contains(curriculum.Id)),
            cancellationToken);

        // T392 (flow 15, D01, D05): each choice's state for the institution, read once here, never on the page. The
        // discipline's adoptions are few (one a version adopted), so they are ordered in memory.
        var adoptions = (await _dbContext.Set<InstitutionCurriculumAdoption>()
                .AsNoTracking()
                .Where(adoption => adoption.InstitutionId == profile.InstitutionId && adoption.SubSpecialityId == profile.SubSpecialityId)
                .Select(adoption => new { adoption.Id, adoption.CurriculumId, adoption.AdoptedOn, adoption.IsActive })
                .ToListAsync(cancellationToken))
            .OrderBy(adoption => adoption.AdoptedOn)
            .ThenBy(adoption => adoption.Id)
            .ToList();

        return choices
            .Select(choice =>
            {
                if (adoptions.Any(adoption => adoption.IsActive && adoption.CurriculumId == choice.Id))
                {
                    return choice with { IsActiveAdoption = true };
                }

                var latest = adoptions.FindLastIndex(adoption => adoption.CurriculumId == choice.Id);
                return latest >= 0 && latest + 1 < adoptions.Count
                    ? choice with { SupersededOn = adoptions[latest + 1].AdoptedOn }
                    : choice;
            })
            .ToList();
    }
}

// The admission's own picker (GetAdmissionCurriculumChoicesQuery, T304) is gone with the profile page's admission branch:
// the Admit page reads what it offers from GetAdmissionCandidateQuery, narrowed to the invited sub-speciality (T373, F1).

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
