# Page states: the redesign's visual baseline

Every state a person can meet on each page Wombat serves, and exactly how a replay reaches it, so that a replay can
screenshot each one. Together with each step's own capture (README § How to play, "Screenshots"), these images are the
baseline a GUI redesign is briefed from and checked against.

Written 2026-09-26 (T293) from the code of each page, `DESIGN.md`'s page contracts and the act files. The pages are the
67 page files of `coverage.md`'s Pages table, grouped by area. A state is listed where the page really has it; one no
local replay can reach is in the last section, with the reason.

## How to capture

- **Viewport.** Desktop is 1280 × 800 at 100% zoom. "Narrow" is 390 × 844 in the browser's device mode. Capture the full
  page, except a dialog, the reconnect modal or the error banner, which are captured as the viewport shows them.
- **Settled.** Wait until no skeleton is left, unless the state is the skeleton, and until any result alert has taken
  the focus.
- **File name.** `design/baseline/states/<page-slug>--<state>.png`. The tables give the part after `design/baseline/`.
- **Page slug.** The page file's name in kebab case: `ActivityView.razor` is `activity-view`. Four exceptions keep the
  slug readable: `Admin/EntrustmentDecisions/Index.razor` is `entrustment-decisions`, `Admin/DataRights/RequestsList.razor`
  is `data-rights-requests`, `Admin/DataRights/RequestDetail.razor` is `data-rights-request` and `Account/Logout.razor` is
  `logout-confirm`. States of the layout itself use `shell`.
- **Accounts** are the README's cast. `devadmin` is `devadmin@wombat.local`. `D` is the replay day, `J` Act 2's anchor.

## Three ways a state is reached

1. **From the story.** The row names the step that shows it: "at Step 3.5" is while the step is played, "after Step
   3.5" is any time after it and before the next change to that record. Capturing it changes nothing the step does not.
2. **Inline, changing nothing.** A typed address or query code, a filter, a dialog opened and then cancelled, a form left
   invalid in the browser, or a held read (below). None of these sends a command, so no row, audit entry or mail is
   written, and every act's outcome SQL still holds. The row ends "No change."
3. **Deliberately, on a scratch database.** The state needs a change the story does not make: a refused command (which
   writes a FAILED audit row and would move the acts' audit counts), a lock and reactivation, an extra invitation,
   campaign, panel or review. Never on the replay's database. The row starts "Scratch (post-actN)":
   ```
   createdb wombat_states
   pg_restore --no-owner -d wombat_states recovery/scenario-post-act<N>.dump
   ```
   Then start a second instance on it, as README § How to play starts the replay's, on its own port
   (`--urls http://localhost:5280`, `ConnectionStrings__DefaultConnection` naming `wombat_states`, `Email__SmtpHost`
   empty, `Wombat__PseudonymSalt` set). The cast's passwords are the replay's. Drop the database afterwards.

### Holding a read: the loading and load-error states

Every list, detail and dashboard renders `StatePanel`'s skeletons while its read runs, and its danger alert when the read
fails. On a full page load the read runs in the prerender, so the skeleton is never seen. It is seen on a navigation
inside the circuit while the database is made to wait. In psql against the replay's database:

```sql
BEGIN;
DO $$
DECLARE t text;
BEGIN
  FOR t IN SELECT format('%I.%I', schemaname, tablename) FROM pg_tables WHERE schemaname = 'public' LOOP
    EXECUTE 'LOCK TABLE ' || t || ' IN ACCESS EXCLUSIVE MODE';
  END LOOP;
END $$;
-- leave the session open; ROLLBACK; releases every lock and changes nothing
```

- **Loading.** With the locks held, go to the page inside the circuit: its nav link, or `Blazor.navigateTo('/route')`
  in the browser's console. Capture the skeleton, then `ROLLBACK;`. The page then loads as usual.
- **Load error.** The same, held for 35 seconds: the database command times out at 30 (Npgsql's default; nothing in
  Wombat sets another), and the page shows its danger alert with the raw timeout text. Capture, then `ROLLBACK;`.
- **Keep a hold under two minutes.** The circuit's sign-in check runs once a minute and a hold makes it fail; only the
  third failure in a row signs the circuit out (`SessionRevalidatingAuthenticationStateProvider`). The scheduler and any
  other browser wait meanwhile.
- **Static pages** (signed out, `/msf/respond`, `/portfolio/verify`) have no circuit: a hold delays the response, and
  past 30 seconds they show their own failure state, listed on each.

### Typed codes on the account pages

The sign-in, register and change-password pages choose their words from a code in the address (`?error=`, and
`?status=updated` on change password), never from text in it (T265, T285). Typing a code shows exactly the page a real
refusal shows, with nothing sent. Where the story plays the real refusal, the row uses the story; the rest are typed.

- **Sign-in** (`/account/login?error=<code>`): `FieldsMissing`, `Refused`, `TooManyAttempts`, `LockedOut`,
  `SessionEnded`, `PasswordChanged`, `ExternalLoginUnavailable`, `ExternalSessionExpired`, `SsoFailed`, the twelve
  institutional sign-in codes (`SsoUnknownProvider`, `SsoNoEmail`, `SsoEmailNotVerified`, `SsoAccountLocked`,
  `SsoAdministrator`, `SsoWrongInstitution`, `SsoEmailInUse`, `SsoAccountNotCreated`, `SsoAlreadyLinked`,
  `SsoLinkRefused`, `SsoLinkLockedOut`, `SsoLinkFailed`), and any other code, which gets the general refusal.
- **Change password** (`/account/change-password?error=<code>`, repeatable): `FieldsMissing`, `ConfirmationMismatch`,
  `Failed`, `LockedOut`, `TooManyAttempts`, `InstitutionalSignIn`, and Identity's `PasswordMismatch`,
  `PasswordTooShort`, `PasswordRequiresDigit`, `PasswordRequiresUpper`, `PasswordRequiresLower`,
  `PasswordRequiresNonAlphanumeric`.
- **Register** (`/account/register?token=<a usable token>&error=<code>`): `ConfirmationMismatch`, `DetailsInvalid`,
  `Failed` and Identity's password codes, shown under the form. With an unusable token the page says why the invitation
  cannot be used, whatever the code.

## Shell and framework

The layout, the nav and what the Blazor runtime shows. Each acting role's menu is captured with its dashboard (Home,
below), and so are the shell's other states: "Awaiting admission" and a graduate's "Your training record" ("No role
assigned" is under States no local replay reaches). The sidebar's role head carries the switch (T335, flow 01): "Switch to …" beside a second role, "Change role"
beside two or more. A session that ends lands on the sign-in page by a full load of the mapped endpoint
`/account/session-ended`, so its state is the sign-in page's ("Session ended", under Account and sign-in).

| State | Screenshot | Account | How to reach it |
|---|---|---|---|
| Nav folded at narrow width | `states/shell--nav-folded.png` | Dr Dlamini (Trainee) | At Step A.7.3, at 390 px, before opening the menu: the phone bar, with the brand, "Acting as Trainee" and Menu. No change. |
| Nav open at narrow width | `states/shell--nav-open.png` | Dr Dlamini (Trainee) | At Step A.7.3, at 390 px, press Menu: Close in the bar, the role head, the 44px rows, and her name and Sign out at the foot. No change. |
| A long role in the folded bar | `states/shell--nav-folded-long-role.png` | Dr Sithole (SubSpecialityAdmin) | At Step A.7.8, at 390 px: "Sub-speciality admin" in two lines, the bar taller. No change. |
| The Administrator's menu open at narrow width | `states/shell--nav-open-admin.png` | devadmin | At Step A.7.11, at 390 px, press Menu and scroll the list: the head and the foot stay. No change. |
| The Administrator's sidebar scrolled | `states/shell--sidebar-scrolled.png` | devadmin | At 1280 × 700, open Decision panels: the list scrolls it into view under the fixed brand cell and role head. No change. |
| Two roles: the switch | `states/shell--switch.png` | Dr Zulu (CommitteeMember + Assessor) | At Step 2.33: "Acting as Committee member" with "Switch to Assessor" under it. No change. |
| Three or more roles: Change role open | `states/shell--change-role.png` | an account holding three roles | Give a copy's Dr Zulu a third role (a scratch database), sign in and open Change role: a switch link for each other role. |
| No role | `states/shell--no-role.png` | Dr Molefe after Step 5.17 | At Step 5.21: no "Acting as" head; Home, then My progress and My data rights. No change. |
| The current item under a list | `states/shell--owner-lit.png` | Mr Smit | At Step 3.41, a campaign's MSF report: MSF campaigns lit (T331), and the trail Home › MSF campaigns › MSF report. No change. |
| My account current | `states/shell--my-account.png` | Dr Botha | At Step A.4.1: the name in the top bar underlined, nothing in the menu lit. No change. |
| The trail folded at narrow width | `states/shell--trail-folded.png` | Dr Zulu | At Step A.7.6, at 390 px, a review: one 44px link back to Committee reviews. No change. |
| Reconnect: rejoining | `states/shell--reconnect-rejoining.png` | Dr Zulu (CommitteeMember) | With her dashboard open, suspend the replay's `Wombat.Web` process (Process Explorer's Suspend, or `NtSuspendProcess`), wait about 30 s for the client's server timeout, capture "Reconnecting" with "The connection to Wombat dropped. Reconnecting now." over a running bar, alone in the dialog, then resume the process. Stopping it does not hold this state: the runtime's first ten attempts are refused at once and the dialog moves to "Could not reconnect" within milliseconds (T295 sweep). No change. |
| Reconnect: retrying | `states/shell--reconnect-retrying.png` | Dr Zulu | Stop the app: "Could not reconnect" and "Trying again in N seconds.", the count running down a second at a time, with nothing above it (T330). No change. |
| Reconnect: attempt started | `states/shell--reconnect-attempt-started.png` | Dr Zulu | With the app stopped and the count running, listen on its port without answering (PowerShell: `$l = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, <port>); $l.Start()`). At the next attempt the line reads "Trying again now." over a running bar, under the same "Could not reconnect", which keeps the focus (one state, its line replaced), and holds while the attempt waits on the listener. Capture it, then `$l.Stop()` (needs confirmation: that the attempt waits rather than fails). No change. |
| Reconnect: failed | `states/shell--reconnect-failed.png` | Dr Zulu | Keep the app stopped until the attempts run out (about six minutes: ten at once, ten 5 s apart, ten 30 s apart): "Connection lost", "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and anything not yet saved on it is lost." and Try again. Restart the app and press Try again: the circuit is gone and the restarted server kept nothing to resume it from, so "Reloading" and "Reloading the page…" show as the page reloads. No change. |
| Reconnect: failed at narrow width | `states/shell--reconnect-failed-narrow.png` | Dr Zulu | As Reconnect: failed, at 390 px: the dialog 358 px wide, 16 px from each side, and Try again as wide as the dialog. No change. |
| Reconnect: paused | `states/shell--reconnect-paused.png` | Dr Zulu | In the browser's console run `Blazor.pauseCircuit()`: "Page paused" and "This page is paused. Resume to carry on." with Resume (needs confirmation: the console call is .NET 10's). Resume restores the page. No change. |
| Reconnect: resume failed | `states/shell--reconnect-resume-failed.png` | Dr Zulu | Pause as above, stop the app, press Resume: "Could not resume", the sentence Connection lost gives, and Try again, which takes the focus from Resume. Restart the app and press Try again: the page resumes, or shows "Reloading the page…" and reloads when the server has nothing to resume it from (needs confirmation: which). No change. |
| Unhandled error banner | `states/shell--error-banner.png` | Dr Dlamini (Trainee) | New activity reads its type list with no error handling (`NewActivity.OnInitializedAsync`). Hold a read, open Log an activity from the menu, and wait about 65 s: its first read (the programme start) times out and is passed over, then the type list's times out and the bar "This page no longer responds; copy anything you need, then reload." with Reload and Dismiss, small (`btn-sm`) buttons with their refresh-cw and x icons, appears beside the sidebar as the circuit ends. `ROLLBACK;`, then Reload. No change. |
| Unhandled error banner at narrow width | `states/shell--error-banner-narrow.png` | Dr Dlamini (Trainee) | As above, at 390 px: the sentence first, then Reload and Dismiss side by side, each half the bar, 44 px high and without their icons. `ROLLBACK;`, then Reload. No change. |

## Home and the role dashboards

`/` (Home.razor) is headed "Home", with "{acting role} · Semester N, YYYY" under it and the role's header action, if it
has one. It hosts one dashboard per role (`Pages/Dashboards/*`), under the shell's sidebar and its switch (T335, flow
01). The mapped endpoint `/dashboard/switch/{role}` stores the acting role with the account, which chooses the
dashboard Home shows, and the page it lands on says so once ("You are now acting as …", T335).

