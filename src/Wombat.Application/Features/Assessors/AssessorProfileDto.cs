using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

/// <summary>
/// One assessor profile: training only (T376, flow 12, F4). The profile's own speciality and sub-speciality were dropped;
/// an assessor's discipline is the person's scope, read from the user (Q12).
/// </summary>
public sealed record AssessorProfileDto(
    int Id,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Qualifications,
    int InstitutionId,
    string InstitutionName,
    AssessorTrainingStatus TrainingStatus,
    DateOnly? TrainingCompletedOn);
