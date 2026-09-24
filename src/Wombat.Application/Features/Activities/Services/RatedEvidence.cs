using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What one activity of a rated type turned out to be when read against its pinned version (T135, T150).
/// </summary>
public enum RatedEvidenceOutcome
{
    /// <summary>The EPA, the rating and the assessor all read. The row is an assessor's rating of an EPA.</summary>
    Attributed = 1,

    /// <summary>
    /// There is no named assessor's rating in it, and that is allowed: the pinned version names nobody who writes the
    /// rating (<c>msf_cpsa</c>, D36), or the rating or the assessor was left empty where the form lets it be. Nothing in
    /// the row could have entered an assessor count.
    /// </summary>
    Unattributed = 2,

    /// <summary>
    /// The row's EPA, a field the pinned version declares, or the data itself, is missing or malformed: no stamped EPA
    /// (<c>Activity.EpaId</c>, T137), a rating that is not a rung, an assessor field holding something that is not an
    /// id, or a rating or assessor left empty where every move into the row's state required it. The row is evidence
    /// the reader could not use.
    /// </summary>
    Unreadable = 3
}

/// <summary>One row, read. <see cref="EpaId" />, <see cref="Rating" /> and <see cref="AssessorUserId" /> are
/// meaningful only for <see cref="RatedEvidenceOutcome.Attributed" />.</summary>
public readonly record struct RatedEvidenceReading(
    RatedEvidenceOutcome Outcome,
    int EpaId,
    int Rating,
    string AssessorUserId)
{
    internal static RatedEvidenceReading Unreadable { get; } = new(RatedEvidenceOutcome.Unreadable, 0, 0, string.Empty);

    internal static RatedEvidenceReading Unattributed { get; } = new(RatedEvidenceOutcome.Unattributed, 0, 0, string.Empty);

    internal static RatedEvidenceReading EmptyWhere(bool required) => required ? Unreadable : Unattributed;
}

/// <summary>
/// How one pinned activity-type version records rated evidence: which states count, and which fields hold the rating
/// and the assessor. The one answer the committee sampling report and the trajectory chart share, so the
/// two cannot count different things. (T135, T150, D44)
/// </summary>
/// <remarks>
/// <para>
/// Built once per (type, version) pin, not per row. Before T135 both readers parsed each row for the literal keys
/// <c>epa_id</c>, <c>assessor_user_id</c> and <c>overall</c>/<c>overall_level</c>, in any state, which counted drafts,
/// cancelled requests and declined ratings, missed a builder type whose fields are called something else, and could
/// not tell "this row names no assessor by design" from "this row is broken".
/// </para>
/// <para>
/// <b>Which rows are evidence (D44):</b> those whose state is a terminal state of the pinned workflow, the point where
/// credit fires (<c>ActivityService.PlanCreditIfTerminalAsync</c>): <c>completed</c> for the assessor-rated seeds and
/// <c>recorded</c> for <c>msf_cpsa</c>. Every current seed makes <c>declined</c> and <c>cancelled</c> non-terminal dead
/// ends, so there a rating an assessor wrote and then declined is not evidence. A version that marks them terminal (the
/// generic seeds before <c>c33c14b</c> did) counts them, exactly as it credits them. A type with no workflow falls back
/// to the literal <c>completed</c>.
/// </para>
/// <para>
/// <b>The EPA</b> is not read from the data at all. It is the one stamped on the activity, <c>Activity.EpaId</c>
/// (T137): resolved from the pinned schema's <c>evidence_epa_field</c> by <c>EvidenceEpaResolver</c> at create, on
/// every transition and at an MSF release, and held to the field credit reads by <c>EvidenceEpa.EnsureCreditAgrees</c>.
/// So the EPA a row is counted under here is the EPA its list shows and the EPA it credited. A null stamp is a row about
/// no EPA (its pinned schema declares no pointer, or the field is empty, not an integer, or names no EPA), and such a
/// row is unreadable: loud, not silent. Before T137 this profile guessed the EPA from the data (the credit rules'
/// <c>epa_field</c>, else the schema's <c>epa</c> fields, else the literal <c>epa_id</c>): a second answer to the
/// question the stamp now answers once.
/// </para>
/// <para>
/// <b>The rating</b> is the pinned schema's <c>rated_level_field</c>. A version published before T126 declares none;
/// it then falls back to the type's current declaration, which is also what makes the type rated at all
/// (<see cref="RatedActivityTypes.Classify" />). A version that does not declare that field at all cannot hold the
/// rating, and every row of it is unreadable.
/// </para>
/// <para>
/// <b>The assessor</b>, tried in this order, stopping at the first that names a field:
/// </para>
/// <list type="number">
///   <item>Whoever may write the rating: the fields the <c>field:</c> rules on the rated field's <c>editable_by</c>
///   name (the field's own rule, else its section's, as <c>FieldPermissionEvaluator</c> resolves it).</item>
///   <item>Whoever finishes the record: the <c>field:</c> actors of the moves into a terminal state. That is the
///   person who signed the rating off when the rule on the rating names nobody.</item>
///   <item>When a role or a scope writes the rating, every nominee field
///   (<see cref="ActorFieldRules.RequiredRolesByNomineeField" />): someone other than the trainee rated it, and the
///   form's named people are the only candidates.</item>
///   <item>Otherwise nobody. The trainee writes their own rating (<c>subject|creator</c>, the default) and nobody
///   named finishes it, or it is an MSF: the version names no assessor by design. Naming a supervisor field here
///   would let a trainee pad the distinct-assessor count with ratings they wrote themselves (T150).</item>
/// </list>
/// <para>
/// Ids are compared exactly, as the actor grammar and T102's gate compare them: no trimming.
/// </para>
/// <para>
/// <b>An empty value</b> (missing, null or blank) in the rated or assessor field is unreadable when every move into the
/// row's state required that field — it is visible unconditionally, and each such move declares <c>validation: all</c>
/// with the field <c>required</c>, or lists it in <c>requires_fields</c> — and unattributed otherwise. A row that can be
/// born in its state was checked by no move. <c>validation: owned</c> checks only what the mover may write, which is
/// not knowable without the actor, so it is not counted as a check. Without a workflow the <c>required</c> flag
/// decides. So a completed <c>mini_cex_cpsa</c> with no rating is broken data, and an MSF with no overall level
/// (optional, D10) is not.
/// </para>
/// </remarks>
public sealed class RatedEvidenceProfile
{
    /// <summary>The state a type with no workflow is taken to finish in.</summary>
    public const string NoWorkflowEvidenceState = "completed";

