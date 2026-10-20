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
The stub is tagged by kind only (`invitation`; since T373 no role) and does not name the address (T282): its text says
whom it invites, as what ("You are invited to Wombat as Coordinator at Kgosi Kgari Teaching Hospital."). Invite a
person shows each link once, when it is issued, and Invitations a resent one. Copy it then, and use the log to confirm
that the email was written. Nothing reports a stub email as delivered. So an invitation's Delivery column reads "Being
sent" for its first hour and "Not delivered" after that, with a note row under it (T283, `LoggingEmailSender`).
Against a mail server that accepts the email, it reads "Sent". Since T373 each admission mails the registrar too ("You
are admitted in Wombat", Step 2.29), so the log holds five more stubs after this act than before T373.

## Phase 2.A — Staff invitations

### Step 2.1 — Prof Mbatha opens the invitations page
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /admin/invitations → /admin/invitations/new
Do: Sign in and open Invitations from the nav, then press Invite a person. Read what the form offers before issuing
  anything.
Expect: Active invitations is empty: "No active invitations". Kruger's College admin invitation is the College's, not
  KGK's, and Mbatha's own has been used. Invite a person reads "* marks a field you must fill in." Role offers seven
  radios by label, none chosen: Institutional admin, Speciality admin, Sub-speciality admin, Coordinator, Committee
  member, Assessor and Trainee, each with its description; not College admin, which only an Administrator issues
  (T093). Institution is read out, not offered: "Kgosi Kgari Teaching Hospital" (S18). No speciality field is drawn
  until a role that takes one is chosen.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signed in to Home; Invitations from the nav: "No active
  invitations" / "Issue an invitation to create the first onboarding link.". Invite a person opened
  /admin/invitations/new (Home › Invitations › Invite a person): "* marks a field you must fill in.", Email *, Role *
  with seven radios, none chosen: Institutional admin, Speciality admin, Sub-speciality admin, Coordinator, Committee
  member, Assessor and Trainee, each with its description; no College admin. Institution read out, "Kgosi Kgari
  Teaching Hospital", no select; no Speciality field drawn.
Gap: none

### Step 2.2 — Mbatha invites Mr Smit as Coordinator
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue an invitation to `smit@kgk.wombat.local` as Coordinator at KGK, with the speciality left at Any speciality.
  Press Copy link. Stay on the page for the next issue.
Expect: Coordinator draws Speciality with no star, at "Any speciality" (offering Paediatrics), with "Leave it at Any
  speciality for the whole institution."; no Sub-speciality field (T060). The result, focused, reads "**Invitation
  issued for smit@kgk.wombat.local.** Its email is being sent. Copy the link below — it is shown only once.", then the
  link with Copy link and "It works until `D+14`. …" as in Step 1.5, and Open Invitations. Copy link says "Link
  copied.". The form is drawn again, empty, under "Invite the next person". The log holds a stub "Your Wombat
  invitation", tagged `invitation`, reading "You are invited to Wombat as Coordinator at Kgosi Kgari Teaching
  Hospital." His row, read at Step 2.7, is "Coordinator" over "Any speciality", Issued `D` over its time in SAST, Last
  day `D+14`, Delivery "Being sent".
Note: One invitation from Home's Invite a person is 4 presses and 1 field: Invite a person, Coordinator, Issue
  invitation and Copy link; the address.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Coordinator drew Speciality, no star, at "Any speciality"
  (offering Paediatrics) with "Leave it at Any speciality for the whole institution."; no Sub-speciality. The result,
  focused: "Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown
  only once.", the link with Copy link, "It works until 2026-11-03. It is shown here until your next action, then
  never again. …" and Open Invitations. Copy link: "Link copied." (the clipboard held the link). The form came back
  empty, no role chosen, under "Invite the next person". Stub "Your Wombat invitation" (tags: invitation): "You are
  invited to Wombat as Coordinator at Kgosi Kgari Teaching Hospital." Row (2.7): Coordinator / Any speciality,
  2026-10-20 11:15 SAST, 2026-11-03, "Being sent".
Gap: none

### Step 2.3 — Mbatha invites Dr Zulu, Dr Naidoo and Dr Botha as committee members
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue one invitation each to `zulu@`, `naidoo@` and `botha@kgk.wombat.local` as Committee member at KGK, with
  speciality Paediatrics. Copy each link before issuing the next.
Expect: Committee member draws Speciality with no star, offering Paediatrics alone: KGK's adopted specialities (T092).
  No Sub-speciality field is drawn, because a committee member may not be scoped to one. Each issue reads as Step
  2.2's, and each stub reads "You are invited to Wombat as Committee member in Paediatrics at Kgosi Kgari Teaching
  Hospital." Their rows (Step 2.7) read "Committee member" over "Paediatrics".
Note: An invitation carries one role. Assessor is added on each user's page in Step 2.13 (T061).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Committee member drew Speciality, no star, at Any speciality
  offering Paediatrics alone; no Sub-speciality field. Each issue read as 2.2's for its address, focused, Copy link
  "Link copied." each time; three stubs "You are invited to Wombat as Committee member in Paediatrics at Kgosi Kgari
  Teaching Hospital."; rows "Committee member" over "Paediatrics".
Gap: none

### Step 2.4 — Mbatha invites Dr Patel and Dr Khumalo as assessors
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue an invitation each to `patel@` and `khumalo@kgk.wombat.local` as Assessor at KGK, with speciality Paediatrics
  and sub-speciality Paediatrics.
Expect: For an Assessor, Speciality is starred, with "Then choose its sub-speciality." under it until the
  sub-speciality is chosen. Once Paediatrics is chosen, Sub-speciality is drawn, starred, offering that speciality's
  sub-speciality, Paediatrics, and the hint stays; it goes once Paediatrics is chosen as the sub-speciality. Each stub reads "You are invited to Wombat as Assessor in Paediatrics / Paediatrics at Kgosi Kgari
  Teaching Hospital." Their rows (Step 2.7) read "Assessor" over "Paediatrics / Paediatrics".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Assessor drew Speciality * at "Choose a speciality" with "Then
  choose its sub-speciality."; with Paediatrics chosen, Sub-speciality * appeared ("Choose a sub-speciality",
  Paediatrics) and the hint stayed; it went once the sub-speciality was chosen. Two stubs "You are invited to Wombat
  as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital."; rows "Assessor" over "Paediatrics /
  Paediatrics".
Gap: none

### Step 2.5 — Mbatha invites Dr van Rensburg as an external committee member
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue an invitation to `vanrensburg@sun.wombat.local` as Committee member at KGK, with the speciality left at Any
  speciality.
Expect: It is issued with no speciality (T060). Its stub reads "You are invited to Wombat as Committee member at Kgosi
  Kgari Teaching Hospital.", and its row (Step 2.7) reads "Committee member" over "Any speciality". The address's
  domain does not matter: the invitation scopes him to KGK.
Note: A panel seats only committee members at its own institution (T165), so the Stellenbosch examiner is invited at KGK.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Issued with Speciality left at Any speciality: "Invitation
  issued for vanrensburg@sun.wombat.local. …"; stub "You are invited to Wombat as Committee member at Kgosi Kgari
  Teaching Hospital."; row "Committee member" over "Any speciality", last day 2026-11-03.
Gap: none

### Step 2.6 — Mbatha invites Dr Mokoena as the programme's SpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue `mokoena@kgk.wombat.local` as Speciality admin at KGK with no speciality chosen. Then choose Paediatrics and
  issue it again.
