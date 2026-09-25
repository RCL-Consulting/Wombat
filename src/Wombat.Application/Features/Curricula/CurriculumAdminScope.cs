using System.Linq.Expressions;
using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// Which curricula an admin opens in the curriculum pages, which of a curriculum's items they read there, and which of
/// those they may change (T211). The curricula list, the one-curriculum read and every command's returned curriculum are
/// all cut to the caller by this class, so the pages offer exactly what the commands accept.
/// </summary>
/// <remarks>
/// <para>
/// <b>Who opens a curriculum.</b> An Administrator opens every one. A CollegeAdmin opens their College's: they write its
/// details and national items (<c>CanAccessCollege</c>). An InstitutionalAdmin opens the versions their institution has
/// actively adopted, the ones it admits trainees into (T091 phase 4), to read the national items and to keep the
/// institution's own; and any version that holds an item of their institution's own, to keep it. The second arm is for a
/// version the institution has moved off: re-adopting deactivates the old adoption, but its trainees stay pinned to the
/// old version and are still measured against the institution's items there, which clone does not copy. Without it only
/// an Administrator could change those items, though the commands accept the institution's own writes. Before T211 the
/// list showed an InstitutionalAdmin their adopted curricula and the read refused them, so every link on the list led to
/// not-found. Anyone else opens nothing, and a curriculum they cannot open reads as not found (T056: 404, not 403).
/// </para>
/// <para>
/// <b>Which items they read.</b> A national item, and an institution's own item only where the caller can act for that
/// institution (<c>CanAccessInstitution</c>): an Administrator reads every institution's, an InstitutionalAdmin their own,
/// and a CollegeAdmin none. A curriculum is shared by every institution that adopts it, so one institution's items are
/// nobody else's business; the EPA pages hide a local EPA from the same people. Before T211 every caller read every item,
/// and was offered Edit and Remove on items the commands then refused.
/// </para>
/// <para>
/// <b>Which items they may change.</b> <see cref="CurriculumItemEpas.MayWrite" />, the rule the Add, Update and Remove
/// commands enforce: a national item is the College's, a local item its institution's. Since a caller reads a local item
/// only where they may write it, the one item a caller reads and may not change is a national item read by an
/// InstitutionalAdmin.
/// </para>
/// </remarks>
public static class CurriculumAdminScope
{
    /// <summary>
    /// The curricula the caller may open. The query form of <c>CanAccessCollege</c> (a CollegeAdmin's College) joined with
    /// an InstitutionalAdmin's active adoptions and the curricula holding their institution's own items, so the list and
    /// the one-curriculum read are the same rule. Both arms are correlated EXISTS subqueries, evaluated by the server.
    /// </summary>
    public static IQueryable<Curriculum> Openable(IApplicationDbContext dbContext, ClaimsPrincipal principal)
    {
        var curricula = dbContext.Set<Curriculum>().AsQueryable();
        if (principal.IsAdministrator())
        {
            return curricula;
        }

        // Each role only with its own claim, as CanAccessCollege and CanAccessInstitution read them: every signed-in user
        // carries an institution claim (T113), and it makes nobody an InstitutionalAdmin.
        var collegeId = principal.IsCollegeAdmin() ? principal.GetCollegeId() : null;
        var institutionId = principal.IsInstitutionalAdmin() ? principal.GetInstitutionId() : null;
        if (collegeId is null && institutionId is null)
        {
            return curricula.Where(_ => false);
        }

        var adoptions = dbContext.Set<InstitutionCurriculumAdoption>();
        return curricula.Where(curriculum =>
            (collegeId != null && curriculum.SubSpeciality.Speciality.CollegeId == collegeId)
            || (institutionId != null
                && (adoptions.Any(adoption =>
                        adoption.IsActive && adoption.InstitutionId == institutionId && adoption.CurriculumId == curriculum.Id)
                    || curriculum.Items.Any(item => item.OwningInstitutionId == institutionId))));
    }

    /// <summary>Whether the caller reads an item with this owner: a national item, or a local one they can act for.</summary>
    public static bool Reads(ClaimsPrincipal principal, int? itemOwningInstitutionId)
        => itemOwningInstitutionId is null || principal.CanAccessInstitution(itemOwningInstitutionId.Value);

