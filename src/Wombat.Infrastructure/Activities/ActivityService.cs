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
    private readonly TimeProvider _timeProvider;

    /// <param name="timeProvider">
    /// The clock every write takes its instant from, and so the South African "today" the encounter date is judged
    /// against (T160). Optional so that a caller without one gets the system clock; a test passes a fixed one.
    /// </param>
    public ActivityService(
        IApplicationDbContext dbContext,
        ISchemaValidator schemaValidator,
        IWorkflowEvaluator workflowEvaluator,
        ICreditApplier creditApplier,
        IFieldPermissionEvaluator fieldPermissionEvaluator,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _schemaValidator = schemaValidator;
        _workflowEvaluator = workflowEvaluator;
        _creditApplier = creditApplier;
        _fieldPermissionEvaluator = fieldPermissionEvaluator;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    public async Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == input.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        // T162. Nobody files a type only the system writes, whoever they are: msf_cpsa's rows come from a released MSF
        // campaign (StageCompletedAsync, which does not ask this). First, before anything is built, so the refusal leaves
        // nothing for the audit pipeline's save to commit. The picker does not offer such a type; this is for a caller
        // that sends its id anyway.
        if (activityType.SystemManaged)
        {
            throw new InvalidOperationException(
                $"'{activityType.Name}' records are written by the system, not filed by hand, so one cannot be created here.");
        }

        var (schema, workflow) = ParsePublished(activityType);

        var subjectUserId = input.SubjectUserId.Trim();
        var subjectScope = await SubjectScopeResolver.ResolveAsync(_dbContext, subjectUserId, cancellationToken);

        var utcNow = UtcNow();
        var activity = await BuildDraftActivityAsync(
            activityType,
            schema,
            workflow,
            subjectUserId,
            subjectScope,
            input.CreatedByUserId,
            input.InitialDataJson,
            input.Principal,
            utcNow,
            cancellationToken);

        var createIsTheFiling = CreateIsTheFiling(workflow, activity, input.Principal);
        if (createIsTheFiling)
        {
            ThrowIfFiledIncomplete(schema, workflow, activity, input.Principal);
        }

        // T160. A create writes the encounter date for the first time, so it is always judged, draft or filing. After the
        // schema's own checks, so an unparseable date fails as a format error first, and before either gate and the Add.
        // The live published rules ARE the pinned version's (BuildDraftActivityAsync pins to activityType.Version), so they
        // decide whether the programme-start bound applies.
        ThrowIfInvalid(schema, await EncounterDateGate.ValidateAsync(
            _dbContext,
            activity,
            schema,
            activity.DataJson,
            ProgrammeCalendar.DateOf(utcNow),
            activityType.CreditRulesJson,
            cancellationToken));

        // T122. A create always writes the credit target, and for a type whose initial state is already `requested`
        // (the legacy WBA shape) the create IS the author's submission: the next move is the assessor's. Gating here
        // also means a refused Submit on /activities/new fails before the draft exists, so it leaves no orphan behind.
        // After BuildDraftActivityAsync, which reads the context but never writes to it, and before Add: see
        // ToolPermissionGate.
        await ToolPermissionGate.EnsurePermittedAsync(
            _dbContext,
            activityType.WbaToolKey,
            // The live published rules ARE the pinned version's: BuildDraftActivityAsync pins to activityType.Version.
            activityType.CreditRulesJson,
            schema,
            activity.SubjectUserId,
            activity.ObservedOn,
            activity.DataJson,
            cancellationToken);

        // T102. Every nominee the create writes is judged: stored is empty, so each non-empty one has changed. Against the
        // subject's institution as stamped, whoever the creator is. After the T122 gate and before Add, like it.
        var requiredRolesByField = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);
        await NomineeGate.EnsurePermittedAsync(
            _dbContext,
            schema,
            requiredRolesByField,
            NomineeGate.ChangedFields(requiredRolesByField.Keys, EmptyObjectJson, activity.DataJson),
            activity.DataJson,
            activity.InstitutionId,
            activity.SubjectUserId,
            cancellationToken);

        // T160, D15. When the create is itself the filing of a type that can credit, its own history row records how late
        // the filing was. The entity is not in the context yet, so this is no mutation of it.
        if (createIsTheFiling && EncounterDatePolicy.CanCredit(activityType.CreditRulesJson))
        {
            RecordFiling(activity, activity.Transitions.Single());
        }

        _dbContext.Set<Activity>().Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(activity);
    }

    /// <summary>
    /// Whether creating this activity files it: no move out of the initial state that leads on is the author's to take
    /// (<see cref="Workflow.TransitionsLeadingOn" />). T127's rule, shared by <see cref="ThrowIfFiledIncomplete" /> and
    /// the lateness record (T160).
    /// </summary>
    private bool CreateIsTheFiling(Workflow workflow, Activity activity, ClaimsPrincipal principal)
        => !workflow.TransitionsLeadingOn(workflow.InitialState)
            .Any(transition => _workflowEvaluator.Evaluate(workflow, activity, transition.Key, principal).Allowed);

    /// <summary>
    /// Whether this move files the activity: the author's FIRST move out of the workflow's initial state that leads on
    /// towards credit (a CPSA <c>submit</c>). The counterpart of <see cref="CreateIsTheFiling" /> for a draft-born type
    /// (T160).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The author's, because out of a type born in <c>requested</c> the assessor's <c>accept</c> also leaves the initial
    /// state and leads on, and there the create was the filing. Asked before the move, while the activity is still in
    /// the state it leaves.
    /// </para>
    /// <para>
    /// The first, because a workflow can bring the activity back: <c>reflective_exercise_cpsa</c>'s supervisor
    /// <c>return</c>s it to <c>draft</c>. The re-submission after that is not a second filing, and recording it as one
    /// would call an encounter filed on day 3 and returned on day 18 "filed 19 days after". Once any move has left the
    /// initial state and led on (<see cref="Workflow.LeftInitialStateLeadingOn" />), whoever made it, nothing later is
    /// the filing. The page asks the same of the same rows (<c>FilingLateness.FiledOnFor</c>).
    /// </para>
    /// <para>
    /// Whether the filing's lateness is RECORDED is a further question, asked by the caller: only a type that can credit
    /// records it (<see cref="EncounterDatePolicy.CanCredit" />). <c>reflective_exercise_cpsa</c> itself credits nothing,
    /// so it records no lateness at all; a crediting type built with the same <c>return</c> is what this rule protects.
    /// </para>
    /// </remarks>
    private static bool IsTheFiling(Workflow workflow, Activity activity, WorkflowTransition transition, string actorUserId)
        => string.Equals(activity.CurrentState, workflow.InitialState, StringComparison.Ordinal) &&
           IsTheAuthor(actorUserId, activity) &&
           workflow.TransitionsLeadingOn(workflow.InitialState).Contains(transition) &&
           !activity.Transitions.Any(row => workflow.LeftInitialStateLeadingOn(row.FromState, row.ToState, row.TransitionKey));

    /// <summary>
    /// Records on the filing's history row how many days after the stated encounter it was filed (T160, D15). Nothing is
    /// recorded when nobody stated the date; see <see cref="ActivityTransition.DaysAfterEncounter" />. Never a refusal.
    /// Called only for a type that can credit (<see cref="EncounterDatePolicy.CanCredit" />), which the caller decides
    /// before its first mutation.
    /// </summary>
    /// <remarks>
    /// Synchronous and total: it runs where nothing may fail, after <c>ApplyTransition</c>. It reads the stamp, which
    /// the caller writes from the same data first, and the South African day of the row's own timestamp, so the figure
    /// agrees with the "today" the future check used: an encounter on or before that day is on or before this one.
    /// </remarks>
    private static void RecordFiling(Activity activity, ActivityTransition filing)
        => filing.DaysAfterEncounter = activity.ObservedOnSource == ObservationDateSource.Declared
            ? EncounterDatePolicy.DaysAfterEncounter(activity.ObservedOn, ProgrammeCalendar.DateOf(filing.OccurredOn))
            : null;

    /// <summary>
    /// Refuses a create that files the activity with a required field of the author's still empty.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Asked only when the create is the author's filing (<see cref="CreateIsTheFiling" />). For a type born in
    /// <c>requested</c> (the generic <c>mini_cex</c>, <c>dops</c>, <c>cbd</c> and <c>acat</c>) the create is the
    /// submission and the next move is the assessor's; for a type born terminal (<c>procedure_log</c>,
    /// <c>journal_club</c>) the create is the whole record. Either way no later move of the author's will check what
    /// they owe, so the create checks it now, as a submit declaring
    /// <c>validation: owned</c> would (T105): the required fields the author may write. Without it a trainee could file
    /// a request that names no assessor, which nobody could then accept or repair: edits travel only with a move, and
    /// the author's one move left is the withdrawal (found in the T127/T148 review).
    /// </para>
    /// <para>
    /// Where the author does have such a move (a draft-born type), the create stays a draft save and checks formats
    /// only; that move checks the rest. Whose move it is is judged against the activity as built, as the transition gate
    /// judges it, so <c>scope:</c> and <c>field:</c> rules read the stamped scope and the filtered data. What the author
    /// owes is read as the create's own filter reads it, against empty data: a field that filter strips is never one the
    /// author could have filled, so it is never one they are asked for.
    /// </para>
    /// </remarks>
    private void ThrowIfFiledIncomplete(FormSchema schema, Workflow workflow, Activity activity, ClaimsPrincipal principal)
    {
        var owned = _fieldPermissionEvaluator.GetWritableFieldKeys(
            schema,
            workflow,
            ProbeInState(activity, workflow.InitialState, EmptyObjectJson),
            principal,
            ignoreStateGate: true);

        ThrowIfInvalid(schema, _schemaValidator.Validate(
            schema, activity.DataJson, SchemaValidationMode.Submit, requiredFieldScope: owned));
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
    /// may write, validated, date-stamped and EPA-stamped — everything but <c>Add</c> and <c>SaveChanges</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extracted so <see cref="StageCompletedAsync" /> creates activities by exactly the same rules as
    /// <see cref="CreateDraftAsync" /> rather than by a second, drifting copy of them (T121). The
    /// per-create work that is NOT here is the work that must not be repeated per row in a batch: the
    /// type lookup and the subject-scope resolution, both of which are the same for every row.
    /// </para>
    /// <para>
    /// Asynchronous since T137, for one read: whether the EPA the data names exists. It still writes nothing to the
    /// context, so both callers can refuse after it without the audit pipeline committing anything.
    /// </para>
    /// </remarks>
    private async Task<Activity> BuildDraftActivityAsync(
        ActivityType activityType,
        FormSchema schema,
        Workflow workflow,
        string subjectUserId,
        (int? InstitutionId, int? SpecialityId, int? SubSpecialityId) subjectScope,
        string createdByUserId,
        string initialDataJson,
        ClaimsPrincipal principal,
        DateTime utcNow,
        CancellationToken cancellationToken)
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

        ThrowIfInvalid(schema, _schemaValidator.Validate(schema, normalizedDataJson, SchemaValidationMode.Draft));

        // T119: after the writable-key filter, so the stamp reflects what was actually stored.
        StampObservedOn(activity, schema, normalizedDataJson);

        // T137: the same rule for the EPA, from the same stored data and the same pinned schema.
        activity.EpaId = await EvidenceEpaResolver.ResolveAsync(_dbContext, schema, normalizedDataJson, cancellationToken);

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

        // T105: the transition says how much of the form it insists on. Against the pre-move state and the merged data.
        ThrowIfInvalid(schema, ValidateForTransition(schema, workflow, activity, transition, input.Principal, mergedDataJson));

        // One instant for the whole move: the date is judged against its South African day, and the history row is
        // stamped with it, so the lateness recorded below counts to the same day the future check used.
        var utcNow = UtcNow();

        // T160: the encounter date's two bounds, as field errors. Which moves judge it is EncounterDateToJudge's decision;
        // see its remarks. After the transition's own validation, so a malformed date fails as a format error first, and
        // before both gates and the first mutation below.
        if (EncounterDateToJudge(workflow, schema, activity, transition, input.Principal, input.ActorUserId, mergedDataJson))
        {
            ThrowIfInvalid(schema, await EncounterDateGate.ValidateAsync(
                _dbContext,
                activity,
                schema,
                mergedDataJson,
                ProgrammeCalendar.DateOf(utcNow),
                version.CreditRulesJson,
                cancellationToken));
        }

        // Asked before the move, while the activity is still in the state the move leaves. Only a type that can credit
        // records its filing's lateness (EncounterDatePolicy.CanCredit); its rules are parsed here, before the first
        // mutation, because RecordFiling runs where nothing may fail.
        var recordsTheFiling = IsTheFiling(workflow, activity, transition, input.ActorUserId) &&
                               EncounterDatePolicy.CanCredit(version.CreditRulesJson);

        // T122, D20: a changed credit target is checked on any move that can still lead to credit; an unchanged one is
        // re-checked only when the author, before anyone else has acted, hands it on while still able to correct it
        // (see DirectivesToJudge). So an assessor's `complete` or `decline`, a sign-off after assessment, or a
        // resubmission after a decline never strands an encounter the trainee filed legitimately. The check runs after the transition's own validation, so a
        // missing EPA fails with the schema's own message first, and before the first mutation below. It reads the
        // MERGED data, because a trainee repairing an in-flight draft sends the new EPA in the same patch as the submit.
        var directivesToJudge = DirectivesToJudge(
            workflow, schema, activity, transition, input.Principal, input.ActorUserId, mergedDataJson, version.CreditRulesJson);
        if (directivesToJudge.Count > 0)
        {
            await ToolPermissionGate.EnsurePermittedAsync(
                _dbContext,
                activity.ActivityType.WbaToolKey,
                version.CreditRulesJson,
                schema,
                activity.SubjectUserId,
                ObservationDateResolver.Resolve(activity, schema, mergedDataJson).ObservedOn,
                mergedDataJson,
                cancellationToken,
                directivesToJudge.Contains);
        }

        // T102. After the T122 gate, so a move both would refuse is refused for the EPA first, and before the first
        // mutation below. Which nominees are judged is NomineeFieldsToJudge's decision; see its remarks.
        var requiredRolesByField = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);
        await NomineeGate.EnsurePermittedAsync(
            _dbContext,
            schema,
            requiredRolesByField,
            NomineeFieldsToJudge(workflow, schema, activity, transition, input.Principal, input.ActorUserId, mergedDataJson, requiredRolesByField),
            mergedDataJson,
            activity.InstitutionId,
            activity.SubjectUserId,
            cancellationToken);

        // Every read credit needs happens HERE, before the first mutation of this request (T130). The audit
        // pipeline's catch saves this request's DbContext, so anything thrown after ApplyTransition commits
        // the half-finished move: a terminal activity with no credit and a CreditedItemCount of null, which
        // reads as "never evaluated". Planning first means that from ApplyTransition to SaveChanges nothing
        // awaits and nothing can fail. The plan is built from the data and the encounter date the transition
        // is about to write, because the entity does not carry them yet.
        var creditPlan = await PlanCreditIfTerminalAsync(activity, version, schema, workflow, transition, mergedDataJson, cancellationToken);

        // T137. Resolved here, with the other reads, and assigned below with the date stamp. It reads the MERGED data,
        // so a trainee who corrects the EPA in the submit's own patch moves the stamp with it.
        var epaId = await EvidenceEpaResolver.ResolveAsync(_dbContext, schema, mergedDataJson, cancellationToken);

        // T119. Ordering here is load-bearing twice: stamping BEFORE ApplyTransition keeps the column and
        // the transition's SnapshotJson in agreement, and the stamp is the same pure function of (pinned
        // schema, data, CreatedOn) that the credit plan above was built from, so the two cannot disagree
        // about the encounter date. CreditApplier picks the curriculum item's effective minimum from the
        // stage the trainee was in ON THE ENCOUNTER DATE, and the bucket from the semester containing it.
        StampObservedOn(activity, schema, mergedDataJson);
        activity.EpaId = epaId;

        var record = activity.ApplyTransition(
            workflow, input.TransitionKey, input.ActorUserId, mergedDataJson, input.Note, occurredOn: utcNow);

        // T160, D15: how late the filing was, from the stamp written just above. Recorded, never refused.
        if (recordsTheFiling)
        {
            RecordFiling(activity, record);
        }

        if (creditPlan is not null)
        {
            ApplyCredit(creditPlan, activity, record);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    /// <summary>
    /// The credit a move into a terminal state will apply, gathered without mutating anything, or null when
    /// the move is not into a terminal state or the pinned version credits nothing.
    /// </summary>
    /// <remarks>
    /// The one credit entry point on the live path. Forking it was the option T121 rejected: each fork would
    /// then need its own copy of the scale resolution, the stage resolution, the dedupe key namespace and the
    /// T108 stamp, and the second copy is the one that gets forgotten. <see cref="StageCompletedAsync" />
    /// deliberately does not credit at all.
    /// </remarks>
    private async Task<CreditPlan?> PlanCreditIfTerminalAsync(
        Activity activity,
        ActivityTypeVersion version,
        FormSchema schema,
        Workflow workflow,
        WorkflowTransition transition,
        string mergedDataJson,
        CancellationToken cancellationToken)
    {
        var targetState = workflow.States.Single(state => string.Equals(state.Key, transition.To, StringComparison.Ordinal));

        // The `counts_for` gate is checked BEFORE planning, not after, and that is what stops the T108 signal
        // crying wolf: a reflective note, journal club, procedure log, QI project, research output or teaching
        // session declares an empty `counts_for`, so it is never evaluated and its transition stays null for
        // ever.
        if (!targetState.Terminal || !DeclaresCredit(version.CreditRulesJson))
        {
            return null;
        }

        // The date the stamp below will write. ObservationDateResolver is the one implementation of "what date
        // did this happen", and Stamp is nothing but Resolve assigned to the entity.
        var (observedOn, _) = ObservationDateResolver.Resolve(activity, schema, mergedDataJson);

        return await _creditApplier.PlanAsync(
            new CreditSubject(activity.SubjectUserId, observedOn, mergedDataJson),
            // Deliberately carries no WbaToolKey (T122). Credit does not re-check the EPA→tool allow-list (D20): the
            // write path already did, and an allow-list edited since must not take back credit a trainee earned.
            new ActivityType
            {
                CreditRulesJson = version.CreditRulesJson,
                // The pinned schema, not the live one: it declares the `scale_key` of the field the
                // credit directive gates on, and that is what binds the achieved ordinal to a ladder
                // (T109). Pinning means the binding cannot drift under the activity.
                SchemaJson = version.SchemaJson
            },
            cancellationToken);
    }

    /// <summary>
    /// Applies a credit plan and stamps the outcome onto the transition that caused it. Synchronous: it runs
    /// between <c>ApplyTransition</c> and <c>SaveChangesAsync</c>, where nothing may fail.
    /// </summary>
    /// <remarks>
    /// Stamping the outcome onto the transition that caused it is T108. Until then the result was discarded,
    /// which made "credited nothing" indistinguishable from "credited" at every surface in the product: no
    /// return value read, no domain event, no log line. Both writes land in the caller's one SaveChangesAsync,
    /// so the stamp is atomic with the credit it describes. The rebuild re-stamps too (T106 item 12), so a
    /// stale zero left by a since-corrected curriculum is cleared by the remediation the banner asks for.
    /// </remarks>
    private void ApplyCredit(CreditPlan plan, Activity activity, ActivityTransition record)
    {
        var credited = _creditApplier.Apply(plan, activity);

        record.CreditedItemCount = credited.UpdatedRows.Count;
        record.CreditScaleMismatchCount = credited.ScaleMismatchCount;
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
        var subjectScope = await SubjectScopeResolver.ResolveAsync(_dbContext, subjectUserId, cancellationToken);

        EnsureSubjectIsInTypeScope(activityType, subjectScope, subjectUserId);

        var utcNow = UtcNow();

        var requiredRolesByField = ActorFieldRules.RequiredRolesByNomineeField(schema, workflow);

        // No EPA→tool gate here (T122), and the omission is deliberate. A type that declares credit was refused above.
        // What is left is an unrated type, whose evidence EPA the gate judges on the interactive path (T154), and the two
        // system-written types. msf_cpsa: an MSF release covers every EPA the campaign declared, whatever its list says
        // (T121), and must not start dropping declared EPAs because of one; every v11.1 list names MSF, so nothing is lost
        // today. learner_feedback_cpsa (T164): its campaign's EPAs were narrowed to the lists naming learner feedback at
        // create and again by the release that calls this (MsfCampaignRules.CoverableEpaIdsAsync), so the list has been
        // read before anything reaches here. If a crediting system-written type is ever allowed through, the gate must be
        // awaited inside this loop, before AddRange, while the caller has still not mutated anything.

        // Built and validated in full BEFORE anything is added to the context. Nothing below this loop
        // may throw, because by then the caller's own mutation is pending and an exception would be
        // flushed to the database by the audit pipeline's catch. (See the interface remarks.)
        var built = new List<Activity>(input.DataJsonPerActivity.Count);
        foreach (var dataJson in input.DataJsonPerActivity)
        {
            // Stamps ObservedOn and EpaId (T119, T137). For an MSF release this is where each per-EPA row gets the EPA
            // that tells it apart from its siblings, which share a type, a state and an encounter date.
            var activity = await BuildDraftActivityAsync(
                activityType,
                schema,
                workflow,
                subjectUserId,
                subjectScope,
                input.CreatedByUserId,
                dataJson,
                input.Principal,
                utcNow,
                cancellationToken);

            // T160: the future check only, whatever the type's credit rules (null). A system-written record has nobody who
            // could correct its date, and it is never a filing, so it is never judged against the programme start and never
            // records lateness. Its type credits nothing anyway (refused above), so CanCredit would say the same today.
            ThrowIfInvalid(schema, await EncounterDateGate.ValidateAsync(
                _dbContext,
                activity,
                schema,
                activity.DataJson,
                ProgrammeCalendar.DateOf(utcNow),
                creditRulesJson: null,
                cancellationToken));

            // T102, as at create: every nominee a system-written record carries is judged, before AddRange. msf_cpsa has
            // no user field, so today this judges nothing; it is here so no write path is left without it.
            await NomineeGate.EnsurePermittedAsync(
                _dbContext,
                schema,
                requiredRolesByField,
                NomineeGate.ChangedFields(requiredRolesByField.Keys, EmptyObjectJson, activity.DataJson),
                activity.DataJson,
                activity.InstitutionId,
                activity.SubjectUserId,
                cancellationToken);

            // The same evaluator the interactive path uses, against the same rule the seed declares:
            // `msf_cpsa` says `role:Coordinator|role:Administrator`, so only staff release MSF evidence. Since
            // T162 nobody can hand-create an msf_cpsa draft at all (SystemManaged), and this rule is the
            // second line: a trainee could never complete one about themselves.
            var decision = _workflowEvaluator.Evaluate(workflow, activity, transition.Key, input.Principal);
            if (!decision.Allowed)
            {
                throw new InvalidOperationException(
                    decision.Reason ?? "The current actor is not allowed to perform this transition.");
            }

            ThrowIfInvalid(schema, ValidateForTransition(schema, workflow, activity, transition, input.Principal, activity.DataJson));

            // T119, and the ordering note from TransitionAsync applies unchanged: stamping before
            // ApplyTransition keeps the column and the transition's SnapshotJson in agreement. EpaId (T137) is not
            // re-resolved: the move below writes the very DataJson BuildDraftActivityAsync stamped it from, so a second
            // read per row could only return the same answer.
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
            .Select(transition => new ActivityActionDto(
                transition.Key,
                transition.RequiresNote,
                ExplainUnreachable(schema, workflow, activity, transition, principal, writableFieldKeys)))
            .ToList();

        return new ActivityDetailDto(
            Map(activity),
            OrderBySchema(schema, writableFieldKeys),
            availableActions);
    }

    /// <summary>
    /// Why this actor cannot complete a transition they are allowed to take, or null when they can (T107, D33).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The transition's own validation (<see cref="ValidateForTransition" />: its <c>validation</c> scope plus its
    /// <c>requires_fields</c>) is run against the STORED data under the PINNED version, and every field it flags that
    /// this actor may not write in the current state is a field no patch from this page can supply. If any remain, the
    /// move can only be refused. The action is still listed, disabled with this reason, so the actor learns what is
    /// missing instead of finding the button gone. A flagged field the actor CAN write is theirs to fill in with the
    /// move, and does not count.
    /// </para>
    /// <para>
    /// The same validator decides what is required, so a field hidden by its <c>show_if</c> is not. A flagged field whose
    /// condition (its own <c>show_if</c> or its section's) reads a field the actor may write does not count either: the
    /// move's patch can hide it, and the server's validator, run on the merged data, will then not require it
    /// (<see cref="ActorCanHide" />). Neither the EPA→tool gate (T122) nor the nominee gate (T102) is part of this: both
    /// judge a patch this page has not sent yet.
    /// </para>
    /// <para>
    /// The case that motivated it is an activity pinned to a superseded version whose assessor fields its assessor
    /// could not write, which no republish can reach (T103 pins; re-pinning is T171). Saying which version the
    /// activity was filed on is what tells that case apart from someone else's field left empty.
    /// </para>
    /// </remarks>
    private string? ExplainUnreachable(
        FormSchema schema,
        Workflow workflow,
        Activity activity,
        WorkflowTransition transition,
        ClaimsPrincipal principal,
        IReadOnlySet<string> writableFieldKeys)
    {
        var unwritable = ValidateForTransition(schema, workflow, activity, transition, principal, activity.DataJson)
            .Select(error => error.FieldKey)
            .OfType<string>()
            .Where(fieldKey => !writableFieldKeys.Contains(fieldKey))
            .Where(fieldKey => !ActorCanHide(schema, fieldKey, writableFieldKeys))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (unwritable.Count == 0)
        {
            return null;
        }

        var reason = $"Needs {JoinLabels(unwritable.Select(schema.FieldLabel).ToList())}, " +
                     "which you cannot fill in here.";

        return activity.SchemaVersion < activity.ActivityType.Version
            ? $"{reason} This activity was filed on version {activity.SchemaVersion} of the form; " +
              $"the current version is {activity.ActivityType.Version}."
            : reason;
    }

    /// <summary>
    /// Whether the condition that shows a field, its own <c>show_if</c> or its section's, reads a field this actor may
    /// write, so that the actor's own move could hide it.
    /// </summary>
    /// <remarks>
    /// Only the condition's key is read; whether it holds is still <c>SchemaValidator</c>'s call, on the data the move
    /// sends. It errs towards offering the action: when the actor could not in fact choose a hiding value, the server
    /// refuses the move as it did before T107.
    /// </remarks>
    private static bool ActorCanHide(FormSchema schema, string fieldKey, IReadOnlySet<string> writableFieldKeys)
        => schema.Sections.Any(section => section.Fields.Any(field =>
            string.Equals(field.Key, fieldKey, StringComparison.Ordinal) &&
            (ReadsWritableField(field.ShowIf, writableFieldKeys) || ReadsWritableField(section.ShowIf, writableFieldKeys))));

    private static bool ReadsWritableField(VisibilityCondition? condition, IReadOnlySet<string> writableFieldKeys)
        => condition is not null && writableFieldKeys.Contains(condition.Field);

    /// <summary>"A", "A and B", "A, B and C".</summary>
    private static string JoinLabels(IReadOnlyList<string> labels)
        => labels.Count == 1
            ? labels[0]
            : $"{string.Join(", ", labels.Take(labels.Count - 1))} and {labels[^1]}";

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

        return ActorFieldRules.DeclaredActorRules(schema, workflow)
            .Select(DropUnqualifiedRoleArms)
            .Any(rule => rule is not null && ActorRuleMatcher.Matches(rule, activity, principal));
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

    private static ActivityTypeVersion GetPinnedVersion(Activity activity)
    {
        return activity.ActivityType.Versions.SingleOrDefault(entity => entity.Version == activity.SchemaVersion)
            ?? throw new InvalidOperationException(
                $"The published activity type version '{activity.SchemaVersion}' could not be found.");
    }

    /// <summary>
    /// Which of the pinned version's gated targets (<see cref="ToolPermissionGate.GatedTargets" />: its credit directives,
    /// or an unrated instrument's evidence EPA, T154) a transition must put to the EPA→tool gate (T122, D20). Empty means
    /// the gate does not run.
    /// </summary>
    /// <remarks>
    /// <para>
    /// D20 gates the write path so that a refusal lands on someone who can still act on it, never at "the one moment
    /// nobody can act". Four review rounds showed that recognising "the author's submission" from the shape of an actor
    /// rule cannot be made right (<c>role:Trainee</c> submits, sign-offs, fallback approvers, holding states,
    /// <c>requested</c>-born types, assessor-writable EPA fields and multi-directive rules each broke a syntactic test).
    /// So the rule states D20's reason directly, per directive:
    /// <list type="bullet">
    ///   <item><b>A changed target is judged</b>, on any move from which credit can still be reached, whoever makes it.
    ///   "Changed" is what the credit engine would resolve (<see cref="CreditTargetResolver.DescribeTarget" />), so
    ///   <c>5</c> and <c>"5"</c> are the same target.</item>
    ///   <item><b>An unchanged target is judged only when the author hands it on while still able to correct it:</b>
    ///   the mover IS the subject or the creator; nobody else has acted yet; the mover can write that directive's
    ///   source field now; and the move hands it on, meaning credit can follow without coming back through the state
    ///   the move left, or the mover loses write access to that field. So a pre-T122 draft is refused at the trainee's
    ///   submit, while an assessor's completion (even an assessor allowed to correct the EPA), a sign-off after
    ///   assessment, a resubmission after a decline and a withdrawal are never refused for an unchanged target.</item>
    /// </list>
    /// A literal <c>curriculum_item_id</c> directive has no field anyone can correct, so after the create it is never
    /// judged. A move from which no terminal state can be reached at all (the CPSA workflows' <c>cancel</c> and
    /// <c>decline</c>) is never judged.
    /// </para>
    /// <para>
    /// The boundary this leaves, recorded with D20: a list or instrument changed after the create is applied to an
    /// unchanged target only if the author hands the activity on before anyone else acts. If an assessor picks a draft up
    /// first, or the type is born with the assessor (the legacy <c>requested</c>-initial shape), the create was the last
    /// check for that target.
    /// </para>
    /// <para>
    /// One conservative residual is kept deliberately (the fifth review round): a withdrawal out of the draft into a
    /// holding state the author cannot edit counts as a hand-on, because losing write access is how a hand-on is
    /// recognised, even when only the author can reopen it. The refusal still lands on the author, who can act on it.
    /// Telling "only the author can leave" from "someone else can" would mean reading actor-rule syntax, which every
    /// earlier round showed breaks.
    /// </para>
    /// </remarks>
    private IReadOnlySet<int> DirectivesToJudge(
        Workflow workflow,
        FormSchema schema,
        Activity activity,
        WorkflowTransition transition,
        ClaimsPrincipal principal,
        string actorUserId,
        string mergedDataJson,
        string creditRulesJson)
    {
        if (!workflow.CanReachTerminal(transition.To))
        {
            return EmptyDirectives;
        }

        var targets = ToolPermissionGate.GatedTargets(creditRulesJson, schema);
        if (targets.Count == 0)
        {
            return EmptyDirectives;
        }

        using var stored = JsonDocument.Parse(activity.DataJson);
        using var merged = JsonDocument.Parse(mergedDataJson);

        var judge = new HashSet<int>();
        var unchangedFieldTargets = new List<(int Index, string SourceField)>();
        for (var index = 0; index < targets.Count; index++)
        {
            var matchRule = targets[index];
            var before = CreditTargetResolver.DescribeTarget(matchRule, stored.RootElement);
            var after = CreditTargetResolver.DescribeTarget(matchRule, merged.RootElement);

            if (before != after)
            {
                judge.Add(index);
            }
            else if (after.SourceField is not null)
            {
                unchangedFieldTargets.Add((index, after.SourceField));
            }
        }

        if (unchangedFieldTargets.Count == 0)
        {
            return judge;
        }

        var handedOn = UnchangedFieldsHandedOn(
            workflow,
            schema,
            activity,
            transition,
            principal,
            actorUserId,
            mergedDataJson,
            unchangedFieldTargets.Select(target => target.SourceField).ToHashSet(StringComparer.Ordinal));

        foreach (var (index, sourceField) in unchangedFieldTargets)
        {
            if (handedOn.Contains(sourceField))
            {
                judge.Add(index);
            }
        }

        return judge;
    }

    /// <summary>
    /// Of the fields whose value this move leaves unchanged, the ones the author is handing on while still able to
    /// correct them: D20's clause, shared by the EPA→tool gate (T122) and the nominee gate (T102).
    /// </summary>
    /// <remarks>
    /// A field qualifies when the move can still reach a terminal state; the mover IS the subject or the creator;
    /// nobody else has acted yet; the mover can write the field now; and the move hands it on, meaning a terminal state
    /// can be reached without coming back through the state the move left, or the mover loses write access to the
    /// field. <see cref="DirectivesToJudge" />'s remarks record why each condition is there: four review rounds
    /// broke every rule that read actor-rule syntax instead. Each gate asks this about its own fields and nothing
    /// else, so neither can widen the other.
    /// </remarks>
    private IReadOnlySet<string> UnchangedFieldsHandedOn(
        Workflow workflow,
        FormSchema schema,
        Activity activity,
        WorkflowTransition transition,
        ClaimsPrincipal principal,
        string actorUserId,
        string mergedDataJson,
        IReadOnlySet<string> unchangedFields)
    {
        if (unchangedFields.Count == 0 ||
            !workflow.CanReachTerminal(transition.To) ||
            !IsTheAuthor(actorUserId, activity) ||
            !OnlyTheAuthorHasActed(activity))
        {
            return EmptyFields;
        }

        var writableNow = _fieldPermissionEvaluator.GetWritableFieldKeys(schema, workflow, activity, principal);
        var correctable = unchangedFields.Where(writableNow.Contains).ToHashSet(StringComparer.Ordinal);
        if (correctable.Count == 0 ||
            workflow.CanReachTerminal(transition.To, avoidingState: activity.CurrentState))
        {
            return correctable;
        }

        var writableAfter = _fieldPermissionEvaluator.GetWritableFieldKeys(
            schema, workflow, ProbeInState(activity, transition.To, mergedDataJson), principal);

        return correctable.Where(field => !writableAfter.Contains(field)).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Which nominee fields a transition must put to <see cref="NomineeGate" /> (T102). Empty means it does not run.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A changed nominee is judged on every move, whoever makes it</b> — including a move into a dead end, unlike the
    /// EPA→tool gate. A credit target matters only where credit can follow; a <c>field:</c> value grants read of the
    /// whole record, an inbox row and nudge emails in every state.
    /// </para>
    /// <para>
    /// <b>An unchanged nominee is judged only at the author's hand-on</b> (<see cref="UnchangedFieldsHandedOn" />). So a
    /// draft saved while its assessor was eligible, or before T102, is judged at the trainee's submit, when the trainee
    /// can still pick someone else; and an assessor's own completion, a sign-off after assessment, a resubmission after
    /// a decline and a withdrawal are never refused because the assessor has since lost the role or moved. Re-judging a
    /// stored nominee on those moves would strand an in-flight encounter on someone who cannot fix it.
    /// </para>
    /// <para>
    /// The boundary, as with D20: if someone other than the author acts first, the create or the last change was the
    /// last check for that nominee. A nominee who loses eligibility after the hand-on keeps what the rules give them.
    /// </para>
    /// </remarks>
    private IReadOnlySet<string> NomineeFieldsToJudge(
        Workflow workflow,
        FormSchema schema,
        Activity activity,
        WorkflowTransition transition,
        ClaimsPrincipal principal,
        string actorUserId,
        string mergedDataJson,
        IReadOnlyDictionary<string, IReadOnlyList<string>> requiredRolesByField)
    {
        if (requiredRolesByField.Count == 0)
        {
            return EmptyFields;
        }

        var changed = NomineeGate.ChangedFields(requiredRolesByField.Keys, activity.DataJson, mergedDataJson);
        var unchangedNamingSomeone = requiredRolesByField.Keys
            .Where(field => !changed.Contains(field) && NomineeGate.NamesSomeone(mergedDataJson, field))
            .ToHashSet(StringComparer.Ordinal);

        var handedOn = UnchangedFieldsHandedOn(
            workflow, schema, activity, transition, principal, actorUserId, mergedDataJson, unchangedNamingSomeone);

        return handedOn.Count == 0 ? changed : changed.Union(handedOn, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Whether a transition must put the encounter date to <see cref="EncounterDateGate" /> (T160).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A changed date is judged on every move, whoever makes it</b>, including a move into a dead end, as a changed
    /// nominee is (T102): the stamp is rewritten from it on any move, and the stamp is what credit buckets on.
    /// </para>
    /// <para>
    /// <b>An unchanged date is judged only at the author's hand-on</b> (<see cref="UnchangedFieldsHandedOn" />, the clause
    /// the EPA→tool gate and the nominee gate share). A stored date cannot newly fall in the future, but it can newly fall
    /// before the programme when the trainee's profile is created or its start date is moved after the draft was saved.
    /// The hand-on is the last moment the author can still correct it; an assessor's completion, a sign-off, or anyone
    /// else's move is never refused for a date they cannot write.
    /// </para>
    /// </remarks>
    private bool EncounterDateToJudge(
        Workflow workflow,
        FormSchema schema,
        Activity activity,
        WorkflowTransition transition,
        ClaimsPrincipal principal,
        string actorUserId,
        string mergedDataJson)
    {
        if (string.IsNullOrWhiteSpace(schema.ObservationDateField))
        {
            return false;
        }

        if (EncounterDateGate.Changed(activity, schema, activity.DataJson, mergedDataJson))
        {
            return true;
        }

        return UnchangedFieldsHandedOn(
            workflow,
            schema,
            activity,
            transition,
            principal,
            actorUserId,
            mergedDataJson,
            new HashSet<string>(StringComparer.Ordinal) { schema.ObservationDateField }).Count > 0;
    }

    private static bool IsTheAuthor(string actorUserId, Activity activity)
        => string.Equals(actorUserId, activity.SubjectUserId, StringComparison.Ordinal) ||
           string.Equals(actorUserId, activity.CreatedByUserId, StringComparison.Ordinal);

    private static bool OnlyTheAuthorHasActed(Activity activity)
        => activity.Transitions.All(transition => IsTheAuthor(transition.ActorUserId, activity));

    /// <summary>
    /// A detached copy of the activity as it would be in <paramref name="stateKey" />, for asking what the mover could
    /// write there. Never added to the context: it references the tracked type only one way, so change detection can
    /// never reach it (the same reasoning <see cref="BuildDraftActivityAsync" /> relies on).
    /// </summary>
    private static Activity ProbeInState(Activity activity, string stateKey, string dataJson)
        => new()
        {
            ActivityTypeId = activity.ActivityTypeId,
            ActivityType = activity.ActivityType,
            SchemaVersion = activity.SchemaVersion,
            SubjectUserId = activity.SubjectUserId,
            CreatedByUserId = activity.CreatedByUserId,
            CurrentState = stateKey,
            DataJson = dataJson,
            InstitutionId = activity.InstitutionId,
            SpecialityId = activity.SpecialityId,
            SubSpecialityId = activity.SubSpecialityId
        };

    private static readonly IReadOnlySet<int> EmptyDirectives = new HashSet<int>();

    private static readonly IReadOnlySet<string> EmptyFields = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// The validation a transition declares (T105), run against the data the move is about to write.
    /// </summary>
    /// <remarks>
    /// <c>owned</c> counts the <c>required</c> flags of only the fields the mover may write in the state they are moving
    /// FROM, so it must be asked before the move, against the stored data that decides <c>field:</c> ownership. A
    /// transition's <c>requires_fields</c> apply under every value.
    /// </remarks>
    private IReadOnlyList<ActivityValidationErrorDto> ValidateForTransition(
        FormSchema schema,
        Workflow workflow,
        Activity activity,
        WorkflowTransition transition,
        ClaimsPrincipal principal,
        string dataJson)
        => transition.Validation switch
        {
            TransitionValidation.Draft => _schemaValidator.Validate(
                schema, dataJson, SchemaValidationMode.Draft, transition.RequiresFields),
            TransitionValidation.Owned => _schemaValidator.Validate(
                schema,
                dataJson,
                SchemaValidationMode.Submit,
                transition.RequiresFields,
                _fieldPermissionEvaluator.GetWritableFieldKeys(schema, workflow, activity, principal)),
            _ => _schemaValidator.Validate(
                schema, dataJson, SchemaValidationMode.Submit, transition.RequiresFields)
        };

    /// <summary>
    /// Refuses the write when the validator found anything, naming each field by the label the form shows it under.
    /// </summary>
    /// <remarks>
    /// The validator reports by key, and code that reasons about the errors (<see cref="ExplainUnreachable" />) reads the
    /// key. The refusal is read by a person, on the same page as the form, so it names the field as the form does (T172),
    /// through the same <see cref="FormSchema.FieldLabel" /> a disabled action's reason uses (T107). The schema passed in
    /// is the one the data was validated against, the version the activity is pinned to.
    /// </remarks>
    private static void ThrowIfInvalid(FormSchema schema, IReadOnlyList<ActivityValidationErrorDto> validationErrors)
    {
        if (validationErrors.Count == 0)
        {
            return;
        }

        var message = string.Join("; ", validationErrors.Select(error =>
            error.FieldKey is null
                ? error.Message
                : $"{schema.FieldLabel(error.FieldKey)}: {error.Message}"));

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
    /// take your own <c>complete</c>, awarding your own curriculum credit. <c>SchemaValidator</c> treats a
    /// <c>user</c> field as a plain string, so before T102 nothing else caught it.
    ///
    /// This is the narrow guard, kept because its message says exactly what is wrong. It checks every <c>field:</c>
    /// field on every patched move; <see cref="NomineeGate" />, which runs later on the same write, also refuses the
    /// subject, but only in the fields it judges (T102).
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

        var actorFieldNames = ActorFieldRules.ActorFieldNames(schema, workflow);

        foreach (var fieldName in actorFieldNames)
        {
            var value = ActorRuleMatcher.ReadUserFieldValue(dataJson, fieldName);
            if (string.Equals(value, subjectUserId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{schema.FieldLabel(fieldName)}: this decides who may act on the activity, so it cannot name the person the activity is about.");
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
            activity.ActivityType.WbaToolKey,
            activity.SchemaVersion,
            pinnedVersion.SchemaJson,
            pinnedVersion.WorkflowJson,
            pinnedVersion.DisplayFieldsJson,
            pinnedVersion.CreditRulesJson,
            activity.SubjectUserId,
            activity.InstitutionId,
            activity.CreatedByUserId,
            activity.CurrentState,
            activity.DataJson,
            activity.EpaId,
            activity.CurriculumItemId,
            activity.ObservedOn,
            activity.ObservedOnSource == ObservationDateSource.Declared,
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
                    entity.CreditScaleMismatchCount,
                    entity.DaysAfterEncounter))
                .ToList());
    }
}
