using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// What the campaign page needs to show one campaign and offer what its state allows: whose it is, its questionnaire's
/// kind, its state and window, whom it may invite, and how many were invited and have responded in each respondent group;
/// or null when there is no such campaign or the caller does not run it. (T164, T217)
/// </summary>
/// <remarks>
/// Null for both, as <see cref="GetCampaignAggregateReportQuery" /> answers (T113): the page is reached by a campaign id
/// in the address bar. Who runs it is <see cref="MsfCampaignRules.IsSubjectInScopeAsync" />, so never the trainee it is
/// about, whose own role would otherwise show them each group's responses coming in before release, and never anyone
/// who holds Trainee (T224). Once the campaign has opened, nothing that names a respondent is read: no address, and no
/// row per invitee (see <see cref="MsfCampaignSetupDto.Invitees" />). A draft's addresses are read, so that one added by
/// mistake can be removed (<see cref="MsfCampaignSetupDto.DraftInvitees" />, T247).
/// </remarks>
public sealed record GetMsfCampaignSetupQuery(int CampaignId, ClaimsPrincipal Principal) : IRequest<MsfCampaignSetupDto?>;

/// <param name="AcceptedCategories">
/// The respondent categories this campaign's questionnaire accepts (<c>MsfTemplate.Accepts</c>), which the invite command
/// holds it to: Learner alone for learner feedback, every other category for multi-source feedback.
/// </param>
public sealed record MsfCampaignSetupDto(
    int CampaignId,
    string TemplateName,
    MsfTemplateKind Kind,
    MsfCampaignState State,
    IReadOnlyList<MsfRespondentCategory> AcceptedCategories)
{
    /// <summary>
    /// Whose feedback it is, by name (<see cref="UserDisplayNames.NameOf" />), or their id when they have no name on
    /// record. (T142, T217)
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>The first day of the response window. (T217)</summary>
    public DateOnly OpensOn { get; init; }

    /// <summary>
    /// The last day of the response window: every respondent's last day to respond, and the day after which the
    /// auto-close job closes the campaign. (T217)
    /// </summary>
    public DateOnly ClosesOn { get; init; }

    /// <summary>
    /// How many were invited, and how many have responded, in each respondent group that has an invitee, in the
    /// category's order. Empty while nobody has been invited. (T217)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Counts by group, never a row per invitee, in every state. A row per invitee is a re-identification oracle even
    /// without its address: the coordinator added the invitees and knows which row is whom, so a "responded" mark
    /// appearing on one row beside a report that has just gained one comment names that comment's author.
    /// </para>
    /// <para>
    /// A count by group adds little to what the campaign's report already shows whoever runs it: the total responses, a
    /// card for each group that has responded, and, for a group at or above the category threshold, its answers and how
    /// many gave them. What it does add, until release, is the exact count of a group below the threshold, whose card the
    /// report shows but whose count it hides. That names nobody, and with a single such group it is already the total
    /// less the other groups' counts. (The portfolio PDF prints every group's count, suppressed ones included, but only
    /// for a released campaign.) Nor does a count make an answer untraceable: a campaign may set its category threshold
    /// at one, and then its report shows a one-person group's answers, whatever this page shows. (T217 review)
    /// </para>
    /// <para>
    /// Responses are counted as the report counts them (<see cref="IMsfAggregationService.BuildReport" />): each
    /// response under its invitation's category. An invitation stays counted once its campaign has closed or been
    /// withdrawn: anonymising it erases the address, not the category.
    /// </para>
    /// </remarks>
    public IReadOnlyList<MsfInviteeCountDto> Invitees { get; init; } = [];

    /// <summary>
    /// A draft's invitees one by one, by the address the coordinator typed, in the order they were added; empty in every
    /// other state. What the campaign page's Remove reaches an invitee through (<see cref="RemoveMsfInvitationCommand" />).
    /// (T247)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not the re-identification oracle <see cref="Invitees" /> refuses to be. That is a row per invitee beside the
    /// responses coming in; a draft's invitees hold no working link and have given no response, so a row here says
    /// nothing about anyone's answers.
    /// </para>
    /// <para>
    /// It does show the addresses to whoever runs the campaign (<see cref="MsfCampaignRules.IsSubjectInScopeAsync" />),
    /// which is any coordinator at the trainee's institution and any Administrator, not only the one who typed them. That
    /// is accepted (T247 review): whoever runs a campaign may add to it, open it and read its report, and knowing who was
    /// invited is part of running it. It is not nothing: a second coordinator who saw the draft knows, as the one who
    /// typed the addresses does, who a one-person group is, and a campaign whose category threshold is one shows that
    /// group's answers on its report (T217 review).
    /// </para>
    /// <para>
    /// Read by a statement of its own, sent only when the campaign was read as a draft, so no statement the page sends for
    /// a campaign that has opened asks for an address (T217). That statement asks the campaign row again, so a campaign
    /// opened between the two reads lists no address. Once it opens the list goes, and the page counts invitees by group
    /// only; closing or withdrawing it then erases each address (<see cref="MsfInvitation.Anonymize" />).
    /// </para>
    /// </remarks>
    public IReadOnlyList<MsfDraftInviteeDto> DraftInvitees { get; init; } = [];

    /// <summary>
    /// While the campaign is open, how many respondents' links did not reach them
    /// (<see cref="MsfInvitation.LinkNotDelivered" />): the number the page's Resend sends again
    /// (<see cref="ResendMsfLinksCommand" />). Zero in every other state. (T251)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A number, never who, for the reason <see cref="Invitees" /> is counted by group (T217): the coordinator knows
    /// which address is whom, and an undelivered respondent is one who has not answered. Counted in one statement that
    /// reads no row per invitee.
    /// </para>
    /// <para>
    /// Not quite nothing. Two links not delivered beside two groups' counts can let a coordinator who knows one address
    /// is mistyped guess that it is one of the two. Resending, not naming, is what the page offers: an address cannot be
    /// corrected once the campaign has opened.
    /// </para>
    /// </remarks>
    public int LinksNotDelivered { get; init; }

    /// <summary>
    /// While the campaign is open, how many links' mail the mail worker has not yet reported on
    /// (<see cref="MsfInvitation.LinkBeingSent" />). Zero in every other state. (T251)
    /// </summary>
    public int LinksBeingSent { get; init; }
}

