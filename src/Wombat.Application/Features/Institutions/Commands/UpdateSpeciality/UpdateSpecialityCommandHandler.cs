using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.UpdateSpeciality;

public sealed class UpdateSpecialityCommandHandler : IRequestHandler<UpdateSpecialityCommand, SpecialityDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSpecialityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpecialityDto> Handle(UpdateSpecialityCommand request, CancellationToken cancellationToken)
    {
        var speciality = await _dbContext.Set<Speciality>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Speciality {request.Id} was not found.");

        if (!request.Principal.CanAccessCollege(speciality.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this speciality.");
        }

        if (!request.Principal.CanAccessCollege(request.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to move this speciality to that college.");
        }

        speciality.CollegeId = request.CollegeId;
        speciality.Name = request.Name.Trim();
        speciality.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        speciality.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A speciality with the same name already exists for this college.", exception);
        }

        return new SpecialityDto(speciality.Id, speciality.CollegeId, speciality.Name, speciality.Description, speciality.IsActive);
    }
}
