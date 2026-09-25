using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions;

/// <summary>
/// The specialities an institution trains: those it holds an active curriculum adoption in, through the adopted
/// curriculum's sub-speciality. (T092, T245)
/// </summary>
/// <remarks>
/// One predicate, read wherever "the specialities this institution has adopted" is asked: the speciality list an
/// institutional administrator is offered (<c>GetSpecialitiesListQuery</c>, T092), and the specialities a new
/// Speciality-scoped decision panel may cover (<c>CommitteeDecisionAuthorization.CreatableSpecialityIdsAsync</c>, T245),
/// which the panel form offers and panel create demands. A speciality counts once any of its sub-specialities is adopted:
/// a panel covers a speciality, not a sub-speciality. An adoption at another institution, or one no longer active, does
/// not count.
/// </remarks>
internal static class AdoptedSpecialities
{
    /// <summary>The ids of the specialities <paramref name="institutionId" /> holds an active adoption in, as a query.</summary>
    public static IQueryable<int> IdsAt(IApplicationDbContext dbContext, int institutionId)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return from adoption in dbContext.Set<InstitutionCurriculumAdoption>()
               where adoption.IsActive && adoption.InstitutionId == institutionId
               join subSpeciality in dbContext.Set<SubSpeciality>() on adoption.SubSpecialityId equals subSpeciality.Id
               select subSpeciality.SpecialityId;
    }
}
