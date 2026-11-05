# T394 (flow 16) build: code review of the Application halves

Reviewed: `git diff master...t394` at `770c0b27` (`src/Wombat.Application`, the new Web words and the pages' catch blocks, and
the tests that prove them); the Razor layout is another reviewer's. Line numbers are the worktree's files at that head. Read
against `build-contracts.md` § 0, CLAUDE.md's scope, jsonb and architecture rules, and flow 15's `build-review-code.md`.

**Counts: 0 must, 4 should, 8 nit.** No defect that lets a CollegeAdmin or InstitutionalAdmin read or act outside their College
or institution was found; no exception text reaches a flow 16 page; no refusal leaves a mutation staged ahead of a throw.

Run (observed), `-c Release`, no `--no-build`: Application tests filtered to `EntrustmentScaleReorderGuard`, `UpdateCurriculumTests`,
`CollegeRefusal`, `SpecialityCommandRefusal`, `CatalogueReach`, `GetCollegeTree`, `EpaScopeGuard`: 83 passed, 0 failed. The PostgreSQL
integration suites were not run.

## Must

None.

## Should

### S-1. Q16 and Q19 are checked, then saved, with nothing holding what was checked

`UpdateEntrustmentScaleCommandHandler.cs:94-102` reads "in use" and the save is at `:144`; `UpdateCurriculum.cs:102-104` reads
items and adoptions and the save is at `:137`. Both guards are reads, with no row lock, no concurrency token and no re-check after
the save (the scale, the curriculum and the adoption carry no row version).

Failure scenario, Q19: the College admin presses Save with a changed sub-speciality on an empty version; an Institutional admin
adopts that version in the window. The adoption row stores the old `SubSpecialityId` and the curriculum the new one, which is the
invariant Q19 exists to keep ("an adoption's `SubSpecialityId` always equals its curriculum's", `UpdateCurriculum.cs:51-52`).
Scenario, Q16: an admin saves a reorder while another saves a curriculum item pinning the scale; the CPSA ladder's minima then name
other rungs, the exact harm T395 closes. Both windows are small and the actors are different roles, so a race is realistic only
rarely, but the harm is silent.

Fix: either re-ask the rule after `SaveChangesAsync` inside the transaction the pipeline gives the request and throw (the pipeline
discards the refused save, T201), or take a lock on the curriculum or scale for the check-and-save, as the repo does for credit
(`IEpaCreditLock`). At the least add a PostgreSQL test that shows the window, so the decision to accept it is written down. Flow 15's
guards have the same shape; this is the first place the harm is silent.

### S-2. The new reads are proved only on the in-memory provider

`CatalogueReachTests.cs:171`, `GetCollegeTreeTests.cs:26`, `EntrustmentScaleReorderGuardTests.cs:244` and `EntrustmentScaleUsesTests`
use `UseInMemoryDatabase`. Not proved to translate to SQL: `GetCollegeTreeQueryHandler.cs:330-356` (nested
`OrderBy(...).Select(new Dto(..)).ToList()` inside a `Select`, with a null-conditional on `DefaultEntrustmentScale`);
`CatalogueReach.cs:96-101` and `:219-225` (`GroupBy` over a composite key then `Select`); `EntrustmentScaleUses.FirstUseAsync`
(`decision.AuthorisedLevel.ScaleId`, `:111-114`); `GetEntrustmentScaleByIdQueryHandler` (`GetEpas.cs:473-496`, `Include` +
`AsNoTracking`, then `IsInUseAsync`). CLAUDE.md and `CurriculumAdminScopePostgresTests` say the in-memory provider is not evidence for
SQL. The PostgreSQL tests added are for duplicates only (`CurriculumDuplicatePostgresTests`, `SubSpecialitySaveRacePostgresTests`).

Fix: one PostgreSQL run each of `GetCollegeTreeQuery`, `CatalogueReach.ForCurricula` and `ForEpa` (an owner EPA included),
`GetEntrustmentScaleByIdQuery` on a used and an unused scale, and `UpdateEntrustmentScaleCommand`'s reorder refusal.

### S-3. D57's "no writes" is proved for the Institutional admin and never for the College admin

`EntrustmentScalesListPageTests.cs:152` proves the College admin sees the list with no actions column. The writes are refused by
`IsAdministrator()` at `CreateEntrustmentScaleCommandHandler.cs:21`, `UpdateEntrustmentScaleCommandHandler.cs:28` and
`DeleteEntrustmentScaleCommandHandler.cs:23`, which is correct, but `EntrustmentScaleAdminHandlerTests.cs:749` is the only
non-Administrator handler test (an Institutional admin, create only). Update and Delete have no refusal test for any non-Administrator,
and nothing pins the College admin, the one role D57 newly brings near the scales.