    /// <summary>
    /// Whether the item editor names the institution that owns each local item the caller reads (T222). True where the
    /// caller's reads can span institutions: an Administrator, who reads every institution's items, and a CollegeAdmin, who
    /// reads none today (<see cref="Reads" />) and would need the name the day they did. False for an InstitutionalAdmin
    /// who is no Administrator: <c>CanAccessInstitution</c> admits them to their own institution's items and no other, so
    /// every local item they read is their own, and the editor says so ("Your institution's own item").
    /// </summary>
    /// <remarks>
    /// Before T222 every local item read "The institution's own item", so an Administrator looking at a curriculum that
    /// several institutions had added to could not tell whose each was. A user who is both CollegeAdmin and
    /// InstitutionalAdmin reads only their own institution's items, and is answered as an InstitutionalAdmin.
    /// </remarks>
    public static bool NamesItemOwners(ClaimsPrincipal principal)
        => principal.IsAdministrator() || !principal.IsInstitutionalAdmin();

    /// <summary>
    /// The curriculum as the caller sees it: only the items they read, each marked with whether they may change it and
    /// named with its owner where <see cref="NamesItemOwners" /> says so, and the curriculum marked with whether they may
    /// change it. Every path that hands a <see cref="CurriculumDto" /> to the pages goes through here, the commands'
    /// returned curricula included: the item editor redraws from them.
    /// </summary>
    /// <param name="collegeId">The College whose curriculum it is.</param>
    public static CurriculumDto ForCaller(CurriculumDto curriculum, int collegeId, ClaimsPrincipal principal)
    {
        var namesOwners = NamesItemOwners(principal);
        return curriculum with
        {
            CanEditCurriculum = principal.CanAccessCollege(collegeId),
            Items = curriculum.Items
                .Where(item => Reads(principal, item.OwningInstitutionId))
                .Select(item => item with
                {
                    OwningInstitutionName = namesOwners && item.IsLocal ? item.OwningInstitutionName : null,
                    CanEdit = CurriculumItemEpas.MayWrite(principal, collegeId, item.OwningInstitutionId)
                })
                .ToList()
        };
    }

    /// <summary>
    /// The items of a curriculum that keep their EPA from another item of it (T222): every item but the one being edited,
    /// whoever owns it. The one rule <see cref="EnsureEpaNotYetOn" /> refuses by, and the item editor's EPA pickers leave
    /// out by (<see cref="CurriculumItemEpas.ListAsync" />), so neither picker offers an EPA its command would refuse as
    /// already on the curriculum.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every owner's item counts, the College's and each institution's alike, because the unique index is one item per EPA
    /// per curriculum, whoever owns it (T091). An EPA held by an item of the same owner is therefore left out, and so is one
    /// held by another owner's item, which the Add and Update commands refuse just the same. [T223] makes the index per
    /// owner; this is the one place the rule then changes, and both the refusal and the pickers follow it.
    /// </para>
    /// <para>
    /// Before T222 both pickers listed every EPA the item could name. On the v11.1 curriculum, whose fifteen national EPAs
    /// are all items, the Add picker offered fifteen EPAs and the command refused every one.
    /// </para>
    /// </remarks>
    /// <param name="editedItemId">The item being edited, which keeps its own EPA; null for an Add.</param>
    internal static Expression<Func<CurriculumItem, bool>> HoldsItsEpaAgainst(int? editedItemId)
        => item => item.Id != editedItemId;

    /// <summary>
    /// Refuses an EPA the curriculum already holds (<see cref="HoldsItsEpaAgainst" />). Where the item holding it is one
    /// the caller does not read, the refusal says it is an institution's own, and names neither the institution nor the
    /// item: otherwise the caller is told the EPA is on a list that does not show it.
    /// </summary>
    /// <param name="exceptItemId">The item being edited, which may keep its own EPA; null for an Add.</param>
    internal static void EnsureEpaNotYetOn(Curriculum curriculum, int epaId, int? exceptItemId, ClaimsPrincipal principal)
    {
        var holdsItsEpa = HoldsItsEpaAgainst(exceptItemId).Compile();
        var holder = curriculum.Items.FirstOrDefault(item => holdsItsEpa(item) && item.EpaId == epaId);
        if (holder is null)
        {
            return;
        }

        throw new InvalidOperationException(Reads(principal, holder.OwningInstitutionId)
            ? "This curriculum already contains the selected EPA."
            : "This curriculum already contains the selected EPA, as an institution's own item. An EPA can be on a curriculum only once.");
    }
}
