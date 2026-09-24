using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Wombat.Infrastructure.MultiSourceFeedback;

/// <summary>
/// The rate limits on a respondent's link: ten requests a minute for one link from one address, and sixty a minute from
/// one address whatever the links. (T021, T205)
/// </summary>
/// <remarks>
/// <para>
/// One definition for both places a link is answered: the web app's respondent page (<c>/msf/respond</c>, where every
/// respondent lands) and the Api's integration endpoint. Each host registers it (<see cref="AddMsfRespondPolicy" />) and
/// puts <see cref="PolicyName" /> on its respond route, so the two cannot drift apart.
/// </para>
/// <para>
/// Per link AND address, as the Api always was. By address alone at ten, one hospital's respondents behind one NAT
/// address would share ten requests a minute between them, and the invitations of a campaign are sent together. Loading
/// the page and submitting it is two requests, so a respondent who corrects a refused submission a few times stays well
/// inside the limit.
/// </para>
/// <para>
/// Per address as well, because the per-link limit alone does not limit a client: every made-up link gets a fresh ten.
/// Until T163 every request through one also loaded and hashed every invitation there was; now it reads one row by an
/// index, or nothing when the link could not be a token at all. Sixty a minute is thirty respondents behind one address
/// loading and submitting in the same minute. It applies to a respond route only: the rest of the host is untouched.
/// </para>
/// <para>
/// The link is a partition's key as a hash, not as sent. A key lives at least a window in memory, and a link can be
/// several kilobytes of anything.
/// </para>
/// </remarks>
public static class MsfRespondRateLimit
{
    public const string PolicyName = "msf-respond";

    /// <summary>Requests a minute for one link from one address.</summary>
    public const int PermitLimit = 10;

    /// <summary>Requests a minute from one address, to any respond route, whatever the links.</summary>
    public const int AddressPermitLimit = 60;

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Registers the per-link policy under <see cref="PolicyName" />, and the per-address limit on every route that names
    /// it, chained after any global limiter the host already has.
    /// </summary>
    public static RateLimiterOptions AddMsfRespondPolicy(this RateLimiterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var perAddress = PartitionedRateLimiter.Create<HttpContext, string>(AddressPartition);
        options.GlobalLimiter = options.GlobalLimiter is { } existing
            ? PartitionedRateLimiter.CreateChained(existing, perAddress)
            : perAddress;

        return options.AddPolicy(PolicyName, LinkPartition);
    }

    /// <summary>Whether this request is to a respond route, so that a refusal of it was one of these limits'.</summary>
    public static bool Limits(HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == PolicyName;

    /// <summary>
    /// The per-link partition's key: the client's address and a hash of the link's token. Never the token itself: its
    /// length is the sender's to choose.
    /// </summary>
    public static string LinkPartitionKey(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var token = context.Request.Query["token"].ToString();
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        return string.Create(CultureInfo.InvariantCulture, $"{AddressOf(context)}:{tokenHash}");
    }

    /// <summary>
    /// The client as the per-address limit counts it: an IPv4 address as it is, an IPv6 address by its /64, the block one
    /// site is given (an IPv6 client can move within it freely).
    /// </summary>
    public static string AddressOf(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var address = context.Connection.RemoteIpAddress;
        if (address is null)
        {
            return "unknown-ip";
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

    private static RateLimitPartition<string> LinkPartition(HttpContext context)
        => RateLimitPartition.GetFixedWindowLimiter(LinkPartitionKey(context), _ => FixedWindow(PermitLimit));

    private static RateLimitPartition<string> AddressPartition(HttpContext context)
        => Limits(context)
            ? RateLimitPartition.GetFixedWindowLimiter(AddressOf(context), _ => FixedWindow(AddressPermitLimit))
            : RateLimitPartition.GetNoLimiter(string.Empty);

    private static FixedWindowRateLimiterOptions FixedWindow(int permitLimit) => new()
    {
        PermitLimit = permitLimit,
        Window = Window,
        QueueLimit = 0
    };
}