    /// <summary>
    /// An entrustment scale is data, so the rung count is not knowable here. This bound only rejects a value that
    /// cannot be a rung at all (a mis-mapped field, a year, a score out of 100). Moved from the trajectory query.
    /// </summary>
    public const int MaxPlausibleRung = 20;

    private static readonly Workflow NoWorkflow = new(0, string.Empty, [], []);

    private static readonly IReadOnlySet<string> NoStates = new HashSet<string>(StringComparer.Ordinal);

    private readonly bool _readable;
    private readonly IReadOnlySet<string> _statesRequiringRating;
    private readonly IReadOnlySet<string> _statesRequiringAssessor;

    private RatedEvidenceProfile(
        bool readable,
        IReadOnlySet<string> evidenceStates,
        string? ratedLevelField,
        IReadOnlyList<string> assessorFields,
        IReadOnlySet<string> statesRequiringRating,
        IReadOnlySet<string> statesRequiringAssessor)
    {
        _readable = readable;
        EvidenceStates = evidenceStates;
        RatedLevelField = ratedLevelField;
        AssessorFields = assessorFields;
        _statesRequiringRating = statesRequiringRating;
        _statesRequiringAssessor = statesRequiringAssessor;
    }

    /// <summary>The states an activity must be in to count as evidence.</summary>
    public IReadOnlySet<string> EvidenceStates { get; }

    /// <summary>The field holding the rating, or null when neither the pinned version nor the type declares one.</summary>
    public string? RatedLevelField { get; }

    /// <summary>The fields read for the assessor, in schema order. Empty when the version names no assessor.</summary>
    public IReadOnlyList<string> AssessorFields { get; }

    /// <summary>
    /// Whether this version names nobody who writes its rating (<c>msf_cpsa</c>, D36), so no row of it can ever be an
    /// assessor's rating. Known from the version alone, without reading a row: which is what lets the sampling report
    /// place a row the caller may not read as unattributed rather than withheld, since the figures would be the same
    /// with it. (T135 review)
    /// </summary>
    public bool NamesNoAssessor => _readable && AssessorFields.Count == 0;

