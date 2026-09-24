using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Domain.Activities;

/// <summary>
/// The rule that keeps an activity's EPA and the EPA its credit lands on the same field (T137).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Activity.EpaId" /> is stamped from the field the pinned schema's <c>evidence_epa_field</c> names. Credit
/// finds its curriculum item through a directive's <c>curriculum_item_match</c>. Two properties naming one fact is how
/// the list and the progress page end up describing different EPAs, so a type is publishable only when the two cannot
/// disagree. Two shapes pass, and nothing else does:
/// <list type="bullet">
///   <item>
///     <b>A type about one EPA.</b> The schema declares the pointer, and every directive credits through
///     <c>epa_field</c> naming the pointer's field. No directive may target an item (a literal
///     <c>curriculum_item_id</c> or a <c>curriculum_item_field</c>): the credit engine resolves an item target FIRST
///     (<c>CreditTargetResolver.DescribeTarget</c>), and that item's EPA is a database fact nobody picked, so a row
///     stamped with the EPA the trainee chose could have credited another.
///   </item>
///   <item>
///     <b>A type about no single EPA.</b> No pointer, and no directive reads an <c>epa_field</c>. It may credit a fixed
///     item or an item field (journal club, a procedure log), because its rows stamp no EPA and so cannot contradict
///     what they credit.
///   </item>
/// </list>
/// A directive naming an item target AND an <c>epa_field</c> is refused under both: whichever of the two the engine
/// used, the other would be a second, unenforced statement of what was credited.
/// </para>
/// <para>
/// Crediting is therefore single-EPA-field by design. A type cannot credit two EPA fields with two directives, and
/// cannot credit from an EPA held in a field of another type (a <c>number</c>): the pointer must name an <c>epa</c>
/// field (<see cref="FormSchemaParser" />), and credit must read the pointer. Both refusals say so directly rather than
/// suggesting a pointer the parser would then refuse.
/// </para>
/// <para>
/// Refused at save and at publish, beside <see cref="ActorFieldRules.EnsurePublishable" />, rather than defaulted:
/// making a directive with no <c>epa_field</c> fall back to the pointer was the alternative, and it was rejected. It
/// would make the credit rules mean something they do not say, so every reader of <c>epa_field</c> (the credit engine,
/// the EPA→tool gate, the rebuild, the EPA picker's narrowing) would have to learn the default, and the one that did not
/// would disagree. It would also change what an existing <c>curriculum_item_field</c> directive falls through to. And a
/// default alone does not stop a directive that names a different field, so the refusal would be needed anyway.
/// </para>
/// <para>
/// Not in either parser, because it spans two DSLs, and so that a stored version predating the rule still loads. The
/// readers that cope with the shapes it now refuses (the tool gate, <c>CreditRuleFields</c>) keep doing so for those
/// pinned versions.
/// </para>
/// </remarks>
public static class EvidenceEpa
{
    /// <summary>
    /// Throws <see cref="CreditRulesParseException" /> unless every credit directive agrees with the schema's
    /// <c>evidence_epa_field</c>: through <c>epa_field</c> naming the pointer when there is one, or by targeting an item
    /// or nothing when there is none. See the type's remarks.
    /// </summary>
    public static void EnsureCreditAgrees(FormSchema schema, CreditRules creditRules)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(creditRules);

        var pointer = schema.EvidenceEpaField;
        var problems = new List<string>();

        for (var index = 0; index < creditRules.CountsFor.Count; index++)
        {
            var rule = index + 1;
            var match = creditRules.CountsFor[index].CurriculumItemMatchRule;
            var epaField = string.IsNullOrWhiteSpace(match.EpaField) ? null : match.EpaField;
            var itemTarget = DescribeItemTarget(match);

            if (itemTarget is not null && epaField is not null)
            {
                problems.Add(
                    $"Credit rule {rule} names both {itemTarget} and epa_field '{epaField}'. Credit lands on the item " +
                    "whenever it resolves, so the EPA field would not say what was credited. " +
                    (pointer is null ? "Name one of them." : $"Credit through epa_field '{pointer}' alone."));
                continue;
            }

            if (itemTarget is not null)
            {
                if (pointer is not null)
                {
                    problems.Add(
                        $"Credit rule {rule} credits {itemTarget}, but the form's activities are filed against the EPA " +
                        $"in '{pointer}'. That item's EPA need not be the one picked, so a row could show one EPA and " +
                        $"credit another. Credit through epa_field '{pointer}' instead, or remove evidence_epa_field " +
                        "if the form is about no single EPA.");
                }

                continue;
            }

            if (epaField is null)
            {
                continue;
            }

            if (pointer is not null)
            {
                if (!string.Equals(epaField, pointer, StringComparison.Ordinal))
                {
                    problems.Add(
                        $"Credit rule {rule} reads its EPA from field '{epaField}', but the form's EPA field is " +
                        $"'{pointer}'. The EPA an activity is filed against and the EPA it credits must be the same field.");
                }

                continue;
            }

            var field = schema.Sections
                .SelectMany(section => section.Fields)
                .FirstOrDefault(candidate => string.Equals(candidate.Key, epaField, StringComparison.Ordinal));

            if (field is null)
            {
                problems.Add(
                    $"Credit rule {rule} reads its EPA from field '{epaField}', which is not a field of the form.");
            }
            else if (field.Type != FieldType.Epa)
            {
                problems.Add(
                    $"Credit rule {rule} reads its EPA from field '{epaField}', which is a '{field.Type}' field. Credit " +
                    "by EPA must read the form's EPA field, which must be an 'epa' field: change the field's type to " +
                    "EPA, or credit from an EPA field.");
            }
            else
            {
                problems.Add(
                    $"Credit rule {rule} reads its EPA from field '{epaField}', but the form does not say which " +
                    $"field carries the activity's EPA. Set evidence_epa_field to '{epaField}' (in the builder: " +
                    "Form settings, EPA field).");
            }
        }

        if (problems.Count > 0)
        {
            throw new CreditRulesParseException(string.Join(" ", problems.Distinct(StringComparer.Ordinal)));
        }
    }

    private static string? DescribeItemTarget(CurriculumItemMatchRule match)
    {
        if (match.CurriculumItemId.HasValue)
        {
            return $"curriculum item {match.CurriculumItemId.Value}";
        }

        return string.IsNullOrWhiteSpace(match.CurriculumItemField)
            ? null
            : $"the curriculum item in field '{match.CurriculumItemField}'";
    }
}