| State | Screenshot | Account | How to reach it |
|---|---|---|---|
| Administrator | `states/home--administrator.png` | devadmin | At Step 1.1: System health (with the job status), Users across institutions. |
| CollegeAdmin | `states/home--college-admin.png` | Dr Kruger | At Step 1.8: the one National catalogue card. |
| InstitutionalAdmin, before any adoption | `states/home--institutional-admin-first.png` | Prof Mbatha | At Step 1.10: Users lists InstitutionalAdmin 1. |
| InstitutionalAdmin, onboarded | `states/home--institutional-admin.png` | Prof Mbatha | At Step 2.44. |
| Coordinator, nothing waiting | `states/home--coordinator-empty.png` | Mr Smit | At Step 2.32: "No stalled requests.", "No invitations expiring soon.", Start an MSF campaign. |
| Coordinator, stalled requests | `states/home--coordinator-stalled.png` | Mr Smit | At Step 3.30, after its ageing SQL: the warning card lists the waiting requests. |
| Coordinator, invitations nearing expiry | `states/home--coordinator-expiring.png` | Mr Smit | Scratch (post-appendix): Prof Mbatha invites `expiring@kgk.wombat.local` as Trainee at KGK, Paediatrics / Paediatrics; then `UPDATE "Invitations" SET "ExpiresOn" = now() + interval '2 days' WHERE "Email" = 'expiring@kgk.wombat.local';`. The card lists it with its expiry (the card shows three days ahead). |
| CommitteeMember, two roles | `states/home--committee-member.png` | Dr Zulu | At Step 2.33: "Committee member · Semester N, YYYY", five registrars at 0. |
| CommitteeMember, nobody to show | `states/home--committee-member-empty.png` | Dr van Rensburg | At Step 2.37: "No trainees have targets this period." |
| CommitteeMember with figures | `states/home--committee-member-figures.png` | Dr Zulu | At Step 3.52, after switching back. |
| Assessor view after a switch | `states/home--assessor-switched.png` | Dr Zulu | At Step 2.34: "You are now acting as Assessor.", "Assessor · Semester N, YYYY", every card empty. |
| Assessor, first sign-in | `states/home--assessor-empty.png` | Dr Patel | At Step 2.36: "Waiting for your rating" badged 0, "No decisions yet." |
| Assessor with a request waiting | `states/home--assessor-pending.png` | Dr Zulu | At Step 3.33, after the switch: "Waiting for your rating" badged 1. |
| Assessor with recent decisions | `states/home--assessor-decisions.png` | Dr Khumalo | At Step 3.51: Completed in green, Declined in red. |
| SpecialityAdmin | `states/home--speciality-admin.png` | Dr Mokoena | At Step 2.38. |
| SpecialityAdmin with figures | `states/home--speciality-admin-figures.png` | Dr Mokoena | At Step 3.53. |
| SubSpecialityAdmin | `states/home--sub-speciality-admin.png` | Dr Sithole | At Step 3.54. |
| Trainee awaiting admission | `states/home--awaiting-admission.png` | Dr Mahlangu (PendingTrainee) | At Step 2.18: the one Awaiting admission card, and the pending trainee's nav. |
| Trainee, admitted, nothing filed | `states/home--trainee-first.png` | Dr Molefe | At Step 2.39: "0 / 10", "0 / 5", the five largest shortfalls. |
| Trainee with activity | `states/home--trainee.png` | Dr Dlamini | At Step 3.50. |
| Trainee with work returned | `states/home--trainee-returned.png` | Dr Ndlovu | At Step 3.16: the Activity inbox card lists the reflection as Draft. |
| Trainee whose programme ended | `states/home--trainee-ended.png` | Dr du Plessis | At Step 5.28: "Your programme ended on …, so no target applies to you any more." |
| Former trainee, no role | `states/home--no-role.png` | Dr Molefe (former trainee) | At Step 5.21: no subtitle, and one card, "Your training record", pointing to My progress (T335; the step's expectation, the ended line, is T311's). |
| Loading | `states/home--loading.png` | Dr Dlamini | Hold a read, then choose Home in the menu: the header, each card's title and a skeleton in each card, and nothing to press in the cards. No change. |
| Load error | `states/home--load-error.png` | Dr Dlamini | The same, held 35 s: one alert, "Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.", with Try again, and no cards. `ROLLBACK;`, then Try again shows the cards. No change. |
| Narrow: trainee | `states/home--narrow-trainee.png` | Dr Dlamini | At Step A.7.3. |
| Narrow: former trainee | `states/home--narrow-former-trainee.png` | Dr Molefe | At Step A.7.4. |
| Narrow: assessor | `states/home--narrow-assessor.png` | Dr Patel | At Step A.7.2, open Home at 390 px before the inbox. No change. |
| Narrow: coordinator | `states/home--narrow-coordinator.png` | Mr Smit | At Step A.7.5. |
| Narrow: committee member | `states/home--narrow-committee-member.png` | Dr Zulu | At Step A.7.6. |
| Narrow: speciality admin | `states/home--narrow-speciality-admin.png` | Dr Mokoena | At Step A.7.7. |
| Narrow: sub-speciality admin | `states/home--narrow-sub-speciality-admin.png` | Dr Sithole | At Step A.7.8. |
| Narrow: institutional admin | `states/home--narrow-institutional-admin.png` | Prof Mbatha | At Step A.7.9. |
| Narrow: college admin | `states/home--narrow-college-admin.png` | Dr Kruger | At Step A.7.10. |
| Narrow: administrator | `states/home--narrow-administrator.png` | devadmin | At Step A.7.11. |

## System pages

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/access-denied` | Signed in | `states/access-denied--signed-in.png` | Dr Kruger | At Step 1.15. |
| `/access-denied` | Signed out | `states/access-denied--signed-out.png` | Anonymous | Signed out, type `/access-denied`: the static page, under the signed-out bar with Sign in: "Sign in to open this page" and "Sign in to carry on.", with Sign in; with `?ReturnUrl=%2Fadmin%2Faudit`, "After you sign in, Wombat brings you back to the page you asked for." (T335). No change. |
| `/access-denied` | Narrow | `states/access-denied--narrow.png` | Dr Dlamini | At Step A.5.1, at 390 px. No change. |
| `/not-found` | Signed in | `states/not-found--signed-in.png` | Prof Mbatha | At Step 1.23 (the Demo Institution's id). |
| `/not-found` | Signed out | `states/not-found--signed-out.png` | Anonymous | Signed out, type `/not-found`: the card under the signed-out bar, "There is no page at this address." and Go to Home, with no "Check the address" line. No change. |
| `/not-found` | From a data-rights download that is not his | `states/not-found--download.png` | Mr Smit | At Step A.1.4. |
| `/not-found` | Narrow | `states/not-found--narrow.png` | Dr Dlamini | At Step A.5.4, at 390 px. No change. |
| `/Error` | Typed, signed in | `states/error--signed-in.png` | Dr Dlamini | At Step A.5.8: "Nothing went wrong", with no reference and no Try again (T335). |
| `/Error` | Typed, signed out | `states/error--signed-out.png` | Anonymous | Signed out, type `/Error`: the same words in the signed-out card, with no sign-in first (T321). No change. |
| `/Error` | After a failure | `states/error--failed.png` | Dr Dlamini | Outside Development only: on dev the developer exception page answers instead. Start the app with another environment (the connection string then comes from `ConnectionStrings__DefaultConnection`), hold a read (§ Holding a read), and load `/activities/new` by its address: its prerender's type list times out (T329) and the request answers 500 with "Something went wrong", the reference and the time in SAST, Try again and Go to Home. The log has the failure with the same reference. `ROLLBACK;`, then Try again (needs confirmation: that the prerender's read fails the request). `Hosting/ErrorPageFlowTests` plays it with a failing test endpoint. No change. |
| `/Error` | Narrow | `states/error--narrow.png` | Dr Dlamini | At Step A.5.8, at 390 px. No change. |

## Account and sign-in

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/account/login` | Blank | `states/login--blank.png` | Anonymous | At Step 1.1, before signing in. No SSO button (no provider). |
| `/account/login` | Wrong password | `states/login--refused.png` | Anonymous | At Step A.3.2: "Invalid email or password." |
| `/account/login` | Session ended | `states/login--session-ended.png` | Dr Mahlangu | At Step 2.31: "Your session has ended. Please sign in again." Also typed: `?error=SessionEnded`. |
| `/account/login` | Locked out | `states/login--locked-out.png` | Dr Patel | At Step A.6.5: "Too many failed sign-in attempts. Please try again later or reset your password." |
| `/account/login` | Unknown SSO provider | `states/login--sso-unknown-provider.png` | Anonymous | At Step A.3.2, after the challenge for `kgk`. |
| `/account/login` | No external sign-in | `states/login--external-unavailable.png` | Anonymous | At Step A.3.3: "External login information was not available." |
| `/account/login` | Throttled | `states/login--too-many-attempts.png` | Anonymous | Typed: `?error=TooManyAttempts`. No change. |
| `/account/login` | Fields missing | `states/login--fields-missing.png` | Anonymous | Typed: `?error=FieldsMissing`. No change. |
| `/account/login` | Password changed elsewhere | `states/login--password-changed.png` | Anonymous | Typed: `?error=PasswordChanged`. No change. |
| `/account/login` | External sign-in expired | `states/login--external-expired.png` | Anonymous | Typed: `?error=ExternalSessionExpired`. No change. |
| `/account/login` | Each institutional sign-in refusal | `states/login--sso-<code>.png` | Anonymous | Typed, one capture per code of § Typed codes, `SsoNoEmail` to `SsoLinkFailed`, and `SsoFailed`. No change. |
| `/account/login` | Unknown code | `states/login--general-refusal.png` | Anonymous | Typed: `?error=Call012`: "Sign-in could not be completed. Please try again." No change. |
| `/account/login` | Narrow | `states/login--narrow.png` | Anonymous | At Step A.7.12. |
| `/account/register` | No token | `states/register--token-missing.png` | Anonymous | Typed: `/account/register`: "The invitation token is missing." No change. |
| `/account/register` | The form | `states/register--form.png` | Anonymous | At Step 1.8, before Register: "Registering kruger@cmsa.wombat.local as CollegeAdmin." |
| `/account/register` | Password too short | `states/register--password-short.png` | Anonymous | At Step 2.9, the first attempt. |
| `/account/register` | Confirmation differs | `states/register--confirmation.png` | Anonymous | At Step 2.9, the second attempt. |
| `/account/register` | Details invalid | `states/register--details-invalid.png` | Anonymous | After Step 2.26, before 2.27: Dr du Plessis's resent link with `&error=DetailsInvalid` added. No change. |
| `/account/register` | Used | `states/register--used.png` | Anonymous | At Step 1.9. |
| `/account/register` | Revoked | `states/register--revoked.png` | Anonymous | At Step 2.17. |
| `/account/register` | Invalid | `states/register--invalid.png` | Anonymous | At Step 2.27, the replaced link. |
| `/account/register` | Expired | `states/register--expired.png` | Anonymous | Scratch (post-appendix): Prof Mbatha invites `expired@kgk.wombat.local`; copy the link; `UPDATE "Invitations" SET "ExpiresOn" = now() - interval '1 day' WHERE "Email" = 'expired@kgk.wombat.local';`; open the link: "This invitation has expired." |
| `/account/register` | Address already has an account | `states/register--account-exists.png` | Anonymous | Scratch (post-appendix): Prof Mbatha invites `botha@kgk.wombat.local` as Assessor; open the link: "A user with this email address already exists." |
| `/account/register` | Could not be completed | `states/register--general-refusal.png` | Anonymous | Hold a read, open any registration link (static), wait 35 s: "Registration could not be completed. Please try again." No change. |
| `/account/register` | Narrow | `states/register--narrow.png` | Anonymous | At Step 2.18, Dr Molefe's link at 390 px before registering. No change. |
| `/account/forgot-password` | The stub | `states/forgot-password--stub.png` | Anonymous | At Step A.4.4. |
| `/account/forgot-password` | Narrow | `states/forgot-password--narrow.png` | Anonymous | At Step A.7.12. |
| `/account/link-external` | No sign-in in progress | `states/link-external-login--expired.png` | Anonymous | At Step A.3.3: "Your institutional sign-in has expired. Start again from the sign-in page." |
| `/account/link-external` | Narrow | `states/link-external-login--narrow.png` | Anonymous | At Step A.3.3, at 390 px. No change. |
| `/account/logout-confirm` | The confirmation | `states/logout-confirm--default.png` | Mr Smit | At Step A.4.7, the first visit. |
| `/account/logout-confirm` | Narrow | `states/logout-confirm--narrow.png` | Mr Smit | At Step A.4.7, at 390 px, then Cancel. No change. |
| `/account/profile` | Before admission | `states/profile--pending-trainee.png` | Dr Mahlangu | At Step 2.19: Roles reads PendingTrainee. |
| `/account/profile` | Two roles | `states/profile--loaded.png` | Dr Zulu | At Step 2.41. |
| `/account/profile` | Saved | `states/profile--saved.png` | Dr Khumalo | At Step 2.41: "Profile saved." |
| `/account/profile` | Required field empty | `states/profile--invalid.png` | Dr Botha | At Step A.4.1, the last name of a single space: the alert "Enter your first name and your last name.", which takes the focus. The cleared last name before it is stopped by the browser's own required-field check, and nothing is sent. |
| `/account/profile` | Loading | `states/profile--loading.png` | Dr Botha | Hold a read, then My account from the name in the top bar. No change. |
| `/account/profile` | Load error | `states/profile--load-error.png` | Dr Botha | The same, held 35 s. No change. |
| `/account/profile` | Narrow | `states/profile--narrow.png` | Dr Khumalo | At Step 2.41, at 390 px. No change. |
| `/account/change-password` | Blank | `states/change-password--blank.png` | Dr Khumalo | At Step A.4.2, before the first try. |
| `/account/change-password` | Wrong current password | `states/change-password--incorrect.png` | Dr Khumalo | At Step A.4.2 (a): the tab title starts "Error:". |
| `/account/change-password` | Confirmation differs | `states/change-password--confirmation.png` | Dr Khumalo | At Step A.4.2 (b). |
| `/account/change-password` | Rules broken | `states/change-password--rules.png` | Dr Khumalo | At Step A.4.2 (c): one sentence per rule. |
| `/account/change-password` | Updated | `states/change-password--updated.png` | Dr Khumalo | At Step A.4.2 (d): "Password updated." |
| `/account/change-password` | Locked or throttled | `states/change-password--locked.png` | Dr Khumalo | Typed: `?error=LockedOut`, then `?error=TooManyAttempts` (two captures, `--locked` and `--throttled`). No change. |
| `/account/change-password` | Narrow | `states/change-password--narrow.png` | Dr du Plessis | At Step A.4.6, at 390 px, before changing it. No change. |
| `/account/data-rights` | Nothing requested | `states/data-rights--empty.png` | Dr Dlamini | At Step A.1.1, before saving: "No requests". |
| `/account/data-rights` | Preferences saved | `states/data-rights--saved.png` | Dr Dlamini | At Step A.1.1. |
| `/account/data-rights` | No reason given | `states/data-rights--reason-missing.png` | Dr Dlamini | At Step A.1.2, the first submit. |
| `/account/data-rights` | Submitted | `states/data-rights--submitted.png` | Dr Dlamini | At Step A.1.2: the row with Withdraw. |
| `/account/data-rights` | Completed, with Download | `states/data-rights--completed.png` | Dr Dlamini | At Step A.1.5. |
| `/account/data-rights` | Withdrawn | `states/data-rights--withdrawn.png` | Dr du Plessis | At Step A.1.6. |
| `/account/data-rights` | Rejected | `states/data-rights--rejected.png` | Dr Mahlangu | At Step A.1.9. |
| `/account/data-rights` | Loading | `states/data-rights--loading.png` | Dr Dlamini | Hold a read, then My data rights in the menu. No change. |
| `/account/data-rights` | Narrow | `states/data-rights--narrow.png` | Dr Dlamini | At Step A.7.3. |

