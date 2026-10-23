# Flow 12 round 2: the review

Round 2 (the boards saved in `round-2/project/`: 164 boards, 78 at 1280, 78 at 390 and 8 at 768, plus `R2-0-spec`,
`R2-0-spec-2`, `R2-0-steps` and `f12r2.css`) drew S1, H1, UA, I and O at fidelity: her Home (H), Users (U), the user
page of sections (P), Assessor training and Assessors (T), her institution's record (I) and SSO mappings (O), with the
heavy set, the Administrator's views and every loading and load-error state. Reviewed 2026-10-23 from three sides, each a
Sonnet agent reading the boards' text: **the code** (`round-2-review-code.md`, K1–K30: `src/` and the ask), **the cast**
(`round-2-review-cast.md`, C1–C15: the runbook, the fixtures and the captures) and **the design system**
(`round-2-review-ds.md`, D1–D20: `bundle.css` v29, DESIGN.md, `app.css`, accessibility, measured in Chromium on a scratch
copy). The three source files are kept as they are; this file merges them. Every must-fix below was checked against the
board and the code before it was carried; what did not hold is said where it was narrowed or dropped. Severity: M
must-fix, S should-fix, N nit. Code is cited at `6b4d39dc`.

**The captures arrived, by the upload folder, not as attachments.** The Spec says the message reached the session with no
files; the ten key captures were read from `C:\dev\Wombat\design\upload` before drawing, with four more (14 read), and
seven more were copied but not opened (`2.13-1-zulu-before`, `1.23-1-kgk-own-record`, `5.17-3-mbatha-user-no-roles`,
`A.7.9-1-home-390`, `2.12-3-users-filter-zulu`, `states/users-list--loading`, `A.6.7-1-patel-reactivated`); the 09-26
`states/` captures were not used. They show what the brief did not say: training status defaults to Not started, the
assessor form draws its scope as "All", Remove and Lock out user are filled danger buttons, Add role and Reset password
draw disabled until used, and the own-account note is an info alert. The five boards that lean on the unopened seven
(P02, I01, P12, U02, P17) carry the runbook's Actual words, which the cast found true (C8). The cast found every person,
address, role, scope, date, count and order true, Fatma until 2.41 and Fatima from 2.44. What slipped is **behaviour the
boards assume and the code does not do** (K1–K4), **one Expect whose dates are paired with the wrong names** (C1), and
**system classes and wiring the boards invent or break** (D1–D5).

## Decisions for the operator

The operator's Q1–Q22 (`round-1-assessment.md` § 10) stand as given; nothing below reopens them. F1–F3, F5 and F7 are the
build choices Q4, Q10, Q11, Q12 and Q16 need; the rest are new.