Expect: Speciality admin draws Speciality, starred, at "Choose a speciality", and no Sub-speciality. The first press is
  refused under Speciality: "Speciality administrators must be scoped to a speciality." (T060), the field marked and
  focused. Nothing is issued, and no registration link is on the page: van Rensburg's, from Step 2.5, went when the
  press began (T264, M5). The second is issued; its stub reads "… as Speciality admin in Paediatrics at Kgosi Kgari
  Teaching Hospital.", and its row (Step 2.7) reads "Speciality admin" over "Paediatrics".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Speciality admin drew Speciality * at "Choose a speciality",
  no Sub-speciality. The first press: "Speciality administrators must be scoped to a speciality." under Speciality,
  the select aria-invalid, described by it and focused; no result box, no row, and no registration link on the page
  (van Rensburg's went). With Paediatrics chosen the second issued (focused result); stub "… as Speciality admin in
  Paediatrics at Kgosi Kgari Teaching Hospital."; row "Speciality admin" over "Paediatrics".
Gap: none (T264 no longer occurs: the refused issue left no registration link on screen)

### Step 2.7 — Mbatha invites Dr Sithole as the SubSpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new → /admin/invitations
Do: Issue `sithole@kgk.wombat.local` as Sub-speciality admin at KGK with speciality Paediatrics and no sub-speciality.
  Then choose sub-speciality Paediatrics and issue it again. Copy the link, then press Open Invitations.
Expect: The first is refused under Sub-speciality: "The selected role requires speciality and sub-speciality scope.",
  focused; no registration link is on the page. The second is issued. Active invitations reads "9 active", captioned
  "At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this
  list; one that passes its last day unused moves to Expired, not used." Its nine rows, newest first:
  - Sithole, "Sub-speciality admin" over "Paediatrics / Paediatrics"; Mokoena, "Speciality admin" over "Paediatrics";
  - van Rensburg, "Committee member" over "Any speciality"; Khumalo and Patel, "Assessor" over "Paediatrics /
    Paediatrics";
  - Botha, Naidoo and Zulu, "Committee member" over "Paediatrics"; Smit, "Coordinator" over "Any speciality".
  Each reads Issued `D` over its time in SAST, Last day `D+14` and Delivery "Being sent", with Revoke… named for its
  row ("Revoke the Committee member invitation to zulu@kgk.wombat.local"). There is no Expired, not used section. The
  log holds 11 invitation stubs (Act 1's two and these nine), each tagged `invitation`.
Note: The nine in a row, from Home's Invite a person with both refusals, are 40 presses and 9 fields (counting a Copy
  link for each, and not Open Invitations), the same at 1280 and 390. "Invite the next person" is the reset form's heading, not a press.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The first press: "The selected role requires speciality and
  sub-speciality scope." under Sub-speciality, marked and focused; no link on the page. The second issued; Copy link
  "Link copied."; Open Invitations: no link on the list. "9 active", captioned as quoted; nine rows newest first,
  Sithole (Sub-speciality admin / Paediatrics / Paediatrics), Mokoena (Speciality admin / Paediatrics), van Rensburg
  (Committee member / Any speciality), Khumalo, Patel (Assessor / Paediatrics / Paediatrics), Botha, Naidoo, Zulu
  (Committee member / Paediatrics), Smit (Coordinator / Any speciality), each Issued 2026-10-20 over 11:15 or 11:16
  SAST, Last day 2026-11-03, "Being sent", Revoke… named per row ("Revoke the Committee member invitation to
  zulu@kgk.wombat.local"); no Expired, not used section. 11 invitation stubs, each tagged invitation. At 901, 1000 and
  1100 px, and 390 px, no sideways scroll (scrollWidth = width) and no word in any cell broke mid-word; at 390 Invite
  a person is 44 px tall.
Gap: none (T264 no longer occurs: no link under the refusal)

## Phase 2.B — The staff register

### Step 2.8 — Mr Smit registers from his link
Role: Anonymous — Mr Pieter Smit, holding his invitation link
Route: /account/register/start → /account/register → / → /account/logout/submit → /account/login
Do: Open the link and enter first name Pieter, last name Smit and a password that meets the rules. Confirm it and
  register. Read the landing page, then sign out.
Expect: The link's redirect leaves `/account/register` in the address bar, with no token (T315). The page reads "This
  invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital.", Email read-only with
  its help, and under the form "Not expecting this invitation? Ignore it: its link works until `D+14`."; Register is
  in the form's actions. Registering signs him in and lands on Home, headed "Home" with "Coordinator · Semester N,
  YYYY" under it. Signing out (the top bar's Sign out, one press) returns him to the sign-in page, which reads "You
  have signed out."
Note: Record every password chosen in this act in `pwd_DO_NOT_COMMIT.txt` only (README § Passwords). Registering is 1
  press and 4 fields.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The link answered 303 to /account/register (200); the address
  bar read /account/register with no query (3 s later too) and the page source held no token. "Complete registration":
  "This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital.", Email read-only
  with "The address this invitation is for. It cannot be changed here.", Register in the actions and "Not expecting
  this invitation? Ignore it: its link works until 2026-11-03." under the form. Registering landed on Home, "Home" /
  "Coordinator · Semester 2, 2026", the top bar "Pieter Smit". The top bar's Sign out (one press):
  /account/login?error=SignedOut, "You have signed out.".
Gap: none (T315 no longer occurs: no token in the address bar or the page source)

### Step 2.9 — Dr Patel's first two attempts are refused
Role: Anonymous — Dr Mohammed Patel, holding his invitation link
Route: /account/register/start → /account/register → / → /account/logout/submit → /account/login
Do: Register with a 10-character password that mixes upper case, lower case, a digit and a symbol. Then use a
  12-character password with a different confirmation. Then use a 12-character password confirmed correctly. Sign out.
Expect: Before anything is typed, the six rules stand under Password: "The new password needs:" and "At least 12
  characters.", "At least 4 different characters.", "A digit (0 to 9).", "An upper-case letter.", "A lower-case
  letter.", "A symbol, such as ! or #."; each password field has its Show toggle. The first attempt returns to the
  form with "The new password needs: At least 12 characters." under Password, which is marked and takes the focus;
  First name and Last name keep what he typed (T317), and no account is created (T285). The second returns with "The
  password confirmation does not match. Type the password again in both fields." under Confirm password, both
  password fields empty and the focus on Password. The address bar reads `/account/register` throughout: the refusals
  travel in the register cookie, not the address (T341). The third registers him and lands on Home, "Assessor ·
  Semester N, YYYY". Signing out lands on the sign-in page with "You have signed out."
Note: The Do's two refusals and then the success are 3 presses and 8 fields: the names are kept, so only the two
  passwords are typed again after each refusal (one refusal and then the success would be 2 presses and 6 fields).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Before typing, "The new password needs:" and the six rules
  word for word under Password; toggles "Show password" and "Show confirm password". 10 characters: back on
  /account/register with "The new password needs: At least 12 characters." under Password (aria-invalid, focused),
  First name "Mohammed" and Last name "Patel" kept. Mismatch: "The password confirmation does not match. Type the
  password again in both fields." under Confirm password, both password fields empty, focus on Password, the names
  kept. The address read /account/register throughout. The third (same invitation, so the refusals made no account)
  landed on Home, "Assessor · Semester 2, 2026"; signing out read "You have signed out.".
Gap: none (T317 no longer occurs: the names are kept; the register half of T341 no longer occurs: no refusal in the
  address)

### Step 2.10 — The other seven staff register
Role: Anonymous — Dr Zulu, Dr Naidoo, Dr Botha, Dr Khumalo, Dr van Rensburg, Dr Mokoena and Dr Sithole
Route: /account/register/start → /account/register → / → /account/logout/submit → /account/login
Do: Each registers from their own link with the cast's name and signs out. Dr Khumalo types her first name as "Fatma"
  by mistake (she corrects it in Step 2.41).
Expect: Each page names the address, the role's label and its scope, and the link's last day: "This invitation is for
  zulu@kgk.wombat.local, as Committee member in Paediatrics at Kgosi Kgari Teaching Hospital.", "… as Assessor in
  Paediatrics / Paediatrics at …", "… as Committee member at …" for van Rensburg, "… as Speciality admin in
  Paediatrics at …", "… as Sub-speciality admin in Paediatrics / Paediatrics at …", each with "… its link works until
  `D+14`." under the form. Each lists the six password rules under Password (Step 2.9). Each person lands on Home
  viewing as that role. Zulu, Naidoo, Botha and van Rensburg view as CommitteeMember, Khumalo as Assessor, Mokoena as
  SpecialityAdmin and Sithole as SubSpecialityAdmin. Each sign-out lands on the sign-in page with "You have signed
  out."
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Each link went through /account/register/start to
  /account/register (no token in the address or source). Pages: "This invitation is for zulu@kgk.wombat.local, as
  Committee member in Paediatrics at Kgosi Kgari Teaching Hospital." (Naidoo, Botha alike), "…
  khumalo@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at …", "… vanrensburg@sun.wombat.local, as
  Committee member at …", "… mokoena@…, as Speciality admin in Paediatrics at …", "… sithole@…, as Sub-speciality
  admin in Paediatrics / Paediatrics at …", each with "… its link works until 2026-11-03." and the six rules. Each
  landed on Home: Zulu, Naidoo, Botha, van Rensburg "Committee member · Semester 2, 2026", Khumalo (typed "Fatma")
  "Assessor · …", Mokoena "Speciality admin · …", Sithole "Sub-speciality admin · …", the sidebar acting as that role.
  Each sign-out: "You have signed out.".
Gap: none

### Step 2.11 — A used link cannot be used again
Role: Anonymous — Mr Pieter Smit
Route: /account/register/start → /account/register
Do: Open his invitation link a second time.
Expect: As Step 1.9: "Invitation already used", "This invitation has already been used: an account was registered from
  it." and "If it was yours, sign in with that account.", with Sign in; no form.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The link went through the start link to /account/register:
  "Invitation already used" (its tab too), "This invitation has already been used: an account was registered from it."
  and "If it was yours, sign in with that account.", with Sign in (and Forgotten your password?); no input.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): "No active invitations". Users: ten rows (Mbatha and the
  nine), all KGK, Active, each with its invited role; no Demo, devadmin or Kruger row. "zulu" left Thandi Zulu alone;
  cleared, ten again. Manage on her row: Home › Users › Nolwazi Mbatha, "Your own account's summary, roles and pending
  invitations.", the note as quoted, then "Change your password from your account." with the Change your password link
  (/account/change-password). Roles InstitutionalAdmin; no button on the page: no Remove, Add role, Reset password or
  Lockout.
Gap: none

### Step 2.13 — Mbatha adds Assessor to Dr Zulu, Dr Naidoo and Dr Botha
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: For each of the three, open Manage, choose Assessor under Add role and press Add role.
Expect: Before the change, Roles lists CommitteeMember with Remove. Add role offers InstitutionalAdmin, SpecialityAdmin,
  SubSpecialityAdmin, Coordinator and Assessor, never Administrator, CollegeAdmin, PendingTrainee or Trainee (T303).
  Its help reads "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with
  'Admit…'.", on every user's page.
  A Lockout card lists what a lock does (T284), and Pending invitations reads "No active invitations are outstanding for
  this email.". After the change the page reads "Role 'Assessor' added.", Roles lists both, and Add role no longer offers
  Assessor. The users list's Roles column shows both.
Note: A role change ends any session the account still has open within a minute (T279). Step 2.10 signed everyone out.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Before: Roles CommitteeMember with Remove ("Remove the
  CommitteeMember role"); Add role offers exactly InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator
  and Assessor, with "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with
  'Admit…'." (aria-describedby; Naidoo's and Botha's pages too); the Lockout card lists four effects; "No active
  invitations are outstanding for this email.". After: "Role 'Assessor' added." (focused), Roles lists both, Assessor
  gone from Add role. Same for Naidoo and Botha; the users list's Roles reads "CommitteeMember, Assessor" for all
  three.
Gap: none

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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The list first read "No assessor profiles" / "Create the first
  assessor profile to make assessors selectable in the workflow.". Picker: the five KGK Assessors only, each
  "(email)"; Institution KGK alone; Sub-speciality "All", disabled, until a speciality is chosen; Training status
  offers the four; the date shown only for Provisional or Trained. An empty save read "The UserId field is required."
  and "The Qualifications field is required.". Each save read "Assessor profile saved." (focused) but stayed at
  /admin/assessors/edit, and every new form, a sixth included, still offered all five.
Gap: [F-2.14a, T289] (still) No move to `?id=` after a save (T064). [F-2.14b, T289] (still) The new form's picker is
  not narrowed to unprofiled assessors (T064). [F-2.14c, T326] (still) The empty-list body says "selectable in the
  workflow". [F-2.14d, T324] (still) The empty save reads "The UserId field is required.".

### Step 2.15 — The assessor list and who is not on it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors
Do: Read the list.
Expect: Five rows, each at KGK in Paediatrics / Paediatrics. Training status reads Trained for Zulu, Naidoo and Botha, In
  training for Patel and Provisional for Khumalo. Training completed reads `D−8 years`, `D−6 years`, "Not recorded",
  "Not recorded" and `D−1 month` respectively. Each row has an Edit named for its assessor. Van Rensburg is neither listed
  nor offered, because he does not hold the Assessor role.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Five rows, KGK, Paediatrics / Paediatrics: Botha Trained "Not
  recorded", Khumalo (Fatma) Provisional 2026-09-20, Naidoo Trained 2020-10-20, Patel In training "Not recorded", Zulu
  Trained 2018-10-20. Each Edit is named "Edit <name>". Van Rensburg is not listed and was never offered.
Gap: none

## Phase 2.D — Registrar invitations

### Step 2.16 — Mbatha invites the five registrars, one to a mistyped address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/invitations/new → /admin/invitations
Do: From Invitations, press Invite a person. Invite `molefe@`, `dlamini@`, `duplessis@` and `mahlangu@kgk.wombat.local`
  as Trainee at KGK, Paediatrics / Paediatrics. Copy each link. For Dr Ndlovu she types `ndlvou@kgk.wombat.local`.
  Then press Open Invitations.
Expect: A Trainee invitation needs both speciality and sub-speciality: Speciality is starred, and Sub-speciality is
  drawn, starred, once Paediatrics is chosen. Each stub reads "You are invited to Wombat as Trainee in Paediatrics /
  Paediatrics at Kgosi Kgari Teaching Hospital." Active invitations reads "5 active": five rows "Trainee" over
  "Paediatrics / Paediatrics", newest first (`ndlvou@` first), each Issued `D` over its time in SAST, Last day `D+14`
  and Delivery "Being sent".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): From Invitations, Invite a person. For Trainee, Speciality *
  at "Choose a speciality"; with Paediatrics chosen, Sub-speciality * was drawn. Each issue read as 2.2's for its
  address (focused result, "Link copied."). Five stubs "You are invited to Wombat as Trainee in Paediatrics /
  Paediatrics at Kgosi Kgari Teaching Hospital." Open Invitations (no link on it): "5 active", five rows "Trainee"
  over "Paediatrics / Paediatrics", ndlvou@ first, each 2026-10-20 over 11:21 or 11:22 SAST, Last day 2026-11-03,
  "Being sent".
Gap: none

### Step 2.17 — Mbatha revokes the mistyped invitation and issues the right one
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/invitations/new → /account/register/start → /account/register
Do: Press Revoke… on the `ndlvou@` row and confirm. From the result, press Invite another address as Trainee,
  Paediatrics / Paediatrics, enter `ndlovu@kgk.wombat.local`, issue it and copy the link. Then open the revoked link,
  as anyone holding it could.
Expect:
  - **Revoke…** is an outline button named "Revoke the Trainee invitation to ndlvou@kgk.wombat.local" (T239). It opens
    "Revoke this invitation?": "Revoke the Trainee invitation to **ndlvou@kgk.wombat.local** (Paediatrics /
    Paediatrics)?", then "The registration link already emailed to that address stops working at once: nobody can
    register from it." and "The invitation leaves this list. An address is never corrected in place: to invite the
    right one, issue a new invitation.", then "Revoking cannot be undone.", with Keep the invitation and Revoke
    invitation (T264). At 390 px the two are stacked, 44 px each, Keep the invitation last.
  - **Revoke invitation:** the result, focused: "**Invitation revoked.** The link sent to ndlvou@kgk.wombat.local no
    longer works: nobody can register from it.", with "Invite another address as Trainee, Paediatrics / Paediatrics".
    No registration link is on the page. The row is gone: "4 active".
  - **Invite another address** opens Invite a person with "Role and scope filled from the revoked invitation to
    ndlvou@kgk.wombat.local. Type the right address.", Trainee, Paediatrics and Paediatrics chosen, and Email empty and
    focused. The issue reads as Step 2.2's, for ndlovu@kgk.wombat.local.
  - **The revoked link:** "Invitation revoked" (its tab too): "This invitation has been revoked." and "Ask the person
    who invited you for a new invitation."; no form.
