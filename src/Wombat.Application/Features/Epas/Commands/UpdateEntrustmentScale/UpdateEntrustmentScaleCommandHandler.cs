using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

public sealed class UpdateEntrustmentScaleCommandHandler : IRequestHandler<UpdateEntrustmentScaleCommand, UpdateEntrustmentScaleResult>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateEntrustmentScaleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Every check runs before the first assignment, and the order is load-bearing (T253 review). The audit pipeline's
    /// catch saves this request's DbContext, so a name or level assigned before a later check throws is COMMITTED under
    /// the refused save. Before T253 that was a stray rename; since a rename refuses nothing and its warning rides only
    /// on a successful result, it was a rename that unbound a form's ladder with only the refusal on the page.
    /// </remarks>
    public async Task<UpdateEntrustmentScaleResult> Handle(UpdateEntrustmentScaleCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may update entrustment scales.");
        }

        var scale = await _dbContext.Set<EntrustmentScale>()
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Entrustment scale {request.Id} was not found.");

        var trimmedName = request.Name.Trim();
        var renamed = !string.Equals(scale.Name, trimmedName, StringComparison.Ordinal);

        if (renamed)
        {
            var nameInUse = await _dbContext.Set<EntrustmentScale>()
                .AnyAsync(entity => entity.Id != request.Id && entity.Name == trimmedName, cancellationToken);
            if (nameInUse)
            {
                throw new InvalidOperationException($"An entrustment scale named '{trimmedName}' already exists.");
            }
        }

        var existingIds = scale.Levels.Select(level => level.Id).ToHashSet();
        var incomingIds = request.Levels
            .Where(level => level.Id.HasValue && level.Id.Value > 0)
            .Select(level => level.Id!.Value)
            .ToHashSet();

        var unknownIds = incomingIds.Where(id => !existingIds.Contains(id)).Order().ToList();
        if (unknownIds.Count > 0)
        {
            throw new InvalidOperationException($"Level {unknownIds[0]} was not found on scale {scale.Id}.");
        }

        var removedLevels = scale.Levels.Where(existing => !incomingIds.Contains(existing.Id)).ToList();
        if (removedLevels.Count > 0)
        {
            var removedIds = removedLevels.Select(level => level.Id).ToList();
            var pendingRefs = await _dbContext.Set<PendingEntrustmentDecision>()
                .AnyAsync(decision => removedIds.Contains(decision.AuthorisedLevelId), cancellationToken);
            var issuedRefs = await _dbContext.Set<EntrustmentDecision>()
                .AnyAsync(decision => removedIds.Contains(decision.AuthorisedLevelId), cancellationToken);
            if (pendingRefs || issuedRefs)
            {
                throw new InvalidOperationException(
                    "One or more levels are referenced by entrustment decisions and cannot be removed.");
            }

            // A pinned curriculum item asserts that its ordinals are rungs on this scale (T109). The
            // validator forces the incoming set to be contiguous from 1, so a removal always removes the
            // TOP rung — exactly the one an "Independent" or "Supervises others" minimum is most likely to
            // name. Losing it would make that minimum permanently unreachable, silently.
            await EntrustmentScaleReferences.ThrowIfPinnedItemNeedsARemovedRungAsync(
                _dbContext,
                scale.Id,
                request.Levels.Select(level => level.Order).ToHashSet(),
                cancellationToken);
        }

        // A rename refuses nothing (T253): the seeds bind by seed key and the builder by id, and neither moves with the
        // name. A schema still bound by the old name loses its ladder, so the save says which, read before anything is
        // changed.
        var renameWarning = renamed
            ? await EntrustmentScaleReferences.DescribeWhatARenameUnbindsAsync(_dbContext, scale.Name, trimmedName, cancellationToken)
            : null;

        // Nothing below this line throws.
        scale.Name = trimmedName;
        scale.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        foreach (var removed in removedLevels)
        {
            scale.Levels.Remove(removed);
            _dbContext.Set<EntrustmentLevel>().Remove(removed);
        }

        foreach (var incoming in request.Levels)
        {
            var trimmedLabel = incoming.Label.Trim();
            var trimmedDescription = string.IsNullOrWhiteSpace(incoming.Description) ? null : incoming.Description.Trim();

            if (incoming.Id is { } existingId && existingId > 0)
            {
                var existing = scale.Levels.First(level => level.Id == existingId);
                existing.Order = incoming.Order;
                existing.Label = trimmedLabel;
                existing.Description = trimmedDescription;
            }
            else
            {
                scale.Levels.Add(new EntrustmentLevel
                {
                    Order = incoming.Order,
                    Label = trimmedLabel,
                    Description = trimmedDescription
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateEntrustmentScaleResult(
            new EntrustmentScaleDto(
                scale.Id,
                scale.Name,
                scale.Description,
                scale.Levels
                    .OrderBy(level => level.Order)
                    .Select(level => new EntrustmentLevelDto(level.Id, level.Order, level.Label, level.Description))
                    .ToList()),
            renameWarning);
    }

}
