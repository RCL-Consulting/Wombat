using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// The references to an entrustment scale that no foreign key protects (T109).
/// </summary>
/// <remarks>
/// Two of the three ways a scale is depended on are invisible to the database:
/// <list type="bullet">
///   <item>An activity-type schema binds a field to a ladder by <c>scale_key</c>, which is a bare string
///   holding either an id or an exact scale NAME. Nothing constrains it.</item>
///   <item>A pinned <see cref="CurriculumItem" /> asserts that its ordinals are rungs on its scale. The FK
///   protects the scale's existence; nothing protects the rungs from being removed underneath it.</item>
/// </list>
/// Both are shared here rather than written twice, because the rename path and the delete path have to agree
/// about them — a guard on one door and not the other is the same hole with an extra step.
/// </remarks>
internal static class EntrustmentScaleReferences
{
    /// <summary>
    /// Throws when a published activity-type schema binds a field to this scale by name.
    /// </summary>
    /// <remarks>
    /// Breaking that binding — by renaming the scale or deleting it — is silent and total: the key stops
    /// resolving, <c>CreditApplier</c> falls back to comparing bare ordinals, the cross-scale refusal can
    /// never fire for that type again, and the rung picker empties at the same moment. Pinned versions make
    /// it worse rather than better: an in-flight activity keeps pointing at the old version, whose schema
    /// still carries the old name, so it can never resolve again.
    /// </remarks>
    public static async Task ThrowIfNamedByAPublishedSchemaAsync(
        IApplicationDbContext dbContext,
        string scaleName,
        string action,
        CancellationToken cancellationToken)
    {
        var schemas = await dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Select(version => new { version.ActivityTypeId, version.Version, version.SchemaJson })
            .ToListAsync(cancellationToken);

        foreach (var candidate in schemas)
        {
            if (string.IsNullOrWhiteSpace(candidate.SchemaJson))
            {
                continue;
            }

            FormSchema schema;
            try
            {
                schema = FormSchemaParser.Parse(candidate.SchemaJson);
            }
            catch (SchemaParseException)
            {
                // An unparseable stored schema is a different problem and not this command's to report.
                continue;
            }

            var named = schema.Sections
                .SelectMany(section => section.Fields)
                .Any(field => string.Equals(field.ScaleKey?.Trim(), scaleName, StringComparison.Ordinal));

            if (named)
            {
                throw new InvalidOperationException(
                    $"The form schema of activity type {candidate.ActivityTypeId} (version {candidate.Version}) " +
                    $"binds to this entrustment scale by name. {action} would silently break that binding. " +
                    "Change the scale_key in the activity type first.");
            }
        }
    }

    /// <summary>
    /// Throws when a curriculum item pinned to this scale requires a rung the incoming level set does not have.
    /// </summary>
    /// <remarks>
    /// <c>CurriculumMappings.EnsureScaleCanExpressMinimaAsync</c> establishes the invariant "every ordinal a
    /// pinned item uses is a real rung on its scale", but it only runs when the ITEM changes. This is the
    /// other half: the scale can change under a pinned item too, and the validator forces the incoming level
    /// set to be contiguous from 1, so removing a level always removes the TOP rung. An item requiring that
    /// rung would become permanently unreachable — its minimum could never again be met by any assessment.
    /// </remarks>
    public static async Task ThrowIfPinnedItemNeedsARemovedRungAsync(
        IApplicationDbContext dbContext,
        int scaleId,
        IReadOnlyCollection<int> incomingOrders,
        CancellationToken cancellationToken)
    {
        var pinnedItems = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.ScaleId == scaleId)
            .Select(item => new { item.Id, item.MinimumLevelOrder, item.MinimumLevelByStageJson })
            .ToListAsync(cancellationToken);

        foreach (var item in pinnedItems)
        {
            var required = new List<int> { item.MinimumLevelOrder };
            required.AddRange(CurriculumItem.ParseStageOverrides(item.MinimumLevelByStageJson).Values);

            var unreachable = required.Where(order => !incomingOrders.Contains(order)).Distinct().Order().ToList();
            if (unreachable.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Curriculum item {item.Id} is pinned to this entrustment scale and requires level " +
                    $"{string.Join(", ", unreachable)}, which the new level set does not have. " +
                    "Change that curriculum item's minimum first.");
            }
        }
    }
}
