using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;

public sealed class UpdateSubSpecialityCommandHandler : IRequestHandler<UpdateSubSpecialityCommand, SubSpecialityDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSubSpecialityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubSpecialityDto> Handle(UpdateSubSpecialityCommand request, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.Set<SubSpeciality>()
            .Include(entity => entity.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Sub-speciality {request.Id} was not found.");

        if (!request.Principal.CanAccessInstitution(subSpeciality.Speciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this sub-speciality.");
        }

        var targetInstitutionId = await _dbContext.Set<Speciality>()
            .Where(entity => entity.Id == request.SpecialityId)
            .Select(entity => (int?)entity.InstitutionId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Speciality {request.SpecialityId} was not found.");

        if (!request.Principal.CanAccessInstitution(targetInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to move this sub-speciality to that speciality.");
        }

        subSpeciality.SpecialityId = request.SpecialityId;
        subSpeciality.Name = request.Name.Trim();
        subSpeciality.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        subSpeciality.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A sub-speciality with the same name already exists for this speciality.", exception);
        }

        return new SubSpecialityDto(subSpeciality.Id, subSpeciality.SpecialityId, subSpeciality.Name, subSpeciality.Description, subSpeciality.IsActive);
    }
}
