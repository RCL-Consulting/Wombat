namespace Wombat.Application.Features.Forms;

public sealed record AssessmentFormDto(
    int Id,
    string Name,
    int? InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    int ScaleId,
    string ScaleName,
    bool CanDelete,
    bool IsActive,
    IReadOnlyList<FormCriterionDto> Criteria,
    IReadOnlyList<FormEpaLinkDto> EpaLinks);

public sealed record FormCriterionDto(int Id, int Order, string Prompt, string? HelpText, bool IsRequired);

public sealed record FormEpaLinkDto(int Id, int EpaId, string EpaCode, string EpaTitle);
