---
id: T153
title: A trainee who has left an institution still files new activities there, and is shown its staff to nominate
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T153 — A trainee who has left an institution still files new activities there, and is shown its staff to nominate

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A privacy leak of one institution's staff names and emails to someone who used to train there, and
activities stamped to a programme the subject has left.
**Surfaced:** 2026-09-24, T102 review round 2 (upheld 3/3). Pre-existing in the stamp rule (T101); T102 made it visible
by resolving the create-page picker the same way the stamp is resolved.

## Symptom

`SubjectScopeResolver` (formerly `ActivityService.ResolveSubjectScopeAsync`) prefers the active trainee profile, then
the highest-id inactive one, then the Identity row. A trainee whose only profile is an inactive one at institution A,
and whose Identity institution is now B, is stamped to A on every new activity. Since T102 the create page's nominee
picker resolves the same way, so it lists A's assessors (name and email), and the server accepts only A's.

## What to build

A decision first. The review suggested treating "has left" as a create-time fact: have the resolver report when its
answer came from an inactive profile at an institution the subject no longer belongs to, and refuse the create (before
any mutation) with a message that says so, mirrored on the create page. Changing the stamp rule alone would split it
from `CreditTargetResolver`, the T108 EPA picker and the T122 gate, which all follow the profile.

## Verification

- [ ] A subject with an inactive profile at A and an Identity institution B is refused at create, and the picker lists
  nobody — tests.

## Related

T102, T101, T108, T122.

## Note from T113, 2026-09-24

Two more places depend on a trainee's institution the same way:
- **An MSF campaign follows its trainee.** Scope is checked against the subject's current preferred profile, so a
  campaign moves with a trainee who changes institution. The fix is to stamp an institution on `MsfCampaign` (a
  migration).
- **`ListTraineesForSpecialityQuery` offers every profile at the caller's institution, past ones included,** while
  Create checks the preferred profile. The picker and the gate disagree.

## Notes

- **T295 replay, 2026-09-26 (C08).** Note, 2026-09-26 (the T295 replay, Step 5.22, F-5.22b): the same question for a completed programme, and the proposal above would not catch it. After Mark complete removed her only role (`CompleteTraineeProfile`, T080), Dr Molefe typed /activities/new. It opened 'New activity' with KGK's instruments, Case-Based Discussion to Reflective Exercise plus the KGK Teaching Session Log; she filed nothing (design/baseline/act-5/5.22-4-molefe-new-activity-opens.png). Nothing refuses her: `NewActivity.razor:2` is a bare [Authorize], `ListActivityTypesQuery` filters by the institution and speciality claims she keeps, and `ActivityService.CreateDraftAsync` (ActivityService.cs:69-160) checks no role and no current profile. So a graduate can file and submit a request about herself into an active assessor's inbox. An encounter dated on or before her last day would credit her completed, archived profile when the assessor completes it: `CreditTargetResolver.PickProfileAsync` takes an ended profile on purpose, on the stated assumption (its remarks, line 41) that graduates 'no longer submit'. T281 stops credit only for an encounter after the last day. Her ended profile is at her own institution, so 'an inactive profile at an institution the subject no longer belongs to' misses her. The decision this task asks for should therefore say who may file about themselves at all. The nav offers Activities only to Trainee and PendingTrainee, but the page admits anyone signed in: a graduate, an Assessor or a Coordinator. It should also say whether a graduate may still file a pre-end encounter late: D15 never refuses a late filing, which argues for allowing it. If she may not, refuse at create before any mutation, do not offer the form, and correct PickProfileAsync's remark. Also consider P2: an archived record that can still gain credit is data changed after the committee has decided. Verify with a handler test (a subject whose only profile is completed and who holds no role) and by replaying Step 5.22.
