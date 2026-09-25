using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Tests.Shared;

/// <summary>
/// Seeds an institution's adoption of a curriculum: what it trains (T091). A Speciality-scoped decision panel is offered
/// and created only for a speciality its institution has adopted (T245), as a trainee is admitted only into an adopted
/// curriculum. Linked into every test project that creates such a panel through the real handler.
/// </summary>
/// <remarks>
/// A fixture whose speciality panel is created through the real handler must adopt that speciality at the panel's
/// institution, whoever creates it: an Administrator is held to it too (T245 review), so its assertions keep proving what
/// they proved before T245. Adds only; the caller saves, and seeds the curriculum and its sub-speciality.
/// </remarks>
public static class AdoptionSeed
{
    public static InstitutionCurriculumAdoption Adopt(
        ApplicationDbContext db,
        int id,
        int institutionId,
        int curriculumId,
        int subSpecialityId,
        bool isActive = true)
    {
        var adoption = new InstitutionCurriculumAdoption
        {
            Id = id,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            SubSpecialityId = subSpecialityId,
            AdoptedOn = new DateOnly(2024, 1, 1),
            IsActive = isActive
        };

        db.Set<InstitutionCurriculumAdoption>().Add(adoption);
        return adoption;
    }
}