Note: The revoke and the new issue are 5 presses and 1 field: Revoke…, Revoke invitation, Invite another address,
  Issue invitation and Copy link; the address.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Revoke… is an outline button named "Revoke the Trainee
  invitation to ndlvou@kgk.wombat.local". It opened "Revoke this invitation?" with the three paragraphs word for word
  and Keep the invitation and Revoke invitation. At 390 px the two are stacked, 44 px each, Keep the invitation below
  (last on screen), focused on open and first in tab order; no sideways scroll; Escape closed it and returned the
  focus to Revoke…. Revoke invitation: the result, focused, "Invitation revoked. The link sent to
  ndlvou@kgk.wombat.local no longer works: nobody can register from it." with "Invite another address as Trainee,
  Paediatrics / Paediatrics"; no link on the page; "4 active". Invite another address: /admin/invitations/new?from=16,
  "Role and scope filled from the revoked invitation to ndlvou@kgk.wombat.local. Type the right address.", Trainee,
  Paediatrics and Paediatrics chosen, Email empty and focused; the issue read as 2.2's for ndlovu@kgk.wombat.local,
  "Link copied.". The revoked link (another browser): "Invitation revoked" (its tab too), "This invitation has been
  revoked." and "Ask the person who invited you for a new invitation."; no form.
