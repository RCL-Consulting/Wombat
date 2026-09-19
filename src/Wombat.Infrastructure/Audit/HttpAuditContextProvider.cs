using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wombat.Application.Audit;
using Wombat.Application.Common.Extensions;

namespace Wombat.Infrastructure.Audit;

/// <summary>
/// Reads actor context from the current HTTP request.
/// Returns nulls gracefully when there is no active HTTP context
/// (e.g. background job execution via ScheduledJobHost).
/// </summary>
public sealed class HttpAuditContextProvider : IAuditContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private int? _declaredInstitutionId;

    public HttpAuditContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserDisplay =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public string? IpAddress =>
        TruncateIp(_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress);

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()
            is { Length: > 0 } ua ? ua : null;

    /// <summary>
    /// Read from the same institution_id claim that WombatUserClaimsPrincipalFactory issues and that
    /// every scope-aware handler filters on, so an audit row lands in exactly the scope the action
    /// itself was performed under. Null for a global Administrator or a background job — deliberately,
    /// since their actions may span institutions and must not be attributed to one. Null too for an
    /// anonymous caller, which is why <see cref="DeclareInstitution"/> exists and takes precedence. (T101)
    /// </summary>
    public int? InstitutionId =>
        _declaredInstitutionId ?? _httpContextAccessor.HttpContext?.User.GetInstitutionId();

    /// <summary>
    /// The handler's own answer beats the claim: it is resolved from the record the command acts on,
    /// while the principal may be anonymous (an AllowAnonymous endpoint) or stale (a leftover auth
    /// cookie belonging to someone else). Registered scoped, and ScopedSender opens a scope per Send,
    /// so this lives for one dispatch and cannot bleed into the next command. (T101)
    /// </summary>
    public void DeclareInstitution(int institutionId) => _declaredInstitutionId = institutionId;

    private static string? TruncateIp(IPAddress? address)
    {
        if (address is null) return null;

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // Truncate IPv4 to /24 (zero last octet)
            var bytes = address.GetAddressBytes();
            bytes[3] = 0;
            return new IPAddress(bytes).ToString() + "/24";
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // Truncate IPv6 to /48 (zero last 10 bytes)
            var bytes = address.GetAddressBytes();
            for (int i = 6; i < 16; i++) bytes[i] = 0;
            return new IPAddress(bytes).ToString() + "/48";
        }

        return address.ToString();
    }
}
