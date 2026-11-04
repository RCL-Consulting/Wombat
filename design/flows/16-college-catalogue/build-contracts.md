# T394 build contracts (flow 16)

The names two or more lanes share, and the ones each lane provides. Use them exactly; a lane that must differ says so in its
report. Each name says whether it exists (a path, at master before § 0) or is new (on `t394`, written by § 0, or by the
lane named). Lanes A, B, C and D run side by side in one wave (`build-lanes.md`); the runbook lane W follows the merge and
the sweep.

Flow 16's lanes are cut by page family, each owning its pages **and** the Application code behind them, so little crosses:
§ 0 holds the shapes two lanes would otherwise both write, and every other name is one lane's.

## § 0: on `t394` before any lane is cut, written by the planner

- **The boards' text**: `design/flows/16-college-catalogue/round-3/boards-text.md` (230 boards: Spec, Spec-2, Steps and the
  three check boards, then A, B, C, D, E, F, each at 1280 then its other widths) and `round-2/boards-text.md` (306 boards;
  the R2 boards that stand are the ids round 3 did not redraw, listed in `build-lanes.md` § Read first, and R2-0-spec-2's
  § 6 code changes and § 9 messages). One heading per board (`######## BOARD file`), the label strip's lines first; style,
  script, svg, head stripped; aria-labels, placeholders, values, `[checked]`, `[disabled]`, `[aria-disabled]`, `[selected]`,
  `[aria-haspopup]`, `[aria-current]` and alt text kept; a table row one `| a | b |` line, a `dt` opened `**`. Design data,
  read by every lane.
- **No migration.** Nothing flow 16 needs is stored that is not stored today: the reach, a scale's "in use", a curriculum's
  "fixed", a name's or short code's holder, the national item count and "has national EPAs" are read; a College's Created
  day is computed from `CreatedOn`. **No lane adds a migration**; a lane that finds it needs one stops and says so.
