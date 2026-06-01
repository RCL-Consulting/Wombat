using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Resolves the programme default entrustment scale for a committee review's trainee, via
/// review → trainee profile → curriculum → sub-speciality → DefaultEntrustmentScaleId. Returns null
/// when the trainee has no profile or the sub-speciality has no default scale, in which case the
/// committee STAR picker falls back to offering every scale. (T076 / F-4D-1)
/// </summary>
public sealed record GetProgramScaleIdForReviewQuery(int ReviewId) : IRequest<int?>;

public sealed class GetProgramScaleIdForReviewQueryHandler : IRequestHandler<GetProgramScaleIdForReviewQuery, int?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProgramScaleIdForReviewQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int?> Handle(GetProgramScaleIdForReviewQuery request, CancellationToken cancellationToken)
    {
        var traineeUserId = await _dbContext.Set<CommitteeReview>()
            .Where(review => review.Id == request.ReviewId)
            .Select(review => review.TraineeUserId)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(traineeUserId))
        {
            return null;
        }

        return await _dbContext.Set<TraineeProfile>()
            .Where(profile => profile.UserId == traineeUserId)
            .Select(profile => profile.Curriculum.SubSpeciality.DefaultEntrustmentScaleId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
