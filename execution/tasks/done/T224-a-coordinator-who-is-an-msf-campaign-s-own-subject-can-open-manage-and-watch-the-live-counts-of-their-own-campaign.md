---
id: T224
title: A Coordinator who is an MSF campaign's own subject can open, manage and watch the live counts of their own campaign
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T224 — A Coordinator who is an MSF campaign's own subject can open, manage and watch the live counts of their own campaign

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It needs a user who is both the campaign's subject and a Coordinator (or admin) in scope. It is a
gap in T185's "a Trainee is a trainee first" rule, on the MSF side.
**Surfaced:** 2026-09-25, the T217 review (finding 4).

## Symptom

`GetMsfCampaignSetup` (and `MsfCampaignRules.IsSubjectInScopeAsync`, which every campaign command uses) checks only
that the subject is in the caller's scope. It never checks that the caller is not the subject, which
`CanReadReportAsync` does. `WhereRunBy` (the campaign list's totals) has the same gap. Such a person can add invitees to,
open and withdraw a campaign about themselves, and sees per-group Responded counts before release.

## What to build

Refuse the subject in `IsSubjectInScopeAsync` and `WhereRunBy` together, the same way `CanReadReportAsync` does, and
apply `TraineeScopeResolver.ActsAsTrainee` where the caller is also a Trainee.

## Verification

- [x] A Coordinator who is the subject cannot list, read the setup of, add to, open, close or withdraw their own
      campaign. Handler tests.

## Related

T217, T185, T216, T205.

---

## As built — 2026-09-25 (`9520aab`)

No one runs or watches the MSF campaign about themselves.
- `MsfCampaignRules.IsSubjectInScopeAsync` and `WhereRunBy` refuse the caller's own campaign, as `CanReadReportAsync`
  already did.
- Anyone who holds Trainee runs no campaign (`RunsNoCampaigns`, the trainee-first rule), and the pages say so.
- Create checks scope before the template.

Handler tests cover each command and the list; bUnit covers the pages.

Browser on dev (scripted Chrome, master `ec58d2e`; `pg_dump` first, at `recovery/pre-g2-migrations.dump`): trainee was given Coordinator by instadmin, then removed.
- `/msf/campaigns` warned "You hold the Trainee role, so you cannot run…" and offered no New campaign.
- `/msf/campaigns/15` and `/14` read "Campaign unavailable", and `/msf/reports/15` "Report unavailable".
- **Not run:** removing the Trainee role (the permission classifier refused it; restoring it would need SQL).

**Needs confirmation, noted on [T248]:** the subject's own released report (`/msf/reports/14`) showed the coordinator's
actions card.

**Filed from the review:** [T248] (picker against create, and templates need scope).
