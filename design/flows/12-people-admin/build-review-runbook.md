# Flow 12 build review: the runbook and the cast

Reviewer: read-only, Sonnet. Worktree `t376` at `aeb29eda`, diff against `a95e1daa`. Scope: `act-1-setup.md`, `act-2-onboarding.md`
(intro, Goal, 2.C, outcome SQL), the changed steps in Acts 3, 4, 5 and the appendix, `coverage.md`, `states.md`, against the
built words (`UserPageText.cs`, `UsersListWords.cs`, `InstitutionalAdminHomeWords.cs`, `AssessorsText.cs`,
`AssessorTrainingWords.cs`, `SsoMappingWords.cs`, `UserAdministrationRules.cs`, `PersonScopeWords.cs`, `InstitutionEdit.razor`,
`InstitutionCreate.razor`, `UserDetail.razor`, `AssessorTrainingSection.razor`, `AssessorsList.razor`, `UsersList.razor`,
`InstitutionalAdminDashboard.razor`, the validators and refusal constants), the cast, and `round-3/project/R3-0-steps.dc.html`.
Paths below are under `execution/knowledge/scenario-paediatrics/` unless stated.

Summary: **0 must-fix, 3 should-fix, 4 nit.** Every fixed sentence quoted in the changed Expects matches what the code prints
(a script matched every quoted string of the added lines against `src/`; the only misses were interpolated templates, each
checked by hand). Cast, counts and order hold.

## What was checked and holds

