# F17 Build and publish an activity type

**Who and why:** an activity type is one form, its workflow and its credit rules; every form a registrar files (flow
03) and an assessor rates (flow 04) is drawn from one at runtime (`src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor`).
InstitutionalAdmin Prof Nolwazi Mbatha (KGK) reads the College's twelve instruments, which she may not change (Act 1,
1.24–1.25), then builds KGK Teaching Session Log, has a duplicate key and a `field:` rule refused, and publishes version
1 (1.26–1.31). CollegeAdmin Dr Anton Kruger (CPSA) reads the list, opens the system-managed MSF type, and saves a draft
of a College type, CPSA Case Presentation Log (Act 6, 6.14a). The appendix opens the builder at 390 px (A.7.9) and
checks Publish's contrast (A.7.14). Pages: the list, and the builder (new and existing, read-only to a reader). No flow
has redesigned them: flow 01 gave them the shell and the " · Wombat" title, flow 03 the preview's renderer
(`725237ee`, the preview's section headings at h4), and nothing else has touched them since T300 (`1e154abe`,
2026-09-26). Every step capture is from the T394 replay (2026-11-05), on the shell as built; every `states/` capture is
from 2026-09-26, on the old frame.

Restated 2026-11-09 for T398 (the brief of 2026-09-26 predates flows 01–16: its Mode cited "§ 2.3 step 3" and § 4's
"three dense pages", both since rewritten; its CONSTRAINTS digest named no component after flow 01; its known problems
listed T322's contrast, done since T335 (A.7.14's Actual: white on Publish 5.88:1, the success alert 11.23:1), and
T329's builder half and T291 item 5, which T300 did; it said the College's `msf_cpsa` would be read-only "once T334
lands", which it has not; it put the preview "about 4,250 px down" at 390, now about 4,390; it said nothing of the field
rows at 1280, which do not fit their buttons, nor of the list at 390, which does not stack, nor of "v0"). Paths to
screenshots are under `design/baseline/` (gitignored). Code is cited at `d831ee7a`; `W/` is
`src/Wombat.Web/Components/Pages/`, `A/` is `src/Wombat.Application/Features/`, `Nav/` is `src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **W** in BRIEF.md § 8, and it stays W: the frame (four tabs, or one page of sections, or a stepped path for a new type; Q1), where the field editor opens (Q4), how the workflow and credit rules are read (Q2), where the preview sits on a phone (Q5) and what a publish pins (Q3) are structural. BRIEF.md § 4 names the builder one of the dense pages to lay out deliberately. |
| Frequency and stakes | Rare: a type is built once and republished seldom; the College's twelve are seeded. Medium-high: a type defines every form filed against it; a publish moves every new activity to the new version while those filed stay on theirs; a draft saved on a seeded type stops its seed updates (T103, D52). |
| Mode | **Wireframe first** (BRIEF.md § 2.3 step 4 and § 2.4; the old row's "§ 2.3 step 3" is that step, renumbered). One part: round 1 draws 2–3 STRUCTURES as wireframes covering both pages (Q1, Q2, Q4, Q5); the pick; then fidelity with every state; then one review round from the boards' text, the operator's decisions, and one correction round. Seed JSON is supplied (BRIEF.md § 3.1: the three seed folders). |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768, and the builder's two columns and any new table are swept from 900 to 1100 px against their container (§ 11, flows 06 and 15), and every column of the list read at 1280 for each role (§ 11, flow 16) |
| Variations | 2–3 structures (wireframes); the chosen one at fidelity after the pick |
| People | InstitutionalAdmin Prof Nolwazi Mbatha (Kgosi Kgari Teaching Hospital, KGK). CollegeAdmin Dr Anton Kruger (College of Paediatricians of South Africa, CPSA). Administrator `devadmin` ("Demo Administrator"), who writes every type and the Global ones alone. Seen through the result: Trainee Dr Anele Dlamini, offered the KGK log when filing (flow 03's). `D` is the replay day: 2026-11-05 in the T394 replay |
| Runbook steps | 11, pasted verbatim in § 8: 1.24–1.31 (`act-1-setup.md`), 6.14a (`act-6-catalogue.md`), A.7.9 and A.7.14 (`appendix-cross-cutting.md`). A.7.9 walks Home, Users, a user's page, Trainees and a profile (flows 06, 11, 12, 13's) before the builder; A.7.14 checks contrast on flows 01, 02, 07, 09 and 12's pages and the builder's Publish: only their builder parts are this flow's |
| Pages (`coverage.md` templates) | Two components, three templates. `/admin/activity-types` (`W/Admin/ActivityTypes/ActivityTypesList.razor`, 131 lines, `@page` at `:1`, `NationalCatalogueAccess` at `:4`: Administrator, CollegeAdmin, InstitutionalAdmin, `src/Wombat.Infrastructure/Identity/AuthorizationPolicies.cs:81–82`). `/admin/activity-types/new` and `/admin/activity-types/{ActivityTypeId:int}` (`W/Admin/ActivityTypes/ActivityTypeEdit.razor`, 1,142 lines, `@page` at `:1–2`, `NationalCatalogueAccess` at `:5`), with `W/Admin/ActivityTypes/BuilderModels.cs` (442: the schema model, its JSON and the publish warnings). Changed since 2026-09-26 only by `b347e11c` (flow 01, the shell) and `725237ee` (flow 03: the preview's `SectionHeadingLevel="4"`, `:508`). No page was renamed or moved. |
| Menus (`Nav/NavItems.cs`) | Activity types (`/admin/activity-types`, icon `clipboard-list`, `:112`) sits in the Administrator's "Catalogue" (Epas, Curricula, Activity types, Entrustment scales, `:145`), the InstitutionalAdmin's "Curriculum" (Curricula, EPAs, Activity types, Entrustment scales, `:153`) and the CollegeAdmin's one ungrouped group (Specialities, EPAs, Curricula, Activity types, Entrustment scales, `:161`). The builder lights it (`Nav/NavOwners.cs:152`, `CatalogueAuthors`). The College admin's Home card links it (`W/Dashboards/CollegeAdminDashboard.razor:17`, flow 16). |
| Components drawn at runtime | **The list:** `PageHeader` "Activity types" / "Manage draft and published activity-type definitions." with New activity type only where `CanCreate` (`ActivityTypesList.razor:11–19`); a search box filtering **on every keystroke** (`@bind:event="oninput"`, `:25`) in `.search-container`; `StatePanel` with `LoadError` = `RefusalText.Of(exception)` (`:103`) and no `OnRetry`, and **one** empty state for an empty catalogue and a search matching nothing ("No activity types" / "Create the first activity type to start the builder.", `:31`); `DataTable` captioned "Configured activity types" with **no `Stack` or `StackWide`** and no pager (`:32`): Name, Key (`<code>`), Scope (`FormatScope`, `:117–130`), Published (`v@(item.PublishedVersion)`, so "v0" for a type never published, `:49`), Draft ("Draft saved" / "None"), Status, and Edit or View named by `RowNames.Distinct` (`:56–65`). **The builder:** `PageHeader` titled from the address (`PageTitleText`, `:642–646`: "New activity type", "Activity type" while loading, "Edit {name}" to a writer, the name to a reader), subtitle "Build the form, workflow, and credit definition for this activity type." or "The form, workflow, and credit definition of this activity type." (`:648–650`), with **Back to list** (`:17`) and, where `CanWrite`, Save draft (`btn-primary`), Discard draft (`btn-outline`, **no dialog**, `:28`) and Publish (`btn-success`, its reason only in `title`, `:30`); one `ActionResult` holding the success and danger alerts, focused after a save on an existing type, a publish and a discard (`:38–48`, `:861`, `:957`, `:982`); the publish warnings as a warning `Alert` list (`:50–61`); `StatePanel` with `LoadError` = `RefusalText.Of` (`:818`), no `OnRetry`, and an empty state "Editor unavailable" (`:63`); the reader's standing info `Alert` (`Role=""`, `:78`, `ReadOnlyNotice` `:656–672`); the no-scope `Alert` on `/new` (`:64–71`); `.tab-bar` of four plain `<button>`s with only `.is-active` (`:82–87`); the reader's `dl.form-group` text views (`:89–118`, `:184–200`, `:331–418`) and `pre.code-block` JSON (`:512–518`, `:525–531`); the writer's `FormField`s (`Required` marks), the Scope picker over `WritableScopes` (`:131–149`), "This tool is" (`:153–162`), the form settings `select`s listing **field keys** (`:204–239`); `.builder-two-col` (`:244`) holding the Sections card (sections as `h4`, fields as `h5` with `<code>key</code> · {enum}`, `:274–301`; Edit, Up, Down and a **`btn-danger` Delete** on each, `:281–284`, `:307–310`; Add section, Add field) with the section and field editors **after the whole list** (`:419–502`), and the Live preview card (`h3`, `ActivityForm` with `SectionHeadingLevel="4"`, `:505–509`); Workflow and Credit as monospace `textarea`s (`:521–523`, `:534–536`). Headings: `h1`, then `h3` (Form settings, Sections, Live preview), `h4`, `h5`: **no `h2`**. Requests: `ListActivityTypesAdminQuery` (`A/Activities/Queries/ListActivityTypesAdmin/ListActivityTypesAdminQuery.cs`), `GetActivityTypeEditorQuery` (`A/Activities/Queries/GetActivityTypeEditor/GetActivityTypeEditorQuery.cs`; it already returns every version's number, date and publisher, `:206–212`, which no page shows), `SaveActivityTypeDraftCommand`, `PublishActivityTypeDraftCommand`, `DiscardActivityTypeDraftCommand`, `GetWbaToolsQuery`, `IActivityReferenceDataService.GetEntrustmentScaleOptionsAsync` (options keyed by scale id, `src/Wombat.Infrastructure/Activities/ActivityReferenceDataService.cs:439–447`); the rule is `A/Activities/ActivityTypeAdminScope.cs` (`MayWrite` `:50–67`, `WritableScopesAsync` `:89–134`, the guard's refusals `:273–280`). |
| Held | None. Every step capture is from the T394 replay (2026-11-05) on the shell as built. Every `states/` capture of these pages is from 2026-09-26, on the old frame (a Title Case sidebar with Logout, an email in the top bar): read their bodies only; `activity-type-edit--user-field`, `--duplicate-key`, `--field-rule`, `--credit`, `--published` (11:26–11:28), `--narrow` (16:30), `--publish-warnings` and `--discarded` (17:16) were taken that day before T300 landed (its re-captures are 22:14–22:20), so they also show a writer's page from before T300 |
| Depends on | Flow 01, built (`b347e11c`): the shell, the menus, the trail, not found, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again. Flow 03 (`725237ee`): `ActivityForm`, the preview's renderer, and `RefusalSummary` (one summary linking each refused field). Flow 11 (`69a17c70`): refusals under their field, focused; the required mark; `StackWide`. Flow 12 (`e53d4ae7`): a destructive action confirmed, its trigger an outline button; a detail page in one column of `<h2>` sections. Flow 14 (`e8eea1d2`): `ConfirmDialog` in action mode opens on Cancel; a page's own Unavailable state. Flow 15 (`c931d56f`): owner marks in words, "Set by the College.", one-shot result tokens across a page change (`CatalogueLinks`), fixed load-failure words. Flow 16 (`de4cc718`): no Back links beside the trail (Q9); Remove on a rung asks nothing and is an outline button (Q11); the College admin's Home card links Activity types (Q12). The design system is **version 49** |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 17`, then copy the nine seed files (tracked) into
   `design/upload/seeds/<folder>/` (BRIEF.md § 3.1: the script does not stage them). Open each screenshot. No capture
   here is an invitation, issued, resent or reset capture, and none shows a password or a one-time link; they show
   scenario names, keys and ids, which are the pages' own record.
