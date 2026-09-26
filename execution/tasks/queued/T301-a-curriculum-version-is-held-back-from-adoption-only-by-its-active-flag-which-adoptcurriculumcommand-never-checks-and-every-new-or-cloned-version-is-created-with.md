---
id: T301
title: A curriculum version is held back from adoption only by its Active flag, which AdoptCurriculumCommand never checks and every new or cloned version is created with
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T301 — A curriculum version is held back from adoption only by its Active flag, which AdoptCurriculumCommand never checks and every new or cloned version is created with

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Unticking Active is the College's only way to hold a version back from institutions. The adoption picker honours it, but the command does not. Every new version is also open for adoption from the moment it exists until the College unticks it. An institution that adopts in that window, or through a request that bypasses the picker, admits registrars into a version the College is still building, and the College's later item edits then apply to them live. The actor is a trusted InstitutionalAdmin acting for her own institution, and a re-adoption undoes it, so this is not P2.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings from the code read).

## Symptom

- **A clone is open at once** (Step 6.29; `design/baseline/act-6/6.29-2-cloned-11.2-active.png`). Dr Kruger clones 11.1 as 11.2, and the new version's page opens with Active ticked. The runbook records this as current behaviour: "a clone is adoptable the moment it exists". It stays on every InstitutionalAdmin's adoption picker until he unticks Active and saves (`6.29-3-list-11.2-inactive.png`), and only after that, at Step 6.30, does he add PAED-016 and change PAED-011's target.
- **A new curriculum is open too, with nothing in it** (Step 6.13; `design/baseline/act-6/6.13-1-neonatology-curriculum-created.png`). The Neonatology curriculum is created Active with 0 items, and the create form has no Active box, so the College can only untick it after saving. Between the two saves an empty curriculum is adoptable.
- **The command does not check the flag.** Found in code, not played: `AdoptCurriculumCommand` adopts a curriculum whether it is Active or not, so the College's hold is kept only by the adoption page's picker.

## Root cause

- `AdoptCurriculum.cs:47-52` loads the curriculum by id with no `IsActive` condition, and nothing after it checks one. Only `GetAdoptableCurricula.cs:45` filters on `entity.IsActive`.
- `Curriculum.cs:35`: `CloneAsNewVersion` sets `IsActive = true`.
- `Curriculum.cs:19` defaults `IsActive` to true, and `CreateCurriculumCommand` (`CreateCurriculum.cs:11-17`, `:66-73`) takes no Active value. `CurriculumEdit.razor:72-80` shows the box only on an existing curriculum.
- The flag's meaning is written nowhere but the runbook. Step 6.13's note says Active "decides only whether an institution may adopt the curriculum", and Step 6.31 calls ticking it "publishes 11.2". Its only readers are the picker and the curricula list's Active/Inactive column (`CurriculaList.razor:53`). No decision in EPA-PROGRAMME § 3 covers it.

## What to build

1. **Name the flag for what it does.** It is the version's release for adoption. Label the box "Open for adoption" and the list column "Open" / "Held back", with help text: "An institution can adopt this version only while it is open. Closing it does not affect an institution that has already adopted it." Renaming the column with a destructive migration is fine, since compatibility is not a constraint. An existing adoption is unaffected by closing: `TraineeAdoptionResolver` does not read the flag, and should not.
2. **A new version starts held back.** `CloneAsNewVersion` and `CreateCurriculumCommand` create it closed, and the College opens it when it is ready (Step 6.31 already does that).
3. **The command enforces it.** `AdoptCurriculumCommand` refuses a closed curriculum with "This curriculum version is not open for adoption.", before it supersedes the current adoption or stages anything (the audit trap, T201). The adoption page shows the refusal through `RefusalText` and asks its picker again (DESIGN.md's picker rule).

## Verification

- [ ] Application test: `AdoptCurriculumCommand` on a closed curriculum is refused and writes no adoption, and the institution's current active adoption stays active. Mutation-check it: removing the check fails the test.
- [ ] Domain test: `CloneAsNewVersion` returns a closed version. Application test: `CreateCurriculumCommand` creates a closed one, and neither is in `GetAdoptableCurricula` until it is opened.
- [ ] Application test: closing a version an institution has adopted leaves that adoption active, and still admits a trainee into it.
- [ ] bUnit: the curriculum edit page labels the box "Open for adoption", with its help text, and the list shows Open or Held back.
- [ ] Browser, runbook Steps 6.13, 6.29, 6.31 and 6.32: the new Neonatology curriculum and the clone 11.2 read Held back from creation, with no untick needed. Prof Mbatha's picker does not offer 11.2 until Dr Kruger opens it at Step 6.31, and then does.

## Related

T091 (adoption, Phases 4 and P5b), T211 (`CurriculumAdminScope`), T201 (refuse before mutating), DESIGN.md picker rule (about line 427). Runbook Steps 6.13, 6.29, 6.30, 6.31 and 6.32. Reviewer suspects at 6.13/6.29 ('AdoptCurriculumCommand accepts an inactive curriculum' and 'a cloned curriculum version is created Active').
