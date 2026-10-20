# T373 build contracts (flow 11)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report. Each name says
whether it exists (file:line at `69a7df20`, or at § 0's commit on `t373` where § 0 wrote it) or is new. Lanes I, G and A
run side by side in one wave (`build-lanes.md`).

## § 0: on `t373` before any lane is cut, written by the integrator's planner

- **`design/flows/11-onboarding/round-3/boards-text.md`**: the text of every `R3-*` board (style, script, svg and helmet
  blocks stripped), one heading per board with its `canvas.json` title, in the canvas's order, as flow 10's. Design data,
  read by every lane.
- **The one migration** (F3), so that lane A's Home can read what lane I writes without waiting for lane I:
  - `Invitation.IssuedAsRole` (NEW, `string?`, `src/Wombat.Domain/Invitations/Invitation.cs:42`): the role the issue was
    authorised as, a `WombatRoles` key. Null on rows before T373 (nothing is live: no backfill).
  - `InvitationConfiguration.cs`: `HasMaxLength(64)`, nullable, no index.
  - `20261020055819_T373_InvitationIssuedAsRole` (`.cs` and `.Designer.cs`, made by `dotnet ef`, read: one `AddColumn`,
    `character varying(64)`, nullable), and the snapshot's one new property.
  - **Lane I writes it** at issue (contracts § 1); **lane A reads it** for "Invited by" (§ 3). No lane adds a migration.
- **F7, a link's last day on the South African calendar**, shared because the issue and the list are lane I's and the
  preview and the accept lane G's:
  - `Invitation.LinkExpiresOn(DateTime utcNow)` (exists, now `Invitation.cs:108`): `ProgrammeCalendar.DateOf(utcNow)` + 14
    days. Was the UTC date's.
  - `Invitation.HasPassedLastDay(DateOnly expiresOn, DateTime utcNow)` (NEW, `Invitation.cs:114`): `expiresOn <
    ProgrammeCalendar.DateOf(utcNow)`. A link works through its last day, until midnight SAST.
  - `InvitationRules.GetActiveInvitationOrThrow(invitations, rawToken, tokenService, DateTime utcNow)` (exists,
    `InvitationRules.cs:125`; the `utcNow` parameter is NEW): judges `Expired` by `HasPassedLastDay`.
  - `AcceptInvitationCommandHandler` and `GetInvitationPreviewQueryHandler` take an optional `TimeProvider? timeProvider =
    null` (NEW, last parameter) and pass its UTC now; the accept stamps `UsedOn` from it too.
  - `ListActiveInvitationsQueryHandler` keeps `ExpiresOn >= ProgrammeCalendar.DateOf(utcNow)`;
    `ResendInvitationCommandHandler` refuses by `HasPassedLastDay`.
  - Tests: `tests/Wombat.Domain.Tests/Invitations/InvitationCalendarTests.cs`; the preview and the list at 21:59 and 22:00
    UTC on the last day in `tests/Wombat.Application.Tests/Features/Invitations/InvitationSharedWordsTests.cs`.
- **S4, one address, one spelling** (Q10's check is lane I's; the accept's sweep lane G's; the used invitation's reads lane
  A's):
  - `Invitation.NormaliseAddress(string email)` (NEW, `Invitation.cs:121`): trimmed, lower-cased (invariant).
  - `InvitationAddress.ToAddress(this IQueryable<Invitation>, string email)` (NEW,
    `src/Wombat.Application/Features/Invitations/InvitationAddress.cs`): the invitations whose `Email.ToLower()` equals
    the normalised address. Every comparison of an invitation's address with another address goes through it (or, in
    memory, through `NormaliseAddress`). A page shows the stored spelling.
- **The role and scope in the pages' words** (Register's card is lane G's, the mail and the list lane I's, Waiting and
  Home lane A's): `InvitationScopeWords` (NEW, `src/Wombat.Application/Features/Invitations/InvitationScopeWords.cs`):
  - `static string? Disciplines(string? specialityName, string? subSpecialityName)`: "Paediatrics / Paediatrics",
    "Paediatrics", or null.
  - `static string RoleAndScope(string targetRole, string? institutionName, string? collegeName, string? specialityName,
    string? subSpecialityName)`: the role's label (`WombatRoleLabels.For`), " in ‹Disciplines›" when there is one, then
    " at ‹institution›" (or " at ‹College›"): "Coordinator at Kgosi Kgari Teaching Hospital", "Assessor in Paediatrics /
    Paediatrics at Kgosi Kgari Teaching Hospital", "College admin at College of Paediatricians of South Africa". The caller
    writes "as" before it.
- **`InvitationLinks`** (exists, moved unchanged from `IssueInvitation.cs:135–147` to
  `src/Wombat.Application/Features/Invitations/InvitationLinks.cs`), so lane G can change the link's path (§ 2) without
  touching lane I's handler.
- **`OnboardingLinks`** (NEW, `src/Wombat.Web/Components/Shared/OnboardingLinks.cs`, beside `MsfLinks.cs`), the routes,
  so no page writes one by hand:
  - `const string Invitations = "/admin/invitations"`, `InviteAPerson = "/admin/invitations/new"`, `Trainees =
    "/admin/trainees"`, `CurriculumAdoptions = "/admin/adoptions"`.
  - `static string InviteFrom(int invitationId)` = `/admin/invitations/new?from={id}` (lane I: N19, N20, Q9);
    `Admit(string userId)` = `/admin/trainees/admit?userId={escaped}` (lane A); `Admitted(int traineeProfileId)` =
    `/admin/trainees?admitted={id}` (lane A, S8); `User(string userId)` = `/admin/users/{escaped}` (lane I's N15, lane A's
    M13). Invariant culture.
  - `tests/Wombat.Web.Tests/Admin/OnboardingLinksTests.cs` holds each against the page it names; lanes I and A add their
    new pages' rows (`InviteAPerson`, `AdmitRegistrar`). Register's paths stay `RegisterOutcome`'s (lane G).
  - Today's literals it replaces, each the owning lane's to swap: `HomeFrame.cs:51` (lane I),
    `PendingTraineesList.razor:51`, `:83`, `:111` (lane A). `NavItems.cs:121–122` stay, held equal by the test.
- **`DataTable`'s `StackWide`** (NEW, `src/Wombat.Web/Components/Shared/DataTable.razor:50`): writes `clinic-table--stack
  clinic-table--stack-wide` and the explicit roles `Stack` gives (the caller's rows carry `role="row"`,
  `role="columnheader"`, `role="rowheader"`, `role="cell"`). A note row is a second `<tr>` from the `Row` fragment
  (`tests/Wombat.Web.Tests/Design/DataTableStackWideTests.cs`). Both lists of lane I and both of lane A use it.
- **`app.css`, `/* ── Onboarding ── */` then `/* Flow 11: the onboarding pages, shared (T373) */`** (`app.css:6157–6267`),
  rebuilt from `r3.css` on the spacing scale:
  - `.inv-page` (the column, 24px between parts, each part's margin 0), `.inv-page--form` (48rem), `.inv-page
    .list-section-title` and `.inv-page .action-result .alert` (margin 0), `.inv-count` (the muted count beside a
    section's title), `.inv-when` (the date, `nowrap`) with `.inv-when span` (the second line, block, muted, wrapping);
  - M7: `.action-result .alert, .validation-message, .alert { overflow-wrap: anywhere }`;
  - the loading state: `.loading-line` (muted, `role="status"` in the markup), `.skel-title`, `.skel-line`, `.skel-short`
    on a raw `div.skeleton` (no inline style: `Skeleton`'s own `Width`/`Height` write one);
  - **the general rules** (M8, M9, S16): ≤900px `.clinic-table--stack-wide .btn` 44px; ≤640px `.dialog-card .form-actions`
    `column-reverse` with 44px full-width buttons, `.header-container .actions-cell` full width with 44px buttons (`flex: 1
    1 auto`), `.inv-page .btn` 44px. Flows 08's (`.committee-calendar .header-container …`, `.committee-calendar
    .clinic-table--stack-wide .btn`) and 10's (`.msf-page dialog …`, `.msf-page .clinic-table--stack-wide td .actions-cell
    .btn`) scoped copies are folded into them (`StylesheetRuleTests.Flow11Shared.cs`; `Flow10Shared.cs` reads the general
    rules now).
  - DESIGN.md: § Button system (every dialog stacks; every header's actions take the row), § Table system (`StackWide`),
    § app.css section order (the Onboarding section). The lanes write the rest of DESIGN.md (`build-lanes.md`).
- **Icons** (NEW, Lucide, the system's format `<svg id="i" …viewBox="0 0 24 24">`): `wwwroot/icons/hourglass.svg` (the
  wait card) and `arrow-right.svg` (its links), the paths the P boards draw.

## § 1: lane I provides

- **`Invitation.IssuedAsRole` written at issue** by `IssueInvitationCommandHandler`, from the principal, never from the
  page: `WombatRoles.Administrator` when `request.Principal.IsAdministrator()`, otherwise `WombatRoles.InstitutionalAdmin`
  (the page admits no one else; `AdministratorOrInstitutionalAdmin`). The resend keeps it. The "acting role" of the boards
  is the role the issue was authorised under: an Administrator acting as an Institutional admin is recorded as
  Administrator, and no "Invited by" line shows (say so in the report; it is F3's rule read safely).
- **`InvitationEmail.Build(string toEmail, string roleAndScope, string registrationUrl, DateOnly expiresOn, string
  deliveryKey)`** (exists, `InvitationEmail.cs:11`; its second parameter becomes the scope words): S12's words. Its callers
  are lane I's (`IssueInvitation`, `ResendInvitation`). Lane G's link path reaches it through `InvitationLinks`.
- **`IUserAdministrationService.FindByEmailAsync(string email, CancellationToken)`** (NEW; `UserIdentityDetails?`,
  normalised as S4) and its `UserAdministrationService` implementation: Q12's lookup. Lane I's alone.

## § 2: lane G provides

- **The link's path:** `InvitationLinks.RegistrationUrl` (§ 0) writes `{base}/account/register/start?token=…` (S2), and
  `RegisterOutcome.StartPath = "/account/register/start"` (NEW) equals it (a Web test). The OneTimeLink (lane I) and the
  mail (lane I) both build from `InvitationLinks.RegistrationUrl`, so lane I changes nothing for S2.
- **`InvitationPreviewDto`** (exists, `InvitationDtos.cs:5`) gains, as `init` properties or positional members lane G
  chooses: `string? InstitutionName`, `string? CollegeName`, `string? SpecialityName`, `string? SubSpecialityName`,
  `DateOnly ExpiresOn`. Only Register reads it.
- **`SignOutOutcome`'s `returnTo`** (F5): the sign-out endpoint (`Program.cs:760–799`) accepts a form field `returnTo` only
  when it equals `RegisterOutcome.PagePath` (ordinal), and then lands there instead of `SignOutOutcome.SignedOutUrl`. No
  other value changes anything (never an open redirect). Flow 02's page and the shell's Sign out send no `returnTo` and
  are unchanged.

## § 3: lane A's reads of what § 0 and lane I write

- **"Invited by"** (P01–P03, F3; check rule 7): the used invitation to her address (`InvitationAddress.ToAddress`, `UsedOn`
  not null, the latest), shown only when its `IssuedAsRole == WombatRoles.InstitutionalAdmin` and the issuer, read now
  (`GetDisplayNamesAsync` or `GetByIdAsync`), exists and is not deactivated. "‹name›, Institutional admin, on ‹date›",
  the date the invitation's `IssuedOn` on the South African calendar (`ActivityMoments.Date`'s rule, done in the
  Application as `ProgrammeCalendar.DateOf`), **not** `UsedOn` (the boards' DATA chip says UsedOn; the check overrides it).
  After a resend `IssuedOn` is the current link's (F2), so it is that day.
- **"Registered"** on Waiting and Admit: the used invitation's `UsedOn` as a South African date; "—" without one (an SSO
  registrar).
- **"Invited as"**: `InvitationScopeWords.Disciplines` of the account's scope names; "—" when it has none (S5).

## § 4: words and constants already built and kept (lanes use them, do not reword)

- `InvitationText.Issued`, `.Resent`, `.ResendRetiresTheLink`, `.CheckAddress` (`InvitationText.cs:41–70`), with "Not
  delivered." losing its full stop (Steps 2.26: "the full stop goes with the split").
- `InvitationRules.ValidateScope`'s sentences (`InvitationRules.cs:35–69` at `69a7df20`): N02's "A college is required for a
  college administrator.", N10's "Speciality administrators must be scoped to a speciality.", N11's "The selected role
  requires speciality and sub-speciality scope.", each now under its field.
- `InvitationRefusals.Describe` (`InvitationRefusedException.cs:81–92`): Register's cards quote "This invitation is
  invalid.", "This invitation has been revoked.", "This invitation has already been used." (G07 rewords it: see lane G),
  "This invitation has expired." (G10 extends it), "A user with this email address already exists.", "This email address
  cannot be used for an account. …".
- `RegisterOutcome.TokenMissingMessage`, `UserNotLoadedMessage`, `ConfirmationMismatchMessage` (G05 adds a sentence after
  it), `GeneralRefusal` (G14 reads it without "Try again.", the button says it).
- `UserAdministrationRules.TraineeNoteOnUserPages` (`UserAdministrationRules.cs:156`; words `:83`) and
  `TraineeAdministrationRules.TraineeNoteOnTraineePages` (`TraineeAdministrationRules.cs:49`; words `:20`): the T290
  notes, asked before any read (M1).
- `DashboardFrame`'s "Could not load your Home." (`DashboardFrame.razor:23`); `StatePanel`'s Try again
  (`StatePanel.razor:16–29`).
- `Invitation.DeliveryOf` (`Invitation.cs:108–117` at `69a7df20`): Being sent for an hour, then Not delivered.
- `TraineeProfile.StageOn(DateOnly programmeStartDate, DateOnly day)` (`TraineeProfile.cs:196`): 365-day blocks.
- `AdmitTraineeCommandHandler.GetDefaultCompletionMonths` (`AdmitTrainee.cs:156`, `internal`): the derived completion's
  months; lane A exposes its result through a query, never by calling it from Razor.

## Across lanes

- **Typed refusals:** each lane writes its own, on the pattern of `MsfRefusedException` / `MsfRefusals`
  (`src/Wombat.Application/Features/MultiSourceFeedback/MsfRefusedException.cs`) and `InvitationRefusedException`:
  derived from `InvalidOperationException` (so nothing that catches a refusal changes and the audit row keeps its message),
  a reason enum, and a `…Refusals.IsRefusal(Exception)`. Lane I: `InvitationIssueRefusedException` (M3); lane A:
  `TraineeAdmissionRefusedException` (S9). No shared base: the two share no reason, and the pattern is two dozen lines.
- **Classes that exist and are reused as given:** `tr.has-note` / `td.history-note` (`app.css:5220–5250`), `.col-wrap`
  (`:363`), `.col-actions` (`:388`), `.actions-cell` (`:442`), `.person-cell` (`:4700`), `.readonly-value` (`:4712`),
  `.choice-row` (`:4676`), `.choice-group` / `.is-invalid`, `.review-type-choice` / `.review-type-help` (`:4824`),
  `.list-section` / `.list-section-title` (`:2577`, `:2581`), `.detail-list` (`:1980`), `.account-steps` (`:771`),
  `.submit-check` (`:1629`), `.required-mark` (`:1556`), `.alert-row`, `.system-panel`, `.dashboard-grid`. Line numbers at
  `69a7df20`.
- **Components used as built:** `ConfirmDialog` (`CancelLabel`, `DangerAction`, `BodyContent`; no new parameter),
  `ActionResult` (`Id`, `FocusOnLoad`, `FocusAfterRender`), `StatePanel` (`LoadError`, `OnRetry`, `FocusAfterRetry`,
  `LoadingContent`), `DataTable` (`Stack`, `StackWide`, `Caption`), `SubmitCheck`
  (`Components/Shared/Activities/SubmitCheck.razor`), `DashboardFrame`, `DashboardCard` (`Icon="hourglass"`, no
  `Emphasis`), `PageHeader` (`Trail`), `FormField` (`Required`), `FormActions`, `PasswordField`, `PasswordRules`, `Alert`,
  `Icon`, `RowNames.Distinct`, `PageFocus.FocusByIdAsync`, `RefusalText.Of`.
- **Dates and times:** ISO dates; a South African day from `ProgrammeCalendar.DateOf(utc)` (`ProgrammeCalendar.cs:23`) or
  `QuotaCalendar.Today(TimeProvider)` (`QuotaProgress.cs:16`); a moment "2026-10-19 09:22 SAST" from `ActivityMoments.When`
  and a date from `ActivityMoments.Date` (`ActivityPageModel.cs:910`, `:914`).
- **Titles** (each tab "{h1} · Wombat"): "Invitations"; "Invite a person"; "Trainees"; "Admit {name}" (M12's T290 note:
  "Admit a registrar"; M08: "Registrar not found"); "Complete registration" and each card's h1 (G07 "Invitation already
  used", G08 "Invitation revoked", G09 "Invitation link not recognised", G10 "Invitation expired", G11 "Account already
  exists", G12 "Address not accepted", G13 "Invitation link incomplete", G14 "Registration not completed", G15
  "Registered, but not signed in", G16 "Invitation timed out", G18 "Signed in as someone else"); "Home".
- **Owners** (Spec 2's owner table): NEW `[typeof(InviteAPerson)] = [new([WombatRoles.Administrator,
  WombatRoles.InstitutionalAdmin], NavItems.Invitations)]` (lane I, added after the `ProgrammeCoverage` row,
  `NavOwners.cs:134`); NEW `[typeof(AdmitRegistrar)] = [new([WombatRoles.InstitutionalAdmin], NavItems.Trainees)]` (lane A,
  added after `TraineeProfileEdit`'s, `NavOwners.cs:155`), the Administrator's trail given explicitly (Home › Trainees ›
  Admit ‹name›, nothing lit). `Register` stays outside the rule (`NavOwners.cs:188`). No menu item changes.
- **No new role check.** Every page keeps its policy: the two new admin pages `AdministratorOrInstitutionalAdmin`, as
  their lists; Register `AllowAnonymous`.
