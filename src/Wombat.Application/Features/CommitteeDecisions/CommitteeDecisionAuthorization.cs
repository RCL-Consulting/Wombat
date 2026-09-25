using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

internal static class CommitteeDecisionAuthorization
{
    public static string GetRequiredUserId(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("The current user identifier is missing.");

    public static void DemandPanelAdministration(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not allowed to manage committee panels.");
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
    /// The speciality half reads the SubSpecialityAdmin's sub-specialities' specialities from the store, so it is asked
    /// once per request, not once per speciality.
    /// </remarks>
    public static async Task<PanelAdministrationReach> PanelReachAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);

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
    /// Stricter than <see cref="MayAdministerPanelAsync" />, which lets a SpecialityAdmin manage their own speciality's
    /// panel. A body tag takes that body's EPAs away from every other panel covering the trainee: a panel carrying
    /// <c>neonatal</c> that covers the trainee's speciality comes before the institution's own. Which committee decides an
    /// EPA is the institution's arrangement, so it is the institution's administrator's to make.
    /// </remarks>
    public static bool MaySetDecisionBody(ClaimsPrincipal principal, int institutionId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.IsAdministrator() ||
               (principal.IsInstitutionalAdmin() && principal.GetInstitutionId() == institutionId);
    }

    /// <summary>Whether this caller holds a role that could set a panel's decision body anywhere: the pre-lookup check.</summary>
    public static bool HoldsDecisionBodyRole(ClaimsPrincipal principal)
        => principal.IsAdministrator() || principal.IsInstitutionalAdmin();

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
    /// Whether the caller works on this panel's reviews: a global Administrator, a Coordinator of the panel's institution,
    /// or a member of the panel, and in every case not someone who holds Trainee. Who may start a review
    /// (<see cref="DemandStartableReview" />), and the read ladder's last rung (<see cref="MayReadReview" />).
    /// </summary>
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
    /// </remarks>
    public static bool WorksOnPanel(ClaimsPrincipal principal, DecisionPanel panel)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            return false;
        }

        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return true;
        }

        if (principal.IsInRole(WombatRoles.Coordinator) && CoordinatesInstitution(principal, panel.InstitutionId))
        {
            return true;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) &&
               panel.Members.Any(member => string.Equals(member.UserId, userId, StringComparison.Ordinal));
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
    /// </remarks>
    public static CommitteeReview DemandReviewAccess(ClaimsPrincipal principal, CommitteeReview? review)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (review is null || !MayReadReview(principal, review))
        {
            throw new UnauthorizedAccessException(ReviewNotReadableByCaller);
        }

        return review;
    }

    /// <summary>
    /// Whether the caller may read this review: the rule <see cref="DemandReviewAccess" /> demands, and the decisions-due
    /// page's links read so it links only to a review that will open. The review's <see cref="CommitteeReview.Panel" />
    /// and its members must be loaded.
    /// </summary>
    public static bool MayReadReview(ClaimsPrincipal principal, CommitteeReview review)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(review);

        // The trainee arm comes first deliberately: someone holding Trainee alongside an oversight role is still a trainee
        // about their own record, and must not read a panel's working notes on someone else through the wider role.
        if (principal.IsInRole(WombatRoles.Trainee))
        {
            return IsOwnVisibleReview(principal, review);
        }

        if (principal.IsAdministrator())
        {
            return true;
        }

        if (principal.IsInstitutionalAdmin())
        {
            // An InstitutionalAdmin can view (read-only) any review for a panel in their institution, even without panel
            // membership. Conduct actions (start/record/ratify) keep their own gates. (T075 / F-4A-1)
            //
            // And, as anyone else, the reviews of a panel they work on (T194 review): one who holds a seat on another
            // institution's panel, which happens only when a seated member moves institution (PanelSeat refuses such a
            // seat on save), was admitted to Start by WorksOnPanel and refused the review it had just started. So whoever
            // may start, chair or resolve an appeal on a review may read it.
            return principal.CanAccessInstitution(review.Panel.InstitutionId) || WorksOnPanel(principal, review.Panel);
        }

        return WorksOnPanel(principal, review.Panel);
    }

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
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> exists and the caller works on its
    /// panel (<see cref="WorksOnPanel" />). An unknown id and another panel's review get the one refusal; the review's
    /// state is judged only after this. The review page offers Start by the same predicate
    /// (<see cref="CommitteeReviewDetailDto.CallerMayStart" />). (T194 item 1)
    /// </summary>
    /// <remarks>The review's <see cref="CommitteeReview.Panel" /> and its members must be loaded.</remarks>
    public static CommitteeReview DemandStartableReview(ClaimsPrincipal principal, CommitteeReview? review)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (review is null || !WorksOnPanel(principal, review.Panel))
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
    /// Refuses, before anything else is looked at, unless <paramref name="review" /> exists and the caller chairs its
    /// panel: the gate of every chair's action, recording the decision, ratifying it, closing a formative review, staging
    /// and removing entrustment decisions, and deferring and reinstating agenda lines. An unknown id and a review of
    /// another panel get the one refusal. (T131, T165, T194 item 1)
    /// </summary>
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
    /// <para>The review's <see cref="CommitteeReview.Panel" /> and its members must be loaded.</para>
    /// </remarks>
    public static CommitteeReview DemandChairedReview(ClaimsPrincipal principal, CommitteeReview? review)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (review is null || !Chairs(principal, review.Panel))
        {
            throw new UnauthorizedAccessException(ReviewNotChairedByCaller);
        }

        return review;
    }

    /// <summary>
    /// Whether the caller holds this panel's Chair seat: the one predicate every chair's action demands
    /// (<see cref="DemandChairedReview" />) and the review page's offer of those actions reads
    /// (<see cref="CommitteeReviewDetailDto.CallerChairs" />), so the page offers the chair's controls to exactly the
    /// people the handlers let use them. (T165, T213)
    /// </summary>
    public static bool Chairs(ClaimsPrincipal principal, DecisionPanel panel)
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
    /// (<see cref="WorksOnPanel" />). Until the T194 review it was not: a Trainee seated as the Chair could record, ratify
    /// and stage on a peer's review the page refused them, and one seated as the Chair or an External member of the panel
    /// that reviews them was offered, and could use, the resolve form on their own appeal, since the page shows a trainee
    /// their own review under appeal: they could dismiss it, or remit it and write the replacement decision with two
    /// others named present.
    /// </para>
    /// <para>
    /// Since T237 nobody who holds Trainee is seated either (<see cref="PanelSeat" />, the same rung read from the user
    /// store): the panel form does not offer them, and panel create and update refuse them in any seat. The rung is still
    /// asked here for a member given Trainee after they were seated, whom the panel's next save takes off: until then a
    /// panel whose Chair holds Trainee has no one who can take the chair's actions, and its review page says the chair
    /// cannot be recorded as present (<see cref="CommitteeReviewDetailDto.PanelShortfall" />).
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
    /// There is no Administrator bypass, as there is none on the chair's actions (<see cref="DemandChairedReview" />).
    /// Until T165 an Administrator with no seat on the panel could dismiss a trainee's appeal, or remit it and write the
    /// replacement decision alone, which every page then showed beside the original sitting's attendance. The review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded. The resolve handler then demands that the
    /// caller may act from the seat now (<see cref="PanelSeat.DemandSitsOnAppealBody" />, T237).
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
