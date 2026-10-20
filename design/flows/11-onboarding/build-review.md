# Flow 11's build: the review, merged

Three read-only Sonnet reviews of `t373` (2026-10-20): the code, data, scope and authorization
(`build-review-code.md`, B1–B12), the pages, design system and accessibility (`build-review-pages.md`, P1–P13), and the
runbook and cast (`build-review-runbook.md`, C1–C9), plus the seven points the runbook lane reported (R1–R7, below).
**No must-fix.** The authorization side holds: M3's audit rows carry nothing of another institution, M4 refuses before
anything is accepted, F5 is no open redirect, F10's transaction and its fault test are real, S1/S2's cookie and
redirects leak nothing, out-of-scope reads as not found everywhere. Every quoted word in the 29 Expects matches the code.

## The fix pass (`t373-fix`): everything below

**Should-fix**
- **B1** Two tabs, two invitations: the submit accepts whichever invitation the one cookie holds. A non-secret
  fingerprint of the invitation (its id, or a hash of the token) in a hidden field, checked against the cookie at
  submit; a mismatch is G16's card (or a new "This page is for another invitation" card: say which). Test: X then Y,
  submit X's form, nothing accepted.
- **B2** Admission in one transaction (profile, scope, the role swap), as F10's accept; a fault leaves the registrar
  pending and a retry admits. Test with a faulted role step.
- **B3** `GetUserByIdQuery.cs:58/62` and `RevokePendingInvitationsForEmailCommand.cs:43`: `InvitationAddress.ToAddress`
  and the South African day (`ProgrammeCalendar.DateOf`), with a case-variant and a 22:30 UTC test.
- **B4** `ListExpiredUnusedInvitations.cs:128`: `GetValueOrDefault`, never a throw on an address the two lowerings
  disagree about; a test with a non-ASCII address.
- **P1** The OneTimeLink's live region stays in the accessibility tree while empty (no `display:none`).
- **P2** `FocusAfterRetry` on Invitations, Invite a person and Admit, as Trainees.
- **P3** Admit's `NotPending` refusal: the focus to the not-found panel's heading (or keep the form and a danger Alert:
  say which).
- **P4** Trainees reacts to `?admitted=` / `?back=` on every navigation (`OnParametersSetAsync` or
  `[SupplyParameterFromQuery]`), so the alert never outlives its URL.
- **C1** `states.md` "A start long ago": the training year `StageOn` gives (6, or 7 near 15 January).
- **C2** Runbook README § The browser: grant the clipboard permission (`clipboard-read`, `clipboard-write`) in the
  Playwright context, or 1.5 and 2.2 read "Could not copy…".
- **C3** 2.9's Note: 3 presses, 8 fields (two refusals).
- **C4** 2.29's Expect: for a passed derived date the help is replaced, not added to (quote the built sentence).
- **C5 / R1** "Admit to curriculum" in `UserAdministrationRules.cs:44`, `:50`, DESIGN.md (~2792), and 2.13's and 2.28's
  Expects: "Admit…" in all, the same commit.
- **R2** `InvitationText`'s six dead helpers: the pages use them, or they go (one copy of each sentence).
- **R3** Native `disabled` on the list's other rows during a resend: `aria-disabled` with `.is-unavailable` and the
  reason, or the Spec's exception said in DESIGN.md (choose the one DESIGN.md already allows for busy rows; say which).
- **R4** 2.4's hint: keep "Then choose its sub-speciality." once a speciality is chosen until a sub-speciality is, as
  R3-0-steps says, and 2.4's Expect with it.

**Nits worth the pass**
- B6 (an in-scope holder with no name: no "open  under Users"), B7 (a token-length bound on `/start`), B8 (sign-out
  never 500s on a malformed body), B10 (the timed-out cookie's window not extended), B11 (the two test gaps: `no-store`
  on the 303, `returnTo` case variants at the endpoint), B12 (`GetAdmissionDates` judges the curriculum in scope).
- P5 (the dead `is-invalid`), P8 (the info alerts' `role="status"` at load), P9 (one role for the trainee note on all
  four pages), P11 ("At ." never printed), P12 (dates through the invariant culture), P13 (the dialog phone rule reaches
  `.dialog-actions` too).
- C6 (the issuer-gone row reactivates Prof Mbatha after), C7 (M04's row needs-confirmation), C8 (2.7's count: Copy link
  only where a step says), C9 (2.29's order as Current trainees lists them).

**Left as they are:** B5 (per-row queries on lists of a handful), B9 (`?admitted=` for any active profile in scope:
it names only what the caller may read), P6 (no `aria-label` on ActionResult anywhere: a system change), P7 (Cancel, as
the boards), P10 (Resend's sibling rows: R3 covers it), R5–R7 (for the replay: stubs tagged `invitation` only; flow 13's
Completed table at 390; N26's stand-in).
