using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Programme.Filing;

/// <summary>
/// When a registrar's activities were filed, and the one rule "Nothing filed in 30 days" reads (T358, flow 06, E5): the
/// rule Programme trainees' filter, the Coordinator's Home card and the weekly coordinator digest share.
/// </summary>
/// <remarks>
/// <para>
/// <b>Filed is having left draft.</b> An activity is filed by the move <c>ActivityTransition.DaysAfterEncounter</c>'s
/// filing is judged by (T160): its first move out of the workflow's initial state that leads on
/// (<see cref="Workflow.LeftInitialStateLeadingOn" />), or its create, when the create was itself the filing, because the
/// type's initial state is no draft: no move that leads on out of it is the subject's or the creator's to take (T127, T148:
/// a type born <c>requested</c>, or born terminal; build review R2). A recorded MSF counts: its release moves each record out of <c>draft</c> by <c>record</c>, which leads on.
/// A draft never submitted, and a draft cancelled from draft, was never filed. A request returned and re-submitted was
/// filed once, at its first submission.
/// </para>
/// <para>
/// Until T358 the digest counted any activity <i>created</i> about the registrar in the last 30 days, drafts, cancelled
/// and declined requests included (review E5), so a registrar who had opened one draft and abandoned it read as active.
/// </para>
/// <para>
/// <b>The window starts at the later of admission and 30 days ago.</b> A registrar admitted last week has not had 30 days
/// to file anything, so is not listed (Step 2.32's board is empty because the registrars were admitted that week; D1 adds
/// <c>TraineeProfile.AdmittedOn</c>). Days are South African calendar days (<see cref="ProgrammeCalendar" />).
/// </para>
/// <para>
/// The activity table is read for dates only, and only for the registrars a caller has already admitted to its list (the
/// programme scope's current registrars, or a digest recipient's roster): its type, create time and moves, never
/// its data (<c>WeeklyCoordinatorDigestPostgresTests</c> holds the digest to that). No activity row leaves this class.
/// </para>
/// </remarks>
public static class FilingMoments
{
    /// <summary>How many days "Nothing filed" looks back: 30 (E5).</summary>
    public const int WindowDays = 30;

    /// <summary>
    /// Each registrar's latest filing, in UTC; a registrar who has never filed anything is absent.
    /// </summary>
    /// <remarks>
    /// Each activity's moment is read from its own pinned workflow (<see cref="PinnedWorkflows.LoadAsync" />, which falls
    /// back to the type's own columns). An activity whose workflow is missing or no longer parses was never filed by any
    /// move this can recognise, so it is not counted.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<string, DateTime>> LastFiledAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<string> traineeUserIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(traineeUserIds);

        var ids = traineeUserIds.Where(id => !string.IsNullOrEmpty(id)).Distinct(StringComparer.Ordinal).ToArray();
        var lastFiled = new Dictionary<string, DateTime>(StringComparer.Ordinal);
        if (ids.Length == 0)
        {
            return lastFiled;
        }

        var activities = await dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => ids.Contains(activity.SubjectUserId))
            .Select(activity => new FilingFacts(
                activity.ActivityTypeId,
                activity.SchemaVersion,
                activity.SubjectUserId,
                activity.CreatedOn,
                activity.Transitions
                    .Select(row => new FilingMove(row.FromState, row.ToState, row.TransitionKey, row.OccurredOn))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var workflows = await PinnedWorkflows.LoadAsync(
            dbContext,
            activities.Select(activity => (activity.ActivityTypeId, activity.SchemaVersion)),
            cancellationToken);

        foreach (var activity in activities)
        {
            if (workflows.GetValueOrDefault((activity.ActivityTypeId, activity.SchemaVersion)) is not { } workflow ||
                FiledOn(workflow, activity) is not { } filedOn)
            {
                continue;
            }

            if (!lastFiled.TryGetValue(activity.SubjectUserId, out var latest) || filedOn > latest)
            {
                lastFiled[activity.SubjectUserId] = filedOn;
            }
        }

        return lastFiled;
    }

    /// <summary>
    /// Whether a current registrar has filed nothing in the window: admitted on or before today − 30 days, and nothing
    /// filed on or after that day, by the South African day of the filing (E5). The window then starts 30 days ago; a
    /// registrar admitted since has had less than 30 days, so is never listed.
    /// </summary>
    public static bool NothingFiled(DateOnly admittedOn, DateTime? lastFiledOn, DateOnly today)
    {
        var windowStart = today.AddDays(-WindowDays);
        if (admittedOn > windowStart)
        {
            return false;
        }

        return lastFiledOn is not { } filed || ProgrammeCalendar.DateOf(filed) < windowStart;
    }

    /// <summary>
    /// When one activity was filed, in UTC, or null when it never was: its create when the create was the filing, else
    /// its first recorded move out of the initial state that leads on.
    /// </summary>
    internal static DateTime? FiledOn(Workflow workflow, FilingFacts activity)
    {
        if (CreateIsTheFiling(workflow))
        {
            return activity.CreatedOn;
        }

        var filing = activity.Moves
            .Where(move => workflow.LeftInitialStateLeadingOn(move.FromState, move.ToState, move.TransitionKey))
            .OrderBy(move => move.OccurredOn)
            .FirstOrDefault();

        return filing?.OccurredOn;
    }

    /// <summary>
    /// Whether the create was the filing: the type's initial state is no draft, because no move that leads on out of it is
    /// the author side's, the subject's or the creator's (T127, T148: a type born <c>requested</c>, or born terminal). A
    /// <c>role:</c> or <c>scope:</c> arm is read as the author side's, as the activity page reads it: an MSF record's
    /// <c>record</c> is staff's, and its create, by staff, is then not the filing; the <c>record</c> move is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A question of the type, not of who created the row (T358, build review R2). Until the fix pass a <c>subject</c> arm
    /// counted only when the creator was the subject, so a draft someone else opened about the registrar, which only the
    /// registrar may submit, read as filed at its create. It is a draft whoever opened it, and is filed when it is
    /// submitted. The write path asks the creator alone (<c>ActivityService.CreateIsTheFiling</c>), and agrees wherever
    /// the creator is the subject, which is every create a registrar makes.
    /// </para>
    /// <para>
    /// A <c>field:</c> arm is read as someone else's, without reading the data: it names a nominee, and the nominee gate
    /// never lets a field name the activity's subject (T102), so a type whose way on is only the nominee's is born
    /// requested. The page asks the data; this read keeps away from it (no content reaches a date read).
    /// </para>
    /// </remarks>
    private static bool CreateIsTheFiling(Workflow workflow)
        => !workflow.TransitionsLeadingOn(workflow.InitialState).Any(transition => IsTheAuthorSides(transition.Actor));

    private static bool IsTheAuthorSides(ActorRule rule) => rule switch
    {
        SubjectUserActorRule => true,
        CreatorUserActorRule => true,
        FieldUserActorRule => false,
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.All(IsTheAuthorSides),
        CombinedActorRule any => any.Rules.Any(IsTheAuthorSides),
        _ => true
    };

    /// <summary>What the filing rule reads of one activity.</summary>
    internal sealed record FilingFacts(
        int ActivityTypeId,
        int SchemaVersion,
        string SubjectUserId,
        DateTime CreatedOn,
        IReadOnlyList<FilingMove> Moves);

    /// <summary>One recorded move, as the filing rule reads it.</summary>
    internal sealed record FilingMove(string FromState, string ToState, string TransitionKey, DateTime OccurredOn);
}
