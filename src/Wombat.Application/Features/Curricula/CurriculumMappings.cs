using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Curricula;

internal static class CurriculumMappings
{
    public static async Task<Curriculum> LoadCurriculumAsync(IApplicationDbContext dbContext, int curriculumId, CancellationToken cancellationToken)
        => await dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
            .ThenInclude(entity => entity.Speciality)
            .ThenInclude(entity => entity.College)
            .Include(entity => entity.Items)
            .ThenInclude(entity => entity.Epa)
            .Include(entity => entity.Items)
            .ThenInclude(entity => entity.Scale)
            .SingleOrDefaultAsync(entity => entity.Id == curriculumId, cancellationToken)
            ?? throw new InvalidOperationException("The requested curriculum was not found.");

    public static CurriculumDto ToDto(Curriculum curriculum, int specialityId, string specialityName, string subSpecialityName, string collegeName, bool canEditInPlace)
        => new(
            curriculum.Id,
            specialityId,
            curriculum.SubSpecialityId,
            specialityName,
            subSpecialityName,
            collegeName,
            curriculum.Name,
            curriculum.Version,
            curriculum.EffectiveFrom,
            curriculum.EffectiveTo,
            curriculum.IsActive,
            canEditInPlace,
            curriculum.Items
                .OrderBy(entity => entity.Epa.Code)
                .Select(entity => new CurriculumItemDto(entity.Id, entity.EpaId, entity.Epa.Code, entity.Epa.Title, entity.RequiredCount, entity.QuotaPeriod, entity.MinimumLevelOrder, entity.WindowMonths, entity.Weight, entity.MinimumLevelByStageJson, entity.ScaleId, entity.Scale == null ? null : entity.Scale.Name))
                .ToList());

    public static void EnsureCurriculumCanBeEditedInPlace()
    {
        // T006 introduces trainee profiles. Until then, no curriculum can have attached trainees.
    }

    /// <summary>
    /// Refuses a pin to an entrustment scale that cannot express the item's minima (T109).
    /// </summary>
    /// <remarks>
    /// A wrong pin is worse than no pin. An unpinned item compares ordinals exactly as it always did, but an
    /// item pinned to the wrong ladder makes <c>CreditApplier</c> refuse credit a trainee legitimately
    /// earned — silently, and for every future completion. Requiring every ordinal the item uses to be a
    /// real rung on the chosen ladder catches the obvious half of that: a five-rung minimum of 5 cannot be
    /// pinned to a four-rung scale. It cannot catch a pin that is wrong but arithmetically plausible, which
    /// is why nothing infers this value and a human chooses it.
    /// </remarks>
    public static async Task EnsureScaleCanExpressMinimaAsync(
        IApplicationDbContext dbContext,
        int? scaleId,
        int minimumLevelOrder,
        string? minimumLevelByStageJson,
        CancellationToken cancellationToken)
    {
        if (scaleId is null)
        {
            return;
        }

        var scaleOrders = await dbContext.Set<EntrustmentLevel>()
            .Where(level => level.ScaleId == scaleId.Value)
            .Select(level => level.Order)
            .ToListAsync(cancellationToken);

        if (scaleOrders.Count == 0)
        {
            throw new InvalidOperationException(
                $"Entrustment scale {scaleId.Value} was not found, or has no levels to express a minimum on.");
        }

        var required = new List<int> { minimumLevelOrder };
        required.AddRange(CurriculumItem.ParseStageOverrides(minimumLevelByStageJson).Values);

        var unreachable = required.Where(order => !scaleOrders.Contains(order)).Distinct().Order().ToList();
        if (unreachable.Count > 0)
        {
            throw new InvalidOperationException(
                $"This curriculum item requires level {string.Join(", ", unreachable)}, which the selected " +
                $"entrustment scale does not have. The scale has {scaleOrders.Count} levels.");
        }
    }
}
