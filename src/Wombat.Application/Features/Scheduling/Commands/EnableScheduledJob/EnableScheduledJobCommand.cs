using MediatR;

namespace Wombat.Application.Features.Scheduling.Commands.EnableScheduledJob;

public sealed record EnableScheduledJobCommand(string Key) : IRequest;
