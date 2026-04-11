namespace Wombat.Application.Features.Assessors;

public sealed record AssessorProfileDto(
    int Id,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string Qualifications,
    int InstitutionId,
    string InstitutionName,
    int? SpecialityId,
    string? SpecialityName,
    int? SubSpecialityId,
    string? SubSpecialityName);
