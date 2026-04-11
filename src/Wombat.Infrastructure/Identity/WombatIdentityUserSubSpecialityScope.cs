namespace Wombat.Infrastructure.Identity;

public sealed class WombatIdentityUserSubSpecialityScope
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SubSpecialityId { get; set; }
    public WombatIdentityUser User { get; set; } = null!;
}
