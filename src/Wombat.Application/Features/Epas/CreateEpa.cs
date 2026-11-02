using System.Data.Common;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

public sealed record CreateEpaCommand(
    int SubSpecialityId,
    string Code,
    string Title,
    string? Description,
    string? RequiredKnowledgeSkills,
    EpaCategory Category,
    ClaimsPrincipal Principal) : IRequest<EpaDto>
{
    public CreateEpaCommand(
        int subSpecialityId,
        string code,
        string title,
        string? description,
        string? requiredKnowledgeSkills,
        ClaimsPrincipal principal)
        : this(subSpecialityId, code, title, description, requiredKnowledgeSkills, EpaCategory.Core, principal)
    {
    }
}

/// <summary>
/// The sentences an EPA's fields are refused with (T392, flow 15, lane C): the validators' and the page's own checks say the
/// same, under the field (F4), never a property's name (T324).
/// </summary>
public static class EpaFieldMessages
{
    public const string SubSpecialityRequired = "Choose a sub-speciality.";
    public const string CodeRequired = "Enter a code.";
    public const string CodeTooLong = "A code is at most 64 characters.";
    public const string TitleRequired = "Enter a title.";
    public const string TitleTooLong = "A title is at most 200 characters.";
    public const string DescriptionTooLong = "A description is at most 4,000 characters.";
    public const string RequiredKnowledgeSkillsTooLong = "The required knowledge and skills are at most 8,000 characters.";
    public const string CategoryRequired = "Choose a category.";

    /// <summary>A real duplicate, refused by the code's unique index only (T272): the built sentence.</summary>
    public const string DuplicateCode = "An EPA with the same code already exists for this sub-speciality.";

    /// <summary>
    /// F9, Q12: an institution's own EPA in a discipline the institution has not actively adopted, on a create, or on a save
    /// that moves it there. Said to the institution's own admin; an Administrator, whose reads span institutions, is told
    /// which institution.
    /// </summary>
    public static string NotAdopted(string disciplineName, string? institutionName = null)
        => institutionName is null
            ? $"Your institution has not adopted {disciplineName}, so an EPA cannot be added to it."
            : $"{institutionName} has not adopted {disciplineName}, so an EPA cannot be added to it.";
}

public sealed class CreateEpaCommandValidator : AbstractValidator<CreateEpaCommand>
{
    public CreateEpaCommandValidator()
    {
        RuleFor(command => command.SubSpecialityId).GreaterThan(0).WithMessage(EpaFieldMessages.SubSpecialityRequired);
        RuleFor(command => command.Code).NotEmpty().WithMessage(EpaFieldMessages.CodeRequired)
            .MaximumLength(64).WithMessage(EpaFieldMessages.CodeTooLong);
        RuleFor(command => command.Title).NotEmpty().WithMessage(EpaFieldMessages.TitleRequired)
            .MaximumLength(200).WithMessage(EpaFieldMessages.TitleTooLong);
        RuleFor(command => command.Description).MaximumLength(4000).WithMessage(EpaFieldMessages.DescriptionTooLong);
        RuleFor(command => command.RequiredKnowledgeSkills).MaximumLength(8000).WithMessage(EpaFieldMessages.RequiredKnowledgeSkillsTooLong);
        RuleFor(command => command.Category).IsInEnum().WithMessage(EpaFieldMessages.CategoryRequired);
    }
}

public sealed class CreateEpaCommandHandler : IRequestHandler<CreateEpaCommand, EpaDto>
{
    /// <summary>PostgreSQL's unique_violation.</summary>
    private const string UniqueViolation = "23505";

    private readonly IApplicationDbContext _dbContext;

    public CreateEpaCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Every refusal is found before the EPA is staged (T392): the audit pipeline commits what a failed handler staged
    /// (T201), so the discipline check (F9) reads the adoption first. A save the database refuses is discarded by the
    /// pipeline's own retry.
    /// </remarks>
    public async Task<EpaDto> Handle(CreateEpaCommand request, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => new
            {
                entity.Name,
                SpecialityName = entity.Speciality.Name,
                entity.Speciality.CollegeId,
                CollegeName = entity.Speciality.College.Name
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpeciality is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        // A CollegeAdmin/Administrator authors the national core; an InstitutionalAdmin adds an
        // institution-local extra (T091 phase 3). The owner determines who may later edit it.
        var owningInstitutionId = EpaReads.CreatesOwn(request.Principal) ? request.Principal.GetInstitutionId() : null;

        var authorized = owningInstitutionId is null
            ? request.Principal.CanAccessCollege(subSpeciality.CollegeId)
            : request.Principal.CanAccessInstitution(owningInstitutionId.Value);
        if (!authorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to create an EPA in this sub-speciality.");
        }

        // F9, Q12 (T291's command half): an institution's own EPA only in a discipline it has actively adopted. Until T392 the
        // check was the institution alone, so its own EPA could be made in any College's discipline, where no list showed it.
        var disciplineName = EpaReads.DisciplineName(subSpeciality.SpecialityName, subSpeciality.Name);
        if (owningInstitutionId is { } owner
            && !(await EpaReads.AdoptedDisciplinesAsync(_dbContext, owner, cancellationToken)).Contains(request.SubSpecialityId))
        {
            throw new FieldRefusalException(nameof(CreateEpaCommand.SubSpecialityId), EpaFieldMessages.NotAdopted(disciplineName));
        }

        var epa = new Epa
        {
            SubSpecialityId = request.SubSpecialityId,
            OwningInstitutionId = owningInstitutionId,
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            RequiredKnowledgeSkills = string.IsNullOrWhiteSpace(request.RequiredKnowledgeSkills) ? null : request.RequiredKnowledgeSkills.Trim(),
            Category = request.Category,
            CreatedOn = DateTime.UtcNow
        };

        _dbContext.Set<Epa>().Add(epa);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is DbException { SqlState: UniqueViolation })
        {
            // T272: a duplicate only when the code's unique index refused the save (EpaConfiguration: the sub-speciality's
            // namespace for a national EPA, the sub-speciality and institution's for an own one), the only unique indexes an
            // EPA's insert meets. Until T272 every refused save read as a duplicate; any other now reaches the page as an
            // unexpected failure, in its fixed words. The refusal stays underneath, so the pipeline drops the save (T201).
            throw new FieldRefusalException(nameof(CreateEpaCommand.Code), EpaFieldMessages.DuplicateCode, exception);
        }

        var institutionName = owningInstitutionId is { } id
            ? await EpaReads.InstitutionNameAsync(_dbContext, id, cancellationToken)
            : null;

        return new EpaDto(epa.Id, epa.SubSpecialityId, subSpeciality.Name, subSpeciality.CollegeName, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, epa.IsActive, epa.CreatedOn)
        {
            OwningInstitutionId = owningInstitutionId,
            OwningInstitutionName = institutionName,
            NamesOwner = EpaReads.NamesOwners(request.Principal),
            MayChange = true,
            DisciplineName = disciplineName
        };
    }
}
