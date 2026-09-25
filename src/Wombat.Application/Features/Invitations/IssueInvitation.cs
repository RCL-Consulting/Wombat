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
    private readonly TimeProvider _timeProvider;

    public IssueInvitationCommandHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IEmailSender emailSender,
        IOptions<WombatOptions> options,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _options = options.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
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

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
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
            IssuedOn = utcNow,
            ExpiresOn = Invitation.LinkExpiresOn(utcNow)
        };

        _dbContext.Set<Invitation>().Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Stored first, then handed over (T283): the mail's key names the invitation's id, which the save gives it, and
        // the worker's report of the mail can then never arrive before the link it is about. Not cancellable once the
        // link is stored: an invitation whose mail was never handed over would read as being sent for an hour.
        await _emailSender.SendAsync(
            InvitationEmail.Build(
                invitation.Email,
                invitation.TargetRole,
                InvitationLinks.RegistrationUrl(_options, token),
                invitation.ExpiresOn,
                Invitation.DeliveryKey(invitation.Id, invitation.TokenHash)),
            CancellationToken.None);

        return new IssuedInvitationResult(invitation.Id, token);
    }
}

/// <summary>The link an account invitation's mail carries. (T283)</summary>
public static class InvitationLinks
{
    /// <summary>The registration page's address with <paramref name="token" />, under the configured base URL.</summary>
    public static string RegistrationUrl(WombatOptions options, string token)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var baseUrl = (options.BaseUrl ?? string.Empty).TrimEnd('/');
        return $"{baseUrl}/account/register?token={Uri.EscapeDataString(token)}";
    }
}
