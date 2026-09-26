# Act 2 — Day 1: team onboarding

**Scenario date:** Tuesday 13 January 2026, the morning after Act 1. The date is narrative only (README § The clock).

**Who acts:** Prof Nolwazi Mbatha (InstitutionalAdmin, KGK) issues every invitation, gives the second roles and the
assessor profiles, admits the registrars and forms the review panel. Each invitee plays in a browser of their own (a
private window per person). Dr Mokoena and Dr Sithole handle the panel once it exists. No Administrator is needed.

**Why:** Act 1 set up the catalogue KGK trains against. Nothing can be filed, rated or decided until the people are in:
consultants with the roles they work in, registrars admitted to the adopted curriculum, and a panel to review them.

**Starting state** (Act 1's outcome, or `recovery/scenario-post-act1.dump`):
- The seeded CPSA catalogue: the Paediatrics speciality and sub-speciality, the six-rung `CPSA Paediatric Entrustment
  Scale v11.1`, PAED-001 to PAED-015, and `Paediatric EPA Curriculum` 11.1 with its 15 items.
- Kgosi Kgari Teaching Hospital (KGK) has adopted curriculum 11.1. It publishes `KGK Teaching Session Log`
  (`kgk_teaching_log`) beside the seeded `*_cpsa` instruments.
- KGK's only user is Prof Mbatha (`mbatha@kgk.wombat.local`). Dr Kruger is the CPSA's CollegeAdmin. The dev accounts
  belong to the Demo Institution and `devadmin@wombat.local` to none, so none of them appears in a KGK picker below.
- No KGK invitation is active. KGK has no assessor profiles, trainee profiles or decision panels.

**Goal:**
1. Nine staff onboarded with the cast's roles: Smit (Coordinator); Zulu, Naidoo and Botha (CommitteeMember, then
   Assessor too); Patel and Khumalo (Assessor); van Rensburg (external CommitteeMember); Mokoena (SpecialityAdmin,
   Paediatrics); Sithole (SubSpecialityAdmin, Paediatrics).
2. Five assessor profiles with a training status (T065). Van Rensburg has none.
3. Five registrars registered and admitted to curriculum 11.1, pinned to KGK's adoption (T091), in training years 4, 3,
   2, 1 and 1.
4. One decision panel, `Paed Annual Review Panel`: Zulu chairs, Naidoo and Botha are members and van Rensburg is the
   external member. It is a general panel (T131).
5. Every role has signed in once and seen its dashboard, nav and account before any activity exists.

**Dates in this act.** `D` is the replay day. `J` is the latest 15 January on or before `D` (2026-01-15 for a replay in
2026), the day the story's January intakes start. `J−3y` is 15 January three years before `J`. `J+4y−1d` is the day
before `J`'s fourth anniversary. Written from `J`, each registrar's training year comes out as the story's (4, 3, 2, 1,
1) whenever the act is replayed. `TraineeProfile.GetStage` counts whole 365-day blocks, so a leap day can put a
registrar a year ahead in the last day or two before the next 15 January.

**Mail.** With `Email__SmtpHost` unset (README § Mail), each email's text goes to the application log as a stub email.
The stub is tagged by kind (`invitation, role:Coordinator`) and does not name the address (T282). The invitations page
shows each link once, when it is issued. Copy it then, and use the log to confirm that the email was written. Nothing
reports a stub email as delivered. So an invitation's Delivery column reads "Being sent" for its first hour and "Not
delivered." after that (T283, `LoggingEmailSender`). Against a mail server that accepts the email, it reads "Sent".

## Phase 2.A — Staff invitations

### Step 2.1 — Prof Mbatha opens the invitations page
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /admin/invitations
Do: Sign in and open Invitations from the nav. Read what the issue form offers before issuing anything.
Expect: Active invitations is empty: "No active invitations". Kruger's CollegeAdmin invitation is the College's, not
  KGK's, and Mbatha's own has been used. Role offers InstitutionalAdmin (the default), SpecialityAdmin,
  SubSpecialityAdmin, Coordinator, CommitteeMember, Assessor and Trainee, but not CollegeAdmin, which only an
  Administrator issues (T093). Institution offers KGK only. Speciality is disabled for the default role.
Actual (2026-09-26, T295 replay, wombat_scenario): Active invitations reads "No active invitations" / "Issue an
  invitation to create the first onboarding link.". Role offers the seven roles named, InstitutionalAdmin selected, no
  CollegeAdmin. Institution offers KGK alone (behind "Select institution"). Speciality and Sub-speciality are disabled.
Gap: none

### Step 2.2 — Mbatha invites Mr Smit as Coordinator
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation to `smit@kgk.wombat.local` as Coordinator at KGK, with speciality and sub-speciality left blank.
  Copy the link.
Expect: For a Coordinator, Speciality is optional and Sub-speciality stays disabled (T060). The page reads "Invitation
  issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once." (T283's
  wording). Below it the link says it expires in 14 days. Active invitations gains a row: Coordinator, KGK, no
  speciality, expiring `D+14`, Delivery "Being sent". The form goes back to its defaults, and the log holds a stub
  "Your Wombat invitation" tagged `role:Coordinator`.
Actual (2026-09-26, T295 replay, wombat_scenario): Coordinator enabled Speciality, Sub-speciality stayed disabled. The
  T283 wording and "It expires in 14 days" shown; row Coordinator, KGK, scopes blank, expires 2026-10-10, "Being sent";
  form reset. The replay's Development config sends to an SMTP sink: "Your Wombat invitation" to smit@ in the sink, and
  the log reads "Email … (tags: invitation, role:Coordinator) sent on attempt 1.".
Gap: none

### Step 2.3 — Mbatha invites Dr Zulu, Dr Naidoo and Dr Botha as committee members
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue one invitation each to `zulu@`, `naidoo@` and `botha@kgk.wombat.local` as CommitteeMember at KGK, with
  speciality Paediatrics and no sub-speciality. Copy each link before issuing the next.
Expect: Speciality offers Paediatrics alone: KGK's adopted specialities (T092). Sub-speciality stays disabled, because a
  committee member may not be scoped to one. Three rows read CommitteeMember, KGK, Paediatrics, with the sub-speciality
  blank.
Note: An invitation carries one role. Assessor is added on each user's page in Step 2.13 (T061).
Actual (2026-09-26, T295 replay, wombat_scenario): Speciality offered Paediatrics alone; Sub-speciality stayed disabled
  for CommitteeMember. Three rows read CommitteeMember, KGK, Paediatrics, sub-speciality blank, each issued with its own
  link. Earlier rows turned to "Sent" on re-render (the SMTP sink accepted them).
Gap: none

### Step 2.4 — Mbatha invites Dr Patel and Dr Khumalo as assessors
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation each to `patel@` and `khumalo@kgk.wombat.local` as Assessor at KGK, with speciality Paediatrics
  and sub-speciality Paediatrics.
Expect: For an Assessor the Sub-speciality select is enabled. Once Paediatrics is chosen as the speciality, it offers
  that speciality's sub-speciality, Paediatrics. Two rows read Assessor, KGK, Paediatrics, Paediatrics.
Actual (2026-09-26, T295 replay, wombat_scenario): For Assessor, Sub-speciality was enabled and empty until Paediatrics
  was chosen as the speciality, then offered Paediatrics. Two rows read Assessor, KGK, Paediatrics, Paediatrics.
Gap: none

### Step 2.5 — Mbatha invites Dr van Rensburg as an external committee member
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation to `vanrensburg@sun.wombat.local` as CommitteeMember at KGK, with speciality and sub-speciality
  blank.
Expect: It is issued with no speciality (T060). The row reads CommitteeMember, KGK, with both scope columns blank. The
  address's domain does not matter: the invitation scopes him to KGK.
Note: A panel seats only committee members at its own institution (T165), so the Stellenbosch examiner is invited at KGK.
Actual (2026-09-26, T295 replay, wombat_scenario): Issued with no speciality. The row reads CommitteeMember, KGK, both
  scope columns blank, expires 2026-10-10 (already "Sent" when the row rendered).
Gap: none

### Step 2.6 — Mbatha invites Dr Mokoena as the programme's SpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue `mokoena@kgk.wombat.local` as SpecialityAdmin at KGK with no speciality. Then issue it again with speciality
  Paediatrics.
