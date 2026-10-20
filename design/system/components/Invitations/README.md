# Invitations

Invitations, the invitations list (`/admin/invitations`, `Pages/Admin/Invitations/InvitationsList.razor`; its words in `InvitationText.cs`, its addresses in `Shared/OnboardingLinks.cs`): **every invitation not yet used, with what became of its email**, and the last 30 days' invitations that passed their last day unused. Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-L01 to L17; round 3 Q9, Q15, Q18, F2, F7, M5, M7, S16; T234, T239, T264, T283, T325, T329; build review P2, P4, P11, R3) rebuilt it. Until then the issue form sat above the list in a details grid, the table had ten columns and scrolled sideways at 1280 (T323), Revoke was a red row button with no confirmation (T264), the link a revoke had just killed stayed on screen, Delivery was a sentence in a cell, and a failed read printed the exception's text.

## Who reads it

Administrator and Institutional admin (`AdministratorOrInstitutionalAdmin`): the Administrator every institution's and College's, the Institutional admin her institution's. The menu's "Invitations" (`mail`), under People for both; it owns Invite a person (NavMenu). Someone who also holds Trainee issues, resends and revokes nothing (T290): the page reads nothing for them and shows one info alert, `#invitations-trainee-note`, "You hold the Trainee role, so you cannot view or change user accounts, including your own.", with no header action (M1, L17).

## The page

