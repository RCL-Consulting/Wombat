using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Infrastructure.Persistence;

public sealed class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;

    public DataSeeder(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
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
    }
}
