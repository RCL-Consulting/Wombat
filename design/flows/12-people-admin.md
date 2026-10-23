# F12 The institution's people: users, roles, assessors, locks and resets

**Who and why:** the Institutional admin keeps her institution's people once they are in. She reads KGK's users, gives a
consultant a second role, records each assessor's training, resets a registrar's password, locks a consultant out and
lets him back in, keeps her own institution's record and nobody else's, and reads her SSO mappings. A role grant, a
scope and a lock decide who can see and do what; a record outside her institution is "not found", never "forbidden".
Seven pages carry it: Users, the user page, Assessors, the assessor profile form, her institution's record, SSO
mappings and her Home. None has been redesigned since flow 01 gave them the shell and flow 02 gave the user page's
Reset password its field, Show toggle and six rules; flow 11 changed the Home's header action and the invitations they
link to. The step captures are from flow 11's replay (2026-10-20), on the shell as built; the state captures are from
2026-09-26 and 09-27, mostly on the old frame.

Restated 2026-10-23 for T376 (the brief of 2026-09-26 predates flows 01–11, D54 and D55). Paths to screenshots are
under `design/baseline/` (gitignored). Code is cited at `98584921`; `W/` is `src/Wombat.Web/Components/Pages/`, `U/` is
`W/Admin/Users/`, `As/` is `W/Admin/Assessors/`, `A/` is `src/Wombat.Application/Features/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F/W** in BRIEF.md § 8: the lists (Users, Assessors) and the institution's record are expected to change little (F); the user page, which gathers roles, scope, sign-in, a reset, a lock and assessor training, and the Institutional admin's Home are not (W). |
| Mode | **Kept, read as flow 11 read it** (BRIEF.md § 2.4: an F page's structural round may be a single proposal to confirm). **Wireframe first for the user page with assessor training, and for her Home:** round 1 draws 2–3 STRUCTURES for `/admin/users/{UserId}` together with where an assessor's training lives (Q1, Q2: sections or tabs; the profile on the user page or on Assessors' own form), and 2–3 for the Institutional admin's Home (Q3, Q7, Q8: what it counts, the waiting line, the way to her own record). **Straight to fidelity for Users:** 2–3 variations (columns, the filter, a pager, stacking at 390; Q6). **One proposal at fidelity each** for her institution's record (its shape fixed since T302: three fields, Status as text) and SSO mappings with no provider (read-only, its words T326's), to confirm or correct. Assessors' list is drawn inside each user-page structure, since Q2 decides whether it keeps its own form. Round 2: the picks with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 structures (wireframes) for the user page with assessor training and for Home; 2–3 variations (fidelity) for Users; one proposal each for the institution's record and SSO mappings; the chosen ones in round 2 |
| People | InstitutionalAdmin: Prof Nolwazi Mbatha (KGK, Kgosi Kgari Teaching Hospital). Her subjects (`README.md` § Cast): Dr Thandi Zulu, Dr David Naidoo and Dr Sarah Botha (CommitteeMember, each gains Assessor, 2.13); Dr Mohammed Patel and Dr Fatima Khumalo (Assessor; Khumalo registered as "Fatma" and corrected her name at 2.41); the five assessor profiles (2.14); Dr Pieter du Plessis (withdrawn at 5.27, no role since D54; his password reset at A.4.5); Dr Mohammed Patel (locked out, A.6.4, and reactivated, A.6.7); Dr Anele Dlamini (her account opened on the phone, A.7.9); Dr Sipho Ndlovu (erased, A.1.14); Dr John van Rensburg (external CommitteeMember, `sun.wombat.local`, never an Assessor); Mr Pieter Smit (Coordinator), Dr Refilwe Mokoena (SpecialityAdmin), Dr Kabelo Sithole (SubSpecialityAdmin), Dr Lerato Molefe, Dr Nomsa Mahlangu (Trainees). Another institution: the Demo Institution and the dev trainee `trainee@wombat.local` (A.5.5, by id). Administrator: `devadmin@wombat.local` ("Demo Administrator"), admitted to every page here (flow 18 owns his views) |
| Runbook steps | 15, pasted verbatim in § 8: 1.23 (Act 1), 2.12–2.15 and 2.44 (Act 2), A.1.14, A.3.1, A.4.5, A.5.5, A.6.3, A.6.4, A.6.7, A.7.9 and A.7.14 (appendix). A.1.14 crosses flow 07's Committee reviews and flow 11's Trainees; A.7.9 crosses flow 11's Trainees and flow 17's builder; A.7.14 crosses flows 01, 02 and 07 |
| Pages (`coverage.md` templates) | `/admin/users` (`U/UsersList.razor`, 118 lines), `/admin/users/{UserId}` (`U/UserDetail.razor`, 430: Account summary, Roles with Add role, Reset password, Lockout, Pending invitations; words in `A/Users/UserAdministrationRules.cs`, 225), `/admin/assessors` (`As/AssessorsList.razor`, 78), `/admin/assessors/edit` (`As/AssessorProfileEdit.razor`, 240: new with no id, an existing profile with `?id=`), `/admin/institutions/{Id:int}` (`W/Admin/Institutions/InstitutionEdit.razor`, 384, shared with the Administrator's create and edit, flow 18's; `/admin/institutions/new` is on the same page), `/admin/sso/group-mappings` (`W/Admin/Sso/GroupMappings.razor`, 319), `/` (her Home: `W/Dashboards/InstitutionalAdminDashboard.razor`, 61, in `DashboardFrame`, header action "Invite a person", `Nav/HomeFrame.cs:53`). Every page but Home is `AdministratorOrInstitutionalAdmin`; Home's card set is `Roles = "InstitutionalAdmin"`. Since `b347e11c` (flow 01) only `f50dffb2` (flow 02: the reset card's PasswordField and rules), `f3471780` (T290: the trainee-first refusals in the Users commands) and `69a17c70` (flow 11: the "Admit…" help words, GetUserById's invitation read) touched them. |
| Menus (`Nav/NavItems.cs`) | Her menu (`:150–156`): Home; People: Invitations, Trainees, Assessors, Users; Curriculum: …; Reviews: …; Access and audit: SSO mappings, Audit log; then My data rights. The Administrator's (`:144–149`): Users and Invitations under People, SSO mappings under Platform; no Assessors item, though both assessor pages admit him. The user page lights Users for both, trail Home › Users › ‹name› (`Nav/NavOwners.cs:157`); the assessor form lights Assessors for her only (`:164`). Her institution's record is owned by Institutions for the Administrator alone (`:150`), so for her nothing is lit and the trail is Home › Edit institution (observed, A.6.3); no menu item or Home link leads there (`coverage.md` § Reached only by address). Someone who holds Trainee beside an admin role sees a note in place of Users and the user page (T278, `U/UsersList.razor:13–16`, `U/UserDetail.razor:18–21`). |
| Components drawn at runtime | `PageHeader` (the user page and the assessor form each with a "Back to …" header action beside the trail, `U/UserDetail.razor:11`, `As/AssessorProfileEdit.razor:11`; Assessors with "Create assessor profile", `As/AssessorsList.razor:11–14`; the institution's record with "Back to home" for her), `StatePanel` (no `OnRetry` on any of these pages; the institution's record has none for a failed read), `DataTable` (captioned, no `Stack`/`StackWide`: Users `U/UsersList.razor:35`, the user page's pending invitations `U/UserDetail.razor:184`, Assessors `As/AssessorsList.razor:20`, mappings `GroupMappings.razor:125`), `.details-grid` of `section.detail-card`s with `<h3>` titles (the user page's five cards; the mappings page's two), `ActionResult` with `Alert`s (every result and refusal at the page's top), `FormField`, `FormActions`, `FieldHelp`, `EditForm` with `DataAnnotationsValidator` and `ValidationSummary` (the assessor form, the institution's record, the mapping form), `InputSelect`/`InputDate`/`InputTextArea`, `PasswordField` with `PasswordRules` (the reset card), `InFlight`, `RowNames.Distinct` (Manage, Edit and Delete named per row, T239), `ConfirmDialog` (the Administrator's Deactivate only). Her Home: `DashboardFrame` with Try again, three `DashboardCard`s (Users by role, Specialities & sub-specialities, Quick links). Queries: `ListUsersQuery`, `GetUserByIdQuery`, `ListAssessorsForSpecialityQuery`, `ListAssessorUsersQuery`, `GetAssessorProfileByIdQuery`, `GetInstitutionByIdQuery`, `GetInstitutionsListQuery`, `GetSpecialitiesListQuery`, `GetSubSpecialitiesListQuery`, `ListSsoGroupMappingsQuery`, `GetInstitutionalAdminDashboardSummaryQuery`; commands `AddRoleToUserCommand`, `RemoveRoleFromUserCommand`, `ResetUserPasswordCommand`, `SetUserLockoutCommand`, `RevokePendingInvitationsForEmailCommand` (`A/Users/`), `CreateOrUpdateAssessorProfileCommand` (`A/Assessors/`), `UpdateInstitutionCommand` (`A/Institutions/`), `CreateSsoGroupMappingCommand`, `DeleteSsoGroupMappingCommand` (`A/Sso/`). No mail: a reset, a role and a lock send none. |
| Held | None. Every step capture is from the T373 replay (2026-10-20), on the shell as built. The `states/` captures of Users, the user page, Assessors, the assessor form, the institution's record and SSO mappings are from 2026-09-26, before flow 01 (a flat Title Case sidebar, an email in the top bar, red `btn-danger` buttons: checked by opening `user-detail--pending-invitations`); `home--institutional-admin.png` and `--narrow-institutional-admin.png` (2026-09-27) are on the shell. `act-A/A.6.4-1-patel-before-lock.png` is Dr Patel's own Activity inbox in his second browser, not his user page: the old brief cited it as the user page, and it is not attached. The step captures stand in where there is one (§ 3) |
| Depends on | Flow 01, built (`b347e11c`): the shell, the trail and lit item, Home's frame and header action, access denied, not found, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again, the tokens' contrast (T322, A.7.14). Flow 02 (`f50dffb2`): `PasswordField` with Show and the six rules, a reset refused for its rules marking the field (A15); My account and Change password (her own account's link). Flow 03 (`725237ee`) and 04 (`06aa51d7`): row actions named per row. Flow 05 (`b020c942`): ISO dates (D1), stacked rows below 641 px. Flow 06 (`85d5a508`): the staff Homes and their cards; filters applied with Show. Flow 08 (`622c5ad1`): one action a row; the pager's focus on the list's heading. Flow 09 (`49e7cf5e`): `ConfirmDialog` opening on Cancel; a page of its own per list row. Flow 10 (`68327c9d`): a create page of its own reached from a header action. Flow 11 (`69a17c70`): the list is the page and creating is a page of its own (Invite a person); `StackWide`; a role chosen from described radios; the required mark; only the chosen role's fields; refusals under their field; Trainees and Admit ‹name›; the issue refusing an address that has an account. D54 (`f3471780`): a withdrawn or completed registrar holds no role. D55 (`fb6e465e`): only a registrar whose programme is running files. The design system is **version 29**, re-synced to flow 11 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `powershell -File design/tools/stage_upload.ps1 -Flow 12`. Open each screenshot. The
   script puts any capture whose name contains "invit", "issued", "resent", "resend" or "reset" in
   `design/upload/crop-first/`. § 4 names three of these: `act-A/A.4.5-0-reset-card.png`, `act-A/A.4.5-1-reset-refused.png`
   and `act-A/A.4.5-2-reset.png`; the last two were opened on 2026-10-23 (the field empty in both, Show not pressed,
   no password on screen) and go as they are; open the first before it goes. `act-2/2.12-1-invitations-none.png` is
   flow 11's page and is not attached.
2. **Start a new canvas** from the main app's Design page, "Wombat 12", on the Wombat design system v29
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Write its `tokens.css` by hand
   from app.css's `:root` before round 1 (§ 11, flow 09: the system serves only `tokens.json`).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). The canvas is https://claude.ai/artifact/EUfPe9d3zgnGxvQ9LeSzz8 and its mark `/_blob/b8532f490e278114d4c3b43f9a0533f6`,
   written into § 1 and `design/flows/12-people-admin/round-1-ask.txt`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/12-people-admin/round-1-ask.txt`), with the key screenshots in
   § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's notes for a line
   saying it never received the attachments (§ 11, flow 06); re-attach and name the folder before round 2 if so. Say
   again that every person, address and value comes from § 1 and § 8 (flow 09's round 1 invented people).
5. **Pick a structure for the user page with assessor training and for Home, a variation for Users, and confirm or
   correct the institution's record and SSO mappings,** say why in a sentence, and answer § 6's questions one sentence
   each; the operator's are marked. A question whose answer rests on an open task names it, and the task lands before
   the build (§ 11, flow 05): here T288, T333 and T289 (Q11, Q12). Ask for round 2 at fidelity, with its Spec and Steps
   boards.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/11-onboarding/round-2-review.md` did:
   one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system; the round's
   check reads the boards' stacked rows against DESIGN.md as well as the ask (§ 11, flow 11). Put the decisions to the
   operator, then ask for one correction round.
7. **Record every round** in `design/flows/12-people-admin/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 12 — Keep my institution's people: their roles and scope, assessor training, passwords and access

This is flow 12 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns) and 11 (invitation to admission) are designed and built; every page here sits
  inside the shell. Round 1: 2–3 STRUCTURES as wireframes for the user page together with where an assessor's
  training lives, and 2–3 for the Institutional admin's Home; 2–3 variations at FULL FIDELITY for Users; ONE proposal
  at fidelity each for her institution's record and for SSO mappings, to confirm or correct. I pick before round 2.
  The runbook steps follow below in this same message. The brand mark is at /_blob/b8532f490e278114d4c3b43f9a0533f6: use it, never a drawn disc.
  Every person, address, role, date and count comes from this message and the steps below; invent none.

