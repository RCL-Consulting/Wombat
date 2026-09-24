---
id: T160
title: The encounter date is unbounded: a future or pre-programme date is accepted, and nothing warns past D15's fourteen days
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
started: 2026-09-24
---

# T160 — An encounter date can be in the future or before the programme began, and a late filing goes unremarked

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The encounter date decides which training year's minimum applies and which period an activity
credits into ([T119], [T130]). A mistyped or invented date moves credit silently. The College's answer to D15 is
a soft warning, which is new work that nothing yet does.
**Surfaced:** 2026-09-24, the EPA-stream survey. [T119] specified both hard bounds and the warning (its "Bounding
backdating" section and test plan). [T119] shipped without them, and `EPA-PROGRAMME.md` § 3A-ii (D15) records that
neither is enforced.

## Symptom

Observed at `431e69e`:

- `SchemaValidator.ValidateDateField` (`SchemaValidator.cs:170-176`) checks only that the value parses as a date.
- `ObservationDateResolver.Resolve` (`ObservationDateResolver.cs:36-68`) takes whatever parses as the encounter date and
  bounds nothing.
- Nothing warns on a late filing. A grep of `src` for `Backdating` finds nothing. The only 14-day constant is the
  draft-nudge cutoff (`ActivityDraftNudgeJob.cs:31`).

So a trainee can file an encounter dated 2031 or 2019. Inferred: a future date credits into a semester that has not
happened yet, because [T130]'s calendar is total over every date (`EPA-PROGRAMME.md` § 3E D40;
`CreditApplier.cs:157-159`). A date before `TraineeProfile.ProgrammeStartDate` gets no stage from `GetStage`, so it
falls back to the flat minimum instead of a stage minimum ([T119]).

## What to build

[T119]'s design stands, with D15's fourteen days in place of its ninety:

1. **Two hard refusals, on the write path.** Apply them to the field the pinned schema's `observation_date_field`
   names. Refuse a date after today, taken on the South African calendar (`ProgrammeCalendar`, as the resolver's
   fallback already uses). Refuse a date before the subject's `ProgrammeStartDate`. A subject with no trainee profile
   gets the future check only. Report the refusal as a field error against the date, not a page-level exception. Judge
   a changed value on every write, as [T102]'s nominee gate does. Every check runs before the first mutation (the audit
   trap).
2. **A soft, non-blocking warning** when the encounter is more than 14 days before the filing date. Show it on the form
   and record it on the transition, so a reader later can see the filing was late. Never refuse for lateness (D15):
   a registrar blocked by a date rule types today's date instead, which destroys the date [T119] protects. Keep the 14
   as a named option or constant, not a literal.
3. **The system-written path** (`StageCompletedAsync`, MSF evidence rows dated `EvidenceCompleteOn(campaign)`) is never
   refused for lateness either. Decide whether it gets the future check; recommendation: yes, it costs nothing.

## Verification

- [ ] `observed_on` set to tomorrow is refused with a field error on that field. Application test, and in the browser.
- [ ] A date before the trainee's `ProgrammeStartDate` is refused. Application test.
- [ ] A refused create leaves no activity row behind (the audit trap). Application test.
- [ ] A date 15 days back files, shows the warning, and the transition records it. Test, and in the browser.
- [ ] A date exactly 14 days back files with no warning. Boundary test.
- [ ] Full suite green, no `--no-build`.

## Related

D15 (closed: soft warning, fourteen days), [T119] (the design and the two rules), [T130] (the calendar that makes a
future date creditable), [T102] (the changed-value-on-every-write pattern). `EPA-PROGRAMME.md`
§ 3A-ii D15, and § 3B's pre-reply D15 text at `431e69e`.
