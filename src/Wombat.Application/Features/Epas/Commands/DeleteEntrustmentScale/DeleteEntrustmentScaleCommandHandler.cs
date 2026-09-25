using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;

public sealed class DeleteEntrustmentScaleCommandHandler : IRequestHandler<DeleteEntrustmentScaleCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteEntrustmentScaleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteEntrustmentScaleCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may delete entrustment scales.");
        }

        var scale = await _dbContext.Set<EntrustmentScale>()
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Entrustment scale {request.Id} was not found.");

        var msfRefs = await _dbContext.Set<MsfQuestion>()
            .AnyAsync(question => question.ScaleId == request.Id, cancellationToken);
        if (msfRefs)
        {
            throw new InvalidOperationException(
                "This entrustment scale is referenced by one or more MSF questions and cannot be deleted.");
        }

        // T109 pinned curriculum minima and scored progress rows to scales, both ON DELETE RESTRICT.
        // Without these two checks the delete reaches the database and comes back as a raw
        // DbUpdateException, which is a stack trace where an explanation belongs.
        var curriculumRefs = await _dbContext.Set<CurriculumItem>()
            .AnyAsync(item => item.ScaleId == request.Id, cancellationToken);
        if (curriculumRefs)
        {
            throw new InvalidOperationException(
                "This entrustment scale is pinned to one or more curriculum items and cannot be deleted.");
        }

        var progressRefs = await _dbContext.Set<CurriculumItemProgress>()
            .AnyAsync(progress => progress.MinimumLevelScaleId == request.Id, cancellationToken);
        if (progressRefs)
        {
            throw new InvalidOperationException(
                "Trainee progress has been scored against this entrustment scale and it cannot be deleted.");
        }

        // A sub-speciality's default scale is the third ON DELETE RESTRICT reference (T076), and until T232 the one
        // this handler never asked about: the delete reached the database and came back as that same raw exception.
        await EntrustmentScaleReferences.ThrowIfDefaultOfASubSpecialityAsync(_dbContext, request.Id, cancellationToken);

        // Deleting a scale a schema binds to by name does exactly what renaming one does — the key stops
        // resolving and the cross-scale refusal can never fire for that type again — so the same guard has
        // to stand on this door too (T109).
        await EntrustmentScaleReferences.ThrowIfNamedByAPublishedSchemaAsync(
            _dbContext, scale.Name, "Deleting it", cancellationToken);

        var levelIds = scale.Levels.Select(level => level.Id).ToList();
        if (levelIds.Count > 0)
        {
            var pendingRefs = await _dbContext.Set<PendingEntrustmentDecision>()
                .AnyAsync(decision => levelIds.Contains(decision.AuthorisedLevelId), cancellationToken);
            var issuedRefs = await _dbContext.Set<EntrustmentDecision>()
                .AnyAsync(decision => levelIds.Contains(decision.AuthorisedLevelId), cancellationToken);
            if (pendingRefs || issuedRefs)
            {
                throw new InvalidOperationException(
                    "Levels of this entrustment scale are referenced by entrustment decisions and cannot be deleted.");
            }
        }

        foreach (var level in scale.Levels.ToList())
        {
            _dbContext.Set<EntrustmentLevel>().Remove(level);
        }

        _dbContext.Set<EntrustmentScale>().Remove(scale);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
