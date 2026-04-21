using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

internal static class CurriculumMappings
{
    public static async Task<Curriculum> LoadCurriculumAsync(IApplicationDbContext dbContext, int curriculumId, CancellationToken cancellationToken)
        => await dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
            .ThenInclude(entity => entity.Speciality)
            .Include(entity => entity.Items)
            .ThenInclude(entity => entity.Epa)
            .SingleOrDefaultAsync(entity => entity.Id == curriculumId, cancellationToken)
            ?? throw new InvalidOperationException("The requested curriculum was not found.");

    public static CurriculumDto ToDto(Curriculum curriculum, int specialityId, string specialityName, string subSpecialityName, bool canEditInPlace)
        => new(
            curriculum.Id,
            specialityId,
            curriculum.SubSpecialityId,
            specialityName,
            subSpecialityName,
            curriculum.Name,
            curriculum.Version,
            curriculum.EffectiveFrom,
            curriculum.EffectiveTo,
            curriculum.IsActive,
            canEditInPlace,
            curriculum.Items
                .OrderBy(entity => entity.Epa.Code)
                .Select(entity => new CurriculumItemDto(entity.Id, entity.EpaId, entity.Epa.Code, entity.Epa.Title, entity.RequiredCount, entity.MinimumLevelOrder, entity.WindowMonths, entity.Weight, entity.MinimumLevelByStageJson))
                .ToList());

    public static void EnsureCurriculumCanBeEditedInPlace()
    {
        // T006 introduces trainee profiles. Until then, no curriculum can have attached trainees.
    }
}
