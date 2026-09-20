using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence;

/// <summary>
/// Seeds the national Paediatric EPA catalogue published by the College of Paediatricians of
/// South Africa (EPA version 11.1, September 2026) — the College, its discipline, the six-rung
/// entrustment ladder, the fifteen EPAs, and the curriculum that carries their per-year targets.
/// </summary>
/// <remarks>
/// <para>
/// This is a real national catalogue, not demo data: the EPAs are seeded College-owned
/// (<c>OwningInstitutionId == null</c>), which under T091 means institutions adopt them and can
/// never edit them. It is deliberately separate from <see cref="DataSeeder"/>, whose "Demo
/// College"/"General Medicine" content exists only to make a fresh install explorable.
/// </para>
/// <para>
/// Idempotent: every step is keyed on a stable identifier and skipped when already present, so
/// this runs on every boot alongside the other seeders.
/// </para>
/// <para>
/// See <c>execution/tasks/done/T098-epa-v11-adoption.md</c> for the gap analysis behind this, including
/// what v11.1 asks for that Wombat cannot yet enforce.
/// </para>
/// </remarks>
public sealed class PaediatricCatalogueSeeder
{
    private const string CollegeShortCode = "CPSA";
    private const string SpecialityName = "Paediatrics";
    private const string SubSpecialityName = "Paediatrics";
    private const string CurriculumName = "Paediatric EPA Curriculum";

    /// <summary>
    /// Training programme length in years. v11.1 states its observation frequencies per annum;
    /// <see cref="CurriculumItem.RequiredCount"/> is a whole-programme total, so the annual figure
    /// is multiplied up. See <see cref="BuildCurriculumItem"/> for why that is lossy.
    /// </summary>
    private const int ProgrammeYears = 4;

    private static readonly JsonSerializerOptions SeedJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApplicationDbContext _dbContext;

    public PaediatricCatalogueSeeder(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var catalogue = await ReadCatalogueAsync(cancellationToken);

        var (specialityId, subSpecialityId) = await EnsureCollegeAndDisciplineAsync(cancellationToken);
        var scale = await EnsureScaleAsync(catalogue.Scale, cancellationToken);
        await EnsureDefaultScaleAsync(subSpecialityId, scale.Id, cancellationToken);
        var epaIdsByCode = await EnsureEpasAsync(subSpecialityId, catalogue.Epas, cancellationToken);
        await EnsureCurriculumAsync(subSpecialityId, scale.Id, catalogue, epaIdsByCode, cancellationToken);
        await EnsureActivityTypesAsync(specialityId, cancellationToken);
    }

