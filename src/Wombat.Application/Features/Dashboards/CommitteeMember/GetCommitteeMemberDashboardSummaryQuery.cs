using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Dashboards.CommitteeMember;

public sealed record GetCommitteeMemberDashboardSummaryQuery(ClaimsPrincipal Principal)
    : IRequest<CommitteeMemberDashboardSummaryDto>;

public sealed class GetCommitteeMemberDashboardSummaryQueryHandler
    : IRequestHandler<GetCommitteeMemberDashboardSummaryQuery, CommitteeMemberDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly DashboardThresholds _thresholds;

    public GetCommitteeMemberDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IOptions<DashboardThresholds> thresholds)
    {
        _dbContext = dbContext;
        _thresholds = thresholds.Value;
    }

    public async Task<CommitteeMemberDashboardSummaryDto> Handle(
        GetCommitteeMemberDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var subSpecialityIds = request.Principal.GetSubSpecialityIds();

        var traineeProfiles = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(p => p.IsActive && subSpecialityIds.Contains(p.Curriculum.SubSpecialityId))
            .ToListAsync(cancellationToken);

        var traineeUserIds = traineeProfiles.Select(p => p.UserId).ToList();
        var curriculumIds = traineeProfiles.Select(p => p.CurriculumId).Distinct().ToList();

        var items = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Include(ci => ci.Epa)
            .Where(ci => curriculumIds.Contains(ci.CurriculumId))
            .ToListAsync(cancellationToken);

        var progress = await _dbContext.Set<CurriculumItemProgress>()
            .AsNoTracking()
            .Where(p => traineeUserIds.Contains(p.TraineeUserId))
            .ToListAsync(cancellationToken);

        // Calculate per-trainee overall completion percentage
        var traineesNearCompletion = new List<TraineeNearCompletionItem>();
        foreach (var profile in traineeProfiles)
        {
            var profileItems = items.Where(i => i.CurriculumId == profile.CurriculumId).ToList();
            if (profileItems.Count == 0) continue;

            var totalPercent = 0.0;
            foreach (var item in profileItems)
            {
                var p = progress.FirstOrDefault(
                    pr => pr.CurriculumItemId == item.Id && pr.TraineeUserId == profile.UserId);
                if (p is not null && item.RequiredCount > 0)
                {
                    totalPercent += Math.Min(100.0, (double)p.CountsSoFar / item.RequiredCount * 100);
                }
            }

            var overallPercent = Math.Round(totalPercent / profileItems.Count, 1);
            if (overallPercent >= _thresholds.CommitteeCompletionPercent)
            {
                traineesNearCompletion.Add(new TraineeNearCompletionItem(
                    profile.UserId, profile.UserId, overallPercent));
            }
        }

        // Programme progress (per-EPA average across all active trainees)
        var programmeProgress = new List<ProgrammeProgressItem>();
        if (traineeUserIds.Count > 0)
        {
            var progressLookup = progress
                .GroupBy(p => p.CurriculumItemId)
                .ToDictionary(g => g.Key, g => g.ToList());

            programmeProgress = items
                .GroupBy(ci => ci.Epa.Title)
                .Select(g =>
                {
                    var total = 0.0;
                    foreach (var item in g)
                    {
                        if (!progressLookup.TryGetValue(item.Id, out var pList)) continue;
                        foreach (var p in pList)
                        {
                            total += item.RequiredCount > 0
                                ? Math.Min(100.0, (double)p.CountsSoFar / item.RequiredCount * 100)
                                : 100.0;
                        }
                    }

                    return new ProgrammeProgressItem(
                        g.Key, Math.Round(total / traineeUserIds.Count, 1));
                })
                .OrderBy(p => p.EpaTitle)
                .ToList();
        }

        return new CommitteeMemberDashboardSummaryDto(
            traineesNearCompletion.OrderByDescending(t => t.OverallCompletionPercent).ToList(),
            programmeProgress);
    }
}
