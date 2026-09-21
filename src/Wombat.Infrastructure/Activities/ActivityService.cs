using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Activities;

public sealed class ActivityService : IActivityService
{
    private const string EmptyObjectJson = "{}";

    private readonly IApplicationDbContext _dbContext;
    private readonly ISchemaValidator _schemaValidator;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly ICreditApplier _creditApplier;
    private readonly IFieldPermissionEvaluator _fieldPermissionEvaluator;

    public ActivityService(
        IApplicationDbContext dbContext,
        ISchemaValidator schemaValidator,
        IWorkflowEvaluator workflowEvaluator,
        ICreditApplier creditApplier,
        IFieldPermissionEvaluator fieldPermissionEvaluator)
    {
        _dbContext = dbContext;
        _schemaValidator = schemaValidator;
        _workflowEvaluator = workflowEvaluator;
        _creditApplier = creditApplier;
        _fieldPermissionEvaluator = fieldPermissionEvaluator;
    }

    public async Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == input.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        var (schema, workflow) = ParsePublished(activityType);

        var subjectUserId = input.SubjectUserId.Trim();
        var subjectScope = await ResolveSubjectScopeAsync(subjectUserId, cancellationToken);

        var activity = BuildDraftActivity(
            activityType,
            schema,
            workflow,
            subjectUserId,
            subjectScope,
            input.CreatedByUserId,
            input.InitialDataJson,
            input.Principal,
            DateTime.UtcNow);

