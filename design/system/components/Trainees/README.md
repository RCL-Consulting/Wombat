# Trainees

Trainees (`/admin/trainees`, `Pages/Admin/Trainees/PendingTraineesList.razor`; its words in `AdmissionText.cs`, its addresses in `Shared/OnboardingLinks.cs`): **the registrars waiting to be admitted, then the current trainees, then the programmes that have ended** (Completed and closed, flow 13). Each waiting row opens Admit ‹name› (AdmitRegistrar), and an admission lands back here saying what was done. Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-T01 to T10; round 3 S8, S16, M6, M7, check rules 6 and 7; T280, T306, T329; build review P2, P4, P9) rebuilt it. Until then its two tables sat in cards headed by `h3`s ("Pending admission", "Active profiles") whose captions repeated them, a waiting row's button was a primary "Admit to curriculum" that opened the trainee profile page, an admission landed on that profile saying nothing, Current trainees showed no training year (T306), and a failed read printed the exception's text (T329).

## Who reads it

Administrator and Institutional admin (`AdministratorOrInstitutionalAdmin`), each in their scope. The Institutional admin's menu offers it as "Trainees" (`user-round`, under People), which owns Admit ‹name› and a trainee's profile; the Administrator's menu has no Trainees (NavMenu). Someone who also holds Trainee admits, changes and ends no programme, their own included (T290): the page reads nothing for them and shows one info alert, `#trainees-trainee-note`, "You hold the Trainee role, so you cannot view or change trainee profiles, including your own." (M1, T07).

## The page

The header: "Trainees", "Admit registrars who are waiting, and keep current trainees' programmes." Then the column, `.inv-page` (its sections 24px apart):

