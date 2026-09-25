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
        var owningCollegeId = await _dbContext.Set<Speciality>()
            .Where(entity => entity.Id == request.SpecialityId)
            .Select(entity => (int?)entity.CollegeId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Speciality {request.SpecialityId} was not found.");

        if (!request.Principal.CanAccessCollege(owningCollegeId))
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
            // T254. Before this every refused save was reported as a duplicate name, including a speciality deleted after
            // the check above. The database's refusal stays underneath, so the audit pipeline discards the refused insert
            // (T201); a refusal none of these explains is left as the database gave it.
            var refusal = await SubSpecialitySaveRefusal.ReadBackAsync(
                _dbContext, exception, subSpecialityId: null, subSpeciality.SpecialityId, subSpeciality.Name,
                defaultEntrustmentScaleId: null, cancellationToken);
            if (refusal is null)
            {
                throw;
            }

            throw new InvalidOperationException(refusal, exception);
        }

        return new SubSpecialityDto(subSpeciality.Id, subSpeciality.SpecialityId, subSpeciality.Name, subSpeciality.Description, subSpeciality.IsActive);
    }
}
