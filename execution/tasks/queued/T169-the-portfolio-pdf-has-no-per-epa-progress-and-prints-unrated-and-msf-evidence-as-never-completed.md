---
id: T169
title: The portfolio PDF has no per-EPA progress, and prints unrated and MSF evidence as never completed
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T169 — The portfolio PDF has no per-EPA progress and counts only the literal state 'completed'

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low-Medium. The portfolio export is the record a committee and the College read. It says nothing about
progress per EPA against the v11.1 targets, and its summary tells the reader that a trainee's reflective exercises and
MSF evidence were never completed.
**Surfaced:** 2026-09-24, the EPA-stream survey, gap "the portfolio export".

## Symptom

Observed at `431e69e`:

- `PortfolioPdfService.cs:82-106` composes Summary, Entrustment, Committee, Activities, MSF and the audit appendix. It
  has no curriculum-progress or per-EPA section. Activities are grouped by type (`ActivitiesSectionComponent.cs:28`).
- `SummaryPageComponent.cs:30-31` counts an activity as complete only when `CurrentState` is the literal `"completed"`.
  Terminal states in the seeds (read from each `Seeds/*/workflow.json`) are `completed` for the rated instruments, but
  `discussed` (`reflective_exercise_cpsa`), `recorded` (`msf_cpsa`), `logged` (`journal_club`, `procedure_log`),
  `reviewed` (`qi_project`), `approved` (`reflective_note`), `verified` (`research_output`) and `accepted`
  (`teaching_session`). Every one of those prints "0 completed", whatever the trainee did.

## What to build

1. **"Complete" means a terminal state of the activity's pinned workflow**, the same predicate [T135] adopts, not the
   literal `completed`. Better still, share one helper.
2. **A per-EPA progress section**: each curriculum item's target and count for the periods the export covers ([T130]'s
   reader), the active entrustment decision against the year target ([T166]) once that exists, and MSF coverage per
   period ([T168]) once that exists. Build the section so the later two slot in without reworking it.
3. Keep the snapshot deterministic, as [T077]-[T080] required: same data, same bytes.

## Verification

- [ ] A trainee with one discussed reflective exercise, one recorded MSF row and one logged procedure shows each as
      completed in the summary. PDF component test.
- [ ] The export has a per-EPA section whose counts match the progress page for the same trainee and period. Test, and a
      check of a dev export against `/portfolio/progress`.
- [ ] The existing determinism test stays green.
- [ ] Full suite green, no `--no-build`.

## Related

[T023] (portfolio export), [T077]-[T080], [T130], [T135] (the terminal-state predicate), [T161] (undated lines in the
same PDF), [T166], [T168].
