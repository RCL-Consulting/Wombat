---
id: T224
title: A Coordinator who is an MSF campaign's own subject can open, manage and watch the live counts of their own campaign
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
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

- [ ] A Coordinator who is the subject cannot list, read the setup of, add to, open, close or withdraw their own
      campaign. Handler tests.

## Related

T217, T185, T216, T205.
