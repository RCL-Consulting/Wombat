# F17 — An institution builds and publishes its own activity type

**Who and why.** Prof Nolwazi Mbatha, KGK's InstitutionalAdmin, reads the College's instruments that she may not change,
then builds KGK's own teaching log and publishes it. Every form a registrar files and an assessor rates (F03, F04) is
drawn at runtime from a type built on this page (BRIEF § 5.2; `Components/Shared/Activities/ActivityForm.razor`).

| | |
|---|---|
| **Mode** | **Wireframe first** (BRIEF § 2.3 step 3). This is the most complex UI in the product, and its output becomes every F03 and F04 form. The research advises treating the builder as a flow of its own, with seed schema JSON supplied (research `best_inputs_for_existing_app_redesign[5]`). BRIEF § 4 names it one of three dense pages to lay out deliberately inside its shape. |
| **People** | InstitutionalAdmin Prof Nolwazi Mbatha; the Administrator (devadmin). After T300, the CollegeAdmin Dr Anton Kruger as well (BRIEF § 7 B11). |
| **Frequency and stakes** | Rare: a type is built once and republished seldom. Stakes are medium-high, because it defines every form. |
| **Pages** | `/admin/activity-types` (`Admin/ActivityTypes/ActivityTypesList.razor`); `/admin/activity-types/new` and `/admin/activity-types/{ActivityTypeId:int}` (`ActivityTypeEdit.razor`) (`coverage.md` § Pages) |
| **Runbook steps** | 10: `act-1-setup.md` 1.24–1.31; `appendix-cross-cutting.md` A.7.9 and A.7.14 |
| **Held** | T300 (group 1): 22 images, listed in § 5.4. |

## 1. Before you paste

1. **T300 has not landed.** It changes four things (BRIEF § 10):
   - the list offers View or Edit per row;
   - a College instrument opens read-only;
   - the Scope picker is narrowed;
   - the CollegeAdmin gets Activity Types in the nav.

   You can run the wireframe round now from the key screenshots in § 5.1. They show Mbatha's own type, which T300
   leaves writable (inference from T300 § What to build). Attach the § 5.4 images only after T300 lands and they are
   re-captured.
2. **Open every image before you upload it** (BRIEF § 3.3). None of this flow's images is an invitation capture. These
   were opened for this brief and show no link or password:
   - `states/activity-type-edit--user-field.png`, `--field-rule.png`, `--published.png`, `--publish-warnings.png` and
     `--discarded.png`;
   - `act-1/1.27-2-form-saved-preview.png`, `1.28-2-workflow-saved.png` and `1.29-1-credit-tab.png`;
   - `act-A/A.7.9-5-builder-top.png`.
3. **Some full-page captures show the sidebar part-way down the page.** In `states/activity-type-edit--user-field.png`,
   the sidebar and the top row appear again about 2,500 px down. That is how the full-page capture stitched a fixed
   element, not the layout (observed; the cause is an inference). Say so in the chat if Claude Design copies it.
4. **Attach three seed folders as text** (BRIEF § 3.1). They are nine tracked JSON files, about 10 KB in all:
   - `src/Wombat.Infrastructure/Activities/Seeds/mini_cex_cpsa/` holds a rated College instrument: 3 sections, 5
     states, and one credit directive;
   - `…/reflective_exercise_cpsa/` holds an unrated College instrument that credits nothing;
   - `…/teaching_session/` holds a generic teaching log, the nearest to KGK's type.

   Each holds `schema.json`, `workflow.json` and `credit.json`. F03 gets the same three, so the builder and the renderer
   share one vocabulary.

## 2. The ask (paste into Claude Design)

Paste everything inside the fence. Then attach § 5.1, and § 5.3's seed files.

````text
Design the activity-type builder for Wombat, a work-based assessment tool for specialist registrars. It is 5 screens,
each at desktop 1280×800 and phone 390×844, wireframes first. An institution's administrator reads the College's
assessment instruments, which she may not change. She then builds her own activity type (a form, a workflow and credit
rules) and publishes version 1. Use the Wombat design system set up earlier in this project: its tokens, type, Lucide
icons and components. The constraints below are binding.

