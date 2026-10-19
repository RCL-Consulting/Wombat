# Flow 11 round 2: the review

Round 2 (the boards saved in `round-2/project/`: 181 boards, 87 at 1280 with the captures board, 85 at 390, 6 at 768,
plus `R2-0-spec`, `R2-0-spec-2`, `R2-0-steps`, `r2.css` and `f11.css`) drew B, W1, R and H at fidelity: the invitations
list (L), Invite a person (N), Register and its cards (G), the pending registrar's Home (P), Trainees (T), Admit ‹name›
(M) and the note for flow 12 (X01). Reviewed 2026-10-19 from three sides, each a Sonnet agent reading the boards' text:
**the code** (`round-2-review-code.md`, K1–K32: `src/` and the ask), **the cast** (`round-2-review-cast.md`, C1–C14: the
runbook, the fixtures and the captures) and **the design system** (`round-2-review-ds.md`, D1–D33: `bundle.css` v25,
DESIGN.md, `app.css`, accessibility, measured in Chromium on a scratch copy). The three source files are kept as they are;
this file merges them. Every must-fix below was checked against the board and the code before it was carried; what did
not hold is said where it was dropped. Severity: M must-fix, S should-fix, N nit. Code is cited at `a4685e56`.

**The captures arrived, by the upload folder.** The captures board (`R2-00-captures-1280`) says round 1 had none of the
eleven; round 2 had all eleven, read from `C:\dev\Wombat\design\upload` with the operator's approval since they did not
arrive as attachments. They show: the two admins' menus, Pending admission's order by surname, the built completion help,
"No pending trainees found.", and today's Register refusal in the card's message slot with the focus on First name. The
cast found every name, address, role, scope, time, date and training year true. What slipped is what no capture shows:
**states that cannot coexist** on Illustration boards (C1), a **premise of the ask itself** that the model forbids (two
adopted versions, F1), and **behaviour the boards assume and the code does not do** (K1, K3–K5).

## Decisions for the operator

The operator's Q1–Q18 (`round-1-assessment.md` § 8) stand as given; nothing below reopens them. F3 and F7 are the build
choices those decisions need; the rest are new.

