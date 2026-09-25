using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <param name="EpaIds">
/// The EPAs this campaign is declared to be evidence for. College decision D9: a campaign is run once
/// per period and covers many EPAs, not once per EPA. Required to be non-empty — a campaign covering
/// nothing releases, anonymises its respondents and leaves no record of what it was evidence for, which
/// is the defect [T121] exists to remove rather than a configuration worth supporting.
/// </param>
public sealed record CreateMsfCampaignCommand(
    string SubjectUserId,
    int TemplateId,
    DateOnly OpensOn,
    DateOnly ClosesOn,
    int MinimumResponses,
    int MinimumCategoryResponses,
    int MinimumRespondentCategories,
    IReadOnlyList<int> EpaIds,
    string CreatedByUserId,
    ClaimsPrincipal Principal) : IRequest<MsfCampaignSummaryDto>;

public sealed class CreateMsfCampaignCommandValidator : AbstractValidator<CreateMsfCampaignCommand>
{
    public CreateMsfCampaignCommandValidator()
    {
        RuleFor(command => command.SubjectUserId).NotEmpty();
        RuleFor(command => command.TemplateId).GreaterThan(0);
        RuleFor(command => command.MinimumResponses).GreaterThanOrEqualTo(1);
        RuleFor(command => command.MinimumCategoryResponses).GreaterThanOrEqualTo(1);
        RuleFor(command => command.MinimumRespondentCategories).GreaterThanOrEqualTo(1);
        RuleFor(command => command.EpaIds).NotEmpty()
            .WithMessage("A campaign must say which EPAs it is evidence for.");
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.ClosesOn).GreaterThanOrEqualTo(command => command.OpensOn);
    }
}

public sealed class CreateMsfCampaignCommandHandler : IRequestHandler<CreateMsfCampaignCommand, MsfCampaignSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IActivityReferenceDataService _referenceDataService;
    private readonly IUserAdministrationService _users;

    public CreateMsfCampaignCommandHandler(
        IApplicationDbContext dbContext,
        IActivityReferenceDataService referenceDataService,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _referenceDataService = referenceDataService;
        _users = users;
    }

    public async Task<MsfCampaignSummaryDto> Handle(CreateMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        // Scope first, before the template is read (T224 review): a caller who may not run this campaign is told that,
        // whatever template id they sent. Only a current trainee (T238): an erased trainee's pseudonym and a trainee
        // whose programme has ended are refused here, before anything is written.
        var subjectUserId = request.SubjectUserId.Trim();
        await MsfCampaignRules.EnsureSubjectIsInScopeAsync(
            _dbContext, _users, request.Principal, subjectUserId, cancellationToken);

        var template = await _dbContext.Set<MsfTemplate>()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.TemplateId && candidate.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("The selected MSF template could not be found.");

        // A learner-feedback campaign invites learners only (MsfTemplate.Accepts), so one respondent group is all it can
        // ever have. Asking for two would leave it unreleasable whatever came back. (T164)
        if (template.Kind == MsfTemplateKind.LearnerFeedback && request.MinimumRespondentCategories > 1)
        {
            throw new InvalidOperationException(
                "A learner-feedback campaign has one respondent group, learners, so it can require only one reporting " +
                "category.");
        }

        // Narrowed here as well as in the picker, because the picker is an affordance and this is the
        // gate. The same predicate is applied a third time at release, against the curriculum as it
        // stands then — a trainee may be moved between curricula while the window is open. For learner
        // feedback it is also the EPA's tool list: only an EPA whose list names learner feedback. (T164)
        var coverableEpaIds = await MsfCampaignRules.CoverableEpaIdsAsync(
            _referenceDataService, subjectUserId, template.Kind, cancellationToken);

        var requestedEpaIds = request.EpaIds.Distinct().ToArray();
        var notCoverable = requestedEpaIds.Where(epaId => !coverableEpaIds.Contains(epaId)).ToArray();
        if (notCoverable.Length > 0)
        {
            throw new InvalidOperationException(
                (template.Kind == MsfTemplateKind.LearnerFeedback
                    ? "These EPAs are not on the trainee's curriculum, or their College list does not name learner " +
                      "feedback, and cannot be covered by this campaign: "
                    : "These EPAs are not on the trainee's curriculum and cannot be covered by this campaign: ") +
                string.Join(", ", notCoverable));
        }

        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            TemplateId = request.TemplateId,
            CreatedByUserId = request.CreatedByUserId.Trim(),
            CreatedOn = DateTime.UtcNow,
            OpensOn = request.OpensOn,
            ClosesOn = request.ClosesOn,
            MinimumResponses = request.MinimumResponses,
            MinimumCategoryResponses = request.MinimumCategoryResponses,
            MinimumRespondentCategories = request.MinimumRespondentCategories,
            State = MsfCampaignState.Draft,
            CoveredEpas = requestedEpaIds
                .Select(epaId => new MsfCampaignEpa { EpaId = epaId })
                .ToList()
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
