using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// A refused form's summary's title and text (T342, flow 03; Spec § 1, "Refusal"): the title says what did not happen
/// and what state the work is still in, the text what to do. The lines are the refusal's own, one a field
/// (<see cref="FieldRefusals.Split" />); <see cref="RefusalSummary" /> draws all three, on Log an activity and on the
/// activity page alike.
/// </summary>
/// <param name="Title">"Not submitted. It is still a draft."</param>
/// <param name="Text">"Fix the field below and submit again."; null when the refusal names no field.</param>
public sealed record RefusalHeading(string Title, string? Text)
{
    /// <summary>Title and text as one message, for a notice that carries them across a navigation.</summary>
    public override string ToString() => Text is null ? Title : $"{Title} {Text}";
}

public static class RefusalWords
{
    /// <summary>The title of a submit refused straight after the create that kept the draft (T127).</summary>
    public const string SavedNotSubmitted = "Saved as a draft, but not submitted.";

    /// <summary>A move refused for its fields: "Not submitted. It is still a draft." · "Fix the 2 fields below and submit again."</summary>
    public static RefusalHeading ForMove(ActivityActionDto action, string currentStateLabel, int fieldCount)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new RefusalHeading(
            $"{NotDone(action)} It is still {StateInWords(currentStateLabel)}.",
            FixText(fieldCount, action.Label.ToLowerInvariant()));
    }

    /// <summary>A Save draft refused for its fields: "Not saved. It is still a draft." (E3).</summary>
    public static RefusalHeading ForSave(string currentStateLabel, int fieldCount)
        => new($"Not saved. It is still {StateInWords(currentStateLabel)}.", FixText(fieldCount, "save"));

    /// <summary>
    /// The submit refused straight after the create, which Log an activity hands to the activity's page (T127): "Saved as
    /// a draft, but not submitted." · "Fix the 6 fields below and submit again.".
    /// </summary>
    public static RefusalHeading ForSubmitAfterCreate(int fieldCount)
        => new(SavedNotSubmitted, FixText(fieldCount, "submit"));

    /// <summary>
    /// The note panel's refused summary's title (T350, note 3; Spec § 1, "Refusal"): what did not happen and the state the
    /// activity is still in, as <see cref="ForMove" /> words it: "Not declined. It is still Requested.", "Not returned. It is
    /// still Awaiting discussion.". The panel has one field, the note, so it has no "Fix …" text of its own; the line under
    /// the title is the refusal's.
    /// </summary>
    public static string ForNote(ActivityActionDto action, string currentStateLabel)
    {
        ArgumentNullException.ThrowIfNull(action);

        return $"{NotDone(action)} It is still {StateInWords(currentStateLabel)}.";
    }

    /// <summary>
    /// "Not submitted.", "Not returned.", "Not declined.", "Not completed.", "Not signed off."; "Verify was not made." for a
    /// move with no word for it.
    /// </summary>
    public static string NotDone(ActivityActionDto action)
    {
        ArgumentNullException.ThrowIfNull(action);

        // The moves that lead on, which no target state names: a submit, and a return to the author (T350, note 3).
        if (LeadingOnWords.TryGetValue(action.TransitionKey, out var words))
        {
            return words;
        }

        // A move that ends the activity reads as the state it would have ended in (MoveOutcome.TargetIsFinal, T342).
        var ends = action.TargetIsFinal && !string.IsNullOrWhiteSpace(action.TargetStateLabel);
        return ends ? $"Not {action.TargetStateLabel.Trim().ToLowerInvariant()}." : $"{action.Label} was not made.";
    }

    private static readonly IReadOnlyDictionary<string, string> LeadingOnWords = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["submit"] = "Not submitted.",
        ["return"] = "Not returned."
    };

    /// <summary>"Fix the field below and submit again.", "Fix the 2 fields below and save again."; null for no field.</summary>
    public static string? FixText(int fieldCount, string verb)
        => fieldCount switch
        {
            <= 0 => null,
            1 => $"Fix the field below and {verb} again.",
            _ => $"Fix the {fieldCount} fields below and {verb} again."
        };

    private static string StateInWords(string stateLabel)
        => string.Equals(stateLabel?.Trim(), "Draft", StringComparison.OrdinalIgnoreCase) ? "a draft" : stateLabel?.Trim() ?? "as it was";
}
