using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateSpeciality;

public sealed class DeactivateSpecialityCommandHandler : IRequestHandler<DeactivateSpecialityCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateSpecialityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateSpecialityCommand request, CancellationToken cancellationToken)
    {
        var speciality = await _dbContext.Set<Speciality>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Speciality {request.Id} was not found.");

        speciality.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
