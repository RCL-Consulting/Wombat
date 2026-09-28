extern alias WombatWeb;

using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Security;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T339, flow 02 (the round 2 review's B1 and A12, E6): signing out, through the whole of <c>Wombat.Web</c>'s
/// <c>Program.cs</c>. The page answers GET at both its addresses and signs no one out; its form posts to
/// <c>/account/logout/submit</c>, which requires the antiforgery token of a signed-in request and lands on the sign-in page
/// saying so; a post that arrives not signed in, as another site's does, is only redirected and deletes no cookie; a POST
/// to either page address signs no one out; and a visitor who is not signed in is sent to sign in with no return address.
/// </summary>
/// <remarks>
/// Until T339 the post went to <c>/account/logout</c> and needed no token (the endpoint bound no form, so the framework
/// checked none): any site could sign a Wombat user out with a form of its own, and signing out landed on a sign-in page
/// that said nothing.
/// </remarks>
public sealed class SignOutFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string SignedInPage = "/account/profile";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public SignOutFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData("/account/logout")]
    [InlineData("/account/logout-confirm")]
    public async Task EachAddress_DrawsTheSignOutPage_WhoseFormSignsOut_AndSaysSo(string address)
    {
        using var browser = await SignedInBrowserAsync();

        var page = await LoadAsync(browser, address);
        (await SignedInAsync(browser)).Should().BeTrue("loading the page signs no one out");

        var form = page.QuerySelector("form[method=post]")!;
        form.GetAttribute("action").Should().Be("/account/logout/submit");
        using var signOut = await browser.PostAsync(form.GetAttribute("action"), new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", TokenOf(page))
        ]));

        signOut.StatusCode.Should().Be(HttpStatusCode.Redirect);
        signOut.Headers.Location!.ToString().Should().Be("/account/login?error=SignedOut").And.Be(SignOutOutcome.SignedOutUrl);
        (await SignedInAsync(browser)).Should().BeFalse("signed out");

        var signInPage = await LoadAsync(browser, signOut.Headers.Location.ToString());
        signInPage.QuerySelectorAll(".alert").Should().ContainSingle().Which.TextContent.Trim().Should().Be("You have signed out.");
    }

    [Fact]
    public async Task TheShellsSignOut_PostsToTheSameEndpoint_WithItsToken()
    {
        using var browser = await SignedInBrowserAsync();

        var page = await LoadAsync(browser, SignedInPage);
        var form = page.QuerySelector(".account-row form.sign-out")!;
        form.GetAttribute("action").Should().Be(SignOutOutcome.SubmitPath);

        using var signOut = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!)
        ]));

        signOut.Headers.Location!.ToString().Should().Be(SignOutOutcome.SignedOutUrl);
        (await SignedInAsync(browser)).Should().BeFalse();
    }

    /// <summary>
    /// E6, for a post that carries the sign-in cookie but no token. Another site's form does not reach here: the cookie is
    /// SameSite=Lax, so a browser leaves it off a cross-site POST, and such a post arrives not signed in
    /// (<see cref="ACrossSitePost_ArrivesWithoutTheCookie_IsOnlyRedirected_AndDeletesNoCookie" />). This is what is left: a
    /// page on this site, or a browser that sends the cookie anyway, which has the cookie but could not read a token.
    /// </summary>
    [Fact]
    public async Task APostWithoutTheToken_IsRefused_AndTheSessionGoesOn()
    {
        using var browser = await SignedInBrowserAsync();

        using var forged = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent([]));

        forged.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await SignedInAsync(browser)).Should().BeTrue("a post without the token signs no one out");

        using var wrongToken = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", "not-a-token")
        ]));
        wrongToken.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await SignedInAsync(browser)).Should().BeTrue();
    }

    /// <summary>
    /// The page's own addresses answer POST as well as GET, as every Razor component's does (B1), which is why the post
    /// moved beside them. A post to either, with a good token, signs no one out.
    /// </summary>
    [Theory]
    [InlineData("/account/logout")]
    [InlineData("/account/logout-confirm")]
    public async Task APostToAPageAddress_SignsNoOneOut(string address)
    {
        using var browser = await SignedInBrowserAsync();
        var page = await LoadAsync(browser, address);

        using var post = await browser.PostAsync(address, new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", TokenOf(page))
        ]));

        post.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "a Razor component's address answers a form post it has no handler for with 400, and signs no one out");
        (await SignedInAsync(browser)).Should().BeTrue();
    }

    [Theory]
    [InlineData("/account/logout")]
    [InlineData("/account/logout-confirm")]
    [InlineData("/account/logout?returnUrl=%2Fprogress")]
    public async Task AVisitorWhoIsNotSignedIn_IsSentToSignIn_WithNoReturnAddress(string address)
    {
        using var browser = NewBrowser();

        using var response = await browser.GetAsync(address);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        PathAndQuery(response.Headers.Location!).Should().Be("/account/login", "signing in must not come back here to sign out");
    }

    /// <summary>
    /// A tab whose session has already ended has nothing to sign out, and its token names someone the request no longer
    /// does: it lands where signing out lands, not on a bare 400.
    /// </summary>
    /// <remarks>
    /// The cross-site case is <see cref="ACrossSitePost_ArrivesWithoutTheCookie_IsOnlyRedirected_AndDeletesNoCookie" />.
    /// </remarks>
    [Fact]
    public async Task APostFromABrowserThatIsNotSignedIn_LandsWhereSigningOutLands()
    {
        using var browser = NewBrowser();

        using var post = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent([]));

        post.StatusCode.Should().Be(HttpStatusCode.Redirect);
        post.Headers.Location!.ToString().Should().Be(SignOutOutcome.SignedOutUrl);
    }

    /// <summary>
    /// E6, the case that matters (the review of the t339 branch). Another site's auto-submitted form reaches the endpoint
    /// without the sign-in cookie, which is SameSite=Lax, so it arrives not signed in. It is only redirected: no
    /// Set-Cookie at all. Until the review that branch signed out, and a browser applies a response's deletions whether or
    /// not it sent the cookies: any site could sign a Wombat user out, or end a link in progress by deleting the external
    /// cookie, with no token.
    /// </summary>
    [Fact]
    public async Task ACrossSitePost_ArrivesWithoutTheCookie_IsOnlyRedirected_AndDeletesNoCookie()
    {
        // The victim's browser, posting another site's form: without the sign-in cookie, as a browser sends it cross-site.
        using var otherSite = NewBrowser();

        using var forged = await otherSite.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent([]));

        forged.StatusCode.Should().Be(HttpStatusCode.Redirect);
        forged.Headers.Location!.ToString().Should().Be(SignOutOutcome.SignedOutUrl);
        (forged.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : [])
            .Should().BeEmpty("the browser applies a deletion to the cookies it holds, whether or not it sent them");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private HttpClient NewBrowser() => _host.NewBrowser($"10.239.{Interlocked.Increment(ref _nextSubnet)}.7");

    private async Task<HttpClient> SignedInBrowserAsync()
    {
        var email = $"sign-out-{Guid.NewGuid():N}@example.test";
        await _host.CreateAssessorAsync(email);
        var browser = NewBrowser();
        await _host.SignInAsync(browser, email);
        return browser;
    }

    private static async Task<bool> SignedInAsync(HttpClient browser)
    {
        using var response = await browser.GetAsync(SignedInPage);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "guard: refused by being sent to sign in");
        PathAndQuery(response.Headers.Location!).Should().StartWith("/account/login");
        return false;
    }

    private static async Task<IHtmlDocument> LoadAsync(HttpClient browser, string address)
    {
        using var response = await browser.GetAsync(address);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: {address} loads");
        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
    }

    private static string TokenOf(IHtmlDocument page)
        => page.QuerySelector("form[method=post] input[name=__RequestVerificationToken]")!.GetAttribute("value")!;

    private static string PathAndQuery(Uri location) => location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
}
