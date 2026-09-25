using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record CreateMsfTemplateQuestionItem(string Prompt, MsfQuestionType Type, int? ScaleId, bool Required);

/// <param name="Principal">
/// The caller, who must run campaigns (<see cref="MsfCampaignRules.EnsureRunsCampaigns" />, T248): a Coordinator at an
/// institution or an Administrator, and never someone who holds Trainee.
/// </param>
/// <param name="Kind">
/// What the questionnaire collects (T164, D35): multi-source feedback, or learner feedback, which only learners answer and
/// whose release records <c>learner_feedback_cpsa</c> evidence. Fixed for the template's life.
/// </param>
public sealed record CreateMsfTemplateCommand(
    string Name,
    int? SpecialityId,
    bool AllowPatientResponses,
    IReadOnlyList<CreateMsfTemplateQuestionItem> Questions,
    ClaimsPrincipal Principal,
    MsfTemplateKind Kind = MsfTemplateKind.Msf) : IRequest<MsfTemplateDto>;

public sealed class CreateMsfTemplateCommandValidator : AbstractValidator<CreateMsfTemplateCommand>
{
    public CreateMsfTemplateCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Questions).NotEmpty();
        RuleFor(command => command.Kind).IsInEnum();

        // Only learners answer learner feedback (MsfTemplate.Accepts), so a patient never could. (T164)
        RuleFor(command => command.AllowPatientResponses).Equal(false)
            .When(command => command.Kind == MsfTemplateKind.LearnerFeedback)
            .WithMessage("A learner-feedback template is answered by learners only, so it cannot allow patient responses.");
        RuleForEach(command => command.Questions).ChildRules(question =>
        {
            question.RuleFor(item => item.Prompt).NotEmpty().MaximumLength(1000);
        });
    }
}

public sealed class CreateMsfTemplateCommandHandler : IRequestHandler<CreateMsfTemplateCommand, MsfTemplateDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateMsfTemplateCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MsfTemplateDto> Handle(CreateMsfTemplateCommand request, CancellationToken cancellationToken)
    {
        // Before the template is built, let alone added (T248): the audit pipeline saves the request's context from its
        // catch, so a refusal after the Add would store the template it refused.
        MsfCampaignRules.EnsureRunsCampaigns(request.Principal);

        var template = new MsfTemplate
        {
            Name = request.Name.Trim(),
            SpecialityId = request.SpecialityId,
            AllowPatientResponses = request.AllowPatientResponses,
            IsActive = true,
            Kind = request.Kind,
            Questions = request.Questions
                .Select((question, index) => new MsfQuestion
                {
                    Order = index + 1,
                    Prompt = question.Prompt.Trim(),
                    Type = question.Type,
                    ScaleId = question.ScaleId,
                    Required = question.Required
                })
                .ToList()
        };

        _dbContext.Set<MsfTemplate>().Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MsfTemplateDto(
            template.Id,
            template.Name,
            template.SpecialityId,
            template.AllowPatientResponses,
            template.IsActive,
            template.Questions
                .OrderBy(question => question.Order)
                .Select(question => new MsfQuestionDto(question.Id, question.Order, question.Prompt, question.Type, question.ScaleId, question.Required))
                .ToList(),
            template.Kind);
    }
}
