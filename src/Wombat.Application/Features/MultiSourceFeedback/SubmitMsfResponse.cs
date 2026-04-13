using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record SubmitMsfResponseAnswerItem(int QuestionId, int? ScaleValue, string? LongText);

public sealed record SubmitMsfResponseCommand(string Token, IReadOnlyList<SubmitMsfResponseAnswerItem> Answers) : IRequest;

public sealed class SubmitMsfResponseCommandValidator : AbstractValidator<SubmitMsfResponseCommand>
{
    public SubmitMsfResponseCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.Answers).NotEmpty();
    }
}

public sealed class SubmitMsfResponseCommandHandler : IRequestHandler<SubmitMsfResponseCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;

    public SubmitMsfResponseCommandHandler(IApplicationDbContext dbContext, IInvitationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task Handle(SubmitMsfResponseCommand request, CancellationToken cancellationToken)
    {
        var invitation = await MsfCampaignRules.GetActiveInvitationByTokenAsync(_dbContext, request.Token, _tokenService, cancellationToken);
        MsfCampaignRules.ValidateResponsePayload(invitation.Campaign.Template, request.Answers);

        var response = new MsfResponse
        {
            CampaignId = invitation.CampaignId,
            InvitationId = invitation.Id,
            SubmittedOn = DateTime.UtcNow,
            Answers = request.Answers
                .Select(answer => new MsfResponseAnswer
                {
                    QuestionId = answer.QuestionId,
                    ScaleValue = answer.ScaleValue,
                    LongText = string.IsNullOrWhiteSpace(answer.LongText) ? null : answer.LongText.Trim()
                })
                .ToList()
        };

        invitation.RespondedOn = response.SubmittedOn;
        _dbContext.Set<MsfResponse>().Add(response);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
