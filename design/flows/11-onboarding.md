# F11 From invitation to first sign-in and admission

**Who and why:** nobody gets into Wombat except by invitation. An Administrator invites the College's admin and each
institution's first Institutional admin; the Institutional admin invites her staff and her registrars, each with one
role and a scope (institution, speciality, sub-speciality). The invitee registers from the emailed link on
`/account/register`, a static page on the auth card, and lands on Home. A registrar invited as Trainee registers as a
Pending trainee and waits, able to do nothing but read her account (D55), until the Institutional admin admits her to
the adopted curriculum with her programme dates; admission changes her role and ends her open sessions (T279). Every
user meets this flow once; a role and a scope are access control. Four pages carry it: Invitations, Register, the
pending registrar's Home, and Trainees with its admission form. None has been redesigned since flow 01 gave them the
shell and flow 02 gave Register its heading and password fields; the step captures are from flow 10's replay
(2026-10-19), the state captures from 2026-09-26 to 09-30, mostly on the old frame.

Restated 2026-10-19 for T373 (the brief of 2026-09-26 predates flows 01–10, D54 and D55). Paths to screenshots are
under `design/baseline/` (gitignored). Code is cited at `a1a9b120`; `W/` is `src/Wombat.Web/Components/Pages/`, `I/` is
`W/Admin/Invitations/`, `T/` is `W/Admin/Trainees/`, `A/` is `src/Wombat.Application/Features/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell and, for Register, flow 02's auth card. Marked **F/W** in BRIEF.md § 8: the Register card is settled (F); Invitations and the admission pages are less settled than an F suggests (Mode). |
| Mode | **Kept, read as flow 10 read it** (BRIEF.md § 2.4: an F page's structural round may be a single proposal to confirm). **Straight to fidelity for Invitations and Register:** round 1 draws Invitations at full fidelity in **2–3 variations**, because where issuing lives is open (a form beside a 10-column table that is cut at 1280 today, Q1), and Register as **ONE proposal** on flow 02's card to confirm (static, its shape fixed by DESIGN.md § Account / auth page). **Wireframe first for the admission pages:** 2–3 structures for Trainees and the admission form (Q4: admission today shares `/admin/trainees/edit` with the profile page that ends a programme, flows 13 and 15), then fidelity for the pick. **One change, for the pending registrar's pages:** D55 (`fb6e465e`) and T141 (`e0a06a2a`) cut them to Home's one card, flow 02's My account and flow 01's access denied, so nothing structural is left to vary there: round 1 draws her Home as ONE proposal at fidelity (its words are Q3). Round 2: the picks with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 for Invitations (fidelity) and for Trainees with admission (wireframes) in round 1; one proposal each for Register and the pending Home; the chosen ones in round 2 |
| People | Administrator: the platform operator (`devadmin@wombat.local`, "Demo Administrator"), who invites the CollegeAdmin and the first InstitutionalAdmin. InstitutionalAdmin: Prof Nolwazi Mbatha (KGK, Kgosi Kgari Teaching Hospital), who invites the staff and registrars and admits the registrars. Invitees, anonymous until they register: Dr Anton Kruger (CollegeAdmin, CPSA); Prof Mbatha; Mr Pieter Smit (Coordinator); Dr Thandi Zulu, Dr David Naidoo, Dr Sarah Botha (CommitteeMember); Dr Mohammed Patel, Dr Fatima Khumalo (Assessor); Dr John van Rensburg (external CommitteeMember, `sun.wombat.local`); Dr Refilwe Mokoena (SpecialityAdmin); Dr Kabelo Sithole (SubSpecialityAdmin); five registrars invited as Trainee: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu (1), Dr Sipho Ndlovu (1; first invited at the mistyped `ndlvou@`). PendingTrainee: each registrar between registering and admission (`README.md` § Cast) |
| Runbook steps | 29, pasted verbatim in § 8: 1.5, 1.7–1.11 (Act 1), 2.1–2.11, 2.16–2.19, 2.26–2.32 and 2.44 (Act 2). 2.31 lands on flow 02's session-ended notice; 2.32 is flows 06 and 08's Coordinator; 1.8, 1.10 and 2.44 read Homes flows 12 and 16 design |
| Pages (`coverage.md` templates) | `/admin/invitations` (`I/InvitationsList.razor`, 471 lines; words in `I/InvitationText.cs`, 79), `/account/register` (`W/Account/Register.razor`, 148: static for a signed-out visitor, `AuthLayout`; refusals as codes in `src/Wombat.Web/Security/RegisterOutcome.cs`, 181; posts to `/account/register/submit`, `src/Wombat.Web/Program.cs:343–393`), `/` (the pending registrar's Home: `W/Dashboards/TraineeDashboard.razor:18–28`, one card), `/account/profile`, `/activities/mine`, `/activities/new` and `/access-denied` as she meets them (flows 02, 03 and 01's, built), `/admin/users` and `/admin/users/{UserId}` as Prof Mbatha reads the pending registrars (flow 12's pages), `/admin/trainees` (`T/PendingTraineesList.razor`, 175) and `/admin/trainees/edit` (`T/TraineeProfileEdit.razor`, 495: admission with `?userId=`, a profile with `?id=`, one page). Arrived at from `/` (the Institutional admin's header action "Invite a person", `Nav/HomeFrame.cs:51`) and the menus. Since `b347e11c` (flow 01) only `f50dffb2` (flow 02: Register's PasswordFields), `95d4e950`/`a27983fd` (T304), `f3471780` (T290) and `78e95df6` (D54's words on Deactivate) touched them. |
| Menus (`Nav/NavItems.cs`) | Invitations is in the Administrator's People group (`:146`) and the Institutional admin's (`:152`); Trainees in the Institutional admin's only. The admission form and a profile light Trainees for the Institutional admin, trail Home › Trainees › Trainee profile (`Nav/NavOwners.cs:155`). The Administrator is admitted to both Trainees pages (`AdministratorOrInstitutionalAdmin`) with no item and nothing lit. The Pending trainee's menu is Home alone, then My data rights (D55, `:182`). Register is outside the shell: nothing lit, no trail. Someone who holds Trainee beside an admin role sees a note in place of each page (T290, `I/InvitationsList.razor:14–17`, `T/PendingTraineesList.razor:12–15`). |
| Components drawn at runtime | `PageHeader` (Invitations and Trainees with no trail; the profile page with "Back to trainees" in its header beside the trail, `T/TraineeProfileEdit.razor:13–17`), `.details-grid` of two `section.detail-card`s on all three admin pages, `DataTable` (captioned, no `Stack`: invitations `I/InvitationsList.razor:139`, pending `T/PendingTraineesList.razor:36`, active `:67`, closed `:95`), `StatePanel` (no `LoadError`, no `OnRetry` anywhere), `EditForm` with `DataAnnotationsValidator` and `ValidationSummary`, `FormField`, `FormActions`, `FieldHelp`, `InputSelect`/`InputDate`, `Alert` (every result and refusal), `ActionResult` (Invitations only; the profile page hand-rolls `div.action-result`, `:24`), `ConfirmDialog` (Deactivate and Mark complete only; none for Revoke), `InFlight`, `RowNames.Distinct` (row actions named per row, T239). Register: `.account-form-container` with an `<h1>`, `Alert`, plain `<form method="post">`, `.mb-3` wrappers around raw inputs (DESIGN.md:4069), two `PasswordField`s with `PasswordRules`. The pending Home: `DashboardFrame`, one `DashboardCard` with emphasis. Queries: `ListActiveInvitations`, `GetInvitationPreview`, `ListPendingTrainees`, `ListTraineesForSpeciality`, `GetTraineeProfileById`, `GetAdmissionCurriculumChoices`, `GetTraineeCurriculumChoices`, `GetTraineeDashboardSummary`; commands `IssueInvitation`, `ResendInvitation`, `RevokeInvitation`, `AcceptInvitation`, `AdmitTrainee`, `UpdateTraineeProfile` (all `A/Invitations/`, `A/Trainees/`). Mail: `InvitationEmail` (issue and resend), flow 19's; admission sends none. |
| Held | None. Every step capture is from the T369 replay (2026-10-19), on the shell as built. The `states/` captures of Invitations, Trainees, the profile page and the users pages are from 2026-09-26, before flow 01 (a flat Title Case sidebar, an email in the top bar, the old alert colours: checked by opening `invitations-list--check-address` and `--narrow`); Register's are from 2026-09-28 (flow 02's card, current); `states/new-activity--pending-trainee.png` (2026-09-30) shows the type picker D55 took away: do not use it. The step captures stand in where there is one (§ 3) |
| Depends on | Flow 01, built (`b347e11c`): the shell, "Acting as Pending trainee", Home's heading and role line, the person's name in the top bar, the Institutional admin's "Invite a person", access denied, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again, the tokens' contrast (T322). Flow 02 (`f50dffb2`): the auth card, its `<h1>`, `PasswordField` with Show and the six rules, refusals as fixed words (T285), the session-ended notice (2.31), the sign-out page. Flow 03 (`725237ee`) and 04 (`06aa51d7`): row actions named per row. Flow 05 (`b020c942`): ISO dates (D1), stacked rows below 641 px, the training year on the Trainee's Home (T306's dashboard half). Flow 06 (`85d5a508`): the Coordinator's Home (2.32). Flow 08 (`622c5ad1`): one action a row; a form page of its own (Schedule a review). Flow 09 (`49e7cf5e`): `ConfirmDialog` opening on Cancel. Flow 10 (`68327c9d`): a create page of its own (New MSF campaign) reached from Home's header action. D54 (`f3471780`, `78e95df6`): ending a programme takes the Trainee role. D55 (`fb6e465e`): only a registrar whose programme is running files. The design system is **version 25**, re-synced to flow 10 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `powershell -File design/tools/stage_upload.ps1 -Flow 11`. Open each screenshot. The
   script puts any capture whose name contains "invit", "issued", "resent", "resend" or "reset" in
   `design/upload/crop-first/`. § 4 names eight of these (three step captures of Invitations and five old-frame
   `states/invitations-list--*`), each a page load with no issue or resend in it, so no link is on screen (the link
   shows only on the load that issued it): each was opened on 2026-10-19 and goes as it is. Every
   capture taken just after an issue, a resend or a revoke shows a live registration link and is left out (§ 4's "Do
   not attach"); four of those (`2.6-1`, `2.7-1`, `2.7-2`, `2.17-1`) have names the script does not catch, which is why
   § 4 never names them as attachments.
2. **Start a new canvas** from the main app's Design page, "Wombat 11", on the Wombat design system v25
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Write its `tokens.css` by hand
   from app.css's `:root` before round 1 (§ 11, flow 09: the system serves only `tokens.json`).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Done 2026-10-19: the canvas is
   https://claude.ai/artifact/SFn6dDmJnoww2fhYrQD416 and its mark `/_blob/b3cd1c9ecfc87765138aec8743e383bc`, written
   into § 1 and `design/flows/11-onboarding/round-1-ask.txt`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/11-onboarding/round-1-ask.txt`), with the key screenshots in
   § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's notes for a line
   saying it never received the attachments (§ 11, flow 06); re-attach and name the folder before round 2 if so. Say
   again that every person, address and value comes from § 1 and § 8 (flow 09's round 1 invented people).
5. **Pick a variation for Invitations and a structure for Trainees and admission, confirm or correct Register and the
   pending Home,** say why in a sentence, and answer § 6's questions one sentence each; the operator's are marked. A
   question whose answer rests on an open task names it, and the task lands before the build (§ 11, flow 05). Ask for
   round 2 at fidelity, with its Spec and Steps boards.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/10-msf-campaign/round-2-review.md` did:
   one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system. Put the decisions
   to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/11-onboarding/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 11 — Invite each person with the right role and scope, register from the link, and be admitted

This is flow 11 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals) and 10 (MSF campaigns) are designed and built; the signed-in pages sit inside the shell, and
  Register sits on flow 02's auth card. Round 1: 2–3 variations at FULL FIDELITY for Invitations (where issuing lives
  is open); ONE proposal at fidelity for Register and for the pending registrar's Home, to confirm or correct; and 2–3
  STRUCTURES as wireframes for Trainees and the admission form. I pick before round 2. The runbook steps follow below
  in this same message. The brand mark is at /_blob/b3cd1c9ecfc87765138aec8743e383bc: use it, never a drawn disc. Every person, address, role,
  date and count comes from this message and the steps below; invent none.

GOAL: The Administrator and the Institutional admin must be able to:
    - invite one person at a time by address, with one role and the scope that role takes (institution or College,
      speciality, sub-speciality), offered only the roles and scopes they may issue;
    - be refused in words beside the field at fault, keeping what they typed;
    - copy the one-time registration link once, and never see it again after any other action;
    - follow each invitation's email (being sent, sent, not delivered, check the address), resend one not delivered,
      and revoke one sent in error, knowing what each does to the link already sent.
  The invitee, signed in to nothing, must be able to open the emailed link, see which address and role it is for,
  set a name and a password, and land on Home; or be told plainly why the link no longer works and what to do.
  A registrar who has registered must see that she is waiting, for whom and for what, and that she can do nothing yet.
  The Institutional admin must be able to see who is waiting, admit each to the adopted curriculum with a programme
  start (and an expected completion, or leave it to be derived), see the training year that start gives, and be told
  the admission happened.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-10-19):
  - Invitations is an issue form in a narrow card beside a 10-column table: at 1280 the table is cut after Speciality,
    hiding Issued, Expires, Delivery, Resend and Revoke (2.26); at the old frame a not-delivered cell runs one word a
    line and breaks "delivere / d.". Refusals show at the page's top, far from the field (1.5).
  - After a refused issue or a revoke, the previous invitee's live link stays on screen under the new message
    (Steps 2.6, 2.7, 2.17's Actuals). Revoke is a red in-row button with no confirmation.
  - Roles print as code names everywhere on these pages ("InstitutionalAdmin", "Registering … as CollegeAdmin."). The
    Role picker defaults to InstitutionalAdmin; the Institution picker reads "Select institution" though KGK is the
    only choice; Speciality and Sub-speciality stay on screen, disabled, for roles that take neither.
  - Register drops the names typed when it refuses a password (2.9); the token stays in the address bar; a refused
    link says why but not what to do next (used: no Sign in).
  - The pending registrar's Home is one card naming "your programme administrator": no institution, no programme, no
    person, no word of what happens next (2.18).
  - Trainees puts Pending admission in a narrow card beside Active profiles: at 1280 the email is cut and Institution
    and "Admit to curriculum" are off-screen (2.28); at 390 both tables scroll sideways (A.7.9).
  - The admission form says "Status: Active" for a registrar not yet admitted, shows no training year, and Admit trainee
    lands on the profile page with no word that the admission happened, offering Deactivate and Mark complete with
    today as the last day (2.29). Active profiles shows no programme start and no training year.
  - A failed read on Trainees or the profile page prints the exception's own text, with no Try again.

AUDIENCE:
  - Administrator: the platform operator ("Demo Administrator" on dev). Invites Dr Anton Kruger as the CPSA's
    CollegeAdmin and Prof Nolwazi Mbatha as KGK's InstitutionalAdmin; rarely, at a desk.
  - InstitutionalAdmin: Prof Nolwazi Mbatha, head of department, Kgosi Kgari Teaching Hospital (KGK). Invites nine
    staff and five registrars on one morning, and admits the registrars; at a desk, sometimes on her phone.
  - Invitees, signed in to nothing, often on a phone from the email: Mr Pieter Smit (Coordinator); Dr Thandi Zulu, Dr
    David Naidoo and Dr Sarah Botha (CommitteeMember, Paediatrics); Dr Mohammed Patel and Dr Fatima Khumalo (Assessor,
    Paediatrics / Paediatrics); Dr John van Rensburg (CommitteeMember, external, Stellenbosch, no speciality); Dr
    Refilwe Mokoena (SpecialityAdmin, Paediatrics); Dr Kabelo Sithole (SubSpecialityAdmin, Paediatrics / Paediatrics);
    and five registrars as Trainee, Paediatrics / Paediatrics: Dr Lerato Molefe, Dr Anele Dlamini, Dr Pieter du
    Plessis, Dr Nomsa Mahlangu and Dr Sipho Ndlovu (first mistyped as ndlvou@kgk.wombat.local).
  - Pending trainee: each registrar between registering and admission; Dr Mahlangu keeps her tab open through it.
  - KGK has adopted the College's (CPSA) Paediatric EPA Curriculum 11.1. Training years at admission, from the
    programme start: Molefe 4, Dlamini 3, du Plessis 2, Mahlangu 1, Ndlovu 1.
  - Frequency: each January intake (about two registrars) and each staff join; every user meets it once. Stakes:
    high. It is the first impression, and a role and scope decide who sees what. Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Invitations (Administrator, InstitutionalAdmin; "Issue invitation-only onboarding links and review active
    invitations."): Issue invitation: Email, Role, Institution (or College for a CollegeAdmin), Speciality,
    Sub-speciality, Issue invitation. Role offers the Administrator eight roles, never Administrator; the
    InstitutionalAdmin seven, never CollegeAdmin (T093). Scope by role: CollegeAdmin a College only; InstitutionalAdmin
    an institution only; Coordinator and CommitteeMember a speciality optional, never a sub-speciality; SpecialityAdmin
    a speciality required; SubSpecialityAdmin, Assessor and Trainee both required. Speciality offers the specialities
    of the institution's adopted curricula (KGK: Paediatrics alone, T092); Sub-speciality those of the chosen speciality.
    Refusals: "A college is required for a college administrator.", "Speciality administrators must be scoped to a
    speciality.", "The selected role requires speciality and sub-speciality scope.", "The selected role may not be
    scoped to a sub-speciality.", "An institution is required.", "You do not have permission to issue invitations for
    that institution.", and a malformed address.
    Issued: "Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown
    only once." over "Share this registration link with the invitee. It expires in 14 days. The link is shown here
    only on this page-load — copy it now; it cannot be retrieved later. It is also being emailed to the invitee, and
    the Delivery column below says whether that arrived. If it was not delivered, share this link another way, or press
    Resend, which emails a new link in place of this one." and the link (http://…/account/register?token=…). The form
    goes back to its defaults.
    Active invitations: Email, Role, Institution, College, Speciality, Sub-speciality, Issued ("2026-10-19 09:22", no
    zone), Expires (D+14), Delivery, actions. Delivery is words, not a badge: "Being sent" (muted), "Sent", "Not
    delivered." and, from the second failure, "Its email has failed 2 times, so the mail server may be refusing this
    address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address.",
    then on any row not delivered "Resend emails a new link in place of the current one, which then stops working."
    A row not delivered offers Resend before Revoke; each is named for its row ("Revoke the Trainee invitation to
    ndlvou@kgk.wombat.local"). Resent: "A new invitation link is being emailed to duplessis@kgk.wombat.local. The link
    it replaces no longer works. Copy the new link below — it is shown only once." Revoked: "Invitation revoked.", the
    row gone. A used, revoked or expired invitation leaves the list. Empty: "No active invitations" / "Issue an
    invitation to create the first onboarding link.". An address is never corrected in place (T283).
  - Register (/account/register, static HTML, form post to /account/register/submit, the auth card): "Complete
    registration"; "Registering smit@kgk.wombat.local as Coordinator." (an info alert); Email (shown, not editable),
    First name, Last name, Password (its Show toggle and the six rules: "The new password needs:", "At least 12
    characters.", "At least 4 different characters.", "A digit (0 to 9).", "An upper-case letter.", "A lower-case
    letter.", "A symbol, such as ! or #."), Confirm password (Show), Register. Refusals that bring the form back, the
    first field focused: "The new password needs: At least 12 characters." (each rule broken), "The password
    confirmation does not match.", "Enter your first name and last name, each of at most 100 characters, and a
    password." Refusals with no form: "This invitation has already been used.", "This invitation has been revoked.",
    "This invitation is invalid.", "This invitation has expired.", "A user with this email address already exists.",
    "This email address cannot be used for an account. Ask your administrator for an invitation to another address.",
    "The invitation token is missing.", "Registration could not be completed. Try again." Registered: signed in, Home,
    "<Role> · Semester 2, 2026"; a Trainee invitation registers as Pending trainee.
  - The pending registrar (Pending trainee): menu Home, then My data rights, nothing else (D55). Home "Pending trainee ·
    Semester 2, 2026", no header action, one card "Awaiting admission": "You are registered and waiting to be admitted
    to a curriculum by your programme administrator.", "Review your account →" (My account: Roles "Pending trainee",
    Institution Kgosi Kgari Teaching Hospital). My activities "No activities yet" / "Nothing has been filed about
    you.", no Log an activity. Log an activity and My progress, typed: "You cannot open this page", "Your role (Pending
    trainee) does not open this page.", Go to Home. Admission changes her role; within a minute her open tab goes to
    sign-in with "Your session has ended. Sign in again." (flow 02's), and she returns as a Trainee.
  - Trainees (InstitutionalAdmin; Administrator, no menu item; "Admit pending trainees into a curriculum and maintain
    active trainee profiles."): Pending admission (Name, Email, Institution, "Admit to curriculum" named for the
    registrar: "Admit to curriculum: Lerato Molefe"); "No pending trainees found."; Active profiles (Name, Curriculum
    "Paediatric EPA Curriculum (11.1)", Sub-speciality, Expected completion, Edit); "No active trainee profiles found.";
    Completed & closed profiles (flow 13's: Outcome "Completed 2026-…" / "Withdrawn 2026-…").
  - The admission form (/admin/trainees/edit?userId=…, "Trainee profile" / "Admit a pending trainee into a
    curriculum.", trail Home › Trainees › Trainee profile, Back to trainees): Trainee summary (Email, Name, "Status:
    Active"); Curriculum (only what the institution has adopted, T091, T304; none adopted: "This institution has adopted
    no curriculum yet, so no trainee can be admitted. Adopt one under Curriculum adoptions, then admit the trainee.");
    Programme start date (today); Expected completion date, optional, "Leave this empty to derive it from the curriculum
    window." (derived as start + 12 months on 11.1, T139); Cancel, Admit trainee. Admitted: the profile page
    (?id=…, "Update curriculum and completion details for this trainee."), with Last day in the programme (today),
    Deactivate and Mark complete (flow 13's), Save profile → "Trainee profile saved." (and, after a move or a new start,
    what its replay checked: flow 15's). Refusals: "Only users in the PendingTrainee role can be admitted.", "This user
    already has an active trainee profile.", and the Trainee-role refusal (T303).
  - The training year is derived from the programme start (whole 365-day blocks; "training year N", D17): today only
    My progress and the Trainee's Home show it.
  - Data volumes: none (no invitation, nobody pending); typical (nine staff invitations, then five registrars, one
    revoked and reissued, one not delivered); heavy (a January intake while staff invitations are still open: about
    twenty rows, three not delivered, one at "check the address"; eight registrars pending at once).

WHAT FLOW 11 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the trail, the acting role, access denied, not found, Home's frame); the menus' items
    (propose, do not draw as settled); the auth card's frame and its password fields (flow 02); My account (flow 02);
    the session-ended notice (flow 02); the Homes of the Institutional admin, the CollegeAdmin and the staff roles
    (flows 06, 12 and 16); the profile page's Deactivate and Mark complete (flow 13) and a move to a new version
    (flow 15); the rules above (who may issue what; the scopes by role; one link per invitation, retired by a resend;
    an address never corrected in place; Trainee granted only by admission, T303; nothing filed before admission, D55).
  May, for these pages: their whole layout (where issuing lives, the table's columns and how rows stack, the link's
    place and how it is copied, Pending admission and Active profiles as one list or two, admission as a page of its
    own), the order and grouping of parts, titles and subtitles, where each result and refusal shows and where focus
    goes, confirmations, stacking at 390, the pending registrar's card, and the words. Mark PROPOSED any rule change (a
    refusal that is not built, a mail, a new route, data a page does not read today): those are mine. The runbook's
    Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. Invitations (Administrator; InstitutionalAdmin). The Administrator's form with CollegeAdmin chosen (the College
     picker), its refusal (1.5); none (1.5, 2.1); the institution's form (2.1); issued, with its link, then the next
     issue (2.2–2.5); a refused issue with no link on screen (2.6, 2.7); nine rows (2.7); five registrars "Being sent"
     (2.16); the revoke confirmation and "Invitation revoked." with no link (2.17); one row not delivered, then resent
     with the new link (2.26); sent; check the address; an invalid address; loading; a load error; 390 px.
  2. Register (anonymous, the auth card): the form (1.8, 2.8); a password refused, the names kept (2.9); the
     confirmation refused (2.9); details invalid; used (1.9, 2.11); revoked (2.17); invalid (2.27); expired; the address
     already has an account; the address not accepted; no token; could not be completed; each at 390 px.
  3. The pending registrar (Pending trainee): Home's card (2.18, 2.27) at 1280 and 390, with her menu (2.19); the access
     denied she meets on Log an activity and My progress is flow 01's (2.19), keep it.
  4. Trainees and admission (InstitutionalAdmin): five pending, nobody admitted (2.28); the admission form, with the
     training year as the start is chosen (2.29); admitted, with its result (2.29); four admitted, one pending (2.29);
     an expected completion left empty, derived, then corrected (2.30); none pending (2.30); nothing adopted; an unknown
     id; loading; a load error; 390 px (A.7.9's).

STEPS: 1.5, 1.7–1.11, 2.1–2.11, 2.16–2.19, 2.26–2.32 and 2.44. Pasted verbatim below (Role / Route / Do / Expect). In
  them, D is the day the story is replayed and J the January anchor. Where an Expect quotes today's wording or layout,
  that is today's page, not a requirement on the new one. 1.8, 1.10, 2.8–2.10, 2.32 and 2.44 land on Homes other flows
  designed (keep them); 2.31 on flow 02's session-ended notice; 2.28's users pages are flow 12's (draw only what
  Prof Mbatha reads of the pending registrars there).

STATES TO SHOW:
  Every state named in SCREENS; loading for every signed-in page (the header and a skeleton, no action offered until
  the read is in); a load error (a sentence and Try again, never an exception's text); each confirmation, its cancel,
  and the result after it with the focus on it; a refusal beside the field that caused it, keeping what was typed
  (never a password); 390 px for every page; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T264: Revoke is an outline button named for its row, and confirms in a ConfirmDialog naming the invitee and saying
    the link stops working. A refused issue, a revoke, a resend's refusal or any later action clears the previous
    link from the screen.
  - T315: the invitation token leaves the address bar once the page loads. Register is static, so this cannot depend
    on component code after render (wombat.js, or a cookie and a redirect).
  - T317: a refused registration comes back with the first and last names as typed; never a password, and never the
    names in the address.
  - T286: an invitation to an address that already has an account is refused at issue, beside Email, before anything
    is stored or mailed (PROPOSED words). Register's "already exists" offers Sign in.
  - T306: the admission form shows a read-only "Training year N" that follows the programme start as it is chosen
    (on the date's change, not per keystroke); Active profiles shows the programme start and the training year.
  - T323 (A3): at 1280 nothing on Invitations or Trainees is cut or hidden behind a sideways scroll, and no word breaks
    mid-word; at 390 the lists stack.
  - T324 (A8): roles by their labels ("Institutional admin", "Pending trainee"), never their keys; no message names a
    model property ("The Email field is not a valid e-mail address.").
  - T280: one <h1>, then <h2> in order (the cards are <h3> under the <h1> today); a table's caption does not repeat its
    card's heading.
  - T325 (A9): every time in South African time with the zone ("2026-10-19 09:22 SAST"); expiry and "today" on the
    South African calendar.
  - T329/T272's family: a failed read is fixed words with Try again; no exception's text on any page.
  - T328: date inputs in the body font.
  - A5 (BRIEF.md § 6): every action says what it did where the focus lands: an admission says so ("Lerato Molefe
    admitted …"), focused; a refusal beside its field, keeping what was typed.
  - Done since the old brief, keep them: Trainee is "System-managed", never offered by Add role, granted only by
    admission (T303, 4824d627); the curriculum picker offers only what admission accepts and a save never re-admits
    (T304, 95d4e950); the pending menu is Home alone and nothing is filed before admission (D55, fb6e465e; T141);
    "Acting as Pending trainee", the person's name in the top bar, Home's role line (flow 01); Register's <h1>, Show
    toggles and six rules (flow 02); refusals as fixed words (T285); the tokens' contrast (T322).

QUESTIONS THE DESIGN MUST ANSWER:
  1. Where issuing lives: the form beside the table (today), above it, or a page of its own reached from the list's
     header and Home's "Invite a person" (a PROPOSED route, as flow 10 gave New MSF campaign)? The table must show
     Delivery and the actions at 1280.
  2. The one-time link: where it shows, how it is copied (a Copy button needs wombat.js on an interactive page), and
     how it never outlives the issue or resend that made it.
  3. The issue form: which fields each role shows (hidden, or disabled as today), its defaults (no role chosen; the
     caller's only institution chosen and shown), and where a refusal sits.
  4. The wait: what a Pending trainee's Home tells her: her institution and programme, who invited her, who admits
     her, what happens when she is admitted (she is signed out and back in as a Trainee), and what she can do meanwhile.
  5. Admission: a page of its own (a PROPOSED route) or the profile page as today; where Admit trainee lands, and what
     it says; where the training year sits; Pending admission and Active profiles as one list or two.
  6. Register: what a refused link offers next (Sign in for a used link or an existing account; ask for a new
     invitation otherwise); whether the card carries the lockup and purpose line, as sign-in does.
  7. (Mine) Tell the registrar she has been admitted: a mail, or only her session ending as today?
  8. (Mine) Tell the Institutional admin that a registrar is waiting: a line on her Home (flow 12's), or only the
     Trainees list as today?
  9. (Mine) An expired invitation nobody used leaves the list silently: show it with "Invite again", or keep as today?
  10. (Mine) A second active invitation to the same address is not refused today: refuse it at issue?
  11. (Mine) A signed-in person who opens someone's link gets the form, and registering signs the new account in over
      their session (inferred): show "You are signed in as …; sign out to register" instead?

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
  Since flow 02 also: the auth card (.account-form-container, its <h1>, one message slot), PasswordField with its Show
  toggle (wombat.js on a static page) and PasswordRules. Since flow 03: RefusalSummary, DataTable's stacked rows
  (Stack). Since flow 05: ISO dates, the training year's wording. Since flow 06: the role Homes. Since flow 07: the
  container-query stack. Since flow 08: one action a row, a choice of a few kinds as radios (choice-group), a form page
  of its own (Schedule a review). Since flow 09: a page of its own per list row. Since flow 10: a create page of its
  own reached from Home's header action (New MSF campaign), a draft's rows that stack with their action beside them.
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
  ConfirmDialog. Register is plain HTML with a form post: no script beyond wombat.js, no live validation, the browser's
  own required check. A live registration link is a credential: never draw a real-looking token; write
  "…/account/register?token=•••" on every board.

ASK:
  - Round 1: 2–3 variations at full fidelity for screen 1 (Prof Mbatha's Invitations at 2.1–2.7, 2.16, 2.17 and 2.26;
    the Administrator's at 1.5), each with its reasoning and its step count; ONE proposal at fidelity for screen 2 (Mr
    Smit's and Dr Patel's Register at 2.8–2.9, the used link at 1.9) and for screen 3 (Dr Molefe's Home at 2.18), to
    confirm or correct; 2–3 structures as wireframes for screen 4 (Prof Mbatha's Trainees and the admission at
    2.28–2.30), each with its step count. At 1280 and 390 px. With them, two boards of their own: a Spec board (every
    component each variation uses, NEW marked; the DESIGN.md rules each changes; everything PROPOSED; the data a page
    would need that it does not read today, marked DATA) and a Steps board (each variation's journeys counted, presses +
    fields typed, at 1280 and 390, and which of the 29 Expects below it would change). Stop and wait for my pick.
  - Round 2: the chosen designs with every screen and state above, at 1280 and 390 px, each role's view, every
    confirmation, and where the focus lands after each action; with two boards of their own:
    - a Spec board: every NEW and changed component, its classes, the DESIGN.md text each changes, the tokens, the
      data each page needs that it does not read today (marked DATA), and every message by kind (result, refusal,
      hint, notice, empty) with its words;
    - a Steps board: each of the 29 runbook steps below, and what its Expect will say once built (quoting today's words
      and giving the new ones; "unchanged" where so), with each journey's count (presses + fields typed) at 1280 and
      390: inviting one person, revoking and reissuing, resending, registering, a refused registration, admitting one
      registrar.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Table system; § Form system; § Page-level patterns: List page, Form page, Detail page and the title rule;
    § The invitations list; § Account / auth page; § Button system's destructive action).
  - Mark PROPOSED anything not built: a confirmation, a new route, a mail, a refusal, a rule change.
  - Flag edge cases: a 64-character address; a refused issue after an issue (no link may remain); two invitations to
    one address; an invitation that expires unused; a registrar invited twice by two admins; an Administrator admitting
    a registrar with no institution; an institution that has adopted nothing; a second adopted version (only the
    adopted one is offered); a start date in the future (training year 1, programme not running yet, D55); a start
    more than four years back; a signed-in visitor opening someone's link; Register with script blocked (Show toggles
    hidden, the token cleared by the redirect if that is the choice).
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, focus after every action and confirmation, refusals tied to their fields
    (aria-describedby), targets ≥ 24 px (44 at 390), the link's Copy announced.

ATTACHED: act-2/2.26-1-duplessis-not-delivered.png, act-1/1.5-1-refused-no-college.png,
  states/invitations-list--check-address.png, act-2/2.8-1-smit-register-form.png, act-2/2.9-1-too-short.png,
  act-1/1.9-1-used-link.png, act-2/2.18-1-molefe-pending.png, act-2/2.28-4-trainees-pending.png,
  act-2/2.29-1-admit-form.png, act-2/2.29-3-four-admitted.png and act-A/A.7.9-4-trainees-390.png. Every other capture
  dated 2026-09-26 is an older frame (a Title Case sidebar, an email in the top bar, older alert colours): read its body
  only. No attached capture shows a registration link; the captures that do were left out on purpose.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 1.5 | `/admin/invitations` | `devadmin` | "No active invitations"; eight roles, never Administrator; College in place of Institution; "A college is required for a college administrator." at the page's top; then the issued message, the link once, a row "Being sent" |
| 1.7 | `/admin/invitations` | `devadmin` | InstitutionalAdmin the default; Speciality and Sub-speciality disabled; two rows |
| 1.8 | `/account/register` → `/` | Dr Kruger | "Registering kruger@cmsa.wombat.local as CollegeAdmin.", the address not editable; his Home (flow 16's) |
| 1.9 | `/account/register` | anyone | "This invitation has already been used.", no form (T285) |
| 1.10 | `/account/register` → `/` | Prof Mbatha | "… as InstitutionalAdmin."; her Home with "Invite a person" (flow 12's dashboard; T291's count is a known gap) |
| 1.11 | `/admin/invitations` → `/` | `devadmin` | "No active invitations": a used invitation leaves the list |
| 2.1 | `/account/login` → `/` → `/admin/invitations` | Prof Mbatha | seven roles, no CollegeAdmin (T093); "Select institution" over KGK alone; Speciality disabled for the default role |
| 2.2 | `/admin/invitations` | Prof Mbatha | Speciality optional, Sub-speciality disabled for a Coordinator (T060); the issued message and the link; the form back at its defaults |
| 2.3 | `/admin/invitations` | Prof Mbatha | Speciality offers Paediatrics alone (T092); three CommitteeMember rows |
| 2.4 | `/admin/invitations` | Prof Mbatha | Sub-speciality enabled for an Assessor, filled by the speciality chosen |
| 2.5 | `/admin/invitations` | Prof Mbatha | an external committee member issued with no speciality |
| 2.6 | `/admin/invitations` | Prof Mbatha | "Speciality administrators must be scoped to a speciality.", no row; **today van Rensburg's link stays on screen under it (T264)** |
| 2.7 | `/admin/invitations` | Prof Mbatha | "The selected role requires speciality and sub-speciality scope."; nine rows; **Mokoena's link stays on screen** |
| 2.8 | `/account/register` → `/` → `/account/logout/submit` → `/account/login` | Mr Smit | his address and role named; **the token not cleared (T315)**; his Home (flow 06's); signed out |
| 2.9 | `/account/register` → `/` → `/account/logout/submit` → `/account/login` | Dr Patel | the six rules; "The new password needs: At least 12 characters.", focus on First name, **the names gone (T317)**; then the mismatch; then his Home |
| 2.10 | `/account/register` → `/` → `/account/logout/submit` → `/account/login` | seven staff | each address and role named; each Home as that role |
| 2.11 | `/account/register` | Mr Smit | "This invitation has already been used." |
| 2.16 | `/admin/invitations` | Prof Mbatha | both scope fields required for a Trainee; five rows "Being sent" |
| 2.17 | `/admin/invitations` → `/account/register` | Prof Mbatha | Revoke named for its row, one press, no confirmation; "Invitation revoked." **with the revoked link still shown**; a new `ndlovu@` row; the revoked link refused |
| 2.18 | `/account/register` → `/` → `/account/logout/submit` → `/account/login` | four registrars | "Registering … as Trainee."; Home "Pending trainee · Semester N, YYYY", the Awaiting admission card |
| 2.19 | `/` → `/account/profile` → `/activities/mine` → `/activities/new` → `/portfolio/progress` → `/access-denied` → `/` | Dr Mahlangu | Home and My data rights only (D55); her role and institution on My account; "No activities yet"; access denied on Log an activity and My progress |
| 2.26 | `/admin/invitations` | Prof Mbatha | du Plessis's row "Not delivered. Resend emails a new link …", Resend before Revoke; the resent message and new link; the row "Being sent" |
| 2.27 | `/account/register` → `/` → `/account/logout/submit` → `/account/login` | Dr du Plessis | the old link "This invitation is invalid."; the new one registers him, Awaiting admission |
| 2.28 | `/admin/invitations` → `/admin/users` → `/admin/users/{UserId}` → `/admin/trainees` | Prof Mbatha | "No active invitations"; 15 users, five PendingTrainee (flow 12's list); Molefe's "System-managed", Add role without Trainee and its help (T303); Pending admission's five, "Admit to curriculum: <name>"; "No active trainee profiles found." |
| 2.29 | `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` | Prof Mbatha | only 11.1 offered (T091); the start pre-filled with today; **no training year (T306)**; each admission lands on the profile page with Deactivate and Mark complete, **saying nothing**; four Active profiles |
| 2.30 | `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` | Prof Mbatha | the completion derived as J + 12 months and its help; "Trainee profile saved."; "No pending trainees found." |
| 2.31 | `/` → `/account/session-ended` → `/account/login` → `/` | Dr Mahlangu | her tab sent to sign-in, "Your session has ended. Sign in again." (flow 02's, T279); her Trainee Home with "Training year 1" (flow 05's) |
| 2.32 | `/account/login` → `/` → `/admin/invitations` → `/access-denied` → `/committee/panels` → `/committee/panels/new` → `/access-denied` | Mr Smit | flow 06 and 08's: his Home and menu; access denied on Invitations (T178) |
| 2.44 | `/` → `/admin/users` → `/admin/assessors` | Prof Mbatha | flow 12's: Users by role with no PendingTrainee line; "Invite a person" |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Account and sign-in, § Home,
§ Activities and § Institution administration. The step captures are from the T369 replay (2026-10-19), on the shell as
built. The `states/` captures of Invitations, Trainees and the profile page are from 2026-09-26, on the old frame: use
the step capture where there is one, and the old one for its body only. Register's states are from 2026-09-28, on flow
02's card, current. **A capture marked "link" shows a live registration link: never attach it (§ 4).**

| Page | State | Screenshot | Reached at |
|---|---|---|---|
| `/admin/invitations` | None (Administrator) | `act-1/1.5-0-before.png` (`states/invitations-list--empty.png`, old) | Step 1.5, before the first press |
| `/admin/invitations` | The CollegeAdmin form, refused | `act-1/1.5-1-refused-no-college.png` (`states/invitations-list--college-admin.png`, `--refused.png`, old) | Step 1.5 |
| `/admin/invitations` | Issued, with its link | link: `act-1/1.5-2…`, `act-1/1.7-1…`, `act-2/2.2-1…` to `2.5-1…`, `states/invitations-list--issued.png` | Steps 1.5, 1.7, 2.2–2.5 |
| `/admin/invitations` | Both spent | `act-1/1.11-1-invitations-spent.png` | Step 1.11 |
| `/admin/invitations` | The institution's form, none | `act-2/2.1-1-invitations-empty.png` (`states/invitations-list--institution.png`, old) | Step 2.1 |
| `/admin/invitations` | A refused issue (today under the last link) | link: `act-2/2.6-1…`, `act-2/2.7-1…` | Steps 2.6, 2.7 |
| `/admin/invitations` | Nine rows | link: `act-2/2.7-2…` | Step 2.7 |
| `/admin/invitations` | Invalid address | `states/invitations-list--invalid.png` (old) | Step 2.1, email `not-an-address` |
| `/admin/invitations` | Five rows being sent | link: `act-2/2.16-1…`, `states/invitations-list--being-sent.png` | Step 2.16 |
| `/admin/invitations` | Revoked (today with the revoked link) | link: `act-2/2.17-1…`, `act-2/2.17-2…`; `states/invitations-list--revoked.png` (old) | Step 2.17 |
| `/admin/invitations` | Not delivered | `act-2/2.26-1-duplessis-not-delivered.png` (`states/invitations-list--not-delivered.png`, old) | Step 2.26, before Resend |
| `/admin/invitations` | Resent, with the new link | link: `act-2/2.26-2…`, `states/invitations-list--resent.png` | Step 2.26 |
| `/admin/invitations` | None, after registration | `act-2/2.28-1-invitations-none.png` | Step 2.28 |
| `/admin/invitations` | Sent | `states/invitations-list--sent.png` (old) | Scratch, with an SMTP sink |
| `/admin/invitations` | Check the address | `states/invitations-list--check-address.png` (old) | Scratch, with a refusing sink |
| `/admin/invitations` | Loading | `states/invitations-list--loading.png` (old) | A held read |
| `/admin/invitations` | 390 px | `states/invitations-list--narrow.png` (old) | Step 2.16 at 390 |
| `/admin/invitations` | A Trainee's note (T290) | not captured | Anyone holding Trainee beside an admin role |
| `/account/register` | No token | `states/register--token-missing.png` | Typed `/account/register` |
| `/account/register` | The form | `act-2/2.8-1-smit-register-form.png`, `act-1/1.8-2-kruger-register-form.png` (`states/register--form.png`) | Steps 1.8, 2.8 |
| `/account/register` | Password too short | `act-2/2.9-1-too-short.png` (`states/register--password-short.png`) | Step 2.9 |
| `/account/register` | Confirmation differs | `act-2/2.9-2-mismatch.png` (`states/register--confirmation.png`) | Step 2.9 |
| `/account/register` | Details invalid | `states/register--details-invalid.png` | `&error=DetailsInvalid` added |
| `/account/register` | Used | `act-1/1.9-1-used-link.png`, `act-2/2.11-1-used-link.png` (`states/register--used.png`) | Steps 1.9, 2.11 |
| `/account/register` | Revoked | `act-2/2.17-3-revoked-link.png` (`states/register--revoked.png`) | Step 2.17 |
| `/account/register` | Invalid | `act-2/2.27-1-old-link-invalid.png` (`states/register--invalid.png`) | Step 2.27 |
| `/account/register` | Expired | `states/register--expired.png` | Scratch |
| `/account/register` | The address has an account | `states/register--account-exists.png` | Scratch |
| `/account/register` | Could not be completed | `states/register--general-refusal.png` | A held read |
| `/account/register` | 390 px | `states/register--narrow.png` | Step 2.18 at 390 |
| `/` | Awaiting admission | `act-2/2.18-1-molefe-pending.png`, `act-2/2.27-2-duplessis-pending.png` (`states/home--awaiting-admission.png`, 2026-09-27) | Steps 2.18, 2.27 |
| `/` | Her menu | `act-2/2.19-1-mahlangu-home-menu.png` | Step 2.19 |
| `/account/profile` | Before admission | `act-2/2.19-2-mahlangu-account.png` (`states/profile--pending-trainee.png`) | Step 2.19 |
| `/activities/mine` | Nothing filed | `act-2/2.19-3-mahlangu-my-activities.png` (`states/my-activities--empty.png`) | Step 2.19 |
| `/activities/new`, `/portfolio/progress` | Access denied | `act-2/2.19-4-mahlangu-new-activity.png`, `act-2/2.19-5-mahlangu-progress-denied.png` | Step 2.19 (D55; `states/new-activity--pending-trainee.png` predates D55: not this) |
| `/account/login` | Session ended | `act-2/2.31-1-mahlangu-session-ended.png` (`states/login--session-ended.png`) | Step 2.31 (flow 02's) |
| `/admin/users`, `/admin/users/{UserId}` | Pending trainees | `act-2/2.28-2-users-pending.png`, `act-2/2.28-3-molefe-user.png` (`states/users-list--pending.png`, `states/user-detail--pending-trainee.png`, old) | Step 2.28 (flow 12's pages) |
| `/admin/trainees` | Five pending | `act-2/2.28-4-trainees-pending.png` (`states/pending-trainees-list--pending.png`, old) | Step 2.28 |
| `/admin/trainees` | Four admitted, one pending | `act-2/2.29-3-four-admitted.png` (`states/pending-trainees-list--admitted.png`, old) | Step 2.29 |
| `/admin/trainees` | None pending | `act-2/2.30-3-none-pending.png` (`states/pending-trainees-list--none-pending.png`, old) | Step 2.30 |
| `/admin/trainees` | Loading | `states/pending-trainees-list--loading.png` (old) | A held read |
| `/admin/trainees` | 390 px | `act-A/A.7.9-4-trainees-390.png` (`states/pending-trainees-list--narrow.png`, old) | Step A.7.9 |
| `/admin/trainees/edit` | Admission | `act-2/2.29-1-admit-form.png` (`states/trainee-profile-edit--admission.png`, old) | Step 2.29, before Admit |
| `/admin/trainees/edit` | Just admitted | `act-2/2.29-2-molefe-admitted.png` (`states/trainee-profile-edit--active.png`, old) | Step 2.29 |
| `/admin/trainees/edit` | Completion derived | `act-2/2.30-1-ndlovu-derived.png` | Step 2.30 |
| `/admin/trainees/edit` | Saved | `act-2/2.30-2-ndlovu-saved.png` (`states/trainee-profile-edit--saved.png`, old) | Step 2.30 |
| `/admin/trainees/edit` | Unknown id | `states/trainee-profile-edit--not-found.png` (old) | Typed `?id=999999` |
| `/admin/trainees/edit` | 390 px | `act-A/A.7.9-5-trainee-profile-390.png` (`states/trainee-profile-edit--narrow.png`, old) | Step A.7.9 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Anywhere: a load error on Invitations, Trainees or the admission form (Trainees and the profile page print the
  exception's text today); the Trainee's note on Invitations and Trainees (T290); the address-not-accepted refusal on
  Register; a signed-in visitor on Register.
- Never built: the revoke confirmation; an issue refused because the address has an account (T286) or holds an active
  invitation; the training year on the admission form and Active profiles (T306); an admission's result; Register with
  the names kept (T317); the pending card's new words.
- The admission form when nothing is adopted ("This institution has adopted no curriculum yet, …"); an
  `?userId=` that names nobody pending (today "The pending trainee could not be found.", the exception's text).

**Belongs to other flows:** flow 01: access denied, Home's frame. Flow 02: My account, the sign-out page, the
session-ended notice (`act-2/2.31-1…`), the sign-in page after sign-out (`act-2/2.8-3-signed-out.png`). Flow 06 and 08:
the Coordinator's Home and Decision panels (`act-2/2.32-*`). Flow 12: the users list and user page beyond what 2.28
reads, the Institutional admin's Home (`act-1/1.10-1…`, `act-2/2.44-*`), the staff Homes (`act-2/2.8-2…`, `2.9-3…`,
`2.10-*`). Flow 13: Completed & closed profiles, Mark complete and Deactivate (`states/trainee-profile-edit--complete-dialog.png`,
`--deactivated.png`, `pending-trainees-list--closed.png`). Flow 15: a move to a new version
(`states/trainee-profile-edit--moved.png`). Flow 16: the CollegeAdmin's Home (`act-1/1.8-1…`). Flow 19: the invitation
mail (`mail/InvitationEmail*`, which carries a link).

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-19; none shows a
registration link, a token or a password (the addresses are the cast's `wombat.local` ones). The link shows only on the
page load that issued or resent it, so a capture of a fresh load cannot hold one. Eight go through `crop-first/` by
name only (How to run, step 1): the three Invitations step captures below whose names say "invitations", and the five
old-frame `states/invitations-list--*` captures (check-address, invalid, sent, loading, narrow); each is a fresh load,
was opened, and goes as it is. In every tall capture at 1280 the sidebar's colour stops at 800 px: an artefact of the
capture.

**Key screenshots (attach these first, with § 1):**
1. `act-2/2.26-1-duplessis-not-delivered.png`: the form beside the table; the table cut after Speciality at 1280
2. `act-1/1.5-1-refused-no-college.png`: a refusal at the page's top; CollegeAdmin's disabled scope fields
3. `states/invitations-list--check-address.png`: (old frame) the Delivery cell one word a line, "delivere / d.", red Revokes
4. `act-2/2.8-1-smit-register-form.png`: Register on the auth card, "as Coordinator."
5. `act-2/2.9-1-too-short.png`: a refused password, the names gone, First name focused
6. `act-1/1.9-1-used-link.png`: a used link: an alert and nothing else
7. `act-2/2.18-1-molefe-pending.png`: the pending Home's one card
8. `act-2/2.28-4-trainees-pending.png`: Pending admission cut at 1280
9. `act-2/2.29-1-admit-form.png`: the admission form, "Status: Active", no training year
10. `act-2/2.29-3-four-admitted.png`: Active profiles with no start or year
11. `act-A/A.7.9-4-trainees-390.png`: Trainees at 390, the tables scrolling sideways

**Invitations:** `act-1/1.5-0-before.png`, `act-1/1.11-1-invitations-spent.png`, `act-2/2.1-1-invitations-empty.png`,
`act-2/2.28-1-invitations-none.png`, and (old frame) `states/invitations-list--invalid.png`,
`states/invitations-list--sent.png`, `states/invitations-list--loading.png`, `states/invitations-list--narrow.png`.

**Register:** `act-1/1.8-2-kruger-register-form.png`, `act-2/2.9-2-mismatch.png`, `act-2/2.11-1-used-link.png`,
`act-2/2.17-3-revoked-link.png`, `act-2/2.27-1-old-link-invalid.png`, `states/register--token-missing.png`,
`states/register--details-invalid.png`, `states/register--expired.png`, `states/register--account-exists.png`,
`states/register--general-refusal.png`, `states/register--narrow.png`.

**The pending registrar:** `act-2/2.19-1-mahlangu-home-menu.png`, `act-2/2.19-2-mahlangu-account.png`,
`act-2/2.19-3-mahlangu-my-activities.png`, `act-2/2.19-5-mahlangu-progress-denied.png`,
`act-2/2.27-2-duplessis-pending.png`, `act-2/2.31-1-mahlangu-session-ended.png`.

**Trainees and admission:** `act-2/2.28-2-users-pending.png`, `act-2/2.28-3-molefe-user.png`,
`act-2/2.29-2-molefe-admitted.png`, `act-2/2.30-1-ndlovu-derived.png`, `act-2/2.30-2-ndlovu-saved.png`,
`act-2/2.30-3-none-pending.png`, `act-A/A.7.9-5-trainee-profile-390.png`, and (old frame)
`states/pending-trainees-list--loading.png`, `states/trainee-profile-edit--not-found.png`.

That is 45 files.

**Do not attach** (each shows a live one-time registration link, or another flow's page): the captures taken just after an issue, a resend, or a refusal or revoke under a link left on screen, `act-1/1.5-2-kruger-invited.png`, `act-1/1.7-1-mbatha-invited.png`, `act-2/2.2-1-smit-issued.png`, `act-2/2.3-1-committee-issued.png`, `act-2/2.4-1-assessors-issued.png`, `act-2/2.5-1-vanrensburg-issued.png`, `act-2/2.6-1-mokoena-refused.png`, `act-2/2.6-2-mokoena-issued.png`, `act-2/2.7-1-sithole-refused.png`, `act-2/2.7-2-nine-rows.png`, `act-2/2.16-1-registrars-invited.png`, `act-2/2.17-1-ndlvou-revoked.png`, `act-2/2.17-2-ndlovu-issued.png`, `act-2/2.26-2-duplessis-resent.png`, `states/invitations-list--issued.png`, `states/invitations-list--being-sent.png`, `states/invitations-list--resent.png`; each would have to be cropped first, and § 1 says in words what each shows. Also `states/invitations-list--revoked.png` and `states/invitations-list--refused.png` (old frame; the step captures and § 1 cover them), `states/new-activity--pending-trainee.png` (the type picker D55 removed), `act-2/2.19-4-mahlangu-new-activity.png` (the same access denied as `2.19-5`), the Homes of other flows (`act-1/1.8-1-kruger-home.png`, `act-1/1.10-1-mbatha-home.png`, `act-2/2.8-2-smit-home.png`, `act-2/2.44-1-mbatha-home.png`), `act-2/2.32-*` (flows 06 and 08's) and `mail/InvitationEmail*` (flow 19's, with a link).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `a1a9b120`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T264** (P3, queued; the invitations half) | **Closed in the build (T373):** Revoke… is an outline button named for its row, confirming in a `ConfirmDialog` that names the invitee and says the link stops working (Keep the invitation / Revoke invitation); its result names the address and offers Invite another address, filled from the revoked one (N19); the link is shown only by the issue or resend that made it, in the OneTimeLink, and cleared at the start of the next action or navigation (M5). **Revoke:** an outline button named for its row, confirming in a ConfirmDialog that names the invitee and says the link stops working. **The link:** shown only for the issue or resend that made it. Observed: a refused issue leaves the previous invitee's live link under the refusal (Steps 2.6, 2.7, F-2.6a), and a revoke leaves the revoked link under "Invitation revoked." (2.17). Code: `IssueAsync`'s catch and `RevokeAsync` never clear `_lastIssuedRegistrationUrl` (`I/InvitationsList.razor:362–365`, `:436–442`); only Resend does (`:388`). Revoke is `btn-danger`, one press (`:192–195`). | Steps 2.6, 2.7, 2.17's Actuals; `states/invitations-list--check-address.png` |
| **T323** (A3; the invitations and trainees halves) | **Closed in the build (T373):** the list is the page, the form a page of its own; both Invitations tables and both Trainees tables stack from 900 px down (`StackWide`, S16), the row header breaking only after its `<wbr>` (check rule 1); Delivery and Curriculum keep a floor between 901 and 1100 px (the replay measures at 901, 1000 and 1100). **Invitations at 1280:** the table is cut after Speciality; Issued, Expires, Delivery, Resend and Revoke sit behind a sideways scroll (observed, `act-2/2.26-1…`), because a 10-column `DataTable` shares `.details-grid` with the form (`:51–203`); at the old frame the not-delivered cell runs one word a line and breaks "delivere / d." (`.col-wrap`, `:150`, `:168`). **Trainees at 1280:** Pending admission is cut (email clipped; Institution and Admit off-screen, observed `act-2/2.28-4…`), its rows about 130 px tall. **At 390:** both Trainees tables scroll sideways with Edit off-screen (observed, A.7.9); no `Stack` on any of the four tables. | `act-2/2.26-1…`, `act-2/2.28-4…`, `act-A/A.7.9-4…` |
| **T315** (P2, queued) | **Closed in the build (T373):** the link opens `/account/register/start`, which keeps the token in the register cookie and answers 303 to `/account/register` with `no-store` (S1, S2); no script is involved. The token leaves the address bar on load. Code: `Register.razor:138–147` clears it in `OnAfterRenderAsync`, which a static page never runs (`App.razor` `PageRenderMode`: a signed-out visitor gets none). Observed at 2.8 and 2.27 (F-2.8a). The design adds nothing that needs component code after load. | Steps 2.8, 2.27's Gaps |
| **T317** (P3, queued; the registration half) | **Closed in the build (T373):** a refusal keeps the two names (in the register cookie, never a password, never the address); the refusal sits under its field, focused (G04, G05). A refused registration keeps the names, never a password, never in the address. Code: the endpoint redirects with the token and codes only (`Program.cs:343–393`, `RegisterOutcome.Url`); the inputs render empty (`Register.razor:50–57`). Observed at 2.9 (F-2.9a). T317 recommends a short-lived cookie bound to the token. | `act-2/2.9-1…`, `act-2/2.9-2…` |
| **T286** (P3, queued; the issue half) | **Closed in the build (T373):** Invite a person refuses under Email, before anything is stored or mailed, an address that has an account (N15, N16) or an active invitation (Q10, N17, N18), naming and linking only what is in the caller's scope (M3). **At issue:** an address that already has an account is refused beside Email before anything is stored or mailed. Code: `A/Invitations/IssueInvitation.cs` checks scope only; Register's preview catches it later ("A user with this email address already exists.", no Sign in). **New, code:** nothing refuses a second active invitation to the same address, so two live links can name one person (Q10). | `states/register--account-exists.png` |
| **T306** (P3, queued; the admission half) | **Closed in the build (T373):** Admit's Training year is an `<output>` that follows the start (S18); Current trainees shows the programme start over its training year. **The admission form:** a read-only "Training year N" that follows the start as chosen. **Active profiles:** the programme start and the training year. Observed: neither shows them (2.29, F-2.29a; `T/PendingTraineesList.razor:68–84`, `T/TraineeProfileEdit.razor:94–100`). The Trainee's Home shows it since flow 05 (`b020c942`), so that half is done. | `act-2/2.29-1…`, `act-2/2.29-3…` |
| **T324** (A8; observed) | **Closed in the build (T373), the flow 11 half:** roles by label on the form, the rows, the row names, Register's card and the invitation mail (`WombatRoleLabels`, `InvitationScopeWords`); every form message in Wombat's words. The users pages' keys stay flow 12's. Roles by label. Code: the Role picker, the rows and the row names print keys (`I/InvitationsList.razor:68`, `:135`, `:157`), Register's line too (`Register.razor:28`: "as CollegeAdmin."), though `WombatRoleLabels` exists and the shell uses it. The form's `DataAnnotations` give framework messages ("The Email field is not a valid e-mail address.", `:456–458`). The users pages' keys (2.28, 2.44) are flow 12's. | `act-1/1.5-1…`, `act-2/2.8-1…` |
| **T280** (headings, captions; observed) | **Closed in the build (T373), the flow 11 half:** each section an `<h2>` with its count under the page's `<h1>`; each table's caption says what the list holds, not its heading. The cards' titles are `<h3>` under the page's `<h1>` on all three admin pages (`I/InvitationsList.razor:53`, `:129`; `T/PendingTraineesList.razor:27`, `:59`, `:93`; `T/TraineeProfileEdit.razor:41`, `:56`); each table's visible caption repeats its card's heading ("Active invitations" twice, observed). | `act-2/2.26-1…`, `act-2/2.28-4…` |
| **T325** (A9; code) | **Closed in the build (T373):** Issued is "yyyy-MM-dd" over "hh:mm SAST" (`ActivityMoments.When`); an invitation's last day is a date on the South African calendar (F7), in the list, the OneTimeLink, Register's note and the mail. Issued prints `ToLocalTime()` with no zone (`I/InvitationsList.razor:136`, `:162`): the server's clock. An invitation's expiry is a date compared with "today" (`A/Invitations/ListActiveInvitations.cs:59–61`). | — |
| **T329 / T272's family** (code) | **Closed in the build (T373), the flow 11 half:** every failed read on Invitations, Invite a person, Trainees and Admit is fixed words with Try again, the failure to the log; an unknown or out-of-scope registrar is Admit's not-found panel (M08). A failed read prints the exception's text on Trainees (`T/PendingTraineesList.razor:165`) and the profile page (`T/TraineeProfileEdit.razor:269`, `:353`, `:431`); an admission form for nobody pending prints "The pending trainee could not be found." (`:249`), and one with no id "A trainee profile id or user id is required." (`:264`). No `StatePanel` here takes a `LoadError` or `OnRetry` (`I/InvitationsList.razor:130`, `T/PendingTraineesList.razor:24`, `T/TraineeProfileEdit.razor:36`); Invitations shows a load failure as a page-top danger alert (`:298–301`). | `states/trainee-profile-edit--not-found.png` |
| **T328** (code; observed) | **For the replay (T373):** every control takes the body's font since flow 01 (`font: inherit`, app.css); the build adds no rule of its own, so the replay judges Admit's two dates. Date inputs render in another face, not the body font (the admission form's two dates). | `act-2/2.29-1…` |
| **A5: the admission's result** (observed, not filed) | **Closed in the build (T373):** Admit lands on Trainees with `?admitted=` and the result, read again in scope, focused above the Waiting table (S8); the profile page lost its admission branch, and admission mails the registrar (Q7). Admit trainee navigates to the profile page (`T/TraineeProfileEdit.razor:326`), whose load clears every message (`:206–211`): nothing says the admission happened. The page it lands on offers Deactivate and Mark complete with today as the last day (`:103–125`, `:240`). The summary reads "Status: Active" for someone not yet admitted (`:44`; the model's default, `:479`). Q5. | `act-2/2.29-1…`, `act-2/2.29-2-molefe-admitted.png` |
| **The issue form** (observed, not filed) | **Closed in the build (T373):** Invite a person, a page of its own (Q1): no role chosen; the institution read out for an Institutional admin; only the chosen role's fields drawn (S15); a refusal under its field, focused (Q3); a change of speciality clears the sub-speciality; no inline `style`. Refusals show at the page's top (`ActionResult`, `I/InvitationsList.razor:25–49`), not beside the field (observed, 1.5). The default role is InstitutionalAdmin (`:461`); Institution reads "Select institution" though the caller has one (observed, 2.1, 2.26); Speciality and Sub-speciality stay on screen, disabled, for roles that take neither (`:98–116`). Inferred: Sub-speciality keeps its bound value when the speciality changes (`:269–270` filters the options only), so a change of speciality can be refused "The selected sub-speciality does not belong to the selected speciality." An inline `style` on the link (`:45`). Q3. | `act-1/1.5-1…`, `act-2/2.26-1…` |
| **The wait** (observed, not filed) | **Closed in the build (T373), Q4 and Q7:** the wait card names the institution, the programme and its curriculum, who invited her and who admits her, what admission does, and her address; admission mails her. Q8's line on Prof Mbatha's Home is flow 12's. The pending card names nobody and nothing: no institution, programme or person, no word that admission will sign her out (`W/Dashboards/TraineeDashboard.razor:20–28`). Nothing tells her when she is admitted: admission sends no mail (`A/Trainees/AdmitTrainee.cs`; no template in `Common/Email/Templates/`), her open sessions just end (T279). Nothing tells Prof Mbatha a registrar is waiting: her Home has no such line (`W/Dashboards/InstitutionalAdminDashboard.razor`). Q4, Q7, Q8. | `act-2/2.18-1…` |
| **Register's refusals and frame** (observed, not filed) | **Closed in the build (T373):** each refusal a card with its heading and next step (Sign in for used and exists; ask for a new invitation for revoked and expired); the lockup; `FormField`s with `given-name`/`family-name`. A refused link is one danger alert and nothing else: no Sign in for "already been used" or "already exists", no "ask for a new invitation" for revoked, invalid or expired (`Register.razor:21–25`). The card has no lockup (DESIGN.md: "sign in only"), though it is the first Wombat page an invitee sees. Names and email sit in `.mb-3` wrappers, not `FormField` (`:43–58`; DESIGN.md:4069), with no `given-name`/`family-name` autocomplete. The skeleton (`:17–20`) never shows: the page is static. Q6. | `act-1/1.9-1…`, `act-2/2.8-1…` |
| **Register for a signed-in visitor** (inferred) | **Closed in the build (T373), Q11 and M4:** the page shows "Signed in as someone else" with Sign out and continue (F5), and the submit refuses a signed-in caller first, creating nothing. `Register.razor` has no signed-in check, and a signed-in visitor's pages are interactive (`App.razor` `PageRenderMode`), so a person who opens another's link gets the form, and Register signs the new account in over their session (`Program.cs:374`). Not replayed. Q11. | — |
| **T341** (P2, queued; the register half) | **Closed in the build (T373):** Register's codes travel in the register cookie, so no address carries a refusal. Register's refusals ride in the address as codes (`?error=InvitationUsed`), so a crafted link shows a fixed refusal over a working form. T317's cookie could carry them too. | — |
| **Expired invitations** (code, not filed) | **Closed in the build (T373), Q9:** Invitations' Expired, not used lists them for 30 days, with Invite again (N20). An invitation that expires unused leaves the list (`A/Invitations/ListActiveInvitations.cs:59–61`), and nothing tells the admin; only the Coordinator's Home lists invitations nearing expiry. Q9. | — |
| **T333** (P2, queued), **T289** (P2, queued), **T291** (P3, queued) | SSO mappings that grant Trainee round admission; the assessor profile form; Home's specialities count (1.10, 2.44). Flow 12's pages; name them, do not design them here. | — |

**Dropped from the old brief (done since):**
- T303's user page and Add role (`4824d627`): Trainee is "System-managed" and never offered; a rule now, not a
  requirement.
- T304's picker (`95d4e950`, `a27983fd`): the admission picker offers exactly what admission accepts (the active
  adoption), and a profile save never re-admits and reports its replay.
- D55 (`fb6e465e`) and T141 (`e0a06a2a`): the pending registrar's menu is Home and My data rights. The old screen 3's
  "Activities: the type picker, with 11 instruments" and its nav of six links are gone; `/activities/new` refuses her.
- Flow 01 (`b347e11c`): "Viewing as PendingTrainee" is "Acting as Pending trainee"; Home greets by its heading and role
  line, and the top bar names the person, not the email; the Institutional admin's Home offers "Invite a person".
- Flow 02 (`f50dffb2`): Register's `<h1>`, Show toggles that are 24 px or larger with `aria-pressed` (A4, T328's toggle
  half), the six rules before anything is typed, one refusal per broken rule; the card's input contrast (T322, A1).
- T285 (`a494581e`): refusals as codes with fixed words. The general refusal now reads "Registration could not be
  completed. Try again."
- T290 (`f3471780`): someone holding Trainee beside an admin role sees a note in place of Invitations and Trainees.
- The old screen 5 (the Institutional admin's first Home) is flow 12's dashboard; 1.10 stays in the steps.
- The old brief's DESIGN.md citations (§ The invitations list "from line 1115", § Account / auth page "from line
  1856") are now :2145 and :3976.

## 6. Questions for the operator

Each with a recommendation. The canvas answers 1–6 in its variations; 7–14 are the operator's.

1. **Q1. Where issuing lives** (2.1–2.7, 2.16). *Recommendation:* a page of its own, "Invite a person" (PROPOSED
   `/admin/invitations/new`, trail Home › Invitations › Invite a person), reached from the list's header and Home's
   header action, as flow 10 gave New MSF campaign; it stays on the page after an issue with the link and "Invite
   another", so a morning of nine invitations is not nine round trips to the list. The list then has the width for
   Email, Role and scope, Expires, Delivery and the actions, and stacks at 390.
2. **Q2. The one-time link.** *Recommendation:* shown once, in the issue's (or resend's) result, with its expiry and a
   Copy button (NEW, wombat.js; announced "Link copied"); any next action, refusal or navigation clears it; drawn as
   "…/account/register?token=•••" on the boards. Keep showing it (the mail can fail, and T283's Delivery says when).
3. **Q3. The issue form.** *Recommendation:* no role chosen until she chooses one; a field the role takes no part in is
   not shown (not disabled); the caller's only institution is chosen and shown as text; each refusal beside its field
   (A5); a role's scope rule as help under Role ("An assessor needs a speciality and a sub-speciality.").
4. **Q4. The wait.** *Recommendation:* the card names her institution and the role and scope she was invited to
   ("Trainee in Paediatrics at Kgosi Kgari Teaching Hospital"), who invited her (Nolwazi Mbatha; DATA: the
   invitation's issuer), that an Institutional admin admits her with her programme dates, that admission will sign
   her out once and back in as a Trainee, and that meanwhile she can read My account and My data rights. No date: the
   product has none to give.
5. **Q5. Admission's page and result** (2.29). *Recommendation:* a page of its own (PROPOSED `/admin/trainees/admit`,
   trail Home › Trainees › Admit a registrar) with no end actions, the registrar's name and invited scope, Curriculum,
   Programme start, the training year it gives, Expected completion; Admit returns to Trainees with "Lerato Molefe
   admitted to Paediatric EPA Curriculum (11.1), training year 4." focused. The profile page stays flow 13 and 15's.
   Step 2.29's Expect changes.
6. **Q6. Register's refusals and frame.** *Recommendation:* a used link and "already exists" offer Sign in; revoked,
   invalid and expired say "Ask the person who invited you for a new invitation."; the card carries the lockup and
   purpose line as sign-in does (an invitee's first sight of Wombat).
7. **Q7. (The operator's) Tell the registrar she is admitted.** *Recommendation:* a mail on admission naming the
   curriculum and her training year, with a Sign in link (flow 19's words; T367's `AccountNotices` path), since today
   her session ends with no word why.
8. **Q8. (The operator's) Tell the Institutional admin someone is waiting.** *Recommendation:* a "Waiting for
   admission: 5" line linking to Trainees on her Home, drawn here as PROPOSED for flow 12 to place; no mail.
9. **Q9. (The operator's) Expired, unused invitations.** *Recommendation:* list them for 30 days under "Expired, not
   used" with "Invite again" (issues a new invitation, same role and scope; PROPOSED), so a registrar who never
   registered is not lost.
10. **Q10. (The operator's) Two active invitations to one address.** *Recommendation:* refuse the second at issue
    beside Email: "An invitation to this address is already active. Resend it or revoke it first." (PROPOSED).
11. **Q11. (The operator's) A signed-in visitor on Register.** *Recommendation:* no form: "You are signed in as Pieter
    Smit. Sign out to register mahlangu@kgk.wombat.local." with Sign out (flow 02's page), so one person's session is
    never replaced by another's account.
12. **Q12. (The operator's) An address that already has an account** (T286). *Recommendation:* refuse at issue beside
    Email ("An account with this address already exists. Add the role on the user's page instead." with a link to
    Users), and keep Register's refusal, offering Sign in.
13. **Q13. (The operator's) Role labels.** *Recommendation:* labels on every flow 11 page, the Role picker included
    ("Institutional admin", "Sub-speciality admin"); the runbook's Expects that quote keys change (1.5, 1.7, 1.8, 1.10,
    2.1, 2.6, 2.8, 2.18); the users pages' keys stay flow 12's.
14. **Q14. (The operator's) What the build folds in.** *Recommendation:* fold T264's invitations half, T315, T317's
    registration half, T286's issue half, T306's admission half, and the T323, T324, T280, T325, T328 and T329 halves
    of these pages; fold T341's register half if T317's cookie carries the codes too. T333, T289, T291 and the rest of
    T264, T286, T317 and T341 stay their own.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are Act 1's 1.5–1.11 and Act 2's; § 9): every
  Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 1.5 and 1.7 (the form, the
  labels, where a refusal sits, the link); 1.8, 1.10, 2.8, 2.10, 2.18 ("Registering … as <label>."); 2.1 (defaults);
  2.2–2.7 and 2.16 (the issue's page and its link; no link under 2.6 and 2.7's refusals); 2.9 (the names kept); 2.17
  (the confirmation, no link); 2.18, 2.27 (the pending card); 2.26 (Resend and Revoke's place); 2.28–2.30 (Trainees,
  the admission page, its result, the training year); search the whole runbook (BRIEF.md § 9 item 7) for the words
  that change, including 5.16, 5.17, 5.27, 6.34, 6.36 and A.7.9, which cross Trainees. A new page or route needs a step
  (`Scenario/ScenarioRunbookTests`). A new step takes a letter when its number is taken (§ 11, flow 06). Record
  passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** 2.6, 2.7 and 2.17, no link on screen after a refusal or a revoke; 2.17, a ConfirmDialog
  naming the invitee, the result focused; 2.8, the address bar without the token once loaded (and after a refusal);
  2.9, both names kept and both passwords empty; 2.29, the training year 4, 3, 2, 1 as each start is chosen, and the
  admission's result focused; Invitations and Trainees at 1280 with nothing cut, and at 390 and 768 with nothing
  scrolling sideways; every table captioned; Register still static, with no script beyond wombat.js.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - the invitations, register and trainees pages' own tests (`RegisterPageTests`, the invitations list's, the
    trainee profile's), `Hosting/` for Register's static rendering and the integration suite's `AccountRefusalFlowTests`
    (register), with new ones for the link's clearing, the revoke confirmation, the kept names, the token's removal,
    the training year and the admission's result;
  - `Navigation/NavMenuAuthorizationTests`, `ActiveNavItemTests`; `Hosting/PageTitleTests`;
  - `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones, and the scratch ones on scratch copies, as `states.md` says)
  and the steps' own captures, keeping their names; mask or crop the link in every capture taken just after an issue
  or a resend; `check_baseline_paths.py` at missing 0. Compare them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as each person in § 1: `devadmin`; Prof Mbatha; an invitee on Register in a
  browser signed in to nothing, where the page must work as plain HTML; a Pending trainee; a newly admitted Trainee.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Round 3 was drawn
