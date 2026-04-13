using MediatR;

namespace Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;

public sealed record RunScheduledJobNowCommand(string Key, string TriggeredByUserId) : IRequest;
