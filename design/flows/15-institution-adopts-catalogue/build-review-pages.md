# Flow 15 build review: the pages and the runbook (T392)

Reviewed 2026-11-02, branch `t392` (`ee98ba1c`), worktree `C:\dev\Wombat\.claude\worktrees\t392`, diff `9d4b2a2f..HEAD` over
`src/Wombat.Web`, `execution/knowledge/scenario-paediatrics` and `execution/architecture/DESIGN.md`. Paths below are relative to
`src/Wombat.Web/Components/Pages/Admin/` unless they start `src/`, `execution/` or `design/`.

**Method.** Static review: every changed page and Words file read in full, against the round-3 boards, `round-3-check.md` § Decisions,
`build-lanes.md` (P1-P6) and `build-contracts.md`. Every board's text (107 screens, plus the standing round-2 boards) was extracted and each
line searched for in `src/`: all of it is found, or is dynamic data, or is a canvas label, except the lines listed under "New words".
Scripted greps over the diff for: gendered pronouns, `ex.Message`/`exception.Message`, `style=`, hex, `bi bi-`, `<svg`, `td…actions-cell`.
Runbook: every quoted UI string in the steps lane W rewrote was checked against the Razor and Words files. **Not done:** nothing was run or
rendered. The 901 px items table (decision 1), the 390 px 44 px rules and every focus claim are read from markup and CSS, not measured.

**Counts: 1 must, 9 should, 13 nit.**

## Must

### M1. A Target or Completion window of 0 or less shows FluentValidation's default sentence, naming a property
- `Curricula/CurriculumItemPage.razor:392` (the `EditForm` has `OnSubmit` and no `DataAnnotationsValidator`, so `CurriculumItemFormModel`'s
  `[Range(1, int.MaxValue)]` never runs); `:418` (Target) and `:496` (Completion window) take the refusal back through `RefusedInput`
  (`:129-136`, `RequiredCount`, `WindowMonths`). The command's rules carry no message:
  `src/Wombat.Application/Features/Curricula/ManageCurriculumItems.cs:74`, `:77`, `:98`, `:101` (`GreaterThan(0)`).
- A user who types 0 (or clears the box, or types -1) in Target and presses Add item or Save item sees, under Target: **"'Required Count' must be
  greater than '0'."**, and under Completion window "'Window Months' must be greater than '0'." (focused, aria-invalid, correctly placed, wrong words).
  `FieldRefusal.Of` joins the validator's own text, so the T324 rule (never a property's name) and the lane brief's "refusals under their field in
  the board's words" both fail on the first plausible slip. The EPA page does this right (`EpaFieldMessages`, page-side `Check`).
- Fix: give the four `GreaterThan(0)` rules a `WithMessage` ("Enter a target of 1 or more." / "Enter a completion window of 1 month or more.") and add
  the same two page-side checks beside `RungsPicked` so the refusal is found before any send; or run `DataAnnotationsValidator` with `ErrorMessage` on
  the `Range`s. Add the words to `CurriculumItemsWords`. A test: Target 0 refuses under Target with no property name in the text.

## Should

### S1. A successful adoption whose re-read fails shows a success and a failure together
- `Curricula/CurriculaList.razor:592-593` sets `_adopted`, then `ReadAgainAsync(user)` runs inside the same `try`; if the re-read throws, the catch at `:601`
  sets `_refused = "The curriculum could not be adopted. Nothing has changed. …"` while `_adopted` stays set.
- User sees: the green "Paediatric EPA Curriculum (11.2) adopted." at the top and, under Open for adoption, a red "could not be adopted. Nothing has
  changed", and a list that still offers 11.2. The adoption did happen. `CurriculumItemsEdit.razor`'s Remove already separates the two reads (`:363` and `:379`).
