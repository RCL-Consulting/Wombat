using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// Resolves schema <c>scale_key</c>s to the scales they bind: the one resolver every reader of a binding goes through
/// (T253).
/// </summary>
/// <remarks>
/// <para>
/// A key is read by <see cref="ScaleBinding.Parse" />: <c>seed:</c> and a seed key, an id as digits, or an exact name.
/// <c>CreditApplier</c> asks which ladder an achieved rating sits on; <see cref="EntrustmentRungLabels.LoadForScaleKeysAsync" />
/// asks the same for the rung picker, the portfolio, the trajectory, the committee pages and the type picker; the scale's
/// delete and rename ask whether a published schema binds it (<see cref="EntrustmentScaleReferences" />). Before T253
/// the first two each kept a copy of an id-or-name rule and the third compared names only, so a seed bound by name broke
/// on a rename and a type the builder bound by id did not stop a delete.
/// </para>
/// <para>
/// A key that binds nothing is left out of the result, and every caller reads a missing key as "no ladder": the credit
/// engine compares bare ordinals and the form offers the field's own options, or asks for a bare number. That is a real
/// state for a version already published (a scale deleted before T253, a hand-written key), not an error, so a read never
/// throws. Publishing one is refused instead (<see cref="ThrowIfAFieldBindsNoScaleAsync" />).
/// </para>
/// </remarks>
public static class EntrustmentScaleBindings
{
    /// <summary>
    /// Why a scale may not be called something <see cref="ScaleBinding.Parse" /> reads as an id or a seed key: a form
    /// could never bind it by that name, and the three forms would stop being told apart by the string alone.
    /// </summary>
    public const string ReservedNameRefusal =
        "A scale's name cannot be digits alone or start with \"seed:\", because an activity type's form would read " +
        "that name as a scale's id or seed key.";

    /// <summary>
    /// The scale each key binds, by its trimmed key. Blank keys and keys that bind no scale are left out.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, int>> ResolveAsync(
        IApplicationDbContext dbContext,
        IEnumerable<string?> scaleKeys,
        CancellationToken cancellationToken = default)
    {
        var bindings = scaleKeys
            .Select(ScaleBinding.Parse)
            .OfType<ScaleBinding>()
            .DistinctBy(binding => binding.Key, StringComparer.Ordinal)
            .ToList();

        if (bindings.Count == 0)
        {
            return EmptyResolution;
        }

        var ids = bindings.Where(binding => binding.ScaleId.HasValue).Select(binding => binding.ScaleId!.Value).ToList();
        var seedKeys = bindings.Where(binding => binding.SeedKey is not null).Select(binding => binding.SeedKey!).ToList();
        var names = bindings.Where(binding => binding.Name is not null).Select(binding => binding.Name!).ToList();

        var scales = await dbContext.Set<EntrustmentScale>()
            .AsNoTracking()
            .Where(scale => ids.Contains(scale.Id)
                            || (scale.SeedKey != null && seedKeys.Contains(scale.SeedKey))
                            || names.Contains(scale.Name))
            .Select(scale => new { scale.Id, scale.SeedKey, scale.Name })
            .ToListAsync(cancellationToken);

        var resolved = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var binding in bindings)
        {
            // At most one scale can match: the three forms are disjoint (ScaleBinding), and ids, seed keys and names are
            // each unique. First wins all the same, so a database that broke that cannot break a read.
            var match = scales.FirstOrDefault(scale => binding.Binds(scale.Id, scale.SeedKey, scale.Name));
            if (match is not null)
            {
                resolved[binding.Key] = match.Id;
            }
        }

        return resolved;
    }

    /// <summary>
    /// Throws when a field of <paramref name="schema" /> has a <c>scale_key</c> that binds no scale, naming each such field
    /// (T253 review). The activity-type publish asks this of the draft before it publishes anything.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The scale's delete refuses while a published version binds the scale, but a draft is not published and may bind
    /// what it likes: an administrator picks a scale in the builder, saves the draft, the scale is deleted, and the draft
    /// is published. Without this the new version's field binds nothing from the moment it is published, which is the
    /// silent loss of a ladder T253 closes on the delete side, reached by a different order of steps. The two together
    /// keep "a published version binds only scales that exist" (short of a delete and a publish that race).
    /// </para>
    /// <para>
    /// A field with no <c>scale_key</c> is not asked about: it was left unbound, not bound to something that is gone.
    /// Seeds are published by the seeders, which do not come through here; <c>SeedScaleKeyTests</c> holds them to keys a
    /// seeder stamps.
    /// </para>
    /// </remarks>
    public static async Task ThrowIfAFieldBindsNoScaleAsync(
        IApplicationDbContext dbContext,
        FormSchema schema,
        CancellationToken cancellationToken = default)
    {
        var boundFields = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => !string.IsNullOrWhiteSpace(field.ScaleKey))
            .ToList();
        if (boundFields.Count == 0)
        {
            return;
        }

        var resolved = await ResolveAsync(dbContext, boundFields.Select(field => field.ScaleKey), cancellationToken);
        var unbound = boundFields
            .Where(field => !resolved.ContainsKey(field.ScaleKey!.Trim()))
            .Select(field => $"\"{field.Label}\" ({field.Key}, scale_key \"{field.ScaleKey!.Trim()}\")")
            .ToList();
        if (unbound.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(unbound.Count == 1
            ? $"The field {unbound[0]} is bound to an entrustment scale that does not exist, so this draft cannot be " +
              "published: the field would have no ladder to rate on. Pick its entrustment scale again, save the draft, " +
              "then publish."
            : $"The fields {string.Join(", ", unbound.Take(unbound.Count - 1))} and {unbound[^1]} are bound to entrustment " +
              "scales that do not exist, so this draft cannot be published: those fields would have no ladder to rate " +
              "on. Pick each one's entrustment scale again, save the draft, then publish.");
    }

    private static readonly IReadOnlyDictionary<string, int> EmptyResolution =
        new Dictionary<string, int>(StringComparer.Ordinal);
}
