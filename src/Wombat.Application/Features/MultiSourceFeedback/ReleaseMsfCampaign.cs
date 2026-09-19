using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <remarks>
/// <c>Narrative</c> is redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; this is the reviewer's written summary of what colleagues said about a named
/// trainee, up to 4000 characters of it, and it does not belong in an admin-searchable table. The
/// campaign and reviewer ids stay in the clear — who released what, and when. (T101)
/// </remarks>
public sealed record ReleaseMsfCampaignCommand(
    int CampaignId,
    string ReviewerUserId,
    [property: Redact] string? Narrative) : IRequest;

public sealed class ReleaseMsfCampaignCommandValidator : AbstractValidator<ReleaseMsfCampaignCommand>
{
    public ReleaseMsfCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId).GreaterThan(0);
        RuleFor(command => command.ReviewerUserId).NotEmpty();
        RuleFor(command => command.Narrative).MaximumLength(4000);
    }
}

public sealed class ReleaseMsfCampaignCommandHandler : IRequestHandler<ReleaseMsfCampaignCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _aggregationService;

    public ReleaseMsfCampaignCommandHandler(IApplicationDbContext dbContext, IMsfAggregationService aggregationService)
    {
        _dbContext = dbContext;
        _aggregationService = aggregationService;
    }

    public async Task Handle(ReleaseMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);
        var report = _aggregationService.BuildReport(campaign);
        if (!report.ReadyForRelease)
        {
            throw new InvalidOperationException("The campaign cannot be released until the minimum response count is met.");
        }

        campaign.Release(request.ReviewerUserId, request.Narrative, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