GOAL: The Institutional admin must be able to:
    - find any of her institution's people by name or address, and see each one's roles and state at a glance;
    - open one person and read who they are, what they may do and where (their roles, by label, with the scope each
      carries), and how they sign in;
    - give a consultant a second role, with the scope that role takes, and take a role away, offered only what she may
      grant, and told what each did where her focus lands;
    - record each assessor's training (status, completion date, qualifications), and read every assessor's at once;
    - reset a password, knowing the person is not emailed and that their open sessions end;
    - lock a person out, after reading what a lock does and confirming it, and let them back in;
    - read and correct her own institution's name, short code and contact email, reached from her own pages, and
      never another institution's (Page not found);
    - read her SSO mappings and be told in her own terms when institutional sign-in is not set up.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-10-20):
  - The user page is a grid of five cards (Account summary, Roles, Reset password, Lockout, Pending invitations), each
    titled <h3>; the email is cut at the card's edge ("duplessis@kgk.wombat.lc", A.4.5); each role's name runs into its
    red Remove ("AssessorRemove", A.6.4) or its note ("TraineeSystem-managed", A.7.9); at 390 the five cards stack into
    one long page.
  - Lock out user is a red button that locks at once, with no confirmation (A.6.4); each role's Remove and "Revoke all
    pending invitations" are red and act at once too.
  - Roles print as code names everywhere on these pages: the Users list ("CommitteeMember, Assessor"), the user page and
    its Add role picker, the result ("Role 'Assessor' added."), Home's Users card ("InstitutionalAdmin 1"), the SSO
    picker. Add role grants a role with no scope, and the page never shows the scope a person holds.
  - The user page's subtitle reads "Roles, password reset, lockout, and pending-invitation cleanup for a single user.";
    a withdrawn registrar's page says only "This user has no roles." (D54, A.4.5); Pending invitations can no longer hold
    anything (flow 11 refuses an invitation to an address that has an account).
  - Users is one table in a card with a Filter that filters per keystroke, an Institution column that reads "Kgosi Kgari
    Teaching Hospital" on every row, no pager, and at 390 the table scrolls sideways in its box (807 px, A.7.9).
  - The assessor form's empty save reads "The UserId field is required." (2.14); it stays on its new form after a save
    and offers the already-profiled assessors again; its Speciality offers the national catalogue with "All"; the
    empty list says a profile makes assessors "selectable in the workflow", which it does not.
  - Home counts "Specialities & sub-specialities" as 2 and 2, the national catalogue, where KGK has adopted 1 and 1
    (2.44); nothing tells her that a registrar is waiting to be admitted; her institution's record is reached only by
    typing its address.
  - SSO mappings with no provider tells her to edit "the Sso:Providers section in application settings and restart" and
    its empty state says "Add a mapping above" where no form is drawn (A.3.1).
  - A failed read on Users, the user page, Assessors, the assessor form or SSO mappings prints the exception's own text,
    with no Try again; the institution's record then draws an empty form with Save.

