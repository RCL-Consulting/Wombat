using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// The references to an entrustment scale that its rename and delete paths ask about before they write (T109, T232, T253).
/// </summary>
/// <remarks>
/// Two of the ways a scale is depended on are invisible to the database:
/// <list type="bullet">
///   <item>An activity-type schema binds a field to a ladder by <c>scale_key</c>, a bare string holding the scale's
///   id, <c>seed:</c> and its seed key, or its exact name (<see cref="ScaleBinding" />). Nothing constrains it.</item>
///   <item>A pinned <see cref="CurriculumItem" /> asserts that its ordinals are rungs on its scale. The FK
///   protects the scale's existence; nothing protects the rungs from being removed underneath it.</item>
/// </list>
/// Both are shared here rather than written twice, because the rename path and the delete path have to agree
/// about them — a guard on one door and not the other is the same hole with an extra step.
/// <para>
/// A third, a sub-speciality's default scale, the database does protect, but only with a refusal the administrator
/// cannot read; see <see cref="ThrowIfDefaultOfASubSpecialityAsync" />.
/// </para>
/// </remarks>
internal static class EntrustmentScaleReferences
{
    /// <summary>
    /// Throws when a published activity-type schema binds a field to this scale, by id, seed key or name, naming each
    /// such type (T253).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Deleting the scale would unbind that field silently and for good: the key stops resolving, <c>CreditApplier</c>
    /// falls back to comparing bare ordinals, the cross-scale refusal can never fire for that type again, and the form
    /// stops offering the ladder's rungs (it offers the field's own options, or asks for a bare number). Every version is
    /// asked about, not only the current one: a published version never changes and an activity stays on the version it
    /// was filed on, so no later publish undoes a binding an earlier one made.
    /// </para>
    /// <para>
    /// The refusal does not suggest a rename as the way out. A rename keeps every form that binds the scale by id or seed
    /// key, but unbinds one that binds it by name, which is this same damage (T253 review); the rename's own warning names
    /// such a form.
    /// </para>
    /// <para>
    /// "Binds" is <see cref="ScaleBinding.Binds(EntrustmentScale)" />, the reading every resolver shares
    /// (<see cref="EntrustmentScaleBindings" />). Before T253 this compared names only, so a type the builder bound by
    /// id, which is how the builder binds, did not stop the delete.
    /// </para>
    /// </remarks>
    public static async Task ThrowIfBoundByAPublishedSchemaAsync(
        IApplicationDbContext dbContext,
        EntrustmentScale scale,
        CancellationToken cancellationToken)
    {
        var bound = await FindPublishedBindingsAsync(dbContext, binding => binding.Binds(scale), cancellationToken);
        if (bound.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            (bound.Count == 1
                ? $"The form of the activity type {NameEach(bound)} uses this entrustment scale, so it cannot be deleted. "
                : $"The forms of the activity types {NameEach(bound)} use this entrustment scale, so it cannot be deleted. ") +
            "A published version never changes, and activities stay on the version they were filed on, so no later " +
            "version can take that back. Leave the scale in place instead.");
    }

    /// <summary>
    /// What a rename from <paramref name="oldName" /> to <paramref name="newName" /> does to the published schemas that
    /// bind this scale by its old name, naming each such type; or null when none does (T253).
    /// </summary>
    /// <remarks>
    /// A rename refuses nothing: the seeds bind by seed key and the builder by id, and neither moves with the name. A
    /// schema that binds by name was written before T253 or by hand, and the rename leaves its fields bound to nothing.
    /// That is the administrator's to hear in the same save, with what brings the binding back.
    /// </remarks>
    public static async Task<string?> DescribeWhatARenameUnbindsAsync(
        IApplicationDbContext dbContext,
        string oldName,
        string newName,
        CancellationToken cancellationToken)
    {
        var bound = await FindPublishedBindingsAsync(
            dbContext,
            binding => binding.Kind == ScaleBindingKind.Name && string.Equals(binding.Name, oldName, StringComparison.Ordinal),
            cancellationToken);
        if (bound.Count == 0)
        {
            return null;
        }

        return (bound.Count == 1
                   ? $"The form of the activity type {NameEach(bound)} names this scale by its old name, \"{oldName}\", "
                   : $"The forms of the activity types {NameEach(bound)} name this scale by its old name, \"{oldName}\", ") +
               $"so since it became \"{newName}\" their scale fields have no ladder: the form offers a field's own " +
               "options, or asks for a bare number, instead of the ladder's rungs, and credit compares bare numbers. " +
               "Renaming the scale back restores them. Picking the scale again in the " +
               "activity-type builder and publishing restores it for new activities only, because activities already " +
               "filed stay on their version.";
    }

