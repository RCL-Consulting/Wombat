using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Trainees;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateTraineeProfileCommand(int Id, ClaimsPrincipal Principal) : IRequest;

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
            .Include(entity => entity.Curriculum)
                .ThenInclude(entity => entity.SubSpeciality)
                    .ThenInclude(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("The trainee profile could not be found.");

        if (!request.Principal.CanAccessInstitution(profile.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to deactivate this trainee profile.");
        }

        profile.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
