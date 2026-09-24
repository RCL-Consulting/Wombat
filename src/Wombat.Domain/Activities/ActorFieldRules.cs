using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;

namespace Wombat.Domain.Activities;

/// <summary>
/// Which fields of an activity type name a person who may act on it, what that person must be, and the invariants a
/// type must meet before it can be published with them (T102).
/// </summary>
/// <remarks>
/// <para>
/// A <c>field:</c> actor rule makes whoever a data field names an actor on the activity: a transition's actor, the
/// writer of a state or section, a reader through the read gate, a row in their inbox. That value is authorization
/// input, so everything that asks "which fields are those?" — the self-nomination guard, the nominee gate, the read
/// gate, the publish check — walks the rules through this one class, so no two of them can disagree about the set.
/// </para>
/// <para>
/// A <b>nominee field</b> is every <c>user</c> field plus every field a <c>field:</c> rule names. The union, not either
/// half: a <c>user</c> field no rule names still lands a person on the record, and a pinned version whose rule names a
/// non-user field (refused at publish from T102 on, but a stored version may predate that) still grants rights.
/// </para>
/// </remarks>
public static class ActorFieldRules
{
    /// <summary>
    /// Every actor rule a type declares: transition actors, state <c>editable_by</c>, and section and field
    /// <c>editable_by</c>.
    /// </summary>
    public static IEnumerable<ActorRule> DeclaredActorRules(FormSchema schema, Workflow.Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(workflow);

        foreach (var transition in workflow.Transitions)
        {
            yield return transition.Actor;
        }

        foreach (var state in workflow.States)
        {
            if (state.EditableBy is not null)
            {
                yield return state.EditableBy;
            }
        }

        foreach (var section in schema.Sections)
        {
            if (section.EditableBy is not null)
            {
                yield return section.EditableBy;
            }

            foreach (var field in section.Fields)
            {
                if (field.EditableBy is not null)
                {
                    yield return field.EditableBy;
                }
            }
        }
    }

    /// <summary>
    /// Collects the names of every field a <c>field:</c> rule reads, anywhere in the rule tree.
    /// </summary>
    public static void CollectFieldNames(ActorRule? rule, ISet<string> into)
    {
        ArgumentNullException.ThrowIfNull(into);

        switch (rule)
        {
            case null:
                return;
            case FieldUserActorRule fieldUser:
                into.Add(fieldUser.Field);
                return;
            case CombinedActorRule combined:
                foreach (var child in combined.Rules)
                {
                    CollectFieldNames(child, into);
                }

                return;
        }
    }

