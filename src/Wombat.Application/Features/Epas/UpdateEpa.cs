using System.Data.Common;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <param name="IsActive">
/// Unticking it deactivates the EPA from now, as <see cref="DeactivateEpaCommand" /> does; ticking it on an inactive EPA
/// reactivates it and credits what was filed while it was inactive (T196, D48).
/// </param>
public sealed record UpdateEpaCommand(
    int Id,
    int SubSpecialityId,
    string Code,
    string Title,
    string? Description,
    string? RequiredKnowledgeSkills,
    EpaCategory Category,
    bool IsActive,
    ClaimsPrincipal Principal) : IRequest<UpdateEpaResult>;

/// <param name="CompletionsCredited">
/// Completions filed while the EPA was inactive that this save credited by reactivating it (T196); zero on any other save.
/// </param>
public sealed record UpdateEpaResult(EpaDto Epa, int CompletionsCredited);

public sealed class UpdateEpaCommandValidator : AbstractValidator<UpdateEpaCommand>
{
    public UpdateEpaCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.SubSpecialityId).GreaterThan(0);
        RuleFor(command => command.Code).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(4000);
        RuleFor(command => command.RequiredKnowledgeSkills).MaximumLength(8000);
        RuleFor(command => command.Category).IsInEnum();
    }
}

