using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var speciality = new Speciality
        {
            InstitutionId = request.InstitutionId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _dbContext.Set<Speciality>().Add(speciality);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A speciality with the same name already exists for this institution.", exception);
        }

        return new SpecialityDto(speciality.Id, speciality.InstitutionId, speciality.Name, speciality.Description, speciality.IsActive);
    }
}
