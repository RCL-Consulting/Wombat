---
id: T193
title: Refusal alerts are not announced, and field help text is not linked to its input
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T193 — Refusal alerts are not announced, and field help text is not linked to its input

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Accessibility: a screen reader may miss a refusal, and it never reads a field's help text.
**Surfaced:** 2026-09-24, the T160 browser check.

## Symptom

- `Alert.razor` renders `<div class="alert alert-danger">` with no `role="alert"` (or a live region), so a refusal
  that appears after Submit may not be announced. T125 gave `Alert` an optional `Role`; nothing sets it for errors.
- In `ActivityForm`, a field's help text ("When the encounter happened, not when this form is completed.", the Assessor
  hint) is not referenced by the input's `aria-describedby`. On the date field, `aria-describedby` names only T160's
  lateness region.

## What to build

Danger alerts default to `role="alert"`, and warning alerts to `role="status"`, unless the caller overrides. Every
field's help text gets an id, and the input's `aria-describedby` lists the help and any warning region.

## Verification

- [x] A danger `Alert` renders with `role="alert"`. bUnit test.
- [x] Every ActivityForm input with help text references it. bUnit test over the seed schemas.

## Related

T160, T177, T125.

## Note, 2026-09-24 (T158 review)

The root of the help-text half: `FormField.razor:13-16` renders `HelpText` with no id, so no page can reference it.
Give `FormField` an id for its help, and have `FormField` set the input's `aria-describedby` itself where it can. T158
fixed EpaEdit only locally.

Note, 2026-09-25: the sign-in password input lacks `autocomplete="current-password"`.

---

## As built — 2026-09-25 (`2561f07`)

- **Alert roles:** a danger alert is `role="alert"`, and warning and success alerts are `role="status"`. A refused-submit
  notice on ActivityView is an alert.
- **Help text:** `FieldHelp` gives each field's help an id. The input names its help, then any notice region. 21 help
  texts on other pages were linked too, and a scan test fails on any unlinked help written into a FormField.
- **Sign-in:** the fields carry `autocomplete="username"` and `"current-password"`.

Browser on dev (scripted Chrome, `2561f07`): 
- Both refusals are `role="alert"`, and the success notice is `role="status"`.
- "Date observed" has `aria-describedby="observed_on-help observed_on-filing-notice"`, both present.
- The sign-in autocomplete attributes are present.
