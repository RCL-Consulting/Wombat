using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Adoptions;

internal static class AdoptionMappings
{
    /// <summary>Maps an adoption whose <paramref name="curriculum"/> has its SubSpeciality/Speciality/College loaded.</summary>
    public static AdoptionDto ToDto(InstitutionCurriculumAdoption adoption, Curriculum curriculum)
        => new(
            adoption.Id,
            adoption.InstitutionId,
            curriculum.Id,
            curriculum.Name,
            curriculum.Version,
            curriculum.SubSpecialityId,
            curriculum.SubSpeciality.Name,
            curriculum.SubSpeciality.Speciality.Name,
            curriculum.SubSpeciality.Speciality.College.Name,
            adoption.AdoptedOn,
            adoption.IsActive);
}