    /// <summary>Whether an activity in this state is evidence at all.</summary>
    public bool IsEvidence(string? currentState)
        => currentState is not null && EvidenceStates.Contains(currentState);

    /// <summary>Reads one activity, in the state it is in. Total: never throws on bad data.</summary>
    /// <param name="currentState">The activity's state.</param>
    /// <param name="epaId">The activity's stamped <c>Activity.EpaId</c> (T137), which is the EPA; null is no EPA. The
    /// data is never consulted for it (see the class remarks).</param>
    /// <param name="dataJson">The activity's data, read for the rating and the assessor.</param>
    /// <remarks>
    /// The checks run in this order, and the order is the classification. A version that names no assessor is
    /// unattributed whatever the row holds: nothing in it could enter an assessor count, so a garbled MSF must not make
    /// the committee report incomplete. Then: no stamped EPA is unreadable; an empty rating is unreadable or
    /// unattributed by whether this state required it; a rating that is not a rung is unreadable; an assessor field
    /// holding something that is not an id is unreadable; an empty assessor is unreadable or unattributed by whether
    /// this state required it.
    /// </remarks>
    public RatedEvidenceReading Read(string? currentState, int? epaId, string? dataJson)
    {
        if (!_readable)
        {
            return RatedEvidenceReading.Unreadable;
        }

        if (AssessorFields.Count == 0)
        {
            return RatedEvidenceReading.Unattributed;
        }

        if (epaId is not int stampedEpaId)
        {
            return RatedEvidenceReading.Unreadable;
        }

        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return RatedEvidenceReading.Unreadable;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return RatedEvidenceReading.Unreadable;
            }

            if (!root.TryGetProperty(RatedLevelField!, out var ratingValue) || IsEmpty(ratingValue))
            {
                return RatedEvidenceReading.EmptyWhere(RequiredIn(_statesRequiringRating, currentState));
            }

            if (!TryGetInt32(ratingValue, out var rating) || rating < 1 || rating > MaxPlausibleRung)
            {
                return RatedEvidenceReading.Unreadable;
            }

            var malformed = false;
            foreach (var assessorField in AssessorFields)
            {
                if (!root.TryGetProperty(assessorField, out var assessorValue) || IsEmpty(assessorValue))
                {
                    continue;
                }

                if (assessorValue.ValueKind == JsonValueKind.String)
                {
                    return new RatedEvidenceReading(
                        RatedEvidenceOutcome.Attributed, stampedEpaId, rating, assessorValue.GetString()!);
                }

                malformed = true;
            }

