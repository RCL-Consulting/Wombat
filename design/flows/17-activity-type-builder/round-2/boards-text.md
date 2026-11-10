# Flow 17 round 2: the text of every board

Generated from `round-2/R2-*.dc.html` (T398, build-contracts § 0) by a stdlib script, run with `python -I`, as flow 16's was: style, script, svg, head and helmet blocks stripped. Each board opens with its label strip: its `<title>` as `Label:`, then any `cv-chip` or `anno` chip after `>`. Kept: headings, labels, aria-labels, placeholders, input values, alt text, `role`, `[checked]`, `[disabled]`, `[aria-disabled]`, `[selected]`, `[aria-selected]`, `[aria-haspopup]`, `[aria-current]`, `[aria-required]`; a link is marked `[link]`, a button `[button]`. The order is by id: the Spec, Steps and check boards, then A, B, C, D, E, each at 1280 then its other widths (the widest first). Design data: a label strip, a chip, a caption saying Illustration, FIXTURES, DATA or PROPOSED is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R2-0-spec.dc.html
Label: R2-0-spec · Spec (1 of 3)
### Flow 17 · Round 2 · Spec (1 of 3)
Structure A at full fidelity on the Wombat v49 design system (namespace wombat). Round 1's 17 boards are on the page "Round 1", untouched.
First: the reference files opened, all 39. I read them from this canvas's own project files before drawing.
The 30 captures under project/ref/baseline/, every one opened and read whole (the tall ones in parts): act-1/1.24-1-types-search-cpsa.png, act-1/1.25-1-mini-cex-form.png, act-1/1.25-2-mini-cex-metadata.png, act-1/1.25-3-mini-cex-workflow.png, act-1/1.25-4-mini-cex-credit.png, act-1/1.25-5-overall-level-field-editor.png, act-1/1.26-1-new-type-default.png, act-1/1.26-2-first-save-no-status.png, act-1/1.27-1-duplicate-key-refused.png, act-1/1.27-2-form-saved-preview.png, act-1/1.27-3-user-field-names-a.png, act-1/1.28-1-field-rule-refused.png, act-1/1.28-2-workflow-saved.png, act-1/1.29-1-credit-tab.png, act-1/1.30-1-published-v1.png, act-1/1.30-2-metadata-key-locked.png, act-1/1.31-1-types-all.png, act-1/1.31-3-types-search-nothing.png, act-1/1.31-4-types-390.png, act-6/6.14a-1-kruger-activity-types.png, act-6/6.14a-2-kruger-msf-cpsa-open.png, act-6/6.14a-3-new-type-scope.png, act-6/6.14a-6-list-with-draft.png, act-A/A.7.14-6-builder-publish.png, act-A/A.7.9-6-builder-390.png, states/activity-type-edit--discarded.png, states/activity-type-edit--loading.png, states/activity-type-edit--not-found.png, states/activity-type-edit--publish-warnings.png, states/activity-types-list--loading.png. None failed to open.
The 9 seed files under project/ref/seeds/, every one read: mini_cex_cpsa/schema.json, mini_cex_cpsa/workflow.json, mini_cex_cpsa/credit.json, reflective_exercise_cpsa/schema.json, reflective_exercise_cpsa/workflow.json, reflective_exercise_cpsa/credit.json, teaching_session/schema.json, teaching_session/workflow.json, teaching_session/credit.json. msf_cpsa's and learner_feedback_cpsa's forms are not among them; their words come from your FIXTURES, and the MSF help texts on R2-B06 from capture 6.14a-2 (the page's own record).
Nothing was drawn from memory. The words in the captures are today's pages (Metadata, v1, v0, key · enum, "seed:cpsa:scale:v11.1", Back to list, the red Delete), read as defects, not as the target. The states/ captures (2026-09-26, the old frame) were read for their bodies only.
Also read: the Wombat design system's README and the previews of MainLayout, MainLayoutPhone, NavMenu (and its README), PageHeader, TabBar, StatePanel, RefusalSummary, FilterBar, ConfirmDialog, DataTable, FormField, ActivityForm, Alert, Icon, Users and UserPage, and bundle.css, so the shell and every part are the system's own markup.
#### Illustrations
Boards for states the replay never reaches, each titled and chipped "Illustration": R2-A09-create, R2-A09-nocreate, R2-A12, R2-B07, R2-B08, R2-C07, R2-C08-new, R2-C08-list, R2-C15-before, R2-C15-after, R2-C17, R2-C18, R2-C19, R2-C20, R2-D04, R2-E04, R2-E05, R2-E06, R2-E07, R2-E09, R2-E10, R2-E11, R2-E12, R2-E13, R2-E15 (and their 390 and 768 twins).
#### Decisions
The decisions (Q1–Q16), each with the boards that draw it
| Q | Decided | Boards |
| Q1 | Tabs as the tabs pattern; Details first on a new type; a Publish tab; Publish kept in the header | every builder board; R2-C01, R2-E01, R2-E03, R2-E04 |
| Q2 | JSON stays the editor (textarea.form-control.font-mono); the read in words above it, from the saved JSON; a reader gets no JSON; no diagram | R2-B04, R2-B05, R2-D01–D05 |
| Q3 | Versions: number, the SA day, publisher; "Activities filed under a version stay on it."; no counts | R2-B03, R2-E01, R2-E03, R2-E04, R2-E06 |
| Q4 | Editor in place under its row, one at a time, Edit → Done; focus to the first input, back to Edit | R2-C10–C14, R2-C17, R2-B02 (a reader's View → Close) |
| Q5 | At 390 and wherever the columns stack (to 1115px): Show, "Edit the form" / "Preview" (a reader: "The form") | R2-C09-768, R2-C16-768, R2-C21-390, R2-C21-768, every builder 390 board |
| Q6 | RefusalSummary, lines linked across tabs; words at the place; today's sentences kept; commands return the place (DATA) | R2-C14, R2-C15-before/after, R2-C20, R2-D01, R2-D04 |
| Q7 | Name with key under it; Set by in words; Version 1 / Not published; Saved ‹date› / None; StackWide; Show and Clear | R2-A01–A12 |
| Q8 | Delete outline, asks nothing; Discard draft… asks on Cancel; Publish asks only with warnings | every writer Form board; R2-E05, R2-E07, R2-E09 |
| Q9 | System-managed: read-only to everyone, the Administrator included; View; the release's notice | R2-A06–A08, R2-B06, R2-B07, R2-B08 |
| Q10 | The build folds in T334, T324's builder half, T271 item 2, T291 items 3 and 6, T326's list half, T280's tab item, T264's kind, T329's kind, § 5's faults | all |
| Q11 | The institution's list keeps every College's national and Global types; own types first | R2-A01, R2-A05 |
| Q12 | Subtitles; T300's notice word for word | every board |
| Q13 | The JSON laid out by the parser after each save; no stored text | R2-D02, R2-D03, R2-D05 |
| Q14 | Discard's two bodies; the Details line | R2-E07, R2-E09, every Details board |
| Q15 | Active newly unticked on a published type asks at Save draft; reactivating asks nothing | R2-E11, R2-E12, R2-A12 |
| Q16 | Publish saves first, works out the warnings from what was saved, asks only if any, then publishes | R2-E05 (and R2-E13 for a save that fails first) |
#### Corrections
Round 1's corrections, each with its board
| Item | Done as | Boards |
| M1 | Q12's subtitles; the owner line under the subtitle, p.muted.text-sm | every list and builder board |
| M2 | Own (Edit) types first by name, then the rest; the Administrator's by name; key under Name in ; Draft "Saved 2026-11-05" | R2-A01–A08 |
| M3 | Q9's notice with T22's second sentence; the row lines | R2-B06, R2-B07, R2-B08; list rows |
| M4 | Q14's two bodies | R2-E07, R2-E09 (results R2-E08, R2-E10) |
| M5 | The Publish tab: versions moved there for a writer, Q3's line, the warnings before the press; no counts | R2-E01–E06, R2-E15; a reader's versions on Details, R2-B03 |
| M6 | Moot: B and C are not drawn | — |
| M7 | One counting rule; 1.27 is 15 + 24; 1.26–1.30 22 + 29; Publish has no ellipsis | R2-0-steps |
| S1 | Edit, View and New activity type are a.btn links | R2-A boards |
| S2 | role=tablist/tab/tabpanel; aria-controls on the selected tab only; roving tabindex; Enter or Space selects | every builder board |
| S3 | Each panel's first heading an h2; a section's title an h3; the editor's heading an h4 | R2-B04, R2-B05, R2-B06, R2-C10–C13 |
| S4 | Done only, no Cancel; changes kept as typed | R2-C10–C13 |
| S5 | Final means terminal: Mini-CEX Completed; KGK Logged; MSF Recorded | R2-B04, R2-D02, R2-B06 |
| S6 | Unavailable in the system's words | R2-B09 |
| S7 | The result region under PageHeader at every width | every result and refusal board |
| S8 | Every state round 1 left out is a board | R2-C10, R2-C17, R2-B07, R2-E09, R2-A12, R2-C20, R2-D04, R2-C19, R2-E13 and the 768 twins |
| S9 | Every bracket the code fills is filled (FIXTURES) | all; brackets left only where FIXTURES has none |
| S10 | DATA gains SystemManaged, owner words, the null read, the preview's EPA list | R2-0-spec-3 |
| S11 | Details says what takes effect at Save draft; Active's help | R2-C01–C06, R2-E02, R2-E11 |
| S12 | The form settings' built line and EPA field help; round 1's picker help gone | R2-C09, R2-C16 |
| S13 | "Kept as you typed it" gone; T10's help | R2-D01–D05 |
| S14 | The versions' date is the SA day alone | R2-B03, R2-E03, R2-E06 |
| N1 | Moot | — |
| N2 | The heavy case's h1 and crumb the same bracketed name | R2-C19 |
| N3 | "Activity type" while loading and after a failed read, as built | R2-B10, R2-B11 |
| N4 | The editors' labels and help as built | R2-C10–C13, R2-C17 |
| N5 | The parser's role refusal by label | R2-0-spec-2 |
| N6 | The non-creator's empty state as built | R2-A09-nocreate |
| N7 | No .json-editor; textarea.form-control.font-mono | R2-D boards |
| N8 | Publish off is aria-disabled | every writer board with no draft |
| N9 | "Publish", never "Publish…" | every writer board |
| N10 | The Scope read-out is PROPOSED on its own (round 1's) | R2-C01 |
| N11 | Moot | — |
| N12 | "Could not load the activity types." | R2-A11 |
#### T1–T24
PROPOSED (THIS ASK) T1–T24, each with its board
| T | Drawn as | Boards |
| T1 | A reader has four tabs; Versions close the reader's Details tab | R2-B03 |
| T2 | New type on Details; existing on Form; a save keeps the tab | R2-C01, R2-B01, R2-C03 |
| T3 | .tab-bar restyled (C1); .tab-problem (S1) outside the tab's name: aria-label="Form", aria-describedby the count | R2-C14, R2-D01, R2-D04 |
| T4 | Measured, failed (see spec-2): round 1's outline drawn as S2 | every Form board |
| T5 | The CSS-only choice (C2) | R2-C21-390, R2-C21-768, R2-C09-768 |
| T6 | Done only | R2-C10–C13 |
| T7 | "Everything you typed is kept." | R2-C14, R2-D01 |
| T8 | The read's words | R2-B04, R2-D02, R2-D05, R2-B06 |
| T9 | The read follows the saved JSON, "Moves, as saved" | R2-D01 (reads the starting draft's Submit) |
| T10 | The JSON editors' help | R2-D boards |
| T11 | The College admin's and the Administrator's subtitles | R2-A06–A08 |
| T12 | "The platform" | R2-C07's owner line |
| T13 | Own = may change (Edit); Anton Kruger's two system-managed types sort with the rest | R2-A06, R2-A07 |
| T14 | One Publish action; aria-disabled with its reason; "Draft saved. Nothing was published." after a cancelled dialog | R2-C01, R2-E02, R2-E03, R2-E05 |
| T15 | The Publish tab with no draft; on /new; the first publish's workflow line | R2-E03, R2-E01 |
| T16 | The SA day; the publisher by name; "Wombat's catalogue" | R2-B01, R2-B03 |
| T17 | Q15's words | R2-E11, R2-E12 |
| T18 | The seed-owned note under the strip and on the Publish tab | R2-E15 |
| T19 | Settings by label with the key after; "delivered_on (not in the form)" | R2-C16, R2-C18 |
| T20 | Failure, load and loading words | R2-E13, R2-A10, R2-A11, R2-B10, R2-B11 |
| T21 | A writer's h1 stays "Edit KGK Teaching Session Log" (alternative for review: the record's name alone, flow 16's S11) | every writer board |
| T22 | "Nobody is offered it when filing an activity." | R2-B06–B08 |
| T23 | Hide fields / Show fields above 20 fields | R2-C19 |
| T24 | "an Assessor" | R2-0-spec-3 (the warnings) |

######## BOARD R2-0-spec-2.dc.html
Label: R2-0-spec-2 · Spec (2 of 3)
### Flow 17 · Round 2 · Spec (2 of 3)
#### Code changes
The changes the boards need, as code changes (each PROPOSED; a decided one says its Q)
| Where | Change |
| ActivityTypesList | Search applied with Show and Clear (no @bind:event="oninput"); the count line; the no-match state apart from the empty catalogue (T326); Q7's columns and Q11's order (T13); StackWide plus the 1024px stack (C3); Edit, View and New activity type kept as links; the subtitle by role (Q12, T11); loading line and the load error with Try again (T329). |
| ListActivityTypesAdminQuery | Each row's owner in words: the owning College's short code, the institution's name, or the platform (DATA: only the College's id is read today, :81–82); SystemManaged and its release; CanWrite false on a system-managed type (Q9). |
| ActivityTypeAdminScope.MayWrite, ActivityTypeScopeGuard | Refuse a system-managed type to everyone, the Administrator included (Q9, T334; today the scope alone, :50–67), with "Not saved. Multi-Source Feedback (Paediatrics) is written by the MSF release, so it cannot be changed here." |
| GetActivityTypeEditorQuery | Null for an unknown id and for one the caller may not read (today both throw "The activity type could not be found.", :83, :87); SystemManaged and release; the owner's words; the draft saver's and each publisher's name (DATA: ids today, :204, :211), "Wombat's catalogue" for seed-system; seed-owned (DATA, T18); the preview's EPAs by the type's own rule (DATA, T291 item 6); the JSON laid out (Q13). |
| WorkflowParser, CreditRulesParser | An indented Serialize (Q13), used on load and after each save; no store change. Malformed JSON: "The workflow is not valid JSON. Check the brackets and commas near ‹line›." in place of the exception's text. |
| SaveActivityTypeDraftCommand and the parsers | A refusal carries its place: field key or keys, section key, move key, directive number (Q6, DATA). |
| ActivityTypeEdit | The tabs pattern on .tab-bar (Q1, S2, T280); Details first on /new (T2); the Publish tab (Q1, M5, T1, T14, T15); no Back to list; Q12's subtitle and the owner line; the version strip; the editor in place, one at a time, Done (Q4, S4; SelectSection no longer opens the first field, :1015–1019, and nothing opens at load, :839–842); Delete btn-outline, asking nothing (Q8; today btn-danger, :284, :310); settings, Names a and types by label (T324; today keys and raw roles, :189–236, :301, :375, :467); the scale picker resolving a seed key (T271 item 2, :487–493); the read in words and no JSON to a reader (Q2); the first save's result by a one-shot token (T291 item 3; today :900–903, :802 drop it); Discard draft… and its dialog (Q8, Q14; today :28, :965–992 discard at once); Publish per Q16 (today :943–960, :1107–1119); Q15's question; RefusalSummary with linked lines (Q6); failures in fixed words (T393; today RefusalText.Of, :818, :910); Unavailable for null; a loading line; Publish aria-disabled with its reason as text (today a title, :30). |
| BuilderModels.GetPublishWarnings | Labels for field types, roles and the settings' fields (T324; today 'LongText', the raw role and keys, :209, :227, :359–369), with "an Assessor" (T24). |
| FormSchemaParser:351 | The role refusal names roles by label (N5). |
#### Classes
The class table: every class the boards use
| Row | Class | Kind | Why |
| C1 | .tab-bar, .tab-bar-tab | CHANGED (T3) | The builder's own pill tabs become the tabs pattern: underline on [aria-selected="true"] (.is-active goes), one row, 44px, equal tracks below 641px. Measured at 390: five tracks of 72px; "Workflow" 61px and "Publish" 48px of label, no wrap; "Form" with "1 problem" stacks the chip under the word, 44px. |
| S1 | .tab-problem | NEW (T3) | The count's chip inside a tab: text-color on danger-bg 11.81:1, its edge 5.56:1; on the selected tab's surface the same. |
| C2 | .builder-two-col's container rule; .form-container > .choice-group | CHANGED (T5) | Below 46.5rem of card the choice shows and hides the unchosen column (:has(:checked)); above, the choice is hidden. Measured: columns side by side from 1116px (rail 363px, preview 508px at 1280; card 958px). |
| C3 | .activity-types-page (page class) in the 1024px stack | CHANGED + NEW page class | The list overflowed its container at 950px (682 in 642; the Administrator's 687) and fits from 1000px; it joins flow 16's narrow-desktop stack. |
| S2 | .form-outline, .outline-section, .outline-section-head, .field-row, .field-row-label, .field-row-meta, .field-row-actions, .is-editing, .is-refused, .field-editor-inplace | NEW (T4 failed) | T4 measured first: a DataTable per section in the 363px rail leaves its label column about 93px beside four btn-sm buttons (204px) and two cells' padding; "Supervision required for this encounter" would run 5 lines. So round 1's outline, renamed nothing. |
| reuses | .page, .sidebar, .nav-*, .account-row, .breadcrumbs, .header-container, .page-subtitle, .actions-cell, .people-page, .search-container, .search-grid, .search-field, .filter-actions, .table-container, .clinic-table, .clinic-table--stack, .clinic-table--stack-wide, .col-actions, .user-page, .user-section, .detail-card, .detail-card--empty, .state-panel-title, .state-panel-copy, .form-container, .form-grid, .form-group, .full-width, .form-control, .form-select, .form-check, .choice-group, .field-help, .validation-message, .required-mark, .detail-list, .alert, .alert-row, .action-result, .error-summary, .validation-summary-errors, .refusal-summary-list, .loading-line, .skeleton, .skel-*, .activity-form, .form-section, .form-section-head, .dialog-card, .dialog-scrim, .dialog-body, .form-actions, .btn and its kinds, .muted, .text-sm, .font-mono, .visually-hidden | reuses | as shipped, never redeclared |
| K1–K3 | .cv-chip, .cv-modal-stage, .cv-doc, .cv-cols, .cv-first | canvas-only | The Illustration chip, a dialog's painted backdrop, these document boards. Kept off every component. |
#### Measured
The 900–1100px sweep (50px steps), each against its container
| What | 900 | 950 | 1000 | 1050 | 1100 |
| The list (Nolwazi Mbatha) | stacked, 592 | as a table 682 in 642: over → C3; stacked, 642 | stacked (C3), 692 | 742 in 742 | 792 in 792 |
| The list (Anton Kruger) | stacked | as a table 682 in 642 → C3; stacked | stacked (C3) | 742 in 742 | 792 in 792 |
| The list (devadmin) | stacked | as a table 687 in 642 → C3; stacked | stacked (C3) | 742 in 742 | 792 in 792 |
| Moves, as saved | stacked, 542 | 592 in 592 | 642 in 642 | 692 in 692 | 742 in 742 |
| Published versions | stacked | fits | fits | fits | fits |
| The builder's two columns (C16, C19) | stacked, rail 528 in a 592 card, the choice shown | stacked, 578 | stacked, 628 | stacked, 678 | stacked, 728; side by side from 1116 |
- Every column of the list at 1280 (no name past two lines; no empty band beside one): Nolwazi Mbatha Name 313, Set by 191, Published 101, Draft 68, Status 75, the action 224 — one name wraps, "Portfolio and Logbook Review (Paediatrics)", to two lines; Anton Kruger 319 / 185 / 101 / 68 / 75 / 224, the same one; devadmin 299 / 154 / 112 / 108 / 75 / 224 (Draft wider for "Saved 2026-11-05"), the same one; a system-managed row adds its release line and the key sits under the name, so the tallest cell (Multi-Source Feedback (Paediatrics)) is name, line and key, no band beside it. The action column's 224px is the table's slack: it holds one Edit or View; a .col-fit there would hand it to Name.
- Header actions stay beside the title block at 1280 and 1100 on the list (Anton Kruger's T11 subtitle, the longest, included) and on the builder (Save draft, Publish, Discard draft…). At 390 they take the row, each 44px (.header-container's phone rule).
- The 390 scope that gives 44px: .people-page on the list (buttons, inputs, selects), .filter-actions (Show and Clear full width), StackWide's row buttons (from 900px; from 1024px with C3); .user-page on the builder (its buttons, inputs, selects, a choice-group radio's label, and a lone .form-check row: Active, Required); C1's tabs; the outline's row buttons (S2's phone line); .dialog-card's stacked actions. Measured at 390: no control under 44px but the radios' 16px input and Active's 20px box, each inside its 44px label row.
- At 768: the list stacks (C3 and StackWide); the moves and versions tables stack (≤900); the builder's columns stack with the choice of two shown (R2-C09-768, R2-C16-768, R2-C19-768, R2-C21-768). Nothing scrolls sideways at 1280, 768 or 390 on any board (measured, each board's scrollWidth against its width).
- At 100 rows (no pager, as built): the list is one table of 100 rows, the search and Show the way through it; a pager would come with PagerControls if the catalogue grows past a page.
#### DESIGN.md
- § Builder layout (rewritten): the tab bar is a tablist (role=tablist, role=tab with aria-selected, a roving tabindex, Left/Right/Home/End, Enter or Space selects; T280 recorded as "the tabs pattern"); a writer's five tabs, a reader's four; a new type opens on Details, an existing one on Form; the version strip (dl.detail-list) above the tabs; the Publish tab; a field's or section's editor opens in place under its row, one at a time (S2); where the columns stack, the CSS-only choice "Edit the form" / "Preview"; the workflow and credit read in words above their JSON, a reader's whole view; the JSON laid out by Wombat after each save; Delete an outline button.
- § Button system: Discard draft… asks (ConfirmDialog on Cancel); Delete asks nothing; Publish asks only with warnings and saves first; Active newly unticked on a published type asks at Save draft; Publish off is aria-disabled with its reason in text beside it.
- § Form system: a refusal's summary links across tabs, each line prefixed by tab and place; its words at the place; the form settings by label.
- § Table system: the list StackWide and in the 1024px stack; the moves table; the fields as round 1's outline (T4 measured).
- § Page-level patterns: Unavailable gains Activity type; no Back to list beside a trail; the loading and failure words.
#### Tokens and contrast
- Every colour is an existing token (tokens.css v49).
- Selected tab: secondary-color underline on surface 4.86:1; its label primary-color 10.98:1. Unselected: text-color 12.63:1; hover-bg ground.
- .tab-problem: text-color on danger-bg 11.81:1; edge danger-color 5.56:1 on its tint.
- Publish's reason, the scope line and the owner line: muted-text on surface 5.09:1, on background 4.83:1.
- White on Publish (btn-success) 5.88:1 (A.7.14); off, aria-disabled at 65% and exempt.
- The refusal stripe: danger-color on surface 5.95:1; the editing stripe secondary-color 4.86:1; the focus ring 4.86:1 on surface, 4.61:1 on the page.

######## BOARD R2-0-spec-3.dc.html
Label: R2-0-spec-3 · Spec (3 of 3)
### Flow 17 · Round 2 · Spec (3 of 3)
#### DATA
DATA: what a page needs that it does not read today, by query
| Query | Read |
| ListActivityTypesAdminQuery | The owner's words per row (College short code, institution name, the platform); SystemManaged with its release; CanWrite false when system-managed. |
| GetActivityTypeEditorQuery | Null for unknown or unreadable (Unavailable); SystemManaged and release; the owner's words; the draft saver's name; each publisher's name, "Wombat's catalogue" for the seeder; seed-owned (T18); the preview's EPA list by the type's own rule (the fifteen PAED EPAs, not EPA-001); the JSON laid out. |
| SaveActivityTypeDraftCommand | Each refusal's place: field key(s), section key, move key, directive number. |
| Read today, not DATA | The draft's date (DraftUpdatedOn) and "None"; each version's number and date. |
#### Messages
Every message by kind, with its words and where they come from
| Kind | Words | Source |
| result | "Draft saved." · "Published version 1." · "Published version 2." · "Draft discarded." | built |
| result | "Draft saved. Nothing was published." · "Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it." · "Draft saved. KGK Teaching Session Log is active again: registrars are offered it." | PROPOSED (T14, T17) |
| refusal (summary) | "Nothing was saved." + "Everything you typed is kept."; lines "Form: Field key 'topic' is used by 2 fields (…); give each field its own key." · "Workflow, move Cancel: Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a field: rule must name a User field." · "Form, section Section 2: Section 'section_2' must contain at least one field." · "Credit, rule 1: Credit rule 1 reads its EPA from field 'epa', which is not a field of the form." | built sentences after a prefix; T7 |
| refusal (at the place) | "Key 'topic' is also the key of Audience. Give each field its own key." / "… of Topic. …" · "Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody." | round 1's, accepted |
| refusal (at the place) | "This section has no fields. Add a field, or delete the section." · "Rule 1 reads its EPA from 'epa', which is not a field of this form. Use the form's EPA field, 'epa_id'." · "Not saved. Multi-Source Feedback (Paediatrics) is written by the MSF release, so it cannot be changed here." | NEW (your message) |
| refusal (at the place) | "Date delivered is no longer in the form. Choose another Date field, or None." (R2-C18, under "delivered_on (not in the form)") | NEW (this round's; for review) |
| refusal (guard) | "You do not have permission to modify activity types in that speciality." | built, never reached |
| failure | "The draft could not be saved. Nothing has changed. Try again, or come back in a few minutes." · "Version 2 could not be published. …" · "The draft could not be discarded. …" | NEW (flow 16's sentence) |
| load error | "Could not load the activity types. Nothing has changed. Try again, or come back in a few minutes." · "Could not load this activity type. …" | flow 16's words (T20); replaces the exception's text |
| loading | "Loading the activity types." · "Loading this activity type." | T20 |
| empty | "No activity types" / "Create the first activity type to start the builder." · "No activity type is open to you yet." · 'No activity types match "zzz".' / "The search is kept above. Clear it to see all 23." | built; round 1's (accepted) |
| unavailable | "Activity type unavailable" / "This activity type does not exist, or you cannot open it." / Go to Activity types | the system's (replaces "The activity type could not be found.") |
| notice | T300's three; "Written by the MSF release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity." (and the learner-feedback release's); the no-scope notice | built; Q9 + T22 |
| hint | "Save a draft to publish." (beside Publish, aria-describedby) · "There is no draft to publish: version 1 is what registrars file. Save a draft to publish." · "Nothing is published yet. Save a draft to publish." · "Not published yet." · "Nothing on the form changes for activities already filed." | built; NEW (T15; your message) |
| hint | Details: "Saved with Save draft, not with Publish: a change here reaches everyone when you save." · Active's help (T17) · the JSON help (T10) · "While this draft is saved, updates from Wombat's catalogue stop reaching this type, until you publish or discard it." · "Once you publish it, updates from Wombat's catalogue no longer reach this type." | Q14; T17; T10; T18 |
| hint | Credit with none: "Credits nothing." then "An activity of this type counts towards no curriculum item, and its encounter date may not be after today." | this round's sentence for "its line" (for review) |
| hint | The preview's line "In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar." | round 1's, accepted |
| question | "Discard the draft of KGK Teaching Session Log?" + Q14's published body · "Discard the draft of CPSA Case Presentation Log?" + Q14's never-published body · "Publish version 2 of KGK Teaching Session Log?" + "Publishing makes this draft version 2. It changes:" · "Deactivate KGK Teaching Session Log?" + T17's body | Q8, Q14, Q16, T17 |
| warnings | "Section 'Request' will be removed." · "Field 'Audience' will be removed." · "Field 'Learning objectives' changes type from Long text to Text." · "Field 'Topic' becomes required." · "Field 'Supervising consultant' will name a Coordinator instead of an Assessor. Activities already filed keep the person they named." · "This form will stop recording which field carries the encounter date." · "Field 'Date delivered' will become the encounter date for this form." · "The encounter date moves from field 'Date delivered' to 'Date of session'." | built, by label (T324, T24) |
#### Edge cases
Edge cases
| Case | Answer | Board |
| A type with no sections | The parser refuses it: "Schema must contain at least one section." | words only |
| A section with no fields | Refused at save, at the section's head and in the summary | R2-C20 |
| A 40-field form | Sections fold (T23); a long label wraps clear of its buttons; a 12-option choice | R2-C19 |
| A field's key changed after publish | A removal and an addition in the warnings | words only |
| A setting at a deleted field | Kept, "delivered_on (not in the form)", invalid | R2-C18 |
| A draft saved by someone else | The strip names the saver, "Saved 2026-11-05 by Demo Administrator"; Save draft overwrites it, no lock | words only |
| A seed-owned type with a draft | T18's note under the strip and on the Publish tab | R2-E15 |
| A College admin with no scope | /new: the no-scope notice alone; the list: no New activity type | R2-C08-new, R2-C08-list |
| An inactive type | Status Inactive in its writer's list; Q15's question and result | R2-A12, R2-E11, R2-E12 |
| An unpublished type to one who may not write it | Unavailable | R2-B09 |
| A 2,000-character description | "2,000 of 2,000 characters"; the read-out wraps (.user-page .detail-list dd: overflow-wrap anywhere) | words only |
| The starting draft's section "Details" beside the Details tab | One name for two things until the section is renamed | R2-C09 |
| Malformed JSON | "The workflow is not valid JSON. Check the brackets and commas near ‹line›." (NEW), never the exception's text | words only |
#### Accessibility
- One h1 a board (PageHeader); h2 for each panel's parts, h3 an outline section and a preview section, h4 the editor in place; no h3 without an h2 above it.
- Every table captioned without repeating its heading ("Activity types you can read or change", "Moves, as saved", "Published versions"), scope on headers, the row's th scope="row"; stacked tables keep DataTable's roles.
- Row actions named for their row, no two alike ("Edit field Audience", "Move section Teaching session up", "Add field to section Teaching session", "View Mini-CEX (Paediatrics)").
- Focus: each result on the result region; a refusal on its summary; a summary line on its place (switching tab); Edit on the editor's first input, Done on the row's Edit; Delete on the next row's Edit or Add field; Show on the count line; Clear and Clear the search on the search input; a dialog on Cancel, Cancel and Escape back to its trigger; Try again to the h1; a tab's selection on the tab.
- Help outside its label, named by aria-describedby; a refusal first in aria-describedby; a standing notice an Alert with no role; role="status" only on what arrives with a result.
- No gendered pronoun on any board.
#### Later, not built
- T019-b drag-and-drop reorder (Up and Down stay the keyboard path)
- T019-c nested and repeatable sections
- T019-d a visual workflow editor
- T019-e a visual credit-rules editor
- T019-f visibility on several conditions
- T019-g templates and copy from a type
- T106 item 11 an input for who may write a field or section (editable_by: Mini-CEX's Entrustment and Feedback carry "field:assessor_user_id", and its Requested state too, read nowhere on these pages)
- T179 "This tool is" at publish
- T271 item 1 (the race)
- T352 item 4 (a move's label from its key)

######## BOARD R2-0-steps.dc.html
Label: R2-0-steps · Steps
### Flow 17 · Round 2 · Steps
The counting rule, used on every row: a journey counts presses + fields. A press is a click on a button, tab, link or checkbox; a field is a text typed or a choice made in a select or a radio group, and is not also a press. A menu link is a press; at 390 opening the menu is one more. A dialog's confirm is a press. A field that arrives chosen counts as none. A page the app goes to by itself (the type's own address after the first save) is no press. Flow 16 counted a box as a field; this flow counts a tick as a press, as round 1's assessment did.
11 steps: 1.24–1.31 (8), 6.14a, A.7.9 and A.7.14 (their builder parts only).
#### The Expects
Each step, its boards, and what its Expect will say once built
| Step | Boards | Do and Expect: today → once built |
| 1.24 | R2-A02 | "Each reads Scope `Speciality · Paediatrics`, Published `v1`, Draft `None` and Active" → "each reads Set by CPSA (Speciality · Paediatrics), Published Version 1, Draft None and Active"; the search applied with Show, "12 of 22 activity types match “cpsa”." above the table; Clear brings back 22; View named for the row (unchanged); New activity type offered (unchanged). Do → "search `cpsa`, press Show and read the list. Then press Clear." |
| 1.25 | R2-B01–B05 | Do "leave by Back to list" → "leave by the trail's Activity types". The h1, tab and notice unchanged; the subtitle Q12's; "opens on the Form tab" unchanged (T2); Form settings → "Date observed (observed_on), Supervision required for this encounter (overall_level) and EPA (epa_id)"; each row's type by label ("EPA", "Long text"), not "epa_id · Epa"; View opens under its row, overall_level's reading CPSA Paediatric Entrustment Scale v11.1 (now true); "Metadata, as text" → "Details, as text", Set by for Scope, with Versions (1, 2026-11-05, Wombat's catalogue); "Workflow, as a code block" → "Workflow, in words: Draft, where a new one starts; Completed, final; Submit, Complete, Decline and Cancel, the person Assessor names completing or declining"; "Credit, as a code block" → "Credit, in words: Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records."; no JSON shown; the preview's EPA picker the fifteen PAED EPAs, not EPA-001. |
| 1.26 | R2-C01–C03 | Do "open the Metadata tab and enter" → "enter". "The builder opens on the Form tab" → "on the Details tab"; the starting draft on Form (unchanged words); Scope reads "Institution · Kgosi Kgari Teaching Hospital", a read-out, "The one scope you can write in."; the first Save draft moves to /admin/activity-types/23 and reads "Draft saved." focused (now true); the strip Not published / Saved 2026-11-05 by Nolwazi Mbatha; h1 and tab unchanged. |
| 1.27 | R2-C09–C16 | Do: "edit the Details section" unchanged, through its Edit, the editor under its row; "Then correct its key" through the summary's line or Audience's Edit. The thirteen types unchanged; Names a "… InstitutionalAdmin, Coordinator, CommitteeMember and Trainee" → "Institutional admin, Coordinator, Committee member and Trainee"; the refusal word for word after "Form:" and at both fields; Form carries "1 problem"; the corrected save "Draft saved." (unchanged); the pickers by label: Date delivered (delivered_on), EPA (epa_id); the preview unchanged but the EPA picker (the fifteen, now true). |
| 1.28 | R2-D01, R2-D02 | The refusal word for word after "Workflow, move Cancel:" and its words under the JSON; the second save "Draft saved." (unchanged); the read: Log, Draft → Logged, the registrar or whoever filed it, the whole form; Cancel, Draft → Cancelled, the same, formats only; Logged final; the JSON laid out one property a line, in the parser's order. |
| 1.29 | R2-D03 | Gains "The Credit tab reads 'Credits nothing.' and the date rule in words"; the JSON `{` / `"counts_for": []` / `}` laid out. The rest unchanged. |
| 1.30 | R2-E01, R2-E02, R2-E03 | Do unchanged ("Press Publish."). "Published version 1." focused; Discard draft gone (unchanged); "Publish is disabled, with the hint 'Save a draft to publish.'" → "Publish is unavailable (aria-disabled), its reason 'Save a draft to publish.' shown beside it"; the Key a read-out, "Fixed: version 1 is published."; the strip Version 1, 2026-11-05, Nolwazi Mbatha; the Publish tab's versions one row; no warnings and no question (unchanged). |
| 1.31 | R2-A01, R2-A03, R2-A04 | Do "Press Back to list" → "Press Activity types in the trail"; "Then search `KGK`" + "and press Show". 23 rows, KGK Teaching Session Log first, "Set by Kgosi Kgari Teaching Hospital (Institution)", Version 1, None, Active, Edit; the Demo rows Set by Demo College (Speciality · General Medicine); `KGK` leaves one row, "1 of 23 activity types match “KGK”."; `zzz` reads 'No activity types match "zzz".' with Clear the search. |
| 6.14a | R2-A06, R2-A07, R2-B06, R2-C04–C06, R2-B09 | Do "open the Metadata tab and read what Scope offers" → "read what Scope offers". 22 rows, Anton Kruger's ten Edit rows first, then View; msf_cpsa and learner_feedback_cpsa View with their lines (now true, T334); MSF "with a notice that the system writes it" → "under 'Written by the MSF release. Its form changes only with a release of Wombat.'"; "headed 'New activity type', on Form" → "on Details"; Scope Speciality and Sub-speciality as radios; the save "Draft saved." focused (now true); in the list "no published version, Draft 'Draft saved'" → "Not published, Saved 2026-11-05"; Nolwazi Mbatha's "could not be found" → "Activity type unavailable" / "This activity type does not exist, or you cannot open it." |
| A.7.9 (builder) | R2-E14-390, R2-C21-390 | "editor and live preview stack in one column (…), and its tab bar stays usable" → "the five tabs sit in one row, the selected one exposed; the Form tab offers 'Edit the form' and 'Preview', so the preview is one press away; every control 44 px; nothing sideways". |
| A.7.14 (Publish) | R2-E02 | "with no draft it is disabled, and a disabled control is exempt" → "with no draft it is unavailable (aria-disabled), and exempt; its reason beside it meets 4.5:1"; white on Publish 5.88:1 unchanged. |
#### states.md
states.md rows that change
| Row | Change |
| 903 | "Draft reads 'Draft saved'" → "Saved 2026-11-05" (R2-A05) |
| 905 | "the page's empty card" → the no-match state (R2-A04) |
| 907 | narrow: stacked, Edit or View 44px (R2-A01-390) |
| 908 | the Form tab, by label (R2-B01) |
| 909 | "New, the default draft" on Details (R2-C01) |
| 910 | "Metadata tab" → "Details tab", "Draft saved." focused (R2-C03) |
| 913 | the refusal at the move and the summary (R2-D01) |
| 914 | "Credit tab": in words (R2-D03) |
| 915 | Publish unavailable with its reason in text (R2-E02) |
| 916 | warnings on the Publish tab, by label (R2-E04) |
| 917 | Discard asks first, Q14's body (R2-E07, R2-E08) |
| 918 | "The activity type could not be found." → Unavailable's words (R2-B09) |
| 919 | the loading line (R2-B10) |
| 920 | narrow: one row of tabs, the choice of two (R2-E14-390) |
#### Runbook hits for the words that change
- "Back to list": 1.25 Do and Actual (act-1-setup.md:789, :820); 1.31 Do (:958)
- "Metadata": act-1-setup.md:745 (the act's prose), :800 (1.25 Expect), :816 (1.25 Actual), :829 (1.26 Do), :952 (1.30 Actual); act-6-catalogue.md:444 (6.14a Do)
- "code block": 1.25 (act-1-setup.md:802, :804, :817)
- "on the Form tab" / "on Form": 1.25 :791 and :812 (stay true); 1.26 :836 and :849; 6.14a act-6-catalogue.md:462, :479
- "v1" in the list: 1.24 :751, 1.31 :962; "v0": 6.14a's Actual (act-6-catalogue.md:483)
- Draft "Draft saved": 6.14a Expect :468, Actual :482; states.md:903
- "could not be found": 6.14a :470, :484; states.md:918
- "Save a draft to publish." as a hint: 1.30 :947, :951
- the builder's "stack in one column" and tab bar: A.7.9 (appendix-cross-cutting.md:1708, :1732); "disabled" Publish: A.7.14 (appendix-cross-cutting.md:1872)
- "Configured activity types": no hit. Act 2's filing of the KGK log is flow 03's page: unchanged.
#### Counts
Each journey counted (presses + fields), today's beside it
| Step | Presses named | 1280 | 390 | Today |
| 1.24 | Activity types, Show, Clear; `cpsa` | 3 + 1 | 4 + 1 (Menu) | 1 + 2 (typed and cleared) |
| 1.25 | View, View overall_level, Details, Workflow, Credit, the trail | 6 + 0 | 6 + 0 | 6 + 0 |
| 1.26 | New activity type, Save draft; Key, Name, Description | 2 + 3 | 2 + 3 | 3 + 3 (the Metadata tab) |
| 1.27 | Form, Edit (section), Edit (Title), Add field ×5, Required ×4, Save draft, the summary's line, Save draft | 15 + 24 | 17 + 24 (Preview and back) | 14 + 24 (Edit section opened Title too); 23 + 24 if Done were required (S4: it is not) |
| 1.28 | Workflow, Save draft, Save draft; the JSON, the actor | 3 + 2 | 3 + 2 | 3 + 2 |
| 1.29 | Credit | 1 + 0 | 1 + 0 | 1 + 0 |
| 1.30 | Publish | 1 + 0 | 1 + 0 | 1 + 0 |
| 1.26–1.30 |  | 22 + 29 | 24 + 29 | 22 + 29 |
| 1.31 | the trail, Show; `KGK` | 2 + 1 | 2 + 1 | 1 + 1 |
| 6.14a | Activity types, View (MSF), the trail, New activity type, Save draft, the trail; Key, Name, Description, Sub-speciality, Speciality | 6 + 5 | 7 + 5 | 7 + 5 (the Metadata tab) |
| A.7.9 (builder) | Menu, Activity types, Edit, Preview | — | 4 + 0; the preview at 0px of scrolling, one press | about 4,390px down a 5,245px page |

######## BOARD R2-A01.dc.html
Label: R2-A01 · 1.31 Nolwazi Mbatha's 23
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A01-768.dc.html
Label: R2-A01-768 · 1.31 Nolwazi Mbatha's 23
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A01-390.dc.html
Label: R2-A01-390 · 1.31 Nolwazi Mbatha's 23
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A02.dc.html
Label: R2-A02 · 1.24 cpsa and Show: the twelve
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[value="cpsa"]
[button] Show
[button] Clear
[role="status"] 12 of 22 activity types match “cpsa”.
[role="table"]
Activity types matching “cpsa”
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |

######## BOARD R2-A02-390.dc.html
Label: R2-A02-390 · 1.24 cpsa and Show: the twelve
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[value="cpsa"]
[button] Show
[button] Clear
[role="status"] 12 of 22 activity types match “cpsa”.
[role="table"]
Activity types matching “cpsa”
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |

######## BOARD R2-A03.dc.html
Label: R2-A03 · 1.31 KGK and Show: one row
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[value="KGK"]
[button] Show
[button] Clear
[role="status"] 1 of 23 activity types match “KGK”.
[role="table"]
Activity types matching “KGK”
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |

######## BOARD R2-A03-390.dc.html
Label: R2-A03-390 · 1.31 KGK and Show: one row
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[value="KGK"]
[button] Show
[button] Clear
[role="status"] 1 of 23 activity types match “KGK”.
[role="table"]
Activity types matching “KGK”
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |

######## BOARD R2-A04.dc.html
Label: R2-A04 · zzz: the no-match state
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[value="zzz"]
[button] Show
[button] Clear
[role="status"] No activity types match "zzz".
The search is kept above. Clear it to see all 23.
[button] Clear the search

######## BOARD R2-A04-390.dc.html
Label: R2-A04-390 · zzz: the no-match state
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[value="zzz"]
[button] Show
[button] Clear
[role="status"] No activity types match "zzz".
The search is kept above. Clear it to see all 23.
[button] Clear the search

######## BOARD R2-A05.dc.html
Label: R2-A05 · Between 1.26 and 1.30: KGK Not published, Saved 2026-11-05
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A05-390.dc.html
Label: R2-A05-390 · Between 1.26 and 1.30: KGK Not published, Saved 2026-11-05
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A06.dc.html
Label: R2-A06 · 6.14a before the save: Anton Kruger's 22
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types College of Paediatricians of South Africa sets; the others you can read.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 22 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A06-768.dc.html
Label: R2-A06-768 · 6.14a before the save: Anton Kruger's 22
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types College of Paediatricians of South Africa sets; the others you can read.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 22 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A06-390.dc.html
Label: R2-A06-390 · 6.14a before the save: Anton Kruger's 22
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types College of Paediatricians of South Africa sets; the others you can read.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 22 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A07.dc.html
Label: R2-A07 · 6.14a after the save: Anton Kruger's 23
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types College of Paediatricians of South Africa sets; the others you can read.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| CPSA Case Presentation Logcpsa_case_presentation_log | CPSASpeciality · Paediatrics | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit CPSA Case Presentation Log"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A07-390.dc.html
Label: R2-A07-390 · 6.14a after the save: Anton Kruger's 23
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types College of Paediatricians of South Africa sets; the others you can read.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| CPSA Case Presentation Logcpsa_case_presentation_log | CPSASpeciality · Paediatrics | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit CPSA Case Presentation Log"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A08.dc.html
Label: R2-A08 · devadmin's 24
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change every type but the two that Wombat's releases write.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 24 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit ACAT"] Edit |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Case-based Discussion"] Edit |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| CPSA Case Presentation Logcpsa_case_presentation_log | CPSASpeciality · Paediatrics | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit CPSA Case Presentation Log"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit DOPS"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Journal Club"] Edit |
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Procedure Log"] Edit |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit QI Project"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Reflective Note"] Edit |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Research Output"] Edit |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Teaching Session"] Edit |

######## BOARD R2-A08-768.dc.html
Label: R2-A08-768 · devadmin's 24
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change every type but the two that Wombat's releases write.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 24 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit ACAT"] Edit |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Case-based Discussion"] Edit |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| CPSA Case Presentation Logcpsa_case_presentation_log | CPSASpeciality · Paediatrics | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit CPSA Case Presentation Log"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit DOPS"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Journal Club"] Edit |
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Procedure Log"] Edit |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit QI Project"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Reflective Note"] Edit |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Research Output"] Edit |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Teaching Session"] Edit |

######## BOARD R2-A08-390.dc.html
Label: R2-A08-390 · devadmin's 24
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change every type but the two that Wombat's releases write.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 24 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit ACAT"] Edit |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Case-based Discussion"] Edit |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Case-Based Discussion (Paediatrics)"] Edit |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Chart-Stimulated Recall (Paediatrics)"] Edit |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Audit (Paediatrics)"] Edit |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Clinical Case Analysis (Paediatrics)"] Edit |
| CPSA Case Presentation Logcpsa_case_presentation_log | CPSASpeciality · Paediatrics | Not published | Saved 2026-11-05 | Active | [link] [aria-label="Edit CPSA Case Presentation Log"] Edit |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Direct Observation (Paediatrics)"] Edit |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit DOPS"] Edit |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit DOPS (Paediatrics)"] Edit |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Journal Club"] Edit |
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Active | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX"] Edit |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Mini-CEX (Paediatrics)"] Edit |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Portfolio and Logbook Review (Paediatrics)"] Edit |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Procedure Log"] Edit |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit QI Project"] Edit |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Random Case Analysis (Paediatrics)"] Edit |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="Edit Reflective Exercise (Paediatrics)"] Edit |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Reflective Note"] Edit |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Research Output"] Edit |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="Edit Teaching Session"] Edit |

