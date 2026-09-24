using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Reads an activity type's credit rules to answer one question the UI needs: which schema fields does
/// the credit engine actually read an EPA out of? (T108)
/// </summary>
/// <remarks>
/// This lives in Application rather than in the Blazor component that needs it because
/// <c>CLAUDE.md</c> forbids a <c>.razor</c> file from touching Domain types — the credit DSL and its
/// parser are Domain. It returns plain strings, so the component stays on the right side of that line.
/// </remarks>
public static class CreditRuleFields
{
    /// <summary>
    /// The set of field keys whose value the credit engine will look an EPA up by.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Follows the precedence in <c>CreditTargetResolver.ResolveCurriculumItemsAsync</c>, which tests
    /// <c>curriculum_item_id</c> first, then <c>curriculum_item_field</c>, and otherwise reads
    /// <c>epa_field</c>. Nothing in <c>CreditRulesParser</c> makes the three mutually exclusive. A
    /// directive naming <c>curriculum_item_field</c> is skipped here, although the engine FALLS THROUGH to
    /// its <c>epa_field</c> when the item field holds no parseable id. That case is left un-narrowed on
    /// purpose: narrowing on a field the engine reads only as a fallback would hide choices that usually
    /// cannot affect credit. The write path's tool gate (T122) resolves through the engine's own resolver
    /// and is authoritative there, so a builder-made rule mixing both keys can be refused at submit for an
    /// EPA this picker offered.
    /// </para>
    /// <para>
    /// An unparseable or absent rule set yields an empty set — no narrowing. Surfacing a malformed
    /// credit block is the builder's job, not the runtime form's, and failing open here keeps a
    /// required field submittable.
    /// </para>
    /// </remarks>
    public static IReadOnlySet<string> ResolveCreditedEpaFieldKeys(string? creditRulesJson)
    {
        var creditRules = TryParse(creditRulesJson);
        return creditRules is null ? new HashSet<string>(StringComparer.Ordinal) : CreditedEpaFieldKeys(creditRules);
    }

    /// <summary>
    /// The set of field keys the EPA picker narrows to what the write path accepts: the fields credit reads an EPA out of
    /// (<see cref="ResolveCreditedEpaFieldKeys" />), or, for a College instrument whose rules credit nothing, the field its
    /// schema names as the EPA the activity is evidence for (T154).
    /// </summary>
    /// <param name="creditRulesJson">The pinned version's credit rules.</param>
    /// <param name="evidenceEpaField">The pinned schema's <c>evidence_epa_field</c>.</param>
    /// <param name="wbaToolKey">The type's instrument. Null or blank is no instrument (D21).</param>
    /// <remarks>
    /// <para>
    /// The same targets the EPA→tool gate judges (<c>ToolPermissionGate.GatedTargets</c>). An unrated instrument (the
    /// reflective exercise, the clinical audit, the portfolio review) credits nothing but is stamped as evidence for its
    /// EPA (T137), and the College's list says which instruments are evidence for an EPA, so the write path refuses an EPA
    /// whose list does not name the instrument. The picker offers the same set, rather than every EPA and a refusal at
    /// submit.
    /// </para>
    /// <para>
    /// Only for an instrument. A type that is none (a reflective note, a teaching session) is not held to any list
    /// (D21), and narrowing its EPA to the subject's curriculum would hide EPAs whose choice changes nothing: the T108
    /// reason its field was never narrowed. Only when nothing is credited, so a crediting type's answer is exactly the
    /// credited fields, as before.
    /// </para>
    /// </remarks>
    public static IReadOnlySet<string> ResolveNarrowedEpaFieldKeys(
        string? creditRulesJson,
        string? evidenceEpaField,
        string? wbaToolKey)
    {
        var creditRules = TryParse(creditRulesJson);
        if (creditRules is null)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        if (creditRules.CountsFor.Count > 0 ||
            WbaTool.NormalizeKey(wbaToolKey) is null ||
            string.IsNullOrWhiteSpace(evidenceEpaField))
        {
            return CreditedEpaFieldKeys(creditRules);
        }

        return new HashSet<string>(StringComparer.Ordinal) { evidenceEpaField };
    }

    private static CreditRules? TryParse(string? creditRulesJson)
    {
        if (string.IsNullOrWhiteSpace(creditRulesJson))
        {
            return null;
        }

        try
        {
            return CreditRulesParser.Parse(creditRulesJson);
        }
        catch (Exception)
        {
            // Deliberately broad. CreditRulesParser raises CreditRulesParseException for the shapes it
            // checks, but a JSON value of the wrong primitive type surfaces as InvalidOperationException
            // out of System.Text.Json. Neither is this method's error to report.
            return null;
        }
    }

    private static HashSet<string> CreditedEpaFieldKeys(CreditRules creditRules)
    {
        var creditedFieldKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var directive in creditRules.CountsFor)
        {
            var matchRule = directive.CurriculumItemMatchRule;
            if (matchRule.CurriculumItemId.HasValue ||
                !string.IsNullOrWhiteSpace(matchRule.CurriculumItemField))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(matchRule.EpaField))
            {
                creditedFieldKeys.Add(matchRule.EpaField);
            }
        }

        return creditedFieldKeys;
    }
}
