# Flow 17 round 1: assessment (T398)

Read-only, at HEAD `afe97fbd` (the boards are `465480c7`). Canvas "Wombat 17"
(https://claude.ai/artifact/AQNS1QZDaUNizG7dzKPSGs), design system copied at version 1794239792-3119 (`canvas.json`,
`designSystems`). Boards: `design/flows/17-activity-type-builder/round-1/*.dc.html` (17: Main, Steps, A1–A5, B1–B5,
C1–C5; every board's text and markup read), `canvas.json` and the round's stylesheet `f17r1.css`. `W/` =
`src/Wombat.Web/Components/Pages/`, `A/` = `src/Wombat.Application/Features/`, `D/` = `src/Wombat.Domain/`, `edit` =
`W/Admin/ActivityTypes/ActivityTypeEdit.razor`, `list` = `W/Admin/ActivityTypes/ActivityTypesList.razor`, `models` =
`W/Admin/ActivityTypes/BuilderModels.cs`, `app.css` = `src/Wombat.Web/wwwroot/app.css`, `seeds` =
`src/Wombat.Infrastructure/Activities/Seeds/`, `catalogue` = `src/Wombat.Infrastructure/Persistence/ActivityTypeSeedCatalogue.cs`.
**Obs** = observed in the code, the runbook or the boards. **Inf** = inferred. Brief sections are
`design/flows/17-activity-type-builder.md`'s. The `design/baseline/` captures were not opened: today's pages were read in
the code, at the lines cited.

**Q1–Q12 are decided** (the operator took every recommendation as written on 2026-11-10). The ask the canvas got
(`round-1-ask.txt:279`, "QUESTIONS THE DESIGN MUST ANSWER") still lists them as open, so the canvas answered some of them
itself (Spec, "The structures, side by side"). As in flow 16, a board that contradicts a decision is a must-fix below,
unless it argues to reopen it; where it argues, the point is a question in § 8.

## Summary

- **Pick A** (four tabs, repaired: Details, Form, Workflow, Credit as a tablist; the field editor in place; the rules
  read in words above the JSON).
  - **Why A:** it is Q1 as decided ((a), the tabs as a real tabs pattern, Details first for a new type). It is also the
    only structure that keeps Q2 (no diagram), Q4 (the editor under the row) and Q8 (Publish asks when there are
    warnings) together. It needs the fewest NEW components that are not shared by all three (N1 and N3).
  - **B does not argue convincingly to reopen Q1.** Its gain is real but small: 1.28 and 1.29 lose their tab presses
    (Steps, recomputed § 6: B and A both cost 5 more presses than today when every editor needs Done). Its costs are
    the ones Q1 weighed. Mini-CEX alone makes the longest page in the product. It adds two NEW layout components (N8
    page contents, N9 the sticky split). Its field editor is a dialog, so a refusal has to be carried back onto a closed
    row (its own note 4, DATA). Publish sits at the foot of a 40-field page (its own "Risk"). It publishes with warnings
    without asking (B5.1), against Q8 (M6).
  - **C does not either.** It draws a state diagram, against Q2 (M6). It puts the JSON editor behind "Edit as JSON",
    adds a route per step (PROPOSED) and a stepper (N10). Its own note 4 concedes that editing and previewing are never
    side by side. Its review step publishes with warnings without a question (C5.3), against Q8.
  - **Take from B:** B5.1's Publish section, as A's Publish panel (M5). It holds the versions, the warnings before the
    press, what stays pinned, and the button named for the version it makes. Q1 asked for "a Publish panel of its own",
    and Q3 for the warnings "before the press". A draws neither.
  - **Take from C:** C5.1's "What you are publishing" summary (form "1 section, 6 fields (5 required)", workflow "Draft →
    Logged or Cancelled; 2 moves", credit "Credits nothing") as the panel's first lines on a first publish, where there
    is nothing to compare. Nothing else: C1's Set by filter adds radios that Q7 did not ask for.
  - **Correct in A:**
    - the list's subtitle and the builder's (M1); the list's order, columns and words (M2);
    - the system-managed notice (M3); Discard's question (M4, Q14); the Publish panel and Q3's versions (M5);
    - the Steps board (M7); and S1–S14 below.
- **The must-fixes** (§ 7):
  - **M1:** Q12 is contradicted. The list's subtitle is new words, one sentence for all three roles. The builder has no
    subtitle at all: the owner line "College of Paediatricians of South Africa · Speciality · Paediatrics" stands in its
    place.
  - **M2:** Q7 and Q11 are contradicted.
    - Prof Mbatha's list at 1280 (A1.1) and Dr Kruger's (A1.6) put the Demo types first and their own last. A1.10 at 390
      puts hers first. Q11: "order her own types first".
    - Key is a column of its own (Q7: under Name, in monospace).
    - Set by gives the College's full name (Q7: "CPSA"), and Draft reads "Draft saved ‹date and time›" (Q7: "Saved
      ‹date›").
  - **M3:** Q9 is contradicted. The notice reads "Written by the MSF release. Nobody changes it here, the College
    included, and nobody is offered it when filing an activity." Q9's is "Written by the MSF release. Its form changes
    only with a release of Wombat." Learner Feedback's notice is not drawn.
  - **M4:** Discard draft's body is neither Q8's nor true. The board says "Its changes since version 1 are lost …".
    Q8's says "Your changes since version 1 are removed." Both are false against the code:
    - Save draft writes Name, Description, Scope, Active and "This tool is" at once (`A/Activities/Commands/SaveActivityTypeDraft/SaveActivityTypeDraftCommand.cs:113–119`).
    - Discard clears only the staged form, workflow and credit (`D/Activities/ActivityType.cs:169–177`).
    - On a type never published (KGK's before 1.30, Dr Kruger's after 6.14a) there is no version 1, and Discard puts
      back the default draft.
    - Q14 asks for the corrected words.
  - **M5:** Q1's Publish panel and Q3's Versions are missing or contradicted.
    - A has no Publish panel: the warnings appear only inside the dialog, after the press.
    - The Versions table on Details adds "Activities filed on it" (DATA), which Q3 left out.
    - Q3's line "Activities filed under a version stay on it." is not drawn.
  - **M6:** B and C contradict Q1, Q2 and Q8. B5 and C5 publish with warnings without asking, and C draws a diagram.
    Moot once A is picked.
  - **M7:** the Steps board.
    - Its "Today" column counts each select as a press and as a field, so 1.27 reads 23 + 24 where today is 14 + 24.
      With that fixed, A's "30 + 29 against today's 31 + 29" becomes 30 + 29 against 22 + 29. So A costs 8 presses
      more, all Done presses, unless an open editor's change is kept without Done (S4).
    - It misses `states.md`'s rows 903, 905, 907, 910 and 916–918, and the 1.25 Expect's "Metadata, as text".
- **The screenshots and the seeds did not arrive.** Spec § 1, Obs: "Opened: none of the 15 screenshots … and none of the
  nine seed files". The canvas's files held only `project/ds/wombat/`; its request for the folder was refused. The
  boards rightly bracket what only a capture or a seed held, but the code fills most of those brackets (S9). Round 2's
  ask must say how the files reach it: flow 16's lesson ("from the staged folder") did not hold this time, since the
  staged folder never reached the canvas.
- **Step count: the 11 steps are right** (1.24–1.31 is 8, then 6.14a, A.7.9 and A.7.14; the brief's § 8 pastes each).
  The Steps board counts journeys for 1.26–1.30 and 6.14a only, as the ask asked. Its counts are recomputed in § 6.

## 1. What was drawn

17 boards in four rows (`canvas.json` notes row0–row3).

| Row | Boards | State drawn |
|---|---|---|
| 0 · Spec, Steps | Main, Steps | Arrival (none); the structures side by side (Q1–Q9); components (N1–N12 NEW); classes (W1–W12 canvas only, N1–N12); DESIGN.md rules changed; PROPOSED; DATA; later; edge cases; accessibility. Journeys counted for 1.26–1.30, 6.14a and the 390 scroll; Expects changed |
| 1 · A, four tabs | A1 list (10 frames), A2 read (6), A3 new and form (9), A4 workflow and credit (4), A5 publish and after (8) | 1.24, 1.31, `zzz`, empty, Kruger, Administrator, loading, load error, 390; Mini-CEX read on Form, Details, Workflow, Credit, the MSF type, 390; 1.26 for both, the first save, the form built, duplicate key, a User field, 40 fields, 390 edit and preview; 1.28 refused and saved, 1.29, 390; a later publish asked, 1.30, Discard asked and done, Unavailable, load error, loading, 390 |
| 2 · B, one page | B1 (4), B2 (3), B3 (5), B4 (2), B5 (3) | The list grouped by owner; Mini-CEX and MSF read as one page; the form with its editor in a dialog; the rules beside the JSON; Publish as the last section |
| 3 · C, a stepped path | C1 (2), C2 (2), C3 (6), C4 (3), C5 (4) | The list with a Set by filter; a reader's summary page with a diagram; steps 1–5 with a panel editor |

- **The wireframes** are grey `.wf-*` boxes with real words. Their inline `style` attributes (A1 154, A3 101, Main 4)
  are layout scaffolding for the wireframes: canvas only, as flow 16's 390 padding was. Every board links
  `ds/wombat/tokens.css`, `components/bundle.css` and `f17r1.css` in that order, with `body{margin:0}` alone in
  `<style>` (Obs).
- **f17r1.css holds no hex** (Obs). Its W rows are canvas only, and each N rule names its row.
- **No gendered word on any page** (Obs). "They" in the runbook's description only.

**Not drawn in round 1** (round 2's, S8):
- **For every page:** a failed save (a non-validation exception); 768; the 641–1115 px range, where Q5 puts the same
  Preview control and DESIGN.md stacks the columns to 1115 px; and the 900–1100 px sweep (§ 11, flow 15).
- **The builder:** the section editor in place (1.27's first act: key `session`, title `Teaching session`); the writer's
  Scale field with the scale by name in its picker (T271's "Select…", `edit:488`); the Administrator's builder (Global
  first, four scopes); a College admin with no scope (words only); Learner Feedback's notice; an inactive type; a draft
  saved by someone else; a seeded type with a draft (words only); a form setting pointing at a deleted field (words
  only); a refusal naming a section or a credit directive; Discard on a type never published (M4).
- **The tabs pattern's other half:** the tabpanels (S2).

## 2. Structure A: four tabs, repaired

A draws, rightly (Obs):
- **The tabs:** a `role="tablist"` (`aria-label="Activity type"`) of four `role="tab"` buttons with `aria-selected`,
  one row at 390 (87 px each), `.builder-tab` at 44 px. Details is selected for a new type; Form is selected when a
  reader opens a type (A2.1, as 1.25 expects).
- **The header:** no Back to list. The trail is Home › Activity types › the type, folded at 390 to the shell's one link
  "‹ Activity types" (`.wf-back` stands in for `.breadcrumb-ancestor`, DESIGN.md § Breadcrumbs "Below 641px"). Save
  draft, Publish… and Discard draft… sit in the header. Publish's reason, "Publish: Save a draft to publish.", is text
  beside it.
- **The rows:** each field row reads label, then type by label and key in monospace, then Required ("Choice · audience ·
  Required"; today "audience · Choice", `edit:301`). The actions are named for the row ("Edit field Audience",
  "Delete section Teaching session", "Move field EPA up"; Obs, 60 `aria-label`s on A3). Delete is an outline button that
  asks nothing (Q8; today `btn-danger`, `edit:284`, `:310`).
- **The editor in place:** Edit opens the editor under its row (Q4), striped with the row (`.field-row.is-editing`).
- **Labels, not keys:** the form settings by label with the key as detail ("Date delivered — delivered_on"; today keys
  only, `edit:189–197`, `:211`, `:222`, `:236`). Names a by role label ("Institutional admin", "Committee member";
  today `WombatRoles` raw, `edit:1037–1049`, `:375`). The scale by name to a reader, "CPSA Paediatric Entrustment Scale
  v11.1" (T271).
- **The rules in words:** a states line and a captioned "Moves, in words" table (Move, From, To, Who moves it, Checks,
  Note). It is right against the seed (`seeds/mini_cex_cpsa/workflow.json`):
  - Submit is `subject|creator` with `owned`, "The mover's own fields";
  - Complete is `field:assessor_user_id` with `all`, "The person Assessor names";
  - Decline needs a note and checks `draft`, "Formats only";
  - Cancel runs from draft or requested.
  The credit read matches `credit.json` too: `curriculum_item_match.epa_field: epa_id`, `amount: 1`,
  `minimum_level_field: overall_level`.
- **1.29's words:** "This type credits nothing. … Its only date rule: the encounter date may not be after today."
  (T160).
- **The refusals:** the summary first, focused (`role="alert" tabindex="-1"`, `.refusal-summary-list`, `app.css:1641`),
  each item a link that switches tab (Q6). The shipped sentence is kept word for word after a prefix ("Form:",
  "Workflow, move Cancel:"). The tab carries a count ("Form · 1 problem"), and both rows are marked with PROPOSED
  per-field words.
- **The results:** "Draft saved." on the first save at the type's own address, focused (T291 item 3), with the h1 "Edit
  KGK Teaching Session Log". "Published version 1." with the key read-out "Fixed: version 1 is published.".
- **Loading** draws no tabs and no actions, and is titled "Activity type". **A load error** is a sentence and Try
  again. **An unknown id** is Unavailable, with no "New activity type".
- **At 390:** the list stacks, a block per type with its View or Edit. The Form tab offers "Edit the form" / "Preview" as
  a choice of two (Q5), so the preview is one press away, not 4,390 px down.

**Correct in A** (§ 7): M1–M5, M7; S1–S14; the nits in one pass. The ones that change the structure:
- **M5:** a Publish panel. Recommend a fifth tab, "Publish", holding:
  - C5.1's summary on a first publish;
  - the warnings by label ("Field 'Audience' will be removed."; "changes type from Long text to Text", T324);
  - Q3's line, "Activities filed under a version stay on it.";
  - the Versions table (number, ISO date in SAST, publisher by name), moved from Details;
  - the button named for the version it makes ("Publish version 2"), or its reason as text when off.

  The header keeps Publish…, so 1.30 stays one press, and the dialog (A5.1) still asks when there are warnings (Q8).
  If the header drops Publish, 1.30 is 2 + 0 (the tab, then the button).
- **S4:** say whether Done is needed. The step cost turns on it (§ 6).
- **Q13:** the JSON "as typed".

**Against the code** (Obs):
- **The publish warnings need no DATA.** The page computes them, live, from the published schema and the builder's
  current model (`edit:1107–1119`, `models:178–239`). B5's "today they are computed at publish" is false. But they read
  the *unsaved* model, while Publish publishes the *saved* draft (`edit:943–960`; it saves first only for a new type,
  `:927`). So A5.1's dialog would list warnings for a form that is not the one published if she edits after her last
  save (Q16). They also cover the form only: a changed workflow or credit rule warns of nothing (Inf; not this flow's to
  fix).
- **The warnings still print enum names and keys**: "changes type from 'LongText' to 'Text'" (`models:209`); the role
  raw in "will name a …" (`models:227`); the form settings by key ("The encounter date moves from field 'a' to 'b'.",
  `models:350–369`). A5's notes promise labels. That is T324's work, folded by Q10.
- **"Draft: None" and "Draft saved ‹time›" are marked DATA, but both are read today.** The editor reads `HasDraft`,
  `StagingUpdatedOn` and each version's `PublishedOn` (`A/Activities/Queries/GetActivityTypeEditor/GetActivityTypeEditorQuery.cs:194`,
  `:205`, `:210`). The list reads `DraftUpdatedOn` (`A/Activities/Dtos/ActivityDtos.cs:75`, filled at
  `ListActivityTypesAdminQuery.cs:61`). Only the names are DATA: who saved the draft (`StagingUpdatedByUserId`, an id,
  `:204`) and who published (`PublishedByUserId`, an id, `:211`). For a seeded version the publisher is the seeder, so
  it needs words of its own (S9).
- **Unavailable is a handler change.** The editor query throws "The activity type could not be found." for an unknown
  id and for one the caller may not read alike (`GetActivityTypeEditorQuery.cs:83`, `:87`), and the page shows the
  exception's text (`edit:818`). Flow 14's Unavailable needs a null answer for both, with the load error kept for a
  throw (as `GetEpaByIdQuery` in flow 15). DATA, cheap; not on the Spec's table (S10).
- **System-managed is on neither DTO** (`ActivityDtos.cs:65–77`, `:96–`). `MayWrite` judges the scope only
  (`A/Activities/ActivityTypeAdminScope.cs:50–67`), so Q9's View on every row needs `SystemManaged` on the list item and
  the editor, and a refusal in `MayWrite` and the guard. DATA plus a rule change; Q9 decided it, and the Spec does not
  list it (S10).
- **Set by in words needs the College's name per row.** The list resolves the target's name and its owning College's
  id (`ListActivityTypesAdminQuery.cs:81–82`), not the College's name. DATA, cheap; unlisted (S10).
- **Opening a section opens its first field too** (`SelectSection`, `edit:1015–1019`). At load the first section and
  field are already open (`edit:839–842`). A's one editor at a time removes both behaviours. Say so on the Spec
  (PROPOSED).
- **Two of A's words are today's, not proposals.** "Activity type" as the h1 while loading or after a failure is built
  (`edit:642–646`), so A5.6's "PROPOSED" mark is wrong (N3). Publish is offered on `/new`, off, with its reason, as
  today (`edit:30`, `:626`); A3.1 is right to draw it.
- **The field editor's words are shortened, unmarked:** "Key" for "Field key" (`edit:441`); "Options, one per line" and
  "'value | Label' shows the label and stores the value." for today's "One option per line; a comma is part of the
  option. Write value | Label to show a label: the form shows the label, and the activity stores the value."
  (`edit:477`). The help that a comma is part of the option goes, and it is the rule a 12-option Audience needs (N4).
- **The refusal for a bad role prints code names:** "Use one of: InstitutionalAdmin, Coordinator, …"
  (`D/Activities/Schema/FormSchemaParser.cs:351`). It cannot be reached from A's picker, but a typed workflow or a
  hand-edited schema can reach it. T324's kin (N5).

## 3. Structure B: one page of h2 sections

- **The list (B1):** an `<h2>` per owner, hers first, each a captioned table with Scope as a column. It is flow 16's
  tree pattern, but grouping costs three tables where Q7 asked for one with Set by in words. Its 390 count lines read
  "1 types" (N).
- **The builder (B2–B5):** h2 Details, Form, Preview, Workflow, Credit, Versions/Publish under "On this page" (N8, links
  that move the focus); the preview sticky beside Form (N9); the field editor a dialog (flow 09's dialog with a field);
  Publish the last section, "Publish version 2", with no dialog.
- **Its headings are out of order:** `### Sections and fields` with `### Request` beside it (B2.1, B3.2). Moot.
- **Reject** (§ Summary). **Take** B5.1's section as A's Publish panel (M5).

## 4. Structure C: a stepped path

- **The list (C1):** A's table with a Set by filter (radios, counted, with Show).
- **The builder (C2–C5):** Details → Form → Workflow → Credit → Review and publish. Each step's primary is "Save draft and
  continue to …". A route per step (PROPOSED); a panel editor that replaces the preview while open (N11); a state diagram
  (N12, `aria-hidden`, the table as the text of record); the JSON behind "Edit as JSON"; a review step as the publish's
  confirmation.
- **Reject** (§ Summary; M6 for Q2 and Q8). **Take** C5.1's "What you are publishing" (M5).

## 5. The list (screen 1), across the structures

All three draw, rightly (Obs):
- the search applied with Show and Clear (Q7), counted in a status line ("12 of 23 activity types match “cpsa”.",
  `role="status"`);
- no-match apart from the empty catalogue, in T326's words ('No activity types match "zzz".', "The search is kept above.
  Clear it to see all 23.", Clear the search). Today one empty state serves both (`list:31`), and the DataTable's own
  Empty is never reached (`list:69–73`);
- "Version 1" / "Not published" in place of "v1" / "v0" (`list:49`);
- the system-managed rows' line under the name ("Written by the MSF release."), View for everyone, the Administrator
  included (Q9);
- no New activity type to a caller with no scope, and New after the rows load (A1.8; `list:14`).

**Against the code** (Obs):
- **The list is ordered by name** (`ListActivityTypesAdminQuery.cs:49`): ACAT, Case-based Discussion, Case-Based
  Discussion (Paediatrics), Chart-Stimulated Recall (Paediatrics) … KGK Teaching Session Log among the K's. A1.1's order
  (ten Demo types, then the twelve CPSA, then KGK) is neither today's nor Q11's (M2).
- **The search filters in memory, not on the server** (`list:84–90`). Each keystroke is still a SignalR round trip
  (`@bind:event="oninput"`, `list:25`), so Show is right. The Spec's "one round trip" holds.
- **Edit and View are links today** (`<a class="btn btn-outline btn-sm">`, `list:59`, `:63`), and New activity type is a
  link (`list:16`). Every list board draws them as `<button type="button">` (S1).
- **The non-creator's empty state** is "No activity type is open to you yet." today (`list:31`). A1.5's "No activity types
  yet." is new words, unmarked (N6).
- **Recount at 1280** (§ 11, flow 16): seven columns with a two-line Set by, at 1280, for each of the three roles. M2
  takes Key into the Name cell, which leaves six.

## 6. The Spec and Steps boards

- **Spec § 1** says first that nothing arrived, and brackets in its place. That is right, as the ask asked.
- **Spec, Components:** N2 (the outline) and N5–N7 are shared by all three structures. A adds N1 (tabs) and N3 (the
  editor in place). The Spec marks `choice-group` for A's Edit/Preview, which is shipped (`app.css:4734`), and calls
  `details-list` a read view (`.detail-list`, shipped).
- **Spec, DESIGN.md:** "JSON monospace: kept; wraps at 390; kept as typed". N5's `.json-editor` has
  `white-space: pre` (`f17r1.css`), which does not wrap: the textarea scrolls inside its box (N7). "Kept as typed" is
  Q13.
- **Spec, Accessibility:** "Publish when off is aria-disabled". The boards draw `disabled` (A5.2: `<button … disabled>`).
  Pick one. `aria-disabled` keeps the reason reachable by Tab, and A.7.14's exemption still holds (N8).
- **Spec, edge cases:** "A draft saved by someone else: the version strip names who saved it; Save draft overwrites it
  (no lock today)" is right (`D/Activities/ActivityType.cs:88–126`). "A College admin whose College has no speciality:
  … 'You have no scope to create …' with no form" is built (`edit:64–71`).

**Steps § 1, recomputed.** A press is a click on a button, tab, link or checkbox. A field is a text typed or a choice
made in a select or radio, and is not also a press. The board counts each select as both ("Today 23 + 24 … 3
settings"), so its "Today" is 9 presses high at 1.27.

| Step | Today | A 1280 | A 390 | B 1280 | C 1280 |
|---|---|---|---|---|---|
| 1.26 | 3 + 3 (New, Metadata tab, Save draft; Key, Name, Description) | 2 + 3 | 2 + 3 | 2 + 3 | 2 + 3 |
| 1.27 | **14 + 24** (Form tab; Edit section, which opens Title too, `edit:1015–1019`; Add ×5; Required ×4; save; Edit Audience; save) | **15 + 24** if an open editor's change is kept without Done; **23 + 24** if each needs Done (Form tab; Edit section, Done; Edit Title, Done; 5 × (Add, Done); Required ×4; save; the summary's link; Done; save) | +2 (Preview, Edit the form) | 22 + 24 (a dialog needs Done) | 22 + 24 |
| 1.28 | 3 + 2 | 3 + 2 | 3 + 2 | 2 + 2 | 3 + 2 |
| 1.29 | 1 + 0 | 1 + 0 | 1 + 0 | 0 + 0 | 1 + 0 |
| 1.30 | 1 + 0 | 1 + 0 (2 + 0 if Publish leaves the header, M5) | 1 + 0 | 1 + 0 | 1 + 0 |
| **Total** | **22 + 29** (board 31) | **22 + 29** or **30 + 29** | 24 or 32 | 27 + 29 | 29 + 29 |
| 6.14a | 7 + 5 (menu, MSF, the trail, New, Metadata tab, Save, the trail; Key, Name, Description, Sub-speciality, back to Speciality) | 6 + 5 | 6 + 5 | 6 + 5 | 6 + 5 |

- **The Total row is wrong:** today is 22 + 29, not 31 + 29. A matches today only if Done is optional. With Done it is 8
  presses more, and B and C are 5 and 7 more. The board presents A as one press cheaper than today, which is wrong
  (M7).
- **6.14a:** the board's 8 + 3 / 7 + 3 counts "Scope looked at" as one press. Two choices (Sub-speciality, then
  Speciality again) are the runbook's Do. The −1 for A holds.

**Steps § 2, Expects changed.** The board's rows for 1.24–1.31, 6.14a, A.7.9 and A.7.14 are right as far as they go.
Missing (M7):
- **1.25's Expect:** "**Metadata**, as text" → Details; "**Workflow**, as a code block" and "**Credit**, as a code
  block" → in words (listed); `overall_level`'s settings read under its row.
- **1.26's Do:** "open the Metadata tab" (listed), and its Expect, "opens on the Form tab" → Details.
- **1.30:** under M5, the Expect may name the Publish panel. The hint "Save a draft to publish." becomes text (listed).
- **`states.md`:**
  - 903 ("Draft reads 'Draft saved'" → "Saved ‹date›");
  - 905 ("the page's empty card" → no-match);
  - 907 (narrow → stacked);
  - 910 ("Metadata tab" → Details);
  - 916 (warnings: reached on the Publish panel, or by pressing Publish…);
  - 917 (Discard asks first);
  - 918 ("The activity type could not be found." → Unavailable's words).
- **Searched, no other hit:** "Back to list" (1.25's and 1.31's Do, listed), "v0" (6.14a's Actual only), "Configured
  activity types" (none). Act 2's KGK log filing is flow 03's page and unchanged.

## 7. Corrections

Severity: **M** must (round 2 cannot go out without it), **S** should, **N** nit.

### Wrong against the code, the runbook or a decision

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| M1 | A1–C1 every list frame; A2–A5, B2–B5, C2–C5 headers | **Q12 is contradicted.** The list's subtitle is "The forms registrars file and assessors rate. You can change the ones you set; the others you can read." for all three roles. Use Q12's: "The forms registrars file and assessors complete. You can change the types you set; the College sets its instruments." to an institution, and the College's and the Administrator's in their own words (round 2 drafts them, PROPOSED). The builder has no subtitle; "College of Paediatricians of South Africa · Speciality · Paediatrics" stands under the h1. Use Q12's "What a registrar fills in, who moves it on, and what it counts towards." as the subtitle, with the owner line as flow 15's owner mark under it (`.muted .text-sm`). Today's subtitles: `list:11`, `edit:648–650`. | M |
| M2 | A1.1, A1.6, A1.7, A1.10 | **Q7 and Q11 are contradicted.** (a) Her own types first (Q11): A1.1 and A1.6 list the Demo types first and hers or his last, while A1.10 at 390 puts hers first. One order at every width: the caller's own (the types they may change), then the rest by name. For the Administrator, by name. (b) Key under Name in monospace (Q7), not a column. (c) Set by in Q7's words ("CPSA", "Kgosi Kgari Teaching Hospital", "Demo College", "the platform"), the scope as detail. (d) Draft "Saved ‹date›" (Q7; the date is read, `ActivityDtos.cs:75`) or "None". | M |
| M3 | A2.5, B2.2 (and C by reference); Spec PROPOSED | **Q9's notice.** "Written by the MSF release. Its form changes only with a release of Wombat." and, for Learner Feedback, "Written by the learner-feedback release. Its form changes only with a release of Wombat." (Q9). Draw the second (A2.5 draws MSF only). The board's sentence "nobody is offered it when filing an activity" is true (`A/Activities/Queries/ListActivityTypes/ListActivityTypesQuery.cs:51`) and may follow, marked PROPOSED. The row line "Written by the MSF release." is right. | M |
| M4 | A5.3; Spec PROPOSED (Discard) | **Discard's body is neither Q8's nor true.** The title "Discard the draft of KGK Teaching Session Log?", Cancel first and the confirm "Discard draft" are right. The body: (1) Q8's words, "Your changes since version 1 are removed. Version 1 stays published.", not the board's "Its changes … are lost …". (2) Both overstate. Discard clears only the staged form, workflow and credit (`ActivityType.cs:169–177`). Name, Description, Scope, Active and "This tool is" were written by the last Save draft (`SaveActivityTypeDraftCommand.cs:113–119`) and stay. (3) On a type never published there is no version 1, and the page reloads the default draft (`GetActivityTypeEditorQuery.cs:195–197`). Q14 asks for the words. Draw both cases. | M |
| M5 | A2.2, A5.1–A5.4; Spec N7 | **Q1's Publish panel and Q3's Versions.** (a) A has no Publish panel. The warnings show only in A5.1's dialog, after the press, where Q3 put them "before the press" and Q1 asked for "a Publish panel of its own". Draw the panel (§ 2: versions, warnings, Q3's line, the button), from B5.1 and C5.1. (b) The Versions table adds "Activities filed on it" (DATA), and A5.1 adds "(‹n› of them DATA)". Q3 left the count out ("left out unless you want it"): drop both. (c) Add Q3's line, "Activities filed under a version stay on it." The dialog's "Activities already filed stay on version 1 …" may stay without the count. | M |
| M6 | B5.1, B5.3, C5.1, C5.3; C2.1, C4.1 | **B and C contradict Q1, Q2 and Q8.** B and C publish with warnings without a question (Q8: Publish asks when there are warnings). C draws a state diagram (Q2: "no state diagram"). Neither argues to reopen the decision. Moot once A is picked. | M |
| M7 | Steps § 1, § 2 | **The Steps board.** (a) The "Today" column counts each select as a press and as a field: 1.27 is 14 + 24, not 23 + 24, and the total 22 + 29, not 31 + 29 (§ 6). A's figure stands only if Done is required on every editor (S4). (b) The Expects it misses (§ 6, Steps § 2): 1.25's "Metadata, as text"; 1.26's "opens on the Form tab"; `states.md` rows 903, 905, 907, 910, 916, 917, 918. (c) 1.30's "Publish… named with an ellipsis": an ellipsis promises a question, and Q8 asks only when there are warnings, so 1.30's Publish asks nothing. Call it "Publish" and let the dialog follow when there are warnings, or "Publish version 1", as B and C name it (N9). | M |
| S1 | A1–C1 every list frame | **Edit, View and New activity type are links**, not `<button>`s: they navigate (`list:16`, `:59`, `:63`; `Accessibility/RowActionMarkupTests`). Keep `<a class="btn …">`. | S |
| S2 | A2–A5 tab bars | **The tabs pattern, whole** (Q1). The boards draw `role="tablist"`, `role="tab"` and `aria-selected` only. Draw each panel as `role="tabpanel"`, `aria-labelledby` its tab, with `aria-controls` on the tab, and a roving `tabindex` (0 on the selected tab, −1 on the others) with Left/Right/Home/End. Record the choice in DESIGN.md § Builder layout (T280). The problem count ("Form · 1 problem") is inside the tab's name; keep it, or move it to `aria-describedby` so the name stays "Form". | S |
| S3 | A2.3, A2.4, A2.5 | **Headings.** The reader's Workflow and Credit tabs have no h2: the panel starts with the states line and a caption. The MSF frame puts `### ‹section›` straight under the h1, which is what T280 forbids. Each panel's first heading is an h2: "Workflow", "Credit" (or A4's "The workflow, in words"). The in-place editor's heading, "Editing field Audience", is an h3 beside its section's h3; make it an h4. | S |
| S4 | A3.4–A3.6; A notes 5 | **Q4's Done.** Q4: "its Edit becoming Done". A keeps Edit, Up, Down and Delete on the open row, and puts Done and Cancel at the editor's foot. A3.6 (the User field) draws neither. Say whether opening another editor, or Save draft, keeps the open editor's change without Done. It does today, since the editor binds live (`edit:441–498`). If it does, 1.27 is 15 + 24; if not, 23 + 24. Recommend: it does, as today. The row's Edit becomes Done (`aria-expanded`). Drop "Cancel puts back what was there", a NEW copy-and-restore that no step needs. If Cancel stays, mark it PROPOSED. | S |
| S5 | A2.1–A2.4 states line | **Mark the final state.** Mini-CEX's Completed is its only `terminal` state (`seeds/mini_cex_cpsa/workflow.json`). A2.3 marks none, while A4 marks KGK's Logged "final". Declined and Cancelled are not terminal on Mini-CEX; read them as such. | S |
| S6 | A5.5 | **Unavailable's words** (DESIGN.md § Alerts, "Every Unavailable is drawn one way"): h1 "Activity type unavailable"; "This activity type does not exist, or you cannot open it."; Go to Activity types. The trail ends at the h1. The board brackets the heading and keeps "The activity type could not be found." Drop that sentence (the handler change is S10). | S |
| S7 | A3.4, A3.8, A5.8 | **Where results and the summary sit.** A3.3, A3.5, A4 and A5.2 draw "Draft saved.", the refusal summary and "Published version 1." above the h1, between the trail and the header. A5.8 at 390 draws it under the h1. Today the `ActionResult` follows `PageHeader` (`edit:15–48`). One place, under the header, at every width. | S |
| S8 | (not drawn) | **The states § 1 lists** under "Not drawn": a failed save; 768; 641–1115 with Q5's Preview control; the 900–1100 sweep of the builder and the list against their container; the section editor in place; the writer's Scale field with the scale by name in its picker (T271, `edit:488`); the Administrator's builder; Learner Feedback's notice (M3); Discard on a type never published (M4); an inactive type; a refusal naming a section or a directive; the heavy case's real h1 (A3.7 reads "Edit KGK Teaching Session Log" over ‹type name›). | S |
| S9 | A1, A2, B2, C2 | **Brackets the code fills.** Paste them. (a) The Demo keys: `acat`, `cbd`, `dops`, `journal_club`, `mini_cex`, `procedure_log`, `qi_project`, `reflective_note`, `research_output`, `teaching_session` (`seeds/`). (b) The CPSA keys: `dops_cpsa`, `cbd_cpsa`, `direct_observation_cpsa`, `cca_cpsa`, `rca_cpsa`, `chart_stimulated_recall_cpsa`, `clinical_audit_cpsa`, `portfolio_review_cpsa` (`catalogue:126–166`). (c) Mini-CEX's description: "Mini clinical evaluation exercise - direct observation of a focused clinical encounter, followed by immediate feedback." (`catalogue:125`). (d) Its sections, keyed `request`, `assessment`, `feedback`. Its fields: `setting` Choice, `presenting_problem` Text, `complexity` Choice, `strengths`, `improvements`, `plan` Long text, all ten required; the Assessor's help "The supervisor who observed you. They complete this assessment." and Date observed's "When the encounter happened, not when this form is completed." (`seeds/mini_cex_cpsa/schema.json`). (e) The MSF description, "The per-EPA evidence record a released multi-source feedback campaign leaves behind." (`catalogue:174`). (f) Each seeded version's publisher is the seeder, not a person: draw its words ("Published by Wombat's catalogue", PROPOSED) rather than ‹name›. | S |
| S10 | Spec, DATA | **DATA missing from the table:** `SystemManaged` on the list item and the editor, with `MayWrite` refusing it (Q9; `ActivityDtos.cs:65–77`, `ActivityTypeAdminScope.cs:50–67`); the College's name per row for Set by (`ListActivityTypesAdminQuery.cs:81–82`); a null read for Unavailable (`GetActivityTypeEditorQuery.cs:83`, `:87`); the preview's EPA list by the type's own rule (T291 item 6; the board draws "15; not EPA-001" without saying where it is read). **Mislabelled DATA:** the draft's time, Draft None and each version's date are read today (§ 2). | S |
| S11 | A3.1–A3.3, A5.2 (Details) | **The Details tab says what takes effect at save.** Name, Description, Scope, Active and "This tool is" are written by Save draft, not by Publish (`SaveActivityTypeDraftCommand.cs:113–119`). Unticking Active hides a published type from every registrar's picker at that save (`ListActivityTypesQuery.cs:48`). Only "This tool is" says so today (`edit:153–154`). Q14 and Q15. | S |
| S12 | A3.4, A3.5 (Form settings) | **The pickers' help is new, unmarked** ("Date fields only. Or: None — …", "Scale fields only. This form has none."), and today's card subtitle, "Which field carries the encounter date, which carries the entrustment rating, and which names the EPA." (`edit:181`), is gone. Mark the new help PROPOSED, or keep today's. | S |
| S13 | A4.1–A4.4, B4, C4 | **"Kept as you typed it: a reload or a publish gives this text back"** is drawn as page text on every JSON editor. Under Q13's recommendation it becomes "Shown as saved, laid out one rule per line." or nothing. | S |
| S14 | A2.2 | **The Versions table's date** is ISO and SAST (Q3). The time is in `PublishedOn`; draw "2026-11-05 14:32 SAST" or the date alone, consistently with the strip. | S |
| N1 | B1.4 | "1 types". | N |
| N2 | A3.7 | The heavy case's h1 reads "Edit KGK Teaching Session Log" under ‹type name›. | N |
| N3 | A5.6, A5.7 | "Activity type" as the loading and failed h1 is built (`edit:642–646`): not PROPOSED. | N |
| N4 | A3.4–A3.6 | The editor's labels and help as built (`edit:425`, `:441`, `:477`), or marked PROPOSED. Keep "a comma is part of the option". | N |
| N5 | (none) | The parser's role refusal lists code names (`FormSchemaParser.cs:351`): T324's kin, for the build. | N |
| N6 | A1.5 | The non-creator's empty state: today "No activity type is open to you yet." (`list:31`). Keep it, or mark it. | N |
| N7 | f17r1.css N5 | `.json-editor { white-space: pre }` against A4's note "the textarea wraps (pre-wrap)". | N |
| N8 | Spec § Accessibility; A5.2 | `aria-disabled` (Spec) against `disabled` (boards). | N |
| N9 | Steps § 2; A header | "Publish…" with an ellipsis when it asks only sometimes (M7 c). | N |
| N10 | A3.1 | "flow 16's Q19" for a one-choice read-out: flow 16's Q19 is a curriculum's sub-speciality fixed once used. The read-out for a picker of one is new: PROPOSED on its own. | N |
| N11 | A2.1, A2.6 `summary` | The JSON fold's `<summary>` is 32 px at 390: 44. | N |
| N12 | A1.9 | "Could not load Activity types." → "Could not load the activity types." (as "Could not load the specialities.", `W/Admin/Institutions/CollegeTreeWords.cs:78`). | N |

### Invented people or values

- **No invented person** (Obs). Nolwazi Mbatha, Anton Kruger, Demo Administrator, Kgosi Kgari Teaching Hospital, the
  Colleges, the 23 names, the five keys given, the ids, KGK's six fields and its workflow are the ask's.
- **Right against the seed:** Mini-CEX's field labels and the three form settings; overall_level's help, word for word;
  the four moves, their actors, validations and Decline's note; the credit directive (§ 2).
- **"2026-11-05 by Nolwazi Mbatha"** for KGK's version 1 is the replay day and the publisher, right.
- **Bracketed where the ask gave nothing,** rightly. S9 fills those the code holds.

### PROPOSED items, with a recommendation each

| Item | Recommend |
|---|---|
| Subtitles (list, builder) | Q12's words (M1) |
| Set by in words; "Version 1" / "Not published"; search with Show; no-match with Clear | Accept, in Q7's words and order (M2) |
| A system-managed row's line and notice; View to everyone, the Administrator included | Accept, with Q9's notice (M3) |
| A new type opens on Details | Accept (Q1) |
| Scope a read-out where it has one choice | Accept: it reads as a fact, and the picker of one is noise. N10 for its citation |
| Per-field and per-move refusal words; the summary's prefixes | Accept (Q6), today's sentence kept after the prefix |
| One editor at a time, under its row; Done; Cancel restores | Accept the first two; drop Cancel's restore, or mark it (S4) |
| Sections fold (Show fields / Hide fields) on a heavy form | Accept, for the 40-field case only (more than ‹n› fields; round 2 says n) |
| The preview's person-picker line, "In the preview this offers nobody …" | Accept |
| A form setting kept as "Date delivered — not in the form", invalid, so it can be cleared | Accept |
| A seeded type with a draft: the version strip's seed note | Accept the fact; shorten it: "While this draft is saved, the College's seed updates stop reaching this type, until you publish or discard it." |
| Discard draft… asks | Accept, in Q14's words (M4) |
| Publish asks when there are warnings, listing them; "Publish version 2" | Accept (Q8), with the panel (M5) |
| The pinned line, with a count | Accept without the count (M5, Q3) |
| The JSON kept as typed | Q13: drop; lay out the saved JSON instead |
| Loading's h1 "Activity type"; the load-error sentences | Built (N3); the sentences accept (N12) |
| B: grouped list, contents list, sticky preview, dialog editor, Publish section with no dialog | Drop (A picked), but take B5.1's section as A's panel |
| C: a route per step, "Save draft and continue", the diagram, the review step as confirmation | Drop (A picked; M6), but take C5.1's summary |

### DATA items, with a recommendation each

| Page | Item | Recommend |
|---|---|---|
| List | When the draft was saved | Not DATA: `DraftUpdatedOn` is read (`ActivityDtos.cs:75`). Draw it (Q7) |
| List | Who saved it | Drop from the list (Q7: "Saved ‹date›") |
| List | The College's name per row (Set by) | Accept (cheap: the owning College's id is resolved, `ListActivityTypesAdminQuery.cs:81`) |
| List, builder | `SystemManaged`, and `MayWrite` refusing it | Accept (Q9; unlisted, S10) |
| Builder, version strip | Who saved the draft, by name | Accept (cheap: `StagingUpdatedByUserId` is read, an id) |
| Builder, versions | Each version's publisher by name | Accept (cheap: an id today), with words for the seeder (S9) |
| Builder, versions; the pinned line | Activities filed per version | Drop (Q3 decided; M5) |
| Builder, refusals | The field, section, move or directive a refusal names | Accept (Q6). The parsers throw one sentence; a location on the exception is the cost, per parser |
| Builder, JSON | The workflow and credit as typed | Q13: drop |
| Builder | B's warnings computed at save | Drop (B); false premise: the page computes them (`edit:1107–1119`) |
| Builder | The label of the field a `field:` actor names | Not DATA: read from the draft's schema, as the Spec says |
| Builder | Unavailable as a null read | Accept (unlisted, S10) |
| Builder, preview | The EPAs the type's own form offers (T291 item 6) | Accept (unlisted, S10); the build finds the read |

### Access control

| # | Board(s) | Finding | Sev |
|---|---|---|---|
| A1 | A1, A2.5 | **Q9 is a rule change on both sides.** `MayWrite` (`ActivityTypeAdminScope.cs:50–67`) and the guard's refusal must refuse a system-managed type to everyone, the Administrator included. Otherwise View is a page offering less than the command accepts, which DESIGN.md § Table system's T211 paragraph forbids in the other direction too. The list's `CanWrite` comes from the same `MayWrite` (`ListActivityTypesAdminQuery.cs:81`). | M3 |
| A2 | A5.5 | **Unavailable for an unknown id and an unreadable one alike** (a draft of another College's, 6.14a). The read rule is `CanReadAsync` (`GetActivityTypeEditorQuery.cs:85`); the list already leaves out what it refuses (`ListActivityTypesAdminQuery.cs:84`). Holds on A5.5's caption (Obs). | S6 |
| A3 | A1, A3.2 | **Scope offers exactly the writable scopes** (T300, D52). A3.2's Speciality / Sub-speciality radios and the read-out for Prof Mbatha follow `WritableScopes` (`edit:131–149`). Radios in place of today's select are a presentation change only. | — |
| A4 | A2 | **A reader is offered nothing to change** (DESIGN.md § Builder layout): no Save draft, Discard, Publish, Add, Up, Down or Delete; each section and field offers View under its row. Holds on every A2 frame (Obs). | — |

### Accessibility and the 390 boards

- **Good (Obs):**
  - one h1 per frame; tables captioned ("Moves, in words", "Credit rules, in words", "Published versions", "Activity
    types you can read or change");
  - every row action named for its row; the summary focused and linking;
  - refusals tied by `aria-describedby="k-err"` under the Key input;
  - the result alerts focused; the status line `role="status"`;
  - the 390 rows' actions at 44 px (`.field-row--narrow .btn { min-height: 44px }`);
  - the tabs at 44 px; no hex anywhere.
- **S1:** links, not buttons, on the list. **S2:** tabpanels. **S3:** headings.
- **Not checked here:** the Spec's "tab underline secondary on surface 4.86:1" and "danger stripe 5.95:1". They are
  token pairs, and round 2's ask should have them measured against `tokens.json` (Inf: plausible, since both tokens pass
  elsewhere).
- **A.7.14:** white on Publish 5.88:1 is unchanged; A keeps `btn-success`.

### DESIGN.md and flows 01–16's patterns

- **§ Builder layout must be rewritten, not amended:**
  - the tab bar becomes the tabs pattern (S2), with `.builder-tabs` replacing `.tab-bar` (`app.css:2133–2150`, used only
    here, Inf);
  - the outline (N2) replaces the stacked `.detail-card`s and h4/h5;
  - the editor opens in place (N3);
  - the preview at 390 sits behind a choice of two, where today the columns stack (`DESIGN.md:1812–1820`);
  - the JSON gains the read in words (N5);
  - the Publish panel (N7, M5).
- **§ Button system:** Delete outline, asking nothing (as flow 16's Remove on a rung); Discard draft… asking first (flow
  12, 14). **§ Page-level patterns:** Unavailable (S6); no Back to list (flows 12–16).
- **The classes:**
  - **N1–N3 and N5–N7 are A's.** Three of them duplicate shipped classes. N5's `.json-editor` is `textarea.form-control.font-mono`
    (`app.css:3243`, built, `edit:522`); use that. N7's `.publish-pinned` is an info `Alert`; use that. N6's
    `.version-strip` is a horizontal `dl.detail-list`; check whether a modifier will do.
  - **`.field-row.is-editing`** reuses the name of `tr.is-editing` (`app.css:453`) for the same stripe: consistent.
  - **`.tab-problem`** puts text on `--danger-bg`, with the text colour inherited. Round 2 measures it.
- **BRIEF § 11's lessons, checked:**
  - no gendered word on a page;
  - no existing class duplicated as NEW: three are (above);
  - step coverage (M7);
  - one word for one thing: "Details" for today's "Metadata" everywhere, and the runbook with it;
  - flow 15's 1280 column read and flow 16's every-column-per-role read apply to the list (§ 5).

## 8. New questions for the operator

Q1–Q12 are decided. These four are new; each has a recommendation.

13. **Q13. The JSON "as typed".** A, B and C keep the workflow and credit JSON as she typed it, PROPOSED DATA. Today a
    reload shows one line with keys reordered ("states" first, "initial_state" last).
    - **Why it comes back that way:** Save draft parses each payload and stores the parser's own serialisation
      (`ActivityType.SaveDraft`, `D/Activities/ActivityType.cs:104–117`). The column is `jsonb`, and PostgreSQL discards
      those bytes and re-renders its own text: keys sorted by length, then by name, one line (CLAUDE.md § Key technical
      choices).
    - **What "as typed" would cost:**
      - a second, plain-text column for each payload, for the draft, the live type and every version;
      - a migration, and every write path keeping the two in step;
      - an editor that can lie. The parser drops a property it does not know with no error (CLAUDE.md § Editing a seed
        folder: "a property with no Serialize half is dropped at publish"). The typed text would still show the property
        that the saved definition lost, and the words beside it would read the definition. The two would disagree with
        no error shown.
    - *Recommend:* drop it. Show the saved JSON laid out:
      - parse it, serialise it in the DSL's own key order (`WorkflowParser.Serialize`, `CreditRulesParser.Serialize`),
        and indent it, one rule per line, on load and after each save;
      - no store change: a `JsonWriterOptions { Indented = true }` overload on the two serialisers, or an indent pass
        over their output (Inf: `Utf8JsonWriter` takes it);
      - what she sees after a save is then exactly what was kept, in a stable order, and the read in words agrees with
        it by construction;
      - S13 drops the "Kept as you typed it" line.
14. **Q14. What Discard draft discards, and its words.** Q8's body ("Your changes since version 1 are removed. Version 1
    stays published.") is untrue in two ways (M4).
    - Save draft writes the details at once: Name, Description, Scope, Active and "This tool is"
      (`SaveActivityTypeDraftCommand.cs:113–119`). Discard reverts only the form, workflow and credit.
    - A type never published has no version 1, and Discard puts back the default draft (one Details section, a Title
      field, draft → submitted).

    *Recommend* two bodies, PROPOSED, with one line on the Details tab:
    - published: "Its form, workflow and credit rules go back to version ‹n›, which stays published. Its details (name,
      description, scope, This tool is, Active) were saved as you changed them, and stay.";
    - never published: "It has never been published. Its form, workflow and credit rules go back to the starting draft;
      its details stay.";
    - the line on the Details tab: "Saved with Save draft, not with Publish: a change here reaches everyone when you
      save." (S11). "This tool is" already says it of itself (`edit:153–154`).
15. **Q15. Unticking Active on a published type.** At Save draft it takes a type out of every registrar's filing picker
    (`ListActivityTypesQuery.cs:48`) and out of every reader's list (`ListActivityTypesAdminQuery.cs:84`), with no
    question. On the College's Mini-CEX that reaches every institution. D56 and the EPA page
    (`W/Admin/Epas/EpaEdit.razor:395–430`) ask first for the same kind of act.
    *Recommend:* Save draft with Active newly unticked on a published type asks first, as the EPA page does:
    "Deactivate Mini-CEX (Paediatrics)? Registrars are no longer offered it when filing. Activities already filed are
    not changed." Cancel and Deactivate (PROPOSED words; D56's pattern). This needs no Deactivate… button: Active
    stays the box, as on an EPA. Reactivating asks nothing.
16. **Q16. Publish with unsaved changes.** Publish publishes the saved draft (`edit:943–960`), but the warnings, and A5.1's
    dialog listing them, read the page's unsaved model (`edit:1107–1119`). After an edit and no save, the dialog would
    list changes the publish does not make, or miss some it does.
    *Recommend:* Publish saves the draft first, as it already does for a new type (`edit:925–928`). If the save is
    refused, it stops there with the refusal. Otherwise the warnings are recomputed from what was saved, it asks if
    there are any, and then it publishes. One press, as 1.30 counts it, and the dialog lists exactly what is published.

## 9. What round 2's ask must carry forward

- **The pick:** A, with M1–M7, S1–S14 and the nits in one pass. Draw it at fidelity with every state the brief lists
  (§ 1's STATES TO SHOW) and those § 1 here lists under "Not drawn", at 1280, 768 and 390. Add the 641–1115 range for
  Q5's Preview control, and the 900–1100 px sweep of the builder's columns and of the list.
- **The decisions, stated as decided, not as questions:** Q1–Q12, and the operator's answers to Q13–Q16. The ask must say
  so in its first lines. Round 1's ask listed them as open (`round-1-ask.txt:279`), and the canvas answered Q3, Q7, Q9,
  Q11 and Q12 against them.
- **Words round 1 settles, for round 2 to reuse:**
  - the list: Q7's columns, Q11's order and Q12's subtitles;
  - Q9's notice (both releases);
  - Q14's Discard bodies (both cases) and the Details line;
  - Q15's question;
  - the Unavailable triplet (S6);
  - the load-error sentences ("Could not load the activity types. Nothing has changed. …", "Could not load this
    activity type. …");
  - the refusal summary's prefix plus today's sentence;
  - "Version 1" / "Not published" / "Saved ‹date›";
  - Q3's line, "Activities filed under a version stay on it.".
- **The Publish panel** (M5), its fifth tab or panel drawn for: a first publish (C5.1's summary, "Nothing to compare");
  a republish with warnings (by label, T324); off, with its reason; after, with the version.
- **Paste the values the code holds** (S9), the seed JSON of the three folders the ask named, and `msf_cpsa`'s and
  `learner_feedback_cpsa`'s schemas, so the MSF read is drawn, not bracketed.
- **Get the files there.** The canvas saw no capture and no seed: its files held only `ds/wombat/`, and its request for
  the folder was refused. Before the send, put the 15 key captures and the nine seeds into the canvas's own project
  files (or attach them), and check that they are there. Have the Spec say first which arrived. Read `canvas.json`'s
  version before and after the send.
- **The stylesheet:** `f17r2.css` as a project file. Drop B's and C's N rows (N4, N8–N12). Use shipped classes where N5–N7
  duplicate them (§ 7, DESIGN.md).
- **The steps:** recount with one convention (a choice is a field, never also a press), state whether Done is needed
  (S4), and list every Expect and `states.md` row that changes (M7).
- **Checked here, so the ask need not re-derive it:**
  - the warnings are computed in the page, and print enums and keys;
  - Publish publishes the saved draft;
  - Save draft writes the details at once, and Discard reverts only the form, workflow and credit;
  - the draft's time and each version's date are read today, while the saver's and the publisher's names are ids;
  - neither DTO carries `SystemManaged`, and `MayWrite` judges scope only;
  - the editor query throws for an unknown or unreadable id;
  - the list is ordered by name and filters in memory;
  - the list's actions are links.