- **The header**: "Invitations", the subtitle "Invitations not yet used, with the state of each one's email.", and its one action **Invite a person** (primary, `user-plus`, to `OnboardingLinks.InviteAPerson`), drawn once the list has loaded: none while it loads, after a failed read, or with the T290 note (L15, L16, L17). Below 641px it takes the row, 44px (PageHeader).
- **The column** (`.inv-page`): its parts 24px apart, each with no margin of its own.
- **The result** (`ActionResult`, `#invitations-result`; `.inv-page .action-result:empty` takes no row): a resend's OneTimeLink (OneTimeLink), a revoke's success Alert, or a refusal. It takes the focus once the action has answered (T234). **The link is cleared at the start of every action** on the page, and on Invite a person and Invite again (M5): no dead link stays on screen beside "Invitation revoked." (T264's note).
- **Active invitations** (`section.list-section`): `h2.list-section-title` "Active invitations" with its count, `span.inv-count` "4 active" (muted, 0.9rem/400, not a badge), over a `DataTable` with `StackWide` (S16; DataTable), captioned for the Institutional admin "At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used." and for the Administrator "Every institution and College. Not delivered first, then newest first. A used or revoked invitation leaves this list." (L03, L04). A row with no institution's name drops the place, never "At ." (P11).

| Column | What |
|---|---|
| Invitee | **the row's header**: the address as stored, with `<wbr>` after its @, and no `.col-wrap`, so it breaks only there (check rule 1) |
| Role and scope | `.person-cell`: the role's label over its scope, "Trainee" over "Paediatrics / Paediatrics"; "Any speciality" for a Coordinator or Committee member kept to none (Q18); for the Administrator the place too, "Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital", or the College for a College admin |
| Issued | `.inv-when`: the South African date over its time, "2026-10-20" over "11:21 SAST"; after a resend the new link's, with nothing saying "Resent" (F2, T325) |
| Last day | `.inv-when`: the last South African calendar day the link works, "2026-11-03" (F7) |
| Delivery | `.col-wrap` (a 7.5rem floor, `.inv-page .clinic-table th.col-wrap`, so "delivered" and "address" stay whole from 901 to 1100px): a state word with its icon, **no badge** (Q15): Being sent (`clock`, muted), Sent (`circle-check` in `success-color`), Not delivered and Check the address (`circle-alert` in `warning-color`, 600) (`.delivery`, `--sending`, `--sent`, `--warn`; `#invitation-delivery-{id}`) |
| (Actions) | `.col-actions`: Resend (`refresh-cw`) before Revoke… on a row not delivered; Revoke… alone on any other; both `.btn .btn-sm .btn-outline` |

- **Not delivered first**, newest first among them, then the rest newest first (`ListActiveInvitations`, after `Invitation.DeliveryOf`: Being sent for an hour, then Not delivered).
- **The note row** (L09, L12; DataTable's row line): a row not delivered is `tr.has-note`, and a second `<tr>` from the same `Row` fragment holds `td.history-note.history-note--warning` across the six columns, its `p#invitation-note-{id}` on the warning tint with a 4px `warning-color` stripe (0.9rem, `text-color`): "Resend emails a new link in place of the current one, which then stops working.", led from the second failed email by "Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address." (Delivery then reads "Check the address"). The row's Resend names the note with `aria-describedby`.
- **Row actions are named by the row** (T239): "Resend the Trainee invitation to duplessis@kgk.wombat.local", "Revoke the Trainee invitation to ndlvou@kgk.wombat.local"; where two would read the same, `RowNames.Distinct` adds ", issued 2026-10-20 11:21 SAST", then the invitation's number.
- **Expired, not used** (Q9, L14), only when it has rows: `h2` "Expired, not used" with "1 expired", a second `StackWide` table captioned "Past their last day unused, in the last 30 days on the South African calendar, newest first. Each leaves this list 30 days after its last day, or once a new invitation goes to its address.", the columns Invitee, Role and scope, Issued, Last day, and **Invite again** (`user-plus`, a link to `OnboardingLinks.InviteFrom(id)`, named "Invite again expired@kgk.wombat.local as Trainee, Paediatrics / Paediatrics"). A row leaves once a new invitation goes to its address or an account holds it, the two compared normalised (S4).

## Revoke… (ConfirmDialog, DangerAction; T264)

"Revoke this invitation?" · "Revoke the Trainee invitation to **ndlvou@kgk.wombat.local** (Paediatrics / Paediatrics)?", then a `ul.dialog-list`: "The registration link already emailed to that address stops working at once: nobody can register from it." and "The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation.", then "Revoking cannot be undone." · **Keep the invitation** · Revoke invitation. Keep the invitation and Escape give the focus back to Revoke… (ConfirmDialog). Below 641px its buttons stack, 44px and the row's width, Revoke invitation on top (M8). The result, focused: "**Invitation revoked.** The link sent to ndlvou@kgk.wombat.local no longer works: nobody can register from it.", with, in the alert's row (`.alert-row`), **Invite another address as Trainee, Paediatrics / Paediatrics** (`user-plus`, to `OnboardingLinks.InviteFrom(id)`), which Invite a person fills from it (InviteAPerson). The list is read again: "4 active".

## Resend (T283)

On a row not delivered. Its result is the OneTimeLink: "**A new invitation link is being emailed to duplessis@kgk.wombat.local.** The link it replaces no longer works. Copy the new link below — it is shown only once.", the link and Copy link, and "It works until 2026-11-03. It is shown here until your next action, then never again." The row then reads Issued at the resend's time, "Being sent", no note row, only Revoke…. A refusal is the command's own words (`InvitationIssueRefusedException`), anything else "That could not be done just now. Nothing has changed. Try again, or come back in a few minutes.", logged. The list is read again after every resend, refused or not; a refusal takes the focus once its row no longer offers Resend. **While a resend runs** the pressed Resend keeps the focus with `aria-disabled` (a second press sends nothing, T234), and every other row's Resend and every row's Revoke… are natively `disabled`: the race rule of § Button system, since the rows are available again the moment the resend answers (build review R3).

## States (L01, L02, L15, L16, T329)

- **None**: "Active invitations" over the empty card, "No active invitations" · "Issue an invitation to create the first onboarding link.", and Invite a person in the header.
- **Loading**: `p.loading-line` (`role="status"`) "Loading the invitations." over a skeleton card (`div.skeleton.skel-title`, two `.skel-line`, one `.skel-line.skel-short`), no header action.
- **Load failed**: "Could not load the invitations. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), never the exception's text; a Try again that loads hands the focus to Active invitations' heading (P2).

## Narrow widths

**Stacked at 900px and below** (`StackWide`): each row a block, the address heading its labelled cells, the note row joining its block; every row button is 44px from 900px (`.clinic-table--stack-wide .btn`, the general rule since T373). Below 641px the header's Invite a person takes the row, 44px. Nothing scrolls sideways at 390 (states.md's Narrow, at Step 2.16: each row a block, Revoke… 44px).

## Contrast

No new colour token. The Delivery words: body text 12.63:1 and Being sent `muted-text` 5.09:1 on the surface; Sent's icon `success-color` 5.88:1 and the warning icon `warning-color` 5.77:1 on the surface (Status dots' pairs). The note row's words 11.89:1 on `warning-bg`, its stripe 5.43:1. The section's count, `muted-text` on the page's `background-color`, 4.83:1.
