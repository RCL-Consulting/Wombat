# T127 — Every failed Submit on /activities/new leaves a half-filled draft behind, and the next attempt makes another

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
