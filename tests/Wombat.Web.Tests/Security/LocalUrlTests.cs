using FluentAssertions;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// <see cref="LocalUrl" />: a path on this site passes, as <c>LocalRedirect</c> judges one, in a form a response header can
/// carry; anything else is dropped (T335, flow 01; the review of the t335 branch). The integration suite tries every caller
/// in the shape it takes the address (<c>SignInReturnAddressFlowTests</c>, <c>ActingRoleFlowTests</c>).
/// </summary>
public sealed class LocalUrlTests
{
    [Theory]
    [InlineData("/", "/")]
    [InlineData("/activities/inbox?page=2", "/activities/inbox?page=2")]
    [InlineData("/activities/inbox?q=a%20b#top", "/activities/inbox?q=a%20b#top")]
    [InlineData("/a b", "/a b")]
    public void APathOnThisSite_InAscii_PassesAsItIs(string url, string expected)
        => LocalUrl.OrNull(url).Should().Be(expected);

    // Kestrel refuses a header value outside printable ASCII, so LocalRedirect of such an address answered 500. Encoded as
    // the browser itself sends it, it opens the same page.
    [Theory]
    [InlineData("/é", "/%C3%A9")]
    [InlineData("/admin/users?search=José", "/admin/users?search=Jos%C3%A9")]
    [InlineData("/€/x", "/%E2%82%AC/x")]
    [InlineData("/q?\U0001F989", "/q?%F0%9F%A6%89")]
    [InlineData("/ ", "/%C2%A0")]
    [InlineData("/%C3%A9é", "/%C3%A9%C3%A9")]
    public void ACharacterAboveU007E_IsPercentEncodedAsUtf8(string url, string expected)
    {
        var local = LocalUrl.OrNull(url);

        local.Should().Be(expected, "what was already percent-encoded stays as it was, and only the rest is encoded");
        local.Should().MatchRegex("^[\\x20-\\x7E]*$", "a Location header Kestrel will send");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("activities")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("https://evil.example/")]
    [InlineData("/\t/evil.example")]
    [InlineData("/a\u0085b")]
    [InlineData("/a\u007Fb")]
    public void AnythingElse_IsDropped(string? url) => LocalUrl.OrNull(url).Should().BeNull();

    [Fact]
    public void HalfASurrogatePair_IsDropped_NotEncodedAsSomethingElse()
        => LocalUrl.OrNull("/a\ud83eb").Should().BeNull("a broken character names no page");

    [Fact]
    public void AFullWidthSolidusSecond_StaysAPath()
        => LocalUrl.OrNull("/／evil.example").Should().Be("/%EF%BC%8Fevil.example",
            "encoded, the browser reads it as a path segment, never as a second slash");
}
