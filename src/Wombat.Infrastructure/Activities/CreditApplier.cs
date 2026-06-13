using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Activities;

public sealed class CreditApplier : ICreditApplier
{
    private readonly IApplicationDbContext _dbContext;

    public CreditApplier(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CurriculumItemProgress>> ApplyAsync(
        Activity completedActivity,
        ActivityType activityType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(completedActivity);
        ArgumentNullException.ThrowIfNull(activityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(activityType.CreditRulesJson);

        var rules = CreditRulesParser.Parse(activityType.CreditRulesJson);
        if (rules.CountsFor.Count == 0)
        {
            return [];
        }

        using var document = JsonDocument.Parse(completedActivity.DataJson);
        var creditKey = GetCreditKey(completedActivity);

        // Credit only accrues against the trainee's own portfolio: the national curriculum version their
        // institution adopted, plus that institution's local extras. Without an active trainee profile
        // there is nothing to credit against. (T091 phase 4.)
        var trainee = await ResolveTraineeAsync(completedActivity.SubjectUserId, cancellationToken);
        if (trainee is null)
        {
            return [];
        }

        var updatedRows = new List<CurriculumItemProgress>();

        foreach (var directive in rules.CountsFor)
        {
            var curriculumItems = await ResolveCurriculumItemsAsync(directive.CurriculumItemMatchRule, document.RootElement, trainee, cancellationToken);
            foreach (var curriculumItem in curriculumItems)
            {
                // A completed activity that matches a curriculum item always counts toward volume
                // (CountsSoFar). The entrustment level is a separate progression signal: a completion
                // below the curriculum item's required level still counts as evidence, but only
                // contributes to MinimumLevelReachedCount when the level is actually met. (T071)
                var minimumLevelReached = MeetsMinimumLevel(curriculumItem, directive, document.RootElement, trainee.Stage);

                var progressSet = _dbContext.Set<CurriculumItemProgress>();
                var progress = progressSet.Local.SingleOrDefault(
                    entity => entity.CurriculumItemId == curriculumItem.Id && entity.TraineeUserId == completedActivity.SubjectUserId)
                    ?? await progressSet.SingleOrDefaultAsync(
                        entity => entity.CurriculumItemId == curriculumItem.Id && entity.TraineeUserId == completedActivity.SubjectUserId,
                        cancellationToken);

                if (progress is null)
                {
                    progress = new CurriculumItemProgress
                    {
                        CurriculumItemId = curriculumItem.Id,
                        TraineeUserId = completedActivity.SubjectUserId,
                        LastUpdated = DateTime.UtcNow
                    };

                    progressSet.Add(progress);
                }

                var creditedKeys = DeserializeCreditedKeys(progress.CreditedActivityKeysJson);
                if (!creditedKeys.Add(creditKey))
                {
                    continue;
                }

                progress.CountsSoFar += directive.Amount;
                if (minimumLevelReached)
                {
                    progress.MinimumLevelReachedCount += directive.Amount;
                }

                progress.LastActivityId = completedActivity.Id;
                progress.LastUpdated = DateTime.UtcNow;
                progress.CreditedActivityKeysJson = JsonSerializer.Serialize(creditedKeys.OrderBy(value => value));

                updatedRows.Add(progress);
            }
        }

        return updatedRows;
    }

    private async Task<IReadOnlyList<CurriculumItem>> ResolveCurriculumItemsAsync(
        CurriculumItemMatchRule matchRule,
        JsonElement data,
        TraineeContext trainee,
        CancellationToken cancellationToken)
    {
        // Every match is confined to the trainee's adopted curriculum version (national core) plus their
        // own institution's local extras. This prevents credit leaking across curriculum versions or
        // onto another institution's local items that happen to share an EPA. (T091 phase 4.)
        var scoped = _dbContext.Set<CurriculumItem>()
            .Where(entity => entity.CurriculumId == trainee.CurriculumId
                && (entity.OwningInstitutionId == null || entity.OwningInstitutionId == trainee.InstitutionId));

        if (matchRule.CurriculumItemId.HasValue)
        {
            return await scoped
                .Where(entity => entity.Id == matchRule.CurriculumItemId.Value)
                .ToListAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(matchRule.CurriculumItemField) &&
            TryGetInt32(data, matchRule.CurriculumItemField, out var curriculumItemId))
        {
            return await scoped
                .Where(entity => entity.Id == curriculumItemId)
                .ToListAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(matchRule.EpaField) &&
            TryGetInt32(data, matchRule.EpaField, out var epaId))
        {
            return await scoped
                .Where(entity => entity.EpaId == epaId)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    private static bool MeetsMinimumLevel(
        CurriculumItem curriculumItem,
        CreditDirective directive,
        JsonElement data,
        int? traineeStage)
    {
        if (string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
            string.IsNullOrWhiteSpace(directive.MinimumLevelFixed))
        {
            return true;
        }

        // Gate on the level required for the trainee's current stage, not the flat target level, so
        // the credit engine agrees with the stage-aware minimum the dashboard/progress page displays.
        var requiredLevel = curriculumItem.GetMinimumLevelForStage(traineeStage);

        if (!string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
            TryGetInt32(data, directive.MinimumLevelField, out var providedLevel))
        {
            return providedLevel >= requiredLevel;
        }

        if (!string.IsNullOrWhiteSpace(directive.MinimumLevelFixed) &&
            int.TryParse(directive.MinimumLevelFixed, CultureInfo.InvariantCulture, out var fixedLevel))
        {
            return fixedLevel >= requiredLevel;
        }

        return false;
    }

    private async Task<TraineeContext?> ResolveTraineeAsync(string traineeUserId, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == traineeUserId && p.IsActive, cancellationToken);

        if (profile is null)
        {
            return null;
        }

        return new TraineeContext(
            profile.CurriculumId,
            profile.InstitutionId,
            profile.GetStage(DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    private sealed record TraineeContext(int CurriculumId, int InstitutionId, int? Stage);

    private static HashSet<string> DeserializeCreditedKeys(string json)
    {
        try
        {
            var keys = JsonSerializer.Deserialize<string[]>(json) ?? [];
            return keys.ToHashSet(StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string GetCreditKey(Activity activity)
    {
        var transition = activity.Transitions
            .OrderByDescending(entity => entity.OccurredOn)
            .FirstOrDefault();

        return transition is null
            ? activity.Id.ToString(CultureInfo.InvariantCulture)
            : $"{activity.Id}:{transition.TransitionKey}";
    }

    private static bool TryGetInt32(JsonElement root, string fieldKey, out int value)
    {
        if (root.TryGetProperty(fieldKey, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
            {
                return true;
            }

            if (property.ValueKind == JsonValueKind.String &&
                int.TryParse(property.GetString(), CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }
}
