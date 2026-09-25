using Microsoft.AspNetCore.Diagnostics;

namespace Wombat.Web.Navigation;

/// <summary>
/// An unknown address answers a signed-in user's browser with the app's own "Page not found" page, still with status
/// 404, not with an empty 404 the browser replaces with its own error page (T233).
/// </summary>
/// <remarks>
/// <para>
/// A Razor-components endpoint exists only for a page's route, so an address no page claims reaches no endpoint and the
/// pipeline ends in a 404 with no body. <c>Routes.razor</c>'s <c>NotFoundPage</c> is used only once a router is running:
/// in a circuit, or on a page that calls <c>NavigationManager.NotFound()</c>. The status-code-pages middleware closes
/// that gap by running the request again as <see cref="Path" />, a routed page, keeping the 404. The rerun gets a service
/// scope of its own, as .NET 10 asks for Blazor: a page that ended its render at 404 has already used the request's
/// renderer, and an endpoint that answered 404 may have left changes in the request's <c>DbContext</c>, which the audit
/// writer shares and saves.
/// </para>
/// <para>
/// Only a browser's page load is rerun (<see cref="ShowsThePage" />). Every other response the middleware would have
/// taken is left as it was, by turning the middleware off for it once the rest of the pipeline has answered:
/// </para>
/// <list type="bullet">
/// <item>Any status but 404: the circuit's 401 at <c>/_blazor/negotiate</c> (T181), a 400, a 405.</item>
/// <item>A request that did not ask for HTML: a script's <c>fetch</c>, the SignalR client, an image or a stylesheet.</item>
/// <item>A method other than GET. A form's post is the page's to answer, and a rerun of it would reach the not-found
/// page as a post of a form it does not have. HEAD too: a Razor-components endpoint maps GET and POST only, so every
/// page, the not-found page included, answers HEAD with a 405, and a rerun would turn the 404 into that 405.</item>
/// <item>An address whose last segment has a dot, which is how routing's own <c>nonfile</c> constraint tells a file
/// from a page. A missing static file stays a bare 404, even when it is typed into the address bar.</item>
/// </list>
/// <para>
/// A response that already has a body is left alone by the middleware itself, which reruns only a response with no
/// body, no content type and no length that has not started. So the MSF respondent page's own 404, "Feedback link not
/// recognised", keeps its text: it sets its status as the response starts, after its body is written (T163, T205).
/// </para>
/// <para>
/// A visitor who has not signed in never reaches this: the fallback policy sends them to the sign-in page first, and
/// the address they asked for is where they land after it.
/// </para>
/// </remarks>
internal static class NotFoundPages
{
    /// <summary>The route of <c>Pages/NotFound.razor</c>, the page <c>Routes.razor</c> names as its not-found page.</summary>
    public const string Path = "/not-found";

    /// <summary>
    /// Reruns a browser's 404 as <see cref="Path" />. Call it before anything that routes or answers a request (static
    /// files, routing), and after the middleware whose headers the page must carry (the CSP nonce), so the rerun passes
    /// through the one and keeps the other.
    /// </summary>
    public static IApplicationBuilder UseWombatNotFoundPage(this IApplicationBuilder app)
    {
        app.UseStatusCodePagesWithReExecute(Path, createScopeForStatusCodePages: true);

        // Runs inside the status-code-pages middleware, so it sees the response first and can turn the rerun off.
        return app.Use(async (context, next) =>
        {
            await next(context);

            if (!ShowsThePage(context) && context.Features.Get<IStatusCodePagesFeature>() is { } statusCodePages)
            {
                statusCodePages.Enabled = false;
            }
        });
    }

    /// <summary>
    /// Whether the response, as the pipeline answered it, is a 404 for a browser loading a page: a GET that accepts
    /// HTML, for an address that does not name a file.
    /// </summary>
    private static bool ShowsThePage(HttpContext context)
        => context.Response.StatusCode == StatusCodes.Status404NotFound
           && HttpMethods.IsGet(context.Request.Method)
           && AcceptsHtml(context.Request)
           && !NamesAFile(context.Request.Path);

    private static bool AcceptsHtml(HttpRequest request)
        => request.GetTypedHeaders().Accept.Any(mediaType =>
            mediaType.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase)
            && mediaType.Quality is not 0);

    /// <summary>A dot in the last segment, as routing's <c>nonfile</c> constraint reads it.</summary>
    private static bool NamesAFile(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.AsSpan(value.LastIndexOf('/') + 1).Contains('.');
    }
}
