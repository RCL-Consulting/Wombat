using System.Data.Common;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Stages, or edits, the entrustment decision a review's chair is minded to issue on one EPA, naming the lines of the
/// review's frozen evidence snapshot it rests on. Ratifying the review issues it as a STAR.
/// </summary>
/// <remarks>
/// <para>
/// <c>Rationale</c> is redacted from the audit summary: the AuditPipelineBehavior would otherwise write the committee's
/// written justification about a named trainee into SummaryJson. Staging is edited repeatedly before the decision is
/// issued, so leaving it unmarked also preserved every superseded draft of that reasoning, including wording the
/// committee chose to withdraw. The ids, level and dates stay in the clear. (T101)
/// </para>
/// <para>
/// <c>EvidenceItemIds</c> are ids of this review's <see cref="CommitteeEvidence" /> lines, at least one (D38, T131).
/// The staged row keeps the ids and nothing else; ratifying builds each STAR's evidence links from the rows themselves.
/// Before T131 the command took free-text links, and the page sent none.
/// </para>
/// </remarks>
public sealed record StagePendingEntrustmentDecisionCommand(
    int ReviewId,
    int? PendingId,
    int EpaId,
    int AuthorisedLevelId,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    [property: Redact] string Rationale,
    IReadOnlyList<int> EvidenceItemIds,
    ClaimsPrincipal Principal) : IRequest<PendingEntrustmentDecisionDto>;

public sealed class StagePendingEntrustmentDecisionCommandValidator : AbstractValidator<StagePendingEntrustmentDecisionCommand>
{
    public StagePendingEntrustmentDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.EpaId).GreaterThan(0);
        RuleFor(command => command.AuthorisedLevelId).GreaterThan(0);
        RuleFor(command => command.Rationale).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command)
            .Must(command => !command.ExpiresOn.HasValue || command.ExpiresOn.Value > command.IssuedOn)
            .WithMessage("An expiry date must be after the issue date.");

        // D38 (T131): a decision names the evidence it rests on.
        RuleFor(command => command.EvidenceItemIds)
            .NotEmpty()
            .WithMessage(StagedEvidence.NoneNamed);
        RuleFor(command => command.EvidenceItemIds)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("Name each item of the evidence snapshot once.");
        RuleForEach(command => command.EvidenceItemIds).GreaterThan(0);
    }
}

