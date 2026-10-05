# ReminderAction

Send a reminder (`Components/Shared/Programme/ReminderAction.razor`): at the tail of a waiting row, the last reminder's record and either a small outline button that emails the row's assessor about that one request, behind a confirmation, or why no reminder can be sent. Flow 06 (T358, 2026-10-05, 85d5a508; C4; round 3 items 22, 23, 29; R2-Waiting w1–w8, R2-Registrar r2, r2d, r2r, r2x; E1, E3) made it. It moves nothing: the request keeps its state and its wait, and its registrar is not told.

## What the consumer provides

```razor
<ReminderAction Item="item" ActingRole="@_role" LinkName="@NameOf(item)"
                OnResult="ShowResultAsync" OnFailed="ShowFailureAsync" />
```

- `Item` (required): the waiting row (`ActivitySummaryDto`): its nominee, its last reminder, whether one went today, and whether one may be sent.
- `ActingRole` (required): the role the page reads as (`ProgrammeReadAs.RoleFor`), which the command is sent as (E4).
- `LinkName`: the row link's own accessible name where two rows read the same (`ActivityRowNames.Waiting`), so the button says "(1 of 2)" as the link does (round 3 item 29).
- `OnResult`: the answer, sent or refused, handed over once the dialog has closed; the host shows it in its own `ActionResult`, which takes the focus.
- `OnFailed`: the send failed (not a refusal); the host shows `ReminderWords.Failed`.

The host draws it only for a role that may remind (`MayRemind`: both speciality admins and the Coordinator, never the Committee member). Waiting for assessors puts it in the Waiting cell under "8 days" and "since … SAST"; the registrar page in WaitingList's row, through `RowAction`. Home sends none.

## Markup and words (`ReminderWords`)

In order, each on its own line (`span.needs-you-why.progress-row-meta`, muted, as the Waiting cell's spans):

- **The last reminder**, when there is one: "Reminded 2026-10-05 by Pieter Smit".
- Then one of:
  - **Why not** (E3's w9), in place of the button: "No reminder: Fatima Khumalo's account is deactivated.", "No reminder: Mohammed Patel has no email address in Wombat.", "No reminder: the person this request names has no account.".
  - **Nothing more**, when someone has reminded the assessor today already: one per request per South African day, for every member of staff alike.
  - **The button**, `button.btn.btn-outline.btn-sm.reminder-action` "Send a reminder" (28px; 4px under the lines above it), named "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-004 · 2026-10-02, from Nomsa Mahlangu". Below 641px it is 44px and the row's width.

## The dialog (ConfirmDialog)

"Send Thandi Zulu a reminder?" · "Thandi Zulu gets one email, "Activities awaiting your assessment", listing this request: Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days. It moves nothing: the request stays Requested, its wait is not restarted, and Nomsa Mahlangu is not told." It opens on its safe button, **Don't send**; the confirm is **Send the reminder**. While the send is on its way the confirm reads "Sending…" and is `aria-disabled`, keeping the focus, and Don't send is disabled (ConfirmDialog's in-flight rule); a second press sends nothing. The dialog closes before the host is answered, since nothing outside an open modal can take the focus.

## The answers (shown by the host, `ActionResult`, focused)

| Outcome | Alert |
|---|---|
| Sent | success, `role="status"`: "**Reminder sent to Thandi Zulu.** It lists Mini-CEX (Paediatrics) · PAED-004 · 2026-10-02, from Nomsa Mahlangu, waiting 8 days. The request is still Requested; its wait is unchanged." |
| Reminded today | danger, `role="alert"`: "**Not sent.** Pieter Smit reminded Thandi Zulu today already." |
| Deactivated | "**Not sent.** Fatima Khumalo's account is deactivated, so Wombat sends Fatima Khumalo no email. The request still waits." |
| No email | "**Not sent.** Mohammed Patel has no email address in Wombat. Ask your institutional admin to add one." |
| No account | "**Not sent.** Wombat has no account for the person this request names; it was erased or deleted, so there is nobody to email. The request still waits." |
| Moved meanwhile | "**Not sent.** Case-Based Discussion (Paediatrics) · PAED-002 · 2026-09-30 moved at 2026-10-05 16:12 SAST: it is now Completed and waits for nobody." ("…: it is now Awaiting review, with Mohammed Patel." while it still waits) |
| Off the list | "**Not sent.** This request is no longer on your list." |
| Failed | danger: "Not sent. Something went wrong, and no reminder was sent. Try again, or come back in a few minutes." (the exception goes to the log, T329) |

After an answer the host reads its list again: a sent reminder's row gives way to "Reminded … by …", a moved request leaves the list, and the heading is recounted.

## Rules (DESIGN.md § Page-level patterns, "List page"; § Dashboard page, "The oversight Homes")

- **A reminder is one email about one request** (`AssessorPendingNudgeEmail` for one row, its own tag), sent to the nominee alone; the registrar is never told. The weekly nudge after `AssessorNudgeDays` (5) is separate.
- **It moves nothing**, says so before it is pressed, and says so again after.
- **One a day per request** (`ActivityReminders`: one row per request per South African day, a unique index; a race is caught and answered as Reminded today). The sender and the assessor are pseudonymised on erasure.
- **The request it was read with** is sent with it (its state and last move): a request that moved since is refused, never reminded about in words it was not asked in.
- No pronoun for a person in any of its words: names, "the assessor", "the person this request names".

## Contrast

The button `secondary-color` 4.86:1 on the surface; the lines `muted-text` 5.09:1; the answers' words on their tints 11.23 (success) and 11.81:1 (danger).

## Known gaps

- No Reassign (T359): a refused reminder leaves the request where it is.
