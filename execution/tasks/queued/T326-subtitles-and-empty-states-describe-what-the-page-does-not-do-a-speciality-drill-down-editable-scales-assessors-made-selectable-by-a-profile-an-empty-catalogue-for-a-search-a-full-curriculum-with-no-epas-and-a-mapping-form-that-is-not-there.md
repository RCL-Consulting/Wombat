---
id: T326
title: Subtitles and empty states describe what the page does not do: a speciality drill-down, editable scales, assessors made selectable by a profile, an empty catalogue for a search, a full curriculum with no EPAs, and a mapping form that is not there
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T326 — Subtitles and empty states describe what the page does not do: a speciality drill-down, editable scales, assessors made selectable by a profile, an empty catalogue for a search, a full curriculum with no EPAs, and a mapping form that is not there

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The fixes are copy only. Each misleads a reader about what the page does or why it is empty, but blocks nothing.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-1.6a, F-A.6.1a, F-1.22a, F-2.14c, F-1.31b, F-6.13a, F-A.3.1a).

## Symptom

- **Steps 1.6 and A.6.1:** the Institutions list says "Maintain institution records and drill into their speciality structure.", but each row offers only Edit (`design/baseline/act-1/1.6-1-institutions-one.png`, `states/institutions-list--two.png`).
- **Step 1.22:** the read-only scales list tells an InstitutionalAdmin to "Define and maintain the rating ladders…", with no Create and no actions (`act-1/1.22-1-scales-read-only.png`).
- **Step 2.14:** the empty assessor list says "Create the first assessor profile to make assessors selectable in the workflow.", but a nominee needs only the Assessor role (`states/assessors-list--empty.png`).
- **Step 1.31:** searching "zzz" with 23 types present shows "No activity types / Create the first activity type to start the builder." (`states/activity-types-list--no-match.png`).
- **Step 6.13:** `/admin/curricula/3/items` tells the College that every national EPA of Neonatology is already on the curriculum, when Neonatology has none, and the Existing items table shows its headers over an empty body (`states/curriculum-items-edit--no-items.png`).
- **Step A.3.1:** with no SSO provider, Current mappings says "Add a mapping above…" though no form renders, and the provider card tells an InstitutionalAdmin to edit application settings (`states/group-mappings--no-provider.png`).

## Root cause

- **Institutions list.** `InstitutionsList.razor:8`'s Subtitle predates T091, which made specialities national.
- **Scales list.** `EntrustmentScalesList.razor:11` has one Subtitle for both roles, although `:81` already knows `_isAdministrator`.
- **Assessor list.** `AssessorsList.razor:19`'s EmptyBody makes a claim the code does not support: `NomineeDirectory.cs` never references `AssessorProfile` (CLAUDE.md § NomineeGate: an active holder of the field's role).
- **Activity types list.** `ActivityTypesList.razor:29` has one EmptyTitle/EmptyBody for an empty catalogue and for a search matching nothing (`FilteredItems`, :75-80).
- **Curriculum items.** `CurriculumItemsEdit.razor:1137-1142` uses `AddEmptyText` whenever the picker is empty, with no branch for a sub-speciality that has no national EPA. The Existing items table has no empty line.
- **SSO mappings.** `GroupMappings.razor:116`'s EmptyBody points at a form that renders only when `SsoOptions.Value.Providers.Count > 0` (:29). The no-provider card (:108-111) addresses every viewer with an operator's instruction.

## What to build

Each page's copy says what that page does for that viewer, and each empty state names its actual cause.
- **Institutions:** a subtitle about maintaining institution records only.
- **Scales:** for a read-only viewer, a subtitle that says the ladders are set nationally and shown for reference.
- **Assessors:** an empty body that says what an assessor profile records, not that it makes anyone selectable.
- **Activity types:** a no-match state for a search ("No activity types match \"zzz\".") apart from the empty-catalogue state. Add one line to DESIGN § Alerts, validation, empty states: a filtered list's no-match state is not its empty state.
- **Curriculum items:** branch on the sub-speciality having no national EPA at all ("Neonatology has no national EPA yet. Add one first."), and give the Existing items table a "No items yet" line.
- **SSO mappings:** with no provider, the empty state points at no form, and the provider card speaks to the viewer's role. An Administrator is told about `Sso:Providers`; an InstitutionalAdmin is told to ask a Wombat administrator.

## Verification

- [ ] bUnit, one test per state:
- the Institutions subtitle;
- the scales list as an InstitutionalAdmin;
- the empty assessor list;
- the activity types list with a search that matches nothing, and with no types;
- the items page of a curriculum whose sub-speciality has no national EPA;
- the mappings page with no provider, as an Administrator and as an InstitutionalAdmin.
- [ ] Browser, runbook steps 1.6, 1.22, 1.31, 2.14, 6.13, A.3.1 and A.6.1: each page reads as above. Retake the cited screenshots.

## Related

T091 (national specialities), T239, T222 (the empty state after the last Add), T211, T288 (the same GroupMappings page, P2, security; keep it separate), CLAUDE.md § NomineeGate, runbook steps 1.6, 1.22, 1.31, 2.14, 6.13, A.3.1 and A.6.1.
