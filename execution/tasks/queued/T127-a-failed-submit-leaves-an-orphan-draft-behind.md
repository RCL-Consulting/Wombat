---
id: T127
title: "Every failed Submit on /activities/new leaves a half-filled draft behind, and the next attempt makes another"
status: queued
priority: P2
created: 2026-09-19
---
# T127 — Every failed Submit on /activities/new leaves a half-filled draft behind, and the next attempt makes another

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Status:** open
**Surfaced:** 2026-09-19, browser-verifying [T100] / [T123] on dev. Hit by accident on the first attempt,
which is the point.
**Severity:** Medium. No data is lost or corrupted, but a registrar accumulates junk in their own activity
list by doing nothing wrong, and the junk is indistinguishable from a draft they meant to keep.

## What happens

`NewActivity.razor`'s `CreateOrTransitionAsync` sends `CreateActivityCommand` **unconditionally**, then —
only if `submit: true` — sends `TransitionActivityCommand`:

```csharp
var draft = await Sender.Send(new CreateActivityCommand(...));   // persisted here
if (submit)
{
    ...
    await Sender.Send(new TransitionActivityCommand(draft.Id, transitionKey, ...));  // may throw
    _status = "Activity submitted.";
}
```

The transition validates the whole schema in Submit mode. When it throws, the `catch` sets `_error` and
returns — and `draft` goes out of scope. **The activity is already in the database.** The page holds no
reference to it, so clicking Submit again runs `CreateActivityCommand` a second time and creates a second
activity.

Observed on dev, filing one Mini-CEX:

| Id | State | Created | Data |
|---|---|---|---|
| 13 | `draft` | 17:19:00 | `epa_id`, `setting`, `complexity`, `assessor_user_id` — the fields that had bound |
| 14 | `completed` | 17:19:11 | the full, correct submission |

Eleven seconds apart. Activity 13 is the orphan; nothing references it and nothing will ever finish it.

## Why it is easy to hit, not a corner case

The failure that produced it was the ordinary one: two required fields not yet filled. And the validation
that rejects it is the one [T105] exists to fix — `ActivityService` validates the **whole schema in Submit
mode on every transition**, so a submit that is one field short fails after the create has already
committed.

The two compound. Until [T105] lands, any transition from a partially-filled form fails this way, and each
retry mints another orphan.

## Options

1. **Create only on success.** Wrap create+transition so a failed transition rolls the create back. Needs
   the two commands in one transaction, or a compensating delete. Cleanest end state.
2. **Remember the created id and reuse it.** Keep `draft.Id` on the component; on a retry, patch and
   transition the existing activity instead of creating a new one. Smaller change, and it also fixes
   "Save draft" clicked twice, which creates two drafts today for the same reason.
3. **Leave the draft and tell the user.** `_error` becomes "Saved as a draft — fix the fields below and
   submit." Honest, one line, and it turns the orphan into a feature. Does not fix the duplicate on retry.

**Recommendation: 2, then 3's message.** Remembering the id fixes both the orphan and the duplicate, and it
is contained in the page. Option 1 is the better end state but it needs a transaction boundary across two
MediatR commands, which nothing in this repository does yet and which is a bigger decision than the defect
warrants. The message matters either way: right now a failed submit says nothing about the fact that the
activity exists.

⚠️ Whatever is chosen, **check `Save draft` on the same path** — it has the same shape and the same bug.

## Verification

- Submit with a required field missing, fix it, submit again: **one** activity exists, not two.
- Click Save draft twice: one draft.
- The error message says whether anything was saved.
- A successful first-time submit is unchanged.

## Cleanup owed on dev

Activity **13** is the orphan this investigation created. It is scenario data and harmless, but the next
session should not mistake it for evidence. Discard it through `/activities/13` or leave it as the
reproduction.

## Related

Compounds with [T105] (whole-schema Submit-mode validation on every transition), which is what makes the
failing path common. Found while browser-verifying [T100] and [T123]; unrelated to either.

> **Note from T102, 2026-09-24.** There is no update command any more: `UpdateActivityDraftCommand` was deleted. A Submit
> retry is `TransitionActivityCommand` with the form as `DataPatchJson`, which already runs the writable-key filter, fix 1
> and both gates, and must not re-create the activity. A second "Save draft" needs either no data write or a guarded one
> (see T106 item 1). NewActivity now ignores a second click while the first is in flight, which stops the double-click
> duplicate but not a deliberate second save.

> **Note from T105, 2026-09-24.** A half-filled draft can now be cancelled (`cancel` validates `draft`), so an orphan
> left by a refused Submit is at least disposable by its author. The duplicate itself is still this task's.

## Update 2026-09-24 — EPA-stream survey

**[T143] and [T148] land in the same change**, to `NewActivity.razor`'s `CreateOrTransitionAsync`, and both close with this
task. Three sequential edits to the same seventy lines would be worse than one.

**The approach: the survey's recommendation, replacing option 2 above.**

- **After a successful create, navigate to `/activities/{id}`.** This closes T143, because the filled form is gone.
  Any further action happens on `ActivityView`, which already sends edits as a patched transition.
- **If the submit is refused after the draft saved, navigate to the draft** and show the refusal with the message "Saved
  as a draft", so it is clear the activity exists. No second create is possible from there.
- **A second "Save draft" becomes moot**, because the page has navigated after the first. No `save` self-transition is
  added; [T106] item 1 stays out of this task.
- **Create-and-transition in one transaction (option 1) stays rejected.** `TransitionAsync` loads a saved activity
  (`ActivityService.cs:205`, `:600-609`), so it is the larger change for no gain here.
- **T148, same method:** `ResolveInitialTransitionKey` skips any transition whose target cannot reach a terminal state
  without passing back through the initial state (`CanReachTerminal(to, avoidingState: initial)`, mirroring the T122
  gate's withdrawal rule). When only withdrawals remain, it reports the create as the submission.
- Carry the outcome message across the navigation (a query flag, or `ActivityView` stating the state).

Changed since this was filed (observed at `431e69e`):

- The [T122] tool gate and the [T102] nominee gate now run before the activity is added (`ActivityService.cs:67-93`),
  so their refusals no longer leave an orphan.
- What still leaves one is the submit's `owned` validation. `mini_cex_cpsa` has six trainee-owned required fields.
- The `_busy` guard stops a double-click, not a deliberate second press.
- "Cleanup owed on dev" is stale. Activity 13 on today's dev database is a T122 staging leftover (STATE.md), not this
  orphan.

**Verification, added:**

- [ ] After a successful Submit or Save draft the page is on `/activities/{id}`. bUnit test with a FakeSender that
      records commands.
- [ ] A refused Submit sends one `CreateActivityCommand`, lands on the draft showing the refusal and the "saved as a
      draft" message, and nothing on that page can create again. bUnit test.
- [ ] T148: Submit on a requested-born workflow sends no `TransitionActivityCommand` and reports "Submitted". On a
      draft-born CPSA workflow it sends `submit`. bUnit tests.
- [ ] Browser, on dev: a CPSA Mini-CEX with one trainee-owned required field blank leaves one activity. A generic
      Mini-CEX filed by a trainee on an O-R curriculum stays in `requested` and reaches the assessor's inbox.
- [ ] Full suite green, no `--no-build`.
