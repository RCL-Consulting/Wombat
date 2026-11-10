using Wombat.Domain.Identity;

namespace Wombat.Domain.Activities.Workflow;

/// <summary>
/// The actor grammar in words, for the builder's read of a workflow (T398, flow 17, Q2, S9, R3-0-spec-2 § 3): who may make
/// a move, and how much of the form it checks. The one implementation, so a move's "Who moves it" reads alike wherever it
/// is shown.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><c>subject</c> "The registrar"; <c>creator</c> "Whoever filed it".</item>
///   <item><c>role:X</c> "A ‹role label›" with its article, by <see cref="WombatRoleLabels" /> ("An Assessor").</item>
///   <item><c>field:k</c> "The person ‹the field's label› names".</item>
///   <item><c>a+b</c> joined by "who is also" ("The person Assessor names, who is also an Assessor"); a <c>scope:</c>
///   part narrows what it is joined to: <c>scope:speciality</c> "of the activity's speciality",
///   <c>scope:subspeciality</c> "of the activity's sub-speciality" ("A Speciality admin of the activity's
///   speciality").</item>
///   <item><c>a|b</c> joined by "or": "A Coordinator or an Administrator" when every choice is a role, else with a comma,
///   "The registrar, or whoever filed it".</item>
/// </list>
/// Anything else (a role Wombat does not hold, a field the form does not have, another <c>scope:</c>, a scope alone) is
/// not worded: <see cref="Describe" /> returns the rule as written and says so, and the page shows it as code.
/// </remarks>
public static class ActorRuleWords
{
    /// <summary>
    /// Who may act by <paramref name="rule" />, in words; or, when it cannot be worded, the rule as written with
    /// <c>Worded</c> false.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <param name="fieldLabel">A field's label by its key; null for a key the form does not hold.</param>
    public static (string Words, bool Worded) Describe(ActorRule rule, Func<string, string?> fieldLabel)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(fieldLabel);

        var words = Phrase(rule, fieldLabel);
        return words is null ? (ActorRuleParser.Serialize(rule), false) : (words, true);
    }

    /// <summary>How much of the form a move checks (T105): "The whole form", "The mover's own fields", "Formats only".</summary>
    public static string Validation(TransitionValidation validation) => validation switch
    {
        TransitionValidation.All => "The whole form",
        TransitionValidation.Owned => "The mover's own fields",
        TransitionValidation.Draft => "Formats only",
        _ => throw new ArgumentOutOfRangeException(nameof(validation), validation, null)
    };

    private static string? Phrase(ActorRule rule, Func<string, string?> fieldLabel) => rule switch
    {
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any => Choices(any.Rules, fieldLabel),
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => Conjunction(all.Rules, fieldLabel),
        _ => Single(rule, fieldLabel)
    };

    private static string? Single(ActorRule rule, Func<string, string?> fieldLabel) => rule switch
    {
        SubjectUserActorRule => "The registrar",
        CreatorUserActorRule => "Whoever filed it",
        NamedRoleActorRule role => Role(role.Role),
        FieldUserActorRule field => fieldLabel(field.Field) is { Length: > 0 } label ? $"The person {label} names" : null,
        _ => null
    };

    private static string? Role(string role)
    {
        if (!WombatRoleLabels.All.TryGetValue(role, out var label))
        {
            return null;
        }

        return ("AEIOU".Contains(char.ToUpperInvariant(label[0])) ? "An " : "A ") + label;
    }

    private static string? Scope(string scope) => scope switch
    {
        "speciality" => "of the activity's speciality",
        "subspeciality" or "sub_speciality" => "of the activity's sub-speciality",
        _ => null
    };

    /// <summary>
    /// <c>a+b</c>: the people it names joined by "who is also", narrowed by its one <c>scope:</c> part when it has one.
    /// </summary>
    private static string? Conjunction(IReadOnlyList<ActorRule> rules, Func<string, string?> fieldLabel)
    {
        var people = new List<string>();
        var scopes = new List<string>();

        foreach (var part in rules)
        {
            if (part is ScopeMatchActorRule scope)
            {
                if (Scope(scope.Scope) is not { } words)
                {
                    return null;
                }

                scopes.Add(words);
                continue;
            }

            if (Single(part, fieldLabel) is not { } person)
            {
                return null;
            }

            people.Add(person);
        }

        // A scope alone names nobody, and two scopes on one person do not read as one phrase.
        if (people.Count == 0 || scopes.Count > 1)
        {
            return null;
        }

        var phrase = people[0] + string.Concat(people.Skip(1).Select(person => ", who is also " + LowerFirst(person)));
        return scopes.Count == 1 ? $"{phrase} {scopes[0]}" : phrase;
    }

    /// <summary><c>a|b</c>: each choice, joined by "or".</summary>
    private static string? Choices(IReadOnlyList<ActorRule> rules, Func<string, string?> fieldLabel)
    {
        var choices = new List<string>();
        foreach (var choice in rules)
        {
            if (Phrase(choice, fieldLabel) is not { } words)
            {
                return null;
            }

            choices.Add(choices.Count == 0 ? words : LowerFirst(words));
        }

        if (choices.Count == 1)
        {
            return choices[0];
        }

        // Roles alone read as a list, "A Coordinator or an Administrator"; people otherwise take a comma before "or",
        // "The registrar, or whoever filed it", so the second does not read as part of the first.
        return rules.All(choice => choice is NamedRoleActorRule)
            ? string.Join(", ", choices.Take(choices.Count - 1)) + " or " + choices[^1]
            : string.Join(", ", choices.Take(choices.Count - 1)) + ", or " + choices[^1];
    }

    // Every phrase opens with an article or a pronoun ("The", "A", "An", "Whoever"), so its first letter alone lowers.
    private static string LowerFirst(string words) => char.ToLowerInvariant(words[0]) + words[1..];
}
