# T364 build contracts (flow 08)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report.

## § 0: already on `t364` (`94e14fee`), written by the integrator

- `ReviewLinks.ScheduleNew` = `"/committee/reviews/new"`.
- `ReviewLinks.Schedule(int? panelId, string? traineeUserId, string? period, string? returnTo)` builds
  `/committee/reviews/new?panel=10&trainee=…&period=2026-1&return=…`, leaving out each null part.
  - `period` is the key Decisions due writes (`2026-1`), always the row's own period (F1).
  - Lane D builds every Schedule link with it; lane S reads the same keys.
- `ReviewLinks.ReturnKey` = `"return"`. `ReviewLinks.ScheduleReturn(string? returnTo)` is where Cancel goes:
  - `returnTo` when its path is `/committee/decisions-due` or `/committee/reviews` (a query allowed, no `#`, `\` or
    control characters);
  - else `/committee/reviews`. Lane S uses it, and never follows `return=` raw.
- `PagerControls.PageSizeLabel` (`string`, default `"Per page:"`): the page-size select's `<label>`, now a real label.
  Lane D passes `"Decisions per page"`.
- `app.css`, `/* Flow 08: the committee calendar, shared (T364) */`:
  - `.status-cell`;
  - `.period-window` (`span` unbroken);
  - `.list-section-page`, `.list-section-count`;
  - `.choice-row` (a bare fieldset), `.choice-group`, `.choice-group.is-invalid` (with its forced-colours border);
  - `.person-cell` (`span` the address);
  - `.readonly-value`;
  - the `.committee-calendar` phone rules: below 641 px, its forms' `.form-check`, `.form-control` and `.form-select`
    are 44 px, and so are its stacked rows', header's and form actions' buttons, full width; up to 900 px, the
    stack-wide lists' buttons are 44 px.
  - **Every one of the five pages wraps its content in `<div class="committee-calendar">`.**

## Across lanes

- **The seat rule** is `CommitteeReviewSeats` (lane S owns the file). Lane D's trainee line and lane S's Already held
  both call it, and never re-implement "who holds the period".
  - If lane S changes its signature, lane S says so; lane D's call is fixed at the merge.
  - Lane D may **add** a read-only helper to it, stated in its report, but must not change what exists.
- **Review type words.** `CommitteeDecisionWording` is lane S's.
  - The trainee line (lane D) names a binding review's type as `CommitteeDecisionWording.ReviewTypeLabel` gives it, in
    lower case inside the sentence ("(annual progression)").
  - A check-in is "Formative check-in #6".
- **Periods** are `CommitteeReviewPeriods` (lane S owns the file): `Around` gives the six (2025 S1 to 2027 S2 in
  2026), the same on both pages (M5).
- **Panel names** on Decisions due rows are the holding review's panel (T259) from lane D's query. Nobody changes
  `DecisionRouting`.
- **No new role check.** Every page keeps its policy and its handler's scope check.
