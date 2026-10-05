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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Signed in to Home and opened Invitations from the sidebar: "No
  active invitations" / "Issue an invitation to create the first onboarding link." (act 1's revoked second Mbatha
  invitation is not listed). Role offers the seven named, InstitutionalAdmin selected, no CollegeAdmin. Institution
  offers KGK alone (behind "Select institution"). Speciality and Sub-speciality are disabled.
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Coordinator enabled Speciality (Paediatrics); Sub-speciality
  stayed disabled. "Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is
  shown only once.", then "It expires in 14 days" and the link; row Coordinator, KGK, scopes blank, issued 2026-10-04
  13:11, expires 2026-10-18 (`D+14`), "Being sent"; the form back to its defaults (InstitutionalAdmin, Institution
  unselected, Speciality disabled). The log holds a stub "Your Wombat invitation" tagged `invitation,
  role:Coordinator`.
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Speciality offered Paediatrics alone; Sub-speciality stayed
  disabled for CommitteeMember. Three rows read CommitteeMember, KGK, Paediatrics, sub-speciality blank, "Being sent",
  each issued with its own link; three stubs tagged `role:CommitteeMember`.
Gap: none

### Step 2.4 — Mbatha invites Dr Patel and Dr Khumalo as assessors
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation each to `patel@` and `khumalo@kgk.wombat.local` as Assessor at KGK, with speciality Paediatrics
  and sub-speciality Paediatrics.
Expect: For an Assessor the Sub-speciality select is enabled. Once Paediatrics is chosen as the speciality, it offers
  that speciality's sub-speciality, Paediatrics. Two rows read Assessor, KGK, Paediatrics, Paediatrics.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): For Assessor the Sub-speciality select was enabled, and with
  Paediatrics chosen as the speciality it offered Paediatrics. Two rows read Assessor, KGK, Paediatrics, Paediatrics;
  two stubs tagged `role:Assessor`.
Gap: none

### Step 2.5 — Mbatha invites Dr van Rensburg as an external committee member
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation to `vanrensburg@sun.wombat.local` as CommitteeMember at KGK, with speciality and sub-speciality
  blank.
Expect: It is issued with no speciality (T060). The row reads CommitteeMember, KGK, with both scope columns blank. The
  address's domain does not matter: the invitation scopes him to KGK.
Note: A panel seats only committee members at its own institution (T165), so the Stellenbosch examiner is invited at KGK.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Issued with no speciality. The row reads CommitteeMember, KGK,
  both scope columns blank, expires 2026-10-18, "Being sent".
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The first was refused with "Speciality administrators must be
  scoped to a speciality." and no row was added; van Rensburg's "Share this registration link…" box and link (Step 2.5)
  stayed on screen under the refusal, as the Note says. The second was issued: SpecialityAdmin, KGK, Paediatrics;
  Sub-speciality stayed disabled.
Gap: [F-2.6a, T264] (still) A refused issue leaves the previous invitee's registration link (van Rensburg's) on screen
  under the refusal.

### Step 2.7 — Mbatha invites Dr Sithole as the SubSpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue `sithole@kgk.wombat.local` as SubSpecialityAdmin at KGK with speciality Paediatrics and no sub-speciality.
  Then issue it again with sub-speciality Paediatrics.
Expect: The first is refused: "The selected role requires speciality and sub-speciality scope." The second is issued.
  Active invitations now holds nine rows: Smit, Zulu, Naidoo, Botha, Patel, Khumalo, van Rensburg, Mokoena and Sithole,
  each with the role and scope above.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The first was refused with "The selected role requires
  speciality and sub-speciality scope.", and Mokoena's "Share this registration link…" box and link stayed on screen
  under it (F-2.6a). The second was issued. Nine rows, Smit to Sithole, each role and scope as above, all expiring
  2026-10-18 and "Being sent"; nine stubs in the log, each tagged with its role.
Gap: [F-2.6a, T264] (still) A refused issue leaves the previous invitee's registration link (Mokoena's) on screen under
  the refusal.

## Phase 2.B — The staff register

### Step 2.8 — Mr Smit registers from his link
Role: Anonymous — Mr Pieter Smit, holding his invitation link
Route: /account/register → / → /account/logout/submit → /account/login
Do: Open the link and enter first name Pieter, last name Smit and a password that meets the rules. Confirm it and
  register. Read the landing page, then sign out.
