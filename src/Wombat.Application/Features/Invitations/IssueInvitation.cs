using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record IssueInvitationCommand(
    string Email,
    string TargetRole,
    int InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    string IssuedByUserId) : IRequest<IssuedInvitationResult>;

public sealed class IssueInvitationCommandValidator : AbstractValidator<IssueInvitationCommand>
{
    public IssueInvitationCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.TargetRole).NotEmpty().MaximumLength(64);
        RuleFor(command => command.InstitutionId).GreaterThan(0);
        RuleFor(command => command.IssuedByUserId).NotEmpty();
    }
}

public sealed class IssueInvitationCommandHandler : IRequestHandler<IssueInvitationCommand, IssuedInvitationResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IEmailSender _emailSender;

    public IssueInvitationCommandHandler(IApplicationDbContext dbContext, IInvitationTokenService tokenService, IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
    }

    public async Task<IssuedInvitationResult> Handle(IssueInvitationCommand request, CancellationToken cancellationToken)
    {
        var scopeError = InvitationRules.ValidateScope(request.TargetRole, request.InstitutionId, request.SpecialityId, request.SubSpecialityId);
        if (scopeError is not null)
        {
            throw new InvalidOperationException(scopeError);
        }

        var scopeEntityError = await InvitationRules.ValidateScopeEntitiesAsync(
            _dbContext,
            request.InstitutionId,
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
            SpecialityId = request.SpecialityId,
            SubSpecialityId = request.SubSpecialityId,
            IssuedByUserId = request.IssuedByUserId,
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14))
        };

        _dbContext.Set<Invitation>().Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailSender.SendAsync(
            invitation.Email,
            "Your Wombat invitation",
            $"""
            You have been invited to register for Wombat as {invitation.TargetRole}.

            Complete registration:
            /account/register?token={token}

            This link expires on {invitation.ExpiresOn:yyyy-MM-dd}.
            """,
            cancellationToken);

        return new IssuedInvitationResult(invitation.Id, token);
    }
}
