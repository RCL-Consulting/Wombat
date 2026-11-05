using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.CreateSpeciality;

public sealed class CreateSpecialityCommandHandler : IRequestHandler<CreateSpecialityCommand, SpecialityDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSpecialityCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpecialityDto> Handle(CreateSpecialityCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessCollege(request.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create a speciality for this college.");
        }

        // T394 (flow 16, N1): everything that can refuse is read before anything is staged, so a refusal leaves nothing
        // for the audit pipeline's failure row to send (T201). A taken name is the Name field's refusal.
        if (!await _dbContext.Set<College>().AnyAsync(college => college.Id == request.CollegeId, cancellationToken))
        {
            throw new InvalidOperationException($"College {request.CollegeId} was not found.");
        }

        var name = request.Name.Trim();
        if (await SpecialitySaveRefusal.IsTakenAsync(_dbContext, specialityId: null, request.CollegeId, name, cancellationToken))
        {
            throw new FieldRefusalException(nameof(CreateSpecialityCommand.Name), SpecialitySaveRefusal.DuplicateName);
        }

        var speciality = new Speciality
        {
            CollegeId = request.CollegeId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _dbContext.Set<Speciality>().Add(speciality);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            // Another save took the name after the check (T394, N1). Any other refusal is left as the database gave it, so
            // the page shows its fixed failure sentence (T272) and the audit pipeline discards the refused insert (T201).
            if (!await SpecialitySaveRefusal.WasTakenAsync(
                    _dbContext, exception, specialityId: null, speciality.CollegeId, speciality.Name, cancellationToken))
            {
                throw;
            }

            throw new FieldRefusalException(nameof(CreateSpecialityCommand.Name), SpecialitySaveRefusal.DuplicateName, exception);
        }

        return new SpecialityDto(speciality.Id, speciality.CollegeId, speciality.Name, speciality.Description, speciality.IsActive);
    }
}
