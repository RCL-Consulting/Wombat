using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence;

public sealed class DataSeeder
{
    /// <summary>
    /// The name of the generic observation-to-entrustment scale this seeder creates.
    /// </summary>
    /// <remarks>
    /// A constant rather than a literal because a schema's <c>scale_key</c> binds to a scale by its
    /// EXACT NAME — there is no key column — so this string is a contract with the seed corpus,
    /// not an implementation detail. The four generic WBA seeds declared <c>or_scale</c> against it for
    /// months and bound to nothing at all, because nothing compared the two. `SeedScaleKeyTests` now
    /// does, and references this. (T110)
    /// </remarks>
    public const string OrScaleName = "O-R Scale";

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
        var context = await EnsureDemoContextAsync(cancellationToken);
        await EnsureProcedureCatalogueAsync(cancellationToken);
        await EnsureActivityTypeSeedsAsync(context.SpecialityId, cancellationToken);
    }

    private async Task<DemoSeedContext> EnsureDemoContextAsync(CancellationToken cancellationToken)
    {
        var institution = await _dbContext.Institutions
            .SingleOrDefaultAsync(entity => entity.ShortCode == "DEMO", cancellationToken);

        College college;
        if (institution is null)
        {
            // Specialities are owned by a national College now (T091); the Institution is a separate
            // training site. Seed both, with the demo discipline under the College.
            college = new College
            {
                Name = "Demo College",
                ShortCode = "DEMO-C",
                Description = "Seeded demo college (national catalogue owner).",
                CreatedOn = DateTime.UtcNow
            };

            institution = new Institution
            {
                Name = "Demo Institution",
                ShortCode = "DEMO",
                ContactEmail = "admin@demo.local",
                CreatedOn = DateTime.UtcNow
            };

            var speciality = new Speciality
            {
                Name = "General Medicine",
                Description = "Seeded demo speciality.",
                College = college
            };

            var subSpeciality = new SubSpeciality
            {
                Name = "General Internal Medicine",
                Description = "Seeded demo sub-speciality.",
                Speciality = speciality
            };

            _dbContext.Colleges.Add(college);
            _dbContext.Institutions.Add(institution);
            _dbContext.Specialities.Add(speciality);
            _dbContext.SubSpecialities.Add(subSpeciality);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            college = await _dbContext.Colleges
                .SingleAsync(entity => entity.ShortCode == "DEMO-C", cancellationToken);
        }

        var specialityId = await _dbContext.Specialities
            .Where(entity => entity.CollegeId == college.Id)
            .Select(entity => entity.Id)
            .FirstAsync(cancellationToken);

        var subSpecialityId = await _dbContext.SubSpecialities
            .Where(entity => entity.Speciality.CollegeId == college.Id)
            .Select(entity => entity.Id)
            .FirstAsync(cancellationToken);

        var scale = await _dbContext.EntrustmentScales
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Name == OrScaleName, cancellationToken);

        if (scale is null)
        {
            scale = new EntrustmentScale
            {
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

            _dbContext.EntrustmentScales.Add(scale);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var epa = await _dbContext.Epas
            .SingleOrDefaultAsync(entity => entity.SubSpecialityId == subSpecialityId && entity.Code == "EPA-001", cancellationToken);

        if (epa is null)
        {
            epa = new Epa
            {
                SubSpecialityId = subSpecialityId,
                Code = "EPA-001",
                Title = "Clerk, assess, and present a general medical admission",
                Description = "Seeded demo EPA for local verification.",
                RequiredKnowledgeSkills = "History-taking, examination, differential diagnosis, and presentation.",
                CreatedOn = DateTime.UtcNow
            };

            _dbContext.Epas.Add(epa);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var curriculum = await _dbContext.Curricula
            .Include(entity => entity.Items)
            .SingleOrDefaultAsync(
                entity => entity.SubSpecialityId == subSpecialityId && entity.Name == "IM Core Curriculum" && entity.Version == "2026.1",
                cancellationToken);

        if (curriculum is null)
        {
            curriculum = new Curriculum
            {
                SubSpecialityId = subSpecialityId,
                Name = "IM Core Curriculum",
                Version = "2026.1",
                EffectiveFrom = new DateOnly(2026, 1, 1),
                IsActive = true
            };

            curriculum.Items.Add(new CurriculumItem
            {
                EpaId = epa.Id,
                RequiredCount = 5,
                MinimumLevelOrder = 4,
                WindowMonths = 12,
                ScaleId = scale.Id
            });

            _dbContext.Curricula.Add(curriculum);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!curriculum.Items.Any(entity => entity.EpaId == epa.Id))
        {
            curriculum.Items.Add(new CurriculumItem
            {
                EpaId = epa.Id,
                RequiredCount = 5,
                MinimumLevelOrder = 4,
                WindowMonths = 12,
                ScaleId = scale.Id
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Provenance pin (T109). MinimumLevelOrder = 4 above means "Independent" on the O-R Scale this
        // method seeds, and this seeder is the only author of that number, so it is the one place that
        // can state the ladder without guessing. Applied to rows seeded before T109 too — pinning is
        // idempotent and nothing else may infer it.
        //
        // Scoped to the single EPA this seeder authors. An admin who adds their own item to the demo
        // curriculum chose their own minima, on a ladder this seeder has no way to know.
        foreach (var item in curriculum.Items.Where(entity => entity.EpaId == epa.Id && entity.ScaleId is null))
        {
            item.ScaleId = scale.Id;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DemoSeedContext(institution.Id, specialityId, subSpecialityId, epa.Id);
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

    private async Task EnsureActivityTypeSeedsAsync(int specialityId, CancellationToken cancellationToken)
    {
        var existingKeys = await _dbContext.ActivityTypes
            .Select(entity => entity.Key)
            .ToHashSetAsync(StringComparer.Ordinal, cancellationToken);

        // Creation only. Evolving a type that already exists is ActivityTypeSeedRefresher's job
        // (T103) — editing a seed file used to be a silent no-op here.
        foreach (var seed in ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.Generic))
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
                // On create only (T122). An existing database got its keys from the T122 migration; see
                // ActivityTypeSeedEntry.WbaToolKey.
                WbaToolKey = seed.WbaToolKey
            };

            activityType.SaveDraft(schemaJson, workflowJson, creditJson, displayFieldsJson, ActivityTypeSeedCatalogue.SeedActorUserId);
            activityType.PublishDraft(ActivityTypeSeedCatalogue.SeedActorUserId);

            _dbContext.ActivityTypes.Add(activityType);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await WarnWhereToolKeysDifferAsync(cancellationToken);
    }

    /// <summary>
    /// Logs, and never writes, every seed-owned generic type whose instrument key differs from its catalogue entry
    /// (T122). See <see cref="ActivityTypeSeedCatalogue.FindWbaToolKeyDrift" />.
    /// </summary>
    private async Task WarnWhereToolKeysDifferAsync(CancellationToken cancellationToken)
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
    }

    private sealed record DemoSeedContext(
        int InstitutionId,
        int SpecialityId,
        int SubSpecialityId,
        int EpaId);

    private sealed record ProcedureSeed(
        string Key,
        string Name,
        string Category);
}
