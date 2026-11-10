# Flow 17 build review: the pages and the runbook (T398)

Reviewed 2026-11-10, branch `t398` (`68e127d8`), worktree `C:\dev\Wombat\.claude\worktrees\t398`. Read-only (nothing committed, no source changed).
Paths are relative to `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/` (`ATE` = `ActivityTypeEdit.razor`, `ATE.cs` = `ActivityTypeEdit.razor.cs`) unless they start `src/`, `execution/` or `design/`.

**Method.** Static review plus two runs.
- Read in full: `ATE`, `ATE.cs`, the four words files, `BuilderRefusal.cs`, `ActivityTypesList.razor`, the three read-out components, the diffs of `app.css`, `wombat.js`, `RefusalSummary`, `FieldRefusals`, `ActivityForm`, `NewActivity` and DESIGN.md, the lane notes, `sweep.md`, and the runbook steps 1.24 to 1.31, 6.14a, A.7.9, A.7.14 and `states.md` :902-:920.
- Words: a script took every string literal of the four words files (placeholders as wildcards) and looked for it in the text of all round 1, 2 and 3 boards; the misses are the NEW list below. A hand check of the razor's literal text and `aria-label`s found nothing the words files do not hold.
- Counts: traced against `ListActivityTypesAdminQuery` (the read rule), `ActivityTypesListWords.Ordered`/`CountLine`/`NoMatchBody`, and the runbook's own order (Act 1 leaves 22 for Prof Mbatha before 1.26 and 23 after; Dr Kruger reads 22 at 6.14a, 23 after his draft; Prof Mbatha stays at 23 because an unpublished draft she may not write is unreadable to her).
- Run: `dotnet test` Web.Tests filtered to everything flow 17 and its shared parts (826 passed, 0 failed), and flow 03's shared parts (`RefusedFieldMarking`, `NewActivitySubmitFlow`, `ActivityForm`, `ActionFocus`, `AccessibleNames`, `PageTitle`, `IdReferences`, `ActiveNavItem`, `RowNames`: 556 passed, 0 failed). No bUnit page of my own was needed.
- **Not done:** nothing was rendered in a browser; widths and focus are read from markup, CSS and `sweep.md`, not measured. Lane A's words (Application) were read only where a runbook step quotes them.

**Counts: 0 must, 4 should, 12 nit. NEW words: 31 groups** (list at the end).

## Must

None. Every journey the runbook steps 1.24 to 1.31, 6.14a, A.7.9 and A.7.14 play is built, each word they quote is on a board or in the NEW list, every count traces, and flow 03's filing page and summary render as before (additive parameters only, `RefusalSummary.razor` diff, `ActivityForm.razor` `EpaOptions ?? await ...`).

## Should

### S1. The list's Actions column is 224px for one 60px button, so Set by, Published and Draft wrap for nothing
- `ActivityTypesList.razor:112` and `:125` give the header and every cell `class="col-actions"` alone. `app.css:407` makes that 12rem ("room to hold three small buttons"); `app.css:6577` ships `.col-actions--single { width: auto; }` for exactly this, "a row's one button (Admit..., Edit)", used by `AssessorsList.razor:101,119` and `PendingTraineesList.razor:126,139`.
- `sweep.md` ("Every column at 1280") measures 224px for the column, and Set by wrapping its scope line to two lines on nearly every row for all three people, and Published and Draft wrapping "Not published" and "Saved 2026-11-05" for Dr Kruger and devadmin. The sweep calls it "not a width fault"; it is a layout fault the shared class cures (about 164px back to the other columns, and the 1025px spare grows from 40-55px).
- Verdict on the sweep's note: **real defect, small.** Fix: add `col-actions--single` to the `th` and `td`; re-measure at 1025 and 1280.

