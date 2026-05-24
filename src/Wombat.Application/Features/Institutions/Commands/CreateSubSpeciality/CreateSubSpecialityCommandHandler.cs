using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;

public sealed class CreateSubSpecialityCommandHandler : IRequestHandler<CreateSubSpecialityCommand, SubSpecialityDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSubSpecialityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubSpecialityDto> Handle(CreateSubSpecialityCommand request, CancellationToken cancellationToken)
    {
        var owningInstitutionId = await _dbContext.Set<Speciality>()
            .Where(entity => entity.Id == request.SpecialityId)
            .Select(entity => (int?)entity.InstitutionId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Speciality {request.SpecialityId} was not found.");

        if (!request.Principal.CanAccessInstitution(owningInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create a sub-speciality for this speciality.");
        }

        var subSpeciality = new SubSpeciality
        {
            SpecialityId = request.SpecialityId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _dbContext.Set<SubSpeciality>().Add(subSpeciality);

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
