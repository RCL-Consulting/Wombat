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

        // First, so the vocabulary exists before anything that names it: an allow-list or a type's instrument.
        await EnsureWbaToolsAsync(catalogue.WbaToolVocabulary, cancellationToken);

        // Before the curriculum too, and for a harder reason: an item's DecisionBodyKey is a foreign key (T131).
        await EnsureDecisionBodiesAsync(catalogue.DecisionBodyVocabulary, cancellationToken);

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
    /// The keys are <c>&lt;family&gt;_cpsa</c>.
    /// <list type="bullet">
    /// <item>A SUFFIX, not a prefix, by convention. It once decided the evidence category, which was
    /// matched by exact key or by a "&lt;family&gt;_" prefix; since T144 a type is classified by the
    /// instrument it declares (<c>WbaToolKey</c>), and the key is consulted only for an unkeyed type.</item>
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
                IsActive = true,
                // Which instrument this is, so each EPA's tool list binds it (T122). On create only: an existing
                // database got these from the T122 migration, and a later difference is warned about below.
                WbaToolKey = seed.WbaToolKey,
                // Whether only the system writes it (T162): msf_cpsa. On create only; an existing database got it from the
                // T162 migration, and a later difference is warned about below.
                SystemManaged = seed.SystemManaged
            };

            activityType.SaveDraft(schemaJson, workflowJson, creditJson, displayFieldsJson, ActivityTypeSeedCatalogue.SeedActorUserId);
            activityType.PublishDraft(ActivityTypeSeedCatalogue.SeedActorUserId);

            _dbContext.ActivityTypes.Add(activityType);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await WarnWhereCatalogueColumnsDifferAsync(cancellationToken);
    }

    /// <summary>
    /// Logs, and never writes, every seed-owned CPSA type whose instrument key (T122) or system-managed flag (T162)
    /// differs from its catalogue entry. See <see cref="ActivityTypeSeedCatalogue.FindWbaToolKeyDrift" /> and
    /// <see cref="ActivityTypeSeedCatalogue.FindSystemManagedDrift" />.
    /// </summary>
    private async Task WarnWhereCatalogueColumnsDifferAsync(CancellationToken cancellationToken)
    {
        var keys = ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege).Select(entry => entry.Key).ToArray();
        var stored = await _dbContext.ActivityTypes
            .AsNoTracking()
            .Where(entity => keys.Contains(entity.Key))
            .ToListAsync(cancellationToken);

        foreach (var (entry, storedKey) in ActivityTypeSeedCatalogue.FindWbaToolKeyDrift(stored, ActivityTypeSeedSource.PaediatricCollege))
        {
            _logger.LogWarning(
                "Activity type '{Key}' is recorded as instrument {StoredWbaToolKey}, but its seed entry says {ExpectedWbaToolKey}. Not changed: seeders stamp the instrument on create only, and a seeded type with the wrong instrument is refused or unrestricted on every EPA with a tool list.",
                entry.Key,
                storedKey ?? "(none)",
                entry.WbaToolKey ?? "(none)");
        }

        foreach (var (entry, storedSystemManaged) in ActivityTypeSeedCatalogue.FindSystemManagedDrift(stored, ActivityTypeSeedSource.PaediatricCollege))
        {
            _logger.LogWarning(
                "Activity type '{Key}' is recorded as system-managed {StoredSystemManaged}, but its seed entry says {ExpectedSystemManaged}. Not changed: seeders stamp it on create only, and an existing database follows a change only through a migration. A system-written type that is not flagged is offered to every trainee in its scope.",
                entry.Key,
                storedSystemManaged,
                entry.SystemManaged);
        }
    }

    /// <summary>
    /// Inserts the College's instrument vocabulary and keeps each row's name and definition equal to the catalogue
    /// (T122).
    /// </summary>
    /// <remarks>
    /// A reconcile, unlike every other pass in this seeder, and safe only while nothing else writes this table:
    /// there is no admin command for WbaTools, so the catalogue is the only author a row can have, and without the
    /// reconcile a corrected display name would never reach an existing database. The day an admin surface can edit
    /// a tool, this must become warn-only, as the allow-list and target passes are. A key the catalogue no longer
    /// lists is left in place: an activity type or an allow-list may still name it.
    /// </remarks>
    private async Task EnsureWbaToolsAsync(IReadOnlyList<WbaToolSeed> vocabulary, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.WbaTools.ToDictionaryAsync(tool => tool.Key, StringComparer.Ordinal, cancellationToken);

        foreach (var seed in vocabulary)
        {
            var key = WbaTool.NormalizeKey(seed.Key)
                ?? throw new InvalidOperationException("The paediatric EPA catalogue names a WBA tool with a blank key.");

            if (!existing.TryGetValue(key, out var tool))
            {
                tool = new WbaTool { Key = key };
                _dbContext.WbaTools.Add(tool);
                existing[key] = tool;
            }

            tool.Name = seed.Name;
            tool.Description = seed.Description;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts the College's decision bodies and keeps each row's name equal to the catalogue (T131, Decision 2).
    /// </summary>
    /// <remarks>
    /// A reconcile, like <see cref="EnsureWbaToolsAsync" />, and safe for the same reason: there is no admin command for
    /// DecisionBodies, so the catalogue is the only author a row can have. On a database the T131 migration stamped, the
    /// row it inserted is found here and left as it is. A key the catalogue no longer lists is left in place: an item may
    /// still name it, and the foreign key would refuse the delete anyway.
    /// </remarks>
    private async Task EnsureDecisionBodiesAsync(IReadOnlyList<DecisionBodySeed> vocabulary, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.DecisionBodies.ToDictionaryAsync(body => body.Key, StringComparer.Ordinal, cancellationToken);

        foreach (var seed in vocabulary)
        {
            var key = DecisionBody.NormalizeKey(seed.Key)
                ?? throw new InvalidOperationException("The paediatric EPA catalogue names a decision body with a blank key.");

            if (!existing.TryGetValue(key, out var body))
            {
                body = new DecisionBody { Key = key };
                _dbContext.DecisionBodies.Add(body);
                existing[key] = body;
            }

            body.Name = seed.Name;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
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

            // The provenance pin (T109) is stamped here, on create, and nowhere else (T174).
            curriculum.Items.Add(BuildCurriculumItem(epaId, scaleId, seed, catalogue.DecisionBodyVocabulary));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        WarnWhereTargetsDifferFromTheCatalogue(curriculum, catalogue, epaIdsByCode);
        WarnWhereToolListsDifferFromTheCatalogue(curriculum, catalogue, epaIdsByCode);
        await WarnWhereScalePinsDifferFromTheCatalogueAsync(curriculum, catalogue, epaIdsByCode, scaleId, cancellationToken);
        WarnWhereDecisionsDifferFromTheCatalogue(curriculum, catalogue, epaIdsByCode);
    }

    /// <summary>
    /// Logs, and never writes, every seeded item whose decision cadence, decision body or opportunistic flag no longer
    /// matches the catalogue (T131).
    /// </summary>
    /// <remarks>
    /// The same contract as the target, tool-list and scale-pin passes, for the same reason. Null is a meaningful cadence
    /// ("no published cadence") and a meaningful body ("the general panel"), and both are what an administrator saves when
    /// they clear the field on purpose, so a boot-time <c>is null</c> backfill could not tell a choice from a gap. Existing
    /// databases were stamped once by the T131 migration; a later change reaches them only through a new migration.
    /// </remarks>
    private void WarnWhereDecisionsDifferFromTheCatalogue(
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

            var expected = DecisionFor(seed, catalogue.DecisionBodyVocabulary);
            if (item.DecisionCadence != expected.Cadence
                || !string.Equals(item.DecisionBodyKey, expected.BodyKey, StringComparison.Ordinal)
                || item.DecisionIsOpportunistic != expected.Opportunistic)
            {
                _logger.LogWarning(
                    "Curriculum item {CurriculumItemId} ({EpaCode}) is decided {DecisionCadence} by {DecisionBody}{Opportunistic}, but EPA v11.1 has it decided {ExpectedCadence} by {ExpectedBody}{ExpectedOpportunistic}. Not changed: this seeder never overwrites an existing item.",
                    item.Id,
                    seed.Code,
                    DescribeCadence(item.DecisionCadence),
                    item.DecisionBodyKey ?? "the general panel",
                    item.DecisionIsOpportunistic ? ", as opportunity allows" : string.Empty,
                    DescribeCadence(expected.Cadence),
                    expected.BodyKey ?? "the general panel",
                    expected.Opportunistic ? ", as opportunity allows" : string.Empty);
            }
        }

        static string DescribeCadence(QuotaPeriod? cadence) => cadence switch
        {
            QuotaPeriod.Semester => "each semester",
            QuotaPeriod.AcademicYear => "each academic year",
            null => "on no published cadence",
            _ => $"on cadence {(int)cadence}"
        };
    }

    /// <summary>
    /// The entrustment decision a catalogue EPA publishes, as (cadence, body key, opportunistic) (T131, Decisions 1–2).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cadence is Annexure B's <c>entrustment_decision</c> column as an explicit key, <c>semester</c> or
    /// <c>annual</c>, never parsed out of <c>currency</c>, which may be an expiry (§ 3F question 10). Null in the file is
    /// null here: no published cadence, never due. Nothing defaults to <see cref="QuotaPeriod.AcademicYear" />, the zero
    /// value.
    /// </para>
    /// <para>
    /// Throws on a cadence it does not know, on a body the vocabulary does not list, and on an opportunistic EPA with no
    /// cadence. Each is a defect in the shipped file, and on PostgreSQL the second would otherwise surface as a foreign-key
    /// violation naming no EPA.
    /// </para>
    /// </remarks>
    internal static (QuotaPeriod? Cadence, string? BodyKey, bool Opportunistic) DecisionFor(
        EpaSeed seed,
        IReadOnlyList<DecisionBodySeed> vocabulary)
    {
        QuotaPeriod? cadence = seed.DecisionCadence switch
        {
            null => null,
            "semester" => QuotaPeriod.Semester,
            "annual" => QuotaPeriod.AcademicYear,
            var other => throw new InvalidOperationException(
                $"The paediatric EPA catalogue gives {seed.Code} the decision cadence '{other}'. Expected 'semester', 'annual' or null.")
        };

        var bodyKey = DecisionBody.NormalizeKey(seed.DecisionBody);
        if (bodyKey is not null && !vocabulary.Any(body => DecisionBody.NormalizeKey(body.Key) == bodyKey))
        {
            throw new InvalidOperationException(
                $"The paediatric EPA catalogue has {seed.Code} decided by '{bodyKey}', which its decisionBodyVocabulary does not list.");
        }

        if (seed.DecisionOpportunistic && cadence is null)
        {
            throw new InvalidOperationException(
                $"The paediatric EPA catalogue has {seed.Code} decided as opportunity allows, but gives it no decision cadence.");
        }

        return (cadence, bodyKey, seed.DecisionOpportunistic);
    }

    /// <summary>
    /// Logs, and never writes, every seeded item that is not pinned to the ladder EPA v11.1's minima are written on
    /// (T174).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pin (T109) says which ladder an item's stored minima are ordinals on. It is stamped when this seeder creates
    /// the item (<see cref="BuildCurriculumItem" />), and on no later boot. Before T174 every boot pinned any seeded
    /// item whose <c>ScaleId</c> was null, without resetting its minima and without the check the item editor runs
    /// (<c>CurriculumMappings.EnsureScaleCanExpressMinimaAsync</c>). An administrator who had chosen "Not pinned" and
    /// entered 4 and 8 (both valid unpinned) found the item pinned again after a restart: 4 now meant 3b, and 8 was no
    /// rung at all, so the item could never be credited. Null is also a deliberate, permanent state (unpinned items
    /// compare ordinals as before T109), so a boot-time <c>is null</c> backfill cannot tell a choice from a gap. The
    /// same contract as the target and tool-list passes: seeds stamp on create, and a later change reaches an existing
    /// database only through a migration.
    /// </para>
    /// <para>
    /// Scoped to the EPAs this catalogue declares, as those passes are. National core is not a synonym for seeded: a
    /// CollegeAdmin may add their own national item to this curriculum, on a ladder this seeder cannot know, and there
    /// is nothing for it to differ from. The expected ladder is the one <see cref="EnsureScaleAsync" /> returned, and
    /// deliberately NOT <c>SubSpeciality.DefaultEntrustmentScaleId</c>: that is a committee-picker default an
    /// administrator may change at any time.
    /// </para>
    /// </remarks>
    private async Task WarnWhereScalePinsDifferFromTheCatalogueAsync(
        Curriculum curriculum,
        CatalogueSeed catalogue,
        IReadOnlyDictionary<string, int> epaIdsByCode,
        int scaleId,
        CancellationToken cancellationToken)
    {
        var differing = new List<(string Code, CurriculumItem Item)>();
        foreach (var seed in catalogue.Epas)
        {
            if (!epaIdsByCode.TryGetValue(seed.Code, out var epaId))
            {
                continue;
            }

            var item = curriculum.Items.FirstOrDefault(entity => entity.EpaId == epaId && entity.OwningInstitutionId is null);
            if (item is not null && item.ScaleId != scaleId)
            {
                differing.Add((seed.Code, item));
            }
        }

        if (differing.Count == 0)
        {
            return;
        }

        // Named, not only numbered, so the line says which ladder without a second query by whoever reads it.
        var scaleIds = differing
            .Select(entry => entry.Item.ScaleId)
            .OfType<int>()
            .Append(scaleId)
            .Distinct()
            .ToArray();
        var scaleNames = await _dbContext.EntrustmentScales
            .AsNoTracking()
            .Where(entity => scaleIds.Contains(entity.Id))
            .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

        string Describe(int id) => scaleNames.TryGetValue(id, out var name) ? $"'{name}' (scale {id})" : $"scale {id}";

        foreach (var (code, item) in differing)
        {
            _logger.LogWarning(
                "Curriculum item {CurriculumItemId} ({EpaCode}) is {StoredScale}, but EPA v11.1's minima are written on {ExpectedScale}. Not changed: this seeder pins an item's scale only when it creates the item, because re-pinning an existing item changes what its stored minima mean.",
                item.Id,
                code,
                item.ScaleId is int stored ? $"pinned to {Describe(stored)}" : "not pinned to any scale",
                Describe(scaleId));
        }
    }

    /// <summary>
    /// Logs, and never writes, every seeded item whose tool list no longer matches the catalogue (T122).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The same contract as <see cref="WarnWhereTargetsDifferFromTheCatalogue" />, and for the same reason. The list
    /// is null for "any instrument", and null is also what an administrator saves when they clear it on purpose. An
    /// <c>is null</c> backfill on every boot cannot tell those apart, and would silently re-impose a restriction
    /// someone withdrew. The one-off stamping of existing databases is the T122 migration's job.
    /// </para>
    /// <para>
    /// Compared canonical to canonical: the column is <c>jsonb</c>, so Postgres hands back its own rendering and the
    /// stored string is never byte-equal to what was written.
    /// </para>
    /// </remarks>
    private void WarnWhereToolListsDifferFromTheCatalogue(
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

            var stored = CurriculumItem.ParsePermittedTools(item.PermittedToolsJson);
            var expected = CurriculumItem.ParsePermittedTools(CurriculumItem.NormalizePermittedToolsJson(seed.WbaTools));
            if (!stored.SequenceEqual(expected, StringComparer.Ordinal))
            {
                _logger.LogWarning(
                    "Curriculum item {CurriculumItemId} ({EpaCode}) permits {StoredTools}, but EPA v11.1 permits {ExpectedTools}. Not changed: this seeder never overwrites an existing item.",
                    item.Id,
                    seed.Code,
                    stored.Count == 0 ? "any instrument" : string.Join(", ", stored),
                    expected.Count == 0 ? "any instrument" : string.Join(", ", expected));
            }
        }
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

    private static CurriculumItem BuildCurriculumItem(int epaId, int scaleId, EpaSeed seed, IReadOnlyList<DecisionBodySeed> decisionBodies)
    {
        var (quotaPeriod, requiredCount) = QuotaFor(seed);
        var decision = DecisionFor(seed, decisionBodies);

        return new CurriculumItem
        {
            EpaId = epaId,
            OwningInstitutionId = null,
            // The ladder v11.1's minima are expressed on (T109). Pinned here, on create, and never by a later boot:
            // re-pinning an existing item would change what its stored minima mean (T174). Every minimum below is
            // read straight out of EPA v11.1, whose rungs ARE the ladder EnsureScaleAsync seeds, so this seeder knows
            // the ladder for certain, which nothing downstream does.
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
            WindowMonths = 12,
            // Annexure A's tools cell, as instrument keys with the College's aliases applied (T122, D4, D12). Checked
            // when an activity is filed and submitted, never at credit (D20).
            PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(seed.WbaTools),
            // Annexure B's decision cadence and, for EPAs 4 and 5, the neonatal CCC (T131). On create only, like every
            // value above; an existing database got them from the T131 migration.
            DecisionCadence = decision.Cadence,
            DecisionBodyKey = decision.BodyKey,
            DecisionIsOpportunistic = decision.Opportunistic
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
    // window came to have no source, and how every CPSA tool came to credit every CPSA EPA (T122).
    internal sealed record CatalogueSeed(
        [property: JsonPropertyName("catalogueVersion")] string CatalogueVersion,
        [property: JsonPropertyName("scale")] ScaleSeed Scale,
        [property: JsonPropertyName("wbaToolVocabulary")] IReadOnlyList<WbaToolSeed> WbaToolVocabulary,
        [property: JsonPropertyName("decisionBodyVocabulary")] IReadOnlyList<DecisionBodySeed> DecisionBodyVocabulary,
        [property: JsonPropertyName("epas")] IReadOnlyList<EpaSeed> Epas);

    /// <summary>
    /// One committee the College names as deciding an EPA (T131). The catalogue also carries a <c>note</c> quoting the
    /// Annexure the entry comes from, deliberately not read: provenance for a reader.
    /// </summary>
    internal sealed record DecisionBodySeed(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("name")] string Name);

    /// <summary>
    /// One instrument of the College's vocabulary (T122). The catalogue also carries <c>annexureNames</c>, the
    /// verbatim Annexure A names the key stands for, and a <c>note</c> recording the decision behind an alias. Both
    /// are deliberately not read: they are the audit trail a reader of the annexure needs, not data the product
    /// uses, and a test resolves every EPA's verbatim cell through them.
    /// </summary>
    internal sealed record WbaToolSeed(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string? Description);

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
    /// <param name="WbaTools">
    /// The instruments that may credit this EPA, as vocabulary keys (T122). Annexure A's verbatim cell sits beside it
    /// as <c>annexureTools</c>, deliberately unread, with the College's two aliases (D4, D12) resolved in the
    /// vocabulary and EPA 7's addition of Direct observation (D12) noted as <c>wbaToolsNote</c>.
    /// </param>
    /// <param name="DecisionCadence">
    /// Annexure B's entrustment-decision cadence, <c>semester</c> or <c>annual</c>, or null for none (T131). An explicit
    /// key, like <paramref name="ObservationsPerSemester" />, so nothing parses <c>currency</c>; see <see cref="DecisionFor" />.
    /// </param>
    /// <param name="DecisionBody">A <c>decisionBodyVocabulary</c> key, or null for the general panel (T131).</param>
    /// <param name="DecisionOpportunistic">Whether Annexure A has the EPA decided as opportunity allows (T131, O7).</param>
    internal sealed record EpaSeed(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("domain")] string Domain,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("descriptors")] IReadOnlyList<string> Descriptors,
        [property: JsonPropertyName("observationsPerYear")] int ObservationsPerYear,
        [property: JsonPropertyName("observationsPerSemester")] int? ObservationsPerSemester,
        [property: JsonPropertyName("stageLevels")] IReadOnlyDictionary<string, int> StageLevels,
        [property: JsonPropertyName("minimumLevelOrder")] int MinimumLevelOrder,
        [property: JsonPropertyName("wbaTools")] IReadOnlyList<string> WbaTools,
        [property: JsonPropertyName("decisionCadence")] string? DecisionCadence,
        [property: JsonPropertyName("decisionBody")] string? DecisionBody,
        [property: JsonPropertyName("decisionOpportunistic")] bool DecisionOpportunistic);

    /// <summary>Reads the catalogue file exactly as <see cref="SeedAsync" /> does. For tests.</summary>
    internal static Task<CatalogueSeed> ReadCatalogueForTestsAsync(CancellationToken cancellationToken = default)
        => ReadCatalogueAsync(cancellationToken);
}
