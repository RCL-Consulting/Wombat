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
  Wombat sets another), and the page shows its danger alert: the raw timeout text on most pages, a fixed sentence with
  Try again where the page words its own (My account, T339). Capture, then `ROLLBACK;`.
- **Keep a hold under two minutes.** The circuit's sign-in check runs once a minute and a hold makes it fail; only the
  third failure in a row signs the circuit out (`SessionRevalidatingAuthenticationStateProvider`). The scheduler and any
  other browser wait meanwhile.
- **Static pages** (signed out, `/msf/respond`, `/portfolio/verify`) have no circuit: a hold delays the response, and
  past 30 seconds they show their own failure state, listed on each.

### Typed codes on the account pages

The sign-in, register, change-password and My account pages choose their words from a code in the address (`?error=`,
and `?status=` on My account), never from text in it (T265, T285). Typing a code shows exactly the page a real refusal
shows, with nothing sent. Where the story plays the real refusal, the row uses the story; the rest are typed.

- **Sign-in** (`/account/login?error=<code>`): `FieldsMissing`, `Refused`, `TooManyAttempts`, `SessionEnded`,
  `PasswordChanged`, `SignedOut`, `LockedSignedOut`, `ExternalLoginUnavailable`, `ExternalSessionExpired`, `SsoFailed`,
  the twelve institutional sign-in codes (`SsoUnknownProvider`, `SsoNoEmail`, `SsoEmailNotVerified`,
  `SsoAccountLocked`, `SsoAdministrator`, `SsoWrongInstitution`, `SsoEmailInUse`, `SsoAccountNotCreated`,
  `SsoAlreadyLinked`, `SsoLinkRefused`, `SsoLinkLockedOut`, `SsoLinkFailed`), and any other code, which gets the
  general refusal. `LockedOut` reads as `Refused` does (T287): no endpoint sends it since T339. `SessionEnded`,
  `PasswordChanged`, `SignedOut` and `LockedSignedOut` are notices (info, the focus on Email on dev); every other code is
  a refusal (danger, focused, the tab's title starting "Error:").
- **Change password** (`/account/change-password?error=<code>`, repeatable): `FieldsMissing`, `ConfirmationMismatch`,
  `Failed`, `TooManyAttempts`, `InstitutionalSignIn`, `AccountLocked` (with `&minutes=N`: "Your account is locked. Wait N
  minutes, then try again.", for an account already locked by someone else; no sign-out), and Identity's `PasswordMismatch`, `PasswordTooShort`,
  `PasswordRequiresUniqueChars`, `PasswordRequiresDigit`, `PasswordRequiresUpper`, `PasswordRequiresLower`,
  `PasswordRequiresNonAlphanumeric`; any other code (`LockedOut` included) gets "The password could not be changed.
  Try again." A change lands on My account (`/account/profile?status=password-updated`).
- **My account** (`/account/profile?error=<code>` or `?status=<code>`): `?error=FirstNameMissing`, `LastNameMissing`,
  `NameMissing`, `NameTooLong`, `Failed`; `?status=saved` and `?status=password-updated`. The How you sign in card's
  results take `&provider=<key>` as well: `?status=sign-in-removed`, and `?error=RemoveWrongPassword`,
  `RemoveTooManyAttempts`, `RemoveAccountLocked` (with `&minutes=N`), `RemoveLastSignIn`, `RemoveFailed`. With no institutional sign-in linked (every cast account
  on dev) each shows in the card, naming no institution; the dialog reopened with its refusal needs a linked sign-in
  (States no local replay reaches).
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
| Reconnect: attempt started | `states/shell--reconnect-attempt-started.png` | Dr Zulu | With the app stopped and the count running, listen on its port without answering on both loopbacks, since Chrome's attempts go to `[::1]` first (PowerShell: `$l = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, <port>); $l.Start(); $l6 = [Net.Sockets.TcpListener]::new([Net.IPAddress]::IPv6Loopback, <port>); $l6.Start()`). At the next attempt the line reads "Trying again now." over a running bar, under the same "Could not reconnect", which keeps the focus (one state, its line replaced), and holds while the attempt waits on the listener (confirmed 2026-09-27: over 6.5 s). Capture it, then stop both listeners. No change. |
| Reconnect: failed | `states/shell--reconnect-failed.png` | Dr Zulu | Keep the app stopped until the attempts run out (about six minutes: ten at once, ten 5 s apart, ten 30 s apart): "Connection lost", "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and anything not yet saved on it is lost." and Try again. Restart the app and press Try again: the circuit is gone and the restarted server kept nothing to resume it from, so "Reloading" and "Reloading the page…" show as the page reloads. No change. |
| Reconnect: failed at narrow width | `states/shell--reconnect-failed-narrow.png` | Dr Zulu | As Reconnect: failed, at 390 px: the dialog 358 px wide, 16 px from each side, and Try again the full width of the dialog's content (318 px inside its 20 px padding), 44 px tall. No change. |
| Reconnect: paused | `states/shell--reconnect-paused.png` | Dr Zulu | On a page whose connection has never dropped (a fresh sign-in; after a reconnect, .NET 10's `pauseCircuit()` resolves true and does nothing, because only a resume clears its paused flag), run `Blazor.pauseCircuit()` in the browser's console: "Page paused" and "This page is paused. Resume to carry on." with Resume. Resume restores a live circuit. No change. |
| Reconnect: resume failed | `states/shell--reconnect-resume-failed.png` | Dr Zulu | Pause as above, stop the app, press Resume: "Could not resume", the sentence Connection lost gives, and Try again, which takes the focus from Resume. Restart the app and press Try again: the page resumes, or shows "Reloading the page…" and reloads when the server has nothing to resume it from (needs confirmation: which). No change. |
| Unhandled error banner | `states/shell--error-banner.png` | Dr Dlamini (Trainee) | Needs confirmation: until T342 it was reached through New activity, whose type list had no error handling; since T342 that page words its own failure (`new-activity--load-error` under Activities), so the banner needs another page whose read throws unhandled. On such a page, hold a read past its command timeout: the bar "This page no longer responds; copy anything you need, then reload." with Reload and Dismiss, small (`btn-sm`) buttons with their refresh-cw and x icons, appears beside the sidebar as the circuit ends. `ROLLBACK;`, then Reload. No change. |
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
| Assessor, first sign-in | `states/home--assessor-empty.png` | Dr Patel | At Step 2.36: "Waiting for you" with no badge, "Nothing is waiting for you.", Open Activity inbox; Recent decisions "No decisions yet.", with no All your decisions. |
| Assessor with a request waiting | `states/home--assessor-pending.png` | Dr Zulu | At Step 3.33, after the switch: "Waiting for you" in the warning stripe, badged "1 waiting, 1 overdue", the rule line "Oldest first. Overdue once it has waited 7 days.", the Mini-CEX "from Nomsa Mahlangu" with Requested and Overdue beside it, "Waiting 8 days". |
| Assessor with two waiting, none overdue | `states/home--assessor-two.png` | Dr Patel | At Step 3.24, before opening the DOPS: "Waiting for you" badged "2 waiting", the portfolio review then the DOPS, each "Waiting less than a day", in the inbox's order. |
| Assessor with recent decisions | `states/home--assessor-decisions.png` | Dr Khumalo | At Step 3.51: each decision linked by its full name with "from <registrar>", its badge (Completed in green, Declined in red) and its day; All your decisions. |
| Committee member with one overdue in the inbox | `states/home--committee-line-overdue.png` | Dr Zulu | At Step 3.33, before the switch: under the header, above the committee cards, the warning line "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics) · PAED-004 · `D−3`, from Nomsa Mahlangu, waiting 8 days." with Open it. |
| Committee member with one waiting in the inbox | `states/home--committee-line-one.png` | Dr Naidoo | At Step 5.25, before the switch: the info line "1 activity waits for you in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · `D−1`, from Pieter du Plessis, waiting less than a day." with Open it. |
| Committee member with several waiting in the inbox | `states/home--committee-line-several.png` | Dr Botha | Scratch (post-act3): Dr Dlamini files two Mini-CEX requests naming Dr Botha and submits both. Dr Botha, acting as Committee member, reads the info line "2 activities wait for you in the Activity inbox. The oldest: Mini-CEX (Paediatrics) · …, from Anele Dlamini, waiting less than a day." with Open the oldest. Then age the first by eight days as Step 3.30's Note ages its two (the activity's `UpdatedOn` and its history rows) and reload: the warning line "2 activities wait for you in the Activity inbox; 1 is overdue. The oldest: …, waiting 8 days." |
| SpecialityAdmin | `states/home--speciality-admin.png` | Dr Mokoena | At Step 2.38. |
| SpecialityAdmin with figures | `states/home--speciality-admin-figures.png` | Dr Mokoena | At Step 3.53. |
| SubSpecialityAdmin | `states/home--sub-speciality-admin.png` | Dr Sithole | At Step 3.54. |
| Trainee awaiting admission | `states/home--awaiting-admission.png` | Dr Mahlangu (PendingTrainee) | At Step 2.18: the one Awaiting admission card, and the pending trainee's nav. |
| Trainee, admitted, nothing filed | `states/home--trainee-first.png` | Dr Molefe | At Step 2.39: "0 / 10", "0 / 5", the five largest shortfalls. |
| Trainee with activity | `states/home--trainee.png` | Dr Dlamini | At Step 3.50. |
| Trainee with work returned | `states/home--trainee-returned.png` | Dr Ndlovu | At Step 3.16: the Needs you card, badged 1, lists the reflection, "Returned to you by Sarah Botha on `D`. Change it and submit again." |
| Trainee whose programme ended | `states/home--trainee-ended.png` | Dr du Plessis | At Step 5.28: "Your programme ended on …, so no target applies to you any more." |
| Former trainee, no role | `states/home--no-role.png` | Dr Molefe (former trainee) | At Step 5.21: no subtitle, and one card, "Your training record", pointing to My progress (T335; the step's expectation, the ended line, is T311's). |
| Loading | `states/home--loading.png` | Dr Dlamini | Hold a read, then choose Home in the menu: the header, each card's title and a skeleton in each card, and nothing to press in the cards; a screen reader hears "Loading your Home." (T350). No change. |
| Loading, the assessor's | `states/home--assessor-loading.png` | Dr Patel | The same, as Dr Patel after Step 3.24: Waiting for you and Recent decisions, each its title and a skeleton; "Loading your Home." to a screen reader. `ROLLBACK;`. No change. |
| Load error | `states/home--load-error.png` | Dr Dlamini | The same, held 35 s: one alert, "Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.", with Try again, and no cards. `ROLLBACK;`, then Try again shows the cards. No change. |
| Narrow: trainee | `states/home--narrow-trainee.png` | Dr Dlamini | At Step A.7.3. |
| Narrow: former trainee | `states/home--narrow-former-trainee.png` | Dr Molefe | At Step A.7.4. |
| Narrow: assessor | `states/home--narrow-assessor.png` | Dr Patel | At Step A.7.2, open Home at 390 px before the inbox: "Waiting for you" badged "2 waiting, 1 overdue", the rows stacked, Open Activity inbox 44 px tall. No change. |
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

The sign-in page has no signed-in state: a signed-in visitor who opens `/account/login` is sent Home (T339, E4). The
sign-out page (`/account/logout` and `/account/logout-confirm`) sends a visitor who is not signed in to the sign-in page.

| Page | State | Screenshot | Account | How to reach it |
|---|---|---|---|---|
| `/account/login` | Blank | `states/login--blank.png` | Anonymous | At Step 1.1, before signing in: "Work-based assessment for specialist training.", "Sign in", the focus on Email. No SSO button (no provider). |
| `/account/login` | Password shown | `states/login--password-shown.png` | Anonymous | At Step 1.1, type a password and press Show: the toggle filled (pressed), its eye-off icon, the password as text. No change. |
| `/account/login` | Scripts blocked | `states/login--no-script.png` | Anonymous | DevTools, Disable JavaScript, then open the page: no Show toggle, only the password field; the form still signs in, as it posts without script. No change. |
| `/account/login` | Wrong password | `states/login--refused.png` | Anonymous | At Step A.3.2: "Invalid email or password.", focused; the tab's title starts "Error:". |
| `/account/login` | Signed out | `states/login--signed-out.png` | Mr Smit | At Step 2.8, after Sign out: "You have signed out.", an information notice. Also typed: `?error=SignedOut`. |
| `/account/login` | Session ended | `states/login--session-ended.png` | Dr Mahlangu | At Step 2.31: "Your session has ended. Sign in again.", an information notice, the focus on Email. Also typed: `?error=SessionEnded`. |
| `/account/login` | Locked account | `states/login--locked-out.png` | Dr Patel | At Step A.6.5: "Invalid email or password.", the same page as a wrong password (T287). |
| `/account/login` | Locked by wrong current passwords | `states/login--locked-signed-out.png` | Dr Khumalo (a copy) | Scratch (post-act2): Dr Khumalo signs in and opens Change password from My account, and enters a wrong current password five times. The first four read "Incorrect password."; the fifth locks the account, ends her session and lands here: "Your current password was entered incorrectly too many times, so your account is locked for 15 minutes and you have been signed out. Wait 15 minutes, then sign in again.", an information notice. Also typed, with no change: `?error=LockedSignedOut`. |
| `/account/login` | Unknown SSO provider | `states/login--sso-unknown-provider.png` | Anonymous | At Step A.3.2, after the challenge for `kgk`: "That institution's sign-in is not set up in Wombat. Sign in with your email and password." |
| `/account/login` | No external sign-in | `states/login--external-unavailable.png` | Anonymous | At Step A.3.3: "Your institution's sign-in did not complete. Sign in with your email and password." |
| `/account/login` | Throttled | `states/login--too-many-attempts.png` | Anonymous | Typed: `?error=TooManyAttempts`: "Too many failed sign-in attempts from this network. Wait a few minutes and try again." No change. |
| `/account/login` | Fields missing | `states/login--fields-missing.png` | Anonymous | Typed: `?error=FieldsMissing`. No change. |
| `/account/login` | Password changed elsewhere | `states/login--password-changed.png` | Anonymous | Typed: `?error=PasswordChanged`. No change. |
| `/account/login` | External sign-in expired | `states/login--external-expired.png` | Anonymous | Typed: `?error=ExternalSessionExpired`. No change. |
| `/account/login` | Each institutional sign-in refusal | `states/login--sso-<code>.png` | Anonymous | Typed, one capture per code of § Typed codes, `SsoNoEmail` to `SsoLinkFailed`, and `SsoFailed`. No change. |
| `/account/login` | Unknown code | `states/login--general-refusal.png` | Anonymous | Typed: `?error=Call012`: "Sign-in could not be completed. Try again." No change. |
| `/account/login` | Narrow | `states/login--narrow.png` | Anonymous | At Step A.7.12. |
| `/account/register` | No token | `states/register--token-missing.png` | Anonymous | Typed: `/account/register`: "The invitation token is missing." No change. |
| `/account/register` | The form | `states/register--form.png` | Anonymous | At Step 1.8, before Register: "Registering kruger@cmsa.wombat.local as CollegeAdmin.", and the six password rules under Password. |
| `/account/register` | Password too short | `states/register--password-short.png` | Anonymous | At Step 2.9, the first attempt: "The new password needs: At least 12 characters." |
| `/account/register` | Confirmation differs | `states/register--confirmation.png` | Anonymous | At Step 2.9, the second attempt. |
| `/account/register` | Details invalid | `states/register--details-invalid.png` | Anonymous | After Step 2.26, before 2.27: Dr du Plessis's resent link with `&error=DetailsInvalid` added. No change. |
| `/account/register` | Used | `states/register--used.png` | Anonymous | At Step 1.9. |
| `/account/register` | Revoked | `states/register--revoked.png` | Anonymous | At Step 2.17. |
| `/account/register` | Invalid | `states/register--invalid.png` | Anonymous | At Step 2.27, the replaced link. |
| `/account/register` | Expired | `states/register--expired.png` | Anonymous | Scratch (post-appendix): Prof Mbatha invites `expired@kgk.wombat.local`; copy the link; `UPDATE "Invitations" SET "ExpiresOn" = now() - interval '1 day' WHERE "Email" = 'expired@kgk.wombat.local';`; open the link: "This invitation has expired." |
| `/account/register` | Address already has an account | `states/register--account-exists.png` | Anonymous | Scratch (post-appendix): Prof Mbatha invites `botha@kgk.wombat.local` as Assessor; open the link: "A user with this email address already exists." |
| `/account/register` | Could not be completed | `states/register--general-refusal.png` | Anonymous | Hold a read, open any registration link (static), wait 35 s: "Registration could not be completed. Try again." No change. |
| `/account/register` | Narrow | `states/register--narrow.png` | Anonymous | At Step 2.18, Dr Molefe's link at 390 px before registering. No change. |
| `/account/forgot-password` | The page | `states/forgot-password--stub.png` | Anonymous | At Step A.4.4: "Forgotten password", who to ask, three steps, and Back to sign in. |
| `/account/forgot-password` | Narrow | `states/forgot-password--narrow.png` | Anonymous | At Step A.7.12. |
| `/account/link-external` | No sign-in in progress | `states/link-external-login--expired.png` | Anonymous | At Step A.3.3: "Institutional sign-in expired" and "Your institutional sign-in has expired. Start again from the sign-in page." |
| `/account/link-external` | Narrow | `states/link-external-login--narrow.png` | Anonymous | At Step A.3.3, at 390 px. No change. |
| `/account/logout-confirm` | The confirmation | `states/logout-confirm--default.png` | Mr Smit | At Step A.4.7, the first visit: "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends your session in this browser.", Cancel and Sign out. |
| `/account/logout` | The same page, typed | `states/logout-confirm--typed.png` | Mr Smit | At Step A.4.7, the typed `/account/logout`: the same page; nobody is signed out. No change. |
| `/account/logout-confirm` | Narrow | `states/logout-confirm--narrow.png` | Mr Smit | At Step A.4.7, at 390 px, then Cancel. No change. |
| `/account/profile` | Before admission | `states/profile--pending-trainee.png` | Dr Mahlangu | At Step 2.19: Roles reads "Pending trainee", Institution Kgosi Kgari Teaching Hospital. |
| `/account/profile` | Two roles | `states/profile--loaded.png` | Dr Zulu | At Step 2.41: the Account card, the roles one per line, Your name, and How you sign in with Password and Change password. |
| `/account/profile` | Saved | `states/profile--saved.png` | Dr Khumalo | At Step 2.41: "Name saved." in the Your name card. |
| `/account/profile` | Required field empty | `states/profile--invalid.png` | Dr Botha | At Step A.4.1, the last name of a single space: "Your name was not saved. Enter your last name." in the card, which takes the focus; Last name marked and empty, "Enter your last name." under it. The cleared last name before it is stopped by the browser's own required-field check, and nothing is sent. Typed with no change: `?error=FirstNameMissing`, `?error=NameMissing`. |
| `/account/profile` | Password updated | `states/profile--password-updated.png` | Dr Khumalo | At Step A.4.2 (d): "Password updated." under the header, focused. Also typed: `?status=password-updated`. |
| `/account/profile` | A removal's result, typed | `states/profile--remove-result.png` | Dr Botha | Typed, one capture each: `?status=sign-in-removed&provider=kgk` ("Institutional sign-in removed."), `?error=RemoveLastSignIn&provider=kgk` ("Your institutional sign-in was not removed. It is the only way you sign in to Wombat."), `?error=RemoveWrongPassword&provider=kgk` ("Your sign-in was not removed. Incorrect password."), each in the How you sign in card. No change. |
| `/account/profile` | Loading | `states/profile--loading.png` | Dr Botha | Hold a read, then My account from the name in the top bar. No change. |
| `/account/profile` | Load error | `states/profile--load-error.png` | Dr Botha | The same, held 35 s. No change. |
| `/account/profile` | Narrow | `states/profile--narrow.png` | Dr Khumalo | At Step 2.41, at 390 px. No change. |
| `/account/change-password` | Blank | `states/change-password--blank.png` | Dr Khumalo | At Step A.4.2, before the first try: the six rules under New password. |
| `/account/change-password` | Password shown | `states/change-password--shown.png` | Dr Khumalo | At Step A.4.2, before the first try, type in New password and press "Show new password". No change. |
| `/account/change-password` | Wrong current password | `states/change-password--incorrect.png` | Dr Khumalo | At Step A.4.2 (a): "Your password was not changed." and "Incorrect password." in the card, Current password marked; the tab title starts "Error:". |
| `/account/change-password` | Confirmation differs | `states/change-password--confirmation.png` | Dr Khumalo | At Step A.4.2 (b): "The password confirmation does not match.", Confirm new password marked. |
| `/account/change-password` | Rules broken | `states/change-password--rules.png` | Dr Khumalo | At Step A.4.2 (c): "The new password needs:" and the four rules broken, in the six rules' order; New password says "The new password does not meet the rules below." |
| `/account/change-password` | Throttled | `states/change-password--throttled.png` | Dr Khumalo | Typed: `?error=TooManyAttempts`: "Too many attempts from this network. Wait a few minutes and try again." No change. For real, Scratch (post-act2): sign Dr Khumalo in first; then, from the same machine, sign in ten times within five minutes with an address no account has (each "Invalid email or password."; the throttle is per client address and shared with sign-in); within the five minutes she presses Change password with any current password. |
| `/account/change-password` | Account already locked | `states/change-password--account-locked.png` | Dr Khumalo (a copy) | Scratch (post-act2): sign Dr Khumalo in; from another browser, sign in five times as her with a wrong password (each "Invalid email or password."; the fifth locks the account); within the lock she presses Change password with her right current password: "Your account is locked. Wait 15 minutes, then try again." in the card; she stays signed in and nothing changes (T339 review, fixsec 2). Also typed: `?error=AccountLocked&minutes=15`. |
| `/account/change-password` | Could not be changed | `states/change-password--general-refusal.png` | Dr Khumalo | Typed: `?error=Failed`: "The password could not be changed. Try again." No change. |
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
| `/activities/new` | The instrument picker (no type chosen) | `states/new-activity--blank.png` | Dr Dlamini | At Step 3.1, before choosing the type: "Log an activity", "Choose what you are filing. Each opens its own form.", and eleven links in three groups, Rated by an assessor (7 types), Discussed or reviewed, not rated (3) and Logged by you (1). |
| `/activities/new` | Before admission | `states/new-activity--pending-trainee.png` | Dr Mahlangu (PendingTrainee) | At Step 2.19: the same picker. |
| `/activities/new` | A rated form | `states/new-activity--mini-cex.png` | Dr Molefe | At Step 2.43: Request open; Entrustment and Feedback locked, "The assessor you name fills this in"; the check line, then Submit and Save draft. |
| `/activities/new` | A rated form, an assessor named | `states/new-activity--named.png` | Dr Dlamini | At Step 3.1, once Dr Naidoo is named: the locked sections read "David Naidoo fills this in", the check line names his Activity inbox, and the button reads Submit to David Naidoo. No change. |
| `/activities/new` | An unrated form | `states/new-activity--reflective.png` | Dr Ndlovu | At Step 3.14, before submitting: no Entrustment section, Discussion locked, "Sarah Botha fills this in". |
| `/activities/new` | A self-logged form | `states/new-activity--teaching-log.png` | Dr du Plessis | At Step 3.18, before logging: one section, the button Log, and "When you log it: it is Logged at once, and credits nothing. Nobody else acts on it." |
| `/activities/new` | Refused: a future date | `states/new-activity--refused-future.png` | Dr Ndlovu | At Step 3.8: the focused summary "Nothing was saved. Everything you typed is kept below." with its linked line, the date field marked with its own message. |
| `/activities/new` | Hint: before the programme | `states/new-activity--before-programme.png` | Dr Ndlovu | At Step 3.9, once `J−1d` is entered: "This date is before your programme started (`J`), and will not be accepted.", the field marked. |
| `/activities/new` | Refused: before the programme | `states/new-activity--refused-before-programme.png` | Dr Ndlovu | At Step 3.9, after Submit. |
| `/activities/new` | Late filing warning | `states/new-activity--late-warning.png` | Dr Ndlovu | At Step 3.10, once `D−20` is entered: the warning under the date, and the check line's "Filed today, 20 days after the encounter: it will be recorded as late." |
| `/activities/new` | File it again | `states/new-activity--file-again.png` | Dr Ndlovu | At Step 3.12, after File it again, to someone else: the notice "Copied from your request to Fatima Khumalo, which was declined. …", everything copied but the assessor, the late warning at once, and the button Submit. |
| `/activities/new` | Submitting | `states/new-activity--submitting.png` | Dr Ndlovu | At Step 3.10: hold a read, press Submit to Fatima Khumalo, and capture within 20 s: the button reads "Submitting…" and Save draft is disabled. `ROLLBACK;`: the submit then completes as the step expects. |
| `/activities/new` | Picker without a paused EPA | `states/new-activity--paused-epa.png` | Dr Mahlangu | At Step 6.20, the EPA picker open. |
| `/activities/new` | Loading | `states/new-activity--loading.png` | Dr Dlamini | Hold a read, open Log an activity from the menu, capture within 20 s: the header and three skeleton cards ("Loading what you can file." to a screen reader). `ROLLBACK;`. No change. |
| `/activities/new` | Load error | `states/new-activity--load-error.png` | Dr Dlamini | The same, held about 65 s: the programme start's read times out and is passed over, then the type list's, and the page reads "Could not load what you can file. Nothing has changed. Try again, or come back in a few minutes." with Try again. `ROLLBACK;`, then Try again shows the picker. No change. |
| `/activities/new` | Narrow | `states/new-activity--narrow.png` | Dr Dlamini | At Step A.6.6, at 390 px, with a Mini-CEX chosen. No change. |
| `/activities/{ActivityId:int}` | Draft saved | `states/activity-view--draft-saved.png` | Dr Dlamini | At Step 3.1: the focused result "Draft saved. It has not been submitted. It is in nobody's inbox until you submit it." and the status card "With you. Not submitted yet." |
| `/activities/{ActivityId:int}` | Refused submit | `states/activity-view--refused-submit.png` | Dr Dlamini | At Step 3.2: the focused summary "Not submitted. It is still a draft. Fix the field below and submit again." with its linked line "Presenting problem: A value is required.", the field marked. |
| `/activities/{ActivityId:int}` | Saved as a draft, not submitted | `states/activity-view--not-submitted.png` | Dr Dlamini | At Step A.7.1, after the empty Submit: "Saved as a draft, but not submitted. Fix the 6 fields below and submit again." |
| `/activities/{ActivityId:int}` | Submitting | `states/activity-view--submitting.png` | Dr Dlamini | At Step 3.3: hold a read, press Submit to David Naidoo, and capture within 20 s: the button reads "Submitting…", every other action disabled. `ROLLBACK;`: the submit then completes as the step expects. |
| `/activities/{ActivityId:int}` | Requested, the trainee's view | `states/activity-view--requested.png` | Dr Dlamini | At Step 3.3: "Submitted. It is now Requested. It is in David Naidoo's Activity inbox.", the status card "With David Naidoo since …" with its one action, Cancel request…. |
| `/activities/{ActivityId:int}` | Cancel request dialog | `states/activity-view--cancel-request-dialog.png` | Dr Dlamini | At Step 3.3, press Cancel request… on the status card: "Cancel this request?", "It leaves David Naidoo's Activity inbox. A cancelled request cannot be reopened, and it credits nothing.", Keep the request and Cancel request. Press Keep the request. No change. |
| `/activities/{ActivityId:int}` | Submitted notice, late | `states/activity-view--submitted-late.png` | Dr Ndlovu | At Step 3.10: "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox.", the status card's "Filed 20 days after the encounter: recorded as late.", and the history's "Filed 20 days after the encounter". |
| `/activities/{ActivityId:int}` | Unavailable | `states/activity-view--unavailable.png` | Dr Patel | At Step 3.4: "Activity unavailable", "This activity does not exist, or you cannot open it.", Go to Activity inbox, the list his acting role opens activities from (T350). |
| `/activities/{ActivityId:int}` | Assessor to rate | `states/activity-view--to-rate.png` | Dr Naidoo | At Step 3.5, before rating: "Your move. Anele Dlamini asked you on …", Request's Assessor "David Naidoo" with no email, Entrustment and Feedback open, the rung picker's six radios with none chosen and "What each rung means" open under them; Complete, Discard changes (greyed, "Nothing to discard yet." beside it) and Decline. |
| `/activities/{ActivityId:int}` | Assessor, a rung chosen | `states/activity-view--rated.png` | Dr Naidoo | At Step 3.5, rung 4 chosen: "4" filled and marked "chosen", its descriptor "Unsupervised practice. The trainee carries the responsibility for the activity." under the row, "What each rung means" shut; Discard changes enabled, its reason gone. |
| `/activities/{ActivityId:int}` | Completing | `states/activity-view--completing.png` | Dr Naidoo | At Step 3.5: hold a read, press Complete, and capture within 20 s: the button reads "Completing…", every other action disabled, "Completing." to a screen reader. `ROLLBACK;`: the completion then goes through as the step expects. |
| `/activities/{ActivityId:int}` | Completed, nothing else waiting | `states/activity-view--completed-last.png` | Dr Naidoo | At Step 3.5, after Complete: the focused result "Completed. Nothing else waits for you.", then Go to Home; the status card "Done. You completed it on …". |
| `/activities/{ActivityId:int}` | Completed, one more waiting | `states/activity-view--completed-next.png` | Dr Patel | At Step 3.24, after Complete: the focused result "Completed. 1 more waits for you.", then the portfolio review's row with "since … SAST", Open the next (named for the review) and Back to Activity inbox. |
| `/activities/{ActivityId:int}` | Completed | `states/activity-view--completed.png` | Dr Dlamini | At Step 3.6: "Done. David Naidoo completed it on …", "Rated 4. Credited 1 item to PAED-001.", Open My progress. |
| `/activities/{ActivityId:int}` | Decline note open | `states/activity-view--decline-note.png` | Dr Khumalo | At Step 3.11, Decline pressed: the note panel "Decline this request", the focus in "Note for Sipho Ndlovu", marked required, Decline with this note and Keep the request. |
| `/activities/{ActivityId:int}` | Decline without a note | `states/activity-view--decline-refused.png` | Dr Khumalo | At Step 3.11: the panel stays open with the note as typed, its summary "Not declined. It is still Requested." and "Note for Sipho Ndlovu: Decline requires a note." |
| `/activities/{ActivityId:int}` | Declined, the assessor's view | `states/activity-view--declined-by-assessor.png` | Dr Khumalo | At Step 3.11, after Decline with this note: "Declined. Nothing else waits for you.", Go to Home, the status card "Closed. You declined it on …". |
| `/activities/{ActivityId:int}` | Declined | `states/activity-view--declined.png` | Dr Ndlovu | At Step 3.12, the declined request: "Closed. Fatima Khumalo declined it on …", her reason quoted, and File it again, to someone else. |
| `/activities/{ActivityId:int}` | Awaiting discussion | `states/activity-view--awaiting-discussion.png` | Dr Botha | At Step 3.15, before returning: "Your move. …", Record discussion, Discard changes (greyed, "Nothing to discard yet.") and Return. |
| `/activities/{ActivityId:int}` | Return note open | `states/activity-view--return-note.png` | Dr Botha | At Step 3.15, Return pressed: the note panel "Return this reflection", "Note for Sipho Ndlovu", marked required, Return with this note (primary) and Keep the reflection. |
| `/activities/{ActivityId:int}` | Returned, the assessor's view | `states/activity-view--returned-by-assessor.png` | Dr Botha | At Step 3.15, after Return with this note: "Returned to Sipho Ndlovu. Nothing else waits for you.", Go to Home, the status card "With Sipho Ndlovu. Sarah Botha returned it on …". |
| `/activities/{ActivityId:int}` | Returned to the trainee | `states/activity-view--returned.png` | Dr Ndlovu | At Step 3.16, before submitting again: "With you. Sarah Botha returned it on …", her note quoted, his fields open. |
| `/activities/{ActivityId:int}` | Discussed | `states/activity-view--discussed.png` | Dr Ndlovu | After Step 3.17: "Done. Sarah Botha recorded the discussion on …", "A reflective exercise credits nothing. Nothing more happens to it." |
| `/activities/{ActivityId:int}` | Logged | `states/activity-view--logged.png` | Dr du Plessis | At Step 3.18: "Logged.", "Done. Logged on …". |
| `/activities/{ActivityId:int}` | Cancel draft dialog | `states/activity-view--cancel-draft-dialog.png` | Dr du Plessis | At Step 3.20, Cancel this draft… pressed: "Cancel this draft?", "A cancelled draft cannot be reopened, and it credits nothing.", Keep the draft and Cancel draft. |
| `/activities/{ActivityId:int}` | Cancelled | `states/activity-view--cancelled.png` | Dr du Plessis | At Step 3.20: "Cancelled.", "Closed. You cancelled it on …", "It was never submitted, and it credits nothing." |
| `/activities/{ActivityId:int}` | Awaiting review | `states/activity-view--awaiting-review.png` | Dr du Plessis | At Step 3.22: "With Mohammed Patel since …", Review locked, About's Credit "None: a portfolio and logbook review credits nothing". |
| `/activities/{ActivityId:int}` | Completed, EPA paused | `states/activity-view--credited-nothing.png` | Dr Patel | At Step 6.18, after Complete: "Its credit to PAED-012 waits while the EPA is paused.", and About's Credit None with "This activity's EPA is paused: its credit waits." |
| `/activities/{ActivityId:int}` | Credited nothing, after the last day | `states/activity-view--after-programme-end.png` | Dr du Plessis | After Step 5.27, his `D−1` Mini-CEX: "It counted towards no curriculum requirement.", and About's Credit None with the warning naming its causes. |
| `/activities/{ActivityId:int}` | Loading | `states/activity-view--loading.png` | Dr Dlamini | Hold a read, open an activity from My activities: "Loading the activity", a skeleton status card and details. No change. |
| `/activities/{ActivityId:int}` | Load error | `states/activity-view--load-error.png` | Dr Dlamini | The same, held 35 s: "Could not load this activity. Nothing has changed. Try again, or come back in a few minutes." with Try again. No change. |
| `/activities/{ActivityId:int}` | Opened from the other-role line | `states/activity-view--from-the-line.png` | Dr Zulu | At Step 3.33, before the switch, press Open it on Home's line: the Mini-CEX's page, trail Home › the activity with nothing lit in the menu, "Your move. Nomsa Mahlangu asked you on …". Go back to Home without a move. No change. |
| `/activities/{ActivityId:int}` | Narrow | `states/activity-view--narrow.png` | Dr Patel | At Step A.7.2: the history folded into "All N moves". |
| `/activities/{ActivityId:int}` | Narrow, to rate | `states/activity-view--rated-narrow.png` | Dr Patel | At Step A.7.2, at 390 px, 3b chosen: the Request folded, "Request", "Filled in by Anele Dlamini, `D`" and Show; Entrustment first, six radios abreast; the three feedback fields; the bar stacked at 44 px; About under the bar. |
| `/activities/{ActivityId:int}` | Narrow, the request unfolded | `states/activity-view--request-open-narrow.png` | Dr Patel | The same, Show pressed: the Request's fields read out, the summary reading Hide. No change. |
| `/activities/{ActivityId:int}` | Narrow, one more waiting | `states/activity-view--completed-next-narrow.png` | Dr Patel | At Step A.7.2, after Complete: "Completed. 1 more waits for you.", the review's row, Open the next and Back to Activity inbox stacked; the Request is shown open, not folded, and the history folds into "All 3 moves" as everywhere. |
| `/activities/mine` | Nothing filed | `states/my-activities--empty.png` | Dr Mahlangu | At Step 2.19: "No activities yet", "Log an activity to ask an assessor to rate an encounter, or to log a teaching session.", with Log an activity. |
| `/activities/mine` | A draft | `states/my-activities--draft.png` | Dr Dlamini | At Step 3.2: Needs you, badged 1, above All activities (1). |
| `/activities/mine` | Work returned | `states/my-activities--returned.png` | Dr Ndlovu | At Step 3.16, before submitting, open My activities: Needs you lists the reflection, "Returned to you by Sarah Botha on `D`. Change it and submit again." No change. |
| `/activities/mine` | Logged and cancelled | `states/my-activities--mixed.png` | Dr du Plessis | At Step 3.20: no Needs you; Who has it now "Closed" and "Done". |
| `/activities/mine` | With MSF records | `states/my-activities--msf.png` | Dr Molefe | At Step 3.48. |
| `/activities/mine` | Credited None after leaving | `states/my-activities--credited-none.png` | Dr du Plessis | At Step 5.28. |
| `/activities/mine` | A paused EPA | `states/my-activities--paused-epa.png` | Dr Dlamini | At Step 6.19. |
| `/activities/mine` | Loading | `states/my-activities--loading.png` | Dr Dlamini | Hold a read, then My activities in the menu. No change. |
| `/activities/mine` | Load error | `states/my-activities--load-error.png` | Dr Dlamini | The same, held 35 s: "Could not load your activities. Nothing has changed. Try again, or come back in a few minutes." with Try again. No change. |
| `/activities/mine` | Narrow | `states/my-activities--narrow.png` | Dr Dlamini | At Step A.7.3: the rows stacked. |
| `/activities/inbox` | Inbox clear | `states/activity-inbox--empty.png` | Dr Patel | At Step 3.4: the subtitle "What waits for you to rate, review or discuss.", Waiting for you "Inbox clear" / "Nothing is waiting for you.", Decided by you "No decisions yet." |
| `/activities/inbox` | Clear, with a decision | `states/activity-inbox--clear-with-decisions.png` | Dr Khumalo | At Step 3.11, after the decline: "Inbox clear"; Decided by you badged "1 decision", the rule line, the request Declined, decided "… SAST", Credit "—". |
| `/activities/inbox` | An assessor's requests | `states/activity-inbox--assessor.png` | Dr Patel | At Step 3.24: Waiting for you badged "2 waiting", the rule line, two rows oldest first, each its link with "from <registrar>", EPA, State and Waiting ("Less than a day" over "since … SAST"). |
| `/activities/inbox` | One overdue | `states/activity-inbox--overdue.png` | Dr Patel | At Step A.6.8, open Activity inbox: "1 waiting, 1 overdue", the review Awaiting review with Overdue beside it, "8 days" over "since `D−8` … SAST"; Decided by you under it. No change. |
| `/activities/inbox` | Decided by you, a page at a time | `states/activity-inbox--decided-paged.png` | Dr Patel | Scratch (post-act3): make Dr Patel the last mover of every activity, `UPDATE "ActivityTransitions" SET "ActorUserId" = (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'patel@kgk.wombat.local') WHERE "Id" IN (SELECT DISTINCT ON ("ActivityId") "Id" FROM "ActivityTransitions" ORDER BY "ActivityId", "OccurredOn" DESC, "Id" DESC);`, so every finished one is his decision. On his inbox choose Per page 10: "Showing 1–10 of N" with Previous aria-disabled, the focus still on the select. Press Next: the next page, the focus on the "Decided by you" heading, and on the last page Next aria-disabled. |
| `/activities/inbox` | A registrar's inbox | `states/activity-inbox--trainee.png` | Dr Ndlovu | At Step 3.16, its last move: under the subtitle "What waits for you to rate, review or discuss.", "Nothing here is yours to act on.", pointing to Needs you, with Open My activities. |
| `/activities/inbox` | A paused EPA | `states/activity-inbox--paused-epa.png` | Dr Patel | At Step 6.18, before opening it: the review first, Overdue, then the Mini-CEX, its EPA "(no longer in use)". |
| `/activities/inbox` | Loading | `states/activity-inbox--loading.png` | Dr Patel | Hold a read, then Activity inbox in the menu: both sections' headings over skeletons, "Loading the Activity inbox." to a screen reader. No change. |
| `/activities/inbox` | Load error | `states/activity-inbox--load-error.png` | Dr Patel | The same, held 35 s: "Could not load the Activity inbox. Nothing has changed. Try again, or come back in a few minutes." with Try again. No change. |
| `/activities/inbox` | Narrow | `states/activity-inbox--narrow.png` | Dr Patel | At Step A.7.2: "2 waiting, 1 overdue", the rows stacked, nothing scrolling sideways. |

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
| `/portfolio/progress` | New version after the move | `states/my-progress--after-move.png` | Dr Ndlovu | At Step 6.37: PAED-002 already counts on 11.2, before any rebuild (T304). |
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
| `/admin/trainees/edit` | Completed | `states/trainee-profile-edit--completed.png` | Prof Mbatha | At Step 5.17: the details read-only, no Save profile (T305). |
| `/admin/trainees/edit` | Deactivate dialog | `states/trainee-profile-edit--deactivate-dialog.png` | Prof Mbatha | At Step 5.27. |
| `/admin/trainees/edit` | Deactivated | `states/trainee-profile-edit--deactivated.png` | Prof Mbatha | At Step 5.27: the details read-only, no Save profile (T305). |
| `/admin/trainees/edit` | Moved to a new version | `states/trainee-profile-edit--moved.png` | Prof Mbatha | At Step 6.36: "Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it." (T304) |
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

- **Institutional sign-in.** The sign-in page's "or" divider and "Sign in with …" buttons (and the notices beside them,
  which take the focus themselves), the link page's password form and its refusals, and the SSO mappings page's Add
  mapping form, mapping rows and Delete. Each needs a configured identity provider (`Sso:Providers` is empty on dev;
  `coverage.md` § Flows and states not played). The refusal texts alone are reachable as typed codes on the sign-in
  page.
- **An account with an institutional sign-in** (T339, flow 02): My account's How you sign in listing the institution
  with Remove, the Remove dialog (with a password field when the account has a password, without one when it has
  none), the dialog reopened with "Incorrect password." or the throttle's words, the lock sign-out a fifth wrong
  password in the dialog causes, and "Remove: this is the only way you sign in." on an account whose one institutional
  sign-in is its only way in. And the institution-only account's pages: My account's "You sign in through your
  institution. This account has no Wombat password, so there is no password to change here." and Change password's
  "This account signs in through your institution, so it has no password to change here." with Back to My account, in
  place of the form. Every cast account has a password and no linked sign-in; linking needs a provider. The card's
  results alone are reachable as typed codes on My account (§ Typed codes).
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
