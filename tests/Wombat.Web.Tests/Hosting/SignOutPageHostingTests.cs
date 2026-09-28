using System.Net;
using FluentAssertions;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The sign-out page as a browser receives it (T339, flow 02; the round 2 review's B1 and A12): both addresses draw the
/// one static page with its own form and token, posting beside it; neither GET signs anyone out; and a visitor who is not
/// signed in is sent to the sign-in page with no return address.
/// </summary>
/// <remarks>
/// The post itself, with and without its token, runs through the whole of <c>Program.cs</c> in the integration suite
/// (<c>Hosting/SignOutFlowTests</c>): this host maps no endpoint of Program.cs's own.
/// </remarks>
public sealed class SignOutPageHostingTests
{
    [Theory]
    [InlineData("/account/logout")]
    [InlineData("/account/logout-confirm")]
    public async Task BothAddresses_DrawTheSignOutPage_StaticWithItsOwnFormAndToken(string address)
    {
        await using var host = await AppTestHost.StartAsync(SignedInVisitor.Register);

        var (response, html, document) = await host.LoadAsync(address);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        document.Title.Should().Be("Sign out · Wombat");
        document.QuerySelectorAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Sign out");
        document.QuerySelector(".account-form-container p")!.TextContent.Trim().Should().Be(
            "You are signed in as Signed-in visitor. Signing out ends your session in this browser.");

        var form = document.QuerySelectorAll("form").Should().ContainSingle().Which;
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be(SignOutOutcome.SubmitPath).And.Be("/account/logout/submit");
        form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value").Should().NotBeNullOrEmpty(
            "the post requires the token (E6), so the page draws a fresh one");
        form.QuerySelector("button[type=submit]")!.TextContent.Trim().Should().Be("Sign out");
        form.QuerySelector("a")!.TextContent.Trim().Should().Be("Cancel");

        ServerComponentMarker.IsIn(html).Should().BeFalse("the page is static: no circuit, and the form is an HTTP post");
        document.QuerySelector("nav").Should().BeNull("the anonymous card: no navigation");
    }

    [Theory]
    [InlineData("", "/")]
    [InlineData("?returnUrl=%2Fmsf%2Fcampaigns", "/msf/campaigns")]
    [InlineData("?returnUrl=%2F%2Fevil.example", "/")]
    [InlineData("?returnUrl=https%3A%2F%2Fevil.example%2F", "/")]
    public async Task Cancel_GoesBackToALocalReturnAddress_ElseHome(string query, string cancel)
    {
        await using var host = await AppTestHost.StartAsync(SignedInVisitor.Register);

        var (_, _, document) = await host.LoadAsync($"{SignOutOutcome.PagePath}{query}");

        document.QuerySelector("form a")!.GetAttribute("href").Should().Be(cancel);
    }

    [Theory]
    [InlineData("/account/logout")]
    [InlineData("/account/logout-confirm")]
    [InlineData("/account/logout?returnUrl=%2Fprogress")]
    public async Task AVisitorWhoIsNotSignedIn_IsSentToSignIn_WithNoReturnAddress(string address)
    {
        await using var host = await AppTestHost.StartAsync();

        using var response = await host.Client.GetAsync(address);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location!;
        (location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString).Should().Be("/account/login",
            "signing in must not come back here to sign out");
    }
}
