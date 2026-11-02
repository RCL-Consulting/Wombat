# Flow 15 build review, merged (T392 step 6)

Two Sonnet reviews of `t392` at `ee98ba1c`: code (`build-review-code.md`, `75e50cc6`: 0 must, 4 should, 6 nit) and pages
and runbook (`build-review-pages.md`, `0274936f`: 1 must, 9 should, 13 nit; static, nothing rendered). Merged here by the
integrator: **1 must, 13 should, 19 nit.** Checked here: M1 (`ManageCurriculumItems.cs:74,77`: `GreaterThan(0)` with no
message) and code S-1 (`GetEpas.cs:211-239`: the scope reads claims, not roles; no cross-institution read).

## Must
- **M1** (pages M1). A Target or Completion window of 0 or less shows FluentValidation's default sentence ("'Required
  Count' must be greater than '0'.") under the field. Fix: `WithMessage` on the commands' rules and page-side checks.

## Should
- **C1** (code S-1). `EpaReadScope` and the discipline picker read the College and institution claims without the role.
- **C2** (code S-2). F6 is a page rule only: AddCurriculumItem accepts an own item on a superseded version.
- **C3** (code S-3). The new state, adoptable and EPA queries are tested in memory only; add Postgres tests.
- **C4** (code S-4). The adopt dialog's consent can go stale: the command supersedes whatever is active at the save.
- **P1** (pages S1). An adoption that succeeds but whose re-read fails shows "adopted" and "could not be adopted".
- **P2** (pages S2). A saved move whose picker re-read fails says "could not be saved".
- **P3** (pages S3). `/items/new` on a superseded version says the item does not exist.
- **P4** (pages S4). A failed Deactivate… leaves the failure unfocused.
- **P5** (pages S5). The EPA pager hides when the size holds every row (T390, five lists already).
- **P6** (pages S6). `RefusalText.Of` prints an InvalidOperationException's text (profile, scales; and outside the lanes).
- **P7** (pages S7). DESIGN.md records none of: Curriculum/Item Unavailable, the item record, the Administrator's Show, the
  EPA pager.
- **P8** (pages S8). Show with no institution chosen clears the section silently.
- **P9** (pages S9). A failed Show has no Try again.