2. **The canvas** is a new "Wombat 17" on the Wombat design system v49, with `tokens.css` written by hand (§ 11, flow
   09) and the mark copied in before round 1 (§ 2.3 step 4). `round-1-ask.txt` carries `‹MARK_URL›` where the mark's
   `/_blob/` URL goes, filled in before the send.
3. **Captures and seeds reach the canvas from the staged folder** (§ 11, flow 16): the ask tells it to say first, on
   its Spec board, which arrived. Then **send § 1 and § 8 as ONE message**
   (`design/flows/17-activity-type-builder/round-1-ask.txt`). Read `project/canvas.json`'s version before and after:
   a send can draw nothing. The ask tells the canvas to save its stylesheet as a project file (§ 11, flow 14).
4. **Answer § 6's operator questions first**; Q1, Q2 and Q8 are answered before round 1 (they decide what the
   structures draw). A question whose answer rests on an open task names it, and the task lands before the build when
   it changes data or rules (§ 11, flow 05). Pick a structure and say why in a sentence.
5. **Review the fidelity round from the boards' text** (`Artifact read`), as
   `design/flows/16-college-catalogue/round-2-review.md` did: one reviewer on the code (and the ask), one on the cast
   and the runbook, one on the design system. Put the decisions to the operator, then ask for one correction round (its
   own canvas if over about 200 boards, § 11, flow 16).
