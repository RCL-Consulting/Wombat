---
id: T189
title: Refusal texts: errors joined with '; ' after full stops, and TransitionAsync's remaining key-named refusals
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

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

- [ ] A two-error refusal reads as two sentences. Test.
- [ ] The transition refusals name labels. Tests.

## Related

T172.
