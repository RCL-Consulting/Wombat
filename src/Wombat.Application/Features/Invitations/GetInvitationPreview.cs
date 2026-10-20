using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Institutions;
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
    private readonly TimeProvider _timeProvider;

    /// <param name="timeProvider">What a link's last day is judged against (T373, F7); the system clock unless a test pins it.</param>
    public GetInvitationPreviewQueryHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IInvitedUserProvisioner provisioner,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _provisioner = provisioner;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<InvitationPreviewDto> Handle(GetInvitationPreviewQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _dbContext.Set<Invitation>()
            .OrderByDescending(entity => entity.IssuedOn)
            .ToListAsync(cancellationToken);

        // The invitation's own state first: an accepted invitation's address is held by the account it created, and it
        // reads as used, not as taken.
        Invitation invitation;
        try
        {
            invitation = InvitationRules.GetActiveInvitationOrThrow(
                invitations, request.Token, _tokenService, _timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (InvitationRefusedException refused) when (refused.Reason == InvitationRefusal.Expired)
        {
            // The card names the day the link worked until (T373, flow 11, G10).
            var expired = invitations.First(entity => _tokenService.VerifyToken(request.Token, entity.TokenHash));
            throw new InvitationRefusedException(InvitationRefusal.Expired) { LastDay = expired.ExpiresOn };
        }

        var status = await _provisioner.GetAddressStatusAsync(invitation.Email, cancellationToken);
        if (InvitationRefusals.For(status) is { } refusal)
        {
            throw new InvitationRefusedException(refusal);
        }

        // The scope's names and the link's last day, for the card's "as ‹role and scope›" and its note (T373, flow 11, Q6, F7).
        var institutionName = invitation.InstitutionId is int institutionId
            ? await _dbContext.Set<Institution>().Where(entity => entity.Id == institutionId).Select(entity => entity.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var collegeName = invitation.CollegeId is int collegeId
            ? await _dbContext.Set<College>().Where(entity => entity.Id == collegeId).Select(entity => entity.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var specialityName = invitation.SpecialityId is int specialityId
            ? await _dbContext.Set<Speciality>().Where(entity => entity.Id == specialityId).Select(entity => entity.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;
        var subSpecialityName = invitation.SubSpecialityId is int subSpecialityId
            ? await _dbContext.Set<SubSpeciality>().Where(entity => entity.Id == subSpecialityId).Select(entity => entity.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        return new InvitationPreviewDto(
            invitation.Email,
            invitation.TargetRole,
            institutionName,
            collegeName,
            specialityName,
            subSpecialityName,
            invitation.ExpiresOn);
    }
}
