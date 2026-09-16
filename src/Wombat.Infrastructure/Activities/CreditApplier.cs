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
        // institution adopted, plus that institution's local extras. Without a trainee profile there is
        // nothing to credit against. (T091 phase 4.)
        //
        // The stage is resolved from WHEN THE ACTIVITY HAPPENED, not from today. This matters because
        // the stage selects the curriculum item's effective minimum level (T073): an encounter observed
        // in year 1 must be judged against the year-1 minimum for ever, not against whatever year the
        // trainee has since reached. Getting this wrong made RebuildCurriculumProgress re-score a
        // trainee's whole history against their current year and silently change what
        // MinimumLevelReachedCount meant.
        //
        // CreatedOn is the best encounter date the model currently holds — the WBA schemas carry no
        // observation date of their own (see T098 gap 2). For a live submission it is effectively
        // today, so normal credit is unchanged; only replayed history is corrected.
        var observedOn = DateOnly.FromDateTime(ResolveObservationDate(completedActivity));
        var trainee = await ResolveTraineeAsync(completedActivity.SubjectUserId, observedOn, cancellationToken);
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

    /// <summary>
    /// Best available date for when the activity actually happened, used to resolve the trainee's
    /// programme stage.
    /// </summary>
    /// <remarks>
    /// Preference order: the activity's own <c>CreatedOn</c> (set when the WBA is logged, so closest
    /// to the encounter), then the earliest recorded transition, then now. The fallbacks matter
    /// because an activity constructed without <c>CreatedOn</c> would otherwise date to year 0001,
    /// land before every programme start date, and resolve to a null stage — quietly falling back
    /// to the flat minimum instead of the stage minimum.
    /// </remarks>
    private static DateTime ResolveObservationDate(Activity activity)
    {
        if (activity.CreatedOn != default)
        {
            return activity.CreatedOn;
        }

        var earliestTransition = activity.Transitions
            .Where(transition => transition.OccurredOn != default)
            .Select(transition => transition.OccurredOn)
            .DefaultIfEmpty(default)
            .Min();

        return earliestTransition != default ? earliestTransition : DateTime.UtcNow;
    }

    private async Task<TraineeContext?> ResolveTraineeAsync(
        string traineeUserId,
        DateOnly observedOn,
        CancellationToken cancellationToken)
    {
        // Deliberately NOT filtered on IsActive. TraineeProfile.Complete() clears IsActive on
        // graduation, so filtering here meant a graduated trainee earned no credit — harmless for
        // live submissions (they no longer submit), but destructive under
        // RebuildCurriculumProgress, which deletes every progress row before replaying: alumni
        // came back with nothing and could not be restored by re-running the rebuild.
        // Prefer an active profile when a user somehow has more than one, then the most recent.
        var profile = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(p => p.UserId == traineeUserId)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.ProgrammeStartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        return new TraineeContext(
            profile.CurriculumId,
            profile.InstitutionId,
            profile.GetStage(observedOn));
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
