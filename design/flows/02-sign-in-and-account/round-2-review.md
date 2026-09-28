# Flow 02 · Round 2 review (2026-09-28)

Four reviewers (tokens; build against the code; states and copy; accessibility and security) read the 36 boards in
`round-2/` against `../02-sign-in-and-account.md`, `round-1-review.md`, `round-2-ask.txt`, DESIGN.md and the code at
`ccc61a41`. This is the synthesis. Ids in brackets are the reviewers' own (T tokens, B build, S states, A access).

**Verdict: accept with changes.** The tokens hold (every colour is a `:root` token; all 14 contrast figures recompute
correctly). The structure and the operator's round 1 decisions are drawn faithfully. What needs a correction round is
data that contradicts the runbook, a missing password rule, words that are still framework text or wrong advice, a
focus rule that contradicts itself, and a Spec that calls things "unchanged" or "existing" that the build must make.
Nothing blocks; three things cannot be built as drawn until corrected (C1, C4, C7).

## Corrections for the canvas (round 3)

- **C1. Drop "Linked 2026-08-14".** `AspNetUserLogins` stores no date (stock `IdentityUserLogin<string>`). Name each
  sign-in by the provider's display name only. [B3, S7]
- **C2. Six password rules, not five.** `RequiredUniqueChars = 4` (`Infrastructure/DependencyInjection.cs:61`) is
  enforced and already described by `PasswordRuleMessages`. Add "At least 4 different characters." One heading, one
  order and one sentence per rule in both the up-front list and the refusal; add the field message to the Spec. [S1, S2, B4]
- **C3. Fictitious people for the edge states.** Thandi Zulu is CommitteeMember + Assessor at `zulu@kgk.wombat.local`,
  and Mohammed Patel signs in with a password (A.6.5, A.6.8). Put four roles, the long email and institutional-only on
  an invented person. [S3]
- **C4. The institution is "Kgosi Kgari Teaching Hospital"** for every KGK person (Step 1.6), not a placeholder. Draw
  the Account card for an account with none (devadmin; a College admin): omit the row. Forgotten password must not say
  "your institution" to such a person: word it for both. [S4, N7, S-D4]
- **C5. Words.**
  - Rewrite `SsoFailed` ("SSO login failed.") and `SsoNoEmail` ("The identity provider did not supply…").
  - `ExternalLoginUnavailable` gets a no-provider variant (A.3.3 plays on dev, where there is no button to try again).
  - `LinkFailed`: the person did nothing wrong; say try again later.
  - The link page's missing password: "Enter your password."
  - "Show new password again" must match its field's label, "Confirm new password".
  - The ConfirmDialog's "You will sign in with your email and Wombat password only." is untrue when another
    institution is linked; word it from what remains.
  - A.3.2's new Expect adds the Show toggle; Step 2.19's "PendingTrainee" becomes "Pending trainee".
  - "Please": keep or drop it consistently.
  [S5, S6, B11, B13, N3, N4, N6, N7, N10]
- **C6. One focus rule for notices, with or without providers.** "Every notice: focus Email" and "providers: no
  autofocus" contradict each other; a `role="status"` present at load is not announced without focus. With providers,
  the notice itself takes the focus (`tabindex="-1"`), so the next Tab reaches the first institution. Draw that
  state. [A6, S8, B-D3]
