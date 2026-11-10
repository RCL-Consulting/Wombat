using System.Globalization;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

/// <summary>
/// The builder's own words (T398, flow 17, lane C): the boards' R3-B07, C01–C23, D01–D05, E01–E17 and the standing round 2
/// boards, R3-0-spec-4 § 9 and R2-0-spec-3's messages. The words both activity-type pages print are
/// <see cref="ActivityTypeWords" />'s (§ 0). A sentence no board draws is marked NEW (build-contracts § 5).
/// </summary>
public static class ActivityTypeEditWords
{
    // ---- the page ----

    public const string Subtitle = "What a registrar fills in, who moves it on, and what it counts towards.";

    public const string NewTitle = "New activity type";

    /// <summary>The h1 while the type is read, and after its read failed (T21).</summary>
    public const string LoadingTitle = "Activity type";

    public static string EditTitle(string name) => $"Edit {name}";

    public const string LoadingLine = "Loading this activity type.";

    public const string LoadFailed =
        "Could not load this activity type. Nothing has changed. Try again, or come back in a few minutes.";

    public const string NoScopeNotice =
        "You have no scope to create an activity type in. An institution's administrators create their institution's " +
        "types, and a College's administrators the types of its specialities and sub-specialities.";

    // ---- the header ----

    public const string SaveDraft = "Save draft";

    public const string Publish = "Publish";

    /// <summary>"…" on a trigger that opens a dialog (round 1's M7), never on Publish.</summary>
    public const string DiscardDraftTrigger = "Discard draft…";

    public const string PublishReason = "Save a draft to publish.";

    // ---- the version strip ----

    public const string PublishedTerm = "Published";

    public const string DraftTerm = "Draft";

    /// <summary>"Version 1, 2026-11-05, Nolwazi Mbatha" (R2-B01, R3-E02); the publisher left out when it is not known.</summary>
    public static string PublishedLine(int version, DateTime? publishedOnUtc, string? publishedBy)
    {
        if (version <= 0)
        {
            return ActivityTypeWords.Published(0);
        }

        var parts = new List<string> { ActivityTypeWords.Published(version) };
        if (publishedOnUtc is DateTime on)
        {
            parts.Add(Day(on));
        }

        if (!string.IsNullOrWhiteSpace(publishedBy))
        {
            parts.Add(publishedBy);
        }

        return string.Join(", ", parts);
    }

    /// <summary>"Saved 2026-11-05 by Nolwazi Mbatha" (R3-C03), or "None".</summary>
    public static string DraftLine(bool hasDraft, DateTime? savedOnUtc, string? savedBy)
    {
        if (!hasDraft)
        {
            return ActivityTypeWords.Draft(null);
        }

        var saved = savedOnUtc is DateTime on ? ActivityTypeWords.Draft(on) : "Saved";
        return string.IsNullOrWhiteSpace(savedBy) ? saved : $"{saved} by {savedBy}";
    }

    /// <summary>T18's note under the strip on a seed-owned type with a draft (R2-E15).</summary>
    public const string SeedDraftNote =
        "While this draft is saved, updates from Wombat's catalogue stop reaching this type, until you publish or discard it.";

    /// <summary>T18's line on the Publish tab of a seed-owned type with a draft (R2-E15).</summary>
    public const string SeedPublishNote = "Once you publish it, updates from Wombat's catalogue no longer reach this type.";

    /// <summary>The owner line under the subtitle: "Kgosi Kgari Teaching Hospital · Institution" (R3-C01, R3-C07).</summary>
    public static string OwnerLine(string setBy, string scopeDetail) => $"{setBy} · {scopeDetail}";

    // ---- the tabs ----

    public const string TabsLabel = "Parts of this activity type";

    /// <summary>The word after a tab's count, part of its name: "Form 1 problem", "Form 2 problems" (M2).</summary>
    public static string ProblemWord(int count) => count == 1 ? " problem" : " problems";

    // ---- Details ----

    public const string DetailsHeading = "Details";

    /// <summary>Q14's line at the head of Details.</summary>
    public const string DetailsLine = "Saved with Save draft, not with Publish: a change here reaches everyone when you save.";

    public const string KeyLabel = "Key";