public sealed class UpdateEpaCommandHandler : IRequestHandler<UpdateEpaCommand, UpdateEpaResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICreditApplier _creditApplier;
    private readonly IEpaCreditLock _epaCreditLock;
    private readonly TimeProvider _timeProvider;

    public UpdateEpaCommandHandler(
        IApplicationDbContext dbContext,
        ICreditApplier creditApplier,
        IEpaCreditLock epaCreditLock,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _creditApplier = creditApplier;
        _epaCreditLock = epaCreditLock;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>PostgreSQL's unique_violation.</summary>
    private const string UniqueViolation = "23505";

    private const string DuplicateCode = "An EPA with the same code already exists for this sub-speciality.";

    private const string ProgressChanged =
        "Curriculum progress changed while this EPA was being reactivated, so nothing was saved. Save again.";

    public async Task<UpdateEpaResult> Handle(UpdateEpaCommand request, CancellationToken cancellationToken)
    {
        // T230. Held before the EPA is read, and until the save below commits, so no completion's credit can judge the EPA
        // in between: one that already holds it is waited for, and is then among the candidates a reactivation reads; one
        // that comes after waits for this save and reads what it wrote. The deactivation's moment is read under it too, so
        // every completion it let credit has an earlier moment, as a rebuild judges. Before the authorization check, which
        // needs the EPA read; a refused caller holds it only until the throw disposes the hold.
        await using var hold = await _epaCreditLock.HoldForChangeAsync(request.Id, cancellationToken);

        var epa = await _dbContext.Set<Epa>()
            .Include(entity => entity.SubSpeciality)
            .ThenInclude(subSpeciality => subSpeciality.Speciality)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (epa is null)
        {
            throw new InvalidOperationException("The requested EPA was not found.");
        }

        // National EPA -> CollegeAdmin; institution-local extra -> the owning InstitutionalAdmin (T091 phase 3).
        var authorized = epa.OwningInstitutionId is null
            ? request.Principal.CanAccessCollege(epa.SubSpeciality.Speciality.CollegeId)
            : request.Principal.CanAccessInstitution(epa.OwningInstitutionId.Value);
        if (!authorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this EPA.");
        }

        var subSpeciality = await _dbContext.Set<Domain.Institutions.SubSpeciality>()
            .Where(entity => entity.Id == request.SubSpecialityId)
            .Select(entity => new { entity.Name, entity.Speciality.CollegeId, CollegeName = entity.Speciality.College.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (subSpeciality is null)
        {
            throw new InvalidOperationException("The selected sub-speciality was not found.");
        }

        var targetAuthorized = epa.OwningInstitutionId is null
            ? request.Principal.CanAccessCollege(subSpeciality.CollegeId)
            : request.Principal.CanAccessInstitution(epa.OwningInstitutionId.Value);
        if (!targetAuthorized)
        {
            throw new UnauthorizedAccessException("You do not have permission to move this EPA to that sub-speciality.");
        }

        // T196, D48: reactivating credits what was filed while the EPA was inactive. Every read that needs happens here,
        // before the first mutation below, because the audit pipeline's catch saves this request's DbContext: a failure
        // after the reactivation would otherwise commit it without the credit it owes. The rows are confined to the
        // pause and reach no caller; only a count comes back (ActivityReadBoundaryTests.ScopeExemptHandlers).
        ResumedEpaCredit? resumedCredit = null;
        if (request.IsActive && !epa.IsActive)
        {
            var candidates = await ResumedEpaCredit.LoadCandidatesAsync(
                _dbContext.Set<Activity>(), _dbContext.Set<ActivityTypeVersion>(), epa.DeactivatedOn, cancellationToken);
            resumedCredit = await ResumedEpaCredit.PlanAsync(_creditApplier, epa, candidates, cancellationToken);
        }

        epa.SubSpecialityId = request.SubSpecialityId;
        epa.Code = request.Code.Trim();
        epa.Title = request.Title.Trim();
        epa.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        epa.RequiredKnowledgeSkills = string.IsNullOrWhiteSpace(request.RequiredKnowledgeSkills) ? null : request.RequiredKnowledgeSkills.Trim();
        epa.Category = request.Category;

        var completionsCredited = 0;
        if (request.IsActive)
        {
            epa.Reactivate();
            completionsCredited = resumedCredit?.Apply() ?? 0;
        }
        else
        {
            // From now. An EPA already inactive keeps the moment its pause began (Epa.DeactivatedOn).
            epa.Deactivate(_timeProvider.GetUtcNow().UtcDateTime);
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Something moved one of the same progress rows while this save was being made (the rows' xmin token). Not a
            // completion on this EPA, which waits for the hold above (T230): an Administrator rebuild, which takes none.
            // The audit pipeline discards a refused save (T201), so nothing was written.
            throw new InvalidOperationException(ProgressChanged, exception);
        }
        catch (DbUpdateException exception) when (exception.InnerException is DbException { SqlState: UniqueViolation })
        {
            // Two unique indexes can refuse this save: the EPA's code within its sub-speciality, and, on a reactivation, a
            // progress row's (item, trainee, semester), when another save opened the same row after this one read the table
            // (a rebuild, say; a second reactivation of the same EPA now waits for the hold above, T230). Which one is read
            // back, as ScheduleCommitteeReview does, rather than parsed out of a provider exception this layer cannot see.
            // Before the T196 review every refusal was reported as a duplicate code. The refusal stays underneath, so the
            // audit pipeline drops the save (T201).
            //
            // Under the hold (T230) the read-back runs in its transaction, which EF rolled back to the savepoint it takes
            // before a save in a transaction of the caller's, so the refusal has not left it unusable.
            if (await CodeTakenAsync(epa, cancellationToken))
            {
                throw new InvalidOperationException(DuplicateCode, exception);
            }

            throw new InvalidOperationException(ProgressChanged, exception);
        }

        await hold.CommitAsync(cancellationToken);

        return new UpdateEpaResult(
            new EpaDto(epa.Id, epa.SubSpecialityId, subSpeciality.Name, subSpeciality.CollegeName, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, epa.IsActive, epa.CreatedOn),
            completionsCredited);
    }

    /// <summary>
    /// Whether another EPA already holds this one's code in the namespace its unique index enforces: the sub-speciality
    /// for a national EPA, the sub-speciality and owning institution for a local one (<c>EpaConfiguration</c>).
    /// </summary>
    private Task<bool> CodeTakenAsync(Epa epa, CancellationToken cancellationToken)
        => _dbContext.Set<Epa>()
            .AsNoTracking()
            .AnyAsync(other => other.Id != epa.Id
                && other.SubSpecialityId == epa.SubSpecialityId
                && other.OwningInstitutionId == epa.OwningInstitutionId
                && other.Code == epa.Code, cancellationToken);
}