## Activities

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/activities/new` | No type chosen | `states/new-activity--blank.png` | Dr Dlamini | At Step 3.1, before choosing the type. |
| `/activities/new` | Before admission | `states/new-activity--pending-trainee.png` | Dr Mahlangu (PendingTrainee) | At Step 2.19. |
| `/activities/new` | A rated form | `states/new-activity--mini-cex.png` | Dr Molefe | At Step 2.43: Request open, Entrustment and Feedback locked. |
| `/activities/new` | An unrated form | `states/new-activity--reflective.png` | Dr Ndlovu | At Step 3.14, before submitting: no Entrustment section, Discussion locked. |
| `/activities/new` | A self-logged form | `states/new-activity--teaching-log.png` | Dr du Plessis | At Step 3.18, before submitting. |
| `/activities/new` | Refused: a future date | `states/new-activity--refused-future.png` | Dr Ndlovu | At Step 3.8: "Nothing was saved. …", the date field marked. |
| `/activities/new` | Hint: before the programme | `states/new-activity--before-programme.png` | Dr Ndlovu | At Step 3.9, as `J−1d` is typed. |
| `/activities/new` | Refused: before the programme | `states/new-activity--refused-before-programme.png` | Dr Ndlovu | At Step 3.9, after Submit. |
| `/activities/new` | Late filing warning | `states/new-activity--late-warning.png` | Dr Ndlovu | At Step 3.12, as `D−20` is typed. |
| `/activities/new` | Picker without a paused EPA | `states/new-activity--paused-epa.png` | Dr Mahlangu | At Step 6.20, the EPA picker open. |
| `/activities/new` | Loading (no skeleton) | `states/new-activity--loading.png` | Dr Dlamini | Hold a read, open Log an activity from the menu, capture within 20 s: the header and an empty type select. `ROLLBACK;`. No change. |
| `/activities/new` | Narrow | `states/new-activity--narrow.png` | Dr Dlamini | At Step A.6.6, at 390 px, with a Mini-CEX chosen. No change. |
| `/activities/{ActivityId:int}` | Draft saved | `states/activity-view--draft-saved.png` | Dr Dlamini | At Step 3.1: "Draft saved. It has not been submitted." |
| `/activities/{ActivityId:int}` | Refused submit | `states/activity-view--refused-submit.png` | Dr Dlamini | At Step 3.2: "Presenting problem: A value is required.", the field marked. |
| `/activities/{ActivityId:int}` | Saved as a draft, not submitted | `states/activity-view--not-submitted.png` | Dr Dlamini | At Step A.7.1, after the empty Submit. |
| `/activities/{ActivityId:int}` | Requested, the trainee's view | `states/activity-view--requested.png` | Dr Dlamini | At Step 3.3: read-only, Cancel only. |
| `/activities/{ActivityId:int}` | Submitted notice, late | `states/activity-view--submitted-late.png` | Dr Ndlovu | At Step 3.10: "Submitted. It is now Requested.", the history's "Filed 20 days after the encounter". |
| `/activities/{ActivityId:int}` | Unavailable | `states/activity-view--unavailable.png` | Dr Patel | At Step 3.4. |
| `/activities/{ActivityId:int}` | Assessor to rate | `states/activity-view--to-rate.png` | Dr Naidoo | At Step 3.5, before rating: Entrustment and Feedback open, Complete and Decline. |
| `/activities/{ActivityId:int}` | Completed | `states/activity-view--completed.png` | Dr Dlamini | At Step 3.6. |
| `/activities/{ActivityId:int}` | Decline note open | `states/activity-view--decline-note.png` | Dr Khumalo | At Step 3.11, Decline pressed. |
| `/activities/{ActivityId:int}` | Decline without a note | `states/activity-view--decline-refused.png` | Dr Khumalo | At Step 3.11: "Decline requires a note." |
| `/activities/{ActivityId:int}` | Declined | `states/activity-view--declined.png` | Dr Ndlovu | At Step 3.12, the declined request. |
| `/activities/{ActivityId:int}` | Awaiting discussion | `states/activity-view--awaiting-discussion.png` | Dr Botha | At Step 3.15, before returning: Record Discussion and Return. |
| `/activities/{ActivityId:int}` | Returned to the trainee | `states/activity-view--returned.png` | Dr Ndlovu | At Step 3.16, before submitting again. |
| `/activities/{ActivityId:int}` | Discussed | `states/activity-view--discussed.png` | Dr Ndlovu | After Step 3.17. |
| `/activities/{ActivityId:int}` | Logged | `states/activity-view--logged.png` | Dr du Plessis | At Step 3.18. |
| `/activities/{ActivityId:int}` | Cancelled | `states/activity-view--cancelled.png` | Dr du Plessis | At Step 3.20. |
| `/activities/{ActivityId:int}` | Awaiting review | `states/activity-view--awaiting-review.png` | Dr du Plessis | At Step 3.22. |
| `/activities/{ActivityId:int}` | Credited nothing, EPA paused | `states/activity-view--credited-nothing.png` | Dr Patel | At Step 6.18, after Complete: the warning, and "(no longer in use)" on the EPA. |
| `/activities/{ActivityId:int}` | Credited nothing, after the last day | `states/activity-view--after-programme-end.png` | Dr du Plessis | After Step 5.27, his `D−1` Mini-CEX: the same warning, credit None. |
| `/activities/{ActivityId:int}` | Loading | `states/activity-view--loading.png` | Dr Dlamini | Hold a read, open an activity from My activities. No change. |
| `/activities/{ActivityId:int}` | Load error | `states/activity-view--load-error.png` | Dr Dlamini | The same, held 35 s. No change. |
| `/activities/{ActivityId:int}` | Narrow | `states/activity-view--narrow.png` | Dr Patel | At Step A.7.2. |
| `/activities/mine` | Nothing filed | `states/my-activities--empty.png` | Dr Mahlangu | At Step 2.19: "No activities yet". |
| `/activities/mine` | A draft | `states/my-activities--draft.png` | Dr Dlamini | At Step 3.2. |
| `/activities/mine` | Logged and cancelled | `states/my-activities--mixed.png` | Dr du Plessis | At Step 3.20. |
| `/activities/mine` | With MSF records | `states/my-activities--msf.png` | Dr Molefe | At Step 3.48. |
| `/activities/mine` | Credited None after leaving | `states/my-activities--credited-none.png` | Dr du Plessis | At Step 5.28. |
| `/activities/mine` | A paused EPA | `states/my-activities--paused-epa.png` | Dr Dlamini | At Step 6.19. |
| `/activities/mine` | Loading | `states/my-activities--loading.png` | Dr Dlamini | Hold a read, then My activities in the menu. No change. |
| `/activities/mine` | Load error | `states/my-activities--load-error.png` | Dr Dlamini | The same, held 35 s. No change. |
| `/activities/mine` | Narrow | `states/my-activities--narrow.png` | Dr Dlamini | At Step A.7.3. |
| `/activities/inbox` | Inbox clear | `states/activity-inbox--empty.png` | Dr Patel | At Step 3.4. |
| `/activities/inbox` | An assessor's requests | `states/activity-inbox--assessor.png` | Dr Patel | At Step 3.24: two rows. |
| `/activities/inbox` | A trainee's returned work | `states/activity-inbox--trainee.png` | Dr Ndlovu | At Step 3.16. |
| `/activities/inbox` | A paused EPA | `states/activity-inbox--paused-epa.png` | Dr Patel | At Step 6.18, before opening it. |
| `/activities/inbox` | Loading | `states/activity-inbox--loading.png` | Dr Patel | Hold a read, then Activity inbox in the menu. No change. |
| `/activities/inbox` | Narrow | `states/activity-inbox--narrow.png` | Dr Patel | At Step A.7.2. |

## Portfolio

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/portfolio/progress` | Admitted, nothing counted | `states/my-progress--first.png` | Dr Molefe | At Step 2.39. |
| `/portfolio/progress` | One encounter counted | `states/my-progress--counting.png` | Dr Dlamini | At Step 3.7. |
| `/portfolio/progress` | Target met, with MSF coverage | `states/my-progress--msf.png` | Dr Molefe | At Step 3.48. |
| `/portfolio/progress` | Standing with STARs | `states/my-progress--standing.png` | Dr Molefe | At Step 4.40. |
| `/portfolio/progress` | Exit rule met | `states/my-progress--exit-met.png` | Dr Molefe | At Step 5.15. |
| `/portfolio/progress` | Completed programme, read-only | `states/my-progress--completed.png` | Dr Molefe (former trainee) | At Step 5.20. |
| `/portfolio/progress` | Withdrawn, read-only | `states/my-progress--withdrawn.png` | Dr du Plessis | At Step 5.28. |
| `/portfolio/progress` | A paused EPA | `states/my-progress--paused-epa.png` | Dr Dlamini | At Step 6.19: no PAED-012 card, its trajectory "(no longer in use)". |
| `/portfolio/progress` | An institution's own item | `states/my-progress--local-item.png` | Dr Dlamini | At Step 6.27. |
| `/portfolio/progress` | New version before the rebuild | `states/my-progress--before-rebuild.png` | Dr Ndlovu | At Step 6.37. |
| `/portfolio/progress` | New version after the rebuild | `states/my-progress--after-rebuild.png` | Dr Ndlovu | At Step 6.39. |
| `/portfolio/progress` | December notice | `states/my-progress--december.png` | Dr Dlamini | Only when `D` is in December: "The <year> academic year ended on 30 November. …". December replays only: on any other replay it is not shown, and its absence is not a gap. No change. |
| `/portfolio/progress` | Loading | `states/my-progress--loading.png` | Dr Dlamini | Hold a read, then My progress in the menu. No change. |
| `/portfolio/progress` | Load error | `states/my-progress--load-error.png` | Dr Dlamini | The same, held 35 s. No change. |
| `/portfolio/progress` | Narrow, current | `states/my-progress--narrow.png` | Dr Dlamini | At Step A.7.3. |
| `/portfolio/progress` | Narrow, former trainee | `states/my-progress--narrow-former.png` | Dr Molefe | At Step A.7.4. |
| `/portfolio/authorisations` | None yet | `states/my-authorisations--empty.png` | Dr Mahlangu | At Step 4.43, from her dashboard's card: "No active authorisations yet". No change. |
| `/portfolio/authorisations` | With an expiry | `states/my-authorisations--expiring.png` | Dr Molefe | At Step 4.39: "Expires in 20 days". |
| `/portfolio/authorisations` | After a revocation | `states/my-authorisations--after-revocation.png` | Dr Dlamini | At Step 4.42. |
| `/portfolio/authorisations` | Fifteen | `states/my-authorisations--fifteen.png` | Dr Molefe | At Step 5.8. |
| `/portfolio/authorisations` | Loading | `states/my-authorisations--loading.png` | Dr Molefe | Hold a read, then the dashboard's View authorisations. No change. |
| `/portfolio/authorisations` | Narrow | `states/my-authorisations--narrow.png` | Dr Molefe | At Step 4.39, at 390 px. No change. |
| `/portfolio/export` | Own, before export | `states/export-portfolio--own.png` | Dr Molefe | At Step 5.9, before Export: the last twelve months filled in. |
| `/portfolio/export` | Exported | `states/export-portfolio--exported.png` | Dr Molefe | At Step 5.9. |
| `/portfolio/export/{TraineeUserId}` | Staff, by address | `states/export-portfolio--staff.png` | Prof Mbatha | At Step 5.10, before Export. |
| `/portfolio/export/{TraineeUserId}` | Refused | `states/export-portfolio--refused.png` | Dr Dlamini | Scratch (post-appendix): type Dr Molefe's export address, press Export: "You are not authorized to export this portfolio." |
| `/portfolio/export` | Narrow | `states/export-portfolio--narrow.png` | Dr Molefe | At Step 5.23, at 390 px, before Export. No change. |
| `/portfolio/verify` | Blank | `states/verify-export--blank.png` | Anonymous | At Step 5.13, before Verify. |
| `/portfolio/verify` | Verified | `states/verify-export--verified.png` | Anonymous | At Step 5.13. |
| `/portfolio/verify` | No match | `states/verify-export--no-match.png` | Anonymous | At Step 5.14, the tampered copy. |
| `/portfolio/verify` | Empty hash | `states/verify-export--empty.png` | Anonymous | At Step 5.14, spaces only. |
| `/portfolio/verify` | Could not check | `states/verify-export--check-failed.png` | Anonymous | Hold a read, open `/portfolio/verify?hash=0&check=1`, wait 35 s: "The export could not be checked just now." No change. |
| `/portfolio/verify` | Narrow | `states/verify-export--narrow.png` | Anonymous | At Step A.7.12. |

