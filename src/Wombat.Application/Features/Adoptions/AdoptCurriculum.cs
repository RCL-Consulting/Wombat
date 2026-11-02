using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Adoptions;

/// <summary>
/// An institution adopts (or re-adopts) a national curriculum version for a discipline. Re-adopting a
/// different version of the same discipline deactivates the institution's current active adoption and
/// activates the new one, so at most one adoption per (institution, discipline) is ever active. (T091 phase 4.)
/// Going back to an older version is a re-adoption like any other (T392, Q7, Q21).
/// </summary>
/// <param name="ExpectedSupersededCurriculumId">
/// The version the page said adopting would supersede (the row's <c>AdoptedCurriculumId</c>), null when it said none (T392
/// build review C4). The dialog is the consent (Q6): if the institution's active adoption of the discipline is another
/// version by the save, nothing is adopted (<see cref="AdoptionRefusal.Stale" />). Required, not defaulted, so no caller
/// adopts without saying what it expects to supersede.
/// </param>
public sealed record AdoptCurriculumCommand(
    int InstitutionId,
    int CurriculumId,
    int? ExpectedSupersededCurriculumId,
    ClaimsPrincipal Principal) : IRequest<AdoptionDto>;

/// <summary>Why an adoption was refused (T392, flow 15): the page maps the reason to its words, never the message.</summary>
public enum AdoptionRefusal
{
    /// <summary>The version does not exist (a stale page).</summary>
    NotFound,

    /// <summary>The College has held the version back (T301, Q5).</summary>
    HeldBack,

    /// <summary>The institution's active adoption is already this version (a stale page: the list no longer offers it, Q7).</summary>
    AlreadyAdopted,

    /// <summary>
    /// The institution's active adoption of the discipline is not the one the page named as superseded (C4): another adoption
    /// was made since the page was read, so the dialog's consent no longer holds.
    /// </summary>
    Stale
}

/// <summary>
/// A refused adoption, thrown before anything is staged, so the audit pipeline commits nothing of it (T201). An
/// <see cref="InvalidOperationException" />, as the refusals were before T392, with its <see cref="Reason" /> typed.
/// </summary>
public sealed class AdoptionRefusedException(AdoptionRefusal reason, string message) : InvalidOperationException(message)
{
    public AdoptionRefusal Reason { get; } = reason;
}

public sealed class AdoptCurriculumCommandValidator : AbstractValidator<AdoptCurriculumCommand>
{
    public AdoptCurriculumCommandValidator()
    {
        RuleFor(command => command.InstitutionId).GreaterThan(0);
        RuleFor(command => command.CurriculumId).GreaterThan(0);
    }
}

public sealed class AdoptCurriculumCommandHandler : IRequestHandler<AdoptCurriculumCommand, AdoptionDto>
{
    /// <summary>T301 (Q5): the refusal of a version its College holds back.</summary>
    public const string HeldBackRefusal = "This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted.";

    /// <summary>The built refusal of the version the institution has already adopted.</summary>
    public const string AlreadyAdoptedRefusal = "This institution has already adopted this curriculum version.";

    /// <summary>C4: the refusal when what adopting would supersede changed after the page was read.</summary>
    public const string StaleRefusal = "Another adoption was made since this page was read, so nothing was adopted.";

    /// <summary>The built refusal of a version that does not exist.</summary>
    public const string NotFoundRefusal = "The selected curriculum could not be found.";

    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public AdoptCurriculumCommandHandler(IApplicationDbContext dbContext, TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<AdoptionDto> Handle(AdoptCurriculumCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessInstitution(request.InstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to manage adoptions for this institution.");
        }

        // Every refusal is read before anything is staged (T201: the audit pipeline commits a failed handler's staged rows).
        var curriculum = await _dbContext.Set<Curriculum>()
            .Include(entity => entity.SubSpeciality)
                .ThenInclude(entity => entity.Speciality)
                    .ThenInclude(entity => entity.College)
            .SingleOrDefaultAsync(entity => entity.Id == request.CurriculumId, cancellationToken)
            ?? throw new AdoptionRefusedException(AdoptionRefusal.NotFound, NotFoundRefusal);

        // T301 (T392, Q5): the version's flag is its release for adoption. Before T392 only the picker left a held-back
        // version out, so a stale page or a direct call adopted it.
        if (!curriculum.IsActive)
        {
            throw new AdoptionRefusedException(AdoptionRefusal.HeldBack, HeldBackRefusal);
        }

        var existingActive = await _dbContext.Set<InstitutionCurriculumAdoption>()
            .SingleOrDefaultAsync(
                entity => entity.InstitutionId == request.InstitutionId
                    && entity.SubSpecialityId == curriculum.SubSpecialityId
                    && entity.IsActive,
                cancellationToken);

        if (existingActive?.CurriculumId == curriculum.Id)
        {
            throw new AdoptionRefusedException(AdoptionRefusal.AlreadyAdopted, AlreadyAdoptedRefusal);
        }

        // C4: the dialog named what the adoption supersedes (Q6); if that is no longer the active adoption, the consent was
        // to other words, so nothing is adopted and the page reads the list again.
        if (existingActive?.CurriculumId != request.ExpectedSupersededCurriculumId)
        {
            throw new AdoptionRefusedException(AdoptionRefusal.Stale, StaleRefusal);
        }

        // What the adoption supersedes, for the result (T392, Q6): the version and its registrars still on it, the
        // institution's ACTIVE profiles pinned to it (by CurriculumId, S1).
        Superseded? superseded = null;
        if (existingActive is not null)
        {
            var old = await _dbContext.Set<Curriculum>()
                .AsNoTracking()
                .Where(entity => entity.Id == existingActive.CurriculumId)
                .Select(entity => new { entity.Name, entity.Version })
                .SingleAsync(cancellationToken);
            var registrars = await _dbContext.Set<TraineeProfile>()
                .CountAsync(
                    profile => profile.InstitutionId == request.InstitutionId
                        && profile.CurriculumId == existingActive.CurriculumId
                        && profile.IsActive,
                    cancellationToken);
            superseded = new Superseded(existingActive.CurriculumId, old.Name, old.Version, registrars);

            // Re-adoption: supersede the current active adoption for this discipline.
            existingActive.IsActive = false;
        }

        var adoption = new InstitutionCurriculumAdoption
        {
            InstitutionId = request.InstitutionId,
            CurriculumId = curriculum.Id,
            SubSpecialityId = curriculum.SubSpecialityId,
            // T325 (T392): the South African day, so an adoption between 00:00 and 02:00 there is not dated the day before.
            AdoptedOn = ProgrammeCalendar.DateOf(_timeProvider.GetUtcNow().UtcDateTime),
            IsActive = true
        };

        _dbContext.Set<InstitutionCurriculumAdoption>().Add(adoption);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AdoptionMappings.ToDto(adoption, curriculum) with
        {
            SupersededCurriculumId = superseded?.CurriculumId,
            SupersededName = superseded?.Name,
            SupersededVersion = superseded?.Version,
            SupersededRegistrarCount = superseded?.Registrars ?? 0
        };
    }

    private sealed record Superseded(int CurriculumId, string Name, string Version, int Registrars);
}
