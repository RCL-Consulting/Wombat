using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

public sealed record CloneCurriculumAsNewVersionCommand(
    int CurriculumId,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo) : IRequest<CurriculumDto>;

public sealed class CloneCurriculumAsNewVersionCommandValidator : AbstractValidator<CloneCurriculumAsNewVersionCommand>
{
    public CloneCurriculumAsNewVersionCommandValidator()
    {
        RuleFor(command => command.CurriculumId).GreaterThan(0);
        RuleFor(command => command.Version).NotEmpty().MaximumLength(64);
        RuleFor(command => command.EffectiveTo)
            .GreaterThanOrEqualTo(command => command.EffectiveFrom)
            .When(command => command.EffectiveTo.HasValue);
    }
}

public sealed class CloneCurriculumAsNewVersionCommandHandler : IRequestHandler<CloneCurriculumAsNewVersionCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CloneCurriculumAsNewVersionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(CloneCurriculumAsNewVersionCommand request, CancellationToken cancellationToken)
    {
        var current = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        var clone = current.CloneAsNewVersion(request.Version, request.EffectiveFrom, request.EffectiveTo);

        _dbContext.Set<Curriculum>().Add(clone);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A curriculum with the same name and version already exists for this sub-speciality.", exception);
        }

        clone = await CurriculumMappings.LoadCurriculumAsync(_dbContext, clone.Id, cancellationToken);
        return CurriculumMappings.ToDto(clone, clone.SubSpeciality.SpecialityId, clone.SubSpeciality.Speciality.Name, clone.SubSpeciality.Name, true);
    }
}
