namespace Wombat.Infrastructure.Identity;

public sealed class WombatIdentityUserSpecialityScope
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SpecialityId { get; set; }
    public WombatIdentityUser User { get; set; } = null!;
}
