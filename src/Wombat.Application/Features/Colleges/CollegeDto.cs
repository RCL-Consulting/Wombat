namespace Wombat.Application.Features.Colleges;

/// <summary>
/// A national governing body (a CMSA constituent College) that owns a discipline's EPAs and curricula.
/// Managed by global Administrators; a CollegeAdmin is scoped to one College's catalogue. (T091.)
/// </summary>
public sealed record CollegeDto(
    int Id,
    string Name,
    string ShortCode,
    string? Description,
    bool IsActive,
    DateTime CreatedOn);