    public const string KeyHelp = "Fixed once published.";

    public static string KeyFixed(int version)
        => string.Create(CultureInfo.InvariantCulture, $"Fixed: version {version} is published.");

    public const string NameLabel = "Name";

    public const string ScopeLabel = "Scope";

    public const string OneScope = "The one scope you can write in.";

    public const string TargetLabel = "Target";

    public const string GlobalTarget = "None: a Global type belongs to no institution or speciality.";

    public const string SetByTerm = "Set by";

    public const string ToolLabel = "This tool is";

    public const string NotAnInstrument = "Not a WBA instrument";

    public const string ToolHelp =
        "Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. " +
        "'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and " +
        "discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed " +
        "updates stop reaching the type.";

    public const string DescriptionLabel = "Description";

    /// <summary>The description's limit, counted under it (R3-0-spec-4 § 10).</summary>
    public const int DescriptionLimit = 2000;

    /// <summary>"94 of 2,000 characters" (R3-C03).</summary>
    public static string DescriptionCounter(int length)
        => string.Create(CultureInfo.InvariantCulture, $"{length:N0} of {DescriptionLimit:N0} characters");

    public const string ActiveLabel = "Active";

    /// <summary>N2's help under Active.</summary>
    public const string ActiveHelp =
        "An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.";

    public const string StatusTerm = "Status";

    public static string Status(bool isActive) => isActive ? "Active" : "Inactive";

    public const string None = "None";

    public static string ScopeName(Wombat.Domain.Activities.ActivityScope scope) => scope switch
    {
        Wombat.Domain.Activities.ActivityScope.Global => "Global",
        Wombat.Domain.Activities.ActivityScope.Institution => "Institution",
        Wombat.Domain.Activities.ActivityScope.Speciality => "Speciality",
        _ => "Sub-speciality"
    };

    // ---- Form ----

    public const string ShowLegend = "Show";

    public const string ShowFormWriter = "Edit the form";

    public const string ShowFormReader = "The form";

    public const string ShowPreview = "Preview";

    public const string FormSettingsHeading = "Form settings";

    public const string FormSettingsHelp =
        "Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.";

    public const string EncounterDateLabel = "Encounter date field";

    public const string RatingLabel = "Entrustment rating field";

    public const string EpaFieldLabel = "EPA field";

    public const string EpaFieldHelp =
        "The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that " +
        "match on an EPA field must use this one.";

    public const string NoEncounterDate = "None — this form has no single encounter date";

    public const string NoRating = "None — this form asserts no entrustment level";

    public const string NoEpaField = "None — this form is about no single EPA";

    /// <summary>A setting by its field's label with the key after: "Date delivered (delivered_on)" (T19, R3-C17).</summary>
    public static string SettingOption(string label, string key)
        => string.IsNullOrWhiteSpace(label) || string.Equals(label.Trim(), key, StringComparison.Ordinal) ? key : $"{label.Trim()} ({key})";

    public const string SectionsHeading = "Sections and fields";

    public const string AddSection = "Add section";

    public const string AddField = "Add field";

    public const string Edit = "Edit";

    public const string Done = "Done";

    public const string View = "View";

    public const string Close = "Close";

    public const string Up = "Up";

    public const string Down = "Down";

    public const string Delete = "Delete";

    public const string HideFields = "Hide fields";

    public const string ShowFields = "Show fields";

    /// <summary>The folds appear above this many fields in the form (T23, M2(b), S17).</summary>
    public const int FoldAbove = 20;

    public static string HideFieldsOf(string section) => $"Hide fields of section {section}";

    public static string ShowFieldsOf(string section) => $"Show fields of section {section}";

    public static string EditSection(string section) => $"Edit section {section}";

    public static string DoneWithSection(string section) => $"Done with section {section}";

    public static string ViewSection(string section) => $"View section {section}";

    public static string CloseSection(string section) => $"Close section {section}";

    public static string MoveSectionUp(string section) => $"Move section {section} up";

    public static string MoveSectionDown(string section) => $"Move section {section} down";

    public static string DeleteSection(string section) => $"Delete section {section}";

    public static string AddFieldTo(string section) => $"Add field to section {section}";

