namespace Wombat.Application.Audit;

/// <summary>
/// Provides actor context (current user, IP, user agent) for audit entries.
/// The Infrastructure layer registers an HTTP-context-backed implementation.
/// Returns nulls safely when there is no HTTP context (e.g. background jobs).
/// </summary>
public interface IAuditContextProvider
{
    string? UserId { get; }
    string? UserDisplay { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
