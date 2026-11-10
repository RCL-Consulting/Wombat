using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Wombat.Application.Common;
using Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

/// <summary>
/// The activity type builder's behaviour (T398, flow 17, lane C). The markup is <c>ActivityTypeEdit.razor</c>.
/// </summary>
/// <remarks>
/// <para>
/// Refusals and failures (P1, build-lanes § "Refusals and failures"): this page tells one from the other itself and never
/// calls <c>RefusalText.Of</c>. A refusal is an <see cref="IActivityTypeRefusal" /> (the three parse exceptions, and P2's
/// aggregate), a <see cref="FieldRefusalException" />, a <see cref="ValidationException" /> (Details' four words) or an
/// <see cref="UnauthorizedAccessException" /> (the guard's sentence, in the region, no place). Anything else is a failure:
/// the fixed sentence, logged, never the exception's text (T329, T393).
/// </para>
/// <para>
/// Publish's order (Q16, Q17): Active newly unticked on a published type asks Q15's question first (Cancel leaves
/// everything unsaved, the focus back on Publish); then the edits since the draft was saved are saved; then, with warnings
/// from the saved draft, the publish question; then the publish. The result lands on the tab it was pressed from (Q20).
/// </para>
/// </remarks>
public partial class ActivityTypeEdit
{
    private const string SaveAction = "save";
    private const string DiscardAction = "discard";
    private const string PublishAction = "publish";

    private const string SaveButtonId = "builder-save";
    private const string PublishButtonId = "builder-publish";
    private const string DiscardButtonId = "builder-discard";
    private const string PublishReasonId = "publish-reason";
    private const string PublishTabButtonId = "publish-tab-action";
    private const string PublishTabReasonId = "publish-tab-reason";
    private const string AddSectionId = "add-section";
    private const string KeyInputId = "type-key";
    private const string NameInputId = "type-name";
    private const string DescriptionInputId = "type-description";
    private const string ActiveInputId = "type-is-active";
    private const string DescriptionCounterId = "type-description-count";
    private const string WorkflowJsonId = "workflow-json";
    private const string CreditJsonId = "credit-json";

    private static readonly FieldType[] SupportedFieldTypes =
    [
        FieldType.Text,
        FieldType.LongText,
        FieldType.Number,
        FieldType.Date,
        FieldType.Choice,
        FieldType.MultiChoice,
        FieldType.Scale,
        FieldType.User,
        FieldType.Epa,
        FieldType.Likert,
        FieldType.ProcedureRef,
        FieldType.File,
        FieldType.Signature
    ];

    private static readonly BuilderTab[] WriterTabs = [BuilderTab.Details, BuilderTab.Form, BuilderTab.Workflow, BuilderTab.Credit, BuilderTab.Publish];
    private static readonly BuilderTab[] ReaderTabs = [BuilderTab.Details, BuilderTab.Form, BuilderTab.Workflow, BuilderTab.Credit];

