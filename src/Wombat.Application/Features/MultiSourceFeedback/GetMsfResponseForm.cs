using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// The questionnaire a respondent's link opens, or a refusal written for them (<see cref="MsfResponseRefusedException" />).
/// </summary>
/// <remarks>
/// Asked by both places a respondent can answer: the web app's respondent page (<c>/msf/respond</c>, T205) and the Api's
/// integration endpoint. The link is judged by <see cref="MsfCampaignRules.GetActiveInvitationByTokenAsync" />, the same
/// rule the submit runs, so the page never shows a questionnaire the submit would refuse.
/// </remarks>
public sealed record GetMsfResponseFormQuery(string Token) : IRequest<MsfResponseFormDto>;

public sealed class GetMsfResponseFormQueryHandler : IRequestHandler<GetMsfResponseFormQuery, MsfResponseFormDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IUserAdministrationService _users;

    public GetMsfResponseFormQueryHandler(
        IApplicationDbContext dbContext,
        IInvitationTokenService tokenService,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _users = users;
    }

    public async Task<MsfResponseFormDto> Handle(GetMsfResponseFormQuery request, CancellationToken cancellationToken)
    {
        var invitation = await MsfCampaignRules.GetActiveInvitationByTokenAsync(_dbContext, request.Token, _tokenService, cancellationToken);
        var campaign = invitation.Campaign;
        var scalePoints = await MsfRatingScale.ResolveAsync(_dbContext, campaign.Template, cancellationToken);

        // Whom the feedback is for, named as the invitation named them (T202). Naming the trainee is what MSF needs of a
        // respondent: its anonymity hides the respondent from the trainee, not the other way round.
        var names = await _users.GetDisplayNamesAsync([campaign.SubjectUserId], cancellationToken);
        var traineeName = names.TryGetValue(campaign.SubjectUserId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name.Trim()
            : null;

        return new MsfResponseFormDto(
            campaign.Template.Name,
            campaign.Template.Kind,
            traineeName,
            MsfInvitation.LastDayToRespond(campaign.ClosesOn, invitation.ExpiresOn),
            invitation.RespondentCategory,
            campaign.Template.Questions
                .OrderBy(question => question.Order)
                .Select(question => new MsfResponsePromptDto(
                    question.Id,
                    question.Prompt,
                    question.Type,
                    question.Required,
                    scalePoints.TryGetValue(question.Id, out var points) ? points : []))
                .ToList());
    }
}
