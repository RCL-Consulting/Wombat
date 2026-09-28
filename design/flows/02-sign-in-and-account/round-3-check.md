# Flow 02 · Round 3 — the check (2026-09-28)

Round 3 (`round-3/`, canvas version `1790582802-44c1`, page "R3 · Corrected", 41 boards) checked item by item against
`round-3-ask.txt`, from the boards' text. **Accepted: the design is final for step F.**

| Item | Held | Where |
|---|---|---|
| C1 no link date | yes: no board carries a date | R3-MA-Linked, InstOnly, Confirm |
| C2 six rules | yes: one heading, one order, "At least 4 different characters." second; the field's own message | R3-Spec, R3-CP-Blank, Rules, 390, AdminReset |
| C3 cast | yes: Zulu and Patel not misused; edge states on Tumi Moloi and Karabo Sithole (invented) | R3-Spec § People |
| C4 institution | yes: "Kgosi Kgari Teaching Hospital"; devadmin's card has no Institution row; forgot-password worded for both | R3-MA-Loaded, R3-MA-Admin, R3-FG |
| C5 words | yes: SsoFailed, SsoNoEmail, LinkFailed, the link page's password, both variants of unavailable and expired, "Please" dropped everywhere, the third toggle "Show confirm new password", Steps 2.19 and A.3.2 | R3-SI-Messages, R3-Spec, R3-Steps |
| C6 one focus rule | yes: no buttons, Email; with buttons, the notice itself (tabindex -1) | R3-Spec, R3-SI-SSO-SessionEnded |
| C7 Remove as a form post | yes: ConfirmDialog form mode with the antiforgery token, 448 px / 32 px; success, wrong password, last way in, failure | R3-MA-Confirm, R3-MA-RemoveStates, R3-MA-390-Confirm |
| C8 the Spec's claims | yes: owners of the words; every DESIGN.md rule changed; `.details-list--stacked`; four icons NEW; `.btn-sm` 28 px; the error page's link unchanged | R3-Spec |
| C9 missing states | yes: 390 linked, institution-only, dialog, link expired, a refusal; Change password institution-only without subtitle | R3-MA-390-*, R3-LK-390-Expired, R3-SI-390-Refused, R3-CP-InstOnly |
| E1–E11 | yes: E2 drawn as the `LockedSignedOut` notice; E3 the password in the dialog; E4–E6 as build notes; E7–E9 in the Spec; E11 the admin reset card | R3-SI-Locked, R3-MA-Confirm, R3-Spec, R3-CP-AdminReset |

## Carried into the build (not worth another round)

- **"Karabo Sithole"** shares a surname with the cast's Dr Kabelo Sithole. Boards only; never use the name in tests or
  the runbook.
- **The lockout's length:** `LockedSignedOut` says "a few minutes"; the lockout is 15 (`DependencyInjection.cs:69`).
  Say "15 minutes", from the option, not a literal.
- **E6's route:** the Spec says `POST /account/logout`; the build moves the post to `/account/logout/submit` (round 2
  review, B1). The antiforgery requirement goes with it.
- **The Remove endpoint** is named `/account/external-logins/remove` on the boards; keep it, with a constant path.
