using System.Data.Common;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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
/// institution's own; and any version it adopted before that still holds an item of its own, to keep it. The second arm
/// is for a version the institution has moved off: re-adopting deactivates the old adoption, but its trainees stay pinned
/// to the old version and are still measured against the institution's items there, which clone does not copy. Without it
/// only an Administrator could change those items, though the commands accept the institution's own writes. Before T211
/// the list showed an InstitutionalAdmin their adopted curricula and the read refused them, so every link on the list led
/// to not-found. Anyone else opens nothing, and a curriculum they cannot open reads as not found (T056: 404, not 403).
/// </para>
/// <para>
/// <b>Only where the institution adopted it (T223).</b> Both arms need an adoption, active or since superseded, which is
/// the rule the Add and Update commands keep for an institution's own item (<see cref="EnsureOwnerAdoptedAsync" />). T211's
/// second arm asked only for an item of the institution's own, and the commands did not ask for an adoption, so an
/// institution that saved an item on any curriculum by calling the command directly could then open it. An item left on a
/// curriculum its institution never adopted opens nothing to that institution; the Remove command, which asks for no
/// adoption, takes it off, an Administrator's from the item editor or the institution's sent directly.
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
    /// an InstitutionalAdmin's adoptions: an active one, or a superseded one of a curriculum that holds their institution's
    /// own items (T223), so the list and the one-curriculum read are the same rule. The institution arm is one correlated
    /// EXISTS over the adoptions, with the own-item test a correlated EXISTS inside it, evaluated by the server.
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
                && adoptions.Any(adoption =>
                    adoption.InstitutionId == institutionId
                    && adoption.CurriculumId == curriculum.Id
                    && (adoption.IsActive || curriculum.Items.Any(item => item.OwningInstitutionId == institutionId)))));
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
    /// The items of a curriculum that keep their EPA from an item owned by <paramref name="itemOwningInstitutionId" />
    /// (null for a national item) (T222, T223): every other item that some trainee would be measured against alongside it.
    /// The one rule <see cref="EnsureEpaNotYetOn" /> refuses by, and the item editor's EPA pickers leave out by
    /// (<see cref="CurriculumItemEpas.ListAsync" />), so neither picker offers an EPA its command would refuse as already
    /// on the curriculum.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The rule (T223).</b> An EPA is on a curriculum once for each institution's trainees. A national item is read by
    /// every institution that adopts the curriculum, and an institution's own item by that institution alone. So:
    /// </para>
    /// <list type="bullet">
    /// <item>A national item's EPA is held by every other item on it: another national item, and any institution's own.</item>
    /// <item>An institution's own item's EPA is held by a national item and by another item of the same institution's own,
    /// and not by another institution's: no trainee is measured against both.</item>
    /// </list>
    /// <para>
    /// The database says the same (<c>CurriculumItemConfiguration</c>): two unique indexes, one national item and one item
    /// of each institution's own per EPA, and an exclusion constraint for a national item beside an institution's own.
    /// </para>
    /// <para>
    /// <b>Why a national item and an institution's own item may not share an EPA.</b> That institution's trainees would be
    /// measured against the EPA twice. Credit matches an encounter's EPA to every item the trainee reads and credits each,
    /// progress shows two targets for one EPA, and the committee plans a line per item. It also keeps T091's rule: an
    /// institution adds to the national core and never restates it with a target of its own. So the College cannot add an
    /// EPA an institution already has as its own item until that institution removes it; what should happen to that item
    /// when the College does is not decided here.
    /// </para>
    /// <para>
    /// Before T223 one index held one item per EPA per curriculum whoever owned it (T091), so institution A's own item on a
    /// national EPA kept institution B from one on it too. Before T222 both pickers listed every EPA the item could name. On
    /// the v11.1 curriculum, whose fifteen national EPAs are all items, the Add picker offered fifteen EPAs and the command
    /// refused every one.
    /// </para>
    /// </remarks>
    /// <param name="editedItemId">The item being edited, which keeps its own EPA; null for an Add.</param>
    /// <param name="itemOwningInstitutionId">The owner of the item being added or edited; null for a national item.</param>
    internal static Expression<Func<CurriculumItem, bool>> HoldsItsEpaAgainst(int? editedItemId, int? itemOwningInstitutionId)
        => item => item.Id != editedItemId
            && (itemOwningInstitutionId == null
                || item.OwningInstitutionId == null
                || item.OwningInstitutionId == itemOwningInstitutionId);

    /// <summary>
    /// Refuses an EPA the curriculum already holds against an item of this owner (<see cref="HoldsItsEpaAgainst" />).
    /// Where the item holding it is of another kind, the refusal says which: a national item, or an institution's own,
    /// naming neither the institution nor the item. The College reads no institution's items, so "already contains" alone
    /// would point it at a list that does not show the EPA.
    /// </summary>
    /// <param name="exceptItemId">The item being edited, which may keep its own EPA; null for an Add.</param>
    /// <param name="itemOwningInstitutionId">The owner of the item being added or edited; null for a national item.</param>
    internal static void EnsureEpaNotYetOn(Curriculum curriculum, int epaId, int? exceptItemId, int? itemOwningInstitutionId)
    {
        var holdsItsEpa = HoldsItsEpaAgainst(exceptItemId, itemOwningInstitutionId).Compile();
        var holder = curriculum.Items.FirstOrDefault(item => holdsItsEpa(item) && item.EpaId == epaId);
        if (holder is null)
        {
            return;
        }

        throw new InvalidOperationException(EpaHeldRefusal(itemOwningInstitutionId, holder.OwningInstitutionId));
    }

    /// <summary>
    /// The refusal of an item whose EPA another item holds, in words of which kind of item holds it (T223): the check above
    /// and the translation of a racing write's refusal (<see cref="EpaHeldRefusalAsync" />) say the same thing.
    /// </summary>
    private static string EpaHeldRefusal(int? itemOwningInstitutionId, int? holderOwningInstitutionId)
        => (itemOwningInstitutionId, holderOwningInstitutionId) switch
        {
            (null, not null) => NationalBesideLocal,
            (not null, null) => LocalBesideNational,
            _ => AlreadyContains
        };

    /// <summary>The refusal of an item whose EPA an item of the same kind holds: another national item, or another of the same institution's own.</summary>
    internal const string AlreadyContains = "This curriculum already contains the selected EPA.";

    /// <summary>PostgreSQL's unique_violation: a second national item on an EPA, or a second of one institution's own.</summary>
    private const string UniqueViolation = "23505";

    /// <summary>PostgreSQL's exclusion_violation: a national item beside an institution's own on one EPA (T223).</summary>
    private const string ExclusionViolation = "23P01";

    /// <summary>
    /// Whether the database refused an item's save because another item holds its EPA: one of the two unique indexes, or
    /// the exclusion constraint (<c>CurriculumItemConfiguration</c>). Reached only by a write that raced the command's own
    /// check (<see cref="EnsureEpaNotYetOn" />): the College adding an EPA nationally while an institution adds it as its
    /// own, say. Apart from the generated primary key, those three are the table's only unique or exclusion constraints.
    /// </summary>
    internal static bool IsEpaHeldRefusal(DbUpdateException exception)
        => exception.InnerException is DbException { SqlState: UniqueViolation or ExclusionViolation };

    /// <summary>
    /// The refusal a racing write gets, in the words <see cref="EnsureEpaNotYetOn" /> would have used had the other item
    /// been there when the command checked (T223 review). Which item holds the EPA is read back, as <c>UpdateEpa</c> reads
    /// back which index refused it, rather than parsed out of a provider exception this layer cannot see.
    /// </summary>
    /// <remarks>
    /// The database's refusal stays underneath, so the audit pipeline discards the refused save and writes its row alone
    /// (T201). Before this, the page showed EF's "An error occurred while saving the entity changes".
    /// </remarks>
    /// <param name="exceptItemId">The item being edited; null for an Add.</param>
    /// <param name="itemOwningInstitutionId">The owner of the item being added or edited; null for a national item.</param>
    internal static async Task<InvalidOperationException> EpaHeldRefusalAsync(
        IApplicationDbContext dbContext,
        int curriculumId,
        int epaId,
        int? exceptItemId,
        int? itemOwningInstitutionId,
        DbUpdateException refused,
        CancellationToken cancellationToken)
    {
        var holder = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == curriculumId && item.EpaId == epaId)
            .Where(HoldsItsEpaAgainst(exceptItemId, itemOwningInstitutionId))
            .Select(item => new { item.OwningInstitutionId })
            .FirstOrDefaultAsync(cancellationToken);

        // No holder: the racing item has gone again since. Nothing was saved either way, and the page reads the
        // curriculum again after a refusal (T222 review), so the kind-neutral words are enough.
        return new InvalidOperationException(
            holder is null ? AlreadyContains : EpaHeldRefusal(itemOwningInstitutionId, holder.OwningInstitutionId),
            refused);
    }

    /// <summary>The refusal of a national item on an EPA an institution has as its own item (T223).</summary>
    internal const string NationalBesideLocal =
        "This curriculum already contains the selected EPA, as an institution's own item. A national item cannot share an EPA with an institution's own item: that institution's trainees would be measured against the EPA twice.";

    /// <summary>The refusal of an institution's own item on an EPA a national item holds (T223).</summary>
    internal const string LocalBesideNational =
        "This curriculum already contains the selected EPA, as a national item. An institution's own item adds an EPA to the national curriculum and cannot repeat one on it.";

    /// <summary>
    /// Refuses an institution's own item on a curriculum that institution has never adopted (T223). An adoption since
    /// superseded counts: the institution's trainees admitted under it stay on that version, measured against its items
    /// there (T211). A national item (<paramref name="itemOwningInstitutionId" /> null) is not asked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Add and Update commands and the EPA picker ask it, of the new item's owner and of the stored item's. Remove does
    /// not: an institution takes its own item off a curriculum wherever it is. It is judged by the item's owner, not by the
    /// caller, so an Administrator changing such an item is refused as its institution would be.
    /// </para>
    /// <para>
    /// A read, so every command asks it before its first write: the audit pipeline's save would commit a mutation made
    /// before it threw. Before T223 no command asked, so an InstitutionalAdmin could put an item on any curriculum by
    /// calling the command directly, and then open that curriculum through it (<see cref="Openable" />).
    /// </para>
    /// </remarks>
    internal static async Task EnsureOwnerAdoptedAsync(
        IApplicationDbContext dbContext,
        int curriculumId,
        int? itemOwningInstitutionId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        if (itemOwningInstitutionId is not int owner)
        {
            return;
        }

        var adopted = await dbContext.Set<InstitutionCurriculumAdoption>()
            .AsNoTracking()
            .AnyAsync(adoption => adoption.InstitutionId == owner && adoption.CurriculumId == curriculumId, cancellationToken);
        if (adopted)
        {
            return;
        }

        throw new UnauthorizedAccessException(principal.IsInstitutionalAdmin() && principal.GetInstitutionId() == owner
            ? "Your institution has not adopted this curriculum. An institution adds items of its own only to a curriculum it has adopted."
            : "This item's institution has not adopted this curriculum. An institution keeps items of its own only on a curriculum it has adopted.");
    }
}
