using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>What a curriculum item's tool list says about one instrument (T122).</summary>
public enum ToolPermissionVerdict
{
    /// <summary>The item has no list, or the activity type declares no instrument (D21). Behaves as before T122.</summary>
    NotRestricted = 0,

    /// <summary>The item's list names this instrument.</summary>
    Permitted = 1,

    /// <summary>The item has a list and this recognised instrument is not on it.</summary>
    NotPermitted = 2
}

/// <summary>
/// The one predicate behind the EPA→tool allow-list (T122). The EPA picker narrows with it, and the write path
/// refuses with it (D20), so the two cannot disagree about what an instrument may credit.
/// </summary>
/// <remarks>
/// <para>
/// Pure, and in Application beside <see cref="CreditRuleFields" /> for the same reason: the picker's service and the
/// write-path gate both live in Infrastructure, and a mirror-the-engine rule belongs one layer from the engine, not in
/// a component. <c>CreditApplier</c> deliberately never calls it. Credit does not re-check the list (D20), because an
/// allow-list can be edited after an encounter was filed, and refusing credit then would delete evidence collected
/// under the rule in force at the time.
/// </para>
/// <para>
/// <see cref="ToolPermissionVerdict.NotRestricted" /> is the load-bearing default, the same discipline T108 and T109
/// landed on. A restriction that fires where the answer is unknown produces an empty picker or a refusal nobody can
/// act on, and T123 d3 narrows the same picker from the other direction, so between them they must never empty it.
/// </para>
/// </remarks>
public static class ToolPermission
{
    public static ToolPermissionVerdict Evaluate(IReadOnlyCollection<string>? permittedToolKeys, string? wbaToolKey)
    {
        var toolKey = WbaTool.NormalizeKey(wbaToolKey);
        if (toolKey is null || permittedToolKeys is null)
        {
            return ToolPermissionVerdict.NotRestricted;
        }

        // Emptiness is judged AFTER normalising, as CurriculumItem.ParsePermittedTools judges it. A list of blanks is no
        // list at all, and reading it as a list that names nothing would refuse every instrument.
        var permitted = permittedToolKeys.Select(WbaTool.NormalizeKey).OfType<string>().ToList();
        if (permitted.Count == 0)
        {
            return ToolPermissionVerdict.NotRestricted;
        }

        return permitted.Contains(toolKey, StringComparer.Ordinal)
            ? ToolPermissionVerdict.Permitted
            : ToolPermissionVerdict.NotPermitted;
    }
}
