# T392 build contracts (flow 15)

The names two or more lanes share, and the ones each lane provides. Use them exactly; a lane that must differ says so in its
report. Each name says whether it exists (a path, at master `9d4b2a2f`) or is new (on `t392`, written by § 0, or by the
lane named). Lanes A, B, C and D run side by side in one wave (`build-lanes.md`); the runbook lane W follows the merge.

Flow 15's lanes are cut by page group, each owning its pages **and** the Application code behind them, so little crosses:
§ 0 holds the shapes two lanes would otherwise both write, and every other name is one lane's.

## § 0: on `t392` before any lane is cut, written by the planner (`3aa6e6e8`)

- **The boards' text**: `design/flows/15-institution-adopts-catalogue/round-3/boards-text.md` (112 boards: the three Spec and
  two Steps boards, then A, B, C, D, E, F, each at 1280 then its other widths) and `round-2/boards-text.md` (168 boards; the
  R2 boards that stand, R3-0-spec-2 § 5, and R2-0-spec-2's § 7 DATA, § 8 messages, § 9 re-pointing, § 10 T1–T12). One heading
  per board (`######## BOARD file`), the label strip's lines first; style, script, svg, head stripped; aria-labels,
  placeholders, values, `[checked]`, `[disabled]`, `[aria-disabled]`, `[aria-haspopup]`, `[aria-current]` and alt text
  kept; a table row one `| a | b |` line, a `dt` opened `**`. `round-3/` has no `canvas.json`, so the order is by id. Design
  data, read by every lane.
- **No migration.** Nothing flow 15 needs is stored that is not stored today: the state of a version for an institution, the
  registrar counts, the relation newer/older, the owner and "may change" of an EPA, and the adoption's South African day are
  read or computed. **No lane adds a migration**; a lane that finds it needs one stops and says so.
- **The typed field refusal** (NEW, `src/Wombat.Application/Common/FieldRefusalException.cs`):
  `FieldRefusalException(string field, string message, Exception? inner = null) : InvalidOperationException`, with
  `string Field`: the refused command's property name (`nameof(CreateEpaCommand.Code)`), the sentence the page shows. Thrown
  where the commands throw today, before anything is staged, or wrapping the database's refusal (`inner`), so the audit
  pipeline still discards a refused save (T201). Its keys, each thrown by the lane named:
  | Command | Field | Lane |
  |---|---|---|
  | `CreateEpaCommand`, `UpdateEpaCommand` | `"SubSpecialityId"` (F9: an unadopted discipline), `"Code"` (T272: a real duplicate, on the unique index only) | C |
  | `AddCurriculumItemCommand`, `UpdateCurriculumItemCommand` | `"EpaId"` (the EPA another item holds, T223's three sentences; an EPA not offered) | B |
  | `UpdateTraineeProfileCommand` | `"CurriculumId"` (`MoveNotAdopted`, D06) | D |
- **The page's half** (NEW, `src/Wombat.Web/Components/Shared/FieldRefusal.cs`): `sealed record FieldRefusal(string Field,
  string Message)`; `static FieldRefusal? Of(Exception)` (a `FieldRefusalException`, or a `ValidationException` whose
  failures name one property, their messages once each; null for anything else, which the page answers with its own fixed
  failure sentence, never the text, T329); `bool Is(string field)`; `static string Id(string inputId)` (`"‹input›-refusal"`);
  `static string? DescribedBy(string inputId, bool refused, string? helpText, params string?[] regionIds)` (the refusal
  first, then the help, then a counter: flow 14's N17). The refusal renders as `div.validation-message role="alert"` with
  that id, the control `aria-invalid="true"` and focused, what was typed kept. Tests: `Admin/FieldRefusalTests`.
- **The catalogue's addresses** (NEW, `src/Wombat.Web/Components/Shared/CatalogueLinks.cs`): `Curricula`, `Epas`, `NewEpa`,
  `Epa(id)`, `EpaCreated(id)` (`/admin/epas/17?created`), `Items(curriculumId)`, `NewItem(curriculumId)`
  (`/admin/curricula/4/items/new`), `Item(curriculumId, itemId)` (`…/items/31`), `ItemAdded`, `ItemSaved`, `BackFromItem`
  (`…/items?added=31`, `?saved=31`, `?back=31`); the token names `CreatedToken`, `AddedToken`, `SavedToken`, `BackToken`;
  `HasToken(uri, name)` (reads `?created`, which has no value), `TokenId(uri, name)` (a non-negative int or null), and
  `WithoutTokens(uri)` (drops the four, keeps any other query part and the fragment). F1's rule, written on the class: the
  arriving page re-reads its record in scope, writes the sentence itself, focuses it (for `back`, the row's Edit), then
  `NavigateTo(WithoutTokens(uri), replace: true)`; a stale or foreign id writes and focuses nothing. Tests:
  `Admin/CatalogueLinksTests` (lane B adds `NewItem` and `Item` to its route theory once its page exists).
- **The curriculum's state for an institution** (`src/Wombat.Application/Features/Curricula/CurriculumDto.cs`): two `init`
  members on `CurriculumDto`, both **filled by lane A** in `GetCurriculaListQuery` and `GetCurriculumByIdQuery` (null/false
  from every command's returned curriculum, and until lane A merges):
  - `CurriculumInstitutionStateDto? Institution`: the caller's own institution for an Institutional admin; the institution
    chosen with Show for the Administrator on Curricula; null for the College admin and on the Administrator's All curricula
    and items page. `CurriculumInstitutionStateDto(int InstitutionId, string InstitutionName, bool IsActiveAdoption,
    DateOnly AdoptedOn, DateOnly? SupersededOn, string? SupersededByName, string? SupersededByVersion, int RegistrarCount,
    int NationalItemCount, int OwnItemCount, IReadOnlyList<OwnItemsOnVersionDto> OwnItemsElsewhere)`;
    `OwnItemsOnVersionDto(int CurriculumId, string Version, int Count)`. The rules (S1, Q10, F6) are on the record's doc
    comment: SupersededOn the AdoptedOn of the adoption that followed the version's latest one, by (AdoptedOn, Id);
    RegistrarCount the institution's **active** profiles with that `CurriculumId` (never `AdoptionId`).
  - `bool MayAddItem`: `CanEditCurriculum`, or an Institutional admin whose institution's active adoption is this version.
    False on a superseded version (F6). Lane B's items page offers an add only where it is true.
- **The items' order** (round-3-check decision 3; `CurriculumRow.Projection` in `GetCurricula.cs` and
  `CurriculumMappings.ToDtoAsync`): the national items in EPA order, then each institution's own, by institution and then
  EPA, on every read and every command's returned curriculum. KGK-001 follows PAED-015 (and PAED-016). Test:
  `CurriculumAdminScopeTests.TheItems_AreTheNationalOnesInEpaOrder_ThenEachInstitutionsOwn`.
- **A new version starts held back** (F5, T301's second half): `CreateCurriculumCommandHandler` stores `IsActive = false`;
  `Curriculum.CloneAsNewVersion` returns `IsActive = false` (the source keeps its flag). `DataSeeder` and
  `PaediatricCatalogueSeeder` already set their curricula open explicitly (commented). `CurriculumEdit.razor` (flow 16's)
  shows no Active box on create and reads the stored flag after it, so its page needs no change. Test:
  `CurriculumStartsHeldBackTests`.
- **The menu and the links into adoption** (Q1, S4):
  - `NavItems.CurriculumAdoptions` is gone; the Institutional admin's Curriculum group is Curricula, EPAs, Activity types,
    Entrustment scales (14 links counted). `NavOwners`: `AdoptionsList` → Curricula for the Administrator and the
    Institutional admin (the address sends there once lane A makes it a redirect; until then it is today's page, lit
    under Curricula).
  - `OnboardingLinks.CurriculumAdoptions` is renamed `OnboardingLinks.Curricula` (`= CatalogueLinks.Curricula`) and its three
    callers re-pointed with R2-0-spec-2 § 9's words: Invite a person "… Adopt a curriculum under Curricula." (link
    "Curricula"); Admit "… Adopt one under Curricula, then admit the trainee." (`AdmissionText.NothingAdoptedAtAllLink =
    "Curricula"`); Home's Curriculum card "Open Curricula" (`InstitutionalAdminHomeWords.OpenAdoptions`).
  - DESIGN.md: the NavMenu table's Institutional admin row and its count (14), the owner table's `/admin/adoptions` row,
    the two other mentions (Invite's link, the Home's ways on). `NavMenuAuthorizationTests` holds the table.
- **`app.css`, `/* ── The catalogue ── */`** at the file's end: one comment saying nothing is shared, then the four lanes'
  headings with their `/* end … */` lines and nothing between: "Flow 15: curricula (T392)" (A), "Flow 15: a curriculum's
  items (T392)" (B), "Flow 15: EPAs (T392)" (C), "Flow 15: the move, the rebuild and the scales (T392)" (D). DESIGN.md
  § app.css section order has the heading, one placeholder line per lane, and the sentence under the block.
- Suites on `t392` at § 0: see `build-lanes.md` § "§ 0's counts".

## § 1: lane A (curricula and adoption) provides

- `GetCurriculaListQuery(ClaimsPrincipal Principal, int? InstitutionId = null)` (S3): null is the Institutional admin's
  own; required for the Administrator's "An institution's curricula" section (a scoped caller's other id reads as her own
  or nothing, never another institution's). Its rows fill `Institution`. The Administrator's All curricula is the same
  query with no institution (`Institution` null on every row).
- `GetCurriculumByIdQuery` fills `Institution` (for the Institutional admin) and `MayAddItem` (everyone): lane B reads both.
- `CurriculumAdminScope.Openable`'s second arm (Q9): a superseded version opens to the institution while any of its
  profiles, **active or ended**, is pinned to it, or it holds an item of the institution's own; with neither it leaves her
  list. The list and the read stay one rule (T211).
- `GetAdoptableCurriculaQuery(ClaimsPrincipal Principal, int? InstitutionId = null)`: open versions the institution has not
  **actively** adopted (Q7: an older one stays, under the dialog, Q21), each with `EffectiveFrom`, `NationalItemCount`, its
  relation to the active adoption of its discipline, `Newer`/`Older`/`None` by (`EffectiveFrom`, `Id`) (F2), the version it
  is measured against, and for the dialog the version it would supersede, that version's active-registrar count and the
  institution's own items on it the other does not hold (by code). Names: `AdoptableCurriculumDto` + `init` members, and
  `AdoptionRelation`, lane A's to name.
- `AdoptCurriculumCommand` (unchanged signature): refuses a held-back version **before anything is written** (Q5, T301:
  "This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted."); keeps the
  two built refusals for a stale page; going back accepted; `AdoptedOn` the South African day of the handler's
  `TimeProvider` (T325; a test pins 22:30 UTC); its result says what it superseded and how many registrars stay
  (`AdoptionDto` + `init` members). The refusals are typed (lane A's `AdoptionRefusedException` and reason, or its own
  names); the page maps the reason, never the message.
- `/admin/adoptions` (`AdoptionsList.razor`) becomes a redirect to `CatalogueLinks.Curricula` (replace), keeping its
  `AdministratorOrInstitutionalAdmin` policy. `ListAdoptionsQuery` goes if nothing else sends it.

## § 2: lane B (a curriculum's items) provides

- NEW page `W/Admin/Curricula/CurriculumItemPage.razor` at `/admin/curricula/{Id:int}/items/new` and
  `/admin/curricula/{Id:int}/items/{ItemId:int}` (`NationalCatalogueAccess`), its `NavOwners` row (Curricula, the catalogue
  authors), for every caller (F3). Nothing another lane reads.
- The item commands' `FieldRefusalException("EpaId", …)` (§ 0's table).

## § 3: lane C (EPAs) provides

- `EpaDto` + `init` members (lane C's to name): the owner (`OwningInstitutionId`, its name), "may change" from
  `UpdateEpa`'s rule (never roles, S4), the discipline's words; the list's Create EPA flag (true for the Administrator and a
  College admin, for an Institutional admin only with an actively adopted discipline). `GetEpaByIdQuery` null outside the
  list's rule (Q2, Q19). Read by no other lane: `ListEpasForSubSpecialityQuery`'s other callers keep compiling and their
  results unchanged but for the narrower by-id read.
- `CreateEpaCommand`, `UpdateEpaCommand`: § 0's two keys.

## § 4: lane D (the move, the rebuild and the scales) provides

- `CurriculumChoiceDto` + `init` members for each option's state (the active adoption, or superseded on ‹date›, DATA);
  `UpdateTraineeProfileCommand`'s `FieldRefusalException("CurriculumId", MoveNotAdopted)`; the profile's own no-curriculum
  sentence (`TraineeProfileEdit.razor`'s `NothingAdopted`) re-pointed to Curricula with its link, as § 0 did Admit's.
  Nothing another lane reads.

## § 5: words and constants already built and kept (lanes use them, do not reword)

- The items page's standing notice for an institution (`CurriculumItemsEdit.razor:44–49`) and the College's notice; the
  paused notice and "(inactive: not in force)"; the built national empty line (`:1139–1140`) and the institution's empty
  line; "Curriculum item added.", "Curriculum item saved.", the Remove dialog's body (its hyphen an em dash, S10); T223's
  three refusals (`CurriculumAdminScope.AlreadyContains`, `NationalBesideLocal`, `LocalBesideNational`) and the two
  owner-adoption refusals.
- "EPA saved.", "EPA deactivated.", "EPA saved and deactivated.", the two "EPA reactivated." results, the Active help's
  closing sentences, "An EPA with the same code already exists for this sub-speciality." (a real duplicate only, T272).
- T304's move results ("Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it.", the
  no-completions and the start-change ones; the first sentence bold), `MoveNotAdopted`, "Trainee profile saved.".
- The rebuild's three paragraphs, its dialog ("Rebuild curriculum progress?", its body, "Rebuild progress"), "Curriculum
  progress was rebuilt." and the five figures' names.
- `EpaOptionLabel` ("PAED-001 — Title", em dash) for every EPA a page names; `RowNames.Distinct`; `TraineeProfileWords`
  (flow 13); `ProgrammeCalendar.DateOf` for a day.
- Components: `PageHeader` (`Trail`, `CurrentCrumb`, `Page`; the trail is `Breadcrumbs.razor`'s markup, never the boards',
  round-3-check decision 7), `DataTable` (`StackWide`, `Caption`, `Class`), `StatePanel` (`LoadError`, `OnRetry`,
  `FocusAfterRetry`), `ActionResult` (`FocusOnLoad`, `FocusAfterRender`), `ConfirmDialog` (action mode, opening on Cancel,
  `BodyContent`, `DangerAction`, `InitialFocusId`, `ReturnFocusId`), `PagerControls` (`PageSizeLabel`), `FormField`,
  `FormActions`, `Alert` (`Role=""` for a standing notice), `Icon`, `InFlight`, `PageFocus.FocusByIdAsync`, `RungPicker`,
  `StageMinimaEditor` (as it renders, decision 5), `.people-page`, `.user-page`, `.user-section`, `.form-section-head`,
  `.submit-check`, `.details-list`, `.search-container`/`.search-field`/`.filter-actions` (flow 06's Show).

## Across lanes

- **Typed refusals only**: a page maps `FieldRefusal.Of(ex)` to its field, or its lane's typed reason to its words; never
  `ex.Message` (T329). Any other failure is the board's fixed sentence ("… Nothing has changed. Try again, or come back in a
  few minutes."), logged.
- **Out of scope is Unavailable** (Q19): an id the caller cannot open, or that does not exist, is one page in the route's
  own place, no id shown, its menu item lit: "EPA unavailable" / "This EPA does not exist, or you cannot open it." / Go to
  EPAs; "Curriculum unavailable" / "This curriculum does not exist, or you cannot open it." / Go to Curricula; "Item
  unavailable" / "This curriculum item does not exist, or you cannot change it." / Go to ‹the curriculum's items›.
- **Whose is what, in words** (Q3, F8): "Your institution's own EPA" / "Your institution's own item" (to the Administrator
  "‹institution›'s own EPA / item") in `.muted .text-sm` under the title; a national row's actions cell "Set by the
  College." inside the same `div.actions-cell` (M1); never a badge on the admin pages. My progress and the trainee's EPA
  page keep their badge (T357).
- **Titles** (each tab "{h1} · Wombat"): "Curricula"; "Items of ‹name› (‹version›)"; "Add your institution's own item" /
  "Add item"; "Edit ‹EpaOptionLabel›"; "EPAs"; an EPA by `EpaOptionLabel`, "Create EPA" new, "EPA" while loading or
  failed (T7); "Curriculum items" while the items page loads or fails (T7); "Entrustment scales"; "Curriculum progress";
  the profile's as flow 13 built it.
- **Times and days**: ISO days (`ProgrammeCalendar.DateOf` for today; never `DateTime.Today` or `ToLocalTime()`).
- **No new role check, no new route beyond lane B's two.** The pages keep their policies.

## § 6: words on no board, marked NEW (the fix pass)

| Words | Where | NEW because |
|---|---|---|
| (filled from the lanes' reports) | | |
