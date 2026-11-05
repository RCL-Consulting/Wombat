using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;
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

        // T394 (flow 16, Q1, N1): the default ladder is chosen at create, and everything that can refuse is read before
        // anything is staged, so a refusal leaves nothing for the audit pipeline's failure row to send (T201).
        if (request.DefaultEntrustmentScaleId is int scaleId
            && !await _dbContext.Set<EntrustmentScale>().AnyAsync(scale => scale.Id == scaleId, cancellationToken))
        {
            throw new InvalidOperationException($"Entrustment scale {scaleId} was not found.");
        }

        var name = request.Name.Trim();
        if (await SubSpecialitySaveRefusal.IsTakenAsync(_dbContext, subSpecialityId: null, request.SpecialityId, name, cancellationToken))
        {
            throw new FieldRefusalException(nameof(CreateSubSpecialityCommand.Name), SubSpecialitySaveRefusal.DuplicateName);
        }

        var subSpeciality = new SubSpeciality
        {
            SpecialityId = request.SpecialityId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DefaultEntrustmentScaleId = request.DefaultEntrustmentScaleId
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
            // (T201); a refusal none of these explains is left as the database gave it. A name taken after the check is the
            // Name field's refusal (T394, N1).
            var refusal = await SubSpecialitySaveRefusal.ReadBackAsync(
                _dbContext, exception, subSpecialityId: null, subSpeciality.SpecialityId, subSpeciality.Name,
                subSpeciality.DefaultEntrustmentScaleId, cancellationToken);
            if (refusal is null)
            {
                throw;
            }

            throw SubSpecialitySaveRefusal.Refused(refusal, exception);
        }

        return new SubSpecialityDto(subSpeciality.Id, subSpeciality.SpecialityId, subSpeciality.Name, subSpeciality.Description, subSpeciality.IsActive, subSpeciality.DefaultEntrustmentScaleId);
    }
}
