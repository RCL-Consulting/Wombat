using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record GetMsfResponseFormQuery(string Token) : IRequest<MsfResponseFormDto>;

public sealed class GetMsfResponseFormQueryHandler : IRequestHandler<GetMsfResponseFormQuery, MsfResponseFormDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;

    public GetMsfResponseFormQueryHandler(IApplicationDbContext dbContext, IInvitationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<MsfResponseFormDto> Handle(GetMsfResponseFormQuery request, CancellationToken cancellationToken)
    {
        var invitation = await MsfCampaignRules.GetActiveInvitationByTokenAsync(_dbContext, request.Token, _tokenService, cancellationToken);
        return new MsfResponseFormDto(
            invitation.CampaignId,
            invitation.Campaign.Template.Name,
            invitation.Campaign.ClosesOn,
            invitation.RespondentCategory,
            invitation.Campaign.Template.Questions
                .OrderBy(question => question.Order)
                .Select(question => new MsfResponsePromptDto(question.Id, question.Prompt, question.Type, question.ScaleId, question.Required))
                .ToList());
    }
}