######## BOARD R2-A09-create.dc.html
Label: R2-A09-create · Illustration: the empty catalogue, a caller who may create
> Illustration: an empty catalogue (Demo Administrator)
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change every type but the two that Wombat's releases write.
[link] New activity type
No activity types
Create the first activity type to start the builder.
[link] New activity type

######## BOARD R2-A09-create-390.dc.html
Label: R2-A09-create-390 · Illustration: the empty catalogue, a caller who may create
> Illustration: an empty catalogue (Demo Administrator)
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change every type but the two that Wombat's releases write.
[link] New activity type
No activity types
Create the first activity type to start the builder.
[link] New activity type

######## BOARD R2-A09-nocreate.dc.html
Label: R2-A09-nocreate · Illustration: the empty catalogue, a caller who may not create
> Illustration: a College admin with no scope, ‹name›
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] ‹name›
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types ‹College name› sets; the others you can read.
No activity types
No activity type is open to you yet.

######## BOARD R2-A09-nocreate-390.dc.html
Label: R2-A09-nocreate-390 · Illustration: the empty catalogue, a caller who may not create
> Illustration: a College admin with no scope, ‹name›
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] ‹name›
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types ‹College name› sets; the others you can read.
No activity types
No activity type is open to you yet.

######## BOARD R2-A10.dc.html
Label: R2-A10 · Loading the list
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[role="status"] Loading the activity types.