AUDIENCE:
  - InstitutionalAdmin: Prof Nolwazi Mbatha, head of department, Kgosi Kgari Teaching Hospital (KGK). At a desk,
    sometimes on her phone (A.7.9). Her institution: 15 accounts after onboarding (one InstitutionalAdmin, one
    SpecialityAdmin, one SubSpecialityAdmin, one Coordinator, four CommitteeMembers, five Assessors, five Trainees;
    Zulu, Naidoo and Botha hold both CommitteeMember and Assessor); five assessor profiles; KGK has adopted the
    College's (CPSA) Paediatric EPA Curriculum 11.1, so one speciality and one sub-speciality (Paediatrics /
    Paediatrics).
  - The people she manages (README.md § Cast): Mr Pieter Smit (Coordinator); Dr Thandi Zulu, Dr David Naidoo, Dr Sarah
    Botha (CommitteeMember and Assessor); Dr Mohammed Patel and Dr Fatima Khumalo (Assessor); Dr John van Rensburg
    (CommitteeMember, external, vanrensburg@sun.wombat.local, no speciality); Dr Refilwe Mokoena (SpecialityAdmin,
    Paediatrics); Dr Kabelo Sithole (SubSpecialityAdmin, Paediatrics / Paediatrics); Dr Lerato Molefe, Dr Anele
    Dlamini, Dr Pieter du Plessis, Dr Nomsa Mahlangu, Dr Sipho Ndlovu (registrars; du Plessis withdrawn and Molefe
    completed, each then with no role, D54; Ndlovu erased, A.1.14). Addresses are <surname>@kgk.wombat.local.
  - Assessor training (Step 2.14): each at KGK, Paediatrics / Paediatrics, qualifications "MBChB, MMed (Paed), FC Paed
    (SA)"; Zulu Trained, completed D−8 years; Naidoo Trained, D−6 years; Botha Trained, no date; Patel In training;
    Khumalo Provisional (awaiting sign-off), D−1 month.
  - Frequency: monthly (rotations, staff joining, lockouts, forgotten passwords). Stakes: high. A role, a scope or a lock
    decides who sees what. Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Users (/admin/users; "Manage roles, passwords, and access for all users across the institutions you
    administer."): Filter ("Search by name or email"), then Name, Email, Institution, Roles, Status ("Active" or
    "Locked out"), Manage named for its row ("Manage Thandi Zulu"); sorted by last name. An Institutional admin lists
    her institution's accounts only (T056): no devadmin, no Dr Kruger, no Demo account. "No users match the current
    filter."; "No users" / "No user accounts exist within your scope.". An erased registrar is gone from the list.
  - The user page (/admin/users/{UserId}, titled by the person's name, trail Home › Users › ‹name›, Back to users):
    Account summary: Name, Email, Institution, Status. Roles: each with Remove ("Remove the Assessor role"); Trainee and
    Pending trainee read "System-managed", no Remove (T303); "This user has no roles." Add role: offers Institutional
    admin, Speciality admin, Sub-speciality admin, Coordinator, Committee member and Assessor, less what the person
    holds; never Administrator, College admin, Pending trainee or Trainee; help "Trainee is not offered: a registrar
    becomes a trainee only when admitted, from Trainees with 'Admit…'."; result "Role 'Assessor' added." / "Role
    'Assessor' removed.". A role change, a reset and a lock end the person's open sessions within a minute (T279).
    Reset password: "Sets a new password directly. The user is not emailed; share the password securely out-of-band.";
    New password with Show and the six rules ("The new password needs:", "At least 12 characters.", "At least 4
    different characters.", "A digit (0 to 9).", "An upper-case letter.", "A lower-case letter.", "A symbol, such as ! or
    #."); enabled once anything is typed; refused: "The password was not reset. The new password needs: At least 12
    characters. An upper-case letter. A symbol, such as ! or #." with the field marked "The new password does not meet
    the rules below."; done: "Password reset. Share the new password with the user out-of-band."; the field cleared
    either way. Lockout: "Locking out the user prevents sign-in immediately. Until they are reactivated:" "They sit on no
    committee panel." "They are not a current trainee. …" "They are sent no activity reminders or weekly digests." "They
    cannot be named as an activity's assessor." "Their records stay as they are, and a review or feedback campaign
    already under way can still be finished. Reactivating clears the lockout."; Lock out user → "User locked out.",
    Status "Locked out", "This user is currently locked out and cannot sign in.", Reactivate user → "User reactivated.";
    an Administrator's account: "Global administrators cannot be locked out from this surface.". Pending invitations:
    "No active invitations are outstanding for this email." or a table (Role, Institution / College, Speciality,
    Issued, Expires) with "Revoke all pending invitations" → "Pending invitations revoked.".
    Her own account: subtitle "Your own account's summary, roles and pending invitations."; "This is your own account,
    so you cannot change its roles, lockout or password here. Another administrator can change your roles or lockout.
    Change your password from your account." (the link to Change password); her roles with no Remove; no Add role,
    Reset password or Lockout (T278).
    Out of scope or unknown: "User unavailable" / "The user could not be found or is outside your scope.", trail Home ›
    Users › User (A.5.5): never a "no access" variant.
    Someone who holds Trainee beside an admin role: "You hold the Trainee role, so you cannot view or change user
    accounts, including your own." in place of Users and the user page.
  - Assessors (/admin/assessors; "Create and maintain assessor profiles across institutions and specialities.";
    header action "Create assessor profile"): Name, Email, Institution, Speciality, Sub-speciality ("All" when none),
    Training status (Not started, In training, Provisional, Trained), Training completed (a date or "Not recorded"),
    Edit named for its assessor. "No assessor profiles" / "Create the first assessor profile to make assessors
    selectable in the workflow.". Only Assessor-role holders have a profile; van Rensburg is never listed or offered.
  - The assessor form (/admin/assessors/edit, ?id= for an existing one; "Assessor profile" / "Assign the assessor to an
    institution and optional speciality scope."; trail Home › Assessors › Assessor profile; Back to assessors):
    Assessor user* (the institution's Assessors, each "Thandi Zulu (zulu@kgk.wombat.local)"; fixed once
    saved), Institution* (hers alone), Speciality ("All" or one), Sub-speciality
    ("All", disabled until a speciality), Qualifications*, Assessor training status* (Not started, In training,
    Provisional (awaiting sign-off), Trained; help "Faculty-development lifecycle. In-training assessors may assess
    under supervision."), Assessor training completed (only for Provisional or Trained; "Date the assessor completed
    assessor training. Leave blank if unrecorded."), Cancel, Save assessor profile → "Assessor profile saved.".
    Refusals: "A sub-speciality requires a speciality.", "Only users in the Assessor role can have an assessor
    profile.", "The selected sub-speciality does not belong to the selected speciality.", "You do not have permission
    to manage assessors in that institution."; unknown id "The assessor profile could not be found.". Saving a profile
    sets the person's speciality and sub-speciality scope to the profile's (T289).
  - Her institution's record (/admin/institutions/{Id}, by its address only; "Edit institution" / "Maintain the
    institution record used for scoping and onboarding."; Back to home): Name*, Short code*, Contact email; Status as
    text, "Active" over "Set by a global administrator." (T302); Cancel (to Home), Save → "Institution saved.". No
    Active box, no Deactivate, no link to the Institutions list (the Administrator's). Another institution's id: Page not
    found. The Administrator's own view (the box, Deactivate in a ConfirmDialog, the list) is flow 18's: keep it.
  - SSO mappings (/admin/sso/group-mappings; "Map external identity provider groups to Wombat roles. Changes take effect
    on the next login for each user."): with no provider, a card "No SSO providers are configured. Add providers to the
    Sso:Providers section in application settings and restart." and Current mappings "No group mappings" / "Add a
    mapping above to link external groups to Wombat roles."; nothing can be changed. With a provider (not reachable on
    the replay): Add mapping (Provider, External Group ID, Group Display Name, Wombat Role, Institution, Speciality,
    Sub-speciality), "Mapping added for group '…'."; rows (Provider, External Group, Wombat Role, Institution,
    Speciality, Sub-speciality, Delete named "Delete the mapping of ‹group› to ‹role›") → "Mapping deleted.". The
    Administrator role can never be mapped.
  - Her Home (/; "Institutional admin · Semester 2, 2026", header action "Invite a person"): Users (a count per role,
    roles that have none left out; locked accounts counted), Specialities & sub-specialities (two figures), Quick links
    (Users, Invitations, Curriculum adoptions, Entrustment decisions); its load error a sentence and Try again.
  - Data volumes: none (her first sign-in: one user, herself; no assessor profile; no mapping); typical (15 users, five
    assessor profiles, three people holding two roles); heavy (about 200 accounts over several intakes, a person holding
    four roles, an address of 60 characters, a dozen locked accounts, 30 assessor profiles).

WHAT FLOW 12 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the trail, the acting role, access denied, not found, Home's frame and header action);
    the menus' items (propose, do not draw as settled); the reset card's PasswordField, its Show toggle and six rules
    (flow 02); My account and Change password (flow 02); Invitations, Invite a person, Trainees and Admit ‹name› (flow
    11); the Administrator's institution pages (flow 18); the rules above (who may grant what; Trainee and Pending
    trainee system-managed, T303; nobody changes their own roles, lockout or password, T278; nobody who holds Trainee
    administers a user; an out-of-scope record is not found; the Administrator role never granted here or by SSO; the
    institution's state the Administrator's alone, T302).
  May, for these pages: their whole layout (the user page as sections or tabs, where scope, sign-in links and assessor
    training sit, Users' columns and filter and how rows stack, what Home counts), the order and grouping of parts,
    titles and subtitles, where each result and refusal shows and where focus goes, confirmations, stacking at 390, and
    the words. Mark PROPOSED any rule change (a refusal that is not built, a new route, a new action, data a page does
    not read today): those are mine. The runbook's Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. Her Home (InstitutionalAdmin): onboarded (2.44: the counts by role, the adopted count 1 and 1); her first sign-in
     (one user, nothing adopted, 0 and 0); five registrars waiting to be admitted (between 2.18 and 2.29; Q8); loading;
     a load error; 390 px (A.7.9).
  2. Users (InstitutionalAdmin): KGK's 10 staff (2.12); filtered "zulu" (2.12); no match; 15 accounts with five pending
     registrars (2.28); two roles a row (2.13); a locked account (A.6.4); after the erasure, 14 (A.1.14); the heavy
     volume with a pager; loading; a load error; the Trainee's note; 390 px (A.7.9).
  3. The user page (InstitutionalAdmin): her own (2.12); Dr Zulu before and after Add role (2.13), with the scope the
     role takes; a role removed; Dr Patel before the lock, the lock's confirmation, locked, reactivated (A.6.4, A.6.7);
     Dr du Plessis with no role (D54), his reset refused, then done (A.4.5); Dr Dlamini at 390 (A.7.9); a pending
     registrar (2.28, flow 11 reads it); an assessor's training on it if a structure puts it there (Q2); institutional
     sign-in links (Q10); out of scope (A.5.5); loading; a load error; the Trainee's note.
  4. Assessors and the assessor form (InstitutionalAdmin), drawn inside each structure of screen 3: none (2.14); the new
     form, its empty save refused, Dr Khumalo Provisional with the date, saved (2.14); five (2.15); an unknown id;
     loading; a load error; 390 px.
  5. Her institution's record (InstitutionalAdmin): KGK's (1.23), saved with its new contact email (A.6.3), the Demo
     Institution's id (Page not found, 1.23, A.5.5), a required field empty, 390 px; and how she reaches it (Q3).
  6. SSO mappings (InstitutionalAdmin): no provider (A.3.1), at 1280 and 390; and, from the text, with a provider: the
     add form with her institution fixed and no Trainee or Pending trainee offered (T288, T333), a row, its delete
     confirmation.

STEPS: 1.23, 2.12–2.15, 2.44, A.1.14, A.3.1, A.4.5, A.5.5, A.6.3, A.6.4, A.6.7, A.7.9 and A.7.14. Pasted verbatim
  below (Role / Route / Do / Expect). In them, D is the day the story is replayed. Where an Expect quotes today's
  wording or layout, that is today's page, not a requirement on the new one. A.1.14's Committee reviews are flow 07's,
  its Trainees flow 11's; A.7.9's Trainees are flow 11's and its builder flow 17's; A.7.14's sign-in, My account and
  committee pages are flows 01, 02 and 07's (keep them; draw only what is on these pages).

STATES TO SHOW:
  Every state named in SCREENS; loading for every page (the header and a skeleton, no action offered until the read is
  in); a load error (a sentence and Try again, never an exception's text); each confirmation, its cancel, and the
  result after it with the focus on it; a refusal beside the field that caused it, keeping what was typed (never a
  password); 390 px for every page; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T264 (the user page's and SSO's half): Lock out user asks first, in a ConfirmDialog naming the person and saying
    what the lock does, its trigger an outline button; a role's Remove and a mapping's Delete are outline buttons named
    for their row and confirm, naming the person and the role (or the group and the role); no destructive button on a
    record already in that state.
  - T323 (the users half): at 1280 an address, a long name and a role list wrap inside their column, never cut; a gap
    before each role's action or note; at 390 Users and Assessors stack (flow 11's StackWide), nothing scrolls sideways.
  - T324 (the users half): roles by their labels ("Committee member", "Institutional admin", "Pending trainee") on every
    page here, Home's counts, the results and the row names included; no message names a model property ("The UserId
    field is required." becomes the field's own words).
  - T326 (the assessors and SSO items): the empty Assessors list says what a profile records (training status and date,
    qualifications); an assessor is offered as a nominee by the Assessor role alone (NomineeDirectory). SSO with no
    provider points at no form, and tells an Institutional admin, in her terms, that institutional sign-in is not set
    up and to ask a Wombat administrator; the Administrator keeps the setting's name.
  - T289 (lands first, Q12): the assessor form refuses someone who holds Trainee and anyone not at her institution;
    after a save the page is the saved profile's; a new form offers only assessors with no profile; a save changes only
    the scope the profile contributes.
  - T288 and T333 (land first, Q11): once a provider exists, a mapping grants roles only at her institution (no
    institution to pick), and never Trainee or Pending trainee.
  - T291 item 4: Home's adopted count is what KGK has adopted (1 and 1 after 1.21; 0 and 0 before). Item 8: the create
    form at /admin/institutions/new is never drawn for her.
  - T286 (the external-logins half, Q10): the user page lists the person's institutional sign-in links, with Remove.
  - T280: one <h1>, then <h2> in order (the cards are <h3> under the <h1> today); a table's caption does not repeat the
    page's heading ("Users" under "Users").
  - T325 (A9): every time in South African time with the zone; dates on the South African calendar.
  - T329/T272's family: a failed read is fixed words with Try again; no exception's text on any page; the institution's
    record draws no form it could not read.
  - A5 (BRIEF.md § 6): every action says what it did where the focus lands, naming the person ("Assessor added to Thandi
    Zulu."); a refusal beside its field, keeping what was typed.
  - A10/A11: each page's subtitle says what it does for her ("across the institutions you administer" is one
    institution); offer only what the caller can do.
  - Done since the old brief, keep them: T302 (Status as text, no Deactivate for her, Back and Cancel to Home); T303
    (Trainee system-managed, never offered by Add role); T278 (her own account: no Remove, Add role, Reset or Lockout;
    the Trainee's note); T284 (the lock's effects listed); T317's reset half (flow 02: no minlength, enabled once
    anything is typed, one sentence per broken rule); T322 (the tokens' contrast, A.7.14); T321 (access denied drawn once,
    flow 01; Back to institutions went with T302); D54 (a withdrawn or completed registrar holds no role).

QUESTIONS THE DESIGN MUST ANSWER:
  1. The user page: one page with sections (who they are and their scope; roles; sign-in and security: reset, lock,
     sign-in links; assessor training) or tabs? Which section comes first for her most frequent task?
  2. Assessor training: on the user page as a section for anyone holding Assessor, with Assessors as the list across
     them (its Edit opening that section), or Assessors' own form as today?
  3. Her own institution: how she reaches its record (a Home card or link, the page titled by its name), with no menu
     item drawn as settled.
  4. Role and scope: how the page shows the scope each role carries, and whether Add role asks the scope the role
     takes, by Invite a person's rules (PROPOSED).
  5. The lock: what the confirmation says, where the result shows, and what the locked page reads (Status, the lock's
     effects, Reactivate user).
  6. Users: its columns (Institution only for a caller whose reads span institutions), the filter applied with Show or
     per keystroke, a pager, how rows stack at 390.
  7. Home: what it counts (roles by label, the adopted count), whether Quick links stay beside the menu, where her
     institution's record and the waiting line sit.
  8. (Mine) Tell her a registrar is waiting: a "Waiting to be admitted: 5" line linking to Trainees (flow 11's Q8,
     PROPOSED there for this flow to place)?
  9. (Mine) Pending invitations on the user page: drop the card and Revoke all, now that an invitation to an address
     with an account is refused at issue?
  10. (Mine) Institutional sign-in links (T286): list them on the user page with Remove, and does a reset drop them?
  11. (Mine) SSO mappings: T288 and T333 land before the build, and the page is read-only for her until a provider
      exists?
  12. (Mine) Which record owns an assessor's discipline (T289): the profile adds and removes only its own scope?
  13. (Mine) A person with no role (a graduate or a withdrawn registrar, D54): what the page says, and what it offers?
  14. (Mine) Role labels on every page here, and the Expects that quote keys change?
  15. (Mine) A person's name or address: corrected on the user page, or only by the person (name, My account) as today?
  16. (Mine) What the build folds in.

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
  Since flow 02 also: the auth card, PasswordField with its Show toggle and PasswordRules. Since flow 03:
  RefusalSummary, DataTable's stacked rows (Stack). Since flow 05: ISO dates, the training year's wording. Since flow
  06: the role Homes; filters applied with Show. Since flow 07: the container-query stack. Since flow 08: one action a
  row, a choice of a few kinds as radios (choice-group), a form page of its own (Schedule a review), the pager's focus
  on the list's heading. Since flow 09: a page of its own per list row; a dialog with a field. Since flow 10: a create
  page of its own reached from Home's header action (New MSF campaign). Since flow 11: the list is the page and its
  create form a page of its own (Invite a person); DataTable's StackWide (stacked from 900 px down); a role chosen from
  described radios; the required mark; only the chosen role's fields drawn; a value that follows another as an
  <output>; refusals under their field, focused.
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
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. A password is never drawn: the reset field is empty or masked (••••••••••••) on every board.

ASK:
  - Round 1: 2–3 structures as wireframes for screens 3 and 4 together (Prof Mbatha's user page at 2.12, 2.13, A.4.5,
    A.6.4 and A.6.7, with Dr Khumalo's and Dr Zulu's training at 2.14), and for screen 1 (her Home at 2.44), each with
    its reasoning and its step count; 2–3 variations at full fidelity for screen 2 (Users at 2.12, 2.13 and A.7.9);
    ONE proposal at fidelity each for screen 5 (KGK's record at 1.23 and A.6.3) and screen 6 (A.3.1), to confirm or
    correct. At 1280 and 390 px. With them, two boards of their own: a Spec board (every component each variation
    uses, NEW marked; the DESIGN.md rules each changes; everything PROPOSED; the data a page would need that it does not
    read today, marked DATA) and a Steps board (each variation's journeys counted, presses + fields typed, at 1280 and
    390, and which of the 15 Expects below it would change). Stop and wait for my pick.
  - Round 2: the chosen designs with every screen and state above, at 1280 and 390 px, every confirmation, and where
    the focus lands after each action; with two boards of their own:
    - a Spec board: every NEW and changed component, its classes, the DESIGN.md text each changes, the tokens, the
      data each page needs that it does not read today (marked DATA), and every message by kind (result, refusal,
      hint, notice, empty) with its words;
    - a Steps board: each of the 15 runbook steps below, and what its Expect will say once built (quoting today's words
      and giving the new ones; "unchanged" where so), with each journey's count (presses + fields typed) at 1280 and
      390: finding a person; adding a role with its scope; recording one assessor's training; resetting a password;
      locking out and reactivating; correcting the institution's contact email.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Table system; § Form system; § Button system's destructive action; § Page-level patterns: List page,
    Form page, Detail page and the title rule; § Dashboard page; § Alerts).
  - Mark PROPOSED anything not built: a confirmation, a new route, a new action, a refusal, a rule change.
  - Flag edge cases: a 60-character address; a person holding four roles; her own account; an Administrator's account
    opened by her by id (not found); a locked person who is also the assessor of a request waiting; a person with no
    role; an assessor whose Assessor role is removed (the profile's fate); a Speciality admin added with no speciality;
    two people with one name; the Demo Institution's id; a provider configured for another institution.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, focus after every action and confirmation, refusals tied to their fields
    (aria-describedby), targets ≥ 24 px (44 at 390).

ATTACHED: act-A/A.6.4-2-patel-locked.png, act-A/A.4.5-1-reset-refused.png, act-A/A.7.9-3-user-390.png,
  act-2/2.12-2-users-list.png, act-A/A.7.9-2-users-390.png, act-2/2.44-1-mbatha-home.png, act-2/2.14-2-empty-save.png,
  act-2/2.15-1-assessors-five.png, act-A/A.3.1-1-sso-no-provider.png and act-A/A.6.3-1-contact-saved.png. Every other
  capture dated 2026-09-26 is an older frame (a Title Case sidebar, an email in the top bar, older button colours): read
  its body only. No attached capture shows a password.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 1.23 | `/admin/institutions/{Id:int}` → `/not-found` | Prof Mbatha | KGK's record by its typed address ("Edit institution", trail Home › Edit institution, nothing lit); Status as text; no Deactivate, no box, no link to the Institutions list; the Demo Institution's id: Page not found. **Typed, `/admin/institutions/new` still draws the create form (T291 item 8)** |
| 2.12 | `/admin/invitations` → `/admin/users` → `/admin/users/{UserId}` | Prof Mbatha | ten rows, KGK's only; the filter leaves Zulu; her own page: the note, Change your password, no Remove, Add role, Reset or Lockout. **Roles as keys ("InstitutionalAdmin")** |
| 2.13 | `/admin/users` → `/admin/users/{UserId}` | Prof Mbatha | Add role without Trainee and its help; the Lockout card's effects; "Role 'Assessor' added." focused; both roles listed. **Keys in the picker, the list and the result; Remove red and touching the role's name; no scope shown or asked** |
| 2.14 | `/admin/assessors` → `/admin/assessors/edit` → `/admin/assessors` | Prof Mbatha | "No assessor profiles"; KGK's five Assessors offered, KGK alone; the date only for Provisional or Trained; "Assessor profile saved.". **The empty body's "selectable" (T326); "The UserId field is required." (T324); no move to `?id=`, the picker never narrows (T289)** |
| 2.15 | `/admin/assessors` | Prof Mbatha | five rows, statuses and dates ("Not recorded"); Edit named per assessor; van Rensburg absent |
| 2.44 | `/` → `/admin/invitations/new` → `/` → `/admin/users` → `/admin/assessors` | Prof Mbatha | "Institutional admin · Semester 2, 2026" and Invite a person (flow 11's page); Users by role with no PendingTrainee line; Quick links; Fatima Khumalo named on both lists. **The adopted count reads 2 and 2 (T291 item 4); roles as keys** |
| A.1.14 | `/admin/users` → `/admin/trainees` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Prof Mbatha | no Dr Ndlovu among KGK's users (14) or trainees; his reviews under `deleted_user_…` (flow 07's pages) |
| A.3.1 | `/admin/sso/group-mappings` | Prof Mbatha | no provider configured; no form; "No group mappings". **The operator's words to her; "Add a mapping above" with no form (T326)** |
| A.4.5 | `/admin/users` → `/admin/users/{UserId}` | Prof Mbatha | Dr du Plessis (no role, D54); the user not emailed; Show and the six rules; the refusal naming the rules broken, focused, the field marked; then "Password reset. …", the field cleared. **The refusal at the page's top; the email cut at the card's edge (T323)** |
| A.5.5 | `/admin/institutions/{Id:int}` → `/not-found` → `/admin/users/{UserId}` | Prof Mbatha | Page not found; "User unavailable", the same for an unknown id |
| A.6.3 | `/admin/institutions/{Id:int}` → `/` | Prof Mbatha | "Institution saved." focused; Status "Active", "Set by a global administrator."; Back to home and Cancel lead to `/` |
| A.6.4 | `/admin/users` → `/admin/users/{UserId}` | Prof Mbatha | the lock's effects; "User locked out." focused, "Locked out", Reactivate user. **No confirmation (T264)** |
| A.6.7 | `/admin/users/{UserId}` | Prof Mbatha | "User reactivated." focused, Active, Lock out user again |
| A.7.9 | `/` → `/admin/users` → `/admin/users/{UserId}` → `/admin/trainees` → `/admin/trainees/edit` → `/admin/activity-types/{ActivityTypeId:int}` | Prof Mbatha | at 390: the user page's cards stacked and buttons wrapped; flow 11's Trainees stacked; the builder (flow 17's). **Users' table scrolls in its box (807 px); "TraineeSystem-managed" (T323)** |
| A.7.14 | `/account/login` → … → `/admin/users/{UserId}` → … | Prof Mbatha | every pair at AA, Lock out user's white on the danger fill 5.95:1 (T322, flow 01) |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Home and § Institution
administration. The step captures are from the T373 replay (2026-10-20), on the shell as built. The `states/` captures
of these pages are from 2026-09-26, on the old frame: use the step capture where there is one, and the old one for its
body only. `home--*` are from 2026-09-27, on the shell.

| Page | State | Screenshot | Reached at |
|---|---|---|---|
| `/` | Onboarded | `act-2/2.44-1-mbatha-home.png` (`states/home--institutional-admin.png`) | Step 2.44 |
| `/` | Her first sign-in | `act-1/1.10-1-mbatha-home.png` (`states/home--institutional-admin-first.png`) | Step 1.10 (flow 11's step; this flow's page) |
| `/` | KGK adopted, before onboarding | `act-1/1.21-4-mbatha-home-adopted.png` | Step 1.21 |
| `/` | 390 px | `act-A/A.7.9-1-home-390.png` (`states/home--narrow-institutional-admin.png`) | Step A.7.9 |
| `/admin/users` | KGK's staff | `act-2/2.12-2-users-list.png` (`states/users-list--staff.png`, old) | Step 2.12 |
| `/admin/users` | Filtered | `act-2/2.12-3-users-filter-zulu.png` (`states/users-list--filtered.png`, old) | Step 2.12 |
| `/admin/users` | No match | `states/users-list--no-match.png` (old) | Step 2.12, "zzz" |
| `/admin/users` | Two roles a row | `act-2/2.13-5-users-both-roles.png` | Step 2.13 |
| `/admin/users` | With pending registrars | `act-2/2.28-2-users-pending.png` (`states/users-list--pending.png`, old) | Step 2.28 |
| `/admin/users` | Fifteen, onboarded | `act-2/2.44-2-users.png` | Step 2.44 |
| `/admin/users` | After the erasure | `act-A/A.1.14-1-users-no-ndlovu.png` (`states/users-list--after-erasure.png`, old) | Step A.1.14 |
| `/admin/users` | Loading | `states/users-list--loading.png` (old) | A held read |
| `/admin/users` | 390 px | `act-A/A.7.9-2-users-390.png` (`states/users-list--narrow.png`, old) | Step A.7.9 |
| `/admin/users/{UserId}` | Her own | `act-2/2.12-4-own-account.png` (`states/user-detail--own.png`, old) | Step 2.12 |
| `/admin/users/{UserId}` | Another's, before Add role | `act-2/2.13-1-zulu-before.png` (`states/user-detail--other.png`, old) | Step 2.13 |
| `/admin/users/{UserId}` | Role added | `act-2/2.13-2-zulu-assessor-added.png` (`states/user-detail--role-added.png`, old) | Step 2.13 |
| `/admin/users/{UserId}` | A pending registrar | `act-2/2.28-3-molefe-user.png` (`states/user-detail--pending-trainee.png`, old) | Step 2.28 (flow 11 reads it) |
| `/admin/users/{UserId}` | No role (completed) | `act-5/5.17-3-mbatha-user-no-roles.png` (`states/user-detail--no-roles.png`, old) | Step 5.17 (flow 13's step) |
| `/admin/users/{UserId}` | The reset card | `act-A/A.4.5-0-reset-card.png` | Step A.4.5 |
| `/admin/users/{UserId}` | Reset refused | `act-A/A.4.5-1-reset-refused.png` (`states/user-detail--reset-refused.png`, old) | Step A.4.5 |
| `/admin/users/{UserId}` | Reset | `act-A/A.4.5-2-reset.png` (`states/user-detail--reset.png`, old) | Step A.4.5 |
| `/admin/users/{UserId}` | Locked | `act-A/A.6.4-2-patel-locked.png` (`states/user-detail--locked.png`, old) | Step A.6.4 |
| `/admin/users/{UserId}` | Reactivated | `act-A/A.6.7-1-patel-reactivated.png` (`states/user-detail--reactivated.png`, old) | Step A.6.7 |
| `/admin/users/{UserId}` | Unavailable | `act-A/A.5.5-2-dev-trainee-unavailable.png` (`states/user-detail--unavailable.png`, old) | Step A.5.5 |
| `/admin/users/{UserId}` | Pending invitations | `states/user-detail--pending-invitations.png` (old) | Scratch; **no longer reachable that way since flow 11 (Q9)** |
| `/admin/users/{UserId}` | Loading | `states/user-detail--loading.png` (old) | A held read |
| `/admin/users/{UserId}` | 390 px | `act-A/A.7.9-3-user-390.png` (`states/user-detail--narrow.png`, old) | Step A.7.9 |
| `/admin/users/{UserId}` | The danger button (contrast) | `act-A/A.7.14-5-danger-button.png` | Step A.7.14 |
| `/admin/assessors` | None | `act-2/2.14-1-assessors-empty.png` (`states/assessors-list--empty.png`, old) | Step 2.14 |
| `/admin/assessors` | Five | `act-2/2.15-1-assessors-five.png` (`states/assessors-list--five.png`, old) | Step 2.15 |
| `/admin/assessors` | Khumalo's name corrected | `act-2/2.44-3-assessors.png` | Step 2.44 |
| `/admin/assessors` | Loading | `states/assessors-list--loading.png` (old) | A held read |
| `/admin/assessors` | 390 px | `states/assessors-list--narrow.png` (old) | Step 2.15 at 390 |
| `/admin/assessors/edit` | New | `states/assessor-profile-edit--new.png` (old) | Step 2.14 |
| `/admin/assessors/edit` | Empty save refused | `act-2/2.14-2-empty-save.png` (`states/assessor-profile-edit--invalid.png`, old) | Step 2.14 |
| `/admin/assessors/edit` | Provisional, with the date | `states/assessor-profile-edit--provisional.png` (old) | Step 2.14, Dr Khumalo |
| `/admin/assessors/edit` | Saved | `act-2/2.14-3-zulu-saved.png`, `act-2/2.14-4-khumalo-saved.png` (`states/assessor-profile-edit--saved.png`, old) | Step 2.14 |
| `/admin/assessors/edit` | Unknown id | `states/assessor-profile-edit--not-found.png` (old) | Typed `?id=999999` |
| `/admin/assessors/edit` | 390 px | `states/assessor-profile-edit--narrow.png` (old) | Step 2.14 at 390 |
| `/admin/institutions/{Id:int}` | Her own | `act-1/1.23-1-kgk-own-record.png` (`states/institution-edit--own.png`, old) | Step 1.23 |
| `/admin/institutions/{Id:int}` | Saved | `act-A/A.6.3-1-contact-saved.png` (`states/institution-edit--deactivate-refused.png`, old; the name of a state that ceased with T302) | Step A.6.3 |
| `/admin/institutions/{Id:int}` | Out of scope | `act-1/1.23-3-demo-not-found.png`, `act-A/A.5.5-1-demo-institution-not-found.png` (`states/institution-edit--not-found.png`, old) | Steps 1.23, A.5.5 |
| `/admin/institutions/new` | The create form she cannot use | `act-1/1.23-4-institutional-admin-create-form.png` | Step 1.23 (T291 item 8) |
| `/` after Cancel | Her Home | `act-A/A.6.3-2-cancel-home.png` | Step A.6.3 |
| `/admin/sso/group-mappings` | No provider | `act-A/A.3.1-1-sso-no-provider.png` (`states/group-mappings--no-provider.png`, old) | Step A.3.1 |
| `/admin/sso/group-mappings` | 390 px | `states/group-mappings--narrow.png` (old) | Step A.3.1 at 390 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Anywhere: a load error on any of these pages (each prints the exception's text today; Home's is a sentence and Try
  again); the Trainee's note on Users and the user page (T278; no cast member holds Trainee beside a staff role).
- Never built: the lock's confirmation; a role's Remove confirmed; the scope a role carries, shown or asked; an
  assessor's training on the user page (Q2); institutional sign-in links (Q10); the waiting line on Home (Q8); a link to
  her institution's record (Q3); Users' pager; a person with no role named as a former trainee (Q13).
- With an SSO provider (no replay reaches it): the add form, a mapping's row and its Delete; T288's and T333's
  refusals.
- A role removed (no step removes one); the Administrator's account opened by her (not found: ListUsers leaves out
  accounts with no institution).

**Belongs to other flows:** flow 01: access denied, not found, Home's frame. Flow 02: My account (2.41's corrected
name), Change password (her own account's link), the session-ended notice after a lock or reset (A.6.5, A.4.6). Flow 07:
Committee reviews and the withdrawn check-in (`act-A/A.1.14-3…`, `-4…`). Flow 11: Invitations, Invite a person
(`act-2/2.12-1-invitations-none.png`), Trainees and Admit (`act-A/A.1.14-2…`, `act-A/A.7.9-4…`, `-5…`). Flow 13: the
completion that leaves no role (5.17), the portfolio export by address (5.10). Flow 17: the builder (`act-A/A.7.9-6…`).
Flow 18: the Administrator's institution pages (`states/institution-edit--administrator.png`,
`states/institution-edit--create.png`, `states/institution-edit--deactivated.png`, `states/institution-edit--invalid.png`,
`states/institution-edit--saved.png`, `states/institution-edit--narrow.png`), his own account (`states/user-detail--administrator-own.png`,
A.6.9), the audit log of a reset. Flow 06: Waiting for assessors' reminder refusals after a lock (A.5.15).

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-23; none shows a
password (the reset field is empty in `A.4.5-1` and `A.4.5-2`, Show not pressed). Three go through `crop-first/` by
name only (How to run, step 1). In every tall capture at 1280 the sidebar's colour stops at 800 px: an artefact of the
capture.

**Key screenshots (attach these first, with § 1):**
1. `act-A/A.6.4-2-patel-locked.png`: the five-card grid; "AssessorRemove" in red; the lock's result at the page's top
2. `act-A/A.4.5-1-reset-refused.png`: the refusal at the page's top; the email cut at the card's edge; a no-role page
3. `act-A/A.7.9-3-user-390.png`: the user page at 390, "TraineeSystem-managed", a full-width Back to users
4. `act-2/2.12-2-users-list.png`: Users with keys and an Institution column that never varies
5. `act-A/A.7.9-2-users-390.png`: Users at 390, the table scrolling in its box
6. `act-2/2.44-1-mbatha-home.png`: Home's keys, 2 and 2, Quick links
7. `act-2/2.14-2-empty-save.png`: "The UserId field is required."; Speciality "All"
8. `act-2/2.15-1-assessors-five.png`: Assessors' eight columns
9. `act-A/A.3.1-1-sso-no-provider.png`: the operator's words and "Add a mapping above"
10. `act-A/A.6.3-1-contact-saved.png`: her institution's record, Status as text, trail Home › Edit institution

**Home:** `act-1/1.10-1-mbatha-home.png`, `act-1/1.21-4-mbatha-home-adopted.png`, `act-A/A.7.9-1-home-390.png`.

**Users:** `act-2/2.12-3-users-filter-zulu.png`, `act-2/2.13-5-users-both-roles.png`, `act-2/2.28-2-users-pending.png`,
`act-2/2.44-2-users.png`, `act-A/A.1.14-1-users-no-ndlovu.png`, and (old frame) `states/users-list--no-match.png`,
`states/users-list--loading.png`.

**The user page:** `act-2/2.12-4-own-account.png`, `act-2/2.13-1-zulu-before.png`, `act-2/2.13-2-zulu-assessor-added.png`,
`act-2/2.28-3-molefe-user.png`, `act-5/5.17-3-mbatha-user-no-roles.png`, `act-A/A.4.5-0-reset-card.png`,
`act-A/A.4.5-2-reset.png`, `act-A/A.6.7-1-patel-reactivated.png`, `act-A/A.5.5-2-dev-trainee-unavailable.png`,
`act-A/A.7.14-5-danger-button.png`, and (old frame) `states/user-detail--pending-invitations.png` (the table and Revoke
all, red), `states/user-detail--loading.png`.

**Assessors:** `act-2/2.14-1-assessors-empty.png`, `act-2/2.14-3-zulu-saved.png`, `act-2/2.14-4-khumalo-saved.png`,
`act-2/2.44-3-assessors.png`, and (old frame) `states/assessor-profile-edit--provisional.png`,
`states/assessor-profile-edit--not-found.png`, `states/assessor-profile-edit--narrow.png`,
`states/assessors-list--narrow.png`, `states/assessors-list--loading.png`.

**The institution's record and SSO:** `act-1/1.23-1-kgk-own-record.png`, `act-1/1.23-3-demo-not-found.png`,
`act-1/1.23-4-institutional-admin-create-form.png`, `act-A/A.5.5-1-demo-institution-not-found.png`,
`act-A/A.6.3-2-cancel-home.png`, and (old frame) `states/group-mappings--narrow.png`.

That is 47 files.

**Do not attach:** `act-A/A.6.4-1-patel-before-lock.png` (Dr Patel's own Activity inbox in his second browser, not his
user page; the old brief cited it as the user page); `act-2/2.12-1-invitations-none.png` (flow 11's page); the old-frame
`states/` captures a step capture replaces (`users-list--staff`, `--filtered`, `--pending`, `--after-erasure`,
`--narrow`; `user-detail--own`, `--other`, `--role-added`, `--pending-trainee`, `--no-roles`, `--reset-refused`,
`--reset`, `--locked`, `--reactivated`, `--unavailable`, `--narrow`; `assessors-list--empty`, `--five`;
`assessor-profile-edit--invalid`, `--saved`, `--new`; `institution-edit--own`, `--deactivate-refused`, `--not-found`;
`group-mappings--no-provider`; `home--institutional-admin`, `--institutional-admin-first`,
`--narrow-institutional-admin`); the Administrator's institution and account captures (flow 18's); `act-A/A.1.14-2…` to
`-4…`, `act-A/A.7.9-4…` to `-6…` (flows 11, 07 and 17's pages).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `98584921`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T264** (P3, queued; the users and SSO half) | **Lock out:** a ConfirmDialog naming the person and the lock's effects; an outline trigger; today `btn-danger`, one press (`U/UserDetail.razor:166`; observed A.6.4's Actual "no dialog"). **A role's Remove:** outline, named for its row, confirmed naming the person and the role; today `btn-danger` at once (`:82`). **Revoke all pending invitations:** `btn-danger` at once (`:205`); Q9 drops it. **SSO Delete:** `btn-danger` at once (`W/Admin/Sso/GroupMappings.razor:148`). Flow 11 closed the invitations half; CollegeEdit, SpecialityEdit and SubSpecialityEdit stay flow 16's, the scales list flow 15's. | `act-A/A.6.4-2…`, `states/user-detail--pending-invitations.png` |
| **T323** (A3; the users half) | **Card values:** the user page's email is cut at the card's edge at 1280 (observed: "duplessis@kgk.wombat.lc", `A.4.5-1`; "patel@kgk.wombat.local" at the edge, `A.6.4-2`), because `.detail-list dd` sets only a margin (T323's root cause). **Role rows:** two spans in an unstyled `li` (`U/UserDetail.razor:74–88`), so "AssessorRemove" and "TraineeSystem-managed" (observed, `A.6.4-2`, `A.7.9-3`; F-2.28b). **Tables:** no `Stack`/`StackWide` on Users (`U/UsersList.razor:35`), Assessors (`As/AssessorsList.razor:20`) or mappings (`GroupMappings.razor:125`); at 390 Users scrolls in its box, 807 px (observed, A.7.9's Actual). **Pending invitations:** a five-column table in the grid's narrow column, cut (old frame, `states/user-detail--pending-invitations.png`). | `act-A/A.4.5-1…`, `act-A/A.7.9-2…`, `-3…` |
| **T324** (A8; the users half) | **Keys everywhere:** the Users list's Roles (`U/UsersList.razor:51`), the user page's roles (`U/UserDetail.razor:75`), Add role's options (`:105`), the result "Role 'Assessor' added." (`:372`, `:385`), the row name "Remove the CommitteeMember role" (`:82`), the pending invitations' Role (`:196`), Home's Users card (`W/Dashboards/InstitutionalAdminDashboard.razor:22`), the SSO picker and rows (`GroupMappings.razor:62`, `:141`), though `WombatRoleLabels` (`src/Wombat.Domain/Identity/WombatRoleLabels.cs`) exists and My account uses it (2.41). **Property names:** "The UserId field is required." and "The Qualifications field is required." (observed 2.14; `As/AssessorProfileEdit.razor:221–239`); the institution's record (`InstitutionEdit.razor:369–383`) and the mapping form (`GroupMappings.razor:299–318`) the same. | `act-2/2.12-2…`, `act-2/2.14-2…`, `act-2/2.44-1…` |
| **T326** (A10; the assessors and SSO items) | **Assessors' empty body** says a profile makes assessors "selectable in the workflow" (`As/AssessorsList.razor:19`; observed 2.14, F-2.14c); `NomineeDirectory` reads the Assessor role alone. **SSO with no provider:** the card names `Sso:Providers` and a restart to her (`GroupMappings.razor:110`), and the empty state points at a form that is not drawn (`:116`; observed A.3.1, F-A.3.1a). | `act-2/2.14-1…`, `act-A/A.3.1-1…` |
| **T289** (P2, queued; lands before the build, Q12) | **The profile command** never asks the trainee-first rule, never checks that the user is at the caller's institution (`A/Assessors/CreateOrUpdateAssessorProfile.cs:50–60`), and finds a profile by user, so a "new" profile for someone profiled overwrites theirs (`:96–111`, inferred). **The save rewrites the user's whole speciality scope** (`:125–130` → `UpdateScopeAsync`), stripping a SpecialityAdmin's or CommitteeMember's (code, T289's C24). **The page** stays on its new form after a save (`As/AssessorProfileEdit.razor:207–209`) and the picker never narrows (`A/Assessors/ListAssessorUsers.cs:28–43`; observed 2.14, F-2.14a, F-2.14b). | `act-2/2.14-3…`, `-4…` |
| **T288** and **T333** (P2, queued; land before the build, Q11) | **Cross-institution mappings:** create checks only the chosen institution, never the provider's (`A/Sso/CreateSsoGroupMapping.cs:37–40`). **Trainee round admission:** the page offers Trainee and Pending trainee (`GroupMappings.razor:162–172`) and the command refuses only Administrator (`CreateSsoGroupMapping.cs:41–44`). The page's Institution reads "Select institution" over her one (`:69`) and Speciality the national catalogue (`:190`). Not reachable on the replay. | `coverage.md` § Flows and states not played |
| **T291** (P3, queued; items 4 and 8) | **Item 4:** Home counts every Speciality and SubSpeciality row (`A/Dashboards/InstitutionalAdmin/GetInstitutionalAdminDashboardSummaryQuery.cs:46–55`, under a stale "phase 4" comment); observed 2 and 2 at 1.10, 1.21 and 2.44 (F-2.44a). **Item 8:** typed by her, `/admin/institutions/new` draws "Create institution" (`InstitutionEdit.razor:1–3`), which `CreateInstitutionCommand` always refuses (observed 1.23, F-1.23c). Items 1, 2, 3, 5 and 6 are flows 15, 16 and 17's. | `act-2/2.44-1…`, `act-1/1.23-4…` |
| **T286** (P3, queued; the external-logins half, Q10) | No page lists a person's institutional sign-in links, and only an institution move or an erasure removes one; whether a reset drops them is undecided. Flow 11 closed the issue half. The audit, unique-email and link-race items stay T286's own. | — |
| **T329 / T272's family** (code) | **Exception text:** Users' and the user page's load (`U/UsersList.razor:111`, `U/UserDetail.razor:313`, `RefusalText.Of`, which is the message for anything but a validation failure), Assessors (`As/AssessorsList.razor:63`), the assessor form's load and save (`As/AssessorProfileEdit.razor:175`, `:213`; an unknown id prints "The assessor profile could not be found." as a danger alert), SSO (`GroupMappings.razor:220`, `:262`, `:291`). **No Try again:** no `StatePanel` here takes `OnRetry`; SSO's passes `LoadError="@(null)"` (`:116`). **The institution's record** has no load error at all: `StatePanel IsEmpty="false"` (`InstitutionEdit.razor:39`) draws the empty form with Save under the failure. | `states/assessor-profile-edit--not-found.png` |
| **T280** (headings, captions; code) | Every card title is `<h3>` under the page's `<h1>` (`U/UserDetail.razor:54`, `:64`, `:119`, `:143`, `:177`; `GroupMappings.razor:33`, `:115`), Add role an `<h4>` with an inline `style` (`U/UserDetail.razor:96`); Users' caption "Users" repeats its `<h1>` (`U/UsersList.razor:35`). | `act-2/2.12-2…` |
| **T325** (A9; code) | The pending invitations' Issued prints `ToLocalTime()` with no zone (`U/UserDetail.razor:199`). | — |
| **Users** (observed, not filed) | The filter runs per keystroke (`@bind:event="oninput"`, `U/UsersList.razor:23`); no pager, everything sorted by surname (`A/Users/Queries/ListUsers/ListUsersQuery.cs:69–70`); an Institution column that reads KGK on every row for her (observed, 2.12); the subtitle says "across the institutions you administer" (`U/UsersList.razor:76`) to someone who administers one; a person with no role (D54) gets an empty Roles cell (`:51`, inferred). Q6. | `act-2/2.12-2…` |
| **The user page** (observed and code, not filed) | **Scope never shown:** `UserDetailDto` carries `SpecialityIds` and `SubSpecialityIds` (`A/Users/UserDtos.cs:21–22`); the page reads neither (`U/UserDetail.razor:52–61`). **Add role grants no scope** (`A/Users/Commands/AddRoleToUser/AddRoleToUserCommand.cs`, no scope written): a Speciality admin added here holds whatever speciality the person already had, or none (inferred). Q4. **A person with no role** (D54: du Plessis after 5.27, Molefe after 5.17) reads "This user has no roles." and is offered every staff role (observed A.4.5, `5.17-3`); nothing says they are a former trainee whose record stays readable to them. Q13. **Words:** the subtitle "…pending-invitation cleanup for a single user." (`:265`); "Global administrators cannot be locked out from this surface." (`:171`); Back to users repeats the trail and is a full-width 44 px button at 390 (`:11`; observed A.7.9). **Results at the page's top**, far from the card that acted (`:28–38`; observed A.4.5, A.6.4). | `act-A/A.4.5-1…`, `act-5/5.17-3…`, `act-A/A.7.9-3…` |
| **Pending invitations** (code; inferred) | Unreachable since flow 11: issue refuses an address that has an account (`A/Invitations/IssueInvitation.cs:191`), and registering revokes the person's other invitations (`A/Invitations/AcceptInvitation.cs:119–124`). Only an account made by SSO after an invitation was issued could still show one. `states.md`'s way to reach `user-detail--pending-invitations` (invite Dr Botha again) is now refused. Q9. | `states/user-detail--pending-invitations.png` |
| **Assessors** (observed and code, not filed) | Institution is a one-option select (`As/AssessorProfileEdit.razor:48–55`); Speciality offers the national catalogue with "All" (`:57–65`, `:130`), not KGK's adopted Paediatrics; the list says "Provisional" where the form says "Provisional (awaiting sign-off)" (`As/AssessorsList.razor:74`); "across institutions" in the list's subtitle (`:9`); one title, "Assessor profile", for new and existing; jargon help ("Faculty-development lifecycle.", `:83`); `@using Wombat.Domain.Identity` and the Domain enum in both `.razor` files (`As/AssessorsList.razor:3`, `As/AssessorProfileEdit.razor:3`), against CLAUDE.md's "only via DTOs". The Administrator is admitted with no menu item and nothing lit (`Nav/NavItems.cs:144–149`, `Nav/NavOwners.cs:164`). Q2. | `act-2/2.14-2…`, `act-2/2.15-1…` |
| **Her institution's record** (observed, not filed) | Reached only by address (`coverage.md` § Reached only by address: 1.23, A.6.3); nothing lit, trail Home › Edit institution (`Nav/NavOwners.cs:150` owns it for the Administrator alone; observed A.6.3); titled "Edit institution", not KGK's name, with the generic subtitle "Maintain the institution record used for scoping and onboarding." (`InstitutionEdit.razor:9–11`); Contact email has no address check (`:379–380`). Q3. | `act-A/A.6.3-1…`, `act-1/1.23-1…` |
| **Her Home** (observed and code, not filed) | Roles as keys and locked accounts counted (`GetInstitutionalAdminDashboardSummaryQuery.cs:33–42`); two inline `style`s (`InstitutionalAdminDashboard.razor:23`, `:38`); Quick links repeat four menu items; nothing says a registrar is waiting (flow 11's Q8, decided as recommended and left to this flow to place); no way to her institution's record. Q7, Q8. | `act-2/2.44-1…` |

**Dropped from the old brief (done since):**
- T302 (`41be531`): her institution's Status is text, "Set by a global administrator.", with no box and no Deactivate;
  Back and Cancel lead home (T291 item 7). A rule now, not a requirement.
- T303 (`4824d627`): Trainee is "System-managed" and never offered by Add role; the help line's words are flow 11's
  ("…from Trainees with 'Admit…'.").
- T278 (`2290ed50`): her own account offers no Remove, Add role, Reset or Lockout, with the note and Change your password;
  the Trainee's note on both Users pages (T290, `f3471780`, made the commands refuse first).
- T317's reset half (flow 02, `f50dffb2`): no `minlength`, Reset password enabled once anything is typed, one sentence
  per broken rule, the field marked (A15). Its registration half was flow 11's. Nothing of T317 is left to this flow.
- T321 (flow 01, `b347e11c`): access denied is drawn once; Back to institutions, the link that reached it, went with
  T302.
- T322 (flow 01, `b347e11c`): white on the danger button 5.95:1, the focus ring 4.61:1, an input's border 3.60:1
  (A.7.14's Actual).
- Flow 01: Home's heading and role line, "Invite a person" as its header action (flow 11 now opens Invite a person), the
  person's name in the top bar; the old brief's "Home greets by email" and the "Welcome" line are gone.
- The old brief's DIRECTION placeholder (the aesthetic direction is the design system's, version 29) and its A-list
  (now BRIEF § 6, quoted in § 1's requirements).

## 6. Questions for the operator

Each with a recommendation. The canvas answers 1–7 in its variations; 8–16 are the operator's.

1. **Q1. The user page: sections or tabs** (2.12, 2.13, A.4.5, A.6.4). *Recommendation:* one page, sections under
   `<h2>`s in this order: the person (name, address, institution, status, and the scope of each role); Roles (Add role
   with its scope); Assessor training (Q2, only for an Assessor); Sign-in (Reset password, institutional sign-in links,
   Q10); Access (Lock out or Reactivate). No tabs: a tab hides the state a lock or a role changes. Each result shows in
   its own section, focused. Back to users goes (the trail carries it).
2. **Q2. Assessor training.** *Recommendation:* a section of the user page for anyone holding Assessor (status, date,
   qualifications, its own Save), and Assessors stays the list across them, its Edit opening that section
   (`/admin/users/{UserId}#training`, PROPOSED); `/admin/assessors/edit` and "Create assessor profile" go, so no profile
   is ever created for someone without the role, and the "new" picker problem (T289) cannot arise. Scope (institution,
   speciality) is the role's (Q4), not the profile's.
3. **Q3. Her own institution.** *Recommendation:* a Home card "Kgosi Kgari Teaching Hospital" (short code, contact
   email, Edit) linking to the record; the record titled by the institution's name, its trail Home › ‹name›, with no
   menu item drawn as settled.
4. **Q4. Role and scope.** *Recommendation:* each role listed with its scope ("Assessor · Paediatrics / Paediatrics";
   "Committee member · Paediatrics"; "Committee member · no speciality" for van Rensburg); Add role asks the scope the
   chosen role takes by Invite a person's rules (radios by label, only the role's fields, required marks; PROPOSED:
   `AddRoleToUserCommand` gains the scope, DATA).
5. **Q5. The lock.** *Recommendation:* "Lock out Mohammed Patel?" with the four effects and "His records stay as they
   are." (Keep him active / Lock out); the result "Mohammed Patel is locked out. He was signed out everywhere." in the
   Access section, focused; Status a badge "Locked out"; Reactivate user with no confirmation.
6. **Q6. Users.** *Recommendation:* Name over address, Roles by label (one a line), Status (a badge only when Locked
   out), Manage; Institution only for a caller whose reads span institutions; the filter applied with Show (flow 06), its
   count announced; a pager at 50 (flow 08's); stacked from 900 px (StackWide).
7. **Q7. Home.** *Recommendation:* People (each role by label with its count, linking to Users; locked accounts named
   apart), Curriculum (what KGK has adopted, 1 and 1, linking to Curriculum adoptions), her institution's card (Q3), and
   Q8's line; Quick links go (the menu holds them).
8. **Q8. (The operator's) A registrar waiting.** *Recommendation:* "Waiting to be admitted: 5" at the top of Home,
   linking to Trainees, shown only when someone waits; no mail (flow 11's Q8 as decided).
9. **Q9. (The operator's) Pending invitations on the user page.** *Recommendation:* drop the card and "Revoke all
   pending invitations" (and `RevokePendingInvitationsForEmailCommand`): flow 11 refuses an invitation to an address
   that has an account and registration revokes the rest, so it can hold nothing; retire the state
   `user-detail--pending-invitations`.
10. **Q10. (The operator's) Institutional sign-in links (T286).** *Recommendation:* list them in Sign-in ("Signed in
    through ‹provider› since ‹date›") with Remove…, confirmed; a reset does not drop them; drawn from the text (no
    provider on the replay). Fold T286's external-logins half into the build.
11. **Q11. (The operator's) SSO mappings.** *Recommendation:* T288 and T333 land first as their own tasks; for her the
    page is read-only until a provider exists and then offers her institution fixed, staff roles only; with no provider:
    "Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them
    if your hospital signs staff in through its own system." (PROPOSED words); the Administrator keeps the setting's name.
12. **Q12. (The operator's) What owns an assessor's discipline (T289).** *Recommendation:* the role's scope (Q4), not the
    profile; the profile records training only, and its save writes no scope. T289's refusals (Trainee first, the user at
    her institution) land first as their own task.
13. **Q13. (The operator's) A person with no role** (D54). *Recommendation:* the page reads "No role. Lerato Molefe's
    programme was completed on ‹date›; her record stays readable to her." (or withdrawn), and offers staff roles with that
    scope; how an account re-enters a programme stays T374's decision, not drawn here.
14. **Q14. (The operator's) Role labels.** *Recommendation:* labels on every page here, the Add role choices, the results
    ("Assessor added to Thandi Zulu."), Home's counts and the SSO picker; the Expects that quote keys change (2.12, 2.13,
    2.44, A.6.7).
15. **Q15. (The operator's) Correcting a name or an address.** *Recommendation:* not on the user page in this flow: the
    person corrects their name on My account (2.41), and an address change (a sign-in identity) is its own task, proposed
    if wanted (A.5.15's note: "No page edits an account's address").
16. **Q16. (The operator's) What the build folds in.** *Recommendation:* land T288, T333 and T289 first as their own
    tasks (P2, security); fold T264's users and SSO half, T326's assessors and SSO items, T291 items 4 and 8, T286's
    external-logins half (if Q10), and the T323, T324, T280, T325 and T329 halves of these pages. T286's other items,
    T291's other items and T374 stay their own.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are 1.23, 2.12–2.15, 2.44 and the appendix's;
  § 9): every Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 1.23 and A.6.3
  (the record's title and how she reaches it; no create form at `/new`); 2.12 (the list's columns and filter, labels);
  2.13 (labels, the scope, the result's words); 2.14 and 2.15 (where training lives, Q2; the refusal's words; the empty
  body); 2.44 (Home's cards, 1 and 1, the waiting line); A.3.1 (the words for her); A.4.5 (where the result shows; the
  no-role page); A.6.4 and A.6.7 (the confirmation, the result's words); A.7.9 (Users stacked); search the whole runbook
  (BRIEF.md § 9 item 7) for the words that change, including 1.10, 1.21, 2.28, 5.10, 5.17, 5.28, A.5.15 and A.6.9, which
  cross these pages. A new page or route needs a step (`Scenario/ScenarioRunbookTests`). A new step takes a letter when
  its number is taken (§ 11, flow 06). Record passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** A.6.4, a ConfirmDialog naming Dr Patel, its cancel locking nothing, the result focused;
  2.13, a role's scope shown and asked, labels everywhere; 2.14, the empty save's words, no move to a new form after a
  save; 2.44, 1 and 1; Users and the user page at 1280 with nothing cut (the whole address, a gap before each action),
  and at 390 and 768 with nothing scrolling sideways; every table captioned; no exception's text on any failed read.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - the users, assessors, institution and SSO pages' own tests (`UsersPagesSelfAndTraineeTests`, `UserDetailLockCardTests`,
    `UserDetailResetCardTests`), with new ones for the lock's confirmation, the scope, the labels, the training section
    and the load errors; the handler tests T288, T333 and T289 bring;
  - `Navigation/NavMenuAuthorizationTests`, `ActiveNavItemTests`; `Hosting/PageTitleTests`;
  - `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones, and the scratch ones on scratch copies, as `states.md` says)
  and the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare them with the
  chosen artboards.
- **Browser check** at 1280, 768 and 390 px as Prof Mbatha, and as `devadmin` on Users and a user page (his view is
  flow 18's, but these pages are shared); a contrast pass as in Step A.7.14, with every pair at AA.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only), as flow 11's replay
left them (T373, 2026-10-20; the Gaps it recorded are 1.23's T291 item 8, 2.14's T289, T326 and T324, 2.44's T291 item
4, A.3.1's T326 and A.7.9's T323). `D` is the replay day (README § How to play).

```text
Step 1.23 — KGK's own record
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found
Do: Open KGK's record at `/admin/institutions/{InstitutionId}` by typing its address, since nothing in her nav leads
  there. Read it and change nothing. Then open the Demo Institution's id the same way.
Expect: KGK's page is headed "Edit institution" and shows KGK's name, the short code `KGK`, the contact email, and
  that KGK is active. Save is offered for the name, short code and contact email.
  She is offered nothing that would deactivate KGK, neither a Deactivate nor an Active she can untick, and no link
  (Back or Cancel) to the Institutions list: deactivating an institution and listing them belong to an Administrator
  alone (`DeactivateInstitutionCommand`; DESIGN.md § Table system, T211).
  The Demo Institution's id sends her to Page not found, not "You cannot open this page". The product never confirms
  that a record outside her institution exists (CLAUDE.md § InstitutionalAdmin scope-aware powers).

Step 2.12 — Mbatha reviews KGK's users
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

Step 2.13 — Mbatha adds Assessor to Dr Zulu, Dr Naidoo and Dr Botha
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

Step 2.14 — Mbatha creates five assessor profiles
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

Step 2.15 — The assessor list and who is not on it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors
Do: Read the list.
Expect: Five rows, each at KGK in Paediatrics / Paediatrics. Training status reads Trained for Zulu, Naidoo and Botha, In
  training for Patel and Provisional for Khumalo. Training completed reads `D−8 years`, `D−6 years`, "Not recorded",
  "Not recorded" and `D−1 month` respectively. Each row has an Edit named for its assessor. Van Rensburg is neither listed
  nor offered, because he does not hold the Assessor role.

Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/invitations/new → / → /admin/users → /admin/assessors
Do: Open Home and press Invite a person, then go back to Home. Then open Users and Assessors.
Expect: Home reads "Institutional admin · Semester N, YYYY" and offers "Invite a person", which opens Invite a person
  (`/admin/invitations/new`), not the list (T373, Q1). Users reads InstitutionalAdmin 1, SpecialityAdmin 1,
  SubSpecialityAdmin 1, Coordinator 1, CommitteeMember 4, Assessor 5 and Trainee 5, with no PendingTrainee line.
  Specialities & sub-specialities reads 1 and 1, what KGK has adopted. Quick links reads Users, Invitations, Curriculum
  adoptions and Entrustment decisions. The users and assessors lists both name Fatima Khumalo (Step 2.41).

Step A.1.14 — What the erasure left, as KGK sees it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or trainees (T026, T258).
  - Committee reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review as it was; the closed check-in, Type "Formative check-in", State Closed and Decision "—" (read as "None");
    and A.1.10's check-in, Type "Formative check-in", State Withdrawn and Decision "None: the review was withdrawn".
  - The check-in's sitting bar reads Withdrawn and "Withdrawn on `D`. Withdrawn because the trainee's personal data
    was erased at their request. Nothing more is decided at this review." (T258, T361), and the page offers no action:
    no Start, no Refresh.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.

Step A.3.1 — Prof Mbatha opens SSO Mappings with no provider configured
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/sso/group-mappings
Do: Open SSO mappings from the menu.
Expect: A card says that no SSO providers are configured, and to add them to the `Sso:Providers` section of the
  application settings and restart. No form for adding a mapping is offered. Current mappings is empty ("No group
  mappings"), although its empty state still says to add a mapping above. Nothing on the page can be changed.

Step A.4.5 — Prof Mbatha resets his password
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Set a new password of 10 characters, then one that meets every rule. Record the
  second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: The Reset password card says it sets a password directly and that the user is not emailed. Its field has the
  Show toggle ("Show new password") and the six rules under it, and Reset password is enabled once anything is typed.
  The 10-character password is refused with "The password was not reset. The new password needs:" and the rules it
  breaks, in the six rules' order. The second is accepted, and the field is cleared. The audit log records the reset
  with the password redacted (T101).

Step A.5.5 — Another institution's records, by id
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not "You cannot open this page". The user shows "User unavailable" ("The
  user could not be found or is outside your scope."). Neither page confirms that the record exists (CLAUDE.md: 404, not
  403).

Step A.6.3 — Prof Mbatha edits her own institution
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /
Do: Open KGK's page by its address (no nav link leads there). Set its contact email to
  `hod.paediatrics@kgk.wombat.local` and save. Read Status, then look at Back to home and Cancel.
Expect: "Institution saved." takes the focus. Status reads "Active" as text, with "Set by a global administrator."
  beneath: there is no Active box and no Deactivate, because an institution's state is the Administrator's alone
  (T302). "Back to home" and Cancel both lead to `/`. SQL:
  `SELECT "Id","ContactEmail","IsActive" FROM "Institutions" WHERE "ShortCode"='KGK'` gives
  `2|hod.paediatrics@kgk.wombat.local|t`.

Step A.6.4 — Prof Mbatha locks Dr Patel out
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: With Dr Patel signed in on another browser, open his account, read the Lockout card, and lock him out.
Expect: Once locked, his status reads "Locked out" and the card offers Reactivate user. The focus moves to the result.
  No confirmation is asked first (T264). Before the lock, the card lists what it does until he is reactivated, and says
  his records stay as they are (T284):
  - he sits on no committee panel;
  - he is not a current trainee;
  - he is sent no reminders or digests;
  - he cannot be named as an activity's assessor.

Step A.6.7 — Prof Mbatha reactivates Dr Patel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users/{UserId}
Do: Reactivate Dr Patel.
Expect: His status reads Active, the card offers Lock out user again, and the focus moves to the result.

Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK Teaching Session
  Log in the builder.
Expect: The user page's cards stack, and its buttons wrap rather than overflow. On Trainees, Current trainees is
  stacked, a block per row with its Edit 44 px, and does not scroll sideways (T373, S16); Completed and closed is flow
  13's list as built. The trainee profile form stacks its fields. The builder's editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
  stays usable.

Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on Committee reviews (each state a badge since T364, Scheduled grey there and on Decisions due)
    and on a committee review opened from it (Dr Molefe's review 7);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
```