        _dbContext.Set<Activity>().Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(activity);
    }

    /// <summary>
    /// The published schema and workflow of a type that is actually offerable, or a refusal.
    /// </summary>
    private static (FormSchema Schema, Workflow Workflow) ParsePublished(ActivityType activityType)
    {
        if (activityType.Version <= 0 ||
            string.IsNullOrWhiteSpace(activityType.SchemaJson) ||
            string.IsNullOrWhiteSpace(activityType.WorkflowJson))
        {
            throw new InvalidOperationException("The selected activity type has not been published yet.");
        }

        return (FormSchemaParser.Parse(activityType.SchemaJson), WorkflowParser.Parse(activityType.WorkflowJson));
    }

    /// <summary>
    /// A new activity in its type's initial state, pinned, scope-stamped, filtered to what this creator
    /// may write, validated and date-stamped — everything but <c>Add</c> and <c>SaveChanges</c>.
    /// </summary>
    /// <remarks>
    /// Extracted so <see cref="RecordCompletedAsync" /> creates activities by exactly the same rules as
    /// <see cref="CreateDraftAsync" /> rather than by a second, drifting copy of them (T121). The
    /// per-create work that is NOT here is the work that must not be repeated per row in a batch: the
    /// type lookup and the subject-scope resolution, both of which are the same for every row.
    /// </remarks>
    private Activity BuildDraftActivity(
        ActivityType activityType,
        FormSchema schema,
        Workflow workflow,
        string subjectUserId,
        (int? InstitutionId, int? SpecialityId, int? SubSpecialityId) subjectScope,
        string createdByUserId,
        string initialDataJson,
        ClaimsPrincipal principal,
        DateTime utcNow)
    {
        var submittedDataJson = NormalizeObjectJson(initialDataJson);

        var activity = new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = subjectUserId,
            CreatedByUserId = createdByUserId.Trim(),
            CurrentState = workflow.InitialState,
            // Deliberately EMPTY while the writable set is computed below. See the note there.
            DataJson = EmptyObjectJson,
            // T101: the activity's own organisational home, snapshotted now. Read authorization and
            // every `scope:` actor rule resolve from these rather than from the activity type, so
            // oversight follows the trainee the assessment is about. See Activity.InstitutionId.
            InstitutionId = subjectScope.InstitutionId,
            SpecialityId = subjectScope.SpecialityId,
            SubSpecialityId = subjectScope.SubSpecialityId,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        };

        // T070: the creator may only supply the fields they own, so a trainee cannot pre-fill the
        // assessor's ratings and make `complete` satisfiable before the assessor ever sees the form.
        // Keys outside the writable set (including keys the schema does not declare at all) are
        // dropped silently rather than rejected — a create is not a patch, and rejecting would break
        // every caller that echoes a full form.
        //
        // The permission check runs against EMPTY data, not against what the caller submitted. A
        // `field:` rule reads its answer out of DataJson, so evaluating it against the caller's own
        // payload lets the caller grant themselves the rule: name yourself in `assessor_user_id`
        // and the assessor-owned sections unlock, self-rating and all. At creation nobody is bound
        // by a data field yet, so the only rules that can match here are the ones backed by real
        // columns (`subject`, `creator`), roles and scope.
        //
        // The state gate is ignored here because procedure_log and journal_club declare a terminal
        // initial state; gating on it would make those types uncreatable.
        var writableFieldKeys = _fieldPermissionEvaluator.GetWritableFieldKeys(
            schema,
            workflow,
            activity,
            principal,
            ignoreStateGate: true);

        var normalizedDataJson = FilterToWritableKeys(submittedDataJson, writableFieldKeys);
        ThrowIfActorFieldNamesSubject(schema, workflow, normalizedDataJson, activity.SubjectUserId);
        activity.DataJson = normalizedDataJson;

        ThrowIfInvalid(_schemaValidator.Validate(schema, normalizedDataJson, SchemaValidationMode.Draft));

        // T119: after the writable-key filter, so the stamp reflects what was actually stored.
        StampObservedOn(activity, schema, normalizedDataJson);

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = workflow.InitialState,
            ToState = workflow.InitialState,
            TransitionKey = "create",
            ActorUserId = createdByUserId.Trim(),
            OccurredOn = utcNow,
            SnapshotJson = normalizedDataJson
        });

        return activity;
    }

    public async Task<ActivityDto> UpdateDraftAsync(UpdateActivityDraftInput input, CancellationToken cancellationToken = default)
    {
        var activity = await LoadActivityAsync(input.ActivityId, cancellationToken);
        var version = GetPinnedVersion(activity);
        var workflow = WorkflowParser.Parse(version.WorkflowJson);
        var currentState = workflow.States.Single(state => string.Equals(state.Key, activity.CurrentState, StringComparison.Ordinal));

        if (currentState.Terminal)
        {
            throw new InvalidOperationException("Terminal activities cannot be edited.");
        }

        if (!CanEditDraft(activity, input.ActorUserId))
        {
            throw new InvalidOperationException("The current actor is not allowed to edit this activity.");
        }

        var schema = FormSchemaParser.Parse(version.SchemaJson);
        var normalizedDataJson = NormalizeObjectJson(input.NewDataJson);
        ThrowIfInvalid(_schemaValidator.Validate(schema, normalizedDataJson, SchemaValidationMode.Draft));

        activity.DataJson = normalizedDataJson;
        activity.UpdatedOn = DateTime.UtcNow;

        // T119: a trainee correcting the encounter date before submitting must not leave a stale stamp.
        StampObservedOn(activity, schema, normalizedDataJson);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    public async Task<ActivityDto> TransitionAsync(TransitionActivityInput input, CancellationToken cancellationToken = default)
    {
        var activity = await LoadActivityAsync(input.ActivityId, cancellationToken);
        var version = GetPinnedVersion(activity);
        var schema = FormSchemaParser.Parse(version.SchemaJson);
        var workflow = WorkflowParser.Parse(version.WorkflowJson);
        var transition = workflow.Transitions.SingleOrDefault(candidate =>
            string.Equals(candidate.Key, input.TransitionKey, StringComparison.Ordinal) &&
            candidate.From.Contains(activity.CurrentState, StringComparer.Ordinal))
            ?? throw new InvalidOperationException(
                $"Transition '{input.TransitionKey}' is not available from state '{activity.CurrentState}'.");

        var decision = _workflowEvaluator.Evaluate(workflow, activity, input.TransitionKey, input.Principal);
        if (!decision.Allowed)
        {
            throw new InvalidOperationException(decision.Reason ?? "The current actor is not allowed to perform this transition.");
        }

        if (transition.RequiresNote && string.IsNullOrWhiteSpace(input.Note))
        {
            throw new InvalidOperationException($"Transition '{input.TransitionKey}' requires a note.");
        }

        // The writable set is computed AFTER the transition gate above and BEFORE the merge below,
        // against the PRE-transition state and the PRE-patch data. Both orderings are load-bearing:
        // a patch must not be able to authorise its own transition, and the actor's permissions are
        // those of the state they are acting from, not the state they are moving to.
        var mergedDataJson = activity.DataJson;
        if (!string.IsNullOrWhiteSpace(input.DataPatchJson))
        {
            var writableFieldKeys = _fieldPermissionEvaluator.GetWritableFieldKeys(
                schema,
                workflow,
                activity,
                input.Principal);

            mergedDataJson = MergeWritableKeys(
                activity.DataJson,
                input.DataPatchJson,
                writableFieldKeys,
                activity.CurrentState);

            // The subject legitimately owns `assessor_user_id` while the request is still theirs to
            // edit, so the merge above will happily accept a patch that points it at themselves —
            // and from the next state on, every `field:assessor_user_id` rule would match them.
            ThrowIfActorFieldNamesSubject(schema, workflow, mergedDataJson, activity.SubjectUserId);
        }

        ThrowIfInvalid(_schemaValidator.Validate(
            schema,
            mergedDataJson,
            SchemaValidationMode.Submit,
            transition.RequiresFields));

        // T119. Ordering here is load-bearing twice: stamping BEFORE ApplyTransition keeps the column and
        // the transition's SnapshotJson in agreement, and stamping before the credit call below is the
        // whole point of the task — CreditApplier picks the curriculum item's effective minimum from the
        // stage the trainee was in ON THE ENCOUNTER DATE.
        StampObservedOn(activity, schema, mergedDataJson);

        var record = activity.ApplyTransition(workflow, input.TransitionKey, input.ActorUserId, mergedDataJson, input.Note);

        await ApplyCreditIfTerminalAsync(activity, version, workflow, transition, record, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    /// <summary>
    /// Applies curriculum credit for a move into a terminal state and stamps the outcome onto the
    /// transition that caused it, or does neither.
    /// </summary>
    /// <remarks>
    /// The one credit entry point, called from <see cref="TransitionAsync" /> and
    /// <see cref="RecordCompletedAsync" />. Forking it was the option T121 rejected: each fork would
    /// then need its own copy of the scale resolution, the stage resolution, the dedupe key namespace
    /// and the T108 stamp, and the second copy is the one that gets forgotten.
    /// </remarks>
    private async Task ApplyCreditIfTerminalAsync(
        Activity activity,
        ActivityTypeVersion version,
        Workflow workflow,
        WorkflowTransition transition,
        ActivityTransition record,
        CancellationToken cancellationToken)
    {
        var targetState = workflow.States.Single(state => string.Equals(state.Key, transition.To, StringComparison.Ordinal));
        if (targetState.Terminal && DeclaresCredit(version.CreditRulesJson))
        {
            // Stamp the outcome onto the transition that caused it. Until T108 this result was
            // discarded, which made "credited nothing" indistinguishable from "credited" at every
            // surface in the product: no return value read, no domain event, no log line.
            //
            // The `counts_for` gate is checked BEFORE the call, not after, and that is what stops the
            // signal crying wolf: a reflective note, journal club, procedure log, QI project, research
            // output or teaching session declares an empty `counts_for`, so it is never evaluated and
            // its transition stays null for ever.
            //
            // Both writes land in the SaveChangesAsync below, so the stamp is atomic with the credit
            // it describes.
            //
            // Stamped on the transition path only, and that is a GAP rather than a design.
            // RebuildCurriculumProgress deletes every progress row before replaying
            // (RebuildCurriculumProgressCommand.cs:30-32), so it re-credits properly — meaning a stale
            // zero left by a since-corrected curriculum is never cleared, and the warning banner stays
            // on for ever after the exact remediation it tells the reader to perform. Making the
            // rebuild stamp is the fix; recorded as T106 item 12.
            var credited = await _creditApplier.ApplyAsync(
                activity,
                new ActivityType
                {
                    CreditRulesJson = version.CreditRulesJson,
                    // The pinned schema, not the live one: it declares the `scale_key` of the field the
                    // credit directive gates on, and that is what binds the achieved ordinal to a ladder
                    // (T109). Pinning means the binding cannot drift under the activity.
                    SchemaJson = version.SchemaJson
                },
                cancellationToken);

            record.CreditedItemCount = credited.UpdatedRows.Count;
            record.CreditScaleMismatchCount = credited.ScaleMismatchCount;
        }
    }

    /// <inheritdoc />
    public async Task<int> StageCompletedAsync(
        RecordCompletedActivitiesInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.DataJsonPerActivity.Count == 0)
        {
            return 0;
        }

        var activityTypeKey = input.ActivityTypeKey.Trim();
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Key == activityTypeKey, cancellationToken)
            ?? throw new InvalidOperationException($"The activity type '{activityTypeKey}' could not be found.");

        var (schema, workflow) = ParsePublished(activityType);

        if (DeclaresCredit(activityType.CreditRulesJson ?? string.Empty))
        {
            // See the interface remarks: credit is keyed on the activity id, which does not exist until
            // a save, and this method deliberately does not save. Refusing loudly beats crediting once
            // for a whole batch under the key "0:...".
            throw new InvalidOperationException(
                $"Activity type '{activityTypeKey}' declares curriculum credit, so it cannot be recorded " +
                "through the staged path. Credit is keyed on the persisted activity id.");
        }

        // Resolved from the INITIAL state, because that is where every row in the batch starts. A type
        // whose initial state is already terminal has nothing to transition and does not belong here.
        var transition = workflow.Transitions.SingleOrDefault(candidate =>
            string.Equals(candidate.Key, input.TransitionKey, StringComparison.Ordinal) &&
            candidate.From.Contains(workflow.InitialState, StringComparer.Ordinal))
            ?? throw new InvalidOperationException(
                $"Transition '{input.TransitionKey}' is not available from state '{workflow.InitialState}'.");

        var subjectUserId = input.SubjectUserId.Trim();
        var subjectScope = await ResolveSubjectScopeAsync(subjectUserId, cancellationToken);

        EnsureSubjectIsInTypeScope(activityType, subjectScope, subjectUserId);

        var utcNow = DateTime.UtcNow;

        // Built and validated in full BEFORE anything is added to the context. Nothing below this loop
        // may throw, because by then the caller's own mutation is pending and an exception would be
        // flushed to the database by the audit pipeline's catch. (See the interface remarks.)
        var built = new List<Activity>(input.DataJsonPerActivity.Count);
        foreach (var dataJson in input.DataJsonPerActivity)
        {
            var activity = BuildDraftActivity(
                activityType,
                schema,
                workflow,
                subjectUserId,
                subjectScope,
                input.CreatedByUserId,
                dataJson,
                input.Principal,
                utcNow);

            // The same evaluator the interactive path uses, against the same rule the seed declares.
            // This is the gate: `msf_cpsa` says `role:Coordinator|role:Administrator`, so a trainee who
            // has hand-created a stray draft from /activities/new can never complete it, and therefore
            // never record MSF evidence about themselves.
            var decision = _workflowEvaluator.Evaluate(workflow, activity, transition.Key, input.Principal);
            if (!decision.Allowed)
            {
                throw new InvalidOperationException(
                    decision.Reason ?? "The current actor is not allowed to perform this transition.");
            }

            ThrowIfInvalid(_schemaValidator.Validate(
                schema,
                activity.DataJson,
                SchemaValidationMode.Submit,
                transition.RequiresFields));

            // T119, and the ordering note from TransitionAsync applies unchanged: stamping before
            // ApplyTransition keeps the column and the transition's SnapshotJson in agreement.
            StampObservedOn(activity, schema, activity.DataJson);

            activity.ApplyTransition(
                workflow,
                transition.Key,
                input.CreatedByUserId,
                activity.DataJson,
                note: null);

            built.Add(activity);
        }

        _dbContext.Set<Activity>().AddRange(built);

        return built.Count;
    }

    /// <summary>
    /// Refuses to write a scoped type's record about a subject outside that scope. (T121)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The interactive path never needs this: <c>ListActivityTypesQuery</c> only offers a
    /// Speciality-scoped type to someone whose claims carry that speciality, so the picker is the
    /// narrowing. A system-written record has no picker, and the caller's own subject list may be
    /// scoped differently — the MSF trainee picker is scoped by institution, not by speciality — so the
    /// narrowing has to be applied here or not at all.
    /// </para>
    /// <para>
    /// Without it, an institution running Paediatrics alongside another discipline could run an MSF
    /// campaign for a non-paediatric registrar and stamp <c>msf_cpsa</c> records, carrying an ordinal
    /// pinned to the CPSA ladder, onto a trainee whose curriculum measures on a different one. D8's
    /// empty <c>counts_for</c> removes the only thing that would otherwise have caught it, because
    /// <c>CreditApplier</c>'s <c>ScaleMismatch</c> refusal is never reached.
    /// </para>
    /// <para>
    /// A subject whose scope did not resolve is refused rather than admitted: an unstamped record
    /// satisfies no <c>scope:</c> rule and would be readable only by the people named on it.
    /// </para>
    /// </remarks>
    private static void EnsureSubjectIsInTypeScope(
        ActivityType activityType,
        (int? InstitutionId, int? SpecialityId, int? SubSpecialityId) subjectScope,
        string subjectUserId)
    {
        var matches = activityType.Scope switch
        {
            ActivityScope.Global => true,
            ActivityScope.Institution => subjectScope.InstitutionId == activityType.ScopeId,
            ActivityScope.Speciality => subjectScope.SpecialityId == activityType.ScopeId,
            ActivityScope.SubSpeciality => subjectScope.SubSpecialityId == activityType.ScopeId,
            _ => false
        };

        if (!matches)
        {
            throw new InvalidOperationException(
                $"'{activityType.Key}' is scoped to {activityType.Scope} {activityType.ScopeId}, and " +
                $"'{subjectUserId}' does not train there, so no record of it can be written about them.");
        }
    }

    public async Task<ActivityDetailDto?> GetDetailAsync(
        int activityId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var activity = await FindActivityAsync(activityId, cancellationToken);
        if (activity is null || !IsReadableBy(activity, principal))
        {
            // One outcome for "no such activity" and "not yours", so incrementing the id in the
            // address bar cannot enumerate. The Web layer renders null as "Activity unavailable";
            // it must not be turned back into a message that distinguishes the two. (T101)
            return null;
        }

        var version = GetPinnedVersion(activity);
        var schema = FormSchemaParser.Parse(version.SchemaJson);
        var workflow = WorkflowParser.Parse(version.WorkflowJson);

        var writableFieldKeys = _fieldPermissionEvaluator.GetWritableFieldKeys(schema, workflow, activity, principal);

        // Evaluated against the REAL loaded ActivityType, which LoadActivityAsync Includes. The Web
        // layer used to build a synthetic ActivityType for this, defaulting Scope to Global and
        // ScopeId to null, so every `scope:` rule evaluated false in the button list while the server
        // would have allowed the transition. (T070)
        var availableActions = workflow.Transitions
            .Where(transition => transition.From.Contains(activity.CurrentState, StringComparer.Ordinal))
            .Where(transition => _workflowEvaluator.Evaluate(workflow, activity, transition.Key, principal).Allowed)
            .Select(transition => new ActivityActionDto(transition.Key, transition.RequiresNote))
            .ToList();

        return new ActivityDetailDto(
            Map(activity),
            OrderBySchema(schema, writableFieldKeys),
            availableActions);
    }

    /// <summary>
    /// Whether the pinned version declares any credit at all — the anti-cry-wolf gate for the T108
    /// signal, and the same test <c>CreditApplier</c> makes first thing.
    /// </summary>
    /// <remarks>
    /// Blank rules count as "credits nothing" rather than an error: a type published without a credit
    /// block should still be completable, and before T108 an empty string here threw out of the
    /// terminal transition.
    /// </remarks>
    private static bool DeclaresCredit(string creditRulesJson)
        => !string.IsNullOrWhiteSpace(creditRulesJson) &&
           CreditRulesParser.Parse(creditRulesJson).CountsFor.Count > 0;

    /// <summary>Writable keys in schema declaration order, so the UI can render them predictably.</summary>
    private static IReadOnlyList<string> OrderBySchema(FormSchema schema, IReadOnlySet<string> writableFieldKeys)
        => schema.Sections
            .SelectMany(section => section.Fields)
            .Select(field => field.Key)
            .Where(writableFieldKeys.Contains)
            .ToList();

    private async Task<Activity> LoadActivityAsync(int activityId, CancellationToken cancellationToken)
        => await FindActivityAsync(activityId, cancellationToken)
            ?? throw new InvalidOperationException("The activity could not be found.");

    private async Task<Activity?> FindActivityAsync(int activityId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Activity>()
            .Include(entity => entity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(entity => entity.Transitions)
            .SingleOrDefaultAsync(entity => entity.Id == activityId, cancellationToken);
    }

    /// <summary>
    /// Whether this principal may read this activity at all. (T101)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before T101 the only check on the read path was a null check on the principal, so any
    /// authenticated user — including a PendingTrainee holding no programme role — could read any
    /// activity's clinical data and full transition history by putting an integer in the URL.
    /// </para>
    /// <para>
    /// The set is deliberately a SUPERSET of everyone who can act on the activity. A read gate
    /// narrower than the write gate produces buttons that 404 and inbox rows that cannot be opened,
    /// so the declared-rule arm below asks <see cref="ActorRuleMatcher" /> — the same matcher
    /// <see cref="WorkflowEvaluator" /> and <see cref="FieldPermissionEvaluator" /> use — about every
    /// rule the pinned version declares anywhere, with the state gate dropped. If any rule could ever
    /// name this principal in any state, they may read it now.
    /// </para>
    /// <para>
    /// Scope arms resolve from the activity's own stamped columns, never from the activity type's
    /// scope: oversight follows the trainee the assessment is about. An activity with no stamped
    /// scope satisfies no scoped arm.
    /// </para>
    /// </remarks>
    private bool IsReadableBy(Activity activity, ClaimsPrincipal principal)
    {
        if (principal.IsAdministrator())
        {
            return true;
        }

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(callerUserId) &&
            (string.Equals(activity.SubjectUserId, callerUserId, StringComparison.Ordinal) ||
             string.Equals(activity.CreatedByUserId, callerUserId, StringComparison.Ordinal) ||
             // Anyone who has already moved this activity keeps sight of what they did. Without this
             // an assessor who declines a WBA loses the record of their own decision the moment the
             // trainee re-assigns the assessor field.
             activity.Transitions.Any(transition =>
                 string.Equals(transition.ActorUserId, callerUserId, StringComparison.Ordinal))))
        {
            return true;
        }

        if (IsScopedOverseerOf(activity, principal))
        {
            return true;
        }

        return MatchesAnyDeclaredActorRule(activity, principal);
    }

    /// <summary>
    /// Programme oversight: the roles that supervise a trainee may read that trainee's assessments,
    /// each at the level of the tree they are scoped to. A null stamp never matches. (T101)
    /// </summary>
    /// <remarks>
    /// EVERY arm carries the institution, including the speciality ones. A speciality is owned by a
    /// College and is therefore a NATIONAL id (<see cref="Wombat.Domain.Institutions.Speciality.CollegeId" />),
    /// so <c>IsInSpeciality</c> alone would let one hospital's SpecialityAdmin read every paediatric
    /// trainee in the country. CLAUDE.md is explicit that a SpecialityAdmin is scoped to one speciality
    /// *within an institution*; the claim pair expresses that, neither claim on its own does.
    /// </remarks>
    private static bool IsScopedOverseerOf(Activity activity, ClaimsPrincipal principal)
    {
        if (activity.InstitutionId is not int institutionId ||
            principal.GetInstitutionId() != institutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin() ||
            principal.IsInRole(WombatRoles.Coordinator) ||
            principal.IsInRole(WombatRoles.CommitteeMember))
        {
            return true;
        }

        if (activity.SpecialityId is int specialityId &&
            principal.IsInRole(WombatRoles.SpecialityAdmin) &&
            principal.IsInSpeciality(specialityId))
        {
            return true;
        }

        return activity.SubSpecialityId is int subSpecialityId &&
               principal.IsInRole(WombatRoles.SubSpecialityAdmin) &&
               principal.IsInSubSpeciality(subSpecialityId);
    }

    /// <summary>
    /// Whether any actor rule the pinned version declares — on a transition, a state, a section or a
    /// field — names this principal, ignoring which state the activity is actually in. (T101)
    /// </summary>
    /// <remarks>
    /// A bare <c>role:</c> rule is NOT honoured here. Unqualified, it would turn any workflow that
    /// says "a Coordinator may approve this" into a grant to read every activity of that type in
    /// every institution — a read gate widened by an unrelated seed edit. A role token only counts
    /// when it is conjoined with something that binds it to this activity, which is what every rule
    /// in the seed corpus does today (<c>role:SpecialityAdmin+scope:speciality</c>). Role-only
    /// oversight is granted by <see cref="IsScopedOverseerOf" />, where the scope is explicit.
    /// </remarks>
    private static bool MatchesAnyDeclaredActorRule(Activity activity, ClaimsPrincipal principal)
    {
        ActivityTypeVersion? version;
        FormSchema schema;
        Wombat.Domain.Activities.Workflow.Workflow workflow;

        try
        {
            version = GetPinnedVersion(activity);
            schema = FormSchemaParser.Parse(version.SchemaJson);
            workflow = WorkflowParser.Parse(version.WorkflowJson);
        }
        // SchemaParseException and WorkflowParseException derive from Exception directly, and the
        // parsers WRAP JsonException into them — so filtering on JsonException here caught nothing a
        // parser actually throws, and a malformed pinned version escaped to ActivityView as a parser
        // message while a nonexistent id rendered "Activity unavailable". That told the two apart,
        // which is the one thing this method exists to prevent. ArgumentException covers a blank
        // SchemaJson reaching ThrowIfNullOrWhiteSpace.
        catch (Exception exception) when (exception is InvalidOperationException
                                              or JsonException
                                              or SchemaParseException
                                              or WorkflowParseException
                                              or ArgumentException)
        {
            // An unreadable pinned version must not become a disclosure: refuse rather than reveal
            // that the id exists and its schema is broken. A caller admitted by an arm above still
            // gets the real exception, because this runs only after those have all declined.
            return false;
        }

        return DeclaredActorRules(schema, workflow)
            .Select(DropUnqualifiedRoleArms)
            .Any(rule => rule is not null && ActorRuleMatcher.Matches(rule, activity, principal));
    }

    private static IEnumerable<ActorRule> DeclaredActorRules(FormSchema schema, Workflow workflow)
    {
        foreach (var transition in workflow.Transitions)
        {
            yield return transition.Actor;
        }

        foreach (var state in workflow.States)
        {
            if (state.EditableBy is not null)
            {
                yield return state.EditableBy;
            }
        }

        foreach (var section in schema.Sections)
        {
            if (section.EditableBy is not null)
            {
                yield return section.EditableBy;
            }

            foreach (var field in section.Fields)
            {
                if (field.EditableBy is not null)
                {
                    yield return field.EditableBy;
                }
            }
        }
    }

    /// <summary>
    /// Rewrites a declared rule into the part of it that may grant a READ, or null if none of it may.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A role token with nothing conjoined to bind it to this activity is dropped. Unqualified, it
    /// would turn "a Coordinator may approve this" in one seed into a grant to read every activity of
    /// that type in every institution — a read boundary widened by an unrelated seed edit. Scoped
    /// oversight is granted by <see cref="IsScopedOverseerOf" />, where the scope is explicit.
    /// </para>
    /// <para>
    /// The rewrite is per ARM, not per rule. An earlier version answered "does this rule contain an
    /// unqualified role?" and discarded the whole rule if so — which threw away the bound arms with
    /// it. <c>field:assessor_user_id|role:Coordinator</c> parses as one <c>Any</c> node, so the named
    /// assessor lost their read while <see cref="WorkflowEvaluator" /> still let them act: the row
    /// appeared in their inbox and opening it said "Activity unavailable". That is precisely the
    /// buttons-that-404 failure this class exists to prevent, caused by the guard against it.
    /// </para>
    /// <para>
    /// <c>Any</c> keeps its qualified arms. <c>All</c> is kept only when at least one conjunct binds
    /// to the activity — <c>role:A+role:B</c> binds nothing, and neither does <c>role:X+scope:global</c>,
    /// whose scope token reads the activity TYPE and so is the same for every activity of that type.
    /// </para>
    /// </remarks>
    private static ActorRule? DropUnqualifiedRoleArms(ActorRule rule)
    {
        switch (rule)
        {
            case NamedRoleActorRule:
                return null;

            case CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.Any:
            {
                var kept = combined.Rules
                    .Select(DropUnqualifiedRoleArms)
                    .Where(child => child is not null)
                    .Select(child => child!)
                    .ToList();

                return kept.Count switch
                {
                    0 => null,
                    1 => kept[0],
                    _ => combined with { Rules = kept }
                };
            }

            case CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.All:
                // Kept whole — narrowing a conjunction would WIDEN it — but only if something in it
                // actually binds to this activity.
                return combined.Rules.Any(BindsToTheActivity) ? combined : null;

            default:
                return rule;
        }
    }

    /// <summary>Whether a rule ties its answer to this particular activity rather than to its type.</summary>
    private static bool BindsToTheActivity(ActorRule rule)
        => rule switch
        {
            SubjectUserActorRule or CreatorUserActorRule or FieldUserActorRule => true,
            ScopeMatchActorRule scope => !string.Equals(scope.Scope, "global", StringComparison.Ordinal),
            CombinedActorRule combined => combined.Rules.Any(BindsToTheActivity),
            _ => false
        };

    /// <summary>Stamps the encounter date from the pinned schema. Safe to call repeatedly. (T119)</summary>
    private static void StampObservedOn(Activity activity, FormSchema schema, string dataJson)
        => ObservationDateResolver.Stamp(activity, schema, dataJson);

    /// <summary>
    /// Where the subject trains, read once at creation and stamped onto the activity. (T101)
    /// </summary>
    /// <remarks>
    /// Prefers the active profile; a trainee who has graduated or been withdrawn keeps their most
    /// recent one, so activities logged afterwards still carry a scope. Null for a subject with no
    /// profile at all, which withholds scoped oversight rather than granting it.
    /// </remarks>
    private async Task<(int? InstitutionId, int? SpecialityId, int? SubSpecialityId)> ResolveSubjectScopeAsync(
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return (null, null, null);
        }

        // Resolved one level at a time, NOT as a single join through
        // TraineeProfile -> Curriculum -> SubSpeciality. Those navigations are required, so one query
        // would be an INNER join: a curriculum row that has gone missing would take the institution
        // down with it, even though the institution sits on the profile itself. Each level degrades
        // on its own instead, and the most important stamp — the institution — survives the other two
        // failing. Three primary-key lookups, once, on a create.
        var profile = await _dbContext.Set<TraineeProfile>()
            .Where(entity => entity.UserId == subjectUserId)
            .OrderByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.Id)
            .Select(entity => new { entity.InstitutionId, entity.CurriculumId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return await ResolveScopeFromIdentityAsync(subjectUserId, cancellationToken);
        }

        var subSpecialityId = await _dbContext.Set<Curriculum>()
            .Where(entity => entity.Id == profile.CurriculumId)
            .Select(entity => (int?)entity.SubSpecialityId)
            .FirstOrDefaultAsync(cancellationToken);

        var specialityId = subSpecialityId is null
            ? null
            : await _dbContext.Set<SubSpeciality>()
                .Where(entity => entity.Id == subSpecialityId.Value)
                .Select(entity => (int?)entity.SpecialityId)
                .FirstOrDefaultAsync(cancellationToken);

        return (profile.InstitutionId, specialityId, subSpecialityId);
    }

    /// <summary>
    /// The fallback for a subject who is not an admitted trainee: their own identity record. (T101)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not every activity is about a trainee. An invited user is given an institution and speciality
    /// scopes by <c>InvitedUserProvisioner</c> at acceptance, while a <c>TraineeProfile</c> is created
    /// only later by <c>AdmitTrainee</c> — and nothing stops them filing a reflective note in between.
    /// </para>
    /// <para>
    /// Without this, such an activity was stamped with nothing, and the seeded reflective-note family
    /// offers only <c>role:SpecialityAdmin+scope:speciality</c> out of <c>submitted</c>. A null stamp
    /// matches no <c>scope:</c> rule for anybody, and neither <see cref="WorkflowEvaluator" /> nor
    /// <c>TransitionAsync</c> has an Administrator bypass — so the row was frozen in <c>submitted</c>
    /// for ever, unreadable by the admin who should have acted on it. Reading the same facts the login
    /// claims are issued from is not a guess; it is the same answer one step earlier.
    /// </para>
    /// <para>
    /// A user holding several speciality scopes yields null rather than an arbitrary pick: the column
    /// holds one id, and choosing between them would be inventing an answer. That leaves the residual
    /// frozen-row case for a subject with no institution at all — see T116.
    /// </para>
    /// </remarks>
    private async Task<(int? InstitutionId, int? SpecialityId, int? SubSpecialityId)> ResolveScopeFromIdentityAsync(
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        var institutionId = await _dbContext.Set<WombatIdentityUser>()
            .Where(entity => entity.Id == subjectUserId)
            .Select(entity => entity.InstitutionId)
            .FirstOrDefaultAsync(cancellationToken);

        var specialityIds = await _dbContext.Set<WombatIdentityUserSpecialityScope>()
            .Where(entity => entity.UserId == subjectUserId)
            .Select(entity => entity.SpecialityId)
            .Distinct()
            .Take(2)
            .ToListAsync(cancellationToken);

        var subSpecialityIds = await _dbContext.Set<WombatIdentityUserSubSpecialityScope>()
            .Where(entity => entity.UserId == subjectUserId)
            .Select(entity => entity.SubSpecialityId)
            .Distinct()
            .Take(2)
            .ToListAsync(cancellationToken);

        return (
            institutionId,
            specialityIds.Count == 1 ? specialityIds[0] : null,
            subSpecialityIds.Count == 1 ? subSpecialityIds[0] : null);
    }

    private static ActivityTypeVersion GetPinnedVersion(Activity activity)
    {
        return activity.ActivityType.Versions.SingleOrDefault(entity => entity.Version == activity.SchemaVersion)
            ?? throw new InvalidOperationException(
                $"The published activity type version '{activity.SchemaVersion}' could not be found.");
    }

    private static bool CanEditDraft(Activity activity, string actorUserId)
        => string.Equals(activity.SubjectUserId, actorUserId, StringComparison.Ordinal) ||
           string.Equals(activity.CreatedByUserId, actorUserId, StringComparison.Ordinal);

    private static void ThrowIfInvalid(IReadOnlyList<ActivityValidationErrorDto> validationErrors)
    {
        if (validationErrors.Count == 0)
        {
            return;
        }

        var message = string.Join("; ", validationErrors.Select(error =>
            error.FieldKey is null
                ? error.Message
                : $"{error.FieldKey}: {error.Message}"));

        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Refuses data in which a field that decides who may act on the activity names the activity's
    /// own subject.
    /// </summary>
    /// <remarks>
    /// A <c>field:</c> rule — <c>field:assessor_user_id</c> on the CPSA seeds — makes whoever that
    /// field names the actor for a transition and, since T070, the owner of the fields that
    /// transition fills in. The subject legitimately owns that field while the request is still
    /// theirs to edit, which is the whole escalation: name yourself, and you may rate yourself and
    /// take your own <c>complete</c>, awarding your own curriculum credit. Nothing else in the
    /// pipeline catches it — <c>SchemaValidator</c> treats a <c>user</c> field as a plain string
    /// and the assessor picker is a UI affordance, not a check.
    ///
    /// This is the narrow guard. The general one — validating a <c>user</c> value against the users
    /// the caller may legitimately nominate — is T102.
    /// </remarks>
    private static void ThrowIfActorFieldNamesSubject(
        FormSchema schema,
        Workflow workflow,
        string dataJson,
        string subjectUserId)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return;
        }

        var actorFieldNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var transition in workflow.Transitions)
        {
            ActorRuleMatcher.CollectUserFieldNames(transition.Actor, actorFieldNames);
        }

        foreach (var state in workflow.States)
        {
            ActorRuleMatcher.CollectUserFieldNames(state.EditableBy, actorFieldNames);
        }

        foreach (var section in schema.Sections)
        {
            ActorRuleMatcher.CollectUserFieldNames(section.EditableBy, actorFieldNames);
            foreach (var field in section.Fields)
            {
                ActorRuleMatcher.CollectUserFieldNames(field.EditableBy, actorFieldNames);
            }
        }

        foreach (var fieldName in actorFieldNames)
        {
            var value = ActorRuleMatcher.ReadUserFieldValue(dataJson, fieldName);
            if (string.Equals(value, subjectUserId, StringComparison.Ordinal))
            {
                var label = schema.Sections
                    .SelectMany(section => section.Fields)
                    .FirstOrDefault(field => string.Equals(field.Key, fieldName, StringComparison.Ordinal))
                    ?.Label ?? fieldName;

                throw new InvalidOperationException(
                    $"{label}: this decides who may act on the activity, so it cannot name the person the activity is about.");
            }
        }
    }

    private static string NormalizeObjectJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Activity data must be a JSON object.");
        }

        return JsonSerializer.Serialize(document.RootElement);
    }

    /// <summary>
    /// Shallow top-level merge restricted to the keys this actor owns (T070).
    /// </summary>
    /// <remarks>
    /// A key the actor may write is applied. A key they may not write is ignored when the patch
    /// carries the value already stored — full-form post-backs echo every field, including locked
    /// ones, and without that carve-out the rule would fire on untouched data. A key they may not
    /// write whose value actually differs throws: that is the case where an assessor's `complete`
    /// patch would otherwise rewrite <c>epa_id</c> or <c>assessor_user_id</c> and redirect which
    /// curriculum item gets credited.
    /// </remarks>
    private static string MergeWritableKeys(
        string currentJson,
        string patchJson,
        IReadOnlySet<string> writableFieldKeys,
        string currentState)
    {
        using var currentDocument = JsonDocument.Parse(currentJson);
        using var patchDocument = JsonDocument.Parse(patchJson);

        if (currentDocument.RootElement.ValueKind != JsonValueKind.Object ||
            patchDocument.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Activity data and patch data must both be JSON objects.");
        }

        var merged = currentDocument.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);

        foreach (var property in patchDocument.RootElement.EnumerateObject())
        {
            if (writableFieldKeys.Contains(property.Name))
            {
                merged[property.Name] = property.Value.Clone();
                continue;
            }

            var isUnchanged = merged.TryGetValue(property.Name, out var storedValue) &&
                              JsonElement.DeepEquals(storedValue, property.Value);

            if (!isUnchanged)
            {
                throw new InvalidOperationException(
                    $"Field '{property.Name}' cannot be written in state '{currentState}' by the current actor.");
            }
        }

        return JsonSerializer.Serialize(merged);
    }

    /// <summary>
    /// Drops every key the actor does not own. Used at creation, where silently ignoring an
    /// unowned key is right (see <see cref="CreateDraftAsync" />).
    /// </summary>
    private static string FilterToWritableKeys(string dataJson, IReadOnlySet<string> writableFieldKeys)
    {
        using var document = JsonDocument.Parse(dataJson);

        var retained = document.RootElement.EnumerateObject()
            .Where(property => writableFieldKeys.Contains(property.Name))
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);

        return JsonSerializer.Serialize(retained);
    }

    private static ActivityDto Map(Activity activity)
    {
        var pinnedVersion = GetPinnedVersion(activity);

        return new ActivityDto(
            activity.Id,
            activity.ActivityTypeId,
            activity.ActivityType.Key,
            activity.ActivityType.Name,
            activity.SchemaVersion,
            pinnedVersion.SchemaJson,
            pinnedVersion.WorkflowJson,
            pinnedVersion.DisplayFieldsJson,
            pinnedVersion.CreditRulesJson,
            activity.SubjectUserId,
            activity.CreatedByUserId,
            activity.CurrentState,
            activity.DataJson,
            activity.EpaId,
            activity.CurriculumItemId,
            activity.CreatedOn,
            activity.UpdatedOn,
            activity.Transitions
                .OrderBy(entity => entity.OccurredOn)
                .Select(entity => new ActivityTransitionDto(
                    entity.Id,
                    entity.FromState,
                    entity.ToState,
                    entity.TransitionKey,
                    entity.ActorUserId,
                    entity.OccurredOn,
                    entity.Note,
                    entity.SnapshotJson,
                    entity.CreditedItemCount,
                    entity.CreditScaleMismatchCount))
                .ToList());
    }
}
