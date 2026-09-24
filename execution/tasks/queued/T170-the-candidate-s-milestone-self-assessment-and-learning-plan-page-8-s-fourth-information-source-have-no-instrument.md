---
id: T170
title: The candidate's milestone self-assessment and learning plan, page 8's fourth information source, have no instrument
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T170 — The candidate's milestone self-assessment and learning plan have no instrument

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is one of the four information sources behind every EPA's summative decision, and nothing
College-seeded captures it. It needs College input before it can be built well.
**Surfaced:** 2026-09-24, the EPA-stream survey, gap "the fourth page-8 information source".

## Symptom

Observed at `431e69e`:

- Page 8 names four standard information sources for every EPA's summative decision. The fourth is "The candidate —
  self-reflection on the milestones achieved, and learning plans that promote progressive milestone achievement"
  (`tasks/done/T098-data/page-8-wba-tools.json:53`).
- Nothing in `src` models it. A grep for learning plan, self-assessment and milestone finds no type, entity or seed.
  `reflective_exercise_cpsa` is page 8's Reflective exercise, a reflection on a challenging case, not on milestones or a
  plan.
- Inferred: an institution could build such a form in the activity builder, but the committee view would have no slot
  for it and nothing would tie it to a period.

## What to build

**First, ask the College** (add it to the next message, [T129]'s channel): is there a prescribed form for the milestone
self-assessment and learning plan, how often is it written (per semester, before each committee sitting), and who signs
it off? Do not invent the form.

Then, on the answer:

1. A seeded, unrated instrument (`counts_for []`, like `reflective_exercise_cpsa`) written by the trainee. It carries a
   self-rated milestone position per EPA, or free text if the College gives no structure, and a learning plan. A named
   supervisor records the discussion.
2. A slot for the latest one in the committee's evidence view ([T167]) and in the portfolio export ([T169]).
3. The seed rules apply: register it in `ActivityTypeSeedCatalogue` with a `WbaToolKey`, declare `validation` on every
   transition, update the five pinned seed-list tests deliberately, and add a vocabulary key if the College names it.

## Verification

- [ ] The College's answer, or the operator's decision to proceed without one, is recorded here.
- [ ] A trainee can file the instrument and a supervisor can record the discussion. Application tests, and a browser
      run on dev.
- [ ] It appears in the committee's evidence view for its period. Browser.
- [ ] Seed tests updated deliberately; the refresher is idempotent over two boots. Test, and the startup log.
- [ ] Full suite green, no `--no-build`.

## Related

[T120] (the precedent for seeding an unrated instrument), [T129] (the College message), [T131], [T167], [T169].