- **Waiting to be admitted** (`section.list-section`): `h2#trainees-waiting-heading.list-section-title` (focusable by script) with `span.inv-count` "5 waiting". Then, after an admission, **the result** (S8; below). Then a `DataTable` with `StackWide` (S16), captioned "Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted." (the Administrator's: "… from a Trainee invitation at any institution, by surname. …").

| Column | What |
|---|---|
| Name | **the row's header**: "Lerato Molefe" |
| Email | `.col-wrap`: the address with `<wbr>` after its @ (M7) |
| Institution | the Administrator's only (check rule 6) |
| Invited as | the used invitation's disciplines, "Paediatrics / Paediatrics" (`InvitationScopeWords.Disciplines`); "—" with none (S5: an SSO-provisioned registrar) |
| Registered | the used invitation's day on the South African calendar, "2026-10-20"; "—" with none |
| (Actions) | `.col-actions.col-actions--single` (`width: auto`, M6): **Admit…**, `.btn .btn-outline .btn-sm`, a link to `OnboardingLinks.Admit(userId)`, `#admit-{userId}`, named "Admit Lerato Molefe" (the email added where two share a name; T239). The ellipsis says a page follows |

- **Current trainees**: `h2` with "4 current", a second `StackWide` table captioned "Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.":

| Column | What |
|---|---|
| Name | **the row's header**; the Administrator's "Name and institution", the institution under the name (`.person-cell`, check rule 6) |
| Curriculum and sub-speciality | `.person-cell`: "Paediatric EPA Curriculum (11.1)" over "Paediatrics"; its header `.col-wrap.col-wrap--curriculum`, floored at 7.5rem by the onboarding column's `th.col-wrap` rule (its own 7rem rule loses to it; Known gaps), so "Curriculum" and "Paediatric" never split from 901 to 1100px |
| Programme start | `.inv-when`: "2023-01-15" over "Training year 4" (T306's list half) |
| Expected completion | "2027-01-14" |
| (Actions) | `.col-actions--single`: Edit, named "Edit Lerato Molefe's profile", to the trainee profile page |

- **Completed and closed** (flow 13, T379, 2026-10-27, 5cc10c66; S11, S22, S24, F7, Q22; boards R3-C01 to C07): every ended programme in the caller's scope, read on its own (`ListEndedProfilesQuery`, paged in SQL, 20 a page), the latest end first. Always drawn: `h2#trainees-closed-heading.list-section-title` (focusable by script) with `span.inv-count` "3 ended" (the count in scope, not the page's). Then, in `.closed-list`, a `StackWide` table captioned "Programmes that have ended, the latest end first. An ended record is read-only.":

| Column | What |
|---|---|
| Name | **the row's header**, no `.col-wrap` (`overflow-wrap: break-word`, so a long name breaks inside itself only when it must); the Administrator's "Name and institution", the institution under the name (`.person-cell`); the address under it where two share a name (C05). An erased person is listed under the pseudonym ("deleted_user_0ae1d64b") |
| Curriculum and sub-speciality | `.col-wrap.col-wrap--curriculum`: `.person-cell`, "Paediatric EPA Curriculum (11.1)" over "Paediatrics" |
| Programme | `.col-wrap`: `.person-cell`, "2023-01-15 to 2026-10-27" over "Training year 4 when it ended" (the start alone for an end with no day recorded); on the onboarding column's 7.5rem floor, so "2023-01-15" stays whole from 901 to 1100px |
| Outcome | `.outcome-cell`: the badge, **Completed** green or **Withdrawn** grey (`BadgeFor.ProgrammeOutcome`, `badge-withdrawn`; Badge), then the day, which never breaks |
| (Actions) | `.col-actions--single`: **View**, `.btn-outline .btn-sm`, named "View Lerato Molefe's record" (the address, then the curriculum, added where names collide), to the profile (TraineeProfile) |

  The pager (`PagerControls`, "Completed and closed, pages", "Ended programmes per page") only past one page; a turn focuses the section's h2. None ended: the empty card "No programme has ended yet." · "A trainee is listed here once their programme is marked complete or a withdrawal is recorded, on their programme's page." Until flow 13 it was Current's read filtered, unpaged, with an Edit.

## The result of an admission (S8; T02–T04)

Admit ‹name› goes to `/admin/trainees?admitted=‹profileId›`: the address carries the id alone. The page reads that profile again in the caller's scope and says, under Waiting's heading and directly above its table, in an `ActionResult` (`#trainees-admitted`) **focused as it loads**, a success Alert, `role="status"`, its sentence bold: "**Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15.**"; for a completion left empty, the first sentence bold and the derived date plain after it (`AdmissionText.AdmittedDerived`), "**Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15.** Expected completion 2027-01-15, derived from the curriculum window: change it under Edit."; before the programme starts, "… from ‹start›, when the programme starts." The page reads again on every navigation (P4), so the result never outlives its address. **Back from Admit** (`?back=‹userId›`, T10): Cancel on Admit lands here with the focus on that registrar's Admit….

## States (T08, T09, T329)

- **Nobody waiting**: "Nobody is waiting to be admitted." · "A registrar appears here after registering from a Trainee invitation." (the empty card). **No current trainees**: "No current trainees" · "A registrar appears here once admitted."
- **Loading**: `p.loading-line` "Loading the trainees." over a skeleton card.
- **Load failed**: "Could not load the trainees. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), never the exception's text; a Try again that loads hands the focus to Waiting's heading.

## Narrow widths

All three tables stack at 900px and below (`StackWide`): each row a block, the name heading its labelled cells; Admit…, Edit and View are 44px from 900px (`.clinic-table--stack-wide .btn`). Measured on the replay at 1100, 1000, 901 and 390: no sideways scroll and no word split mid-word (Step 2.29; flow 13's closed list in T379's replay and again at 1280 and 390 in its step 8).

## Known gaps

- **Found re-syncing, not filed:** the onboarding column's floor for a wrapping header (`.inv-page .clinic-table th.col-wrap`, 7.5rem, specificity 0,3,1), written for Invitations' Delivery, also reaches Waiting's Email and Current trainees' Curriculum and sub-speciality, both `.col-wrap`; it beats `.clinic-table th.col-wrap--curriculum` (7rem, 0,2,1), which is dead on this page, its one user.

## Contrast

No new colour token. The counts and the second lines (`.inv-count`, `.inv-when span`, `.person-cell`'s) `muted-text` 5.09:1 on the surface (4.83:1 for the count on the page's ground); the row header 11.36:1 on `header-bg`; the outline buttons 4.86:1.
