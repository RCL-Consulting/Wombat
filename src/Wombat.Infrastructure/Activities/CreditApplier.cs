using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Curricula;

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

        var rules = CreditRulesParser.Parse(activityType.CreditRulesJson);
        if (rules.CountsFor.Count == 0)
        {
            return [];
        }

        using var document = JsonDocument.Parse(completedActivity.DataJson);
        var creditKey = GetCreditKey(completedActivity);
        var updatedRows = new List<CurriculumItemProgress>();

        foreach (var directive in rules.CountsFor)
        {
            var curriculumItems = await ResolveCurriculumItemsAsync(directive.CurriculumItemMatchRule, document.RootElement, cancellationToken);
            foreach (var curriculumItem in curriculumItems)
            {
                var minimumLevelReached = MeetsMinimumLevel(curriculumItem, directive, document.RootElement);
                if (!minimumLevelReached && !string.IsNullOrWhiteSpace(directive.MinimumLevelField))
                {
                    continue;
                }

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
        CancellationToken cancellationToken)
    {
        if (matchRule.CurriculumItemId.HasValue)
        {
            return await _dbContext.Set<CurriculumItem>()
                .Where(entity => entity.Id == matchRule.CurriculumItemId.Value)
                .ToListAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(matchRule.CurriculumItemField) &&
            TryGetInt32(data, matchRule.CurriculumItemField, out var curriculumItemId))
        {
            return await _dbContext.Set<CurriculumItem>()
                .Where(entity => entity.Id == curriculumItemId)
                .ToListAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(matchRule.EpaField) &&
            TryGetInt32(data, matchRule.EpaField, out var epaId))
        {
            return await _dbContext.Set<CurriculumItem>()
                .Where(entity => entity.EpaId == epaId)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    private static bool MeetsMinimumLevel(
        CurriculumItem curriculumItem,
        CreditDirective directive,
        JsonElement data)
    {
        if (string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
            string.IsNullOrWhiteSpace(directive.MinimumLevelFixed))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
            TryGetInt32(data, directive.MinimumLevelField, out var providedLevel))
        {
            return providedLevel >= curriculumItem.MinimumLevelOrder;
        }

        if (!string.IsNullOrWhiteSpace(directive.MinimumLevelFixed) &&
            int.TryParse(directive.MinimumLevelFixed, CultureInfo.InvariantCulture, out var fixedLevel))
        {
            return fixedLevel >= curriculumItem.MinimumLevelOrder;
        }

        return false;
    }

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