| | Question | Recommendation | Alternatives |
|---|---|---|---|
| F1 | **May Add role give a role that may not be invited with a sub-speciality to a person who holds one?** Q4 says Add role works "by Invite a person's rules"; that is `InvitationRules.CheckScope`, which refuses a sub-speciality on Speciality admin, Coordinator and Committee member (`InvitationRules.cs:81–82`, checked). So once Zulu holds Assessor (Paediatrics / Paediatrics, P05), she can never be given Committee member, Coordinator or Speciality admin, while the reverse order (2.13's) works. The boards draw only the working order (K2 (c)). | **Yes.** Judge the **role's own need** against the resulting scope (Assessor and Sub-speciality admin: a speciality and a sub-speciality; Speciality admin: a speciality); "may not be scoped to a sub-speciality" is an invitation rule and is not applied to a scope the person already holds. A handler test for the Assessor-first order. | Today's rule: refuse, the board drawing the refusal under Role ("The selected role may not be scoped to a sub-speciality.") and the Spec saying the order matters. |
| F2 | **A second role that narrows the first.** One scope serves every role (Q4), so adding Speciality admin (Paediatrics) to Pieter Smit, an unrestricted Coordinator, narrows his Coordinator to Paediatrics. P08's result shows the new scope but nothing says it is a narrowing, and P06's "Thandi Zulu's scope stays" hides the reverse (K2 (d)). | **Allow it and say so before the save**: under the scope in Add role, "Pieter Smit's scope becomes Paediatrics, for every role he holds." (NEW), and the result repeats it; a board for Smit. | Refuse a role whose scope would narrow a held role ("Remove Coordinator first, or …"); a per-role scope (Q4 rejected it: its own task). |
| F3 | **The last way in when the account has only an institutional sign-in** (K3, checked). P22 tells the refused administrator "Reset a password first to give the account another way in." A reset sets a hash but never `AllowLocalPassword` (`UserAdministrationService.cs:278–283`, `:470–481`), and the sign-in endpoints refuse an account with it false, so the reset succeeds, the guard still fires, and the password cannot sign in. P21–P22 also draw the reset form for that person. | **An SSO-provisioned account holds no password.** For `AllowLocalPassword` false the Password part reads "‹name› signs in only through ‹provider›. A password cannot be set for them." (NEW), no reset field; the Remove refusal is the built "The institutional sign-in is the account's only way in." with no hint. The page reads `AllowLocalPassword` and `HasPassword` (`GetSignInMethodsAsync` returns the second). | A reset enables a password (sets `AllowLocalPassword`): a product decision and a DOMAIN change, then the hint is true. |
| F4 | **The assessor profile's dead columns** (K1). Under Q12 the profile records training only, so its `SpecialityId` and `SubSpecialityId` are written by nothing and read by nothing; `InstitutionId` is required and read by erasure and the list. | **Drop `SpecialityId` and `SubSpecialityId` now** (one migration; nothing is live); keep `InstitutionId`, written from the person's institution at save. | Leave the columns until T289's open "which record owns the discipline" note is closed. |
| F5 | **What lands before the build** (K4, narrowed). At `6b4d39dc` T288, T333, T289, T264, T326 and T286 are all `queued/` (checked); the ask told the boards to draw T288 and T333 "as built", and nothing on the Spec or Steps says which board needs which task. K4 lists all six as prerequisites, but Q16 decided otherwise for three: T264's users and SSO half, T326's assessors and SSO items and T286's external-logins half **fold into this build**. | **Q16 as decided.** T288, T333 and T289 land first, as their own tasks; the Spec gains a Prerequisites block, and every O board and the training section say "after T288 and T333" / "after T289"; T264, T326 and T286's halves are this build's and drawn as fixed. | Fold T288 and T333 into this build (one task, one replay; larger review). |
| F6 | **The waiting-request line has no read** (K5). "Waiting for X" exists only for the caller (`ActivityWaiting.LoadActionableAsync` over a `ClaimsPrincipal`); the one reader for a third person is the nudge job in Infrastructure. The line also carries trainee, type, PAED number, waiting-since and overdue, and is drawn for one request only. | **A new `GetRequestsWaitingForUserQuery(userId, principal)`**, read when the lock or Remove dialog opens (never on every page view): the person scope-checked, the `AwaitsReviewer` predicate and nominee extraction moved to an Application service the nudge job shares; a count, up to five rows and the oldest waiting-since. Boards for N = 2 and N > 5 ("and 3 more"). | The count alone, one sentence, no rows ("2 requests waiting for ‹name› stay with them; …"). |
| F7 | **Removing another person's institutional sign-in** (K8). Q10 put Remove… beside each sign-in, but the only command is the person's own (`RemoveMyInstitutionalSignIn`), and its form-post mode exists to re-issue the person's own cookie. | **A new `RemoveUserInstitutionalSignInCommand(userId, provider, principal)`** through `DemandUserAdministration`, `DemandNotCaller` and `DemandInScope`, the last-way-in guard reused; an action-mode ConfirmDialog. Mbatha's own sign-in stays flow 02's My account (K28). | List the sign-ins without Remove… and leave the removal to T286 later. |
| F8 | **The Delete dialog's "anyone who had Assessor only from this group loses it"** (K9). No code writes a Manual `UserRoleAssignment` (T377), so a role given by hand **and** by the group is lost too; and the mapper never removes the scope a mapping added. Q20 put T377 outside this flow. | **Reword to what the code keeps**: "From each person's next sign-in, belonging to ‹group› no longer gives Assessor." plus "The scope it gave stays." (NEW); T377 stays its own. | Land T377 first (Add role writes the Manual row), then the sentence holds. |
| F9 | **The pager as built** (K13 (a), (b)). `PagerControls` draws for any `TotalCount > 0` (a list of ten gets "Showing 1–10 of 10" and the select) and has no "Page n of N"; U10 draws one. | **Use it as built**: guard it here with `total > 10`, so the small lists draw none (as the boards do), and take "Page 1 of ‹10›" off U10. No change to the shared component, so flows 06 and 08 are untouched. | Add "Page n of N" to `PagerControls` for every list (a change to flows 06 and 08, which the ask forbade drawing). |
| F10 | **Entrustment decisions loses its only link** (K16). Q7 dropped Quick links because "the menu repeats them"; three are in her menu, but `/admin/entrustment-decisions` is not (`ByRole[InstitutionalAdmin]` has no item), and it admits her. | **Add it to her menu** under Reviews (an owner row; `ActiveNavItemTests` checks it). | Keep one link on Home, in the Curriculum card. |
| F11 | **The disclosure's leaving word and its frame** (D4, D19). Add a role… and Record training open in place (Q21); the system's note panel (`.note-panel`, DESIGN.md:3596–3640) already is that pattern, with a quiet "Keep …"; the boards use an outline "Cancel" and a new `.disclosure-panel`. Flow 11 settled "Cancel" for leaving a form (its D30). | **The note panel's pattern with "Cancel"**: `.detail-card.note-panel` (a modifier for a form's gap only), Cancel as the panel's quiet button, DESIGN.md saying a form in a note panel leaves with Cancel and a note with Keep. | A new `.disclosure-panel` with an outline Cancel, as drawn, written into DESIGN.md as a second pattern. |

