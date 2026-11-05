using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
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
                // T394 (flow 16, R3-C14): under Name, in the built words.
                throw new FieldRefusalException(
                    nameof(UpdateEntrustmentScaleCommand.Name), EntrustmentScaleBindings.NameTaken(trimmedName));
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

        // T394 (flow 16, Q18): a save that removes rungs is asked Q6's questions first, so its words say which item needs
        // which rung (and offer to put it back), then the ended records', then the decisions'. Only then Q16 (T395).
        if (await EntrustmentScaleReferences.RefusalForRemovedRungsAsync(_dbContext, scale.Id, removedLevels, cancellationToken)
            is { } needed)
        {
            throw needed;
        }

        if (removedLevels.Count > 0)
        {
            var removedIds = removedLevels.Select(level => level.Id).ToList();
            var pendingRefs = await _dbContext.Set<PendingEntrustmentDecision>()
                .AnyAsync(decision => removedIds.Contains(decision.AuthorisedLevelId), cancellationToken);
            var issuedRefs = await _dbContext.Set<EntrustmentDecision>()
                .AnyAsync(decision => removedIds.Contains(decision.AuthorisedLevelId), cancellationToken);
            if (pendingRefs || issuedRefs)
            {
                throw new EntrustmentScaleUpdateRefusedException(
                    EntrustmentScaleUpdateRefusal.RungDecided, [], 0, [], EntrustmentScaleRefusalWords.RungDecided);
            }
        }

        // T395 (T394, flow 16, Q16): every use of a scale reads a rung by its rank (an item's minimum, a progress row's, a
        // form's answer, a decision's level), so on a scale in use a rung may not move or go: the CPSA ladder's nine
        // items' minima would silently name other rungs. A reorder is a kept rung whose rank changes; a new rung added at
        // the top and a relabelled one change no rank and stay allowed (P2). "In use" is the delete's seven uses, the
        // same read the page offers Up, Down and Remove by (EntrustmentScaleDto.InUse), asked before anything is assigned.
        var storedOrderById = scale.Levels.ToDictionary(level => level.Id, level => level.Order);
        var reordered = request.Levels.Any(level =>
            level.Id is { } id && id > 0 && storedOrderById.TryGetValue(id, out var stored) && stored != level.Order);
        if ((reordered || removedLevels.Count > 0)
            && await EntrustmentScaleUses.IsInUseAsync(_dbContext, scale, cancellationToken))
        {
            throw new EntrustmentScaleUpdateRefusedException(
                EntrustmentScaleUpdateRefusal.InUse, [], 0, [], EntrustmentScaleRefusalWords.InUse);
        }

        // A rename refuses nothing (T253): the seeds bind by seed key and the builder by id, and neither moves with the
        // name. A schema still bound by the old name loses its ladder, so the save says which, read before anything is
        // changed.
        var renameWarning = renamed
            ? await EntrustmentScaleReferences.DescribeWhatARenameUnbindsAsync(_dbContext, scale.Name, trimmedName, cancellationToken)
            : null;

        // Nothing below this line throws but the save.
        //
        // A reorder moves rungs inside the unique (scale, order) index (T397). EF Core orders a save's updates so that no
        // single statement breaks a unique index, and a swap (4 to 5 and 5 to 4) has no such order: it gives up with a
        // circular dependency before anything is sent, which the in-memory provider, holding no index, never shows. So a
        // save that moves a stored rung first parks each moved rung at the negative of its new order (no stored order is
        // negative, and the new ones are distinct) and deletes the removed rungs, then writes the rest; both in one
        // ambient transaction, so a refusal of the second write (a taken name, T394 code N-7) undoes the first.
        var moved = request.Levels
            .Where(level => level.Id is { } id && id > 0 && storedOrderById.TryGetValue(id, out var stored) && stored != level.Order)
            .ToList();
        using var transaction = moved.Count > 0
            ? new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled)
            : null;

        foreach (var removed in removedLevels)
        {
            scale.Levels.Remove(removed);
            _dbContext.Set<EntrustmentLevel>().Remove(removed);
        }

        if (moved.Count > 0)
        {
            foreach (var incoming in moved)
            {
                scale.Levels.First(level => level.Id == incoming.Id).Order = -incoming.Order;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        scale.Name = trimmedName;
        scale.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

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

        await EntrustmentScaleNameRace.SaveAsync(
            _dbContext, scale.Id, trimmedName, nameof(UpdateEntrustmentScaleCommand.Name), cancellationToken);
        transaction?.Complete();

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
