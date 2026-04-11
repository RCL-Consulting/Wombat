namespace Wombat.Domain.Identity;

public sealed class WombatUser
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int? InstitutionId { get; init; }
    public IReadOnlyCollection<int> SpecialityIds { get; init; } = [];
    public IReadOnlyCollection<int> SubSpecialityIds { get; init; } = [];
}
