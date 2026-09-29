using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What an activity row, or the activity's page, shows beyond its columns (T342, B6, B7, E7): who has it now, its
/// nominee, whether it was returned, and its name. My activities, Needs you and the activity's page read these through
/// <see cref="ResolveAsync" />, so no two of them can name one activity, or its holder, differently.
/// </summary>
public static class ActivityRowDetails
{
    /// <summary>
    /// The details of every row, keyed by activity id, with every name they show looked up in one call.
    /// </summary>
    /// <param name="alsoNamed">
    /// Other people the page names, looked up in the same call and returned with the details: the activity page's
    /// history actors, so the page still makes one lookup (T142).
    /// </param>
    /// <param name="formOf">Each row's pinned form (<see cref="PinnedForms" />).</param>
    /// <param name="sharesTheRest">
    /// Whether another of the row's subject's activities the caller can read has the same type, EPA and date
    /// (<see cref="ActivityDisplayNames.CollisionKey" />): the name then carries the nominee (E7).
    /// </param>
    public static async Task<(IReadOnlyDictionary<int, ActivityRowDetail> Details, UserDisplayNames Names)> ResolveAsync(
        IReadOnlyList<ActivityRowFacts> rows,
        Func<ActivityRowFacts, PinnedForm> formOf,
        Func<ActivityRowFacts, bool> sharesTheRest,
        ClaimsPrincipal principal,
        IUserAdministrationService users,
        CancellationToken cancellationToken,
        IEnumerable<string?>? alsoNamed = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(formOf);
        ArgumentNullException.ThrowIfNull(sharesTheRest);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(users);

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var read = rows
            .Select(row =>
            {
                var form = formOf(row);
                return new
                {
                    Row = row,
                    Form = form,
                    Holder = ActivityHolders.Resolve(
                        form.Workflow, row.CurrentState, row.SubjectUserId, row.CreatedByUserId, row.DataJson,
                        row.LastMove?.OccurredOn, callerUserId),
                    NomineeUserId = ActivityHolders.NomineeField(form.Workflow, row.CurrentState) is { } field
                        ? ActivityHolders.ReadUserField(row.DataJson, field)
                        : null,
                    Returned = ActivityHolders.ReturnOf(
                        form.Workflow, row.CurrentState, row.SubjectUserId, row.CreatedByUserId, row.LastMove)
                };
            })
            .ToList();

        var names = await UserDisplayNames.ResolveAsync(
            users,
            read.SelectMany(entry => new[] { entry.Holder.UserId, entry.NomineeUserId, entry.Returned?.ByUserId })
                .Concat(alsoNamed ?? []),
            cancellationToken);

        var details = read.ToDictionary(
            entry => entry.Row.Id,
            entry =>
            {
                var nomineeName = entry.NomineeUserId is { } nominee ? names.NameOf(nominee) : null;
                var name = ActivityDisplayNames.Compose(
                    entry.Row.ActivityTypeName,
                    entry.Row.EpaCode,
                    entry.Row.ObservedOn,
                    entry.Row.ObservedOnDeclared,
                    formHasEpa: entry.Form.Schema is not { EvidenceEpaField: null },
                    formHasDate: entry.Form.Schema is not { ObservationDateField: null });
                var withNominee = sharesTheRest(entry.Row) && nomineeName is not null;

                return new ActivityRowDetail(
                    ActivityHolders.WithName(entry.Holder, names.NameOf),
                    nomineeName,
                    entry.Returned is { } returned ? returned with { ByName = names.NameOf(returned.ByUserId) } : null,
                    ActivityDisplayNames.WithNominee(name, nomineeName, withNominee),
                    withNominee)
                {
                    // Whether the nominee is the one it goes to, or the one it is talked over with: the link's
                    // "to"/"with" (T342, lane D). Null when the pin no longer resolves to a form and a workflow.
                    Shape = entry.Form is { Schema: { } schema, Workflow: { } workflow }
                        ? ActivityTypeShapes.Of(schema, workflow)
                        : null
                };
            });

        return (details, names);
    }