- **C7. The Remove flow as a form post.** ConfirmDialog today runs `@onclick` in the circuit (`ConfirmDialog.razor`),
  which cannot re-issue the cookie. Draw it as a dialog holding a posted form (NEW: ConfirmDialog's form mode), at the
  built size (`.dialog-card` 448 px, 32 px padding). Draw the Remove success and refusal, and the server's refusal of
  the last way in (a crafted or two-tab post). [B2, T3, S9, B-D6]
- **C8. The Spec's claims.**
  - `PasswordToggleButton` is also used by Register (flow 11) and the admin reset card (flow 12): the Spec says both
    move to the new toggle in this flow.
  - `.details-list` exists (side by side at desktop): use it as built or name a modifier.
  - The icons `eye`, `eye-off`, `building-2`, `key-round` are NEW to `wwwroot/icons/`.
  - The owners of the words: `SignInOutcome` holds FieldsMissing, the external codes and SsoFailed;
    `ExternalLoginHandler` (Infrastructure) the twelve SSO sentences; `ChangePasswordOutcome` SessionEnded and
    PasswordChanged.
  - DESIGN.md rules broken but not named: § Typography's 1.5rem h1 (the auth h1 is 2rem) and card titles as h3; §
    Accessibility's required `*` (dropped here); § Account / auth page's 400 px card, `.mb-3`, "Change password stays
    interactive" and the PasswordToggleButton paragraph; `?status=updated`. "§ Page-level patterns (Account)" is §
    Account / auth page.
  - The error page's Sign out links to the bare `/account/logout-confirm` today; if it is to carry a returnUrl, say so
    as a change.
  - Remove the drawn `.btn-sm` at 32 px: it is 28 px as built (flow 01).
  [T1, T2, T4, T5, T11, B6, B8, B9, B10, N3]
- **C9. States still missing.** At 390: the linked account, institutional-only, the ConfirmDialog, the link page's
  expired state, and one ordinary sign-in refusal. Also the Change password institutional-only state without the
  generic subtitle. [S9, N2]

## For the build (step F), not the canvas

- **Sign-out route [B1, A12].** Razor component endpoints take GET and POST, so `@page "/account/logout"` collides with
  `MapPost("/account/logout")`. Move the post to `/account/logout/submit`, as every other `SubmitPath`; the page answers
  GET on both addresses. An anonymous GET goes to sign-in with no returnUrl.
- **T287 in the address, not only the words [A1, A2, B5].** Redirect a lockout with `SignInOutcome.Refused` (words from
  `SignInMessages.Refused(offered)`), and map `LinkLockedOut` to `LinkRefused`'s code. Keep the audit actions. A test
  that a locked account's redirect is byte-identical to an unknown address's.
- **The toggle [A3, A4, A5, B6, B7].** `[hidden]{display:none !important}` (`.btn` overrides it today); one delegated
  listener, wired on `DOMContentLoaded` and `enhancedload`; every field back to `type=password` on submit and on
  `pageshow`. A `PasswordField` component: static markup when not interactive, `@onclick` when interactive.
- **Remove's endpoint [A8, B-D6].** `ValidateSecurityStampAsync` first; the account from the cookie; the last-way-in
  guard on the server (`!AllowLocalPassword && logins.Count <= 1` and no password); a Command so the audit pipeline
  records it; every throwable check before `RemoveLoginAsync`; antiforgery with a test; concurrency across two tabs.
- **StatePanel's Try again** needs an `OnRetry` (T329) or a page reload [B12]. **Change password's focus** is "the top
  of the page": reaching an excluded page is a full load [N2]. The refusal's autofocus replaces Email's [N4].
- **Tests to change deliberately:** the build reviewer's list (Web: ChangePasswordPageTests, ProfilePageTests,
  SignInPageFieldTests, LinkExternalLoginPageTests, AccountRefusalCodeTests, PageTitleTests, StylesheetRuleTests,
  MainLayoutTests; Integration: ChangePasswordFlowTests, ProfileFlowTests, AccountRefusalFlowTests,
  SessionRevalidationFlowTests, ErrorPageFlowTests, ActingRoleFlowTests; Infrastructure: SsoLinkAndSignInTests;
  coverage.md § Pages). Shared helpers named up front: `Hosting/ServerComponentMarker.cs`, a toggle-markup assertion,
  and `Design/Stylesheet.cs` (never a `StyleSheet.cs`).
- **Lanes:** 1 words and outcomes; 2 the anonymous card and the toggle; 3 endpoints and sign-out (sole owner of
  `Program.cs`); 4 My account and T286's half; 5 Change password static; 6 after merge, the runbook and the replay. No
  migration. `app.css`: delimited blocks per lane.

## Decisions (the operator accepted all eleven recommendations, 2026-09-28)

- **E1.** An institutional sign-in to a locked account says "Wombat could not sign you in through your institution.
  Contact your administrator." (only a provider-verified person reaches it). [A10, S-D1, B-D1]
- **E2.** Five wrong current passwords on Change password lock the account **and sign the session out**; the sign-in
  page then shows a notice saying why. [A9, S-D3, B-D2]
- **E3.** Removing an institutional sign-in **asks for the password** when the account has one. [A8]
- **E4.** The sign-in page's notices (signed out, session ended, password changed) show **only to a signed-out
  visitor**; a signed-in visitor is sent Home. [A7]
- **E5.** T287's timing half lands in this flow: a dummy password check on the unknown, institution-only and locked
  paths. [A11]
- **E6.** Signing out requires the antiforgery token. [B-D4]
- **E7.** 44 px controls below 641 px on this flow's pages; recorded as the rule for later flows. [T6]
- **E8.** The boards' values win, each named in the Spec as a change: 16 px rhythm on the card, the card top-aligned,
  the auth h1 2rem, 1.5rem below 641 px. [T7, T8, T9]
- **E9.** The pressed toggle keeps the word "Show", with `aria-pressed`, the eye-off icon and the fill (T328). [S-D2, A17]
- **E10.** Saving an unchanged name says "Name saved.". [S-D5]
- **E11.** Register (flow 11) and the admin reset card (flow 12) move to the new toggle and the rule sentences in this
  flow. [T1, B6, B-D5]
