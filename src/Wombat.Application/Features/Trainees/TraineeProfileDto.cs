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
    /// rule <see cref="UpdateTraineeProfileCommand" /> refuses a save by, read from the fields this record carries.
    /// </summary>
    public bool IsEnded => !IsActive && (CompletedOn ?? DeactivatedOn) is not null;
}

public sealed record PendingTraineeDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds);
