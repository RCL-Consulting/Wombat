---
id: T300
title: The activity-type builder does not follow ActivityTypeScopeGuard: an InstitutionalAdmin is offered Save, Publish and a Global scope she is refused, and the College the guard names as owner cannot open it
status: in_progress
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
---

# T300 — The activity-type builder does not follow ActivityTypeScopeGuard: an InstitutionalAdmin is offered Save, Publish and a Global scope she is refused, and the College the guard names as owner cannot open it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. No write gets through, because the guard refuses every save, discard and publish. But the platform's central admin page offers an InstitutionalAdmin every edit on the twelve College instruments. Every new type she starts is also refused on its first save unless she changes Scope. The College, which the guard names as the author of Speciality- and SubSpeciality-scoped types (T091), cannot open the builder at all, so only an Administrator can change a College instrument.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-1.25a, F-1.26a).

## Symptom

- **A College instrument looks editable to an institution** (Step 1.25; `design/baseline/act-1/1.25-1-mini-cex-form.png`, `states/activity-type-edit--college-instrument.png`). Prof Mbatha (InstitutionalAdmin, KGK) opens Mini-CEX (Paediatrics), a Speciality-scoped `*_cpsa` type. The header offers Save draft, enabled, and Publish, disabled only because there is no draft. Every section and field editor is live: Add section, Add field, Edit, Up, Down and Delete. Any save she sent would be refused: "You do not have permission to modify activity types in that speciality."
- **A new type offers scopes she cannot save** (Step 1.26; `design/baseline/states/activity-type-edit--new.png`, `act-1/1.26-1-new-type-default.png`). Scope defaults to Global and offers Global, Institution, Speciality and SubSpeciality, and Speciality then offers Paediatrics. The guard accepts only Institution with KGK from her, so a new type saved without touching Scope is refused: "Only global administrators may edit a globally-scoped activity type."
- **The list offers Edit on every row** (Steps 1.24 and 1.31, `act-1/1.31-1-types-all.png`). That is 22 rows, twelve College instruments and ten Demo types, and she may write none of them.
- **The College cannot reach its own instruments.** Dr Kruger (CollegeAdmin, CPSA) is refused `/admin/activity-types` and `/admin/activity-types/{id}`, and his nav has no Activity Types, although the guard lets him write the College's Paediatrics-scoped types. This was verified in code; no runbook step plays it.

## Root cause

- **The page does not know whether the caller may write.** `ActivityTypeEdit.razor:21-26` renders Save draft, Discard draft and Publish for every caller, and the section and field editors (`:180-232`), and the workflow and credit editors, take no read-only state. `ActivityTypeEditorDto` carries nothing about writing: `GetActivityTypeEditorQuery.CanReadAsync` (`GetActivityTypeEditorQuery.cs:107-150`) answers only whether the caller may read, and any published, active type is readable by anyone.
- **The Scope picker is not the guard's.** `ActivityTypeEdit.razor:81` loops `Enum.GetValues<ActivityScope>()`. `ScopeIdOptions` (`:453-465`) lists whatever `GetInstitutionsListQuery`, `GetSpecialitiesListQuery` and `GetSubSpecialitiesListQuery` return, which for an InstitutionalAdmin is her institution and her adopted disciplines. `GetActivityTypeEditorQuery.cs:72` makes a new type Global.
- **The list has no write flag.** `ActivityTypesList.razor:15` and `:53` offer New activity type and Edit to every caller, and `ActivityTypeAdminListItemDto` has no field for it.
- **The policy is not the guard's either.** `ActivityTypeScopeGuard` (`PublishActivityTypeDraftCommand.cs:61-105`) is the rule. An Administrator may write anything. Global is Administrator-only. Institution admits only the InstitutionalAdmin of that institution. Speciality and SubSpeciality admit `CanAccessCollege` of the owning College, a case written with T091. Both builder pages are `AdministratorOrInstitutionalAdmin` (`ActivityTypeEdit.razor:3`, `ActivityTypesList.razor:2`; `AuthorizationPolicies.cs:74-75`), and the CollegeAdmin's nav section (`NavMenu.razor:160`) has no Activity Types. The pages predate T091's re-scoping and were not moved with it.

## What to build

1. **One rule, shared by the pages and the commands.** Add a pure predicate in Application for writing a type at a (scope, scopeId), with the DB lookups the guard does now, in the way `CurriculumAdminScope` serves the curriculum pages. Add a second one listing the (scope, target) pairs a caller may create in:
   - an Administrator: all four scopes and every target;
   - an InstitutionalAdmin: Institution, with her own institution;
   - a CollegeAdmin: Speciality and SubSpeciality, with his College's.
   `ActivityTypeScopeGuard` calls the first predicate and throws on false, so the pages and the commands cannot drift.
