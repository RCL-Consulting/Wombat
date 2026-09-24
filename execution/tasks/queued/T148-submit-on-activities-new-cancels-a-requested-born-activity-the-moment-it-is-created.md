---
id: T148
title: Submit on /activities/new cancels a requested-born activity the moment it is created
status: queued
priority: P2
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T148 — "Submit" on a legacy-shaped type files the encounter and immediately withdraws it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The trainee is told "Activity submitted." while the activity lands in `cancelled`. Nothing
errors and the encounter silently never reaches the assessor. Pre-existing at 256de22, not introduced by [T122].
**Surfaced:** 2026-09-23, by [T122]'s third review round.

## Symptom

For a type whose initial state is already `requested` (the generic `mini_cex`, `dops`, `cbd` and `acat` seeds),
Submit on `/activities/new` first creates the activity, which is correct and lands it in `requested`. It then
calls `ResolveInitialTransitionKey` (`NewActivity.razor`), which returns the first transition out of `requested`
that the trainee may take:
- `accept` and `decline` are `field:assessor_user_id`, so the trainee is denied them;
- `cancel` is `subject|field:assessor_user_id`, so the trainee is allowed it.

The page therefore sends `cancel`, and the activity moves to the dead end `cancelled`.

On dev the generic rated seeds are hidden from CPSA-pinned trainees by [T123] d3's ladder filter, so this bites
only where they are offered: a trainee on a curriculum pinned to the O-R Scale, or none.

## Root cause

`ResolveInitialTransitionKey` assumes the first transition the author may take out of the initial state is the
submission. For a type born in `requested`, the create WAS the submission, and the only author transition left is
the withdrawal.

## What to build

In `ResolveInitialTransitionKey`, skip transitions whose target cannot reach a terminal state
(`Workflow.CanReachTerminal`, added by [T122]). Return null when nothing is left, so the page reports the create as
the submission ("Submitted.") instead of sending a transition.

## Verification

- [ ] Submit on a legacy-shaped type leaves the activity in `requested` — `NewActivityCreationLockTests` or a new
      bUnit test
- [ ] Submit on a CPSA type still sends `submit` — existing tests
- [ ] Browser: file a generic Mini-CEX as a trainee who is offered it; the assessor sees it in their inbox

## Related

[T095] (introduced `ResolveInitialTransitionKey`), [T122], [T127].

## Update 2026-09-24 — EPA-stream survey

Lands with [T127], in the same change to `CreateOrTransitionAsync`, and closes with it. Use
`CanReachTerminal(to, avoidingState: initial state)`, not plain `CanReachTerminal(to)`. Otherwise a builder-made
"cancelled" state with a reopen transition would count as a submission.
