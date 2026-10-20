# Trainees

Trainees (`/admin/trainees`, `Pages/Admin/Trainees/PendingTraineesList.razor`; its words in `AdmissionText.cs`, its addresses in `Shared/OnboardingLinks.cs`): **the registrars waiting to be admitted, then the current trainees**, then flow 13's Completed and closed. Each waiting row opens Admit ‹name› (AdmitRegistrar), and an admission lands back here saying what was done. Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-T01 to T10; round 3 S8, S16, M6, M7, check rules 6 and 7; T280, T306, T329; build review P2, P4, P9) rebuilt it. Until then its two tables sat in cards headed by `h3`s ("Pending admission", "Active profiles") whose captions repeated them, a waiting row's button was a primary "Admit to curriculum" that opened the trainee profile page, an admission landed on that profile saying nothing, Current trainees showed no training year (T306), and a failed read printed the exception's text (T329).

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
| Curriculum and sub-speciality | `.person-cell`: "Paediatric EPA Curriculum (11.1)" over "Paediatrics"; its header `.col-wrap.col-wrap--curriculum`, floored at 7rem, so "Curriculum" and "Paediatric" never split from 901 to 1100px |
| Programme start | `.inv-when`: "2023-01-15" over "Training year 4" (T306's list half) |
| Expected completion | "2027-01-14" |
| (Actions) | `.col-actions--single`: Edit, named "Edit Lerato Molefe's profile", to the trainee profile page |

- **Completed and closed** (flow 13's, as built; only when it has rows): `h2` "Completed and closed" over a plain `DataTable`, its columns Name, Curriculum, Sub-speciality, Outcome ("Completed 2026-…", "Withdrawn 2026-…") and Edit. Flow 13 redraws it.

## The result of an admission (S8; T02–T04)

Admit ‹name› goes to `/admin/trainees?admitted=‹profileId›`: the address carries the id alone. The page reads that profile again in the caller's scope and says, under Waiting's heading and directly above its table, in an `ActionResult` (`#trainees-admitted`) **focused as it loads**, a success Alert, `role="status"`, its sentence bold: "**Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15.**"; for a completion left empty, the first sentence bold and the derived date plain after it (`AdmissionText.AdmittedDerived`), "**Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15.** Expected completion 2027-01-15, derived from the curriculum window: change it under Edit."; before the programme starts, "… from ‹start›, when the programme starts." The page reads again on every navigation (P4), so the result never outlives its address. **Back from Admit** (`?back=‹userId›`, T10): Cancel on Admit lands here with the focus on that registrar's Admit….

## States (T08, T09, T329)

- **Nobody waiting**: "Nobody is waiting to be admitted." · "A registrar appears here after registering from a Trainee invitation." (the empty card). **No current trainees**: "No current trainees" · "A registrar appears here once admitted."
- **Loading**: `p.loading-line` "Loading the trainees." over a skeleton card.
- **Load failed**: "Could not load the trainees. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), never the exception's text; a Try again that loads hands the focus to Waiting's heading.

## Narrow widths

Both tables stack at 900px and below (`StackWide`): each row a block, the name heading its labelled cells; Admit… and Edit are 44px from 900px (`.clinic-table--stack-wide .btn`). Measured on the replay at 1100, 1000, 901 and 390: no sideways scroll and no word split mid-word (Step 2.29).

## Known gaps

- **Found re-syncing, not filed:** the onboarding column's floor for a wrapping header (`.inv-page .clinic-table th.col-wrap`, 7.5rem), written for Invitations' Delivery, also reaches Waiting's Email column, which is `.col-wrap` too.

## Contrast

No new colour token. The counts and the second lines (`.inv-count`, `.inv-when span`, `.person-cell`'s) `muted-text` 5.09:1 on the surface (4.83:1 for the count on the page's ground); the row header 11.36:1 on `header-bg`; the outline buttons 4.86:1.
