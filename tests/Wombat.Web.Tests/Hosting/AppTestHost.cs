using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.MultiSourceFeedback;
using Wombat.Web.Components;
using Wombat.Web.Security;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// <see cref="App" /> served in process by the framework's own Razor-components endpoint, against the static-assets
/// manifest Wombat.Web's build wrote, behind the CSP middleware. What a browser receives on a full load, without a
/// database or a browser.
/// </summary>
/// <remarks>
/// <para>
/// bUnit cannot show what <c>@Assets["app.css"]</c> resolves to: its renderer's asset collection is always empty, so it
/// hands the label back unchanged and a fingerprinted link is indistinguishable from a bare one. Only an endpoint
/// rendered next to <c>MapStaticAssets()</c> resolves it, so this host maps both.
/// </para>
/// <para>
/// It mirrors <c>Program.cs</c> only as far as <see cref="App" /> and <see cref="AnonymousPage" /> need, and runs as
/// Production, as the server does. The authorization policies, the static-asset endpoints and the Razor-components
/// endpoints are registered by the methods <c>Program.cs</c> calls, so an anonymous visitor meets the real fallback
/// policy and its real opt-outs (T175, T181). A request is anonymous unless the test signs it in
/// (<see cref="SignedInVisitor" />). There is no database, so a page rendered through it must inject nothing beyond
/// what is registered here.
/// </para>
/// <para>
/// The files are the ones a publish copies into <c>wwwroot</c>: the project's own, the scoped-CSS bundle and the
/// framework's <c>blazor.web.js</c>, found through the build's static-web-assets manifest. Without it only the project's
/// <c>wwwroot</c> is there, and a generated asset's endpoint answers 200 with an empty body.
/// </para>
/// </remarks>
internal sealed class AppTestHost : IAsyncDisposable
{
    /// <summary>A page that renders the whole of <see cref="App" /> for an anonymous visitor and needs no service.</summary>
    public const string AnonymousPage = "/account/forgot-password";

    /// <summary>Where a request the fallback policy refuses is sent, as Identity's cookie is configured.</summary>
    private const string SignInPath = "/account/login";

    private readonly WebApplication _app;

    private AppTestHost(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => _app.Services;

    /// <param name="configureServices">
    /// Registers what a page under test injects beyond this host's own services: the MSF respondent page's sender (T205).
    /// </param>
    public static async Task<AppTestHost> StartAsync(Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            // MapStaticAssets() reads "{ApplicationName}.staticwebassets.endpoints.json", which the project reference
            // copies next to this assembly.
            ApplicationName = typeof(App).Assembly.GetName().Name,
            EnvironmentName = Environments.Production,
            ContentRootPath = AppContext.BaseDirectory,
        });
        builder.WebHost.UseTestServer();

        // "{ApplicationName}.staticwebassets.runtime.json", also copied here, maps each asset to where the build left
        // it. Development turns this on by itself; Production, which this host runs as, needs it asked for.
        builder.WebHost.UseStaticWebAssets();
        builder.Logging.ClearProviders();

        // A build's manifest (this one) switches on the static-assets reloader, which answers every asset with
        // no-cache so an edited file shows at once; a publish's manifest does not. Off, the host answers with the
        // manifest's own headers, as the published server does.
        builder.Configuration["ReloadStaticAssetsAtRuntime"] = "false";

        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

        // The circuit's own services, as Program.cs registers them: Routes.razor renders LeaveEndedSession, which injects
        // EndedSessionExit, on every page (the T279 review).
        builder.Services.AddWombatCircuitServices();

        // The real policies, fallback included: every endpoint requires a signed-in user unless it opts out. The cookie
        // scheme stands in for Identity's (AddInfrastructure needs a database) and challenges the way it does, with a
        // redirect to the sign-in page.
        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options => options.LoginPath = SignInPath);
        builder.Services.AddWombatAuthorization();
        builder.Services.AddHttpContextAccessor();

        // The MSF respondent page's rate limit and what it answers when the limit refuses, as Program.cs registers them
        // (T205). Program.cs also carries the sign-in throttle; the integration suite hosts the whole of it.
        builder.Services.AddRateLimiter(options =>
        {
            options.AddMsfRespondPolicy();
            options.OnRejected = (context, cancellationToken) => MsfRespondThrottle.RefuseAsync(context.HttpContext, cancellationToken);
        });

        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        // The order Program.cs uses.
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapWombatStaticAssets();
        app.MapWombatRazorComponents();

        await app.StartAsync();
        return new AppTestHost(app);
    }

    /// <summary>A full load of <paramref name="path" />: the response and its prerendered HTML, parsed.</summary>
    public async Task<(HttpResponseMessage Response, string Html, IHtmlDocument Document)> LoadAsync(string path)
    {
        var response = await Client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return (response, html, new HtmlParser().ParseDocument(html));
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
