using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// Turns a stored entrustment ordinal into the rung label a clinician reads (T100).
/// </summary>
/// <remarks>
/// <para>
/// <c>Order</c> is the rank: it is what gets written to <c>DataJson</c>, what <c>CreditApplier</c>
/// compares, and what every minimum is stored as. <c>Label</c> is the rung as the College prints it.
/// On the ladders Wombat shipped before v11.1 the two were unrelated by kind — ordinal 4 against the
/// label "Independent" — so printing the ordinal read as a numbering of prose and nobody minded. On the
/// CPSA v11.1 ladder they are both numbers and they disagree: ordinal 5 is the College's rung "4". A
/// page that prints "Minimum level 3" against a ladder whose rungs are 1, 2, 3a, 3b, 4, 5 is naming a
/// rung that does not exist.
/// </para>
/// <para>
/// This is deliberately a lookup built from ids the caller already has, not a service: the callers are
/// list handlers that would otherwise issue one query per row. Load once for the scales in play, then
/// resolve in memory.
/// </para>
/// <para>
/// Resolution is permissive by design. An unpinned curriculum item (<c>CurriculumItem.ScaleId</c> null,
/// which T109 documents as a permanent and meaningful state) or an ordinal that is not a rung on its
/// scale falls back to printing the ordinal — exactly what the product did before this type existed.
/// A resolver that hid the number when it could not name it would replace a confusing label with no
/// label at all.
/// </para>
/// </remarks>
public static class EntrustmentRungLabels
{
    /// <summary>
    /// Loads the rung labels for the given scales. Ids that are null, duplicated or unknown are ignored.
    /// </summary>
    public static async Task<EntrustmentRungLookup> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<int?> scaleIds,
        CancellationToken cancellationToken = default)
    {
        var ids = scaleIds
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return EntrustmentRungLookup.Empty;
        }

        var rungs = await dbContext.Set<EntrustmentLevel>()
            .AsNoTracking()
            .Where(level => ids.Contains(level.ScaleId))
            .Select(level => new { level.ScaleId, level.Order, level.Label })
            .ToListAsync(cancellationToken);

        var byScaleAndOrder = new Dictionary<(int ScaleId, int Order), string>(rungs.Count);
        foreach (var rung in rungs)
        {
            // A scale with two rungs at the same Order is malformed; first wins rather than throwing,
            // because this is a display path and a broken ladder should not take a progress page down.
            byScaleAndOrder.TryAdd((rung.ScaleId, rung.Order), rung.Label);
        }

        return new EntrustmentRungLookup(byScaleAndOrder);
    }

    /// <summary>
    /// Loads rung labels for the scales named by a set of schema <c>scale_key</c> values, each of which
    /// holds either a scale id or an exact scale name.
    /// </summary>
    /// <remarks>
    /// The id-or-exact-name rule is the one <c>CreditApplier.ResolveScaleIdAsync</c> applies when it
    /// decides whether an achieved rating and a curriculum minimum are on the same ladder (T109). Display
    /// sites must agree with it, or a chart will label a rung the credit engine refused to compare.
    /// <c>CreditApplier</c> keeps its own copy because it resolves a set of achieved scales inside a
    /// larger query; this is the copy every read-side caller should use.
    /// </remarks>
    public static async Task<EntrustmentRungLookup> LoadForScaleKeysAsync(
        IApplicationDbContext dbContext,
        IEnumerable<string?> scaleKeys,
        CancellationToken cancellationToken = default)
    {
        var keys = scaleKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key!.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (keys.Length == 0)
        {
            return EntrustmentRungLookup.Empty;
        }

        var numericKeys = new List<int>();
        foreach (var key in keys)
        {
            if (int.TryParse(key, out var id))
            {
                numericKeys.Add(id);
            }
        }

        var scales = await dbContext.Set<EntrustmentScale>()
            .AsNoTracking()
            .Where(scale => numericKeys.Contains(scale.Id) || keys.Contains(scale.Name))
            .Select(scale => new { scale.Id, scale.Name })
            .ToListAsync(cancellationToken);

        if (scales.Count == 0)
        {
            return EntrustmentRungLookup.Empty;
        }

        var scaleIdByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var key in keys)
        {
            // Id first, then exact name — the order CreditApplier uses.
            var match = (int.TryParse(key, out var id) ? scales.FirstOrDefault(s => s.Id == id) : null)
                        ?? scales.FirstOrDefault(s => string.Equals(s.Name, key, StringComparison.Ordinal));
            if (match is not null)
            {
                scaleIdByKey[key] = match.Id;
            }
        }

        var lookup = await LoadAsync(
            dbContext, scaleIdByKey.Values.Select(id => (int?)id), cancellationToken);

        return lookup.WithScaleKeys(scaleIdByKey);
    }
}