### S2. A section's key and title carry no refusal: the message sits in the head, the input knows nothing of it
- `ATE:437-440` draws a section's words (`p.validation-message#sec-N-refusal`) in the head; `ATE:623-628` (`#section-key`, `#section-title`) have no `aria-invalid` and no `aria-describedby`. A refusal line for a blank section key ("Form, section Section 2: Enter a section key.", NEW), a duplicate section key (NEW) or an empty section links to `TargetIdOf` = `section-key` (`ATE.cs:927`) and opens the section's editor there. A screen reader user lands in an unmarked, undescribed input; the field editor's key does it properly (`ATE:703-707`, `FieldRefusal.DescribedBy`).
- The required-fields rule in this review's brief ("the words at the place") holds for Details, fields and the JSON, not for a section's editor.
- Fix: give `#section-key` `aria-invalid` and `aria-describedby="sec-N-refusal"` while the section holds words (the id exists).

### S3. The pins the lanes planned for the tabs, the count and the outline were never written, nor for the sweep's two fixes
- Lane C's report: "Not done: NarrowLayoutTests pins for C1, R1, S2". `build-lanes.md` (lane C's tests) lists them. Grep over `tests/Wombat.Web.Tests/Design/` finds no `builder-page`, `.workflow-readout`, `.field-editor-inplace` or `.outline-section`; only `.tab-bar-tab[aria-selected]` (`ContrastTests.cs:279`) and the list's seven selector lists (`NarrowLayoutTests.cs:265-271`) are pinned.
- The sweep's two fixes are the ones a later edit would undo silently: the size container `app.css:2142`, the 34rem tracks `:2181`, the 46rem count word `:7953`, and `.workflow-readout` in all seven stack lists (`:7639` and six more).
- Fix: pin those four in `NarrowLayoutTests` (the same shape as the C3 pin).

### S4. DESIGN.md records the builder but not the list, nor the read-outs' shared words
- The diff adds the builder (Builder layout, Button system, the refusals paragraph) and a clause on the 1024px stack. Nothing records the list page: the search applied on Show with Clear, the count line that takes the focus, the no-match state apart from the empty catalogue, own rows first then by name, Set by in words (College by short code), "Not published" / "Saved ‹day›", the release line (Q9), View for what the caller may not write, the subtitle by role and N12. Flow 16's review (S3) asked for the same of that flow.
- Also unrecorded: the three read-outs as a pattern (the states line, "Moves, as saved", "Published versions", `Who moves it`), the Q15 results, and Unavailable's markup for this page (`ATE:40-45` is `div.state-panel-copy` + `div.form-actions`, flow 15's, fine).
- Fix: three short paragraphs under Page-level patterns (list) and Builder layout (read-outs).

## Nit

