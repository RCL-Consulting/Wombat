using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// Sends a new link to every respondent of an open campaign whose link was not delivered
/// (<see cref="MsfInvitation.LinkNotDelivered" />), and answers how many were sent. The campaign page's Resend. (T251)
/// </summary>
/// <remarks>
/// No validator: carries a non-nullable int ID and the caller; the handler authorises the caller against the campaign's
/// subject (T113) and refuses a campaign that is not open. It names nobody: the page is told how many links were not
/// delivered, never whose, and the resend reaches exactly those (T217).
/// </remarks>
[NoValidator]
public sealed record ResendMsfLinksCommand(int CampaignId, ClaimsPrincipal Principal) : IRequest<int>;

public sealed class ResendMsfLinksCommandHandler : IRequestHandler<ResendMsfLinksCommand, int>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IInvitationTokenService _tokenService;
    private readonly IUserAdministrationService _users;
    private readonly WombatOptions _options;
    private readonly TimeProvider _timeProvider;

    public ResendMsfLinksCommandHandler(
        IApplicationDbContext dbContext,
        IEmailSender emailSender,
        IInvitationTokenService tokenService,
        IUserAdministrationService users,
        IOptions<WombatOptions> options,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tokenService = tokenService;
        _users = users;
        _options = options.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <remarks>
    /// <para>
    /// A new link, as a reminder issues one (T214), never the old one again: only a link's selector and hash are stored,
    /// so the token it was mailed with cannot be recovered. The link replaced is kept as the previous link, which still
    /// takes the respondent's one response, unless its own mail was reported dropped, or nothing was heard of it and the
    /// link before it was reported sent (<see cref="MsfInvitation.ReplaceLink" />): the link the respondent is known or
    /// likeliest to hold, and may be answering through, is the one that stays.
    /// </para>
    /// <para>
    /// In the open's order, and for the open's reasons (T184, the audit trap): every link is minted and handed over
    /// before any invitation is touched, then all are stored in one save, checked against the campaign's xmin token.
    /// A hand-off that fails leaves every invitation as it was, and resending again sends each a new link. A close, a
    /// withdrawal or a reminder that saved while this was sending moves that token, and this save is refused whole; so
    /// is one that finds a respondent answered meanwhile through the link kept
    /// (<c>CK_MsfInvitations_PreviousLinkUnanswered</c>). Either way the links this attempt sent open nothing, and the
    /// refusal says so.
    /// </para>
    /// <para>
    /// Not sent synchronously to the mail server (rejected in T251): that would hold the coordinator's circuit on SMTP,
    /// and a failure half-way through the list would leave some respondents mailed and none of their links stored.
    /// </para>
    /// </remarks>
    public async Task<int> Handle(ResendMsfLinksCommand request, CancellationToken cancellationToken)
    {
        var respondUrl = _options.RequireMsfRespondUrl();

        // Before anything is loaded to be touched, and before the state check, whose message would describe another
        // institution's campaign to someone who may not see it. (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

        var campaign = await _dbContext.Set<MsfCampaign>()
            .Include(candidate => candidate.Template)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.CampaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");

        if (campaign.State != MsfCampaignState.Open)
        {
            throw new InvalidOperationException(OnlyOpenCampaigns);
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        // The one rule the page counts by, as one statement: the page is never sent more than the number (T217).
        var undelivered = (await _dbContext.Set<MsfInvitation>()
                .Where(invitation => invitation.CampaignId == campaign.Id)
                .Where(MsfInvitation.LinkNotDelivered(utcNow))
                .OrderBy(invitation => invitation.Id)
                .ToListAsync(cancellationToken))
            // As the open mails only an invitation with an address: the add refuses one without.
            .Where(invitation => !string.IsNullOrWhiteSpace(invitation.RespondentEmail))
            .ToList();

        if (undelivered.Count == 0)
        {
            throw new InvalidOperationException(NothingToResend);
        }

        // Whom the links ask about, before the first mail, as the open resolves it (T202).
        var names = await _users.GetDisplayNamesAsync([campaign.SubjectUserId], cancellationToken);
        if (!names.TryGetValue(campaign.SubjectUserId, out var traineeName) || string.IsNullOrWhiteSpace(traineeName))
        {
            throw new InvalidOperationException(TraineeHasNoName);
        }

        var links = undelivered
            .Select(invitation => (Invitation: invitation, Token: _tokenService.GenerateSelectorToken()))
            .ToList();

        foreach (var link in links)
        {
            var responseUrl = $"{respondUrl}?token={Uri.EscapeDataString(link.Token.Token)}";

            try
            {
                // The invitation's own words: most of these respondents were never sent one.
                await _emailSender.SendAsync(
                    MsfInvitationEmail.Build(new MsfInvitationEmailContent(
                        campaign.Id,
                        link.Invitation.RespondentEmail!,
                        traineeName.Trim(),
                        campaign.Template.Name,
                        campaign.OpensOn,
                        campaign.ClosesOn,
                        link.Invitation.ExpiresOn,
                        responseUrl,
                        campaign.Template.Kind,
                        MsfInvitation.DeliveryKey(link.Invitation.Id, link.Token.Selector))),
                    cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Replaced rather than passed on, as the open does (T184): the audit row keeps a refusal's message, and a
                // sender's own may name the respondent's address.
                throw new InvalidOperationException(LinksNotSent, exception);
            }
        }

        // From here to the save nothing can throw, and the save is not cancellable: the mail has been handed over.
        foreach (var link in links)
        {
            link.Invitation.ReplaceLink(link.Token.Selector, link.Token.Hash, utcNow);
        }

        // Written, unchanged, so the save is checked against the campaign's xmin token (MsfCampaignConfiguration), which
        // a close, a withdrawal and a reminder's store all move.
        _dbContext.Set<MsfCampaign>().Entry(campaign).Property(candidate => candidate.State).IsModified = true;

        try
        {
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (DbUpdateException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201).
            throw new InvalidOperationException(CampaignChanged, exception);
        }

        return links.Count;
    }

    /// <summary>The refusal of a campaign that is not open: a draft has sent no link, and a closed one takes no answer.</summary>
    public const string OnlyOpenCampaigns =
        "Links are sent again only while the campaign is open. A draft has not sent any yet, and a campaign that has " +
        "closed or been withdrawn takes no more responses.";

    /// <summary>
    /// The refusal when no link awaits sending again: each was delivered or is still being sent, or its respondent has
    /// answered. Nothing was sent. (T251)
    /// </summary>
    public const string NothingToResend =
        "No link needs sending again: each one has been delivered or is still being sent, or its respondent has " +
        "answered. Nothing was sent.";

    /// <summary>The refusal when a link could not be handed over for sending. No link has changed. (T251)</summary>
    public const string LinksNotSent =
        "The links could not all be sent again, so none has changed. Resend them again; any link sent before this " +
        "failure will not work.";

    /// <summary>The refusal when the trainee has no name to put in a link's mail. Nothing has been sent. (T202, T251)</summary>
    public const string TraineeHasNoName =
        "The trainee this campaign is about has no name on record, so the links could not say whom the feedback is " +
        "for. Nothing has been sent. Ask an administrator to add the trainee's name, then resend the links.";

    /// <summary>
    /// The refusal when the campaign changed between being read and the save: it was closed or withdrawn, a reminder or
    /// another resend replaced a link, or a respondent answered. No link has changed. (T251) Shown on the campaign page,
    /// which reads the campaign again and shows what it is now.
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while its links were being sent again: it was closed or withdrawn, a respondent answered, " +
        "or a link was sent again elsewhere. No link has changed, and any link this attempt sent will not work. If links " +
        "are still shown as not delivered, resend them again.";
}
