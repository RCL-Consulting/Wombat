using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record CloseFormativeReviewCommand(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class CloseFormativeReviewCommandValidator : AbstractValidator<CloseFormativeReviewCommand>
{
    public CloseFormativeReviewCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class CloseFormativeReviewCommandHandler : IRequestHandler<CloseFormativeReviewCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public CloseFormativeReviewCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeReviewDetailDto> Handle(CloseFormativeReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // Authorise first: an unknown review and one the caller does not chair get the one refusal, before the review's
        // formative flag or state is said (T194 item 1). The chair alone, with no Administrator bypass (T165, D46), and only
        // while they may sit at the review (T256).
        (review, _) = await CommitteeDecisionAuthorization.DemandChairedReviewAsync(
            _dbContext, request.Principal, review, _users, cancellationToken);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);
        review.Close(CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal), DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
