using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record GetInvitationPreviewQuery(string Token) : IRequest<InvitationPreviewDto>;

/// <remarks>
/// The register page offers its form only for an invitation this answers, so it refuses what the submit would refuse
/// whatever the person entered: an invitation that is revoked, used, expired or not recognised, and one for an address no
/// account can be created for, which it judges by the provisioner's own test (T285). The page keeps its form under a refusal
/// the person can put right, a password or a confirmation, so a refusal no input can put right must stop here: an
/// invitation to an address an account already holds would otherwise come back to the form after every submit, refused
/// each time.
/// </remarks>
public sealed class GetInvitationPreviewQueryHandler : IRequestHandler<GetInvitationPreviewQuery, InvitationPreviewDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitedUserProvisioner _provisioner;

    public GetInvitationPreviewQueryHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IInvitedUserProvisioner provisioner)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _provisioner = provisioner;
    }

    public async Task<InvitationPreviewDto> Handle(GetInvitationPreviewQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _dbContext.Set<Invitation>()
            .OrderByDescending(entity => entity.IssuedOn)
            .ToListAsync(cancellationToken);

        // The invitation's own state first: an accepted invitation's address is held by the account it created, and it
        // reads as used, not as taken.
        var invitation = InvitationRules.GetActiveInvitationOrThrow(invitations, request.Token, _tokenService);

        var status = await _provisioner.GetAddressStatusAsync(invitation.Email, cancellationToken);
        if (InvitationRefusals.For(status) is { } refusal)
        {
            throw new InvitationRefusedException(refusal);
        }

        return new InvitationPreviewDto(invitation.Email, invitation.TargetRole);
    }
}
