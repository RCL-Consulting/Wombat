using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.DeactivateInstitution;

public sealed class DeactivateInstitutionCommandHandler : IRequestHandler<DeactivateInstitutionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateInstitutionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var institution = await _dbContext.Set<Institution>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Institution {request.Id} was not found.");

        institution.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
