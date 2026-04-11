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
    bool IsActive);

public sealed record PendingTraineeDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds);