    [Inject] private IScopedSender Sender { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private IActivityReferenceDataService ReferenceData { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<ActivityTypeEdit> Logger { get; set; } = default!;

    [Parameter] public int? ActivityTypeId { get; set; }

    private ActivityTypeEditorDto? _editor;
    private BuilderSchemaModel _builderSchema = new();
    private IReadOnlyList<EntrustmentScaleOption> _entrustmentScales = [];
    private IReadOnlyList<WbaToolDto> _wbaTools = [];

    private bool _loading = true;
    private string? _loadError;
    private bool _read;
    private int? _readId;

    private BuilderTab _tab = BuilderTab.Form;

    // The open editor in place: one at a time (Q4). By reference, so Up and Down keep it on its row.
    private BuilderSectionModel? _openSection;
    private BuilderFieldModel? _openField;
    private readonly HashSet<BuilderSectionModel> _folded = new(ReferenceEqualityComparer.Instance);

    private string _key = string.Empty;
    private string _name = string.Empty;
    private string? _description;
    private ActivityScope _scope;
    private string _scopeIdText = string.Empty;
    private bool _isActive = true;
    private string _wbaToolKey = string.Empty;
    private string _workflowJson = string.Empty;
    private string _creditRulesJson = string.Empty;
    private string _previewDataJson = "{}";
    private string _lastPreviewSchemaJson = string.Empty;

    // The result region (one per page, S7): a refusal, a result, a failure, or the guard's sentence.
    private ActionResult? _result;
    private RefusalSummary? _summary;
    private BuilderRefusal? _refusal;
    private IReadOnlyList<RefusalLine> _summaryLines = [];
    private readonly Dictionary<RefusalLine, BuilderGoTo> _goTo = new(ReferenceEqualityComparer.Instance);
    private (string Lead, string? Tail)? _status;
    private (string Lead, string Tail)? _failure;
    private string? _guard;

    private bool _focusResult;
    private bool _focusSummary;
    private string? _focusId;
    private bool _stripToken;

    /// <summary>The header action that has been sent and has not yet answered, or null (T234).</summary>
    private string? _inFlight;

    private ConfirmDialog? _discardDialog;
    private ConfirmDialog? _deactivateDialog;
    private ConfirmDialog? _publishDialog;

    // Which action asked Q15's question (Save draft or Publish), and which control pressed Publish (Q17, Q20).
    private string _deactivateFor = SaveAction;
    private string _publishTrigger = PublishButtonId;

    /// <summary>A publish waiting on its question: whether it deactivates, whether it saved first, and what it changes.</summary>
    private sealed record PendingPublish(bool Deactivating, bool SavedFirst, IReadOnlyList<string> Warnings, IReadOnlyList<string> Removed);

    private PendingPublish? _pendingPublish;

    // ---- what the page is ----

    /// <summary>The page's own address says whether it is a new type, never a missing editor (the T295 sweep).</summary>
    private bool IsNewRoute => ActivityTypeId is null;

    /// <summary>An id that names no type, or one the caller may not read (N3): the page's own Unavailable (R2-B09).</summary>
    private bool IsUnavailable => !IsNewRoute && _read && !_loading && _loadError is null && _editor is null;

    /// <summary>Whether the caller may save, discard and publish (T300, T334): false until the editor has loaded.</summary>
    private bool CanWrite => _editor?.CanWrite == true;

    private bool IsPublished => (_editor?.PublishedVersion ?? 0) > 0;

    private bool HasStoredDraft => (_editor?.Id ?? 0) > 0 && _editor?.HasDraft == true;

    /// <summary>
    /// Whether Discard draft… is offered: a writer's saved draft, or the draft a system-managed type holds from before
    /// T334, to whoever its scope admits (T398, the operator's F1). Save draft and Publish stay a writer's alone.
    /// </summary>
    private bool CanDiscard => HasStoredDraft && (CanWrite || _editor?.CanDiscardDraft == true);

    /// <summary>The header offers actions to a writer, and Discard draft… alone where only it is offered (F1).</summary>
    private bool HasHeaderActions => CanWrite || CanDiscard;

    /// <summary>
    /// Whether the Discard question is in the page: once offered it stays to the page's end, so a discard that clears
    /// the draft does not take the dialog away while it closes (F1).
    /// </summary>
    private bool _discardOffered;

    /// <summary>Publish is offered on a saved draft only; otherwise it is unavailable and inert (S5).</summary>
    private bool CanPublish => HasStoredDraft;

    private int NextVersion => (_editor?.PublishedVersion ?? 0) + 1;

    private string StoredName => _editor?.Name ?? string.Empty;

    private string TypedName => string.IsNullOrWhiteSpace(_name) ? StoredName : _name.Trim();

    /// <summary>Q15: Active newly unticked on a published type, which Save draft and Publish ask about first.</summary>
    private bool WouldDeactivate => IsPublished && _editor!.IsActive && !_isActive;

    private IReadOnlyList<BuilderTab> Tabs => CanWrite ? WriterTabs : ReaderTabs;

    private static string TabId(BuilderTab tab) => $"tab-{tab.ToString().ToLowerInvariant()}";

    private static string PanelId(BuilderTab tab) => $"panel-{tab.ToString().ToLowerInvariant()}";

    private static string TabName(BuilderTab tab) => tab.ToString();

    private static string SectionDomId(int s) => $"sec-{s}";

    private static string SectionEditorId(int s) => $"se-{s}";

    private static string FieldsListId(int s) => $"fields-{s}";

    private static string RowId(int s, int f) => $"row-{s}-{f}";

    private static string FieldEditorId(int s, int f) => $"fe-{s}-{f}";

    private static string SectionButtonId(int s, string what) => $"{SectionDomId(s)}-{what}";

    private static string FieldButtonId(int s, int f, string what) => $"{RowId(s, f)}-{what}";

    private static string AddFieldId(int s) => $"add-field-{s}";

    private static string? Invalid(bool refused) => refused ? "true" : null;

    private bool OtherInFlight(string action) => _inFlight is not null && _inFlight != action;

    private bool Running(string action) => _inFlight == action;

    // ---- the header's words ----

    /// <summary>
    /// The owner line (Q12, R3-C01–C07): whose type, then its scope, "Kgosi Kgari Teaching Hospital · Institution". On a
    /// writer's page it follows the scope chosen; the College by its short code, read with the editor (§ 0's owner).
    /// </summary>
    private string? OwnerLineText
    {
        get
        {
            if (_editor is null || (IsNewRoute && !CanWrite))
            {
                return null;
            }

            var scope = CanWrite ? _scope : _editor.Scope;
            var targetName = CanWrite ? ChosenTargetName : _editor.ScopeTargetName;
            return ActivityTypeEditWords.OwnerLine(SetByFor(scope, targetName), ActivityTypeWords.ScopeDetail(scope, targetName));
        }
    }

    private string SetByFor(ActivityScope scope, string? targetName)
    {
        var stored = _editor!.Owner;
        var unchanged = scope == _editor.Scope && (!CanWrite || ParseNullableInt(_scopeIdText) == _editor.ScopeId);
        if (unchanged && stored is not null)
        {
            return ActivityTypeWords.SetBy(stored);
        }

        // A scope chosen here: the institution by its name, the platform, or the College that owns the chosen target by
        // its short code (the fix pass, N2), else the College the editor names.
        return scope switch
        {
            ActivityScope.Global => ActivityTypeWords.SetBy(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Platform, null)),
            ActivityScope.Institution => ActivityTypeWords.SetBy(new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, targetName)),
            _ => ActivityTypeWords.SetBy(new ActivityTypeOwnerDto(
                ActivityTypeOwnerKind.College,
                ChosenTarget?.OwningCollegeShortCode ?? (stored?.Kind == ActivityTypeOwnerKind.College ? stored.Name : null)))
        };
    }

