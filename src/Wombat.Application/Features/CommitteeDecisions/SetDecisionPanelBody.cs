using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Says which College decision body a panel sits as, or makes it a general panel again (a blank key). (T131 slice 3)
/// </summary>
/// <remarks>
/// <para>
/// Only an InstitutionalAdmin of the panel's institution or a global Administrator
/// (<see cref="CommitteeDecisionAuthorization.MaySetDecisionBody" />). Anyone else is refused before the panel is looked
/// up, and an unknown panel is refused exactly as another institution's is, so neither refusal says which ids exist
/// (T194 item 1). A global Administrator is told when an id names no panel.
/// </para>
/// <para>
/// A change reaches only what is decided afterwards. Reviews already under way keep the routing their agenda recorded
/// (Decision 3); there is nothing to strand before slice 4 adds the agenda.
/// </para>
/// <para>
/// Every check runs before the one mutation, and there is one save: the audit pipeline saves the request's context from
/// its catch, so a refusal after a mutation would commit it.
/// </para>
/// </remarks>
public sealed record SetDecisionPanelBodyCommand(
    int PanelId,
    string? DecisionBodyKey,
    ClaimsPrincipal Principal) : IRequest<DecisionPanelDetailDto>;

public sealed class SetDecisionPanelBodyCommandValidator : AbstractValidator<SetDecisionPanelBodyCommand>
{
    public SetDecisionPanelBodyCommandValidator()
    {
        RuleFor(command => command.PanelId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.DecisionBodyKey).MaximumLength(DecisionBody.KeyMaxLength);
    }
}

public sealed class SetDecisionPanelBodyCommandHandler : IRequestHandler<SetDecisionPanelBodyCommand, DecisionPanelDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public SetDecisionPanelBodyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DecisionPanelDetailDto> Handle(SetDecisionPanelBodyCommand request, CancellationToken cancellationToken)
    {
        if (!CommitteeDecisionAuthorization.HoldsDecisionBodyRole(request.Principal))
        {
            throw new UnauthorizedAccessException(CommitteeDecisionAuthorization.DecisionBodyNeedsInstitutionalAdmin);
        }

        var panel = await _dbContext.Set<DecisionPanel>()
            .Include(entity => entity.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        if (panel is null)
        {
            throw request.Principal.IsAdministrator()
                ? new InvalidOperationException("The decision panel could not be found.")
                : new UnauthorizedAccessException(CommitteeDecisionAuthorization.PanelOutOfScope);
        }

        if (!CommitteeDecisionAuthorization.MaySetDecisionBody(request.Principal, panel.InstitutionId))
        {
            throw new UnauthorizedAccessException(CommitteeDecisionAuthorization.PanelOutOfScope);
        }

        var body = await DecisionPanelBodies.DemandAsync(
            _dbContext, request.DecisionBodyKey, panel.InstitutionId, panel.SpecialityId, panel.Id, cancellationToken);

        panel.DecisionBodyKey = body?.Key;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (body is not null)
        {
            // Another request tagged a panel in the same slot between the check above and this save, and the unique
            // index refused this one: say which panel, as the check would have, and keep the refused save underneath.
            var taken = await DecisionPanelBodies.TakenRefusalAsync(
                _dbContext, body, panel.InstitutionId, panel.SpecialityId, panel.Id, cancellationToken);
            if (taken is null)
            {
                throw;
            }

            throw new InvalidOperationException(taken, exception);
        }

        return DecisionPanelBodies.ToDetailDto(panel, body?.Name);
    }
}
