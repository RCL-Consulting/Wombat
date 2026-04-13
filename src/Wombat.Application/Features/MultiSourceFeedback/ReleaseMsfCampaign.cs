using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record ReleaseMsfCampaignCommand(int CampaignId, string ReviewerUserId, string? Narrative) : IRequest;

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
