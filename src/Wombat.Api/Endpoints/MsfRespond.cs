using System.Threading.RateLimiting;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Wombat.Application.Features.MultiSourceFeedback;

namespace Wombat.Api.Endpoints;

public static class MsfRespondEndpoint
{
    public static IEndpointRouteBuilder MapMsfRespondEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/msf")
            .AllowAnonymous()
            .RequireRateLimiting("msf-respond")
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
            var (status, title) = Describe(refusal.Reason);
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
    /// The status and title a refusal answers with. A link that names nothing is 404; a link that did name an invitation
    /// but can no longer be used is 410, because it will never work again; answers that do not complete the form are 400.
    /// </summary>
    internal static (int Status, string Title) Describe(MsfResponseRefusal reason) => reason switch
    {
        MsfResponseRefusal.LinkNotRecognised => (StatusCodes.Status404NotFound, "Feedback link not recognised"),
        MsfResponseRefusal.LinkExpired => (StatusCodes.Status410Gone, "Feedback link expired"),
        MsfResponseRefusal.LinkRevoked => (StatusCodes.Status410Gone, "Feedback link revoked"),
        MsfResponseRefusal.LinkUsed => (StatusCodes.Status410Gone, "Feedback link already used"),
        MsfResponseRefusal.CampaignNotOpen => (StatusCodes.Status410Gone, "Feedback request closed"),
        MsfResponseRefusal.AnswersIncomplete => (StatusCodes.Status400BadRequest, "The response is not complete"),
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "An MSF refusal with no answer.")
    };

    public static void AddMsfResponseRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("msf-respond", context =>
            {
                var token = context.Request.Query["token"].ToString();
                var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                var partitionKey = $"{remoteIp}:{token}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
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
