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
/// <param name="DecidedInWindow">The codes of EPAs this panel decides that a STAR already decided in their window.</param>
/// <param name="PanelDecidesAnything">
/// Whether any of the trainee's in-force curriculum items routes to this panel, with a cadence or without, due or not: whether
/// a sitting before it could decide anything at all. An entrustment-only review is not scheduled where it could not (T131
/// slice 5), since its decision is what its agenda holds.
/// </param>
internal sealed record AgendaPlan(
    bool TraineeHasCurriculum,
    IReadOnlyList<CommitteeAgendaLine> Lines,
    IReadOnlyList<CommitteeAgendaElsewhereDto> RoutedElsewhere,
    IReadOnlyList<string> DecidedInWindow,
    bool PanelDecidesAnything)
{
    public static readonly AgendaPlan Empty = new(false, [], [], [], false);

    /// <summary>The plan for a panel the trainee is no longer eligible for: nothing, and nothing about anyone else.</summary>
    public static readonly AgendaPlan Stranded = new(true, [], [], [], false);
}

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
/// An item already Decided on some review's line for the same window is skipped. What another panel sitting as a College
/// committee owes in the period is reported beside the agenda, read-only, with its standing in the window.
/// </para>
/// <para>
/// <b>A review its trainee has left.</b> A panel plans, and reports, nothing for a trainee it is not eligible for
/// (<see cref="DecisionRouting.IsEligible" />): a trainee who has moved to another institution strands their open review
/// (T182), and its page must not show the new institution's panels or where its decisions stand.
/// </para>
/// <para>
/// <b>What another sitting already did.</b> A decision is the trainee's and goes with them: a line Decided on any review
/// decides its window while its STAR does (<see cref="CommitteeAgendaStatus.StarDecides" />, the decisions-due page's
/// rule, T131 slice 6). A revoked STAR decides nothing, so its EPA is planned again, and so does one superseded inside
/// the window by a STAR since revoked; an expired one decided its window (Decision 10). The records are the page's
/// (<see cref="DecisionWindowRecords" />). A sitting is its institution's: a line still due, or deferred, on a review
/// before a panel at another institution (a review the trainee stranded when they moved) says nothing about where the
/// decision stands here.
/// </para>
/// <para>
/// Reads only. Every caller runs it before its first mutation.
/// </para>
/// </remarks>
internal static class AgendaPlanner
{
    /// <summary>What a sitting of <paramref name="panel" /> for <paramref name="sitting" /> would put on its agenda.</summary>
    /// <param name="today">The programme's today (<see cref="ProgrammeCalendar.DateOf" />): only "missed" reads it.</param>
    public static async Task<AgendaPlan> PlanAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        DecisionPanel panel,
        AcademicPeriod sitting,
        DateOnly today,
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
            return AgendaPlan.Stranded;
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

        var lines = new List<CommitteeAgendaLine>();
        var elsewhere = new List<CommitteeAgendaElsewhereDto>();
        var decided = new List<string>();

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

            if (DecisionRouting.RoutesTo(item.BodyKey, panel, context.Trainee, context.Panels))
            {
                if (inWindow.Any(line => records.Standing(line, window).Decides))
                {
                    decided.Add(item.Code);
                }
                else
                {
                    lines.Add(CommitteeAgendaLine.ForCadence(
                        item.CurriculumItemId, item.EpaId, item.Code, item.Title, item.IsOpportunistic, window, sitting));
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
                        mayBeMissed: !item.IsOpportunistic && window.Status == QuotaWindowStatus.Counting,
                        window.End,
                        today)));
            }
        }

        return new AgendaPlan(true, lines, elsewhere, decided, panelDecidesAnything);
    }

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
