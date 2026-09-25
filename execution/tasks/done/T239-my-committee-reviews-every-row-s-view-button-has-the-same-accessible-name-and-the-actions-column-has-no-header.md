---
id: T239
title: My committee reviews: every row's View button has the same accessible name, and the actions column has no header
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T239 — My committee reviews: every row's View button has the same accessible name, and the actions column has no header

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A screen-reader user hears "View, View, View".
**Surfaced:** 2026-09-25, the T212 review (finding 7). It predates T212.

## What to build

Give each View an `aria-label` naming the review ("View the 2026 S2 review"), and the actions column a `.visually-hidden`
"Actions" header, as DESIGN.md asks for row actions (T198, T222). Then sweep the other lists for the same pattern and
fix them, or list them.

## Verification

- [x] bUnit: each View has a unique accessible name, and the header has text.

## Related

T212, T198, T222.

---

## As built — 2026-09-25 (`0097df6`)

Row actions are named by what they act on, and no two on one list share a name (`RowNames.Distinct`). Every actions
column has a hidden "Actions" header. A scan test checks what a name says.

Browser on dev (scripted Chrome, master `b4fd566`):
- **Unique, descriptive names** as trainee, assessor, coordinator, committee, instadmin and collegeadmin, across My
  reviews, My activities, the inbox, authorisations, My MSF reports, `/committee/reviews`, data rights, panels, users,
  trainees, activity types, the builder, entrustment decisions, audit and curriculum items.
- **Headers:** every actions header is a hidden "Actions", and there is no empty `<th>`.

**Found:** `/msf/campaigns` still has duplicate names (22), and the assessor dashboard's names repeat for one trainee.
Both are noted on [T280].