######## BOARD R2-A10-390.dc.html
Label: R2-A10-390 · Loading the list
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[role="status"] Loading the activity types.

######## BOARD R2-A11.dc.html
Label: R2-A11 · The list's load error
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[role="alert"]
Could not load the activity types. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A11-390.dc.html
Label: R2-A11-390 · The list's load error
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[role="alert"]
Could not load the activity types. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-A12.dc.html
Label: R2-A12 · Illustration: KGK Teaching Session Log inactive in its writer's list
> Illustration: KGK Teaching Session Log inactive, after R2-E12
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Inactive | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-A12-390.dc.html
Label: R2-A12-390 · Illustration: KGK Teaching Session Log inactive in its writer's list
> Illustration: KGK Teaching Session Log inactive, after R2-E12
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments.
[link] New activity type
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 23 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | None | Inactive | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-B01.dc.html
Label: R2-B01 · 1.25 Mini-CEX (Paediatrics) on Form, read
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Date observed (observed_on)
**Entrustment rating field
Supervision required for this encounter (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Request request
[button] [aria-label="View section Request"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="View field Assessor"] View
Date observed
Date · observed_on · Required
[button] [aria-label="View field Date observed"] View
Clinical setting
Choice · setting · Required
[button] [aria-label="View field Clinical setting"] View
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="View field Presenting problem"] View
Case complexity
Choice · complexity · Required
[button] [aria-label="View field Case complexity"] View
##### Entrustment assessment
[button] [aria-label="View section Entrustment"] View
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="View field Supervision required for this encounter"] View
##### Feedback feedback
[button] [aria-label="View section Feedback"] View
What was done well
Long text · strengths · Required
[button] [aria-label="View field What was done well"] View
Areas for development
Long text · improvements · Required
[button] [aria-label="View field Areas for development"] View
Agreed plan
Long text · plan · Required
[button] [aria-label="View field Agreed plan"] View
#### Live preview
* marks a field you must fill in.
##### Request
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Assessor *
[aria-required="true"]
Select…
The supervisor who observed you. They complete this assessment.
Date observed *
[aria-required="true"]
When the encounter happened, not when this form is completed.
Clinical setting *
[aria-required="true"]
Select…
Ward
Outpatient clinic
Emergency unit
Neonatal unit
PICU
Community
Presenting problem *
[aria-required="true"]
Case complexity *
[aria-required="true"]
Select…
Low
Moderate
High
##### Entrustment
Supervision required for this encounter *
[aria-required="true"]
Select…
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
##### Feedback
What was done well *
[aria-required="true"]
Areas for development *
[aria-required="true"]
Agreed plan *
[aria-required="true"]
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-B01-390.dc.html
Label: R2-B01-390 · 1.25 Mini-CEX (Paediatrics) on Form, read
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Date observed (observed_on)
**Entrustment rating field
Supervision required for this encounter (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Request request
[button] [aria-label="View section Request"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="View field Assessor"] View
Date observed
Date · observed_on · Required
[button] [aria-label="View field Date observed"] View
Clinical setting
Choice · setting · Required
[button] [aria-label="View field Clinical setting"] View
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="View field Presenting problem"] View
Case complexity
Choice · complexity · Required
[button] [aria-label="View field Case complexity"] View
##### Entrustment assessment
[button] [aria-label="View section Entrustment"] View
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="View field Supervision required for this encounter"] View
##### Feedback feedback
[button] [aria-label="View section Feedback"] View
What was done well
Long text · strengths · Required
[button] [aria-label="View field What was done well"] View
Areas for development
Long text · improvements · Required
[button] [aria-label="View field Areas for development"] View
Agreed plan
Long text · plan · Required
[button] [aria-label="View field Agreed plan"] View
#### Live preview
* marks a field you must fill in.
##### Request
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Assessor *
[aria-required="true"]
Select…
The supervisor who observed you. They complete this assessment.
Date observed *
[aria-required="true"]
When the encounter happened, not when this form is completed.
Clinical setting *
[aria-required="true"]
Select…
Ward
Outpatient clinic
Emergency unit
Neonatal unit
PICU
Community
Presenting problem *
[aria-required="true"]
Case complexity *
[aria-required="true"]
Select…
Low
Moderate
High
##### Entrustment
Supervision required for this encounter *
[aria-required="true"]
Select…
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
##### Feedback
What was done well *
[aria-required="true"]
Areas for development *
[aria-required="true"]
Agreed plan *
[aria-required="true"]
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-B02.dc.html
Label: R2-B02 · 1.25 overall_level's View open: the scale by name
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Date observed (observed_on)
**Entrustment rating field
Supervision required for this encounter (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Request request
[button] [aria-label="View section Request"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="View field Assessor"] View
Date observed
Date · observed_on · Required
[button] [aria-label="View field Date observed"] View
Clinical setting
Choice · setting · Required
[button] [aria-label="View field Clinical setting"] View
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="View field Presenting problem"] View
Case complexity
Choice · complexity · Required
[button] [aria-label="View field Case complexity"] View
##### Entrustment assessment
[button] [aria-label="View section Entrustment"] View
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="Close field Supervision required for this encounter"] Close
[role="group"]
###### Field Supervision required for this encounter
**Field key
overall_level
**Label
Supervision required for this encounter
**Type
Scale
**Help text
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
**Options
None
**Catalogue key
None
**Entrustment scale
CPSA Paediatric Entrustment Scale v11.1
**Required
Yes
##### Feedback feedback
[button] [aria-label="View section Feedback"] View
What was done well
Long text · strengths · Required
[button] [aria-label="View field What was done well"] View
Areas for development
Long text · improvements · Required
[button] [aria-label="View field Areas for development"] View
Agreed plan
Long text · plan · Required
[button] [aria-label="View field Agreed plan"] View
#### Live preview
* marks a field you must fill in.
##### Request
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Assessor *
[aria-required="true"]
Select…
The supervisor who observed you. They complete this assessment.
Date observed *
[aria-required="true"]
When the encounter happened, not when this form is completed.
Clinical setting *
[aria-required="true"]
Select…
Ward
Outpatient clinic
Emergency unit
Neonatal unit
PICU
Community
Presenting problem *
[aria-required="true"]
Case complexity *
[aria-required="true"]
Select…
Low
Moderate
High
##### Entrustment
Supervision required for this encounter *
[aria-required="true"]
Select…
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
##### Feedback
What was done well *
[aria-required="true"]
Areas for development *
[aria-required="true"]
Agreed plan *
[aria-required="true"]
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-B02-390.dc.html
Label: R2-B02-390 · 1.25 overall_level's View open: the scale by name
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Date observed (observed_on)
**Entrustment rating field
Supervision required for this encounter (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Request request
[button] [aria-label="View section Request"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="View field Assessor"] View
Date observed
Date · observed_on · Required
[button] [aria-label="View field Date observed"] View
Clinical setting
Choice · setting · Required
[button] [aria-label="View field Clinical setting"] View
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="View field Presenting problem"] View
Case complexity
Choice · complexity · Required
[button] [aria-label="View field Case complexity"] View
##### Entrustment assessment
[button] [aria-label="View section Entrustment"] View
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="Close field Supervision required for this encounter"] Close
[role="group"]
###### Field Supervision required for this encounter
**Field key
overall_level
**Label
Supervision required for this encounter
**Type
Scale
**Help text
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
**Options
None
**Catalogue key
None
**Entrustment scale
CPSA Paediatric Entrustment Scale v11.1
**Required
Yes
##### Feedback feedback
[button] [aria-label="View section Feedback"] View
What was done well
Long text · strengths · Required
[button] [aria-label="View field What was done well"] View
Areas for development
Long text · improvements · Required
[button] [aria-label="View field Areas for development"] View
Agreed plan
Long text · plan · Required
[button] [aria-label="View field Agreed plan"] View
#### Live preview
* marks a field you must fill in.
##### Request
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Assessor *
[aria-required="true"]
Select…
The supervisor who observed you. They complete this assessment.
Date observed *
[aria-required="true"]
When the encounter happened, not when this form is completed.
Clinical setting *
[aria-required="true"]
Select…
Ward
Outpatient clinic
Emergency unit
Neonatal unit
PICU
Community
Presenting problem *
[aria-required="true"]
Case complexity *
[aria-required="true"]
Select…
Low
Moderate
High
##### Entrustment
Supervision required for this encounter *
[aria-required="true"]
Select…
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
##### Feedback
What was done well *
[aria-required="true"]
Areas for development *
[aria-required="true"]
Agreed plan *
[aria-required="true"]
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-B03.dc.html
Label: R2-B03 · 1.25 Details, read, with Versions
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
#### Details
**Key
mini_cex_cpsa
**Name
Mini-CEX (Paediatrics)
**Set by
College of Paediatricians of South Africa · Speciality · Paediatrics
**This tool is
Mini-CEX
**Description
Mini clinical evaluation exercise - direct observation of a focused clinical encounter, followed by immediate feedback.
**Status
Active
#### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Wombat's catalogue |
Activities filed under a version stay on it.

