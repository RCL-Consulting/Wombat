using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

public sealed record UpdateCurriculumCommand(
    int Id,
    int SubSpecialityId,
    string Name,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive) : IRequest<CurriculumDto>;

public sealed class UpdateCurriculumCommandValidator : AbstractValidator<UpdateCurriculumCommand>
{
    public UpdateCurriculumCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.SubSpecialityId).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Version).NotEmpty().MaximumLength(64);
        RuleFor(command => command.EffectiveTo)
            .GreaterThanOrEqualTo(command => command.EffectiveFrom)
            .When(command => command.EffectiveTo.HasValue);
    }
}

public sealed class UpdateCurriculumCommandHandler : IRequestHandler<UpdateCurriculumCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCurriculumCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(UpdateCurriculumCommand request, CancellationToken cancellationToken)
    {
        var curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.Id, cancellationToken);
        CurriculumMappings.EnsureCurriculumCanBeEditedInPlace();

        var subSpecialityName = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => entity.Name)
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpecialityName is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        curriculum.SubSpecialityId = request.SubSpecialityId;
        curriculum.Name = request.Name.Trim();
        curriculum.Version = request.Version.Trim();
        curriculum.EffectiveFrom = request.EffectiveFrom;
        curriculum.EffectiveTo = request.EffectiveTo;
        curriculum.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A curriculum with the same name and version already exists for this sub-speciality.", exception);
        }

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.Id, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, subSpecialityName, true);
    }
}
