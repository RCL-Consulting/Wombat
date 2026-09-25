using Wombat.Infrastructure.MultiSourceFeedback;

namespace Wombat.Web.Security;

/// <summary>
/// What a respondent is answered with when their link's rate limit refuses a request (<see cref="MsfRespondRateLimit" />,
/// T205).
/// </summary>
/// <remarks>
/// The web host's rate limiter carries these limits alone; the sign-in throttle is not one of its policies
/// (<see cref="SignInThrottle" />, T156) and answers with a redirect to the sign-in page. A respondent is not signed in
/// and has no account, so sending them there would say something untrue. They get a 429 that says what happened, in
/// plain text: nothing on it needs the page's layout, its script or its stylesheet.
/// </remarks>
internal static class MsfRespondThrottle
{
    /// <summary>
    /// Said for either limit (<see cref="MsfRespondRateLimit" />): too many requests through this link, or too many from
    /// this network to any link. The respondent does the same about both.
    /// </summary>
    public const string Message =
        "Too many requests have been made to this feedback page from your network in the last minute, so this one was " +
        "not accepted. Wait a minute, then try again. If you were submitting the questionnaire, go back to it and " +
        "submit it again.";

    public static async ValueTask RefuseAsync(HttpContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers.RetryAfter = ((int)MsfRespondRateLimit.Window.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(Message, cancellationToken);
    }
}
