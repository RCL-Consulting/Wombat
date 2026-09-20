---
id: T135
title: "The sampling denominator and numerator disagree about what counts as a rating"
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-20
---

# T135 — Two ways `TotalRatedActivities` can be wrong while `EvidenceComplete` says it is not

**Severity:** Medium. Neither is visible on the seed corpus today, and both are the same shape as the
defect [T134] just removed: a number a committee reads, wrong in a way the report asserts it is not.
**Surfaced:** 2026-09-20, in T134's design pass. Filed as one task rather than two because they are
one invariant, in one method, and would be verified together — the same convention as [T106] and
[T114]. The design panel recommended two; this is the deviation and the reason for it.

## The invariant

`GetSamplingConcentrationWarnings` reports `WithheldRatedActivities`, and `EvidenceComplete` is
`WithheldRatedActivities == 0`. The page renders that as the difference between *"we looked and it is
clean"* and *"we could not look"*. For it to mean anything, **every rated row in the window must be
either in the arithmetic or in the withheld count.** Two paths break that.

## Defect 1 — a draft counts as a rated observation

The window query filters on `SubjectUserId` and the `ObservedOn` bounds and **nothing else**. There is
no `CurrentState` predicate, and `TryParseRating` reads only `epa_id` and `assessor_user_id` — it never
looks at a rating value. So an activity that was created, had an EPA and an assessor named, and was
never submitted is counted as rated evidence a committee can sample.

T134 widened the rated set from four legacy keys to every rated type, which makes this reachable for
every v11.1 trainee where before it was reachable only for the four generic tools.

**Interacts with [T127]** — every failed Submit leaves an orphan draft behind, and the next attempt
makes another. Those orphans now land in a committee statistic.

## Defect 2 — an unparseable row is in neither column

A row that passes the rated gate and is readable, but whose `DataJson` fails `TryParseRating`, is
inside `activities.Count` and therefore **outside** `withheldRatedActivities` — while also being
outside `ratings.Count`, which is what `TotalRatedActivities` reports. It is silently in neither
column, and `EvidenceComplete` still reads true.

All four CPSA seeds use the literal `epa_id` / `assessor_user_id` keys, so the corpus is safe today.
A builder-made type need not: nothing declares where an activity's EPA or assessor live, which is the
same class of gap [T126] closed for the rating and that [T122] is queued to close for the tool.

## What to build

1. A state predicate on the window query, and a decision recorded about which states count. "Completed
   only" is the obvious answer and it is still a decision — `procedure_log` and `journal_club` declare
   terminal initial states, so "completed" is not a synonym for "finished being edited".
2. Rows that fail `TryParseRating` counted somewhere a reader can see. Either fold them into
   `WithheldRatedActivities` (they are evidence the report could not use, which is what that number
   means) or give them their own count. Do not leave them in neither.

## Verification

- [ ] A draft in the window is not counted as rated evidence — handler test, would fail today
- [ ] A rated readable row with unreadable `DataJson` moves the report off `EvidenceComplete` —
      handler test, would fail today
- [ ] `TotalRatedActivities + WithheldRatedActivities` accounts for every rated row in the window —
      a property asserted directly, since it is the invariant both defects break
- [ ] Every existing sampling test stays green **without being edited** — the fixture is the evidence
      legacy behaviour is unchanged, as it was for T134
- [ ] Full suite green, no `--no-build`

## Related

Found in [T134]'s design pass. [T127] (orphan drafts) supplies defect 1's volume. [T122] would let
defect 2 be closed properly by declaring where an EPA lives rather than reading a literal key.

## Notes

- **Observed 2026-09-20:** the window query carries no `CurrentState` predicate and `TryParseRating`
  reads two literal keys and no rating value. Both read directly from the source.
- **Not verified:** whether any draft or unparseable row exists in the dev or production data today.
  The defect is structural either way, and T134 widened who it can reach.