    public static string EditField(string field) => $"Edit field {field}";

    public static string DoneWithField(string field) => $"Done with field {field}";

    public static string ViewField(string field) => $"View field {field}";

    public static string CloseField(string field) => $"Close field {field}";

    public static string MoveFieldUp(string field) => $"Move field {field} up";

    public static string MoveFieldDown(string field) => $"Move field {field} down";

    public static string DeleteField(string field) => $"Delete field {field}";

    /// <summary>The editor in place's h4: "Field Topic", "Section Teaching session" (R3-C10, C11).</summary>
    public static string FieldHeading(string field) => $"Field {field}";

    public static string SectionHeading(string section) => $"Section {section}";

    public const string NoFieldsYet = "No fields in this section yet.";

    public const string Required = "Required";

    public const string SectionKeyLabel = "Section key";

    public const string TitleLabel = "Title";

    public const string FieldKeyLabel = "Field key";

    public const string LabelLabel = "Label";

    public const string TypeLabel = "Type";

    public const string HelpTextLabel = "Help text";

    public const string NamesALabel = "Names a";

    public const string NamesAHelp = "Who this field may name: someone at the trainee's institution holding this role.";

    public const string DefaultRole = "Assessor (default)";

    public const string OptionsLabel = "Options";

    public const string OptionsHelp =
        "One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the " +
        "label, and the activity stores the value.";

    public const string CatalogueKeyLabel = "Catalogue key";

    public const string ScaleLabel = "Entrustment scale";

    public const string ScaleHelp = "Levels of the chosen scale render as the rating dropdown at runtime.";

    public const string ScaleNone = "Select…";

    public const string Yes = "Yes";

    public const string No = "No";

    public const string LivePreview = "Live preview";

    /// <summary>
    /// The preview's person line under a User field (round 1's, R2-B01): the preview has no registrar, so it offers nobody.
    /// </summary>
    public static string PersonLine(string role)
        => $"In the preview this offers nobody: it offers an active {WombatRoleLabels.For(role)} at the registrar's institution, never the registrar.";

    /// <summary>A field type by its label (T324): "Long text", never "LongText".</summary>
    public static string FieldTypeLabel(FieldType type) => type switch
    {
        FieldType.LongText => "Long text",
        FieldType.MultiChoice => "Multi-choice",
        FieldType.Epa => "EPA",
        FieldType.ProcedureRef => "Procedure reference",
        _ => type.ToString()
    };

    /// <summary>A role as a person with its article: "an Assessor", "a Committee member" (T24).</summary>
    public static string RoleWithArticle(string role)
    {
        var label = WombatRoleLabels.For(role);
        return label.Length > 0 && "AEIOU".Contains(char.ToUpperInvariant(label[0]), StringComparison.Ordinal)
            ? $"an {label}"
            : $"a {label}";
    }

    /// <summary>A row's type line: "User · Names an Assessor" (R3-C13).</summary>
    public static string NamesA(string? role) => $"Names {RoleWithArticle(role ?? WombatRoles.Assessor)}";

    // ---- Workflow and Credit ----

    public const string WorkflowHeading = "Workflow";

    public const string CreditHeading = "Credit";

    public const string WorkflowJsonLabel = "Workflow JSON";

    public const string CreditJsonLabel = "Credit rules JSON";

    /// <summary>The JSON editors' help (T10, R3-D01).</summary>
    public const string JsonHelp = "After each save this shows the rules as they were saved, laid out by Wombat, one property a line.";

    // ---- Publish ----

    public const string PublishHeading = "Publish";

    public static string NoDraftPublished(int version)
        => string.Create(CultureInfo.InvariantCulture, $"There is no draft to publish: version {version} is what registrars file. Save a draft to publish.");

    public const string NothingPublished = "Nothing is published yet. Save a draft to publish.";

    /// <summary>S6: the Publish tab reads the saved draft; with edits unsaved it says so above its list.</summary>
    public const string SaveToSee = "Save draft to see what publishing changes.";

    public const string WhatPublishingChanges = "What publishing this draft changes";

    public const string WhatYouArePublishing = "What you are publishing";

    public const string NothingToCompare = "Nothing to compare: this is the first version.";