- **The reach** (NEW, `src/Wombat.Application/Features/Curricula/CatalogueReach.cs`; Q2, S16, T8): one rule for every page
  that tells it.
  - `CurriculumReachDto(IReadOnlyList<string> AdoptedBy, int RegistrarCount, IReadOnlyList<string> SupersededBy, int
    SupersededRegistrarCount)`, with `None` and `bool Reaches`. AdoptedBy: the institutions whose **active** adoption is the
    version, by name (ordinal). RegistrarCount: their **active** `TraineeProfile`s with that `InstitutionId` and
    `CurriculumId` (flow 15's `RegistrarCount` rule; never `AdoptionId`). SupersededBy: institutions that adopted it, now on
    another version, with active registrars still on it (an institution with none left is not reached).
  - `EpaReachDto(int CurriculumCount, IReadOnlyList<string> Institutions, int RegistrarCount)`, with `None`. A national EPA:
    over the reached versions holding it as a **national** item, each institution once, registrars summed (S16). An own
    EPA: over its owner's own items, the owner's adoptions and registrars alone.
  - `CatalogueReach.ForCurriculaAsync(db, ids, ct)` (three reads whatever the count), `ForCurriculumAsync(db, id, ct)`,
    `ForEpaAsync(db, epaId, owningInstitutionId, ct)`. Public static; the reads do not check scope: their callers fill the
    DTOs only where the caller may change the record.
  - Tests: `Application.Tests/Features/Curricula/CatalogueReachTests`.
- **`CurriculumDto`'s four new members** (`CurriculumDto.cs`), all **filled by § 0 in `GetCurriculumByIdQuery`** and at their
  defaults (null/0/false) on the list and from every command's returned curriculum:
  - `CurriculumReachDto? Reach`: for a caller with `CanEditCurriculum` (the College admin of its College, the Administrator);
    null for an Institutional admin (it names other institutions).
  - `int NationalItemCount`: the College's items on the version, never an own item (S9): the page's "Items (15)" and the
    clone intro. The items page's heading counts the rows its caller reads.
  - `bool HasNationalEpas`: the sub-speciality has a national EPA, active or not (S8, Q14).
  - `bool SubSpecialityFixed`: the version holds any item (national or own) or has any adoption row, active or superseded
    (Q19). Lane C's `UpdateCurriculumCommand` refuses by the same rule.
- **`EpaDto.Reach`** (`EpaDto.cs`, `EpaReachDto?`): filled by § 0 in `GetEpaPageQuery` where `MayChange` holds (the College
  for its national EPA, the Administrator, an institution's admin for its own EPA); null on the list, the by-id read and for
  a reader. Lane D renders it for a national EPA only (P3).
- **A scale's "in use"** (NEW, `src/Wombat.Application/Features/Epas/EntrustmentScaleUses.cs`, internal; Q16, T395):
  `FirstUseAsync(db, scale, ct)` is the delete's refusal moved here unchanged (its seven uses in its order, each sentence as
  built); `IsInUseAsync(db, scale, ct)`. `DeleteEntrustmentScaleCommandHandler` calls it before the removal and in its
  read-back. `EntrustmentScaleDto.InUse` (NEW `init` member) is filled by § 0 in `GetEntrustmentScaleByIdQuery`; false on the
  list and from the commands. Lane B adds the update's guard (T395) through `IsInUseAsync`. Tests:
  `Application.Tests/Features/Epas/EntrustmentScaleUsesTests`.
- **The catalogue's addresses, flow 16's** (`src/Wombat.Web/Components/Shared/CatalogueLinks.cs`, beside flow 15's):
  `Colleges`, `NewCollege`, `College(id)`, `CollegeCreated(id)` (`/admin/colleges/4?created`); `Specialities`
  (`/admin/specialities`, the College admin's menu item); `CollegeTree(collegeId)` (`/admin/colleges/2/specialities`),
  `CollegeTreeAt(collegeId, specialityId)` (`…#speciality-2`), `TreeSectionId(specialityId)` (`speciality-2`, the section
  heading's id), `TreeCreated(collegeId, TreeRecord)` (`…/specialities?created=sub:4`), `TreeSaved(collegeId, TreeRecord)`
  (`…?saved=speciality:2`), `NewSpeciality(collegeId)`, `Speciality(collegeId, id)`, `SubSpecialities(specialityId)` (the
  old list's address, now a redirect), `NewSubSpeciality(specialityId)`, `SubSpeciality(specialityId, id)`;
  `EntrustmentScales`, `NewEntrustmentScale`, `EntrustmentScale(id)`, `EntrustmentScaleCreated(id)`
  (`/admin/entrustment-scales/3?created`); `NewCurriculum`, `Curriculum(id)`, `CurriculumCreated(id)`
  (`/admin/curricula/3?created`), `CurriculumCloned(id, fromId)` (`/admin/curricula/4?cloned=2`); the token
  `ClonedToken` (`cloned`; `WithoutTokens` drops five now); `TreeRecordOf(uri, name)` reading `speciality:‹id›` /
  `sub:‹id›` (null when absent or malformed); `enum TreeRecordKind { Speciality, SubSpeciality }` and `readonly record
  struct TreeRecord(Kind, Id)` with `OfSpeciality(id)`, `OfSubSpeciality(id)` and its `ToString()` the token's value. F1's
  rule holds for every token: the arriving page re-reads its record in scope, writes the sentence, focuses it, then
  replaces its address with `WithoutTokens`; a stale or foreign id writes and focuses nothing. Tests:
  `Web.Tests/Admin/CatalogueLinksTests` (the routes theory now covers every flow 16 page's route as declared at § 0).
- **The reach in words** (NEW, `src/Wombat.Web/Components/Shared/CatalogueReachWords.cs`): `NoAdoption` ("No institution has
  adopted this version yet."), `ForCurriculum(reach)` ("Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3
  registrars in training on it." then T8's "Still used by 1 institution that has superseded it, …; 2 registrars in training
  on it."), `ForNationalEpa(code, reach)` ("PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching
  Hospital; 3 registrars in training are measured against it." / "PAED-016 is on no curriculum an institution has adopted,
  so no registrar is measured against it yet."), `ForOwnEpa(code, owner, reach)` (Spec-2 § 9's, P3), `JoinAnd(names)` ("A",
  "A and B", "A, B and C"). Every count has its singular; the singulars and a zero registrar count are NEW (§ 6). Tests:
  `Web.Tests/Admin/CatalogueReachWordsTests`.
- **D57: the College admin reads the scales** (the policy and the menu, and nothing else widened):
  `EntrustmentScalesList.razor`'s `@attribute` is `[Authorize(Policy = "NationalCatalogueAccess")]` (Administrator,
  CollegeAdmin, InstitutionalAdmin; the page's reader branch serves the College admin as it serves the institution);
  `NavItems`' CollegeAdmin row is Specialities, EPAs, Curricula, Activity types, **Entrustment scales**; DESIGN.md's NavMenu
  row and the owner table's "An entrustment scale" row say so. A scale's page (`EntrustmentScaleEdit.razor`, `Administrator`)
  is unchanged. Test: `NavMenuAuthorizationTests.ACollegeAdmin_IsAdmittedToTheScalesList_AndOfferedIt_ButNotToAScalesPage`.
- **Q20, the header** (`app.css`, under `.header-container h1`): from 1024 px `.header-container` does not wrap, its first
  child is `flex: 1 1 20rem; min-width: 0`, its `.actions-cell` `flex: none`. Below 1024 px as built (P1). DESIGN.md's header
  bullet and § app.css section order's Header line say so.
- **`app.css`, `/* ── The catalogue ── */`**: flow 16's shared part at the file's end, written by § 0 and by no lane: (1) the
  1024 px stack for `.epas-page`, `.colleges-page`, `.college-tree-page`, `.curricula-page` (moved out of flow 15 lane B's
  heading, which kept the items' and the EPA list's), and the 1100 px stack for `.curriculum-items-page` (round 3 S1, check
  decisions 1 and 2); (2) R1: below 641 px `.user-page .form-check:not(.choice-group .form-check)` a 44 px row with a 20 px
  box, in place of flow 15's `.user-page.epa-page` and `.user-page.curriculum-item-page` copies (My data rights keeps its
  own). Then the four lanes' headings with their `/* end … */` lines and nothing between: "Flow 16: the College's tree and
  Home (T394)" (A), "Flow 16: Colleges and the scales (T394)" (B), "Flow 16: a curriculum's page and Curricula (T394)" (C),
  "Flow 16: EPAs, the items and an item's page (T394)" (D). DESIGN.md § app.css section order has the shared line and one
  placeholder line per lane.
- **A time bomb defused**: two `RegisterInvitationTests` previews read the system clock against a link whose last day was
  2026-11-02, so from 2026-11-03 they failed on master; both now pass a `FixedClock` of the issue day.
- Suites on `t394` at § 0: see `build-lanes.md` § "§ 0's counts".

## § 1: lane A (the College's tree and Home) provides

- NEW `GetCollegeTreeQuery(int CollegeId, ClaimsPrincipal Principal)` → its own DTO (the College; each speciality with its
  sub-specialities and their default scale's name), null for an unknown or out-of-scope College. Read by no other lane.
- `CreateSubSpecialityCommand(int SpecialityId, string Name, string? Description, int? DefaultEntrustmentScaleId,
  ClaimsPrincipal Principal)` (the default chosen at create, Q1). `FieldRefusalException("Name", …)` from the four
  speciality and sub-speciality commands.
- The tree page reads `CatalogueLinks.TreeRecordOf` for `created` and `saved`; the record pages write `TreeCreated` and
  `TreeSaved`; the tree's section headings carry `CatalogueLinks.TreeSectionId`, which `SubSpecialitiesList.razor`'s redirect
  and lane B's delete-refusal link rely on (`CatalogueLinks.SubSpeciality` opens the record; its Cancel returns to the
  tree).

## § 2: lane B (Colleges and the scales) provides

- `CreateCollegeCommand`, `UpdateCollegeCommand`: `FieldRefusalException("Name" | "ShortCode", …)` naming the holder.
- `UpdateEntrustmentScaleCommand`'s typed refusal (lane B's to name) for Q18 (per removed rung: EPA code, curriculum name and
  version, rung label; the ended record's) and Q16 (in use); the levels' label refusal by rung; the delete's
  `EntrustmentScaleDeleteRefusedException` gains each holder's (SpecialityId, SubSpecialityId). Read by no other lane: the
  scales list links a holder with `CatalogueLinks.SubSpeciality` (§ 0), lane A's page.

## § 3: lane C (a curriculum's page and Curricula) provides

- `UpdateCurriculumCommand`'s Q19 refusal (lane C's typed reason: national items, or the adopting institutions by name;
  decision 5's words in `build-lanes.md`), and the field refusals of the three curriculum commands. The clone's result names
  its source (for `CurriculumCloned`). Read by no other lane: the items page links the curriculum's page with
  `CatalogueLinks.Curriculum` (§ 0).

## § 4: lane D (EPAs, the items and an item's page) provides

- `CurriculumAdminScope`'s T242 refusal naming the holding institution (Q10, R3-F14), still `FieldRefusalException("EpaId",
  …)`. Read by no other lane.

## § 5: words and constants already built and kept (lanes use them, do not reword)

- Flow 15's: every institution-facing word on the EPA list, an EPA's page, Curricula, the items and an item's page
  (`EpaWords`, `CurriculaWords`, `CurriculumItemsWords`: the institution's branches); "Set by the College."; the owner marks;
  "Open" / "Held back"; "EPA saved.", "EPA deactivated.", "EPA reactivated. 1 activity completed against it while it was
  inactive now counts towards progress."; "Curriculum item added.", "Curriculum item saved."; the Remove dialog; "Item
  unavailable" (`CurriculumItemsWords.cs:177–185`); the paused notice and "(inactive: not in force)"; T223's three
  refusals.
- The delete's seven refusals (`EntrustmentScaleUses.FirstUseAsync`, word for word, R3-C08, C10; Spec-2 § 9 S20); "Entrustment
  scale deleted."; the rename warning (`EntrustmentScaleReferences.DescribeWhatARenameUnbindsAsync`); "Committee STARs for
  this programme's trainees are limited to this scale's levels."; "No default — offer every scale".
- `SubSpecialitySaveRefusal.DuplicateName`; the speciality's duplicate; `EntrustmentScaleBindings.ReservedNameRefusal`; "A
  curriculum with the same name and version already exists for this sub-speciality."; "An EPA with the same code already
  exists for this sub-speciality."; "Enter a code.", "Enter a title." (`CreateEpa.cs`).
- `RequiredMarkLine` ("* marks a field you must fill in.", as `InvitationText` and `AdmissionText` write it);
  `SystemPageText.Refusal` (Access denied, flow 01's).
- `EpaOptionLabel` ("PAED-001 — Title", em dash) for every EPA a page names; `RowNames.Distinct`; `ProgrammeCalendar.DateOf`
  for a day.
- Components: `PageHeader` (`Trail`, `CurrentCrumb`; the trail is `Breadcrumbs.razor`'s markup), `DataTable` (`StackWide`,
  `Caption`, `Class`), `StatePanel` (`LoadError`, `OnRetry`, `FocusAfterRetry`, `LoadingContent`), `ActionResult`
  (`FocusOnLoad`, `FocusAfterRender`), `ConfirmDialog` (action mode, opening on Cancel, `BodyContent`, `DangerAction`,
  `InitialFocusId`, `ReturnFocusId`), `FormField` (`Required`), `FieldHelp`, `FieldRefusal` and `FieldRefusalException`
  (flow 15's), `FormActions`, `Alert` (`Role=""` for a standing notice), `DashboardCard` (its footer as `R2-A01` draws it),
  `Icon`, `InFlight`, `PageFocus.FocusByIdAsync`, `.people-page`, `.user-page`, `.submit-check`, `.detail-list`,
  `.form-section-head`.

## Across lanes

- **Typed refusals only**: a page maps `FieldRefusal.Of(ex)` to its field, or its lane's typed reason to its words; never
  `ex.Message` (T329, T393). Any other failure is the board's fixed sentence (R2-0-spec-2 § 9: "The College could not be
  saved. Nothing has changed. Try again, or come back in a few minutes.", and the speciality's, sub-speciality's, entrustment
  scale's and curriculum's; "could not be created"; "The new version could not be made."), logged.
- **Out of scope is Unavailable** (Q7): an id the caller cannot open, or that does not exist, is one page in the route's own
  place, no id shown, its menu item lit, as R2-A18a–c, B13, C17, D15, E15, F17 and R3-F16 word it ("College unavailable" /
  "This College does not exist, or you cannot open it." / Go to Specialities, or Go to Colleges for the Administrator; and
  the speciality's, sub-speciality's, scale's and curriculum's), through `StatePanel` (N19).
- **Whose is what, in words**: the College's pages never show an institution's own EPA or item (T211); the reach names
  institutions and counts registrars, never a registrar.
- **Titles**: `build-lanes.md` § How.
- **Times and days**: ISO days (`ProgrammeCalendar.DateOf`; never `DateTime.Today` or `ToLocalTime()`).
- **No new role check and no new route.** § 0's D57 is the one access change.

## § 6: words on no board, marked NEW (the fix pass)

| Words | Where | NEW because |
|---|---|---|
| `CatalogueReachWords`' singulars ("1 registrar in training on it", "1 registrar in training is measured against it", "on 2 curricula", "2 institutions that have superseded it") and zero ("no registrar in training on it") | § 0 | the boards draw the plurals of the replay's counts |
| "KGK-002 is on no curriculum Kgosi Kgari Teaching Hospital trains against, so no registrar is measured against it yet." | § 0, `ForOwnEpa` | Spec-2 § 9 words the own EPA's line, not its none (P3) |
| The clone intro's and result's singular and none; the speciality's inactive line and deactivation result; Hold back's no-adopter and several-adopter bodies; Q19's two causes | lanes A, C | `build-lanes.md` § Decision 3's words, decision 5 |
| (filled from the lanes' reports) | | |
