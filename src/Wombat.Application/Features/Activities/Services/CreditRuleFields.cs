using Wombat.Domain.Activities.Credit;

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
        var creditedFieldKeys = new HashSet<string>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(creditRulesJson))
        {
            return creditedFieldKeys;
        }

        CreditRules creditRules;
        try
        {
            creditRules = CreditRulesParser.Parse(creditRulesJson);
        }
        catch (Exception)
        {
            // Deliberately broad. CreditRulesParser raises CreditRulesParseException for the shapes it
            // checks, but a JSON value of the wrong primitive type surfaces as InvalidOperationException
            // out of System.Text.Json. Neither is this method's error to report.
            return creditedFieldKeys;
        }

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
