using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

using Wombat.Application.Common;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// No validator: carries a non-nullable int ID and the caller; the handler authorises the caller against the
/// campaign's subject (T113) and validates the state transition.
/// </summary>
[NoValidator]
public sealed record OpenMsfCampaignCommand(int CampaignId, ClaimsPrincipal Principal) : IRequest;

public sealed class OpenMsfCampaignCommandHandler : IRequestHandler<OpenMsfCampaignCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IInvitationTokenService _tokenService;
    private readonly IUserAdministrationService _users;
    private readonly WombatOptions _options;

    public OpenMsfCampaignCommandHandler(
        IApplicationDbContext dbContext,
        IEmailSender emailSender,
        IInvitationTokenService tokenService,
        IUserAdministrationService users,
        IOptions<WombatOptions> options)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tokenService = tokenService;
        _users = users;
        _options = options.Value;
    }

    public async Task Handle(OpenMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var respondUrl = _options.RequireMsfRespondUrl();

        // Before anything is loaded to be touched: opening rotates every invitation token and mails each respondent.
        // (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

        var campaign = await _dbContext.Set<MsfCampaign>()
            .Include(candidate => candidate.Template)
            .Include(candidate => candidate.Invitations)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.CampaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");

        if (campaign.Invitations.Count == 0)
        {
            throw new InvalidOperationException("At least one respondent invitation is required before opening a campaign.");
        }

        // Refused before the first mail, not by Open() after it: an open campaign must not mail a second round of
        // links that its stored tokens do not match. (T184)
        campaign.EnsureCanOpen();

        // Whom the invitations ask about, resolved before the first mail for the same reason as the check above. (T202)
        var traineeName = await ResolveTraineeNameAsync(campaign.SubjectUserId, cancellationToken);

        // Every link is minted and every respondent mailed BEFORE the campaign or any invitation is touched. (T184)
        //
        // The audit pipeline saves this request's DbContext from its catch, so a throw with a mutation pending commits
        // it. Until T184 the campaign was opened first and each token rotated just before its own mail, so a send that
        // threw committed an open campaign whose respondents after the failure held no link at all. Nothing could
        // repair that: Open() refuses a campaign that is not a draft, and there is no resend.
        //
        // Sending after the save was the alternative, and it strands respondents the same way: the open is committed,
        // an unmailed respondent is unreachable, and making that right needs a durable outbox or a resend command.
        // This order fails the other way. A failed send leaves the campaign a draft with every stored token as it was,
        // and opening it again mails everyone a fresh link. The cost is that a respondent mailed before the failure
        // holds a link that will not work, and the refusal says so. In production a "send" is an enqueue on an
        // in-process channel (QueuedEmailSender), which throws only when cancelled or shut down; SMTP delivery happens
        // after the request whichever order is chosen.
        var links = campaign.Invitations
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.RespondentEmail))
            .Select(invitation => (Invitation: invitation, Token: _tokenService.GenerateSelectorToken()))
            .ToList();

        foreach (var link in links)
        {
            var submitUrl = $"{respondUrl}?token={Uri.EscapeDataString(link.Token.Token)}";

            try
            {
                await _emailSender.SendAsync(
                    MsfInvitationEmail.Build(new MsfInvitationEmailContent(
                        campaign.Id,
                        link.Invitation.RespondentEmail!,
                        traineeName,
                        campaign.Template.Name,
                        campaign.OpensOn,
                        campaign.ClosesOn,
                        link.Invitation.ExpiresOn,
                        submitUrl,
                        campaign.Template.Kind)),
                    cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Replaced rather than passed on, for two readers. The coordinator is told what state the campaign is
                // in. And the audit row records a refusal's message: a sender's own message may name the respondent's
                // address, which is exactly what AddMsfInvitationCommand keeps out of that log. (T184)
                throw new InvalidOperationException(InvitationsNotSent, exception);
            }
        }

        // From here to the save nothing can throw: every hash is already computed, and EnsureCanOpen() above is the
        // only refusal Open() has. The save is not cancellable, because the mail has been handed over and cannot be
        // recalled: a cancellation now would strand every link just sent.
        //
        // The save itself can still fail, and then the links this request sent are dead. Two opens that race (a
        // double-click can) both pass EnsureCanOpen() and both mail. The hashes and the open go in this one save, under
        // the campaign's xmin token, so the second save is refused whole: the first open's links are the stored ones,
        // and a racing open cannot overwrite them (MsfOpenCampaignRacePostgresTests). The refused request keeps its
        // failure audit row and gets CampaignChanged, which carries the concurrency error: the audit pipeline discards
        // refused changes before writing (T201, AuditOnRefusedSavePostgresTests).
        //
        // The same token refuses an open that an added invitee raced (T206). Adding an invitee writes the campaign row
        // (AddMsfInvitationCommandHandler), so an invitee stored after this open read the invitations it mails changes
        // the xmin this save is checked against: the open is refused, and opening again mails them too. An add that read
        // the draft before this save commits, and saves after it, is refused at its own save the same way
        // (MsfInviteDuringOpenRacePostgresTests).
        var openedAt = DateTime.UtcNow;

        foreach (var link in links)
        {
            // Stamped as issued now, so the reminder job does not replace a link mailed inside its window (T206).
            link.Invitation.IssueLink(link.Token.Selector, link.Token.Hash, openedAt);
        }

        campaign.Open(openedAt);

        try
        {
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201). EF's own message names row counts; the coordinator needs to know what to do next.
            throw new InvalidOperationException(CampaignChanged, exception);
        }
    }

    /// <summary>
    /// The refusal when the campaign changed between being read and the save: an invitee was added, or it was opened or
    /// withdrawn elsewhere. Nothing this attempt did is stored. (T206)
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while it was being opened: an invitee was added, or it was opened or withdrawn elsewhere. " +
        "This attempt did not open it, and any link it sent will not work. If the campaign is still a draft on the " +
        "campaigns list, open it again: every respondent, including anyone just added, is sent a new link.";

    /// <summary>The refusal when an invitation could not be sent. Nothing about the campaign has changed. (T184)</summary>
    public const string InvitationsNotSent =
        "The invitations could not all be sent, so the campaign has not been opened. Open it again to send every " +
        "respondent a new link; any link sent before this failure will not work.";

    /// <summary>The refusal when the trainee has no name to put in an invitation. Nothing has been sent. (T202)</summary>
    public const string TraineeHasNoName =
        "The trainee this campaign is about has no name on record, so the invitations could not say whom the feedback " +
        "is for. Nothing has been sent. Ask an administrator to add the trainee's name, then open the campaign again.";

    /// <summary>
    /// The trainee's name as the invitations give it, or a refusal when there is none. (T202)
    /// </summary>
    /// <remarks>
    /// A refusal rather than a fallback. Elsewhere the product shows a user id where no name exists (T142), but an id
    /// in an email means nothing to its reader and names an internal record to someone outside the product, and a
    /// feedback request that does not say whom it is about cannot be answered.
    /// </remarks>
    private async Task<string> ResolveTraineeNameAsync(string subjectUserId, CancellationToken cancellationToken)
    {
        var names = await _users.GetDisplayNamesAsync([subjectUserId], cancellationToken);

        return names.TryGetValue(subjectUserId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : throw new InvalidOperationException(TraineeHasNoName);
    }
}
