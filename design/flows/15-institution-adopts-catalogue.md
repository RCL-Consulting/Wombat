# F15 An institution adopts the College's curriculum, adds its own, and moves to a new version

Prof Nolwazi Mbatha adopts the College's curriculum so KGK can admit registrars, adds an EPA and an item of KGK's own,
and moves to a newer version. Adoption gates admission, and a version move rewrites a registrar's progress, so the
pages must show whose rules these are and what a change will do.

Part of the GUI redesign briefed in `design/BRIEF.md` (T332). Written 2026-09-26.

| | |
|---|---|
| **People** | InstitutionalAdmin Prof Nolwazi Mbatha (KGK); Administrator `devadmin`, who runs the progress rebuild and can adopt for an institution; Trainee Dr Sipho Ndlovu, whose figures show the effect of a move; Dr Anele Dlamini's profile (6.34) |
| **Frequency and stakes** | At set-up, then once per curriculum version. Medium-high: adoption gates admission, and a version move rewrites progress |
| **Mode** | **Straight to fidelity**: the List and Form page shapes hold (BRIEF § 4). **Wireframe first** for the adoption page only if a variation merges it into Curricula (question 1) |
| **Pages** (`coverage.md` templates) | `/admin/curricula`, `/admin/epas`, `/admin/epas/new`, `/admin/epas/{Id:int}`, `/admin/adoptions`, `/admin/curricula/{Id:int}/items`, `/admin/entrustment-scales`, `/admin/entrustment-decisions`, `/admin/trainees/edit`, `/admin/curriculum-progress`, `/portfolio/progress` |
| **Held screenshots** | None: no group-1 task changes these pages. T304, a P2 running in parallel, changes the profile save and the move (§ Known problems) |

