using MediatR;
using Wombat.Application.Common;

namespace Wombat.Application.Features.Scheduling.Commands.DisableScheduledJob;

/// <summary>No validator: Key is a well-known string constant from the job registry; handler validates it exists.</summary>
[NoValidator]
public sealed record DisableScheduledJobCommand(string Key) : IRequest;
