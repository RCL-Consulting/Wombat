namespace Wombat.Application.Common.Options;

public sealed class WombatOptions
{
    public const string SectionName = "Wombat";

    public bool AllowSelfRegistration { get; set; }
    public string? BaseUrl { get; set; }
    public string? MsfRespondUrl { get; set; }
    public string? SeedAdminEmail { get; set; }
    public string? SeedAdminPassword { get; set; }
}
