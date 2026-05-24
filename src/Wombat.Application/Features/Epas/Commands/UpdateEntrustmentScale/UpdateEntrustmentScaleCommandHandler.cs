using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

public sealed class UpdateEntrustmentScaleCommandHandler : IRequestHandler<UpdateEntrustmentScaleCommand, EntrustmentScaleDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateEntrustmentScaleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentScaleDto> Handle(UpdateEntrustmentScaleCommand request, CancellationToken cancellationToken)
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

        if (!string.Equals(scale.Name, trimmedName, StringComparison.Ordinal))
        {
            var nameInUse = await _dbContext.Set<EntrustmentScale>()
                .AnyAsync(entity => entity.Id != request.Id && entity.Name == trimmedName, cancellationToken);
            if (nameInUse)
            {
                throw new InvalidOperationException($"An entrustment scale named '{trimmedName}' already exists.");
            }
        }

        scale.Name = trimmedName;
        scale.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        var incomingIds = request.Levels
            .Where(level => level.Id.HasValue && level.Id.Value > 0)
            .Select(level => level.Id!.Value)
            .ToHashSet();

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

            foreach (var removed in removedLevels)
            {
                scale.Levels.Remove(removed);
                _dbContext.Set<EntrustmentLevel>().Remove(removed);
            }
        }

        foreach (var incoming in request.Levels)
        {
            var trimmedLabel = incoming.Label.Trim();
            var trimmedDescription = string.IsNullOrWhiteSpace(incoming.Description) ? null : incoming.Description.Trim();

            if (incoming.Id is { } existingId && existingId > 0)
            {
                var existing = scale.Levels.SingleOrDefault(level => level.Id == existingId)
                    ?? throw new InvalidOperationException($"Level {existingId} was not found on scale {scale.Id}.");
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

        return new EntrustmentScaleDto(
            scale.Id,
            scale.Name,
            scale.Description,
            scale.Levels
                .OrderBy(level => level.Order)
                .Select(level => new EntrustmentLevelDto(level.Id, level.Order, level.Label, level.Description))
                .ToList());
    }
}
