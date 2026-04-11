using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

public sealed record DeactivateTraineeProfileCommand(int Id) : IRequest;

public sealed class DeactivateTraineeProfileCommandHandler : IRequestHandler<DeactivateTraineeProfileCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateTraineeProfileCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateTraineeProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        profile.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