FLOW 17 — Build and publish an activity type of our own
GOAL: Let an institution's administrator do seven things:
  1. find a type in the list, and see at a glance whether she may change it;
  2. read a College instrument she may not change: its fields and each field's settings, its workflow and its credit
     rules, under a standing notice that the College sets it;
  3. start a type in a scope she can write;
  4. build its form: sections; fields of 13 types; a User field that names a role; a unique key per field; and the
     three form settings (which field is the encounter date, which is the entrustment rating, which names the EPA);
  5. set its workflow: its states, its moves, who may make each move, and how much of the form each move checks;
  6. set its credit rules;
  7. see what a publish will change, publish version 1, and find the type in the list.
  A live preview shows the form a registrar will fill.
  What is wrong today:
  - the builder offers Save draft, Publish and a Global scope that its own guard then refuses (T300, being fixed);
  - the field list prints key and code name ("epa_id · Epa", "objectives · LongText");
  - the tabs do not tell a screen reader which tab is selected;
  - Workflow and Credit are raw JSON text boxes;
  - the field editor opens below every section, far from the Edit that opened it.
AUDIENCE: Mainly an InstitutionalAdmin, Prof Nolwazi Mbatha, at Kgosi Kgari Teaching Hospital (KGK). She is at ease
  with forms, not with JSON, and uses the builder rarely, for a high-stakes job. Also the Administrator (the platform
  operator), and after T300 the College's administrator, Dr Anton Kruger (CPSA). Each writes a different set of scopes.
  Desktop 1280×800 and phone 390×844.