- Fix: move the re-read into its own try (as `RemoveItemAsync` does); on its failure keep the success, log, and say nothing more (or a line "The list could not
  be read again. Reload to see it.").

### S2. The same shape on the trainee profile: a saved move says "could not be saved"
- `Trainees/TraineeProfileEdit.razor:615` writes the success result, `:630` re-reads `GetTraineeCurriculumChoicesQuery` in the same `try`; a throw lands in
  `:649-650`, which replaces the result with `TraineeProfileWords.SaveFailedLead` (refused).
- User sees "The profile could not be saved" after a move that was saved and has already re-counted the registrar's progress; pressing Save again finds
  the picker on the old pin. Fix: wrap `:630` alone (the page has `RereadChoicesAsync` for exactly this, `:659`; call it) and keep the success.

### S3. Add on a version the institution has superseded says the item "does not exist"
- `Curricula/CurriculumItemPage.razor:256` (`!_curriculum.MayAddItem` on `/items/new`) → `PageState.ItemUnavailable`; words
  `CurriculumItemsWords.cs:177-179`: "Item unavailable" / "This curriculum item does not exist, or you cannot change it." with "Go to Items of …".
- User who types or bookmarks `/admin/curricula/9/items/new` for 11.1 after superseding it is told an item (they never named) does not exist.
  F6's own line for that state is on the list page ("Your institution has superseded this version, so no item is added to it.", `CurriculumItemsWords.cs:101`).
- Fix: for the add address with `!MayAddItem`, render the list page's sentence in the same card with "Go to Items of …", titled "Cannot add an item" (NEW,
  say so in § 6), or redirect to the list (no token). A states row for it.

### S4. A failed Deactivate… leaves the failure unfocused
- `Epas/EpaEdit.razor:570-577` sets `_errorMessage = EpaWords.DeactivateFailed` and, unlike the save's catch (`:508-513`), never sets `_focusResult`.
  The comment says the dialog returns the focus to Deactivate…; it does not on confirm (`ConfirmDialog` closes with `returnFocus:false` after an action).
- User (keyboard or screen reader) confirms, the dialog closes, the focus goes to the page body; the red alert is at the page's top, announced (role alert) but the
  focus is lost. Brief: "focus after every action and failure". Fix: `_focusResult = true;` in that catch, as the save's.

### S5. The Administrator's EPA pager disappears when the chosen size holds every row
- `Epas/EpasList.razor:116`: `@if (list.Epas.Count > _pageSize)`. At 40 EPAs the Administrator who picks 50 per page loses the pager, size select
  included, and cannot return to 20 or read how many there are. This is T390 (five lists already have it); P5 adds the sixth. The design's pager always
  keeps its size select once it is drawn.
- Fix: draw `PagerControls` while `list.Epas.Count > 20` (the smallest size), not `> _pageSize`; and fix T390 in one go.