- **N1. Up and Down are natively `disabled` at the ends** (`ATE:449,451,512,514`). DESIGN.md ("An action that is unavailable is never `disabled`") reads against it. The boards draw `[disabled]` there (R3-E07 and the rest), the code is the old pattern, and `MoveSection`/`MoveField` place the focus for it (`ATE.cs:716-717, 732-733`). Accepted: it is a position, not an unavailable action with a reason; say so in DESIGN.md's Builder paragraph.
- **N2. "Its College" in the owner line** for an Administrator who picks Speciality on a new Global-started type (`ATE.cs:247-249`: `stored` is the Platform owner, so the College is null). Dr Kruger and Prof Mbatha are right (their editor carries the College's short code); only devadmin sees "Its College · Speciality · Paediatrics". The fallback word is NEW and unhelpful; the targets carry no short code, so either name it "The College" or add the code to the scope choice. Not in any runbook step.
- **N3. `dl.builder-strip` has `aria-label="Versions"`** (`ATE:183`, `ActivityTypeEditWords.StripLabel`, the boards' own) on a role-less `dl`, which assistive technology mostly ignores, and the Publish tab and a reader's Details each also hold an `h2`/`h3` "Versions". Harmless; the label could go.
- **N4. The runbook lists the groups, the page merges them.** 1.31 lists KGK's, the twelve `*_cpsa`, then the ten Demo types; `ActivityTypesListWords.Ordered` (`ActivityTypesListWords.cs:123-128`) puts the caller's own first, then everything else in one alphabetical run, so "Mini-CEX" and "Mini-CEX (Paediatrics)" are neighbours. 1.31 says "the others by name" and 6.14a "each by name", so a careful reader is right; a replayer reading the bullets as an order records a false Gap. One clause ("interleaved by name, not grouped") in each would end it.
- **N5. `design/system/README.md:268`** still says the builder's unselected tab "(`.tab-bar-tab`, pre-restructure, flow 17) is edged in `border-color`". The tab has no edge now (`app.css:2150-2166`). Lane C already flagged the re-sync.
- **N6. Lane W's fault 4 (a published type saved with only Active changed shows a draft): by design, not a defect.** `ActivityType.SaveDraft` stages whenever it is called (`src/Wombat.Domain/Activities/ActivityType.cs:88-113`, `HasDraft` at `:83`). After Q15's question the strip reads "Draft: Saved ‹day› by ‹name›", Discard draft… appears, and Publish offers version 2 with "Nothing on the form changes for activities already filed." (`ActivityTypeEditWords.NothingChanges`). Honest and board-consistent (R3-E11's results; Q17's "Draft saved. … Nothing was published."), but no runbook step or `states.md` row plays it, so the behaviour is unwritten. Add one `states.md` row, "Not yet captured".
- **N7. Lane W's fault 1 (the list subtitle names the College in full, the rows say "CPSA"): fine.** R2's board draws it (`round-2/boards-text.md:1102`, "You can change the types College of Paediatricians of South Africa sets; the others you can read."); the Set by cell is the short code by Q7. Both are right for their place. 6.14a quotes the full name.
- **N8. Lane W's fault 3 (the no-match body says "all 23", Dr Kruger reads 22): fine.** `NoMatchBody(_ordered.Count)` is the caller's own total (`ActivityTypesList.razor:94`); 1.31 and `states.md:908` are Prof Mbatha's (23), 6.14a is Dr Kruger's (22 before his draft, 23 after). Every number traces.
- **N9. Lane W's fault 2 (Add section creates a "New field", so an empty section only by deleting its last field): fine.** `AddSection` is the old behaviour (master's `AddSection` created the same field), keeps the form valid, and Delete reaches the empty-section refusal (`EmptySection`, `BuilderRefusal.cs:140-146`). The runbook's EmptySection state says how.
- **N10. The sweep's "for review" item 1 (equal tab tracks up to a 34rem column, 92px each at 768): fine.** The boards drew label-width tabs at 768 and the build cannot, because from 641 the sidebar leaves a 335px bar for 416px of labels (`sweep.md` first pass, measured: 57px sideways). Equal tracks with a 0.9375rem label and a stacked count keep one row, 44px high, no sideways scroll, every label whole (the 1px scan from 360 to 1300 passes). `app.css:2181-2195`. Accept; DESIGN.md says so already.
- **N11. The sweep's "for review" item 2 (the count's word hidden below a 46rem column, up to a ~1040px viewport): fine.** A tab reads "Form 2" there and its name is still "Form 2 problems" (the word is `clip`ped, not `display:none`: `app.css:7953-7964`; `ATE:204`), so the label-in-name rule holds, the summary above states the problem in words, and four "2 problems" counts would not fit a 692px column (710px measured). The runbook's "Form 1 problem" holds at 1280, where a replay runs.
- **N12. Not measured by the sweep** (its own list): a real refusal state, Details and Credit at 900-1150, Dr Kruger and devadmin on the builder. All three are plain flow (cards fill their column); low risk, and the next replay measures them.

## The departures the integrator accepted (judged)

