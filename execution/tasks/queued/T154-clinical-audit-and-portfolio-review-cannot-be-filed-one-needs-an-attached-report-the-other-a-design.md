---
id: T154
title: Clinical audit and portfolio review cannot be filed: one needs an attached report, the other a design
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T154 — Clinical audit and portfolio review cannot be filed: one needs an attached report, the other a design


> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Neither unlocks an EPA (every EPA already permits a seeded tool); both are unrated evidence that
credits nothing (D6, D7). What is missing is fidelity to the College's tool mix.
**Surfaced:** 2026-09-24, split out of [T120] on the operator's call ("Defer both").

## Symptom

Annexure A names **Clinical audit** on EPAs 1, 2, 3 and **Portfolio and logbook review** on EPA 15. Neither can be
filed: no seed exists. The vocabulary keys (`clinical_audit`, `portfolio_review`) exist and the allow-lists name them
(T122), so the day a seed lands it is already permitted on the right EPAs.

## Why each is blocked

- **Clinical audit** (page 8: *"a cycle of measurement against an agreed standard, followed by change and
  re-measurement"*) wants its report attached, and Wombat has no file storage: `FieldType.File` renders a placeholder.
  An attachment is personal data — subject-access export (`AccessReportBuilder`), erasure (`ErasureExecutor`), the
  portfolio PDF and the CSP all have to handle it. D34 recommends a text field holding a link as the interim.
- **Portfolio and logbook review** (EPA 15: *"teaching sessions, journal club presentations, feedback received, and
  reflective entries"*) reviews a body of work Wombat already holds. A form that asks the reviewer to re-type it is the
  wrong shape; it wants a review that names the trainee and a period and points at the existing portfolio export.

## What to build

Decide D34 (attachments or a link field) and the portfolio review's shape, then seed each on T120's checklist:
unrated, `"counts_for": []`, no `rated_level_field`, a `WbaToolKey`, honest `required` flags and declared
`validation` per transition (T105), a user field for the reviewer if one signs it off (T102).

## Verification

- [ ] Each seed is registered in `ActivityTypeSeedCatalogue` with its `WbaToolKey`, created on a fresh database, and a
  second boot republishes nothing.
- [ ] Filing each against an EPA whose list names it works; against one that does not, the EPA is not offered.
- [ ] Neither credits anything, and neither appears on an entrustment trajectory.

## Related

[T120] (the checklist), D6, D7, D34, T106 item 8 (file fields render a placeholder), [T122] (the allow-lists).
