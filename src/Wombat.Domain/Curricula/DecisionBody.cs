using System.Globalization;

namespace Wombat.Domain.Curricula;

/// <summary>
/// A committee the College names as the one that takes an EPA's entrustment decision, where that is not the trainee's
/// general panel (T131, Decision 2). One so far: Annexure B gives EPAs 4 and 5 to the neonatal team Clinical Competency
/// Committee.
/// </summary>
/// <remarks>
/// <para>
/// A national vocabulary, like <see cref="Epas.WbaTool" />. A curriculum item names the body that decides its EPA
/// (<see cref="CurriculumItem.DecisionBodyKey" />); a decision panel says which body it sits as
/// (<see cref="CommitteeDecisions.DecisionPanel.DecisionBodyKey" />, T131 slice 3), so that committee structure stays a
/// per-institution arrangement while the tag on the EPA is national. If the College
/// answers that the structure is national (§ 3F question 6), only the panel-to-body mapping moves.
/// </para>
/// <para>
/// Keyed by <see cref="Key" />, which is the primary key and what every reference stores, because the catalogue seed file
/// is national data that boots every deployment and an identity sequence is not. Items point at it with a restricting
/// foreign key, and so do panels, so a body in use cannot be deleted from under them.
/// </para>
/// <para>
/// Seed-owned. <c>PaediatricCatalogueSeeder</c> inserts these rows from the catalogue's <c>decisionBodyVocabulary</c> and
/// keeps each name equal to it, which is safe only because nothing else writes this table: there is no admin command for
/// it. The day one exists, that reconcile must become warn-only.
/// </para>
/// </remarks>
public sealed class DecisionBody
{
    /// <summary>The longest key the column holds.</summary>
    public const int KeyMaxLength = 32;

    /// <summary>Stable and lowercase, such as <c>neonatal</c>. See <see cref="NormalizeKey" />.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>The College's name for the committee, which is what the curriculum editor and a routing notice print.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The one normaliser for a decision-body key: trimmed and lower-cased, with a blank value meaning no body at all.
    /// Every writer goes through it, so a key reaching the store from the editor, the seeder or a migration compares equal.
    /// </summary>
    public static string? NormalizeKey(string? key)
        => string.IsNullOrWhiteSpace(key) ? null : key.Trim().ToLower(CultureInfo.InvariantCulture);
}
