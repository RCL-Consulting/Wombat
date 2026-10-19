# Flow 11 round 1: assessment (T373 step 5)

Read-only, at HEAD `be8ab2aa`. Canvas "Wombat 11" (https://claude.ai/artifact/SFn6dDmJnoww2fhYrQD416), design system
copied at version 1792413020-e2bb. Boards: `design/flows/11-onboarding/round-1/project/*.dc.html` (53, every board's
text read), `canvas.json` and `f11.css`. `W/` = `src/Wombat.Web/Components/Pages/`, `I/` = `W/Admin/Invitations/`,
`T/` = `W/Admin/Trainees/`, `A/` = `src/Wombat.Application/Features/`, `D/` = `src/Wombat.Domain/`, `Act2` =
`execution/knowledge/scenario-paediatrics/act-2-onboarding.md`. **Obs** = observed in the code, the runbook or the
boards. **Inf** = inferred. Brief sections are `design/flows/11-onboarding.md`'s.

## Summary

- **Invitations: pick B** (a page of its own, `/admin/invitations/new`, PROPOSED). The designer recommends it, and so
  does the brief (Q1). Borrow two things from A:
  - the list page's table as A draws it;
  - one link in the issue's result, "Open Invitations", so the row just issued is a press away.

  Reject C. Its form column reorders below 1100 px against the DOM, and its row results are a new pattern.
- **Trainees and admission: pick W1** (two lists, admission on a page of its own, `/admin/trainees/admit?userId=…`,
  PROPOSED), as recommended. Borrow W3's aim: after Admit, the result sits directly over the next waiting registrar's
  Admit…. W1d already draws this.
- **Register (R) and the pending Home (H): confirm, with corrections.** Register keeps the lockup, the field-level
  refusals, the kept names and the token cookie, with the cookie's SameSite fixed. The Home's card needs Mahlangu's own
  address on H3 and two DATA reads marked.
- **The must-fixes, in four kinds** (§ 5):
  - **Delivery wrong for the replay:** the replay's rows read "Sent", where the log sender gives "Being sent" and
    never "Sent".
  - **Access-control leaks:** T286's and Q10's refusals reach outside the caller's institution. T286's also names
    the account holder.
  - **Wrong claims about the code:**
    - a second invitation "stays active" (it is swept on acceptance);
    - a future start is "Training year 1" (the code gives none);
    - a `SameSite=Strict` cookie that a mailed link would not carry.
  - **Words changed without marking:** built refusals reworded and not marked PROPOSED.
- **The Spec and Steps boards were drawn** (Z1, Z2), as the ask required. Flow 10's round 1 lacked them.
- **Attachments:** no board says they did not arrive (Obs). No board shows a fact found only in a capture, so their
  arrival cannot be confirmed (Inf). Re-attach the key eleven for round 2 and name the folder.

## 1. What was drawn

53 boards in eight groups (`canvas.json` notes tA–tZ). Every page board uses the copied mark
(`/_blob/b3cd1c9ecfc87765138aec8743e383bc`). All 53 link `ds/wombat/tokens.css`, `bundle.css` and `f11.css`. The
`ds/` files were not saved with the boards, so whether `tokens.css` was written by hand is not checked here. Every
link is drawn `…/account/register?token=•••` (Obs, 11 boards). The saved files are LF.

| Group | Boards | State drawn |
|---|---|---|
| A · issue above the list | A1–A6 (1280), A7, A8 (390) | 2.5 issued; 2.6 refused (van Rensburg's link gone); 2.17 revoked; 2.26 resent; heavy (20 rows); 1.5 the Administrator, College refused; 2.16 Molefe issued, Copied |
| B · a page of its own | B1–B6 (1280), B7, B8 (390) | the same states: issue and refusal on "Invite a person"; revoke, resend and heavy on the list; the Administrator's eight roles as radios |
| C · beside the list, as rows | C1–C6 (1280), C7, C8 (390) | the same states, with the form in a 22rem column; the revoke result and the resent link inside the row |
| S · shared | S1, S2 (390) revoke confirmation; S3 every issue refusal; S4 loading; S5, S6 (390) load error | all variations |
| R · Register | R1, R2 (390) form; R3 password refused; R4 (390) confirmation refused; R5, R6 (390) used; R7 seven other refusals and Q11, plus the T315 mechanism | 1.9, 2.8, 2.9, 2.11, 2.17, 2.27 |
| H · the pending Home | H1, H2 (390), H3 (390, menu open) | 2.18, 2.19 |
| W · Trainees (wireframes) | W1a–W1d, W2a–W2d, W3a–W3c | 2.28–2.30; each structure at 390 |
| Z · Spec, Steps | Z1, Z2 | as § 7 of the ask |

**Not drawn in round 1** (round 2's):
- **Invitations:**
  - 2.1's empty institution form;
  - 2.7's nine rows as a step board (A5 holds them inside the heavy set);
  - 2.16's five "Being sent";
  - 2.26 before Resend (du Plessis alone, not delivered);
  - the Administrator's list with rows (1.7: Kruger's College row and Mbatha's institution row);
  - "Sent" against a real mail server;
  - the T290 note;
  - 768.
- **Register:**
  - details invalid (the ask's SCREENS list);
  - every refused link at 390 (R7 is 1280 only);
  - UserNotLoaded.
- **The pending Home:** an SSO-provisioned pending registrar, who has no invitation (T333).
- **Trainees:**
  - nothing adopted;
  - an unknown `userId`, or one not pending;
  - loading and a load error;
  - the Administrator's view (any institution);
  - a future start;
  - a start more than four years back;
  - the T290 note;
  - the correction of 2.30 on the profile page (flow 13's page, unchanged).

## 2. Invitations: A, B, C

All three share the same form rules, the OneTimeLink, the revoke dialog (S1) and the refusals (S3). Shared, and right
against the code (Obs):

- **The role labels** are `WombatRoleLabels`'s exactly (`D/Identity/WombatRoleLabels.cs:13-22`).
- **The offer:** the Administrator gets eight roles, never Administrator; the Institutional admin gets seven, without
  College admin (`I/InvitationsList.razor:214-256`).
- **The scope rules** match `RequiresSpeciality` and `RequiresSubSpeciality` (`:260-264`).
- **The refusal words** are `A/Invitations/InvitationRules.cs:38-68`'s.
- **The built words are kept:** the issued and resent results (`I/InvitationText.cs:64-70`), the check-the-address
  sentence and `ResendRetiresTheLink` (`:50-61`).
- **The rows are named for their invitation** as `RowNames` does (`:132-137`).
- **The counts are right:**
  - 7 active at 2.5 and 2.6;
  - 4 after 2.17's revoke (the staff registered at 2.8–2.10);
  - 1 at 2.26 and at 2.16's first issue;
  - 20 in the heavy set: 9 staff, 3 cast registrars, ndlvou@, du Plessis and six ‹further› rows.
- **Expiry** 2026-11-02 (D+14; `D/Invitations/Invitation.cs:79`).
- **The malformed-address words** reuse flow 10's (`A/MultiSourceFeedback/AddMsfInvitation.cs:212`).
- **No link remains** after a refusal or a revoke (A2, B2, C2, A3, B3, C3): T264 holds on every board.

The three differ as follows:

- **A, issue above the list.**
  - **Shape:** one page as today, re-stacked. The OneTimeLink first, then the form at full width, then the table at
    full width with six columns. "Invitee" is the row header, and "Role and scope" holds one cell.
  - **Against the code:** A keeps the route and `NavOwners` as they are. It moves every result to the page's top, so
    a Resend pressed on the twentieth row (A5) sends the focus up past the form. That is the built ActionResult
    behaviour, and acceptable.
  - **Against the runbook:** 2.2–2.7 read as today with the link moved. 2.26's result sits at the top.
  - **Risk (Inf, not rendered):** at 1280 the content column is about 970 px (`--sidebar-width: 250px`, app.css:90).
    The table carries a 12rem Invitee, three `nowrap` columns (Issued, Delivery, the two-button actions) and stacks
    only below 900 px. A not-delivered row with Resend and Revoke… may still overflow, which is T323. Round 2 must show
    the A5/B5 1280 board un-scrolled.
- **B, a page of its own.**
  - **Shape:** Invitations is the list alone, with "Invite a person" in its header. "Invite a person" is the form,
    with Role as radios whose descriptions say what each role does. After an issue it stays, with the link above "Invite
    the next person".
  - **Fits Q1 and flow 10:** Home's "Invite a person" lands on the form (today it opens the list,
    `src/Wombat.Web/Navigation/HomeFrame.cs:51`; the change is PROPOSED). Nine issues in a row never scroll past a
    list. The list page is the widest of the three.
  - **The cost:** one press from the list (Z2: 4 + 1 against 3 + 1). B1's link help says "press Resend on its row",
    but the row is on another page.
  - **Radios:** eight radios (B6), each with a description, make B7 2300 px tall at 390. Each is one press, and the
    descriptions answer "which role is this person". Keep them. Two descriptions are wrong (§ 5, C10).
- **C, beside the list, as rows.**
  - **Shape:** today's arrangement rebuilt as a `ul.inv-rows` list (no table, no caption), with the form in a 22rem
    aside. A revoked row stays as its result (`inv-row--done`), and a resent link opens inside its row.
  - **Against flows 01–10:** each in-row result is a new result pattern, not ActionResult.
  - **Accessibility:** below 1100 px `.inv-aside{order:-1}` (f11.css:72) shows the form first while the DOM and focus
    order keep the list first (WCAG 1.3.2, 2.4.3).
  - **Width:** at 1280 the rows get about 950 − 22rem ≈ 600 px, the designer's own stated risk.

**Recommend B.** It answers Q1 as the brief and flow 10 did, keeps the link with the form across a morning of
issues, and gives the list the page.

**Borrow:**
- **From A:** the list page is A's table and caption as drawn.
- **From A's single page:** the issued result offers "Open Invitations" (a link), so the row just issued, "Being sent",
  is one press away. B1 shows no list at all.
- **From C:** nothing structural. C3's "This line goes at your next action." says the result is transient, and B's
  OneTimeLink help already says so.

## 3. Trainees and admission: W1, W2, W3

All three show the same form content:
- the curriculum read out when one is adopted (a select when two are);
- Programme start, today until changed;
- "Training year N", read-only and following the start on change (T306);
- Expected completion, optional, with the built help;
- the "When you admit" consequence line.

The counts are equal: 2 presses + 2 fields, or + 1 with the completion left empty (Z2). The years are right for
J = 2026-01-15 (`Act2:554-559`): Molefe 4, Dlamini 3, du Plessis 2, Mahlangu 1, Ndlovu 1. The derived completion is
J + 12 months (`Act2:572-575`).

- **W1, two lists, admission a page.** "Waiting to be admitted · N" over "Current trainees", then Completed and closed
  (flow 13's, unchanged). Admit… opens "Admit Lerato Molefe" at the PROPOSED `/admin/trainees/admit?userId=…`, and
  Admit returns to Trainees with the result focused.
  - It never lands on the profile page, so Deactivate and Mark complete are not offered at admission. That cures A5's
    problem (`T/TraineeProfileEdit.razor:326`).
  - The page has room for every edge case: nothing adopted (the built `NothingAdopted` words), an unknown id, a
    future start, a start far back.
- **W2, one list filtered by status, admission in a dialog.**
  - The status chips are a filter pattern no flow has built (NEW, unmarked).
  - With Waiting chosen, Current is hidden, so 2.28's Expect, which reads both lists, takes a press.
  - The dialog is labelled "ConfirmDialog, form mode". Form mode is a browser post (`FormAction`,
    `src/Wombat.Web/Components/Shared/ConfirmDialog.razor:114-124`). A circuit dialog with fields is the action mode
    with `FieldContent` and `InitialFocusId` (T366, `:96-102`, `:129-134`).
  - The edge cases have no room in a dialog.
- **W3, admission inside the row.** The next registrar stays in view. But the open row (W3c, 1500 px at 390) pushes
  the list down. The "editing stripe" form inside a list is a new pattern for a two-date form. When Admit removes the
  row, the result has nowhere stable to sit (W3b puts it at the top anyway).

**Recommend W1.** It is Q5's page of its own and holds the edge cases. It keeps admission apart from the profile
page, which is flows 13 and 15's, and it is the only structure with a title and trail of its own.

**Borrow:** W3's aim. On return, the result sits directly above Waiting's next Admit…, so five in a morning are five
round trips with no scrolling (W1d already draws it). Keep "Cancel" on the page; W2's "Not yet" belongs to a dialog.

**Confirm:**
- the h1 "Admit Lerato Molefe" (it names whom, as flow 10's "MSF campaign for …" does), with the tab
  "Admit Lerato Molefe · Wombat" and the trail Home › Trainees › Admit Lerato Molefe;
- Trainees lit (`aria-current="true"`).

New § 6 Q17 asks the operator to confirm the h1.

## 4. Register and the pending Home

### Register (R1–R7): confirm, with corrections

- **Confirm:**
  - the lockup and purpose line, as sign-in has them (`W/Account/Login.razor:32-36`; this changes DESIGN.md:3986's
    "sign in only");
  - the info line naming address, role label, scope and institution;
  - Email `readonly` with `autocomplete="username"` (today it is `disabled`, `W/Account/Register.razor:45`, which a
    password manager skips);
  - `given-name` and `family-name`;
  - no `*` (DESIGN.md § Account / auth page: every field is required, none marked);
  - each refusal under its field, with `aria-invalid` and `aria-describedby`;
  - the names kept and both passwords empty (T317);
  - each refused link as an h1 and its next step: Sign in for used and for exists, "Ask the person who invited you
    for a new invitation." otherwise;
  - the T315 mechanism: a GET with the token sets a cookie and answers 303 to `/account/register`, so no script is
    needed;
  - Q11's static "Signed in as someone else" card.
- **The focus moves** from First name, today (`Register.razor:51`; Step 2.9's Expect), to Password. That is right,
  since the names are kept. Z2's 2.9 says so.
- **Correct:**
  - **The cookie's `SameSite=Strict`** (R7) would not be sent on the 303 that follows a link opened from webmail. The
    navigation began on another site, and Chromium withholds Strict cookies across that redirect chain (Inf, browser
    behaviour). The page would then say the token is missing. Use `SameSite=Lax`, Data-Protection-protected, as
    `src/Wombat.Web/Navigation/ActingRoleSwitchResults.cs:16, :169` does. The refused-registration redirect must stop
    putting the token back in the address too (`RegisterOutcome.Url`, `src/Wombat.Web/Program.cs:357, :371, :390`).
  - **`novalidate` on Register's form** (R1–R4) removes the browser's required check, which § 1's constraints keep
    for this static page. S3's own note assumes that check. Drop it on Register.
  - **Reworded built refusals, not marked PROPOSED:**
    - R4 adds "Type the password again in both fields.";
    - R5 adds ": an account was registered from it.";
    - R7 rewords no token ("missing from this link"), expired ("an invitation lasts 14 days"), invalid and the
      fault ("Nothing was saved.").

    These are T285's fixed words (`src/Wombat.Web/Security/RegisterOutcome.cs:70-83`), and the runbook quotes them
    (1.9, 2.9, 2.11, 2.17, 2.27). Changing them is allowed (§ 1 "the words"). Mark each NEW in the Spec's message
    list, and give each Expect its new words in the Steps board.
  - **The info line and "its link stops working on 2026-11-02" need DATA.** `InvitationPreviewDto` carries `Email`
    and `TargetRole` only (`A/Invitations/InvitationDtos.cs:5`). Z1 marks the names. Add `ExpiresOn` too.
  - **The signed-in visitor:** a signed-in visitor's pages are interactive (`W/../App.razor:110-114`). Q11's card and
    the cookie redirect must be served static for them as well. This is a build note (Inf).

### The pending Home (H1–H3): confirm, with corrections

- **Confirm:**
  - one emphasis card, "Awaiting admission";
  - what she waits for and where;
  - "Until then, nothing can be filed by you or about you." This is right under D55: `CreateDraftAsync` refuses a
    subject with no active profile;
  - "When you are admitted": her start and year set, signed out once, back as a Trainee (T279);
  - "Meanwhile": My account and My data rights;
  - the menu, Home then My data rights (D55), with no header action.
- **Correct:**
  - **H3 is Mahlangu's Home** (top bar "Nomsa Mahlangu"), but its Q7 line names molefe@kgk.wombat.local. Use
    mahlangu@kgk.wombat.local.
  - **"Then you can log activities…"** holds only once the programme has started (D55). Say "once your programme
    has started", or draw the future-start wording.
  - **"Invited by" needs DATA.** The issuer is stored (`D/Invitations/Invitation.cs:33`, `IssuedByUserId`), but no
    user links to her invitation: accepting sets `UsedOn` only (`A/Invitations/AcceptInvitation.cs:96-97`). So the
    read is the used invitation for her address.
    - An SSO-provisioned pending registrar has no invitation. Drop the line for her, and drop "ask Nolwazi Mbatha".
    - An Administrator-issued invitation would name "Demo Administrator". Say "a Wombat administrator" instead.
  - **"Programme: Paediatrics / Paediatrics"** can come from her own scopes. They are stored at registration
    (`src/Wombat.Infrastructure/Identity/InvitedUserProvisioner.cs:104-116`), so this needs no invitation read.
    "…on the Paediatric EPA Curriculum (11.1)" is true only while exactly one version is adopted, since admission
    chooses. Name it only then.

## 5. Corrections

Severity: **must** (round 2 cannot go out without it), **should**, **nit**.

### Wrong against the code or the runbook

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| C1 | A1–A5, A7, B3, B5, B8, C1–C5, C8 | Rows at 2.5, 2.6 and 2.17 read **"Sent"**. On the replay the log sender reports no delivery: every row reads "Being sent" for its first hour, then "Not delivered." (`Act2:38-42`; 2.2, 2.7 and 2.16's Actuals, `Act2:70-75`, `:141-145`, `:300-302`; BRIEF § 11 flow 10). Draw "Being sent" on step boards, and "Sent" only on a board labelled "against a mail server" (the brief's Sent state). | must |
| C2 | H3 | Mahlangu's card names molefe@kgk.wombat.local. | must |
| C3 | Z1 Edge cases | "Today both stay active, and the first used spends only itself" is wrong. Accepting one invitation revokes every other active invitation to that address, other institutions' included (`A/Invitations/AcceptInvitation.cs:99-113`, T061). | must |
| C4 | Z1 Edge cases | "A start in the future: Training year 1". `TraineeProfile.StageOn` returns null before the start (`D/Identity/TraineeProfile.cs:196-205`), and the product says "Not started" / "Programme not started" (`src/Wombat.Web/Components/Shared/Programme/ProgrammeWords.cs:86, :89`). The brief's own edge case says "training year 1" and is wrong too. Draw "Not started: the programme starts on 2027-01-15" (or the date typed). | must |
| C5 | R7 (T315) | `SameSite=Strict` (§ 4): use Lax, Data-Protection-protected. Drop the token from the refused redirect too. | must |
| C6 | R4, R5, R7, A3/B3/C3 | Built words reworded and not marked (§ 4), and "Invitation revoked." became two sentences. Mark each NEW, and give its Expect in Z2. | should |
| C7 | R1–R4 | `novalidate` on the static form (§ 4). | should |
| C8 | A1–C8 | The issued times are 09:22–09:36, 10:05–10:16 and 11:20. The replay's are 09:19 for the staff, 09:22 for the registrars and 10:23 for the resend (`Act2:70-75`, `:141-145`, `:300-302`, `:499-505`). Z1 admits it. Use the replay's times. Same-minute rows order by issue, newest first. | should |
| C9 | A6, B6, C6 | The College picker lists Demo College before the CPSA. The Actual reads CPSA, Demo College (`act-1-setup.md:140-146`). | nit |
| C10 | B1–B7 (and A/C's role help) | Two role descriptions are wrong. "Assessor: assesses registrars' work in one sub-speciality": an assessor is nominated by institution, not sub-speciality (`src/Wombat.Infrastructure/Activities/NomineeDirectory.cs:44`). "Speciality admin: watches one speciality's programme": the role forms panels and schedules reviews (`execution/knowledge/DOMAIN.md:95`). Suggest "Assesses the registrars who ask them, at this institution." and "Runs one speciality's committee work: panels, reviews and decisions due." | should |
| C11 | Z1 Components | Icons: `mail`, `user-plus` and `graduation-cap` are not NEW (`src/Wombat.Web/wwwroot/icons/`). "refresh" is `refresh-cw`. `send` and `hourglass` are not in the set, so mark them NEW. | nit |
| C12 | W2b, W2d | "ConfirmDialog, form mode" is the action mode with `FieldContent` (§ 3). | nit |
| C13 | S5, S6 | "Could not load Invitations." The house form is "Could not load the invitations. Nothing has changed. Try again, or come back in a few minutes." (`W/Activities/ActivityInbox.razor:157`). | nit |
| C14 | A5, A8, B5, B8 | Inside the not-delivered group the order is 10:09, 10:07, 10:14, against the caption's "newest first". | nit |

### Invented people or values

- **No invented person** (Obs): every name, address, role, scope and institution is from § 1 or § 8. The heavy set's
  extra rows are written ‹a further registrar's address›, which is right.
- **The heavy set's ndlvou@ "failed 2 times"** is a composite: in the story that row is revoked at 2.17 and never
  failed twice. It is acceptable for a volume board, but label it as such (nit).
- **"Any speciality"**, for a Coordinator or Committee member with no speciality (A1, Z2 2.2, 2.5), is new wording.
  Today the cells are blank. Mark it NEW (nit).

### DATA claims (the build's new reads)

Each is a mapper or query addition. None moves a domain rule.

- **Register:** the invitation's institution or College, speciality and sub-speciality names, and `ExpiresOn`, on
  `InvitationPreviewDto`. `ExpiresOn` is not yet marked on Z1.
- **The pending Home:**
  - the used invitation's issuer name, role and issue date, found by address and `UsedOn`, and absent for SSO;
  - her scope names, from `UserSpecialityScopes`;
  - the adopted curriculum, named only when exactly one version is adopted.
- **Trainees, Waiting:**
  - "Invited as": names for `PendingTraineeDto.SpecialityIds` and `SubSpecialityIds` (`A/Trainees/TraineeProfileDto.cs:29-36`), which the page already reads lists for;
  - "Registered": the user has no creation date (`src/Wombat.Infrastructure/Identity/WombatIdentityUser.cs`), so it
    is the used invitation's `UsedOn`, or drop the column.
- **Trainees, Current:** `TrainingYear`. Programme start is already on `TraineeProfileDto` (`:16`) and is not DATA.
- **Admission:**
  - `TrainingYear` for a chosen start, on the date's change;
  - the derived completion for an empty field;
  - the registrar's invited scope and registration date.
- **Invitations at issue:** whether the address has an account (T286) and whether it has an active invitation (Q10),
  each scoped as C15 and C16 say.
- **Copy link:** a clipboard call in `wombat.js`, which today has none (`src/Wombat.Web/wwwroot/wombat.js`). Mark
  it NEW. The old `clearInvitationTokenFromUrl` (`:3`) goes once the redirect lands.

### PROPOSED items

Round 1 marks every route, dialog and rule change PROPOSED, and all are right to be:
- `/admin/invitations/new`;
- `/admin/trainees/admit`;
- the revoke confirmation;
- T286's refusal and Q10's;
- "Invite another address as …";
- Not delivered rows first;
- the token cookie;
- the refused links' h1s;
- Q7, Q8, Q9 and Q11.

Add three:
- Home's "Invite a person" retargeted to `/admin/invitations/new` (`HomeFrame.cs:51`), which Z2's counts assume;
- the role descriptions (NEW words);
- DESIGN.md § The invitations list. Delivery becomes a state word with an icon, with the explanation in a
  warning-tinted note row. DESIGN.md:2145-2157 says Delivery "is the text, not a badge … nothing on the row is a
  verdict to tint", and round 2 must write the text that replaces it (should).

### Access control and anonymity

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| C15 | S3, Z1 (T286) | The refusal names the account holder and points to Users: "smit@… already has a Wombat account (Pieter Smit) … open Pieter Smit under Users." When the account is another institution's (or a College admin's, or the Administrator's), this tells an Institutional admin who holds the address. That breaks the scope rule (out of scope reads as not found, CLAUDE.md § T056). Name and link only when the account is in the caller's scope. Otherwise use fixed words with no name (Q12). | must |
| C16 | S3, Z1 (Q10) | "… already has an active invitation, issued … expiring …. Resend it or revoke it in the list." When that invitation is another institution's, it is not in her list (`A/Invitations/ListActiveInvitations.cs:39-47`). The refusal then leaks its dates and asks for the impossible. Scope it (Q10). | must |
| C17 | A1–A8, B3–B8, C1–C8 | The caption "At Kgosi Kgari Teaching Hospital." cannot stand for the Administrator, who sees every institution's invitations. Draw the Administrator's list (1.7's two rows), with the institution or College in "Role and scope", as Z2's 1.5 and 1.7 say. | should |
| C18 | W1a–W3c | The Institution column is gone (today `T/PendingTraineesList.razor:41`). The Administrator is admitted to Trainees and sees every institution's pending registrars. Keep the column, or the institution in "Invited as", for her. | should |
| C19 | H1–H3 | "Invited by" for an Administrator-issued or SSO-provisioned registrar (§ 4). | should |

The revoke dialog, the OneTimeLink and Q11's card show nothing the viewer could not already see (Obs).

### Accessibility and the 390 boards

- **Good (Obs):**
  - one h1 per page and h2s in order (Invite a person, then Active invitations; Awaiting admission with h3s);
  - each table captioned, with a caption that does not repeat the heading;
  - row headers `scope="row"`, and `data-label` on stacked cells;
  - every row action named for its row;
  - Resend described by its note;
  - the dialog labelled and described, opening on "Keep the invitation" (flow 09's rule);
  - the Copy link announced through `aria-live` ("Link copied.");
  - 44 px targets at 390 (`f11.css`: the link row, the actions, the radios, the wait card's links);
  - refusals tied by `aria-describedby`, with the field's stripe as well as colour.
- **C20 (should):** C's `order:-1` below 1100 px (§ 2). Moot if B is picked.
- **C21 (nit):**
  - the "N active" count is a `badge-draft` inside the h2, which reads as a state (flow 10 kept "Suppressed" off
    badges for this reason). Use plain text.
  - B1's link help says "press Resend on its row", but on B that row is on the list page. Say "Resend it from
    Invitations".
- **C22 (should):** round 2 must show A5/B5 at 1280 with the not-delivered rows' two buttons and no sideways scroll
  (§ 2's risk). It must show 768 for the list, as flow 06 and § 11 ask.

### DESIGN.md and flows 01–10's patterns

The DESIGN.md changes are named on Z1:
- § The invitations list;
- § Form system (fields shown per role, not disabled);
- § Button system (Revoke outline, with confirmation);
- § Table system (the note row);
- § Page-level patterns: Form page;
- § Account / auth page (the lockup).

Nothing breaks the CSS contract. Every new class is in `f11.css`, colours are tokens, and the boards have no
Bootstrap or inline `<style>` (the one inline `style` per board draws the focus ring).

## 6. The operator's decisions still needed

Q1–Q6 are the canvas's. Round 1 answered them, and my recommendation follows each. Q7–Q14 are the operator's.
Q15–Q18 are new.

1. **Q1. Where issuing lives.** Drawn: A (above the list), B (a page of its own), C (beside, as rows).
   *Recommend:* **B**, with Home's "Invite a person" retargeted to `/admin/invitations/new` (PROPOSED). Borrow A's list
   page and an "Open Invitations" link in the result.
2. **Q2. The one-time link.** Drawn: the OneTimeLink (NEW). It is the issue's or resend's result, with the link in a
   read-only field, Copy link, "Link copied." announced, and the expiry. It clears on any next action or navigation.
   *Recommend:* as drawn. Copy is a NEW `wombat.js` clipboard call.
3. **Q3. The issue form.** Drawn:
   - no role chosen;
   - only the fields a role takes;
   - the only institution as text;
   - Sub-speciality "Choose a speciality first" (`aria-disabled`);
   - refusals under their field;
   - role descriptions as B's radios.

   *Recommend:* as drawn, with C10's two descriptions corrected.
4. **Q4. The wait.** Drawn: H1, which names the institution, the programme, who invited, who admits, what admission
   does and what she can do meanwhile. *Recommend:* as drawn, with § 4's corrections. "Invited by" appears only when
   an Institutional admin issued it, and "Then you can log activities" follows the start.
5. **Q5. Admission.** Drawn: W1, W2, W3. *Recommend:* **W1**. It is a page of its own at `/admin/trainees/admit?userId=…`
   (PROPOSED). Admit returns to Trainees with "Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1),
   training year 4, from 2023-01-15." focused. There are two lists, and Current trainees gains Programme start and
   Training year.
6. **Q6. Register's refusals and frame.** Drawn: R5–R7 and the lockup. *Recommend:* as drawn, with § 4's
   corrections: Lax cookie, no `novalidate`, the reworded refusals marked.
7. **Q7 (the operator's). Tell the registrar she is admitted.** Drawn: one PROPOSED line on H ("An email tells you …").
   *Recommend:* yes. Send one mail on admission through T367's `AccountNotices`
   (`src/Wombat.Application/Common/Email/AccountNotices.cs:52`). It names the curriculum, her training year (or "starts
   on …"), and that she signs in again, with a Sign in link. No feedback or data goes in it. The admission's result
   does not claim delivery.
8. **Q8 (the operator's). Tell the Institutional admin someone is waiting.** Drawn: nothing, only Z1's mention.
   *Recommend:* a "Waiting for admission: 5" line on her Home linking to Trainees, PROPOSED here for flow 12 to place.
   No mail.
9. **Q9 (the operator's). Expired, unused invitations.** Drawn: nothing. Z1 proposes 7 days and leans to keeping
   today's silent drop, with Q8's line. *Recommend:* the brief's version. List them for 30 days under "Expired, not
   used", with "Invite again" opening Invite a person prefilled. That is the same mechanism as B's "Invite another
   address as …", so it needs no new command. Otherwise a registrar who never registered is lost.
10. **Q10 (the operator's). A second active invitation to one address.** Drawn: S3's refusal (PROPOSED).
    *Recommend:* refuse it at issue, in any institution, since acceptance sweeps the others anyway
    (`AcceptInvitation.cs:99-113`).
    - **Within her own institution:** S3's words.
    - **Another institution's invitation:** "This address already has an active invitation from another institution.
      Ask a Wombat administrator." No dates (C16).
11. **Q11 (the operator's). A signed-in visitor on Register.** Drawn: R7's "Signed in as someone else", with "Sign out
    and continue" (PROPOSED). *Recommend:* yes, served static. It never registers over a live session.
12. **Q12 (the operator's). An address that already has an account (T286).** Drawn: S3's refusal beside Email, and
    R7's "Account already exists" with Sign in. *Recommend:* refuse it at issue, before anything is stored or mailed.
    - **The account in her scope:** S3's words and the link to the user.
    - **Otherwise:** "An account with this address already exists. Ask a Wombat administrator to add the role." No
      name (C15).

    Keep Register's refusal with Sign in.
13. **Q13 (the operator's). Role labels.** Drawn: labels on every board. *Recommend:* yes, everywhere on flow 11's pages
    (`WombatRoleLabels`). Expects 1.5, 1.7, 1.8, 1.10, 2.1, 2.6, 2.8, 2.10 and 2.18 change. The users pages stay
    flow 12's.
14. **Q14 (the operator's). What the build folds in.** *Recommend:* fold in the brief's list:
    - T264's invitations half;
    - T315;
    - T317's registration half;
    - T286's issue half;
    - T306's admission half;
    - the T323, T324, T280, T325, T328 and T329 halves for these pages.

    Add to it:
    - T341's register half, because the T315 cookie is built anyway and can carry the codes;
    - Q7's mail, on T367's path;
    - § 5's DATA reads.

    None needs to land first. T333, T289, T291 and the other halves stay their own.
15. **Q15 (new). Delivery on the list.** Should it be a state word with an icon, with the explanation in a tinted note
    row and not-delivered rows first? This changes DESIGN.md:2145-2157's "the text, not a badge … nothing to tint".
    *Recommend:* yes. "Not delivered" asks for an action, and the words stay the state. Round 2 writes the DESIGN text.
16. **Q16 (new). A second invitation from another institution (Q10) and an account outside her scope (Q12).** How much
    may the refusal say? *Recommend:* that the address is taken and whom to ask, never a name, a date or a link (C15,
    C16). Any refusal reveals that the address is known. That is accepted, as Register's "already exists" already does
    for anyone holding a link.
17. **Q17 (new). The admission page's h1.** Drawn: "Admit Lerato Molefe". The brief suggested "Admit a registrar".
    *Recommend:* as drawn, with tab "Admit Lerato Molefe · Wombat" and Trainees lit.
18. **Q18 (new). An unscoped Coordinator's or Committee member's scope cell.** Drawn: "Any speciality" (new; blank
    today). *Recommend:* as drawn, marked NEW. The alternative is a dash with "No speciality" as its accessible name.

## 7. What round 2's ask must require

- **R1. Spec and Steps boards, as the ask's round 2 says.**
  - **The Spec board:**
    - every NEW and changed component with its classes: OneTimeLink, the note row, the radios with descriptions, the
      Admit page, the wait card;
    - the DESIGN.md text each changes, with § The invitations list rewritten (Q15) and § Account / auth page's
      lockup;
    - the tokens;
    - every DATA read in § 5;
    - every message by kind (result, refusal, hint, notice, empty), with the built words and the NEW ones marked.
  - **The Steps board:** each of the 29 Expects with its new words, and each journey's count at 1280 and 390.
- **R2. The picks at fidelity: B and W1**, with A's list page and the "Open Invitations" link, and W1's result above
  the next Admit….
- **R3. The states not yet drawn** (§ 1):
  - **Invitations:**
    - 2.1 empty;
    - 2.7's nine and 2.16's five "Being sent";
    - 2.26 before Resend;
    - the Administrator's list with rows;
    - "Sent" against a mail server;
    - the T290 note on both Invitations pages;
    - Invite a person's loading and load error;
    - 768.
  - **Register:**
    - details invalid;
    - every refused link at 390;
    - the token cookie expired ("Try again" after 30 minutes).
  - **The pending Home:**
    - an SSO-provisioned registrar;
    - an Administrator-issued invitation.
  - **Trainees and Admit:**
    - nothing adopted;
    - unknown or not-pending `userId` ("not found", never the exception's text);
    - loading and a load error with Try again;
    - the Administrator's view;
    - a future start ("Not started", C4);
    - a start more than four years back (training year 5, with a warning);
    - two adopted versions;
    - the T290 note;
    - 390 and 768.
- **R4. The corrections:**
  - C1–C5 and C15–C16 (must);
  - C6–C8, C10, C17–C19 and C22 (should);
  - the nits as one pass.
- **R5. Real values for J:**
  - J = 2026-01-15;
  - starts 2023-01-15, 2024-01-15, 2025-01-15 and 2026-01-15;
  - completions 2027-01-14 to 2030-01-14;
  - Ndlovu's derived 2027-01-15.

  Use the replay's issue times (C8). The wireframes' symbolic dates go.
- **R6. Q7–Q18 drawn as the operator decides.** Nothing of flows 01–10 is drawn as changed, except Home's "Invite a
  person" target (Q1) and, if taken, Q8's line, marked for flow 12.
- **R7. Every person, address and value from § 1 and § 8.** Re-attach the eleven key screenshots and name the folder
  (`design/baseline/`, § 4's list), and ask round 2 to say if any did not arrive.

## 8. The operator's decisions (2026-10-19)

The operator took every recommendation in § 6 as recommended. Each in one line, with what it means for round 2 and the
build:

- **Q1:** B. Invite a person at `/admin/invitations/new` (PROPOSED route), Home's "Invite a person" retargeted to it
  (`HomeFrame.cs:51`); A's list page and an "Open Invitations" link in the result. A and C are dropped.
- **Q2:** the OneTimeLink as drawn; the build adds a clipboard call to `wombat.js` and clears the link on any next
  action.
- **Q3:** the issue form as drawn: no role chosen, only the role's fields, the only institution as text, radios with
  descriptions (C10's two corrected), refusals under their field.
- **Q4:** the wait card as drawn, with § 4's corrections: "Invited by" only for an Institutional admin's invitation,
  "log activities" once the programme has started; the card's DATA reads join `GetTraineeDashboardSummary`.
- **Q5:** W1. Admission at `/admin/trainees/admit?userId=…` (PROPOSED route), returning to Trainees with "Lerato Molefe
  admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15." focused; Current trainees gains
  Programme start and Training year. W2 and W3 are dropped.
- **Q6:** Register as drawn, with a Lax, Data-Protection-protected cookie, no `novalidate`, and each reworded refusal
  marked NEW with its Expect.
- **Q7:** a mail on admission through T367's `AccountNotices`, naming the curriculum and the training year (or the
  start), with a Sign in link; the result claims no delivery. Folded into the build.
- **Q8:** a "Waiting for admission: 5" line on the Institutional admin's Home, PROPOSED here for flow 12 to place; no mail.
  Round 2 draws it once, as a note.
- **Q9:** expired, unused invitations listed for 30 days under "Expired, not used", with "Invite again" prefilling Invite
  a person; the build adds the read.
- **Q10:** a second active invitation to one address refused at issue, in any institution; dates only in the caller's
  scope, otherwise "… from another institution. Ask a Wombat administrator."
- **Q11:** Register's "Signed in as someone else" card, served static, also for a signed-in visitor (build note).
- **Q12:** an address with an account refused at issue: in scope with the name and the link to the user, otherwise
  "An account with this address already exists. Ask a Wombat administrator to add the role."; Register keeps its
  refusal with Sign in.
- **Q13:** role labels everywhere on flow 11's pages; Expects 1.5, 1.7, 1.8, 1.10, 2.1, 2.6, 2.8, 2.10 and 2.18 change.
- **Q14:** the build folds in the brief's list, T341's register half, Q7's mail and § 5's DATA reads; nothing lands
  first; T333, T289, T291 and the other halves stay their own.
- **Q15:** Delivery a state word with an icon, its explanation in a tinted note row, rows not delivered first; round 2
  writes the DESIGN.md § The invitations list text that replaces :2145-2157.
- **Q16:** an out-of-scope refusal says the address is taken and whom to ask: no name, date or link.
- **Q17:** the h1 "Admit Lerato Molefe", its tab, the trail Home › Trainees › Admit Lerato Molefe, Trainees lit.
- **Q18:** "Any speciality" for an unscoped Coordinator or Committee member, marked NEW.

Round 2's ask is `round-2-ask.txt`: the decisions as requirements, C1–C22 each with the board it fixes (C12 and C20
moot with W2 and C dropped), § 7's states, and the Spec and Steps boards. Settled in writing it, PROPOSED there for the
operator to overrule:
- Q4's "Invited by only for an Institutional admin" wins over § 4's "say 'a Wombat administrator'": the line is
  dropped for an Administrator's invitation.
- Q10's out-of-scope words name "another institution"; an address holding a College's invitation reads "… from a
  College. Ask a Wombat administrator."
- Under B the list is another page, so Q10's and Q12's in-scope words say "under Invitations" and "under Users" as links
  (C21's reasoning).
- Q12's "add the role" cannot hold for a Trainee invitation: Add role never offers Trainee (T303) and Trainees admits only
  a Pending trainee, so an existing account cannot be invited as a trainee at all. The ask words that refusal and leaves
  the gap (a registrar who already holds an account) to the operator.
- An expired row leaves "Expired, not used" once its address is invited again; its 30 days count from its expiry on the
  South African calendar.
- The Waiting list keeps Registered, read from the used invitation's `UsedOn`, "—" without one.
- The Admit page's not-found words and the training-year-5 warning are NEW words.
