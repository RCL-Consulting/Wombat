using Microsoft.EntityFrameworkCore;
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
        if (await _dbContext.Institutions.AnyAsync(cancellationToken))
        {
            return;
        }

        var institution = new Institution
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
}