## Committee

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/committee/panels` | No panels | `states/panels-list--empty.png` | Prof Mbatha | At Step 2.20. |
| `/committee/panels` | The manager's list | `states/panels-list--manager.png` | Prof Mbatha | At Step 2.24, with the routing card. |
| `/committee/panels` | A speciality admin's list | `states/panels-list--speciality-admin.png` | Dr Mokoena | At Step 2.23, back on the list. |
| `/committee/panels` | A member's list | `states/panels-list--member.png` | Dr Botha | At Step 4.14: no New panel, no Edit column. |
| `/committee/panels` | A coordinator's list | `states/panels-list--coordinator.png` | Mr Smit | At Step 2.32. |
| `/committee/panels` | Loading | `states/panels-list--loading.png` | Dr Botha | Hold a read, then Decision panels in the menu. No change. |
| `/committee/panels` | Narrow | `states/panels-list--narrow.png` | Dr Mokoena | At Step A.7.7. |
| `/committee/panels/new` | The new-panel form | `states/panel-edit--new.png` | Prof Mbatha | At Step 2.21, before choosing members. |
| `/committee/panels/new` | Chair moved out of Members | `states/panel-edit--chair-note.png` | Prof Mbatha | At Step 2.21: "Thandi Zulu is the chair now, …". |
| `/committee/panels/new` | A panel of one refused | `states/panel-edit--one-member.png` | Prof Mbatha | At Step 2.21, after Save. |
| `/committee/panels/{PanelId:int}` | Saved | `states/panel-edit--saved.png` | Prof Mbatha | At Step 2.22. |
| `/committee/panels/{PanelId:int}` | Read-only committee | `states/panel-edit--speciality-admin.png` | Dr Mokoena | At Step 2.23: "Decides for" read-only, then "Panel members updated." |
| `/committee/panels/new` | Speciality scope only | `states/panel-edit--sub-speciality-admin.png` | Dr Sithole | At Step 2.25. |
| `/committee/panels/{PanelId:int}` | Committee change refused | `states/panel-edit--body-refused.png` | Prof Mbatha | At Step 4.13. |
| `/committee/panels/{PanelId:int}` | Not found | `states/panel-edit--not-found.png` | Prof Mbatha | Typed: `/committee/panels/999999`: "The decision panel could not be found." No change. |
| `/committee/panels/{PanelId:int}` | A member who can no longer sit | `states/panel-edit--unseatable.png` | Prof Mbatha | Scratch (post-appendix): lock Dr Botha out on her user page, open the panel: "1 member of this panel can no longer sit on it, …". |
| `/committee/panels/{PanelId:int}` | Narrow | `states/panel-edit--narrow.png` | Prof Mbatha | At Step 2.22, at 390 px. No change. |
| `/committee/reviews` | None yet, a scheduler | `states/reviews-schedule--empty-scheduler.png` | Mr Smit | At Step 4.1, before Step 4.6: "No reviews yet". No change. |
| `/committee/reviews` | None yet, a member | `states/reviews-schedule--empty-member.png` | Dr Naidoo | At Step 4.5: "The reviews of the panels you sit on appear here once they are scheduled." No change. |
| `/committee/reviews` | The form, no panel chosen | `states/reviews-schedule--form.png` | Mr Smit | At Step 4.6, Schedule review pressed: Trainee disabled. |
| `/committee/reviews` | The agenda preview | `states/reviews-schedule--preview.png` | Mr Smit | At Step 4.6, before Create review. |
| `/committee/reviews` | A second review refused | `states/reviews-schedule--refused.png` | Mr Smit | At Step 4.8. |
| `/committee/reviews` | Scheduled | `states/reviews-schedule--scheduled.png` | Mr Smit | At Step 4.7. |
| `/committee/reviews` | Filled from Decisions due | `states/reviews-schedule--prefilled.png` | Dr Mokoena | At Step 4.9. |
| `/committee/reviews` | Formative preview | `states/reviews-schedule--formative.png` | Mr Smit | At Step 4.49. |
| `/committee/reviews` | Already decided in this window | `states/reviews-schedule--pre-graduation.png` | Mr Smit | At Step 5.2, before Create review. |
| `/committee/reviews` | Ratified | `states/reviews-schedule--ratified.png` | Mr Smit | At Step 4.33. |
| `/committee/reviews` | Current trainees only | `states/reviews-schedule--current-only.png` | Mr Smit | At Step 5.29, the trainee list open. |
| `/committee/reviews` | After an erasure | `states/reviews-schedule--pseudonym.png` | Prof Mbatha | At Step A.1.14: `deleted_user_…`, one Withdrawn. |
| `/committee/reviews` | Preview failed | `states/reviews-schedule--preview-failed.png` | Mr Smit | At Step 4.6, before choosing the trainee: hold a read, choose Dr Molefe, wait 35 s: "The agenda could not be previewed: …". `ROLLBACK;`, then choose another trainee and her again. No change. |
| `/committee/reviews` | Loading | `states/reviews-schedule--loading.png` | Mr Smit | Hold a read, then Committee reviews in the menu. No change. |
| `/committee/reviews` | Narrow | `states/reviews-schedule--narrow.png` | Dr Sithole | At Step A.7.8. |
| `/committee/reviews/{ReviewId:int}` | Scheduled, the chair | `states/review-detail--scheduled.png` | Dr Zulu | At Step 4.15, with the sampling warnings above the cards. |
| `/committee/reviews/{ReviewId:int}` | Scheduled, no Start | `states/review-detail--scheduled-no-start.png` | Dr Mokoena | At Step 4.9. |
| `/committee/reviews/{ReviewId:int}` | In progress, the chair | `states/review-detail--in-progress.png` | Dr Zulu | At Step 4.16: staging form, Record decision disabled with its reason. |
| `/committee/reviews/{ReviewId:int}` | One item named | `states/review-detail--single-item.png` | Dr Zulu | At Step 4.17, one line ticked. |
| `/committee/reviews/{ReviewId:int}` | Staged | `states/review-detail--staged.png` | Dr Zulu | At Step 4.17, after staging. |
| `/committee/reviews/{ReviewId:int}` | Expiry refused | `states/review-detail--expiry-refused.png` | Dr Zulu | At Step 4.18. |
| `/committee/reviews/{ReviewId:int}` | Second decision refused | `states/review-detail--duplicate-refused.png` | Dr Zulu | At Step 4.20. |
| `/committee/reviews/{ReviewId:int}` | Deferral form, no reason | `states/review-detail--defer-empty.png` | Dr Zulu | At Step 4.21: "Say why the committee is deferring the decision." |
| `/committee/reviews/{ReviewId:int}` | Every closing line settled | `states/review-detail--ready-to-record.png` | Dr Zulu | At Step 4.21, after the last deferral: Record decision enabled. |
| `/committee/reviews/{ReviewId:int}` | A member during the sitting | `states/review-detail--member.png` | Dr Naidoo | At Step 4.22. |
| `/committee/reviews/{ReviewId:int}` | No quorum | `states/review-detail--quorum-refused.png` | Dr Zulu | At Step 4.23. |
| `/committee/reviews/{ReviewId:int}` | No category | `states/review-detail--category-refused.png` | Dr Zulu | At Step 4.24. |
| `/committee/reviews/{ReviewId:int}` | Decided, the chair | `states/review-detail--decided.png` | Dr Zulu | At Step 4.25: Ratify offered. |
| `/committee/reviews/{ReviewId:int}` | Decided, an administrator | `states/review-detail--decided-admin.png` | Prof Mbatha | At Step 4.26. |
| `/committee/reviews/{ReviewId:int}` | Ratified, STARs issued | `states/review-detail--ratified.png` | Dr Zulu | At Step 4.27. |
| `/committee/reviews/{ReviewId:int}` | No item about the EPA | `states/review-detail--none-about-epa.png` | Dr Zulu | At Step 4.28, staging PAED-002. |
| `/committee/reviews/{ReviewId:int}` | Ratified, no STAR | `states/review-detail--ratified-no-star.png` | Dr Zulu | At Step 4.30. |
| `/committee/reviews/{ReviewId:int}` | Under appeal, a member | `states/review-detail--appeal-member.png` | Dr Naidoo | At Step 4.44. |
| `/committee/reviews/{ReviewId:int}` | Under appeal, the resolve form | `states/review-detail--appeal-form.png` | Dr van Rensburg | At Step 4.45, Remitted chosen. |
| `/committee/reviews/{ReviewId:int}` | Remit without a quorum | `states/review-detail--remit-refused.png` | Dr Zulu | At Step 4.46. |
| `/committee/reviews/{ReviewId:int}` | Closed after a remit | `states/review-detail--remitted.png` | Dr Zulu | At Step 4.47. |
| `/committee/reviews/{ReviewId:int}` | Formative, scheduled | `states/review-detail--formative.png` | Mr Smit | At Step 4.49. |
| `/committee/reviews/{ReviewId:int}` | Formative, in progress | `states/review-detail--formative-in-progress.png` | Dr Zulu | At Step 4.50, before Close review. |
| `/committee/reviews/{ReviewId:int}` | Formative, closed | `states/review-detail--formative-closed.png` | Dr Zulu | At Step 4.50. |
| `/committee/reviews/{ReviewId:int}` | Pre-graduation, in progress | `states/review-detail--pre-graduation.png` | Dr Zulu | At Step 5.3. |
| `/committee/reviews/{ReviewId:int}` | Graduate recorded | `states/review-detail--graduate.png` | Dr Zulu | At Step 5.5. |
| `/committee/reviews/{ReviewId:int}` | Every EPA at its exit level | `states/review-detail--graduate-ratified.png` | Dr Zulu | At Step 5.6. |
| `/committee/reviews/{ReviewId:int}` | Withdrawn by an erasure | `states/review-detail--withdrawn.png` | Prof Mbatha | At Step A.1.14. |
| `/committee/reviews/{ReviewId:int}` | Not found | `states/review-detail--not-found.png` | Mr Smit | Typed: `/committee/reviews/999999`: "The committee review could not be found among the reviews you can view." No change. |
| `/committee/reviews/{ReviewId:int}` | The chair can no longer act | `states/review-detail--chair-cannot-act.png` | Dr Naidoo | Scratch (post-act4): Mr Smit schedules a formative review of Dr Dlamini for the period holding `D`; Prof Mbatha locks Dr Zulu out; Dr Naidoo opens the review. |
| `/committee/reviews/{ReviewId:int}` | Agenda: no longer decided | `states/review-detail--no-longer-decided.png` | Dr Zulu | Scratch (post-act4): Mr Smit schedules a second annual review of Dr Dlamini for the period (her first is ratified, so it holds no seat) and Dr Zulu starts it; Dr Mokoena revokes Dr Dlamini's PAED-001 STAR; Dr Zulu reloads: the agenda's warning that PAED-001 is no longer decided in its window. |
| `/committee/reviews/{ReviewId:int}` | Ratify blocked | `states/review-detail--ratify-blocked.png` | Dr Zulu | Scratch (post-act4), continuing the review above: defer every closing line, record with Dr Naidoo present; Dr Zulu reloads. Ratify reads as the page gives it. Its quorum reason cannot be reached from the UI: the quorum is who sat when the decision was recorded (T165), so locking a member out afterwards changes nothing, and Record refuses a sitting without one (T295 sweep). |
| `/committee/reviews/{ReviewId:int}` | Appeal dismissed | `states/review-detail--appeal-dismissed.png` | Dr Zulu | Scratch (post-act4): Dr Dlamini lodges an appeal on My committee reviews; Dr Zulu resolves it Dismissed. There is no Upheld outcome since T307 (D51), so `--appeal-upheld` is no longer captured. |
| `/committee/reviews/{ReviewId:int}` | Entrustment-only, and decided by another panel | `states/review-detail--entrustment-only.png` | Dr Zulu | Scratch (post-act4; the T295 sweep used it, with Semester 1, 2026 as the previous semester): Prof Mbatha creates a second Paediatrics panel sitting as the Neonatal team Clinical Competency Committee (Zulu chair, Botha member); Mr Smit schedules Dr Dlamini before it for the previous semester, type Entrustment-only review, then before the general panel for the same semester. Start the entrustment-only review and capture its decision form (no Category, its note) and the second (`--decided-elsewhere`, PAED-004 and 005 under "Decided by another panel"). Needs confirmation of the sitting-order warning, which may also show. |
| `/committee/reviews/{ReviewId:int}` | Loading | `states/review-detail--loading.png` | Dr Zulu | Hold a read, open a review from Committee reviews. No change. |
| `/committee/reviews/{ReviewId:int}` | Narrow | `states/review-detail--narrow.png` | Dr Zulu | At Step A.7.6. |
| `/committee/my-reviews` | None yet | `states/my-reviews--empty.png` | Dr Dlamini | At Step 3.50, My committee reviews from the menu: "No decisions yet". No change. |
| `/committee/my-reviews` | The list and a review | `states/my-reviews--detail.png` | Dr Molefe | At Step 4.41. |
| `/committee/my-reviews` | The appeal form | `states/my-reviews--appeal-form.png` | Dr Mahlangu | At Step 4.43, before lodging. |
| `/committee/my-reviews` | Under appeal | `states/my-reviews--appealed.png` | Dr Mahlangu | At Step 4.43: "Appeal lodged." |
| `/committee/my-reviews` | The replacement decision | `states/my-reviews--remitted.png` | Dr Mahlangu | At Step 4.48. |
| `/committee/my-reviews` | Annual and formative | `states/my-reviews--two.png` | Dr Ndlovu | At Step 4.51. |
| `/committee/my-reviews` | Pre-graduation | `states/my-reviews--pre-graduation.png` | Dr Molefe | At Step 5.8. |
| `/committee/my-reviews` | Loading | `states/my-reviews--loading.png` | Dr Molefe | Hold a read, then My committee reviews in the menu. No change. |
| `/committee/my-reviews` | Narrow | `states/my-reviews--narrow.png` | Dr Molefe | At Step 4.41, at 390 px. No change. |
| `/committee/decisions-due` | Nothing due | `states/decisions-due--nothing-due.png` | Prof Mbatha | After Step 1.22, before Step 2.29 (no trainee admitted): "Nothing due for <period>". No change. |
| `/committee/decisions-due` | An administrator, no institution | `states/decisions-due--choose-institution.png` | devadmin | Any time after Act 1: "Choose an institution". No change. |
| `/committee/decisions-due` | Outstanding | `states/decisions-due--outstanding.png` | Mr Smit | At Step 4.1. |
| `/committee/decisions-due` | No row matches | `states/decisions-due--no-match.png` | Mr Smit | At Step 4.1, Status set to Missed: "No decision matches the filters". No change. |
| `/committee/decisions-due` | A past semester, missed | `states/decisions-due--missed.png` | Prof Mbatha | At Step 4.2. |
| `/committee/decisions-due` | All scheduled | `states/decisions-due--scheduled.png` | Dr Sithole | At Step 4.10. |
| `/committee/decisions-due` | After the sitting | `states/decisions-due--after-sitting.png` | Prof Mbatha | At Step 4.34, Every status. |
| `/committee/decisions-due` | Revoked, to decide again | `states/decisions-due--revoked.png` | Dr Sithole | At Step 4.37. |
| `/committee/decisions-due` | Loading | `states/decisions-due--loading.png` | Mr Smit | Hold a read, then Decisions due in the menu. No change. |
| `/committee/decisions-due` | Narrow | `states/decisions-due--narrow.png` | Mr Smit | At Step A.7.5. |
| `/admin/entrustment-decisions` | None issued | `states/entrustment-decisions--empty.png` | Prof Mbatha | At Step 4.2, from her dashboard's quick link: "No entrustment decisions". No change. |
| `/admin/entrustment-decisions` | Issued | `states/entrustment-decisions--issued.png` | Prof Mbatha | At Step 4.35. |
| `/admin/entrustment-decisions` | Filtered | `states/entrustment-decisions--filtered.png` | Prof Mbatha | At Step 4.35, "Molefe" applied. |
| `/admin/entrustment-decisions` | Revoke form | `states/entrustment-decisions--revoke-form.png` | Dr Mokoena | At Step 4.36, before confirming. |
| `/admin/entrustment-decisions` | Revoked | `states/entrustment-decisions--revoked.png` | Dr Mokoena | At Step 4.36. |
| `/admin/entrustment-decisions` | Superseded | `states/entrustment-decisions--superseded.png` | Prof Mbatha | At Step 5.7, status All. |
| `/admin/entrustment-decisions` | A paused EPA | `states/entrustment-decisions--paused-epa.png` | Prof Mbatha | At Step 6.22. |
| `/admin/entrustment-decisions` | Loading | `states/entrustment-decisions--loading.png` | Prof Mbatha | Hold a read, then the dashboard's Entrustment decisions. No change. |
| `/admin/entrustment-decisions` | Narrow | `states/entrustment-decisions--narrow.png` | Dr Mokoena | At Step A.7.7. |

## Multi-source feedback

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/msf/campaigns` | No campaigns | `states/campaigns-list--empty.png` | Mr Smit | At Step 3.34. |
| `/msf/campaigns` | Open and draft | `states/campaigns-list--open-draft.png` | Mr Smit | At Step 3.38, before Withdraw. |
| `/msf/campaigns` | Withdraw dialog | `states/campaigns-list--withdraw-dialog.png` | Mr Smit | At Step 3.38. |
| `/msf/campaigns` | Withdrawn | `states/campaigns-list--withdrawn.png` | Mr Smit | At Step 3.38, after confirming. |
| `/msf/campaigns` | Released | `states/campaigns-list--released.png` | Mr Smit | At Step 3.46. |
| `/msf/campaigns` | Loading | `states/campaigns-list--loading.png` | Mr Smit | Hold a read, then MSF campaigns in the menu. No change. |
| `/msf/campaigns` | Narrow | `states/campaigns-list--narrow.png` | Mr Smit | At Step A.7.5. |
| `/msf/campaigns/new` | No template yet | `states/campaign-edit--no-template.png` | Mr Smit | At Step 3.34, before adding the Quick template. |
| `/msf/campaigns/new` | Template created | `states/campaign-edit--template-created.png` | Mr Smit | At Step 3.34. |
| `/msf/campaigns/new` | A trainee chosen | `states/campaign-edit--new.png` | Mr Smit | At Step 3.35, before Create: 15 EPAs offered. |
| `/msf/campaigns/{CampaignId:int}` | Draft, nobody invited | `states/campaign-edit--draft-empty.png` | Mr Smit | At Step 3.35: Open campaign disabled with its reason. |
| `/msf/campaigns/{CampaignId:int}` | Remove dialog | `states/campaign-edit--remove-dialog.png` | Mr Smit | At Step 3.36. |
| `/msf/campaigns/{CampaignId:int}` | Draft with invitees | `states/campaign-edit--draft.png` | Mr Smit | At Step 3.36, the end. |
| `/msf/campaigns/{CampaignId:int}` | Opened | `states/campaign-edit--opened.png` | Mr Smit | At Step 3.37. |
| `/msf/campaigns/{CampaignId:int}` | Withdraw dialog | `states/campaign-edit--withdraw-dialog.png` | Mr Smit | After Step 3.37, before 3.44: Withdraw campaign on Dr Molefe's page, capture, Cancel. No change. |
| `/msf/campaigns/{CampaignId:int}` | Links not delivered | `states/campaign-edit--not-delivered.png` | Mr Smit | At Step 3.43, before Resend. |
| `/msf/campaigns/{CampaignId:int}` | Link resent | `states/campaign-edit--resent.png` | Mr Smit | At Step 3.43. |
| `/msf/campaigns/{CampaignId:int}` | Closed | `states/campaign-edit--closed.png` | Mr Smit | At Step 3.44. |
| `/msf/campaigns/{CampaignId:int}` | Withdrawn | `states/campaign-edit--withdrawn.png` | Mr Smit | After Step 3.38: View campaign on Dr du Plessis's row. |
| `/msf/campaigns/{CampaignId:int}` | Released | `states/campaign-edit--released.png` | Mr Smit | After Step 3.46: View campaign on Dr Molefe's row. |
| `/msf/campaigns/{CampaignId:int}` | Unavailable | `states/campaign-edit--unavailable.png` | Mr Smit | Typed: `/msf/campaigns/999999`: "Campaign unavailable". No change. |
| `/msf/campaigns/{CampaignId:int}` | Narrow | `states/campaign-edit--narrow.png` | Mr Smit | At Step A.7.5. |
| `/msf/reports/{CampaignId:int}` | Open, a group suppressed | `states/campaign-report--open.png` | Mr Smit | At Step 3.41. |
| `/msf/reports/{CampaignId:int}` | Under review, ready | `states/campaign-report--under-review.png` | Mr Smit | At Step 3.44. |
| `/msf/reports/{CampaignId:int}` | Released | `states/campaign-report--released.png` | Mr Smit | At Step 3.46. |
| `/msf/reports/{CampaignId:int}` | Blocked from release | `states/campaign-report--blocked.png` | Mr Smit | Scratch (post-appendix): a campaign for Dr Dlamini, evidence for PAED-012, one invitee; open it; answer the link from the log; close it: Release disabled, "Release needs 5 responses; 1 have come back." |
| `/msf/reports/{CampaignId:int}` | Names no EPA | `states/campaign-report--no-epa.png` | Mr Smit | Scratch (post-appendix): the same with no EPA ticked: "This campaign names no EPA, …". |
| `/msf/reports/{CampaignId:int}` | Unavailable | `states/campaign-report--unavailable.png` | Mr Smit | Typed: `/msf/reports/999999`: "Report unavailable". No change. |
| `/msf/reports/{CampaignId:int}` | Loading | `states/campaign-report--loading.png` | Mr Smit | Hold a read, then View report on the list. No change. |
| `/msf/reports/{CampaignId:int}` | Narrow | `states/campaign-report--narrow.png` | Mr Smit | At Step 3.46, at 390 px. No change. |
| `/msf/coverage` | Nobody to show | `states/programme-coverage--empty.png` | Mr Smit | At Step 2.8, before signing out (no trainee admitted): "No trainees to show". No change. |
| `/msf/coverage` | Coverage | `states/programme-coverage--loaded.png` | Mr Smit | At Step 3.49. |
| `/msf/coverage` | Loading | `states/programme-coverage--loading.png` | Mr Smit | Hold a read, then MSF coverage on the list. No change. |
| `/msf/coverage` | Narrow | `states/programme-coverage--narrow.png` | Mr Smit | At Step 3.49, at 390 px. No change. |
| `/msf/my-reports` | None released | `states/my-msf-reports--empty.png` | Dr Molefe | At Step 3.25, MSF reports in the menu: "No released reports". No change. |
| `/msf/my-reports` | The list | `states/my-msf-reports--list.png` | Dr Molefe | At Step 3.47. |
| `/msf/my-reports/{CampaignId:int}` | A report | `states/my-msf-reports--report.png` | Dr Molefe | At Step 3.47. |
| `/msf/my-reports/{CampaignId:int}` | Someone else's id | `states/my-msf-reports--foreign.png` | Dr Dlamini | After Step 3.46, typed with Dr Molefe's campaign id: her empty list, nothing selected. No change. |
| `/msf/my-reports` | Loading | `states/my-msf-reports--loading.png` | Dr Molefe | Hold a read, then MSF reports in the menu. No change. |
| `/msf/my-reports` | Narrow | `states/my-msf-reports--narrow.png` | Dr Molefe | At Step 3.47, at 390 px. No change. |
| `/msf/respond` | The questionnaire | `states/msf-respond--form.png` | Anonymous | At Step 3.39, before answering. |
| `/msf/respond` | Rating left out | `states/msf-respond--required.png` | Anonymous | At Step 3.39, Submit with no rating: the browser's own required prompt. No change. |
| `/msf/respond` | Thank you | `states/msf-respond--thank-you.png` | Anonymous | At Step 3.39. |
| `/msf/respond` | Link used | `states/msf-respond--used.png` | Anonymous | At Step 3.40. |
| `/msf/respond` | Request closed | `states/msf-respond--closed.png` | Anonymous | At Step 3.45. |
| `/msf/respond` | Link not recognised | `states/msf-respond--not-recognised.png` | Anonymous | Typed: `/msf/respond?token=not-a-link`: "Feedback link not recognised". No change. |
| `/msf/respond` | Link expired | `states/msf-respond--expired.png` | Anonymous | Scratch (post-appendix): open a campaign for Dr Dlamini with one invitee; `UPDATE "MsfInvitations" SET "ExpiresOn" = current_date - 1 WHERE "CampaignId" = <its id>;`; open the link: "Feedback link expired". |
| `/msf/respond` | Something went wrong | `states/msf-respond--fault.png` | Anonymous | Hold a read, open a used link from Act 3, wait 35 s. No change. |
| `/msf/respond` | Narrow | `states/msf-respond--narrow.png` | Anonymous | At Step 3.39, Sister Mokwena's link at 390 px before answering. No change. |