            return malformed
                ? RatedEvidenceReading.Unreadable
                : RatedEvidenceReading.EmptyWhere(RequiredIn(_statesRequiringAssessor, currentState));
        }
        catch (JsonException)
        {
            return RatedEvidenceReading.Unreadable;
        }
    }

    /// <summary>
    /// The profile of one version, from its stored JSON.
    /// </summary>
    /// <param name="schemaJson">The pinned schema.</param>
    /// <param name="workflowJson">The pinned workflow.</param>
    /// <param name="fallbackRatedLevelField">The type's CURRENT <c>rated_level_field</c>, for a version that
    /// predates T126 and declares none.</param>
    public static RatedEvidenceProfile Build(
        string? schemaJson,
        string? workflowJson,
        string? fallbackRatedLevelField)
    {
        var workflow = TryParseWorkflow(workflowJson);
        IReadOnlySet<string> evidenceStates = workflow is null
            ? new HashSet<string>(StringComparer.Ordinal) { NoWorkflowEvidenceState }
            : workflow.States
                .Where(state => state.Terminal)
                .Select(state => state.Key)
                .ToHashSet(StringComparer.Ordinal);

        var schema = TryParseSchema(schemaJson);
        if (schema is null)
        {
            // Without the schema nobody can say who wrote the rating, so every row reads as unreadable: honest, and
            // it makes the committee report say so rather than count a guess.
            return Unreadable(evidenceStates, fallbackRatedLevelField);
        }

        var ratedLevelField = schema.RatedLevelField ?? fallbackRatedLevelField;
        var fields = schema.Sections
            .SelectMany(section => section.Fields.Select(field => (Section: section, Field: field)))
            .ToArray();

        var rated = fields.FirstOrDefault(entry =>
            string.Equals(entry.Field.Key, ratedLevelField, StringComparison.Ordinal));
        if (rated.Field is null)
        {
            // The parser guarantees a DECLARED rated_level_field names a field, so this is a version that declares none
            // and lacks the field its type now rates on. Its rows cannot hold the rating the type says they carry, and
            // that is not "the rating was left empty": unreadable, so the committee report says it could not read them.
            return Unreadable(evidenceStates, ratedLevelField);
        }

        var assessorFieldNames = ResolveAssessorFieldNames(schema, workflow, rated.Field.EditableBy ?? rated.Section.EditableBy);

        // Schema order, so "the first that holds an id" means the same thing on every read. A name no field declares
        // (a stored rule naming a missing field) goes last.
        var schemaOrder = fields.Select(entry => entry.Field.Key).Distinct(StringComparer.Ordinal).ToList();
        var assessorFields = assessorFieldNames
            .OrderBy(name =>
            {
                var index = schemaOrder.IndexOf(name);
                return index >= 0 ? index : int.MaxValue;
            })
            .ThenBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var declaredAssessors = fields
            .Where(entry => assessorFieldNames.Contains(entry.Field.Key))
            .ToArray();

        var statesRequiringRating = evidenceStates
            .Where(state => IsRequiredOnArrival(rated, state, workflow))
            .ToHashSet(StringComparer.Ordinal);
        var statesRequiringAssessor = evidenceStates
            .Where(state => declaredAssessors.Any(entry => IsRequiredOnArrival(entry, state, workflow)))
            .ToHashSet(StringComparer.Ordinal);

        return new RatedEvidenceProfile(
            true,
            evidenceStates,
            ratedLevelField,
            assessorFields,
            statesRequiringRating,
            statesRequiringAssessor);
    }

    private static RatedEvidenceProfile Unreadable(IReadOnlySet<string> evidenceStates, string? ratedLevelField)
        => new(false, evidenceStates, ratedLevelField, [], NoStates, NoStates);

    /// <summary>The assessor rule in the class remarks, steps 1 to 4.</summary>
    private static HashSet<string> ResolveAssessorFieldNames(FormSchema schema, Workflow? workflow, ActorRule? ratedRule)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        ActorFieldRules.CollectFieldNames(ratedRule, names);
        if (names.Count > 0)
        {
            return names;
        }

        if (workflow is not null)
        {
            var terminalStates = workflow.States
                .Where(state => state.Terminal)
                .Select(state => state.Key)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var transition in workflow.Transitions.Where(transition => terminalStates.Contains(transition.To)))
            {
                ActorFieldRules.CollectFieldNames(transition.Actor, names);
            }

            if (names.Count > 0)
            {
                return names;
            }
        }

        if (AdmitsSomeoneOtherThanTheTrainee(ratedRule))
        {
            names.UnionWith(ActorFieldRules.RequiredRolesByNomineeField(schema, workflow ?? NoWorkflow).Keys);
        }

        return names;
    }

    /// <summary>
    /// Whether the rule lets someone who is neither the subject nor the creator write the rating. A null rule is the
    /// <c>subject|creator</c> default. <c>+</c> needs every part to admit someone else; <c>|</c> needs one.
    /// </summary>
    private static bool AdmitsSomeoneOtherThanTheTrainee(ActorRule? rule) => rule switch
    {
        null or SubjectUserActorRule or CreatorUserActorRule => false,
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.All(AdmitsSomeoneOtherThanTheTrainee),
        CombinedActorRule any => any.Rules.Any(AdmitsSomeoneOtherThanTheTrainee),
        _ => true
    };

    /// <summary>
    /// Whether every move into <paramref name="state" /> insisted on this field, so a row in that state holding no value
    /// in it is broken rather than allowed to be empty. The rule in the class remarks.
    /// </summary>
    private static bool IsRequiredOnArrival((FormSection Section, FormField Field) entry, string state, Workflow? workflow)
    {
        // SchemaValidator skips a hidden field, required or not, so a field that can be hidden is never guaranteed.
        if (entry.Field.ShowIf is not null || entry.Section.ShowIf is not null)
        {
            return false;
        }

        if (workflow is null)
        {
            return entry.Field.Required;
        }

        if (string.Equals(state, workflow.InitialState, StringComparison.Ordinal))
        {
            return false;
        }

        var arrivals = workflow.Transitions
            .Where(transition => string.Equals(transition.To, state, StringComparison.Ordinal))
            .ToArray();

        return arrivals.Length > 0 && arrivals.All(transition =>
            transition.RequiresFields.Contains(entry.Field.Key, StringComparer.Ordinal) ||
            (transition.Validation == TransitionValidation.All && entry.Field.Required));
    }

    private static bool RequiredIn(IReadOnlySet<string> states, string? currentState)
        => currentState is not null && states.Contains(currentState);

    private static Workflow? TryParseWorkflow(string? workflowJson)
    {
        if (string.IsNullOrWhiteSpace(workflowJson))
        {
            return null;
        }

        try
        {
            return WorkflowParser.Parse(workflowJson);
        }
        catch (Exception)
        {
            // Deliberately broad, as CreditRuleFields is: the parser raises its own exception for the shapes it
            // checks, and System.Text.Json raises InvalidOperationException for a value of the wrong kind. A stored
            // workflow that no longer parses is a defect, but not a committee report's to raise.
            return null;
        }
    }

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
            // As above.
            return null;
        }
    }

    internal static string? TryReadRatedLevelField(string? schemaJson) => TryParseSchema(schemaJson)?.RatedLevelField;

    private static bool IsEmpty(JsonElement value)
        => value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
           (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString()));

    private static bool TryGetInt32(JsonElement property, out int value)
    {
        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String &&
            int.TryParse(property.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>
/// Loads <see cref="RatedEvidenceProfile" />s for the (type, version) pins a set of activities carries. (T135)
/// </summary>
public static class RatedEvidenceProfiles
{
    /// <summary>
    /// One profile per pin asked for, always: the pinned <see cref="ActivityTypeVersion" /> when it exists, else the
    /// type's own columns (a type whose version rows were never written).
    /// </summary>
    /// <remarks>
    /// Matched in memory over a set already bounded by the caller's activities, because <c>Activity</c> carries no
    /// foreign key to its version (<c>ActivityConfiguration</c>). A pin whose type is gone reads every row as
    /// unreadable.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<(int ActivityTypeId, int Version), RatedEvidenceProfile>> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<(int ActivityTypeId, int Version)> pins,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(pins);

        var wanted = pins.Distinct().ToArray();
        var profiles = new Dictionary<(int ActivityTypeId, int Version), RatedEvidenceProfile>(wanted.Length);
        if (wanted.Length == 0)
        {
            return profiles;
        }

        var typeIds = wanted.Select(pin => pin.ActivityTypeId).Distinct().ToArray();
        var versionNumbers = wanted.Select(pin => pin.Version).Distinct().ToArray();

        var types = await dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(type => typeIds.Contains(type.Id))
            .Select(type => new { type.Id, type.SchemaJson, type.WorkflowJson })
            .ToDictionaryAsync(type => type.Id, cancellationToken);

        var versions = await dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Where(version => typeIds.Contains(version.ActivityTypeId) && versionNumbers.Contains(version.Version))
            .Select(version => new
            {
                version.ActivityTypeId,
                version.Version,
                version.SchemaJson,
                version.WorkflowJson
            })
            .ToListAsync(cancellationToken);

        var versionByPin = versions
            .GroupBy(version => (version.ActivityTypeId, version.Version))
            .ToDictionary(group => group.Key, group => group.First());

        var currentRatedFieldByType = types.Values.ToDictionary(
            type => type.Id,
            type => RatedEvidenceProfile.TryReadRatedLevelField(type.SchemaJson));

        foreach (var pin in wanted)
        {
            if (!types.TryGetValue(pin.ActivityTypeId, out var type))
            {
                profiles[pin] = RatedEvidenceProfile.Build(null, null, null);
                continue;
            }

            var fallbackRatedField = currentRatedFieldByType[type.Id];
            profiles[pin] = versionByPin.TryGetValue(pin, out var version)
                ? RatedEvidenceProfile.Build(version.SchemaJson, version.WorkflowJson, fallbackRatedField)
                : RatedEvidenceProfile.Build(type.SchemaJson, type.WorkflowJson, fallbackRatedField);
        }

        return profiles;
    }
}