######## BOARD R2-B03-390.dc.html
Label: R2-B03-390 · 1.25 Details, read, with Versions
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
#### Details
**Key
mini_cex_cpsa
**Name
Mini-CEX (Paediatrics)
**Set by
College of Paediatricians of South Africa · Speciality · Paediatrics
**This tool is
Mini-CEX
**Description
Mini clinical evaluation exercise - direct observation of a focused clinical encounter, followed by immediate feedback.
**Status
Active
#### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Wombat's catalogue |
Activities filed under a version stay on it.

######## BOARD R2-B04.dc.html
Label: R2-B04 · 1.25 Workflow in words
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Requested · Completed, final · Declined · Cancelled
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Submit | Draft | Requested | The registrar, or whoever filed it | The mover's own fields | No |
| Complete | Requested | Completed | The person Assessor names | The whole form | No |
| Decline | Requested | Declined | The person Assessor names | Formats only | Required |
| Cancel | Draft or Requested | Cancelled | The registrar, or whoever filed it | Formats only | No |

######## BOARD R2-B04-390.dc.html
Label: R2-B04-390 · 1.25 Workflow in words
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Requested · Completed, final · Declined · Cancelled
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Submit | Draft | Requested | The registrar, or whoever filed it | The mover's own fields | No |
| Complete | Requested | Completed | The person Assessor names | The whole form | No |
| Decline | Requested | Declined | The person Assessor names | Formats only | Required |
| Cancel | Draft or Requested | Cancelled | The registrar, or whoever filed it | Formats only | No |

######## BOARD R2-B05.dc.html
Label: R2-B05 · 1.25 Credit in words
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="true"] Credit
[role="tabpanel"]
#### Credit
Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records.

######## BOARD R2-B05-390.dc.html
Label: R2-B05-390 · 1.25 Credit in words
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Mini-CEX (Paediatrics)
### Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Set by the College that owns Paediatrics. You can read this activity type here, but not change it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="true"] Credit
[role="tabpanel"]
#### Credit
Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records.

