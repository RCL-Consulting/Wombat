# Assessors

Assessors (`/admin/assessors`, `Pages/Admin/Assessors/AssessorsList.razor`; its words in `AssessorsText.cs` and `AssessorTrainingWords`, its read `ListAssessors`): **every holder of the Assessor role in the reader's scope, by last name, with the training recorded on their page** (Q2). The list records nothing itself: each row opens the person's page at its training section (AssessorTraining). Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-T01 to T14; Q2, Q18, S8, S9, S11, S14, F9; T326, T329) rebuilt it. Until then it listed assessor profiles, so a holder with no profile was missing and a removed Assessor was still listed; it offered "Create assessor profile", printed a failed read's exception, and said a profile made assessors "selectable in the workflow".

## Who reads it

Administrator and Institutional admin, each in their scope; the Administrator's adds an Institution column (T14, S14). Someone who also holds Trainee reads nobody: the standing note `#assessors-trainee-note`, no subtitle (S8).

## The page

The header: "Assessors", "Everyone at ‹institution› who holds the Assessor role, with the assessor training recorded for each." (the Administrator's "… at every institution …"; "… at your institution …" before the name is read, S9). No header action. In `.people-page`:

- `h2#assessors-heading.list-section-title` "Assessors at ‹institution›" ("Assessors at every institution") with its count in `.inv-count`.
- `DataTable` `StackWide`, captioned "At ‹institution›, by last name: each holder of the Assessor role, and the training recorded on their page.":

| Column | What |
|---|---|
| Name | the row header, `.person-cell`: the name over the address (`<wbr>` after its @; no `.col-wrap`) |
| Institution | the Administrator's only |
| Scope | `PersonScopeWords`: "Paediatrics / Paediatrics" |
| Training status | "Trained", "Provisional (awaiting sign-off)", …; **"Not recorded" for no record** (Not started is a record) |
| Training completed | `.inv-when`: "2020-10-26"; "Not recorded" for a record with no date; "—" with no record |
| (Actions) | `.col-actions--single`: **Record training** (no record) or **Edit training**, `.btn-outline .btn-sm`, named "… for ‹name›", to `PeopleLinks.UserTraining(id)` |

- **The pager only when the holders fill more than a page** (`TotalCount > PageSize`, F9); a page turn focuses the heading, a change of rows per page keeps the focus on its select (build review P6).
- A removed Assessor is not listed, with no note saying so (Q18, S11).

## States

- **Nobody holds the role** (T01): the empty card `#assessors-empty`, "Nobody holds the Assessor role yet" · "An assessor's training record holds their training status, the date they completed it and their qualifications. Give someone the Assessor role from their page in Users; they are listed here, and their training is recorded on their page."
- **Loading**: "Loading the assessors." over a skeleton card. **Failed**: "Could not load the assessors. Nothing has changed. Try again, or come back in a few minutes." with Try again, which focuses the heading (or the empty card).

## Narrow widths

The table stacks at 900px and below; the row's button 44px from there; below 641px every control 44px. Measured on the replay at 1280, 1000 and 390 (T376 step 8): no sideways scroll.

## Contrast

No new colour token; the address and the count `muted-text` 5.09:1.
