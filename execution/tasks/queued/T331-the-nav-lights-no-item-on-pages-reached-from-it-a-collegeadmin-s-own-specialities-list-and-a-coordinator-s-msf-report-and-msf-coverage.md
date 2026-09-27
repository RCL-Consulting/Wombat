---
id: T331
title: The nav lights no item on pages reached from it: a CollegeAdmin's own Specialities list, and a Coordinator's MSF report and MSF coverage
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T331 — The nav lights no item on pages reached from it: a CollegeAdmin's own Specialities list, and a Coordinator's MSF report and MSF coverage

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. On these pages the sidebar has no 'you are here'. Nothing is hidden, refused or misreported.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings S-specialities-list--loading, S-sub-specialities-list--loading, S-campaign-report--loading, S-campaign-report--blocked, S-programme-coverage--loading).

## Symptom

- Dr Kruger (CollegeAdmin) on his Specialities list: the Specialities item is not highlighted (design/baseline/states/specialities-list--loading.png). It is highlighted one level down, on /admin/specialities/{id}/sub-specialities (sub-specialities-list--loading.png).
- Mr Smit (Coordinator) on /msf/reports/{id} and /msf/coverage: no item is highlighted (campaign-report--loading.png, campaign-report--blocked.png, programme-coverage--loading.png). On /msf/campaigns/{id}, MSF Campaigns is highlighted.

## Root cause

- NavMenu.razor:47 renders each role link as `<NavLink class="nav-link" href="@link.Href">` with the default NavLinkMatch.Prefix, so an item lights only on addresses under its own href.
- Specialities' href is /admin/specialities (NavMenu.razor:117). That page is MySpecialitiesRedirect (MySpecialitiesRedirect.razor:1), which sends a CollegeAdmin on to /admin/colleges/{id}/specialities (:25). A CollegeAdmin has no Colleges item, so none of his items matches that address.
- The MSF report (/msf/reports/{CampaignId}, CampaignReport.razor:1) and MSF coverage (/msf/coverage, ProgrammeCoverage.razor:1) are reached from MSF Campaigns (CampaignsList.razor:17, :82; CampaignEdit.razor:385), but live outside /msf/campaigns (NavMenu.razor:109).
- Found by reading the code, not observed: /activities/{id}, reached from the inbox, My Activities, the dashboards and a review, lights nothing either. Which item it should light depends on the list the reader came from, so leave it unlit unless a rule is chosen.

## What to build

- Let a nav item name the other addresses it owns, declared once on its NavItem (T178's one-declaration rule):
  - Specialities also matches /admin/colleges/{n}/specialities and the pages under it.
  - MSF Campaigns also matches /msf/reports and /msf/coverage.
- Match those addresses in a NavLink subclass that overrides ShouldMatch (check that .NET 10 exposes it), or in a small matcher NavMenu applies.
- Do not light two items at once. An Administrator on /admin/colleges/{n}/specialities already lights Colleges, and has no Specialities item.
- The alternative is to move the two MSF pages under /msf/campaigns. That changes addresses the runbook names. Choose one approach and record it in DESIGN.md § The NavMenu.

## Verification

- [x] bUnit beside NavMenuAuthorizationTests: with the NavigationManager at /admin/colleges/5/specialities and a CollegeAdmin principal, Specialities carries the active class. At /msf/reports/3 and at /msf/coverage, a Coordinator's MSF Campaigns carries it. At /admin/colleges/5/specialities, an Administrator's Colleges carries it and no second item does. — `b347e11c`: `Navigation/ActiveNavItemTests`, from `NavOwners`. Specialities is lit for a College admin and Colleges for an Administrator on `/admin/colleges/{{id}}/specialities`, and MSF campaigns for a Coordinator on `/msf/reports/{{id}}` and `/msf/coverage`. At most one item is lit, and the real Routes' cascade is tested.
- [ ] Browser: retake states specialities-list--loading (Dr Kruger), and campaign-report--blocked and programme-coverage--loading (Mr Smit), each with the owning item highlighted.

## As built in T335 (`b347e11c`, 2026-09-27)

- The browser item belongs to T335's step G replay.

## Related

T178 (one nav declaration per page), T291 item 2 (Back to colleges on the same Specialities page), DESIGN.md § The NavMenu, tests/Wombat.Web.Tests/Navigation/NavMenuAuthorizationTests.cs, the T295 states sweep.
