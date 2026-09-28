# F02 Getting in, staying in, and managing one's own account

**Who and why:** everyone signs in every session, and every refusal on the way in is access control. The account pages
are where a person corrects their name and changes their password. Most of these pages are static HTML with no
interactivity, so what can be designed is narrower than it looks (DESIGN.md:1887–1933).

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked F in BRIEF.md § 8: little structural change is expected, so round 1 is one structural proposal to confirm, with alternatives only where § 6's questions fork. |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: one structural proposal as wireframes. Round 2, after the confirmation: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | One structural proposal, with 2 options only at § 6's forks (forgot password, sign-out confirmation, where SSO sits); the chosen one at fidelity |
| People | Everyone, and anonymous visitors. Cast: devadmin; Dr Fatima Khumalo (changes her password); Dr Pieter du Plessis (given a password by an administrator); Dr Mohammed Patel (locked out); Mr Pieter Smit (signs out); Dr Sarah Botha (edits her profile) |
| Runbook steps | 19, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/account/login`, `/account/logout-confirm`, `/account/profile`, `/account/change-password`, `/account/forgot-password`, `/account/link-external`. Endpoints: `/account/login/submit`, `/account/logout`, `/account/session-ended`, `/account/sso-challenge/{providerKey}`, `/account/sso-callback`, `/account/profile/submit` (T335: the name is posted, and the sign-in is issued again with it), `/account/change-password/submit`, `/account/link-external/submit` (not played: it needs an identity provider) |
| Held | None: no group-1 task changes these pages |
| Depends on | Flow 01, built (`b347e11c`): the shell, its tokens, alerts, focus ring, `ActionResult`, `StatePanel` and the anonymous card (`AuthLayout`) |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 02`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/`, or crop its link first.
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v4
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not flow 01's canvas, which holds the old design-system copy.
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in the first message.
4. **Paste § 1** with the key screenshots in § 4, then **§ 8** as the next message.
5. **Confirm or correct round 1's structure,** answering § 6's questions one sentence each. Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/01-shell/round-2-review.md` did. Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/02-sign-in-and-account/` before moving on.

---

## 1. The ask (paste this first)

```text
FLOW 02 — Get in, be told plainly why when I cannot, and keep my own account right

This is flow 02 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flow 01, the shell, is designed and built;
  these pages sit inside it (signed in) or on its anonymous card (signed out). Structure first, then fidelity: round 1
  is wireframes only, and I confirm it before round 2. The brand mark is at /_blob/a6c690a50a59fa44f668ab96ac894576:
  use it, never a drawn disc.