######## BOARD R2-B06.dc.html
Label: R2-B06 · 6.14a Multi-Source Feedback (Paediatrics) to Anton Kruger
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Multi-Source Feedback (Paediatrics)
### Multi-Source Feedback (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Written by the MSF release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Feedback window closed (observed_on)
**Entrustment rating field
Supervision the reviewer judged this trainee to need (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Multi-source feedback evidence
[button] [aria-label="View section Multi-source feedback"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
MSF campaign
Number · campaign_id · Required
[button] [aria-label="View field MSF campaign"] View
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
##### Reviewer's judgement review
[button] [aria-label="View section Reviewer's judgement"] View
Supervision the reviewer judged this trainee to need
Scale · overall_level
[button] [aria-label="View field Supervision the reviewer judged this trainee to need"] View
Reviewer's summary
Long text · summary
[button] [aria-label="View field Reviewer's summary"] View
#### Live preview
* marks a field you must fill in.
##### Multi-source feedback
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
The EPA the campaign was declared as evidence for. One record per covered EPA.
MSF campaign *
[aria-required="true"]
The campaign this record is one face of. Sibling records share it and differ only by EPA.
Feedback window closed *
[aria-required="true"]
The date the campaign closed to responses - when the evidence was complete.
Questionnaires returned *
[aria-required="true"]
##### Reviewer's judgement
Supervision the reviewer judged this trainee to need
Select…
Optional. Stated by the clinician who released the report, informed by the aggregate - never a mean of respondent scores, because a mean is not a rung.
Reviewer's summary
The narrative already released to the trainee. No respondent is named and no individual comment is reproduced here.

######## BOARD R2-B06-390.dc.html
Label: R2-B06-390 · 6.14a Multi-Source Feedback (Paediatrics) to Anton Kruger
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Multi-Source Feedback (Paediatrics)
### Multi-Source Feedback (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Written by the MSF release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Feedback window closed (observed_on)
**Entrustment rating field
Supervision the reviewer judged this trainee to need (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Multi-source feedback evidence
[button] [aria-label="View section Multi-source feedback"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
MSF campaign
Number · campaign_id · Required
[button] [aria-label="View field MSF campaign"] View
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
##### Reviewer's judgement review
[button] [aria-label="View section Reviewer's judgement"] View
Supervision the reviewer judged this trainee to need
Scale · overall_level
[button] [aria-label="View field Supervision the reviewer judged this trainee to need"] View
Reviewer's summary
Long text · summary
[button] [aria-label="View field Reviewer's summary"] View
#### Live preview
* marks a field you must fill in.
##### Multi-source feedback
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
The EPA the campaign was declared as evidence for. One record per covered EPA.
MSF campaign *
[aria-required="true"]
The campaign this record is one face of. Sibling records share it and differ only by EPA.
Feedback window closed *
[aria-required="true"]
The date the campaign closed to responses - when the evidence was complete.
Questionnaires returned *
[aria-required="true"]
##### Reviewer's judgement
Supervision the reviewer judged this trainee to need
Select…
Optional. Stated by the clinician who released the report, informed by the aggregate - never a mean of respondent scores, because a mean is not a rung.
Reviewer's summary
The narrative already released to the trainee. No respondent is named and no individual comment is reproduced here.

######## BOARD R2-B07.dc.html
Label: R2-B07 · Illustration: Learner Feedback (Paediatrics) to Anton Kruger
> Illustration: no step opens it
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Learner Feedback (Paediatrics)
### Learner Feedback (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Written by the learner-feedback release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Feedback window closed (observed_on)
**Entrustment rating field
None — this form asserts no entrustment level
**EPA field
EPA (epa_id)
#### Sections and fields
##### Learner feedback evidence
[button] [aria-label="View section Learner feedback"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
Learner feedback campaign
Number · campaign_id · Required
[button] [aria-label="View field Learner feedback campaign"] View
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
Teaching contexts that responded
Number · teaching_context_count · Required
[button] [aria-label="View field Teaching contexts that responded"] View
##### Reviewer's summary review
[button] [aria-label="View section Reviewer's summary"] View
Reviewer's summary
Long text · summary
[button] [aria-label="View field Reviewer's summary"] View
#### Live preview
* marks a field you must fill in.
##### Learner feedback
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Learner feedback campaign *
[aria-required="true"]
Feedback window closed *
[aria-required="true"]
Questionnaires returned *
[aria-required="true"]
Teaching contexts that responded *
[aria-required="true"]
##### Reviewer's summary
Reviewer's summary

######## BOARD R2-B07-390.dc.html
Label: R2-B07-390 · Illustration: Learner Feedback (Paediatrics) to Anton Kruger
> Illustration: no step opens it
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Learner Feedback (Paediatrics)
### Learner Feedback (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Written by the learner-feedback release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Feedback window closed (observed_on)
**Entrustment rating field
None — this form asserts no entrustment level
**EPA field
EPA (epa_id)
#### Sections and fields
##### Learner feedback evidence
[button] [aria-label="View section Learner feedback"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
Learner feedback campaign
Number · campaign_id · Required
[button] [aria-label="View field Learner feedback campaign"] View
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
Teaching contexts that responded
Number · teaching_context_count · Required
[button] [aria-label="View field Teaching contexts that responded"] View
##### Reviewer's summary review
[button] [aria-label="View section Reviewer's summary"] View
Reviewer's summary
Long text · summary
[button] [aria-label="View field Reviewer's summary"] View
#### Live preview
* marks a field you must fill in.
##### Learner feedback
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Learner feedback campaign *
[aria-required="true"]
Feedback window closed *
[aria-required="true"]
Questionnaires returned *
[aria-required="true"]
Teaching contexts that responded *
[aria-required="true"]
##### Reviewer's summary
Reviewer's summary

######## BOARD R2-B08.dc.html
Label: R2-B08 · Illustration: the Administrator on Multi-Source Feedback (Paediatrics)
> Illustration: Q9, the Administrator included
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Multi-Source Feedback (Paediatrics)
### Multi-Source Feedback (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Written by the MSF release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Feedback window closed (observed_on)
**Entrustment rating field
Supervision the reviewer judged this trainee to need (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Multi-source feedback evidence
[button] [aria-label="View section Multi-source feedback"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
MSF campaign
Number · campaign_id · Required
[button] [aria-label="View field MSF campaign"] View
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
##### Reviewer's judgement review
[button] [aria-label="View section Reviewer's judgement"] View
Supervision the reviewer judged this trainee to need
Scale · overall_level
[button] [aria-label="View field Supervision the reviewer judged this trainee to need"] View
Reviewer's summary
Long text · summary
[button] [aria-label="View field Reviewer's summary"] View
#### Live preview
* marks a field you must fill in.
##### Multi-source feedback
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
The EPA the campaign was declared as evidence for. One record per covered EPA.
MSF campaign *
[aria-required="true"]
The campaign this record is one face of. Sibling records share it and differ only by EPA.
Feedback window closed *
[aria-required="true"]
The date the campaign closed to responses - when the evidence was complete.
Questionnaires returned *
[aria-required="true"]
##### Reviewer's judgement
Supervision the reviewer judged this trainee to need
Select…
Optional. Stated by the clinician who released the report, informed by the aggregate - never a mean of respondent scores, because a mean is not a rung.
Reviewer's summary
The narrative already released to the trainee. No respondent is named and no individual comment is reproduced here.

######## BOARD R2-B08-390.dc.html
Label: R2-B08-390 · Illustration: the Administrator on Multi-Source Feedback (Paediatrics)
> Illustration: Q9, the Administrator included
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Multi-Source Feedback (Paediatrics)
### Multi-Source Feedback (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
Written by the MSF release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[role="tabpanel"]
Show
(radio) [checked="checked"]
The form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
**Encounter date field
Feedback window closed (observed_on)
**Entrustment rating field
Supervision the reviewer judged this trainee to need (overall_level)
**EPA field
EPA (epa_id)
#### Sections and fields
##### Multi-source feedback evidence
[button] [aria-label="View section Multi-source feedback"] View
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
MSF campaign
Number · campaign_id · Required
[button] [aria-label="View field MSF campaign"] View
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
##### Reviewer's judgement review
[button] [aria-label="View section Reviewer's judgement"] View
Supervision the reviewer judged this trainee to need
Scale · overall_level
[button] [aria-label="View field Supervision the reviewer judged this trainee to need"] View
Reviewer's summary
Long text · summary
[button] [aria-label="View field Reviewer's summary"] View
#### Live preview
* marks a field you must fill in.
##### Multi-source feedback
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
The EPA the campaign was declared as evidence for. One record per covered EPA.
MSF campaign *
[aria-required="true"]
The campaign this record is one face of. Sibling records share it and differ only by EPA.
Feedback window closed *
[aria-required="true"]
The date the campaign closed to responses - when the evidence was complete.
Questionnaires returned *
[aria-required="true"]
##### Reviewer's judgement
Supervision the reviewer judged this trainee to need
Select…
Optional. Stated by the clinician who released the report, informed by the aggregate - never a mean of respondent scores, because a mean is not a rung.
Reviewer's summary
The narrative already released to the trainee. No respondent is named and no individual comment is reproduced here.

######## BOARD R2-B09.dc.html
Label: R2-B09 · 6.14a Unavailable: /admin/activity-types/24 to Nolwazi Mbatha
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Activity type unavailable
### Activity type unavailable
This activity type does not exist, or you cannot open it.
[link] Go to Activity types

######## BOARD R2-B09-390.dc.html
Label: R2-B09-390 · 6.14a Unavailable: /admin/activity-types/24 to Nolwazi Mbatha
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Activity type unavailable
### Activity type unavailable
This activity type does not exist, or you cannot open it.
[link] Go to Activity types

######## BOARD R2-B10.dc.html
Label: R2-B10 · Loading a type
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Activity type
### Activity type
What a registrar fills in, who moves it on, and what it counts towards.
[role="status"] Loading this activity type.

######## BOARD R2-B10-390.dc.html
Label: R2-B10-390 · Loading a type
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Activity type
### Activity type
What a registrar fills in, who moves it on, and what it counts towards.
[role="status"] Loading this activity type.

######## BOARD R2-B11.dc.html
Label: R2-B11 · A type's load error
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Activity type
### Activity type
What a registrar fills in, who moves it on, and what it counts towards.
[role="alert"]
Could not load this activity type. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-B11-390.dc.html
Label: R2-B11-390 · A type's load error
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Activity type
### Activity type
What a registrar fills in, who moves it on, and what it counts towards.
[role="alert"]
Could not load this activity type. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-C01.dc.html
Label: R2-C01 · 1.26 New activity type on Details: the starting draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C01-390.dc.html
Label: R2-C01-390 · 1.26 New activity type on Details: the starting draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C02.dc.html
Label: R2-C02 · 1.26 Details filled
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="kgk_teaching_log"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C02-390.dc.html
Label: R2-C02-390 · 1.26 Details filled
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="kgk_teaching_log"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C03.dc.html
Label: R2-C03 · 1.26 the first Save draft: /admin/activity-types/23
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="kgk_teaching_log"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C03-390.dc.html
Label: R2-C03-390 · 1.26 the first Save draft: /admin/activity-types/23
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="kgk_teaching_log"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C04.dc.html
Label: R2-C04 · 6.14a New as Anton Kruger: Speciality
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C04-390.dc.html
Label: R2-C04-390 · 6.14a New as Anton Kruger: Speciality
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C05.dc.html
Label: R2-C05 · 6.14a Sub-speciality chosen
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Sub-speciality · Paediatrics / Neonatology
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
Scope *required
(radio)
Speciality
(radio) [checked="checked"]
Sub-speciality
Sub-speciality *required
Paediatrics / Neonatology
Paediatrics / Paediatrics
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C05-390.dc.html
Label: R2-C05-390 · 6.14a Sub-speciality chosen
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Sub-speciality · Paediatrics / Neonatology
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
Scope *required
(radio)
Speciality
(radio) [checked="checked"]
Sub-speciality
Sub-speciality *required
Paediatrics / Neonatology
Paediatrics / Paediatrics
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C06.dc.html
Label: R2-C06 · 6.14a saved: /admin/activity-types/24
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit CPSA Case Presentation Log
### Edit CPSA Case Presentation Log
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Anton Kruger
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="cpsa_case_presentation_log"]
Fixed once published.
Name *required
[value="CPSA Case Presentation Log"]
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a case they presented at a departmental meeting. Not rated, and credits nothing.
101 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C06-390.dc.html
Label: R2-C06-390 · 6.14a saved: /admin/activity-types/24
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit CPSA Case Presentation Log
### Edit CPSA Case Presentation Log
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Anton Kruger
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="cpsa_case_presentation_log"]
Fixed once published.
Name *required
[value="CPSA Case Presentation Log"]
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a case they presented at a departmental meeting. Not rated, and credits nothing.
101 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C07.dc.html
Label: R2-C07 · Illustration: devadmin's New, Global
> Illustration: Scope as the Administrator
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
The platform · Global
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
Scope *required
(radio) [checked="checked"]
Global
(radio)
Institution
(radio)
Speciality
(radio)
Sub-speciality
**Target
None: a Global type belongs to no institution or speciality.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C07-390.dc.html
Label: R2-C07-390 · Illustration: devadmin's New, Global
> Illustration: Scope as the Administrator
(checkbox)
[link] Wombat
Acting as Administrator
MenuClose
[aria-label="Main"]
Acting as Administrator
-
[link] Home
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
Organisations
-
[link] Institutions
-
[link] Colleges
People
-
[link] Users
-
[link] Invitations
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
The platform · Global
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
Fixed once published.
Name *required
Scope *required
(radio) [checked="checked"]
Global
(radio)
Institution
(radio)
Speciality
(radio)
Sub-speciality
**Target
None: a Global type belongs to no institution or speciality.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
0 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C08-list.dc.html
Label: R2-C08-list · Illustration: the list to a College admin with no scope: no New activity type
> Illustration: a College admin with no scope, ‹name›
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] ‹name›
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types ‹College name› sets; the others you can read.
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 22 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-C08-list-390.dc.html
Label: R2-C08-list-390 · Illustration: the list to a College admin with no scope: no New activity type
> Illustration: a College admin with no scope, ‹name›
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="page"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] ‹name›
[button] Sign out
### Activity types
The forms registrars file and assessors complete. You can change the types ‹College name› sets; the others you can read.
[aria-label="Search activity types"] [role="search"]
Search by name or key
[button] Show
[role="status"] 22 activity types.
[role="table"]
Activity types you can read or change
[role="rowgroup"]
| Name | Set by | Published | Draft | Status | Actions |
[role="rowgroup"]
| ACATacat | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View ACAT"] View |
| Case-based Discussioncbd | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Case-based Discussion"] View |
| Case-Based Discussion (Paediatrics)cbd_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Case-Based Discussion (Paediatrics)"] View |
| Chart-Stimulated Recall (Paediatrics)chart_stimulated_recall_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Chart-Stimulated Recall (Paediatrics)"] View |
| Clinical Audit (Paediatrics)clinical_audit_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Audit (Paediatrics)"] View |
| Clinical Case Analysis (Paediatrics)cca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Clinical Case Analysis (Paediatrics)"] View |
| Direct Observation (Paediatrics)direct_observation_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Direct Observation (Paediatrics)"] View |
| DOPSdops | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View DOPS"] View |
| DOPS (Paediatrics)dops_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View DOPS (Paediatrics)"] View |
| Journal Clubjournal_club | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Journal Club"] View |
| Learner Feedback (Paediatrics)Written by the learner-feedback release.learner_feedback_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Learner Feedback (Paediatrics)"] View |
| Mini-CEXmini_cex | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Mini-CEX"] View |
| Mini-CEX (Paediatrics)mini_cex_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Mini-CEX (Paediatrics)"] View |
| Multi-Source Feedback (Paediatrics)Written by the MSF release.msf_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Multi-Source Feedback (Paediatrics)"] View |
| Portfolio and Logbook Review (Paediatrics)portfolio_review_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Portfolio and Logbook Review (Paediatrics)"] View |
| Procedure Logprocedure_log | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Procedure Log"] View |
| QI Projectqi_project | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View QI Project"] View |
| Random Case Analysis (Paediatrics)rca_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Random Case Analysis (Paediatrics)"] View |
| Reflective Exercise (Paediatrics)reflective_exercise_cpsa | CPSASpeciality · Paediatrics | Version 1 | None | Active | [link] [aria-label="View Reflective Exercise (Paediatrics)"] View |
| Reflective Notereflective_note | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Reflective Note"] View |
| Research Outputresearch_output | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Research Output"] View |
| Teaching Sessionteaching_session | Demo CollegeSpeciality · General Medicine | Version 1 | None | Active | [link] [aria-label="View Teaching Session"] View |

######## BOARD R2-C08-new.dc.html
Label: R2-C08-new · Illustration: /new to a College admin with no scope
> Illustration: a College admin with no scope, ‹name›
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] ‹name›
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
You have no scope to create an activity type in. An institution's administrators create their institution's types, and a College's administrators the types of its specialities and sub-specialities.

######## BOARD R2-C08-new-390.dc.html
Label: R2-C08-new-390 · Illustration: /new to a College admin with no scope
> Illustration: a College admin with no scope, ‹name›
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] ‹name›
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
You have no scope to create an activity type in. An institution's administrators create their institution's types, and a College's administrators the types of its specialities and sub-specialities.

######## BOARD R2-C09.dc.html
Label: R2-C09 · 1.27 the Form tab of the starting draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]

######## BOARD R2-C09-768.dc.html
Label: R2-C09-768 · 1.27 the Form tab of the starting draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]

######## BOARD R2-C09-390.dc.html
Label: R2-C09-390 · 1.27 the Form tab of the starting draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]

######## BOARD R2-C10.dc.html
Label: R2-C10 · 1.27 the section editor in place
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Done with section Teaching session"] Done
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
[role="group"]
###### Section Teaching session
Section key *required
[value="session"]
Title *required
[value="Teaching session"]
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Title *
[aria-required="true"]

######## BOARD R2-C10-390.dc.html
Label: R2-C10-390 · 1.27 the section editor in place
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Done with section Teaching session"] Done
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
[role="group"]
###### Section Teaching session
Section key *required
[value="session"]
Title *required
[value="Teaching session"]
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Title *
[aria-required="true"]

######## BOARD R2-C11.dc.html
Label: R2-C11 · 1.27 Title's field editor in place, now Topic
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Done with field Topic"] Done
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Topic"] Delete
[role="group"]
###### Field Topic
Field key *required
[value="topic"]
Label *required
[value="Topic"]
Type *required
Text
Long text
Number
Date
Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]

######## BOARD R2-C11-390.dc.html
Label: R2-C11-390 · 1.27 Title's field editor in place, now Topic
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Done with field Topic"] Done
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Topic"] Delete
[role="group"]
###### Field Topic
Field key *required
[value="topic"]
Label *required
[value="Topic"]
Type *required
Text
Long text
Number
Date
Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]

