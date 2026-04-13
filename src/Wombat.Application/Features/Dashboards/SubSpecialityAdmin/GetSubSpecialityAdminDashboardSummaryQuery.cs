using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Dashboards.SubSpecialityAdmin;

public sealed record GetSubSpecialityAdminDashboardSummaryQuery(ClaimsPrincipal Principal)
    : IRequest<SubSpecialityAdminDashboardSummaryDto>;

public sealed class GetSubSpecialityAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetSubSpecialityAdminDashboardSummaryQuery, SubSpecialityAdminDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSubSpecialityAdminDashboardSummaryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubSpecialityAdminDashboardSummaryDto> Handle(
        GetSubSpecialityAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var subSpecialityIds = request.Principal.GetSubSpecialityIds();

        var pendingReviewCount = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.CurrentState == "submitted" || a.CurrentState == "in_review")
            .CountAsync(cancellationToken);

        var traineeProfiles = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(p => subSpecialityIds.Contains(p.Curriculum.SubSpecialityId))
            .ToListAsync(cancellationToken);

        var activeCount = traineeProfiles.Count(p => p.IsActive);
        var inactiveCount = traineeProfiles.Count(p => !p.IsActive);

        var activeTraineeUserIds = traineeProfiles
            .Where(p => p.IsActive)
            .Select(p => p.UserId)
            .ToList();

        var curriculumIds = traineeProfiles
            .Where(p => p.IsActive)
            .Select(p => p.CurriculumId)
            .Distinct()
            .ToList();

        var coverage = await BuildCurriculumCoverage(
            curriculumIds, activeTraineeUserIds, cancellationToken);

        return new SubSpecialityAdminDashboardSummaryDto(
            pendingReviewCount, activeCount, inactiveCount, coverage);
    }

    private async Task<IReadOnlyList<EpaCoverageItem>> BuildCurriculumCoverage(
        List<int> curriculumIds,
        List<string> traineeUserIds,
        CancellationToken cancellationToken)
    {
        if (curriculumIds.Count == 0 || traineeUserIds.Count == 0)
            return [];

        var items = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Include(ci => ci.Epa)
            .Where(ci => curriculumIds.Contains(ci.CurriculumId))
            .ToListAsync(cancellationToken);

        var progress = await _dbContext.Set<CurriculumItemProgress>()
            .AsNoTracking()
            .Where(p => traineeUserIds.Contains(p.TraineeUserId))
            .ToListAsync(cancellationToken);

        var progressLookup = progress
            .GroupBy(p => p.CurriculumItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var totalTrainees = traineeUserIds.Count;
        return items
            .GroupBy(ci => ci.Epa.Title)
            .Select(g =>
            {
                var totalPercent = 0.0;
                foreach (var item in g)
                {
                    if (!progressLookup.TryGetValue(item.Id, out var progressList)) continue;
                    foreach (var p in progressList)
                    {
                        totalPercent += item.RequiredCount > 0
                            ? Math.Min(100.0, (double)p.CountsSoFar / item.RequiredCount * 100)
                            : 100.0;
                    }
                }

                return new EpaCoverageItem(g.Key, Math.Round(totalPercent / totalTrainees, 1));
            })
            .OrderBy(c => c.EpaTitle)
            .ToList();
    }
}