## National catalogue

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/admin/colleges` | Two Colleges | `states/colleges-list--two.png` | devadmin | At Step 1.2. |
| `/admin/colleges` | Three Colleges | `states/colleges-list--three.png` | devadmin | At Step 6.1. |
| `/admin/colleges` | Loading | `states/colleges-list--loading.png` | devadmin | Hold a read, then Colleges in the menu. No change. |
| `/admin/colleges` | Narrow | `states/colleges-list--narrow.png` | devadmin | At Step 6.2, at 390 px. No change. |
| `/admin/colleges/{Id:int}` | Edit | `states/college-edit--edit.png` | devadmin | At Step 1.3. |
| `/admin/colleges/new` | Create | `states/college-edit--create.png` | devadmin | At Step 6.1, before typing. |
| `/admin/colleges/new` | Required fields empty | `states/college-edit--invalid.png` | devadmin | At Step 6.1, Save with the form empty: the fields' messages. Nothing is sent. No change. |
| `/admin/colleges/new` | Duplicate refused | `states/college-edit--duplicate.png` | devadmin | At Step 6.1, the first save. |
| `/admin/colleges/{Id:int}` | Saved | `states/college-edit--saved.png` | devadmin | At Step 6.2: "College saved." |
| `/admin/colleges/{Id:int}` | Deactivated | `states/college-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on CNSA: "College deactivated." |
| `/admin/colleges/{Id:int}` | Unknown id | `states/college-edit--not-found.png` | devadmin | Typed: `/admin/colleges/999999`: Page not found. No change. |
| `/admin/colleges/{Id:int}` | Narrow | `states/college-edit--narrow.png` | devadmin | At Step 6.2, at 390 px, before saving. No change. |
| `/admin/specialities` | (redirect) | — | Dr Kruger, devadmin | No state of its own: it redirects before it renders (Step 1.12; devadmin lands on Colleges). |
| `/admin/colleges/{CollegeId:int}/specialities` | One speciality | `states/specialities-list--one.png` | Dr Kruger | At Step 1.12. |
| `/admin/colleges/{CollegeId:int}/specialities` | None yet | `states/specialities-list--empty.png` | devadmin | At Step 6.3, before the create. |
| `/admin/colleges/{CollegeId:int}/specialities` | Another College's id | `states/specialities-list--other-college.png` | Dr Kruger | Typed with the Demo College's id (`SELECT "Id" FROM "Colleges" WHERE "ShortCode" = 'DEMO-C';`): reads as a College with no specialities. No change. |
| `/admin/colleges/{CollegeId:int}/specialities` | Loading | `states/specialities-list--loading.png` | Dr Kruger | Hold a read, then Specialities in the menu. No change. |
| `/admin/colleges/{CollegeId:int}/specialities` | Narrow | `states/specialities-list--narrow.png` | Dr Kruger | At Step 6.11, at 390 px. No change. |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Edit | `states/speciality-edit--edit.png` | Dr Kruger | At Step 6.11, before saving. |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Saved | `states/speciality-edit--saved.png` | Dr Kruger | At Step 6.11: "Speciality saved." |
| `/admin/colleges/{CollegeId:int}/specialities/new` | Create | `states/speciality-edit--create.png` | devadmin | At Step 6.3, before typing. |
| `/admin/colleges/{CollegeId:int}/specialities/new` | Required field empty | `states/speciality-edit--invalid.png` | devadmin | At Step 6.3, Save with the form empty. No change. |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Deactivated | `states/speciality-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on Neurology: "Speciality deactivated." |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Unknown id | `states/speciality-edit--not-found.png` | Dr Kruger | Typed with his College's id and `999999`: Page not found. No change. |
| `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Narrow | `states/speciality-edit--narrow.png` | Dr Kruger | At Step 6.11, at 390 px. No change. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | One | `states/sub-specialities-list--one.png` | Dr Kruger | At Step 1.13. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | None yet | `states/sub-specialities-list--empty.png` | devadmin | At Step 6.4, before the create. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | Two | `states/sub-specialities-list--two.png` | Dr Kruger | At Step 6.12. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | Loading | `states/sub-specialities-list--loading.png` | Dr Kruger | Hold a read, then Sub-specialities on the speciality's row. No change. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | Narrow | `states/sub-specialities-list--narrow.png` | Dr Kruger | At Step 6.12, at 390 px. No change. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | With a default ladder | `states/sub-speciality-edit--edit.png` | Dr Kruger | At Step 1.14. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/new` | Create | `states/sub-speciality-edit--create.png` | devadmin | At Step 6.4, before typing. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/new` | Required field empty | `states/sub-speciality-edit--invalid.png` | devadmin | At Step 6.4, Save with the form empty. No change. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | No default | `states/sub-speciality-edit--no-default.png` | devadmin | At Step 6.4, after the create. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Default saved | `states/sub-speciality-edit--saved.png` | devadmin | At Step 6.7. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Deactivated | `states/sub-speciality-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on Adult Neurology. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Unknown id | `states/sub-speciality-edit--not-found.png` | Dr Kruger | Typed with the Paediatrics speciality's id and `999999`: Page not found. No change. |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Narrow | `states/sub-speciality-edit--narrow.png` | Dr Kruger | At Step 6.12, at 390 px. No change. |
| `/admin/epas` | None, an institution | `states/epas-list--empty.png` | Prof Mbatha | At Step 1.19. |
| `/admin/epas` | The College's fifteen | `states/epas-list--college.png` | Dr Kruger | At Step 1.16. |
| `/admin/epas` | The adopted fifteen | `states/epas-list--institution.png` | Prof Mbatha | At Step 1.21. |
| `/admin/epas` | One inactive | `states/epas-list--inactive.png` | Dr Kruger | At Step 6.17. |
| `/admin/epas` | With a local EPA | `states/epas-list--local.png` | Prof Mbatha | At Step 6.25. |
| `/admin/epas` | Loading | `states/epas-list--loading.png` | Dr Kruger | Hold a read, then EPAs in the menu. No change. |
| `/admin/epas` | Load error | `states/epas-list--load-error.png` | Dr Kruger | The same, held 35 s. No change. |
| `/admin/epas` | Narrow | `states/epas-list--narrow.png` | Dr Kruger | At Step A.7.10. |
| `/admin/epas/{Id:int}` | Edit | `states/epa-edit--edit.png` | Dr Kruger | At Step 6.14, before saving. |
| `/admin/epas/{Id:int}` | Saved | `states/epa-edit--saved.png` | Dr Kruger | At Step 6.14. |
| `/admin/epas/{Id:int}` | Deactivate dialog | `states/epa-edit--deactivate-dialog.png` | Dr Kruger | At Step 6.17. |
| `/admin/epas/{Id:int}` | Deactivated | `states/epa-edit--deactivated.png` | Dr Kruger | At Step 6.17, after confirming. |
| `/admin/epas/{Id:int}` | Reactivated | `states/epa-edit--reactivated.png` | Dr Kruger | At Step 6.23: "EPA reactivated. 1 activity … now counts towards progress." |
| `/admin/epas/new` | Create, an institution | `states/epa-edit--create-local.png` | Prof Mbatha | At Step 6.25, before saving. |
| `/admin/epas/new` | Create, the College | `states/epa-edit--create-national.png` | Dr Kruger | At Step 6.28, before saving. |
| `/admin/epas/new` | Required fields empty | `states/epa-edit--invalid.png` | Dr Kruger | At Step 6.28, Save with the form empty. No change. |
| `/admin/epas/new` | Duplicate code refused | `states/epa-edit--duplicate.png` | Dr Kruger | Scratch (post-appendix): create `PAED-001` again in Paediatrics / Paediatrics. |
| `/admin/epas/{Id:int}` | A local EPA deactivated | `states/epa-edit--local-deactivated.png` | Prof Mbatha | Scratch (post-appendix): Deactivate on KGK-001, confirm; then tick Active and save (`--local-reactivated`). |
| `/admin/epas/{Id:int}` | Out of scope | `states/epa-edit--not-found.png` | Prof Mbatha | Typed with the Demo `EPA-001`'s id: Page not found. No change. |
| `/admin/epas/{Id:int}` | Narrow | `states/epa-edit--narrow.png` | Dr Kruger | At Step 6.14, at 390 px. No change. |
| `/admin/curricula` | None, an institution | `states/curricula-list--empty.png` | Prof Mbatha | At Step 1.19. |
| `/admin/curricula` | The College's | `states/curricula-list--college.png` | Dr Kruger | At Step 1.17. |
| `/admin/curricula` | Adopted, items only | `states/curricula-list--institution.png` | Prof Mbatha | At Step 1.21. |
| `/admin/curricula` | Active and inactive | `states/curricula-list--versions.png` | Dr Kruger | At Step 6.31. |
| `/admin/curricula` | Two versions, an institution | `states/curricula-list--institution-versions.png` | Prof Mbatha | At Step 6.33. |
| `/admin/curricula` | Loading | `states/curricula-list--loading.png` | Dr Kruger | Hold a read, then Curricula in the menu. No change. |
| `/admin/curricula` | Narrow | `states/curricula-list--narrow.png` | Dr Kruger | At Step A.7.10. |
| `/admin/curricula/new` | Create | `states/curriculum-edit--create.png` | Dr Kruger | At Step 6.13, before typing. |
| `/admin/curricula/new` | Required fields empty | `states/curriculum-edit--invalid.png` | Dr Kruger | At Step 6.13, Save with the form empty. No change. |
| `/admin/curricula/{Id:int}` | Saved, inactive | `states/curriculum-edit--saved.png` | Dr Kruger | At Step 6.13. |
| `/admin/curricula/{Id:int}` | Edit, with the clone card | `states/curriculum-edit--clone.png` | Dr Kruger | At Step 6.29, before cloning. |
| `/admin/curricula/{Id:int}` | Cloned | `states/curriculum-edit--cloned.png` | Dr Kruger | At Step 6.29, the new version's page. |
| `/admin/curricula/{Id:int}` | Unknown id | `states/curriculum-edit--not-found.png` | Dr Kruger | Typed: `/admin/curricula/999999`: Page not found. No change. |
| `/admin/curricula/{Id:int}` | Narrow | `states/curriculum-edit--narrow.png` | Dr Kruger | At Step 6.31, at 390 px, before saving. No change. |
| `/admin/curricula/{Id:int}/items` | The College's items | `states/curriculum-items-edit--college.png` | Dr Kruger | At Step 1.18: the live-target warning, "Every national EPA … is already on this curriculum". |
| `/admin/curricula/{Id:int}/items` | Read-only to an institution | `states/curriculum-items-edit--institution.png` | Prof Mbatha | At Step 1.21. |
| `/admin/curricula/{Id:int}/items` | No items, nothing to add | `states/curriculum-items-edit--no-items.png` | Dr Kruger | At Step 6.13. |
| `/admin/curricula/{Id:int}/items` | An inactive EPA | `states/curriculum-items-edit--inactive.png` | Dr Kruger | At Step 6.17: "(inactive: not in force)" and its notice. |
| `/admin/curricula/{Id:int}/items` | The own-item form | `states/curriculum-items-edit--own-form.png` | Prof Mbatha | At Step 6.26, before Add item. |
| `/admin/curricula/{Id:int}/items` | An own item added | `states/curriculum-items-edit--own-added.png` | Prof Mbatha | At Step 6.26. |
| `/admin/curricula/{Id:int}/items` | A row being edited | `states/curriculum-items-edit--row-edit.png` | Dr Kruger | At Step 6.30, PAED-011's edit row open. |
| `/admin/curricula/{Id:int}/items` | Remove dialog | `states/curriculum-items-edit--remove-dialog.png` | Prof Mbatha | After Step 6.33, on 11.2: Remove on KGK's own item, capture, Cancel. No change. |
| `/admin/curricula/{Id:int}/items` | Removed | `states/curriculum-items-edit--removed.png` | Prof Mbatha | Scratch (post-appendix): the same, confirmed. |
| `/admin/curricula/{Id:int}/items` | Out of scope | `states/curriculum-items-edit--not-found.png` | Prof Mbatha | Typed with the Demo `IM Core Curriculum`'s id: Page not found. No change. |
| `/admin/curricula/{Id:int}/items` | Loading | `states/curriculum-items-edit--loading.png` | Dr Kruger | Hold a read, then Items on a curriculum. No change. |
| `/admin/curricula/{Id:int}/items` | Narrow | `states/curriculum-items-edit--narrow.png` | Dr Kruger | At Step A.7.10. |
| `/admin/entrustment-scales` | An administrator's | `states/entrustment-scales-list--administrator.png` | devadmin | At Step 1.4. |
| `/admin/entrustment-scales` | Read-only | `states/entrustment-scales-list--read-only.png` | Prof Mbatha | At Step 1.22. |
| `/admin/entrustment-scales` | Delete refused | `states/entrustment-scales-list--delete-refused.png` | devadmin | At Step 6.8. |
| `/admin/entrustment-scales` | Deleted | `states/entrustment-scales-list--deleted.png` | devadmin | Scratch (post-appendix): create a two-level scale (a scale needs at least two levels), back on the list press Delete on it: "Entrustment scale deleted." |
| `/admin/entrustment-scales` | Loading | `states/entrustment-scales-list--loading.png` | Prof Mbatha | Hold a read, then Entrustment scales in the menu. No change. |
| `/admin/entrustment-scales` | Narrow | `states/entrustment-scales-list--narrow.png` | Prof Mbatha | At Step 6.9, at 390 px. No change. |
| `/admin/entrustment-scales/{Id:int}` | Six rungs | `states/entrustment-scale-edit--edit.png` | devadmin | At Step 1.4. |
| `/admin/entrustment-scales/new` | Create | `states/entrustment-scale-edit--create.png` | devadmin | At Step 6.5, before typing. |
| `/admin/entrustment-scales/new` | Duplicate labels refused | `states/entrustment-scale-edit--duplicate.png` | devadmin | At Step 6.5, the first save. |
| `/admin/entrustment-scales/{Id:int}` | Saved | `states/entrustment-scale-edit--saved.png` | devadmin | At Step 6.6. |
| `/admin/entrustment-scales/{Id:int}` | A rung still needed | `states/entrustment-scale-edit--rung-refused.png` | devadmin | Scratch (post-appendix): on the CPSA ladder remove `5` and save: "Curriculum item N is pinned to this entrustment scale and requires level 6, …". |
| `/admin/entrustment-scales/{Id:int}` | Unknown id | `states/entrustment-scale-edit--unknown.png` | devadmin | Typed: `/admin/entrustment-scales/999999`: it returns to the scales list. No change. |
| `/admin/entrustment-scales/{Id:int}` | Narrow | `states/entrustment-scale-edit--narrow.png` | devadmin | At Step 6.6, at 390 px, before saving. No change. |

