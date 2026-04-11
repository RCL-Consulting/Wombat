using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;

namespace Wombat.Application.Features.Forms;

public sealed record UpdateAssessmentFormCommand(
    int Id,
    string Name,
    int? InstitutionId,
    int? SpecialityId,
    int? SubSpecialityId,
    int ScaleId,
    bool CanDelete,
    bool IsActive) : IRequest<AssessmentFormDto>;

public sealed class UpdateAssessmentFormCommandValidator : AbstractValidator<UpdateAssessmentFormCommand>
{
    public UpdateAssessmentFormCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ScaleId).GreaterThan(0);
    }
}

public sealed class UpdateAssessmentFormCommandHandler : IRequestHandler<UpdateAssessmentFormCommand, AssessmentFormDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateAssessmentFormCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto> Handle(UpdateAssessmentFormCommand request, CancellationToken cancellationToken)
    {
        var form = await _dbContext.Set<AssessmentForm>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);
        if (form is null)
        {
            throw new InvalidOperationException("The requested assessment form was not found.");
        }

        await EnsureScopeExistsAsync(request.InstitutionId, request.SpecialityId, request.SubSpecialityId, request.ScaleId, cancellationToken);

        form.Name = request.Name.Trim();
        form.InstitutionId = request.InstitutionId;
        form.SpecialityId = request.SpecialityId;
        form.SubSpecialityId = request.SubSpecialityId;
        form.ScaleId = request.ScaleId;
        form.CanDelete = request.CanDelete;
        form.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await FormMappings.GetByIdAsync(_dbContext, form.Id, cancellationToken)
            ?? throw new InvalidOperationException("The assessment form could not be loaded after it was updated.");
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

        if (specialityId.HasValue && institutionId.HasValue)
        {
            var specialityMatchesInstitution = await _dbContext.Set<Domain.Institutions.Speciality>()
                .AnyAsync(entity => entity.Id == specialityId.Value && entity.InstitutionId == institutionId.Value, cancellationToken);

            if (!specialityMatchesInstitution)
            {
                throw new InvalidOperationException("The selected speciality does not belong to the selected institution.");
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
