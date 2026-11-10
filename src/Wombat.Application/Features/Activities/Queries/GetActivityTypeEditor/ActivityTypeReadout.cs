using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;

/// <summary>
/// What the builder shows of a type's three payloads beyond their JSON (T398, flow 17): the workflow and the credit rules
/// in words (Q2, S9, J12; R2-B04, B05, D02, D03, R3-D05), and the JSON laid out for its editor (Q13, S11, J14). Computed
/// here so a page never parses a DSL (<c>WorkflowReadout</c> and <c>CreditReadout</c> render it).
/// </summary>
internal static class ActivityTypeReadout
{
    /// <summary>
    /// The workflow in words: each state by label, where a new activity starts and which are final; each move by its label,
    /// the states it leaves and enters by label, who may make it (<see cref="ActorRuleWords" />, a <c>field:</c> rule by
    /// the field's label in <paramref name="schemaJson" />), what it checks, and whether it asks for a note. Null when the
    /// workflow does not parse.
    /// </summary>
    public static ActivityTypeWorkflowReadDto? Workflow(string workflowJson, string schemaJson)
    {
        var workflow = TryParse(() => WorkflowParser.Parse(workflowJson));
        if (workflow is null)
        {
            return null;
        }

        var labelOf = FieldLabels(schemaJson);
        var stateLabels = workflow.States
            .GroupBy(state => state.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Label, StringComparer.Ordinal);
        string StateLabel(string key) => stateLabels.TryGetValue(key, out var label) ? label : key;

        var states = workflow.States
            .Select(state => new ActivityTypeStateReadDto(
                state.Key,
                state.Label,
                string.Equals(state.Key, workflow.InitialState, StringComparison.Ordinal),
                state.Terminal))
            .ToList();

        var moves = workflow.Transitions
            .Select(move =>
            {
                var (who, worded) = ActorRuleWords.Describe(move.Actor, key => labelOf(key));
                return new ActivityTypeMoveReadDto(
                    move.Key,
                    WorkflowTransition.LabelFor(move.Key),
                    move.From.Select(StateLabel).ToList(),
                    StateLabel(move.To),
                    who,
                    ActorRuleWords.Validation(move.Validation),
                    move.RequiresNote)
                {
                    WhoAsWritten = worded ? null : who
                };
            })
            .ToList();

        return new ActivityTypeWorkflowReadDto(states, moves);
    }

    /// <summary>
    /// The credit rules in words: "Credits nothing." when there is no directive (the page words it), else one sentence a
    /// directive, its fields by label: "Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision
    /// required for this encounter records." (R2-B05). Null when the rules do not parse.
    /// </summary>
    public static ActivityTypeCreditReadDto? Credit(string creditRulesJson, string schemaJson)
    {
        var rules = TryParse(() => CreditRulesParser.Parse(creditRulesJson));
        if (rules is null)
        {
            return null;
        }

        var labelOf = FieldLabels(schemaJson);
        string Field(string key) => labelOf(key) ?? $"'{key}'";

        var sentences = rules.CountsFor
            .Select(directive =>
            {
                var match = directive.CurriculumItemMatchRule;

                // An item target first, as the credit engine resolves one first (CreditTargetResolver).
                var target = match.CurriculumItemId is int itemId
                    ? $"curriculum item {itemId}"
                    : !string.IsNullOrWhiteSpace(match.CurriculumItemField)
                        ? $"the curriculum item in {Field(match.CurriculumItemField)}"
                        : !string.IsNullOrWhiteSpace(match.EpaField)
                            ? $"the curriculum item for the EPA in {Field(match.EpaField)}"
                            : "no curriculum item";

                var level = !string.IsNullOrWhiteSpace(directive.MinimumLevelField)
                    ? $", at the level {Field(directive.MinimumLevelField)} records"
                    : !string.IsNullOrWhiteSpace(directive.MinimumLevelFixed)
                        ? $", at level {directive.MinimumLevelFixed}"
                        : string.Empty;

                return $"Counts {directive.Amount} towards {target}{level}.";
            })
            .ToList();

        return new ActivityTypeCreditReadDto(sentences.Count == 0, sentences);
    }

    /// <summary>The schema laid out for the page (Q13, S11); the text as it is when it does not parse.</summary>
    public static string LaidOutSchema(string schemaJson)
        => TryParse(() => FormSchemaParser.Parse(schemaJson)) is { } schema
            ? FormSchemaParser.Serialize(schema, indented: true)
            : schemaJson;

    /// <summary>The workflow laid out for the page (Q13, S11); the text as it is when it does not parse.</summary>
    public static string LaidOutWorkflow(string workflowJson)
        => TryParse(() => WorkflowParser.Parse(workflowJson)) is { } workflow
            ? WorkflowParser.Serialize(workflow, indented: true)
            : workflowJson;

    /// <summary>The credit rules laid out for the page (Q13, S11); the text as it is when they do not parse.</summary>
    public static string LaidOutCreditRules(string creditRulesJson)
        => TryParse(() => CreditRulesParser.Parse(creditRulesJson)) is { } rules
            ? CreditRulesParser.Serialize(rules, indented: true)
            : creditRulesJson;

    /// <summary>A field's label by its key in the schema, the first declaration's; null for a key it does not hold.</summary>
    private static Func<string, string?> FieldLabels(string schemaJson)
    {
        var schema = TryParse(() => FormSchemaParser.Parse(schemaJson));
        if (schema is null)
        {
            return _ => null;
        }

        var labels = schema.Sections
            .SelectMany(section => section.Fields)
            .GroupBy(field => field.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Label, StringComparer.Ordinal);

        return key => labels.TryGetValue(key, out var label) ? label : null;
    }

    // A stored payload predating a rule still parses; one that does not (only a hand-written row could) is not read.
    private static T? TryParse<T>(Func<T> parse)
        where T : class
    {
        try
        {
            return parse();
        }
        catch (Exception exception) when (exception is IActivityTypeRefusal or ArgumentException)
        {
            return null;
        }
    }
}
