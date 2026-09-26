---
id: T318
title: The data-rights page offers an opt-out of 'optional processing' that nothing reads, and leaves an earlier 'saved' status beside a later refusal
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T318 — The data-rights page offers an opt-out of 'optional processing' that nothing reads, and leaves an earlier 'saved' status beside a later refusal

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Wombat does no optional processing, so the opt-out stops nothing, and nothing is processed against anyone's wishes. Still, a data-rights page should not offer a choice that has no effect. The stale status is a copy fault.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-A.1.1a, F-A.1.2b).

## Symptom

- **Step A.1.1** (design/baseline/states/data-rights--saved.png): ticking "Opt out of optional processing (e.g. analytics)" and saving reads "Processing preferences saved." and sets AspNetUsers.OptOutOfOptionalProcessing to true. Nothing in the product behaves differently, because Wombat does no analytics and no code reads the flag (F-A.1.1a).
- **Step A.1.2** (design/baseline/states/data-rights--reason-missing.png): after saving preferences, submitting a request with no reason shows the alert "Please provide a reason for your request." next to the success status "Processing preferences saved." from the earlier action (F-A.1.2b).

## Root cause

- OptOutOfOptionalProcessing (WombatIdentityUser.cs:19) is read and written only by GetObjectionFlags, UpdateObjectionFlags and ObjectionFlagService (ObjectionFlagService.cs:26, :38). DataRights.razor:34-39 shows it, and ErasureExecutor.cs:362 sets it. No processing checks it, and a grep of src finds no analytics or telemetry for it to stop. The digest opt-out beside it is honoured (T240).
- DataRights.razor:244-247: SubmitRequestAsync's missing-reason branch sets _errorMessage and returns before lines 251-252 clear _statusMessage. The previous action's status therefore stays in the same ActionResult (lines 13-22).

## What to build

- Remove the optional-processing preference: the checkbox; the flag in GetObjectionFlags, UpdateObjectionFlags and ObjectionFlagService; the column (a migration); and ErasureExecutor's write. Reword the card's paragraph to say what can be opted out of (the three digest reminders) and that processing required for the programme cannot be objected to. A later feature that adds optional processing brings its own opt-out, with a reader.
- One result per action. Every branch of SaveFlagsAsync, SubmitRequestAsync and WithdrawAsync clears the other message before setting its own, or a single result field replaces the two. RequestDetail.razor's 'A decision note is required.' branches (lines 126-130, 163-167) follow the same rule.

## Verification

- [ ] bUnit (alongside DataRightsDigestOptOutTests): the page offers no optional-processing box, and UpdateObjectionFlagsCommand carries only the digest flag.
- [ ] The migration drops the column, and the ErasureExecutor Postgres tests still pass.
- [ ] bUnit: save preferences, then submit with no reason; only the refusal shows.
- [ ] Browser check, runbook Steps A.1.1 and A.1.2, with A.1.1's Note about the unread flag removed and its Expect describing the one remaining preference.

## Related

T240 (the digest opt-out), T234 (ActionResult and focus), T258 (erasure); runbook Steps A.1.1, A.1.2; F-A.1.1a, F-A.1.2b.
