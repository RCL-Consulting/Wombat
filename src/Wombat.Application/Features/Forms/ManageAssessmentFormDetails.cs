using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;

namespace Wombat.Application.Features.Forms;

/// <summary>No validator: carries a single non-nullable int ID; EF lookup enforces existence.</summary>
[NoValidator]
public sealed record DeactivateAssessmentFormCommand(int Id) : IRequest;
public sealed record AddFormCriterionCommand(int FormId, string Prompt, string? HelpText, bool IsRequired) : IRequest<AssessmentFormDto>;
public sealed record RemoveFormCriterionCommand(int FormId, int CriterionId) : IRequest<AssessmentFormDto>;
public sealed record LinkFormToEpaCommand(int FormId, int EpaId) : IRequest<AssessmentFormDto>;
public sealed record UnlinkFormFromEpaCommand(int FormId, int LinkId) : IRequest<AssessmentFormDto>;

public sealed class AddFormCriterionCommandValidator : AbstractValidator<AddFormCriterionCommand>
{
    public AddFormCriterionCommandValidator()
    {
        RuleFor(command => command.FormId).GreaterThan(0);
        RuleFor(command => command.Prompt).NotEmpty().MaximumLength(500);
        RuleFor(command => command.HelpText).MaximumLength(2000);
    }
}

public sealed class RemoveFormCriterionCommandValidator : AbstractValidator<RemoveFormCriterionCommand>
{
    public RemoveFormCriterionCommandValidator()
    {
        RuleFor(command => command.FormId).GreaterThan(0);
        RuleFor(command => command.CriterionId).GreaterThan(0);
    }
}

public sealed class LinkFormToEpaCommandValidator : AbstractValidator<LinkFormToEpaCommand>
{
    public LinkFormToEpaCommandValidator()
    {
        RuleFor(command => command.FormId).GreaterThan(0);
        RuleFor(command => command.EpaId).GreaterThan(0);
    }
}

public sealed class UnlinkFormFromEpaCommandValidator : AbstractValidator<UnlinkFormFromEpaCommand>
{
    public UnlinkFormFromEpaCommandValidator()
    {
        RuleFor(command => command.FormId).GreaterThan(0);
        RuleFor(command => command.LinkId).GreaterThan(0);
    }
}

public sealed class DeactivateAssessmentFormCommandHandler : IRequestHandler<DeactivateAssessmentFormCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateAssessmentFormCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateAssessmentFormCommand request, CancellationToken cancellationToken)
    {
        var form = await _dbContext.Set<AssessmentForm>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (form is null)
        {
            throw new InvalidOperationException("The requested assessment form was not found.");
        }

        form.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AddFormCriterionCommandHandler : IRequestHandler<AddFormCriterionCommand, AssessmentFormDto>
{
    private readonly IApplicationDbContext _dbContext;

    public AddFormCriterionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto> Handle(AddFormCriterionCommand request, CancellationToken cancellationToken)
    {
        var form = await _dbContext.Set<AssessmentForm>()
            .Include(entity => entity.Criteria)
            .SingleOrDefaultAsync(entity => entity.Id == request.FormId, cancellationToken);

        if (form is null)
        {
            throw new InvalidOperationException("The requested assessment form was not found.");
        }

        var nextOrder = form.Criteria.Count == 0 ? 1 : form.Criteria.Max(entity => entity.Order) + 1;
        form.Criteria.Add(new FormCriterion
        {
            Order = nextOrder,
            Prompt = request.Prompt.Trim(),
            HelpText = string.IsNullOrWhiteSpace(request.HelpText) ? null : request.HelpText.Trim(),
            IsRequired = request.IsRequired
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await FormMappings.GetByIdAsync(_dbContext, request.FormId, cancellationToken)
            ?? throw new InvalidOperationException("The assessment form could not be loaded after the criterion was added.");
    }
}

public sealed class RemoveFormCriterionCommandHandler : IRequestHandler<RemoveFormCriterionCommand, AssessmentFormDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveFormCriterionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto> Handle(RemoveFormCriterionCommand request, CancellationToken cancellationToken)
    {
        var criterion = await _dbContext.Set<FormCriterion>()
            .SingleOrDefaultAsync(entity => entity.Id == request.CriterionId && entity.FormId == request.FormId, cancellationToken);

        if (criterion is null)
        {
            throw new InvalidOperationException("The requested form criterion was not found.");
        }

        _dbContext.Set<FormCriterion>().Remove(criterion);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await FormMappings.GetByIdAsync(_dbContext, request.FormId, cancellationToken)
            ?? throw new InvalidOperationException("The assessment form could not be loaded after the criterion was removed.");
    }
}

public sealed class LinkFormToEpaCommandHandler : IRequestHandler<LinkFormToEpaCommand, AssessmentFormDto>
{
    private readonly IApplicationDbContext _dbContext;

    public LinkFormToEpaCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto> Handle(LinkFormToEpaCommand request, CancellationToken cancellationToken)
    {
        var form = await _dbContext.Set<AssessmentForm>()
            .Include(entity => entity.EpaLinks)
            .SingleOrDefaultAsync(entity => entity.Id == request.FormId, cancellationToken);

        if (form is null)
        {
            throw new InvalidOperationException("The requested assessment form was not found.");
        }

        if (!await _dbContext.Set<Domain.Epas.Epa>().AnyAsync(entity => entity.Id == request.EpaId, cancellationToken))
        {
            throw new InvalidOperationException("The selected EPA was not found.");
        }

        if (form.EpaLinks.Any(entity => entity.EpaId == request.EpaId))
        {
            throw new InvalidOperationException("This form is already linked to the selected EPA.");
        }

        form.EpaLinks.Add(new FormEpaLink { EpaId = request.EpaId });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await FormMappings.GetByIdAsync(_dbContext, request.FormId, cancellationToken)
            ?? throw new InvalidOperationException("The assessment form could not be loaded after the EPA link was added.");
    }
}

public sealed class UnlinkFormFromEpaCommandHandler : IRequestHandler<UnlinkFormFromEpaCommand, AssessmentFormDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UnlinkFormFromEpaCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto> Handle(UnlinkFormFromEpaCommand request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.Set<FormEpaLink>()
            .SingleOrDefaultAsync(entity => entity.Id == request.LinkId && entity.FormId == request.FormId, cancellationToken);

        if (link is null)
        {
            throw new InvalidOperationException("The requested EPA link was not found.");
        }

        _dbContext.Set<FormEpaLink>().Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await FormMappings.GetByIdAsync(_dbContext, request.FormId, cancellationToken)
            ?? throw new InvalidOperationException("The assessment form could not be loaded after the EPA link was removed.");
    }
}
