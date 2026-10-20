# InviteAPerson

Invite a person (`/admin/invitations/new`, `Pages/Admin/Invitations/InviteAPerson.razor`; its words in `InvitationText.cs`, the scope sentences in `InvitationRules.ValidateScope`, its addresses in `Shared/OnboardingLinks.cs`): **the issue form, a page of its own**, one person and one role, drawing only the fields the chosen role takes, and showing the registration link once (OneTimeLink). Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-N01 to N26; round 3 Q1, Q3, Q10, Q12, Q18, F9, M3, M5, M7, S10, S13, S15, S17, S18; T264, T286, T324, T329; build review P2, R4, B6) split it from the invitations list. Until then the form sat beside the list, Role was a select defaulting to Institutional admin, every scope field was drawn and the unused ones disabled, an address an account already held was issued and refused only when its link was opened (T286), and a refusal printed the command's text in a summary above the form.

## Who reads it

Administrator and Institutional admin, as Invitations. Owned by Invitations (NavMenu): Home › Invitations › Invite a person, Invitations lit. Someone who also holds Trainee sees the header and one info alert, `#invite-trainee-note`, "You hold the Trainee role, so you cannot view or change user accounts, including your own.", before anything is read (M1, N24).

## The page

The header: "Invite a person" (h1 and tab), "One person, one role. The registration link is shown here once, and emailed to them." Then `.inv-page.inv-page--form` (48rem): the result (`ActionResult`, `#invitation-result`), then the form, `form.form-container`, `novalidate` (the page refuses in its own words, T324), named by no `aria-label` that repeats the h1:

