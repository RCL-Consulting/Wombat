---
id: T266
title: Design-system leftovers: undefined badge classes on eight pages, Bootstrap classes in NavMenu, and the builder overflows at 901–1000px
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T266 — Design-system leftovers: undefined badge classes on eight pages, Bootstrap classes in NavMenu, and the builder overflows at 901–1000px

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low, visual.
**Surfaced:** 2026-09-25, the T226 review.

## Symptom

- **Undefined badge classes.** `badge-success`, `-danger`, `-warning`, `-info` and `-primary` are not in app.css. They
  are used on AuditList, AuditDetail, RequestsList, RequestDetail, Profile/DataRights, ScheduledJobsList,
  ScheduledJobRunsList and MyAuthorisations. `badge-@item.CurrentState` on the dashboards is tinted only for five state
  keys.
- **Undefined table class.** `td.actions-cell` is on nine pages: MyActivities, ActivityInbox, CampaignsList,
  MyMsfReports, DecisionsDue, MyReviews, PanelsList, ActivityTypesList and ReviewsSchedule. Check it against DESIGN.md's
  table rules.
- **Bootstrap classes in NavMenu:** `ps-3`, `navbar`, `navbar-dark`, `container-fluid` and `nav-item px-3`.
- **The builder.** `.builder-two-col` (`minmax(320px,1fr) minmax(400px,1.4fr)`) scrolls sideways by up to 125px between
  901 and 1000px.
- **`.stage-minima`** may overflow at phone widths (not measured).
- **The entrustment decisions page** shows "Loading…" as text rather than a `StatePanel`, and `exception.Message` rather
  than `RefusalText`.

## What to build

Map every badge to a defined state token (one `BadgeFor` helper). Remove the Bootstrap classes. Make the builder
collapse by container width, not the viewport. Fix the decisions page leftovers. CSS tests where possible, and one
browser pass at 390, 950 and 1280px.

## Verification

- [x] No undefined class remains (a test that scans `.razor` files for classes app.css does not define). Browser at the
      three widths.

## Related

T226, T198, T188.

Note, 2026-09-25 (the H1 browser check): on `/msf/campaigns/4` at 1000px, the only card sits in the narrow column of
the page's two-column grid (223px wide, 447px empty beside it), and its table scrolls 143px inside.

Note, 2026-09-25 (the lifecycle browser check): the trainee's data-rights table shows UTC times with no zone label
("12:58" for 14:58 SAST). Show South African time, or label the zone.

Notes, 2026-09-25 (the final browser check): the Formative checkbox is centred against its four-line label rather than
lined up with the first line. After signing in with `returnUrl=/access-denied?...`, the user lands on Access denied even
when now authorised.

---

## As built — 2026-09-25 (`ff42c17`)

- **Classes.** Every class a page uses is defined, enforced by a test (`DefinedClassTests`). Badges map to state tokens
  through one helper (`BadgeFor`).
- **NavMenu** is free of Bootstrap classes.
- **Widths.** The builder's two columns collapse by container width, `.stage-minima` fits phone widths, and the campaign
  page's lone card fills its row.
- **The decisions page** uses a `StatePanel` and `RefusalText`.

CSS and bUnit tests.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`):
- **The builder:** stacked at 950–1115px, and side by side from 1116px.
- **Stage minima** fit at 360px.
- **Campaign 4's card** fills its row at 1000px.
- **Badges** are green, red and grey by state, and the audit list's OK and FAILED badges are coloured.
- **No sideways scroll** at any measured width.
- **Not run:** accepting a teaching session (no dev SpecialityAdmin).

A nit and a pre-existing redirect are noted on this task's file.