    /// <summary>The target chosen on the page among the caller's writable scopes; null for Global or none chosen.</summary>
    private ActivityTypeScopeTargetDto? ChosenTarget
        => _scope == ActivityScope.Global
            ? null
            : _editor?.WritableScopes.FirstOrDefault(choice => choice.Scope == _scope)?.Targets
                .FirstOrDefault(target => target.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) == _scopeIdText);

    private string? ChosenTargetName
        => _scope == ActivityScope.Global
            ? null
            : ChosenTarget?.Name ?? (_scope == _editor?.Scope ? _editor.ScopeTargetName : null);

    /// <summary>Scope's read-out where one is offered: "Institution · Kgosi Kgari Teaching Hospital" (R3-C01).</summary>
    private string ScopeAndTarget
        => ChosenTargetName is { } target ? $"{ActivityTypeEditWords.ScopeName(_scope)} · {target}" : ActivityTypeEditWords.ScopeName(_scope);

    /// <summary>T300's notice word for word, or the release's on a system-managed type (Q9, T22).</summary>
    private string ReaderNotice => _editor is { SystemManaged: true } editor
        ? ActivityTypeWords.ReleaseNotice(editor.WrittenBy ?? "a release of Wombat")
        : ActivityTypeWords.ReadOnlyNotice(_editor?.Scope ?? ActivityScope.Global, _editor?.ScopeTargetName);

    private string PublishedText
    {
        get
        {
            var editor = _editor!;
            var version = editor.Versions.FirstOrDefault(v => v.Version == editor.PublishedVersion);
            return ActivityTypeEditWords.PublishedLine(editor.PublishedVersion, version?.PublishedOn, version?.PublishedByName);
        }
    }

    private string DraftText
        => ActivityTypeEditWords.DraftLine(_editor!.HasDraft && _editor.Id > 0, _editor.StagingUpdatedOn, _editor.DraftSavedByName);

    private string WbaToolText => string.IsNullOrEmpty(_wbaToolKey)
        ? ActivityTypeEditWords.NotAnInstrument
        : _wbaTools.FirstOrDefault(tool => tool.Key == _wbaToolKey)?.Name ?? $"{_wbaToolKey} (not a known instrument)";

    /// <summary>The vocabulary, plus the stored key when the vocabulary no longer holds it (T122).</summary>
    private IEnumerable<WbaToolDto> WbaToolOptions =>
        string.IsNullOrEmpty(_wbaToolKey) || _wbaTools.Any(tool => tool.Key == _wbaToolKey)
            ? _wbaTools
            : _wbaTools.Append(new WbaToolDto(_wbaToolKey, $"{_wbaToolKey} (not a known instrument)", null));

    // ---- loading ----

    protected override async Task OnParametersSetAsync()
    {
        // Read again when the address's id changes (a first save arrives at its new type), never when only the query
        // changes: the token's own replace must not read the page again.
        if (_read && _readId == ActivityTypeId)
        {
            return;
        }

        _readId = ActivityTypeId;
        _tab = IsNewRoute ? BuilderTab.Details : _tab;
        await LoadAsync(arrival: true);
    }

    private Task RetryAsync() => LoadAsync(arrival: false);

    private ValueTask FocusHeadingAsync() => PageFocus.FocusHeadingAsync(JS);

    private async Task LoadAsync(bool arrival)
    {
        // T300: the editor of the address before this one is not this address's; until this one loads, the page offers
        // nothing to save (the T295 sweep: a Save draft while loading sent a create).
        var keepTab = _editor is not null && arrival;
        _editor = null;
        _loading = true;
        _loadError = null;
        ClearResults();

        try
        {
            var user = await UserAsync();
            ActivityTypeEditorDto? editor = await Sender.Send(new GetActivityTypeEditorQuery(ActivityTypeId, user));
            _entrustmentScales = await ReferenceData.GetEntrustmentScaleOptionsAsync();
            _wbaTools = await Sender.Send(new GetWbaToolsQuery());

            // Held back until everything has loaded, so a failed load leaves no editor, and so no Save draft (T300).
            _editor = editor;
            if (editor is not null)
            {
                LoadEditorState(editor, keepUi: false);
                if (IsNewRoute)
                {
                    _tab = BuilderTab.Details;
                }
                else if (CatalogueLinks.HasToken(Navigation.Uri, CatalogueLinks.CreatedToken))
                {
                    // T291 item 3: a first save arrives by ?created; the page says so itself, focuses it, keeps Details
                    // (T2: a save keeps the tab) and then replaces its address without the token.
                    _tab = editor.CanWrite ? BuilderTab.Details : BuilderTab.Form;
                    _status = (ActivityTypeEditWords.DraftSaved, null);
                    _focusResult = true;
                    _stripToken = true;
                }
                else if (!keepTab)
                {
                    _tab = BuilderTab.Form;
                }

                if (!Tabs.Contains(_tab))
                {
                    _tab = BuilderTab.Form;
                }
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // T329, R2-B11: the page's own read failed, which is not Unavailable. Never the exception's text.
            Logger.LogError(exception, "The activity type {ActivityTypeId} could not be read.", ActivityTypeId);
            _editor = null;
            _loadError = ActivityTypeEditWords.LoadFailed;
        }
        finally
        {
            _loading = false;
            _read = true;
        }
    }

    /// <summary>
    /// Fills the page from <paramref name="editor" />. With <paramref name="keepUi" /> (after a save or a publish) the open
    /// editor and the folds stay where they were, by position: a save reorders nothing.
    /// </summary>
    private void LoadEditorState(ActivityTypeEditorDto editor, bool keepUi)
    {
        var openSection = keepUi && _openSection is not null ? _builderSchema.Sections.IndexOf(_openSection) : -1;
        var openField = keepUi && _openField is not null ? FieldPosition(_openField) : null;
        var folded = keepUi ? _folded.Select(section => _builderSchema.Sections.IndexOf(section)).Where(i => i >= 0).ToList() : [];

        _discardOffered |= editor.CanDiscardDraft || editor.CanWrite;
        _key = editor.Key;
        _name = editor.Name;
        _description = editor.Description;
        _scope = editor.Scope;
        _scopeIdText = editor.ScopeId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        _isActive = editor.IsActive;
        _wbaToolKey = editor.WbaToolKey ?? string.Empty;

        // A reader reads what registrars file: the published version where there is one (Q2's "the published version for a
        // reader"); a writer the draft.
        var schemaJson = editor.CanWrite ? editor.DraftSchemaJson : editor.PublishedSchemaJson ?? editor.DraftSchemaJson;
        _builderSchema = BuilderSchemaModel.Parse(schemaJson);
        _workflowJson = editor.DraftWorkflowJson;
        _creditRulesJson = editor.DraftCreditRulesJson;
        if (!keepUi)
        {
            _previewDataJson = "{}";
        }

        _openSection = openSection >= 0 && openSection < _builderSchema.Sections.Count ? _builderSchema.Sections[openSection] : null;
        _openField = openField is { } at && at.Section < _builderSchema.Sections.Count && at.Field < _builderSchema.Sections[at.Section].Fields.Count
            ? _builderSchema.Sections[at.Section].Fields[at.Field]
            : null;
        _folded.Clear();
        foreach (var index in folded.Where(i => i < _builderSchema.Sections.Count))
        {
            _folded.Add(_builderSchema.Sections[index]);
        }
    }

    private (int Section, int Field)? FieldPosition(BuilderFieldModel field)
    {
        for (var s = 0; s < _builderSchema.Sections.Count; s++)
        {
            var f = _builderSchema.Sections[s].Fields.IndexOf(field);
            if (f >= 0)
            {
                return (s, f);
            }
        }

        return null;
    }

    // ---- the tabs ----

    /// <summary>A tab's selection. A result stays in its region: a tab press is no save, publish or discard (E02b).</summary>
    private void SelectTab(BuilderTab tab) => _tab = tab;

    /// <summary>
    /// Left, Right, Home and End move the focus along the tabs; Enter and Space select, as any button's press does (the
    /// tabs pattern, manual activation). No blanket preventDefault here: it would swallow Tab and trap the keyboard in the
    /// bar (build-lanes § "Settled here"); wombat.js stops these four keys alone from scrolling the page.
    /// </summary>
    private async Task OnTabKeyDownAsync(KeyboardEventArgs args, BuilderTab tab)
    {
        var tabs = Tabs;
        var index = tabs.ToList().IndexOf(tab);
        int? target = args.Key switch
        {
            "ArrowRight" => (index + 1) % tabs.Count,
            "ArrowLeft" => (index - 1 + tabs.Count) % tabs.Count,
            "Home" => 0,
            "End" => tabs.Count - 1,
            _ => null
        };

        if (target is int to)
        {
            await PageFocus.FocusByIdAsync(JS, TabId(tabs[to]));
        }
    }

    // ---- Details ----

    private void ChooseScope(ActivityTypeScopeChoiceDto choice)
    {
        // A target the caller may save at in it: the type's own where it is stored in this scope, else the scope's first.
        // Never none, so a save is never sent without a target (T300).
        _scope = choice.Scope;
        var stored = choice.Scope == _editor!.Scope ? choice.Targets.FirstOrDefault(target => target.Id == _editor.ScopeId) : null;
        _scopeIdText = (stored ?? choice.Targets.FirstOrDefault())?.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private string? DetailsRefusal(string inputId)
        => _refusal is not null && _refusal.Details.TryGetValue(inputId, out var words) ? words : null;

    // ---- Form ----

    private int TotalFields => _builderSchema.Sections.Sum(section => section.Fields.Count);

    // T239: each section's and field's buttons are named by what they act on, "Move section Assessment up". A section is
    // named by its title and a field by its label, or by its key while it has none, and by its key as well where another
    // reads the same.
    private IReadOnlyDictionary<int, string> SectionNames()
        => RowNames.Distinct(
            Enumerable.Range(0, _builderSchema.Sections.Count),
            index => index,
            index => BuilderRefusal.Name(_builderSchema.Sections[index]),
            index => $" ({_builderSchema.Sections[index].Key})");

    private IReadOnlyDictionary<BuilderFieldModel, string> FieldNames()
        => RowNames.Distinct(
            _builderSchema.Sections.SelectMany(section => section.Fields),
            field => field,
            BuilderRefusal.Name,
            field => $" ({field.Key})");

    private static bool OptionsRequired(FieldType type) => type is FieldType.Choice or FieldType.MultiChoice;

    /// <summary>
    /// The field keys eligible to be a root pointer: those of the type the parser demands (T133). The current pointer is
    /// included even when its field has been deleted, so the select shows what is set rather than silently reading as
    /// "None" (Q18); the save drops it either way, and the Publish tab says so.
    /// </summary>
    private IReadOnlyList<string> EligibleFieldKeys(FieldType fieldType)
    {
        var keys = _builderSchema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == fieldType && !string.IsNullOrWhiteSpace(field.Key))
            .Select(field => field.Key.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var current = fieldType switch
        {
            FieldType.Date => _builderSchema.ObservationDateField,
            FieldType.Scale => _builderSchema.RatedLevelField,
            FieldType.Epa => _builderSchema.EvidenceEpaField,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(current) && !keys.Contains(current, StringComparer.Ordinal))
        {
            keys.Insert(0, current);
        }

        return keys;
    }

    /// <summary>A setting's option by its field's label with the key after (T19): "Date delivered (delivered_on)".</summary>
    private string SettingOption(string key)
        => ActivityTypeEditWords.SettingOption(
            _builderSchema.Sections.SelectMany(section => section.Fields)
                .FirstOrDefault(field => string.Equals(field.Key.Trim(), key, StringComparison.Ordinal))?.Label ?? string.Empty,
            key);

    private string SettingText(string? key, string none) => string.IsNullOrWhiteSpace(key) ? none : SettingOption(key);

    private void OnObservationDateFieldChanged(ChangeEventArgs args) => _builderSchema.ObservationDateField = NullIfEmpty(args.Value?.ToString());

    private void OnRatedLevelFieldChanged(ChangeEventArgs args) => _builderSchema.RatedLevelField = NullIfEmpty(args.Value?.ToString());

    private void OnEvidenceEpaFieldChanged(ChangeEventArgs args) => _builderSchema.EvidenceEpaField = NullIfEmpty(args.Value?.ToString());

    /// <summary>
    /// The roles a User field may name, plus a stored value outside them so the select never shows a different role from
    /// the one the schema holds (as the tool picker keeps an unknown key, T122). The parser refuses such a value at save.
    /// </summary>
    private static IEnumerable<string> NomineeRoleChoices(string? current)
    {
        var choices = WombatRoles.Nominable.Where(role => role != WombatRoles.Assessor).ToList();
        if (!string.IsNullOrWhiteSpace(current) &&
            !string.Equals(current, WombatRoles.Assessor, StringComparison.Ordinal) &&
            !choices.Contains(current, StringComparer.Ordinal))
        {
            choices.Add(current);
        }

        return string.Equals(current, WombatRoles.Assessor, StringComparison.Ordinal)
            ? choices.Prepend(WombatRoles.Assessor)
            : choices;
    }

    /// <summary>
    /// The scales a Scale field may rate on (T271 item 2, S12): a stored <c>seed:</c> key resolves to its scale, and keeps
    /// its stored text as the option's value, so opening and saving the field changes nothing; a key no scale answers to is
    /// listed as stored.
    /// </summary>
    private IReadOnlyList<(string Value, string Name)> ScaleChoices(string? current)
    {
        var choices = _entrustmentScales
            .Select(scale => (Value: Binds(current, scale) ? current! : scale.Value, scale.Name))
            .ToList();
        if (!string.IsNullOrWhiteSpace(current) && choices.All(choice => !string.Equals(choice.Value, current, StringComparison.Ordinal)))
        {
            choices.Add((current, current));
        }

        return choices;
    }

    private string ScaleText(string? scaleKey) => string.IsNullOrWhiteSpace(scaleKey)
        ? ActivityTypeEditWords.None
        : _entrustmentScales.FirstOrDefault(scale => scale.Value == scaleKey || Binds(scaleKey, scale))?.Name ?? scaleKey;

    /// <summary>
    /// Whether a stored <c>scale_key</c> binds this scale, read as <see cref="Wombat.Domain.Epas.ScaleBinding" /> reads it:
    /// by id, by <c>seed:</c> key (the prefix is not the scale's seed key) or by name (T398 replay, 1.25).
    /// </summary>
    private static bool Binds(string? scaleKey, EntrustmentScaleOption scale)
    {
        var binding = Wombat.Domain.Epas.ScaleBinding.Parse(scaleKey);
        return binding is not null && binding.Kind switch
        {
            Wombat.Domain.Epas.ScaleBindingKind.Id => string.Equals(binding.ScaleId?.ToString(System.Globalization.CultureInfo.InvariantCulture), scale.Value, StringComparison.Ordinal),
            Wombat.Domain.Epas.ScaleBindingKind.SeedKey => string.Equals(binding.SeedKey, scale.SeedKey, StringComparison.Ordinal),
            _ => string.Equals(binding.Name, scale.Name, StringComparison.Ordinal)
        };
    }

    private static IReadOnlyList<string> OptionLines(string? optionsText) => (optionsText ?? string.Empty)
        .Split('\n')
        .Select(line => line.Trim())
        .Where(line => line.Length > 0)
        .ToList();

    /// <summary>The roles the preview's User fields would offer, for its person line (round 1's, R2-B01).</summary>
    private IReadOnlyList<string> PreviewNomineeRoles => _builderSchema.Sections
        .SelectMany(section => section.Fields)
        .Where(candidate => candidate.Type == FieldType.User)
        .Select(candidate => NullIfEmpty(candidate.NomineeRole) ?? WombatRoles.Assessor)
        .Distinct(StringComparer.Ordinal)
        .ToList();

    /// <summary>The preview's schema: the draft as typed, or the last one that could be written while a key is blank.</summary>
    private string PreviewSchemaJson
    {
        get
        {
            if (TryDraftSchemaJson() is { } json)
            {
                _lastPreviewSchemaJson = json;
            }

            return _lastPreviewSchemaJson.Length > 0 ? _lastPreviewSchemaJson : _editor?.DraftSchemaJson ?? string.Empty;
        }
    }

    private string? TryDraftSchemaJson()
    {
        try
        {
            return _builderSchema.ToJson();
        }
        catch (InvalidOperationException)
        {
            // A section or field with no key yet: the page refuses it at save, in its own words.
            return null;
        }
    }

    private Task OnPreviewChanged(string dataJson)
    {
        _previewDataJson = dataJson;
        return Task.CompletedTask;
    }

    /// <summary>Edit opens the field's editor under its row and focuses its first input; Done closes it, back to Edit (Q4).</summary>
    private void ToggleField(BuilderSectionModel section, BuilderFieldModel field)
    {
        if (ReferenceEquals(_openField, field))
        {
            _openField = null;
            _focusId = PositionOf(field) is { } at ? FieldButtonId(at.Section, at.Field, "edit") : null;
            return;
        }

        _openSection = null;
        _openField = field;
        _folded.Remove(section);
        _focusId = CanWrite ? "field-key" : null;
    }

    private void ToggleSection(BuilderSectionModel section)
    {
        var s = _builderSchema.Sections.IndexOf(section);
        if (ReferenceEquals(_openSection, section))
        {
            _openSection = null;
            _focusId = SectionButtonId(s, "edit");
            return;
        }

        _openField = null;
        _openSection = section;
        _focusId = CanWrite ? "section-key" : null;
    }

    private void ToggleFold(BuilderSectionModel section)
    {
        if (!_folded.Remove(section))
        {
            _folded.Add(section);
            if (section.Fields.Contains(_openField!))
            {
                _openField = null;
            }
        }
    }

    private void AddSection()
    {
        var fieldKey = _builderSchema.NextFieldKey();
        var section = new BuilderSectionModel
        {
            Key = _builderSchema.NextSectionKey(),
            Title = $"Section {_builderSchema.Sections.Count + 1}",
            Fields =
            [
                new BuilderFieldModel
                {
                    Key = fieldKey,
                    Label = "New field"
                }
            ]
        };
        _builderSchema.Sections.Add(section);
        _openField = null;
        _openSection = section;
        _focusId = "section-key";
    }

    /// <summary>S13: Add field adds the row and opens its editor under it, Field key focused.</summary>
    private void AddField(BuilderSectionModel section)
    {
        var field = new BuilderFieldModel
        {
            Key = _builderSchema.NextFieldKey(),
            Label = "New field"
        };
        section.Fields.Add(field);
        _openSection = null;
        _openField = field;
        _focusId = "field-key";
    }

    private void MoveSection(BuilderSectionModel section, int delta)
    {
        var sections = _builderSchema.Sections;
        var from = sections.IndexOf(section);
        var to = from + delta;
        if (from < 0 || to < 0 || to >= sections.Count)
        {
            return;
        }

        (sections[from], sections[to]) = (sections[to], sections[from]);

        // The pressed button moved with its row; where it is now the first or last, its twin keeps the focus.
        var stuck = (delta < 0 && to == 0) || (delta > 0 && to == sections.Count - 1);
        _focusId = SectionButtonId(to, (delta < 0) ^ stuck ? "up" : "down");
    }

    private void MoveField(BuilderSectionModel section, BuilderFieldModel field, int delta)
    {
        var fields = section.Fields;
        var from = fields.IndexOf(field);
        var to = from + delta;
        if (from < 0 || to < 0 || to >= fields.Count)
        {
            return;
        }

        (fields[from], fields[to]) = (fields[to], fields[from]);
        var s = _builderSchema.Sections.IndexOf(section);
        var stuck = (delta < 0 && to == 0) || (delta > 0 && to == fields.Count - 1);
        _focusId = FieldButtonId(s, to, (delta < 0) ^ stuck ? "up" : "down");
    }

    /// <summary>Q8: nothing asked, nothing saved until Save draft; the focus to the next row's Edit, else Add section.</summary>
    private void DeleteSection(BuilderSectionModel section)
    {
        var s = _builderSchema.Sections.IndexOf(section);
        _builderSchema.Sections.RemoveAt(s);
        if (ReferenceEquals(_openSection, section) || section.Fields.Contains(_openField!))
        {
            _openSection = null;
            _openField = null;
        }

        _folded.Remove(section);
        _focusId = s < _builderSchema.Sections.Count ? SectionButtonId(s, "edit") : AddSectionId;
    }

    /// <summary>Q8: nothing asked; the focus to the next row's Edit, else the section's Add field.</summary>
    private void DeleteField(BuilderSectionModel section, BuilderFieldModel field)
    {
        var s = _builderSchema.Sections.IndexOf(section);
        var f = section.Fields.IndexOf(field);
        section.Fields.RemoveAt(f);
        if (ReferenceEquals(_openField, field))
        {
            _openField = null;
        }

        _focusId = f < section.Fields.Count ? FieldButtonId(s, f, "edit") : AddFieldId(s);
    }

    private (int Section, int Field)? PositionOf(BuilderFieldModel field) => FieldPosition(field);

    // ---- Workflow and Credit ----

    /// <summary>The JSON editor's height: its lines, from 8 to 30.</summary>
    private static int JsonRows(string text) => Math.Clamp(text.Count(character => character == '\n') + 2, 8, 30);

    // ---- Publish ----

    /// <summary>What publishing the saved draft changes (S6: never the edits since), by label (N1).</summary>
    private IReadOnlyList<string> SavedWarnings
    {
        get
        {
            try
            {
                return BuilderSchemaModel.GetPublishWarnings(_editor?.PublishedSchemaJson, _editor?.DraftSchemaJson ?? string.Empty);
            }
            catch (SchemaParseException)
            {
                return [];
            }
        }
    }

    private IReadOnlyList<string> SavedRemovedLabels
    {
        get
        {
            try
            {
                return BuilderSchemaModel.RemovedFieldLabels(_editor?.PublishedSchemaJson, _editor?.DraftSchemaJson ?? string.Empty);
            }
            catch (SchemaParseException)
            {
                return [];
            }
        }
    }

    /// <summary>R3-E01's Form line, from the saved draft: "1 section, 6 fields (5 required)".</summary>
    private string? SavedFormSummary
    {
        get
        {
            try
            {
                var schema = FormSchemaParser.Parse(_editor!.DraftSchemaJson);
                var fields = schema.Sections.SelectMany(section => section.Fields).ToList();
                return ActivityTypeEditWords.FormSummary(schema.Sections.Count, fields.Count, fields.Count(candidate => candidate.Required));
            }
            catch (SchemaParseException)
            {
                return null;
            }
        }
    }

    /// <summary>R3-E01's Workflow line, from the read of the saved draft.</summary>
    private string? SavedWorkflowSummary
        => _editor?.WorkflowRead is { } read && read.States.FirstOrDefault(state => state.IsInitial) is { } initial
            ? ActivityTypeEditWords.WorkflowSummary(
                initial.Label,
                read.States.Where(state => !state.IsInitial).Select(state => state.Label).ToList(),
                read.Moves.Count)
            : null;

    /// <summary>
    /// Whether anything on the page differs from the saved draft (S6, Q16), judged canonical to canonical: each JSON is
    /// parsed and compared as JSON, never as text (CLAUDE.md), since the saved text is laid out by Wombat.
    /// </summary>
    private bool HasUnsavedEdits
    {
        get
        {
            if (_editor is null || !CanWrite)
            {
                return false;
            }

            if (_editor.Id <= 0)
            {
                return true;
            }

            if (!string.Equals(_key, _editor.Key, StringComparison.Ordinal) ||
                !string.Equals(_name, _editor.Name, StringComparison.Ordinal) ||
                !string.Equals(NullIfEmpty(_description), NullIfEmpty(_editor.Description), StringComparison.Ordinal) ||
                _scope != _editor.Scope ||
                ParseNullableInt(_scopeIdText) != _editor.ScopeId ||
                _isActive != _editor.IsActive ||
                !string.Equals(NullIfEmpty(_wbaToolKey), NullIfEmpty(_editor.WbaToolKey), StringComparison.Ordinal))
            {
                return true;
            }

            // The saved schema is put through the same builder as the page's, so both sides are written alike.
            return TryDraftSchemaJson() is not { } schema ||
                   !SameJson(schema, SavedSchemaThroughBuilder) ||
                   !SameJson(_workflowJson, _editor.DraftWorkflowJson) ||
                   !SameJson(_creditRulesJson, _editor.DraftCreditRulesJson);
        }
    }

    private string SavedSchemaThroughBuilder
    {
        get
        {
            try
            {
                return BuilderSchemaModel.Parse(_editor!.DraftSchemaJson).ToJson();
            }
            catch (Exception exception) when (exception is SchemaParseException or InvalidOperationException)
            {
                return _editor!.DraftSchemaJson;
            }
        }
    }

    private static bool SameJson(string left, string right)
    {
        try
        {
            return JsonNode.DeepEquals(JsonNode.Parse(left), JsonNode.Parse(right));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // ---- results ----

    private void ClearResults()
    {
        _refusal = null;
        _summaryLines = [];
        _goTo.Clear();
        _status = null;
        _failure = null;
        _guard = null;
    }

    private void ShowRefusal(BuilderRefusal refusal)
    {
        _refusal = refusal;
        _goTo.Clear();
        var lines = new List<RefusalLine>();
        foreach (var line in refusal.Lines)
        {
            var refusalLine = new RefusalLine(null, line.Text, null) { TargetId = TargetIdOf(line.GoTo) };
            _goTo[refusalLine] = line.GoTo;
            lines.Add(refusalLine);
        }

        _summaryLines = lines;
        _focusSummary = true;
    }

    private static string TargetIdOf(BuilderGoTo goTo)
        => goTo.DetailsInputId
           ?? (goTo.Field is not null ? "field-key"
               : goTo.Section is not null ? "section-key"
               : goTo.Json ? (goTo.Tab == BuilderTab.Workflow ? WorkflowJsonId : CreditJsonId)
               : TabId(goTo.Tab));

    /// <summary>
    /// A summary line's press (Q6, S1): its tab, then its place: the row's editor opened in place (its section unfolded)
    /// and its key focused; the section's editor; the JSON editor; a Details control. A line with no place focuses its tab.
    /// </summary>
    private Task GoToLineAsync(RefusalLine line)
    {
        if (!_goTo.TryGetValue(line, out var goTo))
        {
            return Task.CompletedTask;
        }

        _tab = goTo.Tab;
        if (goTo.Field is { } field && goTo.Section is { } section && section.Fields.Contains(field))
        {
            _folded.Remove(section);
            _openSection = null;
            _openField = field;
        }
        else if (goTo.Section is { } onlySection && _builderSchema.Sections.Contains(onlySection))
        {
            _openField = null;
            _openSection = onlySection;
        }

        _focusId = TargetIdOf(goTo);
        return Task.CompletedTask;
    }

    /// <summary>
    /// What the page refuses before it sends anything, in the command's and the parser's words: Details' four (S10), and
    /// a section or a field left with no key, which no schema can be written without (NEW).
    /// </summary>
    private BuilderRefusal? CheckBeforeSend()
    {
        var refusal = new BuilderRefusal(ActivityTypeEditWords.NothingWasSaved);
        if (!IsPublished)
        {
            if (string.IsNullOrWhiteSpace(_key))
            {
                refusal.AddDetails(KeyInputId, ActivityTypeEditWords.EnterKey);
            }
            else if (_key.Trim().Length > 100)
            {
                refusal.AddDetails(KeyInputId, ActivityTypeEditWords.KeyTooLong);
            }
        }

        if (string.IsNullOrWhiteSpace(_name))
        {
            refusal.AddDetails(NameInputId, ActivityTypeEditWords.EnterName);
        }
        else if (_name.Trim().Length > 200)
        {
            refusal.AddDetails(NameInputId, ActivityTypeEditWords.NameTooLong);
        }

        foreach (var section in _builderSchema.Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Key))
            {
                refusal.AddSection(section, BuilderRefusal.Name(section), ActivityTypeEditWords.EnterSectionKey);
            }

            foreach (var field in section.Fields.Where(field => string.IsNullOrWhiteSpace(field.Key)))
            {
                refusal.AddField(section, field, ActivityTypeEditWords.EnterFieldKey);
            }
        }

        return refusal.IsEmpty ? null : refusal;
    }

    /// <summary>
    /// A refused or failed save, publish or discard (P1): a refusal in its own words at its places; the guard's sentence in
    /// the region; anything else the fixed failure, logged, never the exception's text.
    /// </summary>
    private void Refused(Exception exception, string title, string failureLead, string what)
    {
        if (PlacesOf(exception) is { } places)
        {
            var refusal = new BuilderRefusal(title);
            refusal.AddPlaces(places, _builderSchema, _workflowJson, _editor?.WorkflowRead);
            ShowRefusal(refusal);
            return;
        }

        if (exception is ValidationException validation && validation.Errors.Any())
        {
            var refusal = new BuilderRefusal(title);
            foreach (var failure in validation.Errors.Where(failure => !string.IsNullOrWhiteSpace(failure.ErrorMessage)))
            {
                AddDetailsRefusal(refusal, failure.PropertyName, failure.ErrorMessage.Trim());
            }

            ShowRefusal(refusal);
            return;
        }

        if (exception is FieldRefusalException field)
        {
            var refusal = new BuilderRefusal(title);
            AddDetailsRefusal(refusal, field.Field, field.Message);
            ShowRefusal(refusal);
            return;
        }

        if (exception is UnauthorizedAccessException guard)
        {
            // The guard's sentence (N10, or the scope's), in the region with no place: no page offers its control.
            _guard = guard.Message;
            _focusResult = true;
            return;
        }

        Logger.LogError(exception, "The activity type {ActivityTypeId} could not be {What}.", _editor?.Id, what);
        _failure = (failureLead, ActivityTypeEditWords.FailureRest);
        _focusResult = true;
    }

    private static void AddDetailsRefusal(BuilderRefusal refusal, string property, string message)
    {
        switch (property)
        {
            case nameof(SaveActivityTypeDraftCommand.Key):
                refusal.AddDetails(KeyInputId, message);
                break;
            case nameof(SaveActivityTypeDraftCommand.Name):
                refusal.AddDetails(NameInputId, message);
                break;
            case nameof(SaveActivityTypeDraftCommand.Description):
                // The fix pass (N-1): the counter stops at the column, so only a typed-around page reaches it.
                refusal.AddDetails(DescriptionInputId, message);
                break;
            default:
                refusal.AddDetailsLine(message);
                break;
        }
    }

    /// <summary>
    /// A refusal's places: the Domain's own (<see cref="IActivityTypeRefusal" />; a parse exception built without places
    /// gives one place of its own tab with its message).
    /// </summary>
    private static IReadOnlyList<RefusalPlace>? PlacesOf(Exception exception) => exception switch
    {
        IActivityTypeRefusal refusal => refusal.Places,
        _ => null
    };

    // ---- Save draft ----

    private enum SaveOutcome
    {
        Saved,
        Left,
        Refused
    }

    /// <remarks>
    /// Sent at most once at a time (T234). With Active newly unticked on a published type it asks Q15's question first
    /// (R3-E11). A save that is done moves the focus to its result, unless it created the type: the page then moves to the
    /// type's own address with the created token, which reads it again and says "Draft saved." (T291 item 3).
    /// </remarks>
    private async Task SaveDraftAsync()
    {
        if (_inFlight is not null || !CanWrite)
        {
            return;
        }

        ClearResults();
        if (CheckBeforeSend() is { } refusal)
        {
            ShowRefusal(refusal);
            return;
        }

        if (WouldDeactivate && _deactivateDialog is not null)
        {
            _deactivateFor = SaveAction;
            await _deactivateDialog.ShowAsync(SaveButtonId);
            return;
        }

        _inFlight = SaveAction;
        try
        {
            await SaveAndSayAsync();
        }
        finally
        {
            _inFlight = null;
        }
    }

    private async Task SaveAndSayAsync()
    {
        var wasActive = _editor!.IsActive;
        var published = IsPublished;
        if (await SaveCoreAsync() == SaveOutcome.Saved)
        {
            _status = (ActivityTypeEditWords.DraftSaved, published ? ActiveChange(wasActive) : null);
            _focusResult = true;
        }
    }

    /// <summary>Q15's results: "… is inactive: registrars are no longer offered it." / "… is active again: …".</summary>
    private string? ActiveChange(bool wasActive) => (wasActive, _editor!.IsActive) switch
    {
        (true, false) => ActivityTypeEditWords.Inactive(_editor.Name),
        (false, true) => ActivityTypeEditWords.ActiveAgain(_editor.Name),
        _ => null
    };

    private async Task<SaveOutcome> SaveCoreAsync()
    {
        try
        {
            var user = await UserAsync();
            var wasNew = (_editor?.Id ?? 0) == 0;
            var schemaJson = _builderSchema.ToJson();
            var editor = await Sender.Send(new SaveActivityTypeDraftCommand(
                wasNew ? null : _editor!.Id,
                _key,
                _name,
                _description,
                _scope,
                ParseNullableInt(_scopeIdText),
                _isActive,
                NullIfEmpty(_wbaToolKey),
                schemaJson,
                _workflowJson,
                _creditRulesJson,
                BuilderSchemaModel.BuildDisplayFieldsJson(_builderSchema),
                UserIdOf(user),
                user));

            if (wasNew && editor.Id > 0)
            {
                Navigation.NavigateTo(CatalogueLinks.ActivityTypeCreated(editor.Id));
                return SaveOutcome.Left;
            }

            // The JSON comes back laid out by Wombat (Q13, S11); a setting at a deleted field comes back dropped (Q18).
            _editor = editor;
            LoadEditorState(editor, keepUi: true);
            return SaveOutcome.Saved;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Refused(exception, ActivityTypeEditWords.NothingWasSaved, ActivityTypeEditWords.SaveFailedLead, "saved");
            return SaveOutcome.Refused;
        }
    }

    // ---- Publish ----

    /// <summary>
    /// Publish, from the header or the Publish tab (Q1, Q16, Q17, S5). Inert without a saved draft. The tab is kept: the
    /// result lands above the tabs on the tab it was pressed from (Q20).
    /// </summary>
    private async Task PublishAsync(string triggerId)
    {
        if (_inFlight is not null || !CanWrite || !CanPublish)
        {
            return;
        }

        ClearResults();
        if (HasUnsavedEdits && CheckBeforeSend() is { } refusal)
        {
            ShowRefusal(refusal);
            return;
        }

        _publishTrigger = triggerId;
        if (WouldDeactivate && _deactivateDialog is not null)
        {
            _deactivateFor = PublishAction;
            await _deactivateDialog.ShowAsync(triggerId);
            return;
        }

        _inFlight = PublishAction;
        try
        {
            await ContinuePublishAsync(deactivating: false);
        }
        finally
        {
            _inFlight = null;
        }
    }

    /// <summary>Q16: save the edits since the draft was saved, then ask on warnings, then publish.</summary>
    private async Task ContinuePublishAsync(bool deactivating)
    {
        var savedFirst = false;
        if (HasUnsavedEdits)
        {
            if (await SaveCoreAsync() != SaveOutcome.Saved)
            {
                return;
            }

            savedFirst = true;
        }

        var warnings = SavedWarnings;
        if (warnings.Count > 0 && _publishDialog is not null)
        {
            _pendingPublish = new PendingPublish(deactivating, savedFirst, warnings, SavedRemovedLabels);

            // One dialog at a time: Q15's closes before the publish question opens (Q17).
            if (_deactivateDialog is not null)
            {
                await _deactivateDialog.CloseAsync();
            }

            await _publishDialog.ShowAsync(_publishTrigger);
            return;
        }

        await PublishCoreAsync(deactivating);
    }

    private async Task ConfirmPublishAsync()
    {
        if (_inFlight is not null || _pendingPublish is not { } pending)
        {
            return;
        }

        _pendingPublish = null;
        _inFlight = PublishAction;
        try
        {
            await PublishCoreAsync(pending.Deactivating);
        }
        finally
        {
            _inFlight = null;
        }
    }

    /// <summary>
    /// The publish question closed unanswered, by Cancel or Escape: nothing is published; what was saved first says so
    /// (Q16's "Draft saved. Nothing was published.", Q17's with the deactivation). The focus goes back to Publish.
    /// </summary>
    private Task PublishQuestionClosedAsync()
    {
        if (_pendingPublish is not { } pending)
        {
            return Task.CompletedTask;
        }

        _pendingPublish = null;
        if (pending.SavedFirst)
        {
            _status = (ActivityTypeEditWords.DraftSaved, pending.Deactivating
                ? $"{ActivityTypeEditWords.Inactive(_editor!.Name)} {ActivityTypeEditWords.NothingWasPublished}"
                : ActivityTypeEditWords.NothingWasPublished);
        }

        return Task.CompletedTask;
    }

    private async Task PublishCoreAsync(bool deactivating)
    {
        var next = NextVersion;
        try
        {
            var user = await UserAsync();
            var editor = await Sender.Send(new PublishActivityTypeDraftCommand(_editor!.Id, UserIdOf(user), user));
            _editor = editor;
            LoadEditorState(editor, keepUi: true);
            _status = (ActivityTypeEditWords.Published(editor.PublishedVersion), deactivating ? ActivityTypeEditWords.Inactive(editor.Name) : null);
            _focusResult = true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Refused(exception, ActivityTypeEditWords.NothingWasPublished, ActivityTypeEditWords.PublishFailedLead(next), "published");
        }
    }

    // ---- Q15's question ----

    /// <summary>Q15 answered Deactivate: the save it asked for, or Publish's sequence (Q17).</summary>
    private async Task ConfirmDeactivateAsync()
    {
        if (_inFlight is not null)
        {
            return;
        }

        var action = _deactivateFor;
        _inFlight = action;
        try
        {
            if (action == PublishAction)
            {
                await ContinuePublishAsync(deactivating: true);
            }
            else
            {
                await SaveAndSayAsync();
            }
        }
        finally
        {
            _inFlight = null;
        }
    }

    // ---- Discard draft ----

    private async Task AskDiscardAsync()
    {
        if (_inFlight is not null || !CanDiscard || _discardDialog is null)
        {
            return;
        }

        await _discardDialog.ShowAsync(DiscardButtonId);
    }

    private async Task ConfirmDiscardAsync()
    {
        if (_inFlight is not null || !CanDiscard)
        {
            return;
        }

        _inFlight = DiscardAction;
        ClearResults();
        try
        {
            var user = await UserAsync();
            var editor = await Sender.Send(new DiscardActivityTypeDraftCommand(_editor!.Id, user));
            _editor = editor;
            LoadEditorState(editor, keepUi: false);
            _openField = null;
            _openSection = null;
            _status = (ActivityTypeEditWords.DraftDiscarded, null);
            _focusResult = true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Refused(exception, ActivityTypeEditWords.NothingWasSaved, ActivityTypeEditWords.DiscardFailedLead, "discarded");
        }
        finally
        {
            _inFlight = null;
        }
    }

    // ---- focus ----

    /// <remarks>
    /// The focus moves once the browser has the render that shows the result, after any dialog that sent it has closed
    /// (T206 review). A ?created arrival's result is focused the same way, and only then is the token taken out.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_focusSummary && _summary is not null)
        {
            _focusSummary = false;
            await _summary.FocusAsync();
        }
        else if (_focusResult && _result is not null)
        {
            _focusResult = false;
            await _result.Element.FocusAsync();
        }

        if (_focusId is { } id)
        {
            _focusId = null;
            await PageFocus.FocusByIdAsync(JS, id);
        }

        if (_stripToken)
        {
            _stripToken = false;
            Navigation.NavigateTo(CatalogueLinks.WithoutTokens(Navigation.Uri), replace: true);
        }
    }

    // ---- helpers ----

    private async Task<ClaimsPrincipal> UserAsync() => (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;

    private static string UserIdOf(ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? throw new InvalidOperationException("The current user identifier is missing.");

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseNullableInt(string? value)
        => int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
}
