# StarRegister

Entrustment decisions, the STAR register (`/admin/entrustment-decisions`, `Pages/Admin/EntrustmentDecisions/Index.razor`; the row's line and dates in `Shared/StarWords.cs`; its script `wwwroot/js/review-appeal.js`): **every STAR the programme has issued**, filtered with Show, each with its certificate, and Revoke on one in force. Flow 09 (T366, 2026-10-14, 49e7cf5e, with T367, e9b29081; boards R3-6; round 3 F3, F9, M4, M5, M6, M9, M10, S1, S4, S6, S7, S11; C21, C22, C24; T260, T319) rebuilt it. Until then its filters were applied with "Apply filters", Issued did not name its review, a revoked STAR read only "Revoked", and Revoke opened an inline section whose "Confirm revocation" stayed disabled until the reason box lost the focus (T260) and which said "The trainee is notified." though nothing was sent (T319).

## Who reads it

Administrator, Institutional admin and both speciality admins (`[Authorize(Roles = …)]`), each the STARs in their scope (`ListEntrustmentDecisionsForAdminQuery`). Both speciality admins' menus offer it as "Entrustment decisions", which lights itself; the Institutional admin reaches it from Home's quick link, under the trail Home › Entrustment decisions (NavMenu).

## The page

`div.entrustment-list` (the wrapper its phone rules are scoped to). The header: "Entrustment decisions", subtitle "Issued Statements of Awarded Responsibility across the programme."

- **The filters** (FilterBar; F9): a `form.search-container` named "Filter Entrustment decisions": **Trainee** (`input[type=search]`, "Filter by name"), **Status** (All, Active, Expired, Revoked, Superseded), **Show**, and **Clear filters** once a filter is applied (its button gone, the focus goes to Trainee). Nothing changes until Show. **After Show a visually hidden status says "3 shown"** (S11; "Loading the STARs." while it reads), present from the first render. Nothing issued at all draws no form.
- **The result**, above the table in an `ActionResult` that takes the focus: "Anele Dlamini's PAED-002 STAR is revoked." (green, claiming no delivery of the mail, S6), or a download's refusal, "The certificate could not be found among the decisions you oversee." (red).
- **The list**: a hand-written `.clinic-table.clinic-table--stack.clinic-table--stack-wide`, captioned "Issued STARs (entrustment decisions) · 5 shown" (C24: the h1 keeps the menu's words); **newest issued first, then STAR number descending** (F1):

| Column | What |
|---|---|
| Trainee | **the row's header**: "Lerato Molefe" |
| EPA | the EPA by name (EpaLabel), the paused mark after a deactivated EPA (T255) |
| Level | "4", "3b" |
| Issued (`.col-fit`) | "2026-10-14", and under it **"review #1"**: a link to the committee's page, named "Open review #1", **only where the reader may open that review** (`IssuingReviewReadable`, the review read ladder, S7), plain text otherwise; `a.issued-review`, an inline box 24px tall (0.9rem), never a 21.6px line (M9) |
| Expires (`.col-fit`) | the date, or "—" |
| Status | the badge (`BadgeFor.EntrustmentDecision`): Active green, Expired amber, Revoked red, Superseded grey |
| (Actions) | **Download** (`btn-sm btn-outline`, `download`) on every row, **Revoke** (`btn-sm btn-outline`; the red is the dialog's, C21) on an Active one, each named for its row: "Download the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-14", "Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-14" (T239) |

- **A row's line** (S4, C22; DataTable's note row): a revoked or superseded STAR's row is `tr.has-note`, followed by a row whose one cell spans the seven columns, `td.history-note`: "Revoked 2026-10-14 at 16:50 SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again." (the stored moment on the South African clock); "Superseded by STAR #18, issued 2026-10-13." (no supersession date is stored, so the successor's own).
- **Filtered to nothing**: the count stays (`p.list-section-count`, "Issued STARs (entrustment decisions) · 0 shown") over the empty card, "No STAR matches these filters." and what was asked, "Trainee “Mahlangu”, status Revoked."; the card offers no Clear filters of its own, the filter bar's just above being the one (D10).

## Revoke (a dialog with a field; ConfirmDialog, M5)

The page's one `ConfirmDialog`, `DangerAction`, its body naming the row:

- "Revoke Anele Dlamini's PAED-002 STAR?" · "PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-14. Revocation is immediate and cannot be undone." · "Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a link to My authorisations, unless Anele Dlamini's account is deactivated. The reason is not in the email: Anele Dlamini reads it on My authorisations and on the certificate." (T367; true where the send is skipped, and claiming no delivery, S6) · "The EPA reads 'Revoked: re-decide' on Decisions due, and Anele Dlamini's review agenda keeps its line." · Cancel · **Revoke the STAR**.
- **The field** (`FieldContent`, outside the dialog's description, so its label and counter are read once, D9): **Revocation reason**, required, a `textarea` (3 rows) with **no `maxlength`** (D2); its refusal, then its counter, a `.field-help`, "69 of 1,000 characters", counted in the browser (`review-appeal.js`'s `count`), named by the box (FormField). **The box takes the first focus** (`InitialFocusId`).
- **Revoke the STAR is enabled from the start** (T260). Pressed blank: "Give a reason for the revocation." under the box, which takes the focus; over 1,000: "Give the reason in 1,000 characters or fewer." (R3). The dialog stays open with what was typed (`OnConfirmClosesWhen` answers false); nothing is sent.
- **A refusal from the command** shows inside the dialog, under the reason, in an `ActionResult` that takes the focus, the reason kept (M4); an unexpected failure, "The STAR could not be revoked just now. Try again, or come back in a few minutes.", logged.
- **Done**: the dialog closes, the list reads again, and the result above the table takes the focus once nothing modal holds it; the row reads Revoked, Download only, with its line. Cancel or Escape returns the focus to the row's Revoke (D6).

## States (T329)

- **Loading**: the header and the filters at once, five row skeletons (3rem), no Download or Revoke (C25).
- **Load failed**: "Could not load the STARs. Nothing has changed. Try again, or come back in a few minutes." with Try again.
- **Nothing issued**: the empty card, "No STARs issued yet", "When a committee ratifies an entrustment decision, its STAR is listed here.", with no form.

## Narrow widths

**Stacked at 900px and below** (both stack classes; DataTable): each row a block, the trainee heading its labelled cells, its line joining the row's block, unpadded, wrapping, never cut (D4). Below 641px every row's button and Issued's review link are 44px targets. No sideways scroll at 1280, 768 or 390 (the replay's 4.35).

## Contrast

No new colour token. Body text 12.63:1 and the row's line 12.63:1 on the surface; the row header 11.36:1 on `header-bg`; the review link 6.70:1; the buttons 4.86:1; the badges as Badge's.

## Known gaps

- **Found by the replay, not filed:** the revoke dialog keeps "Give a reason for the revocation." under the box until the next press, after the reason has been typed.
- **Kept by the build review** (D8): while the revoke runs, the dialog's Cancel is natively disabled (the busy-state convention of flow 07 and T234).
