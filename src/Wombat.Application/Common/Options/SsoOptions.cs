namespace Wombat.Application.Common.Options;

public sealed class SsoOptions
{
    public const string SectionName = "Sso";

    public List<SsoProviderOptions> Providers { get; set; } = [];
}

public sealed class SsoProviderOptions
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = ["openid", "profile", "email"];
    public string GroupsClaim { get; set; } = "groups";
    public bool EnableFederatedLogout { get; set; }
}
