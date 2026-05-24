using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Forms;
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
        var scale = await _dbContext.Set<EntrustmentScale>()
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Entrustment scale {request.Id} was not found.");

        var formRefs = await _dbContext.Set<AssessmentForm>()
            .AnyAsync(form => form.ScaleId == request.Id, cancellationToken);
        if (formRefs)
        {
            throw new InvalidOperationException(
                "This entrustment scale is referenced by one or more assessment forms and cannot be deleted.");
        }

        var msfRefs = await _dbContext.Set<MsfQuestion>()
            .AnyAsync(question => question.ScaleId == request.Id, cancellationToken);
        if (msfRefs)
        {
            throw new InvalidOperationException(
                "This entrustment scale is referenced by one or more MSF questions and cannot be deleted.");
        }

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
