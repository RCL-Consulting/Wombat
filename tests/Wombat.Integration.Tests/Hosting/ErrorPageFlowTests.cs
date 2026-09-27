extern alias WombatWeb;

using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T321 and T335, flow 01 (R2-Error-In, R2-Error-Out, R2-Error-Typed; the round-2 review, S5 and S6): a browser's page load
/// that fails answers 500 with the app's own "Something went wrong" page, carrying the CSP header and its nonce, and a
/// reference the log holds with the failure; every other failed request keeps its bare 500; typed, /Error says nothing
/// went wrong.
/// </summary>
/// <remarks>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own, as the MSF respondent page's
/// fixture hosts it (<see cref="MsfRespondPageFlowTests.WebFactory" />), in the "IntegrationTest" environment, so the
/// pipeline is the server's and not Development's. The one addition is the failure itself: a test-only endpoint that
/// throws, which a startup filter gives the request before the app's pipeline sees it (<see cref="Throws" />). Nothing in
/// the app's code throws on purpose.
/// </para>
/// <para>
/// Until T321 nothing caught a failure outside Development: Kestrel answered 500 with no body, and the browser showed its
/// own error page. TestServer does not stand in for Kestrel there: an exception nothing catches reaches the test's client
/// as itself, which is how the tests below see that no page was served.
/// </para>
/// </remarks>
public sealed class ErrorPageFlowTests : IClassFixture<ErrorPageFlowTests.Host>
{
    /// <summary>What Chrome sends when it loads a page from the address bar or a link.</summary>
    private const string PageLoadAccept =
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8," +
        "application/signed-exchange;v=b3;q=0.7";

    private static int _nextSubnet;

    private readonly Host _host;

    public ErrorPageFlowTests(Host host)
    {
        _host = host;
    }

    [Fact]
    public async Task APageLoadThatFails_SignedIn_Answers500_WithTheErrorPage_ItsNonce_AndAReferenceTheLogHolds()
    {
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, await _host.CreateUserAsync(WombatRoles.Trainee));
        var failure = $"{Throws.Path}/signed-in?tab=history";

