using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;

namespace Wombat.Infrastructure.Http;

/// <summary>
/// The client a per-address limit counts: the MSF respondent limits (T205) and the sign-in throttle (T156).
/// </summary>
/// <remarks>
/// The address is the connection's, after the forwarded-headers middleware has put the client's in its place. That
/// middleware believes <c>X-Forwarded-For</c> from the loopback proxy only (Caddy), so a client cannot name an address of
/// its choosing here (T156).
/// </remarks>
public static class ClientAddress
{
    /// <summary>What a request without an address is counted as. They are all counted together.</summary>
    public const string Unknown = "unknown-ip";

    /// <summary>
    /// An IPv4 address as it is (one mapped into IPv6 by a dual-stack socket too), and an IPv6 address by its /64: the
    /// block one site is given, which a client can move within freely, so each of its addresses would otherwise be counted
    /// as a client of its own.
    /// </summary>
    public static string Of(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var address = context.Connection.RemoteIpAddress;
        if (address is null)
        {
            return Unknown;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        var bytes = address.GetAddressBytes();
        Array.Clear(bytes, 8, 8);
        return new IPAddress(bytes) + "/64";
    }
}
