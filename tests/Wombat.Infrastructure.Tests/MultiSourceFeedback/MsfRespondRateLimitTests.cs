using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Wombat.Infrastructure.MultiSourceFeedback;

namespace Wombat.Infrastructure.Tests.MultiSourceFeedback;

/// <summary>
/// How the respondent's rate limits tell requests apart (T205 review). The limits themselves are exercised over HTTP in
/// the web suite (<c>MsfRespondPageHostingTests</c>) and against the whole app in the integration suite.
/// </summary>
public sealed class MsfRespondRateLimitTests
{
    /// <summary>
    /// A partition's key lives at least a window in memory, and the link is the sender's to make as long as they like.
    /// The key holds a fixed-length hash of it, never the link itself.
    /// </summary>
    [Fact]
    public void ALinksPartitionKey_IsTheSameSizeWhateverTheLink_AndNeverHoldsTheLink()
    {
        var shortLink = LinkPartitionKey("203.0.113.7", "abc");
        var longToken = new string('Z', 8 * 1024);
        var longLink = LinkPartitionKey("203.0.113.7", longToken);

        longLink.Should().HaveLength(shortLink.Length);
        longLink.Should().NotContain("ZZZZ");
        LinkPartitionKey("203.0.113.7", "a-real-looking-token").Should().NotContain("a-real-looking-token");
    }

    [Fact]
    public void ALinksPartitionKey_SeparatesLinks_AndAddresses_ButNotRepeats()
    {
        LinkPartitionKey("203.0.113.7", "one").Should().Be(LinkPartitionKey("203.0.113.7", "one"));
        LinkPartitionKey("203.0.113.7", "one").Should().NotBe(LinkPartitionKey("203.0.113.7", "two"));
        LinkPartitionKey("203.0.113.7", "one").Should().NotBe(LinkPartitionKey("203.0.113.8", "one"));
    }

    /// <summary>
    /// An IPv6 client can move freely within its site's /64, so the per-address limit counts the /64. An IPv4 address,
    /// or one mapped into IPv6 by a dual-stack socket, counts as itself.
    /// </summary>
    [Theory]
    [InlineData("2001:db8:1:2:aaaa:bbbb:cccc:dddd", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:2::1", "2001:db8:1:2::/64")]
    [InlineData("203.0.113.7", "203.0.113.7")]
    [InlineData("::ffff:203.0.113.7", "203.0.113.7")]
    public void AnAddress_IsCountedByTheBlockAClientCanMoveWithin(string address, string counted)
        => MsfRespondRateLimit.AddressOf(ContextFrom(address, token: null)).Should().Be(counted);

    [Fact]
    public void NoAddress_IsCountedTogether()
        => MsfRespondRateLimit.AddressOf(new DefaultHttpContext()).Should().Be("unknown-ip");

    private static string LinkPartitionKey(string address, string token)
        => MsfRespondRateLimit.LinkPartitionKey(ContextFrom(address, token));

    private static DefaultHttpContext ContextFrom(string address, string? token)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        if (token is not null)
        {
            context.Request.QueryString = QueryString.Create("token", token);
        }

        return context;
    }
}
