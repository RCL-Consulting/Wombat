using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wombat.Application.Audit;

namespace Wombat.Infrastructure.Audit;

/// <summary>
/// Reads actor context from the current HTTP request.
/// Returns nulls gracefully when there is no active HTTP context
/// (e.g. background job execution via ScheduledJobHost).
/// </summary>
public sealed class HttpAuditContextProvider : IAuditContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

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
