---
id: T324
title: Pages print code names where labels belong: a revoked STAR and job runs name people by user id, the builder and the data-rights pages print enum names, and required-field messages name the model property
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T324 — Pages print code names where labels belong: a revoked STAR and job runs name people by user id, the builder and the data-rights pages print enum names, and required-field messages name the model property

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The revoked STAR certificate is a formal document a trainee may hand to others, and it names the revoker by GUID, breaking T142's rule on a record that leaves Wombat. The job-runs id, the enum names and the property-named messages are Low on their own.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-4.37a, F-A.2.10a, F-1.27b, F-A.1.3b, F-1.6b, F-2.14d, F-6.1a, F-A.4.1a).

## Symptom

- **Step 4.37:** the revoked STAR certificate reads "Revoked on 26 September 2026 by f3e77734-04e5-4c77-bfc4-e27581bfeb6f" (Dr Mokoena's user id) where it should name her (`tmp/act4b/star-dlamini-paed002-revoked.pdf`).
- **Step A.2.10:** job run history's "Triggered by" column shows the administrator's GUID on the 11 manual runs (`design/baseline/states/scheduled-job-runs-list--loaded.png`).
- **Step 1.27:** the builder's field list reads "epa_id · Epa" and "objectives · LongText", while its own Type picker says "EPA" and "Long text" (`act-1/1.27-2-form-saved-preview.png`).
- **Step A.1.3:** the data-rights queue's Status filter offers "UnderReview", and its Type filter offers "Objection", which no one can submit (`states/data-rights-requests--coordinator.png`).
- **Steps 1.6, 6.1, 2.14 and A.4.1:** saving an empty form shows "The ShortCode field is required." (institution and college forms), "The UserId field is required." (assessor profile) and "The LastName field is required." (profile), in the summary and under the field (`states/institution-edit--invalid.png`, `college-edit--invalid.png`, `assessor-profile-edit--invalid.png`, `profile--invalid.png`).

## Root cause

- **Revoker.** `EntrustmentCertificatePdfService.cs:189` prints `data.RevokedByUserId`. The loader (:230-235, :262) resolves the chair and the trainee through `FormatName`, but passes the revoker's id through raw (:270).
- **Job runs.** `ScheduledJobRunsList.razor:70` renders `run.TriggeredBy ?? "scheduler"`, which is `RunScheduledJobNowCommand.TriggeredByUserId`. `GetScheduledJobRunsQueryHandler.cs:53` carries the id and resolves no name. DESIGN § Page shapes (1693-1702) allows ids only on the audit, data-rights and verify pages.
- **Builder.** `ActivityTypeEdit.razor:218` prints `@field.value.Type`, though `FieldTypeLabel` exists at :870-880. The same page's Scope picker (:80-83) prints `ActivityScope` raw ("SubSpeciality").
- **Data-rights pages.** `RequestsList.razor:25-27,35-37` loop `Enum.GetValues` and print member names. The raw status also reaches every data-rights badge (`RequestsList.razor:65`, `RequestDetail.razor:43`, `DataRights.razor:110`), and the raw type the type cells and row names. The request form offers four types (`DataRights.razor:71-74`). Objection is a set of profile flags, and approving one only acknowledges it (`ApproveDataRightsRequest.cs:101-105`), yet `SubmitDataRightsRequest`'s validator admits it (`IsInEnum`).
- **Audit filter.** `AuditList.razor:25-27` prints `AuditCategory` raw ("DataRights", "Msf", "ActivityType").
- **Validation messages.** No form model in Wombat.Web carries `[Display(Name)]` (grep: 0), and `[Required]`/`[Range]` carry an `ErrorMessage` only at `PanelEdit.razor:687` and `ReviewDetail.razor:2067`. DataAnnotations therefore names the property: `InstitutionEdit.razor:211`, `CollegeEdit.razor:210`, `AssessorProfileEdit.razor:223` and `Profile.razor:161`. The same code affects CurriculumEdit, EpaEdit, InvitationsList (`TargetRole`), GroupMappings (`ExternalGroupId`, `WombatRole`, `InstitutionId`), TraineeProfileEdit and ReviewsSchedule (`TraineeUserId`, `PanelId`, `PeriodKey`). Seven `[Range(1, int.MaxValue)]` pickers would read "The field CurriculumId must be between 1 and 2147483647."

## What to build

A person sees a name, a state or type its label, and a refused field its own label, never a code name. DESIGN § Page shapes already states this for people (T142) and for states (T220, T250).
- **Certificate:** resolve the revoker with the chair and print "Revoked on … by <name>". Fall back to the id only when no user or name exists, as for the chair.
- **Job runs:** `ScheduledJobRunDto` carries `TriggeredByName`, filled by one `UserDisplayNames.ResolveAsync` call in the query. The page prints the name, or "Scheduler" for a scheduled run.
- **Builder:** field rows use `FieldTypeLabel`, and the Scope picker labels its options ("Sub-speciality").
- **Data-rights:** one label helper for type and status ("Under review"), used by the queue's filters, every data-rights badge on the three pages, and the row `aria-label`s. The Type filter offers the four types the form offers. Decide whether `SubmitDataRightsRequest` should refuse Objection, and say which.
- **Audit filter:** label the audit log's category filter the same way.
- **Validation:** every validation attribute on a Web form model names the field as its label does, through `[Display(Name = "Short code")]` or an `ErrorMessage` in the product's voice. A `[Range(1, …)]` picker says what to choose ("Choose a curriculum.").

## Verification

- [ ] A PDF text test on a revoked certificate: it reads "by" followed by the revoker's name, and contains no GUID.
- [ ] bUnit: job runs show the triggering administrator's name; builder field rows read "EPA" and "Long text"; the data-rights queue's options and badges read "Under review", and its Type filter offers exactly four types.
- [ ] A reflection test over Wombat.Web: every property with a `ValidationAttribute` has `[Display(Name)]` or an `ErrorMessage` on each attribute. Mutation check: remove one and it fails.
- [ ] Browser: at runbook steps 1.6, 6.1, 2.14 and A.4.1, empty saves name "Short code", "Assessor user" and "Last name". Also check step 4.37 (the revoked STAR's PDF), A.2.10 (job runs), A.1.3 (the data-rights queue) and 1.27 (the builder's field list). Retake the cited screenshots.

## Related

T142 (people by name), T220 and T250 (states by label), DESIGN.md § Page shapes and line 1202, T239, runbook steps 1.6, 1.27, 2.14, 4.37, 6.1, A.1.3, A.2.10 and A.4.1.

## Notes

- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (design/baseline/states/entrustment-scale-edit--rung-refused.png).** This widens the task by the scale editor's refusals, which name records by id and rungs by ordinal. Removing the CPSA ladder's top rung is refused with 'Curriculum item 2 is pinned to this entrustment scale and requires level 6, …' (EntrustmentScaleReferences.cs:221). 'Level 6' is the rung's Order. The v11.1 ladder labels that rung '5' (rungs 1/2/3a/3b/4/5), so the message names a rung the page does not show, and an item id nobody sees. Name the item as the curriculum pages do (its EPA code and title, and its curriculum), and name each rung by its label. There are two siblings in UpdateEntrustmentScaleCommandHandler.cs: 'Level {id} was not found on scale {id}.' (:59), and 'One or more levels are referenced by entrustment decisions and cannot be removed.' (:72-73), which names neither the rung nor how many decisions hold it. Test: a handler test on a v11.1-shaped scale whose refusal reads the rung label ('5') and the EPA code, and no bare id.
