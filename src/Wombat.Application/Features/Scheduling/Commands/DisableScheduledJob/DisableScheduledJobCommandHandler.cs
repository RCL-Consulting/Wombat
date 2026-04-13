using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Scheduling.Commands.DisableScheduledJob;

public sealed class DisableScheduledJobCommandHandler : IRequestHandler<DisableScheduledJobCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DisableScheduledJobCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DisableScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var definition = await _dbContext.Set<ScheduledJobDefinition>()
            .SingleOrDefaultAsync(d => d.Key == request.Key, cancellationToken)
            ?? throw new InvalidOperationException($"Scheduled job '{request.Key}' not found.");

        definition.IsEnabled = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
