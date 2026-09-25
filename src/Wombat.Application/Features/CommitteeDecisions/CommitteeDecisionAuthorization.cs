using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Institutions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

internal static class CommitteeDecisionAuthorization
{
    public static string GetRequiredUserId(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("The current user identifier is missing.");

    /// <summary>The refusal to manage panels for a caller who holds no role that manages them. Given before any lookup.</summary>
    internal const string MayNotManagePanels = "You are not allowed to manage committee panels.";

    /// <summary>
    /// The refusal to create, change or open any decision panel for a caller who holds the Trainee role beside a role that
    /// manages panels, and what the panel pages say to them in place of what those roles would offer. It is given before
    /// any panel is looked up, so it says nothing about an id. (T256)
    /// </summary>
    internal const string TraineeManagesNoPanel =
        "You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you.";

    /// <summary>
    /// Refuses, before anything is looked up, a caller who may not manage decision panels at all
    /// (<see cref="MayAdministerPanels" />): panel create and update ask it first. (T256)
    /// </summary>
    /// <remarks>
    /// Only a caller whose Trainee role is what stands in the way, one who also holds a role that manages panels, is told
    /// that it is, as scheduling tells them (<see cref="DemandReviewScheduling" />, T216). Anyone else holds no role that
    /// manages panels, and is told so.
    /// </remarks>
    public static void DemandPanelAdministration(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (MayAdministerPanels(principal))
        {
            return;
        }

        throw new UnauthorizedAccessException(
            HoldsPanelAdministrationRole(principal) ? TraineeManagesNoPanel : MayNotManagePanels);
    }

    /// <summary>
    /// Whether this caller may manage decision panels at all: a role that manages them, and not the Trainee role. Which
    /// panels is <see cref="PanelReachAsync" />'s question, which asks this first. (T256)
    /// </summary>
    /// <remarks>
    /// <b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185), asked before every arm, the
    /// Administrator's included. A registrar who also administers their institution, speciality or sub-speciality, or the
    /// system, creates no panel, changes none and opens none to change it. Not a peer's: a panel's members decide the
    /// entrustment of every trainee it reviews, and its chair alone records and ratifies. Not the one that reviews them
    /// either: a trainee does not choose who sits on their review, as they do not schedule it (T216). Until T256 a Trainee
    /// who also held InstitutionalAdmin, SpecialityAdmin or SubSpecialityAdmin could name the chair and the external
    /// members of the panel that decides their own EPAs, and, as an InstitutionalAdmin, which College committee it sits
    /// as (<see cref="MaySetDecisionBody" />).
    /// </remarks>
    public static bool MayAdministerPanels(ClaimsPrincipal principal)
        => !TraineeScopeResolver.ActsAsTrainee(principal) && HoldsPanelAdministrationRole(principal);

    /// <summary>
    /// Whether this caller holds a role that manages panels, whatever else they hold. Not the right to manage them, which
    /// is <see cref="MayAdministerPanels" />: this only says which refusal a caller who may not is given.
    /// </summary>
    private static bool HoldsPanelAdministrationRole(ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Administrator) ||
           principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin);

    /// <summary>
    /// What the panel pages say to someone who holds Trainee beside a role that manages panels: why they offer no panel to
    /// create or change (<see cref="TraineeManagesNoPanel" />); null for anyone else. (T256)
    /// </summary>
    public static string? TraineeNoteOnPanelPages(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return TraineeScopeResolver.ActsAsTrainee(principal) && HoldsPanelAdministrationRole(principal)
            ? TraineeManagesNoPanel
            : null;
    }

    /// <summary>The one refusal to create, change or open a panel outside the caller's scope.</summary>
    internal const string PanelOutOfScope = "You can only manage panels in your institution.";

