using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Dtos;

/// <param name="Shape">
/// Which group of the Log page's instrument picker the type belongs to (T342, B8, Q1), from its published version:
/// <c>ActivityTypeShapes.Of</c>. Defaulted only so a hand-built row in a test compiles; the one producer,
/// <c>ListActivityTypesQuery</c>, always sets it.
/// </param>
/// <param name="CreditsNothing">
/// Whether the published credit rules credit nothing (an empty <c>counts_for</c>): the picker's "credits nothing" note.
/// </param>
public sealed record ActivityTypeListItemDto(
    int Id,
    string Key,
    string Name,
    ActivityScope Scope,
    int? ScopeId,
    int Version,
    bool IsActive,
    ActivityTypeShape Shape = ActivityTypeShape.DiscussedOrReviewed,
    bool CreditsNothing = false);

/// <summary>
/// The three groups of the Log page's instrument picker (T342, B8, Q1). See <c>ActivityTypeShapes.Of</c> for the
/// rule.
/// </summary>
public enum ActivityTypeShape
{
    /// <summary>The form carries an entrustment rating (a <c>scale</c> field): CBD, Mini-CEX, DOPS and the rest.</summary>
    Rated = 0,

    /// <summary>
    /// Nobody else acts on it: the author's own move out of the initial state goes straight to a terminal state (the
    /// teaching log's <c>log</c>), or the type is born terminal (<c>procedure_log</c>, <c>journal_club</c>).
    /// </summary>
    LoggedByYou = 1,

    /// <summary>Someone else discusses, reviews or signs it off: the reflection, the audit, the portfolio review.</summary>
    DiscussedOrReviewed = 2
}

/// <summary>
/// The builder's list as the caller sees it (T300): the types they may open, and whether they may start a new one, which
/// is whether <c>ActivityTypeAdminScope.WritableScopesAsync</c> offers them any scope at all.
/// </summary>
public sealed record ActivityTypeAdminListDto(
    IReadOnlyList<ActivityTypeAdminListItemDto> Items,
    bool CanCreate)
{
    /// <summary>
    /// The caller's College by name, for a College admin (T398, flow 17, N12: the subtitle "‹College name› has no
    /// speciality yet, so there are no types for you to change."); null for everyone else. Filled by lane B.
    /// </summary>
    public string? CallerCollegeName { get; init; }

    /// <summary>
    /// False only for a College admin whose College holds no speciality (T398, N12); true for everyone else. Filled by
    /// lane B.
    /// </summary>
    public bool CallerCollegeHasSpeciality { get; init; } = true;
}

/// <summary>Who owns an activity type, in kind (T398, flow 17, R2-0-spec-2): the platform, a College or an institution.</summary>
public enum ActivityTypeOwnerKind
{
    /// <summary>A Global type: the platform's, written by an Administrator.</summary>
    Platform,

    /// <summary>A Speciality or Sub-speciality type: the College's that owns the discipline (T091).</summary>
    College,

    /// <summary>An Institution type: its institution's.</summary>
    Institution
}

/// <summary>
/// Whose type it is, as the list's Set by cell and the builder's owner line name it (T398, flow 17, R2-A01, R3-C07):
/// <c>ActivityTypeAdminScope.OwnerOf</c>, so the two name an owner alike.
/// </summary>
/// <param name="Kind">Platform for Global, College for Speciality and Sub-speciality, Institution for Institution.</param>
/// <param name="Name">
/// The owner by name: the owning College's short code ("CPSA", "DEMO-C"), the institution's name; null for the platform,
/// and for a target that no longer exists.
/// </param>
public sealed record ActivityTypeOwnerDto(ActivityTypeOwnerKind Kind, string? Name);

/// <param name="CanWrite">
/// Whether the caller may save, discard and publish this type: <c>ActivityTypeAdminScope.MayWrite</c>, the rule the
/// commands' guard refuses by (T300). The list offers Edit where it is true and View elsewhere.
/// </param>
/// <param name="ScopeTargetName">
/// The scope's target as a reader sees it, as the editor names it ("Kalafong", "Paediatrics", "Paediatrics /
/// Neonatology"); null for Global, or for a target that no longer exists. The query names every row's target, so the page
/// resolves no names of its own: it did, from the caller's own institutions and disciplines, and every other College's
/// row read "Speciality · #1" (T291 item 5; the T300 review, once the College was admitted).
/// </param>
public sealed record ActivityTypeAdminListItemDto(
    int Id,
    string Key,
    string Name,
    string? Description,
    ActivityScope Scope,
    int? ScopeId,
    int PublishedVersion,
    bool IsActive,
    bool HasDraft,
    DateTime? DraftUpdatedOn,
    bool CanWrite,
    string? ScopeTargetName)
{
    /// <summary>Whose type it is (T398, flow 17, Q7, Q12): <c>ActivityTypeAdminScope.OwnerOf</c>. Filled by lane B.</summary>
    public ActivityTypeOwnerDto? Owner { get; init; }

    /// <summary>
    /// Whether only a release of Wombat writes the type (T162; T398, Q9, T334): <c>CanWrite</c> is false on it for
    /// everyone. Filled by lane B.
    /// </summary>
    public bool SystemManaged { get; init; }

    /// <summary>
    /// The release that writes a system-managed type ("the MSF release", <c>SystemManagedReleases.WrittenBy</c>); null for
    /// any other. Filled by lane B.
    /// </summary>
    public string? WrittenBy { get; init; }
}

