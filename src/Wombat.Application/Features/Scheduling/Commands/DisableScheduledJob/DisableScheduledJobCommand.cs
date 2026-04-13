using MediatR;

namespace Wombat.Application.Features.Scheduling.Commands.DisableScheduledJob;

public sealed record DisableScheduledJobCommand(string Key) : IRequest;
