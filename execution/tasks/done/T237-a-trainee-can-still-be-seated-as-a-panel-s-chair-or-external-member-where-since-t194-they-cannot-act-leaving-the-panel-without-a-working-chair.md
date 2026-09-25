---
id: T237
title: A trainee can still be seated as a panel's chair or external member, where since T194 they cannot act, leaving the panel without a working chair
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T237 — A trainee can still be seated as a panel's chair or external member, where since T194 they cannot act, leaving the panel without a working chair

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It needs a user holding both Trainee and CommitteeMember.
**Surfaced:** 2026-09-25, the T194 fix.

## Symptom

T194 made `WorksOnPanel` and `HoldsSeat` refuse anyone who holds Trainee (T185's trainee-first rule). The panel form and
its save still accept such a user as Chair or External. A panel chaired by one then has no working chair, and quorum
(D46) counts a member who cannot act.

## What to build

Decide, and then build: either the panel form and `UpdateDecisionPanel`/`CreateDecisionPanel` refuse a Trainee in any
seat (the picker leaves them out, the same predicate), or the page says the seat cannot act. The recommendation is to
refuse. It is the same rule, and a seat that cannot act is a trap.

## Verification

- [x] A Trainee+CommitteeMember is not offered for a seat, and a crafted save is refused before any write. Handler and
      bUnit tests.

## Related

T194, T185, T216, D46.

Note, 2026-09-25 (the committee chain's browser check): a trainee already seated as External is still named in the
appeal-body note ("…and its external member, Demo Trainee"), including to the trainee on their own appeal, although the
seat cannot act since T194. The remit's Present list leaves them out. Whatever T237 decides for new seats, the note
should name only members who can act.

---

## As built — 2026-09-25 (`db21bf5`, D46 amended)

No one who holds Trainee sits in any seat. The panel pickers and the save use one rule, `PanelSeat.SittingAt`.
- **The appeal body** resolves only from a seat that rule still admits (`PanelSeat.AppealBodyAt`), so a chair or external
  who lost CommitteeMember, moved or was deactivated cannot resolve an appeal. The note and the page flag use the same
  list.
- **The seat rule** asks only the institution's committee members whether they hold Trainee (`WhichHoldRoleAsync`).
- **The chair field** says "Choose a chair."

Handler, bUnit and Postgres tests.

Browser on dev (scripted Chrome, master `810236c`):
- **The pickers.** With trainee given CommitteeMember, the pickers listed only committee and committee2.
- **A trainee seated by SQL** as External on panel 1 raised the warning, and was removed by the next save.
- **The appeal note** named only the chair, to committee2 and to the trainee.
- **A member who lost the role.** With committee2 made External, then losing CommitteeMember, the form went. A stale
  tab's Dismiss was refused: "You sit on this panel's appeal body but cannot resolve its appeals now…".
- **With neither able to act**, the trainee's note said so.
- **Not run:** seating trainee as chair, because the classifier refused the SQL. It is covered by handler tests.

**Filed from the review:** [T256] (P2: a trainee administers panels; a chair who can no longer sit keeps the chair's
actions) and [T257] (a chair left in the members list). **Found:** a stale warning after the save, filed as [T260].