/// <summary>
/// One scope a caller may put an activity type in, with every target they may put it at (T300): none for Global, their
/// institution for an InstitutionalAdmin, their College's disciplines for a CollegeAdmin, and all of them for an
/// Administrator. The builder's Scope picker offers exactly these.
/// </summary>
public sealed record ActivityTypeScopeChoiceDto(
    ActivityScope Scope,
    IReadOnlyList<ActivityTypeScopeTargetDto> Targets);

/// <summary>An institution, a speciality, or a sub-speciality ("Paediatrics / Neonatology") a type may be scoped to.</summary>
public sealed record ActivityTypeScopeTargetDto(int Id, string Name)
{
    /// <summary>
    /// The short code of the College that owns a speciality or sub-speciality target ("CPSA"); null for an institution.
    /// So the builder's owner line names the College of a scope chosen on the page, as it names a stored one (T398, the
    /// fix pass, pages review N2: an Administrator who chose Speciality on a new type read "Its College").
    /// </summary>
    public string? OwningCollegeShortCode { get; init; }
}

public sealed record ActivityTypeVersionDto(
    int Version,
    DateTime PublishedOn,
    string PublishedByUserId)
{
    /// <summary>
    /// The publisher by display name (T398, flow 17, Q3, T16); "Wombat's catalogue" for the seeder. Filled by lane A.
    /// </summary>
    public string? PublishedByName { get; init; }
}

public sealed record ActivityTypeEditorDto(
    int Id,
    string Key,
    string Name,
    string? Description,
    ActivityScope Scope,
    int? ScopeId,
    bool IsActive,
    // Which College-named instrument this type is, or null (T122). Live metadata like Name, not a staged
    // payload. /activities/new reads it from here to narrow the EPA picker the way the write path will check.
    string? WbaToolKey,
    int PublishedVersion,
    bool HasDraft,
    string DraftSchemaJson,
    string DraftWorkflowJson,
    string DraftCreditRulesJson,
    string DraftDisplayFieldsJson,
    string? PublishedSchemaJson,
    string? PublishedWorkflowJson,
    string? PublishedCreditRulesJson,
    string PublishedDisplayFieldsJson,
    string OwnerUserId,
    string? StagingUpdatedByUserId,
    DateTime? StagingUpdatedOn,
    IReadOnlyList<ActivityTypeVersionDto> Versions,
    // T300. Whether the caller may save, discard and publish this type (ActivityTypeAdminScope.MayWrite, the rule the
    // commands' guard refuses by); for a new type, whether they may create one anywhere. The builder is read-only without.
    // Not judged for the filing page, which asks with ForBuilder false: there it and the next two are false, empty, null.
    bool CanWrite,
    // The scopes and targets the caller may save this type in, in the order the Scope picker offers them; empty unless
    // CanWrite. A new type starts in the first of them.
    IReadOnlyList<ActivityTypeScopeChoiceDto> WritableScopes,
    // The stored scope's target as a reader sees it ("Kalafong", "Paediatrics / Neonatology"), or null for Global.
    string? ScopeTargetName)
{
    // T398 (flow 17, build-contracts § 0): the builder's members, each with a default so no existing constructor call
    // changes. Filled by lane A on the builder's path only (ForBuilder true); the filing page reads none of them.

    /// <summary>Whose type it is: <c>ActivityTypeAdminScope.OwnerOf</c>, as the list names it (Q12, R3-C07).</summary>
    public ActivityTypeOwnerDto? Owner { get; init; }

    /// <summary>Whether only a release writes the type (Q9, T334); <c>CanWrite</c> is false on it for everyone.</summary>
    public bool SystemManaged { get; init; }

    /// <summary>The release that writes a system-managed type ("the MSF release"); null for any other.</summary>
    public string? WrittenBy { get; init; }

    /// <summary>
    /// Who saved the draft, by display name (Q3: "Saved 2026-11-05 by Nolwazi Mbatha"); null without a draft, and null to a
    /// caller the type's scope does not admit (T398 fix, code review S-2).
    /// </summary>
    public string? DraftSavedByName { get; init; }

    /// <summary>
    /// Whether the caller may discard the draft: a writer's draft, or the draft a system-managed type holds from before
    /// T334, to whoever its scope admits (T398, the operator's F1). False without a draft.
    /// </summary>
    public bool CanDiscardDraft { get; init; }

    /// <summary>
    /// Whether the seed refresher still owns the type (N11, T18): its owner and its newest version's publisher are both
    /// the seeder, so a save here hands it over.
    /// </summary>
    public bool IsSeedOwned { get; init; }

    /// <summary>
    /// The EPAs the preview's EPA field offers (S8, J11), labelled as the filing page labels them; null when the form
    /// loads its own.
    /// </summary>
    public IReadOnlyList<ActivityCatalogueOption>? PreviewEpaOptions { get; init; }

    /// <summary>The workflow in words (Q2): of the saved draft, or of the published version for a reader.</summary>
    public ActivityTypeWorkflowReadDto? WorkflowRead { get; init; }

    /// <summary>The credit rules in words (Q2): of the saved draft, or of the published version for a reader.</summary>
    public ActivityTypeCreditReadDto? CreditRead { get; init; }
}

