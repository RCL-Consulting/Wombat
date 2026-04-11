using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record GetInvitationPreviewQuery(string Token) : IRequest<InvitationPreviewDto>;

public sealed class GetInvitationPreviewQueryHandler : IRequestHandler<GetInvitationPreviewQuery, InvitationPreviewDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;

    public GetInvitationPreviewQueryHandler(IApplicationDbContext dbContext, IInvitationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<InvitationPreviewDto> Handle(GetInvitationPreviewQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _dbContext.Set<Invitation>()
            .OrderByDescending(entity => entity.IssuedOn)
            .ToListAsync(cancellationToken);

        var invitation = InvitationRules.GetActiveInvitationOrThrow(invitations, request.Token, _tokenService);
        return new InvitationPreviewDto(invitation.Email, invitation.TargetRole);
    }
}
