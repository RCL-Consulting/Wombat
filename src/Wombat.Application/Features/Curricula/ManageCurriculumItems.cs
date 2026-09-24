using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

/// <param name="RequiredCount">The target per <paramref name="QuotaPeriod" /> window, not a programme total (T130, D18).</param>
/// <param name="QuotaPeriod">
/// Which window the target is for. Positional and required, deliberately not defaulted: a defaulted argument is one
/// a future call site can silently drop, and dropping this one would turn a per-semester target into a per-year one.
/// </param>
/// <param name="PermittedToolKeys">
/// The instruments that may credit this EPA, as tool keys; null or empty means any instrument (T122, D21). Required,
/// not defaulted, for <paramref name="QuotaPeriod" />'s reason: an edit that dropped it would clear the list, and a
/// cleared list is permissive, so nothing would ever say it had happened.
/// </param>
public sealed record AddCurriculumItemCommand(
    int CurriculumId,
    int EpaId,
    int RequiredCount,
    QuotaPeriod QuotaPeriod,
    int MinimumLevelOrder,
    int WindowMonths,
    double? Weight,
    string? MinimumLevelByStageJson,
    IReadOnlyList<string>? PermittedToolKeys,
    ClaimsPrincipal Principal,
    int? ScaleId = null) : IRequest<CurriculumDto>;

/// <param name="QuotaPeriod">Required, not defaulted: see <see cref="AddCurriculumItemCommand" />. An edit that omitted it would reset a semester item to a yearly one.</param>
/// <param name="PermittedToolKeys">Required, not defaulted: see <see cref="AddCurriculumItemCommand" />. An edit that omitted it would let every instrument credit this EPA.</param>
public sealed record UpdateCurriculumItemCommand(
    int CurriculumId,
    int ItemId,
    int EpaId,
    int RequiredCount,
    QuotaPeriod QuotaPeriod,
    int MinimumLevelOrder,
    int WindowMonths,
    double? Weight,
    string? MinimumLevelByStageJson,
    IReadOnlyList<string>? PermittedToolKeys,
    ClaimsPrincipal Principal,
    int? ScaleId = null) : IRequest<CurriculumDto>;

public sealed record RemoveCurriculumItemCommand(int CurriculumId, int ItemId, ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed class AddCurriculumItemCommandValidator : AbstractValidator<AddCurriculumItemCommand>
{
    public AddCurriculumItemCommandValidator()
    {
        RuleFor(command => command.CurriculumId).GreaterThan(0);
        RuleFor(command => command.EpaId).GreaterThan(0);
        RuleFor(command => command.RequiredCount).GreaterThan(0);
        RuleFor(command => command.QuotaPeriod).IsInEnum();
        RuleFor(command => command.MinimumLevelOrder).InclusiveBetween(1, 20);
        RuleFor(command => command.WindowMonths).GreaterThan(0);
        RuleFor(command => command.MinimumLevelByStageJson)
            .Must(StageOverridesValidation.BeValidStageOverridesJson)
            .WithMessage("Stage overrides must be a JSON object keyed by training year, with integer levels 1-20.");
        RuleForEach(command => command.PermittedToolKeys).NotEmpty().MaximumLength(64);
    }
}

public sealed class UpdateCurriculumItemCommandValidator : AbstractValidator<UpdateCurriculumItemCommand>
{
    public UpdateCurriculumItemCommandValidator()
    {
        RuleFor(command => command.CurriculumId).GreaterThan(0);
        RuleFor(command => command.ItemId).GreaterThan(0);
        RuleFor(command => command.EpaId).GreaterThan(0);
        RuleFor(command => command.RequiredCount).GreaterThan(0);
        RuleFor(command => command.QuotaPeriod).IsInEnum();
        RuleFor(command => command.MinimumLevelOrder).InclusiveBetween(1, 20);
        RuleFor(command => command.WindowMonths).GreaterThan(0);
        RuleFor(command => command.MinimumLevelByStageJson)
            .Must(StageOverridesValidation.BeValidStageOverridesJson)
            .WithMessage("Stage overrides must be a JSON object keyed by training year, with integer levels 1-20.");
        RuleForEach(command => command.PermittedToolKeys).NotEmpty().MaximumLength(64);
    }
}

public sealed class RemoveCurriculumItemCommandValidator : AbstractValidator<RemoveCurriculumItemCommand>
{
    public RemoveCurriculumItemCommandValidator()
    {
        RuleFor(command => command.CurriculumId).GreaterThan(0);
        RuleFor(command => command.ItemId).GreaterThan(0);
    }
}

internal static class StageOverridesValidation
{
    public static bool BeValidStageOverridesJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            var parsed = CurriculumItem.ParseStageOverrides(json);
            // If the caller provided something non-trivial but parsing dropped everything, reject.
            using var document = System.Text.Json.JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return false;
            }
            var declared = 0;
            foreach (var _ in document.RootElement.EnumerateObject())
            {
                declared++;
            }
            return declared == 0 || parsed.Count == declared;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }
}

