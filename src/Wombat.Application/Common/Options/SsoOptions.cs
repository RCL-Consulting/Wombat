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

    /// <summary>
    /// The claim by which the provider asserts that its <c>email</c> claim is verified. OIDC Core § 5.1 names it
    /// <c>email_verified</c>. Microsoft Entra ID does not emit that claim: configure its <c>xms_edov</c> optional claim and
    /// name it here. An email the provider does not assert as verified (<c>true</c> in any case, or <c>1</c>) is never
    /// written to an account, provisions none, and is matched to none for linking (T155).
    /// </summary>
    public string EmailVerifiedClaim { get; set; } = "email_verified";
    public bool EnableFederatedLogout { get; set; }
}