1. "* marks a field you must fill in." (`p.field-help`; FormField's required marks).
2. **Email** * (`#invitation-email`, `type="email"`, `aria-required`, `autocomplete="off"`, `spellcheck="false"`).
3. **Role** * (`fieldset#invitation-role.form-group.choice-row`, its legend "Role" with the mark and a hidden " required"): one `.form-check.review-type-choice` a role, a radio named by its label alone, its description a `p.page-subtitle.review-type-help` outside the label, in the radio's `aria-describedby` (S17). **None chosen** at first. In this order, College admin to the Administrator only (N01, N02):
   - College admin: "Keeps a College's specialities, EPAs and curricula. Choose the College."
   - Institutional admin: "Runs an institution in Wombat: invites and admits people, adopts curricula."
   - Speciality admin: "Runs one speciality's committee work: panels, reviews and decisions due."
   - Sub-speciality admin: "Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality."
   - Coordinator: "Works across the institution. Choose a speciality only to keep them to it."
   - Committee member: "Sits committee reviews. Choose a speciality only to keep them to it."
   - Assessor: "Assesses the registrars who ask them, at this institution."
   - Trainee: "A registrar. Registers as Pending trainee, and waits to be admitted under Trainees."
4. **Only the chosen role's fields** (S15), in a `.form-grid`; a field the role does not take is not drawn, never disabled:
   - College admin: **College** * ("Choose a College").
   - Every other role: **Institution**, read out for the Institutional admin as `dl.form-group` with `dd.readonly-value` "Kgosi Kgari Teaching Hospital" (S18), a select * for the Administrator ("Choose an institution", N04).
   - Speciality admin, Sub-speciality admin, Assessor and Trainee: **Speciality** * ("Choose a speciality"); for the three that need a sub-speciality, its help "Then choose its sub-speciality." stays until one is chosen (R4, N08a). Coordinator and Committee member: Speciality with no mark, at "Any speciality", its help "Leave it at Any speciality for the whole institution." (F9, Q18, N05).
   - Sub-speciality admin, Assessor and Trainee, once a speciality is chosen: **Sub-speciality** * ("Choose a sub-speciality"), its options that speciality's (N08).
   - **Nothing adopted** (N26, S13): for a role that needs a speciality, Speciality is drawn as its name and a `p.field-warning`, "Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot be invited. Adopt a curriculum under Curriculum adoptions.", with the link; Issue invitation stays for the roles that need none.
5. `FormActions`: **Cancel** (outline, a link to Invitations) and **Issue invitation** (primary). While the issue runs it keeps the focus with `aria-disabled`; a second press sends nothing (T234).

## Opened from another invitation (`?from=‹id›`, Q9; N19, N20)

From a revoke's "Invite another address as …" or Expired, not used's Invite again (`OnboardingLinks.InviteFrom`), the form is filled from that invitation, re-read in the caller's scope, under an info alert (`role="status"`):
- **Revoked**: "Role and scope filled from the revoked invitation to ndlvou@kgk.wombat.local. Type the right address.", role and scope chosen, Email empty and focused.
- **Expired**: "Filled from the invitation to expired@kgk.wombat.local, whose last day was 2026-10-19. Check it, then issue.", the address, role and scope filled.
An id that is not the caller's fills nothing and says nothing.

## Issued (N03, S10)

The result takes the focus: the OneTimeLink, "**Invitation issued for vanrensburg@sun.wombat.local.** Its email is being sent. Copy the link below — it is shown only once.", "Registration link for …", the link and Copy link, "It works until 2026-11-03. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.", and "Open Invitations" (OneTimeLink). Under it the form again, empty, under `h2#invite-next-person` "Invite the next person", which then names the form. The link is cleared at the start of the next issue, refusal or navigation (M5).

## Refusals (Q3, M3, S15; N02, N10–N18, N25)

**Each refusal sits under its field**, never in a summary: `p.validation-message` (`{field}-msg`), the control `.input-validation-error` and `aria-invalid`, described by it, and the focus on it; a refused Role's message under its legend, the group `.choice-group.is-invalid`, the first radio focused (FormField).
- The page's own, before it sends: "Enter the person's email address." (N13), "Enter a valid email address, for example name@example.org." (N14), "Choose a role." (N25).
- The scope's (`InvitationRules.ValidateScope`): "A college is required for a college administrator." under College (N02), "Speciality administrators must be scoped to a speciality." under Speciality (N10), "The selected role requires speciality and sub-speciality scope." under Sub-speciality (N11).
- **The address** (Q10, Q12; `InvitationIssueRefusedException`), under Email, naming the holder and linking it **only in the caller's scope** (M3): "smit@kgk.wombat.local already has a Wombat account (Pieter Smit), so it cannot be invited. To give Pieter Smit another role, open Pieter Smit under Users." (N15; an account with no name: "… already has a Wombat account, so it cannot be invited. To give the account another role, open it under Users.", B6); "An account with this address already exists. Ask a Wombat administrator to add the role." (N16); "twice@kgk.wombat.local already has an active invitation, issued ‹date› ‹hh:mm› SAST, which works until 2026-11-03. Resend it or revoke it under Invitations." (N17); "This address already has an active invitation from another institution. Ask a Wombat administrator." or "… from a College. …" (N18).
- Anything else, logged: "The invitation could not be issued just now. Nothing was saved. Try again, or come back in a few minutes." in the result.

## States (N22, N23, T329)

- **Loading**: `p.loading-line` "Loading what you can invite." over a skeleton card.
- **Load failed**: "Could not load what you can invite. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel); a Try again that loads hands the focus to Email (P2).

## Narrow widths

Below 641px every button in the column is 44px (`.inv-page .btn`), the form's actions stacked, each the row's width, Issue invitation on top (`.inv-page .form-actions`, `column-reverse`, as every stacked action row; Cancel stays first in the tab order), each role's row 44px tall (`.inv-page .review-type-choice`); the link's field stacks over Copy link (OneTimeLink). Nothing scrolls sideways at 390, a 64-character address included: a refusal or result naming it breaks after its @ and, at need, anywhere (M7, `overflow-wrap: anywhere` on every `.alert` and `.validation-message`).

## Contrast

No new colour token. A role's description `muted-text` 5.09:1 on the surface; the read-out institution body text 12.63:1; a refusal `danger-color` 5.95:1; the nothing-adopted warning as FormField's `.field-warning`.

## Known gaps

- **A described radio's row is 44px tall, but only its label chooses it** (T375, proposed): a tap on the description or below the label does nothing.