## Institution administration

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/admin/institutions` | One | `states/institutions-list--one.png` | devadmin | At Step 1.6, before the create. |
| `/admin/institutions` | Two | `states/institutions-list--two.png` | devadmin | At Step A.6.1. |
| `/admin/institutions` | Loading | `states/institutions-list--loading.png` | devadmin | Hold a read, then Institutions in the menu. No change. |
| `/admin/institutions` | Narrow | `states/institutions-list--narrow.png` | devadmin | At Step A.7.11. |
| `/admin/institutions/new` | Create | `states/institution-edit--create.png` | devadmin | At Step 1.6, before typing. |
| `/admin/institutions/new` | Required fields empty | `states/institution-edit--invalid.png` | devadmin | At Step 1.6, Save with the form empty. No change. |
| `/admin/institutions/{Id:int}` | An administrator's edit | `states/institution-edit--administrator.png` | devadmin | At Step 1.6, after Save. |
| `/admin/institutions/{Id:int}` | Saved | `states/institution-edit--saved.png` | devadmin | At Step A.6.1: "Institution saved." |
| `/admin/institutions/{Id:int}` | Her own institution | `states/institution-edit--own.png` | Prof Mbatha | At Step 1.23. |
| `/admin/institutions/{Id:int}` | Her own institution, saved (the "Deactivate refused" state ceased with T302; the file keeps its name) | `states/institution-edit--deactivate-refused.png` | Prof Mbatha | At Step A.6.3, after Save: "Institution saved.", Status as text, no Deactivate. |
| `/admin/institutions/{Id:int}` | Deactivated | `states/institution-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on the Demo Institution, confirm the dialog: "Institution deactivated." |
| `/admin/institutions/{Id:int}` | Out of scope | `states/institution-edit--not-found.png` | Prof Mbatha | At Step 1.23, the Demo Institution's id. |
| `/admin/institutions/{Id:int}` | Narrow | `states/institution-edit--narrow.png` | devadmin | At Step A.6.1, at 390 px, before saving. No change. |
| `/admin/adoptions` | Nothing adopted | `states/adoptions-list--empty.png` | Prof Mbatha | At Step 1.20, before Adopt. |
| `/admin/adoptions` | Adopted | `states/adoptions-list--adopted.png` | Prof Mbatha | At Step 1.20: "Curriculum adopted." |
| `/admin/adoptions` | Adopted twice, refused | `states/adoptions-list--refused.png` | Prof Mbatha | At Step 1.20, the second Adopt. |
| `/admin/adoptions` | Superseded | `states/adoptions-list--superseded.png` | Prof Mbatha | At Step 6.32. |
| `/admin/adoptions` | An administrator, no institution | `states/adoptions-list--choose-institution.png` | devadmin | Any time after Act 1: only the Institution picker. No change. |
| `/admin/adoptions` | An administrator, KGK chosen | `states/adoptions-list--administrator.png` | devadmin | The same, choose Kgosi Kgari Teaching Hospital; adopt nothing. No change. |
| `/admin/adoptions` | Loading | `states/adoptions-list--loading.png` | Prof Mbatha | Hold a read, then Curriculum adoptions in the menu. No change. |
| `/admin/adoptions` | Narrow | `states/adoptions-list--narrow.png` | Prof Mbatha | At Step 6.32, at 390 px, before Adopt. No change. |
| `/admin/activity-types` | Searched | `states/activity-types-list--search.png` | Prof Mbatha | At Step 1.24: `cpsa`. |
| `/admin/activity-types` | A draft in the list | `states/activity-types-list--draft.png` | Prof Mbatha | Between Steps 1.26 and 1.30: Draft reads "Draft saved" on the KGK row. No change. |
| `/admin/activity-types` | All | `states/activity-types-list--all.png` | Prof Mbatha | At Step 1.31. |
| `/admin/activity-types` | No match | `states/activity-types-list--no-match.png` | Prof Mbatha | At Step 1.31, search `zzz`: the page's empty card. No change. |
| `/admin/activity-types` | Loading | `states/activity-types-list--loading.png` | Prof Mbatha | Hold a read, then Activity types in the menu. No change. |
| `/admin/activity-types` | Narrow | `states/activity-types-list--narrow.png` | Prof Mbatha | At Step 1.31, at 390 px. No change. |
| `/admin/activity-types/{ActivityTypeId:int}` | A College instrument, to an institution | `states/activity-type-edit--college-instrument.png` | Prof Mbatha | At Step 1.25, the Form tab. |
| `/admin/activity-types/new` | New, the default draft | `states/activity-type-edit--new.png` | Prof Mbatha | At Step 1.26, before typing. |
| `/admin/activity-types/{ActivityTypeId:int}` | Metadata tab | `states/activity-type-edit--metadata.png` | Prof Mbatha | At Step 1.26, after Save draft: "Draft saved." |
| `/admin/activity-types/{ActivityTypeId:int}` | The field editor, a User field | `states/activity-type-edit--user-field.png` | Prof Mbatha | At Step 1.27, Supervising consultant open. |
| `/admin/activity-types/{ActivityTypeId:int}` | Duplicate key refused | `states/activity-type-edit--duplicate-key.png` | Prof Mbatha | At Step 1.27. |
| `/admin/activity-types/{ActivityTypeId:int}` | A `field:` rule refused | `states/activity-type-edit--field-rule.png` | Prof Mbatha | At Step 1.28, the Workflow tab. |
| `/admin/activity-types/{ActivityTypeId:int}` | Credit tab | `states/activity-type-edit--credit.png` | Prof Mbatha | At Step 1.29. |
| `/admin/activity-types/{ActivityTypeId:int}` | Published | `states/activity-type-edit--published.png` | Prof Mbatha | At Step 1.30: "Published version 1.", Publish disabled. |
| `/admin/activity-types/{ActivityTypeId:int}` | Publish warnings | `states/activity-type-edit--publish-warnings.png` | Prof Mbatha | Scratch (post-appendix): on KGK Teaching Session Log delete the Audience field and Save draft: the warnings list. |
| `/admin/activity-types/{ActivityTypeId:int}` | Draft discarded | `states/activity-type-edit--discarded.png` | Prof Mbatha | Scratch (post-appendix), after the warnings: Discard draft: "Draft discarded." |
| `/admin/activity-types/{ActivityTypeId:int}` | Unknown id | `states/activity-type-edit--not-found.png` | Prof Mbatha | Typed: `/admin/activity-types/999999`: "The activity type could not be found." No change. |
| `/admin/activity-types/{ActivityTypeId:int}` | Loading | `states/activity-type-edit--loading.png` | Prof Mbatha | Hold a read, then Edit on a row. No change. |
| `/admin/activity-types/{ActivityTypeId:int}` | Narrow | `states/activity-type-edit--narrow.png` | Prof Mbatha | At Step A.7.9. |
| `/admin/invitations` | No active invitations | `states/invitations-list--empty.png` | devadmin | At Step 1.5, before the first press. |
| `/admin/invitations` | The CollegeAdmin form | `states/invitations-list--college-admin.png` | devadmin | At Step 1.5, CollegeAdmin chosen: the College picker. |
| `/admin/invitations` | Refused | `states/invitations-list--refused.png` | devadmin | At Step 1.5, the first press. |
| `/admin/invitations` | Issued, with its link | `states/invitations-list--issued.png` | devadmin | At Step 1.5, the second press. |
| `/admin/invitations` | An institution's form | `states/invitations-list--institution.png` | Prof Mbatha | At Step 2.1. |
| `/admin/invitations` | Rows being sent | `states/invitations-list--being-sent.png` | Prof Mbatha | At Step 2.16. |
| `/admin/invitations` | Revoked | `states/invitations-list--revoked.png` | Prof Mbatha | At Step 2.17. |
| `/admin/invitations` | Not delivered | `states/invitations-list--not-delivered.png` | Prof Mbatha | At Step 2.26, before Resend. |
| `/admin/invitations` | Resent | `states/invitations-list--resent.png` | Prof Mbatha | At Step 2.26. |
| `/admin/invitations` | Invalid address | `states/invitations-list--invalid.png` | Prof Mbatha | At Step 2.1, Issue invitation with the email `not-an-address`: the field's message. Nothing is sent. No change. |
| `/admin/invitations` | Sent | `states/invitations-list--sent.png` | Prof Mbatha | Scratch (post-appendix), with an SMTP sink that accepts mail on the scratch instance's `Email__SmtpHost`: issue one invitation and reload once the worker reports it. |
| `/admin/invitations` | Check the address | `states/invitations-list--check-address.png` | Prof Mbatha | Scratch (post-appendix), with a sink that refuses the address: issue, then Resend after each failure until the row says to check the address. |
| `/admin/invitations` | Loading | `states/invitations-list--loading.png` | Prof Mbatha | Hold a read, then Invitations in the menu. No change. |
| `/admin/invitations` | Narrow | `states/invitations-list--narrow.png` | Prof Mbatha | At Step 2.16, at 390 px, after the issues. No change. |
| `/admin/users` | KGK's staff | `states/users-list--staff.png` | Prof Mbatha | At Step 2.12. |
| `/admin/users` | Filtered | `states/users-list--filtered.png` | Prof Mbatha | At Step 2.12, "zulu". |
| `/admin/users` | No match | `states/users-list--no-match.png` | Prof Mbatha | At Step 2.12, filter "zzz": "No users match the current filter." No change. |
| `/admin/users` | With pending trainees | `states/users-list--pending.png` | Prof Mbatha | At Step 2.28. |
| `/admin/users` | After an erasure | `states/users-list--after-erasure.png` | Prof Mbatha | At Step A.1.14. |
| `/admin/users` | Loading | `states/users-list--loading.png` | Prof Mbatha | Hold a read, then Users in the menu. No change. |
| `/admin/users` | Narrow | `states/users-list--narrow.png` | Prof Mbatha | At Step A.7.9. |
| `/admin/users/{UserId}` | Her own account | `states/user-detail--own.png` | Prof Mbatha | At Step 2.12. |
| `/admin/users/{UserId}` | Another's, before a change | `states/user-detail--other.png` | Prof Mbatha | At Step 2.13, Dr Zulu before Add role. |
| `/admin/users/{UserId}` | Role added | `states/user-detail--role-added.png` | Prof Mbatha | At Step 2.13. |
| `/admin/users/{UserId}` | A pending trainee | `states/user-detail--pending-trainee.png` | Prof Mbatha | At Step 2.28: PendingTrainee "System-managed". |
| `/admin/users/{UserId}` | No roles | `states/user-detail--no-roles.png` | Prof Mbatha | At Step 5.17. |
| `/admin/users/{UserId}` | Reset refused | `states/user-detail--reset-refused.png` | Prof Mbatha | At Step A.4.5, the 10-character password. |
| `/admin/users/{UserId}` | Reset | `states/user-detail--reset.png` | Prof Mbatha | At Step A.4.5, the second password. |
| `/admin/users/{UserId}` | Locked out | `states/user-detail--locked.png` | Prof Mbatha | At Step A.6.4. |
| `/admin/users/{UserId}` | Reactivated | `states/user-detail--reactivated.png` | Prof Mbatha | At Step A.6.7. |
| `/admin/users/{UserId}` | An administrator's own | `states/user-detail--administrator-own.png` | devadmin | At Step A.6.9. |
| `/admin/users/{UserId}` | Out of scope | `states/user-detail--unavailable.png` | Prof Mbatha | At Step A.5.5: "User unavailable". |
| `/admin/users/{UserId}` | Pending invitations | `states/user-detail--pending-invitations.png` | Prof Mbatha | Scratch (post-appendix): invite `botha@kgk.wombat.local` again, then open Dr Botha's page: the invitation listed with Revoke. |
| `/admin/users/{UserId}` | Loading | `states/user-detail--loading.png` | Prof Mbatha | Hold a read, then Manage on a row. No change. |
| `/admin/users/{UserId}` | Narrow | `states/user-detail--narrow.png` | Prof Mbatha | At Step A.7.9. |
| `/admin/assessors` | None | `states/assessors-list--empty.png` | Prof Mbatha | At Step 2.14, before the first save. |
| `/admin/assessors` | Five | `states/assessors-list--five.png` | Prof Mbatha | At Step 2.15. |
| `/admin/assessors` | Loading | `states/assessors-list--loading.png` | Prof Mbatha | Hold a read, then Assessors in the menu. No change. |
| `/admin/assessors` | Narrow | `states/assessors-list--narrow.png` | Prof Mbatha | At Step 2.15, at 390 px. No change. |
| `/admin/assessors/edit` | New | `states/assessor-profile-edit--new.png` | Prof Mbatha | At Step 2.14, before choosing. |
| `/admin/assessors/edit` | With a completion date | `states/assessor-profile-edit--provisional.png` | Prof Mbatha | At Step 2.14, Dr Khumalo as Provisional: the date field shown. |
| `/admin/assessors/edit` | Required field empty | `states/assessor-profile-edit--invalid.png` | Prof Mbatha | At Step 2.14, Save with Qualifications empty. Nothing is sent. No change. |
| `/admin/assessors/edit` | Saved | `states/assessor-profile-edit--saved.png` | Prof Mbatha | At Step 2.14: "Assessor profile saved." |
| `/admin/assessors/edit` | Unknown id | `states/assessor-profile-edit--not-found.png` | Prof Mbatha | Typed: `/admin/assessors/edit?id=999999`: the load's danger alert. No change. |
| `/admin/assessors/edit` | Narrow | `states/assessor-profile-edit--narrow.png` | Prof Mbatha | At Step 2.14, at 390 px. No change. |
| `/admin/trainees` | Pending admission | `states/pending-trainees-list--pending.png` | Prof Mbatha | At Step 2.28. |
| `/admin/trainees` | Some admitted | `states/pending-trainees-list--admitted.png` | Prof Mbatha | At Step 2.29. |
| `/admin/trainees` | None pending | `states/pending-trainees-list--none-pending.png` | Prof Mbatha | At Step 2.30. |
| `/admin/trainees` | Completed and closed | `states/pending-trainees-list--closed.png` | Prof Mbatha | At Step 5.27. |
| `/admin/trainees` | Loading | `states/pending-trainees-list--loading.png` | Prof Mbatha | Hold a read, then Trainees in the menu. No change. |
| `/admin/trainees` | Narrow | `states/pending-trainees-list--narrow.png` | Prof Mbatha | At Step A.7.9. |
| `/admin/trainees/edit` | Admission | `states/trainee-profile-edit--admission.png` | Prof Mbatha | At Step 2.29, before saving. |
| `/admin/trainees/edit` | An active profile | `states/trainee-profile-edit--active.png` | Prof Mbatha | At Step 2.29, after the admission. |
| `/admin/trainees/edit` | Saved | `states/trainee-profile-edit--saved.png` | Prof Mbatha | At Step 2.30: "Trainee profile saved." |
| `/admin/trainees/edit` | Mark complete dialog | `states/trainee-profile-edit--complete-dialog.png` | Prof Mbatha | At Step 5.16. |
| `/admin/trainees/edit` | A future day refused | `states/trainee-profile-edit--future-refused.png` | Prof Mbatha | At Step 5.16. |
| `/admin/trainees/edit` | Completed | `states/trainee-profile-edit--completed.png` | Prof Mbatha | At Step 5.17. |
| `/admin/trainees/edit` | Deactivate dialog | `states/trainee-profile-edit--deactivate-dialog.png` | Prof Mbatha | At Step 5.27. |
| `/admin/trainees/edit` | Deactivated | `states/trainee-profile-edit--deactivated.png` | Prof Mbatha | At Step 5.27. |
| `/admin/trainees/edit` | Curriculum not adopted | `states/trainee-profile-edit--curriculum-refused.png` | Prof Mbatha | At Step 6.34. |
| `/admin/trainees/edit` | Unknown id | `states/trainee-profile-edit--not-found.png` | Prof Mbatha | Typed: `/admin/trainees/edit?id=999999`: "Profile unavailable" under the load's alert. No change. |
| `/admin/trainees/edit` | Narrow | `states/trainee-profile-edit--narrow.png` | Prof Mbatha | At Step A.7.9. |