Gap: none (T264 no longer occurs: Revoke confirms in a dialog, and no link stays on screen)

## Phase 2.E — The registrars register and wait

### Step 2.18 — Four registrars register and are pending
Role: Anonymous — Dr Molefe, Dr Dlamini, Dr Mahlangu and Dr Ndlovu
Route: /account/register/start → /account/register → / → /account/logout/submit → /account/login
Do: Each registers from their own link. Molefe, Dlamini and Ndlovu sign out. Dr Mahlangu stays signed in with Home open
  in her tab, which Step 2.31 uses. Dr du Plessis cannot register: he says no email reached him.
Expect: Each page reads "This invitation is for ‹address›, as Trainee in Paediatrics / Paediatrics at Kgosi Kgari
  Teaching Hospital." Each registrar lands on Home, "Pending trainee · Semester N, YYYY", because a Trainee invitation
  registers as PendingTrainee until admission. Home shows one card, "Awaiting admission", with the hourglass icon and
  the control edge, no stripe (F8):
  - "You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital.
    Until then, nothing can be filed by you or about you.";
  - Institution "Kgosi Kgari Teaching Hospital"; Programme "Paediatrics / Paediatrics, on the Paediatric EPA
    Curriculum (11.1)"; Invited by "Nolwazi Mbatha, Institutional admin, on `D`"; Who admits you "An Institutional
    admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.";
  - "When you are admitted", four numbered steps: "Your programme start date is set, and with it your training
    year.", "You are signed out once, and sign in again as a Trainee.", "Then, once your programme has started, you
    can log activities and follow your progress." and "An email tells you, at ‹address›, that it has happened.";
  - "Meanwhile": "You can check your account and your data rights.", with Review your account and Open My data
    rights, each with its arrow.
  The header offers no action. Each sign-out lands on the sign-in page with "You have signed out."
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Each link went through the start link to /account/register (no
  token in the address or source); each page read "This invitation is for <address>, as Trainee in Paediatrics /
  Paediatrics at Kgosi Kgari Teaching Hospital.". Each landed on Home, "Pending trainee · Semester 2, 2026", with one
  card, "Awaiting admission" (hourglass icon, a 1 px grey edge all round, no stripe): the sentence as quoted;
  Institution, Programme "Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1)", Invited by "Nolwazi
  Mbatha, Institutional admin, on 2026-10-20", Who admits you as quoted; "When you are admitted" with the four steps
  in a numbered list, the last naming the registrar's own address; "Meanwhile" with Review your account and Open My
  data rights, each with its arrow. No header action. Molefe, Dlamini and Ndlovu signed out to "You have signed out.";
  Mahlangu stayed signed in on Home, held open by a background browser from 11:24 SAST. Du Plessis did not register.
Gap: none

