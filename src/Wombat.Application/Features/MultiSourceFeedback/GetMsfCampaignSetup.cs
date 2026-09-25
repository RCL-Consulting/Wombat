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
/// who holds Trainee (T224). Nothing that names a respondent is read: no address, and no row per invitee (see
/// <see cref="MsfCampaignSetupDto.Invitees" />).
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
}

/// <summary>One respondent group's invitees on a campaign: how many were invited, and how many have responded. (T217)</summary>
public sealed record MsfInviteeCountDto(MsfRespondentCategory Category, int Invited, int Responded);

public sealed class GetMsfCampaignSetupQueryHandler : IRequestHandler<GetMsfCampaignSetupQuery, MsfCampaignSetupDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetMsfCampaignSetupQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
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
                .ToList()
        };
    }
}
