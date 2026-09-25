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
/// <param name="DecisionCadence">
/// How often a committee decides this EPA, or null for no published cadence (T131). Required, not defaulted, and
/// nullable: <see cref="QuotaPeriod.AcademicYear" /> is the zero value, so a defaulted or non-nullable argument would make
/// an EPA due every year without anyone choosing it.
/// </param>
/// <param name="DecisionBodyKey">The body that decides this EPA, by key, or null for the general panel (T131). Required, not defaulted, for the same reason.</param>
/// <param name="DecisionIsOpportunistic">Whether the EPA is decided as opportunity allows (T131, O7). Needs a cadence.</param>
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
    QuotaPeriod? DecisionCadence,
    string? DecisionBodyKey,
    bool DecisionIsOpportunistic,
    ClaimsPrincipal Principal,
    int? ScaleId = null) : IRequest<CurriculumDto>;

/// <param name="QuotaPeriod">Required, not defaulted: see <see cref="AddCurriculumItemCommand" />. An edit that omitted it would reset a semester item to a yearly one.</param>
/// <param name="PermittedToolKeys">Required, not defaulted: see <see cref="AddCurriculumItemCommand" />. An edit that omitted it would let every instrument credit this EPA.</param>
/// <param name="DecisionCadence">Required, not defaulted, null allowed: see <see cref="AddCurriculumItemCommand" />. An edit that omitted it would clear the cadence, or make the EPA due every year.</param>
/// <param name="DecisionBodyKey">Required, not defaulted, null allowed. An edit that omitted it would send EPAs 4 and 5 back to the general panel.</param>
/// <param name="DecisionIsOpportunistic">Required, not defaulted. An edit that omitted it would make EPAs 8, 9 and 13 closing lines.</param>
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
    QuotaPeriod? DecisionCadence,
    string? DecisionBodyKey,
    bool DecisionIsOpportunistic,
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
        RuleFor(command => command.DecisionCadence).IsInEnum();
        RuleFor(command => command.DecisionBodyKey).MaximumLength(DecisionBody.KeyMaxLength);
        RuleFor(command => command.DecisionIsOpportunistic)
            .Equal(false)
            .When(command => command.DecisionCadence is null)
            .WithMessage(DecisionValidation.OpportunisticNeedsACadence);
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
        RuleFor(command => command.DecisionCadence).IsInEnum();
        RuleFor(command => command.DecisionBodyKey).MaximumLength(DecisionBody.KeyMaxLength);
        RuleFor(command => command.DecisionIsOpportunistic)
            .Equal(false)
            .When(command => command.DecisionCadence is null)
            .WithMessage(DecisionValidation.OpportunisticNeedsACadence);
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

internal static class DecisionValidation
{
    /// <summary>
    /// An opportunistic decision is one that is never overdue within its cadence (T131, O7); with no cadence it is never
    /// due at all, so the flag would say nothing and a later cadence would inherit a choice nobody made for it.
    /// </summary>
    public const string OpportunisticNeedsACadence =
        "An EPA can be decided as opportunity allows only if it has a decision cadence.";
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

        // The curriculum's CollegeAdmin or an Administrator edits the national core; an InstitutionalAdmin adds an
        // institution-local item to the (adopted) national curriculum (T091 phase 3). Decided for this curriculum's
        // College, so a CollegeAdmin of another College who is also an InstitutionalAdmin adds their institution's item.
        var owningInstitutionId = CurriculumItemEpas.OwnerOfNewItem(request.Principal, curriculum.SubSpeciality.Speciality.CollegeId);