    /// <summary>
    /// Creates the paediatric WBA tools, Speciality-scoped to Paediatrics. The four keys themselves
    /// live in <see cref="ActivityTypeSeedCatalogue"/>, which the seed refresher reads too.
    /// </summary>
    /// <remarks>
    /// These cannot be seeded by <see cref="DataSeeder"/>: it hard-codes the demo speciality as the
    /// scope for every type it seeds, and it runs before this seeder, so the Paediatrics discipline
    /// does not exist yet when it executes.
    /// <para>
    /// The keys are <c>&lt;family&gt;_cpsa</c>, and both halves of that matter.
    /// <list type="bullet">
    /// <item>A SUFFIX, not a prefix: the EPA trajectory query matches an assessment family by exact
    /// key or by a "&lt;family&gt;_" prefix, so "mini_cex_cpsa" charts as Direct observation while
    /// "cpsa_mini_cex" would silently never appear on any trajectory.</item>
    /// <item><c>_cpsa</c>, not <c>_paed</c>: ActivityType.Key is globally unique and this method
    /// skips keys that already exist, so a collision is a SILENT no-op, not an error. Earlier
    /// scenario play-throughs left institution-scoped "mini_cex_paed" and "dops_paed" types behind,
    /// which swallowed two of these four seeds. Naming them for the owning College keeps them
    /// unambiguous. T103 did not fix that hazard — a seed key that collides with an operator-built
    /// type is still a silent no-op here, and the refresher will not touch it either.</item>
    /// </list>
    /// </para>
    /// </remarks>
    private async Task EnsureActivityTypesAsync(int specialityId, CancellationToken cancellationToken)
    {
        var existingKeys = await _dbContext.ActivityTypes
            .Select(entity => entity.Key)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

        // Creation only; ActivityTypeSeedRefresher (T103) carries later seed edits into types that
        // already exist. The keys and the empty display-fields rule live in the shared catalogue so
        // the refresher reproduces this seeder's own behaviour rather than guessing at it.
        foreach (var seed in ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege))
        {
            if (existingKeys.Contains(seed.Key))
            {
                continue;
            }

            var schemaJson = await ActivityTypeSeedCatalogue.ReadSeedFileAsync(seed.Key, "schema.json", cancellationToken);
            var workflowJson = await ActivityTypeSeedCatalogue.ReadSeedFileAsync(seed.Key, "workflow.json", cancellationToken);
            var creditJson = await ActivityTypeSeedCatalogue.ReadSeedFileAsync(seed.Key, "credit.json", cancellationToken);
            var displayFieldsJson = ActivityTypeSeedCatalogue.BuildDisplayFieldsJson(seed, schemaJson);

            var activityType = new ActivityType
            {
                Key = seed.Key,
                Name = seed.Name,
                Description = seed.Description,
                Scope = seed.Scope,
                ScopeId = specialityId,
                OwnerUserId = ActivityTypeSeedCatalogue.SeedActorUserId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            activityType.SaveDraft(schemaJson, workflowJson, creditJson, displayFieldsJson, ActivityTypeSeedCatalogue.SeedActorUserId);
            activityType.PublishDraft(ActivityTypeSeedCatalogue.SeedActorUserId);

            _dbContext.ActivityTypes.Add(activityType);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<(int SpecialityId, int SubSpecialityId)> EnsureCollegeAndDisciplineAsync(CancellationToken cancellationToken)
    {
        var college = await _dbContext.Colleges
            .SingleOrDefaultAsync(entity => entity.ShortCode == CollegeShortCode, cancellationToken);

        if (college is null)
        {
            college = new College
            {
                Name = "College of Paediatricians of South Africa",
                ShortCode = CollegeShortCode,
                Description = "Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue.",
                CreatedOn = DateTime.UtcNow
            };

            _dbContext.Colleges.Add(college);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var speciality = await _dbContext.Specialities
            .SingleOrDefaultAsync(entity => entity.CollegeId == college.Id && entity.Name == SpecialityName, cancellationToken);

        if (speciality is null)
        {
            speciality = new Speciality
            {
                Name = SpecialityName,
                Description = "Specialist training in Paediatrics.",
                CollegeId = college.Id
            };

            _dbContext.Specialities.Add(speciality);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var subSpeciality = await _dbContext.SubSpecialities
            .SingleOrDefaultAsync(entity => entity.SpecialityId == speciality.Id && entity.Name == SubSpecialityName, cancellationToken);

        if (subSpeciality is null)
        {
            subSpeciality = new SubSpeciality
            {
                Name = SubSpecialityName,
                Description = "General paediatric specialist training programme.",
                SpecialityId = speciality.Id
            };

            _dbContext.SubSpecialities.Add(subSpeciality);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return (speciality.Id, subSpeciality.Id);
    }

    private async Task<EntrustmentScale> EnsureScaleAsync(ScaleSeed seed, CancellationToken cancellationToken)
    {
        var scale = await _dbContext.EntrustmentScales
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Name == seed.Name, cancellationToken);

        if (scale is not null)
        {
            return scale;
        }

        // Order is the rank; Label is the rung as the College prints it. The two diverge from
        // rung 3 onward because v11.1 splits level 3 into 3a and 3b — so Order 5 is labelled "4"
        // and Order 6 is labelled "5". Everything that compares levels must use Order; everything
        // shown to a clinician must use Label.
        scale = new EntrustmentScale
        {
            Name = seed.Name,
            Description = seed.Description,
            Levels = seed.Levels
                .OrderBy(level => level.Order)
                .Select(level => new EntrustmentLevel
                {
                    Order = level.Order,
                    Label = level.Label,
                    Description = level.Description
                })
                .ToList()
        };

        _dbContext.EntrustmentScales.Add(scale);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return scale;
    }

    private async Task EnsureDefaultScaleAsync(int subSpecialityId, int scaleId, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.SubSpecialities
            .SingleAsync(entity => entity.Id == subSpecialityId, cancellationToken);

        if (subSpeciality.DefaultEntrustmentScaleId == scaleId)
        {
            return;
        }

        // Constrains committee STAR level pickers to this ladder for paediatric trainees (T076).
        subSpeciality.DefaultEntrustmentScaleId = scaleId;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyDictionary<string, int>> EnsureEpasAsync(
        int subSpecialityId,
        IReadOnlyList<EpaSeed> seeds,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Epas
            .Where(entity => entity.SubSpecialityId == subSpecialityId && entity.OwningInstitutionId == null)
            .ToDictionaryAsync(entity => entity.Code, entity => entity.Id, StringComparer.Ordinal, cancellationToken);

        foreach (var seed in seeds)
        {
            if (existing.ContainsKey(seed.Code))
            {
                continue;
            }

            var epa = new Epa
            {
                SubSpecialityId = subSpecialityId,
                OwningInstitutionId = null, // national core — institutions adopt, never edit (T091)
                Code = seed.Code,
                Title = seed.Title,
                Domain = seed.Domain,
                Description = seed.Description,
                // v11.1's descriptors have no first-class home in the model yet (T098 gap 3), so
                // they are carried here to keep them visible to trainees and committees rather
                // than living only in the repository. They are narrative here, not individually
                // assessable.
                RequiredKnowledgeSkills = seed.Descriptors.Count == 0
                    ? null
                    : string.Join(Environment.NewLine, seed.Descriptors),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            _dbContext.Epas.Add(epa);
            await _dbContext.SaveChangesAsync(cancellationToken);
            existing[seed.Code] = epa.Id;
        }

        return existing;
    }

    private async Task EnsureCurriculumAsync(
        int subSpecialityId,
        int scaleId,
        CatalogueSeed catalogue,
        IReadOnlyDictionary<string, int> epaIdsByCode,
        CancellationToken cancellationToken)
    {
        var curriculum = await _dbContext.Curricula
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(
                entity => entity.SubSpecialityId == subSpecialityId
                       && entity.Name == CurriculumName
                       && entity.Version == catalogue.CatalogueVersion,
                cancellationToken);

        if (curriculum is null)
        {
            curriculum = new Curriculum
            {
                SubSpecialityId = subSpecialityId,
                Name = CurriculumName,
                Version = catalogue.CatalogueVersion,
                EffectiveFrom = new DateOnly(2026, 1, 1),
                IsActive = true
            };

            _dbContext.Curricula.Add(curriculum);
        }

        foreach (var seed in catalogue.Epas)
        {
            if (!epaIdsByCode.TryGetValue(seed.Code, out var epaId) ||
                curriculum.Items.Any(item => item.EpaId == epaId))
            {
                continue;
            }

            curriculum.Items.Add(BuildCurriculumItem(epaId, scaleId, seed));
        }

        // Provenance pin (T109). Every minimum in this curriculum is read straight out of EPA v11.1, whose
        // rungs ARE the six-rung ladder EnsureScaleAsync seeds, so this seeder knows the ladder for certain
        // — which nothing downstream does.
        //
        // Scoped to the EPAs THIS CATALOGUE declares, not to `OwningInstitutionId is null`. National core
        // is not a synonym for seeded: a CollegeAdmin may add their own national item to this curriculum
        // through the admin UI, and its minima were authored against a ladder this seeder cannot know.
        // Pinning those would assert something untrue, and a wrong pin refuses credit for ever.
        //
        // Deliberately NOT derived from SubSpeciality.DefaultEntrustmentScaleId, even though
        // EnsureDefaultScaleAsync has just set it to this very scale. That field is a committee-picker
        // default an admin may change at any time, and reading it back would silently re-pin a whole
        // curriculum the next time someone did.
        var seededEpaIds = catalogue.Epas
            .Select(seed => epaIdsByCode.TryGetValue(seed.Code, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToHashSet();

        foreach (var item in curriculum.Items.Where(entity =>
                     entity.OwningInstitutionId is null &&
                     entity.ScaleId is null &&
                     seededEpaIds.Contains(entity.EpaId)))
        {
            item.ScaleId = scaleId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static CurriculumItem BuildCurriculumItem(int epaId, int scaleId, EpaSeed seed) => new()
    {
        EpaId = epaId,
        OwningInstitutionId = null,
        // The ladder v11.1's minima are expressed on (T109).
        ScaleId = scaleId,
        // LOSSY: v11.1 states an ANNUAL quota that resets each year ("Six per annum"), but
        // RequiredCount is a whole-programme total and CurriculumItemProgress holds one lifetime
        // row per (item, trainee). Multiplying up preserves the total volume while losing the
        // "per year" semantics entirely — a trainee who does all 24 in year one reads as complete.
        // Making the annual quota real is T098 phase 3.
        RequiredCount = seed.ObservationsPerYear * ProgrammeYears,
        // End-of-programme target, used when a trainee has no resolvable stage.
        MinimumLevelOrder = seed.MinimumLevelOrder,
        // The Y1-Y4 curve from Annexure A, as scale ORDERS (so 3a is 3, 3b is 4, 4 is 5, 5 is 6).
        MinimumLevelByStageJson = JsonSerializer.Serialize(seed.StageLevels),
        WindowMonths = 12
    };

    private static async Task<CatalogueSeed> ReadCatalogueAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The paediatric EPA catalogue seed file was not found.", path);
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<CatalogueSeed>(stream, SeedJsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The paediatric EPA catalogue seed file could not be parsed.");
    }

    private sealed record CatalogueSeed(
        [property: JsonPropertyName("catalogueVersion")] string CatalogueVersion,
        [property: JsonPropertyName("scale")] ScaleSeed Scale,
        [property: JsonPropertyName("epas")] IReadOnlyList<EpaSeed> Epas);

    private sealed record ScaleSeed(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("levels")] IReadOnlyList<LevelSeed> Levels);

    private sealed record LevelSeed(
        [property: JsonPropertyName("order")] int Order,
        [property: JsonPropertyName("label")] string Label,
        [property: JsonPropertyName("description")] string Description);

    private sealed record EpaSeed(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("domain")] string Domain,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("descriptors")] IReadOnlyList<string> Descriptors,
        [property: JsonPropertyName("observationsPerYear")] int ObservationsPerYear,
        [property: JsonPropertyName("stageLevels")] IReadOnlyDictionary<string, int> StageLevels,
        [property: JsonPropertyName("minimumLevelOrder")] int MinimumLevelOrder);
}
