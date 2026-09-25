using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record RatifyCommitteeDecisionCommand(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class RatifyCommitteeDecisionCommandValidator : AbstractValidator<RatifyCommitteeDecisionCommand>
{
    public RatifyCommitteeDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

/// <summary>
/// Ratifies a decided review and issues every entrustment decision staged at it as a STAR, superseding the trainee's
/// active STAR on each EPA.
/// </summary>
/// <remarks>
/// <para>
/// Every check runs before the first mutation, and everything is written by one save. <see cref="IApplicationDbContext" />
/// has no transaction, and the audit pipeline saves the request's context from its catch, so a refusal or a failure
/// after the first mutation would commit a half-ratified review: before T131 the handler saved the new STARs, then looked
/// each one back up and superseded the prior one, in two saves.
/// </para>
/// <para>
/// A STAR's evidence links are built here, on the server, from the review's frozen snapshot rows the staged decision
/// names (<see cref="EntrustmentEvidenceLink.FromSnapshot" />), never from anything the chair typed (D38, T131).
/// </para>
/// </remarks>
public sealed class RatifyCommitteeDecisionCommandHandler : IRequestHandler<RatifyCommitteeDecisionCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RatifyCommitteeDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(RatifyCommitteeDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .Include(entity => entity.AgendaLines)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // Authorise first: an unknown review and one the caller does not chair get the one refusal (T194 item 1). The
        // panel's chair, and no one else: T165 removed the Administrator's bypass from the actions that take a decision
        // (D46).
        review = CommitteeDecisionAuthorization.DemandChairedReview(request.Principal, review);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);

        // The review's state, and (T165) the decision's recorded attendance, which must hold a quorum: the chair and at
        // least one other. Checked here, before the staged decisions are read and long before review.Ratify, the first
        // mutation: a refused ratify issues no STAR, even after the audit pipeline's save.
        review.DemandRatifiable();
        var chairUserId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);
        var utcNow = DateTime.UtcNow;

        var pending = await _dbContext.Set<PendingEntrustmentDecision>()
            .Where(p => p.ReviewId == review.Id)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        // T131 slice 4, Decision 6: every closing line of the agenda is staged or deferred with a reason. Right after the
        // quorum, and before the staged decisions themselves are judged: recording the decision demanded it too, so this
        // refuses only a review whose staged decision on a closing line has since been removed (T167's exception). The
        // page shows Ratify disabled with the same lines named (CommitteeAgendaDto.RatifyBlockedReason).
        // T235: or decided elsewhere. Recording settled every line another sitting had decided by then; a line still due
        // whose window another sitting has decided since is read now, by the window rule the agenda page reads, and is
        // settled so when the agenda closes below. Read before the first mutation, so none of this review's STARs is
        // among the records: the window is decided by another sitting's.
        var decidedElsewhere = (await AgendaPlanner.PlanForReviewAsync(
                _dbContext, review, ProgrammeCalendar.DateOf(utcNow), cancellationToken))
            .DecidedElsewhere;
        review.EnsureAgendaClosable(pending.Select(p => p.EpaId), decidedElsewhere);

        // T167: ratifying is what turns a staged decision into a STAR, so each is held to the trainee's curriculum again
        // here, not only when it was staged; the item, its ladder, its EPA or the trainee's curriculum may have changed
        // since.
        await StarCurriculum.DemandStagedAsync(_dbContext, review.TraineeUserId, pending, cancellationToken);

        // D38 (T131): no STAR without the evidence it rests on, each item a line of this review's frozen snapshot.
        var evidence = await StagedEvidence.DemandGroundedAsync(_dbContext, review, pending, cancellationToken);

        // One staged decision per EPA (the table is unique on review and EPA). Checked here as well, because two would
        // each supersede the trainee's prior STAR, and the second SupersedeBy would throw after the first mutation.
        if (pending.GroupBy(p => p.EpaId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException(TwoStagedOnOneEpa);
        }

        // The trainee's active STARs on those EPAs, which the new ones supersede. Loaded before the first mutation.
        var epaIds = pending.Select(p => p.EpaId).ToArray();
        var priorActive = epaIds.Length == 0
            ? []
            : await _dbContext.Set<EntrustmentDecision>()
                .Where(d => d.TraineeUserId == review.TraineeUserId
                    && epaIds.Contains(d.EpaId)
                    && d.Status == EntrustmentDecisionStatus.Active)
                .ToListAsync(cancellationToken);

        // Every STAR is built before anything is tracked: Issue validates, and a refusal after an Add would be committed.
        var issued = pending
            .Select(staged => (Staged: staged, Decision: EntrustmentDecision.Issue(
                review.TraineeUserId,
                staged.EpaId,
                staged.AuthorisedLevelId,
                staged.IssuedOn,
                staged.ExpiresOn,
                review.Id,
                chairUserId,
                staged.Rationale,
                evidence[staged.Id].Select(EntrustmentEvidenceLink.FromSnapshot))))
            .ToList();

        // The first mutation.
        review.Ratify(chairUserId, utcNow);

        foreach (var (staged, decision) in issued)
        {
            _dbContext.Set<EntrustmentDecision>().Add(decision);
            _dbContext.Set<PendingEntrustmentDecision>().Remove(staged);

            // Through the navigation: the new STAR has no id until the save below stores both.
            foreach (var prior in priorActive.Where(prior => prior.EpaId == staged.EpaId))
            {
                prior.SupersedeBy(decision);
            }
        }

        // The agenda closes with the review: a staged line is Decided with its STAR (through the navigation, as the STAR has
        // no id until the save below), a line another sitting has decided is DecidedElsewhere (T235), an optional line
        // nothing was staged on is NotDecided, a deferred one stays so.
        review.CloseAgenda(issued.ToDictionary(pair => pair.Staged.EpaId, pair => pair.Decision), decidedElsewhere);

        // Refused whole when the review or a staged row changed after it was read: the review's xmin token, and a staged
        // row that is no longer there to delete. So a decision staged, edited or removed meanwhile, or a second ratify,
        // never leaves a staged row on a ratified review or issues a STAR from a version the chair has since changed.
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201).
            throw new InvalidOperationException(ReviewChanged, exception);
        }

        return review.ToDetailDto();
    }

    /// <summary>The refusal when the review changed between being read and the save. Nothing is written.</summary>
    public const string ReviewChanged =
        "This review changed while it was being ratified: " + CommitteeReviewChanged.WhatChanges + ". Nothing was " +
        "ratified. Reload the review; if it is still decided, ratify it again.";

    /// <summary>
    /// The refusal for two staged decisions on one EPA. Staging never stores them (the table is unique on review and EPA),
    /// and once the decision is recorded the staged decisions are fixed with it (D46), so the chair can remove neither: an
    /// administrator must look into it. Before the T213 review it told the chair to remove one.
    /// </summary>
    public const string TwoStagedOnOneEpa =
        "This review cannot be ratified: two entrustment decisions are staged on one EPA. Staging never stores two, and " +
        "the staged decisions are fixed once the committee's decision is recorded, so ask an administrator to look into it.";
}