- **Wording:** the Users page (subtitle, "Filter people", Name or email and its help, Role and Status choices and their order,
  Show/Clear, "People 10", the caption, columns, "People 1 of 10 match “zulu”", the hidden "1 of 10 people match “zulu”.", "No one
  matches “zzz”" and its body, "People 5 of 15 hold Assessor", "People 2 of 14 hold no role", the No role note); the user page
  (both subtitles, account line, own-account note and link text, Roles and scope, the Add a role… panel, radio helps, read-outs
  "Already held by …" and "The only sub-speciality … has adopted.", the added result and the scope sentence, Remove… names and
  dialog titles, "System-managed" against "System-managed: waiting to be admitted.", the no-role sentences, Password, Institutional
  sign-in, Lockout lines per role, the lock dialog, the waiting warning, "locked out" and "reactivated" results); Assessors
  (subtitle, heading, caption, columns, "Not recorded", "—", the Record/Edit training names); the training section (both
  refusals, help lines, the saved sentence with and without a date); Home (the waiting line, People, Curriculum, Your
  institution, "1 account: yours.", "15 accounts."); her institution's record (subtitle, required line, Status read-out, "‹name›
  saved. Its contact email is now …", the malformed-address, short-code and duplicate refusals); `/admin/institutions/new` as
  access denied; the SSO card; the old `/admin/assessors/edit` redirect and "User unavailable".
- **The lane's flags:** (1) Users' "Locked out" and the user page's agree. `GetUserByIdQuery.cs:76` passes `user.IsDeactivated`
  into `UserDetailDto.IsLockedOut`, the directory filter uses the same `UserDeactivation.Threshold`, and the Identity
  brute-force lockout (`UserAdministrationService.cs:650`) is read by neither page. (2) The count grammar is built from the
  total: "1 of 14 are locked out" (heading), "1 of 14 people are locked out." (hidden); no step quotes it (R4). (3) Her own page
  does show Institutional sign-in, "No institutional sign-in is linked. Nolwazi Mbatha signs in with an email and a password.",
  and the 2.12 Expect says so.
- **Cast:** Users 10 (2.12), 15 (2.28, 2.44), 14 (A.1.14); "15 accounts."; Home People counts in `WombatRoles.All` order
  (Committee member 4, Assessor 5, Pending trainee 5 or Trainee 5); scopes per person against Steps 2.2–2.7 (Smit and van
  Rensburg "Any speciality", Mokoena "Paediatrics", Sithole "Paediatrics / Paediatrics"); Zulu, Naidoo and Botha gain the
  sub-speciality at 2.13 and the outcome SQL's "the other eleven 1 | 1" still counts; KGK's contact email is `paeds-admin@` at 1.23
  and 2.44, `paediatrics@` after A.6.1, `hod.paediatrics@` after A.6.3, and no step between reads the card; "Fatma" at 2.14 (the
  saved sentence and the Record training name), "Fatima" from 2.41 (2.44, A.5.15, the SSO-only state); Patel's one waiting request
  at A.6.4 is du Plessis's review aged to `D−8` (3.2x, A.6.8); du Plessis "left the programme on `D−2`" (Act 5), Molefe "completed
  … on `D`".
- **Order:** 2.14 (Record training, `?section=training`) needs only 2.13 (Assessor) and the five holders exist; 2.15 reads the
  list then types the old address, and `AssessorProfileEdit` redirects with `replace: true` to the same section; the profile-id SQL
  uses `AssessorProfiles."Id"`/`"UserId"`, both kept. The outcome SQL's training query no longer touches the dropped columns and
  joins on `"InstitutionId"`, which the profile keeps; no other file in the runbook names `AssessorProfiles`. 2.28 gains a Home
  read and an Add a role… open-and-cancel with no state change. 2.44's Do presses the Assessor count (a link to `?role=Assessor`).
  5.17 and 5.10 now go through `/admin/users`; the Users search finds "Molefe". Nothing after 2.14 relies on 2.14 ending open
  sessions (the old T279 write moved to 2.13, noted in 2.13). The 3.52 and 2.37 Notes follow.
- **Coverage:** every `Route:` line against every `coverage.md` row (script over all 331 steps): no difference on `/`,
  `/access-denied`, `/admin/users`, `/admin/users/{UserId}`, `/admin/assessors`, `/admin/assessors/edit`,
  `/admin/institutions/new`, `/admin/institutions/{Id:int}`, `/not-found`, `/admin/trainees`, `/admin/entrustment-decisions`,
  `/admin/sso/group-mappings` (the only differences anywhere are old, unrelated rows). 78 `@page` files and 88 templates match
  `src/`. The new jobs-table rows list exactly the routes of their steps.
- **Steps board:** every row of R3-0-steps (1.23, 2.12–2.15, 2.44, A.1.14, A.3.1, A.4.5, A.5.5, A.6.3, A.6.4, A.6.7, A.7.9,
  A.7.14, the crossing steps, the journeys) is carried by the runbook in its words; the journeys 3 + 1 / 4 + 1, 2 + 0, 3 + 0,
  4 + 2 / 5 + 2 / 3 + 2, 1 + 1, 3 + 0, 2 + 1 recount from the Do lines.

## Findings

### R1 — S: states.md still sends two Entrustment decisions rows through Home's Quick links

`states.md:594` ("Entrustment decisions from her dashboard's quick link") and `states.md:605` ("then the dashboard's Entrustment
decisions"). Home has no Quick links and no card names the page since T376 (Q7); it is in her menu under Reviews (F10). A replay
agent looks for a link that is not there. Fix: "from the menu, under Reviews" in both; the 4.2 detour is then a menu press.

### R2 — S: Step 1.6 and its states rows ignore that the Create institution form now refuses in Wombat's words

`act-1-setup.md:196-200` (Actual and Gap for 1.6: "The Name field is required.", "[F-1.6b, T324] still") and `states.md:786`
("Required fields empty … No change.") quote nothing, and `coverage.md`'s T376 paragraph does not list F-1.6b as closed.
`InstitutionCreate.razor` uses `UpdateInstitutionCommandValidator.NameMissing` and `.ShortCodeMissing` ("Type the institution's
name. The record was not saved.", "Type a short code. The record was not saved."), so the next replay finds the gap gone with no
Expect to confirm it. Fix: in 1.6's Expect, "An empty Save reads … under Name and … under Short code", say in the Note that F-1.6b
is expected closed, and add F-1.6b to coverage.md's list of gaps the build closes; quote both in the states row.

### R3 — S: A.6.9 omits what devadmin's own page does offer

`appendix-cross-cutting.md:1132-1139`: the Expect lists what is not offered but not Institutional sign-in, which his page does
draw (`UserDetail.razor:995-1040`, shown on any own account): "No institutional sign-in is linked. Demo Administrator signs in
with an email and a password." 2.12 says it for Mbatha; A.6.9 should for devadmin or an agent will log a surplus section. Fix: add
the sentence and "only Institutional sign-in".

### R4 — N: no step quotes the Locked out filter's count

`appendix-cross-cutting.md:1048-1071` and `states.md:872` play Users with Status Locked out but quote only Patel's row. The heading
reads "People 1 of 14 are locked out" and the hidden status "1 of 14 people are locked out." (`UsersListWords.HeadingCount`,
`LiveCount`). Fix: quote both, so the grammar is checked once.

### R5 — N: 2.13's note that abandoning an add is 2 presses "at 1280 and at 390"

`act-2-onboarding.md:337`. The Steps board gives 3 + 0 at 390 for the add and states 2 + 0 for abandoning only at 1280. The
Cancel path is the same at 390, so the figure is likely right; say "at 1280" or add the board's 390 figure.

### R6 — N: 5.10 takes the user id from an address that now carries `?back=`

`act-5-graduation.md:342`. Manage links carry `?back=%2Fadmin%2Fusers%3Fq%3DMolefe` (`PeopleLinks.User`). Fix: "from the path of the
page's address (before any `?`)".

### R7 — N: states.md:8 still says "67 page files"

Written 2026-09-26; `coverage.md` says 78 (and `src/` has 78). Not flow 12's own, but the lane touched the file and the build adds
`InstitutionCreate.razor`. Fix: "the page files of `coverage.md`'s Pages table".
