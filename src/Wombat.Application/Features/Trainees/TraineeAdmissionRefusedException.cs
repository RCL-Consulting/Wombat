namespace Wombat.Application.Features.Trainees;

/// <summary>Why an admission was refused (T373, flow 11, S9): what the Admit page maps to its own words and place.</summary>
public enum TraineeAdmissionRefusal
{
    /// <summary>
    /// No registrar waiting for admission has this id: none exists, the account holds no PendingTrainee, or it is outside
    /// the caller's scope. One reason for all three, so a refusal says nothing about another institution's account.
    /// </summary>
    NotPending,

    /// <summary>The registrar already has an active trainee profile: a second Admit from a stale tab (M09).</summary>
    AlreadyAdmitted,

    /// <summary>The registrar holds Trainee beside PendingTrainee, which only admission should grant (T303 review).</summary>
    HoldsTrainee,

    /// <summary>The registrar's account is deactivated (S6, M13): an administrator's lock or an erasure.</summary>
    Deactivated,

    /// <summary>The registrar belongs to no institution, so no curriculum can be offered (M04b).</summary>
    NoInstitution,

    /// <summary>
    /// No curriculum is adopted at the institution for the registrar's invited sub-speciality, or none at all for a
    /// registrar invited to none (M04, check rule 3).
    /// </summary>
    NothingAdopted,

    /// <summary>
    /// The curriculum asked for is not one the admission offers: not the one adopted for the invited sub-speciality (F1),
    /// or, for a registrar invited to none, not one of the institution's active adoptions.
    /// </summary>
    CurriculumNotOffered,

    /// <summary>
    /// The expected completion was left empty, and the date it would be derived as has passed (F4, M15): the field must be
    /// filled. <see cref="TraineeAdmissionRefusedException.DerivedCompletion" /> holds the date.
    /// </summary>
    CompletionPassed
}

/// <summary>
/// An admission's own refusal (T373, flow 11, S9), on the pattern of <c>MsfRefusedException</c>: a reason the page maps to
/// the boards' words (M08, M09, M13, M15), and fixed words of its own for the audit row. Derived from
/// <see cref="InvalidOperationException" />, so nothing that already catches a refusal changes. Until T373 the admission
/// threw bare <see cref="InvalidOperationException" />s, which the profile page printed whatever their source (T329).
/// </summary>
/// <remarks>
/// The message never names the registrar or another institution: <see cref="TraineeAdmissionRefusal.NotPending" /> is the
/// one reason for an unknown id, an account that is not waiting and one outside the caller's scope, so the audit row of a
/// refused admission carries nothing the caller may not read. The page writes the registrar's name from its own read.
/// </remarks>
public sealed class TraineeAdmissionRefusedException : InvalidOperationException
{
    public TraineeAdmissionRefusedException(TraineeAdmissionRefusal reason, DateOnly? derivedCompletion = null)
        : base(TraineeAdmissionRefusals.MessageFor(reason, derivedCompletion))
    {
        Reason = reason;
        DerivedCompletion = derivedCompletion;
    }

    public TraineeAdmissionRefusal Reason { get; }

    /// <summary>The date an empty expected completion would have been derived as: set for <see cref="TraineeAdmissionRefusal.CompletionPassed" />.</summary>
    public DateOnly? DerivedCompletion { get; }
}

/// <summary>The admission's refusals and their fixed words (T373, flow 11, S9).</summary>
public static class TraineeAdmissionRefusals
{
    public const string NotPending = "No registrar waiting for admission has this link.";

    public const string AlreadyAdmitted = "This registrar already has an active trainee profile.";

    public const string Deactivated = "This registrar's account is deactivated. Reactivate it under Users first.";

    /// <summary>M04b's words, as the page says them (R3-0-spec-2, "Words changed").</summary>
    public const string NoInstitution = "This registrar belongs to no institution, so no curriculum can be offered.";

    public const string NothingAdopted =
        "No curriculum is adopted for this registrar's sub-speciality at their institution, so they cannot be admitted.";

    public const string CurriculumNotOffered =
        "A registrar is admitted into the curriculum their institution has adopted for the sub-speciality they were invited to.";

    /// <summary>
    /// F4 and check rule 7: one sentence for the hint and the refusal, beside Expected completion (M15), "Left empty, it would
    /// be 2024-01-15, which has passed. Enter the expected completion."
    /// </summary>
    public static string CompletionPassed(DateOnly derived) =>
        $"Left empty, it would be {derived.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}, which has passed. Enter the expected completion.";

    internal static string MessageFor(TraineeAdmissionRefusal reason, DateOnly? derivedCompletion) => reason switch
    {
        TraineeAdmissionRefusal.NotPending => NotPending,
        TraineeAdmissionRefusal.AlreadyAdmitted => AlreadyAdmitted,
        TraineeAdmissionRefusal.HoldsTrainee => AdmitTraineeCommandHandler.AlreadyHoldsTrainee,
        TraineeAdmissionRefusal.Deactivated => Deactivated,
        TraineeAdmissionRefusal.NoInstitution => NoInstitution,
        TraineeAdmissionRefusal.NothingAdopted => NothingAdopted,
        TraineeAdmissionRefusal.CurriculumNotOffered => CurriculumNotOffered,
        TraineeAdmissionRefusal.CompletionPassed when derivedCompletion is { } derived => CompletionPassed(derived),
        TraineeAdmissionRefusal.CompletionPassed => "Enter the expected completion.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };

    /// <summary>
    /// The validator's (<see cref="FluentValidation.ValidationException" />), a caller refused outright
    /// (<see cref="UnauthorizedAccessException" />: <see cref="TraineeAdministrationRules.DemandTraineeAdministration" />),
    /// or the admission's own (<see cref="TraineeAdmissionRefusedException" />). Anything else is an unexpected failure: a
    /// fixed sentence on the page, and the failure to the log.
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FluentValidation.ValidationException or UnauthorizedAccessException
            or TraineeAdmissionRefusedException;
    }
}
