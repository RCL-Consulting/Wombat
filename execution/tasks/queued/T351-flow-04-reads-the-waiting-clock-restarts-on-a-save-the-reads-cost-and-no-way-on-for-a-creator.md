---
id: T351
title: Flow 04 reads: the waiting clock restarts on a save, the reads' cost, and no way on for a creator
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-30
---

# T351 — Flow 04 reads: the waiting clock restarts on a save, the reads' cost, and no way on for a creator

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Each is a wrong order, a slow read or a missing tail in a case the scenario does not reach; nothing
is lost or refused.
**Surfaced:** 2026-09-30, T350 step 6, the build's four-sided review (`design/flows/04-assessor-inbox/build-review.md`:
R3, R4, G2, R5). Observed in the code at `b5c0e837`.

## Symptom

1. **The overdue clock restarts on an assessor's own save (R3).** `WaitingForYou` measures a row's wait from
   `Activity.UpdatedOn`, and `ActivityService.SaveDraftAsync` (which admits a CPSA assessor's save on a request) sets it.
   An assessor who opens a request seven days old, fills a field and saves turns "Waiting 7 days, Overdue" into
   "Waiting less than a day" and drops it to the bottom of the oldest-first list; the status card, which reads the
   newest move, still says "since" the move.
2. **The reads cost O(all decisions) on every Home load (R4, G2).** `GetAssessorDashboardSummaryQuery` calls
   `WaitingForYou.ReadAsync` (every candidate with its transitions, names and pinned forms) and `DecidedByYou.ReadAsync`
   (every activity the caller ever moved last, judged in C#) to show five rows each. The activity page's way on and the
   other-role line read the whole waiting list for a first row and a count, once per move.
3. **No way on for a creator who is also the assessor (R5).** `ActivityView` shows the way on only when
   `!ViewerIsAuthor`, and the author is the subject or the creator. An assessor who created a request on a trainee's
   behalf and completes it through the `field:` arm gets flow 03's inbox sentence, not "N more wait for you".

## Root cause

1. `UpdatedOn` is the one clock (T350 note 10) shared with the Coordinator's stall and the assessor nudge, and a save
   bumps it. The class doc accepts this; a fix has to move all three readers together, which is why the fix pass left it.
2. "Finished or no move left" is judged per pinned workflow in C# (note 6), so nothing is cut in SQL.
3. The page decides by identity, not by which arm of the move's actor rule admitted the viewer.

## What to build

1. Measure a wait from the newest transition's `OccurredOn` (falling back to `CreatedOn`) in `WaitingForYou`, the
   stall and the nudge together, or stop a non-author's save bumping `UpdatedOn`. The runbook's waits are unchanged
   (every scenario write to a waiting activity is a move).
2. Push the decided test, the order and the `Take` into SQL (each pinned (type, version)'s finished and dead-end states
   as an IN list), and give the way on and the other-role line a count-and-first-row read (`withTransitions: false`).
3. Carry, per available action, whether the viewer is admitted by an author arm only (`ActivityActionDto`), and show
   the way on for any move made through another arm.

## Verification

- [ ] 1: an Application test: a save by the assessor on a seven-day request keeps it overdue and first; the nudge and
  the stall agree (their tests).
- [ ] 2: an Integration (Postgres) test that the Home read is bounded whatever the number of decisions; the parity tests
  (inbox rows = Home's first five) stay green.
- [ ] 3: a bUnit test: a creator who completes through the assessor arm gets "Nothing else waits for you." and the way
  on; a creator who submits keeps flow 03's sentence.

## Related

T350 (flow 04), `design/flows/04-assessor-inbox/build-review.md`; T297; T343 (My activities' cost, the same shape).
