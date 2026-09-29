# F16 The College keeps the national catalogue

Dr Anton Kruger keeps the CPSA's specialities, EPAs and curriculum versions, and the Administrator keeps the Colleges
and the entrustment scales. One save here changes what every adopting institution's registrars are measured against,
so the pages must show whose page it is and how far a change reaches.

Part of the GUI redesign briefed in `design/BRIEF.md` (T332). Written 2026-09-26.

| | |
|---|---|
| **People** | CollegeAdmin Dr Anton Kruger (CPSA); Administrator `devadmin`. Steps 6.18, 6.19 and 6.24 show the EPA pause as Assessor Dr Mohammed Patel and Trainee Dr Anele Dlamini meet it |
| **Frequency and stakes** | A few times a year; about one catalogue version a year. High: "a target and its period are read live, in every institution that has adopted the curriculum, including periods that have already closed" (Step 1.18's Expect, `act-1-setup.md:360-362`) |
| **Mode** | **Straight to fidelity, in two parts** (the List and Form page shapes hold: BRIEF § 4): part 1, the Colleges, specialities, sub-specialities and scales; part 2, the EPAs and curricula, where the curriculum item editor needs a narrow-width layout |
| **Pages** (`coverage.md` templates, 21) | `/admin/colleges`, `/admin/colleges/new`, `/admin/colleges/{Id:int}`, `/admin/colleges/{CollegeId:int}/specialities`, `/admin/colleges/{CollegeId:int}/specialities/new`, `/admin/colleges/{CollegeId:int}/specialities/{Id:int}`, `/admin/specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities/new`, `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}`, `/admin/entrustment-scales`, `/admin/entrustment-scales/new`, `/admin/entrustment-scales/{Id:int}`, `/admin/epas`, `/admin/epas/new`, `/admin/epas/{Id:int}`, `/admin/curricula`, `/admin/curricula/new`, `/admin/curricula/{Id:int}`, `/admin/curricula/{Id:int}/items`, `/access-denied` |
| **Held screenshots** | None. T300 landed in 1e154ab and Dr Kruger's dashboard was re-captured on 2026-09-26 (§ Attach). T300 added Activity Types to his nav (flow 17). The T335 replay re-took every step capture of him on 2026-09-27, so their sidebars show it; his state captures other than the two Homes are older: brief them from the page, not the nav |

**How to run this thread** (BRIEF § 2.3, step 3; source keys are BRIEF § 2.0's):
1. Open a new thread in the Wombat design system. Attach the KEY SCREENSHOTS (§ Attach, first list).
2. Paste **The ask** below. Then paste **The runbook steps, verbatim** (the last section) as the next message: "Walk
   Claude through the journey and it will generate each screen in context" [academy].
3. Ask for part 1, pick, then ask for part 2 in the same thread. Add the other screenshots in § Attach as the chat asks.
4. Write every decision into the chat as a sentence; the chat travels in the handoff bundle [academy]. Export the chosen
   artboards to `design/flows/16-college-catalogue/` before moving on: there is no version history [start].

## The ask

Paste this block as the thread's first message.

```text
FLOW 16 — The College keeps the national catalogue: specialities, ladders, EPAs and curriculum versions.

GOAL: Let the platform Administrator keep the Colleges and the entrustment scales (the rating ladders). Let a College's
  administrator keep the College's specialities and sub-specialities (each with a default ladder), its EPAs and its
  curriculum versions: correct an EPA's wording; pause an EPA while the College revises it and restore it; start a new
  curriculum and hold it back from adoption; clone a version, change its items and release it. Tell the College's
  administrator clearly which pages are the Administrator's, before he meets Access denied. The blast radius is high: a
  curriculum item's target and its period are read live in every institution that has adopted the curriculum,
  including periods already closed. What is wrong today:
  - at 390 px the curriculum item editor opens as a 796 px row inside the items table, so the phone must scroll 490 px
    sideways and Save is out of view;
  - "no items" says every EPA is already on the curriculum when the discipline has none, over an empty table;
  - Deactivate on a College, speciality or sub-speciality, and Delete on a scale, act at once without asking;
  - the scale editor calls a rung "Level 6" where the College prints "5", and names a curriculum item by its id;
  - the College's administrator is offered "Back to colleges", a page that refuses him;
  - nothing shows how far a target change or a pause reaches before it is saved.

AUDIENCE: CollegeAdmin Dr Anton Kruger, College of Paediatricians of South Africa (CPSA). The platform Administrator.
  A few times a year, about one catalogue version a year; high stakes. Desktop 1280×800 and phone 390×844.

LAYOUT: Signed-in shell. Colleges, Specialities, Sub-specialities, Entrustment Scales, EPAs and Curricula are List pages
  (a table in a container, row actions, a create action in the page header). Their records are Form pages. A scale's
  page edits its rungs as an ordered list. A curriculum's page carries a card for cloning it. A curriculum's items page
  is a nine-column table with an edit row per item and an add form below. The College's administrator's home is a
  Dashboard page with one card.

CONTENT (use these real scenario values):
  - Colleges: College of Paediatricians of South Africa (CPSA), "Constituent College of the Colleges of Medicine of
    South Africa; owns the national Paediatric EPA catalogue."; College of Neurologists of South Africa (CNSA); Demo
    College (DEMO-C).
  - CPSA: speciality Paediatrics ("Specialist training in Paediatrics."); sub-specialities Paediatrics ("General
    paediatric specialist training programme.", default ladder CPSA Paediatric Entrustment Scale v11.1) and Neonatology.
    CNSA: Neurology, and Adult Neurology (no default at first: "No default — offer every scale").
  - The CPSA ladder: six rungs labelled 1, 2, 3a, 3b, 4, 5, from "Not entrusted to act…" to "Supervises others…". Level
    3 is split, so a rung's label is not its rank: 4 is the fifth rung and 5 the sixth. The scale editor is the only
    place that shows the rank. The CNSA ladder: Observe only, Direct supervision, Indirect supervision, Distant
    supervision, Unsupervised practice, Supervises others. The O-R Scale has 5.
  - EPAs: PAED-001 to PAED-016, e.g. PAED-006 "Managing long-term health conditions (LTHCs) in children", PAED-012
    "Communicating with and counselling patients, caregivers and healthcare teams" (paused, then restored), PAED-016
    "Transitioning adolescents with long-term conditions to adult care" (new).
  - Curricula: Paediatric EPA Curriculum 11.1 (15 items, effective 2026-01-01), 11.2 (16 items: adds PAED-016, and
    PAED-011 at 2 per academic year), Neonatology EPA Curriculum 1.0 (no items, held back).
  - An item: PAED-002, "3 per semester" above "Decided each semester", CPSA ladder, minimum 5, by training year 3a / 3b
    / 4 / 5, completion window 12 months, weight —, tools CBD, CCA, Chart-stimulated recall, Clinical audit, Direct
    observation, DOPS, Mini-CEX, MSF, RCA.
  - Reach: Kgosi Kgari Teaching Hospital (KGK) adopted 11.1 and then 11.2, with five registrars.

SCREENS, in order. Part 1:
  1. / — the College's administrator's home: one "National catalogue" card linking Specialities, EPAs and Curricula;
     desktop and 390 px.
  2. /admin/colleges, /admin/colleges/new, /admin/colleges/{Id} (the Administrator's) — list; create; edit; required
     fields empty (messages name the field's label, "Short code"); duplicate refused ("A college with the same name or
     short code already exists."); saved; deactivated, after a dialog that asks first; not found; 390 px.
  3. Specialities (/admin/colleges/{CollegeId}/specialities) and sub-specialities
     (/admin/specialities/{SpecialityId}/sub-specialities) — each list: one, several, none yet (inviting the first),
     another College's id, loading, 390 px; each edit form: create, edit, required field empty, saved, deactivated
     (after asking), not found. The sub-speciality's default ladder picker, with "No default — offer every scale" and
     its help ("Committee STARs for this programme's trainees are limited to this scale's levels.").
  4. /admin/entrustment-scales and a scale (the Administrator's) — the list (Create, Edit and Delete for him); Delete
     asks first, naming the scale, and is refused with what still uses it; deleted; a scale's rungs: edit, add a rung,
     duplicate labels refused ("Level labels must be unique within a scale."), a rung a curriculum item still needs
     refused (naming the item and the rung by its label), saved, unknown id; 390 px.
  7. /access-denied — as Dr Kruger meets it on the Colleges record and on creating a scale; and how the pages he can
     open tell him beforehand that the scales and the College's own record are the Administrator's.
  Part 2:
  5. /admin/epas and an EPA — the College's list, one inactive, loading, load error; an EPA: edit, saved, the pause
     dialog ("Deactivate this EPA?": it can no longer be chosen for a new activity; it stops being a target on every
     progress page and dashboard, for every institution that uses it; it keeps the progress it has earned; it credits
     nothing for an activity completed while inactive, until reactivated), paused, restored ("EPA reactivated. 1
     activity completed against it while it was inactive now counts towards progress."), create, duplicate code
     refused, required fields empty; 390 px.
  6. /admin/curricula, a curriculum and its items — the list with versions (active and held back); create; saved;
     the clone card (the new version starts as "11.1-copy"); cloned; required fields empty; not found. The items
     editor: the College's items under the live-target warning; a row being edited; an inactive EPA's item ("(inactive:
     not in force)" and its notice); no items and nothing to add; loading; not found; and at 390 px, with the item
     editor open and Save in view.

STEPS: 1.2–1.4, 1.12–1.18, 6.1–6.8, 6.10–6.14, 6.17–6.19, 6.23, 6.24, 6.28–6.31 and A.7.10 (33 steps), pasted verbatim
  in my next message (Role / Route / Do / Expect). Walk them in order; each Expect is what the screen must let the
  person see or do.

STATES TO SHOW, beyond those above: every list's empty and load-error states; every edit page's not found. Data
  volumes: one College and five; a ladder of 5 rungs and of 8; an EPA list of 15 and of 60; an items table of 0, 16 and
  40 rows; a curriculum with one version and with four.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
  - T323: at 390 px the curriculum item editor fits the width with Save in view, and nothing scrolls sideways (today
    the edit row is 796 px wide inside a 291 px container).
  - T326: "no items" does not claim every EPA is already on the curriculum when there are none; it says a national EPA
    of the discipline must be added first, and the table is not drawn with an empty body.
  - T331: on the College's specialities list, reached from the nav's Specialities, that nav item is the active one.
  - T324: the scale editor names a rung by its own label ("5"), never "Level 6", and a curriculum item by its EPA,
    never its id. Required-field messages name the field's label ("Short code"), not the model property ("ShortCode").
  - T264: Deactivate on a College, speciality or sub-speciality asks first in a dialog naming the target, as Delete on a
    scale does; the trigger is an outline button. Nothing reads their Active flag yet, so the dialog must not promise an
    effect (a decision is pending).
  - T329: the EPA list's load error replaces its skeleton; the alert shows with no empty state under it.
  - T291: a College's administrator is never offered "Back to colleges"; a page offers only what its target admits.
  - T300 (landed in 1e154ab): Activity Types is in the College's administrator's nav section, after Specialities, EPAs
    and Curricula (the builder itself is flow 17). Draw his sidebar with it; only the re-captured dashboard shows it.
  - T301: a curriculum version's box reads "Open for adoption" and the list column "Open" / "Held back", with the help
    "An institution can adopt this version only while it is open. Closing it does not affect an institution that has
    already adopted it." A new or cloned version starts held back.
  - T272: a failed save says what failed; "already exists" only for a real duplicate; no raw database text.
  - T242: adding nationally an EPA that an institution already holds as its own item stays refused, and the refusal
    names the institution (the recommended option; the operator may choose otherwise).
  - Every dialog names its target, its trigger is an outline button, and the result takes the focus; a refusal keeps
    what was typed. States and categories by label. At 390 px nothing scrolls sideways; tables scroll inside their
    containers.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Is the College's catalogue one workspace (speciality > sub-speciality > curriculum > items), with a tree or
     breadcrumbs, or separate lists as today?
  2. How is the blast radius shown before the College saves a target or pauses an EPA: how many institutions have
     adopted the curriculum, and how many registrars it measures?
  3. At 390 px, does the item editor become a card per item, or open one item at a time?

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
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Still to design where a flow meets
  them: field validation (invalid border plus a stripe, not colour alone; message under the field; the summary) and
  session ended.
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.

ASK:
  - FULL FIDELITY, 2–3 variations, in two parts. Part 1: screens 1, 2, 3, 4 and 7. Stop for my pick. Part 2: screens 5
    and 6, in the variation I picked, with the item editor designed for 390 px first.
  - Name every design-system component you use and mark anything else NEW. Say which Wombat rule a variation breaks.
  - Show the empty, error and loading states and the data volumes above. Review each screen for WCAG 2.1 AA: contrast,
    focus, targets of at least 24 px, and colour never the only signal (Active and Inactive written in words).
  - Ask me before assuming anything the steps leave open.

ATTACHED: states/colleges-list--two.png, states/sub-speciality-edit--edit.png, states/entrustment-scale-edit--edit.png,
  states/entrustment-scales-list--delete-refused.png, states/epas-list--college.png,
  states/epa-edit--deactivate-dialog.png, states/epa-edit--reactivated.png, states/curriculum-items-edit--college.png,
  states/curriculum-edit--clone.png, states/curricula-list--versions.png, act-A/A.7.10-4-items-390.png,
  act-A/A.7.10-5-item-editor-390.png, states/curriculum-items-edit--no-items.png,
  states/entrustment-scale-edit--rung-refused.png. These show today's pages, defects included; they are not the target.
  In Dr Kruger's captures the sidebar is out of date (it lacks Activity Types).
```

## The journey

One line per step, in play order: who does what, and what the screen must let them see. The full steps are in the last
section.

| Step | Page | Who does what | What they must be able to see |
|---|---|---|---|
| 1.2 | `/admin/colleges` | `devadmin` reads the Colleges | Two rows, each Active, with Edit and Specialities; Create college |
| 1.3 | `/admin/colleges` → `/admin/colleges/{Id:int}` | He reads the CPSA's record and leaves | "Edit college"; the fields; Deactivate and Save; leaving saves nothing |
| 1.4 | `/admin/entrustment-scales` → `/admin/entrustment-scales/{Id:int}` | He reads the CPSA ladder | Two scales with their level counts; six rungs labelled 1 to 5 with 3a and 3b, each with its descriptor, and the rank in its own column |
| 1.12 | `/admin/specialities` → `/admin/colleges/{CollegeId:int}/specialities` | Dr Kruger opens Specialities | His own College's list, one row, Create speciality, and no link to the Colleges list (T291) |
| 1.13 | `/admin/specialities/{SpecialityId:int}/sub-specialities` | He opens Paediatrics' sub-specialities | One row; Back to specialities; Create sub-speciality |
| 1.14 | `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | He reads the sub-speciality | The default ladder, the other choices, and the help on what the default limits |
| 1.15 | `/admin/entrustment-scales` → `/access-denied` | He types the scales address | Access denied; beforehand, that the ladders are the Administrator's and where he reads them by name |
| 1.16 | `/admin/epas` | He reads the EPAs | 15 rows, the College's alone, each Active with Edit; Create EPA |
| 1.17 | `/admin/curricula` | He reads the curricula | 11.1, effective 2026-01-01, 15 items, Active, Edit and Items; Create curriculum |
| 1.18 | `/admin/curricula/{Id:int}/items` | He reads 11.1's items | The live-target warning; 15 rows; the target above the decision; the Add form saying nothing is left to add |
| 6.1 | `/admin/colleges` → `/admin/colleges/new` → `/admin/colleges/{Id:int}` → `/admin/colleges` | `devadmin` creates CNSA, first with a duplicate code | The duplicate refused, nothing created; then the new College and three rows |
| 6.2 | `/admin/colleges` → `/admin/colleges/{Id:int}` → `/admin/colleges` | He corrects CPSA's description | Saved; the list's new description |
| 6.3 | `/admin/colleges/{CollegeId:int}/specialities` → `…/new` → `…/{Id:int}` → list | He adds Neurology | An empty list inviting the first; the new speciality and "Manage sub-specialities" |
| 6.4 | `/admin/specialities/{SpecialityId:int}/sub-specialities` → `…/new` → `…/{Id:int}` → list | He adds Adult Neurology | An empty list, then one row; the default ladder reading "No default — offer every scale" |
| 6.5 | `/admin/entrustment-scales` → `…/new` → `…/{Id:int}` → list | He creates the CNSA ladder, first with a duplicate label | "Level labels must be unique within a scale."; then five rungs in order; three scales |
| 6.6 | `/admin/entrustment-scales` → `…/{Id:int}` → list | He adds a sixth rung | Six rungs; the list reads 6 |
| 6.7 | `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | He sets Adult Neurology's default | Saved and still selected on reopening |
| 6.8 | `/admin/entrustment-scales` | He deletes the CNSA ladder | A dialog naming it first (T264); the refusal naming the sub-speciality that uses it |
| 6.10 | `/account/login` → `/` → `/admin/colleges/{Id:int}` → `/access-denied` → `/admin/entrustment-scales/new` → `/access-denied` | Dr Kruger signs in and types two Administrator addresses | His one-card dashboard; Access denied twice, with Back to home |
| 6.11 | `/admin/specialities` → `/admin/colleges/{CollegeId:int}/specialities` → `…/{Id:int}` → list | He corrects Paediatrics' description | CPSA's list; saved; no way to the Colleges list |
| 6.12 | `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` → sub-specialities → new → `{Id}` → list | He adds Neonatology with the CPSA ladder as default | Two sub-specialities; the default saved |
| 6.13 | `/admin/curricula` → `/admin/curricula/new` → `/admin/curricula/{Id:int}` → list → items | He starts Neonatology 1.0 and holds it back | The clone card; held back (after T301, "Open for adoption" unticked); an items page that says a national EPA must be added first (T326) |
| 6.14 | `/admin/epas` → `/admin/epas/{Id:int}` → `/admin/epas` | He corrects PAED-006's title | Saved; the new title, read live everywhere |
| 6.17 | `/admin/epas` → `/admin/epas/{Id:int}` → `/admin/epas` → `/admin/curricula` → items | He pauses PAED-012 | The dialog's four consequences (and its reach, question 2); Inactive; the item marked "(inactive: not in force)" with its notice |
| 6.18 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel completes a Mini-CEX on PAED-012 during the pause | "(no longer in use)"; completion allowed; it credits nothing, and the page says why |
| 6.19 | `/portfolio/progress` → `/activities/mine` → `/activities/new` | Dr Dlamini during the pause | No PAED-012 card; its trajectory "(no longer in use)"; the picker without it |
| 6.23 | `/admin/epas` → `/admin/epas/{Id:int}` → `/admin/epas` | Dr Kruger restores PAED-012 | "EPA reactivated. 1 activity … now counts towards progress."; Active |
| 6.24 | `/portfolio/progress` → `/activities/mine` | Dr Dlamini after the restore | The card back with the paused completion counted; the marker gone |
| 6.28 | `/admin/epas` → `/admin/epas/new` → `/admin/epas/{Id:int}` → `/admin/epas` | He adds PAED-016 | CPSA's two sub-specialities offered; PAED-016 Active; no institution's own EPA in his list |
| 6.29 | `/admin/curricula` → items → `/admin/curricula/{Id:int}` → `/admin/curricula` | He clones 11.1 as 11.2 and holds it back | 15 items (not KGK's own); "11.1-copy"; after T301 the clone starts held back |
| 6.30 | `/admin/curricula` → `/admin/curricula/{Id:int}/items` | He adds PAED-016 and edits PAED-011 on 11.2 | The Add form offering PAED-016 alone; 16 rows; the edit row beneath PAED-011; "2 per academic year" |
| 6.31 | `/admin/curricula` → `/admin/curricula/{Id:int}` → `/admin/curricula` | He releases 11.2 | Saved; 11.1 and 11.2 open, 11.2 at 16 items, Neonatology still held back |
| A.7.10 | `/` → `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` | Dr Kruger at 390 px | Tables scrolling in their containers; row actions reachable; the item editor usable without sideways scrolling (T323) |

## States to design

84 captures, from `execution/knowledge/scenario-paediatrics/states.md`. The line number is the row in `states.md`.
**HELD** means the capture predates a group-1 fix and must be re-captured before it is briefed from (BRIEF § 10).

| Page slug | State | Screenshot (`design/baseline/…`) | Who | How it is reached (`states.md` line) |
|---|---|---|---|---|
| `home` | CollegeAdmin | `states/home--college-admin.png` (re-captured after T300) | Dr Kruger | At Step 1.8: the one National catalogue card. (:115) |
| `home` | Narrow: college admin | `states/home--narrow-college-admin.png` (re-captured after T300) | Dr Kruger | At Step A.7.10. (:147) |
| `colleges-list` | Two Colleges | `states/colleges-list--two.png` | devadmin | At Step 1.2. (:481) |
| `colleges-list` | Three Colleges | `states/colleges-list--three.png` | devadmin | At Step 6.1. (:482) |
| `colleges-list` | Loading | `states/colleges-list--loading.png` | devadmin | Hold a read, then Colleges in the nav. No change. (:483) |
| `colleges-list` | Narrow | `states/colleges-list--narrow.png` | devadmin | At Step 6.2, at 390 px. No change. (:484) |
| `college-edit` | Edit | `states/college-edit--edit.png` | devadmin | At Step 1.3. (:485) |
| `college-edit` | Create | `states/college-edit--create.png` | devadmin | At Step 6.1, before typing. (:486) |
| `college-edit` | Required fields empty | `states/college-edit--invalid.png` | devadmin | At Step 6.1, Save with the form empty: the fields' messages. Nothing is sent. No change. (:487) |
| `college-edit` | Duplicate refused | `states/college-edit--duplicate.png` | devadmin | At Step 6.1, the first save. (:488) |
| `college-edit` | Saved | `states/college-edit--saved.png` | devadmin | At Step 6.2: "College saved." (:489) |
| `college-edit` | Deactivated | `states/college-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on CNSA: "College deactivated." (:490) |
| `college-edit` | Unknown id | `states/college-edit--not-found.png` | devadmin | Typed: `/admin/colleges/999999`: Page not found. No change. (:491) |
| `college-edit` | Narrow | `states/college-edit--narrow.png` | devadmin | At Step 6.2, at 390 px, before saving. No change. (:492) |
| `specialities-list` | One speciality | `states/specialities-list--one.png` | Dr Kruger | At Step 1.12. (:494) |
| `specialities-list` | None yet | `states/specialities-list--empty.png` | devadmin | At Step 6.3, before the create. (:495) |
| `specialities-list` | Another College's id | `states/specialities-list--other-college.png` | Dr Kruger | Typed with the Demo College's id (`SELECT "Id" FROM "Colleges" WHERE "ShortCode" = 'DEMO-C';`): reads as a College with no specialities. No change. (:496) |
| `specialities-list` | Loading | `states/specialities-list--loading.png` | Dr Kruger | Hold a read, then Specialities in the nav. No change. (:497) |
| `specialities-list` | Narrow | `states/specialities-list--narrow.png` | Dr Kruger | At Step 6.11, at 390 px. No change. (:498) |
| `speciality-edit` | Create | `states/speciality-edit--create.png` | devadmin | At Step 6.3, before typing. (:501) |
| `speciality-edit` | Edit | `states/speciality-edit--edit.png` | Dr Kruger | At Step 6.11, before saving. (:499) |
| `speciality-edit` | Required field empty | `states/speciality-edit--invalid.png` | devadmin | At Step 6.3, Save with the form empty. No change. (:502) |
| `speciality-edit` | Saved | `states/speciality-edit--saved.png` | Dr Kruger | At Step 6.11: "Speciality saved." (:500) |
| `speciality-edit` | Deactivated | `states/speciality-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on Neurology: "Speciality deactivated." (:503) |
| `speciality-edit` | Unknown id | `states/speciality-edit--not-found.png` | Dr Kruger | Typed with his College's id and `999999`: Page not found. No change. (:504) |
| `speciality-edit` | Narrow | `states/speciality-edit--narrow.png` | Dr Kruger | At Step 6.11, at 390 px. No change. (:505) |
| `sub-specialities-list` | One | `states/sub-specialities-list--one.png` | Dr Kruger | At Step 1.13. (:506) |
| `sub-specialities-list` | Two | `states/sub-specialities-list--two.png` | Dr Kruger | At Step 6.12. (:508) |
| `sub-specialities-list` | None yet | `states/sub-specialities-list--empty.png` | devadmin | At Step 6.4, before the create. (:507) |
| `sub-specialities-list` | Loading | `states/sub-specialities-list--loading.png` | Dr Kruger | Hold a read, then Sub-specialities on the speciality's row. No change. (:509) |
| `sub-specialities-list` | Narrow | `states/sub-specialities-list--narrow.png` | Dr Kruger | At Step 6.12, at 390 px. No change. (:510) |
| `sub-speciality-edit` | Create | `states/sub-speciality-edit--create.png` | devadmin | At Step 6.4, before typing. (:512) |
| `sub-speciality-edit` | With a default ladder | `states/sub-speciality-edit--edit.png` | Dr Kruger | At Step 1.14. (:511) |
| `sub-speciality-edit` | No default | `states/sub-speciality-edit--no-default.png` | devadmin | At Step 6.4, after the create. (:514) |
| `sub-speciality-edit` | Required field empty | `states/sub-speciality-edit--invalid.png` | devadmin | At Step 6.4, Save with the form empty. No change. (:513) |
| `sub-speciality-edit` | Default saved | `states/sub-speciality-edit--saved.png` | devadmin | At Step 6.7. (:515) |
| `sub-speciality-edit` | Deactivated | `states/sub-speciality-edit--deactivated.png` | devadmin | Scratch (post-appendix): Deactivate on Adult Neurology. (:516) |
| `sub-speciality-edit` | Unknown id | `states/sub-speciality-edit--not-found.png` | Dr Kruger | Typed with the Paediatrics speciality's id and `999999`: Page not found. No change. (:517) |
| `sub-speciality-edit` | Narrow | `states/sub-speciality-edit--narrow.png` | Dr Kruger | At Step 6.12, at 390 px. No change. (:518) |
| `entrustment-scales-list` | An administrator's | `states/entrustment-scales-list--administrator.png` | devadmin | At Step 1.4. (:565) |
| `entrustment-scales-list` | Delete refused | `states/entrustment-scales-list--delete-refused.png` | devadmin | At Step 6.8. (:567) |
| `entrustment-scales-list` | Deleted | `states/entrustment-scales-list--deleted.png` | devadmin | Scratch (post-appendix): create a two-level scale (a scale needs at least two levels), back on the list press Delete on it: "Entrustment scale deleted." (:568) |
| `entrustment-scales-list` | Loading | `states/entrustment-scales-list--loading.png` | Prof Mbatha | Hold a read, then Entrustment Scales in the nav. No change. (:569) |
| `entrustment-scales-list` | Narrow | `states/entrustment-scales-list--narrow.png` | Prof Mbatha | At Step 6.9, at 390 px. No change. (:570) |
| `entrustment-scale-edit` | Create | `states/entrustment-scale-edit--create.png` | devadmin | At Step 6.5, before typing. (:572) |
| `entrustment-scale-edit` | Six rungs | `states/entrustment-scale-edit--edit.png` | devadmin | At Step 1.4. (:571) |
| `entrustment-scale-edit` | Duplicate labels refused | `states/entrustment-scale-edit--duplicate.png` | devadmin | At Step 6.5, the first save. (:573) |
| `entrustment-scale-edit` | Saved | `states/entrustment-scale-edit--saved.png` | devadmin | At Step 6.6. (:574) |
| `entrustment-scale-edit` | A rung still needed | `states/entrustment-scale-edit--rung-refused.png` | devadmin | Scratch (post-appendix): on the CPSA ladder remove `5` and save: "Curriculum item N is pinned to this entrustment scale and requires level 6, …". (:575) |
| `entrustment-scale-edit` | Unknown id | `states/entrustment-scale-edit--unknown.png` | devadmin | Typed: `/admin/entrustment-scales/999999`: it returns to the scales list. No change. (:576) |
| `entrustment-scale-edit` | Narrow | `states/entrustment-scale-edit--narrow.png` | devadmin | At Step 6.6, at 390 px, before saving. No change. (:577) |
| `epas-list` | The College's fifteen | `states/epas-list--college.png` | Dr Kruger | At Step 1.16. (:520) |
| `epas-list` | One inactive | `states/epas-list--inactive.png` | Dr Kruger | At Step 6.17. (:522) |
| `epas-list` | Load error | `states/epas-list--load-error.png` | Dr Kruger | The same, held 35 s. No change. (:525) |
| `epas-list` | Loading | `states/epas-list--loading.png` | Dr Kruger | Hold a read, then EPAs in the nav. No change. (:524) |
| `epas-list` | Narrow | `states/epas-list--narrow.png` | Dr Kruger | At Step A.7.10. (:526) |
| `epa-edit` | Edit | `states/epa-edit--edit.png` | Dr Kruger | At Step 6.14, before saving. (:527) |
| `epa-edit` | Saved | `states/epa-edit--saved.png` | Dr Kruger | At Step 6.14. (:528) |
| `epa-edit` | Deactivate dialog | `states/epa-edit--deactivate-dialog.png` | Dr Kruger | At Step 6.17. (:529) |
| `epa-edit` | Deactivated | `states/epa-edit--deactivated.png` | Dr Kruger | At Step 6.17, after confirming. (:530) |
| `epa-edit` | Reactivated | `states/epa-edit--reactivated.png` | Dr Kruger | At Step 6.23: "EPA reactivated. 1 activity … now counts towards progress." (:531) |
| `epa-edit` | Create, the College | `states/epa-edit--create-national.png` | Dr Kruger | At Step 6.28, before saving. (:533) |
| `epa-edit` | Duplicate code refused | `states/epa-edit--duplicate.png` | Dr Kruger | Scratch (post-appendix): create `PAED-001` again in Paediatrics / Paediatrics. (:535) |
| `epa-edit` | Required fields empty | `states/epa-edit--invalid.png` | Dr Kruger | At Step 6.28, Save with the form empty. No change. (:534) |
| `epa-edit` | Narrow | `states/epa-edit--narrow.png` | Dr Kruger | At Step 6.14, at 390 px. No change. (:538) |
| `curricula-list` | The College's | `states/curricula-list--college.png` | Dr Kruger | At Step 1.17. (:540) |
| `curricula-list` | Active and inactive | `states/curricula-list--versions.png` | Dr Kruger | At Step 6.31. (:542) |
| `curricula-list` | Loading | `states/curricula-list--loading.png` | Dr Kruger | Hold a read, then Curricula in the nav. No change. (:544) |
| `curricula-list` | Narrow | `states/curricula-list--narrow.png` | Dr Kruger | At Step A.7.10. (:545) |
| `curriculum-edit` | Create | `states/curriculum-edit--create.png` | Dr Kruger | At Step 6.13, before typing. (:546) |
| `curriculum-edit` | Saved, inactive | `states/curriculum-edit--saved.png` | Dr Kruger | At Step 6.13. (:548) |
| `curriculum-edit` | Edit, with the clone card | `states/curriculum-edit--clone.png` | Dr Kruger | At Step 6.29, before cloning. (:549) |
| `curriculum-edit` | Cloned | `states/curriculum-edit--cloned.png` | Dr Kruger | At Step 6.29, the new version's page. (:550) |
| `curriculum-edit` | Required fields empty | `states/curriculum-edit--invalid.png` | Dr Kruger | At Step 6.13, Save with the form empty. No change. (:547) |
| `curriculum-edit` | Unknown id | `states/curriculum-edit--not-found.png` | Dr Kruger | Typed: `/admin/curricula/999999`: Page not found. No change. (:551) |
| `curriculum-edit` | Narrow | `states/curriculum-edit--narrow.png` | Dr Kruger | At Step 6.31, at 390 px, before saving. No change. (:552) |
| `curriculum-items-edit` | The College's items | `states/curriculum-items-edit--college.png` | Dr Kruger | At Step 1.18: the live-target warning, "Every national EPA … is already on this curriculum". (:553) |
| `curriculum-items-edit` | No items, nothing to add | `states/curriculum-items-edit--no-items.png` | Dr Kruger | At Step 6.13. (:555) |
| `curriculum-items-edit` | An inactive EPA | `states/curriculum-items-edit--inactive.png` | Dr Kruger | At Step 6.17: "(inactive: not in force)" and its notice. (:556) |
| `curriculum-items-edit` | A row being edited | `states/curriculum-items-edit--row-edit.png` | Dr Kruger | At Step 6.30, PAED-011's edit row open. (:559) |
| `curriculum-items-edit` | Loading | `states/curriculum-items-edit--loading.png` | Dr Kruger | Hold a read, then Items on a curriculum. No change. (:563) |
| `curriculum-items-edit` | Out of scope | `states/curriculum-items-edit--not-found.png` | Prof Mbatha | Typed with the Demo `IM Core Curriculum`'s id: Page not found. No change. (:562) |
| `curriculum-items-edit` | Narrow | `states/curriculum-items-edit--narrow.png` | Dr Kruger | At Step A.7.10. (:564) |
| `access-denied` | Signed in | `states/access-denied--signed-in.png` | Dr Kruger | At Step 1.15. (:154) |

These captures show today's behaviour where the state should differ. Design the state, not the capture:
- **`states/specialities-list--other-college.png`**: another College's id reads as a College with no specialities.
  BRIEF § 5.1's content rule is that an out-of-scope record is "not found"; design it so unless the operator decides
  otherwise.
- **`states/entrustment-scale-edit--unknown.png`**: an unknown scale id silently returns to the list. BRIEF § 6 A6 asks
  every page for a designed not-found state.
- **`states/college-edit--deactivated.png`**, **`states/speciality-edit--deactivated.png`** and
  **`states/sub-speciality-edit--deactivated.png`**: deactivated with no question first (T264), and with no effect
  anywhere yet (`coverage.md` § Flows and states not played).
- **`states/curriculum-items-edit--no-items.png`**: the false "already on this curriculum" line over an empty table
  (T326).

Three states have no capture. Design them from the requirements:
- **The item editor at 390 px with Save in view** (T323).
- **The deactivate dialog** on a College, speciality and sub-speciality (T264).
- **The reach of a change** before a target is saved or an EPA paused (question 2).

## Attach

Every path is under `design/baseline/` and was checked to exist on 2026-09-26. Open each before uploading (BRIEF
§ 3.3). None of these shows a registration link or a password; the invitation captures that do (`act-1/1.5-2-…`) are
not in this flow.

**First, with the ask (the KEY SCREENSHOTS):**
- `states/colleges-list--two.png`
- `states/sub-speciality-edit--edit.png`
- `states/entrustment-scale-edit--edit.png`
- `states/entrustment-scales-list--delete-refused.png`
- `states/epas-list--college.png`
- `states/epa-edit--deactivate-dialog.png`
- `states/epa-edit--reactivated.png`
- `states/curriculum-items-edit--college.png`
- `states/curriculum-edit--clone.png`
- `states/curricula-list--versions.png`
- `act-A/A.7.10-4-items-390.png` (the items table at 390 px; brief from the table, not the sidebar)
- `act-A/A.7.10-5-item-editor-390.png` (the item editor open in it)
- `states/curriculum-items-edit--no-items.png`
- `states/entrustment-scale-edit--rung-refused.png`

**Then, as the chat asks. Part 1:**
- Colleges: `states/colleges-list--three.png`, `states/colleges-list--loading.png`, `states/colleges-list--narrow.png`,
  `states/college-edit--edit.png`, `states/college-edit--create.png`, `states/college-edit--invalid.png`,
  `states/college-edit--duplicate.png`, `states/college-edit--saved.png`, `states/college-edit--deactivated.png`,
  `states/college-edit--not-found.png`, `states/college-edit--narrow.png`, `act-1/1.2-1-colleges-list.png`,
  `act-1/1.3-1-cpsa-record.png`, `act-6/6.1-1-duplicate-refused.png`, `act-6/6.1-3-three-colleges.png`,
  `act-6/6.2-1-cpsa-saved.png`.
- Specialities and sub-specialities: `states/specialities-list--one.png`, `states/specialities-list--empty.png`,
  `states/specialities-list--other-college.png`, `states/specialities-list--loading.png`,
  `states/specialities-list--narrow.png`, `states/speciality-edit--create.png`, `states/speciality-edit--edit.png`,
  `states/speciality-edit--invalid.png`, `states/speciality-edit--saved.png`, `states/speciality-edit--deactivated.png`,
  `states/speciality-edit--not-found.png`, `states/speciality-edit--narrow.png`,
  `states/sub-specialities-list--one.png`, `states/sub-specialities-list--two.png`,
  `states/sub-specialities-list--empty.png`, `states/sub-specialities-list--loading.png`,
  `states/sub-specialities-list--narrow.png`, `states/sub-speciality-edit--create.png`,
  `states/sub-speciality-edit--no-default.png`, `states/sub-speciality-edit--invalid.png`,
  `states/sub-speciality-edit--saved.png`, `states/sub-speciality-edit--deactivated.png`,
  `states/sub-speciality-edit--not-found.png`, `states/sub-speciality-edit--narrow.png`,
  `act-1/1.12-1-kruger-specialities.png`, `act-1/1.13-1-sub-specialities.png`, `act-1/1.14-1-sub-speciality-ladder.png`,
  `act-6/6.3-1-no-specialities.png`, `act-6/6.4-2-adult-neurology-created.png`, `act-6/6.7-2-default-reopened.png`,
  `act-6/6.11-1-cpsa-specialities.png`, `act-6/6.12-2-two-sub-specialities.png`.
- Scales: `states/entrustment-scales-list--administrator.png`, `states/entrustment-scales-list--deleted.png`,
  `states/entrustment-scales-list--loading.png`, `states/entrustment-scales-list--narrow.png`,
  `states/entrustment-scale-edit--create.png`, `states/entrustment-scale-edit--duplicate.png`,
  `states/entrustment-scale-edit--saved.png`, `states/entrustment-scale-edit--unknown.png`,
  `states/entrustment-scale-edit--narrow.png`, `act-1/1.4-1-scales-list.png`, `act-1/1.4-2-cpsa-ladder.png`,
  `act-6/6.5-1-duplicate-labels-refused.png`, `act-6/6.6-1-sixth-level-saved.png`, `act-6/6.8-1-delete-refused.png`.
- Access denied: `states/access-denied--signed-in.png`, `act-1/1.15-1-kruger-access-denied.png`,
  `act-6/6.10-2-college-record-refused.png`, `act-6/6.10-3-scale-create-refused.png`.

**Part 2:**
- EPAs: `states/epas-list--inactive.png`, `states/epas-list--load-error.png`, `states/epas-list--loading.png`,
  `states/epas-list--narrow.png`, `states/epa-edit--edit.png`, `states/epa-edit--saved.png`,
  `states/epa-edit--deactivated.png`, `states/epa-edit--create-national.png`, `states/epa-edit--duplicate.png`,
  `states/epa-edit--invalid.png`, `states/epa-edit--narrow.png`, `act-1/1.16-1-college-epas.png`,
  `act-6/6.14-1-paed-006-saved.png`, `act-6/6.17-1-deactivate-dialog.png`, `act-6/6.17-2-deactivated.png`,
  `act-6/6.23-1-reactivated.png`, `act-6/6.28-1-paed016-saved.png`, `act-6/6.28-2-kruger-epa-list.png`.
- The pause as others meet it: `act-6/6.18-1-patel-inbox-paused.png`, `act-6/6.18-2-completed-credited-nothing.png`,
  `act-6/6.19-1-dlamini-progress.png`, `act-6/6.19-3-dlamini-picker.png`, `act-6/6.24-1-dlamini-progress.png`.
- Curricula: `states/curricula-list--college.png`, `states/curricula-list--loading.png`,
  `states/curricula-list--narrow.png`, `states/curriculum-edit--create.png`, `states/curriculum-edit--saved.png`,
  `states/curriculum-edit--cloned.png`, `states/curriculum-edit--invalid.png`, `states/curriculum-edit--not-found.png`,
  `states/curriculum-edit--narrow.png`, `act-1/1.17-1-college-curricula.png`, `act-6/6.13-2-saved-inactive.png`,
  `act-6/6.29-2-cloned-11.2-active.png`, `act-6/6.31-2-curricula-list.png`.
- Items: `states/curriculum-items-edit--inactive.png`, `states/curriculum-items-edit--row-edit.png`,
  `states/curriculum-items-edit--loading.png`, `states/curriculum-items-edit--not-found.png`,
  `states/curriculum-items-edit--narrow.png`, `act-1/1.18-1-college-items.png`, `act-6/6.13-4-items-nothing-to-add.png`,
  `act-6/6.17-4-items-paused.png`, `act-6/6.30-1-add-paed016-form.png`, `act-6/6.30-3-paed011-edit-row.png`,
  `act-6/6.30-4-paed011-saved.png` (Dr Kruger's items at 390 px are `A.7.10-4`, above).

**Re-captured after T300 landed (1e154ab, 2026-09-26), so no longer held:** `states/home--college-admin.png` (from a
post-act1 copy), `act-6/6.10-1-kruger-home.png` (post-act6), and `states/home--narrow-college-admin.png` and
`act-A/A.7.10-1-home-390.png` (post-actA); all four were re-taken on 2026-09-27 (the step captures by the T335
replay). The dashboard itself did not change: one National catalogue card. Since T335 his menu reads Home,
Specialities, EPAs, Curricula, Activity types, then My data rights (Step A.7.10), and the T335 replay re-took every
step capture of him with it; at 390 px the menu is folded (`A.7.10-4`, `A.7.10-5`).

## Known problems this design must solve

| Task | Priority | What it means for the design | Evidence |
|---|---|---|---|
| T323 | P3 | The item editor renders as a `colspan` row inside the items table's scroll container, so it takes the table's 796 px width; its two-column form (370 px inputs) spills off a 291 px column (F-A.7.10a). It needs its own narrow layout | `act-A/A.7.10-4-items-390.png`, `act-A/A.7.10-5-item-editor-390.png` |
| T326 | P3 | `CurriculumItemsEdit.razor` chooses the "every national EPA is already on this curriculum" text whenever the picker is empty, even with no EPA at all; the Existing items table renders its headers over an empty body (F-6.13a) | `states/curriculum-items-edit--no-items.png`, `act-6/6.13-4-items-nothing-to-add.png` |
| T331 | P3 | No nav item lights on `/admin/colleges/{n}/specialities` for the CollegeAdmin, whose nav item is `/admin/specialities` | `states/specialities-list--loading.png` |
| T324 | P3 | The rung refusal reads "Curriculum item N … requires level 6"; say the College's label ("5") and the item's EPA. Required messages read "The ShortCode field is required." (F-6.1a) | `states/entrustment-scale-edit--rung-refused.png`, `states/college-edit--invalid.png` |
| T264 | P3 | Deactivate on the College, speciality and sub-speciality pages, and Delete on the scales list, are red in-row or form buttons with no ConfirmDialog (F-6.8a). A deactivation's effect is undecided: nothing in Application or Infrastructure reads their `IsActive` (`coverage.md` § Flows and states not played) | `act-6/6.8-1-delete-refused.png`, `states/college-edit--deactivated.png` |
| T329 | P2 | The EPA list keeps its skeleton beside the load error | `states/epas-list--load-error.png` |
| T291 | P3 | Item 2: `SpecialitiesList.razor:10` shows "Back to colleges" to every caller; show it only to an Administrator (F-1.12a, F-6.11a) | `act-1/1.12-1-kruger-specialities.png`, `act-6/6.11-1-cpsa-specialities.png` |
| T300 | P3, group 1, landed in 1e154ab | Activity Types is in the CollegeAdmin's nav section, and the builder has a read-only mode, a narrowed Scope and View or Edit per row (flow 17; BRIEF § 10; D52). The dashboard captures were re-taken on 2026-09-26, and every step capture of him again on 2026-09-27 by the T335 replay | `states/home--college-admin.png` (re-captured) |
| T301 | P3 | "Open for adoption" and "Open" / "Held back"; a new or cloned version starts held back (6.29 today: "a clone is adoptable the moment it exists"); adoption refuses a held-back version | `states/curriculum-edit--clone.png`, `states/curricula-list--versions.png` |
| T272 | P3 | Only a real unique violation reads "already exists"; other failures read "could not be saved; try again"; no raw EF text | `states/college-edit--duplicate.png` |
| T271 | P3 | A scale delete and a publish that binds the scale can race (a lock, not a screen); the builder labels a scale bound by seed key (flow 17) | none in this flow |
| T242 | P3 | Recommended (c): the College's add of a national EPA that an institution holds as its own item stays refused, and the refusal names the institution | none (a decision) |

## Questions the design must answer

1. **One workspace or separate lists?** Today the College's administrator moves through five separate lists:
   Specialities (reached by a redirect from `/admin/specialities`, 1.12), Sub-specialities (1.13), EPAs (1.16),
   Curricula (1.17) and a curriculum's items (1.18). Each has its own Back link. A tree or breadcrumbs would make
   speciality > sub-speciality > curriculum > items one place.
2. **How is the blast radius shown before a save?** The items page warns in words that a target is read live in every
   adopting institution, closed periods included (1.18). The pause dialog says it applies "for every institution that
   uses it" (6.17). Neither says how many institutions or registrars that is. KGK alone had five registrars on 11.1.
3. **At 390 px, is the item editor a card per item, or does it open one item at a time?** Today it is a table row
   (A.7.10), and T323 requires Save in view.

## Notes

- **The Administrator's pages must say so before the College's administrator meets Access denied** (steps 1.15 and
  6.10). His nav offers neither the scales nor the Colleges (DESIGN.md:203, the CollegeAdmin row: "Specialities, EPAs,
  Curricula, Activity Types" since T300). He reads a ladder by name where the College uses it: the sub-speciality's default (1.14) and each item's
  Scale column (1.18).
- **A rung's label is not its rank** (D32; Step 1.4). The scale editor is the only page that shows the rank. Everywhere
  else, including a refusal, a rung is named by its label.
- **Pause, not delete.** A paused EPA keeps its STARs and earned progress and credits nothing while paused;
  reactivating it counts what was completed meanwhile (D48; CLAUDE.md § Activity platform, `CreditApplier`). Steps
  6.18, 6.19 and 6.24 show that from the assessor's and the trainee's side.

## Acceptance

After Claude Code builds the chosen design (BRIEF § 9), the flow is done when:
- **The steps replay on a fresh database.** Play Act 1 through **1.2–1.4** and **1.12–1.18**, then Acts 2–5, then Act 6
  through **6.1–6.8**, **6.10–6.14**, **6.17–6.19**, **6.23**, **6.24** and **6.28–6.31**, then the appendix's
  **A.7.10** (`README.md` § How to play; `tools/scenario-replay.ps1 create|publish|start wombat_scenario_f16`). Every
  Expect holds. Where the design changes wording, the steps' Expect lines change in the same task (BRIEF § 9, item 7).
  Expect to rewrite 1.12 and 6.11 (no Back to colleges), 6.1 (the field's label), 6.8 and the Deactivate steps (the
  dialogs), 6.13 and 6.29 (held back by "Open for adoption"; a clone starts held back), 6.13 (the no-items text), 6.10
  (his nav with Activity Types) and A.7.10 (the editor at 390 px).
- **The tests pass:** `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`, including
  `Admin/CurriculumItemsEditLayoutTests` and `Design/NarrowLayoutTests` (the item editor),
  `Admin/EpaEditDeactivationTests`, `Admin/EntrustmentScalesListActionsTests`, `Navigation/NavMenuAuthorizationTests`
  (the CollegeAdmin's nav), and T294's guard in `tests/Wombat.Web.Tests/Scenario/`.
- **The baseline is re-captured:** the 84 states above and the step captures of the 33 steps, compared with the chosen
  artboards.
- **The browser check passes** at 1280 and 390 px for Dr Kruger and `devadmin`, and for Dr Patel and Dr Dlamini on the
  pause steps.

## The runbook steps, verbatim

Paste this as the second message. It is each step's Role, Route, Do and Expect, copied from
`execution/knowledge/scenario-paediatrics/` (`act-1-setup.md`, `act-6-catalogue.md`, `appendix-cross-cutting.md`).
`D` is the replay day. Steps 1.16 and 1.18 point to tables of the 15 EPAs and their items; those tables are in
`act-1-setup.md` under each step and are not pasted here.

```text
Step 1.2 — The CPSA is among the Colleges (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/colleges
Do: Open Colleges from the nav and read the list. Create nothing.
Expect: The list has two rows, each Active and each offering Edit and Specialities:
  - `Demo College` (`DEMO-C`);
  - `College of Paediatricians of South Africa` (`CPSA`), described as "Constituent College of the Colleges of Medicine
    of South Africa; owns the national Paediatric EPA catalogue."
  The header offers Create college. No College is named "College of Paediatricians" alone.

Step 1.3 — The CPSA's own record (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/colleges → /admin/colleges/{Id:int}
Do: Open the CPSA's Edit and read the record. Leave by Back to colleges without saving. Record the CPSA's id as
  `{CollegeId}`.
Expect: The page is headed "Edit college", and the browser tab reads "Edit college · Wombat" (T190). Name, Short
  code and Description hold Step 1.2's values, and Active is ticked. Deactivate and Save are offered. Leaving saves
  nothing, and the list is unchanged.

Step 1.4 — The CPSA ladder has six rungs (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/entrustment-scales → /admin/entrustment-scales/{Id:int}
Do: Open Entrustment scales and read the list. Then open Edit on `CPSA Paediatric Entrustment Scale v11.1` and read its
  levels. Leave by Cancel without saving. Record the scale's id as `{ScaleId}`.
Expect: The list has two scales: `CPSA Paediatric Entrustment Scale v11.1` at 6 levels and the Demo `O-R Scale` at 5.
  The Administrator is offered Create scale, and Edit and Delete on each row.
  The CPSA ladder's levels, in order 1 to 6, are labelled `1`, `2`, `3a`, `3b`, `4` and `5`, each with its descriptor
  (from "Not entrusted to act…" to "Supervises others…"). Level 3 is split, so a rung's label is not its rank: `4` is
  the fifth rung and `5` is the sixth. Everything that compares levels uses the rank. This editor is the only place
  that shows the rank, in its own Order column (D32).

Step 1.12 — The Paediatrics speciality (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities → /admin/colleges/{CollegeId:int}/specialities
Do: Open Specialities from the nav and read the page. Record the Paediatrics speciality's id as `{SpecialityId}`.
Expect: He is redirected to his own College's specialities, subtitled "College: College of Paediatricians of South
  Africa (CPSA)". The page has one row: `Paediatrics`, "Specialist training in Paediatrics.", Active, offering Edit and
  Sub-specialities. The Demo College's speciality is not listed. The header offers Create speciality. It offers no link
  to the Colleges list, which does not admit him (T291 item 2). His menu's Specialities stays lit on it (T331), and the
  trail above the header reads Home › Specialities › College of Paediatricians of South Africa.

Step 1.13 — The Paediatrics sub-speciality (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities/{SpecialityId:int}/sub-specialities
Do: Open Sub-specialities on the Paediatrics row. Record the sub-speciality's id as `{SubSpecialityId}`.
Expect: The page is subtitled "Speciality: Paediatrics". It has one row: `Paediatrics`, "General paediatric specialist
  training programme.", Active, offering Edit. The header offers Back to specialities and Create sub-speciality. The
  sub-speciality shares its speciality's name, and there is no "General Paediatrics".

Step 1.14 — The sub-speciality defaults to the CPSA ladder (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}
Do: Open the sub-speciality's Edit and read it. Leave by Cancel without saving.
Expect: The page is headed "Edit sub-speciality" and subtitled "Speciality: Paediatrics".
  - Default entrustment scale is set to `CPSA Paediatric Entrustment Scale v11.1`. The other choices are "No default —
    offer every scale" and `O-R Scale`.
  - The field's help reads "Committee STARs for this programme's trainees are limited to this scale's levels."
  - Active is ticked, and Deactivate and Save are offered.
  The seeder set this default when it created the sub-speciality, and never resets it (T187). So Act 4's STAR picker
  offers the six rungs only (T076).

Step 1.15 — The College does not open the scale pages (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/entrustment-scales → /access-denied
Do: Type the address of the entrustment scales list.
Expect: The page reads "You cannot open this page" and "Your role (College admin) does not open this page.", then "If
  you need it for your work, ask the platform administrator.", with Go to Home (T335).
  The scales list admits only an Administrator or an InstitutionalAdmin, and Kruger's menu offers no Entrustment scales.
  He reads the ladder by name where the College uses it: the sub-speciality's default (Step 1.14) and each item's Scale
  column (Step 1.18).

Step 1.16 — The 15 v11.1 EPAs (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas
Do: Open EPAs and read the list. Create nothing.
Expect: The list has exactly 15 rows, PAED-001 to PAED-015, with the titles in the table below. Every row reads:
  - College `College of Paediatricians of South Africa`;
  - Speciality `Paediatrics` and Sub-speciality `Paediatrics`;
  - Active, with Edit.
  The Demo `EPA-001` is not listed, because a CollegeAdmin reads only his own College's EPAs. The header offers Create
  EPA.

Step 1.17 — Curriculum 11.1 (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula
Do: Open Curricula and read the list. Record the curriculum's id as `{CurriculumId}`.
Expect: The list has one row: `Paediatric EPA Curriculum`, version `11.1`, College `College of Paediatricians of South
  Africa`, Speciality and Sub-speciality `Paediatrics`, effective `2026-01-01`, 15 items, Active, offering Edit and
  Items. The header offers Create curriculum. The Demo `IM Core Curriculum` is not listed.

Step 1.18 — The 15 items and their targets (act-1-setup.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula/{Id:int}/items
Do: Open Items on the curriculum's row. Read the Existing items table against the table below. Add, edit and remove
  nothing.
Expect: The page is subtitled "Paediatric EPA Curriculum (11.1)" and offers Back to curricula and Back to curriculum.
  - A warning says that a target and its period are read live, in every institution that has adopted the curriculum,
    including periods that have already closed. It also says that a tool list is checked when an activity is filed and
    submitted, and never takes back credit already earned.
  - The table has 15 rows, which match the table below. Every row reads Scale `CPSA Paediatric Entrustment Scale v11.1`,
    Completion window (months) `12` and Weight `—`, and every row offers Edit and Remove.
  - Target and decision holds the target above the decision, for example "3 per semester" above "Decided each
    semester".
  - The Add item form offers no EPA. It says "Every national EPA of Paediatrics is already on this curriculum, …", and
    explains how an EPA would be freed.

Step 6.1 — devadmin creates a second national College (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/new → /admin/colleges/{Id:int} → /admin/colleges
Do: Create the College of Neurologists of South Africa, with the description "Constituent College of the CMSA; owns
  the national Neurology catalogue." Save it first with the short code `CPSA` by mistake, then correct the code to
  `CNSA` and save again.
Expect: The first save is refused: "A college with the same name or short code already exists." Nothing is created.
  The second save opens the new College's edit page, Active. The colleges list reads three rows, each Active and each
  offering Edit and Specialities: the new College, the College of Paediatricians of South Africa and the Demo College.

Step 6.2 — devadmin corrects CPSA's description, and nothing else (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/{Id:int} → /admin/colleges
Do: Open CPSA's record and note its id from the address, for Step 6.10. Change the description to "Constituent
  College of the Colleges of Medicine of South Africa; owns the national Paediatric EPA catalogue and its versions."
  Leave the name, the short code and Active as they are, and save.
Expect: The save is confirmed, and the list shows the new description, with CPSA still Active. The name and short code
  are unchanged, so every CPSA page still reads "College: College of Paediatricians of South Africa (CPSA)". A restart
  neither re-seeds nor restores the record: the seeder finds its College by seed key (T221).

Step 6.3 — devadmin adds the Neurology speciality (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges → /admin/colleges/{CollegeId:int}/specialities → /admin/colleges/{CollegeId:int}/specialities/new → /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/colleges/{CollegeId:int}/specialities
Do: From the new College's row, open its specialities, and create `Neurology` with the description "Specialist training
  in Neurology."
Expect: Before the create, the list says the College has no specialities yet and invites the first. After it:
  - the edit page reads "College: College of Neurologists of South Africa (CNSA)", Active, and offers "Manage
    sub-specialities";
  - the list has one row, Neurology, Active.

Step 6.4 — devadmin adds the Adult Neurology sub-speciality (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Neurology's page, manage its sub-specialities and create `Adult Neurology`, with the description "Adult
  neurology specialist training programme."
Expect: The list is empty before the create and has one row after it. The new sub-speciality's edit page offers a
  default entrustment scale, which reads "No default — offer every scale". Its choices are the two existing scales.

Step 6.5 — devadmin creates the Neurology ladder (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/entrustment-scales → /admin/entrustment-scales/new → /admin/entrustment-scales/{Id:int} → /admin/entrustment-scales
Do: Create `CNSA Neurology Entrustment Scale`, with the description "Supervision ladder for CNSA EPAs." Give it five
  levels, in this order, each with a one-line description:
  1. `Observe only`
  2. `Direct supervision`
  3. `Indirect supervision`
  4. `Distant supervision`
  5. `Unsupervised practice`
  Save it first with the fourth level also labelled `Indirect supervision`, then correct that label and save again.
Expect: The first save is refused: "Level labels must be unique within a scale." Nothing is created. The second save
  opens the new scale's edit page, with the five levels numbered 1 to 5 in the order given. The scales list shows
  three scales, the new one with 5 levels.

Step 6.6 — devadmin adds a sixth level (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/entrustment-scales → /admin/entrustment-scales/{Id:int} → /admin/entrustment-scales
Do: Open the CNSA scale, add a sixth level, `Supervises others` ("Supervises juniors performing the activity."), and
  save.
Expect: The save is confirmed, and the page lists six levels, 1 to 6, with `Supervises others` at 6. The list shows 6
  levels.

Step 6.7 — devadmin makes it Adult Neurology's default (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}
Do: Open Adult Neurology, set its default entrustment scale to the CNSA Neurology Entrustment Scale, and save.
Expect: The save is confirmed, and reopening the page shows the scale selected. Its help says that committee STARs for
  this programme's trainees are limited to that scale's levels.

Step 6.8 — devadmin cannot delete a scale in use (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/entrustment-scales
Do: Delete the CNSA Neurology Entrustment Scale from the list.
Expect: Delete asks first, naming the scale (DESIGN.md § Button system: a destructive action opens a ConfirmDialog).
  Once confirmed, it is refused: "This entrustment scale is the default scale of the sub-speciality "Adult Neurology"
  (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to
  no default, first." The scale stays, with 6 levels.

Step 6.10 — Dr Kruger is refused what only an Administrator may do (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /account/login → /account/login/submit → / → /admin/colleges/{Id:int} → /access-denied → /admin/entrustment-scales/new → /access-denied
Do: Sign in. From the dashboard, type the address of CPSA's own College record (the id from Step 6.2), then the
  address for creating an entrustment scale.
Expect: The dashboard has one card, National catalogue, with Specialities, EPAs and Curricula. His menu offers neither
  of the pages he types. Each address lands on "You cannot open this page" ("Your role (College admin) does not open
  this page."), with Go to Home, and nothing changes.

Step 6.11 — Dr Kruger edits the Paediatrics speciality (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/specialities → /admin/colleges/{CollegeId:int}/specialities → /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/colleges/{CollegeId:int}/specialities
Do: From Specialities in his menu, open Paediatrics. Change its description from "Specialist training in Paediatrics."
  to "Specialist training in Paediatrics, and certificate training in its sub-specialities." Save.
Expect:
  - Specialities takes him to CPSA's list, which reads "College: College of Paediatricians of South Africa (CPSA)"
    and has one row.
  - The save is confirmed, and the list shows the new description.
  - Neither page offers him a way to the colleges list, which he may not open (T211's rule: a page offers only what
    its target admits).

Step 6.12 — Dr Kruger creates the Neonatology sub-speciality (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/colleges/{CollegeId:int}/specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities → /admin/specialities/{SpecialityId:int}/sub-specialities/new → /admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int} → /admin/specialities/{SpecialityId:int}/sub-specialities
Do: From Paediatrics, manage its sub-specialities and create `Neonatology`, with the description "Certificate training
  in Neonatology." On its edit page, set the default entrustment scale to the CPSA Paediatric Entrustment Scale v11.1,
  and save.
Expect: The list holds Neonatology and Paediatrics, both Active. The default is saved and stays selected when the page
  is reopened. The Paediatrics sub-speciality keeps its own default, the CPSA ladder. Nothing about KGK's programme
  changes.

Step 6.13 — Dr Kruger starts a Neonatology curriculum, and holds it back (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/new → /admin/curricula/{Id:int} → /admin/curricula → /admin/curricula/{Id:int}/items
Do: Create `Neonatology EPA Curriculum`, version `1.0`, in Paediatrics / Neonatology, effective from `D`, with no end.
  The form selects Neonatology first. Untick Active and save, so that no institution can adopt it yet. Then open its
  items.
Expect:
  - The create opens the curriculum's edit page, with Active ticked and a card for cloning it.
  - After Active is unticked, the save is confirmed. The curricula list shows the curriculum Inactive, with 0 items.
  - Its items page has no rows. In place of the Add form, it says there is nothing to add, and that a national EPA of
    Neonatology must be added first.

Step 6.14 — Dr Kruger corrects an EPA's wording (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-006. Correct its title from "Managing long-term health conditions (LTHCs)" to "Managing long-term health
  conditions (LTHCs) in children". Change nothing else, and save.
Expect: The save is confirmed, and the EPA stays Active. The EPA list shows the new title. The title is read live
  wherever the EPA is named: 11.1's items, the activity pickers and every registrar's progress card (Step 6.15). An
  EPA's wording has no versions.

Step 6.17 — Dr Kruger pauses PAED-012 (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: Open PAED-012 and press Deactivate. Read the question and confirm. Then open 11.1's items.
Expect:
  - **The question.** Deactivate asks first, "Deactivate this EPA?". The dialog says that the EPA:
    - can no longer be chosen for a new activity;
    - stops being a target on every progress page and dashboard, for every institution that uses it;
    - keeps the progress it has earned;
    - credits nothing for an activity completed against it while it is inactive, until it is reactivated.
  - **After confirming.** The page says the EPA is deactivated. Active is unticked, and Deactivate is gone. The EPA list
    reads PAED-012 Inactive.
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)". A notice says its credit is paused and that
    reactivating it credits what was completed meanwhile.

Step 6.18 — Dr Patel completes the Mini-CEX during the pause (act-6-catalogue.md)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Dlamini's Mini-CEX from the inbox. Rate it at the rung that Dr Dlamini's PAED-012 card named as the minimum
  now (Step 6.15). Write the three feedback fields, and complete it.
Expect:
  - The inbox row and the request's EPA both read "PAED-012 — Communicating with and counselling patients, caregivers
    and healthcare teams (no longer in use)".
  - The completion is not refused (D48): "Completed.". The activity is read-only.
  - The status card reads "Done. You completed it on …" and "Rated <the rung>. Its credit to PAED-012 waits while the
    EPA is paused." About's Credit reads None, with "This activity's EPA is paused: its credit waits." under it. In
    its history, the completion's Credit reads None.

Step 6.19 — Dr Dlamini during the pause (act-6-catalogue.md)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/mine → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read her progress and her activities. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without
  saving.
Expect:
  - **Progress.** No PAED-012 card, and the semester targets line counts `m − 1` EPAs. The rating trajectory for
    PAED-012 still charts every rating on it, the one from Step 6.18 included, under the heading "PAED-012 — … (no
    longer in use)" (T255).
  - **My activities.** The Mini-CEX carries "PAED-012 (no longer in use)" under its name, and its Credit reads None.
  - **The picker.** It offers eight EPAs, without PAED-012.

Step 6.23 — Dr Kruger restores PAED-012 (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/{Id:int} → /admin/epas
Do: Open PAED-012, tick Active and save.
Expect: Nothing asks first. The page reads "EPA reactivated. 1 activity completed against it while it was inactive now
  counts towards progress." Deactivate is offered again. The EPA list reads PAED-012 Active.

Step 6.24 — Dr Dlamini's credit is counted (act-6-catalogue.md)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/mine
Do: Read her progress and her activities.
Expect:
  - PAED-012's card is back, with `n + 1` this semester, and one more at the minimum level when observed than in Step
    6.15. The semester targets line counts `m` EPAs again.
  - The trajectory heading and My activities no longer carry "(no longer in use)", and the Mini-CEX's Credit reads
    1 item.
  - The credit PAED-012 had earned before the pause was kept, not cancelled. Dr Dlamini holds none from Acts 3-4
    (`n` is 0 in Step 6.15), so this is checked on Dr Molefe's PAED-012 progress from Act 3: its row is unchanged
    through the pause (by SQL, or on her record in Step 6.40).

Step 6.28 — Dr Kruger adds a national EPA (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/epas → /admin/epas/new → /admin/epas/{Id:int} → /admin/epas
Do: Create an EPA:
  - sub-speciality: Paediatrics / Paediatrics (the form selects Paediatrics / Neonatology first, so change it);
  - code: `PAED-016`;
  - title: "Transitioning adolescents with long-term conditions to adult care";
  - description: "Plans and leads the handover of an adolescent with a long-term condition to adult services, with the
    young person and their caregivers.";
  - category: Core.
  Save.
Expect:
  - The sub-speciality list offers CPSA's two, Paediatrics / Neonatology and Paediatrics / Paediatrics.
  - Once saved, PAED-016 is Active.
  - His EPA list holds PAED-001 to PAED-016, and not KGK-001: a College does not see an institution's own EPAs.
  - PAED-016 is on no curriculum yet, so no registrar is offered it or measured against it.

Step 6.29 — Dr Kruger clones 11.1 as 11.2, and holds it back (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int}/items → /admin/curricula/{Id:int} → /admin/curricula
Do: Read 11.1's items. Then, on 11.1's page, clone it: new version `11.2`, effective from `D`, with no end. On the new
  version's page, untick Active and save.
Expect:
  - 11.1's items still show him 15 rows: KGK's own item is not shown to the College (T211).
  - The clone form starts with the version "11.1-copy".
  - Once cloned, the page is 11.2's, with Active ticked: a clone is adoptable the moment it exists.
  - After Active is unticked and saved, the list shows 11.2 Inactive, with 15 items.

Step 6.30 — Dr Kruger edits 11.2's items (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: On 11.2's items:
  1. Add PAED-016:
     - target: 1 per academic year;
     - decided each academic year, by the trainee's general panel;
     - the suggested scale, and minimum level 4;
     - year-by-year minima: 2, 3a, 3b and 4 for years 1 to 4;
     - tools: Mini-CEX, CBD and MSF.
  2. Edit PAED-011: set its target to 2 per academic year, and change nothing else.
Expect:
  - **Before the edits.** The 15 rows match 11.1's, cell for cell.
  - **The add.** The form is headed "Add item", and its EPA picker offers PAED-016 alone. After the add, the
    confirmation heads a note that every national EPA of Paediatrics is already on the curriculum. The table has 16
    rows.
  - **The edit.** PAED-011's edit row opens beneath its row. Once saved, the row reads "2 per academic year". 11.1's
    PAED-011 still reads 1 (outcome SQL).

Step 6.31 — Dr Kruger publishes 11.2 (act-6-catalogue.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: /admin/curricula → /admin/curricula/{Id:int} → /admin/curricula
Do: Open 11.2, tick Active and save.
Expect: The save is confirmed. The curricula list shows 11.1 and 11.2 Active, with 11.2 at 16 items, and the
  Neonatology curriculum still Inactive.

Step A.7.10 — Dr Kruger on his phone (appendix-cross-cutting.md)
Role: CollegeAdmin — Dr Anton Kruger
Route: / → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: At 390 px, open EPAs, then Curricula and the items of Paediatric EPA Curriculum 11.2.
Expect: The EPA list and the items table scroll inside their containers (DESIGN.md § Table system). Each item's row
  actions stay reachable, and the item editor's inputs keep usable widths (T198, T226).
```
