# AdmitRegistrar

Admit ‹name› (`/admin/trainees/admit?userId=…`, `Pages/Admin/Trainees/AdmitRegistrar.razor`; its words in `AdmissionText.cs` and `TraineeAdmissionRefusals`, its addresses in `Shared/OnboardingLinks.cs`): **a form page of its own that admits one waiting registrar**: who is being admitted, then the programme, with the curriculum adopted for the invited sub-speciality read out, the training year an output of the start, and what admitting will do. Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-M01 to M15; round 3 Q5, Q17, F1, F4, M04b, S8, S9, S18, check rules 2, 3 and 7; T290, T306, T329; build review B2, B12, P2, P3) split it from the trainee profile page, whose `?userId=` address now redirects here. Until then admission was a branch of that page: a curriculum select of every adopted version, no training year, an empty completion derived silently, and the page left on the profile saying nothing.

## Who reads it

Administrator and Institutional admin, each a registrar waiting in their scope. Owned by Trainees for the Institutional admin (NavMenu): Home › Trainees › Admit Lerato Molefe, Trainees lit. The Administrator, whose menu has no Trainees, has nothing lit and the same trail, its Trainees crumb given by the page (`PageHeader`'s `Trail`; M04b). Someone who holds Trainee sees "Admit a registrar" and T290's note, `#admit-trainee-note`, "You hold the Trainee role, so you cannot view or change trainee profiles, including your own.", asked before any read (M1).

## The page

The header: **"Admit Lerato Molefe"** (h1, tab and crumb, Q17), "Admit a registrar who is waiting into the institution's curriculum." While it loads, when its read failed, and with T290's note, the page is **"Admit a registrar"**: no name before the read (check rule 2). Then `.inv-page.inv-page--form` (48rem):

- **The result** (`ActionResult`, `#admit-result`), at the column's top: a refusal that is not its field's.
- **Who is being admitted** (`section.detail-card`, `h2.dashboard-card-title`; `dl.detail-list`): Email (with `<wbr>` after its @), Institution "Kgosi Kgari Teaching Hospital", Invited as "Paediatrics / Paediatrics" ("—" with none), Registered "2026-10-20" (the used invitation's day; "—" with none).
- **Programme** (an `EditForm`, `form.form-container`, `novalidate`, named by its `h2` "Programme"):
  - "* marks a field you must fill in." (`p.field-help`). The fields are written by hand, not with the FormField component: each `.form-group`, its `label` ending in `span.required-mark` (`aria-hidden`), the control with `aria-required`.
  - **Curriculum, read out** (F1, S18): `dl.form-group` with `dt` "Curriculum", `dd.readonly-value` "Paediatric EPA Curriculum (11.1)" and `dd.field-help` "Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality." Only a registrar invited to no sub-speciality chooses (M14): **Curriculum** * a select from "Choose a curriculum", its help "This registrar was invited to no sub-speciality, so choose from the curricula Kgosi Kgari Teaching Hospital has adopted. Admitting sets the registrar's scope from it."
  - **Programme start** * (`#admit-start`, the browser's date input), today on the South African calendar at first. A start more than four years back adds a `p.field-warning` under it, which the field names: "This start is more than four years ago. Check the date before you admit." (M07: a warning, never a refusal).
  - **Training year**, beside it, **an output, not a field** (S18, T306): `label` "Training year" over `output#admit-training-year.ty-output[for=admit-start][aria-live=polite]` (block, 1.1rem/600), "Training year 4", or before the programme starts "Not started: the programme starts on 2027-01-15", with "Training years turn every 365 days from the programme start, counted to 2026-10-20. It follows the date when you change it." The page asks the Application for it (`GetAdmissionDatesQuery`) each time the start changes; Razor never counts the blocks.
  - **Expected completion** (`#admit-end`, not required): its help "Leave this empty to derive it from the curriculum window.", and while it is empty " Left empty, it will be 2027-01-15." after it (M03). **Where that derived date has passed** the help is replaced by the refusal's one sentence, "Left empty, it would be 2024-01-15, which has passed. Enter the expected completion."; once Admit is refused for it, the sentence moves to the field's `.validation-message` (`#admit-end-error`), the field is `aria-invalid` and described by both, the help goes back to its first words and the focus goes to the field (F4, check rule 7): the sentence is shown once.
  - **The check line** (`.submit-check`; SubmitCheck): "**When you admit:**" over a list of three: "Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.", "Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.", "An email tells Lerato Molefe so." Before the programme starts (M06): "… becomes a Trainee on the …, whose programme starts on ‹start›. Nothing can be filed before then.", the session line, "An email tells Lerato Molefe the day the programme starts."
  - `FormActions`: **Cancel** (outline, back to Trainees with `?back=‹userId›`, whose Admit… then takes the focus, T10) and **Admit Lerato Molefe** (primary). While the admission runs it keeps the focus with `aria-disabled`; a second press sends nothing (T234).
- **Admitted**, the page goes to Trainees with `?admitted=‹profileId›`, which says what was done (Trainees, S8). Profile, scope and the role swap are one transaction: a fault leaves the registrar waiting, and a retry admits (B2).

## Nothing to offer (M04, M04b, check rule 3)

Standing content, so a warning Alert with **no role**, and "Back to trainees" (outline), with no form: "No curriculum is adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, so Lerato Molefe cannot be admitted." where other curricula are adopted; "This institution has adopted no curriculum yet, so no trainee can be admitted. Adopt one under Curriculum adoptions, then admit the trainee." (its link) where none is; "This registrar belongs to no institution, so no curriculum can be offered."

## Refusals (S9, M08, M09, M13)

The admission's own (`TraineeAdmissionRefusedException`), each led by "**Not admitted.**":
- In the result at the column's top, focused, a danger Alert: "**Not admitted.** Lerato Molefe already has an active trainee profile. Open Trainees" (a stale tab's second Admit, M09; the link); "**Not admitted.** Lerato Molefe's account is deactivated. Reactivate it under Users first." (M13; Users a link to that account).
- Beside its field: F4's, under Expected completion (above); "Choose a curriculum." and "Enter the programme start." under theirs.
- **A registrar not waiting, out of scope or unknown** is the system's not-found panel (M08, S20): the h1 "Registrar not found" with `search`, "No registrar waiting for admission has this link.", "Those waiting are listed on Trainees.", and Go to Trainees (primary). A refusal that turns the page into it moves the focus to its heading (P3).
- Anything else, logged: "**Not admitted.** Could not admit this registrar. Nothing has changed. Try again, or come back in a few minutes."

## States (M10, M11, T329)

- **Loading**: "Admit a registrar", `p.loading-line` "Loading this registrar." over a skeleton card.
- **Load failed**: "Could not load this registrar. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel); a Try again that loads hands the focus to Who is being admitted (P2).

## Narrow widths

Below 641px every button in the column is 44px (`.inv-page .btn`) and the form's two actions stack, each the row's width, **Admit ‹name› on top** (`.inv-page .form-actions`, `column-reverse`, as every stacked action row; Cancel stays first in the tab order); Programme start, the output and Expected completion go one under another (`.form-grid`). Nothing scrolls sideways at 390 (Step 2.29).

## Known gaps

- The admission writes the registrar's speciality scope from the curriculum's discipline, as an assessor-profile save does (T289's other callers, `AdmitTrainee`).

## Contrast

No new colour token. The read-out curriculum and the output body text 12.63:1 on the surface; the help and the terms `muted-text` 5.09:1; the warning as FormField's `.field-warning`; the check line as SubmitCheck's.
