using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record RevokeInvitationCommand(int InvitationId) : IRequest;

public sealed class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RevokeInvitationCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.Set<Invitation>()
            .SingleOrDefaultAsync(entity => entity.Id == request.InvitationId, cancellationToken);

        if (invitation is null)
        {
            throw new InvalidOperationException("The invitation was not found.");
        }

        if (invitation.UsedOn.HasValue)
        {
            throw new InvalidOperationException("Used invitations cannot be revoked.");
        }

        if (!invitation.RevokedOn.HasValue)
        {
            invitation.RevokedOn = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