CONTENT (what the builder edits; the attached seed JSON shows each part):
  - Metadata: Key (fixed once published), Name, Scope (Global, Institution, Speciality or Sub-speciality, plus which
    one), "This tool is" (the instrument: Not a WBA instrument, or one of the College's twelve), Description, Active.
  - Form: sections, each holding fields. A field has a key, a label, a type, help text, options, a catalogue key and
    Required. The 13 types are Text, Long text, Number, Date, Choice, Multi-choice, Scale, User, EPA, Likert,
    Procedure reference, File and Signature. A User field has "Names a" (a role, Assessor by default) in place of
    Options. A Scale field is bound to an entrustment scale. The Form settings are Encounter date field, Entrustment
    rating field and EPA field.
  - Workflow: states (key, label, terminal) and moves. A move has a key, from, to, and an actor rule: who may make it,
    written with subject, creator, role:, scope: or field:<a User field>, combined with | and +. It also has a
    validation: all (the whole form), owned (the mover's fields) or draft (formats only), and may require a note.
  - Credit: counts_for directives. Each matches a curriculum item through the EPA field, counts an amount, and may judge
    the level from the rating field. An empty list credits nothing.
  - Versions: publishing makes version n+1. Activities already filed stay on the version they were filed under.
SCREENS, in order:
  1. /admin/activity-types — Activity types. Show all 23 types, a search, a row with a draft in flight, a search that
     matches nothing, and 390 px. Each row offers View, or Edit where she may write it. New activity type shows only
     to someone who can write in at least one scope.
  2. /admin/activity-types/{id} — a College instrument, Mini-CEX (Paediatrics), opened read-only by an institution.
     Every part can be read, and the field editor opens to read a field, including the scale it is bound to. There is
     no Save draft, Discard draft, Publish, Add, Up, Down or Delete. A standing notice says whose it is ("Set by the
     College"; the wording is yours to design).
  3. /admin/activity-types/new → /{id} — a new type:
     - the metadata, with Scope offering only the scopes she can write;
     - the form: the section and field list, the field editor, a User field, a duplicate key refused, and the live
       preview.
  4. /admin/activity-types/{id} — the workflow (states, moves and who may make each; a field: rule naming a non-User
     field refused) and the credit rules (a type that credits nothing; a directive that credits an EPA).
  5. Publishing and after:
     - the warnings shown before a publish ("Field 'Audience' will be removed.");
     - "Published version 1.";
     - a discarded draft;
     - an unknown id (not found);
     - the builder at 390 px.
STEPS: 1.24, 1.25, 1.26, 1.27, 1.28, 1.29, 1.30, 1.31, A.7.9, A.7.14, pasted verbatim at the end.
STATES TO SHOW:
  - Loading: the header shows at once, and there is no Save draft until the type has loaded.
  - Load error: the alert only, with no actions.
  - Not found: "The activity type could not be found.", with no "New activity type" title and no Save draft.
  - Refused save: a danger alert that names the field. What she typed stays.
  - Saved ("Draft saved."), published ("Published version 1."), discarded ("Draft discarded."), read-only, and narrow.
  - Data volumes:
    - none: a new type's default draft, which is one Details section with a required Title field, draft → submitted,
      and {"counts_for": []};
    - typical: KGK Teaching Session Log, with 1 section, 6 fields, 3 states, 2 moves and no credit;
    - heavy: Mini-CEX (Paediatrics), with 3 sections, 10 fields, 5 states, 4 moves and 1 credit directive.
  - Today's captures of these states are listed under ATTACHED.
REQUIREMENTS FROM KNOWN DEFECTS:
  - T300 (being fixed):
    - A caller who cannot write a type gets it read-only. A standing notice says whose it is: the College's for a
      Speciality or Sub-speciality type, the platform's for a Global one. No Save draft, Discard draft, Publish, Add,
      Up, Down or Delete is shown. The field editor still opens for reading.
    - The list offers Edit on writable rows and View on the rest, each named for its row ("Edit KGK Teaching Session
      Log"). New activity type shows only to a caller who can write in at least one scope.
    - Scope offers exactly the scopes the caller may write, and a new type starts in the first of them:
      - an InstitutionalAdmin: Institution, with only her own institution;
      - a CollegeAdmin: Speciality and Sub-speciality, of his College only;
      - an Administrator: all four.
      A new type never starts as Global unless an Administrator makes it.
    - The page title comes from the address, so an existing type never reads "New activity type".
    - No Save draft, Discard draft or Publish appears before the type has loaded, or after a load error (also T329).
  - T324: types are shown by label. The field list reads "EPA" and "Long text", not "epa_id · Epa" and
    "objectives · LongText". Scope reads "Sub-speciality", not "SubSpeciality". A key may be shown as a secondary,
    monospace detail.
  - T291 item 5: a scope names its speciality or institution, never "Speciality · #1".
  - T326: a search that matches nothing says so ('No activity types match "zzz".'). It is not the empty-catalogue
    state "No activity types. Create the first activity type to start the builder."
  - T280 and T190 (accessible names): the tab bar tells a screen reader which tab is selected. The browser tab's title
    follows the page heading and is never empty (on /new it is empty today).
  - T291 item 3: the first Save draft of a new type shows "Draft saved." and moves the focus to it. Today the page moves
    to the type's address and shows nothing.
  - T271: a Scale field bound to the College's ladder shows "CPSA Paediatric Entrustment Scale v11.1", not "Select…",
    in both the editable and the read-only builder.
  - Destructive actions (T264; DESIGN.md: a red filled button is only a dialog's confirm):
    - Delete on a section or field is an outline button named for its target. Today every row has a red filled Delete.
    - Discard draft asks first and names the type. Today it does not ask.
  - Contrast (T322): Publish is green with white text at 2.87:1, and the "Draft saved." alert is 2.55:1. Both need
    4.5:1.
  - When Publish is unavailable, say why in text. Today the reason ("Save a draft to publish.") is only a tooltip.
  - At 390 px (T323): nothing scrolls sideways. The preview starts about 4,250 px down the page today, and the four
    tabs wrap to two rows.
  - The Form settings pickers list fields by label, not by key (today they read "delivered_on").
  - Later, not built (show as a later phase if at all, and mark it NEW):
    - T019-b: drag-and-drop reorder (Up and Down stay as the keyboard path);
    - T019-c: nested and repeatable sections;
    - T019-d: a visual workflow editor;
    - T019-e: a visual credit-rules editor;
    - T019-f: visibility on several conditions;
    - T019-g: templates, and copy from an existing type;
    - T106 item 11: an input for who may write a field or section (editable_by), which is JSON-only today;
    - T179: "This tool is" takes effect when a draft is saved, not when it is published.
    Until those land, the workflow and credit rules are edited as JSON (monospace). A read view drawn from that JSON is
    fair game.
QUESTIONS THE DESIGN MUST ANSWER:
  1. Form, workflow and credit as tabs (as today: Metadata, Form, Workflow, Credit), or one page with a live preview of
     the form a trainee will see?
  2. How is the workflow shown: as a state diagram, or as a table of moves (who, from, to)?
  3. How does the builder show what a published version pins? Existing activities stay on their version.
  4. On a phone, where does the live preview go?
CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side behaviour beyond a
  ≤10-line script module; no live validation; the phone nav toggle is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence.
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token or a NEW token with its value; spacing on the scale xs 4, sm 8, md 16, lg 24, xl 32,
  2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, primary action),
  Breadcrumbs, DataTable (.clinic-table in .table-container, PagerControls), FormField / FormActions (.form-container,
  .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid), StatePanel (loading / empty / error),
  Skeleton, Alert (success / info / warning / danger), ActionResult, ConfirmDialog (native <dialog>), badges (five tints),
  Icon, TrajectoryChart (hand-drawn SVG, no chart library).
Design these framework states explicitly (no source file shows them): the active nav item; field validation (invalid
  border plus a stripe, not colour alone; message under the field; the summary); the reconnect dialog (rejoining,
  retrying, failed, paused, resume-failed); the in-app error bar; access denied; not found; session ended.
Content: people by name, states and types by label, times in South African time with the zone shown; an out-of-scope
  record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (sidebar included); targets ≥ 24 px; focus
  moves to an action's result; every page works at 390 px with no sideways scroll.
Viewports: 1280×800 and 390×844.
ASK: 2–3 variations, as wireframes first, then the chosen one at full fidelity. Name every design-system component you
  use, and mark anything else NEW. Say which DESIGN.md rule a variation breaks. The rules most at risk are these:
  - § Builder layout: the tab bar under PageHeader; an editor rail beside a live preview, which stack when the card is
    under 46.5rem wide;
  - the JSON tabs are monospace (DESIGN.md:2087).
  The live preview is the real form renderer, so a field in the preview must look exactly as it will in F03. Flag edge
  cases: empty, error and loading states, and the three data volumes. Then review the result for accessibility against
  WCAG 2.1 AA.
ATTACHED: the screenshots below (paths under design/baseline/), in this order:
  1. states/activity-type-edit--user-field.png
  2. act-1/1.27-2-form-saved-preview.png
  3. states/activity-type-edit--field-rule.png
  4. states/activity-type-edit--published.png
  5. states/activity-type-edit--publish-warnings.png
  6. act-A/A.7.9-5-builder-top.png
  7. act-1/1.28-2-workflow-saved.png
  8. act-1/1.29-1-credit-tab.png
  Also, as text, the nine seed files under src/Wombat.Infrastructure/Activities/Seeds/:
    mini_cex_cpsa/: schema.json, workflow.json, credit.json
    reflective_exercise_cpsa/: schema.json, workflow.json, credit.json
    teaching_session/: schema.json, workflow.json, credit.json
  More state captures follow when you ask for a state.

RUNBOOK STEPS, verbatim (Role / Route / Do / Expect). Where an Expect describes today's behaviour and a requirement
above says otherwise, the requirement wins:
- 1.24's "each offers Edit" becomes View for the College rows (T300);
- 1.26's "the browser tab reading 'Activity Type'" follows the heading instead (T190).
A.7.14 checks contrast across several pages; for this flow, only its builder part applies (the Publish button).

[act-1-setup.md]
### Step 1.24 — The twelve CPSA instruments are published
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types
Do: Open Activity Types, search `cpsa` and read the list. Then clear the search.
Expect: The search leaves the 12 rows in the table below. Each reads Scope `Speciality · Paediatrics`, Published `v1`,
  Draft `None` and Active, and each offers Edit. With the search cleared there are 22 rows: these twelve and the ten
  Demo types.
  Two of the twelve are system-managed (T162, T164): `msf_cpsa` and `learner_feedback_cpsa`. A released MSF or
  learner-feedback campaign writes them, and nobody is offered them when filing an activity.

[act-1-setup.md]
### Step 1.25 — A College instrument is read-only to KGK
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Open `Mini-CEX (Paediatrics)` and read each tab, opening the `overall_level` field in the field editor. Change
  nothing, save nothing, and leave by Back to list.
Expect: The page is headed "Edit Mini-CEX (Paediatrics)" and opens on the Form tab.
  - **Form settings** read Encounter date field `observed_on`, Entrustment rating field `overall_level` and EPA field
    `epa_id`.
  - **Sections.** Request holds the EPA, the Assessor (a User field), the date observed, the clinical setting, the
    presenting problem and the case complexity. Entrustment holds the `overall_level` rating; in the field editor its
    Entrustment scale reads `CPSA Paediatric Entrustment Scale v11.1`, the ladder its seed key binds (T271). Feedback
    holds three long-text fields.
  - **Metadata.** The key `mini_cex_cpsa` cannot be changed, because the type is published. Scope reads Speciality,
    `Paediatrics`, and This tool is reads `Mini-CEX`.
  - **Workflow.** Draft → Requested → Completed, with Declined and Cancelled. The named assessor
    (`field:assessor_user_id`) completes or declines.
  - **Credit.** One directive: it matches on the EPA field, counts 1, and judges the level reached by what
    `overall_level` records.
  She is offered nothing that would change the type, no Save draft and no Publish: the College's instruments are the
  College's (T211's rule). Any save or publish she sent would be refused by `ActivityTypeScopeGuard`: "You do not have
  permission to modify activity types in that speciality."

[act-1-setup.md]
### Step 1.26 — Start KGK's teaching log
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types → /admin/activity-types/new → /admin/activity-types/{ActivityTypeId:int}
Do: Press New activity type, open the Metadata tab and enter:
  - Key `kgk_teaching_log` and Name `KGK Teaching Session Log`;
  - Scope Institution, with `Kgosi Kgari Teaching Hospital`;
  - This tool is `Not a WBA instrument`;
  - Description `A registrar's log of a teaching session they delivered at KGK. Not rated, and credits nothing.`;
  - Active on.
  Press Save draft. Record the type's id as `{ActivityTypeId}`.
Expect: The builder opens on the Form tab, headed "New activity type", with the browser tab reading "Activity Type". It
  starts with a default draft:
  - one Details section holding a required Title text field;
  - a workflow from draft to submitted;
  - the credit rules `{"counts_for": []}`.
  Scope offers her only what she may save: Institution, with KGK the only institution. (DESIGN.md: a picker offers
  exactly what its command accepts.) This tool is offers `Not a WBA instrument` and the twelve instruments.
  Save draft moves the page to the type's own address and reads "Draft saved." (T291 item 3). The page is then headed
  "Edit KGK Teaching Session Log", and the browser tab reads `KGK Teaching Session Log`.

[act-1-setup.md]
### Step 1.27 — The form, and a duplicate key refused
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

[act-1-setup.md]
### Step 1.28 — The workflow, and a `field:` rule refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: On the Workflow tab, replace the Workflow JSON with the workflow below, but with the cancel transition's actor
  written as `subject|field:topic`. Press Save draft. Then restore the actor to `subject|creator` and press Save draft
  again.
Expect: The first save is refused: "Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a
  field: rule must name a User field." (T102). The second save reads "Draft saved."
  In the saved workflow, the registrar, or whoever filed the log for them, either logs the session or cancels the
  draft. Logging checks the whole form (`validation: all`), and cancelling checks formats only (`draft`, T105).

[act-1-setup.md]
### Step 1.29 — The credit rules
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Open the Credit tab. Leave the Credit rules JSON as `{"counts_for": []}`.
Expect: The type credits nothing. The save accepts an EPA field with no credit directive (T137,
  `EvidenceEpa.EnsureCreditAgrees`). Each log is stamped with its EPA, which activity lists show.
  A type that credits nothing is held to one encounter-date rule only: the date may not be after today (T160). It is
  not held to the programme start, and it gets no 14-day late-filing warning (`EncounterDatePolicy.CanCredit`, D15).

[act-1-setup.md]
### Step 1.30 — Publish version 1
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types/{ActivityTypeId:int}
Do: Press Publish.
Expect: The page reads "Published version 1." Discard draft disappears, and Publish is disabled, with the hint "Save a
  draft to publish." The Key can no longer be changed. There are no publish warnings, since there is no earlier version
  to compare against.

[act-1-setup.md]
### Step 1.31 — KGK's type in the list
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/activity-types
Do: Press Back to list and read the list. Then search `KGK`.
Expect: The list has 23 rows, and no two share a name:
  - the ten Demo types, whose Scope reads `Speciality · General Medicine` (T291 item 5);
  - the twelve `*_cpsa` types;
  - `KGK Teaching Session Log`, whose Scope reads `Institution · Kgosi Kgari Teaching Hospital`, with `v1`, `None` and
    Active.
  The search leaves that last row only. Which of these types a KGK registrar is offered is checked in Act 2.

[appendix-cross-cutting.md]
### Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK Teaching Session
  Log in the builder.
Expect: The user page's cards stack, and its buttons wrap rather than overflow. The trainee profile form stacks its
  fields. The builder's editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
  stays usable.

[appendix-cross-cutting.md]
### Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient;
  - muted text on the page background;
  - the status badges on a committee review opened from Committee Reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My Account unchanged: "Profile saved.") and a danger alert (Change password with a
    confirmation that differs, which checks no password and changes nothing);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.

The workflow Step 1.28 pastes (act-1-setup.md, after Step 1.28):
```json
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
```
````

## 3. The journey

| # | Step | Page template | What she does | What she must be able to see |
|---|---|---|---|---|
| 1 | 1.24 | `/admin/activity-types` | Searches `cpsa`, then clears the search | The twelve College instruments, each with its scope, version, draft state and status. After T300 each offers View, not Edit. |
| 2 | 1.25 | `/admin/activity-types/{ActivityTypeId:int}` | Opens Mini-CEX (Paediatrics), reads each tab, and opens `overall_level` in the field editor | That the type is the College's and she can only read it; the scale its rating is bound to (T271); its workflow and credit rule |
| 3 | 1.26 | `/admin/activity-types` → `/new` → `/{ActivityTypeId:int}` | Presses New activity type, fills Metadata and saves a draft | Only Institution and KGK under Scope; "Draft saved."; the page moving to the type's own address and title |
| 4 | 1.27 | `/admin/activity-types/{ActivityTypeId:int}` | Builds the form, with a duplicate key refused and then fixed, and sets the Form settings | A refusal naming both fields; field types by label; the live preview with six inputs |
| 5 | 1.28 | same | Writes the workflow; a `field:` rule on a Text field is refused, then fixed | The refusal; the saved moves (log checks all, cancel checks formats only; both `subject\|creator`) |
| 6 | 1.29 | same | Opens Credit and leaves it empty | That the type credits nothing, and what follows from that (only the "not after today" date rule applies) |
| 7 | 1.30 | same | Presses Publish | "Published version 1."; the key locked; no warnings (there is no earlier version to compare with) |
| 8 | 1.31 | `/admin/activity-types` | Goes back to the list and searches `KGK` | Her type: Institution · Kgosi Kgari Teaching Hospital, v1, no draft, Active. A search that finds nothing says so. |
| 9 | A.7.9 | `/admin/activity-types/{ActivityTypeId:int}` at 390 px | Opens the builder on her phone | The editor and preview stacked in one column; a usable tab bar; no sideways scroll |
| 10 | A.7.14 | same | Checks contrast | Publish (white on green) and the success alert meeting 4.5:1 |

## 4. States to design

From `states.md` § Institution administration, rows 604–622. "Held" means T300 changes the page, so re-capture before
attaching.

| Page | State | Capture (under `design/baseline/`) | Held | How it is reached |
|---|---|---|---|---|
| `/admin/activity-types` | All | `states/activity-types-list--all.png` | T300 | At Step 1.31 |
| | Searched | `states/activity-types-list--search.png` | T300 | At Step 1.24: `cpsa` |
| | A draft in the list | `states/activity-types-list--draft.png` | T300 | Between Steps 1.26 and 1.30: Draft reads "Draft saved" |
| | No match | `states/activity-types-list--no-match.png` | T300 | At Step 1.31, search `zzz` |
| | Loading | `states/activity-types-list--loading.png` | T300 | Hold a read (`states.md` § Holding a read) |
| | Narrow | `states/activity-types-list--narrow.png` | T300 | At Step 1.31, at 390 px |
| `/admin/activity-types/{id}` | A College instrument, seen by an institution | `states/activity-type-edit--college-instrument.png` | T300 | At Step 1.25, the Form tab |
| `/admin/activity-types/new` | New, the default draft | `states/activity-type-edit--new.png` | T300 | At Step 1.26, before typing |
| `/admin/activity-types/{id}` | Metadata tab | `states/activity-type-edit--metadata.png` | T300 | At Step 1.26, after Save draft |
| | The field editor, a User field | `states/activity-type-edit--user-field.png` | — | At Step 1.27 |
| | Duplicate key refused | `states/activity-type-edit--duplicate-key.png` | — | At Step 1.27 |
| | A `field:` rule refused | `states/activity-type-edit--field-rule.png` | — | At Step 1.28 |
| | Credit tab | `states/activity-type-edit--credit.png` | — | At Step 1.29 |
| | Published | `states/activity-type-edit--published.png` | — | At Step 1.30 |
| | Publish warnings | `states/activity-type-edit--publish-warnings.png` | — | On a scratch database: delete Audience, then Save draft |
| | Draft discarded | `states/activity-type-edit--discarded.png` | — | On a scratch database, after the warnings: Discard draft |
| | Unknown id | `states/activity-type-edit--not-found.png` | T300 | Typed: `/admin/activity-types/999999` |
| | Loading | `states/activity-type-edit--loading.png` | T300 | Hold a read, then Edit on a row |
| | Narrow | `states/activity-type-edit--narrow.png` | — | At Step A.7.9 |

**States with no capture, which the design must draw:**
- A College instrument read-only, with its notice (T300).
- The CollegeAdmin's builder: Scope offers Speciality and Sub-speciality of his College (T300).
- A View row beside an Edit row (T300).
- The no-match search state, distinct from the empty catalogue (T326).
- Publish unavailable, with its reason in text.
- The loading and not-found states without "New activity type" or Save draft (T300, T329).

No builder state is on `states.md`'s list of states a replay cannot reach.

## 5. Attach

Every path below is relative to `design/baseline/` and was checked with `ls` on 2026-09-26.

### 5.1 Attach these first

1. `states/activity-type-edit--user-field.png`
2. `act-1/1.27-2-form-saved-preview.png`
3. `states/activity-type-edit--field-rule.png`
4. `states/activity-type-edit--published.png`
5. `states/activity-type-edit--publish-warnings.png`
6. `act-A/A.7.9-5-builder-top.png`
7. `act-1/1.28-2-workflow-saved.png`
8. `act-1/1.29-1-credit-tab.png`

### 5.2 Add when the chat asks for a state (not held)

- `states/activity-type-edit--duplicate-key.png`, `--credit.png`, `--discarded.png` and `--narrow.png`
- `act-1/1.27-1-duplicate-key-refused.png`, `1.28-1-field-rule-refused.png` and `1.30-1-published-v1.png`
- `act-A/A.7.14-5-builder-publish-disabled.png`

### 5.3 Seed JSON (as text)

The nine files under `src/Wombat.Infrastructure/Activities/Seeds/`:
- `mini_cex_cpsa/`: `schema.json`, `workflow.json` and `credit.json`;
- `reflective_exercise_cpsa/`: the same three files;
- `teaching_session/`: the same three files.

All nine are tracked (`git ls-files`).

### 5.4 Held until T300 lands and they are re-captured

- act-1:
  - `act-1/1.24-1-types-search-cpsa.png`;
  - `1.25-1-mini-cex-form.png`, `1.25-2-mini-cex-metadata.png`, `1.25-3-mini-cex-workflow.png`,
    `1.25-4-mini-cex-credit.png` and `1.25-5-overall-level-field-editor.png`;
  - `1.26-1-new-type-default.png` and `1.26-2-first-save-no-status.png`;
  - `1.31-1-types-all.png` and `1.31-2-types-search-kgk.png`.
- act-A: `act-A/A.7.9-6-mbatha-builder.png`.
- states:
  - `states/activity-type-edit--college-instrument.png`, `--new.png`, `--loading.png`, `--not-found.png` and
    `--metadata.png`;
  - `states/activity-types-list--all.png`, `--search.png`, `--draft.png`, `--narrow.png`, `--no-match.png` and
    `--loading.png`.

## 6. Known problems this design must solve

| Task | Lane | What it means for the design | Evidence |
|---|---|---|---|
| T300 | queued; group 1, being fixed (BRIEF § 10) | Adds a read-only mode with a standing notice. Each list row offers View or Edit. New activity type shows only to a caller who can write somewhere. Scope offers exactly the writable scopes (an InstitutionalAdmin sees Institution and KGK only). The title comes from the address. No Save draft appears until the editor has loaded. The CollegeAdmin gets Activity Types in the nav (BRIEF § 7 B11). | Steps 1.25, 1.26; `ActivityTypeEdit.razor:21-26, :81`; `ActivityTypesList.razor:15, :53` |
| T329 | queued, P2 | While loading, or on an unknown id, the editor is not titled "New activity type" and does not offer Save draft (BRIEF § 6 A6). | `states/activity-type-edit--loading.png`, `--not-found.png` (held) |
| T324 | queued | Labels, not keys or code names: "EPA" and "Long text" in the field list (`ActivityTypeEdit.razor:218` prints the enum, though `FieldTypeLabel` exists at :870); "Sub-speciality" in Scope. The publish warning for a change of type also prints code names (`BuilderModels.cs:209`; from the code, not captured). (A8) | `act-1/1.27-2-form-saved-preview.png` |
| T326 | queued | A no-match search is its own state, not "Create the first activity type" (A10). | `states/activity-types-list--no-match.png` (held) |
| T190, T280 | queued | The tab bar exposes the selected tab: the tabs pattern, or `aria-current` on the active button. DESIGN.md § Builder layout says nothing on this yet. The page title follows the heading and is never empty. (A14) | `act-A/A.7.9-5-builder-top.png`; `ActivityTypeEdit.razor:14, :62-66` |
| T271 | queued | A field bound to a scale by seed key shows that scale, not "Select…". This includes the read-only mode, where Step 1.25 reads it. | `act-1/1.25-5-overall-level-field-editor.png` (held) |
| T291 items 3, 5, 6 | queued | Item 3: the first save shows "Draft saved." and takes the focus. Item 5: a scope label is never "#1". Item 6: the preview's EPA picker lists what the real form would (the 15 PAED EPAs, not the Demo EPA-001). | `act-1/1.26-2-first-save-no-status.png`, `1.31-1-types-all.png` (held); `1.27-2-form-saved-preview.png` |
| T264, A5 | queued | Section and field Delete are red filled buttons (`ActivityTypeEdit.razor:207, :225`). DESIGN.md:245–267 keeps `.btn-danger` for a dialog footer (BRIEF § 5.4). Discard draft has no confirmation (`:22-25`; observed in code, not filed). | `states/activity-type-edit--user-field.png` |
| T322, A1 | queued, P2 | Publish is `.btn-success`: white on it is 2.87:1. The success alert is 2.55:1. | `act-A/A.7.14-5-builder-publish-disabled.png`; Step A.7.14 Actual |
| T323, A3 | queued | At 390 px the tabs wrap to two rows, and the preview starts about 4,250 px down the page (Step A.7.9 Actual). The columns stack by the card's width (DESIGN.md § Builder layout, T266). | `act-A/A.7.9-5-builder-top.png`, `states/activity-type-edit--narrow.png` |
| (not filed) | — | Publish's reason is a tooltip only (Step 1.30 Actual: "carried as the button's title"). The field and section editors render below the whole section list, far from the Edit pressed. The Form settings pickers list field keys (`delivered_on`), not labels. All three are observed. | `act-1/1.27-2-form-saved-preview.png`, `states/activity-type-edit--user-field.png` |
| T019-b…g, T106 item 11, T179 | queued | The builder backlog (BRIEF § 7 B10). The design may anticipate it but must not assume it is built: drag-and-drop reorder, nested and repeatable sections, visual workflow and credit editors, visibility on several conditions, templates and copy, an `editable_by` input. "This tool is" takes effect on save, not on publish (T179), so the design should not show it as pinned by a version. | the task files |

## 7. Questions the design must answer

Write each answer as a sentence in the Claude Design chat, since the chat travels in the handoff bundle (BRIEF § 2.3).

1. **Tabs, or one page with a live preview?** Today there are four tabs: Metadata, Form, Workflow and Credit. The
   preview sits beside Form only (`ActivityTypeEdit.razor:62-66`; DESIGN.md § Builder layout). A single page would let
   the workflow and credit rules be read against the form they act on.
2. **A state diagram, or a table of moves (who, from, to)?** Either must be drawn from today's workflow JSON. Editing
   it visually is T019-d, which is not built. A table also has to carry each move's validation and whether a note is
   required.
3. **How does the builder show what a published version pins?** Existing activities stay on the version they were
   filed under (CLAUDE.md § Activity platform). Today the page shows only the publish warnings
   (`BuilderModels.cs:178-242`: a section or field removed, a type changed, a field made required, a User field's role
   changed, a form setting lost). The list shows "Published v1" and "Draft".
4. **On a phone, where does the preview go?** This one was added from Step A.7.9, where the preview starts about
   4,250 px down.

## 8. Acceptance

After Claude Code builds it (BRIEF § 9):

1. **Replay the steps** on a fresh database, as BRIEF § 9 says: 1.24, 1.25, 1.26, 1.27, 1.28, 1.29, 1.30, 1.31, A.7.9
   and A.7.14.
   - Play Act 1 up to and through 1.31. Play the appendix steps after Act 6, on the same database.
   - Add T300's new step as Dr Kruger: the builder opens, and he saves a draft of a new Paediatrics-scoped type (not a
     seeded one) (T300 § Verification).
2. **Update the Expects** whose wording changes, in the same task (BRIEF § 9 item 7):
   - 1.24: View on the College rows;
   - 1.26: the browser tab's title;
   - 1.30: Publish's reason, if it becomes on-page text;
   - 1.31: the no-match wording.
3. **Run the tests.** `tests/Wombat.Web.Tests` must be green without `--no-build`, including:
   - `Admin/ActivityTypeBuilderLayoutTests`;
   - `Design/NarrowLayoutTests` (the builder's columns stack by the card's width);
   - `Design/DefinedClassTests`;
   - `Design/RowActionMarkupTests`;
   - T300's bUnit tests.

   If the tabs or the two-column rule change, amend DESIGN.md § Builder layout in the same task.
4. **Re-capture** the 19 states in § 4 and this flow's step captures into `design/baseline/`, and compare them with the
   chosen artboards.
5. **Check it in a browser** at 1280 and 390, as Prof Mbatha, as devadmin and, after T300, as Dr Kruger.
