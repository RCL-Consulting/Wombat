# T379 build review: the pages, the design system and accessibility

Reviewer: read-only, by reading markup, C# and CSS (no app run, no test run, no browser; every measurement is from CSS and
markup). Diff `git diff master...t379` at `95d3f765`. Checked against `round-3/boards-text.md` (the three Spec boards and the
R, H, E, V, P and C boards at 1280), `round-3-check.md` (Build notes), `build-contracts.md` (§ 5, § 6) and DESIGN.md's
flow 13 text. Read: `FormerTraineeRecordCard`/`Words`, `Home`, `MyProgress`, `EpaProgress`, `MyAuthorisations`,
`MyActivities`, `ExportPortfolio`, `ExportTraineePortfolio`, `ExportPortfolioForm`, `ExportUnavailable`, `ExportSkeleton`,
`ExportWords`, `VerifyExport`, `VerifyExportWords`, `TraineeProfileEdit`, `TraineeProfileWords`, `PendingTraineesList`,
`AdmissionText`, `ReviewDetail`/`ProgrammeTraineeDetail` header actions, `ExportPortfolioAction`, `PortfolioLinks`,
`PortfolioWords`, `BadgeFor`, `NavItems`, `NavOwners`, the 295 new `app.css` lines and the DESIGN.md diff.

Result: **0 must-fix, 4 should-fix, 7 nits.** The words match the boards almost everywhere; the four should-fixes are two
deviations from boards (R3-R01 and R3-R09), one pager the boards do not draw, and one form that keeps a summary where the
system wants the refusal under its field.

## Findings

