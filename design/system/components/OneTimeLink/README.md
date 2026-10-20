# OneTimeLink

The registration link an issue or a resend made, shown once (`Pages/Admin/Invitations/OneTimeLink.razor`; its words in `InvitationText.cs`): a success Alert, the link in a read-only field the person can select, **Copy link**, a live line that says whether the copy worked, and until when the link works. Flow 11 (T373, 2026-10-20, 69a17c70; S10, M5, M7, F7; boards N03, N06, N06b, N09, N21, L10; build review P1) made it a component, drawn on two pages: Invite a person after an issue (InviteAPerson) and Invitations after a resend (Invitations). Until then the link sat in an info Alert as text with no way to copy it, said the link "expires in 14 days", and stayed on screen after the invitation it named was revoked (T264's note).

## What the consumer provides

```razor
<ActionResult @ref="_result" Id="invitation-result">
  <OneTimeLink Email="@issued.Email" Link="@issued.Link" ShowOpenInvitations="true"
               Rest="@InvitationText.IssuedRest" Help="@InvitationText.IssuedLinkHelp(issued.ExpiresOn)">
    <Lead>Invitation issued for @InvitationText.Address(issued.Email).</Lead>
  </OneTimeLink>
</ActionResult>
```

- `Email`: the address the link is for, named in the label with `<wbr>` after its @ (M7).
- `Link`: the registration link itself (`InvitationLinks.RegistrationUrl`, `…/account/register/start?token=…`).
- `Lead` (bold) and `Rest` (plain): the result's first sentence and the rest (the first sentence bold, as flow 10's L4). Issued: "**Invitation issued for ‹address›.** Its email is being sent. Copy the link below — it is shown only once."; resent: "**A new invitation link is being emailed to ‹address›.** The link it replaces no longer works. Copy the new link below — it is shown only once."
- `Help`: until when it works, on the South African calendar (F7), and that it is shown once: "It works until 2026-11-03. It is shown here until your next action, then never again."; after an issue, also "If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one." Until T373 it said the link "expires in 14 days".
- `ShowOpenInvitations`: Invite a person's "Open Invitations" link under it (N03); the list itself offers none (L10).

## Markup

```html
<div class="otl">
  <div class="alert alert-success" role="status"><strong>Invitation issued for …</strong> Its email is being sent. …</div>
  <label for="otl-link">Registration link for vanrensburg@<wbr>sun.wombat.local</label>
  <div class="otl-row">
    <input id="otl-link" class="form-control" readonly value="https://…/account/register/start?token=…"
           aria-describedby="otl-help otl-said">
    <button type="button" class="btn btn-outline" aria-describedby="otl-said">(copy) Copy link</button>
  </div>
  <p id="otl-said" class="otl-said field-help" aria-live="polite"></p>
  <p id="otl-help" class="otl-help">It works until 2026-11-03. …</p>
  <p><a href="/admin/invitations">Open Invitations</a></p>
</div>
```

- `.otl`: a flex column, 8px between its parts, each with no margin of its own.
- `.otl-row`: the field and the button side by side, 8px apart; the field takes the room (`flex: 1 1 auto`, `min-width: 0`) in the mono face at 0.9rem, so the real link is selectable and never widens the page; the button keeps its width.
- `.otl-help`: muted, 0.9rem.

## Copy link (S10)

`wombat.copyText` (`wombat.js`; no inline script) writes the link to the clipboard. Taken: the button reads "Copied" with `check` (16px), and the live line says "Link copied." Refused (no permission, no clipboard, an insecure page) or failed: the button keeps "Copy link" with `copy`, and the live line says "Could not copy. Select the link and copy it yourself." A new link has not been copied: the line empties. **The live line stays in the accessibility tree while empty** (P1): `.otl-said:empty` is hidden the visually-hidden way and taken out of the flex flow, never `display: none`, so a screen reader announces the copy's result when it arrives.

## Once

The page that holds it clears it at the start of every action, refusal or navigation, and never restores it (M5): nothing stores the link, and none is drawn after a revoke. It is inside the page's `ActionResult`, which takes the focus once the issue or resend has answered.

## Narrow widths

Below 641px, inside `.inv-page`: the field stacks over Copy link (`.inv-page .otl-row`, `flex-direction: column`), the field 44px tall, the button 44px and the row's width. A 64-character address breaks after its @ in the label and the result (M7).

## Contrast

No new colour token. The alert's words 11.23:1 on `success-bg`; the link `text-color` on the surface 12.63:1; the help and the live line `muted-text` 5.09:1; Copy link 4.86:1.
