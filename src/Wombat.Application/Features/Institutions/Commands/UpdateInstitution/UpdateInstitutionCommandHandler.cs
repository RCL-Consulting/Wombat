using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

public sealed class UpdateInstitutionCommandHandler : IRequestHandler<UpdateInstitutionCommand, InstitutionDto>
{
    /// <summary>The refusal of a name or short code another institution holds (built; I08's words).</summary>
    public const string Duplicate = "An institution with the same name or short code already exists.";

    private readonly IApplicationDbContext _dbContext;

    public UpdateInstitutionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// T376 (flow 12, I08): a name or short code another institution holds is refused beside its own field, so it is asked
    /// before the save, the short code first (the field the board draws it on), and the unique indexes stay the last word:
    /// a clash they find (two saves at once) is the same refusal under Short code. Neither names the other institution.
    /// </remarks>
    public async Task<InstitutionDto> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessInstitution(request.Id))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this institution.");
        }

        var institutions = _dbContext.Set<Institution>();
        var institution = await institutions.SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.NotFound, InstitutionRecordField.None, $"Institution {request.Id} was not found.");

        var name = request.Name.Trim();
        var shortCode = request.ShortCode.Trim();

        if (await institutions.AnyAsync(entity => entity.Id != request.Id && entity.ShortCode == shortCode, cancellationToken))
        {
            throw new InstitutionUpdateRefusedException(InstitutionUpdateRefusal.Duplicate, InstitutionRecordField.ShortCode, Duplicate);
        }

        if (await institutions.AnyAsync(entity => entity.Id != request.Id && entity.Name == name, cancellationToken))
        {
            throw new InstitutionUpdateRefusedException(InstitutionUpdateRefusal.Duplicate, InstitutionRecordField.Name, Duplicate);
        }

        institution.Name = name;
        institution.ShortCode = shortCode;
        institution.ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            // A name or code saved by someone else since the checks above (T376 build review, B4): only the unique index's
            // refusal is a duplicate; a concurrency or connection fault goes on as the unexpected failure it is.
            throw new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.Duplicate, InstitutionRecordField.ShortCode, Duplicate, exception);
        }

        return new InstitutionDto(institution.Id, institution.Name, institution.ShortCode, institution.ContactEmail, institution.IsActive, institution.CreatedOn);
    }
}