### P1 (should-fix): "Your record:" stands above the ended notice; R3-R01, R3-R02 and R3-R10 draw it under, and DESIGN.md does not say it deviates
- `MyProgress.razor:40-55` draws `div.record-line` before `<EndedRecord>`, whose first element is the notice. The boards read
  h1, subtitle, notice ("You completed your programme on 2026-10-26. This page is your record of it ..."), then "Your record:",
  then "Curriculum targets". The build reads h1, subtitle, "Your record:", notice, targets. The code comment is honest ("so it
  stands above the notice rather than under it, where R3-R01 draws it"); it exists because `EndedRecord` holds the notice and its
  golden markup is unchanged (S8).
- DESIGN.md `:3714-3718` says "so it stands above the ended notice" but does not say that is not what R3-R01 draws, so a reader
  who checks the board against the page finds a difference the document does not own. For a screen reader the page's first
  statement is now a list of links, before what the page is.
- Fix: either give `EndedRecord` an optional `RenderFragment AfterNotice` (default null, so the golden markup holds) and draw the
  line there; or the operator accepts the position, and DESIGN.md says "R3-R01, R3-R02, R3-R10 draw it under the notice; built
  above it because EndedRecord owns the notice".

### P2 (should-fix): My authorisations' empty card follows Spec 3's Empty row, not board R3-R09, and DESIGN.md cites R3-R09 for the opposite
- `MyAuthorisations.razor:~147-163`: for a reader with no Trainee role the empty card's title is "No STAR was issued to you, so
  there is no certificate to download." and its body is `string.Empty`. R3-R09 (Pieter du Plessis, "Flow 09's empty card, as
  worded") draws "No active authorisations yet" with "When a committee review ratifies a decision ... You can then download a STAR
  certificate for your records." Spec 3's Empty row has the NEW words; the boards disagree with each other, and build-contracts
  § 6 says "R09 keeps the built words" for the NEW row. The lane's choice (a graduate will never be issued one, so "will appear
  here" is false) is defensible.
- DESIGN.md `:3717-3718` reads "... the empty card's title is 'No STAR was issued to you ...' in place of 'No active authorisations
  yet', which promises one (R3-R09)". R3-R09 is the board that draws the built words, so the citation is wrong.
- A side effect: `StatePanel` always draws `<div class="state-panel-copy">@EmptyBody</div>` (`StatePanel.razor:40`), so an empty
  body leaves an empty div (its margin and the card's gap) under the title.
- Fix: say "(Spec 3's Empty row; R3-R09 draws the built words)" in DESIGN.md, or redraw R3-R09; and pass a one-sentence body
  ("Your programme has ended, so no more will be issued." is the kind of line, NEW and for the operator) or have `StatePanel` omit
  the copy div when the body is empty.

### P3 (should-fix): My activities draws the pager for any list; R3-R07's seven rows draw none, and the brief says only past one page
- `MyActivities.razor:104` renders `<PagerControls ...>` under `@if (_page is { TotalCount: > 0 })`; `PagerControls.razor:10`
  guards only on `TotalCount > 0`. So seven activities get "Showing 1-7 of 7", Previous, Next and the size select (28 px at 390,
  N16). Board R3-R07 (1280, 768 and 390) has no pager, and build-contracts § 5 describes `PagerControls` as "rendered only when
  `TotalCount > PageSize`", which is true of the pages that guard it (Users, Assessors, the closed list) and not of the component.
  Flow 03's page, but flow 13 rebuilt its table, caption and note row on it.
- Fix: guard at `:104` with `list.TotalCount > list.PageSize`. Check `MyActivitiesTests` and the flow 03 runbook steps that expect
  the pager at small counts.

### P4 (should-fix): the profile's Profile details form keeps a `ValidationSummary`; its refusals are not under their field
- `TraineeProfileEdit.razor:388-389` (`DataAnnotationsValidator` and `ValidationSummary`, unchanged from master). A cleared Programme
  start date or no curriculum is refused in a list above the fields: no `aria-describedby` to a message under the field, no focus
  in it. DESIGN.md's rule (and this build's own End section, Export and Verify) put a refusal under its field, `aria-invalid`,
  the focus in it. Whether `InputDate`'s own `aria-invalid` fires is Blazor's, not the page's. No board draws a refusal here, so
  nothing contradicts the page; this is the one form of flow 13's pages that does not follow the rule it states.
- Fix: `ValidationMessage For=` under each `FormField` and focus the first invalid field after a refused submit, or file it as a
  task and say in DESIGN.md that the details form is the exception.

### P5 (nit): Export's date fields name the help first and the refusal second; the end date and Verify name the refusal first
- `ExportPortfolioForm.razor:48,63` use `FieldHelp.DescribedBy(id, hint, errorId)`, which orders help then region
  (`FieldHelp.cs:42-60`); `VerifyExport.razor` (`DescribedBy`, N15) and `TraineeProfileEdit.razor:517` put the refusal first. The
  field is focused on a refusal, so the first thing read is the help sentence, and the refusal is the second. DESIGN.md says
  "named first" only for Verify (and S19 for the end date), so it is not false, only uneven. Fix: build the id list by hand with the
  refusal first, as the end date does.

### P6 (nit): the last-day input has no `required` or `aria-required`, though its label says required
- `TraineeProfileEdit.razor:512-520`: the label carries `*` and a `visually-hidden` "required"; the input has neither `required`
  nor `aria-required="true"` (Export and Verify have `required`). Harmless while the press check refuses an empty value (P23).
  Fix: `aria-required="true"` (not `required`, which would add the browser's own refusal the page deliberately avoids, S3).

### P7 (nit): the lane R phone rule on `.clinic-table--stack` is not scoped to its page
- `app.css:6999`: `.clinic-table--stack tbody tr.has-note { border-bottom: 0; padding-bottom: 0 }` sits in "Flow 13: the record" and
  reaches every table that uses `Stack` (My activities, My progress, the EPA page), not under `.my-activities` or any page class,
  though the brief asks each flow 13 section to be scoped to its page. Today only My activities has a `has-note` row there, so there
  is no visible effect. Fix: scope it (`.my-activities`, or the page's root class) or move it to flow 03's note-row block at
  `:5243` next to the `stack-wide` twin.

### P8 (nit): `.hash-value` sets 0.875rem on the Verify input
- `app.css:7087` (`font-size: 0.875rem`) also applies to `input.form-control.hash-value` (`VerifyExport.razor`), a 14 px field; a
  focused input under 16 px zooms the page in iOS Safari at 390. The boards' `f13r3.css` did the same, so this is the design's.
  Fix: `.form-control.hash-value { font-size: 1rem }`, or leave the field at the form's size and set 0.875rem only on `dd.hash-value`.

### P9 (nit): DESIGN.md's banner entry for flow 13 has the date and no commit
- `DESIGN.md:61-67` ends "(four Graduation and exit sections))" with no commit; the banner says each flow adds itself "with its
  date and commit". Flow 12's entry is the same. Fill it when the commit exists.

### P10 (nit): Verify's result link is 20 px at 390 and DESIGN.md does not name it
- `ExportPortfolioForm.razor:36` ("Verify portfolio export" inside the success Alert) is an inline link; round-3-check A3 lists it
  as the exception (E05/E06-390, 20 px, WCAG 2.5.8's inline exemption). DESIGN.md's Export portfolio text ends "At <=640px, inside
  `.export-page`: the date controls 44px, Export PDF full width and 44px", which reads as if every control is. Add the exception, as
  the pager's N16 is named.

### P11 (nit): the empty-state card on the closed list has no focus target
- `PendingTraineesList.razor` Closed(): the empty `div.detail-card.detail-card--empty` (C03) is a plain div while the h2 takes the focus
  after a turn; with no rows there is no turn, so nothing is lost. Noted only because `StatePanel`'s empty card carries
  `tabindex="-1"`; no action.

## Checked and right

- **Words.** Every sentence on Home's card (H01, H02), My progress's subtitle and line label, My authorisations (R08), My activities
  (subtitle, caption, note row, R07), Export (own and staff: subtitles, hints, the note, the result, E11-E13, E19, Unavailable), Verify
  (V01-V10: the three refusals, the answer, the not-found third sentence equal to V04's, "Once" / "3 times, the last on", the failure),
  the profile (P01-P25: h1, subtitles, Status row, notes, dialog titles and bodies, forecast clauses, results with the take-back,
  account sentences and their P24/P25 variants, the refusals, P12-P15) and the closed list (C01-C07: heading count, caption, headers,
  View's name, Withdrawn, the empty words) equals the board or a § 6 row. Words found on no board and not in § 6: the singular
  "it has 1 character" and "Your 1 STAR in force" and "2 times" are in § 6; "Loading this export." / "Loading this trainee profile."
  (live-region words, boards draw skeletons) and "Generating the PDF." are not, nit-level and fixed words in a status region.
- **Structure.** One h1 per page through `PageHeader`; h2 sections in order on the profile (Profile details, End this programme or The
  account), the closed list's h2 inside Trainees' h1, Home's card h2; no `h3` where an h2 is the level; captions on the closed list
  and My activities; `StackWide` on the closed list (inside `.closed-list`); the pager on the closed list only past 20
  (`ended.TotalCount > ended.PageSize`), 20 a page (`ListEndedProfilesQuery.DefaultPageSize`), its heading `tabindex="-1"`, focused
  after a turn.
- **Buttons and focus.** Mark complete... and Deactivate... are `btn-outline btn-sm`, `aria-haspopup="dialog"`, `aria-describedby`
  their notes (`end-mc-note`, `end-dc-note`); the filled danger is only `ConfirmDialog`'s (`DangerAction="true"` for Deactivate,
  false for Mark complete, whose confirm is the primary, Q24); Cancel is first in the DOM and `.dialog-card .form-actions` is
  `column-reverse` at 390; the profile's Save row is Cancel (link) then Save with `.user-page .form-actions` column-reverse (flow 12).
  Results are `ActionResult FocusOnLoad` with a fresh `@key` (the profile), `FocusAfterRender` (Export), `FocusOnLoad="@Pressed"`
  (Verify); a refusal found before a dialog is `div.validation-message role="alert"` with `aria-invalid`, the field in
  `aria-describedby` and `PageFocus.FocusByIdAsync` (end date, Export dates); Verify's field is `autofocus` only after a press with a
  refusal (N4) and names `hash-e hash-h hash-cmds`, each id rendered under the condition that names it.
- **44 px at 390.** Home's `.record-links a` and My progress's `.record-line a` 2.75rem below 641; the closed list's View through
  `.clinic-table--stack-wide .btn` (<=900); the account link `.user-page .account-open a`; the export date controls and button
  `.export-page .form-control`, `.form-actions .btn`; Verify `.verify-page .form-control`, `.form-actions .btn` (full width); the
  Unavailable page's Go to Home through `.detail-card--empty .btn`; the header action through `.header-container .actions-cell .btn`;
  the profile's role-row triggers through flow 12's `.user-page .btn`. Exceptions: P10 and the pager's select (N16, named on the
  boards).
- **Stylesheet.** No `style=` in any changed razor file (the one in `EpaProgress.razor:98` is master's), no `bi bi-`, no raw hex or
  `rgb()` in the 295 new lines; every token they use exists in `:root`; every static `class="..."` in the changed pages is defined
  (scripted over the literals, 0 undefined; `DefinedClassTests` not run); every icon name has an svg. The four sections carry the contract's
  headings and `/* end ... */` lines; `.badge-withdrawn` sits under Badges with the same rule as `.badge-draft` and the
  `.details-list` column under Detail lists, as § 0 says.
- **DESIGN.md.** The banner names flow 13 and its sections; the owner table's rows, the Home card, the Verify, Export, closed list,
  profile and badge paragraphs each match their code (checked paragraph by paragraph: ids, widths, classes, the ordering of
  Status, the "no `.col-wrap` on Name", Programme's 7.5rem floor, 52rem and 44rem columns, the NavItems rule for the Trainee
  acting as another role). The two paragraphs that do not are P1's and P2's.
- **R3-R01 and R3-R09 together.** Both deviations are the lanes' reasoned choices, recorded in code comments and in build-contracts § 6
  or its code, and neither is a defect of behaviour; P1 and P2 are about the board and DESIGN.md saying what the build does.
- **Not run.** No suite, no `DefinedClassTests`, no browser, no widths measured.

## Held

- Whether `InputDate` writes `aria-invalid` itself on the Profile details form (P4) needs a render.
- Whether `DataTable` draws `Caption` visibly on `StackWide` tables (the boards draw it visible) was read from the boards, not the component.
- The 901-1100 px behaviour of the closed list's Programme column and Verify's details list is inferred from CSS, not measured.