    /// <summary>The code of one EPA, or null when it no longer exists.</summary>
    public static Task<string?> EpaCodeAsync(IApplicationDbContext dbContext, int epaId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epa.Id == epaId)
            .Select(epa => (string?)epa.Code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// The collision keys (<see cref="ActivityDisplayNames.CollisionKey" />) that more than one of the rows'
    /// subjects' activities in <paramref name="readable" /> share: the rows whose name needs their nominee (E7).
    /// </summary>
    /// <param name="readable">
    /// The activities the caller may read (<c>Set&lt;Activity&gt;().WhereReadableBy(principal)</c>, applied by the
    /// handler), so an activity the caller cannot open never changes a name they can see.
    /// </param>
    public static async Task<IReadOnlySet<(string SubjectUserId, int ActivityTypeId, int? EpaId, DateOnly? ObservedOn)>> SharedKeysAsync(
        IQueryable<Activity> readable,
        IReadOnlyCollection<ActivityRowFacts> rows,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(readable);
        ArgumentNullException.ThrowIfNull(rows);

        if (rows.Count == 0)
        {
            return new HashSet<(string, int, int?, DateOnly?)>();
        }

        var subjects = rows.Select(row => row.SubjectUserId).Distinct(StringComparer.Ordinal).ToArray();
        var typeIds = rows.Select(row => row.ActivityTypeId).Distinct().ToArray();

        var siblings = await readable
            .AsNoTracking()
            .Where(activity => subjects.Contains(activity.SubjectUserId) && typeIds.Contains(activity.ActivityTypeId))
            .Select(activity => new
            {
                activity.SubjectUserId,
                activity.ActivityTypeId,
                activity.EpaId,
                activity.ObservedOn,
                Declared = activity.ObservedOnSource == ObservationDateSource.Declared
            })
            .ToListAsync(cancellationToken);

        return siblings
            .GroupBy(sibling => ActivityDisplayNames.CollisionKey(
                sibling.SubjectUserId, sibling.ActivityTypeId, sibling.EpaId, sibling.ObservedOn, sibling.Declared))
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();
    }
}

/// <summary>
/// What <see cref="ActivityRowDetails" /> reads of one activity: its columns, its data, and its newest recorded move.
/// </summary>
public sealed record ActivityRowFacts(
    int Id,
    int ActivityTypeId,
    int SchemaVersion,
    string ActivityTypeName,
    string SubjectUserId,
    string CreatedByUserId,
    string CurrentState,
    int? EpaId,
    string? EpaCode,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    string DataJson,
    ActivityLastMove? LastMove)
{
    /// <summary>What another of the subject's activities must share for this one's name to need its nominee (E7).</summary>
    public (string SubjectUserId, int ActivityTypeId, int? EpaId, DateOnly? ObservedOn) CollisionKey
        => ActivityDisplayNames.CollisionKey(SubjectUserId, ActivityTypeId, EpaId, ObservedOn, ObservedOnDeclared);

    /// <summary>The facts of a loaded activity, with its transitions loaded, and the stamped EPA's code.</summary>
    public static ActivityRowFacts From(Activity activity, string? epaCode)
    {
        ArgumentNullException.ThrowIfNull(activity);

        var last = activity.Transitions
            .OrderByDescending(transition => transition.OccurredOn)
            .ThenByDescending(transition => transition.Id)
            .FirstOrDefault();

        return new ActivityRowFacts(
            activity.Id,
            activity.ActivityTypeId,
            activity.SchemaVersion,
            activity.ActivityType.Name,
            activity.SubjectUserId,
            activity.CreatedByUserId,
            activity.CurrentState,
            activity.EpaId,
            epaCode,
            activity.ObservedOn,
            activity.ObservedOnSource == ObservationDateSource.Declared,
            activity.DataJson,
            last is null ? null : new ActivityLastMove(
                last.FromState, last.ToState, last.ActorUserId, last.OccurredOn, last.Note, last.TransitionKey));
    }
}

/// <summary>What an activity row, or the activity's page, shows beyond its columns (T342).</summary>
public sealed record ActivityRowDetail(
    ActivityHolderDto Holder,
    string? NomineeName,
    ActivityReturnDto? Returned,
    string DisplayName,
    bool DisplayNameHasNominee)
{
    /// <summary>The type's shape, from its pinned form (<see cref="ActivityTypeShapes.Of(FormSchema, Workflow)" />); null when the pin does not parse.</summary>
    public ActivityTypeShape? Shape { get; init; }
}

/// <summary>A pin's published workflow and form, either null when missing or no longer parseable.</summary>
public sealed record PinnedForm(Workflow? Workflow, FormSchema? Schema)
{
    /// <summary>A pin that resolves to nothing.</summary>
    public static readonly PinnedForm None = new(null, null);

    /// <summary>A stored form and workflow, each parsed, or null when missing or no longer parseable. Total.</summary>
    public static PinnedForm Parse(string? schemaJson, string? workflowJson)
        => new(PinnedWorkflows.TryParse(workflowJson), TryParseSchema(schemaJson));

    private static FormSchema? TryParseSchema(string? schemaJson)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            return null;
        }

        try
        {
            return FormSchemaParser.Parse(schemaJson);
        }
        catch (Exception)
        {
            // Deliberately broad, as PinnedWorkflows.TryParse is: a list must not fail over a stored form it can still name.
            return null;
        }
    }
}

/// <summary>
/// The workflow and the form each activity is pinned to, resolved as <see cref="PinnedWorkflows.LoadAsync" /> resolves a
/// workflow: the pinned version's row, else the type's own columns (T342). A list's state labels come from the workflow,
/// its names' segments from the form (<see cref="ActivityDisplayNames" />).
/// </summary>
public static class PinnedForms
{
    /// <summary>One entry per pin asked for, always; each distinct pin parsed once.</summary>
    public static async Task<IReadOnlyDictionary<(int ActivityTypeId, int Version), PinnedForm>> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<(int ActivityTypeId, int Version)> pins,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(pins);

        var wanted = pins.Distinct().ToArray();
        var forms = new Dictionary<(int ActivityTypeId, int Version), PinnedForm>(wanted.Length);
        if (wanted.Length == 0)
        {
            return forms;
        }

        var typeIds = wanted.Select(pin => pin.ActivityTypeId).Distinct().ToArray();
        var versionNumbers = wanted.Select(pin => pin.Version).Distinct().ToArray();

        var types = await dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(type => typeIds.Contains(type.Id))
            .Select(type => new { type.Id, type.SchemaJson, type.WorkflowJson })
            .ToDictionaryAsync(type => type.Id, cancellationToken);

        var versions = (await dbContext.Set<ActivityTypeVersion>()
                .AsNoTracking()
                .Where(version => typeIds.Contains(version.ActivityTypeId) && versionNumbers.Contains(version.Version))
                .Select(version => new { version.ActivityTypeId, version.Version, version.SchemaJson, version.WorkflowJson })
                .ToListAsync(cancellationToken))
            .GroupBy(version => (version.ActivityTypeId, version.Version))
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var pin in wanted)
        {
            forms[pin] = versions.TryGetValue(pin, out var version)
                ? PinnedForm.Parse(version.SchemaJson, version.WorkflowJson)
                : types.TryGetValue(pin.ActivityTypeId, out var type)
                    ? PinnedForm.Parse(type.SchemaJson, type.WorkflowJson)
                    : PinnedForm.None;
        }

        return forms;
    }
}
