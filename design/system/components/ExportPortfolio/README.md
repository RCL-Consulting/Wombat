# ExportPortfolio

Export portfolio: **two pages, one form**. The person's own export (`/portfolio/export`, `Pages/Portfolio/ExportPortfolio.razor`) and a member of staff's export of one registrar (`/portfolio/export/{TraineeUserId}`, `ExportTraineePortfolio.razor`, its own component so it lights its owner by route, S2), both drawing `ExportPortfolioForm.razor` once `GetExportSubjectQuery` has answered; `ExportSkeleton.razor` while it reads, `ExportUnavailable.razor` for every refusal; the words `ExportWords.cs`, `Shared/PortfolioWords.cs`, the addresses `Shared/PortfolioLinks.cs`; the entry points' header action `Shared/ExportPortfolioAction.razor` (PageHeader). Flow 13 (T379, 2026-10-27, 5cc10c66; boards R3-E01 to E20, R3-X01 to X10; Q4, Q5, Q17 to Q19, F1, F4, S2, S6, S13, A2, A3) rebuilt it. Until then the staff export was reached only by typing a user id and never said whose portfolio it exported (T314), and the 90-day cleanup job made Verify deny a genuine PDF (T313; the job is retired, so eight scheduled jobs).

## Who reads it

- **The own export**: anyone signed in with a trainee profile (`[Authorize]`): a Trainee from her menu, a former trainee from the menu's personal links and her Home's record card (NavMenu, FormerTraineeHome). A page the menu offers: it lights itself and draws no trail (F4).
- **The staff export**: whoever the export command admits for that registrar (`TraineeScopeResolver.MayReadAsync`): the Institutional admin (Home › Trainees › ‹name›'s programme › Export portfolio), the four programme roles (Home › Programme trainees › ‹name› › Export portfolio), the Administrator (Home › Export portfolio). The middle crumb is the page's (`PageHeader`'s `Trail`), none while it reads, when it failed, or with no profile. **Its entry points** are "Export ‹name›'s portfolio" (`download`, outline) in the header of the trainee profile (TraineeProfile), the committee review (ReviewSitting) and the registrar's programme page (ProgrammeTraineeDetail), each only where the page's DTO says `MayExport`, so no page offers an export that would be refused.

## The page

- **The header**: the own export "Export portfolio" (h1, tab and menu item, Q17), "A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities."; the staff export "Export Lerato Molefe's portfolio" (`PortfolioWords.ExportAction`, the action's own words, Q5), its crumb "Export portfolio", its subtitle "Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4" (the year the programme ended in, for an ended one), "Export portfolio" while it reads and when it failed (N12). No header action.
- **The status**, `p.visually-hidden[role="status"]`, always present: "Loading this export." while the subject is read; the form's own, "Generating the PDF." while the PDF is made.
- **The column** (A2): `.export-page`, a flex column 24px apart at most 52rem: the result region (`.action-result`, taking no row while empty), then `form.form-container` (`novalidate`):
  - `.form-grid` of **From date \*** and **To date \*** (`type="date"`, `required`; the `*` hidden from assistive technology, "required" said instead). They open on the programme start and today on the South African calendar, or the last day of an ended programme (Q18). Each hint, `p.field-help`, names whose: "Your programme started on 2023-01-15." / "Lerato Molefe's programme started on 2023-01-15."; "Today, on the South African calendar." / "Your last day in the programme." / "Lerato Molefe's last day in the programme.".
  - `p.export-note` (muted, 0.9rem, 16px under the dates): "Exporting the same dates again gives the same file, until the record changes."
  - `.form-actions`: **Export PDF** (`btn-primary`, `download`); while the PDF is made "Generating..." and `aria-disabled`, never `disabled`, so it keeps the focus.

## Refusals, the result and failures

- **An emptied date** is the page's own refusal, "Choose the from date." / "Choose the to date."; **From after To** the command's, "The start date must be before or equal to the end date." (its `ValidationException` mapped to From by its property, S6). Either is a `div.validation-message[role="alert"]` under its field, the input `aria-invalid` and `.input-validation-error`, described by the refusal first and then its hint (P5), and focused; the dates are kept. No summary.
- **Exported** (E03, E05): the result above the form takes the focus, a success Alert, its first sentence bold and naming the file: "**Your portfolio exported: portfolio-9ac3aea9a795.pdf.** The download should start automatically. Anyone holding the file can check it on Verify portfolio export." ("**Lerato Molefe's portfolio exported: …**" on the staff export). Verify portfolio export is a plain `<a href>`, a full navigation to the static page (S13).
- **Failed** (E13): fixed words, "**The portfolio could not be exported just now.** Nothing was downloaded. Try again, or come back in a few minutes."; the exception to the log (T329). One result at a time.
- **Portfolio unavailable** (Q19; E09, E10, E19): one page for every cause (refused, an id no account holds, an erased person, and on the own export a caller with no trainee profile): h1 and tab "Portfolio unavailable", the empty card "This portfolio does not exist, or you cannot export it." and **Go to Home** (`btn-primary`, `home`); no form. Drawn as Page not found is (`Page=`), so nothing is lit and no trail is drawn. A refusal by the command at Export PDF draws it too.
- **Loading** (E14): one card of still `.skel-*` lines in `.export-page`, `aria-busy`; no form. **Load failed**: "Could not load this export. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), which focuses the h1.

## Narrow widths

Below 641px, inside `.export-page`: the date controls 44px, Export PDF the form's width and 44px (A3; scoped to the page, never a bare `.btn`). Portfolio unavailable's Go to Home is 44px (`.detail-card--empty .btn`). Measured on the replay (wombat_scenario_t379; Lerato Molefe's own, Nolwazi Mbatha's staff export of Pieter du Plessis) at 1280 and 390: no sideways scroll, every control 44px at 390.

## Contrast

No new colour token. The hints and the note `muted-text`, 5.09:1 on the surface.

## Known gaps

- **Proposed** (T385, extended by the states pass): at 390 a hint's date can split at its hyphen.
- The result's Verify portfolio export is an inline link in a sentence, 20px at 390: WCAG 2.5.8's exemption, named on the boards (build review P10, left).
- The own export and the form each keep an always-present, visually hidden status region, so the page has two; only one speaks at a time.
