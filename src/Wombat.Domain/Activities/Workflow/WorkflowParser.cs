using System.Text.Json;

namespace Wombat.Domain.Activities.Workflow;

public static class WorkflowParser
{
    public static Workflow Parse(string workflowJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowJson);

        try
        {
            using var document = JsonDocument.Parse(workflowJson);
            return ParseWorkflow(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new WorkflowParseException($"Workflow JSON is malformed: {exception.Message}");
        }
    }

    public static string Serialize(Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteNumber("version", workflow.Version);
        writer.WriteString("initial_state", workflow.InitialState);
        writer.WritePropertyName("states");
        writer.WriteStartArray();

        foreach (var state in workflow.States)
        {
            writer.WriteStartObject();
            writer.WriteString("key", state.Key);
            writer.WriteString("label", state.Label);

            if (state.Terminal)
            {
                writer.WriteBoolean("terminal", true);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WritePropertyName("transitions");
        writer.WriteStartArray();

        foreach (var transition in workflow.Transitions)
        {
            writer.WriteStartObject();
            writer.WriteString("key", transition.Key);
            writer.WritePropertyName("from");

            if (transition.From.Count == 1)
            {
                writer.WriteStringValue(transition.From[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var fromState in transition.From)
                {
                    writer.WriteStringValue(fromState);
                }

                writer.WriteEndArray();
            }

            writer.WriteString("to", transition.To);
            writer.WriteString("actor", ActorRuleParser.Serialize(transition.Actor));

            if (transition.RequiresNote)
            {
                writer.WriteBoolean("requires_note", true);
            }

            if (transition.RequiresFields.Count > 0)
            {
                writer.WritePropertyName("requires_fields");
                writer.WriteStartArray();
                foreach (var requiredField in transition.RequiresFields)
                {
                    writer.WriteStringValue(requiredField);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static Workflow ParseWorkflow(JsonElement root)
    {
        EnsureObject(root, "Workflow root must be an object.");
        EnsureAllowedProperties(root, ["version", "initial_state", "states", "transitions"], "workflow");

        var version = GetRequiredInt(root, "version");
        var initialState = GetRequiredString(root, "initial_state");
        var states = GetRequiredArray(root, "states", "Workflow states must be an array.")
            .EnumerateArray()
            .Select(ParseState)
            .ToList();
        var transitions = GetRequiredArray(root, "transitions", "Workflow transitions must be an array.")
            .EnumerateArray()
            .Select(ParseTransition)
            .ToList();

        if (!states.Any(state => string.Equals(state.Key, initialState, StringComparison.Ordinal)))
        {
            throw new WorkflowParseException($"Initial state '{initialState}' is not declared.");
        }

        var stateKeys = states.Select(state => state.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var transition in transitions)
        {
            foreach (var fromState in transition.From)
            {
                if (!stateKeys.Contains(fromState))
                {
                    throw new WorkflowParseException(
                        $"Transition '{transition.Key}' references undeclared source state '{fromState}'.");
                }
            }

            if (!stateKeys.Contains(transition.To))
            {
                throw new WorkflowParseException(
                    $"Transition '{transition.Key}' references undeclared target state '{transition.To}'.");
            }
        }

        var reachableStates = new HashSet<string>(StringComparer.Ordinal) { initialState };
        var queue = new Queue<string>();
        queue.Enqueue(initialState);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var nextState in transitions
                         .Where(transition => transition.From.Contains(current, StringComparer.Ordinal))
                         .Select(transition => transition.To))
            {
                if (reachableStates.Add(nextState))
                {
                    queue.Enqueue(nextState);
                }
            }
        }

        var unreachableStates = states
            .Select(state => state.Key)
            .Where(stateKey => !reachableStates.Contains(stateKey))
            .ToList();

        if (unreachableStates.Count > 0)
        {
            throw new WorkflowParseException(
                $"Workflow contains unreachable states: {string.Join(", ", unreachableStates)}.");
        }

        return new Workflow(version, initialState, states, transitions);
    }

    private static WorkflowState ParseState(JsonElement element)
    {
        EnsureObject(element, "State must be an object.");
        EnsureAllowedProperties(element, ["key", "label", "terminal"], "state");

        return new WorkflowState(
            GetRequiredString(element, "key"),
            GetRequiredString(element, "label"),
            GetBooleanOrDefault(element, "terminal"));
    }

    private static WorkflowTransition ParseTransition(JsonElement element)
    {
        EnsureObject(element, "Transition must be an object.");
        EnsureAllowedProperties(
            element,
            ["key", "from", "to", "actor", "requires_note", "requires_fields"],
            "transition");

        return new WorkflowTransition(
            GetRequiredString(element, "key"),
            ParseFromStates(GetRequiredProperty(element, "from")),
            GetRequiredString(element, "to"),
            ActorRuleParser.Parse(GetRequiredString(element, "actor")),
            GetBooleanOrDefault(element, "requires_note"),
            ParseOptionalStringArray(element, "requires_fields"));
    }

    private static IReadOnlyList<string> ParseFromStates(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => [GetRequiredTrimmedStringValue(element, "from")],
            JsonValueKind.Array => element
                .EnumerateArray()
                .Select(item => GetRequiredTrimmedStringValue(item, "from"))
                .ToList(),
            _ => throw new WorkflowParseException("Property 'from' must be a string or array of strings.")
        };
    }

    private static IReadOnlyList<string> ParseOptionalStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var arrayElement))
        {
            return [];
        }

        if (arrayElement.ValueKind != JsonValueKind.Array)
        {
            throw new WorkflowParseException($"Property '{propertyName}' must be an array.");
        }

        return arrayElement
            .EnumerateArray()
            .Select(item => GetRequiredTrimmedStringValue(item, propertyName))
            .ToList();
    }

    private static string GetRequiredTrimmedStringValue(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.String)
        {
            throw new WorkflowParseException($"Property '{propertyName}' entries must be strings.");
        }

        var value = element.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new WorkflowParseException($"Property '{propertyName}' entries must not be empty.");
        }

        return value;
    }

    private static void EnsureAllowedProperties(JsonElement element, IEnumerable<string> allowedProperties, string subject)
    {
        var allowed = allowedProperties.ToHashSet(StringComparer.Ordinal);

        foreach (var property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                throw new WorkflowParseException($"Unknown property '{property.Name}' in {subject}.");
            }
        }
    }

    private static JsonElement GetRequiredProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            throw new WorkflowParseException($"Required property '{propertyName}' is missing.");
        }

        return property;
    }

    private static JsonElement GetRequiredArray(JsonElement element, string propertyName, string errorMessage)
    {
        var property = GetRequiredProperty(element, propertyName);
        if (property.ValueKind != JsonValueKind.Array)
        {
            throw new WorkflowParseException(errorMessage);
        }

        return property;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        var property = GetRequiredProperty(element, propertyName);
        return GetRequiredTrimmedStringValue(property, propertyName);
    }

    private static bool GetBooleanOrDefault(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False)
        {
            throw new WorkflowParseException($"Property '{propertyName}' must be a boolean.");
        }

        return property.GetBoolean();
    }

    private static int GetRequiredInt(JsonElement element, string propertyName)
    {
        var property = GetRequiredProperty(element, propertyName);
        if (!property.TryGetInt32(out var value))
        {
            throw new WorkflowParseException($"Property '{propertyName}' must be an integer.");
        }

        return value;
    }

    private static void EnsureObject(JsonElement element, string message)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new WorkflowParseException(message);
        }
    }
}