/// <summary>
/// A workflow in words (T398, flow 17, Q2, S9, J12): computed by the editor query, rendered by <c>WorkflowReadout</c>.
/// A page never parses a DSL.
/// </summary>
public sealed record ActivityTypeWorkflowReadDto(
    IReadOnlyList<ActivityTypeStateReadDto> States,
    IReadOnlyList<ActivityTypeMoveReadDto> Moves);

/// <summary>One state, by label: whether a new activity starts in it, and whether it is final.</summary>
public sealed record ActivityTypeStateReadDto(string Key, string Label, bool IsInitial, bool IsTerminal);

/// <summary>
/// One move in words (Q2): its label, the states it leaves from and goes to by label, who may make it
/// (<c>ActorRuleWords</c>), what it checks ("The whole form"), and whether it asks for a note.
/// </summary>
public sealed record ActivityTypeMoveReadDto(
    string Key,
    string Label,
    IReadOnlyList<string> FromLabels,
    string ToLabel,
    string Who,
    string Checks,
    bool RequiresNote)
{
    /// <summary>
    /// The actor rule as written, set only when it could not be worded; the page shows it in <c>&lt;code&gt;</c>.
    /// </summary>
    public string? WhoAsWritten { get; init; }
}

/// <summary>
/// The credit rules in words (Q2): "Credits nothing." when there is no directive, else one sentence a directive.
/// </summary>
public sealed record ActivityTypeCreditReadDto(bool CreditsNothing, IReadOnlyList<string> Sentences);

/// <summary>
/// One recorded workflow move. <paramref name="CreditedItemCount" /> is the T108 signal: <c>null</c>
/// when credit was never evaluated for this move (non-terminal, or a version that credits nothing by
/// design), <c>0</c> when it was evaluated and matched no curriculum item, otherwise the number of
/// curriculum items credited. <paramref name="DaysAfterEncounter" /> is set only on the move that filed the
/// activity, when its encounter date was stated and its pinned version can credit (T160, D15).
/// </summary>
/// <param name="FromStateLabel">
/// <paramref name="FromState" /> as the activity's PINNED workflow labels it (<c>PinnedWorkflows.StateLabel</c>), the key
/// only when that workflow does not declare it (T220). What the history prints; the key stays for logic.
/// </param>
/// <param name="ToStateLabel">The same, for <paramref name="ToState" />.</param>
/// <param name="TransitionLabel">
/// <paramref name="TransitionKey" /> as its button named the move (<c>PinnedWorkflows.TransitionLabel</c>): "Sign off",
/// "Create" for the create row, the key only for a move the pinned workflow does not declare (T220).
/// </param>
public sealed record ActivityTransitionDto(
    int Id,
    string FromState,
    string ToState,
    string TransitionKey,
    // No defaults: a producer that forgets a label is a page that prints a key again (T220).
    string FromStateLabel,
    string ToStateLabel,
    string TransitionLabel,
    string ActorUserId,
    DateTime OccurredOn,
    string? Note,
    string SnapshotJson,
    int? CreditedItemCount,
    // No default: this is the only signal that a completion counted for volume but was refused its
    // supervision level, and a defaulted argument is one a future call site can silently drop (T109).
    int? CreditScaleMismatchCount,
    // No default either, for the same reason: it is the only record that a filing was late (T160).
    int? DaysAfterEncounter)
{
    /// <summary>
    /// Who made the move, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// history table's Actor column (T142). Filled by <c>GetActivityByIdQuery</c>, the query that shows the history, in
    /// one lookup for every actor. Null from every other producer: <c>ActivityService.Map</c> is shared by create,
    /// transition and detail, and does no lookup.
    /// </summary>
    public string? ActorName { get; init; }
}