Fix: a theory over create, update and delete with `TestPrincipals.CollegeAdmin(...)` and `InstitutionalAdmin(...)`, expecting
`UnauthorizedAccessException` and no row changed.

### S-4. Q18's order is proved in two of its four steps

The brief's order is Q6 (live items), then the ended record (T312), then decisions, then Q16 (T395). Proved: Q6 beats Q16
(`EntrustmentScaleReorderGuardTests.cs:122`), Q6 with more than three needs (`:145`), a stage override (`:170`), and, in
`EndedCurriculumListTests.cs:408-413`, the ended-record sentence alone. Not proved: that the ended record is named before a decision
refusal; that a decision refusal (`RungDecided`, `UpdateEntrustmentScaleCommandHandler.cs:75-87`) beats Q16 when both hold; that a Q6
need beats the ended record when both exist (`EntrustmentScaleReferences.cs:236-262` returns Q6 first by construction, untested).
`RungDecided` is asserted nowhere in `tests` outside the Web page's fake.

Fix: three short tests, each a scale with both conditions, asserting `Reason`.

## Nit

- **N-1. A doc block was split.** `EntrustmentScaleReferences.cs:302-319`: the `<summary>`/`<remarks>` that described
  `DescribeDefaultOfASubSpecialityAsync` now sit above `DefaultHoldersAsync`, followed by a second `<summary>`; and
  `DescribeDefaultOfASubSpecialityAsync` (`:330`) has none. Move `DefaultHoldersAsync` above the original block.
- **N-2. Two ways to read a unique violation.** `CollegeHolders.cs:83` and the curriculum commands use
  `PostgresErrors.IsUniqueViolation` (which walks the inner chain); `SpecialitySaveRefusal.cs:25,49` and `SubSpecialitySaveRefusal.cs`
  keep a private `"23505"` and look only at the direct `InnerException`. They agree today; use the shared helper.
- **N-3. The College pre-check is in the database's collation, the holder test in ordinal.** `CollegeHolders.cs:47-63`: on a
  case-insensitive collation the query returns "cpsa" for "CPSA", the ordinal filter drops it, the unique index (same collation) then
  refuses the save, the race branch finds no holder and rethrows the raw violation, so the page says "could not be saved" for a plain
  duplicate. PostgreSQL's default is case-sensitive and `Create_WithTheSameShortCodeInAnotherCase_IsNotRefusedByTheCheck` pins that, so
  this holds only while the database is. State the assumption in the remarks.
- **N-4. The delete's holder links are chosen by comparing two sentences.** `EntrustmentScaleUses.cs:39-44` calls
  `DescribeDefaultOfASubSpecialityAsync` a second time and compares its text with the first use's sentence. A rewording of either
  silently drops the links, and it is two extra reads. Return the use as a typed value (kind + sentence) from `FirstUseAsync`.
- **N-5. "In use" now runs on every scale page load.** `GetEpas.cs:493` calls `IsInUseAsync`; on an unused scale all seven reads run,
  including `IsPinnedOnAnEndedRecordAsync` (`EntrustmentScaleReferences.cs:300`), which loads and parses every ended programme's
  `EndedCurriculumJson`. Cheap while nothing is live, a full scan once trainees graduate. Consider a projection that reads only the
  scale ids of the ended items.
- **N-6. Facts for a College are computed for every caller.** `GetCurricula.cs:113-131` runs four extra reads for any caller who can
  open the version, and `SubSpecialityFixed` (`:131`) tells an Institutional admin, by a boolean, that some institution holds an item
  or an adoption on a version she can open. Not shown to her today (the edit page is the College's), but it is a cross-institution
  fact on a DTO she receives. Fill it where `cut.CanEditCurriculum` holds, as `Reach` is, or document why not.
- **N-7. A scale's name has only a pre-check.** `CreateEntrustmentScaleCommandHandler.cs:28-35` and
  `UpdateEntrustmentScaleCommandHandler.cs:43-50` refuse a taken name under Name, but a race on the unique index surfaces as the raw
  `DbUpdateException` and the page's fixed "could not be saved". The brief names only the College and speciality races, so this is
  consistent with it; it is the one name refusal that does not read the index back.
- **N-8. `UpdateSpeciality` does not check that the target College exists, `CreateSpeciality` does.**
  `CreateSpecialityCommandHandler.cs:28-31` reads the College first; `UpdateSpecialityCommandHandler.cs:30-37` goes straight to the
  name check, and the Administrator's foreign key refuses at the save (the page's fixed sentence, nothing staged wrongly, but
  inconsistent). Separately, `CollegesList.razor:8` has an unused `@using Wombat.Domain.Curricula` (the only Domain reference in a
  flow 16 `.razor`, a line that was already there at master).