/// <summary>One respondent group's invitees on a campaign: how many were invited, and how many have responded. (T217)</summary>
public sealed record MsfInviteeCountDto(MsfRespondentCategory Category, int Invited, int Responded);

/// <summary>
/// One invitee of a draft campaign: the invitation's id, the address typed, the group, and where a learner was taught
/// (null for anyone else). (T247)
/// </summary>
public sealed record MsfDraftInviteeDto(
    int InvitationId,
    string Email,
    MsfRespondentCategory Category,
    string? TeachingContext);

public sealed class GetMsfCampaignSetupQueryHandler : IRequestHandler<GetMsfCampaignSetupQuery, MsfCampaignSetupDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _timeProvider;

    public GetMsfCampaignSetupQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _users = users;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<MsfCampaignSetupDto?> Handle(GetMsfCampaignSetupQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var campaign = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(entity => entity.Template)
            .Where(entity => entity.Id == request.CampaignId)
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign is null ||
            !await MsfCampaignRules.IsSubjectInScopeAsync(_dbContext, request.Principal, campaign.SubjectUserId, cancellationToken))
        {
            return null;
        }

        // Each invitation's category and its response count, and nothing else of it: no address, no link, no date.
        var invitations = await _dbContext.Set<MsfInvitation>()
            .AsNoTracking()
            .Where(invitation => invitation.CampaignId == campaign.Id)
            .Select(invitation => new { invitation.RespondentCategory, Responses = invitation.Responses.Count })
            .ToListAsync(cancellationToken);

        // A draft's addresses, for Remove (T247), and only a draft's. Not asked at all for a campaign read as anything
        // else, so its page never sends a statement that names an address (T217); and the statement asks the campaign row
        // again, so a campaign opened since the read above lists none. Nor for a draft that invites nobody: the counts
        // above found no invitation, and the list is not to show one they do not count.
        IReadOnlyList<MsfDraftInviteeDto> draftInvitees = [];
        if (campaign.State == MsfCampaignState.Draft && invitations.Count > 0)
        {
            draftInvitees = await _dbContext.Set<MsfInvitation>()
                .AsNoTracking()
                .Where(invitation => invitation.CampaignId == campaign.Id &&
                                     invitation.Campaign.State == MsfCampaignState.Draft &&
                                     invitation.RespondentEmail != null)
                .OrderBy(invitation => invitation.Id)
                .Select(invitation => new MsfDraftInviteeDto(
                    invitation.Id, invitation.RespondentEmail!, invitation.RespondentCategory, invitation.TeachingContext))
                .ToListAsync(cancellationToken);
        }

        // How many links did not reach their respondents, and how many are still being sent, while the campaign is open
        // (T251). Two counts, each one statement by the rule the Resend sends by, so the page is sent numbers and no row.
        var linksNotDelivered = 0;
        var linksBeingSent = 0;
        if (campaign.State == MsfCampaignState.Open)
        {
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var campaignInvitations = _dbContext.Set<MsfInvitation>()
                .AsNoTracking()
                .Where(invitation => invitation.CampaignId == campaign.Id);
            linksNotDelivered = await campaignInvitations.CountAsync(MsfInvitation.LinkNotDelivered(utcNow), cancellationToken);
            linksBeingSent = await campaignInvitations.CountAsync(MsfInvitation.LinkBeingSent(utcNow), cancellationToken);
        }

        var names = await UserDisplayNames.ResolveAsync(_users, [campaign.SubjectUserId], cancellationToken);

        return new MsfCampaignSetupDto(
            campaign.Id,
            campaign.Template.Name,
            campaign.Template.Kind,
            campaign.State,
            campaign.Template.AcceptedCategories())
        {
            SubjectName = names.NameOf(campaign.SubjectUserId),
            OpensOn = campaign.OpensOn,
            ClosesOn = campaign.ClosesOn,
            Invitees = invitations
                .GroupBy(invitation => invitation.RespondentCategory)
                .OrderBy(group => group.Key)
                .Select(group => new MsfInviteeCountDto(group.Key, group.Count(), group.Sum(invitation => invitation.Responses)))
                .ToList(),
            DraftInvitees = draftInvitees,
            LinksNotDelivered = linksNotDelivered,
            LinksBeingSent = linksBeingSent
        };
    }
}