public sealed record ActivityDto(
    int Id,
    int ActivityTypeId,
    string ActivityTypeKey,
    string ActivityTypeName,
    // The type row's CURRENT instrument key (T122), not a pinned one: it is unversioned, and it is the key the
    // write path checks at submit, so the EPA picker narrows by the same value.
    string? WbaToolKey,
    int SchemaVersion,
    string SchemaJson,
    string WorkflowJson,
    string DisplayFieldsJson,
    // The pinned version's credit rules, carried for the same reason SchemaJson and WorkflowJson are:
    // the renderer has to know which EPA field the credit engine will actually read before it can
    // narrow that field's options to what would be credited (T108).
    string CreditRulesJson,
    string SubjectUserId,
    // The subject's institution as stamped at creation (T101): the institution a nominee field may name someone from
    // (T102). The picker reads it from here, off an activity the page has already been authorised to read.
    int? InstitutionId,
    string CreatedByUserId,
    string CurrentState,
    // CurrentState as the pinned workflow labels it, the key only when that workflow does not declare it (T220): what
    // the page's header and summary print, so they name the state as a refusal and a notice on the page do (T189).
    string CurrentStateLabel,
    string DataJson,
    int? EpaId,
    int? CurriculumItemId,
    // When the encounter happened (Activity.ObservedOn, T119), and whether anyone stated it. False means it is only
    // the day the activity was created, and the page must say so rather than show it as a clinical date (T161, D28,
    // T197; EncounterDate).
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    IReadOnlyList<ActivityTransitionDto> Transitions);

/// <summary>
/// A workflow transition the current actor is allowed to perform from the activity's current state,
/// evaluated server-side against the real pinned <c>ActivityType</c> (T070).
/// </summary>
/// <param name="UnavailableReason">
/// Null when the actor can take the action from this page. Otherwise the action is shown disabled with this text
/// (T107, D33): the transition's validation would flag, against the stored data, a field this actor cannot write in
/// the current state, so pressing it could only fail. The server still refuses such a move on its own.
/// </param>
public sealed record ActivityActionDto(
    string TransitionKey,
    bool RequiresNote,
    string? UnavailableReason = null)
{
    public bool IsAvailable => UnavailableReason is null;

    /// <summary>
    /// The user field whose person this move hands the activity to (<c>MoveHandOff.NomineeFieldFor</c>), or null when
    /// it hands it to nobody by name (T342, B2, C3). Set whether or not the field is filled: the page, holding the values
    /// being typed, may resolve the name itself from the field's picker options.
    /// </summary>
    public string? HandOffFieldKey { get; init; }

    /// <summary>
    /// The name of the person <see cref="HandOffFieldKey" /> names on the STORED data ("Fatima Khumalo"), for the button
    /// "Submit to Fatima Khumalo" and the result "It is in Fatima Khumalo's Activity inbox."; null when the move hands it
    /// to nobody by name or the field is empty. A value typed on the page and not yet saved is the page's to resolve.
    /// </summary>
    public string? HandsToName { get; init; }

    /// <summary>The label of the state the move goes to ("Requested", "Logged"): the check line's words (C11).</summary>
    public string TargetStateLabel { get; init; } = string.Empty;

    /// <summary>Whether that state is marked <c>terminal</c> (credit fires there).</summary>
    public bool TargetIsTerminal { get; init; }

    /// <summary>
    /// Whether no move leaves that state (<c>MoveOutcome.TargetIsFinal</c>): the move ends the activity, as a decline or a
    /// completion does. The page's test for "this move ends it", in place of reading <see cref="ResultSentence" />, which
    /// since T342 R5 can be "Submitted." alone for a move that leads on.
    /// </summary>
    public bool TargetIsFinal { get; init; }

    /// <summary>
    /// The first sentence of the result once the move is made: "Submitted. It is now Requested.", "Logged.",
    /// "Cancelled." (<c>MoveOutcome.ResultSentence</c>, T342 E4).
    /// </summary>
    public string ResultSentence { get; init; } = string.Empty;

    /// <summary>
    /// The action's name on its button, from <see cref="WorkflowTransition.LabelFor" />: the name a refusal of the same
    /// move uses (T189), and the name the workflow history gives the move once it is made
    /// (<see cref="ActivityTransitionDto.TransitionLabel" />, T220), so all three call it the same thing.
    /// </summary>
    public string Label => WorkflowTransition.LabelFor(TransitionKey);
}

