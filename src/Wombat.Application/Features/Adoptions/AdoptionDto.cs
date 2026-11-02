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
    bool IsActive)
{
    /// <summary>The version this adoption superseded, or null for a first adoption of the discipline (T392, Q6).</summary>
    public int? SupersededCurriculumId { get; init; }

    /// <summary>The superseded version's name; null for a first adoption.</summary>
    public string? SupersededName { get; init; }

    /// <summary>The superseded version's version ("11.1"); null for a first adoption.</summary>
    public string? SupersededVersion { get; init; }

    /// <summary>
    /// The institution's ACTIVE trainee profiles still on the superseded version, which stay on it until each is moved
    /// (T392, Q6, S1); 0 for a first adoption.
    /// </summary>
    public int SupersededRegistrarCount { get; init; }
}
