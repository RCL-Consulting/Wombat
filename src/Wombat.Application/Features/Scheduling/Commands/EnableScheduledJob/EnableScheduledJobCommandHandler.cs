using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Scheduling;

namespace Wombat.Application.Features.Scheduling.Commands.EnableScheduledJob;

public sealed class EnableScheduledJobCommandHandler : IRequestHandler<EnableScheduledJobCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public EnableScheduledJobCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(EnableScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var definition = await _dbContext.Set<ScheduledJobDefinition>()
            .SingleOrDefaultAsync(d => d.Key == request.Key, cancellationToken)
            ?? throw new InvalidOperationException($"Scheduled job '{request.Key}' not found.");

        definition.IsEnabled = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
