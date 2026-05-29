using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// Full curriculum-credit view for a trainee's own portfolio progress page. Lists every
/// curriculum item in the trainee's curriculum — including items with no credit yet (0 of N) —
/// so the trainee can see where credit has and has not accrued. The trainee dashboard shows a
/// summarised version of the same data via <see cref="GetTraineeDashboardSummaryQuery"/>.
/// </summary>
public sealed record GetCurriculumProgressForTraineeQuery(string TraineeUserId)
    : IRequest<IReadOnlyList<TraineeCurriculumProgressDto>>;

public sealed class GetCurriculumProgressForTraineeQueryValidator
    : AbstractValidator<GetCurriculumProgressForTraineeQuery>
{
    public GetCurriculumProgressForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
    }
}

public sealed record TraineeCurriculumProgressDto(
    int CurriculumItemId,
    string EpaCode,
    string EpaTitle,
    int CompletedCount,
    int RequiredCount,
    bool IsComplete,
    int EffectiveMinimumLevelOrder,
    int MinimumLevelReachedCount,
    int? TraineeStage,
    DateTime? LastUpdated);

public sealed class GetCurriculumProgressForTraineeQueryHandler
    : IRequestHandler<GetCurriculumProgressForTraineeQuery, IReadOnlyList<TraineeCurriculumProgressDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurriculumProgressForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TraineeCurriculumProgressDto>> Handle(
        GetCurriculumProgressForTraineeQuery request, CancellationToken cancellationToken)
    {
        var userId = request.TraineeUserId.Trim();

        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, cancellationToken);

        if (profile is null)
        {
            return Array.Empty<TraineeCurriculumProgressDto>();
        }

        var stage = GetTraineeDashboardSummaryQueryHandler.ComputeTraineeStage(
            profile.ProgrammeStartDate, DateOnly.FromDateTime(DateTime.UtcNow));

        var items = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == profile.CurriculumId)
            .OrderBy(item => item.Epa.Code)
            .Select(item => new
            {
                item.Id,
                EpaCode = item.Epa.Code,
                EpaTitle = item.Epa.Title,
                item.RequiredCount,
                item.MinimumLevelOrder,
                item.MinimumLevelByStageJson
            })
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return Array.Empty<TraineeCurriculumProgressDto>();
        }

        var progressByItem = await _dbContext.Set<CurriculumItemProgress>()
            .AsNoTracking()
            .Where(p => p.TraineeUserId == userId &&
                        p.CurriculumItem.CurriculumId == profile.CurriculumId)
            .Select(p => new
            {
                p.CurriculumItemId,
                p.CountsSoFar,
                p.MinimumLevelReachedCount,
                p.LastUpdated
            })
            .ToDictionaryAsync(p => p.CurriculumItemId, cancellationToken);

        var result = new List<TraineeCurriculumProgressDto>(items.Count);
        foreach (var item in items)
        {
            progressByItem.TryGetValue(item.Id, out var progress);

            var levelTemplate = new CurriculumItem
            {
                MinimumLevelOrder = item.MinimumLevelOrder,
                MinimumLevelByStageJson = item.MinimumLevelByStageJson
            };

            var completed = progress?.CountsSoFar ?? 0;
            result.Add(new TraineeCurriculumProgressDto(
                item.Id,
                item.EpaCode,
                item.EpaTitle,
                completed,
                item.RequiredCount,
                completed >= item.RequiredCount,
                levelTemplate.GetMinimumLevelForStage(stage),
                progress?.MinimumLevelReachedCount ?? 0,
                stage,
                progress?.LastUpdated));
        }

        return result;
    }
}