/// <summary>
/// An activity plus the two things that can only be worked out with a principal in hand:
/// which fields this actor may write right now, and which transitions they may perform.
/// </summary>
/// <remarks>
/// T070. <see cref="EditableFieldKeys"/> is in schema order. It is empty for a terminal activity,
/// for an actor the state's <c>editable_by</c> rule excludes, and for an unrelated user — which is
/// what keeps non-bound users read-only.
/// </remarks>
public sealed record ActivityDetailDto(
    ActivityDto Activity,
    IReadOnlyList<string> EditableFieldKeys,
    IReadOnlyList<ActivityActionDto> AvailableActions)
{
    /// <summary>
    /// Who has the activity now (<c>ActivityHolders</c>, T342, B7): the status card's headline. Filled by
    /// <c>GetActivityByIdQuery</c>, with names; null from every other producer (<c>ActivityService</c> maps without a
    /// lookup).
    /// </summary>
    public ActivityHolderDto? Holder { get; init; }

    /// <summary>
    /// Who returned it to its author, when and with what note, while it is back in its first state (T342, B6); null
    /// when it is not returned. Filled by <c>GetActivityByIdQuery</c>.
    /// </summary>
    public ActivityReturnDto? Returned { get; init; }

    /// <summary>
    /// The nominee's name, the person the activity goes to or is discussed with (<c>ActivityHolders.NomineeField</c>,
    /// T342, B7); null for a type whose workflow names no one in a field, or while the field is empty. Filled by
    /// <c>GetActivityByIdQuery</c>.
    /// </summary>
    public string? NomineeName { get; init; }

    /// <summary>
    /// The activity's name for the h1, the tab and the last crumb: "Type · EPA · date", with " · nominee" when another
    /// of the subject's activities the caller can read shares the rest (<c>ActivityDisplayNames</c>, T342, E7, E9).
    /// Filled by <c>GetActivityByIdQuery</c>.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// The subject's name, the registrar the activity is about (<c>UserDisplayNames.NameOf</c>): the activity page's
    /// subtitle, its About card's Registrar and its status card's sentences (T342, flow 03). Filled by
    /// <c>GetActivityByIdQuery</c>, in the lookup that names the history; null from every other producer.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>
    /// The stamped EPA's code, title and whether it is in force now (<c>Epa.IsActive</c>), for the About card's "EPA"
    /// with its full title and the paused-credit warning (T342, C13; D48). Null when the activity is about no EPA. Filled
    /// by <c>GetActivityByIdQuery</c>.
    /// </summary>
    public string? EpaCode { get; init; }

    /// <inheritdoc cref="EpaCode" />
    public string? EpaTitle { get; init; }

    /// <inheritdoc cref="EpaCode" />
    public bool? EpaInForce { get; init; }
}

/// <summary>
/// Who has an activity now, as the status card and My activities' "Who has it now" column read it (T342, B7). Told from
/// the moves out of its state that lead on (<c>Workflow.TransitionsLeadingOn</c>), in its pinned workflow.
/// </summary>
public enum ActivityHolderKind
{
    /// <summary>
    /// Its author: every move that leads on is the subject's or the creator's (a draft, or work returned). "You" to them;
    /// <see cref="ActivityHolderDto.Name" /> is the subject's name for anyone else.
    /// </summary>
    Author = 0,

    /// <summary>One named person: every other move that leads on names the same user in a <c>field:</c> arm.</summary>
    Person = 1,

    /// <summary>
    /// No single person: a <c>role:</c> or <c>scope:</c> arm leads on, or the field names no one, or the pin has no
    /// workflow that declares the state. The page reads "Waiting for &lt;state label&gt;."
    /// </summary>
    Waiting = 2,

    /// <summary>No one, and finished: the state is <c>terminal: true</c>. "Done".</summary>
    Done = 3,

    /// <summary>No one, and not finished: no move out of the state leads on (declined, cancelled). "Closed".</summary>
    Closed = 4
}

