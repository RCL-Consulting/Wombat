---
id: T141
title: A trainee cannot reach My progress from the navigation
status: done
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
started: 2026-09-24
completed: 2026-09-24
---

# T141 — A trainee cannot reach My progress from the navigation

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The page exists and works; nothing in the navigation leads to it.
**Surfaced:** 2026-09-23, during [T130]'s browser verification.

## Symptom

The trainee navigation's "My Curriculum" links to `/placeholder/my-curriculum`. `/portfolio/progress`, the page a
registrar opens to see what is expected of them this period, is reachable only through the dashboard card.

## Root cause

The nav item predates the page. The Trainee block is shared with PendingTrainee (`NavMenu.razor:39`), and
`/portfolio/progress` is `[Authorize(Roles = "Trainee")]`, so pointing the shared item at it would refuse a pending
trainee.

## What to build

Point "My Curriculum" at `/portfolio/progress` for Trainees only (split the item out of the shared block), and update
the nav table in `execution/architecture/DESIGN.md` in the same change.

## Verification

- [x] A Trainee reaches the page in one nav click; a PendingTrainee does not see the item — checked by
      `NavMenuAuthorizationTests` and in the browser

## Related

[T130]; `NavMenu.razor:64-69`; DESIGN.md § The NavMenu.

---

## As built — 2026-09-24

- "My Progress" (Title Case, like every nav label) sits in its own Trainee-only `AuthorizeView` and points at
  `/portfolio/progress`. The review found the same defect in MSF Reports and Committee Reviews: both pages are
  Trainee/Administrator-only, but the links were in the shared block. Both moved into the Trainee-only block. The rule
  is now written under DESIGN.md's nav table: a link goes in the shared block only if its page admits PendingTrainee.
  The dead `my-curriculum` placeholder arm is gone.
- Tests. `NavMenuAuthorizationTests` compares exact link texts per role, where it used to match substrings.
  `PendingTrainee_IsNotOfferedATraineeOnlyPage` runs for each of the three pages. `EveryLinkOffered_OpensAPageTheRoleIsAdmittedTo`
  finds the page behind every rendered link and checks that its `[Authorize]` admits the role. Six mutants were
  caught.
- Browser on dev, as trainee@wombat.local: the nav shows MSF Reports, Committee Reviews and My Progress, and My Progress
  lands on `/portfolio/progress`, rendering the semester targets and the charts. The PendingTrainee half was not
  run in a browser, because dev holds no user with only that role (admin, assessor, committee, trainee). It is covered
  by the bUnit tests above.
- Filed: [T178] (the other stale DESIGN.md nav rows and the dead placeholder arms).
