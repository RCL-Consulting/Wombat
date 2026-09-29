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
            .Select(level => new { level.ScaleId, level.Order, level.Label, level.Description })
            .ToListAsync(cancellationToken);

        var byScaleAndOrder = new Dictionary<(int ScaleId, int Order), EntrustmentRung>(rungs.Count);
        foreach (var rung in rungs)
        {
            // A scale with two rungs at the same Order is malformed; first wins rather than throwing,
            // because this is a display path and a broken ladder should not take a progress page down.
            byScaleAndOrder.TryAdd(
                (rung.ScaleId, rung.Order),
                new EntrustmentRung(
                    rung.Order,
                    rung.Label,
                    string.IsNullOrWhiteSpace(rung.Description) ? null : rung.Description.Trim()));
        }

        return new EntrustmentRungLookup(byScaleAndOrder);
    }

    /// <summary>
    /// Loads rung labels for the scales bound by a set of schema <c>scale_key</c> values, each of which binds a
    /// scale by seed key, id or exact name (<see cref="ScaleBinding" />).
    /// </summary>
    /// <remarks>
    /// The keys are resolved by <see cref="EntrustmentScaleBindings.ResolveAsync" />, the same resolver
    /// <c>CreditApplier</c> uses when it decides whether an achieved rating and a curriculum minimum are on the same
    /// ladder (T109, T253). Display sites must agree with it, or a chart will label a rung the credit engine refused to
    /// compare, and sharing the resolver is what makes them agree.
    /// </remarks>
    public static async Task<EntrustmentRungLookup> LoadForScaleKeysAsync(
        IApplicationDbContext dbContext,
        IEnumerable<string?> scaleKeys,
        CancellationToken cancellationToken = default)
    {
        var scaleIdByKey = await EntrustmentScaleBindings.ResolveAsync(dbContext, scaleKeys, cancellationToken);
        if (scaleIdByKey.Count == 0)
        {
            return EntrustmentRungLookup.Empty;
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
        new(new Dictionary<(int ScaleId, int Order), EntrustmentRung>());

    private readonly IReadOnlyDictionary<(int ScaleId, int Order), EntrustmentRung> _byScaleAndOrder;
    private readonly IReadOnlyDictionary<string, int> _scaleIdByKey;

    internal EntrustmentRungLookup(
        IReadOnlyDictionary<(int ScaleId, int Order), EntrustmentRung> byScaleAndOrder,
        IReadOnlyDictionary<string, int>? scaleIdByKey = null)
    {
        _byScaleAndOrder = byScaleAndOrder;
        _scaleIdByKey = scaleIdByKey ?? new Dictionary<string, int>(StringComparer.Ordinal);
    }

    internal EntrustmentRungLookup WithScaleKeys(IReadOnlyDictionary<string, int> scaleIdByKey) =>
        new(_byScaleAndOrder, scaleIdByKey);

    /// <summary>
    /// The scale a schema <c>scale_key</c> resolves to, or null when it binds nothing. A key that
    /// resolves to nothing is a real state, not an error: a scale deleted before its delete asked about
    /// schemas (T253), or a hand-written key, and every caller then falls back to the bare ordinal.
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
                .Select(pair => (pair.Key.Order, pair.Value.Label))
                .ToList();

    /// <summary>
    /// The ordered rungs of a scale with each rung's descriptor (T342, B12): what the rung row beside a rating prints
    /// ("4 · Supervision at a distance …"). Empty when the scale resolves to nothing.
    /// </summary>
    public IReadOnlyList<EntrustmentRung> RungDetailsOf(int? scaleId) =>
        scaleId is null
            ? []
            : _byScaleAndOrder
                .Where(pair => pair.Key.ScaleId == scaleId.Value)
                .OrderBy(pair => pair.Key.Order)
                .Select(pair => pair.Value)
                .ToList();

    /// <summary>
    /// The rung's descriptor as the College words it, or null when the scale is unpinned, the ordinal is not a rung on
    /// it, or the rung has none (T342, B12).
    /// </summary>
    public string? DescriptionOf(int? scaleId, int order) =>
        scaleId.HasValue && _byScaleAndOrder.TryGetValue((scaleId.Value, order), out var rung)
            ? rung.Description
            : null;

    /// <summary>
    /// The rung label, or null when the scale is unpinned or the ordinal is not a rung on it.
    /// </summary>
    public string? Find(int? scaleId, int order) =>
        scaleId.HasValue && _byScaleAndOrder.TryGetValue((scaleId.Value, order), out var rung)
            ? rung.Label
            : null;

    /// <summary>
    /// The rung label, falling back to the ordinal as text. This is what display sites want: it is
    /// never empty, and where the ladder is unknown it degrades to the pre-T100 rendering.
    /// </summary>
    public string Format(int? scaleId, int order) =>
        Find(scaleId, order) ?? order.ToString();
}

/// <summary>
/// One rung of an entrustment ladder (T342, B12).
/// </summary>
/// <param name="Order">
/// The rank: what an activity's data stores and credit compares. Not the rung's name: on the CPSA v11.1 ladder order 5 is
/// the rung "4" (T100), so a page saying "Rated 4" of a stored 5 prints <paramref name="Label" />.
/// </param>
/// <param name="Label">The rung as the College prints it: "1", "2", "3a", "3b", "4", "5".</param>
/// <param name="Description">The rung's descriptor as the College words it, or null when it has none.</param>
public sealed record EntrustmentRung(int Order, string Label, string? Description);
