using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;

/// <summary>No validator: Key is a well-known string constant from the job registry; TriggeredByUserId is set by the authenticated caller.</summary>
[NoValidator]
public sealed record RunScheduledJobNowCommand(string Key, string TriggeredByUserId) : IRequest;
