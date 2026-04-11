using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateSubSpeciality;

public sealed class DeactivateSubSpecialityCommandHandler : IRequestHandler<DeactivateSubSpecialityCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateSubSpecialityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateSubSpecialityCommand request, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.Set<SubSpeciality>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Sub-speciality {request.Id} was not found.");

        subSpeciality.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
