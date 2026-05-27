using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record AcceptInvitationCommand(
    string Token,
    string Password,
    string FirstName,
    string LastName) : IRequest<AcceptedInvitationResult>;

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
    }
}

public sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, AcceptedInvitationResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IInvitedUserProvisioner _invitedUserProvisioner;

    public AcceptInvitationCommandHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IInvitedUserProvisioner invitedUserProvisioner)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _invitedUserProvisioner = invitedUserProvisioner;
    }

    public async Task<AcceptedInvitationResult> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitations = await _dbContext.Set<Invitation>()
            .Where(entity => entity.Email != null)
            .OrderByDescending(entity => entity.IssuedOn)
            .ToListAsync(cancellationToken);

        var invitation = InvitationRules.GetActiveInvitationOrThrow(invitations, request.Token, _tokenService);

        var provisionedUser = await _invitedUserProvisioner.ProvisionAsync(
            invitation.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            invitation.TargetRole,
            invitation.InstitutionId,
            invitation.SpecialityId,
            invitation.SubSpecialityId,
            cancellationToken);

        var now = DateTime.UtcNow;
        invitation.UsedOn = now;

        // T061: sweep stale invitations for the same email. Once a user is provisioned,
        // any other active invitation for that email cannot be accepted (the registration
        // path rejects "user already exists"), so leaving them open is just clutter.
        // Multi-role onboarding goes through /admin/users after first registration.
        var staleInvitations = await _dbContext.Set<Invitation>()
            .Where(entity =>
                entity.Email == invitation.Email &&
                entity.Id != invitation.Id &&
                !entity.UsedOn.HasValue &&
                !entity.RevokedOn.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var stale in staleInvitations)
        {
            stale.RevokedOn = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AcceptedInvitationResult(provisionedUser.UserId, provisionedUser.AssignedRole);
    }
}
