---
id: T263
title: A server refusal on an activity form marks no field, only the encounter date
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T263 — A server refusal on an activity form marks no field, only the encounter date

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The refusal names the field in words, but the field itself is not marked.
**Surfaced:** 2026-09-25, the T236 review (finding 4).

## Symptom

When the server refuses an activity form (the tool gate, the nominee gate, required fields), `NewActivity.razor` shows
the refusal in its danger alert. Only the encounter date is ever marked (`ActivityForm`, when the page predicts the
refusal). No other input gets `aria-invalid` or `.input-validation-error`.

## What to build

Carry the refused field keys on the refusal (the validators already name fields), and mark those inputs with T236's
invalid style and `aria-invalid`, pointing `aria-describedby` at the refusal. Also on `ActivityView`'s edit and move
refusals.

## Verification

- [x] A refused required field and a refused nominee are marked. bUnit.

## Related

T236, T189, T193, T102.

---

## As built — 2026-09-25 (`3168233`)

A field the server refused is marked, not only named. The refusal carries the refused field keys, and those inputs get
T236's invalid style, `aria-invalid`, and `aria-describedby` pointing at the refusal. It covers `NewActivity`, and
`ActivityView`'s edit and move. A refused date is never also called fileable. bUnit.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`):
- A future date marked only `observed_on`.
- Activity 51's "Saved as a draft, but not submitted" marked only `presenting_problem`. The mark cleared when the field
  was filled.
- Switching the type cleared the marks.
- A refused assessor marked the Assessor select.
- The assessor and committee saw no marks.
