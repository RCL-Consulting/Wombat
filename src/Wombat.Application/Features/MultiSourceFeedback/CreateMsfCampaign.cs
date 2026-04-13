using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record CreateMsfCampaignCommand(
    string SubjectUserId,
    int TemplateId,
    DateOnly OpensOn,
    DateOnly ClosesOn,
    int MinimumResponses,
    int MinimumCategoryResponses,
    string CreatedByUserId) : IRequest<MsfCampaignSummaryDto>;

public sealed class CreateMsfCampaignCommandValidator : AbstractValidator<CreateMsfCampaignCommand>
{
    public CreateMsfCampaignCommandValidator()
    {
        RuleFor(command => command.SubjectUserId).NotEmpty();
        RuleFor(command => command.TemplateId).GreaterThan(0);
        RuleFor(command => command.MinimumResponses).GreaterThanOrEqualTo(1);
        RuleFor(command => command.MinimumCategoryResponses).GreaterThanOrEqualTo(1);
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.ClosesOn).GreaterThanOrEqualTo(command => command.OpensOn);
    }
}

public sealed class CreateMsfCampaignCommandHandler : IRequestHandler<CreateMsfCampaignCommand, MsfCampaignSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateMsfCampaignCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MsfCampaignSummaryDto> Handle(CreateMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var template = await _dbContext.Set<MsfTemplate>()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.TemplateId && candidate.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("The selected MSF template could not be found.");

        var campaign = new MsfCampaign
        {
            SubjectUserId = request.SubjectUserId.Trim(),
            TemplateId = request.TemplateId,
            CreatedByUserId = request.CreatedByUserId.Trim(),
            CreatedOn = DateTime.UtcNow,
            OpensOn = request.OpensOn,
            ClosesOn = request.ClosesOn,
            MinimumResponses = request.MinimumResponses,
            MinimumCategoryResponses = request.MinimumCategoryResponses,
            State = MsfCampaignState.Draft
        };

        _dbContext.Set<MsfCampaign>().Add(campaign);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MsfCampaignSummaryDto(
            campaign.Id,
            campaign.SubjectUserId,
            template.Name,
            campaign.OpensOn,
            campaign.ClosesOn,
            campaign.MinimumResponses,
            campaign.MinimumCategoryResponses,
            campaign.State,
            0,
            0,
            campaign.ReleasedOn);
    }
}
