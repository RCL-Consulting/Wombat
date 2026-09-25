---
id: T248
title: MSF: the campaign trainee picker disagrees with create, and anyone who can open the campaign page can create a template
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T248 — MSF: the campaign trainee picker disagrees with create, and anyone who can open the campaign page can create a template

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T224 review.

## Symptom

- `ListTraineesForSpecialityQuery` lists every profile at the caller's institution, inactive ones included, and checks
  no role. Create refuses some of them (trainee-first, a current profile elsewhere).
- `CreateMsfTemplateCommand` takes no principal, so anyone who can open the campaign page can create a template,
  including someone who holds Trainee (T224's rule).

## What to build

- Build the picker from `TraineeScopeResolver.PreferredProfiles` and the rule create uses: one predicate, as T102 did
  for nominees. This overlaps [T238]'s erased and stale profiles, so share its rule.
- `CreateMsfTemplateCommand` takes the principal and applies `MsfCampaignRules.RunsNoCampaigns` before any write.

## Verification

- [ ] The picker offers exactly whom create accepts. Handler tests.
- [ ] A Trainee holder cannot create a template. Handler test (the audit trap).

## Related

T224, T238, T113, T102.

Note, 2026-09-25 (the G2 browser check), needs confirmation: as Trainee plus Coordinator, `/msf/reports/14` (their own
released campaign) showed the coordinator's report page. It had a Coordinator actions card with an editable Narrative
box and the supervision-level list, no action button, and no T224 warning. The subject should get the trainee's view
(`/msf/my-reports/{id}`), or the T224 warning, never the coordinator's card. Check whether a learner-feedback campaign
would show teaching contexts there.
