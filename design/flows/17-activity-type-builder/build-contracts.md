# T398 build contracts (flow 17)

The names two or more lanes share, and the ones each lane provides. Use them exactly; a lane that must differ says so in
its report. Each name says whether it exists (a path, at master before § 0) or is new (on `t398`, written by § 0, or by
the lane named). Lanes A, B and C run side by side in one wave (`build-lanes.md`); the runbook lane W follows the merge
and the sweep.

Flow 17 is two pages over one aggregate, so its lanes are cut by layer, not by page family: A everything below Web, B
the list and the read in words, C the builder. What would otherwise cross (the DTOs both pages read, the rule both
queries judge by, the refusal's shape, the read-out components' parameters, the words both pages print) is § 0's.

## § 0: on `t398` before any lane is cut, written by one Opus agent

- **The boards' text**: `design/flows/17-activity-type-builder/round-3/boards-text.md` (87 boards: Spec 1–4, Steps and
  the two check boards, then A, B, C, D, E, each at 1280 then its other widths) and `round-2/boards-text.md` (145
  boards; the R2 boards that stand are listed in `build-lanes.md` § Read first, and `R2-0-spec-2`, `R2-0-spec-3` and
  `R2-0-steps` stand where round 3 points back). One heading per board (`######## BOARD file`), the label strip's lines
  first; style, script, svg, head stripped; aria-labels, placeholders, values, `[checked]`, `[disabled]`,
  `[aria-disabled]`, `[selected]`, `[aria-selected]`, `[aria-haspopup]`, `[aria-current]`, `[aria-required]`, `role` and
  alt text kept; a table row one `| a | b |` line, a `dt` opened `**`. Design data, read by every lane. (Flow 16's were
  made by a stdlib script in the integrator's scratchpad; § 0 writes its own the same way, run with `python -I`.)
- **No migration.** Nothing flow 17 needs is stored that is not stored today: the owner's words are the College's short
  code, the institution's name or the platform, read through the scope's target; a release is the type's
  `SystemManaged` and its key; names are read by `IUserAdministrationService.GetDisplayNamesAsync`; seed ownership is the
  type's `OwnerUserId` and its newest version's publisher; the read in words, the preview's EPAs and the laid-out JSON
  are computed; T334 is a rule, not a column. **No lane adds a migration**, and no lane changes what `SaveDraft` stores;
  a lane that finds it needs one stops and says so. (Flow 16 forbade migrations for the same reason: nothing new is
  stored. Nothing being live would allow one, but none is wanted: a column for any of these would be a second copy of
  something already read.)
- **T334 and Q19: the rule** (`src/Wombat.Application/Features/Activities/ActivityTypeAdminScope.cs`; Q9, Q19,
  R3-0-spec-2 § 3's ActivityTypeAdminScope row):
  - `MayWrite(ClaimsPrincipal principal, ActivityScope scope, int? scopeId, int? owningCollegeId, bool systemManaged)`:
    false for a system-managed type, to everyone, the Administrator included; else as built.
    `MayWriteAsync(db, principal, scope, scopeId, systemManaged, ct)` likewise. A new type (no row) passes `false`.
  - NEW `MayRead(...)` / `MayReadAsync(...)`: today's read rule (`GetActivityTypeEditorQuery.CanReadAsync`, `:174–176`,
    and the list's filter, `ListActivityTypesAdminQuery.cs:84`) on the **scope rule alone, ignoring `SystemManaged`**,
    so an inactive system-managed type stays readable to whoever reads it today (Q19). `CanReadAsync` and the list's
    filter call it.
  - `ActivityTypeScopeGuard.EnsureCallerCanWriteAsync` takes the type where there is one (the three commands' existing
    row) and throws, before the scope check, `UnauthorizedAccessException("Nothing was saved. Multi-Source Feedback
    (Paediatrics) is written by the MSF release, so it cannot be changed here.")` (N10, NEW) for a system-managed type;
    the scope refusals' words unchanged.
  - Every caller updated: the list query's per-row `MayWrite` (with the row's `SystemManaged`), `ForCallerAsync`, the
    three commands' guards.
  - NEW `src/Wombat.Application/Features/Activities/SystemManagedReleases.cs`: `WrittenBy(string key)`: `msf_cpsa` →
    "the MSF release", `learner_feedback_cpsa` → "the learner-feedback release" (`ActivityTypeSeedCatalogue.cs:173`,
    `:180`), null for any other key.
  - `ScopeTarget` gains `OwningCollegeShortCode` (read in the same three queries of `ScopeTargetsAsync`), and NEW
    `ActivityTypeAdminScope.OwnerOf(ActivityScope scope, ScopeTarget target) → ActivityTypeOwnerDto` (below), so the
    list and the editor name an owner alike.
  - Tests: `SystemManagedActivityTypeTests.SavingAndPublishingASystemManagedTypeInTheBuilder_KeepsItSystemManaged`
    (`:78–110`) inverted deliberately (the save, the publish and the discard refused for an Administrator and a College
    admin, nothing staged); NEW cases in `ActivityTypeScopeGuardTests` (the N10 sentence; an inactive `msf_cpsa` still
    read by a College admin and an institution's admin, Q19); `ActivityTypeAdminScopePostgresTests` green.
- **The DTOs' new members** (`src/Wombat.Application/Features/Activities/Dtos/ActivityDtos.cs`), each an **`init`
  member with a default**, never a new positional parameter, so no existing constructor call changes:
  - NEW `enum ActivityTypeOwnerKind { Platform, College, Institution }` and NEW `record ActivityTypeOwnerDto(
    ActivityTypeOwnerKind Kind, string? Name)`: Platform for Global (Name null), College for Speciality and
    Sub-speciality (Name the owning College's short code: "CPSA", "DEMO-C"), Institution (Name the institution's name).
    Null Name for a target that no longer exists.
  - `ActivityTypeAdminListItemDto`: `Owner` (`ActivityTypeOwnerDto?`), `SystemManaged` (bool), `WrittenBy` (string?).
    Filled by lane B.
  - `ActivityTypeAdminListDto`: `CallerCollegeName` (string?), `CallerCollegeHasSpeciality` (bool, default true): N12,
    a College admin whose College holds no speciality. Filled by lane B.
  - `ActivityTypeVersionDto`: `PublishedByName` (string?): the publisher's display name, "Wombat's catalogue" for the
    seeder. Filled by lane A.
  - `ActivityTypeEditorDto`: `Owner`, `SystemManaged`, `WrittenBy`, `DraftSavedByName` (string?), `IsSeedOwned` (bool),
    `PreviewEpaOptions` (`IReadOnlyList<ActivityCatalogueOption>?`, null: the form loads its own), `WorkflowRead`
    (`ActivityTypeWorkflowReadDto?`), `CreditRead` (`ActivityTypeCreditReadDto?`). Filled by lane A on the builder's path
    only (`ForBuilder: true`); `CanWrite` false on a system-managed type by the rule above (§ 0).
  - NEW the read in words (Q2; computed by lane A, rendered by lane B):
    `ActivityTypeWorkflowReadDto(IReadOnlyList<ActivityTypeStateReadDto> States, IReadOnlyList<ActivityTypeMoveReadDto> Moves)`;
    `ActivityTypeStateReadDto(string Key, string Label, bool IsInitial, bool IsTerminal)`;
    `ActivityTypeMoveReadDto(string Key, string Label, IReadOnlyList<string> FromLabels, string ToLabel, string Who,
    string Checks, bool RequiresNote)` with `init` `WhoAsWritten` (string?: the actor rule as written, set when
    `ActorRuleWords` could not word it, which the page shows in `<code>`);
    `ActivityTypeCreditReadDto(bool CreditsNothing, IReadOnlyList<string> Sentences)`.
- **The refusal's shape** (NEW, `src/Wombat.Domain/Activities/RefusalPlace.cs`; Q6, J2, S2): Domain, no framework.
  - `enum RefusalTab { Details, Form, Workflow, Credit }`.
  - `enum RefusalKind { Other, MalformedJson, DuplicateSectionKey, DuplicateFieldKey, EmptySection, FieldRuleNamesNoField,
    FieldRuleNotUserField, UserFieldHasOptions, CreditEpaFieldDisagrees }` (lane A may add kinds; each added one is
    named in its report, and a kind lane C does not word falls back to the summary line alone).
  - `sealed record RefusalPlace(RefusalTab Tab, RefusalKind Kind, string Sentence)` with `init` members `SectionKey`
    (string?), `FieldKeys` (`IReadOnlyList<string>`, default empty), `MoveKey` (string?), `DirectiveNumber` (int?, from
    1), `NamedField` (string?: the field a rule names), `ExpectedField` (string?: the field it should name), `JsonLine`
    (long?, 1-based). `Sentence` is the built sentence the summary shows after the page's prefix, word for word.
  - `interface IActivityTypeRefusal { IReadOnlyList<RefusalPlace> Places { get; } }`, implemented by lane A on
    `SchemaParseException`, `WorkflowParseException` and `CreditRulesParseException` (their types, constructors and
    `Message` unchanged: a new constructor or `init` member carries the places; a parse exception thrown with no place
    carries one `RefusalPlace` of its tab, `Kind = Other`, `Sentence = Message`).
  - NEW `sealed class ActivityTypeDraftRefusedException : InvalidOperationException, IActivityTypeRefusal` (Places;
    `Message` the places' sentences joined by a space), thrown by `SaveDraft`/`PublishDraft` **only if P2 is "every tab
    at once"**; § 0 writes the type either way (unused otherwise).
  - Tests: `Domain.Tests/Activities/RefusalPlaceTests` (the record's defaults; the exceptions' fallback place) by § 0;
    lane A extends.
- **Entrustment scales by seed key** (S12, T271 item 2): `IActivityReferenceDataService.GetEntrustmentScaleOptionsAsync`
  returns NEW `record EntrustmentScaleOption(string Value, string? SeedKey, string Name)` (Value the scale's id, as the
  stored `scale_key` reads it today; SeedKey its seed key, so a `seed:…` binding resolves to its name) in place of
  `ActivityCatalogueOption`; `ActivityReferenceDataService.cs:439–447` reads `SeedKey`;
  `StubActivityReferenceDataService` (Web.Tests) and the builder's one call (`ActivityTypeEdit.razor:808`) adapted, the
  page's behaviour unchanged until lane C uses SeedKey.
- **The catalogue's addresses, flow 17's** (`src/Wombat.Web/Components/Shared/CatalogueLinks.cs`): `ActivityTypes`
  (`/admin/activity-types`), `NewActivityType` (`/admin/activity-types/new`), `ActivityType(id)`
  (`/admin/activity-types/23`), `ActivityTypeCreated(id)` (`/admin/activity-types/23?created`, the existing
  `CreatedToken`; `Tokens`, `CatalogueLinks.cs:55`, unchanged). Tests: `Web.Tests/Admin/CatalogueLinksTests` (the routes
  theory covers both pages' routes).
- **The words both pages print** (NEW, `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypeWords.cs`):
  `SetBy(owner)` ("CPSA", "Kgosi Kgari Teaching Hospital", "The platform"; the owner line's and the list cell's first
  line, R2-A01, R3-C07); `ScopeDetail(scope, targetName)` ("Speciality · Paediatrics", "Institution", today's
  `FormatScope`, moved here); `ReadOnlyNotice(scope, targetName)` (T300's three, word for word, moved from
  `ActivityTypeEdit.razor:656–672`); `WrittenByLine(writtenBy)` ("Written by the MSF release.", the list's row line,
  R2-A01) and `ReleaseNotice(writtenBy)` ("Written by the MSF release. Its form changes only with a release of Wombat.
  Nobody is offered it when filing an activity.", R2-B06, T22); `Unavailable` ("Activity type unavailable" / "This
  activity type does not exist, or you cannot open it." / "Go to Activity types"); `Published(version)` ("Version 1" /
  "Not published"); `Draft(savedOnUtc)` ("Saved 2026-11-05" by `ProgrammeCalendar.DateOf` / "None"). Tests:
  `Web.Tests/Admin/ActivityTypeWordsTests`.
- **The read-out components' parameters** (NEW stubs in `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/`, each
  rendering its root element and nothing else; lane B fills the bodies, lane C places them):
  - `WorkflowReadout.razor`: `[Parameter, EditorRequired] ActivityTypeWorkflowReadDto Read`, `[Parameter] int
    HeadingLevel = 3`; root `div.workflow-readout`.
  - `CreditReadout.razor`: `[Parameter, EditorRequired] ActivityTypeCreditReadDto Read`, `[Parameter] int HeadingLevel
    = 3`; root `div.credit-readout`.
  - `VersionsTable.razor`: `[Parameter, EditorRequired] IReadOnlyList<ActivityTypeVersionDto> Versions`, `[Parameter]
    int HeadingLevel = 3`; root `div.versions-readout`.
  - The three classes are added to `app.css` under lane B's heading by § 0 as empty-bodied comments only if
    `DefinedClassTests` needs them defined; otherwise lane B defines what it uses.
- **`app.css`**: (1) **C3**: `.activity-types-page` joins all **seven** selector lists of "The catalogue tables stack from
  1024px down" (`app.css:7539–7604`: thead; tbody tr; tbody tr:last-child; td and tbody th, two selectors; th[scope="row"];
  td[data-label]::before; .btn), and the block's comment names the list; nothing else in the block changes. (2) Under
  `/* ── The catalogue ── */`'s end, two headings with their `/* end … */` lines and nothing between: "Flow 17: the list
  and the read (T398). Lane B." and "Flow 17: the builder (T398). Lane C." DESIGN.md § app.css section order has the C3
  line and one placeholder line per lane; § Table system's 1024px paragraph (`DESIGN.md:1133`) names the list.
  `NarrowLayoutTests` pins C3.
- **The baseline counts**: § 0 runs the six suites at master first and on `t398` after its commit, and writes both in
  `build-lanes.md` § "§ 0's counts".

## § 1: lane A (the commands, the reads and the DSLs) provides

- The three parse exceptions as `IActivityTypeRefusal`, each throw site with its place; `ActorFieldRules.EnsurePublishable`
  and `EvidenceEpa.EnsureCreditAgrees` one place per problem; under P2 "every tab", `ActivityTypeDraftRefusedException`
  from `SaveDraft` and `PublishDraft`. The kinds lane C words at the place: `DuplicateFieldKey` (`FieldKeys` every key
  holder's row, `SectionKey`), `FieldRuleNotUserField` and `FieldRuleNamesNoField` (`MoveKey`, `NamedField`),
  `EmptySection` (`SectionKey`), `CreditEpaFieldDisagrees` (`DirectiveNumber`, `NamedField`, `ExpectedField`),
  `MalformedJson` (`JsonLine`).
- `WorkflowParser.Serialize(Workflow, bool indented)`, `CreditRulesParser.Serialize(CreditRules, bool indented)`,
  `FormSchemaParser.Serialize(FormSchema, bool indented)`; the one-argument overloads unchanged.
- `ActorRuleWords.Describe(ActorRule rule, Func<string, string?> fieldLabel) → (string Words, bool Worded)` and
  `ActorRuleWords.Validation(TransitionValidation v)` (its own names if the grammar asks; named in the report).
- `ActivityTypeSeedOwnership.SeederUserId`, `ActivityTypeSeedOwnership.IsSeedOwned(ActivityType)`.
- `GetActivityTypeEditorQuery` → `ActivityTypeEditorDto?` (null for unknown or unreadable), § 0's editor members filled.
- `SaveActivityTypeDraftCommandValidator` (Key, Name); `FieldRefusalException("Key", …)` for the key taken.
- Read by lane C through the DTOs and the exceptions only; lane C's tests build both by hand.

## § 2: lane B (the list and the read in words) provides

- `ListActivityTypesAdminQuery`'s § 0 members filled (`Owner`, `SystemManaged`, `WrittenBy`; `CallerCollegeName`,
  `CallerCollegeHasSpeciality`).
- The bodies of `WorkflowReadout`, `CreditReadout` and `VersionsTable` (captions "Moves, as saved", "Published
  versions"; the R2-B04, B05, B03/E03 markup), on § 0's parameters. Read by lane C by placing them, never by their words.

## § 3: lane C (the builder) provides

- The page and any component it is split into; `GetPublishWarnings` by label; `RefusalSummary`'s optional line action;
  `ActivityForm`'s optional `EpaOptions`. Read by no other lane.
- **The words at the place** (`R3-0-spec-4` § 9, the boards'), by kind: `DuplicateFieldKey` "Key 'topic' is also the
  key of Audience. Give each field its own key." (at each holder, naming the others by label); `FieldRuleNotUserField`
  "Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody."; `EmptySection` "This section
  has no fields. Add a field, or delete the section."; `CreditEpaFieldDisagrees` "Rule 1 reads its EPA from 'epa', but
  this form's EPA field is 'epa_id'. Use 'epa_id'."; `MalformedJson` `build-lanes.md` § "Settled here"'s; the key taken
  "An activity type with key 'kgk_teaching_log' already exists." under Key. **NEW**, lane C's to word and list in its
  report: `FieldRuleNamesNoField`, `DuplicateSectionKey`, `UserFieldHasOptions` at their place (the summary line keeps
  the built sentence).
- The summary's prefixes: "Details:", "Form:", "Form, section ‹section title›:", "Workflow, move ‹move label›:",
  "Workflow:", "Credit:" (S4: never "Credit, rule 1:").

## § 4: words and constants already built and kept (lanes use them, do not reword)

- T300's read-only notice, word for word ("Set by the College that owns Paediatrics. You can read this activity type
  here, but not change it.", "Set by Kgosi Kgari Teaching Hospital. …", "Set by a global administrator. …"; moved into
  `ActivityTypeWords` by § 0, not reworded); the no-scope notice ("You have no scope to create an activity type in. …",
  `ActivityTypeEdit.razor:67–70`); "Draft saved.", "Published version 1.", "Draft discarded."; "This tool is" and its
  options; the Options help ("One option per line; a comma is part of the option. …"); "Who this field may name:
  someone at the trainee's institution holding this role."; the EPA field's help; the three "None — …" options; the
  parsers' and the cross-checks' sentences (the summary's lines, after the prefix), but S4's credit sentence.
- "Enter a name." and "A name is at most 200 characters." (`CollegeHolders.cs:31`, `:33`'s words); `RequiredMarkLine`
  ("* marks a field you must fill in."); `SystemPageText` (flow 01's); `EpaOptionLabel` ("PAED-001 — Title");
  `RowNames.Distinct`; `ProgrammeCalendar.DateOf`; `WombatRoleLabels.For`.
- Components: `PageHeader` (`Trail`, `CurrentCrumb`), `DataTable` (`StackWide`, `Caption`, `Class`), `StatePanel`
  (`LoadError`, `OnRetry`, `FocusAfterRetry`, `LoadingContent`), `ActionResult` (`FocusOnLoad`, `FocusAfterRender`),
  `ConfirmDialog` (action mode, opening on Cancel, `BodyContent`, `DangerAction`, `ReturnFocusId`), `FormField`
  (`Required`), `FieldHelp`, `FieldRefusal` and `FieldRefusalException` (flow 15's), `RefusalSummary` (flow 03's),
  `FormActions`, `Alert` (`Role=""` for a standing notice), `Icon`, `InFlight`, `PageFocus.FocusByIdAsync`,
  `.people-page`, `.user-page`, `.person-cell`, `.choice-group`, `.detail-list`, `.badge.badge-declined`,
  `.list-unstyled`, `.actions-cell`, `.visually-hidden`.

## Across lanes

- **Refusals by type, failures in fixed words** (`build-lanes.md` § How): a page maps `IActivityTypeRefusal`,
  `FieldRefusal.Of(ex)` and `UnauthorizedAccessException` to their words and places; anything else is the fixed sentence,
  logged; never `ex.Message` of an unknown exception and never `RefusalText.Of` on these pages (P1).
- **Out of scope is Unavailable**: an id the caller cannot read, or that does not exist, is the builder's own
  Unavailable (`ActivityTypeWords.Unavailable`), its menu item lit, no id shown, never forbidden.
- **Whose is what, in words**: `ActivityTypeWords.SetBy`, the College by its short code; a system-managed type by its
  release; never an enum.
- **Titles**: `build-lanes.md` § How.
- **Times and days**: ISO days (`ProgrammeCalendar.DateOf`); never `DateTime.Today` or `ToLocalTime()`.
- **No new role check and no new route.** § 0's T334/Q19 is the one rule change.

## § 5: words on no board, marked NEW (the fix pass)

Filled by the fix pass (2026-11-10) from the pages review's 31 groups (`build-review-pages.md` § "Words on no board"),
checked against the words files at `317778f6`, and the one the fix pass adds (32). Words a board draws, or that were built
before T398 (T300's "Set by the College." and "Set by its institution.", the key taken's built sentence), are not here.

| # | Words | Where | NEW because |
|---|---|---|---|
| 1 | "Counts ‹n› towards curriculum item ‹id›" | lane A, `ActivityTypeReadout` (Credit's read) | R2-D02/D03 word only the curriculum-item-of-the-EPA case |
| 2 | "…towards the curriculum item in ‹field label›" | lane A, the same | as 1 |
| 3 | "…towards no curriculum item" | lane A, the same | as 1 |
| 4 | ", at level ‹fixed›" | lane A, the same | as 1 |
| 5 | "Use one of: Institutional admin, Coordinator, Committee member, Assessor, Trainee." (the nominee role's refusal by label; the labels are now accepted too, S-1) | lane A, `FormSchemaParser` | N1 named the roles by label; no board words the list |
| 6 | "a release of Wombat" (in "Written by a release of Wombat.", a system-managed type no release is named for) | § 0 / lane A, the fallback | no board draws an unnamed release |
| 7 | "1 activity type." | lane B, `ActivityTypesListWords.CountLine` | the singular of R2-A01's count |
| 8 | "‹n› of 1 activity type matches “‹q›”." | lane B, the same | the singular of R2-A02's |
| 9 | "The search is kept above. Clear it to see the one activity type." | lane B, `NoMatchBody` | the singular of R2-A04's |
| 10 | A College admin whose account names no College: the subtitle's lead sentence alone | lane B, the subtitle | no board draws a College admin with no College |
| 11 | "A, B or C" for three or more From states ("Draft, Requested or Returned") | lane B, `ActivityTypeReadWords` | the boards draw at most two |
| 12 | "Its College" (a College owner whose target no longer exists) | § 0, `ActivityTypeWords.SetBy` | no board draws a missing target; the builder no longer reaches it for a scope chosen on the page (N2) |
| 13 | "Its institution" (likewise) | § 0, the same | as 12 |
| 14 | "Enter a section key." | lane C, the page's own check | no board draws a blank section key |
| 15 | "Enter a field key." | lane C, the same | no board draws a blank field key |
| 16 | "The credit rules are not valid JSON. Check the brackets and commas near line ‹n›." | lane C, `ActivityTypeEditWords` | R3-0-spec-4 § 10 words the workflow's alone |
| 17 | "The workflow is not valid JSON. Check the brackets and commas." (no line known) | lane C, the same | as 16 |
| 18 | "The credit rules are not valid JSON. Check the brackets and commas." (no line known) | lane C, the same | as 16 |
| 19 | "Key '‹key›' is also the key of section ‹title›. Give each section its own key." (`DuplicateSectionKey` at its place) | lane C, `BuilderRefusal` | no board draws it at its place |
| 20 | "A User field names a person, so it offers no options. Clear its options." (`UserFieldHasOptions` at its place) | lane C, the same | as 19 |
| 21 | "Move ‹move› cannot be read: '‹field›' is not a field of this form, so it names nobody." (`FieldRuleNamesNoField` at its place) | lane C, the same | as 19 |
| 22 | "Version ‹n› could not be published. Nothing has changed. Try again, or come back in a few minutes." | lane C, the failure | words only, no board (N5) |
| 23 | "The draft could not be discarded. Nothing has changed. Try again, or come back in a few minutes." | lane C, the failure | as 22 |
| 24 | "Draft saved. Nothing was published." (Cancel on the warnings' question after a save) | lane C, Publish's results | Q16's outcome, no board |
| 25 | "Draft saved. ‹name› is inactive: registrars are no longer offered it. Nothing was published." | lane C, Q17's results | Q17, words only |
| 26 | "Published version ‹n›. ‹name› is inactive: registrars are no longer offered it." | lane C, the same | as 25 |
| 27 | "From ‹initial› to ‹others›; ‹n› moves" (and "‹initial›; ‹n› moves"; "1 move") | lane C, "What you are publishing" (a first publish, R3-E01) | the board draws it with "→", which never ships |
| 28 | "‹n› credit rule(s)" ("1 credit rule", "2 credit rules") | lane C, the same | R3-E01 draws only "Credits nothing" |
| 29 | "Version ‹n›, ‹day›" and "Saved ‹day›" with no publisher or saver named (a type no name is read for; and, since the fix pass, every reader outside the type's scope, S-2) | lane C, the strip | the boards always name the person |
| 30 | "Close section ‹title›" (a reader's section open in place) | lane C, the outline | R2 draws "Close field ‹label›" only |
| 31 | "Activities already filed stay on version ‹n›. Only activities filed after the publish use version ‹n+1›." (the pinned line with no field removed) | lane C, `PinnedLine` | the boards draw it with a removed field's clause |
| 32 | "A description is at most 2,000 characters." (under Description) | the fix pass, `ActivityTypeFieldMessages` (code review N-1) | the page's counter stops at 2,000, so no board draws it; the twin of S10's Key and Name words |
