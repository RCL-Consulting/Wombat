---
id: T169
title: The portfolio PDF has no per-EPA progress, and prints unrated and MSF evidence as never completed
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
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

- [x] A trainee with one discussed reflective exercise, one recorded MSF row and one logged procedure shows each as
      completed in the summary. PDF component test.
- [x] The export has a per-EPA section whose counts match the progress page for the same trainee and period. Test, and a
      check of a dev export against `/portfolio/progress`.
- [x] The existing determinism test stays green.
- [x] Full suite green, no `--no-build`.

## Related

[T023] (portfolio export), [T077]-[T080], [T130], [T135] (the terminal-state predicate), [T161] (undated lines in the
same PDF), [T166], [T168].

---

## As built — 2026-09-24

- **"Progress per EPA" section** (`PortfolioEpaProgress`, `EpaProgressSectionComponent`). It reads the same
  per-period targets as the progress page (`TraineeQuotaProgressReader.ReadForProfileAsync`) for the profile the cover
  names (`PreferredProfiles`, active or not), plus the trajectory's attributed ratings.
  - The read day is the earliest of today, the export's last day, and a completed programme's completion day.
  - A running period reads "N of M so far; K more by …". "Met" and "short" appear only once a period has ended.
  - A row with no target says why: no programme, not in the curriculum, or deactivated (T158).
- **Summary completeness** counts an activity as complete in any terminal state of its pinned workflow (D44).
- **Test determinism.** The determinism test found that concurrent QuestPDF renders can wipe the text layer; the three
  PDF test classes now share a non-parallel collection. The production side is filed as [T200].

**Browser on dev (trainee, 2026-09-24):**
- The open-ended export's section matches `/portfolio/progress` for PAED-001 (5 of 3 so far, met; 2 of 3 in S1, 1
  short; 7 ratings from 2 assessors, latest 3a), PAED-006, PAED-011 and PAED-002.
- Every current-period line reads "so far".
- The summary counts the reflective exercise (activity 32, `discussed`), the journal club (`logged`) and 6 recorded MSF
  rows as complete.
- Undated lines are marked.
- An export from 2027-02-01 prints "Periods: none within this export's dates".
- PyMuPDF extracts 3,334 words from 17 pages: the text layer is intact.

**Filed from the review:**
- [T200] the PDF render race;
- [T203] the literal-`completed` dashboards, and the partial-end rule;
- [T185] the profile picks.

The operator should know that an open-ended export's bytes change daily, because the read day is today.