Expect: The first is refused: "Speciality administrators must be scoped to a speciality." (T060). Nothing is issued and
  no link is shown. The second is issued. Sub-speciality stays disabled for this role, and the row reads SpecialityAdmin,
  KGK, Paediatrics.
Note: The code keeps the last issued link's alert (van Rensburg's, from Step 2.5) on screen under a refused issue, and
  under a revoke (Step 2.17): reported as a suspected defect. Copy nothing from it here.
Actual (2026-09-26, T295 replay, wombat_scenario): The first was refused with "Speciality administrators must be scoped
  to a speciality." and no row was added, but van Rensburg's link and its "Share this registration link…" box stayed on
  screen under the refusal. The second was issued: SpecialityAdmin, KGK, Paediatrics; Sub-speciality stayed disabled.
Gap: [F-2.6a, T264] A refused issue leaves the previous invitee's registration link (van Rensburg's) on screen under the
  refusal, inviting the admin to share the wrong person's link (InvitationsList.IssueAsync never clears it).

### Step 2.7 — Mbatha invites Dr Sithole as the SubSpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue `sithole@kgk.wombat.local` as SubSpecialityAdmin at KGK with speciality Paediatrics and no sub-speciality.
  Then issue it again with sub-speciality Paediatrics.
Expect: The first is refused: "The selected role requires speciality and sub-speciality scope." The second is issued.
  Active invitations now holds nine rows: Smit, Zulu, Naidoo, Botha, Patel, Khumalo, van Rensburg, Mokoena and Sithole,
  each with the role and scope above.
Actual (2026-09-26, T295 replay, wombat_scenario): The first was refused with "The selected role requires speciality and
  sub-speciality scope." (Mokoena's link stayed on screen under it, as F-2.6a). The second was issued. Nine rows, each
  role and scope as above, all expiring 2026-10-10; Sithole's "Being sent", the other eight "Sent".
Gap: none (the stale link under the refusal is F-2.6a)

## Phase 2.B — The staff register

### Step 2.8 — Mr Smit registers from his link
Role: Anonymous — Mr Pieter Smit, holding his invitation link
Route: /account/register → / → /account/logout → /account/login
Do: Open the link and enter first name Pieter, last name Smit and a password that meets the rules. Confirm it and
  register. Read the landing page, then sign out.
Expect: The page reads "Registering smit@kgk.wombat.local as Coordinator.", with the email filled in and not editable.
  The token is cleared from the address bar once the page loads. Registering signs him in and lands on Home: "Welcome,
  smit@kgk.wombat.local", "Viewing as Coordinator". Signing out returns him to the sign-in page.
