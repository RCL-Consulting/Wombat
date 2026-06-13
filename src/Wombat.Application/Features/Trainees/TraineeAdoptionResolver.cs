using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Trainees;

internal static class TraineeAdoptionResolver
{
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
        var activeAdoption = await dbContext.Set<InstitutionCurriculumAdoption>()
            .SingleOrDefaultAsync(
                entity => entity.InstitutionId == institutionId
                    && entity.SubSpecialityId == curriculum.SubSpecialityId
                    && entity.IsActive,
                cancellationToken);

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
}
