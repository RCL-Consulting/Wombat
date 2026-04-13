namespace Wombat.Application.Common.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string TextBody,
    string? Cc = null,
    IReadOnlyList<string>? Tags = null);