        using var response = await LoadPageAsync(browser, failure);
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, html);
        response.Headers.Location.Should().BeNull("the page answers at the failed address, with no redirect");
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        page.QuerySelector("h1")!.TextContent.Should().Be("Something went wrong");
        page.Title.Should().Be("Something went wrong · Wombat");
        Text(page.QuerySelector(".system-panel p")!).Should().EndWith(
            "send this reference to your institution's Wombat administrator.");

        // The CSP header and the import map carry one nonce: the rerun passed the security headers again after the
        // failed response was cleared, or Blazor's scripts would be refused by the page's own policy.
        var policy = response.Headers.GetValues("Content-Security-Policy")
            .Should().ContainSingle(value => value.Contains("script-src", StringComparison.Ordinal)).Which;
        var nonce = Regex.Match(policy, "'nonce-([^']+)'").Groups[1].Value;
        nonce.Should().NotBeNullOrEmpty();
        page.QuerySelector("script[type=importmap]")!.GetAttribute("nonce").Should().Be(nonce);

        // Static: no circuit to lose the reference in (T321).
        html.Should().NotContain("<!--Blazor:", "the error page is excluded from interactive routing");

        var reference = page.QuerySelector(".reference-block dd code")!.TextContent;
        reference.Should().MatchRegex("^[0-9a-f]{32}$", "the W3C trace id, not the 55-character traceparent (S6)");
        page.QuerySelector(".reference-block dd:last-of-type")!.TextContent.Should().EndWith(" SAST");
        _host.Log.Failures(failure: "signed-in").Should().ContainSingle(
                "the failure is logged once, with its reference: every error line carrying the thrown exception is counted, " +
                "whatever its words, so a second line from the exception middleware would fail this")
            .Which.Message.Should().Contain(reference);

        var tryAgain = page.QuerySelector("a[data-enhance-nav=false]")!;
        Text(tryAgain).Should().Be("Try again");
        tryAgain.GetAttribute("href").Should().Be(failure, "the failed address, its path and its query");
        page.QuerySelector("main a[href='/']")!.TextContent.Trim().Should().Be("Go to Home");

        // Sign out is a link to the sign-out page, which draws its own form and token: the layout's token was made in the
        // failed pass, whose antiforgery cookie the handler cleared with the response, so a form here could be refused.
        var signOut = page.QuerySelector(".account-row a.sign-out-button")!;
        signOut.GetAttribute("href").Should().Be("/account/logout-confirm");
        Text(signOut).Should().Be("Sign out");
        page.QuerySelector("form[action='/account/logout']").Should().BeNull();
    }

    // The database down, and the sign-in cookie over a minute old: its security stamp is checked against the account
    // (T279), and the check throws, in the failed request and again in the error page's rerun. Until the review of the t335
    // branch nothing caught the second, so a signed-in person got the server's bare 500 in an outage, which is T321's
    // defect. The rerun now draws the page signed out, and leaves the cookie alone for when the database is back.
    [Fact]
    public async Task WhenTheSignInCannotBeChecked_ThePageLoadStillAnswersWithTheErrorPage_SignedOut_KeepingTheCookie()
    {
        var email = await _host.CreateUserAsync(WombatRoles.Trainee);
        await using var outage = _host.WithSignInCheckThatFails();
        using var browser = _host.NewBrowser(outage, $"10.237.{Interlocked.Increment(ref _nextSubnet)}.7");
        await _host.SignInAsync(browser, email);

        using var response = await LoadPageAsync(browser, "/activities/mine");
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, html);
        page.QuerySelector("h1")!.TextContent.Should().Be("Something went wrong");
        Text(page.QuerySelector(".system-card p")!).Should().EndWith(
            "send this reference to whoever sent you the link, or to your Wombat administrator.", "the page is drawn signed out");
        page.QuerySelector(".signed-out-bar a.sign-in").Should().NotBeNull();
        page.QuerySelector(".account-row").Should().BeNull();
        page.QuerySelector("a[data-enhance-nav=false]")!.GetAttribute("href").Should().Be("/activities/mine");

        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        (cookies ?? []).Should().NotContain(cookie => cookie.StartsWith(".AspNetCore.Identity.Application=;", StringComparison.Ordinal),
            "a check that could not be made signs nobody out: the cookie is good again once the database is back");

        var reference = page.QuerySelector(".reference-block dd code")!.TextContent;
        _host.Log.Errors(SignInCheckThatFails.Message).Should().ContainSingle("the failure is logged once, with its reference")
            .Which.Message.Should().Contain(reference);
    }

    // An MSF respondent, or anyone on the sign-in page, is not signed in: the page is theirs too, not a sign-in form.
    [Fact]
    public async Task APageLoadThatFails_SignedOut_Answers500_WithTheErrorPage_NotASignIn()
    {
        using var browser = NewBrowser();

        using var response = await LoadPageAsync(browser, $"{Throws.Path}/signed-out");
        var page = Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Headers.Location.Should().BeNull();
        page.QuerySelector("h1")!.TextContent.Should().Be("Something went wrong");
        Text(page.QuerySelector(".system-card p")!).Should().EndWith(
            "send this reference to whoever sent you the link, or to your Wombat administrator.");
        var reference = page.QuerySelector(".reference-block dd code")!.TextContent;
        _host.Log.Failures(failure: "signed-out").Should().ContainSingle()
            .Which.Message.Should().Contain(reference);
        page.QuerySelectorAll("main .actions-cell .btn svg").Should().BeEmpty("signed out, the buttons carry no icons (R2-Error-Out)");
    }

    // A form's post, a script's fetch or the SignalR client asked for no page, and gets none: the failure is not caught.
    [Theory]
    [InlineData("POST", PageLoadAccept)]
    [InlineData("GET", "application/json")]
    [InlineData("GET", "*/*")]
    [InlineData("GET", "text/html;q=0, */*")]
    public async Task ARequestThatIsNotAPageLoad_GetsNoPage(string method, string accept)
    {
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, await _host.CreateUserAsync(WombatRoles.Trainee));

        using var request = new HttpRequestMessage(new HttpMethod(method), $"{Throws.Path}/not-a-page-load");
        request.Headers.TryAddWithoutValidation("Accept", accept);
        if (method == "POST")
        {
            request.Content = new FormUrlEncodedContent([new("field", "value")]);
        }

        var send = () => browser.SendAsync(request);

        (await send.Should().ThrowAsync<InvalidOperationException>(
                "nothing caught it, so the server answers the bare 500 it did before T321"))
            .Which.Message.Should().Be(Throws.Message);
    }

    // A.5.8 (R2-Error-Typed): no request failed, so there is nothing to report and nothing to try again.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TypedAsAnAddress_TheErrorPageSaysNothingWentWrong(bool signedIn)
    {
        using var browser = NewBrowser();
        if (signedIn)
        {
            await _host.SignInAsync(browser, await _host.CreateUserAsync(WombatRoles.Assessor));
        }

        using var response = await LoadPageAsync(browser, "/Error");
        var html = await response.Content.ReadAsStringAsync();
        var page = Parse(html);

        response.StatusCode.Should().Be(HttpStatusCode.OK, html);
        page.QuerySelector("h1")!.TextContent.Should().Be("Nothing went wrong");
        page.Title.Should().Be("Nothing went wrong · Wombat");
        page.QuerySelector(".reference-block").Should().BeNull();
        page.QuerySelectorAll("a[href='/']").Select(link => link.TextContent.Trim()).Should().Contain("Go to Home");
        page.QuerySelector("a[data-enhance-nav=false]").Should().BeNull("no Try again");
    }

    private static Task<HttpResponseMessage> LoadPageAsync(HttpClient browser, string address)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, address);
        request.Headers.TryAddWithoutValidation("Accept", PageLoadAccept);
        return browser.SendAsync(request);
    }

    private HttpClient NewBrowser() => _host.NewBrowser($"10.237.{Interlocked.Increment(ref _nextSubnet)}.7");

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);

    private static string Text(AngleSharp.Dom.IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// The test-only failure: a request under <see cref="Path" /> is given an endpoint, open to anyone, that throws. Set
    /// before the app's pipeline runs, so routing keeps it and authorization admits it, and the failure is met where a
    /// page's would be, inside the app's exception handling.
    /// </summary>
    private sealed class Throws : IStartupFilter
    {
        public const string Path = "/__test/throws";

        public const string Message = "A test endpoint failed on purpose (ErrorPageFlowTests).";

        /// <summary>The key of the exception's <see cref="Exception.Data" /> that holds the path of the request it failed.</summary>
        public const string PathKey = "ErrorPageFlowTests.Path";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((context, rest) =>
            {
                if (context.Request.Path.StartsWithSegments(Path))
                {
                    context.SetEndpoint(new Endpoint(
                        failed => throw new InvalidOperationException(Message) { Data = { [PathKey] = failed.Request.Path.Value } },
                        new EndpointMetadataCollection(new AllowAnonymousAttribute()),
                        "Test: throws"));
                }

                return rest(context);
            });

            next(app);
        };
    }

    /// <summary>
    /// The sign-in cookie's check against the account, failing as it does with the database down: the check the cookie
    /// handler asks for on every request that carries the cookie (<c>OnValidatePrincipal</c>), which reads the account once
    /// the cookie is over a minute old (T279). Here it throws whatever the cookie's age.
    /// </summary>
    private sealed class SignInCheckThatFails : ISecurityStampValidator
    {
        public const string Message = "The account could not be read (ErrorPageFlowTests: the database is down).";

        public Task ValidateAsync(CookieValidatePrincipalContext context) => throw new InvalidOperationException(Message);
    }

    /// <summary>One log line: its level, its message as written, and its exception.</summary>
    public sealed record LogEntry(string Category, LogLevel Level, string Message, Exception? Exception);

    /// <summary>Every log line the host writes, kept for the tests to read.</summary>
    public sealed class CapturingLog : ILoggerProvider
    {
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        /// <summary>
        /// Every error logged with the exception <see cref="Throws" /> threw for the request whose path holds
        /// <paramref name="failure" />, whatever the line's words: the reference's line, and any other line that carries it.
        /// </summary>
        public IReadOnlyList<LogEntry> Failures(string failure)
            => _entries.Where(entry => entry.Level >= LogLevel.Error
                    && entry.Exception is { Message: Throws.Message } exception
                    && exception.Data[Throws.PathKey] is string path
                    && path.Contains(failure, StringComparison.Ordinal))
                .ToList();

        /// <summary>Every error logged with an exception whose message is <paramref name="message" />, whatever the line's words.</summary>
        public IReadOnlyList<LogEntry> Errors(string message)
            => _entries.Where(entry => entry.Level >= LogLevel.Error && entry.Exception?.Message == message).ToList();

        public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _entries);

        public void Dispose()
        {
        }

        private sealed class Logger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                {
                    entries.Enqueue(new LogEntry(category, logLevel, formatter(state, exception), exception));
                }
            }
        }
    }

    /// <summary>
    /// <c>Wombat.Web</c> as the server runs it, on a schema of its own, with the failing endpoint and a log the tests read.
    /// </summary>
    public sealed class Host : IAsyncLifetime
    {
        private const string Password = "Error-Page-Pa55word!";
        private const string DemoInstitutionShortCode = "DEMO";

        private readonly string _schemaName = TestDatabase.NewSchemaName();
        private MsfRespondPageFlowTests.WebFactory? _inner;
        private WebApplicationFactory<WombatWeb::Program>? _factory;

        public CapturingLog Log { get; } = new();

        private WebApplicationFactory<WombatWeb::Program> Factory
            => _factory ?? throw new InvalidOperationException("The web host is not running.");

        public async Task InitializeAsync()
        {
            try
            {
                await TestDatabase.CreateSchemaAsync(_schemaName);
                _inner = new MsfRespondPageFlowTests.WebFactory(
                    TestDatabase.SchemaConnectionString(_schemaName), MsfRespondPageFlowTests.WebHost.RespondUrl);
                _factory = _inner.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IStartupFilter, Throws>();
                    services.AddSingleton<ILoggerProvider>(Log);
                }));

                // Starts the host: it migrates the schema and seeds the catalogue before it serves anything.
                using var client = _factory.CreateClient();
            }
            catch (Exception setupFailure)
            {
                // xUnit 2 does not call DisposeAsync after a failed InitializeAsync, so the schema is dropped here (T140).
                try
                {
                    await DisposeAsync();
                }
                catch (Exception teardownFailure)
                {
                    throw new AggregateException(
                        "InitializeAsync failed, and so did the teardown that should have dropped its schema.",
                        setupFailure,
                        teardownFailure);
                }

                throw;
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (_factory is not null)
                {
                    var factory = _factory;
                    _factory = null;
                    await factory.DisposeAsync();
                }

                if (_inner is not null)
                {
                    var inner = _inner;
                    _inner = null;
                    await inner.DisposeAsync();
                }
            }
            finally
            {
                await TestDatabase.DropSchemaAsync(_schemaName);
            }
        }

        /// <summary>A browser of its own: its own cookies and its own address, so the sign-in throttle it meets is its own.</summary>
        public HttpClient NewBrowser(string address) => NewBrowser(Factory, address);

        /// <summary>A browser of its own on <paramref name="app" />, one of this host's variants.</summary>
        public HttpClient NewBrowser(WebApplicationFactory<WombatWeb::Program> app, string address)
        {
            var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Forwarded-For", address);
            return client;
        }

        /// <summary>This host with the database down as far as the sign-in cookie's check sees it (<see cref="SignInCheckThatFails" />).</summary>
        public WebApplicationFactory<WombatWeb::Program> WithSignInCheckThatFails()
            => Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISecurityStampValidator>();
                services.AddScoped<ISecurityStampValidator, SignInCheckThatFails>();
            }));

        /// <summary>A user at the Demo Institution who holds <paramref name="role" /> and signs in with a password.</summary>
        public async Task<string> CreateUserAsync(string role)
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var institutionId = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Institutions
                .Where(entity => entity.ShortCode == DemoInstitutionShortCode)
                .Select(entity => entity.Id)
                .SingleAsync();

            var email = $"error-page-{Guid.NewGuid():N}@example.test";
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var user = new WombatIdentityUser
            {
                UserName = email,
                Email = email,
                FirstName = "Error",
                LastName = "Page",
                InstitutionId = institutionId
            };
            (await users.CreateAsync(user, Password)).Succeeded.Should().BeTrue($"guard: the {role} exists");
            (await users.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue($"guard: the {role} holds the role");
            return email;
        }

        /// <summary>Signs a browser in through the sign-in page's own form, as a person does.</summary>
        public async Task SignInAsync(HttpClient browser, string email)
        {
            using var loginPage = await browser.GetAsync("/account/login");
            var form = new HtmlParser().ParseDocument(await loginPage.Content.ReadAsStringAsync())
                .QuerySelector("form[action='/account/login/submit']")!;

            using var signIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
            [
                new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
                new("Email", email),
                new("Password", Password)
            ]));

            signIn.StatusCode.Should().Be(HttpStatusCode.Redirect);
            signIn.Headers.Location!.ToString().Should().NotContain("/account/login", "guard: the sign-in succeeded");
        }
    }
}
