---
id: T236
title: No field shows an invalid state: app.css never styles Blazor's .invalid class or aria-invalid inputs
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T236 — No field shows an invalid state: app.css never styles Blazor's .invalid class or aria-invalid inputs

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The validation message shows, but the input itself does not change.
**Surfaced:** 2026-09-25, the T212 review (finding 2). App-wide.

## Symptom

Nothing in `app.css` styles Blazor's `.invalid` class or `[aria-invalid="true"]`, and the project has no field CSS class
provider. On every EditForm an invalid input looks exactly like a valid one, for example the agenda deferral reason.

## What to build

Style `.form-control.invalid`, `.form-select.invalid` and `[aria-invalid="true"]` with the existing
`.input-validation-error` tokens (a border in the danger token, and not colour alone: a thicker border or an icon, so it
passes WCAG 1.4.1). Record it in DESIGN.md § Forms.

## Verification

- [x] An invalid input on the deferral form and on one activity form shows the invalid style. A CSS test in the style of
      `TableColumnClassTests`, and browser.

## Related

T212, T193.

---

## As built — 2026-09-25 (`3b5e15a`)

An invalid input shows it, not only its message. Blazor's `.invalid` and `[aria-invalid="true"]` on `.form-control`,
`.form-select` and textarea take the danger token's border and a 3px inset left stripe. It is paint only: text does not
move. In forced colours it becomes a 4px left border. Recorded in DESIGN.md § Forms. A CSS test pins the rule.

Browser on dev (scripted Chrome, master `675a53d`):
- **The deferral form, left empty** (review 10, as the chair): `form-control invalid`, `aria-invalid=true`, border
  rgb(231,76,60), inset stripe, and the message shown. The focus ring sits outside the stripe. Typing a reason cleared
  all of it.
- **The activity form:** a date before the programme start gets the stripe and the refusal hint. A late date gets only
  the warning.
- **The staging form's** empty level select shows the stripe too.

**Filed from the review:** [T263] (a server refusal marks no field).