        if (!CurriculumItemEpas.MayWrite(request.Principal, curriculum.SubSpeciality.Speciality.CollegeId, owningInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        // T195: a national item names a national EPA of the curriculum's sub-speciality; a local one may also name its
        // institution's own local EPAs. The Add picker lists exactly these.
        await CurriculumItemEpas.EnsureNameableAsync(_dbContext, curriculum, owningInstitutionId, request.EpaId, cancellationToken);

        CurriculumAdminScope.EnsureEpaNotYetOn(curriculum, request.EpaId, exceptItemId: null, request.Principal);

        await CurriculumMappings.EnsureScaleCanExpressMinimaAsync(
            _dbContext, request.ScaleId, request.MinimumLevelOrder, request.MinimumLevelByStageJson, currentScaleId: null, cancellationToken);
        await CurriculumMappings.EnsurePermittedToolsExistAsync(_dbContext, request.PermittedToolKeys, cancellationToken);
        await CurriculumMappings.EnsureDecisionBodyExistsAsync(_dbContext, request.DecisionBodyKey, cancellationToken);

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
            PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(request.PermittedToolKeys),
            DecisionCadence = request.DecisionCadence,
            DecisionBodyKey = DecisionBody.NormalizeKey(request.DecisionBodyKey),
            DecisionIsOpportunistic = request.DecisionIsOpportunistic
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return await CurriculumMappings.ToDtoAsync(_dbContext, curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, true, request.Principal, cancellationToken);
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
        if (!CurriculumItemEpas.MayWrite(request.Principal, curriculum.SubSpeciality.Speciality.CollegeId, item.OwningInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        // T195: judged against the STORED item's owner, and on the requested EPA whether or not it changed. The edit
        // row's picker lists exactly these.
        await CurriculumItemEpas.EnsureNameableAsync(_dbContext, curriculum, item.OwningInstitutionId, request.EpaId, cancellationToken);

        CurriculumAdminScope.EnsureEpaNotYetOn(curriculum, request.EpaId, request.ItemId, request.Principal);

        // Before the first mutation (the audit pipeline commits a half-finished one), and judged on the REQUESTED
        // values: a save that changes the scale, the flat minimum and the stage minima together is re-pinning the
        // item, and must not be refused for the values it replaces (T136). item.ScaleId is still the stored ladder
        // here, so the refusal can name each value as the rung the operator knew it by.
        await CurriculumMappings.EnsureScaleCanExpressMinimaAsync(
            _dbContext, request.ScaleId, request.MinimumLevelOrder, request.MinimumLevelByStageJson, item.ScaleId, cancellationToken);
        await CurriculumMappings.EnsurePermittedToolsExistAsync(_dbContext, request.PermittedToolKeys, cancellationToken);
        await CurriculumMappings.EnsureDecisionBodyExistsAsync(_dbContext, request.DecisionBodyKey, cancellationToken);

        item.EpaId = request.EpaId;
        item.RequiredCount = request.RequiredCount;
        item.QuotaPeriod = request.QuotaPeriod;
        item.MinimumLevelOrder = request.MinimumLevelOrder;
        item.WindowMonths = request.WindowMonths;
        item.Weight = request.Weight;
        item.MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(request.MinimumLevelByStageJson);
        item.ScaleId = request.ScaleId;
        item.PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(request.PermittedToolKeys);
        item.DecisionCadence = request.DecisionCadence;
        item.DecisionBodyKey = DecisionBody.NormalizeKey(request.DecisionBodyKey);
        item.DecisionIsOpportunistic = request.DecisionIsOpportunistic;

        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return await CurriculumMappings.ToDtoAsync(_dbContext, curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, true, request.Principal, cancellationToken);
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

        // National core item -> CollegeAdmin; institution-local item -> the owning InstitutionalAdmin. The same rule as Add,
        // Update and the EPA picker (CurriculumItemEpas.MayWrite), so the four cannot drift apart.
        if (!CurriculumItemEpas.MayWrite(request.Principal, curriculum.SubSpeciality.Speciality.CollegeId, item.OwningInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        _dbContext.Set<CurriculumItem>().Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return await CurriculumMappings.ToDtoAsync(_dbContext, curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, curriculum.SubSpeciality.Speciality.College.Name, true, request.Principal, cancellationToken);
    }
}
