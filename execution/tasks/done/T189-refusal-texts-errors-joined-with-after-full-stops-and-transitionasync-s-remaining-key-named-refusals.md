---
id: T189
title: Refusal texts: errors joined with '; ' after full stops, and TransitionAsync's remaining key-named refusals
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T189 — Refusal texts: errors joined with '; ' after full stops, and TransitionAsync's remaining key-named refusals

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The wording is untidy, but every refusal is correct.
**Surfaced:** 2026-09-24, the T172 review.

## Symptom

- Every `SchemaValidator` message ends in ".", and `ThrowIfInvalid` joins them with "; ", so a refusal reads
  "A.; B.".
- `TransitionAsync` still names by key: the `MergeWritableKeys` refusal (reached only by a hand-made request), "requires
  a note", and the state-key text "Transition 'x' is not available from state 'y'".

## What to build

Join with " ". Name fields and states by label through `FormSchema.FieldLabel` and the workflow's state labels. Update
the exact-match tests deliberately.

## Verification

- [x] A two-error refusal reads as two sentences. Test.
- [x] The transition refusals name labels. Tests.

## Related

T172.

---

## As built — 2026-09-25 (`7e52344`)

Validation errors join with a space, and every message ends with a full stop. `TransitionAsync`'s remaining refusals
name fields and states by label.

Browser on dev (scripted Chrome, `2561f07`):  Activity 37 reads "Saved as a draft, but not submitted: Presenting problem: A value is required. Case complexity: A
value is required. Fix the fields below and submit again."

**Filed:** [T220] (pages still show state and move keys, the display half of the review's finding).
