using System.Threading.RateLimiting;
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
            .RequireRateLimiting("msf-respond");

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
