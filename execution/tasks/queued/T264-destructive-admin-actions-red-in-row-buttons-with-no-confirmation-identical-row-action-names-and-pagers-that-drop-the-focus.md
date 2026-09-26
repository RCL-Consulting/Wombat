---
id: T264
title: Destructive admin actions: red in-row buttons with no confirmation, identical row-action names, and pagers that drop the focus
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T264 — Destructive admin actions: red in-row buttons with no confirmation, identical row-action names, and pagers that drop the focus

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is one click from a deactivation, a lock-out or a delete, and screen-reader users hear "Revoke,
Revoke, Revoke".
**Surfaced:** 2026-09-25, the T234 review (and its implementer's list).

## Symptom

- **No ConfirmDialog on a red button:**
  - Deactivate on the college, institution, speciality and sub-speciality pages;
  - Lock out, Remove and Revoke all on the user page;
  - Revoke on invitations;
  - Delete on SSO mappings;
  - Delete on the entrustment scales list;
  - the Revoke panel on entrustment decisions, which is a card, not a ConfirmDialog.
- **Identical row-action names with no `aria-label`:** Revoke, Delete, Withdraw, the user page's Remove, and the review
  page's Remove.
- **Pagers drop the focus on a page change:** `AuditList` (it nulls `_result`, which removes the pager),
  `RequestsList`, and `PagerControls` (Next and Previous disable themselves at either end).
- **Discard changes on the activity page** disables itself when pressed.

## What to build

Apply DESIGN.md's destructive-action pattern (T222, T206): a ConfirmDialog naming the target, a `btn-outline` in-row
trigger, and the focus moved to the result. Give each row action an `aria-label` naming its row. Keep pagers mounted,
and keep the focus on the pressed control or move it to the list heading. bUnit for each page.

## Verification

- [ ] Each listed action asks first and names its target, and each pager keeps the focus. bUnit.

## Related

T234, T222, T206, T239.

Note, 2026-09-25 (the H1 browser check): on `/admin/invitations`, after revoking the invitation just issued, the result
area still shows its "Share this registration link…" panel with the now-dead link, beside "Invitation revoked.".

Note, 2026-09-25 (the T253 review): the entrustment scales list's Delete also shows `exception.Message` rather than
`RefusalText.Of`.

Note, 2026-09-25 (the T258 review): approving an erasure request (`Admin/DataRights/RequestDetail.razor`) has no
ConfirmDialog. It should count the open reviews (including other institutions' and College panels') and campaigns it
will withdraw, and say it cannot be undone.

## Notes

- **T295 replay, 2026-09-26 (C19).** Note, 2026-09-26 (the T295 replay): confirmed at Step 6.8 (`design/baseline/act-6/6.8-1-delete-refused.png`). On `/admin/entrustment-scales`, pressing the CNSA row's red Delete sent `DeleteEntrustmentScaleCommand` at once, with no dialog (`EntrustmentScalesList.razor:61`, `:98-114`). It was refused with the in-use text, the scale kept its six levels, and one FAILED audit row was written at 12:35:30 UTC. At Step 6.2 the College page's Deactivate was seen as a `btn-danger` with no dialog (`CollegeEdit.razor:69`, `:173-190`); it was not pressed. `SpecialityEdit.razor:68` and `SubSpecialityEdit.razor:76` are the same. Nothing is new: every button is already in this task's list, and the T253 note covers the `exception.Message`. This does not widen the task.
- **T295 replay, 2026-09-26 (C22).** Note, 2026-09-26 (the T295 replay, Steps 2.6 and 2.7): the H1 note's leftover link also stays under a **refused issue**, and there it is another invitee's live link.
- **What the replay saw.** Mokoena's issue with no speciality was refused ('Speciality administrators must be scoped to a speciality.'). Van Rensburg's 'Share this registration link with the invitee' panel (token `PjVrn…`) stayed on screen under the refusal (`design/baseline/act-2/2.6-1-mokoena-refused.png`). At 2.7, Mokoena's link stayed under Sithole's refusal in the same way (`2.7-1-sithole-refused.png`).
- **The code.** In `InvitationsList.razor`, `IssueAsync` (`:285-347`) sets `_lastIssuedRegistrationUrl` only on success (`:333`) and never clears it first. `RevokeAsync` (`:402-428`) never clears it. `ResendAsync` does (`:365`). Clear it at the start of both, as `ResendAsync` does.
- **The revoke half** was not re-tested at 2.17, because the page had reloaded since the issue.

This widens the task by the refused-issue half. It is the one item here that can hand a live link to the wrong person: the register page would show the other invitee's email and role, and completing it would create that person's account. So take it first. bUnit: after a refused issue and after a revoke, no link panel is shown.
- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (states/college-edit--deactivated.png, speciality-edit--deactivated.png, sub-speciality-edit--deactivated.png, institution-edit--deactivated.png).** Confirmed on all four pages, and widened by one: after the deactivation, the red Deactivate is still offered on the now-inactive record, beside Cancel and Save. CollegeEdit.razor:67-70, InstitutionEdit.razor:67-70, SpecialityEdit.razor:66-69 and SubSpecialityEdit.razor:74-77 render it whenever `!IsNew`, whatever the stored state. Follow EpaEdit (T196): offer Deactivate only on a record stored as active (EpaEdit.razor:92-96), behind a ConfirmDialog that names it (:106-112), and remove it once the deactivation is done (states/epa-edit--local-deactivated.png shows the model). Scale Delete with no dialog was seen again (entrustment-scales-list--deleted.png); it is already listed. bUnit: after a confirmed deactivation, each of the four pages renders no Deactivate. Once the dialog exists, states.md rows 490, 503, 516 and 593 need a 'confirm' step.
