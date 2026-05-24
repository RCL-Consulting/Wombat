using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.CreateInstitution;

public sealed class CreateInstitutionCommandHandler : IRequestHandler<CreateInstitutionCommand, InstitutionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateInstitutionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InstitutionDto> Handle(CreateInstitutionCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may create institutions.");
        }

        var institution = new Institution
        {
            Name = request.Name.Trim(),
            ShortCode = request.ShortCode.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim(),
            CreatedOn = DateTime.UtcNow
        };

        _dbContext.Set<Institution>().Add(institution);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("An institution with the same name or short code already exists.", exception);
        }

        return new InstitutionDto(institution.Id, institution.Name, institution.ShortCode, institution.ContactEmail, institution.IsActive, institution.CreatedOn);
    }
}
