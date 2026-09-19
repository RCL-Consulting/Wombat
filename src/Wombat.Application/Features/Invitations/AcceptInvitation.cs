using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <remarks>
/// <c>Token</c> and <c>Password</c> are redacted from the audit summary. The AuditPipelineBehavior
/// audits every request whose type name ends in "Command" and AuditPayloadSerializer writes its
/// properties into SummaryJson; both of these are bearer credentials. The password was landing in
/// the audit table in plaintext, and the invitation token is what proves the right to claim an
/// account — a failed accept (wrong password, say) wrote a still-live token into a table any
/// InstitutionalAdmin can search. The names stay in the clear; they identify the row. (T101)
/// </remarks>
public sealed record AcceptInvitationCommand(
    [property: Redact] string Token,
    [property: Redact] string Password,
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
    private readonly IAuditContextProvider _auditContext;

    public AcceptInvitationCommandHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IInvitedUserProvisioner invitedUserProvisioner,
        IAuditContextProvider auditContext)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _invitedUserProvisioner = invitedUserProvisioner;
        _auditContext = auditContext;
    }

    public async Task<AcceptedInvitationResult> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitations = await _dbContext.Set<Invitation>()
            .Where(entity => entity.Email != null)
            .OrderByDescending(entity => entity.IssuedOn)
            .ToListAsync(cancellationToken);

        // The audit row for this command belongs to the institution that issued the invitation, not to
        // whoever's cookie happens to be in the browser. /account/register/submit is AllowAnonymous and
        // signs the new user in only after this handler returns, so the pipeline's principal carries no
        // institution — or, worse, carries the previous registrant's. Declared from the matched
        // invitation, which is server-side truth: the token proves nothing until it matches a stored
        // hash, and the row we stamp from is the one it matched.
        //
        // Matched before the validity checks so that a revoked, used or expired token still lands in the
        // issuing admin's audit log — a stale invitation being retried is precisely what they need to
        // see. A token that matches nothing stays unstamped (Administrator-only), because there is then
        // no institution to attribute it to. So does a CollegeAdmin invitation: audit rows carry an
        // institution, and a college is not one. (T101)
        var matchedInvitation = invitations.FirstOrDefault(
            entity => _tokenService.VerifyToken(request.Token, entity.TokenHash));

        if (matchedInvitation?.InstitutionId is int invitedInstitutionId)
        {
            _auditContext.DeclareInstitution(invitedInstitutionId);
        }

        var invitation = InvitationRules.GetActiveInvitationOrThrow(invitations, request.Token, _tokenService);

        var provisionedUser = await _invitedUserProvisioner.ProvisionAsync(
            invitation.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            invitation.TargetRole,
            invitation.InstitutionId,
            invitation.CollegeId,
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
