using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.ReactivateInstitution;

/// <summary>
/// T302. Until T302 an institution was reactivated by saving its record with Active ticked, which
/// <c>UpdateInstitutionCommand</c> wrote for any caller in scope: an InstitutionalAdmin could undo an Administrator's
/// deactivation of her own institution and restart onboarding there. Each state change is now one command with one rule.
/// </summary>
public sealed class ReactivateInstitutionCommandHandler : IRequestHandler<ReactivateInstitutionCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public ReactivateInstitutionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ReactivateInstitutionCommand request, CancellationToken cancellationToken)
    {
        // Refused before anything is read or changed: the audit pipeline saves this context from its catch (T201).
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may reactivate institutions.");
        }

        var institution = await _dbContext.Set<Institution>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Institution {request.Id} was not found.");

        institution.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
