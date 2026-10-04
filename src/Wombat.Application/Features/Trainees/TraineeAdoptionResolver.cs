using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Trainees;

internal static class TraineeAdoptionResolver
{
    /// <summary>
    /// The institution's active adoptions: the curriculum versions it admits trainees into, one per discipline (T091
    /// phase 4). The one rule the admission, a profile's move and both pickers read (T304), so a picker offers only what
    /// its command accepts.
    /// </summary>
    public static IQueryable<InstitutionCurriculumAdoption> ActiveAdoptions(IApplicationDbContext dbContext, int institutionId)
        => dbContext.Set<InstitutionCurriculumAdoption>()
            .Where(adoption => adoption.InstitutionId == institutionId && adoption.IsActive);

    /// <summary>
    /// Resolves the institution's active adoption that pins a trainee to <paramref name="curriculum"/>.
    /// A trainee may only be admitted into the national curriculum version their institution has adopted
    /// for that discipline (T091 phase 4). Throws if the institution has no active adoption for the
    /// discipline, or has adopted a different version.
    /// </summary>
    public static async Task<int> ResolveAdoptionIdAsync(
        IApplicationDbContext dbContext,
        int institutionId,
        Curriculum curriculum,
        CancellationToken cancellationToken)
    {
        var activeAdoption = await ActiveAdoptions(dbContext, institutionId)
            .SingleOrDefaultAsync(entity => entity.SubSpecialityId == curriculum.SubSpecialityId, cancellationToken);

        if (activeAdoption is null)
        {
            throw new InvalidOperationException(
                "This institution has not adopted a curriculum for this discipline. Adopt one before admitting trainees.");
        }

        if (activeAdoption.CurriculumId != curriculum.Id)
        {
            throw new InvalidOperationException(
                "Trainees must be admitted into the curriculum version this institution has adopted.");
        }

        return activeAdoption.Id;
    }

    /// <summary>
    /// Resolves the adoption a trainee's profile is moved to when its curriculum changes (T304): the institution's active
    /// adoption for the trainee's own discipline, and only when it is of <paramref name="curriculumId" />. Null when the
    /// move is not into it: a version the institution has superseded, one it never adopted, or another discipline's.
    /// </summary>
    /// <remarks>
    /// Only a move is judged. A profile whose curriculum is unchanged keeps the adoption it is pinned to, superseded or
    /// not: re-adopting deactivates the old adoption, but its trainees stay pinned to the old version
    /// (<c>CurriculumAdminScope</c>, D24). Before T304 every save was judged as an admission, so no profile left on a
    /// superseded version could be saved at all.
    /// </remarks>
    public static async Task<int?> ResolveMoveAdoptionIdAsync(
        IApplicationDbContext dbContext,
        int institutionId,
        int subSpecialityId,
        int curriculumId,
        CancellationToken cancellationToken)
        => await ActiveAdoptions(dbContext, institutionId)
            .Where(adoption => adoption.SubSpecialityId == subSpecialityId && adoption.CurriculumId == curriculumId)
            .Select(adoption => (int?)adoption.Id)
            .SingleOrDefaultAsync(cancellationToken);
}