### S6. An exception's text can still reach the user on pages this flow touched
- `Trainees/TraineeProfileEdit.razor:645` (`catch … InvalidOperationException` → `RefusalText.Of(exception)` → `exception.Message`), same at `:890` (the end's refusal);
  `EntrustmentScales/EntrustmentScalesList.razor:145` (delete). `RefusalText` prints any `InvalidOperationException`'s message; a bug that throws one
  ("Sequence contains no elements") reaches the screen. T329's rule, which the lane brief states for "these pages". The move's own refusal is now a
  `FieldRefusalException` (`:635-643`), so `:645` has no remaining reason to print.
- Fix: catch `FluentValidation.ValidationException` and `UnauthorizedAccessException` there, and `FieldRefusalException` where a field is named; everything else to the fixed
  sentence. Outside the lane but the same rule: `Curricula/CurriculumEdit.razor:193`, `:231`, `:259` (flow 16) and `EntrustmentScales/EntrustmentScaleEdit.razor:176`
  still assign `exception.Message`. File them; do not leave them unsaid.

### S7. DESIGN.md does not record the new pages' patterns
- `execution/architecture/DESIGN.md` § Page-level patterns (Unavailable, `:1903-1920`) has the EPA's "EPA unavailable" paragraph only. "Curriculum unavailable" and "Item unavailable"
  (`CurriculumItemsWords.cs:171-182`; their "Go to …" actions), the item page's record (P2: `dl.detail-list`, Back with `?back=`), Curricula's Administrator section
  (Institution and Show, the page's state not the address, P4; the h3 focus after Show), and the EPA list's pager (P5, "EPAs per page") are in no section; the row-action paragraph
  (`:938`) mentions View and nothing else of P2. Only the app.css heading list (`:5192-5195`) and the move/adoption paragraphs were written.
- The next flow (16, the College's curriculum page) will rebuild these from the boards. Fix: three short paragraphs under § Page-level patterns (Unavailable gets the two pages;
  List page gets Show and the pager; Detail page gets the record), in the same commit as the S3 fix.

### S8. "Show" with no institution chosen clears the section and says nothing
- `Curricula/CurriculaList.razor:501-508`: `chosen is null` → `_chosen = null`, lists emptied, return; the focus stays on Show and the only change is that the hint
  (`ChooseHint`) appears (or stays). A refusal under the select ("Choose an institution." NEW) and the focus on the select is the pattern (`FieldRefusal`).
  It is also how a stale choice is cleared by choosing the placeholder, so the page changes with no words and no focus move.
- Fix: when `_institutionField == 0`, show a refusal under the select, focused, and leave what is shown as it is.

### S9. The Administrator's failed Show offers no way to retry
- `Curricula/CurriculaList.razor:519-526`, `:372-378`: the danger alert holds the sentence only (`LoadFailed` = "Could not load the curricula. Nothing has changed. Try
  again, or come back in a few minutes."): it says "Try again" and has no button, unlike the page-level failure (`StatePanel` Try again, S7). Pressing Show again works,
  so this is a words and affordance gap, not a dead end. Fix: reuse `alert-row` with a "Try again" button (the S7 form) or word it "Press Show to try again."

## Nit

- **N1.** `Curricula/CurriculaWords.cs:134-135`: two `<summary>` blocks in a row on `AdoptWord`; the first belongs to `AdoptName` (`:138-141`, which carries only `<param>`s).
- **N2.** Gendered pronouns, comments only (no shown string has one): `Curricula/CurriculaList.razor:14`, `:17`, `:25`, `:130`, `:157`, `:212`, `:440`, `:515`; `Curricula/CurriculaWords.cs:20`, `:24`, `:55`; `Epas/EpaWords.cs:25` ("as he is", the College admin); `Epas/EpasList.razor:13`, `:16`; `src/Wombat.Web/Components/Pages/Dashboards/InstitutionalAdminDashboard.razor`
  (comment). The rest of the codebase says "the Institutional admin". Cheap to clean; no user effect.
- **N3.** Two sentence forms for the same failure, across the new Words: "Could not load/add/save/remove …" (`CurriculumItemsWords.cs:26`, `:132`, `:232`, `:235`; `EpaWords.cs:23`, `:107`) and "The X could not be
  saved/deactivated/adopted/deleted" (`EpaWords.cs:195`, `:198`; `CurriculaWords.cs:215`; `EntrustmentScalesWords.cs:95`). Boards draw both. Pick "The X could not be …" for actions and "Could not load …" for reads in § 8 and
  hold it.
- **N4.** `Epas/EpaEdit.razor:130` ("Saving...") and `Trainees/TraineeProfileEdit.razor:473`'s "Saving..." use three full stops; every other busy label in the flow ("Rebuilding…") uses "…".
- **N5.** `Epas/EpasList.razor:142`, `Epas/EpaEdit.razor:355`, `:508`, `:570` catch every `Exception`, including `OperationCanceledException` (a circuit teardown logs an error and writes a failure to a dead page);
  the Curricula and Items pages filter it. Add `when (exception is not OperationCanceledException)`.
- **N6.** `Epas/EpaEdit.razor:294-295` and `:300`: Deactivate… is natively `disabled="@Busy"` and Save `disabled="@_deactivating"`; the lane's own Never list forbids native `disabled` for an unavailable action ("the
  built form controls that are disabled today stay as built"), so this stands, but it drops the focus of the pressed button on a race. Note it for T234's list.
- **N7.** `CurriculumProgress/CurriculumProgressRebuild.razor:42`, `:71`: Rebuild progress opens a dialog and has neither "…" nor `aria-haspopup="dialog"`; Q16 keeps it a primary and R3-F02 draws no "…", so
  it follows the board. DESIGN.md § Button system says every trigger that opens a dialog carries `aria-haspopup` (`:758`); the two disagree. Decide in DESIGN.md (an exception sentence, or add the attribute: it costs nothing).
- **N8.** `Curricula/CurriculaList.razor:363`: the College table's Actions `th` has no `col-fit`, which the other two tables have (`:198`, `:264`); `Epas/EpasList.razor:81`, `:90`: the EPA `th` has `col-wrap--wide` without `col-wrap`.
  Neither shows at 1280.
- **N9.** `Curricula/CurriculumItemPage.razor:616`: after an Add that returns no new item for the EPA sent, the page opens the list with nothing said (`CatalogueLinks.Items`), a silent success. Unreachable in practice (the command
  returns the curriculum with the new row); log it at least.
- **N10.** `EntrustmentScales/EntrustmentScalesList.razor:86`: Delete is still a filled `btn-danger` outside a `ConfirmDialog` (T264's other half, runbook 6.8). Known and open; the lane could not touch it, but the page is now
  in flow 15's design-system version.
- **N11.** Runbook A.7.10 (`appendix-cross-cutting.md`, the Note about "Add your institution's own item" at 36 px): the step is Dr Kruger's (College admin), whose link reads "Add item"; the note describes the Institutional admin's
  link and "a replay that reaches it on her phone". Move the note to an Institutional-admin phone step (A.7.9 is Prof Mbatha's) or say it is for a later replay.
- **N12.** `coverage.md`: the `CurriculumItemPage.razor` row lists 6.26, 6.30, 6.33, A.7.10, but `states.md`'s "A national item's record" row is "After Step 1.21: View on PAED-001" and 1.21's Expect only says View
  "opens the item's record": no step presses it, so the P2 record (a page with no board of its own) is played by none. Add "press View on PAED-001 and read the record, then Back" to 1.21, or to 6.22, and add the step to the row.
- **N13.** Board and DESIGN.md disagreements noticed, all already decided or harmless: the boards' items table has a Window and weight column (decision 1); the boards draw no plus on Create EPA / Create curriculum (decision 4); the
  boards' trail markup (decision 7); `.dialog-body--scroll` on A03/A08/A11 is not rendered by action mode (round-3-check S13); `.pager-info` is a `span` on C15 and a `div` in `PagerControls` (S14). The build follows DESIGN.md in all five.
  **No new disagreement found.**

## Checked and right (no finding)

- **Curricula** (`CurriculaList.razor`, `CurriculaWords.cs`): the two Institutional-admin sections, the open section's A2 lines, Adopt vs Adopt… by `Relation`, the dialog (title, supersedes, singular and plural registrars, own items, "Adopt 11.2"),
  the result (first sentence bold, focused), the held-back refusal under the open section's heading (focused), the Administrator's All curricula (College column, Status Open / Held back, Edit only where `CanEditCurriculum`), the institution section
  (h3s named for the institution, focus on the first h3 after Show, Items links "Items of … for …", the choice the page's state and kept across an adoption), `/admin/adoptions` a redirect with `replace` and no menu item (`NavItems.cs`,
  `NavOwners.cs`). `aria-haspopup="dialog"` on Adopt… only where it asks; every `td` holds one `div.actions-cell`; no `td.actions-cell`; no inline style, hex, `<i class="bi">`, `<svg>`.
- **Items list** (`CurriculumItemsEdit.razor`): six columns, no Window and weight (decision 1), the caption, "Items (n)", own rows last, Edit and Remove… (`aria-haspopup`, named by `RowNames.Distinct`), View beside "Set by the College." (P2), the standing notices
  with `Role=""`, the paused notice, the new-version line, F6's superseded line, the Remove dialog (title, body, "Remove item", focus returns to the trigger on Cancel) and its failure; `?added=`, `?saved=`, `?back=` each read in the caller's scope, the result written and focused
  (or the Edit/View focused), a stale or foreign id writing and focusing nothing, the address then replaced without the token, and the `_readId` guard so the replace does not read again and lose the result; a reload says nothing. Load failure and Unavailable as S7/Q19.
- **Item page** (`CurriculumItemPage.razor`): h1 and tab by `CanEditCurriculum` (F3), subtitles word for word (R3-B22, B24, R2-B04, B09), the EPA select offering what the command accepts and read again after a refusal, the refusal under EPA first in `aria-describedby`, focused,
  values kept; Cancel before the submit in the DOM, "Add item" / "Save item" `aria-disabled` with the reason in the `role="status"` span (never natively disabled); Cancel to the list with no token from the add (P6) and `?back=` from an item; the record (P2) with no form and a Back link
  carrying `?back=`; Unavailable pages in the route's own place. `StageMinimaEditor` and `RungPicker` as built (decision 5). The 44 px rules at ≤640 px for the checkbox rows and the add link are scoped to the pages' own classes (`app.css`, decision 2, N9).
- **EPA list and page** (`EpasList.razor`, `EpaEdit.razor`, `EpaWords.cs`): the three readers' subtitles and captions, "Set by the College." where Edit would be (Edit only where `MayChange`), the owner mark under the title, Create EPA only with `MayCreate` (with the plus, decision 4),
  Q12's empty state with Go to Curricula, the pager's words and the h1 focus after a page turn (P5), the record (About this EPA, `dl.detail-list`, subtitle word for word, no form, no Save, no Deactivate…), "EPA unavailable" with no id, `?created` writing "EPA saved." focused then stripping,
  Deactivate… `aria-haspopup="dialog"` with the dialog naming the EPA and whose trainees, Save with Active unticked asking the same question, field refusals under their fields (focused, typed values kept), `FieldRefusal.Of` mapping, and every fixed failure sentence logged.
- **Profile, rebuild, scales:** "Save profile and move…" (`aria-haspopup="dialog"` while the picker differs from the pin, decision 6), the line under the picker (`.submit-check`, a standing line, refusal first in `aria-describedby`), the dialog opening on Cancel with "Move and save" primary,
  the D06 refusal under Curriculum (focused, picker re-read, set back to the pin if no longer offered); the rebuild's failure in its result region, focused, first sentence bold; the scales' reader subtitle, "Loading the entrustment scales.", a failed read with Try again, the Delete result focused.
- **CSS** (`app.css:7338-7479`): every length on the spacing scale (`var(--space-*)`) except the two 44 px rules and the 20 px checkbox (`2.75rem`, `1.25rem`, the system's own precedent), no hex outside `:root`, no `!important`, every class defined, each rule scoped to its page's class. No inline `style=` in any changed Razor.
- **Runbook quoted words, every one against the Razor and Words files:** 1.10, 1.17, 1.18, 1.19, 1.20, 1.21, 1.22, 2.44, 6.9, 6.13, 6.17, 6.22, 6.23, 6.25, 6.26, 6.29, 6.30, 6.31, 6.32, 6.33, 6.34, 6.36, 6.37, 6.38, the Act 6 Outcome (Status Open / Held back; "Adopted on `D`", "Superseded on `D`" over "2 registrars
  still on it"), A.5.2, A.7.9, A.7.10: **all match, word for word** (the dialogs' sentences, the results' bold lead, the Status words, the caption and subtitle strings, "Items (n)" arithmetic 15, 16, 17, the aria-label names "Edit KGK-001", "Remove KGK-001"). `coverage.md`'s counts hold (332
  `### Step` headings; 80 page files, 90 `@page` templates in `src/`), and its routes, and `states.md`'s rows, agree with the built routes (`/admin/curricula/{Id:int}/items/new`, `…/items/{ItemId:int}`, the `/admin/adoptions` redirect). The Actual lines the lane left as "predates T392" are marked so.

## New words: every user-visible string that no board draws

Judged against the brief's tone (plain, short, names the person or the thing, "Nothing has changed. Try again, or come back in a few minutes." for a failure, no pronoun, no exception text). **All are acceptable in tone**; N3 is the only wording change asked.

**`Curricula/CurriculaWords.cs`**
1. `:25` "The College curricula your institution trains against, and the versions open to it." (the subtitle before the institution's name is read, R3-A01 with "your institution")
2. `:48` "No other College curriculum version is open for adoption." (an empty Open for adoption)
3. `:84-85` "‹institution› has not adopted a curriculum yet. Adopt one below before admitting trainees." (the Administrator's named empty body; A01 draws "Your institution …")
4. `:187-204` the adoption result's variants: " at ‹institution›" ("New admissions to Paediatrics / Paediatrics at ‹institution› join it."); "‹version› is superseded." (no registrar on it); "‹version› is superseded; its registrar stays on it until moved." (one registrar). The 3-registrar form is A05's.
5. `:165-177` the plural own-items line: "Your institution's own items on 11.1, X and Y, are not carried over: add them again on 11.2." and, to the Administrator, "‹institution›'s own items on …". (A03 and A11 draw the singular.)
6. `:215-217` "The curriculum could not be adopted." + "Nothing has changed. Try again, or come back in a few minutes." (the generic failure of an adoption; A07 draws only the held-back refusal)
7. Kept built, on no board: "This institution has already adopted this curriculum version." and "The selected curriculum could not be found." (`CurriculaWords.cs:210-211`, from `AdoptCurriculum.cs`).

**`Curricula/CurriculumItemsWords.cs`**
8. `:116-119` the new-version line's plural and multi-version forms: "Your ‹n› items on ‹version› stay there." (one clause a version)
9. `:132-133` "Could not remove ‹EPA code›. Nothing has changed. Try again, or come back in a few minutes." (the Remove's generic failure)
10. `:187-194` "Curriculum item" (the item page's h1 and tab while it reads or fails), "Loading this curriculum item.", "Could not load this curriculum item. Nothing has changed. Try again, or come back in a few minutes."
11. `:200-202` "On ‹version›. It applies only to your institution's trainees." (the add subtitle with no institution name to read)
12. `:216-217` "A national item on ‹version›. The College sets it, so it is read only here." (the record's subtitle, P2)
13. `:220` "Back to Items of ‹version›" (the record's link back, with `?back=`) and `:223` "View" with its name "View ‹EPA code›" (P2; the board for a national row draws only "Set by the College.")
14. `:232`, `:235` "Could not add the item. Nothing has changed. Try again, or come back in a few minutes." and "Could not save the item. …"
15. The record's labels and values (P2, from the table's headers): "EPA", "Target and decision", "Minimum level", "Minimum by training year", "Completion window", "Weight", "Tools"; "‹n› months" (`:262`); the weight's "—" (`:265`); "No year has its own minimum." (`:268`, StageMinimaEditor's own words); each year's "Year ‹n›: ‹rung›" (`:316-317`).
16. `:300` the table cell's gap form "Year 3: 3b / Year 4: 4" when the years do not run 1, 2, 3 …
17. `Curricula/CurriculumItemPage.razor`: "(inactive)" after an inactive EPA in the picker is built; "Suggested because it is the default scale of ‹sub-speciality›." (`:748`; the second reason, built, T109) is on no board (B22 and B24 draw the first).

**`Epas/EpaWords.cs`**
18. `:198` "The EPA could not be deactivated. Nothing has changed. Try again, or come back in a few minutes."
19. `:33`, `:98`, `:155`, `:174` the fallbacks when an institution's name cannot be read: "your institution" in the subtitle, the create subtitle, the Active help and the Deactivate body.
20. `:130` "Saving..." (built; N4).
21. **`src/Wombat.Application/Features/Epas/CreateEpa.cs:38-60` `EpaFieldMessages`**, shown under an EPA's fields and drawn on no board: "Choose a sub-speciality.", "Enter a code.", "A code is at most 64 characters.", "Enter a title.", "A title is at most 200 characters.", "A description is at most 4,000 characters.", "The required knowledge and skills are at most 8,000 characters.", "Choose a category.";
    and the Administrator's form of `NotAdopted`: "‹institution› has not adopted ‹discipline›, so an EPA cannot be added to it." (C07 draws "Your institution has not adopted …").

**`Trainees/TraineeMoveWords.cs`**
22. `:36-38` "‹name› is on ‹curriculum›, which is not the version your institution has adopted. Keeping it keeps ‹name› on ‹version›." (D01's case with no day to name)

**`EntrustmentScales/EntrustmentScalesWords.cs`**
23. `:95` "The scale could not be deleted. Nothing has changed. Try again, or come back in a few minutes."
24. `EntrustmentScalesList.razor`'s "Entrustment scale deleted." is built.

**Findings that would add words** (to be written into § 6 with the fix): M1's two sentences, S3's, S8's "Choose an institution.", S9's "Press Show to try again." or the Try again button.
