using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Commands.CreateInstitution;

public sealed class CreateInstitutionCommandHandler : IRequestHandler<CreateInstitutionCommand, InstitutionDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public CreateInstitutionCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    /// <summary>The handler as the tests that predate T400 build it: the system clock.</summary>
    public CreateInstitutionCommandHandler(IApplicationDbContext dbContext)
        : this(dbContext, TimeProvider.System)
    {
    }

    /// <remarks>
    /// T400 (flow 18, R3-I09, T378): a name or short code another institution holds is refused under its own field, as the
    /// record's save refuses it (<see cref="UpdateInstitutionCommandHandler" />): asked before anything is staged, the
    /// short code first, so the audit's failure row carries the refusal and nothing else (AuditPipelineBehavior, T201).
    /// The unique indexes stay the last word: a clash they find (two creates at once) is the same refusal, under the field
    /// that clashes when it is read again (the build review's N-6), else Short code. Until T400 the clash was found only by the save, and thrown as a plain exception the page could not place.
    /// </remarks>
    public async Task<InstitutionDto> Handle(CreateInstitutionCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may create institutions.");
        }

        var institutions = _dbContext.Set<Institution>();
        var name = request.Name.Trim();
        var shortCode = request.ShortCode.Trim();

        if (await institutions.AnyAsync(entity => entity.ShortCode == shortCode, cancellationToken))
        {
            throw new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.Duplicate, InstitutionRecordField.ShortCode, UpdateInstitutionCommandHandler.Duplicate);
        }

        if (await institutions.AnyAsync(entity => entity.Name == name, cancellationToken))
        {
            throw new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.Duplicate, InstitutionRecordField.Name, UpdateInstitutionCommandHandler.Duplicate);
        }

        var institution = new Institution
        {
            Name = name,
            ShortCode = shortCode,
            ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim(),
            CreatedOn = _timeProvider.GetUtcNow().UtcDateTime
        };

        institutions.Add(institution);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            // A name or code saved by someone else since the checks above: only the unique index's refusal is a duplicate;
            // any other fault goes on as the unexpected failure it is. The refusal sits under the field that now clashes,
            // read again (the build review's N-6): the short code when it is taken, else the name when that is, else (the
            // clash already gone) the short code, as the first check would have placed it.
            var field = await ClashingFieldAsync(institutions, name, shortCode, cancellationToken);
            throw new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.Duplicate, field, UpdateInstitutionCommandHandler.Duplicate, exception);
        }

        return new InstitutionDto(institution.Id, institution.Name, institution.ShortCode, institution.ContactEmail, institution.IsActive, institution.CreatedOn);
    }

    /// <summary>
    /// The field a refused insert clashes on, read again after the unique index refused it: the short code when another
    /// institution holds it, else the name when one does, else the short code. A read that fails leaves the short code,
    /// so the refusal is never replaced by the read's own fault.
    /// </summary>
    private static async Task<InstitutionRecordField> ClashingFieldAsync(
        IQueryable<Institution> institutions, string name, string shortCode, CancellationToken cancellationToken)
    {
        try
        {
            if (await institutions.AsNoTracking().AnyAsync(entity => entity.ShortCode == shortCode, cancellationToken))
            {
                return InstitutionRecordField.ShortCode;
            }

            return await institutions.AsNoTracking().AnyAsync(entity => entity.Name == name, cancellationToken)
                ? InstitutionRecordField.Name
                : InstitutionRecordField.ShortCode;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return InstitutionRecordField.ShortCode;
        }
    }
}
