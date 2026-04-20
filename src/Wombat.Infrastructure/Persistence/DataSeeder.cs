using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence;

public sealed class DataSeeder
{
    private const string SeedActorUserId = "seed-system";

    private static readonly SeedDefinition[] ActivityTypeSeeds =
    [
        new("mini_cex", "Mini-CEX", "Mini clinical evaluation exercise.", ActivityScope.Speciality),
        new("dops", "DOPS", "Direct observation of procedural skills.", ActivityScope.Speciality),
        new("cbd", "Case-based Discussion", "Structured case-based discussion.", ActivityScope.Speciality),
        new("acat", "ACAT", "Acute care assessment tool.", ActivityScope.Speciality),
        new("reflective_note", "Reflective Note", "Structured reflective note using the situation-task-action-result frame.", ActivityScope.Speciality),
        new("procedure_log", "Procedure Log", "Self-logged procedural experience.", ActivityScope.Speciality),
        new("research_output", "Research Output", "Publication, poster, or presentation evidence.", ActivityScope.Speciality),
        new("teaching_session", "Teaching Session", "Teaching activity delivered by the trainee.", ActivityScope.Speciality),
        new("qi_project", "QI Project", "Quality-improvement project with fixed PDSA sections.", ActivityScope.Speciality),
        new("journal_club", "Journal Club", "Journal club attendance or presentation log.", ActivityScope.Speciality)
    ];

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

    public DataSeeder(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
            .Include(entity => entity.Specialities)
            .ThenInclude(entity => entity.SubSpecialities)
            .SingleOrDefaultAsync(entity => entity.ShortCode == "DEMO", cancellationToken);

        if (institution is null)
        {
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
                Institution = institution
            };

            var subSpeciality = new SubSpeciality
            {
                Name = "General Internal Medicine",
                Description = "Seeded demo sub-speciality.",
                Speciality = speciality
            };

            _dbContext.Institutions.Add(institution);
            _dbContext.Specialities.Add(speciality);
            _dbContext.SubSpecialities.Add(subSpeciality);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var specialityId = await _dbContext.Specialities
            .Where(entity => entity.InstitutionId == institution.Id)
            .Select(entity => entity.Id)
            .FirstAsync(cancellationToken);

        var subSpecialityId = await _dbContext.SubSpecialities
            .Where(entity => entity.Speciality.InstitutionId == institution.Id)
            .Select(entity => entity.Id)
            .FirstAsync(cancellationToken);

        var scale = await _dbContext.EntrustmentScales
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Name == "O-R Scale", cancellationToken);

        if (scale is null)
        {
            scale = new EntrustmentScale
            {
                Name = "O-R Scale",
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
                WindowMonths = 12
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
                WindowMonths = 12
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

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

        foreach (var seed in ActivityTypeSeeds)
        {
            if (existingKeys.Contains(seed.Key))
            {
                continue;
            }

            var schemaJson = await ReadSeedFileAsync(seed.Key, "schema.json", cancellationToken);
            var workflowJson = await ReadSeedFileAsync(seed.Key, "workflow.json", cancellationToken);
            var creditJson = await ReadSeedFileAsync(seed.Key, "credit.json", cancellationToken);
            var displayFieldsJson = BuildDisplayFieldsJson(schemaJson);

            var activityType = new ActivityType
            {
                Key = seed.Key,
                Name = seed.Name,
                Description = seed.Description,
                Scope = seed.Scope,
                ScopeId = specialityId,
                OwnerUserId = SeedActorUserId,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            activityType.SaveDraft(schemaJson, workflowJson, creditJson, displayFieldsJson, SeedActorUserId);
            activityType.PublishDraft(SeedActorUserId);

            _dbContext.ActivityTypes.Add(activityType);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string BuildDisplayFieldsJson(string schemaJson)
    {
        var fields = FormSchemaParser.Parse(schemaJson)
            .Sections
            .SelectMany(section => section.Fields)
            .Select(field => field.Key)
            .Take(3)
            .ToArray();

        return System.Text.Json.JsonSerializer.Serialize(fields);
    }

    private static async Task<string> ReadSeedFileAsync(string activityKey, string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", activityKey, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Seed file '{fileName}' for activity type '{activityKey}' was not found.", path);
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
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

    private sealed record SeedDefinition(
        string Key,
        string Name,
        string Description,
        ActivityScope Scope);
}