GOAL: A person signs in, and is told clearly why when they are refused, throttled, locked out, or signed out by the
  system. Signed in, they correct their name and change their password, and see what their account is.
  What is wrong today (the attached screenshots show each; all are on flow 01's shell as built):
  - The password Show toggle on the sign-in page does nothing: the page is static HTML, and the toggle needs a live
    connection. Nowhere does it expose its state (aria-pressed) or name the field it controls.
  - Change password has three toggles, all named "Show".
  - The password-rules refusal is one run-on paragraph of framework text ("non alphanumeric character", "('0'-'9')").
  - My account prints roles by their codes ("CommitteeMember, Assessor"), as a bold-label paragraph.
  - Forgot password is a stub: "Password reset is not wired yet in the rewrite. Ask an administrator to issue a new
    invitation or reset the account directly."
  - The sign-out confirmation breaks on the anonymous layout: its heading sits beside an empty card that runs the
    screen's full height.
  - "Your session has ended" is shown in the red danger alert a refusal uses, though nothing was refused.
  - Typing /account/logout while signed in answers 405 (T317).

AUDIENCE: everyone, and anonymous visitors; desktop 1280×800 and phone 390×844. Clinicians sign in between patients,
  often on a phone. Cast: devadmin (the platform operator); Dr Fatima Khumalo (changes her password, getting it wrong
  first); Dr Pieter du Plessis (forgot his password; an administrator sets one, then he chooses his own); Dr Mohammed
  Patel (locked out by an administrator, then let back in); Mr Pieter Smit (signs out through the confirmation page);
  Dr Sarah Botha (edits her name).

SCREENS, in order:
  1. Sign in (static; the anonymous card): Email, Password, Remember me, Sign in, and "Forgotten your password?".
     Its states are listed below.
  2. Sign in when an institution signs its people in (SSO): an "or" divider and one "Sign in with <institution>"
     button per provider. No provider is configured on dev, so there is no screenshot; design it from this text.
  3. Sign out confirmation (signed in, but no nav): one button that ends the session, and Cancel. Also what a typed
     GET /account/logout shows.
  4. My account (in the shell; reached from the name in the top bar, which it underlines; no nav item lights): an
     account summary (email, roles, Change password) and the profile form (email read-only, first name, last name,
     Save profile). The form posts to an endpoint, which saves the name, issues the sign-in again, and reloads the page
     with the outcome, which takes the focus. States: loaded, saved ("Profile saved."), a blank name stopped by the
     browser's required check, a space refused ("Enter your first name and your last name.", tab title "Error: …"),
     a pending trainee, loading, load error.
  5. Change password (in the shell; trail Home › My account › Change password): current, new and confirm, each with a
     show/hide toggle. States: blank; "Incorrect password."; "The password confirmation does not match."; the rules
     not met (one sentence per rule broken: at least 12 characters, a digit, an upper-case letter, a lower-case letter,
     a symbol); "Password updated."; locked; throttled ("Too many attempts from this network. Please wait a few minutes
     and try again."); an institutional-only account ("This account signs in through your institution, so it has no
     password to change here."). After a refusal the page reloads with empty fields, the tab title starts "Error:",
     and the refusal takes the focus.
  6. Forgot password (static): see question 2.
  7. Link an institutional sign-in to an existing account (static; needs SSO). Only the expired state can be
     captured: "Your institutional sign-in has expired. Start again from the sign-in page." with Back to sign in.
     Design the live state from this text: "A Wombat account with the email <email> already exists. Enter your current
     password to link your institutional sign-in.", a Password field with its toggle, "Link & sign in" and Cancel, and
     a refusal slot above the field (a wrong password, a lockout, a failure).

  The sign-in page's words, by state (from the code; the runbook quotes them):
  - refused: "Invalid email or password." With SSO offered: "Invalid email or password. If your institution signs you
    in, use its sign-in button below."
  - too many attempts (from this network): "Too many failed sign-in attempts from this network. Please wait a few
    minutes and try again."
  - locked out: "Too many failed sign-in attempts. Please try again later or reset your password."
  - fields missing: "Email and password are required."
  - session ended: "Your session has ended. Please sign in again."
  - password changed elsewhere: "Your password was changed. Please sign in with your new password."
  - external sign-in unavailable: "External login information was not available."
  - external sign-in expired: "External login session expired. Please try again."
  - unknown provider: "Unknown SSO provider."
  - anything else: "Sign-in could not be completed. Please try again."
  - plus twelve institutional sign-in refusals (no email, email not verified, account locked, administrator, wrong
    institution, email in use, account not created, already linked, link refused, link locked out, link failed, SSO
    failed): design one refusal slot that holds a sentence of up to about 25 words.
  Which of these is a refusal (danger) and which a notice (session ended, password changed) is the design's to say.

STEPS: 1.1, 2.8, 2.31, 2.41, A.3.2, A.3.3, A.4.1, A.4.2, A.4.3, A.4.4, A.4.5, A.4.6, A.4.7, A.6.5, A.6.8, A.1.13,
  A.7.12, A.7.14, 5.19. Pasted verbatim in the next message (Role / Route / Do / Expect). Where an Expect quotes today's
  wording, that is today's page, not a requirement on the new one.

STATES TO SHOW:
  Sign in: blank, refused, too many attempts, locked out, fields missing, session ended, password changed, external
    unavailable, external expired, unknown provider, general refusal, the SSO variant (three providers), narrow.
  Sign out confirmation: default, a typed GET /account/logout, narrow.
  My account: loaded (two roles), saved, blank name (browser check), space refused, pending trainee, loading, load
    error, narrow.
  Change password: blank, incorrect, confirmation, rules, updated, locked, throttled, institutional-only, narrow.
  Forgot password: the chosen answer to question 2, narrow.
  Link an institutional sign-in: expired, the live form (described), a refusal, narrow.
  Data volumes: a long email (up to about 40 characters) in the summary and the top bar; four roles on one account.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T328: every button and link is at least 24 px tall (44 px on a phone). The password toggle (28 px, centred since
    flow 01) exposes aria-pressed and names the input it controls ("Show current password", not three buttons named
    "Show"); its label sits outside the input's wrapper.
  - T324: roles are named by label ("Committee member"); validation names the field by its label; the password rules
    are short sentences in plain words.
  - T317: a typed GET /account/logout gets a page, not a 405.
  - T287: the lockout reply must not tell a locked account apart from a wrong password. Today an account an
    administrator deactivated is told "Too many failed sign-in attempts…" on its first try, and an unknown address
    never is. The recommendation is one generic refusal for all, with the lockout explained by email. The design keeps
    one refusal slot either way.
  - T286: nothing lists or removes an account's institutional sign-in links; My account shows Change password even to
    an account that has no local password.
  - T329: My account's loading and load-error states use flow 01's StatePanel.
  - Static pages: sign-in, forgot password and link are plain server-posted forms: no live validation and no
    client-side reveal logic. The password toggle there must be a tiny script module (wombat.js, wired without an
    inline onclick) or be dropped. Sign-in, sign-out, the profile, change password and link stay real
    <form method="post"> elements: the sign-in cookie is written only by an HTTP POST, and each endpoint redirects
    with a code (?error=…, ?status=…) that the page turns into one of a fixed set of sentences, never text from the
    address.

QUESTIONS THE DESIGN MUST ANSWER:
  1. What does the sign-in page say about the product: a line of purpose, the College's and the institution's names?
     Do the SSO buttons sit above or below the password form?
  2. Forgot password: design a self-service reset by emailed link (not built), or replace the stub with plain "Ask
     your administrator" copy?
  3. Is a sign-out confirmation page wanted at all? Flow 01's top bar signs out in one press; today the page is
     reached only by typing its address.
  4. Should My account show the person's institution and institutional sign-ins, beside their roles?

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5; restated after the flow 01 pilot, 2026-09-27)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  the error page, /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side
  behaviour beyond a small script module; no live validation; the phone menu is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence (Source Sans 3 and Fraunces, OFL, ship).
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token (the design system's tokens.json) or a NEW token with its value; prefer the spacing scale
  xs 4, sm 8, md 16, lg 24, xl 32, 2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
The shell is designed and built (flow 01): design the page body inside it, not a new frame. The sidebar shows the
  acting role's navigation (one role at a time; access is the union of the roles held), grouped above eight links, with
  My progress and My data rights under a rule; the top bar holds the person's name and Sign out; below 641 px a phone
  bar with a CSS-only menu. A new page names its owning list (its nav item lights, its breadcrumb follows it) and its
  title "<Page> · Wombat", the same words as its h1 and its nav label, in sentence case.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, the trail from the
  owning list, an optional icon, header actions), DataTable (.clinic-table in .table-container, PagerControls),
  FormField / FormActions (.form-container, .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid,
  loading and load error), StatePanel (loading / empty / error), Skeleton, Alert (success / info / warning / danger,
  text on the tint), ActionResult, ConfirmDialog (native <dialog>), badges (five tints), Icon, TrajectoryChart
  (hand-drawn SVG, no chart library), ReferenceBlock.
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Still to design where a flow meets
  them: field validation (invalid border plus a stripe, not colour alone; message under the field; the summary) and
  session ended.
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.

ASK:
  - Round 1: ONE structural proposal as wireframes, since little structural change is expected here: the pages and
    their order, the steps of each journey (sign in, refused, signed out by the system, forgot, change name, change
    password, sign out), and where each outcome, refusal and notice shows. Give 2 options only where question 1, 2 or
    3 forks, with your recommendation. No colour or type choices yet. Stop and wait for my confirmation.
  - Round 2: the confirmed structure at full fidelity on flow 01's tokens, every screen and state above, at 1280 and
    390 px.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule the proposal
    changes (DESIGN.md § Page-level patterns has the Account and Anonymous shapes).
  - Mark which pages are static (no live behaviour) and keep every interaction on them to links and form posts.
  - Flag edge cases: a long email, a refusal sentence of 25 words, four roles, the SSO variant with three providers.
  - Run an accessibility review against WCAG 2.1 AA: contrast, targets, the toggle's name and state, and where the
    focus lands after each refusal and each notice.

ATTACHED: act-A/A.7.14-1-login.png, act-A/A.6.5-2-locked-out.png, act-A/A.4.3-1-session-ended.png,
  act-A/A.7.12-1-login-390.png, act-A/A.4.2-3-rules.png, act-A/A.4.1-3-profile-saved.png,
  act-A/A.4.1-2-space-refused.png, act-A/A.4.4-1-forgot-password-stub.png, act-A/A.4.7-1-sign-out-page.png,
  act-A/A.7.14-4-danger-alert.png
```

---

## 2. The journey

| Step | Page | The person does | They must be able to see |
|---|---|---|---|
| 1.1 | `/account/login` → `/` | devadmin signs in | the sign-in card, then Home |
| 2.8 | `/account/register` → `/` → `/account/logout` → `/account/login` | Mr Smit registers from his link, lands on Home, signs out | that sign-out returns him to the sign-in page (registration itself is F11's) |
| 2.31 | `/` → `/account/session-ended` → `/account/login` → `/` | Dr Mahlangu's open tab is signed out when her role changes at admission | "Your session has ended. Please sign in again.", then her new Trainee Home |
| 2.41 | `/account/profile` | each person reviews My account; Dr Khumalo corrects "Fatma" to "Fatima" | the email (read-only), the roles held, Change password; "Profile saved." |
| A.3.2 | `/account/login` → `/account/sso-challenge/{providerKey}` → `/account/login` | a consultant looks for an institutional button, gets a password wrong, opens a challenge for an unknown provider | no SSO button when none is configured; "Invalid email or password."; "Unknown SSO provider." |
| A.3.3 | `/account/sso-callback` → `/account/login`; `/account/link-external` | opens the callback and the link page directly | "External login information was not available."; the link page's expired state with Back to sign in |
| A.4.1 | `/` → `/account/profile` | Dr Botha opens My account from the top row, clears her last name, saves, restores it | the browser's required check; a space refused, "Enter your first name and your last name.", nothing saved; then "Profile saved." with the focus on it |
| A.4.2 | `/account/profile` → `/account/change-password` | Dr Khumalo tries four times | each refusal in its own words, the focus on it, the tab title "Error: …"; then "Password updated." |
| A.4.3 | `/activities/inbox` → `/account/session-ended` → `/account/login` | her other browser is signed out; she tries the old password, then the new | the session-ended notice; the old password refused; back to her inbox after signing in |
| A.4.4 | `/account/login` → `/account/forgot-password` | Dr du Plessis follows "Forgotten your password?" | what to do instead (today, the stub) |
| A.4.5 | `/admin/users/{UserId}` | Prof Mbatha sets his password (F12's page) | that the user is not emailed |
| A.4.6 | `/account/login` → `/` → `/account/profile` → `/account/change-password` | he signs in with it, then chooses his own | "Password updated." |
| A.4.7 | `/account/logout-confirm` → `/account/logout` → `/account/login` | Mr Smit opens the sign-out confirmation, cancels, signs out, then opens a page by its address | the confirmation with no nav; Cancel returns him signed in; after signing in he is brought back to the page he asked for |
| A.6.5 | `/account/session-ended` → `/account/login` | Dr Patel, locked out by an administrator, is signed out and cannot sign in | the session-ended notice; the lockout refusal (question: T287) |
| A.6.8 | `/account/login` → `/` | Dr Patel signs in again after reactivation | his Assessor Home |
| A.1.13 | `/account/session-ended` → `/account/login` | Dr Ndlovu, erased, is signed out and cannot sign in | the session-ended notice; "Invalid email or password.", with no mention of erasure |
| A.7.12 | `/account/login`, `/account/forgot-password` | a verifier on a phone | the sign-in card fits 390 px; its fields and buttons are easy to tap |
| A.7.14 | `/account/login`, `/account/profile`, `/account/change-password` | Prof Mbatha checks contrast | the sign-in card, the success and danger alerts, the focus ring and the input border meet AA |
| 5.19 | `/portfolio/progress` → `/account/session-ended` → `/account/login` | Dr Molefe's tab is signed out when her Trainee role is removed at graduation | the session-ended notice |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Account and sign-in.

**The `states/` captures below were taken on 2026-09-26, before flow 01's shell was built.** The shell, the anonymous
card, the tokens and the alert colours in them are old. Where T335's replay re-took the same state on the new shell,
§ 4 attaches that capture instead (`act-A/…`, 2026-09-27). The `states/` column is kept as the re-capture list for § 7.

| Page | State | Screenshot | Words on the page |
|---|---|---|---|
| Sign in | Blank | `states/login--blank.png` | no SSO button (no provider) |
| Sign in | Refused | `states/login--refused.png` | "Invalid email or password." |
| Sign in | Session ended | `states/login--session-ended.png` | "Your session has ended. Please sign in again." |
| Sign in | Locked out | `states/login--locked-out.png` | "Too many failed sign-in attempts. Please try again later or reset your password." |
| Sign in | Throttled | `states/login--too-many-attempts.png` | "Too many failed sign-in attempts from this network. …" |
| Sign in | Fields missing | `states/login--fields-missing.png` | "Email and password are required." |
| Sign in | Password changed elsewhere | `states/login--password-changed.png` | "Your password was changed. Please sign in with your new password." |
| Sign in | External sign-in unavailable | `states/login--external-unavailable.png` | "External login information was not available." |
| Sign in | External sign-in expired | `states/login--external-expired.png` | "External login session expired. Please try again." |
| Sign in | Unknown provider | `states/login--sso-unknown-provider.png` | "Unknown SSO provider." |
| Sign in | General refusal | `states/login--general-refusal.png` | "Sign-in could not be completed. Please try again." |
| Sign in | Narrow | `states/login--narrow.png` | the 45×23 px toggle at the field's top edge (T328) |
| Sign out | Confirmation | `states/logout-confirm--default.png` | |
| Sign out | Narrow | `states/logout-confirm--narrow.png` | |
| My account | Two roles | `states/profile--loaded.png` | |
| My account | Saved | `states/profile--saved.png` | "Profile saved." |
| My account | Invalid | `states/profile--invalid.png` | "The LastName field is required." twice, before T335 moved the save to an endpoint. Today a blank name is stopped by the browser, and a space is refused with "Enter your first name and your last name." (`act-A/A.4.1-2-space-refused.png`) |
| My account | Pending trainee | `states/profile--pending-trainee.png` | Roles reads PendingTrainee |
| My account | Loading | `states/profile--loading.png` | |
| My account | Load error | `states/profile--load-error.png` | |
| My account | Narrow | `states/profile--narrow.png` | |
| Change password | Blank | `states/change-password--blank.png` | |
| Change password | Incorrect | `states/change-password--incorrect.png` | "Incorrect password." |
| Change password | Confirmation | `states/change-password--confirmation.png` | "The password confirmation does not match." |
| Change password | Rules | `states/change-password--rules.png` | one run-on paragraph today; the step asks for one sentence per rule |
| Change password | Updated | `states/change-password--updated.png` | "Password updated." |
| Change password | Locked | `states/change-password--locked.png` | |
| Change password | Narrow | `states/change-password--narrow.png` | |
| Forgot password | Stub | `states/forgot-password--stub.png` | "Password reset is not wired yet in the rewrite. …" |
| Forgot password | Narrow | `states/forgot-password--narrow.png` | |
| Link an institutional sign-in | Expired | `states/link-external-login--expired.png` | "Your institutional sign-in has expired. Start again from the sign-in page." |
| Link an institutional sign-in | Narrow | `states/link-external-login--narrow.png` | |

Not captured, so describe them in words:
- **Change password, throttled.** `states.md` names `change-password--throttled.png`, but no such file was captured.
  Its words: "Too many attempts from this network. Please wait a few minutes and try again."
  (`src/Wombat.Web/Security/ChangePasswordOutcome.cs:67`).
- **Change password for an institutional-only account.** Its words: "This account signs in through your institution,
  so it has no password to change here." (`ChangePasswordOutcome.cs:70–71`). T286 would hide the link to the page for
  such an account instead.
- **Everything that needs an identity provider** (`states.md` § States no local replay reaches): the sign-in page's
  "or" divider and "Sign in with …" buttons, the link page's password form and its refusals, and the twelve
  institutional refusal texts as they would appear after a real attempt. The texts themselves can be shown with typed
  codes (`states.md` § Typed codes on the account pages).

## 4. Attach

Paths are relative to `design/baseline/`. The key screenshots were checked on 2026-09-28, each opened and compared
with what it is cited for; all are T335's replay on flow 01's shell.

**Key screenshots (attach these first, with § 1):**
1. `act-A/A.7.14-1-login.png` (sign in, blank)
2. `act-A/A.6.5-2-locked-out.png` (the lockout refusal)
3. `act-A/A.4.3-1-session-ended.png` (session ended, in the danger alert)
4. `act-A/A.7.12-1-login-390.png` (sign in at 390 px)
5. `act-A/A.4.2-3-rules.png` (the run-on rules refusal; three toggles named "Show")
6. `act-A/A.4.1-3-profile-saved.png` (My account saved; roles by code)
7. `act-A/A.4.1-2-space-refused.png` (My account refused)
8. `act-A/A.4.4-1-forgot-password-stub.png` (the stub)
9. `act-A/A.4.7-1-sign-out-page.png` (the broken sign-out confirmation)
10. `act-A/A.7.14-4-danger-alert.png` (change password's danger alert)

**New-shell captures of other states (attach as the chat asks):** `act-A/A.3.2-2-wrong-password.png`,
`act-A/A.3.2-3-unknown-provider.png`, `act-A/A.3.3-1-callback-unavailable.png`, `act-A/A.3.3-2-link-expired.png`,
`act-A/A.4.2-1-incorrect.png`, `act-A/A.4.2-2-confirmation.png`, `act-A/A.4.2-4-updated.png`,
`act-A/A.7.12-2-forgot-390.png`, `act-2/2.41-2-zulu-account.png`, `act-2/2.41-16-khumalo-narrow.png`. No new capture
shows a pending trainee's account; use `states/profile--pending-trainee.png`.

**Old-shell states (2026-09-26; attach only for a state with no new capture, and say it predates the shell):**
- Sign in: `states/login--refused.png`, `states/login--too-many-attempts.png`, `states/login--fields-missing.png`,
  `states/login--password-changed.png`, `states/login--external-unavailable.png`, `states/login--external-expired.png`,
  `states/login--sso-unknown-provider.png`, `states/login--general-refusal.png`
- Sign out: `states/logout-confirm--narrow.png`
- My account: `states/profile--loaded.png`, `states/profile--pending-trainee.png`, `states/profile--loading.png`,
  `states/profile--load-error.png`, `states/profile--narrow.png`
- Change password: `states/change-password--blank.png`, `states/change-password--incorrect.png`,
  `states/change-password--confirmation.png`, `states/change-password--updated.png`,
  `states/change-password--locked.png`, `states/change-password--narrow.png`
- Forgot password: `states/forgot-password--narrow.png`
- Link: `states/link-external-login--expired.png`, `states/link-external-login--narrow.png`

**Held:** none. **Never attach** the register captures from the invitation flow here: an invitation link may be
visible in them (BRIEF.md § 3.3).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists.

| Task | What it means for the design | Evidence (under `design/baseline/`) |
|---|---|---|
| **T328** | Every target is at least 24 px. The toggle is 28 px and centred since T335; it still needs `aria-pressed` and a name for the input it controls, so Change password's three are distinct, and its label outside the wrapper. | `act-A/A.7.12-1-login-390.png`, `act-A/A.4.2-3-rules.png` |
| **Static pages** (DESIGN.md:1887–1891, 1919–1921) | Sign-in, forgot password and link have no live connection. `PasswordToggleButton` uses `@onclick`, so on the sign-in page it has never worked. A toggle there is a `wwwroot/wombat.js` handler (which already holds `togglePasswordVisibility`) wired without an inline `onclick`, or it goes. Change password is interactive, so its toggles work. | `states/login--blank.png` |
| **Form posts** (DESIGN.md:1925–1933) | Sign-in, sign-out, change password and link each post a real form to an endpoint, which writes the cookie and redirects with a code (`?error=…`, `?status=updated`). The page picks its words from the code, so the design's refusal slot holds one of a fixed set of sentences, never free text from the address. | `src/Wombat.Web/Security/SignInOutcome.cs`, `ChangePasswordOutcome.cs` |
| **T322** (done in T335) | The alerts and the validation text already reach 4.5:1 on flow 01's tokens. Keep them there. | `act-A/A.7.14-3-success-alert.png`, `act-A/A.7.14-4-danger-alert.png` |
| **Sign-out page** (T335's replay) | The confirmation puts `PageHeader` and a `.form-container` on the anonymous card's layout, which lays them side by side, the card at full height. | `act-A/A.4.7-1-sign-out-page.png` |
| **Session ended** | The notice is a danger alert, the colour of a refusal. Flow 01 left "session ended" to the flow that meets it (BRIEF.md § 5.1). | `act-A/A.4.3-1-session-ended.png` |
| **T324** | Validation names the field by its label; roles by their labels; the password rules as short sentences in plain words. | `act-A/A.4.1-3-profile-saved.png` (roles), `act-A/A.4.2-3-rules.png` |
| **T317** | A typed `GET /account/logout` gets a page, not a 405. | T317's symptom (Steps 2.8, A.4.7) |
| **T287** | One refusal for a wrong password, an unknown address and a locked account, unless the operator decides a locked person is told (question for the operator, not the design). The design keeps one refusal slot either way. | `act-A/A.6.5-2-locked-out.png` (Step A.6.5) |
| **T286** | If My account lists institutional sign-ins (question 4), each has Remove. Change password is not offered to an account without a local password. | T286's symptom |
| **T329** | My account's loading and load-error states use F01's shared states. | `states/profile--loading.png`, `states/profile--load-error.png` |

## 6. Questions the design must answer

1. **What does the sign-in page say about the product?** A line of purpose, the College's and the institution's
   names? And do the SSO buttons sit above or below the password form? Consultants at a hospital that has SSO would
   use it daily (inference); no provider is configured anywhere yet.
2. **Forgot password.** Design a self-service reset by emailed link? It is not built: nothing sends the
   `PasswordResetEmail` template (`coverage.md` § Flows and states not played). Or replace the stub with copy that
   says an administrator resets passwords, which is how Step A.4.5 does it today? BRIEF.md § 7 B10 says to show a
   not-built feature as future, or leave it out.
3. **Is a sign-out confirmation page wanted at all?** `/account/logout-confirm` is reached only by typing its address
   (`coverage.md` § Reached only by address). Flow 01 put the one Sign out in the top bar, a form post that signs out
   at once; this decides whether it confirms first, and what a typed `GET /account/logout` shows (T317).
4. **Should My account show the person's roles, institution and institutional sign-ins?** It shows roles today, by
   code (`act-A/A.4.1-3-profile-saved.png`). T286 asks for the sign-ins to be listed and removable.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the 19 steps** on a fresh database, in act order: 1.1, 2.8, 2.31, 2.41, 5.19, then the appendix's A.1.13,
  A.3.2, A.3.3, A.4.1–A.4.7, A.6.5, A.6.8, A.7.12 and A.7.14. Every Expect must hold. Steps A.3.2, A.4.2, A.4.4 and
  A.6.5 quote the page's words: update them in the same task if the wording changes (BRIEF.md § 9 item 7).
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include `Account/ChangePasswordPageTests`, `Accessibility/AccessibleNamesTests` (the toggles' names),
  `Accessibility/ActionFocusTests`, `Design/AlertRoleTests`, `Design/InvalidFieldStyleTests`,
  `Design/PageShapeSmokeTests` (the Account and Anonymous shapes), `Design/DefinedClassTests`,
  `Hosting/AppAssetUrlTests` (any new script), `Security/` and `Scenario/`.
- **Re-capture** the 32 states in § 3 into `design/baseline/`, and compare them with the chosen artboards. Add
  `states/change-password--throttled.png`, which was never captured.
- **Browser check** at 1280 and 390 px, signed out and signed in. Check that the sign-in page still works with
  JavaScript blocked, and that the toggle on the sign-in page works (or is gone).

## 8. The runbook steps, verbatim (paste this second)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today. `pwd_DO_NOT_COMMIT.txt`, where it appears, is only the name of the file the
replay writes passwords to; no password is in these steps.

```text
Step 1.1 — The Administrator signs in (act-1-setup.md:50)
Role: Administrator — the platform operator (`devadmin@wombat.local`)
Route: /account/login → /
Do: Sign in with `devadmin@wombat.local` and the dev password that `DevUserSeeder` gives it.
Expect: Home is headed "Home", with "Administrator · Semester N, YYYY" under it, naming the current semester, and its
  header offers no action. No switch is offered, in the sidebar or on Home, since devadmin holds one role. The dashboard
  has two cards:
  - System health, which shows the database connection as healthy;
  - Users across institutions, which counts the seeded accounts (8, or 9 where the bootstrap Administrator exists).
  There is no Maintenance card: Curriculum progress is linked from the Curricula page's header (T335).
  The sidebar reads "Acting as Administrator", with no switch, over his menu, grouped: Home; Platform: Scheduled jobs,
  Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users, Invitations;
  Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision
  panels; then My data rights under a rule (DESIGN.md § The NavMenu). Home is lit. The top bar names him "Demo
  Administrator", with Sign out beside it.

Step 2.8 — Mr Smit registers from his link (act-2-onboarding.md:146)
Role: Anonymous — Mr Pieter Smit, holding his invitation link
Route: /account/register → / → /account/logout/submit → /account/login
Do: Open the link and enter first name Pieter, last name Smit and a password that meets the rules. Confirm it and
  register. Read the landing page, then sign out.
Expect: The page reads "Registering smit@kgk.wombat.local as Coordinator.", with the email filled in and not editable.
  The token is cleared from the address bar once the page loads. Registering signs him in and lands on Home, headed
  "Home" with "Coordinator · Semester N, YYYY" under it. Signing out (the top bar's Sign out, one press) returns him to
  the sign-in page, which reads "You have signed out."

Step 2.31 — Dr Mahlangu's open session ends, and she signs back in as a Trainee (act-2-onboarding.md:532)
Role: Trainee — Dr Nomsa Mahlangu
Route: / → /account/session-ended → /account/login → /
Do: Her tab from Step 2.18 has stayed open through her admission. Once it leaves Home, she signs in again.
Expect: Within a minute of her admission the tab moves to the sign-in page, which reads "Your session has ended. Sign in
  again.", an information notice; the focus is on Email (no institution buttons on dev). Admission changes her role,
  and a role change ends open sessions (T279). Signed in again, she sees
  "Trainee · Semester N, YYYY" under Home's heading and the trainee dashboard of Step 2.39.

Step 2.41 — Everyone reviews their account; Dr Khumalo corrects her name (act-2-onboarding.md:701)
Role: Every role in this act — each person onboarded here, signed in as themselves
Route: /account/profile → /account/profile/submit → /account/profile
Do: Each opens My account (the name in the top bar). Dr Khumalo changes her first name from "Fatma" to "Fatima" and
  saves her name.
Expect: The page reads "My account" and "Your name, your roles, and how you sign in." The Account card shows the email
  as text, the institution (Kgosi Kgari Teaching Hospital) and the roles by label, one per line ("Committee member" and
  "Assessor" for Zulu, Naidoo and Botha; "Trainee" for the registrars). How you sign in shows Password with Change
  password. Khumalo's Save name reloads the page with "Name saved." in the Your name card, which takes the focus, and
  the top bar's account row now reads "Fatima Khumalo" (the lists are checked on Mbatha's in Step 2.44).

Step A.3.2 — The sign-in page offers no institutional sign-in (appendix-cross-cutting.md:477)
Role: Anonymous — a KGK consultant
Route: /account/login → /account/sso-challenge/{providerKey} → /account/login
Do: Look for an institutional sign-in button. Sign in with a wrong password. Then open the challenge address for a
  provider called `kgk`.
Expect: Under the line "Work-based assessment for specialist training." and the heading "Sign in", the page offers an
  email, a password with its Show toggle, Remember me and the "Forgotten your password?" link. There is no "or"
  divider and no "Sign in with …" button. The wrong password reads "Invalid email or password.", which takes the
  focus, and points to no institutional button (T156); the tab's title starts "Error:". The challenge for a provider
  that is not configured returns to the sign-in page, which says "That institution's sign-in is not set up in Wombat.
  Sign in with your email and password."

Step A.3.3 — The callback and link pages with no institutional sign-in in progress (appendix-cross-cutting.md:495)
Role: Anonymous — a KGK consultant
Route: /account/sso-callback → /account/login → /account/link-external
Do: Open the callback address, then the link-your-account page, directly.
Expect: The callback returns to the sign-in page, which says "Your institution's sign-in did not complete. Sign in with
  your email and password." (the words of a page with no institution's button: none is configured). The link page is
  headed "Institutional sign-in expired" and says "Your institutional sign-in has expired. Start again from the
  sign-in page." It offers Back to sign in and no password field (T149).

Step A.4.1 — Dr Botha reviews and edits her account (appendix-cross-cutting.md:511)
Role: CommitteeMember — Dr Sarah Botha
Route: / → /account/profile → /account/profile/submit → /account/profile → /account/profile/submit → /account/profile
Do: Open My account from her name in the top bar. Clear her last name and save. Then type a single space as her last
  name and save. Then put "Botha" back and save.
Expect: On My account her name in the top bar is the current page (underlined), and nothing in the menu is lit. The
  Account card shows her address as text, her institution (Kgosi Kgari Teaching Hospital) and both her roles, one per
  line ("Committee member", "Assessor"); How you sign in shows Password with Change password. The cleared last name is
  stopped by the browser's own required-field check, and nothing is sent. The space is sent and refused: the page
  reloads with "Your name was not saved. Enter your last name." in the Your name card, which takes the focus. Last
  name is marked, empty, and names its own message, "Enter your last name."; First name reads "Sarah"; the tab reads
  "Error: My account · Wombat"; nothing is saved. Restored, Save name reloads the page with "Name saved.", which takes
  the focus (T234), and the top bar's account row names her as saved, "Sarah Botha".

Step A.4.2 — Dr Khumalo changes her password, getting it wrong first (appendix-cross-cutting.md:534)
Role: Assessor — Dr Fatima Khumalo
Route: /account/profile → /account/change-password → /account/profile
Do: Open My account (the name in the top bar), then its Change password. Try four times, and record the password (d)
  sets in `pwd_DO_NOT_COMMIT.txt`:
  - (a) a wrong current password;
  - (b) the right current password, with a new password and a confirmation that differ;
  - (c) the right current password, with a new one of 8 different lower-case letters;
  - (d) the right current password, with a new one that meets every rule.
Expect: The page, "Change password" with "Choose a new password for signing in to Wombat.", lists the six rules under
  New password before anything is typed (Step 2.9's), and each field has its Show toggle: "Show current password",
  "Show new password", "Show confirm new password". Each refusal reloads the page with empty fields. The tab's title
  starts "Error:", and "Your password was not changed." and the reason stand in the form's card above its buttons,
  taking the focus; the field the refusal concerns is marked and names it (T265, T193).
  - (a) reads "Incorrect password."; Current password is marked.
  - (b) reads "The password confirmation does not match."; Confirm new password is marked.
  - (c) reads "The new password needs:" and "At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.",
    "A symbol, such as ! or #.", in the six rules' order. It says nothing of lower-case letters, a rule it keeps. New
    password says "The new password does not meet the rules below."
  - (d) lands on My account with "Password updated." under its header, which takes the focus, and she stays signed in
    in this browser.

Step A.4.3 — Dr Khumalo's other session ends (appendix-cross-cutting.md:569)
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Sign in again."
  (T279), an information notice. The old password is refused, and "Invalid email or password." takes the notice's
  place. The new one brings her back to her Activity inbox: the return address survives the refusal.

Step A.4.4 — Dr du Plessis has forgotten his password (appendix-cross-cutting.md:582)
Role: Anonymous — Dr Pieter du Plessis
Route: /account/login → /account/forgot-password → /account/login
Do: Follow "Forgotten your password?" from the sign-in page.
Expect: A "Forgotten password" page: "A Wombat administrator can set a new password for you." then three steps ("Ask
  your Wombat administrator for a new password: the one at your institution or, if your account belongs to no
  institution, the platform's administrator. They give it to you themselves; Wombat does not email it." "Sign in with
  it." "Choose your own password on My account, under Change password."), and Back to sign in. There is no field, and
  no email is sent.

Step A.4.5 — Prof Mbatha resets his password (appendix-cross-cutting.md:600)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Set a new password of 10 characters, then one that meets every rule. Record the
  second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: The Reset password card says it sets a password directly and that the user is not emailed. Its field has the
  Show toggle ("Show new password") and the six rules under it, and Reset password is enabled once anything is typed.
  The 10-character password is refused with "The password was not reset. The new password needs:" and the rules it
  breaks, in the six rules' order. The second is accepted, and the field is cleared. The audit log records the reset
  with the password redacted (T101).

Step A.4.6 — Dr du Plessis signs in with it and chooses his own (appendix-cross-cutting.md:621)
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → / → /account/profile → /account/change-password → /account/profile
Do: Sign in with the password Prof Mbatha set. Then change it, from My account (the name in the top bar), to one of
  his own. Record it in `pwd_DO_NOT_COMMIT.txt`.
Expect: He lands on his Trainee dashboard, which still says that his programme ended (Step 5.28). The change lands on
  My account with "Password updated."

Step A.4.7 — Mr Smit signs out through the confirmation page (appendix-cross-cutting.md:635)
Role: Coordinator — Mr Pieter Smit
Route: /account/logout-confirm → / → /account/logout → / → /account/logout-confirm → /account/logout/submit → /account/login → /msf/campaigns → /account/login → /msf/campaigns
Do: Open the sign-out confirmation by its address and press Cancel. Type `/account/logout` and press Cancel. Open the
  confirmation again and sign out. Then open MSF campaigns by its address.
Expect: A "Sign out" page, with no nav: "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends
  your session in this browser.", with Cancel and Sign out. Cancel returns him to his dashboard, still signed in. A
  typed GET /account/logout draws the same page and signs nobody out (T317). Sign out lands on the sign-in page with
  "You have signed out." MSF campaigns then asks him to sign in, and after signing in he is brought back to it. The
  audit log records a Logout.

Step A.6.5 — Dr Patel's session ends, and he cannot sign in (appendix-cross-cutting.md:901)
Role: Assessor — Dr Mohammed Patel
Route: /account/session-ended → /account/login
Do: Wait up to a minute on the page he had open, then sign in.
Expect: The tab leaves for the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Signing in is refused: "Invalid email or password." (T287).

Step A.6.8 — Dr Patel signs in again (appendix-cross-cutting.md:937)
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard. "Waiting for your rating" is badged 1 and lists Dr du Plessis's Portfolio
  and Logbook Review, badged Overdue, as his inbox lists it (T297, T335). Dr Dlamini's
  assessor list names him again (checked at A.7.1).

Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again (appendix-cross-cutting.md:251)
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Sign in again." (T279), an
  information notice. Signing in is refused in the words an unknown address gets, "Invalid email or password.", with
  no mention of an erasure (T156). No email is sent to him.

Step A.7.12 — The anonymous pages on a phone (appendix-cross-cutting.md:1190)
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's portfolio PDF from Act 5 by
  its hash.
Expect: The sign-in card fits the width. Every field, toggle, button and link on the sign-in and forgot pages is
  44 px tall; Sign in is full width; the h1 is 1.5rem. The verify page's result fits the width.

Step A.7.14 — Prof Mbatha's pages, checked for contrast (appendix-cross-cutting.md:1230)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on a committee review opened from Committee reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.

Step 5.19 — Dr Molefe's open session ends (act-5-graduation.md:466)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /account/session-ended → /account/login
Do: Go back to the tab left open at Step 5.15 and wait up to a minute.
Expect: Removing her role changed her account's security stamp. The session check, which runs once a minute, ends the
  session: the tab reloads to the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Nothing she had open still acts as a Trainee.
```
