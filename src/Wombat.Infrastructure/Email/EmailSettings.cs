namespace Wombat.Infrastructure.Email;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string SmtpUser { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "Wombat";
    public bool UseSsl { get; init; } = false;
    public int TimeoutSeconds { get; init; } = 30;
}
