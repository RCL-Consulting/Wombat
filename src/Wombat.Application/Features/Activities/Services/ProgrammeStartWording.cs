using System.Globalization;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// How the programme-start bound on an encounter date is worded, to whoever reads it (T342, C12, B10).
/// </summary>
/// <remarks>
/// <para>
/// The form is shared: the registrar filing her own activity reads "your programme", and an assessor or the committee
/// reading the same form reads "the trainee's programme". The server's refusal (<c>EncounterDateGate</c>) and the form's
/// hint, predicted while the date is typed (T192), use these two sentences so that the hint and the refusal it predicts
/// agree word for word, for each reader.
/// </para>
/// <para>
/// "Is the reader the subject" is the one question. On a write it is the mover; on the page it is the viewer.
/// </para>
/// </remarks>
public static class ProgrammeStartWording
{
    /// <summary>"your" to the activity's subject, "the trainee's" to anyone else.</summary>
    public static string Whose(bool readerIsSubject) => readerIsSubject ? "your" : "the trainee's";

    /// <summary>
    /// The refusal: "The date cannot be before your programme started (2026-01-15)." It follows the date field's own
    /// label on the page ("Date observed: …").
    /// </summary>
    public static string Refusal(bool readerIsSubject, DateOnly programmeStartedOn)
        => $"The date cannot be before {Whose(readerIsSubject)} programme started ({Iso(programmeStartedOn)}).";

    /// <summary>
    /// The form's hint as the date is typed: "This date is before your programme started (2026-01-15), and will not be
    /// accepted."
    /// </summary>
    public static string Hint(bool readerIsSubject, DateOnly programmeStartedOn)
        => $"This date is before {Whose(readerIsSubject)} programme started ({Iso(programmeStartedOn)}), and will not be accepted.";

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
