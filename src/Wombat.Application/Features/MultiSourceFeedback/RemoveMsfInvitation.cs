using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// Removes one invitee from a draft campaign: an address added by mistake, or in the wrong group. (T247)
/// </summary>
/// <remarks>
/// <para>
/// Until T247 an invitee, once added, stayed: the only remedy was to withdraw the campaign and create it again. The
/// campaign page reaches one invitee through its draft's list of addresses
/// (<see cref="MsfCampaignSetupDto.DraftInvitees" />), which is read only while the campaign is a draft, and so is this
/// command: a draft's invitees hold no working link and have given no response, so removing one takes nothing from
/// anyone. (An open that failed, or was refused at its save, may have mailed them a link, but no such link works: the
/// open stored none of them, T184.) Once the campaign opens, every invitee has been mailed a link that works, and the
/// page counts them by group only (T217).
/// </para>
/// <para>
/// The invitation is named by its id, never by its address, so the audit row the pipeline writes for this command holds
/// no address at all: it records the campaign, the invitation id and who removed it. A respondent is never named on an
/// audit row (T184, T205), and none of the refusals below names one either, since the row keeps a refusal's message.
/// The id names a row this command deletes, so the row records only that an invitee was removed, and when.
/// </para>
/// </remarks>
public sealed record RemoveMsfInvitationCommand(int CampaignId, int InvitationId, ClaimsPrincipal Principal) : IRequest;

public sealed class RemoveMsfInvitationCommandValidator : AbstractValidator<RemoveMsfInvitationCommand>
{
    public RemoveMsfInvitationCommandValidator()
    {
        RuleFor(command => command.CampaignId).GreaterThan(0);
        RuleFor(command => command.InvitationId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RemoveMsfInvitationCommandHandler : IRequestHandler<RemoveMsfInvitationCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveMsfInvitationCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(RemoveMsfInvitationCommand request, CancellationToken cancellationToken)
    {
        // Before anything is loaded to be touched, and before the state and invitee checks, whose messages would describe
        // another institution's campaign to someone who may not see it. (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

        var campaign = await _dbContext.Set<MsfCampaign>()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.CampaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");

        // The state first, as the add asks it before the address (T228): once the campaign has opened, which of its
        // invitees an id names is asked of nobody, so an id that names nothing, or another campaign's invitee, gets this
        // refusal too, and the answer says nothing about which ids the campaign holds. Worded by what happened to it, so
        // a campaign withdrawn while still a draft is not told that its invitees were mailed a link. (T247 review)
        if (campaign.State != MsfCampaignState.Draft)
        {
            throw new InvalidOperationException(
                campaign.State == MsfCampaignState.Withdrawn ? OnlyFromADraftWithdrawn : OnlyFromADraft);
        }

        // Asked of this campaign's invitations only, so an id that names another campaign's invitee is refused in the
        // words an id that names nothing is, and says nothing about that campaign.
        var invitation = await _dbContext.Set<MsfInvitation>()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == request.InvitationId && candidate.CampaignId == campaign.Id,
                cancellationToken)
            ?? throw new InvalidOperationException(NotInvited);

        // Every check above changes nothing, so a refusal leaves nothing tracked for the audit pipeline's write to commit.
        _dbContext.Set<MsfInvitation>().Remove(invitation);

        // The campaign is marked modified, though nothing about it changes, so that this save writes the campaign row and
        // is checked against its xmin token (MsfCampaignConfiguration), as the add's is (T206). An open that commits after
        // the campaign was read above refuses this save whole, so an invitee who has been mailed a link keeps their
        // invitation, and with it any response they give: an invitation's responses go with it (MsfResponseConfiguration).
        // And a remove that commits while an open is mailing refuses the open's save, so no link that open mailed is
        // stored, and opening again mails only the invitees left. (MsfRemoveInviteePostgresTests)
        _dbContext.Set<MsfCampaign>().Entry(campaign).Property(candidate => candidate.State).IsModified = true;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201): the refused delete is not sent again with it.
            throw new InvalidOperationException(CampaignChanged, exception);
        }
    }

    /// <summary>
    /// The refusal of a campaign that has opened since the page read it: open, closed, under review or released. (T247)
    /// </summary>
    public const string OnlyFromADraft =
        "Invitees can only be removed while a campaign is in draft. This one has been opened, and each invitee has been " +
        "emailed a link to respond. Nothing has been changed.";

    /// <summary>
    /// The refusal of a campaign that has been withdrawn, from a draft or once open. It says nothing of links, since a
    /// campaign withdrawn as a draft mailed none. (T247 review)
    /// </summary>
    public const string OnlyFromADraftWithdrawn =
        "Invitees can only be removed while a campaign is in draft. This one has been withdrawn: it takes no responses, " +
        "and its invitees' addresses have been removed. Nothing has been changed.";

    /// <summary>
    /// The refusal of an invitation id this campaign does not hold: removed already, in another tab, or never one of its
    /// invitees. It names no address: the audit row keeps the message. (T247)
    /// </summary>
    public const string NotInvited =
        "That invitee is no longer on this campaign: they may have been removed in another tab. Nothing has been changed.";

    /// <summary>
    /// The refusal when the campaign changed between being read and the save: it was opened or withdrawn, or its invitees
    /// were changed at the same moment. Nothing is removed. (T247)
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while this invitee was being removed: it was opened or withdrawn, or an invitee was added " +
        "or removed at the same moment. The invitee has not been removed. If the campaign is still a draft and still " +
        "lists them, remove them again.";
}
