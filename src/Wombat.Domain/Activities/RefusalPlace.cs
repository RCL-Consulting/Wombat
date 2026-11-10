namespace Wombat.Domain.Activities;

/// <summary>
/// The builder's tab a refusal belongs to (T398, flow 17, Q6, J2): a schema problem Form's, a workflow problem
/// Workflow's, a credit problem Credit's, a Details refusal Details'. <c>EvidenceEpa</c>'s agreement check is Credit's;
/// <c>ActorFieldRules</c>' <c>field:</c> rule Workflow's, its duplicate keys and a user field's options Form's.
/// </summary>
public enum RefusalTab
{
    Details,
    Form,
    Workflow,
    Credit
}

/// <summary>
/// What kind of problem a <see cref="RefusalPlace" /> is, so the builder can word it at its place (T398, flow 17, Q6,
/// R3-0-spec-4 § 9). <see cref="Other" /> is worded by its <see cref="RefusalPlace.Sentence" /> alone, in the summary;
/// a kind the page does not word falls back to that too.
/// </summary>
public enum RefusalKind
{
    /// <summary>Anything else: the summary line alone.</summary>
    Other,

    /// <summary>A payload that is not JSON at all; <see cref="RefusalPlace.JsonLine" /> where the reader gave one.</summary>
    MalformedJson,

    /// <summary>Two sections share a key; <see cref="RefusalPlace.SectionKey" /> the key.</summary>
    DuplicateSectionKey,

    /// <summary>
    /// Two or more fields share a key; <see cref="RefusalPlace.FieldKeys" /> every holder's key (one entry each, the same
    /// key) and <see cref="RefusalPlace.SectionKey" /> the section of the first.
    /// </summary>
    DuplicateFieldKey,

    /// <summary>A section with no fields; <see cref="RefusalPlace.SectionKey" /> the section.</summary>
    EmptySection,

    /// <summary>A <c>field:</c> actor rule names no field of the form; <see cref="RefusalPlace.MoveKey" />, <see cref="RefusalPlace.NamedField" />.</summary>
    FieldRuleNamesNoField,

    /// <summary>A <c>field:</c> actor rule names a field that is not a User field; <see cref="RefusalPlace.MoveKey" />, <see cref="RefusalPlace.NamedField" />.</summary>
    FieldRuleNotUserField,

    /// <summary>A User field declares options; <see cref="RefusalPlace.FieldKeys" /> the field.</summary>
    UserFieldHasOptions,

    /// <summary>
    /// A credit directive reads its EPA from a field other than the form's EPA field; <see cref="RefusalPlace.DirectiveNumber" />,
    /// <see cref="RefusalPlace.NamedField" /> (what it reads), <see cref="RefusalPlace.ExpectedField" /> (what it should).
    /// </summary>
    CreditEpaFieldDisagrees
}

/// <summary>
/// Where in the builder one refusal of a draft belongs, and its sentence (T398, flow 17, Q6, J2, S2): the builder's
/// summary prints <see cref="Sentence" /> after the page's prefix ("Form:", "Workflow, move Cancel:"), word for word, and
/// words the problem again at its place by <see cref="Kind" /> and the members that kind sets. A member the kind does
/// not use is left at its default.
/// </summary>
/// <param name="Tab">The tab the place is on.</param>
/// <param name="Kind">What kind of problem it is.</param>
/// <param name="Sentence">The built sentence: what the parser or cross-check says, as its exception's message says it.</param>
public sealed record RefusalPlace(RefusalTab Tab, RefusalKind Kind, string Sentence)
{
    /// <summary>The section the place is in, by key; null when the place is in none.</summary>
    public string? SectionKey { get; init; }

    /// <summary>The fields at the place, by key: every holder of a duplicate key, or the one field. Empty by default.</summary>
    public IReadOnlyList<string> FieldKeys { get; init; } = [];

    /// <summary>The workflow move at the place, by key; null when the place is no move.</summary>
    public string? MoveKey { get; init; }

    /// <summary>The credit directive at the place, counted from 1 (S4: the summary says "Credit:", never "rule 1").</summary>
    public int? DirectiveNumber { get; init; }

    /// <summary>The field a rule names, by key, as written.</summary>
    public string? NamedField { get; init; }

    /// <summary>The field the rule should name, by key.</summary>
    public string? ExpectedField { get; init; }

    /// <summary>The line of the JSON the reader stopped at, counted from 1; null when it gave none.</summary>
    public long? JsonLine { get; init; }
}

/// <summary>
/// A refusal of a draft that says where each of its problems is (T398, flow 17, Q6): the three parse exceptions, and
/// <see cref="ActivityTypeDraftRefusedException" />. <see cref="Places" /> is never empty: a refusal with no place of
/// its own carries one of its tab, <see cref="RefusalKind.Other" />, its message as the sentence.
/// </summary>
public interface IActivityTypeRefusal
{
    IReadOnlyList<RefusalPlace> Places { get; }
}
