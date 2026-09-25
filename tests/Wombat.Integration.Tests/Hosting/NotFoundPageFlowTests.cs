using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Wombat.Integration.Tests.MultiSourceFeedback;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T233: a signed-in user who opens an address no page claims gets the app's own "Page not found" page, with status
/// 404, and nothing else the pipeline answers changes.
/// </summary>
/// <remarks>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c>, hosted on a schema of its own by the MSF respondent page's fixture
/// (<see cref="MsfRespondPageFlowTests.WebHost" />): the fallback policy, the CSP middleware, the rate limiters, the
/// status-code-pages rerun and the Razor-components endpoint, with the static-asset endpoints a publish maps (no
/// development reloader, whose catch-all fallback would answer a POST to an unknown address with a 405). A browser is a
/// client with its own cookies and its own address, so its sign-in and the respondent page's throttles are its own.
/// </para>
/// <para>
/// Until T233 the unknown address was a 404 with no body, and the browser showed its own error page with no way back.
/// The rerun is for a browser's page load only: a 404, a GET, a request that accepts HTML, an address that is not a
/// file. Each case below that must stay as it was differs from the page load in at least one of those, or is a response
/// that already has a body; the doc on each says which check keeps it as it was.
/// </para>
/// </remarks>
public sealed class NotFoundPageFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    /// <summary>What Chrome sends when it loads a page from the address bar or a link.</summary>
    private const string PageLoadAccept =
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8," +
        "application/signed-exchange;v=b3;q=0.7";

    private const string NotFoundHeading = "Page not found";

    /// <summary>The route of <c>Pages/NotFound.razor</c>.</summary>
    private const string NotFoundPagePath = "/not-found";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public NotFoundPageFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Theory]
    [InlineData("/no-such-page")]
    [InlineData("/admin/forms")] // retired by T145, where this was found
    [InlineData("/activities/no/such/page?from=a-bookmark")]
    [InlineData("/placeholder/activities")] // a stub T178 retired: the page it stood in for exists
    [InlineData("/placeholder/no-such-feature")] // the placeholder's route matches, but it stands in for nothing
    public async Task ASignedInUser_OpeningAnUnknownAddress_GetsTheAppsPageNotFoundPage_WithStatus404(string address)
    {
        using var browser = NewBrowser();
        var email = await SignInAsANewAssessorAsync(browser);

        using var response = await LoadPageAsync(browser, address);
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "the address names nothing, and the page says so");
        response.Headers.Location.Should().BeNull("the page answers at the address the user asked for");
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");

        page.QuerySelector("h1")!.TextContent.Should().Be(NotFoundHeading, html);
        page.Title.Should().Contain(NotFoundHeading);
        page.QuerySelector("a[href='/']")!.TextContent.Trim().Should().Be("Back to home", "the way back into the app");

        // In the app's own layout, for the user who is signed in: the rerun is authenticated like the request it reran.
        page.QuerySelector(".sidebar").Should().NotBeNull("the page is in the main layout, with the navigation");
        page.QuerySelector(".top-row.auth")!.TextContent.Should().Contain(email);

        // The app's CSP header and the import map carry one nonce: the rerun kept the request's, or Blazor's scripts would
        // be refused by the page's own policy. (The other CSP header is Blazor's own "frame-ancestors 'self'", which it
        // adds to every interactive page.)
        var policy = response.Headers.GetValues("Content-Security-Policy")
            .Should().ContainSingle(value => value.Contains("script-src", StringComparison.Ordinal)).Which;
        var nonce = Regex.Match(policy, "'nonce-([^']+)'").Groups[1].Value;
        nonce.Should().NotBeNullOrEmpty();
        page.QuerySelector("script[type=importmap]")!.GetAttribute("nonce").Should().Be(nonce);
    }

    /// <summary>
    /// A placeholder the nav still links answers as before, with its own heading and status 200: only a feature the
    /// placeholder does not stand in for is Page not found (T178). Named in any case, as the route itself is matched.
    /// </summary>
    [Theory]
    [InlineData("/placeholder/recent-activities")]
    [InlineData("/placeholder/Recent-Activities")]
    public async Task ASignedInUser_OpeningAPlaceholderTheNavLinks_GetsItsComingSoonPage(string address)
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var response = await LoadPageAsync(browser, address);
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(HttpStatusCode.OK, html);
        page.QuerySelector("h1")!.TextContent.Should().Be("Recent Activities", html);
        page.QuerySelector(".state-panel-title")!.TextContent.Should().Be("Coming soon");
    }

    /// <summary>
    /// A download link whose request is gone answers 404 with no body from its own endpoint, and the page load that
    /// follows the link gets the same page (Profile/DataRights.razor links it with a plain anchor).
    /// </summary>
    [Fact]
    public async Task ASignedInUser_FollowingALinkWhoseEndpointAnswers404WithNoBody_GetsThePageToo()
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var response = await LoadPageAsync(browser, $"/account/data-rights/download/{Guid.NewGuid()}");
        var page = Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        page.QuerySelector("h1")!.TextContent.Should().Be(NotFoundHeading);
    }

    /// <summary>
    /// A script, the SignalR client or an API caller asked for something else, and gets the bare 404 it did: the
    /// accepts-HTML check.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("*/*")]
    [InlineData("application/json")]
    [InlineData("text/html;q=0, */*")]
    public async Task ARequestThatDoesNotAcceptHtml_StillGetsABare404(string? accept)
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/no-such-page");
        if (accept is not null)
        {
            request.Headers.TryAddWithoutValidation("Accept", accept);
        }

        using var response = await browser.SendAsync(request);

        await ShouldBeBareAsync(response, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A missing file stays a bare 404, however it is asked for: a stylesheet or a script by the page, an image, or the
    /// address typed into the address bar. A dot in the last segment is a file, as routing's <c>nonfile</c> reads it.
    /// </summary>
    [Theory]
    [InlineData("/css/no-such.css", "text/css,*/*;q=0.1")]
    [InlineData("/_framework/no-such.js", "*/*")]
    [InlineData("/brand/no-such-mark.svg", "image/avif,image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8")]
    [InlineData("/brand/no-such-mark.svg", PageLoadAccept)]
    public async Task AMissingStaticFile_StillGetsABare404(string address, string accept)
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var request = new HttpRequestMessage(HttpMethod.Get, address);
        request.Headers.TryAddWithoutValidation("Accept", accept);
        using var response = await browser.SendAsync(request);

        await ShouldBeBareAsync(response, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A form posted to an unknown address stays a bare 404. Rerun, the post would reach the not-found page as a post
    /// of a form it does not have, and Blazor would answer it with a 400.
    /// </summary>
    [Fact]
    public async Task AFormPostedToAnUnknownAddress_StillGetsABare404()
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/no-such-page")
        {
            Content = new FormUrlEncodedContent([new("field", "value")])
        };
        request.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        using var response = await browser.SendAsync(request);

        await ShouldBeBareAsync(response, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A HEAD of an unknown address stays a bare 404, the GET-only check again. No page answers HEAD: a
    /// Razor-components endpoint maps GET and POST only, so a known page and the not-found page itself are a 405 to it,
    /// and a rerun would turn the 404 into that 405.
    /// </summary>
    [Fact]
    public async Task AHeadOfAnUnknownAddress_StillGetsABare404()
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var knownPage = new HttpRequestMessage(HttpMethod.Head, NotFoundPagePath);
        knownPage.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        using var knownPageResponse = await browser.SendAsync(knownPage);
        knownPageResponse.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed, "why HEAD is not rerun");

        using var request = new HttpRequestMessage(HttpMethod.Head, "/no-such-page");
        request.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        using var response = await browser.SendAsync(request);

        await ShouldBeBareAsync(response, HttpStatusCode.NotFound);
    }

    /// <summary>
    /// A page load of an address that exists for another method is a 405, not a 404, and stays one: the rerun is for
    /// 404 alone.
    /// </summary>
    [Fact]
    public async Task APageLoadOfAPostOnlyAddress_StillGetsABare405()
    {
        using var browser = NewBrowser();
        await SignInAsANewAssessorAsync(browser);

        using var response = await LoadPageAsync(browser, "/account/login/submit");

        await ShouldBeBareAsync(response, HttpStatusCode.MethodNotAllowed);
    }

    /// <summary>
    /// A sign-in form posted without its antiforgery token is refused with a 400, and stays one. It is a post as well as
    /// a 400, so the GET-only check and the 404-only check each keep it bare on their own: Wombat.Web has no GET that
    /// answers a 400 with no body. The 404-only check alone is proved by the 405 and the circuit's GET 401 below.
    /// </summary>
    [Fact]
    public async Task ASignInPostWithoutItsAntiforgeryToken_StillGetsABare400()
    {
        using var browser = NewBrowser();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/account/login/submit")
        {
            Content = new FormUrlEncodedContent([new("Email", "nobody@example.test"), new("Password", "not-a-password")])
        };
        request.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        using var response = await browser.SendAsync(request);

        await ShouldBeBareAsync(response, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// A visitor who has not signed in is sent to the sign-in page, as before, and lands on the address they asked for
    /// after it: the fallback policy answers before any 404 can.
    /// </summary>
    [Fact]
    public async Task AVisitorWhoHasNotSignedIn_OpeningAnUnknownAddress_IsStillSentToTheSignInPage()
    {
        using var browser = NewBrowser();

        using var response = await LoadPageAsync(browser, "/no-such-page");

        response.StatusCode.Should().Be(HttpStatusCode.Found);
        response.Headers.Location!.PathAndQuery.Should().Be("/account/login?ReturnUrl=%2Fno-such-page");
    }

    /// <summary>
    /// The circuit still refuses a visitor who has not signed in with a 401 (T181): its negotiation as
    /// <c>blazor.web.js</c> sends it, and its hub opened as a page, which is a GET that accepts HTML and differs from an
    /// unknown address only in its status.
    /// </summary>
    [Theory]
    [InlineData("POST", "/_blazor/negotiate?negotiateVersion=1", "*/*")]
    [InlineData("GET", "/_blazor?id=circuit-connection", PageLoadAccept)]
    public async Task TheCircuit_StillAnswersAVisitorWhoHasNotSignedIn_WithABare401(string method, string address, string accept)
    {
        using var browser = NewBrowser();

        using var request = new HttpRequestMessage(new HttpMethod(method), address);
        request.Headers.TryAddWithoutValidation("Accept", accept);
        if (method == "POST")
        {
            request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest"); // as the SignalR client sends it
        }

        using var response = await browser.SendAsync(request);

        await ShouldBeBareAsync(response, HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// The MSF respondent page's own 404 already has a body, and keeps it: a tampered link reads "Feedback link not
    /// recognised", for a respondent who has not signed in and for one who has (T163, T205).
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ATamperedFeedbackLink_StillReadsFeedbackLinkNotRecognised(bool signedIn)
    {
        var respondent = $"tampered-{(signedIn ? "signed-in" : "anonymous")}@example.test";
        await _host.OpenCampaignAsync([respondent]);
        var link = _host.LinkMailedTo(respondent);
        var tampered = link[..^1] + (link[^1] == 'A' ? 'B' : 'A');

        using var browser = NewBrowser();
        if (signedIn)
        {
            await SignInAsANewAssessorAsync(browser);
        }

        using var response = await LoadPageAsync(browser, tampered);
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound, html);
        page.QuerySelector("h2")!.TextContent.Should().Be("Feedback link not recognised");
        page.QuerySelector(".alert")!.TextContent.Should().NotBeNullOrWhiteSpace();
        page.QuerySelector("h1").Should().BeNull("the respondent page's own refusal, not the app's not-found page");
    }

    private static async Task ShouldBeBareAsync(HttpResponseMessage response, HttpStatusCode status)
    {
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(status, body);
        body.Should().BeEmpty("the response is answered as it was before T233, with no page");
        response.Content.Headers.ContentType.Should().BeNull();
    }

    private static Task<HttpResponseMessage> LoadPageAsync(HttpClient browser, string address)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, address);
        request.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        return browser.SendAsync(request);
    }

    /// <summary>A browser of its own, from an address of its own: a /24 no other test's sign-in counts against.</summary>
    private HttpClient NewBrowser()
        => _host.NewBrowser($"10.233.{Interlocked.Increment(ref _nextSubnet)}.7");

    /// <summary>Signs <paramref name="browser" /> in, through the sign-in form, as a new Assessor, and returns their email.</summary>
    private async Task<string> SignInAsANewAssessorAsync(HttpClient browser)
    {
        var email = $"not-found-{Guid.NewGuid():N}@example.test";
        await _host.CreateAssessorAsync(email);
        await _host.SignInAsync(browser, email);
        return email;
    }

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);
}