######## BOARD R2-C12.dc.html
Label: R2-C12 · 1.27 Audience's editor, its key topic by mistake
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
[role="group"]
###### Field Audience
Field key *required
[value="topic"]
Label *required
[value="Audience"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C12-390.dc.html
Label: R2-C12-390 · 1.27 Audience's editor, its key topic by mistake
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
[role="group"]
###### Field Audience
Field key *required
[value="topic"]
Label *required
[value="Audience"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C13.dc.html
Label: R2-C13 · 1.27 Supervising consultant's editor: Names a
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Done with field Supervising consultant"] Done
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[role="group"]
###### Field Supervising consultant
Field key *required
[value="supervisor_user_id"]
Label *required
[value="Supervising consultant"]
Type *required
User
Text
Long text
Number
Date
Choice
Multi-choice
Scale
EPA
Likert
Procedure reference
File
Signature
Help text
Names a
Assessor (default)
Institutional admin
Coordinator
Committee member
Trainee
Who this field may name: someone at the trainee's institution holding this role.
(checkbox)
Required
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C13-390.dc.html
Label: R2-C13-390 · 1.27 Supervising consultant's editor: Names a
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Done with field Supervising consultant"] Done
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[role="group"]
###### Field Supervising consultant
Field key *required
[value="supervisor_user_id"]
Label *required
[value="Supervising consultant"]
Type *required
User
Text
Long text
Number
Date
Choice
Multi-choice
Scale
EPA
Likert
Procedure reference
File
Signature
Help text
Names a
Assessor (default)
Institutional admin
Coordinator
Committee member
Trainee
Who this field may name: someone at the trainee's institution holding this role.
(checkbox)
Required
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C14.dc.html
Label: R2-C14 · 1.27 the duplicate key refused
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form: Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
[role="group"]
###### Field Audience
Field key *required
[value="topic"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C14-390.dc.html
Label: R2-C14-390 · 1.27 the duplicate key refused
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form: Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
[role="group"]
###### Field Audience
Field key *required
[value="topic"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C15-after.dc.html
Label: R2-C15-after · Illustration: the line pressed: Form, Audience's editor, its Key focused
> Illustration: after the press, the focus on Audience's Key
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form: Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
[role="group"]
###### Field Audience
Field key *required
[value="topic"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C15-after-390.dc.html
Label: R2-C15-after-390 · Illustration: the line pressed: Form, Audience's editor, its Key focused
> Illustration: after the press, the focus on Audience's Key
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form: Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
[role="group"]
###### Field Audience
Field key *required
[value="topic"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C15-before.dc.html
Label: R2-C15-before · Illustration: the same refusal while Details is selected
> Illustration: before the summary's line is pressed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form: Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="kgk_teaching_log"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C15-before-390.dc.html
Label: R2-C15-before-390 · Illustration: the same refusal while Details is selected
> Illustration: before the summary's line is pressed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form: Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
Key *required
[value="kgk_teaching_log"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-C16.dc.html
Label: R2-C16 · 1.27 saved: Form settings by label, six rows, the preview
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C16-768.dc.html
Label: R2-C16-768 · 1.27 saved: Form settings by label, six rows, the preview
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C16-390.dc.html
Label: R2-C16-390 · 1.27 saved: Form settings by label, six rows, the preview
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C17.dc.html
Label: R2-C17 · Illustration: a Scale field's editor, the scale by name
> Illustration: Anton Kruger opens Edit, saves nothing
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit Mini-CEX (Paediatrics)
### Edit Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date observed (observed_on)
None — this form has no single encounter date
Entrustment rating field
Supervision required for this encounter (overall_level)
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Request request
[button] [aria-label="Edit section Request"] Edit
[button] [aria-label="Move section Request up"] [disabled="disabled"] Up
[button] [aria-label="Move section Request down"] Down
[button] [aria-label="Delete section Request"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] [disabled="disabled"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="Edit field Assessor"] Edit
[button] [aria-label="Move field Assessor up"] Up
[button] [aria-label="Move field Assessor down"] Down
[button] [aria-label="Delete field Assessor"] Delete
Date observed
Date · observed_on · Required
[button] [aria-label="Edit field Date observed"] Edit
[button] [aria-label="Move field Date observed up"] Up
[button] [aria-label="Move field Date observed down"] Down
[button] [aria-label="Delete field Date observed"] Delete
Clinical setting
Choice · setting · Required
[button] [aria-label="Edit field Clinical setting"] Edit
[button] [aria-label="Move field Clinical setting up"] Up
[button] [aria-label="Move field Clinical setting down"] Down
[button] [aria-label="Delete field Clinical setting"] Delete
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="Edit field Presenting problem"] Edit
[button] [aria-label="Move field Presenting problem up"] Up
[button] [aria-label="Move field Presenting problem down"] Down
[button] [aria-label="Delete field Presenting problem"] Delete
Case complexity
Choice · complexity · Required
[button] [aria-label="Edit field Case complexity"] Edit
[button] [aria-label="Move field Case complexity up"] Up
[button] [aria-label="Move field Case complexity down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Case complexity"] Delete
[button] [aria-label="Add field to section Request"] Add field
##### Entrustment assessment
[button] [aria-label="Edit section Entrustment"] Edit
[button] [aria-label="Move section Entrustment up"] Up
[button] [aria-label="Move section Entrustment down"] Down
[button] [aria-label="Delete section Entrustment"] Delete
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="Done with field Supervision required for this encounter"] Done
[button] [aria-label="Move field Supervision required for this encounter up"] [disabled="disabled"] Up
[button] [aria-label="Move field Supervision required for this encounter down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervision required for this encounter"] Delete
[role="group"]
###### Field Supervision required for this encounter
Field key *required
[value="overall_level"]
Label *required
[value="Supervision required for this encounter"]
Type *required
Scale
Text
Long text
Number
Date
Choice
Multi-choice
User
EPA
Likert
Procedure reference
File
Signature
Help text
[value="The supervision this activity actually required on this occasion - not a judgement of the trainee's worth."]
Options
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
Entrustment scale
CPSA Paediatric Entrustment Scale v11.1
O-R Scale
CNSA Neurology Entrustment Scale
Levels of the chosen scale render as the rating dropdown at runtime.
(checkbox) [checked="checked"]
Required
[button] [aria-label="Add field to section Entrustment"] Add field
##### Feedback feedback
[button] [aria-label="Edit section Feedback"] Edit
[button] [aria-label="Move section Feedback up"] Up
[button] [aria-label="Move section Feedback down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Feedback"] Delete
What was done well
Long text · strengths · Required
[button] [aria-label="Edit field What was done well"] Edit
[button] [aria-label="Move field What was done well up"] [disabled="disabled"] Up
[button] [aria-label="Move field What was done well down"] Down
[button] [aria-label="Delete field What was done well"] Delete
Areas for development
Long text · improvements · Required
[button] [aria-label="Edit field Areas for development"] Edit
[button] [aria-label="Move field Areas for development up"] Up
[button] [aria-label="Move field Areas for development down"] Down
[button] [aria-label="Delete field Areas for development"] Delete
Agreed plan
Long text · plan · Required
[button] [aria-label="Edit field Agreed plan"] Edit
[button] [aria-label="Move field Agreed plan up"] Up
[button] [aria-label="Move field Agreed plan down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Agreed plan"] Delete
[button] [aria-label="Add field to section Feedback"] Add field
#### Live preview
* marks a field you must fill in.
##### Request
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Assessor *
[aria-required="true"]
Select…
The supervisor who observed you. They complete this assessment.
Date observed *
[aria-required="true"]
When the encounter happened, not when this form is completed.
Clinical setting *
[aria-required="true"]
Select…
Ward
Outpatient clinic
Emergency unit
Neonatal unit
PICU
Community
Presenting problem *
[aria-required="true"]
Case complexity *
[aria-required="true"]
Select…
Low
Moderate
High
##### Entrustment
Supervision required for this encounter *
[aria-required="true"]
Select…
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
##### Feedback
What was done well *
[aria-required="true"]
Areas for development *
[aria-required="true"]
Agreed plan *
[aria-required="true"]
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C17-390.dc.html
Label: R2-C17-390 · Illustration: a Scale field's editor, the scale by name
> Illustration: Anton Kruger opens Edit, saves nothing
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit Mini-CEX (Paediatrics)
### Edit Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date observed (observed_on)
None — this form has no single encounter date
Entrustment rating field
Supervision required for this encounter (overall_level)
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Request request
[button] [aria-label="Edit section Request"] Edit
[button] [aria-label="Move section Request up"] [disabled="disabled"] Up
[button] [aria-label="Move section Request down"] Down
[button] [aria-label="Delete section Request"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] [disabled="disabled"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="Edit field Assessor"] Edit
[button] [aria-label="Move field Assessor up"] Up
[button] [aria-label="Move field Assessor down"] Down
[button] [aria-label="Delete field Assessor"] Delete
Date observed
Date · observed_on · Required
[button] [aria-label="Edit field Date observed"] Edit
[button] [aria-label="Move field Date observed up"] Up
[button] [aria-label="Move field Date observed down"] Down
[button] [aria-label="Delete field Date observed"] Delete
Clinical setting
Choice · setting · Required
[button] [aria-label="Edit field Clinical setting"] Edit
[button] [aria-label="Move field Clinical setting up"] Up
[button] [aria-label="Move field Clinical setting down"] Down
[button] [aria-label="Delete field Clinical setting"] Delete
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="Edit field Presenting problem"] Edit
[button] [aria-label="Move field Presenting problem up"] Up
[button] [aria-label="Move field Presenting problem down"] Down
[button] [aria-label="Delete field Presenting problem"] Delete
Case complexity
Choice · complexity · Required
[button] [aria-label="Edit field Case complexity"] Edit
[button] [aria-label="Move field Case complexity up"] Up
[button] [aria-label="Move field Case complexity down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Case complexity"] Delete
[button] [aria-label="Add field to section Request"] Add field
##### Entrustment assessment
[button] [aria-label="Edit section Entrustment"] Edit
[button] [aria-label="Move section Entrustment up"] Up
[button] [aria-label="Move section Entrustment down"] Down
[button] [aria-label="Delete section Entrustment"] Delete
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="Done with field Supervision required for this encounter"] Done
[button] [aria-label="Move field Supervision required for this encounter up"] [disabled="disabled"] Up
[button] [aria-label="Move field Supervision required for this encounter down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervision required for this encounter"] Delete
[role="group"]
###### Field Supervision required for this encounter
Field key *required
[value="overall_level"]
Label *required
[value="Supervision required for this encounter"]
Type *required
Scale
Text
Long text
Number
Date
Choice
Multi-choice
User
EPA
Likert
Procedure reference
File
Signature
Help text
[value="The supervision this activity actually required on this occasion - not a judgement of the trainee's worth."]
Options
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
Entrustment scale
CPSA Paediatric Entrustment Scale v11.1
O-R Scale
CNSA Neurology Entrustment Scale
Levels of the chosen scale render as the rating dropdown at runtime.
(checkbox) [checked="checked"]
Required
[button] [aria-label="Add field to section Entrustment"] Add field
##### Feedback feedback
[button] [aria-label="Edit section Feedback"] Edit
[button] [aria-label="Move section Feedback up"] Up
[button] [aria-label="Move section Feedback down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Feedback"] Delete
What was done well
Long text · strengths · Required
[button] [aria-label="Edit field What was done well"] Edit
[button] [aria-label="Move field What was done well up"] [disabled="disabled"] Up
[button] [aria-label="Move field What was done well down"] Down
[button] [aria-label="Delete field What was done well"] Delete
Areas for development
Long text · improvements · Required
[button] [aria-label="Edit field Areas for development"] Edit
[button] [aria-label="Move field Areas for development up"] Up
[button] [aria-label="Move field Areas for development down"] Down
[button] [aria-label="Delete field Areas for development"] Delete
Agreed plan
Long text · plan · Required
[button] [aria-label="Edit field Agreed plan"] Edit
[button] [aria-label="Move field Agreed plan up"] Up
[button] [aria-label="Move field Agreed plan down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Agreed plan"] Delete
[button] [aria-label="Add field to section Feedback"] Add field
#### Live preview
* marks a field you must fill in.
##### Request
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Assessor *
[aria-required="true"]
Select…
The supervisor who observed you. They complete this assessment.
Date observed *
[aria-required="true"]
When the encounter happened, not when this form is completed.
Clinical setting *
[aria-required="true"]
Select…
Ward
Outpatient clinic
Emergency unit
Neonatal unit
PICU
Community
Presenting problem *
[aria-required="true"]
Case complexity *
[aria-required="true"]
Select…
Low
Moderate
High
##### Entrustment
Supervision required for this encounter *
[aria-required="true"]
Select…
The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.
##### Feedback
What was done well *
[aria-required="true"]
Areas for development *
[aria-required="true"]
Agreed plan *
[aria-required="true"]
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C18.dc.html
Label: R2-C18 · Illustration: a form setting whose field was deleted
> Illustration: Date delivered deleted, not yet saved
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
delivered_on (not in the form)
None — this form has no single encounter date
Date delivered is no longer in the form. Choose another Date field, or None.
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C18-390.dc.html
Label: R2-C18-390 · Illustration: a form setting whose field was deleted
> Illustration: Date delivered deleted, not yet saved
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
delivered_on (not in the form)
None — this form has no single encounter date
Date delivered is no longer in the form. Choose another Date field, or None.
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C19.dc.html
Label: R2-C19 · Illustration: the heavy type, 6 sections and 40 fields
> Illustration: 6 sections, 40 fields; no cast member
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit ‹a type of 6 sections and 40 fields›
### Edit ‹a type of 6 sections and 40 fields›
What a registrar fills in, who moves it on, and what it counts towards.
‹owner› · ‹scope›
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
‹published›
**Draft
‹draft›
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### ‹section 1› ‹section key 1›
[button] [aria-label="Edit section ‹section 1›"] Edit
[button] [aria-label="Move section ‹section 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move section ‹section 1› down"] Down
[button] [aria-label="Delete section ‹section 1›"] Delete
[button] [aria-label="Hide the fields of section ‹section 1›"] Hide fields
‹field 1›
Text · ‹key 1›
[button] [aria-label="Edit field ‹field 1›"] Edit
[button] [aria-label="Move field ‹field 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move field ‹field 1› down"] Down
[button] [aria-label="Delete field ‹field 1›"] Delete
Supervision the reviewer judged this trainee to need
Scale · ‹key 2›
[button] [aria-label="Edit field Supervision the reviewer judged this trainee to need"] Edit
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need up"] Up
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need down"] Down
[button] [aria-label="Delete field Supervision the reviewer judged this trainee to need"] Delete
‹a 12-option choice›
Choice · ‹key 3› · Required
[button] [aria-label="Done with field ‹a 12-option choice›"] Done
[button] [aria-label="Move field ‹a 12-option choice› up"] Up
[button] [aria-label="Move field ‹a 12-option choice› down"] Down
[button] [aria-label="Delete field ‹a 12-option choice›"] Delete
[role="group"]
###### Field ‹a 12-option choice›
Field key *required
[value="‹key 3›"]
Label *required
[value="‹a 12-option choice›"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
‹option 1› ‹option 2› ‹option 3› ‹option 4› ‹option 5› ‹option 6› ‹option 7› ‹option 8› ‹option 9› ‹option 10› ‹option 11› ‹option 12›
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
‹field 4›
Text · ‹key 4›
[button] [aria-label="Edit field ‹field 4›"] Edit
[button] [aria-label="Move field ‹field 4› up"] Up
[button] [aria-label="Move field ‹field 4› down"] Down
[button] [aria-label="Delete field ‹field 4›"] Delete
‹field 5›
Text · ‹key 5›
[button] [aria-label="Edit field ‹field 5›"] Edit
[button] [aria-label="Move field ‹field 5› up"] Up
[button] [aria-label="Move field ‹field 5› down"] Down
[button] [aria-label="Delete field ‹field 5›"] Delete
‹field 6›
Text · ‹key 6›
[button] [aria-label="Edit field ‹field 6›"] Edit
[button] [aria-label="Move field ‹field 6› up"] Up
[button] [aria-label="Move field ‹field 6› down"] Down
[button] [aria-label="Delete field ‹field 6›"] Delete
‹field 7›
Text · ‹key 7›
[button] [aria-label="Edit field ‹field 7›"] Edit
[button] [aria-label="Move field ‹field 7› up"] Up
[button] [aria-label="Move field ‹field 7› down"] [disabled="disabled"] Down
[button] [aria-label="Delete field ‹field 7›"] Delete
[button] [aria-label="Add field to section ‹section 1›"] Add field
##### ‹section 2› ‹section key 2›
[button] [aria-label="Edit section ‹section 2›"] Edit
[button] [aria-label="Move section ‹section 2› up"] Up
[button] [aria-label="Move section ‹section 2› down"] Down
[button] [aria-label="Delete section ‹section 2›"] Delete
[button] [aria-label="Show the fields of section ‹section 2›"] Show fields
##### ‹section 3› ‹section key 3›
[button] [aria-label="Edit section ‹section 3›"] Edit
[button] [aria-label="Move section ‹section 3› up"] Up
[button] [aria-label="Move section ‹section 3› down"] Down
[button] [aria-label="Delete section ‹section 3›"] Delete
[button] [aria-label="Show the fields of section ‹section 3›"] Show fields
##### ‹section 4› ‹section key 4›
[button] [aria-label="Edit section ‹section 4›"] Edit
[button] [aria-label="Move section ‹section 4› up"] Up
[button] [aria-label="Move section ‹section 4› down"] Down
[button] [aria-label="Delete section ‹section 4›"] Delete
[button] [aria-label="Show the fields of section ‹section 4›"] Show fields
##### ‹section 5› ‹section key 5›
[button] [aria-label="Edit section ‹section 5›"] Edit
[button] [aria-label="Move section ‹section 5› up"] Up
[button] [aria-label="Move section ‹section 5› down"] Down
[button] [aria-label="Delete section ‹section 5›"] Delete
[button] [aria-label="Show the fields of section ‹section 5›"] Show fields
##### ‹section 6› ‹section key 6›
[button] [aria-label="Edit section ‹section 6›"] Edit
[button] [aria-label="Move section ‹section 6› up"] Up
[button] [aria-label="Move section ‹section 6› down"] [disabled="disabled"] Down
[button] [aria-label="Delete section ‹section 6›"] Delete
[button] [aria-label="Show the fields of section ‹section 6›"] Show fields
#### Live preview
* marks a field you must fill in.
##### ‹section 1›
‹field 1›
Supervision the reviewer judged this trainee to need
Select…
‹a 12-option choice› *
[aria-required="true"]
Select…
‹option 1›
‹option 2›
‹option 3›
‹option 4›
‹option 5›
‹option 6›
‹option 7›
‹option 8›
‹option 9›
‹option 10›
‹option 11›
‹option 12›
‹field 4›
‹field 5›
‹field 6›
‹field 7›
##### ‹section 2›
‹field 8›
‹field 9›
‹field 10›
‹field 11›
‹field 12›
‹field 13›
‹field 14›
##### ‹section 3›
‹field 15›
‹field 16›
‹field 17›
‹field 18›
‹field 19›
‹field 20›
‹field 21›
##### ‹section 4›
‹field 22›
‹field 23›
‹field 24›
‹field 25›
‹field 26›
‹field 27›
‹field 28›
##### ‹section 5›
‹field 29›
‹field 30›
‹field 31›
‹field 32›
‹field 33›
‹field 34›
##### ‹section 6›
‹field 35›
‹field 36›
‹field 37›
‹field 38›
‹field 39›
‹field 40›

######## BOARD R2-C19-768.dc.html
Label: R2-C19-768 · Illustration: the heavy type, 6 sections and 40 fields
> Illustration: 6 sections, 40 fields; no cast member
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit ‹a type of 6 sections and 40 fields›
### Edit ‹a type of 6 sections and 40 fields›
What a registrar fills in, who moves it on, and what it counts towards.
‹owner› · ‹scope›
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
‹published›
**Draft
‹draft›
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### ‹section 1› ‹section key 1›
[button] [aria-label="Edit section ‹section 1›"] Edit
[button] [aria-label="Move section ‹section 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move section ‹section 1› down"] Down
[button] [aria-label="Delete section ‹section 1›"] Delete
[button] [aria-label="Hide the fields of section ‹section 1›"] Hide fields
‹field 1›
Text · ‹key 1›
[button] [aria-label="Edit field ‹field 1›"] Edit
[button] [aria-label="Move field ‹field 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move field ‹field 1› down"] Down
[button] [aria-label="Delete field ‹field 1›"] Delete
Supervision the reviewer judged this trainee to need
Scale · ‹key 2›
[button] [aria-label="Edit field Supervision the reviewer judged this trainee to need"] Edit
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need up"] Up
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need down"] Down
[button] [aria-label="Delete field Supervision the reviewer judged this trainee to need"] Delete
‹a 12-option choice›
Choice · ‹key 3› · Required
[button] [aria-label="Done with field ‹a 12-option choice›"] Done
[button] [aria-label="Move field ‹a 12-option choice› up"] Up
[button] [aria-label="Move field ‹a 12-option choice› down"] Down
[button] [aria-label="Delete field ‹a 12-option choice›"] Delete
[role="group"]
###### Field ‹a 12-option choice›
Field key *required
[value="‹key 3›"]
Label *required
[value="‹a 12-option choice›"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
‹option 1› ‹option 2› ‹option 3› ‹option 4› ‹option 5› ‹option 6› ‹option 7› ‹option 8› ‹option 9› ‹option 10› ‹option 11› ‹option 12›
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
‹field 4›
Text · ‹key 4›
[button] [aria-label="Edit field ‹field 4›"] Edit
[button] [aria-label="Move field ‹field 4› up"] Up
[button] [aria-label="Move field ‹field 4› down"] Down
[button] [aria-label="Delete field ‹field 4›"] Delete
‹field 5›
Text · ‹key 5›
[button] [aria-label="Edit field ‹field 5›"] Edit
[button] [aria-label="Move field ‹field 5› up"] Up
[button] [aria-label="Move field ‹field 5› down"] Down
[button] [aria-label="Delete field ‹field 5›"] Delete
‹field 6›
Text · ‹key 6›
[button] [aria-label="Edit field ‹field 6›"] Edit
[button] [aria-label="Move field ‹field 6› up"] Up
[button] [aria-label="Move field ‹field 6› down"] Down
[button] [aria-label="Delete field ‹field 6›"] Delete
‹field 7›
Text · ‹key 7›
[button] [aria-label="Edit field ‹field 7›"] Edit
[button] [aria-label="Move field ‹field 7› up"] Up
[button] [aria-label="Move field ‹field 7› down"] [disabled="disabled"] Down
[button] [aria-label="Delete field ‹field 7›"] Delete
[button] [aria-label="Add field to section ‹section 1›"] Add field
##### ‹section 2› ‹section key 2›
[button] [aria-label="Edit section ‹section 2›"] Edit
[button] [aria-label="Move section ‹section 2› up"] Up
[button] [aria-label="Move section ‹section 2› down"] Down
[button] [aria-label="Delete section ‹section 2›"] Delete
[button] [aria-label="Show the fields of section ‹section 2›"] Show fields
##### ‹section 3› ‹section key 3›
[button] [aria-label="Edit section ‹section 3›"] Edit
[button] [aria-label="Move section ‹section 3› up"] Up
[button] [aria-label="Move section ‹section 3› down"] Down
[button] [aria-label="Delete section ‹section 3›"] Delete
[button] [aria-label="Show the fields of section ‹section 3›"] Show fields
##### ‹section 4› ‹section key 4›
[button] [aria-label="Edit section ‹section 4›"] Edit
[button] [aria-label="Move section ‹section 4› up"] Up
[button] [aria-label="Move section ‹section 4› down"] Down
[button] [aria-label="Delete section ‹section 4›"] Delete
[button] [aria-label="Show the fields of section ‹section 4›"] Show fields
##### ‹section 5› ‹section key 5›
[button] [aria-label="Edit section ‹section 5›"] Edit
[button] [aria-label="Move section ‹section 5› up"] Up
[button] [aria-label="Move section ‹section 5› down"] Down
[button] [aria-label="Delete section ‹section 5›"] Delete
[button] [aria-label="Show the fields of section ‹section 5›"] Show fields
##### ‹section 6› ‹section key 6›
[button] [aria-label="Edit section ‹section 6›"] Edit
[button] [aria-label="Move section ‹section 6› up"] Up
[button] [aria-label="Move section ‹section 6› down"] [disabled="disabled"] Down
[button] [aria-label="Delete section ‹section 6›"] Delete
[button] [aria-label="Show the fields of section ‹section 6›"] Show fields
#### Live preview
* marks a field you must fill in.
##### ‹section 1›
‹field 1›
Supervision the reviewer judged this trainee to need
Select…
‹a 12-option choice› *
[aria-required="true"]
Select…
‹option 1›
‹option 2›
‹option 3›
‹option 4›
‹option 5›
‹option 6›
‹option 7›
‹option 8›
‹option 9›
‹option 10›
‹option 11›
‹option 12›
‹field 4›
‹field 5›
‹field 6›
‹field 7›
##### ‹section 2›
‹field 8›
‹field 9›
‹field 10›
‹field 11›
‹field 12›
‹field 13›
‹field 14›
##### ‹section 3›
‹field 15›
‹field 16›
‹field 17›
‹field 18›
‹field 19›
‹field 20›
‹field 21›
##### ‹section 4›
‹field 22›
‹field 23›
‹field 24›
‹field 25›
‹field 26›
‹field 27›
‹field 28›
##### ‹section 5›
‹field 29›
‹field 30›
‹field 31›
‹field 32›
‹field 33›
‹field 34›
##### ‹section 6›
‹field 35›
‹field 36›
‹field 37›
‹field 38›
‹field 39›
‹field 40›

######## BOARD R2-C19-390.dc.html
Label: R2-C19-390 · Illustration: the heavy type, 6 sections and 40 fields
> Illustration: 6 sections, 40 fields; no cast member
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit ‹a type of 6 sections and 40 fields›
### Edit ‹a type of 6 sections and 40 fields›
What a registrar fills in, who moves it on, and what it counts towards.
‹owner› · ‹scope›
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
‹published›
**Draft
‹draft›
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### ‹section 1› ‹section key 1›
[button] [aria-label="Edit section ‹section 1›"] Edit
[button] [aria-label="Move section ‹section 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move section ‹section 1› down"] Down
[button] [aria-label="Delete section ‹section 1›"] Delete
[button] [aria-label="Hide the fields of section ‹section 1›"] Hide fields
‹field 1›
Text · ‹key 1›
[button] [aria-label="Edit field ‹field 1›"] Edit
[button] [aria-label="Move field ‹field 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move field ‹field 1› down"] Down
[button] [aria-label="Delete field ‹field 1›"] Delete
Supervision the reviewer judged this trainee to need
Scale · ‹key 2›
[button] [aria-label="Edit field Supervision the reviewer judged this trainee to need"] Edit
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need up"] Up
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need down"] Down
[button] [aria-label="Delete field Supervision the reviewer judged this trainee to need"] Delete
‹a 12-option choice›
Choice · ‹key 3› · Required
[button] [aria-label="Done with field ‹a 12-option choice›"] Done
[button] [aria-label="Move field ‹a 12-option choice› up"] Up
[button] [aria-label="Move field ‹a 12-option choice› down"] Down
[button] [aria-label="Delete field ‹a 12-option choice›"] Delete
[role="group"]
###### Field ‹a 12-option choice›
Field key *required
[value="‹key 3›"]
Label *required
[value="‹a 12-option choice›"]
Type *required
Choice
Text
Long text
Number
Date
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options
‹option 1› ‹option 2› ‹option 3› ‹option 4› ‹option 5› ‹option 6› ‹option 7› ‹option 8› ‹option 9› ‹option 10› ‹option 11› ‹option 12›
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
‹field 4›
Text · ‹key 4›
[button] [aria-label="Edit field ‹field 4›"] Edit
[button] [aria-label="Move field ‹field 4› up"] Up
[button] [aria-label="Move field ‹field 4› down"] Down
[button] [aria-label="Delete field ‹field 4›"] Delete
‹field 5›
Text · ‹key 5›
[button] [aria-label="Edit field ‹field 5›"] Edit
[button] [aria-label="Move field ‹field 5› up"] Up
[button] [aria-label="Move field ‹field 5› down"] Down
[button] [aria-label="Delete field ‹field 5›"] Delete
‹field 6›
Text · ‹key 6›
[button] [aria-label="Edit field ‹field 6›"] Edit
[button] [aria-label="Move field ‹field 6› up"] Up
[button] [aria-label="Move field ‹field 6› down"] Down
[button] [aria-label="Delete field ‹field 6›"] Delete
‹field 7›
Text · ‹key 7›
[button] [aria-label="Edit field ‹field 7›"] Edit
[button] [aria-label="Move field ‹field 7› up"] Up
[button] [aria-label="Move field ‹field 7› down"] [disabled="disabled"] Down
[button] [aria-label="Delete field ‹field 7›"] Delete
[button] [aria-label="Add field to section ‹section 1›"] Add field
##### ‹section 2› ‹section key 2›
[button] [aria-label="Edit section ‹section 2›"] Edit
[button] [aria-label="Move section ‹section 2› up"] Up
[button] [aria-label="Move section ‹section 2› down"] Down
[button] [aria-label="Delete section ‹section 2›"] Delete
[button] [aria-label="Show the fields of section ‹section 2›"] Show fields
##### ‹section 3› ‹section key 3›
[button] [aria-label="Edit section ‹section 3›"] Edit
[button] [aria-label="Move section ‹section 3› up"] Up
[button] [aria-label="Move section ‹section 3› down"] Down
[button] [aria-label="Delete section ‹section 3›"] Delete
[button] [aria-label="Show the fields of section ‹section 3›"] Show fields
##### ‹section 4› ‹section key 4›
[button] [aria-label="Edit section ‹section 4›"] Edit
[button] [aria-label="Move section ‹section 4› up"] Up
[button] [aria-label="Move section ‹section 4› down"] Down
[button] [aria-label="Delete section ‹section 4›"] Delete
[button] [aria-label="Show the fields of section ‹section 4›"] Show fields
##### ‹section 5› ‹section key 5›
[button] [aria-label="Edit section ‹section 5›"] Edit
[button] [aria-label="Move section ‹section 5› up"] Up
[button] [aria-label="Move section ‹section 5› down"] Down
[button] [aria-label="Delete section ‹section 5›"] Delete
[button] [aria-label="Show the fields of section ‹section 5›"] Show fields
##### ‹section 6› ‹section key 6›
[button] [aria-label="Edit section ‹section 6›"] Edit
[button] [aria-label="Move section ‹section 6› up"] Up
[button] [aria-label="Move section ‹section 6› down"] [disabled="disabled"] Down
[button] [aria-label="Delete section ‹section 6›"] Delete
[button] [aria-label="Show the fields of section ‹section 6›"] Show fields
#### Live preview
* marks a field you must fill in.
##### ‹section 1›
‹field 1›
Supervision the reviewer judged this trainee to need
Select…
‹a 12-option choice› *
[aria-required="true"]
Select…
‹option 1›
‹option 2›
‹option 3›
‹option 4›
‹option 5›
‹option 6›
‹option 7›
‹option 8›
‹option 9›
‹option 10›
‹option 11›
‹option 12›
‹field 4›
‹field 5›
‹field 6›
‹field 7›
##### ‹section 2›
‹field 8›
‹field 9›
‹field 10›
‹field 11›
‹field 12›
‹field 13›
‹field 14›
##### ‹section 3›
‹field 15›
‹field 16›
‹field 17›
‹field 18›
‹field 19›
‹field 20›
‹field 21›
##### ‹section 4›
‹field 22›
‹field 23›
‹field 24›
‹field 25›
‹field 26›
‹field 27›
‹field 28›
##### ‹section 5›
‹field 29›
‹field 30›
‹field 31›
‹field 32›
‹field 33›
‹field 34›
##### ‹section 6›
‹field 35›
‹field 36›
‹field 37›
‹field 38›
‹field 39›
‹field 40›

######## BOARD R2-C20.dc.html
Label: R2-C20 · Illustration: a section with no fields refused
> Illustration: Section 2 added, its New field deleted
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form, section Section 2: Section 'section_2' must contain at least one field.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
##### Section 2 section_2
This section has no fields. Add a field, or delete the section.
[button] [aria-label="Edit section Section 2"] Edit
[button] [aria-label="Move section Section 2 up"] Up
[button] [aria-label="Move section Section 2 down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Section 2"] Delete
No fields in this section yet.
[button] [aria-label="Add field to section Section 2"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
##### Section 2
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C20-390.dc.html
Label: R2-C20-390 · Illustration: a section with no fields refused
> Illustration: Section 2 added, its New field deleted
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Form, section Section 2: Section 'section_2' must contain at least one field.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form1 problem
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
##### Section 2 section_2
This section has no fields. Add a field, or delete the section.
[button] [aria-label="Edit section Section 2"] Edit
[button] [aria-label="Move section Section 2 up"] Up
[button] [aria-label="Move section Section 2 down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Section 2"] Delete
No fields in this section yet.
[button] [aria-label="Add field to section Section 2"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
##### Section 2
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C21-768.dc.html
Label: R2-C21-768 · The choice of two at 768, Edit the form chosen
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-C21-390.dc.html
Label: R2-C21-390 · A.7.9 the choice of two at 390, Preview chosen
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio)
Edit the form
(radio) [checked="checked"]
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-D01.dc.html
Label: R2-D01 · 1.28 the field: rule refused
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Workflow, move Cancel: Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a field: rule must name a User field.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow1 problem
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Submitted, final
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Submit | Draft | Submitted | The registrar | The whole form | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "logged", "label": "Logged", "terminal": true }, { "key": "cancelled", "label": "Cancelled" } ], "transitions": [ { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" }, { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|field:topic", "validation": "draft" } ] }
Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody.
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D01-390.dc.html
Label: R2-D01-390 · 1.28 the field: rule refused
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Workflow, move Cancel: Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a field: rule must name a User field.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow1 problem
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Submitted, final
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Submit | Draft | Submitted | The registrar | The whole form | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "logged", "label": "Logged", "terminal": true }, { "key": "cancelled", "label": "Cancelled" } ], "transitions": [ { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" }, { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|field:topic", "validation": "draft" } ] }
Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody.
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D02.dc.html
Label: R2-D02 · 1.28 saved: the read of Log and Cancel; the JSON laid out
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Logged, final · Cancelled
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Log | Draft | Logged | The registrar, or whoever filed it | The whole form | No |
| Cancel | Draft | Cancelled | The registrar, or whoever filed it | Formats only | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "logged", "label": "Logged", "terminal": true }, { "key": "cancelled", "label": "Cancelled" } ], "transitions": [ { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" }, { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" } ] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D02-768.dc.html
Label: R2-D02-768 · 1.28 saved: the read of Log and Cancel; the JSON laid out
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Logged, final · Cancelled
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Log | Draft | Logged | The registrar, or whoever filed it | The whole form | No |
| Cancel | Draft | Cancelled | The registrar, or whoever filed it | Formats only | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "logged", "label": "Logged", "terminal": true }, { "key": "cancelled", "label": "Cancelled" } ], "transitions": [ { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" }, { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" } ] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D02-390.dc.html
Label: R2-D02-390 · 1.28 saved: the read of Log and Cancel; the JSON laid out
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Logged, final · Cancelled
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Log | Draft | Logged | The registrar, or whoever filed it | The whole form | No |
| Cancel | Draft | Cancelled | The registrar, or whoever filed it | Formats only | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "logged", "label": "Logged", "terminal": true }, { "key": "cancelled", "label": "Cancelled" } ], "transitions": [ { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" }, { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" } ] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D03.dc.html
Label: R2-D03 · 1.29 Credit: Credits nothing
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="true"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D03-390.dc.html
Label: R2-D03-390 · 1.29 Credit: Credits nothing
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="true"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D04.dc.html
Label: R2-D04 · Illustration: a credit rule refused
> Illustration: "epa_field": "epa" typed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Credit, rule 1: Credit rule 1 reads its EPA from field 'epa', which is not a field of the form.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="true"] Credit1 problem
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa" }, "amount": 1 } ] }
Rule 1 reads its EPA from 'epa', which is not a field of this form. Use the form's EPA field, 'epa_id'.
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D04-390.dc.html
Label: R2-D04-390 · Illustration: a credit rule refused
> Illustration: "epa_field": "epa" typed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Credit, rule 1: Credit rule 1 reads its EPA from field 'epa', which is not a field of the form.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="true"] Credit1 problem
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa" }, "amount": 1 } ] }
Rule 1 reads its EPA from 'epa', which is not a field of this form. Use the form's EPA field, 'epa_id'.
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D05.dc.html
Label: R2-D05 · The starting draft's Workflow tab on /new
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Submitted, final
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Submit | Draft | Submitted | The registrar | The whole form | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "submitted", "label": "Submitted", "terminal": true } ], "transitions": [ { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" } ] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-D05-390.dc.html
Label: R2-D05-390 · The starting draft's Workflow tab on /new
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] New activity type
### New activity type
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="true"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Workflow
Draft, where a new one starts · Submitted, final
[role="table"]
Moves, as saved
[role="rowgroup"]
| Move | From | To | Who moves it | Checks | Note |
[role="rowgroup"]
| Submit | Draft | Submitted | The registrar | The whole form | No |
Workflow JSON
{ "version": 1, "initial_state": "draft", "states": [ { "key": "draft", "label": "Draft" }, { "key": "submitted", "label": "Submitted", "terminal": true } ], "transitions": [ { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" } ] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R2-E01.dc.html
Label: R2-E01 · 1.30 before: the Publish tab of a first publish
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What you are publishing
**Form
1 section, 6 fields (5 required)
**Workflow
Draft → Logged or Cancelled; 2 moves
**Credit
Credits nothing
Nothing to compare: this is the first version.
##### Versions
Not published yet.
Activities filed under a version stay on it.
[button] Publish version 1

######## BOARD R2-E01-390.dc.html
Label: R2-E01-390 · 1.30 before: the Publish tab of a first publish
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What you are publishing
**Form
1 section, 6 fields (5 required)
**Workflow
Draft → Logged or Cancelled; 2 moves
**Credit
Credits nothing
Nothing to compare: this is the first version.
##### Versions
Not published yet.
Activities filed under a version stay on it.
[button] Publish version 1

######## BOARD R2-E02.dc.html
Label: R2-E02 · 1.30 published: Published version 1.
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Published version 1.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-E02-390.dc.html
Label: R2-E02-390 · 1.30 published: Published version 1.
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Published version 1.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox) [checked="checked"]
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-E03.dc.html
Label: R2-E03 · After 1.30: the Publish tab with no draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
There is no draft to publish: version 1 is what registrars file. Save a draft to publish.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] [aria-disabled="true"] Publish version 2
Save a draft to publish.

######## BOARD R2-E03-390.dc.html
Label: R2-E03-390 · After 1.30: the Publish tab with no draft
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
There is no draft to publish: version 1 is what registrars file. Save a draft to publish.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] [aria-disabled="true"] Publish version 2
Save a draft to publish.

######## BOARD R2-E04.dc.html
Label: R2-E04 · Illustration: a later draft, Audience deleted: the warning before the press
> Illustration: states.md:916, Audience deleted and saved
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2

######## BOARD R2-E04-768.dc.html
Label: R2-E04-768 · Illustration: a later draft, Audience deleted: the warning before the press
> Illustration: states.md:916, Audience deleted and saved
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2

######## BOARD R2-E04-390.dc.html
Label: R2-E04-390 · Illustration: a later draft, Audience deleted: the warning before the press
> Illustration: states.md:916, Audience deleted and saved
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2

######## BOARD R2-E05.dc.html
Label: R2-E05 · Illustration: Publish pressed: saved first, then the question
> Illustration: Publish pressed with a warning
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2
#### Publish version 2 of KGK Teaching Session Log?
Publishing makes this draft version 2. It changes:
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
[button] Cancel
[button] Publish version 2

######## BOARD R2-E05-390.dc.html
Label: R2-E05-390 · Illustration: Publish pressed: saved first, then the question
> Illustration: Publish pressed with a warning
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2
#### Publish version 2 of KGK Teaching Session Log?
Publishing makes this draft version 2. It changes:
- Field 'Audience' will be removed.
Activities already filed stay on version 1, with Audience as they filed it. Only activities filed after the publish use version 2.
[button] Cancel
[button] Publish version 2

######## BOARD R2-E06.dc.html
Label: R2-E06 · Illustration: Published version 2.
> Illustration: version 2 published
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Published version 2.
[aria-label="Versions"]
**Published
Version 2, ‹date›, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
There is no draft to publish: version 2 is what registrars file. Save a draft to publish.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
| 2 | ‹date› | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] [aria-disabled="true"] Publish version 3
Save a draft to publish.

