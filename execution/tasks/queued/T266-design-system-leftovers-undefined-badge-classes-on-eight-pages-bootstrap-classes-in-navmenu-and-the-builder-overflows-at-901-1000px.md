---
id: T266
title: Design-system leftovers: undefined badge classes on eight pages, Bootstrap classes in NavMenu, and the builder overflows at 901–1000px
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
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

- [ ] No undefined class remains (a test that scans `.razor` files for classes app.css does not define). Browser at the
      three widths.

## Related

T226, T198, T188.

Note, 2026-09-25 (the H1 browser check): on `/msf/campaigns/4` at 1000px, the only card sits in the narrow column of
the page's two-column grid (223px wide, 447px empty beside it), and its table scrolls 143px inside.
