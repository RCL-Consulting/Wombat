namespace Wombat.Application.Features.Institutions;

public sealed record InstitutionDto(
    int Id,
    string Name,
    string ShortCode,
    string? ContactEmail,
    bool IsActive,
    DateTime CreatedOn);
