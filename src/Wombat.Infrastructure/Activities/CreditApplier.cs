using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Activities;

public sealed class CreditApplier : ICreditApplier
{
    private readonly IApplicationDbContext _dbContext;

    public CreditApplier(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreditApplicationResult> ApplyAsync(
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
            return CreditApplicationResult.Empty;
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
        // Since T119 that date is a real column: the clinician's own encounter date, resolved by
        // ActivityService from the field the PINNED schema's observation_date_field names and stamped on
        // every write. It used to be CreatedOn — the audit clock — behind a fallback chain that lived
        // here. The chain is gone because ObservedOn is never null and the fallback now happens once, at
        // the stamp, where ObservedOnSource records that it happened. There must be exactly one
        // implementation of "what date did this happen", and it is not this one.
        var observedOn = completedActivity.ObservedOn;
        var trainee = await ResolveTraineeAsync(completedActivity.SubjectUserId, observedOn, cancellationToken);
        if (trainee is null)
        {
            return CreditApplicationResult.Empty;
        }

        var updatedRows = new List<CurriculumItemProgress>();
        var scaleMismatchCount = 0;
        var unverifiedLevelCount = 0;

        // The ladder an achieved ordinal sits on is declared by the `scale_key` of the schema field the
        // directive names, and the schema here is the one the activity is PINNED to — so this answer is
        // fixed for the life of the activity and replays identically under RebuildCurriculumProgress (T109).
        var achievedScaleIds = await ResolveAchievedScaleIdsAsync(activityType.SchemaJson, rules, cancellationToken);

        foreach (var directive in rules.CountsFor)
        {
            var curriculumItems = await ResolveCurriculumItemsAsync(directive.CurriculumItemMatchRule, document.RootElement, trainee, cancellationToken);
            int? achievedScaleId = null;
            if (!string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
                achievedScaleIds.TryGetValue(directive.MinimumLevelField, out var resolvedScaleId))
            {
                achievedScaleId = resolvedScaleId;
            }

            foreach (var curriculumItem in curriculumItems)
            {
                // A completed activity that matches a curriculum item always counts toward volume
                // (CountsSoFar). The entrustment level is a separate progression signal: a completion
                // below the curriculum item's required level still counts as evidence, but only
                // contributes to MinimumLevelReachedCount when the level is actually met. (T071)
                //
                // Since T109 that gate can also be REFUSED outright: when the assessment's ladder and the
                // item's are both known and different, the ordinals mean different things and comparing
                // them is worse than not counting. Volume still counts — the encounter did happen.
                var comparison = CompareMinimumLevel(
                    curriculumItem, directive, document.RootElement, trainee.Stage, achievedScaleId);
                var minimumLevelReached = comparison.MinimumMet;

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

                // The progress counters are amount-weighted, so they stay comparable with CountsSoFar and
                // MinimumLevelReachedCount beside them. The per-call counters count curriculum ITEMS, so
                // they stay comparable with CreditedItemCount, which is a row count — the transition stamp
                // must not be able to exceed the number of items the same transition credited.
                switch (comparison.Basis)
                {
                    case LevelComparisonBasis.ScaleMismatch:
                        progress.ScaleMismatchCount += directive.Amount;
                        scaleMismatchCount++;
                        break;

                    case LevelComparisonBasis.Unpinned:
                        progress.UnverifiedLevelCount += directive.Amount;
                        unverifiedLevelCount++;
                        break;

                    case LevelComparisonBasis.SameScale:
                        // The only case where the stored tally rests on a verified ladder. Record which one,
                        // so a later re-pin of the curriculum item makes this row detectably stale rather
                        // than quietly wrong.
                        progress.MinimumLevelScaleId = curriculumItem.ScaleId;
                        break;
                }

                progress.LastActivityId = completedActivity.Id;
                progress.LastUpdated = DateTime.UtcNow;
                progress.CreditedActivityKeysJson = JsonSerializer.Serialize(creditedKeys.OrderBy(value => value));

                updatedRows.Add(progress);
            }
        }

        return new CreditApplicationResult(updatedRows, scaleMismatchCount, unverifiedLevelCount);
    }

    /// <summary>
    /// Maps each level-gated directive's field key to the entrustment scale that field's values sit on,
    /// as declared by its <c>scale_key</c> in the pinned schema (T109).
    /// </summary>
    /// <remarks>
    /// Every failure to resolve is silent and returns nothing for that key, which lands the comparison in
    /// <see cref="LevelComparisonBasis.Unpinned" /> — the pre-T109 behaviour. That covers an empty schema
    /// (the synthetic <c>ActivityType</c> the older call sites passed), a schema that no longer parses, a
    /// field with no <c>scale_key</c>, and a <c>scale_key</c> naming no scale in the database. The last of
    /// those is not hypothetical: the four generic WBA seeds declare <c>or_scale</c> while the seeded scale
    /// is named <c>O-R Scale</c>, so they resolve to nothing and must go on comparing exactly as they do now.
    /// </remarks>
    private async Task<IReadOnlyDictionary<string, int>> ResolveAchievedScaleIdsAsync(
        string? schemaJson,
        CreditRules rules,
        CancellationToken cancellationToken)
    {
        var gatedFieldKeys = rules.CountsFor
            .Select(directive => directive.MinimumLevelField)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key!)
            .ToHashSet(StringComparer.Ordinal);

        if (gatedFieldKeys.Count == 0 || string.IsNullOrWhiteSpace(schemaJson))
        {
            return EmptyScaleIds;
        }

        FormSchema schema;
        try
        {
            schema = FormSchemaParser.Parse(schemaJson);
        }
        catch (SchemaParseException)
        {
            return EmptyScaleIds;
        }

        var scaleKeysByField = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => gatedFieldKeys.Contains(field.Key) && !string.IsNullOrWhiteSpace(field.ScaleKey))
            .GroupBy(field => field.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().ScaleKey!, StringComparer.Ordinal);

