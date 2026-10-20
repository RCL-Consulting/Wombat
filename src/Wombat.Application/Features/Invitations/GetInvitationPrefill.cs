using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Curricula;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <summary>
/// What Invite a person is filled from when it is opened with <c>?from=‹id›</c> (T373, flow 11, N19, N20, Q9): the revoked
/// invitation the list's "Invite another address" names, or the expired one its "Invite again" names. Null for an id that
/// names nothing, is outside the caller's scope, or is neither revoked nor expired unused: the page then fills nothing and
/// says nothing, so an id another institution holds reads as one that does not exist.
/// </summary>
public sealed record GetInvitationPrefillQuery(int InvitationId, ClaimsPrincipal Principal) : IRequest<InvitationPrefillDto?>;

public sealed class GetInvitationPrefillQueryHandler : IRequestHandler<GetInvitationPrefillQuery, InvitationPrefillDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public GetInvitationPrefillQueryHandler(IApplicationDbContext dbContext, TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<InvitationPrefillDto?> Handle(GetInvitationPrefillQuery request, CancellationToken cancellationToken)
    {
        // A trainee first (T290): asked before the id is read, so it says nothing about the id.
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var invitation = await _dbContext.Set<Invitation>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.InvitationId, cancellationToken);

        if (invitation is null || !InvitationRules.IsInScope(request.Principal, invitation.InstitutionId, invitation.CollegeId))
        {
            return null;
        }

        InvitationPrefillSource? source = invitation switch
        {
            { UsedOn: not null } => null,
            { RevokedOn: not null } => InvitationPrefillSource.Revoked,
            _ when Invitation.HasPassedLastDay(invitation.ExpiresOn, _timeProvider.GetUtcNow().UtcDateTime)
                => InvitationPrefillSource.Expired,
            _ => null
        };

        return source is { } filledFrom
            ? new InvitationPrefillDto(
                invitation.Id,
                filledFrom,
                invitation.Email,
                invitation.TargetRole,
                invitation.InstitutionId,
                invitation.CollegeId,
                invitation.SpecialityId,
                invitation.SubSpecialityId,
                invitation.ExpiresOn)
            : null;
    }
}
