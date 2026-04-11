namespace Wombat.Domain.Activities.Workflow;

public static class ActorRuleParser
{
    public static ActorRule Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmedValue = value.Trim();

        if (trimmedValue.Contains('|', StringComparison.Ordinal))
        {
            return new CombinedActorRule(
                ActorRuleCombinationKind.Any,
                Split(trimmedValue, '|').Select(ParseConjunction).ToList());
        }

        return ParseConjunction(trimmedValue);
    }

    public static string Serialize(ActorRule actorRule)
    {
        ArgumentNullException.ThrowIfNull(actorRule);

        return actorRule switch
        {
            SubjectUserActorRule => "subject",
            CreatorUserActorRule => "creator",
            NamedRoleActorRule namedRole => $"role:{namedRole.Role}",
            ScopeMatchActorRule scopeMatch => $"scope:{scopeMatch.Scope}",
            CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.All
                => string.Join('+', combined.Rules.Select(Serialize)),
            CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.Any
                => string.Join('|', combined.Rules.Select(Serialize)),
            _ => throw new WorkflowParseException($"Unsupported actor rule '{actorRule.GetType().Name}'.")
        };
    }

    private static ActorRule ParseConjunction(string value)
    {
        var parts = Split(value, '+');
        if (parts.Count == 1)
        {
            return ParseSingle(parts[0]);
        }

        return new CombinedActorRule(
            ActorRuleCombinationKind.All,
            parts.Select(ParseSingle).ToList());
    }

    private static ActorRule ParseSingle(string value)
    {
        return value switch
        {
            "subject" => new SubjectUserActorRule(),
            "creator" => new CreatorUserActorRule(),
            _ when value.StartsWith("role:", StringComparison.Ordinal) =>
                new NamedRoleActorRule(GetNamedValue(value, "role:")),
            _ when value.StartsWith("scope:", StringComparison.Ordinal) =>
                new ScopeMatchActorRule(GetNamedValue(value, "scope:")),
            _ => throw new WorkflowParseException(
                "Actor rule grammar supports only 'subject', 'creator', 'role:<name>', 'scope:<name>', '+' and '|'.")
        };
    }

    private static string GetNamedValue(string value, string prefix)
    {
        var name = value[prefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new WorkflowParseException($"Actor rule '{value}' is missing a name after '{prefix}'.");
        }

        return name;
    }

    private static IReadOnlyList<string> Split(string value, char separator)
    {
        var parts = value
            .Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (parts.Count == 0)
        {
            throw new WorkflowParseException("Actor rule expression cannot be empty.");
        }

        return parts;
    }
}
