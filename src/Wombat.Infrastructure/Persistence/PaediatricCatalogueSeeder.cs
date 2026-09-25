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
/// Idempotent, so this runs on every boot alongside the other seeders. The College, its speciality and sub-speciality,
/// the v11.1 ladder, the fifteen EPAs and the curriculum are each found by a seed key stored on the row (T221), never by a
/// name, code or version an administrator can edit. The catalogue is created once, in one save, on a database that holds
/// none of it; after that a row that cannot be found by its key is announced and skipped, never created again. Before
/// T221 each was found by an editable name and created when the lookup missed, so renaming the sub-speciality duplicated
/// the catalogue and changing CPSA's short code stopped startup on the College name's unique index.
/// </para>
/// <para>
/// See <c>execution/tasks/done/T098-epa-v11-adoption.md</c> for the gap analysis behind this, including
/// what v11.1 asks for that Wombat cannot yet enforce.
/// </para>
/// </remarks>
public sealed class PaediatricCatalogueSeeder
{
    // The seed keys (T221). Stored on each row when this seeder creates it, stamped on existing databases once by the T221
    // migration, and written by nothing else. A change to any of them is a migration, like any other stored seed value.
    internal const string CollegeSeedKey = "cpsa";
    internal const string SpecialitySeedKey = "cpsa:paediatrics";
    internal const string SubSpecialitySeedKey = "cpsa:paediatrics:paediatrics";

    /// <summary>The v11.1 ladder's key, from the catalogue's own version: <c>cpsa:scale:v11.1</c>.</summary>
    internal static string ScaleSeedKey(string catalogueVersion) => $"cpsa:scale:v{catalogueVersion}";

    /// <summary>The curriculum's key, from the catalogue's own version: <c>cpsa:paediatrics:curriculum:v11.1</c>.</summary>
    internal static string CurriculumSeedKey(string catalogueVersion) => $"cpsa:paediatrics:curriculum:v{catalogueVersion}";

    /// <summary>An EPA's key, from the College's code for it: <c>cpsa:paediatrics:epa:PAED-001</c>.</summary>
    internal static string EpaSeedKey(string code) => $"cpsa:paediatrics:epa:{code}";

    // What the catalogue's rows are called when this seeder creates them. Never used to find one.
    private const string CollegeName = "College of Paediatricians of South Africa";
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

        // Each row by its seed key (T221). Created only when the database holds none of the catalogue.
        var rows = await FindCatalogueAsync(catalogue, cancellationToken)
            ?? await CreateCatalogueAsync(catalogue, cancellationToken);

        if (rows.SubSpecialityId is int subSpecialityId && rows.Scale is not null)
        {
            await WarnWhereTheDefaultScaleDiffersAsync(subSpecialityId, rows.Scale, cancellationToken);
        }

        if (rows.Curriculum is not null)
        {
            await EnsureCurriculumItemsAsync(rows.Curriculum, rows.Scale, catalogue, rows.EpasByCode, cancellationToken);
        }