######## BOARD R2-E06-390.dc.html
Label: R2-E06-390 · Illustration: Published version 2.
> Illustration: version 2 published
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Published version 2.
[aria-label="Versions"]
**Published
Version 2, ‹date›, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
There is no draft to publish: version 2 is what registrars file. Save a draft to publish.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
| 2 | ‹date› | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] [aria-disabled="true"] Publish version 3
Save a draft to publish.

######## BOARD R2-E07.dc.html
Label: R2-E07 · Illustration: Discard draft… on KGK's draft
> Illustration: after R2-E04
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.
#### Discard the draft of KGK Teaching Session Log?
Its form, workflow and credit rules go back to version 1, which stays published. Its details (name, description, scope, This tool is, Active) were saved as you changed them, and stay.
[button] Cancel
[button] Discard draft

######## BOARD R2-E07-390.dc.html
Label: R2-E07-390 · Illustration: Discard draft… on KGK's draft
> Illustration: after R2-E04
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.
#### Discard the draft of KGK Teaching Session Log?
Its form, workflow and credit rules go back to version 1, which stays published. Its details (name, description, scope, This tool is, Active) were saved as you changed them, and stay.
[button] Cancel
[button] Discard draft

######## BOARD R2-E08.dc.html
Label: R2-E08 · states.md:917 Draft discarded.: the form back to version 1
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Draft discarded.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-E08-390.dc.html
Label: R2-E08-390 · states.md:917 Draft discarded.: the form back to version 1
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Draft discarded.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-E09.dc.html
Label: R2-E09 · Illustration: Discard draft… on a type never published
> Illustration: CPSA Case Presentation Log, never published
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit CPSA Case Presentation Log
### Edit CPSA Case Presentation Log
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Anton Kruger
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]
#### Discard the draft of CPSA Case Presentation Log?
It has never been published. Its form, workflow and credit rules go back to the starting draft; its details stay.
[button] Cancel
[button] Discard draft

