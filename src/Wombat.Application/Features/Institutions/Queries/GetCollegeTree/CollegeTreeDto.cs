namespace Wombat.Application.Features.Institutions.Queries.GetCollegeTree;

/// <summary>A College with its specialities, by name (T394, flow 16, Q1).</summary>
public sealed record CollegeTreeDto(
    int Id,
    string Name,
    string ShortCode,
    IReadOnlyList<CollegeTreeSpecialityDto> Specialities);

/// <summary>A speciality on the tree, with its sub-specialities by name.</summary>
public sealed record CollegeTreeSpecialityDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<CollegeTreeSubSpecialityDto> SubSpecialities);

/// <summary>
/// A sub-speciality on the tree: its default ladder by id and by name (null when it has none), read in the tree's one
/// query, where <see cref="SubSpecialityDto" /> carries only the id.
/// </summary>
public sealed record CollegeTreeSubSpecialityDto(
    int Id,
    int SpecialityId,
    string Name,
    string? Description,
    bool IsActive,
    int? DefaultEntrustmentScaleId,
    string? DefaultEntrustmentScaleName);
