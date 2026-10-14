using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Application.Audit;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <remarks>
/// <c>Reason</c> is redacted from the audit summary: it is the trainee's own grounds for appealing
/// a progression decision, written in the expectation that the appeal panel reads it — not every
/// administrator with access to the audit log. (T101)
/// </remarks>
public sealed record LodgeAppealCommand(
    int ReviewId,
    [property: Redact] string Reason,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>
{
    /// <summary>The longest reason the appeal keeps, in characters, as the lodge block's counter says (T366).</summary>
    public const int MaxReasonLength = 4000;

    /// <summary>A blank reason, refused (T366, round 3 board 2 b).</summary>
    public const string GiveReason = "Give your reason for appealing.";

    /// <summary>A reason over <see cref="MaxReasonLength" />, refused.</summary>
    public const string ReasonTooLong = "Give your reason in 4,000 characters or fewer.";
}

public sealed class LodgeAppealCommandValidator : AbstractValidator<LodgeAppealCommand>
{
    public LodgeAppealCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        // T366: Wombat's words, as the page says them, never the validator's default.
        RuleFor(command => command.Reason)
            .Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage(LodgeAppealCommand.GiveReason)
            .MaximumLength(LodgeAppealCommand.MaxReasonLength).WithMessage(LodgeAppealCommand.ReasonTooLong);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class LodgeAppealCommandHandler : IRequestHandler<LodgeAppealCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public LodgeAppealCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(LodgeAppealCommand request, CancellationToken cancellationToken)
    {
        // Only a trainee lodges an appeal, asked before the review is looked up (T165).
        CommitteeDecisionAuthorization.DemandLodgesAppeals(request.Principal);

        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // Then one refusal for an unknown review, another trainee's, and the caller's own before it is ratified, before
        // the review's state is said (T194 item 1).
        review = CommitteeDecisionAuthorization.DemandOwnRatifiedReview(request.Principal, review);
        try
        {
            review.LodgeAppeal(request.Reason, CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal), DateTime.UtcNow);
        }
        catch (InvalidOperationException refusal) when (refusal.Message is CommitteeReview.OnlyRatifiedAppealed or CommitteeReview.AppealAlreadyOpen)
        {
            // The review's own state rules, in Wombat's words ("Only ratified reviews can be appealed.", a second tab's
            // lodge, M3), which her page shows as they are; any other failure stays one, a fixed sentence there (T366, D3).
            // Raised before anything is staged (T201).
            throw new CommitteeRefusedException(refusal.Message, refusal);
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Two tabs read the review Ratified and both lodged: the review's xmin refuses the second save. The first tab's
            // appeal exists, so this one is told the review's rule, and her page reads the review again (T366, R6). Carried
            // as the inner exception, so the audit pipeline still sees a refused save (T201).
            throw new CommitteeRefusedException(CommitteeReview.OnlyRatifiedAppealed, exception);
        }

        return review.ToDetailDto();
    }
}
