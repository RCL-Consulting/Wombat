---
id: T136
title: "Changing a curriculum item's scale fails silently when its ordinals do not fit"
status: done
priority: P2
owner: agent
created: 2026-09-20
started: 2026-09-24
completed: 2026-09-24
---

# T136 — The save button does nothing, and says nothing

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Nothing is corrupted — the refusal is correct — but the administrator is given
no reason and no clue, and the row simply stays as it was. An admin will conclude the page is broken.
**Surfaced:** 2026-09-20, browser-verifying [T126] on dev. I needed to re-pin PAED-001 from the CPSA
ladder to the O-R Scale and spent three attempts working out why nothing happened.

## Symptom

On `/admin/curricula/{id}/items`, edit a row, change **Scale** to a ladder with fewer rungs, click
**Save**. The row closes as though it saved. Reload: it is unchanged. **No error, no validation
message, no toast.** Repeating it produces the same silence.

Observed on curriculum 3, PAED-001, changing *CPSA Paediatric Entrustment Scale v11.1* (six rungs) to
*O-R Scale* (five rungs), with `MinimumLevelOrder` 6 and per-stage minima `{"1":3,"2":4,"3":5,"4":6}`.

## Root cause — inferred, not yet confirmed in code

Almost certainly T109's guard: an ordinal only means something on a named ladder, so a minimum of 6,
and per-stage minima containing 5 and 6, cannot be carried onto a five-rung scale. **The refusal is
right.** What is missing is telling anyone.

Confirmed by working around it: the save succeeds the moment **all three** are changed together —
scale to O-R, `MinimumLevelOrder` to 4, and per-stage minima to `{"1":2,"2":3,"3":3,"4":4}`. So the
validation is on the combination, and every earlier attempt was being rejected wholesale.

**Verify this before fixing it.** The alternative explanation is that the save is silently throwing
and being swallowed, which would be a different and worse defect. Read
`ManageCurriculumItems`'s save path and find where the failure goes.

## What to build

1. Surface the refusal. The page already has a `_actionError`-style pattern in the activity-type
   editor; use whatever this page's equivalent is rather than inventing one.
2. Say which value is the problem: *"Minimum level 6 is not a rung on the O-R Scale, which has 5."*
   A generic "could not save" would leave the admin exactly as stuck.
3. Consider offering the fix: when the scale changes, either clamp the ordinals and say so, or block
   the scale change until the ordinals are valid. **Do not silently clamp** — that is how an ordinal
   comes to mean something nobody chose, which is what T109 exists to prevent.

## Verification

- [ ] Changing a scale with incompatible ordinals shows a message naming the offending value —
      checked in the browser
- [ ] Changing scale and ordinals together still succeeds — checked in the browser, and by a handler
      test since that is the path that works today and nothing guards it
- [ ] The refusal itself is unchanged — a test asserting the command still rejects the bad
      combination, so fixing the message cannot accidentally loosen the rule
- [ ] Full suite green, no `--no-build`

## Related

T109 (ordinals pinned to a scale) is why the refusal exists. Found while verifying [T126].

## Notes

- **Observed:** three consecutive silent failures, then success when all three fields were changed
  together. That is the whole evidence; the cause is inferred from it and from T109's rule.
- **Not checked:** whether the same silence affects other admin grids that edit an ordinal beside a
  scale. Worth a look while in there.

## Update 2026-09-24 — EPA-stream survey

Folded into [T125], which covers the same page and handler. Its message wording and tests are listed in T125's update,
and this task closes when T125 lands.

**Closed 2026-09-24 with [T125]**, which records the as-built. The refusal names the scale, the field and the value, and
shows beside the row; the Update path is covered by handler tests.