    public const string NothingChanges = "Nothing on the form changes for activities already filed.";

    /// <summary>
    /// The pinned line (R3-E04, C18b): "Activities already filed stay on version 1, with Audience as they filed it. Only
    /// activities filed after the publish use version 2." With no field removed, the clause about one goes (NEW).
    /// </summary>
    public static string PinnedLine(int published, IReadOnlyList<string> removedLabels)
    {
        var next = published + 1;
        var kept = removedLabels.Count == 0
            ? string.Create(CultureInfo.InvariantCulture, $"Activities already filed stay on version {published}.")
            : string.Create(CultureInfo.InvariantCulture, $"Activities already filed stay on version {published}, with {Joined(removedLabels)} as they filed it.");
        return string.Create(CultureInfo.InvariantCulture, $"{kept} Only activities filed after the publish use version {next}.");
    }

    public static string PublishVersion(int version) => string.Create(CultureInfo.InvariantCulture, $"Publish version {version}");

    public const string FormTerm = "Form";

    public const string WorkflowTerm = "Workflow";

    public const string CreditTerm = "Credit";

    /// <summary>"1 section, 6 fields (5 required)" (R3-E01).</summary>
    public static string FormSummary(int sections, int fields, int required)
        => string.Create(CultureInfo.InvariantCulture,
            $"{sections} {(sections == 1 ? "section" : "sections")}, {fields} {(fields == 1 ? "field" : "fields")} ({required} required)");

    /// <summary>
    /// The workflow in one line (R3-E01, NEW wording): "From Draft to Logged or Cancelled; 2 moves". The board's arrow is
    /// words here: no arrow is a shipped character (build-lanes § Never).
    /// </summary>
    public static string WorkflowSummary(string initial, IReadOnlyList<string> others, int moves)
    {
        var count = string.Create(CultureInfo.InvariantCulture, $"{moves} {(moves == 1 ? "move" : "moves")}");
        return others.Count == 0 ? $"{initial}; {count}" : $"From {initial} to {JoinedOr(others)}; {count}";
    }

    /// <summary>"Credits nothing" (R3-E01), or "1 credit rule" (NEW).</summary>
    public static string CreditSummary(bool creditsNothing, int rules)
        => creditsNothing || rules == 0
            ? "Credits nothing"
            : string.Create(CultureInfo.InvariantCulture, $"{rules} {(rules == 1 ? "credit rule" : "credit rules")}");

    // ---- results ----

    public const string DraftSaved = "Draft saved.";

    public const string DraftDiscarded = "Draft discarded.";

    public const string NothingWasPublished = "Nothing was published.";

    public static string Published(int version) => string.Create(CultureInfo.InvariantCulture, $"Published version {version}.");

    public static string Inactive(string name) => $"{name} is inactive: registrars are no longer offered it.";

    public static string ActiveAgain(string name) => $"{name} is active again: registrars are offered it.";

    // ---- failures (fixed words; build-lanes § "Refusals and failures") ----

    public const string SaveFailedLead = "The draft could not be saved.";

    public static string PublishFailedLead(int version)
        => string.Create(CultureInfo.InvariantCulture, $"Version {version} could not be published.");

    public const string DiscardFailedLead = "The draft could not be discarded.";

    public const string FailureRest = "Nothing has changed. Try again, or come back in a few minutes.";

    // ---- refusals ----

    public const string NothingWasSaved = "Nothing was saved.";

    public const string EverythingKept = "Everything you typed is kept.";

    public const string DetailsPrefix = "Details:";

    public const string FormPrefix = "Form:";

    public static string FormSectionPrefix(string section) => $"Form, section {section}:";

    public const string WorkflowPrefix = "Workflow:";

    public static string WorkflowMovePrefix(string move) => $"Workflow, move {move}:";

    /// <summary>S4: "Credit:", never "Credit, rule 1:".</summary>
    public const string CreditPrefix = "Credit:";

    public const string EnterKey = "Enter a key.";

    public const string EnterName = "Enter a name.";

    public const string KeyTooLong = "A key is at most 100 characters.";

    public const string NameTooLong = "A name is at most 200 characters.";

