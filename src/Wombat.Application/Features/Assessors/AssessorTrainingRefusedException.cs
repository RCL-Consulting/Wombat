namespace Wombat.Application.Features.Assessors;

/// <summary>Why a training save was refused, for a person the caller may see (T376, flow 12, lane T).</summary>
public enum AssessorTrainingRefusal
{
    /// <summary>The person belongs to no institution, so their profile would have nowhere to be recorded (M1).</summary>
    NoInstitution,

    /// <summary>The person does not hold Assessor (now: the role was removed in another tab, say).</summary>
    NotAnAssessor
}

/// <summary>
/// A training save refused for a person in the caller's scope (T376, flow 12, lane T; the pattern of
/// <c>InvitationIssueRefusedException</c>). The page words it from <see cref="Reason" />, never from the message, which is
/// what the audit row keeps and names nobody. Derived from <see cref="InvalidOperationException" />, as the refusals it
/// replaces were, so nothing that catches one changes. The caller and scope refusals stay
/// <see cref="UnauthorizedAccessException" /> in <c>UserAdministrationRules</c>' words.
/// </summary>
public sealed class AssessorTrainingRefusedException : InvalidOperationException
{
    public AssessorTrainingRefusedException(AssessorTrainingRefusal reason, string message)
        : base(message)
    {
        Reason = reason;
    }

    public AssessorTrainingRefusal Reason { get; }

    /// <summary>The page's words for a refusal, naming the person the page shows.</summary>
    public static string Words(AssessorTrainingRefusal reason, string personName) => reason switch
    {
        AssessorTrainingRefusal.NoInstitution =>
            $"{personName} belongs to no institution, so no assessor training can be recorded. The training was not saved.",
        _ => $"{personName} does not hold the Assessor role now, so the training was not saved."
    };
}
