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
/// Only an InstitutionalAdmin of the panel's institution or a global Administrator, and never someone who holds Trainee
/// beside either role (<see cref="CommitteeDecisionAuthorization.MaySetDecisionBody" />, T256). Anyone else is refused
/// before the panel is looked up, and an unknown panel is refused exactly as another institution's is, so neither refusal
/// says which ids exist (T194 item 1). A global Administrator is told when an id names no panel.
/// </para>
/// <para>
/// A change reaches only reviews scheduled afterwards, and it is refused while the panel holds a review that is scheduled,
/// in progress, or decided and awaiting ratification (T131 slice 5), naming it. Such a review took its type from the
/// panel's body when it was scheduled (<see cref="CommitteeReviewTypes" />), and its agenda is planned again at Start from
/// the panel as it is then: a general panel tagged as the neonatal CCC would hold a progression review whose Start adds the
/// committee's EPAs, and a neonatal panel made general an entrustment-only review where the rule allows none. A ratified
/// review, under appeal or final, keeps its type and agenda, and changing the tag changes nothing about it.
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
        // Who may set any panel's body at all, before the panel is looked up: never someone who holds Trainee (T256).
        CommitteeDecisionAuthorization.DemandDecisionBodyRole(request.Principal);

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

        // T131 slice 5: not under a review that is still open (see the remarks). Read before the one mutation.
        if (!string.Equals(DecisionBody.NormalizeKey(panel.DecisionBodyKey), body?.Key, StringComparison.Ordinal))
        {
            var open = await _dbContext.Set<CommitteeReview>()
                .AsNoTracking()
                .Where(review => review.PanelId == panel.Id && OpenStates.Contains(review.State))
                .OrderBy(review => review.Id)
                .Select(review => new { review.Id, review.AcademicYear, review.Semester, review.State })
                .FirstOrDefaultAsync(cancellationToken);

            if (open is not null)
            {
                throw new InvalidOperationException(
                    ReviewStillOpen(open.Id, new AcademicPeriod(open.AcademicYear, open.Semester), open.State));
            }
        }

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

    /// <summary>The states of a review whose type and agenda still follow the panel's body (see the remarks).</summary>
    private static readonly CommitteeReviewState[] OpenStates =
        [CommitteeReviewState.Scheduled, CommitteeReviewState.InProgress, CommitteeReviewState.Decided];

    /// <summary>The refusal naming the first open review before the panel. (T131 slice 5)</summary>
    internal static string ReviewStillOpen(int reviewId, AcademicPeriod period, CommitteeReviewState state)
        => $"Review #{reviewId} ({period}, {StateLabel(state)}) still sits before this panel. What a review decides, and its " +
           "agenda, follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as " +
           "until its open reviews are ratified, or closed if formative.";

    private static string StateLabel(CommitteeReviewState state) => state switch
    {
        CommitteeReviewState.Scheduled => "scheduled",
        CommitteeReviewState.InProgress => "in progress",
        CommitteeReviewState.Decided => "decided, not yet ratified",
        _ => state.ToString()
    };
}
