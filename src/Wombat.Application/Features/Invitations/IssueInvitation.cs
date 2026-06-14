using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record IssueInvitationCommand(
    string Email,
    string TargetRole,
    int? InstitutionId,
    int? CollegeId,
    int? SpecialityId,
    int? SubSpecialityId,
    string IssuedByUserId,
    ClaimsPrincipal Principal) : IRequest<IssuedInvitationResult>;

public sealed class IssueInvitationCommandValidator : AbstractValidator<IssueInvitationCommand>
{
    public IssueInvitationCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.TargetRole).NotEmpty().MaximumLength(64);
        RuleFor(command => command.IssuedByUserId).NotEmpty();
        // The institution-vs-college scope shape is validated by InvitationRules.ValidateScope,
        // which depends on the target role (a CollegeAdmin has no institution). (T093)
    }
}

public sealed class IssueInvitationCommandHandler : IRequestHandler<IssueInvitationCommand, IssuedInvitationResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly WombatOptions _options;

    public IssueInvitationCommandHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IEmailSender emailSender,
        IOptions<WombatOptions> options)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public async Task<IssuedInvitationResult> Handle(IssueInvitationCommand request, CancellationToken cancellationToken)
    {
        // A CollegeAdmin is provisioned against a national College and is Administrator-only; every
        // other (institution-scoped) role is gated by the caller's institution claim. (T093)
        if (request.TargetRole == WombatRoles.CollegeAdmin)
        {
            if (!request.Principal.IsAdministrator())
            {
                throw new UnauthorizedAccessException("Only an administrator may issue college administrator invitations.");
            }
        }
        else if (!request.InstitutionId.HasValue || !request.Principal.CanAccessInstitution(request.InstitutionId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to issue invitations for that institution.");
        }

        var scopeError = InvitationRules.ValidateScope(request.TargetRole, request.InstitutionId, request.CollegeId, request.SpecialityId, request.SubSpecialityId);
        if (scopeError is not null)
        {
            throw new InvalidOperationException(scopeError);
        }

        var scopeEntityError = await InvitationRules.ValidateScopeEntitiesAsync(
            _dbContext,
            request.InstitutionId,
            request.CollegeId,
            request.SpecialityId,
            request.SubSpecialityId,
            cancellationToken);

        if (scopeEntityError is not null)
        {
            throw new InvalidOperationException(scopeEntityError);
        }

        var token = _tokenService.GenerateToken();
        var invitation = new Invitation
        {
            Email = request.Email.Trim(),
            TokenHash = _tokenService.HashToken(token),
            TargetRole = request.TargetRole,
            InstitutionId = request.InstitutionId,
            CollegeId = request.CollegeId,
            SpecialityId = request.SpecialityId,
            SubSpecialityId = request.SubSpecialityId,
            IssuedByUserId = request.IssuedByUserId,
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14))
        };

        _dbContext.Set<Invitation>().Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var baseUrl = (_options.BaseUrl ?? string.Empty).TrimEnd('/');
        var registrationUrl = $"{baseUrl}/account/register?token={Uri.EscapeDataString(token)}";
        await _emailSender.SendAsync(
            InvitationEmail.Build(invitation.Email, invitation.TargetRole, registrationUrl, invitation.ExpiresOn),
            cancellationToken);

        return new IssuedInvitationResult(invitation.Id, token);
    }
}