| Departure | Verdict |
|---|---|
| States as one `<p>`; `HeadingLevel` draws nothing in the Workflow and Credit read-outs | Right: the boards have no heading of their own; the panel's `h2` heads them. |
| "Who moves it" for the column | Right: on R2-B04. |
| Versions ascending; By column dropped when no names | Right, with one note: ascending is the board's and the table's reading order; a column of blanks would be worse. |
| "Credits nothing." with its line | Right (R2-D03). |
| The load error not bolded; the College admin's subtitle after load; no count line on no-match (the card takes the focus) | Right: the card is `tabindex="-1"` with a `role="status"` title (`ActivityTypesList.razor:94-100`) and Show focuses it. |
| `h2` "Live preview" (spec text says "Preview") | Right: "Live preview" is the heading on 27 R3 boards; "Preview" is the Show radio's label. |
| "From Draft to Logged or Cancelled; 2 moves" without the arrow | Right: `build-lanes.md:89` bans "→" as shipped text. |
| Tab square-topped; section head `border-start-*-radius`; badge via `BadgeFor.State`; old control ids kept; positional ids | Right. No collision with the preview's ids (`ActivityFieldIds.Input` = `‹key›-in`). |
| The section editor keeps `role="group"` | Right: it is a `div`, not an `li`; check item 1 concerned the field editor `li`, which has none (`ATE:649`). |
| Reader's View/Close; dialogs writers only; client-side Details and blank-key checks | Right; "Close section ‹x›" is NEW (below). |
| Add section opens its editor | Right (S13 for sections). |
| A failed retry's focus stays on the alert | Right. |
| "Nothing was published."; the pinned line's clause dropped with no field removed | Right; both NEW. |
| A `field:` rule on Form has no at-place words | Right: it is a Workflow refusal (`BuilderRefusal.cs:176-189`). |
| `:root` `--radius-pill` comment edited | Right (`app.css:98`, DESIGN.md `:237`). |

## Checks that came out clean

- **Tabs** (`ATE:197-206`, `ATE.cs:435-452`, `wombat.js:189-200`): `role="tablist"` with a label, `role="tab"`, `aria-selected`, roving `tabindex`, `aria-controls` on the selected tab only, no `aria-label`/`aria-describedby` on a tab, the count inside the name. Left, Right, Home, End move the focus (wrapping); Enter and Space are a button's own click. The listener prevents those four keys only and only inside `.tab-bar[role="tablist"] [role="tab"]`; Tab, Shift+Tab, Enter and Space are untouched. Panels: `aria-labelledby` the tab; `tabindex="0"` only on a panel holding no control (a reader's Details, Workflow, Credit: `ATE:236, 780, 797`); none on a writer's.
- **Required** : `aria-required="true"` on Key, Name, the scope select, the scope group, Section key, Title, Field key, Label, Type, and on Options for Choice and Multi-choice only; each editor and Details open with "* marks a field you must fill in." (`ATE:260, 621, 700`). Description and the settings carry none.
- **Names with their visible words:** Edit, Done, View, Close, Up, Down, Delete, Hide fields, Show fields, Add field, the list's Edit and View, the tabs, the radios; each aria-label contains the visible word (Up and Down as "…up", "…down": case-insensitively, as WCAG 2.5.3 allows).
- **No inline `style`, no hex outside `:root`, no `bi bi-`** in the razor files or the CSS diff (`StylesheetRuleTests` pass); stripes in a contrast theme use `Highlight`/`CanvasText`. Publish off is `aria-disabled` with its reason (`ATE:136-143, 899-907`), inert (`ATE.cs:1190`). The only native `disabled` are the T234 in-flight race (accepted by DESIGN.md) and N1.
- **Focus after each action:** Save and Publish (result), a refusal (the summary, `ATE.cs:921`), the first save (the result after the created token, `ATE.cs:337-345`), Edit (first input), Done (that row's Edit), Add field and Add section (the new key), Up and Down (the moved button, or its twin at an end), Delete (next row's Edit, else Add field or Add section), a line in the summary (tab, editor, key), Show and Clear and Try again on the list, Cancel on every dialog (`ReturnFocusId`, and `ShowAsync(trigger)` for Save and Publish, so Cancel on Q15 returns to the button pressed: the flow 16 S2 trap is avoided here).
- **Publish order** (`ATE.cs:1188-1310`): Q15 first, then save, then the warnings' question (one dialog at a time), then publish; Cancel and Escape both reach `PublishQuestionClosedAsync` once (`_pendingPublish` is nulled by Confirm first), saying "Draft saved. Nothing was published." only when something was saved.
- **Flow 03:** `RefusalSummary` branches on `LineAction.HasDelegate && TargetId`; flow 03 passes neither. `ActivityForm`'s `EpaOptions` is null there. `NewActivity`'s `?? throw` stays inside its catch (`NewActivity.razor:307-330`).

## Runbook against the build (steps 1.24 to 1.31, 6.14a, A.7.9, A.7.14, `states.md` :902-:920)