against each Expect as flow 10's replay left it (T369, 2026-10-19; the Gaps it recorded are 1.10's and 2.44's T291, 2.6,
2.7 and 2.17's T264, 2.8 and 2.27's T315, 2.9's T317, 2.28's T323 and 2.29's T306). Since the build (T373, 2026-10-20)
each Expect describes the product as flow 11 built it, from `R3-0-steps` and the code: 26 of the 29 changed (1.11, 2.31
and 2.32 did not; 1.5's rows are L03's, check rule 5), and each journey's count is in its step's Note (the nine in a
row, 40 presses and 9 fields). `D` is the replay day and `J` the January anchor (README § How to play).

```text
Step 1.5 — Invite Dr Kruger as the CPSA's CollegeAdmin
Role: Administrator — the platform operator
Route: /admin/invitations → /admin/invitations/new → /admin/invitations
Do: Open Invitations and press Invite a person. Enter `kruger@cmsa.wombat.local` and choose College admin. Press Issue
  invitation before choosing a College. Then choose `College of Paediatricians of South Africa` and issue the
  invitation. Copy the link, then press Open Invitations.
Expect:
  - **The list:** "Invitations", "Invitations not yet used, with the state of each one's email.", with Invite a person
    in its header; Active invitations reads "No active invitations" / "Issue an invitation to create the first
    onboarding link.".
  - **Invite a person** is a page of its own (its tab too), under Home › Invitations › Invite a person with Invitations
    lit (T373, Q1): "One person, one role. The registration link is shown here once, and emailed to them.", then "*
    marks a field you must fill in.". Email is starred. Role is a group of eight radios by label, each with its
    description and none chosen: College admin, Institutional admin, Speciality admin, Sub-speciality admin,
    Coordinator, Committee member, Assessor and Trainee; never Administrator. No scope field is drawn before a role is.
  - **College admin** draws College alone, starred, at "Choose a College", offering the Demo College and the CPSA. No
    Institution, Speciality or Sub-speciality field is drawn, disabled or otherwise (S15).
  - **The first press** is refused under College: "A college is required for a college administrator.", the field
    marked and focused. Nothing is issued and no link is shown.
  - **The second press** puts the result above the form, focused: "**Invitation issued for kruger@cmsa.wombat.local.**
    Its email is being sent. Copy the link below — it is shown only once." Under it, "Registration link for
    kruger@cmsa.wombat.local" labels the link (a `/account/register/start?token=…` address) in a read-only field, with
    Copy link beside it, and "It works until `D+14`. It is shown here until your next action, then never again. If the
    email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place
    of this one.", then Open Invitations. Copy link says "Link copied." under the field, and the button reads "Copied"
    (or, where the browser refuses the clipboard, "Could not copy. Select the link and copy it yourself."). The form is
    drawn again, empty, under "Invite the next person".
  - **The row** (L03): "Active invitations" "1 active", captioned "Every institution and College. Not delivered first,
    then newest first. A used or revoked invitation leaves this list."; Invitee kruger@cmsa.wombat.local; Role and
    scope "College admin" over "College of Paediatricians of South Africa"; Issued `D` over its time, "hh:mm SAST";
    Last day `D+14`; Delivery "Being sent"; Revoke…. No Expired, not used section.
  - **The mail:** the application log holds a stub email "Your Wombat invitation", tagged `invitation` and nothing
    else (T373, S12: the role left the tags). Its text reads "You are invited to Wombat as College admin at College of
    Paediatricians of South Africa." and "Register with this link; it works until `D+14`.", then the same link. The log
    never names the address (T282), so the mail's own words tell it from Mbatha's.

Step 1.7 — Invite Prof Mbatha as KGK's InstitutionalAdmin
Role: Administrator — the platform operator
Route: /admin/invitations → /admin/invitations/new → /admin/invitations
Do: Press Invite a person. Invite `mbatha@kgk.wombat.local` as Institutional admin at `Kgosi Kgari Teaching Hospital`.
  Copy the link, then press Open Invitations.
Expect: No role is chosen until one is picked: Institutional admin is chosen here, not a default. It draws Institution
  alone, starred, at "Choose an institution", offering the Demo Institution and KGK; no Speciality or Sub-speciality
  field is drawn. The result and the link read as in Step 1.5, for mbatha@kgk.wombat.local. Active invitations reads
  "2 active": Mbatha's row first (newest first), "Institutional admin" over "Kgosi Kgari Teaching Hospital", then
  Kruger's. The log holds a second stub email "Your Wombat invitation", tagged `invitation`, reading "You are invited
  to Wombat as Institutional admin at Kgosi Kgari Teaching Hospital."

Step 1.8 — Dr Kruger registers
Role: Anonymous — Dr Anton Kruger, invited
Route: /account/register/start → /account/register → /
Do: In a fresh browser session, open the link from the College admin stub email in the application log (the link Step
  1.5 showed). Enter:
  - First name `Anton` and Last name `Kruger`;
  - a password, twice, that meets the policy: at least 12 characters, with an upper-case and a lower-case letter, a
    digit and a symbol.
  Press Register. Write the password to `pwd_DO_NOT_COMMIT.txt`.
Expect: The link answers with a redirect to `/account/register`, so the address bar never holds the token (T315; S2).
  The page is "Complete registration" (its tab too), under the Wombat lockup, and reads "This invitation is for
  kruger@cmsa.wombat.local, as College admin at College of Paediatricians of South Africa." Email holds the address,
  read-only, with "The address this invitation is for. It cannot be changed here."; then First name, Last name,
  Password with the six rules under it, Confirm password, and Register. Under the form: "Not expecting this
  invitation? Ignore it: its link works until `D+14`." Register signs Kruger in and lands on Home, with "College admin
  · Semester N, YYYY" under its heading. The dashboard has one card, National catalogue, with Specialities, EPAs and
  Curricula. The sidebar reads "Acting as College admin" over Home, Specialities, EPAs, Curricula and Activity types,
  then My data rights: the College writes its disciplines' activity types in the builder (T300, D52). The top bar
  names him "Anton Kruger".

Step 1.9 — A used link is refused
Role: Anonymous — anyone who holds Kruger's link
Route: /account/register/start → /account/register
Do: In another fresh session, open Kruger's link again.
Expect: The page is "Invitation already used" (its tab too): "This invitation has already been used: an account was
  registered from it." and "If it was yours, sign in with that account.", with Sign in and "Forgotten your password?".
  It offers no form. The page refuses up front whatever no input could put right (T285).

Step 1.10 — Prof Mbatha registers
Role: Anonymous — Prof Nolwazi Mbatha, invited
Route: /account/register/start → /account/register → /
Do: Register as in Step 1.8, from the Institutional admin stub email in the log, with First name `Nolwazi` and Last name
  `Mbatha`. Write her password to `pwd_DO_NOT_COMMIT.txt`.
Expect: The page reads "This invitation is for mbatha@kgk.wombat.local, as Institutional admin at Kgosi Kgari Teaching
  Hospital." Register lands her on Home, with "Institutional admin · Semester N, YYYY" under its heading and "Invite a
  person" in its header, which opens Invite a person (`/admin/invitations/new`), not the list (T373, Q1). No waiting
  line stands above the cards: nobody waits to be admitted (T376, Q8). The dashboard has three cards and no Quick links
  (T376, Q7):
  - **People:** "1" over "Institutional admin", a link to Users filtered by that role (its name "1 Institutional
    admin"); then "1 account: yours." and Open Users;
  - **Curriculum:** "0" over "specialities adopted" and "0" over "sub-specialities adopted", because KGK has adopted
    nothing yet (T291 item 4); "Nothing is adopted yet. Adopt the College's curriculum to set up a programme."; Open
    Curriculum adoptions;
  - **Your institution:** Name Kgosi Kgari Teaching Hospital, Short code KGK, Contact email
    paeds-admin@kgk.wombat.local, Status "Active" over "Set by a Wombat administrator.", and Open Kgosi Kgari Teaching
    Hospital.
  The sidebar reads "Acting as Institutional admin" over her menu, grouped: Home; People: Invitations, Trainees,
  Assessors, Users; Curriculum: Curriculum adoptions, Curricula, EPAs, Activity types, Entrustment scales; Reviews:
  Decisions due, Committee reviews, Decision panels, Entrustment decisions (T376, F10); Access and audit: SSO mappings,
  Audit log; then My data rights.

Step 1.11 — Both invitations are spent
Role: Administrator — the platform operator
Route: /admin/invitations → /
Do: Reload the invitations list, then go Home.
Expect: Active invitations reads "No active invitations", because a used invitation leaves the list. The dashboard's
  count of registered users is two higher than in Step 1.1.

Step 2.1 — Prof Mbatha opens the invitations page
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

Step 2.2 — Mbatha invites Mr Smit as Coordinator
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

Step 2.3 — Mbatha invites Dr Zulu, Dr Naidoo and Dr Botha as committee members
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue one invitation each to `zulu@`, `naidoo@` and `botha@kgk.wombat.local` as Committee member at KGK, with
  speciality Paediatrics. Copy each link before issuing the next.
Expect: Committee member draws Speciality with no star, offering Paediatrics alone: KGK's adopted specialities (T092).
  No Sub-speciality field is drawn, because a committee member may not be scoped to one. Each issue reads as Step
  2.2's, and each stub reads "You are invited to Wombat as Committee member in Paediatrics at Kgosi Kgari Teaching
  Hospital." Their rows (Step 2.7) read "Committee member" over "Paediatrics".

Step 2.4 — Mbatha invites Dr Patel and Dr Khumalo as assessors
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue an invitation each to `patel@` and `khumalo@kgk.wombat.local` as Assessor at KGK, with speciality Paediatrics
  and sub-speciality Paediatrics.
Expect: For an Assessor, Speciality is starred, with "Then choose its sub-speciality." under it until the
  sub-speciality is chosen. Once Paediatrics is chosen, Sub-speciality is drawn, starred, offering that speciality's
  sub-speciality, Paediatrics, and the hint stays; it goes once Paediatrics is chosen as the sub-speciality. Each stub reads "You are invited to Wombat as Assessor in Paediatrics / Paediatrics at Kgosi Kgari
  Teaching Hospital." Their rows (Step 2.7) read "Assessor" over "Paediatrics / Paediatrics".

Step 2.5 — Mbatha invites Dr van Rensburg as an external committee member
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue an invitation to `vanrensburg@sun.wombat.local` as Committee member at KGK, with the speciality left at Any
  speciality.
Expect: It is issued with no speciality (T060). Its stub reads "You are invited to Wombat as Committee member at Kgosi
  Kgari Teaching Hospital.", and its row (Step 2.7) reads "Committee member" over "Any speciality". The address's
  domain does not matter: the invitation scopes him to KGK.

Step 2.6 — Mbatha invites Dr Mokoena as the programme's SpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations/new
Do: Issue `mokoena@kgk.wombat.local` as Speciality admin at KGK with no speciality chosen. Then choose Paediatrics and
  issue it again.
Expect: Speciality admin draws Speciality, starred, at "Choose a speciality", and no Sub-speciality. The first press is
  refused under Speciality: "Speciality administrators must be scoped to a speciality." (T060), the field marked and
  focused. Nothing is issued, and no registration link is on the page: van Rensburg's, from Step 2.5, went when the
  press began (T264, M5). The second is issued; its stub reads "… as Speciality admin in Paediatrics at Kgosi Kgari
  Teaching Hospital.", and its row (Step 2.7) reads "Speciality admin" over "Paediatrics".

Step 2.7 — Mbatha invites Dr Sithole as the SubSpecialityAdmin
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

Step 2.8 — Mr Smit registers from his link
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

Step 2.9 — Dr Patel's first two attempts are refused
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

Step 2.10 — The other seven staff register
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

Step 2.11 — A used link cannot be used again
Role: Anonymous — Mr Pieter Smit
Route: /account/register/start → /account/register
Do: Open his invitation link a second time.
Expect: As Step 1.9: "Invitation already used", "This invitation has already been used: an account was registered from
  it." and "If it was yours, sign in with that account.", with Sign in; no form.

Step 2.16 — Mbatha invites the five registrars, one to a mistyped address
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

Step 2.17 — Mbatha revokes the mistyped invitation and issues the right one
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

Step 2.18 — Four registrars register and are pending
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

Step 2.19 — What a registrar sees before admission
Role: PendingTrainee — Dr Nomsa Mahlangu
Route: / → /account/profile → /activities/mine → /activities/new → /portfolio/progress → /access-denied → /
Do: Read the menu. Follow Review your account. Type the addresses of My activities, Log an activity and My progress.
Expect: The sidebar reads "Acting as Pending trainee" over Home, then My data rights under the rule, and nothing else: a
  pending trainee files nothing until admitted, so has no activities to list (D55, T141). Her Home's card is Step
  2.18's, its last step naming mahlangu@kgk.wombat.local. My account lists her role as "Pending trainee", and her
  institution, Kgosi Kgari Teaching Hospital. My activities reads "No activities yet", with no Log an activity. Log
  an activity and My progress each show "You cannot open this page" ("Your role (Pending trainee) does not open this
  page.") with Go to Home (T335).

Step 2.26 — Mbatha resends Dr du Plessis's invitation
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

Step 2.27 — Dr du Plessis registers from the new link
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

Step 2.28 — The pending registrars, as Mbatha sees them
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/invitations → /admin/users → /admin/users/{UserId} → / → /admin/trainees
Do: Open Home and read it. Open Invitations. Read the users list and open Dr Molefe's page; press Add a role…, read
  it and press Cancel. Then go Home and press Open Trainees on the line above its cards.
Expect: Home reads, above its cards, "**5 registrars are waiting to be admitted.** Nothing can be filed about them until
  then." with Open Trainees: a standing line, not announced (T376, Q8, S23). People reads Institutional admin 1,
  Speciality admin 1, Sub-speciality admin 1, Coordinator 1, Committee member 4, Assessor 5 and Pending trainee 5, and
  "15 accounts.".
  - Active invitations reads "No active invitations".
  - Users reads "People 15", the five registrars' Roles "Pending trainee" and Scope Paediatrics / Paediatrics.
  - **Molefe's page:** Roles and scope lists Pending trainee, then "System-managed: waiting to be admitted." with Open
    Trainees, and no Remove…, the label and the note apart (T323, F-2.28b). Add a role… offers Institutional admin,
    Speciality admin, Sub-speciality admin, Coordinator, Committee member and Assessor, each with its description, not
    Trainee, and "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with
    'Admit…'." (T303). Cancel closes it and adds nothing.
  Trainees reads "Admit registrars who are waiting, and keep current trainees' programmes." under its heading:
  - "Waiting to be admitted" "5 waiting", captioned "Registered from a Trainee invitation, by surname. A deactivated
    account is not listed. Nothing can be filed about them until they are admitted.": Dlamini, du Plessis, Mahlangu,
    Molefe and Ndlovu, each with Email, Invited as "Paediatrics / Paediatrics", Registered `D` and an outline Admit…
    named "Admit ‹name›". There is no Institution column: her list is KGK's.
  - "Current trainees": "No current trainees" / "A registrar appears here once admitted." There is no Completed and
    closed section.

Step 2.29 — Mbatha admits four registrars
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

Step 2.30 — Dr Ndlovu's completion date is derived, then corrected
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/admit → /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit Ndlovu with start `J` and the completion date left empty. Then open his profile from Current trainees (Edit),
  set completion to `J+4y−1d` and save the profile.
Expect: With start `J` and the completion empty, its help reads "Leave this empty to derive it from the curriculum
  window. Left empty, it will be `J+1y`.": `J` + 12 months, the longest item window on v11.1. The result reads
  "**Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from `J`.** Expected completion
  `J+1y`, derived from the curriculum window: change it under Edit." (only the first sentence bold), and Waiting to be admitted reads "Nobody is
  waiting to be admitted." / "A registrar appears here after registering from a Trainee invitation." His profile is
  "Sipho Ndlovu's programme" (h1, tab and crumb under Trainees), with "Export Sipho Ndlovu's portfolio" in its header,
  End this programme below Profile details, and no Back to trainees (T379). The save reads "Trainee profile saved.",
  and Current trainees shows `J+4y−1d`.

Step 2.31 — Dr Mahlangu's open session ends, and she signs back in as a Trainee
Role: Trainee — Dr Nomsa Mahlangu
Route: / → /account/session-ended → /account/login → /
Do: Her tab from Step 2.18 has stayed open through her admission. Once it leaves Home, she signs in again.
Expect: Within a minute of her admission the tab moves to the sign-in page, which reads "Your session has ended. Sign in
  again.", an information notice; the focus is on Email (no institution buttons on dev). Admission changes her role,
  and a role change ends open sessions (T279). Signed in again, she sees
  "Trainee · Semester N, YYYY" under Home's heading and the trainee dashboard of Step 2.39.

Step 2.32 — Mr Smit, Coordinator
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

Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/invitations/new → / → /admin/users → /admin/assessors
Do: Open Home and press Invite a person, then go back to Home. Press the count beside Assessor on People. Then open
  Users and Assessors.
Expect: Home reads "Institutional admin · Semester N, YYYY" and offers "Invite a person", which opens Invite a person
  (`/admin/invitations/new`), not the list (T373, Q1). No waiting line: nobody waits to be admitted (Q8). Three cards
  and no Quick links (T376, Q7):
  - **People:** Institutional admin 1, Speciality admin 1, Sub-speciality admin 1, Coordinator 1, Committee member 4,
    Assessor 5 and Trainee 5, each count a link named by it ("5 Assessor"), with no Pending trainee; "15 accounts." and
    Open Users;
  - **Curriculum:** "1" over "speciality adopted" and "1" over "sub-speciality adopted", what KGK has adopted, with
    "Paediatrics / Paediatrics, from the CPSA's Paediatric EPA Curriculum (11.1)." and Open Curriculum adoptions;
  - **Your institution:** Name Kgosi Kgari Teaching Hospital, Short code KGK, Contact email
    paeds-admin@kgk.wombat.local, Status "Active" over "Set by a Wombat administrator.", and Open Kgosi Kgari Teaching
    Hospital.
  Assessor's count opens Users filtered by that role (`?role=Assessor`, Role reading Assessor): "People 5 of 15 hold
  Assessor". The users and assessors lists both name Fatima Khumalo (Step 2.41).
```
