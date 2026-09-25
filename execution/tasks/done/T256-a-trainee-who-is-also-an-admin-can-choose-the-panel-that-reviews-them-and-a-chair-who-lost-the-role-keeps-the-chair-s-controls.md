---
id: T256
title: A trainee who is also an admin can choose the panel that reviews them, and a chair who lost the role keeps the chair's controls
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T256 — A trainee who is also an admin can choose the panel that reviews them, and a chair who lost the role keeps the chair's controls

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A trainee who also holds InstitutionalAdmin, SpecialityAdmin or SubSpecialityAdmin can choose
the chair and external members of their own review panel.
**Surfaced:** 2026-09-25, the T237 review (out of scope 1 and 2). Both predate T237.

## Symptom

1. **Panel administration never asks the trainee-first rule.** `CommitteeDecisionAuthorization.DemandPanelAdministration`,
   `MayAdministerPanelAsync` and `PanelReachAsync` feed panel create and update, `GetDecisionPanelById` and
   `GetDecisionPanelFormOptions`. None of them checks `TraineeScopeResolver.ActsAsTrainee`.
2. **`Chairs()` does not use T237's seat rule.** A chair who lost CommitteeMember, moved institution or was deactivated
   keeps every chair action that records no attendance:
   - Ratify, Close review, Stage and Remove, and Defer and Reinstate;
   - the page's `CallerChairs` flag.

   Record already refuses. The appeal body uses `PanelSeat.AppealBodyAt` since T237.

## What to build

1. `ActsAsTrainee` first in those three places, with the panel pages saying why ("You hold the Trainee role…").
2. `Chairs()` requires the chair to be one `PanelSeat.SittingAt` still admits, so D46's "an active CommitteeMember at the
   panel's institution" holds for every chair action, and the page reads the same rule.

## Verification

- [x] A Trainee holding each admin role is refused panel administration, and the pages offer nothing. Handler and bUnit
      tests.
- [x] A chair who lost the role, moved or was deactivated is refused each chair action, and is offered none. Handler
      tests (the audit trap).

## Related

T237, T194, T185, D46.

---

## As built — 2026-09-25 (`34ff7c0`)

- **Panel administration** asks the trainee-first rule first: anyone who holds Trainee creates or changes no panel,
  and the pages say why and offer nothing.
- **The chair** acts only from a seat `PanelSeat.SittingAt` still admits (an active CommitteeMember at the panel's
  institution, not a Trainee). This covers Record, Ratify, Close, Stage, Remove, Defer, Reinstate and the page's
  `CallerChairs` flag.

Handler tests read back what each allowed action wrote; bUnit covers the pages.

Browser on dev (scripted Chrome, master `b4fd566`):
- **A trainee given SpecialityAdmin** saw "You hold the Trainee role, so you cannot create or change a decision
  panel…", with no New or Edit. The panel form showed only the note.
- **instadmin** was offered both.
- **A chair who lost CommitteeMember,** on review 15 without a reload, was refused Defer: "You chair this panel but
  cannot take the chair's actions now…". Nothing was stored. After a reload the controls were gone, with the note shown
  to the chair and to committee2. After the role was restored, the chair's controls came back.

**Filed from the review:** [T278] (P2: no self-change guard in user administration) and [T279] (P2: Start and reads
trust claims).