/// <param name="Kind">Which of the five answers it is.</param>
/// <param name="UserId">
/// The holder's id for <see cref="ActivityHolderKind.Author" /> (the subject) and <see cref="ActivityHolderKind.Person" />;
/// null otherwise.
/// </param>
/// <param name="Name">That user's name (<c>UserDisplayNames.NameOf</c>); null when <paramref name="UserId" /> is.</param>
/// <param name="IsViewer">
/// Whether the holder is the caller: for <see cref="ActivityHolderKind.Author" />, the caller is the subject or the
/// creator; for <see cref="ActivityHolderKind.Person" />, the named user. The page says "You" / "With you." then.
/// </param>
/// <param name="Since">
/// When it entered its current state: the newest recorded move (UTC; the page formats it in SAST). Null only for an
/// activity with no history row.
/// </param>
public sealed record ActivityHolderDto(
    ActivityHolderKind Kind,
    string? UserId,
    string? Name,
    bool IsViewer,
    DateTime? Since);

/// <summary>
/// An activity returned to its author (T342, B6): its newest move entered the workflow's initial state from another state.
/// </summary>
/// <param name="ByUserId">Who made the move that returned it.</param>
/// <param name="ByName">Their name (<c>UserDisplayNames.NameOf</c>).</param>
/// <param name="ReturnedOn">When (UTC; the page formats it in SAST).</param>
/// <param name="Note">The note on that move, which a return requires in every shipped workflow; null if none.</param>
public sealed record ActivityReturnDto(
    string ByUserId,
    string ByName,
    DateTime ReturnedOn,
    string? Note);

/// <summary>One page of an activity list (T342, B7): My activities' All activities table.</summary>
/// <param name="Items">The page's rows, in the list's order.</param>
/// <param name="Page">The page served, from 1: the page asked for, brought within 1 and <see cref="PageCount" />.</param>
/// <param name="PageSize">Rows per page.</param>
/// <param name="TotalCount">Every row the list holds, across all pages: "All activities (40)", "1-20 of 40".</param>
public sealed record ActivityListPageDto(
    IReadOnlyList<ActivitySummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    /// <summary>How many pages the list has; 1 for an empty list, so "page 1 of 1" is always true.</summary>
    public int PageCount => Math.Max(1, (TotalCount + PageSize - 1) / PageSize);

    /// <summary>
    /// The last day of the subject's programme, when the subject's preferred profile has ended (<c>TraineeProfile.EndedOn</c>):
    /// My activities' "Read-only: your programme ended on 2026-10-24, so nothing new can be filed." and its note rows'
    /// day (T379, flow 13, R3-R07; S18). Set by <c>ListActivitiesBySubjectQuery</c> on the caller's own list only; null
    /// while the programme runs, for a list of someone else, and elsewhere.
    /// </summary>
    public DateOnly? SubjectEndedOn { get; init; }
}

