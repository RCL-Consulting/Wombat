namespace Wombat.Domain.Activities.Workflow;

/// <summary>
/// How much of the form a transition insists on before it moves the activity, declared as the transition's
/// <c>validation</c> property (T105). Format is always checked; this decides which <c>required</c> fields count.
/// </summary>
/// <remarks>
/// <para>
/// Before T105 every transition validated the whole schema as if submitting, so a half-filled draft could not be
/// cancelled and an assessor could not decline without filling in ratings. The CPSA seeds worked around it by leaving
/// the assessor's fields un-required and listing them in <c>requires_fields</c> on <c>complete</c>, which moved "what is
/// mandatory" out of the schema and into the workflow. A transition now says which it is, and the schema says what is
/// mandatory. <c>requires_fields</c> is back to meaning "additionally required for this step", under every value.
/// </para>
/// <para>
/// Declared rather than inferred from the target state: every convention this repository has inferred from the shape
/// of a workflow has broken on some builder-made shape (T122's review history).
/// </para>
/// </remarks>
public enum TransitionValidation
{
    /// <summary>
    /// Every <c>required</c> field in the schema (that is visible). The default, and the strict one: a transition that
    /// declares nothing is checked more, never less. Use it for a move into a terminal state, where everything the form
    /// asks for must be there.
    /// </summary>
    All = 0,

    /// <summary>
    /// Only the <c>required</c> fields the mover may write in the state they are moving from — what is theirs to fill in
    /// before handing on. A trainee's submit need not wait for fields only the assessor can write.
    /// </summary>
    Owned = 1,

    /// <summary>
    /// Formats only; nothing is required. For a withdrawal, a decline, a recall: a move that disposes of the activity or
    /// sends it back must not demand a finished form.
    /// </summary>
    Draft = 2
}
