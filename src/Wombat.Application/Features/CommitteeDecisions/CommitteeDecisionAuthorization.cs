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
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return true;
        }

        if (principal.GetInstitutionId() != institutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin())
        {
            return true;
        }

        if (scope != DecisionPanelScope.Speciality || specialityId is not int speciality)
        {
            return false;
        }

        if (principal.IsInRole(WombatRoles.SpecialityAdmin) && principal.IsInSpeciality(speciality))
        {
            return true;
        }

        if (!principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return false;
        }

        var subSpecialityIds = principal.GetSubSpecialityIds().ToArray();
        return subSpecialityIds.Length > 0 &&
               await dbContext.Set<SubSpeciality>()
                   .AsNoTracking()
                   .AnyAsync(
                       subSpeciality => subSpecialityIds.Contains(subSpeciality.Id) && subSpeciality.SpecialityId == speciality,
                       cancellationToken);
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

    public static void DemandPanelAccess(ClaimsPrincipal principal, DecisionPanel panel)
    {
        if (principal.IsInRole(WombatRoles.Administrator))
        {
            return;
        }

        // A Coordinator supports the programmes of ONE institution, so they reach the panels their
        // own institution runs — not every institution's. The waiver used to be role-only, which
        // made any Coordinator anywhere a reader of every panel's reviews and, through the queries
        // that hang off a review, of every trainee's evidence. (T101 finding E)
        if (principal.IsInRole(WombatRoles.Coordinator) && CoordinatesInstitution(principal, panel.InstitutionId))
        {
            return;
        }

        var userId = GetRequiredUserId(principal);
        if (panel.Members.Any(member => string.Equals(member.UserId, userId, StringComparison.Ordinal)))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not a member of this decision panel.");
    }

    /// <summary>
    /// The one read ladder for a committee review and for everything computed from it.
    /// </summary>
    /// <remarks>
    /// ReviewDetail.razor is fed by sibling queries — the review itself, the sampling
    /// concentration report, and the entrustment decisions staged against it — and each used to
    /// carry its own idea of who may read a review, or none at all. One ladder, called by all
    /// of them, is the only arrangement in which they cannot drift apart again; the count of the
    /// window's MSF campaigns missing from the snapshot climbs it too (T173). The review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded. (T101 finding E)
    /// </remarks>
    public static void DemandReviewAccess(ClaimsPrincipal principal, CommitteeReview review)
    {
        // The trainee arm comes first deliberately: someone holding Trainee alongside an oversight
        // role is still a trainee about their own record, and must not read a panel's working notes
        // on someone else through the wider role.
        if (principal.IsInRole(WombatRoles.Trainee))
        {
            var userId = GetRequiredUserId(principal);
            if (!string.Equals(review.TraineeUserId, userId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You can only view your own committee reviews.");
            }

            if (review.State is not (CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final))
            {
                throw new UnauthorizedAccessException("This review is not yet visible to the trainee.");
            }

            return;
        }

        if (principal.IsAdministrator())
        {
            return;
        }

        if (principal.IsInstitutionalAdmin())
        {
            // An InstitutionalAdmin can view (read-only) any review for a panel in their
            // institution, even without panel membership. Conduct actions (start/record/
            // ratify) remain chair-gated in their respective handlers. (T075 / F-4A-1)
            if (!principal.CanAccessInstitution(review.Panel.InstitutionId))
            {
                throw new UnauthorizedAccessException("You can only view committee reviews for panels in your institution.");
            }

            return;
        }

        DemandPanelAccess(principal, review.Panel);
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
    /// panel. An unknown id and a review of another panel get the one refusal. (T131, T165)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The page prints a command's refusal. Before T131 the entrustment commands said "could not be found" for an unknown
    /// id and only then checked the formative flag, the review's state and the chair, each with its own message, so a
    /// committee member of one panel could try review ids and learn which exist, which are formative and how far each
    /// has got (T194 item 1). The state checks now come after this one.
    /// </para>
    /// <para>
    /// There is no Administrator bypass, as there is none on <see cref="DemandChairAccess" /> (T165, D46): a global
    /// Administrator without a Chair seat on the review's panel gets the same one refusal as anyone else. The review's
    /// <see cref="CommitteeReview.Panel" /> and its members must be loaded.
    /// </para>
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
    /// (<see cref="DemandChairAccess" />, <see cref="DemandChairedReview" />) and the review page's offer of those actions
    /// reads (<see cref="CommitteeReviewDetailDto.CallerChairs" />), so the page offers the chair's controls to exactly the
    /// people the handlers let use them. (T165, T213)
    /// </summary>
    public static bool Chairs(ClaimsPrincipal principal, DecisionPanel panel)
        => HoldsSeat(principal, panel, role => role == DecisionPanelMemberRole.Chair);

    /// <summary>
    /// Whether the caller sits on this panel's appeal body, as its Chair or one of its External members: the predicate
    /// <see cref="DemandAppealResolverAccess" /> demands and the review page's offer of the resolve form reads
    /// (<see cref="CommitteeReviewDetailDto.CallerResolvesAppeals" />). (T165, T213)
    /// </summary>
    public static bool ResolvesAppeals(ClaimsPrincipal principal, DecisionPanel panel)
        => HoldsSeat(principal, panel, role => role is DecisionPanelMemberRole.Chair or DecisionPanelMemberRole.External);

    private static bool HoldsSeat(ClaimsPrincipal principal, DecisionPanel panel, Func<DecisionPanelMemberRole, bool> role)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(userId) &&
               panel.Members.Any(member =>
                   string.Equals(member.UserId, userId, StringComparison.Ordinal) && role(member.Role));
    }

    /// <summary>The refusal of a chair's action to anyone who is not the panel's chair.</summary>
    internal const string OnlyTheChair = "Only the panel's chair can do this.";

    /// <summary>
    /// Refuses anyone who is not a Chair of this panel: the gate for the actions that take a committee decision.
    /// Recording the decision and closing a formative review call it; ratifying, and staging and removing entrustment
    /// decisions, hold the same rule through <see cref="DemandChairedReview" />, which also gives an unknown review id the
    /// one refusal (T131). (T165, D46)
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is no Administrator bypass. Until T165 a global Administrator who was not on the panel passed this check,
    /// so one person with no seat on the committee could record, ratify and issue a STAR end to end, and the record
    /// could not show that anyone else was involved. An Administrator keeps panel administration and read access; one
    /// who must act joins the panel first, which the panel's own record then shows. Joining takes what any member's seat
    /// takes (<see cref="PanelSeat" />): the CommitteeMember role, at the panel's institution. So a review stranded by a
    /// trainee's move is finished by seating an active committee member of the panel's institution as its chair, not by
    /// an Administrator acting in their own right.
    /// </para>
    /// <para>
    /// There used to be two copies of this check, this one and one in <c>EntrustmentDecisionAuthorization</c>, each
    /// with the bypass. This is the only one now.
    /// </para>
    /// </remarks>
    public static void DemandChairAccess(ClaimsPrincipal principal, DecisionPanel panel)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        GetRequiredUserId(principal);
        if (Chairs(principal, panel))
        {
            return;
        }

        throw new UnauthorizedAccessException(OnlyTheChair);
    }

    /// <summary>
    /// Refuses anyone who is not the appeal body: the panel's Chair or one of its External members. (T165, D46)
    /// </summary>
    /// <remarks>
    /// There is no Administrator bypass, as there is none on the chair's actions (<see cref="DemandChairAccess" />).
    /// Until T165 an Administrator with no seat on the panel could dismiss a trainee's appeal, or remit it and write the
    /// replacement decision alone, which every page then showed beside the original sitting's attendance.
    /// </remarks>
    public static void DemandAppealResolverAccess(ClaimsPrincipal principal, DecisionPanel panel)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(panel);

        GetRequiredUserId(principal);
        if (ResolvesAppeals(principal, panel))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only the appeal body can resolve committee appeals.");
    }

    /// <summary>
    /// Refuses anyone but the trainee whose review it is: an appeal is the trainee's own. (T165)
    /// </summary>
    /// <remarks>
    /// There is no Administrator bypass. Until T165 an Administrator could lodge an appeal on a trainee's behalf, which
    /// with the appeal body's own bypass let one person reopen and replace a ratified committee decision end to end.
    /// </remarks>
    public static void DemandTraineeSelfAccess(ClaimsPrincipal principal, string traineeUserId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (!principal.IsInRole(WombatRoles.Trainee))
        {
            throw new UnauthorizedAccessException("Only trainees can lodge appeals.");
        }

        var userId = GetRequiredUserId(principal);
        if (!string.Equals(userId, traineeUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("You can only appeal your own committee reviews.");
        }
    }
}
