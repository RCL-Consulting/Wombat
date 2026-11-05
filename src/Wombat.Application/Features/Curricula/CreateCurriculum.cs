using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

public sealed record CreateCurriculumCommand(
    int SubSpecialityId,
    string Name,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed class CreateCurriculumCommandValidator : AbstractValidator<CreateCurriculumCommand>
{
    public CreateCurriculumCommandValidator()
    {
        // T394 (flow 16, T324, N1): each failure in the words the page shows under its field, never a default's.
        RuleFor(command => command.SubSpecialityId).GreaterThan(0).WithMessage(CurriculumFieldMessages.SubSpecialityRequired);
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(CurriculumFieldMessages.NameRequired)
            .MaximumLength(CurriculumFieldMessages.NameMaxLength).WithMessage(CurriculumFieldMessages.NameTooLong);
        RuleFor(command => command.Version)
            .NotEmpty().WithMessage(CurriculumFieldMessages.VersionRequired)
            .MaximumLength(CurriculumFieldMessages.VersionMaxLength).WithMessage(CurriculumFieldMessages.VersionTooLong);
        RuleFor(command => command.EffectiveTo)
            .GreaterThanOrEqualTo(command => command.EffectiveFrom)
            .When(command => command.EffectiveTo.HasValue)
            .WithMessage(CurriculumFieldMessages.EffectiveToBeforeFrom);
    }
}

/// <summary>
/// The words of the curriculum commands' field refusals (T394, flow 16, T324, N1; R3-D02, D03, D09a, D19): one place for the
/// three commands and for the page, which checks the same fields before it sends.
/// </summary>
public static class CurriculumFieldMessages
{
    public const int NameMaxLength = 200;

    public const int VersionMaxLength = 64;

    /// <summary>NEW: a create with no sub-speciality chosen (no board draws it).</summary>
    public const string SubSpecialityRequired = "Choose a sub-speciality.";

    /// <summary>R3-D02.</summary>
    public const string NameRequired = "Enter a name.";

    /// <summary>N1's words for a College's name, said of a curriculum's too.</summary>
    public const string NameTooLong = "A name is at most 200 characters.";

    /// <summary>R3-D02.</summary>
    public const string VersionRequired = "Enter a version.";

    /// <summary>NEW (no board draws it).</summary>
    public const string VersionTooLong = "A version is at most 64 characters.";

    /// <summary>R3-D09a: the clone's empty New version.</summary>
    public const string NewVersionRequired = "Enter the new version.";

    /// <summary>R3-D19, under Effective to.</summary>
    public const string EffectiveToBeforeFrom = "Effective to must be on or after Effective from.";

    /// <summary>The built duplicate (R3-D03, D09b), under Version or New version, on the unique index's refusal only (T272).</summary>
    public const string Duplicate = "A curriculum with the same name and version already exists for this sub-speciality.";

    /// <summary>
    /// Whether a refused save is the unique index (sub-speciality, name, version) refusing it (T272), the only unique index a
    /// curriculum's row meets. Until T394 every refused save read as a duplicate; any other now reaches the page as its fixed
    /// failure sentence.
    /// </summary>
    internal static bool IsDuplicate(DbUpdateException exception) => PostgresErrors.IsUniqueViolation(exception);

    /// <summary>The duplicate under <paramref name="field" />, the database's refusal kept underneath so the pipeline drops the save (T201).</summary>
    internal static FieldRefusalException DuplicateUnder(string field, DbUpdateException exception) => new(field, Duplicate, exception);
}

public sealed class CreateCurriculumCommandHandler : IRequestHandler<CreateCurriculumCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCurriculumCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(CreateCurriculumCommand request, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => new
            {
                entity.Name,
                entity.SpecialityId,
                SpecialityName = entity.Speciality.Name,
                entity.Speciality.CollegeId,
                CollegeName = entity.Speciality.College.Name,
                entity.DefaultEntrustmentScaleId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpeciality is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        if (!request.Principal.CanAccessCollege(subSpeciality.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to create a curriculum in this sub-speciality.");
        }

        var curriculum = new Curriculum
        {
            SubSpecialityId = request.SubSpecialityId,
            Name = request.Name.Trim(),
            Version = request.Version.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            // T392, flow 15, F5 (T301's second half): a new version starts held back, so no institution adopts it before its
            // College opens it (UpdateCurriculumCommand's IsActive, the College's box). The seeders open theirs explicitly.
            IsActive = false
        };

        _dbContext.Set<Curriculum>().Add(curriculum);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CurriculumFieldMessages.IsDuplicate(exception))
        {
            // T394 (T272): a duplicate only on the unique index's refusal, under Version (R3-D03); any other refused save
            // reaches the page as "The curriculum could not be created. …" (R3-D04).
            throw CurriculumFieldMessages.DuplicateUnder(nameof(CreateCurriculumCommand.Version), exception);
        }

        return CurriculumAdminScope.ForCaller(
            new CurriculumDto(curriculum.Id, subSpeciality.SpecialityId, curriculum.SubSpecialityId, subSpeciality.SpecialityName, subSpeciality.Name, subSpeciality.CollegeName, curriculum.Name, curriculum.Version, curriculum.EffectiveFrom, curriculum.EffectiveTo, curriculum.IsActive, [], subSpeciality.DefaultEntrustmentScaleId),
            subSpeciality.CollegeId,
            request.Principal);
    }
}
