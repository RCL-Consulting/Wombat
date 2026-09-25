using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;
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

        if (!request.Principal.CanAccessCollege(subSpeciality.Speciality.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this sub-speciality.");
        }

        var targetCollegeId = await _dbContext.Set<Speciality>()
            .Where(entity => entity.Id == request.SpecialityId)
            .Select(entity => (int?)entity.CollegeId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Speciality {request.SpecialityId} was not found.");

        if (!request.Principal.CanAccessCollege(targetCollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to move this sub-speciality to that speciality.");
        }

        if (request.DefaultEntrustmentScaleId.HasValue &&
            !await _dbContext.Set<EntrustmentScale>().AnyAsync(scale => scale.Id == request.DefaultEntrustmentScaleId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Entrustment scale {request.DefaultEntrustmentScaleId.Value} was not found.");
        }

        subSpeciality.SpecialityId = request.SpecialityId;
        subSpeciality.Name = request.Name.Trim();
        subSpeciality.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        subSpeciality.IsActive = request.IsActive;
        subSpeciality.DefaultEntrustmentScaleId = request.DefaultEntrustmentScaleId;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            // T254. Before this every refused save was reported as a duplicate name, including a foreign key whose row
            // went away after the checks above: the speciality it moves to, or the default scale an administrator deleted
            // meanwhile. The database's refusal stays underneath, so the audit pipeline discards the refused update
            // (T201); a refusal none of these explains is left as the database gave it.
            var refusal = await SubSpecialitySaveRefusal.ReadBackAsync(
                _dbContext, exception, subSpeciality.Id, subSpeciality.SpecialityId, subSpeciality.Name,
                subSpeciality.DefaultEntrustmentScaleId, cancellationToken);
            if (refusal is null)
            {
                throw;
            }

            throw new InvalidOperationException(refusal, exception);
        }

        return new SubSpecialityDto(subSpeciality.Id, subSpeciality.SpecialityId, subSpeciality.Name, subSpeciality.Description, subSpeciality.IsActive, subSpeciality.DefaultEntrustmentScaleId);
    }
}