Expect: The page reads "Registering smit@kgk.wombat.local as Coordinator.", with the email filled in and not editable.
  The token is cleared from the address bar once the page loads. Registering signs him in and lands on Home, headed
  "Home" with "Coordinator · Semester N, YYYY" under it. Signing out (the top bar's Sign out, one press) returns him to
  the sign-in page, which reads "You have signed out."
Note: Record every password chosen in this act in `pwd_DO_NOT_COMMIT.txt` only (README § Passwords).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "Registering smit@kgk.wombat.local as Coordinator.", the email
  filled and disabled. The address bar still held `?token=…` after load (and 3 s later). Registering landed on Home:
  "Home", "Coordinator · Semester 2, 2026", the top bar reading "Pieter Smit". The top bar's Sign out, one press,
  returned to /account/login?error=SignedOut reading "You have signed out.".
Gap: [F-2.8a, T315] (still) The invitation token is never cleared from the address bar after the page loads.

### Step 2.9 — Dr Patel's first two attempts are refused
Role: Anonymous — Dr Mohammed Patel, holding his invitation link
Route: /account/register → / → /account/logout/submit → /account/login
Do: Register with a 10-character password that mixes upper case, lower case, a digit and a symbol. Then use a
  12-character password with a different confirmation. Then use a 12-character password confirmed correctly. Sign out.
Expect: Before anything is typed, the six rules stand under Password: "The new password needs:" and "At least 12
  characters.", "At least 4 different characters.", "A digit (0 to 9).", "An upper-case letter.", "A lower-case
  letter.", "A symbol, such as ! or #."; each password field has its Show toggle. The first attempt returns to the
  form with "The new password needs: At least 12 characters." The first field takes the focus and no account is
  created (T285). The second returns with "The password confirmation does not match." The third registers him and
  lands on Home, "Assessor · Semester N, YYYY". Signing out lands on the sign-in page with "You have signed out."
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Before typing: "The new password needs:" and the six rules word
  for word under Password, and a Show toggle on each password field ("Show password", "Show confirm password"). 10
  characters: back at `&error=PasswordTooShort` with "The new password needs: At least 12 characters.", First name
  focused; the same invitation registered him at the third try, so no account was made. Mismatch: back at
  `&error=ConfirmationMismatch`, "The password confirmation does not match.". Each refusal emptied First and Last name.
  The third landed on Home, "Assessor · Semester 2, 2026" ("Waiting for you", "Nothing is waiting for you.", "Open
  Activity inbox", Recent decisions "No decisions yet."); signing out read "You have signed out.".
Gap: [F-2.9a, T317] (still) A refused registration drops the first and last name typed, so each retry retypes them.

### Step 2.10 — The other seven staff register
Role: Anonymous — Dr Zulu, Dr Naidoo, Dr Botha, Dr Khumalo, Dr van Rensburg, Dr Mokoena and Dr Sithole
Route: /account/register → / → /account/logout/submit → /account/login
Do: Each registers from their own link with the cast's name and signs out. Dr Khumalo types her first name as "Fatma"
  by mistake (she corrects it in Step 2.41).
Expect: Each page names the address and the invited role, and lists the six password rules under Password (Step 2.9).
  Each person lands on Home viewing as that role. Zulu, Naidoo, Botha and van Rensburg view as CommitteeMember, Khumalo
  as Assessor, Mokoena as SpecialityAdmin and Sithole as SubSpecialityAdmin. Each sign-out lands on the sign-in page
  with "You have signed out."
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Each page read "Registering <address> as <role>." with the
  email disabled and the six rules under Password, and each landed on Home: Zulu, Naidoo, Botha and van Rensburg
  "Committee member · Semester 2, 2026", Khumalo (typed "Fatma") "Assessor · …", Mokoena "Speciality admin · …" and
  Sithole "Sub-speciality admin · …", the sidebar acting as that role. Each sign-out landed on the sign-in page with
  "You have signed out.".
Gap: none

### Step 2.11 — A used link cannot be used again
Role: Anonymous — Mr Pieter Smit
Route: /account/register
Do: Open his invitation link a second time.
Expect: No form is shown, only "This invitation has already been used." (T285's words).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "Complete registration" with "This invitation has already been
  used." and no form (no input).
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "No active invitations". Users: ten rows (Mbatha and the nine),
  all KGK, Active, each with its invited role; no Demo, devadmin or Kruger row. "zulu" left Zulu alone; cleared, ten
  again. Manage on her row: trail Home › Users › Nolwazi Mbatha, the subtitle and the note as quoted, then "Change your
  password from your account." with the Change your password link (/account/change-password). Roles lists
  InstitutionalAdmin; the page has no button: no Remove, Add role, Reset password or Lockout.
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Before: Roles "CommitteeMember" with "Remove the
  CommitteeMember role"; Add role offers exactly InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator
  and Assessor, with the T303 help under it (aria-describedby); the Lockout card lists four effects; "No active
  invitations are outstanding for this email.". After: "Role 'Assessor' added." (it takes the focus), Assessor gone
  from Add role. Same for Naidoo and Botha; the users list's Roles reads "CommitteeMember, Assessor" for all three. The
  log holds three pairs of "Navigation failed when changing the location to /account/session-ended" and "Unhandled
  exception in circuit" (TaskCanceledException), about a minute after Step 2.14's saves changed the three committee
  members' security stamps, a few minutes after they had signed out (log lines 29780-29975); none at the admissions.
Gap: [F-2.13a, T290] (still) A role or scope change on a recently signed-out, disconnected circuit logs a cancelled
  session-ended navigation and "Unhandled exception in circuit" at fail level, three times here.

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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): List first "No assessor profiles" / "Create the first assessor
  profile to make assessors selectable in the workflow.". Picker: the five KGK Assessors only, each "(email)";
  Institution KGK alone; Sub-speciality enabled once a speciality is chosen; Training status offers the four; the date
  shown only for Provisional or Trained. An empty save reads "The UserId field is required." and "The Qualifications
  field is required.". Each save read "Assessor profile saved." (focused) but stayed at /admin/assessors/edit, and
  every new form, a sixth included, still offered all five.
Gap: [F-2.14a, T289] (still) No move to `?id=` after a save (T064). [F-2.14b, T289] (still) The new form's picker is
  not narrowed to unprofiled assessors (T064). [F-2.14c, T326] (still) The empty list says a profile makes assessors
  "selectable in the workflow". [F-2.14d, T324] (still) The empty save reads "The UserId field is required.".

### Step 2.15 — The assessor list and who is not on it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors
Do: Read the list.
Expect: Five rows, each at KGK in Paediatrics / Paediatrics. Training status reads Trained for Zulu, Naidoo and Botha, In
  training for Patel and Provisional for Khumalo. Training completed reads `D−8 years`, `D−6 years`, "Not recorded",
  "Not recorded" and `D−1 month` respectively. Each row has an Edit named for its assessor. Van Rensburg is neither listed
  nor offered, because he does not hold the Assessor role.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Five rows, KGK, Paediatrics / Paediatrics: Botha Trained "Not
  recorded", Khumalo (Fatma) Provisional 2026-09-04, Naidoo Trained 2020-10-04, Patel In training "Not recorded", Zulu
  Trained 2018-10-04. Each Edit is named "Edit <name>". Van Rensburg is not listed and was never offered.
Gap: none

## Phase 2.D — Registrar invitations

### Step 2.16 — Mbatha invites the five registrars, one to a mistyped address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Invite `molefe@`, `dlamini@`, `duplessis@` and `mahlangu@kgk.wombat.local` as Trainee at KGK, Paediatrics /
  Paediatrics. Copy each link. For Dr Ndlovu she types `ndlvou@kgk.wombat.local`.
Expect: A Trainee invitation needs both speciality and sub-speciality, and the Sub-speciality select is enabled for it.
  Five rows read Trainee, KGK, Paediatrics, Paediatrics, and each Delivery reads "Being sent".
Actual (2026-10-04, T355 replay, wombat_scenario_t355): For Trainee the Sub-speciality select was enabled; both scopes
  were chosen. Five rows read Trainee, KGK, Paediatrics, Paediatrics, issued 2026-10-04 13:16, expiring 2026-10-18,
  each "Being sent".
Gap: none

### Step 2.17 — Mbatha revokes the mistyped invitation and issues the right one
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /account/register
Do: Revoke the `ndlvou@` row and issue a Trainee invitation to `ndlovu@kgk.wombat.local` with the same scope. Then open
  the revoked link, as anyone holding it could.
Expect: The row's button is named "Revoke the Trainee invitation to ndlvou@kgk.wombat.local" (T239). Pressing it shows
  "Invitation revoked." and the row is gone, with no link shown (see Step 2.6's note). An address is never corrected in
  place: the invitation names who may register (T283). A new row reads `ndlovu@`. The revoked link shows no form, only
  "This invitation has been revoked."
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The button is named "Revoke the Trainee invitation to
  ndlvou@kgk.wombat.local"; pressing it showed "Invitation revoked." and the row went, but the "Share this registration
  link…" box still showed the revoked ndlvou@ link under it. The ndlovu@ row was added (issued 13:17). The revoked
  link, in a separate browser, read "This invitation has been revoked." with no form.
Gap: [F-2.6a, T264] (still) A revoke leaves the revoked invitation's link on screen. The link itself is dead ("This
  invitation has been revoked.").

## Phase 2.E — The registrars register and wait

### Step 2.18 — Four registrars register and are pending
Role: Anonymous — Dr Molefe, Dr Dlamini, Dr Mahlangu and Dr Ndlovu
Route: /account/register → / → /account/logout/submit → /account/login
Do: Each registers from their own link. Molefe, Dlamini and Ndlovu sign out. Dr Mahlangu stays signed in with Home open
  in her tab, which Step 2.31 uses. Dr du Plessis cannot register: he says no email reached him.
Expect: Each page reads "Registering <address> as Trainee." Each registrar lands on Home, "Pending trainee · Semester N,
  YYYY", because a Trainee invitation registers as PendingTrainee until admission. Home shows one card, "Awaiting
  admission": "You are registered and waiting to be admitted to a curriculum by your programme administrator." It has a
  "Review your account →" link, and the header offers no action. Each sign-out lands on the sign-in page with "You
  have signed out."
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Each page read "Registering <address> as Trainee."; each landed
  on Home, "Pending trainee · Semester 2, 2026", with the one "Awaiting admission" card, its sentence as quoted and
  "Review your account →", and no header action. Molefe, Dlamini and Ndlovu signed out to "You have signed out.";
  Mahlangu stayed signed in, on Home, in her own browser, held open by a background script from 13:17. Du Plessis did
  not register.
Gap: none

### Step 2.19 — What a registrar sees before admission
Role: PendingTrainee — Dr Nomsa Mahlangu
Route: / → /account/profile → /activities/mine → /activities/new → /portfolio/progress → /access-denied → /
Do: Read the menu. Follow Review your account, then open My activities and Log an activity, and file nothing. Type the
  My progress address.
Expect: The sidebar reads "Acting as Pending trainee" over Home, Log an activity and My activities, then My data rights
  under the rule. It has no MSF reports, My committee reviews, My progress or Export portfolio, because those pages do
  not admit a pending trainee (T141). My account lists her role as "Pending trainee", and her institution, Kgosi Kgari
  Teaching Hospital. My activities reads "No activities yet" and "Log an activity to ask an assessor to rate an
  encounter, or to log a teaching session.", with Log an activity. Log an activity opens its instrument picker: the
  eleven instruments of Step 2.42, in its three groups. Her invitation's Paediatrics scope selects them, and with no
  curriculum yet no ladder narrows them. My progress shows "You cannot open this page" ("Your role (Pending trainee)
  does not open this page.") with Go to Home (T335).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The sidebar reads "Acting as Pending trainee" over Home, Log an
  activity and My activities, then My data rights, and nothing else. Review your account: My account, Institution Kgosi
  Kgari Teaching Hospital, Roles "Pending trainee". My activities: "No activities yet" and the quoted sentence, with
  Log an activity (and the header's). Log an activity: "Choose what you are filing. Each opens its own form." over
  three groups, "Rated by an assessor" 7 types, "Discussed or reviewed, not rated" 3, "Logged by you" 1: the eleven
  instruments of Step 2.42 as links to `/activities/new?type=<key>`; no MSF, Learner Feedback or Demo type. The typed
  My progress address went to /access-denied?ReturnUrl=…: "You cannot open this page", "Your role (Pending trainee)
  does not open this page.", Go to Home; nothing in the menu lit. Go to Home; the tab left on Home.
Gap: none

## Phase 2.F — The review panel

While the registrars' links age (Step 2.26 needs an hour), Mbatha forms the panel.

### Step 2.20 — Decision panels before any exist
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Open Decision panels.
Expect: The page reads "No decision panels" / "Create a panel before scheduling reviews." and offers New panel. "Who
  decides each EPA" names `Paediatric EPA Curriculum 11.1` and says against every EPA "No panel at this institution
  covers this programme."
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "No decision panels" / "Create a panel before scheduling
  reviews.", New panel offered. "Who decides each EPA" names Paediatric EPA Curriculum 11.1 in two rows (PAED-004,
  PAED-005; and the other thirteen), each "No panel at this institution covers this programme."
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Scope offered Institution and Speciality, on Speciality;
  Speciality offered Paediatrics with the quoted help; Decides for offered the two. Chair, Members and External members
  each listed Botha, Naidoo, van Rensburg and Zulu. Choosing Zulu as chair showed the T257 sentence and dropped her
  from Members. With Naidoo unselected, Save panel was refused with the D46 sentence; the page stayed at
  /committee/panels/new.
Gap: none

### Step 2.22 — Mbatha saves the panel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new → /committee/panels/{PanelId:int}
Do: Select Naidoo under Members and van Rensburg under External members, then save. Botha is left for Step 2.23.
Expect: The page reloads as the panel's own page. It has a "Decides for" card reading General panel with Save
  committee, and Update members shows Zulu as chair, Naidoo as a member and van Rensburg as external. The External
  members help text says external members sit with the chair on the appeal body and that cross-institution externals
  are not yet supported.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Saved to /committee/panels/1. The "Decides for" card reads
  General panel with Save committee; Update members shows Zulu chair, Naidoo member, van Rensburg external, and the
  External members help as expected. The heading reads "Decision panel": nowhere does the page name the panel, its
  scope or its speciality.
Gap: [F-2.22a, T260] (still) A panel's own page never shows its name (nor scope and speciality), so the admin cannot
  tell which panel she is editing; there is also no way to rename it there.

### Step 2.23 — Dr Mokoena adds Dr Botha to the panel
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/panels → /committee/panels/{PanelId:int} → /committee/panels
Do: Sign in, open Decision panels and Edit the panel. Select Botha under Members as well, then save and go back to the
  panels.
Expect: The list offers her New panel and an Edit on this panel, a Speciality-scoped panel in her speciality at her
  institution (T194). Decides for is read-only: "General panel", with "Only an institutional administrator can change
  which College committee a panel sits as." The save reads "Panel members updated.", and the list's Members column
  reads 4.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The list offered her New panel and "Edit Paed Annual Review
  Panel" (Members 3). Decides for was read-only, "General panel", with the quoted sentence and no select. Save panel
  read "Panel members updated." (members Botha and Naidoo, chair Zulu, external van Rensburg); back on the list,
  Members read 4.
Gap: none

### Step 2.24 — Who decides each EPA now
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Read the list and the routing card.
Expect: One row: Paed Annual Review Panel, Speciality, General panel, 4 members, with an Edit. PAED-004 and PAED-005 go to
  the panel with "No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the
  general panel decides." (T131). The other thirteen EPAs go to the panel.
Note: KGK has no neonatal CCC, so "Decides for" stays General panel. Act 4 schedules every review on this panel.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): One row: Paed Annual Review Panel, Speciality, General panel,
  4, "Edit Paed Annual Review Panel". Routing: PAED-004, PAED-005 → Paed Annual Review Panel with the T131 sentence as
  quoted; the other thirteen → Paed Annual Review Panel.
Gap: none

### Step 2.25 — Dr Sithole looks at what he may create
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/panels → /committee/panels/new
Do: Sign in, open Decision panels, press New panel and read the form. He leaves without saving.
Expect: The list offers him New panel and an Edit on the panel. The form's Scope offers Speciality only, because a
  sub-speciality administrator manages the Speciality-scoped panels of his sub-speciality's speciality (T194, T245).
  Speciality offers Paediatrics. There is no Decides for field. Nothing is created.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The list offered him New panel and "Edit Paed Annual Review
  Panel". The form's Scope offered Speciality only, Speciality offered Paediatrics, and there was no Decides for field.
  He went Back to panels without saving; still one panel.
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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Played a real hour after Step 2.16 (issued 13:16:50; Resend
  pressed 14:18). His row alone: "Not delivered. Resend emails a new link in place of the current one, which then stops
  working.", with Resend before Revoke, each named "... the Trainee invitation to duplessis@kgk.wombat.local". Resend
  gave "A new invitation link is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works.
  Copy the new link below — it is shown only once." with the new link under it; one new stub (invitation, role:Trainee)
  in the log. The row reads "Being sent", issued 2026-10-04 14:18, expiring 2026-10-18 (14 days), and offers only
  Revoke.
Gap: none

### Step 2.27 — Dr du Plessis registers from the new link
Role: Anonymous — Dr Pieter du Plessis
Route: /account/register → / → /account/logout/submit → /account/login
Do: Open the first link, then the resent one. Register from the resent link and sign out.
Expect: The first link shows no form, only "This invitation is invalid.": the resend replaced the link's hash, so the old
  token matches no invitation. The resent link registers him and lands on Home, "Pending trainee · Semester N, YYYY",
  with the Awaiting admission card. Signing out lands on the sign-in page with "You have signed out."
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The first link read "Complete registration" / "This invitation
  is invalid." with no form (no input). The resent link read "Registering duplessis@kgk.wombat.local as Trainee.",
  registered him and landed on Home, "Pending trainee · Semester 2, 2026", with the Awaiting admission card; signing
  out read "You have signed out.".
Gap: none (the token stayed in the address bar after load: F-2.8a, T315)

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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "No active invitations". Users lists 15, the five registrars as
  PendingTrainee. Molefe's page: Roles lists PendingTrainee with "System-managed" and no Remove; Add role offers
  exactly InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember and Assessor, with the
  T303 help under it (aria-describedby). Trainees: Pending admission lists the five with emails and KGK, each "Admit to
  curriculum: <name>"; "No active trainee profiles found."
Gap: [F-2.28b, T323] (still) The Roles row runs the role's name into its note, "PendingTraineeSystem-managed", on
  screen and as the list item's text.

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
Actual (2026-10-04, T355 replay, wombat_scenario_t355): "Trainee profile" / "Admit a pending trainee into a
  curriculum." (trail Home › Trainees › Trainee profile); Curriculum offered only "Paediatric EPA Curriculum (11.1)";
  start pre-filled 2026-10-04 (today); no training-year field. Each admission moved to `?id=` (Molefe 2, Dlamini 3, du
  Plessis 4, Mahlangu 5): Status Active, "Update curriculum and completion details for this trainee.", Last day in the
  programme, Deactivate, Mark complete. Active profiles lists the four with 11.1, Paediatrics and 2027-01-14,
  2028-01-14, 2029-01-14, 2030-01-14 (J = 2026-01-15).
Gap: [F-2.29a, T306] (still) Neither the admit form nor Active profiles shows the derived training year (or the start
  date).

### Step 2.30 — Dr Ndlovu's completion date is derived, then corrected
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit Ndlovu with start `J` and the completion date left empty. Then set completion to `J+4y−1d` on his profile and
  save the profile.
Expect: Left empty, the completion date is derived as `J` + 12 months, the longest item window on v11.1. The help text
  says so ("Leave this empty to derive it from the curriculum window."). The save reads "Trainee profile saved.", and
  the list shows `J+4y−1d`. Pending admission reads "No pending trainees found."
Note: A 12-month window for a four-year programme is T139's open College question.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Help text "Leave this empty to derive it from the curriculum
  window."; admitted with it empty (profile 6), it was derived as 2027-01-15 (`J` + 12 months). Set to 2030-01-14 and
  saved: "Trainee profile saved." (it takes the focus); the list shows 2030-01-14. Pending admission: "No pending
  trainees found."
Gap: none

### Step 2.31 — Dr Mahlangu's open session ends, and she signs back in as a Trainee
Role: Trainee — Dr Nomsa Mahlangu
Route: / → /account/session-ended → /account/login → /
Do: Her tab from Step 2.18 has stayed open through her admission. Once it leaves Home, she signs in again.
Expect: Within a minute of her admission the tab moves to the sign-in page, which reads "Your session has ended. Sign in
  again.", an information notice; the focus is on Email (no institution buttons on dev). Admission changes her role,
  and a role change ends open sessions (T279). Signed in again, she sees
  "Trainee · Semester N, YYYY" under Home's heading and the trainee dashboard of Step 2.39.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Her Step 2.18 tab, held open by one background browser from
  13:17 through the hour's wait and Steps 2.26-2.30, never reopened, sat on Home. 35 s after her admission (12:19:03
  UTC) it moved to /account/login?error=SessionEnded&returnUrl=%2F: "Your session has ended. Sign in again." as an info
  notice (role status), the focus on Email. Signed in again: Home, "Trainee · Semester 2, 2026", and the trainee
  dashboard of Step 2.39 (Your targets "Training year 1 — …", Needs you, Recent decisions, My authorisations); the
  sidebar adds MSF reports, My committee reviews, Export portfolio and My progress.
Gap: none

## Phase 2.I — First sign-in for every role

### Step 2.32 — Mr Smit, Coordinator
Role: Coordinator — Mr Pieter Smit
Route: /account/login → / → /admin/invitations → /access-denied → /committee/panels → /committee/panels/new → /access-denied
Do: Sign in and read the dashboard and nav. Type the invitations address. Open Decision panels from the menu, then type
  the new-panel address.
Expect: Home reads "Coordinator · Semester N, YYYY", with the header action "Start an MSF campaign"
  (`/msf/campaigns/new`), where a Quick action card was (T358, Q6). Its cards, in order:
  - "Waiting for assessors", with no badge: "Nothing is waiting for an assessor.", with Open Waiting for assessors;
  - "Nothing filed in 30 days", its rule line "Current registrars with nothing filed (a draft is not filed) in the last
    30 days. A registrar admitted less than 30 days ago is not listed.", then "Every current registrar has filed
    something in the last 30 days.", with Open in Programme trainees: the registrars were admitted today, and a
    registrar admitted less than 30 days ago is not listed (E5);
  - "Invitations nearing expiry": "No invitations expiring soon."
  The sidebar reads "Acting as Coordinator" over Home, Programme trainees, Waiting for assessors, Decisions due, MSF
  campaigns, Committee reviews, Decision panels and Data rights requests, one flat list (E2), then My data rights. It
  has no Invitations, and the invitations page reads "You cannot open this page", "Your role (Coordinator) does not open
  this page." and "If you need it for your work, ask your institution's Wombat administrator.", with Go to Home (T178,
  T335). Decision panels lists the panel with no New panel and no Edit column, and Decision panels is lit there (T358,
  C10). The panel form reads "You cannot open this page" in the same words: the Coordinator reads panels, and edits none
  (D4).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Home, "Coordinator · Semester 2, 2026": "No stalled requests.",
  "No invitations expiring soon." and Quick action "Start an MSF campaign". The sidebar reads "Acting as Coordinator"
  over exactly the five named, then My data rights. Typed Invitations: /access-denied, "You cannot open this page", the
  two sentences as quoted, Go to Home. Typed /committee/panels: the panel (Speciality, General panel, 4) and the
  Who-decides card, no New panel, no Actions column, nothing in the sidebar lit (only the trail's last crumb is
  aria-current). The typed panel form: "You cannot open this page" in the same words.
Gap: none

### Step 2.33 — Dr Zulu's first view: committee member
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: /account/login → /
Do: Sign in and read the dashboard and nav.
Expect: Home reads "Committee member · Semester N, YYYY" under its heading. The sidebar reads "Acting as Committee
  member", with "Switch to Assessor" under it: Dr Zulu holds both, and the precedence opens Dr Zulu's sessions as a
  Committee member. The cards (T358, Q1, Q5):
  - "Registrars", badged "5 registrars", under the rule line "Fewest met first, then by surname.": Anele Dlamini, Pieter
    du Plessis, Nomsa Mahlangu, Lerato Molefe and Sipho Ndlovu, all tied at nothing met, so by surname, then first name
    (T298). Each name is a link to the registrar's page, over the training year ("Training year 3"), then "0 of 10" over
    "EPAs met this semester" and "0 of 5" over "EPAs met in YYYY". Nobody is exempt: a 15 January start counts from the
    boundary (D42). Its foot is Open Programme trainees.
  - "Targets by EPA", under "Fewest registrars met first. Semester N, YYYY ends on <the semester's last day, ISO>.":
    PAED-001 to PAED-015 in code order, each EPA's name a link to Programme trainees filtered Short on it, over its
    cadence ("3 per semester", "1 per academic year"), then "0 of 5" over "registrars met this semester" or "registrars
    met in YYYY" (T130's count, never a percentage). It has no foot.
  The menu is the Committee member's alone, never the union of Dr Zulu's roles: Home, Programme trainees, Committee
  reviews and Decision panels, then My data rights.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Home, "Committee member · Semester 2, 2026"; the sidebar
  "Acting as Committee member" with "Switch to Assessor", over Home, Committee reviews and Decision panels, then My
  data rights. "Semester 2, 2026 · July to November"; five registrars at "semester 0/10 · yearly 0/5" (in the order du
  Plessis, Dlamini, Mahlangu, Ndlovu, Molefe), no exempt line; PAED-001 to 015 each "0 of 5 met".
Gap: [F-2.33a, T298] (still) With every trainee level, the Targets card lists them in user-id order (the tie-break
  after "fewest met first"), which no reader can follow; a name tie-break would read.

### Step 2.34 — Dr Zulu switches between her dashboards
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /account/logout/submit → /account/login → / → /dashboard/switch/{role} → / → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, and reload the page. Sign out and in again. Then type
  `/dashboard/switch/Administrator`. Last, choose Switch to Committee member in the sidebar.
Expect: Under the header an info alert reads "You are now acting as Assessor." and takes the focus; on Home it offers no
  Switch back. The sidebar reads "Acting as Assessor", with "Switch to Committee member" under it, and the menu changes
  to the Assessor's: Home and Activity inbox, then My data rights. Home's subtitle reads "Assessor · Semester N, YYYY".
  The Assessor dashboard has two cards: "Waiting for you", with no badge, which reads "Nothing is waiting for you." with
  "Open Activity inbox" (T297, T335, T350), and Recent decisions, "No decisions yet.", with no "All your decisions".
  Nothing waits in her inbox, so the Committee member's Home shows no line about it (T350). The reload shows no alert:
  it is said once. Signing out lands on the sign-in page with "You have signed out." After signing in again she lands
  acting as Assessor, with no alert, because the choice is stored with her account, not in the browser (T317). The
  Administrator address writes nothing and says nothing: she is still acting as Assessor, since a role she does not hold
  is never shown or stored. Switch to Committee member brings back the Committee member's sidebar and menu, "Committee
  member · Semester N, YYYY" under Home's heading, and the alert "You are now acting as Committee member.", the
  role Step 3.33 starts from.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Switch to Assessor: Home, "Assessor · Semester 2, 2026", "You
  are now acting as Assessor." with the focus, no Switch back; the sidebar "Acting as Assessor" / "Switch to Committee
  member" over Home and Activity inbox, then My data rights; "Waiting for you" with no badge, "Nothing is waiting for
  you.", "Open Activity inbox" (/activities/inbox), and Recent decisions "No decisions yet." with no "All your
  decisions". The reload: no alert. Signed out to "You have signed out."; Naidoo, signing in next in the same browser,
  landed as Committee member. Zulu, signed in again in a fresh browser: Assessor, no alert.
  /dashboard/switch/Administrator: back on Home, still Assessor, nothing said. Switch to Committee member: its sidebar,
  menu and subtitle, and "You are now acting as Committee member." with the focus; her Committee member Home has no
  line about the inbox.
Gap: none

### Step 2.35 — Dr Naidoo and Dr Botha
Role: CommitteeMember + Assessor — Dr David Naidoo and Dr Sarah Botha
Route: /account/login → / → /committee/panels
Do: Each signs in, reads the dashboard and opens Decision panels.
Expect: Dr Naidoo and Dr Botha each see what Dr Zulu saw in Step 2.33: the same menu, the five registrars on Registrars
  in its order, and "0 of 5" for every EPA on Targets by EPA. Decision panels lists the panel both sit on, with no New
  panel and no Edit.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Each: Home, "Committee member · Semester 2, 2026", "Acting as
  Committee member" with "Switch to Assessor", Zulu's menu, the same five registrars at 0/10 · 0/5 and PAED-001 to 015
  at "0 of 5 met". Decision panels lists Paed Annual Review Panel (Speciality, General panel, 4) and the Who-decides
  card, with no New panel and no Edit (no action in main).
Gap: [F-2.33a, T298] (still) The same user-id order on the Targets card as Zulu's.

### Step 2.36 — Dr Patel and Dr Khumalo, assessors
Role: Assessor — Dr Mohammed Patel and Dr Fatima Khumalo
Route: /account/login → /
Do: Each signs in and reads the dashboard and nav.
Expect: Each sees "Assessor · Semester N, YYYY" on Home, and the empty Assessor dashboard of Step 2.34: "Waiting for
  you" with no badge and no "0", "Nothing is waiting for you.", "Open Activity inbox"; Recent decisions, "No decisions
  yet." (T350). The sidebar reads "Acting as Assessor" with no switch, over Home and Activity inbox, then My data
  rights.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Each: Home, "Assessor · Semester 2, 2026"; the sidebar "Acting
  as Assessor" with no switch, over Home and Activity inbox, then My data rights; "Waiting for you" with no badge and
  no "0", "Nothing is waiting for you.", "Open Activity inbox"; Recent decisions "No decisions yet.".
Gap: none

### Step 2.37 — Dr van Rensburg, external committee member
Role: CommitteeMember — Dr John van Rensburg
Route: /account/login → / → /committee/panels
Do: Sign in, read the dashboard and open Decision panels.
Expect: Dr van Rensburg sees "Committee member · Semester N, YYYY" on Home, and no switch: Dr van Rensburg holds one
  role. "Registrars", badged "5 registrars", lists KGK's five current registrars in Step 2.33's order, and "Targets by
  EPA" reads "0 of 5" for each EPA: a committee member reads every current registrar at the member's institution,
  whatever the member's own sub-specialities, and Dr van Rensburg holds none (T290). The sidebar reads "Acting as
  Committee member" with no switch, over Home, Programme trainees, Committee reviews and Decision panels, then My data
  rights. Decision panels lists the panel Dr van Rensburg sits on as external member, with no New panel and no Edit.
Note: Zulu, Naidoo and Botha hold the Paediatrics sub-speciality only through their assessor profiles (Step 2.14), and a
  CommitteeMember invitation may carry none (Step 2.3). So a committee member with no assessor profile sees no trainee
  here, although he sits on the panel and may read every KGK trainee's record (T113). Fixed by T290 (2026-10-05):
  the card no longer reads sub-speciality claims.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Home, "Committee member · Semester 2, 2026", "Acting as
  Committee member" with no switch, over Home, Committee reviews and Decision panels, then My data rights. "Semester 2,
  2026 · July to November", "No trainees have targets this period.", "No curriculum targets for these trainees.".
  Decision panels as Naidoo saw them, with no New panel and no Edit.
Gap: [F-2.37a, T290] (still) The panel's external member, who may read every KGK trainee (T113), sees an empty
  committee dashboard, because the card filters on sub-speciality scope he cannot hold.

### Step 2.38 — Dr Mokoena's and Dr Sithole's dashboards
Role: SpecialityAdmin and SubSpecialityAdmin — Dr Refilwe Mokoena and Dr Kabelo Sithole
Route: /
Do: Each opens Home, now that the registrars are admitted.
Expect: Each sees "Speciality admin · Semester N, YYYY" or "Sub-speciality admin · Semester N, YYYY" on Home. The cards,
  read for Paediatrics (T358, Q3): "Waiting for assessors", with no badge, reads "Nothing is waiting for an assessor.",
  with Open Waiting for assessors; "Registrars" lists the five as Step 2.33's card does, badged "5 registrars", with
  Open Programme trainees; "Targets by EPA" lists PAED-001 to PAED-015, each "0 of 5". There is no Pending reviews, no
  Trainees in programme and no "inactive" anywhere (Q10). The sidebar reads "Acting as Speciality admin" or "Acting as
  Sub-speciality admin", over Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews,
  Decision panels and Entrustment decisions, then My data rights.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Both as expected: "Speciality admin · Semester 2, 2026" and
  "Sub-speciality admin · …"; Pending reviews 0 "activities awaiting review" with no link (no action in main); Trainees
  in programme 5 "active / 0 inactive"; "Curriculum coverage — Semester 2, 2026" over "Semester 2, 2026 · July to
  November", PAED-001 to 015 at "0 of 5 met". The sidebar "Acting as Speciality admin" or "Acting as Sub-speciality
  admin" over Home, Decisions due, Committee reviews and Decision panels, then My data rights.
Gap: none

### Step 2.39 — Dr Molefe, final-year registrar
Role: Trainee — Dr Lerato Molefe
Route: /account/login → / → /portfolio/progress
Do: Sign in, read the dashboard and nav, and open My progress.
Expect: Home's first card, Your targets, reads "Training year 4 — it sets the minimum level each encounter is judged
  against." (T306), then "0 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in 2026", never "0 / 10"
  (T355). Under "Furthest short" are PAED-001 to PAED-005, each its name as its own link to its EPA's page, with "0 of 3
  this semester · 3 more by 2026-11-30" under it; there is no "started part-way" line (D42), and the card ends with Open
  My progress. The other cards read: Needs you, "Nothing needs you. Requests you have filed are in My activities.", with
  Open My activities (T342); Recent decisions, "No decisions yet."; and My authorisations, "No STAR yet. When the
  committee issues one, it shows here against training year 4's level.", with Open My authorisations. There is no Recent
  activities card and no Upcoming deadlines card (T355). The header offers "Log an activity" (T335). The sidebar reads
  "Acting as Trainee" over Home, Log an activity, My activities, MSF reports, My committee reviews and Export portfolio,
  then My progress and My data rights under the rule. My progress's subtitle reads "Training year 4 · Semester 2, 2026".
  Its This period reads "0 of 10" over "EPAs met this semester", "0 of 5" over "EPAs met in 2026", "Semester 2, 2026
  ends on 2026-11-30.", "Training year 4 — it sets the minimum level each encounter is judged against." and
  "Multi-source feedback: 0 of 15 EPAs covered by a released campaign that closed this semester. …". Your EPAs lists
  "Each semester · 10 EPAs" and "Once a year · 5 EPAs", every count 0 and every STAR "No decision"; Entrustment against
  Annexure A reads "0 at or above · 0 below · 15 with no decision, of 15 EPAs".
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Home, "Trainee · Semester 2, 2026", header action "Log an
  activity". Your targets: "Training year 4 — it sets the minimum level each encounter is judged against.", "0 of 10"
  over "EPAs met this semester" and "0 of 5" over "EPAs met in 2026"; Furthest short PAED-001 to 005, each its name as
  a link to /portfolio/progress/{EpaId} (2 to 6) with "0 of 3 this semester · 3 more by 2026-11-30", no part-way line;
  Open My progress. Needs you "Nothing needs you. Requests you have filed are in My activities." with Open My
  activities; Recent decisions "No decisions yet."; My authorisations "No STAR yet. When the committee issues one, it
  shows here against training year 4's level." with Open My authorisations. No Recent activities or Upcoming deadlines
  card. The sidebar reads "Acting as Trainee" over the six named, then My progress and My data rights under the rule.
  My progress (lit): "Training year 4 · Semester 2, 2026"; This period "0 of 10" / "EPAs met this semester", "0 of 5" /
  "EPAs met in 2026", "Semester 2, 2026 ends on 2026-11-30.", the training-year sentence and "Multi-source feedback: 0
  of 15 EPAs covered by a released campaign that closed this semester. …"; Your EPAs "Each semester · 10 EPAs" and
  "Once a year · 5 EPAs", every count 0 and every STAR "No decision"; Entrustment against Annexure A "0 at or above · 0
  below · 15 with no decision, of 15 EPAs".
Gap: none (T306 no longer occurs: Home's Your targets names training year 4)

### Step 2.40 — The other registrars' training years
Role: Trainee — Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu
Route: /account/login → / → /portfolio/progress
Do: Each signs in and opens My progress.
Expect: Each dashboard has Molefe's shape and figures, with its own training year: Your targets opens "Training year N —
  it sets the minimum level each encounter is judged against.", as My progress does (T306), My authorisations reads "No
  STAR yet. When the committee issues one, it shows here against training year N's level.", and My progress's subtitle
  reads "Training year N · Semester 2, 2026". N is 3 for Dlamini, 2 for du Plessis and 1 for Mahlangu and Ndlovu. Their
  semester and yearly targets apply now: a start on 15 January is on time for both (D42).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Each dashboard has Molefe's shape and figures ("0 of 10", "0 of
  5" in 2026, PAED-001 to 005 "0 of 3 this semester", no part-way line, the same Needs you card), with its own year:
  Your targets opens "Training year N — it sets the minimum level each encounter is judged against.", My authorisations
  "No STAR yet. When the committee issues one, it shows here against training year N's level.", and My progress's
  subtitle "Training year N · Semester 2, 2026": N = 3 (Dlamini), 2 (du Plessis), 1 (Mahlangu), 1 (Ndlovu), right for
  2026-10-04.
Gap: none

### Step 2.41 — Everyone reviews their account; Dr Khumalo corrects her name
Role: Every role in this act — each person onboarded here, signed in as themselves
Route: /account/profile → /account/profile/submit → /account/profile
Do: Each opens My account (the name in the top bar). Dr Khumalo changes her first name from "Fatma" to "Fatima" and
  saves her name.
Expect: The page reads "My account" and "Your name, your roles, and how you sign in." The Account card shows the email
  as text, the institution (Kgosi Kgari Teaching Hospital) and the roles by label, one per line ("Committee member" and
  "Assessor" for Zulu, Naidoo and Botha; "Trainee" for the registrars). How you sign in shows Password with Change
  password. Khumalo's Save name reloads the page with "Name saved." in the Your name card, which takes the focus, and
  the top bar's account row now reads "Fatima Khumalo" (the lists are checked on Mbatha's in Step 2.44).
Actual (2026-10-04, T355 replay, wombat_scenario_t355): All 14 opened My account from the top bar's name (then marked
  aria-current page): "My account" and "Your name, your roles, and how you sign in."; the Account card shows the email
  as text (no input), Kgosi Kgari Teaching Hospital and the roles by label, one per line: "Committee member" and
  "Assessor" for Zulu, Naidoo and Botha, "Trainee" for the five, each other its one label ("Coordinator", "Speciality
  admin", "Sub-speciality admin", "Assessor", "Committee member"); How you sign in shows Password with Change password.
  Khumalo: Fatma → Fatima, Save name: back at ?status=saved with "Name saved." in the Your name card, focused; the top
  bar reads "Fatima Khumalo". At 390 px no sideways scroll (scroll width 390).
Gap: none

### Step 2.42 — The instruments a KGK registrar is offered
Role: Trainee — Dr Lerato Molefe
Route: /activities/new → /activities/new?type=mini_cex_cpsa
Do: Open Log an activity and read its instrument picker. Open each type in turn to see what it rates, going back by
  Choose another type, and save nothing.
Expect: The page is headed "Log an activity", with "Choose what you are filing. Each opens its own form." under it, and
  lists eleven types as links, no name twice, in three groups (T342, Q1). Each link opens its own form,
  `/activities/new?type=<key>`, and the form's subtitle names the type with Choose another type, which leads back.
  - **Rated by an assessor**, "7 types": "You name an assessor. They rate the encounter on 1, 2, 3a, 3b, 4, 5 and
    write feedback. Completed, it counts towards the EPA." Case-Based Discussion, Chart-Stimulated Recall, Clinical
    Case Analysis, Direct Observation, DOPS, Mini-CEX and Random Case Analysis, each "(Paediatrics)". Each of the seven
    rates on `1 2 3a 3b 4 5`.
  - **Discussed or reviewed, not rated**, "3 types": "You name a supervisor or reviewer, who discusses or reviews it
    with you. No rating." Clinical Audit, Portfolio and Logbook Review and Reflective Exercise, each "(Paediatrics)".
  - **Logged by you**, "1 type": "Only you fill it in. It is logged at once, and credits nothing." `KGK Teaching
    Session Log`.
  Multi-Source Feedback and Learner Feedback (Paediatrics) are not offered, because only the system writes them (T162,
  T164). Nor are the Demo types, which belong to another discipline.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Headed "Log an activity" with "Choose what you are filing. Each
  opens its own form."; eleven links, no name twice, in three groups: "Rated by an assessor" "7 types" with the quoted
  sentence and the seven named, each "(Paediatrics)"; "Discussed or reviewed, not rated" "3 types" with its sentence
  and the three; "Logged by you" "1 type" with its sentence and KGK Teaching Session Log. Each link opened
  `/activities/new?type=<key>` with Choose another type, which led back to /activities/new every time. The seven rated
  forms each show the ladder `1 2 3a 3b 4 5`; Audit, Portfolio Review, Reflective and the Teaching Log show none. No
  MSF, Learner Feedback or Demo type. None saved.
Gap: none

### Step 2.43 — The Mini-CEX link and whom it may name
Role: Trainee — Dr Lerato Molefe
Route: /activities/new?type=mini_cex_cpsa
Do: Open `/activities/new?type=mini_cex_cpsa`. Read the EPA and Assessor pickers, then leave without saving.
Expect: The Mini-CEX (Paediatrics) form opens at once, its name under the heading with Choose another type. Request is
  open. Entrustment and Feedback are locked, each reading "The assessor you name fills this in" until an assessor is
  named. Above the actions: "When you submit: it goes to the Activity inbox of the assessor you name, and stays
  Requested until that assessor acts on it." The actions read Submit first, then Save draft, with "Save draft keeps it
  in My activities. It is in nobody's inbox until you submit it." under them. The EPA picker offers the nine EPAs whose
  Annexure A list names the Mini-CEX: PAED-001, 002, 003, 004, 006, 007, 008, 012 and 013 (T122). The Assessor picker
  offers exactly Botha, Khumalo, Naidoo, Patel and Zulu, each with their email. It does not offer van Rensburg, Molefe
  herself or any Demo account (T102). Nothing is saved.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): The Mini-CEX form opened at once, subtitled "Mini-CEX
  (Paediatrics) · Choose another type". Request is open ("You fill this in"); Entrustment (the ladder) and Feedback
  each read "The assessor you name fills this in" and "Not filled in yet.". Above the actions, "When you submit: it
  goes to the Activity inbox of the assessor you name, and stays Requested until that assessor acts on it."; Submit,
  then Save draft, with "Save draft keeps it in My activities. It is in nobody's inbox until you submit it." under
  them. EPA: PAED-001, 002, 003, 004, 006, 007, 008, 012, 013. Assessor: Botha, Khumalo (as Fatima), Naidoo, Patel,
  Zulu, each "(email)"; nobody else. Left unsaved (SQL: no KGK activity).