## Checked and sound

- **Q18 on a scale save, order and staging.** `UpdateEntrustmentScaleCommandHandler.cs:28-102`: administrator, load, name (field
  refusal), unknown level, Q6 and the ended record (`RefusalForRemovedRungsAsync`, `:69`), decisions (`:75-87`), Q16 (`:97-102`), the
  rename warning read (`:107`), then "Nothing below this line throws" (`:111`) and only then the first assignment (`:112`). Every
  refusal precedes any change to the tracked scale, so the pipeline has nothing to commit (T201). `RefusalForRemovedRungsAsync`
  names the rung by the stored level's id and label at the item's rank, not by comparing incoming ranks: the fix for "remove rung 2
  of six" passing an item that needs rung 3.
- **T395's reorder guard.** A reorder is a kept level (`Id > 0`, found in the stored set) whose `Order` differs (`:94-96`); a new rung
  at the top and a relabel change no stored rank and stay allowed; "in use" is `EntrustmentScaleUses.IsInUseAsync`, the read that
  fills `EntrustmentScaleDto.InUse` for the page (`GetEpas.cs:493`), so the page's Up, Down and Remove and the handler cannot
  disagree. The binding check reads the stored name (assignments are below), so a rename in the same save does not change the
  answer. Tests: refused with nothing changed (`:27`), unused saves (`:55`), a default holder counts (`:68`), a removal no item needs
  is `InUse` (`:84`), add-at-top and relabel save (`:100`).
- **Q19's lock.** `UpdateCurriculum.cs:95` scope first (`CanAccessCollege` on the stored College), then `:102-104` the lock read
  before the target is read and before `curriculum.SubSpecialityId = ...` (`:128`); it throws `CurriculumSubSpecialityFixedException`
  carrying the cause and the adopters, the message free of names (the audit row). A save with an unchanged sub-speciality is not
  asked (`AFixedVersion_StillSaves_WhenItsSubSpecialityIsUnchanged`). The cause order (national items, adopters, own items) is tested
  (`UpdateCurriculumTests.cs:85`, `:100`). Only the race (S-1) remains.
- **College and speciality name refusals.** Create and Update College, Create and Update Speciality read the holder before the first
  `Add` or assignment (`CreateCollege.cs:55`, `UpdateCollege.cs:60`, `CreateSpecialityCommandHandler.cs:34`,
  `UpdateSpecialityCommandHandler.cs:37`), throw `FieldRefusalException` under the field, and on the save translate a unique violation
  only when a holder exists now (`CollegeHolders.SaveAsync`, `SpecialitySaveRefusal.WasTakenAsync`, whose failed read-back answers
  false); any other refusal is rethrown by `throw;` with the database's exception. The raced refusal keeps the `DbUpdateException`
  inner, so the pipeline discards the save. Tests: pre-check, raced (`CollegeRefusalTests.cs:100`), other reason not called a
  duplicate (`:123`, `:135`), speciality and sub-speciality (`SpecialityCommandRefusalTests`, and on PostgreSQL
  `SubSpecialitySaveRacePostgresTests.cs:344`, `:392`).
- **Q10's holder.** `CurriculumAdminScope.cs:209-228` reads only, before staging; takes the lowest-id holder (`:219`) so the named
  institution is stable; names the institution and EPA only for a national item beside an institution's own (`EpaHeldWordsAsync`,
  `:237-263`), whose caller is the College or the Administrator (scope enforced before the call); an institution's own item beside a
  national one names nothing. The raced path (`:296-320`) orders by id too and uses the same words.
- **The reach (`CatalogueReach`).** Registrars are counted from `TraineeProfile` by `InstitutionId` + `CurriculumId` + `IsActive`
  (`CatalogueReach.cs:96-101`, `:219-225`); `AdoptionId` is used nowhere, and `CatalogueReachTests` builds profiles without one, so
  it would fail were it read. An institution with several adoption rows of a version is reached once, by its active row (`:109-114`).
  A superseder with no registrar left is not named (`ASuperseder_WithNoRegistrarLeftOnTheVersion_IsNotReached`). An own EPA counts its
  owner only (`:152-171`). The reads do not check scope; the callers fill the DTO only where the caller may change the record
  (`GetCurricula.cs:122` on `CanEditCurriculum`, `GetEpas.cs:178` on `MayChange`), tested by
  `TheCurriculumsPage_IsToldItsReach_OnlyWhereTheCallerMayChangeIt` and `AnEpasPage_IsToldItsReach_WhereTheCallerMayChangeIt`.
