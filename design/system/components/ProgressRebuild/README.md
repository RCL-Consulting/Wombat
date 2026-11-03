# ProgressRebuild

Curriculum progress (`/admin/curriculum-progress`, `Pages/Admin/CurriculumProgress/CurriculumProgressRebuild.razor`; the command `RebuildCurriculumProgressCommand`, `RebuildCurriculumProgressResult`): **the Administrator's rebuild of every trainee's curriculum tallies from their completed activities.** Flow 15 (T392, 2026-11-02, c931d56f; boards R2-F01 to F05; Q16, S9, T8; build review N7) kept its words and its one button and rebuilt its frame: `.user-page`'s column, `aria-haspopup="dialog"` on the trigger, and a failure in fixed words in the result's region, focused, where it printed the exception's text (T329).

## Who reads it

The Administrator alone (`[Authorize(Policy = "Administrator")]`, and the command's own check). Reached from Curricula's header action "Curriculum progress" (outline), where Home's Maintenance card linked it until flow 01; Curricula stays lit, and the trail is Home › Curricula › Curriculum progress.

## The page

- **The header**: h1 and tab "Curriculum progress", subtitle "Recalculate every trainee's curriculum targets from their completed activities." No header action.
- **The column**: `<div class="user-page rebuild-page">` holding one `.form-container`:
  - Three paragraphs, the second and third `muted`: what a rebuild does ("Each completed activity is credited to the curriculum items it matches, in the semester that contains its encounter date. A rebuild throws the stored tallies away and credits every completed activity again, in the order the activities were completed."); when to run it, that it is safe to run more than once, that it "runs as one transaction, so if it fails, nothing changes", and that it runs by itself at startup when the tallies are empty; and what it judges as of when ("A rebuild credits each activity against the curriculum as it is today: its items, targets and minimum levels. Only whether an EPA was active is judged as of when the activity was completed. …"; T196, D48).
  - `.form-actions` with **Rebuild progress**, the primary (Q16: it stays a primary that opens its dialog, its words with no "…" as R3-F02 draws them), `aria-haspopup="dialog"` (build review N7); "Rebuilding…" while it runs, `aria-disabled` and never natively disabled (T234).
  - The result's region (`ActionResult`) under the bar, inside the card: `.rebuild-page .action-result:not(:empty)` gives it its own 16px step under the bar (S9).
- **The dialog** (`ConfirmDialog`, action mode, so it opens on Cancel): "Rebuild curriculum progress?" over "Every trainee's curriculum progress is recalculated from their completed activities, each filed into the semester of its encounter date. An inactive EPA keeps the progress it earned while it was active. It is safe to run more than once, and if it fails nothing changes."; **Rebuild progress** the primary. Escape or Cancel returns the focus to the trigger. The dialog closes before the work starts, so the page's own "Rebuilding…" shows.

## Results (S9, T234)

- **Rebuilt**: a success Alert, "**Curriculum progress was rebuilt.**", over a `dl.details-list` of five figures, each a row: Activities re-read, Curriculum items credited, Semester tallies written, Stale tallies removed, Completions re-stamped. The region takes the focus once the rebuild has answered; the figures are not announced on their own.
- **Failed**: a danger Alert, "**The rebuild failed, and nothing was changed.** Try again, or come back in a few minutes." (`FailedLead`, `FailedRest`), focused, the cause logged (T329). No Try again button: Rebuild progress stays offered (T8).

## Narrow widths

Below 641px `.user-page`'s rules hold: Rebuild progress 44px and the row's width, the dialog's buttons stacked with Rebuild progress on top. The five figures are `.details-list` rows. Measured on the states pass at 390 (`curriculum-progress-rebuild--narrow`): nothing sideways.

## Contrast

No new colour token. The muted paragraphs `muted-text`, 5.09:1 on the surface; the alerts as Alert's.

## Known gaps

- None new. Its words are the built ones (Q16: no board redrew them); its failure is reached only by holding a lock on a scratch database (`curriculum-progress-rebuild--failed`, states.md).