2. **The queries carry the rule.** Add `CanWrite` to `ActivityTypeEditorDto` and to `ActivityTypeAdminListItemDto`, set from the predicate. The editor query also returns the writable scopes and targets. The Scope picker lists exactly those (DESIGN.md: a picker offers exactly what the command it feeds accepts), and a new type defaults to the first of them. For an InstitutionalAdmin that is Institution with her own institution, never Global.
3. **Read-only when the caller cannot write.** When `CanWrite` is false, render no Save draft, Discard draft or Publish, and no Add, Up, Down or Delete. Show the metadata, workflow and credit as reads. Put a standing `Alert` (`Role=""`) at the top saying whose type it is and that she can read it but not change it ("Set by the College" for a Speciality or SubSpeciality type, the Administrator for a Global one). The field editor still opens so that a field can be read, since Step 1.25 reads `overall_level`'s scale there (T271 item 2).
4. **The list.** Show Edit where `CanWrite` and View elsewhere, both to the same page, each named by its row as T239 names it. Show New activity type only to a caller with at least one writable scope.
5. **Admit the College.** Put both builder pages under a policy that admits Administrator, InstitutionalAdmin and CollegeAdmin (`NationalCatalogueAccess` has exactly those roles), and add Activity Types to the CollegeAdmin nav section. This is the recommended default, because the guard already names the College as the author of its disciplines' types (T091). Record it as a decision. The rejected alternative is to narrow the guard's Speciality and SubSpeciality cases to the Administrator, so that College instruments change only by seed release. Note that T103's refresher stops refreshing a seeded type once someone other than the seeder publishes it. That is the intended hand-over, and it is why scenario replays still must not save drafts on seeded types (Step 1.25's note).

Compatibility is not a constraint: change the DTOs and the policy outright.

## Verification

- [ ] `ActivityTypeScopeGuardTests` becomes a table (Application test). Callers: an Administrator; an InstitutionalAdmin of A, judged on A and on B; a CollegeAdmin of CPSA, judged on his own College's speciality and sub-speciality and on another College's. Scopes: Global, Institution, Speciality and SubSpeciality. In every cell, the editor's and the list's `CanWrite` equal the guard's verdict.
- [ ] bUnit: the builder on a Speciality-scoped type as an InstitutionalAdmin renders no Save draft, Discard draft, Publish, Add, Up, Down or Delete, and shows the standing alert. The same type as the owning CollegeAdmin renders them.
- [ ] bUnit: New activity type as an InstitutionalAdmin offers Scope Institution only, defaulting to her institution. As a CollegeAdmin it offers Speciality and SubSpeciality of his College, and as an Administrator all four. An Application test shows the InstitutionalAdmin's Save draft succeeds without Scope being touched.
- [ ] bUnit: the list shows Edit only on writable rows and View on the rest, and hides New activity type from a caller with no writable scope.
- [ ] Policy and nav test: a CollegeAdmin is admitted to `/admin/activity-types` and `/admin/activity-types/{id}`, and is offered Activity Types in the nav.
- [ ] Browser, runbook Steps 1.24 to 1.26 as Prof Mbatha: the College rows read View, `mini_cex_cpsa` opens read-only with the alert, and the new type's Scope offers Institution with KGK only, with the first Save draft accepted. Then a new Act 6 step as Dr Kruger: the builder opens, and he saves a draft of a new Paediatrics-scoped type (not a seeded one).

## Related

T091 (the guard's College half), T211 (offer only what the command admits), T103 (seed refresher hand-over), T291 items 3 and 6 (same page), T271 item 2 (same field editor), T239 (row names). DESIGN.md § Table system (T211 paragraph) and the picker rule (about line 427). Runbook Steps 1.24, 1.25, 1.26 and 1.31. Replay findings F-1.25a and F-1.26a.

## Notes

- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (states/activity-type-edit--loading.png, --not-found.png).** The builder's header does not wait for the editor. PageTitleText (ActivityTypeEdit.razor:434) reads 'New activity type' whenever `_editor` is null. So /admin/activity-types/23 reads 'New activity type' while it loads, and an unknown id reads the same above 'The activity type could not be found.' (GetActivityTypeEditorQuery.cs:95-102 throws, and the page keeps `_editor` null, :543-559). Save draft is disabled only while another action runs (:21), so it is offered in both states. SaveDraftCoreAsync (:610-629) then sends a create (ActivityTypeId null) with an empty key, name and workflow. That is refused before anything is staged: the scope guard refuses a Global create by an InstitutionalAdmin (SaveActivityTypeDraftCommand.cs:83), and the empty workflow's parse refuses an Administrator's (:101-107). This is inferred from code; the button was not pressed. Nothing is created, but the answer is a scope or parse refusal, on a page that calls itself new. The header rework in this task should render no Save draft, Discard draft or Publish until the editor has loaded, and none after a load error. It should also take the title from the route, so a page with an id never says 'New activity type'. bUnit: in the loading and not-found states, the page renders neither 'New activity type' nor Save draft.
