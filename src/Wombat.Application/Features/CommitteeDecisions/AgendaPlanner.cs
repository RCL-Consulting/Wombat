using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>What a sitting's agenda would hold. Built by <see cref="AgendaPlanner" />; nothing in it is tracked.</summary>
/// <param name="TraineeHasCurriculum">False when the trainee holds no profile, so nothing can be planned.</param>
/// <param name="Lines">The cadence lines the review's panel decides at this sitting, in code order. Unattached.</param>
/// <param name="RoutedElsewhere">The EPAs due in the period that a panel sitting as a College committee decides instead.</param>
/// <param name="DecidedInWindow">
/// The EPAs this panel decides, due in the period, that a STAR already decided in their window, whether or not an agenda
/// line records it (<see cref="CommitteeAgendaStatus.IsDecided" />, T215), in code order: not planned.
/// </param>
/// <param name="PanelDecidesAnything">
/// Whether any of the trainee's in-force curriculum items routes to this panel, with a cadence or without, due or not: whether
/// a sitting before it could decide anything at all. An entrustment-only review is not scheduled where it could not (T131
/// slice 5), since its decision is what its agenda holds.
/// </param>
internal sealed record AgendaPlan(
    bool TraineeHasCurriculum,
    IReadOnlyList<CommitteeAgendaLine> Lines,
    IReadOnlyList<CommitteeAgendaElsewhereDto> RoutedElsewhere,
    IReadOnlyList<DecidedEpa> DecidedInWindow,
    bool PanelDecidesAnything)
{
    public static readonly AgendaPlan Empty = new(false, [], [], [], false);

    /// <summary>The plan for a panel the trainee is no longer eligible for: nothing, and nothing about anyone else.</summary>
    public static readonly AgendaPlan Stranded = new(true, [], [], [], false);

    /// <summary>
    /// Of <see cref="Lines" />, the EPAs whose window was decided and has lost that decision
    /// (<see cref="CommitteeAgendaStatus.HasLostItsDecision" />), in code order: what a review in progress names when its
    /// agenda does not hold one (T235).
    /// </summary>
    public IReadOnlyList<CommitteeAgendaEpaDto> NoLongerDecided { get; init; } = [];

    /// <summary>
    /// The EPAs of the review's own lines still due whose window another sitting has decided
    /// (<see cref="CommitteeAgendaStatus.IsDecided" /> on the line's own window): optional, and never blocking ratify (T235).
    /// Filled only by <see cref="AgendaPlanner.PlanForReviewAsync" />; empty for a sitting not yet scheduled.
    /// </summary>
    public IReadOnlySet<int> DecidedElsewhere { get; init; } = new HashSet<int>();
}

/// <summary>An EPA the planner left off because a STAR already decided its window. (T215)</summary>
internal sealed record DecidedEpa(int EpaId, string Code);