Gap: none

### Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/assessors
Do: Open Home. Then open Users and Assessors.
Expect: Home reads "Institutional admin · Semester N, YYYY" and offers "Invite a person". Users reads InstitutionalAdmin
  1, SpecialityAdmin 1, SubSpecialityAdmin 1, Coordinator 1, CommitteeMember 4, Assessor 5 and Trainee 5, with no
  PendingTrainee line.
  Specialities & sub-specialities reads 1 and 1, what KGK has adopted. Quick links reads Users, Invitations, Curriculum
  adoptions and Entrustment decisions. The users and assessors lists both name Fatima Khumalo (Step 2.41).
Note: The adopted count is T291 item 4's decision, still queued: the query counts the whole national catalogue, which
  with the Demo world reads 2 and 2, so expect a Gap until T291 lands.
Actual (2026-10-04, T355 replay, wombat_scenario_t355): Home, "Institutional admin · Semester 2, 2026", header action
  "Invite a person"; Users 1/1/1/1/4/5/5, no PendingTrainee line; Specialities & sub-specialities reads 2 and 2; Quick
  links Users, Invitations, Curriculum adoptions and Entrustment decisions. Users lists 15 KGK accounts and Assessors
  five profiles, both naming Fatima Khumalo.
Gap: [F-2.44a, T291] (still) Specialities & sub-specialities reads 2 and 2 (the whole national catalogue with the Demo
  world), not KGK's adopted 1 and 1: T291 item 4, still queued.

## Act 2 outcome state

Replay check (2026-10-04, T355 replay, wombat_scenario_t355): match but one; all 7 queries as expected except
invitations, 15 | 2 | 0: act 1's script issued Prof Mbatha a second invitation and revoked the first, so one more
revoked row than the runbook's (not a finding). Activities and reviews 0 | 0; the assessor dates are D−8y, D−6y and D−1
month from 2026-10-04.

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
