---
id: T261
title: Role dashboards link to pages that do not exist or refuse the role
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Every dashboard link resolves to a page that admits the dashboard's role. Test.

## Related

T178, T141.

---

## As built — 2026-09-25 (`a675941`)

Every role dashboard link opens a page that admits the role. A test walks each dashboard and the components it uses,
and judges every link against the page's own authorisation. A user with no role gets a "No role assigned" card with no
links, and every dashboard names the roles it is for. bUnit.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`):
- **Every role's links open:** trainee 16, coordinator, instadmin 4, collegeadmin, assessor 13, and committee.
- **The pending trainee** registered from an invitation and saw "Review your account →", which opens their profile.
- **A user with no roles** saw "No role assigned" and no links.

**Filed from the review:** nested links in `DashboardCard`, the institution edit's Back link, and the coordinator's
invitation card. They are noted for the design pass ([T266]).