Settled from the evidence (no decision needed):

- **The training save writes no scope** (K1, M1): Q12 already decided it; what was open was only that the command still
  writes one.
- **The record's name while loading** (K15, S9): a cheap read, not a claim; an Administrator renames institutions (flow
  18), and a claim would stay stale until the next sign-in.
- **No owner row for `/admin/institutions/{Id}`** (K19 over D12 (f)): with no owner for the acting role the trail is
  already "Home › title" (`NavOwners.cs:252`, `TrailTo`), and a row naming Home would fail `ActiveNavItemTests`. D12 (f)
  is dropped.
- **KGK's contact email at each step** (C9): `paeds-admin@kgk.wombat.local` from Act 1 (`act-1-setup.md:184`) through
  1.23 and 2.44; `paediatrics@kgk.wombat.local` after A.6.1 (`appendix-cross-cutting.md:979`), so A.6.3's form opens on
  it; `hod.paediatrics@kgk.wombat.local` after A.6.3 (`:1012`). Each board shows the one in force at its step and none is
  wrong; round 2's ask's single "paeds-admin@ until A.6.3" was too coarse, and the A.6.3 row must say the field starts at
  `paediatrics@` (S15).
- **What holds** (all three reviews): every person, address, role label, scope as invited and after 2.13, institution and
  College; Fatma/Fatima by step; the ten, fifteen and fourteen counts and their orders, Dlamini before du Plessis and
  Mokoena before Molefe; 2.14's five records (2018-10-20, 2020-10-20, 2026-09-20, two with no date); du Plessis's
  2026-10-18 and Molefe's 2026-10-20; Patel's one waiting request at A.6.4 (PAED-015, 2026-10-12 12:35 SAST, overdue); the
  15 steps and their journey counts; the built words quoted as built (the own-account and Trainee notes, the reset help
  and refusal, the six role descriptions, the lockout lines, "User unavailable", the SSO Administrator card); no other
  institution's row on any of Mbatha's boards; no password drawn; no raw colour, no new token; one h1, no skipped level,
  no duplicate id; every table captioned and **none scrolling sideways** from 1280 to 768 (flow 11's D1 not repeated);
  the dialog and header 44 px phone rules (flow 11's D3, D4 not repeated); load errors as StatePanel's Alert (flow 11's D5
  not repeated); required marks on the record form; the 390 and 768 twins carry the same words as their 1280 boards.

**Data the build must add** (from K's list, restated by query; observed unless marked):

| Board shows | Gap | Size |
|---|---|---|
| Users: the filter, the page of 20, the total, the order | `ListUsers` as one paged SQL query over `Users` joined to `UserRoles` (no N+1), name matched across first, last and both orders and the address, Role with No role (and the Administrator's two extras), Status, ordered `LastName.ToLower()`, `FirstName.ToLower()`, `Id` (K12, K13, K26) | M |
| The scope cell and who line | scope **names**, `CollegeId`/`CollegeName`, the cell rule (S1) (K6) | S |
| The no-role sentence | the latest ended trainee profile (end and how) in `GetUserById` and Users' no-role rows (K7) | S |
| The user page's Sign-in and access | institutional sign-ins (`GetSignInMethodsAsync`), `AllowLocalPassword`, `HasPassword` (F3); the training record joined; the invitations read removed (K30) | S |
| The lock and Remove dialogs' waiting line | `GetRequestsWaitingForUserQuery`, read on open (F6) | M |
| Add role's scope as text | `GetAdoptedDisciplinesForInstitutionQuery(institutionId, principal)` on `AdoptedSpecialities.IdsAt`: the **person's** institution, not the caller's (K20) | S |
| Add role's write | `AddRoleToUserCommand(UserId, Role, SpecialityId?, SubSpecialityId?, Principal)`, union, one transaction (M2, F1, F2) | M |
| Assessor training | `CreateOrUpdateAssessorProfileCommand` training-only (M1); F4's migration | M |
| Remove a sign-in | `RemoveUserInstitutionalSignInCommand` (F7) | S |
| Assessors | every Assessor holder joined to a record, by last name, with scope names (Q2); the redirect (S10) | S |
| SSO | `ListSsoGroupMappings`: `ProviderGone` from `IOptions<SsoOptions>`; Create: Institutional admin refused to a non-Administrator; Delete: a gone-provider mapping the Administrator's alone (K10, K11); the rest is T288's and T333's (F5) | S |
| Home | distinct account count, Pending trainee count, adopted specialities and sub-specialities (not the catalogue totals), the institution's name and record (K24, K15) | S |
| The record's name on loading and error | `GetMyInstitutionNameQuery` before the list read (S9) | S |
| Pending invitations | nothing: removed (K30) | none |

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | code (K1) | The training section "asks no institution, speciality or sub-speciality" and "writes no scope" (Q12), but the only save is `CreateOrUpdateAssessorProfileCommand`, which writes the profile's scope columns and then calls `UpdateScopeAsync` with the form's ids (`CreateOrUpdateAssessorProfile.cs:125`, checked); `UpdateScopeAsync` **replaces** the person's scope (`SyncScopes`). Fed from T03–T07's form, Save training on Zulu strips Paediatrics / Paediatrics, her committee counts and panels with it, and the changed stamp signs her out | Split training from scope: the command becomes `(UserId, TrainingStatus, TrainingCompletedOn, Qualifications, Principal)`, institution from the person, no `UpdateScopeAsync`; F4's columns. Test: saving training for a Committee member who is also an Assessor leaves the scope and the stamp alone. The Spec says the command changes and that T289 is a prerequisite for its refusals only |
| M2 | code (K2 (a), (b)) | "Add a role … adds to the person's scope" (Q4) is a write path the Spec never specifies. `AddRoleToUserCommand(UserId, Role, Principal)` has no scope (`AddRoleToUserCommand.cs:10`, checked) and `AddRoleAsync` writes the role only. Built naively: `UpdateScopeAsync` replaces the set (Zulu's Committee member speciality lost), and the role and the scope commit apart, so a fault between leaves an Assessor with no sub-speciality | The Spec writes the command: add the role, then `UpdateScopeAsync` with the **union** of the held ids and the new ones, inside `InOneTransactionAsync` (`IInvitedUserProvisioner.cs:47`); the role's need judged as F1 decides; the narrowing as F2; what Add role does for a person with no institution (U14's College admin). Handler tests: union, atomicity, the Assessor-first order |
| M3 | code (K3) | P22's "Reset a password first to give the account another way in." is false (checked: the guard counts a password only if `AllowLocalPassword`, `UserAdministrationService.cs:278–283`; a reset never sets it), and P21–P22 draw a reset form that cannot give an SSO-only account a way in | Drop the hint; draw the Password part for `AllowLocalPassword` false as F3 decides; the page reads both flags |
| M4 | code (K4, narrowed by Q16) | The O boards and the training section draw T288, T333 and T289 "as built"; all three are queued (checked), and the Spec names no prerequisite, so the boards cannot be replayed at HEAD. Today's page offers any institution, Trainee and Pending trainee, every provider anywhere, and a filled Delete with no dialog. K4's T264, T326 and T286 are not prerequisites: Q16 folds their halves into this build | A Prerequisites block on Spec-2 (F5): O-family after T288 and T333; the training section's refusals after T289; each O board and T03–T07 says so on its face. T264's, T326's and T286's halves listed as this build's |
| M5 | cast (C1) | R2-0-steps row 2.15's Expect reads "Completed 2018-10-20, 2020-10-20, Not recorded, Not recorded, 2026-09-20 by last name (Botha, Khumalo, Naidoo, Patel, Zulu)" (checked): it pairs Botha with 2018-10-20. T08 is right, so a replay written from the Expect records a false gap | Row by row: Botha Trained, Not recorded; Khumalo Provisional (awaiting sign-off), 2026-09-20; Naidoo Trained, 2020-10-20; Patel In training, Not recorded; Zulu Trained, 2018-10-20 |
| M6 | design (D1) | The password input collapses to 26 px at 390 (measured on P14-390; the Show button 293 px). `f12r2.css:120`'s `.user-page .btn{width:100%}` (checked) also matches `.password-toggle` in the field's flex row. A password cannot be typed at the width the Spec calls 44 px and stacked | Scope the full width to what stacks (`.user-page .form-actions .btn`, `.user-page .section-part > .btn`), never the bare `.btn`; the toggle keeps the system's 44 px high, auto wide. Re-measure P03, P14, P20 at 390 |
| M7 | design (D2) | Home's "Your institution" card scrolls the page 93 px at 1280 (measured on H04: the `detail-list` 420 px in a 304 px column; page 1358 against 1265). Only the email `dd` wraps, by an inline style (checked on H04); `.detail-list`'s `1fr` (`bundle.css:1898`) takes the cell's min-content | A class: `.dashboard-grid .detail-list dd{overflow-wrap:anywhere;min-width:0}` and `minmax(0,1fr)` in a card; the inline style goes (10 boards). Re-measure H01–H05 at 1280, 1100 and 390 with the 64-character address |
| M8 | design (D3) | `.field-label` exists in no stylesheet (checked: 0 in `app.css`; 18 boards use it) over `p.readonly-value` for the record's Status, the mapping's provider and Role and the training read-out: plain text, no weight, no programmatic label | FormField's read-out: `dl.form-group` with `dt` and `dd.readonly-value`; `.status-readout` becomes that and leaves the NEW list |
| M9 | design (D4), code (K22) | `.disclosure-panel` is a second class for the shipped note panel (`.note-panel`, `app.css:2314`, checked; same 4 px secondary stripe, the same toggle), and `.choice-stack` already ships (`app.css:5850`, checked; the MSF Kind radios) while `f12r2.css:61–66` redeclares it with other rules, which would reach the MSF page once moved into `app.css` | Reuse `.detail-card.note-panel` as F11 decides, its DESIGN.md text "the note panel's pattern, generalised to a section's form"; the radios `.choice-group` with `.form-check`, any column-with-help rule renamed (`.choice-help-stack`) so the shipped class is untouched |
| M10 | design (D5) | The disclosure's wiring: collapsed, the trigger's `aria-controls="ar-panel"`/`"tr-panel"` names no element (checked on P02; 63 references over 35 boards); open, the panel is `role="group" aria-labelledby="ar-legend"` around a fieldset the same legend names (checked on P03), so "Role, group" is read twice, and each radio's label wraps its help while `aria-describedby` points at it too (flow 11's D7, T375) | `aria-controls` only while the panel exists (or the panel rendered `hidden`); no `role="group"` on the panel (the fieldset is the group); the help outside the `<label>`, kept in `aria-describedby` |

## Should fix

- **S1 The scope cell as a rule** (K6, C12; U01–U14, P01–P27's who line): the same stored scope prints the institution for
  Mbatha, "Any speciality" for Smit and van Rensburg and the College for Kruger, and `UserDetailDto` carries ids only.
  The Spec writes the function: Institutional admin, the institution's name (PROPOSED); College admin, the College's;
  otherwise the speciality names comma-joined, then " / " and the sub-speciality names; none, "Any speciality". One board
  for a person with two ids (SSO adds scope as it matches, `SsoGroupMapper.cs:164–217`), or the Spec says Add role refuses
  a second speciality.
- **S2 Three no-role states** (K7, D16; U07, U09, P09–P13): the sentence assumes every person with no role is a former
  registrar, but Remove on a staff member's last role reaches it too (Zulu, no profile, no My progress). Ended profile:
  Q13's words with the date; no profile: "‹name› holds no role. They can sign in to My account and My data rights." (NEW),
  drawn; the Users cell's sentence for the first only; the no-role state written into DESIGN.md's Table system and the
  user page.
- **S3 Remove's dialog for every role** (K8; P06, P07, P20–P22): only Assessor's is drawn. One sentence each from the
  lockout list's facts: Committee member (off panels; a chair's removal stalls ratification, T256), Speciality admin (the
  programme's oversight), Sub-speciality admin, Coordinator, Institutional admin, and the last role (S2). A waiting request
  "stays with them; it does not move to anyone else", never "stays with them" alone: without Assessor the inbox is not in
  their menu (`NavItems.cs:176`).
- **S4 The SSO form** (K10; O04–O06): O06's refusal can come only from a forged post (the select never offers Institutional
  admin to her): relabel it "handler refusal (a posted role the select does not offer)", tested in
  `CreateSsoGroupMapping`; say when Speciality and Sub-speciality appear (a role that needs them chosen); Group display
  name stays required (the row's name and the result need it).
- **S5 Mappings whose provider is gone** (K11; O03): "read only" is a hidden button: `DeleteSsoGroupMapping.cs:34` checks the
  institution only. Delete refuses such a mapping unless the caller is the Administrator (a test); `ProviderGone` from
  `IOptions<SsoOptions>`; the Spec says roles already given remain.
- **S6 The Users filter as the code can do it** (K12; U01–U14, H01–H05): (a) match `first last`, `last first` and the
  address, so "thandi zulu" finds her (`UsersList.razor:86–91` tests each field); (b) offer Administrator and College admin
  in the Role select to the Administrator only (U14's rows hold them); (c) `role`, `q`, `status`, `page`, `size` in the query
  string (`SupplyParameterFromQuery`), so Home's role links have something to open, and the user page's Users crumb
  returns to the filtered page (an allow-listed return, as flow 11's sign-out).
- **S7 One order on every server** (K13 (c); U10, T11): by last name with page boundaries needs one collation:
  `LastName.ToLower()`, `FirstName.ToLower()`, `Id` in the query, pinned by a test with "van Rensburg" and "du Plessis"
  (today the database orders, then `ListUsers` re-sorts with the culture comparer).
- **S8 The Trainee notes withhold the subtitle** (K14; U13, P26): the built pages drop it on purpose (T278,
  `UsersList.razor:70–76`, `UserDetail.razor:261–265`); redraw both without.
- **S9 The institution's name on loading and error** (K15; I05, I06, U11, U12, T12, T13): the claims carry the id only. I05
  and I06 h1 "Institution", tab "Institution · Wombat" (as P24/P25's "User"); the lists' subtitles from a
  `GetMyInstitutionNameQuery` read first (settled above), or without the name until it loads.
- **S10 The `/admin/assessors/edit` redirect** (K17; T notes, Routes): `?id=` is the **profile** id: profile → `UserId` →
  `/admin/users/{id}?section=training`; an unknown, out-of-scope or missing id → "User unavailable"; an unknown `section`
  opens nothing; a person without Assessor gets the section's absence said once.
- **S11 T10's "Thandi Zulu is not listed"** (K18): the list cannot know a Remove happened elsewhere; drop the note (P's
  Remove result already says she leaves Assessors).
- **S12 The record's two routes** (K19; I01–I06, 1.23): `/admin/institutions/new` and `/{Id:int}` share one component and
  one policy, so "access denied on `/new` for her" needs `InstitutionCreate` split off (Administrator policy); the
  Administrator's variant (Active box, Deactivate, "Edit institution") is flow 18's, and A.6.1's row says so (S15). No
  owner-table row (settled above).
- **S13 Add role's adopted set is the person's institution's** (K20; P04, P27): today's lists return the caller's set, or
  the whole catalogue for the Administrator; `GetAdoptedDisciplinesForInstitutionQuery` as the DATA table.
- **S14 The Administrator's lists** (C2; U14, T14 and 768): U14 omits the five Demo accounts and `collegeadmin@`, so "every
  institution" is false, and T14's count is 6 (`assessor@wombat.local`), not 5; U14 is never a state devadmin reads. Mark
  both Illustration and bracket the missing rows (`‹Demo account›` ×5, `collegeadmin@wombat.local` Global, College admin,
  `‹Demo Institution assessor›`), or redraw U14 at A.6.9; name the step on both.
- **S15 The Steps board** (C3–C7, C9): drop 5.28 from the crossing list (Mbatha opens no admin page there) and say the no-role
  sentence first appears at A.4.5; A.6.4's Today quotes the four-bullet list with the trainee line and "no dialog (T264)",
  and its Expect says both go; A.5.15 (devadmin's "Lock out…" and dialog) joins the crossing list; 2.14's Note retired and
  2.37 and 3.52 listed, the scope arriving at 2.13 (ask's PROPOSED (c)); 2.14's Today "stays on /admin/assessors/edit (the
  `?id=` move was never built)" with both old refusals quoted; 2.28's Do gains "open Home"; 5.10 and A.6.1 in the crossing
  list, A.6.1 "unchanged: the Administrator's page is flow 18's"; A.6.3's field starts at `paediatrics@kgk.wombat.local`
  (A.6.1), I01's `paeds-admin@` being 1.23's.
- **S16 The seven unopened captures** (C8): round 3 opens them and says so on its Spec, or states that P02, I01, P12, U02 and
  P17 were drawn from the runbook's Actual lines.
- **S17 The radio's hit area** (D6; P03, P04, T06 at 390): the row is 44 px but the radio 16 and its label 24; the label
  becomes the target (`min-height:2.75rem`, `display:flex;align-items:center`), or the Spec states the exception (T375).
- **S18 Inline styles** (D7; every P and T board): 69 `actions-cell[style="justify-content:flex-start"]`, plus
  `margin-right:auto`, `overflow-wrap:anywhere`, `margin:0`, `max-width:14rem`: `.user-section .actions-cell`,
  `.dashboard-card-footer .field-help`, M7's wrap, and the rest deleted (flow 11's D17 again).
- **S19 Hand-built system pieces** (D8, D11; P03–P06, P15, H01–H05): `.waiting-request` is a hand-written warning alert:
  `Alert` (warning, `Role=""`) in the dialog; `.who-line` as `dl.detail-list`; `.card-link` and `.count-rows` as
  DashboardCard's footer link and `.dashboard-metric-row`, or the Spec says why not; the count inside the link's name.
- **S20 Stacked tables' roles** (D9; 43 boards): `role="table"`, `rowgroup`, `row`, `columnheader`, `cell` as DataTable's
  Stack emits (flow 11's D24 not fixed); a line in the build note.
- **S21 The filter card** (D10, K22 (b); U01–U14): there is no FilterBar component (flow 06 inlines `.search-grid`), so
  `form.detail-card.people-filter` is NEW: list it once, with `role="search"` and its name decided for flows 06 and 12
  alike; the heading Show focuses keeps `tabindex="-1"` and gains a hidden live count.
- **S22 The DESIGN.md text** (D12 (a)–(e), D14, K29): cite the note panel (M9); the one-scope rule's exemptions (Home's
  People card, flow 11's forms); the read-out as `dd.readonly-value` (M8); the destructive-trigger rule as DESIGN.md:734
  restated, superseding :3442's filled Delete; the waiting line with flow 06's oversight line as precedent; the single-field
  refusal beside its field with no RefusalSummary as the stated exception, and the record page's `ValidationSummary`
  removed and the first invalid field focused (`InstitutionEdit.razor:42–43`).
- **S23 Standing notices are not live regions** (D13; H04, O03, P01, P26): the waiting line and the notes are on every
  visit: `Role=""`; `status` only for what arrives with a result.
- **S24 The NEW table** (D15, K22 (a)): a row for every class (NEW and why, or reuses `.x`): add `.person-address`,
  `.list-head`, `.status-locked`, `.no-role-cell`, `.role-cell`, `.card-link`, `.action-reason`, `.inst-status`,
  `.who-line > div`; `.choice-stack` out (M9); the canvas-only `.captures`, `.doc`, `.old` and the redundant phone
  `.form-control` rule deleted.

## Nits

- Add role's help repeats flow 11's "Choose a speciality only to keep them to it." and "Choose the speciality, then the
  sub-speciality." under options that ask no speciality: a `RoleHelp` variant for an already-scoped person (K21).
- The lockout list's "They sit on no committee panel." and "They cannot be named as an activity's assessor." only for a
  Committee member and an Assessor, as the trainee line is for a Trainee (K23).
- Home's summary: the adopted counts and the distinct account count are new reads (in the DATA table); one board with a
  locked account, or the read dropped (K24).
- "Locked out" is also Identity's self-lifting brute-force lockout: say so on the Spec, or tell an administrative lock
  (`UserDeactivation.IsDeactivated`) from it in the badge (K25).
- `ListAllUsersAsync`'s N+1 `GetRolesAsync` goes with S6's paged query (K26).
- Training's small text: "Not recorded" before a record against a saved "Not started" (drop Not started from the new form,
  or define the cell); the no-date result "Training saved for Sarah Botha: Trained, no date." (K27).
- Three undrawn cases: Mbatha's own institutional sign-in (flow 02's My account), the record's result when the contact email
  is cleared, and "An institution with the same name or short code already exists." on its field (K28).
- The Spec lists what Q9 deletes: the card, `RevokePendingInvitationsForEmailCommand`, `UserDetailDto.PendingInvitations`,
  `UserPendingInvitationDto`, the read in `GetUserByIdQuery`, the CSS state and their tests (K30).
- T11's and U10's placeholder rows sit after the last cast row; place them where they would sort, or caption "order not
  shown" (C10).
- I04's malformed address `hod.paediatrics.kgk.wombat.local` reads as a typo of A.6.3's value: `‹a malformed address›`
  (C11).
- The Families row's "notes: /new and the Demo id" are on no board's face: one note board for I and T, or the word
  dropped; the Demo Institution's id is 1 (C13).
- Row 2.12's "Scope 'Paediatrics'" for all ten: "each person's one scope as invited (U01)" (C14).
- A.1.14's Expect: "(new: the No role cell and its sentence)" (C15).
- The 390 boards of I and O scroll sideways from the canvas-only `.anno` chip: `white-space:normal` (D17).
- The breadcrumb's "Home" is a 39×24 inline link at 1280: the accepted inline exception, as flows 03–11 (D18).
- "Delete mapping" names no mapping: "Delete the mapping", optional (D20).

## What round 3's ask must do

- **Restate F1–F11 as decided**, beside Q1–Q22, and draw what each decides (F1: the Assessor-first order; F2: Smit's
  narrowing line and board; F3: the SSO-only Password part; F6: N = 2 and N > 5; F7: Remove… on another person's sign-in;
  F8: the Delete words; F9: no pager on the small lists, none of "Page n of N"; F10: her menu; F11: the note panel).
- **Fix M1–M10 and S1–S24 board by board**, citing the id; redraw P21–P22 (M3), every O board and T03–T07 with their
  prerequisite (M4), the Steps board's 2.15 (M5), every P and T board at 390 (M6), H01–H05 (M7), every read-out (M8), every
  disclosure state (M9, M10).
- **Re-measure** H01–H05 at 1280, 1100 and 390 with the 64-character address, P03, P14 and P20 at 390, and every table
  again at 1280, 1100, 1000, 901, 900, 768 and 390; say on the Spec that nothing scrolls sideways and every 390 control,
  the radio's label included, is 44 px.
- **Add the missing boards**: the no-profile no-role state (S2); a Remove dialog per role (S3); a person with two scope ids,
  or the refusal (S1); N = 2 and N > 5 waiting requests (F6); Smit's narrowing (F2); the SSO-only Password part (F3);
  another person's sign-in removed (F7); the three cases of K28 or a Spec line each.
- **The Spec**: the Prerequisites block (M4, F5); the commands as written (M1, M2, F7) and the DATA table above in place of
  its own; the scope cell's rule (S1); the redirect (S10); the record's split (S12); the DESIGN.md text S22 lists; every
  class a NEW-table row (S24); which boards are Illustrations, U14 and T14 among them (S14).
- **The Steps board** as M5 and S15, with the journey counts recomputed after S6's address state.
- **Keep** every fixture, word and order the reviews found true; invent nothing (C11); open the seven captures not read
  (S16), attach the ten again, and say if they did not arrive.

## Who raised what

| Here | Code review | Cast review | Design review |
|---|---|---|---|
| M1 | K1 | | |
| M2 | K2 (a), (b) | | |
| M3 | K3 | | |
| M4 | K4 (narrowed) | | |
| M5 | | C1 | |
| M6 | | | D1 |
| M7 | | | D2 |
| M8 | | | D3 |
| M9 | K22 (a) | | D4 |
| M10 | | | D5 |
| F1, F2 | K2 (c), (d), Q4 | | |
| F3 | K3 | | |
| F4 | K1, Q12 | | |
| F5 | K4, Q11, Q16 | | |
| F6 | K5, Q5 | | |
| F7 | K8, Q10 | | |
| F8 | K9, Q20 | | |
| F9 | K13 (a), (b), Q22 | | |
| F10 | K16, Q7 | | |
| F11 | Q21 | | D4, D19 |
| S1 | K6 | C12 | |
| S2 | K7 | | D16 |
| S3–S13 | K8, K10, K11, K12, K13 (c), K14, K15, K17, K18, K19, K20 | | |
| S14–S16 | | C2–C9 | |
| S17–S20 | | | D6–D9, D11 |
| S21 | K22 (b) | | D10 |
| S22 | K29 | | D12 (a)–(e), D14 |
| S23, S24 | K22 (a) | | D13, D15 |
| Nits | K21, K23–K28, K30 | C10, C11, C13–C15 | D17, D18, D20 |
| Dropped | | | D12 (f) (no owner row needed: K19) |

## The operator's decisions (2026-10-23)

"Yes": F1–F11 all taken as recommended. Round 3's ask is `round-3-ask.txt`; what each means for round 3 and the build:

- **F1** Add role judges the role's own need against the resulting scope; the invitation-only "may not be scoped to a
  sub-speciality" is not applied to a scope the person holds. Round 3 draws the Assessor-first order; the build adds a
  handler test for it.
- **F2** A second role may narrow the first, said before the save ("Pieter Smit's scope becomes Paediatrics, for every
  role he holds.", NEW) and repeated in the result; round 3 draws Smit's board.
- **F3** An SSO-provisioned account holds no password: for `AllowLocalPassword` false the Password part reads "‹name›
  signs in only through ‹provider›. A password cannot be set for them." (NEW), no reset field, and Remove's refusal loses
  its hint; the page reads `AllowLocalPassword` and `HasPassword`. P20–P22 redrawn.
- **F4** The assessor profile's `SpecialityId` and `SubSpecialityId` are dropped in one migration; `InstitutionId` is kept,
  written from the person's institution at save. Spec and build only.
- **F5** Q16 as decided: T288, T333 and T289 land first as their own tasks; T264's, T326's and T286's halves are this
  build's. A Prerequisites block on the Spec, and every O board and T03–T07 says its prerequisite on its face.
- **F6** A new `GetRequestsWaitingForUserQuery(userId, principal)`, read when the lock or Remove dialog opens, sharing an
  Application service with the nudge job: a count, up to five rows and the oldest waiting-since. Round 3 draws N = 2 and
  N > 5 ("and 3 more").
- **F7** A new `RemoveUserInstitutionalSignInCommand(userId, provider, principal)` behind `DemandUserAdministration`,
  `DemandNotCaller` and `DemandInScope`, the last-way-in guard reused, in an action-mode ConfirmDialog; round 3 draws
  another person's sign-in removed. Mbatha's own stays flow 02's.
- **F8** The Delete dialog says what the code keeps: "From each person's next sign-in, belonging to ‹group› no longer
  gives Assessor." and "The scope it gave stays." (NEW); T377 stays its own. O07 redrawn.
- **F9** PagerControls as built, guarded here with `total > 10`: no pager on the small lists, no "Page n of N" (U10
  redrawn); the shared component and flows 06 and 08 untouched.
- **F10** Entrustment decisions joins her menu under Reviews (an owner row; `ActiveNavItemTests` checks it); every round 3
  board of hers carries it.
- **F11** The disclosures are the note panel's pattern, `.detail-card.note-panel` with a form-gap modifier, Cancel as its
  quiet button; DESIGN.md says a form in a note panel leaves with Cancel and a note with Keep. `.disclosure-panel` goes.
