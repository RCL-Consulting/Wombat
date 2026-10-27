---
id: T386
title: After Try again, the former trainee's Home puts the focus on its card, not the h1
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-28
---

# T386 — After Try again, the former trainee's Home puts the focus on its card, not the h1

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A focus target after a retried load.
**Surfaced:** 2026-10-28, T379's states pass (`states/home--former-trainee-load-error.png`, board R3-H06).

## Why

The states agent: "After the retry the focus lands on the card's div ('Your training record…'), not the h1 the row says
(R3-H06). Everything else in that row holds."

## Symptom

On a former trainee's Home, a failed load's Try again, once the card draws, leaves the focus on the card, not on the h1.

## Root cause

Not yet read (`FormerTraineeRecordCard.razor` and Home's frame); inferred: the card takes the focus as its own result.

## What to build

The focus after Try again goes where DESIGN.md's rule for a page's retried load puts it (the h1, as R3-H06 draws), or the
row is corrected if the card is the rule; decide from DESIGN.md.

## Verification

- [ ] A bUnit test: Try again on the former trainee's Home puts the focus where the rule says.

## Related

T379 (flow 13, lane R), R3-H06, `states.md` row `home--former-trainee-load-error`.

## Notes
