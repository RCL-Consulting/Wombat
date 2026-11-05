using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

/// <remarks>
/// The new version starts held back (T392, F5) with the version cloned from's national items. The page leaves for the new
/// version by <c>?cloned=‹CurriculumId›</c> (T394, Q5), and the arriving page reads both versions again to say "… cloned
/// from 11.1, with its 15 national items. It is held back."
/// </remarks>
public sealed record CloneCurriculumAsNewVersionCommand(
    int CurriculumId,
    string Version,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    ClaimsPrincipal Principal) : IRequest<CurriculumDto>;

public sealed class CloneCurriculumAsNewVersionCommandValidator : AbstractValidator<CloneCurriculumAsNewVersionCommand>
{
    public CloneCurriculumAsNewVersionCommandValidator()
    {
        RuleFor(command => command.CurriculumId).GreaterThan(0);

        // T394 (flow 16, R3-D09a, D19): the page's words, under New version and the clone's Effective to.
        RuleFor(command => command.Version)
            .NotEmpty().WithMessage(CurriculumFieldMessages.NewVersionRequired)
            .MaximumLength(CurriculumFieldMessages.VersionMaxLength).WithMessage(CurriculumFieldMessages.VersionTooLong);
        RuleFor(command => command.EffectiveTo)
            .GreaterThanOrEqualTo(command => command.EffectiveFrom)
            .When(command => command.EffectiveTo.HasValue)
            .WithMessage(CurriculumFieldMessages.EffectiveToBeforeFrom);
    }
}

public sealed class CloneCurriculumAsNewVersionCommandHandler : IRequestHandler<CloneCurriculumAsNewVersionCommand, CurriculumDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CloneCurriculumAsNewVersionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto> Handle(CloneCurriculumAsNewVersionCommand request, CancellationToken cancellationToken)
    {
        var current = await CurriculumMappings.LoadCurriculumAsync(_dbContext, request.CurriculumId, cancellationToken);

        if (!request.Principal.CanAccessCollege(current.SubSpeciality.Speciality.CollegeId))
        {
            throw new UnauthorizedAccessException("You do not have permission to clone this curriculum.");
        }

        var clone = current.CloneAsNewVersion(request.Version, request.EffectiveFrom, request.EffectiveTo);

        _dbContext.Set<Curriculum>().Add(clone);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (CurriculumFieldMessages.IsDuplicate(exception))
        {
            // T394 (T272, R3-D09b): a duplicate only on the unique index's refusal, under New version (the command's Version);
            // any other refused save reaches the page as "The new version could not be made. …".
            throw CurriculumFieldMessages.DuplicateUnder(nameof(CloneCurriculumAsNewVersionCommand.Version), exception);
        }

        clone = await CurriculumMappings.LoadCurriculumAsync(_dbContext, clone.Id, cancellationToken);
        return await CurriculumMappings.ToDtoAsync(_dbContext, clone, clone.SubSpeciality.SpecialityId, clone.SubSpeciality.Speciality.Name, clone.SubSpeciality.Name, clone.SubSpeciality.Speciality.College.Name, request.Principal, cancellationToken);
    }
}
