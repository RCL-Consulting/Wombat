using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence;

/// <summary>
/// Seeds the national Paediatric EPA catalogue published by the College of Paediatricians of
/// South Africa (EPA version 11.1, September 2026) — the College, its discipline, the six-rung
/// entrustment ladder, the fifteen EPAs, and the curriculum that carries their per-period targets.
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

    private static readonly JsonSerializerOptions SeedJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PaediatricCatalogueSeeder> _logger;

    public PaediatricCatalogueSeeder(ApplicationDbContext dbContext, ILogger<PaediatricCatalogueSeeder>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger ?? NullLogger<PaediatricCatalogueSeeder>.Instance;
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
    /// Creates the paediatric instruments, Speciality-scoped to Paediatrics. The keys themselves
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
    /// which swallowed two of these seeds. Naming them for the owning College keeps them
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

        WarnWhereTargetsDifferFromTheCatalogue(curriculum, catalogue, epaIdsByCode);
    }

    /// <summary>
    /// Logs, and never writes, every seeded item whose quota no longer matches the catalogue (T130).
    /// </summary>
    /// <remarks>
    /// This seeder skips items that already exist, so a changed target in the catalogue never reaches a
    /// database that already holds the curriculum. That is deliberate: an administrator may have set a
    /// target on purpose, and a reconcile on every boot would silently revert them. The one-off correction
    /// of the pre-T130 lifetime totals is the T130 migration's job. What this adds is that a difference is
    /// never silent. An item still reading 24 against a published "three per semester" is announced at every
    /// startup until someone decides which of the two is right.
    /// </remarks>
    private void WarnWhereTargetsDifferFromTheCatalogue(
        Curriculum curriculum,
        CatalogueSeed catalogue,
        IReadOnlyDictionary<string, int> epaIdsByCode)
    {
        foreach (var seed in catalogue.Epas)
        {
            if (!epaIdsByCode.TryGetValue(seed.Code, out var epaId))
            {
                continue;
            }

            var item = curriculum.Items.FirstOrDefault(entity => entity.EpaId == epaId && entity.OwningInstitutionId is null);
            if (item is null)
            {
                continue;
            }

            var (expectedPeriod, expectedCount) = QuotaFor(seed);
            if (item.QuotaPeriod != expectedPeriod || item.RequiredCount != expectedCount)
            {
                _logger.LogWarning(
                    "Curriculum item {CurriculumItemId} ({EpaCode}) targets {RequiredCount} per {QuotaPeriod}, but EPA v11.1 publishes {ExpectedCount} per {ExpectedPeriod}. Not changed: this seeder never overwrites an existing item.",
                    item.Id,
                    seed.Code,
                    item.RequiredCount,
                    item.QuotaPeriod,
                    expectedCount,
                    expectedPeriod);
            }
        }
    }

    /// <summary>
    /// The quota a catalogue EPA publishes, as (window, target per window) (T130, D39).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Annexure B publishes a per-semester figure for ten EPAs and none for the five at one per annum. Where
    /// one exists the target is per semester; otherwise it is per academic year at the annual figure. The
    /// window comes from that explicit number, never from the catalogue's <c>currency</c> string. That
    /// string is the College's "expiry period if not practised", which Annexure B relabels as the
    /// entrustment-decision cadence. It says "annually" for EPAs 3, 6 and 7, which Annexure B nonetheless
    /// observes three, two and one per semester, and seeding from it would break Annexure B's own total of
    /// 25 per semester. Whether those three are a hard target each semester or a planning split of the annual
    /// figure is still with the College. Progress is stored per semester either way, so the answer changes a
    /// reader, not the data.
    /// </para>
    /// </remarks>
    internal static (QuotaPeriod Period, int RequiredCount) QuotaFor(EpaSeed seed)
        => seed.ObservationsPerSemester is { } perSemester
            ? (QuotaPeriod.Semester, perSemester)
            : (QuotaPeriod.AcademicYear, seed.ObservationsPerYear);

    private static CurriculumItem BuildCurriculumItem(int epaId, int scaleId, EpaSeed seed)
    {
        var (quotaPeriod, requiredCount) = QuotaFor(seed);

        return new CurriculumItem
        {
            EpaId = epaId,
            OwningInstitutionId = null,
            // The ladder v11.1's minima are expressed on (T109).
            ScaleId = scaleId,
            // The published target PER WINDOW (T130, D18): three per semester for PAED-001, one per academic
            // year for PAED-008. Before T130 this was the annual figure multiplied by four programme years,
            // stored as a lifetime total, and the progress page read "1 / 24" against a number the College
            // never published.
            RequiredCount = requiredCount,
            QuotaPeriod = quotaPeriod,
            // End-of-programme target, used when a trainee has no resolvable stage.
            MinimumLevelOrder = seed.MinimumLevelOrder,
            // The Y1-Y4 curve from Annexure A, as scale ORDERS (so 3a is 3, 3b is 4, 4 is 5, 5 is 6).
            MinimumLevelByStageJson = JsonSerializer.Serialize(seed.StageLevels),
            // Not the quota window and not enforced by credit (D19); see CurriculumItem.WindowMonths.
            WindowMonths = 12
        };
    }

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

    // Internal rather than private so Wombat.Infrastructure.Tests can check that every key the catalogue
    // carries is either deserialized here or deliberately left out (T130). Before T130, per-EPA data sat in
    // the JSON that nothing read: `currency` and `wbaTools` were both unread, and that is how the quota's
    // window came to have no source.
    internal sealed record CatalogueSeed(
        [property: JsonPropertyName("catalogueVersion")] string CatalogueVersion,
        [property: JsonPropertyName("scale")] ScaleSeed Scale,
        [property: JsonPropertyName("epas")] IReadOnlyList<EpaSeed> Epas);

    internal sealed record ScaleSeed(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("levels")] IReadOnlyList<LevelSeed> Levels);

    internal sealed record LevelSeed(
        [property: JsonPropertyName("order")] int Order,
        [property: JsonPropertyName("label")] string Label,
        [property: JsonPropertyName("description")] string Description);

    /// <param name="ObservationsPerSemester">
    /// Annexure B's per-semester figure, or null for the five EPAs it lists at one per annum (T130). It is an
    /// explicit key rather than something parsed out of prose, and it, not <c>currency</c>, decides the
    /// quota window; see <see cref="QuotaFor" />.
    /// </param>
    internal sealed record EpaSeed(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("domain")] string Domain,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("descriptors")] IReadOnlyList<string> Descriptors,
        [property: JsonPropertyName("observationsPerYear")] int ObservationsPerYear,
        [property: JsonPropertyName("observationsPerSemester")] int? ObservationsPerSemester,
        [property: JsonPropertyName("stageLevels")] IReadOnlyDictionary<string, int> StageLevels,
        [property: JsonPropertyName("minimumLevelOrder")] int MinimumLevelOrder);

    /// <summary>Reads the catalogue file exactly as <see cref="SeedAsync" /> does. For tests.</summary>
    internal static Task<CatalogueSeed> ReadCatalogueForTestsAsync(CancellationToken cancellationToken = default)
        => ReadCatalogueAsync(cancellationToken);
}
