using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.Curricula;
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
    /// <summary>The refusal of an empty address, in Invite a person's words (T373, N13; T324: no property's name).</summary>
    public const string EmailMissing = "Enter the person's email address.";

    /// <summary>The refusal of a malformed address (T373, N14), the MSF form's words (<c>MsfRespondentAddress.Malformed</c>).</summary>
    public const string EmailMalformed = "Enter a valid email address, for example name@example.org.";

    /// <summary>The refusal of an issue with no role chosen (T373, N25).</summary>
    public const string RoleMissing = "Choose a role.";

    public IssueInvitationCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().WithMessage(EmailMissing);
        RuleFor(command => command.Email).EmailAddress().WithMessage(EmailMalformed)
            .MaximumLength(320).WithMessage(EmailMalformed)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));
        RuleFor(command => command.TargetRole).NotEmpty().WithMessage(RoleMissing).MaximumLength(64);
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
    private readonly IUserAdministrationService _users;
    private readonly WombatOptions _options;
    private readonly TimeProvider _timeProvider;

    public IssueInvitationCommandHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IEmailSender emailSender,
        IUserAdministrationService users,
        IOptions<WombatOptions> options,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _users = users;
        _options = options.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <remarks>
    /// Every check comes before anything is staged (the audit trap, T201): who may issue, the role's scope, the address.
    /// An address is refused when an account holds it (Q12, T286's issue half: the invitee could only ever have been
    /// refused at Register) or when an invitation that still works names it at any institution or College (Q10: two links
    /// to one person). Both compare the address trimmed and whatever its case (S4). Each refusal's message is fixed
    /// generic words, and the holder is named only when judged in the caller's scope (M3,
    /// <see cref="InvitationIssueRefusedException" />).
    /// </remarks>
    public async Task<IssuedInvitationResult> Handle(IssueInvitationCommand request, CancellationToken cancellationToken)
    {
        // A trainee first (T290), as on the Users surface (T278): a registrar who also administers their institution
        // invites nobody.
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        // A CollegeAdmin is provisioned against a national College and is Administrator-only; every
        // other (institution-scoped) role is gated by the caller's institution claim. (T093)
        if (request.TargetRole == WombatRoles.CollegeAdmin)
        {
            if (!request.Principal.IsAdministrator())
            {
                throw new UnauthorizedAccessException("Only an administrator may issue college administrator invitations.");
            }
        }
        else if (request.InstitutionId.HasValue && !request.Principal.CanAccessInstitution(request.InstitutionId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to issue invitations for that institution.");
        }

        // Each scope sentence under its field (T373, Q3; N02, N10, N11), its words unchanged.
        if (InvitationRules.CheckScope(request.TargetRole, request.InstitutionId, request.CollegeId, request.SpecialityId, request.SubSpecialityId)
            is { } scopeRefusal)
        {
            throw InvitationIssueRefusedException.ForScope(scopeRefusal);
        }

        if (await InvitationRules.CheckScopeEntitiesAsync(
                _dbContext,
                request.InstitutionId,
                request.CollegeId,
                request.SpecialityId,
                request.SubSpecialityId,
                cancellationToken) is { } entityRefusal)
        {
            throw InvitationIssueRefusedException.ForScope(entityRefusal);
        }

        var email = request.Email.Trim();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        await RefuseAnAccountHoldingAsync(email, request.Principal, cancellationToken);
        await RefuseAnActiveInvitationToAsync(email, request.Principal, utcNow, cancellationToken);

        var roleAndScope = await InvitationRules.RoleAndScopeAsync(
            _dbContext,
            request.TargetRole,
            request.InstitutionId,
            request.CollegeId,
            request.SpecialityId,
            request.SubSpecialityId,
            cancellationToken);

        var token = _tokenService.GenerateToken();
        var invitation = new Invitation
        {
            // Stored as typed, trimmed (S4): a page shows this spelling; every comparison normalises it.
            Email = email,
            TokenHash = _tokenService.HashToken(token),
            TargetRole = request.TargetRole,
            InstitutionId = request.InstitutionId,
            CollegeId = request.CollegeId,
            SpecialityId = request.SpecialityId,
            SubSpecialityId = request.SubSpecialityId,
            IssuedByUserId = request.IssuedByUserId,
            // The role the issue was authorised as (T373, F3), from the principal, never from the page: the page admits
            // no one else (AdministratorOrInstitutionalAdmin). An Administrator acting as an Institutional admin is
            // recorded as Administrator, so the pending registrar's Home shows no "Invited by" line for that issue.
            IssuedAsRole = request.Principal.IsAdministrator() ? WombatRoles.Administrator : WombatRoles.InstitutionalAdmin,
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
                roleAndScope,
                InvitationLinks.RegistrationUrl(_options, token),
                invitation.ExpiresOn,
                Invitation.DeliveryKey(invitation.Id, invitation.TokenHash)),
            CancellationToken.None);

        return new IssuedInvitationResult(invitation.Id, token) { ExpiresOn = invitation.ExpiresOn };
    }

    /// <summary>
    /// Q12: an account holds the address. In the caller's scope (an Administrator; an Institutional admin, for an account
    /// at their institution) the refusal carries the account's id and name, for N15's link to it; outside it, nothing.
    /// </summary>
    private async Task RefuseAnAccountHoldingAsync(string email, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (await _users.FindByEmailAsync(email, cancellationToken) is not { } holder)
        {
            return;
        }

        var inScope = principal.IsAdministrator() ||
                      (holder.InstitutionId is int institutionId && principal.CanAccessInstitution(institutionId));

        throw new InvitationIssueRefusedException(
            InvitationIssueRefusal.AccountExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse)
        {
            InCallerScope = inScope,
            HolderUserId = inScope ? holder.UserId : null,
            HolderName = inScope ? $"{holder.FirstName} {holder.LastName}".Trim() : null
        };
    }

    /// <summary>
    /// Q10: an invitation that still works names the address: not used, not revoked, not past its last day, at any
    /// institution or College (the lookup is unfiltered; only what the refusal says is the caller's scope's). In scope the
    /// refusal carries its issue time and last day, for N17; outside it, only whether it is a College's (N18).
    /// </summary>
    private async Task RefuseAnActiveInvitationToAsync(
        string email,
        ClaimsPrincipal principal,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var today = ProgrammeCalendar.DateOf(utcNow);
        var active = await _dbContext.Set<Invitation>()
            .AsNoTracking()
            .ToAddress(email)
            .Where(entity => entity.UsedOn == null && entity.RevokedOn == null && entity.ExpiresOn >= today)
            .OrderByDescending(entity => entity.IssuedOn)
            .Select(entity => new { entity.InstitutionId, entity.CollegeId, entity.IssuedOn, entity.ExpiresOn })
            .FirstOrDefaultAsync(cancellationToken);

        if (active is null)
        {
            return;
        }

        var inScope = InvitationRules.IsInScope(principal, active.InstitutionId, active.CollegeId);
        throw new InvitationIssueRefusedException(
            InvitationIssueRefusal.ActiveInvitationExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse)
        {
            InCallerScope = inScope,
            ActiveIssuedOn = inScope ? active.IssuedOn : null,
            ActiveExpiresOn = inScope ? active.ExpiresOn : null,
            HeldByCollege = !inScope && active.CollegeId.HasValue
        };
    }
}