- **Scope of every new query and command.** `GetCollegeTreeQueryHandler.cs:325` returns null for `!CanAccessCollege`, and an unknown
  College is null even for the Administrator (404, not 403); the speciality and sub-speciality pages read through it, so another
  College's record reads as unavailable (`SubSpecialityEdit.razor:268` goes through `GetSpecialityByIdQuery` first). Create and Update
  Speciality and SubSpeciality refuse by `CanAccessCollege` on the stored row and the target (`UpdateSubSpecialityCommandHandler.cs:160-174`).
  `UpdateCurriculum` checks the stored and the target College (`:95`, `:123`). `EpaReadScope` reads each claim only with its role
  (`GetEpas.cs:239-240`), which closes flow 15's S-1; `EpaScopeGuardTests.cs:439`, `:451`, `:465` prove it.
- **D57.** `EntrustmentScalesList.razor:3` is `NationalCatalogueAccess` (Administrator, CollegeAdmin, InstitutionalAdmin); the actions
  column, Create, Edit, Delete… and the dialog are all behind `_isAdministrator`; the scale's own page stays `Administrator`
  (`EntrustmentScaleEdit.razor:3`); the three write handlers refuse a non-Administrator. The nav gains the item for the CollegeAdmin
  (`NavItems.cs:161`). The list query returns the scales (global data) with no institution content.
- **Architecture.** No EF type crosses a handler's signature; handlers use `IApplicationDbContext` and name `DbUpdateException` only to
  catch it (the repo's pattern). No flow 16 `.razor` uses a Domain type (see N-8 for the one unused `using`); the Domain imports in
  `CurriculumWords.cs`, `CurriculumItemsWords.cs` and `EpaWords.cs` are `.cs` words classes.
- **Exception text and the pages (T393).** Every flow 16 page catch is a typed refusal read by its data
  (`EntrustmentScaleUpdateRefusedException`, `CurriculumSubSpecialityFixedException`, `CurriculumItemOnEndedRecordsException`), a
  `FieldRefusal` (messages written by the handlers), or the page's fixed sentence with `Logger.LogError`. The only other `.Message`
  reads are `EntrustmentScalesList.razor:216` (the delete's typed refusal, whose sentence is the product) and
  `CurriculumItemPage.razor:727` (`CurriculumVersionSupersededException`, typed). `InstitutionsList.razor:63` still prints
  `exception.Message`; it is not in this diff.
- **Racing saves leave nothing staged.** Every refusal above is thrown before the first `Add` or assignment, or (the raced ones)
  carries the database's exception so the pipeline discards the save; the curriculum duplicate narrows to the unique violation only
  (`CurriculumFieldMessages.IsDuplicate`) and keeps the inner (`CreateCurriculum.cs:139`, `UpdateCurriculum.cs:139-143`,
  `CloneCurriculum.cs:66-70`). `CurriculumDuplicatePostgresTests` proves it on PostgreSQL.
- **Dead code from the rewrite.** `ThrowIfPinnedItemNeedsARemovedRungAsync`, the old `Unreachable`, the synchronous `EnsureEpaNotYetOn`
  and `EpaHeldRefusal` are gone with no reference left (grep of `src` and `tests`; the Web fake's own `EnsureEpaNotYetOn` is its own
  method); flow 15's `CanEditInPlace` and the two broken `<summary>` comments are fixed. The only leftover is N-1.

## Test coverage against the brief's list

Covered: Q6 and Q16 order and wording, the three-named tail, stage overrides, add-at-top and relabel, Q19 both causes and the unchanged
save, the College, speciality and sub-speciality duplicate (pre-check, raced, other reason), Q10's wording, the reach (active,
superseded, none, own EPA, caller gate), the tree's scope, EPA read scope with stray claims, D57's page.
Not covered: Q16 and Q19 under a concurrent writer (S-1); SQL translation of the new reads (S-2); a College admin's or Institutional
admin's refused scale update or delete (S-3); the ended-record, decision and Q16 order pairs (S-4); `UpdateSpeciality` to a College
that does not exist (N-8).