public sealed class AddCurriculumItemCommandHandler : IRequestHandler<AddCurriculumItemCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public AddCurriculumItemCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(AddCurriculumItemCommand request, CancellationToken cancellationToken)
    {
        var curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        CurriculumMappings.EnsureCurriculumCanBeEditedInPlace();

        // A CollegeAdmin/Administrator edits the national core; an InstitutionalAdmin adds an
        // institution-local item to the (adopted) national curriculum (T091 phase 3).
        var owningInstitutionId = !request.Principal.IsAdministrator()
            && !request.Principal.IsCollegeAdmin()
            && request.Principal.IsInstitutionalAdmin()
                ? request.Principal.GetInstitutionId()
                : null;

        var authorized = owningInstitutionId is null
            ? request.Principal.CanAccessCollege(curriculum.SubSpeciality.Speciality.CollegeId)
            : request.Principal.CanAccessInstitution(owningInstitutionId.Value);
        if (!authorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        if (curriculum.Items.Any(entity => entity.EpaId == request.EpaId))
        {
            throw new InvalidOperationException("This curriculum already contains the selected EPA.");
        }

        await CurriculumMappings.EnsureScaleCanExpressMinimaAsync(
            _dbContext, request.ScaleId, request.MinimumLevelOrder, request.MinimumLevelByStageJson, currentScaleId: null, cancellationToken);
        await CurriculumMappings.EnsurePermittedToolsExistAsync(_dbContext, request.PermittedToolKeys, cancellationToken);

        curriculum.Items.Add(new CurriculumItem
        {
            EpaId = request.EpaId,
            OwningInstitutionId = owningInstitutionId,
            RequiredCount = request.RequiredCount,
            QuotaPeriod = request.QuotaPeriod,
            MinimumLevelOrder = request.MinimumLevelOrder,
            WindowMonths = request.WindowMonths,
            Weight = request.Weight,
            MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(request.MinimumLevelByStageJson),
            ScaleId = request.ScaleId,
            PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(request.PermittedToolKeys)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, true);
    }
}

public sealed class UpdateCurriculumItemCommandHandler : IRequestHandler<UpdateCurriculumItemCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCurriculumItemCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(UpdateCurriculumItemCommand request, CancellationToken cancellationToken)
    {
        var curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        CurriculumMappings.EnsureCurriculumCanBeEditedInPlace();

        // Coarse gate (don't leak item existence): only the curriculum's CollegeAdmin or some InstitutionalAdmin
        // (who may own a local item) may touch it. The fine-grained per-item owner check follows. (T091 phase 3.)
        if (!request.Principal.CanAccessCollege(curriculum.SubSpeciality.Speciality.CollegeId)
            && !request.Principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        var item = curriculum.Items.SingleOrDefault(entity => entity.Id == request.ItemId);
        if (item is null)
        {
            throw new InvalidOperationException("The requested curriculum item was not found.");
        }

        // National core item -> CollegeAdmin; institution-local item -> the owning InstitutionalAdmin.
        var authorized = item.OwningInstitutionId is null
            ? request.Principal.CanAccessCollege(curriculum.SubSpeciality.Speciality.CollegeId)
            : request.Principal.CanAccessInstitution(item.OwningInstitutionId.Value);
        if (!authorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        if (curriculum.Items.Any(entity => entity.Id != request.ItemId && entity.EpaId == request.EpaId))
        {
            throw new InvalidOperationException("This curriculum already contains the selected EPA.");
        }

        // Before the first mutation (the audit pipeline commits a half-finished one), and judged on the REQUESTED
        // values: a save that changes the scale, the flat minimum and the stage minima together is re-pinning the
        // item, and must not be refused for the values it replaces (T136). item.ScaleId is still the stored ladder
        // here, so the refusal can name each value as the rung the operator knew it by.
        await CurriculumMappings.EnsureScaleCanExpressMinimaAsync(
            _dbContext, request.ScaleId, request.MinimumLevelOrder, request.MinimumLevelByStageJson, item.ScaleId, cancellationToken);
        await CurriculumMappings.EnsurePermittedToolsExistAsync(_dbContext, request.PermittedToolKeys, cancellationToken);

        item.EpaId = request.EpaId;
        item.RequiredCount = request.RequiredCount;
        item.QuotaPeriod = request.QuotaPeriod;
        item.MinimumLevelOrder = request.MinimumLevelOrder;
        item.WindowMonths = request.WindowMonths;
        item.Weight = request.Weight;
        item.MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(request.MinimumLevelByStageJson);
        item.ScaleId = request.ScaleId;
        item.PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(request.PermittedToolKeys);

        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, true);
    }
}

public sealed class RemoveCurriculumItemCommandHandler : IRequestHandler<RemoveCurriculumItemCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveCurriculumItemCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(RemoveCurriculumItemCommand request, CancellationToken cancellationToken)
    {
        var curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        CurriculumMappings.EnsureCurriculumCanBeEditedInPlace();

        // Coarse gate (don't leak item existence): only the curriculum's CollegeAdmin or some InstitutionalAdmin
        // (who may own a local item) may touch it. The fine-grained per-item owner check follows. (T091 phase 3.)
        if (!request.Principal.CanAccessCollege(curriculum.SubSpeciality.Speciality.CollegeId)
            && !request.Principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        var item = curriculum.Items.SingleOrDefault(entity => entity.Id == request.ItemId);
        if (item is null)
        {
            throw new InvalidOperationException("The requested curriculum item was not found.");
        }

        // National core item -> CollegeAdmin; institution-local item -> the owning InstitutionalAdmin.
        var authorized = item.OwningInstitutionId is null
            ? request.Principal.CanAccessCollege(curriculum.SubSpeciality.Speciality.CollegeId)
            : request.Principal.CanAccessInstitution(item.OwningInstitutionId.Value);
        if (!authorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        _dbContext.Set<CurriculumItem>().Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, true);
    }
}
