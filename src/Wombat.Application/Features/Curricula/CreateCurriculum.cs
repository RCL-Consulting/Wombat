using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

public sealed record CreateCurriculumCommand(
    int SubSpecialityId,
    string Name,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed class CreateCurriculumCommandValidator : AbstractValidator<CreateCurriculumCommand>
{
    public CreateCurriculumCommandValidator()
    {
        RuleFor(command => command.SubSpecialityId).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Version).NotEmpty().MaximumLength(64);
        RuleFor(command => command.EffectiveTo)
            .GreaterThanOrEqualTo(command => command.EffectiveFrom)
            .When(command => command.EffectiveTo.HasValue);
    }
}

public sealed class CreateCurriculumCommandHandler : IRequestHandler<CreateCurriculumCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCurriculumCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(CreateCurriculumCommand request, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => new
            {
                entity.Name,
                entity.SpecialityId,
                SpecialityName = entity.Speciality.Name,
                entity.Speciality.InstitutionId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpeciality is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        if (!request.Principal.CanAccessInstitution(subSpeciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create a curriculum in this sub-speciality.");
        }

        var curriculum = new Curriculum
        {
            SubSpecialityId = request.SubSpecialityId,
            Name = request.Name.Trim(),
            Version = request.Version.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        _dbContext.Set<Curriculum>().Add(curriculum);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A curriculum with the same name and version already exists for this sub-speciality.", exception);
        }

        return new CurriculumDto(curriculum.Id, subSpeciality.SpecialityId, curriculum.SubSpecialityId, subSpeciality.SpecialityName, subSpeciality.Name, curriculum.Name, curriculum.Version, curriculum.EffectiveFrom, curriculum.EffectiveTo, curriculum.IsActive, true, []);
    }
}