6. **Record every round** in `design/flows/17-activity-type-builder/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 17 — Build and publish an activity type: the list, a College instrument read, a type of our own built and published

This is flow 17 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns), 11 (invitation to admission), 12 (the institution's people), 13 (graduation
  and exit), 14 (data rights), 15 (an institution adopts the catalogue) and 16 (the College keeps the national
  catalogue) are designed and built; every page here sits inside the shell. No flow has redesigned the activity-type
  pages: they have the shell (flow 01) and flow 03's form renderer in the live preview, and nothing else since T300
  (2026-09-26). Every form a registrar files (flow 03) and an assessor rates (flow 04) is drawn from a type built here.
  This round draws 2–3 STRUCTURES as WIREFRAMES for the builder and its list (screens 1–5 below), each with its
  reasoning and its step count. I pick before fidelity.
  The runbook steps follow below in this same message. The brand mark is at the URL on the next line: use it, never a
  drawn disc.
  MARK: <the mark's /_blob/ URL in this canvas, filled in before the send>
  Every person, institution, College, key, name, version, count and id comes from this message and the steps below;
  invent none. Where a value is not given, write a bracket ‹…›.

THE SCREENSHOTS, THE SEED FILES AND THE STYLESHEET:
  - The key screenshots listed under ATTACHED are in design/upload/baseline/ (copies of design/baseline/), and the nine
    seed files in design/upload/seeds/. SAY ON YOUR SPEC BOARD, FIRST, whether they arrived (as attachments, or read
    from a folder), which you opened and which you did not. If any did not arrive, draw nothing from memory in its
    place: write a bracket (flows 06, 12, 13 and 16's lesson). The words in them are today's pages, defects included;
    they are not the target.
  - The seed files are three real types, each schema.json (the form), workflow.json (states and moves) and credit.json
    (credit directives): mini_cex_cpsa (a rated College instrument: 3 sections, 10 fields, 5 states, 4 moves, 1 credit
    directive), reflective_exercise_cpsa (an unrated College instrument: 2 sections, 9 fields, 4 states, 4 moves, credits
    nothing) and teaching_session (a Demo log: 1 section, 8 fields, 3 states, 2 moves, credits nothing). The builder
    and flow 03's renderer share their vocabulary; draw from them, not from invented JSON.
  - Write the round's shared stylesheet as a PROJECT FILE, project/f17r1.css (upload or write it as a file of the
    project, NOT as an asset, so the saved round holds it). Every board's head links ds/wombat/tokens.css,
    ds/wombat/components/bundle.css, then f17r1.css, in that order, and no /_blob/ stylesheet; a board's <style>
    holds body{margin:0} at most. Each NEW rule in f17r1.css names its row on the Spec's class table.

GOAL:
  An institution's administrator (Prof Mbatha, KGK) must be able to:
    - find a type in the list and see at a glance whose it is and whether she may change it (Edit) or only read it
      (View); find that a search matched nothing, as distinct from an empty catalogue;
    - read a College instrument she may not change: its details, its form (sections, fields, each field's settings,
      the scale a rating is bound to, by name), its workflow (who moves it from which state to which, and how much of
      the form each move checks) and its credit rules, under a standing notice saying whose it is;
    - start a type in the one scope she may write (Institution, KGK), build its form (sections; fields of 13 types; a
      User field naming a role; a unique key per field; the three form settings, chosen by field label), write its
      workflow and its credit rules, watch the live preview show the form a registrar will fill, and be told each
      refusal at the field or rule that caused it, keeping what she typed;
    - know before she publishes what the publish will change for activities already filed, and why Publish is not
      offered when it is not; publish version 1 and find the type in the list.
  The College's administrator (Dr Kruger, CPSA) must be able to write his College's types (Speciality and
  Sub-speciality of his College only), read the Demo College's, and be told that the two system-managed types (MSF and
  learner feedback) are written by their releases, not here. The Administrator (devadmin) writes every type, Global
  ones alone.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-11-05):
  - The field list prints each field as "key · enum": "epa_id · Epa", "objectives · LongText", while the Type picker
    says "EPA" and "Long text" (1.27-2). At 1280 the field rows are so narrow that Edit, Up, Down and Delete sit over
    the label: "Topi", "Audi", "Lear" (1.27-1, 1.27-2). The field and section editors open below the whole section
    list, far from the Edit pressed. Every section and field has a red filled Delete (1.27-2).
  - The Form settings pickers and their read-only text list field keys ("delivered_on", "overall_level"), not labels;
    their "None — …" options are cut off at 1280 (1.27-1). "Names a" lists roles by code name ("InstitutionalAdmin",
    "CommitteeMember") (1.27-3). A field bound to the College's ladder reads "seed:cpsa:scale:v11.1" to a reader
    (1.25-5) and "Select…" to a writer (code).
  - Workflow and Credit are raw JSON in a textarea (writer) or a code block (reader) (1.25-3, 1.28-1); nothing reads
    them out in words. A refusal from either is a danger alert at the page's top, naming no tab, while she may be on
    another (1.27-1, 1.28-1).
  - A reload or a publish hands the JSON back as one unbroken line, its keys reordered ("states" first, "initial_state"
    last), not as she typed it (1.25-3, 1.30-1): the store re-renders it.
  - The first Save draft of a new type moves to its own address and says nothing: no "Draft saved." (1.26-2, 6.14a).
  - Publish's reason when it is off ("Save a draft to publish.") is a tooltip only (1.30-1). The publish warnings are
    a list at the top that names fields by key and types by enum ("changes type from 'LongText' to 'Text'"; code).
    Discard draft discards at once, asking nothing (code).
  - The tabs (Metadata, Form, Workflow, Credit) are plain buttons: nothing tells a screen reader which is selected; at
    390 they wrap to two rows (A.7.9-6). The page has an h1 then h3, h4 and h5, and no h2 (code).
  - At 390 the editor and the preview stack, and the preview starts about 4,390 px down a 5,245 px page; each Delete
    is a full-width red bar (A.7.9-6). The list does not stack: at 390 only Name and Key show, and Scope, Published,
    Draft, Status and Edit or View are scrolled off inside the table's box (1.31-4).
  - The list's no-match search reads "No activity types" / "Create the first activity type to start the builder."
    though 23 types exist (1.31-3). An unpublished type reads "v0" under Published (6.14a). The list's subtitle,
    "Manage draft and published activity-type definitions.", and the builder's, "Build the form, workflow, and credit
    definition for this activity type.", speak in the system's words. The header keeps a "Back to list" beside the
    trail, which flows 12–16 dropped.
  - The two system-managed types (Multi-Source Feedback (Paediatrics), Learner Feedback (Paediatrics)) offer the College
    Edit, and msf_cpsa opens with every editor control and no notice (6.14a-2), though their releases write them.
  - The live preview's EPA picker lists the Demo College's EPA-001 above the 15 PAED EPAs; the real form does not.
  - A failed read prints the exception's text, with no Try again; an unknown id is a danger alert "The activity type
    could not be found." inside the page, not flow 14's Unavailable (code).

AUDIENCE:
  - InstitutionalAdmin: Prof Nolwazi Mbatha, Kgosi Kgari Teaching Hospital (KGK). At ease with forms, not with JSON;
    builds a type once and republishes it seldom; high stakes, since every form a registrar files comes from it.
  - CollegeAdmin: Dr Anton Kruger, College of Paediatricians of South Africa (CPSA). Writes the College's instruments.
  - Administrator: devadmin ("Demo Administrator"). Writes every type; a Global type is the Administrator's alone.
  - Seen through the result, on pages other flows built: Dr Anele Dlamini (Trainee) filing a KGK log (flow 03).
  - Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Who writes what (ActivityTypeAdminScope, T300, D52). An Administrator writes every type; a Global type is the
    Administrator's alone. An Institution type is its institution's: that institution's InstitutionalAdmin writes it.
    A Speciality or Sub-speciality type is the College's that owns the discipline: its CollegeAdmin writes it. Everyone
    else reads it if it is published and active (or Global), and is offered View, never Edit. A draft or inactive type
    a caller may not write is left out of their list, and its address reads "could not be found".
  - The read-only notice (standing, no alert role): "Set by the College that owns Paediatrics. You can read this
    activity type here, but not change it." ("Set by Kgosi Kgari Teaching Hospital." for an institution's type; "Set
    by a global administrator." for a Global one.) A reader is offered no Save draft, Discard draft, Publish, Add, Up,
    Down or Delete; each section and field offers View, which shows its settings as text.
  - Scope offers exactly the scopes the caller may write, and a new type starts in the first: an InstitutionalAdmin
    Institution with her own institution; a CollegeAdmin Speciality (his College's specialities) and Sub-speciality
    ("Paediatrics / Neonatology", "Paediatrics / Paediatrics"); an Administrator Global, Institution, Speciality and
    Sub-speciality. A caller with no scope at all reads "You have no scope to create an activity type in. …".
  - The list (/admin/activity-types): Name, Key, Scope ("Speciality · Paediatrics", "Institution · Kgosi Kgari Teaching
    Hospital"), Published ("v1"), Draft ("Draft saved" or "None"), Status (Active, Inactive), Edit or View named for
    the row ("View Mini-CEX (Paediatrics)"); New activity type only to a caller who may write somewhere; a search on
    name and key. The rows any non-Administrator sees: every Global and every national (Speciality or Sub-speciality)
    type of every College, and their own institution's.
  - Metadata: Key* (fixed once published), Name*, Scope* and its target*, "This tool is" (Not a WBA instrument, or one
    of the College's twelve instruments; help: "Choosing an instrument lets each curriculum item's tool list decide
    which EPAs this form can be filed against. 'Not a WBA instrument' leaves it unrestricted. Takes effect when you save
    the draft, not when you publish, and discarding the draft does not undo it. …"), Description, Active.
  - Form: sections (key*, title*), each holding fields: key*, label*, type*, help text, options (one per line; "value |
    Label" shows the label and stores the value), catalogue key, Required. The 13 types: Text, Long text, Number, Date,
    Choice, Multi-choice, Scale, User, EPA, Likert, Procedure reference, File, Signature. A User field has "Names a"
    (Assessor by default; also InstitutionalAdmin, Coordinator, CommitteeMember, Trainee; help "Who this field may name:
    someone at the trainee's institution holding this role.") in place of options and catalogue key. A Scale field is
    bound to an entrustment scale. Form settings: Encounter date field (a Date field, or "None — this form has no
    single encounter date"), Entrustment rating field (a Scale field, or "None — this form asserts no entrustment
    level"), EPA field (an EPA field, or "None — this form is about no single EPA"; help "The field naming the EPA this
    activity is evidence for. It is shown on activity lists, and credit rules that match on an EPA field must use this
    one.").
  - Workflow (JSON today): an initial state; states (key, label, terminal); moves (key, from one state or several, to,
    actor, validation, requires_note). The actor: subject (the registrar), creator (whoever filed it), role:<Role>,
    scope:<…>, field:<a User field> (the person that field names), combined with | (either) and + (both). Validation:
    all (the whole form, the default), owned (the mover's own fields), draft (formats only).
  - Credit (JSON today): counts_for directives, each matching a curriculum item through the EPA field, counting an
    amount, and judging the level reached from the rating field. {"counts_for": []} credits nothing. A type that credits
    nothing is held only to "the encounter date may not be after today".
  - Versions: Save draft keeps a draft beside the published version; Publish makes version n+1; Discard draft drops the
    draft. Activities already filed stay on the version they were filed under. A publish compares the draft with the
    published version and warns, before it is made: "Section 'X' will be removed.", "Field 'Audience' will be
    removed.", "Field 'X' changes type from 'LongText' to 'Text'.", "Field 'X' becomes required.", "Field 'X' will name
    a Coordinator instead of a Assessor. Activities already filed keep the person they named.", "This form will stop
    recording which field carries the encounter date.", "The encounter date moves from field 'a' to 'b'." The editor
    already reads each version's number, publish date and publisher; it does not read how many activities each holds.
  - Results and refusals, word for word: "Draft saved.", "Published version 1.", "Draft discarded."; "Field key 'topic'
    is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own
    key."; "Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a field: rule must name a
    User field."; to a caller who may not write: "You do not have permission to modify activity types in that
    speciality." (never shown on today's pages, since they offer nothing to save); an unknown id: "The activity type
    could not be found."
  - System-managed types: msf_cpsa and learner_feedback_cpsa are written by the MSF and learner-feedback releases; nobody
    is offered them when filing. Today the College may edit them (T334); the runbook already expects them read-only.
  - Seeded types: a seeded type with a draft saved by anyone but the seeder stops receiving seed updates until the
    draft is published or discarded, and once anyone else publishes it, for good (T103, D52). (A rule of the data, not
    a word on any page today.)

CONTENT (real scenario values from the T394 replay, D = 2026-11-05):
  - The list as Prof Mbatha reads it at 1.31: 23 rows, no two names alike: ten Demo types (ACAT, Case-based Discussion,
    DOPS, Journal Club, Mini-CEX, Procedure Log, QI Project, Reflective Note, Research Output, Teaching Session), each
    "Speciality · General Medicine", View; the twelve CPSA instruments, each "Speciality · Paediatrics", v1, None,
    Active, View: Mini-CEX (Paediatrics), DOPS (Paediatrics), Case-Based Discussion (Paediatrics), Direct Observation
    (Paediatrics), Clinical Case Analysis (Paediatrics), Random Case Analysis (Paediatrics), Chart-Stimulated Recall
    (Paediatrics), Reflective Exercise (Paediatrics), Clinical Audit (Paediatrics), Portfolio and Logbook Review
    (Paediatrics), Multi-Source Feedback (Paediatrics), Learner Feedback (Paediatrics); and KGK Teaching Session Log
    (kgk_teaching_log), "Institution · Kgosi Kgari Teaching Hospital", v1, None, Active, Edit.
  - As Dr Kruger reads it at 6.14a: 22 rows (no KGK type); Edit on the ten CPSA instruments, View on msf_cpsa and
    learner_feedback_cpsa (as the runbook expects) and on the ten Demo types; then 23 with CPSA Case Presentation Log
    (cpsa_case_presentation_log), "Speciality · Paediatrics", no published version, Draft saved, Active, Edit.
  - Mini-CEX (Paediatrics): Request (EPA, Assessor (User), Date observed, Clinical setting, Presenting problem, Case
    complexity), Entrustment ("Supervision required for this encounter", overall_level, a Scale field bound to the CPSA
    Paediatric Entrustment Scale v11.1, help "The supervision this activity actually required on this occasion - not a
    judgement of the trainee's worth."), Feedback (What was done well, Areas for development, Agreed plan); form
    settings observed_on, overall_level, epa_id; This tool is Mini-CEX; states draft, requested, completed, declined,
    cancelled; moves submit (draft → requested, subject|creator, owned), complete (requested → completed,
    field:assessor_user_id, all), decline (requested → declined, field:assessor_user_id, draft, a note required),
    cancel (draft or requested → cancelled, subject|creator, draft); one credit directive on epa_id, amount 1, level
    from overall_level.
  - KGK Teaching Session Log, as 1.27–1.29 leave it: one section "Teaching session" (session) with Topic (topic, Text,
    required), EPA (epa_id, EPA, required), Date delivered (delivered_on, Date, required), Audience (audience, Choice,
    required: Interns, Medical students, Nursing staff, Registrars), Learning objectives (objectives, Long text,
    required), Supervising consultant (supervisor_user_id, User, Names a Assessor); form settings delivered_on, none,
    epa_id; states Draft, Logged (terminal), Cancelled; moves log (draft → logged, subject|creator, all) and cancel
    (draft → cancelled, subject|creator, draft); credits nothing. Description "A registrar's log of a teaching session
    they delivered at KGK. Not rated, and credits nothing."
  - A new type's default draft: one section Details (details) with a required Title text field (title); workflow draft
    → submitted (submit, subject, all); {"counts_for": []}.
  - The preview's EPA picker should offer the 15 PAED EPAs ("PAED-001 — Title" to "PAED-015 — Title"), not the Demo
    EPA-001; its person picker offers nobody (whom it offers depends on the registrar filing it).
  - Ids as replayed (they change on every replay: draw them as given): mini_cex_cpsa 11, msf_cpsa 21,
    learner_feedback_cpsa 22, kgk_teaching_log 23, cpsa_case_presentation_log 24.

WHAT FLOW 17 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell, the menus (Activity types sits in the Administrator's Catalogue, the institution's Curriculum and
    the College admin's one group) and Access denied (flow 01); the form renderer the preview draws, ActivityForm,
    which is flow 03's and must look in the preview exactly as on the filing page; the rules above (who writes what;
    a new type starts in the first writable scope; activities stay on their version; the DSLs' grammar); the College
    admin's Home card and its Activity types link (flow 16).
  May, for these pages: the list's columns, search, stacking and empty states; the builder's frame (tabs or sections),
    where the field and section editors open, where the preview sits at each width, the form settings' pickers, read
    views of the workflow and the credit rules drawn from their JSON (the JSON stays the editor until T019-d and
    T019-e), the publish panel and its warnings, the results and refusals and where they show, Discard draft's
    question, Delete's look, the notices, Unavailable and loading. Mark PROPOSED any rule change (a role newly allowed,
    a new route, a new action, a new refusal, a new confirmation, data a page does not read today): those are mine.
    The runbook's Expects change with the words.
  Say which of these each structure changes, and why.

SCREENS, in order:
  1. /admin/activity-types — the list: Prof Mbatha's 23 rows (1.31), searched `cpsa` (1.24) and `KGK`, a search that
     matches nothing (`zzz`), a row with a draft in flight, Dr Kruger's list with Edit and View (6.14a), the
     Administrator's, loading, a load error, 390 px.
  2. /admin/activity-types/{ActivityTypeId:int} — a College instrument read by an institution: Mini-CEX (Paediatrics)
     (1.25): its details, its form with overall_level's settings open (the scale by name), its workflow and its credit
     rules read in words; a system-managed type, Multi-Source Feedback (Paediatrics), read by the College (6.14a).
  3. /admin/activity-types/new → /{ActivityTypeId:int} — a new type: the default draft (1.26), Scope as Prof Mbatha and
     as Dr Kruger (6.14a, Sub-speciality too), the first Save draft and its result; the form built (1.27): the section
     and field list, a field opened for editing where it sits, a User field's Names a, a duplicate key refused at both
     fields, the form settings by label, the live preview beside it.
  4. /admin/activity-types/{ActivityTypeId:int} — the workflow (1.28) and the credit rules (1.29): the JSON editor and
     a read of it in words (states; moves as from, to, who, how much it checks, note required); the field: rule refused
     at its move; a type that credits nothing; a directive that credits an EPA (Mini-CEX's, read).
  5. Publishing and after (1.30): Publish with its reason in text when it is off; the warnings shown before a publish
     ("Field 'Audience' will be removed."), and what stays pinned (activities already filed stay on version 1);
     "Published version 1." with the key locked; Discard draft asked and done ("Draft discarded."); an unknown id
     (Unavailable); a load error; loading; the builder at 390 px (A.7.9).

STEPS: 1.24, 1.25, 1.26, 1.27, 1.28, 1.29, 1.30, 1.31, 6.14a, A.7.9, A.7.14 (11). Pasted verbatim below (Role / Route /
  Do / Expect). In them, D is the day the story is replayed. Where an Expect quotes today's wording or layout, that is
  today's page, not a requirement on the new one. A.7.9 walks five pages other flows built before the builder, and
  A.7.14 checks contrast across several pages: for this flow only their builder parts apply (the builder at 390; the
  Publish button).

STATES TO SHOW:
  Loading for both pages (the header and a skeleton, no Save draft, Discard draft or Publish until the type has
  loaded); a load error (a sentence and Try again, never an exception's text, and no empty state under it); not found
  for an unknown id (flow 14's Unavailable, with no "New activity type" title and no Save draft); the list empty, and a
  search that matches nothing ('No activity types match "zzz".'), distinct; read-only (a College instrument; a
  system-managed type; an institution's type to the College is not listed at all); each refusal at the field or rule
  that caused it, with a summary that links to it, keeping what was typed; "Draft saved." (the first save too),
  "Published version 1.", "Draft discarded.", each taking the focus; Publish unavailable, with its reason in text; the
  publish warnings; Discard draft's question opening on Cancel; 390 px for both pages, nothing sideways; the data
  volumes: none (a new type's default draft), typical (KGK Teaching Session Log: 1 section, 6 fields, 3 states, 2 moves,
  no credit), heavy (Mini-CEX (Paediatrics): 3 sections, 10 fields, 5 states, 4 moves, 1 credit directive; and a type
  of 6 sections and 40 fields, long labels and a 12-option choice).

REQUIREMENTS FROM KNOWN DEFECTS:
  - T300 and D52 (built; keep): read-only for a caller who may not write, with its standing notice; Edit or View per
    row, named for the row; New activity type only where the caller may write somewhere; Scope offering exactly the
    writable scopes, a new type starting in the first; the title from the address, so an existing type never reads
    "New activity type"; nothing to save before the type has loaded or after a failed load.
  - T334: a system-managed type is read-only to everyone, the College included, with a notice that its release writes
    it; the list offers View on it (PROPOSED words; the runbook's 6.14a already expects it).
  - T324: types, roles and scopes by label everywhere: the field list ("EPA", "Long text"), Names a and its read view
    ("Committee member", not "CommitteeMember"), the publish warnings ("changes type from Long text to Text"; a field
    by its label, never its key); a key may show as a secondary, monospace detail. The form settings' pickers and read
    view list fields by label ("Date delivered", with delivered_on as detail).
  - T271: a Scale field bound to the College's ladder by its seed key shows "CPSA Paediatric Entrustment Scale v11.1",
    to a reader and in the writer's picker, never "seed:cpsa:scale:v11.1" or "Select…".
  - T291 item 3: the first Save draft of a new type reads "Draft saved." on the type's own address, with the focus on
    it. Item 6: the preview's EPA picker offers what the real form would (the 15 PAED EPAs, not EPA-001).
  - T326: a search that matches nothing says so ('No activity types match "zzz".'), with the search kept and a way to
    clear it; the empty catalogue keeps "Create the first activity type …" only for a caller who may create.
  - T280 (and DESIGN.md § Builder layout): the tab bar, if tabs stay, exposes which tab is selected (the tabs pattern,
    or aria-current); record the choice. One h1, then h2 in order: no h3 straight under the h1.
  - T264's kind (flow 12 and 16's rule): a destructive action's trigger is an outline button; Delete on a section or
    field changes nothing until Save draft, so it asks nothing (as flow 16's Remove on a rung) and is named for its
    target; Discard draft… asks first, naming the type and saying what stays ("Discard the draft of KGK Teaching Session
    Log?"; PROPOSED words), opening on Cancel.
  - T329 and flow 14: loading shows a skeleton, a load error a sentence and Try again, an unknown id Unavailable; no
    exception's text anywhere (T393's kind).
  - T323 (gone at 390 for the page; keep it): nothing scrolls sideways at 390 and every control is 44 px; the list
    stacks (StackWide), a block per type with its Edit or View; the builder's preview is reachable without scrolling
    past every field editor; the tabs, if kept, do not wrap into two rows. Sweep 900–1100 px: the builder's columns
    stand side by side only from 1116 px today (DESIGN.md § Builder layout), and at 1280 its field rows do not fit.
  - The header: no Back to list; the trail is the way back (flows 12, 13, 15, 16).
  - Publish's reason when it is unavailable is on-page text, not a tooltip ("Save a draft to publish." today).
  - Contrast (T322, gone; keep it): white on Publish 5.88:1, the success alert 11.23:1 (A.7.14's Actual).
  - Later, not built (show as a later phase if at all, marked NEW; never assume it): T019-b drag-and-drop reorder (Up
    and Down stay the keyboard path); T019-c nested and repeatable sections; T019-d a visual workflow editor; T019-e a
    visual credit-rules editor; T019-f visibility on several conditions; T019-g templates and copy from an existing
    type; T106 item 11 an input for who may write a field or section (editable_by), carried today but never shown;
    T179 "This tool is" taking effect at publish, not at save.

QUESTIONS THE DESIGN MUST ANSWER:
  1. (Mine) The frame: four tabs as today (Metadata, Form, Workflow, Credit), or one page of h2 sections (Details,
     Form, Workflow, Credit, Publish) with the preview beside Form, or a stepped path for a new type? Draw one structure
     each, with step counts for 1.26–1.30.
  2. (Mine) The workflow and the credit rules: a read in words (a table of moves: from, to, who, checks, note) beside or
     above the JSON editor, or a state diagram? The JSON stays the editor in every structure.
  3. (Mine) What a published version pins: where the version history (number, date, by whom) shows, and how the
     publish panel says what changes for activities already filed. A count of activities per version is DATA.
  4. Where the field and section editors open: under the row pressed, in a panel beside the list, or in a dialog?
  5. On a phone, where does the live preview go, and how does she move between the editor and it?
  6. A refusal from Save draft: where it shows when it names a field, a section, a move or a directive on another tab,
     and in what words (the refusals are one sentence each today: a field-level mapping is DATA).
  7. The list: its columns (whose it is, in words; Published "Not published" for v0; when the draft was saved), the
     search (applied live per keystroke today, a server round trip each: or Show, as flow 06's filters), and its rows at
     390.
  8. (Mine) Discard draft's question, and Publish: does a publish with warnings ask first, listing them?
  9. (Mine) A system-managed type's notice, and its row.
  10. (Mine) What the build folds in.

ASK:
  - Round 1: 2–3 STRUCTURES as wireframes, each covering screens 1–5 in order (1.24–1.31 as Prof Mbatha, 6.14a as Dr
    Kruger), each with its reasoning and its step count (presses + fields typed for 1.26–1.30 at 1280 and at 390), at
    1280 and 390 px. With them, two boards of their own: a Spec board (FIRST, whether the screenshots and seed files
    arrived; then every component each structure uses, NEW marked; the DESIGN.md rules each changes; everything
    PROPOSED; the data a page would need that it does not read today, marked DATA) and a Steps board (each structure's
    journeys counted, and which of the 11 Expects it would change). Stop and wait for my pick.
  - After the pick: the chosen structure at full fidelity with every state above, at 1280 and 390 px, every
    confirmation, and where the focus lands after each action, with a Spec board (every NEW and changed component, its
    classes, the DESIGN.md text each changes, the tokens, the DATA, and every message by kind with its words) and a
    Steps board.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a structure
    changes (§ Builder layout: the tab bar under PageHeader, the editor rail beside the preview stacking under a 46.5rem
    card, the JSON monospace; § Button system's destructive action; § Form system's refusals; § Table system's
    stacking; § Page-level patterns' Unavailable and Breadcrumbs).
  - The live preview is the real form renderer (flow 03's ActivityForm): a field in the preview must look exactly as it
    will on the filing page. Do not redraw it; place it.
  - Flag edge cases: a type with no sections; a section with no fields; a 40-field form; a field whose key is changed
    after publish (a removal and an addition); a form setting pointing at a field just deleted (kept visible so it can
    be cleared); a type whose draft was saved by someone else; a seeded type with a draft (it stops receiving seed
    updates); a College admin whose College has no speciality (no scope to create in); an inactive type; an
    unpublished type opened by someone who may not write it (not found); a 2,000-character description.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, the selected tab
    exposed, every row action named for its row ("Edit field Audience", "Delete section Teaching session"), focus
    after every action and confirmation, refusals tied to their fields (aria-describedby), targets ≥ 24 px (44 at 390).

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5; restated after the flow 01 pilot, 2026-09-27)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  the error page, /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side
  behaviour beyond a small script module; no live validation; the phone menu is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence (Source Sans 3 and Fraunces, OFL, ship).
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token (the design system's tokens.json) or a NEW token with its value; prefer the spacing scale
  xs 4, sm 8, md 16, lg 24, xl 32, 2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
The shell is designed and built (flow 01): design the page body inside it, not a new frame. The sidebar shows the
  acting role's navigation (one role at a time; access is the union of the roles held), grouped above eight links, with
  My progress and My data rights under a rule; the top bar holds the person's name and Sign out; below 641 px a phone
  bar with a CSS-only menu. A new page names its owning list (its nav item lights, its breadcrumb follows it) and its
  title "<Page> · Wombat", the same words as its h1 and its nav label, in sentence case.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, the trail from the
  owning list, an optional icon, header actions), DataTable (.clinic-table in .table-container, PagerControls),
  FormField / FormActions (.form-container, .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid,
  loading and load error), StatePanel (loading / empty / error), Skeleton, Alert (success / info / warning / danger,
  text on the tint), ActionResult, ConfirmDialog (native <dialog>), badges (five tints), Icon, TrajectoryChart
  (hand-drawn SVG, no chart library), ReferenceBlock.
  Since flow 02 also: the auth card, PasswordField with its Show toggle and PasswordRules. Since flow 03:
  RefusalSummary, DataTable's stacked rows (Stack). Since flow 05: ISO dates, the training year's wording, the ended
  record (R5). Since flow 06: the role Homes; filters applied with Show. Since flow 07: the container-query stack.
  Since flow 08: one action a row, a choice of a few kinds as radios (choice-group), a form page of its own, the
  pager's focus on the list's heading. Since flow 09: a page of its own per list row; a dialog with a field. Since flow
  10: a create page of its own reached from Home's header action. Since flow 11: the list is the page and its create
  form a page of its own; DataTable's StackWide (stacked from 900 px down); a role chosen from described radios; the
  required mark; only the chosen role's fields drawn; refusals under their field, focused. Since flow 12: a detail
  page in one column of <h2> sections with no details-grid (the user page); a form in a note panel opened in place
  (Add a role…); a destructive action confirmed, its trigger an outline button. Since flow 13: a record card naming
  an end and linking the record (FormerTraineeRecordCard); a page's header action named for the person; an Alert with
  its first sentence bold over a details list (Verify's answer); an end section of its own apart from Save. Since
  flow 14: a ConfirmDialog in action mode opens with the focus on Cancel; a page's own Unavailable state for a record
  that does not exist or is not the caller's; a textarea's character counter; the refusal first in aria-describedby.
  Since flow 15: the owner mark in words under a title (.muted .text-sm); "Set by the College." where a row offers no
  change; a one-shot result token that survives a page change and leaves the address; a list stacked to 1024 px when
  its columns need it; an item's page of its own with its record view; "Open" / "Held back".
  Since flow 16: a College's tree on one page, an <h2> per speciality over its table; no Back link beside the trail;
  Deactivate… asking first, naming its target and saying only what it does; Remove on an unsaved rung an outline
  button that asks nothing; Unavailable for a list or record typed with another's id.
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Designed by flow 02: session ended.
  Still to design where a flow meets them: field validation (invalid border plus a stripe, not colour alone; message
  under the field; the summary).
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. A date is ISO (YYYY-MM-DD) on every screen. An EPA is written "PAED-001 — Title" everywhere
  (EpaOptionLabel). A rung is written by its label as the College prints it ("3a", "5"); its rank (Order) appears only
  on a scale's own page.

ATTACHED: act-1/1.31-1-types-all.png, act-1/1.31-4-types-390.png, act-1/1.31-3-types-search-nothing.png,
  act-1/1.25-1-mini-cex-form.png, act-1/1.25-5-overall-level-field-editor.png, act-1/1.25-3-mini-cex-workflow.png,
  act-1/1.26-2-first-save-no-status.png, act-1/1.27-1-duplicate-key-refused.png, act-1/1.27-2-form-saved-preview.png,
  act-1/1.27-3-user-field-names-a.png, act-1/1.28-1-field-rule-refused.png, act-1/1.30-1-published-v1.png,
  act-6/6.14a-2-kruger-msf-cpsa-open.png, act-6/6.14a-3-new-type-scope.png and act-A/A.7.9-6-builder-390.png, all from
  2026-11-05 on the shell as built; and the nine seed files (seeds/mini_cex_cpsa/, seeds/reflective_exercise_cpsa/,
  seeds/teaching_session/: schema.json, workflow.json, credit.json). The pages' states/ captures are from 2026-09-26,
  an older frame (a Title Case sidebar with Logout, an email in the top bar): if you ask for one, read its body only.
  No attached capture shows a password.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 1.24 | `/admin/activity-types` | Prof Mbatha (InstitutionalAdmin) | `cpsa` leaves the twelve, each "Speciality · Paediatrics", v1, None, Active, View named for its row; cleared, 22; New activity type offered. **Search filters per keystroke; Key a column of its own; nothing says whose a type is but its scope** |
| 1.25 | `/admin/activity-types/{ActivityTypeId:int}` | Prof Mbatha | Mini-CEX (Paediatrics) read-only under the notice; its form settings, sections, overall_level's settings, metadata, workflow and credit. **The scale reads "seed:cpsa:scale:v11.1" (T271); the form settings and field list by key and enum (T324); workflow and credit only as JSON; the preview's EPA picker lists EPA-001 (T291 item 6)** |
| 1.26 | `/admin/activity-types` → `/admin/activity-types/new` → `/admin/activity-types/{ActivityTypeId:int}` | Prof Mbatha | the default draft; Scope Institution with KGK only; the first save accepted, the page at `/23` headed "Edit KGK Teaching Session Log". **No "Draft saved." (T291 item 3)** |
| 1.27 | `/admin/activity-types/{ActivityTypeId:int}` | Prof Mbatha | the 13 types; Names a; the duplicate refused naming both fields; the corrected save; the preview's six inputs. **The refusal at the page's top; field rows' labels under their buttons at 1280; the editor below the list; Names a in code names; pickers by key** |
| 1.28 | same | Prof Mbatha | the `field:` refusal, then "Draft saved.". **JSON only; the refusal at the top** |
| 1.29 | same | Prof Mbatha | `{"counts_for": []}`: credits nothing. **Nothing says so in words** |
| 1.30 | same | Prof Mbatha | "Published version 1."; Discard draft gone; Publish off; Key locked; no warnings. **Publish's reason a tooltip only** |
| 1.31 | `/admin/activity-types` | Prof Mbatha | 23 rows, hers alone Edit; `KGK` leaves hers. **`zzz` reads the empty-catalogue state (T326); at 390 only Name and Key show** |
| 6.14a | `/admin/activity-types` → `/{ActivityTypeId:int}` → list → `/new` → `/{ActivityTypeId:int}` → list | Dr Kruger (CollegeAdmin) | Edit on the College's instruments, View on the Demo types; msf_cpsa read-only (T334); Scope Speciality and Sub-speciality of his College; the draft saved; Prof Mbatha cannot find it. **msf_cpsa and learner_feedback_cpsa offer Edit and msf_cpsa opens fully editable with no notice (T334, observed); no "Draft saved."; the list reads "v0"** |
| A.7.9 | `/admin/activity-types/{ActivityTypeId:int}` at 390 px (after flows 06, 11–13's pages) | Prof Mbatha | the editor and preview in one column, the tab bar usable, nothing sideways. **The tabs wrap to two rows and expose no selected state (T280); the preview starts about 4,390 px down; each Delete a full-width red bar** |
| A.7.14 | `/admin/activity-types/{ActivityTypeId:int}` (among flows 01, 02, 07, 09, 12's pages) | Prof Mbatha | white on Publish 5.88:1 (T322, gone) |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in
`execution/knowledge/scenario-paediatrics/states.md` (the line numbers below are its rows on 2026-11-09). The step
captures are from the T394 replay (2026-11-05), on the shell as built; the `states/` captures are from 2026-09-26 (old
frame: read their bodies only; those marked "pre-T300" also show the writer's page before T300).

| Page | State | Screenshot | Who | Reached at (`states.md` line) |
|---|---|---|---|---|
| `/admin/activity-types` | Searched `cpsa`; all, 22 | `act-1/1.24-1-types-search-cpsa.png` (`states/activity-types-list--search.png`), `act-1/1.24-2-types-all.png` | Prof Mbatha | Step 1.24 (:902) |
| `/admin/activity-types` | All, 23; searched `KGK`; no match (**the empty-catalogue words**); narrow (**not stacked**) | `act-1/1.31-1-types-all.png` (`states/activity-types-list--all.png`), `act-1/1.31-2-types-search-kgk.png`, `act-1/1.31-3-types-search-nothing.png` (`states/activity-types-list--no-match.png`), `act-1/1.31-4-types-390.png` (`states/activity-types-list--narrow.png`) | Prof Mbatha | Step 1.31 (:904, :905, :907) |
| `/admin/activity-types` | A draft in the list; loading | `states/activity-types-list--draft.png`, `states/activity-types-list--loading.png` | Prof Mbatha | between 1.26 and 1.30; held read (:903, :906) |
| `/admin/activity-types` | The College's: Edit and View; with his draft (**"v0"**) | `act-6/6.14a-1-kruger-activity-types.png`, `act-6/6.14a-6-list-with-draft.png` | Dr Kruger | Step 6.14a |
| `/admin/activity-types/{ActivityTypeId:int}` | A College instrument, read: Form, Metadata, Workflow, Credit, a field's settings (**the raw seed key**) | `act-1/1.25-1-mini-cex-form.png` (`states/activity-type-edit--college-instrument.png`), `act-1/1.25-2-mini-cex-metadata.png`, `act-1/1.25-3-mini-cex-workflow.png`, `act-1/1.25-4-mini-cex-credit.png`, `act-1/1.25-5-overall-level-field-editor.png` | Prof Mbatha | Step 1.25 (:908) |
| `/admin/activity-types/{ActivityTypeId:int}` | A system-managed type, to the College (**fully editable, no notice: T334**) | `act-6/6.14a-2-kruger-msf-cpsa-open.png` | Dr Kruger | Step 6.14a |
| `/admin/activity-types/new` | The default draft; Metadata filled | `act-1/1.26-1-new-type-default.png` (`states/activity-type-edit--new.png`), `act-1/1.26-3-metadata-filled.png` | Prof Mbatha | Step 1.26 (:909) |
| `/admin/activity-types/new` | The College's Scope; Sub-speciality's targets; the default draft's preview | `act-6/6.14a-3-new-type-scope.png`, `act-6/6.14a-4-sub-speciality-targets.png`, `act-6/6.14a-3b-new-type-form-preview.png` | Dr Kruger | Step 6.14a |
| `/admin/activity-types/{ActivityTypeId:int}` | The first save (**no "Draft saved."**) | `act-1/1.26-2-first-save-no-status.png`, `act-6/6.14a-5-draft-saved.png` | Prof Mbatha, Dr Kruger | Steps 1.26, 6.14a (:910 reads "Draft saved." in its words; the capture shows none) |
| `/admin/activity-types/{ActivityTypeId:int}` | Duplicate key refused; saved with the preview; a User field | `act-1/1.27-1-duplicate-key-refused.png` (`states/activity-type-edit--duplicate-key.png`, pre-T300), `act-1/1.27-2-form-saved-preview.png`, `act-1/1.27-3-user-field-names-a.png` (`states/activity-type-edit--user-field.png`, pre-T300) | Prof Mbatha | Step 1.27 (:911, :912) |
| `/admin/activity-types/{ActivityTypeId:int}` | `field:` rule refused; workflow saved; credit | `act-1/1.28-1-field-rule-refused.png` (`states/activity-type-edit--field-rule.png`, pre-T300), `act-1/1.28-2-workflow-saved.png`, `act-1/1.29-1-credit-tab.png` (`states/activity-type-edit--credit.png`, pre-T300) | Prof Mbatha | Steps 1.28, 1.29 (:913, :914) |
| `/admin/activity-types/{ActivityTypeId:int}` | Published; the key locked | `act-1/1.30-1-published-v1.png` (`states/activity-type-edit--published.png`, pre-T300), `act-1/1.30-2-metadata-key-locked.png` | Prof Mbatha | Step 1.30 (:915) |
| `/admin/activity-types/{ActivityTypeId:int}` | Publish warnings; draft discarded (**no question**) | `states/activity-type-edit--publish-warnings.png`, `states/activity-type-edit--discarded.png` (both pre-T300) | Prof Mbatha | scratch, post-appendix (:916, :917) |
| `/admin/activity-types/{ActivityTypeId:int}` | Unknown id (**an alert, not Unavailable**); loading | `states/activity-type-edit--not-found.png`, `states/activity-type-edit--loading.png` | Prof Mbatha | typed; held read (:918, :919) |
| `/admin/activity-types/{ActivityTypeId:int}` | Narrow; a type Prof Mbatha may not open (his draft) | `act-A/A.7.9-6-builder-390.png` (`states/activity-type-edit--narrow.png`, pre-T300), `act-6/6.14a-7-mbatha-cannot-find-draft.png` | Prof Mbatha | Step A.7.9 (:920); Step 6.14a |
| `/admin/activity-types/{ActivityTypeId:int}` | Publish, not pressed (contrast) | `act-A/A.7.14-6-builder-publish.png` | Prof Mbatha | Step A.7.14 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Never built: Discard draft's question; Publish's reason as text; a system-managed type read-only (T334); a no-match
  search distinct from an empty catalogue (T326); the scale by name (T271); a refusal at its field; Unavailable for an
  unknown id; a version history; a read of the workflow and credit in words.
- Unplayed by the story: Discard draft (`coverage.md:215`: the story publishes KGK's draft and keeps the College's); a
  republish with warnings (only on a scratch copy, `:916`); the Administrator's builder (no step plays it; devadmin's
  Scope offers Global, Institution, Speciality and Sub-speciality, code); a College admin with no scope to create in
  (`ActivityTypeEdit.razor:64–71`, code); a load error (no capture: the list's and the builder's `StatePanel` would
  print the exception's text, code).

**Belongs to other flows:** A.7.9's Home, Users, a user's page, Trainees and a profile (`act-A/A.7.9-1` to
`A.7.9-5d`, flows 06, 11, 12, 13); A.7.14's sign-in, Home and nav, alerts, the lock-out dialog and the badges
(`act-A/A.7.14-1` to `-5`, `-7` to `-9`, flows 01, 02, 07, 09, 12). The filing page that draws a published type (flow
03), and the College admin's Home card (flow 16).

## 4. Attach

Paths are relative to `design/baseline/`. Every file below was opened on 2026-11-09 for this brief. None shows a
password or a one-time link.

**Key screenshots (attach these first, with § 1):**
1. `act-1/1.31-1-types-all.png`: the list, 23 rows, her one Edit
2. `act-1/1.31-4-types-390.png`: the list at 390, Name and Key only
3. `act-1/1.31-3-types-search-nothing.png`: `zzz`, the empty-catalogue words
4. `act-1/1.25-1-mini-cex-form.png`: a College instrument read, under the notice; form settings and fields by key
5. `act-1/1.25-5-overall-level-field-editor.png`: a field's settings, "seed:cpsa:scale:v11.1"
6. `act-1/1.25-3-mini-cex-workflow.png`: the workflow as a code block
7. `act-1/1.26-2-first-save-no-status.png`: the first save, no result
8. `act-1/1.27-1-duplicate-key-refused.png`: the refusal at the top; labels under their buttons
9. `act-1/1.27-2-form-saved-preview.png`: the form, the editor below the list, the preview
10. `act-1/1.27-3-user-field-names-a.png`: Names a in code names
11. `act-1/1.28-1-field-rule-refused.png`: the workflow JSON and its refusal
12. `act-1/1.30-1-published-v1.png`: published, Publish off with a tooltip
13. `act-6/6.14a-2-kruger-msf-cpsa-open.png`: the MSF type editable to the College
14. `act-6/6.14a-3-new-type-scope.png`: the College's Scope
15. `act-A/A.7.9-6-builder-390.png`: the builder at 390

**As the chat asks:** `act-1/1.24-1-types-search-cpsa.png`, `act-1/1.25-2-mini-cex-metadata.png`,
`act-1/1.25-4-mini-cex-credit.png`, `act-1/1.26-1-new-type-default.png`, `act-1/1.28-2-workflow-saved.png`,
`act-1/1.29-1-credit-tab.png`, `act-1/1.30-2-metadata-key-locked.png`, `act-6/6.14a-1-kruger-activity-types.png`,
`act-6/6.14a-6-list-with-draft.png`, `act-A/A.7.14-6-builder-publish.png`, and (old frame, read their bodies only)
`states/activity-type-edit--publish-warnings.png`, `states/activity-type-edit--discarded.png`,
`states/activity-type-edit--not-found.png`, `states/activity-type-edit--loading.png`,
`states/activity-types-list--loading.png`.

That is 15 key files and 15 more: 30. With them, as text, the nine seed files (tracked) under
`src/Wombat.Infrastructure/Activities/Seeds/`: `mini_cex_cpsa/`, `reflective_exercise_cpsa/` and `teaching_session/`,
each `schema.json`, `workflow.json` and `credit.json`, copied to `design/upload/seeds/`.

**Do not attach:** the duplicates of a step capture, all on the old frame (`states/activity-types-list--all.png`, `states/activity-types-list--search.png`, `states/activity-types-list--no-match.png`, `states/activity-types-list--narrow.png`, `states/activity-types-list--draft.png`, `states/activity-type-edit--college-instrument.png`, `states/activity-type-edit--new.png`, `states/activity-type-edit--metadata.png`, `states/activity-type-edit--user-field.png`, `states/activity-type-edit--duplicate-key.png`, `states/activity-type-edit--field-rule.png`, `states/activity-type-edit--credit.png`, `states/activity-type-edit--published.png`, `states/activity-type-edit--narrow.png`); the step captures the key ones cover (`act-1/1.24-2-types-all.png`, `act-1/1.26-3-metadata-filled.png`, `act-1/1.31-2-types-search-kgk.png`, `act-6/6.14a-3b-new-type-form-preview.png`, `act-6/6.14a-4-sub-speciality-targets.png`, `act-6/6.14a-5-draft-saved.png`, `act-6/6.14a-7-mbatha-cannot-find-draft.png`); other flows' pages (`act-A/A.7.9-1-home-390.png` to `act-A/A.7.9-5d-deactivate-dialog-390.png`, `act-A/A.7.14-1-login.png` to `act-A/A.7.14-5-danger-button.png`, `act-A/A.7.14-7-review-7-badges.png` to `act-A/A.7.14-9-decisions-due-badges.png`).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `d831ee7a`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T300 and D52** (done, `1e154abe`; BRIEF § 7 B11) | **Built; keep.** Read-only mode with its standing notice; Edit or View per row; New activity type only where the caller may write; Scope exactly the writable scopes, a new type in the first; the title from the address ("Activity type" while loading and on an unknown id, observed in `states/…--loading`, `--not-found`); nothing to save before the load or after a failure; the College admin's menu item (observed in every 6.14a capture). The redesign keeps all of it. | Steps 1.24–1.26, 6.14a |
| **T334** (P2, queued) | Q9. **Still true:** `ActivityTypeAdminScope.MayWrite` (`:50–67`) judges scope only; nothing in the builder or its commands reads `ActivityType.SystemManaged`, so the College is offered Edit on `msf_cpsa` and `learner_feedback_cpsa`, and `msf_cpsa` opens with Save draft, Publish and every editor control and no notice (observed 6.14a: "F-6.14ab, T334 (still)"). A rule change: the operator's. | `act-6/6.14a-2…`, `6.14a-1…` |
| **T324** (P3, queued; the builder half) | Q10. **Still true:** the field list prints `@field.value.Type` (`ActivityTypeEdit.razor:301`; observed 1.25, 1.27: "epa_id · Epa", "objectives · LongText"), though `FieldTypeLabel` exists (`:1131–1141`). **The same kind, not in T324:** Names a lists `WombatRoles` raw ("InstitutionalAdmin", "CommitteeMember", `:467`; observed 1.27's Actual), and its read view prints the stored role (`:375`); the publish warnings print enum names ("changes type from 'LongText' to 'Text'", `BuilderModels.cs:209`), raw roles (`:226–227`) and field **keys** for the form settings (`:359–369`); the form settings' pickers and read view list keys (`ActivityTypeEdit.razor:189–197`, `:211`, `:222`, `:236`; observed 1.25-1, 1.27-2). **Gone:** the Scope picker's "SubSpeciality" (`ScopeLabel`, `:674–681`). | `act-1/1.27-2…`, `act-1/1.27-3…` |
| **T271** (P3, queued; item 2) | **Still true:** `GetEntrustmentScaleOptionsAsync` keys options by scale id (`ActivityReferenceDataService.cs:445`), so a field bound by seed key reads "Select…" to a writer (`ActivityTypeEdit.razor:487–493`, code; the College would see it on Mini-CEX) and the raw key to a reader (`ScaleText`, `:702–704`; observed 1.25: "F-1.25b, T271 (still)"). Item 1 (the race) is a lock, no screen. | `act-1/1.25-5…` |
| **T291** (P3, queued; items 3 and 6; item 5 done by T300) | **Item 3, still true:** a first save navigates with `forceLoad: false` (`:900–903`) and `OnParametersSetAsync` clears `_status` (`:802`), so the type's own address reads nothing (observed 1.26 at 150 ms to 3 s, 6.14a). **Item 6, still true:** the preview's EPA picker lists the Demo EPA-001 above the 15 PAED EPAs (observed 1.25, 1.27). **Item 5, gone:** every Demo row reads "Speciality · General Medicine" (observed 1.24, 1.31). | `act-1/1.26-2…`, `act-6/6.14a-5…`, `act-1/1.27-2…` |
| **T326** (P3, queued; the list half) | Q7. **Still true:** one `StatePanel` empty state for an empty catalogue and a search matching nothing (`ActivityTypesList.razor:31`); `zzz` reads "No activity types" / "Create the first activity type to start the builder." (observed 1.31: "F-1.31b, T326 (still)"). The `DataTable`'s own Empty, "No activity types match the current filter." (`:69–73`), is never reached. | `act-1/1.31-3…` |
| **T280** (P3, queued; its builder item) | Q1. **Still true:** the tabs are plain buttons with `.is-active` only (`:82–87`; observed A.7.9: "no role="tab", aria-selected or aria-pressed", "F-A.7.9a, T280 (still)"). DESIGN.md § Builder layout is silent on ARIA (`DESIGN.md:1799–1836`). | `act-A/A.7.9-6…` |
| **T264's kind** (T264 names no builder control) | Q8. **Still true (code):** section and field Delete are `btn-danger btn-sm` (`:284`, `:310`), observed red on every row (1.27-2) and as full-width red bars at 390 (A.7.9-6); they change nothing until Save draft. Discard draft drops the draft at once with no dialog (`:28`, `:965–992`). Flow 16's model: Remove on a rung, unsaved until Save, an outline button with no question (Q11); a stored change behind a `ConfirmDialog` opening on Cancel (flow 14). | `act-1/1.27-2…`, `states/…--discarded.png` |
| **T329** (P2, queued; the builder half done by T300) | **Gone:** the builder titled "New activity type" while loading or on an unknown id, and Save draft offered then (T300; `:642–646`, `:21`). **The same kind, still true:** both pages' `LoadError` is `RefusalText.Of(exception)` (`ActivityTypesList.razor:103`, `ActivityTypeEdit.razor:818`), which prints any non-validation exception's text (`src/Wombat.Web/Components/Shared/RefusalText.cs:42`), with no Try again (no `OnRetry`); an unknown id is that same alert ("The activity type could not be found.", `A/…/GetActivityTypeEditorQuery.cs:83`, `:87`), not flow 14's Unavailable (code; `states/…--not-found.png` shows it on the old frame). | `states/activity-type-edit--not-found.png`, `--loading.png` |
| **T322** (done) | **Gone:** white on Publish 5.88:1, the success alert 11.23:1 (observed A.7.14's Actual, 2026-11-05). Keep. | `act-A/A.7.14-6…` |
| **T323** (P3, queued; no builder half) and **T266** (done) | **Gone:** sideways scroll on the builder at 390 (observed A.7.9: "390 in 390") and from 901 to 1115 px (T266: the columns stack by the card's width, `DESIGN.md:1812–1820`). **The same kind, still true:** at 390 the preview starts about 4,390 px down a 5,245 px page, behind every field row and both editors, and the tabs wrap to two rows (observed A.7.9-6). The list has no `Stack` or `StackWide` (`ActivityTypesList.razor:32`): at 390 only Name and Key show, the rest scrolled off inside the table's box with Edit or View out of view (observed `1.31-4`; the Actual's "390 px wide with no sideways scroll" measured the page, not the table). | `act-A/A.7.9-6…`, `act-1/1.31-4…` |
| **New: the field rows do not fit at 1280** (observed; unfiled) | At 1280 the columns stand side by side (from 1116 px), the Sections rail at its 320 px minimum; each field row's Edit, Up, Down and Delete sit over the row's title, which shows as "Topi", "Audi", "Lear", "Supe" (observed 1.27-1, 1.27-2; the reader's View-only rows fit, 1.25-5). The form settings' `select`s cut off "None — this form has no single enc…" (observed 1.27-1). | `act-1/1.27-1…`, `act-1/1.27-2…` |
| **New: the editors open far from the row** (observed; the old brief's, unfiled) | The section and field editors render after the whole section list (`:419–502`), so an Edit on the first field opens an editor below the last; at 1280 the field editor of 1.27-2 starts about 2,600 px down, and at 390 below every row (observed). Opening a section selects its first field too (`SelectSection`, `:1015–1019`). | `act-1/1.27-2…`, `act-A/A.7.9-6…` |
| **New: a refusal at the page's top, naming no tab** (observed) | Every Save draft refusal is one sentence in the `ActionResult`'s danger alert above the tabs (`:44–47`), not at the field, section or move it names; a workflow refusal shows on whatever tab is open (observed 1.27-1, 1.28-1). The commands refuse in one sentence each (the schema and workflow parsers' messages); tying one to its field is DATA (inferred). | `act-1/1.27-1…`, `act-1/1.28-1…` |
| **New: the JSON comes back reflowed** (observed) | After a reload or a publish the Workflow and Credit JSON show as one unbroken line, keys reordered ("states" first, "initial_state" last), not as typed (observed 1.25-3, 1.25-4, 1.30-1; 1.28-2 kept her layout only because the page had not reloaded). The store is `jsonb`, which re-renders what it keeps (CLAUDE.md § Key technical choices); the editor shows the stored text as it is (`ActivityTypeEdit.razor:836–837`). A read in words (Q2), or the editor pretty-printing what it loads, answers it. | `act-1/1.25-3…`, `act-1/1.30-1…` |
| **New: Publish's reason is a tooltip** (observed; the old brief's, unfiled) | "Save a draft to publish." is the disabled button's `title` (`:30`; observed 1.30: "carried as the button's title (tooltip), not on-page text"). | `act-1/1.30-1…` |
| **New: headings skip h2** (code) | `h1` (PageHeader), then `h3` Form settings, Sections and Live preview, `h4` sections and editors, `h5` fields (`:180`, `:248`, `:274`, `:300`, `:336`, `:423`, `:439`, `:506`); flow 12's rule is one column of `<h2>` sections. | — (code) |
| **New: "v0", Back to list, and the subtitles** (observed; code) | An unpublished type reads "v0" under Published (`ActivityTypesList.razor:49`; observed 6.14a's Actual). The header keeps Back to list (`ActivityTypeEdit.razor:17`), which flows 12, 13, 15 and 16 dropped for the trail. The subtitles speak the system's words ("Manage draft and published activity-type definitions.", "Build the form, workflow, and credit definition…"), and a reader's Sections card "Build the schema visually and keep the preview in sync." to a writer (`:249`). | `act-1/1.31-1…`, `act-6/6.14a-6…` |
| **New: the list's search filters per keystroke** (code) | `@bind:event="oninput"` (`ActivityTypesList.razor:25`): each key a server round trip (BRIEF § 5.1 asks to flag it); flow 06's filters apply on Show. | — (code) |
| **New: whose a type is, beyond its scope** (code) | Every non-Administrator's list holds every College's national types and every Global one, not only those of the curricula her institution adopted ("Adoption-based narrowing of national types arrives in phase 4", `ListActivityTypesAdminQuery.cs:37–45`): Prof Mbatha reads the Demo College's ten. Nothing on a row says whose it is but the scope's words (Q7, Q12). | `act-1/1.31-1…` |
| **New: no version history** (code) | The editor returns each version's number, publish date and publisher (`GetActivityTypeEditorQuery.cs:206–212`) and the list the draft's save time (`StagingUpdatedOn`, `ListActivityTypesAdminQuery.cs:61`); no page shows either. How many activities each version holds is not read (DATA). The old brief's question 3. | — (code) |
| **T179** (P3, queued) | "This tool is" takes effect when a draft is saved, not at publish; its help says so (`:154`). The design must not show it as pinned by a version. | — (code) |
| **T106 item 11, T019-b, -c, -d, -e, -f, -g** (queued; BRIEF § 7 B10) | The builder backlog: an `editable_by` input (carried through `BuilderModels.cs`, never shown); drag-and-drop reorder; nested and repeatable sections; visual workflow and credit editors; visibility on several conditions; templates and copy. The design may show them as later and NEW; never assumed. | the task files |
| **T352 item 4** (queued; builder-authored keys) | A move's label is derived from its key (`return_to_A` reads "Return to a"); an author's keys become the labels a registrar reads. A read view of the workflow should show the label the activity page will use. | — (code) |

**Dropped from the old brief (done since, or decided):**
- T322's contrast (T335's tokens; observed A.7.14). T329's builder title and Save draft while loading (T300). T291
  item 5 (T300). T190's page title (done). T266's sideways scroll at 901–1115 px (done).
- "Publish is green with white text at 2.87:1, and the "Draft saved." alert is 2.55:1": no longer so.
- The old CONSTRAINTS digest (pre-flow 02) and its components list; restated from flow 16's with what flow 16 added.

## 6. Questions for the operator

Each with a recommendation. The canvas answers Q4–Q7 in its structures; Q1–Q3 and Q8–Q12 are the operator's, and **Q1,
Q2 and Q8 are answered before round 1** (they decide what the structures draw and what the round may show as built).

**Decided 2026-11-10:** the operator took Q1–Q12 as recommended (sent the round-1 ask and said "done").

1. **Q1. (The operator's; the old brief's question 1; must be answered before round 1) The frame.** Today: four tabs
   (Metadata, Form, Workflow, Credit), opening on Form, the preview beside Form only. *Recommendation:* round 1 draws
   three structures with their step counts: (a) the tabs kept, as a real tabs pattern (`role="tablist"`,
   `aria-selected`, arrow keys), Details first for a new type, and a Publish panel of its own; (b) one page of `<h2>`
   sections (Details, Form, Workflow, Credit, Publish) with a jump list, the preview a column beside Form; (c) a stepped
   path for a new type (Details → Form → Workflow → Credit → Publish) over (a) or (b) for an existing one. Lean: (a),
   because Mini-CEX's form alone is about 3,800 px today and one page would be the longest in the product; the tabs'
   ARIA is T280's.
2. **Q2. (The operator's; must be answered before round 1) JSON stays the editor.** *Recommendation:* yes: Workflow and
   Credit stay monospace JSON (DESIGN.md:447–450's rule; T019-d and T019-e are not built), with a read in words beside or
   above it drawn from the saved JSON: the states (initial, terminal), a table of moves (from, to, who in words: "the
   registrar or whoever filed it", "the person named in Assessor", how much it checks: "the whole form", "their own
   fields", "formats only", a note required), and the credit as sentences ("Credits nothing." / "Counts 1 towards the
   curriculum item for the EPA in EPA, at the level Supervision required records."). The read is the reader's whole
   view; no state diagram.
3. **Q3. (The operator's; the old brief's question 3) What a version pins.** *Recommendation:* a Versions section (or
   panel) listing each version's number, publish date (ISO, SAST) and publisher, which the editor already reads, and
   the line "Activities filed under a version stay on it."; the publish panel shows the warnings in labels before the
   press. A count of activities per version is DATA (PROPOSED), and left out unless you want it.
4. **Q4. (Canvas) Where the field and section editors open.** *Recommendation:* in place, under the row pressed, one at
   a time, its Edit becoming Done; the focus to its first input and back to the row's Edit after; never below the
   whole list.
5. **Q5. (Canvas; the old brief's question 4) The preview on a phone.** *Recommendation:* at 390, a "Preview" control
   in the Form section's header (a section of its own, or a tab of the Form) rather than below every row; between 641
   and 1115 px the same; the preview stays flow 03's renderer.
6. **Q6. (Canvas) A refusal from Save draft.** *Recommendation:* flow 03's `RefusalSummary` at the top, linking to the
   field, section, move or directive it names and switching to its tab; the duplicate key under both Key inputs;
   PROPOSED: the commands return which field or move each refusal names (DATA). Today's sentences kept word for word.
7. **Q7. (Canvas; T326) The list.** *Recommendation:* Name with the key under it in monospace; "Set by" in words ("CPSA",
   "Kgosi Kgari Teaching Hospital", "Demo College", "the platform") in place of Scope's code-shaped "Speciality ·
   Paediatrics", with the scope as detail; Published "Version 1" or "Not published"; Draft "Saved ‹date›" (the list
   reads it) or "None"; Status; Edit or View. StackWide at 390. The search applied with Show (flow 06), and
   'No activity types match "zzz".' with Clear.
8. **Q8. (The operator's; T264's kind; must be answered before round 1) Discard, Delete and Publish.** *Recommendation:*
   Delete on a section or field: an outline button named for its target, no question (nothing is saved until Save
   draft; flow 16's Q11). Discard draft…: a `ConfirmDialog` opening on Cancel, "Discard the draft of KGK Teaching
   Session Log?" / "Your changes since version 1 are removed. Version 1 stays published." (PROPOSED words). Publish:
   asks first only when there are warnings, listing them in labels ("Publish version 2 of KGK Teaching Session Log?";
   PROPOSED); with no warnings it publishes at once, as today. Publish's reason when off is text beside it.
9. **Q9. (The operator's; T334) A system-managed type.** *Recommendation:* fold T334 in: `MayWrite` refuses a
   system-managed type to everyone, the Administrator included; the list offers View; the notice "Written by the MSF
   release. Its form changes only with a release of Wombat." (and "the learner-feedback release"; PROPOSED words). The
   runbook's 6.14a already expects it.
10. **Q10. (The operator's) What the build folds in.** *Recommendation:* T334; T324's builder half and its kin (Names a,
    the warnings, the form settings by label); T271 item 2 (the scale by name; the resolver T253 built); T291 items 3
    and 6; T326's list half; T280's tab item; the builder's Delete and Discard (T264's kind); T329's kind on both pages
    (Try again, fixed words, Unavailable); the new faults above (the field rows at 1280, the editors' place, refusals
    at their field where the DATA allows, Publish's reason, the headings, "v0", Back to list, the search). Leave T271
    item 1 (the race), T179, T106 item 11, T019-b to -g and T352 item 4 as their own.
11. **Q11. (The operator's) The institution's list.** It shows every College's national types (the Demo College's ten
    to Prof Mbatha), pending "phase 4". *Recommendation:* keep the rule (narrowing what she reads is a rule change of
    its own, with flow 03's picker); say whose each row is (Q7), and order her own types first. If you want the
    narrowing, it is a task of its own before the build.
12. **Q12. (The operator's) Subtitles and the notice's words.** *Recommendation:* the list "The forms registrars file
    and assessors complete. You can change the types you set; the College sets its instruments." (to an institution;
    the College's and the Administrator's in their words); the builder "What a registrar fills in, who moves it on, and
    what it counts towards." (PROPOSED). Keep T300's notice ("a product decision").

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are 1.24–1.31, 6.14a, A.7.9 and A.7.14; § 9):
  every Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 1.24 and 1.31 (Q7's
  columns; the no-match words), 1.25 (Q2's read; the scale by name; no Back to list), 1.26 ("Draft saved." on the first
  save, already in its Expect; Q1's frame), 1.27 (Q4's editor in place; Q6's refusal at both fields; labels), 1.28 (Q2;
  the refusal at its move), 1.29 (the credit read in words), 1.30 (Publish's reason as text; Q3's versions; Q8), 6.14a
  (T334's View and notice, already in its Expect; Q1), A.7.9 (Q5's preview at 390; the tabs). Search the whole runbook
  (BRIEF.md § 9 item 7) for the words that change ("Back to list", "v0", "Metadata", "Configured activity types").
  Record passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** no "seed:" key and no enum or role code name on any page; a no-match search distinct from
  an empty list; the College offered View on the two system-managed types; the first save's result focused; Discard
  asks first and names the type; at 1280 every field row's title reads whole beside its buttons; 390 and 768 with
  nothing scrolling sideways, the list stacked, every control 44 px at 390; the builder's columns and the list swept
  from 900 to 1100 px against their container and every list column read at 1280 as Prof Mbatha, Dr Kruger and
  devadmin; no exception's text on any failed read or save.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include `Admin/ActivityTypeBuilderLayoutTests`, `ActivityTypeBuilderAccessTests`,
  `ActivityTypesListAccessTests`, `Activities/BuilderPreviewParityTests`, with new ones for T334 (handler and bUnit),
  the labels (T324), the scale (T271), the first save's result (T291 item 3), the preview's EPAs (T291 item 6), the
  no-match state (T326), the tabs' ARIA (T280), Discard's dialog, Unavailable and Try again; `Hosting/PageTitleTests`;
  `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`; `Design/*`, including
  `NarrowLayoutTests`, `DefinedClassTests`, `RowActionMarkupTests`, `TableColumnClassTests`, `ContrastTests`; the
  scenario guard in `Scenario/`; `SeedRoundTripTests` if a DSL property is touched. Amend DESIGN.md § Builder layout
  with its banner.
- **Re-capture** the states in § 3 (every `states/` capture, all old-frame, and the new ones on scratch copies, as
  `states.md` says) and the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare
  them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as Prof Mbatha, Dr Kruger and `devadmin`, and as Dr Dlamini filing the KGK
  log (flow 03's page draws the published type).

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (`act-1-setup.md`, `act-6-catalogue.md`,
`appendix-cross-cutting.md`; Role, Route, Do and Expect only), as the T394 replay left them (2026-11-05; the Gaps it
recorded on these steps are 1.25's T271, T291 and T324, 1.26's T291, 1.27's T291 and T324, 1.31's T326, 6.14a's T291
and T334, and A.7.9's T280). `D` is the replay day (README § How to play). Step 1.24 points to a table of the twelve
instruments; it is in `act-1-setup.md` under the step and is not pasted here. Step 1.28's workflow follows it.

```text
Step 1.24 — The twelve CPSA instruments are published (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types
Do: Open Activity types, search `cpsa` and read the list. Then clear the search.
Expect: The search leaves the 12 rows in the table below. Each reads Scope `Speciality · Paediatrics`, Published `v1`,
  Draft `None` and Active, and each offers View, named for its row ("View Mini-CEX (Paediatrics)"), not Edit: the
  College's instruments are the College's, and she may not write them (T300, D52; T239 names each row's action). With
  the search cleared there are 22 rows: these twelve and the ten Demo types, which also offer View and whose Scope
  names the Demo speciality, `Speciality · General Medicine` (T291 item 5). New activity type is still offered, since
  she may create a type in her own institution.
  Two of the twelve are system-managed (T162, T164): `msf_cpsa` and `learner_feedback_cpsa`. A released MSF or
  learner-feedback campaign writes them, and nobody is offered them when filing an activity.

Step 1.25 — A College instrument is read-only to KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Open `Mini-CEX (Paediatrics)` from its View and read each tab, opening the `overall_level` field to read its
  settings. Change nothing, save nothing, and leave by Back to list.
Expect: The page is headed "Mini-CEX (Paediatrics)", not "Edit …", and the browser tab reads "Mini-CEX (Paediatrics) ·
  Wombat" (T190). It opens on the Form tab, under a standing information notice that is there on every visit (no alert
  role): "Set by the College that owns Paediatrics. You can read this activity type here, but not change it." (T300;
  the wording is a product decision.)
  - **Form settings** read, as text, Encounter date field `observed_on`, Entrustment rating field `overall_level` and
    EPA field `epa_id`.
  - **Sections.** Request holds the EPA, the Assessor (a User field), the date observed, the clinical setting, the
    presenting problem and the case complexity. Entrustment holds the `overall_level` rating. Feedback holds three
    long-text fields. Each section and field offers View, which shows its settings as text; `overall_level`'s
    Entrustment scale reads `CPSA Paediatric Entrustment Scale v11.1`, the ladder its seed key binds (T271).
  - **Metadata**, as text: Key `mini_cex_cpsa`, Name, Scope `Speciality · Paediatrics`, This tool is `Mini-CEX`, the
    description, and Status Active.
  - **Workflow**, as a code block. Draft → Requested → Completed, with Declined and Cancelled. The named assessor
    (`field:assessor_user_id`) completes or declines.
  - **Credit**, as a code block. One directive: it matches on the EPA field, counts 1, and judges the level reached by
    what `overall_level` records.
  She is offered nothing that would change the type: no Save draft, Discard draft or Publish, and no Add, Up, Down or
  Delete. The College's instruments are the College's (T211's rule; T300, D52). Any save or publish she sent would be
  refused by `ActivityTypeScopeGuard`: "You do not have permission to modify activity types in that speciality."

Step 1.26 — Start KGK's teaching log (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types → /admin/activity-types/new → /admin/activity-types/{ActivityTypeId:int}
Do: Press New activity type, open the Metadata tab and enter:
  - Key `kgk_teaching_log` and Name `KGK Teaching Session Log`;
  - Scope Institution, with `Kgosi Kgari Teaching Hospital`, which is how it opens: leave it untouched;
  - This tool is `Not a WBA instrument`;
  - Description `A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.`;
  - Active on.
  Press Save draft. Record the type's id as `{ActivityTypeId}`.
Expect: The builder opens on the Form tab, headed "New activity type", with the browser tab reading "New activity type
  · Wombat" (T300, T190). It starts with a default draft:
  - one Details section holding a required Title text field;
  - a workflow from draft to submitted;
  - the credit rules `{"counts_for": []}`.
  Scope offers her only what she may save: Institution, with KGK the only institution, and a new type starts there,
  never Global (T300, D52; DESIGN.md: a picker offers exactly what its command accepts). So the first Save draft is
  accepted without Scope being touched. This tool is offers `Not a WBA instrument` and the twelve instruments.
  Save draft moves the page to the type's own address and reads "Draft saved." (T291 item 3). The page is then headed
  "Edit KGK Teaching Session Log", and the browser tab says the same: "Edit KGK Teaching Session Log · Wombat"
  (T190).

Step 1.27 — The form, and a duplicate key refused (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: On the Form tab, edit the Details section: key `session`, title `Teaching session`. Edit its Title field: key
  `topic`, label `Topic`, type Text, required. Then add five fields to the section:
  - `epa_id`, labelled `EPA`, type EPA, required;
  - `delivered_on`, labelled `Date delivered`, type Date, required;
  - `audience`, labelled `Audience`, type Choice, required, with the options `Interns`, `Medical students`, `Nursing
    staff` and `Registrars`, one per line;
  - `objectives`, labelled `Learning objectives`, type Long text, required;
  - `supervisor_user_id`, labelled `Supervising consultant`, type User, not required, leaving Names a on its default.
  Give Audience the key `topic` by mistake, and press Save draft. Then correct its key to `audience`. Under Form
  settings, choose Encounter date field `delivered_on`, Entrustment rating field "None — this form asserts no
  entrustment level" and EPA field `epa_id`. Press Save draft again.
Expect: The field types offered are Text, Long text, Number, Date, Choice, Multi-choice, Scale, User, EPA, Likert,
  Procedure reference, File and Signature.
  - **The User field** shows Names a in place of Options. It defaults to Assessor, and also offers InstitutionalAdmin,
    Coordinator, CommitteeMember and Trainee. The people it offers come from the directory (T102).
  - **The mistaken save** is refused, and the refusal names both fields: "Field key 'topic' is used by 2 fields ('Topic'
    in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key."
  - **The corrected save** reads "Draft saved." The Form settings pickers offer only fields of the matching type.
  - **The live preview** shows one section, Teaching session, with six inputs: a text box, an EPA picker, a date
    input, a choice of four, a long-text box and a person picker. The first five are required.
  - **The EPA picker** offers the 15 PAED EPAs and not the Demo `EPA-001`, following the form's own rule (T291 item 6).
  - **The person picker** offers nobody in the preview. Whom it offers depends on the registrar filing it: an active
    Assessor at their institution, never the registrar themselves.

Step 1.28 — The workflow, and a `field:` rule refused (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: On the Workflow tab, replace the Workflow JSON with the workflow below, but with the cancel transition's actor
  written as `subject|field:topic`. Press Save draft. Then restore the actor to `subject|creator` and press Save draft
  again.
Expect: The first save is refused: "Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a
  field: rule must name a User field." (T102). The second save reads "Draft saved."
  In the saved workflow, the registrar, or whoever filed the log for them, either logs the session or cancels the
  draft. Logging checks the whole form (`validation: all`), and cancelling checks formats only (`draft`, T105).

The workflow Step 1.28 pastes (act-1-setup.md, after Step 1.28):
  {
    "version": 1,
    "initial_state": "draft",
    "states": [
      { "key": "draft", "label": "Draft" },
      { "key": "logged", "label": "Logged", "terminal": true },
      { "key": "cancelled", "label": "Cancelled" }
    ],
    "transitions": [
      { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" },
      { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" }
    ]
  }

Step 1.29 — The credit rules (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Open the Credit tab. Leave the Credit rules JSON as `{"counts_for": []}`.
Expect: The type credits nothing. The save accepts an EPA field with no credit directive (T137,
  `EvidenceEpa.EnsureCreditAgrees`). Each log is stamped with its EPA, which activity lists show.
  A type that credits nothing is held to one encounter-date rule only: the date may not be after today (T160). It is
  not held to the programme start, and it gets no 14-day late-filing warning (`EncounterDatePolicy.CanCredit`, D15).

Step 1.30 — Publish version 1 (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Press Publish.
Expect: The page reads "Published version 1." Discard draft disappears, and Publish is disabled, with the hint "Save a
  draft to publish." The Key can no longer be changed. There are no publish warnings, since there is no earlier version
  to compare against.

Step 1.31 — KGK's type in the list (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types
Do: Press Back to list and read the list. Then search `KGK`.
Expect: The list has 23 rows, and no two share a name:
  - the ten Demo types, whose Scope reads `Speciality · General Medicine` (T291 item 5);
  - the twelve `*_cpsa` types;
  - `KGK Teaching Session Log`, whose Scope reads `Institution · Kgosi Kgari Teaching Hospital`, with `v1`, `None` and
    Active.
  Her own type's row offers Edit, and every other row View (T300). The search leaves that last row only. Which of these
  types a KGK registrar is offered is checked in Act 2.

Step 6.14a — Dr Kruger drafts a College activity type (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/activity-types → /admin/activity-types/{ActivityTypeId:int} → /admin/activity-types → /admin/activity-types/new → /admin/activity-types/{ActivityTypeId:int} → /admin/activity-types
Do: Open Activity types from his menu and read the list. Open `Multi-Source Feedback (Paediatrics)` and change nothing.
  Back in the list, press New activity type, open the Metadata tab and read what Scope offers, under Sub-speciality
  too. Then enter:
  - Key `cpsa_case_presentation_log` and Name `CPSA Case Presentation Log`;
  - Scope Speciality, with `Paediatrics`;
  - This tool is `Not a WBA instrument`;
  - Description `A registrar's log of a case they presented at a departmental meeting. Not rated, and credits
    nothing.`;
  - Active on.
  Press Save draft, publish nothing, and go back to the list.
Expect:
  - **The list.** Activity types is in his menu, and lit (T300, D52). The list has 22 rows: the twelve `*_cpsa`
    instruments and the ten Demo types. The College writes its disciplines' types, so each `*_cpsa` row offers Edit,
    named for its row ("Edit Mini-CEX (Paediatrics)"). The exceptions are `msf_cpsa` and `learner_feedback_cpsa`: the
    MSF and learner-feedback releases write their data, so they offer View (T334). The Demo types belong to the Demo
    College, so they offer View. KGK Teaching Session Log is not listed, because it is an institution's own type. New
    activity type is offered.
  - **A system-managed type.** Multi-Source Feedback (Paediatrics) opens read-only, with a notice that the system writes
    it, and no Save draft, Discard draft, Publish or editor controls (T334).
  - **The new type.** It opens headed "New activity type", on Form, with the default draft. Scope offers Speciality and
    Sub-speciality only, never Global or Institution. It starts on Speciality with `Paediatrics`, the College's one
    speciality; CNSA's Neurology is not offered. Sub-speciality offers `Paediatrics / Neonatology` (Step 6.12) and
    `Paediatrics / Paediatrics`.
  - **The save.** Save draft is accepted. The page moves to the type's own address, headed "Edit CPSA Case
    Presentation Log", and reads "Draft saved." (T291 item 3). In the list the type reads `Speciality · Paediatrics`,
    with no published version, Draft "Draft saved", Active and Edit.
  - **Who sees the draft.** It is unpublished, so no registrar is offered it. Prof Mbatha's list leaves it out, and its
    address reads "could not be found" to her: a type she may neither write nor open is not listed (T300, T211).

Step A.7.9 — Prof Mbatha on her phone (appendix-cross-cutting.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, read Home, then open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK
  Teaching Session Log in the builder.
Expect: Nothing scrolls sideways on any of them (T323).
  - **Home's** three cards stack; People's counts and each card's footer link are 44 px tall, and a long contact address
    wraps (M7).
  - **Users** stacks: the filter's fields one under another, Show and Clear full width; People a block per person, the
    name over the address, then Roles, Scope, Status and Manage, 44 px (T376; until T376 the table scrolled 807 px
    in its box).
  - **Dr Dlamini's account** stacks its account line and sections: Roles and scope reads "Trainee" and "System-managed"
    apart (F-2.28b), with Add a role…; Sign-in and access's Password, Institutional sign-in and Lockout one under another,
    the password field's Show toggle 44 px tall, its buttons wrapping rather than overflowing. Her Lockout lists two
    lines: the trainee line and the reminders line. There is no Back to users.
  - **Trainees:** Current trainees is stacked, a block per row with its Edit 44 px, and does not scroll sideways (T373,
    S16); Completed and closed is stacked too, a block per ended programme, the latest end first, with its View 44 px
    (T379, T323).
  - **Dr Dlamini's profile**, "Anele Dlamini's programme", has no Back to trainees and offers "Export Anele Dlamini's
    portfolio" in its header (T379). It stacks three fields plus the last day and two triggers: Curriculum, Programme
    start date and Expected completion date, then End this programme's last day with Mark complete… and Deactivate….
  - **The builder's** editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
    stays usable.

Step A.7.14 — Prof Mbatha's pages, checked for contrast (appendix-cross-cutting.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on Committee reviews (each state a badge since T364, Scheduled grey there and on Decisions due)
    and on a committee review opened from it (Dr Molefe's review 7);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (the dialog's Lock out Mohammed Patel: open Lock out… on Dr Patel's page, read it,
    then Keep access; T376, S22) and success (Publish in the builder, not pressed; with no draft it is disabled, and a
    disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
```
