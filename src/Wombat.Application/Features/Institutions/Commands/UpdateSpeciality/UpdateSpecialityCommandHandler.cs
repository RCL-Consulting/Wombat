using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
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

        // T394 (flow 16, N1): the target College and the name's holder are read before the speciality is touched, so a
        // refusal stages nothing for the audit pipeline's failure row to send (T201). A taken name is the Name field's
        // refusal. The College is read as the create reads it (fix pass, code N-8), rather than left to the foreign key.
        if (request.CollegeId != speciality.CollegeId
            && !await _dbContext.Set<College>().AnyAsync(college => college.Id == request.CollegeId, cancellationToken))
        {
            throw new InvalidOperationException($"College {request.CollegeId} was not found.");
        }

        var name = request.Name.Trim();
        if (await SpecialitySaveRefusal.IsTakenAsync(_dbContext, speciality.Id, request.CollegeId, name, cancellationToken))
        {
            throw new FieldRefusalException(nameof(UpdateSpecialityCommand.Name), SpecialitySaveRefusal.DuplicateName);
        }

        speciality.CollegeId = request.CollegeId;
        speciality.Name = name;
        speciality.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        speciality.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            // Another save took the name after the check (T394, N1). Any other refusal is left as the database gave it, so
            // the page shows its fixed failure sentence (T272) and the audit pipeline discards the refused update (T201).
            if (!await SpecialitySaveRefusal.WasTakenAsync(
                    _dbContext, exception, speciality.Id, speciality.CollegeId, speciality.Name, cancellationToken))
            {
                throw;
            }

            throw new FieldRefusalException(nameof(UpdateSpecialityCommand.Name), SpecialitySaveRefusal.DuplicateName, exception);
        }

        return new SpecialityDto(speciality.Id, speciality.CollegeId, speciality.Name, speciality.Description, speciality.IsActive);
    }
}