######## BOARD R2-E09-390.dc.html
Label: R2-E09-390 · Illustration: Discard draft… on a type never published
> Illustration: CPSA Case Presentation Log, never published
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit CPSA Case Presentation Log
### Edit CPSA Case Presentation Log
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Anton Kruger
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]
#### Discard the draft of CPSA Case Presentation Log?
It has never been published. Its form, workflow and credit rules go back to the starting draft; its details stay.
[button] Cancel
[button] Discard draft

######## BOARD R2-E10.dc.html
Label: R2-E10 · Illustration: Draft discarded. on a type never published
> Illustration: the starting draft back, the details kept
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit CPSA Case Presentation Log
### Edit CPSA Case Presentation Log
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Draft discarded.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]

######## BOARD R2-E10-390.dc.html
Label: R2-E10-390 · Illustration: Draft discarded. on a type never published
> Illustration: the starting draft back, the details kept
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit CPSA Case Presentation Log
### Edit CPSA Case Presentation Log
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[role="status"] Draft discarded.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Details details
[button] [aria-label="Edit section Details"] Edit
[button] [aria-label="Move section Details up"] [disabled="disabled"] Up
[button] [aria-label="Move section Details down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Details"] Delete
Title
Text · title · Required
[button] [aria-label="Edit field Title"] Edit
[button] [aria-label="Move field Title up"] [disabled="disabled"] Up
[button] [aria-label="Move field Title down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Title"] Delete
[button] [aria-label="Add field to section Details"] Add field
#### Live preview
* marks a field you must fill in.
##### Details
Title *
[aria-required="true"]

######## BOARD R2-E11.dc.html
Label: R2-E11 · Illustration: Active unticked on a published type: Save draft asks
> Illustration: Active unticked, Save draft pressed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox)
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.
#### Deactivate KGK Teaching Session Log?
Registrars are no longer offered it when filing, and only those who may change it can find it. Activities already filed are not changed.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-E11-390.dc.html
Label: R2-E11-390 · Illustration: Active unticked on a published type: Save draft asks
> Illustration: Active unticked, Save draft pressed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox)
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.
#### Deactivate KGK Teaching Session Log?
Registrars are no longer offered it when filing, and only those who may change it can find it. Activities already filed are not changed.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R2-E12.dc.html
Label: R2-E12 · Illustration: KGK Teaching Session Log is inactive
> Illustration: Q15's result
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox)
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-E12-390.dc.html
Label: R2-E12-390 · Illustration: KGK Teaching Session Log is inactive
> Illustration: Q15's result
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="status"] Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="true"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
Not a WBA instrument
CBD
CCA
Chart-stimulated recall
Clinical audit
Direct observation
DOPS
Learner feedback
Mini-CEX
MSF
Portfolio and logbook review
RCA
Reflective exercise
Choosing an instrument lets each curriculum item's tool list decide which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save the draft, not when you publish, and discarding the draft does not undo it. On a seeded type, publish or discard that draft afterwards, or seed updates stop reaching the type.
Description
A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.
94 of 2,000 characters
(checkbox)
Active
An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed.

######## BOARD R2-E13.dc.html
Label: R2-E13 · Illustration: a failed save
> Illustration: the save failed; the form as typed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] The draft could not be saved. Nothing has changed. Try again, or come back in a few minutes.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-E13-390.dc.html
Label: R2-E13-390 · Illustration: a failed save
> Illustration: the save failed; the form as typed
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[role="alert"] The draft could not be saved. Nothing has changed. Try again, or come back in a few minutes.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-E14-390.dc.html
Label: R2-E14-390 · A.7.9 KGK Teaching Session Log at 390 after 1.30
(checkbox)
[link] Wombat
Acting as Institutional admin
MenuClose
[aria-label="Main"]
Acting as Institutional admin
-
[link] Home
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit KGK Teaching Session Log
### Edit KGK Teaching Session Log
What a registrar fills in, who moves it on, and what it counts towards.
Kgosi Kgari Teaching Hospital · Institution
[button] Save draft
[button] [aria-disabled="true"] Publish
Save a draft to publish.
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="true"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
Date delivered (delivered_on)
None — this form has no single encounter date
Entrustment rating field
None — this form asserts no entrustment level
EPA field
EPA (epa_id)
None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
EPA *
[aria-required="true"]
Select…
PAED-001 — Providing paediatric emergency care to children
PAED-002 — Managing common paediatric presentations
PAED-003 — Providing intensive care to children
PAED-004 — Managing common neonatal conditions
PAED-005 — Providing neonatal care in intensive and high-care settings
PAED-006 — Managing long-term health conditions (LTHCs)
PAED-007 — Maintaining and promoting the health and well-being of children
PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children
PAED-009 — Providing care for an adolescent patient
PAED-010 — Leading and operating within a clinical team
PAED-011 — Managing population health challenges
PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams
PAED-013 — Managing child protection, legal and ethical aspects of children at risk
PAED-014 — Providing palliative and end-of-life care
PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research
Date delivered *
[aria-required="true"]
Audience *
[aria-required="true"]
Select…
Interns
Medical students
Nursing staff
Registrars
Learning objectives *
[aria-required="true"]
Supervising consultant
Select…
In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.

######## BOARD R2-E15.dc.html
Label: R2-E15 · Illustration: a seed-owned type with a draft
> Illustration: Mini-CEX (Paediatrics) with a draft saved by Anton Kruger
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit Mini-CEX (Paediatrics)
### Edit Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
Saved ‹date› by Anton Kruger
While this draft is saved, updates from Wombat's catalogue stop reaching this type, until you publish or discard it.
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
Nothing on the form changes for activities already filed.
Once you publish it, updates from Wombat's catalogue no longer reach this type.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Wombat's catalogue |
Activities filed under a version stay on it.
[button] Publish version 2

######## BOARD R2-E15-390.dc.html
Label: R2-E15-390 · Illustration: a seed-owned type with a draft
> Illustration: Mini-CEX (Paediatrics) with a draft saved by Anton Kruger
(checkbox)
[link] Wombat
Acting as College admin
MenuClose
[aria-label="Main"]
Acting as College admin
-
[link] Home
-
[link] Specialities
-
[link] EPAs
-
[link] Curricula
-
[link] [aria-current="true"] Activity types
-
[link] Entrustment scales
-
[link] My data rights
[link] Anton Kruger
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
[link] Activity types
- [aria-current="page"] Edit Mini-CEX (Paediatrics)
### Edit Mini-CEX (Paediatrics)
What a registrar fills in, who moves it on, and what it counts towards.
College of Paediatricians of South Africa · Speciality · Paediatrics
[button] Save draft
[button] Publish
[button] [aria-haspopup="dialog"] Discard draft…
[aria-label="Versions"]
**Published
Version 1, 2026-11-05, Wombat's catalogue
**Draft
Saved ‹date› by Anton Kruger
While this draft is saved, updates from Wombat's catalogue stop reaching this type, until you publish or discard it.
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [aria-label="Details"] [role="tab"] [aria-selected="false"] Details
[button] [aria-label="Form"] [role="tab"] [aria-selected="false"] Form
[button] [aria-label="Workflow"] [role="tab"] [aria-selected="false"] Workflow
[button] [aria-label="Credit"] [role="tab"] [aria-selected="false"] Credit
[button] [aria-label="Publish"] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
Nothing on the form changes for activities already filed.
Once you publish it, updates from Wombat's catalogue no longer reach this type.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Wombat's catalogue |
Activities filed under a version stay on it.
[button] Publish version 2