## Nits
Code: `CanEditInPlace` dead; broken `///` at `GetCurricula.cs:13`, `:265`; item order in memory in one place and in SQL in
another; `IsActiveAdoption`'s doc; `FieldRefusalException`'s remark; `GetAdoptableCurriculaQuery` for an unknown
institution. Pages: see `build-review-pages.md` (gendered comments, two failure-sentence forms (N3), "Saving...", catches
not excluding OperationCanceledException, Rebuild progress without aria-haspopup, T264's Delete, no step presses View,
A.7.10's 36 px note on the wrong step, and the rest).

## New words on no board
Listed whole, with file:line, in `build-review-pages.md` § New words; the reviewer read them as plain and in tone.

## Fix pass

2026-11-02, on `t392` (Opus), by the operator's list: M1, C1–C4, P1–P4, P6 (profile and scales only), P7–P9 and every nit;
not P5 (T390's). Commits on `t392`: `f49a909f` (C1), `3ba8ebae` (adoption), `bc07530e` (items), `f4c2a50a` (profile, scales, EPA), `b1a5e0d8` (Postgres tests), `d8f4e40e` (DESIGN.md, runbook).

- **M1** fixed (`bc07530e`). The four rules carry plain sentences (`CurriculumItemFieldMessages`: "Enter a target of 1 or
  more.", "Enter a completion window of 1 month or more.", and "Choose an EPA.", "Choose a minimum level on the scale."
  for the EPA and minimum rules); the item page checks Target and Completion window before sending, under the field,
  focused. Tests: the page (both fields, no send, no property name) and the validator.
- **C1** fixed (`f49a909f`). `EpaReadScope.ForAsync` and `DisciplinesAsync` read the College claim only for a CollegeAdmin and the
  institution claim only for an InstitutionalAdmin (`Openable`'s form). Tests: a CollegeAdmin and an Assessor carrying an
  institution claim; an InstitutionalAdmin carrying a stale College claim.
- **C2** fixed (`bc07530e`). `AddCurriculumItem` refuses an own item on a version the institution has superseded,
  `CurriculumVersionSupersededException` ("Your institution has superseded this version, so no item is added to it. Nothing
  has changed."), before anything is staged; Update and Remove stay allowed. The page says it at the form's top, focused.
  Tests: refused with nothing written (two EPAs); the T223 case that added on B's superseded version now adds on its active one.
- **C3** fixed (`b1a5e0d8`). `InstitutionCatalogueQueriesPostgresTests`: the adoptable versions (IA and the Administrator's
  choice: Relation, counts, own items not carried), the curricula list's institution state (both readers), and the EPA list
  and page (owner lookup, scope, create) on PostgreSQL.
- **C4** fixed (`3ba8ebae`). `AdoptCurriculumCommand` carries `ExpectedSupersededCurriculumId` (required, null for none); a
  different active adoption at the save is `AdoptionRefusal.Stale`, "Another adoption was made since this page was read, so
  nothing was adopted." (the page adds "The list below has been read again." and re-reads). Tests: command (two cases, nothing
  staged) and page.
- **P1** fixed (`3ba8ebae`): the re-read after an adoption is its own try; a failure keeps the success and adds "The list
  could not be read again: reload the page to see it."
- **P2** fixed (`f4c2a50a`): the profile's picker re-read after a saved move goes through `RereadChoicesAsync`, apart from the save.
- **P3** fixed (`bc07530e`): `/items/new` on a superseded version is "Cannot add an item" over F6's line, with Go to Items of ….
- **P4** fixed (`f4c2a50a`): a failed Deactivate… focuses its failure.
- **P5** not done, by the operator's word: T390's.
- **P6** fixed on the profile and scales pages (`f4c2a50a`): typed refusals (`TraineeProfileRefusedException` in Domain, for
  the end's day and an ended record; `EntrustmentScaleDeleteRefusedException`) are shown, any other exception is the page's
  fixed sentence. **Left, for a task:** `CurriculumEdit.razor:193`, `:231`, `:259` and `EntrustmentScaleEdit.razor:176` still
  assign `exception.Message`.
- **P7** fixed (`d8f4e40e`): DESIGN.md § Page-level patterns (Unavailable: Curriculum, Item, and the add's "Cannot add an
  item"), § List page (Curricula's "An institution's curricula"; the EPA list's pager), § Detail page (an item's record).
- **P8** fixed (`3ba8ebae`): Show with no institution is "Choose an institution." under the select, focused; nothing read.
- **P9** fixed (`3ba8ebae`): the failed Show is the S7 `alert-row` with Try again.
- **Code nits.** N-1 `CanEditInPlace` and `EnsureCurriculumCanBeEditedInPlace` removed; N-2 both `///` fixed; N-3 the
  command's order is ordinal (`StringComparer.Ordinal`), the assumption noted; N-4 `IsActiveAdoption`'s doc says not to read it
  off the profile's own query; N-5 `FieldRefusalException`'s remark reworded; N-6 the Administrator naming no existing
  institution reads nothing (test) (`bc07530e`, `3ba8ebae`, with N-1 to N-5).
- **Page nits.** N1 summaries reordered; N2 the pronouns in the listed comments; N3 actions now "The X could not be …"
  ("The item could not be added/saved.", "‹code› could not be removed."), reads "Could not load …"; N4 "Saving…" on the EPA
  and profile pages; N5 the EPA pages' catches spare `OperationCanceledException`; N7 Rebuild progress carries
  `aria-haspopup="dialog"` (DESIGN.md says so); N8 `col-fit` and `col-wrap`; N9 logged; N11 A.7.10's note says it is not
  that step's; N12 Step 1.21 presses View on PAED-001 (coverage row too). N6: stands, noted for T234's list. N10: T264's
  open half, not touched. N13: nothing to do.

Runbook: 1.21 and A.7.10 changed; `sync_verbatim_steps.py` re-pasted 5 steps in flows 01, 02, 07 and 15;
`check_verbatim_steps.py` (425 steps, 0 problems) and `check_flow_completeness.py` exit 0. Build: 0 warnings, 0 errors.
Suites (`-c Release`): Domain 836, Application 4334, Infrastructure 1035, Architecture 52, Web 4499, Integration 520, all passed.
