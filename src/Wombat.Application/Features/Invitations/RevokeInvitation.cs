using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record RevokeInvitationCommand(int InvitationId, ClaimsPrincipal Principal) : IRequest;

public sealed class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RevokeInvitationCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// A trainee first (T290), then one refusal for an id that names no invitation and one outside the caller's scope,
    /// so an InstitutionalAdmin cannot tell which ids exist elsewhere (T194's rule). A global Administrator sees every
    /// invitation, so is told plainly that an id names none.
    /// </remarks>
    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var invitation = await _dbContext.Set<Invitation>()
            .SingleOrDefaultAsync(entity => entity.Id == request.InvitationId, cancellationToken);

        if (invitation is null && request.Principal.IsAdministrator())
        {
            throw new InvitationIssueRefusedException(
                InvitationIssueRefusal.CannotRevoke, InvitationIssueField.None, "The invitation was not found.");
        }

        // CollegeAdmin invitations are college-scoped (Administrator-only); every other invitation
        // is institution-scoped. (T093)
        var canAccess = invitation is not null && (invitation.CollegeId.HasValue
            ? request.Principal.CanAccessCollege(invitation.CollegeId.Value)
            : invitation.InstitutionId.HasValue && request.Principal.CanAccessInstitution(invitation.InstitutionId.Value));

        if (invitation is null || !canAccess)
        {
            throw new UnauthorizedAccessException("You do not have permission to revoke this invitation.");
        }

        if (invitation.UsedOn.HasValue)
        {
            // Typed, so the page shows these words and no other exception's (T373, S9).
            throw new InvitationIssueRefusedException(
                InvitationIssueRefusal.CannotRevoke, InvitationIssueField.None, "Used invitations cannot be revoked.");
        }

        if (!invitation.RevokedOn.HasValue)
        {
            invitation.RevokedOn = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
