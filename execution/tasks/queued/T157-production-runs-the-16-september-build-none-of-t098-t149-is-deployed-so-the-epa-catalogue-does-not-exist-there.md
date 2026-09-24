---
id: T157
title: Production runs the 16 September build: none of T098–T149 is deployed, so the EPA catalogue does not exist there
status: queued
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
---

# T157 — Production runs the 16 September build: none of T098–T149 is deployed, so the EPA catalogue does not exist there


> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High for the product's usefulness, none for data (production holds no real users or records). Every
piece of the CPSA v11.1 programme exists only on dev.
**Surfaced:** 2026-09-24, closing T099: a read-only query of the production database.

## Symptom (observed 2026-09-24, read-only)

```
colleges: 1=DEMO-C            specialities: 1=General Medicine        cpsa types: none
users: 1 (the admin)          users scoped per speciality: none        activities: 0
curricula: 1=IM Core Curriculum v2026.1                                 last migration: 20260619065511_T096_AuditDeleteForArchival
```

Production last received a build with T097 (2026-09-16, no migration of its own). Everything from T098 on — the
national catalogue, T101 scope stamps, T119, T122, T126, T130, T102, T105, T120, T149 — has never reached it.

## What to do

An operator step, not code:

1. `pg_dump` the production database (the HANDOFF's standing instruction; rollback = restore). Production holds only
   the admin and the demo seed, so the dump is small and the risk is low, but the migrations are many.
2. Deploy master with `deploy/deploy.ps1`. Startup applies every migration since T096, and the seeders create the
   national catalogue (College, Paediatrics speciality and sub-speciality, 15 EPAs, the v11.1 curriculum, scales,
   the 12-instrument vocabulary) and all 19 activity types.
3. Verify: `deploy/verify/drift-check.sh`, `deploy/verify/smoke-test.sh`; the startup log's seed-refresh line and no
   warnings; the admin sees the CPSA types and the 15 PAED EPAs.
4. Onboarding on production is ordinary: invite trainees and assessors with speciality **Paediatrics** / sub-speciality
   **Paediatrics** (the invitation form offers it, checked on dev 2026-09-24), then admit each trainee to the v11.1
   curriculum. Nobody has to be re-scoped: production has no users but the admin.

## Verification

- [ ] A pre-deploy `pg_dump` exists and its file is named here.
- [ ] After deploy: last migration is master's newest; the seed-refresh line reports 19 seeded types; no warnings.
- [ ] Signed in as the admin on production: `/activities/new` lists the CPSA instruments; an EPA picker lists
  PAED-001..015; the rating field shows the six College rungs.

## Related

T099 (closed by this finding), T128 (off-host backup), HANDOVER.md.
