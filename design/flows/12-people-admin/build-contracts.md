# T376 build contracts (flow 12)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report. Each name says
whether it exists (a path, at master `a95e1daa`) or is new (on `t376`, written by § 0). Lanes U, P, T and I run side by
side in one wave (`build-lanes.md`).

## § 0: on `t376` before any lane is cut, written by the integrator's planner

- **`design/flows/12-people-admin/round-3/boards-text.md`**: the text of every `R3-*` board (175: the Spec, Spec 2, Spec
  3 and Steps, then H, U, P, T, I, O; style, script, svg, head and helmet blocks stripped; aria-labels, placeholders and
  input values kept as `[aria-label: …]`, `[value: …]`), one heading per board with its `canvas.json` title, in the
  canvas's order, as flow 11's. A table's cells are one `| ` line each. Design data, read by every lane.
- **The one migration** (F4, with M1's command, so that lane T's section and lane P's page both build on the profile as it
  will be, and no lane needs `dotnet ef`):
  - `AssessorProfile` (`src/Wombat.Domain/Identity/AssessorProfile.cs`) loses `SpecialityId`, `SubSpecialityId`,
    `Speciality`, `SubSpeciality`; `InstitutionId` stays, documented as written from the person at every save.
  - `AssessorProfileConfiguration.cs`: the two foreign keys gone.
  - `20261023132146_T376_AssessorProfileTrainingOnly` (`.cs` and `.Designer.cs`, made by `dotnet ef` and read: two
    `DropForeignKey`, two `DropIndex`, two `DropColumn` on `AssessorProfiles`; `Down` restores them), and the snapshot's
    24 lines removed. **No lane adds a migration.**
  - **M1, the command** (`src/Wombat.Application/Features/Assessors/CreateOrUpdateAssessorProfile.cs`):
    `CreateOrUpdateAssessorProfileCommand(string UserId, AssessorTrainingStatus TrainingStatus, DateOnly?
    TrainingCompletedOn, string Qualifications, ClaimsPrincipal Principal) : IRequest<AssessorProfileDto>` (Spec 2 writes
    `IRequest`; the DTO return is kept so the section can word its result from what was saved). The handler asks
    `DemandUserAdministration`, then `DemandNotCaller` (`OwnTrainingRefused`), then `DemandInScope` (`AssessorNotInScope`),
    refuses a person with no institution (`AssessorHasNoInstitution`) or without Assessor (`NotAnAssessor`), stamps
    `InstitutionId` from the person, and **writes no scope** (no `UpdateScopeAsync`, so no stamp change). The validator
    keeps `NotEmpty`/`IsInEnum` with no words of its own: **the words are lane T's** (T05's "Choose the training status.",
    "Type the qualifications, as they should read.").
    T289's `AssessorAtAnotherInstitution` is gone (no form names an institution).
  - `AssessorProfileDto` loses `SpecialityId`, `SpecialityName`, `SubSpecialityId`, `SubSpecialityName`;
    `GetAssessorProfileById` and `ListAssessorsForSpeciality` (its `int? SpecialityId` parameter gone:
    `ListAssessorsForSpecialityQuery(ClaimsPrincipal)`) compile against it; `AssessorsList.razor` lost its two scope
    columns and `AssessorProfileEdit.razor` sends the new command (both lane T's to replace). Tests:
    `AssessorProfileRefusalTests` (rewritten: no scope written, the person's institution, own training refused),
    `AssessorTrainingStatusTests`, `ActionFocusTests.AssessorProfile()`.
- **What the directory reads** (`src/Wombat.Application/Common/Interfaces/IUserAdministrationService.cs`), shared because
  lane U's list and lane P's page both need it, and only one lane may touch the file's existing members:
  - `UserIdentityDetails.CollegeId` (NEW, `int? CollegeId = null`, last): a College admin's College, which S1's scope
    reads. `UserAdministrationService.Map` fills it.
  - `AccountSignInMethods.AllowsLocalPassword` (NEW, `bool AllowsLocalPassword = true`, last): `AllowLocalPassword` read
    apart from `HasLocalPassword` (which stays `Allow && hash`), for F3. `GetSignInMethodsAsync` fills it
    (`InstitutionalSignInMethodsTests.WhetherTheAccountMayUseAPassword_IsReadApart_FromWhetherItHasOne`).
  - Lane U adds one member to the interface and the service (§ 1); no other lane changes either file.
- **The scope in words** (S1; Users' Scope column is lane U's, the who line and Add role's results lane P's, Assessors'
  scope lane T's): `src/Wombat.Application/Features/Users/PersonScopeWords.cs` (NEW):
  - `PersonScopeWords.For(roles, institutionName, collegeName, specialityNames, subSpecialityNames)`: Institutional admin →
    the institution's name; College admin → the College's; no institution → `NoScope` ("—", U14's Administrator);
    otherwise `Disciplines(...)` ("Paediatrics / Paediatrics", "Paediatrics", "Paediatrics, ‹second›") or `AnySpeciality`
    ("Any speciality"). Names sort ordinally ignoring case. A person with no role reads by the same rule.
  - `PersonScopeWords.NoInstitution` = "Global (no institution)" (the one wording for an account with no institution,
    round 2's PROPOSED: Users' Institution column and the who line).
  - `PersonScopeNames.LoadAsync(IApplicationDbContext, IEnumerable<UserIdentityDetails>, CancellationToken)`: the
    institution, College, speciality and sub-speciality names for a set of people in four queries; then
    `Scope(person)`, `InstitutionName(person)`, `CollegeName(person)`, `SpecialityNames(person)`,
    `SubSpecialityNames(person)`. A query loads it once for its rows; never per row, never in Razor.
  - Tests: `tests/Wombat.Application.Tests/Features/Users/PeopleSharedWordsTests.cs`.
- **The roles in one order:** `WombatRoleLabels.InOrder(IEnumerable<string>)` (NEW, `src/Wombat.Domain/Identity/
  WombatRoleLabels.cs`): the keys held, each once, in `WombatRoles.All`'s order (an unknown key last). Users' Roles cell
  (lane U) and the user page's Roles and scope (lane P) list by it and print `WombatRoleLabels.For`. The Role filter's
  option order (U14: Pending trainee before Trainee) is lane U's own list.
- **The ended programme** (S2, Q13; Users' No role cell is lane U's, the user page's sentence lane P's):
  `src/Wombat.Application/Features/Users/EndedProgrammes.cs` (NEW): `EndedProgrammeDto(DateOnly EndedOn, bool Completed)`
  and `EndedProgrammes.LatestForAsync(IApplicationDbContext, IReadOnlyCollection<string> userIds, CancellationToken)` →
  the latest ended trainee profile per person (a profile with no recorded end is left out). Ask it for the people who hold
  no role. Lane U's cell says only that one exists ("A completed or withdrawn registrar keeps an account and its
  records."); lane P's sentence says how and when ("they left the programme on 2026-10-18", "they completed the programme
  on 2026-10-20").
- **Her institution's name** (S9; Users' and Assessors' subtitles, Home's card): `GetMyInstitutionNameQuery(ClaimsPrincipal
  Principal) : IRequest<string?>` (NEW, `src/Wombat.Application/Features/Institutions/Queries/GetMyInstitutionName/`):
  read now from the claim's id; null for the Administrator or a gone institution. A page sends it before its list; while
  it loads, and when the list fails, the subtitle is the one without the name (U11, U12, T12, T13). The record (lane I)
  needs it not: I05 and I06 read "Institution".
- **The DTO files split** (no change of shape): `UserDtos.cs` is now `UserSummaryDto.cs` (lane U's) and `UserDetailDto.cs`
  (with `UserPendingInvitationDto`, lane P's), so the two lanes change their own DTO without meeting in one file.
- **`PeopleLinks`** (NEW, `src/Wombat.Web/Components/Shared/PeopleLinks.cs`, beside `OnboardingLinks`), the routes, so no
  page writes one by hand:
  - `const string Users`, `Assessors`, `LegacyAssessorEdit` (`/admin/assessors/edit`), `SsoMappings`; `TrainingSection =
    "training"`; Users' parameter names `QueryParameter` (`q`), `RoleParameter` (`role`), `StatusParameter` (`status`),
    `PageParameter` (`page`), `SizeParameter` (`size`), `BackParameter` (`back`), and `NoRole = "none"` (the Role filter's
    No role). The Status filter's values are lane U's (lower-case words; say which in the report).
  - `UsersFiltered(q, role, status, page, size)` (Home's People card links each role, lane U); `User(userId, section,
    back)`; `UserTraining(userId)` = `/admin/users/{id}?section=training` (lane T's Record training links and redirect);
    `Institution(int id)` = `/admin/institutions/{id}` (Home's card, lane U); `SafeUsersReturn(string? back)`: S6's
    allow-listed return, the Users path with only Users' own parameters, rebuilt from their parsed values, anything else
    `/admin/users`.
  - **The way back** (S6): lane U writes each row's Manage as `PeopleLinks.User(id, back: ‹this page's Users address›)`;
    lane P reads `[SupplyParameterFromQuery] back` and gives the trail's Users crumb `PeopleLinks.SafeUsersReturn(back)`.
  - `tests/Wombat.Web.Tests/Admin/PeopleLinksTests.cs` holds each against the page it names and the allow-list against a
    crafted `back`. `PeopleLinks.User(id)` equals `OnboardingLinks.User(id)` (a test); flow 11's pages keep theirs.
- **`AssessorTrainingSection`** (NEW, `src/Wombat.Web/Components/Pages/Admin/Users/AssessorTrainingSection.razor`): the
  user page's "Assessor training" section as a component, so lane P mounts it and lane T fills it. A stub that renders
  nothing. Parameters: `string UserId` (EditorRequired), `string PersonName` (EditorRequired), `bool HoldsAssessor`,
  `bool Open` (the page was opened with `?section=training`), `EventCallback OnActed` (raised when the section starts an
  action, so the page clears its own result). Lane T may add a parameter only with a default.
- **`UserPageWords`** (NEW, `src/Wombat.Web/Components/Pages/Admin/Users/UserPageWords.cs`): the not-found words as built
  (`Heading` "User", `UnavailableTitle` "User unavailable", `UnavailableBody` "The user could not be found or is outside
  your scope.", `GoToUsers` "Go to Users"), so lane P's page and lane T's redirect (S10) say the same.
- **F10, her menu:** `NavItems.ByRole[InstitutionalAdmin]`'s Reviews group gains `EntrustmentDecisions`
  (`NavItems.cs:155`). It is an item, so it lights itself and needs no owner row; `ActiveNavItemTests.
  EntrustmentDecisions_LightsItself_WithNoTrail` gains the Institutional admin, and DESIGN.md's menu table her row.
- **`app.css`, `/* ── People admin ── */` then `/* Flow 12: the people pages, shared (T376) */`** (`app.css:6464–6594`),
  rebuilt from `f12r3.css` on the spacing scale:
  - `.people-page` (the list pages' column: Users, Assessors, SSO mappings; 24px between parts, each margin 0);
  - `.user-page` (60rem; `> *`, `.detail-list dd` wrapping, `.action-result .alert` margin 0), `.user-section` (h2 1.25rem,
    h3 1.1rem, `.actions-cell` at the start), `.section-part` (a rule above every part but the first), which lane T's
    section uses exactly as lane P's sections do;
  - `.note-panel--form` (F11: the system's `.note-panel` at `app.css:2314`, its parts 24px apart; `.form-actions` margin 0);
  - `.badge.status-locked` (the lock icon's gap in the badge, Users and the who line);
  - phone rules (≤640.98px): 44px controls in `.people-page` and `.user-page`; `.user-page .form-actions` and
    `.note-panel--form .form-actions` stacked `column-reverse` (the primary on top; flow 11's lesson); full width only on
    `.user-page .form-actions .btn` and `.user-page .section-part > .btn`, **never a bare `.btn`** (M6, the Show toggle);
    `.user-page .choice-group .form-check label` a 44px flex row (S17; scoped, so flows 08 and 11 are untouched).
  - `tests/Wombat.Web.Tests/Design/StylesheetRuleTests.Flow12Shared.cs`. DESIGN.md § app.css section order has the heading
    and this line; each lane adds its own line under it.
  - Classes that already exist and are reused, not restated: `dl.form-group` / `dl.form-group > dd` (`app.css:596`, the
    read-out, M8), `.readonly-value` (`:4712`), `.choice-group` / `.is-invalid` (`:4683`), `.note-panel` (`:2314`),
    `.person-cell` (`:4700`), `.list-section-title`, `.detail-list` (`:1980`), `.clinic-table--stack-wide`, flow 11's
    `.loading-line`, `.skel-*`, `.inv-when` and its general phone rules (dialogs, header actions, stack-wide row buttons).

## § 1: lane U provides

- **The paged list:** a new `IUserAdministrationService` member, appended after the last one (lane U names it; Spec 2's
  "one paged SQL query over Users joined to UserRoles, no N+1"), with a default implementation over `ListAllUsersAsync` so
  the test directories need not override it, and the real one in `UserAdministrationService` (its own region, after
  `GetSignInMethodsAsync`'s). S6's match (each word against first name, last name or email), S7's order
  (`LastName.ToLower()`, `FirstName.ToLower()`, `Id`, in SQL), Role with No role, Status, page and size. Lane U's alone;
  nothing else reads it.
- **Status:** "Locked out" is the administrative lock (`UserIdentityDetails.IsDeactivated`), never Identity's brute-force
  lockout (the nit, PROPOSED). Lane P's who line reads the same field: `IsDeactivated`, not `IsLockedOut`.

## § 2: lane P provides

- **The training section's mount** (contracts § 0): `<AssessorTrainingSection UserId PersonName HoldsAssessor Open
  OnActed />` between "Roles and scope" and "Sign-in and access", on every person's page lane P draws (none for the
  Trainee note, P26, or before the read), `HoldsAssessor` from the page's roles (so Add role's Assessor shows it at once,
  Remove hides it, Q18), `Open` true only for `section=training` (`PeopleLinks.TrainingSection`, ordinal). When the
  section raises `OnActed`, the page clears its own `ActionResult`; when the page acts (Add role, Remove, reset, lock), it
  re-renders the section with its parameters unchanged (the section keeps its own state).
- **`?section=training` focus:** the page lets the section take the focus when `Open` (T03: the first radio); it focuses
  nothing else on load.
- `GetAdoptedDisciplinesForInstitutionQuery` (S13), `GetRequestsWaitingForUserQuery` (F6) and
  `RemoveUserInstitutionalSignInCommand` (F7) are lane P's alone.

## § 3: lane T provides

- **The section**, filled behind § 0's parameters, with its own read (lane T names it: the training record of `UserId`,
  in scope, null when none) and the M1 save; "Record training" / "Edit training", the note panel (F11), its result, the
  S10 sentence for a person without Assessor when `Open`.
- **The redirect:** `/admin/assessors/edit?id=‹profile id›` → `PeopleLinks.UserTraining(profile.UserId)`; an unknown,
  out-of-scope or missing id → the "User" page with `UserPageWords`' panel. `PeopleLinksTests`'s row for
  `LegacyAssessorEdit` names whatever component keeps the route.

## § 4: lane I provides

- Nothing another lane reads. Home's card (lane U) links `PeopleLinks.Institution(id)` and the record's own Cancel goes to
  Home (`/`); the record's words ("‹name› saved. Its contact email is now ‹address›.") are lane I's.

## § 5: words and constants already built and kept (lanes use them, do not reword)

- `UserAdministrationRules.TraineeNoteOnUserPages` and `TraineeAdministersNoUser` (the T278/T290 notes, asked before any
  read, S8: no subtitle with them), `MayNotAdministerUsers`, `DemandUserAdministration`, `DemandNotCaller`, `DemandInScope`
  (`src/Wombat.Application/Features/Users/UserAdministrationRules.cs`); `TraineeRoleNotOffered` ("Trainee is not offered: …",
  `UserAdministrationRules.cs:50`) and `TraineeRoleByAdmissionOnly` (`:42`); the six role descriptions as built.
- The reset's help ("Sets a new password directly. The user is not emailed; share the password securely out-of-band.") and
  its refusal (`UserAdministrationService.ResetRefusedMessage` "The password was not reset." then the rules' words);
  `PasswordField`, `PasswordRules` (flow 02).
- The lockout list's sentences (`UserDetail.razor:157–164` today: "Locking out the user prevents sign-in immediately. Until
  they are reactivated:", the four effects, "Their records stay as they are, …"), shown per person (Q17, the nit).
- "The institutional sign-in is the account's only way in." (flow 02's last-way-in refusal), F3's and F7's.
- The SSO refusals as built after T288 and T333 (`CreateSsoGroupMapping.cs`: `ProviderNotTheInstitutions`,
  `AdmissionRoleNotMappable`, the Administrator refusal) and "An institution with the same name or short code already
  exists." (I08).
- `StatePanel`'s Try again and `LoadingContent`, `ActionResult` (`Id`, `FocusOnLoad`, `FocusAfterRender`), `ConfirmDialog`
  (`CancelLabel`, `DangerAction`, `BodyContent`), `DataTable` (`StackWide`, `Caption`), `PagerControls` **as built**
  (F9: unchanged; rendered only when `TotalCount > PageSize`, round-3-check Left 2, so flows 06 and 08 are untouched),
  `DashboardFrame`, `DashboardCard`, `PageHeader` (`Trail`), `FormField`, `FormActions`, `Alert` (`Role=""` for a standing
  notice, S23), `Icon`, `RowNames.Distinct`, `PageFocus.FocusByIdAsync`, `RefusalText.Of`, flow 11's `InvitationScopeWords`
  (not used for a person: a person's scope is `PersonScopeWords`).

## Across lanes

- **Typed refusals:** a lane that needs a refusal's reason on the page writes its own exception on the pattern of
  `InvitationIssueRefusedException` / `TraineeAdmissionRefusedException` (derived from `InvalidOperationException`, a
  reason enum, `…Refusals.IsRefusal`); no shared base. A refusal's message never names another institution's person.
- **Who acts on whom:** every Users command keeps `DemandUserAdministration` first, `DemandNotCaller` where it changes an
  account, and `DemandInScope`; an out-of-scope or unknown id reads as not found (404 words), never a refusal that leaks it.
- **Dates and times:** ISO dates; a South African day from `ProgrammeCalendar.DateOf(utc)`; a moment "2026-10-12 12:35
  SAST" from `ActivityMoments.When` (the waiting request's "since"). No `ToLocalTime()`.
- **Titles** (each tab "{h1} · Wombat"): "Users"; the person's name, "User" before the read and for the note and not
  found; "Assessors"; the institution's name, "Institution" before the read and on failure (S9); "SSO mappings"; "Home".
- **Owners:** no menu item changes but F10's (§ 0). Lane T: `NavOwners`' `AssessorProfileEdit` row goes or stays with the
  route's component (say which). Lane I: an owner row for the Administrator's create page if S12 gives it a component of
  its own (Institutions, Administrator only). The record for her keeps no owner row (Home › title).
- **No new role check** beyond S12's: every page keeps its policy (`AdministratorOrInstitutionalAdmin`; Home's
  `Roles = "InstitutionalAdmin"`); S12 moves `/admin/institutions/new` to the Administrator alone.

## § 6: words on no board, marked NEW (the fix pass, build review P5)

The sentences the build ships that no round 3 board draws, each with why there is none. Round 4, if any, sees these.

| Words | Where | NEW because |
|---|---|---|
| "That did not go through. Nothing has changed. Try again, or come back in a few minutes." | `UserPageText.ActionFailed` | the user page's unexpected action failure (T329); Spec 3 draws only the load failure's sibling |
| "Not changed here." | `UserPageText.RoleRowNote` | a role row for a role this surface does not change (Administrator, College admin), which no board's cast holds |
| "‹name› has no institution, so no role can be added here: every role offered here works at an institution." | `UserPageText.NoInstitutionForRoles`, `UserAdministrationRules.RoleNeedsAnInstitution` | U14's College admin, drawn only for the Administrator's view of Users |
| "‹institution› has adopted nothing this role can be scoped to yet, so it cannot be added. Adopt a curriculum first." | `UserPageText` (Add a role…'s read-out) | an institution with no adoption, which no board's cast is |
| "Could not check for requests waiting for ‹name›. Any that wait stay with them." | `UserPageText.WaitingUnread` | F6's read failing when the dialog opens; the boards draw the read answered |
| "No accounts yet" / "Nobody has an account here yet. Use Invite a person to add the first." | `UsersListWords.EmptyTitle`, `EmptyBody` | an institution with no account at all; her own account means Users is never empty for her |
| "‹name› can no longer sign in through ‹provider›. ‹name› still signs in through their other institutional sign-in(s)." | `UserPageText.SignInRemoveBody` | P4: the dialog for an account with no password and two sign-ins; the board draws the password case only |
| "This person's institution changed while the role was being added. Nothing was saved." | `AddRoleToUserCommand` | B2: a move committed while Add a role… was open; shown as the fixed action failure, logged |
| "Institutional administrators may only be scoped to an institution.", "The selected role may not be scoped to a sub-speciality.", "Speciality administrators must be scoped to a speciality.", "The selected role requires speciality and sub-speciality scope." | `CreateSsoGroupMappingCommandHandler` | B3: a posted mapping scope; the built words Add a role… and the form's own checks already use |
| "Choose a speciality the institution has adopted." / "Choose a sub-speciality of that speciality the institution has adopted." | `CreateSsoGroupMappingCommandHandler` | B3: a posted speciality or pair the institution has not adopted; the page offers only adopted pairs |