    /// <summary>
    /// Whether this caller may create, change or open a panel of this scope, covering this speciality, at this
    /// institution. (T182; the speciality since T131 slice 3)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A global Administrator may manage any panel. Everyone else manages only panels at their own institution, and
    /// only through a panel-administration role: an InstitutionalAdmin any of them; a SpecialityAdmin only a
    /// Speciality-scoped panel covering their own speciality; a SubSpecialityAdmin only a Speciality-scoped panel
    /// covering the speciality one of their sub-specialities belongs to. Neither manages the institution-wide panel.
    /// </para>
    /// <para>
    /// Before T182 the panel's institution was stamped only when an InstitutionalAdmin created it, and the checks
    /// that read it (<c>CanAccessInstitution</c>, which is true for an InstitutionalAdmin alone) were skipped when it
    /// was null. So a SpecialityAdmin's panel carried no institution, could be rewritten by any panel administrator in
    /// the country, and once T182 compared the panel's institution with the trainee's, could review nobody. Every
    /// panel now carries its institution, and this is the one rule that reads it for administration.
    /// </para>
    /// <para>
    /// Until T131 slice 3 the speciality was not read either, so a Surgery SpecialityAdmin could open and rewrite the
    /// members of the Paediatrics panel at their hospital, and create panels for any speciality. Once a panel may sit as
    /// a decision body that is a hijack: a speciality panel sitting as the neonatal CCC decides EPAs 4 and 5 for that
    /// speciality's trainees ahead of every other panel (<see cref="DecisionRouting" />), so whoever names its chair
    /// decides them. The speciality is a national id (T091), so it counts only together with the institution, which is
    /// checked first. The role and its own scope claim are asked together, as <see cref="TraineeScopeResolver" /> does:
    /// a SpecialityAdmin's reach is never lent to another role's claim. Which decision body a panel sits as is a
    /// stricter right, <see cref="MaySetDecisionBody" />.
    /// </para>
    /// </remarks>
    public static async Task<bool> MayAdministerPanelAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        int institutionId,
        DecisionPanelScope scope,
        int? specialityId,
        CancellationToken cancellationToken)
        => (await PanelReachAsync(dbContext, principal, cancellationToken)).Admits(institutionId, scope, specialityId);

    /// <summary>
    /// The panels this caller may create, change or open, as one value: what <see cref="MayAdministerPanelAsync" /> asks
    /// of a panel, and what the panel form offers (<see cref="GetDecisionPanelFormOptionsQuery" />), so the form offers a
    /// scope and a speciality exactly when creating that panel would be accepted. (T194)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The speciality half reads the SubSpecialityAdmin's sub-specialities' specialities from the store, so it is asked
    /// once per request, not once per speciality.
    /// </para>
    /// <para>
    /// Someone who holds Trainee reaches no panel, whatever other role they hold, the Administrator's included
    /// (<see cref="MayAdministerPanels" />, T256): asked first, so the gate, the panel form's read and its offer all refuse
    /// them by this one value, as panel create and update refuse them first.
    /// </para>
    /// </remarks>
    public static async Task<PanelAdministrationReach> PanelReachAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            return PanelAdministrationReach.None;
        }

        if (principal.IsAdministrator())
        {
            return PanelAdministrationReach.EveryPanel;
        }

        if (principal.GetInstitutionId() is not int institutionId)
        {
            return PanelAdministrationReach.None;
        }

        if (principal.IsInstitutionalAdmin())
        {
            return new PanelAdministrationReach(false, institutionId, EveryPanelAtInstitution: true, new HashSet<int>());
        }

        var specialityIds = new HashSet<int>();
        if (principal.IsInRole(WombatRoles.SpecialityAdmin))
        {
            specialityIds.UnionWith(principal.GetSpecialityIds());
        }

        var subSpecialityIds = principal.IsInRole(WombatRoles.SubSpecialityAdmin)
            ? principal.GetSubSpecialityIds().ToArray()
            : [];
        if (subSpecialityIds.Length > 0)
        {
            specialityIds.UnionWith(await dbContext.Set<SubSpeciality>()
                .AsNoTracking()
                .Where(subSpeciality => subSpecialityIds.Contains(subSpeciality.Id))
                .Select(subSpeciality => subSpeciality.SpecialityId)
                .ToListAsync(cancellationToken));
        }

        return new PanelAdministrationReach(false, institutionId, EveryPanelAtInstitution: false, specialityIds);
    }

    /// <summary>
    /// The refusal to create a Speciality-scoped panel for a speciality the panel's institution has adopted no curriculum
    /// in, given to anyone but a global Administrator: their panel runs at their own institution (T245). Given only once
    /// the caller's reach admits the panel, and never instead of <see cref="PanelOutOfScope" /> or
    /// <see cref="DecisionBodyNeedsInstitutionalAdmin" />: who may act is said before what the store holds.
    /// </summary>
    internal const string SpecialityNotAdopted =
        "Your institution has adopted no curriculum in this speciality, so a panel for it would have no trainee to review.";

    /// <summary>
    /// <see cref="SpecialityNotAdopted" /> as a global Administrator is told it, who belongs to no institution and names the
    /// one every panel of theirs runs at (T245 review).
    /// </summary>
    internal const string SpecialityNotAdoptedAtChosenInstitution =
        "The institution you chose has adopted no curriculum in this speciality, so a panel for it would have no trainee " +
        "to review.";

    /// <summary>
    /// Refuses a new panel at <paramref name="institutionId" /> covering a speciality that is not among
    /// <see cref="CreatableSpecialityIdsAsync" /> there, the specialities the panel form offers (T245): with
    /// <see cref="SpecialityNotAdopted" />, or <see cref="SpecialityNotAdoptedAtChosenInstitution" /> for a global
    /// Administrator. Panel create asks it once the caller's <paramref name="reach" /> admits the panel, and before
    /// anything about its members or its College committee's slot is read.
    /// </summary>
    /// <param name="institutionId">The institution the new panel runs at, as panel create resolved it.</param>
    /// <param name="specialityId">
    /// The speciality the new panel covers: null for any panel but a Speciality-scoped one, which asks nothing.
    /// </param>
    public static async Task DemandAdoptedSpecialityAsync(
        IApplicationDbContext dbContext,
        PanelAdministrationReach reach,
        int institutionId,
        int? specialityId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reach);

        if (specialityId is int speciality &&
            !(await CreatableSpecialityIdsAsync(dbContext, reach, institutionId, cancellationToken)).Contains(speciality))
        {
            throw new InvalidOperationException(
                reach.EveryInstitution ? SpecialityNotAdoptedAtChosenInstitution : SpecialityNotAdopted);
        }
    }

    /// <summary>
    /// The specialities a new Speciality-scoped panel of this caller's at <paramref name="institutionId" /> may cover: of
    /// the specialities their reach covers there, the ones that institution has adopted a curriculum in
    /// (<see cref="AdoptedSpecialities" />, the institutional administrator's speciality list's predicate). What the panel
    /// form offers (<see cref="GetDecisionPanelFormOptionsQuery" />) and panel create demands
    /// (<see cref="DemandAdoptedSpecialityAsync" />). (T245)
    /// </summary>
    /// <returns>
    /// Every speciality the institution has adopted for a global Administrator or its InstitutionalAdmin, and only those
    /// among their own for a Speciality or SubSpecialityAdmin; empty at an institution their reach does not admit.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Before T245 a speciality administrator was offered, and could create, a panel for every speciality their claims
    /// named: a SpecialityAdmin whose claims named General Medicine created a General Medicine panel at a hospital that
    /// trains only Paediatrics, which could never have a trainee. An InstitutionalAdmin was offered only the adopted
    /// specialities, but create accepted any from them too, and a global Administrator was offered, and could create, any
    /// speciality at any institution (T245 review). No one is exempt: a panel for a speciality its institution does not
    /// train has no trainee to review, whoever creates it.
    /// </para>
    /// <para>
    /// The institution is the panel's own, as panel create resolved it, not the caller's: a global Administrator has none,
    /// and anyone else's reach admits only their own, so the adoptions read are always those of the institution the panel
    /// would run at, whichever order a caller asks in.
    /// </para>
    /// <para>
    /// Only a new panel is asked. An existing panel keeps its administrators when its institution's adoption lapses: the
    /// trainees admitted under the adoption still follow it, and the panel that reviews them is still theirs to manage by
    /// <see cref="PanelReachAsync" />, which reads no adoption. Updating a panel changes no speciality.
    /// </para>
    /// </remarks>
    public static async Task<IReadOnlySet<int>> CreatableSpecialityIdsAsync(
        IApplicationDbContext dbContext,
        PanelAdministrationReach reach,
        int institutionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(reach);

        if (!reach.EveryInstitution && reach.InstitutionId != institutionId)
        {
            return new HashSet<int>();
        }

        var creatable = (await AdoptedSpecialities.IdsAt(dbContext, institutionId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();

        if (!reach.ManagesEveryPanel)
        {
            creatable.IntersectWith(reach.SpecialityIds);
        }

        return creatable;
    }

    /// <summary>
    /// The one refusal to say which decision body a panel sits as, for anyone but an InstitutionalAdmin or a global
    /// Administrator. It is given before any panel is looked up, so it confirms nothing about an id. (T131)
    /// </summary>
    internal const string DecisionBodyNeedsInstitutionalAdmin =
        "Only an institutional administrator can say which College committee a panel sits as.";

    /// <summary>
    /// Whether this caller may say which College decision body a panel at this institution sits as: a global
    /// Administrator, or an InstitutionalAdmin of that institution. (T131, Decision 3)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Stricter than <see cref="MayAdministerPanelAsync" />, which lets a SpecialityAdmin manage their own speciality's
    /// panel. A body tag takes that body's EPAs away from every other panel covering the trainee: a panel carrying
    /// <c>neonatal</c> that covers the trainee's speciality comes before the institution's own. Which committee decides an
    /// EPA is the institution's arrangement, so it is the institution's administrator's to make.
    /// </para>
    /// <para>
    /// Never someone who holds Trainee (<see cref="MayAdministerPanels" />, T256): the tag chooses which panel decides EPAs
    /// 4 and 5 for every trainee it covers, the caller among them.
    /// </para>
    /// </remarks>
    public static bool MaySetDecisionBody(ClaimsPrincipal principal, int institutionId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return !TraineeScopeResolver.ActsAsTrainee(principal) &&
               (principal.IsAdministrator() ||
                (principal.IsInstitutionalAdmin() && principal.GetInstitutionId() == institutionId));
    }

    /// <summary>
    /// Refuses, before the panel is looked up, a caller who could set no panel's decision body anywhere: one who holds no
    /// role that may (<see cref="DecisionBodyNeedsInstitutionalAdmin" />), or who holds Trainee beside one
    /// (<see cref="TraineeManagesNoPanel" />, T256).
    /// </summary>
    public static void DemandDecisionBodyRole(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (!principal.IsAdministrator() && !principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException(DecisionBodyNeedsInstitutionalAdmin);
        }

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            throw new UnauthorizedAccessException(TraineeManagesNoPanel);
        }
    }

    /// <summary>The refusal to schedule, or preview, a review for a caller who holds no role that schedules.</summary>
    internal const string MayNotScheduleReviews = "You are not allowed to schedule committee reviews.";

    /// <summary>
    /// The refusal to schedule, or preview the agenda of, any committee review for a caller who holds the Trainee role
    /// beside a role that schedules. It is given before any panel or trainee is looked up, so it says nothing about
    /// either id. (T216)
    /// </summary>
    internal const string TraineeSchedulesNoReview =
        "You hold the Trainee role, so you cannot schedule a committee review or preview its agenda, including your own.";

    /// <summary>
    /// Refuses, before anything is looked up, a caller who may not put trainees before a panel at all
    /// (<see cref="MayScheduleReviews" />). (T216)
    /// </summary>
    /// <remarks>
    /// Only a caller whose Trainee role is what stands in the way, one who also holds a role that schedules, is told that
    /// it is. A trainee alone, or one who also sits on a committee, holds no role that schedules, and is told so: the
    /// Trainee sentence would say that dropping the role lets them schedule, which it does not.
    /// </remarks>
    public static void DemandReviewScheduling(ClaimsPrincipal principal)
    {
        if (MayScheduleReviews(principal))
        {
            return;
        }

        throw new UnauthorizedAccessException(
            HoldsSchedulingRole(principal) ? TraineeSchedulesNoReview : MayNotScheduleReviews);
    }

    /// <summary>
    /// What the committee reviews page says to someone who holds Trainee beside a role that schedules: why it offers no
    /// scheduling and lists no review, and where their own are. (T216)
    /// </summary>
    internal const string TraineeSchedulesAndListsNoReview =
        "You hold the Trainee role, so you cannot schedule a committee review or preview its agenda, and this page lists " +
        "no one's reviews. Your own are on My committee reviews once they are ratified.";

    /// <summary>
    /// What the committee reviews page says to someone who holds Trainee beside a role that lists a panel's reviews but
    /// schedules none, a committee member's: why it lists no review, and where their own are. (T216)
    /// </summary>
    internal const string TraineeListsNoReview =
        "You hold the Trainee role, so this page lists no one's committee reviews. Your own are on My committee reviews " +
        "once they are ratified.";

    /// <summary>
    /// Whether this caller may put trainees before a panel at all: a role that schedules reviews, and not the Trainee
    /// role. Which trainees, and on which panels, is <see cref="CommitteeTraineeScope" />'s question. (T182, T216)
    /// </summary>
    /// <remarks>
    /// <para>
    /// InstitutionalAdmin can schedule reviews on panels in their own institution. This mirrors
    /// DemandPanelAdministration, which already admits InstitutionalAdmin — scheduling a review on a panel you can
    /// administer should not require a lesser Coordinator role. (T075 / F-4A-1)
    /// </para>
    /// <para>
    /// <b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185, T216). A registrar who also
    /// coordinates or administers the programme, the Administrator role included, schedules no review and previews no
    /// agenda. Not a peer's: scheduling chooses the panel that decides the peer's EPAs and the window of evidence it
    /// reads, and the agenda preview names the peer's standing on every EPA due, which is the peer's record. Not their
    /// own either, as with revoking, where a trainee revokes none of their own entrustment decisions: a trainee does not
    /// choose which panel sits on them, for which period, or on which evidence. For scheduling, the rung is asked here
    /// and nowhere else. The scheduling command and the agenda preview demand it before anything is looked up
    /// (<see cref="DemandReviewScheduling" />). The scheduling predicate asks it before its Administrator arm
    /// (<see cref="CommitteeTraineeScope.MayScheduleFor" />), so the picker and the decisions-due page's Schedule link
    /// offer such a caller nobody, and the committee reviews page reads it (<see cref="GetCommitteeReviewsAccessQuery" />)
    /// to offer no Schedule review. Until T216 a Trainee who was also a Coordinator could schedule a peer's review, and
    /// preview its agenda.
    /// </para>
    /// </remarks>
    public static bool MayScheduleReviews(ClaimsPrincipal principal)
        => !TraineeScopeResolver.ActsAsTrainee(principal) && HoldsSchedulingRole(principal);

    /// <summary>
    /// Whether this caller holds a role that schedules reviews, whatever else they hold. Not the right to schedule, which
    /// is <see cref="MayScheduleReviews" />: this only says which refusal a caller who may not is given.
    /// </summary>
    private static bool HoldsSchedulingRole(ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Administrator) ||
           principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
           principal.IsInRole(WombatRoles.Coordinator) ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin);

    /// <summary>
    /// Why the committee reviews page lists this caller no review and offers them no scheduling, when the reason is the
    /// Trainee role (T185's rung); null for anyone else. (T216)
    /// </summary>
    public static string? TraineeNoteOnReviewsPage(ClaimsPrincipal principal)
        => !TraineeScopeResolver.ActsAsTrainee(principal) ? null
            : HoldsSchedulingRole(principal) ? TraineeSchedulesAndListsNoReview
            : TraineeListsNoReview;

    /// <summary>
    /// Whether the caller works on this review now: a global Administrator, a Coordinator of the panel's institution, or a
    /// member of its panel who may sit at it now (<see cref="PanelSeat.SittingAt" />, T237's one rule), and in every case
    /// not someone who holds Trainee. Who may start a review (<see cref="DemandStartableReviewAsync" />), whom the review
    /// page offers Start (<see cref="CommitteeReviewDetailDto.CallerMayStart" />), and one of the read ladder's last two
    /// rungs (<see cref="MayReadReview" />).
    /// </summary>
    /// <param name="eligible">
    /// Who may sit on the review's panel now (<see cref="PanelSeat.EligibleAsync" />). A caller who holds no seat on the
    /// panel, or is answered by an arm before the seat's, is answered the same whatever it holds, so the gates read it only
    /// where the seat can decide (<see cref="EligibleWhereTheSeatDecidesAsync" />) and pass <see cref="NotRead" />
    /// elsewhere.
    /// </param>
    /// <remarks>
    /// <para>
    /// A Coordinator supports the programmes of ONE institution, so they reach the panels their own institution runs, not
    /// every institution's. The waiver used to be role-only, which made any Coordinator anywhere a reader of every panel's
    /// reviews and, through the queries that hang off a review, of every trainee's evidence. (T101 finding E)
    /// </para>
    /// <para>
    /// <b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185), asked before every arm, the
    /// Administrator's included. A registrar who coordinates, administers or sits on the panel works on no review: the read
    /// ladder already refused them every review but their own ratified one, and until the T194 review Start did not, so a
    /// Trainee who was also a Coordinator could start a peer's review, freeze its evidence and agenda, and be handed the
    /// whole review the page would not show them. The chair's actions and the appeal body ask the same rung
    /// (<see cref="HoldsSeat" />).
    /// </para>
    /// <para>
    /// <b>A seat counts only while its holder may sit at the review</b> (T279), read from the user store: an active
    /// committee member at the panel's institution who does not hold Trainee, and never the trainee under review. The seat
    /// alone reads the caller's claims, which cannot say whether they still are, and a circuit's claims are frozen for its
    /// life. Until T279 a member who had lost the CommitteeMember role, moved to another institution or been deactivated
    /// could still read the panel's reviews and their frozen evidence, and start one, which freezes the trainee's evidence
    /// snapshot and agenda: the chair's actions had been held to the rule since T256 and the appeal body since T237, but
    /// Start and the read were not. A member of the panel now works on its reviews exactly when they could be seated on it.
    /// </para>
    /// </remarks>
    public static bool WorksOnReview(
        ClaimsPrincipal principal,
        CommitteeReview review,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(eligible);

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            return false;
        }

        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return true;
        }

        if (principal.IsInRole(WombatRoles.Coordinator) && CoordinatesInstitution(principal, review.Panel.InstitutionId))
        {
            return true;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) &&
               PanelSeat.SittingAt(review, eligible)
                   .Any(member => string.Equals(member.UserId, userId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Who may sit on a panel, when it was not read because it could not change the answer
    /// (<see cref="EligibleWhereTheSeatDecidesAsync" />): nobody.
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, UserIdentityDetails> NotRead =
        new Dictionary<string, UserIdentityDetails>(StringComparer.Ordinal);

    /// <summary>
    /// Whether <see cref="WorksOnReview" /> can turn on who may sit on this panel: the caller holds a seat on it, from their
    /// claims, and neither holds Trainee (every seat's first rung, <see cref="HoldsSeat" />) nor is a global Administrator,
    /// whom the arms before the seat's answer. For anyone else the user store is not asked. (T279)
    /// </summary>
    private static bool TheSeatMayDecide(ClaimsPrincipal principal, DecisionPanel panel)
        => !principal.IsInRole(WombatRoles.Administrator) && HoldsSeat(principal, panel, _ => true);

    /// <summary>
    /// Who may sit on each panel among <paramref name="reviews" />' on which the caller holds a seat, by the panel's
    /// institution (<see cref="PanelSeat.EligibleAsync" />, one read per institution); no entry for an institution where the
    /// seat cannot decide (<see cref="TheSeatMayDecide" />). Reads only. (T279)
    /// </summary>
    private static async Task<IReadOnlyDictionary<int, IReadOnlyDictionary<string, UserIdentityDetails>>> EligibleWhereTheSeatDecidesAsync(
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        IEnumerable<CommitteeReview> reviews,
        CancellationToken cancellationToken)
    {
        var eligible = new Dictionary<int, IReadOnlyDictionary<string, UserIdentityDetails>>();
        foreach (var institutionId in reviews
                     .Where(review => TheSeatMayDecide(principal, review.Panel))
                     .Select(review => review.Panel.InstitutionId)
                     .Distinct())
        {
            eligible[institutionId] = await PanelSeat.EligibleAsync(users, institutionId, cancellationToken);
        }

        return eligible;
    }

    /// <summary>
    /// The one refusal for a review id the caller may not read, whether or not the id names a review. (T194 item 1)
    /// </summary>
    internal const string ReviewNotReadableByCaller =
        "The committee review could not be found among the reviews you can view.";

    /// <summary>
    /// The one read ladder for a committee review and for everything computed from it: refuses, before anything about
    /// the review is said, unless <paramref name="review" /> exists and <see cref="MayReadReview" /> holds. An unknown id
    /// and a review out of the caller's reach get the one refusal. (T101 finding E, T194 item 1)
    /// </summary>
    /// <remarks>
    /// <para>
    /// ReviewDetail.razor is fed by sibling queries — the review itself, the sampling concentration report, and the
    /// entrustment decisions staged against it — and each used to carry its own idea of who may read a review, or none
    /// at all. One ladder, called by all of them, is the only arrangement in which they cannot drift apart again; the
    /// count of the window's MSF campaigns missing from the snapshot climbs it too (T173).
    /// </para>
    /// <para>
    /// The page prints the refusal. Until T194 an unknown id was "could not be found" and every other reason had its own
    /// sentence ("You are not a member of this decision panel", "This review is not yet visible to the trainee"), so
    /// anyone who could open the page could walk review ids and learn which exist, and a trainee that a review of theirs
    /// was scheduled. The review's <see cref="CommitteeReview.Panel" /> and its members must be loaded.
    /// </para>
    /// <para>
    /// It reads only (where the review's trainee trains, and only for a speciality administrator; who may sit on the panel,
    /// and only for a caller who holds a seat on it, T279), so it can be asked before anything is changed.
    /// </para>
    /// </remarks>
    public static async Task<CommitteeReview> DemandReviewAccessAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        CommitteeReview? review,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(principal);

        if (review is null ||
            (await ReadableReviewsAsync(dbContext, users, principal, [review], cancellationToken)).Count == 0)
        {
            throw new UnauthorizedAccessException(ReviewNotReadableByCaller);
        }

        return review;
    }

    /// <summary>
    /// The reviews among <paramref name="reviews" /> the caller may read: the set form of
    /// <see cref="DemandReviewAccessAsync" />, by the same rule (<see cref="MayReadReview" />). What the committee reviews
    /// page lists (<see cref="ListReviewsForPanelQuery" />, T218) and what the decisions-due page links to
    /// (<see cref="GetEntrustmentDecisionsDueQuery" />), so each lists or links a review exactly when it will open.
    /// </summary>
    /// <remarks>
    /// Where each review's trainee trains is read only for a caller whose rung asks it (a speciality or sub-speciality
    /// administrator, <see cref="ReadsReviewsByTrainee" />), in one resolve for every review however many there are
    /// (<see cref="TraineeScopeResolver.ResolveManyAsync" />, three queries at most). Who may sit on a panel is read only
    /// for a caller who holds a seat on it, once per panel institution (<see cref="EligibleWhereTheSeatDecidesAsync" />,
    /// T279). Each review's <see cref="CommitteeReview.Panel" /> and its members must be loaded.
    /// </remarks>
    public static async Task<IReadOnlyList<CommitteeReview>> ReadableReviewsAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        IReadOnlyCollection<CommitteeReview> reviews,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(reviews);

        var trainees = reviews.Count > 0 && ReadsReviewsByTrainee(principal)
            ? await TraineeScopeResolver.ResolveManyAsync(
                dbContext, reviews.Select(review => review.TraineeUserId), cancellationToken)
            : new Dictionary<string, TraineeScope>(StringComparer.Ordinal);
        var eligible = await EligibleWhereTheSeatDecidesAsync(users, principal, reviews, cancellationToken);

        return reviews
            .Where(review => MayReadReview(
                principal,
                review,
                trainees.GetValueOrDefault(review.TraineeUserId),
                eligible.GetValueOrDefault(review.Panel.InstitutionId) ?? NotRead))
            .ToList();
    }

    /// <summary>
    /// Whether the caller may read this review, by the read ladder (<see cref="MayReadReview" />), when who may sit on its
    /// panel has already been read (<paramref name="eligible" />, <see cref="PanelSeat.EligibleAsync" />). Where the
    /// review's trainee trains is read only for a caller whose rung asks it. Reads only. The seat refusals ask it, so that
    /// only someone who may read the review is told that they hold a seat on its panel (T279).
    /// </summary>
    internal static async Task<bool> MayReadReviewAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CommitteeReview review,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(eligible);

        var trainee = ReadsReviewsByTrainee(principal)
            ? (await TraineeScopeResolver.ResolveManyAsync(dbContext, [review.TraineeUserId], cancellationToken))
                .GetValueOrDefault(review.TraineeUserId)
            : null;

        return MayReadReview(principal, review, trainee, eligible);
    }

    /// <summary>
    /// Whether the caller may read this review, given where its trainee trains (<paramref name="trainee" />, their
    /// preferred profile, <see cref="TraineeScopeResolver.ResolveAsync" />; null when they hold none, or when the caller's
    /// rung does not ask it) and who may sit on its panel (<paramref name="eligible" />, or <see cref="NotRead" /> where the
    /// seat cannot decide). Asked only through <see cref="DemandReviewAccessAsync" />, <see cref="ReadableReviewsAsync" />
    /// and <see cref="MayReadReviewAsync" />, which read both as the rungs need, so no caller can hand it a scope read
    /// some other way.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item><b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185): someone holding Trainee
    /// alongside an oversight role is still a trainee about their own record, and must not read a panel's working notes on
    /// someone else through the wider role. They read their own review once it is ratified, and nothing else.</item>
    /// <item>A global Administrator reads every review.</item>
    /// <item>Whoever works on the review (<see cref="WorksOnReview" />): the members of its panel who may sit at it now,
    /// and the coordinators of its institution. So whoever may start, chair or resolve an appeal on a review may read it
    /// (the T194 review: a member who had moved institution was admitted to Start and refused the review it had just
    /// started). Since T279 a member who may no longer sit at it (moved, no longer a committee member, deactivated, given
    /// Trainee, or the trainee under review) reads it through this rung no more, as they no longer start it.</item>
    /// <item>A role that schedules reviews, over the reviews it could have scheduled (<see cref="InSchedulingReach" />,
    /// T182's scope, T218).</item>
    /// </list>
    /// </remarks>
    private static bool MayReadReview(
        ClaimsPrincipal principal,
        CommitteeReview review,
        TraineeScope? trainee,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            return IsOwnVisibleReview(principal, review);
        }

        if (principal.IsAdministrator())
        {
            return true;
        }

        return WorksOnReview(principal, review, eligible) || InSchedulingReach(principal, review.Panel, trainee);
    }

    /// <summary>
    /// Whether a role that schedules reviews reaches a review on this panel of this trainee: T182's scope, read back over
    /// a review already scheduled. The panel must be at the caller's institution. There, an InstitutionalAdmin or a
    /// Coordinator reaches every review the panel holds; a SpecialityAdmin or SubSpecialityAdmin those whose trainee trains
    /// in their own speciality or sub-speciality at that institution (<see cref="TraineeScopeResolver.IsAdministeredBy" />,
    /// the role and its own scope claim asked together). (T218)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Until T218 a speciality or sub-speciality administrator read only the reviews of a panel they sat on. They schedule
    /// reviews of their own trainees (<see cref="CommitteeTraineeScope.MayScheduleFor" />), and the scheduling page then
    /// opened the review it had just created, which refused them. They already read those trainees' records
    /// (<see cref="TraineeScopeResolver.IsOverseenBy" />) and preview the agenda before scheduling, so the review is no
    /// wider a read.
    /// </para>
    /// <para>
    /// The institution-wide roles read by the panel's institution, as they did (T075, T101 finding E), not by where the
    /// trainee now trains: a coordinator supports the panels of their institution, including a review whose trainee has
    /// since moved away. A speciality administrator reads by the trainee, whose speciality is what they administer: once a
    /// trainee has moved to another institution, their reviews there are no longer in reach.
    /// </para>
    /// <para>
    /// <b>By the trainee's programme now, not the one they were reviewed in</b>, deliberately (the T218 review). When a
    /// trainee moves from Surgery to Paediatrics at the same institution, the Paediatrics administrator reaches every review
    /// of theirs on that institution's panels, the Surgery-era ones included, and the Surgery administrator who scheduled
    /// them reaches none. That is how every other read about the trainee already answers
    /// (<see cref="TraineeScopeResolver.MayReadAsync" />, their preferred profile): the Paediatrics administrator reads the
    /// trainee's progress, entrustment history and ratified reviews (<see cref="ListReviewsForTraineeQuery" />), and the
    /// Surgery administrator none of them, and scheduling the next review asks the same programme
    /// (<see cref="CommitteeTraineeScope.MayScheduleFor" />). Stamping the review with the programme it was scheduled in
    /// was rejected: it would make a review the one part of a trainee's record that stays with an administrator who no
    /// longer administers them, and hide it from the one who now must.
    /// </para>
    /// <para>
    /// The frozen evidence is not the per-activity read. <see cref="CommitteeReview.EvidenceItems" /> copies every activity
    /// of the trainee's in the review window at Start, whatever programme or institution it was stamped to (T185), and
    /// whoever reads the review reads those lines: a panel member and a coordinator as much as a speciality administrator.
    /// So a reader of the review may see a line for an activity whose own page, judged by its stamp, would refuse them.
    /// </para>
    /// <para>
    /// Never someone who holds Trainee: <see cref="MayReadReview" /> asks the trainee rung before this.
    /// </para>
    /// </remarks>
    private static bool InSchedulingReach(ClaimsPrincipal principal, DecisionPanel panel, TraineeScope? trainee)
    {
        if (!ReadsReviewsByInstitution(principal) || principal.GetInstitutionId() != panel.InstitutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin() || principal.IsInRole(WombatRoles.Coordinator))
        {
            return true;
        }

        return trainee is not null && TraineeScopeResolver.IsAdministeredBy(trainee, principal);
    }

    /// <summary>
    /// Whether <see cref="MayReadReview" /> reads where the review's trainee trains for this caller: only the speciality
    /// administrators' arm of <see cref="InSchedulingReach" /> does, and only for someone who neither holds Trainee nor is
    /// an Administrator, whose rungs answer first. For anyone else the answer is the same with no trainee, so no lookup is
    /// made. (T218)
    /// </summary>
    private static bool ReadsReviewsByTrainee(ClaimsPrincipal principal)
        => !TraineeScopeResolver.ActsAsTrainee(principal) &&
           !principal.IsAdministrator() &&
           (principal.IsInRole(WombatRoles.SpecialityAdmin) || principal.IsInRole(WombatRoles.SubSpecialityAdmin));

    /// <summary>
    /// Whether a rung of the read ladder can reach this caller through the panel's institution rather than a seat on the
    /// panel: an InstitutionalAdmin, a Coordinator (<see cref="WorksOnReview" /> and <see cref="InSchedulingReach" />), a
    /// SpecialityAdmin or a SubSpecialityAdmin (<see cref="InSchedulingReach" />, which asks it first). For anyone else,
    /// a committee member who holds none of them, only a seat admits, wherever the panel is. (T218 review)
    /// </summary>
    /// <remarks>
    /// The committee reviews list asks it so as to ask the store only for reviews the ladder could admit
    /// (<see cref="ListReviewsForPanelQuery" />): a committee member alone is sent their panels' reviews, not every review
    /// at their institution to judge one by one. It answers the role alone, so the Administrator's and the Trainee's rungs,
    /// which answer before it, are the caller's to ask.
    /// </remarks>
    internal static bool ReadsReviewsByInstitution(ClaimsPrincipal principal)
        => principal.IsInstitutionalAdmin() ||
           principal.IsInRole(WombatRoles.Coordinator) ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin);

    /// <summary>
    /// Whether this is the caller's own review in a state its trainee may see: ratified, under appeal or final. Before
    /// ratification a review is the panel's working record, not yet the trainee's.
    /// </summary>
    private static bool IsOwnVisibleReview(ClaimsPrincipal principal, CommitteeReview review)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) &&
               string.Equals(review.TraineeUserId, userId, StringComparison.Ordinal) &&
               review.State is CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final;
    }

    /// <summary>
    /// The one refusal for a review id the caller may not start, whether or not the id names a review. (T194 item 1)
    /// </summary>
    internal const string ReviewNotStartableByCaller =
        "The committee review could not be found among the reviews you can start.";

    /// <summary>
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> exists and the caller works on it now
    /// (<see cref="WorksOnReview" />): a member of its panel only while they may sit at it (T279). An unknown id, another
    /// panel's review and a review whose panel seats the caller but where they may not sit now get the one refusal; the
    /// review's state is judged only after this. The review page offers Start by the same predicate
    /// (<see cref="CommitteeReviewDetailDto.CallerMayStart" />). (T194 item 1, T279)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Starting freezes the trainee's evidence snapshot and the review's agenda, so a start by someone who no longer
    /// belongs on the panel decides what the panel will weigh. Who may sit is read from the user store, only for a caller
    /// who holds a seat on the panel and whom no arm before the seat's admits. Reads only: the start handler calls this
    /// before it changes anything, and the audit pipeline saves the request's context from its catch.
    /// </para>
    /// <para>
    /// The seat's holder who may not sit gets the one refusal, not a sentence of their own as the chair's holder is given
    /// (<see cref="DemandChairedReviewAsync" />): they may not read the review either, and the one refusal says nothing about
    /// the id.
    /// </para>
    /// <para>The review's <see cref="CommitteeReview.Panel" /> and its members must be loaded.</para>
    /// </remarks>
    public static async Task<CommitteeReview> DemandStartableReviewAsync(
        ClaimsPrincipal principal,
        CommitteeReview? review,
        IUserAdministrationService users,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(users);

        if (review is null)
        {
            throw new UnauthorizedAccessException(ReviewNotStartableByCaller);
        }

        var eligible = TheSeatMayDecide(principal, review.Panel)
            ? await PanelSeat.EligibleAsync(users, review.Panel.InstitutionId, cancellationToken)
            : NotRead;
        if (!WorksOnReview(principal, review, eligible))
        {
            throw new UnauthorizedAccessException(ReviewNotStartableByCaller);
        }

        return review;
    }

    /// <summary>
    /// True when the caller's institution claim names the panel's institution. Not
    /// <c>CanAccessInstitution</c>, which answers the narrower question of whether an
    /// InstitutionalAdmin owns an institution; a Coordinator is scoped by the same claim without
    /// being an admin of it.
    /// </summary>
    private static bool CoordinatesInstitution(ClaimsPrincipal principal, int institutionId)
        => principal.GetInstitutionId() == institutionId;

    /// <summary>
    /// The one refusal for a review id the caller may not conduct as its chair, whether or not the id names a review.
    /// (T131, T194 item 1)
    /// </summary>
    internal const string ReviewNotChairedByCaller =
        "The committee review could not be found among the reviews you chair.";

    /// <summary>
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> exists and the caller chairs it now
    /// (<see cref="Chairs" />): the gate of every chair's action, recording the decision, ratifying it, closing a formative
    /// review, staging and removing entrustment decisions, and deferring and reinstating agenda lines. An unknown id and a
    /// review of another panel get the one refusal; the Chair seat's holder who may not sit at the review now gets the
    /// seat's own (<see cref="PanelSeat.MayNotChairFromSeat" />). (T131, T165, T194 item 1, T256)
    /// </summary>
    /// <returns>
    /// The review, and who may sit on its panel now (<see cref="PanelSeat.EligibleAsync" />), read once here for a handler
    /// that holds attendance to the same rule (recording the decision).
    /// </returns>
    /// <remarks>
    /// <para>
    /// The page prints a command's refusal. Before T131 the entrustment commands said "could not be found" for an unknown
    /// id and only then checked the formative flag, the review's state and the chair, each with its own message, so a
    /// committee member of one panel could try review ids and learn which exist, which are formative and how far each
    /// has got (T194 item 1). Recording and closing kept that shape until T194. The state checks now come after this one.
    /// </para>
    /// <para>
    /// There is no Administrator bypass (T165, D46). Until T165 a global Administrator who was not on the panel passed the
    /// chair's check, so one person with no seat on the committee could record, ratify and issue a STAR end to end, and
    /// the record could not show that anyone else was involved. An Administrator keeps panel administration and read
    /// access; one who must act joins the panel first, which the panel's own record then shows. Joining takes what any
    /// member's seat takes (<see cref="PanelSeat" />): the CommitteeMember role, at the panel's institution. So a review
    /// stranded by a trainee's move is finished by seating an active committee member of the panel's institution as its
    /// chair, not by an Administrator acting in their own right. There used to be two copies of the check, one here and
    /// one in <c>EntrustmentDecisionAuthorization</c>, each with the bypass; this is the only one now.
    /// </para>
    /// <para>
    /// Two steps, in this order. The seat, from the caller's claims, gives the one refusal and reads nothing: so an id's
    /// existence is never confirmed to someone who holds no Chair seat on its panel. Then acting from the seat now, read
    /// from the user store (T256): the Chair seat's holder who may read the review is told that the seat is theirs but
    /// they may not act from it, which tells them nothing they cannot read on the review page. Since T279 a chair who may
    /// not sit at the review reads it only through another rung of the ladder (a coordinator of its institution, say), so
    /// one who reads it through none gets the one refusal: told the seat's sentence, they would learn that the id names a
    /// review of their panel. Reads only: every handler calls this before it changes anything, and the audit pipeline
    /// saves the request's context from its catch.
    /// </para>
    /// <para>The review's <see cref="CommitteeReview.Panel" /> and its members must be loaded.</para>
    /// </remarks>
    public static async Task<(CommitteeReview Review, IReadOnlyDictionary<string, UserIdentityDetails> Eligible)> DemandChairedReviewAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CommitteeReview? review,
        IUserAdministrationService users,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(users);

        if (review is null || !HoldsChairSeat(principal, review.Panel))
        {
            throw new UnauthorizedAccessException(ReviewNotChairedByCaller);
        }

        var eligible = await PanelSeat.EligibleAsync(users, review.Panel.InstitutionId, cancellationToken);
        if (!Chairs(principal, review, eligible))
        {
            throw new UnauthorizedAccessException(
                await MayReadReviewAsync(dbContext, principal, review, eligible, cancellationToken)
                    ? PanelSeat.MayNotChairFromSeat
                    : ReviewNotChairedByCaller);
        }

        return (review, eligible);
    }

    /// <summary>
    /// Refuses a caller who holds a seat on the review's appeal body (<see cref="DemandAppealBodyReview" />, asked first)
    /// but may not resolve its appeal from it now (<see cref="PanelSeat.AppealBodyAt" />, T237). Their refusal is the
    /// seat's own sentence (<see cref="PanelSeat.MayNotResolveFromSeat" />) only if they may read the review; otherwise
    /// the one refusal, which says nothing about the id (T279, as <see cref="DemandChairedReviewAsync" /> answers the
    /// chair). Reads only: the resolve handler calls it before it changes anything.
    /// </summary>
    /// <param name="eligible">Who may sit on the review's panel now (<see cref="PanelSeat.EligibleAsync" />).</param>
    public static async Task DemandResolvesFromSeatAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CommitteeReview review,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(eligible);

        if (PanelSeat.SitsOnAppealBody(review, GetRequiredUserId(principal), eligible))
        {
            return;
        }

        throw new UnauthorizedAccessException(
            await MayReadReviewAsync(dbContext, principal, review, eligible, cancellationToken)
                ? PanelSeat.MayNotResolveFromSeat
                : ReviewNotResolvableByCaller);
    }

    /// <summary>
    /// Whether the caller chairs this review now: they hold its panel's Chair seat, and may sit at the review now
    /// (<see cref="PanelSeat.SittingAt" />, T237's one rule). The one predicate every chair's action demands
    /// (<see cref="DemandChairedReviewAsync" />) and the review page's offer of those actions reads
    /// (<see cref="CommitteeReviewDetailDto.CallerChairs" />), so the page offers the chair's controls to exactly the
    /// people the handlers let use them. (T165, T213, T256)
    /// </summary>
    /// <param name="eligible">Who may sit on the review's panel now (<see cref="PanelSeat.EligibleAsync" />).</param>
    /// <remarks>
    /// <para>
    /// The seat alone reads the caller's claims, which cannot say whether they are still an active committee member at
    /// the panel's institution: a circuit's claims are frozen for its life. Until T256 a chair who had lost the
    /// CommitteeMember role, moved to another institution or been deactivated kept every chair's action that records no
    /// attendance: ratifying, which issues the staged STARs, closing a formative review, staging and removing STARs, and
    /// deferring and reinstating agenda lines. Recording refused them only because it records the chair as present, and
    /// the appeal body since T237 (<see cref="PanelSeat.AppealBodyAt" />). So D46's "an active CommitteeMember at the
    /// panel's institution" now holds for every chair's action, by the rule the panel form seats people by.
    /// </para>
    /// <para>
    /// It rules out the trainee under review too, as <see cref="PanelSeat.SittingAt" /> does: a trainee who has lost the
    /// Trainee role and chairs the panel that reviews them never acts on their own review.
    /// </para>
    /// </remarks>
    public static bool Chairs(
        ClaimsPrincipal principal,
        CommitteeReview review,
        IReadOnlyDictionary<string, UserIdentityDetails> eligible)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(eligible);

        if (!HoldsChairSeat(principal, review.Panel))
        {
            return false;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return PanelSeat.SittingAt(review, eligible).Any(member =>
            member.Role == DecisionPanelMemberRole.Chair &&
            string.Equals(member.UserId, userId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Whether the caller holds this panel's Chair seat, from their claims: the first half of <see cref="Chairs" />, which
    /// the gate asks before it reads the user store, and the review page asks to know whom to tell that they may not act
    /// from it (<see cref="CommitteeReviewDetailDto.ChairCannotAct" />).
    /// </summary>
    public static bool HoldsChairSeat(ClaimsPrincipal principal, DecisionPanel panel)
        => HoldsSeat(principal, panel, role => role == DecisionPanelMemberRole.Chair);

    /// <summary>
    /// Whether the caller sits on this panel's appeal body, as its Chair or one of its External members: the predicate
    /// <see cref="DemandAppealBodyReview" /> demands and the review page's offer of the resolve form reads
    /// (<see cref="CommitteeReviewDetailDto.CallerResolvesAppeals" />). (T165, T213)
    /// </summary>
    /// <remarks>
    /// The seat, from the caller's claims. Acting from it also takes being on the appeal body that can act now, read from
    /// the user store (<see cref="PanelSeat.AppealBodyAt" />, T237), which the resolve handler demands next and the page's
    /// offer reads too: a Chair or External member who has lost the CommitteeMember role, moved away or been deactivated
    /// holds the seat but resolves nothing.
    /// </remarks>
    public static bool ResolvesAppeals(ClaimsPrincipal principal, DecisionPanel panel)
        => HoldsSeat(principal, panel, role => role is DecisionPanelMemberRole.Chair or DecisionPanelMemberRole.External);

    /// <summary>
    /// Whether the caller holds a seat of this kind on the panel and acts in it: never someone who holds Trainee.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Nobody who holds Trainee acts on a review from a seat, whichever seat it is: the trainee rung
    /// (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185) is asked first, as the read ladder and Start ask it
    /// (<see cref="WorksOnReview" />). Until the T194 review it was not: a Trainee seated as the Chair could record, ratify
    /// and stage on a peer's review the page refused them, and one seated as the Chair or an External member of the panel
    /// that reviews them was offered, and could use, the resolve form on their own appeal, since the page shows a trainee
    /// their own review under appeal: they could dismiss it, or remit it and write the replacement decision with two
    /// others named present.
    /// </para>
    /// <para>
    /// Since T237 nobody who holds Trainee is seated either (<see cref="PanelSeat" />, the same rung read from the user
    /// store): the panel form does not offer them, and panel create and update refuse them in any seat. The rung is still
    /// asked here for a member given Trainee after they were seated, whom the panel's next save takes off: until then a
    /// panel whose Chair holds Trainee has no one who can take the chair's actions, and its review page says so
    /// (<see cref="CommitteeReviewDetailDto.ChairCannotAct" />, T256).
    /// </para>
    /// <para>
    /// The seat, from claims, is only half of acting from it: the chair's actions and the appeal body also demand that the
    /// caller may sit at the review now, read from the user store (<see cref="Chairs" />, T256;
    /// <see cref="DemandResolvesFromSeatAsync" />, T237). Starting a review and reading it through a seat demand the same
    /// (<see cref="WorksOnReview" />, T279).
    /// </para>
    /// </remarks>
    private static bool HoldsSeat(ClaimsPrincipal principal, DecisionPanel panel, Func<DecisionPanelMemberRole, bool> role)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            return false;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) &&
               panel.Members.Any(member =>
                   string.Equals(member.UserId, userId, StringComparison.Ordinal) && role(member.Role));
    }

    /// <summary>
    /// The one refusal for a review id whose appeal the caller may not resolve, whether or not the id names a review.
    /// (T194 item 1)
    /// </summary>
    internal const string ReviewNotResolvableByCaller =
        "The committee review could not be found among the reviews whose appeals you resolve.";

    /// <summary>
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> exists and the caller sits on its
    /// panel's appeal body: the panel's Chair or one of its External members. An unknown id and another panel's review get
    /// the one refusal; whether the review is under appeal is judged only after this. (T165, D46, T194 item 1)
    /// </summary>
    /// <remarks>
    /// There is no Administrator bypass, as there is none on the chair's actions (<see cref="DemandChairedReviewAsync" />).
    /// Until T165 an Administrator with no seat on the panel could dismiss a trainee's appeal, or remit it and write the
    /// replacement decision alone, which every page then showed beside the original sitting's attendance. The review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded. The resolve handler then demands that the
    /// caller may act from the seat now (<see cref="DemandResolvesFromSeatAsync" />, T237).
    /// </remarks>
    public static CommitteeReview DemandAppealBodyReview(ClaimsPrincipal principal, CommitteeReview? review)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (review is null || !ResolvesAppeals(principal, review.Panel))
        {
            throw new UnauthorizedAccessException(ReviewNotResolvableByCaller);
        }

        return review;
    }

    /// <summary>The refusal to lodge an appeal for anyone who does not hold the Trainee role. Given before any lookup.</summary>
    internal const string OnlyTraineesAppeal = "Only trainees can lodge appeals.";

    /// <summary>
    /// The one refusal for a review id a trainee may not appeal, whether or not the id names a review: someone else's, or
    /// their own before it is ratified, which they cannot see yet. (T194 item 1)
    /// </summary>
    internal const string ReviewNotAppealableByCaller =
        "The committee review could not be found among your own ratified reviews.";

    /// <summary>
    /// Refuses anyone who does not hold the Trainee role: an appeal is the trainee's own. Asked before the review is
    /// looked up, so it says nothing about the id. (T165)
    /// </summary>
    /// <remarks>
    /// There is no Administrator bypass. Until T165 an Administrator could lodge an appeal on a trainee's behalf, which
    /// with the appeal body's own bypass let one person reopen and replace a ratified committee decision end to end.
    /// </remarks>
    public static void DemandLodgesAppeals(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (!principal.IsInRole(WombatRoles.Trainee))
        {
            throw new UnauthorizedAccessException(OnlyTraineesAppeal);
        }
    }

    /// <summary>
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> is the caller's own and in a state its
    /// trainee may see, the read ladder's trainee arm (<see cref="MayReadReview" />). An unknown id, another trainee's
    /// review, and the caller's own review before it is ratified get the one refusal. (T165, T194 item 1)
    /// </summary>
    /// <remarks>
    /// Until T194 an unknown id was "could not be found", another trainee's review "You can only appeal your own committee
    /// reviews", and the caller's own scheduled review "Only ratified reviews can be appealed", which told a trainee that a
    /// review of theirs was under way before the panel had said anything. Whether a review they can see takes an appeal
    /// (one is already open, or it is final) is the review's own rule, judged after this.
    /// </remarks>
    public static CommitteeReview DemandOwnRatifiedReview(ClaimsPrincipal principal, CommitteeReview? review)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (review is null || !IsOwnVisibleReview(principal, review))
        {
            throw new UnauthorizedAccessException(ReviewNotAppealableByCaller);
        }

        return review;
    }
}