### Step 2.19 — What a registrar sees before admission
Role: PendingTrainee — Dr Nomsa Mahlangu
Route: / → /account/profile → /activities/mine → /activities/new → /portfolio/progress → /access-denied → /
Do: Read the menu. Follow Review your account. Type the addresses of My activities, Log an activity and My progress.
Expect: The sidebar reads "Acting as Pending trainee" over Home, then My data rights under the rule, and nothing else: a
  pending trainee files nothing until admitted, so has no activities to list (D55, T141). Her Home's card is Step
  2.18's, its last step naming mahlangu@kgk.wombat.local. My account lists her role as "Pending trainee", and her
  institution, Kgosi Kgari Teaching Hospital. My activities reads "No activities yet", with no Log an activity. Log
  an activity and My progress each show "You cannot open this page" ("Your role (Pending trainee) does not open this
  page.") with Go to Home (T335).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in a second browser; her held tab stayed on Home. The
  sidebar reads "Acting as Pending trainee" over Home, then My data rights, nothing else; Home's card is 2.18's, its
  last step naming mahlangu@kgk.wombat.local. Review your account: My account, Institution Kgosi Kgari Teaching
  Hospital, Roles "Pending trainee". My activities: "No activities yet" / "Nothing has been filed about you.", no Log
  an activity. Typed Log an activity and My progress: /access-denied?ReturnUrl=…, "You cannot open this page", "Your
  role (Pending trainee) does not open this page.", Go to Home; nothing in the menu lit. Go to Home returned to Home.
Gap: none

## Phase 2.F — The review panel

While the registrars' links age (Step 2.26 needs an hour), Mbatha forms the panel.

### Step 2.20 — Decision panels before any exist
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Open Decision panels.
Expect: While the page reads, the header shows at once over skeletons: no New panel yet, and Who decides each EPA draws
  skeletons, never a sentence saying there is no routing (T329). Then the page reads "No decision panels" / "Create a
  panel before scheduling reviews." and offers New panel in the header. "Who decides each EPA" names `Paediatric EPA
  Curriculum 11.1` in one row, "PAED-001 to PAED-015", against "No panel at this institution covers this programme."
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Navigated from Home (client-side), watching the DOM: while it
  read, the header was there at once ("Decision panels" / "The panels that take entrustment and progression
  decisions.") over "Loading Decision panels." and five skeletons, with no New panel and no sentence about routing
  under "Who decides each EPA". Then: "No decision panels" / "Create a panel before scheduling reviews.", New panel in
  the header, and "Who decides each EPA" naming Paediatric EPA Curriculum 11.1 in one row, "PAED-001 to PAED-015",
  against "No panel at this institution covers this programme.".
Gap: none

### Step 2.21 — The new-panel form, and a panel of one refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new
Do: Name it "Paed Annual Review Panel", with Scope Speciality, Speciality Paediatrics and Decides for General panel.
  Under Who sits on it, seat Zulu and Naidoo as Member, then choose Zulu as Chair. Set Naidoo to Not seated, leaving the
  chair alone, and press Create panel.
Expect: The page reads "New panel": "A panel takes entrustment and progression decisions for the trainees it covers. Its
  name, scope, institution and speciality cannot change once it is created."
  - Scope offers Institution and Speciality and starts on Speciality. Speciality offers Paediatrics alone ("The panel
    runs at your institution. Only the specialities it has adopted a curriculum in are listed."). Decides for offers
    General panel and the Neonatal team Clinical Competency Committee.
  - Chair, and the seat picker under it (one row a person: Not seated, Member or External member), each list exactly
    Botha, Naidoo, van Rensburg and Zulu, by surname, each with the whole address (van Rensburg's on
    `sun.wombat.local`): active committee members at KGK who are not trainees (T165, T237).
  - Choosing Zulu as chair says "Thandi Zulu is the chair now, so is no longer seated as a member." (T257), and her row
    reads "Chair · chosen above".
  - Create panel is refused and nothing is created: "The panel was not created. Everything you chose is kept below.",
    with "Seats: A panel needs at least two members: the chair and at least one other, so that no decision is one
    person's." (D46). The summary takes the focus, and the seats are marked with the same sentence.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): "New panel" with the quoted sentence. Scope offered
  Institution and Speciality, on Speciality; Speciality offered Paediatrics with "The panel runs at your institution.
  Only the specialities it has adopted a curriculum in are listed."; Decides for offered General panel and the
  Neonatal team Clinical Competency Committee. Chair and the seat picker (Not seated / Member / External member) each
  listed Botha, Naidoo, van Rensburg and Zulu by surname, with the whole address (van Rensburg's on sun.wombat.local).
  Choosing Zulu as chair: "Thandi Zulu is the chair now, so is no longer seated as a member." and her row "Chair ·
  chosen above". With Naidoo then Not seated, Create panel was refused at /committee/panels/new: "The panel was not
  created. Everything you chose is kept below." with "Seats: A panel needs at least two members: the chair and at
  least one other, so that no decision is one person's." (the summary focused, the seats repeating the sentence).
  Nothing created.
Gap: none

### Step 2.22 — Mbatha saves the panel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new → /committee/panels/{PanelId:int}
Do: Seat Naidoo as Member and van Rensburg as External member, then press Create panel. Botha is left Not seated, for
  Dr Mokoena in Step 2.23.
Expect: The panel's own page opens, and names the panel: its h1 and tab read "Paed Annual Review Panel", its subtitle
  "Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital" (T260). "Paed Annual Review Panel was created. Its
  reviews can now be scheduled on Committee reviews." takes the focus.
  - **About this panel:** Scope "Speciality: Paediatrics", Institution Kgosi Kgari Teaching Hospital, Open reviews
    None, and "Its name, scope, institution and speciality cannot change once it is created. Its members can, even
    while its reviews are open."
  - **Who sits on it:** Chair Zulu; Naidoo Member, van Rensburg External member, Botha Not seated; "3 seats: the chair,
    1 member and 1 external member." The externals' help says external members sit with the chair on the panel's
    appeal body, and that cross-institution externals are not yet supported.
  - **Decides for:** General panel, with Save committee and the rule under it.
  There is no "Back to panels".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Naidoo Member, van Rensburg External member, Create panel:
  /committee/panels/1?created=1, h1 and tab "Paed Annual Review Panel", subtitle "Speciality panel · Paediatrics ·
  Kgosi Kgari Teaching Hospital"; "Paed Annual Review Panel was created. Its reviews can now be scheduled on Committee
  reviews." focused. About this panel: Scope "Speciality: Paediatrics", Institution KGK, Open reviews "None", the
  name-cannot-change sentence. Who sits on it: Chair Zulu ("Chair · chosen above"), Naidoo Member, van Rensburg
  External member, Botha Not seated; "3 seats: the chair, 1 member and 1 external member." and the externals' help as
  expected. Decides for: General panel with Save committee and the rule. No "Back to panels".
Gap: none

### Step 2.23 — Dr Mokoena adds Dr Botha to the panel
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/panels → /committee/panels/{PanelId:int} → /committee/panels
Do: Sign in, open Decision panels and Edit the panel. Seat Botha as Member, press Save panel, then go back to Decision
  panels from the menu.
Expect: The list offers her New panel and an Edit on this panel, a Speciality-scoped panel in her speciality at her
  institution (T194). On the panel's page Decides for is read-only: "General panel", with "Only an institutional
  administrator can change which College committee a panel sits as." Save panel reads "Panel members updated. Thandi
  Zulu chairs, with 2 members and 1 external member.", with the focus on it. Back on the list, Members names the four,
  "4: Thandi Zulu (chair), …, John van Rensburg (external)", with Sarah Botha, then David Naidoo, between (the members
  in the picker's surname order).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The list offered her New panel and "Edit Paed Annual Review
  Panel" (Members "3: Thandi Zulu (chair), David Naidoo, John van Rensburg (external)"). On the panel's page Decides
  for was read-only, "General panel", with "Only an institutional administrator can change which College committee a
  panel sits as.", no select and no Save committee. Botha as Member, Save panel: "Panel members updated. Thandi Zulu
  chairs, with 2 members and 1 external member." focused; back on the list from the menu, Members "4: Thandi Zulu
  (chair), Sarah Botha, David Naidoo, John van Rensburg (external)".
Gap: none

### Step 2.24 — Who decides each EPA now
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Read the list and Who decides each EPA.
Expect: One row: Paed Annual Review Panel, Scope "Speciality · Paediatrics", Decides for General panel, and Members
  naming the four (the chair first, van Rensburg marked external), with an Edit. Who decides each EPA has two rows:
  "PAED-001 to PAED-003, PAED-006 to PAED-015" go to Paed Annual Review Panel, and "PAED-004, PAED-005" to "Paed Annual
  Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the
  general panel decides." (T131).
Note: KGK has no neonatal CCC, so "Decides for" stays General panel. Act 4 schedules every review on this panel.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): One row: Paed Annual Review Panel, "Speciality · Paediatrics",
  General panel, "4: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van Rensburg (external)", with Edit. Who
  decides each EPA: "PAED-001 to PAED-003, PAED-006 to PAED-015" to Paed Annual Review Panel, and "PAED-004, PAED-005"
  to "Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency
  Committee, so the general panel decides.".
Gap: none

### Step 2.25 — Dr Sithole looks at what he may create
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/panels → /committee/panels/new → /committee/panels
Do: Sign in, open Decision panels, press New panel and read the form. Then Cancel without creating.
Expect: The list offers him New panel and an Edit on the panel. The form's Scope reads "Speciality" as text, with "A
  sub-speciality administrator creates Speciality-scoped panels for the speciality of the sub-speciality.": a
  sub-speciality administrator manages the Speciality-scoped panels of his sub-speciality's speciality (T194, T245).
  Speciality offers Paediatrics. There is no Decides for field. Cancel returns to Decision panels; nothing is created.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The list offered him New panel and "Edit Paed Annual Review
  Panel". The form's Scope read "Speciality" as text with "A sub-speciality administrator creates Speciality-scoped
  panels for the speciality of the sub-speciality." (no radios); Speciality offered Paediatrics; no Decides for field.
  Cancel returned to Decision panels; still one panel.
Gap: none

## Phase 2.G — A lost email

### Step 2.26 — Mbatha resends Dr du Plessis's invitation
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: At least an hour after Step 2.16, open Invitations and press Resend on du Plessis's row. Copy the new link.
Expect: His is the only row left. Its Delivery reads "Not delivered", with the warning icon, and a note row under it
  reads "Resend emails a new link in place of the current one, which then stops working." The row offers Resend
  (named "Resend the Trainee invitation to duplessis@kgk.wombat.local", described by the note) before Revoke….
  Afterwards the result, focused: "**A new invitation link is being emailed to duplessis@kgk.wombat.local.** The link
  it replaces no longer works. Copy the new link below — it is shown only once.", then the link with Copy link and "It
  works until `D+14`. It is shown here until your next action, then never again." The row reads Issued `D` over the
  resend's time in SAST, with no "Resent" mark (F2), Last day `D+14` and "Being sent"; it has no note row and offers
  only Revoke…. The log holds one more stub "Your Wombat invitation", "… as Trainee in Paediatrics / Paediatrics at
  Kgosi Kgari Teaching Hospital."
Note: This needs the log sender (see Mail). Against a mail server that accepted the first email, the row reads "Sent"
  and offers no Resend. The resend is 2 presses: Resend and Copy link.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played over an hour after Step 2.16 (du Plessis's invitation
  issued 11:21 SAST; Resend pressed 12:24); the hour was waited in short background waits, Step 2.36 played meanwhile.
  His row alone: Delivery "Not delivered" with the warning icon, a note row under it "Resend emails a new link in
  place of the current one, which then stops working."; Resend (named "Resend the Trainee invitation to
  duplessis@kgk.wombat.local", described by the note) before Revoke…. Afterwards the result, focused: "A new
  invitation link is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy the new
  link below — it is shown only once.", the link with Copy link ("Link copied.") and "It works until 2026-11-03. It is
  shown here until your next action, then never again.". The row: Issued 2026-10-20 over 12:24 SAST, no Resent mark,
  Last day 2026-11-03, "Being sent", no note row, only Revoke…. One more stub (18 in all), "… as Trainee in
  Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.".
Gap: none

### Step 2.27 — Dr du Plessis registers from the new link
Role: Anonymous — Dr Pieter du Plessis
Route: /account/register/start → /account/register → / → /account/logout/submit → /account/login
Do: Open the first link, then the resent one. Register from the resent link and sign out.
Expect: The first link reads "Invitation link not recognised" (its tab too): "This invitation is invalid." and "The link
  may be incomplete, or a newer invitation email may have replaced it. Open the link in the most recent invitation
  email, or ask the person who invited you for a new invitation."; no form. The resend replaced the link's hash, so
  the old token matches no invitation. The resent link reads "This invitation is for duplessis@kgk.wombat.local, as
  Trainee in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.", registers him and lands on Home, "Pending
  trainee · Semester N, YYYY", with Step 2.18's Awaiting admission card. Signing out lands on the sign-in page with
  "You have signed out."
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The first link (through the start link, no token in the
  address): "Invitation link not recognised" (its tab too), "This invitation is invalid." and the sentence as quoted;
  no form. The resent link read "This invitation is for duplessis@kgk.wombat.local, as Trainee in Paediatrics /
  Paediatrics at Kgosi Kgari Teaching Hospital.", registered him and landed on Home, "Pending trainee · Semester 2,
  2026", with 2.18's Awaiting admission card; signing out read "You have signed out.".
Gap: none (T315 no longer occurs: no token in the address bar)

## Phase 2.H — Admission

### Step 2.28 — The pending registrars, as Mbatha sees them
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/users → /admin/users/{UserId} → /admin/trainees
Do: Open Invitations. Read the users list and open Dr Molefe's page. Then open Trainees.
Expect: Active invitations reads "No active invitations". Users lists 15, with the five registrars as PendingTrainee. On
  Molefe's page, PendingTrainee reads "System-managed" and has no Remove. Add role offers InstitutionalAdmin,
  SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember and Assessor, not Trainee, and its help reads "Trainee
  is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'." (T303).
  Trainees reads "Admit registrars who are waiting, and keep current trainees' programmes." under its heading:
  - "Waiting to be admitted" "5 waiting", captioned "Registered from a Trainee invitation, by surname. A deactivated
    account is not listed. Nothing can be filed about them until they are admitted.": Dlamini, du Plessis, Mahlangu,
    Molefe and Ndlovu, each with Email, Invited as "Paediatrics / Paediatrics", Registered `D` and an outline Admit…
    named "Admit ‹name›". There is no Institution column: her list is KGK's.
  - "Current trainees": "No current trainees" / "A registrar appears here once admitted." There is no Completed and
    closed section.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): "No active invitations". Users lists 15, the five registrars
  as PendingTrainee. Molefe's page: Roles lists PendingTrainee with "System-managed" and no Remove; Add role offers
  exactly InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember and Assessor, with the
  T303 help under it. Trainees: "Admit registrars who are waiting, and keep current trainees' programmes."; "Waiting
  to be admitted" "5 waiting", the caption as quoted, Dlamini, du Plessis, Mahlangu, Molefe and Ndlovu, each Email,
  Invited as "Paediatrics / Paediatrics", Registered 2026-10-20 and an outline Admit… named "Admit <name>"; no
  Institution column. "Current trainees": "No current trainees" / "A registrar appears here once admitted."; no
  Completed and closed section.
Gap: [F-2.28b, T323] (still) The Roles item runs the role's name into its note: its text (innerText and textContent)
  is "PendingTraineeSystem-managed".

### Step 2.29 — Mbatha admits four registrars
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/admit → /admin/trainees
Do: Admit each from its Admit…, with its programme start and expected completion. Molefe starts `J−3y` and completes
  `J+1y−1d`. Dlamini starts `J−2y` and completes `J+2y−1d`. Du Plessis starts `J−1y` and completes `J+3y−1d`.
  Mahlangu starts `J` and completes `J+4y−1d`.
Expect:
  - **Admit ‹name›** is a page of its own: "Admit Lerato Molefe" (its tab too), under Home › Trainees › Admit Lerato
    Molefe with Trainees lit, and "Admit a registrar who is waiting into the institution's curriculum.". "Who is being
    admitted" reads Email, Institution "Kgosi Kgari Teaching Hospital", Invited as "Paediatrics / Paediatrics" and
    Registered `D`.
  - **Programme:** "* marks a field you must fill in."; Curriculum is read out, not offered: "Paediatric EPA
    Curriculum (11.1)", with "Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited
    sub-speciality." (KGK's adopted version, T091). Programme start is starred and pre-filled with today. Beside it,
    Training year is an output of the start, not a field (S18, T306): "Training year 1" for today, "Training year 4"
    once `J−3y` is entered, with "Training years turn every 365 days from the programme start, counted to `D`. It
    follows the date when you change it." Expected completion's help is "Leave this empty to derive it from the
    curriculum window."; once a start is typed, while the field is empty, the help says what it would be. Where that
    date has passed the help is replaced by "Left empty, it would be ‹`J−2y`›, which has passed. Enter the expected
    completion." (Molefe's start; Dlamini's and du Plessis's too); otherwise it adds "Left empty, it will be ‹date›." to the first
    sentence.
  - **"When you admit:"** "Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year
    4.", "Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee." and "An email
    tells Lerato Molefe so."; then Cancel and "Admit Lerato Molefe".
  - **The result:** each admission lands on Trainees with, focused, directly above the Waiting table, "**Lerato Molefe
    admitted to the Paediatric EPA Curriculum (11.1), training year 4, from `J−3y`.**" (S8); Waiting then reads "4
    waiting", Dlamini's Admit… first. After the four, Waiting reads "1 waiting", and "Current trainees" "4 current",
    captioned "Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.",
    lists each with "Paediatric EPA Curriculum (11.1)" over "Paediatrics", the programme start over "Training year N"
    (Dlamini 3, du Plessis 2, Mahlangu 1, Molefe 4), the completion entered, and Edit named "Edit ‹name›'s profile".
  - **The mail:** each admission writes a stub "You are admitted in Wombat", tagged `trainee, admitted`: "You have been
    admitted to the Paediatric EPA Curriculum (11.1), in training year 4." and "If you were signed in to Wombat, you
    have been signed out once: sign in again to log activities and follow your progress.", with the sign-in link.
Note: Admission into a version KGK has not adopted is refused (T091); the page offers none. One admission from
  Trainees is 2 presses and 2 fields: Admit… and Admit ‹name›; the start and the completion.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): "Admit Lerato Molefe" (its tab too), Home › Trainees › Admit
  Lerato Molefe, Trainees lit, "Admit a registrar who is waiting into the institution's curriculum."; Who is being
  admitted: Email, Institution KGK, Invited as "Paediatrics / Paediatrics", Registered 2026-10-20. Programme: "* marks
  a field you must fill in."; Curriculum read out, "Paediatric EPA Curriculum (11.1)" with "Adopted at Kgosi Kgari
  Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality."; Programme start * pre-filled
  2026-10-20 with the output "Training year 1", "Training year 4" once 2023-01-15 was entered, and the 365-day
  sentence counted to 2026-10-20. Completion help at today's start: "… Left empty, it will be 2027-10-20."; at
  Molefe's start "Left empty, it would be 2024-01-15, which has passed. Enter the expected completion." (Dlamini
  2025-01-15 and du Plessis 2026-01-15 also "has passed"; Mahlangu "Left empty, it will be 2027-01-15."). "When you
  admit:" the three sentences as quoted (year 4), Cancel and "Admit Lerato Molefe"; at 390 px both 44 px tall, no
  sideways scroll. Each admission landed on /admin/trainees?admitted=<id> with, focused, under the Waiting heading,
  "Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15." (Dlamini year 3
  from 2024-01-15, du Plessis 2 from 2025-01-15, Mahlangu 1 from 2026-01-15); "4 waiting" with Dlamini first. After
  four: "1 waiting", "4 current" with the caption as quoted, each "Paediatric EPA Curriculum (11.1)" over
  "Paediatrics", start over "Training year N" (Dlamini 3, du Plessis 2, Mahlangu 1, Molefe 4), completions 2028-01-14,
  2029-01-14, 2030-01-14, 2027-01-14, Edit "Edit <name>'s profile". At 901, 1000, 1100 and 390 px no sideways scroll
  and no word split mid-word. Four stubs "You are admitted in Wombat" (tags: trainee, admitted), "You have been
  admitted to the Paediatric EPA Curriculum (11.1), in training year 4." (3, 2, 1) and the signed-out sentence, with
  the sign-in link.
Gap: none (the Expect named only Molefe and Dlamini for "has passed"; du Plessis's start (`J−1y`) derives `J`, which
  has passed too, and the Expect now says so. T306 no longer occurs: the form and Current trainees show
  the start and the training year.)

### Step 2.30 — Dr Ndlovu's completion date is derived, then corrected
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/admit → /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit Ndlovu with start `J` and the completion date left empty. Then open his profile from Current trainees (Edit),
  set completion to `J+4y−1d` and save the profile.
Expect: With start `J` and the completion empty, its help reads "Leave this empty to derive it from the curriculum
  window. Left empty, it will be `J+1y`.": `J` + 12 months, the longest item window on v11.1. The result reads
  "**Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from `J`.** Expected completion
  `J+1y`, derived from the curriculum window: change it under Edit." (only the first sentence bold), and Waiting to be admitted reads "Nobody is
  waiting to be admitted." / "A registrar appears here after registering from a Trainee invitation." On his profile
  the save reads "Trainee profile saved.", and Current trainees shows `J+4y−1d`.
Note: A 12-month window for a four-year programme is T139's open College question. Admitting with the completion empty
  is 2 presses and 1 field.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Start 2026-01-15, completion empty: "Leave this empty to
  derive it from the curriculum window. Left empty, it will be 2027-01-15." Admitted: "Sipho Ndlovu admitted to the
  Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15. Expected completion 2027-01-15, derived from the
  curriculum window: change it under Edit." (focused), and Waiting "Nobody is waiting to be admitted." / "A registrar
  appears here after registering from a Trainee invitation.". Edit Sipho Ndlovu's profile (/admin/trainees/edit?id=6):
  completion 2027-01-15 set to 2030-01-14, Save profile: "Trainee profile saved." (focused); Current trainees shows
  2030-01-14. The fifth admission stub came (23 stubs in all).
Gap: none

### Step 2.31 — Dr Mahlangu's open session ends, and she signs back in as a Trainee
Role: Trainee — Dr Nomsa Mahlangu
Route: / → /account/session-ended → /account/login → /
Do: Her tab from Step 2.18 has stayed open through her admission. Once it leaves Home, she signs in again.
Expect: Within a minute of her admission the tab moves to the sign-in page, which reads "Your session has ended. Sign in
  again.", an information notice; the focus is on Email (no institution buttons on dev). Admission changes her role,
  and a role change ends open sessions (T279). Signed in again, she sees
  "Trainee · Semester N, YYYY" under Home's heading and the trainee dashboard of Step 2.39.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Not her Step 2.18 tab: the background browser holding it was
  stopped by the host for low memory before the admissions. A stand-in: her Home opened in a second context of the
  admitting browser at 12:24:44 SAST, never reloaded. Admitted at 12:25:14, it moved at 12:25:44 to
  /account/login?error=SessionEnded&returnUrl=%2F: "Your session has ended. Sign in again." as an information notice
  (alert-info), the focus on Email. Signed in again: Home, "Trainee · Semester 2, 2026" with the trainee dashboard
  (Your targets "Training year 1 — …"); the sidebar adds Log an activity, My activities, MSF reports, My committee
  reviews, Export portfolio and My progress.
Gap: none (played on a stand-in session opened a minute before the admission, not the 2.18 tab)

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
  T335). Decision panels lists the panel (Scope "Speciality · Paediatrics", its four members by name) with no New
  panel and no Edit column, and Decision panels is lit there (T358, C10). The panel form reads "You cannot open this
  page" in the same words: the Coordinator reads panels, and edits none (D4).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home, "Coordinator · Semester 2, 2026", header action "Start
  an MSF campaign" (/msf/campaigns/new), no Quick action card. Cards in order: "Waiting for assessors", no badge,
  "Nothing is waiting for an assessor.", Open Waiting for assessors; "Nothing filed in 30 days", the rule line as
  quoted, "Every current registrar has filed something in the last 30 days.", Open in Programme trainees; "Invitations
  nearing expiry", "No invitations expiring soon.". The sidebar "Acting as Coordinator" over Home, Programme trainees,
  Waiting for assessors, Decisions due, MSF campaigns, Committee reviews, Decision panels and Data rights requests,
  then My data rights; no Invitations. Typed Invitations: /access-denied, "You cannot open this page", the two
  sentences as quoted, Go to Home. Decision panels (lit): the panel (Speciality · Paediatrics, its four members by
  name), no New panel, no Edit or Actions column. The typed panel form: "You cannot open this page" in the same words.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home, "Committee member · Semester 2, 2026"; the sidebar
  "Acting as Committee member" with "Switch to Assessor", over Home, Programme trainees, Committee reviews and
  Decision panels, then My data rights. "Registrars", badged "5 registrars", "Fewest met first, then by surname.":
  Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu, Lerato Molefe, Sipho Ndlovu, each a link to
  /programme/trainees/{ProfileId} (3, 4, 5, 2, 6) over "Training year N" (3, 2, 1, 4, 1), then "0 of 10" / "EPAs met
  this semester" and "0 of 5" / "EPAs met in 2026"; no exempt line; foot Open Programme trainees. "Targets by EPA",
  "Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30.": PAED-001 to PAED-015 in code order, each name a
  link to /programme/trainees?short=<EpaId> (2 to 16), over its cadence ("3 per semester", "1 per academic year"),
  then "0 of 5" over "registrars met this semester" or "registrars met in 2026"; no foot.
Gap: none

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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Switch to Assessor: Home, "Assessor · Semester 2, 2026", "You
  are now acting as Assessor." with the focus, no Switch back; the sidebar "Acting as Assessor" / "Switch to Committee
  member" over Home and Activity inbox, then My data rights; "Waiting for you" with no badge, "Nothing is waiting for
  you.", "Open Activity inbox", and Recent decisions "No decisions yet." with no "All your decisions". The reload: no
  alert. Signed out to "You have signed out."; Naidoo, signing in next in the same browser, landed as Committee
  member. Zulu in a fresh browser: Assessor, no alert. /dashboard/switch/Administrator: back on Home, still Assessor,
  nothing said. Switch to Committee member: its sidebar, menu and subtitle, and "You are now acting as Committee
  member." with the focus; her Committee member Home has no line about the inbox.
Gap: none

### Step 2.35 — Dr Naidoo and Dr Botha
Role: CommitteeMember + Assessor — Dr David Naidoo and Dr Sarah Botha
Route: /account/login → / → /committee/panels
Do: Each signs in, reads the dashboard and opens Decision panels.
Expect: Dr Naidoo and Dr Botha each see what Dr Zulu saw in Step 2.33: the same menu, the five registrars on Registrars
  in its order, and "0 of 5" for every EPA on Targets by EPA. Decision panels lists the panel both sit on, Scope
  "Speciality · Paediatrics" and its four members by name, with no New panel and no Edit.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Each: Home, "Committee member · Semester 2, 2026", "Acting as
  Committee member" with "Switch to Assessor", Zulu's menu, Registrars badged "5 registrars" in Zulu's order at "0 of
  10" / "0 of 5", and Targets by EPA PAED-001 to 015 each "0 of 5". Decision panels (lit) lists Paed Annual Review
  Panel (Speciality · Paediatrics, General panel, "4: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van
  Rensburg (external)") and the Who-decides card, no New panel and no Edit.
Gap: none

### Step 2.36 — Dr Patel and Dr Khumalo, assessors
Role: Assessor — Dr Mohammed Patel and Dr Fatima Khumalo
Route: /account/login → /
Do: Each signs in and reads the dashboard and nav.
Expect: Each sees "Assessor · Semester N, YYYY" on Home, and the empty Assessor dashboard of Step 2.34: "Waiting for
  you" with no badge and no "0", "Nothing is waiting for you.", "Open Activity inbox"; Recent decisions, "No decisions
  yet." (T350). The sidebar reads "Acting as Assessor" with no switch, over Home and Activity inbox, then My data
  rights.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played during the wait for Step 2.26. Each: Home, "Assessor ·
  Semester 2, 2026"; the sidebar "Acting as Assessor" with no switch, over Home and Activity inbox, then My data
  rights; "Waiting for you" with no badge and no "0", "Nothing is waiting for you.", "Open Activity inbox"; Recent
  decisions "No decisions yet.".
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
  rights. Decision panels lists the panel Dr van Rensburg sits on as external member, its Members marking him
  "(external)", with no New panel and no Edit.
Note: Zulu, Naidoo and Botha hold the Paediatrics sub-speciality only through their assessor profiles (Step 2.14), and a
  CommitteeMember invitation may carry none (Step 2.3). So a committee member with no assessor profile sees no trainee
  here, although he sits on the panel and may read every KGK trainee's record (T113). Fixed by T290 (2026-10-05):
  the card no longer reads sub-speciality claims.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home, "Committee member · Semester 2, 2026", "Acting as
  Committee member" with no switch, over Home, Programme trainees, Committee reviews and Decision panels, then My data
  rights. "Registrars", badged "5 registrars", lists the five in Step 2.33's order, each "0 of 10" / "0 of 5"; Targets
  by EPA PAED-001 to 015 each "0 of 5". Decision panels (lit) lists the panel with "John van Rensburg (external)", no
  New panel and no Edit.
Gap: none

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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Mokoena "Speciality admin · Semester 2, 2026", Sithole
  "Sub-speciality admin · Semester 2, 2026". Each: "Waiting for assessors", no badge, "Nothing is waiting for an
  assessor.", Open Waiting for assessors; "Registrars", badged "5 registrars", the five in Step 2.33's order, Open
  Programme trainees; "Targets by EPA" PAED-001 to 015 each "0 of 5". No Pending reviews, no Trainees in programme, no
  "inactive". The sidebar "Acting as Speciality admin" or "Acting as Sub-speciality admin" over Home, Programme
  trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and Entrustment decisions, then
  My data rights.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home, "Trainee · Semester 2, 2026", header action "Log an
  activity". Your targets: "Training year 4 — it sets the minimum level each encounter is judged against.", "0 of 10"
  over "EPAs met this semester", "0 of 5" over "EPAs met in 2026"; Furthest short PAED-001 to 005, each a link to
  /portfolio/progress/{EpaId} (2 to 6) with "0 of 3 this semester · 3 more by 2026-11-30", no part-way line; Open My
  progress. Needs you "Nothing needs you. Requests you have filed are in My activities." with Open My activities;
  Recent decisions "No decisions yet."; My authorisations "No STAR yet. When the committee issues one, it shows here
  against training year 4's level." with Open My authorisations. No Recent activities or Upcoming deadlines card. The
  sidebar "Acting as Trainee" over Home, Log an activity, My activities, MSF reports, My committee reviews, Export
  portfolio, My progress and My data rights. My progress (lit): "Training year 4 · Semester 2, 2026"; This period "0
  of 10", "0 of 5" in 2026, "Semester 2, 2026 ends on 2026-11-30.", the training-year sentence and "Multi-source
  feedback: 0 of 15 EPAs covered by a released campaign that closed this semester. …"; Your EPAs "Each semester · 10
  EPAs" and "Once a year · 5 EPAs", every count 0 and every STAR "No decision"; "0 at or above · 0 below · 15 with no
  decision, of 15 EPAs".
Gap: none

### Step 2.40 — The other registrars' training years
Role: Trainee — Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu
Route: /account/login → / → /portfolio/progress
Do: Each signs in and opens My progress.
Expect: Each dashboard has Molefe's shape and figures, with its own training year: Your targets opens "Training year N —
  it sets the minimum level each encounter is judged against.", as My progress does (T306), My authorisations reads "No
  STAR yet. When the committee issues one, it shows here against training year N's level.", and My progress's subtitle
  reads "Training year N · Semester 2, 2026". N is 3 for Dlamini, 2 for du Plessis and 1 for Mahlangu and Ndlovu. Their
  semester and yearly targets apply now: a start on 15 January is on time for both (D42).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Each dashboard has Molefe's shape and figures ("0 of 10", "0
  of 5", no part-way line, the same Needs you card), with its own year: Your targets "Training year N — it sets the
  minimum level each encounter is judged against.", My authorisations "… against training year N's level.", and My
  progress "Training year N · Semester 2, 2026": N = 3 (Dlamini), 2 (du Plessis), 1 (Mahlangu), 1 (Ndlovu). Each
  Entrustment summary reads "0 at or above · 0 below · 15 with no decision, of 15 EPAs".
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): All 14 opened My account from the top bar's name (aria-current
  page): "My account" and "Your name, your roles, and how you sign in."; the Account card shows the email as text (no
  input), Kgosi Kgari Teaching Hospital and the roles by label, one per line: "Committee member" and "Assessor" for
  Zulu, Naidoo and Botha, "Trainee" for the five registrars, and each other's one label; How you sign in shows
  Password with Change password. Khumalo: Fatma to Fatima, Save name: ?status=saved with "Name saved." focused; the
  top bar reads "Fatima Khumalo". At 390 px no sideways scroll (scroll width 390).
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Headed "Log an activity" with "Choose what you are filing.
  Each opens its own form."; eleven links, no name twice, in three groups: "Rated by an assessor" "7 types" with the
  quoted sentence and the seven, each "(Paediatrics)"; "Discussed or reviewed, not rated" "3 types" with its sentence
  and the three; "Logged by you" "1 type" with its sentence and KGK Teaching Session Log. Each link opened
  /activities/new?type=<key> with Choose another type, which led back to /activities/new every time. The seven rated
  forms each show 1 2 3a 3b 4 5; the other four none. No MSF, Learner Feedback or Demo type. None saved.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The Mini-CEX form opened at once, subtitled "Mini-CEX
  (Paediatrics) · Choose another type". Request open ("You fill this in"); Entrustment (the ladder) and Feedback each
  read "The assessor you name fills this in" and "Not filled in yet.". Above the actions, "When you submit: it goes to
  the Activity inbox of the assessor you name, and stays Requested until that assessor acts on it."; Submit, then Save
  draft, with "Save draft keeps it in My activities. It is in nobody's inbox until you submit it.". EPA: PAED-001,
  002, 003, 004, 006, 007, 008, 012, 013. Assessor: Botha, Khumalo (as Fatima), Naidoo, Patel, Zulu, each "(email)";
  nobody else. Left unsaved.
Gap: none

### Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/invitations/new → / → /admin/users → /admin/assessors
Do: Open Home and press Invite a person, then go back to Home. Then open Users and Assessors.
Expect: Home reads "Institutional admin · Semester N, YYYY" and offers "Invite a person", which opens Invite a person
  (`/admin/invitations/new`), not the list (T373, Q1). Users reads InstitutionalAdmin 1, SpecialityAdmin 1,
  SubSpecialityAdmin 1, Coordinator 1, CommitteeMember 4, Assessor 5 and Trainee 5, with no PendingTrainee line.
  Specialities & sub-specialities reads 1 and 1, what KGK has adopted. Quick links reads Users, Invitations, Curriculum
  adoptions and Entrustment decisions. The users and assessors lists both name Fatima Khumalo (Step 2.41).
Note: The adopted count is T291 item 4's decision, still queued: the query counts the whole national catalogue, which
  with the Demo world reads 2 and 2, so expect a Gap until T291 lands.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home, "Institutional admin · Semester 2, 2026", header action
  "Invite a person", which opened /admin/invitations/new (Invite a person); Home from the menu. Users 1/1/1/1/4/5/5,
  no PendingTrainee line; Specialities & sub-specialities reads 2 and 2; Quick links Users, Invitations, Curriculum
  adoptions and Entrustment decisions. Users lists 15 KGK accounts and Assessors five profiles, both naming "Fatima
  Khumalo".
Gap: [F-2.44a, T291] (still) Specialities & sub-specialities reads 2 and 2 (the whole national catalogue with the Demo
  world), not KGK's adopted 1 and 1: T291 item 4, still queued.

## Act 2 outcome state

Replay check (2026-10-20, T373 replay, wombat_scenario_t373): all 7 queries as expected: 15 users with the roles
listed; scopes mbatha, smit, vanrensburg 0 | 0, mokoena 1 | 0, the other eleven 1 | 1; five profiles active and pinned,
J-3y to J with the completion dates of Steps 2.29-2.30; assessor dates D-8y, D-6y and D-1 month from 2026-10-20; the
panel 1 zulu, 2 botha, 2 naidoo, 3 vanrensburg, Scope 2, no DecisionBodyKey; invitations 15 | 1 | 0; activities and
reviews 0 | 0. The log holds 23 stubs: 18 invitations (Act 1's two among them) and 5 admissions.

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