        await EnsureActivityTypesAsync(rows.SpecialityId, cancellationToken);
    }

    /// <summary>
    /// The catalogue's rows, each found by its seed key, or null when the database holds none of them (T221).
    /// </summary>
    /// <remarks>
    /// Each row is found on its own, not through its parent, so a sub-speciality moved to another speciality or an EPA
    /// moved to another sub-speciality is still the catalogue's. A row that is missing while any other is present is
    /// announced once and left missing: this seeder cannot tell a row that was never created from one an administrator
    /// renamed before the T221 migration stamped the keys, or a ladder they deleted, and creating it again is the
    /// duplicate this replaced. Everything that depends on a missing row is skipped with it.
    /// <para>
    /// So the catalogue file reaches a database only once. An EPA added to it later, or a new <c>catalogueVersion</c>
    /// (whose ladder and curriculum keys are new), is announced here as missing at every boot and never created: like
    /// every other seeded value, it reaches an existing database only through a migration.
    /// </para>
    /// </remarks>
    private async Task<CatalogueRows?> FindCatalogueAsync(CatalogueSeed catalogue, CancellationToken cancellationToken)
    {
        var scaleKey = ScaleSeedKey(catalogue.CatalogueVersion);
        var curriculumKey = CurriculumSeedKey(catalogue.CatalogueVersion);
        var codesByEpaKey = catalogue.Epas.ToDictionary(seed => EpaSeedKey(seed.Code), seed => seed.Code, StringComparer.Ordinal);
        var epaKeys = codesByEpaKey.Keys.ToArray();

        var scale = await _dbContext.EntrustmentScales
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.SeedKey == scaleKey, cancellationToken);
        var collegeId = await _dbContext.Colleges
            .Where(entity => entity.SeedKey == CollegeSeedKey)
            .Select(entity => (int?)entity.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var specialityId = await _dbContext.Specialities
            .Where(entity => entity.SeedKey == SpecialitySeedKey)
            .Select(entity => (int?)entity.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var subSpecialityId = await _dbContext.SubSpecialities
            .Where(entity => entity.SeedKey == SubSpecialitySeedKey)
            .Select(entity => (int?)entity.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var curriculum = await _dbContext.Curricula
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(entity => entity.SeedKey == curriculumKey, cancellationToken);
        var epas = await _dbContext.Epas
            .Where(entity => entity.SeedKey != null && epaKeys.Contains(entity.SeedKey))
            .ToListAsync(cancellationToken);

        if (scale is null && collegeId is null && specialityId is null && subSpecialityId is null && curriculum is null && epas.Count == 0)
        {
            return null;
        }

        if (collegeId is null)
        {
            WarnMissing("College", CollegeSeedKey, "Nothing else this seeder does depends on it.");
        }

        if (specialityId is null)
        {
            WarnMissing("speciality", SpecialitySeedKey, "A CPSA activity type not yet in this database is not created, because each is scoped to the speciality.");
        }

        if (subSpecialityId is null)
        {
            WarnMissing("sub-speciality", SubSpecialitySeedKey, "Its default entrustment scale is not checked.");
        }

        if (scale is null)
        {
            WarnMissing("entrustment scale", scaleKey, "No curriculum item is created or checked against the ladder, and the sub-speciality's default scale is not checked.");
        }

        if (curriculum is null)
        {
            WarnMissing("curriculum", curriculumKey, "No curriculum item is created or checked.");
        }

        var epasByCode = epas.ToDictionary(entity => codesByEpaKey[entity.SeedKey!], StringComparer.Ordinal);
        foreach (var seed in catalogue.Epas.Where(seed => !epasByCode.ContainsKey(seed.Code)))
        {
            WarnMissing($"EPA ({seed.Code})", EpaSeedKey(seed.Code), "Its curriculum item is not created or checked.");
        }

        return new CatalogueRows(scale, specialityId, subSpecialityId, curriculum, epasByCode);
    }

    private void WarnMissing(string row, string seedKey, string consequence)
        => _logger.LogWarning(
            "The paediatric EPA catalogue is in this database, but no {CatalogueRow} carries the seed key '{SeedKey}'. Not created: this seeder finds its rows by seed key, never by a name an administrator can edit, and creates the catalogue only on a database that holds none of it. Either the row lost its key (renamed before the T221 migration, or deleted), or the catalogue file added it after this database was seeded, which reaches an existing database only through a migration. {Consequence}",
            row,
            seedKey,
            consequence);

    /// <summary>
    /// Creates the whole catalogue on a database that holds none of it, in one save: the v11.1 ladder, the College, its
    /// speciality and sub-speciality, the fifteen EPAs and the curriculum with an item for each, every row carrying its
    /// seed key (T221).
    /// </summary>
    /// <remarks>
    /// <para>
    /// One save, so the catalogue exists whole or not at all. A boot that stopped half way would otherwise leave rows that
    /// no later boot completes, because once any row is there the seeder creates none (<see cref="FindCatalogueAsync" />).
    /// </para>
    /// <para>
    /// Writes nothing, and says so, when a row it would create collides with one it did not make: a College of the same
    /// name or short code, or a scale of the same name. Creating beside it would break a unique index and stop startup.
    /// Adopting it would find a row by a name an administrator can edit, the defect T221 removed.
    /// </para>
    /// </remarks>
    private async Task<CatalogueRows> CreateCatalogueAsync(CatalogueSeed catalogue, CancellationToken cancellationToken)
    {
        var collisions = new List<string>();
        var collegeCollision = await _dbContext.Colleges
            .AsNoTracking()
            .Where(entity => entity.Name == CollegeName || entity.ShortCode == CollegeShortCode)
            .Select(entity => new { entity.Id, entity.Name, entity.ShortCode })
            .FirstOrDefaultAsync(cancellationToken);
        if (collegeCollision is not null)
        {
            collisions.Add($"College {collegeCollision.Id} ('{collegeCollision.Name}', {collegeCollision.ShortCode})");
        }

        var scaleCollision = await _dbContext.EntrustmentScales
            .AsNoTracking()
            .Where(entity => entity.Name == catalogue.Scale.Name)
            .Select(entity => (int?)entity.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (scaleCollision is int scaleCollisionId)
        {
            collisions.Add($"entrustment scale {scaleCollisionId} ('{catalogue.Scale.Name}')");
        }

        if (collisions.Count > 0)
        {
            _logger.LogWarning(
                "The paediatric EPA catalogue was not seeded, because a row it did not create already holds a name the catalogue needs: {Collisions}. Nothing was written: creating the catalogue beside it would break a unique index. Rename that row, or, if it is the catalogue's own, give it its seed key; the next startup then seeds or finds the catalogue.",
                string.Join("; ", collisions));
            return CatalogueRows.None;
        }

        // Order is the rank; Label is the rung as the College prints it. The two diverge from
        // rung 3 onward because v11.1 splits level 3 into 3a and 3b — so Order 5 is labelled "4"
        // and Order 6 is labelled "5". Everything that compares levels must use Order; everything
        // shown to a clinician must use Label.
        var scale = new EntrustmentScale
        {
            SeedKey = ScaleSeedKey(catalogue.CatalogueVersion),
            Name = catalogue.Scale.Name,
            Description = catalogue.Scale.Description,
            Levels = catalogue.Scale.Levels
                .OrderBy(level => level.Order)
                .Select(level => new EntrustmentLevel
                {
                    Order = level.Order,
                    Label = level.Label,
                    Description = level.Description
                })
                .ToList()
        };

        var college = new College
        {
            SeedKey = CollegeSeedKey,
            Name = CollegeName,
            ShortCode = CollegeShortCode,
            Description = "Constituent College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue.",
            CreatedOn = DateTime.UtcNow
        };

        var speciality = new Speciality
        {
            SeedKey = SpecialitySeedKey,
            Name = SpecialityName,
            Description = "Specialist training in Paediatrics.",
            College = college
        };

        var subSpeciality = new SubSpeciality
        {
            SeedKey = SubSpecialitySeedKey,
            Name = SubSpecialityName,
            Description = "General paediatric specialist training programme.",
            Speciality = speciality,
            // Constrains committee STAR level pickers to this ladder for paediatric trainees (T076). On create only:
            // an administrator may change it on the sub-speciality's edit page, and a later boot never reverts that
            // (T187). Existing databases were stamped by the boots before T187.
            DefaultEntrustmentScale = scale
        };

        var epas = catalogue.Epas.Select(seed => (Seed: seed, Epa: BuildEpa(seed, subSpeciality))).ToList();

        var curriculum = new Curriculum
        {
            SeedKey = CurriculumSeedKey(catalogue.CatalogueVersion),
            SubSpeciality = subSpeciality,
            Name = CurriculumName,
            Version = catalogue.CatalogueVersion,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true,
            // The provenance pin (T109) is stamped here, on create, and nowhere else (T174).
            Items = epas.Select(pair => BuildCurriculumItem(pair.Epa, scale, pair.Seed, catalogue.DecisionBodyVocabulary)).ToList()
        };

        // Added in catalogue order, so the EPAs and items are numbered in it, as they were when each had its own save.
        _dbContext.EntrustmentScales.Add(scale);
        _dbContext.Colleges.Add(college);
        _dbContext.Specialities.Add(speciality);
        _dbContext.SubSpecialities.Add(subSpeciality);
        _dbContext.Epas.AddRange(epas.Select(pair => pair.Epa));
        _dbContext.Curricula.Add(curriculum);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CatalogueRows(
            scale,
            speciality.Id,
            subSpeciality.Id,
            curriculum,
            epas.ToDictionary(pair => pair.Seed.Code, pair => pair.Epa, StringComparer.Ordinal));
    }

    private static Epa BuildEpa(EpaSeed seed, SubSpeciality subSpeciality) => new()
    {
        SeedKey = EpaSeedKey(seed.Code),
        SubSpeciality = subSpeciality,
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

    /// <summary>
    /// The rows <see cref="FindCatalogueAsync" /> found or <see cref="CreateCatalogueAsync" /> created. A null is a row
    /// that is missing, already announced, and skipped by everything that needs it.
    /// </summary>
    private sealed record CatalogueRows(
        EntrustmentScale? Scale,
        int? SpecialityId,
        int? SubSpecialityId,
        Curriculum? Curriculum,
        IReadOnlyDictionary<string, Epa> EpasByCode)
    {
        /// <summary>Nothing: the catalogue could not be created, and the warning says why.</summary>
        public static CatalogueRows None { get; } = new(null, null, null, null, new Dictionary<string, Epa>(StringComparer.Ordinal));
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
    /// <param name="specialityId">
    /// The Paediatrics speciality, which a type this creates is scoped to, or null when it could not be found by its seed
    /// key (T221). Then nothing is created, which the missing row's warning has already said, and the drift warnings still
    /// run.
    /// </param>
    private async Task EnsureActivityTypesAsync(int? specialityId, CancellationToken cancellationToken)
    {
        var existingKeys = await _dbContext.ActivityTypes
            .Select(entity => entity.Key)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

        // Creation only; ActivityTypeSeedRefresher (T103) carries later seed edits into types that
        // already exist. The keys and the empty display-fields rule live in the shared catalogue so
        // the refresher reproduces this seeder's own behaviour rather than guessing at it.
        foreach (var seed in ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege))
        {
            if (existingKeys.Contains(seed.Key) || specialityId is null)
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
                ScopeId = specialityId.Value,
                OwnerUserId = ActivityTypeSeedCatalogue.SeedActorUserId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                // Which instrument this is, so each EPA's tool list binds it (T122). On create only: an existing
                // database got these from the T122 migration, and a later difference is warned about below.
                WbaToolKey = seed.WbaToolKey,
                // Whether only the system writes it (T162): msf_cpsa and learner_feedback_cpsa (T164). On create only; an existing database got it from the
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

    /// <summary>
    /// Logs, and never writes, when the Paediatrics sub-speciality does not default to the v11.1 ladder (T187).
    /// </summary>
    /// <remarks>
    /// Before T187 every boot set the default back to v11.1, so an administrator's change on the sub-speciality's edit
    /// page was silently undone at the next restart. Null is a deliberate state too ("No default — offer every scale"),
    /// so a boot cannot tell a choice from a gap. The same contract as the target, tool-list and scale-pin passes: the
    /// seeder sets the value when it creates the row (<see cref="CreateCatalogueAsync" />), and a difference is announced
    /// at every startup, never reverted.
    /// </remarks>
    private async Task WarnWhereTheDefaultScaleDiffersAsync(int subSpecialityId, EntrustmentScale scale, CancellationToken cancellationToken)
    {
        var subSpeciality = await _dbContext.SubSpecialities
            .AsNoTracking()
            .Where(entity => entity.Id == subSpecialityId)
            .Select(entity => new { entity.Name, entity.DefaultEntrustmentScaleId })
            .SingleAsync(cancellationToken);

        if (subSpeciality.DefaultEntrustmentScaleId == scale.Id)
        {
            return;
        }

        var storedDefault = "has no default scale";
        if (subSpeciality.DefaultEntrustmentScaleId is int storedScaleId)
        {
            // Named, not only numbered, so the line says which ladder without a second query by whoever reads it.
            var storedName = await _dbContext.EntrustmentScales
                .AsNoTracking()
                .Where(entity => entity.Id == storedScaleId)
                .Select(entity => entity.Name)
                .FirstOrDefaultAsync(cancellationToken);
            storedDefault = storedName is null
                ? $"defaults to scale {storedScaleId}"
                : $"defaults to '{storedName}' (scale {storedScaleId})";
        }

        _logger.LogWarning(
            "Sub-speciality {SubSpecialityId} ({SubSpecialityName}) {StoredDefault}, but EPA v11.1 is written on {ExpectedScale}. Not changed: this seeder sets a sub-speciality's default scale only when it creates the sub-speciality, so an administrator's choice on its edit page survives a restart.",
            subSpecialityId,
            subSpeciality.Name,
            storedDefault,
            $"'{scale.Name}' (scale {scale.Id})");
    }

    /// <summary>
    /// Creates the item of every catalogue EPA the curriculum has lost, then announces, and never writes, every item that
    /// differs from the catalogue.
    /// </summary>
    /// <param name="scale">
    /// The v11.1 ladder, which a recreated item is pinned to, or null when it could not be found by its seed key (T221).
    /// Then no item is created and no pin is checked, because this seeder would have no ladder to write the minima on.
    /// </param>
    /// <remarks>
    /// An item an administrator removed comes back on the next boot, pinned (T174): it is found by the EPA it names, which
    /// no rename changes, so this is not the T221 defect. An EPA is found by its seed key wherever it now is, though, and
    /// one a CollegeAdmin has moved to another sub-speciality gets no new item here: a national item names a national EPA
    /// of its curriculum's own sub-speciality (T195).
    /// </remarks>
    private async Task EnsureCurriculumItemsAsync(
        Curriculum curriculum,
        EntrustmentScale? scale,
        CatalogueSeed catalogue,
        IReadOnlyDictionary<string, Epa> epasByCode,
        CancellationToken cancellationToken)
    {
        if (scale is not null)
        {
            foreach (var seed in catalogue.Epas)
            {
                if (!epasByCode.TryGetValue(seed.Code, out var epa) ||
                    curriculum.Items.Any(item => item.EpaId == epa.Id))
                {
                    continue;
                }

                if (epa.SubSpecialityId != curriculum.SubSpecialityId)
                {
                    _logger.LogWarning(
                        "Curriculum {CurriculumId} has no item for EPA {EpaId} ({EpaCode}), which has moved to sub-speciality {EpaSubSpecialityId}. Not created: a national item names a national EPA of its curriculum's own sub-speciality.",
                        curriculum.Id,
                        epa.Id,
                        seed.Code,
                        epa.SubSpecialityId);
                    continue;
                }

                curriculum.Items.Add(BuildCurriculumItem(epa, scale, seed, catalogue.DecisionBodyVocabulary));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var epaIdsByCode = epasByCode.ToDictionary(pair => pair.Key, pair => pair.Value.Id, StringComparer.Ordinal);
        WarnWhereTargetsDifferFromTheCatalogue(curriculum, catalogue, epaIdsByCode);
        WarnWhereToolListsDifferFromTheCatalogue(curriculum, catalogue, epaIdsByCode);
        if (scale is not null)
        {
            await WarnWhereScalePinsDifferFromTheCatalogueAsync(curriculum, catalogue, epaIdsByCode, scale.Id, cancellationToken);
        }

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
    /// is nothing for it to differ from. The expected ladder is the one found by its seed key (T221), and deliberately
    /// NOT <c>SubSpeciality.DefaultEntrustmentScaleId</c>: that is a committee-picker default an administrator may
    /// change at any time.
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

    /// <param name="epa">The EPA, by reference, so a catalogue created in one save can name one it has not saved yet.</param>
    /// <param name="scale">The v11.1 ladder, by reference for the same reason.</param>
    private static CurriculumItem BuildCurriculumItem(Epa epa, EntrustmentScale scale, EpaSeed seed, IReadOnlyList<DecisionBodySeed> decisionBodies)
    {
        var (quotaPeriod, requiredCount) = QuotaFor(seed);
        var decision = DecisionFor(seed, decisionBodies);

        return new CurriculumItem
        {
            Epa = epa,
            OwningInstitutionId = null,
            // The ladder v11.1's minima are expressed on (T109). Pinned here, on create, and never by a later boot:
            // re-pinning an existing item would change what its stored minima mean (T174). Every minimum below is
            // read straight out of EPA v11.1, whose rungs ARE the ladder this seeder creates from the same file, so it
            // knows the ladder for certain, which nothing downstream does.
            Scale = scale,
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