        if (scaleKeysByField.Count == 0)
        {
            return EmptyScaleIds;
        }

        var resolved = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (fieldKey, scaleKey) in scaleKeysByField)
        {
            var scaleId = await ResolveScaleIdAsync(scaleKey, cancellationToken);
            if (scaleId.HasValue)
            {
                resolved[fieldKey] = scaleId.Value;
            }
        }

        return resolved;
    }

    /// <summary>
    /// Resolves a schema <c>scale_key</c> to a scale id by numeric id or exact name — the same match
    /// <c>ActivityReferenceDataService</c> uses to render the rung options, so the engine and the picker
    /// cannot disagree about which ladder a field is on.
    /// </summary>
    private async Task<int?> ResolveScaleIdAsync(string scaleKey, CancellationToken cancellationToken)
    {
        // Deliberately character-for-character the query in
        // ActivityReferenceDataService.GetEntrustmentScaleLevelOptionsAsync, including the trim, the
        // discarded TryParse result (a non-numeric key leaves scaleId at 0, which matches no row) and the
        // single OR. Two subtly different resolutions would mean the rung the assessor picked and the
        // ladder the engine scored it on could come from different scales.
        var key = scaleKey.Trim();
        _ = int.TryParse(key, out var scaleId);

        return await _dbContext.Set<EntrustmentScale>()
            .AsNoTracking()
            .Where(scale => scale.Id == scaleId || scale.Name == key)
            .Select(scale => (int?)scale.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static readonly IReadOnlyDictionary<string, int> EmptyScaleIds =
        new Dictionary<string, int>(StringComparer.Ordinal);

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

    private static LevelComparison CompareMinimumLevel(
        CurriculumItem curriculumItem,
        CreditDirective directive,
        JsonElement data,
        int? traineeStage,
        int? achievedScaleId)
    {
        if (string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
            string.IsNullOrWhiteSpace(directive.MinimumLevelFixed))
        {
            return EntrustmentLevelComparer.NotGated();
        }

        // Gate on the level required for the trainee's current stage, not the flat target level, so
        // the credit engine agrees with the stage-aware minimum the dashboard/progress page displays.
        var requiredLevel = curriculumItem.GetMinimumLevelForStage(traineeStage);

        if (!string.IsNullOrWhiteSpace(directive.MinimumLevelField) &&
            TryGetInt32(data, directive.MinimumLevelField, out var providedLevel))
        {
            return EntrustmentLevelComparer.Compare(
                providedLevel, achievedScaleId, requiredLevel, curriculumItem.ScaleId);
        }

        if (!string.IsNullOrWhiteSpace(directive.MinimumLevelFixed) &&
            int.TryParse(directive.MinimumLevelFixed, CultureInfo.InvariantCulture, out var fixedLevel))
        {
            // A literal in the credit rules names no schema field and therefore has no scale of its own.
            // It stays unpinned by construction, which means it compares exactly as it always has.
            return EntrustmentLevelComparer.Compare(
                fixedLevel, providedScaleId: null, requiredLevel, curriculumItem.ScaleId);
        }

        return new LevelComparison(false, LevelComparisonBasis.ValueMissing);
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
