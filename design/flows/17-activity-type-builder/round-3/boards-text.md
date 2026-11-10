# Flow 17 round 3: the text of every board

Generated from `round-3/R3-*.dc.html` (T398, build-contracts § 0) by a stdlib script, run with `python -I`, as flow 16's was: style, script, svg, head and helmet blocks stripped. Each board opens with its label strip: its `<title>` as `Label:`, then any `cv-chip` or `anno` chip after `>`. Kept: headings, labels, aria-labels, placeholders, input values, alt text, `role`, `[checked]`, `[disabled]`, `[aria-disabled]`, `[selected]`, `[aria-selected]`, `[aria-haspopup]`, `[aria-current]`, `[aria-required]`; a link is marked `[link]`, a button `[button]`. The order is by id: the Spec, Steps and check boards, then A, B, C, D, E, each at 1280 then its other widths (the widest first). Design data: a label strip, a chip, a caption saying Illustration, FIXTURES, DATA or PROPOSED is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R3-0-spec.dc.html
Label: R3-0-spec · Spec (1 of 4)
### Flow 17 · Round 3 · Spec (1 of 4)
#### § 1 The reference files, and where the boards are
First: the reference files. Everything arrived and opened; nothing was unreadable, and nothing was drawn from memory.
- project/r2/: all 145 round-2 board files (R2-….ref.html, the four document boards included) and f17r2.css, read whole. Each R3 board was drawn from its R2 file: the 145 files are byte for byte the round-2 boards, and every R3 board was then compared line by line against the R2 file its row names, so what no item changes is as that file has it.
- The 30 captures under project/ref/baseline/: all present (30 names, each the bytes round 2 opened). Viewed for this round's boards: act-1/1.24-1-types-search-cpsa.png (R3-A13); act-1/1.26-1-new-type-default.png and act-1/1.26-2-first-save-no-status.png (R3-C01–C03); act-1/1.27-1-duplicate-key-refused.png (R3-C14, C15, C23); act-1/1.27-3-user-field-names-a.png (R3-C13); act-1/1.28-1-field-rule-refused.png (R3-D01: the JSON as typed); act-1/1.29-1-credit-tab.png and act-1/1.30-1-published-v1.png (R3-E02); act-1/1.30-2-metadata-key-locked.png (R3-E02b); act-6/6.14a-2-kruger-msf-cpsa-open.png (R3-B07's kin); act-6/6.14a-3-new-type-scope.png (R3-C04, C05); states/activity-type-edit--publish-warnings.png (R3-E04, C18b); states/activity-type-edit--discarded.png (R3-E07). No word on a capture contradicts a board where the board does not mean to change it: the captures show today's defects (Metadata, v1, "epa_id · Epa", the live warnings alert, the select of field keys, Back to list), which the boards replace on purpose.
- The 9 seed files under project/ref/seeds/: mini_cex_cpsa/schema.json, mini_cex_cpsa/workflow.json, mini_cex_cpsa/credit.json, reflective_exercise_cpsa/schema.json, reflective_exercise_cpsa/workflow.json, reflective_exercise_cpsa/credit.json, teaching_session/schema.json, teaching_session/workflow.json, teaching_session/credit.json, all read. Learner Feedback's and MSF's help texts are from your FIXTURES (their seeds, learner_feedback_cpsa/schema.json and msf_cpsa/schema.json, are not in project/ref/).
- The brand mark is this canvas's own copy, /_blob/1464be2bd2c027ed86bec42ee064aa32, on every R3 board.
Round 3 is on its own canvas, "Wombat 17 · Round 3" (the first had 162 boards; 249 would pass a canvas's ceiling). Round 2's boards that stand, on "Wombat 17": R2-A01, A02, A03, A04, A05, A06, A07, A08, A09-create, A10, A11; R2-B01, B02, B03, B04, B05, B06, B08, B09, B10, B11; R2-C08-new, C09, C16, C21 (390 and 768); R2-D02, D03; R2-E03, E06, E08, E09, E10, E13, E14 (390), E15. The R3 boards: redrawn R3-A09-nocreate, A12; R3-B07; R3-C01, C02, C03, C04, C05, C06, C07, C08-list, C10, C11, C12, C13, C14, C15-before, C15-after, C17, C18, C19 (and 768), C20; R3-D01, D04, D05; R3-E01, E02, E04 (and 768), E05, E07, E11, E12; new R3-A13, C18b, C22, C23, E02b, E16, E17.
#### § 2 Decisions, items, and what is PROPOSED
The decisions Q1–Q20, each with its boards
| Q | Decided | Boards |
| Q1 | Tabs as the tabs pattern; Details first on a new type; a Publish tab; Publish kept in the header | every builder board; R3-C01, R3-E01, R2-E03, R3-E04 |
| Q2 | JSON stays the editor (textarea.form-control.font-mono); the read in words above it, from the saved JSON; a reader gets no JSON; no diagram | R2-B04, R2-B05, R3-D01, R2-D02, R2-D03, R3-D04, R3-D05 |
| Q3 | Versions: number, the SA day, publisher; "Activities filed under a version stay on it."; no counts | R2-B03, R3-E01, R2-E03, R3-E04, R2-E06, R3-C18b |
| Q4 | Editor in place under its row, one at a time, Edit → Done; focus to the first input, back to Edit | R3-C10–C14, R3-C17, R3-C23, R2-B02 |
| Q5 | At 390 and wherever the columns stack (to 1115px): Show, "Edit the form" / "Preview" | R2-C09-768, R2-C16-768, R2-C21-390, R2-C21-768, every builder 390 board |
| Q6 | RefusalSummary, lines linked across tabs; words at the place; today's sentences kept; commands return the place (DATA) | R3-C14, R3-C15-before/after, R3-C20, R3-D01, R3-D04, R3-C22 |
| Q7 | Name with key under it, both in .person-cell; Set by in words; Version 1 / Not published; Saved ‹date› / None; StackWide; Show and Clear | R2-A01–A11, R3-A12, R3-A13 |
| Q8 | Delete outline, asks nothing; Discard draft… asks on Cancel; Publish asks only with warnings | every writer Form board; R3-E05, R3-E07, R2-E09 |
| Q9 | System-managed: read-only to everyone, the Administrator included; View; the release's notice (writing only: Q19) | R2-A06–A08, R2-B06, R3-B07, R2-B08 |
| Q10 | The build folds in T334, T324's builder half, T271 item 2, T291 items 3 and 6, T326's list half, T280's tab item, T264's kind, T329's kind, § 5's faults | all |
| Q11 | Every College's national and every Global type to a non-Administrator, a College admin with no scope included (ListActivityTypesAdminQuery.cs:41-45: R3-C08-list's 22 rows, CPSA's twelve among them); own types first; a subtitle by role, never by count | R2-A01, R2-A05, R3-C08-list, R2-A09-create |
| Q12 | Subtitles; T300's notice word for word | every board |
| Q13 | The JSON laid out by the parser after each save; no stored text (typed JSON shows until the save: R3-D01) | R2-D02, R2-D03, R3-D05 |
| Q14 | Discard's two bodies; the Details line | R3-E07, R2-E09, every Details board |
| Q15 | Active newly unticked on a published type asks at Save draft; reactivating asks nothing | R3-E11, R3-E12, R3-A12 |
| Q16 | Publish saves first (a type that already has a draft: the edits since), works out the warnings from what was saved, asks only if any, then publishes | R3-E05 (and R2-E13 for a save that fails) |
| Q17 | With Active newly unticked, Publish asks Q15's question first, then saves, then asks about warnings, then publishes; Cancel leaves all unsaved | R3-E16, R3-E17 |
| Q18 | A setting at a deleted field is dropped at save (T133); no refusal | R3-C18, R3-C18b |
| Q19 | Q9 changes writing only; an inactive system-managed type stays readable | no board (Spec § 3) |
| Q20 | Publish lands on the tab it was pressed from, the result above the tabs | R3-E02, R3-E02b |
Round 2's T1–T24, as reviewed (changed rows marked)
| T | Drawn as | Boards |
| T1 | A reader has four tabs; Versions close the reader's Details tab | R2-B03 |
| T2 | New type on Details; existing on Form; a save keeps the tab | R3-C01, R2-B01, R3-C03 |
| T3 (changed by M2) | .tab-bar restyled (C1); the count the shipped .badge.badge-declined inside the tab and IN its name ("Form 1 problem"); round 2's "outside the tab's name" (aria-label plus aria-describedby) goes | R3-C14, R3-D01, R3-D04, R3-C22 |
| T4 | Argued again (S26, § 4): round 1's outline, now a list (S27) | every Form board |
| T5 | The CSS-only choice (C2) | R2-C21-390, R2-C21-768, R2-C09-768 |
| T6 | Done only | R3-C10–C13 |
| T7 | "Everything you typed is kept." | R3-C14, R3-D01, R3-C22 |
| T8 | The read's words | R2-B04, R2-D02, R3-D05, R2-B06 |
| T9 | The read follows the saved JSON, "Moves, as saved" | R3-D01 |
| T10 | The JSON editors' help | R3-D boards |
| T11 | The College admin's and the Administrator's subtitles (N12's for a College with no speciality) | R2-A06–A08 |
| T12 | "The platform" | R3-C07's owner line |
| T13 | Own = may change (Edit) | R2-A06, R2-A07 |
| T14 | One Publish action; aria-disabled with its reason (inert: S5) | R3-C01, R3-E02, R2-E03 |
| T15 | The Publish tab with no draft; on /new; the first publish's line | R2-E03, R3-E01 |
| T16 | The SA day; the publisher by name; "Wombat's catalogue" | R2-B01, R2-B03 |
| T17 (changed by N2) | Q15's words: Active's help and the dialog without "only those who may change it can find it" | R3-E11, R3-E12, R3-E16 |
| T18 | The seed-owned note | R2-E15 |
| T19 (changed by Q18) | Settings by label with the key after; the second half (a deleted field kept and invalid) goes | R2-C16, R3-C18 |
| T20 | Failure, load and loading words | R2-E13, R2-A10, R2-A11, R2-B10, R2-B11 |
| T21 | A writer's h1 "Edit KGK Teaching Session Log" | every writer board |
| T22 | "Nobody is offered it when filing an activity." | R2-B06, R3-B07, R2-B08 |
| T23 (drawn as S17) | Hide fields / Show fields above 20 fields; all start open; R3-C19 drawn after Hide fields on sections 2 to 6 | R3-C19 |
| T24 | "an Assessor" | Spec § 9 |
This round's items, each with its boards (the check board says done for each)
| Item | Boards or place |
| M1 | R3-C01–C07, C15-before, C22, E02b, E11, E12, E16, E17 (Details: the line under Q14's, aria-required on Key, Name, Sub-speciality's select; Scope's radiogroup role and aria-required); R3-C10–C14, C15-after, C17, C19, C23 (the editor: the line under its h4; Field key, Label, Type, and a Choice's Options); twins |
| M2 | (a) R3-C14, C15-before, C15-after, C20, D01, D04, C22: no aria-label or aria-describedby on any tab; the name is the words, "Form 1 problem". (b) R3-C19: "Hide fields of section ‹section 1›" / "Show fields of section ‹section 2›", no aria-expanded, aria-controls the section's list |
| M3 | Steps T1: the Preview radio counted as a field |
| M4 | R3-E02 on Credit; R3-E02b; Steps T2 |
| M5 | Steps T3: rows 902, 904, 906, 911, 912 |
| S1 | Spec § 3 (RefusalSummary row) |
| S2 | Spec § 3 (RefusalPlace) |
| S3 | Spec § 3 (the refusal rule) |
| S4 | R3-D04: "Credit: " + EvidenceEpa.cs:107's sentence; the new words under the JSON |
| S5 | Spec § 3 (PublishAsync) |
| S6 | Spec § 3 and § 9 (no board) |
| S7 | R3-E05: the dialog's confirm btn-primary; the header's Publish btn-success |
| S8 | Spec § 3 (ActivityForm EpaOptions; SectionHeadingLevel 3) |
| S9 | Spec § 3 (ActorRuleWords and its table) |
| S10 | R3-C22 (summary, Key's FieldRefusal, aria-invalid, Details 1 problem); Spec § 3 and § 9 (the four words) |
| S11 | Spec § 3 |
| S12 | Spec § 3 |
| S13 | R3-C23; Spec § 6; Steps T5 |
| S14 | R3-A12: KGK's Draft "Saved ‹date›" |
| S15 | Chips by the one rule: D05 and E01 gain it; E04 and E07 lose it; C18, C18b, C22, E16, E17 carry it; A13, C23, E02b do not. Spec § 2 lists the 29 |
| S16 | Spec § 9 |
| S17 | R3-C19: "after Hide fields on sections 2 to 6" in its title and chip |
| S18 | R3-A13; Steps 1.24 |
| S19 | R3-B07: Learner Feedback's help texts under its preview fields |
| S20 | Steps T4 |
| S21 | The badge is .badge.badge-declined on R3-C14, C15-before, C15-after, C20, D01, D04, C22; no .tab-problem on any board or in f17r3.css |
| S22 | The 390 twins: the number alone (R1); measured below |
| S23 | Spec § 6 and § 7 (no board) |
| S24 | f17r3.css C1; Spec § 7 |
| S25 | f17r3.css C3 stand-in; Spec § 3 and § 4; THE LIST below |
| S26 | Spec § 4 (T4's argument corrected; the measure derived, not rendered) |
| S27 | R3-B07, C10–C14, C15-after, C17, C18, C19, C20, C23, E07: ul.list-unstyled of li.field-row; div.actions-cell; the h3 the title alone, its key a p.field-row-meta ; Add section and Add field standalone |
| S28 | f17r3.css S2's forced-colors block; Spec § 4 |
| S29 | R3-C19: ids sec-section-1, fe-section-1-field-3, fields-section-2 …; no id holds a bracket or space (checked on every R3 board) |
| N1 | Spec § 3 (GetPublishWarnings) |
| N2 | R3-C01–C07, C15-before, C22, E02b, E11, E12, E16, E17 (Active's help); R3-E11, E16 (the dialog) |
| N3 | Spec § 3 |
| N4 | Spec § 3 |
| N5 | Spec § 9 |
| N6 | No aria-describedby="" on any R3 board (checked) |
| N7 | Dates stay ‹date›; Spec § 3 says the code fills the SA day of that save |
| N8 | Spec § 3 (the handler named) |
| N9 | R3-C11, C12, C13, C17, C23 (and every select drawn): the built order, the chosen one selected in place |
| N10 | Spec § 9 |
| N11 | Spec § 8 |
| N12 | R3-C08-list, R3-A09-nocreate; Spec § 3 and § 8 |
| N13 | Spec § 3 |
| N14 | = S5 |
| N15 | R3-D01: the JSON as 1.28 types it, cancel's actor subject|field:topic |
| N16 | Spec § 2 (FIXTURES) |
| N17 | Spec § 2; no bracket as any control's value on any R3 board (checked): R3-C19's open editor is empty, its preview's 12-option select offers none |
| N18 | Spec § 2: "key under Name in monospace" |
| N19 | R3-E12's chip says its state; R3-E04 has none; R2-B08's "Q9" stands, not redrawn |
| N20 | No tab has an aria-label (checked) |
| N21 | Panels with a control take no tabindex; reader panels with none keep 0 |
| N22 | R3-C10–C14, C15-after, C17, C19, C23: each Done carries aria-controls its editor's id |
| N23 | Spec § 4 (C2's note) |
| N24 | Spec § 7 |
##### Illustrations, by S15's one rule
A chip and "Illustration" in the title on every state no runbook step and no states.md row reaches, except a loading, a load error, an Unavailable page and a required-field state: R3-A09-nocreate, R3-A12, R3-B07, R3-C07, R3-C08-list, R3-C15-before, R3-C15-after, R3-C17, R3-C18, R3-C18b, R3-C19, R3-C20, R3-C22, R3-D04, R3-D05, R3-E01, R3-E05, R3-E11, R3-E12, R3-E16, R3-E17, and the standing R2-A09-create, R2-B08, R2-C08-new, R2-E06, R2-E09, R2-E10, R2-E13, R2-E15: 29 (each with its twins).
##### PROPOSED (THIS ASK)
- M2(b): the folds' names "Hide fields of section ‹title›" / "Show fields of section ‹title›", no aria-expanded (R3-C19)
- S7: the Publish dialog's confirm a btn-primary (R3-E05)
- S17: R3-C19 drawn after Hide fields on sections 2 to 6
- S22: at 390 a tab's badge shows its number alone
- N2: Active's help and Q15's dialog body, the shorter words
- N7: a date on an Illustration no step dates stays ‹date›; the code fills it with the SA day of that save
- N12's subtitle for a College with no speciality
- Q17's two results (words only)
- E02b's region rule: a result stays in its region until the next save, publish or discard; a tab press is none of them
- S9's ActorRuleWords and RefusalPlace (names)
##### NEW sentences
Every NEW sentence of this round, beside what it replaces
| NEW | Replaces |
| "Credit: " + "Credit rule 1 reads its EPA from field 'epa', but the form's EPA field is 'epa_id'. The EPA an activity is filed against and the EPA it credits must be the same field." | "Credit, rule 1: Credit rule 1 reads its EPA from field 'epa', which is not a field of the form." |
| "Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'." | "Rule 1 reads its EPA from 'epa', which is not a field of this form. Use the form's EPA field, 'epa_id'." |
| "Save draft to see what publishing changes." | the live warnings alert above the tabs |
| "Enter a key." · "A key is at most 100 characters." | nothing (the save has no validator) |
| "An inactive type is not offered to anyone filing an activity. Activities already filed are not changed." | "An inactive type is not offered to anyone filing an activity, and only those who may change it can find it. Activities already filed are not changed." |
| "Registrars are no longer offered it when filing. Activities already filed are not changed." | "Registrars are no longer offered it when filing, and only those who may change it can find it. Activities already filed are not changed." |
| "Nothing was saved. Multi-Source Feedback (Paediatrics) is written by the MSF release, so it cannot be changed here." | "Not saved. Multi-Source Feedback (Paediatrics) is written by …" |
| "The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change." | T11's College admin subtitle, for that case |
| "Published version 2. KGK Teaching Session Log is inactive: registrars are no longer offered it." · "Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it. Nothing was published." | nothing (Q17's new sequence) |
| The fold names "Hide fields of section ‹title›" / "Show fields of section ‹title›" | "Hide the fields of section ‹title›" with aria-expanded |
| The badge's hidden word below 641px ("1" shown, " problem" kept for the name) | "1 problem" in every width |
| S9's scope:subspeciality "of the activity's sub-speciality" | nothing |
##### FIXTURES
- FIXTURES as round 2's, with this round's additions (A13's 22 rows, Learner Feedback's help texts, the typed workflow, the credit and key-taken refusals, New field, C18b's warnings, the selects' built order).
- The help texts under R2-B06's and R3-B07's previews are the seeds' (msf_cpsa/schema.json, learner_feedback_cpsa/schema.json), given in FIXTURES.
- Inferred, not observed (N16): the Demo types' "Version 1", and Mini-CEX's (and the two releases') "2026-11-05, Wombat's catalogue".
- N17: no bracket is a control's value on any R3 board; where FIXTURES give no words the control is empty and the bracket stands in the board's title or chip.
- N18 (round 2's M2 row corrected): "key under Name in monospace".

######## BOARD R3-0-spec-2.dc.html
Label: R3-0-spec-2 · Spec (2 of 4)
### Flow 17 · Round 3 · Spec (2 of 4)
§ 3 The code changes (each PROPOSED; a decided one says its Q)
| Where | Change |
| ActivityTypesList | Search applied with Show and Clear (no @bind:event="oninput"); the count line; the no-match state apart from the empty catalogue; Q7's columns; Q11's order: "by name" is StringComparer.OrdinalIgnoreCase on Name, applied by the page after the query, which orders by the database's collation (ListActivityTypesAdminQuery.cs:49) (N13); each date the SA day of a UTC DateTime through ProgrammeCalendar.DateOf (ProgrammeCalendar.cs:23); StackWide and the 1024px stack; links kept; the subtitle by role (N12's for a College with no speciality); the loading line and the load error with Try again. |
| ListActivityTypesAdminQuery | Each row's owner in words (DATA: the College's code is not read today, :81–82); SystemManaged with its release; CanWrite false when system-managed; whether the caller's College has a discipline (N12, DATA). |
| ActivityTypeAdminScope (Q9, Q19) | MayWrite refuses a system-managed type to everyone, the Administrator included; a separate may-read rule ignores the flag (CanReadAsync, GetActivityTypeEditorQuery.cs:174-176, and the list's own filter, ListActivityTypesAdminQuery.cs:84, read MayWrite today, so refusing in MayWrite alone would hide it). MayWrite and EnsureCallerCanWriteAsync take the type (or its SystemManaged and key) at every caller: the list's per-row MayWrite, ForCallerAsync, the three commands. The release's word from a map of the two keys (msf_cpsa "the MSF release", learner_feedback_cpsa "the learner-feedback release"; ActivityTypeSeedCatalogue.cs:173, :180). SystemManagedActivityTypeTests.SavingAndPublishingASystemManagedTypeInTheBuilder_KeepsItSystemManaged (:78-110) inverts with T334. |
| GetActivityTypeEditorQuery | Null for an unknown id and for one the caller may not read (today both throw, :83, :87). N3: the null changes its callers: the filing page (NewActivity.razor:308, ForBuilder false) maps it to its own load state, and ActivityTypeScopeGuardTests.cs:433 and ActivityTypeBuilderAccessTests.cs:259, which assert the throw, change. SystemManaged and release; owner's words; the draft saver's and each publisher's name (DATA), "Wombat's catalogue" for the seeder; seed-owned (N11); the preview's EPAs (S8); the JSON laid out (S11). |
| WorkflowParser, CreditRulesParser (S11) | An Indented overload of Serialize; the stored text and every caller unchanged (the seeders, ActivityTypeSeedCatalogue.cs:272-274, and the exact-text tests, ActivityTypePublishCheckTests.cs:117-118); applied in Map for the query and for each command's returned editor. SaveDraftCoreAsync and DiscardDraftAsync reload _workflowJson and _creditRulesJson from it (today only LoadEditorState does, :836-837). The parse exceptions carry JsonException.LineNumber, so "near ‹line›" can be said. |
| The refusal (S2) | A refusal is a list of (place, sentence): a typed refusal in Domain carrying IReadOnlyList (RefusalPlace: tab, kind, section key, field keys, move key, directive number, sentence). ActorFieldRules.EnsurePublishable (:155-217) and EvidenceEpa.EnsureCreditAgrees (:60-143) give one entry per problem, not one joined string; the parsers' 59 text-only throw sites (33 + 18 + 8) each give a place where they have one, else "no place: the summary line only"; the duplicate key's place names every row holding the key; a tab's badge counts its lines. |
| The refusal rule (S3) | S2's typed refusal, FieldRefusalException, the three parse exceptions, UnauthorizedAccessException and ValidationException are refusals, shown in their words; everything else (an InvalidOperationException that is not one of these, a DbUpdateException) is a failure in the fixed words with a line in the log, and RefusalText.Of no longer prints it. The key taken becomes a FieldRefusalException on Key (S10). "Published activity type keys cannot be changed." and "A saved draft is required before publishing." are guards no page reaches (Key is a read-out once published; S5). |
| RefusalSummary (S1) | Shared/Activities/RefusalSummary.razor (:25, :70) gains a line action, a callback in place of the fixed FocusFieldAsync and the id to focus (or the builder gets its own summary on the same markup); the line's press switches the tab, opens the row's editor in place and focuses the key, move or JSON editor; the prefix ("Form:", "Workflow, move Cancel:", "Details:") is made by the page from the refusal's place. |
| SaveActivityTypeDraftCommand (S10) | A validator for blank and too long (the save has [NoValidator] today, :24): blank Key "Enter a key.", blank Name "Enter a name." (CollegeHolders.cs:31's words), Key over 100 "A key is at most 100 characters.", Name over 200 "A name is at most 200 characters." (CollegeHolders.cs:33's words); the key taken (:92) a FieldRefusalException on Key. |
| ActivityForm (S8) | An optional EpaOptions parameter (it loads its own today, ActivityForm.razor:1211-1224; the builder passes none, ActivityTypeEdit.razor:508): a Speciality or Sub-speciality type, that discipline's active national EPAs; an Institution type, the EPAs of the curricula the institution has adopted; a Global type, none, with the preview's line. SectionHeadingLevel 3, not 4 (:508). Steps 1.25 and 1.27's "(now true)" rest on it. |
| ActorRuleWords (S9) | One helper in Domain beside ActorRule, the whole grammar to words: subject "The registrar"; creator "Whoever filed it"; a|b joined with "or" ("The registrar, or whoever filed it"); role:X "A ‹role label›" with its article ("A Coordinator or an Administrator"); field:k "The person ‹the field's label› names"; a+b "who is also"; scope:speciality "of the activity's speciality"; scope:subspeciality "of the activity's sub-speciality" (NEW); validation all "The whole form", owned "The mover's own fields", draft "Formats only"; anything it cannot word, the rule as written, in <code>. No board changes. |
| Entrustment scales (S12) | T271 item 2 is a service change: GetEntrustmentScaleOptionsAsync (ActivityReferenceDataService.cs:439-447) returns each scale's id and seed key (or the page calls the T253 resolver); the picker and the read-out resolve a "seed:" scale_key through it. |
| ActivityTypeEdit | Round 2's row (the tabs pattern; Details first; the Publish tab; no Back to list; the subtitle and owner line; the version strip; the editor in place, Done; Delete outline; labels; the scale picker; the read in words; the first save's token; Discard draft…; Q16; Q15; RefusalSummary; failures; Unavailable; loading; Publish aria-disabled), and: S5, PublishAsync returns at once when !CanPublish (today it tests only _inFlight and CanWrite, :915-941), so aria-disabled is inert, the reason wired by aria-describedby; Q16's save-first for a type that already has a draft, never on /new. S6, the Publish tab reads the SAVED draft; with unsaved edits it says "Save draft to see what publishing changes." above its list; the live alert above the tabs (:50-61, RefreshWarnings :1107-1119) goes. Q17, the order: Active newly unticked → Q15's question first; then save; then the warnings' question if any; then publish; Cancel on the first leaves everything unsaved. S13, Add field opens the new row's editor (:1021-1031, as today). N8, the roving tab keys: @onkeydown:preventDefault on the tablist, the key read in C# (chosen; no script). The tabs' badge in the tab's name (M2). The required line and aria-required (M1). |
| BuilderModels.GetPublishWarnings (N1) | Labels for field types, roles and the settings' fields, with "an Assessor" (T24); the quotes round a type go ("changes type from Long text to Text"); the pointer warnings (AddPointerWarning, :350-370) take keys today, so "Field 'Date delivered' will become the encounter date" looks the label up in the draft's schema (the published one's for a field the draft dropped); where the two labels differ the draft's wins. T133 (Q18): a setting at a deleted field is dropped at save (:86-124). |
| CatalogueLinks (N4) | ActivityTypeCreated(id) joins its Tokens list (CatalogueLinks.cs:48); the builder strips it on arrival (WithoutTokens). |
| Seed ownership (N11) | SeedActorUserId ("seed-system") moves from Infrastructure (ActivityTypeSeedCatalogue.cs:92) to Domain; seed-owned is one named predicate: the type's OwnerUserId and its newest version's publisher both the seeder (as ActivityTypeSeedRefresher.cs:56-80 decides). |
| Dates on Illustrations (N7) | Where a board shows ‹date› for a save no step dates, the code fills the South African day of that save. |
| FormSchemaParser:351 | The role refusal names roles by label. |
| app.css | C1's hover on --header-bg; R1 (NEW); S2 changed (.field-row-actions gone, buttons in .actions-cell, the forced-colors block, Add field's inset); C3 as a selector-list addition: .activity-types-page joins each of the six selector lists of "The catalogue tables stack from 1024px down" (app.css:7547-7600), e.g. ".epas-page .clinic-table--stack-wide thead, .colleges-page .clinic-table--stack-wide thead, .college-tree-page .clinic-table--stack-wide thead, .activity-types-page .clinic-table--stack-wide thead { … }" and likewise for tbody tr, tbody tr:last-child, td and tbody th, th[scope="row"], td[data-label]::before and .btn. |
| ContrastTests (S23) | TheActiveTab_TakesOnFill reads .tab-bar-tab.is-active; rewritten to [aria-selected="true"] (the underline on the page ground) in the same change. |
§ 4 The class table: every rule of PART 2 quoted in full
| Row | Classes | Kind | Why, and the rule |
| C1 | .tab-bar, .tab-bar-tab | CHANGED | The tabs pattern on the builder's own tab bar; the hover on --header-bg (S24)./* C1 (CHANGED; Spec row C1; T3, Q1, S2) — the builder's own tab bar as the tabs pattern. It replaces app.css's .tab-bar / .tab-bar-tab / .tab-bar-tab.is-active pill rules (used by the builder alone). role="tablist" on the bar, role="tab" with aria-selected on each button; the selected tab is told by a 3px underline in the action blue and by weight, not by a fill; one row at every width, each tab 44px high; equal tracks below 641px. */ .tab-bar { border-bottom: 1px solid var(--input-border); display: grid; gap: var(--space-xs); grid-auto-columns: max-content; grid-auto-flow: column; margin: 0; } .tab-bar-tab { align-items: center; background: none; border: 0; border-bottom: 3px solid transparent; border-radius: var(--radius-sm) var(--radius-sm) 0 0; color: var(--text-color); cursor: pointer; display: inline-flex; font-weight: 500; gap: var(--space-xs); justify-content: center; margin-bottom: -1px; min-height: 2.75rem; padding: 0 var(--space-md); white-space: nowrap; } .tab-bar-tab:hover { background: var(--header-bg); } /* C1 CHANGED (S24): the hover on header-bg; hover-bg is the page ground the bar stands on */ .tab-bar-tab[aria-selected="true"] { border-bottom-color: var(--secondary-color); color: var(--primary-color); font-weight: 700; } @media (max-width: 640.98px) { .tab-bar { gap: 0; grid-auto-columns: minmax(0, 1fr); } .tab-bar-tab { flex-direction: column; font-size: 0.9375rem; gap: 0; line-height: 1.2; padding: 0 2px; } } @media (forced-colors: active) { .tab-bar-tab[aria-selected="true"] { border-bottom-color: Highlight; } } |
| R1 | .tab-bar-tab .badge, .tab-bar-tab .badge > span | NEW (scoped) | The count is the shipped .badge.badge-declined; its word hidden below 641px (S21, S22)./* R1 (NEW, scoped; Spec row R1; S21, S22) — a tab's problem count is the shipped .badge.badge-declined, inside the tab and part of its name ("Form 1 problem"). Its padding a little tighter in a tab; below 641px its word is visually hidden (app.css's .visually-hidden declarations, copied), so a 72px track shows the number alone. */ .tab-bar-tab .badge { padding-inline: 6px; } @media (max-width: 640.98px) { .tab-bar-tab .badge > span { border: 0; clip: rect(0, 0, 0, 0); height: 1px; margin: -1px; overflow: hidden; padding: 0; position: absolute; white-space: nowrap; width: 1px; } } |
| C2 | .form-container > .choice-group; .builder-two-col's columns | CHANGED | The CSS-only Show choice where the columns stack. N23: it is CSS state, so switching tab and back sets it to "Edit the form" again; intended./* C2 (CHANGED; Spec row C2; Q5, T5) — the builder's two columns, and the CSS-only "Show" choice. Above the stacking width (the card 46.5rem and over) the choice is hidden and both columns stand; below it the columns stack, the choice shows, and the unchosen column is hidden by the chosen radio: no round trip. The choice is the form container's own .choice-group, so nothing outside the builder is touched. */ .form-container:has(> .builder-two-col) > .choice-group { display: none; } @container builder (width < 46.5rem) { .form-container:has(> .builder-two-col) > .choice-group { display: flex; flex-wrap: wrap; gap: var(--space-xs) var(--space-lg); margin: 0 0 var(--space-lg); } .form-container:has(> .choice-group .form-check:first-of-type input:checked) > .builder-two-col > :last-child { display: none; } .form-container:has(> .choice-group .form-check:last-of-type input:checked) > .builder-two-col > :first-child { display: none; } } .form-container:has(> .builder-two-col) > .choice-group legend { font-weight: 600; width: 100%; } .builder-two-col h2 { font-size: 1.25rem; margin: 0 0 var(--space-sm); } .builder-two-col > * > * + h2 { margin-top: var(--space-xl); } |
| S2 | .form-outline, .outline-section, .outline-section-head, .field-row, .field-row-label, .field-row-meta, .is-editing, .is-refused, .field-editor-inplace | CHANGED | The outline as a list (S27); its buttons a shipped .actions-cell; the stripes' forced-colors fallback (S28). Changed to make a board render: ".outline-section > .btn" gives Add field, standing alone at a section's foot, the row's inset./* S2 (CHANGED in round 3; Spec row S2; T4 argued again by S26; S27, S28). The outline: a section is a bordered block, its head on header-bg (its title an h3 alone, its key in a .field-row-meta <code> under it, then its buttons in a shipped div.actions-cell); its fields a ul.list-unstyled of li.field-row (the label, then type, key and Required, then the buttons in a div.actions-cell); the row open for editing and its editor (an li too) share T176's 4px stripe, a refused row a danger stripe (its words under it, so not colour alone). Add section and Add field stand alone. */ .form-outline { display: flex; flex-direction: column; gap: var(--space-md); } .outline-section { background: var(--surface-color); border: 1px solid var(--border-color); border-radius: var(--radius-lg); } .outline-section-head { background: var(--header-bg); border-bottom: 1px solid var(--border-color); border-radius: var(--radius-lg) var(--radius-lg) 0 0; display: flex; flex-direction: column; gap: var(--space-xs); padding: var(--space-sm) var(--space-md); } .outline-section-head > * { margin: 0; } .outline-section-head h3 { font-size: 1.1rem; margin: 0; overflow-wrap: anywhere; } .outline-section-head .field-row-meta code, .field-row-meta code { color: var(--muted-text); font-family: var(--font-mono); font-size: 0.875rem; font-weight: 400; overflow-wrap: anywhere; } .field-row { border-bottom: 1px solid var(--border-color); display: flex; flex-direction: column; gap: var(--space-xs); padding: var(--space-sm) var(--space-md); } .field-row:last-child { border-bottom: 0; } .field-row > p { margin: 0; } .field-row-label { font-weight: 600; overflow-wrap: anywhere; } .field-row-meta { color: var(--muted-text); font-size: 0.875rem; overflow-wrap: anywhere; } .field-row.is-editing, .outline-section-head.is-editing { box-shadow: inset 4px 0 0 var(--secondary-color); } .field-row.is-refused, .outline-section-head.is-refused { box-shadow: inset 4px 0 0 var(--danger-color); } .field-editor-inplace { border-bottom: 1px solid var(--border-color); box-shadow: inset 4px 0 0 var(--secondary-color); display: flex; flex-direction: column; gap: var(--space-md); padding: var(--space-md) var(--space-md) var(--space-md) var(--space-lg); } .field-editor-inplace > * { margin: 0; } .field-editor-inplace h4 { font-size: 1rem; margin: 0; overflow-wrap: anywhere; } .field-editor-inplace .form-grid { gap: var(--space-md); } /* Changed to make a board render (round 3, said on the Spec): Add field stands alone at a section's foot, so it takes the row's own inset rather than sitting on the section's border. */ .outline-section > .btn { margin: var(--space-sm) var(--space-md) var(--space-md); } @media (max-width: 640.98px) { .field-row .actions-cell .btn { flex: 1 1 auto; } } /* S28: the stripes in a contrast theme, as app.css does for .choice-group.is-invalid: a box-shadow is not painted there. */ @media (forced-colors: active) { .field-row.is-editing, .outline-section-head.is-editing { border-left: 4px solid Highlight; box-shadow: none; padding-left: calc(var(--space-md) - 4px); } .field-editor-inplace { border-left: 4px solid Highlight; box-shadow: none; padding-left: calc(var(--space-lg) - 4px); } .field-row.is-refused, .outline-section-head.is-refused { border-left: 4px solid CanvasText; box-shadow: none; padding-left: calc(var(--space-md) - 4px); } } |
| C3 | .activity-types-page (page class) | CHANGED: a selector-list addition; the nine rules are a canvas stand-in | The list stacks from 1024px (S25). The build adds the class to six selector lists (§ 3, app.css)./* C3 (CHANGED; Spec row C3; S25). STAND-IN: the build writes no declaration for the list. It ADDS .activity-types-page to each of the six selector lists of app.css's "The catalogue tables stack from 1024px down" block (beside .epas-page, .colleges-page and .college-tree-page). The canvas cannot edit bundle.css, so these nine rules stand in for that addition here and nowhere else. Round 2's comment follows. C3 (round 2; the 900-1100 sweep) — the list joins flow 16's shared narrow-desktop stack: from 1024px down its six columns stack, as the EPA list's, Colleges' and the College's tree do. Measured: at 950px the table is 682px in a 642px container (40px over; 45px for the Administrator's), at 1000px 692 in 692. Written here as the selector list it adds to (app.css: "The catalogue tables stack from 1024px down"); .activity-types-page is the list's page class (NEW, for this measured need alone), on its .people-page column. */ @media (max-width: 1024px) { .activity-types-page .clinic-table--stack-wide thead { border: 0; clip: rect(0, 0, 0, 0); height: 1px; margin: -1px; overflow: hidden; padding: 0; position: absolute; white-space: nowrap; width: 1px; } .activity-types-page .clinic-table--stack-wide tbody tr { border-bottom: 1px solid var(--border-color); display: grid; gap: var(--space-xs); padding: var(--space-sm) var(--space-md); } .activity-types-page .clinic-table--stack-wide tbody tr:last-child { border-bottom: 0; } .activity-types-page .clinic-table--stack-wide td, .activity-types-page .clinic-table--stack-wide tbody th { border: 0; padding: 0; width: auto; } .activity-types-page .clinic-table--stack-wide th[scope="row"] { display: block; } .activity-types-page .clinic-table--stack-wide td[data-label]::before { color: var(--muted-text); content: attr(data-label) ": "; content: attr(data-label) ": " / ""; font-weight: 600; } .activity-types-page .clinic-table--stack-wide .btn { min-height: 2.75rem; } } |
| gone | .tab-problem, .field-row-actions | removed | The badge is shipped (.badge.badge-declined); the row's buttons a shipped .actions-cell. |
| reuses | .badge, .badge-declined, .list-unstyled, .actions-cell, and round 2's list (.page, .sidebar, .nav-*, .account-row, .breadcrumbs, .header-container, .page-subtitle, .people-page, .person-cell, .search-container, .search-grid, .search-field, .filter-actions, .table-container, .clinic-table, .clinic-table--stack, .clinic-table--stack-wide, .col-actions, .user-page, .user-section, .detail-card, .detail-card--empty, .state-panel-title, .state-panel-copy, .form-container, .form-grid, .form-group, .full-width, .form-control, .form-select, .form-check, .choice-group, .field-help, .validation-message, .required-mark, .detail-list, .alert, .alert-row, .action-result, .error-summary, .validation-summary-errors, .refusal-summary-list, .loading-line, .skeleton, .skel-*, .activity-form, .form-section, .form-section-head, .dialog-card, .dialog-scrim, .dialog-body, .form-actions, .btn and its kinds, .muted, .text-sm, .font-mono, .visually-hidden) | reuses | as shipped, never redeclared |
| K1–K3 | .cv-chip, .cv-modal-stage, .cv-doc, .cv-cols, .cv-first | canvas-only (PART 1) | Kept as round 2 wrote them; never shipped. |
- T4, argued again (S26). Round 2 said a DataTable per section leaves its label column 93px beside four btn-sm buttons (204px). Corrected: four btn-sm are about 220px with their gaps (measured this round at 223–230px on one line), and .actions-cell wraps (app.css:461), so 93px is not what a DataTable would give. Round 2's figure was derived, not rendered.
- The outline stands for two other reasons: a row of Edit, Up, Down and Delete under a label never leaves a readable measure in the 363px rail, wrapped or not; and DataTable stacks by a viewport query (app.css:4144, :6278) that does not fire in a rail at 1280.
- Recorded: a DataTable of Field and Edit alone would fit, but Up and Down are the keyboard's reorder path (T019-b), so they stay on the row.
- The standing boards' div rows (R2-C09, R2-C16, R2-C21, R2-E08, R2-E13, R2-E14) are superseded by this markup: a ul.list-unstyled of li.field-row.

######## BOARD R3-0-spec-3.dc.html
Label: R3-0-spec-3 · Spec (3 of 4)
### Flow 17 · Round 3 · Spec (3 of 4)
#### § 5 The numbers
From the check board (R3-0-check-2), each rendered:
THE TABS AT 390 (each tab: track / content scrollWidth, px)
| Board @ width | Tabs | Result |
| R3-C14-390 @390 | Details 72/72; Form 1 problem 72/72; Workflow 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-C14-390 @640 | Details 122/122; Form 1 problem 122/122; Workflow 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-C14-390 @641 | Details 78/78; Form 1 problem 137/137; Workflow 97/97; Credit 73/73; Publish 83/83 | all fit, 44px |
| R3-C20-390 @390 | Details 72/72; Form 1 problem 72/72; Workflow 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-C20-390 @640 | Details 122/122; Form 1 problem 122/122; Workflow 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-C20-390 @641 | Details 78/78; Form 1 problem 137/137; Workflow 97/97; Credit 73/73; Publish 83/83 | all fit, 44px |
| R3-D01-390 @390 | Details 72/72; Form 72/72; Workflow 1 problem 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-D01-390 @640 | Details 122/122; Form 122/122; Workflow 1 problem 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-D01-390 @641 | Details 78/78; Form 68/68; Workflow 1 problem 168/168; Credit 73/73; Publish 83/83 | all fit, 44px |
| R3-D04-390 @390 | Details 72/72; Form 72/72; Workflow 72/72; Credit 1 problem 72/72; Publish 72/72 | all fit, 44px |
| R3-D04-390 @640 | Details 122/122; Form 122/122; Workflow 122/122; Credit 1 problem 122/122; Publish 122/122 | all fit, 44px |
| R3-D04-390 @641 | Details 78/78; Form 68/68; Workflow 97/97; Credit 1 problem 144/144; Publish 83/83 | all fit, 44px |
| R3-C22-390 @390 | Details 1 problem 72/72; Form 72/72; Workflow 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-C22-390 @640 | Details 1 problem 122/122; Form 122/122; Workflow 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-C22-390 @641 | Details 1 problem 149/149; Form 68/68; Workflow 97/97; Credit 73/73; Publish 83/83 | all fit, 44px |
| scratch copy, a badge of 12 @390 | Form 12 problems: track 72, content 72, height 44 | fits |
| scratch copy, a badge of 12 @640 | Form 12 problems: track 122, content 122, height 44 | fits |
| scratch copy, a badge of 12 @641 | Form 12 problems: track 148, content 148, height 44 | fits |
THE OUTLINE (R3-C11 and R3-C19)
| Board @ width | Rail or column | A field row's four buttons | Checks |
| R3-C11 @1280 | 363px (rail) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @1116 | 320px (rail) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @1115 | 743px (stacked column) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @900 | 528px (stacked column) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @768 | 396px (stacked column) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @390 | 292px (stacked column) | 1 line, 258px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @1280 | 363px (rail) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @1116 | 320px (rail) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @1115 | 743px (stacked column) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @900 | 528px (stacked column) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @768 | 396px (stacked column) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @390 | 292px (stacked column) | 1 line, 258px | clipped 0; label under a button: no; sideways: no |
THE LIST: the table's min-content width (a clone at width: min-content) against its .table-container
| Board @ width | Min-content | Container | Stacked? | Spare or over (px) |
| R3-A13 @1000 | 275 | 692 | stacked | spare 417 |
| R3-A13 @1024 | 275 | 716 | stacked | spare 441 |
| R3-A13 @1025 | 682 | 717 | a table | spare 35 |
| R3-A13 @1050 | 682 | 742 | a table | spare 60 |
| R3-A13 @1100 | 682 | 792 | a table | spare 110 |
| R3-C08-list @1000 | 275 | 692 | stacked | spare 417 |
| R3-C08-list @1024 | 275 | 716 | stacked | spare 441 |
| R3-C08-list @1025 | 682 | 717 | a table | spare 35 |
| R3-C08-list @1050 | 682 | 742 | a table | spare 60 |
| R3-C08-list @1100 | 682 | 792 | a table | spare 110 |
THE HEADER: where the actions sit
| Board @ width | h1 | Actions |
| R3-C03 @1280 | 1 line | Save draft beside h1 (x 925, y 120, 36px); Publish beside h1 (x 1033, y 120, 36px); Discard draft… beside h1 (x 1124, y 120, 36px) |
| R3-C03 @1100 | 1 line | Save draft beside h1 (x 745, y 120, 36px); Publish beside h1 (x 853, y 120, 36px); Discard draft… beside h1 (x 944, y 120, 36px) |
| R3-C03 @1024 | 1 line | Save draft beside h1 (x 669, y 120, 36px); Publish beside h1 (x 777, y 120, 36px); Discard draft… beside h1 (x 868, y 120, 36px) |
| R3-C03 @1023 | 1 line | Save draft under subtitle (x 282, y 239, 36px); Publish under subtitle (x 390, y 239, 36px); Discard draft… under subtitle (x 481, y 239, 36px) |
| R3-C03 @390 | 1 line | Save draft under subtitle (x 16, y 270, 44px); Publish under subtitle (x 133, y 270, 44px); Discard draft… under subtitle (x 233, y 270, 44px) |
| R3-E16 @1280 | 1 line | Save draft beside h1 (x 925, y 120, 36px); Publish beside h1 (x 1033, y 120, 36px); Discard draft… beside h1 (x 1124, y 120, 36px) |
| R3-E16 @1100 | 1 line | Save draft beside h1 (x 745, y 120, 36px); Publish beside h1 (x 853, y 120, 36px); Discard draft… beside h1 (x 944, y 120, 36px) |
| R3-E16 @1024 | 1 line | Save draft beside h1 (x 669, y 120, 36px); Publish beside h1 (x 777, y 120, 36px); Discard draft… beside h1 (x 868, y 120, 36px) |
| R3-E16 @1023 | 1 line | Save draft under subtitle (x 282, y 239, 36px); Publish under subtitle (x 390, y 239, 36px); Discard draft… under subtitle (x 481, y 239, 36px) |
| R3-E16 @390 | 1 line | Save draft under subtitle (x 16, y 270, 44px); Publish under subtitle (x 133, y 270, 44px); Discard draft… under subtitle (x 233, y 270, 44px) |
THE 44 PX CHECK: 952 controls on the R3 boards at 390, none under 44px. The 390 scope: .people-page on the list, .filter-actions, StackWide's row buttons (from 900px; from 1024px with C3); .user-page on the builder (buttons, inputs, selects, a choice-group radio's label, a lone .form-check row: Active, Required); C1's tabs; S2's phone line; .dialog-card's stacked actions. The two columns stand side by side from 1116px (rail 320px there, 363px at 1280) inside .user-page's 60rem.
#### § 6 DESIGN.md
- § Builder layout (round 2's rewrite, and): the tab bar stands on the page ground above the card, and the card is the tabpanel (DESIGN.md:1811's "inside the surrounding .form-container" rewritten; :237's "badges, tabs" for --radius-pill corrected, the tab now square-topped); a tab's problem count is the shipped .badge.badge-declined inside the tab and part of its name, its word hidden below 641px (R1); the outline is a list (S27), its rows' buttons a shipped .actions-cell; Add field opens the new row's editor (S13); the required-field pattern in the builder: "* marks a field you must fill in." at the head of Details and of each editor in place, aria-required on every starred control, a starred radio group a role="radiogroup" with aria-required (M1).
- § Button system: round 2's, and the Publish dialog's confirm a primary (S7); Publish off inert (S5); with Active newly unticked, Publish asks Q15's question first (Q17).
- § Form system: round 2's, and the stripes' forced-colors fallback (S28): a 4px Highlight edge for an open row or editor, CanvasText for a refused one.
- § Table system: round 2's; the list in the 1024px stack by a selector-list addition (C3).
- § Page-level patterns: round 2's.
#### § 7 Tokens and contrast
- Every colour an existing token (tokens.css v49). Restated on the page ground, where the tab bar stands (S23):
- Selected tab: --secondary-color underline on --background-color 4.61:1; its label --primary-color 10.42:1. Unselected label --text-color 11.99:1.
- Hover (S24): the tab on --header-bg: --text-color 11.36:1, --muted-text 4.57:1.
- .badge-declined: --text-color on --danger-bg 11.81:1; its edge --danger-color 5.56:1 on its tint.
- --muted-text on --header-bg 4.57:1 (the section head's key and the row meta, N24), beside the surface's 5.09:1 and the page ground's 4.83:1.
- Publish's reason, the scope line and the owner line: --muted-text 5.09:1 on the surface, 4.83:1 on the page.
- White on Publish (btn-success) 5.88:1; on the dialog's primary confirm (btn-primary, --secondary-color) 4.86:1; off, aria-disabled at 65% and exempt.
- Stripes: --danger-color on the surface 5.95:1; --secondary-color 4.86:1; the focus ring 4.86:1 on the surface, 4.61:1 on the page.

######## BOARD R3-0-spec-4.dc.html
Label: R3-0-spec-4 · Spec (4 of 4)
### Flow 17 · Round 3 · Spec (4 of 4)
§ 8 DATA by query
| Query | Read |
| ListActivityTypesAdminQuery | The owner's words per row; SystemManaged with its release; CanWrite false when system-managed; whether the caller's College has a discipline (N12). |
| GetActivityTypeEditorQuery | Null for unknown or unreadable; SystemManaged and release; the owner's words; the draft saver's name; each publisher's name, "Wombat's catalogue" for the seeder; seed-owned as one named predicate (N11); the preview's EPA list by the type's own rule (S8); the JSON laid out (S11). |
| The may-read rule (Q19) | Read access ignores SystemManaged: an inactive system-managed type stays readable (View) to the roles that read it today. |
| Entrustment scale options (S12) | Each scale's id and seed key, so a "seed:" scale_key resolves to its name. |
| SaveActivityTypeDraftCommand | Each refusal's place (S2). |
| Read today, not DATA | The draft's date (DraftUpdatedOn) and "None"; each version's number and date. |
§ 9 Every message by kind
| Kind | Words | Source |
| result | "Draft saved." · "Published version 1." · "Published version 2." · "Draft discarded." | built |
| result | "Draft saved. Nothing was published." (Q16's Cancel) · "Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it." · "Draft saved. KGK Teaching Session Log is active again: registrars are offered it." | round 2's (T14, T17); the first is words only, no board (N5) |
| result | "Published version 2. KGK Teaching Session Log is inactive: registrars are no longer offered it." · "Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it. Nothing was published." | NEW (Q17), words only |
| refusal (summary) | "Nothing was saved." + "Everything you typed is kept."; lines "Form: Field key 'topic' is used by 2 fields (…); give each field its own key." · "Workflow, move Cancel: Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a field: rule must name a User field." · "Form, section Section 2: Section 'section_2' must contain at least one field." · "Details: An activity type with key 'kgk_teaching_log' already exists." (built sentence, SaveActivityTypeDraftCommand.cs:92) | built sentences after a prefix |
| refusal (summary) | "Credit: Credit rule 1 reads its EPA from field 'epa', but the form's EPA field is 'epa_id'. The EPA an activity is filed against and the EPA it credits must be the same field." (EvidenceEpa.cs:107) | NEW prefix (S4), replaces "Credit, rule 1: … which is not a field of the form." |
| refusal (at the place) | "Key 'topic' is also the key of Audience. Give each field its own key." / "… of Topic. …" · "Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody." · "This section has no fields. Add a field, or delete the section." · "An activity type with key 'kgk_teaching_log' already exists." (under Key) | round 1's and round 2's; built |
| refusal (at the place) | "Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'." | NEW (S4), replaces "… which is not a field of this form. Use the form's EPA field, 'epa_id'." |
| refusal (Details, S10) | blank Key "Enter a key." (NEW); blank Name "Enter a name." (built words); Key over 100 "A key is at most 100 characters." (NEW); Name over 200 "A name is at most 200 characters." (built words) | words only |
| refusal (guard) | "You do not have permission to modify activity types in that speciality." · "Nothing was saved. Multi-Source Feedback (Paediatrics) is written by the MSF release, so it cannot be changed here." (NEW, N10, replaces "Not saved. …"); no page offers either's control | built; NEW |
| refusal (gone) | "Date delivered is no longer in the form. Choose another Date field, or None." (round 2's R2-C18): dropped everywhere (Q18) | — |
| failure | "The draft could not be saved. Nothing has changed. Try again, or come back in a few minutes." (R2-E13) · "Version 2 could not be published. …" · "The draft could not be discarded. …" (the last two words only, no board: N5) | round 2's |
| load error, loading, empty, unavailable, notice | As round 2's: "Could not load the activity types. …" / "Could not load this activity type. …"; "Loading the activity types." / "Loading this activity type."; the empty and no-match states; "Activity type unavailable" / "This activity type does not exist, or you cannot open it."; T300's three, Q9's two release notices, the no-scope notice | round 2's |
| empty / subtitle | "The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change." | NEW (N12), replaces T11's for that case |
| hint | "Save a draft to publish." · "There is no draft to publish: version 1 is what registrars file. Save a draft to publish." · "Nothing is published yet. Save a draft to publish." · "Not published yet." · "Nothing on the form changes for activities already filed." · "Save draft to see what publishing changes." (NEW, S6, words only) | round 2's; NEW |
| hint | Details: Q14's line; "* marks a field you must fill in." (DESIGN.md:1329, :1510; M1); Active's help "An inactive type is not offered to anyone filing an activity. Activities already filed are not changed." (NEW, N2); the JSON help; the seed lines; Credits nothing's line; the preview's person line | round 2's; NEW |
| hint (round 2's, never listed: S16) | "None: a Global type belongs to no institution or speciality." (R2-C07, Target with Global: NEW in round 2, replaced nothing) · "No fields in this section yet." (R2-C20, an empty section's row: NEW in round 2, replaced nothing) | round 2's |
| question | "Discard the draft of KGK Teaching Session Log?" + Q14's published body · "Discard the draft of CPSA Case Presentation Log?" + Q14's never-published body · "Publish version 2 of KGK Teaching Session Log?" + "Publishing makes this draft version 2. It changes:" · "Deactivate KGK Teaching Session Log?" + "Registrars are no longer offered it when filing. Activities already filed are not changed." (NEW, N2) / "You can reactivate it on this page." | Q8, Q14, Q16, Q15 |
| names | A tab with a problem: "Form 1 problem" (M2); below 641px the word " problem" visually hidden, still in the name (S22). The folds: "Hide fields of section ‹title›" / "Show fields of section ‹title›" (M2(b)) | NEW |
| warnings | Round 2's eight by label, with "Field 'Date delivered' will be removed." and "This form will stop recording which field carries the encounter date." drawn on R3-C18b; and the pinned line with Date delivered | built, by label |
| read | S9's scope words: "of the activity's speciality"; "of the activity's sub-speciality" (NEW) | S9 |
§ 10 Edge cases
| Case | Answer | Board |
| A type with no sections | The parser refuses it: "Schema must contain at least one section." | words only |
| A section with no fields | Refused at save, at the section's head and in the summary | R3-C20 |
| A 40-field form | Sections fold (T23); a long label wraps clear of its buttons; drawn after Hide fields on sections 2 to 6 | R3-C19 |
| A field's key changed after publish | A removal and an addition in the warnings | words only |
| A setting at a deleted field (Q18) | Dropped at Save draft (T133, BuilderModels.cs:86-124), saved without a word; the Publish tab warns; between the delete and the save the select shows the field's key as today's page does (EligibleFieldKeys keeps it, ActivityTypeEdit.razor:43-66), not marked invalid; no board for that moment | R3-C18, R3-C18b |
| Publish with Active newly unticked (Q17) | Q15's question first; Cancel leaves everything unsaved and returns the focus to Publish | R3-E16, R3-E17 |
| The key taken | Refused on Details, the summary and under Key, Details 1 problem | R3-C22 |
| A blank or long key or name | "Enter a key." / "Enter a name." / "A key is at most 100 characters." / "A name is at most 200 characters." | words only |
| A draft saved by someone else | The strip names the saver; Save draft overwrites it, no lock | words only |
| A seed-owned type with a draft | T18's note | R2-E15 |
| A College admin with no scope | /new: the notice alone; the list: no New activity type, N12's subtitle | R2-C08-new, R3-C08-list |
| An inactive type | Status Inactive, Draft Saved ‹date›; Q15 | R3-A12, R3-E11, R3-E12 |
| An unpublished type to one who may not write it | Unavailable | R2-B09 |
| A 2,000-character description | "2,000 of 2,000 characters"; the read-out wraps | words only |
| The starting draft's section "Details" beside the Details tab | One name for two things until renamed | R2-C09 |
| Malformed JSON | "The workflow is not valid JSON. Check the brackets and commas near ‹line›.", the line from JsonException.LineNumber (S11) | words only |
#### § 11 Later, not built
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

######## BOARD R3-0-steps.dc.html
Label: R3-0-steps · Steps
### Flow 17 · Round 3 · Steps
The counting rule, unchanged from round 2: a journey counts presses + fields. A press is a click on a button, tab, link or checkbox; a field is a text typed or a choice made in a select or a radio group (the Preview choice included: it is a radio), and is not also a press. A menu link is a press; at 390 opening the menu is one more. A dialog's confirm is a press. A field that arrives chosen counts as none. A page the app goes to by itself is no press.
11 steps: 1.24–1.31 (8), 6.14a, A.7.9 and A.7.14 (their builder parts).
#### The Expects
Each step, its boards, and its Expect once built (round 2's rows, amended)
| Step | Boards | Do and Expect: today → once built |
| 1.24 | R2-A02, R3-A13 | "Each reads Scope `Speciality · Paediatrics`, Published `v1`, Draft `None` and Active" → "Set by CPSA (Speciality · Paediatrics), Published Version 1, Draft None and Active"; the search applied with Show, "12 of 22 activity types match “cpsa”." above the table; Clear brings back the 22 rows and "22 activity types.", the focus on the search input (R3-A13); View named for the row (unchanged); New activity type offered (unchanged). Do → "search `cpsa`, press Show and read the list. Then press Clear." |
| 1.25 | R2-B01–B05 | As round 2's row: "leave by the trail's Activity types"; Form settings by label; types by label; View under its row, the scale by name; Details with Set by and Versions; Workflow and Credit in words; no JSON; the preview's EPAs the fifteen PAED EPAs (now true by S8's EpaOptions). |
| 1.26 | R3-C01–C03 | As round 2's row: it opens on Details; Scope a read-out, "The one scope you can write in."; the first Save draft at /admin/activity-types/23 reads "Draft saved." focused; the strip Not published / Saved 2026-11-05 by Nolwazi Mbatha; and "* marks a field you must fill in." at the head of Details. |
| 1.27 | R2-C09, R3-C10–C15, R2-C16, R3-C23 | As round 2's row, and: Add field opens the new row's editor under it, Field key focused (R3-C23), so no Edit is pressed for a new field; Names a by label; the refusal after "Form:" and at both fields; Form carries "1 problem" in its name; the pickers by label; the preview's EPAs (now true by S8). |
| 1.28 | R3-D01, R2-D02 | As round 2's row; the JSON editor holds the JSON as typed until the save, then laid out. |
| 1.29 | R2-D03 | As round 2's row. |
| 1.30 | R3-E02, R3-E02b, R2-E03 | Do: "Press Publish" (from the Credit tab, where 1.29 leaves it). Expect: "Published version 1." focused above the tabs, Credit still selected (R3-E02); Discard draft gone; Publish unavailable (aria-disabled) with "Save a draft to publish." beside it; then Details: the Key a read-out, "Fixed: version 1 is published.", the result still in its region (R3-E02b); the Publish tab: the versions one row (R2-E03); no warnings and no question. |
| 1.31 | R2-A01, R2-A03, R2-A04 | As round 2's row. |
| 6.14a | R2-A06, R2-A07, R2-B06, R3-C04–C06, R2-B09 | As round 2's row (Scope's radios a required radio group, M1). |
| A.7.9 (builder) | R2-E14-390, R2-C21-390 | As round 2's row. |
| A.7.14 (Publish) | R3-E02 | As round 2's row: unavailable (aria-disabled), exempt, its reason 4.5:1; white on Publish 5.88:1. |
#### states.md
states.md rows that change (T3, T7: the R3 ids)
| Row | Change | Board |
| 902 | Searched, 1.24: applied with Show, "12 of 22 activity types match “cpsa”." above the table | R2-A02 |
| 903 | "Draft reads 'Draft saved'" → "Saved 2026-11-05" | R2-A05 |
| 904 | All, 1.31: Set by in words, Version 1, "23 activity types." | R2-A01 |
| 905 | the no-match state | R2-A04 |
| 906 | the list's Loading: "Loading the activity types." over a skeleton | R2-A10 |
| 907 | narrow: stacked, Edit or View 44px | R2-A01-390 |
| 908 | the Form tab, by label | R2-B01 |
| 909 | "New, the default draft" on Details | R3-C01 |
| 910 | "Metadata tab" → "Details tab", "Draft saved." focused | R3-C03 |
| 911 | the User field editor: in place under its row, Names a by label | R3-C13 |
| 912 | Duplicate key refused: the summary and both fields' words | R3-C14 |
| 913 | the refusal at the move and the summary | R3-D01 |
| 914 | "Credit tab": in words | R2-D03 |
| 915 | Publish unavailable with its reason in text | R3-E02 |
| 916 | warnings on the Publish tab, by label | R3-E04 |
| 917 | Discard asks first, Q14's body | R3-E07, R2-E08 |
| 918 | Unavailable's words | R2-B09 |
| 919 | the loading line | R2-B10 |
| 920 | narrow: one row of tabs, the choice of two | R2-E14-390 |
#### Runbook hits
- "Back to list": 1.25 Do and Actual (act-1-setup.md:789, :820); 1.31 Do (:958)
- "Metadata": act-1-setup.md:745, :800, :816, :829, :952; act-6-catalogue.md:444
- "code block": act-1-setup.md:802, :804, :817
- "on the Form tab" / "on Form": 1.25 :791 and :812 (stay true); 1.26 :836 and :849; 6.14a act-6-catalogue.md:462, :479
- "v1" in the list: 1.24 :751, 1.31 :962
- T4: 6.14a's "Draft saved" and "v0" are both on act-6-catalogue.md:483 (:482 is the "no 'Draft saved.'" clause); 6.14a Expect :468
- T4 added: 1.26's Scope (act-1-setup.md:839-841, a read-out now); 6.14a's Scope (act-6-catalogue.md:459-461, radios and a read-out now)
- "could not be found": 6.14a :470, :484; states.md:918
- "Save a draft to publish." as a hint: 1.30 :947, :951
- the builder's "stack in one column" and tab bar: appendix-cross-cutting.md:1708, :1732; "disabled" Publish: :1872
- "Configured activity types": no hit. Act 2's filing is flow 03's page: unchanged.
#### Counts
Each journey counted (presses + fields), recounted under the rule
| Step | Presses and fields named | 1280 | 390 | Today | Recount |
| 1.24 | Activity types, Show, Clear; `cpsa` | 3 + 1 | 4 + 1 (Menu) | 1 + 2 | unchanged |
| 1.25 | View, View overall_level, Details, Workflow, Credit, the trail | 6 + 0 | 6 + 0 | 6 + 0 | unchanged |
| 1.26 | New activity type, Save draft; Key, Name, Description | 2 + 3 | 2 + 3 | 3 + 3 | unchanged |
| 1.27 | Presses: Form, Edit (section), Edit (Title), Add field ×5 (each opens its editor, R3-C23), Required ×4, Save draft, the summary's line, Save draft. Fields: section key and title 2, Title's key and label 2, five keys and labels 10, five type picks 5, Audience's options 1, the key retyped 1, two Form-settings picks 2 (the rating's None arrives chosen) | 15 + 23 | 15 + 25 (Preview and back are fields) | 14 + 23 | Round 2 carried 15 + 24 and 14 + 24: the fields were 23, not 24 (T1). Today's recount from the runbook's Do on today's page: Form, Edit section (it opened Title too), Add field ×5 (each opens its editor, :1029), Required ×4, Save draft, Edit Audience, Save draft: 14 presses; the same 23 fields |
| 1.28 | Workflow, Save draft, Save draft; the JSON, the actor | 3 + 2 | 3 + 2 | 3 + 2 | unchanged |
| 1.29 | Credit | 1 + 0 | 1 + 0 | 1 + 0 | unchanged |
| 1.30 | Publish (from Credit), then Details and the Publish tab for its Expect | 3 + 0 | 3 + 0 | 1 + 0 | Round 2 carried 1 + 0; the Expect reads Details and the Publish tab (T2) |
| 1.26–1.30 |  | 24 + 28 | 24 + 30 | 22 + 28 | Round 2 carried 22 + 29 (390: 24 + 29); today's 22 + 29 → 22 + 28 by T1's recount |
| 1.31 | the trail, Show; `KGK` | 2 + 1 | 2 + 1 | 1 + 1 | unchanged |
| 6.14a | Activity types, View (MSF), the trail, New activity type, Save draft, the trail; Key, Name, Description, Sub-speciality, Speciality | 6 + 5 | 7 + 5 | 7 + 5 | holds (the Scope radios were fields) |
| A.7.9 (builder) | Menu, Activity types, Edit; Preview (a field) | — | 3 + 1; the preview at 0px of scrolling | about 4,390px down a 5,245px page | Round 2 carried 4 + 0: Preview is a radio, a field (T1) |

######## BOARD R3-0-check.dc.html
Label: R3-0-check · Check (1 of 2)
### Flow 17 · Round 3 · Check (1 of 2)
Every id of the round-3 message, from the boards' text and markup. Round 3 is drawn on its own canvas, "Wombat 17 · Round 3"; the R2 boards that stand are on "Wombat 17".
First: the reference files. Everything arrived and opened; nothing was unreadable, and nothing was drawn from memory.
- project/r2/: all 145 round-2 board files (R2-….ref.html, the four document boards included) and f17r2.css, read whole. Each R3 board was drawn from its R2 file: the 145 files are byte for byte the round-2 boards, and every R3 board was then compared line by line against the R2 file its row names, so what no item changes is as that file has it.
- The 30 captures under project/ref/baseline/: all present (30 names, each the bytes round 2 opened). Viewed for this round's boards: act-1/1.24-1-types-search-cpsa.png (R3-A13); act-1/1.26-1-new-type-default.png and act-1/1.26-2-first-save-no-status.png (R3-C01–C03); act-1/1.27-1-duplicate-key-refused.png (R3-C14, C15, C23); act-1/1.27-3-user-field-names-a.png (R3-C13); act-1/1.28-1-field-rule-refused.png (R3-D01: the JSON as typed); act-1/1.29-1-credit-tab.png and act-1/1.30-1-published-v1.png (R3-E02); act-1/1.30-2-metadata-key-locked.png (R3-E02b); act-6/6.14a-2-kruger-msf-cpsa-open.png (R3-B07's kin); act-6/6.14a-3-new-type-scope.png (R3-C04, C05); states/activity-type-edit--publish-warnings.png (R3-E04, C18b); states/activity-type-edit--discarded.png (R3-E07). No word on a capture contradicts a board where the board does not mean to change it: the captures show today's defects (Metadata, v1, "epa_id · Epa", the live warnings alert, the select of field keys, Back to list), which the boards replace on purpose.
- The 9 seed files under project/ref/seeds/: mini_cex_cpsa/schema.json, mini_cex_cpsa/workflow.json, mini_cex_cpsa/credit.json, reflective_exercise_cpsa/schema.json, reflective_exercise_cpsa/workflow.json, reflective_exercise_cpsa/credit.json, teaching_session/schema.json, teaching_session/workflow.json, teaching_session/credit.json, all read. Learner Feedback's and MSF's help texts are from your FIXTURES (their seeds, learner_feedback_cpsa/schema.json and msf_cpsa/schema.json, are not in project/ref/).
- The brand mark is this canvas's own copy, /_blob/1464be2bd2c027ed86bec42ee064aa32, on every R3 board.
#### Item by item
Each item: done or not done, and where
| Item | Done? | Boards, or Spec / Steps |
| M1 | done | R3-C01–C07, C15-before, C22, E02b, E11, E12, E16, E17 (Details: the line under Q14's, aria-required on Key, Name, Sub-speciality's select; Scope's radiogroup role and aria-required); R3-C10–C14, C15-after, C17, C19, C23 (the editor: the line under its h4; Field key, Label, Type, and a Choice's Options); twins |
| M2 | done | (a) R3-C14, C15-before, C15-after, C20, D01, D04, C22: no aria-label or aria-describedby on any tab; the name is the words, "Form 1 problem". (b) R3-C19: "Hide fields of section ‹section 1›" / "Show fields of section ‹section 2›", no aria-expanded, aria-controls the section's list |
| M3 | done | Steps T1: the Preview radio counted as a field |
| M4 | done | R3-E02 on Credit; R3-E02b; Steps T2 |
| M5 | done | Steps T3: rows 902, 904, 906, 911, 912 |
| S1 | done | Spec § 3 (RefusalSummary row) |
| S2 | done | Spec § 3 (RefusalPlace) |
| S3 | done | Spec § 3 (the refusal rule) |
| S4 | done | R3-D04: "Credit: " + EvidenceEpa.cs:107's sentence; the new words under the JSON |
| S5 | done | Spec § 3 (PublishAsync) |
| S6 | done | Spec § 3 and § 9 (no board) |
| S7 | done | R3-E05: the dialog's confirm btn-primary; the header's Publish btn-success |
| S8 | done | Spec § 3 (ActivityForm EpaOptions; SectionHeadingLevel 3) |
| S9 | done | Spec § 3 (ActorRuleWords and its table) |
| S10 | done | R3-C22 (summary, Key's FieldRefusal, aria-invalid, Details 1 problem); Spec § 3 and § 9 (the four words) |
| S11 | done | Spec § 3 |
| S12 | done | Spec § 3 |
| S13 | done | R3-C23; Spec § 6; Steps T5 |
| S14 | done | R3-A12: KGK's Draft "Saved ‹date›" |
| S15 | done | Chips by the one rule: D05 and E01 gain it; E04 and E07 lose it; C18, C18b, C22, E16, E17 carry it; A13, C23, E02b do not. Spec § 2 lists the 29 |
| S16 | done | Spec § 9 |
| S17 | done | R3-C19: "after Hide fields on sections 2 to 6" in its title and chip |
| S18 | done | R3-A13; Steps 1.24 |
| S19 | done | R3-B07: Learner Feedback's help texts under its preview fields |
| S20 | done | Steps T4 |
| S21 | done | The badge is .badge.badge-declined on R3-C14, C15-before, C15-after, C20, D01, D04, C22; no .tab-problem on any board or in f17r3.css |
| S22 | done | The 390 twins: the number alone (R1); measured below |
| S23 | done | Spec § 6 and § 7 (no board) |
| S24 | done | f17r3.css C1; Spec § 7 |
| S25 | done | f17r3.css C3 stand-in; Spec § 3 and § 4; THE LIST below |
| S26 | done | Spec § 4 (T4's argument corrected; the measure derived, not rendered) |
| S27 | done | R3-B07, C10–C14, C15-after, C17, C18, C19, C20, C23, E07: ul.list-unstyled of li.field-row; div.actions-cell; the h3 the title alone, its key a p.field-row-meta ; Add section and Add field standalone |
| S28 | done | f17r3.css S2's forced-colors block; Spec § 4 |
| S29 | done | R3-C19: ids sec-section-1, fe-section-1-field-3, fields-section-2 …; no id holds a bracket or space (checked on every R3 board) |
| N1 | done | Spec § 3 (GetPublishWarnings) |
| N2 | done | R3-C01–C07, C15-before, C22, E02b, E11, E12, E16, E17 (Active's help); R3-E11, E16 (the dialog) |
| N3 | done | Spec § 3 |
| N4 | done | Spec § 3 |
| N5 | done | Spec § 9 |
| N6 | done | No aria-describedby="" on any R3 board (checked) |
| N7 | done | Dates stay ‹date›; Spec § 3 says the code fills the SA day of that save |
| N8 | done | Spec § 3 (the handler named) |
| N9 | done | R3-C11, C12, C13, C17, C23 (and every select drawn): the built order, the chosen one selected in place |
| N10 | done | Spec § 9 |
| N11 | done | Spec § 8 |
| N12 | done | R3-C08-list, R3-A09-nocreate; Spec § 3 and § 8 |
| N13 | done | Spec § 3 |
| N14 | done | = S5 |
| N15 | done | R3-D01: the JSON as 1.28 types it, cancel's actor subject|field:topic |
| N16 | done | Spec § 2 (FIXTURES) |
| N17 | done | Spec § 2; no bracket as any control's value on any R3 board (checked): R3-C19's open editor is empty, its preview's 12-option select offers none |
| N18 | done | Spec § 2: "key under Name in monospace" |
| N19 | done | R3-E12's chip says its state; R3-E04 has none; R2-B08's "Q9" stands, not redrawn |
| N20 | done | No tab has an aria-label (checked) |
| N21 | done | Panels with a control take no tabindex; reader panels with none keep 0 |
| N22 | done | R3-C10–C14, C15-after, C17, C19, C23: each Done carries aria-controls its editor's id |
| N23 | done | Spec § 4 (C2's note) |
| N24 | done | Spec § 7 |
| Q17 | done | R3-E16, R3-E17; Spec § 3 and § 9 |
| Q18 | done | R3-C18, R3-C18b; the dropped refusal gone; Spec § 10 |
| Q19 | done | Spec § 3 (no board) |
| Q20 | done | R3-E02, R3-E02b; Steps T2 |
| T1 | done | Steps |
| T2 | done | Steps |
| T3 | done | Steps |
| T4 | done | Steps |
| T5 | done | Steps |
| T6 | done | Steps |
| T7 | done | Steps |
| T8 | done | Steps |
Not done: none.

######## BOARD R3-0-check-2.dc.html
Label: R3-0-check-2 · Check (2 of 2)
### Flow 17 · Round 3 · Check (2 of 2)
How this was checked. By rendering: every R3 board was opened in Chromium at its own width with the canvas's tokens.css, bundle.css, f17r3.css and fonts, and each number below is that render's (the brand mark, a /_blob/ address, was not served locally; it is a fixed 32px image). By reading the markup: the items' words and attributes, every for, aria-labelledby, aria-describedby and aria-controls resolving to an id on its board, no duplicate id, no id with a bracket or a space, no aria-describedby="", no aria-label on a tab, no bracket as a control's value, no inline style, no .tab-problem, .field-row-actions or .wf-*, one h1 a board, each board's head linking tokens.css, bundle.css, f17r3.css in that order.
#### THE TABS AT 390
THE TABS AT 390 (each tab: track / content scrollWidth, px)
| Board @ width | Tabs | Result |
| R3-C14-390 @390 | Details 72/72; Form 1 problem 72/72; Workflow 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-C14-390 @640 | Details 122/122; Form 1 problem 122/122; Workflow 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-C14-390 @641 | Details 78/78; Form 1 problem 137/137; Workflow 97/97; Credit 73/73; Publish 83/83 | all fit, 44px |
| R3-C20-390 @390 | Details 72/72; Form 1 problem 72/72; Workflow 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-C20-390 @640 | Details 122/122; Form 1 problem 122/122; Workflow 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-C20-390 @641 | Details 78/78; Form 1 problem 137/137; Workflow 97/97; Credit 73/73; Publish 83/83 | all fit, 44px |
| R3-D01-390 @390 | Details 72/72; Form 72/72; Workflow 1 problem 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-D01-390 @640 | Details 122/122; Form 122/122; Workflow 1 problem 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-D01-390 @641 | Details 78/78; Form 68/68; Workflow 1 problem 168/168; Credit 73/73; Publish 83/83 | all fit, 44px |
| R3-D04-390 @390 | Details 72/72; Form 72/72; Workflow 72/72; Credit 1 problem 72/72; Publish 72/72 | all fit, 44px |
| R3-D04-390 @640 | Details 122/122; Form 122/122; Workflow 122/122; Credit 1 problem 122/122; Publish 122/122 | all fit, 44px |
| R3-D04-390 @641 | Details 78/78; Form 68/68; Workflow 97/97; Credit 1 problem 144/144; Publish 83/83 | all fit, 44px |
| R3-C22-390 @390 | Details 1 problem 72/72; Form 72/72; Workflow 72/72; Credit 72/72; Publish 72/72 | all fit, 44px |
| R3-C22-390 @640 | Details 1 problem 122/122; Form 122/122; Workflow 122/122; Credit 122/122; Publish 122/122 | all fit, 44px |
| R3-C22-390 @641 | Details 1 problem 149/149; Form 68/68; Workflow 97/97; Credit 73/73; Publish 83/83 | all fit, 44px |
| scratch copy, a badge of 12 @390 | Form 12 problems: track 72, content 72, height 44 | fits |
| scratch copy, a badge of 12 @640 | Form 12 problems: track 122, content 122, height 44 | fits |
| scratch copy, a badge of 12 @641 | Form 12 problems: track 148, content 148, height 44 | fits |
At 390 and 640 the five tracks are equal and the badge shows its number alone; at 641 the tracks fit their words and the badge reads "1 problem". "Workflow", "Publish" and "Form" with its "1" fit without wrapping, and so does a badge of "12" (a scratch copy, not a board).
#### THE OUTLINE
THE OUTLINE (R3-C11 and R3-C19)
| Board @ width | Rail or column | A field row's four buttons | Checks |
| R3-C11 @1280 | 363px (rail) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @1116 | 320px (rail) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @1115 | 743px (stacked column) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @900 | 528px (stacked column) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @768 | 396px (stacked column) | 1 line, 230px | clipped 0; label under a button: no; sideways: no |
| R3-C11 @390 | 292px (stacked column) | 1 line, 258px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @1280 | 363px (rail) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @1116 | 320px (rail) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @1115 | 743px (stacked column) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @900 | 528px (stacked column) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @768 | 396px (stacked column) | 1 line, 223px | clipped 0; label under a button: no; sideways: no |
| R3-C19 @390 | 292px (stacked column) | 1 line, 258px | clipped 0; label under a button: no; sideways: no |
The rail is 363px at 1280 and 320px at 1116; the columns stack at 1115 and below. A field row's Edit, Up, Down and Delete sit on one line at every width (230px; 223px on R3-C19's first row, Done in place of Edit; 258px at 390, the buttons 44px and grown to the row); no button clipped, no label under a button, nothing sideways.
#### THE LIST
THE LIST: the table's min-content width (a clone at width: min-content) against its .table-container
| Board @ width | Min-content | Container | Stacked? | Spare or over (px) |
| R3-A13 @1000 | 275 | 692 | stacked | spare 417 |
| R3-A13 @1024 | 275 | 716 | stacked | spare 441 |
| R3-A13 @1025 | 682 | 717 | a table | spare 35 |
| R3-A13 @1050 | 682 | 742 | a table | spare 60 |
| R3-A13 @1100 | 682 | 792 | a table | spare 110 |
| R3-C08-list @1000 | 275 | 692 | stacked | spare 417 |
| R3-C08-list @1024 | 275 | 716 | stacked | spare 441 |
| R3-C08-list @1025 | 682 | 717 | a table | spare 35 |
| R3-C08-list @1050 | 682 | 742 | a table | spare 60 |
| R3-C08-list @1100 | 682 | 792 | a table | spare 110 |
From 1024px down the list is stacked (C3); from 1025px it is a table, 35px to spare at 1025.
#### THE HEADER
THE HEADER: where the actions sit
| Board @ width | h1 | Actions |
| R3-C03 @1280 | 1 line | Save draft beside h1 (x 925, y 120, 36px); Publish beside h1 (x 1033, y 120, 36px); Discard draft… beside h1 (x 1124, y 120, 36px) |
| R3-C03 @1100 | 1 line | Save draft beside h1 (x 745, y 120, 36px); Publish beside h1 (x 853, y 120, 36px); Discard draft… beside h1 (x 944, y 120, 36px) |
| R3-C03 @1024 | 1 line | Save draft beside h1 (x 669, y 120, 36px); Publish beside h1 (x 777, y 120, 36px); Discard draft… beside h1 (x 868, y 120, 36px) |
| R3-C03 @1023 | 1 line | Save draft under subtitle (x 282, y 239, 36px); Publish under subtitle (x 390, y 239, 36px); Discard draft… under subtitle (x 481, y 239, 36px) |
| R3-C03 @390 | 1 line | Save draft under subtitle (x 16, y 270, 44px); Publish under subtitle (x 133, y 270, 44px); Discard draft… under subtitle (x 233, y 270, 44px) |
| R3-E16 @1280 | 1 line | Save draft beside h1 (x 925, y 120, 36px); Publish beside h1 (x 1033, y 120, 36px); Discard draft… beside h1 (x 1124, y 120, 36px) |
| R3-E16 @1100 | 1 line | Save draft beside h1 (x 745, y 120, 36px); Publish beside h1 (x 853, y 120, 36px); Discard draft… beside h1 (x 944, y 120, 36px) |
| R3-E16 @1024 | 1 line | Save draft beside h1 (x 669, y 120, 36px); Publish beside h1 (x 777, y 120, 36px); Discard draft… beside h1 (x 868, y 120, 36px) |
| R3-E16 @1023 | 1 line | Save draft under subtitle (x 282, y 239, 36px); Publish under subtitle (x 390, y 239, 36px); Discard draft… under subtitle (x 481, y 239, 36px) |
| R3-E16 @390 | 1 line | Save draft under subtitle (x 16, y 270, 44px); Publish under subtitle (x 133, y 270, 44px); Discard draft… under subtitle (x 233, y 270, 44px) |
Beside the h1 from 1024px (flow 16's header rule); under the subtitle at 1023 and 390, 44px at 390. No title wraps past one line. R3-C03 and R3-E16 hold a draft, so Publish is on; its reason ("Save a draft to publish.", aria-describedby) sits beside it on the boards with no draft (R3-E02, R3-E02b, R3-C17).
#### THE 44 PX CHECK
952 controls measured on the 39 R3 boards at 390 (every .btn, .form-control, .form-select, .form-check row, tab, the breadcrumb's parent link and every row action, the shell's own menu and account row aside): none under 44px.
#### Sideways
Nothing scrolls sideways on any R3 board at 1280, 768 or 390: each of the 80 renders' scrollWidth equals its width.
#### Counts
Boards against the message
|  | Message | Drawn |
| Redrawn screens | 66 (32 at 1280, 32 at 390, 2 at 768) | 66: R3-A09-nocreate, A12; R3-B07; R3-C01, C02, C03, C04, C05, C06, C07, C08-list, C10, C11, C12, C13, C14, C15-before, C15-after, C17, C18, C19 (and 768), C20; R3-D01, D04, D05; R3-E01, E02, E04 (and 768), E05, E07, E11, E12 |
| New screens | 14 (7 at 1280, 7 at 390) | 14: R3-A13, C18b, C22, C23, E02b, E16, E17 |
| Check, Spec, Steps | about 7 | 7: R3-0-check, -check-2, R3-0-spec, -spec-2, -spec-3, -spec-4, R3-0-steps |
| Standing on Wombat 17 | 35 boards, 75 screens | 35: R2-A01, A02, A03, A04, A05, A06, A07, A08, A09-create, A10, A11; R2-B01, B02, B03, B04, B05, B06, B08, B09, B10, B11; R2-C08-new, C09, C16, C21 (390 and 768); R2-D02, D03; R2-E03, E06, E08, E09, E10, E13, E14 (390), E15 |

######## BOARD R3-A09-nocreate.dc.html
Label: R3-A09-nocreate · Illustration: the empty catalogue, a caller who may not create
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
The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change.
No activity types
No activity type is open to you yet.

######## BOARD R3-A09-nocreate-390.dc.html
Label: R3-A09-nocreate-390 · Illustration: the empty catalogue, a caller who may not create
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
The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change.
No activity types
No activity type is open to you yet.

######## BOARD R3-A12.dc.html
Label: R3-A12 · Illustration: KGK Teaching Session Log inactive in its writer's list
> Illustration: KGK Teaching Session Log, inactive
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
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | Saved ‹date› | Inactive | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
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

######## BOARD R3-A12-390.dc.html
Label: R3-A12-390 · Illustration: KGK Teaching Session Log inactive in its writer's list
> Illustration: KGK Teaching Session Log, inactive
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
| KGK Teaching Session Logkgk_teaching_log | Kgosi Kgari Teaching HospitalInstitution | Version 1 | Saved ‹date› | Inactive | [link] [aria-label="Edit KGK Teaching Session Log"] Edit |
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

######## BOARD R3-A13.dc.html
Label: R3-A13 · 1.24 Clear pressed: the 22 rows again
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

######## BOARD R3-A13-390.dc.html
Label: R3-A13-390 · 1.24 Clear pressed: the 22 rows again
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

######## BOARD R3-B07.dc.html
Label: R3-B07 · Illustration: Learner Feedback (Paediatrics) to Anton Kruger
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
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
##### Learner feedback
evidence
[button] [aria-label="View section Learner feedback"] View
-
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
-
Learner feedback campaign
Number · campaign_id · Required
[button] [aria-label="View field Learner feedback campaign"] View
-
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
-
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
-
Teaching contexts that responded
Number · teaching_context_count · Required
[button] [aria-label="View field Teaching contexts that responded"] View
##### Reviewer's summary
review
[button] [aria-label="View section Reviewer's summary"] View
-
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
The EPA the campaign was declared as evidence for: one whose College list names learner feedback.
Learner feedback campaign *
[aria-required="true"]
The campaign this record is one face of. Sibling records share it and differ only by EPA.
Feedback window closed *
[aria-required="true"]
The date the campaign closed to responses - when the evidence was complete.
Questionnaires returned *
[aria-required="true"]
Teaching contexts that responded *
[aria-required="true"]
How many distinct teaching contexts the returned questionnaires came from. EPA 15 asks for feedback across at least two; this is reported, not enforced.
##### Reviewer's summary
Reviewer's summary
The narrative already released to the trainee. No learner is named and no individual comment is reproduced here.

######## BOARD R3-B07-390.dc.html
Label: R3-B07-390 · Illustration: Learner Feedback (Paediatrics) to Anton Kruger
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
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
##### Learner feedback
evidence
[button] [aria-label="View section Learner feedback"] View
-
EPA
EPA · epa_id · Required
[button] [aria-label="View field EPA"] View
-
Learner feedback campaign
Number · campaign_id · Required
[button] [aria-label="View field Learner feedback campaign"] View
-
Feedback window closed
Date · observed_on · Required
[button] [aria-label="View field Feedback window closed"] View
-
Questionnaires returned
Number · respondent_count · Required
[button] [aria-label="View field Questionnaires returned"] View
-
Teaching contexts that responded
Number · teaching_context_count · Required
[button] [aria-label="View field Teaching contexts that responded"] View
##### Reviewer's summary
review
[button] [aria-label="View section Reviewer's summary"] View
-
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
The EPA the campaign was declared as evidence for: one whose College list names learner feedback.
Learner feedback campaign *
[aria-required="true"]
The campaign this record is one face of. Sibling records share it and differ only by EPA.
Feedback window closed *
[aria-required="true"]
The date the campaign closed to responses - when the evidence was complete.
Questionnaires returned *
[aria-required="true"]
Teaching contexts that responded *
[aria-required="true"]
How many distinct teaching contexts the returned questionnaires came from. EPA 15 asks for feedback across at least two; this is reported, not enforced.
##### Reviewer's summary
Reviewer's summary
The narrative already released to the trainee. No learner is named and no individual comment is reproduced here.

######## BOARD R3-C01.dc.html
Label: R3-C01 · 1.26 New activity type on Details: the starting draft
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C01-390.dc.html
Label: R3-C01-390 · 1.26 New activity type on Details: the starting draft
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C02.dc.html
Label: R3-C02 · 1.26 Details filled
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C02-390.dc.html
Label: R3-C02-390 · 1.26 Details filled
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C03.dc.html
Label: R3-C03 · 1.26 the first Save draft: /admin/activity-types/23
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C03-390.dc.html
Label: R3-C03-390 · 1.26 the first Save draft: /admin/activity-types/23
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C04.dc.html
Label: R3-C04 · 6.14a New as Anton Kruger: Speciality
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
[role="radiogroup"] [aria-required="true"]
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C04-390.dc.html
Label: R3-C04-390 · 6.14a New as Anton Kruger: Speciality
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
[role="radiogroup"] [aria-required="true"]
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C05.dc.html
Label: R3-C05 · 6.14a Sub-speciality chosen
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
[role="radiogroup"] [aria-required="true"]
Scope *required
(radio)
Speciality
(radio) [checked="checked"]
Sub-speciality
Sub-speciality *required
[aria-required="true"]
[selected="selected"] Paediatrics / Neonatology
Paediatrics / Paediatrics
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C05-390.dc.html
Label: R3-C05-390 · 6.14a Sub-speciality chosen
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
[role="radiogroup"] [aria-required="true"]
Scope *required
(radio)
Speciality
(radio) [checked="checked"]
Sub-speciality
Sub-speciality *required
[aria-required="true"]
[selected="selected"] Paediatrics / Neonatology
Paediatrics / Paediatrics
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C06.dc.html
Label: R3-C06 · 6.14a saved: /admin/activity-types/24
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="cpsa_case_presentation_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="CPSA Case Presentation Log"] [aria-required="true"]
[role="radiogroup"] [aria-required="true"]
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C06-390.dc.html
Label: R3-C06-390 · 6.14a saved: /admin/activity-types/24
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="cpsa_case_presentation_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="CPSA Case Presentation Log"] [aria-required="true"]
[role="radiogroup"] [aria-required="true"]
Scope *required
(radio) [checked="checked"]
Speciality
(radio)
Sub-speciality
**Speciality
Paediatrics
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C07.dc.html
Label: R3-C07 · Illustration: devadmin's New, Global
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
[role="radiogroup"] [aria-required="true"]
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
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C07-390.dc.html
Label: R3-C07-390 · Illustration: devadmin's New, Global
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[aria-required="true"]
Fixed once published.
Name *required
[aria-required="true"]
[role="radiogroup"] [aria-required="true"]
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
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C08-list.dc.html
Label: R3-C08-list · Illustration: the list to a College admin with no scope: no New activity type
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
The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change.
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

######## BOARD R3-C08-list-390.dc.html
Label: R3-C08-list-390 · Illustration: the list to a College admin with no scope: no New activity type
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
The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change.
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

######## BOARD R3-C10.dc.html
Label: R3-C10 · 1.27 the section editor in place
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Done with section Teaching session"] Done
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
[role="group"]
###### Section Teaching session
* marks a field you must fill in.
Section key *required
[value="session"] [aria-required="true"]
Title *required
[value="Teaching session"] [aria-required="true"]
-
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

######## BOARD R3-C10-390.dc.html
Label: R3-C10-390 · 1.27 the section editor in place
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Done with section Teaching session"] Done
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
[role="group"]
###### Section Teaching session
* marks a field you must fill in.
Section key *required
[value="session"] [aria-required="true"]
Title *required
[value="Teaching session"] [aria-required="true"]
-
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

######## BOARD R3-C11.dc.html
Label: R3-C11 · 1.27 Title's field editor in place, now Topic
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Done with field Topic"] Done
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Topic"] Delete
- [role="group"]
###### Field Topic
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Label *required
[value="Topic"] [aria-required="true"]
Type *required
[aria-required="true"]
[selected="selected"] Text
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

######## BOARD R3-C11-390.dc.html
Label: R3-C11-390 · 1.27 Title's field editor in place, now Topic
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Done with field Topic"] Done
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Topic"] Delete
- [role="group"]
###### Field Topic
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Label *required
[value="Topic"] [aria-required="true"]
Type *required
[aria-required="true"]
[selected="selected"] Text
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

######## BOARD R3-C12.dc.html
Label: R3-C12 · 1.27 Audience's editor, its key topic by mistake
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
- [role="group"]
###### Field Audience
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Label *required
[value="Audience"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"] Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C12-390.dc.html
Label: R3-C12-390 · 1.27 Audience's editor, its key topic by mistake
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
- [role="group"]
###### Field Audience
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Label *required
[value="Audience"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"] Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C13.dc.html
Label: R3-C13 · 1.27 Supervising consultant's editor: Names a
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Done with field Supervising consultant"] Done
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
- [role="group"]
###### Field Supervising consultant
* marks a field you must fill in.
Field key *required
[value="supervisor_user_id"] [aria-required="true"]
Label *required
[value="Supervising consultant"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
Choice
Multi-choice
Scale
[selected="selected"] User
EPA
Likert
Procedure reference
File
Signature
Help text
Names a
[selected="selected"] Assessor (default)
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

######## BOARD R3-C13-390.dc.html
Label: R3-C13-390 · 1.27 Supervising consultant's editor: Names a
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Done with field Supervising consultant"] Done
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
- [role="group"]
###### Field Supervising consultant
* marks a field you must fill in.
Field key *required
[value="supervisor_user_id"] [aria-required="true"]
Label *required
[value="Supervising consultant"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
Choice
Multi-choice
Scale
[selected="selected"] User
EPA
Likert
Procedure reference
File
Signature
Help text
Names a
[selected="selected"] Assessor (default)
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

######## BOARD R3-C14.dc.html
Label: R3-C14 · 1.27 the duplicate key refused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
- [role="group"]
###### Field Audience
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"] Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C14-390.dc.html
Label: R3-C14-390 · 1.27 the duplicate key refused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
- [role="group"]
###### Field Audience
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"] Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C15-after.dc.html
Label: R3-C15-after · Illustration: the line pressed: Form, Audience's editor, its Key focused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
- [role="group"]
###### Field Audience
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"] Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C15-after-390.dc.html
Label: R3-C15-after-390 · Illustration: the line pressed: Form, Audience's editor, its Key focused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
Key 'topic' is also the key of Audience. Give each field its own key.
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · topic · Required
[button] [aria-label="Done with field Audience"] Done
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
- [role="group"]
###### Field Audience
* marks a field you must fill in.
Field key *required
[value="topic"] [aria-required="true"]
Key 'topic' is also the key of Topic. Give each field its own key.
Label *required
[value="Audience"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"] Interns Medical students Nursing staff Registrars
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C15-before.dc.html
Label: R3-C15-before · Illustration: the same refusal while Details is selected
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C15-before-390.dc.html
Label: R3-C15-before-390 · Illustration: the same refusal while Details is selected
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C17.dc.html
Label: R3-C17 · Illustration: a Scale field's editor, the scale by name
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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
[selected="selected"] Date observed (observed_on)
Entrustment rating field
None — this form asserts no entrustment level
[selected="selected"] Supervision required for this encounter (overall_level)
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Request
request
[button] [aria-label="Edit section Request"] Edit
[button] [aria-label="Move section Request up"] [disabled="disabled"] Up
[button] [aria-label="Move section Request down"] Down
[button] [aria-label="Delete section Request"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] [disabled="disabled"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="Edit field Assessor"] Edit
[button] [aria-label="Move field Assessor up"] Up
[button] [aria-label="Move field Assessor down"] Down
[button] [aria-label="Delete field Assessor"] Delete
-
Date observed
Date · observed_on · Required
[button] [aria-label="Edit field Date observed"] Edit
[button] [aria-label="Move field Date observed up"] Up
[button] [aria-label="Move field Date observed down"] Down
[button] [aria-label="Delete field Date observed"] Delete
-
Clinical setting
Choice · setting · Required
[button] [aria-label="Edit field Clinical setting"] Edit
[button] [aria-label="Move field Clinical setting up"] Up
[button] [aria-label="Move field Clinical setting down"] Down
[button] [aria-label="Delete field Clinical setting"] Delete
-
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="Edit field Presenting problem"] Edit
[button] [aria-label="Move field Presenting problem up"] Up
[button] [aria-label="Move field Presenting problem down"] Down
[button] [aria-label="Delete field Presenting problem"] Delete
-
Case complexity
Choice · complexity · Required
[button] [aria-label="Edit field Case complexity"] Edit
[button] [aria-label="Move field Case complexity up"] Up
[button] [aria-label="Move field Case complexity down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Case complexity"] Delete
[button] [aria-label="Add field to section Request"] Add field
##### Entrustment
assessment
[button] [aria-label="Edit section Entrustment"] Edit
[button] [aria-label="Move section Entrustment up"] Up
[button] [aria-label="Move section Entrustment down"] Down
[button] [aria-label="Delete section Entrustment"] Delete
-
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="Done with field Supervision required for this encounter"] Done
[button] [aria-label="Move field Supervision required for this encounter up"] [disabled="disabled"] Up
[button] [aria-label="Move field Supervision required for this encounter down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervision required for this encounter"] Delete
- [role="group"]
###### Field Supervision required for this encounter
* marks a field you must fill in.
Field key *required
[value="overall_level"] [aria-required="true"]
Label *required
[value="Supervision required for this encounter"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
Choice
Multi-choice
[selected="selected"] Scale
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
CNSA Neurology Entrustment Scale
[selected="selected"] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Levels of the chosen scale render as the rating dropdown at runtime.
(checkbox) [checked="checked"]
Required
[button] [aria-label="Add field to section Entrustment"] Add field
##### Feedback
feedback
[button] [aria-label="Edit section Feedback"] Edit
[button] [aria-label="Move section Feedback up"] Up
[button] [aria-label="Move section Feedback down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Feedback"] Delete
-
What was done well
Long text · strengths · Required
[button] [aria-label="Edit field What was done well"] Edit
[button] [aria-label="Move field What was done well up"] [disabled="disabled"] Up
[button] [aria-label="Move field What was done well down"] Down
[button] [aria-label="Delete field What was done well"] Delete
-
Areas for development
Long text · improvements · Required
[button] [aria-label="Edit field Areas for development"] Edit
[button] [aria-label="Move field Areas for development up"] Up
[button] [aria-label="Move field Areas for development down"] Down
[button] [aria-label="Delete field Areas for development"] Delete
-
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

######## BOARD R3-C17-390.dc.html
Label: R3-C17-390 · Illustration: a Scale field's editor, the scale by name
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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
[selected="selected"] Date observed (observed_on)
Entrustment rating field
None — this form asserts no entrustment level
[selected="selected"] Supervision required for this encounter (overall_level)
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Request
request
[button] [aria-label="Edit section Request"] Edit
[button] [aria-label="Move section Request up"] [disabled="disabled"] Up
[button] [aria-label="Move section Request down"] Down
[button] [aria-label="Delete section Request"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] [disabled="disabled"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Assessor
User · Names an Assessor · assessor_user_id · Required
[button] [aria-label="Edit field Assessor"] Edit
[button] [aria-label="Move field Assessor up"] Up
[button] [aria-label="Move field Assessor down"] Down
[button] [aria-label="Delete field Assessor"] Delete
-
Date observed
Date · observed_on · Required
[button] [aria-label="Edit field Date observed"] Edit
[button] [aria-label="Move field Date observed up"] Up
[button] [aria-label="Move field Date observed down"] Down
[button] [aria-label="Delete field Date observed"] Delete
-
Clinical setting
Choice · setting · Required
[button] [aria-label="Edit field Clinical setting"] Edit
[button] [aria-label="Move field Clinical setting up"] Up
[button] [aria-label="Move field Clinical setting down"] Down
[button] [aria-label="Delete field Clinical setting"] Delete
-
Presenting problem
Text · presenting_problem · Required
[button] [aria-label="Edit field Presenting problem"] Edit
[button] [aria-label="Move field Presenting problem up"] Up
[button] [aria-label="Move field Presenting problem down"] Down
[button] [aria-label="Delete field Presenting problem"] Delete
-
Case complexity
Choice · complexity · Required
[button] [aria-label="Edit field Case complexity"] Edit
[button] [aria-label="Move field Case complexity up"] Up
[button] [aria-label="Move field Case complexity down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Case complexity"] Delete
[button] [aria-label="Add field to section Request"] Add field
##### Entrustment
assessment
[button] [aria-label="Edit section Entrustment"] Edit
[button] [aria-label="Move section Entrustment up"] Up
[button] [aria-label="Move section Entrustment down"] Down
[button] [aria-label="Delete section Entrustment"] Delete
-
Supervision required for this encounter
Scale · overall_level · Required
[button] [aria-label="Done with field Supervision required for this encounter"] Done
[button] [aria-label="Move field Supervision required for this encounter up"] [disabled="disabled"] Up
[button] [aria-label="Move field Supervision required for this encounter down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervision required for this encounter"] Delete
- [role="group"]
###### Field Supervision required for this encounter
* marks a field you must fill in.
Field key *required
[value="overall_level"] [aria-required="true"]
Label *required
[value="Supervision required for this encounter"] [aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
Choice
Multi-choice
[selected="selected"] Scale
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
CNSA Neurology Entrustment Scale
[selected="selected"] CPSA Paediatric Entrustment Scale v11.1
O-R Scale
Levels of the chosen scale render as the rating dropdown at runtime.
(checkbox) [checked="checked"]
Required
[button] [aria-label="Add field to section Entrustment"] Add field
##### Feedback
feedback
[button] [aria-label="Edit section Feedback"] Edit
[button] [aria-label="Move section Feedback up"] Up
[button] [aria-label="Move section Feedback down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Feedback"] Delete
-
What was done well
Long text · strengths · Required
[button] [aria-label="Edit field What was done well"] Edit
[button] [aria-label="Move field What was done well up"] [disabled="disabled"] Up
[button] [aria-label="Move field What was done well down"] Down
[button] [aria-label="Delete field What was done well"] Delete
-
Areas for development
Long text · improvements · Required
[button] [aria-label="Edit field Areas for development"] Edit
[button] [aria-label="Move field Areas for development up"] Up
[button] [aria-label="Move field Areas for development down"] Down
[button] [aria-label="Delete field Areas for development"] Delete
-
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

######## BOARD R3-C18.dc.html
Label: R3-C18 · Illustration: Date delivered deleted and the draft saved: the setting dropped
> Illustration: Date delivered deleted, Save draft pressed
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
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C18-390.dc.html
Label: R3-C18-390 · Illustration: Date delivered deleted and the draft saved: the setting dropped
> Illustration: Date delivered deleted, Save draft pressed
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
Version 1, 2026-11-05, Nolwazi Mbatha
**Draft
Saved ‹date› by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-C18b.dc.html
Label: R3-C18b · Illustration: the Publish tab after that save
> Illustration: Date delivered deleted and saved: the Publish tab
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Date delivered' will be removed.
- This form will stop recording which field carries the encounter date.
Activities already filed stay on version 1, with Date delivered as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2

######## BOARD R3-C18b-390.dc.html
Label: R3-C18b-390 · Illustration: the Publish tab after that save
> Illustration: Date delivered deleted and saved: the Publish tab
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
[role="tabpanel"]
#### Publish
##### What publishing this draft changes
- Field 'Date delivered' will be removed.
- This form will stop recording which field carries the encounter date.
Activities already filed stay on version 1, with Date delivered as they filed it. Only activities filed after the publish use version 2.
##### Versions
[role="table"]
Published versions
[role="rowgroup"]
| Version | Published | By |
[role="rowgroup"]
| 1 | 2026-11-05 | Nolwazi Mbatha |
Activities filed under a version stay on it.
[button] Publish version 2

######## BOARD R3-C19.dc.html
Label: R3-C19 · Illustration: the heavy type, after Hide fields on sections 2 to 6
> Illustration: 6 sections, 40 fields, after Hide fields on sections 2 to 6
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### ‹section 1›
‹section key 1›
[button] [aria-label="Edit section ‹section 1›"] Edit
[button] [aria-label="Move section ‹section 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move section ‹section 1› down"] Down
[button] [aria-label="Delete section ‹section 1›"] Delete
[button] [aria-label="Hide fields of section ‹section 1›"] Hide fields
-
‹field 1›
Text · ‹key 1›
[button] [aria-label="Edit field ‹field 1›"] Edit
[button] [aria-label="Move field ‹field 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move field ‹field 1› down"] Down
[button] [aria-label="Delete field ‹field 1›"] Delete
-
Supervision the reviewer judged this trainee to need
Scale · ‹key 2›
[button] [aria-label="Edit field Supervision the reviewer judged this trainee to need"] Edit
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need up"] Up
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need down"] Down
[button] [aria-label="Delete field Supervision the reviewer judged this trainee to need"] Delete
-
‹a 12-option choice›
Choice · ‹key 3› · Required
[button] [aria-label="Done with field ‹a 12-option choice›"] Done
[button] [aria-label="Move field ‹a 12-option choice› up"] Up
[button] [aria-label="Move field ‹a 12-option choice› down"] Down
[button] [aria-label="Delete field ‹a 12-option choice›"] Delete
- [role="group"]
###### Field ‹a 12-option choice›
* marks a field you must fill in.
Field key *required
[aria-required="true"]
Label *required
[aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"]
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
‹field 4›
Text · ‹key 4›
[button] [aria-label="Edit field ‹field 4›"] Edit
[button] [aria-label="Move field ‹field 4› up"] Up
[button] [aria-label="Move field ‹field 4› down"] Down
[button] [aria-label="Delete field ‹field 4›"] Delete
-
‹field 5›
Text · ‹key 5›
[button] [aria-label="Edit field ‹field 5›"] Edit
[button] [aria-label="Move field ‹field 5› up"] Up
[button] [aria-label="Move field ‹field 5› down"] Down
[button] [aria-label="Delete field ‹field 5›"] Delete
-
‹field 6›
Text · ‹key 6›
[button] [aria-label="Edit field ‹field 6›"] Edit
[button] [aria-label="Move field ‹field 6› up"] Up
[button] [aria-label="Move field ‹field 6› down"] Down
[button] [aria-label="Delete field ‹field 6›"] Delete
-
‹field 7›
Text · ‹key 7›
[button] [aria-label="Edit field ‹field 7›"] Edit
[button] [aria-label="Move field ‹field 7› up"] Up
[button] [aria-label="Move field ‹field 7› down"] [disabled="disabled"] Down
[button] [aria-label="Delete field ‹field 7›"] Delete
[button] [aria-label="Add field to section ‹section 1›"] Add field
##### ‹section 2›
‹section key 2›
[button] [aria-label="Edit section ‹section 2›"] Edit
[button] [aria-label="Move section ‹section 2› up"] Up
[button] [aria-label="Move section ‹section 2› down"] Down
[button] [aria-label="Delete section ‹section 2›"] Delete
[button] [aria-label="Show fields of section ‹section 2›"] Show fields
##### ‹section 3›
‹section key 3›
[button] [aria-label="Edit section ‹section 3›"] Edit
[button] [aria-label="Move section ‹section 3› up"] Up
[button] [aria-label="Move section ‹section 3› down"] Down
[button] [aria-label="Delete section ‹section 3›"] Delete
[button] [aria-label="Show fields of section ‹section 3›"] Show fields
##### ‹section 4›
‹section key 4›
[button] [aria-label="Edit section ‹section 4›"] Edit
[button] [aria-label="Move section ‹section 4› up"] Up
[button] [aria-label="Move section ‹section 4› down"] Down
[button] [aria-label="Delete section ‹section 4›"] Delete
[button] [aria-label="Show fields of section ‹section 4›"] Show fields
##### ‹section 5›
‹section key 5›
[button] [aria-label="Edit section ‹section 5›"] Edit
[button] [aria-label="Move section ‹section 5› up"] Up
[button] [aria-label="Move section ‹section 5› down"] Down
[button] [aria-label="Delete section ‹section 5›"] Delete
[button] [aria-label="Show fields of section ‹section 5›"] Show fields
##### ‹section 6›
‹section key 6›
[button] [aria-label="Edit section ‹section 6›"] Edit
[button] [aria-label="Move section ‹section 6› up"] Up
[button] [aria-label="Move section ‹section 6› down"] [disabled="disabled"] Down
[button] [aria-label="Delete section ‹section 6›"] Delete
[button] [aria-label="Show fields of section ‹section 6›"] Show fields
#### Live preview
* marks a field you must fill in.
##### ‹section 1›
‹field 1›
Supervision the reviewer judged this trainee to need
Select…
‹a 12-option choice› *
[aria-required="true"]
Select…
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

######## BOARD R3-C19-768.dc.html
Label: R3-C19-768 · Illustration: the heavy type, after Hide fields on sections 2 to 6
> Illustration: 6 sections, 40 fields, after Hide fields on sections 2 to 6
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### ‹section 1›
‹section key 1›
[button] [aria-label="Edit section ‹section 1›"] Edit
[button] [aria-label="Move section ‹section 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move section ‹section 1› down"] Down
[button] [aria-label="Delete section ‹section 1›"] Delete
[button] [aria-label="Hide fields of section ‹section 1›"] Hide fields
-
‹field 1›
Text · ‹key 1›
[button] [aria-label="Edit field ‹field 1›"] Edit
[button] [aria-label="Move field ‹field 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move field ‹field 1› down"] Down
[button] [aria-label="Delete field ‹field 1›"] Delete
-
Supervision the reviewer judged this trainee to need
Scale · ‹key 2›
[button] [aria-label="Edit field Supervision the reviewer judged this trainee to need"] Edit
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need up"] Up
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need down"] Down
[button] [aria-label="Delete field Supervision the reviewer judged this trainee to need"] Delete
-
‹a 12-option choice›
Choice · ‹key 3› · Required
[button] [aria-label="Done with field ‹a 12-option choice›"] Done
[button] [aria-label="Move field ‹a 12-option choice› up"] Up
[button] [aria-label="Move field ‹a 12-option choice› down"] Down
[button] [aria-label="Delete field ‹a 12-option choice›"] Delete
- [role="group"]
###### Field ‹a 12-option choice›
* marks a field you must fill in.
Field key *required
[aria-required="true"]
Label *required
[aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"]
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
‹field 4›
Text · ‹key 4›
[button] [aria-label="Edit field ‹field 4›"] Edit
[button] [aria-label="Move field ‹field 4› up"] Up
[button] [aria-label="Move field ‹field 4› down"] Down
[button] [aria-label="Delete field ‹field 4›"] Delete
-
‹field 5›
Text · ‹key 5›
[button] [aria-label="Edit field ‹field 5›"] Edit
[button] [aria-label="Move field ‹field 5› up"] Up
[button] [aria-label="Move field ‹field 5› down"] Down
[button] [aria-label="Delete field ‹field 5›"] Delete
-
‹field 6›
Text · ‹key 6›
[button] [aria-label="Edit field ‹field 6›"] Edit
[button] [aria-label="Move field ‹field 6› up"] Up
[button] [aria-label="Move field ‹field 6› down"] Down
[button] [aria-label="Delete field ‹field 6›"] Delete
-
‹field 7›
Text · ‹key 7›
[button] [aria-label="Edit field ‹field 7›"] Edit
[button] [aria-label="Move field ‹field 7› up"] Up
[button] [aria-label="Move field ‹field 7› down"] [disabled="disabled"] Down
[button] [aria-label="Delete field ‹field 7›"] Delete
[button] [aria-label="Add field to section ‹section 1›"] Add field
##### ‹section 2›
‹section key 2›
[button] [aria-label="Edit section ‹section 2›"] Edit
[button] [aria-label="Move section ‹section 2› up"] Up
[button] [aria-label="Move section ‹section 2› down"] Down
[button] [aria-label="Delete section ‹section 2›"] Delete
[button] [aria-label="Show fields of section ‹section 2›"] Show fields
##### ‹section 3›
‹section key 3›
[button] [aria-label="Edit section ‹section 3›"] Edit
[button] [aria-label="Move section ‹section 3› up"] Up
[button] [aria-label="Move section ‹section 3› down"] Down
[button] [aria-label="Delete section ‹section 3›"] Delete
[button] [aria-label="Show fields of section ‹section 3›"] Show fields
##### ‹section 4›
‹section key 4›
[button] [aria-label="Edit section ‹section 4›"] Edit
[button] [aria-label="Move section ‹section 4› up"] Up
[button] [aria-label="Move section ‹section 4› down"] Down
[button] [aria-label="Delete section ‹section 4›"] Delete
[button] [aria-label="Show fields of section ‹section 4›"] Show fields
##### ‹section 5›
‹section key 5›
[button] [aria-label="Edit section ‹section 5›"] Edit
[button] [aria-label="Move section ‹section 5› up"] Up
[button] [aria-label="Move section ‹section 5› down"] Down
[button] [aria-label="Delete section ‹section 5›"] Delete
[button] [aria-label="Show fields of section ‹section 5›"] Show fields
##### ‹section 6›
‹section key 6›
[button] [aria-label="Edit section ‹section 6›"] Edit
[button] [aria-label="Move section ‹section 6› up"] Up
[button] [aria-label="Move section ‹section 6› down"] [disabled="disabled"] Down
[button] [aria-label="Delete section ‹section 6›"] Delete
[button] [aria-label="Show fields of section ‹section 6›"] Show fields
#### Live preview
* marks a field you must fill in.
##### ‹section 1›
‹field 1›
Supervision the reviewer judged this trainee to need
Select…
‹a 12-option choice› *
[aria-required="true"]
Select…
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

######## BOARD R3-C19-390.dc.html
Label: R3-C19-390 · Illustration: the heavy type, after Hide fields on sections 2 to 6
> Illustration: 6 sections, 40 fields, after Hide fields on sections 2 to 6
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### ‹section 1›
‹section key 1›
[button] [aria-label="Edit section ‹section 1›"] Edit
[button] [aria-label="Move section ‹section 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move section ‹section 1› down"] Down
[button] [aria-label="Delete section ‹section 1›"] Delete
[button] [aria-label="Hide fields of section ‹section 1›"] Hide fields
-
‹field 1›
Text · ‹key 1›
[button] [aria-label="Edit field ‹field 1›"] Edit
[button] [aria-label="Move field ‹field 1› up"] [disabled="disabled"] Up
[button] [aria-label="Move field ‹field 1› down"] Down
[button] [aria-label="Delete field ‹field 1›"] Delete
-
Supervision the reviewer judged this trainee to need
Scale · ‹key 2›
[button] [aria-label="Edit field Supervision the reviewer judged this trainee to need"] Edit
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need up"] Up
[button] [aria-label="Move field Supervision the reviewer judged this trainee to need down"] Down
[button] [aria-label="Delete field Supervision the reviewer judged this trainee to need"] Delete
-
‹a 12-option choice›
Choice · ‹key 3› · Required
[button] [aria-label="Done with field ‹a 12-option choice›"] Done
[button] [aria-label="Move field ‹a 12-option choice› up"] Up
[button] [aria-label="Move field ‹a 12-option choice› down"] Down
[button] [aria-label="Delete field ‹a 12-option choice›"] Delete
- [role="group"]
###### Field ‹a 12-option choice›
* marks a field you must fill in.
Field key *required
[aria-required="true"]
Label *required
[aria-required="true"]
Type *required
[aria-required="true"]
Text
Long text
Number
Date
[selected="selected"] Choice
Multi-choice
Scale
User
EPA
Likert
Procedure reference
File
Signature
Help text
Options *required
[aria-required="true"]
One option per line; a comma is part of the option. Write value | Label to show a label: the form shows the label, and the activity stores the value.
Catalogue key
(checkbox) [checked="checked"]
Required
-
‹field 4›
Text · ‹key 4›
[button] [aria-label="Edit field ‹field 4›"] Edit
[button] [aria-label="Move field ‹field 4› up"] Up
[button] [aria-label="Move field ‹field 4› down"] Down
[button] [aria-label="Delete field ‹field 4›"] Delete
-
‹field 5›
Text · ‹key 5›
[button] [aria-label="Edit field ‹field 5›"] Edit
[button] [aria-label="Move field ‹field 5› up"] Up
[button] [aria-label="Move field ‹field 5› down"] Down
[button] [aria-label="Delete field ‹field 5›"] Delete
-
‹field 6›
Text · ‹key 6›
[button] [aria-label="Edit field ‹field 6›"] Edit
[button] [aria-label="Move field ‹field 6› up"] Up
[button] [aria-label="Move field ‹field 6› down"] Down
[button] [aria-label="Delete field ‹field 6›"] Delete
-
‹field 7›
Text · ‹key 7›
[button] [aria-label="Edit field ‹field 7›"] Edit
[button] [aria-label="Move field ‹field 7› up"] Up
[button] [aria-label="Move field ‹field 7› down"] [disabled="disabled"] Down
[button] [aria-label="Delete field ‹field 7›"] Delete
[button] [aria-label="Add field to section ‹section 1›"] Add field
##### ‹section 2›
‹section key 2›
[button] [aria-label="Edit section ‹section 2›"] Edit
[button] [aria-label="Move section ‹section 2› up"] Up
[button] [aria-label="Move section ‹section 2› down"] Down
[button] [aria-label="Delete section ‹section 2›"] Delete
[button] [aria-label="Show fields of section ‹section 2›"] Show fields
##### ‹section 3›
‹section key 3›
[button] [aria-label="Edit section ‹section 3›"] Edit
[button] [aria-label="Move section ‹section 3› up"] Up
[button] [aria-label="Move section ‹section 3› down"] Down
[button] [aria-label="Delete section ‹section 3›"] Delete
[button] [aria-label="Show fields of section ‹section 3›"] Show fields
##### ‹section 4›
‹section key 4›
[button] [aria-label="Edit section ‹section 4›"] Edit
[button] [aria-label="Move section ‹section 4› up"] Up
[button] [aria-label="Move section ‹section 4› down"] Down
[button] [aria-label="Delete section ‹section 4›"] Delete
[button] [aria-label="Show fields of section ‹section 4›"] Show fields
##### ‹section 5›
‹section key 5›
[button] [aria-label="Edit section ‹section 5›"] Edit
[button] [aria-label="Move section ‹section 5› up"] Up
[button] [aria-label="Move section ‹section 5› down"] Down
[button] [aria-label="Delete section ‹section 5›"] Delete
[button] [aria-label="Show fields of section ‹section 5›"] Show fields
##### ‹section 6›
‹section key 6›
[button] [aria-label="Edit section ‹section 6›"] Edit
[button] [aria-label="Move section ‹section 6› up"] Up
[button] [aria-label="Move section ‹section 6› down"] [disabled="disabled"] Down
[button] [aria-label="Delete section ‹section 6›"] Delete
[button] [aria-label="Show fields of section ‹section 6›"] Show fields
#### Live preview
* marks a field you must fill in.
##### ‹section 1›
‹field 1›
Supervision the reviewer judged this trainee to need
Select…
‹a 12-option choice› *
[aria-required="true"]
Select…
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

######## BOARD R3-C20.dc.html
Label: R3-C20 · Illustration: a section with no fields refused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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
[selected="selected"] Date delivered (delivered_on)
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
##### Section 2
section_2
This section has no fields. Add a field, or delete the section.
[button] [aria-label="Edit section Section 2"] Edit
[button] [aria-label="Move section Section 2 up"] Up
[button] [aria-label="Move section Section 2 down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Section 2"] Delete
-
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

######## BOARD R3-C20-390.dc.html
Label: R3-C20-390 · Illustration: a section with no fields refused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form 1 problem
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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
[selected="selected"] Date delivered (delivered_on)
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Audience
Choice · audience · Required
[button] [aria-label="Edit field Audience"] Edit
[button] [aria-label="Move field Audience up"] Up
[button] [aria-label="Move field Audience down"] Down
[button] [aria-label="Delete field Audience"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
Supervising consultant
User · Names an Assessor · supervisor_user_id
[button] [aria-label="Edit field Supervising consultant"] Edit
[button] [aria-label="Move field Supervising consultant up"] Up
[button] [aria-label="Move field Supervising consultant down"] [disabled="disabled"] Down
[button] [aria-label="Delete field Supervising consultant"] Delete
[button] [aria-label="Add field to section Teaching session"] Add field
##### Section 2
section_2
This section has no fields. Add a field, or delete the section.
[button] [aria-label="Edit section Section 2"] Edit
[button] [aria-label="Move section Section 2 up"] Up
[button] [aria-label="Move section Section 2 down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Section 2"] Delete
-
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

######## BOARD R3-C22.dc.html
Label: R3-C22 · Illustration: the key taken, refused on Details
> Illustration: a second type keyed kgk_teaching_log, after 1.30
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
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Details: An activity type with key 'kgk_teaching_log' already exists.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [role="tab"] [aria-selected="true"] Details 1 problem
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
An activity type with key 'kgk_teaching_log' already exists.
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C22-390.dc.html
Label: R3-C22-390 · Illustration: the key taken, refused on Details
> Illustration: a second type keyed kgk_teaching_log, after 1.30
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
[role="alert"] Nothing was saved. Everything you typed is kept.
-
[link] Details: An activity type with key 'kgk_teaching_log' already exists.
[aria-label="Versions"]
**Published
Not published
**Draft
None
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [role="tab"] [aria-selected="true"] Details 1 problem
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
Key *required
[value="kgk_teaching_log"] [aria-required="true"]
An activity type with key 'kgk_teaching_log' already exists.
Fixed once published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-C23.dc.html
Label: R3-C23 · 1.27 Add field: the new row's editor open under it
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
New field
Text · field_1
[button] [aria-label="Done with field New field"] Done
[button] [aria-label="Move field New field up"] Up
[button] [aria-label="Move field New field down"] [disabled="disabled"] Down
[button] [aria-label="Delete field New field"] Delete
- [role="group"]
###### Field New field
* marks a field you must fill in.
Field key *required
[value="field_1"] [aria-required="true"]
Label *required
[value="New field"] [aria-required="true"]
Type *required
[aria-required="true"]
[selected="selected"] Text
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
(checkbox)
Required
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
New field

######## BOARD R3-C23-390.dc.html
Label: R3-C23-390 · 1.27 Add field: the new row's editor open under it
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
Show
(radio) [checked="checked"]
Edit the form
(radio)
Preview
#### Form settings
Which field carries the encounter date, which carries the entrustment rating, and which names the EPA.
Encounter date field
[selected="selected"] None — this form has no single encounter date
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
[selected="selected"] None — this form is about no single EPA
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
New field
Text · field_1
[button] [aria-label="Done with field New field"] Done
[button] [aria-label="Move field New field up"] Up
[button] [aria-label="Move field New field down"] [disabled="disabled"] Down
[button] [aria-label="Delete field New field"] Delete
- [role="group"]
###### Field New field
* marks a field you must fill in.
Field key *required
[value="field_1"] [aria-required="true"]
Label *required
[value="New field"] [aria-required="true"]
Type *required
[aria-required="true"]
[selected="selected"] Text
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
(checkbox)
Required
[button] [aria-label="Add field to section Teaching session"] Add field
#### Live preview
* marks a field you must fill in.
##### Teaching session
Topic *
[aria-required="true"]
New field

######## BOARD R3-D01.dc.html
Label: R3-D01 · 1.28 the field: rule refused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="true"] Workflow 1 problem
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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

######## BOARD R3-D01-390.dc.html
Label: R3-D01-390 · 1.28 the field: rule refused
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="true"] Workflow 1 problem
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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

######## BOARD R3-D04.dc.html
Label: R3-D04 · Illustration: a credit rule refused
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
[link] Credit: Credit rule 1 reads its EPA from field 'epa', but the form's EPA field is 'epa_id'. The EPA an activity is filed against and the EPA it credits must be the same field.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="true"] Credit 1 problem
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa" }, "amount": 1 } ] }
Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'.
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R3-D04-390.dc.html
Label: R3-D04-390 · Illustration: a credit rule refused
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
[link] Credit: Credit rule 1 reads its EPA from field 'epa', but the form's EPA field is 'epa_id'. The EPA an activity is filed against and the EPA it credits must be the same field.
[aria-label="Versions"]
**Published
Not published
**Draft
Saved 2026-11-05 by Nolwazi Mbatha
[aria-label="Parts of this activity type"] [role="tablist"]
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="true"] Credit 1 problem
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa" }, "amount": 1 } ] }
Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'.
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R3-D05.dc.html
Label: R3-D05 · Illustration: the starting draft's Workflow tab on /new
> Illustration: the Workflow tab before the first save
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="true"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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

######## BOARD R3-D05-390.dc.html
Label: R3-D05-390 · Illustration: the starting draft's Workflow tab on /new
> Illustration: the Workflow tab before the first save
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="true"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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

######## BOARD R3-E01.dc.html
Label: R3-E01 · Illustration: 1.30 before: the Publish tab of a first publish
> Illustration: the Publish tab before the press
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E01-390.dc.html
Label: R3-E01-390 · Illustration: 1.30 before: the Publish tab of a first publish
> Illustration: the Publish tab before the press
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E02.dc.html
Label: R3-E02 · 1.30 published from Credit: Published version 1.
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="true"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R3-E02-390.dc.html
Label: R3-E02-390 · 1.30 published from Credit: Published version 1.
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="true"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Credit
Credits nothing.
An activity of this type counts towards no curriculum item, and its encounter date may not be after today.
Credit rules JSON
{ "counts_for": [] }
After each save this shows the rules as they were saved, laid out by Wombat, one property a line.

######## BOARD R3-E02b.dc.html
Label: R3-E02b · 1.30 then Details: the Key fixed, the result kept, the focus on the Details tab
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-E02b-390.dc.html
Label: R3-E02b-390 · 1.30 then Details: the Key fixed, the result kept, the focus on the Details tab
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-E04.dc.html
Label: R3-E04 · states.md:916 a later draft, Audience deleted: the warning before the press
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E04-768.dc.html
Label: R3-E04-768 · states.md:916 a later draft, Audience deleted: the warning before the press
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E04-390.dc.html
Label: R3-E04-390 · states.md:916 a later draft, Audience deleted: the warning before the press
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E05.dc.html
Label: R3-E05 · Illustration: Publish pressed: saved first, then the question
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E05-390.dc.html
Label: R3-E05-390 · Illustration: Publish pressed: saved first, then the question
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="true"] Publish
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

######## BOARD R3-E07.dc.html
Label: R3-E07 · states.md:917 Discard draft… on KGK's draft
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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
[selected="selected"] Date delivered (delivered_on)
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-E07-390.dc.html
Label: R3-E07-390 · states.md:917 Discard draft… on KGK's draft
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
[button] [role="tab"] [aria-selected="false"] Details
[button] [role="tab"] [aria-selected="true"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
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
[selected="selected"] Date delivered (delivered_on)
Entrustment rating field
[selected="selected"] None — this form asserts no entrustment level
EPA field
None — this form is about no single EPA
[selected="selected"] EPA (epa_id)
The field naming the EPA this activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this one.
#### Sections and fields
[button] Add section
##### Teaching session
session
[button] [aria-label="Edit section Teaching session"] Edit
[button] [aria-label="Move section Teaching session up"] [disabled="disabled"] Up
[button] [aria-label="Move section Teaching session down"] [disabled="disabled"] Down
[button] [aria-label="Delete section Teaching session"] Delete
-
Topic
Text · topic · Required
[button] [aria-label="Edit field Topic"] Edit
[button] [aria-label="Move field Topic up"] [disabled="disabled"] Up
[button] [aria-label="Move field Topic down"] Down
[button] [aria-label="Delete field Topic"] Delete
-
EPA
EPA · epa_id · Required
[button] [aria-label="Edit field EPA"] Edit
[button] [aria-label="Move field EPA up"] Up
[button] [aria-label="Move field EPA down"] Down
[button] [aria-label="Delete field EPA"] Delete
-
Date delivered
Date · delivered_on · Required
[button] [aria-label="Edit field Date delivered"] Edit
[button] [aria-label="Move field Date delivered up"] Up
[button] [aria-label="Move field Date delivered down"] Down
[button] [aria-label="Delete field Date delivered"] Delete
-
Learning objectives
Long text · objectives · Required
[button] [aria-label="Edit field Learning objectives"] Edit
[button] [aria-label="Move field Learning objectives up"] Up
[button] [aria-label="Move field Learning objectives down"] Down
[button] [aria-label="Delete field Learning objectives"] Delete
-
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

######## BOARD R3-E11.dc.html
Label: R3-E11 · Illustration: Active unticked on a published type: Save draft asks
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.
#### Deactivate KGK Teaching Session Log?
Registrars are no longer offered it when filing. Activities already filed are not changed.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-E11-390.dc.html
Label: R3-E11-390 · Illustration: Active unticked on a published type: Save draft asks
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.
#### Deactivate KGK Teaching Session Log?
Registrars are no longer offered it when filing. Activities already filed are not changed.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-E12.dc.html
Label: R3-E12 · Illustration: KGK Teaching Session Log is inactive
> Illustration: KGK Teaching Session Log deactivated
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-E12-390.dc.html
Label: R3-E12-390 · Illustration: KGK Teaching Session Log is inactive
> Illustration: KGK Teaching Session Log deactivated
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-E16.dc.html
Label: R3-E16 · Illustration: Publish pressed with Active newly unticked: the question first
> Illustration: Active unticked on a draft, Publish pressed
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.
#### Deactivate KGK Teaching Session Log?
Registrars are no longer offered it when filing. Activities already filed are not changed.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-E16-390.dc.html
Label: R3-E16-390 · Illustration: Publish pressed with Active newly unticked: the question first
> Illustration: Active unticked on a draft, Publish pressed
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.
#### Deactivate KGK Teaching Session Log?
Registrars are no longer offered it when filing. Activities already filed are not changed.
You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-E17.dc.html
Label: R3-E17 · Illustration: Cancel: nothing saved, the focus back on Publish
> Illustration: after Cancel, nothing saved; the focus on the header's Publish
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.

######## BOARD R3-E17-390.dc.html
Label: R3-E17-390 · Illustration: Cancel: nothing saved, the focus back on Publish
> Illustration: after Cancel, nothing saved; the focus on the header's Publish
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
[button] [role="tab"] [aria-selected="true"] Details
[button] [role="tab"] [aria-selected="false"] Form
[button] [role="tab"] [aria-selected="false"] Workflow
[button] [role="tab"] [aria-selected="false"] Credit
[button] [role="tab"] [aria-selected="false"] Publish
[role="tabpanel"]
#### Details
Saved with Save draft, not with Publish: a change here reaches everyone when you save.
* marks a field you must fill in.
**Key
kgk_teaching_log
Fixed: version 1 is published.
Name *required
[value="KGK Teaching Session Log"] [aria-required="true"]
**Scope
Institution · Kgosi Kgari Teaching Hospital
The one scope you can write in.
This tool is
[selected="selected"] Not a WBA instrument
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
An inactive type is not offered to anyone filing an activity. Activities already filed are not changed.
