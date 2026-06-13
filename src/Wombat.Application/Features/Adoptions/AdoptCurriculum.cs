using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Adoptions;

/// <summary>
/// An institution adopts (or re-adopts) a national curriculum version for a discipline. Re-adopting a
/// different version of the same discipline deactivates the institution's current active adoption and
/// activates the new one, so at most one adoption per (institution, discipline) is ever active. (T091 phase 4.)
/// </summary>
public sealed record AdoptCurriculumCommand(
    int InstitutionId,
    int CurriculumId,
    ClaimsPrincipal Principal) : IRequest<AdoptionDto>;

public sealed class AdoptCurriculumCommandValidator : AbstractValidator<AdoptCurriculumCommand>
{
    public AdoptCurriculumCommandValidator()
    {
        RuleFor(command => command.InstitutionId).GreaterThan(0);
        RuleFor(command => command.CurriculumId).GreaterThan(0);
    }
}

public sealed class AdoptCurriculumCommandHandler : IRequestHandler<AdoptCurriculumCommand, AdoptionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public AdoptCurriculumCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdoptionDto> Handle(AdoptCurriculumCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessInstitution(request.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage adoptions for this institution.");
        }

        var curriculum = await _dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
                .ThenInclude(entity => entity.Speciality)
                    .ThenInclude(entity => entity.College)
            .SingleOrDefaultAsync(entity => entity.Id == request.CurriculumId, cancellationToken)
            ?? throw new InvalidOperationException("The selected curriculum could not be found.");

        var existingActive = await _dbContext.Set<InstitutionCurriculumAdoption>()
            .SingleOrDefaultAsync(
                entity => entity.InstitutionId == request.InstitutionId
                    && entity.SubSpecialityId == curriculum.SubSpecialityId
                    && entity.IsActive,
                cancellationToken);

        if (existingActive is not null)
        {
            if (existingActive.CurriculumId == curriculum.Id)
            {
                throw new InvalidOperationException("This institution has already adopted this curriculum version.");
            }

            // Re-adoption: supersede the current active adoption for this discipline.
            existingActive.IsActive = false;
        }

        var adoption = new InstitutionCurriculumAdoption
        {
            InstitutionId = request.InstitutionId,
            CurriculumId = curriculum.Id,
            SubSpecialityId = curriculum.SubSpecialityId,
            AdoptedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        };

        _dbContext.Set<InstitutionCurriculumAdoption>().Add(adoption);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AdoptionMappings.ToDto(adoption, curriculum);
    }
}
