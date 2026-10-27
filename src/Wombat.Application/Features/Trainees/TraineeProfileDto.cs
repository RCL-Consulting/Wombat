using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Users;

namespace Wombat.Application.Features.Trainees;

public sealed record TraineeProfileDto(
    int Id,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int CurriculumId,
    string CurriculumName,
    string CurriculumVersion,
    int SpecialityId,
    string SpecialityName,
    int SubSpecialityId,
    string SubSpecialityName,
    DateOnly ProgrammeStartDate,
    DateOnly ExpectedCompletionDate,
    bool IsActive,
    DateOnly? CompletedOn = null,
    DateOnly? DeactivatedOn = null)
{
    /// <summary>
    /// Whether the programme has ended and the record is archived, read-only (T305): <c>TraineeProfile.IsEnded</c>, the
    /// rule <see cref="UpdateTraineeProfileCommand" /> refuses a save by. An inactive profile is an ended one.
    /// </summary>
    public bool IsEnded => !IsActive;

    /// <summary>
    /// The training year on today's South African calendar (T373, flow 11; T306's admission half): Current trainees'
    /// "Training year 4" under the programme start, from <see cref="TraineeProfile.StageOn" />. Null before the programme
    /// starts, and where the reader that built the DTO does not say.
    /// </summary>
    public int? TrainingYear { get; init; }

    /// <summary>The profile's institution by name, which the Administrator's Current trainees shows under each name (T373, T06).</summary>
    public string? InstitutionName { get; init; }

    /// <summary>
    /// Whether the caller may export the trainee's portfolio (T379, flow 13, F1): the export command's own check,
    /// <c>TraineeScopeResolver.MayReadAsync</c> on <see cref="UserId" />, so the profile's header offers "Export ‹name›'s
    /// portfolio" only where the export would not be refused (an ended profile whose person's preferred profile is now at
    /// another institution, say). Filled by <c>GetTraineeProfileByIdQuery</c>; false from the list reads.
    /// </summary>
    public bool MayExport { get; init; }

    /// <summary>
    /// How the programme ended, on which day and in which training year (T379, flow 13, S4: the end's kind, day and training
    /// year at the end, read once through <c>EndedProgrammes.Of</c>); null while it runs, and for an end Wombat did not
    /// record a day for. Filled by <c>GetTraineeProfileByIdQuery</c> and <c>ListEndedProfilesQuery</c>'s rows.
    /// </summary>
    public EndedProgrammeDto? Ended { get; init; }

    /// <summary>
    /// The trainee's last ratified committee decision (T379, flow 13, Q13, S4), which the end section names before the last
    /// day and which gates nothing; null when none is ratified. Filled by <c>GetTraineeProfileByIdQuery</c> for a running
    /// programme only.
    /// </summary>
    public LastRatifiedDecisionDto? LastRatifiedDecision { get; init; }

    /// <summary>
    /// The account the ended record's account sentence is about (T379, flow 13, F5, S5); null while the programme runs, and
    /// for an erased person, who has no account and whose record shows no sentence.
    /// </summary>
    public TraineeAccountDto? Account { get; init; }

    /// <summary>
    /// The person was erased (T258): the profile belongs to a pseudonym no account holds, so <see cref="FirstName" /> is the
    /// pseudonym, <see cref="LastName" /> and <see cref="Email" /> are empty, and the record shows no account (T379, F7, S5).
    /// </summary>
    public bool IsErased { get; init; }

    /// <summary>The trainee's name as the pages print it: "Lerato Molefe", or an erased person's pseudonym.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// The account of a person whose programme has ended, as the ended record's account sentence reads it (T379, flow 13, F5,
/// S5): "Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the
/// STAR certificates.", "‹name› now holds Assessor. …", or only the user page's link when the account is locked.
/// </summary>
/// <param name="Roles">The role keys the account holds now; the page names them by <c>WombatRoleLabels.For</c>.</param>
/// <param name="IsLocked">
/// Deactivated by an administrator (<c>UserIdentityDetails.IsDeactivated</c>; T268: a locked account is not current), not
/// a brute-force lockout that lifts itself after minutes. The record then says nothing about what the person can do.
/// </param>
/// <param name="AnyStarIssued">Any ratified STAR decision, in force or not (<c>RecordStars</c>, F6): the sentence's STAR clause.</param>
public sealed record TraineeAccountDto(IReadOnlyList<string> Roles, bool IsLocked, bool AnyStarIssued);

public sealed record PendingTraineeDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds)
{
    /// <summary>The registrar's institution by name; null for one of none (T373, flow 11, check rule 6).</summary>
    public string? InstitutionName { get; init; }

    /// <summary>
    /// Waiting's Invited as (T373, flow 11, build-contracts § 3): the invited scope in the pages' words, "Paediatrics /
    /// Paediatrics" (<c>InvitationScopeWords.Disciplines</c>); null for a registrar invited to none (S5), which the page
    /// writes as "—".
    /// </summary>
    public string? InvitedAs { get; init; }

    /// <summary>
    /// Waiting's Registered: the used invitation's day on the South African calendar (§ 3); null for a registrar who
    /// registered from no invitation (an SSO registrar).
    /// </summary>
    public DateOnly? RegisteredOn { get; init; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
