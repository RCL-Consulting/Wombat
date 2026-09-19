using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record SubmitMsfResponseAnswerItem(int QuestionId, int? ScaleValue, string? LongText);

/// <remarks>
/// Both properties are redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson. <c>Answers</c> carries the respondent's free-text judgement of a named colleague,
/// which multi-source feedback only works because it is confidential and aggregated — the audit
/// table showed it un-aggregated and attributable. <c>Token</c> is the respondent's single-use link
/// and would let a reader re-open or overwrite their response. Nothing identifying the campaign is
/// lost: the row still records that a response was submitted, when, and from where. (T101)
/// </remarks>
public sealed record SubmitMsfResponseCommand(
    [property: Redact] string Token,
    [property: Redact] IReadOnlyList<SubmitMsfResponseAnswerItem> Answers) : IRequest;

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
