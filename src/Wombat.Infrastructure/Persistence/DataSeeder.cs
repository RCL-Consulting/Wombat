using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence;

/// <summary>
/// Seeds the demo data that makes a fresh install explorable: the Demo College with its General Medicine discipline, the
/// Demo Institution, the generic O-R Scale, the demo EPA and the IM Core curriculum, then the procedure catalogue and the
/// ten generic activity types.
/// </summary>
/// <remarks>
/// Idempotent, so this runs on every boot, in every environment. The demo rows are each found by a seed key stored on the
/// row (T229), never by a name, short code or version an administrator can edit. They are created once, in one save, on a
/// database that holds none of them; after that a row that cannot be found by its key is announced and skipped, never
/// created again. Before T229 the College and the institution were found by their short codes: a changed <c>DEMO-C</c>
/// stopped startup on a <c>SingleAsync</c>, and a changed <c>DEMO</c> made the next boot insert a second Demo College and
/// Demo Institution, which the unique name indexes refused, so startup stopped there instead. The scale, EPA and
/// curriculum were found by name or code and created again when the lookup missed. <see cref="PaediatricCatalogueSeeder" />
/// keeps the same contract for the national catalogue (T221).
/// </remarks>
public sealed class DataSeeder
{
    /// <summary>
    /// The name of the generic observation-to-entrustment scale this seeder creates.
    /// </summary>
    /// <remarks>
    /// A constant rather than a literal because a schema's <c>scale_key</c> binds to a scale by its
    /// EXACT NAME, which an administrator can edit, and not by the scale's <c>SeedKey</c> column (T221), so this string
    /// is a contract with the seed corpus, not an implementation detail. The four generic WBA seeds declared
    /// <c>or_scale</c> against it for months and bound to nothing at all, because nothing compared the two.
    /// `SeedScaleKeyTests` now does, and references this. (T110) The seeder itself finds the scale by
    /// <see cref="OrScaleSeedKey" /> (T229), so a renamed O-R Scale is not created again; the name is what it is called on
    /// create, and what a <c>scale_key</c> still has to match.
    /// </remarks>
    public const string OrScaleName = "O-R Scale";

    // The seed keys (T229), each starting "demo". Stored on each row when this seeder creates it, stamped on existing
    // databases once by the T229 migration, and written by nothing else. A change to any of them is a migration, like any
    // other stored seed value.
    internal const string InstitutionSeedKey = "demo";
    internal const string CollegeSeedKey = "demo";
    internal const string SpecialitySeedKey = "demo:general-medicine";
    internal const string SubSpecialitySeedKey = "demo:general-medicine:general-internal-medicine";
    internal const string OrScaleSeedKey = "demo:scale:o-r";
    internal const string EpaSeedKey = "demo:general-medicine:epa:EPA-001";
    internal const string CurriculumSeedKey = "demo:general-medicine:curriculum:v2026.1";

    // Every demo row and its key, as the collision warning names them. The demo data is found only when every row carries
    // its key, so an operator who keys some of them by hand has to key them all.
    private static readonly string DemoSeedKeysForOperators = string.Join(
        "; ",
        new (string Row, string SeedKey)[]
        {
            ("institution", InstitutionSeedKey),
            ("College", CollegeSeedKey),
            ("speciality", SpecialitySeedKey),
            ("sub-speciality", SubSpecialitySeedKey),
            ("entrustment scale", OrScaleSeedKey),
            ($"EPA {EpaCode}", EpaSeedKey),
            ("curriculum", CurriculumSeedKey)
        }.Select(pair => $"{pair.Row} '{pair.SeedKey}'"));

    // What the demo rows are called when this seeder creates them. Never used to find one.
    private const string InstitutionName = "Demo Institution";
    private const string InstitutionShortCode = "DEMO";
    private const string CollegeName = "Demo College";
    private const string CollegeShortCode = "DEMO-C";
    private const string SpecialityName = "General Medicine";
    private const string SubSpecialityName = "General Internal Medicine";
    private const string EpaCode = "EPA-001";
    private const string CurriculumName = "IM Core Curriculum";
    private const string CurriculumVersion = "2026.1";

