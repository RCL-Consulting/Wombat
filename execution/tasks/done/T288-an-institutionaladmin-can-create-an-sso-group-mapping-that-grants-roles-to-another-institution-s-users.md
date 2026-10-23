---
id: T288
title: An InstitutionalAdmin can create an SSO group mapping that grants roles to another institution's users
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
started: 2026-10-23
completed: 2026-10-23
---

# T288 — An InstitutionalAdmin can create an SSO group mapping that grants roles to another institution's users

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High once an SSO provider is configured. Dev and production have none today.
**Surfaced:** 2026-09-25, the T278 review.

## Symptom

- `CreateSsoGroupMapping` checks only `request.InstitutionId`, never the chosen provider's own institution.
- `SsoGroupMapper.ApplyAsync` loads mappings by `ProviderKey` alone.
- `DeleteSsoGroupMapping` checks only the mapping's institution.
- Nothing in create or delete asks the trainee-first rule.

As a result, an InstitutionalAdmin at A can map B's provider groups to roles, and those roles are granted to B's users at
their next SSO sign-in. A Trainee who is also an admin can delete the mapping that gave them Trainee, and lose the role at
their next sign-in.

## What to build

- The provider's institution must equal both the mapping's institution and the caller's, checked before any write.
- The mapper filters mappings by the provider's institution.
- The mapping commands and page refuse anyone who holds Trainee, first (T185).

Handler tests cover each case and the audit trap. A mapper test shows another institution's mapping is ignored.

## Verification

- [x] A cross-institution mapping is refused, and the mapper ignores one that already exists. A Trainee cannot manage
      mappings. Tests. (`SsoMappingAcrossInstitutionsTests`, 9; Application 3994, Web 3980, Infrastructure 1029,
      Architecture 52, Integration SSO 22 green)

## Related

T278, T149, T155, T027.
- **2026-10-23, done (T376's prerequisite, Q16/F5).** `CreateSsoGroupMapping` asks the trainee-first rule
  (`UserAdministrationRules.DemandUserAdministration`), then the caller's institution, then that the provider is
  configured and serves that institution (`ProviderNotTheInstitutions`), the Administrator included. `Delete` and
  `List` ask the trainee-first rule before anything. `SsoGroupMapper.ApplyAsync` takes the provider's institution and
  applies only its mappings; both `ExternalLoginHandler` call sites pass it. The page shows T278's Trainee note in place
  of everything and offers only the caller's institution's providers (the Administrator sees all). The page's redesign
  is T376's (flow 12, O boards); T333 (Trainee and PendingTrainee) follows.
