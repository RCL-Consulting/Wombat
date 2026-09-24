using FluentValidation;
using MediatR;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Infrastructure.MultiSourceFeedback;

namespace Wombat.Api.Endpoints;

/// <summary>
/// The respondent's questionnaire and submission as JSON: the integration endpoint. (T021, T202)
/// </summary>
/// <remarks>
/// Respondents do not come here. Their invitation link opens the web app's page (<c>/msf/respond</c> on Wombat.Web,
/// T205), which is what <c>Wombat:MsfRespondUrl</c> names and what production serves; this host is not deployed. Both
/// ask the same query and send the same command (<see cref="GetMsfResponseFormQuery" />,
/// <see cref="SubmitMsfResponseCommand" />), answer a refusal with the same status (<see cref="MsfResponseRefusals" />)
/// and carry the same rate limit (<see cref="MsfRespondRateLimit" />), so this endpoint cannot accept what the page
/// refuses.
/// </remarks>
public static class MsfRespondEndpoint
{
    public static IEndpointRouteBuilder MapMsfRespondEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/msf")
            .AllowAnonymous()
            .RequireRateLimiting(MsfRespondRateLimit.PolicyName)
            .AddEndpointFilter(AnswerRefusalsPlainly);

        group.MapGet("/respond", async (string token, ISender sender, CancellationToken cancellationToken) =>
        {
            var form = await sender.Send(new GetMsfResponseFormQuery(token), cancellationToken);
            return Results.Ok(form);
        });

        group.MapPost("/respond", async (string token, MsfRespondSubmission request, ISender sender, CancellationToken cancellationToken) =>
        {
            await sender.Send(
                new SubmitMsfResponseCommand(
                    token,
                    request.Answers
                        .Select(answer => new SubmitMsfResponseAnswerItem(answer.QuestionId, answer.ScaleValue, answer.LongText))
                        .ToList()),
                cancellationToken);

            return Results.Ok(new { message = "Response submitted." });
        });

        return app;
    }

    /// <summary>
    /// Answers a refusal meant for the respondent with a 4xx and a body that says what happened. (T202)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Until T202 the Api handled no exception, so a used, expired or unrecognised link, a campaign that had closed, and
    /// an incomplete submission all answered 500 with an empty body. Each is now a problem-details body (RFC 9457) whose
    /// <c>detail</c> is the refusal's own message, written for the respondent.
    /// </para>
    /// <para>
    /// Only <see cref="MsfResponseRefusedException" /> and the request validator's <see cref="ValidationException" /> are
    /// answered here. Anything else is a fault and reaches the host's exception handler, which answers 500 without its
    /// message: a fault's message is not written for a stranger holding a link.
    /// </para>
    /// </remarks>
    private static async ValueTask<object?> AnswerRefusalsPlainly(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (MsfResponseRefusedException refusal)
        {
            var (status, title) = MsfResponseRefusals.Describe(refusal.Reason);
            return Results.Problem(detail: refusal.Message, statusCode: status, title: title);
        }
        catch (ValidationException invalid)
        {
            return Results.Problem(
                detail: string.Join(" ", invalid.Errors.Select(error => error.ErrorMessage).Distinct(StringComparer.Ordinal)),
                statusCode: StatusCodes.Status400BadRequest,
                title: "The response is not complete");
        }
    }

    /// <summary>
    /// The web app's respondent page and this endpoint share one rate limit on a link (T205).
    /// </summary>
    public static void AddMsfResponseRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddMsfRespondPolicy();
        });
    }
}

public sealed class MsfRespondSubmission
{
    public IReadOnlyList<MsfRespondAnswerRequest> Answers { get; init; } = [];
}

public sealed class MsfRespondAnswerRequest
{
    public int QuestionId { get; init; }
    public int? ScaleValue { get; init; }
    public string? LongText { get; init; }
}
