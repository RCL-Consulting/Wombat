---
id: T261
title: Role dashboards link to pages that do not exist or refuse the role
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T261 — Role dashboards link to pages that do not exist or refuse the role

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A dead end on the first page each role sees.
**Surfaced:** 2026-09-25, the T178 review (confirmed by reading the code).

## Symptom

- `TraineeDashboard.razor`: "Complete your profile" goes to `/account/manage`, and no page has that route.
- `CoordinatorDashboard.razor`: "Issue invitation" goes to `/admin/invitations/new`, and no page has that route. The
  invitations page also refuses a Coordinator.
- `InstitutionalAdminDashboard.razor` links to `/admin/institutions`, which admits only an Administrator, and to
  `/admin/specialities`, which admits only an Administrator or CollegeAdmin.

## What to build

Point each link at a page that exists and admits the role, or remove it. Extend T178's nav test (`PageFor`,
`RefusalOf`) to every dashboard's links, so a dead link fails the build.

## Verification

- [ ] Every dashboard link resolves to a page that admits the dashboard's role. Test.

## Related

T178, T141.
