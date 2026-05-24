using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

public sealed class UpdateInstitutionCommandHandler : IRequestHandler<UpdateInstitutionCommand, InstitutionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateInstitutionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InstitutionDto> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessInstitution(request.Id))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this institution.");
        }

        var institution = await _dbContext.Set<Institution>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Institution {request.Id} was not found.");

        institution.Name = request.Name.Trim();
        institution.ShortCode = request.ShortCode.Trim();
        institution.ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim();
        institution.IsActive = request.IsActive;

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
