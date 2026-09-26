---
id: T333
title: SSO group mappings can grant or remove Trainee and PendingTrainee, round admission
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T333 — SSO group mappings can grant or remove Trainee and PendingTrainee, round admission

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High, before real users. T303 made Trainee a role only admission grants and only Mark complete or
Deactivate takes away. SSO is the one path left round that rule. No provider is configured today, so it cannot be
exploited yet.
**Surfaced:** 2026-09-26, while fixing T303 (its implementer's and reviewer's findings).

## Symptom

An InstitutionalAdmin's SSO group-to-role mappings page offers Trainee and PendingTrainee. A mapping to Trainee:
- grants the role at the next sign-in, with no trainee profile and no adoption pin;
- gives a graduate the role back;
- takes the role from a running trainee whenever their group membership changes, mid-programme.

## Root cause

- `CreateSsoGroupMapping.cs:41` refuses only Administrator.
- `GroupMappings.razor:163-172` offers Trainee and PendingTrainee.
- `SsoGroupMapper.ApplyAsync` (`SsoGroupMapper.cs:58-69`) skips only Administrator, and adds and removes the mapped role
  at each sign-in.

## What to build

- Refuse Trainee and PendingTrainee in `CreateSsoGroupMapping`, stop offering them on the page, and skip them in the
  mapper as Administrator is skipped, logging a warning.
- Existing mappings to either role, and their role-assignment rows, can simply be deleted: nothing is live.
- Keep SSO provisioning's own "no group matched, so PendingTrainee" default. That is not a mapping.
- Once this lands, revise the second sentence of admission's refusal message ("An SSO group mapping to Trainee is the
  one way…", `AdmitTraineeCommandHandler.AlreadyHoldsTrainee`, T303's `6697b3d`) and the sentence T303 added to
  DOMAIN.md.

## Verification

- [ ] Handler test: `CreateSsoGroupMapping` refuses Trainee and PendingTrainee for an Administrator and an
  InstitutionalAdmin, and stores nothing.
- [ ] Mapper test: a sign-in whose groups map to Trainee neither adds nor removes Trainee; the warning is logged.
- [ ] bUnit: the mappings page offers neither role.
- [ ] Browser: runbook Step A.3.1, the mappings page, offers neither role.

## Related

T303 (Trainee is system-managed), T288 (SSO mappings across institutions), T149, T155.