**How to run this thread** (BRIEF § 2.3, step 3; source keys are BRIEF § 2.0's):
1. Open a new thread in the Wombat design system. Attach the KEY SCREENSHOTS (§ Attach, first list).
2. Paste **The ask** below. Then paste **The runbook steps, verbatim** (the last section) as the next message: "Walk
   Claude through the journey and it will generate each screen in context" [academy].
3. Add the other screenshots in § Attach as the chat asks for them.
4. If a variation merges Adoptions into Curricula, have that variation drawn as a wireframe first and pick before
   fidelity ("Wireframe first when fidelity doesn't matter" [parrott]).
5. Write every decision into the chat as a sentence; the chat travels in the handoff bundle [academy]. Export the chosen
   artboards to `design/flows/15-institution-adopts-catalogue/` before moving on: there is no version history [start].

## The ask

Paste this block as the thread's first message.

```text
FLOW 15 — My institution adopts the College's curriculum, adds its own EPA and item, and moves to a new version.

GOAL: Let an institution's administrator adopt a College curriculum version, so that her registrars can be admitted
  against it; read the College's EPAs, items and entrustment ladders, which are the College's to change and read-only
  to her; add an EPA and a curriculum item of the institution's own; adopt a newer version; move a registrar to it; and
  have the registrar's progress counted again. What is wrong today:
  - a registrar left on a superseded version cannot have his profile saved at all, even to change a date ("Trainees
    must be admitted into the curriculum version this institution has adopted.");
  - a registrar moved to the new version sees every count at zero, with no word, until the Administrator rebuilds
    everyone's progress; nothing replays his credit at the move;
  - the EPA list offers Edit on every national EPA, which the save then refuses, and does not mark the institution's
    own EPA;
  - before adoption the EPA list offers Create EPA, whose form has no sub-speciality to choose;
  - the read-only scales page tells her to "define and maintain" ladders she cannot change.

AUDIENCE: InstitutionalAdmin Prof Nolwazi Mbatha (Kgosi Kgari Teaching Hospital, KGK). The Administrator, who runs the
  progress rebuild and can adopt on an institution's behalf. Trainee Dr Sipho Ndlovu, whose progress page shows what a
  move does. At set-up, then about once per curriculum version; medium-high stakes, because adoption gates admission
  and a move rewrites progress. Desktop 1280×800 and phone 390×844.

LAYOUT: Signed-in shell. Curricula, EPAs, Adoptions and Entrustment Scales are List pages (a table in a container with
  row actions). A curriculum's items page is a table of items with an add form below it and an edit row per item. An EPA
  is a Form page. The trainee profile is a Form page with a summary card. The progress rebuild is one explanation, one
  button, a confirmation dialog and a result of five figures. My progress is the registrar's page of per-EPA cards.

CONTENT (use these real scenario values):
  - The College: College of Paediatricians of South Africa (CPSA), Paediatrics / Paediatrics.
  - Paediatric EPA Curriculum 11.1: 15 national items, PAED-001 to PAED-015, each "Set by the College", all on the CPSA
    Paediatric Entrustment Scale v11.1 (six rungs: 1, 2, 3a, 3b, 4, 5), e.g. PAED-002 "3 per semester", "Decided each
    semester", minimum 5.
  - Paediatric EPA Curriculum 11.2: 16 national items, adding PAED-016 (1 per academic year) and PAED-011 at "2 per
    academic year".
  - Also offered for adoption: Demo College — General Medicine / General Internal Medicine: IM Core Curriculum (2026.1).
    Not offered: Neonatology EPA Curriculum 1.0 (held back by the College).
  - KGK's own EPA: KGK-001 "Running a paediatric outreach clinic at a district hospital", Elective. Its item: 1 per
    academic year, no decision cadence, CPSA ladder, minimum 3a, completion window 12 months, tools: Direct observation.
  - Adoptions: 11.1 adopted 2026-09-26, then Superseded when 11.2 is adopted.
  - Scales, read-only to her: CPSA Paediatric Entrustment Scale v11.1 (6 levels), O-R Scale (5), CNSA Neurology
    Entrustment Scale (6).
  - Dr Anele Dlamini: on 11.1, programme start 2024-01-15, expected completion 2028-01-14.
  - Dr Sipho Ndlovu: started 2026-01-15; PAED-002 "1 of 3 this semester" before the move; 16 cards on 11.1, 17 on 11.2.
  - The rebuild's result: Activities re-read 15, Curriculum items credited 14, Semester tallies written 9, Stale
    tallies removed 1, Completions re-stamped 15.

SCREENS, in order:
  1. /admin/curricula and /admin/epas — the catalogue as the institution sees it. States: nothing adopted ("No curricula
     yet": adopt one on the adoptions page before admitting trainees; the EPA list says the same, and offers no Create
     EPA until there is a sub-speciality to put one in); the adopted catalogue (each national row read-only, marked as
     the College's); two versions; the EPA list with KGK's own EPA marked as the institution's own and editable.
  2. /admin/adoptions — Curriculum adoptions. States: nothing adopted; adopted ("Curriculum adopted.", the row with
     College, speciality, sub-speciality, curriculum, version, adopted on, status); adopted twice, refused ("This
     institution has already adopted this curriculum version."); superseded (11.2 Active, 11.1 Superseded, and the note
     that a newer version supersedes the current adoption while registrars stay on their version); the Administrator
     with no institution chosen (only the institution picker); the Administrator with KGK chosen; loading; 390 px.
  3. /admin/curricula/{Id:int}/items — a curriculum's items for the institution. States: the College's items, read-only
     to her, under a notice that the College sets them; the "Add your institution's own item" form (the EPA picker
     offers only KGK-001; Add item stays off until a minimum rung is chosen); her item added, with Edit and Remove; the
     Remove dialog, naming the item; removed.
  4. /admin/epas/new and /admin/epas/{Id:int} — a local EPA. States: create (the sub-speciality list offers only what
     KGK has adopted); deactivated (the dialog, then Inactive); not found (an EPA outside her scope).
  5. The consequences. The read-only scales list (no Create, no actions, copy that fits a reader). The trainee profile
     save refused on a superseded version, and the same save after the fix: the curriculum picker offers only his pinned
     version and the adopted one, and a move reports what it did ("Trainee profile saved. 1 completion was counted
     again against 11.2."). The Administrator's progress rebuild: before, the dialog ("Rebuild curriculum progress?"),
     the result with its five figures, failed ("The rebuild failed and nothing was changed: …"), 390 px. Dr Ndlovu's
     My progress on 11.2 before and after his credit is counted again.

STEPS: 1.19–1.22, 6.9, 6.22, 6.25, 6.26 and 6.32–6.39 (16 steps), pasted verbatim in my next message (Role / Route /
  Do / Expect). Walk them in order; each Expect is what the screen must let the person see or do.

STATES TO SHOW, beyond those above: loading and load error on each list and on the items page; not found on a
  curriculum's items outside her scope. Data volumes: one adoption and six (three versions of two disciplines); an
  items table of 15 national rows and of 16 with two of the institution's own; an EPA list of 15 and of 40.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
  - T304: a profile's curriculum picker offers only its pinned version and the institution's active adoption for that
    discipline; at admission, only the active adoption's version. Saving without changing the curriculum keeps the pin,
    so Dr Dlamini's date change on 11.1 succeeds. A move, or a changed programme start, counts the registrar's credit
    again in the same save and says so. A moved registrar never sees zeros waiting for a rebuild.
  - T301: a curriculum version is "Open" or "Held back" for adoption; the adoption picker offers only open versions,
    and adopting a held-back one is refused ("This curriculum version is not open for adoption.").
  - T291: the EPA list offers Edit only where the save admits the caller; a national row shows "Set by the College"
    in its actions cell instead. The institution's own EPA is marked under its title ("Your institution's own EPA"; an
    Administrator, who sees every institution's, reads the owner's name). Create EPA and "Create the first EPA" are
    offered only when there is an adopted sub-speciality to put one in; otherwise the page says to adopt first. An EPA
    outside her scope, typed by its id, is either not found or read-only with its College named: decide which, and
    design that state. The dashboard's specialities card counts what the institution has adopted.
  - T326: the scales page's subtitle says what it does for a reader (today: "Define and maintain the rating ladders
    used by assessment forms and committee reviews.").
  - T272: a failed save says what failed; not every failure is "already exists", and no raw database text is shown.
  - T242: a national EPA the institution already holds as its own item may block the College from adding it; if the
    College's add is refused, the refusal names the institution (the College's side is flow 16).
  - Every dialog names its target, its trigger is an outline button, and the result takes the focus; a refusal keeps
    what was typed. People by name. At 390 px nothing scrolls sideways; tables scroll inside their containers.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Should adoption live on the Curricula page, as an Adopt action on each version, instead of a separate Adoptions
     page? Today Curricula lists only what the institution has adopted, and Adoptions lists every open version with an
     Adopt button and the history of adoptions.
  2. How is "the College's, read-only" told apart from "ours, editable" on the same items table, and on the EPA list?
     Today each national item reads "Set by the College" and her own reads "Your institution's own item" with Edit and
     Remove; the EPA list marks nothing.
  3. Rebuilding one trainee's progress is not built: the command takes a trainee, but only the Administrator's
     rebuild-everyone page exists. Offer it on the trainee's profile, or is T304's replay at the move enough?

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
  - FULL FIDELITY, 2–3 variations: screens 1 to 5. If a variation answers question 1 by merging Adoptions into
    Curricula, draw that variation as a WIREFRAME first and stop for my pick.
  - Name every design-system component you use and mark anything else NEW. Say which Wombat rule a variation breaks.
  - Show the empty, error and loading states and the data volumes above. Review each screen for WCAG 2.1 AA: contrast,
    focus, targets of at least 24 px, and colour never the only signal (a status and an owner are written in words).
  - Ask me before assuming anything the steps leave open.

ATTACHED: states/adoptions-list--empty.png, states/adoptions-list--adopted.png, act-1/1.21-4-mbatha-home-adopted.png,
  states/curriculum-items-edit--institution.png, states/epa-edit--create-local.png,
  states/curriculum-items-edit--own-added.png, states/adoptions-list--superseded.png,
  states/trainee-profile-edit--curriculum-refused.png, states/curriculum-progress-rebuild--result.png,
  act-1/1.22-1-scales-read-only.png. These show today's pages, defects included; they are not the target.
```

## The journey

One line per step, in play order: who does what, and what the screen must let them see. The full steps are in the last
section.

| Step | Page | Who does what | What they must be able to see |
|---|---|---|---|
| 1.19 | `/admin/curricula` → `/admin/epas` | Prof Mbatha opens Curricula and EPAs before adopting | Nothing adopted, and where to adopt; no Create she cannot complete (T291) |
| 1.20 | `/admin/adoptions` | She adopts CPSA 11.1, then tries again | No institution picker (hers is KGK); Adopt off until a curriculum is chosen; "Curriculum adopted." and the row; the second Adopt refused; the supersede note |
| 1.21 | `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/` | She reads what the adoption opened, then Home | 15 EPAs with no Edit (T291); one curriculum with Items only; the items read-only, each "Set by the College", and the own-item form saying KGK must first create a local EPA; Home's card reading 1 and 1 |
| 1.22 | `/admin/entrustment-scales` | She reads the ladders | Two scales with level counts and descriptions; no Create, no actions column; copy for a reader (T326) |
| 6.9 | `/admin/entrustment-scales` | She reads the scales after the CNSA scale is added | Three scales; still read-only |
| 6.22 | `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/admin/entrustment-decisions` | She sees the College's pause of PAED-012 | PAED-012 Inactive and not hers to lift; its item "(inactive: not in force)"; Dr Molefe's STAR on it still Active, named "(no longer in use)" |
| 6.25 | `/admin/epas` → `/admin/epas/new` → `/admin/epas/{Id:int}` → `/admin/epas` | She creates KGK-001 | Only Paediatrics / Paediatrics offered; KGK-001 Active and hers to change; the list marking it as KGK's own, with Edit on it alone |
| 6.26 | `/admin/curricula` → `/admin/curricula/{Id:int}/items` | She adds KGK's item on KGK-001 to 11.1 | Items only on the list; the own-item form; the new row "Your institution's own item" with Edit and Remove |
| 6.32 | `/admin/adoptions` | She adopts 11.2 | The open versions offered, not the held-back one; 11.2 Active and 11.1 Superseded; registrars stay on 11.1 |
| 6.33 | `/admin/curricula` → `/admin/curricula/{Id:int}/items` | She adds KGK's item to 11.2 | Both versions listed; 11.2's 16 national rows and no KGK row until she adds it |
| 6.34 | `/admin/trainees` → `/admin/trainees/edit` | She changes Dr Dlamini's expected completion, curriculum untouched | Today refused. After T304: saved, Dr Dlamini still on 11.1 |
| 6.35 | `/portfolio/progress` | Dr Ndlovu notes his counts before the move | 16 cards on 11.1, PAED-016 not among them |
| 6.36 | `/admin/trainees` → `/admin/trainees/edit` | She moves Dr Ndlovu to 11.2 | "Trainee profile saved."; after T304, also what was counted again |
| 6.37 | `/portfolio/progress` → `/activities/new` | Dr Ndlovu reads his page on 11.2 | 17 cards. Today every count 0 with no word; after T304 his counts at once. The Mini-CEX picker offers PAED-016 |
| 6.38 | `/` → `/admin/curriculum-progress` | `devadmin` rebuilds everyone's progress | What a rebuild does; "Rebuild curriculum progress?"; the result's five figures |
| 6.39 | `/portfolio/progress` | Dr Ndlovu reads his page after the rebuild | His counts carried over; PAED-011 against 2 per academic year |

## States to design

31 captures, from `execution/knowledge/scenario-paediatrics/states.md`. The line number is the row in `states.md`. None
is held.

| Page slug | State | Screenshot (`design/baseline/…`) | Who | How it is reached (`states.md` line) |
|---|---|---|---|---|
| `curricula-list` | None, an institution | `states/curricula-list--empty.png` | Prof Mbatha | At Step 1.19. (:539) |
| `curricula-list` | Adopted, items only | `states/curricula-list--institution.png` | Prof Mbatha | At Step 1.21. (:541) |
| `curricula-list` | Two versions, an institution | `states/curricula-list--institution-versions.png` | Prof Mbatha | At Step 6.33. (:543) |
| `epas-list` | None, an institution | `states/epas-list--empty.png` | Prof Mbatha | At Step 1.19. (:519) |
| `epas-list` | The adopted fifteen | `states/epas-list--institution.png` | Prof Mbatha | At Step 1.21. (:521) |
| `epas-list` | With a local EPA | `states/epas-list--local.png` | Prof Mbatha | At Step 6.25. (:523) |
| `epa-edit` | Create, an institution | `states/epa-edit--create-local.png` | Prof Mbatha | At Step 6.25, before saving. (:532) |
| `epa-edit` | A local EPA deactivated | `states/epa-edit--local-deactivated.png` | Prof Mbatha | Scratch (post-appendix): Deactivate on KGK-001, confirm; then tick Active and save (`--local-reactivated`, which `states.md` names but was never captured: describe it in words; BRIEF § 10). (:536) |
| `epa-edit` | Out of scope | `states/epa-edit--not-found.png` | Prof Mbatha | Typed with the Demo `EPA-001`'s id: Page not found. No change. (:537) |
| `adoptions-list` | Nothing adopted | `states/adoptions-list--empty.png` | Prof Mbatha | At Step 1.20, before Adopt. (:596) |
| `adoptions-list` | Adopted | `states/adoptions-list--adopted.png` | Prof Mbatha | At Step 1.20: "Curriculum adopted." (:597) |
| `adoptions-list` | Adopted twice, refused | `states/adoptions-list--refused.png` | Prof Mbatha | At Step 1.20, the second Adopt. (:598) |
| `adoptions-list` | Superseded | `states/adoptions-list--superseded.png` | Prof Mbatha | At Step 6.32. (:599) |
| `adoptions-list` | An administrator, no institution | `states/adoptions-list--choose-institution.png` | devadmin | Any time after Act 1: only the Institution picker. No change. (:600) |
| `adoptions-list` | An administrator, KGK chosen | `states/adoptions-list--administrator.png` | devadmin | The same, choose Kgosi Kgari Teaching Hospital; adopt nothing. No change. (:601) |
| `adoptions-list` | Loading | `states/adoptions-list--loading.png` | Prof Mbatha | Hold a read, then Curriculum Adoptions in the nav. No change. (:602) |
| `adoptions-list` | Narrow | `states/adoptions-list--narrow.png` | Prof Mbatha | At Step 6.32, at 390 px, before Adopt. No change. (:603) |
| `curriculum-items-edit` | Read-only to an institution | `states/curriculum-items-edit--institution.png` | Prof Mbatha | At Step 1.21. (:554) |
| `curriculum-items-edit` | The own-item form | `states/curriculum-items-edit--own-form.png` | Prof Mbatha | At Step 6.26, before Add item. (:557) |
| `curriculum-items-edit` | An own item added | `states/curriculum-items-edit--own-added.png` | Prof Mbatha | At Step 6.26. (:558) |
| `curriculum-items-edit` | Remove dialog | `states/curriculum-items-edit--remove-dialog.png` | Prof Mbatha | After Step 6.33, on 11.2: Remove on KGK's own item, capture, Cancel. No change. (:560) |
| `curriculum-items-edit` | Removed | `states/curriculum-items-edit--removed.png` | Prof Mbatha | Scratch (post-appendix): the same, confirmed. (:561) |
| `entrustment-scales-list` | Read-only | `states/entrustment-scales-list--read-only.png` | Prof Mbatha | At Step 1.22. (:566) |
| `trainee-profile-edit` | Curriculum not adopted | `states/trainee-profile-edit--curriculum-refused.png` | Prof Mbatha | At Step 6.34. (:682) |
| `curriculum-progress-rebuild` | Before a rebuild | `states/curriculum-progress-rebuild--default.png` | devadmin | At Step 6.38, before pressing. (:711) |
| `curriculum-progress-rebuild` | Confirm dialog | `states/curriculum-progress-rebuild--dialog.png` | devadmin | At Step 6.38. (:712) |
| `curriculum-progress-rebuild` | Rebuilt | `states/curriculum-progress-rebuild--result.png` | devadmin | At Step 6.38: the five figures. (:713) |
| `curriculum-progress-rebuild` | Failed | `states/curriculum-progress-rebuild--failed.png` | devadmin | Scratch (post-appendix): hold a read on the scratch database, confirm a rebuild, wait 35 s, then `ROLLBACK;`. The refusal's audit row waits for the lock too, so the page shows "The rebuild failed and nothing was changed: …" once the hold is released. (:714) |
| `curriculum-progress-rebuild` | Narrow | `states/curriculum-progress-rebuild--narrow.png` | devadmin | At Step 6.38, at 390 px, before pressing. No change. (:715) |
| `my-progress` | New version before the rebuild | `states/my-progress--before-rebuild.png` | Dr Ndlovu | At Step 6.37. (:297) |
| `my-progress` | New version after the rebuild | `states/my-progress--after-rebuild.png` | Dr Ndlovu | At Step 6.39. (:298) |

Three captures show a defect in place of the state they are named for. Design the state, not the capture:
- **`states/epa-edit--not-found.png`** shows the Demo College's EPA-001 as a full editable form with Save and
  Deactivate, not "Page not found". GetEpaById admits any InstitutionalAdmin to any national EPA (`GetEpas.cs:178-182`,
  per T291). T291's sweep note asks to decide between not found and a read-only view with its College named.
- **`states/trainee-profile-edit--curriculum-refused.png`** is the refusal T304 removes: after T304 an unchanged
  curriculum keeps its pin and the save succeeds.
- **`states/my-progress--before-rebuild.png`** shows the zeros T304 removes: after T304 a moved registrar's page reads
  like `states/my-progress--after-rebuild.png` at once.

Two states have no capture. Design them from the requirements:
- **A held-back version refused at adoption** (T301).
- **A move that reports the credit counted again** (T304).

## Attach

Every path is under `design/baseline/` and was checked to exist on 2026-09-26. Open each before uploading (BRIEF
§ 3.3). None of these shows a registration link or a password.

**First, with the ask (the KEY SCREENSHOTS):**
- `states/adoptions-list--empty.png`
- `states/adoptions-list--adopted.png`
- `act-1/1.21-4-mbatha-home-adopted.png` (Home after the adoption; its specialities card reads 2 and 2, T291 item 4)
- `states/curriculum-items-edit--institution.png`
- `states/epa-edit--create-local.png`
- `states/curriculum-items-edit--own-added.png`
- `states/adoptions-list--superseded.png`
- `states/trainee-profile-edit--curriculum-refused.png`
- `states/curriculum-progress-rebuild--result.png`
- `act-1/1.22-1-scales-read-only.png`

**Then, as the chat asks, by screen:**
- Curricula and EPAs: `states/curricula-list--empty.png`, `states/curricula-list--institution.png`,
  `states/curricula-list--institution-versions.png`, `states/epas-list--empty.png`, `states/epas-list--institution.png`,
  `states/epas-list--local.png`, `act-1/1.19-1-curricula-empty.png`, `act-1/1.19-2-epas-empty.png`,
  `act-1/1.19-3-epa-create-no-subspeciality.png`, `act-1/1.21-1-institution-epas.png`,
  `act-1/1.21-2-institution-curricula.png`, `act-6/6.22-1-mbatha-epa-list.png`, `act-6/6.25-2-epa-list-with-kgk001.png`,
  `act-6/6.26-1-curricula-list-items-only.png`, `act-6/6.33-1-mbatha-curricula-list.png`.
- Adoptions: `states/adoptions-list--refused.png`, `states/adoptions-list--choose-institution.png`,
  `states/adoptions-list--administrator.png`, `states/adoptions-list--loading.png`, `states/adoptions-list--narrow.png`,
  `act-1/1.20-1-adopted.png`, `act-1/1.20-2-adopt-refused.png`, `act-6/6.32-1-adopted-11.2.png`.
- Items: `states/curriculum-items-edit--own-form.png`, `states/curriculum-items-edit--remove-dialog.png`,
  `states/curriculum-items-edit--removed.png`, `act-1/1.21-3-institution-items.png`,
  `act-6/6.22-2-mbatha-items-paused.png`, `act-6/6.26-3-own-item-added.png`, `act-6/6.33-2-own-item-on-11.2.png`, and
  `states/curriculum-items-edit--not-found.png` (Prof Mbatha's not-found on the Demo curriculum's items; it is also
  among flow 16's states).
- A local EPA: `states/epa-edit--local-deactivated.png`, `act-6/6.25-1-kgk001-saved.png`, and, as the defect only,
  `states/epa-edit--not-found.png`.
- The consequences: `states/entrustment-scales-list--read-only.png`, `act-6/6.9-1-mbatha-reads-scales.png`,
  `act-6/6.34-1-profile-save-refused.png`, `act-6/6.36-1-ndlovu-moved-to-11.2.png`,
  `states/curriculum-progress-rebuild--default.png`, `states/curriculum-progress-rebuild--dialog.png`,
  `states/curriculum-progress-rebuild--failed.png`, `states/curriculum-progress-rebuild--narrow.png`,
  `act-6/6.38-2-curriculum-progress-page.png`, `act-6/6.38-3-rebuild-dialog.png`, `act-6/6.38-4-rebuilt.png`,
  `states/my-progress--before-rebuild.png`, `states/my-progress--after-rebuild.png`,
  `act-6/6.35-1-ndlovu-progress.png`, `act-6/6.37-1-ndlovu-progress.png`,
  `act-6/6.37-2-ndlovu-picker.png`, `act-6/6.39-1-ndlovu-progress.png`, `act-6/6.22-3-molefe-stars-paused.png`.

## Known problems this design must solve

| Task | Priority | What it means for the design | Evidence |
|---|---|---|---|
| T304 | P2 | Pickers offer exactly what the command accepts (DESIGN.md:427, per T304): the pinned version plus the active adoption, and at admission the active adoption only. An unchanged curriculum keeps its pin. A move or a changed start replays the trainee's credit in the same save, and the result says so. Its Expects at 6.34, 6.37 and 6.38 change | `act-6/6.34-1-profile-save-refused.png`, `states/my-progress--before-rebuild.png` |
| T301 | P3 | The version's flag is its release for adoption: "Open for adoption" (the box is on the College's page, flow 16), "Open" / "Held back" in the list column. Adoption refuses a held-back version before anything is written. A new or cloned version starts held back | `states/adoptions-list--adopted.png` |
| T291 | P3 | Items 1 and 4 and the C17 note: no Edit on national EPAs for her, a `.muted` "Set by the College" in the actions cell, an owner marker on her own EPA, and a specialities card that counts what she adopted. The 1.19 widening: Create EPA only when a sub-speciality can be chosen. The sweep note: decide how a typed out-of-scope EPA reads | `act-1/1.21-1-institution-epas.png`, `act-6/6.25-2-epa-list-with-kgk001.png`, `act-1/1.19-3-epa-create-no-subspeciality.png`, `act-1/1.21-4-mbatha-home-adopted.png`, `states/epa-edit--not-found.png` |
| T326 | P3 | The scales page's subtitle fits a reader, not an editor (F-1.22a) | `act-1/1.22-1-scales-read-only.png` |
| T272 | P3 | A failed save says what failed; only a real unique violation reads "already exists"; no raw EF text | none captured in this flow |
| T242 | P3 | Recommended (c): the College's national add stays refused while an institution holds that EPA as its own item, and the refusal names the institution. Nothing on her pages changes unless the operator picks (a) or (b) | none (a decision) |
| T329 | P2 | BRIEF § 6 A6: the header shows from the first render, a skeleton while loading, the alert with no empty state under it | `states/adoptions-list--loading.png` |

## Questions the design must answer

1. **Should adoption live on the Curricula page, as an Adopt action per version, instead of a separate Adoptions page?**
   Today an institution's Curricula lists only what it has adopted, with Items only (1.21, 6.26), and says to adopt "on
   the Curriculum Adoptions page" (1.19). Adoptions holds the picker of open versions and the adoption history, with
   Active and Superseded (1.20, 6.32). An Administrator adopts there for any institution through an institution picker
   (`states/adoptions-list--choose-institution.png`).
2. **How is "the College's, read-only" told apart from "ours, editable" on the same items table?** Today: "Set by the
   College" on each national row, "Your institution's own item" with Edit and Remove on hers (6.26), and a notice above
   the table (1.21). The EPA list marks nothing (6.25 Gap, F-6.25b). DESIGN.md's T211 paragraph asks for a `.muted
   .text-sm` owner line under the title (per T291's C17 note).
3. **Rebuilding one trainee's progress is not built.** `RebuildCurriculumProgressCommand` takes a trainee, but no page
   offers it; 6.38 rebuilds everyone (`coverage.md` § Flows and states not played; BRIEF § 7 B10). Offer it on the
   trainee's profile? Note that T304 replays a moved trainee's credit in the same save, which covers 6.37's case.

## Acceptance

After Claude Code builds the chosen design (BRIEF § 9), the flow is done when:
- **The steps replay on a fresh database.** Play Act 1 through **1.19–1.22**, then Acts 2–5, then Act 6 through **6.9**,
  **6.22**, **6.25**, **6.26** and **6.32–6.39** (`README.md` § How to play; `tools/scenario-replay.ps1
  create|publish|start wombat_scenario_f15`). Every Expect holds. Where the design changes wording, the steps' Expect
  lines change in the same task (BRIEF § 9, item 7). Expect to rewrite 1.19 and 1.21 (no Create or Edit she cannot
  use), 1.22 (the subtitle), 6.25 (the owner marker), 6.34, 6.36–6.38 (T304) and 6.32 (T301's refusal of a held-back
  version).
- **The tests pass:** `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`, including
  the curriculum pages' tests in `tests/Wombat.Web.Tests/Admin/` (`CurriculumItemsScopeTests`,
  `CurriculumItemsEpaPickerTests`, `CurriculumItemsRemoveTests`, `CurriculaListScopeTests`), the `Design` tests, and
  T294's guard in `tests/Wombat.Web.Tests/Scenario/`.
- **The baseline is re-captured:** the 31 states above and the step captures of the 16 steps, compared with the chosen
  artboards.
- **The browser check passes** at 1280 and 390 px for Prof Mbatha, `devadmin` and Dr Ndlovu.

## The runbook steps, verbatim

Paste this as the second message. It is each step's Role, Route, Do and Expect, copied from
`execution/knowledge/scenario-paediatrics/` (`act-1-setup.md`, `act-6-catalogue.md`). `D` is the replay day, and `J`
the latest 15 January on or before it.

```text
Step 1.19 — Before the adoption, KGK reads no catalogue (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/epas
Do: Open Curricula, then EPAs, before adopting anything.
Expect: Curricula shows the empty state "No curricula yet": "Your institution has not adopted a curriculum yet. Adopt
  one on the Curriculum adoptions page before admitting trainees." It offers no Create curriculum (T211).
  EPAs shows "No EPAs yet". An InstitutionalAdmin reads the national EPAs of the disciplines her institution has
  adopted, plus its own EPAs, and there are none of either.

Step 1.20 — Adopt curriculum 11.1 for KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/adoptions
Do: Open Curriculum adoptions. Under Adopt a curriculum, choose `College of Paediatricians of South Africa —
  Paediatrics / Paediatrics: Paediatric EPA Curriculum (11.1)` and press Adopt. Then choose the same curriculum again
  and press Adopt a second time.
Expect: The page has no institution picker, because hers is KGK. Current adoptions first reads "No curriculum adoptions
  yet for this institution."
  - The picker also offers the Demo `IM Core Curriculum (2026.1)`, which she leaves alone.
  - Adopt stays disabled until a curriculum is chosen.
  - The first Adopt reads "Curriculum adopted." It adds a row: `College of Paediatricians of South Africa`,
    Paediatrics, Paediatrics, `Paediatric EPA Curriculum`, `11.1`, adopted on `D`, Active.
  - The second Adopt is refused: "This institution has already adopted this curriculum version."
  The page notes that re-adopting a newer version would supersede this adoption.

Step 1.21 — What the adoption opens to KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /
Do: Read each page again, then go Home. Change nothing.
Expect: Each page now shows the adopted catalogue:
  - EPAs lists the 15 PAED EPAs and nothing else. No row offers Edit, because a national EPA is the College's to
    change: `UpdateEpa` refuses her (T291 item 1).
  - Curricula has the one row, which offers Items but no Edit. The header offers no Create curriculum.
  - The items page warns that the College sets the curriculum's national items, so they are read only here, and that
    KGK's own items apply only to KGK's trainees. Its 15 rows match Step 1.18, and each reads "Set by the College".
  - The form below the items is headed "Add your institution's own item". It says every EPA KGK could add is already on
    the curriculum: to add an item of its own, KGK must first create a local EPA of Paediatrics.
  - Home's Specialities & sub-specialities card reads 1 and 1 (T291 item 4).

Step 1.22 — The ladder, read-only to KGK (act-1-setup.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Open Entrustment scales and read the list.
Expect: The list shows `CPSA Paediatric Entrustment Scale v11.1` at 6 levels and `O-R Scale` at 5, each with its
  description. It offers no Create scale and has no actions column, because only an Administrator changes a scale
  (T057, T239).

Step 6.9 — Prof Mbatha reads the scales, and changes none (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/entrustment-scales
Do: Read the list.
Expect: Three scales, with their level counts: the CPSA ladder at 6, the O-R Scale at 5 and the CNSA scale at 6. There
  is no Create scale, and no column of Edit and Delete: a column no row offers is not rendered (T239).

Step 6.22 — Prof Mbatha sees the pause, and cannot lift it (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/entrustment-decisions
Do: Read the EPA list, then 11.1's items. Then open Entrustment decisions and filter by Dr Molefe's name. Revoke
  nothing.
Expect:
  - **The EPA list.** PAED-012 reads Inactive. No national EPA offers her Edit: only its College changes one (T211's
    rule; `UpdateEpa`).
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)", under the same notice. Every row reads "Set
    by the College".
  - **Dr Molefe's STARs.** Her fifteen from Act 5 still read Active, beside Act 4's PAED-010, Superseded (the status
    filter starts on All). The PAED-012 row names its EPA "PAED-012 — Communicating
    with and counselling patients, caregivers and healthcare teams (no longer in use)" (T255): a pause leaves a STAR
    standing.

Step 6.25 — Prof Mbatha creates a KGK EPA (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/epas/new → /admin/epas/{Id:int} → /admin/epas
Do: Create an EPA:
  - sub-speciality: Paediatrics / Paediatrics;
  - code: `KGK-001`;
  - title: "Running a paediatric outreach clinic at a district hospital";
  - description: "Plans and runs a monthly outreach clinic at a North West district hospital, with its referral
    follow-up.";
  - category: Elective.
  Save.
Expect:
  - The sub-speciality list offers only Paediatrics / Paediatrics, the discipline KGK has adopted.
  - Once saved, the page is KGK-001's, Active. KGK's own EPA is hers to change (`UpdateEpa`).
  - The EPA list shows KGK-001 beside the 15 national EPAs, and offers Edit on KGK-001 alone.

Step 6.26 — Prof Mbatha adds KGK's own item to 11.1 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: Open 11.1's items, and add KGK's own item:
  - EPA: KGK-001;
  - target: 1 per academic year;
  - no decision cadence, decided by the trainee's general panel;
  - the suggested scale, minimum level 3a, and no year-by-year minima;
  - completion window: 12;
  - tools: Direct observation only.
Expect:
  - **The curricula list.** It offers her Items, and no Edit and no Create curriculum.
  - **The Add form.** It is headed "Add your institution's own item". Its EPA picker offers KGK-001 alone, because the
    College's items hold all 15 national EPAs (T222). The scale is the CPSA ladder, suggested because every item on
    the curriculum is pinned to it. Add item stays off until a minimum rung is chosen.
  - **After the add.** The page confirms it, and the form gives way to a note that every EPA KGK could add is already
    on the curriculum.
  - **The new row.** It reads "Your institution's own item": 1 per academic year, No decision cadence, 3a, Direct
    observation, with Edit and Remove. The national rows still read "Set by the College".

Step 6.32 — Prof Mbatha adopts 11.2 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/adoptions
Do: Adopt "College of Paediatricians of South Africa — Paediatrics / Paediatrics: Paediatric EPA Curriculum (11.2)".
Expect:
  - The picker offers the two active Paediatric versions and the Demo IM Core curriculum. It does not offer the
    inactive Neonatology curriculum.
  - The adoption is confirmed. Current adoptions reads 11.2 Active and 11.1 Superseded, as the page's note says a newer
    version supersedes the current adoption.
  - The registrars already on 11.1 stay on it.

Step 6.33 — Prof Mbatha adds KGK's own item to 11.2 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/curricula → /admin/curricula/{Id:int}/items
Do: Read the curricula list. Open 11.2's items, and add KGK's own item on KGK-001 with the values of Step 6.26.
Expect:
  - **The list.** It shows 11.2, and still 11.1, because 11.1 holds KGK's own item (T211, T223). Both offer Items only.
  - **11.2's items.** It holds the College's 16 rows, PAED-016 and PAED-011's new target among them, all "Set by the
    College". It has no KGK item, because a new version carries no institution's own items.
  - **The add.** The picker offers KGK-001 alone. Once added, the row reads "Your institution's own item".

Step 6.34 — Prof Mbatha cannot save Dr Dlamini's profile while she stays on 11.1 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Dlamini's profile, which is on Paediatric EPA Curriculum (11.1). Move the expected completion date six
  months later than it reads, leave the curriculum as it is, and save the profile. Then reopen the profile.
Expect:
  - The curriculum picker offers 11.1 and 11.2, with 11.1 chosen.
  - The save is refused: "Trainees must be admitted into the curriculum version this institution has adopted."
  - Nothing changes: the reopened profile shows the old date, and Dr Dlamini stays on 11.1, as re-adopting leaves
    registrars on the version they were admitted to (`CurriculumAdminScope`, T211).

Step 6.35 — Dr Ndlovu notes his figures before he moves (act-6-catalogue.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page, and note each card's count for its current period.
Expect: Sixteen cards: the 15 national EPAs of 11.1, and KGK-001. PAED-016 is not among them.

Step 6.36 — Prof Mbatha moves Dr Ndlovu to 11.2 (act-6-catalogue.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Ndlovu's profile. Choose Paediatric EPA Curriculum (11.2), keep his programme start (`J`, from Act 2) and
  his expected completion as they read, and save the profile.
Expect: The save is confirmed, and the profile reads 11.2. From now on, his completions credit 11.2's items.

Step 6.37 — Dr Ndlovu on 11.2, before progress is rebuilt (act-6-catalogue.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read the page. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect:
  - **The cards.** Seventeen: 11.2's 16 national items (PAED-016 once a year, and PAED-011 at 2 per academic year,
    among them) and KGK-001.
  - **The counts.** His encounters before the move are counted in 11.1's tallies, which this page no longer reads. Until
    Step 6.38, each card shows only what 11.2 holds for him, which is nothing yet.
  - **The picker.** The Mini-CEX picker offers ten EPAs, PAED-016 among them.

Step 6.38 — devadmin rebuilds curriculum progress (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/curricula → /admin/curriculum-progress
Do: From Curricula in the menu, open Curriculum progress from the header and read it. Press Rebuild progress, and
  confirm.
Expect:
  - **The way in.** Curricula offers the Administrator an outline "Curriculum progress" in its header, where Home's
    Maintenance card used to link it (T335, flow 01). On the page Curricula stays lit, and the trail reads Home ›
    Curricula › Curriculum progress.
  - **The page.** It says that a rebuild credits every completed activity again against today's curriculum, judges only
    whether an EPA was active as of each completion, and runs as one transaction.
  - **The question.** The button asks first: "Rebuild curriculum progress?".
  - **The result.** The page says progress was rebuilt, with five figures: activities re-read, curriculum items
    credited, semester tallies written, stale tallies removed, and completions re-stamped. Stale tallies removed is at
    least `t` from Step 6.35.

Step 6.39 — Dr Ndlovu's evidence counts on 11.2 (act-6-catalogue.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page.
Expect: Each of the 15 EPAs carried over from 11.1 reads the count he noted in Step 6.35 for its current period.
  PAED-016 and KGK-001 read 0. PAED-011's card measures its count against a target of 2 per academic year.
```