/// <summary>
/// Which EPAs a committee sitting is there to decide: the one planner the schedule preview, scheduling and Start share,
/// so the preview is the agenda (picker = gate). (T131 slice 4, Decisions 3-5 and 9)
/// </summary>
/// <remarks>
/// <para>
/// <b>The items</b> are the trainee's in-force curriculum items, the national core and their own institution's local
/// items (<see cref="StarCurriculum.AdmittedItems" />, the predicate a STAR is granted under), that have a decision
/// cadence. An item with none is never due, though a chair can still decide it (<see cref="ChairLineAsync" />).
/// </para>
/// <para>
/// <b>The window</b> is the item's cadence window holding the sitting's period (<see cref="QuotaWindow.For" />, read on
/// the period's last day): the period itself for a semester EPA, its whole year for an annual one. No other calendar is
/// read. A window the trainee had not started by its end is not due; one they joined part-way through is due, labelled,
/// and optional (<see cref="CommitteeAgendaLine.ClosesAt" />).
/// </para>
/// <para>
/// <b>The panel</b> is <see cref="DecisionRouting" />'s: an item comes onto this panel's agenda only when it routes to
/// this panel for this trainee, so a general review leaves EPAs 4 and 5 off wherever a neonatal panel covers the trainee.
/// An item whose window is already decided is skipped, and named (<see cref="AgendaPlan.DecidedInWindow" />). What
/// another panel sitting as a College committee owes in the period is reported beside the agenda, read-only, with its
/// standing in the window.
/// </para>
/// <para>
/// <b>A review its trainee has left.</b> A panel plans, and reports, nothing for a trainee it is not eligible for
/// (<see cref="DecisionRouting.IsEligible" />): a trainee who has moved to another institution strands their open review
/// (T182), and its page must not show the new institution's panels or where its decisions stand. So such a review's own
/// lines are not read against the window rule either (T235). A speciality panel at the trainee's own institution that no
/// longer covers their speciality plans nothing more too, but its lines are still read, since nothing there is another
/// institution's. With no profile at all, where the trainee trains is unknown, and nothing is read.
/// </para>
/// <para>
/// <b>What another sitting already did.</b> A decision is the trainee's and goes with them. A window is decided by a STAR
/// from a sitting for a period it holds that still decides it (<see cref="CommitteeAgendaStatus.StarDecides" />), whether
/// or not an agenda line records it: <see cref="CommitteeAgendaStatus.IsDecided" />, the one predicate the decisions-due
/// page reads too (T131 slice 6, T215), so an EPA the page calls decided is never planned as due. Agenda lines are the
/// record of what a sitting did; the STAR is the fact, and one ratified before agendas existed has no line. A revoked
/// STAR decides nothing, so its EPA is planned again, and nor does one superseded inside the window by a STAR since
/// revoked; an expired one decided its window (Decision 10). The records are the page's
/// (<see cref="DecisionWindowRecords" />). A sitting is its institution's: a line still due, or deferred, on a review
/// before a panel at another institution (a review the trainee stranded when they moved) says nothing about where the
/// decision stands here.
/// </para>
/// <para>
/// <b>What changes while a review is open</b> (T235). An agenda is planned at schedule and at Start, and a window can be
/// decided, or lose its decision, while the review is open. Each line the review still holds due is read against the same
/// predicate, on its own window, whenever it is judged (<see cref="PlanForReviewAsync" />: the agenda read, recording the
/// decision, ratify, and deferring after the recording): one another sitting has decided is optional, and never blocks
/// ratify. The other way round, an EPA Start left off because a STAR decided its window, whose STAR has since been revoked,
/// is named beside the agenda (<see cref="AgendaPlan.NoLongerDecided" />) and is not added to it. Adding it would change,
/// under the chair, the agenda Start planned and the decision recorded at this sitting settles (D46); staging a decision
/// on it adds the chair's line, as for any EPA that routes here, and otherwise the decisions-due page reads it as revoked
/// and the next sitting plans it.
/// </para>
/// <para>
/// Reads only. Every caller runs it before its first mutation.
/// </para>
/// </remarks>
internal static class AgendaPlanner
{
    /// <summary>What a sitting of <paramref name="panel" /> for <paramref name="sitting" /> would put on its agenda.</summary>
    /// <param name="today">The programme's today (<see cref="ProgrammeCalendar.DateOf" />): only "missed" reads it.</param>
    public static Task<AgendaPlan> PlanAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        DecisionPanel panel,
        AcademicPeriod sitting,
        DateOnly today,
        CancellationToken cancellationToken)
        => PlanAsync(dbContext, traineeUserId, panel, sitting, today, held: [], cancellationToken);

    /// <summary>
    /// The plan for the review's own sitting, with each line it still holds due read against the window rule
    /// (<see cref="AgendaPlan.DecidedElsewhere" />, T235): the one read the agenda, and the record, ratify and defer
    /// handlers share, so the page's disabled Record and Ratify say what the handlers refuse. Nothing for a formative
    /// review, which carries no agenda. Reads only.
    /// </summary>
    /// <param name="review">The review, with its <see cref="CommitteeReview.Panel" /> and agenda lines loaded.</param>
    /// <param name="today">The programme's today: only "missed" reads it.</param>
    public static Task<AgendaPlan> PlanForReviewAsync(
        IApplicationDbContext dbContext,
        CommitteeReview review,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(review);

        return review.IsFormative
            ? Task.FromResult(AgendaPlan.Empty)
            : PlanAsync(dbContext, review.TraineeUserId, review.Panel, review.Period, today, review.AgendaLines.ToArray(), cancellationToken);
    }

    /// <param name="held">The lines the review already holds, each read against the window rule (T235); none for a preview.</param>
    private static async Task<AgendaPlan> PlanAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        DecisionPanel panel,
        AcademicPeriod sitting,
        DateOnly today,
        IReadOnlyCollection<CommitteeAgendaLine> held,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(panel);

        var context = await LoadContextAsync(dbContext, traineeUserId, cancellationToken);
        if (context is null)
        {
            return AgendaPlan.Empty;
        }

        // A panel the trainee has left (T182) decides nothing for them, and is told nothing of their new institution.
        if (!DecisionRouting.IsEligible(panel, context.Trainee))
        {
            // T235: a panel at the trainee's own institution that no longer covers their speciality plans nothing more, but
            // the lines its review holds are still read against the window rule, or one another sitting decided would block
            // Record and Ratify while the decisions-due page calls it decided. Not for a trainee who has moved institution:
            // that another sitting decided a line would say what their new institution did (T182), and the panel can act on
            // nothing there anyway (CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync).
            if (held.Count == 0 || context.Trainee.InstitutionId != panel.InstitutionId)
            {
                return AgendaPlan.Stranded;
            }

            var heldRecords = await DecisionWindowRecords.LoadAsync(dbContext, [traineeUserId], sitting.Year, cancellationToken);
            return AgendaPlan.Stranded with { DecidedElsewhere = DecidedElsewhereOf(held, heldRecords, traineeUserId) };
        }

        // Every admitted item, so the plan can say whether this panel decides anything for the trainee; only those with a
        // cadence are planned.
        var items = await StarCurriculum.AdmittedItems(dbContext, context.CurriculumId, context.Trainee.InstitutionId)
            .Select(item => new PlannableItem(
                item.Id,
                item.EpaId,
                item.Epa.Code,
                item.Epa.Title,
                item.DecisionCadence,
                item.DecisionBodyKey,
                item.DecisionIsOpportunistic))
            .ToListAsync(cancellationToken);
        var panelDecidesAnything = items.Any(item => DecisionRouting.RoutesTo(item.BodyKey, panel, context.Trainee, context.Panels));

        var records = await DecisionWindowRecords.LoadAsync(dbContext, [traineeUserId], sitting.Year, cancellationToken);
        var institutionId = context.Trainee.InstitutionId;

        var decidedElsewhere = DecidedElsewhereOf(held, records, traineeUserId);

        var lines = new List<CommitteeAgendaLine>();
        var elsewhere = new List<CommitteeAgendaElsewhereDto>();
        var decided = new List<DecidedEpa>();
        var noLongerDecided = new List<CommitteeAgendaEpaDto>();

        foreach (var item in items
                     .Where(item => item.Cadence is not null)
                     .OrderBy(item => item.Code, StringComparer.Ordinal)
                     .ThenBy(item => item.EpaId))
        {
            var window = QuotaWindow.For(item.Cadence!.Value, sitting.End, context.ProgrammeStart);
            if (!CommitteeAgendaLine.IsDueAt(window, sitting))
            {
                continue;
            }

            var windowSemester = DecisionWindowRecords.WindowSemesterOf(window);
            var inWindow = records.LinesIn(traineeUserId, item.EpaId, window);

            // The window's STARs, whether or not a line records them (T215): a STAR is the trainee's, and goes with them.
            var starsInWindow = records.StarStandingsIn(traineeUserId, item.EpaId, window);

            if (DecisionRouting.RoutesTo(item.BodyKey, panel, context.Trainee, context.Panels))
            {
                if (records.IsDecided(traineeUserId, item.EpaId, window))
                {
                    decided.Add(new DecidedEpa(item.EpaId, item.Code));
                }
                else
                {
                    lines.Add(CommitteeAgendaLine.ForCadence(
                        item.CurriculumItemId, item.EpaId, item.Code, item.Title, item.IsOpportunistic, window, sitting));

                    // T235: planned because its window lost the decision a STAR gave it.
                    if (records.HasLostItsDecision(traineeUserId, item.EpaId, window))
                    {
                        noLongerDecided.Add(new CommitteeAgendaEpaDto(item.EpaId, item.Code, item.Title));
                    }
                }

                continue;
            }

            // Only a panel sitting as a College committee is reported: a neonatal review is not told about the thirteen EPAs
            // the general panel takes, but a general review is told what the neonatal CCC still owes.
            var bodyPanel = DecisionRouting.BodyPanelFor(item.BodyKey, context.Trainee, context.Panels);
            if (bodyPanel is not null && bodyPanel.Id != panel.Id)
            {
                elsewhere.Add(new CommitteeAgendaElsewhereDto(
                    item.EpaId,
                    item.Code,
                    item.Title,
                    bodyPanel.Id,
                    bodyPanel.Name,
                    CommitteeAgendaLine.WindowLabelOf(window.AcademicYear, windowSemester),
                    CommitteeAgendaStatus.Elsewhere(
                        inWindow
                            .Where(line => line.State == CommitteeAgendaLineState.Decided || line.PanelInstitutionId == institutionId)
                            .Select(line => records.Standing(line, window)),
                        starsInWindow,
                        mayBeMissed: !item.IsOpportunistic && window.Status == QuotaWindowStatus.Counting,
                        window.End,
                        today)));
            }
        }

        return new AgendaPlan(true, lines, elsewhere, decided, panelDecidesAnything)
        {
            NoLongerDecided = noLongerDecided,
            DecidedElsewhere = decidedElsewhere
        };
    }

    /// <summary>
    /// The EPAs of the lines the review still holds due whose window another sitting has decided (T235): each read on the
    /// window it records, by the rule the planner skips an EPA by. A line's window holds its sitting, so it lies in the year
    /// the records were read for.
    /// </summary>
    private static HashSet<int> DecidedElsewhereOf(
        IEnumerable<CommitteeAgendaLine> held,
        DecisionWindowRecords records,
        string traineeUserId)
        => held
            .Where(line => line.State == CommitteeAgendaLineState.Due &&
                           records.IsDecided(traineeUserId, line.EpaId, DecisionWindowRecords.WindowOf(line)))
            .Select(line => line.EpaId)
            .ToHashSet();

    /// <summary>
    /// The line the chair adds by staging a decision on an EPA the review's agenda does not hold, or why the panel may not
    /// decide it: the item must route to this panel for this trainee (<see cref="DecisionRouting.RoutesTo(string?, DecisionPanel, TraineeScope?, IEnumerable{DecisionPanel})" />,
    /// Decision 3). Reads only. The staging handler runs it after <see cref="StarCurriculum.DemandAsync" /> and before its
    /// first mutation.
    /// </summary>
    /// <param name="review">The review, with its <see cref="CommitteeReview.Panel" /> loaded.</param>
    /// <param name="epa">The EPA the decision is staged on.</param>
    public static async Task<(CommitteeAgendaLine? Line, string? Refusal)> ChairLineAsync(
        IApplicationDbContext dbContext,
        CommitteeReview review,
        Epa epa,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(epa);

        var context = await LoadContextAsync(dbContext, review.TraineeUserId, cancellationToken);
        var item = context is null
            ? null
            : await StarCurriculum.AdmittedItems(dbContext, context.CurriculumId, context.Trainee.InstitutionId)
                .Where(candidate => candidate.EpaId == epa.Id)
                .Select(candidate => new { candidate.Id, candidate.DecisionCadence, candidate.DecisionBodyKey })
                .FirstOrDefaultAsync(cancellationToken);

        if (context is null || item is null)
        {
            return (null, $"{epa.Code} is not on this trainee's curriculum, so it cannot be added to the agenda.");
        }

        if (!DecisionRouting.RoutesTo(item.DecisionBodyKey, review.Panel, context.Trainee, context.Panels))
        {
            return (null, NotThisPanels(epa.Code, item.DecisionBodyKey, review.Panel, context));
        }

        // An item with no cadence is decided in the sitting's own semester: the window the line records.
        var window = QuotaWindow.For(item.DecisionCadence ?? QuotaPeriod.Semester, review.Period.End, context.ProgrammeStart);
        return (CommitteeAgendaLine.ForChair(item.Id, epa.Id, epa.Code, epa.Title, window), null);
    }

    private static string NotThisPanels(string code, string? bodyKey, DecisionPanel panel, PlanningContext context)
    {
        if (!DecisionRouting.IsEligible(panel, context.Trainee))
        {
            return $"{code} is not on this review's agenda, and this panel does not cover the trainee's programme, so it " +
                   "cannot decide it.";
        }

        var bodyPanel = DecisionRouting.BodyPanelFor(bodyKey, context.Trainee, context.Panels);
        return bodyPanel is not null
            ? $"{code} is not on this review's agenda: {bodyPanel.Name} decides it for this trainee. Stage it at a review " +
              "before that panel."
            : $"{code} is not on this review's agenda: {panel.Name} sits as a College committee and decides only the EPAs " +
              "the College gives it. Stage it at a review before the trainee's general panel.";
    }

    private static async Task<PlanningContext?> LoadContextAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(traineeUserId))
        {
            return null;
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .Select(entity => new { entity.CurriculumId, entity.ProgrammeStartDate })
            .FirstOrDefaultAsync(cancellationToken);
        var trainee = await TraineeScopeResolver.ResolveAsync(dbContext, traineeUserId, cancellationToken);

        if (profile is null || trainee is null)
        {
            return null;
        }

        // Every panel at the trainee's institution: routing ignores the rest, and needs the body panels among these.
        var panels = await dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Where(entity => entity.InstitutionId == trainee.InstitutionId)
            .ToListAsync(cancellationToken);

        return new PlanningContext(trainee, profile.CurriculumId, profile.ProgrammeStartDate, panels);
    }

    private sealed record PlanningContext(
        TraineeScope Trainee,
        int CurriculumId,
        DateOnly ProgrammeStart,
        IReadOnlyList<DecisionPanel> Panels);

    private sealed record PlannableItem(
        int CurriculumItemId,
        int EpaId,
        string Code,
        string Title,
        QuotaPeriod? Cadence,
        string? BodyKey,
        bool IsOpportunistic);
}
