---
id: T162
title: The type picker offers the system-written msf_cpsa to trainees
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T162 — Trainees are offered msf_cpsa, a type only the system can complete

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing is corrupted. The trainee's stray draft is inert and can never become evidence, but it sits
in their activity list with no way to finish it, and the picker offers an instrument the trainee cannot use.
**Surfaced:** 2026-09-24, the EPA-stream survey (`EPA-PROGRAMME.md` § 5 item 6). The seed catalogue's own comment defers it
to [T118] findings 6-7, which were about legacy duplicate types, not this. T118 is now closed.

## Symptom

Observed at `431e69e`:

- `msf_cpsa` is Speciality-scoped, active and on the CPSA ladder (`ActivityTypeSeedCatalogue.cs:148-157`), so
  `/activities/new` offers it to every paediatric trainee. `ListActivityTypesQuery.cs:42-52` filters on version,
  `IsActive` and scope only, and the ladder filter lets it through.
- Its only transition, `record`, is `role:Coordinator|role:Administrator` (`Seeds/msf_cpsa/workflow.json`). Both sections
  are editable only by those roles, so every field a trainee submits is dropped at creation. The comment at
  `ActivityTypeSeedCatalogue.cs:148-154` admits this: "nothing in the product expresses 'system-managed'".
- `ActivityPermissionRule` is mapped and read by nothing: `Domain/Activities/ActivityPermissionRule.cs:3`,
  `ActivityType.cs:54`, `ApplicationDbContext.cs:60` and its configuration are the only references (grep).

## What to build

1. `ActivityType.SystemManaged` (bool), set from a new property on the type's `ActivityTypeSeedEntry`. `msf_cpsa` is the
   only one today. Like `WbaToolKey`, a catalogue value is written on create and never refreshed. So an existing
   database needs a migration, or a rebuild (W-007 allows either).
2. `ListActivityTypesQuery` leaves system-managed types out.
3. `ActivityService.CreateDraftAsync` refuses a system-managed type before any mutation (the audit trap). The
   system-written path (`StageCompletedAsync`) is unaffected.
4. Decide what to do with `ActivityPermissionRule`: give it this job, or delete the dead table. Recommendation: delete
   it. A per-role create rule is more than the one flag needs, and a table nothing reads misleads the next reader.

## Verification

- [ ] `/activities/new` no longer offers `msf_cpsa` to the dev trainee. Query test, and in the browser.
- [ ] `CreateActivityCommand` for `msf_cpsa` is refused and writes no row. Application test.
- [ ] A released MSF campaign still writes its per-EPA `msf_cpsa` rows. Existing [T121] tests stay green.
- [ ] Whatever is decided about `ActivityPermissionRule` is done and recorded here; if it is dropped, the migration has
      been read after generation (the `--no-build` empty-migration trap).
- [ ] Full suite green, no `--no-build`.

## Related

[T121] (the system-written MSF evidence rows), [T118] (closed), [T135] (a stray `msf_cpsa` draft also reaches the sampling
report), [T122] (`WbaToolKey`, the precedent for a catalogue-entry property).
