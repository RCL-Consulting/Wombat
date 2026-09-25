---
id: T235
title: An agenda line is never reconciled after Start: a window decided elsewhere still blocks ratify, and an EPA whose deciding STAR is revoked mid-review is not put back
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Handler tests for both cases: ratify is not refused, and the card names the undecided EPA. A mutation check on
      each gate.
- [x] Browser: both cases on dev.

## Related

T215, T131 (slices 4 and 6), D38, D46.

---

## As built — 2026-09-25 (`eb20d59`)

Each agenda line is reconciled against the one "decided in its window" rule when the agenda is read and at ratify.
- **Decided elsewhere.** A closing line whose window another sitting decided becomes optional, and records State 5 at
  ratify. It is reconciled even when the review is stranded at its institution.
- **The note** offers staging only while the review is in progress.
- **Revoked since Start.** An EPA whose deciding STAR was revoked since Start is named above the agenda, with a "Stage …"
  button for the chair.
- `RecordDecision` has no overload that settles nothing.

Handler and bUnit tests.

Browser on dev (scripted Chrome, master `810236c`):
- **Case 1, a line decided elsewhere.**
  - Review #11 (2027 S2) held PAED-003 Due.
  - Review #12 (2027 S1) staged PAED-003, recorded and ratified, issuing STAR #7.
  - On #11, PAED-003 then read "Decided elsewhere": "…need not be decided here. A decision staged on it decides it
    again." The Record reason dropped it, ratify succeeded, and the line stored State 5.
  - After recording, the note read "…Ratifying the review records that." with no Stage button.
- **Case 2, a STAR revoked mid-review.**
  - Review #13 read "Already decided in this window…: PAED-003".
  - After instadmin revoked STAR #7, the warning named PAED-003 with "Stage PAED-003", and committee2 saw no button.
  - Staging it made a chair's row, and the warning went.

**Found:** decisions-due names the routing panel, not the holding review's panel. Filed as [T259].
