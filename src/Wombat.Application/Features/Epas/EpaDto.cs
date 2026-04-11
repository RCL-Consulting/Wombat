namespace Wombat.Application.Features.Epas;

public sealed record EpaDto(
    int Id,
    int SubSpecialityId,
    string SubSpecialityName,
    string Code,
    string Title,
    string? Description,
    string? RequiredKnowledgeSkills,
    bool IsActive,
    DateTime CreatedOn);