Every Expect checked against the code and the query. No wrong word, no wrong count. Specific traces:
- 1.24: subtitle `InstitutionSubtitle`; "12 of 22 activity types match “cpsa”." (`CountLine`), Set by "CPSA" over "Speciality · Paediatrics" (`OwnerOf` + `ScopeDetail`), "Version 1", "None", View (no `Edit` for a role that cannot write the College's types), "Written by the MSF release." between name and key.
- 1.25: header `_editor.Name`, owner line "CPSA · Speciality · Paediatrics", four tabs, the notice with no role, the strip with "Wombat's catalogue" (the seeder id maps to `SeederName`, `GetActivityTypeEditorQuery.cs:38, 199`), the settings as text by label, `FieldTypeLabel`/`NamesA` rows, the states line, "Moves, as saved", "Counts 1 towards the curriculum item for the EPA in EPA, at the level … records." (`ActivityTypeReadout.cs:101`).
- 1.26 to 1.30: Details first, the read-out Scope, `?created` arrival with "Draft saved." focused and the token stripped, "Form 1 problem", the line landing on the last holder (Audience), "Workflow, move Cancel:" (the move's label falls back to "Cancel" from the key when the typed JSON has none: `BuilderRefusal.cs:229-262`), the words under the JSON, a publish that keeps its tab.
- 1.31 and `states.md:908`: 23 rows, "1 of 23 activity types match “KGK”.", "all 23" (Prof Mbatha).
- 6.14a: 22 rows before the draft, 23 after; the ten `*_cpsa` first; msf and learner-feedback View with the release's notice; the radios offering Speciality and Sub-speciality; the owner line following the scope; Prof Mbatha's 23 unchanged and the address Unavailable.
- A.7.9 and A.7.14: one row of five tabs, Show's choice, every control 44px (sweep); Publish off with its reason beside it.

## Words on no board (NEW), 31 groups

Lane A (Application, quoted by 1.25):
1. "Counts ‹n› towards curriculum item ‹id›"
2. "…towards the curriculum item in ‹field label›"
3. "…towards no curriculum item"
4. ", at level ‹fixed›"
5. "Use one of: Institutional admin, Coordinator, Committee member, Assessor, Trainee."
6. "a release of Wombat" (the fallback in "Written by a release of Wombat.")

Lane B:
7. "1 activity type."
8. "‹n› of 1 activity type matches “‹q›”."
9. "The search is kept above. Clear it to see the one activity type."
10. A College admin whose account names no College: the lead sentence alone
11. "A, B or C" for three or more From states ("Draft, Requested or Returned")
12. "Its College"
13. "Its institution"

Lane C:
14. "Enter a section key."
15. "Enter a field key."
16. "The credit rules are not valid JSON. Check the brackets and commas near line ‹n›."
17. "The workflow is not valid JSON. Check the brackets and commas." (no line)
18. "The credit rules are not valid JSON. Check the brackets and commas." (no line)
19. "Key ‘‹key›’ is also the key of section ‹title›. Give each section its own key."
20. "A User field names a person, so it offers no options. Clear its options."
21. "Move ‹move› cannot be read: ‘‹field›’ is not a field of this form, so it names nobody."
22. "Version ‹n› could not be published. Nothing has changed. …"
23. "The draft could not be discarded. Nothing has changed. …"
24. "Draft saved. Nothing was published."
25. "Draft saved. ‹name› is inactive: registrars are no longer offered it. Nothing was published." (Q17)
26. "Published version ‹n›. ‹name› is inactive: …" (Q17)
27. "From ‹initial› to ‹others›; ‹n› moves" (and "‹initial›; ‹n› moves")
28. "‹n› credit rule(s)"
29. "Version ‹n›, ‹day›" and "Saved ‹day›" with no publisher or saver named
30. "Close section ‹title›" (the reader's; "Close field ‹label›" is on R2)
31. The pinned line with no field removed: "Activities already filed stay on version ‹n›. Only activities filed after the publish use version ‹n+1›."

Not NEW though lane W listed them as unconfirmed: "Set by the College." and "Set by its institution." (T300's, on master); the key-taken summary wording (the command's built sentence).