    /// <summary>
    /// The fields some <c>field:</c> rule in the type reads.
    /// </summary>
    public static IReadOnlySet<string> ActorFieldNames(FormSchema schema, Workflow.Workflow workflow)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rule in DeclaredActorRules(schema, workflow))
        {
            CollectFieldNames(rule, names);
        }

        return names;
    }

    /// <summary>
    /// Every nominee field, with the roles its nominee must hold — ALL of them, not any.
    /// </summary>
    /// <remarks>
    /// A key the schema declares more than once (refused at publish from T102 on, but possible in a stored version)
    /// requires every role any of its <c>user</c> declarations names, so the weaker declaration can never be the one
    /// that decides. A key named only by a <c>field:</c> rule requires <see cref="WombatRoles.Assessor" />, the role
    /// every such rule in the corpus means.
    /// </remarks>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> RequiredRolesByNomineeField(
        FormSchema schema,
        Workflow.Workflow workflow)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        foreach (var field in schema.Sections.SelectMany(section => section.Fields))
        {
            if (field.Type == FieldType.User && !result.ContainsKey(field.Key))
            {
                result[field.Key] = RequiredRolesForUserField(schema, field.Key);
            }
        }

        foreach (var name in ActorFieldNames(schema, workflow))
        {
            result.TryAdd(name, [WombatRoles.Assessor]);
        }

        return result;
    }

    /// <summary>
    /// The roles a <c>user</c> field's nominee must hold. The picker asks this; so, through
    /// <see cref="RequiredRolesByNomineeField" />, does the server.
    /// </summary>
    public static IReadOnlyList<string> RequiredRolesForUserField(FormSchema schema, string fieldKey)
    {
        ArgumentNullException.ThrowIfNull(schema);

        var roles = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == FieldType.User && string.Equals(field.Key, fieldKey, StringComparison.Ordinal))
            .Select(field => field.NomineeRole ?? WombatRoles.Assessor)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return roles.Length == 0 ? [WombatRoles.Assessor] : roles;
    }

    /// <summary>
    /// Refuses a type whose nominee fields could not be judged unambiguously, naming every problem and its repair.
    /// </summary>
    /// <remarks>
    /// Checked at save and at publish rather than in the parser, so a stored version that predates the rule still
    /// parses and its in-flight activities still move. Refuses:
    /// <list type="bullet">
    ///   <item>a section or field key declared twice — DataJson, the writable set and the actor grammar all treat a key
    ///   as one field, so a second declaration can only disagree with the first;</item>
    ///   <item>a <c>field:</c> rule naming no field, or a field that is not a <c>user</c> field — such a field renders
    ///   as free text, so the rule's actor is whatever id someone types;</item>
    ///   <item>inline <c>options</c> or a <c>catalogue</c> on a <c>user</c> field — the people a user field offers
    ///   come from the directory, which is also what the server checks.</item>
    /// </list>
    /// </remarks>
    public static void EnsurePublishable(FormSchema schema, Workflow.Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(workflow);

        var problems = new List<string>();

        foreach (var duplicate in schema.Sections
                     .GroupBy(section => section.Key, StringComparer.Ordinal)
                     .Where(group => group.Count() > 1))
        {
            problems.Add($"Section key '{duplicate.Key}' is used by {duplicate.Count()} sections; give each section its own key.");
        }

        var fieldsByKey = schema.Sections
            .SelectMany(section => section.Fields.Select(field => (Section: section, Field: field)))
            .GroupBy(entry => entry.Field.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        foreach (var (key, entries) in fieldsByKey.Where(pair => pair.Value.Count > 1))
        {
            problems.Add(
                $"Field key '{key}' is used by {entries.Count} fields ({string.Join(", ", entries.Select(entry => $"'{entry.Field.Label}' in '{entry.Section.Title}'"))}); " +
                "give each field its own key.");
        }

        foreach (var (location, fieldName) in FieldRuleLocations(schema, workflow))
        {
            if (!fieldsByKey.TryGetValue(fieldName, out var entries))
            {
                problems.Add($"{location} names 'field:{fieldName}', but the form has no field '{fieldName}'; add a User field with that key on the Form tab.");
            }
            else if (entries.Any(entry => entry.Field.Type != FieldType.User))
            {
                problems.Add($"{location} names 'field:{fieldName}', but '{fieldName}' is not a User field; a field: rule must name a User field.");
            }
        }

        foreach (var field in schema.Sections.SelectMany(section => section.Fields).Where(field => field.Type == FieldType.User))
        {
            if (field.Options.Count > 0 || !string.IsNullOrWhiteSpace(field.CatalogueKey))
            {
                problems.Add($"User field '{field.Key}' declares its own options; the people it offers come from the directory, so remove them.");
            }
        }

        if (problems.Count > 0)
        {
            throw new SchemaParseException(string.Join(" ", problems.Distinct(StringComparer.Ordinal)));
        }
    }

    private static IEnumerable<(string Location, string FieldName)> FieldRuleLocations(FormSchema schema, Workflow.Workflow workflow)
    {
        foreach (var transition in workflow.Transitions)
        {
            foreach (var name in FieldNamesOf(transition.Actor))
            {
                yield return ($"Transition '{transition.Key}' actor", name);
            }
        }

        foreach (var state in workflow.States)
        {
            foreach (var name in FieldNamesOf(state.EditableBy))
            {
                yield return ($"State '{state.Key}' editable_by", name);
            }
        }

        foreach (var section in schema.Sections)
        {
            foreach (var name in FieldNamesOf(section.EditableBy))
            {
                yield return ($"Section '{section.Key}' editable_by", name);
            }

            foreach (var field in section.Fields)
            {
                foreach (var name in FieldNamesOf(field.EditableBy))
                {
                    yield return ($"Field '{field.Key}' editable_by", name);
                }
            }
        }
    }

    private static IEnumerable<string> FieldNamesOf(ActorRule? rule)
    {
        var names = new SortedSet<string>(StringComparer.Ordinal);
        CollectFieldNames(rule, names);
        return names;
    }
}
