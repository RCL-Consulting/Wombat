using System.Globalization;
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
                .Select(entity => new CurriculumItemDto(entity.Id, entity.EpaId, entity.Epa.Code, entity.Epa.Title, entity.RequiredCount, entity.QuotaPeriod, entity.MinimumLevelOrder, entity.WindowMonths, entity.Weight, entity.MinimumLevelByStageJson, entity.PermittedToolsJson, entity.Epa.IsActive, entity.ScaleId, entity.Scale == null ? null : entity.Scale.Name))
                .ToList(),
            curriculum.SubSpeciality.DefaultEntrustmentScaleId);

    /// <summary>
    /// Refuses a tool list naming an instrument the vocabulary does not hold (T122). A no-op for none.
    /// </summary>
    /// <remarks>
    /// Runs before either handler mutates anything, because the audit pipeline commits a half-finished mutation
    /// when a handler throws. An unknown key is refused rather than stored: it could never match any activity type's
    /// key, so an item listing only unknown instruments would refuse every recognised tool and credit nothing.
    /// </remarks>
    public static async Task EnsurePermittedToolsExistAsync(
        IApplicationDbContext dbContext,
        IReadOnlyList<string>? permittedToolKeys,
        CancellationToken cancellationToken)
    {
        var keys = CurriculumItem.ParsePermittedTools(CurriculumItem.NormalizePermittedToolsJson(permittedToolKeys)).ToArray();
        if (keys.Length == 0)
        {
            return;
        }

        var known = await dbContext.Set<WbaTool>()
            .Where(tool => keys.Contains(tool.Key))
            .Select(tool => tool.Key)
            .ToListAsync(cancellationToken);

        var unknown = keys.Except(known, StringComparer.Ordinal).ToList();
        if (unknown.Count > 0)
        {
            throw new InvalidOperationException(
                $"{string.Join(", ", unknown.Select(key => $"'{key}'"))} {(unknown.Count == 1 ? "is not a workplace-based assessment instrument" : "are not workplace-based assessment instruments")} Wombat knows.");
        }
    }

    public static void EnsureCurriculumCanBeEditedInPlace()
    {
        // T006 introduces trainee profiles. Until then, no curriculum can have attached trainees.
    }

    /// <summary>
    /// Refuses a pin to an entrustment scale that cannot express the item's minima (T109), naming the scale, each
    /// field that does not fit and its value (T136).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A wrong pin is worse than no pin. An unpinned item compares ordinals exactly as it always did, but an
    /// item pinned to the wrong ladder makes <c>CreditApplier</c> refuse credit a trainee legitimately
    /// earned — silently, and for every future completion. Requiring every ordinal the item uses to be a
    /// real rung on the chosen ladder catches the obvious half of that: a five-rung minimum of 5 cannot be
    /// pinned to a four-rung scale. It cannot catch a pin that is wrong but arithmetically plausible, which
    /// is why nothing infers this value and a human chooses it.
    /// </para>
    /// <para>
    /// Both handlers call it before they mutate anything, because the audit pipeline commits a half-finished
    /// mutation when a handler throws. The message names every field that does not fit ("Minimum level 6",
    /// "year 4 minimum 6"), not a bare list of ordinals: an operator told only "requires level 6" cannot tell
    /// which of five values to change. When the item is being moved off another ladder,
    /// <paramref name="currentScaleId" /> is that ladder, and each value is also given as the rung it was there
    /// ("Minimum level 6 (rung 5)"), the name the operator knew it by.
    /// </para>
    /// </remarks>
    /// <param name="currentScaleId">The ladder the item is pinned to now, or null for a new or unpinned item.</param>
    public static async Task EnsureScaleCanExpressMinimaAsync(
        IApplicationDbContext dbContext,
        int? scaleId,
        int minimumLevelOrder,
        string? minimumLevelByStageJson,
        int? currentScaleId,
        CancellationToken cancellationToken)
    {
        if (scaleId is null)
        {
            return;
        }

        var scaleIds = currentScaleId is int current && current != scaleId.Value
            ? new[] { scaleId.Value, current }
            : new[] { scaleId.Value };

        var scales = await dbContext.Set<EntrustmentScale>()
            .AsNoTracking()
            .Where(scale => scaleIds.Contains(scale.Id))
            .Select(scale => new
            {
                scale.Id,
                scale.Name,
                Levels = scale.Levels.Select(level => new { level.Order, level.Label }).ToList()
            })
            .ToListAsync(cancellationToken);

        var target = scales.SingleOrDefault(scale => scale.Id == scaleId.Value);
        if (target is null || target.Levels.Count == 0)
        {
            throw new InvalidOperationException(
                $"Entrustment scale {scaleId.Value} was not found, or has no levels to express a minimum on.");
        }

        var rungs = target.Levels.Select(level => level.Order).ToHashSet();
        var offenders = Minima(minimumLevelOrder, minimumLevelByStageJson)
            .Where(minimum => !rungs.Contains(minimum.Order))
            .ToList();
        if (offenders.Count == 0)
        {
            return;
        }

        var previous = scales.SingleOrDefault(scale => scale.Id != scaleId.Value);
        var labelled = false;
        var named = new List<string>(offenders.Count);
        foreach (var (field, order) in offenders)
        {
            var ordinal = order.ToString(CultureInfo.InvariantCulture);
            var label = previous?.Levels.FirstOrDefault(level => level.Order == order)?.Label?.Trim();

            // A label that is just the ordinal again ("5" for Order 5) says nothing the number did not.
            if (string.IsNullOrEmpty(label) || string.Equals(label, ordinal, StringComparison.Ordinal))
            {
                named.Add($"{field} {ordinal}");
            }
            else
            {
                labelled = true;
                named.Add($"{field} {ordinal} (rung {label})");
            }
        }

        var list = named.Count == 1
            ? named[0]
            : $"{string.Join(", ", named.Take(named.Count - 1))} and {named[^1]}";
        var rungCount = target.Levels.Count == 1
            ? "1 rung"
            : $"{target.Levels.Count.ToString(CultureInfo.InvariantCulture)} rungs";

        var message = $"{char.ToUpperInvariant(list[0])}{list[1..]} {(named.Count == 1 ? "is not a rung" : "are not rungs")} " +
            $"on {target.Name}, which has {rungCount}.";
        if (labelled && previous is not null)
        {
            message += $" The rungs in brackets are as {previous.Name} names them.";
        }

        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Every minimum an item uses, named as the item editor names its field: the flat minimum first, then each
    /// training year in order. Read with <see cref="CurriculumItem.ParseStageOverrides" />, the reader credit uses.
    /// </summary>
    private static IEnumerable<(string Field, int Order)> Minima(int minimumLevelOrder, string? minimumLevelByStageJson)
    {
        yield return ("minimum level", minimumLevelOrder);

        foreach (var stage in CurriculumItem.ParseStageOverrides(minimumLevelByStageJson).OrderBy(entry => entry.Key))
        {
            yield return ($"year {stage.Key.ToString(CultureInfo.InvariantCulture)} minimum", stage.Value);
        }
    }
}