    /// <summary>
    /// The published versions with a field whose <c>scale_key</c> <paramref name="binds" /> accepts, grouped by type
    /// and ordered by the type's name.
    /// </summary>
    private static async Task<IReadOnlyList<BoundType>> FindPublishedBindingsAsync(
        IApplicationDbContext dbContext,
        Func<ScaleBinding, bool> binds,
        CancellationToken cancellationToken)
    {
        var schemas = await dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Select(version => new
            {
                version.ActivityTypeId,
                version.Version,
                version.SchemaJson,
                version.ActivityType.Name,
                version.ActivityType.Key
            })
            .ToListAsync(cancellationToken);

        var bound = new List<(int ActivityTypeId, string Name, string Key, int Version)>();
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

            var bindsIt = schema.Sections
                .SelectMany(section => section.Fields)
                .Select(field => ScaleBinding.Parse(field.ScaleKey))
                .Any(binding => binding is not null && binds(binding));

            if (bindsIt)
            {
                bound.Add((candidate.ActivityTypeId, candidate.Name, candidate.Key, candidate.Version));
            }
        }

        return bound
            .GroupBy(row => row.ActivityTypeId)
            .Select(group => new BoundType(
                group.First().Name,
                group.First().Key,
                group.Select(row => row.Version).Distinct().Order().ToList()))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ThenBy(type => type.Key, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Each type as <c>"Mini-CEX" (mini_cex, versions 1 and 2)</c>, joined into one list. The key is named because two
    /// types may share a name in different scopes, and the versions because a binding in an old version is as lasting as
    /// one in the current version.
    /// </summary>
    private static string NameEach(IReadOnlyList<BoundType> types)
        => JoinAnd(types
            .Select(type =>
                $"\"{type.Name}\" ({type.Key}, {(type.Versions.Count == 1 ? "version" : "versions")} " +
                $"{JoinAnd(type.Versions.Select(version => version.ToString(CultureInfo.InvariantCulture)).ToList())})")
            .ToList());

    private static string JoinAnd(IReadOnlyList<string> items)
        => items.Count == 1 ? items[0] : $"{string.Join(", ", items.Take(items.Count - 1))} and {items[^1]}";

    private sealed record BoundType(string Name, string Key, IReadOnlyList<int> Versions);

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

    /// <summary>
    /// Throws when this scale is a sub-speciality's default entrustment scale, naming every such sub-speciality (T232).
    /// </summary>
    /// <remarks>
    /// <c>SubSpecialities.DefaultEntrustmentScaleId</c> is ON DELETE RESTRICT, so a delete that reaches the database is
    /// refused there. But that refusal is a raw <c>DbUpdateException</c>, which tells the administrator neither why nor
    /// what to change. Asked here, before the handler removes anything, the refusal names the sub-speciality whose
    /// default has to change first. Each is named with its speciality, because a sub-speciality's name is unique only
    /// within its speciality.
    /// </remarks>
    public static async Task ThrowIfDefaultOfASubSpecialityAsync(
        IApplicationDbContext dbContext,
        int scaleId,
        CancellationToken cancellationToken)
    {
        var defaults = await dbContext.Set<SubSpeciality>()
            .AsNoTracking()
            .Where(subSpeciality => subSpeciality.DefaultEntrustmentScaleId == scaleId)
            .OrderBy(subSpeciality => subSpeciality.Speciality.Name)
            .ThenBy(subSpeciality => subSpeciality.Name)
            .Select(subSpeciality => new { subSpeciality.Name, Speciality = subSpeciality.Speciality.Name })
            .ToListAsync(cancellationToken);

        if (defaults.Count == 0)
        {
            return;
        }

        var named = defaults.Select(subSpeciality => $"\"{subSpeciality.Name}\" ({subSpeciality.Speciality})").ToList();
        var list = named.Count == 1 ? named[0] : $"{string.Join(", ", named.Take(named.Count - 1))} and {named[^1]}";

        throw new InvalidOperationException(named.Count == 1
            ? $"This entrustment scale is the default scale of the sub-speciality {list}, so it cannot be deleted. " +
              "Change that sub-speciality's default entrustment scale to another scale, or to no default, first."
            : $"This entrustment scale is the default scale of the sub-specialities {list}, so it cannot be deleted. " +
              "Change each one's default entrustment scale to another scale, or to no default, first.");
    }
}
