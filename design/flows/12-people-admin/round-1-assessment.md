# Flow 12 round 1: assessment (T376 step 5)

Read-only, at HEAD `62f6ec41`. Canvas "Wombat 12" (https://claude.ai/artifact/EUfPe9d3zgnGxvQ9LeSzz8), design system
copied at version 1792508645-23ab (`canvas.json`, `designSystems`). Boards: `design/flows/12-people-admin/round-1/project/*.dc.html`
(44, every board's text read), `canvas.json` and `f12.css`. `W/` = `src/Wombat.Web/Components/Pages/`, `U/` =
`W/Admin/Users/`, `As/` = `W/Admin/Assessors/`, `A/` = `src/Wombat.Application/Features/`, `Id/` =
`src/Wombat.Infrastructure/Identity/`, `Nav/` = `src/Wombat.Web/Navigation/`, `Act1`, `Act2` and `App` =
`execution/knowledge/scenario-paediatrics/act-1-setup.md`, `act-2-onboarding.md` and `appendix-cross-cutting.md`.
**Obs** = observed in the code, the runbook or the boards. **Inf** = inferred. Brief sections are
`design/flows/12-people-admin.md`'s.

## Summary

- **The user page: pick S1** (one page of sections: Roles and scope, Assessor training for an Assessor, Sign-in and
  access). The designer recommends it, and so does the brief (Q1, Q2).
  - **Borrow from S2:** a link that opens one section, so Assessors' "Edit training" lands on the open form. Make it a
    query (`?section=training`, PROPOSED), not S2's tabs.
  - **Borrow from S3:** T289's mended refusals on the command. With the form page gone, they still guard the command.
  - **Reject S2.** Tabs hide the state a lock or a role changes, and a reset or a lock costs one press more (Z2). PageTabs
    would be a NEW component.
  - **Reject S3.** It keeps two pages and 8 + 2 to record training (Z2). It also keeps the profile's own scope, which is
    T289's conflict.
- **Her Home: pick H1** (People, Curriculum and Your institution as cards, with the waiting line above them only when
  someone waits).
  - **Borrow from H3:** Status in the institution card ("Active, set by a Wombat administrator").
  - **Reject H2.** Its two counts are invented (C4), "Needs you" is a new card pattern, and its roles collapse into a
    line that cannot link.
  - **Reject H3.** It keeps Quick links, which the menu repeats, and pushes the work below the institution.
- **Users: pick UA** (one table, Name or email, Role and Status applied with Show, StackWide).
  - **Borrow from UC:** its sentence for an account with no role ("a completed or withdrawn registrar keeps an account
    and its records"), as the help on the Role filter's "No role" and in that row's cell.
  - **Reject UB.** A list of rows whose name is the one link is a rule change (no DataTable, no "Manage ‹name›", against
    2.12's Do).
  - **Reject UC.** Three tables and three captions. At 200 accounts it needs a pager per group, it has no Role filter,
    and a filtered result is split across sections.
- **Her institution's record (I1–I4) and SSO mappings (O1–O4): confirm, with corrections.** For the record: the
  subtitle's claim, the stored contact email, and the words marked NEW. For SSO mappings: show the mappings left from
  a provider that is gone, and the answer to O4's DATA question.
- **The must-fixes, in four kinds** (§ 7):
  - **One model wrong against the code (C1):** every board gives each role its own scope. Wombat stores one scope per
    person (`UserSpecialityScopes`). Zulu's "Committee member · Paediatrics" beside "Assessor · Paediatrics /
    Paediatrics" cannot both be true. This reshapes Q4.
  - **Words that contradict flow 11's decisions (C2):** Add a role's role descriptions are not flow 11's built words.
    One of them is the wording flow 11's C10 rejected.
  - **A wrong claim about the code (C3):** a withdrawn registrar is offered My progress and his training record, not
    "nothing but My account and My data rights".
  - **Invented values (C4):** H2's "Locked out: 1" and "Assessors with no training recorded: 2" at 2.44.
- **The attachments did not arrive.** Z1's Assumptions say "No capture was attached to this session" (Obs). Re-attach
  the ten key screenshots for round 2, name the folder, and ask round 2 to say if they arrived (§ 11, flow 06).
- **The Spec and Steps boards were drawn** (Z1, Z2), as the ask required.

## 1. What was drawn

44 boards in seven groups (`canvas.json` notes rS1, rS2, rS3, rH, rU, rI, rZ):
- **The mark:** the 16 fidelity page boards (UA–UC, I1–I4, O1–O3) use the copied mark
  (`/_blob/b8532f490e278114d4c3b43f9a0533f6`). The wireframes draw a plain top bar.
- **Stylesheets:** every board links `ds/wombat/tokens.css`, `components/bundle.css` and `f12.css`.
- **Inline styles:** the wireframe and strip boards carry inline `style` grids. That is acceptable on a board, and none
  may reach a `.razor` file.
- **Line endings:** the saved files are LF.

| Group | Boards | State drawn |
|---|---|---|
| S1 · sections, training on the person's page | S1a–S1h (1280), S1i (390) | 2.12 her own; 2.13 Zulu with Add a role open, added, a role removed with its confirmation; A.4.5 du Plessis, the reset refused, then done; 2.28 a pending registrar; A.5.5 not found; the Trainee's note; A.6.4/A.6.7 before, the confirmation, locked, reactivated; 2.14 Assessors before any record, Khumalo refused then saved, Zulu read; 2.15 Assessors' five; A.7.9 Dlamini at 390 |
| S2 · tabs | S2a–S2d (1280), S2e (390) | 2.13 on Roles and scope; the training tab appearing; A.4.5 on Sign-in and access; her own, Patel locked, no role; 390 |
| S3 · sections, assessor form kept | S3a–S3c (1280), S3d (390) | 2.13 with a training summary linking out; 2.14 the form mended; 2.15 Assessors as today |
| H · Home | H0 (loading, load error, every page), H1a/b, H2a/b, H3a/b | 2.44; five waiting; first sign-in (1.10); 390 |
| U · Users at fidelity | UA1–3, UB1–3, UC1–3 | 2.13 ten staff; 2.12 "zulu" then Show; A.7.9 at 390, 14 accounts |
| I · her institution's record | I1–I3 (1280), I4 (390) | 1.23 read; A.6.3 saved; Short code emptied; 390 |
| O · SSO mappings | O1, O3, O4 (1280), O2 (390) | A.3.1 no provider; a provider (placeholders), a mapping added; Delete's confirmation |
| Z · Spec, Steps | Z1, Z2 | as the ask's ASK |

**Not drawn in round 1** (round 2's):
- **Users:**
  - no match;
  - 15 with five pending (2.28);
  - a locked row (A.6.4);
  - the heavy volume with its pager;
  - the Trainee's note;
  - its own loading (H0 is generic);
  - the Administrator's view, with Institution;
  - 768.
- **The user page:**
  - a person holding four roles;
  - a 60-character address;
  - loading and a load error drawn on the page;
  - an Administrator's account by id;
  - a Speciality admin added with no speciality, refused;
  - institutional sign-in links listed (Q10);
  - the Administrator's view;
  - 768.
- **Assessors and training:**
  - the empty list (no Assessor holder);
  - `/admin/assessors/edit` redirected, and an unknown id;
  - Assessors at 390;
  - the training section at 390.
- **Home:**
  - the loading and load error drawn as Home's own (H0 says "the same shape");
  - KGK adopted before onboarding (1.21).
- **The institution's record:**
  - the Demo id (a note only, unchanged);
  - `/admin/institutions/new` for her (T291 item 8: the note says it "is never drawn", not what she meets);
  - a malformed contact email.
- **SSO mappings:**
  - a provider at 390;
  - a mapping refused;
  - the Administrator's no-provider view with the setting's name (T326);
  - mappings left when a provider is gone.

## 2. The user page: S1, S2, S3

All three share what follows (Obs).

- **Kept as built:**
  - the h1 by name and the trail Home › Users › ‹name› (`Nav/NavOwners.cs:157`);
  - the own-account note first, with no Remove, Add role, reset or lock (T278, `U/UserDetail.razor:47-50`);
  - the Trainee's note (`A/Users/UserAdministrationRules.cs:83-84`);
  - "User unavailable" / "The user could not be found or is outside your scope." for an out-of-scope or unknown id
    (`U/UserDetail.razor:40`; A.5.5), the same for both, so no existence leaks (`UserAdministrationRules.DemandInScope`,
    `:201-216`);
  - PasswordField, Show and the six rules (flow 02);
  - the reset refusal's words exactly (`Id/UserAdministrationService.cs:515-529`; A.4.5's Actual);
  - the field cleared either way.
- **Proposed, and right to propose:**
  - Pending invitations dropped (Q9);
  - Remove…, Lock out… and Delete… as outline triggers that confirm (T264);
  - Reactivate in one press;
  - each result in its own section, naming the person.
- **True claims about the code:**
  - **"Any open session ends within a minute":** a role change, a reset and a lock each change the security stamp
    (`Id/UserAdministrationService.cs:448, :467, :482, :505`), and `SessionRevalidation.Interval` is one minute
    (`Id/SessionRevalidation.cs:43`; A.6.5's Actual: 55 s).
  - **Sorted by last name** (`A/Users/Queries/ListUsers/ListUsersQuery.cs:69-70`).
  - **The out-of-scope Administrator** reads "User unavailable" (ListUsers and DemandInScope leave out an account with no
    institution).

The three differ as follows.

- **S1, sections.**
  - **Shape:** the who line (address, institution, status), then Roles and scope with Add a role… opening in place.
    Assessor training appears only for an Assessor, read or edited in place. Then Sign-in and access: Password, Lockout
    and Institutional sign-in.
  - **Assessors:** the list of every Assessor holder, its "Record training" or "Edit training" opening the person's
    section. `/admin/assessors/edit` and "Create assessor profile" go.
  - **Counts (Z2):** add a role 5 + 0; record training 4 + 2 from Home, 3 + 2 straight after Add role through the
    result's link; reset 1 + 1; lock and reactivate 3 + 0.
  - **Against the code:** S1 makes the training a section of the person. The command keeps the AssessorProfile row
    (`src/Wombat.Domain/Identity/AssessorProfile.cs`), but its institution and scope stop being asked (Q12).
  - **It changes what Assessors lists.** Today the list reads profiles and never asks the role
    (`A/Assessors/ListAssessorsForSpeciality.cs`, no role filter). S1 lists role holders joined to their profile (DATA,
    PROPOSED). S1c's "the list no longer shows Thandi Zulu" after Remove holds only under that proposal (C9).
  - **The lit item:** Edit training on Assessors lands on a page that lights Users, not Assessors (`NavOwners.cs:157`)
    (Inf). Accept it: the person is the record.
- **S2, tabs.**
  - **Shape:** the same parts behind link tabs (`?tab=`, PROPOSED). The training tab appears only with Assessor.
  - **Cost:** a reset or a lock is one press more. A lock's state (Locked out in the who line) shows, but the effects and
    Reactivate sit behind a tab.
  - **At 390:** the tabs wrap to two lines.
  - **New component:** PageTabs.
- **S3, sections with the form kept.**
  - **Shape:** as S1, with the training read as a one-line summary that links to `/admin/assessors/edit`. The form is
    mended: Institution as text, the picker narrowed to Assessors with no profile, "Choose the assessor.", and a move to
    `?id=` after a save.
  - **Cost:** 8 + 2 to record from Home, and 6 + 2 from the person.
  - **Against T289:** S3 keeps the profile's Speciality, so the save still writes scope. Q12's question remains.

**Recommend S1.** It answers Q1 and Q2 as the brief does. It removes the "new profile" picker and its T289 defects by
construction, and it puts the lock's state and its effects on one page.

**Borrow:**
- **From S2:** a link that opens a section (`/admin/users/{UserId}?section=training`, PROPOSED; a fragment is not
  seen by the server render, Inf).
- **From S3:** the mended refusals ("Choose the training status.", no "UserId") and the empty Assessors body that says
  what a record holds (T326).
- **From Z2's own note:** when KGK has adopted one speciality and one sub-speciality, Add a role draws them as text, as
  flow 11 draws the only institution. Then adding Assessor is 3 + 0, not 5 + 0.

**Correct in S1** (§ 7):
- C1, per-role scope;
- C2, the role descriptions;
- C3, du Plessis's menu;
- C7, Dlamini's lockout list;
- C13, what a lock and a Remove do to waiting requests.

## 3. Her Home: H1, H2, H3

The three show the same values (Obs):
- the counts by label at 2.44 (Institutional admin 1, Speciality admin 1, Sub-speciality admin 1, Coordinator 1,
  Committee member 4, Assessor 5, Trainee 5), matching 2.44's Expect;
- 1 and 1 adopted (T291 item 4, PROPOSED);
- the institution's name, short code and contact email;
- the same three states: five waiting, first sign-in (1.10: "1 account: yours", 0 adopted) and 390.

- **H1, cards.**
  - **Shape:** People (each role a link to Users filtered by it, PROPOSED), Curriculum, Your institution (PROPOSED,
    Q3). A waiting line over them, H1b: "5 registrars are waiting to be admitted. Nothing can be filed about them until
    then. Open Trainees" (PROPOSED, Q8). Quick links dropped.
  - **Against the code:**
    - "Nothing can be filed about them until then" holds under D55;
    - "15 accounts" is DATA: the summary counts per role only, and the roles sum to 18
      (`A/Dashboards/InstitutionalAdmin/GetInstitutionalAdminDashboardSummaryQuery.cs:33-42`). Z1 marks the waiting
      count and the institution, but not the account count;
    - "Locked out: none. (Shown with a count when any.)" draws the line it says is absent (C18).
  - **The role links need UA's Role filter.** UB and UC have none, which is a further reason to pick UA.
- **H2, Needs you then a line of facts.**
  - **Values:** "Locked out: 1" and "Assessors with no training recorded: 2" are drawn at 2.44 and its states. Nobody is
    locked before A.6.4, and every Assessor's training is recorded at 2.14 (C4).
  - **New pattern:** "Needs you" is a new card. The roles become a line with no links.
- **H3, the institution heads Home.**
  - **Shape:** the institution card spans the grid, with People holding "Waiting to be admitted: 0 — Trainees (the line
    bold when above 0)". Quick links are kept.
  - **Against Q7 and Q8:** a zero line always shown is noise, and the waiting work sits under her own record.

**Recommend H1**, with the waiting line only above zero (H1b) and Status added to the institution card (from H3).

**Correct:**
- the contact email is `paeds-admin@kgk.wombat.local` from 1.1 until A.6.3 (`Act1:184`), not "[as stored]";
- "15 accounts" is marked DATA;
- H0's sentence is the house form (C15).

## 4. Users: UA, UB, UC

All three keep what follows (Obs).
- **The rows:** 2.12's ten (Mbatha and the nine staff: `Act2:22-23`), and Zulu alone for "zulu".
- **No Institution column** for her (Q6).
- **The roles by label** (T324), Status as text with a Locked out badge, and Invite a person as a header action
  (PROPOSED).
- **At 390 (A.7.9):** 14 rows. Dlamini and Mahlangu are Trainees, du Plessis and Molefe hold no role, and Ndlovu is
  gone. This is right for the appendix (`App:20-26`: Molefe and du Plessis former trainees; Ndlovu erased at A.1.12,
  before A.4).
- **Unscoped staff:** Smit and van Rensburg read "Any speciality", which is flow 11's Q18 wording and their invitations'
  (`Act2:166-168`).
- **The scope column is per role, which the code does not hold (C1).** Each row draws each role with its own scope.

- **UA, one table with filters.**
  - **Shape:** Name or email (with help), Role (including Pending trainee, Trainee and No role) and Status, applied with
    Show (flow 06), then Clear. The caption is "At Kgosi Kgari Teaching Hospital, by last name …", which does not repeat
    the h1 (T280). The columns are Name over address, Roles and scope (one a line), Status and Manage. StackWide.
  - **After Show:** "People 1 of 10 match "zulu"", with the focus on the list's heading (flow 08).
- **UB, rows.**
  - **Shape:** one field, Show, and a `ul.people-rows` whose name is the row's link. There is no Manage.
  - **Rule change:** a list page not built on DataTable (Z1 says so). It drops "Manage ‹name›" (T239), which 2.12's
    and 2.13's Dos press.
- **UC, grouped.**
  - **Shape:** Staff, Registrars and No role, each a table with its own heading and caption, and no Role filter. A
    filtered result announces "1 of 10 people match "zulu": 1 in Staff.".
  - **Cost:** at 200 accounts each group needs its own pager (Inf). The groups depend on the role, so a person who holds
    Trainee beside a staff role sits in either.

**Recommend UA.** It is the built table mended. Its Role filter is what H1's role links need, and it holds the heavy
volume with one pager.

**Borrow from UC:** the No role sentence. **Pager:** 20 rows to start, as flow 08's Decisions due
(`W/CommitteeDecisions/DecisionsDue.razor:338`). The brief's "50 (flow 08's)" is wrong, and so is Z1's "over 25" (C14).

## 5. Her institution's record (I1–I4): confirm, with corrections

- **Confirm:**
  - the h1, tab and crumb by its name (PROPOSED), with nothing lit (`NavOwners.cs:150` owns the page for the
    Administrator only);
  - Name, Short code and Contact email with the required mark;
  - Status as text, "Set by a Wombat administrator." (PROPOSED words for "Set by a global administrator.");
  - Save changes and Cancel to Home;
  - "Back to home" replaced by the trail;
  - the result naming the change, "Kgosi Kgari Teaching Hospital saved. Its contact email is now
    hod.paediatrics@kgk.wombat.local.", focused;
  - the refusal under its field, the email kept (I3);
  - the Demo id still Page not found (the note).
- **Correct:**
  - **The subtitle:** "Your institution as your people see it in Wombat: …" is untrue for the contact email, which no
    page but the admin pages shows (`ContactEmail` is read only by `W/Admin/Institutions/InstitutionEdit.razor` and
    `InstitutionsList.razor`). Say what the record is for: "Your institution's name, short code and contact email."
  - **"DATA: stored address"** on I1 is wrong: `InstitutionDto` carries `ContactEmail` today. Draw
    `paeds-admin@kgk.wombat.local` (`Act1:184`).
  - **Mark NEW:** "Type a short code. The record was not saved." and "Save changes". Today the refusal is
    DataAnnotations' "The Short code field is required." (`InstitutionEdit.razor:370-376`; T324).
  - **Contact email has no address check** (`:379-380`; brief § 5). Add flow 10's malformed-address words (PROPOSED)
    and draw them.
  - **`/admin/institutions/new` for her (T291 item 8):** draw what she meets. *Recommend* flow 01's access denied, since
    no record is implied.

## 6. SSO mappings (O1–O4): confirm, with corrections

- **Confirm:**
  - O1's words for her (NEW, T326): "Institutional sign-in is not set up", that her people sign in with an email and a
    password, who sets it up, and that there is nothing to change until then;
  - the PROPOSED subtitle;
  - with a provider (O3): her institution fixed (T288), staff roles by label with Trainee and Pending trainee never
    offered (T333, today `W/Admin/Sso/GroupMappings.razor:162-172` offers both), Provider limited to her institution's
    (DATA), "Wombat role and scope" in one column;
  - Delete… confirming, opening on "Keep the mapping" (T264).
- **Correct:**
  - **Mappings left without a provider (C11).** O1 drops Current mappings whenever no provider is set up. A mapping is a
    row keyed by `ProviderKey`, and a provider is a setting, so a provider removed from settings leaves its rows (Inf).
    Keep the list, read-only, when it has rows, and drop only its "Add a mapping above" empty state.
  - **O4's "What becomes of an Assessor role this mapping already gave" (DATA)** is answered by the code. At each
    person's next institutional sign-in, a role that only that mapping gave is taken away and its tracking row removed
    (`Id/SsoGroupMapper.cs:84-137`). Say so: "From each person's next sign-in, belonging to ‹group› no longer gives
    Assessor; anyone who had Assessor only from this group loses it then."
  - **A defect found reading it (Inf, code, not replayed).** The mapper keeps a role that is also "Manual"
    (`:123-127`), but no code writes a Manual `UserRoleAssignment`: Add role writes none. So a role she added by hand,
    which a mapping also gave, is taken at the next sign-in once the mapping stops matching. File it (Q20).
  - **O3's result** "People in ‹group› get Assessor in Paediatrics / Paediatrics at their next sign-in." is right: the
    mapper runs at every institutional sign-in (`:9-12`).

## 7. Corrections

Severity: **must** (round 2 cannot go out without it), **should**, **nit**.

### Wrong against the code or the runbook

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| C1 | UA1–UC3, S1a–S1i, S2a–S2e, S3a–S3d, S1g/S1h "Assesses in", Z1 § Table system | **Scope is one per person, not per role.** `UserDetailDto` carries `SpecialityIds` and `SubSpecialityIds` for the user (`A/Users/UserDtos.cs:186-187`). The scope rows hold `UserId` and `SpecialityId` only (`Id/WombatIdentityUserSpecialityScope.cs:3-9`), and `UpdateScopeAsync` replaces the person's whole set (`Id/UserAdministrationService.cs:336-401`). After 2.14 saves Zulu's profile, her one scope is Paediatrics / Paediatrics (`A/Assessors/CreateOrUpdateAssessorProfile.cs:125-130`), so "Committee member · Paediatrics" beside "Assessor · Paediatrics / Paediatrics" is fiction. A Committee member may not even be scoped to a sub-speciality at invitation (`A/Invitations/InvitationRules.cs:81-82`). At 2.13, before any profile, Zulu, Naidoo and Botha hold Paediatrics alone (their invitations, `Act2:96-106`), so S1g's "Paediatrics / Paediatrics" before any record is wrong too. Draw the person's scope once (the who line, "Paediatrics / Paediatrics"), or Q4 decides a per-role model, which is a migration and a rewrite of every gate that reads the scope claims. | must |
| C2 | S1b, S2a (and Z1 Components) | **Add a role's descriptions are not flow 11's.** They must be `InvitationText.RoleDescription`'s (`W/Admin/Invitations/InvitationText.cs:260-266`). "Speciality admin · Watches one speciality's programme; forms panels." is the wording flow 11's C10 rejected. "Assessor · Rates registrars' work, in one discipline." contradicts C10's "by institution, not sub-speciality" (`NomineeDirectory`). The Institutional admin and Coordinator lines differ as well. Reuse the built ones: "Runs an institution in Wombat: invites and admits people, adopts curricula.", "Runs one speciality's committee work: panels, reviews and decisions due.", "Runs one sub-speciality's committee work. …", "Works across the institution. …", "Sits committee reviews. …", "Assesses the registrars who ask them, at this institution." | must |
| C3 | S1d, S2d | **"Wombat offers Pieter du Plessis nothing but My account and My data rights"** is wrong. A former trainee (trainee record claim, T252) has My progress and My data rights (`Nav/NavItems.cs:211-214`), and Home's card "Your training record" (A.4.6's Actual, `App:661-662`). | must |
| C4 | H2a, H2b | **"Locked out: 1" and "Assessors with no training recorded: 2" at 2.44** are invented. Nobody is locked until A.6.4. All five profiles are saved at 2.14, Botha and Patel without a date, which is not "no training recorded" (`Act2:346-347`). | must |
| C5 | S1g, S1h, S2b, S3c, Z1 Assumptions, Z2 2.14/2.15 | **D is 2026-10-20,** flow 11's replay day, not 2026-10-19 (that was flow 10's). The dates are Zulu 2018-10-20, Naidoo 2020-10-20 and Khumalo 2026-09-20 (2.15's Actual, `Act2:346-347`). | should |
| C6 | UA1, UB1, UC1, S1g, S1h, S2b, S3b, S3c, Z2 2.14 | **Khumalo is "Fatma" until 2.41.** She registered with that name (`Act2:239`) and corrects it at 2.41 (`:968`). 2.15's Actual reads "Khumalo (Fatma)", and 2.44's Expect checks the correction. Boards at 2.12–2.15 read "Fatma Khumalo"; the name sorts the same. | should |
| C7 | S1i (and S1d's lead) | **Dlamini holds Trainee, but her lockout list leaves out "is not a current trainee"**, against Z1's own PROPOSED rule ("only for a person holding Trainee") and against the code (TraineeScopeResolver's current trainee, `U/UserDetail.razor:153-160`). S1i also drops the list's lead sentence. | should |
| C8 | S1a, S1d, S1e, S1f, S2d, I1–I3, Z1 message list | **Built words changed without a NEW mark:** the own-account note (`UserAdministrationRules.cs:102-104`, 2.12's Expect quotes it), the reset help (`U/UserDetail.razor:120`), the reset result (`:403`), "Reactivate user" → "Reactivate" (`:148`; A.6.4's Expect), "Lock out user" → "Lock out…", and "Save" → "Save changes". Changing them is allowed. Mark each NEW in Z1's message list and give its Expect in Z2. | should |
| C9 | S1c, Z1 Edge cases | **"The Assessors list no longer shows Thandi Zulu"** after Remove holds only under S1's PROPOSED list of role holders. Today a profile stays listed whatever the role (`A/Assessors/ListAssessorsForSpeciality.cs`, no role filter), and the profile row is kept (`RemoveRoleAsync`, `Id/UserAdministrationService.cs:451-468`, touches nothing else). Mark it PROPOSED, and add a DATA read. | should |
| C10 | I1–I4, H1a, H2a, H3a | **The record's subtitle claim, the contact email and the DATA mark** (§ 5). | should |
| C11 | O1, O2 | **Mappings left after a provider is removed are hidden** (§ 6). | should |
| C12 | O4 | **Answer the DATA question in the dialog's words** (§ 6). | should |
| C13 | S1c, S1f, Z1 Edge cases | **"Requests already with Thandi Zulu: [stay / move] DATA: Q" and "Requests waiting for Mohammed Patel: [n]".** The code moves nothing: Remove role and a lock change only the role or the lockout end and the stamp (`Id/UserAdministrationService.cs:451-506`). Nothing in `src` reassigns a request. A locked person is left out of new nominations (`src/Wombat.Infrastructure/Activities/NomineeDirectory.cs:46`) and of reminders. At A.6.4 Patel holds one waiting request (A.6.8's Expect, "1 waiting, 1 overdue"). Say "1 request waiting for Mohammed Patel stays with him; he is sent no reminder about it." (count DATA). The Remove dialog says the same. | should |
| C14 | UA (heavy), Z1 PROPOSED | **The pager:** Z1 says "over 25", and the brief "50 (flow 08's)". Flow 08's pager starts at 20 (`DecisionsDue.razor:338`), and `PagerControls` names its select. | nit |
| C15 | H0 | **The load error:** "Could not load Users." The house form is "Could not load the users. Nothing has changed. Try again, or come back in a few minutes." (`InvitationText.cs:42`, `W/Admin/Trainees/AdmissionText.cs:46`). | nit |
| C16 | S1h, S3c | **One training label:** the list reads "Provisional" and the form "Provisional (awaiting sign-off)" (brief § 5). Use one. | nit |
| C17 | Z1 Edge cases | **"Choose the speciality this role keeps."** The issue's refusal is "Speciality administrators must be scoped to a speciality." (`InvitationRules.cs:80`). Reuse it, or mark it NEW. | nit |
| C18 | H1a | **"Locked out: none. (Shown with a count when any.)"** draws the line it says is absent. Leave it out at zero. | nit |
| C19 | S3b | **The picker:** at Khumalo's turn it lists Patel too. 2.14's order is Zulu, Naidoo, Botha, Patel, Khumalo, so Khumalo alone is left. | nit |
| C20 | Z1 Edge cases (4 roles, 60 characters) | **Edge cases described but not drawn:** "Drawn in round 2's heavy board". | nit |

### Invented people or values

- **No invented person** (Obs). Every name, address, role and institution is the cast's (README § Cast, brief § 1).
- **Invented values:**
  - H2's two counts (C4);
  - "[as stored]" for a contact email the runbook gives (`Act1:184`);
  - D a day early (C5).
- **The SSO placeholders** (provider, group ID, display name) are right: no replay reaches a provider.
- **The counts are right:**
  - 10 at 2.12/2.13;
  - 15 at 2.44 (as DATA);
  - 14 at A.1.14 and A.7.9;
  - UC3's Staff 10, Registrars 2, No role 2 (`App:20-26`);
  - five Assessors at 2.14;
  - the 2.44 role counts.

### DATA claims (the build's new reads)

Each is a query or mapper addition. None moves a domain rule, except under Q4.

- **Users:**
  - the scope per row (person's, C1);
  - a role filter, including "No role";
  - a page of 20 with the total;
  - for the No role row, whether the account holds a trainee record (Q13).
- **The user page:**
  - the scope names (the page reads ids only, `UserDtos.cs:186-187`);
  - the institutional sign-in links, provider and linked-on (Q10, T286);
  - the count of waiting requests for the lock and Remove Assessor dialogs (C13);
  - the training record, joined;
  - for a person with no role, the programme's end and why (Q13).
- **Assessors:** every Assessor holder at her institution, joined to a profile if any (C9).
- **Home:**
  - the account count;
  - the waiting count (Pending trainees at her institution);
  - locked accounts;
  - the adopted specialities and sub-specialities (T291 item 4);
  - the institution's name, short code, contact email and status.
- **SSO:** the providers configured for her institution.

### PROPOSED items

Marked PROPOSED on the boards, and right to be:
- Add a role asking scope;
- the three confirmations;
- the results naming the person;
- Pending invitations dropped;
- institutional sign-in links with Remove…;
- training on the person's page and `/admin/assessors/edit` redirected;
- Users' Show, filters and header action;
- Home's waiting line and institution card;
- the record's title by name;
- SSO's not-set-up card, fixed institution and no Trainee;
- the per-person lockout list.

Add these, which are drawn but not marked:
- the section link `?section=training` (a route);
- the Role filter's "No role" (a read);
- H1's role links to a filtered Users (it needs UA's filter);
- the new words of C8;
- the record's address check (§ 5).

### Access control and anonymity

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| C21 | S1e, Z1 | **"User unavailable" stays the same words for unknown and out-of-scope** (A.5.5 unchanged), and that is right. It is DESIGN.md's Unavailable pattern ("one page for a record that does not exist and one the reader may not open", DESIGN.md:1771-1773), with "Go to Users" as its way on. Confirm it: no "no access" variant (flow 01, CLAUDE.md § T056). | — |
| C22 | O3 | **The Provider select** lists only providers for her institution, and her page reads "not set up" when none is hers (Z1 Edge). Keep that, so another institution's provider is never named to her. | — |
| C23 | O3 | **O3 offers Institutional admin through a group.** The command refuses only Administrator (brief § 5, `CreateSsoGroupMapping.cs:41-44`), so an Institutional admin can make every member of an IdP group her peer, with no step in Wombat. That is a rule question, not a board error (Q19). | should |
| C24 | — | **The Administrator's views are not drawn:** Users with Institution, a user page at another institution, Assessors with no menu item (`Nav/NavItems.cs:144-149`), and SSO's no-provider card with the setting's name (T326). Round 2 must draw each once, so the "her institution" words never show to him. | should |

The lock and Remove dialogs show only what she can already read. A waiting-request count is her institution's own
work (Obs).

### Accessibility and the 390 boards

- **Good (Obs):**
  - one h1, and sections as h2 (T280; today h3 under h1 and an h4 with an inline style, `U/UserDetail.razor:54, :96`);
  - every table captioned, with no caption repeating the h1;
  - each row action named for its row and person ("Remove Assessor from Thandi Zulu", "Edit training for Fatima
    Khumalo", "Delete the mapping of … to Assessor"), and RowNames' address only when names repeat (T239);
  - each dialog opening on its safe button and returning to its trigger (flow 09);
  - each result focused;
  - refusals tied by `aria-describedby` and `aria-invalid`, with the stripe;
  - the password never drawn;
  - 44 px at 390 (`f12.css:122-151`).
- **C25 (should):** S1's Add a role… and Edit training open forms in place. That is a disclosure, a NEW pattern. Round 2
  must say:
  - the trigger's `aria-expanded`;
  - where the focus goes on open (the first radio) and on Cancel (the trigger);
  - that the result replaces the form.
  - Write it into DESIGN.md § Form system (Q21).
- **C26 (should):** round 2 must show Users at 1280 with a person of four roles and a 60-character address un-cut
  (T323), and 768 for Users and the user page (§ 11, flow 06).
- **C27 (nit):** S2e and S3d drop the institution from the who line at 390. Keep it, wrapped.

### DESIGN.md and flows 01–11's patterns

Z1 names the changes:
- § Table system (labels and scope, StackWide);
- § Form system (a form in a detail page's section);
- § Button system (outline triggers, the filled danger only in the dialog);
- § Page-level patterns (Detail page as sections; the record titled by its name; Assessors listing role holders);
- § Dashboard page (labels, adopted counts, the waiting line);
- § Alerts (results naming the person).

Two more need text in round 2:
- UA's filter card (`.people-filter`) beside flow 06's FilterBar, if it differs;
- the "No role" row.

Nothing breaks the CSS contract:
- every new class is in `f12.css`;
- colours are tokens (no hex outside them);
- no Bootstrap;
- the wireframes' inline `style` grids are board-only.

## 8. The operator's decisions still needed

Q1–Q7 are the canvas's, answered by round 1, with my recommendation after each. Q8–Q16 are the operator's. Q17–Q22
are new.

1. **Q1. The user page.** Drawn: S1 sections, S2 tabs, S3 sections with the form kept. *Recommend:* **S1**, with a
   section link (`?section=training`, PROPOSED) and the only adopted discipline drawn as text in Add a role.
2. **Q2. Assessor training.** Drawn: on the person's page (S1, S2) or on Assessors' form (S3). *Recommend:* on the
   person's page, with Assessors as the list of every Assessor holder (PROPOSED, DATA). "Record training" or "Edit
   training" opens the section, and `/admin/assessors/edit` redirects to it. An unknown or out-of-scope id gets "User
   unavailable".
3. **Q3. Her own institution.** Drawn: H1's "Your institution" card, the record titled by its name. *Recommend:* as
   drawn, with Status in the card (from H3) and § 5's corrections.
4. **Q4. Role and scope (changed by C1).** Drawn: each role with its own scope, and Add a role asking it.
   *Recommend:*
   - **Keep the person's one scope** (no migration), shown once in the who line and on Users ("Paediatrics /
     Paediatrics").
   - **Add a role asks a scope only when the role needs more than the person holds**, by Invite a person's rules, and
     adds to the person's scope rather than replacing it (PROPOSED: `AddRoleToUserCommand` gains the scope).
   - **Remove takes away no scope.**
   - **The alternative:** a per-role scope, a model change across every scope gate. That is its own task, not this
     flow.
5. **Q5. The lock.** Drawn: "Lock out Mohammed Patel?" with the effects that apply to him. "Keep access" has the
   focus. The result reads "Mohammed Patel is locked out. Any open session ends within a minute." Reactivate takes
   one press. *Recommend:* as drawn, with C13's line about his waiting request, and "Reactivate user" kept or marked
   NEW.
6. **Q6. Users.** Drawn: UA, UB, UC. *Recommend:* **UA**. Show with Clear, Role (with No role) and Status filters, a
   pager at 20, no Institution column for her and one kept for the Administrator, and StackWide. Borrow UC's No role
   sentence.
7. **Q7. Home.** Drawn: H1, H2, H3. *Recommend:* **H1**. People by label with role links, Curriculum 1 and 1, Your
   institution, Quick links dropped, and "15 accounts" read as DATA.
8. **Q8 (the operator's). A registrar waiting.** Drawn: H1b's line. *Recommend:* yes, only above zero, linking to
   Trainees; no mail (flow 11's Q8).
9. **Q9 (the operator's). Pending invitations.** Drawn: dropped. *Recommend:* drop the card, "Revoke all pending
   invitations" and `RevokePendingInvitationsForEmailCommand`, and retire `user-detail--pending-invitations`.
10. **Q10 (the operator's). Institutional sign-in links (T286).** Drawn: "No institutional sign-in is linked." in
    Sign-in and access. *Recommend:* list each ("‹provider›, linked on ‹date›") with Remove…, confirmed. A reset does
    not drop them. Draw one with a provider in round 2. Fold in T286's external-logins half.
11. **Q11 (the operator's). SSO mappings.** Drawn: O1–O4. *Recommend:* T288 and T333 land first as their own tasks.
    The page is read-only for her until a provider exists, with O1's words. Mappings left from a removed provider are
    listed read-only (C11). The Administrator keeps the setting's name.
12. **Q12 (the operator's). What owns an assessor's discipline (T289).** Drawn: "Assesses in … (the Assessor role's
    scope)". *Recommend:* the person's scope (Q4). The profile records training only, and its save writes no scope.
    T289's refusals (Trainee first, the user at her institution) land first.
13. **Q13 (the operator's). A person with no role (D54).** Drawn: "holds no role, so Wombat offers … nothing but My
    account and My data rights" (C3). *Recommend:* "Pieter du Plessis holds no role: he left the programme on
    ‹date›. He can still sign in to read his training record (My progress) and his data rights." For Molefe it reads
    "completed". Add a role offers staff roles, never Trainee. The end and its reason are DATA from the trainee
    profile, and re-entry stays T374's.
14. **Q14 (the operator's). Role labels.** Drawn: everywhere. *Recommend:* yes. The Expects that quote keys change
    (2.12, 2.13, 2.44, A.6.4, A.6.7), and so do the result words (C8).
15. **Q15 (the operator's). Correcting a name or an address.** Drawn: nothing. *Recommend:* not on the user page. The
    person corrects a name on My account (2.41), and an address change is its own task if wanted.
16. **Q16 (the operator's). What the build folds in.** *Recommend:* land T288, T333 and T289 first.
    - **Fold in:**
      - T264's users and SSO half;
      - T326's assessors and SSO items;
      - T291 items 4 and 8;
      - T286's external-logins half;
      - the T323, T324, T280, T325 and T329 halves of these pages;
      - § 7's DATA reads;
      - Q4's scope on Add role.
    - **Stay their own:** T374, Q20's defect and Q19's rule if changed.
17. **Q17 (new). The lockout list per person.** Drawn: "is not a current trainee" only for a person holding Trainee
    (Z1). *Recommend:* yes, so each item is true of the person (C7 for Dlamini). A.6.4's Expect drops that line for
    Patel.
18. **Q18 (new). The profile when Assessor is removed.** Drawn: kept, hidden, shown again if Assessor is given back.
    *Recommend:* as drawn. Assessors lists role holders only, and nothing is deleted.
19. **Q19 (new). Institutional admin through an SSO group** (C23). *Recommend:* refuse Institutional admin in a
    mapping an Institutional admin creates; only the Administrator may map it. This is PROPOSED and folds into T333's
    refusals.
20. **Q20 (new). A hand-given role lost at SSO sign-in** (§ 6). *Recommend:* propose it as a task (Add role records a
    Manual assignment), not part of this flow's build.
21. **Q21 (new). Add a role in place or on a page of its own.** Drawn: in place (S1). Flow 11 put creating on a page of
    its own. *Recommend:* in place. It has one role and at most two fields, with the person in view. Define the
    disclosure in DESIGN.md (C25).
22. **Q22 (new). The pager size.** *Recommend:* 20, flow 08's (C14).

## 9. What round 2's ask must require

- **R1. The Spec and Steps boards.**
  - **The Spec board:**
    - every NEW and changed component with its classes (the who line, the role row, AddRoleForm with its disclosure,
      TrainingSection, the lock and Remove dialogs, the waiting line, the institution card, `.not-set-up`);
    - the DESIGN.md text each changes;
    - the tokens;
    - every DATA read in § 7;
    - every message by kind, the built words and the NEW ones marked (C8).
  - **The Steps board:** each of the 15 Expects with its new words, and each journey's count at 1280 and 390.
- **R2. The picks at fidelity:** S1 (with the section link), H1 (with H3's Status), UA (with UC's No role sentence),
  and I and O as corrected.
- **R3. The states not yet drawn** (§ 1):
  - the user page's and Assessors' loading and load errors drawn on each page;
  - the heavy volume (200 accounts, four roles, 60 characters, a dozen locked, 30 assessors) with the pager;
  - 768 for Users and the user page;
  - the Administrator's view of Users, the user page, Assessors and SSO.
- **R4. The corrections:**
  - C1–C4 (must);
  - C5–C13, C23–C26 (should);
  - the nits as one pass.
- **R5. Real values:**
  - D = 2026-10-20, so 2018-10-20, 2020-10-20 and 2026-09-20;
  - "Fatma Khumalo" before 2.41;
  - KGK's contact email `paeds-admin@kgk.wombat.local` before A.6.3 and `hod.paediatrics@kgk.wombat.local` after;
  - Patel's one waiting request at A.6.4;
  - the scopes as the invitations gave them (`Act2:96-180`) and as 2.14 rewrote them.
- **R6. Q1–Q22 drawn as the operator decides.** Nothing of flows 01–11 is drawn as changed. Users' "Invite a person"
  opens flow 11's page unchanged.
- **R7. The attachments.** Re-attach the ten key screenshots (brief § 4) and name the folder (`design/baseline/`). Ask
  round 2 to say whether they arrived: round 1 says none did (Z1).

## 10. The operator's decisions (2026-10-23)

The operator took every recommendation in § 8 as recommended, and the picks in the Summary (S1, H1 with H3's Status,
UA, I and O confirmed with § 5's and § 6's corrections). Each in one line, with what it means for round 2 and the build:

- **Q1:** S1, one page of sections (the who line, Roles and scope, Assessor training for an Assessor, Sign-in and
  access), with `?section=training` opening the training section (PROPOSED route) and the only adopted discipline drawn
  as text in Add a role. S2 and S3 are dropped.
- **Q2:** training on the person's page; Assessors lists every Assessor holder joined to a profile if any (PROPOSED,
  DATA); "Record training" or "Edit training" opens the section; `/admin/assessors/edit` and "Create assessor profile"
  go, the route redirecting to the person's section, an unknown or out-of-scope id reading "User unavailable".
- **Q3:** H1's "Your institution" card with Status (from H3) links to the record, titled by its name; § 5's corrections
  apply (the subtitle, the stored contact email, the NEW marks, the address check).
- **Q4:** one scope per person, no migration: shown once (the who line, Users); Add a role asks a scope only when the
  role needs more than the person holds, by Invite a person's rules, and adds to it (`AddRoleToUserCommand` gains the
  scope, PROPOSED); Remove takes no scope away. A per-role scope is not this flow's.
- **Q5:** the lock as drawn ("Lock out Mohammed Patel?", the effects that apply to him, "Keep access" focused, the
  result naming him), with C13's line about his waiting request; Reactivate in one press; "Reactivate user" kept or
  marked NEW.
- **Q6:** UA. Name or email, Role (with No role) and Status applied with Show, then Clear; a pager at 20; no Institution
  column for her and one for the Administrator; StackWide; UC's No role sentence as the filter's help and the row's cell.
- **Q7:** H1. People by label, each role linking to Users filtered by it; Curriculum 1 and 1 (T291 item 4); Your
  institution; Quick links dropped; "15 accounts" a DATA read.
- **Q8:** the waiting line above Home's cards only when someone waits, linking to Trainees; no mail.
- **Q9:** Pending invitations goes: the card, "Revoke all pending invitations", `RevokePendingInvitationsForEmailCommand`
  and the state `user-detail--pending-invitations`.
- **Q10:** institutional sign-in links listed in Sign-in and access with Remove…, confirmed; a reset does not drop them;
  one drawn with a provider in round 2; T286's external-logins half folded in.
- **Q11:** T288 and T333 land first as their own tasks; the page is read-only for her until a provider exists, with
  O1's words; mappings left from a removed provider listed read-only (C11); the Administrator keeps the setting's name.
- **Q12:** an assessor's discipline is the person's scope (Q4); the profile records training only and its save writes no
  scope; T289's refusals land first.
- **Q13:** a person with no role reads that they hold none, the programme's end and how (completed or left), and that
  they can still read their training record (My progress) and their data rights; Add a role offers staff roles, never
  Trainee; the end is a DATA read from the trainee profile; re-entry stays T374's.
- **Q14:** role labels on every page here; the Expects of 2.12, 2.13, 2.44, A.6.4 and A.6.7 change, and the result words
  (C8).
- **Q15:** no name or address correction on the user page; the person corrects a name on My account (2.41).
- **Q16:** T288, T333 and T289 land first; the build folds in T264's users and SSO half, T326's assessors and SSO items,
  T291 items 4 and 8, T286's external-logins half, the T323, T324, T280, T325 and T329 halves of these pages, § 7's DATA
  reads and Q4's scope on Add role; T374, Q20's defect and Q19's rule stay their own.
- **Q17:** the lockout list per person: "is not a current trainee" only for a person holding Trainee (C7); A.6.4's
  Expect drops it for Patel.
- **Q18:** the profile is kept when Assessor is removed, hidden from Assessors, and shown again if the role is given
  back; nothing is deleted.
- **Q19:** a mapping an Institutional admin creates may not grant Institutional admin; only the Administrator may map
  it (PROPOSED, folded into T333's refusals).
- **Q20:** proposed as T377 (no code writes a Manual `UserRoleAssignment`, so SSO can take a hand-given role); not part
  of this flow's design or build.
- **Q21:** Add a role opens in place, a disclosure; round 2 defines it (C25) and writes it into DESIGN.md § Form system.
- **Q22:** Users' pager starts at 20, flow 08's (C14).

Round 2's ask is `round-2-ask.txt`: the picks with every state, the decisions as requirements, C1–C27 each with the
board it fixes, § 9's R1–R7, and the Spec and Steps boards. Settled in writing it, PROPOSED there for the operator to
overrule:
- The product stores no pronoun (no such field in `src`), so Q13's and C13's sentences say "they", as the built lockout
  list does (`U/UserDetail.razor:157-161`).
- Q10's "linked on ‹date›" is not stored: `IdentityUserLogin` holds no date, and flow 02's My account lists a sign-in by
  its provider's name only (`W/Account/Profile.razor:166-174`). Each link reads its provider with no date; a date would
  be a migration.
- Assessor needs a speciality and a sub-speciality (`A/Invitations/InvitationRules.cs:81-84`), so under Q4 Add role
  writes Zulu's, Naidoo's and Botha's sub-speciality at 2.13, and their scope reads Paediatrics / Paediatrics from 2.13,
  not from 2.14's save. A Committee member may not be invited with a sub-speciality, but holding both roles, the one
  scope serves both, as today after 2.14.
- An account with no institution reads "Global" on Users (`U/UsersList.razor:50`) and "Global (no institution)" on the
  user page (`U/UserDetail.razor:58`): round 2 gives one wording, NEW.
- Home at 2.28 counts Pending trainee 5: the summary counts every role held by one or more
  (`GetInstitutionalAdminDashboardSummaryQuery.cs:33-42`).