public sealed class StagePendingEntrustmentDecisionCommandHandler
    : IRequestHandler<StagePendingEntrustmentDecisionCommand, PendingEntrustmentDecisionDto>
{
    /// <summary>
    /// The refusal when the review changed between being read and the save. Nothing is written. A committee decision
    /// recorded meanwhile is caught here too, so the staged set stays fixed from the moment it is recorded (T165).
    /// </summary>
    public const string ReviewChanged =
        "This review changed while the decision was being staged: a decision was staged, edited or removed, or the " +
        "committee's decision was recorded or ratified. Nothing was staged. Reload the review and stage the decision again.";

    /// <summary>PostgreSQL's unique_violation, which the (review, EPA) index raises.</summary>
    private const string UniqueViolation = "23505";

    private readonly IApplicationDbContext _dbContext;

    public StagePendingEntrustmentDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PendingEntrustmentDecisionDto> Handle(StagePendingEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        // Every check below runs before the first mutation, in this order: the audit pipeline saves the request's
        // context from its catch, so a refusal after a mutation would commit it.
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(r => r.Panel)
                .ThenInclude(p => p.Members)
            .Include(r => r.EvidenceItems)
            .SingleOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        // 1-2. Authorise first: an unknown review and one the caller does not chair get the one refusal, before anything
        // about the review, its state included, is said (T194 item 1).
        review = CommitteeDecisionAuthorization.DemandChairedReview(request.Principal, review);

        // 3. The trainee still trains at the panel's institution (T182).
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);
        var actorUserId = EntrustmentDecisionAuthorization.GetRequiredUserId(request.Principal);

        if (review.IsFormative)
        {
            throw new InvalidOperationException("Formative reviews cannot issue entrustment decisions.");
        }

        // T165: the STARs staged at a review are part of the decision its panel records, and are fixed with it. Before
        // T165 they could be staged or changed on a decided review, so the chair alone, after the sitting, could add or
        // raise a STAR that ratifying then issued as the committee's.
        if (review.State is not CommitteeReviewState.InProgress)
        {
            throw new InvalidOperationException(StagedStars.FixedWhenDecided);
        }

        // A staged decision keeps the EPA it was staged on (PendingEntrustmentDecision.Update takes none), so an edit is
        // judged against THAT EPA's curriculum item and ladder, and a request naming another EPA is refused rather than
        // silently ignored. Loaded tracked, but only read until every check below has passed.
        PendingEntrustmentDecision? existing = null;
        if (request.PendingId.HasValue)
        {
            existing = await _dbContext.Set<PendingEntrustmentDecision>()
                .SingleOrDefaultAsync(p => p.Id == request.PendingId.Value && p.ReviewId == review.Id, cancellationToken)
                ?? throw new InvalidOperationException("The pending entrustment decision could not be found for this review.");

            if (existing.EpaId != request.EpaId)
            {
                throw new InvalidOperationException(
                    "A staged decision's EPA cannot be changed. Remove it and stage a decision on the other EPA.");
            }
        }

        var epa = await _dbContext.Set<Epa>().AsNoTracking().SingleOrDefaultAsync(e => e.Id == request.EpaId, cancellationToken)
            ?? throw new InvalidOperationException("The specified EPA could not be found.");
        var level = await _dbContext.Set<EntrustmentLevel>().AsNoTracking().SingleOrDefaultAsync(l => l.Id == request.AuthorisedLevelId, cancellationToken)
            ?? throw new InvalidOperationException("The specified entrustment level could not be found.");

        // 4. T167: the EPA must be on the trainee's curriculum, and the level a rung of that item's ladder: its pinned
        // scale, else the programme's default scale (the T076 rule, which this subsumes). The page's picker lists the
        // same predicate.
        await StarCurriculum.DemandAsync(_dbContext, review.TraineeUserId, epa, level, cancellationToken);

        // 6. D38 (T131): every named id is a line of THIS review's frozen snapshot, and none is a supervisor report. The
        // page's picker lists the same lines (CommitteeEvidenceDto.CanGroundADecision).
        StagedEvidence.Demand(review, request.EvidenceItemIds);

        // 7. One staged decision per EPA at a review; the table's unique index holds it when two chairs stage at once.
        var editedId = existing?.Id ?? 0;
        var alreadyStaged = await _dbContext.Set<PendingEntrustmentDecision>()
            .AnyAsync(p => p.ReviewId == review.Id && p.EpaId == request.EpaId && p.Id != editedId, cancellationToken);
        if (alreadyStaged)
        {
            throw new InvalidOperationException(AlreadyStaged(epa.Code));
        }

        PendingEntrustmentDecision pending;
        if (existing is not null)
        {
            pending = existing;
            pending.Update(request.AuthorisedLevelId, request.IssuedOn, request.ExpiresOn, request.Rationale, request.EvidenceItemIds);
        }
        else
        {
            pending = PendingEntrustmentDecision.Stage(
                review.Id,
                request.EpaId,
                request.AuthorisedLevelId,
                request.IssuedOn,
                request.ExpiresOn,
                request.Rationale,
                request.EvidenceItemIds,
                actorUserId,
                DateTime.UtcNow);
            _dbContext.Set<PendingEntrustmentDecision>().Add(pending);
        }

        // The review is marked modified, though nothing about it changes, so that this save is refused whole when the
        // review changed after it was read above: its xmin token (CommitteeReviewConfiguration) is then checked in the
        // same save. A ratify that committed in between would otherwise get this row on a ratified review, and a ratify
        // that read the review first would leave it behind; either way a staged row nothing can remove or issue. Another
        // stage, edit or ratify that reads the review before this commits is refused the same way.
        _dbContext.Set<CommitteeReview>().Entry(review).Property(r => r.State).IsModified = true;

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
        catch (DbUpdateException exception) when (exception.InnerException is DbException { SqlState: UniqueViolation })
        {
            // Check 7's race: another decision on this EPA was staged after the check read the table, and the unique
            // index on (review, EPA) refused this one.
            throw new InvalidOperationException(AlreadyStaged(epa.Code), exception);
        }

        var stored = await _dbContext.Set<PendingEntrustmentDecision>()
            .AsNoTracking()
            .Include(p => p.Epa)
            .Include(p => p.AuthorisedLevel)
            .SingleAsync(p => p.Id == pending.Id, cancellationToken);

        return stored.ToDto();
    }

    /// <summary>The refusal for a second staged decision on one EPA at a review.</summary>
    public static string AlreadyStaged(string epaCode)
        => $"An entrustment decision on {epaCode} is already staged at this review. Remove it to stage another.";
}
