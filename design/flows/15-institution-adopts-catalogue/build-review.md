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
