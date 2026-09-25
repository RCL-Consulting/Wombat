using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Users.Commands.RevokePendingInvitationsForEmail;

public sealed record RevokePendingInvitationsForEmailCommand(string Email, ClaimsPrincipal Principal) : IRequest<int>;

public sealed class RevokePendingInvitationsForEmailCommandValidator : AbstractValidator<RevokePendingInvitationsForEmailCommand>
{
    public RevokePendingInvitationsForEmailCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}

public sealed class RevokePendingInvitationsForEmailCommandHandler : IRequestHandler<RevokePendingInvitationsForEmailCommand, int>
{
    private readonly IApplicationDbContext _dbContext;

    public RevokePendingInvitationsForEmailCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Someone who holds Trainee revokes nothing, and is refused before anything is read (T278,
    /// <see cref="UserAdministrationRules.DemandUserAdministration" />). The caller's own email is not refused
    /// (<see cref="UserAdministrationRules.IsCaller" />).
    /// </remarks>
    public async Task<int> Handle(RevokePendingInvitationsForEmailCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var normalisedEmail = request.Email.Trim();

        var invitations = await _dbContext.Set<Invitation>()
            .Where(entity =>
                entity.Email == normalisedEmail &&
                !entity.UsedOn.HasValue &&
                !entity.RevokedOn.HasValue)
            .ToListAsync(cancellationToken);

        if (invitations.Count == 0)
        {
            return 0;
        }

        // Caller must be able to access every invitation's institution. Administrators pass through;
        // InstitutionalAdmins are blocked if any invitation is for a different institution.
        foreach (var invitation in invitations)
        {
            // CollegeAdmin invitations are college-scoped (Administrator-only); the rest institution-scoped. (T093)
            var canAccess = invitation.CollegeId.HasValue
                ? request.Principal.CanAccessCollege(invitation.CollegeId.Value)
                : invitation.InstitutionId.HasValue && request.Principal.CanAccessInstitution(invitation.InstitutionId.Value);

            if (!canAccess)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to revoke invitations outside your institution.");
            }
        }

        var now = DateTime.UtcNow;
        foreach (var invitation in invitations)
        {
            invitation.RevokedOn = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return invitations.Count;
    }
}
