using System.Globalization;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

/// <summary>
/// The words of an assessor's training record (T376, flow 12, lane T; Spec 3's messages, boards T02–T08): its four
/// statuses, the list's "Not recorded", the save's result, the form's two refusals and the user page's absence sentence.
/// One place, so the Assessors list, the user page's Assessor training section and the save's validator say the same.
/// </summary>
public static class AssessorTrainingWords
{
    /// <summary>What the list's Training status says for a holder with no record (the nit: Not started is a record).</summary>
    public const string NotRecorded = "Not recorded";

    /// <summary>The refusal of a save with no status chosen (T05; it replaces T324's "The … field is required.").</summary>
    public const string ChooseStatus = "Choose the training status.";

    /// <summary>The refusal of a save with no qualifications typed (T05).</summary>
    public const string TypeQualifications = "Type the qualifications, as they should read.";

    /// <summary>The status group's help, as built.</summary>
    public const string StatusHelp = "Faculty-development lifecycle. In-training assessors may assess under supervision.";

    /// <summary>The completion date's help, as built.</summary>
    public const string CompletedOnHelp = "Date the assessor completed assessor training. Leave blank if unrecorded.";

    /// <summary>The four statuses, in the order the form offers them.</summary>
    public static readonly IReadOnlyList<AssessorTrainingStatus> Statuses =
    [
        AssessorTrainingStatus.NotStarted,
        AssessorTrainingStatus.InTraining,
        AssessorTrainingStatus.Provisional,
        AssessorTrainingStatus.Trained
    ];

    /// <summary>A status in words: "Provisional (awaiting sign-off)" (the list said "Provisional" until T376).</summary>
    public static string Status(AssessorTrainingStatus status) => status switch
    {
        AssessorTrainingStatus.InTraining => "In training",
        AssessorTrainingStatus.Provisional => "Provisional (awaiting sign-off)",
        AssessorTrainingStatus.Trained => "Trained",
        _ => "Not started"
    };

    /// <summary>Whether a status records a completion date: only a finished one (Provisional or Trained).</summary>
    public static bool HasCompletion(AssessorTrainingStatus status)
        => status is AssessorTrainingStatus.Provisional or AssessorTrainingStatus.Trained;

    /// <summary>An ISO date, or <see cref="NotRecorded" /> for none.</summary>
    public static string CompletedOn(DateOnly? completedOn)
        => completedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? NotRecorded;

    /// <summary>The save's result, its first sentence: "Training saved for Fatma Khumalo:" (bold on the page).</summary>
    public static string SavedLead(string personName) => $"Training saved for {personName}:";

    /// <summary>
    /// What was saved, after <see cref="SavedLead" />: "Provisional (awaiting sign-off), completed 2026-09-20.", "Trained, no
    /// date." (Botha, check Left 4: a Spec line with no board), or the status alone for one that records no date.
    /// </summary>
    public static string SavedWhat(AssessorTrainingStatus status, DateOnly? completedOn)
        => !HasCompletion(status)
            ? $"{Status(status)}."
            : completedOn is { } date
                ? $"{Status(status)}, completed {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}."
                : $"{Status(status)}, no date.";

    /// <summary>The whole result, as one string (the lead and what was saved).</summary>
    public static string Saved(string personName, AssessorTrainingStatus status, DateOnly? completedOn)
        => $"{SavedLead(personName)} {SavedWhat(status, completedOn)}";

    /// <summary>
    /// The user page opened at <c>?section=training</c> for a person who does not hold Assessor (S10, NEW): said once, in
    /// the section's place.
    /// </summary>
    public static string NotAnAssessor(string personName)
        => $"{personName} does not hold the Assessor role, so has no assessor training here.";

    /// <summary>What the section says on the caller's own page: nobody records their own training (M1, T278).</summary>
    public const string OwnTraining = "You cannot record your own assessor training. Another administrator can.";
}