Note: Record every password chosen in this act in `pwd_DO_NOT_COMMIT.txt` only (README § Passwords).
Actual (2026-09-26, T295 replay, wombat_scenario): "Registering smit@kgk.wombat.local as Coordinator.", the email
  filled and disabled. The address bar still held `?token=…` after load and until Register. Registering landed on Home:
  "Welcome, smit@kgk.wombat.local", "Viewing as Coordinator". Sign-out (the nav's Logout) returned to Sign in.
Gap: [F-2.8a, T315] The invitation token is never cleared from the address bar: Register.razor clears it in
  OnAfterRenderAsync by JS interop, which never runs on the static page an anonymous visitor gets (T181, DESIGN.md
  § Account / auth page). [F-2.8b, T317] Typing /account/logout while signed in shows the browser's bare "HTTP ERROR 405"
  (the endpoint is POST only); it could send a GET to /account/logout-confirm.

### Step 2.9 — Dr Patel's first two attempts are refused
Role: Anonymous — Dr Mohammed Patel, holding his invitation link
Route: /account/register → / → /account/logout
Do: Register with a 10-character password that mixes upper case, lower case, a digit and a symbol. Then use a
  12-character password with a different confirmation. Then use a 12-character password confirmed correctly. Sign out.
Expect: The first attempt returns to the form with "Passwords must be at least 12 characters." The first field takes the
  focus and no account is created (T285). The second returns with "The password confirmation does not match." The
  third registers him and lands on "Viewing as Assessor".
Actual (2026-09-26, T295 replay, wombat_scenario): 10 characters: back with `&error=PasswordTooShort`, "Passwords must be
  at least 12 characters.", First name focused, no account (SQL count 0). Mismatch: "The password confirmation does not
  match.". Each refusal also emptied First and Last name. The third landed on "Viewing as Assessor"; signed out.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc297b2, the end-of-Act-2 snapshot): his Home, "Viewing as
  Assessor", reads Pending requests 0 "assessments awaiting review" with "Review inbox →", "Awaiting your review" with
  "Nothing is awaiting your review." (in place of "No accepted assessments pending action."), "No decisions yet." and
  "Open my inbox →".
Gap: [F-2.9a, T317] A refused registration drops the first and last name the registrar typed, so each retry retypes them
  (unsure: the refusal travels as a code on a static page by design, T285, but nothing there requires losing the names).

### Step 2.10 — The other seven staff register
Role: Anonymous — Dr Zulu, Dr Naidoo, Dr Botha, Dr Khumalo, Dr van Rensburg, Dr Mokoena and Dr Sithole
Route: /account/register → / → /account/logout
Do: Each registers from their own link with the cast's name and signs out. Dr Khumalo types her first name as "Fatma"
  by mistake (she corrects it in Step 2.41).
Expect: Each page names the address and the invited role. Each person lands on Home viewing as that role. Zulu, Naidoo,
  Botha and van Rensburg view as CommitteeMember, Khumalo as Assessor, Mokoena as SpecialityAdmin and Sithole as
  SubSpecialityAdmin.
Actual (2026-09-26, T295 replay, wombat_scenario): Each page read "Registering <address> as <role>." and each landed on
  "Welcome, <address>": Zulu, Naidoo, Botha and van Rensburg "Viewing as CommitteeMember", Khumalo (typed "Fatma")
  "Viewing as Assessor", Mokoena "Viewing as SpecialityAdmin", Sithole "Viewing as SubSpecialityAdmin". All signed out.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc297b2, the end-of-Act-2 snapshot, so after the registrars'
  admission): Dr Khumalo's Home reads as Dr Patel's does in Step 2.9. Dr Mokoena's and Dr Sithole's read Pending
  reviews 0 "activities awaiting review", with no link (in place of "activities in review" and "Review queue →"),
  beside Trainees in programme 5 "active / 0 inactive" (none was admitted yet at this step's own moment).
Gap: none

### Step 2.11 — A used link cannot be used again
Role: Anonymous — Mr Pieter Smit
Route: /account/register
Do: Open his invitation link a second time.
Expect: No form is shown, only "This invitation has already been used." (T285's words).
Actual (2026-09-26, T295 replay, wombat_scenario): "Complete registration" with the alert "This invitation has already
  been used." and no form.
Gap: none

## Phase 2.C — Second roles and assessor profiles

### Step 2.12 — Mbatha reviews KGK's users
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/users → /admin/users/{UserId}
Do: Open Invitations. Then read the user list, filter it by "zulu" and clear the filter. Open Manage on her own
  row.
Expect: Active invitations reads "No active invitations": all nine staff invitations have been used. The user list has
  ten rows: Mbatha and the nine staff, each at KGK with the invited role and status Active. No Demo account,
  `devadmin` or Dr Kruger appears: an InstitutionalAdmin lists only her institution's users (T056). The filter leaves
  Zulu alone. Her own page's subtitle reads "Your own account's summary, roles and pending invitations." A note says
  "This is your own account, so you cannot change its roles, lockout or password here. Another administrator can change
  your roles or lockout.", followed by a Change your password link. It offers no Remove, Add role, Reset password or
  Lockout (T278).
Actual (2026-09-26, T295 replay, wombat_scenario): "No active invitations". Ten rows (Mbatha and the nine), all KGK,
  Active, each with its invited role; no Demo, devadmin or Kruger row. "zulu" left Zulu alone ("zzz": "No users match
  the current filter."); cleared, ten again. Her page: the subtitle and note as quoted, "Change your password" link;
  Roles lists InstitutionalAdmin with no Remove, and no Add role, Reset password or Lockout.
Gap: none

### Step 2.13 — Mbatha adds Assessor to Dr Zulu, Dr Naidoo and Dr Botha
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: For each of the three, open Manage, choose Assessor under Add role and press Add role.
Expect: Before the change, Roles lists CommitteeMember with Remove. Add role offers InstitutionalAdmin, SpecialityAdmin,
  SubSpecialityAdmin, Coordinator and Assessor, never Administrator, CollegeAdmin, PendingTrainee or Trainee (T303).
  Its help reads "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit to
  curriculum'.", on every user's page.
  A Lockout card lists what a lock does (T284), and Pending invitations reads "No active invitations are outstanding for
  this email.". After the change the page reads "Role 'Assessor' added.", Roles lists both, and Add role no longer offers
  Assessor. The users list's Roles column shows both.
Note: A role change ends any session the account still has open within a minute (T279). Step 2.10 signed everyone out.
Actual (2026-09-26, T295 replay, wombat_scenario): Before: Roles CommitteeMember with "Remove the CommitteeMember role";
  Add role offers the six named (no Administrator, CollegeAdmin or PendingTrainee); Lockout card lists four effects;
  "No active invitations are outstanding for this email.". After: "Role 'Assessor' added.", both roles listed, Assessor
  gone from Add role. Same for Naidoo and Botha; the list's Roles reads "CommitteeMember, Assessor" for all three.
  Re-checked after T303 (2026-09-26, wombat_scenario_rc303a, Act 2 replayed from scenario-post-act1): before the change
  each of the three pages' Add role offers exactly InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator
  and Assessor, with no Trainee, and under the select the help (the select's aria-describedby) reads "Trainee is not
  offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit to curriculum'."; CommitteeMember
  keeps its Remove. After: "Role 'Assessor' added." takes the focus and Add role offers the other four
  (states/user-detail--other.png, --role-added.png, act-2/2.13-1 and 2.13-2 re-taken). The Expect does not yet name
  the help line.
Gap: The Trainee offered before the fix (F-2.28a) is fixed by T303 (4824d62). [F-2.13a, T290] The app log records
  three "fail: … Unhandled exception in circuit" (TaskCanceledException, "Navigation failed when changing the location
  to /account/session-ended") here, one per account changed a minute or two after it
  signed out: the session end navigates a disconnected circuit (once more at Step 2.29, inferred du Plessis).
  Unsure: likely harmless, but logged as a failure it hides real ones.

### Step 2.14 — Mbatha creates five assessor profiles
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors → /admin/assessors/edit → /admin/assessors
Do: Create a profile for each assessor at KGK, Paediatrics / Paediatrics, with qualifications "MBChB, MMed (Paed), FC
  Paed (SA)". Zulu is Trained, completed `D−8 years`. Naidoo is Trained, `D−6 years`. Botha is Trained with the date
  left blank. Patel is In training. Khumalo is Provisional (awaiting sign-off), `D−1 month`.
Expect: The list first reads "No assessor profiles". The form's Assessor user offers the five KGK Assessors, and not van
  Rensburg or any Demo account. Institution offers KGK alone. Sub-speciality is enabled once a speciality is chosen.
  Training status offers Not started, In training, Provisional (awaiting sign-off) and Trained, and the completion date
  is asked for only when the status is Provisional or Trained. Each save reads "Assessor profile saved.", and the page
  moves to `?id=` for the saved profile. The next new form's picker leaves out everyone already profiled.
Note: T064 decided both the `?id=` move and the narrowing; the code has neither (T064 is closed, and the T159 replay found
  both missing), so expect a Gap. Saving a profile sets the user's scope to the profile's (T279: open sessions end).
Actual (2026-09-26, T295 replay, wombat_scenario): List first "No assessor profiles" / "Create the first assessor profile
  to make assessors selectable in the workflow.". Picker: the five KGK Assessors only; Institution KGK alone;
  Sub-speciality enabled once a speciality chosen; the date shown only for Provisional or Trained. Each save read
  "Assessor profile saved." but stayed at /admin/assessors/edit, and every new form still offered all five. SQL: five
  profiles as specified.
Gap: [F-2.14a, T289] No move to `?id=` after a save (T064). [F-2.14b, T289] The new form's picker is not narrowed to unprofiled
  assessors (T064). [F-2.14c, T326] The empty list says a profile makes assessors "selectable in the workflow"; the
  nominee picker needs only the role (NomineeDirectory reads no profile). [F-2.14d, T324] The empty save's summary reads
  "The UserId field is required." (a property name, not "Assessor user").

### Step 2.15 — The assessor list and who is not on it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors
Do: Read the list.
Expect: Five rows, each at KGK in Paediatrics / Paediatrics. Training status reads Trained for Zulu, Naidoo and Botha, In
  training for Patel and Provisional for Khumalo. Training completed reads `D−8 years`, `D−6 years`, "Not recorded",
  "Not recorded" and `D−1 month` respectively. Each row has an Edit named for its assessor. Van Rensburg is neither listed
  nor offered, because he does not hold the Assessor role.
Actual (2026-09-26, T295 replay, wombat_scenario): Five rows, KGK, Paediatrics / Paediatrics: Zulu Trained 2018-09-26,
  Naidoo Trained 2020-09-26, Botha Trained "Not recorded", Patel In training "Not recorded", Khumalo Provisional
  2026-08-26. Each Edit is named "Edit <name>". Van Rensburg is not listed and was never offered.
Gap: none

## Phase 2.D — Registrar invitations

### Step 2.16 — Mbatha invites the five registrars, one to a mistyped address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Invite `molefe@`, `dlamini@`, `duplessis@` and `mahlangu@kgk.wombat.local` as Trainee at KGK, Paediatrics /
  Paediatrics. Copy each link. For Dr Ndlovu she types `ndlvou@kgk.wombat.local`.
Expect: A Trainee invitation needs both speciality and sub-speciality, and the Sub-speciality select is enabled for it.
  Five rows read Trainee, KGK, Paediatrics, Paediatrics, and each Delivery reads "Being sent".
Actual (2026-09-26, T295 replay, wombat_scenario): For Trainee the Sub-speciality select was enabled; both scopes were
  chosen. Five rows read Trainee, KGK, Paediatrics, Paediatrics, expiring 2026-10-10, issued 11:46–11:47. Each read
  "Being sent" when issued and "Sent" on the next render: the replay's SMTP sink accepted every mail.
Gap: none (the "Sent" is the environment, as Step 2.26's note says)

### Step 2.17 — Mbatha revokes the mistyped invitation and issues the right one
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /account/register
Do: Revoke the `ndlvou@` row and issue a Trainee invitation to `ndlovu@kgk.wombat.local` with the same scope. Then open
  the revoked link, as anyone holding it could.
Expect: The row's button is named "Revoke the Trainee invitation to ndlvou@kgk.wombat.local" (T239). Pressing it shows
  "Invitation revoked." and the row is gone, with no link shown (see Step 2.6's note). An address is never corrected in
  place: the invitation names who may register (T283). A new row reads `ndlovu@`. The revoked link shows no form, only
  "This invitation has been revoked."
Actual (2026-09-26, T295 replay, wombat_scenario): The button is named "Revoke the Trainee invitation to
  ndlvou@kgk.wombat.local"; pressing it (no confirmation) showed "Invitation revoked." and the row went. No link was on
  screen, but the page had been reloaded since the issue, so Step 2.6's suspicion was not tested here. The ndlovu@ row
  was added. The revoked link, in a separate browser context, read "This invitation has been revoked." with no form.
  Replayed in the T303 re-check (2026-09-26, wombat_scenario_rc303a), with no reload between the issue and the revoke:
  under "Invitation revoked." the "Share this registration link…" box still showed the revoked ndlvou@ link.
Gap: [F-2.6a, T264] The revoke half of Step 2.6's suspicion holds: a revoke leaves the revoked invitation's link on
  screen (InvitationsList.RevokeAsync never clears it). The link itself is dead ("This invitation has been revoked.").

## Phase 2.E — The registrars register and wait

### Step 2.18 — Four registrars register and are pending
Role: Anonymous — Dr Molefe, Dr Dlamini, Dr Mahlangu and Dr Ndlovu
Route: /account/register → / → /account/logout
Do: Each registers from their own link. Molefe, Dlamini and Ndlovu sign out. Dr Mahlangu stays signed in with Home open
  in her tab, which Step 2.31 uses. Dr du Plessis cannot register: he says no email reached him.
Expect: Each page reads "Registering <address> as Trainee." Each registrar lands on "Viewing as PendingTrainee", because
  a Trainee invitation registers as PendingTrainee until admission. Home shows one card, "Awaiting admission": "You are
  registered and waiting to be admitted to a curriculum by your programme administrator." It has a "Review your account
  →" link.
Actual (2026-09-26, T295 replay, wombat_scenario): Each page read "Registering <address> as Trainee."; each landed on
  "Viewing as PendingTrainee" with the one "Awaiting admission" card, its sentence as quoted and "Review your account →"
  (to /account/profile). Molefe, Dlamini and Ndlovu signed out; Mahlangu stays signed in, on Home, in her own browser
  context. Du Plessis did not register.
Gap: none

### Step 2.19 — What a registrar sees before admission
Role: PendingTrainee — Dr Nomsa Mahlangu
Route: / → /account/profile → /activities/mine → /activities/new → /portfolio/progress → /access-denied → /
Do: Read the nav. Follow Review your account, then open My Activities and Activities, and file nothing. Type the My
  Progress address.
Expect: The nav reads Home, My Account, Data Rights, Activities, My Activities and Logout. It has no MSF Reports, My
  Committee Reviews, My Progress or Export Portfolio, because those pages do not admit a pending trainee (T141). My
  account lists her role as PendingTrainee. My Activities reads "No activities yet". Activities opens the type picker
  with the eleven instruments of Step 2.42: her invitation's Paediatrics scope selects them, and with no curriculum yet
  no ladder narrows them. My Progress shows Access denied ("You do not have permission to view this page.") with Back
  to home.
Actual (2026-09-26, T295 replay, wombat_scenario): Nav: Home, My Account, Data Rights, Activities, My Activities,
  Logout. My account: Roles PendingTrainee, email disabled. My Activities: "No activities yet". Activities opened the
  picker with the eleven instruments of Step 2.42 (no MSF, Learner Feedback or Demo type). The typed My Progress address
  went to /access-denied?ReturnUrl=…: "You do not have permission to view this page.", Back to home. Tab left on Home.
Gap: none

## Phase 2.F — The review panel

While the registrars' links age (Step 2.26 needs an hour), Mbatha forms the panel.

### Step 2.20 — Decision panels before any exist
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Open Decision Panels.
Expect: The page reads "No decision panels" / "Create a panel before scheduling reviews." and offers New panel. "Who
  decides each EPA" names `Paediatric EPA Curriculum 11.1` and says against every EPA "No panel at this institution
  covers this programme."
Actual (2026-09-26, T295 replay, wombat_scenario): "No decision panels" / "Create a panel before scheduling reviews.",
  New panel offered. "Who decides each EPA" names Paediatric EPA Curriculum 11.1 in two rows (PAED-004, PAED-005; and
  the other thirteen), each "No panel at this institution covers this programme."
Gap: none

### Step 2.21 — The new-panel form, and a panel of one refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new
Do: Name it "Paed Annual Review Panel", with scope Speciality, speciality Paediatrics and Decides for General panel.
  Select Zulu and Naidoo under Members, then choose Zulu as Chair. Unselect Naidoo, leaving the chair alone, and save.
Expect: Scope offers Institution and Speciality and starts on Speciality. Speciality offers Paediatrics alone ("The panel
  runs at your institution. Only the specialities it has adopted a curriculum in are listed."). Decides for offers
  General panel and the Neonatal team Clinical Competency Committee. Chair, Members and External members each list
  exactly Botha, Naidoo, van Rensburg and Zulu: active committee members at KGK who are not trainees (T165, T237).
  Choosing Zulu as chair shows "Thandi Zulu is the chair now, so is no longer selected under Members." (T257). The save
  is refused: "A panel needs at least two members: the chair and at least one other, so that no decision is one
  person's." (D46).
Actual (2026-09-26, T295 replay, wombat_scenario): Scope offered Institution and Speciality, on Speciality; Speciality
  offered Paediatrics with the quoted help; Decides for offered the two. Chair, Members and External members each listed
  Botha, Naidoo, van Rensburg and Zulu. Choosing Zulu as chair showed the T257 sentence and dropped her from Members.
  With Naidoo unselected, Save was refused with the D46 sentence; the page stayed at /committee/panels/new.
Gap: none

### Step 2.22 — Mbatha saves the panel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new → /committee/panels/{PanelId:int}
Do: Select Naidoo under Members and van Rensburg under External members, then save. Botha is left for Step 2.23.
Expect: The page reloads as the panel's own page. It has a "Decides for" card reading General panel with Save
  committee, and Update members shows Zulu as chair, Naidoo as a member and van Rensburg as external. The External
  members help text says external members sit with the chair on the appeal body and that cross-institution externals
  are not yet supported.
Actual (2026-09-26, T295 replay, wombat_scenario): Saved to /committee/panels/1 (PanelId 1). "Decides for" card reads
  General panel with Save committee; Update members shows Zulu chair, Naidoo member, van Rensburg external, and the
  External help text as expected. The page's title and headings read only "Decision panel", "Decides for" and "Update
  members": nowhere does it name the panel, its scope or its speciality.
Gap: [F-2.22a, T260] A panel's own page never shows its name (nor scope and speciality), so the admin cannot tell which
  panel she is editing; there is also no way to rename it there.

### Step 2.23 — Dr Mokoena adds Dr Botha to the panel
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/panels → /committee/panels/{PanelId:int} → /committee/panels
Do: Sign in, open Decision Panels and Edit the panel. Select Botha under Members as well, then save and go back to the
  panels.
Expect: The list offers her New panel and an Edit on this panel, a Speciality-scoped panel in her speciality at her
  institution (T194). Decides for is read-only: "General panel", with "Only an institutional administrator can change
  which College committee a panel sits as." The save reads "Panel members updated.", and the list's Members column
  reads 4.
Actual (2026-09-26, T295 replay, wombat_scenario): The list offered her New panel and "Edit Paed Annual Review Panel"
  (Members 3). Decides for was read-only, "General panel", with the quoted sentence. Save read "Panel members updated."
  (members Botha and Naidoo, chair Zulu, external van Rensburg); back on the list, Members read 4.
Gap: none

### Step 2.24 — Who decides each EPA now
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Read the list and the routing card.
Expect: One row: Paed Annual Review Panel, Speciality, General panel, 4 members, with an Edit. PAED-004 and PAED-005 go to
  the panel with "No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the
  general panel decides." (T131). The other thirteen EPAs go to the panel.
Note: KGK has no neonatal CCC, so "Decides for" stays General panel. Act 4 schedules every review on this panel.
Actual (2026-09-26, T295 replay, wombat_scenario): One row: Paed Annual Review Panel, Speciality, General panel, 4,
  Edit. Routing: PAED-004, PAED-005 → Paed Annual Review Panel with the T131 sentence as quoted; the other thirteen →
  Paed Annual Review Panel.
Gap: none

### Step 2.25 — Dr Sithole looks at what he may create
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/panels → /committee/panels/new
Do: Sign in, open Decision Panels, press New panel and read the form. He leaves without saving.
Expect: The list offers him New panel and an Edit on the panel. The form's Scope offers Speciality only, because a
  sub-speciality administrator manages the Speciality-scoped panels of his sub-speciality's speciality (T194, T245).
  Speciality offers Paediatrics. There is no Decides for field. Nothing is created.
Actual (2026-09-26, T295 replay, wombat_scenario): The list offered him New panel and "Edit Paed Annual Review Panel".
  The form's Scope offered Speciality only, Speciality offered Paediatrics, and there was no Decides for field. He went
  Back to panels without saving; nothing was created.
Gap: none

## Phase 2.G — A lost email

### Step 2.26 — Mbatha resends Dr du Plessis's invitation
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: At least an hour after Step 2.16, open Invitations and press Resend on du Plessis's row. Copy the new link.
Expect: His is the only row left. Its Delivery reads "Not delivered. Resend emails a new link in place of the current
  one, which then stops working.", and it offers Resend before Revoke. Afterwards the page reads "A new invitation link
  is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy the new link below — it is
  shown only once.", with the link below it. The row reads "Being sent", offers no Resend and expires 14 days from now.
Note: This needs the log sender (see Mail). Against a mail server that accepted the first email, the row reads "Sent"
  and offers no Resend.
Actual (2026-09-26, T295 replay, wombat_scenario): The story's row read "Sent": that run's SMTP sink accepted his mail,
  so nothing was resent and he registered from Step 2.16's link. Replayed with the log sender on a scratch copy of the
  final state (wombat_scenario_states, :5181), for a stand-in invitee issued by Mbatha and aged 61 minutes: the row read
  "Not delivered. Resend emails a new link in place of the current one, which then stops working." with Resend before
  Revoke. Resend gave the expected message word for word, a new link, and a row reading "Being sent", with no Resend,
  expiring 2026-10-10 (act-2/2.26-3-scratch-not-delivered.png, 2.26-4-scratch-resent.png).
  Played in the story in the T303 re-check (2026-09-26, wombat_scenario_rc303a, log sender), with his invitation's
  IssuedOn set back 61 minutes by SQL in place of the wait: his row alone read the "Not delivered." sentence with Resend
  before Revoke; Resend gave the expected message and a row reading "Being sent", with no Resend. His first link then
  read "This invitation is invalid." and he registered from the new one (Step 2.27). No capture was taken.
Gap: runbook corrected: README § Mail. The Development settings send mail to localhost:25, so only the log sender
  (which `tools/scenario-replay.ps1 start` now forces) ever reaches "Not delivered"; played on the scratch copy.

### Step 2.27 — Dr du Plessis registers from the new link
Role: Anonymous — Dr Pieter du Plessis
Route: /account/register → / → /account/logout
Do: Open the first link, then the resent one. Register from the resent link and sign out.
Expect: The first link shows no form, only "This invitation is invalid.": the resend replaced the link's hash, so the old
  token matches no invitation. The resent link registers him and lands on "Viewing as PendingTrainee" with the Awaiting
  admission card.
Actual (2026-09-26, T295 replay, wombat_scenario): In the story he registered from his one link (nothing was resent).
  On the scratch copy, as Step 2.26's stand-in: the replaced link showed "Complete registration / This invitation is
  invalid." with no form; the resent link read "Registering lateregistrar@kgk.wombat.local as Trainee.", registered
  him, and landed on "Viewing as PendingTrainee" with the Awaiting admission card; he signed out
  (act-2/2.27-3-scratch-old-link-invalid.png, 2.27-4-scratch-registered.png).
Gap: none. The token stayed in the address bar after the page loaded [F-2.8a, T315].

## Phase 2.H — Admission

### Step 2.28 — The pending registrars, as Mbatha sees them
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/users → /admin/users/{UserId} → /admin/trainees
Do: Open Invitations. Read the users list and open Dr Molefe's page. Then open Trainees.
Expect: Active invitations reads "No active invitations". Users lists 15, with the five registrars as PendingTrainee. On
  Molefe's page, PendingTrainee reads "System-managed" and has no Remove. Add role offers InstitutionalAdmin,
  SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember and Assessor, not Trainee, and its help reads "Trainee
  is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit to curriculum'." (T303).
  Trainees' Pending admission lists the five with their emails and KGK, each with an
  "Admit to curriculum" named for the registrar. Active profiles reads "No active trainee profiles found."
Actual (2026-09-26, T295 replay, wombat_scenario): "No active invitations". Users lists 15, the five registrars as
  PendingTrainee. Molefe's page: PendingTrainee "System-managed", no Remove; Add role offers InstitutionalAdmin,
  SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember, Assessor and Trainee. Trainees: Pending admission
  lists the five with emails and KGK, each "Admit to curriculum: <name>"; "No active trainee profiles found."
  Re-checked after T303 (2026-09-26, wombat_scenario_rc303a, Act 2 replayed from scenario-post-act1 through this step,
  so on the story's own state): "No active invitations"; Users lists 15, the five registrars as PendingTrainee. Molefe's
  page: Roles lists PendingTrainee with "System-managed" and no button; Add role offers exactly InstitutionalAdmin,
  SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember and Assessor, never Trainee, with the T303 help
  quoted in the Expect under it (the select's aria-describedby names it). Trainees as before. Admitting Molefe (Step
  2.29's first, `J−3y` to `J+1y−1d`) still works: it moved to `?id=2`, Status Active; SQL shows her roles Trainee alone
  and profile 2 pinned to adoption 1; her page then reads Trainee "System-managed", no Remove, and no Trainee under Add
  role. A Trainee option forged into the select on a graduate's page (Molefe, on wombat_scenario_rc303c, a post-actA
  copy) is refused by the command: "The Trainee role cannot be added or removed here. A registrar becomes a trainee when
  admitted to a curriculum: open Trainees and choose 'Admit to curriculum'. Marking their programme complete takes the
  role away.", her roles unchanged (act-2/2.28-3-molefe-user.png, states/user-detail--pending-trainee.png re-taken).
Gap: [F-2.28a, T303] Add role offered Trainee to a PendingTrainee, a path round admission: fixed by T303 (4824d62).
  [F-2.28b, T323] Each Roles row runs the role's name into the "System-managed" note, with no space or gap:
  "PendingTraineeSystem-managed" (and "TraineeSystem-managed" on every admitted trainee's page since T303), both on
  screen and as the list item's accessible text. T323 already has the Remove button touching the name (same cause: an
  unstyled `li`, and Razor drops the whitespace between the two spans); this widens it to the note and its accessible
  text.

### Step 2.29 — Mbatha admits four registrars
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit each with its programme start and expected completion. Molefe starts `J−3y` and completes `J+1y−1d`. Dlamini
  starts `J−2y` and completes `J+2y−1d`. Du Plessis starts `J−1y` and completes `J+3y−1d`. Mahlangu starts `J` and
  completes `J+4y−1d`.
Expect: The form reads "Trainee profile" / "Admit a pending trainee into a curriculum.". Curriculum offers only
  "Paediatric EPA Curriculum (11.1)", KGK's adopted version (T091). The start date is pre-filled with today. There is no
  training-year field, because the year is derived. Each admission moves to the profile's edit page (`?id=`). That page
  shows Status Active and "Update curriculum and completion details for this trainee.", with Last day in the programme,
  Deactivate and Mark complete, none of them used here. The four appear under Active profiles with the curriculum,
  Paediatrics and the completion date entered.
Note: Admission into a version KGK has not adopted is refused (T091). The picker never offers one.
Actual (2026-09-26, T295 replay, wombat_scenario): "Trainee profile" / "Admit a pending trainee into a curriculum.";
  Curriculum offered only "Paediatric EPA Curriculum (11.1)"; start pre-filled 2026-09-26; no training-year field. Each
  admission moved to `?id=` (Molefe 2, Dlamini 3, du Plessis 4, Mahlangu 5): Status Active, "Update curriculum and
  completion details for this trainee.", Last day in the programme, Deactivate, Mark complete. Active profiles lists
  the four with 11.1, Paediatrics and 2027-01-14, 2028-01-14, 2029-01-14, 2030-01-14.
Gap: [F-2.29a, T306] Neither the admit form nor Active profiles shows the derived training year (or the start date), which
  T066's definition of done promised; unsure whether T130 superseded that.

### Step 2.30 — Dr Ndlovu's completion date is derived, then corrected
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit Ndlovu with start `J` and the completion date left empty. Then set completion to `J+4y−1d` on his profile and
  save the profile.
Expect: Left empty, the completion date is derived as `J` + 12 months, the longest item window on v11.1. The help text
  says so ("Leave this empty to derive it from the curriculum window."). The save reads "Trainee profile saved.", and
  the list shows `J+4y−1d`. Pending admission reads "No pending trainees found."
Note: A 12-month window for a four-year programme is T139's open College question.
Actual (2026-09-26, T295 replay, wombat_scenario): Help text "Leave this empty to derive it from the curriculum
  window."; admitted with it empty (profile 6), it was derived as 2027-01-15 (`J` + 12 months). Set to 2030-01-14 and
  saved: "Trainee profile saved."; the list shows 2030-01-14. Pending admission: "No pending trainees found."
Gap: none

### Step 2.31 — Dr Mahlangu's open session ends, and she signs back in as a Trainee
Role: Trainee — Dr Nomsa Mahlangu
Route: / → /account/session-ended → /account/login → /
Do: Her tab from Step 2.18 has stayed open through her admission. Once it leaves Home, she signs in again.
Expect: Within a minute of her admission the tab moves to the sign-in page, which reads "Your session has ended. Please
  sign in again." Admission changes her role, and a role change ends open sessions (T279). Signed in again, she sees
  "Viewing as Trainee" and the trainee dashboard of Step 2.39.
Actual (2026-09-26, T295 replay, wombat_scenario): 35 s after her admission (09:53:45 UTC) her Home tab, untouched,
  moved to /account/login?error=SessionEnded&returnUrl=%2F: "Your session has ended. Please sign in again." Signed in
  again: "Viewing as Trainee", Curriculum targets "Semester 2, 2026 · July to November", 0 / 10 and 0 / 5, PAED-001 to
  005 each "0 of 3 this semester"; the nav adds MSF Reports, My Committee Reviews, My Progress and Export Portfolio.
Gap: none

## Phase 2.I — First sign-in for every role

### Step 2.32 — Mr Smit, Coordinator
Role: Coordinator — Mr Pieter Smit
Route: /account/login → / → /admin/invitations → /access-denied → /committee/panels → /committee/panels/new → /access-denied
Do: Sign in and read the dashboard and nav. Type the invitations address. Type the Decision Panels address, then the
  new-panel address.
Expect: The dashboard reads "No stalled requests.", "No invitations expiring soon." and a Quick action, "Start an MSF
  campaign". The nav adds Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due and Stalled Activities.
  It has no Invitations, and the invitations page shows Access denied (T178). It has no Decision Panels either, though
  that page admits him: it lists the panel with no New panel and no Edit column. The panel form shows Access denied.
Actual (2026-09-26, T295 replay, wombat_scenario): the three cards as expected. Nav adds exactly the five named, no
  Invitations or Decision Panels. Invitations and the panel form go to /access-denied; /committee/panels lists the
  panel (Speciality, General panel, 4) and the Who-decides card, with no New panel, no Edit column and no link.
Gap: none

### Step 2.33 — Dr Zulu's first view: committee member
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: /account/login → /
Do: Sign in and read the dashboard and nav.
Expect: The page reads "Viewing as CommitteeMember" and "You also act as Assessor. Switch view: Assessor". Targets this
  period names the current semester and its months. It lists the five registrars, each at "semester 0/10 · yearly 0/5",
  and nobody is exempt: a 15 January start counts from the boundary (D42). Targets met by EPA lists PAED-001 to PAED-015,
  each "0 of 5 met" (T130's count, never a percentage). The nav reads Activity Inbox, Recent Activities, Programme
  Trainees, Decision Panels and Committee Reviews, each once (T178).
Actual (2026-09-26, T295 replay, wombat_scenario): as expected: the switch line; "Semester 2, 2026 · July to November";
  five registrars at "semester 0/10 · yearly 0/5" (order Dlamini, Molefe, Ndlovu, Mahlangu, du Plessis), no exempt line;
  PAED-001 to 015 each "0 of 5 met". Nav: Home, My Account, Data Rights, then the five named once each, Logout.
Gap: [F-2.33a, T298] With every trainee level, the Targets card lists them by user id (the tie-break after "fewest met
  first"), an order no reader can follow; a name tie-break would read. Unsure: the primary order is deliberate.

### Step 2.34 — Dr Zulu switches between her dashboards
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /account/logout → /account/login → / → /dashboard/switch/{role} → /
Do: Choose Switch view: Assessor. Sign out and in again. Then type `/dashboard/switch/Administrator`.
Expect: The page reads "Viewing as Assessor" and "You also act as CommitteeMember. Switch view: CommitteeMember". The
  Assessor dashboard reads 0 "assessments awaiting review" with Review inbox, "Nothing is awaiting your review." (T297),
  "No decisions yet." and "Open my inbox →". The nav is unchanged. After signing in again she lands on the Assessor view,
  because the choice is remembered. The Administrator address brings her back to "Viewing as CommitteeMember": a role she
  does not hold is never shown.
Actual (2026-09-26, T295 replay, wombat_scenario): "Viewing as Assessor", "You also act as CommitteeMember. Switch view:
  CommitteeMember"; Pending requests 0 "assessments awaiting review" "Review inbox →", the two empty lines, "Open my
  inbox →"; nav unchanged. Nav Logout (POST /account/logout), sign-in lands on the Assessor view; the Administrator
  switch returns to / "Viewing as CommitteeMember".
  Re-checked after T297 (2026-09-26, wombat_scenario_rc297b2): after the switch, "Viewing as Assessor" with the
  CommitteeMember switch line; Pending requests 0 "assessments awaiting review" with "Review inbox →", "Awaiting your
  review" reading "Nothing is awaiting your review.", "No decisions yet." and "Open my inbox →", as the Expect
  (rewritten by T297) says.
Gap: [F-2.34a, T317] The remembered view is a 30-day browser cookie (`wombat_preferred_dashboard_role`), not the account's:
  it survives sign-out, so Naidoo, signing in next in the same browser, landed on "Viewing as Assessor" (Zulu's choice).

### Step 2.35 — Dr Naidoo and Dr Botha
Role: CommitteeMember + Assessor — Dr David Naidoo and Dr Sarah Botha
Route: /account/login → / → /committee/panels
Do: Each signs in, reads the dashboard and opens Decision Panels.
Expect: Each sees what Zulu saw in Step 2.33. Decision Panels lists the panel they sit on, with no New panel and no
  Edit.
Actual (2026-09-26, T295 replay, wombat_scenario): each: "Viewing as CommitteeMember" with the Assessor switch line,
  the same five registrars at 0/10 · 0/5 and PAED-001 to 015 at "0 of 5 met", Zulu's nav. Decision Panels lists Paed
  Annual Review Panel (Speciality, General panel, 4) and the Who-decides card, with no New panel and no Edit.
Gap: none

### Step 2.36 — Dr Patel and Dr Khumalo, assessors
Role: Assessor — Dr Mohammed Patel and Dr Fatima Khumalo
Route: /account/login → /
Do: Each signs in and reads the dashboard and nav.
Expect: Each sees "Viewing as Assessor" with no Switch view line and the empty Assessor dashboard of Step 2.34. The nav
  adds only Activity Inbox and Recent Activities.
Actual (2026-09-26, T295 replay, wombat_scenario): each: "Viewing as Assessor", no switch line, Pending requests 0,
  "No accepted assessments pending action.", "No decisions yet.", "Open my inbox →". Nav: Home, My Account, Data
  Rights, Activity Inbox, Recent Activities, Logout.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc297b2): each reads Pending requests 0 with "Review inbox →",
  "Awaiting your review" with "Nothing is awaiting your review." (the pre-fix "No accepted assessments pending action."
  is gone), "No decisions yet." and "Open my inbox →": the empty dashboard of Step 2.34.
Gap: [F-2.36a, T328] The "Awaiting your review" card, new with T297, always carries the alert-triangle icon
  (AssessorDashboard.razor:23), so it heads "Nothing is awaiting your review." with a warning sign; only its Warning
  styling waits for an overdue item. Unsure: the icon may be meant as the card's emblem.

### Step 2.37 — Dr van Rensburg, external committee member
Role: CommitteeMember — Dr John van Rensburg
Route: /account/login → / → /committee/panels
Do: Sign in, read the dashboard and open Decision Panels.
Expect: He sees "Viewing as CommitteeMember" with no Switch view line. Targets this period names the current semester
  and reads "No trainees have targets this period.", and Targets met by EPA reads "No curriculum targets for these
  trainees.": the card lists the trainees of the member's own sub-specialities at his institution, and he holds none.
  The nav reads Programme Trainees, Decision Panels and Committee Reviews. Decision Panels lists the panel he sits on as
  external member, with no New panel and no Edit.
Note: Zulu, Naidoo and Botha hold the Paediatrics sub-speciality only through their assessor profiles (Step 2.14), and a
  CommitteeMember invitation may carry none (Step 2.3). So a committee member with no assessor profile sees no trainee
  here, although he sits on the panel and may read every KGK trainee's record (T113). Reported as a suspected defect.
Actual (2026-09-26, T295 replay, wombat_scenario): as written: "Viewing as CommitteeMember", no switch line, "Semester
  2, 2026 · July to November", "No trainees have targets this period.", "No curriculum targets for these trainees.".
  Nav: Home, My Account, Data Rights, then the three named, Logout. Panels as Naidoo saw them, no New panel, no Edit.
Gap: [F-2.37a, T290] Confirmed: the panel's external member, who may read every KGK trainee (T113), sees an empty committee
  dashboard, because the card filters on sub-speciality claims he cannot hold (he has no scope rows).

### Step 2.38 — Dr Mokoena's and Dr Sithole's dashboards
Role: SpecialityAdmin and SubSpecialityAdmin — Dr Refilwe Mokoena and Dr Kabelo Sithole
Route: /
Do: Each opens Home, now that the registrars are admitted.
Expect: Each sees "Viewing as SpecialityAdmin" or "Viewing as SubSpecialityAdmin". Pending reviews reads 0 "activities
  awaiting review", with no link (T297). Trainees in programme reads 5 active / 0 inactive. Curriculum coverage names the current
  semester and lists PAED-001 to PAED-015 at "0 of 5 met". The nav reads Programme Trainees, Decision Panels, Committee
  Reviews, STAR Review Queue and Decisions Due.
Actual (2026-09-26, T295 replay, wombat_scenario): both as expected: Pending reviews 0 "activities in review" "Review
  queue →"; 5 "active / 0 inactive"; "Curriculum coverage — Semester 2, 2026" over "Semester 2, 2026 · July to
  November", PAED-001 to 015 at "0 of 5 met". Nav: Home, My Account, Data Rights, the five named, Logout.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc297b2): both read Pending reviews 0 "activities awaiting
  review", with no link, as the Expect (rewritten by T297) says; "activities in review" and "Review queue →" are gone.
  Trainees in programme and Curriculum coverage as before.
Gap: none

### Step 2.39 — Dr Molefe, final-year registrar
Role: Trainee — Dr Lerato Molefe
Route: /account/login → / → /portfolio/progress
Do: Sign in, read the dashboard and nav, and open My Progress.
Expect: Curriculum targets names the current semester and reads "0 / 10" semester targets met and "0 / 5" yearly targets
  met. The largest shortfalls are PAED-001 to PAED-005, each "0 of 3 this semester", and there is no "started part-way"
  line (D42). The other cards read "No pending items.", "No activities yet.", "No deadlines in the next 14 days.", My
  authorisations, and Log an activity / Request an assessment. The nav adds Activities, My Activities, MSF Reports, My
  Committee Reviews, My Progress and Export Portfolio. My Progress reads "0 of 10 EPAs met this semester", "0 of 5 EPAs
  met in <year>" and Training year "4 — it sets the minimum level each encounter is judged against".
Actual (2026-09-26, T295 replay, wombat_scenario): dashboard and nav exactly as expected, no part-way line. My
  Progress: "0 of 10 EPAs met this semester", "0 of 5 EPAs met in 2026", Training year "4 — it sets…"; each yearly EPA
  adds "Your training year changed on 14 Jan 2026" (365-day blocks across 2024's leap day, as the act's Dates says).
Gap: [F-2.39a, T306] The trainee dashboard shows no training year; only My Progress does. T066's verification promised it on
  the dashboard (and the admit form and list). Unsure: T130's My Progress may have superseded that.

### Step 2.40 — The other registrars' training years
Role: Trainee — Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu
Route: /account/login → / → /portfolio/progress
Do: Each signs in and opens My Progress.
Expect: Each dashboard has Molefe's shape and figures. Training year reads 3 for Dlamini, 2 for du Plessis and 1 for
  Mahlangu and Ndlovu. Their semester and yearly targets apply now: a start on 15 January is on time for both (D42).
Actual (2026-09-26, T295 replay, wombat_scenario): each dashboard reads "0 / 10", "0 / 5" (2026) and PAED-001 to 005
  "0 of 3 this semester", no part-way line. My Progress: 0 of 10, 0 of 5 in 2026; Training year 3 (Dlamini), 2 (du
  Plessis), 1 (Mahlangu), 1 (Ndlovu).
Gap: none

### Step 2.41 — Everyone reviews their account; Dr Khumalo corrects her name
Role: Every role in this act — each person onboarded here, signed in as themselves
Route: /account/profile
Do: Each opens My Account. Dr Khumalo changes her first name from "Fatma" to "Fatima" and saves the profile.
Expect: The page reads "My account" and "Update your name and review your assigned roles.". The summary shows the email,
  the roles held (both roles for Zulu, Naidoo and Botha; Trainee for the registrars) and a Change password link. The
  email field is not editable. Khumalo's save reads "Profile saved." (checked on Mbatha's lists in Step 2.44).
Actual (2026-09-26, T295 replay, wombat_scenario): all 14 read "My account" and the subtitle; Roles "CommitteeMember,
  Assessor" for Zulu, Naidoo and Botha, "Trainee" for the five, each other its one role; Change password link; Email
  input disabled. Khumalo: Fatma → Fatima, Save profile: "Profile saved.". At 390 px no sideways scroll.
Gap: none

### Step 2.42 — The instruments a KGK registrar is offered
Role: Trainee — Dr Lerato Molefe
Route: /activities/new
Do: Open the Activity type select and read its options. Choose each in turn to see what it rates, and save nothing.
Expect: Eleven options besides "Select…", no name twice: Case-Based Discussion, Chart-Stimulated Recall, Clinical Audit,
  Clinical Case Analysis, Direct Observation, DOPS, Mini-CEX, Portfolio and Logbook Review, Random Case Analysis and
  Reflective Exercise, each "(Paediatrics)", and `KGK Teaching Session Log`. Seven of them rate on `1 2 3a 3b 4 5`:
  CBD, Chart-Stimulated Recall, CCA, Direct Observation, DOPS, Mini-CEX and RCA. Multi-Source Feedback and Learner
  Feedback (Paediatrics) are not offered, because only the system writes them (T162, T164). Nor are the Demo types,
  which belong to another discipline.
Actual (2026-09-26, T295 replay, wombat_scenario): "Select…" and exactly the eleven named, alphabetical, none twice; no
  MSF, Learner Feedback or Demo type. Overall level offers "1,2,3a,3b,4,5" on exactly CBD, CSR, CCA, Direct
  Observation, DOPS, Mini-CEX and RCA; Audit, Portfolio Review, Reflective and the Teaching Log have none. None saved.
Gap: none

### Step 2.43 — The Mini-CEX link and whom it may name
Role: Trainee — Dr Lerato Molefe
Route: /activities/new
Do: Open `/activities/new?type=mini_cex_cpsa`. Read the EPA and Assessor pickers, then leave without saving.
Expect: The Mini-CEX (Paediatrics) is already chosen, with the sections Request, Entrustment and Feedback, and Save
  draft and Submit. The EPA picker offers the nine EPAs whose Annexure A list names the Mini-CEX: PAED-001, 002, 003,
  004, 006, 007, 008, 012 and 013 (T122). The Assessor picker offers exactly Botha, Khumalo, Naidoo, Patel and Zulu, each
  with their email. It does not offer van Rensburg, Molefe herself or any Demo account (T102). Nothing is saved.
Actual (2026-09-26, T295 replay, wombat_scenario): Mini-CEX (Paediatrics) preselected; Request, Entrustment and
  Feedback (the last two's fields disabled); Save draft and Submit. EPA: PAED-001, 002, 003, 004, 006, 007, 008, 012,
  013. Assessor: Botha, Khumalo (as Fatima), Naidoo, Patel, Zulu, each "(email)"; nobody else. Left unsaved.
Gap: none

### Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/assessors
Do: Open Home. Then open Users and Assessors.
Expect: The page reads "Viewing as InstitutionalAdmin". Users reads InstitutionalAdmin 1, SpecialityAdmin 1,
  SubSpecialityAdmin 1, Coordinator 1, CommitteeMember 4, Assessor 5 and Trainee 5, with no PendingTrainee line.
  Specialities & sub-specialities reads 1 and 1, what KGK has adopted. Quick links reads Users, Invitations, Curriculum
  adoptions and Entrustment decisions. The users and assessors lists both name Fatima Khumalo (Step 2.41).
Note: The adopted count is T291 item 4's decision, still queued: the query counts the whole national catalogue, which
  with the Demo world reads 2 and 2, so expect a Gap until T291 lands.
Actual (2026-09-26, T295 replay, wombat_scenario): "Viewing as InstitutionalAdmin"; Users 1/1/1/1/4/5/5 as expected,
  no PendingTrainee line; Specialities & sub-specialities reads 2 and 2; the four Quick links. Users lists 15 KGK
  accounts and Assessors five profiles, both naming Fatima Khumalo.
Gap: [F-2.44a, T291] Specialities & sub-specialities reads 2 and 2 (the whole national catalogue with the Demo world), not
  KGK's adopted 1 and 1: T291 item 4, still queued.

## Act 2 outcome state

Replay check (2026-09-26, T295, wombat_scenario): match; all 7 queries as expected, invitations 15 | 1 | 0.

- **15 KGK users:** Mbatha (InstitutionalAdmin); Smit (Coordinator); Zulu, Naidoo and Botha (CommitteeMember and
  Assessor); Patel and Khumalo (Assessor); van Rensburg (CommitteeMember); Mokoena (SpecialityAdmin); Sithole
  (SubSpecialityAdmin); the five registrars (Trainee, no longer PendingTrainee).
- **5 assessor profiles** at KGK, Paediatrics / Paediatrics: three Trained, one In training, one Provisional.
- **5 trainee profiles** on curriculum 11.1, each pinned to KGK's adoption. They start on `J−3y`, `J−2y`, `J−1y`, `J`
  and `J`, which gives training years 4, 3, 2, 1 and 1.
- **1 decision panel,** `Paed Annual Review Panel`: Speciality · Paediatrics, a general panel. Zulu chairs, Naidoo and
  Botha are members and van Rensburg is external.
- **No active invitation;** the mistyped one is revoked. No activities, reviews, decisions or STARs.

Queries to compare against (psql). Each comment is the expected result.

```sql
-- Users and their roles
SELECT u."Email", string_agg(r."Name", '+' ORDER BY r."Name") AS roles
  FROM "AspNetUsers" u
  JOIN "Institutions" i ON i."Id" = u."InstitutionId"
  LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
  LEFT JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
 WHERE i."Name" = 'Kgosi Kgari Teaching Hospital'
 GROUP BY u."Email" ORDER BY 1;
  -- 15 rows: botha, naidoo, zulu Assessor+CommitteeMember; khumalo, patel Assessor; mbatha InstitutionalAdmin;
  -- mokoena SpecialityAdmin; sithole SubSpecialityAdmin; smit Coordinator; vanrensburg CommitteeMember;
  -- dlamini, duplessis, mahlangu, molefe, ndlovu Trainee

-- Scope: what the invitations, the assessor profiles and the admissions wrote
SELECT u."Email",
       (SELECT count(*) FROM "UserSpecialityScopes" s WHERE s."UserId" = u."Id") AS specialities,
       (SELECT count(*) FROM "UserSubSpecialityScopes" s WHERE s."UserId" = u."Id") AS sub_specialities
  FROM "AspNetUsers" u JOIN "Institutions" i ON i."Id" = u."InstitutionId"
 WHERE i."Name" = 'Kgosi Kgari Teaching Hospital' ORDER BY 1;
  -- mbatha, smit, vanrensburg 0 | 0; mokoena 1 | 0; the other eleven 1 | 1

-- Trainee profiles, pinned to KGK's active adoption of 11.1
SELECT u."Email", p."ProgrammeStartDate", p."ExpectedCompletionDate", p."IsActive", a."Id" IS NOT NULL AS pinned
  FROM "TraineeProfiles" p
  JOIN "AspNetUsers" u ON u."Id" = p."UserId"
  JOIN "Institutions" i ON i."Id" = p."InstitutionId"
  JOIN "Curricula" c ON c."Id" = p."CurriculumId"
  LEFT JOIN "InstitutionCurriculumAdoptions" a
         ON a."Id" = p."AdoptionId" AND a."InstitutionId" = p."InstitutionId"
        AND a."CurriculumId" = p."CurriculumId" AND a."IsActive"
 WHERE i."Name" = 'Kgosi Kgari Teaching Hospital' AND c."SeedKey" = 'cpsa:paediatrics:curriculum:v11.1'
 ORDER BY 2, 1;
  -- 5 rows, all active and pinned: molefe J−3y / J+1y−1d; dlamini J−2y / J+2y−1d; duplessis J−1y / J+3y−1d;
  -- mahlangu and ndlovu J / J+4y−1d

-- Assessor profiles (TrainingStatus 1 = In training, 2 = Provisional, 3 = Trained)
SELECT u."Email", a."TrainingStatus", a."TrainingCompletedOn", s."Name" AS speciality, ss."Name" AS sub_speciality
  FROM "AssessorProfiles" a
  JOIN "AspNetUsers" u ON u."Id" = a."UserId"
  JOIN "Institutions" i ON i."Id" = a."InstitutionId"
  LEFT JOIN "Specialities" s ON s."Id" = a."SpecialityId"
  LEFT JOIN "SubSpecialities" ss ON ss."Id" = a."SubSpecialityId"
 WHERE i."Name" = 'Kgosi Kgari Teaching Hospital' ORDER BY 1;
  -- 5 rows, each Paediatrics | Paediatrics: botha 3, no date; khumalo 2, D−1 month; naidoo 3, D−6 years;
  -- patel 1, no date; zulu 3, D−8 years

-- The panel (Scope 2 = Speciality; Role 1 = Chair, 2 = Member, 3 = External)
SELECT p."Name", p."Scope", p."DecisionBodyKey", m."Role", u."Email"
  FROM "DecisionPanels" p
  JOIN "DecisionPanelMembers" m ON m."PanelId" = p."Id"
  JOIN "AspNetUsers" u ON u."Id" = m."UserId"
 WHERE p."Name" = 'Paed Annual Review Panel' ORDER BY m."Role", u."Email";
  -- 4 rows, Scope 2, DecisionBodyKey null: 1 zulu; 2 botha; 2 naidoo; 3 vanrensburg

-- Invitations at KGK
SELECT count(*) FILTER (WHERE v."UsedOn" IS NOT NULL) AS used,
       count(*) FILTER (WHERE v."RevokedOn" IS NOT NULL) AS revoked,
       count(*) FILTER (WHERE v."UsedOn" IS NULL AND v."RevokedOn" IS NULL) AS active
  FROM "Invitations" v JOIN "Institutions" i ON i."Id" = v."InstitutionId"
 WHERE i."Name" = 'Kgosi Kgari Teaching Hospital';
  -- 15 | 1 | 0: fourteen used in this act plus Prof Mbatha's from Act 1; the revoked one is ndlvou@

-- Nothing filed or reviewed yet
SELECT (SELECT count(*) FROM "Activities" a JOIN "Institutions" i ON i."Id" = a."InstitutionId"
         WHERE i."Name" = 'Kgosi Kgari Teaching Hospital') AS activities,
       (SELECT count(*) FROM "CommitteeReviews" r JOIN "DecisionPanels" p ON p."Id" = r."PanelId"
         WHERE p."Name" = 'Paed Annual Review Panel') AS reviews;
  -- 0 | 0
```

Snapshot: `pg_dump -Fc` to `recovery/scenario-post-act2.dump`. The passwords chosen in this act are in
`pwd_DO_NOT_COMMIT.txt`.

## Handoff to Act 3

Act 3 (months 1–6) starts the assessment rhythm. It needs from this act:
- **The registrars,** admitted and in training years 4, 3, 2, 1 and 1. Molefe and Dlamini build the completed evidence
  Act 4 stages STARs from. Du Plessis, Mahlangu and Ndlovu supply the stalled, declined and returned work.
- **The nominable assessors:** Zulu, Naidoo, Botha, Patel and Khumalo. They are the only people a KGK registrar's
  assessor field offers (Step 2.43). Van Rensburg never assesses.
- **Mr Smit,** the Coordinator at KGK, who runs Molefe's MSF campaign.
- **The staff dashboards** of Mokoena, Sithole, Zulu and Smit, all empty now, which Act 3 watches fill.
- **The panel,** `Paed Annual Review Panel`, which sits for all five registrars in Act 4 with Zulu in the chair.
