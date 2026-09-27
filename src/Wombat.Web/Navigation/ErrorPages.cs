using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Wombat.Web.Security;

namespace Wombat.Web.Navigation;

/// <summary>
/// A browser's page load that fails answers with the app's own "Something went wrong" page, still with status 500, and a
/// reference the log holds with the failure (T321; T335, flow 01: R2-Error-In, R2-Error-Out, R2-Error-Typed; the round-2
/// review, S5 and S6).
/// </summary>
/// <remarks>
/// <para>
/// Until T321 nothing caught a failure outside Development: Kestrel answered 500 with no body, the browser showed its own
/// error page, and the person had nothing to quote. <c>/Error</c> existed, but only a person who typed it saw it, and
/// once its circuit started it lost the request id it showed.
/// </para>
/// <para>
/// The page is rerun inside the failed request by the exception handler middleware, on a service scope of its own, as
/// .NET 10 asks for Blazor (a render that failed has used the request's renderer, and the request's <c>DbContext</c> may
/// hold changes the audit writer would save). <c>Pages/Error.razor</c> is static and anonymous: it renders once, in that
/// request, so the reference it shows is the request's, and a visitor who has not signed in (an MSF respondent, someone on
/// the sign-in page) sees it too, not a sign-in form.
/// </para>
/// <para>
/// Only a browser's page load gets the page (<see cref="ShowsThePage" />), as for 404 (<see cref="NotFoundPages" />): a
/// GET that accepts HTML. A form's post, a script's <c>fetch</c> and the SignalR client keep the bare 500 they had, and
/// the failure is still logged, by Kestrel. Unlike a 404's, the choice is made on the request, before the failure,
/// because a failure has no response to read.
/// </para>
/// <para>
/// <b>Its place in the pipeline:</b> before <c>SecurityHeadersMiddleware</c> and <see cref="NotFoundPages" />, not after
/// them as T321 proposed. The middleware clears the failed response before the rerun, headers and all, so the CSP header
/// that middleware set would be gone from the page, and the page's import map would carry a nonce no policy named.
/// Outside them, the rerun passes through both again: a fresh nonce, the header naming it, and the 404 rerun as it was. A
/// failure inside the 404's own rerun is caught here too.
/// </para>
/// </remarks>
internal static class ErrorPages
{
    /// <summary>The route of <c>Pages/Error.razor</c>.</summary>
    public const string Path = "/Error";

    /// <summary>The log category of the error page's own faults.</summary>
    public const string LogCategory = "Wombat.Web.Navigation.ErrorPages";

    /// <summary>South Africa keeps UTC+2 all year (<see cref="Domain.Curricula.ProgrammeCalendar" />).</summary>
    private static readonly TimeSpan SouthAfricanOffset = TimeSpan.FromHours(2);

    /// <summary>
    /// Reruns a browser's failed page load as <see cref="Path" />, answered 500. Call it before the middleware whose
    /// headers the page must carry (the CSP nonce) and before anything that answers a request, and outside Development,
    /// whose developer exception page shows the failure itself.
    /// </summary>
    public static IApplicationBuilder UseWombatErrorPage(this IApplicationBuilder app)
        => app.UseWhen(ShowsThePage, branch => branch.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandlingPath = Path,
            CreateScopeForErrors = true,

            // The failure is logged once, with its reference, by ReferenceLog, which runs before the page. The
            // middleware's own line, which names no reference, is left out whenever the page answered; when it could not,
            // the middleware logs as it always does.
            SuppressDiagnosticsCallback = context => context.ExceptionHandledBy != ExceptionHandledType.Unhandled,
        }));

    /// <summary>
    /// Wraps the sign-in cookie's check (<c>OnValidatePrincipal</c>) so that, in the error page's rerun, a check that throws
    /// draws the page signed out rather than failing it. <c>Program.cs</c> configures the application cookie with it, after
    /// Identity has set the check. (The review of the t335 branch.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The rerun runs authentication again, on its own scope. Once the cookie is over a minute old the check reads the
    /// account (<c>SecurityStampValidator</c>, T279), and with the database down that read throws: the failed request
    /// threw it first, and the rerun threw it again, which the exception handler cannot catch. So in an outage a
    /// signed-in person got the server's bare 500, which is the defect T321 was about.
    /// </para>
    /// <para>
    /// In the rerun, and only there, such a fault rejects the principal: the page renders for a visitor who has not signed
    /// in. It does not sign out, as <c>SessionEnd</c> does not for a check it could not make: the cookie is left as it
    /// was, and signs the person in again once the account can be read. Everywhere else the fault goes on to the handler,
    /// whose rerun lands here.
    /// </para>
    /// </remarks>
    public static void GuardTheSignInCheck(CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var check = options.Events.OnValidatePrincipal;
        options.Events.OnValidatePrincipal = async context =>
        {
            if (context.HttpContext.Features.Get<IExceptionHandlerFeature>() is null)
            {
                await check(context);
                return;
            }

            try
            {
                await check(context);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // The failure itself is the request's, already logged with its reference (ReferenceLog).
                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(LogCategory).LogWarning(
                    exception,
                    "The sign-in could not be checked while the error page was drawn; the page is drawn signed out.");
                context.RejectPrincipal();
            }
        };
    }

    /// <summary>A browser loading a page: a GET that accepts HTML.</summary>
    public static bool ShowsThePage(HttpContext context)
        => HttpMethods.IsGet(context.Request.Method) && NotFoundPages.AcceptsHtml(context.Request);

    /// <summary>
    /// The request's reference: its W3C trace id, 32 hexadecimal characters, which the log's request scope carries as
    /// TraceId; the server's own request id where the request has no trace (S6: never the 55-character traceparent, which
    /// wraps at 390px and which no one searches a journal for).
    /// </summary>
    public static string ReferenceOf(HttpContext context)
    {
        var activity = context.Features.Get<IHttpActivityFeature>()?.Activity ?? Activity.Current;
        return activity is not null && activity.TraceId != default
            ? activity.TraceId.ToHexString()
            : context.TraceIdentifier;
    }

    /// <summary>A moment as the page states it: "2026-09-26 15:14 SAST", on the South African clock.</summary>
    public static string TimeOf(DateTimeOffset moment)
        => moment.ToOffset(SouthAfricanOffset).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " SAST";

    /// <summary>
    /// Where Try again goes: the address that failed, its path and query, when it is a path on this site. Null when no
    /// request failed (<c>/Error</c> typed) or the address is not one to send a person to.
    /// </summary>
    public static string? RetryAddressOf(HttpContext context)
    {
        if (context.Features.Get<IExceptionHandlerPathFeature>() is not { } failure)
        {
            return null;
        }

        return LocalUrl.OrNull(failure.Path + context.Request.QueryString.Value);
    }

    /// <summary>
    /// Logs the failure the error page is about to report, with the reference the page shows, so an administrator can find
    /// it in the journal from what a person quotes (T321). It handles nothing: the page does.
    /// </summary>
    internal sealed class ReferenceLog(ILogger<ReferenceLog> logger) : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var failed = httpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path ?? httpContext.Request.Path.Value;
            logger.LogError(
                exception,
                "A request failed: {Method} {Path}. The error page shows its reference {Reference}.",
                httpContext.Request.Method,
                failed,
                ReferenceOf(httpContext));

            return ValueTask.FromResult(false);
        }
    }
}
