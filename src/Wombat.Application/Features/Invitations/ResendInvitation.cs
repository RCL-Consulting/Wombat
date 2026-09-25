using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <summary>
/// Emails a new link for an account invitation whose mail was not delivered (<see cref="InvitationDelivery.NotDelivered" />),
/// and retires the link it replaces. The invitations list's Resend. (T283)
/// </summary>
/// <remarks>
/// No validator: carries a non-nullable int ID and the caller; the handler authorises the caller as the issue does (a
/// CollegeAdmin invitation is the Administrator's alone), and refuses an invitation that cannot be used or whose mail was
/// not lost.
/// </remarks>
[NoValidator]
public sealed record ResendInvitationCommand(int InvitationId, ClaimsPrincipal Principal) : IRequest<IssuedInvitationResult>;

public sealed class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand, IssuedInvitationResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly WombatOptions _options;
    private readonly TimeProvider _timeProvider;

    public ResendInvitationCommandHandler(
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

    /// <remarks>
    /// <para>
    /// A new link, never the old one again: only a token's hash is stored, so the token first mailed cannot be recovered.
    /// The old link is retired, since the invitation holds one hash; a link the invitee never received has nothing to
    /// keep. The new link works for <see cref="Invitation.LinkLifetimeDays" /> days from the resend, as an issued one does,
    /// and the mail says so. The count of failed mails is kept, so a second failure says to check the address.
    /// </para>
    /// <para>
    /// Every check comes before anything is written (the audit trap). Then one conditioned statement stores the new link,
    /// only while the invitation still holds the link this read found, is neither used nor revoked, and has not been
    /// reported sent. So two resends that race store one link and mail one, and a resend that meets an accept, a revoke
    /// or a report that the mail arrived after all, already committed, stores nothing and sends nothing; each of those is
    /// refused as <see cref="InvitationChanged" />. The mail is handed over only once the link is stored, so the worker's
    /// report of it never arrives before the link it is about (<c>AccountInvitationDeliveryRecorder</c>).
    /// </para>
    /// <para>
    /// The other way round is not guarded: the accept and the revoke read the row and save it tracked, with no concurrency
    /// check. An accept that read the old link before this statement and saves after it marks the invitation used over the
    /// new link, and the new link's mail still goes out, to someone who has registered, as a link that no longer works,
    /// while the administrator is told it is being emailed; a revoke that read before this statement and saves after it
    /// does the same. Both are harmless (the invitation ends as its last writer meant, and no link works that should not),
    /// so no row version was added to <see cref="Invitation" />: it would add refusals to the accept and the revoke.
    /// (T283 review)
    /// </para>
    /// <para>
    /// Neither the statement nor the hand-off is cancellable: a link stored and never mailed would read as being sent for
    /// an hour. Nor is it sent synchronously to the mail server (rejected in T251): that would hold the administrator's
    /// circuit on SMTP.
    /// </para>
    /// </remarks>
    public async Task<IssuedInvitationResult> Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.Set<Invitation>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.InvitationId, cancellationToken)
            ?? throw new InvalidOperationException("The invitation was not found.");

        // As the issue, not the revoke: a resend hands its caller a new registration link, so only whoever may issue the
        // invitation may resend it. A CollegeAdmin invitation is the Administrator's alone: CanAccessCollege would also
        // admit that College's own CollegeAdmin, who could then mint a CollegeAdmin link (T283 review). Every other
        // invitation is institution-scoped.
        var canAccess = invitation.CollegeId.HasValue || invitation.TargetRole == WombatRoles.CollegeAdmin
            ? request.Principal.IsAdministrator()
            : invitation.InstitutionId.HasValue && request.Principal.CanAccessInstitution(invitation.InstitutionId.Value);

        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You do not have permission to resend this invitation.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (invitation.UsedOn.HasValue)
        {
            throw new InvalidOperationException(AlreadyUsed);
        }

        if (invitation.RevokedOn.HasValue)
        {
            throw new InvalidOperationException(Revoked);
        }

        if (invitation.ExpiresOn < DateOnly.FromDateTime(utcNow))
        {
            throw new InvalidOperationException(Expired);
        }

        if (invitation.DeliveryAt(utcNow) != InvitationDelivery.NotDelivered)
        {
            throw new InvalidOperationException(NothingToResend);
        }

        var token = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(token);
        var expiresOn = Invitation.LinkExpiresOn(utcNow);
        var readHash = invitation.TokenHash;

        var stored = await _dbContext.Set<Invitation>()
            .Where(entity =>
                entity.Id == invitation.Id &&
                entity.TokenHash == readHash &&
                entity.UsedOn == null &&
                entity.RevokedOn == null &&
                entity.SentOn == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(entity => entity.TokenHash, tokenHash)
                    .SetProperty(entity => entity.IssuedOn, utcNow)
                    .SetProperty(entity => entity.ExpiresOn, expiresOn)
                    .SetProperty(entity => entity.DeliveryFailedOn, (DateTime?)null),
                CancellationToken.None);

        if (stored == 0)
        {
            throw new InvalidOperationException(InvitationChanged);
        }

        await _emailSender.SendAsync(
            InvitationEmail.Build(
                invitation.Email,
                invitation.TargetRole,
                InvitationLinks.RegistrationUrl(_options, token),
                expiresOn,
                Invitation.DeliveryKey(invitation.Id, tokenHash)),
            CancellationToken.None);

        return new IssuedInvitationResult(invitation.Id, token);
    }

    /// <summary>The refusal of an invitation someone has registered with.</summary>
    public const string AlreadyUsed =
        "This invitation has been used to register, so there is nothing to resend.";

    /// <summary>The refusal of a revoked invitation.</summary>
    public const string Revoked =
        "This invitation has been revoked, so it cannot be resent. Issue a new invitation instead.";

    /// <summary>The refusal of an expired invitation.</summary>
    public const string Expired =
        "This invitation has expired, so it cannot be resent. Issue a new invitation instead.";

    /// <summary>
    /// The refusal when the invitation's mail was delivered or is still being sent: only a mail that did not arrive is
    /// sent again. Nothing was sent. (T283)
    /// </summary>
    public const string NothingToResend =
        "This invitation's email has been delivered or is still being sent, so nothing was sent again.";

    /// <summary>
    /// The refusal when the invitation changed between being read and the store: it was resent elsewhere, accepted,
    /// revoked, or its email was reported delivered. Nothing was sent. (T283)
    /// </summary>
    public const string InvitationChanged =
        "This invitation changed while it was being resent: it was resent elsewhere, used, revoked, or its email was " +
        "delivered after all. Nothing was sent. The list now shows the invitation as it is.";
}