    /// <summary>NEW: a section left with no key, which the page refuses before it sends anything.</summary>
    public const string EnterSectionKey = "Enter a section key.";

    /// <summary>NEW: a field left with no key, which the page refuses before it sends anything.</summary>
    public const string EnterFieldKey = "Enter a field key.";

    public static string DuplicateFieldKey(string key, IReadOnlyList<string> others)
        => $"Key '{key}' is also the key of {Joined(others)}. Give each field its own key.";

    /// <summary>NEW (build-contracts § 3): two sections share a key.</summary>
    public static string DuplicateSectionKey(string key, IReadOnlyList<string> others)
        => $"Key '{key}' is also the key of section {Joined(others)}. Give each section its own key.";

    public const string EmptySection = "This section has no fields. Add a field, or delete the section.";

    /// <summary>NEW (build-contracts § 3): a User field declaring options.</summary>
    public const string UserFieldHasOptions = "A User field names a person, so it offers no options. Clear its options.";

    public static string FieldRuleNotUserField(string move, string field, string? label)
        => label is null
            ? $"Move {move} cannot be read: '{field}' is not a User field, so it names nobody."
            : $"Move {move} cannot be read: '{field}' ({label}) is not a User field, so it names nobody.";

    /// <summary>NEW (build-contracts § 3): a field: rule naming no field of the form.</summary>
    public static string FieldRuleNamesNoField(string move, string field)
        => $"Move {move} cannot be read: '{field}' is not a field of this form, so it names nobody.";

    /// <summary>S4's words under the credit rules (R3-D04).</summary>
    public static string CreditEpaFieldDisagrees(int rule, string named, string expected)
        => string.Create(CultureInfo.InvariantCulture, $"Rule {rule} reads its EPA from '{named}', but this form's EPA field is '{expected}'. Use '{expected}'.");

    /// <summary>R3-0-spec-4 § 10 for the workflow; the credit rules' twin and both without a line are NEW.</summary>
    public static string MalformedJson(bool workflow, long? line)
    {
        var what = workflow ? "The workflow is" : "The credit rules are";
        return line is long number
            ? string.Create(CultureInfo.InvariantCulture, $"{what} not valid JSON. Check the brackets and commas near line {number}.")
            : $"{what} not valid JSON. Check the brackets and commas.";
    }

    // ---- dialogs ----

    public static string DiscardTitle(string name) => $"Discard the draft of {name}?";

    /// <summary>Q14's two bodies (R3-E07, R2-E09).</summary>
    public static string DiscardBody(int publishedVersion) => publishedVersion > 0
        ? string.Create(CultureInfo.InvariantCulture,
            $"Its form, workflow and credit rules go back to version {publishedVersion}, which stays published. Its details (name, description, scope, This tool is, Active) were saved as you changed them, and stay.")
        : "It has never been published. Its form, workflow and credit rules go back to the starting draft; its details stay.";

    public const string DiscardConfirm = "Discard draft";

    public static string DeactivateTitle(string name) => $"Deactivate {name}?";

    public const string DeactivateBody = "Registrars are no longer offered it when filing. Activities already filed are not changed.";

    public const string DeactivateReactivate = "You can reactivate it on this page.";

    public const string DeactivateConfirm = "Deactivate";

    public static string PublishTitle(int version, string name)
        => string.Create(CultureInfo.InvariantCulture, $"Publish version {version} of {name}?");

    public static string PublishBody(int version)
        => string.Create(CultureInfo.InvariantCulture, $"Publishing makes this draft version {version}. It changes:");

    // ---- helpers ----

    /// <summary>The South African day of a UTC moment, ISO (ProgrammeCalendar.DateOf; never ToLocalTime).</summary>
    public static string Day(DateTime utc) => ProgrammeCalendar.DateOf(utc).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>"Audience", "Audience and Topic", "Audience, Topic and Notes".</summary>
    public static string Joined(IReadOnlyList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        _ => $"{string.Join(", ", items.Take(items.Count - 1))} and {items[^1]}"
    };

    private static string JoinedOr(IReadOnlyList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        _ => $"{string.Join(", ", items.Take(items.Count - 1))} or {items[^1]}"
    };
}
