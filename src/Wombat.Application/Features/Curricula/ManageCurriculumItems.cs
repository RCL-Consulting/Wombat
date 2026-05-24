using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

public sealed record AddCurriculumItemCommand(
    int CurriculumId,
    int EpaId,
    int RequiredCount,
    int MinimumLevelOrder,
    int WindowMonths,
    double? Weight,
    string? MinimumLevelByStageJson,
    ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed record UpdateCurriculumItemCommand(
    int CurriculumId,
    int ItemId,
    int EpaId,
    int RequiredCount,
    int MinimumLevelOrder,
    int WindowMonths,
    double? Weight,
    string? MinimumLevelByStageJson,
    ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed record RemoveCurriculumItemCommand(int CurriculumId, int ItemId, ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed class AddCurriculumItemCommandValidator : AbstractValidator<AddCurriculumItemCommand>
{
    public AddCurriculumItemCommandValidator()
    {
        RuleFor(command => command.CurriculumId).GreaterThan(0);
        RuleFor(command => command.EpaId).GreaterThan(0);
        RuleFor(command => command.RequiredCount).GreaterThan(0);
        RuleFor(command => command.MinimumLevelOrder).InclusiveBetween(1, 20);
        RuleFor(command => command.WindowMonths).GreaterThan(0);
        RuleFor(command => command.MinimumLevelByStageJson)
            .Must(StageOverridesValidation.BeValidStageOverridesJson)
            .WithMessage("Stage overrides must be a JSON object keyed by training year, with integer levels 1-20.");
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
        RuleFor(command => command.MinimumLevelOrder).InclusiveBetween(1, 20);
        RuleFor(command => command.WindowMonths).GreaterThan(0);
        RuleFor(command => command.MinimumLevelByStageJson)
            .Must(StageOverridesValidation.BeValidStageOverridesJson)
            .WithMessage("Stage overrides must be a JSON object keyed by training year, with integer levels 1-20.");
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

        if (!request.Principal.CanAccessInstitution(curriculum.SubSpeciality.Speciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        if (curriculum.Items.Any(entity => entity.EpaId == request.EpaId))
        {
            throw new InvalidOperationException("This curriculum already contains the selected EPA.");
        }

        curriculum.Items.Add(new CurriculumItem
        {
            EpaId = request.EpaId,
            RequiredCount = request.RequiredCount,
            MinimumLevelOrder = request.MinimumLevelOrder,
            WindowMonths = request.WindowMonths,
            Weight = request.Weight,
            MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(request.MinimumLevelByStageJson)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, true);
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

        if (!request.Principal.CanAccessInstitution(curriculum.SubSpeciality.Speciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        var item = curriculum.Items.SingleOrDefault(entity => entity.Id == request.ItemId);
        if (item is null)
        {
            throw new InvalidOperationException("The requested curriculum item was not found.");
        }

        if (curriculum.Items.Any(entity => entity.Id != request.ItemId && entity.EpaId == request.EpaId))
        {
            throw new InvalidOperationException("This curriculum already contains the selected EPA.");
        }

        item.EpaId = request.EpaId;
        item.RequiredCount = request.RequiredCount;
        item.MinimumLevelOrder = request.MinimumLevelOrder;
        item.WindowMonths = request.WindowMonths;
        item.Weight = request.Weight;
        item.MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(request.MinimumLevelByStageJson);

        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, true);
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

        if (!request.Principal.CanAccessInstitution(curriculum.SubSpeciality.Speciality.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        var item = curriculum.Items.SingleOrDefault(entity => entity.Id == request.ItemId);
        if (item is null)
        {
            throw new InvalidOperationException("The requested curriculum item was not found.");
        }

        _dbContext.Set<CurriculumItem>().Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        curriculum = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);
        return CurriculumMappings.ToDto(curriculum, curriculum.SubSpeciality.SpecialityId, curriculum.SubSpeciality.Speciality.Name, curriculum.SubSpeciality.Name, true);
    }
}
