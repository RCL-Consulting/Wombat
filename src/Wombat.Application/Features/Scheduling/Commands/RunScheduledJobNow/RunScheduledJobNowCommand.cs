using System.Security.Claims;
using MediatR;
using Wombat.Application.Common;
using Wombat.Application.Scheduling;

namespace Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;

/// <summary>
/// Runs a scheduled job now, as the caller (T400, flow 18, K6): the handler takes the person's id from
/// <paramref name="Principal" />, never a value the page passes, and answers how the run ended.
/// </summary>
/// <remarks>No validator: Key is a well-known string constant from the job registry; the handler checks it is there.</remarks>
[NoValidator]
public sealed record RunScheduledJobNowCommand(string Key, ClaimsPrincipal Principal) : IRequest<ScheduledJobRunOutcome>;
