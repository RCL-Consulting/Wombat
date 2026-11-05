using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Curricula;

public sealed record UpdateCurriculumCommand(
    int Id,
    int SubSpecialityId,
    string Name,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed class UpdateCurriculumCommandValidator : AbstractValidator<UpdateCurriculumCommand>
{
    public UpdateCurriculumCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);

        // T394 (flow 16, T324, N1): the page's words, under each field (CurriculumFieldMessages).
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

/// <summary>Why a curriculum version's sub-speciality can no longer change (T394, Q19, round 3's decision 5).</summary>
public enum CurriculumSubSpecialityFixedCause
{
    /// <summary>The version holds an item: a national one, or (with no adoption row, which should not happen) an own one.</summary>
    HoldsItems,

    /// <summary>An institution has adopted the version, actively or since superseded.</summary>
    Adopted
}

/// <summary>
/// <see cref="UpdateCurriculumCommand" />'s refusal of a changed sub-speciality on a version that holds an item or has an
/// adoption row (T394, Q19, decision 5): the rule <see cref="CurriculumDto.SubSpecialityFixed" /> reads, so an adoption's
/// <c>SubSpecialityId</c> always equals its curriculum's. Thrown before anything is staged. The page says its cause in its
/// result region ("Not saved. This version now holds items, …" or "Not saved. Kgosi Kgari Teaching Hospital has adopted
/// this version, …") and reads the version again.
/// </summary>
/// <remarks>
/// The adopters' names are the catalogue's institutions, which only a caller who may change the curriculum (its College, the
/// Administrator) is ever told, as the version's reach tells her (Q2); the message names none, since the audit pipeline
/// writes it into the caller's row.
/// </remarks>
public sealed class CurriculumSubSpecialityFixedException : InvalidOperationException
{
    public CurriculumSubSpecialityFixedException(CurriculumSubSpecialityFixedCause cause, IReadOnlyList<string> adoptedBy)
        : base(cause == CurriculumSubSpecialityFixedCause.HoldsItems
            ? "This curriculum version holds items, so its sub-speciality cannot change."
            : "This curriculum version has been adopted, so its sub-speciality cannot change.")
    {
        Cause = cause;
        AdoptedBy = adoptedBy;
    }

    public CurriculumSubSpecialityFixedCause Cause { get; }

    /// <summary>For <see cref="CurriculumSubSpecialityFixedCause.Adopted" />, every institution that has adopted the version, by name (ordinal); else empty.</summary>
    public IReadOnlyList<string> AdoptedBy { get; }
}

public sealed class UpdateCurriculumCommandHandler : IRequestHandler<UpdateCurriculumCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCurriculumCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(UpdateCurriculumCommand request, CancellationToken cancellationToken)
    {
        var curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.Id, cancellationToken);

        if (!request.Principal.CanAccessCollege(curriculum.SubSpeciality.Speciality.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this curriculum.");
        }

        // T394 (Q19, decision 5): read before anything is assigned, since the audit pipeline commits what a refused handler
        // staged (T201). The version may have gained an item or an adoption since the page drew its select (R3-D20).
        if (request.SubSpecialityId != curriculum.SubSpecialityId)
        {
            await EnsureSubSpecialityMayChangeAsync(curriculum, cancellationToken);
        }

        var subSpeciality = await _dbContext.Set<SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => new
            {
                entity.Name,
                entity.SpecialityId,
                SpecialityName = entity.Speciality.Name,
                entity.Speciality.CollegeId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpeciality is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        if (!request.Principal.CanAccessCollege(subSpeciality.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to move this curriculum to that sub-speciality.");
        }

        curriculum.SubSpecialityId = request.SubSpecialityId;
        curriculum.Name = request.Name.Trim();
        curriculum.Version = request.Version.Trim();
        curriculum.EffectiveFrom = request.EffectiveFrom;
        curriculum.EffectiveTo = request.EffectiveTo;
        curriculum.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CurriculumFieldMessages.IsDuplicate(exception))
        {
            // T394 (T272): a duplicate only on the unique index's refusal, under Version; any other refused save reaches the
            // page as "The curriculum could not be saved. …".
            throw CurriculumFieldMessages.DuplicateUnder(nameof(UpdateCurriculumCommand.Version), exception);
        }

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.Id, cancellationToken);
        return await CurriculumMappings.ToDtoAsync(_dbContext, curriculum, subSpeciality.SpecialityId, subSpeciality.SpecialityName, subSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, request.Principal, cancellationToken);
    }

    /// <summary>
    /// Q19: a version that holds an item or has any adoption row, active or superseded, keeps its sub-speciality (the rule
    /// <c>GetCurriculumByIdQuery</c> reads into <see cref="CurriculumDto.SubSpecialityFixed" />). The cause is the national
    /// items first, then the adopters by name; an institution's own item implies its adoption, so it is named by that.
    /// </summary>
    private async Task EnsureSubSpecialityMayChangeAsync(Curriculum curriculum, CancellationToken cancellationToken)
    {
        // The items were loaded with the curriculum, by this handler, so they are as stored now.
        var items = curriculum.Items;

        if (items.Any(item => item.OwningInstitutionId is null))
        {
            throw new CurriculumSubSpecialityFixedException(CurriculumSubSpecialityFixedCause.HoldsItems, []);
        }

        var adoptedBy = (await _dbContext.Set<InstitutionCurriculumAdoption>()
                .Where(adoption => adoption.CurriculumId == curriculum.Id)
                .Select(adoption => adoption.Institution.Name)
                .Distinct()
                .ToListAsync(cancellationToken))
            .Order(StringComparer.Ordinal)
            .ToList();

        if (adoptedBy.Count > 0)
        {
            throw new CurriculumSubSpecialityFixedException(CurriculumSubSpecialityFixedCause.Adopted, adoptedBy);
        }

        if (items.Count > 0)
        {
            throw new CurriculumSubSpecialityFixedException(CurriculumSubSpecialityFixedCause.HoldsItems, []);
        }
    }
}
