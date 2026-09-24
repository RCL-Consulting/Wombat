namespace Wombat.Application.Tests.TestHelpers;

/// <summary>
/// Stored workflows that finish somewhere other than the literal <c>completed</c>, copied from the seeds that do (T203):
/// the reflective exercise finishes in <c>discussed</c>, the teaching session in <c>accepted</c>, MSF in <c>recorded</c>,
/// the procedure log is born in its terminal <c>logged</c>, and the WBA instruments finish in <c>completed</c> with
/// <c>accepted</c> a step on the way. States, terminal flags and actors are the seeds'; <c>validation</c> and
/// <c>editable_by</c> are left out, as nothing that reads "finished" looks at them.
/// </summary>
internal static class FinishingWorkflows
{
    /// <summary>
    /// <c>reflective_exercise_cpsa</c>: finishes in <c>discussed</c>, recorded by the supervisor the trainee names;
    /// <c>cancelled</c> is a dead end.
    /// </summary>
    public const string ReflectiveExercise = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Awaiting discussion" },
            { "key": "discussed", "label": "Discussed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject|creator" },
            { "key": "record_discussion", "from": "submitted", "to": "discussed", "actor": "field:assessor_user_id" },
            { "key": "return", "from": "submitted", "to": "draft", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["draft", "submitted"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>
    /// Not a seed: a hypothetical later version of the reflective exercise that adds a sign-off after the discussion,
    /// so <c>discussed</c> is a step on the way and only <c>signed_off</c> finishes. An activity pinned to
    /// <see cref="ReflectiveExercise" /> is finished in <c>discussed</c>; one pinned to this is not.
    /// </summary>
    public const string ReflectiveExerciseWithSignOff = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Awaiting discussion" },
            { "key": "discussed", "label": "Discussed" },
            { "key": "signed_off", "label": "Signed off", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject|creator" },
            { "key": "record_discussion", "from": "submitted", "to": "discussed", "actor": "field:assessor_user_id" },
            { "key": "sign_off", "from": "discussed", "to": "signed_off", "actor": "field:assessor_user_id" },
            { "key": "cancel", "from": ["draft", "submitted"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>
    /// <c>teaching_session</c>: finishes in <c>accepted</c>, which only a SpecialityAdmin of its speciality can reach.
    /// </summary>
    public const string TeachingSession = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" },
            { "key": "accepted", "label": "Accepted", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "accept", "from": "submitted", "to": "accepted", "actor": "role:SpecialityAdmin+scope:speciality" }
          ]
        }
        """;

    /// <summary>
    /// <c>msf_cpsa</c>: finishes in <c>recorded</c>. Written by the system when a campaign is released, and recorded by
    /// a Coordinator or an Administrator, never an Assessor.
    /// </summary>
    public const string Msf = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "recorded", "label": "Recorded", "terminal": true }
          ],
          "transitions": [
            { "key": "record", "from": "draft", "to": "recorded", "actor": "role:Coordinator|role:Administrator" }
          ]
        }
        """;

    /// <summary><c>procedure_log</c> (and <c>journal_club</c>): born in <c>logged</c>, its only state and a terminal one.</summary>
    public const string ProcedureLog = """
        {
          "version": 1,
          "initial_state": "logged",
          "states": [
            { "key": "logged", "label": "Logged", "terminal": true }
          ],
          "transitions": []
        }
        """;

    /// <summary>
    /// <c>mini_cex</c>, the legacy WBA shape: finishes in <c>completed</c>, with <c>accepted</c> a step on the way and
    /// <c>declined</c> and <c>cancelled</c> dead ends.
    /// </summary>
    public const string Wba = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "accepted", "label": "Accepted" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "accept", "from": "requested", "to": "accepted", "actor": "field:assessor_user_id" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id" },
            { "key": "cancel", "from": ["requested", "accepted"], "to": "cancelled", "actor": "subject|field:assessor_user_id" },
            { "key": "complete", "from": "accepted", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;
}
