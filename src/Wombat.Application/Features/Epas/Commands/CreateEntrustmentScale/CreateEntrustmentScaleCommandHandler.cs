using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;

public sealed class CreateEntrustmentScaleCommandHandler : IRequestHandler<CreateEntrustmentScaleCommand, EntrustmentScaleDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateEntrustmentScaleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentScaleDto> Handle(CreateEntrustmentScaleCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may create entrustment scales.");
        }

        var trimmedName = request.Name.Trim();

        var nameInUse = await _dbContext.Set<EntrustmentScale>()
            .AnyAsync(entity => entity.Name == trimmedName, cancellationToken);
        if (nameInUse)
        {
            // T394 (flow 16, R3-C14): under Name, in the built words.
            throw new FieldRefusalException(
                nameof(CreateEntrustmentScaleCommand.Name), EntrustmentScaleBindings.NameTaken(trimmedName));
        }

        var scale = new EntrustmentScale
        {
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Levels = request.Levels
                .OrderBy(level => level.Order)
                .Select(level => new EntrustmentLevel
                {
                    Order = level.Order,
                    Label = level.Label.Trim(),
                    Description = string.IsNullOrWhiteSpace(level.Description) ? null : level.Description.Trim()
                })
                .ToList()
        };

        _dbContext.Set<EntrustmentScale>().Add(scale);
        await EntrustmentScaleNameRace.SaveAsync(
            _dbContext, null, trimmedName, nameof(CreateEntrustmentScaleCommand.Name), cancellationToken);

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
