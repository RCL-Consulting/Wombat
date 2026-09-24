---
id: T172
title: A refusal names the form's fields by key (presenting_problem), not by the label the user sees
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T172 — A refusal names the form's fields by key (presenting_problem), not by the label the user sees

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Every refusal is correct; its wording is not what the user sees on the form.
**Surfaced:** 2026-09-24, the T127 browser check. Activity 20's refusal read "presenting_problem: A value is required",
while the form labels the field "Presenting problem".

## Symptom

`SchemaValidator` reports errors keyed by field key, and `ActivityService.ThrowIfInvalid` joins them as
`key: message`. Every page shows that text as it is: `ActivityView`'s action error, T127's "Saved as a draft, but not
submitted" notice, and `/activities/new`'s create refusal. T107's disabled-action reasons already use labels, so the
two surfaces now disagree.

## What to build

Name the field by its schema label, falling back to the key, where the refusal is composed (the validator result
carries the key; the schema is at hand in `ThrowIfInvalid`'s callers). Keep the key available for tests and logs if
anything matches on it. Grep the tests that assert on the `key: message` shape and update them deliberately.

## Verification

- [x] A refused submit names "Presenting problem", not `presenting_problem`. Application test and a browser check.
- [x] A field with no label still shows its key.

## Related

[T127] (the notice that carries the refusal), [T107] (reasons already by label).

---

## As built — 2026-09-24

`FormSchema.FieldLabel` names a field by its label, falling back to its key. Where another field has the same label,
it adds the section title in brackets. The refusal composer, T107's reasons and both gates share it.

Browser on dev, as the trainee:
- Activity 25, a Mini-CEX missing Presenting problem: "Saved as a draft, but not submitted: Presenting problem: A value
  is required. Fix the fields below and submit again."
- Activity 26, a QI Project missing PDSA cycle 1's Plan: "… Plan (PDSA cycle 1): A value is required. …"

Left for a follow-up: the "; " joiner after messages that end in ".", and the key-named refusals left in
`TransitionAsync`.
