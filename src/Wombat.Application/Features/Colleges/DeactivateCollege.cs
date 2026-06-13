using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Colleges;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateCollegeCommand(int Id, ClaimsPrincipal Principal) : IRequest;

public sealed class DeactivateCollegeCommandHandler : IRequestHandler<DeactivateCollegeCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateCollegeCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateCollegeCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may deactivate colleges.");
        }

        var college = await _dbContext.Set<College>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"College {request.Id} was not found.");

        college.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
