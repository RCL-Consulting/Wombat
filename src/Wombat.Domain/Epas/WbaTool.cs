using System.Globalization;

namespace Wombat.Domain.Epas;

/// <summary>
/// A workplace-based assessment instrument the College names: Mini-CEX, DOPS, CBD, MSF and so on (T122).
/// </summary>
/// <remarks>
/// <para>
/// The vocabulary that <see cref="Curricula.CurriculumItem.PermittedToolsJson" /> and
/// <see cref="Activities.ActivityType.WbaToolKey" /> both speak. A curriculum item says which instruments may
/// credit its EPA; an activity type says which instrument it is. Both store the <see cref="Key" />, never the
/// id: the catalogue seed file is national data that boots every deployment, and an identity sequence is not.
/// </para>
/// <para>
/// Seed-owned. <c>PaediatricCatalogueSeeder</c> inserts and reconciles these rows from the catalogue file on
/// every boot, which is safe only because nothing else writes them: there is no admin command for this table.
/// The day one exists, the reconcile must become warn-only, as the seeder's other passes are.
/// </para>
/// </remarks>
public sealed class WbaTool
{
    public int Id { get; set; }

    /// <summary>
    /// Stable, lowercase, and spelled like the instrument's activity-type family where one exists (<c>mini_cex</c>,
    /// <c>dops</c>, <c>cbd</c>, <c>direct_observation</c>, <c>cca</c>). <c>RatedActivityTypes</c> classifies a
    /// type's evidence by this key (T144), and matches an unkeyed type's own key against the same spellings.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>The name the College's Annexure A uses, which is what a refusal message names.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The one-sentence definition on page 8 of EPA v11.1, or null for the three it does not define.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// The one normaliser for a tool key: trimmed and lower-cased, with a blank value meaning no key at all.
    /// </summary>
    /// <remarks>
    /// Every reader and every writer goes through this, so an allow-list written by the seeder, the admin editor
    /// or a migration compares equal to the key an activity type carries whatever casing or padding reached it.
    /// </remarks>
    public static string? NormalizeKey(string? key)
        => string.IsNullOrWhiteSpace(key) ? null : key.Trim().ToLower(CultureInfo.InvariantCulture);
}