/// <summary>
/// An in-memory (scale, ordinal) to rung-label lookup. See <see cref="EntrustmentRungLabels" />.
/// </summary>
public sealed class EntrustmentRungLookup
{
    public static readonly EntrustmentRungLookup Empty =
        new(new Dictionary<(int ScaleId, int Order), string>());

    private readonly IReadOnlyDictionary<(int ScaleId, int Order), string> _byScaleAndOrder;
    private readonly IReadOnlyDictionary<string, int> _scaleIdByKey;

    internal EntrustmentRungLookup(
        IReadOnlyDictionary<(int ScaleId, int Order), string> byScaleAndOrder,
        IReadOnlyDictionary<string, int>? scaleIdByKey = null)
    {
        _byScaleAndOrder = byScaleAndOrder;
        _scaleIdByKey = scaleIdByKey ?? new Dictionary<string, int>(StringComparer.Ordinal);
    }

    internal EntrustmentRungLookup WithScaleKeys(IReadOnlyDictionary<string, int> scaleIdByKey) =>
        new(_byScaleAndOrder, scaleIdByKey);

    /// <summary>
    /// The scale a schema <c>scale_key</c> resolves to, or null when it names nothing. A key that
    /// resolves to nothing is a real and common state — the four generic WBA seeds declare
    /// <c>or_scale</c> and the only seeded scale is named "O-R Scale" (T110).
    /// </summary>
    public int? ResolveScaleKey(string? scaleKey) =>
        !string.IsNullOrWhiteSpace(scaleKey) && _scaleIdByKey.TryGetValue(scaleKey.Trim(), out var id)
            ? id
            : null;

    /// <summary>
    /// The rung label for an ordinal on the ladder a schema <c>scale_key</c> names, falling back to the
    /// ordinal as text.
    /// </summary>
    public string FormatByScaleKey(string? scaleKey, int order) =>
        Format(ResolveScaleKey(scaleKey), order);

    /// <summary>
    /// The ordered rungs of a scale, empty when it resolves to nothing.
    /// </summary>
    public IReadOnlyList<(int Order, string Label)> RungsOf(int? scaleId) =>
        scaleId is null
            ? []
            : _byScaleAndOrder
                .Where(pair => pair.Key.ScaleId == scaleId.Value)
                .OrderBy(pair => pair.Key.Order)
                .Select(pair => (pair.Key.Order, pair.Value))
                .ToList();

    /// <summary>
    /// The rung label, or null when the scale is unpinned or the ordinal is not a rung on it.
    /// </summary>
    public string? Find(int? scaleId, int order) =>
        scaleId.HasValue && _byScaleAndOrder.TryGetValue((scaleId.Value, order), out var label)
            ? label
            : null;

    /// <summary>
    /// The rung label, falling back to the ordinal as text. This is what display sites want: it is
    /// never empty, and where the ladder is unknown it degrades to the pre-T100 rendering.
    /// </summary>
    public string Format(int? scaleId, int order) =>
        Find(scaleId, order) ?? order.ToString();
}
