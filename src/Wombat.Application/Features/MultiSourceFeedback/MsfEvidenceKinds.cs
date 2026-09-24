using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// What each kind of questionnaire becomes on release: the evidence type it writes, the instrument that type is, and
/// what it is called on a page. (T121, T164)
/// </summary>
/// <remarks>
/// <para>
/// One place, because its readers must agree. The release writes the type named here, and every reader of which EPAs a
/// released campaign recorded reads that same type back (<see cref="MsfCampaignCoverage" />, T186); the campaign form's
/// EPA picker, the create command and the release all narrow the covered EPAs with <see cref="CoverageToolKeyFor" />;
/// and the pages name the kind with <see cref="Describe" />.
/// </para>
/// <para>
/// The instrument key must equal the <c>WbaToolKey</c> on the type's <c>ActivityTypeSeedCatalogue</c> entry, which is
/// what the EPA tool lists name. <c>LearnerFeedbackSeedTests</c> holds the two together.
/// </para>
/// </remarks>
public static class MsfEvidenceKinds
{
    /// <summary>The seeded type a released multi-source feedback campaign writes, one row per covered EPA (T121).</summary>
    public const string MsfActivityTypeKey = "msf_cpsa";

    /// <summary>The seeded type a released learner-feedback campaign writes, one row per covered EPA (T164).</summary>
    public const string LearnerFeedbackActivityTypeKey = "learner_feedback_cpsa";

    /// <summary>The College instrument learner feedback is: the key PAED-015's tool list names (T122, T164).</summary>
    public const string LearnerFeedbackToolKey = "learner_feedback";

    /// <summary>The evidence type a released campaign on a template of this kind writes.</summary>
    public static string ActivityTypeKeyFor(MsfTemplateKind kind)
        => kind switch
        {
            MsfTemplateKind.LearnerFeedback => LearnerFeedbackActivityTypeKey,
            _ => MsfActivityTypeKey
        };

    /// <summary>
    /// The instrument a campaign of this kind's covered EPAs must be permitted by, or null when the tool lists are not
    /// consulted at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Null for MSF, as it has been since T121: an MSF release covers every EPA the campaign declared, whatever its list
    /// says, and every v11.1 list names MSF anyway (Annexure A names it for all fifteen EPAs).
    /// </para>
    /// <para>
    /// <see cref="LearnerFeedbackToolKey" /> for learner feedback, because the College names it for one EPA only, and a
    /// campaign that declared any other would write learner-feedback evidence under an EPA whose list refuses it. The
    /// narrowing is <c>ToolPermission.Evaluate</c>'s: an item with no list is unrestricted (D21).
    /// </para>
    /// </remarks>
    public static string? CoverageToolKeyFor(MsfTemplateKind kind)
        => kind switch
        {
            MsfTemplateKind.LearnerFeedback => LearnerFeedbackToolKey,
            _ => null
        };

    /// <summary>The kind as a reader is told it: "Multi-source feedback" or "Learner feedback".</summary>
    public static string Describe(MsfTemplateKind kind)
        => kind switch
        {
            MsfTemplateKind.LearnerFeedback => "Learner feedback",
            _ => "Multi-source feedback"
        };
}