    private static readonly ProcedureSeed[] ProcedureSeeds =
    [
        new("abdominal_paracentesis", "Abdominal paracentesis", "General medicine"),
        new("arterial_blood_gas", "Arterial blood gas sampling", "General medicine"),
        new("arterial_line", "Arterial line insertion", "Critical care"),
        new("ascitic_tap", "Diagnostic ascitic tap", "General medicine"),
        new("blood_culture", "Peripheral blood culture collection", "General medicine"),
        new("central_line", "Central venous catheter insertion", "Critical care"),
        new("chest_drain", "Chest drain insertion", "Respiratory"),
        new("defibrillation", "Defibrillation / cardioversion", "Emergency care"),
        new("endotracheal_intubation", "Endotracheal intubation", "Critical care"),
        new("intercostal_drain", "Intercostal drain management", "Respiratory"),
        new("joint_aspiration", "Joint aspiration", "Musculoskeletal"),
        new("lumbar_puncture", "Lumbar puncture", "Neurology"),
        new("nasogastric_tube", "Nasogastric tube insertion", "General medicine"),
        new("pleural_aspiration", "Pleural aspiration", "Respiratory"),
        new("suturing", "Simple wound suturing", "Emergency care"),
        new("thoracentesis", "Thoracentesis", "Respiratory"),
        new("tracheostomy_care", "Tracheostomy care", "Critical care"),
        new("urinary_catheter", "Urinary catheterisation", "General medicine"),
        new("venepuncture", "Venepuncture", "General medicine"),
        new("ward_ultrasound", "Focused bedside ultrasound", "General medicine")
    ];

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext dbContext, ILogger<DataSeeder>? logger = null)
    {
        _dbContext = dbContext;
        _logger = logger ?? NullLogger<DataSeeder>.Instance;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Each row by its seed key (T229). Created only when the database holds none of the demo data.
        var rows = await FindDemoAsync(cancellationToken)
            ?? await CreateDemoAsync(cancellationToken);

        if (rows.Curriculum is not null && rows.Epa is not null && rows.Scale is not null)
        {
            await EnsureDemoItemAsync(rows.Curriculum, rows.Epa, rows.Scale, cancellationToken);
        }

        // Found by keys no command can change (a procedure's key has no editor; a published type's key cannot be changed),
        // so these are created one by one whenever they are missing, as before.
        await EnsureProcedureCatalogueAsync(cancellationToken);
        await EnsureActivityTypeSeedsAsync(rows.SpecialityId, cancellationToken);
    }

    /// <summary>
    /// The demo rows, each found by its seed key, or null when the database holds none of them (T229).
    /// </summary>
    /// <remarks>
    /// Each row is found on its own, not through its parent, so a speciality moved to another College or an EPA moved to
    /// another sub-speciality is still the demo's. A row that is missing while any other is present is announced once and
    /// left missing: this seeder cannot tell a row an administrator edited before the T229 migration stamped the keys from
    /// one they deleted, and creating it again is the duplicate (or the startup failure) this replaced. Everything that
    /// depends on a missing row is skipped with it.
    /// </remarks>
    private async Task<DemoRows?> FindDemoAsync(CancellationToken cancellationToken)
    {
        var institutionId = await _dbContext.Institutions
            .Where(entity => entity.SeedKey == InstitutionSeedKey)
            .Select(entity => (int?)entity.Id)
            .SingleOrDefaultAsync(cancellationToken);
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
        var scale = await _dbContext.EntrustmentScales
            .SingleOrDefaultAsync(entity => entity.SeedKey == OrScaleSeedKey, cancellationToken);
        var epa = await _dbContext.Epas
            .SingleOrDefaultAsync(entity => entity.SeedKey == EpaSeedKey, cancellationToken);
        var curriculum = await _dbContext.Curricula
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(entity => entity.SeedKey == CurriculumSeedKey, cancellationToken);

        if (institutionId is null && collegeId is null && specialityId is null && subSpecialityId is null
            && scale is null && epa is null && curriculum is null)
        {
            return null;
        }

        if (institutionId is null)
        {
            WarnMissing("institution", InstitutionSeedKey, "Nothing else this seeder does depends on it.");
        }

        if (collegeId is null)
        {
            WarnMissing("College", CollegeSeedKey, "Nothing else this seeder does depends on it.");
        }

        if (specialityId is null)
        {
            WarnMissing("speciality", SpecialitySeedKey, "A generic activity type not yet in this database is not created, because each is scoped to the speciality.");
        }

        if (subSpecialityId is null)
        {
            WarnMissing("sub-speciality", SubSpecialitySeedKey, "Nothing else this seeder does depends on it.");
        }

        if (scale is null)
        {
            WarnMissing("entrustment scale", OrScaleSeedKey, "The demo curriculum's item is not created or checked against the ladder.");
        }

        if (epa is null)
        {
            WarnMissing($"EPA ({EpaCode})", EpaSeedKey, "Its curriculum item is not created or checked.");
        }

        if (curriculum is null)
        {
            WarnMissing("curriculum", CurriculumSeedKey, "No curriculum item is created or checked.");
        }

        return new DemoRows(specialityId, scale, epa, curriculum);
    }

    private void WarnMissing(string row, string seedKey, string consequence)
        => _logger.LogWarning(
            "The demo data is in this database, but no {DemoRow} carries the seed key '{SeedKey}'. Not created: this seeder finds its rows by seed key, never by a name or short code an administrator can edit, and creates the demo data only on a database that holds none of it. Either the row lost its key (edited before the T229 migration stamped the keys, or deleted), or it was added to this seeder after this database was seeded, which reaches an existing database only through a migration. {Consequence}",
            row,
            seedKey,
            consequence);

    /// <summary>
    /// Creates the demo data on a database that holds none of it, in one save: the Demo College, its speciality and
    /// sub-speciality, the Demo Institution, the O-R Scale, the demo EPA and the IM Core curriculum with its item, every
    /// row carrying its seed key (T229).
    /// </summary>
    /// <remarks>
    /// <para>
    /// One save, so the demo data exists whole or not at all. A boot that stopped half way would otherwise leave rows that
    /// no later boot completes, because once any row is there the seeder creates none (<see cref="FindDemoAsync" />).
    /// </para>
    /// <para>
    /// Writes nothing, and says so, when a row it would create collides with one it did not make: a College or an
    /// institution of the same name or short code, or a scale of the same name. Creating beside it would break a unique
    /// index and stop startup. Adopting it would find a row by a value an administrator can edit, the defect T229 removed.
    /// </para>
    /// </remarks>
    private async Task<DemoRows> CreateDemoAsync(CancellationToken cancellationToken)
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

        var institutionCollision = await _dbContext.Institutions
            .AsNoTracking()
            .Where(entity => entity.Name == InstitutionName || entity.ShortCode == InstitutionShortCode)
            .Select(entity => new { entity.Id, entity.Name, entity.ShortCode })
            .FirstOrDefaultAsync(cancellationToken);
        if (institutionCollision is not null)
        {
            collisions.Add($"institution {institutionCollision.Id} ('{institutionCollision.Name}', {institutionCollision.ShortCode})");
        }

        var scaleCollision = await _dbContext.EntrustmentScales
            .AsNoTracking()
            .Where(entity => entity.Name == OrScaleName)
            .Select(entity => (int?)entity.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (scaleCollision is int scaleCollisionId)
        {
            collisions.Add($"entrustment scale {scaleCollisionId} ('{OrScaleName}')");
        }

        if (collisions.Count > 0)
        {
            _logger.LogWarning(
                "The demo data was not seeded, because a row it did not create already holds a name the demo data needs: {Collisions}. Nothing was written: creating the demo data beside it would break a unique index. If that row is not the demo's, change the name or short code it shares with the demo data, and the next startup seeds the demo data. If it is the demo's own, left without its key when the T229 migration could not find it, give it and every other demo row its seed key: {SeedKeys}. The next startup then finds the demo data. Keying only some of them is not enough: the next startup finds the demo data present, announces each row that has no key, and creates none of them. The generic activity types are not created either, because each is scoped to the demo speciality.",
                string.Join("; ", collisions),
                DemoSeedKeysForOperators);
            return DemoRows.None;
        }

        // Specialities are owned by a national College now (T091); the Institution is a separate training site. Both are
        // seeded, with the demo discipline under the College.
        var college = new College
        {
            SeedKey = CollegeSeedKey,
            Name = CollegeName,
            ShortCode = CollegeShortCode,
            Description = "Seeded demo college (national catalogue owner).",
            CreatedOn = DateTime.UtcNow
        };

        var institution = new Institution
        {
            SeedKey = InstitutionSeedKey,
            Name = InstitutionName,
            ShortCode = InstitutionShortCode,
            ContactEmail = "admin@demo.local",
            CreatedOn = DateTime.UtcNow
        };

        var speciality = new Speciality
        {
            SeedKey = SpecialitySeedKey,
            Name = SpecialityName,
            Description = "Seeded demo speciality.",
            College = college
        };

        var subSpeciality = new SubSpeciality
        {
            SeedKey = SubSpecialitySeedKey,
            Name = SubSpecialityName,
            Description = "Seeded demo sub-speciality.",
            Speciality = speciality
        };

        var scale = new EntrustmentScale
        {
            SeedKey = OrScaleSeedKey,
            Name = OrScaleName,
            Description = "Standard 1-5 observation-to-entrustment scale.",
            Levels =
            [
                new EntrustmentLevel { Order = 1, Label = "Observe only", Description = "Not ready to perform the activity." },
                new EntrustmentLevel { Order = 2, Label = "Direct supervision", Description = "Performs with the supervisor at the elbow." },
                new EntrustmentLevel { Order = 3, Label = "Indirect supervision", Description = "Performs with supervision nearby and readily available." },
                new EntrustmentLevel { Order = 4, Label = "Independent", Description = "Performs independently; supervisor reviews afterwards." },
                new EntrustmentLevel { Order = 5, Label = "Supervises others", Description = "Performs independently and can supervise junior colleagues." }
            ]
        };

        var epa = new Epa
        {
            SeedKey = EpaSeedKey,
            SubSpeciality = subSpeciality,
            Code = EpaCode,
            Title = "Clerk, assess, and present a general medical admission",
            Description = "Seeded demo EPA for local verification.",
            RequiredKnowledgeSkills = "History-taking, examination, differential diagnosis, and presentation.",
            CreatedOn = DateTime.UtcNow
        };

        var curriculum = new Curriculum
        {
            SeedKey = CurriculumSeedKey,
            SubSpeciality = subSpeciality,
            Name = CurriculumName,
            Version = CurriculumVersion,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true,
            Items = [BuildDemoItem(epa, scale)]
        };

        _dbContext.Colleges.Add(college);
        _dbContext.Institutions.Add(institution);
        _dbContext.Specialities.Add(speciality);
        _dbContext.SubSpecialities.Add(subSpeciality);
        _dbContext.EntrustmentScales.Add(scale);
        _dbContext.Epas.Add(epa);
        _dbContext.Curricula.Add(curriculum);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoRows(speciality.Id, scale, epa, curriculum);
    }

    /// <summary>
    /// The demo curriculum's own item: five encounters at Independent on the O-R Scale.
    /// </summary>
    /// <remarks>
    /// Provenance pin (T109). <c>MinimumLevelOrder = 4</c> means "Independent" on the O-R Scale this seeder creates, and
    /// this seeder is the only author of that number, so it is the one place that can state the ladder without guessing.
    /// It is stamped here, on create, and on no later boot (T174): an existing item that differs is announced, never
    /// written. A database whose item predates T109 was pinned once by the T174 migration.
    /// </remarks>
    /// <param name="epa">The demo EPA, by reference, so the demo data created in one save can name one not yet saved.</param>
    /// <param name="scale">The O-R Scale, by reference for the same reason.</param>
    private static CurriculumItem BuildDemoItem(Epa epa, EntrustmentScale scale) => new()
    {
        Epa = epa,
        Scale = scale,
        RequiredCount = 5,
        MinimumLevelOrder = 4,
        WindowMonths = 12
    };

    /// <summary>
    /// Creates the demo item when the demo curriculum has lost it, then announces, and never writes, a pin that differs.
    /// </summary>
    /// <remarks>
    /// An item an administrator removed comes back on the next boot, pinned (T174): it is found by the EPA it names, which
    /// no edit changes, so this is not the T229 defect. An EPA is found by its seed key wherever it now is, though, and
    /// one moved to another sub-speciality gets no new item here: a national item names a national EPA of its curriculum's
    /// own sub-speciality (T195). Only called with all three rows found; a missing one was announced when it was looked
    /// for, and without the ladder there is nothing to pin a recreated item to or check a pin against.
    /// </remarks>
    private async Task EnsureDemoItemAsync(
        Curriculum curriculum,
        Epa epa,
        EntrustmentScale scale,
        CancellationToken cancellationToken)
    {
        if (!curriculum.Items.Any(entity => entity.EpaId == epa.Id))
        {
            if (epa.SubSpecialityId != curriculum.SubSpecialityId)
            {
                _logger.LogWarning(
                    "Curriculum {CurriculumId} has no item for EPA {EpaId} ({EpaCode}), which is in sub-speciality {EpaSubSpecialityId}, not the curriculum's. Not created: a national item names a national EPA of its curriculum's own sub-speciality.",
                    curriculum.Id,
                    epa.Id,
                    epa.Code,
                    epa.SubSpecialityId);
                return;
            }

            curriculum.Items.Add(BuildDemoItem(epa, scale));
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await WarnWhereTheDemoItemScalePinDiffersAsync(curriculum, epa, scale, cancellationToken);
    }

    private async Task EnsureProcedureCatalogueAsync(CancellationToken cancellationToken)
    {
        var existingKeys = await _dbContext.ProcedureCatalogueEntries
            .Select(entity => entity.Key)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

        var newEntries = ProcedureSeeds
            .Where(seed => !existingKeys.Contains(seed.Key))
            .Select(seed => new ProcedureCatalogueEntry
            {
                Key = seed.Key,
                Name = seed.Name,
                Category = seed.Category
            })
            .ToList();

        if (newEntries.Count == 0)
        {
            return;
        }

        _dbContext.ProcedureCatalogueEntries.AddRange(newEntries);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <param name="specialityId">
    /// The demo speciality, which a type this creates is scoped to, or null when it could not be found by its seed key
    /// (T229) or the demo data could not be created. Then nothing is created, which a warning has already said, and the
    /// drift warnings still run.
    /// </param>
    private async Task EnsureActivityTypeSeedsAsync(int? specialityId, CancellationToken cancellationToken)
    {
        var existingKeys = await _dbContext.ActivityTypes
            .Select(entity => entity.Key)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

        // Creation only. Evolving a type that already exists is ActivityTypeSeedRefresher's job
        // (T103) — editing a seed file used to be a silent no-op here.
        foreach (var seed in ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.Generic))
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
                // On create only (T122). An existing database got its keys from the T122 migration; see
                // ActivityTypeSeedEntry.WbaToolKey.
                WbaToolKey = seed.WbaToolKey,
                // On create only (T162), like the instrument. See ActivityTypeSeedEntry.SystemManaged.
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
    /// Logs, and never writes, every seed-owned generic type whose instrument key (T122) or system-managed flag (T162)
    /// differs from its catalogue entry. See <see cref="ActivityTypeSeedCatalogue.FindWbaToolKeyDrift" /> and
    /// <see cref="ActivityTypeSeedCatalogue.FindSystemManagedDrift" />.
    /// </summary>
    private async Task WarnWhereCatalogueColumnsDifferAsync(CancellationToken cancellationToken)
    {
        var keys = ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.Generic).Select(entry => entry.Key).ToArray();
        var stored = await _dbContext.ActivityTypes
            .AsNoTracking()
            .Where(entity => keys.Contains(entity.Key))
            .ToListAsync(cancellationToken);

        foreach (var (entry, storedKey) in ActivityTypeSeedCatalogue.FindWbaToolKeyDrift(stored, ActivityTypeSeedSource.Generic))
        {
            _logger.LogWarning(
                "Activity type '{Key}' is recorded as instrument {StoredWbaToolKey}, but its seed entry says {ExpectedWbaToolKey}. Not changed: seeders stamp the instrument on create only.",
                entry.Key,
                storedKey ?? "(none)",
                entry.WbaToolKey ?? "(none)");
        }

        foreach (var (entry, storedSystemManaged) in ActivityTypeSeedCatalogue.FindSystemManagedDrift(stored, ActivityTypeSeedSource.Generic))
        {
            _logger.LogWarning(
                "Activity type '{Key}' is recorded as system-managed {StoredSystemManaged}, but its seed entry says {ExpectedSystemManaged}. Not changed: seeders stamp it on create only, and an existing database follows a change only through a migration.",
                entry.Key,
                storedSystemManaged,
                entry.SystemManaged);
        }
    }

    /// <summary>
    /// Logs, and never writes, when the demo curriculum's own item is not pinned to the <see cref="OrScaleName" />
    /// ladder its seeded minimum is written on (T174).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before T174 every boot pinned this item whenever its <c>ScaleId</c> was null, keeping its minima and skipping
    /// the check the item editor runs (<c>CurriculumMappings.EnsureScaleCanExpressMinimaAsync</c>). An administrator
    /// who had chosen "Not pinned" (T125) and entered 8 found the item on the O-R Scale after a restart, where 8 is no
    /// rung, so it could never be credited. Null is a deliberate, permanent state as well as a gap, and a boot cannot
    /// tell the two apart, so the pin is stamped on create only. <c>PaediatricCatalogueSeeder</c> keeps the same
    /// contract for the v11.1 items.
    /// </para>
    /// <para>
    /// Scoped, as the stamp is, to the one item on the EPA this seeder authors. An administrator's own item in the
    /// demo curriculum was written on a ladder this seeder cannot know, so there is nothing for it to differ from. The
    /// expected ladder is the one found by its seed key (T229), whatever it is now called.
    /// </para>
    /// </remarks>
    private async Task WarnWhereTheDemoItemScalePinDiffersAsync(
        Curriculum curriculum,
        Epa epa,
        EntrustmentScale scale,
        CancellationToken cancellationToken)
    {
        var item = curriculum.Items.FirstOrDefault(entity => entity.EpaId == epa.Id);
        if (item is null || item.ScaleId == scale.Id)
        {
            return;
        }

        var storedScale = "not pinned to any scale";
        if (item.ScaleId is int storedScaleId)
        {
            // Named, not only numbered, so the line says which ladder without a second query by whoever reads it.
            var storedName = await _dbContext.EntrustmentScales
                .AsNoTracking()
                .Where(entity => entity.Id == storedScaleId)
                .Select(entity => entity.Name)
                .FirstOrDefaultAsync(cancellationToken);
            storedScale = storedName is null
                ? $"pinned to scale {storedScaleId}"
                : $"pinned to '{storedName}' (scale {storedScaleId})";
        }

        _logger.LogWarning(
            "Curriculum item {CurriculumItemId} ({EpaCode}) in '{CurriculumName}' is {StoredScale}, but its seeded minimum is written on {ExpectedScale}. Not changed: this seeder pins an item's scale only when it creates the item, because re-pinning an existing item changes what its stored minima mean.",
            item.Id,
            epa.Code,
            curriculum.Name,
            storedScale,
            $"'{scale.Name}' (scale {scale.Id})");
    }

    /// <summary>
    /// The rows <see cref="FindDemoAsync" /> found or <see cref="CreateDemoAsync" /> created. A null is a row that is
    /// missing, already announced, and skipped by everything that needs it.
    /// </summary>
    private sealed record DemoRows(
        int? SpecialityId,
        EntrustmentScale? Scale,
        Epa? Epa,
        Curriculum? Curriculum)
    {
        /// <summary>Nothing: the demo data could not be created, and the warning says why.</summary>
        public static DemoRows None { get; } = new(null, null, null, null);
    }

    private sealed record ProcedureSeed(
        string Key,
        string Name,
        string Category);
}