| | Question | Recommendation | Alternatives |
|---|---|---|---|
| F1 | **May an admission put a registrar in another discipline's curriculum?** The ask drew "two adopted versions" (M05's Curriculum select, P06's Home naming none, P01–P03's "only while exactly one version is adopted"). That cannot exist: one active adoption per institution **per sub-speciality** (unique filtered index, `InstitutionCurriculumAdoptionConfiguration.cs:17–19`; `TraineeAdoptionResolver.cs:32`, checked). What can exist is two **disciplines** adopted: `GetAdmissionCurriculumChoicesQuery` offers every active adoption of the institution (`TraineeCurriculumChoices.cs:84–87`), and `AdmitTrainee` then rewrites the registrar's speciality and sub-speciality to the chosen curriculum's (`AdmitTrainee.cs:129–134`, checked), silently re-scoping someone invited as Paediatrics. | **No.** The Admit page offers the one curriculum adopted for the registrar's **invited** sub-speciality, read out as text; none adopted is NothingAdopted (M04). Home names that curriculum whenever it is adopted. M05 and P06 are dropped; P04's and P06's `molefe@` (C13) and M05's year before a version is chosen (C14) go with them. A registrar with **no invited scope** (an SSO registrar, K12) is the one case offered the institution's adopted curricula, and the admission sets the scope then. | Today's: any adopted discipline, the scope rewritten, the page saying so ("Admitting to ‹curriculum› moves ‹name› to ‹speciality›"); refuse a registrar with no invited scope (T333's, not flow 11's). |
| F2 | **"Resent" on a resent row** (L10, "2026-10-19 10:23 SAST Resent", DATA `ResentOn`). There is no such value: a resend **overwrites** `IssuedOn` (`ResendInvitation.cs:143`, checked), so 10:23 is already the Issued cell and the first issue's 09:22 is gone. The ask names no such word, and 2.26's Actual and the Steps row read the row without it (C5). | **Drop it.** Issued is the current link's issue time, which is what the row's link is; no column, no migration; the Spec's DATA line goes. | A `ResentOn` column (migration, DTO, list) with `IssuedOn` kept as the first issue, and "Resent" added to 2.26's Expect as PROPOSED. |
| F3 | **How Q4's "Invited by … Institutional admin" knows the issuer's role.** The invitation stores `IssuedByUserId` only (`Invitation.cs:31`). Reading the issuer's roles today answers another question: they may since have lost the role, been erased or deactivated, or hold Administrator beside Institutional admin. | **Store the acting role at issue** (`Invitation.IssuedAsRole`, one small migration; nothing is live, no backfill). "Invited by" shows for `InstitutionalAdmin` only, with the name read now (`GetDisplayNamesAsync`); no line for an erased or deactivated issuer. The Spec draws that case. | Read the issuer's roles now (drifts); drop the role from the line and show "Invited by ‹name›" for any non-Administrator issuer (Q4's rule then cannot be kept as written). |
| F4 | **An empty completion whose derived date has passed.** The hint "Left empty, it will be …" holds for any start; the derivation is `start + max(WindowMonths)` (`AdmitTrainee.cs:109`, `:156`, `internal`) and nothing checks the result. Molefe's real start (2023-01-15) left empty gives 2024-01-15: a current registrar saved with a completion nearly three years past. The boards draw the hint only for Ndlovu (future). | **Refuse it.** The hint says so ("Left empty, it would be 2024-01-15, which has passed. Enter the expected completion."), and Admit refuses an empty completion beside the field in that case. One board: Molefe at 2023-01-15 with the completion empty. | Allow it with the warning; derive from today instead of the start. |
| F5 | **G18's "Sign out and continue" cannot continue**: the sign-out endpoint always redirects to Sign in ("You have signed out.", `Program.cs:760–790`, checked; `SignOutOutcome.cs:36`). The token survives in the register cookie, but the person lands on Sign in. | **A fixed return**: the sign-out form posts a `returnTo` the endpoint compares against the one constant `/account/register` (never an open redirect), so the card's button does what it says. It touches flow 02's endpoint: noted as such on the Spec. | Relabel "Sign out", with "then open the link in your invitation again." |
| F6 | **The "Illustration" label** (C6). The ask labels a state the replay never reaches; G10–G17, N13, N14 and T06 are such states and carry none, and the label lives only in `canvas.json` titles: no board's face says it (only R2-00's legend). | **Label each in its title and as a chip on the board's face**, as NEW, DATA and PROPOSED are; the Spec lists which boards are Illustrations. | Titles only, as now (fixed for the eight). |
| F7 | **When a link stops working, and how the pages say it.** It works **through** 2026-11-02: a use is refused when `ExpiresOn < today` (`InvitationRules.cs:142`) and the list keeps `ExpiresOn >= today` (`ListActiveInvitations.cs:61`). "today" is the **UTC** date (`InvitationRules.cs:123`, `DateOnly.FromDateTime(now)`, checked), so the boundary falls at 02:00 SAST, and an invitation issued at 01:00 SAST gets 13 of the reader's days (K28). The register note says "stops working on 2026-11-02", the mail "expires on 2026-11-02" (K18, C11): two different last days to one reader. Ask PROPOSED (d) counts Expired's 30 days on the South African calendar, which the code does not do. | **The South African calendar, and one sentence.** Expiry judged on `ProgrammeCalendar`'s date (the `TimeProvider`), so a last day is a day in Johannesburg; every sentence says "works until 2026-11-02" (Register's note, the OneTimeLink, the mail, which K19 rewrites anyway), and the column reads "Last day". | Keep the UTC date and the mail's "expires on 2026-11-02", the note reworded to match, the 02:00 edge on the Spec. |
| F8 | **The wait card's stripe** (D15). Q4 took the card "as H1 drew it", an emphasis card; DESIGN.md gives `.detail-card--emphasis`'s blue the meaning "the viewer's move" (`bundle.css:2105`), and a pending registrar has none: the move is the admin's. Flow 10's F7 settled the same rule for the MSF status card. | **No emphasis**: `DashboardCard Title="Awaiting admission" Icon="hourglass"` with the control edge, inside Home's `DashboardFrame` (which gives P its loading and load-error states, S14). | Emphasis as drawn (Q4's letter); a warning edge. |
| F9 | **"(optional)" beside a field** (D6). With the required mark added (M11), Speciality, Sub-speciality and Expected completion also say "(optional)" (`.inv-optional`); flow 10 kept that convention only for the respondent's comment and dropped it on its create page. | **Drop "(optional)"**: one convention, the star and DESIGN.md's explaining line; the field's help says what leaving it empty does (Expected completion's built help already does). | Keep both marks; mark optional fields only. |
| F10 | **A failed registration and "Nothing was saved."** (K11). `UserManager.CreateAsync` commits the account (`InvitedUserProvisioner.cs:81`), then the role (`:96`), then the scopes (`:120`), and the invitation is marked used after; a fault between leaves an account, maybe with no role, and an unused invitation, so the retry meets G11 ("already exists"). The Identity store shares `ApplicationDbContext` (`DependencyInjection.cs:71`), so one transaction is possible. | **One transaction** around the provisioning and the accept (folded into the build, Q14's way); G14 then keeps its sentence. | Reword G14 ("Registration could not be completed. Try again. If it fails again, tell the person who invited you.") and leave the code; or both. |

Settled from the evidence (no decision needed):

- **The Waiting list sorts by surname** (K31 dropped): the handler adds no order, but `ListUsersInRoleAsync` sorts by
  `LastName`, `FirstName` (`UserAdministrationService.cs:58–70`, checked), as the cast and capture 2.28-4 say.
- **Register carries no required mark** (D6 applies to Invite and Admit only): § 4's corrections, taken under Q6, say "no
  '*'" on Register, FormField's account-form exception.
- **Refusals under their field** (D16 (a), D20) are the operator's (Q3, Q6; § 4 for Register). They stand; the Spec writes
  the DESIGN.md change to AuthLayout's one-slot rule and the single-field exception to RefusalSummary (S15).
- **"Being sent" for an hour, then "Not delivered"** is `Invitation.DeliveryOf` (`Invitation.cs:108–117`, checked): C1 rests
  on it, and C1 of the ask holds on every step board.
- **The tables stack at 900 on the boards** (D's measurements, 0 overflow from 900 down), but the built `DataTable`'s
  `Stack` stacks below 641 only (`DataTable.razor:42–44`, checked): the boards hand-wrote `clinic-table--stack-wide`. The
  build adds `StackWide` (S16).
- **The dialog and header 44 px rules exist only scoped to flows 08 and 10** (`app.css:4726`, `:5445–5450`, checked): flow
  11 is the third to need them (M8, M9).
- **The ask's PROPOSED (a)–(f)**: none contradicted except (d)'s calendar (F7); (e)'s "—" for an SSO registrar agreed by
  K12; (c)'s gap (a registrar who already holds an account cannot be invited as a trainee) stays open for the operator, as
  round 2's ask left it.
- **What holds** (all three reviews): every person, address (`ndlvou@` only at 2.16, 2.17 and its revoked link), role
  label, scope string, institution and College; the times (09:19, 09:22, 10:23 SAST), dates, training years 4, 3, 2, 1, 1
  (and 5 at 2022-01-15) against `TraineeProfile.StageOn`; the counts in every caption; the 29 steps; no live link, token
  or password on any board; the built words quoted as built (`InvitationText.cs`, `InvitationRules.cs:35–69`,
  `RegisterOutcome.cs`, NothingAdopted, the T290 notes); the Administrator's eight radios and the Institutional admin's
  seven; the pickers' orders; the menus, trails, lit items and "Error: " tab titles; no raw colour, no new token, the
  contrast figures recompute; one h1, no skipped level, no duplicate id, no dangling ARIA reference; the 1280/768/390
  twins carry the same words.

**Data the build must add** (from K's list, restated by query; observed unless marked):

| Board shows | Gap | Size |
|---|---|---|
| Register's "as Coordinator at Kgosi Kgari Teaching Hospital" and its last day | `InvitationPreviewDto`: institution or College name, speciality and sub-speciality names, `ExpiresOn` | S |
| Q10's and Q12's refusals, in scope and out | `IssueInvitation`: a find-by-address on `IUserAdministrationService` (id, name, `InstitutionId`); an unfiltered active-invitation lookup; each judged against the caller; a typed refusal (M3) | M |
| "Expired, not used" (Q9) and Invite again | `ListExpiredUnusedInvitationsQuery`, the institution filter, minus addresses re-invited or holding an account; a prefill read in scope | M |
| Not delivered first; SAST times | in-memory order after `DeliveryOf`; `ActivityMoments.When` (S3) | S |
| "Invited by … Institutional admin" | `Invitation.IssuedAsRole` (F3), one migration | S |
| The wait card | `GetTraineeDashboardSummary` returns early for a pending trainee (`:59–64`): a new part with institution, scope names, the curriculum for her sub-speciality (F1), the used invitation by address (S4): issuer, role, `UsedOn` | M |
| Waiting list: Invited as, Registered | `PendingTraineeDto`: scope **names** (it carries ids), Registered from the used invitation's `UsedOn` on the SA calendar, deactivated excluded (S6) | S |
| Current trainees: Programme start, Training year | `TraineeProfileDto.TrainingYear` from `StageOn` (a Domain static; the `.razor` may not call it) | S |
| Admit ‹name›: the registrar's facts, the training year and derived completion on the date's change | `GetAdmissionCandidateQuery` (one not-found for not pending, out of scope, no such id) and a read for a start's year and derived completion (the month count is `internal`) | M |
| The admission's mail (Q7) and result | `AccountNotices` after `PromotePendingTraineeAsync` (`AdmitTrainee.cs:136`); the result carrier (S8) | S |
| Copy link | `wombat.js` clipboard call and its failure line (S10); `clearInvitationTokenFromUrl` removed | S |
| The token GET and the cookie | middleware or a distinct path (S2); the cookie carries token, codes and names (S1) | S |
| "Resent" | nothing, if F2 drops it | none |
| Q8's count | the same pending list, counted | none |

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | code (K1) | L17 and T07 draw the T290 note **and** the page under it (L17 "No active invitations"; T07 five registrars with Admit…). As built, a Trainee-holding admin gets the note and nothing else, and the handlers refuse before reading (`InvitationsList.razor:14–18`, checked; `PendingTraineesList.razor:12–16`; `ListActiveInvitations.cs:28`, `AdmitTrainee.cs:56`) | L17 and T07: the header and the note only (no Invite a person, no tables, no Admit). The two new pages ask `TraineeNoteOnUserPages` / `TraineeNoteOnTraineePages` before any read; a board each, or the Spec says they share L17's and T07's |
| M2 | code (K2), cast (C13, C14) | P01–P03's rule "on the Paediatric EPA Curriculum (11.1) only while exactly one version is adopted" never fires: two versions of one discipline cannot be adopted (F1). M05 and P06 draw that impossible state, with Molefe's address (C13) and a training year before a version is chosen (C14) | Home names the curriculum adopted for her invited sub-speciality, none when none is; the Admit page reads it out as text. M05 and P06 dropped, or redrawn as F1 decides |
| M3 | code (K3) | Q10's and Q12's refusals, built as today's `InvalidOperationException(scopeError)` (`IssueInvitation.cs:84`, `:90`), put their message into the caller's audit row (`AuditPipelineBehavior.cs:104`, `errorMessage: ex.Message`, checked) and onto the page (`RefusalText.cs:42`). An in-scope refusal naming Pieter Smit writes his name into the row; an out-of-scope one would carry whatever its message holds into a log the Institutional admin can search. Q12 also needs the holder's id for the Users link, which a string cannot carry | A typed refusal beside `InvitationRefusedException`: reason, field, holder id and name **only when judged in scope**; the exception's message fixed generic words in every case, the page mapping the reason to words and field. Test: an Institutional admin of A refused over B's or a College's address leaves nothing of B in A's audit row |
| M4 | code (K4) | Q11 ("it never registers over a live session") is drawn as G18's card, but the decision is the **POST**'s: `/account/register/submit` is `AllowAnonymous`, never looks at who is signed in, creates the account and calls `SignInAsync` over the session (`Program.cs:343–397`, checked). A stale tab registers and swaps identities | The endpoint refuses a signed-in caller (redirect to G18's card, nothing accepted); the Spec says so; a test posts a form under a signed-in cookie |
| M5 | code (K5) | N10, N11 and L07 (2.6, 2.7, 2.17) require no registration link on screen after a refusal or a revoke. Today `IssueAsync` and `RevokeAsync` never clear `_lastIssuedRegistrationUrl`; only the resend does (`InvitationsList.razor:388`, checked) | A build rule on the Spec: the link clears at the start of every action, refusal or navigation, on whichever page holds it (Invite a person's reset form too), with a test per action (Q2) |
| M6 | design (D1) | The invitations table scrolls sideways at 1280 and at every width from 901 to about 1280 (measured: L13 at 1280 +40 px, L12 +6, L04 at 1100 +65, at 901 +264; L13 at 901 +419); Current trainees (T03) +68 at 1000, +167 at 901. Cause: `.inv-when`, `.delivery`, `.inv-actions` `nowrap`, `.inv-actions` `width:1%`, Invitee `min-width:11rem`, Issued on one line (the ask's "never wrapping") | Issued and Expires wrap date over time (`.inv-when span{display:block}`, defined, unused), Delivery `.col-wrap`, actions `.col-actions` with wrapping buttons, `width:1%`/`nowrap` dropped; re-measured at 1280 with L13's longest values, at 1100 and 901; or the stack-wide breakpoint raised for these tables and the Spec says so. The same for `.tr-act`. The ask's "never wrapping" for Issued gives way |
| M7 | design (D2) | N21 at 390: the result's `<strong>Invitation issued for ‹64 characters›@kgk.wombat.local.</strong>` pushes the page 220 px sideways (595 px in a 375 px viewport); `.otl-result` has no `overflow-wrap`, no `<wbr>` after the @ | `overflow-wrap:anywhere` on the result and `<wbr>` after the @ in every sentence naming an address (result, N19/N20 notices, N15–N18 refusals); a 64-character row on a list board |
| M8 | design (D3) | L06 at 390: the revoke dialog's buttons are 36 px ("Keep the invitation" 158×36). No shipped rule raises a `ConfirmDialog` action-mode button below 641 px; `r2.css`'s phone rules are scoped to wrappers the top-layer dialog is not in (BRIEF § 11 "Flow 09/10", the third time) | A general rule in `app.css`: `.dialog-card .form-actions .btn{min-height:2.75rem}` below 641 px, the actions stacked `column-reverse` (safe button last on screen, first in tab order); flows 08's and 10's scoped copies fold into it. The Spec lists it in ConfirmDialog's DESIGN.md text |
| M9 | design (D4) | The header's "Invite a person" (154×36) on every list board at 390, and M04's "Back to trainees" (139×36). The Spec's "44 px at 390" list omits them; `.header-container .btn` is raised only under `.committee-calendar` and `.msf-page` (`app.css:4726`, checked) | A general `.header-container .actions-cell .btn` rule below 641 px (or the page's wrapper class, named on the Spec, reaching the header) and `.inv-page .btn` for stand-alone buttons |
| M10 | design (D5) | L16, M11, N23 and T09 draw the load error as the empty-state card (`div.detail-card--empty[role=alert]`, a full-width outline Try again, inline styles at 390). `StatePanel` with `OnRetry` renders a danger Alert row (bold first sentence, "Nothing has changed. Try again, or come back in a few minutes.", an outline `.btn-sm` Try again with `refresh-cw`, inside `ActionResult`): the boards show a state that does not ship | Redraw the four as StatePanel's Alert in C13's words; the focus on the alert after a failed retry; the 44 px from the system's phone rule, no inline style |
| M11 | design (D6) | Invite a person and Admit carry no required mark: Email, Role, the College or Institution, the scope the chosen role requires and Programme start have none, Role's radios a bare `required` under `novalidate`. DESIGN.md § Form system (:1329) | `<span class="required-mark" aria-hidden="true"> *</span>` and `aria-required="true"` on each; the hidden "required" on Role's legend only; "* marks a field you must fill in." above each form's first field; "(optional)" as F9 |

## Should fix

- **S1 The refused registration's names** (K6; G04–G06, G14, Spec C5): the Spec says the redirect carries neither token
  nor codes, and never where the **names** go; today the query carries token and codes and drops the names
  (`RegisterOutcome.cs:101–114`). The same Data Protection cookie holds token, codes and the two names for its 30
  minutes, deleted on success; no password stored; a test that no refused redirect URL holds a name, code or token.
- **S2 The token's GET and the page share a path** (K7; G01–G03, Spec): a `MapGet` on `/account/register` is ambiguous with
  the routable `Register` (`Register.razor:1`, checked), and the mail's link has that shape. Middleware ahead of routing,
  or a distinct path (`/account/register/start?token=`; nothing is live). `PageRenderMode` returns null for
  `/account/register` whoever is signed in (Q11's build note).
- **S3 Times are the server's zone** (K9; L03–L13, N06's row names): `IssuedOn.ToLocalTime()` with no suffix
  (`InvitationsList.razor:136`, `:162`, checked); on the UTC box the staff would read 07:19. `ActivityMoments.When` for
  cells and row names, `ActivityMoments.Date` for Registered and Expired's dates.
- **S4 One address, two spellings** (K10; N17, N20, Spec C3, P01's Invited by, L14): `Email` is stored as typed (trimmed,
  `IssueInvitation.cs:104`) and compared as stored (`AcceptInvitation.cs:105`, case-sensitive in Postgres; checked). Compare
  a normalised value everywhere (Q10's check, C3's sweep, the used-invitation reads, Expired's leaving rule), with a
  case-variant test each.
- **S5 An SSO registrar has no programme** (K12; P05, T01): no group matched means no scope (`ExternalLoginHandler.cs:778–793`,
  `SsoGroupMapper.cs:174–199`). P05 draws no Programme line; Invited as reads "—"; F1's no-scope case.
- **S6 A deactivated pending account** (K15; T01–T06, M01): listed with Admit… (`ListPendingTrainees.cs:24–34`), admitted
  (`AdmitTrainee.cs` has no `IsDeactivated` check, checked), and Q7's mail silently skipped. Excluded from the list,
  refused by Admit in words, a row in the M refusals.
- **S7 G13 and G16 are told apart only by the method** (K17): a GET with no cookie and no token is G13 whether a cookie
  lapsed or not; G16 is a POST without the cookie. The Spec says which request draws which, or one card fits both.
- **S8 The admission's result across the navigation** (K21; T02–T04, 2.29): today Admit lands on the profile
  (`TraineeProfileEdit.razor:326`). The sentence carries a name, curriculum and date, never the URL: `?admitted=‹profileId›`
  re-read in scope (recommended: no new cookie) or the `ActingRoleSwitchResults` pattern. The Spec names it.
- **S9 Load errors and refusals told apart** (K22; L16, M11, T09, N23, T329): Trainees and the profile page print
  `exception.Message` for both (`PendingTraineesList.razor:165`, `TraineeProfileEdit.razor:269, :353, :431`), and the
  admit handlers throw plain `InvalidOperationException` for refusal and fault alike. A typed refusal for trainee
  administration (as M3's for invitations); everything else logged and shown as M10's words.
- **S10 The OneTimeLink is a second result component with no focus ring and no failure** (D11, K29; N03, N06, N09, N21,
  L10): `.otl` takes the focus but nothing styles `:focus-visible` (the boards ring it inline); DESIGN.md holds the link in
  an Alert inside `ActionResult`; `navigator.clipboard` can refuse, and the link is shown once. `ActionResult` around a
  success Alert and the link's row; the field's value the real link (selectable); "Could not copy. Select the link and
  copy it yourself." in the same live region; today's `<code style="word-break:break-all">`
  (`InvitationsList.razor:45`, checked) replaced by a class.
- **S11 Results in the shipped wrapper** (D12; T02–T04, L07, L10): `.result-block` duplicates `ActionResult`;
  `.inv-page .action-result .alert{margin:0}` and `ActionResult` in the markup.
- **S12 The invitation mail says the role's key** (K19): "as InstitutionalAdmin" (`InvitationEmail.cs:19`, `:26`, checked)
  against the page's "Institutional admin". `WombatRoleLabels.For(role)` and the scope words; the mail in the Spec's mail
  section beside Q7's, in the changed-words list, with F7's "works until".
- **S13 Invite a person with nothing adopted** (K20; N08, N12): an Institutional admin's Speciality lists only adopted
  specialities (`GetSpecialitiesListQueryHandler.cs:32–42`); with none, four roles cannot be issued and the form says
  nothing. A board: the hint under Speciality with the Curriculum adoptions link, Issue available for roles needing none.
- **S14 Missing boards and states** (D15, D19, C8, F4): P's loading and load error (DashboardFrame's, F8); Issue with no Role
  chosen ("Choose a role." under the legend, the first radio focused, `.choice-group.is-invalid`); N08 before a speciality
  is chosen ("Then choose its sub-speciality."), or 2.4's Expect drops the hint; F4's past-completion board.
- **S15 The auth card's changed rules, written down** (D16, D20, D25; G04–G06, N02, N10–N18, the Spec): the Spec's DESIGN.md
  text lists what Q6 and Q3 change: AuthLayout's one message slot becomes a message per field, the focus on the field;
  PasswordRules' refusal (G04 prints "The new password needs: …" as the field's message and describes the field by both it
  and the list); the single-field refusal with no RefusalSummary. G05: the focus stays on Password (both fields emptied,
  C6's "Type the password again in both fields."), and Password is described by the confirmation's message too, so it is
  read on focus. D25's (a): "never disabled" narrowed to the Invite form (Schedule a review and the MSF template keep
  theirs); (b) Resend's `aria-disabled` and link Alert (:2169–2172) in the replaced range; (c) cite `.readonly-value`; (e)
  drop the restatements of the Button system and ConfirmDialog.
- **S16 Stacked tables as built** (K24, D24, D29; L04–L14, T01–T05): `DataTable` gains `StackWide` (stack below 900) and
  emits the explicit roles it already gives for `Stack`; the note row needs two `<tr>` per item from the `Row` fragment;
  row buttons 44 px from 900 down, as flows 08 and 10.
- **S17 Radios named by their label alone** (D7, D8; N01–N23): the description outside the `<label>`, kept in
  `aria-describedby`, as flow 08's `.form-check.review-type-choice` with `.review-type-help`; `fieldset.form-group.choice-row`
  in place of the five `.inv-radio*` classes and the hand-sized legend.
- **S18 Read-outs and the Training year** (D9, D23; M, N, P): a single value as `dl.form-group` with `dd.readonly-value`
  (`.field-label` and `.inv-readback` go); Training year an `<output for="start">` holding the value, its help outside, a
  real label.
- **S19 CSS reuse and the Spec's NEW table** (D10, D13, D14, D26): `tr.has-note`/`td.history-note` (with a
  `.history-note--warning` modifier if the tint stays) for `tr.inv-has-note`/`tr.inv-note`; `SubmitCheck` for
  `.consequence`; `.list-section`/`.list-section-title` for `.inv-section*`; `.detail-list` for `.admit-facts`/
  `.wait-facts`; `.account-steps` for `.wait-steps`; `.person-cell` for `.inv-role`/`.inv-scope`; every class a row ("NEW,
  because …" or "reuses `.x`"); the dead (`.reg-card-icon`, `.ty-value--wait`) and redundant rules (the auth card's three
  phone rules, the date-input font) deleted.
- **S20 Inline styles and the submit's place** (D17; G01–G07, G11, G14, G15, G17, G18, L16, M08): no `width:100%` or
  inline rings; Register's submit in `FormActions` as SignInCard's, or the Spec states the change; M08 the system's
  not-found card.
- **S21 G06's 101-character name cannot be typed** (D18): `maxlength="100"` stops it, and it is built so today
  (`Register.razor:50`, `:56`, checked), against DESIGN.md's "no `maxlength`". Drop it; the refusal names its field ("Last
  name: enter 100 characters or fewer.").
- **S22 Admit's refusals in the system's voice** (D21, D22; M04, M08, M09, L15, M10, N22, T08): M09 "**Not admitted.**
  Lerato Molefe already has an active trainee profile. …"; M04's standing warning `Role=""`; the loading lines name what
  loads ("Loading the invitations.", "Loading this registrar.", "Loading what you can invite.", "Loading the trainees.").
- **S23 The heavy set's impossible states** (C1; L13 at every width): with `duplessis@` and `ndlvou@` Not delivered the
  reading time is at least 10:22, so no 09:19 or 09:22 row is still Being sent (`Invitation.cs:108–117`). Each row as it
  would read then, or every time ‹time›; Being sent only within the hour.
- **S24 The Steps board** (C2, C3, C4, C7, C8 (focus), C10; K25/C9 as a nit): 1.10 and 2.44 say Invite a person opens the new
  page (PROPOSED, Q1; X01), not "unchanged"; 2.28–2.30 quote today's words ("Pending admission", "Active profiles", "Admit
  to curriculum: ‹name›", "No active trainee profiles found.", "Trainee profile" / "Admit a pending trainee into a
  curriculum.", the landing on `?id=` with Deactivate and Mark complete); the scope phrase's rule once on the Spec ("in
  ‹speciality›[ / ‹sub-speciality›]" only when the invitation has one, then "at ‹institution›"), or one board for a
  one-scope role; whether "Invite the next person" is a press (40 + 9, or 48 + 9); 2.9's row names G05's focus; 2.26's
  row says the full stop goes with the split.

## Nits

- The Steps board's last line: "Changed: 25 steps", 2.19 being changed (its card names `mahlangu@`) (K25, C9).
- "Whole years from the programme start": `StageOn` counts 365-day blocks (`TraineeProfile.cs:196–205`, checked): "Training
  years turn every 365 days from the programme start." (K23).
- T06: the Administrator's Current trainees gains an Institution column, as Waiting has (K26).
- M04: a second sentence for a registrar with no institution, which an Administrator can meet: "This registrar belongs
  to no institution, so no curriculum can be offered." (NEW) (K27).
- The Spec's owner table: `AdmitTrainee` under Trainees for the Institutional admin, and an explicit `Trail` crumb to
  `/admin/trainees` for the Administrator, whose menu has no Trainees (`NavOwners.cs:155`, `NavItems.cs:146`) (K30).
- C5's cookie: `Secure = Request.IsHttps`, as `ActingRoleSwitchResults.cs:170`, so the http replay on :5180 registers (K32).
- N14's malformed `ndlovu@kgk` sits beside the cast's real typo: `‹a malformed address›` or `name@example` (C12).
- N15: one link, "open Pieter Smit under Users" (D27).
- Inline links at 390 are 19–21 px: noted on the Spec as the accepted inline exception, as flow 10 did (D28).
- One word for leaving a form: "Cancel" on Invite a person too (D30).
- T04's "‹Flow 13's list, unchanged.›" and X01's "‹Flow 12's card›" as annotation pills, not page content (D31).
- P01–P06: `arrow-right` Icon or no arrow, not "→" (D32).
- The form's `aria-label` duplicates the h1: drop it (D33).
- Save the canvas's `ds/` (tokens.css, bundle.css) with the round: neither K nor D could render from the folder (K Limits,
  D Method).

## What round 3's ask must do

- **Restate F1–F10 as decided**, beside Q1–Q18, and draw what each decides (F1: M05 and P06 go, the Admit curriculum read
  out; F4: one board; F5: G18's button; F6: chips; F7: "works until", "Last day").
- **Fix M1–M11 and S1–S24 board by board**, citing the id; redraw L17, T07, P01–P03, L13 (C1), L16/M11/N23/T09, L06-390,
  every 390 list board's header, N21-390, and every Invite and Admit form state with the required mark.
- **Re-measure every table** at 1280, 1100, 1000, 901, 900, 768 and 390 with L13's longest values, and say on the Spec
  that nothing scrolls sideways; every 390 control 44 px, the dialog's and the header's included.
- **Add the missing boards**: P loading and load error; no Role chosen; N08 before a speciality; nothing adopted on Invite
  a person (S13); a deactivated pending account refused (S6); a copy failure (S10); the past derived completion (F4); the
  two new pages' T290 notes (M1, or say they share L17's and T07's).
- **The Spec**: the build rules (M3's typed refusals, M4's endpoint, M5's clearing, S1's cookie and names, S2's route,
  S7's GET/POST cards, S8's result carrier, S9); the DESIGN.md text S15 lists, the general dialog and header rules
  (M8, M9), `StackWide` (S16); the DATA table above in place of its own, with F2's line dropped and F3's column added;
  every class a NEW-table row (S19); which boards are Illustrations.
- **The Steps board** as S24, with the journey counts recomputed after M6 and S24's "Invite the next person".
- **Keep** every fixture, word and order the reviews found true; invent nothing (C12, C13); attach the eleven captures
  again, and say if they did not arrive.

## Who raised what

| Here | Code review | Cast review | Design review |
|---|---|---|---|
| M1 | K1 | | |
| M2 | K2 | C13, C14 | |
| M3 | K3 | | |
| M4 | K4 | | |
| M5 | K5 | | |
| M6 | | | D1 |
| M7 | | | D2 |
| M8 | | | D3 |
| M9 | | | D4 |
| M10 | K22 (in part) | | D5 |
| M11 | | | D6 |
| F1 | K2, Q1 | C13, C14 | |
| F2 | K8, Q2 | C5 | |
| F3 | K13, Q3 | | |
| F4 | K14, Q4 | | |
| F5 | K16, Q5 | | |
| F6 | | C6 | |
| F7 | K18, K28 | C11 | |
| F8 | | | D15 |
| F9 | | | D6 |
| F10 | K11 | | |
| S1–S9 | K6, K7, K9, K10, K12, K15, K17, K21, K22 | | |
| S10, S11 | K29 | | D11, D12 |
| S12, S13 | K19, K20 | | |
| S14 | K14 | C8 | D15, D19 |
| S15 | | C8 | D16, D20, D25 |
| S16 | K24 | | D24, D29 |
| S17–S22 | | | D7–D10, D13, D14, D17, D18, D21–D23, D26 |
| S23 | | C1 | |
| S24 | K25 | C2–C4, C7–C10 | |
| Nits | K23, K26, K27, K30, K32 | C12 | D27, D28, D30–D33 |
| Dropped | K31 (the service sorts by surname) | | |
