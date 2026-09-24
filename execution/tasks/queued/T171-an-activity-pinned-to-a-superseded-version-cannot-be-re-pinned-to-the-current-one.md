---
id: T171
title: An activity pinned to a superseded version cannot be re-pinned to the current one
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T171 — An activity stranded on a superseded version has no way onto the current one

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low while nothing is live; pre-launch readiness, not needed now. Every seed republish strands in-flight work
on the old version ([T105] republished 12 types at once). Once real users exist, "cancel and refile" is the only remedy,
and it loses the assessor's work and the encounter's history.
**Surfaced:** 2026-09-24, the EPA-stream survey. This is D33 part 2 (`EPA-PROGRAMME.md` § 3D). [T107] takes D33 part 1
(suppress the impossible action). The survey recommended deferring the re-pin and filing it separately.

## Symptom

Observed at `431e69e`:

- `Activity.SchemaVersion` is assigned in one place, `BuildDraftActivity` (`ActivityService.cs:143`), shared by create
  and the system-written path. The assignment at `:1064` is the detached `ProbeInState` copy, not a re-pin.
- Every read and write validates against the pinned version (`GetPinnedVersion`, `ActivityService.cs:820`).
  `ActivityTypeSeedRefresher.cs:82-87` states the consequence: "In-flight activities stay pinned and are not unblocked
  by a republish."
- So an activity whose pinned version cannot be completed stays that way for life.

## What to build

An explicit, audited "move to the current version" for an activity whose pinned version is superseded:

1. **Who and when:** decide which roles may re-pin (recommendation: the author before anyone else has acted, and an
   Administrator) and in which states (not terminal).
2. **Guarded by re-validation:** the stored `DataJson` is validated against the target version's schema and its
   current state must exist in the target workflow. The [T102] nominee gate and the [T122] tool gate judge the data as
   on any write. A refusal names what does not fit. Every check runs before the first mutation (the audit trap).
3. **Audited:** record the from and to versions and the actor, as a transition-history entry or an audit event.
4. **Credit, which needs its own design:** re-pinning overturns D8's premise that `counts_for` is permanent for an
   activity (`EPA-PROGRAMME.md` § 3A-ii, D8), and it changes what `RebuildCurriculumProgress` replays. Decide whether a
   re-pin is refused when the two versions' credit rules differ, or allowed and followed by a replay of that
   activity's credit. Recommendation: refuse on differing credit rules for now. It keeps D8 true and covers the
   stranded-form case that motivates this task.

## Verification

- [ ] An activity pinned to a superseded version whose data fits the current version can be re-pinned, and can then be
      completed. Application test, and in the browser on dev after a republish.
- [ ] A re-pin whose data or state does not fit is refused with a reason and changes nothing. Application test.
- [ ] A re-pin across differing credit rules behaves as decided. Test.
- [ ] The re-pin is recorded with both versions and the actor. Test.
- [ ] Full suite green, no `--no-build`.

## Related

D33 (part 2), D8, [T107] (part 1: suppress), [T103] (the refresher that republishes), [T105], [T102], [T122],
[T119] (rebuild replays).
