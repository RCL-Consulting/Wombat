---
id: T235
title: An agenda line is never reconciled after Start: a window decided elsewhere still blocks ratify, and an EPA whose deciding STAR is revoked mid-review is not put back
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T235 — An agenda line is never reconciled after Start: a window decided elsewhere still blocks ratify, and an EPA whose deciding STAR is revoked mid-review is not put back

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A chair can be refused ratify for an EPA the decisions-due page shows as decided. And an EPA can
fall off the agenda with nobody told to decide it.
**Surfaced:** 2026-09-25, the T215 review (finding 1). It predates T215, and fixing it changes when ratify is refused.

## Symptom

Nothing re-checks an agenda line after Schedule and Start. Start only adds lines, and a closing Due line always blocks
ratify (`CommitteeAgendaLine.BlocksRatify`). Where: `StartCommitteeReview`, `AgendaPlanner`, `GetCommitteeAgenda`.

- **Decided after planning.** An S2 review holds a closing Due line for annual PAED-003. A late S1 review, still open,
  stages PAED-003 and ratifies. The S2 line stays Due, so ratify is refused ("PAED-003 must be decided at this
  sitting…"), while `/committee/decisions-due` shows PAED-003 Decided.
- **Revoked after Start.** Start left an EPA off because a STAR decided its window. That STAR is revoked while the
  review is InProgress. The EPA is not on the agenda and is named nowhere on the card, so the chair is not told to stage
  it.

## What to build

- For a Scheduled or InProgress review, re-check `CommitteeAgendaStatus.IsDecided` for each line when the agenda is
  read and at ratify.
- A closing Due line whose window another sitting has decided becomes optional ("decided elsewhere") instead of blocking
  ratify.
- On an open review, the card names, in its own sentence, any EPA that is no longer decided since Start, so the chair
  can stage it.

Keep D46: staged STARs are fixed once the decision is recorded, and the ratify gate keeps one chair check with no bypass.
Keep the audit trap: every read before the first mutation.

## Verification

- [ ] Handler tests for both cases: ratify is not refused, and the card names the undecided EPA. A mutation check on
      each gate.
- [ ] Browser: both cases on dev.

## Related

T215, T131 (slices 4 and 6), D38, D46.
