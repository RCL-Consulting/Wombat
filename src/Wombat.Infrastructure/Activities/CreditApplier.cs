using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
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

        var plan = await PlanAsync(CreditSubject.Of(completedActivity), activityType, cancellationToken);
        return Apply(plan, completedActivity);
    }

    public async Task<CreditPlan> PlanAsync(
        CreditSubject subject,
        ActivityType activityType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(activityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(activityType.CreditRulesJson);

        // EVERY read happens in this method and nothing is mutated here. That split is load-bearing. The
        // audit pipeline's catch saves the request's shared DbContext, so an exception thrown after a
        // mutation COMMITS that mutation. Before T130 this method awaited the database between increments: a
        // dropped connection on the third curriculum item would have committed credit for the first two.
        var rules = CreditRulesParser.Parse(activityType.CreditRulesJson);
        if (rules.CountsFor.Count == 0)
        {
            return CreditPlan.Nothing;
        }

        using var document = JsonDocument.Parse(subject.DataJson);

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
        // ObservationDateResolver from the field the PINNED schema's observation_date_field names. The live
        // transition path passes the date it is about to stamp; a replay passes the stamped column. There
        // must be exactly one implementation of "what date did this happen", and it is not this one.
        var trainee = await CreditTargetResolver.ResolveTraineeAsync(_dbContext, subject.SubjectUserId, subject.ObservedOn, cancellationToken);
        if (trainee is null)
        {
            return CreditPlan.Nothing;
        }

        // The ladder an achieved ordinal sits on is declared by the `scale_key` of the schema field the
        // directive names, and the schema here is the one the activity is PINNED to — so this answer is
        // fixed for the life of the activity and replays identically under RebuildCurriculumProgress (T109).
        var achievedScaleIds = await ResolveAchievedScaleIdsAsync(activityType.SchemaJson, rules, cancellationToken);

        // Which items may be credited (T196, D48). Ordinarily those in force at the moment of credit: a completion while
        // an EPA is inactive credits nothing, and a replay of one from before the deactivation credits it as it did live.
        // A reactivation's plan (ResumedEpaId) is the one exception: it credits the reactivated EPA's items alone, as in
        // force, because they are in force again from the moment its save commits. The completion's other items were
        // judged when it completed, and credit never re-litigates them.
        var resumedEpaId = subject.ResumedEpaId;
        DateTime? inForceAt = resumedEpaId is null ? subject.CreditedAt : null;

        var credits = new List<PlannedCredit>();
        foreach (var directive in rules.CountsFor)
        {
            var curriculumItems = await CreditTargetResolver.ResolveCurriculumItemsAsync(_dbContext, directive.CurriculumItemMatchRule, document.RootElement, trainee, inForceAt, cancellationToken);
            if (resumedEpaId is { } epaId)
            {
                curriculumItems = curriculumItems.Where(item => item.EpaId == epaId).ToList();
            }

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

                credits.Add(new PlannedCredit(curriculumItem.Id, directive.Amount, comparison, curriculumItem.ScaleId));
            }
        }

        if (credits.Count == 0)
        {
            return new CreditPlan(subject.SubjectUserId, subject.ObservedOn, subject.ObservedOnDeclared, credits, []);
        }

        // Every row the trainee has on every candidate item, in EVERY semester, not only the one the
        // encounter falls in, so Apply can dedupe across buckets. Two sources, merged by reference:
        //  - a TRACKING query, not AsNoTracking. The rebuild zeroes rows in place without saving, and
        //    identity resolution hands back those zeroed instances. An untracked read would return the stale
        //    stored keys, the replay would dedupe against them and credit nothing, and the rebuild would then
        //    remove every row as unreproduced;
        //  - Local, for rows added and not yet saved (a replay's earlier activities, or an earlier directive).
        // Rows already marked Deleted are left out: they are on their way out, and crediting one would throw
        // the increment away at save.
        var progressSet = _dbContext.Set<CurriculumItemProgress>();
        var candidateItemIds = credits.Select(credit => credit.CurriculumItemId).Distinct().ToList();
        var traineeUserId = subject.SubjectUserId;

        var stored = await progressSet
            .Where(row => row.TraineeUserId == traineeUserId && candidateItemIds.Contains(row.CurriculumItemId))
            .ToListAsync(cancellationToken);

        var existingRows = stored
            .Concat(progressSet.Local.Where(row =>
                row.TraineeUserId == traineeUserId && candidateItemIds.Contains(row.CurriculumItemId)))
            .Distinct<CurriculumItemProgress>(ReferenceEqualityComparer.Instance)
            .Where(row => progressSet.Entry(row).State != EntityState.Deleted)
            .ToList();

        return new CreditPlan(traineeUserId, subject.ObservedOn, subject.ObservedOnDeclared, credits, existingRows);
    }

    public CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(completedActivity);

        if (plan.Credits.Count == 0)
        {
            return CreditApplicationResult.Empty;
        }

        // No awaits from here to the end of the method: see PlanAsync.
        var creditKey = GetCreditKey(completedActivity);

        // The bucket is the semester containing the ENCOUNTER, never the filing date or the transition time,
        // and never anything about the trainee or the item (T130, D41). AcademicPeriod.Containing is total over
        // DateOnly, so this line cannot throw for any stored date: a December encounter, a future-dated one, or
        // one from before the programme started all have a bucket.
        var period = AcademicPeriod.Containing(plan.ObservedOn);
        var progressSet = _dbContext.Set<CurriculumItemProgress>();
        var rows = plan.ExistingRows.ToList();

        var updatedRows = new List<CurriculumItemProgress>();
        var scaleMismatchCount = 0;
        var unverifiedLevelCount = 0;

        foreach (var credit in plan.Credits)
        {
            var itemRows = rows
                .Where(row => row.CurriculumItemId == credit.CurriculumItemId && row.TraineeUserId == plan.TraineeUserId)
                .ToList();

            // Dedupe FIRST, across every semester of the item, and only then pick or create the row. The order
            // matters twice. A re-apply that finds its key in another semester's row must create nothing: an
            // empty row added here would reach neither the rebuild's reproduced set nor its removal set, and
            // would be inserted at save. And looking across semesters is what stops an activity whose
            // encounter date moved after it was credited from being counted in two buckets. It stays counted
            // in the old one until a rebuild moves it.
            if (itemRows.Any(row => DeserializeCreditedKeys(row.CreditedActivityKeysJson).Contains(creditKey)))
            {
                continue;
            }

            var progress = itemRows.FirstOrDefault(row => row.IsIn(period));
            if (progress is null)
            {
                progress = new CurriculumItemProgress
                {
                    CurriculumItemId = credit.CurriculumItemId,
                    TraineeUserId = plan.TraineeUserId,
                    AcademicYear = period.Year,
                    Semester = period.Semester,
                    LastUpdated = DateTime.UtcNow
                };

                progressSet.Add(progress);
                rows.Add(progress);
            }

            var creditedKeys = DeserializeCreditedKeys(progress.CreditedActivityKeysJson);
            creditedKeys.Add(creditKey);

            progress.CountsSoFar += credit.Amount;
            if (credit.Comparison.MinimumMet)
            {
                progress.MinimumLevelReachedCount += credit.Amount;
            }

            // The progress counters are amount-weighted, so they stay comparable with CountsSoFar and
            // MinimumLevelReachedCount beside them. The per-call counters count curriculum ITEMS, so
            // they stay comparable with CreditedItemCount, which is a row count — the transition stamp
            // must not be able to exceed the number of items the same transition credited.
            switch (credit.Comparison.Basis)
            {
                case LevelComparisonBasis.ScaleMismatch:
                    progress.ScaleMismatchCount += credit.Amount;
                    scaleMismatchCount++;
                    break;

                case LevelComparisonBasis.Unpinned:
                    progress.UnverifiedLevelCount += credit.Amount;
                    unverifiedLevelCount++;
                    break;

                case LevelComparisonBasis.SameScale:
                    // The only case where the stored tally rests on a verified ladder. Record which one,
                    // so a later re-pin of the curriculum item makes this row detectably stale rather
                    // than quietly wrong.
                    progress.MinimumLevelScaleId = credit.ItemScaleId;
                    break;
            }

            // The latest ENCOUNTER this bucket counts, and whether anybody stated that date (T219). It is what a
            // reader shows as "last encounter", marked when the date is only the day a form was created. Unlike
            // LastUpdated a rebuild reproduces both exactly: the rule is a maximum, with a tie stated if either
            // encounter stated it, so the order of the credits does not matter.
            progress.NoteEncounter(plan.ObservedOn, plan.ObservedOnDeclared);

            progress.LastActivityId = completedActivity.Id;
            progress.LastUpdated = DateTime.UtcNow;
            progress.CreditedActivityKeysJson = JsonSerializer.Serialize(creditedKeys.OrderBy(value => value));

            updatedRows.Add(progress);
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
    /// field with no <c>scale_key</c>, and a <c>scale_key</c> binding no scale in the database. The last of
    /// those is not hypothetical: until T110 the four generic WBA seeds declared <c>or_scale</c>, which bound
    /// nothing, and a scale deleted before T253 leaves its id-bound keys binding nothing.
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

        // The shared resolver (T253): seed key, id or exact name, read exactly as the rung picker
        // (ActivityReferenceDataService) and every label reads it, so the rung the assessor picked and the ladder
        // the engine scores it on cannot come from different scales.
        var scaleIdByKey = await EntrustmentScaleBindings.ResolveAsync(_dbContext, scaleKeysByField.Values, cancellationToken);

        var resolved = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (fieldKey, scaleKey) in scaleKeysByField)
        {
            if (scaleIdByKey.TryGetValue(scaleKey.Trim(), out var scaleId))
            {
                resolved[fieldKey] = scaleId;
            }
        }

        return resolved;
    }

    private static readonly IReadOnlyDictionary<string, int> EmptyScaleIds =
        new Dictionary<string, int>(StringComparer.Ordinal);

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
            CreditTargetResolver.TryGetInt32(data, directive.MinimumLevelField, out var providedLevel))
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
}
