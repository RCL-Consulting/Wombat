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
}

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
