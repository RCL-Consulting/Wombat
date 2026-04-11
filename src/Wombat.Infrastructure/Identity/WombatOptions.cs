namespace Wombat.Infrastructure.Identity;

public sealed class WombatOptions
{
    public const string SectionName = "Wombat";

    public bool AllowSelfRegistration { get; set; }
    public string? SeedAdminEmail { get; set; }
    public string? SeedAdminPassword { get; set; }
}