## Platform operations

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/admin/audit` | An institution's log | `states/audit-list--institution.png` | Prof Mbatha | At Step 3.55, From set. |
| `/admin/audit` | Failures only | `states/audit-list--failures.png` | Prof Mbatha | At Step 3.55. |
| `/admin/audit` | Every institution | `states/audit-list--administrator.png` | devadmin | At Step 3.57, with the pager's Next enabled. |
| `/admin/audit` | No entries | `states/audit-list--empty.png` | Prof Mbatha | At Step 3.55, Action `NoSuchCommand`, applied. No change. |
| `/admin/audit` | Loading | `states/audit-list--loading.png` | Prof Mbatha | Hold a read, then Audit log in the menu. No change. |
| `/admin/audit` | Narrow | `states/audit-list--narrow.png` | Prof Mbatha | At Step 3.55, at 390 px. No change. |
| `/admin/audit/{Id:guid}` | A failure | `states/audit-detail--failure.png` | Prof Mbatha | At Step 3.56. |
| `/admin/audit/{Id:guid}` | With its payload | `states/audit-detail--payload.png` | devadmin | At Step 3.57. |
| `/admin/audit/{Id:guid}` | Not among hers | `states/audit-detail--not-found.png` | Prof Mbatha | After Step 3.57, typed with the id of devadmin's RunScheduledJobNowCommand entry: "Audit entry not found". No change. |
| `/admin/audit/{Id:guid}` | Narrow | `states/audit-detail--narrow.png` | Prof Mbatha | At Step 3.56, at 390 px. No change. |
| `/admin/jobs` | Nine jobs | `states/scheduled-jobs-list--loaded.png` | devadmin | At Step A.2.1. |
| `/admin/jobs` | A job disabled | `states/scheduled-jobs-list--disabled.png` | devadmin | At Step A.2.2, before enabling it again. |
| `/admin/jobs` | Dispatched | `states/scheduled-jobs-list--dispatched.png` | devadmin | At Step A.2.4. |
| `/admin/jobs` | Loading | `states/scheduled-jobs-list--loading.png` | devadmin | Hold a read, then Scheduled jobs in the menu. No change. |
| `/admin/jobs` | Narrow | `states/scheduled-jobs-list--narrow.png` | devadmin | At Step A.7.11. |
| `/admin/jobs/runs` | The history | `states/scheduled-job-runs-list--loaded.png` | devadmin | At Step A.2.10. |
| `/admin/jobs/runs` | One job | `states/scheduled-job-runs-list--filtered.png` | devadmin | At Step A.2.10, the full key. |
| `/admin/jobs/runs` | No runs | `states/scheduled-job-runs-list--empty.png` | devadmin | At Step A.2.10, status Failed. |
| `/admin/jobs/runs` | Narrow | `states/scheduled-job-runs-list--narrow.png` | devadmin | At Step A.7.11. |
| `/admin/sso/group-mappings` | No provider configured | `states/group-mappings--no-provider.png` | Prof Mbatha | At Step A.3.1. |
| `/admin/sso/group-mappings` | Narrow | `states/group-mappings--narrow.png` | Prof Mbatha | At Step A.3.1, at 390 px. No change. |
| `/admin/curriculum-progress` | Before a rebuild | `states/curriculum-progress-rebuild--default.png` | devadmin | At Step 6.38, before pressing. |
| `/admin/curriculum-progress` | Confirm dialog | `states/curriculum-progress-rebuild--dialog.png` | devadmin | At Step 6.38. |
| `/admin/curriculum-progress` | Rebuilt | `states/curriculum-progress-rebuild--result.png` | devadmin | At Step 6.38: the five figures. |
| `/admin/curriculum-progress` | Failed | `states/curriculum-progress-rebuild--failed.png` | devadmin | Scratch (post-appendix): hold a read on the scratch database, confirm a rebuild, wait 35 s, then `ROLLBACK;`. The refusal's audit row waits for the lock too, so the page shows "The rebuild failed and nothing was changed: …" once the hold is released. |
| `/admin/curriculum-progress` | Narrow | `states/curriculum-progress-rebuild--narrow.png` | devadmin | At Step 6.38, at 390 px, before pressing. No change. |
| `/admin/data-rights` | A coordinator's queue | `states/data-rights-requests--coordinator.png` | Mr Smit | At Step A.1.3, filters applied. |
| `/admin/data-rights` | Nothing matches | `states/data-rights-requests--empty.png` | Mr Smit | At Step A.1.3, before approving: type Erasure, applied: "No requests". No change. |
| `/admin/data-rights` | Three requests | `states/data-rights-requests--three.png` | Mr Smit | At Step A.1.8. |
| `/admin/data-rights` | Every institution | `states/data-rights-requests--administrator.png` | devadmin | At Step A.1.12. |
| `/admin/data-rights` | Loading | `states/data-rights-requests--loading.png` | Mr Smit | Hold a read, then Data rights requests in the menu. No change. |
| `/admin/data-rights` | Narrow | `states/data-rights-requests--narrow.png` | Mr Smit | At Step A.7.5. |
| `/admin/data-rights/{Id:guid}` | Submitted | `states/data-rights-request--submitted.png` | Mr Smit | At Step A.1.3, before approving. |
| `/admin/data-rights/{Id:guid}` | No decision note | `states/data-rights-request--note-required.png` | Mr Smit | At Step A.1.3: "A decision note is required." |
| `/admin/data-rights/{Id:guid}` | Approved | `states/data-rights-request--approved.png` | Mr Smit | At Step A.1.3. |
| `/admin/data-rights/{Id:guid}` | Withdrawn | `states/data-rights-request--withdrawn.png` | Mr Smit | At Step A.1.8. |
| `/admin/data-rights/{Id:guid}` | Rejected | `states/data-rights-request--rejected.png` | Mr Smit | At Step A.1.8. |
| `/admin/data-rights/{Id:guid}` | Erasure approved | `states/data-rights-request--erasure.png` | devadmin | At Step A.1.12. |
| `/admin/data-rights/{Id:guid}` | Not his | `states/data-rights-request--not-found.png` | Mr Smit | At Step A.5.7. |
| `/admin/data-rights/{Id:guid}` | Narrow | `states/data-rights-request--narrow.png` | Mr Smit | At Step A.1.8, at 390 px. No change. |

## Access denied, by page

Each gated page answers a role it does not admit with the access-denied page itself (`Routes.razor`), whose states are
captured above. What differs is only the address, so these are listed for the record, not captured again: the story
already reaches most of them. A signed-out visitor is sent to the sign-in page instead, and back after signing in
(Step A.4.7).

| Page | Refused to | Where |
|---|---|---|
| `/admin/invitations` | Coordinator | Step 2.32 |
| `/committee/panels/new` | Coordinator | Step 2.32 |
| `/portfolio/progress` | PendingTrainee | Step 2.19 |
| `/admin/entrustment-scales`, `/admin/entrustment-scales/new`, `/admin/colleges/{Id:int}` | CollegeAdmin | Steps 1.15, 6.10 |
| `/committee/decisions-due` | CommitteeMember | Step 4.5 |
| `/committee/reviews/{ReviewId:int}` | Trainee | Step 4.41 |
| `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports` | former trainee | Step 5.22 |
| `/admin/users`, `/admin/jobs` | Trainee | Step A.5.1 |
| `/admin/institutions`, `/admin/jobs` | InstitutionalAdmin | Step A.5.2 |
| `/admin/curricula/{Id:int}`, `/admin/curriculum-progress`, `/admin/data-rights`, `/admin/colleges/{CollegeId:int}/specialities` | InstitutionalAdmin | Typed by Prof Mbatha any time after Act 1. No change. |
| `/msf/campaigns`, `/msf/reports/{CampaignId:int}`, `/msf/coverage` | Trainee | Typed by Dr Dlamini any time. No change. |
| `/admin/entrustment-decisions` | Coordinator | Typed by Mr Smit any time. No change. |

## States no local replay reaches

Each is either missing from the product, needs something the replay environment lacks, or is guarded so that no cast
member can meet it. None is captured.

- **Institutional sign-in.** The sign-in page's "or" divider and "Sign in with …" buttons, the link page's password form
  and its refusals, and the SSO mappings page's Add mapping form, mapping rows and Delete. Each needs a configured
  identity provider (`Sso:Providers` is empty on dev; `coverage.md` § Flows and states not played). The refusal texts
  alone are reachable as typed codes on the sign-in page.
- **Dashboard cards that nothing fills.** The Trainee's "Upcoming deadlines" with a row (it reads `due_date` keys no
  type has). The Assessor's "Accepted, needing action", which no CPSA or KGK workflow could fill, is now "Waiting for
  your rating" (T297, T335), read from the inbox, and Steps 3.24, 3.33, 3.51 and A.6.8 fill it.
- **Trainee states the admissions never produce.** "No curriculum assigned yet" on Home and "No curriculum items assigned
  yet" on My progress (a Trainee with no profile: admission sets the role and the profile together), "Your programme
  starts on …" (every start is on or before `D`), "You started part-way through a period" (every start is 15 January,
  the semester boundary, D42), and "No EPA on your curriculum is in use" (every EPA of a curriculum inactive at once).
- **Home for an account with no role and no training record**: no subtitle and one card, "No role assigned" ("Your
  account holds no role at the moment, so there is nothing to show you here. An administrator can give you one."), with
  Home, then My data rights, as the menu. Nobody in the story loses their last role without keeping a trainee record:
  Dr Molefe keeps hers, so her Home is "Your training record". A cheap scratch (post-act6), if wanted: devadmin removes
  Dr van Rensburg's one role, CommitteeMember, on his user page, and he signs in again.
- **An activity counted across scales** ("counted towards the required number of observations, but not towards the
  supervision level"): every rated instrument and every item uses the v11.1 ladder.
- **A workflow action shown disabled with its reason** (T107): no seeded or KGK workflow leaves a required field its
  mover cannot write.
- **"Filed. It is now …"**, the notice for a type whose create is its filing: only the Demo generic types are born
  requested or terminal, and no KGK registrar is offered them.
- **Committee review notes nobody in the cast can cause.** "The chair rated all of this evidence" (every snapshot in the
  story has more than one assessor), "Sampling figures are incomplete" (evidence from another institution, or a
  malformed rating), the trainee-elsewhere note (no page moves a trainee to another institution), and "Starting now
  leaves out N feedback campaigns" (the story releases its campaign before any review starts).
- **The "holds Trainee" notes** on Users, a user, the panel list and form, Committee reviews, the MSF campaign pages and
  MSF coverage (T218, T224, T237, T256, T278): no cast member holds Trainee beside a staff role. So is the panel form's
  "You cannot create a decision panel" card (T194): every panel manager in the cast has a scope to create in.
- **MSF respondent refusals.** "Feedback link revoked": nothing in the product sets an MSF link's revoked time, so the
  refusal cannot happen. "The response is not complete": the browser's required check stops the submit before the
  server sees it.
- **A scale rename that unbinds a form** (T253): every seeded form binds by seed key and the builder writes an id or a
  seed key, so no form binds a scale by name.
- **The data-rights refusal "PseudonymSalt is not configured"**: the replay host must set the salt (appendix, starting
  state).
- **Scheduled jobs.** A Failed run badge (no job fails on the replay) and Run now refused while the job runs (timing).
- **The register page's skeleton**: the page is static for its only audience, so its read runs before the page is sent.
- **`/admin/specialities`**: it redirects before it renders, so it has no state of its own.
- **The error page through a real failure, on the replay**: the replay runs in Development, whose developer exception
  page answers a failure. Outside Development the error page does (`ErrorPages`, T321): § System pages' "After a
  failure" row says how to reach it from another environment, and `Hosting/ErrorPageFlowTests` plays it. Step A.5.8
  captures the page by its address.
- **Empty lists the seeders rule out**: Colleges, Institutions, Entrustment scales and Scheduled jobs always hold rows,
  and an InstitutionalAdmin's Users list always holds herself.
- **The reconnect dialog's Reloading state** ("Reloading the page…"): it shows only while the reload it starts is under
  way.
- **Loading of secondary reads inside a page** (the review page's standing and MSF coverage cards, My progress's MSF
  line): they run after the main read, so a hold cannot fail one without the other.
