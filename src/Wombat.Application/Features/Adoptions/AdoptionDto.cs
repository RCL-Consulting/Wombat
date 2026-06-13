namespace Wombat.Application.Features.Adoptions;

/// <summary>
/// A training institution's adoption of a national curriculum version for one discipline (T091 phase 4).
/// </summary>
public sealed record AdoptionDto(
    int Id,
    int InstitutionId,
    int CurriculumId,
    string CurriculumName,
    string CurriculumVersion,
    int SubSpecialityId,
    string SubSpecialityName,
    string SpecialityName,
    string CollegeName,
    DateOnly AdoptedOn,
    bool IsActive);
