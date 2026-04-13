using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record CreateMsfTemplateQuestionItem(string Prompt, MsfQuestionType Type, int? ScaleId, bool Required);

public sealed record CreateMsfTemplateCommand(
    string Name,
    int? SpecialityId,
    bool AllowPatientResponses,
    IReadOnlyList<CreateMsfTemplateQuestionItem> Questions) : IRequest<MsfTemplateDto>;

public sealed class CreateMsfTemplateCommandValidator : AbstractValidator<CreateMsfTemplateCommand>
{
    public CreateMsfTemplateCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Questions).NotEmpty();
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
        var template = new MsfTemplate
        {
            Name = request.Name.Trim(),
            SpecialityId = request.SpecialityId,
            AllowPatientResponses = request.AllowPatientResponses,
            IsActive = true,
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
                .ToList());
    }
}