/// <summary>
/// One row of an activity list: enough to tell an activity from its siblings without opening it (T137).
/// </summary>
/// <param name="EpaId">The stamped <c>Activity.EpaId</c>: the EPA this activity is evidence for, or null.</param>
/// <param name="EpaCode">That EPA's code, or null when the activity is about no EPA.</param>
/// <param name="EpaTitle">That EPA's title, or null when the activity is about no EPA.</param>
/// <param name="EpaInForce">
/// Whether that EPA is in force now, or null when the activity is about no EPA (T231, D48). It is <c>Epa.IsActive</c>,
/// the rule <c>CurriculumItemsInForce.InForce</c> filters by and <see cref="Wombat.Application.Features.Epas.EpaOptionLabel" />
/// labels by, so a list marks "(no longer in use)" on exactly the EPA the activity's own picker marks. It is judged now,
/// not at the activity's completion: the credit already stamped (<paramref name="CreditedItemCount" />) is the record of
/// then.
/// </param>
/// <param name="ObservedOn">
/// When the encounter happened (<c>Activity.ObservedOn</c>, T119), not when the paperwork was filed.
/// </param>
/// <param name="ObservedOnDeclared">
/// False when nobody stated an encounter date and <paramref name="ObservedOn" /> is only the day the activity was
/// created. A list must not present that as a clinical fact.
/// </param>
/// <param name="CreditedItemCount">
/// The T108 outcome of the LATEST transition on which credit was evaluated: null when credit was never evaluated (the
/// activity is not complete, or its type credits nothing by design), 0 when it was evaluated and matched no curriculum
/// item, otherwise the number of items credited. The same three-valued contract as
/// <see cref="ActivityTransitionDto.CreditedItemCount" />, read from the same column. (T106 item 14)
/// </param>
public sealed record ActivitySummaryDto(
    int Id,
    int ActivityTypeId,
    string ActivityTypeKey,
    string ActivityTypeName,
    string SubjectUserId,
    string CurrentState,
    // CurrentState as the activity's pinned workflow labels it, the key only when that workflow does not declare it
    // (T220). What the lists print.
    string CurrentStateLabel,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    int? EpaId,
    string? EpaCode,
    string? EpaTitle,
    bool? EpaInForce,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    int? CreditedItemCount)
{
    /// <summary>
    /// Whose activity it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// assessor's link's second line, "from Anele Dlamini" (T142; T350, R2). Filled by the waiting and the decided reads
    /// (<c>WaitingForYou</c>, <c>DecidedByYou</c>) in one lookup for the rows they return. Null on the subject's own list,
    /// which is all one person and does not show them.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>
    /// Whether it has waited on the caller for <c>DashboardThresholds.AssessorDueDays</c> × 24 h or more since
    /// <see cref="UpdatedOn" /> (T350, round 2 E1): the Overdue badge beside the state, and the row's warning edge. So a
    /// whole-day count of 7 (<see cref="WaitedDays" />) always carries it. Set by the waiting read only.
    /// </summary>
    public bool IsOverdue { get; init; }

    /// <summary>
    /// How long it has waited on the caller, in whole days since <see cref="UpdatedOn" />, rounded down as the assessor
    /// nudge counts them: 0 is under a day ("Waiting less than a day"). Null outside the waiting read (T350, note 10).
    /// </summary>
    public int? WaitedDays { get; init; }

    /// <summary>
    /// When the caller made the move that left it where it is (UTC; the page formats it in SAST): "Decided by you"'s
    /// Decided column and its order (T350, Q1). Null outside the decided read.
    /// </summary>
    public DateTime? DecidedOn { get; init; }

    /// <summary>
    /// Whether its state is a terminal state of its pinned workflow (<c>ActivityCompletion</c>, D44): the decided row's
    /// badge is green when it is, whatever the key is called (T266 review). Set by the decided read only; false elsewhere.
    /// </summary>
    public bool IsFinished { get; init; }

    /// <summary>
    /// Who has it now (<c>ActivityHolders</c>, T342, B7): the "Who has it now" column. Filled by
    /// <c>ListActivitiesBySubjectQuery</c> and <c>ListNeedsYouQuery</c>; null from the inbox.
    /// </summary>
    public ActivityHolderDto? Holder { get; init; }

    /// <summary>
    /// The nominee's name, for the link's second line ("to David Naidoo", "with Sarah Botha"): the user a <c>field:</c>
    /// arm of the next move that leads on names, else the first field any move's rule names
    /// (<c>ActivityHolders.NomineeField</c>, T342, B7). Null for a type whose workflow names no one in a field (a log), or
    /// while the field is empty. Filled by the two queries that fill <see cref="Holder" />.
    /// </summary>
    public string? NomineeName { get; init; }

    /// <summary>
    /// Who returned it, when and with what note, while it is back in its first state (T342, B6); null when it is not
    /// returned. Filled by the two queries that fill <see cref="Holder" />.
    /// </summary>
    public ActivityReturnDto? Returned { get; init; }

    /// <summary>Whether it was returned to its author: <see cref="Returned" /> is set.</summary>
    public bool IsReturned => Returned is not null;

    /// <summary>
    /// "Type · EPA · date", with " · nominee" when another of the subject's activities shares the rest
    /// (<c>ActivityDisplayNames</c>, T342, E7, E9): the link's text. Filled by the two queries that fill
    /// <see cref="Holder" />.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Whether <see cref="DisplayName" /> ends with the nominee because another activity shares the rest (E7). The page
    /// then drops the link's second line, which would repeat the name.
    /// </summary>
    public bool DisplayNameHasNominee { get; init; }

    /// <summary>
    /// The type's shape as its pinned form and workflow read (<c>ActivityTypeShapes.Of</c>, T342): My activities and
    /// Needs you word the nominee "with Sarah Botha" for <see cref="ActivityTypeShape.DiscussedOrReviewed" /> (a
    /// reflection, a review: it is talked over with them) and "to David Naidoo" otherwise (a request goes to its assessor).
    /// Filled by the two queries that fill <see cref="Holder" />; null from the inbox, and for a pin that no longer parses.
    /// </summary>
    public ActivityTypeShape? Shape { get; init; }

    /// <summary>
    /// The count this activity made towards its EPA's item, in the window its encounter counts towards
    /// (<c>EpaCountLines</c>, T355, C5; E5): the line under a Recent decisions row on the Trainee's Home. Set by
    /// <c>DecidedOnYours</c> only; null elsewhere, and null for an activity about no EPA or an EPA that is no in-force item
    /// of the trainee's curriculum.
    /// </summary>
    public EpaCountLineDto? CountLine { get; init; }

    /// <summary>
    /// Whether its EPA's page under My progress opens for the caller (<c>TraineeQuotaProgressReader.EpasWithAPageAsync</c>):
    /// the EPA is an item of the curriculum their preferred profile holds. My activities links the Credit cell to that page
    /// only then (T355, build review G4). Set by <c>ListActivitiesBySubjectQuery</c> for the caller's own list; false
    /// elsewhere.
    /// </summary>
    public bool EpaPageOpens { get; init; }

    /// <summary>
    /// Whether it was observed after the last day of its subject's ended programme (<c>TraineeProfile.IsAfterEnd</c>), so it
    /// counts towards nothing (T281): My activities' note row under it, "Observed after your last day in the programme,
    /// 2026-10-24, so it counts towards nothing." (T379, flow 13, R3-R07; DATA "My activities' note row's reason"). Set by
    /// <c>ListActivitiesBySubjectQuery</c> on the caller's own list, against <see cref="ActivityListPageDto.SubjectEndedOn" />;
    /// false elsewhere.
    /// </summary>
    public bool ObservedAfterEnd { get; init; }

    /// <summary>
    /// Whether the decision was a decline, by its pinned workflow (<c>ActivityDecline</c>, the rule File it again reads;
    /// E6: the rule, not a list): its row offers File it again and no count line (T355, build review R4). Set by
    /// <c>DecidedOnYours</c> only; false elsewhere.
    /// </summary>
    public bool Declined { get; init; }

    /// <summary>
    /// Whether its pinned credit rules can credit anything (<c>EncounterDatePolicy.CanCredit</c>, a non-empty
    /// <c>counts_for</c>): only such a type's nothing-credited completion "waits while the EPA is paused"; a reflection
    /// stamped with an EPA credits nothing by design (T355, build review R4). Set by <c>DecidedOnYours</c> only; false
    /// elsewhere.
    /// </summary>
    public bool CanCredit { get; init; }

    /// <summary>
    /// The newest reminder a member of staff sent about it (T358, flow 06; C4): "Reminded 2026-10-04 by Pieter Smit" in
    /// the Waiting cell. Set by <c>WaitingForAssessorsReader</c> only; null elsewhere, and when none was sent.
    /// </summary>
    public ActivityReminderDto? LastReminder { get; init; }

    /// <summary>
    /// Whether a reminder about it was sent today on the South African calendar, by anyone (T358, round 3's settled
    /// same-day rule): then no one may send another, and the cell shows the record and no button. Set by
    /// <c>WaitingForAssessorsReader</c> only.
    /// </summary>
    public bool RemindedToday { get; init; }

    /// <summary>
    /// Why its nominee cannot be reminded, as the list was read (<c>ReminderRecipientRules.RefusalFor</c>, T358, E3's
    /// w9): <see cref="ReminderOutcome.Deactivated" />, <see cref="ReminderOutcome.NoEmail" /> or
    /// <see cref="ReminderOutcome.NoAccount" />, so the row says so before anyone presses; null when a reminder may be
    /// sent. Never an opt-out of digest emails (E1). Set by <c>WaitingForAssessorsReader</c> only.
    /// </summary>
    public ReminderOutcome? CannotRemind { get; init; }
}

/// <summary>
/// What waits on the caller as someone else's assessor, reviewer or admin (T350, note 5; E5): the one read Home's "Waiting
/// for you", the Activity inbox, the activity page's way on and the other-role line share (<c>WaitingForYou</c>), so no
/// two of them can disagree (T297).
/// </summary>
/// <param name="Items">Every such activity, oldest first by <c>UpdatedOn</c>, then id. Home takes the first five.</param>
/// <param name="OverdueCount">How many of them are <see cref="ActivitySummaryDto.IsOverdue" />.</param>
/// <param name="DueDays">
/// <c>DashboardThresholds.AssessorDueDays</c>, the number in the rule line "Overdue once it has waited 7 days." (E1).
/// </param>
public sealed record WaitingForYouDto(IReadOnlyList<ActivitySummaryDto> Items, int OverdueCount, int DueDays)
{
    /// <summary>How many wait: "2 waiting".</summary>
    public int Count => Items.Count;

    /// <summary>The one that has waited longest, or null when nothing waits: the other-role line's row.</summary>
    public ActivitySummaryDto? Oldest => Items.FirstOrDefault();
}

public sealed record ActivityValidationErrorDto(
    string? FieldKey,
    string Message,
    string Rule);
