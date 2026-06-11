using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;

namespace Wombat.Application.Features.Forms;

public sealed record CreateAssessmentFormCommand(
    string Name,
    int? InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    int ScaleId,
    bool CanDelete,
    ClaimsPrincipal Principal) : IRequest<AssessmentFormDto>;

public sealed class CreateAssessmentFormCommandValidator : AbstractValidator<CreateAssessmentFormCommand>
{
    public CreateAssessmentFormCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ScaleId).GreaterThan(0);
    }
}

public sealed class CreateAssessmentFormCommandHandler : IRequestHandler<CreateAssessmentFormCommand, AssessmentFormDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateAssessmentFormCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto> Handle(CreateAssessmentFormCommand request, CancellationToken cancellationToken)
    {
        await EnsureScopeExistsAsync(request.InstitutionId, request.SpecialityId, request.SubSpecialityId, request.ScaleId, cancellationToken);
        await FormMappings.EnsureCallerCanWriteAsync(_dbContext, request.Principal, request.InstitutionId, request.SpecialityId, request.SubSpecialityId, cancellationToken);

        var form = new AssessmentForm
        {
            Name = request.Name.Trim(),
            InstitutionId = request.InstitutionId,
            SpecialityId = request.SpecialityId,
            SubSpecialityId = request.SubSpecialityId,
            ScaleId = request.ScaleId,
            CanDelete = request.CanDelete
        };

        _dbContext.Set<AssessmentForm>().Add(form);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await FormMappings.GetByIdAsync(_dbContext, form.Id, cancellationToken)
            ?? throw new InvalidOperationException("The assessment form could not be loaded after it was created.");
    }

    private async Task EnsureScopeExistsAsync(int? institutionId, int? specialityId, int? subSpecialityId, int scaleId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Set<Domain.Epas.EntrustmentScale>().AnyAsync(entity => entity.Id == scaleId, cancellationToken))
        {
            throw new InvalidOperationException("The selected entrustment scale was not found.");
        }

        if (institutionId.HasValue && !await _dbContext.Set<Domain.Institutions.Institution>().AnyAsync(entity => entity.Id == institutionId.Value, cancellationToken))
        {
            throw new InvalidOperationException("The selected institution was not found.");
        }

        if (specialityId.HasValue && !await _dbContext.Set<Domain.Institutions.Speciality>().AnyAsync(entity => entity.Id == specialityId.Value, cancellationToken))
        {
            throw new InvalidOperationException("The selected speciality was not found.");
        }

        if (subSpecialityId.HasValue && !await _dbContext.Set<Domain.Institutions.SubSpeciality>().AnyAsync(entity => entity.Id == subSpecialityId.Value, cancellationToken))
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        if (specialityId.HasValue)
        {
            // Specialities are national now (T091); just verify existence. The discipline a form covers is
            // independent of its owning institution.
            var specialityExists = await _dbContext.Set<Domain.Institutions.Speciality>()
                .AnyAsync(entity => entity.Id == specialityId.Value, cancellationToken);

            if (!specialityExists)
            {
                throw new InvalidOperationException("The selected speciality was not found.");
            }
        }

        if (subSpecialityId.HasValue && specialityId.HasValue)
        {
            var subSpecialityMatchesSpeciality = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
                .AnyAsync(entity => entity.Id == subSpecialityId.Value && entity.SpecialityId == specialityId.Value, cancellationToken);

            if (!subSpecialityMatchesSpeciality)
            {
                throw new InvalidOperationException("The selected sub-speciality does not belong to the selected speciality.");
            }
        }
    }
}
