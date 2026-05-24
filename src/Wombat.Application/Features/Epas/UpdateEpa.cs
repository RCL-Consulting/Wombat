using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

public sealed record UpdateEpaCommand(
    int Id,
    int SubSpecialityId,
    string Code,
    string Title,
    string? Description,
    string? RequiredKnowledgeSkills,
    EpaCategory Category,
    bool IsActive,
    ClaimsPrincipal Principal) : IRequest<EpaDto>;

public sealed class UpdateEpaCommandValidator : AbstractValidator<UpdateEpaCommand>
{
    public UpdateEpaCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.SubSpecialityId).GreaterThan(0);
        RuleFor(command => command.Code).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(4000);
        RuleFor(command => command.RequiredKnowledgeSkills).MaximumLength(8000);
        RuleFor(command => command.Category).IsInEnum();
    }
}

public sealed class UpdateEpaCommandHandler : IRequestHandler<UpdateEpaCommand, EpaDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateEpaCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EpaDto> Handle(UpdateEpaCommand request, CancellationToken cancellationToken)
    {
        var epa = await _dbContext.Set<Epa>()
            .Include(entity => entity.SubSpeciality)
            .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (epa is null)
        {
            throw new InvalidOperationException("The requested EPA was not found.");
        }

        if (!request.Principal.CanAccessInstitution(epa.SubSpeciality.Speciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this EPA.");
        }

        var subSpeciality = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => new { entity.Name, entity.Speciality.InstitutionId })
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpeciality is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        if (!request.Principal.CanAccessInstitution(subSpeciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to move this EPA to that sub-speciality.");
        }

        epa.SubSpecialityId = request.SubSpecialityId;
        epa.Code = request.Code.Trim();
        epa.Title = request.Title.Trim();
        epa.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        epa.RequiredKnowledgeSkills = string.IsNullOrWhiteSpace(request.RequiredKnowledgeSkills) ? null : request.RequiredKnowledgeSkills.Trim();
        epa.Category = request.Category;
        epa.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("An EPA with the same code already exists for this sub-speciality.", exception);
        }

        return new EpaDto(epa.Id, epa.SubSpecialityId, subSpeciality.Name, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, epa.IsActive, epa.CreatedOn);
    }
}
